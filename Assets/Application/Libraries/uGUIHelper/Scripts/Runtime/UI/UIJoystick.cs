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
	/// ジョイスティッククラス(複合UI)
	/// </summary>
	public class UIJoystick : UIImage
	{
		/// <summary>
		/// 枠
		/// </summary>
		[SerializeField]
		protected UIImage m_Frame ;
		public    UIImage   Frame{ get{ return m_Frame ; } set{ m_Frame = value ; } }

		/// <summary>
		/// 棒
		/// </summary>
		[SerializeField]
		protected UIImage m_Thumb ;
		public    UIImage   Thumb{ get{ return m_Thumb ; } set{ m_Thumb = value ; } }

		/// <summary>
		/// 値が変化した際に呼び出すコールバック→( string identity, UIJoystick joystick, float magnitude, Vector2 direction )
		/// </summary>
		public Action<string, UIJoystick, float, Vector2> OnValueChangedAction ;

		public void SetOnValueChanged( Action<string, UIJoystick, float, Vector2> onValueChangedAction )
		{
			OnValueChangedAction = onValueChangedAction ;
		}

		/// <summary>
		/// 押された時と離された時に呼び出すコールバック→( string identity, UIJoyStick joystick, bool isPress, Vector2 position, float releaseTime )
		/// </summary>
		public Action<string, UIJoystick, bool, Vector2, float> OnActivateAction ;

		public void SetOnActivate( Action<string, UIJoystick, bool, Vector2, float> onActivateAction )
		{
			OnActivateAction = onActivateAction ;
		}

		/// <summary>
		/// Ｙ軸反転
		/// </summary>
		[SerializeField]
		protected bool m_YAxisInversion = false ;
		public    bool   YAxisInversion
		{
			get{ return m_YAxisInversion ; }
			set{ m_YAxisInversion = value ; }
		}

		/// <summary>
		/// 横方向機能停止
		/// </summary>
		[SerializeField]
		protected bool m_HorizontalFunctionStop = false ;
		public    bool   HorizontalFunctionStop
		{
			get{ return m_HorizontalFunctionStop ; }
			set{ m_HorizontalFunctionStop = value ; }
		}

		/// <summary>
		/// 縦方向機能停止
		/// </summary>
		[SerializeField]
		protected bool m_VerticalFunctionStop = false ;
		public    bool   VerticalFunctionStop
		{
			get{ return m_VerticalFunctionStop ; }
			set{ m_VerticalFunctionStop = value ; }
		}

		/// <summary>
		/// 位置の固定化と常時表示をにするかどうか
		/// </summary>
		[SerializeField]
		protected bool m_AlwaysDisplay = false ;
		public    bool   AlwaysDisplay
		{
			get{ return m_AlwaysDisplay ; }
			set{ m_AlwaysDisplay = value ; }
		}

		/// <summary>
		/// 位置の固定化と常時表示を有効にした場合にタッチ反応範囲を限定化するかどうか
		/// </summary>
		[SerializeField]
		protected bool m_InteractionRangeEnabled = true ;
		public    bool   InteractionRangeEnabled
		{
			get{ return m_InteractionRangeEnabled ; }
			set{ m_InteractionRangeEnabled = value ; }
		}

		/// <summary>
		/// 形状
		/// </summary>
		public enum ShapeTypes
		{
			Circle,
			Rectangle,
		}

		/// <summary>
		/// 反応の形状
		/// </summary>
		[SerializeField]
		protected ShapeTypes m_ShapeType = ShapeTypes.Circle ;
		public    ShapeTypes   ShapeType
		{
			get{ return m_ShapeType ; }
			set{ m_ShapeType = value ; }
		}

		// OnAvtivateの時間計測用
		protected float m_BaseTime ;

		//-----------------------------------------------------------

		// タッチポイントのＩＤ
		private int		m_JoystickPointerId = m_UnKnownCode ;
		private Vector2	m_BasePosition ;

		private Vector2 m_DeltaPosition ;
		public  Vector2   DeltaPosition
		{
			get
			{
				return m_DeltaPosition ;
			}
		} 

		private float m_Magnitude = 0 ;
		public  float   Magnitude
		{
			get
			{
				float kx = Input.GetAxis( "Horizontal" ) ;
				float ky = Input.GetAxis( "Vertical" ) ;

				if( Input.GetKey( KeyCode.LeftArrow ) == true )
				{
					kx = -1 ;
				}
				if( Input.GetKey( KeyCode.RightArrow ) == true )
				{
					kx =  1 ;
				}

				if( Input.GetKey( KeyCode.UpArrow ) == true )
				{
					ky =  1 ;
				}
				if( Input.GetKey( KeyCode.DownArrow ) == true )
				{
					ky = -1 ;
				}

				if( kx != 0 || ky != 0 )
				{
					return 1 ;
				}

				return m_Magnitude ;
			}
		}

		private Vector2 m_Direction ;
		public  Vector2   Direction
		{
			get
			{
				return m_Direction ;
			}
		}

		public Vector2 Delta
		{
			get
			{
				return m_Direction * m_Magnitude ;
			}
		}

		// デジタル系の値で取得する
		public Vector2 Axis
		{
			get
			{
				Vector2 axis = Vector2.zero ;

				float kx = Input.GetAxis( "Horizontal" ) ;
				float ky = Input.GetAxis( "Vertical" ) ;

				if( Input.GetKey( KeyCode.LeftArrow ) == true )
				{
					kx = -1 ;
				}
				if( Input.GetKey( KeyCode.RightArrow ) == true )
				{
					kx =  1 ;
				}

				if( Input.GetKey( KeyCode.UpArrow ) == true )
				{
					ky =  1 ;
				}
				if( Input.GetKey( KeyCode.DownArrow ) == true )
				{
					ky = -1 ;
				}

				if( kx != 0 || ky != 0 )
				{
					if( kx != 0 )
					{
						axis.x = kx ;
					}
					if( ky != 0 )
					{
						axis.y = ky ;
					}

					if( m_YAxisInversion == true )
					{
						axis.y = - axis.y ;
					}

					return axis ;
				}

				//---------------------------------------

				if( m_Magnitude == 0 || ( m_Direction.x == 0 && m_Direction.y == 0 ) )
				{
					return Vector2.zero ;
				}

				Vector2 direction = m_Direction.normalized ;

				float limit = 0.4226f ;	// 60 = 0.5f  45 = 0.7071f 

				if( direction.x <  ( - limit ) )
				{
					axis.x = -1 ;
				}
				else
				if( direction.x >  (   limit ) )
				{
					axis.x =  1 ;
				}

				if( direction.y <  ( - limit ) )
				{
					axis.y = -1 ;
				}
				else
				if( direction.y >  (   limit ) )
				{
					axis.y =  1 ;
				}

				return axis ;
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		/// </summary>
		/// <param name="option"></param>
		override protected void OnBuild( string option = "" )
		{
			SetAnchorToStretch() ;
//			Vector2 size = GetCanvasSize() ;
//			if( size.x >  0 && size.y >  0 )
//			{
//				this.Width = size.x * 0.5f ;
//			}
			SetPivot( 0.5f, 0.5f ) ;

			Image image = CImage ;
			image.color = new Color32( 255, 255, 255, 32 ) ;

			IsCanvasGroup = true ;
			IsInteraction = true ;

			SpriteSet spriteSet = SpriteSet.Create( "uGUIHelper/Textures/UISimpleJoystick" ) ;

			m_Frame = AddView<UIImage>( "Frame" ) ;
			m_Frame.SetAnchorToCenter() ;
			m_Frame.Sprite = spriteSet[ "UISimpleJoystick_Frame_Type_0" ] ;
			m_Frame.SetSize( 128, 128 ) ;

			float thumbWidth = m_Frame.Width * 0.6f ;

			m_Thumb = m_Frame.AddView<UIImage>( "Thumb" ) ;
			m_Thumb.SetAnchorToCenter() ;
			m_Thumb.Sprite = spriteSet[ "UISimpleJoystick_Thumb_Type_0" ] ;
			m_Thumb.SetSize( thumbWidth, thumbWidth ) ;
		}

		/// <summary>
		/// 派生クラスの Start
		/// </summary>
		override protected void OnStart()
		{
			base.OnStart() ;

			// イベントトリガーにトランジション用のコールバックを登録する
			if( Application.isPlaying == true )
			{
				m_Thumb.SetPosition( 0, 0 ) ;
				if( m_AlwaysDisplay == false )
				{
					m_Frame.SetActive( false ) ;
				}
			}
		}

		//---------------------------------------------

		// Down
		override protected void OnPointerDownBasic( PointerEventData pointer, bool fromScrollView )
		{
			base.OnPointerDownBasic( pointer, fromScrollView ) ;

			m_BasePosition = GetLocalPosition( pointer ) ;

			m_Thumb.SetPosition( 0, 0 ) ;

			if( m_AlwaysDisplay == false )
			{
				// 押した位置に表示する
				m_Frame.SetPosition( m_BasePosition ) ;
				m_Frame.SetActive( true ) ;
			}
			else
			{
				// 常に表示する

				if( m_InteractionRangeEnabled == true )
				{
					// 押した位置が有効範囲内でなければならない
					Vector2 frameCenter = m_Frame.PositionInCanvas ;

					if( m_ShapeType == ShapeTypes.Circle )
					{
						// 円
						if( ( m_BasePosition - frameCenter ).magnitude >  ( m_Frame.Width * 0.5f ) )
						{
							// 範囲外なので無効
							m_JoystickPointerId = m_UnKnownCode ;	// +3～-3 あたりが識別子に使われる(-1は使用してはダメ)
							return ;
						}
					}
					else
					{
						// 四角
						Vector2 tp = m_BasePosition - frameCenter ;
						float w = m_Frame.Width  * 0.5f ;
						float h = m_Frame.Height * 0.5f ;

						if( tp.x <  ( - w ) || tp.x >  ( + w ) || tp.y <  ( - h ) || tp.y >  ( + h ) )
						{
							// 範囲外なので無効
							m_JoystickPointerId = m_UnKnownCode ;	// +3～-3 あたりが識別子に使われる(-1は使用してはダメ)
							return ;
						}
					}

				}
			}


			m_JoystickPointerId = pointer.pointerId ;

			m_Magnitude = 0 ;
			m_Direction = Vector2.zero ;
			m_DeltaPosition = Vector2.zero ;
	
			// OnActivate用の時間計測開始
			m_BaseTime = Time.realtimeSinceStartup ;

			// コールバック呼び出し
			OnValueChangedAction?.Invoke( Identity, this, m_Magnitude, m_Direction ) ;
			OnActivateAction?.Invoke( Identity, this, true, m_BasePosition, 0 ) ;
		}

		// Up
		override protected void OnPointerUpBasic( PointerEventData pointer, bool fromScrollView )
		{
			base.OnPointerUpBasic( pointer, fromScrollView ) ;

			if( pointer.pointerId == m_JoystickPointerId )
			{
				Vector2 movePosition = GetLocalPosition( pointer ) ;

				if( m_AlwaysDisplay == false )
				{
					// 押した位置に表示する
					m_Frame.SetActive( false ) ;
				}
				else
				{
					// 常に表示する
					m_Thumb.SetPosition( 0, 0 ) ;
				}

				m_JoystickPointerId = m_UnKnownCode ;	// +3～-3 あたりが識別子に使われる(-1は使用してはダメ)

				m_Magnitude = 0 ;
				m_Direction = Vector2.zero ;
				m_DeltaPosition = Vector2.zero ;

				// コールバック呼び出し
				OnValueChangedAction?.Invoke( Identity, this, m_Magnitude, m_Direction ) ;
				OnActivateAction?.Invoke( Identity, this, false, movePosition, Time.realtimeSinceStartup - m_BaseTime ) ;
			}
		}

		// Move
		override protected void OnDragBasic( PointerEventData pointer, bool fromScrollView )
		{
			base.OnDragBasic( pointer, fromScrollView ) ;

			if( pointer.pointerId == m_JoystickPointerId )
			{
				Vector2 position = GetLocalPosition( pointer ) ;

				Vector2 deltaPosition = position - m_BasePosition ;

				if( m_ShapeType == ShapeTypes.Circle )
				{
					// 円
					float limit = ( m_Frame.Width - m_Thumb.Width ) * 0.5f ;

					// 方向
					Vector2 direction = deltaPosition.normalized ;

					if( m_HorizontalFunctionStop == true )
					{
						direction.x = 0 ;
					}

					if( m_VerticalFunctionStop == true )
					{
						direction.y = 0 ;
					}

					// 大きさ
					float magnitude = deltaPosition.magnitude ;
					if( magnitude >  limit )
					{
						magnitude  = limit ;
					}

					m_Thumb.SetPosition( direction * magnitude ) ;

					magnitude /= limit ;	// 正規化

					if( m_YAxisInversion == true )
					{
						direction.y = - direction.y ;
					}

					// 決定
					m_Direction = direction ;
					m_Magnitude = magnitude ;
				}
				else
				{
					// 四角

					float limitX = ( m_Frame.Width  - m_Thumb.Width  ) * 0.5f ;
					float limitY = ( m_Frame.Height - m_Thumb.Height ) * 0.5f ;

					// 方向
					Vector2 direction = deltaPosition ;

					if( direction.x <  ( - limitX ) )
					{
						direction.x  = ( - limitX ) ;
					}
					if( direction.x >  ( + limitX ) )
					{
						direction.x  = ( + limitX ) ;
					}
					if( direction.y <  ( - limitY ) )
					{
						direction.y  = ( - limitY ) ;
					}
					if( direction.y >  ( + limitY ) )
					{
						direction.y  = ( + limitY ) ;
					}

					direction.Normalize() ;

					if( m_HorizontalFunctionStop == true )
					{
						direction.x = 0 ;
					}

					if( m_VerticalFunctionStop == true )
					{
						direction.y = 0 ;
					}

					// 大きさ
					float magnitudeX = deltaPosition.x ;
					float magnitudeY = deltaPosition.y ;

					if( magnitudeX >  ( + limitX ) )
					{
						magnitudeX  = ( + limitX ) ;
					}
					if( magnitudeX <  ( - limitX ) )
					{
						magnitudeX  = ( - limitX ) ;
					}
					if( magnitudeY >  ( + limitY ) )
					{
						magnitudeY  = ( + limitY ) ;
					}
					if( magnitudeY <  ( - limitY ) )
					{
						magnitudeY  = ( - limitY ) ;
					}

					m_Thumb.SetPosition( magnitudeX, magnitudeY ) ;

					magnitudeX /= limitX ;
					magnitudeY /= limitY ;

					// 0～1.4
					float magnitude = Mathf.Sqrt( magnitudeX * magnitudeX + magnitudeY * magnitudeY ) ;

					if( m_YAxisInversion == true )
					{
						direction.y = - direction.y ;
					}

					// 決定
					m_Direction = direction ;
					m_Magnitude = magnitude ;
				}

				// コールバック呼び出し
				OnValueChangedAction?.Invoke( Identity, this, m_Magnitude, m_Direction ) ;
			}
		}
	}
}

