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
	public class GridMap : MaskableGraphicWrapper
	{
		//----------------------------------------------------------

		[SerializeField][HideInInspector]
		private int m_VertexHorizontalGrid = 1 ;
		public  int   vertexHorizontalGrid
		{
			get
			{
				return m_VertexHorizontalGrid ;
			}
			set
			{
				if( m_VertexHorizontalGrid != value )
				{
					m_VertexHorizontalGrid  = value ;
					ModifyData() ;
					SetVerticesDirty() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private int m_VertexVerticalGrid = 1 ;
		public  int   vertexVerticalGrid
		{
			get
			{
				return m_VertexVerticalGrid ;
			}
			set
			{
				if( m_VertexVerticalGrid != value )
				{
					m_VertexVerticalGrid  = value ;
					ModifyData() ;
					SetVerticesDirty() ;
				}
			}
		}

		// データ配列を作り直す
		private void ModifyData()
		{
			int[] tData = new int[ m_VertexVerticalGrid * m_VertexHorizontalGrid ] ;

			int i, l = data.Length ;
			if( l >  tData.Length )
			{
				l  = tData.Length ;
			}
			for( i  = 0 ; i <  l ; i ++ )
			{
				tData[ i ] = data[ i ] ;
			}
			data = tData ;
		}


		public enum VertexDensity
		{
			Low  = 5,	// セル：５頂点・４ポリゴン
			High = 9,	// セル：９頂点・８ポリゴン
		}

		[SerializeField][HideInInspector]
		private VertexDensity m_VertexDensity = VertexDensity.High ;
		public  VertexDensity  vertexDensity
		{
			get
			{
				return m_VertexDensity ;
			}
			set
			{
				if( m_VertexDensity != value )
				{
					m_VertexDensity  = value ;
					SetVerticesDirty() ;
				}
			}
		}

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

		[SerializeField][HideInInspector]
		private int m_TextureHorizontalGrid = 1 ;
		public  int   textureHorizontalGrid
		{
			get
			{
				return m_TextureHorizontalGrid ;
			}
			set
			{
				if( m_TextureHorizontalGrid != value )
				{
					m_TextureHorizontalGrid  = value ;
					SetVerticesDirty() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private int m_TextureVerticalGrid = 1 ;
		public  int   textureVerticalGrid
		{
			get
			{
				return m_TextureVerticalGrid ;
			}
			set
			{
				if( m_TextureVerticalGrid != value )
				{
					m_TextureVerticalGrid  = value ;
					SetVerticesDirty() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private int m_TextureGridPadding = 0 ;
		public  int   textureGridPadding
		{
			get
			{
				return m_TextureGridPadding ;
			}
			set
			{
				if( m_TextureGridPadding != value )
				{
					m_TextureGridPadding  = value ;
					SetVerticesDirty() ;
				}
			}
		}





		/// <summary>
		/// マップデータ
		/// </summary>
		public int[] data = new int[ 1 ] ;

		/// <summary>
		/// マップデータを設定する
		/// </summary>
		/// <param name="tMap"></param>
		public void SetData( int[,] tData )
		{
			if( data == null || data.Length == 0 || tData == null )
			{
				SetVerticesDirty() ;
				return ;
			}

			int w = tData.GetLength( 1 ) ;
			int h = tData.GetLength( 0 ) ;
			if( w == 0 || h == 0 )
			{
				SetVerticesDirty() ;
				return ;
			}

			int l = data.Length ;

			int i, x, y ;

			for( y  = 0 ; y <  h ; y ++ )
			{
				for( x  = 0 ; x <  w ; x ++ )
				{
					i = y * w + x ;
					if( i >= l )
					{
						return ;
					}

					data[ i ] = tData[ y, x ] ;
				}
			}

			SetVerticesDirty() ;
		}


		public enum TransitionTypes
		{
			None				= -1,
			CircleInToOut		=  0,
			CircleOutToIn		=  1,
			RectangleInToOut	=  2,
			RectangleOutToIn	=  3,
			LeftToRight			=  4,
			RightToLeft			=  5,
			TopToBottom			=  6,
			BottomToTop			=  7,
		}

		[SerializeField][HideInInspector]
		private TransitionTypes m_TransitionType = TransitionTypes.None ;
		public  TransitionTypes  transitionType
		{
			get
			{
				return m_TransitionType ;
			}
			set
			{
				if( m_TransitionType != value )
				{
					m_TransitionType  = value ;
					SetVerticesDirty() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private float m_TransitionFactor = 1 ;
		public  float  transitionFactor
		{
			get
			{
				return m_TransitionFactor ;
			}
			set
			{
				if( m_TransitionFactor != value )
				{
					m_TransitionFactor  = value ;
					SetVerticesDirty() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private float m_TransitionIntensity = 0.1f ;
		public  float  transitionIntensity
		{
			get
			{
				return m_TransitionIntensity ;
			}
			set
			{
				if( m_TransitionIntensity != value )
				{
					m_TransitionIntensity  = value ;
					SetVerticesDirty() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private bool m_TransitionReverse = false ;
		public  bool  transitionReverse
		{
			get
			{
				return m_TransitionReverse ;
			}
			set
			{
				if( m_TransitionReverse != value )
				{
					m_TransitionReverse  = value ;
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

			//-----------------------------------------

			List<UIVertex>	aV = new List<UIVertex>() ;
			List<int>		aI = new List<int>() ;

			Vector2 tSize  = Size ;
			Vector2 tPivot = RectTransform.pivot ;

			float sx = - ( tSize.x * tPivot.x ) ;
			float sy =   ( tSize.y * ( 1.0f - tPivot.y ) ) ;

			int vgx = m_VertexHorizontalGrid ;
			int vgy = m_VertexVerticalGrid ;

			bool tDataEnabled = true ;
			int tgx = m_TextureHorizontalGrid ;
			int tgy = m_TextureVerticalGrid ;
			if( tgx == 0 || tgy == 0 )
			{
				tDataEnabled = false ;
				tgx = vgx ;
				tgy = vgy ;
			}

			int lx, ly, ix, iy, o = 0 ;
			UIVertex v ;

			float cr = 1, cg = 1, cb = 1, ca = 1, fa = 1 ;

			float hx = tSize.x * 0.5f ;
			float hy = tSize.y * 0.5f ;

			float cx = sx + hx ;
			float cy = sy - hy ;

			float vx, vy, vz ;
			float nx, ny, nz ;
			float tx, ty ;

			nx = 0 ; ny = 0 ; nz = -1 ;

			float vdx  = tSize.x / ( float )vgx ;
			float vdy  = tSize.y / ( float )vgy ;
			float vdhx = vdx * 0.5f ;
			float vdhy = vdy * 0.5f ;


			float tTW = 1.0f ;
			float tTH = 1.0f ;
			float tTP = 0.0f ;
			if( texture != null )
			{
				tTW = texture.width ;
				tTH = texture.height ;
				tTP = m_TextureGridPadding ;
			}

			float tdx  = tTW / ( float )tgx ;
			float tdy  = tTH / ( float )tgy ;
			float tdhx = tdx * 0.5f ;
			float tdhy = tdy * 0.5f ;

			float tpx ;
			float tpy ;

			int ci ;

			// １つのセルにつき５頂点で３角は４つ（ＵＶは共有出来ない）
			for( ly  = 0 ; ly <  vgy ; ly ++ )
			{
				for( lx  = 0 ; lx <  vgx ; lx ++ )
				{
					// セル
					ci = 0 ;
					for( iy  = 0 ; iy <= 2 ; iy ++ )
					{
						for( ix  = 0 ; ix <= 2 ; ix ++ )
						{
							if( ( m_VertexDensity == VertexDensity.Low && ( ci == 0 || ci == 2 || ci == 4 || ci == 6 || ci == 8 ) ) || ( m_VertexDensity == VertexDensity.High ) )
							{
								v = new UIVertex() ;

								// 頂点
								vx = sx + ( vdx * ( float )lx + vdhx * ( float )ix ) ;
								vy = sy - ( vdy * ( float )ly + vdhy * ( float )iy ) ;
								vz = 0 ;  
								v.position = new Vector3( vx, vy, vz ) ;

								v.normal   = new Vector3( nx, ny, nz ) ;


								// トランジションのタイプによってα値をコントロールする
								fa = 1 ;
							
								if( m_TransitionType == TransitionTypes.CircleInToOut || m_TransitionType == TransitionTypes.CircleOutToIn )
								{
									// 中心から１が広がっていく(円)
									float px =     ( vx - cx ) / hx   ;	// -1 ～ 0 ～ +1
									float py = - ( ( vy - cy ) / hy ) ;	// -1 ～ 0 ～ +1

									float pv = new Vector2( px, py ).magnitude ;

									if( m_TransitionType == TransitionTypes.CircleOutToIn )
									{
										pv = 1.5f - pv ;
									}

									float cp =  m_TransitionFactor - m_TransitionIntensity ;

									if( pv >= m_TransitionFactor )
									{
										fa = 0 ;
									}
									else
									if( pv <  m_TransitionFactor && pv >  cp )
									{
										fa = 1.0f - ( ( pv - cp ) / m_TransitionIntensity ) ;
									}
									else
									if( pv <= cp )
									{
										fa = 1.0f ;
									}
								}
								else
								if( m_TransitionType == TransitionTypes.RectangleInToOut || m_TransitionType == TransitionTypes.RectangleOutToIn )
								{
									// 中心から１が広がっていく(四角)
									float px =     ( vx - cx ) / hx   ;	// -1 ～ 0 ～ +1
									float py = - ( ( vy - cy ) / hy ) ;	// -1 ～ 0 ～ +1


									if( px <  0 )
									{
										px = - px ;
									}
									if( py <  0 )
									{
										py = - py ;
									}
									float pv ;
									if( px >= py )
									{
										pv =  px ;
									}
									else
									{
										pv =  py ;
									}

									if( m_TransitionType == TransitionTypes.RectangleOutToIn )
									{
										pv = 1.5f - pv ;
									}

									float cp =  m_TransitionFactor - m_TransitionIntensity ;

									if( pv >= m_TransitionFactor )
									{
										fa = 0 ;
									}
									else
									if( pv <  m_TransitionFactor && pv >  cp )
									{
										fa = 1.0f - ( ( pv - cp ) / m_TransitionIntensity ) ;
									}
									else
									if( pv <= cp )
									{
										fa = 1.0f ;
									}
								}
								else
								if( m_TransitionType == TransitionTypes.LeftToRight || m_TransitionType == TransitionTypes.RightToLeft  || m_TransitionType == TransitionTypes.TopToBottom || m_TransitionType == TransitionTypes.BottomToTop )
								{
									// 左右上下
									float px =     ( vx - cx ) / hx   ;	// -1 ～ 0 ～ +1
									float py = - ( ( vy - cy ) / hy ) ;	// -1 ～ 0 ～ +1

									float pv = 0 ;

									if( m_TransitionType == TransitionTypes.LeftToRight )
									{
										// 左→右
										pv = 0.5f + px * 0.5f ;
									}
									else
									if( m_TransitionType == TransitionTypes.RightToLeft )
									{
										// 右→左
										pv = 0.5f - px * 0.5f ;
									}
									else
									if( m_TransitionType == TransitionTypes.TopToBottom )
									{
										// 上→下
										pv = 0.5f + py * 0.5f ;
									}
									else
									if( m_TransitionType == TransitionTypes.BottomToTop )
									{
										// 下→上
										pv = 0.5f - py * 0.5f ;
									}

									float cp =  m_TransitionFactor - m_TransitionIntensity ;

									if( pv >= m_TransitionFactor )
									{
										fa = 0 ;
									}
									else
									if( pv <  m_TransitionFactor && pv >  cp )
									{
										fa = 1.0f - ( ( pv - cp ) / m_TransitionIntensity ) ;
									}
									else
									if( pv <= cp )
									{
										fa = 1 ;
									}
								}

								if( fa <  0 )
								{
									fa  = 0 ;
								}
								else
								if( fa >  1 )
								{
									fa  = 1 ;
								}

								if( m_TransitionReverse == true )
								{
									fa = 1.0f - fa ;
								}

								v.color    = new Color( cr, cg, cb, ca * fa ) ;

								if( tDataEnabled == false || data == null )
								{
									tx = ( float )lx ;
									ty = ( float )ly ;
								}
								else
								{
									int i = ly * vgx + lx ;
									if( i >= data.Length )
									{
										i  = data.Length - 1 ;
									}

									i = data[ i ] ;
									if( i >= ( tgx * tgy ) )
									{
										i  = tgx * tgy - 1 ;
									}
									tx = ( float )( i % tgx ) ;
									ty = ( float )( i / tgx ) ;
								}

								tpx = 0 ;
								if( ix == 0 )
								{
									tpx =   tTP ;
								}
								else
								if( ix == 2 )
								{
									tpx = - tTP ;
								}

								tpy = 0 ;
								if( iy == 0 )
								{
									tpy =   tTP ;
								}
								else
								if( iy == 2 )
								{
									tpy = - tTP ;
								}
								tx =        (   tdx * tx + tdhx * ( float )ix   + tpx ) / tTW ;
								ty = 1.0f - ( ( tdy * ty + tdhy * ( float )iy ) + tpy ) / tTW ;
								v.uv0      = new Vector2( tx, ty ) ;

								aV.Add( v ) ;
							}
							ci ++ ;
						}
					}

					// インデックス

					if( m_VertexDensity == VertexDensity.Low )
					{
						// ５頂点・４ポリゴン

						// 上の三角
						aI.Add( o + 0 ) ;
						aI.Add( o + 1 ) ;
						aI.Add( o + 2 ) ;
					
						// 左の三角
						aI.Add( o + 0 ) ;
						aI.Add( o + 2 ) ;
						aI.Add( o + 3 ) ;

						// 右の三角
						aI.Add( o + 1 ) ;
						aI.Add( o + 4 ) ;
						aI.Add( o + 2 ) ;

						// 下の三角
						aI.Add( o + 2 ) ;
						aI.Add( o + 4 ) ;
						aI.Add( o + 3 ) ;

						o = o + 5 ;
					}
					else
					if( m_VertexDensity == VertexDensity.High )
					{
						// ９頂点・８ポリゴン

						// 上の三角

						aI.Add( o + 0 ) ;
						aI.Add( o + 1 ) ;
						aI.Add( o + 4 ) ;

						aI.Add( o + 1 ) ;
						aI.Add( o + 2 ) ;
						aI.Add( o + 4 ) ;

						// 左の三角

						aI.Add( o + 0 ) ;
						aI.Add( o + 4 ) ;
						aI.Add( o + 3 ) ;

						aI.Add( o + 3 ) ;
						aI.Add( o + 4 ) ;
						aI.Add( o + 6 ) ;

						// 右の三角

						aI.Add( o + 2 ) ;
						aI.Add( o + 5 ) ;
						aI.Add( o + 4 ) ;

						aI.Add( o + 5 ) ;
						aI.Add( o + 8 ) ;
						aI.Add( o + 4 ) ;

						// 下の三角

						aI.Add( o + 8 ) ;
						aI.Add( o + 7 ) ;
						aI.Add( o + 4 ) ;

						aI.Add( o + 7 ) ;
						aI.Add( o + 6 ) ;
						aI.Add( o + 4 ) ;


						o = o + 9 ;
					}
				}
			}

			tHelper.AddUIVertexStream( aV, aI ) ;
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
	}
}
