using System ;
using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;
using UnityEngine.Serialization ;
using UnityEngine.EventSystems ;

namespace uGUIHelper
{
	public class DxStandaloneInputModule : DxPointerInputModule
	{
		private float m_PrevActionTime ;
		private Vector2 m_LastMoveVector ;
		private int m_ConsecutiveMoveCount = 0 ;

		private Vector2 m_LastMousePosition ;
		private Vector2 m_MousePosition ;

		public	Vector2 MouseDelta	=> ( m_MousePosition - m_LastMousePosition ) ;

		private GameObject m_CurrentFocusedGameObject ;

		private PointerEventData m_InputPointerEvent ;

		protected DxStandaloneInputModule()
		{
		}

		[Obsolete("Mode is no longer needed on input module as it handles both mouse and keyboard simultaneously.", false)]
		public enum InputMode
		{
			Mouse,
			Buttons
		}

		[Obsolete("Mode is no longer needed on input module as it handles both mouse and keyboard simultaneously.", false)]
		public InputMode inputMode
		{
			get { return InputMode.Mouse ; }
		}

		[SerializeField]
		private string m_HorizontalAxis = "Horizontal" ;

		/// <summary>
		/// Name of the vertical axis for movement (if axis events are used).
		/// </summary>
		[SerializeField]
		private string m_VerticalAxis = "Vertical" ;

		/// <summary>
		/// Name of the submit button.
		/// </summary>
		[SerializeField]
		private string m_SubmitButton = "Submit" ;

		/// <summary>
		/// Name of the submit button.
		/// </summary>
		[SerializeField]
		private string m_CancelButton = "Cancel" ;

		[SerializeField]
		private float m_InputActionsPerSecond = 10 ;

		[SerializeField]
		private float m_RepeatDelay = 0.5f ;

		[SerializeField]
		[FormerlySerializedAs("m_AllowActivationOnMobileDevice")]
		private bool m_ForceModuleActive ;

		[Obsolete("allowActivationOnMobileDevice has been deprecated. Use forceModuleActive instead (UnityUpgradable) -> forceModuleActive")]
		public bool allowActivationOnMobileDevice
		{
			get { return m_ForceModuleActive ; }
			set { m_ForceModuleActive = value ; }
		}

		/// <summary>
		/// Force this module to be active.
		/// </summary>
		/// <remarks>
		/// If there is no module active with higher priority (ordered in the inspector) this module will be forced active even if valid enabling conditions are not met.
		/// </remarks>
		public bool forceModuleActive
		{
			get { return m_ForceModuleActive ; }
			set { m_ForceModuleActive = value ; }
		}

		/// <summary>
		/// Number of keyboard / controller inputs allowed per second.
		/// </summary>
		public float inputActionsPerSecond
		{
			get { return m_InputActionsPerSecond ; }
			set { m_InputActionsPerSecond = value ; }
		}

		/// <summary>
		/// Delay in seconds before the input actions per second repeat rate takes effect.
		/// </summary>
		/// <remarks>
		/// If the same direction is sustained, the inputActionsPerSecond property can be used to control the rate at which events are fired. However, it can be desirable that the first repetition is delayed, so the user doesn't get repeated actions by accident.
		/// </remarks>
		public float repeatDelay
		{
			get { return m_RepeatDelay ; }
			set { m_RepeatDelay = value ; }
		}

		/// <summary>
		/// Name of the horizontal axis for movement (if axis events are used).
		/// </summary>
		public string horizontalAxis
		{
			get { return m_HorizontalAxis ; }
			set { m_HorizontalAxis = value ; }
		}

		/// <summary>
		/// Name of the vertical axis for movement (if axis events are used).
		/// </summary>
		public string verticalAxis
		{
			get { return m_VerticalAxis ; }
			set { m_VerticalAxis = value ; }
		}

		/// <summary>
		/// Maximum number of input events handled per second.
		/// </summary>
		public string submitButton
		{
			get { return m_SubmitButton ; }
			set { m_SubmitButton = value ; }
		}

