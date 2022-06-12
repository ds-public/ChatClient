using System ;
using System.Text ;
using System.Collections ;
using System.Collections.Generic ;

using Cysharp.Threading.Tasks ;

using UnityEngine ;
using UnityEngine.Networking ;

using uGUIHelper ;

namespace Template
{
	/// <summary>
	/// ネットワーク全般を制御するクラス Version 2022/05/04 0
	/// </summary>
	public class WebAPIManager : SingletonManagerBase<WebAPIManager>
	{
		// サーバー名
		[SerializeField]
		private string m_EndPoint = null ;

		/// <summary>
		/// サーバー名
		/// </summary>
		public static string EndPoint
		{
			get
			{
				if( m_Instance == null )
				{
					Debug.LogError( "WebAPIManager is not create !" ) ;
					return "" ;
				}
				return m_Instance.m_EndPoint ;
			}
			set
			{
				if( m_Instance == null )
				{
					Debug.LogError( "WebAPIManager is not create !" ) ;
					return ;
				}
				if( m_Instance.m_EndPoint != value )
				{
#if UNITY_EDITOR
					if( string.IsNullOrEmpty( m_Instance.m_EndPoint ) == true )
					{
						Debug.Log( "エンドポイントが設定された:" + value ) ;
					}
					else
					{
						Debug.Log( "エンドポイントが変更された:" + value + " ← " + m_Instance.m_EndPoint ) ;
					}
#endif
					m_Instance.m_EndPoint = value ;
				}
			}
		}

		// デフォルトの接続タイムアウト時間
		[SerializeField]
		private float m_ConnectionTimeout = 0.0f ;

		/// <summary>
		/// デフォルトの接続タイムアウト時間
		/// </summary>
		public static float ConnectionTimeout
		{
			get
			{
				if( m_Instance == null )
				{
					Debug.LogError( "WebAPIManager is not create !" ) ;
					return 0 ;
				}
				return m_Instance.m_ConnectionTimeout ;
			}
			set
			{
				if( m_Instance == null )
				{
					Debug.LogError( "WebAPIManager is not create !" ) ;
					return ;
				}
				m_Instance.m_ConnectionTimeout = value ;
			}
		}

		// デフォルトのタイムアウト時間
		[SerializeField]
		private float m_Timeout = 3.0f ;

		/// <summary>
		/// デフォルトのタイムアウト時間(トータル)
		/// </summary>
		public static float Timeout
		{
			get
			{
				if( m_Instance == null )
				{
					Debug.LogError( "WebAPIManager is not create !" ) ;
					return 0 ;
				}
				return m_Instance.m_Timeout ;
			}
			set
			{
				if( m_Instance == null )
				{
					Debug.LogError( "WebAPIManager is not create !" ) ;
					return ;
				}
				m_Instance.m_Timeout = value ;
			}
		}

		// 最大リトライ回数
		[SerializeField]
		private int m_MaxRetryCount = 3 ;

		/// <summary>
		/// 最大リトライ回数
		/// </summary>
		public static int MaxRetryCount
		{
			get
			{
				if( m_Instance == null )
				{
					Debug.LogError( "WebAPIManager is not create !" ) ;
					return 0 ;
				}
				return m_Instance.m_MaxRetryCount ;
			}
			set
			{
				if( m_Instance == null )
				{
					Debug.LogError( "WebAPIManager is not create !" ) ;
					return ;
				}
				m_Instance.m_MaxRetryCount = value ;
			}
		}


		// Xor暗号化の有無
		[SerializeField]
		private byte m_Xor = 0 ;

		/// <summary>
		/// Xor暗号化の有無
		/// </summary>
		public static byte Xor
		{
			get
			{
				if( m_Instance == null )
				{
					Debug.LogError( "WebAPIManager is not create !" ) ;
					return 0 ;
				}
				return m_Instance.m_Xor ;
			}
			set
			{
				if( m_Instance == null )
				{
					Debug.LogError( "WebAPIManager is not create !" ) ;
					return ;
				}
				m_Instance.m_Xor = value ;
			}
		}

