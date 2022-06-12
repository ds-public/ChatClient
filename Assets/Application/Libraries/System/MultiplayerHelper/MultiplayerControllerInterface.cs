using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

namespace MultiplayerHelper
{
	public interface MultiplayerControllerInterface
	{
		/// <summary>
		/// サーバーに接続中かどうか
		/// </summary>
		/// <returns></returns>
		bool IsConnecting() ;

		/// <summary>
		/// サーバーに接続する
		/// </summary>
		/// <param name="tServerName"></param>
		/// <param name="tPlayerName"></param>
		/// <param name="tOnConnected"></param>
		/// <param name="tOnLobbyStatusChanged"></param>
		/// <param name="tRequest"></param>
		/// <returns></returns>
		IEnumerator ConnectServer( string tServerName, string tDefaultLobbyName, Action<ConnectServerResult> tOnConnected, Action tOnDisconnectedFromServer, Action<MultiplayerRoomData[]> tOnLobbyStatusChanged, MultiplayerRequest tRequest ) ;

		/// <summary>
		/// サーバーと切断する
		/// </summary>
		/// <param name="tOnDisconnected"></param>
		/// <param name="tRequest"></param>
		/// <returns></returns>
		IEnumerator DisconnectServer( Action<DisconnectServerResult> tOnDisconnected, MultiplayerRequest tRequest ) ;

		/// <summary>
		/// サーバーからの強制切断をシミュレートする
		/// </summary>
		bool DisconnectFromServer() ;

		//-----------------------------------------------------------

		/// <summary>
		/// ロビーに入室中かどうか
		/// </summary>
		/// <returns></returns>
		bool InLobby() ;

		/// <summary>
		/// ロビーに入室または作成する
		/// </summary>
		/// <param name="tLobbyName"></param>
		/// <param name="tRequest"></param>
		/// <returns></returns>
		IEnumerator JoinLobby( string tLobbyName, Action<JoinLobbyResult> tOnLobbyJoined, Action<MultiplayerRoomData[]> tOnLobbyStatusChanged, MultiplayerRequest tRequest ) ;

		/// <summary>
		/// ロビーから退室する
		/// </summary>
		/// <param name="tOnLobyLeft"></param>
		/// <param name="tRequest"></param>
		/// <returns></returns>
		IEnumerator LeaveLobby( Action<LeaveLobbyResult> tOnLobbyLeft, MultiplayerRequest tRequest ) ;

		/// <summary>
		/// ルーム名の一覧を取得する
		/// </summary>
		/// <param name="tOnLobbyStatusChanged"></param>
		/// <returns></returns>
		string[] GetRoomName( Action<MultiplayerRoomData[]> tOnLobbyStatusChanged ) ;

		//-----------------------------------

		/// <summary>
		/// ルームに入室中かどうか
		/// </summary>
		/// <returns></returns>
		bool InRoom() ;

		/// <summary>
		/// ルームを作成する
		/// </summary>
		/// <param name="tRoomName"></param>
		/// <param name="tMaxPlayer"></param>
		/// <param name="tVisible"></param>
		/// <param name="tOpen"></param>
		/// <param name="tPassword"></param>
		/// <param name="tComment"></param>
		/// <param name="tOnRoomCreated"></param>
		/// <param name="tOnRoomStatusChanged"></param>
		/// <param name="tOnHostChanged"></param>
		/// <returns></returns>
		IEnumerator CreateRoom( string tRoomName, int tMaxPlayer, bool tVisible, bool tOpen, string tPassword, string tComment, string tLobbyName, Action<CreateRoomResult> tOnRoomCreated, Action<int,bool> tOnRoomStatusChanged, Action<int> tOnRoomHostChanged, MultiplayerRequest tRequest ) ;

		/// <summary>
		/// ルームに入室する
		/// </summary>
		/// <param name="tRoomName"></param>
		/// <param name="tMaxPlayer"></param>
		/// <param name="tPassword"></param>
		/// <param name="tLobbyName"></param>
		/// <param name="tOnRoomJoined"></param>
		/// <param name="tOnRoomStatusChanged"></param>
		/// <param name="tOnRoomHostChanged"></param>
		/// <param name="tRequest"></param>
		/// <returns></returns>
		IEnumerator JoinRoom( string tRoomName, string tPassword, Action<JoinRoomResult,int[]> tOnRoomJoined, Action<int,bool> tOnRoomStatusChanged, Action<int> tOnRoomHostChanged, MultiplayerRequest tRequest ) ;

		/// <summary>
		/// ルームに入室または作成する
		/// </summary>
		/// <param name="tRoomName"></param>
		/// <param name="tMaxPlayer"></param>
		/// <param name="tVisible"></param>
		/// <param name="tOpen"></param>
		/// <param name="tPassword"></param>
		/// <param name="tComment"></param>
		/// <param name="tOnRoomCreated"></param>
		/// <param name="tOnRoomStatusChanged"></param>
		/// <param name="tOnHostChanged"></param>
		/// <returns></returns>
		IEnumerator JoinOrCreateRoom( string tRoomName, int tMaxPlayer, bool tVisible, bool tOpen, string tPassword, string tComment, string tLobbyName, Action<JoinOrCreateRoomResult,int[]> tOnRoomJoinedOrCreated, Action<int,bool> tOnRoomStatusChanged, Action<int> tOnRoomHostChanged, MultiplayerRequest tRequest ) ;

