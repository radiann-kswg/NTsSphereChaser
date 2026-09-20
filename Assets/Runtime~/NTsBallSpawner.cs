using System.Collections;
using NTsLotteryEngine;
using UnityEngine;

/// RSC の BallSpawner を止めて、BallSkins.asset でテクスチャが入っている球だけを流す。
/// RSC のシーンは Sync で上書きされるので触らない。NTsLotteryEngine に球テクスチャが増えたら Sync し直すだけで球が増える。
public class NTsBallSpawner : MonoBehaviour
{
    public BallSkinTable table;   // Resources/NTsBallSpawner.prefab で BallSkins.asset を参照（ビルドに含めるため）
    public string[] names;        // table.skins と同じ並びの 創作DB の Name_EN。Sync 後に Editor/BallNameBaker が焼く（Runtime~ 側の prefab は空のまま）

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        var rsc = FindAnyObjectByType<BallSpawner>();
        if (rsc == null) return;
        rsc.enabled = false;   // AfterSceneLoad は Start より前＝RSC 側は 1 球も出さない
        var me = Instantiate(Resources.Load<NTsBallSpawner>("NTsBallSpawner"), rsc.transform.position, Quaternion.identity);
        me.StartCoroutine(me.Spawn(rsc));
    }

    IEnumerator Spawn(BallSpawner rsc)
    {
        int i = 0;
        for (int k = 0; k < table.skins.Count; k++)
        {
            var skin = table.skins[k];
            if (skin.texture == null) continue;
            var pos = transform.position;
            if ((i++ & 1) == 1) pos.z = -pos.z;   // RSC と同じ南北交互投入
            var go = Instantiate(rsc.ballPrefab, pos, Random.rotation);
            // テクスチャ名 BallTex_NTS-{Num_Badge} が Badge の正（創作DB を実行時に読まずに済む）
            go.name = "Ball_" + skin.texture.name.Replace("BallTex_NTS-", "");
            var ball = go.GetComponent<LotteryBall>();
            ball.number = skin.number;
            ball.displayName = names != null && k < names.Length ? names[k] : "";   // 創作DB の Name_EN。RSC の BallHUD が "Ball 93(Nintris)" / "Ball Binor" と出す
            ball.Apply();
            ball.SetCharacterTexture(skin.texture);
            yield return new WaitForSeconds(rsc.interval);
        }
        Debug.Log($"[NTsBallSpawner] {i} balls");
    }
}
