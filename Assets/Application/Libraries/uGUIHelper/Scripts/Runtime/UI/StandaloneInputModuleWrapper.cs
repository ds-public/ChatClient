using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.EventSystems ;

namespace uGUIHelper
{
	[DefaultExecutionOrder(-10000)]
	public class StandaloneInputModuleWrapper : StandaloneInputModule
	{
		public enum ProcessType
		{
			Default	= 0,
			Custom	= 1,
		}

		private ProcessType	m_ProcessType = ProcessType.Default ;

		//-----------------------------------

		/// <summary>
		/// 方向キーのスムース動作
		/// </summary>
		public bool UseSmoothAxis = false ;

		/// <summary>
		/// スムース動作係数
		/// </summary>
		public float SmoothAxisMultiplier = 0.01f ;

		//-----------------------------------------------------------

		// レイキャストでヒットした情報のキャッシュ用
		private PointerEventData m_LookData = null ;

		// レイキャストヒット中のＵＩ
		private GameObject m_CurrentLook = null ;
		
		// 現在プレス状態にあるＵＩ
		private GameObject m_CurrentPressed = null ;

		// 現在ドラッグ状態にあるＵＩ
		private GameObject m_CurrentDragging = null ;

		// 移動系の挙動の時間管理用
		private float m_NextAxisActionTime = 0 ;

		// プレス挙動のアクション
		private Func<GameObject,bool>	m_OnPressAction = null ;

		/// <summary>
		/// プレス挙動のアクションを設定する
		/// </summary>
		/// <param name="onPressAction"></param>
		public void SetOnPressAction( Func<GameObject,bool> onPressAction )
		{
			m_OnPressAction = onPressAction ;
		}

		private bool m_IsPressAction = false ;

		/// <summary>
		/// 注目しているオブジェクトを返す
		/// </summary>
		/// <returns></returns>
		public GameObject GetLookingObject()
		{
			return m_CurrentLook ;
		}


		/// <summary>
		/// 指定のゲームオブジェクト(UI)がホバー対象になっているか判定する
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public bool IsHovering( GameObject target )
		{
/*			if( m_InputPointerEvent == null || m_InputPointerEvent.hovered == null || m_InputPointerEvent.hovered.Count == 0 )
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
			}*/

			return false ;
		}

		/// <summary>
		/// 指定のゲームオブジェクト(UI)がプレス対象になっているか判定する
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public bool IsPressing( GameObject target )
		{
/*			if( m_InputPointerEvent == null || m_InputPointerEvent.pointerPress == null )
			{
				return false ;
			}

			return m_InputPointerEvent.pointerPress == target ;*/
			return false ;
		}

		//-----------------------------------------------------------

		// 選択中のオブジェクトの状態を更新する
/*		private bool SendUpdateEventToSelectedObject()
		{
			if( eventSystem.currentSelectedGameObject == null )
			{
				return false ;
			}

			BaseEventData tData = GetBaseEventData() ;
			ExecuteEvents.Execute( eventSystem.currentSelectedGameObject, tData, ExecuteEvents.updateSelectedHandler ) ;
			return tData.used ;
		}*/
		
		//-----------------------------------------------------------

		// 毎フレームの処理
		public override void Process()
		{
			if( m_ProcessType == ProcessType.Default )
			{
				// Default
				base.Process() ;
				return ;
			}
			else
			{
				// Custom
				if( XRSettings.enabled == false )
				{
					// PC用
					ProcessForPC() ;
				}
				else
				{
					// XR用
					ProcessForXR() ;
				}
			}
		}

		// ＰＣ用
		public void ProcessForPC()
		{
			bool usedEvent = SendUpdateEventToSelectedObject() ;

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

			ProcessMouseEventForPC() ;
		}

		private bool m_IsPressed = false ;
		private GameObject m_LastPress = null ;

