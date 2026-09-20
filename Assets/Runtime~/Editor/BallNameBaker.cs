using System;
using System.IO;
using System.Linq;
using NTsLotteryEngine;
using UnityEditor;
using UnityEngine;

/// Sync 直後（＝コピーされたばかりで names が空）の NTsBallSpawner.prefab へ、創作DB の Name_EN（1 行目。"93(Nintris)" / "Binor"）を焼く。
/// RSC の HUD は名前があれば番号の代わりに出す＝「Ball 93(Nintris)」「Ball Binor」（User 判断 2026-09-20: shortEN ではなく Name_EN を優先）。
/// ビルドしたアプリは創作DB を読めないので、名前はエディタで引いて Assets/External/（git 管轄外）に持たせる。
/// 公開基準は CreationsDb.ShownProgress のまま＝未公開キャラは空（HUD は番号だけ）。
/// ExternalSync からは呼べない（Sync 前は NTsBallSpawner / CreationsDb の型が無い）ので、Sync 後のドメインリロードで自走する。
static class BallNameBaker
{
    const string Prefab = "Assets/External/NTsSphereChaser/Resources/NTsBallSpawner.prefab";
    const string DbRoot = "NTsLotteryEngine/100BeautiesLab_CreationsDB/data/Works_NumberTales";

    [InitializeOnLoadMethod]
    static void Hook() => EditorApplication.delayCall += Bake;   // リロード中は AssetDatabase が使えない

    [MenuItem("Tools/NTsSphere/Bake Ball Names")]
    static void Bake()
    {
        var spawner = AssetDatabase.LoadAssetAtPath<NTsBallSpawner>(Prefab);
        if (spawner == null || spawner.table == null) return;
        if (spawner.names != null && spawner.names.Length == spawner.table.skins.Count) return;   // 焼き済み
        if (!Directory.Exists(Path.Combine(DbRoot, "DataBases"))) return;   // 創作DB 未 init＝名前なしで動く（AGENTS 3章）

        Environment.SetEnvironmentVariable(CreationsDb.EnvVar, Path.GetFullPath(DbRoot));   // Loto 単体と置き場所が違う
        spawner.names = spawner.table.skins.Select(s => CreationsDb.Find(s.db, s.DbNum)?.nameEN ?? "").ToArray();
        EditorUtility.SetDirty(spawner);
        AssetDatabase.SaveAssetIfDirty(spawner);
        Debug.Log($"[BallNameBaker] {spawner.names.Count(n => n != "")} / {spawner.names.Length} names → {Prefab}");
    }
}