		// 暗号化の有無
		[SerializeField]
		private bool m_UseEncrypt = false ;

		/// <summary>
		/// 暗号化の有無
		/// </summary>
		public static bool UseEncrypt
		{
			get
			{
				if( m_Instance == null )
				{
					Debug.LogError( "WebAPIManager is not create !" ) ;
					return false ;
				}
				return m_Instance.m_UseEncrypt ;
			}
			set
			{
				if( m_Instance == null )
				{
					Debug.LogError( "WebAPIManager is not create !" ) ;
					return ;
				}
				m_Instance.m_UseEncrypt = value ;
			}
		}

		// 暗号化のキー配列
		[SerializeField]
		private byte[] m_EncryptKey ;

		/// <summary>
		/// 暗号化のキー配列
		/// </summary>
		public static byte[] EncryptKey
		{
			get
			{
				if( m_Instance == null )
				{
					Debug.LogError( "WebAPIManager is not create !" ) ;
					return null ;
				}
				return m_Instance.m_EncryptKey ;
			}
			set
			{
				if( m_Instance == null )
				{
					Debug.LogError( "WebAPIManager is not create !" ) ;
					return ;
				}
				m_Instance.m_EncryptKey = value ;
			}
		}

		// 暗号化のベクター配列
		[SerializeField]
		private byte[] m_EncryptVector ;

		/// <summary>
		/// 暗号化のベクター配列
		/// </summary>
		public static byte[] EncryptVector
		{
			get
			{
				if( m_Instance == null )
				{
					Debug.LogError( "WebAPIManager is not create !" ) ;
					return null ;
				}
				return m_Instance.m_EncryptVector ;
			}
			set
			{
				if( m_Instance == null )
				{
					Debug.LogError( "WebAPIManager is not create !" ) ;
					return ;
				}
				m_Instance.m_EncryptVector = value ;
			}
		}

		// 圧縮の有無
		[SerializeField]
		private bool m_IsCompress = false ;

		/// <summary>
		/// 圧縮の有無
		/// </summary>
		public static bool IsCompress
		{
			get
			{
				if( m_Instance == null )
				{
					Debug.LogError( "WebAPIManager is not create !" ) ;
					return false ;
				}
				return m_Instance.m_IsCompress ;
			}
			set
			{
				if( m_Instance == null )
				{
					Debug.LogError( "WebAPIManager is not create !" ) ;
					return ;
				}
				m_Instance.m_IsCompress = value ;
			}
		}

		// クッキー
		[SerializeField]
		private string m_Cookie = "" ;

		/// <summary>
		/// クッキー
		/// </summary>
		public static string Cookie
		{
			get
			{
				if( m_Instance == null )
				{
					Debug.LogError( "WebAPIManager is not create !" ) ;
					return "" ;
				}
				return m_Instance.m_Cookie ;
			}
			set
			{
				if( m_Instance == null )
				{
					Debug.LogError( "WebAPIManager is not create !" ) ;
					return ;
				}
				m_Instance.m_Cookie = value ;
			}
		}

		// アクセストークン
		[SerializeField]
		private string m_AccessToken = "" ;

		/// <summary>
		/// アクセストークン
		/// </summary>
		public static string AccessToken
		{
			get
			{
				if( m_Instance == null )
				{
					Debug.LogError( "WebAPIManager is not create !" ) ;
					return null ;
				}
				return m_Instance.m_AccessToken ;
			}
			set
			{
				if( m_Instance == null )
				{
					Debug.LogError( "WebAPIManager is not create !" ) ;
					return ;
				}

				// ここで一度設定してしまえば以後ゲーム中に変更される事はない
				m_Instance.m_AccessToken = value ;
			}
		}

		//-------------------------------------------------------------------------------------------

		new protected void Awake()
		{
			base.Awake() ;

			// デフォルトのタイムアウト時間
			m_Timeout = 10.0f ;

			// 最大リトライ回数
			m_MaxRetryCount = 3 ;

			// UnityWebRequest で SSL 証明書の確認は不要
		}

		//-------------------------------------------------------------------------------------------

		// ブロッカー用
		private UICanvas m_Blocker ;


