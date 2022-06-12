using System ;
using System.Text ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using StorageHelper ;

using UnityEngine ;

/// <summary>
/// アセットバンドルヘルパーパッケージ
/// </summary>
namespace AssetBundleHelper
{
	/// <summary>
	/// アセットバンドルマネージャクラス(メソッド)
	/// </summary>
	public partial class AssetBundleManager
	{
		/// <summary>
		/// マニフェスト情報クラス
		/// </summary>
		public partial class ManifestInfo
		{

			// ManifestInfo :: AllAssetPaths
						
			/// <summary>
			/// アセットバンドルに含まれる全てのアセットのパスを取得する(同期版)　※直接呼び出し非推奨
			/// </summary>
			/// <param name="assetBundlePath">アセットバンドルのパス</param>
			/// <param name="assetBundleCaching">アセットバンドルのキャッシングタイプ</param>
			/// <param name="instance">アセットバンドルマネージャのインスタンス</param>
			/// <returns>アセットバンドルに含まれる全てのアセットのパスとクラス型</returns>
			internal protected ( string, Type )[] GetAllAssetPaths( string assetBundlePath, bool isCaching, bool isRetain, AssetBundleManager instance )
			{
				AssetBundle assetBundle = LoadAssetBundle( assetBundlePath, isCaching, isRetain, instance ) ;
				if( assetBundle == null )
				{
					return null ;	// 失敗
				}

				//---------------------------------------------------------

				// アセットのロード
				( string, Type )[] allAssetPaths = m_AssetBundleHash[ assetBundlePath ].GetAllAssetPaths( assetBundle, assetBundlePath, LocalAssetsRootPath, instance ) ;

				return allAssetPaths ;
			}
			
			/// <summary>
			/// アセットバンドルに含まれる全てのアセットのパスを取得する(非同期版)　※直接呼び出し非推奨
			/// </summary>
			/// <param name="assetBundlePath">アセットバンドルのパス</param>
			/// <param name="onLoaded">ロード成功時のコールバック</param>
			/// <param name="keep">アセットバンドルを永続保存するかどうか</param>
			/// <param name="onError">ロード失敗時のコールバック</param>
			/// <param name="request">非同期待ちオブジェクト</param>
			/// <param name="assetBundleCaching">アセットバンドルのキャッシングタイプ</param>
			/// <param name="instance">アセットバンドルマネージャのインスタンス</param>
			/// <returns>列挙子</returns>
			internal protected IEnumerator GetAllAssetPaths_Coroutine( string assetBundlePath, bool isCaching, bool isRetain, bool keep, Action<( string, Type )[]> onLoaded, Action<string> onError, Request request, AssetBundleManager instance )
			{
				AssetBundle assetBundle = null ;
				
				// 必ず非同期ダウンロードを試みる(依存関係にあるアセットバンドルのロードも行う必要があるため)
				yield return instance.StartCoroutine( LoadAssetBundle_Coroutine( assetBundlePath, isCaching, isRetain, keep, ( _ ) => { assetBundle = _ ; }, onError, request, instance ) ) ;
				if( assetBundle == null )
				{
					yield break ;	// アセットバンドルが展開出来ない
				}

				//---------------------------------------------------------

				// アセットバンドルインフォを取得する
				AssetBundleInfo assetBundleInfo = m_AssetBundleHash[ assetBundlePath ] ;

				// 実際のパスを取得する
				( string, Type )[] allAssetPaths = assetBundleInfo.GetAllAssetPaths( assetBundle, assetBundlePath, LocalAssetsRootPath, instance ) ;
				
				if( allAssetPaths != null )
				{
					onLoaded?.Invoke( allAssetPaths ) ;
				}
				else
				{
					onError?.Invoke( "Could not load ." ) ;
				}
			}

			//----------------------------------

			/// <summary>
			/// アセットバンドルに含まれる全てのアセットのパスを取得する(同期版)　※直接呼び出し非推奨
			/// </summary>
			/// <param name="assetBundlePath">アセットバンドルのパス</param>
			/// <param name="assetBundleCaching">アセットバンドルのキャッシングタイプ</param>
			/// <param name="instance">アセットバンドルマネージャのインスタンス</param>
			/// <returns>アセットバンドルに含まれる全てのアセットのパスとクラス型</returns>
			internal protected string[] GetAllAssetPaths( string assetBundlePath, Type type, bool isCaching, bool isRetain, AssetBundleManager instance )
			{
				AssetBundle assetBundle = LoadAssetBundle( assetBundlePath, isCaching, isRetain, instance ) ;
				if( assetBundle == null )
				{
					return null ;	// 失敗
				}

				//---------------------------------------------------------

				// アセットのロード
				string[] allAssetPaths = m_AssetBundleHash[ assetBundlePath ].GetAllAssetPaths( assetBundle, assetBundlePath, type, LocalAssetsRootPath, instance ) ;

				return allAssetPaths ;
			}
			
			/// <summary>
			/// アセットバンドルに含まれる全てのアセットのパスを取得する(非同期版)　※直接呼び出し非推奨
			/// </summary>
			/// <param name="assetBundlePath">アセットバンドルのパス</param>
			/// <param name="onLoaded">ロード成功時のコールバック</param>
			/// <param name="keep">アセットバンドルを永続保存するかどうか</param>
			/// <param name="onError">ロード失敗時のコールバック</param>
			/// <param name="request">非同期待ちオブジェクト</param>
			/// <param name="assetBundleCaching">アセットバンドルのキャッシングタイプ</param>
			/// <param name="instance">アセットバンドルマネージャのインスタンス</param>
			/// <returns>列挙子</returns>
			internal protected IEnumerator GetAllAssetPaths_Coroutine( string assetBundlePath, Type type, bool isCaching, bool isRetain, bool keep, Action<string[]> onLoaded, Action<string> onError, Request request, AssetBundleManager instance )
			{
				AssetBundle assetBundle = null ;
				
				// 必ず非同期ダウンロードを試みる(依存関係にあるアセットバンドルのロードも行う必要があるため)
				yield return instance.StartCoroutine( LoadAssetBundle_Coroutine( assetBundlePath, isCaching, isRetain, keep, ( _ ) => { assetBundle = _ ; }, onError, request, instance ) ) ;
				if( assetBundle == null )
				{
					yield break ;	// アセットバンドルが展開出来ない
				}

				//---------------------------------------------------------

				// アセットバンドルインフォを取得する
				AssetBundleInfo assetBundleInfo = m_AssetBundleHash[ assetBundlePath ] ;

				// 実際のパスを取得する
				string[] allAssetPaths = assetBundleInfo.GetAllAssetPaths( assetBundle, assetBundlePath, type, LocalAssetsRootPath, instance ) ;
				
				if( allAssetPaths != null )
				{
					onLoaded?.Invoke( allAssetPaths ) ;
				}
				else
				{
					onError?.Invoke( "Could not load ." ) ;
				}
			}

			//---------------------------------------------------------

			// ManifestInfo :: Asset
						
