using UnityEngine ;
using System.Collections.Generic ;

namespace UnityEngine.UI
{
	/// <summary>
	/// 文字単位で色を変える(指定したインデックスから配列数分の色を変える
	/// </summary>
	public class TextColorModifier : BaseMeshEffect
	{
		[SerializeField]
		private int		m_Index = 0 ;

		[SerializeField]
		private Color[]	m_Color = null ;

		//---------------------------------------------------------------------------

		public int		index
		{
			get
			{
				return m_Index ;
			}
			set
			{
				if( m_Index != value )
				{
					m_Index  = value ;
					Refresh() ;
				}
			}
		}

		public Color[]	color
		{
			get
			{
				return m_Color ;
			}
			set
			{
				if( m_Color != value )
				{
					m_Color  = value ;
					Refresh() ;
				}
			}
		}

		//---------------------------------------------------------------------------

		public override void ModifyMesh( VertexHelper tHelper )
		{
			if( IsActive() == false )
			{
				return ;
			}
		
			List<UIVertex> tList = new List<UIVertex>() ;
			tHelper.GetUIVertexStream( tList ) ;
		
			ModifyVertices( tList ) ;
		
			tHelper.Clear() ;
			tHelper.AddUIVertexTriangleStream( tList ) ;
		}
	
		private void ModifyVertices( List<UIVertex> tList )
		{
			if( IsActive() == false || tList == null || tList.Count == 0 || m_Color == null || m_Color.Length == 0 )
			{
				return ;
			}
			
			//----------------------------------------------------------

			// 文字数
			int N = tList.Count / 6 ;	// １文字あたり６頂点

			if( m_Index >= N )
			{
				return ;	// 対象外
			}

			int o = m_Index, l = m_Color.Length ;

			// index はマイナス値の指定も可能
			if( o <  0 )
			{
				l += o ;
				if( l <= 0 )
				{
					return ;	// 対象外
				}
				o = 0 ;
			}

			if( ( o + l ) >  N )
			{
				l  = N - o ;
			}

			// l は最低でも 1 にはなっている

			//----------------------------------

			UIVertex v ;
			Color c ;
			int i, j, p ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				c = m_Color[ i ] ;

				p = ( o + i ) * 6 ;
				for( j  = 0 ; j <  6 ; j ++ )
				{
					v = tList[ p + j ] ;
					v.color = c ;
					tList[ p + j ] = v ;
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