		/// <summary>
		/// ランダムにルームに入室する(将来的に条件指定に対応する)
		/// </summary>
		/// <param name="tRoomName"></param>
		/// <param name="tMaxPlayer"></param>
		/// <param name="tPassword"></param>
		/// <param name="tLobbyName"></param>
		/// <param name="tOnRoomJoined"></param>
		/// <param name="tOnRoomStatusChanged"></param>
		/// <param name="tOnRoomHostChanged"></param>
		/// <param name="tRequest"></param>
		/// <returns></returns>
		IEnumerator JoinRandomRoom( Action<JoinRoomResult,int[]> tOnRoomJoined, Action<int,bool> tOnRoomStatusChanged, Action<int> tOnRoomHostChanged, MultiplayerRequest tRequest ) ;

		/// <summary>
		/// ルームを退室する
		/// </summary>
		/// <param name="tOnRoomLeft"></param>
		/// <param name="tRequest"></param>
		/// <returns></returns>
		IEnumerator LeaveRoom( Action<LeaveRoomResult> tOnRoomLeft, MultiplayerRequest tRequest ) ;

		/// <summary>
		/// 自身がホストかどうか
		/// </summary>
		/// <returns></returns>
		bool IsHost() ;

		/// <summary>
		/// 自身のプレイヤーＩＤ
		/// </summary>
		/// <returns></returns>
		int PlayerId() ;

		/// <summary>
		/// 自身のプレイヤー名前を取得する
		/// </summary>
		/// <returns></returns>
		string GetPlayerName() ;

		/// <summary>
		/// 自身のプレイヤー名を設定する
		/// </summary>
		/// <param name="tPlayerName"></param>
		void SetPlayerName( string tPlayerName ) ;

		/// <summary>
		/// 全員のプレイヤーＩＤ
		/// </summary>
		/// <returns></returns>
		int[] PlayerIds() ;

		/// <summary>
		/// 全員のプレイヤー名前
		/// </summary>
		/// <returns></returns>
		string[] PlayerNames() ;

		/// <summary>
		/// 自身を除く全員のプレイヤーＩＤ
		/// </summary>
		/// <returns></returns>
		int[] OtherPlayerIds() ;

		/// <summary>
		/// 自身を除く全員のプレイヤー名前
		/// </summary>
		/// <returns></returns>
		string[] OtherPlayerNames() ;

		/// <summary>
		/// ホストのプレイヤーＩＤ
		/// </summary>
		/// <returns></returns>
		int HostPlayerId() ;

		/// <summary>
		/// プレイヤーＩＤに該当するプレイヤー名前を取得する
		/// </summary>
		/// <param name="tPlayerId"></param>
		/// <returns></returns>
		string GetPlayerNameById( int tPlayerId ) ;

		/// <summary>
		/// 現在入室中のロビー名
		/// </summary>
		/// <returns></returns>
		string LobbyName() ;

		/// <summary>
		/// 現在入室中のルーム名
		/// </summary>
		string RoomName() ;

		/// <summary>
		/// 現在接続中のサーバーのロビー情報を取得する
		/// </summary>
		/// <returns></returns>
		MultiplayerLobbyData[] GetLobbyData() ;

		/// <summary>
		/// 現在入室中のロビーのルーム情報を取得する
		/// </summary>
		/// <returns></returns>
		MultiplayerRoomData[] GetRoomData() ;

		//-----------------------------------------------------------

		/// <summary>
		/// 指定したプレイヤーに文字列を送信する
		/// </summary>
		/// <param name="tString"></param>
		bool Send( int tPlayerId, bool tSecure, params System.Object[] tObjects ) ;

		/// <summary>
		/// プレイヤー全員に文字列を送信する
		/// </summary>
		/// <param name="tString"></param>
		bool SendToAll( bool tViaServer, bool tSecure, params System.Object[] tObjects ) ;

		/// <summary>
		/// ホスト限定で文字列を送信する
		/// </summary>
		/// <param name="tString"></param>
		bool SendToHost( bool tViaServer, bool tSecure, params System.Object[] tObjects ) ;

		/// <summary>
		/// 受信コールバックを設定する
		/// </summary>
		/// <param name="tOnReceived"></param>
		bool SetOnReceived( Action<bool,int,System.Object[]> tOnReceived ) ;


		/// <summary>
		/// 何等かのデータを受信するまで待つ
		/// </summary>
		/// <param name="tOnReceived"></param>
		/// <param name="tRequest"></param>
		/// <returns></returns>
		IEnumerator Receive( Action<bool,int,System.Object[]> tOnReceived, MultiplayerRequest tRequest ) ;


		//-----------------------------------------------------------

		/// <summary>
		/// ゲームオブジェクトに対しボイスチャット用のアダプターを接続する(OnPreDecodeは PlayerId Data Channels SamplingRate)
		/// </summary>
		/// <param name="tParent"></param>
		/// <param name="t3D"></param>
		/// <returns></returns>
		IEnumerator AttachVoiceAdapter( int tPlayerId, Transform tParent, bool t3D, int tSamplingRate, int PacketTime, Action<int,float[],int,int> tOnPreDecode, MultiplayerRequest tRequest ) ;
		
		/// <summary>
		/// ボイスチャット用のアダプタを除去する(他人のアダプター)
		/// </summary>
		/// <param name="tPlayerId"></param>
		/// <returns></returns>
		bool DetachVoiceAdapter( int tPlayerId ) ;

		/// <summary>
		/// ボイスのサンプリングレートを取得する(0で失敗:アダプタが生成済みである必要がある)
		/// </summary>
		/// <param name="tPlayerId"></param>
		/// <returns></returns>
		int GetVoiceSamplingRate( int tPlayerId ) ;

		/// <summary>
		/// ボイスの録音を開始する
		/// </summary>
		/// <returns></returns>
		bool StartVoiceRecording() ;

		/// <summary>
		/// ボイスの録音を停止する
		/// </summary>
		/// <returns></returns>
		bool StopVoiceRecording() ;

		/// <summary>
		/// コールバック群のクリアを行う
		/// </summary>
		void Terminate() ;
	}
}