			/// <summary>
			/// アセットを取得する(同期版)　※直接呼び出し非推奨
			/// </summary>
			/// <param name="assetBundlePath">アセットバンドルのパス</param>
			/// <param name="assetName">アセット名</param>
			/// <param name="type">アセットのクラス型</param>
			/// <param name="assetBundleCaching">アセットバンドルのキャッシングタイプ</param>
			/// <param name="instance">アセットバンドルマネージャのインスタンス</param>
			/// <returns></returns>
			internal protected UnityEngine.Object LoadAsset( string assetBundlePath, string assetName, Type type, bool isCaching, bool isRetain, AssetBundleManager instance )
			{
				AssetBundle assetBundle = LoadAssetBundle( assetBundlePath, isCaching, isRetain, instance ) ;
				if( assetBundle == null )
				{
					return null ;	// 失敗
				}

				//---------------------------------------------------------

				// アセットのロード
				UnityEngine.Object asset = m_AssetBundleHash[ assetBundlePath ].LoadAsset( assetBundle, assetBundlePath, assetName, type, LocalAssetsRootPath, instance ) ;

				return asset ;
			}
			
			/// <summary>
			/// アセットを取得する(非同期版)　※直接呼び出し非推奨
			/// </summary>
			/// <param name="assetBundlePath">アセットバンドルのパス</param>
			/// <param name="assetName">アセット名</param>
			/// <param name="type">アセットのクラス型</param>
			/// <param name="onLoaded">ロード成功時のコールバック</param>
			/// <param name="keep">アセットバンドルを永続保存するかどうか</param>
			/// <param name="onError">ロード失敗時のコールバック</param>
			/// <param name="request">非同期待ちオブジェクト</param>
			/// <param name="assetBundleCaching">アセットバンドルのキャッシングタイプ</param>
			/// <param name="instance">アセットバンドルマネージャのインスタンス</param>
			/// <returns></returns>
			internal protected IEnumerator LoadAsset_Coroutine( string assetBundlePath, string assetName, Type type, bool isCaching, bool isRetain, bool keep, Action<UnityEngine.Object> onLoaded, Action<string> onError, Request request, AssetBundleManager instance )
			{
				AssetBundle assetBundle = null ;
				
				// 必ず非同期ダウンロードを試みる(依存関係にあるアセットバンドルのロードも行う必要があるため)
				yield return instance.StartCoroutine( LoadAssetBundle_Coroutine( assetBundlePath, isCaching, isRetain, keep, ( _ ) => { assetBundle = _ ; }, onError, request, instance ) ) ;
				if( assetBundle == null )
				{
					yield break ;	// アセットバンドルが展開出来ない
				}

				//---------------------------------------------------------

				// アセットバンドルインフォを取得する
				AssetBundleInfo assetBundleInfo = m_AssetBundleHash[ assetBundlePath ] ;

				UnityEngine.Object asset ;

				// アセットバンドルが読み出せた(あと一息)
//				if( instance.m_FastLoadEnabled == false || fastLoadEnabled == false )
//				{
//					// 非同期(現状使用できない)
//					Debug.LogWarning( "非同期" ) ;
//					yield return tInstance.StartCoroutine( tAssetBundleInfo.LoadAsset_Coroutine( tAssetBundle, tAssetBundleName, tAssetName, tType, rAssetHolder, tRequest, tInstance ) ) ;
//				}
//				else
//				{
					// 同期
					asset = assetBundleInfo.LoadAsset( assetBundle, assetBundlePath, assetName, type, LocalAssetsRootPath, instance ) ;
//				}
				
				if( asset != null )
				{
					onLoaded?.Invoke( asset ) ;
				}
				else
				{
					onError?.Invoke( "Could not load ." ) ;
				}
			}

			//-----------------------

			// ManifestInfo :: AllAssets

			/// <summary>
			/// 全てのアセットを取得する(同期版)　※呼び出し非推奨
			/// </summary>
			/// <param name="assetBundlePath">アセットバンドルのパス</param>
			/// <param name="type">アセットのクラス型</param>
			/// <param name="assetBundleCaching">アセットバンドルのキャッシングタイプ</param>
			/// <param name="instance">アセットバンドルマネージャのインスタンス</param>
			/// <param name="resourcePath">リソースパス</param>
			/// <returns>全てのアセットに含まれる任意のコンポーネントのインスタンス</returns>
			internal protected UnityEngine.Object[] LoadAllAssets( string assetBundlePath, Type type, bool isCaching, bool isRetain, string localAssetPath, AssetBundleManager instance )
			{
				AssetBundle assetBundle = LoadAssetBundle( assetBundlePath, isCaching, isRetain, instance ) ;
				if( assetBundle == null )
				{
					return null ;	// 失敗
				}

				//---------------------------------------------------------

				// アセットのロード
				UnityEngine.Object[] assets = m_AssetBundleHash[ assetBundlePath ].LoadAllAssets( assetBundle, type, LocalAssetsRootPath, localAssetPath, instance ) ;

				return assets ;
			}

			/// <summary>
			/// 全てのアセットを取得する(非同期版)　※呼び出し非推奨
			/// </summary>
			/// <param name="assetBundlePath">アセットバンドルのパス</param>
			/// <param name="type">アセットのクラス型</param>
			/// <param name="onLoaded">ロード成功時のコールバック</param>
			/// <param name="keep">アセットバンドルを永続保存するかどうか</param>
			/// <param name="onError">ロード失敗時のコールバック</param>
			/// <param name="request">非同期待ちオブジェクト</param>
			/// <param name="assetBundleCaching">アセットバンドルのキャッシングタイプ</param>
			/// <param name="instance">アセットバンドルマネージャのインスタンス</param>
			/// <param name="resourcePath">リソースパス</param>
			/// <returns>列挙子</returns>
			internal protected IEnumerator LoadAllAssets_Coroutine( string assetBundlePath, Type type, bool isCaching, bool isRetain, bool keep, Action<UnityEngine.Object[]> onLoaded, Action<string> onError, Request request, string localAssetPath, AssetBundleManager instance )
			{
				AssetBundle assetBundle = null ;
				
				// 必ず非同期ダウンロードを試みる(依存関係にあるアセットバンドルのロードも行う必要があるため)
				yield return instance.StartCoroutine( LoadAssetBundle_Coroutine( assetBundlePath, isCaching, isRetain, keep, ( _ ) => { assetBundle = _ ; }, onError, request, instance ) ) ;
				if( assetBundle == null )
				{
					yield break ;	// アセットバンドルが展開出来ない
				}

				//---------------------------------------------------------

				// アセットバンドルインフォを取得する
				AssetBundleInfo assetBundleInfo = m_AssetBundleHash[ assetBundlePath ] ;

				// アセットバンドルが読み出せた(あと一息)
				UnityEngine.Object[] assets = null ;

//				if( instance.m_FastLoadEnabled == false || fastLoadEnabled == false )
//				{
//					// 非同期
//					List<UnityEngine.Object>[] rAllSubAssetsHolder = { null } ;
//					yield return tInstance.StartCoroutine( tAssetBundleInfo.LoadAllSubAssets_Coroutine( tAssetBundle, tAssetBundleName, tAssetName, tType, rAllSubAssetsHolder, tRequest, tInstance, tResourcePath ) ) ;
//					allSubAssets = rAllSubAssetsHolder[ 0 ] ;
//				}
//				else
//				{
					// 同期
					assets = assetBundleInfo.LoadAllAssets( assetBundle, type, LocalAssetsRootPath, localAssetPath, instance ) ;
//				}

				if( assets != null && assets.Length >  0 )
				{
					onLoaded?.Invoke( assets ) ;
				}
				else
				{
					onError?.Invoke( "Could not load ." ) ;
				}
			}

			//-----------------------

			// ManifestInfo :: SubAsset

