using System ;
using System.Collections ;
using System.Collections.Generic ;

using Cysharp.Threading.Tasks ;

using UnityEngine ;

using uGUIHelper ;

namespace Template.Dialogs
{
	/// <summary>
	/// テンプレートダイアログのコントロールクラス Version 2022/02/15
	/// </summary>
	public class Template : DialogBase
	{
		//-----------------------------------------------------------
		// 量産ダイアログ固有の情報

		[SerializeField]
		protected UITextMesh	m_Title ;			// 固有ダイアログのタイトル部

		[SerializeField]
		protected UIRichText	m_Message ;			// 固有ダイアログのメッセージ部

		[SerializeField]
		protected UIButton		m_CloseButton ;		// 固有ダイアログの閉じるボタン

		//-----------------------------------------------------------

		private Action<int>		m_OnClosed ;		// 固有ダイアログが閉じられた際に呼び出すコールバックメソッド

		private int				m_Result = -1 ;			// 結果

		private bool			m_IsClosed ;		// 閉じられたか

		//-------------------------------------------------------------------------------------------

		// ダイアログシーンの名前を設定する
		protected override string SetDialogSceneName()
		{
			IsResident = true ;	// シーンを常駐させるかどうか
			return Scene.Dialog.Template ;
		}

#if UNITY_EDITOR
		// 単体デバッグを実行する(このダイアログのシーンを開いたままUnityEitorでPlayを実行する)
		protected override async UniTask RunDebug()
		{
			await Open( "タイトル", "メッセージ", null ) ;	// ダミーの引数で固有ダイアログを開く
		}
#endif
		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ダイアログを開く
		/// </summary>
		/// <returns>The open.</returns>
		/// <param name="title">Title.</param>
		/// <param name="message">Message.</param>
		/// <param name="selectionButtonLabels">Selection button labels.</param>
		/// <param name="onClosed">On closed.</param>
		public async UniTask<int> Open( string title, string message, Action<int> onClosed = null )
		{
			m_Title.Text	= title ;		// タイトル文字列を設定する
			m_Message.Text	= message ;	// メッセージ文字列を設定する

			//----------------------------------------------------------

			m_CloseButton.SetOnButtonClick( ( string identity, UIButton button ) =>
			{
				// 閉じるボタンが押された際に閉じる処理を実行する
				Close( 0 ) ;
			} ) ;

			//----------------------------------------------------------

			m_OnClosed		= onClosed ;	// コールバックメソッドを保存する

			//----------------------------------------------------------

			// ダイアログを開く
			await base.OpenBase() ;

			// 閉じられるのを待つ
			await WaitUntil( () => m_IsClosed ) ;

			// 結果を返す
			return m_Result ;
		}

		//-------------------------------------------------------------------------------------------
		
		/// <summary>
		/// ダイアログを閉じる
		/// </summary>
		/// <param name="result">Result.</param>
		public void Close( int result )
		{
			_ = base.CloseBase
			(
				() =>
				{
					// ダイアログが完全に閉じられた際に呼ばれる
					m_OnClosed?.Invoke( result ) ;
					m_Result = result ;
					m_IsClosed = true ;
				}
			) ;
		}
	}
}

