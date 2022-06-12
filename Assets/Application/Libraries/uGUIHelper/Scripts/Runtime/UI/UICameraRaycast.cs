using System ;
using System.Runtime.InteropServices ;

using UnityEngine ;
using UnityEngine.EventSystems ;
using UnityEngine.UI ;

namespace uGUIHelper
{
	/// <summary>
	/// カメラレイキャストを、UIのタップ位置から飛ばします
	/// </summary>
	[Serializable]
	[StructLayout( LayoutKind.Auto )]
	[DisallowMultipleComponent]
	[RequireComponent( typeof(RectTransform) )]
	public class UICameraRaycast : Selectable,
									IPointerClickHandler,
									IPointerDownHandler,
									IPointerUpHandler,
									IBeginDragHandler,
									IDragHandler,
									IEndDragHandler
	{
		[SerializeField]
		private Camera m_Camera = default ;

		[SerializeField]
		private LayerMask m_HitLayerMask = -1 ;

		[SerializeField]
		private QueryTriggerInteraction m_QueryTriggerInteraction = QueryTriggerInteraction.UseGlobal ;

		//-------------------------------------------------------------------------------------------

#if UNITY_EDITOR

		/// <summary>
		/// 選択された時のレイ情報
		/// </summary>
		[NonSerialized]
		private Ray m_DebugFromCameraRay ;

		[NonSerialized]
		private RaycastHit m_DebugHitInfo ;
#endif

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// カメラ設定
		/// </summary>
		public Camera Camera
		{
			get { return m_Camera ; }
			set { m_Camera = value ; }
		}

		public LayerMask HitLayerMask
		{
			get { return m_HitLayerMask ; }
			set { m_HitLayerMask = value ; }
		}

		public QueryTriggerInteraction QueryTriggerInteraction
		{
			get { return m_QueryTriggerInteraction ; }
			set { m_QueryTriggerInteraction = value ; }
		}

		/// <summary>
		/// ポインターダウンの状態か
		/// </summary>
		[field: NonSerialized]
		public bool IsPointerDown { get ; private set ; }

		/// <summary>
		/// ドラッグ中か
		/// </summary>
		[field: NonSerialized]
		public bool IsDrag { get ; private set ; }

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// クリックされたら呼ばれます
		/// </summary>
		public event Action<PointerEventData, Ray, RaycastHit> Click ;

		/// <summary>
		/// オブジェクトが何かしら押されたら呼ばれます
		/// </summary>
		public event Action<PointerEventData, Ray, RaycastHit> PointerDown ;

		/// <summary>
		/// ドラッグを開始したら呼ばれます
		/// </summary>
		public event Action<PointerEventData, Ray, RaycastHit> StartDrag ;

		/// <summary>
		/// ドラックされたら呼ばれます
		/// </summary>
		public event Action<PointerEventData, Ray, RaycastHit> Drag ;

		/// <summary>
		/// ドラッグが終了したら呼ばれます
		/// </summary>
		public event Action EndDrag ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// UI上の処理が実行できる状態なのか
		/// </summary>
		/// <returns></returns>
		public bool IsActiveAndInteractable()
		{
			if ( !IsActive() )
			{
				return false ;
			}

			if ( !IsInteractable() )
			{
				return false ;
			}

			return true ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 現在飛ばしているレイを更新します
		/// </summary>
		/// <param name="screenPos">スクリーン座標の位置</param>
		/// <param name="ray">飛んで行ったレイ</param>
		private void UpdateRay( in Vector2 screenPos, out Ray ray )
		{
			ray = RectTransformUtility.ScreenPointToRay( m_Camera, screenPos ) ;
#if UNITY_EDITOR
			m_DebugFromCameraRay = ray ;
#endif
		}

		/// <summary>
		/// レイを飛ばして、当たり判定を取得します
		/// </summary>
		/// <param name="screenPos"></param>
		/// <param name="ray"></param>
		/// <param name="hitInfo"></param>
		/// <returns></returns>
		private bool CheckHit( in Vector2 screenPos, out Ray ray, out RaycastHit hitInfo )
		{
			if ( !IsActiveAndInteractable() )
			{
				ray     = default ;
				hitInfo = default ;
				return false ;
			}

			UpdateRay( in screenPos, out ray ) ;

			float maxDistance = m_Camera.farClipPlane - m_Camera.nearClipPlane ;

			bool result = Physics.Raycast(
				ray,
				out hitInfo,
				maxDistance,
				m_HitLayerMask,
				m_QueryTriggerInteraction
			) ;
#if UNITY_EDITOR
			m_DebugHitInfo = hitInfo ;
#endif
			return result ;
		}

		//-------------------------------------------------------------------------------------------
		// UnityEngine.EventSystems.IBeginDragHandler
		#region UnityEngine.EventSystems.IBeginDragHandler

		/// <inheritdoc />
		public void OnBeginDrag( PointerEventData eventData )
		{
			if ( !CheckHit(
					eventData.position,
					out Ray ray,
					out RaycastHit hitInfo
				) )
			{
				return ;
			}

			IsDrag = true ;
			StartDrag?.Invoke(
				eventData,
				ray,
				hitInfo
			) ;
			DoStateTransition( SelectionState.Pressed, false ) ;
		}

		#endregion

		//-------------------------------------------------------------------------------------------
		// UnityEngine.EventSystems.IDragHandler
		#region UnityEngine.EventSystems.IDragHandler

		/// <inheritdoc />
		public void OnDrag( PointerEventData eventData )
		{
			if ( !IsDrag )
			{
				return ;
			}

			CheckHit( eventData.position, out Ray ray, out RaycastHit hitInfo ) ;
			Drag?.Invoke(
				eventData,
				ray,
				hitInfo
			) ;
			DoStateTransition( SelectionState.Pressed, false ) ;
		}

		#endregion


		//-------------------------------------------------------------------------------------------
		// UnityEngine.EventSystems.IEndDragHandler
		#region UnityEngine.EventSystems.IEndDragHandler

		/// <inheritdoc />
		public void OnEndDrag( PointerEventData eventData )
		{
			if ( IsDrag )
			{
				EndDrag?.Invoke() ;
				IsDrag = false ;
				DoStateTransition( currentSelectionState, false ) ;
			}
		}

		#endregion

		//-------------------------------------------------------------------------------------------
		// UnityEngine.EventSystems.IPointerClickHandler
		#region UnityEngine.EventSystems.IPointerClickHandler

		/// <inheritdoc />
		public void OnPointerClick( PointerEventData eventData )
		{
			if ( !IsActiveAndInteractable() )
			{
				return ;
			}

			// ドラッグ中はクリックできない
			if ( IsDrag )
			{
				return ;
			}

			if ( CheckHit( eventData.position, out Ray ray, out RaycastHit hitInfo ) )
			{
				Click?.Invoke(
					eventData,
					ray,
					hitInfo
				) ;
			}
		}

		#endregion

		//-------------------------------------------------------------------------------------------
		// UnityEngine.UI.Selectable
		#region UnityEngine.UI.Selectable

		/// <inheritdoc />
		public override bool IsActive()
		{
			if ( !base.IsActive() )
			{
				return false ;
			}

			if ( m_Camera == null )
			{
				return false ;
			}

			return m_Camera.enabled ;
		}

		/// <inheritdoc />
		protected override void OnDisable()
		{
			base.OnDisable() ;

			if ( IsDrag )
			{
				EndDrag?.Invoke() ;
				IsDrag = false ;
			}
		}

		/// <inheritdoc />
		public override void OnPointerDown( PointerEventData eventData )
		{
			if ( !IsActive() || !IsInteractable() )
			{
				return ;
			}

			// ドラッグ中はこの処理はしない
			if ( IsDrag )
			{
				return ;
			}

			// 3D空間上の物体を選択したのか
			if ( !CheckHit(
					eventData.position,
					out Ray ray,
					out RaycastHit hitInfo
				) )
			{
				return ;
			}

			IsPointerDown = true ;

			PointerDown?.Invoke(
				eventData,
				ray,
				hitInfo
			) ;

			// Selection tracking
			if ( IsInteractable() && ( navigation.mode != Navigation.Mode.None ) && EventSystem.current != null )
			{
				EventSystem.current.SetSelectedGameObject( gameObject, eventData ) ;
			}

			DoStateTransition( SelectionState.Pressed, false ) ;
		}

		/// <inheritdoc />
		public override void OnPointerUp( PointerEventData eventData )
		{
			if ( !IsPointerDown )
			{
				return ;
			}

			IsPointerDown = false ;
			DoStateTransition( currentSelectionState, false ) ;
		}

#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			if ( !IsActive() || !IsInteractable() )
			{
				return ;
			}

			Color oldColor = Gizmos.color ;

			Gizmos.color = new Color(
				1.0f,
				1.0f,
				1.0f,
				0.5f
			) ;
			if ( m_Camera.orthographic )
			{
			}
			else
			{
				Gizmos.DrawFrustum(
					m_Camera.transform.position,
					m_Camera.fieldOfView,
					m_Camera.nearClipPlane,
					m_Camera.farClipPlane,
					m_Camera.aspect
				) ;
			}

			if ( IsPointerDown && IsDrag )
			{
				Gizmos.color = new Color(
					1.0f,
					1.0f,
					1.0f,
					1.0f
				) ;

				float rayDistance = m_Camera.farClipPlane - m_Camera.nearClipPlane ;
				Gizmos.DrawRay( m_DebugFromCameraRay.origin, m_DebugFromCameraRay.direction * rayDistance ) ;

				if ( !m_DebugHitInfo.Equals( default ) )
				{
					Gizmos.color = Color.green ;

					Collider hitCollider = m_DebugHitInfo.collider ;
					if ( hitCollider != null )
					{
						Bounds bounds = hitCollider.bounds ;
						Gizmos.DrawWireCube( bounds.center, bounds.size ) ;
					}
				}
			}

			Gizmos.color = oldColor ;
		}

#endif

		#endregion
	}
}