		/// <summary>
		/// Input manager name for the 'cancel' button.
		/// </summary>
		public string cancelButton
		{
			get { return m_CancelButton ; }
			set { m_CancelButton = value ; }
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 指定のゲームオブジェクト(UI)がホバー対象になっているか判定する
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public bool IsHovering( GameObject target )
		{
			if( m_InputPointerEvent == null || m_InputPointerEvent.hovered == null || m_InputPointerEvent.hovered.Count == 0 )
			{
				return false ;
			}

			var hoveredCount = m_InputPointerEvent.hovered.Count ;
			for( var i  = 0 ; i <  hoveredCount ; ++ i )
			{
				if( m_InputPointerEvent.hovered[ i ] != null && m_InputPointerEvent.hovered[ i ] == target )
				{
					return true ;
				}
			}

			return false ;
		}

		/// <summary>
		/// 指定のゲームオブジェクト(UI)がプレス対象になっているか判定する
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public bool IsPressing( GameObject target )
		{
			if( m_InputPointerEvent == null || m_InputPointerEvent.pointerPress == null )
			{
				return false ;
			}

			return m_InputPointerEvent.pointerPress == target ;
		}

		//-------------------------------------------------------------------------------------------

		private bool ShouldIgnoreEventsOnNoFocus()
		{
#if UNITY_EDITOR
			return !UnityEditor.EditorApplication.isRemoteConnected ;
#else
			return true ;
#endif
		}

		public override void UpdateModule()
		{
			if( !eventSystem.isFocused && ShouldIgnoreEventsOnNoFocus() )
			{
				if( m_InputPointerEvent != null && m_InputPointerEvent.pointerDrag != null && m_InputPointerEvent.dragging )
				{
					ReleaseMouse( m_InputPointerEvent, m_InputPointerEvent.pointerCurrentRaycast.gameObject ) ;
				}

				m_InputPointerEvent = null ;

				return ;
			}

			m_LastMousePosition = m_MousePosition ;
			m_MousePosition = input.mousePosition ;
		}

		public override bool IsModuleSupported()
		{
			return m_ForceModuleActive || input.mousePresent || input.touchSupported ;
		}

		public override bool ShouldActivateModule()
		{
			if( !base.ShouldActivateModule() )
			{
				return false ;
			}

			var shouldActivate = m_ForceModuleActive ;
			shouldActivate |= input.GetButtonDown( m_SubmitButton ) ;
			shouldActivate |= input.GetButtonDown( m_CancelButton ) ;
			shouldActivate |= !Mathf.Approximately( input.GetAxisRaw( m_HorizontalAxis ), 0.0f ) ;
			shouldActivate |= !Mathf.Approximately( input.GetAxisRaw( m_VerticalAxis   ), 0.0f ) ;
			shouldActivate |= ( m_MousePosition - m_LastMousePosition ).sqrMagnitude >  0.0f ;
			shouldActivate |= input.GetMouseButtonDown( 0 ) ;

			if( input.touchCount >  0 )
			{
				shouldActivate = true ;
			}

			return shouldActivate ;
		}

		/// <summary>
		/// See BaseInputModule.
		/// </summary>
		public override void ActivateModule()
		{
			if( !eventSystem.isFocused && ShouldIgnoreEventsOnNoFocus() )
			{
				return ;
			}

			base.ActivateModule() ;
			m_MousePosition = input.mousePosition ;
			m_LastMousePosition = input.mousePosition ;

			var toSelect = eventSystem.currentSelectedGameObject ;
			if( toSelect == null )
			{
				toSelect = eventSystem.firstSelectedGameObject ;
			}

			eventSystem.SetSelectedGameObject( toSelect, GetBaseEventData() ) ;
		}

		/// <summary>
		/// See BaseInputModule.
		/// </summary>
		public override void DeactivateModule()
		{
			base.DeactivateModule() ;
			ClearSelection() ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 毎フレームの処理を行う
		/// </summary>
		public override void Process()
		{
			if( !eventSystem.isFocused && ShouldIgnoreEventsOnNoFocus() )
			{
				return ;
			}

			bool usedEvent = SendUpdateEventToSelectedObject() ;

			// case 1004066 - touch / mouse events should be processed before navigation events in case
			// they change the current selected gameobject and the submit button is a touch / mouse button.

			// touch needs to take precedence because of the mouse emulation layer
			if( ProcessTouchEvents() == false && input.mousePresent == true )
			{
				// マウスイベントを処理する
				if( Cursor.visible == true )
				{
					ProcessMouseEvent() ;
				}
			}

			if( eventSystem.sendNavigationEvents )
			{
				if( !usedEvent )
				{
					usedEvent |= SendMoveEventToSelectedObject() ;
				}

				if( !usedEvent )
				{
					SendSubmitEventToSelectedObject() ;
				}
			}
		}

		//-------------------------------------------------------------------------------------------

		// タッチイベントを処理する
		private bool ProcessTouchEvents()
		{
			for( int i  = 0 ; i <  input.touchCount ; ++ i )
			{
				Touch touch = input.GetTouch( i ) ;

				if( touch.type == TouchType.Indirect )
				{
					continue ;
				}

				var pointerEvent = GetTouchPointerEventData( touch, out bool pressed, out bool released, out bool pressing ) ;

				ProcessTouchPress( pointerEvent, pressed, released, pressing ) ;

				if( !released )
				{
					ProcessMove( pointerEvent ) ;	// タッチとマウスで共通
					ProcessDrag( pointerEvent ) ;	// タッチとマウスで共通
				}
				else
				{
					RemovePointerData( pointerEvent ) ;
				}
			}

			if( input.touchCount >  0 )
			{
				Cursor.visible = false ;
			}
			else
			{
				if( Cursor.visible == false )
				{
					if( MouseDelta.sqrMagnitude >  0 )
					{
						Cursor.visible = true ;	// 解除
					}
				}
			}

			return input.touchCount >  0 ;
		}

		// タッチのプレスの処理
		protected void ProcessTouchPress( PointerEventData pointerEvent, bool pressed, bool released, bool pressing )
		{
			// タッチ中はこのメソッドが呼ばれる
			var currentOverGo = pointerEvent.pointerCurrentRaycast.gameObject ;

			//----------------------------------------------------------

			// プレス中の場所にレイキャスト対象が出現した場合の反応処理
			bool isForcePressed = false ;
			if( pressing == true && currentOverGo != null && ( pointerEvent.rawPointerPress == null || ( pointerEvent.rawPointerPress != null && pointerEvent.rawPointerPress != currentOverGo ) ) )
			{
				PointerEventData pointerData = new PointerEventData( eventSystem ) ;
				pointerData.position = pointerEvent.pressPosition ;

				eventSystem.RaycastAll( pointerData, m_RaycastResultCache ) ;

				var raycast = FindFirstRaycast( m_RaycastResultCache ) ;
				pointerData.pointerCurrentRaycast = raycast ;

				// レイキャストの中にこれまでプレス中だった対象が含まれているか確認する
				bool isContains = false ;
				if( pointerEvent.rawPointerPress != null )
				{
					int i, l = m_RaycastResultCache.Count ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						if( m_RaycastResultCache[ i ].gameObject == pointerEvent.rawPointerPress )
						{
							// 含まれている
							isContains = true ;
							break ;	
						}
					}
				}

				m_RaycastResultCache.Clear() ;

				if( pointerData.pointerCurrentRaycast.gameObject == currentOverGo )
				{
					if( isContains == true )
					{
						if( pointerEvent.rawPointerPress != null && pointerEvent.rawPointerPress != currentOverGo )
						{
							ReleaseTouch( pointerEvent ) ;
						}
						isForcePressed = true ;
					}
				}
			}

			// プレス判定
			if( pressed == true || isForcePressed == true )
			{
				pointerEvent.eligibleForClick		= pressed ;
				pointerEvent.delta					= Vector2.zero ;
				pointerEvent.dragging				= false ;
				pointerEvent.useDragThreshold		= true ;
				pointerEvent.pressPosition			= pointerEvent.position ;
				pointerEvent.pointerPressRaycast	= pointerEvent.pointerCurrentRaycast ;

				if( pressed == true )
				{
					DeselectIfSelectionChanged( currentOverGo, pointerEvent ) ;
				}

//				if( pointerEvent.pointerEnter != currentOverGo )
//				{
//					// send a pointer enter to the touched element if it isn't the one to select...
//					HandlePointerExitAndEnter( pointerEvent, currentOverGo ) ;
//					pointerEvent.pointerEnter = currentOverGo ;
//				}

				// search for the control that will receive the press
				// if we can't find a press handler set the press
				// handler to be what would receive a click.
				var newPressed	= ExecuteEvents.ExecuteHierarchy( currentOverGo, pointerEvent, ExecuteEvents.pointerDownHandler ) ;

				GameObject newClick	= null ;
				if( pressed == true )
				{
					newClick = ExecuteEvents.GetEventHandler<IPointerClickHandler>( currentOverGo ) ;
				}

				// didnt find a press handler... search for a click handler
				if( newPressed == null )
				{
					newPressed = newClick ;
				}

				pointerEvent.pointerPress		= newPressed ;
				pointerEvent.rawPointerPress	= currentOverGo ;

				if( pressed == true )
				{
					float time = Time.unscaledTime ;

					if( newPressed == pointerEvent.lastPress )
					{
						var diffTime = time - pointerEvent.clickTime ;
						if( diffTime <  0.3f )
						{
							++ pointerEvent.clickCount ;
						}
						else
						{
							pointerEvent.clickCount = 1 ;
						}

						pointerEvent.clickTime = time ;
					}
					else
					{
						pointerEvent.clickCount = 1 ;
					}

					pointerEvent.pointerClick		= newClick ;
					pointerEvent.clickTime = time ;

					// Save the drag handler as well
					pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>( currentOverGo ) ;

					if( pointerEvent.pointerDrag != null )
					{
						ExecuteEvents.Execute( pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag ) ;
					}
				}
			}

			// リリース判定
			bool isForceRelease = false ;
			if( pressing == true && pointerEvent.rawPointerPress != null && currentOverGo == null )
			{
				PointerEventData pointerData = new PointerEventData( eventSystem ) ;
				pointerData.position = pointerEvent.pressPosition ;

				eventSystem.RaycastAll( pointerData, m_RaycastResultCache ) ;

				var raycast = FindFirstRaycast( m_RaycastResultCache ) ;
				pointerData.pointerCurrentRaycast = raycast ;

				m_RaycastResultCache.Clear() ;

				if( pointerData.pointerCurrentRaycast.gameObject == null )
				{
					isForceRelease = true ;
				}
			}

			if( released == true || isForceRelease == true )
			{
				ReleaseTouch( pointerEvent, currentOverGo ) ;
			}

			m_InputPointerEvent = pointerEvent ;
		}

		// プレスとドラッグの解放処理を行う(タッチとマウスで共通)
		private void ReleaseTouch( PointerEventData pointerEvent )
		{
			// プレスの解放
			if( pointerEvent.pointerPress != null && pointerEvent.pointerPress.activeInHierarchy == false )
			{
				// プレスの通知対象が非アクティブなのでコンポーネントのコールバックメソッドを直接呼ぶ
				var interaction = pointerEvent.pointerPress.GetComponent<UIInteraction>() ;
				if( interaction != null )
				{
					interaction.OnPointerUp( pointerEvent ) ;
				}

				var interactionForScrollView = pointerEvent.pointerPress.GetComponent<UIInteractionForScrollView>() ;
				if( interactionForScrollView != null )
				{
					interactionForScrollView.OnPointerUp( pointerEvent ) ;
				}
			}
			else
			{
				ExecuteEvents.Execute( pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler ) ;
			}

			pointerEvent.eligibleForClick	= false ;
			pointerEvent.pointerPress		= null ;
			pointerEvent.rawPointerPress	= null ;
			pointerEvent.pointerClick		= null ;

			// ドラッグの解放
			if( pointerEvent.pointerDrag != null && pointerEvent.dragging )
			{
				if( pointerEvent.pointerDrag.activeInHierarchy == false )
				{
					// プレスの通知対象が非アクティブなのでコンポーネントのコールバックメソッドを直接呼ぶ
					var interaction = pointerEvent.pointerDrag.GetComponent<UIInteraction>() ;
					if( interaction != null )
					{
						interaction.OnEndDrag( pointerEvent ) ;
					}
				}
				else
				{
					ExecuteEvents.Execute( pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler ) ;
				}
			}

			pointerEvent.dragging		= false ;
			pointerEvent.pointerDrag	= null ;

			//----------------------------------
	
			// ホバーの解放
			if( pointerEvent.pointerEnter != null )
			{
				// マウスオーバーの対象が１つも無くなった→１フレーム前の対象に「出る」を送る
				var hoveredCount = pointerEvent.hovered.Count ;
				for( var i  = 0 ; i <  hoveredCount ; ++ i )
				{
					if( pointerEvent.hovered[ i ] != null && pointerEvent.hovered[ i ].activeInHierarchy == false )
					{
						// 非アクティブだと SendMessage() は機能しないため直接コンポーネントを取得して送信する

						var interaction = pointerEvent.hovered[ i ].GetComponent<UIInteraction>() ;
						if( interaction != null )
						{
							interaction.OnPointerExit( pointerEvent ) ;
						}

						var interactionForScrollView = pointerEvent.hovered[ i ].GetComponent<UIInteractionForScrollView>() ;
						if( interactionForScrollView != null )
						{
							interactionForScrollView.OnPointerExit( pointerEvent ) ;
						}
					}
					else
					{
						ExecuteEvents.Execute( pointerEvent.hovered[ i ], pointerEvent, ExecuteEvents.pointerExitHandler ) ;
					}
				}

				pointerEvent.hovered.Clear() ;
				pointerEvent.pointerEnter = null ;
			}
		}

		// プレスとドラッグの解放処理を行う(タッチとマウスで共通)
		private void ReleaseTouch( PointerEventData pointerEvent, GameObject currentOverGo )
		{
			// プレスの解放
			if( pointerEvent.pointerPress != null && pointerEvent.pointerPress.activeInHierarchy == false )
			{
				// プレスの通知対象が非アクティブなのでコンポーネントのコールバックメソッドを直接呼ぶ
				var interaction = pointerEvent.pointerPress.GetComponent<UIInteraction>() ;
				if( interaction != null )
				{
					interaction.OnPointerUp( pointerEvent ) ;
				}

				var interactionForScrollView = pointerEvent.pointerPress.GetComponent<UIInteractionForScrollView>() ;
				if( interactionForScrollView != null )
				{
					interactionForScrollView.OnPointerUp( pointerEvent ) ;
				}
			}
			else
			{
				ExecuteEvents.Execute( pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler ) ;
			}

			// クリックの判定
			var pointerClickHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>( currentOverGo ) ;

			if( pointerEvent.pointerClick == pointerClickHandler && pointerEvent.eligibleForClick == true )
			{
				ExecuteEvents.Execute( pointerEvent.pointerClick, pointerEvent, ExecuteEvents.pointerClickHandler ) ;
			}

			// ドロップの判定
			if( pointerEvent.pointerDrag != null && pointerEvent.dragging )
			{
				ExecuteEvents.ExecuteHierarchy( currentOverGo, pointerEvent, ExecuteEvents.dropHandler ) ;
			}

			pointerEvent.eligibleForClick	= false ;
			pointerEvent.pointerPress		= null ;
			pointerEvent.rawPointerPress	= null ;
			pointerEvent.pointerClick		= null ;

			// ドラッグの解放
			if( pointerEvent.pointerDrag != null && pointerEvent.dragging )
			{
				if( pointerEvent.pointerDrag.activeInHierarchy == false )
				{
					// プレスの通知対象が非アクティブなのでコンポーネントのコールバックメソッドを直接呼ぶ
					var interaction = pointerEvent.pointerDrag.GetComponent<UIInteraction>() ;
					if( interaction != null )
					{
						interaction.OnEndDrag( pointerEvent ) ;
					}
				}
				else
				{
					ExecuteEvents.Execute( pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler ) ;
				}
			}

			pointerEvent.dragging		= false ;
			pointerEvent.pointerDrag	= null ;

			//----------------------------------
	
			// ホバーの解放
			if( pointerEvent.pointerEnter != null )
			{
				// マウスオーバーの対象が１つも無くなった→１フレーム前の対象に「出る」を送る
				var hoveredCount = pointerEvent.hovered.Count ;
				for( var i  = 0 ; i <  hoveredCount ; ++ i )
				{
					if( pointerEvent.hovered[ i ] != null && pointerEvent.hovered[ i ].activeInHierarchy == false )
					{
						// 非アクティブだと SendMessage() は機能しないため直接コンポーネントを取得して送信する

						var interaction = pointerEvent.hovered[ i ].GetComponent<UIInteraction>() ;
						if( interaction != null )
						{
							interaction.OnPointerExit( pointerEvent ) ;
						}

						var interactionForScrollView = pointerEvent.hovered[ i ].GetComponent<UIInteractionForScrollView>() ;
						if( interactionForScrollView != null )
						{
							interactionForScrollView.OnPointerExit( pointerEvent ) ;
						}
					}
					else
					{
						ExecuteEvents.Execute( pointerEvent.hovered[ i ], pointerEvent, ExecuteEvents.pointerExitHandler ) ;
					}
				}

				pointerEvent.hovered.Clear() ;
				pointerEvent.pointerEnter = null ;
			}
		}

		/// <summary>
		/// Calculate and send a submit event to the current selected object.
		/// </summary>
		/// <returns>If the submit event was used by the selected object.</returns>
		protected bool SendSubmitEventToSelectedObject()
		{
			if( eventSystem.currentSelectedGameObject == null )
			{
				return false ;
			}

			var data = GetBaseEventData() ;
			if( input.GetButtonDown( m_SubmitButton ) )
			{
				ExecuteEvents.Execute( eventSystem.currentSelectedGameObject, data, ExecuteEvents.submitHandler ) ;
			}

			if( input.GetButtonDown( m_CancelButton ) )
			{
				ExecuteEvents.Execute( eventSystem.currentSelectedGameObject, data, ExecuteEvents.cancelHandler ) ;
			}

			return data.used ;
		}

		private Vector2 GetRawMoveVector()
		{
			Vector2 move = Vector2.zero ;
			move.x = input.GetAxisRaw( m_HorizontalAxis ) ;
			move.y = input.GetAxisRaw( m_VerticalAxis   ) ;

			if( input.GetButtonDown( m_HorizontalAxis ) )
			{
				if( move.x <  0 )
				{
					move.x  = -1f ;
				}
				if( move.x >  0 )
				{
					move.x  =  1f ;
				}
			}
			if( input.GetButtonDown( m_VerticalAxis ) )
			{
				if( move.y <  0 )
				{
					move.y  = -1f ;
				}
				if( move.y >  0 )
				{
					move.y  =  1f ;
				}
			}
			return move ;
		}

		/// <summary>
		/// Calculate and send a move event to the current selected object.
		/// </summary>
		/// <returns>If the move event was used by the selected object.</returns>
		protected bool SendMoveEventToSelectedObject()
		{
			float time = Time.unscaledTime ;

			Vector2 movement = GetRawMoveVector() ;
			if( Mathf.Approximately( movement.x, 0f ) && Mathf.Approximately( movement.y, 0f ) )
			{
				m_ConsecutiveMoveCount = 0 ;
				return false ;
			}

			bool similarDir = ( Vector2.Dot( movement, m_LastMoveVector ) >  0 ) ;

			// If direction didn't change at least 90 degrees, wait for delay before allowing consequtive event.
			if( similarDir && m_ConsecutiveMoveCount == 1 )
			{
				if( time <= m_PrevActionTime + m_RepeatDelay )
				{
					return false ;
				}
			}
			// If direction changed at least 90 degree, or we already had the delay, repeat at repeat rate.
			else
			{
				if( time <= m_PrevActionTime + 1f / m_InputActionsPerSecond )
				{
					return false ;
				}
			}

			var axisEventData = GetAxisEventData( movement.x, movement.y, 0.6f ) ;

			if( axisEventData.moveDir != MoveDirection.None )
			{
				ExecuteEvents.Execute( eventSystem.currentSelectedGameObject, axisEventData, ExecuteEvents.moveHandler ) ;
				if( !similarDir )
				{
					m_ConsecutiveMoveCount = 0 ;
				}
				m_ConsecutiveMoveCount ++ ;
				m_PrevActionTime = time ;
				m_LastMoveVector = movement ;
			}
			else
			{
				m_ConsecutiveMoveCount = 0 ;
			}

			return axisEventData.used ;
		}

		//-------------------------------------------------------------------------------------------

		// マウスイベントを処理する
		protected void ProcessMouseEvent()
		{
			ProcessMouseEvent( 0 ) ;
		}

		[Obsolete("This method is no longer checked, overriding it with return true does nothing!")]
		protected virtual bool ForceAutoSelect()
		{
			return false ;
		}

		// マウスイベントを処理する
		protected void ProcessMouseEvent( int id )
		{
			var mouseData = GetMousePointerEventData( id ) ;
			var leftButtonData = mouseData.GetButtonState( PointerEventData.InputButton.Left ).eventData ;

			m_CurrentFocusedGameObject = leftButtonData.buttonData.pointerCurrentRaycast.gameObject ;

			// 左クリックの処理
			bool isReleased = ProcessMousePress( leftButtonData ) ;

			ProcessMove( leftButtonData.buttonData ) ;	// タッチとマウスで共通

			if( isReleased == false )
			{
				ProcessDrag( leftButtonData.buttonData ) ;	// タッチとマウスで共通
			}
			else
			{
//				RemovePointerData( leftButtonData.buttonData ) ;
				leftButtonData.buttonData.pressPosition = Input.mousePosition ;	// 現在位置で初期化しておく
			}

			// 右クリックと中クリックの処理
//			ProcessMousePress( mouseData.GetButtonState( PointerEventData.InputButton.Right ).eventData ) ;
//			ProcessDrag( mouseData.GetButtonState( PointerEventData.InputButton.Right ).eventData.buttonData ) ;
//			ProcessMousePress( mouseData.GetButtonState( PointerEventData.InputButton.Middle ).eventData ) ;
//			ProcessDrag( mouseData.GetButtonState( PointerEventData.InputButton.Middle ).eventData.buttonData ) ;

			if( !Mathf.Approximately( leftButtonData.buttonData.scrollDelta.sqrMagnitude, 0.0f ) )
			{
				var scrollHandler = ExecuteEvents.GetEventHandler<IScrollHandler>( leftButtonData.buttonData.pointerCurrentRaycast.gameObject ) ;
				ExecuteEvents.ExecuteHierarchy( scrollHandler, leftButtonData.buttonData, ExecuteEvents.scrollHandler ) ;
			}
		}

		protected bool SendUpdateEventToSelectedObject()
		{
			if( eventSystem.currentSelectedGameObject == null )
			{
				return false ;
			}

			var data = GetBaseEventData() ;
			ExecuteEvents.Execute( eventSystem.currentSelectedGameObject, data, ExecuteEvents.updateSelectedHandler ) ;
			return data.used ;
		}

		// マウスのプレスの処理
		protected bool ProcessMousePress( MouseButtonEventData data )
		{
			var pointerEvent = data.buttonData ;
			var currentOverGo = pointerEvent.pointerCurrentRaycast.gameObject ;

			bool pressed	= data.PressedThisFrame() ;
			bool released	= data.ReleasedThisFrame() ;


			// プレス中の場所にレイキャスト対象が出現した場合の反応処理
			bool isForcePressed = false ;
			if( data.IsPressing == true && currentOverGo != null && ( pointerEvent.rawPointerPress == null || pointerEvent.rawPointerPress != currentOverGo ) )
			{
				PointerEventData pointerData = new PointerEventData( eventSystem ) ;
				pointerData.position = pointerEvent.pressPosition ;

				// 全ての対象を取得する
				eventSystem.RaycastAll( pointerData, m_RaycastResultCache ) ;

				// 最初にヒットする対象を取得する
				var raycast = FindFirstRaycast( m_RaycastResultCache ) ;
				pointerData.pointerCurrentRaycast = raycast ;

//				Debug.Log( "========" ) ;
//				int i, l = m_RaycastResultCache.Count ;
//				for( i  = 0 ; i <  l ; i ++ )
//				{
//					Debug.Log( "Name:" + m_RaycastResultCache[ i ].gameObject.name ) ;
//				}

				// レイキャストの中にこれまでプレス中だった対象が含まれているか確認する
				bool isContains = false ;
				if( pointerEvent.rawPointerPress != null )
				{
					int i, l = m_RaycastResultCache.Count ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						if( m_RaycastResultCache[ i ].gameObject == pointerEvent.rawPointerPress )
						{
							// 含まれている
							isContains = true ;
							break ;	
						}
					}
				}

				m_RaycastResultCache.Clear() ;

				if( pointerData.pointerCurrentRaycast.gameObject == currentOverGo )
				{
					if( isContains == true )
					{
						if( pointerEvent.rawPointerPress != null && pointerEvent.rawPointerPress != currentOverGo )
						{
							ReleaseMouse( pointerEvent ) ;
						}
						isForcePressed = true ;
					}
				}
			}


			// 現在のフレームで押されたかどうか(レイキャストにヒットするかどうかは関係なし)
			if( pressed == true || isForcePressed == true )
			{
				pointerEvent.eligibleForClick		= pressed ;
				pointerEvent.delta					= Vector2.zero ;
				pointerEvent.dragging				= false ;
				pointerEvent.useDragThreshold		= true ;
				pointerEvent.pressPosition			= pointerEvent.position ;
				pointerEvent.pointerPressRaycast	= pointerEvent.pointerCurrentRaycast ;

				if( pressed == true )
				{
					DeselectIfSelectionChanged( currentOverGo, pointerEvent ) ;
				}

				// search for the control that will receive the press
				// if we can't find a press handler set the press
				// handler to be what would receive a click.

				var newPressed	= ExecuteEvents.ExecuteHierarchy( currentOverGo, pointerEvent, ExecuteEvents.pointerDownHandler ) ;

				GameObject newClick	= null ;
				if( pressed == true )
				{
					newClick = ExecuteEvents.GetEventHandler<IPointerClickHandler>( currentOverGo ) ;
				}

				// didnt find a press handler... search for a click handler
				if( newPressed == null )
				{
					newPressed = newClick ;
				}

				pointerEvent.pointerPress		= newPressed ;
				pointerEvent.rawPointerPress	= currentOverGo ;

				//---------------------------------------------------------

				if( pressed == true )
				{
					float time = Time.unscaledTime ;

					if( newPressed == pointerEvent.lastPress )
					{
						var diffTime = time - pointerEvent.clickTime ;
						if( diffTime <  0.3f )
						{
							++ pointerEvent.clickCount ;
						}
						else
						{
							pointerEvent.clickCount = 1 ;
						}

						pointerEvent.clickTime = time ;
					}
					else
					{
						pointerEvent.clickCount = 1 ;
					}

					pointerEvent.pointerClick		= newClick ;
					pointerEvent.clickTime = time ;
					
					// Save the drag handler as well
					pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>( currentOverGo ) ;

					if( pointerEvent.pointerDrag != null )
					{
						ExecuteEvents.Execute( pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag ) ;
					}
				}

				m_InputPointerEvent = pointerEvent ;
			}

			// リリース判定
			bool isReleased = false ;

			bool isForceRelease = false ;
			if( data.IsPressing == true && pointerEvent.rawPointerPress != null && currentOverGo == null )
			{
				PointerEventData pointerData = new PointerEventData( eventSystem ) ;
				pointerData.position = pointerEvent.pressPosition ;

				eventSystem.RaycastAll( pointerData, m_RaycastResultCache ) ;

				var raycast = FindFirstRaycast( m_RaycastResultCache ) ;
				pointerData.pointerCurrentRaycast = raycast ;
				m_RaycastResultCache.Clear() ;

				if( pointerData.pointerCurrentRaycast.gameObject == null )
				{
					isForceRelease = true ;
				}
			}

			if( released == true || isForceRelease == true )
			{
				ReleaseMouse( pointerEvent, currentOverGo ) ;
				isReleased = true ;
			}

			m_InputPointerEvent = pointerEvent ;

			return isReleased ;
		}

