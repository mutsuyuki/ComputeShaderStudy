using UnityEngine;

public class WriteNoises : MonoBehaviour {
    [SerializeField] private ComputeShader blockNoiseShader;
    [SerializeField] private RenderTexture blockNoiseTexture; // 出力先のテクスチャ

    [SerializeField] private ComputeShader perlinNoiseShader;
    [SerializeField] private RenderTexture perlinNoiseTexture; // 出力先のテクスチャ

    [SerializeField] private ComputeShader fbmNoiseShader;
    [SerializeField] private RenderTexture fbmNoiseTexture; // 出力先のテクスチャ

    [SerializeField] private ComputeShader domainWarpShader;
    [SerializeField] private RenderTexture domainWarpTexture; // 出力先のテクスチャ

    struct KernelInfo {
        public int kernelIndex;
        public uint threadSizeX;
        public uint threadSizeY;
        public uint threadSizeZ;
        public RenderTexture tempTexture; // アセットのテクスチャは直接いじれないので、ここに一回書き込む
    }

    private KernelInfo blockNoiseInfo;
    private KernelInfo perlinNoiseInfo;
    private KernelInfo fbmNoiseInfo;
    private KernelInfo domainWarpInfo;

    void Start() {
        blockNoiseInfo = initShader(blockNoiseShader, blockNoiseTexture, "WriteBlockNoise");
        perlinNoiseInfo = initShader(perlinNoiseShader, perlinNoiseTexture, "WritePerlinNoise");
        fbmNoiseInfo = initShader(fbmNoiseShader, fbmNoiseTexture, "WriteFBMNoise");
        domainWarpInfo = initShader(domainWarpShader, domainWarpTexture, "WriteDomainWarp");
    }

    private static KernelInfo initShader(ComputeShader shader, RenderTexture targetTexture, string kernelName) {
        KernelInfo kernelInfo = new KernelInfo();

        if (targetTexture.format != RenderTextureFormat.ARGB32) {
            Debug.LogError(targetTexture.name + "は書き込み可能なテクスチャではないようです。");
        }

        // ComputeShaderから書き込む用のテクスチャを最終出力テクスチャの形に合わせて生成
        kernelInfo.tempTexture = new RenderTexture(targetTexture.width, targetTexture.height, 1, targetTexture.format);
        kernelInfo.tempTexture.enableRandomWrite = true;

        // ComputeShaderにテクスチャをセット
        kernelInfo.kernelIndex = shader.FindKernel(kernelName);
        shader.SetTexture(kernelInfo.kernelIndex, "textureBuffer", kernelInfo.tempTexture);

        // テクスチャサイズのチェック
        shader.GetKernelThreadGroupSizes(
            kernelInfo.kernelIndex,
            out kernelInfo.threadSizeX,
            out kernelInfo.threadSizeY,
            out kernelInfo.threadSizeZ
        );
        float threadGroupSizeX = (float) kernelInfo.tempTexture.width / kernelInfo.threadSizeX;
        float threadGroupSizeY = (float) kernelInfo.tempTexture.height / kernelInfo.threadSizeY;
        if (threadGroupSizeX % 1 != 0 || threadGroupSizeY % 1 != 0) {
            Debug.LogError(targetTexture.name + "はスレッドグループ数が整数にならないので、テクスチャサイズを変えてください。");
        }

        return kernelInfo;
    }

    void Update() {
        // Block Noise -------------------
        blockNoiseShader.Dispatch(
            blockNoiseInfo.kernelIndex,
            blockNoiseInfo.tempTexture.width / (int) blockNoiseInfo.threadSizeX,
            blockNoiseInfo.tempTexture.height / (int) blockNoiseInfo.threadSizeY,
            1
        );
        Graphics.CopyTexture(blockNoiseInfo.tempTexture, blockNoiseTexture);
        
        // Perlin Noise -------------------
        perlinNoiseShader.Dispatch(
            perlinNoiseInfo.kernelIndex,
            perlinNoiseInfo.tempTexture.width / (int) perlinNoiseInfo.threadSizeX,
            perlinNoiseInfo.tempTexture.height / (int) perlinNoiseInfo.threadSizeY,
            1
        );
        Graphics.CopyTexture(perlinNoiseInfo.tempTexture, perlinNoiseTexture);

        // Fractal Brown Movement Noise -------------------
        fbmNoiseShader.Dispatch(
            fbmNoiseInfo.kernelIndex,
            fbmNoiseInfo.tempTexture.width / (int) fbmNoiseInfo.threadSizeX,
            fbmNoiseInfo.tempTexture.height / (int) fbmNoiseInfo.threadSizeY,
            1
        );
        Graphics.CopyTexture(fbmNoiseInfo.tempTexture, fbmNoiseTexture);

        // Domain Warp Noise -------------------
        domainWarpShader.SetFloat("time", Time.time);
        domainWarpShader.Dispatch(
            domainWarpInfo.kernelIndex,
            domainWarpInfo.tempTexture.width / (int) domainWarpInfo.threadSizeX,
            domainWarpInfo.tempTexture.height / (int) domainWarpInfo.threadSizeY,
            1
        );
        Graphics.CopyTexture(domainWarpInfo.tempTexture, domainWarpTexture);
    }
}