using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.Experimental.Rendering;

[CustomEditor(typeof(TextureMaker))]
public class TextureMakerExecutor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        GUILayout.BeginVertical("Box");
        {
            GUILayout.Label("スレッドIndexを書いた2Dテクスチャを作成");
            if (GUILayout.Button("作成する")) {
                // ランタイムでないせいか、１回目は色が保存されない
                ((TextureMaker) target).CreatePositionColorTexture2D();
                ((TextureMaker) target).CreatePositionColorTexture2D();
            }

            GUILayout.Space(16);

            GUILayout.Label("スレッドIndexを書いた3Dテクスチャを作成");
            if (GUILayout.Button("作成する")) {
                // ランタイムでないせいか、１回目は色が保存されない
                ((TextureMaker) target).CreatePositionColorTexture3D();
                ((TextureMaker) target).CreatePositionColorTexture3D();
            }

            GUILayout.Space(16);
            
            GUILayout.Label("ランダム値で初期化した2Dテクスチャを作成");
            if (GUILayout.Button("作成する")) {
                // ランタイムでないせいか、１回目は色が保存されない
                ((TextureMaker) target).CreateRandomColorTexture2D();
                ((TextureMaker) target).CreateRandomColorTexture2D();
            }

            GUILayout.Space(16);

            GUILayout.Label("ランダム値で初期化した3Dテクスチャを作成");
            if (GUILayout.Button("作成する")) {
                // ランタイムでないせいか、１回目は色が保存されない
                ((TextureMaker) target).CreateRandomColorTexture3D();
                ((TextureMaker) target).CreateRandomColorTexture3D();
            }
        }
        GUILayout.EndVertical();
    }
}


[CreateAssetMenu(menuName = "ScriptableObject/TextureMaker", fileName = "TextureMaker")]
public class TextureMaker : ScriptableObject {
    [SerializeField] private ComputeShader computeShader;
    [SerializeField] private Vector2 texture2DSize = new Vector2(512, 512);
    [SerializeField] private Vector3 texture3DSize = new Vector3(128, 128, 128);

    [Header("色保存:ARGB32 位置保存:ARGB Float")]
    [SerializeField] private RenderTextureFormat textureFormat; // 出力先のテクスチャ

    [SerializeField] private DefaultAsset outDir;

    private int kernelIndex;
    private uint threadSizeX;
    private uint threadSizeY;
    private uint threadSizeZ;

    // 頂点情報をそのままRGBにマッピングした２Dテクスチャ作成
    public void CreatePositionColorTexture2D() {
        CreateTexture2D("SetPositionColor2D", "position_color_texture2d.asset");
    }

    // 頂点情報をそのままRGBにマッピングした３Dテクスチャ作成
    public void CreatePositionColorTexture3D() {
        CreateTexture3D("SetPositionColor3D", "position_color_texture2d.asset");
    }

    // 頂点情報をそのままRGBにマッピングした２Dテクスチャ作成
    public void CreateRandomColorTexture2D() {
        CreateTexture2D("SetRandomColor2D", "random_color_texture2d.asset");
    }

    // 頂点情報をそのままRGBにマッピングした３Dテクスチャ作成
    public void CreateRandomColorTexture3D() {
        CreateTexture3D("SetRandomColor3D", "random_color_texture2d.asset");
    }

    // 頂点情報をそのままRGBにマッピングした２Dテクスチャ作成
    private void CreateTexture2D(string kernelName, string assetName) {
        // ３Dテクスチャ作成。depth方向の設定のしかたが特殊な感じ。コンストラクタでのdepth設定は0、そのあとvolumeDepthで奥行き設定。
        RenderTexture renderTexture = new RenderTexture((int) texture2DSize.x, (int) texture2DSize.y, 0, textureFormat);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();

        // ComputeShaderにテクスチャをセット
        kernelIndex = computeShader.FindKernel(kernelName);
        computeShader.SetTexture(kernelIndex, "textureBuffer2D", renderTexture);

        // テクスチャサイズのチェック
        computeShader.GetKernelThreadGroupSizes(kernelIndex, out threadSizeX, out threadSizeY, out threadSizeZ);
        float threadGroupSizeX = renderTexture.width / threadSizeX;
        float threadGroupSizeY = renderTexture.height / threadSizeY;
        if (threadGroupSizeX % 1 != 0 || threadGroupSizeY % 1 != 0) {
            EditorUtility.DisplayDialog("テクスチャサイズエラー", "スレッドグループ数が整数にならないので、テクスチャサイズを変えてください。", "OK");
        }

        // Compute Shaderでテクスチャ更新
        computeShader.Dispatch(
            kernelIndex,
            renderTexture.width / (int) threadSizeX,
            renderTexture.height / (int) threadSizeY,
            1
        );

        // レンダーテクスチャはランタイム用のためか、Editorからはこれをしないと反映されない
        EditorUtility.SetDirty(renderTexture);

        // 保存
        var path = outDir ? AssetDatabase.GetAssetPath(outDir) : Path.GetDirectoryName(AssetDatabase.GetAssetPath(this));
        AssetDatabase.CreateAsset(renderTexture, path + "/" + assetName);
        AssetDatabase.SaveAssets();
    }

    private void CreateTexture3D(string kernelName, string assetName) {
        // ３Dテクスチャ作成。depth方向の設定のしかたが特殊な感じ。コンストラクタでのdepth設定は0、そのあとvolumeDepthで奥行き設定。
        RenderTexture renderTexture = new RenderTexture((int) texture3DSize.x, (int) texture3DSize.y, 0, textureFormat);
        renderTexture.enableRandomWrite = true;
        renderTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        renderTexture.volumeDepth = (int) texture3DSize.z;
        renderTexture.Create();

        // ComputeShaderにテクスチャをセット
        kernelIndex = computeShader.FindKernel(kernelName);
        computeShader.SetTexture(kernelIndex, "textureBuffer3D", renderTexture);

        // テクスチャサイズのチェック
        computeShader.GetKernelThreadGroupSizes(kernelIndex, out threadSizeX, out threadSizeY, out threadSizeZ);
        float threadGroupSizeX = renderTexture.width / threadSizeX;
        float threadGroupSizeY = renderTexture.height / threadSizeY;
        float threadGroupSizeZ = renderTexture.volumeDepth / threadSizeZ;
        if (threadGroupSizeX % 1 != 0 || threadGroupSizeY % 1 != 0 || threadGroupSizeZ % 1 != 0) {
            EditorUtility.DisplayDialog("テクスチャサイズエラー", "スレッドグループ数が整数にならないので、テクスチャサイズを変えてください。", "OK");
        }

        // Compute Shaderでテクスチャ更新
        computeShader.Dispatch(
            kernelIndex,
            renderTexture.width / (int) threadSizeX,
            renderTexture.height / (int) threadSizeY,
            renderTexture.volumeDepth / (int) threadSizeZ
        );

        // レンダーテクスチャはランタイム用のためか、Editorからはこれをしないと反映されない
        EditorUtility.SetDirty(renderTexture);

        // 保存
        var path = outDir ? AssetDatabase.GetAssetPath(outDir) : Path.GetDirectoryName(AssetDatabase.GetAssetPath(this));
        AssetDatabase.CreateAsset(renderTexture, path + "/" + assetName);
        AssetDatabase.SaveAssets();
    }
}