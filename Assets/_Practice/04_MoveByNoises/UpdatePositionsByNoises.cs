using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UpdatePositionsByNoises : MonoBehaviour {
    [SerializeField] private ComputeShader updatePositionsShader;
    [SerializeField] private RenderTexture initialPositions; // 初期位置のテクスチャ
    [SerializeField] private RenderTexture currentPositions; // 出力先のテクスチャ

    struct KernelInfo {
        public int kernelIndex;
        public uint threadSizeX;
        public uint threadSizeY;
        public uint threadSizeZ;
        public RenderTexture oldPositions;
        public RenderTexture newPositions;
    }

    private List<KernelInfo> kernelInfos = new List<KernelInfo>();

    void Start() {
        kernelInfos.Add(initShader(updatePositionsShader, initialPositions, "MoveByFBMNoise"));
    }

    private static KernelInfo initShader(ComputeShader shader, RenderTexture initialPositions, string kernelName) {
        KernelInfo kernelInfo = new KernelInfo();

        if (initialPositions.format != RenderTextureFormat.ARGBFloat) {
            Debug.LogError(initialPositions.name + "は書き込み可能なテクスチャではないようです。");
        }

        // oldPosition用のテクスチャをinitialPositionに合わせて生成
        kernelInfo.oldPositions = new RenderTexture(initialPositions.width, initialPositions.height, 1, initialPositions.format);
        kernelInfo.oldPositions.enableRandomWrite = true;
        Graphics.CopyTexture(initialPositions, kernelInfo.oldPositions);

        // ComputeShaderにテクスチャをセット
        kernelInfo.kernelIndex = shader.FindKernel(kernelName);

        // テクスチャサイズのチェック
        shader.GetKernelThreadGroupSizes(
            kernelInfo.kernelIndex,
            out kernelInfo.threadSizeX,
            out kernelInfo.threadSizeY,
            out kernelInfo.threadSizeZ
        );
        float threadGroupSizeX = (float) kernelInfo.oldPositions.width / kernelInfo.threadSizeX;
        float threadGroupSizeY = (float) kernelInfo.oldPositions.height / kernelInfo.threadSizeY;
        if (threadGroupSizeX % 1 != 0 || threadGroupSizeY % 1 != 0) {
            Debug.LogError(initialPositions.name + "はスレッドグループ数が整数にならないので、テクスチャサイズを変えてください。");
        }

        // newPosition用のテクスチャをinitialPositionに合わせて生成
        kernelInfo.newPositions = new RenderTexture(initialPositions.width, initialPositions.height, 1, initialPositions.format);
        kernelInfo.newPositions.enableRandomWrite = true;
        Graphics.CopyTexture(initialPositions, kernelInfo.newPositions);

        return kernelInfo;
    }

    void Update() {
        // Domain Warp Noise -------------------
        var currnetKernel = kernelInfos[0];

        Graphics.CopyTexture(currnetKernel.newPositions, currnetKernel.oldPositions);
        updatePositionsShader.SetTexture(currnetKernel.kernelIndex, "oldPositions", currnetKernel.oldPositions);
        updatePositionsShader.SetTexture(currnetKernel.kernelIndex, "newPositions", currnetKernel.newPositions);
        updatePositionsShader.SetFloat("time", Time.time);
        updatePositionsShader.Dispatch(
            currnetKernel.kernelIndex,
            currnetKernel.newPositions.width / (int) currnetKernel.threadSizeX,
            currnetKernel.newPositions.height / (int) currnetKernel.threadSizeY,
            1
        );
        Graphics.CopyTexture(currnetKernel.newPositions, currentPositions);
    }
}