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
	/// 拡張:サークル
	/// </summary>
	public class Arc : MaskableGraphicWrapper
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
		/// 内側のカラー
		/// </summary>
		[HideInInspector][SerializeField]
		protected Color m_InnerColor = Color.white ;

		public    Color  innerColor
		{
			get
			{
				return m_InnerColor ;
			}
			set
			{
				if( m_InnerColor.Equals( value ) == false )
				{
					m_InnerColor = value ;

					SetVerticesDirty() ;
				}
			}
		}
		
		/// <summary>
		/// 外側のカラー
		/// </summary>
		[HideInInspector][SerializeField]
		protected Color m_OuterColor = Color.white ;

		public    Color  outerColor
		{
			get
			{
				return m_OuterColor ;
			}
			set
			{
				if( m_OuterColor.Equals( value ) == false )
				{
					m_OuterColor = value ;

					SetVerticesDirty() ;
				}
			}
		}

		/// <summary>
		/// 円弧開始角度
		/// </summary>
		[HideInInspector][SerializeField]
		protected float m_StartAngle  =   0.0f ;

		public    float  startAngle
		{
			get
			{
				return m_StartAngle ;
			}
			set
			{
				if( m_StartAngle != value )
				{
					m_StartAngle = value ;

					SetVerticesDirty() ;
				}
			}
		}
	
		/// <summary>
		/// 円弧終了角度
		/// </summary>
		[HideInInspector][SerializeField]
		protected float m_EndAngle  = 360.0f ;

		public    float  endAngle
		{
			get
			{
				return m_EndAngle ;
			}
			set
			{
				if( m_EndAngle != value )
				{
					m_EndAngle = value ;

					SetVerticesDirty() ;
				}
			}
		}
		
		/// <summary>
		/// 円弧方向
		/// </summary>
		public enum Direction
		{
			Right =  1,
			Left  = -1,
		}
		
		[HideInInspector][SerializeField]
		protected Direction m_Direction  = Direction.Right ;

		public    Direction  direction
		{
			get
			{
				return m_Direction ;
			}
			set
			{
				if( m_Direction != value )
				{
					m_Direction  = value ;

					SetVerticesDirty() ;
				}
			}
		}
		
		/// <summary>
		/// 形状
		/// </summary>
		public enum ShapeType
		{
			Circle = 0,
			Rectangle = 1,
		}
	
		[HideInInspector][SerializeField]
		protected ShapeType m_ShapeType  = ShapeType.Circle ;

		public    ShapeType  shapeType
		{
			get
			{
				return m_ShapeType ;
			}
			set
			{
				if( m_ShapeType != value )
				{
					m_ShapeType  = value ;

					SetVerticesDirty() ;
				}
			}
		}

		/// <summary>
		/// 分割数(円の場合のみ有効)
		/// </summary>
		[HideInInspector][SerializeField]
		protected int m_Split = 36 ;

		public    int  split
		{
			get
			{
				return m_Split ;
			}
			set
			{
				if( m_Split != value && value >= 3 )
				{
					m_Split = value ;

					SetVerticesDirty() ;
				}
			}
		}

		/// <summary>
		/// テクスチャの張り方(形状が円限定)
		/// </summary>
		public enum DecalType
		{
			Normal = 0,
			Effect = 1,
		}

		[HideInInspector][SerializeField]
		protected DecalType m_DecalType = DecalType.Normal ;

		public    DecalType  decalType
		{
			get
			{
				return m_DecalType ;
			}
			set
			{
				if( m_DecalType != value )
				{
					m_DecalType  = value ;

					SetVerticesDirty() ;
				}
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

			//-----------------------------------------------------------

			float s0 = m_StartAngle / 360.0f ;
			float s1 = m_EndAngle   / 360.0f ;
		
			// 0～1未満に収める
			if( s0 != 1.0f )
			{
				s0 = s0 % 1.0f ;
				if( s0 <  0 )
				{
					s0 = s0 + 1.0f ;
				}
			}
		
			if( s1 != 1.0f )
			{
				s1 = s1 % 1.0f ;
				if( s1 <  0 )
				{
					s1 = s1 + 1.0f ;
				}
			}

			if( m_Direction == Direction.Right )
			{
				if( s0 == s1 || ( s0 == 1 && s1 == 0 ) )
				{
					return ;
				}
			}
			else
			if( m_Direction == Direction.Left )
			{
				if( s0 == s1 || ( s0 == 0 && s1 == 1 ) )
				{
					return ;
				}
			}
		

			if( m_ShapeType == ShapeType.Circle )
			{
				SetCircleArc( tHelper, s0, s1 ) ;
			}
			else
			if( m_ShapeType == ShapeType.Rectangle )
			{
				SetRectAngleArc( tHelper, s0, s1 ) ;
			}
		}

		// 円形の弧
		private void SetCircleArc( VertexHelper tHelper, float s0, float s1 )
		{
			// 位置を入れ替える(常に s0 から s1 に向かって右回り)
			if( m_Direction == Direction.Left )
			{
				float s ;

				s  = s0 ;
				s0 = s1 ;
				s1 = s ;
			}
		
			//------------------------------------

			List<UIVertex>	aV = new List<UIVertex>() ;
			List<int>		aI = new List<int>() ;


			Vector2 tSize = Size ;
			Vector2 tPivot = RectTransform.pivot ;

			UIVertex v ;

			int i, j ;
			float a ;

			Vector2 p = Vector2.zero ;

			float vrw = tSize.x * 0.5f ;
			float vrh = tSize.y * 0.5f ;
			float vcx = tSize.x * ( 0.5f - tPivot.x ) ;
			float vcy = tSize.y * ( 0.5f - tPivot.y ) ;
			
			float tTw = 0 ;
			float tTh = 0 ;

			float trw = 0 ;
			float trh = 0 ;
			float tcx = 0 ;
			float tcy = 0 ;

			if( m_Sprite != null )
			{
				tTw = m_Sprite.texture.width ;
				tTh = m_Sprite.texture.height ;

				trw = m_Sprite.rect.width  * 0.5f ;
				trh = m_Sprite.rect.height * 0.5f ;

				tcx = m_Sprite.rect.x + trw ;
				tcy = m_Sprite.rect.y + trh ;
			}

			Vector3 tNormalVector = new Vector3(  0,  0, -1 ) ;

			//---------------------------------------------

			float da = 1.0f / ( float )m_Split ;

			float a0 = s0 - ( s0 % da ) ; 
			float at = 0 ;

			if( s1 <  s0 )
			{
				s1  = s1 + 1.0f ;
			}

			float[] tA = new float[ m_Split + 1 ] ;

			int ap = 0 ;

			for( i  = 0 ; i <= m_Split ; i ++ )
			{
				if( i == 0 )
				{
					tA[ ap ] = s0 ;
					ap ++ ;
				}
				else
				{
					at = ( a0 + ( ( float )i / ( float )m_Split ) ) ;

					if( at <  s1 )
					{
						tA[ ap ] = at % 1.0f ;
						ap ++ ;
					}
					else
					{
						tA[ ap ] = s1 % 1.0f ;
						ap ++ ;

						break ;
					}
				}
			}

			//---------------------------------------------

			Vector2[] tV = new Vector2[ ap ] ;
			
			Vector2[] tT = null ;

			if( m_Sprite != null && m_DecalType == DecalType.Normal )
			{
				tT = new Vector2[ ap ] ;
			}

			// 頂点・ＵＶ情報を整理する
			for( i  = 0 ; i <  ap ; i ++ )
			{
				a = 2.0f * Mathf.PI * tA[ i ] ;
				
				// 上を頂点開始地点にする(時計回り)
				p.x = Mathf.Sin( a ) ;
				p.y = Mathf.Cos( a ) ;


				tV[ i ] = new Vector2( p.x * vrw + vcx, p.y * vrh + vcy ) ;	// ９０度回転させてＹ符号反転

				if( m_Sprite != null && m_DecalType == DecalType.Normal )
				{
					tT[ i ] = new Vector2( p.x * trw + tcx, p.y * trh + tcy ) ;
				}
			}
			
			//-------------------------------------------------
			
			// 実際に頂点データを生成する(２タイプから選択可能)
			int vi ;
		
			if( m_DecalType == DecalType.Normal )
			{
				// テクスチャの張り方：通常
				// 中心
				v = new UIVertex() ;

				v.position	= new Vector3( vcx, vcy, 0 ) ;
				v.normal	= tNormalVector ;
				v.color		= m_InnerColor ;

				if( m_Sprite != null )
				{
					v.uv0		= new Vector2( tcx / tTw, tcy / tTh ) ;
				}

				aV.Add( v ) ;

				for( i  = 0 ; i <  ( ap - 1 ) ; i ++ )
				{
					vi = aV.Count ;

					j = i + 1 ;
				
					// １点目
					v = new UIVertex() ;

					v.position	= new Vector3( tV[ i ].x, tV[ i ].y, 0 ) ;
					v.normal	= tNormalVector ;
					v.color		= m_OuterColor ;

					if( m_Sprite != null )
					{
						v.uv0	= new Vector2( tT[ i ].x / tTw, tT[ i ].y / tTh ) ;
					}

					aV.Add( v ) ;

					// ２点目
					v.position	= new Vector3( tV[ j ].x, tV[ j ].y, 0 ) ;
					v.normal	= tNormalVector ;
					v.color		= m_OuterColor ;

					if( m_Sprite != null )
					{
						v.uv0	= new Vector2( tT[ j ].x / tTw, tT[ j ].y / tTh ) ;
					}

					aV.Add( v ) ;

					// インデックス
					aI.Add( 0 ) ;
					aI.Add( vi + 0 ) ;
					aI.Add( vi + 1 ) ;
				}
			}
			else
			if( m_DecalType == DecalType.Effect )
			{
				// テクスチャの張り方：効果

				float tx0 = 0 ;
				float ty0 = 0 ;

				float tx1 = 0 ;
				float ty1 = 0 ;

				float tx2 = 0 ;
				float ty2 = 0 ;

				if( m_Sprite != null )
				{
					tx0 = tcx					/ tTw ;
					ty0 = m_Sprite.rect.yMax	/ tTh ;

					tx1 = m_Sprite.rect.xMin	/ tTw ;
					ty1 = m_Sprite.rect.yMin	/ tTh ;

					tx2 = m_Sprite.rect.xMax	/ tTw ;
					ty2 = m_Sprite.rect.yMin	/ tTh ;
				}

				for( i  = 0 ; i <  ( ap - 1 ) ; i ++ )
				{
					vi = aV.Count ;
			
					j = i + 1 ;
				
					// １点目
					v = new UIVertex() ;
				
					v.position	= new Vector3( vcx, vcy, 0 ) ;
					v.normal	= tNormalVector ;
					v.color		= m_InnerColor ;

					if( m_Sprite != null )
					{
						v.uv0	= new Vector2( tx0, ty0 ) ;
					}

					aV.Add( v ) ;

					// ２点目
					v = new UIVertex() ;

					v.position	= new Vector3( tV[ i ].x, tV[ i ].y, 0 ) ;
					v.normal	= tNormalVector ;
					v.color		= m_OuterColor ;

					if( m_Sprite != null )
					{
						v.uv0	= new Vector2( tx1, ty1 ) ;
					}

					aV.Add( v ) ;

					// ３点目
					v.position	= new Vector3( tV[ j ].x, tV[ j ].y, 0 ) ;
					v.normal	= tNormalVector ;
					v.color		= m_OuterColor ;

					if( m_Sprite != null )
					{
						v.uv0	= new Vector2( tx2, ty2 ) ;
					}
					
					aV.Add( v ) ;

					// インデックス
					aI.Add( vi + 0 ) ;
					aI.Add( vi + 1 ) ;
					aI.Add( vi + 2 ) ;
				}
			}


			if( aV.Count >  0 && aI.Count >  0 )
			{
				tHelper.AddUIVertexStream( aV, aI ) ;
			}			

		}

		// 四角の弧
		private void SetRectAngleArc( VertexHelper tHelper, float s0, float s1 )
		{
			Vector2 tSize = Size ;
			Vector2 tPivot = RectTransform.pivot ;

			float rw = tSize.x * 0.5f ;
			float rh = tSize.y * 0.5f ;
		
			float x0 = - rw ;
			float x1 =   rw ;
			float y0 = - rh ;
			float y1 =   rh ;

			float vcx = tSize.x * ( 0.5f - tPivot.x ) ;
			float vcy = tSize.y * ( 0.5f - tPivot.y ) ;

			// 開始角度
			int p0 = 0 ;

			if( s0 == 0.000f || s0 == 1.000f )
			{
				p0 =  0 ;
			}
			else
			if( s0 >  0.000f && s0 <  0.125f )
			{
				p0 =  1 ;
			}
			else
			if( s0 == 0.125f )
			{
				p0 =  2 ; 
			}
			else
			if( s0 >  0.125f && s0 <  0.250f )
			{
				p0 =  3 ;
			}
			else
			if( s0 == 0.250f )
			{
				p0 =  4 ;
			}
			else
			if( s0 >  0.250f && s0 <  0.375f )
			{
				p0 =  5 ;
			}
			else
			if( s0 == 0.375f )
			{
				p0 =  6 ;
			}
			else
			if( s0 >  0.375f && s0 <  0.500f )
			{
				p0 =  7 ;
			}
			else
			if( s0 == 0.500f )
			{
				p0 =  8 ;
			}
			else
			if( s0 >  0.500f && s0 <  0.625f )
			{
				p0 =  9 ;
			}
			else
			if( s0 == 0.625f )
			{
				p0 = 10 ;
			}
			else
			if( s0 >  0.625f && s0 <  0.750f )
			{
				p0 = 11 ;
			}
			else
			if( s0 == 0.750f )
			{
				p0 = 12 ;
			}
			else
			if( s0 >  0.750f && s0 <  0.875f )
			{
				p0 = 13 ;
			}
			else
			if( s0 == 0.875f )
			{
				p0 = 14 ;
			}
			else
			if( s0 >  0.875f && s0 <  1.000f )
			{
				p0 = 15 ;
			}
			
			// 終了角度
			int p1 = 0 ;

			if( s1 == 0.000f || s1 == 1.000f )
			{
				p1 =  0 ;
			}
			else
			if( s1 >  0.000f && s1 <  0.125f )
			{
				p1 =  1 ;
			}
			else
			if( s1 == 0.125f )
			{
				p1 =  2 ; 
			}
			else
			if( s1 >  0.125f && s1 <  0.250f )
			{
				p1 =  3 ;
			}
			else
			if( s1 == 0.250f )
			{
				p1 =  4 ;
			}
			else
			if( s1 >  0.250f && s1 <  0.375f )
			{
				p1 =  5 ;
			}
			else
			if( s1 == 0.375f )
			{
				p1 =  6 ;
			}
			else
			if( s1 >  0.375f && s1 <  0.500f )
			{
				p1 =  7 ;
			}
			else
			if( s1 == 0.500f )
			{
				p1 =  8 ;
			}
			else
			if( s1 >  0.500f && s1 <  0.625f )
			{
				p1 =  9 ;
			}
			else
			if( s1 == 0.625f )
			{
				p1 = 10 ;
			}
			else
			if( s1 >  0.625f && s1 <  0.750f )
			{
				p1 = 11 ;
			}
			else
			if( s1 == 0.750f )
			{
				p1 = 12 ;
			}
			else
			if( s1 >  0.750f && s1 <  0.875f )
			{
				p1 = 13 ;
			}
			else
			if( s1 == 0.875f )
			{
				p1 = 14 ;
			}
			else
			if( s1 >  0.875f && s1 <  1.000f )
			{
				p1 = 15 ;
			}
			

			List<Vector2> tV = new List<Vector2>() ;
		
			int i, p ;
		
		
			if( m_Direction == Direction.Left )
			{
				float s ;

				p  = p0 ;
				p0 = p1 ;
				p1 = p ;
			
				s  = s0 ;
				s0 = s1 ;
				s1 = s ;
			}
		
			Vector2 vp ;

			vp = GetPosition( p0, s0, x0, y0, x1, y1 ) ; vp.y = - vp.y ;	// 方向補正
			tV.Add( vp ) ;
			
			p = p0 ;
			for( i  =  0 ; i <= 16 ; i ++ )
			{
				p = ( p + 1 + 16 ) % 16 ;
			
				if( p == p1 )
				{
					vp = GetPosition( p1, s1, x0, y0, x1, y1 ) ; vp.y = - vp.y ;	// 方向補正
					tV.Add( vp ) ;
					break ;
				}
				else
				{
					if( ( p % 2 ) == 0 )
					{
						vp = GetPosition( p,  0,  x0, y0, x1, y1 ) ; vp.y = - vp.y ;	// 方向補正
						tV.Add( vp ) ;
					}
				}
			}
			
			//-----------------------------------------------------------

			// 実際に頂点バッファを生成する

			List<UIVertex>	aV = new List<UIVertex>() ;
			List<int>		aI = new List<int>() ;


			float tTw = 0  ;
			float tTh = 0  ;
		
			float tx0 = 0 ;
			float ty0 = 0 ;
			float tx1 = 0 ;
			float ty1 = 0 ;

			float tcx = 0 ;
			float tcy = 0 ;
			
			if( m_Sprite != null )
			{
				tTw = m_Sprite.rect.width ;
				tTh = m_Sprite.rect.height ;

				tx0 = m_Sprite.rect.xMin / tTw ;
				ty0 = m_Sprite.rect.yMin / tTh ;
				tx1 = m_Sprite.rect.xMax / tTw ;
				ty1 = m_Sprite.rect.yMax / tTh ;

				tcx = ( tx0 + tx1 ) * 0.5f ;
				tcy = ( ty0 + ty1 ) * 0.5f ;
			}

			int l = tV.Count ;
		
			int vi ;
		
			float vx0, vy0, vx1, vy1 ;
			float rx0, ry0, rx1, ry1 ;
		
			float tpx0, tpy0, tpx1, tpy1 ;

			UIVertex v ;

			Vector3 tNormalVector = new Vector3(  0,  0, -1 ) ;
		
			for( i  = 0 ; i <  ( l - 1 ) ; i ++ )
			{
				vi = aV.Count ;
				
				vx0 = tV[ i     ].x ;
				vy0 = tV[ i     ].y ;
				vx1 = tV[ i + 1 ].x ;
				vy1 = tV[ i + 1 ].y ;
				
				rx0 = ( vx0 - x0 ) / ( x1 - x0 ) ;
				ry0 = ( vy0 - y0 ) / ( y1 - y0 ) ;
			
				rx1 = ( vx1 - x0 ) / ( x1 - x0 ) ;
				ry1 = ( vy1 - y0 ) / ( y1 - y0 ) ;
			
				tpx0 = ( ( tx1 - tx0 ) * rx0 ) + tx0 ;
				tpy0 = ( ( ty1 - ty0 ) * ry0 ) + ty0 ;
			
				tpx1 = ( ( tx1 - tx0 ) * rx1 ) + tx0 ;
				tpy1 = ( ( ty1 - ty0 ) * ry1 ) + ty0 ;

				// １点目

				v = new UIVertex() ;

				v.position  = new Vector3( vx0 + vcx, vy0 + vcy, 0 ) ;
				v.normal	= tNormalVector ;
				v.color		= m_OuterColor ;

				if( m_Sprite != null )
				{
					v.uv0	= new Vector2( tpx0, tpy0 ) ;
				}
				
				aV.Add( v ) ;

				// ２点目

				v = new UIVertex() ;

				v.position	= new Vector3( vx1 + vcx, vy1 + vcy, 0 ) ;
				v.normal	= tNormalVector ;
				v.color		= m_OuterColor ;

				if( m_Sprite != null )
				{
					v.uv0	= new Vector2( tpx1, tpy1 ) ;
				}

				aV.Add( v ) ;

				// ３点目

				v = new UIVertex() ;

				v.position	= new Vector3( vcx, vcy, 0 ) ;
				v.normal	= tNormalVector ;
				v.color		= m_InnerColor ;

				if( m_Sprite != null )
				{
					v.uv0	= new Vector2( tcx, tcy ) ;
				}

				aV.Add( v ) ;
				
				// インデックス
							
				aI.Add( vi + 0 ) ;
				aI.Add( vi + 1 ) ;
				aI.Add( vi + 2 ) ;
			}

			if( aV.Count >  0 && aI.Count >  0 )
			{
				tHelper.AddUIVertexStream( aV, aI ) ;
			}			
		}


		// 角度から交差点を取得する
		private Vector2 GetPosition( int p, float s, float x0, float y0, float x1, float y1 )
		{
			float a, x ,y ;
			float dx, dy ;

			s = s % 1.0f ;

			if( p ==  0 )
			{
				return new Vector2(  0, y0 ) ;
			}
			else
			if( p ==  1 )
			{
				a = s * 2.0f * Mathf.PI ;
			
				dx =   Mathf.Sin( a ) ;
				dy = - Mathf.Cos( a ) ;
			
				x = y0 * dx / dy ;
			
				return new Vector2(  x, y0 ) ;
			}
			else
			if( p ==  2 )
			{
				return new Vector2( x1, y0 ) ;
			}
			else
			if( p ==  3 )
			{
				a = s * 2.0f * Mathf.PI ;
			
				dx =   Mathf.Sin( a ) ;
				dy = - Mathf.Cos( a ) ;
			
				y = x1 * dy / dx ; 
			
				return new Vector2( x1,  y ) ;
			}
			else
			if( p ==  4 )
			{
				return new Vector2( x1,  0 ) ;
			}
			else
			if( p ==  5 )
			{
				a = s * 2.0f * Mathf.PI ;
			
				dx =   Mathf.Sin( a ) ;
				dy = - Mathf.Cos( a ) ;
			
				y = x1 * dy / dx ; 
			
				return new Vector2( x1,  y ) ;
			}
			else
			if( p ==  6 )
			{
				return new Vector2( x1, y1 ) ;
			}
			else
			if( p ==  7 )
			{
				a = s * 2.0f * Mathf.PI ;
			
				dx =   Mathf.Sin( a ) ;
				dy = - Mathf.Cos( a ) ;
			
				x = y1 * dx / dy ;
			
				return new Vector2(  x, y1 ) ;
			}
			else
			if( p ==  8 )
			{
				return new Vector2(  0, y1 ) ;
			}
			else
			if( p ==  9 )
			{
				a = s * 2.0f * Mathf.PI ;
			
				dx =   Mathf.Sin( a ) ;
				dy = - Mathf.Cos( a ) ;
			
				x = y1 * dx / dy ;
			
				return new Vector2(  x, y1 ) ;
			}
			else
			if( p == 10 )
			{
				return new Vector2( x0, y1 ) ;
			}
			else
			if( p == 11 )
			{
				a = s * 2.0f * Mathf.PI ;
			
				dx =   Mathf.Sin( a ) ;
				dy = - Mathf.Cos( a ) ;
			
				y = x0 * dy / dx ; 
			
				return new Vector2( x0,  y ) ;
			}
			else
			if( p == 12 )
			{
				return new Vector2( x0,  0 ) ;
			}
			else
			if( p == 13 )
			{
				a = s * 2.0f * Mathf.PI ;
			
				dx =   Mathf.Sin( a ) ;
				dy = - Mathf.Cos( a ) ;
			
				y = x0 * dy / dx ; 
			
				return new Vector2( x0,  y ) ;
			}
			else
			if( p == 14 )
			{
				return new Vector2( x0, y0 ) ;
			}
			else
			if( p == 15 )
			{
				a = s * 2.0f * Mathf.PI ;
			
				dx =   Mathf.Sin( a ) ;
				dy = - Mathf.Cos( a ) ;
			
				x = y0 * dx / dy ;
			
				return new Vector2(  x, y0 ) ;
			}
		
			return Vector2.zero ;
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
