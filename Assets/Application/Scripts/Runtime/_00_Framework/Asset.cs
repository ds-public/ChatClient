using System ;
using System.Collections ;
using System.Collections.Generic ;

using Cysharp.Threading.Tasks ;

using UnityEngine ;
using UnityEngine.U2D ;

// 要 AssetBundleHelper パッケージ
using AssetBundleHelper ;

using uGUIHelper ;

namespace Template
{
	/// <summary>
	/// アセットクラス(アセット全般の読み出しに使用する) Version 2022/05/04 0
	/// </summary>
	public class Asset : ExMonoBehaviour
	{
		private static Asset	m_Instance ;
		internal void Awake()
		{
			m_Instance = this ;
		}
		internal void OnDestroy()
		{
			m_Instance = null ;
		}

		//-----------------------------------------------------------------

		/// <summary>
		/// キャッシュタイプ
		/// </summary>
		public enum CachingTypes
		{
			None			= 0,	// キャッシュしない
			ResourceOnly	= 1,	// リソースのみキャッシャする
			AssetBundleOnly	= 2,	// アセットバンドルのみキャッシュする
			Same			= 3,	// リソース・アセットバンドルともにキャッシュする
		}

		// 注意：アセットバンドルから展開させるリソースのインスタンスについて
		//
		// パスは同じでも異なるアセットバンドルのインスタンスから展開された同じパスのリソースは、
		// 別のリソースとして扱われる(別のインスタンスとなる)
		// よってアセットバンドルの展開そのものと、
		// 展開されたアセットバンドルからのリソースの展開は、重々注意する必要がある。
		// (重複展開され、無駄にメモリを消費する事になる。その他にもバグの要因となる。)

		// None：展開されたリソースのインスタンスは別個
		// 　動作をきちんと理解していないと危険なタイプである。
		// 　同じ動作を２度行った際の展開されたリソースのインスタンスは別物になる。
		// 　そのシーンで１度しか使わないようなものに対してのみ使用すること。
		//
		// ResourceOnly：展開されたリソースのインスタンスは同一
		// 　展開されたリソースそのもののインスタンスは同一になるが、
		// 　リソース群が同じアセットバンドルに内包されている場合、
		// 　何度も無駄にアセットバンドルの展開が発生する事になる。
		//
		// AssetBundleOnly：展開されたリソースのインスタンスは同一
		// 　展開されたアセットバンドルのインスタンス内に、
		// 　展開されたリソースのインスタンス(実体はシステムリソースキャッシュにある)を
		// 　保持しているため、同じリソースであればリソースの再展開は行われず、
		// 　同一のリソースのリソースのインスタンスが返される。
		//
		// Same：展開されたリソースのインスタンスは同一
		// 　ResourceOnly とと同じく、リソースキャッシュに保存された、
		// 　展開されたリソースのインスタンスを返すので、
		// 　展開されたリソースのインスタンスは同一になる。

		//-------------------------------------------------------------------------------------------

		// Paths

		/// <summary>
		/// アセットバンドルに含まれる全てのアセットのパスを取得する(同期版)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="cachingType"></param>
		/// <returns></returns>
		public static ( string, Type )[] GetAllPaths( string path, CachingTypes cachingType = CachingTypes.None, bool isOriginal = false )
		{
			// 同期版で失敗してもエラーダイアログは表示されないので同期ロードは成功前提です
			return AssetBundleManager.GetAllAssetPaths( path, ( AssetBundleManager.CachingTypes )cachingType, isOriginal ) ;
		}

		/// <summary>
		/// アセットバンドルに含まれる全てのアセットのパスを取得する(非同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="path">リソースまたはアセットバンドルのパス</param>
		/// <param name="asset">読み出されたアセットのインスタンスを格納する要素数１以上の任意のコンポーネント型の配列</param>
		/// <param name="caching">読み出されたアセットをメモリキャッシュにためるかどうか</param>
		/// <returns>列挙子</returns>
		public static async UniTask<( string, Type )[]> GetAllPathsAsync( string path, CachingTypes cachingType = CachingTypes.None, bool isNoDialog = false, bool isOriginal = false )
		{
			( string, Type )[] allAssetPaths = null ;

			// 高速化のためまず同期ロードを行う
			allAssetPaths = AssetBundleManager.GetAllAssetPaths( path, ( AssetBundleManager.CachingTypes )cachingType, isOriginal ) ;
			if( allAssetPaths != null )
			{
				// 成功(awaitを実行させない)
				return allAssetPaths ;
			}

			// 非同期ロード
			while( true )
			{
				allAssetPaths = null ;
				var assetRequest = AssetBundleManager.GetAllAssetPathsAsync( path, ( _ ) => { allAssetPaths = _ ; }, ( AssetBundleManager.CachingTypes )cachingType, false, isOriginal ) ;
				await assetRequest ;
	
				if( allAssetPaths != null )
				{
					// 成功
					return allAssetPaths ;
				}
				else
				{
					// 失敗
					if( isNoDialog == false )
					{
						// エラーダイアログを表示する
						if( await OpenDownloadErrorDialog( assetRequest.ResultCode, path ) == true )
						{
							// コルーチン終了(シングルトンである AssetBundleManager のコルーチンスタックが残り続けてしまうため)
							return null ;
						}
						// Rebootしなければリトライする
					}
					else
					{
						// エラーダイアログを表示しない(そのままエラーとなる)
						return null ;
					}
				}
			}
		}

