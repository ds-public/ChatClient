◆アセットインポーター(アセットがインポートされる時に自動で行う処理)の追加方法について


重要:

　AssetPostprocessor クラスを継承したアセットインポーターを作成する事は、
　原則禁止とします。

　理由は、クラスを追加直後にアセットのフルインポートが走ってしまうためです。


-----------------------------------------------------------------------------------------------
◆アセットインポーターの追加方法


1)
　/Assets/Application/AssetSettings/Editor フォルダ下に、

　目的に応じた名前のフォルダを作成してください。

　アセットインポーターのソースファイルは、
　この作成したフォルダ内に格納してください。

　例
　　/Assets/Application/AssetSettings/Editor
　　　/SpineSprites


2)
　作成するアセットインポータークラスは、

　　namespace を Beast.AssetSettings とし、

　　ImportProcessor クラスを継承してください。

　例
　　namespace Template.AssetSettings
　　{
　　　　public class SpineSpritesImportSettings : ImportProcessor

　　　　※クラス自体の static は禁止です。
　　　　　( NG: public static class )


3)
　ImportProcessor クラスには、
　
　各種のインポート時のコールバックメソッドが用意されていますので、

　override で継承して、処理を記述してください。

　例
　　
	/// <summary>
	/// テクスチャがインポートされる前呼び出される
	/// </summary>
	/// <param name="assetPostprocessor"></param>
	/// <returns></returns>
	public override void OnPreprocessTexture( AssetPostprocessor assetPostprocessor )
	{
		// ここにインポートされたファイルの設定を変更する処理を記述する

		TextureImporter textureImporter = assetPostprocessor.assetImporter as TextureImporter ;
	}

　※
　　どのような種類のコールバックがあるかは、

　　/Assets/Application/AssetSettings/Editor/ImportProcessor.cs を参照してください。


4)
　/Assets/Application/AssetSettings/Editor/RootAssetPostprocessoer.cs が、

　アセットのインポートを直接受け取るクラスになっており、

　このソースファイルを開き、最初にある

　private static readonly List<ImportProcessor> m_ImportProcessors = new List<ImportProcessor>()
　{
　}

　この記述の中に、
　作成したアセットインポータークラスのインスタンスを追加してください。

　例
　　
　　private static readonly List<ImportProcessor> m_ImportProcessors = new List<ImportProcessor>()
　　{
　　　　new SpineSpritesImportSettings()    // 追加したアセットインポーター
　　}



以上で、アセットインポーターの追加方法についての説明は終わりです。


