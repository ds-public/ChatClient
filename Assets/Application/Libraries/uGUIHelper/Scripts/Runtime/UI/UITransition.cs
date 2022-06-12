using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.Events ;
using UnityEngine.EventSystems ;
using System ;
using System.Collections ;
using System.Collections.Generic ;

#if UNITY_EDITOR
using UnityEditor ;
#endif

namespace uGUIHelper
{
	/// <summary>
	/// Transition コンポーネントクラス
	/// </summary>
	public class UITransition : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
	{
		/// <summary>
		/// ボタン状態
		/// </summary>
		public enum StateTypes
		{
			Normal		= 0,
			Highlighted	= 1,
			Pressed		= 2,
			Disabled	= 3,
			Clicked		= 4,
			Finished	= 5,
		}

		/// <summary>
		/// カーブの種別
		/// </summary>
		public enum ProcessTypes
		{
			Ease = 0,
			AnimationCurve = 1,
		}

		/// <summary>
		/// イーズの種別
		/// </summary>
		public enum EaseTypes
		{
			EaseInQuad,
			EaseOutQuad,
			EaseInOutQuad,
			EaseInCubic,
			EaseOutCubic,
			EaseInOutCubic,
			EaseInQuart,
			EaseOutQuart,
			EaseInOutQuart,
			EaseInQuint,
			EaseOutQuint,
			EaseInOutQuint,
			EaseInSine,
			EaseOutSine,
			EaseInOutSine,
			EaseInExpo,
			EaseOutExpo,
			EaseInOutExpo,
			EaseInCirc,
			EaseOutCirc,
			EaseInOutCirc,
			Linear,
			Spring,
			EaseInBounce,
			EaseOutBounce,
			EaseInOutBounce,
			EaseInBack,
			EaseOutBack,
			EaseInOutBack,
			EaseInElastic,
			EaseOutElastic,
			EaseInOutElastic,
	//		Punch
		}

		/// <summary>
		/// トランジョン情報クラス
		/// </summary>
		[System.Serializable]
		public class TransitionData
		{
			public Sprite	Sprite = null ;
			public ProcessTypes ProcessType = ProcessTypes.Ease ;

			public Vector3 FadeRotation = Vector3.zero ;
			public Vector3 FadeScale    = Vector3.one ;

			public EaseTypes FadeEaseType = EaseTypes.Linear ;
			public float FadeDuration = 0.2f ;

			public AnimationCurve FadeAnimationCurve = AnimationCurve.Linear(  0, 0, 0.5f, 1 ) ;

			public TransitionData( int state )
			{
				if( state == 0 )
				{
					FadeEaseType = EaseTypes.EaseOutBack ;
				}
				else
				if( state == 1 )
				{
					FadeScale = new Vector3( 1.05f, 1.05f, 1.05f ) ;
				}
				else
				if( state == 2 )
				{
					FadeScale = new Vector3( 0.95f, 0.95f, 0.95f ) ;
				}
				else
				if( state == 4 )
				{
					FadeScale = new Vector3( 1.25f, 1.25f, 1.0f ) ;
					FadeEaseType = EaseTypes.EaseOutBounce ;
					FadeDuration = 0.25f ;
				}
				else
				if( state == 5 )
				{
					FadeScale = new Vector3( 1.0f, 1.0f, 1.0f ) ;
					FadeEaseType = EaseTypes.Linear ;
					FadeDuration = 0.1f ;
				}
			}
		}

		// SerializeField 対象は readonly にしてはいけない
		[SerializeField][HideInInspector]
		protected List<TransitionData> m_Transitions = new List<TransitionData>()
		{
			new TransitionData( 0 ),	// Normal
			new TransitionData( 1 ),	// Hilighted
			new TransitionData( 2 ),	// Pressed
			new TransitionData( 3 ),	// Disabled
			new TransitionData( 4 ),	// Clicked
			new TransitionData( 5 ),	// Finished
			new TransitionData( 6 ),
			new TransitionData( 7 ),
		} ;

		public List<TransitionData> Transitions
		{
			get
			{
				return m_Transitions ;
			}
		}

		public void InitializeTransitions()
		{
			m_Transitions = new List<TransitionData>()
			{
				new TransitionData( 0 ),	// Normal
				new TransitionData( 1 ),	// Hilighted
				new TransitionData( 2 ),	// Pressed
				new TransitionData( 3 ),	// Disabled
				new TransitionData( 4 ),	// Clicked
				new TransitionData( 5 ),	// Finished
				new TransitionData( 6 ),
				new TransitionData( 7 ),
			} ;
		}


		/// <summary>
		/// スプライトをアトラス画像で動的に変更する
		/// </summary>
		[SerializeField]
		private bool m_SpriteOverwriteEnabled = false ;
		public  bool   SpriteOverwriteEnabled
		{
			get
			{
				return m_SpriteOverwriteEnabled ;
			}
			set
			{
				m_SpriteOverwriteEnabled = value ;
			}
		}

		// 選択中のタイプ
		[SerializeField][HideInInspector]
		private StateTypes m_EditingState = StateTypes.Pressed ;
		public  StateTypes   EditingState
		{
			get
			{
				return m_EditingState ;
			}
			set
			{
				m_EditingState = value ;
			}
		}

		[SerializeField][HideInInspector]
		private bool m_TransitionFoldOut = true ;