		public static void Prepare()
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.Prepare_Private() ;
		}

		/// <summary>
		/// 通信用の無色透明ブロッカーを用意する
		/// </summary>
		public void Prepare_Private()
		{
			m_Blocker = UICanvas.Create( transform ) ;
			m_Blocker.SortingOrder = 990 ;

			// キャンバスの解像度を設定する
			float width  = 1080 ;
			float height = 1920 ;

			Settings settings =	ApplicationManager.LoadSettings() ;
			if( settings != null )
			{
				width  = settings.BasicWidth ;
				height = settings.BasicHeight ;
			}
			m_Blocker.SetResolution( width, height, true ) ;

			//----------------------------------

			UIImage screen = m_Blocker.AddView<UIImage>( "Screen" ) ;

			screen.SetAnchorToStretch() ;
			screen.RaycastTarget = true ;
			screen.Color = ExColor.Transparency ;

			//----------------------------------

			m_Blocker.SetActive( false ) ;
		}

		/// <summary>
		/// リクエストキューをクリアする
		/// </summary>
		public static void Clear()
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.Clear_Private() ;
		}

		private void Clear_Private()
		{
			m_Blocker.SetActive( false ) ;
			m_Queue.Clear() ;
		}

		//---------------------------------------------------------------------------
		
		/// <summary>
		/// リクエストの詳細情報クラス
		/// </summary>
		/// <typeparam name="MapperType"></typeparam>
		public class Request
		{
			public string								Path ;
			public byte[]								RequestData ;
			public Action<int,int>						OnProgress ;
			public bool									UseProgress ;
			public bool									UseDialog ;

			public Request( string path, byte[] requestData, Action<int,int> onProgress, bool useProgress, bool useDialog )
			{
				Path		= path ;
				RequestData	= requestData ;
				OnProgress	= onProgress ;
				UseProgress	= useProgress ;
				UseDialog	= useDialog ;

				RetryCount	= 0 ;
				Completed	= false ;
			}

			//----------------------------------

			// ワーク変数
			public int		RetryCount ;

			public bool		Completed ;

			public int		HttpStatus ;
			public string	ErrorMessage ;
			public byte[]	ResponseData ;
		}

		//-------------------------------------------------------------------------------------------

		// 暗号化を行う
		private byte[] Encrypt( byte[] data )
		{
			if( m_Xor == 0 && m_UseEncrypt == false && m_IsCompress == false )
			{
				// 何もしない
				return data ;
			}

			data = data.Clone() as byte[] ;

			if( m_Xor != 0 )
			{
				// ＸＯＲする
				int i, l = data.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					data[ i ] ^= m_Xor ;
				}
			}

			if( m_UseEncrypt == true && m_EncryptKey != null && m_EncryptVector != null )
			{
				// 暗号化する
				data = Security.Encrypt( data, m_EncryptKey, m_EncryptVector ) ;
			}

			if( m_IsCompress == true )
			{
				// 圧縮する
				data = GZip.Compress( data ) ;
			}

			return data ;
		}

		// 復号化を行う
		private byte[] Decrypt( byte[] data )
		{
			if( m_Xor == 0 && m_UseEncrypt == false && m_IsCompress == true )
			{
				// 何もしない
				return data ;
			}

			if( m_IsCompress == true )
			{
				// 伸長する
				data = GZip.Decompress( data ) ;
			}

			if( m_UseEncrypt == true && m_EncryptKey != null && m_EncryptVector != null )
			{
				// 復号化する
				data = Security.Decrypt( data, m_EncryptKey, m_EncryptVector ) ;
			}

			if( m_Xor != 0 )
			{
				// ＸＯＲする
				int i, l = data.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					data[ i ] ^= m_Xor ;
				}
			}

			return data ;
		}

		//-------------------------------------------------------------------------------------------
		// リクエストを生成・送信する

		/// <summary>
		/// リクエストを生成する
		/// </summary>
		/// <typeparam name="MapperType"></typeparam>
		/// <param name="path"></param>
		/// <param name="parameters"></param>
		/// <param name="onComplete"></param>
		/// <param name="option"></param>
		public static async UniTask<( int, string, byte[] )> SendRequest( string path, byte[] requestData, Action<int, int> onProgress = null, bool useProgress = false, bool useDialog = true )
		{
			if( m_Instance == null )
			{
				Debug.LogError( "WebAPIManager is not create !" ) ;
				return ( 0, string.Empty, null ) ;
			}

			return await m_Instance.MakeRequest( path, requestData, onProgress, useProgress, useDialog ) ;
		}

		//---------------------------
		
		// 通信ＡＰＩのリクエストを生成する
		private async UniTask<( int, string, byte[] )> MakeRequest( string path, byte[] requestData, Action<int, int> onProgress = null, bool useProgress = false, bool useDialog = true )
		{
			//----------------------------------------------------------

			if( useProgress == true || useDialog == true )
			{
				// ブロッカー有効
				m_Blocker.SetActive( true ) ;
			}

			//----------------------------------

			// 必要に応じて暗号化を行う
			requestData = Encrypt( requestData ) ;

			//----------------------------------------------------------

			// リクエストの詳細情報をワンオブジェクトにまとめる
			Request request = new Request( path, requestData, onProgress, useProgress, useDialog ) ;

			// リクエストを送信する
			if( PushRequest( request ) == false )
			{
				return ( 0, string.Empty, null ) ;	// 何らかのエラーが発生した
			}

			await WaitUntil( () => request.Completed ) ;

			//----------------------------------

			if( useProgress == true || useDialog == true )
			{
				// ブロッカー無効
				m_Blocker.SetActive( false ) ;
			}

			return ( request.HttpStatus, request.ErrorMessage, request.ResponseData ) ;
		}

		//-------------------------------------------------------------------

		private readonly Queue<Request> m_Queue = new Queue<Request>() ;

		private bool m_ProcessRequest = false ;
		private bool m_ExecuteRequest = false ;

		/// <summary>
		/// リクエストを追加する
		/// </summary>
		/// <param name="requestData"></param>
		/// <returns></returns>
		private bool PushRequest( Request request )
		{
			if( string.IsNullOrEmpty( m_EndPoint ) == true )
			{
				Debug.LogError( "EndPoint is not set" ) ;
				return false ;
			}

			//----------------------------------------------------------

			// リクエストをキューに加える
			m_Queue.Enqueue( request ) ;

			if( m_ProcessRequest == false )
			{
				// リクエストの処理が実行中でなければ実行する
				m_ProcessRequest = true ;
				_ = When( ProcessRequest() ) ;
			}

			return true ;
		}

		/// <summary>
		/// リクエストを処理する
		/// </summary>
		/// <returns></returns>
		private async UniTask ProcessRequest()
		{
			while( true )
			{
				if( m_Queue.Count == 0 )
				{
					// リクエストが存在しない
					m_ProcessRequest = false ;	// リクエストの処理の実行終了
					return ;
				}

				//----------------------------------

				if( m_ExecuteRequest == false )
				{
					// リクエストが処理可能な状態
					m_ExecuteRequest = true ;
					
					Request request = m_Queue.Dequeue() ;

					_ = ExecuteRequest( request ) ;
				}

				await Yield() ;
			}
		}

		/// <summary>
		/// 通信エラーチェック
		/// </summary>
		/// <param name="unityWebRequest"></param>
		/// <returns></returns>
		private bool IsNetworkError( UnityWebRequest unityWebRequest )
		{
#if UNITY_2020_2_OR_NEWER
			var result = unityWebRequest.result ;
			return
				( result == UnityWebRequest.Result.ConnectionError			) ||
				( result == UnityWebRequest.Result.DataProcessingError		) ||
				( result == UnityWebRequest.Result.ProtocolError			) ||
				( string.IsNullOrEmpty( unityWebRequest.error ) == false	) ;
#else
			return
				( unityWebRequest.isHttpError		== true					) ||
				( unityWebRequest.isNetworkError	== true					) ||
				( string.IsNullOrEmpty( unityWebRequest.error ) == false	) ;
#endif
		}

		// ＵＲＬを整形する
		private static string FormatUrl( string serverName, string path )
		{
			string url = serverName ;
			if( !url.EndsWith( "/" ) )
			{
				url += "/" ;
			}
			url += path ;

			return url ;
		}

		/// <summary>
		/// リクエストを実行する
		/// </summary>
		/// <param name="requestData"></param>
		/// <returns></returns>
		private async UniTask ExecuteRequest( Request request )
		{
			string				path			= request.Path ;
			byte[]				requestData		= request.RequestData ;
			Action<int,int>		onProgress		= request.OnProgress ;
			bool				useProgress		= request.UseProgress ;

			// 基本ＵＲＬ
			string url ;
			if( path.IndexOf( "http://" ) == 0 || path.IndexOf( "https://" ) == 0 )
			{
				// path をそのまま url として使用する
				url = path ;
			}
			else
			{
				// ベース部分を先頭に追加する
				url = FormatUrl( m_EndPoint, path ) ;
			}
			
			//--------------------------------------------------------------------------

			// 通信が機内モードなどになっている場合は最初からエラーを出す
			if( Application.internetReachability == NetworkReachability.NotReachable )
			{
				Debug.LogError( "Network connection is not reachable: " + path ) ;
				await ProcessErrorAsync( true, -9, "Airplane mode", request ) ;
				return ;
			}

			//--------------------------------------------------------------------------

			UnityWebRequest www = null ;

			// 仮
			string playerId		= PlayerData.PlayerId.ToString() ;
			string accessToken	= PlayerData.AccessToken ;

			string uid			= string.Empty ;	// PlayerData.UID ;

			// ＨＴＴＰヘッダ
			Dictionary<string,string> header = new Dictionary<string, string>() ;
//			{
//				// バイト配列通信限定
//				{ "Content-Type",			"application/octet-stream"		},
//				{ "Content-Type",			"application/x-msgpack"			},
//				{ "X-Linegame-UserKey"	,	userKey							},
//				{ "X-Linegame-UserToken",	userToken						},
//			} ;

			//----------------------------------
			// Content-Type

			header.Add( "Content-Type", "application/json" ) ;

			//----------------------------------

			// クッキーがあるならクッキーもヘッダに加える
			if( string.IsNullOrEmpty( m_Cookie ) == false )
			{
				header.Add( "Cookie", m_Cookie ) ;
			}

			// 接続待ちのタイムアウトを設定
			if( m_ConnectionTimeout >  0 )
			{
				header.Add( "client-connect-timeout", m_ConnectionTimeout.ToString() ) ;
			}

			// ダウンロードされる現在量と最大量を逐次記録するようにする

			// 共通の情報をヘッダに加える
//			header.Add( "Accept-Encoding", "gzip, deflate" ) ;

			//----------------------------------

			header.Add( "device", SystemInfo.deviceModel ) ;
			header.Add( "system", SystemInfo.operatingSystem ) ;

			//--------------------------------------------------------------------------

			// キャッシュされないようにする
			if( url.Contains( "?" ) == false )
			{
				url += ( "?ncts=" + ClientTime.GetCurrentUnixTime() ) ;
			}
			else
			{
				url += ( "&ncts=" + ClientTime.GetCurrentUnixTime() ) ;
			}

#if UNITY_EDITOR
			Debug.Log( "<color=#00BFFF><====== 通信API リクエスト送信 : " + url + "</color>" ) ;
			float sessionTimeBase = Time.realtimeSinceStartup ;
#endif
			//--------------------------------------------------------------------------

			if( useProgress == true )
			{
#if UNITY_EDITOR || ( !UNITY_EDITOR && DEVELOPMENT_BUILD )
				// プログレスを表示する
				Progress.On( Progress.Styles.WebAPI, false, "通信中" ) ;
#endif
			}

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

					header[ keys[ i ] ] = EscapeHttpHeaderValue( header[ keys[ i ] ] ) ;

//					Debug.LogWarning( "HN:" + keys[ i ] + " HV:" + header[ keys[ i ] ] ) ;
//					header[ keys[ i ] ] = EscapeHttpHeaderValue( header[ keys[ i ] ] ) ;
				}
			}

			// 通信開始

			// バイト配列を送信する場合は UploadHandlerRaw を使う必要があるためスタティックメソッド(.Post)は使用できない(=文字列限定)
			www = new UnityWebRequest( url, "POST" )
			{
				uploadHandler	= new UploadHandlerRaw( requestData ),
				downloadHandler	= new DownloadHandlerBuffer()	// new コンストラクタの場合は DownloadHandler が生成・設定されないので自前で生成・設定してやる必要がある
			} ;
			
			// ヘッダを設定
			if( header.Count >  0 )
			{
				int i, l = header.Count ;
				string[] keys = new string[ l ] ;
				header.Keys.CopyTo( keys, 0 ) ;
				for( i  = 0 ; i <  l ; i ++ )
				{
//					Debug.Log( "[Http Header] " + keys[ i ] + " = " + header[ keys[ i ] ] ) ;
					www.SetRequestHeader( keys[ i ], header[ keys[ i ] ] ) ;
				}
			}

			// リクエスト実行
			_ = www.SendWebRequest() ;

			//--------------------------------------------------------------------------

			// 以下レスポンスの処理

			// トータルのタイムアウトを設定
