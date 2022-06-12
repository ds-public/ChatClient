using System ;
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

		//-----------------------------------------------------------

		/// <summary>
		/// リソースキャッシュ
		/// </summary>
		[Serializable]
		public class ResourceCacheElement
		{
			public	string				Path ;			// デバッグ表示用のパス

			[NonSerialized]
			public	UnityEngine.Object	Resource ;

			public	bool				Mark ;			// スイープコントロール用のマーク

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="path"></param>
			/// <param name="resource"></param>
			public ResourceCacheElement( string path, UnityEngine.Object resource )
			{
				Path		= path ;
				Resource	= resource ;

				Mark		= true ;
			}

			public UnityEngine.Object Get()
			{
				Mark = true ;

//				Debug.Log( "------------->キャッシュにヒット:" + Path ) ;
				return Resource ;
			}
		}

		/// <summary>
		/// リソースのキャッシュ
		/// </summary>
		private Dictionary<string,ResourceCacheElement> m_ResourceCache ;

		/// <summary>
		/// リソースキャッシュ
		/// </summary>
		internal protected Dictionary<string,ResourceCacheElement> ResourceCache
		{
			get
			{
				return m_ResourceCache ;
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// リソースキャッシュを有効にするかどうか(デフォルトは有効)
		/// </summary>
		public static bool ResourceCacheEnabled
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}

				return m_Instance.ResourceCacheEnabled_Private ;
			}
			set
			{
				if( m_Instance != null )
				{
					m_Instance.ResourceCacheEnabled_Private = value ;
				}
			}
		}
		
		private bool ResourceCacheEnabled_Private
		{
			get
			{
				return !( m_ResourceCache == null ) ;
			}
			set
			{
				if( value == true )
				{
					if( m_ResourceCache == null )
					{
						m_ResourceCache		= new Dictionary<string, ResourceCacheElement>() ;
#if UNITY_EDITOR
						m_ResourceCacheInfo	= new List<ResourceCacheElement>() ;
#endif
					}
				}
				else
				{
					if( m_ResourceCache != null )
					{
						m_ResourceCache.Clear() ;
						m_ResourceCache  = null ;
#if UNITY_EDITOR
						m_ResourceCacheInfo.Clear() ;
						m_ResourceCacheInfo  = null ;
#endif
					}
				}
			}
		}

		//-------------------------------------------------------------------

		/// <summary>
		/// リソースキャッシュをクリアする
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool ClearResourceCache( bool noMarkingOnly, bool unloadUnusedAssets )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.ClearResourceCache_Private( noMarkingOnly, unloadUnusedAssets ) ;
		}

		// リソースキャッシュをクリアする
		private bool ClearResourceCache_Private( bool noMarkingOnly, bool unloadUnusedAssets )
		{
			// アセットバンドルキャッシュをクリア
			if( m_ManifestInfo == null || m_ManifestInfo.Count >  0 )
			{
				foreach( var manifestInfo in m_ManifestInfo )
				{
					// 各マニフェストのアセットバンドルキャッシュもクリアする
					manifestInfo.ClearAssetBundleCache( true, noMarkingOnly ) ;
				}
			}

			//----------------------------------

			// リソースキャッシュをクリア
			if( m_ResourceCache != null && m_ResourceCache.Count >  0 )
			{
#if UNITY_EDITOR
				Debug.Log( "[AssetBundleManager] キャッシュからクリア対象となる展開済みリソース数 = " + m_ResourceCache.Count ) ;
#endif
				if( noMarkingOnly == false )
				{
					// 全て消去する
					m_ResourceCache.Clear() ;
#if UNITY_EDITOR
					m_ResourceCacheInfo.Clear() ;
#endif
				}
				else
				{
					// マークが付いていないもののみ消去する
					List<string> paths = new List<string>() ;
					foreach( var element in m_ResourceCache )
					{
						if( element.Value.Mark == false )
						{
							paths.Add( element.Key ) ;
						}
					}

					if( paths.Count >  0 )
					{
						foreach( var path in paths )
						{
#if UNITY_EDITOR
							m_ResourceCacheInfo.Remove( m_ResourceCache[ path ] ) ;
#endif
							m_ResourceCache.Remove( path ) ;
						}
					}
				}
			}

			//----------------------------------------------------------

			// ランタイムでのリソースキャッシュをクリア
			if( unloadUnusedAssets == true )
			{
				Resources.UnloadUnusedAssets() ;
				System.GC.Collect() ;
			}

			return true ;
		}



		/// <summary>
		/// リソースキャッシュのマークをクリアする
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool ClearResourceCacheMarks()
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.ClearResourceCacheMarks_Private() ;
		}

		// リソースキャッシュのマークをクリアする
		private bool ClearResourceCacheMarks_Private()
		{
			// アセットバンドルキャッシュのマークをクリア
			if( m_ManifestInfo == null || m_ManifestInfo.Count >  0 )
			{
				foreach( var manifestInfo in m_ManifestInfo )
				{
					// 各マニフェストのアセットバンドルキャッシュもクリアする
					manifestInfo.ClearAssetBundleCacheMarks() ;
				}
			}

			// リソースキャッシュのマークをクリアする
			if( m_ResourceCache != null && m_ResourceCache.Count >  0 )
			{
				foreach( var element in m_ResourceCache )
				{
					element.Value.Mark = false ;
				}
			}

			return true ;
		}


		//---------------------------------------------------------------------------------------------------------

		/// <summary>
		/// ローカルストレージ内のアセットバンドルファイルキャッシュをクリアする
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Cleanup( string manifestName = null )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.Cleanup_Private( manifestName ) ;
		}

		// ローカルストレージ内のアセットバンドルキャッシュをクリアする
		private  bool Cleanup_Private( string manifestName )
		{
			bool result = false ;

			if( string.IsNullOrEmpty( manifestName ) == true )
			{
				// 全マニフェストを対象とする
				if( m_ManifestInfo != null && m_ManifestInfo.Count >  0 )
				{
					foreach( var manifestInfo in m_ManifestInfo )
					{
						manifestInfo.SetAllUpdateRequired() ;	// 更新が必要扱いにする
					}
				}
				result = StorageAccessor_Remove( string.Empty, true ) ;
			}
			else
			{
				manifestName = manifestName.ToLower() ;

				// 指定したマニフェストのみを対象とする
				var manifest = m_ManifestInfo.FirstOrDefault( _ => _.ManifestName.ToLower() == manifestName ) ;
				if( manifest != null )
				{
					manifest.RemoveAllFiles() ;
					result = true ;
				}
				else
				{
					result = false ;
				}
			}

			return result ;
		}

		//-------------------------------------------------------------------

	}
}
