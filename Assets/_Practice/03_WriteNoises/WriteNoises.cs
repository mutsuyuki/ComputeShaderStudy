using System;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class WriteNoises : MonoBehaviour {
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

    private KernelInfo fbmNoiseInfo;
    private KernelInfo domainWarpInfo;

    void Start() {
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
        fbmNoiseShader.Dispatch(
            fbmNoiseInfo.kernelIndex,
            fbmNoiseInfo.tempTexture.width / (int) fbmNoiseInfo.threadSizeX,
            fbmNoiseInfo.tempTexture.height / (int) fbmNoiseInfo.threadSizeY,
            1
        );
        Graphics.CopyTexture(fbmNoiseInfo.tempTexture, fbmNoiseTexture);

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