		// プレスとドラッグの解放処理を行う(タッチとマウスで共通)
		private void ReleaseMouse( PointerEventData pointerEvent )
		{
			// プレスの解放
			if( pointerEvent.pointerPress != null && pointerEvent.pointerPress.activeInHierarchy == false )
			{
				// プレスの通知対象が非アクティブなのでコンポーネントのコールバックメソッドを直接呼ぶ
				var interaction = pointerEvent.pointerPress.GetComponent<UIInteraction>() ;
				if( interaction != null )
				{
					interaction.OnPointerUp( pointerEvent ) ;
				}

				var interactionForScrollView = pointerEvent.pointerPress.GetComponent<UIInteractionForScrollView>() ;
				if( interactionForScrollView != null )
				{
					interactionForScrollView.OnPointerUp( pointerEvent ) ;
				}
			}
			else
			{
				ExecuteEvents.Execute( pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler ) ;
			}

			pointerEvent.eligibleForClick	= false ;
			pointerEvent.pointerPress		= null ;
			pointerEvent.rawPointerPress	= null ;
			pointerEvent.pointerClick		= null ;

			// ドラッグの解放
			if( pointerEvent.pointerDrag != null && pointerEvent.dragging == true )
			{
				if( pointerEvent.pointerDrag.activeInHierarchy == false )
				{
					// プレスの通知対象が非アクティブなのでコンポーネントのコールバックメソッドを直接呼ぶ
					var interaction = pointerEvent.pointerDrag.GetComponent<UIInteraction>() ;
					if( interaction != null )
					{
						interaction.OnEndDrag( pointerEvent ) ;
					}
				}
				else
				{
					ExecuteEvents.Execute( pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler ) ;
				}
			}
			
			pointerEvent.dragging			= false ;
			pointerEvent.pointerDrag		= null ;
		}

