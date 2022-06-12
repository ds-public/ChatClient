using System ;
using System.Text ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using Cysharp.Threading.Tasks ;

using UnityEngine ;
using UnityEngine.Networking ;

using uGUIHelper ;
using System.Threading ;

namespace Template
{
	/// <summary>
	/// ファイルダウンロードを制御するクラス Version 2022/06/12 0
	/// </summary>
	public class DownloadManager : SingletonManagerBase<DownloadManager>
	{
		// サーバー名
		[SerializeField]
		private string m_ServerName = null ;

		/// <summary>
		/// サーバー名
		/// </summary>
		public static string ServerName
		{
			get
			{
				if( m_Instance == null )
				{
					Debug.LogError( "DownloadManager is not create !" ) ;
					return "" ;
				}
				return m_Instance.m_ServerName ;
			}
			set
			{
				if( m_Instance == null )
				{
					Debug.LogError( "DownloadManager is not create !" ) ;
					return ;
				}
				if( m_Instance.m_ServerName != value )
				{
					Debug.Log( "サーバー名が変更された:" + value + " ← " + m_Instance.m_ServerName ) ;
					m_Instance.m_ServerName = value ;
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
					Debug.LogError( "DownloadManager is not create !" ) ;
					return 0 ;
				}
				return m_Instance.m_ConnectionTimeout ;
			}
			set
			{
				if( m_Instance == null )
				{
					Debug.LogError( "DownloadManager is not create !" ) ;
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
					Debug.LogError( "DownloadManager is not create !" ) ;
					return 0 ;
				}
				return m_Instance.m_Timeout ;
			}
			set
			{
				if( m_Instance == null )
				{
					Debug.LogError( "DownloadManager is not create !" ) ;
					return ;
				}
				m_Instance.m_Timeout = value ;
			}
		}

		// 最大リトライ回数
		[SerializeField]
		private int m_MaxRetryCount = 1 ;

		/// <summary>
		/// 最大リトライ回数
		/// </summary>
		public static int MaxRetryCount
		{
			get
			{
				if( m_Instance == null )
				{
					Debug.LogError( "DownloadManager is not create !" ) ;
					return 0 ;
				}
				return m_Instance.m_MaxRetryCount ;
			}
			set
			{
				if( m_Instance == null )
				{
					Debug.LogError( "DownloadManager is not create !" ) ;
					return ;
				}
				m_Instance.m_MaxRetryCount = value ;
			}
		}

		// 最大並列実行数
		[SerializeField]
		private int m_MaxParallel = 6 ;

		/// <summary>
		/// 最大並列実行数
		/// </summary>
		public static int MaxParallel
		{
			get
			{
				if( m_Instance == null )
				{
					Debug.LogError( "DownloadManager is not create !" ) ;
					return 0 ;
				}
				return m_Instance.m_MaxParallel ;
			}
			set
			{
				if( m_Instance == null )
				{
					Debug.LogError( "DownloadManager is not create !" ) ;
					return ;
				}
				m_Instance.m_MaxParallel = value ;
			}
		}

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

		//-------------------------------------------------------------------------------------------

		new protected void Awake()
		{
			base.Awake() ;

			// デフォルトのタイムアウト時間
			m_Timeout = 60.0f ;

			// 最大リトライ回数
			m_MaxRetryCount = 1 ;
	   }

		//---------------------------------------------------------------------------
		
		/// <summary>
		/// リクエストの詳細情報クラス
		/// </summary>
		/// <typeparam name="MapperType"></typeparam>
		public class Request
		{
			public string								Path ;
			public Action<int,int>						OnProgress ;
			public bool									UseProgress ;
			public bool									UseDialog ;
			public string								Title ;
			public string								Message ;

			public Request( string path, Action<int,int> onProgress, bool useProgress, bool useDialog, string title, string message )
			{
				Path		= path ;
				OnProgress	= onProgress ;
				UseProgress	= useProgress ;
				UseDialog	= useDialog ;
				Title		= title ;
				Message		= message ;

				RetryCount	= 0 ;
				Completed	= false ;
			}

			//----------------------------------

			// ワーク変数
			public int		RetryCount ;

			public bool		Completed ;
			public byte[]	ResponseData ;

			public UnityWebRequest	WWW ;
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
		public static async UniTask<byte[]> SendRequest( string path, Action<int, int> onProgress = null, bool useProgress = false, bool useDialog = true, string title = null, string message = null )
		{
			if( m_Instance == null )
			{
				Debug.LogError( "DownloadManager is not create !" ) ;
				return null ;
			}

			return await m_Instance.MakeRequest( path, onProgress, useProgress, useDialog, title, message ) ;
		}

		//---------------------------
		
