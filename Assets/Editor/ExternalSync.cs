using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// Tools > NTsSphere > Sync External Assets
/// サブモジュールの資産を .meta ごと Assets/External/ へコピーする（GUID 維持＝RSC のシーンがそのまま開く）。
/// Assets/External/ は git 管轄外。clone 後と、サブモジュール更新後（球テクスチャ追加など）に実行する。
///   RouletteSphereChaser/Assets/**（Editor 以外）→ Assets/External/RouletteSphereChaser/
///   Assets/Runtime~/**（自前の実行時コード。Unity は ~ 付きフォルダを無視する）→ Assets/External/NTsSphereChaser/
///     ＝Sync 前は RSC / Loto の型が無いので、Assets 直下に置くとコンパイルエラーで Safe Mode になりこのメニューが出ない。編集は Runtime~ 側で行い Sync し直す
///   NTsLotteryEngine: BallSkinTable.cs / CreationsDb.cs / LotoRules.cs / Data/*.asset / Textures/BallSkins/*.png
public static class ExternalSync
{
    const string Rsc = "RouletteSphereChaser/Assets", RscDst = "Assets/External/RouletteSphereChaser";
    const string Loto = "NTsLotteryEngine/Assets", LotoDst = "Assets/External/NTsLotteryEngine";
    const string Scene = RscDst + "/Scenes/ParkScene_v2.unity";

    [MenuItem("Tools/NTsSphere/Sync External Assets")]
    public static void Sync()
    {
        if (!Directory.Exists(Rsc) || !Directory.Exists(Loto))
        { Debug.LogError("[ExternalSync] サブモジュールが無い: git submodule update --init --recursive"); return; }

        int n = 0;
        foreach (var dir in Directory.GetDirectories(Rsc).Where(d => Path.GetFileName(d) != "Editor"))
            n += CopyTree(dir, Path.Combine(RscDst, Path.GetFileName(dir)));
        foreach (var f in Directory.GetFiles(Rsc).Where(f => !Path.GetFileName(f).StartsWith("Editor")))
            n += CopyFile(f, RscDst);
        foreach (var name in new[] { "BallSkinTable.cs", "CreationsDb.cs", "LotoRules.cs" })   // BallSkinTable が参照する最小セット
        {
            n += CopyFile($"{Loto}/Scripts/{name}", LotoDst + "/Scripts");
            n += CopyFile($"{Loto}/Scripts/{name}.meta", LotoDst + "/Scripts");
        }
        n += CopyTree("Assets/Runtime~", "Assets/External/NTsSphereChaser");
        n += CopyTree(Loto + "/Data", LotoDst + "/Data");
        n += CopyTree(Loto + "/Textures/BallSkins", LotoDst + "/Textures/BallSkins");

        AssetDatabase.Refresh();
        // Sync 前に開くと Unity が URP のグローバル設定を Assets 直下に新造して GraphicsSettings を差し替える → RSC のものへ戻す
        var urp = AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.RenderPipelineGlobalSettings>(RscDst + "/Settings/UniversalRenderPipelineGlobalSettings.asset");
        UnityEditor.Rendering.EditorGraphicsSettings.SetRenderPipelineGlobalSettingsAsset<UnityEngine.Rendering.Universal.UniversalRenderPipeline>(urp);
        AssetDatabase.DeleteAsset("Assets/UniversalRenderPipelineGlobalSettings.asset");
        AssetDatabase.DeleteAsset("Assets/DefaultVolumeProfile.asset");
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(Scene, true) };
        Debug.Log($"[ExternalSync] {n} files → Assets/External（Build Settings: {Scene}）");
    }

    static int CopyTree(string src, string dst)
    {
        int n = 0;
        foreach (var f in Directory.GetFiles(src, "*", SearchOption.AllDirectories))
            n += CopyFile(f, Path.Combine(dst, Path.GetDirectoryName(f).Substring(src.Length).TrimStart('\\', '/')));
        return n;
    }

    static int CopyFile(string f, string dstDir)
    {
        Directory.CreateDirectory(dstDir);
        File.Copy(f, Path.Combine(dstDir, Path.GetFileName(f)), true);
        return 1;
    }
}
