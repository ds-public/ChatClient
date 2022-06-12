using UnityEngine ;
using UnityEngine.UI ;
using System.Collections ;

namespace uGUIHelper
{
	/// <summary>
	/// uGUI:RawImage クラスの機能拡張コンポーネントクラス
	/// </summary>
	[ RequireComponent( typeof( UnityEngine.UI.RawImage ) ) ]
	public class UIRawImage : UIView
	{
		/// <summary>
		/// テクスチャ(ショートカット)
		/// </summary>
		public Texture Texture
		{
			get
			{
				RawImage rawImage = CRawImage ;
				if( rawImage == null )
				{
					return null ;
				}
				return rawImage.texture ;
			}
			set
			{
				RawImage rawImage = CRawImage ;
				if( rawImage == null )
				{
					return ;
				}
				rawImage.texture = value ;
			}
		}
	
		/// <summary>
		/// カラー(ショートカット)
		/// </summary>
		public Color Color
		{
			get
			{
				RawImage rawImage = CRawImage ;
				if( rawImage == null )
				{
					return Color.white ;
				}
				return rawImage.color ;
			}
			set
			{
				RawImage rawImage = CRawImage ;
				if( rawImage == null )
				{
					return ;
				}
				rawImage.color = value ;
			}
		}
		
		/// <summary>
		/// マテリアル(ショートカット)
		/// </summary>
		public Material Material
		{
			get
			{
				RawImage rawImage = CRawImage ;
				if( rawImage == null )
				{
					return null ;
				}
				return rawImage.material ;
			}
			set
			{
				RawImage rawImage = CRawImage ;
				if( rawImage == null )
				{
					return ;
				}
				rawImage.material = value ;
			}
		}
		
		/// <summary>
		/// レイキャストターゲット(ショートカット)
		/// </summary>
		override public bool RaycastTarget
		{
			get
			{
				return CRawImage != null ? CRawImage.raycastTarget : false ;
			}
			set
			{
				if( CRawImage != null )
				{
					CRawImage.raycastTarget = value ;
				}
			}
		}

		/// <summary>
		/// ＵＶ座標(ショートカット)
		/// </summary>
		public Rect UVRect
		{
			get
			{
				RawImage rawImage = CRawImage ;
				if( rawImage == null )
				{
					return new Rect( 0, 0, 1, 1 ) ;
				}
				return rawImage.uvRect ;
			}
			set
			{
				RawImage rawImage = CRawImage ;
				if( rawImage == null )
				{
					return ;
				}
				rawImage.uvRect = value ;
			}
		}
	
		/// <summary>
		/// RectRransform のサイズをスプライトのサイズに合わせる
		/// </summary>
		public void SetNativeSize()
		{
			if( Texture == null )
			{
				return ;
			}

			SetSize( Texture.width, Texture.height ) ;
		}

		/// <summary>
		/// 自動的に描画可能なテクスチャを生成するかどうか
		/// </summary>
		[SerializeField]
		protected bool m_AutoCreateDrawableTexture = false ;
		public bool AutoCreateDrawableTexture{ get{ return m_AutoCreateDrawableTexture ; } set{ m_AutoCreateDrawableTexture = value ; } }

		//-----------------------------------------------------
	
		// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		override protected void OnBuild( string option = "" )
		{
			RawImage rawImage = CRawImage ;

			if( rawImage == null )
			{
				rawImage = gameObject.AddComponent<RawImage>() ;
			}
			if( rawImage == null )
			{
				// 異常
				return ;
			}

			rawImage.color = Color.white ;
			rawImage.raycastTarget = false ;

			if( IsCanvasOverlay == true )
			{
				rawImage.material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}

			ResetRectTransform() ;
		}

		protected override void OnAwake()
		{
			base.OnAwake() ;

			if( m_AutoCreateDrawableTexture == true )
			{
				CreateDrawableTexture() ;
			}
		}

		//-------------------------------------------------------------------

