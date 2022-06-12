using System.Collections.Generic ;
using UnityEngine ;
using UnityEngine.UI ;

namespace uGUIHelper
{
	/// <summary>
	/// 拡張:サークル
	/// </summary>
	public class Circle : MaskableGraphicWrapper
	{
		/// <summary>
		/// スプライト
		/// </summary>
		[SerializeField][HideInInspector]
		private Sprite m_Sprite = null ;

		private Texture2D m_BlankTexture = null ;

		public  Sprite  Sprite
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

		public    Color  InnerColor
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

		public    Color  OuterColor
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
		/// 分割数
		/// </summary>
		[HideInInspector][SerializeField]
		protected int m_Split = 36 ;

		public    int  Split
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
		/// 内側の塗りつぶしの有無
		/// </summary>
		[HideInInspector][SerializeField]
		protected bool m_FillInner  = true ;

		public    bool  FillInner
		{
			get
			{
				return m_FillInner ;
			}
			set
			{
				if( m_FillInner != value )
				{
					m_FillInner = value ;

					SetVerticesDirty() ;
				}
			}
		}

		/// <summary>
		/// 外周の太さ(塗りつぶし無し限定)
		/// </summary>
		[HideInInspector][SerializeField]
		protected float m_LineWidth =  1 ;

		public    float  LineWidth
		{
			get
			{
				return m_LineWidth ;
			}
			set
			{
				if( m_LineWidth != value && value >  0 )
				{
					m_LineWidth = value ;

					SetVerticesDirty() ;
				}
			}
		}

		/// <summary>
		/// テクスチャの張り方(塗りつぶし有り限定)
		/// </summary>
		public enum DecalTypes
		{
			Normal = 0,
			Effect = 1,
		}

		[HideInInspector][SerializeField]
		protected DecalTypes m_DecalType = DecalTypes.Normal ;

		public    DecalTypes  DecalType
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

//		[HideInInspector][SerializeField]
		[SerializeField]
		protected float[] m_VertexDistanceScales ;

		/// <summary>
		/// 頂点ごとのスケール
		/// </summary>
		public float[] VertexDistanceScales
		{
			get
			{
				return m_VertexDistanceScales ;
			}
			set
			{
				m_VertexDistanceScales = value ;
			}
		}

		/// <summary>
		/// 頂点ごとのスケールを設定する
		/// </summary>
		/// <param name="vertexDistanceScales"></param>
		public void SetVertexDistanceScales( float[] vertexDistanceScales )
		{
			m_VertexDistanceScales = vertexDistanceScales ;
		}

		//----------------------------------------------------------

//		protected override void OnValidate()
//		{
//			base.OnValidate() ;
//		}