		// 通信ＡＰＩのリクエストを生成する
		private async UniTask<byte[]> MakeRequest( string path, Action<int, int> onProgress = null, bool useProgress = false, bool useDialog = true, string title = null, string message = null )
		{
			// リクエストの詳細情報をワンオブジェクトにまとめる
			Request request = new Request( path, onProgress, useProgress, useDialog, title, message ) ;

			// リクエストを送信する
			if( PushRequest( request ) == false )
			{
				return null ;	// 何らかのエラーが発生した
			}

			await WaitUntil( () => request.Completed ) ;

			return request.ResponseData ;
		}

		//-------------------------------------------------------------------

		private readonly List<Request>	m_RequestQueue = new List<Request>() ;

		private bool m_ProcessRequest = false ;

		private readonly List<Request>	m_ProcessQueue = new List<Request>() ;

		/// <summary>
		/// リクエストを追加する
		/// </summary>
		/// <param name="requestData"></param>
		/// <returns></returns>
		private bool PushRequest( Request request )
		{
			// リクエストをキューに加える
			m_RequestQueue.Add( request ) ;

			if( m_ProcessRequest == false )
			{
				// リクエストの処理が実行中でなければ実行する
				m_ProcessRequest = true ;
				ProcessRequest().Forget() ;
			}

			return true ;
		}

		/// <summary>
		/// リクエストを処理する(タスクは１実行)
		/// </summary>
		/// <returns></returns>
		private async UniTask ProcessRequest()
		{
			while( true )
			{
				if( m_RequestQueue.Count == 0 )
				{
					// リクエストが存在しない
					m_ProcessRequest = false ;	// リクエストの処理の実行終了
					return ;
				}

				//----------------------------------

				if( m_ProcessQueue.Count <  m_MaxParallel )
				{
					// リクエストが処理可能な状態
					Request request = m_RequestQueue[ 0 ] ;
					m_RequestQueue.RemoveAt( 0 ) ;

					m_ProcessQueue.Add( request ) ;
					ExecuteRequest( request ).Forget() ;

					Debug.Log( "<color=#FF3F00>[DownloadManager] Parallel = " + m_ProcessQueue.Count + " / " + m_MaxParallel + "</color>" ) ;
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
				url = FormatUrl( m_ServerName, path ) ;
			}
			
			//--------------------------------------------------------------------------

			// 通信が機内モードなどになっている場合は最初からエラーを出す
			if( Application.internetReachability == NetworkReachability.NotReachable )
			{
				Debug.LogError( "Network connection is not reachable: " + path ) ;
				await ProcessErrorAsync( true, -9, "Airplane mode", request ) ;
				return ;
			}

			//------------------------------------------------------------------------------------------

			UnityWebRequest www = null ;

			// ＨＴＴＰヘッダ
			Dictionary<string,string> header = new Dictionary<string, string>()
			{
				// バイト配列通信限定
				{  "Content-Type", "application/octet-stream" }
			} ;

			//--------------------------------------------------------------------------
			// 常に設定する内部固定ヘッダー

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
			// 任意追加する外部可変ヘッダー

//			Debug.Log( "------------>可変ヘッダの登録数:" + m_ConstantHeaders.Count ) ;
			if( m_ConstantHeaders.Count >  0 )
			{
				foreach( var constantHeader in m_ConstantHeaders )
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

			//------------------------------------------------------------------------------------------

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

			//--------------------------------------------------------------------------

			// キャッシュされないように URL を毎回少し変化させる
			if( url.Contains( "?" ) == false )
			{
				url += ( "?ncts=" + ClientTime.GetCurrentUnixTime() ) ;
			}
			else
			{
				url += ( "&ncts=" + ClientTime.GetCurrentUnixTime() ) ;
			}

#if UNITY_EDITOR
			Debug.Log( "<color=#FFDF00><====== ダウンロード リクエスト送信 : " + url + "</color>" ) ;
#endif
			//--------------------------------------------------------------------------

			// バイト配列を送信する場合は UploadHandlerRaw を使う必要があるためスタティックメソッド(.Post)は使用できない(=文字列限定)
			www = new UnityWebRequest( url, "GET" )
			{
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
//					Debug.Log( "<color=#FF3FFF>HTTP Header : Key = " + keys[ i ] + " Value = " + header[ keys[ i ] ] + "</color>" ) ;
					www.SetRequestHeader( keys[ i ], header[ keys[ i ] ] ) ;
				}
			}

			//----------------------------------

			// 通信開始

			// リクエスト実行
			_ = www.SendWebRequest() ;
			request.WWW = www ;

			if( useProgress == true )
			{
				// プログレスを表示する
//				Progress.On( Progress.Styles.LoadingLong ) ;
				Progress.On( Progress.Styles.WebAPI, false, "ダウンロード中", true ) ;
			}

			//--------------------------------------------------------------------------

			// 以下レスポンスの処理

			// トータルのタイムアウトを設定
//			Debug.Log( "タイムアウト時間:" + m_Timeout ) ;

			int statusCode = 0 ;
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
					statusCode = ( int )www.responseCode ;
					errorMessage = www.error ;
					break ;
				}

				if( www.isDone == true )
				{
					// 成功
					statusCode = ( int )www.responseCode ;
					break ;
				}

				if( m_Timeout >  0 && ( Time.realtimeSinceStartup - baseTime ) >  m_Timeout )
				{
					Debug.LogWarning( "タイムアウトしました:" + ( Time.realtimeSinceStartup - baseTime ) ) ;
					statusCode = -1 ;
					errorMessage = "TimeOut" ;
					break ;
				}
	
				await Yield() ;
			}

