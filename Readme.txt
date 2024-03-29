YorkTrail - Music Player for Transcription
Copylight(c) 2022 theta

・概要
  耳コピ用途に特化した音楽プレイヤーです。  対応ファイル形式はwav・mp3・flacで
  テンポ・ピッチ変更、その他各種エフェクトをかけながら再生することができます。
  区間ループ再生やテンポ計算などもできます。というかほぼWPAK32です。
  素晴らしいソフトを開発されたMIO.Hさんに感謝。

・注意
  v1.0.0以降はAVX非対応のCPUでは動作しません。
  v0.7.xをお使いください。

・インストール
  このアーカイブを適当なフォルダに配置し、別でダウンロードした
  tensorflow+models.zipを解凍して同じフォルダに置いてください。
  「.NET デスクトップ ランタイム 6.0」以降が必要ですので
  入っていない場合はインストールしてください。
  https://dotnet.microsoft.com/ja-jp/download/dotnet/6.0


・使い方
  基本的な使い方はいじってればだいたいわかると思います。ほぼWPAK32なので。

・YorkTrail独自機能について
  シークバーの目盛りは拡大時にドラッグで移動できます。
  目盛りの拡大縮小は段階的に出来ます。
  テンポ計算ウィンドウでテンポ計算すると小節表示ができるようになります。
  マーカーを設置するとマーカー間移動ができます。
  あと区間固定とかティックにスナップとかもできます。

・Stem分離機能について
  Stem分離はかなり重い処理なので実行時には注意してください。（処理中も再生できます）
  一度分離してしまえば分離済みファイルを参照するだけですので処理は軽いです。
  分離済みファイルはマイドキュメント下のYorkTrailフォルダにFLACで保存されますので
  不要であれば削除してください。

・Stemのインポート
  外部で作成したStemファイルをインポートできます。
  「ファイル→Stemのインポート」か、フォルダをStem Controlの部分にドラッグで
  インポートできます。ファイル名はvocals/drums/bass/piano/other形式に合わせてください。
  存在しないパートは無音になります。

・波形表示について
  デフォルトで有効ですが設定で無効にできます。
  波形表示をするとファイル読み込みが多少重くなります。
  （↑v1.0.1から別スレッド化したのでファイル読み込み自体は重くなりません）

・タイムストレッチ方式の設定について
  SoundTouchとRubber Bandから選択できます。（デフォルトはRubber Band）
  音質・CPU負荷共に SoundTouch < Rubber Band です。

・SoundTouchの設定について
  値を変更することで、速度やピッチ変更時の品質を調整することができます。
  基本的に値を大きくするとスロー再生向けに、小さくすると早送り向けになりますが
  極端な値にすると逆効果です。

  SEQUENCE_MS … 波形を切り刻む長さ
  SEEKWINDOW_MS … ミキシング位置の検索範囲
  OVERLAP_MS … 波形同士のオーバーラップ長

  詳しくはここら辺を参照のこと
  3.4 Tuning the algorithm parameters
  https://www.surina.net/soundtouch/README.html
  タイムストレッチ、ピッチシフトのアルゴリズム
  http://ackiesound.ifdef.jp/tech/timestretch.html


・使用ライブラリ
  Rubber Band Audio Time Stretcher Library
  https://breakfastquay.com/rubberband/

  SoundTouch Audio Processing Library
  https://www.surina.net/soundtouch/
  
  miniaudio
  https://miniaud.io/


・著作権および免責事項
  本ソフトはフリーソフトです。個人・団体での利用は自由にしてもらって構いません。
  再配布は個人であれば自由ですが、雑誌などに掲載される際にはご連絡ください。
  なお，著作権は作者である theta が保有しています。

  このソフトウェアを使用したことによって生じたすべての障害・損害・不具合等に関して
  は、私と私の関係者および私の所属するいかなる団体・組織とも、一切の責任を負いませ
  ん。各自の責任においてご使用ください。


・著者
  theta
  http://theta.s57.xrea.com/software/
  https://twitter.com/theta88p


・更新履歴
  v1.0.5         2023/07/31
    目盛りをドラッグすると拡大縮小が効かなくなるのを修正

  v1.0.4         2023/05/25
    拡大縮小が効かなくなっていたのを修正
    Stem操作部分を表示するとスペースキーでのショートカットが効かなくなるのを修正
    tensorflow.dllがない場合、エラーメッセージを出して終了するように

  v1.0.3         2023/04/13
    Stem分離したファイルの長さが元音源より少し少なかったのを修正
    Stemファイルが開けなかったとき操作不能になるのを修正
    Stemをインポートする機能の追加

  v1.0.2         2023/03/25
    ソース切り替え後にファイルドロップ出来なかったり
    ファイルを開くダイアログを開くとフリーズすることがあったのを修正
    履歴がない時一番下のセパレータを表示しないようにした

  v1.0.1         2023/03/22
    ボタンの有効・無効が即座に切り替わらなかったのを修正
    ファイルを開くダイアログを開くとフリーズすることがあったのを修正
    StemとSourceを切り替えた時ピッチと速度が切り替わらないのを修正

  v1.0.0dev      2023/03/19
    Stem分離機能の追加
    波形取得の高速化と別スレッド化
    テンポ自動取得機能の追加
    ウィンドウのリサイズモードを変更

  v0.7.0         2023/03/06
    シークバーに波形表示をする機能の追加
    終了時の状態を復元する設定にしていた場合、ファイルを閉じると
    起動時にファイルが存在しないと表示されるのを修正

  v0.6.15         2022/11/20
    メニューとショートカットから速度を変更すると止まることがあるのを修正
    .NET6に移行

  v0.6.14beta     2021/12/29
    区間リセットボタンの位置を変更

  v0.6.13         2021/12/26
    現在位置がファイルの最後に到達したときループしなかったのを修正

  v0.6.12         2021/12/25
    目盛りを動かしたときの挙動を修正

  v0.6.11beta     2021/12/23
    右クリックでスライダーポップアップ機能の追加
    スライダーをドラッグしたときポップアップで値を表示するように

  v0.6.10beta     2021/12/22
    タイムストレッチ設定が保存されないのを修正

  v0.6.9beta     2021/12/22
    タイムストレッチ方式にRubber Bandを追加
    ライセンスをGPLに変更