		/// <summary>
		/// 指定したタイプのアセットバンドルに含まれる全てのアセットのパスを取得する(同期版)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="cachingType"></param>
		/// <returns></returns>
		public static string[] GetAllPaths( string path, Type type, CachingTypes cachingType = CachingTypes.None, bool isOriginal = false )
		{
			// 同期版で失敗してもエラーダイアログは表示されないので同期ロードは成功前提です
			return AssetBundleManager.GetAllAssetPaths( path, type, ( AssetBundleManager.CachingTypes )cachingType, isOriginal ) ;
		}

		/// <summary>
		/// 指定したタイプのアセットバンドルに含まれる全てのアセットのパスを取得する(非同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="path">リソースまたはアセットバンドルのパス</param>
		/// <param name="asset">読み出されたアセットのインスタンスを格納する要素数１以上の任意のコンポーネント型の配列</param>
		/// <param name="caching">読み出されたアセットをメモリキャッシュにためるかどうか</param>
		/// <returns>列挙子</returns>
		public static async UniTask<string[]> GetAllPathsAsync( string path, Type type, CachingTypes cachingType = CachingTypes.None, bool isNoDialog = false, bool isOriginal = false )
		{
			string[] allAssetPaths = null ;

			// 高速化のためまず同期ロードを行う
			allAssetPaths = AssetBundleManager.GetAllAssetPaths( path, type, ( AssetBundleManager.CachingTypes )cachingType, isOriginal ) ;
			if( allAssetPaths != null )
			{
				// 成功(awaitを実行させない)
				return allAssetPaths ;
			}

			// 非同期ロード
			while( true )
			{
				allAssetPaths = null ;
				var assetRequest = AssetBundleManager.GetAllAssetPathsAsync( path, type, ( _ ) => { allAssetPaths = _ ; }, ( AssetBundleManager.CachingTypes )cachingType, false, isOriginal ) ;
				await assetRequest ;
	
				if( allAssetPaths != null )
				{
					// 成功
					return allAssetPaths ;
				}
				else
				{
					// 失敗
					if( isNoDialog == false )
					{
						// エラーダイアログを表示する
						if( await OpenDownloadErrorDialog( assetRequest.ResultCode, path ) == true )
						{
							// コルーチン終了(シングルトンである AssetBundleManager のコルーチンスタックが残り続けてしまうため)
							return null ;
						}
						// Rebootしなければリトライする
					}
					else
					{
						// エラーダイアログを表示しない(そのままエラーとなる)
						return null ;
					}
				}
			}
		}

		//-----------------------------------------------------------

		// Asset

		/// <summary>
		/// アセットを読み出す(同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="path">リソースまたはアセットバンドルのパス</param>
		/// <param name="cachingType">読み出されたアセットをメモリキャッシュにためるかどうか</param>
		/// <returns>任意のコンポーネント型のアセットのインスタンス</returns>
		public static T Load<T>( string path, CachingTypes cachingType = CachingTypes.None ) where T : UnityEngine.Object
		{
			// 同期ロードで失敗してもエラーダイアログは表示されないので同期ロードは成功前提です
			T result = AssetBundleManager.LoadAsset<T>( path, ( AssetBundleManager.CachingTypes )cachingType ) ;
#if UNITY_EDITOR
			if( result == null )
			{
				Debug.LogWarning( "[ Asset.Lodad<T> Error ] " + path ) ;
			}
#endif
			return result ;
		}

		/// <summary>
		/// アセットを読み出す(同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="path">リソースまたはアセットバンドルのパス</param>
		/// <param name="cachingType">読み出されたアセットをメモリキャッシュにためるかどうか</param>
		/// <returns>任意のコンポーネント型のアセットのインスタンス</returns>
		public static UnityEngine.Object Load( string path, Type type, CachingTypes cachingType = CachingTypes.None )
		{
			// 同期ロードで失敗してもエラーダイアログは表示されないので同期ロードは成功前提です
			UnityEngine.Object result = AssetBundleManager.LoadAsset( path, type, ( AssetBundleManager.CachingTypes )cachingType ) ;
#if UNITY_EDITOR
			if( result == null )
			{
				Debug.LogWarning( "[ Asset.Lodad Error ] " + path ) ;
			}
#endif
			return result ;
		}

		//---------------

		/// <summary>
		/// アセットを読み出す(非同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="path">リソースまたはアセットバンドルのパス</param>
		/// <param name="onLoaded">読み出されたアセットのインスタンスを格納する要素数１以上の任意のコンポーネント型の配列</param>
		/// <param name="cachingType">読み出されたアセットをメモリキャッシュにためるかどうか</param>
		/// <returns>列挙子</returns>
		public static async UniTask<T> LoadAsync<T>( string path, CachingTypes cachingType = CachingTypes.None, bool isNoDialog = false ) where T : UnityEngine.Object
		{
			// 高速化のためまず同期ロードを行う
			T result = AssetBundleManager.LoadAsset<T>( path, ( AssetBundleManager.CachingTypes )cachingType ) ;
			if( result != null )
			{
				// 成功(awaitを実行させない)
				return result ;
			}

			// 非同期ロードを行う
			UnityEngine.Object asset = await LoadAsync( path, typeof( T ), cachingType, isNoDialog ) ;
			return asset == null ? null : asset as T ;
		}

