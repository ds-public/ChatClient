using UnityEngine ;
using System.Collections.Generic ;
using UnityEngine.UI ;

#if UNITY_EDITOR
using UnityEditor ;
#endif

namespace uGUIHelper
{
	/// <summary>
	/// 画像を上下左右三転するコンポーネント
	/// </summary>
	public class UIInversion : BaseMeshEffect
	{
/*
#if UNITY_EDITOR
		[MenuItem( "Tools/UIInversion/FieldRefactor" )]
		private static void FieldRefactor()
		{
			int c = 0 ;
			UIInversion[] views = UIEditorUtility.FindComponents<UIInversion>
			(
				"Assets/Application",
				( _ ) =>
				{
					_.m_DirectionType = ( DirectionTypes )_.m_Direction ;

					c ++ ;
				}
			) ;
			Debug.LogWarning( "------> UIInversionの数:" + c ) ;
		}
#endif
*/

		[SerializeField][HideInInspector]
		private   RectTransform	m_RectTransform ;

		protected RectTransform	  RectTransform
		{
			get
			{
				if( m_RectTransform != null )
				{
					return m_RectTransform ;
				}
				m_RectTransform = GetComponent<RectTransform>() ;
				return m_RectTransform ;
			}
		}

		public enum DirectionTypes
		{
			None,
			Horizontal,
			Vertical,
			Both,
		}
		
		[SerializeField]
		private DirectionTypes m_DirectionType = DirectionTypes.None ;

		public DirectionTypes DirectionType
		{
			get
			{
				return m_DirectionType ;
			}
			set
			{
				if( m_DirectionType != value )
				{
					m_DirectionType  = value ;
					Refresh() ;
				}
			}
		}

		//---------------------------------------------------------------------------

		public override void ModifyMesh( VertexHelper helper )
		{
			if( IsActive() == false )
			{
				return ;
			}
		
			List<UIVertex> list = new List<UIVertex>() ;
			helper.GetUIVertexStream( list ) ;
			
			if( m_DirectionType != DirectionTypes.None && RectTransform != null )
			{
				ModifyVertices( list ) ;
			}
				
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

			float dx = RectTransform.sizeDelta.x * ( ( RectTransform.pivot.x - 0.5f ) * 2.0f ) ;
			float dy = RectTransform.sizeDelta.y * ( ( RectTransform.pivot.y - 0.5f ) * 2.0f ) ;

			for( int i  = 0 ; i <  list.Count ; i ++ )
			{
				v = list[ i ] ;

				switch( m_DirectionType )
				{
					// 左右のみ
					case DirectionTypes.Horizontal :
						v.position.x = - v.position.x - dx ;
					break ;

					// 上下のみ
					case DirectionTypes.Vertical :
						v.position.y = - v.position.y - dy ;
					break ;

					// 左右上下
					case DirectionTypes.Both :
						v.position.x = - v.position.x - dx ;
						v.position.y = - v.position.y - dy ;
					break ;
				}

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
	}
}
