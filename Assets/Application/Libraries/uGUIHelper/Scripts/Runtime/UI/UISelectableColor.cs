using UnityEngine ;
using System.Collections.Generic ;
using UnityEngine.UI ;

namespace uGUIHelper
{
	/// <summary>
	/// 文字列に現在のインタラクションの状態を反映させるコンポーネント
	/// </summary>
	public class UISelectableColor : BaseMeshEffect
	{
		/// <summary>
		/// 対象の状態が変化するコンポーネントのインスタンス
		/// </summary>
		public Selectable	Target ;

		private bool		m_Interactable = false ;

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
			if( IsActive() == false || list == null || list.Count == 0 || Target == null )
			{
				return ;
			}
			
			UIVertex v ;

			Color color ;

			if( Target.interactable == true )
			{
				color = Target.colors.normalColor ;
			}
			else
			{
				color = Target.colors.disabledColor ;
			}

			// 全頂点の色を補正する
			for( int i  = 0 ; i <  list.Count ; i ++ )
			{
				v = list[ i ] ;
				v.color *= color ;	// 指定のテキストカラー
				list[ i ] = v ;
			}
		}
	
		public void Refresh()
		{
			if( graphic != null )
			{
				graphic.SetVerticesDirty() ;
			}
		}

		override protected void Start()
		{
			base.Start() ;

			Refresh() ;
			m_Interactable = Target.interactable ;
		}

		public void Update()
		{
			if( Target != null )
			{
				if( m_Interactable != Target.interactable )
				{
					Refresh() ;
					m_Interactable  = Target.interactable ;
				}
			}
		}
	}
}
