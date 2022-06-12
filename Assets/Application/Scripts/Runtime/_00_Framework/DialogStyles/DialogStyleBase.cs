using System.Collections ;
using System.Collections.Generic ;

using Cysharp.Threading.Tasks ;

using UnityEngine ;

using uGUIHelper ;

namespace Template.DialogStyle
{
	/// <summary>
	/// ダイアログスタイルの基底クラス
	/// </summary>
	public class DialogStyleBase : ExMonoBehaviour
	{
		// 共通に存在するプロパティ

		/// <summary>
		/// フェード用のコンポーネントのインスタンス
		/// </summary>
		[SerializeField]
		protected UIImage	m_Fade ;

		/// <summary>
		/// ウィンドウ用のコンポーネントのインスタンス
		/// </summary>
		[SerializeField]
		protected UIImage	m_Window ;

		/// <summary>
		/// パディング
		/// </summary>
		public RectOffset	Padding = new RectOffset() ;

		//-----------------------------------------------------------

		/// <summary>
		/// ボタンのパディング
		/// </summary>
		public RectOffset ButtonPadding = new RectOffset() ;

		/// <summary>
		/// ボタンの最低サイズ
		/// </summary>
		public Vector2 MinimunButtonSize = new Vector2( 120,  48 ) ;

		/// <summary>
		/// ボタンの最大サイズ
		/// </summary>
		public Vector2 MaximumButtonSize = new Vector2( 240,  48 ) ;

		// タイトルとメッセージの有無で縦方向の長さを自動調整する
		// ボタンの最低横幅は決まっているがボタンのラベルによって自動的に広がる

		//-----------------------------------------------------------

		/// <summary>
		/// 状態に変化があったかどうか
		/// </summary>
		protected bool m_Dirty = false ;

		/// <summary>
		/// 最終的なコールバック呼び出しで渡される結果文字列
		/// </summary>
		protected int m_Result = -1 ;	// 結果
		public int Result{ get{ return m_Result ; } }

		/// <summary>
		/// 閉じられたかどうか
		/// </summary>
		protected bool m_IsClosed ;
		public bool IsClosed{ get{ return m_IsClosed ; } }

		/// <summary>
		/// アクティブと非アクティブ状態を設定する
		/// </summary>
		/// <param name="state"></param>
		public void SetActive( bool state )
		{
			gameObject.SetActive( state ) ;
		}

		/// <summary>
		/// フェードインで表示する
		/// </summary>
		virtual public async UniTask FadeIn()
		{
			// ノッチ(セーフエリフ)対策
			m_Fade.SetMarginY( -960, -960 ) ;

			_ = m_Fade.PlayTween( "FadeIn" ) ;
			_ = m_Window.PlayTween( "FadeIn" ) ;

			// フェードイン効果が終了するのを待つ
			await WaitWhile( () => ( m_Fade.IsAnyTweenPlaying | m_Window.IsAnyTweenPlaying ) ) ;

			Visible() ;
		}

		/// <summary>
		/// ＵＩの状態を完成させる
		/// </summary>
		virtual public void Commit(){}

		/// <summary>
		/// 準備を行う
		/// </summary>
		virtual public void Prepare()
		{
			m_Dirty = true ;
			m_IsClosed = false ;
		}

		/// <summary>
		/// ダイアログが表示されたら呼び正される
		/// </summary>
		virtual protected void Visible(){}

		internal void Start()
		{
			Commit() ;	// コミット忘れようの対策
		}
	
		/// <summary>
		/// ダイアログを閉じる
		/// </summary>
		/// <param name="result">ダイアログが閉じられた後に呼び出すコールバックに渡す文字列</param>
		virtual public void Close( int result = -1 )
		{
			m_Result = result ;
			_ = FadeOut() ;
		}

		/// <summary>
		/// ダイアログをフォードアウト効果付きで非表示にして最後にクローズ時のデリゲードを呼び出す
		/// </summary>
		/// <returns>列挙子</returns>
		virtual protected async UniTask FadeOut()
		{
			_ = m_Fade.PlayTween( "FadeOut" ) ;
			_ = m_Window.PlayTween( "FadeOut" ) ;

			// フェードアウト効果が終了するのを待つ
			await WaitWhile( () => ( m_Fade.IsAnyTweenPlaying | m_Window.IsAnyTweenPlaying ) ) ;

			m_Fade.SetActive( false ) ;
			m_Window.SetActive( false ) ;
		}

		//-------------------------------------------------------------------------------------------

		// 必要に応じてボタンをリサイズする
		protected void ResizeButton( UIButton button )
		{
			float bw, bh, tw, th ;

			if( button.LabelTextWidth == 0 )
			{
				// 文字が無い
				return ;
			}

			// 横幅のチェック
			bw = button.Width ;
			tw = button.LabelTextWidth + ButtonPadding.horizontal ;
			if( tw >  bw )
			{
				// 文字に対して現在のボタンの横幅が小さすぎる
				bw  = tw ;
					
				if( bw >  MaximumButtonSize.x )
				{
					bw  = MaximumButtonSize.x ;	// 上限を突破するので上限に合わせる
				}
			}
			if( bw <  MinimunButtonSize.x )
			{
				bw  = MinimunButtonSize.x ;	// 下限を突破するので下限に合わせる
			}

			// 縦幅のチェック
			bh = button.Height ;
			th = button.LabelTextHeight + ButtonPadding.vertical ;
			if( th >  bh )
			{
				// 文字に対して現在のボタンの縦幅が小さすぎる
				bh  = th ;
					
				if( bh >  MaximumButtonSize.y )
				{
					bh  = MaximumButtonSize.y ;	// 上限を突破するので上限に合わせる
				}
			}
			if( bh <  MinimunButtonSize.y )
			{
				bh  = MinimunButtonSize.y ;	// 下限を突破するので下限に合わせる
			}

			button.SetSize( bw, bh ) ;
		}
	}
}
