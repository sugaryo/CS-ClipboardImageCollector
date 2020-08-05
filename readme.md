# ClipboardImageCollector

いわゆるクリップボード監視ツール。  

監視というか `AddClipboardFormatListener` してリスナー登録してイベントドリブンで呼び出してもらって `WndProc` で処理する。

## 機能的な

クリップボードに（ハードコピー・キャプチャとかで）画像が突っ込まれたら、jpegで保存する。

## 今後やりたいこと

- jpeg 圧縮率を外部定義化
- そもそも jpeg 以外に png や bmp でも保存できるようにする？
- ファイル名が今は雑なので、WinShotみたいに連番管理とか？

まぁ、気が向いたら。
