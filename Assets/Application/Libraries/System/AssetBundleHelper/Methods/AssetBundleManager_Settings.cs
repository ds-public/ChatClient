using System ;
using System.Collections.Generic ;
using System.IO ;
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
		/// ダウンロードしたアセットバンドルファイルを保存するルートフォルダ名
		/// </summary>
		public static string DataPath
		{
			get
			{
				return m_Instance == null ? string.Empty : m_Instance.m_DataPath ;
			}
			set
			{
				if( m_Instance != null )
				{
					m_Instance.m_DataPath = value ;
				}
			}
		}

		[SerializeField,Header("ストレージ内の保存対象パス")]
		private string m_DataPath = "AssetBundleCache" ;


		/// <summary>
		/// 保存されたアセットバンドルファイルのパスをハッシュ化して隠蔽するかどうか
		/// </summary>
		public static bool SecurityEnabled
		{
			get
			{
				return m_Instance != null && m_Instance.m_SecurityEnabled ;
			}
			set
			{
				if( m_Instance != null )
				{
					m_Instance.m_SecurityEnabled = value ;
				}
			}
		}

		[SerializeField,Header("各種情報を暗号化するかどうか")]
		private bool m_SecurityEnabled = false ;

		/// <summary>
		/// 非同期版のロードを行う際に通信以外処理を全て同期で行うかどうか(展開速度は上がるが別のコルーチンの呼び出し頻度が下がる)
		/// </summary>
		public static bool FastLoadEnabled
		{
			get
			{
				return m_Instance != null && m_Instance.m_FastLoadEnabled ;
			}
			set
			{
				if( m_Instance != null )
				{
					m_Instance.m_FastLoadEnabled = value ;
				}
			}
		}
		
		[SerializeField,Header("非同期ロード実行時に通信以外を全て同期で行うかどうか")]
		private bool m_FastLoadEnabled = true ;

		/// <summary>
		/// ストレージへの書き込みを非同期で行うかどうか
		/// </summary>
		public static bool AsynchronousWritingEnabled
		{
			get
			{
				return m_Instance != null && m_Instance.m_AsynchronousWritingEnabled ;
			}
			set
			{
				if( m_Instance != null )
				{
					m_Instance.m_AsynchronousWritingEnabled = value ;
				}
			}
		}
		
		[SerializeField,Header("ストレージへの書き込みを非同期で行うかどうか")]
		private bool m_AsynchronousWritingEnabled = true ;

		/// <summary>
		/// アセットバンドルマネージャ起動と同時にマニフェストをロードするかどうかを示す
		/// </summary>
		public static bool LoadManifestOnAwake
		{
			get
			{
				return m_Instance != null && m_Instance.m_LoadManifestOnAwake ;
			}
			set
			{
				if( m_Instance != null )
				{
					m_Instance.m_LoadManifestOnAwake = value ;
				}
			}
		}
		
		[SerializeField,Header("AssetBundleManager起動と同時に自動的にマニフェストのダウンロードを行うかどうか")]
		private bool m_LoadManifestOnAwake = false ;


		//-----------------------------------

		/// <summary>
		/// 通信プロトコルが HTTP/1.1 の場合の最大並列ダウンロード数
		/// </summary>
		public static int MaxParallelOfHttp1
		{
			get
			{
				if( m_Instance == null )
				{
					return 0 ;
				}
				return m_Instance.m_MaxParallelOfHttp1 ;
			}
			set
			{
				if( m_Instance != null )
				{
					m_Instance.m_MaxParallelOfHttp1 = value ;
				}
			}
		}
		
		[SerializeField,Header("通信プロトコルが HTTP/1.1 の場合の最大並列ダウンロード数")]
		private int m_MaxParallelOfHttp1 =  6 ;

		/// <summary>
		/// 通信プロトコルが HTTP/2.0 の場合の最大並列ダウンロード数
		/// </summary>
		public static int MaxParallelOfHttp2
		{
			get
			{
				if( m_Instance == null )
				{
					return 0 ;
				}
				return m_Instance.m_MaxParallelOfHttp2 ;
			}
			set
			{
				if( m_Instance != null )
				{
					m_Instance.m_MaxParallelOfHttp2 = value ;
				}
			}
		}
		
		[SerializeField,Header("通信プロトコルが HTTP/2.0 の場合の最大並列ダウンロード数")]
		private int m_MaxParallelOfHttp2 = 24 ;

		//-----------------------------------

		/// <summary>
		/// ローカルアセットを使うかどうか(デバッグ用)
		/// </summary>
		public static bool UseLocalAssets
		{
			get
			{
#if UNITY_EDITOR
				return m_Instance != null && m_Instance.m_UseLocalAssets ;
#else
				return false ;
#endif
			}
			set
			{
				if( m_Instance != null )
				{
					m_Instance.m_UseLocalAssets = value ;
				}
			}
		}
		
		[SerializeField,Header("ローカルアセットを使用するかどうか(Eitor専用)")]
		private bool m_UseLocalAssets = true ;

		//-----------------------------------

		/// <summary>
		/// マニフェストとパスの区切り記号
		/// </summary>
		public static string ManifestSeparator
		{
			get
			{
				return m_Instance != null ? m_Instance.m_ManifestSeparator : null ;
			}
			set
			{
				if( m_Instance != null )
				{
					m_Instance.m_ManifestSeparator = value ;
				}
			}
		}
		
		[SerializeField,Header("マニフェストとパスの区切り記号")]
		private string m_ManifestSeparator = "|" ;

		//-----------------------------------

		/// <summary>
		/// デフォルトのマニフェスト(無指定の場合はパスの最初のフォルダ名がマニフェスト名とみなされる)
		/// </summary>
		public static string DefaultManifestName
		{
			get
			{
				return m_Instance == null ? string.Empty : m_Instance.m_DefaultManifestName ;
			}
			set
			{
				if( m_Instance != null )
				{
					m_Instance.m_DefaultManifestName = value ;
				}
			}
		}
		
		[SerializeField,Header("デフォルトのマニフェスト名(マニフェスト名省略時の対象)")]
		private string m_DefaultManifestName = "" ;

		//-----------------------------------------------------------------
		
		/// <summary>
		/// 全マニフェスト情報
		/// </summary>
		[SerializeField,Header("【マニフェスト情報】")]
		private List<ManifestInfo> m_ManifestInfo = null ;

		//-------------------------------------------------------------------------------------------
		// ダウンロード時に追加したい HTTP ヘッダ情報

		private readonly Dictionary<string,string>	m_ConstantHeaders = new Dictionary<string, string>() ;