		/// <summary>
		/// アセットを読み出す(非同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="path">リソースまたはアセットバンドルのパス</param>
		/// <param name="onLoaded">読み出されたアセットのインスタンスを格納する要素数１以上の任意のコンポーネント型の配列</param>
		/// <param name="cachingType">読み出されたアセットをメモリキャッシュにためるかどうか</param>
		/// <returns>列挙子</returns>
		public static async UniTask<UnityEngine.Object> LoadAsync( string path, Type type, CachingTypes cachingType = CachingTypes.None, bool isNoDialog = false )
		{
			UnityEngine.Object asset ;

			// 高速化のためまず同期ロードを行う
			asset = AssetBundleManager.LoadAsset( path, type, ( AssetBundleManager.CachingTypes )cachingType ) ;
			if( asset != null )
			{
				// 成功(awaitを実行させない)
				return asset ;
			}

			// 非同期ロード
			while( true )
			{
				asset = null ;
				var assetRequest = AssetBundleManager.LoadAssetAsync( path, type, ( _ ) => { asset = _ ; }, ( AssetBundleManager.CachingTypes )cachingType, false ) ;
				await assetRequest ;
	
				if( asset != null )
				{
					// 成功
					return asset ;
				}
				else
				{
					// 失敗
					if( isNoDialog == false )
					{
						// エラーダイアログを表示する
						if( await OpenDownloadErrorDialog( assetRequest.ResultCode, path ) == true )
						{
							// コルーチン終了(シングルトンである AssetBundleManager のコルーチンスタックが残り続けてしまうため)
							return null ;
						}
						// Rebootしなければリトライする
					}
					else
					{
						// エラーダイアログを表示しない(そのままエラーとなる)
						return null ;
					}
				}
			}
		}

		//-----------------------------------------------------------

		// AllAssets

		/// <summary>
		/// 全てのアセットを読み出す(同期版)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path"></param>
		/// <param name="cachingType"></param>
		/// <returns></returns>
		public static T[] LoadAll<T>( string path, CachingTypes cachingType = CachingTypes.None ) where T : UnityEngine.Object
		{
			// 同期ロードで失敗してもエラーダイアログは表示されないので同期ロードは成功前提です
			T[] result = AssetBundleManager.LoadAllAssets<T>( path, ( AssetBundleManager.CachingTypes )cachingType ) ;
#if UNITY_EDITOR
			if( result == null )
			{
				Debug.LogWarning( "[ Asset.LodadAll<T> Error ] " + path ) ;
			}
#endif
			return result ;
		}

		/// <summary>
		/// 全てのアセットを読み出す(同期版)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="type"></param>
		/// <param name="cachingType"></param>
		/// <returns></returns>
		public static UnityEngine.Object[] LoadAll( string path, Type type, CachingTypes cachingType = CachingTypes.None )
		{
			// 同期ロードで失敗してもエラーダイアログは表示されないので同期ロードは成功前提です
			UnityEngine.Object[] result = AssetBundleManager.LoadAllAssets( path, type, ( AssetBundleManager.CachingTypes )cachingType ) ;
#if UNITY_EDITOR
			if( result == null )
			{
				Debug.LogWarning( "[ Asset.LodadAll Error ] " + path ) ;
			}
#endif
			return result ;
		}

		//---------------

		/// <summary>
		/// 全てのアセットを読み出す(非同期版)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path"></param>
		/// <param name="onLoaded"></param>
		/// <param name="cachingType"></param>
		/// <param name="isNoDialog"></param>
		/// <returns></returns>
		public static async UniTask<T[]> LoadAllAsync<T>( string path, CachingTypes cachingType = CachingTypes.None, bool isNoDialog = false ) where T : UnityEngine.Object
		{
			// 高速化のためまず同期ロードを行う
			T[] result = AssetBundleManager.LoadAllAssets<T>( path, ( AssetBundleManager.CachingTypes )cachingType ) ;
			if( result != null )
			{
				// 成功(awaitを実行させない)
				return result ;
			}

			// 非同期ロードを行う
			UnityEngine.Object[] assets = await LoadAllAsync( path, typeof( T ), cachingType, isNoDialog ) ;
			if( assets == null )
			{
				return null ;
			}

			result = new T[ assets.Length ] ;
			for( int i  = 0 ; i <  assets.Length ; i ++ )
			{
				result[ i ] = assets[ i ] as T ;
			}

			return result ;
		}

		/// <summary>
		/// 全てのアセットを読み出す(非同期版)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="type"></param>
		/// <param name="onLoaded"></param>
		/// <param name="cachingType"></param>
		/// <param name="isNoDialog"></param>
		/// <returns></returns>
		public static async UniTask<UnityEngine.Object[]> LoadAllAsync( string path, Type type, CachingTypes cachingType = CachingTypes.None, bool isNoDialog = false )
		{
			UnityEngine.Object[] assets ;

			// 高速化のためまず同期ロードを行う
			assets = AssetBundleManager.LoadAllAssets( path, type, ( AssetBundleManager.CachingTypes )cachingType ) ;
			if( assets != null )
			{
				// 成功(awaitを実行させない)
				return assets ;
			}

			// 非同期ロード
			while( true )
			{
				assets = null ;
				var assetRequest = AssetBundleManager.LoadAllAssetsAsync( path, type, ( _ ) => { assets = _ ; }, ( AssetBundleManager.CachingTypes )cachingType, false ) ;
				await assetRequest ;
	
				if( assets != null )
				{
					// 成功
					return assets ;
				}
				else
				{
					// 失敗
					if( isNoDialog == false )
					{
						// エラーダイアログを表示する
						if( await OpenDownloadErrorDialog( assetRequest.ResultCode, path ) == true )
						{
							// コルーチン終了(シングルトンである AssetBundleManager のコルーチンスタックが残り続けてしまうため)
							return null ;
						}
						// Rebootしなければリトライする
					}
					else
					{
						// エラーダイアログを表示しない(そのままエラーとなる)
						return null ;
					}
				}
			}
		}

		//-----------------------------------------------------------

		// SubAsset

