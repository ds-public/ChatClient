using System ;
using System.Collections ;
using System.Collections.Generic ;

using Cysharp.Threading.Tasks ;

using UnityEngine ;

using uGUIHelper ;


namespace Template.Layouts
{
	/// <summary>
	/// テンプレートレイアウトのコントロールクラス Version 2022/05/04
	/// </summary>
	public class Template : LayoutBase
	{
		[Header("UIのインスタンス群")]

		[SerializeField]
		protected UITextMesh	m_Annotation ;

		//-------------------------------------------------------------------------------------------

		// レイアウトシーンの名前を設定する
		protected override string SetLayoutSceneName()
		{
			return Scene.Layout.Template ;	// 単体デバックの背景塗りつぶし(兼・常駐しない場合のシーンの削除)用
		}

		/// <summary>
		/// インスタンス生成直後に呼び出される(画面表示は行われていない)
		/// </summary>
		override protected void OnAwake()
		{
			// ＵＩの非表示化など最速で実行しなければならない初期化処理を記述する

			// スクリーンは透明化
			m_Screen.Color = ExColor.Transparency ;

			// 文字列は初期状態では非表示
			m_Annotation.SetActive( false ) ;
		}

		/// <summary>
		/// 最初のアップデートの前に呼び出される(既に画面表示は行われてしまっているのでＵＩの非表示化は OnAwake で行うこと)
		/// </summary>
		/// <returns></returns>
		override protected async UniTask OnStart()
		{
			await Yield() ;
		}

#if UNITY_EDITOR
		// 単体デバッグを実行する(このレイアウトのシーンを開いたままUnityEitorでPlayを実行する)
		protected override async UniTask RunDebug()
		{
			// 準備
			await Prepare() ;

			// 演出を実行する
			await Play() ;
		}
#endif
		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// レイアウトの画面構成の準備を行う
		/// </summary>
		/// <returns></returns>
		public async UniTask Prepare()
		{
			// 文字の色を白に(サンプル)
			m_Annotation.Color = Color.white ;

			await Yield() ;
		}

		/// <summary>
		/// 演出の再生を行(実際は必要な情報を引数として渡す)
		/// </summary>
		/// <returns></returns>
		public async UniTask Play()
		{
			// フェードインを実行する
			await FadeIn() ;

			// 演出を処理する
			await When( m_Annotation.PlayTween( "Animation" ) ) ;

			// フェードアウトを実行する
			await FadeOut() ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// フェードインでレイアウトを表示する
		/// </summary>
		/// <returns></returns>
		public async UniTask FadeIn()
		{
			gameObject.SetActive( true ) ;

			await m_Annotation.PlayTween( "FadeIn" ) ;
		}

		/// <summary>
		/// フェードアウトでレイアウトを非表示にする
		/// </summary>
		/// <returns></returns>
		public async UniTask FadeOut()
		{
			await m_Annotation.PlayTweenAndHide( "FadeOut" ) ;

			gameObject.SetActive( false ) ;
		}
	}
}

