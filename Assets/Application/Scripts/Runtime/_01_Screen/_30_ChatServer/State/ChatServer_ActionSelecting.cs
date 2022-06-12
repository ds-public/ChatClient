using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Threading ;

using WebSocketSharp ;
using WebSocketSharp.Net ;
using WebSocketSharp.Server ;


using UnityEngine ;

using Cysharp.Threading.Tasks ;




using uGUIHelper ;

using Template.Screens.ChatServerClasses.UI ;


namespace Template.Screens
{
	/// <summary>
	/// チャットの制御処理
	/// </summary>
	public partial class ChatServer
	{
		/// <summary>
		/// 行動選択の種類
		/// </summary>
		public enum ActionTypes
		{
			Unknown,
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// クライアントの管理クラス(定義のみ)
		/// </summary>
		public class Client : ExWebSocketBehavior<Client>
		{
			protected override void OnConnected()
			{
				Debug.Log( "接続ありです:" + this.ID ) ;
			}

			protected override void OnReceived( string text )
			{
				Debug.Log( "受信ありです:" + text ) ;
			}

			protected override void OnDisconnected()
			{
				Debug.Log( "切断ありです:" + this.ID ) ;
			}
		}

		// クライアントの制御用インスタンス群を保持する
		private  List<Client> m_Clients = new List<Client>() ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// アクション選択
		/// </summary>
		/// <returns></returns>
		private async UniTask<State> State_ActionSelecting( State previous )
		{
			await m_ChatServerPanel.Prepare() ;

			//----------------------------------------------------------
			// WebSocketServer 準備

			// メインスレッドのコンテキストを取得する
			var context = SynchronizationContext.Current ;

			m_WebSocketServer.AddWebSocketService<Client>( "/", ( Client client ) =>
			{
				if( client != null )
				{
					Debug.Log( "[重要]クライアント接続通知はきているのか:" + client.ID ) ;


					// 生成されたクライアント制御クラスのインスタンスにサーバーのインスタンスを渡す
					client.SetServer( context, OnOpen, OnData, OnText, OnClose ) ;

					// クライアントの管理リストにインスタンスを保存する
					m_Clients.Add( client ) ;
				}
			} ) ;

			// サーバー開始
			m_WebSocketServer.Start() ;

			m_ChatServerPanel.AddLog( "システム", "起動", Color.blue ) ;

			//----------------------------------------------------------
			// サーバーでのイベントコールバック

			void OnOpen( Client client )
			{
				// 接続
				m_ChatServerPanel.AddConnection( client.ID, "名前不明" ) ;
				m_ChatServerPanel.AddLog( client.ID, "接続されました", Color.blue ) ;
			}

			void OnData( Client client, byte[] data )
			{
			}

			void OnText( Client client, string text )
			{
				// 発言

				var chat = JsonUtility.FromJson<NetworkData.ChatData>( text ) ;
				if( chat != null )
				{
					// 名前更新
					m_ChatServerPanel.SetConnection( client.ID, chat.Speaker ) ;

					// ログ追加
					m_ChatServerPanel.AddLog( chat.Speaker, chat.Message, Color.black ) ;
				}
				
				// 全てのクライアントに送信する
				client.Broadcast( text ) ;
			}

			void OnClose( Client client )
			{
				// 切断
				m_ChatServerPanel.RemoveConnection( client.ID ) ;
				m_ChatServerPanel.AddLog( client.ID, "切断されました", Color.blue ) ;
			}

			//----------------------------------------------------------

			// 各種ＵＩをフェードインする
			await WhenAll
			(
				m_ChatServerPanel.FadeIn()
			) ;

			//----------------------------------------------------------

			//----------------------------------
			// アクションのコールバック設定

			ActionTypes actionType = ActionTypes.Unknown ;


			//----------------------------------------------------------

			// ブロックマスクを無効化する
			Blocker.Off() ;

			//----------------------------------------------------------
			// actionType が Unknown 以外の状態になってもループは維持する必要があるケースが存在するので While は true でずっと回しておくこと
			while( actionType == ActionTypes.Unknown )
			{
				// 無いとノーウェイト状態になりフリーズするので注意
				await Yield() ;
			}

			//------------------------------------------------------------------------------------------

			Blocker.On() ;

			//----------------------------------

			//----------------------------------------------------------

			// 各種ＵＩをフェードアウトする
			await WhenAll
			(
				m_ChatServerPanel.FadeOut()
			) ;

			//----------------------------------------------------------

			State next = State.Unknown ;

			// 選択されたアクションによって遷移先が変わる
			return next ;
		}

		//-------------------------------------------------------------------------------------------

	}
}