//			Debug.Log( "タイムアウト時間:" + m_Timeout ) ;

			int httpStatus = 0 ;
			string errorMessage = string.Empty ;

			byte[] responseData = null ;
			int offset ;
			int length = 0 ;

			float baseTime = Time.realtimeSinceStartup ;

			while( true )
			{
				offset = ( int )www.downloadedBytes ;

				if( www.GetResponseHeaders() != null && www.GetResponseHeaders().ContainsKey( "Content-Length" ) == true )
				{
					int.TryParse( www.GetResponseHeaders()[ "Content-Length" ], out length ) ;
				}

				// 現在のダウンロード状況
				onProgress?.Invoke( offset, length ) ;

				//-------------

				if( IsNetworkError( www ) == true )
				{
					// 接続できない場合は statusCode は 0 になる
					httpStatus = ( int )www.responseCode ;
					errorMessage = www.error ;
					break ;
				}

				if( www.isDone == true )
				{
					// 成功
					httpStatus = ( int )www.responseCode ;
					break ;
				}

				if( m_Timeout >  0 && ( Time.realtimeSinceStartup - baseTime ) >  m_Timeout )
				{
					Debug.LogWarning( "<color=#FFFF00>タイムアウトしました:" + ( Time.realtimeSinceStartup - baseTime ) + "</color>" ) ;
					httpStatus = -1 ;
					errorMessage = "TimeOut" ;
					break ;
				}
	
				await Yield() ;
			}

			if( string.IsNullOrEmpty( www.error ) == true )
			{
				// 成功
#if false
				Debug.LogWarning( "-----通信自体は成功" ) ;

				if( www.GetResponseHeaders() != null )
				{
					int i, l = www.GetResponseHeaders().Count ;
					string[] keys = new string[ l ] ;
					www.GetResponseHeaders().Keys.CopyTo( keys, 0 ) ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						Debug.LogWarning( "K:" + keys[ i ] + " V:" + www.GetResponseHeaders()[ keys[ i ] ] ) ;
					}
				}
