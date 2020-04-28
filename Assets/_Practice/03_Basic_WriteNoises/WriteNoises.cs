using System;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

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
        tempTexture = new RenderTexture(targetTexture.width, targetTexture.height, 1, targetTexture.format);
        tempTexture.enableRandomWrite = true;

        // ComputeShaderにテクスチャをセット
        kernelIndex = computeShader.FindKernel("SetColorByPosition");
        computeShader.SetTexture(kernelIndex, "textureBuffer", tempTexture);

        // テクスチャサイズのチェック
        computeShader.GetKernelThreadGroupSizes(kernelIndex, out threadSizeX, out threadSizeY, out threadSizeZ);
        float threadGroupSizeX = (float) tempTexture.width / threadSizeX;
        float threadGroupSizeY = (float) tempTexture.height / threadSizeY;
        if (threadGroupSizeX % 1 != 0 || threadGroupSizeY % 1 != 0) {
            Debug.LogError("スレッドグループ数が整数にならないので、テクスチャサイズを変えてください。");
        }
    }

    void Update() {
        if (Random.Range(0.0f, 1.0f) < 0.1)
            return;
        
        computeShader.Dispatch(
            kernelIndex,
            tempTexture.width / (int) threadSizeX,
            tempTexture.height / (int) threadSizeY,
            1 
        );

        Graphics.CopyTexture(tempTexture, targetTexture);
    }
}