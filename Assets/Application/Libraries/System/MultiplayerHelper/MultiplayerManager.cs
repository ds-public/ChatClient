using UnityEngine ;
using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Text ;

#if UNITY_EDITOR
using UnityEditor ;
#endif

namespace MultiplayerHelper
{
	/// <summary>
	/// リアルタイム系のネットワーク制御クラス(Last Upadte 2018/06/29)
	/// </summary>
	public class MultiplayerManager : MonoBehaviour
	{
#if UNITY_EDITOR
		/// <summary>
		/// MultiplayerManager を生成
		/// </summary>
		[MenuItem("GameObject/Helper/MultiplayerHelper/MultiplayerManager", false, 24)]
		public static void CreateMultiplayerManager()
		{
			GameObject tGameObject = new GameObject( "MultiplayerManager" ) ;
		
			Transform tTransform = tGameObject.transform ;
			tTransform.SetParent( null ) ;
			tTransform.localPosition = Vector3.zero ;
			tTransform.localRotation = Quaternion.identity ;
			tTransform.localScale = Vector3.one ;
		
			tGameObject.AddComponent<MultiplayerManager>() ;
			Selection.activeGameObject = tGameObject ;
		}
#endif

		// シングルトンインスタンス
		private static MultiplayerManager m_Instance = null ;

		/// <summary>
		/// マネージャのインスタンス
		/// </summary>
		public  static MultiplayerManager  instance
		{
			get
			{
				return m_Instance ;
			}
		}

		//---------------------------------------------------------
	
		private MultiplayerControllerInterface	m_Controller = null ;

		//---------------------------------------------------------
	
		/// <summary>
		/// インスタンス生成（スクリプトから生成する場合）
		/// </summary>
		/// <returns></returns>
		public static MultiplayerManager Create( Transform tParent = null, MultiplayerControllerInterface tController = null )
		{
			if( m_Instance != null )
			{
				return m_Instance ;
			}
		
			// オブジェクトが非アクティブだと検出されないのでオブジェクトを非アクティブにしてはならない
			// この判定は必須で mInstance は static であるためシーンの最初はオブジェクトが存在しても null になっている
			m_Instance = GameObject.FindObjectOfType<MultiplayerManager>() ;
			if( m_Instance == null )
			{
				GameObject tGameObject = new GameObject( "MultiplayerManager" ) ;
				if( tParent != null )
				{
					tGameObject.transform.SetParent( tParent, false ) ;
				}

				tGameObject.AddComponent<MultiplayerManager>() ;
			}

			// コントローラーが null の場合はデフォルトの NetworkView を使う
			m_Instance.m_Controller = tController ;

			return m_Instance ;
		}

		/// <summary>
		/// インスタンスを破棄する
		/// </summary>
		public static void Delete()
		{	
			if( m_Instance != null )
			{
				if( Application.isPlaying == false )
				{
					DestroyImmediate( m_Instance.gameObject ) ;
				}
				else
				{
					Destroy( m_Instance.gameObject ) ;
				}
			
				m_Instance = null ;
			}
		}

		//-----------------------------------------------------------------
		
		void Awake()
		{
			// 既に存在し重複になる場合は自身を削除する
			if( m_Instance != null )
			{
				GameObject.DestroyImmediate( gameObject ) ;
				return ;
			}
		
			MultiplayerManager tInstanceOther = GameObject.FindObjectOfType<MultiplayerManager>() ;
			if( tInstanceOther != null )
			{
				if( tInstanceOther != this )
				{
					GameObject.DestroyImmediate( gameObject ) ;
					return ;
				}
			}
		
			//-----------------------------
			// Awake 内でマニュアルで実行した場合とスクリプトで実行した場合の共通の処理を行う必要がある

			m_Instance = this ;
			
			// シーン切り替え時に破棄されないようにする(ただし自身がルートである場合のみ有効)
			if( transform.parent == null )
			{
				DontDestroyOnLoad( gameObject ) ;
			}
		
			//-----------------------------
		
			// 原点じゃないと気持ち悪い
			gameObject.transform.localPosition = Vector3.zero ;
			gameObject.transform.localRotation = Quaternion.identity ;
			gameObject.transform.localScale = Vector3.one ;

			//-----------------------------

			// 各種初期化処理を行う
//			Initialize() ;
		}