		// プレスとドラッグの解放処理を行う(タッチとマウスで共通)
		private void ReleaseMouse( PointerEventData pointerEvent, GameObject currentOverGo )
		{
			// プレスの解放
			if( pointerEvent.pointerPress != null && pointerEvent.pointerPress.activeInHierarchy == false )
			{
				// プレスの通知対象が非アクティブなのでコンポーネントのコールバックメソッドを直接呼ぶ
				var interaction = pointerEvent.pointerPress.GetComponent<UIInteraction>() ;
				if( interaction != null )
				{
					interaction.OnPointerUp( pointerEvent ) ;
				}

				var interactionForScrollView = pointerEvent.pointerPress.GetComponent<UIInteractionForScrollView>() ;
				if( interactionForScrollView != null )
				{
					interactionForScrollView.OnPointerUp( pointerEvent ) ;
				}
			}
			else
			{
				ExecuteEvents.Execute( pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler ) ;
			}

			// クリックの判定
			var pointerClickHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>( currentOverGo ) ;

			if( pointerEvent.pointerClick == pointerClickHandler && pointerEvent.eligibleForClick == true )
			{
				ExecuteEvents.Execute( pointerEvent.pointerClick, pointerEvent, ExecuteEvents.pointerClickHandler ) ;
			}

			// ドロップの判定
			if( pointerEvent.pointerDrag != null && pointerEvent.dragging == true )
			{
				ExecuteEvents.ExecuteHierarchy( currentOverGo, pointerEvent, ExecuteEvents.dropHandler ) ;
			}

			pointerEvent.eligibleForClick	= false ;
			pointerEvent.pointerPress		= null ;
			pointerEvent.rawPointerPress	= null ;
			pointerEvent.pointerClick		= null ;

			// ドラッグの解放
			if( pointerEvent.pointerDrag != null && pointerEvent.dragging == true )
			{
				if( pointerEvent.pointerDrag.activeInHierarchy == false )
				{
					// プレスの通知対象が非アクティブなのでコンポーネントのコールバックメソッドを直接呼ぶ
					var interaction = pointerEvent.pointerDrag.GetComponent<UIInteraction>() ;
					if( interaction != null )
					{
						interaction.OnEndDrag( pointerEvent ) ;
					}
				}
				else
				{
					ExecuteEvents.Execute( pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler ) ;
				}
			}
			
			pointerEvent.dragging			= false ;
			pointerEvent.pointerDrag		= null ;
		}

