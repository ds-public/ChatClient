using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using uGUIHelper ;
using Cysharp.Threading.Tasks ;

namespace Template.Screens.TitleClasses.UI
{
	/// <summary>
	/// シンプル確認ダイアログのクラス
	/// </summary>
	public class SimpleDialog : DialogBase
	{
		//-------------------------------------------------------------------------------------------
		// このダイアログ固有のＵＩの定義

		//-------------------------------------------------------------------------------------------

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ダイアログを開く
		/// </summary>
		/// <returns></returns>
		public async UniTask Open()
		{
			//----------------------------------------------------------
			// このダイアログ固有のＵＩの設定


			//----------------------------------

			//----------------------------------------------------------

			bool isCanceled = false ;

			// ダイアログをフェードインで開く
			await OpenBase( null, () =>
			{
				isCanceled = true ;	// キャンセルされた
			} ) ;

			// 何らかのアクションが実行されるのを待つ
			await WaitUntil( () => ( isCanceled == true ) ) ;

			//----------------------------------------------------------

			return ;
		}
	}
}
