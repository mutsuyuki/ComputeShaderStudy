using UnityEditor;
using UnityEngine;

public class WriteToTexture : MonoBehaviour {
    [SerializeField] private ComputeShader computeShader;
    [SerializeField] private RenderTexture targetTexture; // 出力先のテクスチャ

    private RenderTexture tempTexture; // アセットのテクスチャは直接いじれないので、ここに一回書き込む

    private int kernelIndex;
    private uint threadSizeX;
    private uint threadSizeY;
    private uint threadSizeZ;

    void Start() {
        if (targetTexture.format != RenderTextureFormat.ARGB32) {
            Debug.LogError("書き込み可能なテクスチャではないようです。");
        }

        // ComputeShaderから書き込む用のテクスチャを最終出力テクスチャの形に合わせて生成
        tempTexture = new RenderTexture(targetTexture.width, targetTexture.height, 0, targetTexture.format);
        tempTexture.enableRandomWrite = true;

        // ComputeShaderにテクスチャをセット
        kernelIndex = computeShader.FindKernel("SetColorByPosition");
        computeShader.SetTexture(kernelIndex, "textureBuffer", tempTexture);

        // テクスチャサイズのチェック
        computeShader.GetKernelThreadGroupSizes(kernelIndex, out threadSizeX, out threadSizeY, out threadSizeZ);
        float threadGroupSizeX = (float) targetTexture.width / threadSizeX;
        float threadGroupSizeY = (float) targetTexture.height / threadSizeY;
        if (threadGroupSizeX % 1 != 0 || threadGroupSizeY % 1 != 0) {
            Debug.LogError("スレッドグループ数が整数にならないので、テクスチャサイズを変えてください。");
        }
    }

    void Update() {
        computeShader.Dispatch(
            kernelIndex,
            targetTexture.width / (int) threadSizeX,
            targetTexture.height / (int) threadSizeY,
            (int) threadSizeZ
        );

        Graphics.CopyTexture(tempTexture, targetTexture);
    }
}