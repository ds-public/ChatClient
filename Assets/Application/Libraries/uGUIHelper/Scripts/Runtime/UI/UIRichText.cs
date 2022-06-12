using UnityEngine ;
using UnityEngine.UI ;
using System ;
using System.Collections ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	[RequireComponent( typeof( RichText ) ) ]

	/// <summary>
	/// uGUI:Text クラスの機能拡張コンポーネントクラス
	/// </summary>
	public class UIRichText : UIView
	{
		/// <summary>
		/// RectTransform のサイズを自動的に文字列のサイズに合わせるかどうか
		/// </summary>
		[SerializeField][HideInInspector]
		protected bool m_AutoSizeFitting = true ;

		public bool AutoSizeFitting
		{
			get
			{
				return m_AutoSizeFitting ;
			}
			set
			{
				if( m_AutoSizeFitting != value )
				{
					m_AutoSizeFitting  = value ;
					if( m_AutoSizeFitting == true )
					{
						Resize() ;
					}
				}
			}
		}

		/// <summary>
		/// ローカライズキー
		/// </summary>
		[SerializeField][HideInInspector]
		protected string m_LocalizationKey = string.Empty ;

		public string LocalizationKey
		{
			get
			{
				return m_LocalizationKey ;
			}
			set
			{
				m_LocalizationKey  = value ;	// Start() 以降に変更される事は想定していない
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 文字列自体のサイズ
		/// </summary>
		public Vector2 TextSize
		{
			get
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return Vector2.zero ;
				}

				return new Vector2( richText.preferredWidth, richText.preferredHeight ) ;
			}
		}

		/// <summary>
		/// 文字の最大縦幅
		/// </summary>
		public float FullHeight
		{
			get
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return 0 ;
				}

				return richText.preferredFullHeight ;
			}
		}

		/// <summary>
		/// フォントサイズ(ショートカット)
		/// </summary>
		public int FontSize
		{
			get
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return 0 ;
				}

				return richText.fontSize ;
			}
			set
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return ;
				}

				if( richText.fontSize != value )
				{
					richText.fontSize = value ;

					if( m_AutoSizeFitting == true )
					{
						Resize() ;
					}
				}
			}
		}
		
		/// <summary>
		/// テキスト(ショートカット)
		/// </summary>
		public string Text
		{
			get
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return null ;
				}
				return richText.text ;
			}
			set
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return ;
				}

				if( richText.text != value )
				{
					richText.text = value ;

					// 実行時のみ全角化する
					if( m_Zenkaku == true )
					{
						ToLargeForRichText() ;
					}
					else
					{
						if( m_AutoSizeFitting == true )
						{
							Resize() ;
						}
					}
				}
			}
		}
		
		/// <summary>
		/// フォント(ショートカット)
		/// </summary>
		public Font Font
		{
			get
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return null ;
				}

				return richText.font ;
			}
			set
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return ;
				}

				if( richText.font != value )
				{
					richText.font = value ;

					if( m_AutoSizeFitting == true )
					{
						Resize() ;
					}
				}
			}
		}
	
		/// <summary>
		/// カラー(ショートカット)
		/// </summary>
		public Color Color
		{
			get
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return Color.white ;
				}

				return richText.color ;
			}
			set
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return ;
				}

				if( richText.color.r != value.r || richText.color.g != value.g || richText.color.b != value.b || richText.color.a != value.a )
				{
					richText.color = value ;
				}
			}
		}

		/// <summary>
		/// １６進数値で色を設定する
		/// </summary>
		/// <param name="color"></param>
		public void SetColor( uint color )
		{
			Color = ARGB( color ) ;
		}

		/// <summary>
		/// マテリアル(ショートカット)
		/// </summary>
		public Material Material
		{
			get
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return null ;
				}

				return richText.material ;
			}
			set
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return ;
				}

				richText.material = value ;
			}
		}

		/// <summary>
		/// フォントスタイル(ショートカット)
		/// </summary>
		public FontStyle FontStyle
		{
			get
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return FontStyle.Normal ;
				}

				return richText.fontStyle ;
			}
			set
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return ;
				}

				if( richText.fontStyle != value )
				{
					richText.fontStyle = value ;

					if( m_AutoSizeFitting == true )
					{
						Resize() ;
					}
				}
			}
		}
		
		/// <summary>
		/// リッチテキスト(ショートカット)
		/// </summary>
		public bool SupportRichText
		{
			get
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return false ;
				}

				return richText.supportRichText ;
			}
			set
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return ;
				}

				if( richText.supportRichText != value )
				{
					richText.supportRichText = value ;

					if( m_AutoSizeFitting == true )
					{
						Resize() ;
					}
				}
			}
		}
		
		/// <summary>
		/// アライメント(ショートカット)
		/// </summary>
		public TextAnchor Alignment
		{
			get
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return TextAnchor.MiddleCenter ;
				}

				return richText.alignment ;
			}
			set
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return ;
				}

				if( richText.alignment != value )
				{
					richText.alignment = value ;

					if( m_AutoSizeFitting == true )
					{
						Resize() ;
					}
				}
			}
		}
	
		/// <summary>
		/// リサイズ(ショートカット)
		/// </summary>
		public bool ResizeTextForBestFit
		{
			get
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return false ;
				}

				return richText.resizeTextForBestFit ;
			}
			set
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return ;
				}

				if( richText.resizeTextForBestFit != value )
				{
					richText.resizeTextForBestFit = value ;

					if( m_AutoSizeFitting == true )
					{
						Resize() ;
					}
				}
			}
		}
	
		/// <summary>
		/// 横方向の表示モード(ショートカット)
		/// </summary>
		public  HorizontalWrapMode HorizontalOverflow
		{
			get
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return HorizontalWrapMode.Overflow ;
				}

				return richText.horizontalOverflow ;
			}
			set
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return ;
				}

				if( richText.horizontalOverflow != value )
				{
					richText.horizontalOverflow = value ;

					if( m_AutoSizeFitting == true )
					{
						Resize() ;
					}
				}
			}
		}
	
		/// <summary>
		/// 縦方向の表示モード(ショートカット)
		/// </summary>
		public  VerticalWrapMode VerticalOverflow
		{
			get
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return VerticalWrapMode.Overflow ;
				}

				return richText.verticalOverflow ;
			}
			set
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return ;
				}

				if( richText.verticalOverflow != value )
				{
					richText.verticalOverflow = value ;

					if( m_AutoSizeFitting == true )
					{
						Resize() ;
					}
				}
			}
		}

		/// <summary>
		/// 改行時の下方向への移動量係数(ショートカット)
		/// </summary>
		public  float LineSpacing
		{
			get
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return 1.0f ;
				}

				return richText.lineSpacing ;
			}
			set
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return ;
				}

				if( richText.lineSpacing != value )
				{
					richText.lineSpacing = value ;

					if( m_AutoSizeFitting == true )
					{
						Resize() ;
					}
				}
			}
		}

		/// <summary>
		/// レイキャストターゲット(ショートカット)
		/// </summary>
		override public bool RaycastTarget
		{
			get
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return false ;
				}

				return richText.raycastTarget ;
			}
			set
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return ;
				}

				richText.raycastTarget = value ;
			}
		}


		/// <summary>
		/// 上スペース係数(ショートカット)
		/// </summary>
		public  float TopMaginSpacing
		{
			get
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return 0 ;
				}

				return richText.topMarginSpacing ;
			}
			set
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return ;
				}

				if( richText.topMarginSpacing != value )
				{
					richText.topMarginSpacing  = value ;

					if( m_AutoSizeFitting == true )
					{
						Resize() ;
					}
				}
			}
		}

		/// <summary>
		/// 下スペース係数(ショートカット)
		/// </summary>
		public  float BottomMaginSpacing
		{
			get
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return 0 ;
				}

				return richText.bottomMarginSpacing ;
			}
			set
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return ;
				}

				if( richText.bottomMarginSpacing != value )
				{
					richText.bottomMarginSpacing  = value ;

					if( m_AutoSizeFitting == true )
					{
						Resize() ;
					}
				}
			}
		}