		protected GameObject GetCurrentFocusedGameObject()
		{
			return m_CurrentFocusedGameObject ;
		}
	}
}

//---------------------------------------------------------------------------------------------
// オリジナルコード

#if false
using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.EventSystems ;

namespace uGUIHelper
{
    [AddComponentMenu("Event/Standalone Input Module")]
    /// <summary>
    /// A BaseInputModule designed for mouse / keyboard / controller input.
    /// </summary>
    /// <remarks>
    /// Input module for working with, mouse, keyboard, or controller.
    /// </remarks>
    public class DxStandaloneInputModule : DxPointerInputModule
    {
        private float m_PrevActionTime;
        private Vector2 m_LastMoveVector;
        private int m_ConsecutiveMoveCount = 0;

        private Vector2 m_LastMousePosition;
        private Vector2 m_MousePosition;

        private GameObject m_CurrentFocusedGameObject;

        private PointerEventData m_InputPointerEvent;

        protected DxStandaloneInputModule()
        {
        }

        [Obsolete("Mode is no longer needed on input module as it handles both mouse and keyboard simultaneously.", false)]
        public enum InputMode
        {
            Mouse,
            Buttons
        }

        [Obsolete("Mode is no longer needed on input module as it handles both mouse and keyboard simultaneously.", false)]
        public InputMode inputMode
        {
            get { return InputMode.Mouse; }
        }

