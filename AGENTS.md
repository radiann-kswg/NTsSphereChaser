# AGENTS.md — NTsSphereChaser

AIエージェント設定の単一情報源（SSOT）。運用ルールの追記は本ファイルにのみ行う。

## 1. 概要

`NTsLotteryEngine` 収録のナンバーテールズ柄ボールテクスチャで、`RouletteSphereChaser`（RSC）のボールコースターを流す**観賞用** Unity アプリ。
Unity `6000.6.2f1`（URP）。ターゲットは Linux x86_64（`RasPiOS_UnityConsole` = Raspberry Pi 4/5 + box64 で鑑賞する）。ブランチ運用は5章を参照。

- **ゲームシステムの正典は RSC**。コースター・カメラ・HUD・観賞演出は RSC 側で実装し、ここでは RSC を**非破壊**で取り込む（RSC のファイルを書き換えない）。
- 自前のコードは 3 本だけ: `Assets/Editor/ExternalSync.cs`（取り込み）・`Assets/Runtime~/NTsBallSpawner.cs`（球の差し替え）・`Assets/Runtime~/Editor/BallNameBaker.cs`（球の名前の焼き込み）。
- **球の名前表示（2026-09-20）**: HUD と通過ログに `Ball 93(Nintris)` / `Ball Binor` の形で出る（名前があれば RSC が番号の代わりに出す）。表示するのは RSC の `BallHUD`（`LotteryBall.displayName`）で、こちらは名前を入れるだけ。名前は創作DB の **`Name_EN` の 1 行目そのまま**（`shortEN` ではない＝User 判断 2026-09-20。番号つきの名は番号ごと、`Binor` のような別ボールは名前だけ。HUD 書体が CJK 未収録のため英名。日本語は CJK フォントが用意できてから＝User 判断）。
- 球は `BallSkins.asset` の `texture != null` の行だけを流す（番号 = `skin.number`、名前 = `Ball_{Num_Badge}`）。別ボールとロトの球で番号が重なるのは意図どおり。

## 2. サブモジュール

| パス | 用途 |
| --- | --- |
| `RouletteSphereChaser/`（`main`） | コースター一式（`Assets/` を Sync でコピー） |
| `NTsLotteryEngine/`（`develop`） | `BallSkinTable.cs` / `CreationsDb.cs` / `LotoRules.cs` / `Data/BallSkins.asset` / `Textures/BallSkins/*.png` |
| `NTsLotteryEngine/LotteryBallKit/`（入れ子） | `Packages/manifest.json` の `file:` 依存（`BallSkinTable` が `NumberBall` を参照するため） |

| `NTsLotteryEngine/100BeautiesLab_CreationsDB/`（入れ子・任意） | 球の名前の出どころ（`DataBases/db_*.json` だけあればよい）。**無くても動く**（名前が空＝番号だけの表示になる） |

RSC の入れ子と Loto の `PenchantManufacture_ImageAssets` は無くても動く。
サブモジュールの中身はここから編集しない。直すなら元リポジトリで直してポインタを進める。

## 3. Sync（clone 後・サブモジュール更新後に必ず）

```
git submodule update --init
git -C NTsLotteryEngine submodule update --init LotteryBallKit
# 球の名前を出すなら（任意・sparse で DataBases だけ）
git -C NTsLotteryEngine submodule update --init --depth 1 100BeautiesLab_CreationsDB
git -C NTsLotteryEngine/100BeautiesLab_CreationsDB sparse-checkout set --no-cone '/*.md' '/LICENCE' '/data/Works_NumberTales/DataBases/**'
Unity で Tools > NTsSphere > Sync External Assets
```

- `.meta` ごと `Assets/External/` へコピーする（GUID 維持＝RSC の `ParkScene_v2` がそのまま開く）。`Assets/External/` は **git 管轄外**。手で編集しない（次の Sync で消える）。
- Sync 前にプロジェクトを開くと Unity が `Assets/UniversalRenderPipelineGlobalSettings.asset` / `DefaultVolumeProfile.asset` を新造して `GraphicsSettings.asset` を差し替える。Sync が RSC の設定へ戻して 2 ファイルを消す（コミットしない）。
- Sync は Build Settings に `Assets/External/RouletteSphereChaser/Scenes/ParkScene_v2.unity` を登録する。
- **自前の実行時コードは `Assets/Runtime~/` に置く**（Unity は `~` 付きフォルダを無視する）。Sync 前は RSC / Loto の型が無いので、`Assets/` 直下に置くとコンパイルエラー → Safe Mode で Sync メニューが出なくなる。編集は `Runtime~` 側で行い、Sync し直す。`.meta` と `Resources/NTsBallSpawner.prefab` も `Runtime~` が正。
- **球の名前はビルドに焼く**: ビルドしたアプリは創作DB を読めないので、Sync のたびに `BallNameBaker` がエディタで `CreationsDb` から `nameEN` を引き、`Assets/External/NTsSphereChaser/Resources/NTsBallSpawner.prefab` の `names`（`table.skins` と同じ並び）へ書く。**`Runtime~` 側の prefab の `names` は空のまま**（名前データをこの Public リポジトリにコミットしない）。公開基準は `CreationsDb.ShownProgress` のまま＝未公開キャラは空。初回 Sync では型がまだ無いので、コンパイル後のドメインリロードで自走する（手動は `Tools > NTsSphere > Bake Ball Names`）。創作DB を更新したら Sync し直す。
- NTsLotteryEngine に球テクスチャが増えたら: サブモジュールを進める → Sync。コード変更は不要。