#if false
		/// <summary>
		/// カーソル位置(ショートカット)
		/// </summary>
		public  Vector2 CursorPosition
		{
			get
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return Vector2.zero ;
				}
				return richText.cursorPosition ;
			}
		}
#endif

		/// <summary>
		/// 上のマージンが必要かどうか(ショートカット)
		/// </summary>
		public  bool IsNeedTopMagin
		{
			get
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return false ;
				}

				return richText.isNeedTopMargin ;
			}
		}

		/// <summary>
		/// 下のマージンが必要かどうか(ショートカット)
		/// </summary>
		public  bool IsNeedBottomMagin
		{
			get
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return false ;
				}

				return richText.isNeedBottomMargin ;
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// ビューのコントロールの有効状態
		/// </summary>
		public bool ViewControllEnabled
		{
			get
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return false ;
				}

				return richText.viewControllEnabled ;
			}
			set
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return ;
				}

				richText.viewControllEnabled = value ;
			}
		}
		
		/// <summary>
		/// 表示文字数(ショートカット)
		/// </summary>
		public  int LengthOfView
		{
			get
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return 0 ;
				}

				return richText.lengthOfView ;
			}
			set
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return ;
				}

				if( richText.lengthOfView != value )
				{
					richText.lengthOfView  = value ;

					if( m_AutoSizeFitting == true )
					{
						Resize() ;
					}
				}
			}
		}

		/// <summary>
		/// 表示開始行数(ショートカット)
		/// </summary>
		public  int StartLineOfView
		{
			get
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return 0 ;
				}

				return richText.startLineOfView ;
			}
			set
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return ;
				}

				if( richText.startLineOfView != value )
				{
					richText.startLineOfView  = value ;

					if( m_AutoSizeFitting == true )
					{
						Resize() ;
					}
				}
			}
		}

		/// <summary>
		/// 表示終了行数(ショートカット)
		/// </summary>
		public  int EndLineOfView
		{
			get
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return 0 ;
				}

				return richText.endLineOfView ;
			}
			set
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return ;
				}

				if( richText.endLineOfView != value )
				{
					richText.endLineOfView  = value ;

					if( m_AutoSizeFitting == true )
					{
						Resize() ;
					}
				}
			}
		}

		/// <summary>
		/// フェード開始オフセット(ショートカット)
		/// </summary>
		public  int StartOffsetOfFade
		{
			get
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return 0 ;
				}

				return richText.startOffsetOfFade ;
			}
			set
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return ;
				}

				richText.startOffsetOfFade = value ;
			}
		}

		/// <summary>
		/// フェード終了オフセット(ショートカット)
		/// </summary>
		public  int EndOffsetOfFade
		{
			get
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return 0 ;
				}

				return richText.endOffsetOfFade ;
			}
			set
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return ;
				}

				richText.endOffsetOfFade = value ;
			}
		}
		
		/// <summary>
		/// 表示文字比率(ショートカット)
		/// </summary>
		public  float RatioOfFade
		{
			get
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return 0 ;
				}

				return richText.ratioOfFade ;
			}
			set
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return ;
				}

				richText.ratioOfFade = value ;
			}
		}

		/// <summary>
		/// 透過対象文字数(ショートカット)
		/// </summary>
		public  int WidthOfFade
		{
			get
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return 0 ;
				}

				return richText.widthOfFade ;
			}
			set
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return ;
				}

				richText.widthOfFade = value ;
			}
		}
		
		/// <summary>
		/// ルビが使用されているかどうか(ショートカット)
		/// </summary>
		public bool IsRubyUsed
		{
			get
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return false ;
				}

				return richText.isRubyUsed ;
			}
		}
		
		//--------------------------------------------------

		// 全角にするか
		[HideInInspector][SerializeField]
		protected bool m_Zenkaku = false ;

		/// <summary>
		/// 数値と記号を全角文字にするかどうか
		/// </summary>
		virtual public    bool  Zenkaku
		{
			get
			{
				return m_Zenkaku ;
			}
			set
			{
				if( m_Zenkaku != value )
				{
					m_Zenkaku = value ;
				}
			}
		}

		/// <summary>
		/// 数値と記号を全角文字にするかどうかを設定する
		/// </summary>
		/// <param name="zenkaku">表示状態(true=全角にする・false=全角にしない)</param>
		public void SetZenkaku( bool zenkaku )
		{
			Zenkaku = zenkaku ;
		}

		// 文字列を全角に変更する
		/// <summary>
		/// 文字列中の半角数値を全角数値に置き換える
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		private void ToLargeForRichText()
		{
			RichText richText = CRichText ;
			if( richText == null )
			{
				return ;
			}

			string s = richText.text ;

			if( string.IsNullOrEmpty( s ) == true )
			{
				return ;
			}

			int i, l = s.Length ;

			char[] code = new char[ l ] ;

			char c ;
			bool hit = false ;
			bool tag = false ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				c = s[ i ] ;

				if( tag == false )
				{
					if( c == '<' )
					{
						tag = true ;
					}
					else
					if( c >= '0' && c <= '9' )
					{
						c = ( char )( ( int )c - ( int )'0' + ( int )'０' ) ;
						hit = true ;
					}
					else
					if( c >= 'a' && c <= 'z' )
					{
						c = ( char )( ( int )c - ( int )'a' + ( int )'ａ' ) ;
						hit = true ;
					}
					else
					if( c >= 'A' && c <= 'Z' )
					{
						c = ( char )( ( int )c - ( int )'A' + ( int )'Ａ' ) ;
						hit = true ;
					}
				}
				else
				{
					if( c == '>' )
					{
						tag = false ;
					}
				}
				
				code[ i ] = c ;
			}

			if( hit == true )
			{
				richText.text = new string( code ) ;
				if( m_AutoSizeFitting == true )
				{
					Resize() ;
				}
			}
		}

		//--------------------------------------------------

		public Shadow	Shadow
		{
			get
			{
				return CShadow ;
			}
		}

		public Outline	Outline
		{
			get
			{
				return COutline ;
			}
		}

		public UIGradient Gradient
		{
			get
			{
				return CGradient ;
			}
		}

		//-----------------------------------------------------------

		// ローカライズをリクエスト中か
		private bool m_LocalizeRequest ;

		//-----------------------------------------------------------
		
		/// <summary>
		/// 最大文字数(ショートカット)
		/// </summary>
		public  int Length
		{
			get
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return 0 ;
				}

				return richText.length ;
			}
		}

		/// <summary>
		/// 行数を取得する(ショートカット)
		/// </summary>
		public int Line
		{
			get
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return 0 ;
				}

				return richText.line ;
			}
		}

		/// <summary>
		/// 指定したラインの開始時の描画対象となる文字数を取得する(ショートカット)
		/// </summary>
		/// <param name="tLine"></param>
		/// <returns></returns>
		public int GetStartOffsetOfLine( int line )
		{
			RichText richText = CRichText ;
			if( richText == null )
			{
				return -1 ;
			}

			return richText.GetEndOffsetOfLine( line ) ;
		}

		/// <summary>
		/// 指定したラインの終了時の描画対象となる文字数を取得する(ショートカット)
		/// </summary>
		/// <param name="tLine"></param>
		/// <returns></returns>
		public int GetEndOffsetOfLine( int line )
		{
			RichText richText = CRichText ;
			if( richText == null )
			{
				return -1 ;
			}

			return richText.GetEndOffsetOfLine( line ) ;
		}

		/// <summary>
		/// 最初の表示文字数を取得する
		/// </summary>
		public int StartOffsetOfView
		{
			get
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return -1 ;
				}

				return richText.startOffsetOfView ;
			}
		}

		/// <summary>
		/// 最後の表示文字数を取得する
		/// </summary>
		public int EndOffsetOfView
		{
			get
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return -1 ;
				}

				return richText.endOffsetOfView ;
			}
		}


		/// <summary>
		/// 拡張タグイベント
		/// </summary>
		public RichText.ExtraTagEvent[] ExtraTagEvent
		{
			get
			{
				RichText richText = CRichText ;
				if( richText == null )
				{
					return null ;
				}

				return richText.extraTagEvent ;
			}
		}

		/// <summary>
		/// 拡張タグイベントを取得する
		/// </summary>
		public RichText.ExtraTagEvent[] GetExtraTagEvent( params string[] filter )
		{
			RichText richText = CRichText ;
			if( richText == null )
			{
				return null ;
			}

			return richText.GetExtraTagEvent( filter ) ;
		}

		/// <summary>
		/// 指定した文字数のキャレットの座標を指定する
		/// </summary>
		/// <param name="tLength"></param>
		/// <returns></returns>
		public Vector2 GetCaretPosition( int length = -1 )
		{
			RichText richText = CRichText ;
			if( richText == null )
			{
				return Vector2.zero ;
			}

			return richText.GetCaretPosition( length ) ;
		}

		//-------------------------------------------------------------------------------------------

		private bool m_Playing = false ;
		private bool m_Pausing = false ;
		private bool m_EventPausing = false ;
		private bool m_Break = false ;

		private IEnumerator	m_Callback = null ;
		private IEnumerator m_Coroutine = null ;

		/// <summary>
		/// 拡張タグ名を設定する
		/// </summary>
		/// <param name="extraTagName"></param>
		public void SetExtraTagName( params string[] extraTagName )
		{
			RichText richText = CRichText ;
			if( richText == null )
			{
				return ;
			}

			richText.SetExtraTagName( extraTagName ) ;
		}
		
		/// <summary>
		/// 文章をアニメーション表示させる
		/// </summary>
		/// <param name="startLine">開始行</param>
		/// <param name="endLine">終了行</param>
		/// <param name="codeTime">１文字あたりの表示時間(秒)</param>
		public void Play( int startLine = -1, int endLine = -1, float codeTime = 0.1f, bool isAbsoluteTime = false )
		{
			m_Coroutine = Play_Coroutine<System.Object>( startLine, endLine, codeTime, isAbsoluteTime, null, null, null ) ;
			StartCoroutine( m_Coroutine ) ;
		}
		
		/// <summary>
		/// 文章をアニメーション表示させる
		/// </summary>
		/// <param name="startLine">開始行</param>
		/// <param name="endLine">終了行</param>
		/// <param name="codeTime">１文字あたりの表示時間(秒)</param>
		/// <param name="onEvent">独自イベント発生時のコールバック</param>
		public void Play<T>( int startLine = -1, int endLine = -1, float codeTime = 0.1f, bool isAbsoluteTime = false, Func<RichText.ExtraTagEvent, T, IEnumerator> onEvent = null, T anyObject = null, params string[] filter ) where T : class 
		{
			m_Coroutine = Play_Coroutine( startLine, endLine, codeTime, isAbsoluteTime, onEvent, anyObject, filter ) ;
			StartCoroutine( m_Coroutine ) ;
		}
		
		// 文章をアニメーション表示させる
		private IEnumerator Play_Coroutine<T>( int startLine, int endLine, float codeTime, bool isAbsoluteTime, Func<RichText.ExtraTagEvent, T, IEnumerator> onEvent, T anyObject, params string[] filter )
		{
			RichText richText = CRichText ;
			if( richText == null )
			{
				yield break ;
			}

			int line = Line ;

			if( startLine <  0 )
			{
				startLine   = 0 ;
			}
			if( startLine >  ( line - 1 ) )
			{
				startLine  = ( line - 1 ) ;
			}

			if( endLine <  0 )
			{
				endLine  = line ;
			}
			if( endLine >  line )
			{
				endLine  = line ;
			}

			if( endLine >= 0 && endLine <  ( startLine + 1 ) )
			{
				endLine  = ( startLine + 1 ) ;
			}

			if( codeTime <= 0 )
			{
				codeTime  = 0.05f ;
			}

			//----------------------------------------------------------

			m_Playing = true ;
			m_Pausing = false ;
			m_EventPausing = false ;
			m_Break = false ;

			richText.viewControllEnabled	= true ;
			richText.lengthOfView			= -1 ;
			richText.startLineOfView		= startLine ;
			richText.endLineOfView			= endLine ;

			List<RichText.ExtraTagEvent> events = new List<RichText.ExtraTagEvent>() ;

			RichText.ExtraTagEvent[] eventsTemporary = richText.GetExtraTagEvent( filter ) ;

			bool f = false ;
			if( events != null )
			{
				int i, l = eventsTemporary.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					events.Add( eventsTemporary[ i ] ) ;
				}

				l = events.Count ;
				if( l >  0 )
				{
					if( events[ l - 1 ].offset >=  richText.endOffsetOfView )
					{
						// 一番最後にスベントが存在する
						f = true ;
					}
				}
			}

			if( f == false )
			{
				// 最後の終端を追加する
				events.Add( new RichText.ExtraTagEvent( richText.endOffsetOfView, null, null ) ) ;
			}

			//----------------------------------------------------------

			// タイマー開始
			StartTimer( isAbsoluteTime ) ;

			float time, t ;

			int o = richText.startOffsetOfView ;

			// イベントでループ
			int e ;
			for( e  = 0 ; e <  events.Count ; e ++ )
			{
				//--------------------------------------------------------/

				// １イベントあたりの文字列の表示時間
				time = ( events[ e ].offset - o ) * codeTime ;

				richText.ratioOfFade = 0 ;
				richText.startOffsetOfFade = o ;
				richText.endOffsetOfFade = events[ e ].offset ;

				// １イベントの文字列の表示ループ
				t = 0 ;
				while( t <  time )
				{
					if( m_Pausing == true )
					{
						// 先にポーズ中かのチェックを入れておかないと１フレーム消費して表示が激遅なる
						yield return new WaitWhile( () => m_Pausing == true ) ;
					}

					if( m_Break == true )
					{
						// 強制終了
						richText.ratioOfFade		= 1 ;
						richText.startOffsetOfFade	= richText.startOffsetOfView ;
						richText.endOffsetOfFade	= richText.endOffsetOfView ;

						m_Break = false ;
						m_Playing = false ;

						yield break ;
					}

					t += GetDeltaTime() ;
					if( t >  time )
					{
						t  = time ;
					}

					richText.ratioOfFade = t / time ;

					yield return null ;
				}

				if( string.IsNullOrEmpty( events[ e ].tagName ) == false )
				{
					if( onEvent != null )
					{
						m_EventPausing = true ;

						// イベント用のコールバックを呼び出す
						m_Callback = onEvent( events[ e ], anyObject ) ;
						yield return StartCoroutine( m_Callback  ) ;

						m_EventPausing = false ;
					}
				}

				o = events[ e ].offset ;
			}

			m_Break = false ;
			m_Playing = false ;

			m_Coroutine = null ;
		}
		
		private bool m_IsAbsoluteTime = false ;

		// タイマーを開始する
		private void StartTimer( bool isAbsoluteTime )
		{
			m_IsAbsoluteTime = isAbsoluteTime ;
		}

		// 経過時間を取得する
		private float GetDeltaTime()
		{
			if( m_IsAbsoluteTime == false )
			{
				return Time.deltaTime ;
			}
			else
			{
				return Time.unscaledDeltaTime ;
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 再生中かどうか
		/// </summary>
		public bool IsPlaying
		{
			get
			{
				return m_Playing ;
			}
		}

		/// <summary>
		/// 一時停止中かどうか
		/// </summary>
		public bool IsPausing
		{
			get
			{
				if( m_Pausing == true || m_EventPausing == true )
				{
					return true ;
				}

				return false ;
			}
		}



		/// <summary>
		/// 文章アニメーションを一時停止する
		/// </summary>
		public void Pause()
		{
			m_Pausing = true ;
		}

		/// <summary>
		/// ポーズを解除する
		/// </summary>
		public void Unpause()
		{
			m_Pausing = false ;
		}


		/// <summary>
		/// 文章アニメーションを強制終了する
		/// </summary>
		public void Stop()
		{
			if( m_Coroutine != null )
			{
				StopCoroutine( m_Coroutine ) ;
				m_Coroutine = null ;
			}

			m_Playing = false ;
			m_Pausing = false ;
			m_Break   = false ;
		}


		/// <summary>
		/// アニメーション中のメッセージを全て表示する
		/// </summary>
		public void Finish()
		{
			if( m_Playing == true )
			{
//				if( m_Callback != null )
//				{
//					StopCoroutine( m_Callback ) ;
//					m_Callback = null ;
//				}

				m_Break = true ;
			}
		}

		//--------------------------------------------------
	
		/// <summary>
		/// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		/// </summary>
		/// <param name="option"></param>
		override protected void OnBuild( string option = "" )
		{
			RichText richText = CRichText ;
			if( richText == null )
			{
				richText = gameObject.AddComponent<RichText>() ;
			}
			if( richText == null )
			{
				// 異常
				return ;
			}

			//----------------------------

			Color	defaultTextColor = Color.white ;

			Font	defaultFont = null ;
			int		defaultFontSize = 0 ;

#if UNITY_EDITOR

			if( Application.isPlaying == false )
			{
				// メニューから操作した場合のみ自動設定を行う
				DefaultSettings ds = Resources.Load<DefaultSettings>( "uGUIHelper/DefaultSettings" ) ;
				if( ds != null )
				{
					defaultTextColor	= ds.TextColor ;

					defaultFont			= ds.Text_Font ;
					defaultFontSize		= ds.Text_FontSize ;
				}
			}
			
#endif

			richText.color = defaultTextColor ;

			if( defaultFont == null )
			{
				richText.font = Resources.GetBuiltinResource( typeof( Font ), "Arial.ttf" ) as Font ;
			}
			else
			{
				richText.font = defaultFont ;
			}

			if( defaultFontSize <= 0 )
			{
				richText.fontSize = 32 ;
			}
			else
			{
				richText.fontSize = defaultFontSize ;
			}

			richText.alignment = TextAnchor.MiddleLeft ;

			richText.horizontalOverflow = HorizontalWrapMode.Overflow ;
			richText.verticalOverflow   = VerticalWrapMode.Overflow ;
			
			ResetRectTransform() ;

			//----------------------------------

			if( IsCanvasOverlay == true )
			{
				richText.material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}
		}

		protected override void OnStart()
		{
			base.OnStart() ;

			if( Application.isPlaying == true )
			{
				RichText richText = CRichText ;

				if( richText != null )
				{
					if( string.IsNullOrEmpty( m_LocalizationKey ) == false )
					{
						// ローカライズ対応処理を行う
						if( UILocalization.AddRequest( OnLocalized, m_LocalizationKey ) == false )
						{
							// 準備が整っていないのでキューに貯められた
							m_LocalizeRequest = true ;
						}
					}

					// 実行時のみ全角化する
					if( m_Zenkaku == true )
					{
						// 強制全角化
						ToLargeForRichText() ;
					}
				}
			}
		}
		
		/// <summary>
		/// ローカライズの準備が整い変換後の文字列が返された(ローカライズマネージャーの準備が整わないと設定できない)
		/// </summary>
		/// <param name="text"></param>
		private void OnLocalized( string text )
		{
			Text = text ;
			m_LocalizeRequest = false ;	// ローカライズが行われた
		}

		override protected void OnLateUpdate()
		{
			if( m_AutoSizeFitting == true )
			{
				Resize() ;
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy() ;

			if( m_LocalizeRequest == true )
			{
				// ローカライズのリクエスト中である場合はリクエストをキャンセルする
				UILocalization.RemoveRequest( OnLocalized ) ;
				m_LocalizeRequest = false ;
			}
		}

		private void Resize()
		{
			RichText t = CRichText ;
			RectTransform r = GetRectTransform() ;
			if( r != null && t != null )
			{
				Vector2 size = r.sizeDelta ;

				if( r.anchorMin.x == r.anchorMax.x )
				{
					if( t.horizontalOverflow == HorizontalWrapMode.Overflow )
					{
						size.x = t.preferredWidth ;
					}
				}

				if( r.anchorMin.y == r.anchorMax.y )
				{
					if( t.verticalOverflow == VerticalWrapMode.Overflow )
					{
						size.y = t.preferredHeight ;
					}
				}

				r.sizeDelta = size ;
			}
		}
	}
}