        [SerializeField]
        private string m_HorizontalAxis = "Horizontal";

        /// <summary>
        /// Name of the vertical axis for movement (if axis events are used).
        /// </summary>
        [SerializeField]
        private string m_VerticalAxis = "Vertical";

        /// <summary>
        /// Name of the submit button.
        /// </summary>
        [SerializeField]
        private string m_SubmitButton = "Submit";

        /// <summary>
        /// Name of the submit button.
        /// </summary>
        [SerializeField]
        private string m_CancelButton = "Cancel";

        [SerializeField]
        private float m_InputActionsPerSecond = 10;

        [SerializeField]
        private float m_RepeatDelay = 0.5f;

        [SerializeField]
        [FormerlySerializedAs("m_AllowActivationOnMobileDevice")]
        private bool m_ForceModuleActive;

        [Obsolete("allowActivationOnMobileDevice has been deprecated. Use forceModuleActive instead (UnityUpgradable) -> forceModuleActive")]
        public bool allowActivationOnMobileDevice
        {
            get { return m_ForceModuleActive; }
            set { m_ForceModuleActive = value; }
        }

        /// <summary>
        /// Force this module to be active.
        /// </summary>
        /// <remarks>
        /// If there is no module active with higher priority (ordered in the inspector) this module will be forced active even if valid enabling conditions are not met.
        /// </remarks>
        public bool forceModuleActive
        {
            get { return m_ForceModuleActive; }
            set { m_ForceModuleActive = value; }
        }

        /// <summary>
        /// Number of keyboard / controller inputs allowed per second.
        /// </summary>
        public float inputActionsPerSecond
        {
            get { return m_InputActionsPerSecond; }
            set { m_InputActionsPerSecond = value; }
        }

        /// <summary>
        /// Delay in seconds before the input actions per second repeat rate takes effect.
        /// </summary>
        /// <remarks>
        /// If the same direction is sustained, the inputActionsPerSecond property can be used to control the rate at which events are fired. However, it can be desirable that the first repetition is delayed, so the user doesn't get repeated actions by accident.
        /// </remarks>
        public float repeatDelay
        {
            get { return m_RepeatDelay; }
            set { m_RepeatDelay = value; }
        }

        /// <summary>
        /// Name of the horizontal axis for movement (if axis events are used).
        /// </summary>
        public string horizontalAxis
        {
            get { return m_HorizontalAxis; }
            set { m_HorizontalAxis = value; }
        }

        /// <summary>
        /// Name of the vertical axis for movement (if axis events are used).
        /// </summary>
        public string verticalAxis
        {
            get { return m_VerticalAxis; }
            set { m_VerticalAxis = value; }
        }

        /// <summary>
        /// Maximum number of input events handled per second.
        /// </summary>
        public string submitButton
        {
            get { return m_SubmitButton; }
            set { m_SubmitButton = value; }
        }

        /// <summary>
        /// Input manager name for the 'cancel' button.
        /// </summary>
        public string cancelButton
        {
            get { return m_CancelButton; }
            set { m_CancelButton = value; }
        }

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 指定のゲームオブジェクト(UI)がホバー対象になっているか判定する
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public bool IsHovering( GameObject target )
		{
			if( m_InputPointerEvent == null || m_InputPointerEvent.hovered == null || m_InputPointerEvent.hovered.Count == 0 )
			{
				return false ;
			}

			var hoveredCount = m_InputPointerEvent.hovered.Count ;
			for( var i  = 0 ; i <  hoveredCount ; ++ i )
			{
				if( m_InputPointerEvent.hovered[ i ] != null && m_InputPointerEvent.hovered[ i ] == target )
				{
					return true ;
				}
			}

			return false ;
		}

		/// <summary>
		/// 指定のゲームオブジェクト(UI)がプレス対象になっているか判定する
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public bool IsPressing( GameObject target )
		{
			if( m_InputPointerEvent == null || m_InputPointerEvent.pointerPress == null )
			{
				return false ;
			}

			return m_InputPointerEvent.pointerPress == target ;
		}

		//-------------------------------------------------------------------------------------------


        private bool ShouldIgnoreEventsOnNoFocus()
        {
#if UNITY_EDITOR
            return !UnityEditor.EditorApplication.isRemoteConnected;
#else
            return true;
#endif
        }

        public override void UpdateModule()
        {
            if (!eventSystem.isFocused && ShouldIgnoreEventsOnNoFocus())
            {
                if (m_InputPointerEvent != null && m_InputPointerEvent.pointerDrag != null && m_InputPointerEvent.dragging)
                {
                    ReleaseMouse(m_InputPointerEvent, m_InputPointerEvent.pointerCurrentRaycast.gameObject);
                }

                m_InputPointerEvent = null;

                return;
            }

            m_LastMousePosition = m_MousePosition;
            m_MousePosition = input.mousePosition;
        }

