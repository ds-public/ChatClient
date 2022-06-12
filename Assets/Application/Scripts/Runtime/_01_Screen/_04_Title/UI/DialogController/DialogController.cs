using System ;
using System.Collections.Generic ;
using System.Threading ;
using UnityEngine ;

using Cysharp.Threading.Tasks ;
using MathHelper ;

using uGUIHelper ;

namespace Template.Screens.TitleClasses.UI
{
	/// <summary>
	/// ダイアログ操作のラッパー Version 2022/05/04
	/// </summary>
	[Serializable]
	public class DialogController : CancelableTask
	{
		private Title								m_Owner ;

		//-----------------------------------------------------------

		[SerializeField]
		protected UICanvas							m_DialogCanvas ;

		[SerializeField]
		protected UIImage							m_Mask ;

		[SerializeField]
		protected	int								m_BulrWdith  = 270 ;
		public		int								  BlurWidth  => m_BulrWdith ;

		[SerializeField]
		protected	int								m_BlurHeight = 480 ;
		public		int								  BlurHeight => m_BlurHeight ; 

		[SerializeField]
		protected	int								m_BlurLevel  = 4 ;
		public		int								  BlurLevel  => m_BlurLevel ;

		//-----------------------------------

		[SerializeField]
		protected   SimpleDialog					m_SimpleDialog ;
		public		SimpleDialog					  SimpleDialog => m_SimpleDialog ;

		//-----------------------------------------------------------


		/// <summary>
		/// 必須コンストラクタ
		/// </summary>
		/// <param name="owner"></param>
		public DialogController( Title owner ) : base( owner )
		{
			m_Owner = owner ;
		}

		/// <summary>
		/// 明示的にオーナーを設定する
		/// </summary>
		/// <param name="owner"></param>
		public void SetOwner( Title owner )
		{
			base.SetOwner( owner ) ;

			// 忘れるな
			m_Owner = owner ;

			// ダイアログ群の土台のキャンバスを非アクティブにしておく
			m_DialogCanvas.SetActive( false ) ;

			//----------------------------------

			// 各ダイアログにオーナー(ダイアログコントローラー)を設定する

			m_SimpleDialog.View.SetActive( false ) ;
			m_SimpleDialog.SetOwner( this ) ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 開いているダイアログの数を取得する
		/// </summary>
		/// <returns></returns>
		public	int GetOpeningCount()
		{
			int count = 0 ;

			if( m_SimpleDialog.IsOpening					== true ){ count ++ ; }


			return count ;
		}

		/// <summary>
		/// コントローラーを開始する
		/// </summary>
		public bool StartupController()
		{
			if( m_DialogCanvas.ActiveSelf == false )
			{
				int count = GetOpeningCount() ;
				if( count == 0 )
				{
					// ダイアログコントローラーを有効化する
					m_DialogCanvas.SetActive( true ) ;

					return true ;	// 有効になった
				}
			}

			return false ;
		}

		/// <summary>
		/// コントローラーを終了する
		/// </summary>
		public void CleanupController()
		{
			if( m_DialogCanvas.ActiveSelf == true )
			{
				int count = GetOpeningCount() ;
				if( count == 0 )
				{
					// 開いているダイアログが無くなれば全体も閉じる
					m_DialogCanvas.SetActive( false ) ;
				}
			}
		}

		/// <summary>
		/// マスクをフェードインさせる
		/// </summary>
		public void FadeInMask()
		{
			if( GetOpeningCount() >  1 )
			{
				return ;
			}

			m_Mask.PlayTween( "FadeIn" ) ;
		}

		/// <summary>
		/// マスクをフェードアウトさせる
		/// </summary>
		public void FadeOutMask()
		{
			if( GetOpeningCount() >  1 )
			{
				return ;
			}

			m_Mask.PlayTweenAndHide( "FadeOut" ) ;
		}

		/// <summary>
		/// 表示する
		/// </summary>
		public void Show()
		{
			m_DialogCanvas.SetActive( true ) ;
		}

		/// <summary>
		/// 隠蔽する
		/// </summary>
		public void Hide()
		{
			m_DialogCanvas.SetActive( false ) ;
		}
	}
}