		/// <summary>
		/// サブアセットを読み出す(同期版)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path"></param>
		/// <param name="subAssetName"></param>
		/// <param name="cachingType"></param>
		/// <returns></returns>
		public static T LoadSub<T>( string path, string subAssetName, CachingTypes cachingType = CachingTypes.None ) where T : UnityEngine.Object
		{
			// 同期ロードで失敗してもエラーダイアログは表示されないので同期ロードは成功前提です
			T result = AssetBundleManager.LoadSubAsset<T>( path, subAssetName, ( AssetBundleManager.CachingTypes )cachingType ) ;
#if UNITY_EDITOR
			if( result == null )
			{
				Debug.LogWarning( "[ Asset.LodadSub<T> Error ] " + path ) ;
			}
#endif
			return result ;
		}

		/// <summary>
		/// サブアセットを読み出す(同期版)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="subAssetName"></param>
		/// <param name="type"></param>
		/// <param name="cachingType"></param>
		/// <returns></returns>
		public static UnityEngine.Object LoadSub( string path, string subAssetName, Type type, CachingTypes cachingType = CachingTypes.None )
		{
			// 同期ロードで失敗してもエラーダイアログは表示されないので同期ロードは成功前提です
			UnityEngine.Object result = AssetBundleManager.LoadSubAsset( path, subAssetName, type, ( AssetBundleManager.CachingTypes )cachingType ) ;
#if UNITY_EDITOR
			if( result == null )
			{
				Debug.LogWarning( "[ Asset.LodadSub Error ] " + path ) ;
			}
#endif
			return result ;
		}

		//---------------

		/// <summary>
		/// サブアセットを読み出す(非同期版)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path"></param>
		/// <param name="subAssetName"></param>
		/// <param name="onLoaded"></param>
		/// <param name="cachingType"></param>
		/// <param name="isNoDialog"></param>
		/// <returns></returns>
		public static async UniTask<T> LoadSubAsync<T>( string path, string subAssetName, CachingTypes cachingType = CachingTypes.None, bool isNoDialog = false ) where T : UnityEngine.Object
		{
			// 高速化のためまず同期ロードを行う
			T result = AssetBundleManager.LoadSubAsset<T>( path, subAssetName, ( AssetBundleManager.CachingTypes )cachingType ) ;
			if( result != null )
			{
				// 成功(awaitを実行させない)
				return result ;
			}

			// 非同期ロード
			UnityEngine.Object asset = await LoadSubAsync( path, subAssetName, typeof( T ), cachingType, isNoDialog ) ;
			return asset == null ? null : asset as T ;
		}

		/// <summary>
		/// サブアセットを読み出す(非同期版)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="subAssetName"></param>
		/// <param name="type"></param>
		/// <param name="onLoaded"></param>
		/// <param name="cachingType"></param>
		/// <param name="isNoDialog"></param>
		/// <returns></returns>
		public static async UniTask<UnityEngine.Object> LoadSubAsync( string path, string subAssetName, Type type, CachingTypes cachingType = CachingTypes.None, bool isNoDialog = false )
		{
			UnityEngine.Object asset ;

			// 高速化のためまず同期ロードを行う
			asset = AssetBundleManager.LoadSubAsset( path, subAssetName, type, ( AssetBundleManager.CachingTypes )cachingType ) ;
			if( asset != null )
			{
				// 成功(awaitを実行させない)
				return asset ;
			}

			// 非同期ロード
			while( true )
			{
				asset = null ;
				var assetRequest = AssetBundleManager.LoadSubAssetAsync( path, subAssetName, type, ( _ ) => { asset = _ ; }, ( AssetBundleManager.CachingTypes )cachingType, false ) ;
				await assetRequest ;
					
				if( asset != null )
				{
					// 成功
					return asset ;
				}
				else
				{
					// 失敗
					if( isNoDialog == false )
					{
						// エラーダイアログを表示する
						if( await OpenDownloadErrorDialog( assetRequest.ResultCode, path ) == true )
						{
							// コルーチン終了(シングルトンである AssetBundleManager のコルーチンスタックが残り続けてしまうため)
							return null ;
						}
						// Rebootしなければリトライする
					}
					else
					{
						// エラーダイアログを表示しない(そのままエラーとなる)
						return null ;
					}
				}
			}
		}

		//-----------------------------------------------------------

		// AllSubAssets

		/// <summary>
		/// 全てのサブアセットを読み出す(同期版)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path"></param>
		/// <param name="cachingType"></param>
		/// <returns></returns>
		public static T[] LoadAllSub<T>( string path, CachingTypes cachingType = CachingTypes.None ) where T : UnityEngine.Object
		{
			// 同期ロードで失敗してもエラーダイアログは表示されないので同期ロードは成功前提です
			T[] result = AssetBundleManager.LoadAllSubAssets<T>( path, ( AssetBundleManager.CachingTypes )cachingType ) ;
#if UNITY_EDITOR
			if( result == null )
			{
				Debug.LogWarning( "[ Asset.LodadAllSub<T> Error ] " + path ) ;
			}
#endif
			return result ;
		}

		/// <summary>
		/// 全てのサブアセットを読み出す(同期版)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="type"></param>
		/// <param name="cachingType"></param>
		/// <returns></returns>
		public static UnityEngine.Object[] LoadAllSub( string path, Type type, CachingTypes cachingType = CachingTypes.None )
		{
			// 同期ロードで失敗してもエラーダイアログは表示されないので同期ロードは成功前提です
			UnityEngine.Object[] result = AssetBundleManager.LoadAllSubAssets( path, type, ( AssetBundleManager.CachingTypes )cachingType ) ;
#if UNITY_EDITOR
			if( result == null )
			{
				Debug.LogWarning( "[ Asset.LodadSub Error ] " + path ) ;
			}
#endif
			return result ;
		}

		//---------------

