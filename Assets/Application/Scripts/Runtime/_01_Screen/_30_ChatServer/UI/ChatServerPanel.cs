using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using Cysharp.Threading.Tasks ;

using uGUIHelper ;

using UnityEngine ;

namespace Template.Screens.ChatServerClasses.UI
{
	/// <summary>
	/// チャットサーバー用のＵＩ制御
	/// </summary>
	public class ChatServerPanel : ExMonoBehaviour
	{
		private UIView				m_View ;
		public  UIView				  View
		{
			get
			{
				if( m_View == null )
				{
					m_View  = GetComponent<UIView>() ;
				}
				return m_View ;
			}
		}


		[SerializeField]
		protected UIListView		m_ConnectionView ;

		[SerializeField]
		protected UIListView		m_LogView ;

		//-----------------------------------------------------------

		/// <summary>
		/// コネクションのデータ構造
		/// </summary>
		public class ConnectionStructure
		{
			public string	ServerAddress ;
			public string	Speaker ;
		}

		private List<ConnectionStructure>	m_Connections = new List<ConnectionStructure>() ;

		/// <summary>
		/// ログのデータ構造
		/// </summary>
		public class LogStructure
		{
			public string	Label ;
			public string	Message ;
			public Color	Color ;
		}

		private List<LogStructure>	m_Logs = new List<LogStructure>() ;

		//-----------------------------------------------------------

		/// <summary>
		/// ＵＩの準備を行う
		/// </summary>
		/// <param name="onSend"></param>
		/// <param name="onDisconnect"></param>
		/// <returns></returns>
		public async UniTask Prepare()
		{
			//----------------------------------------------------------

			// コネクションの更新コールバックを設定する
			m_ConnectionView.SetOnItemUpdated<ChatServerPanel_ListViewItem>( ( string identity, UIListView listView, int index, Component component ) =>
			{
				if( component != null )
				{
					var viewItem = component as ChatServerPanel_ListViewItem ;

					var connection = m_Connections[ index ] ;

					viewItem.SetStyle( connection.ServerAddress, connection.Speaker, Color.magenta ) ;
				}

				return 0 ;
			} ) ;

			m_ConnectionView.ItemCount = m_Connections.Count ;

			// ログの更新コールバックを設定する
			m_LogView.SetOnItemUpdated<ChatServerPanel_ListViewItem>( ( string identity, UIListView listView, int index, Component component ) =>
			{
				if( component != null )
				{
					var viewItem = component as ChatServerPanel_ListViewItem ;

					var log = m_Logs[ index ] ;

					viewItem.SetStyle( log.Label, log.Message, log.Color ) ;
				}

				return 0 ;
			} ) ;

			m_LogView.ItemCount = m_Logs.Count ;

			//------------------------------------------------------------------------------------------

			await Yield() ;
		}

		//-------------------------------------------------------------------------------------------
		// コネクション関連

		/// <summary>
		/// 追加する
		/// </summary>
		/// <param name="serverAddress"></param>
		/// <param name="speaker"></param>
		public void AddConnection( string serverAddress, string speaker )
		{
			m_Connections.Add( new ConnectionStructure(){ ServerAddress = serverAddress, Speaker = speaker } ) ;

			m_ConnectionView.SetContentPosition( 0, m_Connections.Count ) ;
		}

		/// <summary>
		/// 削除する
		/// </summary>
		/// <param name="serverAddress"></param>
		public void RemoveConnection( string serverAddress )
		{
			var record = m_Connections.FirstOrDefault( _ => _.ServerAddress == serverAddress ) ;
			if( record == null )
			{
				return ;
			}

			m_Connections.Remove( record ) ;

			m_ConnectionView.SetContentPosition( 0, m_Connections.Count ) ;
		}

		/// <summary>
		/// 名前を設定する
		/// </summary>
		/// <param name="serverAddress"></param>
		/// <param name="speaker"></param>
		public void SetConnection( string serverAddress, string speaker )
		{
			var record = m_Connections.FirstOrDefault( _ => _.ServerAddress == serverAddress ) ;
			if( record == null )
			{
				return ;
			}

			record.Speaker = speaker ;

			m_ConnectionView.Refresh() ;
		}

		//-------------------------------------------------------------------------------------------
		// ログ関連

		/// <summary>
		/// 追加する
		/// </summary>
		/// <param name="label"></param>
		/// <param name="message"></param>
		public void AddLog( string label, string message, Color color )
		{
			m_Logs.Add( new LogStructure(){ Label = label, Message = message, Color = color } ) ;

			m_LogView.ItemCount = m_Logs.Count ;
			m_LogView.SetContentPosition( Mathf.Infinity ) ;	// 最後
			m_LogView.Refresh() ;
		}

		//-------------------------------------------------------------------------------------------
		// フェード関連

		/// <summary>
		/// フェードイン
		/// </summary>
		/// <returns></returns>
		public async UniTask FadeIn()
		{
			if( View.ActiveSelf == true )
			{
				return ;
			}

			await When( View.PlayTween( "FadeIn" ) ) ;
		}

		/// <summary>
		/// フェードアウト
		/// </summary>
		/// <returns></returns>
		public async UniTask FadeOut()
		{
			if( View.ActiveSelf == false )
			{
				return ;
			}

			await When( View.PlayTweenAndHide( "FadeOut" ) ) ;
		}
	}
}

