using System ;
using System.Collections ;
using System.Collections.Generic ;

using Cysharp.Threading.Tasks ;

using UnityEngine ;

using uGUIHelper ;
using System.Linq ;

using AudioHelper ;
using AssetBundleHelper ;
using StorageHelper ;

using TimeHelper ;

using Template.Screens.DownloadingClasses.UI ;

namespace Template.Screens
{
	/// <summary>
	/// 起動直後のダウンロード処理(マスターデータ・アセットバンドルの更新)
	/// </summary>
	public partial class Downloading
	{
		/// <summary>
		/// アセットバンドル全体共通の設定を行う
		/// </summary>
		public static void SetupGeneralAssetBundleSettings()
		{
			// アセットバンドルのマニフェストをロードする
			if( AssetBundleManager.Instance == null )
			{
				return ;
			}

			// 設定情報を取得する
			Settings settings = ApplicationManager.LoadSettings() ;

			//----------------------------------

			// 保存フォルダ名を設定する
			AssetBundleManager.DataPath = "AssetCache" ;

			// パスを難読化する設定
			AssetBundleManager.SecurityEnabled = Define.SecurityEnabled ;
#if UNITY_EDITOR
			if( AssetBundleManager.SecurityEnabled == true )
			{
				Debug.Log( "<color=#FF7F00>アセットバンドルのセキュリティを有効化します</color>" ) ;
			}
#endif
			// ストレージへの書き込みモードを設定する(非同期有効)
			AssetBundleManager.AsynchronousWritingEnabled = true ;

			// ログ出力モードを設定する
#if UNITY_EDITOR
			AssetBundleManager.LogEnabled = true ;
#else
			AssetBundleManager.LogEnabled = false ;

#endif
			//----------------------------------

			// 一部同期化で高速化読み出し
			AssetBundleManager.FastLoadEnabled = false ;

			//----------------------------------

#if UNITY_EDITOR
			if( settings.UseLocalAssets == true && ApplicationManager.HasLocalAssets == true )
			{
				// 使用する
				AssetBundleManager.UseLocalAssets	= true ;
			}
			else
			{
				// 使用しない
				AssetBundleManager.UseLocalAssets	= false ;
			}
#else
			// 実機環境は強制的に使用しない
			AssetBundleManager.UseLocalAssets		= false ;						// ローカルのアセットを使用するかどうか
#endif

			//----------------------------------

			// デフォルトマニフェスト名を登録する
			AssetBundleManager.DefaultManifestName = "Default" ;
		}


