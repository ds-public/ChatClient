using System ;
using System.Collections.Generic ;
using System.Threading ;
using System.Threading.Tasks ;

using WebSocketSharp ;
using WebSocketSharp.Net ;
using WebSocketSharp.Server ;

using Cysharp.Threading.Tasks ;

using UnityEngine ;

namespace Template
{
	/// <summary>
	/// WebSocket まのサーバー側のクライアント管理用クラスのラッパー Version 2022/05/07
	/// </summary>
	public class ExWebSocket
	{
		// メインスレッドのコンテキスト
		private SynchronizationContext	m_Context ;

		//-----------------------------------

		// 新しい接続があった際にサーバーに伝えるコールバック
		private Action					m_OnOpen ;

		// クライアントからの情報を受信した際にサーバーに伝えるコールバック
		private Action<byte[]>			m_OnData ;

		// クライアントからの情報を受信した際にサーバーに伝えるコールバック
		private Action<string>			m_OnText ;

		// クライアントからの切断があった際にサーバーに伝えるコールバック
		private Action<int,string>		m_OnClose ;

		// 異常があった際にサーバーに伝えるコールバック
		private Action<string>			m_OnError ;


		//-----------------------------------------------------------

		private WebSocket				m_WebSocket ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="context"></param>
		/// <param name="onOpen"></param>
		/// <param name="onData"></param>
		/// <param name="onText"></param>
		/// <param name="onClose"></param>
		public ExWebSocket( SynchronizationContext context, Action onOpen, Action<byte[]> onData, Action<string> onText, Action<int,string> onClose, Action<string> onError )
		{
			m_Context	= context ;

			m_OnOpen	= onOpen ;
			m_OnData	= onData ;
			m_OnText	= onText ;
			m_OnClose	= onClose ;
			m_OnError	= onError ;
		}

		/// <summary>
		/// 接続
		/// </summary>
		/// <param name="serverAddress"></param>
		/// <param name="serverPortNumber"></param>
		public void Connect( string serverAddress, int serverPortNumber, bool isBlocking = true )
		{
			string url = "ws://" + serverAddress + ":" + serverPortNumber.ToString() + "/" ;

			Debug.Log( "<color=#00FF00>接続先 " + url + "</color>" ) ;

			m_WebSocket = new WebSocket( url ) ;

			// コールバックを設定する
			m_WebSocket.OnOpen		+= OnOpen ;
			m_WebSocket.OnMessage	+= OnMessage ;
			m_WebSocket.OnClose		+= OnClose ;
			m_WebSocket.OnError		+= OnError ;

			if( isBlocking == true )
			{
				// 同期接続
				m_WebSocket.Connect() ;
			}
			else
			{
				// 非同期接続
				m_WebSocket.ConnectAsync() ;
			}
		}

		//-----------------------------------------------------------

		// 接続された際に呼び出される
		private void OnOpen( object sender, EventArgs e )
		{
			// 接続
			if( m_Context != null )
			{
				m_Context.Post( _ =>
				{
					m_OnOpen?.Invoke() ;
				},
				null ) ;
			}
		}

		// 受信した際に呼び出される
		private void OnMessage( object sender, MessageEventArgs e )
		{
			// 受信
			if( m_Context != null )
			{
				m_Context.Post( _ =>
				{
					if( e.IsBinary == true && e.RawData != null )
					{
						m_OnData?.Invoke( e.RawData ) ;
					}
					if( e.IsText == true && e.Data != null )
					{
						m_OnText?.Invoke( e.Data ) ;
					}
				},
				null ) ;
			}
		}

		// 切断された際に呼び出される
		private void OnClose( object sender, CloseEventArgs e )
		{
			// 切断
			if( m_Context != null )
			{
				m_Context.Post( _ =>
				{
					m_OnClose?.Invoke( e.Code, e.Reason ) ;
				},
				null ) ;
			}

			//----------------------------------

			if( m_WebSocket != null )
			{
				m_WebSocket.Close() ;
				m_WebSocket = null ;
			}
		}

		// 異常が発生した際に呼び出される
		private void OnError(  object sender, ErrorEventArgs e )
		{
			// 異常
			if( m_Context != null )
			{
				m_Context.Post( _ =>
				{
					m_OnError?.Invoke( e.Message ) ;
				},
				null ) ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 送信する
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public bool Send( byte[] data )
		{
			if( m_WebSocket == null )
			{
				return false ;
			}

			m_WebSocket.Send( data ) ;

			return true ;
		}

		/// <summary>
		/// 送信する
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public bool Send( string text )
		{
			if( m_WebSocket == null )
			{
				return false ;
			}

			m_WebSocket.Send( text ) ;

			return true ;
		}

		/// <summary>
		/// 切断する
		/// </summary>
		public void Disconnect()
		{
			if( m_WebSocket != null )
			{
				m_WebSocket.Close() ;
				m_WebSocket = null ;
			}
		}

		/// <summary>
		/// 切断する
		/// </summary>
		public void Close()
		{
			if( m_WebSocket != null )
			{
				m_WebSocket.Close() ;
				m_WebSocket = null ;
			}
		}
	}
}
