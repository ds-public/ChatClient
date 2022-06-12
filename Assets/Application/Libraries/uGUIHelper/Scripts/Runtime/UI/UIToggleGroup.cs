using UnityEngine ;
using UnityEngine.UI ;
using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

namespace uGUIHelper
{
	public class UIToggleGroup
	{
		protected ToggleGroup m_ToggleGroup ;

		public UIToggleGroup( ToggleGroup toggleGroup )
		{
			m_ToggleGroup = toggleGroup ;
		}

		public bool AllowSwitchOff
		{
			get
			{
				if( m_ToggleGroup == null )
				{
					return false ;
				}

				return m_ToggleGroup.allowSwitchOff ;
			}
			set
			{
				if( m_ToggleGroup == null )
				{
					return ;
				}

				m_ToggleGroup.allowSwitchOff = value ;
			}
		}

		public IEnumerable ActiveToggles()
		{
			if( m_ToggleGroup == null )
			{
				return null ;
			}

			return m_ToggleGroup.ActiveToggles() ;
		}

		public bool AnyTogglesOn()
		{
			if( m_ToggleGroup == null )
			{
				return false ;
			}

			return m_ToggleGroup.AnyTogglesOn() ;
		}

		public void NotifyToggleOn( Toggle toggle )
		{
			if( m_ToggleGroup == null )
			{
				return ;
			}

			m_ToggleGroup.NotifyToggleOn( toggle ) ;
		}

		public void RegisterToggle( Toggle toggle )
		{
			if( m_ToggleGroup == null )
			{
				return ;
			}

			m_ToggleGroup.RegisterToggle( toggle ) ;
		}

		public void UnregisterToggle( Toggle toggle )
		{
			if( m_ToggleGroup == null )
			{
				return ;
			}

			m_ToggleGroup.UnregisterToggle( toggle ) ;
		}

		public void SetAllTogglesOff()
		{
			if( m_ToggleGroup == null )
			{
				return ;
			}

			m_ToggleGroup.SetAllTogglesOff() ;
		}

		public Toggle[] GetAllToggles()
		{
			if( m_ToggleGroup == null )
			{
				return null ;
			}

			Toggle[] toggles = m_ToggleGroup.GetComponentsInChildren<Toggle>() ;
			if( toggles == null || toggles.Length == 0 )
			{
				return null ;
			}

			toggles = toggles.Where( _ => _.group == m_ToggleGroup ).ToArray() ;
			if( toggles == null || toggles.Length == 0 )
			{
				return null ;
			}

			return toggles ;
		}

		public void Refresh()
		{

		}

		private Action<UIView> m_OnValueChanged ;

		public void SetOnValueChanged( Action<UIView> onValueChanged )
		{
			m_OnValueChanged = onValueChanged ;

			Toggle[] toggles = GetAllToggles() ;
			if( toggles == null || toggles.Length == 0 )
			{
				return ;
			}

			int i, l = toggles.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				toggles[ i ].onValueChanged.RemoveListener( OnValueChanged ) ;
				toggles[ i ].onValueChanged.AddListener( OnValueChanged ) ;
			}
		}

		private void OnValueChanged( bool state )
		{
			if( state == false )
			{
				return ;
			}

			if( m_OnValueChanged == null )
			{
				return ;
			}

			Toggle[] toggles = GetAllToggles() ;
			if( toggles == null || toggles.Length == 0 )
			{
				return ;
			}

			UIView view ;
			int i, l = toggles.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( toggles[ i ].isOn == true )
				{
					view = toggles[ i ].GetComponent<UIView>() ;
					if( view != null )
					{
						m_OnValueChanged( view ) ;
					}
				}
			}
		}
	}
}

