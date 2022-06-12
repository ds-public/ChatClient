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
	/// Transition(ScrollView用) コンポーネントクラス
	/// </summary>
	public class UITransitionForScrollView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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
			easeInQuad,
			easeOutQuad,
			easeInOutQuad,
			easeInCubic,
			easeOutCubic,
			easeInOutCubic,
			easeInQuart,
			easeOutQuart,
			easeInOutQuart,
			easeInQuint,
			easeOutQuint,
			easeInOutQuint,
			easeInSine,
			easeOutSine,
			easeInOutSine,
			easeInExpo,
			easeOutExpo,
			easeInOutExpo,
			easeInCirc,
			easeOutCirc,
			easeInOutCirc,
			linear,
			spring,
			easeInBounce,
			easeOutBounce,
			easeInOutBounce,
			easeInBack,
			easeOutBack,
			easeInOutBack,
			easeInElastic,
			easeOutElastic,
			easeInOutElastic,
	//		punch
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

			public EaseTypes FadeEaseType = EaseTypes.linear ;
			public float FadeDuration = 0.2f ;

			public AnimationCurve FadeAnimationCurve = AnimationCurve.Linear(  0, 0, 0.5f, 1 ) ;

			public TransitionData( int state )
			{
				if( state == 0 )
				{
					FadeEaseType = EaseTypes.easeOutBack ;
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
					FadeEaseType = EaseTypes.easeOutBounce ;
					FadeDuration = 0.25f ;
				}
				else
				if( state == 5 )
				{
					FadeScale = new Vector3( 1.0f, 1.0f, 1.0f ) ;
					FadeEaseType = EaseTypes.linear ;
					FadeDuration = 0.1f ;
				}
			}
		}

		// SerializeField 対象には readonly をつけてはいけない
		[SerializeField][HideInInspector]
		private List<TransitionData> m_Transitions = new List<TransitionData>()
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

		/// <summary>
		/// スプライトをアトラス画像で動的に変更する
		/// </summary>
		[SerializeField][HideInInspector]
		private bool m_SpriteOverwriteEnabled = true ;
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

		private StateTypes m_State = StateTypes.Normal ;
		public  StateTypes   State
		{
			get
			{
				return m_State ;
			}
		}

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

		//-----------------------------------------------------------------

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
			}
		}
	
		internal void Update()
		{
			if( Application.isPlaying == true )
			{
				if( m_TransitionEnabled == true && m_IsPausing == false )
				{
					bool isDisable = false ;
					if( m_Button == null )
					{
						m_Button = GetComponent<Button>() ;
					}
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
						ChangeTransitionState( StateTypes.Disabled, StateTypes.Disabled ) ;
					}
					else
					if( m_State == StateTypes.Disabled && isDisable == false )
					{
						// 無効状態
						ChangeTransitionState( StateTypes.Normal, StateTypes.Disabled ) ;
					}

					if( m_State == StateTypes.Highlighted && m_IsHover == false )
					{
						// 無効状態
						ChangeTransitionState( StateTypes.Normal, StateTypes.Highlighted ) ;
					}

					//--------------------------------------------------------

					// 実行する
					if( m_Processing == true )
					{
						ProcessTransition() ;
					}
				}
			}
		}

		//---------------------------------------------
	
		// Enter
		public void OnPointerEnter( PointerEventData pointer )
		{
			// → Release 状態であれば Highlight へ遷移
			if( m_State != StateTypes.Finished )
			{
				if( m_Processing == false )
				{
					ChangeTransitionState( StateTypes.Highlighted, StateTypes.Highlighted ) ;
				}
				m_IsHover = true ;
			}
		}

		// Exit
		public void OnPointerExit( PointerEventData pointer )
		{
			// → Release 状態であれば Normal へ遷移
			if( m_State != StateTypes.Finished )
			{
				if( m_Processing == false )
				{
					ChangeTransitionState( StateTypes.Normal, StateTypes.Highlighted ) ;
				}
				m_IsHover = false ;
			}
		}

		//-------------------------------------------------------------------
		// トランジジョンの状態を変える
		private bool ChangeTransitionState( StateTypes state, StateTypes easeState )
		{
			if( m_State == state )
			{
				// 状態が同じなので処理しない
				return true ;
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

			// 現在変化中の状態を変化前の状態とする
			m_BaseRotation	= m_MoveRotation ;
			m_BaseScale		= m_MoveScale ;

			m_State			= state ;
			m_EaseState		= easeState ;
			m_BaseTime		= Time.realtimeSinceStartup ;
			m_Processing	= true ;

			if( m_SpriteOverwriteEnabled == true )
			{
				UIImage image = GetComponent<UIImage>() ;
				if( image != null && image.SpriteSet != null )
				{
					// 画像を変更する
					
					TransitionData data = m_Transitions[ ( int )m_State ] ;
	
					if( data.Sprite != null )
					{
						image.SetSpriteInAtlas( data.Sprite.name ) ;
					}
				}
			}

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
			}

			if( m_Processing == false )
			{
				// 終了
				if( m_State == StateTypes.Finished )
				{
					if( m_PauseAfterFinished == true )
					{
						m_IsPausing = true ;	// 動作をロックする
					}
				}
			}

			return true ;
		}

		/// <summary>
		/// 指定の状態で表示する画像を設定する
		/// </summary>
		/// <param name="tState"></param>
		/// <param name="tName"></param>
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
		/// <param name="tName"></param>
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


		//---------------------------------------------
	
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
				case EaseTypes.easeInQuad		: value = EaseInQuad(		start, end, factor )	; break ;
				case EaseTypes.easeOutQuad		: value = EaseOutQuad(		start, end, factor )	; break ;
				case EaseTypes.easeInOutQuad	: value = EaseInOutQuad(	start, end, factor )	; break ;
				case EaseTypes.easeInCubic		: value = EaseInCubic(		start, end, factor )	; break ;
				case EaseTypes.easeOutCubic		: value = EaseOutCubic(		start, end, factor )	; break ;
				case EaseTypes.easeInOutCubic	: value = EaseInOutCubic(	start, end, factor )	; break ;
				case EaseTypes.easeInQuart		: value = EaseInQuart(		start, end, factor )	; break ;
				case EaseTypes.easeOutQuart		: value = EaseOutQuart(		start, end, factor )	; break ;
				case EaseTypes.easeInOutQuart	: value = EaseInOutQuart(	start, end, factor )	; break ;
				case EaseTypes.easeInQuint		: value = EaseInQuint(		start, end, factor )	; break ;
				case EaseTypes.easeOutQuint		: value = EaseOutQuint(		start, end, factor )	; break ;
				case EaseTypes.easeInOutQuint	: value = EaseInOutQuint(	start, end, factor )	; break ;
				case EaseTypes.easeInSine		: value = EaseInSine(		start, end, factor )	; break ;
				case EaseTypes.easeOutSine		: value = EaseOutSine(		start, end, factor )	; break ;
				case EaseTypes.easeInOutSine	: value = EaseInOutSine(	start, end, factor )	; break ;
				case EaseTypes.easeInExpo		: value = EaseInExpo(		start, end, factor )	; break ;
				case EaseTypes.easeOutExpo		: value = EaseOutExpo(		start, end, factor )	; break ;
				case EaseTypes.easeInOutExpo	: value = EaseInOutExpo(	start, end, factor )	; break ;
				case EaseTypes.easeInCirc		: value = EaseInCirc(		start, end, factor )	; break ;
				case EaseTypes.easeOutCirc		: value = EaseOutCirc(		start, end, factor )	; break ;
				case EaseTypes.easeInOutCirc	: value = EaseInOutCirc(	start, end, factor )	; break ;
				case EaseTypes.linear			: value = Linear(			start, end, factor )	; break ;
				case EaseTypes.spring			: value = Spring(			start, end, factor )	; break ;
				case EaseTypes.easeInBounce		: value = EaseInBounce(		start, end, factor )	; break ;
				case EaseTypes.easeOutBounce	: value = EaseOutBounce(	start, end, factor )	; break ;
				case EaseTypes.easeInOutBounce	: value = EaseInOutBounce(	start, end, factor )	; break ;
				case EaseTypes.easeInBack		: value = EaseInBack(		start, end, factor )	; break ;
				case EaseTypes.easeOutBack		: value = EaseOutBack(		start, end, factor )	; break ;
				case EaseTypes.easeInOutBack	: value = EaseInOutBack(	start, end, factor )	; break ;
				case EaseTypes.easeInElastic	: value = EaseInElastic(	start, end, factor )	; break ;
				case EaseTypes.easeOutElastic	: value = EaseOutElastic(	start, end, factor )	; break ;
				case EaseTypes.easeInOutElastic	: value = EaseInOutElastic(	start, end, factor )	; break ;
			}
			return value ;
		}

		//------------------------

		private float EaseInQuad( float start, float end, float value )
		{
			end -= start ;
			return end * value * value + start ;
		}

		private float EaseOutQuad( float start, float end, float value )
		{
			end -= start ;
			return - end * value * ( value - 2 ) + start ;
		}

		private float EaseInOutQuad( float start, float end, float value )
		{
			value /= 0.5f ;
			end -= start ;
			if( value <  1 ) return end * 0.5f * value * value + start ;
			value -- ;
			return - end * 0.5f * ( value * ( value - 2 ) - 1 ) + start ;
		}

		private float EaseInCubic( float start, float end, float value )
		{
			end -= start ;
			return end * value * value * value + start ;
		}

		private float EaseOutCubic( float start, float end, float value )
		{
			value -- ;
			end -= start ;
			return end * ( value * value * value + 1 ) + start ;
		}

		private float EaseInOutCubic( float start, float end, float value )
		{
			value /= 0.5f ;
			end -= start ;
			if( value <  1 ) return end * 0.5f * value * value * value + start ;
			value -= 2 ;
			return end * 0.5f * ( value * value * value + 2 ) + start ;
		}

		private float EaseInQuart( float start, float end, float value )
		{
			end -= start ;
			return end * value * value * value * value + start ;
		}

		private float EaseOutQuart( float start, float end, float value )
		{
			value -- ;
			end -= start ;
			return - end * ( value * value * value * value - 1 ) + start ;
		}

		private float EaseInOutQuart( float start, float end, float value )
		{
			value /= 0.5f ;
			end -= start ;
			if( value <  1 ) return end * 0.5f * value * value * value * value + start ;
			value -= 2 ;
			return - end * 0.5f * ( value * value * value * value - 2 ) + start ;
		}

		private float EaseInQuint( float start, float end, float value )
		{
			end -= start ;
			return end * value * value * value * value * value + start ;
		}
	
		private float EaseOutQuint( float start, float end, float value )
		{
			value -- ;
			end -= start ;
			return end * ( value * value * value * value * value + 1 ) + start ;
		}

		private float EaseInOutQuint( float start, float end, float value )
		{
			value /= 0.5f ;
			end -= start ;
			if( value <  1 ) return end * 0.5f * value * value * value * value * value + start ;
			value -= 2 ;
			return end * 0.5f * ( value * value * value * value * value + 2 ) + start ;
		}

		private float EaseInSine( float start, float end, float value )
		{
			end -= start ;
			return - end * Mathf.Cos( value * ( Mathf.PI * 0.5f ) ) + end + start ;
		}

		private float EaseOutSine( float start, float end, float value )
		{
			end -= start ;
			return end * Mathf.Sin( value * ( Mathf.PI * 0.5f ) ) + start ;
		}

		private float EaseInOutSine( float start, float end, float value )
		{
			end -= start ;
			return - end * 0.5f * ( Mathf.Cos( Mathf.PI * value ) - 1 ) + start ;
		}

		private float EaseInExpo( float start, float end, float value )
		{
			end -= start ;
			return end * Mathf.Pow( 2, 10 * ( value - 1 ) ) + start ;
		}

		private float EaseOutExpo( float start, float end, float value )
		{
			end -= start ;
			return end * ( - Mathf.Pow( 2, - 10 * value ) + 1 ) + start ;
		}

		private float EaseInOutExpo( float start, float end, float value )
		{
			value /= 0.5f;
			end -= start ;
			if( value <  1 ) return end * 0.5f * Mathf.Pow( 2, 10 * ( value - 1 ) ) + start ;
			value -- ;
			return end * 0.5f * ( - Mathf.Pow( 2, - 10 * value ) + 2 ) + start ;
		}

		private float EaseInCirc( float start, float end, float value )
		{
			end -= start ;
			return - end * ( Mathf.Sqrt( 1 - value * value ) - 1 ) + start ;
		}

		private float EaseOutCirc( float start, float end, float value )
		{
			value -- ;
			end -= start ;
			return end * Mathf.Sqrt( 1 - value * value ) + start ;
		}

		private float EaseInOutCirc( float start, float end, float value )
		{
			value /= 0.5f ;
			end -= start ;
			if( value <  1 ) return - end * 0.5f * ( Mathf.Sqrt( 1 - value * value ) - 1 ) + start ;
			value -= 2 ;
			return end * 0.5f * ( Mathf.Sqrt( 1 - value * value ) + 1 ) + start ;
		}

		private float Linear( float start, float end, float value )
		{
			return Mathf.Lerp( start, end, value ) ;
		}
	
		private float Spring( float start, float end, float value )
		{
			value = Mathf.Clamp01( value ) ;
			value = ( Mathf.Sin( value * Mathf.PI * ( 0.2f + 2.5f * value * value * value ) ) * Mathf.Pow( 1f - value, 2.2f ) + value ) * ( 1f + ( 1.2f * ( 1f - value ) ) ) ;
			return start + ( end - start ) * value ;
		}

		private float EaseInBounce( float start, float end, float value )
		{
			end -= start ;
			float d = 1f ;
			return end - EaseOutBounce( 0, end, d - value ) + start ;
		}
	
		private float EaseOutBounce( float start, float end, float value )
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

		private float EaseInOutBounce( float start, float end, float value )
		{
			end -= start ;
			float d = 1f ;
			if( value <  d * 0.5f ) return EaseInBounce( 0, end, value * 2 ) * 0.5f + start ;
			else return EaseOutBounce( 0, end, value * 2 - d ) * 0.5f + end * 0.5f + start ;
		}

		private float EaseInBack( float start, float end, float value )
		{
			end -= start ;
			value /= 1 ;
			float s = 1.70158f ;
			return end * ( value ) * value * ( ( s + 1 ) * value - s ) + start ;
		}

		private float EaseOutBack( float start, float end, float value )
		{
			float s = 1.70158f ;
			end -= start ;
			value = ( value ) - 1 ;
			return end * ( ( value ) * value * ( ( s + 1 ) * value + s ) + 1 ) + start ;
		}

		private float EaseInOutBack( float start, float end, float value )
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

		private float EaseInElastic( float start, float end, float value )
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

		private float EaseOutElastic( float start, float end, float value )
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

		private float EaseInOutElastic( float start, float end, float value )
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
		private float Punch( float amplitude, float value )
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

		private float Clerp( float start, float end, float value )
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