		// ＰＣ用のマウスイベントを処理する
		private void ProcessMouseEventForPC()
		{
			if( m_Disable == true )
			{
				return ;
			}

			//----------------------------------

			MouseState mouseData = GetMousePointerEventData() ;
			
			bool pressed	= mouseData.AnyPressesThisFrame() ;
			bool released	= mouseData.AnyReleasesThisFrame() ;

			MouseButtonEventData leftButtonData = mouseData.GetButtonState( PointerEventData.InputButton.Left ).eventData ;

			if( leftButtonData.PressedThisFrame() == true )
			{
				m_LastPress = leftButtonData.buttonData.pointerCurrentRaycast.gameObject ;
//				Debug.LogWarning( "LastPress設定:" + m_LastPress ) ;
			}

			// レイキャスト消失により Relese 状態になった場合の対応
			bool isPressed = Input.GetMouseButton( 0 ) ;
			if( isPressed != m_IsPressed )
			{
//				if( isPressed == false )
//				{
//					Debug.LogWarning( "直判定で離された:" + leftButtonData.buttonState ) ;
//				}
	
				if( released == false && isPressed == false )
				{
					released = true ;

					if( leftButtonData.buttonState == PointerEventData.FramePressState.NotChanged )
					{
						leftButtonData.buttonState  = PointerEventData.FramePressState.Released ;

//						Debug.LogWarning( "LastPress:" + m_LastPress ) ;

						leftButtonData.buttonData.pointerPress = m_LastPress ;
						m_LastPress = null ;
					}
				}
	
				m_IsPressed = isPressed ;
			}

			// マウスオーバーに関してはマウスが動いていなくても取りたい
			ProcessHoverForPC( leftButtonData.buttonData ) ;

			if( UseMouseForPC( pressed, released, leftButtonData.buttonData ) == false )
			{
				// 何のイベントも無い
				return ;
			}

			// Process the first mouse button fully
			ProcessMousePress( leftButtonData ) ;	// デフォルトでは途中でレイキャストが変わった場合のリリースがうまくいかないので改造版を使用する
//			ProcessMove( leftButtonData.buttonData ) ;
			ProcessDrag( leftButtonData.buttonData ) ;

			// Now process right / middle clicks
//			ProcessMousePress( mouseData.GetButtonState( PointerEventData.InputButton.Right ).eventData ) ;
//			ProcessDrag( mouseData.GetButtonState( PointerEventData.InputButton.Right ).eventData.buttonData ) ;
//			ProcessMousePress( mouseData.GetButtonState( PointerEventData.InputButton.Middle ).eventData ) ;
//			ProcessDrag( mouseData.GetButtonState(PointerEventData.InputButton.Middle).eventData.buttonData ) ;

			if( Mathf.Approximately( leftButtonData.buttonData.scrollDelta.sqrMagnitude, 0.0f ) == false )
			{
				GameObject scrollHandler = ExecuteEvents.GetEventHandler<IScrollHandler>( leftButtonData.buttonData.pointerCurrentRaycast.gameObject ) ;
				ExecuteEvents.ExecuteHierarchy( scrollHandler, leftButtonData.buttonData, ExecuteEvents.scrollHandler ) ;
			}
		}

		// 何らかのイベントが発生中かの確認を行う(処理を軽くするためのもの)
		private static bool UseMouseForPC( bool pressed, bool released, PointerEventData pointerData )
		{
			if( pressed || released || pointerData.IsPointerMoving() || pointerData.IsScrolling() )
			{
				return true ;	// 何らかのイベントが発生している
			}

			return false ;	// 何もイベントは発生していない
		}