#if UNITY_EDITOR
		/// <summary>
		/// 任意追加のヘッダー
		/// </summary>
		[Serializable]
		public class ConstantHeader
		{
			public string Key ;
			public string Value ;
		}

		private readonly List<ConstantHeader>	m_ConstantHeaers_Monitor = new List<ConstantHeader>() ;
#endif

		/// <summary>
		/// HTTP ヘッダに設定する情報を追加する
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool AddHeader( string key, string value )
		{
			if( m_Instance == null )
			{
				return false ;
			}
			return m_Instance.AddHeader_Private( key, value ) ;
		}

		// HTTP ヘッダに設定する情報を追加する
		private bool AddHeader_Private( string key, string value )
		{
			if( string.IsNullOrEmpty( key ) == true )
			{
				return false ;
			}

			if( m_ConstantHeaders.ContainsKey( key ) == false )
			{
				// 新規
				if( string.IsNullOrEmpty( value ) == false )
				{
					// 追加
					m_ConstantHeaders.Add( key, value ) ;
#if UNITY_EDITOR

					m_ConstantHeaers_Monitor.Add( new ConstantHeader(){ Key = key, Value = value } ) ;
#endif
				}
			}
			else
			{
				// 既存
				if( string.IsNullOrEmpty( value ) == false )
				{
					// 上書
					m_ConstantHeaders[ key ] = value ;
#if UNITY_EDITOR
					var record = m_ConstantHeaers_Monitor.FirstOrDefault( _ => _.Key == key ) ;
					if( record != null )
					{
						record.Value = value ;
					}
#endif

				}
				else
				{
					// 削除
					m_ConstantHeaders.Remove( key ) ;

#if UNITY_EDITOR
					var record = m_ConstantHeaers_Monitor.FirstOrDefault( _ => _.Key == key ) ;
					if( record != null )
					{
						m_ConstantHeaers_Monitor.Remove( record ) ;
					}
#endif
				}
			}

			return true ;
		}

		/// <summary>
		/// HTTP ヘッダに設定する情報を削除する
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public static bool RemoveHeader( string key )
		{
			if( m_Instance == null )
			{
				return false ;
			}
			return m_Instance.RemoveHeader_Private( key ) ;
		}

		// HTTP ヘッダに設定する情報を削除する
		private bool RemoveHeader_Private( string key )
		{
			if( string.IsNullOrEmpty( key ) == true )
			{
				return false ;
			}

			if( m_ConstantHeaders.ContainsKey( key ) == true )
			{
				// 既存

				// 削除
				m_ConstantHeaders.Remove( key ) ;

#if UNITY_EDITOR
				var record = m_ConstantHeaers_Monitor.FirstOrDefault( _ => _.Key == key ) ;
				if( record != null )
				{
					m_ConstantHeaers_Monitor.Remove( record ) ;
				}
#endif
			}

			return true ;
		}

		/// <summary>
		/// HTTP ヘッダに設定する情報を削除する
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public static bool RemoveAllHeaders()
		{
			if( m_Instance == null )
			{
				return false ;
			}
			return m_Instance.RemoveAllHeaders_Private() ;
		}

		// HTTP ヘッダに設定する情報を削除する
		private bool RemoveAllHeaders_Private()
		{
			m_ConstantHeaders.Clear() ;
#if UNITY_EDITOR
			m_ConstantHeaers_Monitor.Clear() ;
#endif
			return true ;
		}

		//-----------------------------------------------------------------

		/// <summary>
		/// ローカルアセットを使うかどうか(デバッグ用)
		/// </summary>
		public static bool LogEnabled
		{
			get
			{
				return m_Instance != null ? m_Instance.m_LogEnabled : false ;
			}
			set
			{
				if( m_Instance != null )
				{
					m_Instance.m_LogEnabled = value ;
				}
			}
		}
		
		[SerializeField,Header("ログを出力するかどうか")]
		private bool m_LogEnabled = true ;

		//-----------------------------------------------------------

		/// <summary>
		/// 処理を実行中かどうか
		/// </summary>
		[SerializeField][NonSerialized]
		private int m_AsyncProcessingCount ;

		//-------------------------------------------------------------------------------------------------------------------

