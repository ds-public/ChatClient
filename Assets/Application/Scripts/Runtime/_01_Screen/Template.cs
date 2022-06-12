using System ;
using System.Collections ;
using System.Collections.Generic ;

using Cysharp.Threading.Tasks ;

using UnityEngine ;

using uGUIHelper ;


namespace Template.Screens
{
	/// <summary>
	/// テンプレートスクリーンのコントロールクラス Version 2022/05/04
	/// </summary>
	public class Template : ScreenBase
	{
		/// <summary>
		/// インスタンス生成直後に呼び出される(画面表示は行われていない)
		/// </summary>
		override protected void OnAwake()
		{
			// ＵＩの非表示化など最速で実行しなければならない初期化処理を記述する
		}

		/// <summary>
		/// 最初のアップデートの前に呼び出される(既に画面表示は行われてしまっているのでＵＩの非表示化は OnAwake で行うこと)
		/// </summary>
		/// <returns></returns>
		override protected async UniTask OnStart()
		{
			//----------------------------------------------------------

			// [シーンの準備が完全に整ったのでフェードイン許可のフラグを有効にする]
			Scene.Ready = true ;

			//----------------------------------------------------------

			// [フェードイン完了を待つ]
			await Scene.WaitForFading() ;
		}
	}
}