		//---------------------------------------------------------------------------
		
		/// <summary>
		/// 動作モード
		/// </summary>
		public static bool isOnline = true ;

		
		/// <summary>
		/// サーバに接続中かどうか
		/// </summary>
		public static bool isConnecting
		{
			get
			{
				if( isOnline == false )
				{
					return true ;
				}

				if( m_Instance == null || m_Instance.m_Controller == null  )
				{
					return false ;
				}

				return m_Instance.m_Controller.IsConnecting() ;
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		// サーバに接続する
		/// </summary>
		/// <param name="tPlayerName"></param>
		/// <returns></returns>
		public static MultiplayerRequest ConnectServer( string tServerName, string tDefaultLobbyName = "DefaultLobby", Action<ConnectServerResult> tOnConnected = null, Action tOnDisconnectedFromServer = null, Action<MultiplayerRoomData[]> tOnLobbyStatusChanged = null )
		{
			if( m_Instance == false || m_Instance.m_Controller == null )
			{
				return null ;
			}

			MultiplayerRequest tRequest = new MultiplayerRequest( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.m_Controller.ConnectServer( tServerName, tDefaultLobbyName, tOnConnected, tOnDisconnectedFromServer, tOnLobbyStatusChanged, tRequest ) ) ;
			return tRequest ;
		}
		
		//-----------------------------

		/// <summary>
		/// サーバと切断する
		/// </summary>
		/// <param name="tOnDisconnected"></param>
		/// <returns></returns>
		public static MultiplayerRequest DisconnectServer( Action<DisconnectServerResult> tOnDisconnected = null )
		{
			if( m_Instance == false || m_Instance.m_Controller == null )
			{
				return null ;
			}

			MultiplayerRequest tRequest = new MultiplayerRequest( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.m_Controller.DisconnectServer( tOnDisconnected, tRequest ) ) ;
			return tRequest ;
		}

		/// <summary>
		/// サーバからの切断をシミュレートする(デバッグ用)
		/// </summary>
		/// <param name="tOnDisconnected"></param>
		/// <returns></returns>
		public static bool DisconnectFromServer()
		{
			if( m_Instance == false || m_Instance.m_Controller == null )
			{
				return false ;
			}

			return m_Instance.m_Controller.DisconnectFromServer() ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// ロビーに入室中かどうか
		/// </summary>
		public static bool inLobby
		{
			get
			{
				if( isOnline == false )
				{
					return true ;
				}

				if( m_Instance == null || m_Instance.m_Controller == null )
				{
					return false ;
				}

				return m_Instance.m_Controller.InLobby() ;
			}
		}

		//-----------------------------------

		/// <summary>
		// ロビーに入室または作成する
		/// </summary>
		/// <param name="tPlayerName"></param>
		/// <returns></returns>
		public static MultiplayerRequest JoinLobby( string tLobbyName, Action<JoinLobbyResult> tOnLobbyJoined = null, Action<MultiplayerRoomData[]> tOnLobbyStatusChanged = null )
		{
			if( m_Instance == false || m_Instance.m_Controller == null )
			{
				return null ;
			}

			MultiplayerRequest tRequest = new MultiplayerRequest( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.m_Controller.JoinLobby( tLobbyName, tOnLobbyJoined, tOnLobbyStatusChanged, tRequest ) ) ;
			return tRequest ;
		}
		
		//-----------------------------------

		/// <summary>
		// ロビーを退室する
		/// </summary>
		/// <param name="tPlayerName"></param>
		/// <returns></returns>
		public static MultiplayerRequest LeaveLobby( Action<LeaveLobbyResult> tOnLobbyLeft = null )
		{
			if( m_Instance == false || m_Instance.m_Controller == null )
			{
				return null ;
			}

			MultiplayerRequest tRequest = new MultiplayerRequest( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.m_Controller.LeaveLobby( tOnLobbyLeft, tRequest ) ) ;
			return tRequest ;
		}
		
		//-----------------------------------

		/// <summary>
		/// ルーム名の一覧を取得する
		/// </summary>
		/// <returns></returns>
		public static string[] GetRoomName( Action<MultiplayerRoomData[]> tOnLobbyStatusChanged = null )
		{
			if( m_Instance == null || m_Instance.m_Controller == null )
			{
				return null ;
			}

			return m_Instance.m_Controller.GetRoomName( tOnLobbyStatusChanged ) ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// ルームに入室中かどうか
		/// </summary>
		public static bool inRoom
		{
			get
			{
				if( isOnline == false )
				{
					return true ;
				}

				if( m_Instance == null || m_Instance.m_Controller == null )
				{
					return false ;
				}

				return m_Instance.m_Controller.InRoom() ;
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		// ルームを作成する
		/// </summary>
		/// <param name="tPlayerName"></param>
		/// <returns></returns>
		public static MultiplayerRequest CreateRoom( string tRoomName, int tMaxPlayer = 0, bool tVisible = true, bool tOpen = true, string tPassword = null, string tComment = null, string tLobbyName = null, Action<CreateRoomResult> tOnRoomCreated = null, Action<int,bool> tOnRoomStatusChanged = null, Action<int> tOnRoomHostChanged = null )
		{
			if( m_Instance == false || m_Instance.m_Controller == null )
			{
				return null ;
			}

			MultiplayerRequest tRequest = new MultiplayerRequest( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.m_Controller.CreateRoom( tRoomName, tMaxPlayer, tVisible, tOpen, tPassword, tComment, tLobbyName, tOnRoomCreated, tOnRoomStatusChanged, tOnRoomHostChanged, tRequest ) ) ;
			return tRequest ;
		}
		
		/// <summary>
		// ルームに入室する
		/// </summary>
		/// <param name="tPlayerName"></param>
		/// <returns></returns>
		public static MultiplayerRequest JoinRoom( string tRoomName, string tPassword = null, Action<JoinRoomResult,int[]> tOnRoomJoined = null, Action<int,bool> tOnRoomStatusChanged = null, Action<int> tOnRoomHostChanged = null )
		{
			if( m_Instance == false || m_Instance.m_Controller == null )
			{
				return null ;
			}

			MultiplayerRequest tRequest = new MultiplayerRequest( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.m_Controller.JoinRoom( tRoomName, tPassword, tOnRoomJoined, tOnRoomStatusChanged, tOnRoomHostChanged, tRequest ) ) ;
			return tRequest ;
		}
		
		/// <summary>
		// ルームに入室または作成する
		/// </summary>
		/// <param name="tPlayerName"></param>
		/// <returns></returns>
		public static MultiplayerRequest JoinOrCreateRoom( string tRoomName, int tMaxPlayer = 0, bool tVisible = true, bool tOpen = true, string tPassword = null, string tComment = null, string tLobbyName = null, Action<JoinOrCreateRoomResult,int[]> tOnRoomJoinedOrCreated = null, Action<int,bool> tOnRoomStatusChanged = null, Action<int> tOnRoomHostChanged = null )
		{
			if( m_Instance == false || m_Instance.m_Controller == null )
			{
				return null ;
			}

			MultiplayerRequest tRequest = new MultiplayerRequest( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.m_Controller.JoinOrCreateRoom( tRoomName, tMaxPlayer, tVisible, tOpen, tPassword, tComment, tLobbyName, tOnRoomJoinedOrCreated, tOnRoomStatusChanged, tOnRoomHostChanged, tRequest ) ) ;
			return tRequest ;
		}
		
		/// <summary>
		// ランダムにルームに入室する(将来的に条件指定に対応する)
		/// </summary>
		/// <param name="tPlayerName"></param>
		/// <returns></returns>
		public static MultiplayerRequest JoinRandomRoom( Action<JoinRoomResult,int[]> tOnRoomJoined = null, Action<int,bool> tOnRoomStatusChanged = null, Action<int> tOnRoomHostChanged = null )
		{
			if( m_Instance == false || m_Instance.m_Controller == null )
			{
				return null ;
			}

			MultiplayerRequest tRequest = new MultiplayerRequest( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.m_Controller.JoinRandomRoom( tOnRoomJoined, tOnRoomStatusChanged, tOnRoomHostChanged, tRequest ) ) ;
			return tRequest ;
		}
		
		//-----------------------------------

		/// <summary>
		// ルームを退室する
		/// </summary>
		/// <param name="tPlayerName"></param>
		/// <returns></returns>
		public static MultiplayerRequest LeaveRoom( Action<LeaveRoomResult> tOnRoomLeft )
		{
			if( m_Instance == false || m_Instance.m_Controller == null )
			{
				return null ;
			}

			MultiplayerRequest tRequest = new MultiplayerRequest( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.m_Controller.LeaveRoom( tOnRoomLeft,  tRequest ) ) ;
			return tRequest ;
		}
		
		//-----------------------------------------------------------

		/// <summary>
		/// 自身がホストかどうか
		/// </summary>
		public static bool isHost
		{
			get
			{
				if( isOnline == false )
				{
					return true ;
				}

				if( m_Instance == null || m_Instance.m_Controller == null )
				{
					return false ;
				}

				return m_Instance.m_Controller.IsHost() ;
			}
		}

		/// <summary>
		/// 自身のプレイヤーＩＤを返す
		/// </summary>
		public static int playerId
		{
			get
			{
				if( isOnline == false )
				{
					return 1 ;
				}

				if( m_Instance == null || m_Instance.m_Controller == null )
				{
					return 0 ;
				}

				return m_Instance.m_Controller.PlayerId() ;
			}
		}

		/// <summary>
		/// 自身のプレイヤー名を返す
		/// </summary>
		public static string playerName
		{
			get
			{
				if( isOnline == false )
				{
					return "Player" ;
				}

				if( m_Instance == null || m_Instance.m_Controller == null )
				{
					return null ;
				}

				return m_Instance.m_Controller.GetPlayerName() ;
			}
			set
			{
				if( string.IsNullOrEmpty( value ) == true )
				{
					return ;
				}

				if( m_Instance == null || m_Instance.m_Controller == null )
				{
					return ;
				}

				m_Instance.m_Controller.SetPlayerName( value ) ;
			}
		}

		/// <summary>
		/// プレイヤー全員のＩＤを返す
		/// </summary>
		public static int[] playerIds
		{
			get
			{
				if( isOnline == false )
				{
					return new int[]{ 1 } ;
				}

				if( m_Instance == null || m_Instance.m_Controller == null )
				{
					return null ;
				}

				return m_Instance.m_Controller.PlayerIds() ;
			}
		}

		/// <summary>
		/// ルーム内のプレイヤー全員の名前を返す
		/// </summary>
		public static string[] playerNames
		{
			get
			{
				if( isOnline == false )
				{
					return new string[]{ "Player" } ;
				}

				if( m_Instance == null || m_Instance.m_Controller == null )
				{
					return null ;
				}

				return m_Instance.m_Controller.PlayerNames() ;
			}
		}

		/// <summary>
		/// 自身を除くプレイヤー全員のＩＤを返す
		/// </summary>
		public static int[] otherPlayerIds
		{
			get
			{
				if( isOnline == false )
				{
					return null ;
				}

				if( m_Instance == null || m_Instance.m_Controller == null )
				{
					return null ;
				}

				return m_Instance.m_Controller.OtherPlayerIds() ;
			}
		}

		/// <summary>
		/// 自身を除くプレイヤー全員の名前を返す
		/// </summary>
		public static string[] otherPlayerNames
		{
			get
			{
				if( isOnline == false )
				{
					return null ;
				}

				if( m_Instance == null || m_Instance.m_Controller == null )
				{
					return null ;
				}

				return m_Instance.m_Controller.OtherPlayerNames() ;
			}
		}

		/// <summary>
		/// ホストのプレイヤーＩＤを返す
		/// </summary>
		public static int hostPlayerId
		{
			get
			{
				if( isOnline == false )
				{
					return 1 ;
				}

				if( m_Instance == null || m_Instance.m_Controller == null )
				{
					return 0 ;
				}

				return m_Instance.m_Controller.HostPlayerId() ;
			}
		}
		
		/// <summary>
		/// プレイヤーＩＤに該当するプレイヤー名を取得する
		/// </summary>
		/// <param name="tPlayerId"></param>
		/// <returns></returns>
		public static string GetPlayerNameById( int tPlayerId )
		{
			if( isOnline == false )
			{
				return "Player" ;
			}

			if( m_Instance == null || m_Instance.m_Controller == null )
			{
				return null ;
			}

			return m_Instance.m_Controller.GetPlayerNameById( tPlayerId ) ;
		}

		/// <summary>
		/// 入室中のロビー名を返す
		/// </summary>
		public static string lobbyName
		{
			get
			{
				if( isOnline == false )
				{
					return "Lobby" ;
				}

				if( m_Instance == null || m_Instance.m_Controller == null )
				{
					return null ;
				}

				return m_Instance.m_Controller.LobbyName() ;
			}
		}
		
		/// <summary>
		/// 入室中のルーム名を返す
		/// </summary>
		public static string roomName
		{
			get
			{
				if( isOnline == false )
				{
					return "Room" ;
				}

				if( m_Instance == null || m_Instance.m_Controller == null )
				{
					return null ;
				}

				return m_Instance.m_Controller.RoomName() ;
			}
		}

		/// <summary>
		/// 現在接続中のサーバーの全ロビー情報を取得する
		/// </summary>
		/// <param name="tPlayerId"></param>
		/// <returns></returns>
		public static MultiplayerLobbyData[] GetLobbyData()
		{
			if( isOnline == false )
			{
				return null ;
			}

			if( m_Instance == null || m_Instance.m_Controller == null )
			{
				return null ;
			}

			return m_Instance.m_Controller.GetLobbyData() ;
		}
		
		/// <summary>
		/// 現在入室中のロビーの全ルーム情報を取得する
		/// </summary>
		/// <param name="tPlayerId"></param>
		/// <returns></returns>
		public static MultiplayerRoomData[] GetRoomData()
		{
			if( isOnline == false )
			{
				return null ;
			}

			if( m_Instance == null || m_Instance.m_Controller == null )
			{
				return null ;
			}

			return m_Instance.m_Controller.GetRoomData() ;
		}



		//---------------------------------------------------------------------

		/// <summary>
		/// 指定したプレイヤーに文字列を送信する
		/// </summary>
		/// <param name="tString"></param>
		public static bool Send( int tPlayerId, bool tSecure, params System.Object[] tObjects )
		{
			if( m_Instance == null || m_Instance.m_Controller == null )
			{
				return false ;
			}

			return m_Instance.m_Controller.Send( tPlayerId, tSecure, tObjects ) ;
		}

		/// <summary>
		/// プレイヤー全員に文字列を送信する
		/// </summary>
		/// <param name="tString"></param>
		public static bool SendToAll( bool tViaServer, bool tSecure, params System.Object[] tObjects )
		{
			if( m_Instance == null || m_Instance.m_Controller == null )
			{
				return false ;
			}

			return m_Instance.m_Controller.SendToAll( tViaServer, tSecure, tObjects ) ;
		}

		/// <summary>
		/// ホスト限定で文字列を送信する
		/// </summary>
		/// <param name="tString"></param>
		public static bool SendToHost( bool tViaServer, bool tSecure, params System.Object[] tObjects )
		{
			if( m_Instance == null || m_Instance.m_Controller == null )
			{
				return false ;
			}

			return m_Instance.m_Controller.SendToHost( tViaServer, tSecure, tObjects ) ;
		}

		/// <summary>
		/// 受信コールバックを設定する
		/// </summary>
		/// <param name="tOnReceived"></param>
		public static bool SetOnReceived( Action<bool,int,System.Object[]> tOnReceived )
		{
			if( m_Instance == null || m_Instance.m_Controller == null )
			{
				return false ;
			}

			return m_Instance.m_Controller.SetOnReceived( tOnReceived ) ;
		}

		/// <summary>
		/// 何等かの情報を受信するまで待つ(待ち受け中にエラーが発生した場合はデータ無し状態のコールバックが発生する)
		/// </summary>
		/// <param name="tOnRoomLeft"></param>
		/// <returns></returns>
		public static MultiplayerRequest Receive( Action<bool,int,System.Object[]> tOnRecievied )
		{
			if( m_Instance == false || m_Instance.m_Controller == null )
			{
				return null ;
			}

			MultiplayerRequest tRequest = new MultiplayerRequest( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.m_Controller.Receive( tOnRecievied, tRequest ) ) ;
			return tRequest ;
		}

		//---------------------------------------------------------------------------

		/// <summary>
		/// ボイスアダプターを接続する(OnPreDecodeは PlayerId Data Channels SamplingRate)
		/// </summary>
		/// <param name="tOnReceived"></param>
		public static MultiplayerRequest  AttachVoiceAdapter( int tPlayerId, Transform tParent, bool t3D = true, int tSamplingRate = 16000, int tPacketTime = 40, Action<int,float[],int,int> tOnPreDecode = null )
		{
			if( m_Instance == null || m_Instance.m_Controller == null )
			{
				return null ;
			}

			MultiplayerRequest tRequest = new MultiplayerRequest( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.m_Controller.AttachVoiceAdapter( tPlayerId, tParent, t3D, tSamplingRate, tPacketTime, tOnPreDecode, tRequest ) ) ;
			return tRequest ;
		}
		
		/// <summary>
		/// ボイスアダプターを除去する
		/// </summary>
		/// <param name="tPlayerId"></param>
		/// <returns></returns>
		public static bool DetachVoiceAdapter( int tPlayerId )
		{
			if( m_Instance == null || m_Instance.m_Controller == null )
			{
				return false ;
			}

			return m_Instance.m_Controller.DetachVoiceAdapter( tPlayerId ) ;
		}

		/// <summary>
		/// ボイスのサンプリングレートを取得する
		/// </summary>
		/// <param name="tPlayerId"></param>
		/// <returns></returns>
		public static int GetVoiceSamplingRate( int tPlayerId )
		{
			if( m_Instance == null || m_Instance.m_Controller == null )
			{
				return 0 ;
			}

			return m_Instance.m_Controller.GetVoiceSamplingRate( tPlayerId ) ;
		}

		/// <summary>
		/// ボイスの録音を開始する
		/// </summary>
		/// <param name="tPlayerId"></param>
		/// <returns></returns>
		public static bool StartVoiceRecording()
		{
			if( m_Instance == null || m_Instance.m_Controller == null )
			{
				return false ;
			}

			return m_Instance.m_Controller.StartVoiceRecording() ;
		}

		/// <summary>
		/// ボイスの録音を停止する
		/// </summary>
		/// <param name="tPlayerId"></param>
		/// <returns></returns>
		public static bool StopVoiceRecording()
		{
			if( m_Instance == null || m_Instance.m_Controller == null )
			{
				return false ;
			}

			return m_Instance.m_Controller.StopVoiceRecording() ;
		}
		
		//---------------------------------------------------------------------------

		/// <summary>
		/// コールバック群のクリアを行う
		/// </summary>
		public static void Terminate()
		{
			if( m_Instance == false || m_Instance.m_Controller == null )
			{
				return ;
			}

			m_Instance.m_Controller.Terminate() ;
		}
		
		//---------------------------------------------------------------------------

		private static System.DateTime UNIX_EPOCH = new System.DateTime( 1970, 1, 1, 0, 0, 0, 0 ) ;

		/// <summary>
		/// 1970年1月1日0時0分0秒からの経過秒を取得する
		/// </summary>
		/// <returns></returns>
		public static long GetClientTime()
		{
			System.DateTime tUTC = System.DateTime.UtcNow ;
			tUTC = tUTC.ToUniversalTime() ;
			System.TimeSpan tTimeSpan = tUTC - UNIX_EPOCH ;
			return (  long )tTimeSpan.TotalSeconds ;
		}
	}
}