#if UNITY_EDITOR
			/// <summary>
			/// デバッグ用のキャッシュ中のリソースの表示リスト
			/// </summary>
			[SerializeField,Header("【リソースキャッシュ情報】")]
			private List<ResourceCacheElement>	m_ResourceCacheInfo = null ;
#endif

		//-------------------------------------------------------------------------------------------------------------------

#if UNITY_EDITOR

		// 使用するアセットバンドルを記録する
		private bool	m_IsRecording ;

		/// <summary>
		/// 使用するアセットバンドルの情報
		/// </summary>
		[Serializable]
		public class UsingAssetBundleInfo
		{
			/// <summary>
			/// マニフェスト名
			/// </summary>
			public string	ManifestName ;

			/// <summary>
			/// アセットバンドルのパス
			/// </summary>
			public string	AssetBundlePath ;

			/// <summary>
			/// ファイルサイズ
			/// </summary>
			public int		AssetBundleSize ;

			/// <summary>
			/// 参照された回数
			/// </summary>
			public int		ReferencedCount ;
		}

		[SerializeField][Header( "使用対象のアセットバンドル記録" )]
		private List<UsingAssetBundleInfo>								m_UsingAssetBundles		= new List<UsingAssetBundleInfo>() ;
		private Dictionary<( string, string ), UsingAssetBundleInfo>	m_UsingAssetBundles_Hash = new Dictionary<(string, string), UsingAssetBundleInfo>() ;

		/// <summary>
		/// 現在使用するアセットバンドル情報を記録中かどうか
		/// </summary>
		public static bool IsRecording
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}

				return m_Instance.m_IsRecording ;
			}
		}

		/// <summary>
		/// 使用するアセットバンドルの記録を開始する
		/// </summary>
		/// <returns></returns>
		public static bool StartRecording()
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.StartRecording_Private() ;
		}

		// 使用するアセットバンドルの記録を開始する
		private bool StartRecording_Private()
		{
			if( m_IsRecording == true )
			{
				Debug.Log( "<color=#00FF7F>[AssetBundleManager] Already started recording</color>" ) ;
			}

			//----------------------------------------------------------

			m_IsRecording = true ;

			m_UsingAssetBundles.Clear() ;
			m_UsingAssetBundles_Hash.Clear() ;

			Debug.Log( "<color=#00FF7F>[AssetBundleManager] StartRecording()</color>" ) ;

			return true ;
		}

		/// <summary>
		/// 使用するアセットバンドルの記録を終了する
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string[] StopRecording( string path, string platformName, string separator = "," )
		{
			if( m_Instance == null )
			{
				return null ;
			}

			return m_Instance.StopRecording_Private( path, platformName, separator ) ;
		}

		// 使用するアセットバンドルの記録を終了する
		private string[] StopRecording_Private( string path, string platformName, string separator )
		{
			if( m_IsRecording == false )
			{
				Debug.LogWarning( "<color=#00FF7F>[AssetBundleManager] Not start recording</color>" ) ;
				return null ;
			}

			//----------------------------------------------------------

			m_IsRecording = false ;

			//----------------------------------------------------------

			if( m_UsingAssetBundles.Count >  0 )
			{
				// 記録された情報がある

				// 情報をソートする
				m_UsingAssetBundles = m_UsingAssetBundles.OrderBy( _ => _.ManifestName ).ThenBy( _ => _.AssetBundlePath ).ToList() ;

				//----------------------------------------------------------
				// ファイルを書き出す

				long totalSize = 0 ;

				foreach( var record in m_UsingAssetBundles )
				{
					totalSize += record.AssetBundleSize ;
				}

				if( string.IsNullOrEmpty( separator ) == true )
				{
					separator = "," ;
				}

				//--------------

				string text = string.Empty ;

				// Platform
				text += "# Platform = " + platformName + "\n" ;

				// TotalFile
				text += "# TotalFile = " + m_UsingAssetBundles.Count + "\n" ;

				// TotalSize
				text += "# TotalSize = " + totalSize.ToString() + " byte" ;
				if( totalSize >= 1024 )
				{
					text += " ( " + GetSizeName( totalSize ) + " )" ;
				}
				text += "\n" ;

				foreach( var record in m_UsingAssetBundles )
				{
					text += record.ManifestName + separator + record.AssetBundlePath + separator + record.AssetBundleSize.ToString() + separator + record.ReferencedCount.ToString() + "\n" ;
				}

				// 保存
				File.WriteAllText( path, text ) ;

				//--------------------------------------------------------------------------
				// プラットフォーム関係無しのパス情報のみを返す

				List<string> assetBundlePaths = new List<string>() ;

				foreach( var record in m_UsingAssetBundles )
				{
					assetBundlePaths.Add( record.ManifestName + "/" + record.AssetBundlePath ) ;
				}

				//----------------------------------

				Debug.Log( "<color=#00FF7F>[AssetBundleManager] StopRecording() : TotalFile = " + m_UsingAssetBundles.Count + " TotalSize = " + GetSizeName( totalSize ) + " > " + path + "</color>" ) ;

				m_UsingAssetBundles.Clear() ;
				m_UsingAssetBundles_Hash.Clear() ;

				//----------------------------------------------------------

				return assetBundlePaths.ToArray() ;
			}
			else
			{
				// 記録された情報がない

				Debug.Log( "<color=#00FF7F>[AssetBundleManager] StopRecording() : Record is empty.</color>" ) ;

				return null ;
			}
		}

		/// <summary>
		/// 使用するアセットバンドルの記録を中断する
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static bool CancelRecording()
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_Instance.CancelRecording_Private() ;
			return true ;
		}

		// 使用するアセットバンドルの記録を中断する
		private bool CancelRecording_Private()
		{
			if( m_IsRecording == false )
			{
				return true ;
			}

			//----------------------------------

			m_IsRecording  = false ;

			m_UsingAssetBundles.Clear() ;
			m_UsingAssetBundles_Hash.Clear() ;

			Debug.Log( "<color=#00FF7F>[AssetBundleManager] Recording canceled</color>" ) ;

			return true ;
		}

		//-----------------------------------------------------------