		/// <summary>
		/// トランジションのホールド時間
		/// </summary>
		public  bool  TransitionFoldOut
		{
			get
			{
				return m_TransitionFoldOut ;
			}
			set
			{
				m_TransitionFoldOut = value ;
			}
		}

		// トランジションを有効にするかどうか
		[SerializeField][HideInInspector]
		private bool m_TransitionEnabled = true ;

		/// <summary>
		/// トランジションの有効無効の設定
		/// </summary>
		public  bool  TransitionEnabled
		{
			get
			{
				return m_TransitionEnabled ;
			}
			set
			{
				m_TransitionEnabled = value ;
			}
		}

		//-------------------------------------------------------------------------------------------

		[SerializeField]
		protected bool	m_UseAnimator ;

		/// <summary>
		/// アニメーターを使用するかどうか
		/// </summary>
		public bool UseAnimator
		{
			get
			{
				return m_UseAnimator ;
			}
			set
			{
				m_UseAnimator = value ;
			}
		}

		//-------------------------------------------------------------------------------------------

		[SerializeField][HideInInspector]
		private bool m_PauseAfterFinished = true ;

		/// <summary>
		/// フィニッシュの後に動作をポーズするかどうか
		/// </summary>
		public bool PauseAfterFinished
		{
			get
			{
				return m_PauseAfterFinished ;
			}
			set
			{
				m_PauseAfterFinished = value ;
			}
		}

		/// <summary>
		/// 色の変化を有効にする
		/// </summary>
		[SerializeField][HideInInspector]
		private bool m_ColorTransmission = false ;
		public  bool   ColorTransmission
		{
			get
			{
				return m_ColorTransmission ;
			}
			set
			{
				m_ColorTransmission = value ;
			}
		}

		//-----------------------------------------------------------

		// ロック状態
		private bool m_IsPausing = false ;

		public bool IsPauseing
		{
			get
			{
				return m_IsPausing ;
			}
			set
			{
				m_IsPausing = value ;
			}
		}

		private bool m_IsHover = false ;

		/// <summary>
		/// ホバー状態
		/// </summary>
		public  bool  IsHover
		{
			get
			{
				return m_IsHover ;
			}
		}

		private bool m_IsPress = false ;

		/// <summary>
		/// プレス状態
		/// </summary>
		public  bool  IsPress
		{
			get
			{
				return m_IsPress ;
			}
		}

		private StateTypes m_State = StateTypes.Normal ;
		public  StateTypes   State
		{
			get
			{
				return m_State ;
			}
		}

		//-----------------------------------

		private StateTypes	m_EaseState ;

		private float		m_BaseTime = 0 ;

		private Vector3		m_BaseRotation ;
		private Vector3		m_BaseScale ;

		private Vector3		m_MoveRotation ;
		private Vector3		m_MoveScale ;

		private bool		m_Processing = false ;

		//--------------------------------

		private RectTransform m_RectTransform ;

		/// <summary>
		/// RectTransform の有無を返す
		/// </summary>
		public bool IsRectTransform
		{
			get
			{
				if( m_RectTransform == null )
				{
					m_RectTransform = GetComponent<RectTransform>() ;
				}
				if( m_RectTransform == null )
				{
					return false ;
				}
				return true ;
			}
		}

		private Button m_Button ;

		/// <summary>
		/// Button の有無を返す
		/// </summary>
		public bool IsButton
		{
			get
			{
				if( m_Button == null )
				{
					m_Button = GetComponent<Button>() ;
				}
				if( m_Button == null )
				{
					return false ;
				}
				return true ;
			}
		}

		// ボタン
		private UIButton m_UIButton ;

		//-----------------------------------------------------------------

		// 展開時に実行される
		internal void Awake()
		{
			if( m_Transitions == null )
			{
				InitializeTransitions() ;
			}
		}

		// 開始時に実行される
		internal void Start()
		{
			// イベントトリガーにトランジション用のコールバックを登録する
			if( Application.isPlaying == true )
			{
				m_State = StateTypes.Normal ;
				TransitionData data = m_Transitions[ ( int )m_State ] ;
				m_BaseRotation = data.FadeRotation ;
				m_BaseScale    = data.FadeScale ;
				m_MoveRotation = data.FadeRotation ;
				m_MoveScale    = data.FadeScale ;
				m_Processing = false ;

				if( m_Button == null )
				{
					m_Button = GetComponent<Button>() ;
				}

				if( m_UIButton == null )
				{
					m_UIButton = GetComponent<UIButton>() ;
				}
			}
		}