		/// <summary>
		/// 全てのサブアセットを読み出す(非同期版)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path"></param>
		/// <param name="onLoaded"></param>
		/// <param name="cachingType"></param>
		/// <param name="isNoDialog"></param>
		/// <returns></returns>
		public static async UniTask<T[]> LoadAllSubAsync<T>( string path, CachingTypes cachingType = CachingTypes.None, bool isNoDialog = false ) where T : UnityEngine.Object
		{
			// 高速化のためまず同期ロードを行う
			T[] result = AssetBundleManager.LoadAllSubAssets<T>( path, ( AssetBundleManager.CachingTypes )cachingType ) ;
			if( result != null )
			{
				// 成功(awaitを実行させない)
				return result ;
			}

			// 非同期ロード
			UnityEngine.Object[] assets = await LoadAllSubAsync( path, typeof( T ), cachingType, isNoDialog ) ;
			if( assets == null )
			{
				return null ;
			}

			result = new T[ assets.Length ] ;
			for( int i  = 0 ; i <  assets.Length ; i ++ )
			{
				result[ i ] = assets[ i ] as T ;
			}

			return result ;
		}

		/// <summary>
		/// 全てのサブアセットを読み出す(非同期版)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="type"></param>
		/// <param name="onLoaded"></param>
		/// <param name="cachingType"></param>
		/// <param name="isNoDialog"></param>
		/// <param name="request"></param>
		/// <returns></returns>
		public static async UniTask<UnityEngine.Object[]> LoadAllSubAsync( string path, Type type, CachingTypes cachingType = CachingTypes.None, bool isNoDialog = false )
		{
			UnityEngine.Object[] assets = null ;

			// 高速化のためまず同期ロードを行う
			assets = AssetBundleManager.LoadAllSubAssets( path, type, ( AssetBundleManager.CachingTypes )cachingType ) ;
			if( assets != null )
			{
				// 成功(awaitを実行させない)
				return assets ;
			}

			// 非同期ロード
			while( true )
			{
				assets = null ;
				var assetRequest = AssetBundleManager.LoadAllSubAssetsAsync( path, type, ( _ ) => { assets = _ ; }, ( AssetBundleManager.CachingTypes )cachingType, false ) ;
				await assetRequest ;
	
				if( assets != null )
				{
					// 成功
					return assets ;
				}
				else
				{
					// 失敗
					if( isNoDialog == false )
					{
						// エラーダイアログを表示する
						if( await OpenDownloadErrorDialog( assetRequest.ResultCode, path ) == true )
						{
							// コルーチン終了(シングルトンである AssetBundleManager のコルーチンスタックが残り続けてしまうため)
							return null ;
						}
						// Rebootしなければリトライする
					}
					else
					{
						// エラーダイアログを表示しない(そのままエラーとなる)
						return null ;
					}
				}
			}
		}

		//-------------------------------------------------------------------------------------------

		// Scene

		/// <summary>
		/// シーンを展開する(非同期版)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="sceneName"></param>
		/// <param name="isNoDialog"></param>
		/// <returns></returns>
		public static async UniTask LoadSceneAsync( string path, string sceneName = null, bool isNoDialog = false )
		{
			await LoadOrAddSceneAsync_Private( path, null, null, sceneName, isNoDialog, 0 ) ;
		}

		/// <summary>
		/// シーンを展開する(非同期版)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path"></param>
		/// <param name="onLoaded"></param>
		/// <param name="targetName"></param>
		/// <param name="sceneName"></param>
		/// <param name="isNoDialog"></param>
		/// <returns></returns>
		public static async UniTask<T[]> LoadSceneAsync<T>( string path, string targetName = null, string sceneName = null, bool isNoDialog = false ) where T : UnityEngine.Object
		{
			UnityEngine.Object[] targets = await LoadOrAddSceneAsync_Private( path, typeof( T ), targetName, sceneName, isNoDialog, 0 ) ;
			if( targets == null )
			{
				return null ;
			}

			T[] result = new T[ targets.Length ] ;
			for( int i  = 0 ; i <  targets.Length ; i ++ )
			{
				result[ i ] = targets[ i ] as T ;
			}

			return result ;
		}

		/// <summary>
		/// シーンを展開する(非同期版)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="type"></param>
		/// <param name="onLoaded"></param>
		/// <param name="targetName"></param>
		/// <param name="sceneName"></param>
		/// <param name="isNoDialog"></param>
		/// <returns></returns>
		public static async UniTask<UnityEngine.Object[]> LoadSceneAsync( string path, Type type, string targetName = null, string sceneName = null, bool isNoDialog = false )
		{
			return await LoadOrAddSceneAsync_Private( path, type, targetName, sceneName, isNoDialog, 0 ) ;
		}

		//---------------

		/// <summary>
		/// シーンを加算する(非同期版)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="sceneName"></param>
		/// <param name="isNoDialog"></param>
		/// <returns></returns>
		public static async UniTask AddSceneAsync( string path, string sceneName = null, bool isNoDialog = false )
		{
			await LoadOrAddSceneAsync_Private( path, null, null, sceneName, isNoDialog, 1 ) ;
		}

		/// <summary>
		/// シーンを加算する(非同期版)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path"></param>
		/// <param name="onLoaded"></param>
		/// <param name="targetName"></param>
		/// <param name="sceneName"></param>
		/// <param name="isNoDialog"></param>
		/// <returns></returns>
		public static async UniTask<T[]> AddSceneAsync<T>( string path, string targetName = null, string sceneName = null, bool isNoDialog = false ) where T : UnityEngine.Object
		{
			UnityEngine.Object[] targets = await LoadOrAddSceneAsync_Private( path, typeof( T ), targetName, sceneName, isNoDialog, 1 ) ;
			if( targets == null )
			{
				return null ;
			}

			T[] result = new T[ targets.Length ] ;
			for( int i  = 0 ; i <  targets.Length ; i ++ )
			{
				result[ i ] = targets[ i ] as T ;
			}

			return result ;
		}