#if false
		/// <summary>
		/// 使用するアセットバンドル情報を記録する
		/// </summary>
		/// <param name="manifestName"></param>
		/// <param name="assetBundlePath"></param>
		/// <param name="size"></param>
		/// <returns></returns>
		public static bool RecordUsingAssetBundle( string manifestName, string assetBundlePath, int assetBundleSize )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_Instance.RecordUsingAssetBundle_Private( manifestName, assetBundlePath, assetBundleSize ) ;

			return true ;
		}
#endif
		// 使用するアセットバンドル情報を記録する
		private void RecordUsingAssetBundle_Private( string manifestName, string assetBundlePath, int assetBundleSize )
		{
			var key = ( manifestName, assetBundlePath ) ;

			// 既に記録済みか判定する
			if( m_UsingAssetBundles_Hash.ContainsKey( key ) == false )
			{
				// 記録はされていない
				var record = new UsingAssetBundleInfo()
				{
					ManifestName	= manifestName,
					AssetBundlePath	= assetBundlePath,
					AssetBundleSize	= assetBundleSize,
					ReferencedCount	= 1
				} ;

				m_UsingAssetBundles.Add( record ) ;
				m_UsingAssetBundles_Hash.Add( key, record ) ;
			}
			else
			{
				// 記録はされている
				m_UsingAssetBundles_Hash[ key ].ReferencedCount ++ ;	// 参照数のみ増加させる
			}
		}
#endif

	}
}
