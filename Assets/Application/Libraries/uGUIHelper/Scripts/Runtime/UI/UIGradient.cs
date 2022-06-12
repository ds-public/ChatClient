using UnityEngine ;
using System.Collections.Generic ;
using UnityEngine.UI ;

namespace uGUIHelper
{
	/// <summary>
	/// 文字列にグラデーションを付与するコンポーネント
	/// </summary>
	public class UIGradient : BaseMeshEffect
	{
		public enum GeometoryTypes
		{
			Image,
			Text,
		}

		public enum DirectionTypes
		{
			Vertical,
			Horizontal,
			Both,
		}
		
		[SerializeField]
		protected GeometoryTypes m_GeometoryType = GeometoryTypes.Image ;
		public GeometoryTypes GeometoryType{ get{ return m_GeometoryType ; } set{ m_GeometoryType = value ; } }

		[SerializeField]
		protected DirectionTypes m_DirectionType = DirectionTypes.Vertical ;
		public DirectionTypes DirectionType{ get{ return m_DirectionType ; } set{ m_DirectionType = value ; } }

		[SerializeField]
		protected Color m_Top = Color.white ;
		public Color Top{ get{ return m_Top ; } set{ m_Top = value ; } }

		[SerializeField]
		protected Color m_Middle = Color.gray ;
		public Color Middle{ get{ return m_Middle ; } set{ m_Middle = value ; } }

		[SerializeField]
		protected Color m_Bottom = Color.black ;
		public Color Bottom{ get{ return m_Bottom ; } set{ m_Bottom = value ; } }

		[SerializeField]
		protected float m_PivotMiddle = 0.5f ;
		public float PivotMiddle{ get{ return m_PivotMiddle ; } set{ m_PivotMiddle = value ; } }

		[SerializeField]
		protected Color m_Left = Color.red ;
		public Color Left{ get{ return m_Left ; } set{ m_Left = value ; } }

		[SerializeField]
		protected Color m_Center = Color.green ;
		public Color Center{ get{ return m_Center ; } set{ m_Center = value ; } }

		[SerializeField]
		protected Color m_Right = Color.blue ;
		public Color Right{ get{ return m_Right ; } set{ m_Right = value ; } }

		[SerializeField]
		protected float m_PivotCenter = 0.5f ;
		public float PivotCenter{ get{ return m_PivotCenter ; } set{ m_PivotCenter = value ; } }
		
		//---------------------------------------------------------------------------

		public override void ModifyMesh( VertexHelper helper )
		{
			if( IsActive() == false )
			{
				return ;
			}
		
			List<UIVertex> list = new List<UIVertex>() ;
			helper.GetUIVertexStream( list ) ;
		
			ModifyVertices( list ) ;
		
			helper.Clear() ;
			helper.AddUIVertexTriangleStream( list ) ;
		}
	