			/// <summary>
			/// アセットに含まれるサブアセットを取得する(同期版)　※呼び出し非推奨
			/// </summary>
			/// <param name="assetBundlePath">アセットバンドルのパス</param>
			/// <param name="assetName">アセット名</param>
			/// <param name="subAssetName">サブアセット名</param>
			/// <param name="type">サブアセットのクラス型</param>
			/// <param name="assetBundleCaching">アセットバンドルのキャッシングタイプ</param>
			/// <param name="instance">アセットバンドルマネージャのインスタンス</param>
			/// <param name="resourcePath">リソースパス</param>
			/// <returns>サブアセットに含まれる任意のコンポーネントのインスタンス</returns>
			internal protected UnityEngine.Object LoadSubAsset( string assetBundlePath, string assetName, string subAssetName, Type type, bool isCaching, bool isRetain,  string localAssetPath, AssetBundleManager instance )
			{
				AssetBundle assetBundle = LoadAssetBundle( assetBundlePath, isCaching, isRetain, instance ) ;
				if( assetBundle == null )
				{
					return null ;	// 失敗
				}

				//---------------------------------------------------------

				// アセットのロード
				UnityEngine.Object asset = m_AssetBundleHash[ assetBundlePath ].LoadSubAsset( assetBundle, assetBundlePath, assetName, subAssetName, type, LocalAssetsRootPath, localAssetPath, instance ) ;

				return asset ;
			}

			/// <summary>
			/// アセットに含まれるサブアセットを取得する(非同期版)　※呼び出し非推奨
			/// </summary>
			/// <param name="assetBundlePath">アセットバンドルのパス</param>
			/// <param name="assetName">アセット名</param>
			/// <param name="subAssetName">サブアセット名</param>
			/// <param name="type">サブアセットのクラス型</param>
			/// <param name="onLoaded">ロード成功時のコールバック</param>
			/// <param name="keep">アセットバンドルを永続保持するかどうか</param>
			/// <param name="onError">ロード失敗時のコールバック</param>
			/// <param name="request">非同期待ちオブジェクト</param>
			/// <param name="assetBundleCaching">アセットバンドルのキャッシングタイプ</param>
			/// <param name="instance">アセットバンドルマネージャのインスタンス</param>
			/// <param name="resourcePath">リソースパス</param>
			/// <returns></returns>
			internal protected IEnumerator LoadSubAsset_Coroutine( string assetBundlePath, string assetName, string subAssetName, Type type, bool isCaching, bool isRetain, bool keep, Action<UnityEngine.Object> onLoaded, Action<string> onError, Request request, string localAssetPath, AssetBundleManager instance )
			{
				AssetBundle assetBundle = null ;
				
				// 必ず非同期ダウンロードを試みる(依存関係にあるアセットバンドルのロードも行う必要があるため)
				yield return instance.StartCoroutine( LoadAssetBundle_Coroutine( assetBundlePath, isCaching, isRetain, keep, ( _ ) => { assetBundle = _ ; }, onError, request, instance ) ) ;
				if( assetBundle == null )
				{
					yield break ;	// アセットバンドルが展開出来ない
				}

				//---------------------------------------------------------

				// アセットバンドルインフォを取得する
				AssetBundleInfo assetBundleInfo = m_AssetBundleHash[ assetBundlePath ] ;

				UnityEngine.Object asset ;

				// アセットバンドルが読み出せた(あと一息)
//				if( instance.m_FastLoadEnabled == false || fastLoadEnabled == false )
//				{
//					// 非同期
//					yield return instance.StartCoroutine( assetBundleInfo.LoadSubAsset_Coroutine( assetBundle, assetBundleName, assetName, subAssetName, type, rSubAssetHolder, request, instance, resourcePath ) ) ;
//				}
//				else
//				{
					// 同期
					asset = assetBundleInfo.LoadSubAsset( assetBundle, assetBundlePath, assetName, subAssetName, type, LocalAssetsRootPath, localAssetPath, instance ) ;
//				}

				if( asset != null )
				{
					onLoaded?.Invoke( asset ) ;
				}
				else
				{
					onError?.Invoke( "Could not load ." ) ;
				}
			}

			//-----------------------

			// ManifestInfo :: AllSubAssets

			/// <summary>
			/// アセットに含まれる全てのサブアセットを取得する(同期版)　※呼び出し非推奨
			/// </summary>
			/// <param name="assetBundlePath">アセットバンドルのパス</param>
			/// <param name="assetName">アセット名</param>
			/// <param name="type">アセットのクラス型</param>
			/// <param name="assetBundleCaching">アセットバンドルのキャッシングタイプ</param>
			/// <param name="instance">アセットバンドルマネージャのインスタンス</param>
			/// <param name="resourcePath">アセットのリソースパス</param>
			/// <returns>全てのサブアセットに含まれる任意のコンポーネントのインスタンス配列</returns>
			internal protected UnityEngine.Object[] LoadAllSubAssets( string assetBundlePath, string assetName, Type type, bool isCaching, bool isRetain, string localAssetPath, AssetBundleManager instance )
			{
				AssetBundle assetBundle = LoadAssetBundle( assetBundlePath, isCaching, isRetain, instance ) ;
				if( assetBundle == null )
				{
					return null ;	// 失敗
				}

				//---------------------------------------------------------

				// アセットのロード
				UnityEngine.Object[] assets = m_AssetBundleHash[ assetBundlePath ].LoadAllSubAssets( assetBundle, assetBundlePath, assetName, type, LocalAssetsRootPath, localAssetPath, instance ) ;

				return assets ;
			}

			/// <summary>
			/// アセットに含まれる全てのサブアセットを取得する(非同期版)　※呼び出し非推奨
			/// </summary>
			/// <param name="assetBundlePath">アセットバンドルのパス</param>
			/// <param name="assetName">アセット名</param>
			/// <param name="type">アセットのクラスタイプ</param>
			/// <param name="onLoaded">読み込み完了時のコールバック</param>
			/// <param name="keep">アセットバンドルを永続保存するか</param>
			/// <param name="onError">読み込み失敗時のコールバック</param>
			/// <param name="request">非同期待ち制御</param>
			/// <param name="assetBundleCaching">アセットバンドルのキャッシングタイプ</param>
			/// <param name="instance">保持インスタンス</param>
			/// <param name="resourcePath">リソースパス</param>
			/// <returns></returns>
			internal protected IEnumerator LoadAllSubAssets_Coroutine( string assetBundlePath, string assetName, Type type, bool isCaching, bool isRetain, bool keep, Action<UnityEngine.Object[]> onLoaded, Action<string> onError, Request request, string localAssetPath, AssetBundleManager instance )
			{
				AssetBundle assetBundle = null ;
				
				// 必ず非同期ダウンロードを試みる(依存関係にあるアセットバンドルのロードも行う必要があるため)
				yield return instance.StartCoroutine( LoadAssetBundle_Coroutine( assetBundlePath, isCaching, isRetain, keep, ( _ ) => { assetBundle = _ ; }, onError, request, instance ) ) ;
				if( assetBundle == null )
				{
					yield break ;	// アセットバンドルが展開出来ない
				}

				//---------------------------------------------------------

				// アセットバンドルインフォを取得する
				AssetBundleInfo assetBundleInfo = m_AssetBundleHash[ assetBundlePath ] ;

				// アセットバンドルが読み出せた(あと一息)
				UnityEngine.Object[] assets = null ;

//				if( instance.m_FastLoadEnabled == false || fastLoadEnabled == false )
//				{
//					// 非同期
//					List<UnityEngine.Object>[] rAllSubAssetsHolder = { null } ;
//					yield return instance.StartCoroutine( assetBundleInfo.LoadAllSubAssets_Coroutine( assetBundle, assetBundleName, assetName, type, rAllSubAssetsHolder, tRequest, instance, resourcePath ) ) ;
//					allSubAssets = rAllSubAssetsHolder[ 0 ] ;
//				}
//				else
//				{
					// 同期
					assets = assetBundleInfo.LoadAllSubAssets( assetBundle, assetBundlePath, assetName, type, LocalAssetsRootPath, localAssetPath, instance ) ;
//				}

				if( assets != null && assets.Length >  0 )
				{
					onLoaded?.Invoke( assets ) ;
				}
				else
				{
					onError?.Invoke( "Could not load ." ) ;
				}
			}