        private void ReleaseMouse(PointerEventData pointerEvent, GameObject currentOverGo)
        {
            ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

            var pointerClickHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

            // PointerClick and Drop events
            if (pointerEvent.pointerClick == pointerClickHandler && pointerEvent.eligibleForClick)
            {
                ExecuteEvents.Execute(pointerEvent.pointerClick, pointerEvent, ExecuteEvents.pointerClickHandler);
            }
            if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
            {
                ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.dropHandler);
            }

            pointerEvent.eligibleForClick = false;
            pointerEvent.pointerPress = null;
            pointerEvent.rawPointerPress = null;
            pointerEvent.pointerClick = null;

            if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);

            pointerEvent.dragging = false;
            pointerEvent.pointerDrag = null;

            // redo pointer enter / exit to refresh state
            // so that if we moused over something that ignored it before
            // due to having pressed on something else
            // it now gets it.
            if (currentOverGo != pointerEvent.pointerEnter)
            {
                HandlePointerExitAndEnter(pointerEvent, null);
                HandlePointerExitAndEnter(pointerEvent, currentOverGo);
            }

            m_InputPointerEvent = pointerEvent;
        }

        public override bool IsModuleSupported()
        {
            return m_ForceModuleActive || input.mousePresent || input.touchSupported;
        }

        public override bool ShouldActivateModule()
        {
            if (!base.ShouldActivateModule())
                return false;

            var shouldActivate = m_ForceModuleActive;
            shouldActivate |= input.GetButtonDown(m_SubmitButton);
            shouldActivate |= input.GetButtonDown(m_CancelButton);
            shouldActivate |= !Mathf.Approximately(input.GetAxisRaw(m_HorizontalAxis), 0.0f);
            shouldActivate |= !Mathf.Approximately(input.GetAxisRaw(m_VerticalAxis), 0.0f);
            shouldActivate |= (m_MousePosition - m_LastMousePosition).sqrMagnitude > 0.0f;
            shouldActivate |= input.GetMouseButtonDown(0);

            if (input.touchCount > 0)
                shouldActivate = true;

            return shouldActivate;
        }

        /// <summary>
        /// See BaseInputModule.
        /// </summary>
        public override void ActivateModule()
        {
            if (!eventSystem.isFocused && ShouldIgnoreEventsOnNoFocus())
                return;

            base.ActivateModule();
            m_MousePosition = input.mousePosition;
            m_LastMousePosition = input.mousePosition;

            var toSelect = eventSystem.currentSelectedGameObject;
            if (toSelect == null)
                toSelect = eventSystem.firstSelectedGameObject;

            eventSystem.SetSelectedGameObject(toSelect, GetBaseEventData());
        }

        /// <summary>
        /// See BaseInputModule.
        /// </summary>
        public override void DeactivateModule()
        {
            base.DeactivateModule();
            ClearSelection();
        }

        public override void Process()
        {
            if (!eventSystem.isFocused && ShouldIgnoreEventsOnNoFocus())
                return;

            bool usedEvent = SendUpdateEventToSelectedObject();

            // case 1004066 - touch / mouse events should be processed before navigation events in case
            // they change the current selected gameobject and the submit button is a touch / mouse button.

            // touch needs to take precedence because of the mouse emulation layer
            if (!ProcessTouchEvents() && input.mousePresent)
                ProcessMouseEvent();

            if (eventSystem.sendNavigationEvents)
            {
                if (!usedEvent)
                    usedEvent |= SendMoveEventToSelectedObject();

                if (!usedEvent)
                    SendSubmitEventToSelectedObject();
            }
        }

        private bool ProcessTouchEvents()
        {
            for (int i = 0; i < input.touchCount; ++i)
            {
                Touch touch = input.GetTouch(i);

                if (touch.type == TouchType.Indirect)
                    continue;

                bool released;
                bool pressed;
				bool pressing ;
                var pointer = GetTouchPointerEventData(touch, out pressed, out released, out pressing);

                ProcessTouchPress(pointer, pressed, released);

                if (!released)
                {
                    ProcessMove(pointer);
                    ProcessDrag(pointer);
                }
                else
                    RemovePointerData(pointer);
            }
            return input.touchCount > 0;
        }

        /// <summary>
        /// This method is called by Unity whenever a touch event is processed. Override this method with a custom implementation to process touch events yourself.
        /// </summary>
        /// <param name="pointerEvent">Event data relating to the touch event, such as position and ID to be passed to the touch event destination object.</param>
        /// <param name="pressed">This is true for the first frame of a touch event, and false thereafter. This can therefore be used to determine the instant a touch event occurred.</param>
        /// <param name="released">This is true only for the last frame of a touch event.</param>
        /// <remarks>
        /// This method can be overridden in derived classes to change how touch press events are handled.
        /// </remarks>
        protected void ProcessTouchPress(PointerEventData pointerEvent, bool pressed, bool released)
        {
            var currentOverGo = pointerEvent.pointerCurrentRaycast.gameObject;

            // PointerDown notification
            if (pressed)
            {
                pointerEvent.eligibleForClick = true;
                pointerEvent.delta = Vector2.zero;
                pointerEvent.dragging = false;
                pointerEvent.useDragThreshold = true;
                pointerEvent.pressPosition = pointerEvent.position;
                pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;

                DeselectIfSelectionChanged(currentOverGo, pointerEvent);

                if (pointerEvent.pointerEnter != currentOverGo)
                {
                    // send a pointer enter to the touched element if it isn't the one to select...
                    HandlePointerExitAndEnter(pointerEvent, currentOverGo);
                    pointerEvent.pointerEnter = currentOverGo;
                }

                // search for the control that will receive the press
                // if we can't find a press handler set the press
                // handler to be what would receive a click.
                var newPressed = ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.pointerDownHandler);

                var newClick = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

                // didnt find a press handler... search for a click handler
                if (newPressed == null)
                    newPressed = newClick;

                // Debug.Log("Pressed: " + newPressed);

                float time = Time.unscaledTime;

                if (newPressed == pointerEvent.lastPress)
                {
                    var diffTime = time - pointerEvent.clickTime;
                    if (diffTime < 0.3f)
                        ++pointerEvent.clickCount;
                    else
                        pointerEvent.clickCount = 1;

                    pointerEvent.clickTime = time;
                }
                else
                {
                    pointerEvent.clickCount = 1;
                }

                pointerEvent.pointerPress = newPressed;
                pointerEvent.rawPointerPress = currentOverGo;
                pointerEvent.pointerClick = newClick;

                pointerEvent.clickTime = time;

                // Save the drag handler as well
                pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);

                if (pointerEvent.pointerDrag != null)
                    ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);
            }

            // PointerUp notification
            if (released)
            {
                // Debug.Log("Executing pressup on: " + pointer.pointerPress);
                ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

                // Debug.Log("KeyCode: " + pointer.eventData.keyCode);

                // see if we mouse up on the same element that we clicked on...
                var pointerClickHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

                // PointerClick and Drop events
                if (pointerEvent.pointerClick == pointerClickHandler && pointerEvent.eligibleForClick)
                {
                    ExecuteEvents.Execute(pointerEvent.pointerClick, pointerEvent, ExecuteEvents.pointerClickHandler);
                }

                if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
                {
                    ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.dropHandler);
                }

                pointerEvent.eligibleForClick = false;
                pointerEvent.pointerPress = null;
                pointerEvent.rawPointerPress = null;
                pointerEvent.pointerClick = null;

                if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
                    ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);

                pointerEvent.dragging = false;
                pointerEvent.pointerDrag = null;

                // send exit events as we need to simulate this on touch up on touch device
                ExecuteEvents.ExecuteHierarchy(pointerEvent.pointerEnter, pointerEvent, ExecuteEvents.pointerExitHandler);
                pointerEvent.pointerEnter = null;
            }

            m_InputPointerEvent = pointerEvent;
        }

        /// <summary>
        /// Calculate and send a submit event to the current selected object.
        /// </summary>
        /// <returns>If the submit event was used by the selected object.</returns>
        protected bool SendSubmitEventToSelectedObject()
        {
            if (eventSystem.currentSelectedGameObject == null)
                return false;

            var data = GetBaseEventData();
            if (input.GetButtonDown(m_SubmitButton))
                ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.submitHandler);

            if (input.GetButtonDown(m_CancelButton))
                ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.cancelHandler);
            return data.used;
        }

        private Vector2 GetRawMoveVector()
        {
            Vector2 move = Vector2.zero;
            move.x = input.GetAxisRaw(m_HorizontalAxis);
            move.y = input.GetAxisRaw(m_VerticalAxis);

            if (input.GetButtonDown(m_HorizontalAxis))
            {
                if (move.x < 0)
                    move.x = -1f;
                if (move.x > 0)
                    move.x = 1f;
            }
            if (input.GetButtonDown(m_VerticalAxis))
            {
                if (move.y < 0)
                    move.y = -1f;
                if (move.y > 0)
                    move.y = 1f;
            }
            return move;
        }

        /// <summary>
        /// Calculate and send a move event to the current selected object.
        /// </summary>
        /// <returns>If the move event was used by the selected object.</returns>
        protected bool SendMoveEventToSelectedObject()
        {
            float time = Time.unscaledTime;

            Vector2 movement = GetRawMoveVector();
            if (Mathf.Approximately(movement.x, 0f) && Mathf.Approximately(movement.y, 0f))
            {
                m_ConsecutiveMoveCount = 0;
                return false;
            }

            bool similarDir = (Vector2.Dot(movement, m_LastMoveVector) > 0);

            // If direction didn't change at least 90 degrees, wait for delay before allowing consequtive event.
            if (similarDir && m_ConsecutiveMoveCount == 1)
            {
                if (time <= m_PrevActionTime + m_RepeatDelay)
                    return false;
            }
            // If direction changed at least 90 degree, or we already had the delay, repeat at repeat rate.
            else
            {
                if (time <= m_PrevActionTime + 1f / m_InputActionsPerSecond)
                    return false;
            }

            var axisEventData = GetAxisEventData(movement.x, movement.y, 0.6f);

            if (axisEventData.moveDir != MoveDirection.None)
            {
                ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, axisEventData, ExecuteEvents.moveHandler);
                if (!similarDir)
                    m_ConsecutiveMoveCount = 0;
                m_ConsecutiveMoveCount++;
                m_PrevActionTime = time;
                m_LastMoveVector = movement;
            }
            else
            {
                m_ConsecutiveMoveCount = 0;
            }

            return axisEventData.used;
        }

        protected void ProcessMouseEvent()
        {
            ProcessMouseEvent(0);
        }

        [Obsolete("This method is no longer checked, overriding it with return true does nothing!")]
        protected virtual bool ForceAutoSelect()
        {
            return false;
        }

        /// <summary>
        /// Process all mouse events.
        /// </summary>
        protected void ProcessMouseEvent(int id)
        {
            var mouseData = GetMousePointerEventData(id);
            var leftButtonData = mouseData.GetButtonState(PointerEventData.InputButton.Left).eventData;

            m_CurrentFocusedGameObject = leftButtonData.buttonData.pointerCurrentRaycast.gameObject;

            // Process the first mouse button fully
            ProcessMousePress(leftButtonData);
            ProcessMove(leftButtonData.buttonData);
            ProcessDrag(leftButtonData.buttonData);

            // Now process right / middle clicks
            ProcessMousePress(mouseData.GetButtonState(PointerEventData.InputButton.Right).eventData);
            ProcessDrag(mouseData.GetButtonState(PointerEventData.InputButton.Right).eventData.buttonData);
            ProcessMousePress(mouseData.GetButtonState(PointerEventData.InputButton.Middle).eventData);
            ProcessDrag(mouseData.GetButtonState(PointerEventData.InputButton.Middle).eventData.buttonData);

            if (!Mathf.Approximately(leftButtonData.buttonData.scrollDelta.sqrMagnitude, 0.0f))
            {
                var scrollHandler = ExecuteEvents.GetEventHandler<IScrollHandler>(leftButtonData.buttonData.pointerCurrentRaycast.gameObject);
                ExecuteEvents.ExecuteHierarchy(scrollHandler, leftButtonData.buttonData, ExecuteEvents.scrollHandler);
            }
        }

        protected bool SendUpdateEventToSelectedObject()
        {
            if (eventSystem.currentSelectedGameObject == null)
                return false;

            var data = GetBaseEventData();
            ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.updateSelectedHandler);
            return data.used;
        }

        /// <summary>
        /// Calculate and process any mouse button state changes.
        /// </summary>
        protected void ProcessMousePress(MouseButtonEventData data)
        {
            var pointerEvent = data.buttonData;
            var currentOverGo = pointerEvent.pointerCurrentRaycast.gameObject;

            // PointerDown notification
            if (data.PressedThisFrame())
            {
                pointerEvent.eligibleForClick = true;
                pointerEvent.delta = Vector2.zero;
                pointerEvent.dragging = false;
                pointerEvent.useDragThreshold = true;
                pointerEvent.pressPosition = pointerEvent.position;
                pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;

                DeselectIfSelectionChanged(currentOverGo, pointerEvent);

                // search for the control that will receive the press
                // if we can't find a press handler set the press
                // handler to be what would receive a click.
                var newPressed = ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.pointerDownHandler);
                var newClick = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

                // didnt find a press handler... search for a click handler
                if (newPressed == null)
                    newPressed = newClick;

                // Debug.Log("Pressed: " + newPressed);

                float time = Time.unscaledTime;

                if (newPressed == pointerEvent.lastPress)
                {
                    var diffTime = time - pointerEvent.clickTime;
                    if (diffTime < 0.3f)
                        ++pointerEvent.clickCount;
                    else
                        pointerEvent.clickCount = 1;

                    pointerEvent.clickTime = time;
                }
                else
                {
                    pointerEvent.clickCount = 1;
                }

                pointerEvent.pointerPress = newPressed;
                pointerEvent.rawPointerPress = currentOverGo;
                pointerEvent.pointerClick = newClick;

                pointerEvent.clickTime = time;

                // Save the drag handler as well
                pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);

                if (pointerEvent.pointerDrag != null)
                    ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);

                m_InputPointerEvent = pointerEvent;
            }

            // PointerUp notification
            if (data.ReleasedThisFrame())
            {
                ReleaseMouse(pointerEvent, currentOverGo);
            }
        }

        protected GameObject GetCurrentFocusedGameObject()
        {
            return m_CurrentFocusedGameObject;
        }
    }
}
#endif