		// Hoverの処理
		private void ProcessHoverForPC( PointerEventData pointerEventData )
		{
			GameObject newEnterTarget = pointerEventData.pointerCurrentRaycast.gameObject ;

			// if we have not changed hover target
//			if( pointerEventData.pointerEnter == newEnterTarget )
//			{
//				return ;
//			}

			if( pointerEventData.pointerEnter == newEnterTarget )
			{
				// 以前と変わらない
				if( newEnterTarget != null )
				{
					// 現在の Enter(同じであってもメッセージを送る)
					Transform t = newEnterTarget.transform ;
					
					while( t != null )
					{
						// 中に入った事を通知する
						ExecuteEvents.Execute( t.gameObject, pointerEventData, ExecuteEvents.pointerEnterHandler ) ;
						t = t.parent ;
					}
				}

				return ;	// ここで終了
			}

			//----------------------------------------------------------
			// 以前と状況が異なる(入る・出る)

			GameObject commonRoot = FindCommonRoot( pointerEventData.pointerEnter, newEnterTarget ) ;

			//----------------------------------
			// 以前と状況が異なるため以前の箇所からは出ている

			if( pointerEventData.pointerEnter != null )
			{
				// 古い Enter
				Transform t = pointerEventData.pointerEnter.transform ;

				while( t != null )
				{
					if( commonRoot != null && commonRoot.transform == t )
					{
						break ;
					}
					
					// 外に出た事を通知する
					if( t.gameObject.gameObject.activeInHierarchy == false )
					{
						// 非アクティブだと SendMessage は機能しないため直接コンポーネントを取得して送信する

						var interaction = t.gameObject.GetComponent<UIInteraction>() ;
						if( interaction != null )
						{
							interaction.OnPointerExit( pointerEventData ) ;
						}

						var interactionForScrollView = t.gameObject.GetComponent<UIInteractionForScrollView>() ;
						if( interactionForScrollView != null )
						{
							interactionForScrollView.OnPointerExit( pointerEventData ) ;
						}
					}
					else
					{
						ExecuteEvents.Execute( t.gameObject, pointerEventData, ExecuteEvents.pointerExitHandler ) ;
					}
					t = t.parent ;
				}
			}

			//----------------------------------
			// 以前と状況が異なるため現在の箇所に入っている

			if( newEnterTarget != null )
			{
				// 新しい Enter
				Transform t = newEnterTarget.transform ;
				
				while( t != null && t.gameObject != commonRoot )
				{
					// 中に入った事を通知する
					ExecuteEvents.Execute( t.gameObject, pointerEventData, ExecuteEvents.pointerEnterHandler ) ;
					t = t.parent ;
				}
			}
			
			// 更新
			pointerEventData.pointerEnter = newEnterTarget ;
		}

		//-----------------------------------------------------------

		private bool m_Disable ;

		public void Disable()
		{
			if( m_ProcessType == ProcessType.Default )
			{
				// Default
				return ;
			}

			MouseState mouseData = GetMousePointerEventData() ;
			MouseButtonEventData leftButtonData = mouseData.GetButtonState( PointerEventData.InputButton.Left ).eventData ;
			PointerEventData pointerEventData = leftButtonData.buttonData ;

			// Press 中なら強制解除
			if( pointerEventData.pointerPress != null )
			{
				ExecuteEvents.Execute( pointerEventData.pointerPress, pointerEventData, ExecuteEvents.pointerUpHandler ) ;
				
//				GameObject pointerUpHandler =
					ExecuteEvents.GetEventHandler<IPointerClickHandler>( pointerEventData.pointerCurrentRaycast.gameObject );

				// PointerClick and Drop events
//				if( pointerEventData.pointerPress == pointerUpHandler && pointerEventData.eligibleForClick )
//				{
//					ExecuteEvents.Execute( pointerEventData.pointerPress, pointerEventData, ExecuteEvents.pointerClickHandler ) ;
//				}
//				else
//				if( pointerEventData.pointerDrag != null && pointerEventData.dragging == true )
//				{
//					ExecuteEvents.ExecuteHierarchy( pointerEventData.pointerCurrentRaycast.gameObject, pointerEventData, ExecuteEvents.dropHandler ) ;
//				}

				pointerEventData.eligibleForClick = false ;
				pointerEventData.pointerPress = null ;
				pointerEventData.rawPointerPress = null ;
			}

			if( pointerEventData.pointerDrag != null && pointerEventData.dragging == true )
			{
				ExecuteEvents.Execute( pointerEventData.pointerDrag, pointerEventData, ExecuteEvents.endDragHandler ) ;

				pointerEventData.dragging = false ;
				pointerEventData.pointerDrag = null ;
			}

			// Hover 中なら強制解除
			if( pointerEventData.pointerEnter != null )
			{
				HandlePointerExitAndEnter( pointerEventData, null ) ;
			}

			m_Disable = true ;
		}

