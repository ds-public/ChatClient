using System ;
using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;
using UnityEngine.UI ;

using TMPro ;

namespace uGUIHelper
{
	[RequireComponent( typeof( TextMeshProUGUI ) ) ]

	/// <summary>
	/// uGUI:Text クラスの機能拡張コンポーネントクラス
	/// </summary>
	public class UITextMesh : UIView
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
		/// RectTransform のサイズを自動的に文字列のサイズに合わせるかどうか
		/// </summary>
		[SerializeField][HideInInspector]
		protected float m_AutoCharacterSpacing = 0 ;

		public float AutoCharacterSpacing
		{
			get
			{
				return m_AutoCharacterSpacing ;
			}
			set
			{
				if( m_AutoCharacterSpacing != value )
				{
					m_AutoCharacterSpacing  = value ;
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
				TextMeshProUGUI textMesh = CTextMesh ;
				if( textMesh == null )
				{
					return 0 ;
				}
				if( string.IsNullOrEmpty( textMesh.text ) == true )
				{
					return 0 ;
				}

				return textMesh.text.Length ;
			}
		}

		/// <summary>
		/// 実際に表示されている文字数
		/// </summary>
		public int LengthOfDisplay
		{
			get
			{
				TextMeshProUGUI textMesh = CTextMesh ;

				if( textMesh == null )
				{
					return 0 ;
				}

				TMP_TextInfo textInfo = textMesh.textInfo ;
				if( textInfo == null || textInfo.characterCount == 0 )
				{
					// 文字が存在しない
					return  0 ;
				}

				return textInfo.characterCount ;
			}
		}

		/// <summary>
		/// 文字列自体のサイズ
		/// </summary>
		public Vector2 TextSize
		{
			get
			{
				TextMeshProUGUI textMesh = CTextMesh ;
				if( textMesh == null )
				{
					return Vector2.zero ;
				}

				string text ;

				if( textMesh.text != null && textMesh.text.Length >  0 && textMesh.preferredHeight == 0 )
				{
					// インスタンス複製などで preferredHeight の値がおかしくなっている
					text = textMesh.text ;
					textMesh.text = string.Empty ;	// 一度文字列をリセットする
					textMesh.text = text ;
				}

				return new Vector2( textMesh.preferredWidth, textMesh.preferredHeight ) ;
			}
		}

		/// <summary>
		/// フォントサイズ(ショートカット)
		/// </summary>
		public int FontSize
		{
			get
			{
				TextMeshProUGUI textMesh = CTextMesh ;
				if( textMesh == null )
				{
					return 0 ;
				}

				return ( int )textMesh.fontSize ;
			}
			set
			{
				TextMeshProUGUI textMesh = CTextMesh ;
				if( textMesh == null )
				{
					return ;
				}

				if( textMesh.fontSize != value )
				{
					textMesh.fontSize = value ;

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
				TextMeshProUGUI textMesh = CTextMesh ;
				if( textMesh == null )
				{
					return null ;
				}

				return textMesh.text ;
			}
			set
			{
				TextMeshProUGUI textMesh = CTextMesh ;
				if( textMesh == null )
				{
					return ;
				}

				string text ;

				if( textMesh.text != null && textMesh.text.Length >  0 && textMesh.preferredHeight == 0 )
				{
					// インスタンス複製などで preferredHeight の値がおかしくなっている
					text = textMesh.text ;
					textMesh.text = string.Empty ;	// 一度文字列をリセットする
					textMesh.text = text ;
				}

				( text, _ ) = ParseWait( value ) ;

				if( textMesh.text != text )
				{
					textMesh.text	= text ;

					// 実行時のみ全角化する
					if( m_Zenkaku == true )
					{
						ToLargeForTextMesh() ;
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
		/// テキスト(ショートカット)
		/// </summary>
		public string ParsedText
		{
			get
			{
				TextMeshProUGUI textMesh = CTextMesh ;
				if( textMesh == null )
				{
					return null ;
				}

				// パースため強制リフレッシュ
				textMesh.ForceMeshUpdate() ;

				return textMesh.GetParsedText() ;
			}
		}
		
		/// <summary>
		/// フォント(ショートカット)
		/// </summary>
		public TMP_FontAsset Font
		{
			get
			{
				TextMeshProUGUI textMesh = CTextMesh ;
				if( textMesh == null )
				{
					return null ;
				}

				return textMesh.font ;
			}
			set
			{
				TextMeshProUGUI textMesh = CTextMesh ;
				if( textMesh == null )
				{
					return ;
				}

				if( textMesh.font != value )
				{
					textMesh.font = value ;

					if( m_AutoSizeFitting == true )
					{
						Resize() ;
					}
				}
			}
		}

		/// <summary>
		/// マテリアル(ショートカット)
		/// </summary>
		public Material Material
		{
			get
			{
				TextMeshProUGUI textMesh = CTextMesh ;
				if( textMesh == null )
				{
					return null ;
				}

				return textMesh.fontMaterial ;
			}
			set
			{
				TextMeshProUGUI textMesh = CTextMesh ;
				if( textMesh == null )
				{
					return ;
				}

				if( textMesh.fontMaterial != value )
				{
					textMesh.fontMaterial = value ;
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
				TextMeshProUGUI textMesh = CTextMesh ;
				if( textMesh == null )
				{
					return Color.white ;
				}

				return textMesh.color ;
			}
			set
			{
				TextMeshProUGUI textMesh = CTextMesh ;
				if( textMesh == null )
				{
					return ;
				}

				if( textMesh.color.r != value.r || textMesh.color.g != value.g || textMesh.color.b != value.b || textMesh.color.a != value.a )
				{
					textMesh.color = value ;
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
		/// フォントスタイル(ショートカット)
		/// </summary>
		public FontStyles FontStyle
		{
			get
			{
				TextMeshProUGUI textMesh = CTextMesh ;
				if( textMesh == null )
				{
					return FontStyles.Normal ;
				}

				return textMesh.fontStyle ;
			}
			set
			{
				TextMeshProUGUI textMesh = CTextMesh ;
				if( textMesh == null )
				{
					return ;
				}

				if( textMesh.fontStyle != value )
				{
					textMesh.fontStyle = value ;

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
				TextMeshProUGUI textMesh = CTextMesh ;
				if( textMesh == null )
				{
					return false ;
				}

				return textMesh.richText ;
			}
			set
			{
				TextMeshProUGUI textMesh = CTextMesh ;
				if( textMesh == null )
				{
					return ;
				}

				if( textMesh.richText != value )
				{
					textMesh.richText = value ;

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
		public TextAlignmentOptions Alignment
		{
			get
			{
				TextMeshProUGUI textMesh = CTextMesh ;
				if( textMesh == null )
				{
					return TextAlignmentOptions.Center ;
				}

				return textMesh.alignment ;
			}
			set
			{
				TextMeshProUGUI textMesh = CTextMesh ;
				if( textMesh == null )
				{
					return ;
				}

				if( textMesh.alignment != value )
				{
					textMesh.alignment = value ;

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
				TextMeshProUGUI textMesh = CTextMesh ;
				if( textMesh == null )
				{
					return false ;
				}

				return textMesh.enableAutoSizing ;
			}
			set
			{
				TextMeshProUGUI textMesh = CTextMesh ;
				if( textMesh == null )
				{
					return ;
				}

				if( textMesh.enableAutoSizing != value )
				{
					textMesh.enableAutoSizing = value ;

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
		public  bool HorizontalOverflow
		{
			get
			{
				TextMeshProUGUI textMesh = CTextMesh ;
				if( textMesh == null )
				{
					return false ;
				}

				return ( ! textMesh.enableWordWrapping ) ;
			}
			set
			{
				TextMeshProUGUI textMesh = CTextMesh ;
				if( textMesh == null )
				{
					return ;
				}

				if( textMesh.enableWordWrapping != ( ! value ) )
				{
					textMesh.enableWordWrapping = ( ! value ) ;

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
		public  TextOverflowModes VerticalOverflow
		{
			get
			{
				TextMeshProUGUI textMesh = CTextMesh ;
				if( textMesh == null )
				{
					return TextOverflowModes.Overflow ;
				}

				return textMesh.overflowMode ;
			}
			set
			{
				TextMeshProUGUI textMesh = CTextMesh ;
				if( textMesh == null )
				{
					return ;
				}

				if( textMesh.overflowMode != value )
				{
					textMesh.overflowMode = value ;

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
				TextMeshProUGUI textMesh = CTextMesh ;
				if( textMesh == null )
				{
					return 1.0f ;
				}

				return textMesh.lineSpacing ;
			}
			set
			{
				TextMeshProUGUI textMesh = CTextMesh ;
				if( textMesh == null )
				{
					return ;
				}

				if( textMesh.lineSpacing != value )
				{
					textMesh.lineSpacing = value ;

					if( m_AutoSizeFitting == true )
					{
						Resize() ;
					}

					// 加工用に頂点情報をバックアップする
//					SetTextInfo() ;
				}
			}
		}

		/// <summary>
		/// ラッピング(ショートカット)
		/// </summary>
		public bool EnableWordWrapping
		{
			get
			{
				TextMeshProUGUI textMesh = CTextMesh ;
				if( textMesh == null )
				{
					return false ;
				}

				return textMesh.enableWordWrapping ;
			}
			set
			{
				TextMeshProUGUI textMesh = CTextMesh ;
				if( textMesh == null )
				{
					return ;
				}

				textMesh.enableWordWrapping = value ;
			}
		}

		/// <summary>
		/// レイキャストターゲット(ショートカット)
		/// </summary>
		override public bool RaycastTarget
		{
			get
			{
				TextMeshProUGUI textMesh = CTextMesh ;
				if( textMesh == null )
				{
					return false ;
				}

				return textMesh.raycastTarget;
			}
			set
			{
				TextMeshProUGUI textMesh = CTextMesh ;
				if( textMesh == null )
				{
					return;
				}

				textMesh.raycastTarget = value ;
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
		private void ToLargeForTextMesh()
		{
			TextMeshProUGUI textMesh = CTextMesh ;
			if( textMesh == null )
			{
				return ;
			}

			string s = textMesh.text ;

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
				textMesh.text = new string( code ) ;
				if( m_AutoSizeFitting == true )
				{
					Resize() ;
				}
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 装飾の一部を変更するか
		/// </summary>
		public bool	IsCustomized = false ;

		/// <summary>
		/// アウトラインの色(実行時にのみ反映)
		/// </summary>
		public Color	OutlineColor = Color.black ;

		private Material	m_BasisMaterial = null ;
		private Material	m_CloneMaterial = null ;

		/// <summary>
		/// グラデーションカラー(ショートカット)
		/// </summary>
		public void SetGradientColor( params uint[] code )
		{
			if( code == null || code.Length == 0 )
			{
				return ;
			}

			int i, l = code.Length ;

			Color[] color = new Color[ l ] ;
			uint c ;
			byte r, g, b ,a ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				c = code[ i ] ;

				r = ( byte )( ( c >> 16 ) & 0xFF ) ;
				g = ( byte )( ( c >>  8 ) & 0xFF ) ;
				b = ( byte )(   c         & 0xFF ) ;
				a = ( byte )( ( c >> 24 ) & 0xFF ) ;

				color[ i ] = new Color32(  r, g, b, a ) ;
			}

			SetGradientColor( color ) ;
		}
		
		/// <summary>
		/// グラデーションカラー(ショートカット)
		/// </summary>
		public void SetGradientColor( params Color[] colors )
		{
			TextMeshProUGUI textMesh = CTextMesh ;
			if( textMesh == null )
			{
				return ;
			}

			if( colors == null || colors.Length == 0 )
			{
				return ;
			}

			int l = colors.Length ;

			if( l == 1 )
			{
				// 単色

				VertexGradient vg =	new VertexGradient()
				{
					topLeft		= colors[ 0 ],
					topRight	= colors[ 0 ],
					bottomLeft	= colors[ 0 ],
					bottomRight	= colors[ 0 ],
				} ;

				textMesh.colorGradient = vg ;
			}
			else
			if( l == 2 || l == 3 )
			{
				// 上下
				VertexGradient vg =	new VertexGradient()
				{
					topLeft		= colors[ 0 ],
					topRight	= colors[ 0 ],
					bottomLeft	= colors[ 1 ],
					bottomRight	= colors[ 1 ],
				} ;

				textMesh.colorGradient = vg ;
			}
			else
			if( l >= 4 )
			{
				// 全体
				VertexGradient vg =	new VertexGradient()
				{
					topLeft		= colors[ 0 ],
					topRight	= colors[ 1 ],
					bottomLeft	= colors[ 2 ],
					bottomRight	= colors[ 3 ],
				} ;

				textMesh.colorGradient = vg ;
			}
		}

		//-----------------------------------------------------------

		// ローカライズをリクエスト中か
		private bool m_LocalizeRequest ;

		//--------------------------------------------------
	
		// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		override protected void OnBuild( string option = "" )
		{
			TextMeshProUGUI textMesh = CTextMesh ;
			if( textMesh == null )
			{
				textMesh = gameObject.AddComponent<TextMeshProUGUI>() ;
			}
			if( textMesh == null )
			{
				// 異常
				return ;
			}

			//----------------------------

			Color	defaultTextColor = Color.white ;

			TMP_FontAsset	defaultFontAsset = null ;
			Material		defaultFontMaterial = null ;
			int				defaultFontSize = 0 ;

#if UNITY_EDITOR

			if( Application.isPlaying == false )
			{
				// メニューから操作した場合のみ自動設定を行う
				DefaultSettings ds = Resources.Load<DefaultSettings>( "uGUIHelper/DefaultSettings" ) ;
				if( ds != null )
				{
					defaultTextColor	= ds.TextColor ;

					if( this.GetType() == typeof( UINumberMesh ) )
					{
						defaultFontAsset	= ds.NumberMesh_FontAsset ;
						defaultFontMaterial	= ds.NumberMesh_FontMaterial ; 
						defaultFontSize		= ds.NumberMesh_FontSize ;
					}
					else
					if( this.GetType() == typeof( UITextMesh ) )
					{
						defaultFontAsset	= ds.TextMesh_FontAsset ;
						defaultFontMaterial	= ds.TextMesh_FontMaterial ; 
						defaultFontSize		= ds.TextMesh_FontSize ;
					}
				}
			}
			
#endif

			textMesh.color = defaultTextColor ;

			if( defaultFontAsset == null )
			{
				textMesh.font = Resources.Load<TMP_FontAsset>( "Fonts & Materials/LiberationSans SDF" ) ;
			}
			else
			{
				textMesh.font = defaultFontAsset ;
			}

			if( defaultFontMaterial != null )
			{
				textMesh.fontMaterial = defaultFontMaterial ; 
			}

			if( defaultFontSize <= 0 )
			{
				textMesh.fontSize = 32 ;
			}
			else
			{
				textMesh.fontSize = defaultFontSize ;
			}

			textMesh.alignment = TextAlignmentOptions.TopLeft ;

			textMesh.enableWordWrapping = false ;
			textMesh.overflowMode = TextOverflowModes.Overflow ;
			
			ResetRectTransform() ;

			//----------------------------------

			textMesh.raycastTarget = false ;

		}

#if false
		protected override void OnAwake()
		{
			base.OnAwake() ;

			if( Application.isPlaying == true )
			{
				TextMeshProUGUI textMesh = CTextMesh ;

				if( textMesh != null )
				{
					textMesh.RegisterDirtyVerticesCallback( OnDirtyVertices ) ;
				}
			}
		}
#endif


		override protected void OnStart()
		{
			base.OnStart() ;

			if( Application.isPlaying == true )
			{
				TextMeshProUGUI textMesh = CTextMesh ;

				if( textMesh != null )
				{
					if( IsCustomized == true )
					{
						if( textMesh.material != null )
						{
							m_BasisMaterial = textMesh.fontMaterial ;
							m_CloneMaterial = GameObject.Instantiate<Material>( textMesh.fontMaterial ) ;
							textMesh.fontMaterial = m_CloneMaterial ;

							UpdateCustom( textMesh ) ;
						}
					}

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
						ToLargeForTextMesh() ;
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

		protected override void OnUpdate()
		{
			base.OnUpdate() ;

			if( Application.isPlaying == true )
			{
				TextMeshProUGUI textMesh = CTextMesh ;
				if( textMesh != null )
				{
					if( IsCustomized == true )
					{
						if( textMesh.material != null )
						{
							UpdateCustom( textMesh ) ;
						}
					}
				}
			}
		}

		private void UpdateCustom( TextMeshProUGUI textMesh )
		{
			if( m_CloneMaterial != null )
			{
				if( m_CloneMaterial.HasProperty( "_OutlineColor" ) == true )
				{
					m_CloneMaterial.SetColor(	"_OutlineColor",	OutlineColor ) ;

					textMesh.SetMaterialDirty() ;
				}
			}
		}

		override protected void OnDestroy()
		{
			base.OnDestroy() ;

			if( m_CloneMaterial != null )
			{
				TextMeshProUGUI textMesh = CTextMesh ;
				textMesh.fontMaterial = m_BasisMaterial ;

				DestroyImmediate( m_CloneMaterial ) ;
				m_CloneMaterial = null ;

				m_BasisMaterial = null ;
			}

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
			else
			if( m_AutoCharacterSpacing >  0 )
			{
				int fontSize = FontSize ;
				float width = Width ;
				string text = Text ;
				if( string.IsNullOrEmpty( text ) == true )
				{
					CTextMesh.characterSpacing = 0 ;
				}
				else
				{
					int length = text.Length ;
					if( length <= 1 )
					{
						CTextMesh.characterSpacing = 0 ;
					}
					else
					{
						float space = width - ( fontSize * length ) ;
						if( space <= 0 )
						{
							CTextMesh.characterSpacing = 0 ;
						}
						else
						{
							CTextMesh.characterSpacing = ( space / Mathf.Pow( length - 1, 1.5f ) ) * m_AutoCharacterSpacing ;
						}
					}
				}
			}
		}

		protected void Resize()
		{
			TextMeshProUGUI t = CTextMesh ;
			RectTransform r = GetRectTransform() ;
			if( r != null && t != null )
			{
				Vector2 size = r.sizeDelta ;

				if( r.anchorMin.x == r.anchorMax.x )
				{
					if( t.enableWordWrapping == false )
					{
						size.x = t.preferredWidth ;
					}
				}

				if( r.anchorMin.y == r.anchorMax.y )
				{
					if( t.overflowMode == TextOverflowModes.Overflow )
					{
						size.y = t.preferredHeight ;
					}
				}

				r.sizeDelta = size ;
			}
		}

		//---------------------------------------------------------------------------

		// ルビフリ方法
		// http://baba-s.hatenablog.com/entry/2019/01/10/122500

		/// <summary>
		/// 文字単位で色を設定する
		/// </summary>
		/// <param name="colors"></param>
		public bool SetColors( Color32[] colors, int offset = 0, int length = 0 )
		{
			TextMeshProUGUI textMesh = CTextMesh ;

			if( textMesh == null || colors == null || colors.Length == 0 )
			{
				return false ;
			}

			textMesh.ForceMeshUpdate() ;

			//----------------------------------------------------------

			TMP_TextInfo textInfo = textMesh.textInfo ;
			if( textInfo == null || textInfo.characterCount == 0 )
			{
				// 文字が存在しない
				return  true ;
			}

			//--------------

			if( length <= 0 )
			{
				length  = colors.Length ;
			}

			if( offset <  0 )
			{
				length += offset ;
				offset  = 0 ;
			}

			if( length <= 0 || offset >= colors.Length || offset >= textInfo.characterCount )
			{
				// 結果として変化は無い
				return true ;
			}

			if( ( offset + length ) >  colors.Length )
			{
				length = colors.Length - offset ;
			}

			if( ( offset + length ) >  textInfo.characterCount )
			{
				length = textInfo.characterCount - offset ;
			}

			//----------------------------------------------------------

			int vertexIndex ;
			int materialIndex ;

			Color32[] baseColors ;

			Color32 replaceColor ;

			int i, j, p ;

			for( i  = 0 ; i <  length ; i ++ )
			{
				p = offset + i ;

				if( textInfo.characterInfo[ p ].isVisible == true )
				{
					if( textInfo.characterInfo[ p ].isVisible == true )
					{
						vertexIndex		= textInfo.characterInfo[ p ].vertexIndex ;

						materialIndex	= textInfo.characterInfo[ p ].materialReferenceIndex ;

						baseColors		= textInfo.meshInfo[ materialIndex ].colors32 ;

						replaceColor = new Color32
						(
							colors[ p ].r,
							colors[ p ].g,
							colors[ p ].b,
							colors[ p ].a
						) ;

						for( j  = 0 ; j <  4 ; j ++ )
						{
							baseColors[ vertexIndex + j ] = replaceColor ;
						}
					}
				}
			}

			//----------------------------------------------------------
			
			textMesh.UpdateVertexData( TMP_VertexDataUpdateFlags.Colors32 ) ;

			return true ;
		}

		/// <summary>
		/// 文字単位で色を設定する
		/// </summary>
		/// <param name="colors"></param>
		public bool SetColors( Color[] colors )
		{
			if( colors == null || colors.Length == 0 )
			{
				return false ;
			}

			int i, l = colors.Length ;
			Color32[] colors32 = new Color32[ l ] ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				colors32[ i ] = colors[ i ] ;
			}

			return SetColors( colors32 ) ;
		}

		//---------------------------------------------------------------------------
		
		/// <summary>
		/// 文字の表示状態を変更する情報体
		/// </summary>
		public class Modifier
		{
			public Vector2	position = Vector2.zero ;
			public float	rotation = 0 ;
			public Vector2	scale = Vector2.one ;

			public float	gamma = 1 ;
			public float	alpha = 1 ;
		}

		/// <summary>
		/// 頂点を変化させる
		/// </summary>
		/// <param name="modifiers"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		public bool SetModifiers( Modifier[] modifiers, int offset = 0, int length = 0 )
		{
			TextMeshProUGUI textMesh = CTextMesh ;

			if( textMesh == null || modifiers == null || modifiers.Length == 0 )
			{
				return false ;
			}

			//----------------------------------------------------------

			TMP_TextInfo textInfo = textMesh.textInfo ;

			if( textInfo == null || textInfo.characterCount == 0 )
			{
				// 文字が存在しない
				return  true ;
			}
			
			//--------------

			if( length <= 0 )
			{
				length  = modifiers.Length ;
			}

			if( offset <  0 )
			{
				length += offset ;
				offset  = 0 ;
			}

			if( length <= 0 || offset >= modifiers.Length || offset >= textInfo.characterCount )
			{
				// 結果として変化は無い
				return true ;
			}

			if( ( offset + length ) >  modifiers.Length )
			{
				length = modifiers.Length - offset ;
			}

			if( ( offset + length ) >  textInfo.characterCount )
			{
				length = textInfo.characterCount - offset ;
			}

			//----------------------------------------------------------

			int vertexIndex ;
			int materialIndex ;

			Vector3[] baseVertices ;
			Color32[] baseColors ;

			Modifier modifier ;

			Vector3[]	replaceVertices ;
			Vector2		center ;

			Color32		replaceColor ;

			float dx, dy, vx, vy ;
			float cv, sv ;
			
			int i, j, p ;

			for( i  = 0 ; i <  length ; i ++ )
			{
				p = offset + i ;

				if( textInfo.characterInfo[ p ].isVisible == true )
				{
					modifier = modifiers[ p ] ;

					if( modifier != null )
					{
						vertexIndex		= textInfo.characterInfo[ p ].vertexIndex ;

						materialIndex	= textInfo.characterInfo[ p ].materialReferenceIndex ;

						baseVertices	= textInfo.meshInfo[ materialIndex ].vertices ;
						baseColors		= textInfo.meshInfo[ materialIndex ].colors32 ;

						// Vertices
						replaceVertices = new Vector3[ 4 ] ;
						center = Vector2.zero ;
						for( j  = 0 ; j <  4 ; j ++ )
						{
							// Vertex
							replaceVertices[ j ] = new Vector3
							(
								baseVertices[ vertexIndex + j ].x,
								baseVertices[ vertexIndex + j ].y,
								baseVertices[ vertexIndex + j ].z
							) ;

							center.x += replaceVertices[ j ].x ;
							center.y += replaceVertices[ j ].y ;
						}

						center.x /= 4f ; 
						center.y /= 4f ;

						// 回転→拡縮→移動
						cv = Mathf.Cos( 2.0f * Mathf.PI * modifier.rotation / 360f ) ;
						sv = Mathf.Sin( 2.0f * Mathf.PI * modifier.rotation / 360f ) ;
						
						for( j  = 0 ; j <  4 ; j ++ )
						{
							// 回転
							dx = replaceVertices[ j ].x - center.x ;
							dy = replaceVertices[ j ].y - center.y ;

							vx = ( dx * cv ) - ( dy * sv ) ;
							vy = ( dx * sv ) + ( dy * cv ) ;

							// 拡縮
							vx *= modifier.scale.x ;
							vy *= modifier.scale.y ;

							// 移動
							vx += modifier.position.x ;
							vy += modifier.position.y ;

							replaceVertices[ j ].x = vx + center.x ;
							replaceVertices[ j ].y = vy + center.y ;
						}

						for( j  = 0 ; j <  4 ; j ++ )
						{
							baseVertices[ vertexIndex + j ] = replaceVertices[ j ] ;
						}

						// Color
						for( j  = 0 ; j <  4 ; j ++ )
						{
							// Color
							replaceColor = new Color32
							(
								baseColors[ vertexIndex + j ].r,
								baseColors[ vertexIndex + j ].g,
								baseColors[ vertexIndex + j ].b,
								baseColors[ vertexIndex + j ].a
							) ;

							if( modifier.gamma != 1 )
							{
								replaceColor.r = ( byte )Math.Round( ( float )replaceColor.r * modifier.gamma ) ;
								replaceColor.g = ( byte )Math.Round( ( float )replaceColor.g * modifier.gamma ) ;
								replaceColor.b = ( byte )Math.Round( ( float )replaceColor.b * modifier.gamma ) ;
							}

							if( modifier.alpha != 1 )
							{
								replaceColor.a = ( byte )Math.Round( ( float )replaceColor.a * modifier.alpha ) ;
							}

							baseColors[ vertexIndex + j ] = replaceColor ;
						}
					}
				}
			}

			textMesh.UpdateVertexData( TMP_VertexDataUpdateFlags.Vertices | TMP_VertexDataUpdateFlags.Colors32 ) ;

			//----------------------------------------------------------

			return true ;
		}

		//---------------------------------------------------------------------------

		// 文字表示演出の実行中かどうか
		private bool	m_IsPlaying ;
		/// <summary>
		/// 文字表示演出の実行中かどうか
		/// </summary>
		public  bool	IsPlaying{ get{ return m_IsPlaying ; } }

		// m_Enabled という名前は MonoBehaviour で定義されているので使ってはいけない
		private bool	m_EnabledFlag ;

		// 演出の強制完了
		private bool	m_Finishing ;


		/// <summary>
		/// １文字ずつ表示する
		/// </summary>
		/// <param name="message"></param>
		/// <param name="duration"></param>
		/// <returns></returns>
		public UIView.AsyncState Play( string message, float duration = 0.1f, int fadeWidth = 2, Func<bool> onExecuteFinish = null, Action onFinished = null )
		{
			TextMeshProUGUI textMesh = CTextMesh ;
			UIView.AsyncState state ;

			if( textMesh == null || string.IsNullOrEmpty( message ) == true )
			{
				state = new AsyncState( this )
				{
					IsDone = true
				} ;
				return state ;
			}

			if( gameObject.activeSelf == false )
			{
				gameObject.SetActive( true ) ;
			}

			if( gameObject.activeInHierarchy == false )
			{
				// 親以上がアクティブになっていないので再生できない
				Debug.LogWarning( "Parent is not active : " + name ) ;
				return null ;
			}

			state = new AsyncState( this ) ;
			StartCoroutine( Play_Private( message, duration, fadeWidth, onExecuteFinish, onFinished, state ) ) ;
			return state ;
		}

		private IEnumerator Play_Private( string message, float duration, int fadeWidth, Func<bool> onExecuteFinish, Action onFinished, UIView.AsyncState state )
		{
			TextMeshProUGUI textMesh = CTextMesh ;

			m_IsPlaying = true ;
			m_EnabledFlag = textMesh.enabled ;

			m_Finishing = false ;

			//----------------------------------------------------------

			// コンポーネントの実行順によって１フレーム目に色反映が行われない事があるので１フレーム目は強制的に非表示にする
			textMesh.enabled = false ;

			AttributeData[] attributes ;
			( message, attributes ) = ParseWait( message ) ;

			// 文字列を設定する(設定したフレームではどう頑張ってもメッシュの操作が出来ない＝表示に反映されない)
			textMesh.text = message ;

			if( attributes != null && attributes.Length >  0 )
			{
				// 文字単位のウェイト指定があるので１文字単位で表示する
				fadeWidth = 0 ;
			}

//			yield return null ;						// レンダリング前に再開
			yield return new WaitForEndOfFrame() ;	// レンダリング後に再開

			textMesh.enabled = true ;

			//----------------------------------------------------------

			// 実際に表示される文字列の長さを取得するために必要(１回目の強制リフレッシュ)
			textMesh.ForceMeshUpdate() ;

			string text = textMesh.GetParsedText() ;

			int i, j, l ;
			float t, f ;

			l = text.Length ;

			// 全文字を非表示にする
			Modifier[] m = new Modifier[ l ] ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				m[ i ] = new Modifier()
				{
					alpha = 0
				} ;
			}

			SetModifiers( m ) ;

			//----------------------------------------------------------

			if( duration <= 0 )
			{
				duration  = 0.05f ;
			}

			if( fadeWidth <= 0 )
			{
				// アルファフェードインを使わずに１文字ずつ表示する(時間は0.1)
				t = 0 ;
				float d = duration ;
				float w ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( attributes != null && attributes.Length >  0 )
					{
						if( i <  attributes.Length )
						{
							d = attributes[ i ].Time ;
							if( d <  0 )
							{
								d  = duration ;
							}
						}
					}

					if( d >  0 )
					{
						while( t <  d )
						{
							t += ( Time.deltaTime * m_TimeScale ) ;

							if( t <  d )
							{
								m[ i ].alpha = t / d ;
							}
							else
							{
								m[ i ].alpha = 1 ;
							}

							textMesh.ForceMeshUpdate() ;
							SetModifiers( m, i, l - i ) ;

							if( t >= d )
							{
								t -= d ;
								break ;
							}

							if( onExecuteFinish != null )
							{
								if( onExecuteFinish() == true )
								{
									break ;	// 強制終了
								}
							}
							if( m_Finishing == true )
							{
								break ;		// 強制終了
							}

	//						yield return null ;						// レンダリング前に再開
							yield return new WaitForEndOfFrame() ;	// レンダリング後に再開
						}
					}
					else
					{
						// 待たずに表示する
						m[ i ].alpha = 1 ;

						textMesh.ForceMeshUpdate() ;
						SetModifiers( m, i, l - i ) ;
					}

					if( m_Finishing == true )
					{
						break ;		// 強制終了
					}

					// ウェイトがあれば処理する
					if( attributes != null && attributes.Length >  0 )
					{
						if( i <  attributes.Length )
						{
							w = attributes[ i ].Wait ;
							if( w >  0 )
							{
								while( t <  w )
								{
									t += ( Time.deltaTime * m_TimeScale ) ;

									if( t >= w )
									{
										t -= w ;
										break ;
									}

									yield return null ;
								}
							}
						}
					}
				}
			}
			else
			{
				// アルファフェードインを使い１文字ずつ表示する

				float wait = duration * fadeWidth ;

				t = 0 ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					while( t <  wait )
					{
						t += ( Time.deltaTime * m_TimeScale ) ;

						for( j  = 0 ; j <  fadeWidth ; j ++ )
						{
							f = t - ( duration * j ) ;

							if( f >= wait )
							{
								f  = wait ;
							}
							if( f <  0 )
							{
								f  = 0 ;
							}

							if( ( ( i + j ) <  l ) && f >  0 )
							{
								m[ i + j ].alpha = ( f / wait ) ;
							}
						}
	
						textMesh.ForceMeshUpdate() ;
						SetModifiers( m, i, l - i ) ;

						if( t >= wait )
						{
							t -= duration ;

							break ;
						}

						if( onExecuteFinish != null )
						{
							if( onExecuteFinish() == true )
							{
								break ;	// 強制終了
							}
						}
						if( m_Finishing == true )
						{
							break ;		// 強制終了
						}

//						yield return null ;						// レンダリング前に再開
						yield return new WaitForEndOfFrame() ;	// レンダリング後に再開
					}
					if( m_Finishing == true )
					{
						break ;		// 強制終了
					}
				}
			}

			// 終了時の呼び出し
			onFinished?.Invoke() ;

			// 元の状態に戻す(一括表示)
			textMesh.ForceMeshUpdate() ;

			m_IsPlaying = false ;
			textMesh.enabled = m_EnabledFlag ;

			m_Finishing = false ;

			// コルーチン終了
			state.IsDone = true ;
		}

		/// <summary>
		/// 表示中の文字のエフェクトを完了させる
		/// </summary>
		/// <returns></returns>
		public bool Finish()
		{
			if( m_IsPlaying == true )
			{
				m_Finishing = true ;
			}

			return m_IsPlaying ;
		}

		/// <summary>
		/// 非アクティブ化された際に呼ばれる
		/// </summary>
		override protected void OnDisable()
		{
			base.OnDisable() ;

			TextMeshProUGUI textMesh = CTextMesh ;
			if( textMesh != null )
			{
				if( m_IsPlaying == true )
				{
					textMesh.enabled = m_EnabledFlag ;
					m_IsPlaying  = false ;

					m_Finishing = false ;
				}
			}
		}

		//-------------------------------------------------------------------------------------------
		// 文字列からウェイトのタグを分離して分離後の文字列とウェイト配列を作る

		public class CodeData
		{
			public char		Code ;
			public string	ColorValue ;
			public string	SizeValue ;
			public string	TimeValue ;
			public string	WaitValue ;
		}

		public class AttributeData
		{
			public float	Time ;
			public float	Wait ;
		}

		/// <summary>
		/// 指定した半角文字数で分割する
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static ( string, AttributeData[] ) ParseWait( String s )
		{
			if( string.IsNullOrEmpty( s ) == true )
			{
				return ( string.Empty, null ) ;
			}

			s = s.Replace( "\\n", "\n" ) ;

			//----------------------------------------------------------

			// 文字単位にバラす
			List<List<CodeData>>	lineCodes = new List<List<CodeData>>() ;
			List<CodeData> codes ;

			bool isControl ;

			char code ;

			string colorValue	= null ;
			string sizeValue	= null ;
			string timeValue	= null ;
			string timeStore ;
			string waitValue	= null ;

			int i, l = s.Length ;

			int count = 0 ;	// 文字数のカウント
			int totalCount = 0 ;

			bool normalTagExists = false ;
			bool attributeExists = false ;	// ウェイト指定があるかどうか

			codes = new List<CodeData>() ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				code = s[ i ] ;

				isControl = false ;
				if( code == '<' )
				{
					// カラー判定
					if( colorValue == null )
					{
						string v = IsTagStart( s, ref i, false, "<color=", "<c=", "<C=" ) ;
						if( string.IsNullOrEmpty( v ) == false )
						{
							colorValue = v ;
							isControl = true ;

							normalTagExists = true ;
						}
					}
					else
					{
						if( IsTagEnd( s, ref i, "</color>", "</c>", "</C>" ) == true )
						{
							colorValue = null ;
							isControl = true ;

							normalTagExists = true ;
						}
					}

					// サイズ判定
					if( sizeValue == null )
					{
						string v = IsTagStart( s, ref i, false, "<size=", "<s=", "<S=" ) ;
						if( string.IsNullOrEmpty( v ) == false )
						{
							sizeValue = v ;
							isControl = true ;

							normalTagExists = true ;
						}
					}
					else
					{
						if( IsTagEnd( s, ref i, "</size>", "</s>", "</S>" ) == true )
						{
							sizeValue = null ;
							isControl = true ;

							normalTagExists = true ;
						}
					}

					// タイム判定
					timeStore = IsTagStart( s, ref i, true, "<time=", "<t=" ) ;
					if( string.IsNullOrEmpty( timeStore ) == false )
					{
						timeValue = timeStore ;
						isControl = true ;

						attributeExists = true ;
					}

					// ウェイト判定
					waitValue = IsTagStart( s, ref i, true, "<wait=", "<w=" ) ;
					if( string.IsNullOrEmpty( waitValue ) == false )
					{
						isControl = true ;

						attributeExists = true ;
					}
				}
				else
				if( code == '\n' )
				{
					// １行分完了
					lineCodes.Add( codes ) ;

					codes = new List<CodeData>() ;
					count = 0 ;

					isControl = true ;
				}

				if( isControl == false )
				{
					if( ( int )code >= 0x20 )
					{
						// コントロールではない
						codes.Add( new CodeData()
						{
							Code		= code,
							ColorValue	= colorValue,
							SizeValue	= sizeValue,
							TimeValue	= timeValue,
						} ) ;

						// 文カウントアップ
						count ++ ;
						totalCount ++ ;
					}
				}
				else
				{
					if( string.IsNullOrEmpty( waitValue ) == false )
					{
						// ウェイト指定がある(一番最初につは付けられない)
						if( count >  0 )
						{
							codes[ count - 1 ].WaitValue = waitValue ;
						}

						waitValue = null ;
					}
				}
			}

			if( codes.Count >  0 )
			{
				lineCodes.Add( codes ) ;
			}

			//---------------------------------------------------------

			if( attributeExists == false )
			{
				if( normalTagExists == true )
				{
					s = s.Replace( "<c=", "<color=" ) ;
					s = s.Replace( "<C=", "<color=" ) ;
					s = s.Replace( "</c>", "</color>" ) ;
					s = s.Replace( "</C>", "</color>" ) ;

					s = s.Replace( "<s=", "<size=" ) ;
					s = s.Replace( "<S=", "<size=" ) ;
					s = s.Replace( "</s>", "</size>" ) ;
					s = s.Replace( "</s>", "</size>" ) ;
				}

				// タイム・ウェイト無し
				return ( s, null ) ;
			}

			// ウェイト有り
			AttributeData[] attributes = new AttributeData[ totalCount ] ;	// 文字列と同じ分のウェイト情報を格納する配列を生成する
			for( i  = 0 ; i <  totalCount ; i ++ )
			{
				attributes[ i ] = new AttributeData() ;
			}

			//---------------------------------------------------------
			// 再び文字列を生成する

			string text ;
			CodeData cd ;

			//---------------------------------

			text = string.Empty ;

			colorValue	= null ;
			sizeValue	= null ;

			int p = 0 ;

			int hi, hl = lineCodes.Count ;
			for( hi  = 0 ; hi <  hl ; hi ++ )
			{
				codes = lineCodes[ hi ] ;

				//------------

				l = codes.Count ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					cd = codes[ i ] ;

					//-------------------------------------------------------

					// サイズ終了
					if( cd.SizeValue != sizeValue )
					{
						if( sizeValue != null )
						{
							text += "</size>" ;
						}
					}

					// カラー終了
					if( cd.ColorValue != colorValue )
					{
						if( colorValue != null )
						{
							text += "</color>" ;
						}
					}

					//-------------------------------

					// カラー開始
					if( cd.ColorValue != colorValue )
					{
						if( cd.ColorValue != null )
						{
							text += "<color=" + cd.ColorValue + ">" ;
						}
					}

					// サイズ開始
					if( cd.SizeValue != sizeValue )
					{
						if( cd.SizeValue != null )
						{
							text += "<size=" + cd.SizeValue + ">" ;
						}
					}

					//-------------------------------------------------------

					// カラー更新
					colorValue	= cd.ColorValue ;

					// サイズ更新
					sizeValue	= cd.SizeValue ;

					// タイム更新
					timeValue	= cd.TimeValue ;

					// ウェイト追加
					waitValue	= cd.WaitValue ;

					//-------------------------------------------------------

					// 文字追加
					text += cd.Code ;

					// タイム
					if( string.IsNullOrEmpty( timeValue ) == true )
					{
						// デフォルト
						attributes[ p ].Time = -1 ;
					}
					else
					{
						// 指定
						if( float.TryParse( timeValue, out float t ) == false )
						{
							t = -1 ;
						}

						attributes[ p ].Time =  t ;
					}

					// ウェイト
					if( string.IsNullOrEmpty( waitValue ) == true )
					{
						// デフォルト
						attributes[ p ].Wait =  0 ;
					}
					else
					{
						// 指定
						if( float.TryParse( waitValue, out float w ) == false )
						{
							w =  0 ;
						}

						attributes[ p ].Wait =  w ;
					}

					p ++ ;
				}

				//--------------------------------
				// 改行まで終了

				if( hi <  ( hl - 1 ) )
				{
					text += "\n" ;	// 改行追加
				}
			}

			// サイズ終了
			if( sizeValue != null )
			{
				text += "</size>" ;
			}

			// カラー終了
			if( colorValue != null )
			{
				text += "</color>" ;
			}

			//----------------------------------------------------------

			return ( text, attributes ) ;
		}

		// タグかどうか判定し内容を取得する
		private static string IsTagStart( string s, ref int o, bool isClosed, params string[] labels )
		{
			int i, p, m ;
			int l = s.Length ;
			string value ;

			int k, n = labels.Length ;
			string label ;

			for( k  = 0 ; k <  n ; k ++ )
			{
				label = labels[ k ] ;
				m = label.Length ;

				p = o ;
				if( ( p + m ) <  l && s.Substring( p, m ).ToLower() == label )
				{
					// 値を取得する
					p += m ;

					for( i  = p ; i <  l ; i ++ )
					{
						if( s[ i ] == '>' )
						{
							// 決定
							o = i ;
							value = s.Substring( p, i - p ) ;

							if( string.IsNullOrEmpty( value ) == false )
							{
								return value ;
							}
							else
							{
								return string.Empty ;	// 値が空文字
							}
						}
						else
						if( isClosed == true )
						{
							if( s[ i ] == '/' && ( ( i + 1 ) <  l ) && s[ i + 1 ] == '>' )
							{
								// 決定
								o = i + 1 ;
								value = s.Substring( p, i - p ) ;

								if( string.IsNullOrEmpty( value ) == false )
								{
									return value ;
								}
								else
								{
									return string.Empty ;	// 値が空文字
								}

							}
						}
					}
				}
			}

			// いずれにも該当しなかった
			return null ;
		}

		// タグかどうか判定し内容を取得する
		private static bool IsTagEnd( string s, ref int o, params string[] labels )
		{
			int m ;
			int l = s.Length ;

			int k, n = labels.Length ;
			string label ;

			for( k  = 0 ; k <  n ; k ++ )
			{
				label = labels[ k ] ;
				m = label.Length ;

				if( ( o + m ) <= l && s.Substring( o, m ).ToLower() == label )
				{
					o += m - 1 ;
					return true ;
				}
			}

			return false ;
		}
	}
}
