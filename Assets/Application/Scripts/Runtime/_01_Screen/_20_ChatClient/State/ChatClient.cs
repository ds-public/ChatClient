using System ;
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
	public partial class ChatClient : ScreenBase
	{
		//-----------------------------------------------------------

		[Header( "ＵＩ参照" )]

		// 画面
		[SerializeField]
		protected UIImage						m_Screen ;

		//-----------------------------------

		[Header( "画面固有ＵＩ" )]

		[SerializeField]
		protected ChatClientPanel				m_ChatClientPanel ;

		//-----------------------------------

		[Header( "固有ダイアログ関係" )]

		// ダイアログ管理
		[SerializeField]
		protected DialogController				m_DialogController ;

		//-------------------------------------------------------------------------------------------

		[Header( "開始ステート" )]

		/// <summary>
		/// 開始ステート
		/// </summary>
		[SerializeField]
		protected State	m_StartupState = State.ActionSelecting ;

		/// <summary>
		/// 現在ステート
		/// </summary>
		[SerializeField]
		protected State m_CurrentState = State.Unknown ;

		//-------------------------------------------------------------------------------------------

		// クライアント用の WebSocket
		private ExWebSocket	m_WebSocket ;

		//-------------------------------------------------------------------------------------------

		override protected void OnAwake()
		{
			// ＵＩを非表示にするなどの処理を行う

			m_ChatClientPanel.View.SetActive( false ) ;

			m_DialogController.SetOwner( this );
		}

		override protected async UniTask OnStart()
		{
			// ナビゲータのフェードインが完了するまでは入力をブロックする(ヘッダーフッターはホーム内シーン切り替え中以外は無防備・シーン切り替えが終わった直後の一瞬でシーン切り替えを行わせる事が可能)
			Blocker.On() ;

			//----------------------------------------------------------

			//----------------------------------------------------------

			// 開始ステートを設定する
			State startupState = m_StartupState ;

			//----------------------------------------------------------



			//----------------------------------------------------------

			// フェードイン(画面表示)を許可する
			Scene.Ready = true ;	// 注意:タイトル画面などから遷移した場合などは全画面フェードも許可しないと表示されない

			// フェードインが終了するのを待つ
			await Scene.WaitForFading() ;

			//----------------------------------------------------------
			// 注意：ロード処理や画面生成などはフェードインの前に行うこと
			// 　　　以後の処理は画面表示後の処理である


			//----------------------------------
			// ブロッキング処理を切り替える

			Blocker.On() ;

			//------------------------------------------------------------------------------------------

			// メイン(ステートマシン)処理実行
			MainLoop( startupState ).Forget() ;
		}


		/// <summary>
		/// メインループ(ステートマシン)処理 ※メソッド名に Main を使うと実行時にエラーがでる(原因不明)
		/// </summary>
		/// <returns></returns>
		private async UniTask MainLoop( State startupState )
		{
			State previous = State.Unknown ;
			State next = startupState ;

			//----------------------------------------------------------

			while( true )
			{
				m_CurrentState = next ;
				switch( m_CurrentState )	// 各略型はC#8.0以降が必要なのでUnity2020を待つべし
				{
					case State.ActionSelecting			: next = await State_ActionSelecting( previous )		; break ;   // アクション選択

					case State.Unknown					:
					default								: next = State.Unknown									; break ;	// 不明なケースでは厩舎終了(保険：通常は Unknown になる事はない)
				}
				previous = m_CurrentState ;

				if( next == State.Unknown )
				{
					break ;	// ここが無いと Unknown になった際に UnityEditor がフリーズするので注意
				}
			}
		}

		//-------------------------------------------------------------------------------------------


		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// MonoBehaviour が破棄される際に呼び出される
		/// </summary>
		internal void OnDestroy()
		{
			// WebSocket を停止させる
			if( m_WebSocket != null )
			{
				m_WebSocket.Close() ;
				m_WebSocket  = null ;
			}
		}
	}
}
