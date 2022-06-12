using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Threading ;

using UnityEngine ;

using WebSocketSharp ;
using WebSocketSharp.Net ;


using Cysharp.Threading.Tasks ;

using uGUIHelper ;

using Template.Screens.ChatClientClasses.UI ;


namespace Template.Screens
{
	/// <summary>
	/// チャットの制御処理
	/// </summary>
	public partial class ChatClient
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
		/// アクション選択
		/// </summary>
		/// <returns></returns>
		private async UniTask<State> State_ActionSelecting( State previous )
		{
			// メインスレッドのコンテキストを取得する
			var context = SynchronizationContext.Current ;

			//----------------------------------------------------------

			string initialServerAddress = string.Empty ;
			int    initialPortNumber = 0 ;
			var settings = ApplicationManager.LoadSettings() ;
			if( settings != null )
			{
				initialServerAddress	= settings.ServerAddress ;
				initialPortNumber		= settings.PortNumber ;
			}

			await m_ChatClientPanel.Prepare( initialServerAddress, initialPortNumber, OnConnect, OnSend, OnDisconnect ) ;

			//----------------------------------------------------------


			//----------------------------------------------------------
			// UI のコールバック

			void OnConnect( string serverAddress, int portNumber )
			{
				// 接続要求が出された

				//---------------------------------
				// WebSocket の準備

				m_WebSocket = new ExWebSocket( context, OnOpen, null, OnText, OnClose, OnError ) ;

				// 非同期で接続を試みる
				m_WebSocket.Connect( serverAddress, portNumber, false ) ;

				Progress.On( Progress.Styles.WebAPI, false, "接続中" ) ;

				//---------------------------------
			}

			// 送信
			void OnSend( string speaker, string message )
			{
				NetworkData.ChatData chat = new NetworkData.ChatData()
				{
					Speaker = speaker,
					Message	= message,
				} ;

				string text = JsonUtility.ToJson( chat ) ;
				if( string.IsNullOrEmpty( text ) == false )
				{
					m_WebSocket.Send( text ) ;
					m_ChatClientPanel.ClearMessage() ;
				}
			}

			// 切断
			void OnDisconnect()
			{
				// 切断要求が出された
				m_ChatClientPanel.ShowConnectControll( null, 0 ) ;

				m_ChatClientPanel.AddLog( "システム", "サーバーと切断しました", Color.blue ) ;

				//---------------------------------

				if( m_WebSocket != null )
				{
					m_WebSocket.Close() ;
					m_WebSocket = null ;
				}
			}

			//----------------------------------------------------------
			// WebSocket のコールバック

			// 接続
			void OnOpen()
			{
				Progress.Off() ;

				m_ChatClientPanel.ShowSendControll() ;

				m_ChatClientPanel.AddLog( "システム", "サーバーに接続しました", Color.blue ) ;
			}

			// 受信
			void OnText( string text )
			{
				var chat = JsonUtility.FromJson<NetworkData.ChatData>( text ) ;
				if( chat != null )
				{
					m_ChatClientPanel.AddLog( chat.Speaker, chat.Message, Color.black ) ;
				}
			}

			// 切断
			void OnClose( int code, string reason )
			{
				if( m_WebSocket != null )
				{
					m_ChatClientPanel.ShowConnectControll( null, 0 ) ;

					if( code == 1006 )
					{
						Progress.Off() ;

						Dialog.Open( "エラー", "サーバーに接続できませんでした", new string[]{ "閉じる" } ).Forget() ;
					}
					else
					{
						m_ChatClientPanel.AddLog( "システム", "サーバーから切断されました(" + code + ")" + reason, Color.blue ) ;
					}

					//---------------------------------

					m_WebSocket.Close() ;
					m_WebSocket = null ;
				}
			}

			// 異常
			void OnError( string message )
			{
				m_ChatClientPanel.AddLog( "エラー", message, Color.red ) ;
			}

			//----------------------------------------------------------

			// 各種ＵＩをフェードインする
			await WhenAll
			(
				m_ChatClientPanel.FadeIn()
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
				m_ChatClientPanel.FadeOut()
			) ;

			//----------------------------------------------------------

			State next = State.Unknown ;

			// 選択されたアクションによって遷移先が変わる
			return next ;
		}

		//-------------------------------------------------------------------------------------------

	}
}
