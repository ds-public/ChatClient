using System ;
using System.Collections ;
using System.Collections.Generic;
using UnityEngine ;

using uGUIHelper ;

namespace Template.Screens.BootClasses
{
	public class ListView_Item : MonoBehaviour
	{
		[SerializeField]
		protected UIButton m_Button ;

		private	int			m_Index ;
		private Action<int> m_OnSelected ;

		public void SetStyle( string label, int index, Action<int> onSelected )
		{
			m_Button.SetLabelText( label ) ;
			m_Button.SetOnButtonClick( ( string identity, UIButton button ) =>
			{
				m_OnSelected?.Invoke( m_Index ) ;
			} ) ;

			m_Index			= index ;
			m_OnSelected	= onSelected ;
		}
	}
}
