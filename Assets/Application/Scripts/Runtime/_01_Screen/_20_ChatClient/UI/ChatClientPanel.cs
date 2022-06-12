using System ;
using System.Collections ;
using System.Collections.Generic ;

using Cysharp.Threading.Tasks ;

using uGUIHelper ;

using UnityEngine ;

namespace Template.Screens.ChatClientClasses.UI
{
	/// <summary>
	/// チャットクライアント用のＵＩ制御
	/// </summary>
	public class ChatClientPanel : ExMonoBehaviour
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
		protected UIListView		m_LogView ;

		[Header( "接続関連" )]

		[SerializeField]
		protected UIView			m_ConnectControll ;

		[SerializeField]
		protected UIInputField		m_ServerAddressInput ;

		[SerializeField]
		protected UIInputField		m_PortNumberInput ;


		[SerializeField]
		protected UIButton			m_ConnectButton ;


		[Header( "入力関連" )]

		[SerializeField]
		protected UIView			m_SendControll ;

		[SerializeField]
		protected UIInputField		m_SpeakerInput ;

		[SerializeField]
		protected UIInputField		m_MessageInput ;

		[SerializeField]
		protected UIButton			m_SendButton ;

		[SerializeField]
		protected UIButton			m_DisconnectButton ;

		//-----------------------------------------------------------

		public class LogStructure
		{
			public string	Label ;
			public string	Message ;
			public Color	Color ;
		}

		private List<LogStructure>	m_Logs = new List<LogStructure>() ;


		//-----------------------------------------------------------

		private Action<string,int>		m_OnConnect ;

		private Action<string,string>	m_OnSend ;
		private Action					m_OnDisconnect ;

		//-----------------------------------------------------------

		/// <summary>
		/// ＵＩの準備を行う
		/// </summary>
		/// <param name="onSend"></param>
		/// <param name="onDisconnect"></param>
		/// <returns></returns>
		public async UniTask Prepare( string serverAddress, int portNumber, Action<string,int> onConnect, Action<string,string> onSend, Action onDisconnect )
		{
			// コールバックを保存する
			m_OnConnect		= onConnect ;

			m_OnSend		= onSend ;
			m_OnDisconnect	= onDisconnect ;

			//----------------------------------------------------------

			// ログの更新コールバックを設定する
			m_LogView.SetOnItemUpdated<ChatClientPanel_ListViewItem>( ( string identity, UIListView listView, int index, Component component ) =>
			{
				if( component != null )
				{
					var viewItem = component as ChatClientPanel_ListViewItem ;

					var log = m_Logs[ index ] ;

					viewItem.SetStyle( log.Label, log.Message, log.Color ) ;
				}

				return 0 ;
			} ) ;

			m_LogView.ItemCount = m_Logs.Count ;

			//------------------------------------------------------------------------------------------

			// 接続関連操作を表示する
			ShowConnectControll( serverAddress, portNumber ) ;

			//----------------------------------------------------------

			await Yield() ;
		}

		//-------------------------------------------------------------------------------------------
		// ログ関連

		public void AddLog( string label, string message, Color color )
		{
			m_Logs.Add( new LogStructure(){ Label = label, Message = message, Color = color } ) ;
			m_LogView.SetContentPosition( Mathf.Infinity, m_Logs.Count ) ;	// 最後
		}

		//-------------------------------------------------------------------------------------------
		// 接続関連

		/// <summary>
		/// 接続関連操作を表示する
		/// </summary>
		public void ShowConnectControll( string serverAddress, int portNumber )
		{
			m_ConnectControll.SetActive( true ) ;

			if( string.IsNullOrEmpty( serverAddress ) == false )
			{
				m_ServerAddressInput.Text = serverAddress ;
			}

			// サーバーアドレス入力のコールバックを設定する
			m_ServerAddressInput.SetOnEndEdit( ( string identity, UIInputField inputField, string text ) =>
			{
				UpdateConnectButton() ;
			} ) ;
			
			if( portNumber != 0  )
			{
				m_PortNumberInput.Text = portNumber.ToString() ;
			}

			// ポート番号入力のコールバックを設定する
			m_PortNumberInput.SetOnEndEdit( ( string identity, UIInputField inputField, string text ) =>
			{
				UpdateConnectButton() ;
			} ) ;

			// 接続ボタンのコールバックを設定する
			m_ConnectButton.SetOnSimpleClick( () =>
			{
				Connect() ;
			} ) ;

			UpdateConnectButton() ;

			//--------------

			m_SendControll.SetActive( false ) ;
		}

		//-----------------------------------

		// 送信ボタンの状態を更新する
		private void UpdateConnectButton()
		{
			string server = m_ServerAddressInput.Text ;

			string portNumberName = m_PortNumberInput.Text ;
			if( int.TryParse( portNumberName, out int portNumber ) == false )
			{
				portNumber = 0 ;
			}

			m_ConnectButton.Interactable =	( string.IsNullOrEmpty( server ) == false && portNumber != 0 ) ;
		}

		// 接続する
		private void Connect()
		{
			string serverAddress = m_ServerAddressInput.Text ;

			string portNumberName = m_PortNumberInput.Text ;
			if( int.TryParse( portNumberName, out int portNumber ) == false )
			{
				portNumber = 0 ;
			}

			m_OnConnect?.Invoke( serverAddress, portNumber ) ;
		}

		//-------------------------------------------------------------------------------------------
		// 送信関連

		/// <summary>
		/// 送信関連操作を表示する
		/// </summary>
		public void ShowSendControll()
		{
			m_SendControll.SetActive( true ) ;

			m_SpeakerInput.Text = "名無し" ;

			// 名前入力のコールバックを設定する
			m_SpeakerInput.SetOnEndEdit( ( string identity, UIInputField inputField, string text ) =>
			{
				UpdateSendButton() ;
			} ) ;

			m_MessageInput.Text = "こんにちは" ;

			// 文章入力のコールバックを設定する
			m_MessageInput.SetOnEndEdit( ( string identity, UIInputField inputField, string text ) =>
			{
				UpdateSendButton() ;
			} ) ;

			// 送信ボタンのコールバックを設定する
			m_SendButton.SetOnSimpleClick( () =>
			{
				Send() ;
			} ) ;

			UpdateSendButton() ;

			// 切断ボタンのコールバックを設定する
			m_DisconnectButton.SetOnSimpleClick( () =>
			{
				Disconnect() ;
			} ) ;

			m_DisconnectButton.Interactable = true ;

			//--------------

			m_ConnectControll.SetActive( false ) ;
		}

		//-----------------------------------

		// 送信ボタンの状態を更新する
		private void UpdateSendButton()
		{
			string speaker = m_SpeakerInput.Text ;
			string message = m_MessageInput.Text ;

			m_SendButton.Interactable =	( string.IsNullOrEmpty( speaker ) == false && string.IsNullOrEmpty( message ) == false ) ;
		}

		// 送信する
		private void Send()
		{
			string speaker = m_SpeakerInput.Text ;
			string message = m_MessageInput.Text ;

			m_OnSend?.Invoke( speaker, message ) ;
		}

		// 切断する
		private void Disconnect()
		{
			m_OnDisconnect?.Invoke() ;
		}

		/// <summary>
		/// 入力中の文章を消去する
		/// </summary>
		public void ClearMessage()
		{
			m_MessageInput.Text = string.Empty ;

			UpdateSendButton() ;
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