			//--------------------------------------

			// ManifestInfo :: AssetBundle

			/// <summary>
			/// アセットバンドルを取得する(同期版)　※呼び出し非推奨
			/// </summary>
			/// <param name="assetBundlePath">アセットバンドルのパス</param>
			/// <param name="assetBundleCaching">アセットバンドルのキャッシングタイプ</param>
			/// <returns></returns>
			internal protected AssetBundle LoadAssetBundle( string assetBundlePath, bool isCaching, bool isRetain, AssetBundleManager instance )
			{
				// そのファイルが更新対象か確認する
				if( m_AssetBundleHash.ContainsKey( assetBundlePath ) == false )
				{
					// 指定の名前のアセットバンドルは存在しない
					return null ;
				}

				//------------------------------------------

				// 指定の名前のアセットバンドルインフォは存在する

				// アセットバンドルインフォを取得する
				AssetBundleInfo assetBundleInfo = m_AssetBundleHash[ assetBundlePath ] ;

				// このアセットバンドルが更新対象になっているか確認する
				if( assetBundleInfo.UpdateRequired == true )
				{
					// 更新対象になっているのでキャッシュから削除する
					RemoveAssetBundleCacheForced( assetBundlePath ) ;

					// 更新対象になっているので取得不可
					return null ;
				}
				
				//-------------------------------------------------------------

				// このアセットバンドルが依存している他のアセットバンドルの情報を取得する

				if( m_Manifest != null && isRetain == false )
				{
					// レガシータイプの場合はマニフェストが存在しないので null チェックはきちんと行う必要がある
					string[] dependentAssetBundlePaths = m_Manifest.GetAllDependencies( assetBundlePath ) ;
					if( dependentAssetBundlePaths != null && dependentAssetBundlePaths.Length >  0 )
					{
						// 依存するものが存在する
#if UNITY_EDITOR
						if( dependentAssetBundlePaths.Length >= TOO_MUCH_DEPENDENCE )
						{
							// 明らかに過量な一定数の依存ファイルが存在する場合に警告を出す
							string wm = "[AssetBundleManager] Too much dependence : " + assetBundlePath + " ( " + dependentAssetBundlePaths.Length + " )\n" ;
							int wi ;
							for( wi  = 0 ; wi <  dependentAssetBundlePaths.Length ; wi ++ )
							{
								wm += " + " + dependentAssetBundlePaths[ wi ] + "\n" ;
							}
							Debug.LogWarning( wm ) ;
						}
#endif
						AssetBundleInfo	dependentAssetBundleInfo ;
						AssetBundle		dependentAssetBundle ;

						foreach( var dependentAssetBundlePath in dependentAssetBundlePaths )
						{
							dependentAssetBundleInfo = m_AssetBundleHash[ dependentAssetBundlePath ] ;
							if( dependentAssetBundleInfo.UpdateRequired == true )
							{
#if UNITY_EDITOR
								Debug.LogWarning( "同期:依存するアセットバンドルが取得できない(要Update):" + dependentAssetBundlePath + " <- " + assetBundlePath ) ;
#endif
								// 更新対象であるため取得不可
								RemoveAssetBundleCacheForced( dependentAssetBundlePath ) ;

								// １つでも依存アセットバンドルが欠けていたら対象のアセットバンドルも取得出来ない(非同期で取得せよ)
								return null ;
							}
						}

						//-------------------------------
						// ここに来るという事は依存するアセットバンドルが全て取得可能であるという事を示す

						// 依存するアセットバンドルでキャッシュにためていないものはためていく
						foreach( var dependentAssetBundlePath in dependentAssetBundlePaths )
						{
//							Debug.LogWarning( "同期:依存するアセットバンドル名:" + dependentAssetBundlePath + " <- " + assetBundlePath ) ;

							dependentAssetBundleInfo = m_AssetBundleHash[ dependentAssetBundlePath ] ;

							// キャッシュに存在するか確認する
							if( CheckAssetBundleCache( dependentAssetBundlePath, isCaching ) == false )
							{
								// キャッシュに存在しない
								dependentAssetBundle = CreateAssetBundle( dependentAssetBundleInfo, instance ) ;
								if( dependentAssetBundle != null )
								{
									// キャッシュにためる
									AddAssetBundleCache( dependentAssetBundlePath, dependentAssetBundle, isCaching, isRetain ) ;
								}
								else
								{
#if UNITY_EDITOR
									Debug.LogWarning( "同期:依存するアセットバンドルが取得できない(IOエラー):" + dependentAssetBundlePath + " <- " + assetBundlePath ) ;
#endif
									// １つでも依存アセットバンドルが欠けていたら対象のアセットバンドルも取得出来ない(非同期で取得せよ)
									return null ;
								}
							}
						}
					}
				}

				//-------------------------------------------------------------
				
				// ここに来るということは既にローカルに最新のアセットバンドルが保存されている事を意味する
				AssetBundle assetBundle ;
				
				if( CheckAssetBundleCache( assetBundlePath, isCaching ) == false )
				{
					// キャッシュには存在しない

					// 保存されたアセットバンドルを同期で展開する
					assetBundle = CreateAssetBundle( assetBundleInfo, instance ) ;

					if( assetBundle != null )
					{
						// キャッシュにためる
						AddAssetBundleCache( assetBundlePath, assetBundle, isCaching, isRetain ) ;
					}
				}
				else
				{
					// 既にキャッシュに存在するのでそれを使用する
					assetBundle = m_AssetBundleCache[ assetBundlePath ].AssetBundle ;
				}

				// アセットバンドルのインスタンスを返す(予測不能のエラーとしてnullが返る可能性も0ではない)
				return assetBundle ;
			}

