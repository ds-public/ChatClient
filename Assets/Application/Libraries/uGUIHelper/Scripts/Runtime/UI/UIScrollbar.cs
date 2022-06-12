using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.Events ;
using UnityEngine.EventSystems ;
using System ;
using System.Collections ;

namespace uGUIHelper
{
	/// <summary>
	/// uGUI:Scrollbar クラスの機能拡張コンポーネントクラス(複合)
	/// </summary>
	[ RequireComponent(typeof(ScrollbarWrapper))]
	public class UIScrollbar : UIImage
	{
		public float Offset
		{
			get
			{
				if( CScrollbar == null )
				{
					return 0 ;
				}
				return CScrollbar.value ;
			}
			set
			{
				if( CScrollbar == null )
				{
					return ;
				}
				CScrollbar.value = value ;
			}
		}

		public float Length
		{
			get
			{
				if( CScrollbar == null )
				{
					return 0 ;
				}
				return CScrollbar.size ;
			}
			set
			{
				if( CScrollbar == null )
				{
					return ;
				}
				CScrollbar.size = value ;
			}
		}

		public int Step
		{
			get
			{
				if( CScrollbar == null )
				{
					return 0 ;
				}
				return CScrollbar.numberOfSteps ;
			}
			set
			{
				if( CScrollbar == null )
				{
					return ;
				}
				CScrollbar.numberOfSteps = value ;
			}
		}

		[SerializeField][HideInInspector]
		private UIScrollView	m_ScrollViewElastic = null ;
		public  UIScrollView	  ScrollViewElastic
		{
			get
			{
				return m_ScrollViewElastic ;
			}
			set
			{
				m_ScrollViewElastic = value ;
			}
		}

		// バーの幅を常に一定にするかどうか
		[SerializeField][HideInInspector]
		private bool			m_FixedSize = false ;
		public  bool			  FixedSize
		{
			get
			{
				return m_FixedSize ;
			}
			set
			{
				m_FixedSize = value ;
			}
		}

		// 方向
		private enum DirectionTypes
		{
			Unknown    = 0,
			Horizontal = 1,
			Vertical   = 2,
		}
		
		//-----------------------------------------------------------

		// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		override protected void OnBuild( string option = "" )
		{
			Scrollbar scrollbar = CScrollbar ;

			if( scrollbar == null )
			{
				scrollbar = gameObject.AddComponent<Scrollbar>() ;
			}
			if( scrollbar == null )
			{
				// 異常
				return ;
			}

			//----------------------------------

			DirectionTypes directionType = DirectionTypes.Unknown ;
			if( option.ToLower() == "h" )
			{
				directionType = DirectionTypes.Horizontal ;
			}
			else
			if( option.ToLower() == "v" )
			{
				directionType = DirectionTypes.Vertical ;
			}

			Vector2 size = GetCanvasSize() ;
			if( size.x >  0 && size.y >  0 )
			{
				float s ;
				if( size.x <= size.y )
				{
					s = size.x ;
				}
				else
				{
					s = size.y ;
				}
				
				if( directionType == DirectionTypes.Horizontal )
				{
					SetSize( s * 0.4f, s * 0.075f ) ;
					scrollbar.direction = Scrollbar.Direction.LeftToRight ;
				}
				else
				if( directionType == DirectionTypes.Vertical )
				{
					SetSize( s * 0.075f, s * 0.4f ) ;
					scrollbar.direction = Scrollbar.Direction.BottomToTop ;
				}
			}
				
			ColorBlock colorBlock = scrollbar.colors ;
			colorBlock.fadeDuration = 0.2f ;
			scrollbar.colors = colorBlock ;
				
			Image image = CImage ;
			image.sprite = Resources.Load<Sprite>( "uGUIHelper/Textures/UIDefaultFrame" ) ;
			image.color = Color.white ;
			image.type = Image.Type.Sliced ;
				
			
//			UIImage tFrame = AddView<UIImage>( "Frame" ) ;
//			tFrame.SetAnchorToStretch() ;
//			tFrame.SetMargin(  0,  0,  0,  0 ) ;
//			tFrame.sprite = Resources.Load<Sprite>( "uGUIHelper/Textures/UIDefaultFrame" ) ;
//			tFrame.color = Color.white ;
//			tFrame.type = Image.Type.Sliced ;

				
			UIView slidingArea = AddView<UIView>( "Sliding Area" ) ;
			slidingArea.SetAnchorToStretch() ;
			slidingArea.SetMargin( 10, 10, 10, 10 ) ;
				
			UIImage handle = slidingArea.AddView<UIImage>( "Handle" ) ;
			if( directionType == DirectionTypes.Horizontal )
			{
				handle.SetAnchorToStretch() ;
			}
			else
			if( directionType == DirectionTypes.Vertical )
			{
				handle.SetAnchorToStretch() ;
			}
			handle.SetMargin( -10, -10, -10, -10 ) ;
				
			handle.Sprite = Resources.Load<Sprite>( "uGUIHelper/Textures/UIDefaultButton" ) ;
			handle.Type = Image.Type.Sliced ;
			handle.FillCenter = true ;
				
			scrollbar.targetGraphic = handle.CImage ;
			scrollbar.handleRect = handle.GetRectTransform() ;
			
			ResetRectTransform() ;
		}
		
