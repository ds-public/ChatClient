using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.EventSystems ;

namespace uGUIHelper
{
	/// <summary>
	/// ドラッグ中かどうかを取得するためにのみ用意した ScrollRect の継承クラス
	/// </summary>
	public class ScrollbarWrapper : Scrollbar
	{
		private bool m_IsPress ;
		public  bool   IsPress
		{
			get
			{
				return m_IsPress ;
			}
		}

	    public override void OnPointerDown( PointerEventData tPointer )
		{
			base.OnPointerDown( tPointer ) ;
			m_IsPress = true ;
		}

	    public override void OnPointerUp( PointerEventData tPointer )
		{
			base.OnPointerUp( tPointer ) ;
			m_IsPress = false ;
		}
	}
}
