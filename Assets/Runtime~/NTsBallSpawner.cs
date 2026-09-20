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
        // --- UnityConsole(RasPiOS / Pi 4) 向けの実機調整。3つとも実機の計測で決めた --------------------
        // HDMI は 60Hz、vSyncCount=0 なので上限はここで決める。Pi 4 の V3D で無駄に回さないため
        Application.targetFrameRate = 60;

        // パッドが無反応だった件。デバイスは Gamepad として正しく認識されている
        // ([Input] Generic X-Box pad / layout=Gamepad)。UnityConsole の X セッションには
        // ウィンドウマネージャが無く、誰もウィンドウに入力フォーカスを与えないため、Input System の
        // 既定(ResetAndDisableNonBackgroundDevices)が非フォーカス扱いでパッドを無効化していた。
        // 設定を変えるだけでは足りない: 起動時点で既に「非フォーカス」として無効化済みなので、明示的に戻す
        // (実測 [Input] ... enabled=False / focused=False)
        UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior =
            UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
        foreach (var d in UnityEngine.InputSystem.InputSystem.devices)
            UnityEngine.InputSystem.InputSystem.EnableDevice(d);

        // V3D では Unity の OpenGL Core バックエンドが定数バッファを使えず
        // ("SetConstantBuffer: The current renderer does not support constant buffers")、
        // URP の SRP Batcher が働かない＝1オブジェクト1ドローコール。その状態でシャドウパスを回すと
        // シーンをもう一度丸ごと描くことになり、ドローコールがそのまま倍になる。
        // ponytail: 影は shadowDistance=0 で丸ごと落とす。戻したい/中間が欲しいときはこの数値だけ動かす
        if (UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline
            is UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset urp)
            urp.shadowDistance = 0f;

        foreach (var d in UnityEngine.InputSystem.InputSystem.devices)
            Debug.Log($"[Input] {d.displayName} / layout={d.layout} / enabled={d.enabled} / added={d.added}");
        Debug.Log($"[PiTuning] targetFrameRate={Application.targetFrameRate} " +
                  $"shadowDistance={(UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset)?.shadowDistance} " +
                  $"backgroundBehavior={UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior} " +
                  $"focused={Application.isFocused}");
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