		private void ModifyVertices( List<UIVertex> list )
		{
			if( IsActive() == false || list == null || list.Count == 0 )
			{
				return ;
			}
			
			UIVertex v ;

			if( m_GeometoryType == GeometoryTypes.Image )
			{
				// イメージのケース

				// 頂点の最少値と最大値を抽出する
				float maxX = - Mathf.Infinity, maxY = - Mathf.Infinity, minX = Mathf.Infinity, minY = Mathf.Infinity ;
				
				for( int i  = 0 ; i <  list.Count ; i ++ )
				{
					v = list[ i ] ;
					minX = Mathf.Min( minX, v.position.x ) ;
					minY = Mathf.Min( minY, v.position.y ) ;
					maxX = Mathf.Max( maxX, v.position.x ) ;
					maxY = Mathf.Max( maxY, v.position.y ) ;
				}
		
				float w = maxX - minX ;
				float h = maxY - minY ;
		
				// 頂点ごとの色を調整する
				Color colorO ;

				Color colorH ;
				Color colorV ;

				Color colorM = Color.white ;

				float xa, ya ;

				for( int i  = 0 ; i <  list.Count ; i ++ )
				{
					v = list[ i ] ;

					colorO = v.color ;	// 指定のテキストカラー

					xa = ( v.position.x - minX ) / w ;	// 横位置
					if( xa <  m_PivotCenter )
					{
						colorH = Color.Lerp( m_Left,		m_Center,	xa / m_PivotCenter ) ;
					}
					else
					if( xa >  m_PivotCenter )
					{
						colorH = Color.Lerp( m_Center,	m_Right,	( xa - m_PivotCenter ) / ( 1.0f - m_PivotCenter ) ) ;
					}
					else
					{
						colorH = m_Center ;
					}

					ya = ( v.position.y - minY ) / h ;	// 縦位置
					if( ya <  m_PivotMiddle )
					{
						colorV = Color.Lerp( m_Bottom,	m_Middle,	ya / m_PivotMiddle ) ;
					}
					else
					if( ya >  m_PivotMiddle )
					{
						colorV = Color.Lerp( m_Middle,	Top,	( ya - m_PivotMiddle ) / ( 1.0f - m_PivotMiddle ) ) ;
					}
					else
					{
						colorV = m_Middle ;
					}

					switch( m_DirectionType )
					{
						case DirectionTypes.Horizontal :
							colorM = colorH ;
						break ;

						case DirectionTypes.Vertical :
							colorM = colorV ;
						break ;

						case DirectionTypes.Both :
							colorM = colorV * colorH ;
						break ;
					}

					v.color = colorO * colorM ;
	
					list[ i ] = v ;
				}
			}
			else
			if( m_GeometoryType == GeometoryTypes.Text )
			{
				// テキストのケース

				int N = list.Count / 6 ;	// １文字あたり６頂点
				int o ;

				for( int n  = 0 ; n <  N ; n ++ )
				{
					o = n * 6 ;
					
					if( m_DirectionType == DirectionTypes.Horizontal )
					{
						v = list[ o + 0 ] ;
						v.color *= m_Left ;
						list[ o + 0 ] = v ;

						v = list[ o + 1 ] ;
						v.color *= m_Right ;
						list[ o + 1 ] = v ;

						v = list[ o + 2 ] ;
						v.color *= m_Right ;
						list[ o + 2 ] = v ;

						v = list[ o + 3 ] ;
						v.color *= m_Right ;
						list[ o + 3 ] = v ;

						v = list[ o + 4 ] ;
						v.color *= m_Left ;
						list[ o + 4 ] = v ;

						v = list[ o + 5 ] ;
						v.color *= m_Left ;
						list[ o + 5 ] = v ;
					}
					else
					if( m_DirectionType == DirectionTypes.Vertical )
					{
						v = list[ o + 0 ] ;
						v.color *= m_Top ;
						list[ o + 0 ] = v ;

						v = list[ o + 1 ] ;
						v.color *= m_Top ;
						list[ o + 1 ] = v ;

						v = list[ o + 2 ] ;
						v.color *= m_Bottom ;
						list[ o + 2 ] = v ;

						v = list[ o + 3 ] ;
						v.color *= m_Bottom ;
						list[ o + 3 ] = v ;

						v = list[ o + 4 ] ;
						v.color *= m_Bottom ;
						list[ o + 4 ] = v ;

						v = list[ o + 5 ] ;
						v.color *= m_Top ;
						list[ o + 5 ] = v ;
					}
					else
					if( m_DirectionType == DirectionTypes.Both )
					{
						v = list[ o + 0 ] ;
						v.color *= ( m_Left * m_Top ) ;
						list[ o + 0 ] = v ;

						v = list[ o + 1 ] ;
						v.color *= ( m_Right * m_Top ) ;
						list[ o + 1 ] = v ;

						v = list[ o + 2 ] ;
						v.color *= ( m_Right * m_Bottom ) ;
						list[ o + 2 ] = v ;

						v = list[ o + 3 ] ;
						v.color *= ( m_Right * m_Bottom ) ;
						list[ o + 3 ] = v ;

						v = list[ o + 4 ] ;
						v.color *= ( m_Left * m_Bottom ) ;
						list[ o + 4 ] = v ;

						v = list[ o + 5 ] ;
						v.color *= ( m_Left * m_Top ) ;
						list[ o + 5 ] = v ;
					}
				}
			}
		}
	
		public void Refresh()
		{
			if( graphic != null )
			{
				graphic.SetVerticesDirty() ;
			}
		}
	}
}
