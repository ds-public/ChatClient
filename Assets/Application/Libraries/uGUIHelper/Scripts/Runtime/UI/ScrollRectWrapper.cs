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
	public class ScrollRectWrapper : ScrollRect, IPointerDownHandler, IPointerUpHandler
	{
		private bool m_Press = false ;
		public  bool IsPress
		{
			get
			{
				return m_Press ;
			}
		}

		private bool m_Drag = false ;
		public  bool IsDrag
		{
			get
			{
				return m_Drag ;
			}
		}

		// 反応可能距離
		public float interactionThresholdRatio = 40.0f / 960.0f ;
		private Vector2	m_InteractionLimitDistance_Base ;

		// 反応可能時間
		public float interactionLimitHoldTime =	 0.5f ;
		private float   m_InteractionLimitHoldTime_Base ;

		//-------------------------------------------------------------------------------------------

	    public void OnPointerDown( PointerEventData pointer )
		{
//			Debug.LogWarning( "ScrollViewプレス開始" ) ;
			if( pointer.pointerPressRaycast.gameObject == null )
			{
				return ;	// ゲームオブジェクトがドラッグ中に消失する可能性もある
			}

			m_Press = true ;

			m_InteractionLimitDistance_Base = pointer.position ;
			m_InteractionLimitHoldTime_Base = Time.realtimeSinceStartup ;
		}

	    public void OnPointerUp( PointerEventData pointer )
		{
//			Debug.LogWarning( "ScrollViewプレス終了" ) ;
			if( pointer.pointerPressRaycast.gameObject == null )
			{
				return ;	// ゲームオブジェクトがドラッグ中に消失する可能性もある
			}

			m_Press = false ;
		}

	    public override void OnBeginDrag( PointerEventData pointer )
		{
//			Debug.LogWarning( "ScrollViewドラッグ開始:" + pointer.pointerPressRaycast.gameObject ) ;
			base.OnBeginDrag( pointer ) ;

			if( pointer.pointerPressRaycast.gameObject == null )
			{
				return ;	// ゲームオブジェクトがドラッグ中に消失する可能性もある
			}

			m_Drag = true ;		// 外からの参照用
			
			m_InteractionLimitDistance_Base = pointer.position ;
		}

		public override void OnDrag( PointerEventData pointer )
		{
			base.OnDrag( pointer ) ;

			if( pointer.pointerPressRaycast.gameObject == null )
			{
				return ;	// ゲームオブジェクトがドラッグ中に消失する可能性もある
			}
		}


		public override void OnEndDrag( PointerEventData pointer )
		{
//			Debug.LogWarning( "ScrollViewドラッグ終了" ) ;

			base.OnEndDrag( pointer ) ;

			m_Drag = false ;	// 外からの参照用

			if( pointer.pointerPressRaycast.gameObject == null )
			{
				return ;	// ゲームオブジェクトがドラッグ中に消失する可能性もある
			}

			//----------------------------------------------------------

//			Debug.LogWarning( "判定に来た") ;

			float holdTime = Time.realtimeSinceStartup - m_InteractionLimitHoldTime_Base ;
			if( holdTime <= interactionLimitHoldTime )
			{
				UIView view = pointer.pointerPressRaycast.gameObject.GetComponent<UIView>() ;
				if( view != null )
				{
					float distance = Vector2.Distance( m_InteractionLimitDistance_Base, pointer.position ) ;

					if( distance <= ( interactionThresholdRatio * view.GetCanvasLength() ) )
					{
						if( view is UIButton )
						{
							// ボタン
							UIButton button = view as UIButton ;
							if( button.enabled == true && button.Interactable == true && button.OnButtonClickAction != null )
							{
								button.OnButtonClickAction( button.Identity, button ) ;
							}
						}
					}
				}
			}
		}
	}
}
