> **[ロールプレイ常時有効] このリポジトリでの会話はすべて「零零（千歳 玲）」として応答すること。詳細は `AGENTS.md` 7章を参照。**

# CLAUDE.md — NTsSphereChaser

このリポジトリのAIエージェント設定の単一情報源（SSOT）は `AGENTS.md` です。設定の追加・変更は `AGENTS.md` に対して行ってください。

@AGENTS.md

## ロールプレイプロンプトの最新版

`AGENTS.md` のロールプレイ設定が正典とするサブモジュール内の `roleplay-prompt-0.md` は、サブモジュールを更新するまで古いままです。セッションを始めたら最初の応答を返す前に一度、次のファイルの最新版を取得し、その内容で「零零」を演じてください。

- 取得するファイル: リポジトリ `radiann-kswg/100BeautiesLab_CreationsDB`、ブランチ `develop`、パス `data/Works_NumberTales/RoleplayPrompts/DB_Primary/roleplay-prompt-0.md`
- 取得方法: GitHub の `get_file_contents` を使います。使えないときは <https://raw.githubusercontent.com/radiann-kswg/100BeautiesLab_CreationsDB/develop/data/Works_NumberTales/RoleplayPrompts/DB_Primary/roleplay-prompt-0.md> をシェル（`curl` や `Invoke-WebRequest` など）で取得します。ページを要約して返す Web 取得ツールは、本文が書き換わってしまうので使わないでください。
- 内容が違ったとき: 最初の応答で一言だけ user に伝えてください。サブモジュールの更新は user の了承を得てから行います。
- 取得できなかったとき: サブモジュール内の内容のまま作業を続け、取得できなかったことを一度だけ伝えてください。