			if( string.IsNullOrEmpty( www.error ) == true )
			{
				// 成功
				responseData = www.downloadHandler.data ;
			}

			request.WWW = null ;
			www.Dispose() ;
			www = null ;

			//------------------------------------------------------------------

			// エラーは極力ステータスコードに統一して欲しい
			// エラーメッセージもレスポンスヘッダに入れてもらう

			// 通信が終了した
			if( statusCode == 200 )
			{
				//-------------------------------------------------------
				// 通信成功

#if UNITY_EDITOR
				Debug.Log( "<color=#FFDF00>======> ダウンロード リクエスト成功 : " + url + "</color>" ) ;
				if( responseData != null )
				{
					Debug.Log( "<color=#FFAF00>[ダウンロードデータサイズ] " + responseData.Length + " ( " + ExString.GetSizeName( responseData.Length ) + " ) </color>" ) ;
//					string s = "" ;
//					for( int i  = 0 ; i <  responseData.Length ; i ++ )
//					{
//						s += responseData[ i ].ToString( "X2" ) + " " ;
//					}
//					Debug.Log( s ) ;
				}
#endif
				request.ResponseData = responseData ;

				// プログレスを非表示にする
				if( useProgress == true && Progress.IsOn == true )
				{
					await Progress.OffAsync() ;	
				}

				// 呼び出し元タスクも停止させる
				request.Completed = true ;

				// 実行処理終了
				m_ProcessQueue.Remove( request ) ;
			}
			else
			if( statusCode >  0 )
			{
				// 失敗(通信は行われたがHTTPエラー)
				if( string.IsNullOrEmpty( errorMessage ) == true )
				{
					errorMessage = DataAsText( responseData ) ;
				}

#if UNITY_EDITOR
				Debug.LogWarning( "======> 通信API リクエスト失敗 : " + url ) ;
				Debug.LogWarning( "======= statusCode = " + statusCode + " - ErrorMessage = " + errorMessage ) ;
#endif
				// エラー処理を実行する
				await ProcessErrorAsync( true, statusCode, errorMessage, request ) ;
			}
			else
			if( statusCode == 0 )
			{
				// 接続できない(終了)

#if UNITY_EDITOR
				Debug.LogWarning( "======> 通信API リクエスト失敗 : " + url ) ;
				Debug.LogWarning( "======= statusCode = " + statusCode + " - ErrorMessage = " + errorMessage ) ;
#endif
				// エラー処理を実行する
				await ProcessErrorAsync( true, statusCode, errorMessage, request ) ;
			}
			else
			{
				// タイムアウトの場合は規定回数までリトライを行う

#if UNITY_EDITOR
				Debug.LogWarning( "======> 通信API リクエスト失敗 : " + url ) ;
				Debug.LogWarning( "======= statusCode = " + statusCode + " - ErrorMessage = " + errorMessage ) ;
#endif					
				// エラー処理を実行する(リトライする)
				await ProcessErrorAsync( false, statusCode, errorMessage, request ) ;
			}
		}

		// Unity2017のクソみたいなバグ対策(HeaderのValueに ( ) が入っているとHeader全体がおかしくなる)
		private string EscapeHttpHeaderValue( string s )
		{
			s = s.Replace( "(", "&#40" ) ;
			s = s.Replace( ")", "&#41" ) ;
			return s ;
		}

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
					await Progress.OffAsync() ;

					// エラーはWebAPI呼び出し元で処理する
					request.Completed = true ;

					// 実行処理終了
					m_ProcessQueue.Remove( request ) ;
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

