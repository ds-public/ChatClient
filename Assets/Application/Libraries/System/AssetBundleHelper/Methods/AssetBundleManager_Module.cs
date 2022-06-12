//#define UseBestHTTP

using System ;
using System.Text ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using UnityEngine ;
using UnityEngine.Networking ;

using StorageHelper ;

#if UseBestHTTP
using BestHTTP ;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Tls ;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.X509 ;
#endif

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
#if !UseBestHTTP
		//-------------------------------------------------------------------------------------------
		// Unity Standard

		//-----------------------------------------------------------

		// マニフェスト単位での処理
		public partial class ManifestInfo
		{
			// ダウンロードを実行する
			private IEnumerator DownloadFromRemote( string path, Action<DownloadStates,byte[],float,long,string,int> onProgress, AssetBundleManager instance )
			{
				DownloadStates	state	= DownloadStates.Processing ;
				byte[]	data			= null ;
				float	progress		= 0 ;
				long	downloadedSize	= 0 ;
				string	error			= null ;
				int		version			= 0 ;

				//---------------------------------
				// ダウンロード実行

				//---------------------------------------------------------
				// HTTP ヘッダーの設定

				Dictionary<string,string> header = new Dictionary<string, string>()
				{
					// バイト配列通信限定
					{  "Content-Type", "application/octet-stream" }
				} ;

				if( instance.m_ConstantHeaders.Count >  0 )
				{
					foreach( var constantHeader in instance.m_ConstantHeaders )
					{
						if( header.ContainsKey( constantHeader.Key ) == false )
						{
							// 追加
							header.Add( constantHeader.Key, constantHeader.Value ) ;
						}
						else
						{
							// 上書
							header[ constantHeader.Key ] = constantHeader.Value ;
						}
					}
				}

				//---------------------------------

				// ヘッダー値のエスケープ(ヘッダも文字列もＵＲＬエンコードする必要がある)
				if( header.Count >  0 )
				{
					int i, l = header.Count ;
					string[] keys = new string[ l ] ;
					header.Keys.CopyTo( keys, 0 ) ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						if( header[ keys[ i ] ] == null )
						{
							// Null はサーバーで問題を引き起こす可能性があるため空文字に変える
							header[ keys[ i ] ] = string.Empty ;
						}

						header[ keys[ i ] ] = instance.EscapeHttpHeaderValue( header[ keys[ i ] ] ) ;

	//					Debug.LogWarning( "HN:" + keys[ i ] + " HV:" + header[ keys[ i ] ] ) ;
	//					header[ keys[ i ] ] = EscapeHttpHeaderValue( header[ keys[ i ] ] ) ;
					}
				}

				//-----------------------------------------------------------------------------------------

				// 通信リクエスト生成
				UnityWebRequest www = UnityWebRequest.Get( path ) ;

				// ヘッダを設定
				if( header.Count >  0 )
				{
					int i, l = header.Count ;
					string[] keys = new string[ l ] ;
					header.Keys.CopyTo( keys, 0 ) ;
					for( i  = 0 ; i <  l ; i ++ )
					{
//						Debug.Log( "<color=#FF3FFF>[AssetBundleManager] HTTP Header : Key = " + keys[ i ] + " Value = " + header[ keys[ i ] ] + "</color>" ) ;
						www.SetRequestHeader( keys[ i ], header[ keys[ i ] ] ) ;
					}
				}

				//---------------------------------------------------------

				// 通信リクエスト実行
				www.SendWebRequest() ;

				while( true )
				{
					if( IsNetworkError( www ) == false )
					{
						// エラーは起きていない
						progress = www.downloadProgress ;
						downloadedSize = ( int )www.downloadedBytes ;

						// 途中のコールバック
						onProgress?.Invoke( state, data, progress, downloadedSize, error, version ) ;

						if( www.isDone == true )
						{
							// 成功
							state	= DownloadStates.Successed ;
							data	= www.downloadHandler.data ;
							version	= 1 ;
							break ;
						}
					}
					else
					{
						// エラー発生
						state	= DownloadStates.Failed ;
						error	= www.error ;
						version	= 1 ;
						break ;
					}

					yield return null ;
				}

				www.Dispose() ;

				// 最後のコールバック
				onProgress?.Invoke( state, data, progress, downloadedSize, error, version ) ;
			}

			// プロトコルの細かい設定を行う
			private void SetHttpSettings()
			{
				// HTTP/1.1

				// 何もしない
			}
		}

		//---------------------------------------------------------------------------