#endif
				responseData = www.downloadHandler.data ;

#if false
				if( www.GetResponseHeaders().ContainsKey( "Response-Hash" ) == true )
				{
					responseHash = true ;
				}
#endif
			}

			www.Dispose() ;
			www = null ;

			//------------------------------------------------------------------

			// エラーは極力ステータスコードに統一して欲しい
			// エラーメッセージもレスポンスヘッダに入れてもらう

			request.HttpStatus		= httpStatus ;
			request.ErrorMessage	= string.Empty ;

			// 通信が終了した
			if( httpStatus == 200 )
			{
				//-------------------------------------------------------
				// 通信成功
#if UNITY_EDITOR
				Debug.Log( "<color=#00BFFF>======> 通信API リクエスト成功 : " + url + "</color>" ) ;
				float sessionTime = Time.realtimeSinceStartup - sessionTimeBase ;
				Debug.Log( "<color=#00DFFF>通信にかかった時間 = " + ( ( int )( sessionTime * 100 ) / 100f ) + "秒</color>" ) ;
#endif
				if( responseData != null )
				{
					Debug.Log( "<color=#FFFF00>レスポンスデータサイズ:" + responseData.Length +"</color>" ) ;
#if false
					string s = "" ;
					for( int i  = 0 ; i <  responseData.Length ; i ++ )
					{
						s += responseData[ i ].ToString( "X2" ) + " " ;
					}
					Debug.Log( s ) ;
#endif
				}

				// 必要に応じて復号化を行う
				request.ResponseData = Decrypt( responseData ) ;

				// プログレスを非表示にする
				Progress.Off() ;	

				// 実行処理終了
				m_ExecuteRequest = false ;

				// 呼び出し元タスクも停止させる
				request.Completed = true ;
			}
			else
			if( httpStatus >  0 )
			{
				// 失敗(通信は行われたがHTTPエラー)
				if( string.IsNullOrEmpty( errorMessage ) == true )
				{
					errorMessage = DataAsText( responseData ) ;
				}

				// エラーメッセージを格納する
				request.ErrorMessage = errorMessage ;

				Debug.LogWarning( "<color=#FF7F00>======> 通信API リクエスト失敗 : " + url + "</color>" ) ;
				Debug.LogWarning( "<color=#FFFF00>======= statusCode = " + httpStatus + "</color> - ErrorMessage = " + errorMessage ) ;

				// エラー処理を実行する
				await ProcessErrorAsync( true, httpStatus, errorMessage, request ) ;
			}
			else
			if( httpStatus == 0 )
			{
				// 接続できない(終了)

				Debug.LogWarning( "<color=#FF7F00>======> 通信API リクエスト失敗 : " + url + "</color>" ) ;
				Debug.LogWarning( "<color=#FFFF00>======= statusCode = " + httpStatus + " - ErrorMessage = " + errorMessage + "</color>" ) ;

				// エラーメッセージを格納する
				request.ErrorMessage = errorMessage ;

				// エラー処理を実行する
				await ProcessErrorAsync( true, httpStatus, errorMessage, request ) ;
			}
			else
			{
				// タイムアウトの場合は規定回数までリトライを行う

				Debug.LogWarning( "<color=#FF7F00>======> 通信API リクエスト失敗 : " + url + "</color>" ) ;
				Debug.LogWarning( "<color=#FFFF00>======= statusCode = " + httpStatus + " - ErrorMessage = " + errorMessage + "</color>" ) ;

				// 一応エラーメッセージを格納する
				request.ErrorMessage = errorMessage ;

				// エラー処理を実行する(リトライする)
				await ProcessErrorAsync( false, httpStatus, errorMessage, request ) ;
			}
		}

		// Unity2017のクソみたいなバグ対策(HeaderのValueに ( ) が入っているとHeader全体がおかしくなる)
		private string EscapeHttpHeaderValue( string s )
		{
			if( string.IsNullOrEmpty( s ) == true )
			{
				return string.Empty ;
			}

			s = s.Replace( "(", "&#40" ) ;
			s = s.Replace( ")", "&#41" ) ;

			return s ;
		}