			/// <summary>
			/// アセットバンドルを取得する(非同期版)　※呼び出し非推奨
			/// </summary>
			/// <param name="assetBundleName">アセットバンドル名</param>
			/// <param name="onLoaded">アセットバンドルのインスタンスを格納するための要素数１以上の配列</param>
			/// <param name="keep">キャッシュオーバー時の動作(true=キャッシュオーバー時に保持する・false=キャッシュオーバー時に破棄する)</param>
			/// <param name="instance">アセットバンドルマネージャのインスタンス</param>
			/// <returns>列挙子</returns>
			internal protected IEnumerator LoadAssetBundle_Coroutine( string assetBundlePath, bool isCaching, bool isRetain, bool keep, Action<AssetBundle> onLoaded, Action<string> onResult, Request request, AssetBundleManager instance )
			{
				// そのファイルが更新対象か確認する
				if( m_AssetBundleHash.ContainsKey( assetBundlePath ) == false )
				{
					// 指定の名前のアセットバンドルは存在しない
					yield break ;
				}

				//------------------------------------------

				// 指定の名前のアセットバンドルインフォは存在する

				// アセットバンドルインフォを取得する
				AssetBundleInfo assetBundleInfo = m_AssetBundleHash[ assetBundlePath ] ;

				// 非同期で同じアセットバンドルにアクセスする場合は排他ロックをかける
				if( assetBundleInfo.Busy == true )
				{
					yield return new WaitWhile( () => assetBundleInfo.Busy == true ) ;
				}

				// 排他ロック有効
				assetBundleInfo.Busy = true ;

				//------------------------------------------

				// このアセットバンドルが更新対象になっているか確認する
				if( assetBundleInfo.UpdateRequired == true )
				{
					// キャッシュから削除する
					RemoveAssetBundleCacheForced( assetBundlePath ) ;

					// 更新対象になっているのでダウンロードを試みる
					yield return instance.StartCoroutine( LoadAssetBundleFromRemote_Coroutine( assetBundleInfo, LocationType, keep, null, onResult, true, request, instance ) ) ;

					if( assetBundleInfo.UpdateRequired == true )
					{
						// 失敗

						// 排他ロック無効
						assetBundleInfo.Busy = false ;

						yield break ;
					}
				}
				
				//------------------------------------------

				// このアセットバンドルが依存している他のアセットバンドルの情報を取得する

				if( m_Manifest != null && isRetain == false )
				{
					// レガシータイプの場合はマニフェストが存在しないので null チェックはきちんと行う必要がある
					string[] dependentAssetBundlePaths = m_Manifest.GetAllDependencies( assetBundlePath ) ;
					if( dependentAssetBundlePaths != null && dependentAssetBundlePaths.Length >  0 )
					{
						// 依存するものが存在する
#if UNITY_EDITOR
						if( dependentAssetBundlePaths.Length >= TOO_MUCH_DEPENDENCE )
						{
							// 明らかに過量な一定数の依存ファイルが存在する場合に警告を出す
							string wm = "[AssetBundleManager] Too much dependence : " + assetBundlePath + " ( " + dependentAssetBundlePaths.Length + " )\n" ;
							int wi ;
							for( wi  = 0 ; wi <  dependentAssetBundlePaths.Length ; wi ++ )
							{
								wm += " + " + dependentAssetBundlePaths[ wi ] + "\n" ;
							}
							Debug.LogWarning( wm ) ;
						}
#endif
						AssetBundleInfo	dependentAssetBundleInfo ;
						AssetBundle		dependentAssetBundle ;

						foreach( var dependentAssetBundlePath in dependentAssetBundlePaths )
						{
//							Debug.LogWarning( "非同期:依存するアセットバンドル名:" + dependentAssetBundlePath + " <- " + assetBundlePath ) ;

							dependentAssetBundleInfo = m_AssetBundleHash[ dependentAssetBundlePath ] ;

							//------------------------------

							// 非同期で同じアセットバンドルにアクセスする場合は排他ロックがかかる
							if( dependentAssetBundleInfo.Busy == true )
							{
								yield return new WaitWhile( () => dependentAssetBundleInfo.Busy == true ) ;
							}
							
							//------------------------------

							// 排他ロック有効
							dependentAssetBundleInfo.Busy = true ;

							if( dependentAssetBundleInfo.UpdateRequired == true )
							{
								// キャッシュから削除する
								RemoveAssetBundleCacheForced( dependentAssetBundlePath ) ;

								// 更新対象になっているのでダウンロードを試みる
								yield return instance.StartCoroutine( LoadAssetBundleFromRemote_Coroutine( dependentAssetBundleInfo, LocationType, keep, null, onResult, true, request, instance ) ) ;
							}

							if( dependentAssetBundleInfo.UpdateRequired == false )
							{
								// 依存アセットバンドルのダウンロードに成功
								if( CheckAssetBundleCache( dependentAssetBundlePath, isCaching ) == false )
								{
									// キャッシュには存在しないのでロードする
//									if( instance.m_FastLoadEnabled == false || FastLoadEnabled == false )
//									{
//										// 非同期(低速)
//										yield return instance.StartCoroutine( StorageAccessor_LoadAssetBundleAsync( dependentAssetBundleLocalPath, ( _ ) => { dependentAssetBundle = _ ; } ) ) ;
//									}
//									else
//									{
										// 同期(高速)
										dependentAssetBundle = CreateAssetBundle( dependentAssetBundleInfo, instance ) ;
//									}

									if( dependentAssetBundle != null )
									{
										// キャッシュにためる
										AddAssetBundleCache( dependentAssetBundlePath, dependentAssetBundle, isCaching, isRetain ) ;
									}
								}
							}

							// 排他ロック解除
							dependentAssetBundleInfo.Busy = false ;

							//------------------------------

							if( dependentAssetBundleInfo.UpdateRequired == true )
							{
#if UNITY_EDITOR
								Debug.LogWarning( "非同期:依存アセットバンドルのダウンロードに失敗した:" + dependentAssetBundlePath + " <- " + assetBundlePath ) ;
#endif
								// 依存アセットバンドルのダウンロードに失敗
								onResult?.Invoke( "Could not dependent load." ) ;

								// 排他ロック無効
								assetBundleInfo.Busy = false ;

								yield break ;
							}
						}
					}
				}

				//----------------------------------------------------------------

				// ここに来るということは既にローカルに最新のアセットバンドルが保存されている事を意味する

				AssetBundle assetBundle ;

				if( CheckAssetBundleCache( assetBundlePath, isCaching ) == false )
				{
					// キャッシュには存在しない


					// StreamingAssets へののダイレクトアクセスの場合

//					if( instance.m_FastLoadEnabled == false || FastLoadEnabled == false )
//					{
//						// 非同期(低速)
//						yield return instance.StartCoroutine( StorageAccessor_LoadAssetBundleAsync( assetBundleLocalPath, ( _ ) => { assetBundle = _ ; } ) ) ;
//					}
//					else
//					{
						// 同期(高速)
						assetBundle = CreateAssetBundle( assetBundleInfo, instance ) ;
//					}

					// キャッシュにためる
					if( assetBundle != null )
					{
						AddAssetBundleCache( assetBundlePath, assetBundle, isCaching, isRetain ) ;
					}
				}
				else
				{
					// キャッシュに存在する
					assetBundle = m_AssetBundleCache[ assetBundlePath ].AssetBundle ;
				}

				//---------------------------------------------------------

				// アセットバンドルのインスタンスを返す(予測不能のエラーとしてnullが返る可能性も0ではない)
				onLoaded?.Invoke( assetBundle ) ;
				onResult?.Invoke( string.Empty ) ;

				//---------------------------------------------------------

				// 排他ロック解除
				assetBundleInfo.Busy = false ;
			}

			// アセットバンドルのインスタンスを生成する
			private AssetBundle CreateAssetBundle( AssetBundleInfo assetBundleInfo, AssetBundleManager instance )
			{
				AssetBundle assetBundle ;

				if( IsStreamingAssetsOnly == true || ( StreamingAssetsDirectAccessEnabled == true && assetBundleInfo.LocationPriority == LocationPriorities.StreamingAssets ) )
				{
					// StreamingAssets にダイレクトアクセス
					assetBundle = StorageAccessor_LoadAssetBundleFromStreamingAssets( StreamingAssetsRootPath + "/" + assetBundleInfo.Path ) ;
				}
				else
				{
					// Storage にアクセス
					assetBundle = StorageAccessor_LoadAssetBundle( StorageCacheRootPath + ManifestName + "/" + assetBundleInfo.Path ) ;

#if UNITY_EDITOR
					if( instance.m_IsRecording == true )
					{
						// 使用するアセットバンドルの情報を記録中
						instance.RecordUsingAssetBundle_Private( ManifestName, assetBundleInfo.Path, assetBundleInfo.Size ) ;
					}
#endif
				}

				return assetBundle ;
			}



