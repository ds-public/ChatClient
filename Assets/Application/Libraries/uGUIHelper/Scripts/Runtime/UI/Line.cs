using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.Events ;
using UnityEngine.EventSystems ;
using System ;
using System.Collections ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// 拡張:ライン
	/// </summary>
	public class Line : MaskableGraphicWrapper
	{
		/// <summary>
		/// スプライト
		/// </summary>
		[SerializeField][HideInInspector]
		private Sprite m_Sprite = null ;

		private Texture2D m_BlankTexture = null ;

		public  Sprite  sprite
		{
			get
			{
				return m_Sprite ;
			}
			set
			{
				if( m_Sprite != value )
				{
					m_Sprite  = value ;

					if( m_Sprite != null )
					{
						CanvasRenderer.SetTexture( m_Sprite.texture ) ;
					}
					else
					{
						if( m_BlankTexture == null )
						{
							m_BlankTexture = Resources.Load<Texture2D>( "uGUIHelper/Textures/UIBlank" ) ;
						}
						CanvasRenderer.SetTexture( m_BlankTexture ) ;
					}

					SetVerticesDirty() ;
				}
			}
		}

		/// <summary>
		/// 最初のカラー
		/// </summary>
		[HideInInspector][SerializeField]
		protected Color m_StartColor = Color.white ;

		public    Color  startColor
		{
			get
			{
				return m_StartColor ;
			}
			set
			{
				if( m_StartColor.Equals( value ) == false )
				{
					m_StartColor = value ;

					SetVerticesDirty() ;
				}
			}
		}
		
		/// <summary>
		/// 最後のカラー
		/// </summary>
		[HideInInspector][SerializeField]
		protected Color m_EndColor = Color.white ;

		public    Color  endColor
		{
			get
			{
				return m_EndColor ;
			}
			set
			{
				if( m_EndColor.Equals( value ) == false )
				{
					m_EndColor = value ;

					SetVerticesDirty() ;
				}
			}
		}
	
		/// <summary>
		/// 最初の太さ
		/// </summary>
		[HideInInspector][SerializeField]
		protected float m_StartWidth = 2 ;

		public    float  startWidth
		{
			get
			{
				return m_StartWidth ;
			}
			set
			{
				if( m_StartWidth != value )
				{
					m_StartWidth = value ;

					SetVerticesDirty() ;
				}
			}
		}
	
		/// <summary>
		/// 最後の太さ
		/// </summary>
		[HideInInspector][SerializeField]
		protected float m_EndWidth =  2 ;

		public    float  endWidth
		{
			get
			{
				return m_EndWidth ;
			}
			set
			{
				if( m_EndWidth != value )
				{
					m_EndWidth = value ;

					SetVerticesDirty() ;
				}
			}
		}
		

		/// <summary>
		/// オフセット
		/// </summary>
		[SerializeField][HideInInspector]
		protected Vector2 m_Offset = Vector2.zero ;

		public  Vector2 offset
		{
			get
			{
				return m_Offset ;
			}
			set
			{
				if( m_Offset.Equals( value ) == false )
				{
					m_Offset = value ;

					SetVerticesDirty() ;
				}
			}
		}




		/// <summary>
		/// 頂点配列
		/// </summary>
		public Vector2[] vertices = new Vector2[]{ Vector2.zero, Vector2.one } ;
//		[HideInInspector][SerializeField]
//		private Vector2[] mVertices = new Vector2[]{ Vector2.zero, Vector2.one } ;
//		public  Vector2[]  vertices
//		{
//			get
//			{
//				return mVertices ;
//			}
//			
//			set
//			{
//				mVertices = value ;
//			}
//		}

		/// <summary>
		/// 座標の位置タイプ
		/// </summary>
		public enum PositionType
		{
			Relative = 0,
			Absolute = 1,
		}

		[SerializeField][HideInInspector]
		private PositionType m_PositionType = PositionType.Relative ;

		public  PositionType  positionType
		{
			get
			{
				return m_PositionType ;
			}
			set
			{
				if( m_PositionType != value )
				{
					m_PositionType  = value ;

					SetVerticesDirty() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private float m_PreferredWidth = 0 ;

		/// <summary>
		/// 実際の横幅
		/// </summary>
		public float   preferredWidth
		{
			get
			{
				return m_PreferredWidth ;
			}
		}

		[SerializeField][HideInInspector]
		private float m_PreferredHeight = 0 ;
		
		/// <summary>
		/// 実際の縦幅
		/// </summary>
		public  float  preferredHeight
		{
			get
			{
				return m_PreferredHeight ;
			}
		}

		//----------------------------------------------------------
		
		// メッシュ更新
		protected override void OnPopulateMesh( VertexHelper tHelper )
		{
			if( IsActive() == false )
			{
				return ;
			}

			tHelper.Clear() ;

			m_PreferredWidth  = 0 ;
			m_PreferredHeight = 0 ;

			//-----------------------------------------
			
			if( vertices == null || vertices.Length <= 1 )
			{
				return ;
			}
		
			int i, j, l = vertices.Length ;

			// 最初に全体の長さを計算する
			float tLength = 0 ;
			for( i  = 1 ; i <  l ; i ++ )
			{
				tLength = tLength + ( vertices[ i ] - vertices[ i - 1 ] ).magnitude ;
			}
		
			if( tLength == 0 )
			{
				return ;
			}

			float bt = 0 ;
			float bb = 0 ;
			float bl = 0 ;
			float br = 0 ;

			if( m_Sprite != null )
			{
				bl = m_Sprite.border.x ;
				bt = m_Sprite.border.y ;
				br = m_Sprite.border.z ;
				bb = m_Sprite.border.w ;
			}

			List<Vector2> tV = new List<Vector2>() ;	// ボーダーも含めた頂点
			List<float>   tC = new List<float>() ;		// 色比率
			List<float>   tT = new List<float>() ;		// テクスチャのＹ座標位置
		
			float tBU = 0 ;
			if( ( vertices[ 1 ] - vertices[ 0 ] ).magnitude >  bt )
			{
				tBU = bt ;
			}
			float tBB = 0 ;
			if( ( vertices[ l - 1 ] - vertices[ l - 2 ] ).magnitude >  bb )
			{
				tBB = bb ;
			}
			
			float tHeight_ = 0 ;
			if( m_Sprite != null )
			{
				tHeight_ = m_Sprite.rect.height - tBU - tBB ;
			}
			float tLength_ = tLength - tBU - tBB ;
			
			Vector2 tPosition0 = Vector2.zero ;
			Vector2 tPosition1 = Vector2.zero ;

			// メッシュを生成する前の事前頂点情報を生成する
			float c = 0 ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( i  == 0 )
				{
					if( m_PositionType == PositionType.Relative )
					{
						tPosition0 = tPosition0 + vertices[ i ] ;
					}
					else
					{
						tPosition0 = vertices[ i ] ;
					}

					tV.Add( tPosition0 ) ;
					tC.Add( 0 ) ;

					if( m_Sprite != null )
					{
						tT.Add( 0 ) ;
					}
				
					if( tBU >  0 )
					{
						if( m_PositionType == PositionType.Relative )
						{
							tPosition1 = tPosition0 + vertices[ 1 ] ;
						}
						else
						{
							tPosition1 = vertices[ 1 ] ;
						}

						if( tPosition0.Equals( tPosition1 ) == false )
						{
							tV.Add( tPosition0 + tBU * ( tPosition1 - tPosition0 ).normalized ) ;
							tC.Add( tBU / tLength ) ;

							if( m_Sprite != null )
							{
								tT.Add( tBU ) ;
							}
						}
					}
				}
				else
				{
					tPosition1 = tPosition0 ;
					
					if( m_PositionType == PositionType.Relative )
					{
						tPosition0 = tPosition0 + vertices[ i ] ;
					}
					else
					{
						tPosition0 = vertices[ i ] ;
					}

					if( tPosition0.Equals( tPosition1 ) == false )
					{
						if( tBB >  0 && i == ( l - 1 ) )
						{
							tV.Add( tPosition0 + tBB * ( tPosition1 - tPosition0 ).normalized ) ;
							tC.Add( ( tLength - tBB ) / tLength ) ;

							if( m_Sprite != null )
							{
								tT.Add( m_Sprite.rect.height - tBB ) ;
							}
						}
				
						tV.Add( tPosition0 ) ;
				
						c = c + ( tPosition0 - tPosition1 ).magnitude ;
				
						tC.Add( ( float )c / ( float )tLength ) ;
					
						if( m_Sprite != null )
						{
							if( i <= ( l - 2 ) )
							{
								tT.Add( tBU + ( tHeight_ * ( c - tBU ) / tLength_ ) ) ;
							}
							else
							{
								tT.Add( m_Sprite.rect.height ) ;
							}
						}
					}
				}
			}

			if( tV.Count <  2 )
			{
				return ;
			}

			//-----------------------------------------------------------

			// 実際に頂点バッファを生成する

			List<UIVertex>	aV = new List<UIVertex>() ;
			List<int>		aI = new List<int>() ;


			float tTw = 0 ;
			float tTh = 0 ;
			
			if( m_Sprite != null )
			{
				tTw = m_Sprite.texture.width ;
				tTh = m_Sprite.texture.height ;
			}
		
			float ty ;
			Vector2 lt = Vector2.zero, rt = Vector2.zero, blt = Vector2.zero, brt = Vector2.zero ;
		
			Vector2 tNormal, tNormal_ = Vector2.one ;
			Vector2 tWidth ;
		
			float r, w, x, y, k ;
		
			Vector2 lv, rv, blv, brv ;
		
			int vi, vc ;

			UIVertex v ;
		
			Color tColor ;

			Vector3 tNormalVector = new Vector3(  0,  0, -1 ) ;
			
			if( m_Sprite != null )
			{
				lt.x = m_Sprite.rect.xMin / tTw ;
				rt.x = m_Sprite.rect.xMax / tTw ;
			
				if( bl  >  0 )
				{
					// 左のボーダーあり
					blt.x = ( m_Sprite.rect.xMin + bl ) / tTw ;
				}
			
				if( br >  0 )
				{
					// 右のボーダーあり
					brt.x = ( m_Sprite.rect.xMax - br ) / tTw ;
				}
			}
			
			for( i  = 0 ; i <  tV.Count ; i ++ )
			{
				r = tC[ i ] ;	// 開始点を０～終了点を１
				
				if( r >  1 )
				{
					r  = 1 ;
				}

				// その位置での半分の太さ
				w = ( ( m_EndWidth - m_StartWidth ) * r + m_StartWidth ) * 0.5f ;
			
				// 点
				x = tV[ i ].x ;
				y = tV[ i ].y ;
			
				if( i == 0 )
				{
					// 最初の点
					tNormal = ( tV[ i + 1 ] - tV[ i ] ).normalized ;
				
					tWidth.x = - tNormal.y * w ;
					tWidth.y =   tNormal.x * w ;
				
					// 左の点
					lv.x = x + tWidth.x ;
					lv.y = y + tWidth.y ;
				
					// 右の点
					rv.x = x - tWidth.x ;
					rv.y = y - tWidth.y ;
				
					//---------------------
				
					tNormal_ = tNormal ;
				}
				else
				if( i >  0 && i <  ( tV.Count - 1 ) )
				{
					// 途中の点
					tNormal = ( tV[ i + 1 ] - tV[ i ] ).normalized ;
				
					k = Vector2.Dot( tNormal, tNormal_ ) ;
				
					if( k >= 0 )
					{
						tNormal_ = ( tNormal + tNormal_ ).normalized ; 
						k = Vector2.Dot( tNormal, tNormal_ ) ;
					}
					else
					{
						tNormal_ = ( tNormal + tNormal_ ).normalized ;
						k = 1.0f ;
					}
				
					tWidth.x = - tNormal_.y * w / k ;
					tWidth.y =   tNormal_.x * w / k ;
				
					// 左の点
					lv.x = x + tWidth.x ;
					lv.y = y + tWidth.y ;
				
					// 右の点
					rv.x = x - tWidth.x ;
					rv.y = y - tWidth.y ;
				
					//---------------------
				
					tNormal_ = tNormal ;
				}
				else
				{
					// 最後の点
					tNormal = ( tV[ i ] - tV[ i - 1 ] ).normalized ;
				
					tWidth.x = - tNormal.y * w ;
					tWidth.y =   tNormal.x * w ;
				
					lv.x = x + tWidth.x ;
					lv.y = y + tWidth.y ;
				
					rv.x = x - tWidth.x ;
					rv.y = y - tWidth.y ;
				}
			
				// オフセット
				vi = aV.Count ;
				
				// 色
				tColor = Color.Lerp( m_StartColor, m_EndColor, r ) ;

				if( m_Sprite != null )
				{
					ty = 1.0f - ( ( m_Sprite.rect.yMin + tT[ i ] ) / tTh ) ;
				}
				else
				{
					ty = 0 ;
				}

				vc = 0 ;

				//-------------------------------------------

				// 左の点
				v = new UIVertex() ;

				v.position	= new Vector3( lv.x + m_Offset.x, lv.y + m_Offset.y, 0 ) ;
				v.normal	= tNormalVector ;
				v.color		= tColor ;

				if( m_Sprite != null )
				{
					v.uv0 = new Vector2( lt.x, ty ) ;
				}

				aV.Add( v ) ;
				vc ++ ;
		
				w = ( rv - lv ).magnitude ;
				if( bl >  0 )
				{
					// 左のボーダーの点
					v = new UIVertex() ;

					if( ( w - br ) >  bl )
					{
						blv = lv + bl * ( rv - lv ).normalized ;
					}
					else
					{
						blv = lv ;
					}
				
					v.position	= new Vector3( blv.x + m_Offset.x, blv.y + m_Offset.y, 0 ) ;
					v.normal	= tNormalVector ;
					v.color		= tColor ;

					if( m_Sprite != null )
					{
						v.uv0 = new Vector2( blt.x, ty ) ;
					}

					aV.Add( v ) ;
					vc ++ ;
				}
			
				if( br >  0 )
				{
					// 右のボーダーの点
					v = new UIVertex() ;

					if( ( w - bl ) >  br )
					{
						brv = rv + br * ( lv - rv ).normalized ;
					}
					else
					{
						brv = rv ;
					}
				
					v.position	= new Vector3( brv.x + m_Offset.x, brv.y + m_Offset.y, 0 ) ;
					v.normal	= tNormalVector ;
					v.color		= tColor ;

					if( m_Sprite != null )
					{
						v.uv0 = new Vector2( brt.x, ty ) ;
					}

					aV.Add( v ) ;
					vc ++ ;
				}
				
				// 右の点
				v = new UIVertex() ;

				v.position	= new Vector3( rv.x + m_Offset.x, rv.y + m_Offset.y, 0 ) ;
				v.normal	= tNormalVector ;
				v.color		= tColor ;

				if( m_Sprite != null )
				{
					v.uv0 = new Vector2( rt.x, ty ) ;
				}

				aV.Add( v ) ;
				vc ++ ;

				//----------------------------------------

				// インデックス
				if( i <  ( tV.Count - 1 ) )
				{
					for( j  = 0 ; j <  ( vc - 1 ) ; j ++ )
					{
						aI.Add( vi + j ) ;
						aI.Add( vi + j + 1 ) ;
						aI.Add( vi + j + vc ) ;
					
						aI.Add( vi + j + 1 ) ;
						aI.Add( vi + j + 1 + vc ) ;
						aI.Add( vi + j + vc ) ;
					}
				}
			}

			//----------------------------------------

			if( aV.Count >  0 && aI.Count >  0 )
			{
				// 頂点の最大最小からバウンディングボックスのサイズを算出する

				float xMin =   Mathf.Infinity ;
				float xMax = - Mathf.Infinity ;
				float yMin =   Mathf.Infinity ;
				float yMax = - Mathf.Infinity ;

				l = aV.Count ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					x = aV[ i ].position.x ;
					y = aV[ i ].position.y ;
					if( x <  xMin )
					{
						xMin = x ;
					}
					if( x >  xMax )
					{
						xMax = x ;
					}
					if( y <  yMin )
					{
						yMin = y ;
					}
					if( y >  yMax )
					{
						yMax = y ;
					}
				}

				m_PreferredWidth  = xMax - xMin ;
				m_PreferredHeight = yMax - yMin ;

//				Debug.LogWarning( "w:" + mPreferredWidth + " h:" + mPreferredHeight ) ;

				tHelper.AddUIVertexStream( aV, aI ) ;
			}
		}

		// マテリアル更新
		protected override void UpdateMaterial()
		{
			base.UpdateMaterial() ;

			// テクスチャを更新する
			if( m_Sprite != null )
			{
				CanvasRenderer.SetTexture( m_Sprite.texture ) ;
			}
			else
			{
				if( m_BlankTexture == null )
				{
					m_BlankTexture = Resources.Load<Texture2D>( "uGUIHelper/Textures/UIBlank" ) ;
				}
				CanvasRenderer.SetTexture( m_BlankTexture ) ;
			}
		}
	}
}