		// メッシュ更新
		protected override void OnPopulateMesh( VertexHelper helper )
		{
			if( IsActive() == false )
			{
				return ;
			}

			helper.Clear() ;

			//-----------------------------------------------------------

			// 実際に頂点バッファを生成する

			List<UIVertex>	aV = new List<UIVertex>() ;
			List<int>		aI = new List<int>() ;


			Vector2 size = Size ;
			Vector2 pivot = RectTransform.pivot ;

			UIVertex v ;

			int i, j ;
			float a ;
			float vds ;

			Vector2 p = Vector2.zero ;

			float vrw = size.x * 0.5f ;
			float vrh = size.y * 0.5f ;
			float vcx = size.x * ( 0.5f - pivot.x ) ;
			float vcy = size.y * ( 0.5f - pivot.y ) ;
			
			float tfw = 0 ;
			float tfh = 0 ;

			float trw = 0 ;
			float trh = 0 ;
			float tcx = 0 ;
			float tcy = 0 ;

			if( m_Sprite != null )
			{
				tfw = m_Sprite.texture.width ;
				tfh = m_Sprite.texture.height ;

				trw = m_Sprite.rect.width  * 0.5f ;
				trh = m_Sprite.rect.height * 0.5f ;

				tcx = m_Sprite.rect.x + trw ;
				tcy = m_Sprite.rect.y + trh ;
			}

			Vector3 normalVector = new Vector3(  0,  0, -1 ) ;

			//-----------------------------------------

			if( m_FillInner == false )
			{
				// 中心の塗りつぶしは無し
			
				float r ;
			
				Vector2 po = Vector2.zero ;
				Vector2 pi ;
				
				Vector2[] aVo = new Vector2[ m_Split ] ;
				Vector2[] aVi = new Vector2[ m_Split ] ;
						
				Vector2[] aTo = null ;
				Vector2[] aTi = null ;

				if( m_Sprite != null )
				{
					aTo = new Vector2[ m_Split ] ;
					aTi = new Vector2[ m_Split ] ;
				}

				// 頂点・ＵＶ情報を整理する
				for( i  = 0 ; i <  m_Split ; i ++ )
				{
					a = 2.0f * Mathf.PI * ( float )i / ( float )m_Split ;
				
					// 上を頂点開始地点にする(時計回り)
					p.x = Mathf.Sin( a ) ;
					p.y = Mathf.Cos( a ) ;

					vds = 1 ;
					if( m_VertexDistanceScales != null && i <  m_VertexDistanceScales.Length )
					{
						vds = m_VertexDistanceScales[ i ] ;
					}

					po.x = p.x * vrw * vds ;
					po.y = p.y * vrh * vds ;
					
					pi = po - ( p.normalized * m_LineWidth ) ;

					aVo[ i ] = new Vector2( po.x + vcx, po.y + vcy ) ;	// ９０度回転させてＹ符号反転
					aVi[ i ] = new Vector2( pi.x + vcx, pi.y + vcy ) ;

					if( m_Sprite != null )
					{
						r = pi.magnitude / po.magnitude ;
						
						aTo[ i ] = new Vector2( p.x * 1 * trw + tcx, p.y * 1 * trh + tcy ) ;
						aTi[ i ] = new Vector2( p.x * r * trw + tcx, p.y * r * trh + tcy ) ;
					}
				}
				
				//-------------------------------------------------
				
				// 実際に頂点データを生成する

				int vi ;
			
				for( i  = 0 ; i <  m_Split ; i ++ )
				{
					vi = aV.Count ;
					
					v = new UIVertex() ;

					j = ( i + 1 ) % m_Split ;

					// １点目
					v.position	= new Vector3( aVo[ i ].x, aVo[ i ].y, 0 ) ;
					v.normal	= normalVector ;
					v.color		= m_OuterColor ;

					if( m_Sprite != null )
					{
						v.uv0	= new Vector2( aTo[ i ].x / tfw, aTo[ i ].y / tfh ) ;
					}

					aV.Add( v ) ;

					// ２点目
					v.position	= new Vector3( aVo[ j ].x, aVo[ j ].y, 0 ) ;
					v.normal	= normalVector ;
					v.color		= m_OuterColor ;

					if( m_Sprite != null )
					{
						v.uv0	= new Vector2( aTo[ j ].x / tfw, aTo[ j ].y / tfh ) ;
					}

					aV.Add( v ) ;

					// ３点目
					v.position	= new Vector3( aVi[ i ].x, aVi[ i ].y, 0 ) ;
					v.normal	= normalVector ;
					v.color		= m_InnerColor ;

					if( m_Sprite != null )
					{
						v.uv0	= new Vector2( aTi[ i ].x / tfw, aTi[ i ].y / tfh ) ;
					}

					aV.Add( v ) ;

					// ４点目
					v.position	= new Vector3( aVi[ j ].x, aVi[ j ].y, 0 ) ;
					v.normal	= normalVector ;
					v.color		= m_InnerColor ;

					if( m_Sprite != null )
					{
						v.uv0	= new Vector2( aTi[ j ].x / tfw, aTi[ j ].y / tfh ) ;
					}

					aV.Add( v ) ;

					//インデックス
				
					aI.Add( vi + 0 ) ;
					aI.Add( vi + 1 ) ;
					aI.Add( vi + 2 ) ;

					aI.Add( vi + 1 ) ;
					aI.Add( vi + 3 ) ;
					aI.Add( vi + 2 ) ;
				}
			}
			else
			{
				// 中心の塗りつぶしは有り
			
				Vector2[] aVf = new Vector2[ m_Split ] ;
			
				Vector2[] aTf = null ;

				if( m_Sprite != null && m_DecalType == DecalTypes.Normal )
				{
					aTf = new Vector2[ m_Split ] ;
				}

				// 頂点・ＵＶ情報を整理する
				for( i  = 0 ; i <  m_Split ; i ++ )
				{
					a = 2.0f * Mathf.PI * ( float )i / ( float )m_Split ;
				
					// 上を頂点開始地点にする(時計回り)
					p.x = Mathf.Sin( a ) ;
					p.y = Mathf.Cos( a ) ;

					vds = 1 ;
					if( m_VertexDistanceScales != null && i <  m_VertexDistanceScales.Length )
					{
						vds = m_VertexDistanceScales[ i ] ;
					}

					aVf[ i ] = new Vector2( p.x * vrw * vds + vcx, p.y * vrh * vds + vcy ) ;	// ９０度回転させてＹ符号反転

					if( m_Sprite != null && m_DecalType == DecalTypes.Normal )
					{
						aTf[ i ] = new Vector2( p.x * trw + tcx, p.y * trh + tcy ) ;
					}
				}
			
				//-------------------------------------------------
				
				// 実際に頂点データを生成する(２タイプから選択可能)

				int vi ;
			
				if( m_DecalType == DecalTypes.Normal )
				{
					// テクスチャの張り方：通常

					// 中心
					v = new UIVertex()
					{
						position	= new Vector3( vcx, vcy, 0 ),
						normal		= normalVector,
						color		= m_InnerColor
					} ;

					if( m_Sprite != null )
					{
						v.uv0		= new Vector2( tcx / tfw, tcy / tfh ) ;
					}

					aV.Add( v ) ;

					for( i  = 0 ; i <  m_Split ; i ++ )
					{
						vi = aV.Count ;
				
						j = ( i + 1 ) % m_Split ;

						// １点目
						v = new UIVertex()
						{
							position	= new Vector3( aVf[ i ].x, aVf[ i ].y, 0 ),
							normal		= normalVector,
							color		= m_OuterColor
						} ;

						if( m_Sprite != null )
						{
							v.uv0	= new Vector2( aTf[ i ].x / tfw, aTf[ i ].y / tfh ) ;
						}

						aV.Add( v ) ;

						// ２点目
						v.position	= new Vector3( aVf[ j ].x, aVf[ j ].y, 0 ) ;
						v.normal	= normalVector ;
						v.color		= m_OuterColor ;

						if( m_Sprite != null )
						{
							v.uv0	= new Vector2( aTf[ j ].x / tfw, aTf[ j ].y / tfh ) ;
						}

						aV.Add( v ) ;

						// インデックス
						aI.Add( 0 ) ;
						aI.Add( vi + 0 ) ;
						aI.Add( vi + 1 ) ;
					}
				}
				else
				if( m_DecalType == DecalTypes.Effect )
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
						tx0 = tcx					/ tfw ;
						ty0 = m_Sprite.rect.yMax	/ tfh ;

						tx1 = m_Sprite.rect.xMin	/ tfw ;
						ty1 = m_Sprite.rect.yMin	/ tfh ;

						tx2 = m_Sprite.rect.xMax	/ tfw ;
						ty2 = m_Sprite.rect.yMin	/ tfh ;
					}

					for( i  = 0 ; i <  m_Split ; i ++ )
					{
						vi = aV.Count ;
				
						j = ( i + 1 ) % m_Split ;

						// １点目
						v = new UIVertex()
						{
							position	= new Vector3( vcx, vcy, 0 ),
							normal		= normalVector,
							color		= m_InnerColor
						} ;

						if( m_Sprite != null )
						{
							v.uv0	= new Vector2( tx0, ty0 ) ;
						}

						aV.Add( v ) ;

						// ２点目
						v = new UIVertex()
						{
							position	= new Vector3( aVf[ i ].x, aVf[ i ].y, 0 ),
							normal		= normalVector,
							color		= m_OuterColor
						} ;

						if( m_Sprite != null )
						{
							v.uv0	= new Vector2( tx1, ty1 ) ;
						}

						aV.Add( v ) ;

						// ３点目
						v.position	= new Vector3( aVf[ j ].x, aVf[ j ].y, 0 ) ;
						v.normal	= normalVector ;
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
			}


			if( aV.Count >  0 && aI.Count >  0 )
			{
				helper.AddUIVertexStream( aV, aI ) ;
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
