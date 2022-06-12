using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using Cysharp.Threading.Tasks ;

using UnityEngine ;

using uGUIHelper ;
using MathHelper;

using Template.Screens.TitleClasses.UI ;

namespace Template.Screens
{
	/// <summary>
	/// タイトル画面の制御クラス Version 2022/05/04
	/// </summary>
	public partial class Title : ScreenBase
	{
		[Header( "全体" )]

		[SerializeField]
		protected UIImage			m_Screen ;

		//-----------------------------------

		[Header( "固有ダイアログ関係" )]

		// ダイアログ管理
		[SerializeField]
		protected DialogController            m_DialogController ;

		//-------------------------------------------------------------------------------------------

		[Header( "開始ステート" )]

		/// <summary>
		/// 開始ステート
		/// </summary>
		[SerializeField]
		protected State	m_StartupState		= State.ActionSelecting ;

		/// <summary>
		/// 現在ステート
		/// </summary>
		[SerializeField]
		protected State m_CurrentState		= State.Unknown ;

		//-------------------------------------------------------------------------------------------

		override protected void OnAwake()
		{
			// 初期状態で見せたくないＵＩを非表示にする

			m_DialogController.SetOwner( this );
		}

		override protected async UniTask OnStart()
		{
			//----------------------------------------------------------

			State startupState = m_StartupState ;

			// ナビゲータのフェードインが完了するまでは入力をブロックする(ヘッダーフッターはホーム内シーン切り替え中以外は無防備・シーン切り替えが終わった直後の一瞬でシーン切り替えを行わせる事が可能)
			Blocker.On() ;

			//----------------------------------------------------------

			// フェードイン(画面表示)を許可する
			Scene.Ready = true ;

			// フェード完了を待つ
			await Scene.WaitForFading() ;

			//------------------------------------------------------------------------------------------

			// メイン(ステートマシン)処理実行
			_ = MainLoop( startupState ) ;
		}


		/// <summary>
		/// メインループ(ステートマシン)処理 ※メソッド名に Main を使うと実行時にエラーがでる(原因不明)
		/// </summary>
		/// <returns></returns>
		private async UniTask MainLoop( State startupState )
		{
			State previous	= State.Unknown ;
			State next		= startupState ;

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
	}
}