		/// <summary>
		/// シーンを加算する(非同期版)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="type"></param>
		/// <param name="onLoaded"></param>
		/// <param name="targetName"></param>
		/// <param name="sceneName"></param>
		/// <param name="isNoDialog"></param>
		/// <returns></returns>
		public static async UniTask<UnityEngine.Object[]> AddSceneAsync( string path, Type type, string targetName = null, string sceneName = null, bool isNoDialog = false )
		{
			return await LoadOrAddSceneAsync_Private( path, type, targetName, sceneName, isNoDialog, 0 ) ;
		}

		//-------------------------------------------

		/// <summary>
		/// シーンを展開または加算する(非同期版)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="type"></param>
		/// <param name="onLoaded"></param>
		/// <param name="targetName"></param>
		/// <param name="sceneName"></param>
		/// <param name="isNoDialog"></param>
		/// <param name="mode"></param>
		/// <returns></returns>
		private static async UniTask<UnityEngine.Object[]> LoadOrAddSceneAsync_Private( string path, Type type, string targetName, string sceneName, bool isNoDialog, int mode )
		{
			if( AssetBundleManager.Instance == null )
			{
				return null ;
			}

			while( true )
			{
				UnityEngine.Object[] targets = null ;
				AssetBundleManager.Request assetRequest ;
				if( mode == 0 )
				{
					// Load
					assetRequest = AssetBundleManager.LoadSceneAsync( path, type, ( _ ) => { targets = _ ; }, targetName, sceneName, false ) ;
				}
				else
				{
					// Add
					assetRequest =  AssetBundleManager.AddSceneAsync( path, type, ( _ ) => { targets = _ ; }, targetName, sceneName, false ) ;
				}
				await assetRequest ;
	
				if( targets != null )
				{
					// 成功
					return targets ;
				}
				else
				{
					// 失敗
					if( isNoDialog == false )
					{
						// エラーダイアログを表示する
						if( await OpenDownloadErrorDialog( assetRequest.ResultCode, path ) == true )
						{
							// コルーチン終了(シングルトンである AssetBundleManager のコルーチンスタックが残り続けてしまうため)
							return null ;
						}
						// Rebootしなければリトライする
					}
					else
					{
						// エラーダイアログを表示しない(そのままエラーとなる)
						return null ;
					}
				}
			}
		}

		//-------------------------------------------------------------------------------------------

		// AssetBundle