		// 毎フレーム実行される
		internal void Update()
		{
			if( Application.isPlaying == true )
			{
				if( m_TransitionEnabled == true && m_IsPausing == false )
				{
					if( m_UseAnimator == false )
					{
						// アニメーターを使用しない

						if( m_State != StateTypes.Clicked && m_State != StateTypes.Finished )
						{
							bool isDisable = false ;
							if( m_Button != null )
							{
								if( m_Button.IsInteractable() == false )
								{
									isDisable = true ;
								}
							}

							if( m_State != StateTypes.Disabled && isDisable == true )
							{
								// 無効状態
								ChangeTransitionStateForStandard( StateTypes.Disabled, StateTypes.Disabled ) ;
							}
							else
							if( m_State == StateTypes.Disabled && isDisable == false )
							{
								// 無効状態
								ChangeTransitionStateForStandard( StateTypes.Normal, StateTypes.Disabled ) ;
							}

							if( m_State == StateTypes.Highlighted && m_IsHover == false )
							{
								// 無効状態
								ChangeTransitionStateForStandard( StateTypes.Normal, StateTypes.Highlighted ) ;
							}
						}

						//--------------------------------------------------------

						// 実行する
						if( m_Processing == true )
						{
							ProcessTransition() ;
						}
					}
					else
					{
						// アニメーターを使用する
						if( m_State != StateTypes.Clicked && m_State != StateTypes.Finished )
						{
							bool isDisable = false ;
							if( m_Button != null )
							{
								if( m_Button.IsInteractable() == false )
								{
									isDisable = true ;
								}
							}

							if( m_State != StateTypes.Disabled && isDisable == true )
							{
								// 無効状態
								ChangeTransitionStateForAnimator( StateTypes.Disabled, StateTypes.Disabled ) ;
							}
							else
							if( m_State == StateTypes.Disabled && isDisable == false )
							{
								// 無効状態
								ChangeTransitionStateForAnimator( StateTypes.Normal, StateTypes.Disabled ) ;
							}

							if( m_State == StateTypes.Highlighted && m_IsHover == false )
							{
								// 無効状態
								ChangeTransitionStateForAnimator( StateTypes.Normal, StateTypes.Highlighted ) ;
							}
						}
					}
				}
			}
		}

		//---------------------------------------------
	
		// Enter
		public void OnPointerEnter( PointerEventData pointer )
		{
			// → Release 状態であれば Highlight へ遷移
			if( m_State != StateTypes.Clicked && m_State != StateTypes.Finished )
			{
				if( m_IsPress == false && m_Processing == false )
				{
					if( m_UseAnimator == false )
					{
						// スタンダード
						ChangeTransitionStateForStandard( StateTypes.Highlighted, StateTypes.Highlighted ) ;
					}
					else
					{
						// アニメーター
						ChangeTransitionStateForAnimator( StateTypes.Highlighted, StateTypes.Highlighted ) ;
					}
				}
				m_IsHover = true ;
			}
		}

		// Exit
		public void OnPointerExit( PointerEventData pointer )
		{
			// → Release 状態であれば Normal へ遷移
			if( m_State != StateTypes.Clicked && m_State != StateTypes.Finished )
			{
				if( m_IsPress == false && m_Processing == false )
				{
					if( m_UseAnimator == false )
					{
						// スタンダード
						ChangeTransitionStateForStandard( StateTypes.Normal, StateTypes.Highlighted ) ;
					}
					else
					{
						// アニメーター
						ChangeTransitionStateForAnimator( StateTypes.Normal, StateTypes.Highlighted ) ;
					}
				}
				m_IsHover = false ;
			}
		}

		// Down
		public void OnPointerDown( PointerEventData pointer )
		{
			// → Press 状態へ遷移
			if( m_State != StateTypes.Clicked && m_State != StateTypes.Finished )
			{
				if( m_UseAnimator == false )
				{
					// スタンダード
					ChangeTransitionStateForStandard( StateTypes.Pressed, StateTypes.Pressed ) ;
				}
				else
				{
					// アニメーター
					ChangeTransitionStateForAnimator( StateTypes.Pressed, StateTypes.Pressed ) ;
				}
				m_IsPress = true ;
			}
		}

		// Up
		public void OnPointerUp( PointerEventData pointer )
		{
			// → Enter 状態であれば Highlight へ遷移
			// → Exit  状態であれば Normal へ遷移

			if( m_State != StateTypes.Clicked && m_State != StateTypes.Finished )
			{
				if( m_IsHover == true )
				{
					if( m_UseAnimator == false )
					{
						// スタンダード
						ChangeTransitionStateForStandard( StateTypes.Highlighted, StateTypes.Normal ) ;
					}
					else
					{
						// アニメーター
						ChangeTransitionStateForAnimator( StateTypes.Highlighted, StateTypes.Normal ) ;
					}
				}
				else
				{
					if( m_UseAnimator == false )
					{
						// スタンダード
						ChangeTransitionStateForStandard( StateTypes.Normal, StateTypes.Normal ) ;
					}
					else
					{
						// アニメーター
						ChangeTransitionStateForAnimator( StateTypes.Normal, StateTypes.Normal ) ;
					}
				}
				m_IsPress = false ;
			}
		}

		// Clicked(UIButton から呼び出される)
		internal protected void OnClicked( bool waitForTransition )
		{
			if( m_State != StateTypes.Clicked && m_State != StateTypes.Finished )
			{
				m_WaitForTransition = waitForTransition ;
				if( m_WaitForTransition == true )
				{
					UIEventSystem.Disable() ;
				}

				// 押されたボタンを同階層で最も手前に表示する
				if( transform.parent != null )
				{
					UIView parent = transform.parent.GetComponent<UIView>() ;
					if( parent != null )
					{
						Vector3 p =	transform.localPosition ;
						p.z = 10 ;
						transform.localPosition = p ;
						parent.SortChildByZ() ;
					}
				}

				if( m_UseAnimator == false )
				{
					// スタンダード
					ChangeTransitionStateForStandard( StateTypes.Clicked, StateTypes.Clicked ) ;
				}
				else
				{
					// アニメーター
					ChangeTransitionStateForAnimator( StateTypes.Clicked, StateTypes.Clicked ) ;
				}
				m_IsPress = false ;
			}
		}

		//-------------------------------------------------------------------

		private bool m_WaitForTransition = false ;