		/// <summary>
		/// 内部アセットバンドルのマニフェストの設定を行う
		/// </summary>
		/// <param name="owner"></param>
		/// <returns></returns>
		public static async UniTask SetupInternalAssetBundleSettings( ExMonoBehaviour owner )
		{
			// アセットバンドルのマニフェストをロードする
			if( AssetBundleManager.Instance == null )
			{
				return ;
			}

			//------------------------------------------------------------------------------------------

			string manifestName ;
			string platformName ;

			// 後で強制的に指定できるようにするかもしれない
			string	languageCodeName = Define.LanguageCodeName ;

			string	streamingAssetsRootPath ;
			bool	streamingAssetsDirectAccessEnabled ;
			string	remoteAssetBundleRootPath ;
			string	localAssetBundleRootPath ;
			string	localAssetsRootPath ;
			AssetBundleManager.LocationTypes locationType ;

			long	cacheSize ;

			//------------------------------------------------------------------------------------------

			// 設定情報を取得する
			Settings settings = ApplicationManager.LoadSettings() ;

			//------------------------------------------------------------------------------------------

			// Local - Internal

			// アセットバンドルのプラットフォーム名を設定する
//			platformName = "Common" ;
			platformName = Define.PlatformName ;

			// マニフェスト名
//			manifestName = "Internal" ;
			manifestName = "Default" ;

			//----------------------------------------------------------
			// StreamingAssets - Internal

			// 実際はマスターの通信が完了してからそちらから取得する

			// StreamingAssetsのルートパス
			streamingAssetsRootPath = "Template/" + platformName + "/" + manifestName ;

#if UNITY_EDITOR
			// エラーデバッグ用(基本は常に使用する)
			if( settings.UseStreamingAssets == false )
			{
				streamingAssetsRootPath = string.Empty ;	// 使用しない
			}
#endif
			//---------------------------------
			// StreamingAssetsDirectAccessEnabled - Internal

#if !UNITY_EDITOR && UNITY_ANDROID
			streamingAssetsDirectAccessEnabled = false ;
#else
			streamingAssetsDirectAccessEnabled = true ;
#endif
			//---------------------------------
			// RemoteAssetBundle - Internal

			remoteAssetBundleRootPath	= string.Empty ;	// 使用しない

			//---------------------------------
			// LocalAssetBundle - Internal

#if UNITY_EDITOR
//			if( settings.UseLocalAssetBundle == true && ApplicationManager.HasLocalAssetBundle == true )
//			{
//				// 使用する
//				localAssetBundleRootPath = "AssetBundle/" + platformName + "/" + manifestName ;
//			}
//			else
//			{
				// 使用しない
				localAssetBundleRootPath = string.Empty ;
//			}
#else
			// 実機環境は強制的に使用しない
			localAssetBundleRootPath = string.Empty ;
#endif
			//---------------------------------
			// LocalAssetsRootPath - Internal

			localAssetsRootPath = "/Assets/Application/AssetBundle/" ;

			//----------------------------------
			// LocationType - Internal

			locationType = AssetBundleManager.LocationTypes.StreamingAssets ;

			//---------------------------------------------

//			Debug.Log( "----- Local -----" ) ;
//			Debug.Log( "[AssetBundlePlatformName  ] " + assetBundlePlatformName ) ;
//			Debug.Log( "[LocalAssetBundleRootPath ] " + localAssetBundleRootPath ) ;
//			Debug.Log( "[StreamingAssetsRootPath  ] " + streamingAssetsRootPath ) ;
//			Debug.Log( "[RemoteAssetBundleRootPath] " + remoteAssetBundleRootPath ) ;

			// キャッシュサイズ
			cacheSize = 64 * 1024 * 1024 ;

			// マニフェストを登録 - Internal
			AssetBundleManager.AddManifest
			(
				manifestName,						// マニフェストファイル名(拡張子込み)
				false,								// CRCファィルのみ
				platformName,						// ストレージキャッシュのルートパス
				streamingAssetsRootPath, 			// StreamingAssetsのルートパス
				streamingAssetsDirectAccessEnabled,	// StreamingAssetsへのダイレクトアクセスを許可するかどうか
				remoteAssetBundleRootPath,			// Remoteのアセットバンドルのルートパス
				localAssetBundleRootPath,			// localのアセットバンドルのルートパス
				localAssetsRootPath,				// localアセットのルートパス
				locationType,						// 外部アセットバンドルをどこから取得するか
				cacheSize,							// キャッシュサイズ
				true								// 一部同期ロードを行いロードを高速化する
			) ;

			if( AssetBundleManager.UseLocalAssets == false )
			{
				// マニフェストのダウンロードを実行する
				await AssetBundleManager.LoadManifestAsync( manifestName ) ;

				// Manifest がダウンロードされるのを待つ
				while( AssetBundleManager.IsManifestCompleted( manifestName ) == false )
				{
					if( AssetBundleManager.GetAnyManifestError( out string errorManifestName, out string errorManifestMessage ) == true )
					{
						Debug.LogWarning( "マニフェストのロードでエラーが発生しました:" + errorManifestName + " -> " + errorManifestMessage );
						break ;
					}

					await owner.Yield() ;
				}

				Debug.Log( "<color=#FFFF00>マニフェスト[" + manifestName + "]のキャッシュサイズを" + ExString.GetSizeName( cacheSize ) + "に設定しました</color>" ) ;

				if( IsForceClear() == true )
				{
					// 強制的にクリアする
					AssetBundleManager.Cleanup( manifestName ) ;

					string message = "<color=#FF7F00>マニフェスト[" + manifestName + "]のキャッシュを消去しました</color>" ;
					Debug.Log( message ) ;
				}
			}
		}

