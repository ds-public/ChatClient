using UnityEngine ;
using UnityEngine.UI ;
using System.Collections ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	[RequireComponent( typeof( UnityEngine.UI.Text ) ) ]

	/// <summary>
	/// uGUI:Text クラスの機能拡張コンポーネントクラス
	/// </summary>
	public class UIText : UIView
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
		/// 最大文字数(ショートカット)
		/// </summary>
		public  int Length
		{
			get
			{
				Text text = CText ;
				if( text == null )
				{
					return 0 ;
				}
				if( string.IsNullOrEmpty( text.text ) == true )
				{
					return 0 ;
				}

				return text.text.Length ;
			}
		}

		/// <summary>
		/// 文字列自体のサイズ
		/// </summary>
		public Vector2 TextSize
		{
			get
			{
				Text text = CText ;
				if( text == null )
				{
					return Vector2.zero ;
				}

				return new Vector2( text.preferredWidth, text.preferredHeight ) ;
			}
		}

		/// <summary>
		/// フォントサイズ(ショートカット)
		/// </summary>
		public int FontSize
		{
			get
			{
				Text text = CText ;
				if( text == null )
				{
					return 0 ;
				}

				return text.fontSize ;
			}
			set
			{
				Text text = CText ;
				if( text == null )
				{
					return ;
				}

				if( text.fontSize != value )
				{
					text.fontSize = value ;

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
				Text text = CText ;
				if( text == null )
				{
					return null ;
				}

				return text.text ;
			}
			set
			{
				Text text = CText ;
				if( text == null )
				{
					return ;
				}

				if( text.text != value )
				{
					text.text = value ;

					// 実行時のみ全角化する
					if( m_Zenkaku == true )
					{
						ToLargeForText() ;
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
				Text text = CText ;
				if( text == null )
				{
					return null ;
				}

				return text.font ;
			}
			set
			{
				Text text = CText ;
				if( text == null )
				{
					return ;
				}

				if( text.font != value )
				{
					text.font = value ;

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
				Text text = CText ;
				if( text == null )
				{
					return Color.white ;
				}

				return text.color ;
			}
			set
			{
				Text text = CText ;
				if( text == null )
				{
					return ;
				}

				if( text.color.r != value.r || text.color.g != value.g || text.color.b != value.b || text.color.a != value.a )
				{
					text.color = value ;
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
				Text text = CText ;
				if( text == null )
				{
					return null ;
				}

				return text.material ;
			}
			set
			{
				Text text = CText ;
				if( text == null )
				{
					return ;
				}

				text.material = value ;
			}
		}

		/// <summary>
		/// フォントスタイル(ショートカット)
		/// </summary>
		public FontStyle FontStyle
		{
			get
			{
				Text text = CText ;
				if( text == null )
				{
					return FontStyle.Normal ;
				}

				return text.fontStyle ;
			}
			set
			{
				Text text = CText ;
				if( text == null )
				{
					return ;
				}

				if( text.fontStyle != value )
				{
					text.fontStyle = value ;

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
				Text text = CText ;
				if( text == null )
				{
					return false ;
				}

				return text.supportRichText ;
			}
			set
			{
				Text text = CText ;
				if( text == null )
				{
					return ;
				}

				if( text.supportRichText != value )
				{
					text.supportRichText = value ;

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
				Text text = CText ;
				if( text == null )
				{
					return TextAnchor.MiddleCenter ;
				}

				return text.alignment ;
			}
			set
			{
				Text text = CText ;
				if( text == null )
				{
					return ;
				}

				if( text.alignment != value )
				{
					text.alignment = value ;

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
				Text text = CText ;
				if( text == null )
				{
					return false ;
				}

				return text.resizeTextForBestFit ;
			}
			set
			{
				Text text = CText ;
				if( text == null )
				{
					return ;
				}

				if( text.resizeTextForBestFit != value )
				{
					text.resizeTextForBestFit = value ;

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
				Text text = CText ;
				if( text == null )
				{
					return HorizontalWrapMode.Overflow ;
				}

				return text.horizontalOverflow ;
			}
			set
			{
				Text text = CText ;
				if( text == null )
				{
					return ;
				}

				if( text.horizontalOverflow != value )
				{
					text.horizontalOverflow = value ;

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
				Text text = CText ;
				if( text == null )
				{
					return VerticalWrapMode.Overflow ;
				}

				return text.verticalOverflow ;
			}
			set
			{
				Text text = CText ;
				if( text == null )
				{
					return ;
				}

				if( text.verticalOverflow != value )
				{
					text.verticalOverflow = value ;

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
				Text text = CText ;
				if( text == null )
				{
					return 1.0f ;
				}

				return text.lineSpacing ;
			}
			set
			{
				Text text = CText ;
				if( text == null )
				{
					return ;
				}

				if( text.lineSpacing != value )
				{
					text.lineSpacing = value ;

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
				Text text = CText ;
				if( text == null )
				{
					return false ;
				}

				return text.raycastTarget ;
			}
			set
			{
				Text text = CText ;
				if( text == null )
				{
					return ;
				}

				text.raycastTarget = value ;
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
		private void ToLargeForText()
		{
			Text text = CText ;
			if( text == null )
			{
				return ;
			}

			string s = text.text ;

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
				text.text = new string( code ) ;
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

		//--------------------------------------------------
		
		// ローカライズをリクエスト中か
		private bool m_LocalizeRequest ;

		//--------------------------------------------------

		// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		override protected void OnBuild( string option = "" )
		{
			Text text = CText ;
			if( text == null )
			{
				text = gameObject.AddComponent<Text>() ;
			}
			if( text == null )
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

			text.color = defaultTextColor ;

			if( defaultFont == null )
			{
				text.font = Resources.GetBuiltinResource( typeof( Font ), "Arial.ttf" ) as Font ;
			}
			else
			{
				text.font = defaultFont ;
			}

			if( defaultFontSize <= 0 )
			{
				text.fontSize = 32 ;
			}
			else
			{
				text.fontSize = defaultFontSize ;
			}

			text.alignment = TextAnchor.MiddleLeft ;

			text.horizontalOverflow = HorizontalWrapMode.Overflow ;
			text.verticalOverflow   = VerticalWrapMode.Overflow ;
			
			ResetRectTransform() ;

			//----------------------------------

			if( IsCanvasOverlay == true )
			{
				text.material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}

			text.raycastTarget = false ;
		}

		protected override void OnStart()
		{
			base.OnStart() ;

			if( Application.isPlaying == true )
			{
				Text text = CText ;

				if( text != null )
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
						ToLargeForText() ;
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
			Text = text ;	// 内部で必要に応じて全角化とリサイズが行われている
			m_LocalizeRequest = false ;	// ローカライズが行われた
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

		override protected void OnLateUpdate()
		{
			if( m_AutoSizeFitting == true )
			{
				Resize() ;
			}
		}

		protected void Resize()
		{
			Text t = CText ;
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