		private int m_Width  = 0 ;
		private int m_Height = 0 ;
		
		private Color32[] m_Pixels = null ;

		private Texture2D m_DrawableTexture = null ;

		// 縦方向の座標反転
		[SerializeField][HideInInspector]
		private bool m_FlipVertical = false ;

		public bool IsFlipVertical
		{
			get
			{
				return m_FlipVertical ;
			}
			set
			{
				m_FlipVertical = value ;
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 描画可能なテクスチャを生成して割り当てる
		/// </summary>
		/// <param name="tWidth"></param>
		/// <param name="tHeight"></param>
		/// <returns></returns>
		public bool CreateDrawableTexture( int width = 0, int height = 0 )
		{
			if( CRawImage == null )
			{
				return false ;
			}

			//----------------------------------------------------------

			DeleteDrawableTexture() ;

			// 描画可能テクスチャを生成して割り当てる
			if( width == 0 )
			{
				width = ( int )this.Width ;
			}
			if( height == 0 )
			{
				height = ( int )this.Height ;
			}

			m_Width  = width ;
			m_Height = height ;

			m_Pixels = new Color32[ m_Width * m_Height ] ;

			m_DrawableTexture = new Texture2D( m_Width, m_Height, TextureFormat.ARGB32, false ) ;

			m_DrawableTexture.SetPixels32( m_Pixels ) ;
			m_DrawableTexture.Apply() ;

			CRawImage.texture = m_DrawableTexture ;

			return true ;
		}

		/// <summary>
		/// 描画可能なテクスチャを破棄する
		/// </summary>
		public void DeleteDrawableTexture()
		{
			if( m_DrawableTexture != null )
			{
				DestroyImmediate( m_DrawableTexture ) ;
				m_DrawableTexture = null ;
			}

			m_Pixels = null ;

			m_Width  = 0 ;
			m_Height = 0 ;
		}

		// ＵＩが破棄される際に描画可能テクスチャが生成されていたら破棄する
		override protected void OnDestroy()
		{
			DeleteDrawableTexture() ;
		}

		//-----------------------------------------------------------
		// 各種描画メソッド群

		/// <summary>
		/// 直線を描画する
		/// </summary>
		/// <param name="x0"></param>
		/// <param name="y0"></param>
		/// <param name="x1"></param>
		/// <param name="y1"></param>
		/// <param name="tColor"></param>
		/// <param name="tPixels"></param>
		/// <param name="tWidth"></param>
		/// <param name="tHeight"></param>
		public bool DrawLine( int x0, int y0, int x1, int y1, uint color, bool isUpdate = true )
		{
			if( m_Pixels == null )
			{
				return false ;
			}

			//----------------------------------

			int dx ;
			int ax ;

			if( x1 >  x0 )
			{
				dx = x1 - x0 + 1 ;
				ax =  1 ;
			}
			else
			if( x0 >  x1 )
			{
				dx = x0 - x1 + 1 ;
				ax = -1 ;
			}
			else
			{
				dx = 1  ;
				ax = 0  ;
			}

			int dy ;
			int ay ;

			if( y1 >  y0 )
			{
				dy = y1 - y0 + 1 ;
				ay =  1 ;
			}
			else
			if( y0 >  y1 )
			{
				dy = y0 - y1 + 1 ;
				ay = -1 ;
			}
			else
			{
				dy = 1  ;
				ay = 0  ;
			}

			int px = x0 ;
			int py = y0 ;

			int cx = 0 ;
			int cy = 0 ;

			int x, y ;
			int tx, ty ;

			Color32 c = GetColor( color ) ;

			if( dx >= dy )
			{
				for( x  = 0 ; x <  dx ; x ++ )
				{
//					DrawPixel( px, py, color ) ;
					if( px >= 0 && px <  m_Width && py >= 0 && py <  m_Height )
					{
						tx = px ;
						ty = py ;
						if( m_FlipVertical == true )
						{
							ty = m_Height - 1 - ty ;
						}

						m_Pixels[ ty * m_Width + tx ] = c ;
					}
					
					px += ax ;

					cy += dy ;
					if( cy >= dx )
					{
						cy -= dx ;
						py += ay ;
					}
				}
			}
			else
			{
				for( y  = 0 ; y <  dy ; y ++ )
				{
//					DrawPixel( px, py, color ) ;
					if( px >= 0 && px <  m_Width && py >= 0 && py <  m_Height )
					{
						tx = px ;
						ty = py ;
						if( m_FlipVertical == true )
						{
							ty = m_Height - 1 - ty ;
						}

						m_Pixels[ ty * m_Width + tx ] = c ;
					}

					py += ay ;

					cx += dx ;
					if( cx >= dy )
					{
						cx -= dy ;
						px += ax ;
					}
				}
			}

			if( isUpdate == true )
			{
				UpdateDrawableTexture() ;
			}

			return true ;
		}

		/// <summary>
		/// 点を描画する
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="tColor"></param>
		/// <returns></returns>
		public bool DrawPixel( int x, int y, uint color, bool isUpdate = true )
		{
			if( m_Pixels == null )
			{
				return false ;
			}

			//----------------------------------

			if( x >= 0 && x <  m_Width && y >= 0 && y <  m_Height )
			{
				if( m_FlipVertical == true )
				{
					y = m_Height - 1 - y ;
				}

				m_Pixels[ y * m_Width + x ] = GetColor( color ) ;
			}

			if( isUpdate == true )
			{
				UpdateDrawableTexture() ;
			}

			return true ;
		}
		
		/// <summary>
		/// 塗りつぶされた矩形を描画する
		/// </summary>
		/// <param name="tColor"></param>
		/// <param name="tUpdate"></param>
		/// <returns></returns>
		public bool FillRectangle( uint color, bool isUpdate = true )
		{
			return FillRectangle( 0, 0, m_Width, m_Height, color, isUpdate ) ;
		}

		/// <summary>
		/// 塗りつぶされた矩形を描画する
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="w"></param>
		/// <param name="h"></param>
		/// <param name="tColor"></param>
		/// <returns></returns>
		public bool FillRectangle( int x, int y, int w, int h, uint color, bool isUpdate = true )
		{
			if( m_Pixels == null )
			{
				return false ;
			}

			//----------------------------------

			Color32 c = GetColor( color ) ;

			int lx, ly ;
			int px, py ;
			int tx, ty ;

			py = y ;
			for( ly  = 0 ; ly <  h ; ly ++ )
			{
				px = x ;
				for( lx  = 0 ; lx <  w ; lx ++ )
				{
					if( px >= 0 && px <  m_Width && py >= 0 && py <  m_Height )
					{
						tx = px ;
						ty = py ;
						if( m_FlipVertical == true )
						{
							ty = m_Height - 1 - ty ;
						}

						m_Pixels[ ty * m_Width + tx ] = c ;
					}
					px ++ ;
				}
				py ++ ;
			}

			if( isUpdate == true )
			{
				UpdateDrawableTexture() ;
			}

			return true ;
		}


		private Color32 GetColor( uint color )
		{
			byte r = ( byte )( ( color >> 16 ) & 0xFF ) ;
			byte g = ( byte )( ( color >>  8 ) & 0xFF ) ;
			byte b = ( byte )( ( color >>  0 ) & 0xFF ) ;
			byte a = ( byte )( ( color >> 24 ) & 0xFF ) ;

			return new Color32( r, g, b, a ) ;
		}

		// 描画可能テクスチャを更新する
		public bool UpdateDrawableTexture()
		{
			if( m_DrawableTexture == null )
			{
				return false ;
			}

			m_DrawableTexture.SetPixels32( m_Pixels ) ;
			m_DrawableTexture.Apply() ;

			return true ;
		}
	}
}