				if( request.RetryCount >= m_MaxRetryCount )
				{
					// アクセストークン異常かリトライ数が限界に達した(この場合は必ずエラーハンドラを呼ぶ)

#if UNITY_EDITOR
					Debug.LogWarning( "==========トークン異常かリトライ限界数到達:" + request.RetryCount + " / " + m_MaxRetryCount ) ;
#endif

					if( request.UseDialog == false )
					{
						// ダイアログを表示しない
						await Progress.OffAsync() ;

						// 待っているタスクがあるので終わらせる(重要)
						request.Completed = true ;

						// 実行処理終了
						m_ProcessQueue.Remove( request ) ;
					}
					else
					{
						// ダイアログを表示する
						await OpenNetworkErrorDialog( statusCode, errorMessage, request ) ;
					}
				}
				else
				{
					// 少し時間をおいてリトライする
#if UNITY_EDITOR
					Debug.LogWarning( "==========自動リトライ実行:" + request.RetryCount + " / " + m_MaxRetryCount ) ;
#endif
					await SendRequestAfterWait_Private( 1.0f, request ) ;	// 再実行
				}
			}
		}
		
		// エラーのステータス番号によってダイアログに必要なタイトルと説明の文言を返す
		private async UniTask OpenNetworkErrorDialog( int statusCode, string errorMessage, Request request )
		{
			string		title	= request.Title ;
			string		message = request.Message ;

			string[]	buttons = { Define.retry, Define.reboot } ;

			switch( statusCode )
			{
				// その他エラー
				default :

					// ユーザーデータがあり、チュートリアル中の場合は
					// 汎用エラーを表示しタイトルへ遷移させる
					// title = Define.general_error_title;

					buttons			= new string[]{ Define.retry, Define.reboot } ;         // 再実行ボタンの文言・再起動ボタンの文言
					
					// ダイアログを表示し、通信の再実行か再起動をしてタイトルへ遷移させる

					// 通信を再実行する場合
					// args.Retry();

					// 再起動する場合は
					// タイトル画面へ遷移させる

				break ;
			}

			if( string.IsNullOrEmpty( title ) == true )
			{
#if UNITY_EDITOR
				title += " ( " + statusCode.ToString() + " )" ;
#else
				if( Debug.isDebugBuild == true )
				{
					title += " ( " + statusCode.ToString() + " )" ;
				}
#endif
			}

			if( string.IsNullOrEmpty( message ) == true )
			{
				message = errorMessage ;
			}

			//----------------------------------------------------------

			bool executeReboot		= false ;	// リブートする場合のリブート後の開始シーン名

			// ダイアログを出すためプログレス表示中であれば消去する
			Progress.Hide() ;

			// ダイアログを出する
			int index = await Dialog.Open( title, message, buttons ) ;
			if( buttons[ index ] == Define.reboot || buttons[ index ] == Define.goto_title )
			{
				// リブート
				executeReboot = true ;
			}

			//----------------------------------------------------------

			if( executeReboot == true )
			{
				// リブートを行う

				TerminateRequest() ;		// リクエストはを全てクリアする

//				request.Completed = true ;	// リクエストの完了待ちタスクは終了させる
				CancelTask() ;				// 余計な処理をされると困るのでタスクを中断して(ルートまで全て中断して)それ以後の処理は行わせないようにする

				//---------------------------------

				Progress.IsOn = false ;	// プログレスの継続表示状態になっていればリセットする

				// リブートする
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
			ExecuteRequest( request ).Forget() ;
		}

		//-------------------------------------------------------------------------------------------

		// リクエストを全て消去し通信中のプロセスを全て停止させる
		private void TerminateRequest()
		{
			m_RequestQueue.Clear() ;

			if( m_ProcessQueue.Count >  0 )
			{
				foreach( var processQueue in m_ProcessQueue )
				{
					if( processQueue.WWW != null )
					{
						processQueue.WWW.Dispose() ;
						processQueue.WWW = null ;
					}
				}
			}

			m_ProcessQueue.Clear() ;
		}

		// マネージャが破棄される際に呼び出される
		protected override void OnTerminate()
		{
			TerminateRequest() ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 登録または実行中のリクエストを停止させる
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static bool RemoveRequest( string path )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_Instance.RemoveRequest_Private( path ) ;

			return true ;
		}

		// 登録または実行中のリクエストを停止させる
		private bool RemoveRequest_Private( string path )
		{
			if( m_RequestQueue.Count >  0 )
			{
				var records = m_RequestQueue.Where( _ => _.Path == path ).ToArray() ;
				if( records != null && records.Length >  0 )
				{
					m_RequestQueue.RemoveRange( records ) ;
				}
			}

			if( m_ProcessQueue.Count >  0 )
			{
				var records = m_ProcessQueue.Where( _ => _.Path == path ).ToArray() ;
				if( records != null && records.Length >  0 )
				{
					foreach( var record in records )
					{
						if( record.WWW != null )
						{
							record.WWW.Dispose() ;
							record.WWW = null ;
						}

						m_ProcessQueue.Remove( record ) ;
					}
				}
			}

			return true ;
		}
	}
}