		// トランジジョンの状態を変える
		private bool ChangeTransitionStateForStandard( StateTypes state, StateTypes easeState )
		{
			if( m_State == state )
			{
				// 状態が同じなので処理しない
				return true ;
			}

			if( m_Button != null && m_UIButton != null )
			{
				if( m_ColorTransmission == true )
				{
					ColorBlock cb = m_Button.colors ;
				
					if( state == StateTypes.Normal )
					{
						m_UIButton.ApplyColorToChidren( cb.normalColor ) ;
					}
					else
					if( state == StateTypes.Highlighted )
					{
						m_UIButton.ApplyColorToChidren( cb.highlightedColor ) ;
					}
					else
					if( state == StateTypes.Pressed )
					{
						m_UIButton.ApplyColorToChidren( cb.pressedColor ) ;
					}
					else
					if( state == StateTypes.Disabled )
					{
						m_UIButton.ApplyColorToChidren( cb.disabledColor ) ;
					}
					else
					{
						m_UIButton.ApplyColorToChidren( cb.normalColor ) ;
					}
				}
			}

			// 現在の RectTransform の状態を退避する

			if( m_RectTransform == null )
			{
				m_RectTransform = GetComponent<RectTransform>() ;
			}
			if( m_RectTransform == null )
			{
				// RectTransform がアタッチされていない
				return false ;
			}

			//----------------------------------

			// 現在変化中の状態を変化前の状態とする
			m_BaseRotation	= m_MoveRotation ;
			m_BaseScale		= m_MoveScale ;

			m_State			= state ;
			m_EaseState		= easeState ;
			m_BaseTime		= Time.realtimeSinceStartup ;

			m_Processing	= true ;	// 処理する

//			if( m_SpriteOverwriteEnabled == true )
//			{
//				UIImage image = GetComponent<UIImage>() ;
//				if( image != null && image.SpriteSet != null )
//				{
//					// 画像を変更する
//					
//					TransitionData data = m_Transitions[ ( int )m_State ] ;
//	
//					if( data.Sprite != null )
//					{
//						image.SetSpriteInAtlas( data.Sprite.name ) ;
//					}
//				}
//			}

			return true ;
		}
		
		// トランジションの状態を反映させる
		private bool ProcessTransition()
		{
			if( m_RectTransform == null )
			{
				m_RectTransform = GetComponent<RectTransform>() ;
			}
			if( m_RectTransform == null )
			{
				// RectTransform がアタッチされていない
				return false ;
			}

			float time = Time.realtimeSinceStartup - m_BaseTime ;

			TransitionData data = m_Transitions[ ( int )m_State ] ;
			TransitionData easeData = m_Transitions[ ( int )m_EaseState ] ;
			if( data.ProcessType == ProcessTypes.Ease )
			{
				if( data.FadeDuration >  0 )
				{
					float factor = time / data.FadeDuration ;
					if( factor >  1 )
					{
						factor  = 1 ;
						m_Processing = false ;
					}
				
					m_MoveRotation = GetValue( m_BaseRotation, data.FadeRotation, factor, easeData.FadeEaseType ) ;
					m_MoveScale    = GetValue( m_BaseScale,    data.FadeScale,    factor, easeData.FadeEaseType ) ;

					m_RectTransform.localEulerAngles = m_MoveRotation ;
					m_RectTransform.localScale       = m_MoveScale ;
				}
				else
				{
					// 0 秒
					m_RectTransform.localEulerAngles = data.FadeRotation ;
					m_RectTransform.localScale       = data.FadeScale ;

					m_Processing = false ;
				}
			}
			else
			if( data.ProcessType == ProcessTypes.AnimationCurve )
			{
				int l = data.FadeAnimationCurve.length ;
				Keyframe keyFrame = data.FadeAnimationCurve[ l - 1 ] ;	// 最終キー
				float fadeDuration = keyFrame.time ;
			
				if( fadeDuration >  0 )
				{
					if( time >  fadeDuration )
					{
						time  = fadeDuration ;
						m_Processing = false ;
					}

					float value = easeData.FadeAnimationCurve.Evaluate( time ) ;
					m_MoveRotation = Vector3.Lerp( m_BaseRotation, data.FadeRotation, value ) ;
					m_MoveScale    = Vector3.Lerp( m_BaseScale,    data.FadeScale,    value ) ;

					m_RectTransform.localEulerAngles = m_MoveRotation ;
					m_RectTransform.localScale       = m_MoveScale ;
				}
				else
				{
					// 0 秒
					m_RectTransform.localEulerAngles = data.FadeRotation ;
					m_RectTransform.localScale       = data.FadeScale ;

					m_Processing = false ;
				}
			}

			if( m_Processing == false )
			{
				// 終了
				if( m_State == StateTypes.Clicked )
				{
					// 元の見た目に戻す
					ChangeTransitionStateForStandard( StateTypes.Finished, StateTypes.Finished ) ;
				}
				else
				if( m_State == StateTypes.Finished )
				{
					if( m_WaitForTransition == true )
					{
//						Debug.LogWarning( "------- クリック後のトランジションが終了した" ) ;
						UIEventSystem.Enable() ;
	
						if( m_UIButton != null )
						{
							m_UIButton.OnButtonClickFromTransition() ;
						}
						m_WaitForTransition = false ;
					}

					if( m_PauseAfterFinished == true )
					{
						m_IsPausing = true ;	// 動作をロックする
					}
				}
			}

			return true ;
		}

#if false
		/// <summary>
		/// 指定の状態で表示する画像を設定する
		/// </summary>
		/// <param name="state"></param>
		/// <param name="imageName"></param>
		/// <returns></returns>
		public bool ReplaceImage( StateTypes state, string imageName )
		{
			UIImage image = GetComponent<UIImage>() ;
			if( image == null || image.SpriteSet == null )
			{
				return false ;
			}

			Sprite sprite = image.GetSpriteInAtlas( imageName ) ;
			if( sprite == null )
			{
				return false ;
			}

			int i = ( int )state ;

			if( m_Transitions[ i ] != null )
			{
				m_Transitions[ i ].Sprite = sprite ;
			}

			if( m_State == state )
			{
				image.SetSpriteInAtlas( imageName ) ;
			}

			return true ;
		}

