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
	/// 拡張:グリッドマップ
	/// </summary>
	public class ComplexRectangle : MaskableGraphicWrapper
	{
		[SerializeField][HideInInspector]
		private Vector2		m_Offset = Vector2.zero ;

		public  Vector2		  offset
		{
			get
			{
				return m_Offset ;
			}
			set
			{
				if( m_Offset.x != value.x || m_Offset.y != value.y )
				{
					m_Offset = value ;
					SetVerticesDirty() ;
				}
			}
		}

		//----------------------------------------------------------

		[Serializable]
		public class Rectangle
		{
			public	string	name ;

			public	bool	visible		= true ;

			public	Vector2	offset		= Vector2.zero ;
			public	Vector2	pivot		= new Vector2( 0.5f, 0.5f ) ;
			public	Vector2	size		= new Vector2( 100, 100 ) ;

			public	float	rotation	= 0 ;
			public	Color	vertexColor	= Color.white ;

			public	Rect	uv			= new Rect( 0, 0, 1, 1 ) ;

			public	int		priority	= 0 ;
		}

		public List<Rectangle>	rectangle = new List<Rectangle>() ;

		//----------------------------------------------------------

		[SerializeField][HideInInspector]
		private Texture2D m_Texture = null ;

		private Texture2D m_BlankTexture = null ;
		
		public  Texture2D  texture
		{
			get
			{
				return m_Texture ;
			}
			set
			{
				if( m_Texture != value )
				{
					m_Texture  = value ;

					CanvasRenderer tCanvasRenderer = CanvasRenderer ;

					if( m_Texture != null )
					{
						tCanvasRenderer.SetTexture( m_Texture ) ;
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

		/// <summary>
		/// 矩形を追加する
		/// </summary>
		/// <param name="tName"></param>
		/// <param name="tOffset"></param>
		/// <param name="tSize"></param>
		/// <param name="tRotation"></param>
		/// <param name="tColor"></param>
		/// <param name="tUV"></param>
		/// <param name="tPriority"></param>
		public void AddRectangle( string tName, Vector2 tOffset, Vector2 tPivot, Vector2 tSize, float tRotation, Color tColor, Rect tUV, int tPriority )
		{
			Rectangle tR = new Rectangle() ;

			tR.name			= tName ;
			tR.offset		= tOffset ;
			tR.pivot		= tPivot ;
			tR.size			= tSize ;
			tR.rotation		= tRotation ;
			tR.vertexColor	= tColor ;
			tR.uv			= tUV ;
			tR.priority		= tPriority ;

			rectangle.Add( tR ) ;

			SetVerticesDirty() ;
		}

		/// <summary>
		/// 矩形を削除する
		/// </summary>
		/// <param name="tName"></param>
		public void RemoveRectangle( string tName )
		{
			List<Rectangle> tRectangle = new List<Rectangle>() ;

			int i, l = rectangle.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( rectangle[ i ].name != tName )
				{
					tRectangle.Add( rectangle[ i ] ) ;
				}
			}

			rectangle = tRectangle ;

			SetVerticesDirty() ;
		}

		/// <summary>
		/// 矩形を全て削除する
		/// </summary>
		public void ClearRectangle()
		{
			rectangle.Clear() ;

			SetVerticesDirty() ;
		}

		//----------------------------------------------------------

		[SerializeField][HideInInspector]
		private float m_PreferredWidth = 0 ; 

		[SerializeField][HideInInspector]
		private float m_PreferredHeight = 0 ;
		
		// メッシュ更新
		protected override void OnPopulateMesh( VertexHelper tHelper )
		{
			if( IsActive() == false )
			{
				return ;
			}

			tHelper.Clear() ;

			//-----------------------------------------

			m_PreferredWidth  = 0 ;
			m_PreferredHeight = 0 ;
				
			int i, l, c, o ;
			float x, y ;

			if( rectangle.Count >  0 )
			{
				l = rectangle.Count ;
				List<Rectangle> tRectangle = new List<Rectangle>() ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					if( rectangle[ i ].visible == true && rectangle[ i ].size.x != 0 && rectangle[ i ].size.y != 0 )
					{
						tRectangle.Add( rectangle[ i ] ) ;
					}
				}

				if( tRectangle.Count >  0 )
				{
					tRectangle.Sort( ( a, b ) => ( a.priority - b.priority ) ) ;	// 昇順ソート


					Rectangle tP ;
					float tSX, tSY, tPX0, tPY0, tPX1, tPY1 ;
					bool tRX, tRY ;

					float tAngle, tCv, tSv ; ;
					
					float[] tVx = new float[ 4 ] ;
					float[] tVy = new float[ 4 ] ;
					float vx, vy ;

					float tTx0, tTy0, tTx1, tTy1 ;
					float[]	tTx = new float[ 4 ] ;
					float[] tTy = new float[ 4 ] ;
					float tx, ty ;

					UIVertex v ;

					List<UIVertex>	aV = new List<UIVertex>() ;
					List<int>		aI = new List<int>() ;

					float xMin =   Mathf.Infinity ;
					float yMin =   Mathf.Infinity ;
					float xMax = - Mathf.Infinity ;
					float yMax = - Mathf.Infinity ;

					l = tRectangle.Count ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						tP = tRectangle[ i ] ;

						if( tP.size.x >= 0 )
						{
							tSX = tP.size.x ;
							tRX = false ;
						}
						else
						{
							tSX = - tP.size.x ;
							tRX = true ;
						}

						if( tP.size.y >= 0 )
						{
							tSY = tP.size.y ;
							tRY = false ;
						}
						else
						{
							tSY = - tP.size.y ;
							tRY = true ;
						}

						tPX0 = tSX * (   - tP.pivot.x ) ;
						tPY0 = tSY * (   - tP.pivot.y ) ;
						tPX1 = tSX * ( 1 - tP.pivot.x ) ;
						tPY1 = tSY * ( 1 - tP.pivot.y ) ;

						tVx[ 0 ] = tPX0 ;
						tVy[ 0 ] = tPY0 ;

						tVx[ 1 ] = tPX0 ;
						tVy[ 1 ] = tPY1 ;

						tVx[ 2 ] = tPX1 ;
						tVy[ 2 ] = tPY1 ;

						tVx[ 3 ] = tPX1 ;
						tVy[ 3 ] = tPY0 ;

						if( tP.rotation != 0 )
						{
							// 回転
							tAngle = 2.0f * Mathf.PI * tP.rotation / 360.0f ;
	
							tCv = Mathf.Cos( tAngle ) ;
							tSv = Mathf.Sin( tAngle ) ;
	
							for( c  = 0 ; c <  4 ; c ++ )
							{
								vx = ( tVx[ c ] * tCv ) - ( tVy[ c ] * tSv ) ;
								vy = ( tVx[ c ] * tSv ) + ( tVy[ c ] * tCv ) ;
	
								tVx[ c ] = vx ;
								tVy[ c ] = vy ;
							}
						}

						tTx0 = tP.uv.x ;
						tTy0 = tP.uv.y ;
						tTx1 = tP.uv.x + tP.uv.width ;
						tTy1 = tP.uv.y + tP.uv.height ;

						if( tRX == true )
						{
							// 左右反転
							tx   = tTx0 ;
							tTx0 = tTx1 ;
							tTx1 = tx ;
						}

						if( tRY == true )
						{
							// 上下反転
							ty   = tTy0 ;
							tTy0 = tTy1 ;
							tTy1 = ty ;
						}

						tTx[ 0 ] = tTx0 ;
						tTy[ 0 ] = tTy0 ;
						tTx[ 1 ] = tTx0 ;
						tTy[ 1 ] = tTy1 ;
						tTx[ 2 ] = tTx1 ;
						tTy[ 2 ] = tTy1 ;
						tTx[ 3 ] = tTx1 ;
						tTy[ 3 ] = tTy0 ;

						o = aV.Count ;

						for( c  = 0 ; c <  4 ; c ++ )
						{
							v = new UIVertex() ;

							x = tVx[ c ] + tP.offset.x + m_Offset.x ;
							y = tVy[ c ] + tP.offset.y + m_Offset.y ;

							if( x <  xMin )
							{
								xMin  = x ;
							}
							else
							if( x >  xMax )
							{
								xMax  = x ;
							}
							if( y <  yMin )
							{
								yMin  = y ;
							}
							else
							if( y >  yMax )
							{
								yMax  = y ;
							}

							v.position	= new Vector3( x, y, 0 ) ;
							v.normal	= new Vector3(  0,  0, -1 ) ;
							v.color		= tP.vertexColor ;
							v.uv0		= new Vector2( tTx[ c ], tTy[ c ] ) ;

							aV.Add( v ) ;
						}

						aI.Add( o     ) ;
						aI.Add( o + 1 ) ;
						aI.Add( o + 2 ) ;

						aI.Add( o     ) ;
						aI.Add( o + 2 ) ;
						aI.Add( o + 3 ) ;
					}

					if( xMin <  0 )
					{
						xMin  = - xMin ;
					}

					if( xMax >= xMin )
					{
						m_PreferredWidth  = xMax * 2 ;
					}
					else
					{
						m_PreferredWidth  = xMin * 2 ;
					}

					if( yMin <  0 )
					{
						yMin  = - yMin ;
					}

					if( yMax >= yMin )
					{
						m_PreferredHeight = yMax * 2 ;
					}
					else
					{
						m_PreferredHeight = yMin * 2 ;
					}

					tHelper.AddUIVertexStream( aV, aI ) ;
				}
			}
		}


		// マテリアル更新
		protected override void UpdateMaterial()
		{
			base.UpdateMaterial() ;

			// テクスチャを更新する
			if( m_Texture != null )
			{
				CanvasRenderer.SetTexture( m_Texture ) ;
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

		public float preferredWidth
		{
			get
			{
				return m_PreferredWidth ;
			}
		}

		public float preferredHeight
		{
			get
			{
				return m_PreferredHeight ;
			}
		}
	}
}