		public void Enable()
		{
			m_Disable = false ;
		}

		//-----------------------------------------------------------

		// ＸＲ用
		private void ProcessForXR()
		{
			// 選択中のオブジェクトの状態を更新する
			SendUpdateEventToSelectedObject() ;	// デフォルトのものを使用する

			// レイキャストでヒットしたＵＩの情報を取得する(ＶＲ限定)
			PointerEventData lookData = GetLookPointerEventData( UnityEngine.XR.XRSettings.eyeTextureWidth  / 2, UnityEngine.XR.XRSettings.eyeTextureHeight / 2 ) ;
			m_CurrentLook = lookData.pointerCurrentRaycast.gameObject ;

			// deselect when look away
			if( m_CurrentLook == null )
			{
				// 選択中のＵＩを非選択状態にする
				ClearSelection() ;
			}

			// ＵＩの領域に入ったまたは出た際の処理を実行する
			HandlePointerExitAndEnter( lookData, m_CurrentLook ) ;

			//----------------------------------

			bool isPressAction = false ;
			if( m_OnPressAction != null )
			{
				// アクションボタン指定あり
				isPressAction = m_OnPressAction( m_CurrentLook ) ;
			}

			//----------------------------------

			// 押された判定
			bool down = false ;
			if( m_OnPressAction == null )
			{
				if( Input.GetMouseButtonDown( 0 ) == true )
				{
					down = true ;
				}
			}
			else
			{
				if( m_IsPressAction == false && isPressAction == true )
				{
					m_IsPressAction = true ;
					down = true ;
				}
			}

			if( m_CurrentLook != null )
			{
				// ＵＩの上にある

				//---------------------------------

				if( down == true )
				{
					// 押された

					// 選択中のＵＩの表示を非選択状態に戻す
					ClearSelection() ;

					lookData.pressPosition			= lookData.position ;
					lookData.pointerPressRaycast	= lookData.pointerCurrentRaycast ;
					lookData.pointerPress			= null ;

					// プレス判定
					m_CurrentPressed = ExecuteEvents.ExecuteHierarchy( m_CurrentLook, lookData, ExecuteEvents.pointerDownHandler ) ;

					if( m_CurrentPressed != null )
					{
						// 階層上にプレスに反応するものがある

						// 新規プレス
						lookData.pointerPress = m_CurrentPressed ;

						// 表示状態を変更する
						ClearSelection() ;	// 古い方の状態を解除

						// フォーカスを当てられるか判定する
						if( ExecuteEvents.GetEventHandler<ISelectHandler>( m_CurrentPressed ) != null )
						{
							// フォーカスをあてる
							eventSystem.SetSelectedGameObject( m_CurrentPressed ) ;
						}
					}

					// ドラッグ開始
					ExecuteEvents.Execute( m_CurrentPressed, lookData, ExecuteEvents.beginDragHandler ) ;

					lookData.pointerDrag	= m_CurrentPressed ;
					m_CurrentDragging		= m_CurrentPressed ;
				}
			}

			//----------------------------------------------------------

			// 離された判定
			bool up = false ;
			if( m_OnPressAction == null )
			{
				if( Input.GetMouseButtonUp( 0 ) == true )
				{
					up = true ;
				}
			}
			else
			{
				if( m_IsPressAction == true && isPressAction == false )
				{
					m_IsPressAction = false ;
					up = true ;
				}
			}

			//----------------------------------

			if( up == true )
			{
				// 離された

				if( m_CurrentDragging != null )
				{
					// ドラッグ中ならドラッグを終了する

					// ドラッグ終了
					ExecuteEvents.Execute( m_CurrentDragging, lookData, ExecuteEvents.endDragHandler ) ;

					if( m_CurrentLook != null )
					{
						// ドロップ実行
						ExecuteEvents.ExecuteHierarchy( m_CurrentLook, lookData, ExecuteEvents.dropHandler ) ;
					}

					lookData.pointerDrag	= null ;
					m_CurrentDragging		= null ;
				}

				if( m_CurrentPressed != null )
				{
					// プレス中ならプレスを終了する

					// プレス終了
					ExecuteEvents.Execute( m_CurrentPressed, lookData, ExecuteEvents.pointerUpHandler ) ;

					lookData.rawPointerPress	= null ;
					lookData.pointerPress		= null ;

					if( m_CurrentLook != null )
					{
						// クリックを判定する
						if( ExecuteEvents.GetEventHandler<IPointerDownHandler>( m_CurrentLook ) == m_CurrentPressed )
						{
							// クリックされた
							ExecuteEvents.Execute( m_CurrentPressed, lookData, ExecuteEvents.pointerClickHandler ) ;
						}
					}

					m_CurrentPressed			= null ;
				}
			}

			// drag handling
			if( m_CurrentDragging != null )
			{
				// ドラッグ実行
				ExecuteEvents.Execute( m_CurrentDragging, lookData, ExecuteEvents.dragHandler ) ;
			}

			//----------------------------------------------------------

			if( m_CurrentLook != null )
			{
				// 方向キー操作
				if( eventSystem.currentSelectedGameObject != null && string.IsNullOrEmpty( horizontalAxis ) == false )
				{
					float newValue = Input.GetAxis( horizontalAxis ) ;
					if( newValue >   0.01f || newValue <  -0.01f )
					{
						if( UseSmoothAxis == true )
						{
							// スムース動作有効
							Slider sl = eventSystem.currentSelectedGameObject.GetComponent<Slider>() ;
							if( sl != null )
							{
								// 対象はスライダー
								float multi = sl.maxValue - sl.minValue ;
								sl.value += newValue * SmoothAxisMultiplier * multi ;
							}
							else
							{
								Scrollbar sb = eventSystem.currentSelectedGameObject.GetComponent<Scrollbar>() ;
								if( sb != null )
								{
									// 対象はスクロールバー
									sb.value += newValue * SmoothAxisMultiplier ;
								}
								ScrollbarWrapper sbw = eventSystem.currentSelectedGameObject.GetComponent<ScrollbarWrapper>() ;
								if( sbw != null )
								{
									// 対象はスクロールバー
									sbw.value += newValue * SmoothAxisMultiplier ;
								}
							}
						}
						else
						{
							// スムース動作はしない
							float time = Time.unscaledTime ;

							if( time >  m_NextAxisActionTime )
							{
								m_NextAxisActionTime = time + ( 1f / inputActionsPerSecond ) ;
								AxisEventData axisData = GetAxisEventData( newValue, 0.0f, 0.0f ) ;
								if( ExecuteEvents.Execute( eventSystem.currentSelectedGameObject, axisData, ExecuteEvents.moveHandler ) == false )
								{
								} 
							}
						}
					}
				}
			}
		}

