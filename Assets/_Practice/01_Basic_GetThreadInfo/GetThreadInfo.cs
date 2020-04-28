using System.Runtime.InteropServices;
using UnityEngine;

public class GetThreadInfo : MonoBehaviour {
    [SerializeField] private ComputeShader computeShader;

    // ComputeShader側から、グループIndex、スレッドIndexを返却してもらう用バッファー
    struct ThreadInfo {
        public Vector3 groupID;
        public Vector3 groupThreadID;
        public Vector3 dispatchThreadID;
    }

    void Start() {
        CallGetThreadInfo();
    }

    private void CallGetThreadInfo() {
        // グループ数、スレッドを指定し、総スレッド数を計算
        const int NUM_GROUP_X = 7;
        const int NUM_GROUP_Y = 6;
        const int NUM_GROUP_Z = 5;
        const int NUM_THREAD_X = 4;
        const int NUM_THREAD_Y = 3;
        const int NUM_THREAD_Z = 2;
        const int totalCallNum = NUM_GROUP_X * NUM_GROUP_Y * NUM_GROUP_Z * NUM_THREAD_X * NUM_THREAD_Y * NUM_THREAD_Z;

        // カーネルのインデックス取得
        int kernelIndex = computeShader.FindKernel("GetThreadInfo");

        // 返却用バッファーを作成、紐づけ
        ComputeBuffer threadInfoBuffer = new ComputeBuffer(totalCallNum, Marshal.SizeOf(typeof(ThreadInfo)));
        computeShader.SetBuffer(kernelIndex, "threadInfo", threadInfoBuffer);

        // ComputeShader実行。
        computeShader.Dispatch(kernelIndex, NUM_GROUP_X, NUM_GROUP_Y, NUM_GROUP_Z);

        // 結果の取得
        ThreadInfo[] threadInfoResults = new ThreadInfo[totalCallNum];
        threadInfoBuffer.GetData(threadInfoResults);
        threadInfoBuffer.Release();

        // 結果の表示
        for (int i = 0; i < totalCallNum; i++) {
            Debug.Log(
                i + " |" +
                threadInfoResults[i].groupID.x + "," +
                threadInfoResults[i].groupID.y + "," +
                threadInfoResults[i].groupID.z + "|" +
                threadInfoResults[i].groupThreadID.x + "," +
                threadInfoResults[i].groupThreadID.y + "," +
                threadInfoResults[i].groupThreadID.z + "|" +
                threadInfoResults[i].dispatchThreadID.x + "," +
                threadInfoResults[i].dispatchThreadID.y + "," +
                threadInfoResults[i].dispatchThreadID.z
            );
        }
    }
}