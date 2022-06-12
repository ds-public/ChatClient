using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

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
		/// 指定のマニフェストが使用可能になったどうか
		/// </summary>
		/// <param name="manifestName"></param>
		/// <returns></returns>
		public static bool IsManifestCompleted( string manifestName )
		{
			ManifestInfo manifestInfo = GetManifest( manifestName ) ;
			if( manifestInfo == null )
			{
				return false ;
			}

			return manifestInfo.Completed ;
		}


		/// <summary>
		/// 全てのマニフェストが使用可能になったかどうかを示す
		/// </summary>
		public static bool IsAllManifestsCompleted
		{
			get
			{
				// 解りにくくなるので簡略化はしない
				return m_Instance != null && m_Instance.IsAllManifestsCompleted_Private ;
			}
		}

		// 全てのマニフェストが使用可能になったかどうかを示す
		private bool IsAllManifestsCompleted_Private
		{
			get
			{
				return ! m_ManifestInfo.Any( _ => _.Completed == false ) ;
			}
		}

		//-------------------------------------------------------------------

		/// <summary>
		/// 全マニフェスト情報の高速アクセス用のハッシュリスト
		/// </summary>
		internal protected Dictionary<string,ManifestInfo> m_ManifestHash = new Dictionary<string, ManifestInfo>() ;

		// マニフェストハッシュを更新する
		private void UpdateManifestHash()
		{
			if( m_ManifestHash == null )
			{
				m_ManifestHash = new Dictionary<string, ManifestInfo>() ;
			}
			else
			{
				m_ManifestHash.Clear() ;
			}

			if( m_ManifestInfo == null || m_ManifestInfo.Count == 0 )
			{
				return ;
			}

			foreach( var manifestInfo in m_ManifestInfo )
			{
				if( m_ManifestHash.ContainsKey( manifestInfo.ManifestName ) == false )
				{
					m_ManifestHash.Add( manifestInfo.ManifestName, manifestInfo ) ;
				}
			}
		}

		//-------------------------------------------------------------------
		
		/// <summary>
		/// マニフェストがローディング中かどうかを示す
		/// </summary>
		public static bool IsAnyManifestLoading
		{
			get
			{
				// 解りにくくなるので簡略化はしない
				return m_Instance != null && m_Instance.m_IsAnyManifestLoading ;
			}
		}

		// マニフェストがローディング中かどうかを示す
		private bool m_IsAnyManifestLoading = false ;

		//-------------------------------------------------------------------

		// マニフェスト全体情報の更新時の排他制御用フラグ
		private bool m_Busy = false ;

		/// <summary>
		/// 指定のマニフェストが登録されているか確認する
		/// </summary>
		/// <param name="manifestName">マニフェスト名</param>
		/// <returns>マニフェストの登録リスト上でのインデックス番号(-1で未登録)</returns>
		public static int IsManifest( string manifestName )
		{
			return m_Instance == null ? -1 : m_Instance.IsManifest_Private( manifestName ) ;
		}

		// 指定のマニフェストが登録されているか確認する
		private int IsManifest_Private( string manifestName )
		{
			if( m_ManifestInfo == null || string.IsNullOrEmpty( manifestName ) == true )
			{
				return -1 ;
			}

			return m_ManifestInfo.FindIndex( _ => _.ManifestName == manifestName ) ;
		}

		/// <summary>
		/// マニフェストを登録する
		/// </summary>
		/// <param name="manifestFileName"></param>
		/// <param name="localAssetBundleRootPath"></param>
		/// <param name="streamingAssetsRootPath"></param>
		/// <param name="remoteAssetBundleRootPath"></param>
		/// <param name="cacheSize"></param>
		/// <returns></returns>
		public static ManifestInfo AddManifest
		(
			string	manifestName, bool crcOnly, string storageCacheRootPath,
			string	streamingAssetsRootPath,
			bool	streamingAssetsDirectAccessEnabled,
			string	remoteAssetBundleRootPath,
			string	localAssetBundleRootPath,
			string	localAssetsRootPath,
			LocationTypes locationType,
			long cacheSize = 0L, bool fastLoadEnabled = true
		)
		{
			if( m_Instance == null )
			{
				return null ;
			}
			return m_Instance.AddManifest_Private
			(
				manifestName, crcOnly, storageCacheRootPath,
				streamingAssetsRootPath,
				streamingAssetsDirectAccessEnabled,
				remoteAssetBundleRootPath,
				localAssetBundleRootPath,
				localAssetsRootPath,
				locationType,
				cacheSize, fastLoadEnabled
			) ;
		}

		// マニフェストを登録する
		private ManifestInfo AddManifest_Private
		(
			string	manifestName, bool crcOnly, string storageCacheRootPath,
			string	streamingAssetsRootPath,
			bool	streamingAssetsDirectAccessEnabled,
			string	remoteAssetBundleRootPath,
			string	localAssetBundleRootPath,
			string	localAssetsRootPath,
			LocationTypes	locationType,
			long cacheSize, bool fastLoadEnabled
		)
		{
			if( m_ManifestInfo == null || string.IsNullOrEmpty( manifestName ) == true )
			{
				return null ;
			}

			var manifestInfo = m_ManifestInfo.FirstOrDefault( _ => _.ManifestName == manifestName ) ;
			if( manifestInfo == null )
			{
				// 未登録(新規に登録する)
				manifestInfo = new ManifestInfo() ;
				manifestInfo.Setup
				(
					manifestName, crcOnly, storageCacheRootPath,
					streamingAssetsRootPath,
					streamingAssetsDirectAccessEnabled,
					remoteAssetBundleRootPath,
					localAssetBundleRootPath,
					localAssetsRootPath,
					locationType,
					cacheSize, fastLoadEnabled
				) ;

				m_ManifestInfo.Add( manifestInfo ) ;
				UpdateManifestHash() ;
			}
			else
			{
				// 登録済(キャッシュサイズとロードモードを更新する)
//				manifestInfo.StorageCacheRootPath		= storageCacheRootPath ;
//				manifestInfo.LocalAssetBundleRootPath	= localAssetBundleRootPath ;
//				manifestInfo.StreamingAssetsRootPath	= streamingAssetsRootPath ;
//				manifestInfo.RemoteAssetBundleRootPath	= remoteAssetBundleRootPath ;
				manifestInfo.CacheSize					= cacheSize ;
				manifestInfo.FastLoadEnabled			= fastLoadEnabled ;
			}

			return manifestInfo ;
		}

		/// <summary>
		/// マニフェストを取得する
		/// </summary>
		/// <param name="manifestName"></param>
		/// <returns></returns>
		public static ManifestInfo GetManifest( string manifestName )
		{
			if( m_Instance == null )
			{
				return null ;
			}
			return m_Instance.GetManifest_Private( manifestName ) ;
		}

		// マニフェストを取得する
		private ManifestInfo GetManifest_Private( string manifestName )
		{
			if( m_ManifestInfo == null || string.IsNullOrEmpty( manifestName ) == true )
			{
				return null ;
			}

			// 大文字小文字の区別無しで取得する
			manifestName = manifestName.ToLower() ;
			return m_ManifestInfo.FirstOrDefault( _ => _.ManifestName.ToLower() == manifestName ) ;
		}		

		/// <summary>
		/// マニフェストを削除する
		/// </summary>
		/// <param name="manifestName"></param>
		/// <returns></returns>
		public static bool RemoveManifest( string manifestName )
		{
			// 解りにくくなるので簡略化はしない
			return m_Instance != null && m_Instance.RemoveManifest_Private( manifestName ) ;
		}

		// マニフェストを削除する
		private bool RemoveManifest_Private( string manifestName )
		{
			if( m_ManifestInfo == null || string.IsNullOrEmpty( manifestName ) == true )
			{
				return false ;
			}

			// 大文字小文字の区別無しで取得する
			// 大文字小文字の区別無しで取得する
			manifestName = manifestName.ToLower() ;
			var manifestInfo = m_ManifestInfo.FirstOrDefault( _ => _.ManifestName.ToLower() == manifestName ) ;
			if( manifestInfo == null )
			{
				return false ;
			}

			m_ManifestInfo.Remove( manifestInfo ) ;
			UpdateManifestHash() ;

			return true ;
		}

		/// <summary>
		/// 全マニフェストを削除する
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool ClearManifest()
		{
			// 解りにくくなるので簡略化はしない
			return m_Instance != null && m_Instance.ClearManifest_Private() ;
		}

		// 全マニフェストを削除する
		private bool ClearManifest_Private()
		{
			if( m_ManifestInfo == null )
			{
				return false ;
			}

			foreach( var manifestInfo in m_ManifestInfo )
			{
				manifestInfo.ClearAssetBundleCache( true, false ) ;
			}

			m_ManifestInfo.Clear() ;
			UpdateManifestHash() ;

			return true ;
		}

		/// <summary>
		/// マニフェストの展開でエラーが発生していれば取得する
		/// </summary>
		/// <param name="oManifestName"></param>
		/// <param name="oError"></param>
		/// <returns></returns>
		public static bool GetAnyManifestError( out string oManifestName, out string oError )
		{
			oManifestName	= null ;
			oError			= null ;

			// 解りにくくなるので簡略化はしない
			return m_Instance != null && m_Instance.GetAnyManifestError_Private( out oManifestName, out oError ) ;
		}
		
		// マニフェストの展開でエラーが発生していれば取得する
		private bool GetAnyManifestError_Private( out string oManifestName, out string oError )
		{
			oManifestName	= null ;
			oError			= null ;

			var manifestInfo = m_ManifestInfo.FirstOrDefault( _ => string.IsNullOrEmpty( _.Error ) == false ) ;
			if( manifestInfo == null )
			{
				return false ;
			}

			oManifestName	= manifestInfo.ManifestName ;
			oError			= manifestInfo.Error ;

			return true ;
		}

		//------------------------------------

		/// <summary>
		/// 登録されたマニフェストを全て展開する(同期版)
		/// </summary>
		public static Request LoadAllManifestsAsync()
		{
			if( m_Instance == null )
			{
				return null ;
			}

			Request request = new Request( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.LoadAllManifestsAsync_Private( request ) ) ;
			return request ;
		}

		// 登録されたマニフェストを全て展開する(非同期版)
		private IEnumerator LoadAllManifestsAsync_Private( Request request )
		{
			m_IsAnyManifestLoading = true ;

			foreach( var manifestInfo in m_ManifestInfo )
			{
				Request subRequest = new Request( this ) ;
				yield return StartCoroutine( LoadManifestAsync_Private( manifestInfo, subRequest, true ) ) ;
				if( string.IsNullOrEmpty( subRequest.Error ) == false )
				{
					request.Error = subRequest.Error ;
					break ;
				}
			}

			m_IsAnyManifestLoading = false ;
			request.IsDone = true ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// マニフェストを展開する(登録されていなければ登録する)(非同期版)
		/// </summary>
		/// <param name="manifestFileName"></param>
		/// <param name="localAssetBundleRootPath"></param>
		/// <param name="streamingAssetsRootPath"></param>
		/// <param name="remoteAssetBundleRootPath"></param>
		/// <param name="cacheSize"></param>
		/// <returns></returns>
		public static Request LoadManifestAsync
		(
			string	manifestName, bool crcOnly, string storageCacheRootPath,
			string	streamingAssetsRootPath,
			bool	streamingAssetsDirectAccessEnabled,
			string	remoteAssetBundleRootPath,
			string	localAssetBundleRootPath,
			string	localAssetsRootPath,
			LocationTypes	locationType,
			long	cacheSize = 0L, bool fastLoadEnabled = true
		)
		{
			if( m_Instance == null )
			{
				return null ;
			}

			Request request = new Request( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.LoadManifestAsync_Private
			(
				manifestName, crcOnly, storageCacheRootPath,
				streamingAssetsRootPath,
				streamingAssetsDirectAccessEnabled,
				remoteAssetBundleRootPath,
				localAssetBundleRootPath,
				localAssetsRootPath,
				locationType,
				cacheSize, fastLoadEnabled, request
			) ) ;
			return request ;
		}

		// マニフェストを展開する(登録されていなければ登録する)(非同期版)
		private IEnumerator LoadManifestAsync_Private
		(
			string	manifestName, bool crcOnly, string storageCacheRootPath,
			string	streamingAssetsRootPath,
			bool	streamingAssetsDirectAccessEnabled,
			string	remoteAssetBundleRootPath,
			string	localAssetBundleRootPath,
			string	localAssetsRootPath,
			LocationTypes	locationType,
			long	cacheSize, bool fastLoadEnabled, Request request
		)
		{
			var manifestInfo = AddManifest_Private
			(
				manifestName, crcOnly, storageCacheRootPath,
				streamingAssetsRootPath,
				streamingAssetsDirectAccessEnabled,
				remoteAssetBundleRootPath,
				localAssetBundleRootPath,
				localAssetsRootPath,
				locationType,
				cacheSize, fastLoadEnabled
			) ;

			if( manifestInfo == null )
			{
				// 何故か失敗した(基本的にありえない)
				request.Error = "Could not load" ;
				yield break ;
			}

			yield return StartCoroutine( LoadManifestAsync_Private( manifestInfo, request, false ) ) ;
		}

		/// <summary>
		/// マニフェストを展開する(登録されていなければ登録する)(非同期版)
		/// </summary>
		/// <param name="manifestFileName"></param>
		/// <returns></returns>
		public static Request LoadManifestAsync( string manifestName )
		{
			if( m_Instance == null )
			{
				return null ;
			}

			ManifestInfo mainfestInfo = GetManifest( manifestName ) ;
			if( mainfestInfo == null )
			{
				return null ;
			}

			Request request = new Request( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.LoadManifestAsync_Private( mainfestInfo, request, false ) ) ;
			return request ;
		}

		//-----------------------------------------------------------

		// マニフェストのダウンロードと展開を行う　※共通処理
		private IEnumerator LoadManifestAsync_Private( ManifestInfo manifestInfo, Request request, bool isAll )
		{
			if( isAll == false )
			{
				m_IsAnyManifestLoading = true ;
			}

			// 実際のダウンロードと展開を行う
			bool isCompleted = false ;
			string error = string.Empty ;
			yield return StartCoroutine( manifestInfo.LoadAsync( () => { isCompleted = true ; }, ( _ ) => { error = _ ; }, this ) ) ;
			if( isCompleted = false || string.IsNullOrEmpty( error ) == false )
			{
				// 失敗
				if( isAll == false )
				{
					m_IsAnyManifestLoading = false ;
				}

				if( string.IsNullOrEmpty( error ) == true )
				{
					error = "Could not load." ;
				}
				request.Error = error ;
				yield break ;
			}

			//----------------------------------------------------------

			// マニフェストリストのタイムスタンプを更新する(排他的にする)

			yield return new WaitWhile( () => m_Busy == true ) ;

			m_Busy = true ;

			UpdateManifestHash() ;

			m_Busy = false ;

			if( isAll == false )
			{
				m_IsAnyManifestLoading = false ;
			}

			//----------------------------------------------------------

			request.IsDone = true ;
		}

		//-------------------------------------------------------------------

		/// <summary>
		/// マニフェストのアセットパンドルのパス一覧を取得する
		/// </summary>
		/// <param name="updateRequiredOnly"></param>
		/// <returns></returns>
		public static string[] GetAllAssetBundlePaths( bool updateRequiredOnly = false )
		{
			if( m_Instance == null )
			{
				return null ;
			}
			return m_Instance.GetAllAssetBundlePaths_Private( m_Instance.m_DefaultManifestName, updateRequiredOnly ) ;
		}

		/// <summary>
		/// マニフェストのアセットパンドルのパス一覧を取得する
		/// </summary>
		/// <param name="manifestName"></param>
		/// <param name="updateRequiredOnly"></param>
		/// <returns></returns>
		public static string[] GetAllAssetBundlePaths( string manifestName, bool updateRequiredOnly = false )
		{
			if( m_Instance == null )
			{
				return null ;
			}
			return m_Instance.GetAllAssetBundlePaths_Private( manifestName, updateRequiredOnly ) ;
		}

		// マニフェストのアセットパンドルのパス一覧を取得する
		private string[] GetAllAssetBundlePaths_Private( string manifestName, bool updateRequiredOnly )
		{
			if( string.IsNullOrEmpty( manifestName ) == true || m_ManifestInfo == null || m_ManifestInfo.Count == 0 )
			{
				// Use Local Assets が有効な場合は、マニフェスト情報を展開していないため、アセットバンドルのパス情報自体が取得できない。
				return null ;
			}

			manifestName = manifestName.ToLower() ;
			var manifestInfo = m_ManifestInfo.FirstOrDefault( _ => _.ManifestName.ToLower() == manifestName ) ;
			if( manifestInfo == null )
			{
				return null ;
			}

			// アセットバンドルのパス一覧を取得する
			return manifestInfo.GetAllAssetBundlePaths( updateRequiredOnly ) ;
		}

		//---------------------------

		/// <summary>
		/// マニフェストのアセットパンドルのパス一覧を取得する
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="updateRequiredOnly"></param>
		/// <param name="isDependency"></param>
		/// <returns></returns>
		public static string[] GetAllAssetBundlePathsWithTag( string tag, bool updateRequiredOnly = false, bool isDependency = false )
		{
			return GetAllAssetBundlePathsWithTags( new string[]{ tag }, updateRequiredOnly, isDependency ) ;
		}
		public static string[] GetAllAssetBundlePathsWithTags( string[] tags, bool updateRequiredOnly = false, bool isDependency = false )
		{
			if( m_Instance == null )
			{
				return null ;
			}
			return m_Instance.GetAllAssetBundlePathsWithTags_Private( m_Instance.m_DefaultManifestName, tags, updateRequiredOnly, isDependency ) ;
		}

		/// <summary>
		/// マニフェストのアセットパンドルのパス一覧を取得する
		/// </summary>
		/// <param name="manifestName"></param>
		/// <param name="tag"></param>
		/// <param name="updateRequiredOnly"></param>
		/// <param name="isDependency"></param>
		/// <returns></returns>
		public static string[] GetAllAssetBundlePathsWithTag( string manifestName, string tag, bool updateRequiredOnly = false, bool isDependency = false )
		{
			return GetAllAssetBundlePathsWithTags( manifestName, new string[]{ tag }, updateRequiredOnly, isDependency ) ;
		}
		public static string[] GetAllAssetBundlePathsWithTags( string manifestName, string[] tags, bool updateRequiredOnly = false, bool isDependency = false )
		{
			if( m_Instance == null )
			{
				return null ;
			}
			return m_Instance.GetAllAssetBundlePathsWithTags_Private( manifestName, tags, updateRequiredOnly, isDependency ) ;
		}

		// マニフェストのアセットパンドルのパス一覧を取得する
		private string[] GetAllAssetBundlePathsWithTags_Private( string manifestName, string[] tags, bool updateRequiredOnly, bool isDependency )
		{
			if( string.IsNullOrEmpty( manifestName ) == true || m_ManifestInfo == null || m_ManifestInfo.Count == 0 )
			{
				return null ;
			}

			manifestName = manifestName.ToLower() ;
			var manifestInfo = m_ManifestInfo.FirstOrDefault( _ => _.ManifestName.ToLower() == manifestName ) ;
			if( manifestInfo == null )
			{
				return null ;
			}

			// アセットバンドルのパス一覧を取得する
			return manifestInfo.GetAllAssetBundlePathsWithTags( tags, updateRequiredOnly, isDependency ) ;
		}

		//-------------------------------------------------------------------

		/// <summary>
		/// 依存関係にあるアセットバンドルのパス一覧を取得する
		/// </summary>
		/// <param name="assetBundleName"></param>
		/// <param name="updateRequiredOnly"></param>
		/// <returns></returns>
		public static string[] GetAllDependentAssetBundlePaths( string assetBundleName, bool updateRequiredOnly = false )
		{
			if( m_Instance == null )
			{
				return null ;
			}
			return m_Instance.GetAllDependentAssetBundlePaths_Private( m_Instance.m_DefaultManifestName, assetBundleName, updateRequiredOnly ) ;
		}

		/// <summary>
		/// 依存関係にあるアセットパンドルのパス一覧を取得する
		/// </summary>
		/// <param name="manifestName"></param>
		/// <param name="assetBundleName"></param>
		/// <param name="updateRequiredOnly"></param>
		/// <returns></returns>
		public static string[] GetAllDependentAssetBundlePaths( string manifestName, string assetBundleName, bool updateRequiredOnly = false )
		{
			if( m_Instance == null )
			{
				return null ;
			}
			return m_Instance.GetAllDependentAssetBundlePaths_Private( manifestName, assetBundleName, updateRequiredOnly ) ;
		}

		// マニフェストのアセットパンドルのパス一覧を取得する
		private string[] GetAllDependentAssetBundlePaths_Private( string manifestName, string assetBundleName, bool updateRequiredOnly )
		{
			if( string.IsNullOrEmpty( manifestName ) == true || m_ManifestInfo == null || m_ManifestInfo.Count == 0 )
			{
				return null ;
			}

			manifestName = manifestName.ToLower() ;
			var manifestInfo = m_ManifestInfo.FirstOrDefault( _ => _.ManifestName.ToLower() == manifestName ) ;
			if( manifestInfo == null )
			{
				return null ;
			}

			// アセットバンドルのパス一覧を取得する
			return manifestInfo.GetAllDependentAssetBundlePaths( assetBundleName, updateRequiredOnly ) ;
		}

		//-------------------------------------------------------------------

		/// <summary>
		/// 指定のマニフェストをローカルストレージに明示的に保存する
		/// </summary>
		/// <param name="manifestName"></param>
		/// <returns></returns>
		public static bool SaveManifestInfo( string manifestName )
		{
			// 解りにくくなるので簡略化はしない
			return m_Instance != null && m_Instance.SaveManifestInfo_Private( manifestName ) ;
		}

		// 指定のマニフェストをローカルストレージに明示的に保存する
		private bool SaveManifestInfo_Private( string manifestName )
		{
			if( string.IsNullOrEmpty( manifestName ) == true || m_ManifestInfo == null || m_ManifestInfo.Count == 0 )
			{
				return false ;
			}

			manifestName = manifestName.ToLower() ;
			var manifestInfo = m_ManifestInfo.FirstOrDefault( _ => _.ManifestName.ToLower() == manifestName ) ;
			if( manifestInfo == null )
			{
				return false ;
			}

			return manifestInfo.Save() ;
		}

		/// <summary>
		/// 全てのマニフェストをローカルストレージに明示的に保存する
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool SaveAllManifestInfo()
		{
			// 解りにくくなるので簡略化はしない
			return m_Instance != null && m_Instance.SaveAllManifestInfo_Private() ;
		}

		// 全てのマニフェストをローカルストレージに明示的に保存する
		private bool SaveAllManifestInfo_Private()
		{
			if( m_ManifestInfo == null || m_ManifestInfo.Count == 0 )
			{
				return false ;
			}

			bool result = true ;
			foreach( var manifestInfo in m_ManifestInfo )
			{
				if( manifestInfo.IsStreamingAssetsOnly == false )
				{
					if( manifestInfo.Save() == false )
					{
						result = false ;
					}
				}
			}

			return result ;
		}


		/// <summary>
		/// ＨＴＴＰのバージョンで最も低いものを取得する
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static int GetHttpVersion()
		{
			// 解りにくくなるので簡略化はしない
			if( m_Instance == null )
			{
				return 0 ;
			}

			return m_Instance.GetHttpVersion_Private() ;
		}

		// ＨＴＴＰのバージョンで最も低いものを取得する(マニフェストとＣＲＣをロードした中で)
		private int GetHttpVersion_Private()
		{
			if( m_ManifestInfo == null || m_ManifestInfo.Count == 0 )
			{
				return 0 ;
			}

			int httpVersion = 10 ;
			foreach( var manifestInfo in m_ManifestInfo )
			{
				if( manifestInfo.HttpVersion != 0 && manifestInfo.HttpVersion <  httpVersion )
				{
					httpVersion = manifestInfo.HttpVersion ;
				}
			}

			if( httpVersion ==  0 || httpVersion == 10 )
			{
				httpVersion  = 1 ;
			}

			return httpVersion ;
		}

		/// <summary>
		/// ＨＴＴＰのバージョンでに応じたダウンロードの並列数を取得する
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static int GetMaxParallel()
		{
			// 解りにくくなるので簡略化はしない
			if( m_Instance == null )
			{
				return 0 ;
			}

			return m_Instance.GetMaxParallel_Private() ;
		}

		// ＨＴＴＰのバージョンでに応じたダウンロードの並列数を取得する
		private int GetMaxParallel_Private()
		{
			int httpVersion = GetHttpVersion_Private() ;

			if( httpVersion <= 1 )
			{
				return m_MaxParallelOfHttp1 ;
			}
			else
			{
				return m_MaxParallelOfHttp2 ;
			}
		}

		/// <summary>
		/// ＨＴＴＰのバージョンでに応じたダウンロードの並列数を設定する
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool SetMaxParallel( int http1, int http2 )
		{
			// 解りにくくなるので簡略化はしない
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.SetMaxParallel_Private( http1, http2 ) ;
		}

		// ＨＴＴＰのバージョンでに応じたダウンロードの並列数を取得する
		private bool SetMaxParallel_Private( int http1, int http2 )
		{
			if( http1 <    1 || http1 >    6 )
			{
				http1  = 6 ;
			}

			m_MaxParallelOfHttp1 = http1 ;

			if( http2 <    1 || http2 >  128 )
			{
				http2  = 128 ;
			}

			m_MaxParallelOfHttp2 = http2 ;

			return true ;
		}
	}
}