			/// <summary>
			/// アセットバンドルのダウンロードを行う　※直接呼び出し非推奨
			/// </summary>
			/// <param name="assetBundleName">アセットバンドル名</param>
			/// <param name="keep">キャッシュオーバー時の動作(true=キャッシュオーバー時に保持する・false=キャッシュオーバー時に破棄する)</param>
			/// <param name="instance">アセットバンドルマネージャのインスタンス</param>
			/// <returns>列挙子</returns>
			internal protected IEnumerator DownloadAssetBundle_Coroutine( string assetBundlePath, bool keep, Action<int,float,float> onProgress, Action<int,string> onResult, bool isManifestSaving, Request request, AssetBundleManager instance )
			{
				// そのファイルが更新対象か確認する
				if( m_AssetBundleHash.ContainsKey( assetBundlePath ) == false )
				{
					// 指定の名前のアセットバンドルは存在しない
					onResult?.Invoke( 0, "Could not load." ) ;
					yield break ;
				}

				//------------------------------------------

				// 指定の名前のアセットバンドルインフォは存在する

				// アセットバンドルインフォを取得する
				AssetBundleInfo assetBundleInfo = m_AssetBundleHash[ assetBundlePath ] ;

				// 非同期で同じアセットバンドルにアクセスする場合は排他ロックをかける
				if( assetBundleInfo.Busy == true )
				{
					yield return new WaitWhile( () => assetBundleInfo.Busy == true ) ;
				}

				// 排他ロック有効
				assetBundleInfo.Busy = true ;

				//------------------------------------------

				// このアセットバンドルが更新対象になっているか確認する
				if( assetBundleInfo.UpdateRequired == true )
				{
					RemoveAssetBundleCacheForced( assetBundlePath ) ;

					// 更新対象になっているのでダウンロードを試みる
					yield return instance.StartCoroutine( LoadAssetBundleFromRemote_Coroutine
					(
						assetBundleInfo,
						LocationType,
						keep,
						( float downloadingProgress, float writingProgress ) =>
						{
							onProgress?.Invoke( assetBundleInfo.Size, downloadingProgress, writingProgress ) ;
						},
						null,
						isManifestSaving,
						request,
						instance
					) ) ;

					if( assetBundleInfo.UpdateRequired == true )
					{
						// 失敗
						onResult?.Invoke( assetBundleInfo.Size, "Can not load " + assetBundlePath ) ;

						// 排他ロック無効
						assetBundleInfo.Busy = false ;

						yield break ;
					}
				}

				// 成功
				onResult?.Invoke( assetBundleInfo.Size, string.Empty ) ;

				//---------------------------------------------------------

				// 排他ロック解除
				assetBundleInfo.Busy = false ;
			}
#if false
			/// <summary>
			/// タグで指定されたアセットバンドルのダウンロードを行う　※直接呼び出し非推奨
			/// </summary>
			/// <param name="assetBundleName">アセットバンドル名</param>
			/// <param name="keep">キャッシュオーバー時の動作(true=キャッシュオーバー時に保持する・false=キャッシュオーバー時に破棄する)</param>
			/// <param name="instance">アセットバンドルマネージャのインスタンス</param>
			/// <returns>列挙子</returns>
			internal protected IEnumerator DownloadAssetBundlesWithTags_Coroutine( string[] tags, bool keep, Action<int,int> onProgress, Request request, AssetBundleManager instance )
			{
				string[] assetBundlePaths =	GetAllAssetBundlePathsWithTags( tags, true, true ) ;

				if( assetBundlePaths == null || assetBundlePaths.Length == 0 )
				{
					// ダウンロード対象となるファイルが見つからない
					request.Error = "Target not found." ;
					yield break ;
				}

				//---------------------------------------------------------
				
				// 全体サイズを計算する
				int entireDataSize = 0 ;
				foreach( var assetBundlePath in assetBundlePaths )
				{
					AssetBundleInfo assetBundleInfo = m_AssetBundleHash[ assetBundlePath ] ;
					entireDataSize += assetBundleInfo.Size ;
				}
				request.EntireDataSize = entireDataSize ;
				request.EntireFileCount = assetBundlePaths.Length ;

				// ダウンロードを行う
				int storedDataSize = 0 ;
				foreach( var assetBundlePath in assetBundlePaths )
				{
					AssetBundleInfo assetBundleInfo = m_AssetBundleHash[ assetBundlePath ] ;

					// 念のためキャッシュから削除する
					RemoveAssetBundleCacheForced( assetBundlePath ) ;

					// 更新対象になっているのでダウンロードを試みる
					Request subRequest = new Request( instance ) ;

					storedDataSize = request.StoredDataSize ;

					yield return instance.StartCoroutine( LoadAssetBundleFromRemote_Coroutine
					(
						assetBundleInfo,
						keep,
						( float progress ) =>
						{
							request.StoredDataSize = storedDataSize + subRequest.StoredDataSize ;
							request.Progress = ( float )request.StoredDataSize / ( float )request.EntireDataSize ;

							onProgress?.Invoke( request.StoredFileCount, request.StoredDataSize ) ;
						},
						( string error ) =>
						{
							if( string.IsNullOrEmpty( error ) == true )
							{
								request.StoredFileCount ++ ;

								onProgress?.Invoke( request.StoredFileCount, request.StoredDataSize ) ;
							}
						},
						subRequest,
						instance
					) ) ;

					if( assetBundleInfo.UpdateRequired == true )
					{
						// ダウンロード失敗
						request.Error = "Could not load " + assetBundlePath ;
						yield break ;
					}
				}

				// 成功
				request.IsDone = true ;
			}
#endif
			//------------------------------------------------------------------------------------------

			//------------------------------------------------------------------------------------------

			/// <summary>
			/// アセットバンドルをキャッシュから削除する　※呼び出し非推奨
			/// </summary>
			/// <param name="assetBundleName">アセットバンドル名</param>
			/// <param name="instance">アセットバンドルマネージャのインスタンス</param>
			/// <returns>列挙子</returns>
			internal protected bool DeleteAssetBundleFromStorageCache( string assetBundlePath, AssetBundleManager instance )
			{
				// そのファイルが更新対象か確認する
				if( m_AssetBundleHash.ContainsKey( assetBundlePath ) == false )
				{
					// 指定の名前のアセットバンドルは存在しない
					return false ;
				}

				//------------------------------------------

				// 指定の名前のアセットバンドルインフォは存在する

				// アセットバンドルインフォを取得する
				AssetBundleInfo assetBundleInfo = m_AssetBundleHash[ assetBundlePath ] ;

				// ベースパス
				string path = StorageCacheRootPath + ManifestName + "/" ;
				
				// 削除した
				StorageAccessor_Remove( path + assetBundleInfo.Path ) ;

				// ファイルが存在しなくなったフォルダも削除する
				if( instance.m_SecurityEnabled == false )
				{
					StorageAccessor_RemoveAllEmptyFolders( path ) ;
				}

				// 更新必要フラグをオンにする
				assetBundleInfo.UpdateRequired = true ;

				return true ;
			}

