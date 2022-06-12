using System ;
using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.Events ;
using UnityEngine.EventSystems ;
using UnityEngine.Playables ;
using UnityEngine.Animations ;

#if UNITY_EDITOR
using UnityEditor ;
#endif

namespace uGUIHelper
{
	/// <summary>
	/// Tween コンポーネントクラス
	/// </summary>
	[ ExecuteInEditMode ]
	public class UITween : UIBehaviour
	{
/*
#if UNITY_EDITOR
		[MenuItem( "Tools/UITween/FieldRefactor" )]
		internal static void FieldRefactor()
		{
			int c = 0 ;
			UITween[] views = UIEditorUtility.FindComponents<UITween>
			(
				"Assets/Application",
				( _ ) =>
				{
//					_.m_InteractionDisableInPlaying = _.interactionDisableInPlaying ;

					c ++ ;
				}
			) ;
			Debug.LogWarning( "------> UITweenの数:" + c ) ;
		}
#endif
*/

		/// <summary>
		/// 識別名
		/// </summary>
		[SerializeField]
		private string m_Identity ;
		public  string   Identity
		{
			get
			{
				return m_Identity ;
			}
			set
			{
				m_Identity = value ;
			}
		}

		/// <summary>
		/// 遅延時間(秒)
		/// </summary>
		[SerializeField]
		private float m_Delay ;
		public  float   Delay
		{
			get
			{
				return m_Delay ;
			}
			set
			{
				m_Delay = value ;
			}
		}

		/// <summary>
		/// 実行時間(秒)
		/// </summary>
		[SerializeField]
		private float m_Duration = 1 ;
		public  float   Duration
		{
			get
			{
				return m_Duration ;
			}
			set
			{
				m_Duration = value ;
			}
		}

		/// <summary>
		/// 動作対象種別の定義
		/// </summary>
		public enum MotionTypes
		{
			Position = 0,
			Rotation = 1,
			Scale    = 2,
			Alpha    = 3,
		}

		[SerializeField][HideInInspector]
		private MotionTypes m_MotionType = MotionTypes.Position ;

		/// <summary>
		/// 動作対象種別
		/// </summary>
		public  MotionTypes  MotionType
		{
			get
			{
				return m_MotionType ;
			}
			set
			{
				if( m_MotionType != value )
				{
					m_MotionType  = value ;
					RefreshChecker() ;
				}
			}
		}
		
		/// <summary>
		/// カーブの処理方法種別の定義
		/// </summary>
		public enum ProcessTypes
		{
			Ease = 0,
			AnimationCurve = 1,
		}

		/// <summary>
		/// イーズの種別の定義
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
			EaseLinear,
			EaseSpring,
			EaseInBounce,
			EaseOutBounce,
			EaseInOutBounce,
			EaseInBack,
			EaseOutBack,
			EaseOutBackSharp,
			EaseInOutBack,
			EaseInElastic,
			EaseOutElastic,
			EaseInOutElastic,
	//		EasePunch
		}

		//-----------------------------------------------------------

		// ポジション

		[SerializeField][HideInInspector]
		private bool m_PositionEnabled = false ;

