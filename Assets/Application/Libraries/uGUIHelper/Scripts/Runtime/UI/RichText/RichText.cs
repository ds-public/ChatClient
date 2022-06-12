using UnityEngine ;
using UnityEngine.UI ;
using System ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	[AddComponentMenu("UI/Text", 10)]
//	public class RichText : MaskableGraphic, ILayoutElement
	public class RichText : MaskableGraphic
	{
		// ＵＩのフォントデータクラス
		[SerializeField] protected RichFontData m_FontData = RichFontData.defaultFontData ;
		
		// オリジナルのテキスト
		[TextArea(  3, 10 )][SerializeField] protected string m_Text = String.Empty ;

		// 頂点データ化されたテキスト
		private RichTextGenerator m_TextCache ;

		static protected Material s_DefaultText = null ;

		// フォントの更新コール禁止フラグ
		[NonSerialized] protected bool m_DisableFontTextureRebuiltCallback = false ;

		// コンストラクタ
		protected RichText()
		{
			useLegacyMeshGeneration = false ;
		}


 		// キャッシュされたテキストジェネレータを返す
		public RichTextGenerator cachedTextGenerator
		{
			get { return m_TextCache ?? ( m_TextCache = ( m_Text.Length != 0 ? new RichTextGenerator( m_Text.Length ) : new RichTextGenerator() ) ) ; }
		}

		// テクスチャを返す
		public override Texture mainTexture
		{
			get
			{
				if( font != null && font.material != null && font.material.mainTexture != null )
				{
					return font.material.mainTexture ;
				}

                if( m_Material != null )
				{
					return m_Material.mainTexture ;
				}
				
				return base.mainTexture ;
			}
		}
		
		//-------------------------------------------------------------------------------------
		
		/// <summary>
		/// フォントのテクスチャに更新があった
		/// </summary>
        public void FontTextureChanged()
        {
			if( this == null )
			{
				// 自身は既に破棄されている
				UntrackText( this ) ;
				return ;
			}

			if( m_DisableFontTextureRebuiltCallback == true )
			{
				// テクスチャの更新によるメッシュの更新は禁止されている
				return ;
			}

			// 頂点情報を更新する
			cachedTextGenerator.Invalidate() ;

			if( IsActive() == false )
			{
				// 自身は非アクティブ状態である
				return ;
			}

			if( CanvasUpdateRegistry.IsRebuildingGraphics() || CanvasUpdateRegistry.IsRebuildingLayout() )
			{
				// メッシュを更新する
                UpdateGeometry() ;
			}
            else
			{
                SetAllDirty() ;
			}
        }

		//-------------------------------------------------------------------------------------
		
		
		/// <summary>
		/// フォントデータ(プロパティ)
		/// </summary>
		public RichFontData fontData
		{
			get
			{
				// 基本的に null は存在しない
				return m_FontData ;
			}
		}
		
		/// <summary>
		/// フォント(プロパティ)
		/// </summary>
        public Font font
        {
			get
			{
				return fontData.font ;
			}
			set
			{
				if( fontData.font == value )
				{
                    return ;
				}

				// 頂点情報を更新する
		        cachedTextGenerator.Invalidate() ;

				UntrackText( this ) ;

				fontData.font = value ;

				TrackText( this ) ;

				SetAllDirty() ;
			}
		}
		
		/// <summary>
		/// テキスト(プロパティ)
		/// </summary>
		public virtual string text
		{
			get
			{
				return m_Text ;
			}
			set
			{
				if( String.IsNullOrEmpty( value ) == true )
                {
					if( String.IsNullOrEmpty( m_Text ) == true )
					{
						return ;
					}
					m_Text = "" ;
					
					// 頂点情報を更新する
			        cachedTextGenerator.Invalidate() ;

					SetVerticesDirty() ;
				}
                else
				if( m_Text != value )
				{
					m_Text = value ;
					
					// 頂点情報を更新する
					cachedTextGenerator.Invalidate() ;

					SetVerticesDirty() ;
					SetLayoutDirty() ;
				}
			}
		}

		/// <summary>
		/// リッチテキスト(プロパティ)
		/// </summary>
		public bool supportRichText
        {
			get
			{
				return fontData.richText ;
			}
			set
			{
				if( fontData.richText == value )
				{
					return ;
				}
				fontData.richText = value ;
				
				// 頂点情報を更新する
		        cachedTextGenerator.Invalidate() ;

				SetVerticesDirty() ;
				SetLayoutDirty() ;
			}
		}

		/// <summary>
		/// ベストフィット(プロパティ)
		/// </summary>
		public bool resizeTextForBestFit
		{
			get
			{
				return fontData.bestFit ;
			}
			set
			{
				if( fontData.bestFit == value )
				{
					return ;
				}
				fontData.bestFit = value ;
				
				// 頂点情報を更新する
		        cachedTextGenerator.Invalidate() ;

				SetVerticesDirty() ;
				SetLayoutDirty() ;
			}
		}
		
		/// <summary>
		/// 最小リサイズ(プロパティ)
		/// </summary>
		public int resizeTextMinSize
		{
			get
			{
				return fontData.minSize ;
			}
			set
			{
				if( fontData.minSize == value )
				{
                    return ;
				}
				fontData.minSize = value ;
				
				// 頂点情報を更新する
		        cachedTextGenerator.Invalidate() ;

				SetVerticesDirty() ;
				SetLayoutDirty() ;
			}
		}
		
		/// <summary>
		/// 最大リサイズ(プロパティ)
		/// </summary>
		public int resizeTextMaxSize
		{
			get
			{
				return fontData.maxSize ;
			}
			set
			{
				if( fontData.maxSize == value )
				{
                    return ;
				}
				fontData.maxSize = value ;
				
				// 頂点情報を更新する
		        cachedTextGenerator.Invalidate() ;

				SetVerticesDirty() ;
				SetLayoutDirty() ;
			}
		}

		/// <summary>
		/// アライメント(プロパティ)
		/// </summary>
		public TextAnchor alignment
		{
			get
			{
				return fontData.alignment ;
			}
			set
			{
				if( fontData.alignment == value )
				{
					return ;
				}
				fontData.alignment = value ;
				
				// 頂点情報を更新する
		        cachedTextGenerator.Invalidate() ;

				SetVerticesDirty() ;
				SetLayoutDirty() ;
			}
		}
		
		/// <summary>
		/// フォントサイズ(プロパティ)
		/// </summary>
		public int fontSize
		{
			get
			{
				return fontData.fontSize ;
			}
			set
			{
				if( fontData.fontSize == value )
				{
					return ;
				}
				fontData.fontSize = value ;
				
				// 頂点情報を更新する
		        cachedTextGenerator.Invalidate() ;

				SetVerticesDirty() ;
				SetLayoutDirty() ;
			}
		}
		
		/// <summary>
		/// 水平方向のオーバーフロー(プロパティ)
		/// </summary>
		public HorizontalWrapMode horizontalOverflow
		{
			get
			{
				return fontData.horizontalOverflow ;
			}
			set
			{
				if( fontData.horizontalOverflow == value )
				{
                    return ;
				}
				fontData.horizontalOverflow = value ;

				// 頂点情報を更新する
		        cachedTextGenerator.Invalidate() ;

				SetVerticesDirty() ;
				SetLayoutDirty() ;
			}
		}
		
		/// <summary>
		/// 垂直方向のオーバーフロー(プロパティ)
		/// </summary>
		public VerticalWrapMode verticalOverflow
		{
			get
			{
				return fontData.verticalOverflow ;
			}
			set
			{
				if( fontData.verticalOverflow == value )
				{
                    return ;
				}
				fontData.verticalOverflow = value ;
				
				// 頂点情報を更新する
		        cachedTextGenerator.Invalidate() ;

				SetVerticesDirty() ;
				SetLayoutDirty() ;
			}
		}
		
		/// <summary>
		/// ラインスペース(プロパティ)
		/// </summary>
		public float lineSpacing
		{
			get
			{
				return fontData.lineSpacing ;
			}
			set
			{
				if( fontData.lineSpacing == value )
				{
					return ;
				}
				fontData.lineSpacing = value ;
				
				// 頂点情報を更新する
		        cachedTextGenerator.Invalidate() ;

				SetVerticesDirty() ;
				SetLayoutDirty() ;
			}
		}
		
		/// <summary>
		/// フォントスタイル(プロパティ)
		/// </summary>
		public FontStyle fontStyle
		{
			get
			{
				return fontData.fontStyle ;
			}
			set
			{
				if( fontData.fontStyle == value )
				{
                    return ;
				}
				fontData.fontStyle = value ;

				// 頂点情報を更新する
		        cachedTextGenerator.Invalidate() ;

				SetVerticesDirty() ;
				SetLayoutDirty() ;
			}
		}
		
		/// <summary>
		/// ピクセルユニット(プロパティ)
		/// </summary>
		public float pixelsPerUnit
		{
			get
			{
				var localCanvas = canvas ;
                if( localCanvas == null )
				{
                    return 1 ;
				}

                if( font == null || font.dynamic == true )
				{
                    return localCanvas.scaleFactor ;
				}
				
				if( fontData.fontSize <= 0 || font.fontSize <= 0 )
				{
                    return 1 ;
				}

				return font.fontSize / ( float )fontData.fontSize ;
			}
		}
		
		//-----------------------------------------------------------

		[SerializeField][HideInInspector]
		protected bool m_ViewControllEnabled = false ;

		/// <summary>
		/// ビューのコントロールの有効状態
		/// </summary>
		public bool viewControllEnabled
		{
			get
			{
				return m_ViewControllEnabled ;
			}
			set
			{
				if( m_ViewControllEnabled == value )
				{
					return ;
				}
				m_ViewControllEnabled  = value ;
					
				// 頂点情報を更新する
		        cachedTextGenerator.Invalidate() ;

				SetVerticesDirty() ;
				SetLayoutDirty() ;	
			}
		}
		
		[SerializeField][HideInInspector]
		protected int m_LengthOfView = -1 ;
		
		/// <summary>
		/// 表示対象文字数
		/// </summary>
		public int lengthOfView
		{
			get
			{
				return m_LengthOfView ;
			}
			set
			{
				if( m_LengthOfView == value )
				{
					return ;
				}
				m_LengthOfView  = value ;
					
				// 頂点情報を更新する
		        cachedTextGenerator.Invalidate() ;

				SetVerticesDirty() ;
				SetLayoutDirty() ;	
			}
		}
		
		[SerializeField][HideInInspector]
		protected int m_StartLineOfView = 0 ;
		
		/// <summary>
		/// 表示開始行
		/// </summary>
		public int startLineOfView
		{
			get
			{
				return m_StartLineOfView ;
			}
			set
			{
				if( m_StartLineOfView == value )
				{
					return ;
				}
				m_StartLineOfView  = value ;
					
				// 頂点情報を更新する
		        cachedTextGenerator.Invalidate() ;

				SetVerticesDirty() ;
				SetLayoutDirty() ;	
			}
		}
		
		[SerializeField][HideInInspector]
		protected int m_EndLineOfView = -1 ;
		
		/// <summary>
		/// 表示終了行
		/// </summary>
		public int endLineOfView
		{
			get
			{
				return m_EndLineOfView ;
			}
			set
			{
				if( m_EndLineOfView == value )
				{
					return ;
				}
				m_EndLineOfView  = value ;
					
				// 頂点情報を更新する
		        cachedTextGenerator.Invalidate() ;

				SetVerticesDirty() ;
				SetLayoutDirty() ;	
			}
		}

		//---------------------------

		[SerializeField][HideInInspector]
		protected int m_StartOffsetOfFade = 0 ;
		
		/// <summary>
		/// フェード開始文字数
		/// </summary>
		public int startOffsetOfFade
		{
			get
			{
				return m_StartOffsetOfFade ;
			}
			set
			{
				if( m_StartOffsetOfFade == value )
				{
					return ;
				}
				m_StartOffsetOfFade  = value ;
					
				// 頂点情報を更新する
		        cachedTextGenerator.Invalidate() ;

				SetVerticesDirty() ;
				SetLayoutDirty() ;	
			}
		}
		
		[SerializeField][HideInInspector]
		protected int m_EndOffsetOfFade = -1 ;
		
		/// <summary>
		/// フェード終了文字数
		/// </summary>
		public int endOffsetOfFade
		{
			get
			{
				return m_EndOffsetOfFade ;
			}
			set
			{
				if( m_EndOffsetOfFade == value )
				{
					return ;
				}
				m_EndOffsetOfFade  = value ;
					
				// 頂点情報を更新する
		        cachedTextGenerator.Invalidate() ;

				SetVerticesDirty() ;
				SetLayoutDirty() ;	
			}
		}
		
		[SerializeField][HideInInspector]
		protected float m_RatioOfFade = 1.0f ;
		
		/// <summary>
		/// 表示対象割合
		/// </summary>
		public float ratioOfFade
		{
			get
			{
				return m_RatioOfFade ;
			}
			set
			{
				if( m_RatioOfFade == value )
				{
					return ;
				}
				m_RatioOfFade  = value ;
					
				// 頂点情報を更新する
		        cachedTextGenerator.Invalidate() ;

				SetVerticesDirty() ;
				SetLayoutDirty() ;	
			}
		}
		
		[SerializeField][HideInInspector]
		protected int m_WidthOfFade = 5 ;
		
		/// <summary>
		/// 透過対象文字数
		/// </summary>
		public int widthOfFade
		{
			get
			{
				return m_WidthOfFade ;
			}
			set
			{
				if( m_WidthOfFade == value )
				{
					return ;
				}
				m_WidthOfFade  = value ;
					
				// 頂点情報を更新する
		        cachedTextGenerator.Invalidate() ;

				SetVerticesDirty() ;
				SetLayoutDirty() ;	
			}
		}

		//-----------------------------------------------------------

		[SerializeField][HideInInspector]
		protected float m_RubySizeScale = 0.5f ;

		/// <summary>
		/// ルビとして表示する文字のフォントサイズに対するスケール
		/// </summary>
		public float rubySizeScale
		{
			get
			{
				return m_RubySizeScale ;
			}
			set
			{
				if( m_RubySizeScale == value )
				{
					return ;
				}
				m_RubySizeScale  = value ;
					
				// 頂点情報を更新する
		        cachedTextGenerator.Invalidate() ;

				SetVerticesDirty() ;
				SetLayoutDirty() ;	
			}
		}

		[SerializeField][HideInInspector]
		protected float m_SupOrSubSizeScale = 0.5f ;

		/// <summary>
		/// 上付きまたは下付きとして表示する文字のフォントサイズに対するスケール
		/// </summary>
		public float supOrSubSizeScale
		{
			get
			{
				return m_SupOrSubSizeScale ;
			}
			set
			{
				if( m_SupOrSubSizeScale == value )
				{
					return ;
				}
				m_SupOrSubSizeScale  = value ;
					
				// 頂点情報を更新する
		        cachedTextGenerator.Invalidate() ;

				SetVerticesDirty() ;
				SetLayoutDirty() ;	
			}
		}

		[SerializeField][HideInInspector]
		protected float m_TopMarginSpacing = 0.5f ;

		/// <summary>
		/// 上側のマージン(フォントサイズに対する比率)
		/// </summary>
		public float topMarginSpacing
		{
			get
			{
				return m_TopMarginSpacing ;
			}
			set
			{
				if( m_TopMarginSpacing == value )
				{
					return ;
				}

				m_TopMarginSpacing = value ;

				// 頂点情報を更新する
		        cachedTextGenerator.Invalidate() ;
					
				SetVerticesDirty() ;
				SetLayoutDirty() ;	
			}
		}

		[SerializeField][HideInInspector]
		protected float m_BottomMarginSpacing = 0.5f ;

		/// <summary>
		/// 下側のマージン(フォントサイズに対する比率)
		/// </summary>
		public float bottomMarginSpacing
		{
			get
			{
				return m_BottomMarginSpacing ;
			}
			set
			{
				if( m_BottomMarginSpacing == value )
				{
					return ;
				}

				m_BottomMarginSpacing = value ;

				// 頂点情報を更新する
		        cachedTextGenerator.Invalidate() ;
					
				SetVerticesDirty() ;
				SetLayoutDirty() ;	
			}
		}

		/// <summary>
		/// ルビが表示されているか
		/// </summary>
		public bool isRubyUsed
		{
			get
			{
		       return cachedTextGenerator.isRubyUsed ;
			}
		}

		//-----------------------------------------------------------------------------------
		
		protected override void OnEnable()
		{
			base.OnEnable() ;

			cachedTextGenerator.Invalidate() ;
			
			TrackText( this ) ;
		}
		
		protected override void OnDisable()
		{
			UntrackText( this ) ;

			base.OnDisable() ;
		}
		
		protected override void UpdateGeometry()
		{
			if( font != null )
			{
				base.UpdateGeometry() ;
			}
		}

#if UNITY_EDITOR
		protected override void Reset()
		{
			font = Resources.GetBuiltinResource<Font>( "Arial.ttf" ) ;
		}
#endif
		
		/// <summary>
		/// テキストジェネレータの設定を取得する
		/// </summary>
		/// <param name="extents"></param>
		/// <returns></returns>
		protected RichTextGenerationSettings GetRichTextGenerationSettings( Vector2 tExtents )
		{
			RichTextGenerationSettings tSettings = new RichTextGenerationSettings() ;
			
			tSettings.generationExtents		= tExtents ;
			tSettings.pivot					= rectTransform.pivot ;

			tSettings.font					= font ;
			tSettings.fontStyle				= fontData.fontStyle ;
			tSettings.lineSpacing			= fontData.lineSpacing ;
			tSettings.richText				= fontData.richText ;
			tSettings.textAnchor			= fontData.alignment ;
			tSettings.horizontalOverflow	= fontData.horizontalOverflow ;
			tSettings.verticalOverflow		= fontData.verticalOverflow ;

			tSettings.resizeTextForBestFit	= fontData.bestFit ;

			tSettings.color					= color ;

			if( font != null )
			{
				tSettings.fontSize			= fontData.fontSize ;
				tSettings.resizeTextMinSize	= fontData.minSize ;
				tSettings.resizeTextMaxSize	= fontData.maxSize ;
			}
			

			tSettings.scaleFactor			= pixelsPerUnit ;
			tSettings.updateBounds			= false ;


			tSettings.viewControllEnabled	= m_ViewControllEnabled ;
			tSettings.lengthOfView			= m_LengthOfView ;
			tSettings.startLineOfView		= m_StartLineOfView ;
			tSettings.endLineOfView			= m_EndLineOfView ;
			tSettings.startOffsetOfFade		= m_StartOffsetOfFade ;
			tSettings.endOffsetOfFade		= m_EndOffsetOfFade ;
			tSettings.ratioOfFade			= m_RatioOfFade ;
			tSettings.widthOfFade			= m_WidthOfFade ;

			tSettings.rubySizeScale			= m_RubySizeScale ;
			tSettings.supOrSubSizeScale		= m_SupOrSubSizeScale ;
			tSettings.topMarginSpacing		= m_TopMarginSpacing ;
			tSettings.bottomMarginSpacing	= m_BottomMarginSpacing ;

			return tSettings ;
		}

		protected override void OnPopulateMesh( VertexHelper toFill )
		{
			if( font == null )
			{
                return ;
			}
			
			// 以下テクスチャの作り直しを禁止する
			m_DisableFontTextureRebuiltCallback = true ;

			//----------------------------------------------------------

			Vector2 tExtents = rectTransform.rect.size ;
			RichTextGenerationSettings tSettings = GetRichTextGenerationSettings( tExtents ) ;
			cachedTextGenerator.Populate( text, tSettings ) ;

			// メッシュ全体の位置を調整する
			toFill.Clear() ;
			
			UIVertex[] vq = new UIVertex[ 4 ] ;

			UIVertex[] va = cachedTextGenerator.verts.ToArray() ;

			if( va != null && va.Length >  0 )
			{
				int i, j, l = va.Length ;
				for( i  = 0 ; i <  l ; i = i + 4 )
				{
					for( j  = 0 ; j <  4 ; j ++ )
					{
						vq[ j ] = va[ i + j ] ;
					}

					toFill.AddUIVertexQuad( vq ) ;
				}
			}
			
			//----------------------------------------------------------

			// 以下テクスチャの作り直しを許可する
			m_DisableFontTextureRebuiltCallback = false ;
        }
		
		//-------------------------------------------------------------------------------------------

		[SerializeField][HideInInspector]
		private   CanvasRenderer	mCanvasRenderer ;
		protected CanvasRenderer	_CanvasRenderer
		{
			get
			{
				if( mCanvasRenderer != null )
				{
					return mCanvasRenderer ;
				}
				mCanvasRenderer = GetComponent<CanvasRenderer>() ;
				return mCanvasRenderer ;
			}
		}

		public new Color color
		{
			get
			{
				return base.color ;
			}
			set
			{
				if( base.color.r != value.r || base.color.g != value.g || base.color.b != value.b || base.color.a != value.a )
				{
					base.color = value ;
					_CanvasRenderer.SetColor( value ) ;
				}
			}
		}

		// マテリアル更新
		protected override void UpdateMaterial()
		{
			base.UpdateMaterial() ;
			
			// マテリアルカラーは白固定(αのみ反映)
			Color tColor = base.color ;

			tColor.r = 1.0f ;
			tColor.g = 1.0f ;
			tColor.b = 1.0f ;

			_CanvasRenderer.SetColor( tColor ) ;

			// テクスチャを更新する
			if( mainTexture != null )
			{
				_CanvasRenderer.SetTexture( mainTexture ) ;
			}
		}

		//-------------------------------------------------------------------------------------------

		public virtual void CalculateLayoutInputHorizontal(){}
		public virtual void CalculateLayoutInputVertical(){}
		
		public virtual float minWidth
		{
			get { return 0 ; }
		}

		/// <summary>
		/// メッシュの横幅
		/// </summary>
		public virtual float preferredWidth
		{
			get
			{
				Vector2 tExtents = rectTransform.rect.size ;
				RichTextGenerationSettings tSettings = GetRichTextGenerationSettings( tExtents ) ;
				return cachedTextGenerator.GetPreferredWidth( text, tSettings ) ;
			}
		}
		
		public virtual float flexibleWidth { get { return -1 ; } }
		
		public virtual float minHeight
		{
			get { return 0 ; }
		}
		
		/// <summary>
		/// 実際の縦幅
		/// </summary>
		public virtual float preferredHeight
		{
			get
            {
				Vector2 tExtents = rectTransform.rect.size ;
				RichTextGenerationSettings tSettings = GetRichTextGenerationSettings( tExtents ) ;
				return cachedTextGenerator.GetPreferredHeight( text, tSettings ) ;
			}
		}
		
		/// <summary>
		/// 最大の縦幅
		/// </summary>
		public virtual float preferredFullHeight
		{
			get
            {
				Vector2 tExtents = rectTransform.rect.size ;
				RichTextGenerationSettings tSettings = GetRichTextGenerationSettings( tExtents ) ;
				return cachedTextGenerator.GetPreferredFullHeight( text, tSettings ) ;
			}
		}
		
		public virtual float flexibleHeight { get { return -1 ; } }

		public virtual int layoutPriority { get { return 0 ; } }


		//-----------------------------------------------------------

		/// <summary>
		/// 表示対象の文字数を返す(読み出し専用)
		/// </summary>
		public int length
		{
			get
			{
				if( cachedTextGenerator == null )
				{
					return 0 ;
				}

				Vector2 tExtents = rectTransform.rect.size ;
				RichTextGenerationSettings tSettings = GetRichTextGenerationSettings( tExtents ) ;
				return cachedTextGenerator.GetLength( text, tSettings ) ;
			}
		}

		/// <summary>
		/// 全行数を取得する
		/// </summary>
		public int line
		{
			get
			{
				if( cachedTextGenerator == null )
				{
					return 0 ;
				}

				Vector2 tExtents = rectTransform.rect.size ;
				RichTextGenerationSettings tSettings = GetRichTextGenerationSettings( tExtents ) ;
				return cachedTextGenerator.GetLine( text, tSettings ) ;
			}
		}

		/// <summary>
		/// 指定したラインの開始時の描画対象となる文字数を取得する
		/// </summary>
		/// <param name="tLine"></param>
		/// <returns></returns>
		public int GetStartOffsetOfLine( int tLine )
		{
			if( cachedTextGenerator == null )
			{
				return -1 ;
			}

			Vector2 tExtents = rectTransform.rect.size ;
			RichTextGenerationSettings tSettings = GetRichTextGenerationSettings( tExtents ) ;
			return cachedTextGenerator.GetStartOffsetOfLine( text, tSettings, tLine ) ;
		}

		/// <summary>
		/// 指定したラインの終了時の描画対象となる文字数を取得する
		/// </summary>
		/// <param name="tLine"></param>
		/// <returns></returns>
		public int GetEndOffsetOfLine( int tLine )
		{
			if( cachedTextGenerator == null )
			{
				return -1 ;
			}

			Vector2 tExtents = rectTransform.rect.size ;
			RichTextGenerationSettings tSettings = GetRichTextGenerationSettings( tExtents ) ;
			return cachedTextGenerator.GetEndOffsetOfLine( text, tSettings, tLine ) ;
		}

		/// <summary>
		/// 最初の表示文字数を取得する
		/// </summary>
		public int startOffsetOfView
		{
			get
			{
				if( m_StartLineOfView <  0 )
				{
					return GetStartOffsetOfLine( 0 ) ;
				}
				else
				{
					return GetStartOffsetOfLine( m_StartLineOfView ) ;
				}
			}
		}

		/// <summary>
		/// 最後の表示文字数を取得する
		/// </summary>
		public int endOffsetOfView
		{
			get
			{
				int tEndOffsetOfView = 0 ;

				if( m_EndLineOfView <  0 )
				{
					tEndOffsetOfView = GetEndOffsetOfLine( line - 1 ) ;
				}
				else
				{
					tEndOffsetOfView = GetEndOffsetOfLine( m_EndLineOfView - 1 ) ;
				}

				if( m_LengthOfView >= 0 )
				{
					if( m_LengthOfView <  tEndOffsetOfView )
					{
						tEndOffsetOfView = m_LengthOfView ;
					}
				}

				return tEndOffsetOfView ;
			}
		}

/*		/// <summary>
		/// カーソルの位置を取得する
		/// </summary>
		public Vector2 cursorPosition
		{
			get
            {
				Vector2 tExtents = rectTransform.rect.size ;
				RichTextGenerationSettings tSettings = GetRichTextGenerationSettings( tExtents ) ;
				return cachedTextGenerator.GetCursorPosition( text, tSettings ) ;
			}

		}*/

		/// <summary>
		/// 指定した文字数のキャレットの座標を指定する
		/// </summary>
		/// <param name="tLength"></param>
		/// <returns></returns>
		public Vector2 GetCaretPosition( int tLength = -1 )
		{
			Vector2 tExtents = rectTransform.rect.size ;
			RichTextGenerationSettings tSettings = GetRichTextGenerationSettings( tExtents ) ;
			return cachedTextGenerator.GetCaretPosition( text, tSettings, tLength, startOffsetOfView, endOffsetOfView ) ;
		}


		/// <summary>
		/// 上のマージンが必要になるか
		/// </summary>
		public bool isNeedTopMargin
		{
			get
            {
				Vector2 tExtents = rectTransform.rect.size ;
				RichTextGenerationSettings tSettings = GetRichTextGenerationSettings( tExtents ) ;
				return cachedTextGenerator.IsNeedTopMargin( text, tSettings ) ;
			}
		}

		/// <summary>
		/// 下のマージンが必要になるか
		/// </summary>
		public bool isNeedBottomMargin
		{
			get
            {
				Vector2 tExtents = rectTransform.rect.size ;
				RichTextGenerationSettings tSettings = GetRichTextGenerationSettings( tExtents ) ;
				return cachedTextGenerator.IsNeedBottomMargin( text, tSettings ) ;
			}
		}

		/// <summary>
		/// 拡張タグ情報
		/// </summary>
		public class ExtraTagEvent
		{
			public int		offset ;	// ウエイトイベントが発生する直前の表示文字数
			public string	tagName ;	// タグ名
			public string	tagValue ;	// タグ値

			public ExtraTagEvent( int tOffset, string tTagName, string tTagValue )
			{
				offset		= tOffset ;
				tagName		= tTagName ;
				tagValue	= tTagValue ;
			}
		}

		/// <summary>
		/// 拡張タグイベント
		/// </summary>
		public RichText.ExtraTagEvent[] extraTagEvent
		{
			get
			{
				Vector2 tExtents = rectTransform.rect.size ;
				RichTextGenerationSettings tSettings = GetRichTextGenerationSettings( tExtents ) ;
				return cachedTextGenerator.GetExtraTagEvent( text, tSettings ) ;
			}
		}
		
		/// <summary>
		/// 拡張タグイベントを取得する
		/// </summary>
		public RichText.ExtraTagEvent[] GetExtraTagEvent( params string[] tFilter )
		{
			Vector2 tExtents = rectTransform.rect.size ;
			RichTextGenerationSettings tSettings = GetRichTextGenerationSettings( tExtents ) ;
			return cachedTextGenerator.GetExtraTagEvent( text, tSettings, tFilter ) ;
		}
		
		/// <summary>
		/// 拡張タグ名を設定する
		/// </summary>
		/// <param name="tTagName"></param>
		public void SetExtraTagName( params string[] tTagName )
		{
			cachedTextGenerator.SetExtraTagName( tTagName ) ;
		}

		//-------------------------------------------------------------------------------------------

#if UNITY_EDITOR
		public override void OnRebuildRequested()
		{
			UntrackText( this ) ;
			TrackText( this ) ;

			// 頂点情報を更新する
            cachedTextGenerator.Invalidate() ;
			
			base.OnRebuildRequested() ;
		}
#endif


		//-------------------------------------------------------------------------------------

		// フォントトラッカーを自身のクラス専用にする

		protected static Dictionary<Font, List<RichText>> m_Tracked = new Dictionary<Font, List<RichText>>() ;

		// フォントのテクスチャ更新時の監視コールバックを登録する
		protected void TrackText( RichText tRichText )
		{
			if( tRichText.font == null )
			{
				// フォントが設定されていないので無関係
                return ;
			}
			
			// 該当のフォントを使用している CustomText のインスタンスのリストを取得する
			List<RichText> tRichTextList ;
			m_Tracked.TryGetValue( tRichText.font, out tRichTextList ) ;

			if( tRichTextList == null )
			{
				// １つも登録されていない
				if( m_Tracked.Count == 0 )
				{
					// コールバックを登録する
                    Font.textureRebuilt += RebuildForFont ;
				}
				
				// 該当のフォントで CustomText のリスト自体を登録する
				tRichTextList = new List<RichText>() ;
				m_Tracked.Add( tRichText.font, tRichTextList ) ;
			}
			
			if( tRichTextList.Contains( tRichText ) == false )
			{
				// リストに登録されていないければ CustomText を登録する
				tRichTextList.Add( tRichText ) ;
			}
        }
		
		// 指定の CutomText を監視対象から外す
		protected void UntrackText( RichText tRichText )
		{
			if( tRichText.font == null )
			{
				// フォントが設定されていないものなので無効
                return ;
			}
			
			// 該当のフォントを使用している CustomText のリストを取得する
			List<RichText> tRichTextList ;
			m_Tracked.TryGetValue( tRichText.font, out tRichTextList ) ;
			
			if( tRichTextList == null )
			{
				// １つも存在しない
				if( m_Tracked.ContainsKey( tRichText.font ) == true )
				{
					m_Tracked.Remove( tRichText.font ) ;
				}

				return ;
			}
			
			// 除去する
			tRichTextList.Remove( tRichText ) ;
			
			if( tRichTextList.Count == 0 )
			{
				// １つも無くなった

				// フォントも除去する
                m_Tracked.Remove( tRichText.font ) ;
				if( m_Tracked.Count == 0 )
				{
					// 監視コールバックを解除する
                    Font.textureRebuilt -= RebuildForFont ;
				}
			}
		}

		// フォントのテクスチャが更新された際に呼ばれる監視コールバック
		protected static void RebuildForFont( Font tFont )
		{
//			Debug.LogWarning( "RebuildForFont:" + f ) ;

			// 該当のフォントを使用している CustomText のリストを取得する
			List<RichText> tRichTextList ;
			m_Tracked.TryGetValue( tFont, out tRichTextList ) ;
			
			if( tRichTextList == null )
			{
				// １つも存在しない
				if( m_Tracked.ContainsKey( tFont ) == true )
				{
					m_Tracked.Remove( tFont ) ;
				}

				return ;
			}
			
//			Debug.LogWarning( "更新対象:" + tRichTextList.Count ) ;
//			for( var p  = 0 ; p <  tRichTextList.Count ; p ++ )
//			{
//				// メッシュ更新を呼ぶ
//				Debug.LogWarning( "確認1:" + p + " " + tRichTextList[ p ].text ) ;
//			}

			// リストだとループの途中でリストから除外されると正常に処理されなくなるので
			// いったん配列にコピーして実行する

			RichText[] tRichTextArray = tRichTextList.ToArray() ;
			int i, l = tRichTextArray.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				tRichTextArray[ i ].FontTextureChanged() ;
//				Debug.LogWarning( "更新:" + tRichTextArray[ i ].text ) ;
			}
		}
	}
}