			//--------------------------------------------------------------

			/// <summary>
			/// アセットバンドルの保有を確認する
			/// </summary>
			/// <param name="assetBundlePath">アセットバンドルパス</param>
			/// <returns>結果(true=存在する・false=存在しない</returns>
			internal protected bool Contains( string assetBundlePath )
			{
				// そのファイルが更新対象か確認する
				if( m_AssetBundleHash.ContainsKey( assetBundlePath ) == false )
				{
					// 指定の名前のアセットバンドルは存在しない
					return false ;
				}

				//------------------------------------------

				// ここに来るということは既にローカルに最新のアセットバンドルが保存されている事を意味する
				return true ;
			}

			/// <summary>
			/// アセットバンドルの存在を確認する
			/// </summary>
			/// <param name="assetBundlePath">アセットバンドルパス</param>
			/// <returns>結果(true=存在する・false=存在しない</returns>
			internal protected bool Exists( string assetBundlePath )
			{
				// そのファイルが更新対象か確認する
				if( m_AssetBundleHash.ContainsKey( assetBundlePath ) == false )
				{
					// 指定の名前のアセットバンドルは存在しない
					Debug.LogWarning( "Unknown AssetBundle Path = " + assetBundlePath ) ;
					return false ;
				}

				//------------------------------------------

				// 指定の名前のアセットバンドルインフォは存在する

				// アセットバンドルインフォを取得する
				AssetBundleInfo assetBundleInfo = m_AssetBundleHash[ assetBundlePath ] ;

				// このアセットバンドルが更新対象になっているか確認する
				if( assetBundleInfo.UpdateRequired == true )
				{
					// 更新対象になっているので取得不可
					return false ;
				}
				
				//------------------------------------------

				// このアセットバンドルが依存している他のアセットバンドルの情報を取得する

				if( m_Manifest != null )
				{
					// レガシータイプの場合はマニフェストが存在しないので null チェックはきちんと行う必要がある
					string[] dependentAssetBundlePaths = m_Manifest.GetAllDependencies( assetBundlePath ) ;
					if( dependentAssetBundlePaths != null && dependentAssetBundlePaths.Length >  0 )
					{
						// 依存するものが存在する
#if UNITY_EDITOR
						if( dependentAssetBundlePaths.Length >= TOO_MUCH_DEPENDENCE )
						{
							// 明らかに過量な一定数の依存ファイルが存在する場合に警告を出す
							string wm = "[AssetBundleManager] Too much dependence : " + assetBundlePath + " ( " + dependentAssetBundlePaths.Length + " )\n" ;
							int wi ;
							for( wi  = 0 ; wi <  dependentAssetBundlePaths.Length ; wi ++ )
							{
								wm += " + " + dependentAssetBundlePaths[ wi ] + "\n" ;
							}
							Debug.LogWarning( wm ) ;
						}
#endif
						AssetBundleInfo	dependentAssetBundleInfo ;

						foreach( var dependentAssetBundlePath in dependentAssetBundlePaths )
						{
							dependentAssetBundleInfo = m_AssetBundleHash[ dependentAssetBundlePath ] ;
							if( dependentAssetBundleInfo.UpdateRequired == true )
							{
								// 更新対象であるため取得不可
								RemoveAssetBundleCacheForced( dependentAssetBundlePath ) ;

								Debug.LogWarning( "----- ※存在:依存するアセットバンドルに更新が必要なものがある:" + dependentAssetBundlePath + " <- " + assetBundlePath ) ;

								// １つでも依存アセットバンドルが欠けていたら対象のアセットバンドルも取得出来ない(非同期で取得せよ)
								return false ;
							}
						}
					}
				}

				// 依存も含めて全て問題の無い状態になっている
				return true ;
			}
			
			/// <summary>
			/// アセットバンドルのサイズを取得する
			/// </summary>
			/// <param name="assetBundlePath">アセットバンドルのパス</param>
			/// <returns>結果(true=存在する・false=存在しない</returns>
			internal protected int GetSize( string assetBundlePath )
			{
				// そのファイルが更新対象か確認する
				if( m_AssetBundleHash.ContainsKey( assetBundlePath ) == false )
				{
					// 指定の名前のアセットバンドルは存在しない
					return 0 ;
				}

				//------------------------------------------

				// 指定の名前のアセットバンドルインフォは存在する

				// アセットバンドルインフォを取得する
				AssetBundleInfo assetBundleInfo = m_AssetBundleHash[ assetBundlePath ] ;

				//------------------------------------------

				// ここに来るということは既にローカルに最新のアセットバンドルが保存されている事を意味する
				return assetBundleInfo.Size ;
			}

			//--------------------------------------------------------------

			/// <summary>
			/// アセットパンドルのパス一覧を取得する
			/// </summary>
			/// <param name="updateRequiredOnly">更新が必要なものみに対象を限定するかどうか</param>
			/// <returns>アセットバンドルのパス一覧</returns>
			internal protected string[] GetAllAssetBundlePaths( bool updateRequiredOnly = true )
			{
				List<string> paths = new List<string>() ;

				foreach( var assetBundleInfo in m_AssetBundleInfo )
				{
					if( updateRequiredOnly == false || ( updateRequiredOnly == true && assetBundleInfo.UpdateRequired == true ) )
					{
						paths.Add( assetBundleInfo.Path ) ;
					}
				}

				if( paths.Count == 0 )
				{
					return null ;
				}

				return paths.ToArray() ;
			}

			/// <summary>
			/// アセットパンドルのパス一覧を取得する
			/// </summary>
			/// <param name="updateRequiredOnly">更新が必要なものみに対象を限定するかどうか</param>
			/// <returns>アセットバンドルのパス一覧</returns>
			internal protected string[] GetAllAssetBundlePathsWithTags( string[] tags, bool updateRequiredOnly = true, bool isDependency = true )
			{
				List<string> paths = new List<string>() ;

				foreach( var assetBundleInfo in m_AssetBundleInfo )
				{
					if( updateRequiredOnly == false || ( updateRequiredOnly == true && assetBundleInfo.UpdateRequired == true ) )
					{
						foreach( var tag in tags )
						{
							if( assetBundleInfo.ContainsTag( tag ) == true )
							{
								paths.Add( assetBundleInfo.Path ) ;
							}
						}
					}
				}

				if( paths.Count == 0 )
				{
					return null ;
				}

				if( isDependency == true )
				{
					// 依存関係にあるものも含めて取得する
					List<string> temporaryPaths = new List<string>() ;
					foreach( var path in paths )
					{
						temporaryPaths.Add( path ) ;	// まずは自身を追加する

						string[] dependentAssetBundlePaths = GetAllDependentAssetBundlePaths( path, updateRequiredOnly ) ;
						if( dependentAssetBundlePaths != null && dependentAssetBundlePaths.Length >  0 )
						{
							foreach( var dependentAssetBundlePath in dependentAssetBundlePaths )
							{
								if( temporaryPaths.Contains( dependentAssetBundlePath ) == false )
								{
									temporaryPaths.Add( dependentAssetBundlePath ) ;	// 未追加の場合のみ追加する
								}
							}
						}
					}
					paths = temporaryPaths ;
				}

				return paths.ToArray() ;
			}

			//--------------------------------------------------------------