		/// <summary>
		/// 内部アセットバンドルのマニフェストの設定を行う
		/// </summary>
		/// <param name="owner"></param>
		/// <returns></returns>
		public static async UniTask SetupExternalAssetBundleSettings( ExMonoBehaviour owner )
		{
			// アセットバンドルのマニフェストをロードする
			if( AssetBundleManager.Instance == null )
			{
				return ;
			}

			//------------------------------------------------------------------------------------------

			string debugRemoteAssetBundlePath = GetDebugRemotoAssetBundlePath() ;
#if UNITY_EDITOR
			if( string.IsNullOrEmpty( debugRemoteAssetBundlePath ) == false )
			{
				Debug.Log( "[注意]リモートアセットバンドルのパスを強制指定する:\n" + debugRemoteAssetBundlePath ) ;
			}
#endif
			//------------------------------------------------------------------------------------------

			string manifestName ;
			string platformName ;

			// 後で強制的に指定できるようにするかもしれない
			string	languageCodeName = Define.LanguageCodeName ;

			string	streamingAssetsRootPath ;
			bool	streamingAssetsDirectAccessEnabled ;
			string	remoteAssetBundleRootPath ;
			string	localAssetBundleRootPath ;
			string	localAssetsRootPath ;
			AssetBundleManager.LocationTypes	locationType ;

			long	cacheSize ;

			//------------------------------------------------------------------------------------------

			// 設定情報を取得する
			Settings settings = ApplicationManager.LoadSettings() ;

			//------------------------------------------------------------------------------------------
			// Remote - Default

			// マニフェスト名
			manifestName = "Default" ;

			// アセットバンドルのプラットフォーム名を取得する
			platformName = Define.PlatformName ;

#if UNITY_EDITOR
			if( settings.AssetBundlePlatformType != Settings.AssetBundlePlatformTypes.BuildPlatform )
			{
				// 強制的にアセットバンドルのプラットフォームを変更する
				platformName = settings.AssetBundlePlatformType.ToString() ;
			}
#endif
			//----------------------------------------------------------
			// StreamingAssets - Default

			streamingAssetsRootPath	= "Template/" + platformName + "/" + manifestName ;

			//----------------------------------------------------------
			// StreamingAssetsDirectAccessEnabled - Default

#if !UNITY_EDITOR && UNITY_ANDROID
			streamingAssetsDirectAccessEnabled = false ;
#else
			streamingAssetsDirectAccessEnabled = true ;
#endif
			Debug.Log( "<color=#FFDFBF>====>StreamingAssets へのダイレクトアクセス = " + streamingAssetsDirectAccessEnabled + "</color>" ) ;

			//----------------------------------------------------------
			// RemoteAssetBundle - Default

			if( settings.UseRemoteAssetBundle == true )
			{
				// 正しくは CheckVersion で取得したパスを使用する
				remoteAssetBundleRootPath	= "https://localhost/Template" ;	// Remoteのアセットバンドルのルートパス
//				remoteAssetBundleRootPath = PlayerData.AssetBundlePath ;			// CheckVersion で所得した AssetBundlePath
				if( string.IsNullOrEmpty( remoteAssetBundleRootPath ) == false && remoteAssetBundleRootPath.Length >  0 )
				{
					// 最後のスラッシュは削る
					remoteAssetBundleRootPath = remoteAssetBundleRootPath.TrimEnd( '/' ) ;
				}

				remoteAssetBundleRootPath = remoteAssetBundleRootPath + "/" + languageCodeName + "/" + platformName + "/" + manifestName ;

				// リモートアセットバンドルパスが強制変更されていたらそちらを使用する
				if( string.IsNullOrEmpty( debugRemoteAssetBundlePath ) == false )
				{
					remoteAssetBundleRootPath = debugRemoteAssetBundlePath ;	// settings で強制指定されている場合はそれを使用する
					if( string.IsNullOrEmpty( remoteAssetBundleRootPath ) == false && remoteAssetBundleRootPath.Length >  0 )
					{
						// 最後のスラッシュは削る
						remoteAssetBundleRootPath = remoteAssetBundleRootPath.TrimEnd( '/' ) ;
					}

					remoteAssetBundleRootPath += "/" + manifestName ;
				}
			}
			else
			{
				remoteAssetBundleRootPath = string.Empty ;
			}

			//----------------------------------------------------------
			// LocalAssetBundle - Default

#if UNITY_EDITOR
			if( settings.UseLocalAssetBundle == true && ApplicationManager.HasLocalAssetBundle == true )
			{
				// 使用する
				localAssetBundleRootPath = "AssetBundle/" + platformName + "/" + manifestName ;
			}
			else
			{
				// 使用しない
				localAssetBundleRootPath = string.Empty ;
			}
#else
			// 実機環境は強制的に使用しない
			localAssetBundleRootPath = string.Empty ;
#endif
			//---------------------------------
			// LocalAssetsRootPath - Default

			localAssetsRootPath = "/Assets/Application/AssetBundle/" ;

			//----------------------------------
			// LocationType - Default

			Debug.Log( "<color=#FFDFBF>====>StreamingAssets の固定アセットバンドルの保持状況を確認する</color>" ) ;

			bool hasConstantAssetBundle = false ;
			await owner.When( ApplicationManager.HasAssetBundleInStreamingAssets( streamingAssetsRootPath + "/" + manifestName + ".txt", ( bool result ) =>
			{
				hasConstantAssetBundle = result ;
			} ) ) ;

			Debug.Log( "<color=#FFDFBF><====StreamingAssets の固定アセットバンドルの保持状況 : " + hasConstantAssetBundle + "</color>" ) ;


			if( hasConstantAssetBundle == false )
			{
				// StreamingAssets に固定アセットバンドルを保持していない
				locationType = AssetBundleManager.LocationTypes.Storage ;
			}
			else
			if( string.IsNullOrEmpty( remoteAssetBundleRootPath ) == false )
			{
				// StreamingAssets に固定アセットバンドルを保持している
				locationType = AssetBundleManager.LocationTypes.StorageAndStreamingAssets ;

				// iOS の場合の対象は、チュートリアル中は StreamingAssets 限定で、チュートリアル後に Storage も解禁される
#if !UNITY_EDITOR && UNITY_IOS
				if( ignorePlayerData == false )
				{
					// ただしプレイヤーデータを参照する(正規のゲーム動作である)場合に限る
					if( Tutorial.IsCompleted == false )
					{
						locationType = AssetBundleManager.LocationTypes.StreamingAssets ;
					}
				}
#endif
			}
			else
			{
				locationType = AssetBundleManager.LocationTypes.StreamingAssets ;
			}

//			locationType = AssetBundleManager.LocationTypes.StorageAndStreamingAssets ;
//			locationType = AssetBundleManager.LocationTypes.StreamingAssets ;
//			locationType = AssetBundleManager.LocationTypes.Storage ;

			//----------------------------------------------------------

//			Debug.Log( "----- Remote -----" ) ;
//			Debug.Log( "[AssetBundlePlatformName  ] " + assetBundlePlatformName ) ;
//			Debug.Log( "[LocalAssetBundleRootPath ] " + localAssetBundleRootPath ) ;
//			Debug.Log( "[StreamingAssetsRootPath  ] " + streamingAssetsRootPath ) ;
			Debug.Log( "[RemoteAssetBundleRootPath] " + remoteAssetBundleRootPath ) ;

			// キャッシュサイズ
			cacheSize = 3L * 1024 * 1024 * 1024 ;

			// マニフェストを登録 - Default
			AssetBundleManager.AddManifest
			(
				manifestName,						// マニフェストファイル名(拡張子込み)
				false,								// 通常のマニフェストファイル
				platformName,						// ストレージキャッシュのルートパス
				streamingAssetsRootPath, 			// StreamingAssetsのルートパス
				streamingAssetsDirectAccessEnabled,	// StreamingAssetsにダイレクトアクセスを許可するかどうか
				remoteAssetBundleRootPath,			// Remoteのアセットバンドルのルートパス
				localAssetBundleRootPath,			// localのアセットバンドルのルートパス
				localAssetsRootPath,				// localアセットのルートパス
				locationType,						// 外部アセットバンドルをどこから取得するか
				cacheSize,							// キャッシュサイズ
				true								// 一部同期ロードを行いロードを高速化する
			) ;

			if( AssetBundleManager.UseLocalAssets == false )
			{
				// マニフェストのダウンロードを実行する
				await AssetBundleManager.LoadManifestAsync( manifestName ) ;

				// Manifest がダウンロードされるのを待つ
				while( AssetBundleManager.IsManifestCompleted( manifestName ) == false )
				{
					if( AssetBundleManager.GetAnyManifestError( out string errorManifestName, out string errorManifestMessage ) == true )
					{
						Debug.LogWarning( "マニフェストのロードでエラーが発生しました:" + errorManifestName + " -> " + errorManifestMessage );
						break ;
					}

					await owner.Yield() ;
				}

				Debug.Log( "<color=#FFFF00>マニフェスト[" + manifestName + "]のキャッシュサイズを" + ExString.GetSizeName( cacheSize ) + "に設定しました</color>" ) ;
			}
		}

	}
}