		//-----------------------------------------------------------

		// レイキャストでヒットしたＵＩの情報を取得する(ＶＲ限定)
		private PointerEventData GetLookPointerEventData( float pointerX, float pointerY )
		{
			Vector2 lookPosition ;

			lookPosition.x = pointerX ;
			lookPosition.y = pointerY ;

			if( m_LookData == null )
			{
				m_LookData = new PointerEventData( eventSystem ) ;
			}

			m_LookData.Reset() ;
			m_LookData.delta		= Vector2.zero ;
			m_LookData.position		= lookPosition ;
			m_LookData.scrollDelta	= Vector2.zero ;

			eventSystem.RaycastAll( m_LookData, m_RaycastResultCache ) ;
			m_LookData.pointerCurrentRaycast = FindFirstRaycast( m_RaycastResultCache ) ;

			m_RaycastResultCache.Clear() ;

			return m_LookData ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 動作タイプを設定する
		/// </summary>
		/// <param name="processType"></param>
		public void SetProcessType( ProcessType processType )
		{
			m_ProcessType = processType ;
		}

		//-----------------------------------------------------------

		// 選択中のＵＩを非選択状態にする
/*		public void ClearSelection()
		{
			if( eventSystem.currentSelectedGameObject != null )
			{
//				RestoreColor( eventSystem.currentSelectedGameObject ) ;
				eventSystem.SetSelectedGameObject( null ) ;
			}
		}*/


		/// <summary>
		/// Process the current mouse press.
		/// </summary>
/*		private void ProcessMousePressCustom( MouseButtonEventData data )
		{
			var pointerEvent = data.buttonData ;
			var currentOverGo = pointerEvent.pointerCurrentRaycast.gameObject ;
			
			// PointerDown notification
			if( data.PressedThisFrame() )
			{
//				Debug.LogWarning( "押されています:" + currentOverGo.name ) ;

				pointerEvent.eligibleForClick = true ;
				pointerEvent.delta = Vector2.zero ;
				pointerEvent.dragging = false ;
				pointerEvent.useDragThreshold = true ;
				pointerEvent.pressPosition = pointerEvent.position ;
				pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast ;
				
				DeselectIfSelectionChanged( currentOverGo, pointerEvent ) ;

				// search for the control that will receive the press
				// if we can't find a press handler set the press
				// handler to be what would receive a click.
				var newPressed = ExecuteEvents.ExecuteHierarchy( currentOverGo, pointerEvent, ExecuteEvents.pointerDownHandler ) ;

				// didnt find a press handler... search for a click handler
				if( newPressed == null )
				{
					newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>( currentOverGo ) ;
				}

				// Debug.Log("Pressed: " + newPressed);

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
				
				pointerEvent.pointerPress = newPressed ;
				pointerEvent.rawPointerPress = currentOverGo ;
				
				pointerEvent.clickTime = time ;

				// Save the drag handler as well
				pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>( currentOverGo ) ;
				
				if( pointerEvent.pointerDrag != null )
				{
					ExecuteEvents.Execute( pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag ) ;
				}
			}

			// PointerUp notification
			if( data.ReleasedThisFrame() )
			{
//				Debug.LogWarning( "離されました:" + pointerEvent.pointerPress ) ;
				// Debug.Log("Executing pressup on: " + pointer.pointerPress);
				ExecuteEvents.Execute( pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler ) ;
				
				// Debug.Log("KeyCode: " + pointer.eventData.keyCode);

				// see if we mouse up on the same element that we clicked on...
				var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>( currentOverGo ) ;
				
				// PointerClick and Drop events
				if( pointerEvent.pointerPress == pointerUpHandler && pointerEvent.eligibleForClick )
				{
					ExecuteEvents.Execute( pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerClickHandler ) ;
				}
				else
				if( pointerEvent.pointerDrag != null )
				{
					ExecuteEvents.ExecuteHierarchy( currentOverGo, pointerEvent, ExecuteEvents.dropHandler ) ;
				}
				
				pointerEvent.eligibleForClick = false ;
				pointerEvent.pointerPress = null ;
				pointerEvent.rawPointerPress = null ;
				
				if( pointerEvent.pointerDrag != null && pointerEvent.dragging )
				{
					ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler ) ;
				}
				
				pointerEvent.dragging = false ;
				pointerEvent.pointerDrag = null ;
				
				// redo pointer enter / exit to refresh state
				// so that if we moused over somethign that ignored it before
				// due to having pressed on something else
				// it now gets it.
				if( currentOverGo != pointerEvent.pointerEnter )
				{
					HandlePointerExitAndEnter( pointerEvent, null ) ;
					HandlePointerExitAndEnter( pointerEvent, currentOverGo ) ;
				}
			}
		}*/


	}
}