			/// <summary>
			/// 依存関係にあるアセットバンドルのパス一覧を取得する
			/// </summary>
			/// <param name="tNeedUpdateOnly">更新が必要なものみに対象を限定するかどうか</param>
			/// <returns>アセットバンドルのパス一覧</returns>
			internal protected string[] GetAllDependentAssetBundlePaths( string assetBundlePath, bool updateRequiredOnly = true )
			{
				// 全て小文字化
				assetBundlePath = assetBundlePath.ToLower() ;

				string[] dependentAssetBundlePaths = m_Manifest.GetAllDependencies( assetBundlePath ) ;

				if( dependentAssetBundlePaths == null || dependentAssetBundlePaths.Length == 0 )
				{
					return null ;	// 依存するアセットバンドル存在しない
				}

				if( updateRequiredOnly == false )
				{
					// そのまま返す
					return dependentAssetBundlePaths ;
				}

				//---------------------------------------------------------

				// 更新が必要なもののみ返す

				List<string> paths = new List<string>() ;

				AssetBundleInfo	dependentAssetBundleInfo ;

				foreach( var dependentAssetBundlePath in dependentAssetBundlePaths )
				{
					dependentAssetBundleInfo = m_AssetBundleHash[ dependentAssetBundlePath ] ;
					if( dependentAssetBundleInfo.UpdateRequired == true )
					{
						// 更新対象限定
						paths.Add( dependentAssetBundlePath ) ;
					}
				}

				if( paths.Count == 0 )
				{
					return null ;
				}

				return paths.ToArray() ;
			}

			//--------------------------------------------------------------

			/// <summary>
			/// 非アセットバンドルの直接パスを取得する
			/// </summary>
			/// <param name="tNeedUpdateOnly">更新が必要なものみに対象を限定するかどうか</param>
			/// <returns>アセットバンドルのパス一覧</returns>
			internal protected string GetAssetFilePath( string assetBundlePath, AssetBundleManager instance )
			{
				// 全て小文字化
				assetBundlePath = assetBundlePath.ToLower() ;

				// そのファイルが更新対象か確認する
				// 存在を確認する
				if( m_AssetBundleHash.ContainsKey( assetBundlePath ) == false )
				{
					// そのような名前のアセットバンドルは存在しない
					return null ;
				}

				AssetBundleInfo assetBundleInfo = m_AssetBundleHash[ assetBundlePath ] ;

				if( assetBundleInfo.UpdateRequired == true )
				{
					// 更新が必要なファイルなので存在しないものとみなす
					return null ;
				}

				if( IsStreamingAssetsOnly == true || ( StreamingAssetsDirectAccessEnabled == true && assetBundleInfo.LocationPriority == LocationPriorities.StreamingAssets ) )
				{
					// StreamingAssets が対象

					// 環境パスを取得する
					return StorageAccessor_GetPathFromStreamingAssets( StreamingAssetsRootPath + "/" + assetBundleInfo.Path ) ;
				}
				else
				{
					// Storage が対象
#if UNITY_EDITOR
					if( string.IsNullOrEmpty( assetBundleInfo.Hash ) == true )
					{
						// ネイティブパスを参照した時点で必要なものとみなす
						if( instance.m_IsRecording == true )
						{
							instance.RecordUsingAssetBundle_Private( ManifestName, assetBundlePath, assetBundleInfo.Size ) ;
						}
					}
#endif
					// 環境パスを取得する
					return StorageAccessor_GetPath( StorageCacheRootPath + ManifestName + "/" + assetBundleInfo.Path ) ;
				}
			}

			//--------------------------------------------------------------

			/// <summary>
			/// 指定のアセットバンドルのキャッシュ内での動作を設定する
			/// </summary>
			/// <param name="tAssetBundleName">アセットバンドル名</param>
			/// <param name="tKeep">キャッシュオーバー時の動作(true=キャッシュオーバー時に保持する・false=キャッシュオーバー時に破棄する)</param>
			/// <returns>結果(true=成功・失敗)</returns>
			public bool SetKeepFlag( string assetBundlePath, bool keep )
			{
				// 全て小文字化
				assetBundlePath = assetBundlePath.ToLower() ;

				// 存在を確認する
				if( m_AssetBundleHash.ContainsKey( assetBundlePath ) == false )
				{
					// そのような名前のアセットバンドルは存在しない
					return false ;
				}

				m_AssetBundleHash[ assetBundlePath ].Keep = keep ;

				return true ;
			}

			/// <summary>
			/// 破棄可能なアセットバンドルをタイムスタンプの古い順に破棄してキャッシュの空き容量を確保する
			/// </summary>
			/// <param name="size">必要なキャッシュサイズ</param>
			/// <returns>結果(true=成功・false=失敗)</returns>
			private bool Cleanup( int requireSize )
			{
				// キープ対象全てとキープ非対象でタイムスタンプの新しい順にサイズを足していきキャッシュの容量をオーバーするまで検査する
				List<AssetBundleInfo> freeAssetBundleInfo = new List<AssetBundleInfo>() ;

				long freeCacheSize = CacheSize ;

				foreach( var assetBundleInfo in m_AssetBundleInfo )
				{
					if( assetBundleInfo.UpdateRequired == false )
					{
						// 更新の必要の無い最新の状態のアセットバンドル
						if( assetBundleInfo.Keep == true )
						{
							// 常時保持する必要のあるアセットパンドル
//							if( assetBundleInfo[ i ].size >  0 )
//							{
								freeCacheSize -= assetBundleInfo.Size ;
//							}
						}
						else
						{
							// 空き容量が足りなくなったら破棄してもよいアセットバンドル
//							if( assetBundleInfo[ i ].size >  0 )
//							{
								// 破棄可能なアセットバンドルの情報を追加する
								freeAssetBundleInfo.Add( assetBundleInfo ) ;
//							}
						}
					}
					else
					if( assetBundleInfo.IsDownloading == true )
					{
						// 並列ダウンロード中
						freeCacheSize -= assetBundleInfo.Size ;
					}
				}

				if( freeCacheSize <  requireSize )
				{
					// 破棄しないアセットバンドルだけで既に空き容量が足りない
					return false ;
				}
				
				if( freeAssetBundleInfo.Count == 0 )
				{
					// 空き容量は足りる
					return true ;
				}

				//--------------------------------

				// 破棄できるアセットバンドルで既にに実体を持っているものをタイムスタンプの新しいものの順にソートする
				freeAssetBundleInfo.Sort( ( a, b ) => ( int )( a.LastUpdateTime - b.LastUpdateTime ) ) ;

				int i, l = freeAssetBundleInfo.Count ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					freeCacheSize -= freeAssetBundleInfo[ i ].Size ;
					if( freeCacheSize <  requireSize )
					{
						// ここから容量が足りない
						break ;
					}
				}

				if( i >= l )
				{
					// 空き容量は足りる
					return true ;
				}

				int s = i ;

				// ここからアセットバンドルを破棄する(古いものを優先的に破棄)
				for( i  = s ; i <  l ; i ++ )
				{
					// 一時的に削除するだけなので空になったフォルダまで削除する事はしない
					StorageAccessor_Remove( StorageCacheRootPath + ManifestName + "/" + freeAssetBundleInfo[ i ].Path ) ;
					
					freeAssetBundleInfo[ i ].LastUpdateTime = 0L ;
					freeAssetBundleInfo[ i ].UpdateRequired = true ;
				}

				// マニフェスト情報を保存しておく
//				Save( tInstance ) ;	// この後の追加保存でマニフェスト情報を保存するのでここでは保存しない

				return true ;
			}
		}
	}
}
