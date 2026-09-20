# NTsSphereChaser

[NTsLotteryEngine](https://github.com/radiann-kswg/NTsLotteryEngine) に収録しているナンバーテールズ柄のボールテクスチャで、
[RouletteSphereChaser](https://github.com/radiann-kswg/RouletteSphereChaser) のボールコースターを眺める観賞用アプリ。
Unity `6000.6.2f1`（URP）／ Linux x86_64 向け（[RasPiOS_UnityConsole](https://github.com/radiann-kswg/RasPiOS_UnityConsole) での鑑賞を想定）。

流れる球は、テクスチャが出来上がっているキャラクターの分だけ。NTsLotteryEngine 側にテクスチャが増えると、球も増える。

## セットアップ

```
git clone https://github.com/radiann-kswg/NTsSphereChaser.git && cd NTsSphereChaser
git submodule update --init
git -C NTsLotteryEngine submodule update --init LotteryBallKit
# 球の名前を HUD に出すなら（任意。無くても動き、番号だけの表示になる）
git -C NTsLotteryEngine submodule update --init --depth 1 100BeautiesLab_CreationsDB
git -C NTsLotteryEngine/100BeautiesLab_CreationsDB sparse-checkout set --no-cone '/*.md' '/LICENCE' '/data/Works_NumberTales/DataBases/**'
```

Unity `6000.6.2f1` で開き、`Tools > NTsSphere > Sync External Assets` を 1 回実行する（サブモジュールの資産を `Assets/External/` へコピー。git 管轄外）。
`Assets/External/RouletteSphereChaser/Scenes/ParkScene_v2.unity` を開いて Play。

追従中の球は HUD に `Ball 93(Nintris)` / `Ball Binor` のように創作DB の `Name_EN` で出る。名前は Sync のときにエディタで焼き込むので、ビルドしたアプリは創作DB を持ち歩かない。
操作（キーボード／ゲームパッド）と音は RouletteSphereChaser と同じ（[README](https://github.com/radiann-kswg/RouletteSphereChaser#readme) の操作表）。

## ビルド

```
unity build . --target StandaloneLinux64 --output-path Builds/NTsSphereChaser/NTsSphereChaser.x86_64
```

RasPiOS_UnityConsole へ入れるときは、ビルドフォルダに `game.json` をコピーして USB メモリの `UnityGames/` に置く。

## ライセンス

[CC BY-NC 4.0](LICENSE.md)（ナンバーテールズ／百花繚乱研究所の創作物に準拠）。サブモジュールはそれぞれのライセンスに従う。

© 百花繚乱研究所 / ラジアン（柏木主税） / RadianN_kswg