　v0.6.8beta     2021/12/20
    小節で表示の時、拍未満を2桁に固定
    ファイルを開いていない状態でピッチとテンポを操作すると、ファイルを開いたとき
    正しく反映されないのを修正

　v0.6.7         2021/12/19
    正式版に昇格

　v0.6.7beta     2021/12/16
    キーカスタマイズに再生区間拡大縮小が表示されていなかったのを修正
    ドラッグ中シークバー外に移動しても大丈夫なように

　v0.6.6beta     2021/12/14
    小節表示・ティックにスナップ・区間固定のパラメータを無条件で保存するように
    シークバー左クリック時にティックにスナップ・区間固定が効いていなかったのを修正

　v0.6.5beta     2021/12/13
    2つ前のマーカーに戻る範囲を1秒に固定
    スロー再生すると即停止してしまうことがあったのを修正
    ズームと目盛りの描画の挙動を変更
    最近使ったファイルをメニューと同じ階層に表示
    シークバー左クリックの挙動を変更

　v0.6.4beta     2021/12/11
    マーカー機能の追加
    入力欄に変な値を入れると落ちる可能性があるのを修正
    再生関係の項目を再生メニューへ移動
    v0.6.3で早送り・巻き戻しができなくなっていたのを修正
    再生中にOS側でデバイスが切り替わると操作不能になるのを修正

　v0.6.3         2021/12/8
    開始終了を固定しているとき終了位置が端にあるとUIと内部でズレが生じるのを修正
    テンポ計算の時間入力ボタンで処理時間を考慮するようにした
    曲の最後にシークするとエラーが発生することがあったのを修正

　v0.6.2beta     2021/12/6
    段階的に拡大縮小できるようにした
    目盛りの描画範囲の修正

　v0.6.1beta     2021/12/5
    目盛りをドラッグ移動できるように
    サウンドデバイスの文字化けを修正
    サウンドデバイスの選択が反映されないのを修正

　v0.6.0beta     2021/12/5
    小節表示の追加
    ティックにスナップ機能の追加
    再生区間固定機能の追加

　v0.5.0　　2021/12/2
    シークバーに時間を表示するようにした
    ファイル形式をヘッダを見て判断するようにした
    （ヘッダで判断できない場合は拡張子で判断します）
    再生処理まわりのリファクタリングをした
    SoundTouchをv2.3.1に更新
    miniaudioをv0.10.42に更新

    ※64bit版はSoundTouchDLL_x64.dllをSoundTouchDLL.dllにリネームしたので
      SoundTouchDLL_x64.dllはアップデート時に捨ててください

　v0.4.0　　2021/11/28
    フィルタプリセット機能の追加
    シークバーに時々点線が表示されていたのを修正（多分）

　v0.3.3　　2021/7/23
    一時停止中にファイルを開くと一時停止状態のまま再生される不具合が
    完全に修正されていなかったのを修正

　v0.3.2　　2021/5/6
    ボタンにフォーカスがある時ショートカットが効かないのを修正
    キーカスタマイズが正常に復元されていなかったのを修正
    再起動するまでキーカスタマイズが適用されなかったのを修正

　v0.3.1　　2021/5/5
    オプションウィンドウを追加
    前回終了時の状態を保存する機能を追加
    早送り・巻き戻しの秒数をオプションに追加
    SoundTouchのパラメータをオプションに追加
    停止したときの挙動をWPAK32と同じにした
    開こうとしたファイルが存在しなかった場合エラーが出ていたのを修正
    一時停止中にファイルを開くと一時停止状態のまま再生されるのを修正
    その他細かい修正

　v0.2.2　　2021/2/5
    速度変更のパラメータをスロー再生向けに変更
    ピッチとテンポのボタン配置を逆に
    シークバー右クリックで終了位置を設定できるようにした

　v0.2.1　　2021/1/31
    一時停止中にシークしたとき即座に再生を開始するように変更

　v0.2.0　　2021/1/31
    現在位置を開始・終了位置にするボタンを追加
    ズームボタンをトグル動作にしてズーム解除ボタンを削除
    ウィンドウ位置を保存するようにした
    ショートカットキーとキーカスタマイズを追加
    設定ファイルの形式を変更（v0.1.0の設定ファイルは読み込めなくなりました）
    その他細かい修正

　v0.1.0　　2021/1/26
    公開