		// 派生クラスの Start
		override protected void OnStart()
		{
			base.OnStart() ;
		
			// 注意:実行のみにしておかないと ExecuteInEditMode で何度も登録されてしまう
			if( Application.isPlaying == true )
			{
				// カスタムリスナー登録（Awake 起動毎に実行する必要がある）
				if( CScrollbar != null )
				{
					CScrollbar.onValueChanged.AddListener( OnValueChangedInner ) ;
				}
			}
		}

		//---------------------------------------------
	
		// Down
		override protected void OnPointerDownBasic( PointerEventData pointer, bool fromScrollView )
		{
			base.OnPointerDownBasic( pointer, fromScrollView ) ;

			if( m_ScrollViewElastic != null )
			{
				if( CScrollbar.direction == Scrollbar.Direction.LeftToRight || CScrollbar.direction == Scrollbar.Direction.RightToLeft )
				{
					// 横
					m_ScrollViewElastic.SetPositionFromScrollbar( UIScrollView.DirectionTypes.Horizontal, Offset ) ;
				}
				if( CScrollbar.direction == Scrollbar.Direction.TopToBottom || CScrollbar.direction == Scrollbar.Direction.BottomToTop )
				{
					// 縦
					m_ScrollViewElastic.SetPositionFromScrollbar( UIScrollView.DirectionTypes.Vertical,   Offset ) ;
				}
			}
		}

		override protected void OnDragBasic( PointerEventData pointer, bool fromScrollView )
		{
			base.OnPointerDownBasic( pointer, fromScrollView ) ;

			if( m_ScrollViewElastic != null )
			{
				if( CScrollbar.direction == Scrollbar.Direction.LeftToRight || CScrollbar.direction == Scrollbar.Direction.RightToLeft )
				{
					// 横
					m_ScrollViewElastic.SetPositionFromScrollbar( UIScrollView.DirectionTypes.Horizontal, Offset ) ;
				}
				if( CScrollbar.direction == Scrollbar.Direction.TopToBottom || CScrollbar.direction == Scrollbar.Direction.BottomToTop )
				{
					// 縦
					m_ScrollViewElastic.SetPositionFromScrollbar( UIScrollView.DirectionTypes.Vertical,   Offset ) ;
				}
			}
		}


		// Up
/*		override protected void OnPointerUpBasic( PointerEventData tPointer )
		{
			base.OnPointerUpBasic( tPointer ) ;

			// → Enter 状態であれば Highlight へ遷移
			// → Exit  状態であれば Normal へ遷移

//			ChangeScrollbarFadeState( false ) ;

			Debug.LogWarning( "離されたよ" ) ;
		}*/

		//---------------------------------------------
	
		/// <summary>
		/// 状態が変化した際に呼び出されるアクション
		/// </summary>
		public Action<string, UIScrollbar, float> OnValueChangedAction ;

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="tIdentity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="tView">ビューのインスタンス</param>
		/// <param name="tValue">変化後の値</param>
		public delegate void OnValueChanged( string identity, UIScrollbar view, float value ) ;

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲート
		/// </summary>
		public OnValueChanged OnValueChangedDelegate ;

		/// <summary>
		/// 状態が変化した際に呼び出されるアクションを設定する
		/// </summary>
		/// <param name="tOnValueChangedAction">アクションメソッド</param>
		public void SetOnValueChanged( Action<string, UIScrollbar, float> onValueChangedAction )
		{
			OnValueChangedAction = onValueChangedAction ;
		}
	
		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートを追加する
		/// </summary>
		/// <param name="tOnValueChangedDelegate">デリゲートメソッド</param>
		public void AddOnValueChanged( OnValueChanged onValueChangedDelegate )
		{
			OnValueChangedDelegate += onValueChangedDelegate ;
		}
		
		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="tOnValueChangedDelegate">デリゲートメソッド</param>
		public void RemoveOnValueChanged( OnValueChanged onValueChangedDelegate )
		{
			OnValueChangedDelegate -= onValueChangedDelegate ;
		}

		// 内部リスナー
		private void OnValueChangedInner( float value )
		{
			if( OnValueChangedAction != null || OnValueChangedDelegate != null )
			{
				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				OnValueChangedAction?.Invoke( identity, this, value ) ;
				OnValueChangedDelegate?.Invoke( identity, this, value ) ;
			}
		}
		
		//---------------------------------------------
		
		/// <summary>
		/// 状態が変化した際に呼び出されるリスナーを追加する
		/// </summary>
		/// <param name="tOnValueChanged">リスナーメソッド</param>
		public void AddOnValueChangedListener( UnityEngine.Events.UnityAction<float> onValueChanged )
		{
			Scrollbar scrollbar = CScrollbar ;
			if( scrollbar != null )
			{
				scrollbar.onValueChanged.AddListener( onValueChanged ) ;
			}
		}
	
		/// <summary>
		/// 状態が変化した際に呼び出されるリスナーを削除する
		/// </summary>
		/// <param name="tOnValueChanged">リスナーメソッド</param>
		public void RemoveOnValueChangedListener( UnityEngine.Events.UnityAction<float> onValueChanged )
		{
			Scrollbar scrollbar = CScrollbar ;
			if( scrollbar != null )
			{
				scrollbar.onValueChanged.RemoveListener( onValueChanged ) ;
			}
		}
	
		/// <summary>
		/// 状態が変化した際に呼び出されるリスナーを全て削除する
		/// </summary>
		public void RemoveOnValueChangedAllListeners()
		{
			Scrollbar scrollbar = CScrollbar ;
			if( scrollbar != null )
			{
				scrollbar.onValueChanged.RemoveAllListeners() ;
			}
		}
	}
}