		/// <summary>
		/// アセットバンドルをダウンロードする
		/// </summary>
		/// <param name="path"></param>
		/// <param name="keep"></param>
		/// <param name="isNoDialog"></param>
		/// <param name="onProgress"></param>
		/// <returns></returns>
		public static async UniTask<bool> DownloadAssetBundleAsync( string path, bool keep = false, Action<int,float,float> onProgress = null, bool isNoDialog = false )
		{
			// リトライループ
			while( true )
			{
				string message = string.Empty ;
				await m_Instance.When( AssetBundleManager.DownloadAssetBundleAsync
				(
					path,
					keep,
					onProgress,
					( string error ) =>
					{
						message = error ;
					}
				) ) ;

				if( string.IsNullOrEmpty( message ) == true )
				{
					// 成功
					return true ;
				}
				else
				{
					// 失敗
					if( isNoDialog == false )
					{
						// エラーダイアログを表示する
						if( await OpenDownloadErrorDialog( 0, path ) == true )
						{
							// コルーチン終了(シングルトンである AssetBundleManager のコルーチンスタックが残り続けてしまうため)
							return false ;
						}
						// Rebootしなければリトライする
					}
					else
					{
						// エラーダイアログを表示しない(そのままエラーとなる)
						return false ;
					}
				}
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 複数のアセットバンドルをまとめてダウンロードする
		/// </summary>
		/// <param name="path"></param>
		/// <param name="keep"></param>
		/// <param name="isNoDialog"></param>
		/// <param name="onProgress"></param>
		/// <returns></returns>
		public static async UniTask<bool> DownloadAssetBundlesAsync
		(
			AssetBundleManager.DownloadEntity[] entities,
			int parallel = 0,
			Action<long,int,int,int> onProgress = null,
			bool isNoDialog = false
		)
		{
			// リトライループ
			while( true )
			{
				string message = string.Empty ;
				await m_Instance.When( AssetBundleManager.DownloadAssetBundlesAsync
				(
					entities,
					parallel,
					onProgress,
					( string error ) =>
					{
						message = error ;
					}
				) ) ;

				if( string.IsNullOrEmpty( message ) == true )
				{
					// 成功
					return true ;
				}
				else
				{
					// 失敗
					if( isNoDialog == false )
					{
						// エラーダイアログを表示する
						if( await OpenDownloadErrorDialog( 0, message ) == true )
						{
							// コルーチン終了(シングルトンである AssetBundleManager のコルーチンスタックが残り続けてしまうため)
							return false ;
						}
						// Rebootしなければリトライする
					}
					else
					{
						// エラーダイアログを表示しない(そのままエラーとなる)
						return false ;
					}
				}
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// アセットバンドルを破棄する(ストレージキャッシュから)
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static bool DeleteAssetBundleFromStorageCache( string path )
		{
			return AssetBundleManager.DeleteAssetBundleFromStorageCache( path ) ;
		}

		/// <summary>
		/// アセットバンドルをメモリに展開する(同期)
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static bool LoadAssetBundle( string path, bool isRetain = false )
		{
			return AssetBundleManager.LoadAssetBundle( path, isRetain ) ;
		}

		/// <summary>
		/// アセットバンドルをメモリに展開する(非同期)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="keep"></param>
		/// <param name="isNoDialog"></param>
		/// <param name="onProgress"></param>
		/// <returns></returns>
		public static async UniTask<bool> LoadAssetBundleAsync( string path, bool isRetain = false, bool keep = false, bool isNoDialog = false, Action<float> onProgress = null )
		{
			float progress = 0 ;
			while( true )
			{
				var assetRequest = AssetBundleManager.LoadAssetBundleAysnc( path, isRetain, keep ) ;
				while( true )
				{
					if( progress != assetRequest.Progress )
					{
						onProgress?.Invoke( assetRequest.Progress ) ;
						progress  = assetRequest.Progress ;
					}

					if( assetRequest.IsDone == true || string.IsNullOrEmpty( assetRequest.Error ) == false )
					{
						break ;	// 終了
					}

					await m_Instance.Yield() ;
				}

				if( assetRequest.IsDone == true )
				{
					// 成功
					return true ;
				}
				else
				{
					// 失敗
					if( isNoDialog == false )
					{
						// エラーダイアログを表示する
						if( await OpenDownloadErrorDialog( assetRequest.ResultCode, path ) == true )
						{
							// コルーチン終了(シングルトンである AssetBundleManager のコルーチンスタックが残り続けてしまうため)
							return false ;
						}
						// Rebootしなければリトライする
					}
					else
					{
						// エラーダイアログを表示しない(そのままエラーとなる)
						return false ;
					}
				}
			}
		}

		/// <summary>
		/// アセットバンドルを破棄する(メモリキャッシュから)
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static bool FreeAssetBundle( string path )
		{
			return AssetBundleManager.FreeAssetBundle( path ) ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// アセットバンドルが管理対象に含まれているか確認する
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static bool Contains( string path )
		{
			return AssetBundleManager.Contains( path ) ;
		}

		/// <summary>
		/// 指定のパスにアセットバンドルファイルが存在するか確認する
		/// </summary>
		/// <param name="path">パス</param>
		/// <returns></returns>
		public static bool Exists( string path )
		{
			return AssetBundleManager.Exists( path ) ;
		}

		/// <summary>
		/// 指定のパスにアセットバンドルファイルのサイズを取得する
		/// </summary>
		/// <param name="path">パス</param>
		/// <returns></returns>
		public static int GetSize( string path )
		{
			return AssetBundleManager.GetSize( path ) ;
		}

		/// <summary>
		/// 環境パスを取得する
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string GetNativePath( string path )
		{
			return AssetBundleManager.GetAssetNativePath( path ) ;
		}

		/// <summary>
		/// アセットバンドルのパス群を取得する
		/// </summary>
		/// <param name="updateRequiredOnly"></param>
		/// <returns></returns>
		public static string[] GetAllAssetBundlePaths( bool updateRequiredOnly = false )
		{
			return AssetBundleManager.GetAllAssetBundlePaths( updateRequiredOnly ) ;
		}

		/// <summary>
		/// 依存関係にあるアセットバンドルのパス群を取得する
		/// </summary>
		/// <param name="assetBundlePath"></param>
		/// <param name="updateRequiredOnly"></param>
		/// <returns></returns>
		public static string[] GetAllDependentAssetBundlePaths( string assetBundlePath, bool updateRequiredOnly = false )
		{
			return AssetBundleManager.GetAllDependentAssetBundlePaths( assetBundlePath, updateRequiredOnly ) ;
		}

		//-------------------------------------------------------------------------------------------

		// ダウンロードエラーダイアログを表示する
		public static async UniTask<bool> OpenDownloadErrorDialog( int resultCode, string path )
		{
			string title = "アセット取得エラー" ;
			string message = "データのダウンロードに失敗しました" ;

			bool showDetail = false ;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			showDetail = true ;
#endif

			if( Define.DevelopmentMode == true )
			{
				showDetail = true ;
			}

			if( showDetail == true )
			{
				message += ( "\n\n" + path ) ;

				message += "\n\n" ;

				if( resultCode ==  1 )
				{
					message += "名前が不正" ;
				}
				else
				if( resultCode ==  2 )
				{
					message += "リモートのファイルが存在しないかデータが不正" ;
				}
				else
				if( resultCode ==  3 )
				{
					message += "ローカルストレージ足らず保存失敗" ;
				}
				else
				{
					message += "エラー原因が不明" ;
				}
			}

			//----------------------------------------------------------

			bool executeReboot		= false ;	// リブートする場合のリブート後の開始シーン名

			// ダイアログを出すためプログレス表示中であれば消去する
			Progress.Hide() ;

			// フローを辿りたいのでログにも出力する
			Debug.LogWarning( message ) ;

			// ダイアログを表示する
			await Dialog.Open
			(
				$"<color=#FFFF00>{title}</color>",
				message,
				new string[]{ "リトライ", "リブート" }, 
				( int result ) =>
				{
					if( result == 1 )
					{
						executeReboot = true ;
					}
				}
			) ;

			//----------------------------------------------------------

			if( executeReboot == true )
			{
				// リブートを行う

				//---------------------------------

				Progress.IsOn = false ;	// プログレスの継続表示状態になっていればリセットする

				// リブートする
				ApplicationManager.Reboot() ;

				// 完全にシーンの切り替えが終了する(古いシーンが破棄される)のを待つ(でないと古いシーンが悪さをする)
				await m_Instance.WaitWhile( () => Scene.IsFading == true ) ;

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}
			else
			{
				// リトライを行う

				if( Progress.IsOn == true )
				{
					// プログレス表示要請での通信であるため再度プログレスを表示する
					Progress.Show() ;
				}
				return false ;
			}
		}

		//-------------------------------------------------------------------------------------------

		//-------------------------------------------------------------------------------------------

		// 切りが無いので AssetBundleManager のメソッドをそのまま使えば良いものはそのまま使う事。
		// 失敗した際にダイアログを表示する等の挙動が必要な場合にのみ上記のメソッド群のようにラッパーメソッドを用意すべし

		/// <summary>
		/// メモリリソースキャッシュをクリアする
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool ClearResourceCache( bool noMarkingOnly, string message = "" )
		{
			Debug.Log( "<color=#00FFFF>[AssetBundleManager] シーン遷移によりキャッシュをクリアします : " + message + "</color>" ) ;
			return AssetBundleManager.ClearResourceCache( noMarkingOnly, false ) ;
		}
		
		/// <summary>
		/// ローカルストレージ内の指定したマニフェストのアセットバンドルを全て消去する
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Cleanup( string manifestName = null )
		{
			return AssetBundleManager.Cleanup( manifestName ) ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// アトラスタイプのスプライトを読み出す(同期)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="chachingType"></param>
		/// <param name="isNoDialog"></param>
		/// <returns></returns>
		[Obsolete("Use LoadAtlas()")]
		public static Dictionary<string,Sprite> LoadSpriteSet( string path, CachingTypes cachingType = CachingTypes.None )
		{
			Sprite[] sprites = Asset.LoadAllSub<Sprite>( path, cachingType ) ;
			if( sprites == null || sprites.Length == 0 )
			{
				return null ;
			}

			Dictionary<string,Sprite> spriteSet = new Dictionary<string, Sprite>() ;
			foreach( var sprite in sprites )
			{
				if( spriteSet.ContainsKey( sprite.name ) == false )
				{
					spriteSet.Add( sprite.name, sprite ) ;
				}
				else
				{
					spriteSet[ sprite.name ] = sprite ;	// 上書き
				}
			}

			return spriteSet ;
		}

		/// <summary>
		/// アトラスタイプのスプライトを読み出す(非同期)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="chachingType"></param>
		/// <param name="isNoDialog"></param>
		/// <returns></returns>
		[Obsolete("Use LoadAtlasAsync()")]
		public static async UniTask<Dictionary<string,Sprite>> LoadSpriteSetAsync( string path, CachingTypes cachingType = CachingTypes.None, bool isNoDialog = false )
		{
			Sprite[] sprites = await Asset.LoadAllSubAsync<Sprite>( path, cachingType, isNoDialog ) ;
			if( sprites == null || sprites.Length == 0 )
			{
				return null ;
			}

			Dictionary<string,Sprite> spriteSet = new Dictionary<string, Sprite>() ;
			foreach( var sprite in sprites )
			{
				if( spriteSet.ContainsKey( sprite.name ) == false )
				{
					spriteSet.Add( sprite.name, sprite ) ;
				}
				else
				{
					spriteSet[ sprite.name ] = sprite ;	// 上書き
				}
			}

			return spriteSet ;
		}

		//---------------

		/// <summary>
		/// アトラスタイプのスプライトを読み出す(同期)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="chachingType"></param>
		/// <param name="isNoDialog"></param>
		/// <returns></returns>
		public static Dictionary<string,Sprite> LoadAtlas( string path, CachingTypes cachingType = CachingTypes.None )
		{
			SpriteAtlas spriteAtlas = Asset.Load<SpriteAtlas>( path, cachingType ) ;
			if( spriteAtlas == null || spriteAtlas.spriteCount == 0 )
			{
				return null ;
			}

			int count = spriteAtlas.spriteCount ;
			Sprite[] sprites = new Sprite[ count ] ;
			spriteAtlas.GetSprites( sprites ) ;

			Dictionary<string,Sprite> spriteSet = new Dictionary<string, Sprite>() ;
			foreach( var sprite in sprites )
			{
				string spriteName = sprite.name.Replace( "(Clone)", "" ) ;

				if( spriteSet.ContainsKey( spriteName ) == false )
				{
					spriteSet.Add( spriteName, sprite ) ;
				}
				else
				{
					spriteSet[ spriteName ] = sprite ;	// 上書き
				}
			}

			return spriteSet ;
		}

		/// <summary>
		/// アトラスタイプのスプライトを読み出す(非同期)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="chachingType"></param>
		/// <param name="isNoDialog"></param>
		/// <returns></returns>
		public static async UniTask<Dictionary<string,Sprite>> LoadAtlasAsync( string path, CachingTypes cachingType = CachingTypes.None, bool isNoDialog = false )
		{
			SpriteAtlas spriteAtlas = await Asset.LoadAsync<SpriteAtlas>( path, cachingType, isNoDialog ) ;
			if( spriteAtlas == null || spriteAtlas.spriteCount == 0 )
			{
				return null ;
			}

			int count = spriteAtlas.spriteCount ;
			Sprite[] sprites = new Sprite[ count ] ;
			spriteAtlas.GetSprites( sprites ) ;

			Dictionary<string,Sprite> spriteSet = new Dictionary<string, Sprite>() ;
			foreach( var sprite in sprites )
			{
				string spriteName = sprite.name.Replace( "(Clone)", "" ) ; 

				if( spriteSet.ContainsKey( spriteName ) == false )
				{
					spriteSet.Add( spriteName, sprite ) ;
				}
				else
				{
					spriteSet[ spriteName ] = sprite ;	// 上書き
				}
			}

			return spriteSet ;
		}
	}
}
