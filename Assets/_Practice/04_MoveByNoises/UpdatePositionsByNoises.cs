﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class UpdatePositionsByNoises : MonoBehaviour {
    [SerializeField] private ComputeShader updatePositionsShader;
    [SerializeField] private RenderTexture currentPositions; // 出力先のテクスチャ

    struct KernelInfo {
        public int kernelIndex;
        public uint threadSizeX;
        public uint threadSizeY;
        public uint threadSizeZ;
        public RenderTexture oldPositions;
        public RenderTexture newPositions;
    }

    private KernelInfo initializeKernel;
    private List<KernelInfo> kernelInfos = new List<KernelInfo>();
    private int currentKernelIndex = 0;

    void Start() {
        initializeKernel = initShader(updatePositionsShader, currentPositions, "SetInitialPositions");
        kernelInfos.Add(initShader(updatePositionsShader, currentPositions, "MoveByFBMNoise"));
        kernelInfos.Add(initShader(updatePositionsShader, currentPositions, "MoveByCurlNoise"));

        for (int i = 0; i < kernelInfos.Count; i++) {
            ResetPosition(kernelInfos[i]);
        }
    }

    private static KernelInfo initShader(ComputeShader shader, RenderTexture positionTexture, string kernelName) {
        KernelInfo kernelInfo = new KernelInfo();

        if (positionTexture.format != RenderTextureFormat.ARGBFloat) {
            Debug.LogError(positionTexture.name + "は書き込み可能なテクスチャではないようです。");
        }

        // oldPosition用のテクスチャをinitialPositionに合わせて生成
        kernelInfo.oldPositions = new RenderTexture(positionTexture.width, positionTexture.height, 1, positionTexture.format);
        kernelInfo.oldPositions.enableRandomWrite = true;
        Graphics.CopyTexture(positionTexture, kernelInfo.oldPositions);

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
            Debug.LogError(positionTexture.name + "はスレッドグループ数が整数にならないので、テクスチャサイズを変えてください。");
        }

        // newPosition用のテクスチャをinitialPositionに合わせて生成
        kernelInfo.newPositions = new RenderTexture(positionTexture.width, positionTexture.height, 1, positionTexture.format);
        kernelInfo.newPositions.enableRandomWrite = true;
        Graphics.CopyTexture(positionTexture, kernelInfo.newPositions);

        return kernelInfo;
    }

    private void ResetPosition(KernelInfo currnetKernel) {
        updatePositionsShader.SetTexture(initializeKernel.kernelIndex, "newPositions", initializeKernel.newPositions);
        updatePositionsShader.Dispatch(
            initializeKernel.kernelIndex,
            initializeKernel.newPositions.width / (int) initializeKernel.threadSizeX,
            initializeKernel.newPositions.height / (int) initializeKernel.threadSizeY,
            1
        );
        Graphics.CopyTexture(initializeKernel.newPositions, currnetKernel.newPositions);
    }

    void Update() {
        var currnetKernel = kernelInfos[currentKernelIndex];

        // initialize per period -------------------
        if (Time.frameCount % 800 == 0) {
            currentKernelIndex++;
            if (currentKernelIndex >= kernelInfos.Count)
                currentKernelIndex = 0;
            
            ResetPosition(currnetKernel);
        }

        // move by noise -------------------
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