#else
		//-------------------------------------------------------------------------------------------
		// BestHTTP

		/// <summary>
		///  セキュア認証を通すためのクラス
		/// </summary>
		class CustomVerifier : ICertificateVerifyer
		{
			public bool IsValid( Uri serverUri, X509CertificateStructure[] certs )
			{
				return true ;
			}
		}

		// マニフェスト単位での処理
		public partial class ManifestInfo
		{
			// BestHTTP を用いたダウンロード
			private IEnumerator DownloadFromRemote( string path, Action<DownloadStates,byte[],float,long,string,int> onProgress )
			{
				DownloadStates	state	= DownloadStates.Processing ;
				byte[]	data			= null ;
				float	progress		= 0 ;
				long	downloadedSize	= 0 ;
				string	error			= null ;
				int		version			= 0 ;

				//----------------------------------
				// コールバック

				void OnDownloadProgress( HTTPRequest request, long offset, long length )
				{
					progress		= ( offset / ( float )length ) ;
					downloadedSize	= offset ;

					onProgress?.Invoke( state, data, progress, downloadedSize, error, version ) ;
				}

				void OnRequestFinished( HTTPRequest request, HTTPResponse response )
				{
//					Debug.Log( "HTTP/" + response.VersionMajor + "." + response.VersionMinor ) ;
					version = response.VersionMajor ;

					if( response.IsSuccess == true )
					{
						state	= DownloadStates.Successed ;
						data	= response.Data ;
					}
					else
					{
						state	= DownloadStates.Failed ;
						error	= response.Message ;
					}
				}

				//----------------------------------
				// ダウンロード実行

				var request = new HTTPRequest( new Uri( path ), OnRequestFinished ) ;

				// セキュア通信の認証
				request.CustomCertificateVerifyer = new CustomVerifier() ;
				request.UseAlternateSSL = true ;

				request.OnDownloadProgress = OnDownloadProgress ;

				request.Send() ;

				// 終了を待つ
				while( true )
				{
					if( state != DownloadStates.Processing )
					{
						break ;
					}

					yield return null ;
				}

				// 最後のコールバック
				onProgress?.Invoke( state, data, progress, downloadedSize, error, version ) ;
			}

			// プロトコルの細かい設定を行う
			private void SetHttpSettings()
			{
				// HTTP/2.0

				HTTPManager.HTTP2Settings.MaxConcurrentStreams = 64 ;
				HTTPManager.HTTP2Settings.InitialStreamWindowSize = 10 * 1024 * 1024;
				HTTPManager.HTTP2Settings.InitialConnectionWindowSize = HTTPManager.HTTP2Settings.MaxConcurrentStreams * 1024 * 1024 ;
				HTTPManager.HTTP2Settings.MaxFrameSize = 1 * 1024 * 1024 ;
				HTTPManager.HTTP2Settings.MaxIdleTime = TimeSpan.FromSeconds( 120 ) ;

				HTTPManager.HTTP2Settings.WebSocketOverHTTP2Settings.EnableWebSocketOverHTTP2 = true ;
				HTTPManager.HTTP2Settings.WebSocketOverHTTP2Settings.EnableImplementationFallback = true ;
			}
		}
#endif

		//-------------------------------------------------------------------------------------------

#if false
		/// <summary>
		/// タグで指定したアセットバンドルのダウンロードを行う(非同期)
		/// </summary>
		/// <param name="path">アセットバンドルのパス</param>
		/// <param name="keep">キャッシュオーバー時の動作(true=キャッシュオーバー時に保持する・false=キャッシュオーバー時に破棄する)</param>
		/// <returns>アセットバンドルのダウンロードリクエストクラスのインスタンス</returns>
		public static Request DownloadAssetBundlesWithTagAsync( string tag, bool keep = false, Action<int,int> onProgress = null )
		{
			return DownloadAssetBundlesWithTagsAsync( new string[]{ tag }, keep, onProgress ) ;
		}
		public static Request DownloadAssetBundlesWithTagsAsync( string[] tags, bool keep = false, Action<int,int> onProgress = null )
		{
			if( m_Instance == null )
			{
				// インスタンスが生成されていない
				return null ;
			}

			Request request = new Request( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.DownloadAssetBundlesWithTagsAsync_Private( m_Instance.m_DefaultManifestName, tags, keep, onProgress, request ) ) ;
			return request ;
		}

		/// <summary>
		/// タグで指定したアセットバンドルのダウンロードを行う(非同期)
		/// </summary>
		/// <param name="path">アセットバンドルのパス</param>
		/// <param name="keep">キャッシュオーバー時の動作(true=キャッシュオーバー時に保持する・false=キャッシュオーバー時に破棄する)</param>
		/// <returns>アセットバンドルのダウンロードリクエストクラスのインスタンス</returns>
		public static Request DownloadAssetBundlesWithTagsAsync( string manifestName, string tag, bool keep = false, Action<int,int> onProgress = null )
		{
			return DownloadAssetBundlesWithTagsAsync( manifestName, new string[]{ tag }, keep, onProgress ) ;
		}
		public static Request DownloadAssetBundlesWithTagsAsync( string manifestName, string[] tags, bool keep = false, Action<int,int> onProgress = null )
		{
			if( m_Instance == null )
			{
				// インスタンスが生成されていない
				return null ;
			}

			Request request = new Request( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.DownloadAssetBundlesWithTagsAsync_Private( manifestName, tags, keep, onProgress, request ) ) ;
			return request ;
		}

		// タグで指定したアセットバンドルのダウンロードを行う
		private IEnumerator DownloadAssetBundlesWithTagsAsync_Private( string manifestName, string[] tags, bool keep, Action<int,int> onProgress, Request request )
		{
			if( tags == null || tags.Length == 0 )
			{
				if( string.IsNullOrEmpty( request.Error ) == true )
				{
					request.Error = "Invalid tags." ;
				}
				yield break ;
			}

			//--------------------------

			bool isCompleted = false ;
			string error = string.Empty ;

			if( string.IsNullOrEmpty( manifestName ) == false && m_ManifestHash.ContainsKey( manifestName ) == true  )
			{
				yield return StartCoroutine( m_ManifestHash[ manifestName ].DownloadAssetBundlesWithTags_Coroutine
				(
					tags,
					keep,
					onProgress,
					request,
					this
				) ) ;
			}

			if( isCompleted == false )
			{
				if( string.IsNullOrEmpty( error ) == true )
				{
					error = "Could not load." ;
				}
				request.Error = error ;
				yield break ;
			}

			request.IsDone = true ;
		}
#endif

	}
}