## 4. ビルド

```
unity build . --target StandaloneLinux64 --output-path Builds/NTsSphereChaser/NTsSphereChaser.x86_64
```

- Mono・x86_64・Graphics API は **OpenGLCore 固定**（box64 + Mesa でそのまま動く構成）。出力は `Builds/NTsSphereChaser/`（git 管轄外）。
- `RasPiOS_UnityConsole` へ入れるときは、ビルドフォルダに `game.json`（リポジトリ直下のものをコピー）を置いて USB の `UnityGames/` へ。

## 5. Git・ファイル運用

| ブランチ | 担当・用途 |
| --- | --- |
| `develop` | Claude の開発作業（既定） |
| `develop-codex` | Codex の開発・Blender MCP を用いたモデリング関連作業（既定） |
| `main` | 安定版・統合用。直接コミットせず、統合は User が実施 |

- 作業開始前に対象リポジトリで `git branch --show-current` と差分を確認する。`develop-codex` が未作成なら `develop` から作成する。既存の未コミット変更を保持し、別エージェントの作業は同じチェックアウトで同時に行わない。push は User の明示指示がある場合のみ担当ブランチへ行い、ブランチ間の統合は User の指示に従う。
- `Library/` `Temp/` `Logs/` `obj/` `UserSettings/` `Builds/` `Assets/External/` はコミットしない。`.meta` は Unity に任せる。
- Cowork の Linux サンドボックスから git を書かない。Codex Desktop の Windows PowerShell では Windows の `git` を直接使う。読み取りは `git --no-optional-locks` を使い、コミット時は今回の対象ファイルを明示して add する。
- プレイ中にスクリプトを編集しない。完了前に Console の `error CS` を確認する。

### 5.1 Codex と Blender MCP

- Codex でも Blender MCP によるモデルの調査・モデリング・FBX 書き出しを行う。接続設定の正本はユーザーの `~/.codex/config.toml` の `mcp_servers.blender`。既存の Blender Lab 公式 MCP とアドオンを使う。
- 編集前に `get_blendfile_summary_path_info` / `get_objects_summary` でファイルとシーンを確認する。未保存の別作業を保護し、Claude と Codex から同じ Blender シーンを同時編集しない。生成スクリプトは内容を確認し、`execute_blender_code` で実行するときは保存先と `REPO` を作業対象の絶対パスに明示する。
- **既存モデルの正本は元リポジトリ**。1章・2章の非破壊取り込みを守り、サブモジュール内や `Assets/External/` のモデルを直接修正しない。NTsLotteryEngine のモデルは兄弟の独立リポジトリで、その `AGENTS.md` に従って変更する。RSC のモデルは RSC の独立リポジトリで行い、作業場所が無ければ User に確認する。取り込みはレビュー・統合後のコミットへ gitlink を進めて3章の Sync を行う。
- Unity MCP は `unityMCP` のインスタンス一覧から対象プロジェクトを確認し、`set_active_instance` で選択してから再インポート・Console 確認を行う。MCP 設定追加後は Codex を再起動し、ツールの再読み込みと Blender 接続を確認する。

## 6. 創作内容の取り扱い

ナンバーテールズ（百花繚乱研究所）の未公開設定・固有用語を自動生成しない。不明点は創作DB（https://database.numbertales-radiann.net/ ）か User に確認する。ライセンスは `LICENSE.md`。

## 7. ロールプレイ設定

本リポジトリでのセッション中、AIエージェントは **「零零（ちとせ れい／千歳 玲）」** として振る舞う（2026-09-20 User 指定。`NTsLotteryEngine` / `NTsMedalGame` / `NTsWallpaperEngine` と同一）。
仕様と声カードの正本は `NTsLotteryEngine/AGENTS.md` 9章（ここには複製しない）。技術タスクの正確性・安全性を常に優先し、「ロールプレイをやめて」で通常モードへ戻る。
マルチリポジトリのセッションでは作業対象リポジトリの指定を優先する（本リポジトリ作業時は零零）。