		/// <summary>
		/// 位置の変化を有効にするかどうか
		/// </summary>
		public  bool  PositionEnabled
		{
			get
			{
				return m_PositionEnabled ;
			}
			set
			{
				if( m_PositionEnabled != value )
				{
					m_PositionEnabled = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private Vector3 m_PositionFrom = Vector3.zero ;

		/// <summary>
		/// 開始位置
		/// </summary>
		public  Vector3  PositionFrom
		{
			get
			{
				return m_PositionFrom ;
			}
			set
			{
				if( m_PositionFrom != value )
				{
					m_PositionFrom = value ;
					RefreshChecker() ;
				}
			}
		}

		/// <summary>
		/// 開始位置Ｘ
		/// </summary>
		public float PositionFromX
		{
			get
			{
				return m_PositionFrom.x ;
			}
			set
			{
				PositionFrom = new Vector2( value, m_PositionFrom.y ) ;
			}
		}

		/// <summary>
		/// 開始位置Ｙ
		/// </summary>
		public float PositionFromY
		{
			get
			{
				return m_PositionFrom.y ;
			}
			set
			{
				PositionFrom = new Vector2( m_PositionFrom.x, value ) ;
			}
		}

		[SerializeField][HideInInspector]
		private Vector3 m_PositionTo   = Vector3.zero ;

		/// <summary>
		/// 終了位置
		/// </summary>
		public  Vector3  PositionTo
		{
			get
			{
				return m_PositionTo ;
			}
			set
			{
				if( m_PositionTo != value )
				{
					m_PositionTo = value ;
					RefreshChecker() ;
				}
			}
		}

		/// <summary>
		/// 終了位置Ｘ
		/// </summary>
		public float PositionToX
		{
			get
			{
				return m_PositionTo.x ;
			}
			set
			{
				PositionTo = new Vector2( value, m_PositionTo.y ) ;
			}
		}

		/// <summary>
		/// 終了位置Ｙ
		/// </summary>
		public float PositionToY
		{
			get
			{
				return m_PositionTo.y ;
			}
			set
			{
				PositionTo = new Vector2( m_PositionTo.x, value ) ;
			}
		}

		[SerializeField][HideInInspector]
		private ProcessTypes m_PositionProcessType = ProcessTypes.Ease ;

		/// <summary>
		/// 位置のカーブの処理方法種別
		/// </summary>
		public  ProcessTypes  PositionProcessType
		{
			get
			{
				return m_PositionProcessType ;
			}
			set
			{
				if( m_PositionProcessType != value )
				{
					m_PositionProcessType  = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private EaseTypes m_PositionEaseType = EaseTypes.EaseLinear ;

		/// <summary>
		/// 位置のイーズの種別
		/// </summary>
		public  EaseTypes  PositionEaseType
		{
			get
			{
				return m_PositionEaseType ;
			}
			set
			{
				if( m_PositionEaseType != value )
				{
					m_PositionEaseType  = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private AnimationCurve m_PositionAnimationCurve = AnimationCurve.Linear(  0, 0, 1.0f, 1.0f ) ;

		/// <summary>
		/// 位置のアニメーションカーブ
		/// </summary>
		public  AnimationCurve  PositionAnimationCurve
		{
			get
			{
				return m_PositionAnimationCurve ;
			}
			set
			{
				if( IsModifyAnamationCurve( m_PositionAnimationCurve, value ) == true )
				{
					m_PositionAnimationCurve = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private bool m_PositionFoldOut = true ;

		/// <summary>
		/// 位置のインスペクターでの表示領域操作(エディタのみ使用)
		/// </summary>
		public  bool  PositionFoldOut
		{
			get
			{
				return m_PositionFoldOut ;
			}
			set
			{
				m_PositionFoldOut = value ;
			}
		}

		//-----------------------------------------------------------

		// ローテーション

		[SerializeField][HideInInspector]
		private bool m_RotationEnabled = false ;

		/// <summary>
		/// 角度の変化を有効にするかどうか
		/// </summary>
		public  bool  RotationEnabled
		{
			get
			{
				return m_RotationEnabled ;
			}
			set
			{
				if( m_RotationEnabled != value )
				{
					m_RotationEnabled = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private Vector3 m_RotationFrom = Vector3.zero ;

		/// <summary>
		/// 開始角度
		/// </summary>
		public  Vector3  RotationFrom
		{
			get
			{
				return m_RotationFrom ;
			}
			set
			{
				if( m_RotationFrom != value )
				{
					m_RotationFrom = value ;
					RefreshChecker() ;
				}
			}
		}

		/// <summary>
		/// 開始角度Ｘ
		/// </summary>
		public float RotationFromX
		{
			get
			{
				return m_RotationFrom.x ;
			}
			set
			{
				RotationFrom = new Vector3( value, m_RotationFrom.y, m_RotationFrom.z ) ;
			}
		}

		/// <summary>
		/// 開始角度Ｙ
		/// </summary>
		public float RotationFromY
		{
			get
			{
				return m_RotationFrom.y ;
			}
			set
			{
				RotationFrom = new Vector3( m_RotationFrom.x, value, m_RotationFrom.z ) ;
			}
		}

		/// <summary>
		/// 開始角度Ｚ
		/// </summary>
		public float RotationFromZ
		{
			get
			{
				return m_RotationFrom.z ;
			}
			set
			{
				RotationFrom = new Vector3( m_RotationFrom.x, m_RotationFrom.y, value ) ;
			}
		}

		[SerializeField][HideInInspector]
		private Vector3 m_RotationTo   = Vector3.zero ;

		/// <summary>
		/// 終了角度
		/// </summary>
		public  Vector3  RotationTo
		{
			get
			{
				return m_RotationTo ;
			}
			set
			{
				if( m_RotationTo != value )
				{
					m_RotationTo = value ;
					RefreshChecker() ;
				}
			}
		}

		/// <summary>
		/// 終了角度Ｘ
		/// </summary>
		public float RotationToX
		{
			get
			{
				return m_RotationTo.x ;
			}
			set
			{
				RotationTo = new Vector3( value, m_RotationTo.y, m_RotationTo.z ) ;
			}
		}

		/// <summary>
		/// 終了角度Ｙ
		/// </summary>
		public float RotationToY
		{
			get
			{
				return m_RotationTo.y ;
			}
			set
			{
				RotationTo = new Vector3( m_RotationTo.x, value, m_RotationTo.z ) ;
			}
		}

		/// <summary>
		/// 終了角度Ｚ
		/// </summary>
		public float RotationToZ
		{
			get
			{
				return m_RotationTo.z ;
			}
			set
			{
				RotationFrom = new Vector3( m_RotationTo.x, m_RotationTo.y, value ) ;
			}
		}

		[SerializeField][HideInInspector]
		private ProcessTypes m_RotationProcessType = ProcessTypes.Ease ;

		/// <summary>
		/// 角度のカーブの処理種別
		/// </summary>
		public  ProcessTypes  RotationProcessType
		{
			get
			{
				return m_RotationProcessType ;
			}
			set
			{
				if( m_RotationProcessType != value )
				{
					m_RotationProcessType  = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private EaseTypes m_RotationEaseType = EaseTypes.EaseLinear ;

		/// <summary>
		/// 角度のイーズの種別
		/// </summary>
		public  EaseTypes  RotationEaseType
		{
			get
			{
				return m_RotationEaseType ;
			}
			set
			{
				if( m_RotationEaseType != value )
				{
					m_RotationEaseType  = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private AnimationCurve m_RotationAnimationCurve = AnimationCurve.Linear(  0, 0, 1.0f, 1.0f ) ;

		/// <summary>
		/// 角度のアニメーションカーブ
		/// </summary>
		public  AnimationCurve  RotationAnimationCurve
		{
			get
			{
				return m_RotationAnimationCurve ;
			}
			set
			{
				if( IsModifyAnamationCurve( m_RotationAnimationCurve, value ) == true )
				{
					m_RotationAnimationCurve = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private bool m_RotationFoldOut = true ;

		/// <summary>
		/// 角度のインスペクターでの表示領域操作(エディタのみ使用)
		/// </summary>
		public  bool  RotationFoldOut
		{
			get
			{
				return m_RotationFoldOut ;
			}
			set
			{
				m_RotationFoldOut = value ;
			}
		}

		//-------------------------------------------

		// スケール

		[SerializeField][HideInInspector]
		private bool m_ScaleEnabled = false ;

		/// <summary>
		/// 縮尺の変化を有効にするかどうか
		/// </summary>
		public  bool  ScaleEnabled
		{
			get
			{
				return m_ScaleEnabled ;
			}
			set
			{
				if( m_ScaleEnabled != value )
				{
					m_ScaleEnabled = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private Vector3 m_ScaleFrom    = Vector3.one ;

		/// <summary>
		/// 開始縮尺
		/// </summary>
		public  Vector3  ScaleFrom
		{
			get
			{
				return m_ScaleFrom ;
			}
			set
			{
				if( m_ScaleFrom != value )
				{
					m_ScaleFrom = value ;
					RefreshChecker() ;
				}
			}
		}

		/// <summary>
		/// 開始縮尺Ｘ
		/// </summary>
		public float ScaleFromX
		{
			get
			{
				return m_ScaleFrom.x ;
			}
			set
			{
				ScaleFrom = new Vector3( value, m_ScaleFrom.y, m_ScaleFrom.z ) ;
			}
		}

		/// <summary>
		/// 開始縮尺Ｙ
		/// </summary>
		public float ScaleFromY
		{
			get
			{
				return m_ScaleFrom.y ;
			}
			set
			{
				ScaleFrom = new Vector3( m_ScaleFrom.x, value, m_ScaleFrom.z ) ;
			}
		}

		/// <summary>
		/// 開始縮尺Ｚ
		/// </summary>
		public float ScaleFromZ
		{
			get
			{
				return m_ScaleFrom.z ;
			}
			set
			{
				ScaleFrom = new Vector3( m_ScaleFrom.x, m_ScaleFrom.y, value ) ;
			}
		}

		[SerializeField][HideInInspector]
		private Vector3 m_ScaleTo      = Vector3.one ;

		/// <summary>
		/// 終了縮尺
		/// </summary>
		public  Vector3  ScaleTo
		{
			get
			{
				return m_ScaleTo ;
			}
			set
			{
				if( m_ScaleTo != value )
				{
					m_ScaleTo = value ;
					RefreshChecker() ;
				}
			}
		}

		/// <summary>
		/// 終了縮尺Ｘ
		/// </summary>
		public float ScaleToX
		{
			get
			{
				return m_ScaleTo.x ;
			}
			set
			{
				ScaleTo = new Vector3( value, m_ScaleTo.y, m_ScaleTo.z ) ;
			}
		}

		/// <summary>
		/// 終了縮尺Ｙ
		/// </summary>
		public float ScaleToY
		{
			get
			{
				return m_ScaleTo.y ;
			}
			set
			{
				ScaleTo = new Vector3( m_ScaleTo.x, value, m_ScaleTo.z ) ;
			}
		}

		/// <summary>
		/// 終了縮尺Ｚ
		/// </summary>
		public float ScaleToZ
		{
			get
			{
				return m_ScaleTo.z ;
			}
			set
			{
				ScaleTo = new Vector3( m_ScaleTo.x, m_ScaleTo.y, value ) ;
			}
		}

		[SerializeField][HideInInspector]
		private ProcessTypes m_ScaleProcessType = ProcessTypes.Ease ;

		/// <summary>
		/// 縮尺のカーブの処理種別
		/// </summary>
		public  ProcessTypes  ScaleProcessType
		{
			get
			{
				return m_ScaleProcessType ;
			}
			set
			{
				if( m_ScaleProcessType != value )
				{
					m_ScaleProcessType  = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private EaseTypes m_ScaleEaseType = EaseTypes.EaseLinear ;

		/// <summary>
		/// 縮尺のイーズの種別
		/// </summary>
		public  EaseTypes  ScaleEaseType
		{
			get
			{
				return m_ScaleEaseType ;
			}
			set
			{
				if( m_ScaleEaseType != value )
				{
					m_ScaleEaseType  = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private AnimationCurve m_ScaleAnimationCurve = AnimationCurve.Linear(  0, 0, 1.0f, 1.0f ) ;

		/// <summary>
		/// 縮尺のアニメーションカーブ
		/// </summary>
		public  AnimationCurve  ScaleAnimationCurve
		{
			get
			{
				return m_ScaleAnimationCurve ;
			}
			set
			{
				if( IsModifyAnamationCurve( m_ScaleAnimationCurve, value ) == true )
				{
					m_ScaleAnimationCurve = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private bool m_ScaleFoldOut = true ;

		/// <summary>
		/// 縮尺のインスペクターでの表示領域操作(エディタのみ使用)
		/// </summary>
		public  bool  ScaleFoldOut
		{
			get
			{
				return m_ScaleFoldOut ;
			}
			set
			{
				m_ScaleFoldOut = value ;
			}
		}

		//-------------------------------------------

		// アルファ
		[SerializeField][HideInInspector]
		private bool m_AlphaEnabled = false ;
		public  bool   AlphaEnabled
		{
			get
			{
				return m_AlphaEnabled ;
			}
			set
			{
				if( m_AlphaEnabled != value )
				{
					m_AlphaEnabled = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private float   m_AlphaFrom    = 1 ;
		public  float     AlphaFrom
		{
			get
			{
				return m_AlphaFrom ;
			}
			set
			{
				if( m_AlphaFrom != value )
				{
					m_AlphaFrom = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private float   m_AlphaTo      = 1 ;
		public  float     AlphaTo
		{
			get
			{
				return m_AlphaTo ;
			}
			set
			{
				if( m_AlphaTo != value )
				{
					m_AlphaTo = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private ProcessTypes m_AlphaProcessType = ProcessTypes.Ease ;

		public  ProcessTypes   AlphaProcessType
		{
			get
			{
				return m_AlphaProcessType ;
			}
			set
			{
				if( m_AlphaProcessType != value )
				{
					m_AlphaProcessType  = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private EaseTypes m_AlphaEaseType = EaseTypes.EaseLinear ;

		public  EaseTypes   AlphaEaseType
		{
			get
			{
				return m_AlphaEaseType ;
			}
			set
			{
				if( m_AlphaEaseType != value )
				{
					m_AlphaEaseType  = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private AnimationCurve m_AlphaAnimationCurve = AnimationCurve.Linear(  0, 0, 1.0f, 1.0f ) ;
		public  AnimationCurve   AlphaAnimationCurve
		{
			get
			{
				return m_AlphaAnimationCurve ;
			}
			set
			{
				if( IsModifyAnamationCurve( m_AlphaAnimationCurve, value ) == true )
				{
					m_AlphaAnimationCurve = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private bool m_AlphaFoldOut = true ;
		public  bool   AlphaFoldOut
		{
			get
			{
				return m_AlphaFoldOut ;
			}
			set
			{
				m_AlphaFoldOut = value ;
			}
		}

		//-------------------------------------------

		[SerializeField][HideInInspector]
		private ProcessTypes m_ProcessType = ProcessTypes.Ease ;

		public  ProcessTypes  ProcessType
		{
			get
			{
				return m_ProcessType ;
			}
			set
			{
				if( m_ProcessType != value )
				{
					m_ProcessType  = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private EaseTypes m_EaseType = EaseTypes.EaseLinear ;

		public  EaseTypes   EaseType
		{
			get
			{
				return m_EaseType ;
			}
			set
			{
				if( m_EaseType != value )
				{
					m_EaseType  = value ;
					RefreshChecker() ;
				}
			}
		}

		// アニメーションカーブバージョン
		[SerializeField][HideInInspector]
		private AnimationCurve m_AnimationCurve = AnimationCurve.Linear(  0, 0, 1.0f, 1.0f ) ;
		public  AnimationCurve   AnimationCurve
		{
			get
			{
				return m_AnimationCurve ;
			}
			set
			{
				if( IsModifyAnamationCurve( m_AnimationCurve, value ) == true )
				{
					m_AnimationCurve = value ;
					RefreshChecker() ;
				}
			}
		}

		// アニメーションカーブに変化があったか確認する
		private bool IsModifyAnamationCurve( AnimationCurve a1, AnimationCurve a2 )
		{
			if( a1.length != a2.length || a1.postWrapMode != a2.postWrapMode || a1.preWrapMode != a2.preWrapMode )
			{
				return true ;
			}

			int i, l = a1.length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				Keyframe k1 = a1[ i ] ;	// 最終キー
				Keyframe k2 = a2[ i ] ;	// 最終キー

				if( k1.time != k2.time || k1.value != k2.value || k1.inTangent != k2.inTangent || k1.outTangent != k2.outTangent )
				{
					return true ;
				}
			}

			return false ;
		}

		public enum ValueTypes
		{
			Relative = 0,
			Absolute = 1,
			RelativeAndUpdate = 2,
		}

		[SerializeField]
		private ValueTypes m_ValueType = ValueTypes.Relative ;

		public  ValueTypes   ValueType
		{
			get
			{
				return m_ValueType ;
			}
			set
			{
				if( m_ValueType != value )
				{
					m_ValueType  = value ;
					RefreshChecker() ;
				}
			}
		}

		//------------------------------------------------------

		// AnimationClip
		[SerializeField][HideInInspector]
		private AnimationClip m_AnimationClip ;
		public  AnimationClip   AnimationClip
		{
			get
			{
				return m_AnimationClip ;
			}
			set
			{
				m_AnimationClip = value ;
			}
		}

		//------------------------------------------------------

		/// <summary>
		/// ループさせるか
		/// </summary>
		[SerializeField]
		private bool m_Loop = false ;
		public  bool   Loop
		{
			get
			{
				return m_Loop ;
			}
			set
			{
				m_Loop = value ;
			}
		}

		/// <summary>
		/// ループ場合に終了時に逆再生するか
		/// </summary>
		[SerializeField]
		private bool m_Reverse = true ;
		public  bool   Reverse
		{
			get
			{
				return m_Reverse ;
			}
			set
			{
				m_Reverse = value ;
			}
		}

		// タイムスケールを無視するかどうか（する）
		[SerializeField]
		private bool m_IgnoreTimeScale = true ;
		public  bool   IgnoreTimeScale
		{
			get
			{
				return m_IgnoreTimeScale ;
			}
			set
			{
				m_IgnoreTimeScale = value ;
			}
		}

		/// <summary>
		/// インスタンス生成と同時に実行されるようにするか
		/// </summary>
		[SerializeField]
		private bool m_PlayOnAwake = false ;
		public  bool   PlayOnAwake
		{
			get
			{
				return m_PlayOnAwake ;
			}
			set
			{
				m_PlayOnAwake = value ;
			}
		}

		/// <summary>
		/// 終了と同時にゲームオブジェクトを破棄するかどうか
		/// </summary>
		[SerializeField]
		private bool m_DestroyAtEnd ;
		public  bool   DestroyAtEnd
		{
			get
			{
				return m_DestroyAtEnd ;
			}
			set
			{
				m_DestroyAtEnd = value ;
			}
		}

		/// <summary>
		/// 処理中は入力を受け付けないようにするか
		/// </summary>
		[SerializeField]
		private bool m_InteractionDisableInPlaying = true ;
		public  bool   InteractionDisableInPlaying
		{
			get
			{
				return m_InteractionDisableInPlaying ;
			}
			set
			{
				m_InteractionDisableInPlaying = value ;
			}
		}

		//--------------------------------------------------------

		private RectTransform			m_RectTransform ;

		private CanvasGroup				m_CanvasGroup ;

		private Vector3					m_BasePosition ;
		private Vector3					m_BaseRotation ;
		private Vector3					m_BaseScale ;

		private float					m_BaseAlpha ;

		private UIView					m_View ;
		public  UIView					  View
		{
			get
			{
				return m_View ;
			}
		}

		private float					m_Time ;
		private float					m_BaseTime ;

		// 実行状況
		private bool					m_IsRunning ;
		public  bool					  IsRunning{ get{ return m_IsRunning ; } }

		// 再生状況
		private bool					m_IsPlaying ;
		public  bool					  IsPlaying{ get{ return m_IsPlaying ; } }

		// 動作状況
		private bool					m_IsMoving ;
		public  bool					  IsMoving{ get{ return m_IsMoving ; }}


		private bool					m_IsStarting ;

		// 動作保存
		private float					m_DelayStack				= -1 ;
		private float					m_DurationStack				= -1 ;
		private float					m_OffsetStack				=  0 ;
		private Action<string,UITween>	m_OnFinishedActionStack		= null ;
		private float					m_AdditionalDelayStack		=  0 ;
		private float					m_AdditionalDurationStack	=  0 ;

		private bool					m_Busy = false ;

		private float					m_ActiveDelay ;
		private float					m_ActiveDuration ;

		//-----------------------------------------------------------

		private bool					m_IsInitialized ;

		// プレイアブルグラフ
		private PlayableGraph			m_PlayableGraph ;

		// プレイアブルアウトプット
		private AnimationPlayableOutput	m_AnimationPlayableOutupt ;

		// 再生中アニメーションクリップ
		private AnimationClipPlayable	m_AnimationClipPlayable ;

		//--------------------------------------------------------

		/// <summary>
		/// 再生中(毎フレーム)呼び出されるアクション
		/// </summary>
		public Action<string, UITween, float> OnModifiedAction ;

		/// <summary>
		/// 再生中(毎フレーム)呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="identity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="tween">トゥイーンのインスタンス</param>
		public delegate void OnModified( string identity, UITween tween, float factor ) ;

		/// <summary>
		/// 再生中(毎フレーム)呼び出されるデリゲート
		/// </summary>
		public OnModified OnModifiedDelegate ;

		//---------------

		/// <summary>
		/// 再生終了した際に呼び出されるアクション
		/// </summary>
		public Action<string, UITween> OnFinishedAction ;

		/// <summary>
		/// 再生終了した際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="identity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="tween">トゥイーンのインスタンス</param>
		public delegate void OnFinished( string identity, UITween tween ) ;

		/// <summary>
		/// 再生終了した際に呼び出されるデリゲート
		/// </summary>
		public OnFinished OnFinishedDelegate ;

		//--------------------------------------------------------

		override protected void OnDisable()
		{
			base.OnDisable() ;

			// 停止状態にする
			m_IsRunning = false ;
			m_IsPlaying = false ;

			// 無効化した際にチェッカーが開いていれば強制的に閉じる
			IsChecker = false ;
		}

		override protected void Awake()
		{
			base.Awake() ;

			if( Application.isPlaying == true && IsChecker == true )
			{
				LoadState() ;
			}		
		}
	
		override protected void Start()
		{
			base.Start() ;

			m_IsStarting = true ;	// スタートが実行された

			if( Application.isPlaying == true && ( m_PlayOnAwake == true || m_IsRunning == true ) )
			{
				// Start() の前に　Play が実行されている可能性もある
				Play( m_DelayStack, m_DurationStack, m_OffsetStack, m_OnFinishedActionStack, m_AdditionalDelayStack, m_AdditionalDurationStack ) ;
			}
		}
	
		//---------------------------------------------

		// 再生中(毎フレーム)呼び出される内部リスナー
		private void OnModifiedInner( float factor )
		{
			OnModifiedAction?.Invoke( m_Identity, this, factor ) ;
			OnModifiedDelegate?.Invoke( m_Identity, this, factor ) ;
		}

		/// <summary>
		/// 再生中(毎フレーム)呼び出されるアクションを設定する
		/// </summary>
		/// <param name="onFinishAction"></param>
		public void SetOnModified( Action<string, UITween, float> onModifiedAction )
		{
			OnModifiedAction = onModifiedAction ;
		}

		/// <summary>
		/// 再生中(毎フレーム)呼び出されるデリゲートを追加する
		/// </summary>
		/// <param name="onFinished"></param>
		public void AddOnModifiedDelegate( OnModified onModifiedDelegate )
		{
			OnModifiedDelegate += onModifiedDelegate ;
		}
		
		/// <summary>
		/// 再生終了の際に呼び出されるデリゲート削除する
		/// </summary>
		public void RemoveOnFinishedDelegate( OnModified onModifiedDelegate )
		{
			OnModifiedDelegate -= onModifiedDelegate ;
		}
	
		//---------------

		// 再生終了の際に呼び出される内部リスナー
		private void OnFinishedInner()
		{
			OnFinishedAction?.Invoke( m_Identity, this ) ;
			OnFinishedDelegate?.Invoke( m_Identity, this ) ;
		}

		/// <summary>
		/// 再生終了の際に呼び出されるアクションを設定する
		/// </summary>
		/// <param name="onFinishAction"></param>
		public void SetOnFinished( Action<string, UITween> onFinishedAction )
		{
			OnFinishedAction = onFinishedAction ;
		}

		/// <summary>
		/// 再生終了の際に呼び出されるデリゲートを追加する
		/// </summary>
		/// <param name="onFinished"></param>
		public void AddOnFinishedDelegate( OnFinished onFinishedDelegate )
		{
			OnFinishedDelegate += onFinishedDelegate ;
		}
		
		/// <summary>
		/// 再生終了の際に呼び出されるデリゲート削除する
		/// </summary>
		public void RemoveOnFinishedDelegate( OnFinished onFinishedDelegate )
		{
			OnFinishedDelegate -= onFinishedDelegate ;
		}
	
		//---------------------------------------------
	
		private bool m_KeepInteractionDisableProcess = false ; 

		private readonly List<Button> m_DisableTargetButtons = new List<Button>() ;

		private void DisableInteraction( CanvasGroup canvasGroup )
		{
#if false
			if( canvasGroup == null )
			{
				// 処理出来ない
				return ;
			}

//			if( canvasGroup.blocksRaycasts == false )
			if( canvasGroup.interactable == false )
			{
				// 最初から無効化されていたら処理しない
				return ;
			}
#endif
			// 同じ gameObject にアタッチされた全ての UITween を取得し、既に処理が行われていないか確認する。
			UITween[] tweens = GetComponents<UITween>() ;
			if( tweens != null && tweens.Length >  0 )
			{
				int i, l = tweens.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( tweens[ i ] != this )
					{
						if( tweens[ i ].m_KeepInteractionDisableProcess == true )
						{
							// 既に処理が行われている
							return ;
						}
					}
				}
			}

#if false
			// ブロックレイキャストを無効にする
//			canvasGroup.blocksRaycasts = false ;
			canvasGroup.interactable = false ;
#endif
			Button[] buttons = GetComponentsInChildren<Button>() ;
			foreach( var button in buttons )
			{
				// ボタンを無効化する
				if( button.enabled == true && button.interactable == true )
				{
//					button.enabled = false ;
					Image image = button.GetComponent<Image>() ;
					if( image != null && image.raycastTarget == true )
					{
						image.raycastTarget = false ;
						m_DisableTargetButtons.Add( button ) ;
					}
				}
			}

			// 自身が処理を担当する
			m_KeepInteractionDisableProcess = true ;
		}

		private void EnableInteraction( CanvasGroup canvasGroup )
		{
			// 自身は処理の担当ではない
			if( m_KeepInteractionDisableProcess == false )
			{
				return ;
			}

			if( m_DisableTargetButtons.Count >  0 )
			{
				// 無効化したボタンを元に戻す
				foreach( var button in m_DisableTargetButtons )
				{
//					button.enabled = true ;
					Image image = button.GetComponent<Image>() ;
					if( image != null )
					{
						image.raycastTarget = true ;
					}
				}

				m_DisableTargetButtons.Clear() ;
			}
#if false
			if( canvasGroup == null )
			{
				// 処理出来ない
				return ;
			}

			// ブロックレイキャストを有効にする
//			canvasGroup.blocksRaycasts = true ;
			canvasGroup.interactable = true ;
#endif
			// 処理終了
			m_KeepInteractionDisableProcess = false ;
		}

		//---------------------------------------------
	
		/// <summary>
		/// 再生する
		/// </summary>
		/// <param name="delay"></param>
		/// <param name="duration"></param>
		public void Play( float delay = -1, float duration = -1, float offset = 0, Action<string,UITween> onFinishedAction = null, float additionalDeley = 0, float additionalDuration = 0 )
		{
			enabled = true ;

			if( onFinishedAction != null )
			{
				// 既に設定されている可能性がある
				OnFinishedAction = onFinishedAction ;
			}

			if( m_IsStarting == false )
			{
				// スタート前なのでためるだけ
				m_DelayStack				= delay ;
				m_DurationStack				= duration ;
				m_OffsetStack				= offset ;
				m_OnFinishedActionStack		= onFinishedAction ; 
				m_AdditionalDelayStack		= additionalDeley ;
				m_AdditionalDurationStack	= additionalDuration ;

				m_IsRunning = true ;

				return ;
			}

			//-------------------------------------------

			if( delay >= 0 )
			{
				m_ActiveDelay = delay ;
			}
			else
			{
				m_ActiveDelay = m_Delay ;
			}

			if( duration >= 0 )
			{
				m_ActiveDuration = duration ;
			}
			else
			{
				m_ActiveDuration = m_Duration ;
			}

			m_ActiveDelay		+= additionalDeley ;
			m_ActiveDuration	+= additionalDuration ;

			m_RectTransform	= gameObject.GetComponent<RectTransform>() ;
			if( m_RectTransform != null )
			{
				m_BasePosition		= m_RectTransform.anchoredPosition ;
				m_BaseRotation		= m_RectTransform.localEulerAngles ;
				m_BaseScale			= m_RectTransform.localScale ;
			}

			m_CanvasGroup	= gameObject.GetComponent<CanvasGroup>() ;
			if( m_CanvasGroup != null )
			{
				m_BaseAlpha			= m_CanvasGroup.alpha ;

				if( m_InteractionDisableInPlaying == true )
				{
					DisableInteraction( m_CanvasGroup ) ;
				}
			}

			//--------------------------------------------------

			m_View = gameObject.GetComponent<UIView>() ;
			if( m_View != null )
			{
				m_BasePosition = m_View.LocalPosition ;
				m_BaseRotation = m_View.LocalRotation ;
				m_BaseScale    = m_View.LocalScale ;
				m_BaseAlpha    = m_View.LocalAlpha ;
			}

			if( m_IsInitialized == false )
			{
				// 全ての再生で１度だけ実行する処理
				m_IsInitialized  = true ;

				if( m_AnimationClip != null )
				{
					// m_IsInitialized で一回だけ実行されるように制限されているが念の為の保険で未初期化の場合のみクリエイトを実行する
					if( m_PlayableGraph.IsValid() == false )
					{
						m_PlayableGraph = PlayableGraph.Create() ;
					}

					// モデルにアニメーターを追加(無ければ)
					Animator animator = gameObject.GetComponent<Animator>() ;
					if( animator == null )
					{
						animator = gameObject.AddComponent<Animator>() ;
					}

					// プレイアブルアウトプットを生成
					m_AnimationPlayableOutupt = AnimationPlayableOutput.Create( m_PlayableGraph, "output", animator ) ;

					// プレイアブル化
					m_AnimationClipPlayable = AnimationClipPlayable.Create( m_PlayableGraph, m_AnimationClip ) ;

					// モーション設定
					m_AnimationPlayableOutupt.SetSourcePlayable( m_AnimationClipPlayable ) ;
				}
			}

			m_IsMoving  = false ;	// SetState より前に初期化すること

			//-------------------------------

			m_Time = offset ;

			SetState( m_Time ) ;	// 初期位置へ移動させる

			m_BaseTime = Time.realtimeSinceStartup ;

			m_IsRunning = true ;
			m_IsPlaying = true ;

			m_Busy = true ;
		}

		/// <summary>
		/// 状態を再生前に戻す(実行中であれば実行継続)
		/// </summary>
		public void Revert()
		{
			if( m_Busy == false )
			{
				return ;
			}

			m_IsRunning = false ;
			m_IsPlaying = false ;
			m_IsMoving  = false ;

			m_RectTransform	= gameObject.GetComponent<RectTransform>() ;
			if( m_RectTransform != null )
			{
				m_RectTransform.anchoredPosition	= m_BasePosition ;
				m_RectTransform.localEulerAngles	= m_BaseRotation ;
				m_RectTransform.localScale			= m_BaseScale ;
			}

			m_CanvasGroup	= gameObject.GetComponent<CanvasGroup>() ;
			if( m_CanvasGroup != null )
			{
				m_CanvasGroup.alpha =  m_BaseAlpha ;
			}

			m_Busy = false ;
		}

		/// <summary>
		/// 一時停止する
		/// </summary>
		public void Pause()
		{
			if( m_IsRunning == true )
			{
				m_IsPlaying = false ;
			}
		}

		/// <summary>
		/// 再開する
		/// </summary>
		public void Unpause()
		{
			if( m_IsRunning == true )
			{
				m_IsPlaying = true ;
			}
		}

		/// <summary>
		/// 完全停止する
		/// </summary>
		public void Stop()
		{
			if( m_IsRunning == true )
			{
				if( m_InteractionDisableInPlaying == true )
				{
					EnableInteraction( m_CanvasGroup ) ;
				}

				m_IsPlaying = false ;
				m_IsRunning = false ;
			}
		}

		/// <summary>
		/// 完全に停止させ状態を元に戻す
		/// </summary>
		public void StopAndReset()
		{
			if( m_IsRunning == true )
			{
				if( m_InteractionDisableInPlaying == true )
				{
					EnableInteraction( m_CanvasGroup ) ;
				}

				m_IsPlaying = false ;
				m_IsRunning = false ;
			}

			Revert() ;
		}

		/// <summary>
		/// 強制的に最後の状態にする
		/// </summary>
		public void Finish()
		{
			// 強制的に最後の状態にする(ただしループになっているものは無効)
			if( m_Loop == true )
			{
				return ;
			}

			if( m_IsRunning == true )
			{
				Modify( 1, true ) ;

				// 終了
				if( m_InteractionDisableInPlaying == true )
				{
					EnableInteraction( m_CanvasGroup ) ;
				}

				// コールバック呼び出し
				OnFinishedInner() ;

				m_IsPlaying = false ;
				m_IsRunning = false ;
			}
			else
			{
				Modify( 1, true ) ;

				m_IsPlaying = false ;
				m_IsRunning = false ;
			}
		}

		internal void Update()
		{
			if( m_IsRunning == true && m_IsPlaying == true )
			{
				float timeScale = 1 ;
				if( m_View != null )
				{
					timeScale = m_View.TimeScale ;
				}

				m_Time += ( GetDeltaTime() * timeScale ) ;

				if( SetState( m_Time ) == true )
				{
					// 終了
					if( m_InteractionDisableInPlaying == true )
					{
						EnableInteraction( m_CanvasGroup ) ;
					}

					m_IsPlaying = false ;
					m_IsRunning = false ;

					// コールバック呼び出し
					OnFinishedInner() ;

					if( m_DestroyAtEnd == true )
					{
						Destroy( gameObject ) ;
					}
				}
			}
		}

		// 現在の経過時間から状態を設定する
		private bool SetState( float time )
		{
			float duration = 0 ;
			if( m_ProcessType == ProcessTypes.Ease )
			{
				duration = m_ActiveDuration ;
			}
			else
			if( m_ProcessType == ProcessTypes.AnimationCurve )
			{
				int l = m_AnimationCurve.length ;
				Keyframe keyFrame = m_AnimationCurve[ l - 1 ] ;	// 最終キー
				duration = keyFrame.time ;
			}

			if( duration <= 0 )
			{
				// 終了
				Modify( 1, true ) ;
				return true ;
			}

			//---------------------------------------------

			if( time <  m_ActiveDelay )
			{
				Modify( 0, false ) ;
				return false ;
			}

			if( m_IsMoving == false )
			{
				if( m_AnimationClip != null )
				{
					// アニメーションクリップを動作させる
					m_PlayableGraph.Play() ;
				}

				m_IsMoving  = true ;
			}

			//---------------------------------------------

			if( m_AnimationClip == null )
			{
				// アニメーションクリップなし

				time = m_Time - m_ActiveDelay ;

				if( m_View != null )
				{
					m_BasePosition = m_View.LocalPosition ;
					m_BaseRotation = m_View.LocalRotation ;
					m_BaseScale    = m_View.LocalScale ;
					m_BaseAlpha    = m_View.LocalAlpha ;
				}

				int count ;
				float factor ;

				if( m_Loop == false )
				{
					// ループ無し

					factor  = time / duration ;
					if( factor >= 1 )
					{
						factor  = 1 ;
					}

					Modify( factor, factor >= 1 ) ;

					if( factor >= 1 )
					{
						return true ;	// 終了
					}
				}
				else
				{
					// ループ有り
				
					count  = ( int )( time / duration ) ;
					factor = ( time % duration ) / duration ;

					if( m_Reverse == true && ( count & 1 ) == 1 )
					{
						factor = 1.0f - factor ;
					}

					Modify( factor, false ) ;
				}
			}
			else
			{
				// アニメーションクリップあり
				if( m_AnimationClip.isLooping == false )
				{
					// ループ無し
					if( ( m_AnimationClipPlayable.GetTime() <  0 ) || ( m_AnimationClipPlayable.GetTime() >  m_AnimationClipPlayable.GetAnimationClip().length ) )
					{
						return true ;	// 終了
					}
				}
				else
				{
					// ループ有り
				}
			}

			return false ;	// 継続
		}

		/// <summary>
		/// 実行開始からの経過時間
		/// </summary>
		public float ProcessTime
		{
			get
			{
				return m_Time ;
			}
			set
			{
				m_Time = value ;

				// ベースタイムをリセットしておかないと次のフレームで大きく飛んでしまう
				m_BaseTime = Time.realtimeSinceStartup ;

				if( m_IsRunning == true )
				{
					SetState( m_Time ) ;
				}
			}
		}

		// 変化後のスケールを 0 にならないよう補正をかける
		private Vector3 CorrectScale( Vector3 target, Vector3 factor )
		{
			// x
			if( target.x == 0 )
			{
				if( factor.x >  0 )
				{
					target.x =  0.001f ;
				}
				else
				if( factor.x <  0 )
				{
					target.x = -0.001f ;
				}
			}

			// y
			if( target.y == 0 )
			{
				if( factor.y >  0 )
				{
					target.y =  0.001f ;
				}
				else
				if( factor.y <  0 )
				{
					target.y = -0.001f ;
				}
			}

			// z
			if( target.z == 0 )
			{
				if( factor.z >  0 )
				{
					target.z =  0.001f ;
				}
				else
				if( factor.z <  0 )
				{
					target.z = -0.001f ;
				}
			}

			return target ;
		}


		// トランスフォームまたはアルファを変化させる
		private void Modify( float factor, bool finished )
		{
			if( m_RectTransform != null )
			{
				if( m_PositionEnabled == true )
				{
					Vector3 delta = GetValue( m_PositionFrom, m_PositionTo, factor, m_PositionProcessType, m_PositionEaseType, m_PositionAnimationCurve ) ;

					if( m_ValueType == ValueTypes.Relative || m_ValueType == ValueTypes.RelativeAndUpdate )
					{
						m_RectTransform.anchoredPosition = m_BasePosition + delta ;

						if( finished == true && m_ValueType == ValueTypes.RelativeAndUpdate && m_View != null )
						{
							m_View.LocalPosition = m_BasePosition + delta ;
						}
					}
					else
					{
						m_RectTransform.anchoredPosition = delta ;
					}
				}

				if( m_RotationEnabled == true )
				{
					Vector3 delta = GetValue( m_RotationFrom, m_RotationTo, factor, m_RotationProcessType, m_RotationEaseType, m_RotationAnimationCurve ) ;
			
					if( m_ValueType == ValueTypes.Relative || m_ValueType == ValueTypes.RelativeAndUpdate )
					{
						m_RectTransform.localEulerAngles = m_BaseRotation + delta ;

						if( finished == true && m_ValueType == ValueTypes.RelativeAndUpdate && m_View != null )
						{
							m_View.LocalRotation = m_BaseRotation + delta ;
						}
					}
					else
					{
						m_RectTransform.localEulerAngles = delta ;
					}
				}

				if( m_ScaleEnabled == true )
				{
					Vector3 delta = GetValue( CorrectScale( m_ScaleFrom, m_ScaleTo ), CorrectScale(  m_ScaleTo, m_ScaleFrom ), factor, m_ScaleProcessType, m_ScaleEaseType, m_ScaleAnimationCurve ) ;

					if( m_ValueType == ValueTypes.Relative || m_ValueType == ValueTypes.RelativeAndUpdate )
					{
						m_RectTransform.localScale = new Vector3( m_BaseScale.x * delta.x, m_BaseScale.y * delta.y, m_BaseScale.z * delta.z ) ;

						if( finished == true && m_ValueType == ValueTypes.RelativeAndUpdate && m_View != null )
						{
							m_View.LocalScale = new Vector3( m_BaseScale.x * delta.x, m_BaseScale.y * delta.y, m_BaseScale.z * delta.z ) ;
						}
					}
					else
					{
						m_RectTransform.localScale = delta ;
					}
				}
			}

			if( m_CanvasGroup != null )
			{
				if( m_AlphaEnabled == true )
				{
					float delta = GetValue( m_AlphaFrom, m_AlphaTo, factor, m_AlphaProcessType, m_AlphaEaseType, m_AlphaAnimationCurve ) ;
				
					if( m_View == null )
					{
						if( m_ValueType == ValueTypes.Relative )
						{
							m_CanvasGroup.alpha = m_BaseAlpha * delta ;
						}
						else
						{
							m_CanvasGroup.alpha = delta ;
						}
					}
					else
					{
						if( m_ValueType == ValueTypes.Relative )
						{
							m_CanvasGroup.alpha = m_BaseAlpha * delta * ( m_View.Visible == true ? 1 : 0 ) ;
						}
						else
						{
							m_CanvasGroup.alpha = delta * ( m_View.Visible == true ? 1 : 0 ) ;
						}

						if( m_CanvasGroup.alpha <  m_View.DisableRaycastUnderAlpha )
						{
							m_CanvasGroup.blocksRaycasts = false ;	// 無効
						}
						else
						{
							m_CanvasGroup.blocksRaycasts = true ;	// 有効
						}
					}
				}
			}

			// コールバック呼び出し
			OnModifiedInner( factor ) ;
		}

		// 経過時間を取得する
		private float GetDeltaTime()
		{
			float baseTime = Time.realtimeSinceStartup ;
			float deltaTime = baseTime - m_BaseTime ;

			m_BaseTime = baseTime ;

			if( m_IgnoreTimeScale == false )
			{
				// タイムスケールを無視しない
				deltaTime *= Time.timeScale ;
			}

			return deltaTime ;
		}

		override protected void OnDestroy()
		{
			base.OnDestroy() ;

			if( m_PlayableGraph.IsValid() == true )
			{
				m_PlayableGraph.Destroy() ;
			}
		}

		//--------------------------------------------------------

		// Vector3 の変化中の値を取得
		public static Vector3 GetValue( Vector3 start, Vector3 end, float factor, ProcessTypes processType, EaseTypes easeType, AnimationCurve animationCurve = null )
		{
			float x = GetValue( start.x, end.x, factor, processType, easeType, animationCurve ) ;
			float y = GetValue( start.y, end.y, factor, processType, easeType, animationCurve ) ;
			float z = GetValue( start.z, end.z, factor, processType, easeType, animationCurve ) ;

			return new Vector3( x, y, z ) ;
		}

		// float の変化中の値を取得
		public static float GetValue( float start, float end, float factor, ProcessTypes processType, EaseTypes easeType, AnimationCurve animationCurve = null )
		{
			float value = 0 ;

			if( processType == ProcessTypes.Ease )
			{
				switch( easeType )
				{
					case EaseTypes.EaseInQuad		: value = GetEaseInQuad(		start, end, factor )	; break ;
					case EaseTypes.EaseOutQuad		: value = GetEaseOutQuad(		start, end, factor )	; break ;
					case EaseTypes.EaseInOutQuad	: value = GetEaseInOutQuad(		start, end, factor )	; break ;
					case EaseTypes.EaseInCubic		: value = GetEaseInCubic(		start, end, factor )	; break ;
					case EaseTypes.EaseOutCubic		: value = GetEaseOutCubic(		start, end, factor )	; break ;
					case EaseTypes.EaseInOutCubic	: value = GetEaseInOutCubic(	start, end, factor )	; break ;
					case EaseTypes.EaseInQuart		: value = GetEaseInQuart(		start, end, factor )	; break ;
					case EaseTypes.EaseOutQuart		: value = GetEaseOutQuart(		start, end, factor )	; break ;
					case EaseTypes.EaseInOutQuart	: value = GetEaseInOutQuart(	start, end, factor )	; break ;
					case EaseTypes.EaseInQuint		: value = GetEaseInQuint(		start, end, factor )	; break ;
					case EaseTypes.EaseOutQuint		: value = GetEaseOutQuint(		start, end, factor )	; break ;
					case EaseTypes.EaseInOutQuint	: value = GetEaseInOutQuint(	start, end, factor )	; break ;
					case EaseTypes.EaseInSine		: value = GetEaseInSine(		start, end, factor )	; break ;
					case EaseTypes.EaseOutSine		: value = GetEaseOutSine(		start, end, factor )	; break ;
					case EaseTypes.EaseInOutSine	: value = GetEaseInOutSine(		start, end, factor )	; break ;
					case EaseTypes.EaseInExpo		: value = GetEaseInExpo(		start, end, factor )	; break ;
					case EaseTypes.EaseOutExpo		: value = GetEaseOutExpo(		start, end, factor )	; break ;
					case EaseTypes.EaseInOutExpo	: value = GetEaseInOutExpo(		start, end, factor )	; break ;
					case EaseTypes.EaseInCirc		: value = GetEaseInCirc(		start, end, factor )	; break ;
					case EaseTypes.EaseOutCirc		: value = GetEaseOutCirc(		start, end, factor )	; break ;
					case EaseTypes.EaseInOutCirc	: value = GetEaseInOutCirc(		start, end, factor )	; break ;
					case EaseTypes.EaseLinear		: value = GetLinear(			start, end, factor )	; break ;
					case EaseTypes.EaseSpring		: value = GetSpring(			start, end, factor )	; break ;
					case EaseTypes.EaseInBounce		: value = GetEaseInBounce(		start, end, factor )	; break ;
					case EaseTypes.EaseOutBounce	: value = GetEaseOutBounce(		start, end, factor )	; break ;
					case EaseTypes.EaseInOutBounce	: value = GetEaseInOutBounce(	start, end, factor )	; break ;
					case EaseTypes.EaseInBack		: value = GetEaseInBack(		start, end, factor )	; break ;
					case EaseTypes.EaseOutBack		: value = GetEaseOutBack(		start, end, factor )	; break ;
					case EaseTypes.EaseOutBackSharp	: value = GetEaseOutBackSharp(	start, end, factor )	; break ;
					case EaseTypes.EaseInOutBack	: value = GetEaseInOutBack(		start, end, factor )	; break ;
					case EaseTypes.EaseInElastic	: value = GetEaseInElastic(		start, end, factor )	; break ;
					case EaseTypes.EaseOutElastic	: value = GetEaseOutElastic(	start, end, factor )	; break ;
					case EaseTypes.EaseInOutElastic	: value = GetEaseInOutElastic(	start, end, factor )	; break ;
				}
			}
			else
			if( processType == ProcessTypes.AnimationCurve )
			{
				if( animationCurve != null )
				{
					int l = animationCurve.length ;
					Keyframe keyFrame = animationCurve[ l - 1 ] ;	// 最終キー
					float duration = keyFrame.time ;
	
					value = animationCurve.Evaluate( duration * factor ) ;
				}

				value = Mathf.Lerp( start, end, value ) ;
			}

			return value ;
		}

		//------------------------

		private static float GetEaseInQuad( float start, float end, float value )
		{
			end -= start ;
			return end * value * value + start ;
		}

		private static float GetEaseOutQuad( float start, float end, float value )
		{
			end -= start ;
			return - end * value * ( value - 2 ) + start ;
		}

		private static float GetEaseInOutQuad( float start, float end, float value )
		{
			value /= 0.5f ;
			end -= start ;
			if( value <  1 ) return end * 0.5f * value * value + start ;
			value -- ;
			return - end * 0.5f * ( value * ( value - 2 ) - 1 ) + start ;
		}

		private static float GetEaseInCubic( float start, float end, float value )
		{
			end -= start ;
			return end * value * value * value + start ;
		}

		private static float GetEaseOutCubic( float start, float end, float value )
		{
			value -- ;
			end -= start ;
			return end * ( value * value * value + 1 ) + start ;
		}

		private static float GetEaseInOutCubic( float start, float end, float value )
		{
			value /= 0.5f ;
			end -= start ;
			if( value <  1 ) return end * 0.5f * value * value * value + start ;
			value -= 2 ;
			return end * 0.5f * ( value * value * value + 2 ) + start ;
		}

		private static float GetEaseInQuart( float start, float end, float value )
		{
			end -= start ;
			return end * value * value * value * value + start ;
		}

		private static float GetEaseOutQuart( float start, float end, float value )
		{
			value -- ;
			end -= start ;
			return - end * ( value * value * value * value - 1 ) + start ;
		}

		private static float GetEaseInOutQuart( float start, float end, float value )
		{
			value /= 0.5f ;
			end -= start ;
			if( value <  1 ) return end * 0.5f * value * value * value * value + start ;
			value -= 2 ;
			return - end * 0.5f * ( value * value * value * value - 2 ) + start ;
		}

		private static float GetEaseInQuint( float start, float end, float value )
		{
			end -= start ;
			return end * value * value * value * value * value + start ;
		}
	
		private static float GetEaseOutQuint( float start, float end, float value )
		{
			value -- ;
			end -= start ;
			return end * ( value * value * value * value * value + 1 ) + start ;
		}

		private static float GetEaseInOutQuint( float start, float end, float value )
		{
			value /= 0.5f ;
			end -= start ;
			if( value <  1 ) return end * 0.5f * value * value * value * value * value + start ;
			value -= 2 ;
			return end * 0.5f * ( value * value * value * value * value + 2 ) + start ;
		}

		private static float GetEaseInSine( float start, float end, float value )
		{
			end -= start ;
			return - end * Mathf.Cos( value * ( Mathf.PI * 0.5f ) ) + end + start ;
		}

		private static float GetEaseOutSine( float start, float end, float value )
		{
			end -= start ;
			return end * Mathf.Sin( value * ( Mathf.PI * 0.5f ) ) + start ;
		}

		private static float GetEaseInOutSine( float start, float end, float value )
		{
			end -= start ;
			return - end * 0.5f * ( Mathf.Cos( Mathf.PI * value ) - 1 ) + start ;
		}

		private static float GetEaseInExpo( float start, float end, float value )
		{
			end -= start ;
			return end * Mathf.Pow( 2, 10 * ( value - 1 ) ) + start ;
		}

		private static float GetEaseOutExpo( float start, float end, float value )
		{
			end -= start ;
			return end * ( - Mathf.Pow( 2, - 10 * value ) + 1 ) + start ;
		}

		private static float GetEaseInOutExpo( float start, float end, float value )
		{
			value /= 0.5f;
			end -= start ;
			if( value <  1 ) return end * 0.5f * Mathf.Pow( 2, 10 * ( value - 1 ) ) + start ;
			value -- ;
			return end * 0.5f * ( - Mathf.Pow( 2, - 10 * value ) + 2 ) + start ;
		}

		private static float GetEaseInCirc( float start, float end, float value )
		{
			end -= start ;
			return - end * ( Mathf.Sqrt( 1 - value * value ) - 1 ) + start ;
		}

		private static float GetEaseOutCirc( float start, float end, float value )
		{
			value -- ;
			end -= start ;
			return end * Mathf.Sqrt( 1 - value * value ) + start ;
		}

		private static float GetEaseInOutCirc( float start, float end, float value )
		{
			value /= 0.5f ;
			end -= start ;
			if( value <  1 ) return - end * 0.5f * ( Mathf.Sqrt( 1 - value * value ) - 1 ) + start ;
			value -= 2 ;
			return end * 0.5f * ( Mathf.Sqrt( 1 - value * value ) + 1 ) + start ;
		}

		private static float GetLinear( float start, float end, float value )
		{
			return Mathf.Lerp( start, end, value ) ;
		}
	
		private static float GetSpring( float start, float end, float value )
		{
			value = Mathf.Clamp01( value ) ;
			value = ( Mathf.Sin( value * Mathf.PI * ( 0.2f + 2.5f * value * value * value ) ) * Mathf.Pow( 1f - value, 2.2f ) + value ) * ( 1f + ( 1.2f * ( 1f - value ) ) ) ;
			return start + ( end - start ) * value ;
		}

		private static float GetEaseInBounce( float start, float end, float value )
		{
			end -= start ;
			float d = 1f ;
			return end - GetEaseOutBounce( 0, end, d - value ) + start ;
		}
	
		private static float GetEaseOutBounce( float start, float end, float value )
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

		private static float GetEaseInOutBounce( float start, float end, float value )
		{
			end -= start ;
			float d = 1f ;
			if( value <  d * 0.5f ) return GetEaseInBounce( 0, end, value * 2 ) * 0.5f + start ;
			else return GetEaseOutBounce( 0, end, value * 2 - d ) * 0.5f + end * 0.5f + start ;
		}

		private static float GetEaseInBack( float start, float end, float value )
		{
			end -= start ;
			value /= 1 ;
			float s = 1.70158f ;
			return end * ( value ) * value * ( ( s + 1 ) * value - s ) + start ;
		}

		private static float GetEaseOutBack( float start, float end, float value )
		{
			float s = 1.70158f ;
			end -= start ;
			value = ( value ) - 1 ;
			return end * ( ( value ) * value * ( ( s + 1 ) * value + s ) + 1 ) + start ;
		}

		private static float GetEaseOutBackSharp( float start, float end, float value )
		{
			value *= value ;
			float s = 1.70158f ;
			end -= start ;
			value = ( value ) - 1 ;
			return end * ( ( value ) * value * ( ( s + 1 ) * value + s ) + 1 ) + start ;
		}

		private static float GetEaseInOutBack( float start, float end, float value )
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

		private static float GetEaseInElastic( float start, float end, float value )
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

		private static float GetEaseOutElastic( float start, float end, float value )
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

		private static float GetEaseInOutElastic( float start, float end, float value )
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
		private static float GetPunch( float amplitude, float value )
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

		private static float GetClerp( float start, float end, float value )
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
		//--------------------------------------------------------------------------

		[SerializeField][HideInInspector]
		private Vector3	m_InitialPosition ;

		[SerializeField][HideInInspector]
		private Vector3	m_InitialRotation ;

		[SerializeField][HideInInspector]
		private Vector3	m_InitialScale ;

		[SerializeField][HideInInspector]
		private float	m_InitialAlpha ;

		//-------------------------------------------------

		[SerializeField][HideInInspector]
		private bool m_IsChecker = false ;
		public  bool   IsChecker
		{
			get
			{
				return m_IsChecker ;
			}
			set
			{
				if( m_IsChecker != value )
				{
					m_IsChecker = value ;
					if( m_IsChecker == true )
					{
						SaveState() ;
						SetCheckFactor( m_CheckFactor, m_InitialPosition, m_InitialRotation, m_InitialScale, m_InitialAlpha ) ;
					}
					else
					{
						LoadState() ;
					}
				}
			}
		}

		private void RefreshChecker()
		{
			if( Application.isPlaying == true )
			{
				return ;	// 実行中は無効
			}

			if( m_IsChecker == true )
			{
				LoadState() ;
				SetCheckFactor( m_CheckFactor, m_InitialPosition, m_InitialRotation, m_InitialScale, m_InitialAlpha ) ;
			}
		}
	
		// ただの位置表示用
		[SerializeField][HideInInspector]
		private float m_CheckFactor = 0 ;
		public  float   CheckFactor
		{
			get
			{
				return m_CheckFactor ;
			}
			set
			{
				if( m_CheckFactor != value )
				{
					m_CheckFactor = value ;
					if( m_IsChecker == true )
					{
						SetCheckFactor( m_CheckFactor, m_InitialPosition, m_InitialRotation, m_InitialScale, m_InitialAlpha ) ;
					}
				}
			}
		}

		// モーションチェック専用で現在の状態を退避する
		public void SaveState()
		{
			RectTransform rectTransform = gameObject.GetComponent<RectTransform>() ;
			if( rectTransform != null )
			{
				m_InitialPosition = rectTransform.anchoredPosition ;
				m_InitialRotation = rectTransform.localEulerAngles ;
				m_InitialScale    = rectTransform.localScale ;
			}

			CanvasGroup canvasGroup = gameObject.GetComponent<CanvasGroup>() ;
			if( canvasGroup != null )
			{
				m_InitialAlpha = canvasGroup.alpha ;
			}
		}

		// モーションチェック専用で現在の状態を復帰する
		private void LoadState()
		{
			RectTransform rectTransform = gameObject.GetComponent<RectTransform>() ;
			if( rectTransform != null )
			{
				rectTransform.anchoredPosition = m_InitialPosition ;
				rectTransform.localEulerAngles = m_InitialRotation ;
				rectTransform.localScale       = m_InitialScale ;
			}

			CanvasGroup canvasGroup = gameObject.GetComponent<CanvasGroup>() ;
			if( canvasGroup != null )
			{
				canvasGroup.alpha = m_InitialAlpha ;
			}
		}

		// デバッグ用
		private void SetCheckFactor( float checkFactor, Vector3 initialPosition, Vector3 initialRotation, Vector3 initialScale, float initialAlpha )
		{
			m_RectTransform	= gameObject.GetComponent<RectTransform>() ;
			m_CanvasGroup	= gameObject.GetComponent<CanvasGroup>() ;

			//-----------------

			m_BasePosition		= initialPosition ;
			m_BaseRotation		= initialRotation ;
			m_BaseScale			= initialScale ;

			m_BaseAlpha			= initialAlpha ;

			//-----------------

			Modify( checkFactor, false ) ;
		}
	}
}