		/// <summary>
		/// 全ての状態で表示する画像を設定する
		/// </summary>
		/// <param name="imageName"></param>
		public bool ReplaceImage( string imageName )
		{
			UIImage image = GetComponent<UIImage>() ;
			if( image == null || image.SpriteSet == null )
			{
				return false ;
			}

			Sprite sprite = image.GetSpriteInAtlas( imageName ) ;
			if( sprite == null )
			{
				return false ;
			}

			int i, l = m_Transitions.Count ;
				
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_Transitions[ i ] != null )
				{
					m_Transitions[ i ].Sprite = sprite ;
				}
			}

			image.SetSpriteInAtlas( imageName ) ;

			return true ;
		}
#endif
		//-------------------------------------------------------------------------------------------
		// アニメーター用

		// トランジジョンの状態を変える
		private bool ChangeTransitionStateForAnimator( StateTypes state, StateTypes easeState )
		{
			if( m_State == state )
			{
				// 状態が同じなので処理しない
				return true ;
			}

			if( m_Button != null && m_UIButton != null )
			{
				if( m_ColorTransmission == true )
				{
					ColorBlock cb = m_Button.colors ;
				
					if( state == StateTypes.Normal )
					{
						m_UIButton.ApplyColorToChidren( cb.normalColor ) ;
					}
					else
					if( state == StateTypes.Highlighted )
					{
						m_UIButton.ApplyColorToChidren( cb.highlightedColor ) ;
					}
					else
					if( state == StateTypes.Pressed )
					{
						m_UIButton.ApplyColorToChidren( cb.pressedColor ) ;
					}
					else
					if( state == StateTypes.Disabled )
					{
						m_UIButton.ApplyColorToChidren( cb.disabledColor ) ;
					}
					else
					{
						m_UIButton.ApplyColorToChidren( cb.normalColor ) ;
					}
				}
			}

			// 現在の RectTransform の状態を退避する

			if( m_RectTransform == null )
			{
				m_RectTransform = GetComponent<RectTransform>() ;
			}
			if( m_RectTransform == null )
			{
				// RectTransform がアタッチされていない
				return false ;
			}

			//----------------------------------

			// 現在変化中の状態を変化前の状態とする
			m_BaseRotation	= m_MoveRotation ;
			m_BaseScale		= m_MoveScale ;

			m_State			= state ;
			m_EaseState		= easeState ;
			m_BaseTime		= Time.realtimeSinceStartup ;

			m_Processing	= true ;	// 処理する

//			if( m_SpriteOverwriteEnabled == true )
//			{
//				UIImage image = GetComponent<UIImage>() ;
//				if( image != null && image.SpriteSet != null )
//				{
//					// 画像を変更する
//					
//					TransitionData data = m_Transitions[ ( int )m_State ] ;
//	
//					if( data.Sprite != null )
//					{
//						image.SetSpriteInAtlas( data.Sprite.name ) ;
//					}
//				}
//			}

			//----------------------------------------------------------

			if( m_UIButton != null )
			{
				if( m_State == StateTypes.Normal )
				{
					m_UIButton.PlayAnimator( "Normal" ) ;
				}
				else
				if( m_State == StateTypes.Highlighted )
				{
					m_UIButton.PlayAnimator( "Highlighted" ) ;
				}
				else
				if( m_State == StateTypes.Pressed )
				{
					m_UIButton.PlayAnimator( "Pressed" ) ;
				}
				else
				if( m_State == StateTypes.Disabled )
				{
					m_UIButton.PlayAnimator( "Disabled" ) ;
				}
				else
				if( m_State == StateTypes.Clicked )
				{
					m_UIButton.PlayAnimator( "Selected" ) ;
				}
			}

			//----------------------------------------------------------

			return true ;
		}



		//-------------------------------------------------------------------------------------------
//#if false
		// ボタン状態復帰用のフラグ
		private bool m_IsLostFocusForButton = false ;

		internal void OnApplicationFocus( bool isFocus )
		{
			if( isFocus == false )
			{
				m_IsLostFocusForButton = true ;
			}
			else
			if( isFocus == true && m_IsLostFocusForButton == true )
			{
				m_IsLostFocusForButton = false ;

				//---------------------------------

				// レジューム時にボタンの状態を復旧させる
				if( m_Button != null && m_UIButton != null )
				{
					// 色まわり
					if( m_ColorTransmission == true )
					{
						ColorBlock cb = m_Button.colors ;

						if( m_Button.interactable == true )
						{
							m_UIButton.ApplyColorToChidren( cb.normalColor ) ;
						}
						else
						{
							m_UIButton.ApplyColorToChidren( cb.disabledColor ) ;
						}
					}

					// 回転と拡縮
					bool isProcess = false ;
					if( m_Button.interactable == true  && m_State != StateTypes.Normal )
					{
						// 有効
						m_State = StateTypes.Normal ;
						isProcess = true ;
					}
					else
					if( m_Button.interactable == false && m_State != StateTypes.Disabled )
					{
						// 無効
						m_State = StateTypes.Disabled ;
						isProcess = true ;
					}

					if( isProcess == true )
					{
						TransitionData data = m_Transitions[ ( int )m_State ] ;
						m_RectTransform.localEulerAngles = data.FadeRotation ;
						m_RectTransform.localScale       = data.FadeScale ;

						m_Processing = false ;
					}
				}
			}
		}