#if false
		/// <summary>
		/// Http の通信ステータス値を取得する(使用していない)
		/// </summary>
		/// <param name="statusString"></param>
		/// <returns></returns>
		private int GetStatus( string statusString )
		{
			int statusValue = -1 ;

			if( string.IsNullOrEmpty( statusString ) == true )
			{
				return statusValue ;
			}

			string[] t = statusString.Split( ' ' ) ;
			if( t.Length == 3 )
			{
				if( t[ 0 ].ToLower().Contains( "http" ) == true && t[ 2 ].ToLower().Contains( "ok" ) == true )
				{
					int.TryParse( t[ 1 ], out statusValue ) ;
				}
			}

			return statusValue ;
		}
#endif

		/// <summary>
		/// バイト配列をUTF-8文字列に変換する
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		private string DataAsText( byte[] data )
		{
			if( data == null || data.Length == 0 )
			{
				return "Data is null" ;
			}

			return Encoding.UTF8.GetString( data ) ;
		}
		
		//-------------------------------------------------------------------------------------------

		// エラー処理
		private async UniTask ProcessErrorAsync( bool isFinished, int statusCode, string errorMessage, Request request )
		{
			if( isFinished == true )
			{
				// 通信は行われたがHTTPエラー

				if( request.UseDialog == false )
				{
					// ダイアログを表示しない
					Progress.Off() ;

					// エラーはWebAPI呼び出し元で処理する
					request.Completed = true ;
				}
				else
				{
					// ダイアログを表示する
					await OpenNetworkErrorDialog( statusCode, errorMessage, request ) ;
				}
			}
			else
			{
				// 通信自体が失敗している

				// リトライカウント増加
				request.RetryCount ++ ;

				if( request.RetryCount >  m_MaxRetryCount )
				{
					// アクセストークン異常かリトライ数が限界に達した(この場合は必ずエラーハンドラを呼ぶ)

					Debug.LogWarning( "==========トークン異常かリトライ限界数到達:" + m_AccessToken + " " + request.RetryCount + " / " + m_MaxRetryCount ) ;

					if( request.UseDialog == false )
					{
						// これにて終了

						// ダイアログを表示しない
						Progress.Off() ;

						// 待っているタスクがあるので終わらせる(重要)
						request.Completed = true ;
					}
					else
					{
						// ダイアログを表示する(リブートかリトライ)
						await OpenNetworkErrorDialog( statusCode, errorMessage, request ) ;
						// ※リトライの場合はダイアログタスク内で待っているタスクを停止させている(大丈夫)
					}
				}
				else
				{
					// 少し時間をおいてリトライする
					Debug.LogWarning( "<color=#3FFF3F>==========自動リトライ実行:" + request.RetryCount + " / " + m_MaxRetryCount + "</color>" ) ;

					await SendRequestAfterWait_Private( 1.0f, request ) ;	// 再実行
				}
			}
		}
		
		// エラーのステータス番号によってダイアログに必要なタイトルと説明の文言を返す
		private async UniTask OpenNetworkErrorDialog( int statusCode, string errorMessage, Request request )
		{
			string		title = "通信エラー" ;
			string[]	buttons = { Define.retry, Define.reboot } ;

			if( Define.DevelopmentMode == true )
			{
				title += " ( " + statusCode.ToString() + " )" ;
			}

			//----------------------------------------------------------

			bool executeReboot		= false ;	// リブートする場合のリブート後の開始シーン名

			// ダイアログを出すためプログレス表示中であれば消去する
			Progress.Hide() ;

			// ダイアログを出する
			int index = await Dialog.Open( title, errorMessage, buttons ) ;
			if( buttons[ index ] == Define.reboot || buttons[ index ] == Define.goto_title )
			{
				// リブート
				executeReboot = true ;
			}

			//----------------------------------------------------------

			if( executeReboot == true )
			{
				// リブートを行う

				m_ExecuteRequest = false ;

//				request.Completed = true ;	// リクエストの完了待ちタスクは終了させる
				CancelTask() ;				// 余計な処理をされると困るのでタスクを中断して(ルートまで全て中断して)それ以後の処理は行わせないようにする

				//---------------------------------

				Progress.IsOn = false ;	// プログレスの継続表示状態になっていればリセットする

				// リブートを実行する
				ApplicationManager.Reboot() ;

				// 完全にシーンの切り替えが終了する(古いシーンが破棄される)のを待つ(でないと古いシーンが悪さをする)
				await WaitWhile( () => Scene.IsFading ) ;

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

				// リトライカウントをクリア
				request.RetryCount = 0 ;

				// 少し時間をおいてリトライする
				await SendRequestAfterWait_Private( 1.0f, request ) ;
			}
		}

		// 少し時間をおいてリトライする
		private async UniTask SendRequestAfterWait_Private( float time, Request request )
		{
			await WaitForSeconds( time ) ;	// 指定時間待つ

			// 実際の処理
			_ = ExecuteRequest( request ) ;
		}
	}
}
