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
	public class ExWebSocketBehavior<T> : WebSocketBehavior where T : WebSocketBehavior
	{
		// メインスレッドのコンテキスト
		private SynchronizationContext	m_Context ;

		//-----------------------------------

		// 新しい接続があった際にサーバーに伝えるコールバック
		private Action<T>			m_OnOpen ;

		// クライアントからの情報を受信した際にサーバーに伝えるコールバック
		private Action<T,byte[]>	m_OnData ;

		// クライアントからの情報を受信した際にサーバーに伝えるコールバック
		private Action<T,string>	m_OnText ;

		// クライアントからの切断があった際にサーバーに伝えるコールバック
		private Action<T>			m_OnClose ;

		//------------------------------------------------------------------------------------------

		/// <summary>
		/// サーバーのインスタンスを設定する
		/// </summary>
		/// <param name="server"></param>
		public void SetServer( SynchronizationContext content, Action<T> onOpen, Action<T,byte[]> onData, Action<T,string> onText, Action<T> onClose )
		{
			m_Context		= content ;

			m_OnOpen		= onOpen ;
			m_OnData		= onData ;
			m_OnText		= onText ;
			m_OnClose		= onClose ;
		}

		//----------------------------------------------------------

		// 誰かがログインしてきたときに呼ばれるメソッド
		protected override void OnOpen()
		{
			Debug.Log( "OnOpen状況:" + m_OnOpen ) ;

			// サブスレッド実行なので使用は推奨されない
			OnConnected() ;

			if( m_Context != null )
			{
				m_Context.Post( _ =>
				{
					// メインスレッドでコールバックを実行する
					m_OnOpen?.Invoke( this as T ) ;
				}, null ) ;
			}
		}

		protected virtual void OnConnected(){}

		// 誰かがメッセージを送信してきたときに呼ばれるメソッド
		protected override void OnMessage( MessageEventArgs e )
		{
			if( e.IsBinary == true && e.RawData != null )
			{
				// サブスレッド実行なので使用は推奨されない
				OnReceived( e.RawData ) ;

				if( m_Context != null )
				{
					m_Context.Post( _ =>
					{
						// メインスレッドでコールバックを実行する
						m_OnData?.Invoke( this as T, e.RawData ) ;
					}, null ) ;
				}
			}
			else
			if( e.IsText == true && e.Data != null )
			{
				// サブスレッド実行なので使用は推奨されない
				OnReceived( e.Data ) ;

				if( m_Context != null )
				{
					m_Context.Post( _ =>
					{
						// メインスレッドでコールバックを実行する
						m_OnText?.Invoke( this as T, e.Data ) ;
					}, null ) ;
				}
			}
		}

		protected virtual void OnReceived( string text ){}
		protected virtual void OnReceived( byte[] data ){}

		// 誰かがログアウトしてきたときに呼ばれるメソッド
		protected override void OnClose( CloseEventArgs e )
		{
			// サブスレッド実行なので使用は推奨されない
			OnDisconnected() ;

			if( m_Context != null )
			{
				m_Context.Post( _ =>
				{
					// メインスレッドでコールバックを実行する
					m_OnClose?.Invoke( this as T ) ;
				}, null ) ;
			}
		}

		protected virtual void OnDisconnected(){}

		//-----------------------------------------------------------

		/// <summary>
		/// 全クライアントにまとめて送信する
		/// </summary>
		/// <param name="text"></param>
		public void Broadcast( byte[] data )
		{
			Sessions.Broadcast( data ) ;
		}

		/// <summary>
		/// 全クライアントにまとめて送信する
		/// </summary>
		/// <param name="text"></param>
		public void Broadcast( string text )
		{
			Sessions.Broadcast( text ) ;
		}
	}
}