//#endif
		//-------------------------------------------------------------------------------------------------------------------
	
		// Vector3 の変化中の値を取得
		private Vector3 GetValue( Vector3 start, Vector3 end, float factor, EaseTypes easeType  )
		{
			float x = GetValue( start.x, end.x, factor, easeType ) ;
			float y = GetValue( start.y, end.y, factor, easeType ) ;
			float z = GetValue( start.z, end.z, factor, easeType ) ;

			return new Vector3( x, y, z ) ;
		}

		// float の変化中の値を取得
		public float GetValue( float start, float end, float factor, EaseTypes easeType )
		{
			float value = 0 ;
			switch( easeType )
			{
				case EaseTypes.EaseInQuad		: value = GetEaseInQuad(		start, end, factor ) ; break ;
				case EaseTypes.EaseOutQuad		: value = GetEaseOutQuad(		start, end, factor ) ; break ;
				case EaseTypes.EaseInOutQuad	: value = GetEaseInOutQuad(		start, end, factor ) ; break ;
				case EaseTypes.EaseInCubic		: value = GetEaseInCubic(		start, end, factor ) ; break ;
				case EaseTypes.EaseOutCubic		: value = GetEaseOutCubic(		start, end, factor ) ; break ;
				case EaseTypes.EaseInOutCubic	: value = GetEaseInOutCubic(	start, end, factor ) ; break ;
				case EaseTypes.EaseInQuart		: value = GetEaseInQuart(		start, end, factor ) ; break ;
				case EaseTypes.EaseOutQuart		: value = GetEaseOutQuart(		start, end, factor ) ; break ;
				case EaseTypes.EaseInOutQuart	: value = GetEaseInOutQuart(	start, end, factor ) ; break ;
				case EaseTypes.EaseInQuint		: value = GetEaseInQuint(		start, end, factor ) ; break ;
				case EaseTypes.EaseOutQuint		: value = GetEaseOutQuint(		start, end, factor ) ; break ;
				case EaseTypes.EaseInOutQuint	: value = GetEaseInOutQuint(	start, end, factor ) ; break ;
				case EaseTypes.EaseInSine		: value = GetEaseInSine(		start, end, factor ) ; break ;
				case EaseTypes.EaseOutSine		: value = GetEaseOutSine(		start, end, factor ) ; break ;
				case EaseTypes.EaseInOutSine	: value = GetEaseInOutSine(		start, end, factor ) ; break ;
				case EaseTypes.EaseInExpo		: value = GetEaseInExpo(		start, end, factor ) ; break ;
				case EaseTypes.EaseOutExpo		: value = GetEaseOutExpo(		start, end, factor ) ; break ;
				case EaseTypes.EaseInOutExpo	: value = GetEaseInOutExpo(		start, end, factor ) ; break ;
				case EaseTypes.EaseInCirc		: value = GetEaseInCirc(		start, end, factor ) ; break ;
				case EaseTypes.EaseOutCirc		: value = GetEaseOutCirc(		start, end, factor ) ; break ;
				case EaseTypes.EaseInOutCirc	: value = GetEaseInOutCirc(		start, end, factor ) ; break ;
				case EaseTypes.Linear			: value = GetLinear(			start, end, factor ) ; break ;
				case EaseTypes.Spring			: value = GetSpring(			start, end, factor ) ; break ;
				case EaseTypes.EaseInBounce		: value = GetEaseInBounce(		start, end, factor ) ; break ;
				case EaseTypes.EaseOutBounce	: value = GetEaseOutBounce(		start, end, factor ) ; break ;
				case EaseTypes.EaseInOutBounce	: value = GetEaseInOutBounce(	start, end, factor ) ; break ;
				case EaseTypes.EaseInBack		: value = GetEaseInBack(		start, end, factor ) ; break ;
				case EaseTypes.EaseOutBack		: value = GetEaseOutBack(		start, end, factor ) ; break ;
				case EaseTypes.EaseInOutBack	: value = GetEaseInOutBack(		start, end, factor ) ; break ;
				case EaseTypes.EaseInElastic	: value = GetEaseInElastic(		start, end, factor ) ; break ;
				case EaseTypes.EaseOutElastic	: value = GetEaseOutElastic(	start, end, factor ) ; break ;
				case EaseTypes.EaseInOutElastic	: value = GetEaseInOutElastic(	start, end, factor ) ; break ;
			}
			return value ;
		}

		//------------------------

		private float GetEaseInQuad( float start, float end, float value )
		{
			end -= start ;
			return end * value * value + start ;
		}

		private float GetEaseOutQuad( float start, float end, float value )
		{
			end -= start ;
			return - end * value * ( value - 2 ) + start ;
		}

		private float GetEaseInOutQuad( float start, float end, float value )
		{
			value /= 0.5f ;
			end -= start ;
			if( value <  1 ) return end * 0.5f * value * value + start ;
			value -- ;
			return - end * 0.5f * ( value * ( value - 2 ) - 1 ) + start ;
		}

		private float GetEaseInCubic( float start, float end, float value )
		{
			end -= start ;
			return end * value * value * value + start ;
		}

		private float GetEaseOutCubic( float start, float end, float value )
		{
			value -- ;
			end -= start ;
			return end * ( value * value * value + 1 ) + start ;
		}

		private float GetEaseInOutCubic( float start, float end, float value )
		{
			value /= 0.5f ;
			end -= start ;
			if( value <  1 ) return end * 0.5f * value * value * value + start ;
			value -= 2 ;
			return end * 0.5f * ( value * value * value + 2 ) + start ;
		}

		private float GetEaseInQuart( float start, float end, float value )
		{
			end -= start ;
			return end * value * value * value * value + start ;
		}

		private float GetEaseOutQuart( float start, float end, float value )
		{
			value -- ;
			end -= start ;
			return - end * ( value * value * value * value - 1 ) + start ;
		}

		private float GetEaseInOutQuart( float start, float end, float value )
		{
			value /= 0.5f ;
			end -= start ;
			if( value <  1 ) return end * 0.5f * value * value * value * value + start ;
			value -= 2 ;
			return - end * 0.5f * ( value * value * value * value - 2 ) + start ;
		}

		private float GetEaseInQuint( float start, float end, float value )
		{
			end -= start ;
			return end * value * value * value * value * value + start ;
		}
	
		private float GetEaseOutQuint( float start, float end, float value )
		{
			value -- ;
			end -= start ;
			return end * ( value * value * value * value * value + 1 ) + start ;
		}

		private float GetEaseInOutQuint( float start, float end, float value )
		{
			value /= 0.5f ;
			end -= start ;
			if( value <  1 ) return end * 0.5f * value * value * value * value * value + start ;
			value -= 2 ;
			return end * 0.5f * ( value * value * value * value * value + 2 ) + start ;
		}

		private float GetEaseInSine( float start, float end, float value )
		{
			end -= start ;
			return - end * Mathf.Cos( value * ( Mathf.PI * 0.5f ) ) + end + start ;
		}

		private float GetEaseOutSine( float start, float end, float value )
		{
			end -= start ;
			return end * Mathf.Sin( value * ( Mathf.PI * 0.5f ) ) + start ;
		}

		private float GetEaseInOutSine( float start, float end, float value )
		{
			end -= start ;
			return - end * 0.5f * ( Mathf.Cos( Mathf.PI * value ) - 1 ) + start ;
		}

		private float GetEaseInExpo( float start, float end, float value )
		{
			end -= start ;
			return end * Mathf.Pow( 2, 10 * ( value - 1 ) ) + start ;
		}

		private float GetEaseOutExpo( float start, float end, float value )
		{
			end -= start ;
			return end * ( - Mathf.Pow( 2, - 10 * value ) + 1 ) + start ;
		}

		private float GetEaseInOutExpo( float start, float end, float value )
		{
			value /= 0.5f;
			end -= start ;
			if( value <  1 ) return end * 0.5f * Mathf.Pow( 2, 10 * ( value - 1 ) ) + start ;
			value -- ;
			return end * 0.5f * ( - Mathf.Pow( 2, - 10 * value ) + 2 ) + start ;
		}

		private float GetEaseInCirc( float start, float end, float value )
		{
			end -= start ;
			return - end * ( Mathf.Sqrt( 1 - value * value ) - 1 ) + start ;
		}

		private float GetEaseOutCirc( float start, float end, float value )
		{
			value -- ;
			end -= start ;
			return end * Mathf.Sqrt( 1 - value * value ) + start ;
		}

		private float GetEaseInOutCirc( float start, float end, float value )
		{
			value /= 0.5f ;
			end -= start ;
			if( value <  1 ) return - end * 0.5f * ( Mathf.Sqrt( 1 - value * value ) - 1 ) + start ;
			value -= 2 ;
			return end * 0.5f * ( Mathf.Sqrt( 1 - value * value ) + 1 ) + start ;
		}

		private float GetLinear( float start, float end, float value )
		{
			return Mathf.Lerp( start, end, value ) ;
		}
	
		private float GetSpring( float start, float end, float value )
		{
			value = Mathf.Clamp01( value ) ;
			value = ( Mathf.Sin( value * Mathf.PI * ( 0.2f + 2.5f * value * value * value ) ) * Mathf.Pow( 1f - value, 2.2f ) + value ) * ( 1f + ( 1.2f * ( 1f - value ) ) ) ;
			return start + ( end - start ) * value ;
		}

		private float GetEaseInBounce( float start, float end, float value )
		{
			end -= start ;
			float d = 1f ;
			return end - GetEaseOutBounce( 0, end, d - value ) + start ;
		}
	
		private float GetEaseOutBounce( float start, float end, float value )
		{
			value /= 1f ;
			end -= start ;
			if( value <  ( 1 / 2.75f ) )
			{
				return end * ( 7.5625f * value * value ) + start ;
			}
			else
			if( value <  ( 2 / 2.75f ) )
			{
				value -= ( 1.5f / 2.75f ) ;
				return end * ( 7.5625f * ( value ) * value + .75f ) + start ;
			}
			else
			if( value <  (  2.5  / 2.75 ) )
			{
				value -= ( 2.25f / 2.75f ) ;
				return end * ( 7.5625f * ( value ) * value + .9375f ) + start ;
			}
			else
			{
				value -= ( 2.625f / 2.75f ) ;
				return end * ( 7.5625f * ( value ) * value + .984375f ) + start ;
			}
		}

		private float GetEaseInOutBounce( float start, float end, float value )
		{
			end -= start ;
			float d = 1f ;
			if( value <  d * 0.5f ) return GetEaseInBounce( 0, end, value * 2 ) * 0.5f + start ;
			else return GetEaseOutBounce( 0, end, value * 2 - d ) * 0.5f + end * 0.5f + start ;
		}

		private float GetEaseInBack( float start, float end, float value )
		{
			end -= start ;
			value /= 1 ;
			float s = 1.70158f ;
			return end * ( value ) * value * ( ( s + 1 ) * value - s ) + start ;
		}

		private float GetEaseOutBack( float start, float end, float value )
		{
			float s = 1.70158f ;
			end -= start ;
			value = ( value ) - 1 ;
			return end * ( ( value ) * value * ( ( s + 1 ) * value + s ) + 1 ) + start ;
		}

		private float GetEaseInOutBack( float start, float end, float value )
		{
			float s = 1.70158f ;
			end -= start ;
			value /= 0.5f ;
			if( ( value ) <  1 )
			{
				s *= ( 1.525f ) ;
				return end * 0.5f * ( value * value * ( ( ( s ) + 1 ) * value - s ) ) + start ;
			}
			value -= 2 ;
			s *= ( 1.525f ) ;
			return end * 0.5f * ( ( value ) * value * ( ( ( s ) + 1 ) * value + s ) + 2 ) + start ;
		}

		private float GetEaseInElastic( float start, float end, float value )
		{
			end -= start ;
		
			float d = 1f ;
			float p = d * 0.3f ;
			float s ;
			float a = 0 ;
		
			if( value == 0 ) return start ;
		
			if( ( value /= d ) == 1 ) return start + end ;
		
			if( a == 0f || a <  Mathf.Abs( end ) )
			{
				a = end ;
				s = p / 4 ;
			}
			else
			{
				s = p / ( 2 * Mathf.PI ) * Mathf.Asin( end / a ) ;
			}
		
			return - ( a * Mathf.Pow( 2, 10 * ( value -= 1 ) ) * Mathf.Sin( ( value * d - s ) * ( 2 * Mathf.PI ) / p ) ) + start ;
		}		

		private float GetEaseOutElastic( float start, float end, float value )
		{
			end -= start ;
		
			float d = 1f ;
			float p = d * 0.3f ;
			float s ;
			float a = 0 ;
		
			if( value == 0 ) return start ;
		
			if( ( value /= d ) == 1 ) return start + end ;
		
			if( a == 0f || a <  Mathf.Abs( end ) )
			{
				a = end ;
				s = p * 0.25f ;
			}
			else
			{
				s = p / ( 2 * Mathf.PI ) * Mathf.Asin( end / a ) ;
			}
		
			return ( a * Mathf.Pow( 2, - 10 * value ) * Mathf.Sin( ( value * d - s ) * ( 2 * Mathf.PI ) / p ) + end + start ) ;
		}		

		private float GetEaseInOutElastic( float start, float end, float value )
		{
			end -= start ;
		
			float d = 1f ;
			float p = d * 0.3f ;
			float s ;
			float a = 0 ;
		
			if( value == 0 ) return start ;
		
			if( ( value /= d * 0.5f ) == 2 ) return start + end ;
		
			if( a == 0f || a <  Mathf.Abs( end ) )
			{
				a = end ;
				s = p / 4 ;
			}
			else
			{
				s = p / ( 2 * Mathf.PI ) * Mathf.Asin( end / a ) ;
			}
		
			if( value <  1 ) return - 0.5f * ( a * Mathf.Pow( 2, 10 * ( value -= 1 ) ) * Mathf.Sin( ( value * d - s ) * ( 2 * Mathf.PI ) / p ) ) + start ;
			return a * Mathf.Pow( 2, - 10 * ( value -= 1 ) ) * Mathf.Sin( ( value * d - s ) * ( 2 * Mathf.PI ) / p ) * 0.5f + end + start ;
		}
#if false
		private float GetPunch( float amplitude, float value )
		{
			float s ;
			if( value == 0 )
			{
				return 0 ;
			}
			else
			if( value == 1 )
			{
				return 0 ;
			}
			float period = 1 * 0.3f ;
			s = period / ( 2 * Mathf.PI ) * Mathf.Asin( 0 ) ;
			return ( amplitude * Mathf.Pow( 2, - 10 * value ) * Mathf.Sin( ( value * 1 - s ) * ( 2 * Mathf.PI ) / period ) ) ;
		}
		
		private float GetClerp( float start, float end, float value )
		{
			float min = 0.0f ;
			float max = 360.0f ;
			float half = Mathf.Abs( ( max - min ) * 0.5f ) ;
			float retval ;
			float diff ;
			if( ( end - start ) <  - half )
			{
				diff =   ( ( max - start ) + end   ) * value ;
				retval = start + diff ;
			}
			else
			if( ( end - start ) >    half )
			{
				diff = - ( ( max - end   ) + start ) * value ;
				retval = start + diff ;
			}
			else retval = start + ( end - start ) * value ;
			return retval ;
		}
#endif
	}
}
