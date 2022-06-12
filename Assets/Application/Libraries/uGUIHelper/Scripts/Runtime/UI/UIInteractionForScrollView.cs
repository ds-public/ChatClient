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
	/// Interaction コンポーネントクラス
	/// </summary>
	public class UIInteractionForScrollView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
	{
		public delegate void InteractionDelegate( PointerEventData pointer, bool fromScrollView ) ;

		public InteractionDelegate onPointerEnter	= null ;
		public InteractionDelegate onPointerExit	= null ;
		public InteractionDelegate onPointerDown	= null ;
		public InteractionDelegate onPointerUp		= null ;
		public InteractionDelegate onPointerClick	= null ;
#if false
		public InteractionDelegate onBeginDrag		= null ;
		public InteractionDelegate onDrag			= null ;
		public InteractionDelegate onEndDrag		= null ;
#endif
		public void OnPointerEnter( PointerEventData pointer )
		{
			onPointerEnter?.Invoke( pointer, true ) ;
		}

		public void OnPointerExit( PointerEventData pointer )
		{
			onPointerExit?.Invoke( pointer, true ) ;
		}

		public void OnPointerDown( PointerEventData pointer )
		{
			onPointerDown?.Invoke( pointer, true ) ;
		}

		public void OnPointerUp( PointerEventData pointer )
		{
			onPointerUp?.Invoke( pointer, true ) ;
		}

		public void OnPointerClick( PointerEventData pointer )
		{
			onPointerClick?.Invoke( pointer, true ) ;
		}
#if false
		public void OnBeginDrag( PointerEventData pointer )
		{
			onBeginDrag?.Invoke( pointer, false ) ;
		}

		public void OnDrag( PointerEventData pointer )
		{
			onDrag?.Invoke( pointer, false ) ;
		}

		public void OnEndDrag( PointerEventData pointer )
		{
			onEndDrag?.Invoke( pointer, false ) ;
		}
#endif
	}
}
