using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.Events ;
using UnityEngine.EventSystems ;

using TMPro ;

#if UNITY_EDITOR
using UnityEditor ;
#endif

namespace uGUIHelper
{
	[ RequireComponent( typeof( RectTransform ) ) ]
	
	[ ExecuteInEditMode ]

	/// <summary>
	/// uGUIの使い勝手を向上させるヘルパークラス(基底クラス)
	/// </summary>
	public class UIView : UIBehaviour
	{
		public const string Version = "Version 2022/06/06  0" ;
		// ソースコード
		// https://bitbucket.org/Unity-Technologies/ui/src/2019.1/


		// SerializeField のリネーム方法
		// http://docs.unity3d.com/jp/current/ScriptReference/Serialization.FormerlySerializedAsAttribute.html
		// https://qiita.com/iwashihead/items/ab1b4f0363e07b32eaef

/*
#if UNITY_EDITOR
		[MenuItem( "Tools/UIView/FieldRefactor" )]
		private static void FieldRefactor()
		{
			int c = 0 ;
			UIView[] views = UIEditorUtility.FindComponents<UIView>
			(
				"Assets/Application",
				( _ ) =>
				{
//					_.m_MaterialType = ( MaterialTypes )_.m_MaterialType_ ;
					c ++ ;
				}
			) ;
			Debug.LogWarning( "------> UIViewの数:" + c ) ;
		}
#endif
*/


	//	[Tooltip("識別子")]

		/// <summary>
		/// View の識別子
		/// </summary>
		public string Identity = null ;
		
		/// <summary>
		/// View の識別子または名前を返す
		/// </summary>
		public string IdentityOrName
		{
			get
			{
				if( string.IsNullOrEmpty( Identity )  == false )
				{
					return Identity ;
				}

				return name ;
			}
		}

		/// <summary>
		/// 任意のオブジェクトを保持する(実行時のみ有効)
		/// </summary>
		[NonSerialized]
		public System.Object AnyObject ;

		/// <summary>
		/// キャンバスグループのアルファ値が指定値未満の場合はレイキャストを無効化する
		/// </summary>
		[SerializeField]
		protected float m_DisableRaycastUnderAlpha = 0.0f ;
		public float DisableRaycastUnderAlpha{ get{ return m_DisableRaycastUnderAlpha ; } set{ m_DisableRaycastUnderAlpha = value ; } }

		//-----------------------------------

		/// <summary>
		/// 子オブジェクトへの色の適用
		/// </summary>
		public    bool			  IsApplyColorToChildren{ get{ return m_IsApplyColorToChildren ; } set{ m_IsApplyColorToChildren = value ; } }

		[SerializeField]
		protected bool			m_IsApplyColorToChildren = false ;

		/// <summary>
		/// 子を含めた乗算色
		/// </summary>
		public	  Color			  EffectiveColor
		{
			get
			{
				return m_EffectiveColor ;
			}
			set
			{
				if( m_EffectiveColor.r != value.r || m_EffectiveColor.g != value.g || m_EffectiveColor.b != value.b || m_EffectiveColor.a != value.a )
				{
					m_EffectiveColor = value ;

					if( m_IsApplyColorToChildren == true && this.GetType() != typeof( UIButton ) )
					{
						ApplyColorToChidren( m_EffectiveColor, true ) ;
					}
				}
			}
		}

		[SerializeField]
		protected Color			m_EffectiveColor = Color.white ;

		protected bool			m_RefreshChildrenColor = true ;
		protected Color			m_PreviousColor ;

		//-------------------------------------------------------------------------------------------

		// 画面表示状態
		private bool m_Visible = true ;	// 自身を含めた子を画面に表示するか

		/// <summary>
		/// 画面表示状態
		/// </summary>
		public bool Visible
		{
			get
			{
				return m_Visible ;
			}
		}

		/// <summary>
		/// 表示状態を設定する
		/// </summary>
		/// <param name="state"></param>
		public void SetVisible( bool state )
		{
			if( state == false )
			{
				Hide() ;
			}
			else
			{
				Show() ;
			}
		}

		/// <summary>
		/// 自身を含めて子を非表示にする
		/// </summary>
		public void Hide()
		{
			m_Visible = false ;

			if( GetCanvasGroup() == null )
			{
				AddCanvasGroup() ;
			}

			Alpha = m_LocalAlpha ;
		}

		public void Show()
		{
			m_Visible = true ;

			if( GetCanvasGroup() != null )
			{
				Alpha = m_LocalAlpha ;
			}
		}

		override protected void OnDisable()
		{
			base.OnDisable() ;

			m_HoverAtFirst = false ;	// 非アクティブになったら一度ホバーフラグはクリアする

			m_SingleClickCheck = false ;	// スマートクリックのシングルクリックを計測していたなら無効化

			if( m_ActiveAnimations != null )
			{
				StopAllAnimators() ;
				m_ActiveAnimations.Clear() ;	// コルーチンが止まるので実行中のアニメーションはすべて停止する
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 動くものの表示確認用クラス
		/// </summary>
		public class AsyncState : CustomYieldInstruction
		{
			private readonly MonoBehaviour m_Owner = default ;
			public AsyncState( MonoBehaviour owner )
			{
				// 自身が削除された際にコルーチンの終了待ちをブレイクする施策
				m_Owner = owner ;
			}

			public override bool keepWaiting
			{
				get
				{
					if( IsDone == false && string.IsNullOrEmpty( Error ) == true && m_Owner != null && m_Owner.gameObject.activeInHierarchy == true )
					{
						return true ;    // 継続
					}
					return false ;   // 終了
				}
			}

			/// <summary>
			/// 終了したかどうか
			/// </summary>
			public bool IsDone = false ;

			/// <summary>
			/// エラーが発生したかどうか
			/// </summary>
			public string	Error = string.Empty ;

			/// <summary>
			/// 多目的保存値
			/// </summary>
			public System.Object	option ;
		}

		//-------------------------------------------------------------------------------------------
	
		override protected void Awake()
		{
			base.Awake() ;

			if( GetRectTransform() == null )
			{
				// 無ければくっつける(Imageなどは自動でAddしているので重複Addしないように注意する必要がある）
				gameObject.AddComponent<RectTransform>() ;
				ResetRectTransform() ;
			}

			this.Alpha = this.Alpha ;	// 半透明具合による CanvasRaycast の有効無効設定
		
			//-------------------------------------------------

			// オーバーライドマテリアルを設定する(プレハブの場合は Awake で毎回設定しないと、プレハブオリジナルのものが設定されてしまう)
			Graphic graphic = GetGraphic() ;

			if( m_MaterialType != MaterialTypes.Default && m_ActiveMaterial == null )
			{
				m_ActiveMaterial = CreateCustomMaterial( m_MaterialType ) ;
			}

			if( graphic != null && m_ActiveMaterial != null )
			{
				graphic.material = m_ActiveMaterial ;
				graphic.GraphicUpdateComplete() ;
			}

			// 基本的なインタラクションイベントを登録する(非アクティブの場合はStartはアクティブになるまで呼ばれないためAwakeでないとマズい)
			if( Application.isPlaying == true )
			{
				AddInteractionCallback() ;
				AddInteractionForScrollViewCallback() ;
			}

			//-------------------------------------------------

			// 継承クラスの Awake() を実行する
			OnAwake() ;
			
			// Tween などで使用する基準情報を保存する(コンポーネントの実行順が不確定なので Awake で必ず実行すること)
			SetLocalState() ;
		}
		
		// RectTransfrom を初期化する
		protected void ResetRectTransform()
		{
			RectTransform rectTransform = GetRectTransform() ;
			if( rectTransform != null )
			{
				rectTransform.anchoredPosition3D = Vector3.zero ;
				rectTransform.localScale = Vector3.one ;
				rectTransform.localRotation = Quaternion.identity ;
			}
		}

		virtual protected void OnAwake(){}

		// Tween などで使用する基準情報を保存する
		protected void SetLocalState()
		{
			LocalPosition = GetRectTransform().anchoredPosition3D ;
			LocalRotation = GetRectTransform().localEulerAngles ;
			LocalScale    = GetRectTransform().localScale ;

			if( GetCanvasGroup() != null )
			{
				LocalAlpha = GetCanvasGroup().alpha ;
			}
		}
		
		/// <summary>
		/// デフォルト設定を実行する（メニューまたは AddView から生成される場合のみ実行れる）
		/// </summary>
		public void SetDefault( string option = "" )
		{
			// レイヤーを設定
			gameObject.layer = GetCanvasTargetLayerOfFirst() ;	// ＵＩ

			// 子オブジェクトを全て破棄する
			int i, l = transform.childCount ;
			if( l >  0 )
			{
				GameObject[] childObjects = new GameObject[ l ] ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					childObjects[ i ] = transform.GetChild( i ).gameObject ;
				}
				for( i  = 0 ; i <  l ; i ++ )
				{
					Destroy( childObjects[ i ] ) ;
				}
			}

			// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
			OnBuild( option ) ;

			//-------------------------------------------------------

			// Tween などで使用する基準情報を保存する(変化している可能性があるので更新)
			SetLocalState() ;
		}
		
		// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		virtual protected void OnBuild( string option = "" ){}

		override protected void Start()
		{
			base.Start() ;

			OnStart() ;
		}

		virtual protected void OnStart(){}

		internal void Update()
		{
#if UNITY_EDITOR
	
			if( Application.isPlaying == false )
			{
				bool tweenChecker = false ;
				UITween[] tweens = GetComponents<UITween>() ;
				if( tweens != null && tweens.Length >  0 )
				{
					for( int i  = 0 ; i <  tweens.Length ; i++ )
					{
						if( tweens[ i ].IsChecker == true )
						{
							tweenChecker = true ;
							break ;
						}
					}
				}

				if( tweenChecker == false )
				{
					// ３つの値が異なっていれば更新する
					if( GetRectTransform() != null )
					{
						if( m_LocalPosition != GetRectTransform().anchoredPosition3D )
						{
							m_LocalPosition  = GetRectTransform().anchoredPosition3D ;
						}
						if( m_LocalRotation != GetRectTransform().localEulerAngles )
						{
							m_LocalRotation  = GetRectTransform().localEulerAngles ;
						}
						if( m_LocalScale != GetRectTransform().localScale )
						{
							m_LocalScale  = GetRectTransform().localScale ;
						}
					}
					if( GetCanvasGroup() != null )
					{
						if( m_LocalAlpha != GetCanvasGroup().alpha )
						{
							m_LocalAlpha  = GetCanvasGroup().alpha ;
						}
					}
				}
			}

			RemoveComponents() ;
#endif
			//----------------------------------------------------------

			SingleClickCheckProcess() ;

			if( OnPinchAction != null || OnPinchDelegate != null || OnTouchAction != null || OnTouchDelegate != null )
			{
				ProcessMultiTouch() ;
			}

			if( OnLongPressAction != null || OnLongPressDelegate != null )
			{
				OnLongPressInner() ;
			}

			if( OnRepeatPressAction != null || OnRepeatPressDelegate != null )
			{
				OnRepeatPressInner( 1 ) ;
			}

			if( m_Click == true && m_ClickCountTime != Time.frameCount )
			{
				m_Click  = false ;
			}

			//----------------------------------------------------------

			// 子への色反映(UIButton の場合は別処理を行うのでここでは処理しない)
			if( m_IsApplyColorToChildren == true && this.GetType() != typeof( UIButton ) )
			{
				ApplyColorToChidren( m_EffectiveColor, true ) ;
			}

			//----------------------------------------------------------

			OnUpdate() ;

			//----------------------------------------------------------
		}

		/// <summary>
		/// 子のオブジェクトに親のオブジェクトの色を適用
		/// </summary>
		public void ApplyColorToChidren( Color color, bool withMyself = false )
		{
			if( m_RefreshChildrenColor == false && ( m_PreviousColor.r == color.r && m_PreviousColor.g == color.g && m_PreviousColor.b == color.b && m_PreviousColor.a == color.a ) )
			{
				// 設定済みの色に変化無し
				return ;	// 何もしない
			}

			//----------------------------------

			// 子 GameObject 群の CanvasRenderer を取得する
			List<CanvasRenderer> targets ;

			if( withMyself == false )
			{
				// 自身を含めない
				targets = GetComponentsInChildren<CanvasRenderer>().Where( _ => _.gameObject != gameObject ).ToList() ;
			}
			else
			{
				// 自身を含める
				targets = GetComponentsInChildren<CanvasRenderer>().ToList() ;
			}

			if( targets != null && targets.Count >  0 )
			{
				targets.ForEach( _ =>
				{
					_.SetColor( color ) ;
				} ) ;
			}

			//----------------------------------

			// 現在の色を保存する
			m_PreviousColor.r = color.r ;
			m_PreviousColor.g = color.g ;
			m_PreviousColor.b = color.b ;
			m_PreviousColor.a = color.a ;

			// 更新済み
			m_RefreshChildrenColor = false ;
		}

#if UNITY_EDITOR
		// コンポーネントの削除
		private void RemoveComponents()
		{
			if( m_RemoveCanvasRenderer == true )
			{
				RemoveCanvasRenderer() ;
				m_RemoveCanvasRenderer = false ;
			}
		
			if( m_RemoveGraphicEmpty == true )
			{
				RemoveGraphicEmpty() ;
				m_RemoveGraphicEmpty = false ;
			}

			if( m_RemoveCanvasGroup == true )
			{
				RemoveCanvasGroup() ;
				m_RemoveCanvasGroup = false ;
			}
		
			if( m_RemoveEventTrigger == true )
			{
				RemoveEventTrigger() ;
				m_RemoveEventTrigger = false ;
			}
		
			if( m_RemoveInteraction == true )
			{
				RemoveInteraction() ;
				m_RemoveInteraction = false ;
			}
		
			if( m_RemoveInteractionForScrollView == true )
			{
				RemoveInteractionForScrollView() ;
				m_RemoveInteractionForScrollView = false ;
			}
		
			if( m_RemoveTransition == true )
			{
				RemoveTransition() ;
				m_RemoveTransition = false ;
			}
		
			if( m_RemoveTransitionForScrollView == true )
			{
				RemoveTransitionForScrollView() ;
				m_RemoveTransitionForScrollView = false ;
			}
		
			if( m_RemoveMask == true )
			{
				RemoveMask() ;
				m_RemoveMask = false ;
			}
		
			if( m_RemoveRectMask2D == true )
			{
				RemoveRectMask2D() ;
				m_RemoveRectMask2D = false ;
			}
		
			if( m_RemoveAlphaMaskWindow == true )
			{
				RemoveAlphaMaskWindow() ;
				m_RemoveAlphaMaskWindow = false ;
			}
		
			if( m_RemoveAlphaMaskTarget == true )
			{
				RemoveAlphaMaskTarget() ;
				m_RemoveAlphaMaskTarget = false ;
			}

			if( m_RemoveHorizontalLayoutGroup == true )
			{
				RemoveHorizontalLayoutGroup() ;
				m_RemoveHorizontalLayoutGroup = false ;
			}

			if( m_RemoveVerticalLayoutGroup == true )
			{
				RemoveVerticalLayoutGroup() ;
				m_RemoveVerticalLayoutGroup = false ;
			}

			if( m_RemoveGridLayoutGroup == true )
			{
				RemoveGridLayoutGroup() ;
				m_RemoveGridLayoutGroup = false ;
			}

			if( m_RemoveContentSizeFitter == true )
			{
				RemoveContentSizeFitter() ;
				m_RemoveContentSizeFitter = false ;
			}

			if( m_RemoveLayoutElement == true )
			{
				RemoveLayoutElement() ;
				m_RemoveLayoutElement = false ;
			}

			if( m_RemoveShadow == true )
			{
				RemoveShadow() ;
				m_RemoveShadow = false ;
			}

			if( m_RemoveOutline == true )
			{
				RemoveOutline() ;
				m_RemoveOutline = false ;
			}

			if( m_RemoveGradient == true )
			{
				RemoveGradient() ;
				m_RemoveGradient = false ;
			}

			if( m_RemoveInversion == true )
			{
				RemoveInversion() ;
				m_RemoveInversion = false ;
			}

			if( m_RemoveButtonGroup == true )
			{
				RemoveButtonGroup() ;
				m_RemoveButtonGroup = false ;
			}

			if( m_RemoveToggleGroup == true )
			{
				RemoveToggleGroup() ;
				m_RemoveToggleGroup = false ;
			}

			if( m_RemoveAnimator == true )
			{
				RemoveAnimator() ;
				m_RemoveAnimator = false ;
			}

			if( string.IsNullOrEmpty( m_RemoveTweenIdentity ) == false && m_RemoveTweenInstance != 0 )
			{
				RemoveTween( m_RemoveTweenIdentity, m_RemoveTweenInstance ) ;
				m_RemoveTweenIdentity = null ;
				m_RemoveTweenInstance = 0 ;
			}

			if( string.IsNullOrEmpty( m_RemoveFlipperIdentity ) == false && m_RemoveFlipperInstance != 0 )
			{
				RemoveFlipper( m_RemoveFlipperIdentity, m_RemoveFlipperInstance ) ;
				m_RemoveFlipperIdentity = null ;
				m_RemoveFlipperInstance = 0 ;
			}
		}
#endif


		virtual protected void OnUpdate(){}

		internal void LateUpdate()
		{
			OnLateUpdate() ;

			if( m_MaterialType == MaterialTypes.Sepia )
			{
				ProcessSepia() ;
			}
			else
			if( m_MaterialType == MaterialTypes.Interpolation )
			{
				ProcessInterpolation() ;
			}
			else
			if( m_MaterialType == MaterialTypes.Mosaic )
			{
				ProcessMosaic() ;
			}
		}

		virtual protected void OnLateUpdate(){}

		//------------------------------------------------------------------

		[SerializeField][HideInInspector]
		private Vector3 m_LocalPosition = Vector3.zero ;
		public  Vector3   LocalPosition
		{
			get
			{
				return m_LocalPosition ;
			}
			set
			{
				m_LocalPosition = value ;
			}
		}

		public Vector3 Position
		{
			get
			{
				if( Application.isPlaying == true )
				{
					return m_LocalPosition ;
				}
				else
				{
					RectTransform rectTransform = GetRectTransform() ;
					if( rectTransform == null )
					{
						return Vector3.zero ;
					}
					return rectTransform.anchoredPosition3D ;
				}
			}
			set
			{
				m_LocalPosition = value ;

				RectTransform rectTransform = GetRectTransform() ;
				if( rectTransform == null )
				{
					return ;
				}
				rectTransform.anchoredPosition3D = value ;
			}
		}

		/// <summary>
		/// ロケーションを設定
		/// </summary>
		/// <param name="position"></param>
		public void SetPosition( Vector2 position )
		{
			Position = new Vector3( position.x, position.y, Position.z ) ;
		}


		/// <summary>
		/// ロケーションを設定
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void SetPosition( float x, float y )
		{
			Position = new Vector3( x, y, Position.z ) ;
		}

		/// <summary>
		/// ロケーションを設定
		/// </summary>
		/// <param name="position"></param>
		public void SetPositionX( float x )
		{
			Position = new Vector3( x, Position.y, Position.z ) ;
		}

		/// <summary>
		/// ロケーションを設定
		/// </summary>
		/// <param name="position"></param>
		public void SetPositionY( float y )
		{
			Position = new Vector3( Position.x, y, Position.z ) ;
		}

		/// <summary>
		/// ロケーションを設定
		/// </summary>
		/// <param name="position"></param>
		public void SetPositionZ( float z )
		{
			Position = new Vector3( Position.x, Position.y, z ) ;
		}
		
		/// <summary>
		/// Ｘ座標(ショートカット)
		/// </summary>
		public float Px
		{
			get
			{
				return Position.x ;
			}
			set
			{
				Position = new Vector3( value, Position.y, Position.z ) ;
			}
		}

		public float Rx
		{
			get
			{
				return GetRectTransform().anchoredPosition3D.x ;
			}
			set
			{
				GetRectTransform().anchoredPosition3D = new Vector3( value, GetRectTransform().anchoredPosition3D.y, GetRectTransform().anchoredPosition3D.z ) ;
				m_LocalPosition = GetRectTransform().anchoredPosition3D ;
			}
		}
		
		/// <summary>
		/// Ｙ座標(ショートカット)
		/// </summary>
		public float Py
		{
			get
			{
				return Position.y ;
			}
			set
			{
				Position = new Vector3( Position.x, value, Position.z ) ;
			}
		}

		public float Ry
		{
			get
			{
				return GetRectTransform().anchoredPosition3D.y ;
			}
			set
			{
				GetRectTransform().anchoredPosition3D = new Vector3( GetRectTransform().anchoredPosition3D.x, value, GetRectTransform().anchoredPosition3D.z ) ;
				m_LocalPosition = GetRectTransform().anchoredPosition3D ;
			}
		}
		
	
		/// <summary>
		/// Ｚ座標(ショートカット)
		/// </summary>
		public float Pz
		{
			get
			{
				return Position.z ;
			}
			set
			{
				Position = new Vector3( Position.x, Position.y, value ) ;
			}
		}
		
		public float Rz
		{
			get
			{
				return GetRectTransform().anchoredPosition3D.z ;
			}
			set
			{
				GetRectTransform().anchoredPosition3D = new Vector3( GetRectTransform().anchoredPosition3D.x, GetRectTransform().anchoredPosition3D.y, value ) ;
				m_LocalPosition = GetRectTransform().anchoredPosition3D ;
			}
		}

		/// <summary>
		/// 基準ポジションを更新する
		/// </summary>
		public void RefreshPosition()
		{
			m_LocalPosition = GetRectTransform().anchoredPosition3D ;
		}

		/// <summary>
		/// サイズ(ショートカット)
		/// </summary>
		virtual public Vector2 Size
		{
			get
			{
				RectTransform rectTransform = GetRectTransform() ;
				if( rectTransform == null )
				{
					return Vector2.zero ;
				}
				Vector2 size = rectTransform.sizeDelta ;

				// Ｘのチェック
				if( GetRectTransform().anchorMin.x != GetRectTransform().anchorMax.x )
				{
					// Stretch なので Stretch じゃなくなるまで親をたどる
					List<RectTransform> hierarchyRects = new List<RectTransform>()
					{
						GetRectTransform()
					} ;

					Transform parentRect = GetRectTransform().parent ;
					Canvas canvas ;
					Vector2 canvasSize ;
					RectTransform targetRect ;
					float delta = 0 ;
					for( int i  = 0 ; i <  64 ; i ++ )
					{
						if( parentRect != null )
						{
							canvas = parentRect.GetComponent<Canvas>() ;
							if( canvas != null )
							{
								// キャンバスに到達した
								canvasSize = GetCanvasSize() ;	// 正確な値を取得する
								delta = canvasSize.x ;
								break ;
							}
							else
							{
								targetRect = parentRect.GetComponent<RectTransform>() ;
								if( targetRect != null )
								{
									if( targetRect.anchorMin.x == targetRect.anchorMax.x  )
									{
										// 発見
										delta = targetRect.sizeDelta.x ;
										break ;
									}
									else
									{
										hierarchyRects.Add( targetRect ) ;
									}
								}
								parentRect = parentRect.parent ;
							}
						}
						else
						{
							// 検索終了
							break ;
						}
					}

					if( delta >   0 )
					{
						// マージン分を引く
						for( int i  = hierarchyRects.Count - 1 ; i >= 0 ; i -- )
						{
							delta *= ( hierarchyRects[ i ].anchorMax.x - hierarchyRects[ i ].anchorMin.x ) ;
							delta += hierarchyRects[ i ].sizeDelta.x ;
						}

						size.x = delta ;
					}
				}

				// Ｙのチェック
				if( GetRectTransform().anchorMin.y != GetRectTransform().anchorMax.y )
				{
					// Stretch なので Stretch じゃなくなるまで親をたどる
					List<RectTransform> hierarchyRects = new List<RectTransform>()
					{
						GetRectTransform()
					} ;

					Transform parentRect = GetRectTransform().parent ;
					Canvas canvas ;
					Vector2 canvasSize ;
					RectTransform targetRect ;
					float delta = 0 ;
					for( int i  = 0 ; i <  64 ; i ++ )
					{
						if( parentRect != null )
						{
							canvas = parentRect.GetComponent<Canvas>() ;
							if( canvas != null )
							{
								// キャンバスに到達した
								canvasSize = GetCanvasSize() ;	// 正確な値を取得する
								delta = canvasSize.y ;
								break ;
							}
							else
							{
								targetRect = parentRect.GetComponent<RectTransform>() ;
								if( targetRect != null )
								{
									if( targetRect.anchorMin.y == targetRect.anchorMax.y )
									{
										// 発見
										delta = targetRect.sizeDelta.y ;
										break ;
									}
									else
									{
										hierarchyRects.Add( targetRect ) ;
									}
								}
								parentRect = parentRect.parent ;
							}
						}
						else
						{
							// 検索終了
							break ;
						}
					}

					if( delta >   0 )
					{
						// マージン分を引く
						for( int i  = hierarchyRects.Count - 1 ; i >= 0 ; i -- )
						{
							delta *= ( hierarchyRects[ i ].anchorMax.y - hierarchyRects[ i ].anchorMin.y ) ;
							delta += hierarchyRects[ i ].sizeDelta.y ;
						}

						size.y = delta ;
					}
				}

				return size ;
			}
			set
			{
				RectTransform rectTransform = GetRectTransform() ;
				if( rectTransform == null )
				{
					return ;
				}

				Vector2 size = rectTransform.sizeDelta ;
				if( rectTransform.anchorMin.x == rectTransform.anchorMax.x )
				{
					size.x = value.x ;
				}
				if( rectTransform.anchorMin.y == rectTransform.anchorMax.y )
				{
					size.y = value.y ;
				}
				rectTransform.sizeDelta = size ;
			}
		}

		/// <summary>
		/// 現在のピボットから矩形の中心への相対的な距離
		/// </summary>
		public Vector3 Center
		{
			get
			{
				Vector2 size = Size ;
				Vector2 pivot = Pivot ;

				return new Vector3( size.x * ( 0.5f - pivot.x ), size.y * ( 0.5f - pivot.y ), 0 ) ;
			}
		}

		/// <summary>
		/// キャンバス上での座標を取得する(ＵＩの中心)
		/// </summary>
		public Vector2 CenterPositionInCanvas
		{
			get
			{
				Vector2 position = PositionInCanvas ;

				float w = Width ;
				float h = Height ;
				float cx = position.x + ( 0.5f - Pivot.x ) * w ;
				float cy = position.y + ( 0.5f - Pivot.y ) * h ;

				return new Vector2( cx, cy ) ;
			}
		}

		/// <summary>
		/// キャンバス上での座標を取得する(ＵＩのピボット)
		/// </summary>
		public Vector2 PositionInCanvas
		{
			get
			{
				// 親サイズ
				Vector2 ps = GetCanvasSize() ;

//				Debug.LogWarning( "キャンバスの大きさ:" + ps ) ;

				List<RectTransform> hierarchyRects = new List<RectTransform>() ;
				int i, l ;

				Transform t = transform ;
				RectTransform rt ;

				// まずはキャンバスを検出するまでリストに格納する
				for( i  =  0 ; i <  64 ; i ++ )
				{
					if( t != null )
					{
						if( t.GetComponent<Canvas>() == null )
						{
							rt = t.GetComponent<RectTransform>() ;
							if( rt != null )
							{
								hierarchyRects.Add( rt ) ;
							}
						}
						else
						{
							break ;	// 終了
						}
					}
					else
					{
						break ;	// 終了
					}

					t = t.parent ;
				}

				if( hierarchyRects.Count <= 0 )
				{
					return Vector2.zero ;	// 異常
				}

				float pw = ps.x ;
				float ph = ps.y ;

				float px  = pw * 0.5f ;
				float px0 = 0, px1 ;
//				float px1 = pw ;

				float py  = ph * 0.5f ;
				float py0 = 0, py1 ;
//				float py1 = ph ;

				l = hierarchyRects.Count ;
//				Debug.LogWarning( "階層の数:" + l ) ;
				for( i  = ( l - 1 ) ; i >= 0 ; i -- )
				{
					rt = hierarchyRects[ i ] ;

					// X

					// 自身の横幅(次の親の横幅)
					if( rt.anchorMin.x != rt.anchorMax.x )
					{
						px0 += ( pw * rt.anchorMin.x ) ;	// 親の最小
						px1  = px0 + ( pw * rt.anchorMax.x ) ;	// 親の最大
						
						// マージンの補正をかける
						px0 -= ( ( rt.sizeDelta.x *       rt.pivot.x   ) - rt.anchoredPosition.x ) ;
						px1 += ( ( rt.sizeDelta.x * ( 1 - rt.pivot.x ) ) + rt.anchoredPosition.x ) ;

						pw = px1 - px0 ;

//						Debug.Log( "親のX:" + px0 + " ～ " + px1 + " / " + pw ) ;

						// 中心位置
						px = px0 + ( pw * rt.pivot.x ) /* - ( ( rt.sizeDelta.x * rt.pivot.x ) - rt.anchoredPosition.x ) */ ;
					}
					else
					{
						// 中心位置
//						Debug.Log( "親のX:" + px0 + " ～ " + px1 ) ;
						px = px0 + ( pw * rt.anchorMin.x ) + rt.anchoredPosition.x ;

						pw = rt.sizeDelta.x ;
					}

					// 親の範囲更新
					px0 = px - ( pw * rt.pivot.x ) ;
//					px1 = px0 + pw ;

//					Debug.Log( "x:" + px ) ;

					// Y
					// 自身の横幅(次の親の横幅)
					if( rt.anchorMin.y != rt.anchorMax.y )
					{
						py0 += ( ph * rt.anchorMin.y ) ;	// 親の最小
						py1  = py0 + ( ph * rt.anchorMax.y ) ;	// 親の最大
						
						// マージンの補正をかける
						py0 -= ( ( rt.sizeDelta.y *       rt.pivot.y   ) - rt.anchoredPosition.y ) ;
						py1 += ( ( rt.sizeDelta.y * ( 1 - rt.pivot.y ) ) + rt.anchoredPosition.y ) ;

						ph = py1 - py0 ;

//						Debug.Log( "親のY:" + py0 + " ～ " + py1 + " / " + ph ) ;

						// 中心位置
						py = py0 + ( ph * rt.pivot.y ) /* - ( ( rt.sizeDelta.x * rt.pivot.x ) - rt.anchoredPosition.x ) */ ;
					}
					else
					{
//						Debug.Log( "親のY:" + py0 + " ～ " + py1 + " / " + py ) ;

						// 中心位置
						py = py0 + ( ph * rt.anchorMin.y ) + rt.anchoredPosition.y ;

						ph = rt.sizeDelta.y ;
					}

					// 親の範囲更新
					py0 = py - ( ph * rt.pivot.y ) ;
//					py1 = py0 + ph ;

//					Debug.Log( "y:" + py ) ;
				}

				// 画面の中心基準
				px -= ( ps.x * 0.5f ) ;
				py -= ( ps.y * 0.5f ) ;

				return new Vector2( px, py ) ;
			}
		}

		/// <summary>
		/// キャンバス上での座標を取得する
		/// </summary>
		public Rect RectInCanvas
		{
			get
			{
				// 親サイズ
				Vector2 ps = GetCanvasSize() ;

//				Debug.LogWarning( "キャンバスの大きさ:" + ps ) ;

				List<RectTransform> hierarchyRects = new List<RectTransform>() ;
				int i, l ;

				Transform t = transform ;
				RectTransform rt ;

				// まずはキャンバスを検出するまでリストに格納する
				for( i  =  0 ; i <  64 ; i ++ )
				{
					if( t != null )
					{
						if( t.GetComponent<Canvas>() == null )
						{
							rt = t.GetComponent<RectTransform>() ;
							if( rt != null )
							{
								hierarchyRects.Add( rt ) ;
							}
						}
						else
						{
							break ;	// 終了
						}
					}
					else
					{
						break ;	// 終了
					}

					t = t.parent ;
				}

				if( hierarchyRects.Count <= 0 )
				{
					return new Rect() ;	// 異常
				}

				float pw = ps.x ;
				float ph = ps.y ;

				float px  = pw * 0.5f ;
				float px0 = 0, px1 ;
//				float px1 = pw ;

				float py  = ph * 0.5f ;
				float py0 = 0, py1 ;
//				float py1 = ph ;

				l = hierarchyRects.Count ;
//				Debug.LogWarning( "階層の数:" + l ) ;
				for( i  = ( l - 1 ) ; i >= 0 ; i -- )
				{
					rt = hierarchyRects[ i ] ;

//					Debug.Log( rt.name ) ;

					// X

					// 自身の横幅(次の親の横幅)
					if( rt.anchorMin.x != rt.anchorMax.x )
					{
						px0 += ( pw * rt.anchorMin.x ) ;	// 親の最小
						px1 = px0 + ( pw * rt.anchorMax.x ) ;	// 親の最大
						
						// マージンの補正をかける
						px0 -= ( ( rt.sizeDelta.x *       rt.pivot.x   ) - rt.anchoredPosition.x ) ;
						px1 += ( ( rt.sizeDelta.x * ( 1 - rt.pivot.x ) ) + rt.anchoredPosition.x ) ;

						pw = px1 - px0 ;

//						Debug.Log( "親のX:" + px0 + " ～ " + px1 + " / " + pw ) ;

						// 中心位置
						px = px0 + ( pw * rt.pivot.x ) /* - ( ( rt.sizeDelta.x * rt.pivot.x ) - rt.anchoredPosition.x ) */ ;
					}
					else
					{
						// 中心位置
//						Debug.Log( "親のX:" + px0 + " ～ " + px1 ) ;
						px = px0 + ( pw * rt.anchorMin.x ) + rt.anchoredPosition.x ;

						pw = rt.sizeDelta.x ;
					}

					// 親の範囲更新
					px0 = px - ( pw * rt.pivot.x ) ;
//					px1 = px0 + pw ;

//					Debug.Log( "x:" + px ) ;

					// Y
					// 自身の横幅(次の親の横幅)
					if( rt.anchorMin.y != rt.anchorMax.y )
					{
						py0 += ( ph * rt.anchorMin.y ) ;	// 親の最小
						py1 = py0 + ( ph * rt.anchorMax.y ) ;	// 親の最大
						
						// マージンの補正をかける
						py0 -= ( ( rt.sizeDelta.y *       rt.pivot.y   ) - rt.anchoredPosition.y ) ;
						py1 += ( ( rt.sizeDelta.y * ( 1 - rt.pivot.y ) ) + rt.anchoredPosition.y ) ;

						ph = py1 - py0 ;

//						Debug.Log( "親のY:" + py0 + " ～ " + py1 + " / " + ph ) ;

						// 中心位置
						py = py0 + ( ph * rt.pivot.y ) /* - ( ( rt.sizeDelta.x * rt.pivot.x ) - rt.anchoredPosition.x ) */ ;
					}
					else
					{
//						Debug.Log( "親のY:" + py0 + " ～ " + py1 + " / " + py ) ;

						// 中心位置
						py = py0 + ( ph * rt.anchorMin.y ) + rt.anchoredPosition.y ;

						ph = rt.sizeDelta.y ;
					}

					// 親の範囲更新
					py0 = py - ( ph * rt.pivot.y ) ;
//					py1 = py0 + ph ;

//					Debug.Log( "y:" + py ) ;
				}
				
				// 画面の中心基準
				px -= ( ps.x * 0.5f ) ;
				py -= ( ps.y * 0.5f ) ;

				pw *= GetRectTransform().localScale.x ;
				ph *= GetRectTransform().localScale.y ;

				px -= ( pw * Pivot.x ) ;
				py -= ( ph * Pivot.y ) ;

				return new Rect( px, py, pw, ph ) ;
//				return new Rect( new Vector2( px, py ), new Vector2( pw, ph ) ) ;
			}
		}

		/// <summary>
		/// キャンバス上での領域を取得する
		/// </summary>
		public Rect ViewInCanvas
		{
			get
			{
				// 親サイズ
				Vector2 ps = GetCanvasSize() ;

//				Debug.LogWarning( "キャンバスの大きさ:" + ps ) ;

				List<RectTransform> hierarchyRects = new List<RectTransform>() ;
				int i, l ;

				Transform t = transform ;
				RectTransform rt ;

				// まずはキャンバスを検出するまでリストに格納する
				for( i  =  0 ; i <  64 ; i ++ )
				{
					if( t != null )
					{
						if( t.GetComponent<Canvas>() == null )
						{
							rt = t.GetComponent<RectTransform>() ;
							if( rt != null )
							{
								hierarchyRects.Add( rt ) ;
							}
						}
						else
						{
							break ;	// 終了
						}
					}
					else
					{
						break ;	// 終了
					}

					t = t.parent ;
				}

				if( hierarchyRects.Count <= 0 )
				{
					return new Rect() ;	// 異常
				}

				float pw = ps.x ;
				float ph = ps.y ;

				float px  = pw * 0.5f ;
				float px0 = 0, px1 ;
//				float px1 = pw ;

				float py  = ph * 0.5f ;
				float py0 = 0, py1 ;
//				float py1 = ph ;


				l = hierarchyRects.Count ;
//				Debug.LogWarning( "階層の数:" + l ) ;
				for( i  = ( l - 1 ) ; i >= 0 ; i -- )
				{
					rt = hierarchyRects[ i ] ;

//					Debug.Log( rt.name ) ;

					// X

					// 自身の横幅(次の親の横幅)
					if( rt.anchorMin.x != rt.anchorMax.x )
					{
						px0 += ( pw * rt.anchorMin.x ) ;	// 親の最小
						px1 = px0 + ( pw * rt.anchorMax.x ) ;	// 親の最大
						
						// マージンの補正をかける
						px0 -= ( ( rt.sizeDelta.x *       rt.pivot.x   ) - rt.anchoredPosition.x ) ;
						px1 += ( ( rt.sizeDelta.x * ( 1 - rt.pivot.x ) ) + rt.anchoredPosition.x ) ;

						pw = px1 - px0 ;

//						Debug.Log( "親のX:" + px0 + " ～ " + px1 + " / " + pw ) ;

						// 中心位置
						px = px0 + ( pw * rt.pivot.x ) /* - ( ( rt.sizeDelta.x * rt.pivot.x ) - rt.anchoredPosition.x ) */ ;
					}
					else
					{
						// 中心位置
//						Debug.Log( "親のX:" + px0 + " ～ " + px1 ) ;
						px = px0 + ( pw * rt.anchorMin.x ) + rt.anchoredPosition.x ;

						pw = rt.sizeDelta.x ;
					}

					// 親の範囲更新
					px0 = px - ( pw * rt.pivot.x ) ;
//					px1 = px0 + pw ;

//					Debug.Log( "x:" + px ) ;

					// Y
					// 自身の横幅(次の親の横幅)
					if( rt.anchorMin.y != rt.anchorMax.y )
					{
						py0 += ( ph * rt.anchorMin.y ) ;	// 親の最小
						py1 = py0 + ( ph * rt.anchorMax.y ) ;	// 親の最大
						
						// マージンの補正をかける
						py0 -= ( ( rt.sizeDelta.y *       rt.pivot.y   ) - rt.anchoredPosition.y ) ;
						py1 += ( ( rt.sizeDelta.y * ( 1 - rt.pivot.y ) ) + rt.anchoredPosition.y ) ;

						ph = py1 - py0 ;

//						Debug.Log( "親のY:" + py0 + " ～ " + py1 + " / " + ph ) ;

						// 中心位置
						py = py0 + ( ph * rt.pivot.y ) /* - ( ( rt.sizeDelta.x * rt.pivot.x ) - rt.anchoredPosition.x ) */ ;
					}
					else
					{
//						Debug.Log( "親のY:" + py0 + " ～ " + py1 + " / " + py ) ;

						// 中心位置
						py = py0 + ( ph * rt.anchorMin.y ) + rt.anchoredPosition.y ;

						ph = rt.sizeDelta.y ;
					}

					// 親の範囲更新
					py0 = py - ( ph * rt.pivot.y ) ;
//					py1 = py0 + ph ;

//					Debug.Log( "y:" + py ) ;
				}
				
				// 画面の中心基準
				px -= ( ps.x * 0.5f ) ;
				py -= ( ps.y * 0.5f ) ;

				pw *= GetRectTransform().localScale.x ;
				ph *= GetRectTransform().localScale.y ;

				px -= ( pw * Pivot.x ) ;
				py -= ( ph * Pivot.y ) ;
				
				//---------------------------------------------------------

				float vx = ( px / ps.x ) * 2.0f ;
				float vy = ( py / ps.y ) * 2.0f ;
				float vw = ( pw / ps.x ) * 2.0f ;
				float vh = ( ph / ps.y ) * 2.0f ;

				return new Rect( vx, vy, vw, vh ) ;
//				return new Rect( new Vector2( vx, vy ), new Vector2( vw, vh ) ) ;
			}
		}

		/// <summary>
		/// 特定のコンポーネントのついた GameObject を親としてその親上での位置を取得する
		/// </summary>
		public Vector2 GetPositionIn<T>() where T : Component
		{
			int i, l ;

			//----------------------------------------------------------

			UIView screenView = null ;

			List<RectTransform> hierarchyRects = new List<RectTransform>() ;

			Transform t = this.transform ;
			RectTransform rt ;

			// まずはスクリーンを検出するまでリストに格納する
			for( i  =  0 ; i <  64 ; i ++ )
			{
				if( t != null )
				{
					if( t.GetComponent<T>() == null )
					{
						rt = t.GetComponent<RectTransform>() ;
						if( rt != null )
						{
							hierarchyRects.Add( rt ) ;
						}
					}
					else
					{
						screenView = t.GetComponent<UIView>() ;
						break ;	// 終了
					}
				}
				else
				{
					break ;	// 終了
				}

				t = t.parent ;
			}

			if( screenView == null )
			{
				// 異常
				Debug.LogWarning( "Not found parent : Component = <" + typeof( T ).ToString() + "> : Path = " + this.Path ) ;

				// ScreenSizeFitter の付いたスクリーンが見つからなかったのでキャンバス上の座標を返す
				return this.CenterPositionInCanvas ;
			}

			//------------------------------------------------------------------------------------------

			// 親サイズ
			Vector2 ps = screenView.Size ;

			float pw = ps.x ;
			float ph = ps.y ;

			float px  = pw * 0.5f ;
			float px0 = 0, px1 ;
//				float px1 = pw ;

			float py  = ph * 0.5f ;
			float py0 = 0, py1 ;
//				float py1 = ph ;

			l = hierarchyRects.Count ;
//				Debug.LogWarning( "階層の数:" + l ) ;
			for( i  = ( l - 1 ) ; i >= 0 ; i -- )
			{
				rt = hierarchyRects[ i ] ;

				// X

				// 自身の横幅(次の親の横幅)
				if( rt.anchorMin.x != rt.anchorMax.x )
				{
					px0 += ( pw * rt.anchorMin.x ) ;	// 親の最小
					px1  = px0 + ( pw * rt.anchorMax.x ) ;	// 親の最大
						
					// マージンの補正をかける
					px0 -= ( ( rt.sizeDelta.x *       rt.pivot.x   ) - rt.anchoredPosition.x ) ;
					px1 += ( ( rt.sizeDelta.x * ( 1 - rt.pivot.x ) ) + rt.anchoredPosition.x ) ;

					pw = px1 - px0 ;

//						Debug.Log( "親のX:" + px0 + " ～ " + px1 + " / " + pw ) ;

					// 中心位置
					px = px0 + ( pw * rt.pivot.x ) /* - ( ( rt.sizeDelta.x * rt.pivot.x ) - rt.anchoredPosition.x ) */ ;
				}
				else
				{
					// 中心位置
//						Debug.Log( "親のX:" + px0 + " ～ " + px1 ) ;
					px = px0 + ( pw * rt.anchorMin.x ) + rt.anchoredPosition.x ;

					pw = rt.sizeDelta.x ;
				}

				// 親の範囲更新
				px0 = px - ( pw * rt.pivot.x ) ;
//					px1 = px0 + pw ;

//					Debug.Log( "x:" + px ) ;

				// Y
				// 自身の横幅(次の親の横幅)
				if( rt.anchorMin.y != rt.anchorMax.y )
				{
					py0 += ( ph * rt.anchorMin.y ) ;	// 親の最小
					py1  = py0 + ( ph * rt.anchorMax.y ) ;	// 親の最大
						
					// マージンの補正をかける
					py0 -= ( ( rt.sizeDelta.y *       rt.pivot.y   ) - rt.anchoredPosition.y ) ;
					py1 += ( ( rt.sizeDelta.y * ( 1 - rt.pivot.y ) ) + rt.anchoredPosition.y ) ;

					ph = py1 - py0 ;

//						Debug.Log( "親のY:" + py0 + " ～ " + py1 + " / " + ph ) ;

					// 中心位置
					py = py0 + ( ph * rt.pivot.y ) /* - ( ( rt.sizeDelta.x * rt.pivot.x ) - rt.anchoredPosition.x ) */ ;
				}
				else
				{
//						Debug.Log( "親のY:" + py0 + " ～ " + py1 + " / " + py ) ;

					// 中心位置
					py = py0 + ( ph * rt.anchorMin.y ) + rt.anchoredPosition.y ;

					ph = rt.sizeDelta.y ;
				}

				// 親の範囲更新
				py0 = py - ( ph * rt.pivot.y ) ;
//					py1 = py0 + ph ;

//					Debug.Log( "y:" + py ) ;
			}

			// 画面の中心基準
			px -= ( ps.x * 0.5f ) ;
			py -= ( ps.y * 0.5f ) ;

			Vector2 position = new Vector2( px, py ) ;

			//----------------------------------------------------------

			// 中心位置に補正する
			Vector2 pivot = this.Pivot ;
			float w = this.Width ;
			float h = this.Height ;

			position.x += ( ( 0.5f - pivot.x ) * w ) ;
			position.y += ( ( 0.5f - pivot.y ) * h ) ;

			return position ;
		}

		/// <summary>
		/// サイズを設定
		/// </summary>
		/// <param name="tSize"></param>
		public void SetSize( Vector2 size )
		{
			Size = size ;
		}

		/// <summary>
		/// サイズを設定
		/// </summary>
		/// <param name="w"></param>
		/// <param name="h"></param>
		public void SetSize( float w, float h )
		{
			Size = new Vector2( w, h ) ;
		}

		/// <summary>
		/// 横幅(ショートカット)
		/// </summary>
		public float Width
		{
			get
			{
				return Size.x ;
			}
			set
			{
				RectTransform rectTransform = GetRectTransform() ;
				if( rectTransform != null )
				{
					SetSize( value, rectTransform.sizeDelta.y ) ;
				}
			}
		}
		
		/// <summary>
		/// 縦幅(ショートカット)
		/// </summary>
		public float Height
		{
			get
			{
				return Size.y ;
			}
			set
			{
				RectTransform rectTransform = GetRectTransform() ;
				if( rectTransform != null )
				{
					SetSize( rectTransform.sizeDelta.x, value ) ;
				}
			}
		}
		
		/// <summary>
		/// テキスト自体の横幅
		/// </summary>
		public float TextWidth
		{
			get
			{
				if( this is UIText )
				{
					UIText text = this as UIText ;
					return text.TextSize.x ;
				}
				else
				if( this is UIRichText )
				{
					UIRichText text = this as UIRichText ;
					return text.TextSize.x ;
				}
				else
				if( this is UITextMesh )
				{
					UITextMesh text = this as UITextMesh ;
					return text.TextSize.x ;
				}
				return 0 ;	
			}
		}

		/// <summary>
		/// テキスト自体の縦幅
		/// </summary>
		public float TextHeight
		{
			get
			{
				if( this is UIText )
				{
					UIText text = this as UIText ;
					return text.TextSize.y ;
				}
				else
				if( this is UIRichText )
				{
					UIRichText text = this as UIRichText ;
					return text.TextSize.y ;
				}
				else
				if( this is UITextMesh )
				{
					UITextMesh text = this as UITextMesh ;
					return text.TextSize.y ;
				}
				return 0 ;	
			}
		}

		/// <summary>
		/// テキスト自体の最大の縦幅
		/// </summary>
		public float FullTextWidth
		{
			get
			{
				if( this is UIRichText )
				{
					UIRichText text = this as UIRichText ;
					return text.FullHeight ;
				}
				return 0 ;	
			}
		}

		/// <summary>
		/// アンカー最少値(ショートカット)
		/// </summary>
		public Vector2 AnchorMin
		{
			get
			{
				RectTransform rectTransform = GetRectTransform() ;
				if( rectTransform == null )
				{
					return Vector2.zero ;
				}
				return rectTransform.anchorMin ;
			}
			set
			{
				RectTransform rectTransform = GetRectTransform() ;
				if( rectTransform == null )
				{
					return ;
				}
				rectTransform.anchorMin = value ;
			}
		}
		
		/// <summary>
		/// アンカー最少値を設定
		/// </summary>
		/// <param name="anchorMin"></param>
		public void SetAnchorMin( Vector2 anchorMin )
		{
			AnchorMin = anchorMin ;
		}
		
		/// <summary>
		/// アンカー最小値を設定
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void SetAnchorMin( float x, float y )
		{
			AnchorMin = new Vector2( x, y ) ;
		}
		
		/// <summary>
		/// アンカーＸ最小値を設定
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void SetAnchorMinX( float x )
		{
			AnchorMin = new Vector2( x, AnchorMin.y ) ;
		}
		
		/// <summary>
		/// アンカーＹ最小値を設定
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void SetAnchorMinY( float y )
		{
			AnchorMin = new Vector2( AnchorMin.x, y ) ;
		}
	
		/// <summary>
		/// アンカー最大値(ショートカット)
		/// </summary>
		public Vector2 AnchorMax
		{
			get
			{
				RectTransform rectTransform = GetRectTransform() ;
				if( rectTransform == null )
				{
					return Vector2.zero ;
				}
				return rectTransform.anchorMax ;
			}
			set
			{
				RectTransform rectTransform = GetRectTransform() ;
				if( rectTransform == null )
				{
					return ;
				}
				rectTransform.anchorMax = value ;
			}
		}
		
		/// <summary>
		/// アンカー最大値を設定
		/// </summary>
		/// <param name="anchorMax"></param>
		public void SetAnchorMax( Vector2 anchorMax )
		{
			AnchorMax = anchorMax ;
		}
		
		/// <summary>
		/// アンカー最大値を設定
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void SetAnchorMax( float x, float y )
		{
			AnchorMax = new Vector2( x, y ) ;
		}
		
		/// <summary>
		/// アンカーＸ最大値を設定
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void SetAnchorMaxX( float x )
		{
			AnchorMax = new Vector2( x, AnchorMax.y ) ;
		}
		
		/// <summary>
		/// アンカーＹ最大値を設定
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void SetAnchorMaxY( float y )
		{
			AnchorMax = new Vector2( AnchorMax.x, y ) ;
		}
		
		/// <summary>
		/// アンカー最小値・最大値を設定
		/// </summary>
		/// <param name="anchorMin"></param>
		/// <param name="anchorMax"></param>
		public void SetAnchorMinAndMax( Vector2 anchorMin, Vector2 anchorMax )
		{
			AnchorMin = anchorMin ;
			AnchorMax = anchorMax ;
		}
		
		/// <summary>
		/// アンカー最大値・最小値を設定
		/// </summary>
		/// <param name="anchorMinX"></param>
		/// <param name="anchorMinY"></param>
		/// <param name="anchorMaxX"></param>
		/// <param name="anchorMaxY"></param>
		public void SetAnchorMinAndMax( float anchorMinX, float anchorMinY, float anchorMaxX, float anchorMaxY )
		{
			AnchorMin = new Vector2( anchorMinX, anchorMinY ) ;
			AnchorMax = new Vector2( anchorMaxX, anchorMaxY ) ;
		}
		
		/// <summary>
		/// アンカー最大値(ショートカット)
		/// </summary>
		public float AnchorMinX
		{
			get
			{
				RectTransform rectTransform = GetRectTransform() ;
				if( rectTransform == null )
				{
					return 0 ;
				}
				return rectTransform.anchorMin.x ;
			}
			set
			{
				RectTransform rectTransform = GetRectTransform() ;
				if( rectTransform == null )
				{
					return ;
				}
				rectTransform.anchorMin = new Vector2( value, rectTransform.anchorMin.y ) ;
			}
		}

		/// <summary>
		/// アンカー最大値(ショートカット)
		/// </summary>
		public float AnchorMinY
		{
			get
			{
				RectTransform rectTransform = GetRectTransform() ;
				if( rectTransform == null )
				{
					return 0 ;
				}
				return rectTransform.anchorMin.y ;
			}
			set
			{
				RectTransform rectTransform = GetRectTransform() ;
				if( rectTransform == null )
				{
					return ;
				}
				rectTransform.anchorMin = new Vector2( rectTransform.anchorMin.x, value ) ;
			}
		}

		/// <summary>
		/// アンカー最大値(ショートカット)
		/// </summary>
		public float AnchorMaxX
		{
			get
			{
				RectTransform rectTransform = GetRectTransform() ;
				if( rectTransform == null )
				{
					return 0 ;
				}
				return rectTransform.anchorMax.x ;
			}
			set
			{
				RectTransform rectTransform = GetRectTransform() ;
				if( rectTransform == null )
				{
					return ;
				}
				rectTransform.anchorMax = new Vector2( value, rectTransform.anchorMax.y ) ;
			}
		}

		/// <summary>
		/// アンカー最大値(ショートカット)
		/// </summary>
		public float AnchorMaxY
		{
			get
			{
				RectTransform rectTransform = GetRectTransform() ;
				if( rectTransform == null )
				{
					return 0 ;
				}
				return rectTransform.anchorMax.y ;
			}
			set
			{
				RectTransform rectTransform = GetRectTransform() ;
				if( rectTransform == null )
				{
					return ;
				}
				rectTransform.anchorMax = new Vector2( rectTransform.anchorMax.x, value ) ;
			}
		}

		/// <summary>
		/// アンカーの値を設定する
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void SetAnchor( float x, float y )
		{
			SetAnchorX( x ) ;
			SetAnchorY( y ) ;
		}

		/// <summary>
		/// アンカーＸの値を設定する
		/// </summary>
		/// <param name="x"></param>
		public void SetAnchorX( float x )
		{
			SetAnchorX( x, x ) ;
		}

		/// <summary>
		/// アンカーのＸ値を設定する
		/// </summary>
		/// <param name="minX"></param>
		/// <param name="maxX"></param>
		public void SetAnchorX( float minX, float maxX )
		{
			Vector2 anchorMin = AnchorMin ;
			Vector2 anchorMax = AnchorMax ;

			anchorMin.x = minX ;
			anchorMax.x = maxX ;

			AnchorMin = anchorMin ;
			AnchorMax = anchorMax ;
		}

		/// <summary>
		/// アンカーＹの値を設定する
		/// </summary>
		/// <param name="x"></param>
		public void SetAnchorY( float y )
		{
			SetAnchorY( y, y ) ;
		}

		/// <summary>
		/// アンカーのＹ値を設定する
		/// </summary>
		/// <param name="minY"></param>
		/// <param name="maxY"></param>
		public void SetAnchorY( float minY, float maxY )
		{
			Vector2 anchorMin = AnchorMin ;
			Vector2 anchorMax = AnchorMax ;

			anchorMin.y = minY ;
			anchorMax.y = maxY ;

			AnchorMin = anchorMin ;
			AnchorMax = anchorMax ;
		}


		/// <summary>
		/// アンカーを位置から設定
		/// </summary>
		/// <param name="anchors"></param>
		public void SetAnchors( UIAnchors anchors )
		{
			switch( anchors )
			{
				case UIAnchors.LeftTop			: SetAnchorMinAndMax( 0.0f, 1.0f, 0.0f, 1.0f ) ; break ;
				case UIAnchors.CenterTop		: SetAnchorMinAndMax( 0.5f, 1.0f, 0.5f, 1.0f ) ; break ;
				case UIAnchors.RightTop			: SetAnchorMinAndMax( 1.0f, 1.0f, 1.0f, 1.0f ) ; break ;
				case UIAnchors.StretchTop		: SetAnchorMinAndMax( 0.0f, 1.0f, 1.0f, 1.0f ) ; break ;
			
				case UIAnchors.LeftMiddle		: SetAnchorMinAndMax( 0.0f, 0.5f, 0.0f, 0.5f ) ; break ;
				case UIAnchors.CenterMiddle		: SetAnchorMinAndMax( 0.5f, 0.5f, 0.5f, 0.5f ) ; break ;
				case UIAnchors.RightMiddle		: SetAnchorMinAndMax( 1.0f, 0.5f, 1.0f, 0.5f ) ; break ;
				case UIAnchors.StretchMiddle	: SetAnchorMinAndMax( 0.0f, 0.5f, 1.0f, 0.5f ) ; break ;
			
				case UIAnchors.LeftBottom		: SetAnchorMinAndMax( 0.0f, 0.0f, 0.0f, 0.0f ) ; break ;
				case UIAnchors.CenterBottom		: SetAnchorMinAndMax( 0.5f, 0.0f, 0.5f, 0.0f ) ; break ;
				case UIAnchors.RightBottom		: SetAnchorMinAndMax( 1.0f, 0.0f, 1.0f, 0.0f ) ; break ;
				case UIAnchors.StretchBottom	: SetAnchorMinAndMax( 0.0f, 0.0f, 1.0f, 0.0f ) ; break ;
			
				case UIAnchors.LeftStretch		: SetAnchorMinAndMax( 0.0f, 0.0f, 0.0f, 1.0f ) ; break ;
				case UIAnchors.CenterStretch	: SetAnchorMinAndMax( 0.5f, 0.0f, 0.5f, 1.0f ) ; break ;
				case UIAnchors.RightStretch		: SetAnchorMinAndMax( 1.0f, 0.0f, 1.0f, 1.0f ) ; break ;
				case UIAnchors.Stretch			: SetAnchorMinAndMax( 0.0f, 0.0f, 1.0f, 1.0f ) ; break ;
			
				case UIAnchors.Center			: SetAnchorMinAndMax( 0.5f, 0.5f, 0.5f, 0.5f ) ; break ;
			}
		}
		
		/// <summary>
		/// アンカーを左上に設定
		/// </summary>
		public void SetAnchorToLeftTop()		{ SetAnchorMinAndMax( 0.0f, 1.0f, 0.0f, 1.0f ) ; }

		/// <summary>
		/// アンカーを中上に設定
		/// </summary>
		public void SetAnchorToCenterTop()		{ SetAnchorMinAndMax( 0.5f, 1.0f, 0.5f, 1.0f ) ; }

		/// <summary>
		/// アンカーを右上に設定
		/// </summary>
		public void SetAnchorToRightTop()		{ SetAnchorMinAndMax( 1.0f, 1.0f, 1.0f, 1.0f ) ; }

		/// <summary>
		/// アンカーを全上に設定
		/// </summary>
		public void SetAnchorToStretchTop()		{ SetAnchorMinAndMax( 0.0f, 1.0f, 1.0f, 1.0f ) ; SetMarginX( 0, 0 ) ; }
		
		/// <summary>
		/// アンカーを左中に設定
		/// </summary>
		public void SetAnchorToLeftMiddle()		{ SetAnchorMinAndMax( 0.0f, 0.5f, 0.0f, 0.5f ) ; }

		/// <summary>
		/// アンカーを中中に設定
		/// </summary>
		public void SetAnchorToCenterMiddle()	{ SetAnchorMinAndMax( 0.5f, 0.5f, 0.5f, 0.5f ) ; }

		/// <summary>
		/// アンカーを右中に設定
		/// </summary>
		public void SetAnchorToRightMiddle()	{ SetAnchorMinAndMax( 1.0f, 0.5f, 1.0f, 0.5f ) ; }

		/// <summary>
		/// アンカーを全中に設定
		/// </summary>
		public void SetAnchorToStretchMiddle()	{ SetAnchorMinAndMax( 0.0f, 0.5f, 1.0f, 0.5f ) ; SetMarginX( 0, 0 ) ; }
		
		/// <summary>
		/// アンカーを左下に設定
		/// </summary>
		public void SetAnchorToLeftBottom()		{ SetAnchorMinAndMax( 0.0f, 0.0f, 0.0f, 0.0f ) ; }

		/// <summary>
		/// アンカーを中下に設定
		/// </summary>
		public void SetAnchorToCenterBottom()	{ SetAnchorMinAndMax( 0.5f, 0.0f, 0.5f, 0.0f ) ; }

		/// <summary>
		/// アンカーを右下に設定
		/// </summary>
		public void SetAnchorToRightBottom()	{ SetAnchorMinAndMax( 1.0f, 0.0f, 1.0f, 0.0f ) ; }

		/// <summary>
		/// アンカーを全下に設定
		/// </summary>
		public void SetAnchorToStretchBottom()	{ SetAnchorMinAndMax( 0.0f, 0.0f, 1.0f, 0.0f ) ; SetMarginX( 0, 0 ) ; }
		
		/// <summary>
		/// アンカーを左全に設定
		/// </summary>
		public void SetAnchorToLeftStretch()	{ SetAnchorMinAndMax( 0.0f, 0.0f, 0.0f, 1.0f ) ; SetMarginY( 0, 0 ) ; }

		/// <summary>
		/// アンカーを中全に設定
		/// </summary>
		public void SetAnchorToCenterStretch()	{ SetAnchorMinAndMax( 0.5f, 0.0f, 0.5f, 1.0f ) ; SetMarginY( 0, 0 ) ; }

		/// <summary>
		/// アンカーを右全に設定
		/// </summary>
		public void SetAnchorToRightStretch()	{ SetAnchorMinAndMax( 1.0f, 0.0f, 1.0f, 1.0f ) ; SetMarginY( 0, 0 ) ; }

		/// <summary>
		/// アンカーを全全に設定
		/// </summary>
		public void SetAnchorToStretch()		{ SetAnchorMinAndMax( 0.0f, 0.0f, 1.0f, 1.0f ) ; SetMargin( 0, 0, 0, 0 ) ; }
		
		/// <summary>
		///  アンカーを中中に設定
		/// </summary>
		public void SetAnchorToCenter()			{ SetAnchorMinAndMax( 0.5f, 0.5f, 0.5f, 0.5f ) ; }

		/// <summary>
		/// マージン
		/// </summary>
		public RectOffset Margin
		{
			get
			{
				return GetMargin() ;
			}
			set
			{
				SetMargin( value ) ;
			}
		}

		/// <summary>
		/// マージンを取得
		/// </summary>
		/// <returns></returns>
		public RectOffset GetMargin()
		{
			RectOffset margin = new RectOffset() ;

			GetMargin( out float left, out float right, out float top, out float bottom ) ;

			margin.left		= ( int )left ;
			margin.right	= ( int )right ;
			margin.top		= ( int )top ;
			margin.bottom	= ( int )bottom ;

			return margin ;
		}

		/// <summary>
		/// マージンを取得
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <param name="top"></param>
		/// <param name="bottom"></param>
		public void GetMargin( out float left, out float right, out float top, out float bottom )
		{
			left	= 0 ;
			right	= 0 ;

			top		= 0 ;
			bottom	= 0 ;

			RectTransform rectTransform = GetRectTransform() ;
			if( rectTransform != null )
			{
				if( rectTransform.anchorMin.x != rectTransform.anchorMax.x )
				{
					// 横方向はマージン設定が可能な状態
					float px = Pivot.x ;
					float x = rectTransform.anchoredPosition3D.x ;
					float w = rectTransform.sizeDelta.x ;

					right		= - x - ( w * ( 1 - px ) ) ;
					left		=   x - ( w *       px )   ;
				}

				if( rectTransform.anchorMin.y != rectTransform.anchorMax.y )
				{
					// 縦方向はマージン設定が可能な状態(座標系が Top=Right Bottom=Left なのに注意)
					float py = Pivot.y ;
					float y = rectTransform.anchoredPosition3D.y ;
					float h = rectTransform.sizeDelta.y ;

					top			= - y - ( h * ( 1 - py ) ) ;
					bottom		=   y - ( h *       py )   ;
				}
			}
		}

		/// <summary>
		/// 横方向のマージンを取得
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		public void GetMarginX( out float left, out float right )
		{
			left	= 0 ;
			right	= 0 ;

			RectTransform rectTransform = GetRectTransform() ;
			if( rectTransform != null && rectTransform.anchorMin.x != rectTransform.anchorMax.x )
			{
				// 横方向はマージン設定が可能な状態
				float px = Pivot.x ;
				float x = rectTransform.anchoredPosition3D.x ;
				float w = rectTransform.sizeDelta.x ;

				right		= - x - ( w * ( 1 - px ) ) ;
				left		=   x - ( w *       px )   ;
			}
		}

		/// <summary>
		/// 縦方向のマージンを取得
		/// </summary>
		/// <param name="top"></param>
		/// <param name="bottom"></param>
		public void GetMarginY( out float top, out float bottom )
		{
			top		= 0 ;
			bottom	= 0 ;

			RectTransform rectTransform = GetRectTransform() ;
			if( rectTransform != null && rectTransform.anchorMin.y != rectTransform.anchorMax.y )
			{
				// 縦方向はマージン設定が可能な状態(座標系が Top=Right Bottom=Left なのに注意)
				float py = Pivot.y ;
				float y = rectTransform.anchoredPosition3D.y ;
				float h = rectTransform.sizeDelta.y ;

				top			= - y - ( h * ( 1 - py ) ) ;
				bottom		=   y - ( h *       py )   ;
			}
		}

		/// <summary>
		/// マージンを設定
		/// </summary>
		/// <param name="margin"></param>
		public void SetMargin( RectOffset margin )
		{
			if( margin == null )
			{
				return ;
			}

			SetMargin( margin.left, margin.right, margin.top, margin.bottom ) ;
		}

		/// <summary>
		/// マージンを設定
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <param name="top"></param>
		/// <param name="bottom"></param>
		public void SetMargin( float left, float right, float top, float bottom )
		{
			RectTransform rectTransform = GetRectTransform() ;
			if( rectTransform != null )
			{
				float x, w ;
				float px = Pivot.x ;

				if( rectTransform.anchorMin.x != rectTransform.anchorMax.x )
				{
					// 横方向はマージン設定が可能な状態
					x = ( left * ( 1.0f - px ) ) - ( right * px ) ;
					w = - left - right ;
				}
				else
				{
					x = rectTransform.anchoredPosition3D.x ;
					w = rectTransform.sizeDelta.y ;
				}

				float y, h ;
				float py = Pivot.y ;

				if( rectTransform.anchorMin.y != rectTransform.anchorMax.y )
				{
					// 縦方向はマージン設定が可能な状態(座標系が Top=Right Bottom=Left なのに注意)
					y = ( ( bottom * ( 1.0f - py ) ) - ( top * py ) ) ;
					h = - bottom - top ;
				}
				else
				{
					y = rectTransform.anchoredPosition3D.y ;
					h = rectTransform.sizeDelta.y ;
				}

				rectTransform.anchoredPosition3D = new Vector3( x, y, rectTransform.anchoredPosition3D.z ) ;
				rectTransform.sizeDelta = new Vector2( w, h ) ;

				m_LocalPosition = rectTransform.anchoredPosition3D ;
			}
		}
		
		/// <summary>
		/// 横方向のマージンを設定
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		public void SetMarginX( float left, float right )
		{
			RectTransform rectTransform = GetRectTransform() ;
			if( rectTransform != null && rectTransform.anchorMin.x != rectTransform.anchorMax.x )
			{
				// 横方向はマージン設定が可能な状態
				float px = Pivot.x ;
				float x = ( left * ( 1.0f - px ) ) - ( right * px ) ;
				float w = - left - right ;

				rectTransform.anchoredPosition3D = new Vector3( x, rectTransform.anchoredPosition3D.y, rectTransform.anchoredPosition3D.z ) ;
				rectTransform.sizeDelta = new Vector2( w, rectTransform.sizeDelta.y ) ;

				m_LocalPosition = rectTransform.anchoredPosition3D ;
			}
		}
		
		/// <summary>
		/// 縦方向のマージンを設定
		/// </summary>
		/// <param name="top"></param>
		/// <param name="bottom"></param>
		public void SetMarginY( float top, float bottom )
		{
			RectTransform rectTransform = GetRectTransform() ;
			if( rectTransform != null && rectTransform.anchorMin.y != rectTransform.anchorMax.y )
			{
				// 縦方向はマージン設定が可能な状態(座標系が Top=Right Bottom=Left なのに注意)
				float py = Pivot.y ;
				float y = ( ( bottom * ( 1.0f - py ) ) - ( top * py ) ) ;
				float h = - bottom - top ;

				rectTransform.anchoredPosition3D = new Vector3( rectTransform.anchoredPosition3D.x, y, rectTransform.anchoredPosition3D.z ) ;
				rectTransform.sizeDelta = new Vector2( rectTransform.sizeDelta.x, h ) ;

				m_LocalPosition = rectTransform.anchoredPosition3D ;
			}
		}
	
		/// <summary>
		/// ピボット(ショートカト)
		/// </summary>
		public Vector2 Pivot
		{
			get
			{
				RectTransform rectTransform = GetRectTransform() ;
				if( rectTransform == null )
				{
					return new Vector2( 0.5f, 0.5f ) ;
				}
				return rectTransform.pivot ;
			}
			set
			{
				RectTransform rectTransform = GetRectTransform() ;
				if( rectTransform == null )
				{
					return ;
				}
				rectTransform.pivot = value ;
			}
		}
		
		/// <summary>
		/// ピボットを設定
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void SetPivot( float x, float y, bool correct = false )
		{
			if( correct == true )
			{
				// 表示位置は変化させない

				Vector2 pivot = Pivot ;
				
				float w = this.Width ;
				float dx0 = w * pivot.x ;
				float dx1 = w * x ;

				this.Rx += ( dx1 - dx0 ) ;

				float h = this.Height ;
				float dy0 = h * pivot.y ;
				float dy1 = h * y ;

				this.Ry += ( dy1 - dy0 ) ;
			}

			Pivot = new Vector2( x, y ) ;
		}
		
		/// <summary>
		/// ピボットを設定
		/// </summary>
		/// <param name="pivot"></param>
		public void SetPivot( Vector2 pivot, bool correct = false )
		{
			SetPivot( pivot.x, pivot.y, correct ) ;
		}
		
		/// <summary>
		/// 横方向のピボットを設定
		/// </summary>
		/// <param name="x"></param>
		public void SetPivotX( float x, bool correct = false )
		{
			SetPivot( x, Pivot.y, correct ) ;
		}

		/// <summary>
		/// 縦方向のピボットを設定
		/// </summary>
		/// <param name="x"></param>
		public void SetPivotY( float y, bool correct = false )
		{
			SetPivot( Pivot.x, y, correct ) ;
		}

		/// <summary>
		/// ピボットを位置から設定
		/// </summary>
		/// <param name="pivots"></param>
		public void SetPivot( UIPivots pivots, bool correct = false )
		{
			switch( pivots )
			{
				case UIPivots.LeftTop		: SetPivot( 0.0f, 1.0f, correct ) ; break ;
				case UIPivots.CenterTop		: SetPivot( 0.5f, 1.0f, correct ) ; break ;
				case UIPivots.RightTop		: SetPivot( 1.0f, 1.0f, correct ) ; break ;
			
				case UIPivots.LeftMiddle	: SetPivot( 0.0f, 0.5f, correct ) ; break ;
				case UIPivots.CenterMiddle	: SetPivot( 0.5f, 0.5f, correct ) ; break ;
				case UIPivots.RightMiddle	: SetPivot( 1.0f, 0.5f, correct ) ; break ;
			
				case UIPivots.LeftBottom	: SetPivot( 0.0f, 0.0f, correct ) ; break ;
				case UIPivots.CenterBottom	: SetPivot( 0.5f, 0.0f, correct ) ; break ;
				case UIPivots.RightBottom	: SetPivot( 1.0f, 0.0f, correct ) ; break ;
			
				case UIPivots.Center		: SetPivot( 0.5f, 0.5f, correct ) ; break ;
			}
		}
		
		/// <summary>
		/// ピボットを左上に設定
		/// </summary>
		public void SetPivotToLeftTop( bool correct		= false ){ SetPivot( 0.0f, 1.0f, correct ) ; }

		/// <summary>
		/// ピボットを中上に設定
		/// </summary>
		public void SetPivotToCenterTop( bool correct		= false ){ SetPivot( 0.5f, 1.0f, correct ) ; }

		/// <summary>
		/// ピボットを右上に設定
		/// </summary>
		public void SetPivotToRightTop( bool correct		= false ){ SetPivot( 1.0f, 1.0f, correct ) ; }
		
		/// <summary>
		/// ピボットを左中に設定
		/// </summary>
		public void SetPivotToLeftMiddle( bool correct		= false ){ SetPivot( 0.0f, 0.5f, correct ) ; }

		/// <summary>
		/// ピボットを中中に設定
		/// </summary>
		public void SetPivotToCenterMiddle( bool correct	= false ){ SetPivot( 0.5f, 0.5f, correct ) ; }

		/// <summary>
		/// ピボットを右中に設定
		/// </summary>
		public void SetPivotToRightMiddle( bool correct	= false ){ SetPivot( 1.0f, 0.5f, correct ) ; }
		
		/// <summary>
		/// ピボットを左下に設定
		/// </summary>
		public void SetPivotToLeftBottom( bool correct		= false ){ SetPivot( 0.0f, 0.0f, correct ) ; }

		/// <summary>
		/// ピボットを中下に設定
		/// </summary>
		public void SetPivotToCenterBottom( bool correct	= false ){ SetPivot( 0.5f, 0.0f, correct ) ; }

		/// <summary>
		/// ピボットを右下に設定
		/// </summary>
		public void SetPivotToRightBottom( bool correct	= false ){ SetPivot( 1.0f, 0.0f, correct ) ; }
		
		/// <summary>
		/// ピボットを中中に設定
		/// </summary>
		public void SetPivotToCenter( bool correct			= false ){ SetPivot( 0.5f, 0.5f, correct ) ; }

		//-------------------------------------------------------------------------------------------

		[SerializeField][HideInInspector]
		private Vector3 m_LocalRotation = Vector3.zero ;
		public  Vector3   LocalRotation
		{
			get
			{
				return m_LocalRotation ;
			}
			set
			{
				m_LocalRotation = value ;
			}
		}

		/// <summary>
		/// ２Ｄでの回転角度を設定する
		/// </summary>
		/// <param name="axisZ"></param>
		public void SetRotation( float axisZ )
		{
			Roll = axisZ ;
		}

		/// <summary>
		/// ３軸での回転角度を設定する
		/// </summary>
		/// <param name="axisZ"></param>
		public void SetRotation( Vector2 value )
		{
			Rotation = value ;
		}

		/// <summary>
		/// ローテーション(ショートカット)
		/// </summary>
		public Vector3 Rotation
		{
			get
			{
				if( Application.isPlaying == true )
				{
					return m_LocalRotation ;
				}
				else
				{
					RectTransform rectTransform = GetRectTransform() ;
					if( rectTransform == null )
					{
						return Vector3.zero ;
					}
					return rectTransform.localEulerAngles ;
				}
			}
			set
			{
				m_LocalRotation = value ;

				RectTransform rectTransform = GetRectTransform() ;
				if( rectTransform == null )
				{
					return ;
				}
				rectTransform.localEulerAngles = value ;
			}
		}

		public float Pitch
		{
			get
			{
				return Rotation.x ;
			}
			set
			{
				Rotation = new Vector3( value, Rotation.y, Rotation.z ) ;
			}
		}

		public float Yaw
		{
			get
			{
				return Rotation.y ;
			}
			set
			{
				Rotation = new Vector3( Rotation.x, value, Rotation.z ) ;
			}
		}

		public float Roll
		{
			get
			{
				return Rotation.z ;
			}
			set
			{
				Rotation = new Vector3( Rotation.x, Rotation.y, value ) ;
			}
		}

		/// <summary>
		/// 基準ローテーションを更新する
		/// </summary>
		public void RefreshRotation()
		{
			m_LocalRotation = GetRectTransform().localEulerAngles ;
		}

		[SerializeField][HideInInspector]
		private Vector3 m_LocalScale = Vector3.one ;
		public  Vector3   LocalScale
		{
			get
			{
				return m_LocalScale ;
			}
			set
			{
				m_LocalScale = value ;
			}
		}

		/// <summary>
		/// スケール(ショートカット)
		/// </summary>
		public Vector3 Scale
		{
			get
			{
				if( Application.isPlaying == true )
				{
					return m_LocalScale ;
				}
				else
				{
					RectTransform rectTransform = GetRectTransform() ;
					if( rectTransform == null )
					{
						return Vector3.zero ;
					}
					return rectTransform.localScale ;
				}
			}
			set
			{
				m_LocalScale = value ;

				RectTransform rectTransform = GetRectTransform() ;
				if( rectTransform == null )
				{
					return ;
				}
				rectTransform.localScale = value ;
			}
		}
		
		/// <summary>
		/// スケールを設定
		/// </summary>
		/// <param name="s"></param>
		public void SetScale( float s )
		{
			Scale = new Vector3( s, s, Scale.z ) ;
		}

		/// <summary>
		/// スケールを設定
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void SetScale( float x, float y )
		{
			Scale = new Vector3( x, y, Scale.z ) ;
		}

		/// <summary>
		/// スケールを設定
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void SetScale( float x, float y, float z )
		{
			Scale = new Vector3( x, y, z ) ;
		}

		/// <summary>
		/// スケールを設定
		/// </summary>
		/// <param name="tScale"></param>
		public void SetScale( Vector2 scale )
		{
			Scale = new Vector3( scale.x, scale.y, Scale.z ) ;
		}

		/// <summary>
		/// スケールを設定
		/// </summary>
		/// <param name="tScale"></param>
		public void SetScale( Vector3 scale )
		{
			Scale = scale ;
		}

		/// <summary>
		/// 基準スケールを更新する
		/// </summary>
		public void RefreshScale()
		{
			m_LocalScale = GetRectTransform().localScale ;
		}

		//-----------------------------------

		[SerializeField][HideInInspector]
		private float m_LocalAlpha = 1.0f ;
		public  float   LocalAlpha
		{
			get
			{
				return m_LocalAlpha ;
			}
			set
			{
				m_LocalAlpha = value ;
			}
		}

		/// <summary>
		/// アルファ値を設定する
		/// </summary>
		/// <param name="alpha"></param>
		public void SetAlpha( float alpha )
		{
			Alpha = alpha ;
		}

		/// <summary>
		/// α値
		/// </summary>
		public float Alpha
		{
			get
			{
				if( Application.isPlaying == true )
				{
					return m_LocalAlpha ;
				}
				else
				{
					CanvasGroup canvasGroup = GetCanvasGroup() ;
					if( canvasGroup == null )
					{
						return 1.0f ;
					}
					return canvasGroup.alpha ;
				}
			}
			set
			{
				m_LocalAlpha = value ;

				CanvasGroup canvasGroup = GetCanvasGroup() ;
				if( canvasGroup == null )
				{
					return ;
				}

				canvasGroup.alpha = value * ( m_Visible == true ? 1 : 0 ) ;

				if( canvasGroup.alpha <  m_DisableRaycastUnderAlpha )
				{
					canvasGroup.blocksRaycasts = false ;	// 無効
				}
				else
				{
					canvasGroup.blocksRaycasts = true ;	// 有効
				}
			}
		}


		/// <summary>
		/// レイキャストターゲット(ショートカット)
		/// </summary>
		virtual public bool RaycastTarget
		{
			get
			{
				Graphic g = GetComponent<Graphic>() ;
				if( g == null )
				{
					return false ;
				}

				return g.raycastTarget ;
			}
			set
			{
				Graphic g = GetComponent<Graphic>() ;
				if( g == null )
				{
					return  ;
				}

				g.raycastTarget = value ;
			}
		}

		//----------------------------------------------------

		/// <summary>
		/// マテリアルタイプ
		/// </summary>
		public enum MaterialTypes
		{
			Default			= 0,
			Additional		= 1,
			Grayscale		= 2,
			Sepia			= 3,
			Interpolation	= 4,
			Mosaic			= 5,
			Blur			= 6,
		}

		[SerializeField][HideInInspector]
		private MaterialTypes m_MaterialType = MaterialTypes.Default ;

		/// <summary>
		/// マテリアルタイプ
		/// </summary>
		public  MaterialTypes  MaterialType
		{
			get
			{
				return m_MaterialType ;
			}
			set
			{
				if( m_MaterialType != value )
				{
					m_MaterialType  = value ;

					Graphic graphic = GetGraphic() ;
	
					if( graphic != null )
					{
						graphic.material = null ;
						graphic.GraphicUpdateComplete() ;
					}

					if( m_ActiveMaterial != null )
					{
						DestroyImmediate( m_ActiveMaterial ) ;
						m_ActiveMaterial = null ;
					}

					if( m_MaterialType != MaterialTypes.Default )
					{
						m_ActiveMaterial = CreateCustomMaterial( m_MaterialType ) ;

						if( graphic != null && m_ActiveMaterial != null )
						{
							graphic.material = m_ActiveMaterial ;
							graphic.GraphicUpdateComplete() ;
						}

						if( m_MaterialType == MaterialTypes.Sepia )
						{
							ProcessSepia() ;
						}
						else
						if( m_MaterialType == MaterialTypes.Interpolation )
						{
							ProcessInterpolation() ;
						}
						else
						if( m_MaterialType == MaterialTypes.Mosaic )
						{
							ProcessMosaic() ;
						}
					}
				}
			}
		}

		// カスタムマテリアルのインスタンスを取得する
		private Material CreateCustomMaterial( MaterialTypes materialType )
		{
			Material material = null ;

			if( materialType == MaterialTypes.Additional )
			{
				// パラメータは無いためアセットファイル側のインスタンスを設定する(SharedMaterial)
//				material = Instantiate( Resources.Load<Material>( "uGUIHelper/Shaders/Normal/UI-Additional" ) ) ;
				material = Resources.Load<Material>( "uGUIHelper/Shaders/Normal/UI-Additional" ) ;
			}
			else
			if( materialType == MaterialTypes.Grayscale )
			{
				// パラメータは無いためアセットファイル側のインスタンスを設定する(SharedMaterial)
//				material = Instantiate( Resources.Load<Material>( "uGUIHelper/Shaders/Normal/UI-Grayscale" ) ) ;
				material = Resources.Load<Material>( "uGUIHelper/Shaders/Normal/UI-Grayscale" ) ;
			}
			else
			if( materialType == MaterialTypes.Sepia )
			{
				material = Instantiate( Resources.Load<Material>( "uGUIHelper/Shaders/Normal/UI-Sepia" ) ) ;
			}
			else
			if( materialType == MaterialTypes.Interpolation )
			{
				material = Instantiate( Resources.Load<Material>( "uGUIHelper/Shaders/Normal/UI-Interpolation" ) ) ;
			}
			else
			if( m_MaterialType == MaterialTypes.Mosaic )
			{
				material = Instantiate( Resources.Load<Material>( "uGUIHelper/Shaders/Normal/UI-Mosaic" ) ) ;
			}
			else
			if( materialType == MaterialTypes.Blur )
			{
				material = Instantiate( Resources.Load<Material>( "uGUIHelper/Shaders/Normal/UI-Blur" ) ) ;
			}

			return material ;
		}

		// マテリアルはシリアライズ対象にならない
		private Material m_ActiveMaterial = null ;

		//-----------------------------------
		// セピア関係

		[SerializeField][HideInInspector]
		private float m_SepiaDark = 0.1f ;

		/// <summary>
		/// セピアの明暗度
		/// </summary>
		public  float  SepiaDark
		{
			get
			{
				return m_SepiaDark ;
			}
			set
			{
				m_SepiaDark = value ;
				ProcessSepia() ;
			}
		}


		[SerializeField][HideInInspector]
		private float m_SepiaStrength = 0.1f ;

		/// <summary>
		/// セピアの強度
		/// </summary>
		public  float  SepiaStrength
		{
			get
			{
				return m_SepiaStrength ;
			}
			set
			{
				m_SepiaStrength = value ;
				ProcessSepia() ;
			}
		}


		[SerializeField][HideInInspector]
		private float m_SepiaInterpolation = 1.0f ;

		/// <summary>
		/// マテリアルパラメータ値
		/// </summary>
		public  float  SepiaInterpolation
		{
			get
			{
				return m_SepiaInterpolation ;
			}
			set
			{
				m_SepiaInterpolation = value ;
				ProcessSepia() ;
			}
		}

		// セピアの反映
		private bool ProcessSepia()
		{
			Graphic graphic = GetGraphic() ;

			if( m_MaterialType != MaterialTypes.Sepia || graphic == null || m_ActiveMaterial == null )
			{
				return false ;
			}
			
			graphic.materialForRendering.SetFloat( "_Dark",				m_SepiaDark ) ;
			graphic.materialForRendering.SetFloat( "_Strength",			m_SepiaStrength ) ;
			graphic.materialForRendering.SetFloat( "_Interpolation",	m_SepiaInterpolation ) ;

			return true ;
		}

		//-----------------------------------
		// インターポリューション関係

		[SerializeField][HideInInspector]
		private float m_InterpolationValue = 1.0f ;

		/// <summary>
		/// マテリアルパラメータ値
		/// </summary>
		public  float  InterpolationValue
		{
			get
			{
				return m_InterpolationValue ;
			}
			set
			{
				m_InterpolationValue = value ;
				ProcessInterpolation() ;
			}
		}

		[SerializeField][HideInInspector]
		private Color m_InterpolationColor = Color.white ;

		/// <summary>
		/// マテリアルパラメータ値
		/// </summary>
		public  Color InterpolationColor
		{
			get
			{
				return m_InterpolationColor ;
			}
			set
			{
				m_InterpolationColor = value ;
				ProcessInterpolation() ;
			}
		}

		// インターポレーションの反映
		private bool ProcessInterpolation()
		{
			Graphic graphic = GetGraphic() ;

			if( m_MaterialType != MaterialTypes.Interpolation || graphic == null || m_ActiveMaterial == null )
			{
				return false ;
			}
			
			graphic.materialForRendering.SetFloat( "_InterpolationValue", m_InterpolationValue ) ;
			graphic.materialForRendering.SetColor( "_InterpolationColor", m_InterpolationColor ) ;

			return true ;
		}
		
		//-----------------------------------
		// モザイク関係

		[SerializeField][HideInInspector]
		private float m_MosaicIntensity = 0.5f ;

		/// <summary>
		/// マテリアルパラメータ値
		/// </summary>
		public  float  MosaicIntensity
		{
			get
			{
				return m_MosaicIntensity ;
			}
			set
			{
				m_MosaicIntensity = value ;
				ProcessMosaic() ;
			}
		}

		[SerializeField][HideInInspector]
		private bool  m_MosaicSquareization = false ;

		/// <summary>
		/// モザイクのドットを正四角形にするかどうか
		/// </summary>
		public  bool  MosaicSquareization
		{
			get
			{
				return m_MosaicSquareization ;
			}
			set
			{
				m_MosaicSquareization = value ;
				ProcessMosaic() ;
			}
		}

		//---------------

		// モザイク反映
		private bool ProcessMosaic()
		{
			Graphic graphic = GetGraphic() ;

			if( m_MaterialType != MaterialTypes.Mosaic || graphic == null || m_ActiveMaterial == null )
			{
				return false ;
			}
			
			float intensity = 1.0f - m_MosaicIntensity ;
			intensity *= intensity ;

			float w = this.Width ;
			float h = this.Height ;

			float sw, sh ;
			float cw, ch ;
				
			if( w >= h )
			{
				// 横の方が長いので横を基準とする

				if( m_MosaicIntensity == 0 )
				{
					// モザイク無し
					sw = w ;
					cw = 0 ;

					sh = h ;
					ch = 0 ;
				}
				else
				{
					// モザイク有り
					if( w <  1 )
					{
						w  = 1 ;
					}
					sw = ( int )( ( ( int )w - 1 ) * intensity + 1 ) ;
					cw = 0.5f / sw ;

					if( m_MosaicSquareization == false )
					{
						// 正方形補正無し
						sh = sw ;
						ch = cw ;
					}
					else
					{
						// 正方形補正有り
						if( h <  1 )
						{
							h  = 1 ;
						}
						sh = ( int )( ( ( int )h - 1 ) * intensity + 1 ) ;
						ch = 0.5f / sh ;
					}
				}
			}
			else
			{
				// 縦の方が長いので縦を基準とする

				if( m_MosaicIntensity == 0 )
				{
					// モザイク無し
					sh = h ;
					ch = 0 ;

					sw = w ;
					cw = 0 ;
				}
				else
				{
					// モザイク有り
					if( h <  1 )
					{
						h  = 1 ;
					}
					sh = ( int )( ( ( int )h - 1 ) * intensity + 1 ) ;
					ch = 0.5f / sh ;

					if( m_MosaicSquareization == false )
					{
						// 正方形補正無し
						sw = sh ;
						cw = ch ;
					}
					else
					{
						// 正方形補正有り
						if( w <  1 )
						{
							w  = 1 ;
						}
						sw = ( int )( ( ( int )w - 1 ) * intensity + 1 ) ;
						cw = 0.5f / sw ;
					}
				}
			}

//			Debug.LogWarning( "モザイク:" + new Vector4( sw, sh, cw, ch ) ) ;
			graphic.materialForRendering.SetVector( "_Mosaic", new Vector4( sw, sh, cw, ch ) ) ;
//			graphic.materialForRendering.SetFloat( "_MosaicIntensity", m_MosaicIntensity ) ;

			return true ;
		}

		//-------------------------------------------------------------------------------------------

		// タイムスケール
		[SerializeField]
		protected float m_TimeScale = 1.0f ;

		/// <summary>
		/// タイムスケール
		/// </summary>
		public float TimeScale
		{
			get
			{
				return m_TimeScale ;
			}
			set
			{
				if( m_TimeScale != value )
				{
					m_TimeScale  = value ;

					// 注意 : １度でも Animator を使った事が無いと m_Animator にはインスタンスが記録されない(キャッシュ)
					if( m_Animator != null )
					{
						m_Animator.speed = m_TimeScale ;
					}

					OnTimeScaleChanged( m_TimeScale ) ;
				}
			}
		}

		/// <summary>
		/// タイムスケールが変更された際に呼び出される
		/// </summary>
		/// <param name="timeScale"></param>
		virtual protected void OnTimeScaleChanged( float timeScale ){}

		//-------------------------------------------

#if UNITY_EDITOR
		
		private string m_RemoveTweenIdentity = null ;

		public  string  RemoveTweenIdentity
		{
			set
			{
				m_RemoveTweenIdentity = value ;
			}
		}

		private int    m_RemoveTweenInstance = 0 ;

		public  int     RemoveTweenInstance
		{
			set
			{
				m_RemoveTweenInstance = value ;
			}
		}

#endif
		
		/// <summary>
		/// Tween の追加
		/// </summary>
		/// <param name="identity"></param>
		public UITween AddTween( string identity )
		{
			UITween tween = gameObject.AddComponent<UITween>() ;
			tween.Identity = identity ;

			return tween ;
		}
		
		/// <summary>
		/// Tween の削除
		/// </summary>
		/// <param name="identity"></param>
		/// <param name="instance"></param>
		public void RemoveTween( string identity, int instance = 0 )
		{
			UITween[] tweens = GetComponents<UITween>() ;
			if( tweens == null || tweens.Length == 0 )
			{
				return ;
			}
			int i, l = tweens.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( ( instance == 0 && tweens[ i ].Identity == identity ) || ( instance != 0 && tweens[ i ].Identity == identity && tweens[ i ].GetInstanceID() == instance ) )
				{
					break ;
				}
			}

			if( i >= l )
			{
#if UNITY_EDITOR
				Debug.LogWarning( "[Tween] Not found this identity -> " + identity ) ;
#endif
				return ;
			}

			if( Application.isPlaying == false )
			{
				DestroyImmediate( tweens[ i ] ) ;
			}
			else
			{
				Destroy( tweens[ i ] ) ;
			}
		}

		/// <summary>
		/// Tween の取得
		/// </summary>
		/// <param name="identity"></param>
		/// <returns></returns>
		public UITween GetTween( string identity )
		{
			UITween[] tweens = gameObject.GetComponents<UITween>() ;
			if( tweens == null || tweens.Length == 0 )
			{
				return null ;
			}

			int i, l = tweens.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( tweens[ i ].Identity == identity )
				{
					return tweens[ i ] ;
				}
			}

#if UNITY_EDITOR
			Debug.LogWarning( "[Tween] Not found this identity -> " + identity + " / "+ name ) ;
#endif
			return null ;
		}

		/// <summary>
		/// 全ての Tween を取得
		/// </summary>
		/// <returns></returns>
		public Dictionary<string,UITween> GetAllTweens()
		{
			UITween[] tweens = gameObject.GetComponents<UITween>() ;
			if( tweens == null || tweens.Length == 0 )
			{
				return null ;
			}

			Dictionary<string,UITween> targets = new Dictionary<string, UITween>() ;

			int i, l = tweens.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( string.IsNullOrEmpty( tweens[ i ].Identity ) == false )
				{
					if( targets.ContainsKey( tweens[ i ].Identity ) == false )
					{
						targets.Add( tweens[ i ].Identity, tweens[ i ] ) ;
					}
				}
			}

			if( targets.Count == 0 )
			{
				return null ;
			}

			return targets ;
		}

		/// <summary>
		/// Tween の Delay と Duration を設定
		/// </summary>
		/// <param name="identity"></param>
		/// <param name="delay"></param>
		/// <param name="duration"></param>
		/// <returns></returns>
		public bool SetTweenTime( string identity, float delay = -1, float duration = -1 )
		{
			UITween tween = GetTween( identity ) ;
			if( tween == null )
			{
				return false ;
			}

			tween.Delay		= delay ;
			tween.Duration	= duration ;
			return true ;
		}

		//-----------------------------------

		/// <summary>
		/// 終了を待つ機構無しに再生する
		/// </summary>
		/// <param name="identity"></param>
		/// <param name="delay"></param>
		/// <param name="duration"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		public bool PlayTweenDirect( string identity, float delay = -1, float duration = -1, float offset = 0, Action<string,UITween> onFinishedAction = null, float additionalDelay = 0, float additionalDuration = 0 )
		{
			UITween tween = GetTween( identity ) ;
			if( tween == null )
			{
				return false ;
			}

			if( gameObject.activeSelf == false )
			{
				gameObject.SetActive( true ) ;
			}

			if( m_Visible == false )
			{
				Show() ;
			}

			// アクティブになったタイミングで実行するので親が非アクティブであっても実行自体は行う
//			if( gameObject.activeInHierarchy == false )
//			{
//				// 親が非アクティブならコルーチンは実行できないので終了
//				return true ;
//			}

			tween.Play( delay, duration, offset, onFinishedAction, additionalDelay, additionalDuration ) ;

			return true ;
		}
		
		/// <summary>
		/// 非アクティブ状態の時のみ再生する
		/// </summary>
		/// <param name="identity"></param>
		/// <returns></returns>
		public AsyncState PlayTweenIfHiding( string identity, float delay = -1, float duration = -1, float offset = 0, Action<string,UITween> onFinishedAction = null, float additionalDelay = 0, float additionalDuration = 0 )
		{
			return PlayTween( identity, delay, duration, offset, onFinishedAction, additionalDelay, additionalDuration, true, false ) ;
		}

		/// <summary>
		/// 再生終了と同時に非アクティブ状態にする
		/// </summary>
		/// <param name="identity"></param>
		/// <returns></returns>
		public AsyncState PlayTweenAndHide( string identity, float delay = -1, float duration = -1, float offset = 0, Action<string,UITween> onFinishedAction = null, float additionalDelay = 0, float additionalDuration = 0 )
		{
			return PlayTween( identity, delay, duration, offset, onFinishedAction, additionalDelay, additionalDuration, false, true ) ;
		}

		/// <summary>
		/// Tween の再生(コルーチン)
		/// </summary>
		/// <param name="identity"></param>
		/// <param name="delay"></param>
		/// <param name="duration"></param>
		/// <returns></returns>
		public AsyncState PlayTween( string identity, float delay = -1, float duration = -1, float offset = 0, Action<string,UITween> onFinishedAction = null, float additionalDelay = 0, float additionalDuration = 0, bool ifHiding = false, bool autoHide = false )
		{
			UITween tween = GetTween( identity ) ;
			if( tween == null )
			{
				Debug.LogWarning( "Not found identity of tween : " + identity + " / " + name ) ;
				return null ;
			}

			if( ifHiding == true && ( gameObject.activeSelf == true && m_Visible == true ) )
			{
				return new AsyncState( this ){ IsDone = true } ;
			}

			if( autoHide == true && gameObject.activeSelf == false )
			{
				return new AsyncState( this ){ IsDone = true } ;
			}

			if( gameObject.activeSelf == false )
			{
				gameObject.SetActive( true ) ;
			}

			if( m_Visible == false )
			{
				Show() ;
			}

			if( gameObject.activeInHierarchy == false )
			{
				// 親が非アクティブならコルーチンは実行できないので終了
				return new AsyncState( this ){ IsDone = true } ;
			}

			AsyncState state = new AsyncState( this ) ;
			StartCoroutine( PlayTweenAsync_Private( tween, delay, duration, offset, onFinishedAction, additionalDelay, additionalDuration, autoHide, state ) ) ;
			return state ;
		}

		public IEnumerator PlayTweenAsync_Private( UITween tween, float delay, float duration, float offset, Action<string,UITween> onFinishedAction, float additionalDelay, float additionalDuration, bool autoHide, AsyncState state )
		{
			// 同じトゥイーンを多重実行出来ないようにする
			if( tween.IsRunning == true || tween.IsPlaying == true )
			{
//				tween.Stop() ;	// ストップを実行してはならない。古い実行の方で停止されるのを待つ
				yield return new WaitWhile( () => ( ( tween.IsRunning == true ) | ( tween.IsPlaying == true ) ) ) ;
			}

			//----------------------------------------------------------

			bool destroyAtEnd = tween.DestroyAtEnd ;
			tween.DestroyAtEnd = false ;

			tween.Play( delay, duration, offset, onFinishedAction, additionalDelay, additionalDuration ) ;

			yield return new WaitWhile( () => ( tween.IsRunning == true || tween.IsPlaying == true ) ) ;
			
			state.IsDone = true ;

			if( autoHide == true )
			{
				gameObject.SetActive( false ) ;
			}

			if( destroyAtEnd == true )
			{
				Destroy( tween.gameObject ) ;
			}
		}
		
		/// <summary>
		/// 指定した Tween の実行中り有無
		/// </summary>
		/// <param name="identity"></param>
		/// <returns></returns>
		public bool IsTweenPlaying( string identity )
		{
			UITween tween = GetTween( identity ) ;
			if( tween == null )
			{
				return false ;
			}

			if( tween.enabled == true && ( tween.IsRunning == true || tween.IsPlaying == true ) )
			{
				return true ;// 実行中
			}
			
			return false ;	
		}

		/// <summary>
		/// いずれかの Tween の実行中の有無 
		/// </summary>
		public bool IsAnyTweenPlaying
		{
			get
			{
				UITween[] tweens = gameObject.GetComponents<UITween>() ;
				if( tweens == null || tweens.Length == 0 )
				{
					return false ;
				}

				int i, l = tweens.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( tweens[ i ].enabled == true && (  tweens[ i ].IsRunning == true || tweens[ i ].IsPlaying == true ) )
					{
						return true ;
					}
				}
			
				return false ;
			}
		}

		/// <summary>
		/// いずれかの Tween の実行中の有無 
		/// </summary>
		public bool IsAnyTweenPlayingInParents
		{
			get
			{
				if( IsAnyTweenPlaying == true )
				{
					return true ;
				}

				// 親も含めてトゥイーンが動作中か確認する
				UIView view ;
				Transform t = transform.parent ;
				while( t != null )
				{
					view = t.GetComponent<UIView>() ;
					if( view != null )
					{
						if( view.IsAnyTweenPlaying == true )
						{
							return true ;
						}
					}
					t = t.parent ;
				}

				return false ;
			}
		}

		/// <summary>
		/// Tween の一時停止
		/// </summary>
		/// <param name="tIdentity"></param>
		/// <returns></returns>
		public bool PauseTween( string identity )
		{
			UITween tween = GetTween( identity ) ;
			if( tween == null )
			{
				return false ;
			}

			tween.Pause() ;
			return true ;
		}

		/// <summary>
		/// Tween の再開
		/// </summary>
		/// <param name="identity"></param>
		/// <returns></returns>
		public bool UnpauseTween( string identity )
		{
			UITween tween = GetTween( identity ) ;
			if( tween == null )
			{
				return false ;
			}

			tween.Unpause() ;
			return true ;
		}

		/// <summary>
		/// Tween の完全停止
		/// </summary>
		/// <param name="identity"></param>
		/// <returns></returns>
		public bool StopTween( string identity )
		{
			UITween tween = GetTween( identity ) ;
			if( tween == null )
			{
				return false ;
			}

			tween.Stop() ;
			return true ;
		}

		/// <summary>
		/// Tween の完全停止と状態のリセット
		/// </summary>
		/// <param name="identity"></param>
		/// <returns></returns>
		public bool StopAndResetTween( string identity )
		{
			UITween tween = GetTween( identity ) ;
			if( tween == null )
			{
				return false ;
			}

			tween.StopAndReset() ;
			return true ;
		}

		/// <summary>
		/// Tween の完全停止
		/// </summary>
		/// <param name="identity"></param>
		/// <returns></returns>
		public bool FinishTween( string identity )
		{
			UITween tween = GetTween( identity ) ;
			if( tween == null )
			{
				return false ;
			}

			tween.Finish() ;
			return true ;
		}

		/// <summary>
		/// 全ての Tween の停止
		/// </summary>
		public bool StopAllTweens()
		{
			UITween[] tweens = gameObject.GetComponents<UITween>() ;
			if( tweens == null || tweens.Length == 0 )
			{
				return false ;
			}

			foreach( var tween in tweens )
			{
				if( tween.enabled == true && ( tween.IsRunning == true || tween.IsPlaying == true ) )
				{
					tween.Stop() ;
				}
			}
			
			return false ;
		}

		/// <summary>
		/// 全ての Tween の停止とリセット
		/// </summary>
		public bool StopAndResetAllTweens()
		{
			UITween[] tweens = gameObject.GetComponents<UITween>() ;
			if( tweens == null || tweens.Length == 0 )
			{
				return false ;
			}

			foreach( var tween in tweens )
			{
				if( tween.enabled == true )
				{
					tween.StopAndReset() ;
				}
			}
			
			return false ;
		}

		/// <summary>
		/// 全ての Tween の停止とリセット
		/// </summary>
		public bool FinishAllTweens()
		{
			UITween[] tweens = gameObject.GetComponents<UITween>() ;
			if( tweens == null || tweens.Length == 0 )
			{
				return false ;
			}

			foreach( var tween in tweens )
			{
				if( tween.enabled == true )
				{
					tween.Finish() ;
				}
			}
			
			return false ;
		}

		/// <summary>
		/// 実行中の Tween の実行開始からの経過時間を取得する
		/// </summary>
		/// <param name="identity"></param>
		/// <returns></returns>
		public float GetTweenProcessTime( string identity )
		{
			UITween tween = GetTween( identity ) ;
			if( tween == null )
			{
				return 0 ;
			}

			return tween.ProcessTime ;
		}

		/// <summary>
		/// 実行中の Tween の実行開始からの経過時間を設定する
		/// </summary>
		/// <param name="identity"></param>
		/// <param name="time"></param>
		public bool SetTweenProcessTime( string identity, float time )
		{
			UITween tween = GetTween( identity ) ;
			if( tween == null )
			{
				return false ;
			}

			tween.ProcessTime = time ;

			return true ;
		}

		//-------------------------------------------

#if UNITY_EDITOR
		
		private string m_RemoveFlipperIdentity = null ;

		public  string  RemoveFlipperIdentity
		{
			set
			{
				m_RemoveFlipperIdentity = value ;
			}
		}

		private int    m_RemoveFlipperInstance = 0 ;

		public  int     RemoveFlipperInstance
		{
			set
			{
				m_RemoveFlipperInstance = value ;
			}
		}

#endif

		/// <summary>
		/// Flipper の追加
		/// </summary>
		/// <param name="identity"></param>
		public UIFlipper AddFlipper( string identity )
		{
			UIFlipper flipper = gameObject.AddComponent<UIFlipper>() ;
			flipper.Identity = identity ;

			return flipper ;
		}
		
		/// <summary>
		/// Flipper の削除
		/// </summary>
		/// <param name="identity"></param>
		/// <param name="instance"></param>
		public void RemoveFlipper( string identity, int instance = 0 )
		{
			UIFlipper[] flippers = GetComponents<UIFlipper>() ;
			if( flippers == null || flippers.Length == 0 )
			{
				return ;
			}

			int i, l = flippers.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( ( instance == 0 && flippers[ i ].Identity == identity ) || ( instance != 0 && flippers[ i ].Identity == identity && flippers[ i ].GetInstanceID() == instance ) )
				{
					break ;
				}
			}

			if( i >= l )
			{
#if UNITY_EDITOR
				Debug.LogWarning( "[Flipper] Not found this identity -> " + identity + " / " + name ) ;
#endif
				return ;
			}

			if( Application.isPlaying == false )
			{
				DestroyImmediate( flippers[ i ] ) ;
			}
			else
			{
				Destroy( flippers[ i ] ) ;
			}
		}

		/// <summary>
		/// Flipper の取得
		/// </summary>
		/// <param name="identity"></param>
		/// <returns></returns>
		public UIFlipper GetFlipper( string identity )
		{
			UIFlipper[] flippers = gameObject.GetComponents<UIFlipper>() ;
			if( flippers == null || flippers.Length == 0 )
			{
				return null ;
			}

			foreach( var flipper in flippers )
			{
				if( flipper.Identity == identity )
				{
					return flipper ;
				}
			}

#if UNITY_EDITOR
				Debug.LogWarning( "[Flipper] Not found this identity -> " + identity ) ;
#endif
			return null ;
		}

		/// <summary>
		/// Flipper の再生
		/// </summary>
		/// <param name="identity"></param>
		/// <param name="delay"></param>
		/// <param name="timeScale"></param>
		/// <returns></returns>
		public bool PlayFlipperDirect( string identity, bool destroyAtEnd = false, float speed = 0, float delay = -1, Action<string,UIFlipper> onFinishedAction = null )
		{
			UIFlipper flipper = GetFlipper( identity ) ;
			if( flipper == null )
			{
				return false ;
			}

			if( flipper.gameObject.activeSelf == false )
			{
				flipper.gameObject.SetActive( true ) ;
			}

			// アクティブになったタイミングで実行するので親が非アクティブであっても実行自体は行う
//			if( gameObject.activeInHierarchy == false )
//			{
//				// 親が非アクティブならコルーチンは実行できないので終了
//				return true ;
//			}

			flipper.Play( destroyAtEnd, speed, delay, onFinishedAction ) ;

			return true ;
		}

		/// <summary>
		///  Flipper の再生(コルーチン)
		/// </summary>
		/// <param name="identity"></param>
		/// <param name="delay"></param>
		/// <param name="duration"></param>
		/// <returns></returns>
		public AsyncState PlayFlipper( string identity, bool destroyAtEnd = false, float speed = 0, float delay = -1 )
		{
			UIFlipper flipper = GetFlipper( identity ) ;
			if( flipper == null )
			{
				Debug.LogWarning( "Not found identity of flipper : " + identity ) ;
				return null ;
			}

			if( flipper.gameObject.activeSelf == false )
			{
				flipper.gameObject.SetActive( true ) ;
			}

			if( gameObject.activeInHierarchy == false )
			{
				// 親が非アクティブならコルーチンは実行できないので終了
				return new AsyncState( this ){ IsDone = true } ;
			}

			AsyncState state = new AsyncState( this ) ;
			StartCoroutine( PlayFlipperAsync_Private( flipper, destroyAtEnd, speed, delay, state ) ) ;
			return state ;
		}

		public IEnumerator PlayFlipperAsync_Private( UIFlipper flipper, bool destroyAtEnd, float speed, float delay, AsyncState state )
		{
			// 同じフリッパーを多重実行出来ないようにする
			if( flipper.IsRunning == true || flipper.IsPlaying == true )
			{
//				flipper.Stop() ;	// ストップを実行してはならない。古い実行の方で停止されるのを待つ
				yield return new WaitWhile( () => ( ( flipper.IsRunning == true ) | ( flipper.IsPlaying == true ) ) ) ;
			}

			//----------------------------------------------------------

			flipper.Play( false, speed, delay ) ;

			yield return new WaitWhile( () => ( flipper.IsRunning == true || flipper.IsPlaying == true ) ) ;

			state.IsDone = true ;

			if( destroyAtEnd == true )
			{
				Destroy( flipper.gameObject ) ;
			}
		}
		
		/// <summary>
		/// 指定した Flipper の実行中り有無
		/// </summary>
		/// <param name="identity"></param>
		/// <returns></returns>
		public bool IsFlipperPlaying( string identity )
		{
			UIFlipper flipper = GetFlipper( identity ) ;
			if( flipper == null )
			{
				return false ;
			}

			if( flipper.enabled == true && ( flipper.IsRunning == true || flipper.IsPlaying == true ) )
			{
				return true ;	// 実行中
			}
			
			return false ;
		}
		
		/// <summary>
		/// いずれかの Flipper の実行中の有無
		/// </summary>
		public bool IsAnyFlipperPlaying
		{
			get
			{
				UIFlipper[] flippers = gameObject.GetComponents<UIFlipper>() ;
				if( flippers == null || flippers.Length == 0 )
				{
					return false ;
				}

				foreach( var flipper in flippers )
				{
					if( flipper.enabled == true && ( flipper.IsRunning == true || flipper.IsPlaying == true ) )
					{
						return true ;
					}
				}
			
				return false ;
			}
		}

		/// <summary>
		/// Flipper の完全停止
		/// </summary>
		/// <param name="identity"></param>
		/// <returns></returns>
		public bool StopFlipper( string identity )
		{
			UIFlipper flipper = GetFlipper( identity ) ;
			if( flipper == null )
			{
				return false ;
			}

			flipper.Stop() ;
			return true ;
		}

		//-------------------------------------------------------
		
		/// <summary>
		/// 親 View を取得する
		/// </summary>
		/// <returns></returns>
		public UIView GetParentView()
		{
			if( transform.parent != null )
			{
				return transform.parent.gameObject.GetComponent<UIView>() ;
			}
			return null ;
		}

		/// <summary>
		/// 親 Canvas を取得する(自身を含む)
		/// </summary>
		/// <returns></returns>
		public Canvas GetParentCanvas()
		{
			int i, l = 64 ;

			Canvas canvas ;

			Transform t = gameObject.transform ;
			for( i  =  0 ; i <  l ; i ++ )
			{
				canvas = t.gameObject.GetComponent<Canvas>() ;
				if( canvas != null )
				{
					return canvas ;
				}
			
				t = t.parent ;
				if( t == null )
				{
					break ;
				}
			}

			return null ;
		}


		/// <summary>
		/// 親 Canvas の設定仮想解像度を取得する
		/// </summary>
		/// <returns></returns>
		public Vector2 GetParentCanvasScalerSize()
		{
			Canvas canvas = GetParentCanvas() ;
			if( canvas == null )
			{
				return Vector2.zero ;
			}

			CanvasScaler canvasScaler = canvas.gameObject.GetComponent<CanvasScaler>() ;
			if( canvasScaler == null )
			{
				return Vector2.zero ;
			}

			return canvasScaler.referenceResolution ;
		}

		/// <summary>
		/// 親 Canvas の実仮想解像度を取得する
		/// </summary>
		/// <returns></returns>
		public Vector2 GetCanvasSize( bool isReal = false )
		{
			Canvas canvas = GetParentCanvas() ;
			if( canvas == null )
			{
				return new Vector2( Screen.width, Screen.height ) ;
			}

			float sw = Screen.width ;
			float sh = Screen.height ;

			if( canvas.worldCamera != null && canvas.worldCamera.targetTexture != null )
			{
				sw = canvas.worldCamera.targetTexture.width ;
				sh = canvas.worldCamera.targetTexture.height ;
			}

			if( Application.isPlaying == false || isReal == true )
			{
				RectTransform rt = canvas.gameObject.GetComponent<RectTransform>() ;
				if( rt == null )
				{
					return  new Vector2( sw, sh ) ;
				}
	
				return rt.sizeDelta ;
			}

			CanvasScaler scaler = canvas.GetComponent<CanvasScaler>() ;
			if( scaler == null )
			{
				return new Vector2( sw, sh ) ;
			}

			if( scaler.uiScaleMode == CanvasScaler.ScaleMode.ConstantPixelSize )
			{
				return new Vector2( sw / scaler.scaleFactor, sh / scaler.scaleFactor ) ;
			}
			else
			if( scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize )
			{
				float rw = scaler.referenceResolution.x ;
				float rh = scaler.referenceResolution.y ;

				if( scaler.screenMatchMode == CanvasScaler.ScreenMatchMode.MatchWidthOrHeight )
				{
					float mf = scaler.matchWidthOrHeight ;

					float wa0 = sw / sh ;
					float wa1 = rw / rh ;
					float wa = Mathf.Lerp( wa0, wa1, mf ) ;

					float w  = rw * wa0 / wa ;

					float ha0 = rh / rw ;
					float ha1 = sw / sh ;
					float ha = Mathf.Lerp( ha0, ha1, mf ) ;

					float h  = rh * ha1 / ha ;

					return new Vector2( w, h ) ;
				}
				else
				if( scaler.screenMatchMode == CanvasScaler.ScreenMatchMode.Expand )
				{
					float w, h ;

					if( sw >= sh )
					{
						// 実スクリーンは横長
						float sa = sw / sh ;
						float ra = rw / rh ;

						if( ra >= sa )
						{
							// 横が１倍
							w = rw ;
							h = rh * ra / sa ;
						}
						else
						{
							// 縦が１倍
							h = rh ;
							w = rw * sa / ra ;
						}
					}
					else
					{
						// 実スクリーンは縦長
						float sa = sh / sw ;
						float ra = rh / rw ;

						if( ra >= sa )
						{
							// 縦が１倍
							h = rh ;
							w = rw * ra / sa ;
						}
						else
						{
							// 横が１倍
							w = rw ;
							h = rh * sa / ra ;
						}
					}

					return new Vector2( w, h ) ;
				}
				else
				if( scaler.screenMatchMode == CanvasScaler.ScreenMatchMode.Shrink )
				{
					float w, h ;

					if( sw >= sh )
					{
						// 実スクリーンは横長
						float sa = sw / sh ;
						float ra = rw / rh ;

						if( ra >= sa )
						{
							// 仮想解像度の縦をスクリーンのの横に合わせる
							h = rh ;
							w = rh * sw / sh ;
						}
						else
						{
							// 仮想解像度の横をスクリーンのの横に合わせる
							w = rw ;
							h = rw * sh / sw ;
						}
					}
					else
					{
						// 実スクリーンは縦長
						float sa = sh / sw ;
						float ra = rh / rw ;

						if( ra >= sa )
						{
							// 仮想解像度の横をスクリーンのの横に合わせる
							w = rw ;
							h = rw * sh / sw ;
						}
						else
						{
							// 仮想解像度の縦をスクリーンのの横に合わせる
							h = rh ;
							w = rh * sw / sh ;
						}
					}

					return new Vector2( w, h ) ;
				}
			}
			else
			if( scaler.uiScaleMode == CanvasScaler.ScaleMode.ConstantPhysicalSize )
			{
				RectTransform rt = canvas.gameObject.GetComponent<RectTransform>() ;
				if( rt == null )
				{
					return  new Vector2( sw, sh ) ;
				}
	
				return rt.sizeDelta ;
			}

			return new Vector2( sw, sh ) ;


//			RectTransform tRectTransform = tCanvas.gameObject.GetComponent<RectTransform>() ;
//			if( tRectTransform == null )
//			{
//				return Vector2.zero ;
//			}
//
//			return tRectTransform.sizeDelta ;
		}

		/// <summary>
		/// 親キャンバスの基準となる長さを取得する
		/// </summary>
		/// <param name="ratio"></param>
		/// <returns></returns>
		public float GetCanvasLength( float ratio = 1 )
		{
			Vector2 size = GetParentCanvasScalerSize() ;

			if( size.x >= size.y )
			{
				return size.x * ratio ;
			}
			else
			{
				return size.y * ratio ;
			}
		}

		/// <summary>
		/// 親 Canvas に設定されているワールドカメラを取得する
		/// </summary>
		/// <returns></returns>
		public Camera GetCanvasCamera()
		{
			Canvas canvas = GetParentCanvas() ;
			if( canvas == null )
			{
				return null ;
			}

			return canvas.worldCamera ;
		}

		/// <summary>
		/// Canvas 描画対象の Layer を取得する
		/// </summary>
		/// <returns></returns>
		public uint GetCanvasTargetLayer()
		{
			Canvas canvas = GetParentCanvas() ;
			if( canvas == null )
			{
				return 0 ;	// 不明
			}

			if( canvas.worldCamera != null )
			{
				return ( uint )canvas.worldCamera.cullingMask ;
			}
			else
			{
				return ( uint )( 1 << canvas.gameObject.layer ) ;
			}
		}
		
		/// <summary>
		/// Canvas の描画対象 Layer の内で最も最初のものを取得する
		/// </summary>
		/// <returns></returns>
		public int GetCanvasTargetLayerOfFirst()
		{
			uint layer = GetCanvasTargetLayer() ;
			if( layer == 0 )
			{
				return 5 ;
			}
		
			int i, l = 32 ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( ( layer & ( uint )( 1 << i ) ) != 0 )
				{
					return i ;
				}
			}
		
			return 5 ;
		}
		
		/// <summary>
		/// 属するキャンバスのオーバーレイ指定状態
		/// </summary>
		public bool IsCanvasOverlay
		{
			get
			{
				int i, l = 64 ;

				UICanvas canvas = null ;

				Transform t = gameObject.transform ;
				for( i  =  0 ; i <  l ; i ++ )
				{
					canvas = t.gameObject.GetComponent<UICanvas>() ;
					if( canvas != null )
					{
						break ;
					}
			
					t = t.parent ;
					if( t == null )
					{
						break ;
					}
				}
				
				if( canvas == null )
				{
					return false ;
				}

				return canvas.IsOverlay ;
			}
		}

		//---------------------------------------------------------------

		/// <summary>
		/// View を追加する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T AddView<T>() where T : UIView
		{
			return AddView<T>( "" ) ;
		}

		/// <summary>
		/// View を追加する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="viewName"></param>
		/// <param name="option"></param>
		/// <returns></returns>
		public T AddView<T>( string viewName, string option = "" ) where T : UIView
		{
			// クラスの名前を取得する
			if( string.IsNullOrEmpty( viewName ) == true )
			{
				viewName = typeof( T ).ToString() ;

				int i ;

				i = viewName.IndexOf( "." ) ;
				if( i >= 0 )
				{
					viewName = viewName.Substring( i ) ;
				}

				i = viewName.IndexOf( "UI" ) ;
				if( i >= 0 )
				{
					viewName = viewName.Substring( i + 2, viewName.Length - ( i + 2 ) ) ;
				}
			}
		
			GameObject go = new GameObject( viewName, typeof( RectTransform ) ) ;
		
			if( go == null )
			{
				// 失敗
				return default ;
			}
		
			// 最初に親を設定してしまう
			go.transform.SetParent( gameObject.transform, false ) ;
		
			// コンポーネントをアタッチする
			T component = go.AddComponent<T>() ;
		
			if( component == null )
			{
				// 失敗
				Destroy( go ) ;
				return default ;
			}
			
			// AddView からの場合は　SetDefault を実行する
			component.SetDefault( option ) ;

			return component ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 空の GameObject を追加する
		/// </summary>
		/// <param name="viewName"></param>
		/// <param name="t"></param>
		/// <param name="layer"></param>
		/// <returns></returns>
		public GameObject AddObject( string viewName, Transform t = null, int layer = -1 )
		{
			GameObject go = new GameObject( viewName ) ;
			go.transform.localPosition = new Vector3(   0,  0,   0 ) ;
			go.transform.localRotation = Quaternion.identity ;
			go.transform.localScale = Vector3.one ;

			if( t == null )
			{
				t = transform ;
			}

			go.transform.SetParent( t, false ) ;

			if( layer >= -1 && layer <= 31 )
			{
				if( layer == -1 )
				{
					layer = t.gameObject.layer ;
				}
				SetLayer( go, layer ) ;
			}

			return go ;
		}

		/// <summary>
		/// 指定のコンポーネントをアタッチしたの GameObject を追加する
		/// </summary>
		/// <param name="tTransform"></param>
		/// <param name="tLayer"></param>
		/// <returns></returns>
		public T AddObject<T>( string viewName, Transform t = null, int layer = -1 ) where T : UnityEngine.Component
		{
			GameObject go = new GameObject( viewName ) ;
			go.transform.localPosition = new Vector3(   0,  0,   0 ) ;
			go.transform.localRotation = Quaternion.identity ;
			go.transform.localScale = Vector3.one ;

			if( t == null )
			{
				t = transform ;
			}

			go.transform.SetParent( t, false ) ;

			if( layer >= -1 && layer <= 31 )
			{
				if( layer == -1 )
				{
					layer = t.gameObject.layer ;
				}
				SetLayer( go, layer ) ;
			}

			T component = go.AddComponent<T>() ;
			return component ;
		}

		//-------------------------------------------------------------------

		/// <summary>
		/// Prefab を追加する
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public GameObject AddPrefab( string path, Transform t = null, int layer = -1 )
		{
			GameObject go = Resources.Load( path, typeof( GameObject ) ) as GameObject ;
			if( go == null )
			{
				return null ;
			}

			go = Instantiate( go ) ;
		
			AddPrefab( go, t, layer ) ;
		
			return go ;
		}
	
		/// <summary>
		/// Prefab を追加する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="tPrefab"></param>
		/// <param name="tTransform"></param>
		/// <returns></returns>
		public GameObject AddPrefab( GameObject prefab, Transform t = null, int layer = -1 )
		{
			if( prefab == null )
			{
				return null ;
			}
			
			if( t == null )
			{
				t = transform ;
			}

			GameObject go = ( GameObject )GameObject.Instantiate( prefab ) ;
			if( go == null )
			{
				return null ;
			}
		
			go.transform.SetParent( t, false ) ;

			if( layer >= -1 && layer <= 31 )
			{
				if( layer == -1 )
				{
					layer = t.gameObject.layer ;
				}
				SetLayer( go, layer ) ;
			}

			return go ;
		}

		/// <summary>
		/// Prefab を追加する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="tPath"></param>
		/// <returns></returns>
		public T AddPrefab<T>( string path, Transform t = null, int layer = -1 ) where T : UnityEngine.Component
		{
			GameObject prefab = Resources.Load( path, typeof( GameObject ) ) as GameObject ;
			if( prefab == null )
			{
				return null ;
			}

			return AddPrefab<T>( prefab, t, layer ) ;
		}
		
		/// <summary>
		/// プレハブからインスタンスを生成し自身の子とする
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="tPrefab"></param>
		/// <param name="tParentName"></param>
		/// <returns></returns>
		public T AddPrefabOnChild<T>( GameObject prefab, string parentName = null, int layer = -1 ) where T : UnityEngine.Component
		{
			Transform t = null ;
			if( string.IsNullOrEmpty( parentName ) == false )
			{
				if( transform.name.ToLower() == parentName.ToLower() )
				{
					t = transform ;
				}
				else
				{
					t = GetTransformByName( transform, parentName ) ;
				}
			}

			return AddPrefab<T>( prefab, t, layer ) ;
		}

		/// <summary>
		/// 自身に含まれる指定した名前のトランスフォームを検索する
		/// </summary>
		/// <param name="tName"></param>
		/// <returns></returns>
		public Transform GetTransformByName( string viewName, bool isContains = false )
		{
			if( string.IsNullOrEmpty( viewName ) == true )
			{
				return null ;
			}

			return GetTransformByName( transform, viewName, isContains ) ;
		}

		// 自身に含まれる指定した名前のトランスフォームを検索する
		private Transform GetTransformByName( Transform t, string viewName, bool isContains = false )
		{
			viewName = viewName.ToLower() ;

			Transform child ;
			string childViewName ;
			bool result ;

			int i, l = t.childCount ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				child = t.GetChild( i ) ;

				childViewName = child.name.ToLower() ;

				result = false ;
				if( isContains == false && childViewName == viewName )
				{
					result = true ;
				}
				else
				if( isContains == true && childViewName.Contains( viewName ) == true )
				{
					result = true ;
				}

				if( result == true )
				{
					// 発見
					return child ;
				}
				else
				{
					if( child.childCount >  0 )
					{
						child = GetTransformByName( child, viewName ) ;
						if( child != null )
						{
							// 発見
							return child ;
						}
					}
				}
			}

			// 発見出来ず
			return null ;
		}

		/// <summary>
		/// Prefab を追加する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="prefab"></param>
		/// <param name="t"></param>
		/// <returns></returns>
		public T AddPrefab<T>( GameObject prefab, Transform t = null, int layer = -1 ) where T : UnityEngine.Component
		{
			if( prefab == null )
			{
				return default ;
			}
			
			if( t == null )
			{
				t = transform ;
			}

			GameObject go = ( GameObject )GameObject.Instantiate( prefab ) ;
			if( go == null )
			{
				return null ;
			}
		
			go.transform.SetParent( t, false ) ;

			if( layer >= -1 && layer <= 31 )
			{
				if( layer == -1 )
				{
					layer = t.gameObject.layer ;
				}
				SetLayer( go, layer ) ;
			}

			T tComponent = go.GetComponent<T>() ;
			return tComponent ;
		}

		/// <summary>
		/// 指定したゲームオブジェクトを自身の子にする
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="go"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		public GameObject SetPrefab( GameObject go, Transform parent = null )
		{
			if( go == null )
			{
				return null ;
			}
		
			if( parent == null )
			{
				parent = transform ;
			}

			go.transform.SetParent( parent, false ) ;
			SetLayer( go, gameObject.layer ) ;

			return go ;
		}
		
		/// <summary>
		/// Layer を設定する
		/// </summary>
		/// <param name="go"></param>
		/// <param name="layer"></param>
		private void SetLayer( GameObject go, int layer )
		{
			go.layer = layer ;
			foreach( Transform t in go.transform )
			{
				SetLayer( t.gameObject, layer ) ;
			}
		}
		
		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// Component を追加する(ショートカット)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T AddComponent<T>() where T : UnityEngine.Component
		{
			return gameObject.AddComponent<T>() ;
		}
		
		/// <summary>
		/// ARGB 32 ビットから Color を返す
		/// </summary>
		/// <param name="color"></param>
		/// <returns></returns>
		public static Color32 ARGB( uint color )
		{
			return new Color32( ( byte )( ( color >> 16 ) & 0xFF ), ( byte )( ( color >>  8 ) & 0xFF ), ( byte )( ( color & 0xFF ) ), ( byte )( ( color >> 24 ) & 0xFF ) ) ;
		}
	
		//-------------------------------------------------
	
		// Interface
		
		//----------

		// キャッシュ
		private Camera m_Camera = null ;

		/// <summary>
		/// Camera(ショートカット)
		/// </summary>
		virtual public Camera GetCamera()
		{
			if( m_Camera == null )
			{
				m_Camera = gameObject.GetComponent<Camera>() ;
			}
			return m_Camera ;
		}

		//----------

		// キャッシュ
		private RectTransform m_RectTransform ;

		/// <summary>
		/// RectTransform(ショートカット)
		/// </summary>
		virtual public RectTransform GetRectTransform()
		{
			if( m_RectTransform != null )
			{
				return m_RectTransform ;
			}
			m_RectTransform = gameObject.GetComponent<RectTransform>() ;
			return m_RectTransform ;
		}
	
		//----------

		// キャッシュ
		private Canvas m_Canvas = null ;

		/// <summary>
		/// Image(ショートカット)
		/// </summary>
		virtual public Canvas CCanvas
		{
			get
			{
				if( m_Canvas == null )
				{
					m_Canvas = gameObject.GetComponent<Canvas>() ;
				}
				return m_Canvas ;
			}
		}

		/// <summary>
		/// Canvas(ショートカット)
		/// </summary>
		virtual public Canvas GetCanvas()
		{
			if( m_Canvas == null )
			{
				m_Canvas = gameObject.GetComponent<Canvas>() ;
			}
			return m_Canvas ;
		}
	
		//----------

		// キャッシュ
		private CanvasRenderer m_CanvasRenderer = null ;

		/// <summary>
		/// CanvasRenderer(ショートカット)
		/// </summary>
		virtual public CanvasRenderer GetCanvasRenderer()
		{
			if( m_CanvasRenderer == null )
			{
				m_CanvasRenderer = gameObject.GetComponent<CanvasRenderer>() ;
			}
			return m_CanvasRenderer ;
		}

		/// <summary>
		/// CanvasRenderer の有無
		/// </summary>
		public bool IsCanvasRenderer
		{
			get
			{
				if( GetCanvasRenderer() == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					AddCanvasRenderer() ;
				}
				else
				{
#if UNITY_EDITOR

					if( Application.isPlaying == true )
					{
						RemoveCanvasRenderer() ;
					}
					else
					{
						m_RemoveCanvasRenderer = true ;
					}
#else
					RemoveCanvasRenderer() ;
#endif
				}
			}
		}
		
		/// <summary>
		/// CanvasRenderer の追加
		/// </summary>
		public void AddCanvasRenderer()
		{
			if( GetCanvasRenderer() != null )
			{
				return ;
			}
		
			m_CanvasRenderer = gameObject.AddComponent<CanvasRenderer>() ;
		}
		
#if UNITY_EDITOR
		private bool m_RemoveCanvasRenderer = false ;
#endif

		/// <summary>
		/// CanvasRenderer の削除
		/// </summary>
		public void RemoveCanvasRenderer()
		{
			CanvasRenderer canvasRenderer = GetCanvasRenderer() ;
			if( canvasRenderer == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( canvasRenderer ) ;
			}
			else
			{
				Destroy( canvasRenderer ) ;
			}

			m_CanvasRenderer = null ;
		}

		//---------------

		// キャッシュ
		private GraphicEmpty m_GraphicEmpty = null ;

		/// <summary>
		/// GraphicEmpty(ショートカット)
		/// </summary>
		virtual public GraphicEmpty GetGraphicEmpty()
		{
			if( m_GraphicEmpty == null )
			{
				m_GraphicEmpty = gameObject.GetComponent<GraphicEmpty>() ;
			}
			return m_GraphicEmpty ;
		}

		/// <summary>
		/// GraphicEmpty の有無
		/// </summary>
		public bool IsGraphicEmpty
		{
			get
			{
				if( GetGraphicEmpty() == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					AddGraphicEmpty() ;
				}
				else
				{
#if UNITY_EDITOR

					if( Application.isPlaying == true )
					{
						RemoveGraphicEmpty() ;
					}
					else
					{
						m_RemoveGraphicEmpty = true ;
					}
#else
					RemoveGraphicEmpty() ;
#endif
				}
			}
		}
		
		/// <summary>
		/// GraphicEmpty の追加
		/// </summary>
		public void AddGraphicEmpty()
		{
			if( GetGraphicEmpty() != null )
			{
				return ;
			}
		
			m_GraphicEmpty = gameObject.AddComponent<GraphicEmpty>() ;
			m_GraphicEmpty.raycastTarget = true ;
		}
		
#if UNITY_EDITOR
		private bool m_RemoveGraphicEmpty = false ;
#endif

		/// <summary>
		/// GraphicEmpty の削除
		/// </summary>
		public void RemoveGraphicEmpty()
		{
			GraphicEmpty graphicEmpty = GetGraphicEmpty() ;
			if( graphicEmpty == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( graphicEmpty ) ;
			}
			else
			{
				Destroy( graphicEmpty ) ;
			}

			m_GraphicEmpty = null ;
		}

		//----------

		// キャッシュ
		private CanvasScaler m_CanvasScaler = null ;

		/// <summary>
		/// CanvasScaler(ショートカット)
		/// </summary>
		virtual public CanvasScaler GetCanvasScaler()
		{
			if( m_CanvasScaler == null )
			{
				m_CanvasScaler = gameObject.GetComponent<CanvasScaler>() ;
			}
			return m_CanvasScaler ;
		}

		//----------

		// キャッシュ
		private CanvasGroup m_CanvasGroup = null ;

		/// <summary>
		/// CanvasGroup(ショートカット)
		/// </summary>
		virtual public CanvasGroup GetCanvasGroup()
		{
			if( m_CanvasGroup == null )
			{
				try
				{
					m_CanvasGroup = gameObject.GetComponent<CanvasGroup>() ;
				}
				catch( Exception e )
				{
					Debug.LogError( "Error:" + e.Message + " " + transform.parent.name ) ;
				}
			}
			return m_CanvasGroup ;
		}

		/// <summary>
		/// CanvasGroup の有無
		/// </summary>
		public bool IsCanvasGroup
		{
			get
			{
				if( GetCanvasGroup() == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					AddCanvasGroup() ;
				}
				else
				{
#if UNITY_EDITOR

					if( Application.isPlaying == true )
					{
						RemoveCanvasGroup() ;
					}
					else
					{
						m_RemoveCanvasGroup = true ;
					}

#else

					RemoveCanvasGroup() ;

#endif
				}
			}
		}

		/// <summary>
		/// CanvasGroup の追加
		/// </summary>
		public void AddCanvasGroup()
		{
			if( GetCanvasGroup() != null )
			{
				return ;
			}
		
			CanvasGroup canvasGroup ;
		
			canvasGroup = gameObject.AddComponent<CanvasGroup>() ;
			canvasGroup.alpha= 1.0f ;
		}
		
#if UNITY_EDITOR
		private bool m_RemoveCanvasGroup = false ;
#endif
		
		/// <summary>
		/// CanvasGroup の削除
		/// </summary>
		public void RemoveCanvasGroup()
		{
			CanvasGroup canvasGroup = GetCanvasGroup() ;
			if( canvasGroup == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( canvasGroup ) ;
			}
			else
			{
				Destroy( canvasGroup ) ;
			}

			m_CanvasGroup = null ;
		}

		//----------

		// キャッシュ
		private GraphicRaycasterWrapper m_GraphicRaycaster = null ;

		/// <summary>
		/// GraphicRaycaster(ショートカット)
		/// </summary>
		virtual public GraphicRaycasterWrapper GetGraphicRaycaster()
		{
			if( m_GraphicRaycaster == null )
			{
				m_GraphicRaycaster = gameObject.GetComponent<GraphicRaycasterWrapper>() ;
			}
			return m_GraphicRaycaster ;
		}

		//----------

		// キャッシュ
		private Graphic m_Graphic = null ;

		/// <summary>
		/// Graphic(ショートカット)
		/// </summary>
		virtual public Graphic GetGraphic()
		{
			if( m_Graphic == null )
			{
				m_Graphic = gameObject.GetComponent<Graphic>() ;
			}
			return m_Graphic ;
		}

		//----------

		// キャッシュ	
		protected EventTrigger m_EventTrigger = null ;

		/// <summary>
		/// EventTrigger(ショートカット)
		/// </summary>
		virtual public EventTrigger CEventTrigger
		{
			get
			{
				if( m_EventTrigger == null )
				{
					m_EventTrigger = gameObject.GetComponent<EventTrigger>() ;
				}
				return m_EventTrigger ;
			}
		}

		/// <summary>
		/// EventTrigger の有無
		/// </summary>
		public bool IsEventTrigger
		{
			get
			{
				if( CEventTrigger == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					AddEventTrigger() ;
				}
				else
				{
#if UNITY_EDITOR

					if( Application.isPlaying == true )
					{
						RemoveEventTrigger() ;
					}
					else
					{
						m_RemoveEventTrigger = true ;
					}
#else
					RemoveEventTrigger() ;
#endif
				}
			}
		}
		
		/// <summary>
		/// EventTrigger の追加
		/// </summary>
		public void AddEventTrigger()
		{
			if( CEventTrigger != null )
			{
				return ;
			}
		
			m_EventTrigger = gameObject.AddComponent<EventTrigger>() ;
		}
		
#if UNITY_EDITOR
		private bool m_RemoveEventTrigger = false ;
#endif

		/// <summary>
		/// EventTrigger の削除
		/// </summary>
		public void RemoveEventTrigger()
		{
			EventTrigger eventTrigger = CEventTrigger ;
			if( eventTrigger == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( eventTrigger ) ;
			}
			else
			{
				Destroy( eventTrigger ) ;
			}

			m_EventTrigger = null ;
		}

		//----------

		// キャッシュ
		protected UIInteraction m_Interaction = null ;

		/// <summary>
		/// Interaction(ショートカット)
		/// </summary>
		virtual public UIInteraction CInteraction
		{
			get
			{
				if( m_Interaction == null )
				{
					m_Interaction = gameObject.GetComponent<UIInteraction>() ;
				}
				return m_Interaction ;
			}
		}
		
		/// <summary>
		/// Interaction の有無
		/// </summary>
		public bool IsInteraction
		{
			get
			{
				if( CInteraction == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					if( IsInteractionForScrollView == true )
					{
						// スクロールビュー用のインタラクションが既に付いていたら通常のインタラクションは無効
						return ;
					}

					AddInteraction() ;
				}
				else
				{
#if UNITY_EDITOR

					if( Application.isPlaying == true )
					{
						RemoveInteraction() ;
					}
					else
					{
						m_RemoveInteraction = true ;
					}
#else
					RemoveInteraction() ;
#endif
				}
			}
		}
		
		/// <summary>
		/// Interaction の追加
		/// </summary>
		public void AddInteraction()
		{
			if( CInteraction != null )
			{
				return ;
			}
			m_Interaction = gameObject.AddComponent<UIInteraction>() ;
			AddInteractionCallback() ;
		}

#if UNITY_EDITOR
		private bool m_RemoveInteraction = false ;
#endif

		/// <summary>
		/// Interaction の削除
		/// </summary>
		public void RemoveInteraction()
		{
			UIInteraction interaction = CInteraction ;
			if( interaction == null )
			{
				return ;
			}
		
//			RemoveInteractionCallback() ;

			if( Application.isPlaying == false )
			{
				DestroyImmediate( interaction ) ;
			}
			else
			{
				Destroy( interaction ) ;
			}

			m_Interaction = null ;

			m_HoverAtFirst = false ;	// 消しておかないと Hover で悪さする
		}

		//----------

		// キャッシュ
		protected UIInteractionForScrollView m_InteractionForScrollView = null ;

		/// <summary>
		/// Interaction(ショートカット)
		/// </summary>
		virtual public UIInteractionForScrollView CInteractionForScrollView
		{
			get
			{
				if( m_InteractionForScrollView == null )
				{
					m_InteractionForScrollView = gameObject.GetComponent<UIInteractionForScrollView>() ;
				}
				return m_InteractionForScrollView ;
			}
		}
		
		/// <summary>
		/// InteractionWithoutDrag の有無
		/// </summary>
		public bool IsInteractionForScrollView
		{
			get
			{
				if( CInteractionForScrollView == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					if( IsInteraction == true )
					{
						// 通常のインタラクションが付いていたら削除する
						IsInteraction  = false ; 
					}

					AddInteractionForScrollView() ;
				}
				else
				{
#if UNITY_EDITOR

					if( Application.isPlaying == true )
					{
						RemoveInteractionForScrollView() ;
					}
					else
					{
						m_RemoveInteractionForScrollView = true ;
					}
#else
					RemoveInteractionForScrollView() ;
#endif
				}
			}
		}
		
		/// <summary>
		/// InteractionWithoutDrag の追加
		/// </summary>
		public void AddInteractionForScrollView()
		{
			if( CInteractionForScrollView != null )
			{
				return ;
			}
			m_InteractionForScrollView = gameObject.AddComponent<UIInteractionForScrollView>() ;
			AddInteractionForScrollViewCallback() ;
		}

#if UNITY_EDITOR
		private bool m_RemoveInteractionForScrollView = false ;
#endif

		/// <summary>
		/// Interaction の削除
		/// </summary>
		public void RemoveInteractionForScrollView()
		{
			UIInteractionForScrollView interactionForScrollView = CInteractionForScrollView ;
			if( interactionForScrollView == null )
			{
				return ;
			}
		
			RemoveInteractionForScrollViewCallback() ;

			if( Application.isPlaying == false )
			{
				DestroyImmediate( interactionForScrollView ) ;
			}
			else
			{
				Destroy( interactionForScrollView ) ;
			}

			m_InteractionForScrollView = null ;

			m_HoverAtFirst = false ;	// 消しておかないと Hover で悪さする
		}

		//----------

		// キャッシュ
		protected UITransition m_Transition = null ;

		/// <summary>
		/// Transition(ショートカット)
		/// </summary>
		virtual public UITransition CTransition
		{
			get
			{
				if( m_Transition == null )
				{
					m_Transition = gameObject.GetComponent<UITransition>() ;
				}
				return m_Transition ;
			}
		}
		
		/// <summary>
		/// Transition の有無
		/// </summary>
		public bool IsTransition
		{
			get
			{
				if( CTransition == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					AddTransition() ;
				}
				else
				{
#if UNITY_EDITOR

					if( Application.isPlaying == true )
					{
						RemoveTransition() ;
					}
					else
					{
						m_RemoveTransition = true ;
					}
#else
					RemoveTransition() ;
#endif
				}
			}
		}
		
		/// <summary>
		/// Transition の追加
		/// </summary>
		public void AddTransition()
		{
			if( CTransition != null )
			{
				return ;
			}
		
			m_Transition = gameObject.AddComponent<UITransition>() ;
		}

#if UNITY_EDITOR
		private bool m_RemoveTransition = false ;
#endif

		/// <summary>
		/// Transition の削除
		/// </summary>
		public void RemoveTransition()
		{
			UITransition transition = CTransition ;
			if( transition == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( transition ) ;
			}
			else
			{
				Destroy( transition ) ;
			}

			m_Transition = null ;
		}

		//----------

		// キャッシュ
		protected UITransitionForScrollView m_TransitionForScrollView = null ;

		/// <summary>
		/// TransitionForScrollView(ショートカット)
		/// </summary>
		virtual public UITransitionForScrollView CTransitionForScrollView
		{
			get
			{
				if( m_TransitionForScrollView == null )
				{
					m_TransitionForScrollView = gameObject.GetComponent<UITransitionForScrollView>() ;
				}
				return m_TransitionForScrollView ;
			}
		}
		
		/// <summary>
		/// TransitionForScrollView の有無
		/// </summary>
		public bool IsTransitionForScrollView
		{
			get
			{
				if( CTransitionForScrollView == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					AddTransitionForScrollView() ;
				}
				else
				{
#if UNITY_EDITOR

					if( Application.isPlaying == true )
					{
						RemoveTransitionForScrollView() ;
					}
					else
					{
						m_RemoveTransitionForScrollView = true ;
					}
#else
					RemoveTransitionForScrollView() ;
#endif
				}
			}
		}
		
		/// <summary>
		/// TransitionForScrollView の追加
		/// </summary>
		public void AddTransitionForScrollView()
		{
			if( CTransitionForScrollView != null )
			{
				return ;
			}
		
			m_TransitionForScrollView = gameObject.AddComponent<UITransitionForScrollView>() ;
		}

#if UNITY_EDITOR
		private bool m_RemoveTransitionForScrollView = false ;
#endif

		/// <summary>
		/// TransitionForScrollView の削除
		/// </summary>
		public void RemoveTransitionForScrollView()
		{
			UITransitionForScrollView transitionForScrollView = CTransitionForScrollView ;
			if( transitionForScrollView == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( transitionForScrollView ) ;
			}
			else
			{
				Destroy( transitionForScrollView ) ;
			}

			m_TransitionForScrollView = null ;
		}

		//----------
		
		// キャッシュ
		private RawImage m_RawImage = null ;

		/// <summary>
		/// RawImage(ショートカット)
		/// </summary>
		virtual public RawImage CRawImage
		{
			get
			{
				if( m_RawImage == null )
				{
					m_RawImage = gameObject.GetComponent<RawImage>() ;
				}
				return m_RawImage ;
			}
		}
	
		//----------

		// キャッシュ
		private Image m_Image = null ;

		/// <summary>
		/// Image(ショートカット)
		/// </summary>
		virtual public Image CImage
		{
			get
			{
				if( m_Image == null )
				{
					m_Image = gameObject.GetComponent<Image>() ;
				}
				return m_Image ;
			}
		}
		
		//----------

		// キャッシュ
		private Button m_Button = null ;

		/// <summary>
		/// Button(ショートカット)
		/// </summary>
		virtual public Button CButton
		{
			get
			{
				if( m_Button == null )
				{
					m_Button = gameObject.GetComponent<Button>() ;
				}
				return m_Button ;
			}
		}

		//----------

		// キャッシュ
		private UIButtonGroup m_ButtonGroup = null ;

		/// <summary>
		/// ButtonGroup(ショートカット)
		/// </summary>
		virtual public UIButtonGroup CButtonGroup
		{
			get
			{
				if( m_ButtonGroup == null )
				{
					m_ButtonGroup = gameObject.GetComponent<UIButtonGroup>() ;
				}
				return m_ButtonGroup ;
			}
		}
		
		/// <summary>
		/// ButtonGroup の有無
		/// </summary>
		public bool IsButtonGroup
		{
			get
			{
				if( CButtonGroup == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					AddButtonGroup() ;
				}
				else
				{
#if UNITY_EDITOR
					if( Application.isPlaying == true )
					{
						RemoveButtonGroup() ;
					}
					else
					{
						m_RemoveButtonGroup = true ;
					}
#else
					RemoveButtonGroup() ;
#endif
				}
			}
		}
		
		/// <summary>
		/// ButtonGroup の追加
		/// </summary>
		public void AddButtonGroup()
		{
			if( CButtonGroup != null )
			{
				return ;
			}

			UIButtonGroup buttonGroup ;
		
			buttonGroup = gameObject.AddComponent<UIButtonGroup>() ;
			buttonGroup.AllowSwitchOff = false ;
		}

#if UNITY_EDITOR
		private bool m_RemoveButtonGroup = false ;
#endif

		/// <summary>
		/// ButtonGroup の削除
		/// </summary>
		public void RemoveButtonGroup()
		{
			UIButtonGroup buttonGroup = CButtonGroup ;
			if( buttonGroup == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( buttonGroup ) ;
			}
			else
			{
				Destroy( buttonGroup ) ;
			}

			m_ButtonGroup = null ;
		}

		//----------

		// キャッシュ
		private Toggle m_Toggle = null ;

		/// <summary>
		/// Toggle(ショートカット)
		/// </summary>
		virtual public Toggle CToggle
		{
			get
			{
				if( m_Toggle == null )
				{
					m_Toggle = gameObject.GetComponent<Toggle>() ;
				}
				return m_Toggle ;
			}
		}

		//----------

		// キャッシュ
		private ToggleGroup m_ToggleGroup = null ;
		private UIToggleGroup m_UIToggleGroup = null ;

		/// <summary>
		/// ToggleGroup(ショートカット)
		/// </summary>
		virtual public ToggleGroup CToggleGroup
		{
			get
			{
				if( m_ToggleGroup == null )
				{
					m_ToggleGroup = gameObject.GetComponent<ToggleGroup>() ;
				}
				return m_ToggleGroup ;
			}
		}
		
		/// <summary>
		/// ToggleGroup の有無
		/// </summary>
		public bool IsToggleGroup
		{
			get
			{
				if( CToggleGroup == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					AddToggleGroup() ;
				}
				else
				{
#if UNITY_EDITOR

					if( Application.isPlaying == true )
					{
						RemoveToggleGroup() ;
					}
					else
					{
						m_RemoveToggleGroup = true ;
					}

#else

					RemoveToggleGroup() ;

#endif
				}
			}
		}
		
		/// <summary>
		/// ToggleGroup の追加
		/// </summary>
		public void AddToggleGroup()
		{
			if( CToggleGroup != null )
			{
				return ;
			}

			ToggleGroup toggleGroup ;
		
			toggleGroup = gameObject.AddComponent<ToggleGroup>() ;
			toggleGroup.allowSwitchOff = false ;

			m_UIToggleGroup = new UIToggleGroup( toggleGroup ) ;
		}

#if UNITY_EDITOR
		private bool m_RemoveToggleGroup = false ;
#endif

		/// <summary>
		/// ToggleGroup の削除
		/// </summary>
		public void RemoveToggleGroup()
		{
			ToggleGroup toggleGroup = CToggleGroup ;
			if( toggleGroup == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( toggleGroup ) ;
			}
			else
			{
				Destroy( toggleGroup ) ;
			}

			m_ToggleGroup = null ;
			m_UIToggleGroup = null ;
		}

		public UIToggleGroup GetToggleGroup()
		{
			if( m_UIToggleGroup == null )
			{
				if( CToggleGroup == null )
				{
					return null ;
				}

				m_UIToggleGroup = new UIToggleGroup( CToggleGroup ) ;
			}

			return m_UIToggleGroup ;
		}

		//----------
		
		// キャッシュ
		private Slider m_Slider = null ;

		/// <summary>
		/// Slider(ショートカット)
		/// </summary>
		virtual public Slider CSlider
		{
			get
			{
				if( m_Slider == null )
				{
					m_Slider = gameObject.GetComponent<Slider>() ;
				}
				return m_Slider ;
			}
		}
	
		//----------

		// キャッシュ
		private ScrollRectWrapper m_ScrollRect = null ;

		/// <summary>
		/// ScrollRect(ショートカット)
		/// </summary>
		virtual public ScrollRectWrapper CScrollRect
		{
			get
			{
				if( m_ScrollRect == null )
				{
					m_ScrollRect = gameObject.GetComponent<ScrollRectWrapper>() ;
				}
				return m_ScrollRect ;
			}
		}

		//----------
		
		// キャッシュ
		private ScrollbarWrapper m_Scrollbar = null ;

		/// <summary>
		/// Scrollbar(ショートカット)
		/// </summary>
		virtual public Scrollbar CScrollbar
		{
			get
			{
				if( m_Scrollbar == null )
				{
					m_Scrollbar = gameObject.GetComponent<ScrollbarWrapper>() ;
				}
				return m_Scrollbar ;
			}
		}
	
		//----------

		// キャッシュ
		private Dropdown m_Dropdown ;

		/// <summary>
		/// Dropdown(ショートカット)
		/// </summary>
		virtual public Dropdown CDropdown
		{
			get
			{
				if( m_Dropdown == null )
				{
					m_Dropdown = gameObject.GetComponent<Dropdown>() ;
				}
				return m_Dropdown ;
			}
		}

		//----------

		// キャッシュ
		private TMP_Dropdown m_TMP_Dropdown ;

		/// <summary>
		/// TMP_Dropdown(ショートカット)
		/// </summary>
		virtual public TMP_Dropdown CTMP_Dropdown
		{
			get
			{
				if( m_TMP_Dropdown == null )
				{
					m_TMP_Dropdown = gameObject.GetComponent<TMP_Dropdown>() ;
				}
				return m_TMP_Dropdown ;
			}
		}
		
		//----------
		
		// キャッシュ
		private Text m_Text = null ;

		/// <summary>
		/// Text(ショートカット)
		/// </summary>
		virtual public Text CText
		{
			get
			{
				if( m_Text == null )
				{
					m_Text = gameObject.GetComponent<Text>() ;
				}
				return m_Text ;
			}
		}
	
		//----------
		
		// キャッシュ
		private RichText m_RichText = null ;

		/// <summary>
		/// RichText(ショートカット)
		/// </summary>
		virtual public RichText CRichText
		{
			get
			{
				if( m_RichText == null )
				{
					m_RichText = gameObject.GetComponent<RichText>() ;
				}
				return m_RichText ;
			}
		}

		//----------

		// キャッシュ
		private TextMeshProUGUI m_TextMesh = null ;

		/// <summary>
		/// Text(ショートカット)
		/// </summary>
		virtual public TextMeshProUGUI CTextMesh
		{
			get
			{
				if( m_TextMesh == null )
				{
					m_TextMesh = gameObject.GetComponent<TextMeshProUGUI>() ;
				}
				return m_TextMesh ;
			}
		}

		//----------

		// キャッシュ
		private HorizontalLayoutGroup m_HorizontalLayoutGroup = null ;

		/// <summary>
		/// HorizontalLayoutGroup(ショートカット)
		/// </summary>
		virtual public HorizontalLayoutGroup CHorizontalLayoutGroup
		{
			get
			{
				if( m_HorizontalLayoutGroup == null )
				{
					m_HorizontalLayoutGroup = gameObject.GetComponent<HorizontalLayoutGroup>() ;
				}
				return m_HorizontalLayoutGroup ;
			}
		}
		
		/// <summary>
		/// HorizontalLayoutGroup の有無
		/// </summary>
		public bool IsHorizontalLayoutGroup
		{
			get
			{
				if( CHorizontalLayoutGroup == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					AddHorizontalLayoutGroup() ;
				}
				else
				{
#if UNITY_EDITOR
					if( Application.isPlaying == true )
					{
						RemoveHorizontalLayoutGroup() ;
					}
					else
					{
						m_RemoveHorizontalLayoutGroup = true ;
					}
#else
					RemoveHorizontalLayoutGroup() ;
#endif
				}
			}
		}
		
		/// <summary>
		/// HorizontalLayoutGroup の追加
		/// </summary>
		public void AddHorizontalLayoutGroup()
		{
			if( CHorizontalLayoutGroup != null )
			{
				return ;
			}
		
			HorizontalLayoutGroup horizontalLayoutGroup ;
		
			horizontalLayoutGroup = gameObject.AddComponent<HorizontalLayoutGroup>() ;
			horizontalLayoutGroup.childAlignment = TextAnchor.MiddleCenter ;
			horizontalLayoutGroup.childControlWidth			= false ;
			horizontalLayoutGroup.childControlHeight		= false ;
			horizontalLayoutGroup.childScaleWidth			= false ;
			horizontalLayoutGroup.childScaleHeight			= false ;
			horizontalLayoutGroup.childForceExpandWidth		= false ;
			horizontalLayoutGroup.childForceExpandHeight	= false ;
		}
		
#if UNITY_EDITOR
		private bool m_RemoveHorizontalLayoutGroup = false ;
#endif

		/// <summary>
		/// HorizontalLayoutGroup の削除
		/// </summary>
		public void RemoveHorizontalLayoutGroup()
		{
			HorizontalLayoutGroup horizontalLayoutGroup = CHorizontalLayoutGroup ;
			if( horizontalLayoutGroup == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( horizontalLayoutGroup ) ;
			}
			else
			{
				Destroy( horizontalLayoutGroup ) ;
			}

			m_HorizontalLayoutGroup = null ;
		}
		
		//----------

		// キャッシュ
		private VerticalLayoutGroup m_VerticalLayoutGroup = null ;

		/// <summary>
		/// VerticalLayoutGroup(ショートカット)
		/// </summary>
		virtual public VerticalLayoutGroup CVerticalLayoutGroup
		{
			get
			{
				if( m_VerticalLayoutGroup == null )
				{
					m_VerticalLayoutGroup = gameObject.GetComponent<VerticalLayoutGroup>() ;
				}
				return m_VerticalLayoutGroup ;
			}
		}
		
		/// <summary>
		/// VerticalLayoutGroup の有無
		/// </summary>
		public bool IsVerticalLayoutGroup
		{
			get
			{
				if( CVerticalLayoutGroup == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					AddVerticalLayoutGroup() ;
				}
				else
				{
#if UNITY_EDITOR
					if( Application.isPlaying == true )
					{
						RemoveVerticalLayoutGroup() ;
					}
					else
					{
						m_RemoveVerticalLayoutGroup = true ;
					}
#else
					RemoveVerticalLayoutGroup() ;
#endif
				}
			}
		}
		
		/// <summary>
		/// VerticalLayoutGroup の追加
		/// </summary>
		public void AddVerticalLayoutGroup()
		{
			if( CVerticalLayoutGroup != null )
			{
				return ;
			}
		
			VerticalLayoutGroup verticalLayoutGroup ;
		
			verticalLayoutGroup = gameObject.AddComponent<VerticalLayoutGroup>() ;
			verticalLayoutGroup.childAlignment = TextAnchor.MiddleCenter ;
			verticalLayoutGroup.childControlWidth		= false ;
			verticalLayoutGroup.childControlHeight		= false ;
			verticalLayoutGroup.childScaleWidth			= false ;
			verticalLayoutGroup.childScaleHeight		= false ;
			verticalLayoutGroup.childForceExpandWidth	= false ;
			verticalLayoutGroup.childForceExpandHeight	= false ;
		}
		
#if UNITY_EDITOR
		private bool m_RemoveVerticalLayoutGroup = false ;
#endif

		/// <summary>
		/// VerticalLayoutGroup の削除
		/// </summary>
		public void RemoveVerticalLayoutGroup()
		{
			VerticalLayoutGroup verticalLayoutGroup = CVerticalLayoutGroup ;
			if( verticalLayoutGroup == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( verticalLayoutGroup ) ;
			}
			else
			{
				Destroy( verticalLayoutGroup ) ;
			}

			m_VerticalLayoutGroup = null ;
		}

		//----------

		// キャッシュ
		private GridLayoutGroup m_GridLayoutGroup = null ;

		/// <summary>
		/// GridLayoutGroup(ショートカット)
		/// </summary>
		virtual public GridLayoutGroup CGridLayoutGroup
		{
			get
			{
				if( m_GridLayoutGroup == null )
				{
					m_GridLayoutGroup = gameObject.GetComponent<GridLayoutGroup>() ;
				}
				return m_GridLayoutGroup ;
			}
		}
		
		/// <summary>
		/// GridLayoutGroup の有無
		/// </summary>
		public bool IsGridLayoutGroup
		{
			get
			{
				if( CGridLayoutGroup == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					AddGridLayoutGroup() ;
				}
				else
				{
#if UNITY_EDITOR

					if( Application.isPlaying == true )
					{
						RemoveGridLayoutGroup() ;
					}
					else
					{
						m_RemoveGridLayoutGroup = true ;
					}
#else
					RemoveGridLayoutGroup() ;
#endif
				}
			}
		}
		
		/// <summary>
		/// GridLayoutGroup の追加
		/// </summary>
		public void AddGridLayoutGroup()
		{
			if( CGridLayoutGroup != null )
			{
				return ;
			}
		
			GridLayoutGroup gridLayoutGroup ;
		
			gridLayoutGroup = gameObject.AddComponent<GridLayoutGroup>() ;
			gridLayoutGroup.childAlignment = TextAnchor.MiddleCenter ;
		}
		
#if UNITY_EDITOR
		private bool m_RemoveGridLayoutGroup = false ;
#endif

		/// <summary>
		/// GridLayoutGroup の削除
		/// </summary>
		public void RemoveGridLayoutGroup()
		{
			GridLayoutGroup gridLayoutGroup = CGridLayoutGroup ;
			if( gridLayoutGroup == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( gridLayoutGroup ) ;
			}
			else
			{
				Destroy( gridLayoutGroup ) ;
			}

			m_GridLayoutGroup = null ;
		}

		//----------

		// キャッシュ
		private ContentSizeFitter m_ContentSizeFitter = null ;

		/// <summary>
		/// ContentSizeFitter(ショートカット)
		/// </summary>
		virtual public ContentSizeFitter CContentSizeFitter
		{
			get
			{
				if( m_ContentSizeFitter == null )
				{
					m_ContentSizeFitter = gameObject.GetComponent<ContentSizeFitter>() ;
				}
				return m_ContentSizeFitter ;
			}
		}
		
		/// <summary>
		/// ContentSizeFitter の有無
		/// </summary>
		public bool IsContentSizeFitter
		{
			get
			{
				if( CContentSizeFitter == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					AddContentSizeFitter() ;
				}
				else
				{
#if UNITY_EDITOR
					if( Application.isPlaying == true )
					{
						RemoveContentSizeFitter() ;
					}
					else
					{
						m_RemoveContentSizeFitter = true ;
					}
#else
					RemoveContentSizeFitter() ;
#endif
				}
			}
		}
		
		/// <summary>
		/// ContentSizeFitter の追加
		/// </summary>
		public void AddContentSizeFitter()
		{
			if( CContentSizeFitter != null )
			{
				return ;
			}
		
			ContentSizeFitter contentSizeFitter ;
		
			contentSizeFitter = gameObject.AddComponent<ContentSizeFitter>() ;
			contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize ;
			contentSizeFitter.verticalFit   = ContentSizeFitter.FitMode.PreferredSize ;
		}
		
#if UNITY_EDITOR
		private bool m_RemoveContentSizeFitter = false ;
#endif

		/// <summary>
		/// ContentSizeFitter の削除
		/// </summary>
		public void RemoveContentSizeFitter()
		{
			ContentSizeFitter contentSizeFitter = CContentSizeFitter ;
			if( contentSizeFitter == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( contentSizeFitter ) ;
			}
			else
			{
				Destroy( contentSizeFitter ) ;
			}

			m_ContentSizeFitter = null ;
		}
		
		//----------
		
		// キャッシュ
		private LayoutElement m_LayoutElement = null ;

		/// <summary>
		/// LayoutElement(ショートカット)
		/// </summary>
		virtual public LayoutElement CLayoutElement
		{
			get
			{
				if( m_LayoutElement == null )
				{
					m_LayoutElement = gameObject.GetComponent<LayoutElement>() ;
				}
				return m_LayoutElement ;
			}
		}

		/// <summary>
		/// LayoutElement の有無
		/// </summary>
		public bool IsLayoutElement
		{
			get
			{
				if( CLayoutElement == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					AddLayoutElement() ;
				}
				else
				{
#if UNITY_EDITOR

					if( Application.isPlaying == true )
					{
						RemoveLayoutElement() ;
					}
					else
					{
						m_RemoveLayoutElement = true ;
					}
#else
					RemoveLayoutElement() ;
#endif
				}
			}
		}
		
		/// <summary>
		/// LayoutElement の追加
		/// </summary>
		public void AddLayoutElement()
		{
			if( CLayoutElement != null )
			{
				return ;
			}
		
			LayoutElement layoutElement ;
		
			layoutElement = gameObject.AddComponent<LayoutElement>() ;
			layoutElement.ignoreLayout		= false ;
			layoutElement.minWidth			= -1 ;
			layoutElement.minHeight			= -1 ;
			layoutElement.preferredWidth	= Width ;
			layoutElement.preferredHeight	= Height ;
			layoutElement.flexibleWidth		= -1 ;
			layoutElement.flexibleHeight	= -1 ;
			layoutElement.layoutPriority	=  1 ;
		}
		
#if UNITY_EDITOR
		private bool m_RemoveLayoutElement = false ;
#endif

		/// <summary>
		/// LayoutElement の削除
		/// </summary>
		public void RemoveLayoutElement()
		{
			LayoutElement layoutElement = CLayoutElement ;
			if( layoutElement == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( layoutElement ) ;
			}
			else
			{
				Destroy( layoutElement ) ;
			}

			m_LayoutElement = null ;
		}
		
		//----------

		// キャッシュ
		private Mask m_Mask = null ;

		/// <summary>
		/// Mask(ショートカット)
		/// </summary>
		virtual public Mask CMask
		{
			get
			{
				if( m_Mask == null )
				{
					m_Mask = gameObject.GetComponent<Mask>() ;
				}
				return m_Mask ;
			}
		}
		
		/// <summary>
		/// Mask の有無
		/// </summary>
		public bool IsMask
		{
			get
			{
				if( CMask == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					AddMask() ;
				}
				else
				{
#if UNITY_EDITOR

					if( Application.isPlaying == true )
					{
						RemoveMask() ;
					}
					else
					{
						m_RemoveMask = true ;
					}

#else

					RemoveMask() ;

#endif
				}
			}
		}
		
		/// <summary>
		/// Mask の追加
		/// </summary>
		public void AddMask()
		{
			if( CMask != null )
			{
				return ;
			}
		
			Mask mask ;
		
			mask = gameObject.AddComponent<Mask>() ;
			mask.showMaskGraphic = true ;
		}

#if UNITY_EDITOR
		private bool m_RemoveMask = false ;
#endif

		/// <summary>
		/// Mask の削除
		/// </summary>
		public void RemoveMask()
		{
			Mask mask = CMask ;
			if( mask == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( mask ) ;
			}
			else
			{
				Destroy( mask ) ;
			}
		}

		//----------

		// キャッシュ
		private RectMask2D m_RectMask2D = null ;

		/// <summary>
		/// RectMask2D(ショートカット)
		/// </summary>
		virtual public RectMask2D CRectMask2D
		{
			get
			{
				if( m_RectMask2D == null )
				{
					m_RectMask2D = gameObject.GetComponent<RectMask2D>() ;
				}
				return m_RectMask2D ;
			}
		}
		
		/// <summary>
		/// RectMask2D の有無
		/// </summary>
		public bool IsRectMask2D
		{
			get
			{
				if( CRectMask2D == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					AddRectMask2D() ;
				}
				else
				{
#if UNITY_EDITOR

					if( Application.isPlaying == true )
					{
						RemoveRectMask2D() ;
					}
					else
					{
						m_RemoveRectMask2D = true ;
					}

#else

					RemoveRectMask2D() ;

#endif
				}
			}
		}
		
		/// <summary>
		/// RectMask2D の追加
		/// </summary>
		public void AddRectMask2D()
		{
			if( CRectMask2D != null )
			{
				return ;
			}

			gameObject.AddComponent<RectMask2D>() ;

//			RectMask2D rectMask2D ;
		
//			rectMask2D = gameObject.AddComponent<RectMask2D>() ;
//			rectMask2D.showMaskGraphic = true ;
		}

#if UNITY_EDITOR
		private bool m_RemoveRectMask2D = false ;
#endif

		/// <summary>
		/// RectMask2D の削除
		/// </summary>
		public void RemoveRectMask2D()
		{
			RectMask2D rectMask2D = CRectMask2D ;
			if( rectMask2D == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( rectMask2D ) ;
			}
			else
			{
				Destroy( rectMask2D ) ;
			}
		}

		//----------

		// キャッシュ
		private UIAlphaMaskWindow m_AlphaMaskWindow = null ;

		/// <summary>
		/// AlphaMaskWindow(ショートカット)
		/// </summary>
		virtual public UIAlphaMaskWindow CAlphaMaskWindow
		{
			get
			{
				if( m_AlphaMaskWindow == null )
				{
					m_AlphaMaskWindow = gameObject.GetComponent<UIAlphaMaskWindow>() ;
				}
				return m_AlphaMaskWindow ;
			}
		}
		
		/// <summary>
		/// AlphaMaskWindow の有無
		/// </summary>
		public bool IsAlphaMaskWindow
		{
			get
			{
				if( CAlphaMaskWindow == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					AddAlphaMaskWindow() ;
				}
				else
				{
#if UNITY_EDITOR

					if( Application.isPlaying == true )
					{
						RemoveAlphaMaskWindow() ;
					}
					else
					{
						m_RemoveAlphaMaskWindow = true ;
					}

#else

					RemoveAlphaMaskWindow() ;

#endif
				}
			}
		}
		
		/// <summary>
		/// AlphaMaskWindow の追加
		/// </summary>
		public void AddAlphaMaskWindow()
		{
			if( CAlphaMaskWindow != null )
			{
				return ;
			}
		
//			UIAlphaMaskWindow alphaMaskWindow ;
		
//			alphaMaskWindow = gameObject.AddComponent<UIAlphaMaskWindow>() ;
			gameObject.AddComponent<UIAlphaMaskWindow>() ;
		}

#if UNITY_EDITOR
		private bool m_RemoveAlphaMaskWindow = false ;
#endif

		/// <summary>
		/// AlphaMaskWindow の削除
		/// </summary>
		public void RemoveAlphaMaskWindow()
		{
			UIAlphaMaskWindow alphaMaskWindow = CAlphaMaskWindow ;
			if( alphaMaskWindow == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( alphaMaskWindow ) ;
			}
			else
			{
				Destroy( alphaMaskWindow ) ;
			}
		}

		//----------

		// キャッシュ
		private UIAlphaMaskTarget m_AlphaMaskTarget = null ;

		/// <summary>
		/// AlphaMaskTarget(ショートカット)
		/// </summary>
		virtual public UIAlphaMaskTarget CAlphaMaskTarget
		{
			get
			{
				if( m_AlphaMaskTarget == null )
				{
					m_AlphaMaskTarget = gameObject.GetComponent<UIAlphaMaskTarget>() ;
				}
				return m_AlphaMaskTarget ;
			}
		}
		
		/// <summary>
		/// AlphaMaskTarget の有無
		/// </summary>
		public bool IsAlphaMaskTarget
		{
			get
			{
				if( CAlphaMaskTarget == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					AddAlphaMaskTarget() ;
				}
				else
				{
#if UNITY_EDITOR

					if( Application.isPlaying == true )
					{
						RemoveAlphaMaskTarget() ;
					}
					else
					{
						m_RemoveAlphaMaskTarget = true ;
					}

#else

					RemoveAlphaMaskTarget() ;

#endif
				}
			}
		}
		
		/// <summary>
		/// AlphaMaskTarget の追加
		/// </summary>
		public void AddAlphaMaskTarget()
		{
			if( CAlphaMaskTarget != null )
			{
				return ;
			}
		
//			UIAlphaMaskTarget tAlphaMaskTarget ;
		
//			alphaMaskTarget = gameObject.AddComponent<UIAlphaMaskTarget>() ;
			gameObject.AddComponent<UIAlphaMaskTarget>() ;
		}

#if UNITY_EDITOR
		private bool m_RemoveAlphaMaskTarget = false ;
#endif

		/// <summary>
		/// AlphaMaskTarget の削除
		/// </summary>
		public void RemoveAlphaMaskTarget()
		{
			UIAlphaMaskTarget alphaMaskTarget = CAlphaMaskTarget ;
			if( alphaMaskTarget == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( alphaMaskTarget ) ;
			}
			else
			{
				Destroy( alphaMaskTarget ) ;
			}
		}

		//----------

		/// <summary>
		/// グラフィックコンポーネントがアタッチされているかどうか
		/// </summary>
		public bool IsGraphic
		{
			get
			{
				if( GetComponent<Graphic>() == null )
				{
					return false ;
				}

				return true ;
			}
		}

		//----------
		
		// キャッシュ
		private InputFieldPlus m_InputField = null ;

		/// <summary>
		/// InputField(ショートカット)
		/// </summary>
		virtual public InputFieldPlus CInputField
		{
			get
			{
				if( m_InputField == null )
				{
					m_InputField = gameObject.GetComponent<InputFieldPlus>() ;
				}
				return m_InputField ;
			}
		}
		
		//----------
		
		// キャッシュ
		private TMP_InputFieldPlus m_TMP_InputField = null ;

		/// <summary>
		/// InputField(ショートカット)
		/// </summary>
		virtual public TMP_InputFieldPlus CTMP_InputField
		{
			get
			{
				if( m_TMP_InputField == null )
				{
					m_TMP_InputField = gameObject.GetComponent<TMP_InputFieldPlus>() ;
				}
				return m_TMP_InputField ;
			}
		}
	
		//----------
		
		// キャッシュ
		private Shadow m_Shadow = null ;

		/// <summary>
		/// Shadow(ショートカット)
		/// </summary>
		virtual public Shadow CShadow
		{
			get
			{
				if( m_Shadow == null )
				{
					m_Shadow = gameObject.GetComponent<Shadow>() ;
				}
				return m_Shadow ;
			}
		}
		
		/// <summary>
		/// Shadow の有無
		/// </summary>
		public bool IsShadow
		{
			get
			{
				Shadow shadow = CShadow ;
				if( shadow == null )
				{
					return false ;
				}
				else
				{
					if( shadow is Shadow == true && shadow is Outline == false )
					{
						return true ;
					}
					else
					{
						return false ;
					}
				}
			}
			set
			{
				if( value == true )
				{
					AddShadow() ;
				}
				else
				{
#if UNITY_EDITOR

					if( Application.isPlaying == true )
					{
						RemoveShadow() ;
					}
					else
					{
						m_RemoveShadow = true ;
					}

#else

					RemoveShadow() ;

#endif
				}
			}
		}

		/// <summary>
		/// Shadow の追加
		/// </summary>
		public void AddShadow()
		{
			if( CShadow != null )
			{
				return ;
			}
		
			Shadow shadow ;
		
			shadow = gameObject.AddComponent<Shadow>() ;
			shadow.effectColor = ARGB( 0xFF000000 ) ;
			shadow.effectDistance = new Vector2(  1, -1 ) ;
			shadow.useGraphicAlpha = true ;
		}

#if UNITY_EDITOR
		private bool m_RemoveShadow = false ;
#endif

		/// <summary>
		/// Shadow の削除
		/// </summary>
		public void RemoveShadow()
		{
			Shadow shadow = CShadow ;
			if( shadow == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( shadow ) ;
			}
			else
			{
				Destroy( shadow ) ;
			}

			m_Shadow = null ;
		}
		
		//----------
		
		// キャッシュ
		private Outline m_Outline = null ;

		/// <summary>
		/// Outline(ショートカット)
		/// </summary>
		virtual public Outline COutline
		{
			get
			{
				if( m_Outline == null )
				{
					m_Outline = gameObject.GetComponent<Outline>() ;
				}
				return m_Outline ;
			}
		}

		/// <summary>
		/// Outline の有無
		/// </summary>
		public bool IsOutline
		{
			get
			{
				if( COutline == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					AddOutline() ;
				}
				else
				{
#if UNITY_EDITOR
					if( Application.isPlaying == true )
					{
						RemoveOutline() ;
					}
					else
					{
						m_RemoveOutline = true ;
					}
#else
					RemoveOutline() ;
#endif
				}
			}
		}
		
		/// <summary>
		/// Outline の追加
		/// </summary>
		public void AddOutline()
		{
			if( COutline != null )
			{
				return ;
			}
		
			Outline outline ;
		
			outline = gameObject.AddComponent<Outline>() ;
			outline.effectColor = ARGB( 0xFF000000 ) ;
			outline.effectDistance = new Vector2(  1, -1 ) ;
			outline.useGraphicAlpha = true ;
		}

#if UNITY_EDITOR
		private bool m_RemoveOutline = false ;
#endif

		/// <summary>
		/// Outline の削除
		/// </summary>
		public void RemoveOutline()
		{
			Outline outline = COutline ;
			if( outline == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( outline ) ;
			}
			else
			{
				Destroy( outline ) ;
			}

			m_Outline = null ;
		}

		//----------
		
		// キャッシュ
		private UIGradient m_Gradient = null ;

		/// <summary>
		/// Gradient(ショートカット)
		/// </summary>
		virtual public UIGradient CGradient
		{
			get
			{
				if( m_Gradient == null )
				{
					m_Gradient = gameObject.GetComponent<UIGradient>() ;
				}
				return m_Gradient ;
			}
		}
		
		/// <summary>
		/// Gradient の有無
		/// </summary>
		public bool IsGradient
		{
			get
			{
				if( CGradient == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					AddGradient() ;
				}
				else
				{
#if UNITY_EDITOR

					if( Application.isPlaying == true )
					{
						RemoveGradient() ;
					}
					else
					{
						m_RemoveGradient = true ;
					}
#else
					RemoveGradient() ;
#endif
				}
			}
		}
		
		/// <summary>
		/// Gradient の追加
		/// </summary>
		public void AddGradient()
		{
			if( CGradient != null )
			{
				return ;
			}
		
			UIGradient gradient ;
		
			gradient = gameObject.AddComponent<UIGradient>() ;
			gradient.DirectionType = UIGradient.DirectionTypes.Vertical ;
			gradient.Top    = ARGB( 0xFFFFFFFF ) ;
			gradient.Bottom = ARGB( 0xFF3F3F3F ) ;
		}

#if UNITY_EDITOR
		private bool m_RemoveGradient = false ;
#endif

		/// <summary>
		/// Gradient の削除
		/// </summary>
		public void RemoveGradient()
		{
			UIGradient gradient = CGradient ;
			if( gradient == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( gradient ) ;
			}
			else
			{
				Destroy( gradient ) ;
			}
		}

		//----------
		
		// キャッシュ
		private UIInversion m_Inversion = null ;

		/// <summary>
		/// Inversion(ショートカット)
		/// </summary>
		virtual public UIInversion CInversion
		{
			get
			{
				if( m_Inversion == null )
				{
					m_Inversion = gameObject.GetComponent<UIInversion>() ;
				}
				return m_Inversion ;
			}
		}
		
		/// <summary>
		/// Inversion の有無
		/// </summary>
		public bool IsInversion
		{
			get
			{
				if( CInversion == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					AddInversion() ;
				}
				else
				{
#if UNITY_EDITOR

					if( Application.isPlaying == true )
					{
						RemoveInversion() ;
					}
					else
					{
						m_RemoveInversion = true ;
					}
#else
					RemoveInversion() ;
#endif
				}
			}
		}
		
		/// <summary>
		/// Inversion の追加
		/// </summary>
		public void AddInversion()
		{
			if( CInversion != null )
			{
				return ;
			}
		
			UIInversion inversion ;
		
			inversion = gameObject.AddComponent<UIInversion>() ;
			inversion.DirectionType = UIInversion.DirectionTypes.None ;
		}

#if UNITY_EDITOR
		private bool m_RemoveInversion = false ;
#endif

		/// <summary>
		/// Inversion の削除
		/// </summary>
		public void RemoveInversion()
		{
			UIInversion inversion = CInversion ;
			if( inversion == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( inversion ) ;
			}
			else
			{
				Destroy( inversion ) ;
			}
		}

		//----------
		
		// キャッシュ
		private ImageNumber m_ImageNumber = null ;

		/// <summary>
		/// ImageNumber(ショートカット)
		/// </summary>
		virtual public ImageNumber CImageNumber
		{
			get
			{
				if( m_ImageNumber == null )
				{
					m_ImageNumber = gameObject.GetComponent<ImageNumber>() ;
				}
				return m_ImageNumber ;
			}
		}

		//----------
		
		// キャッシュ
		private GridMap m_GridMap = null ;

		/// <summary>
		/// GridMap(ショートカット)
		/// </summary>
		virtual public GridMap CGridMap
		{
			get
			{
				if( m_GridMap == null )
				{
					m_GridMap = gameObject.GetComponent<GridMap>() ;
				}
				return m_GridMap ;
			}
		}
		
		//----------
		
		// キャッシュ
		private ComplexRectangle m_ComplexRectangle = null ;

		/// <summary>
		/// ComplexRectangle(ショートカット)
		/// </summary>
		virtual public ComplexRectangle CComplexRectangle
		{
			get
			{
				if( m_ComplexRectangle == null )
				{
					m_ComplexRectangle = gameObject.GetComponent<ComplexRectangle>() ;
				}
				return m_ComplexRectangle ;
			}
		}

		//----------
		
		// キャッシュ
		private Line m_Line = null ;

		/// <summary>
		/// Line(ショートカット)
		/// </summary>
		virtual public Line CLine
		{
			get
			{
				if( m_Line == null )
				{
					m_Line = gameObject.GetComponent<Line>() ;
				}
				return m_Line ;
			}
		}

		//----------
		
		// キャッシュ
		private Circle m_Circle = null ;

		/// <summary>
		/// Circle(ショートカット)
		/// </summary>
		virtual public Circle CCircle
		{
			get
			{
				if( m_Circle == null )
				{
					m_Circle = gameObject.GetComponent<Circle>() ;
				}
				return m_Circle ;
			}
		}

		//----------
		
		// キャッシュ
		private Arc m_Arc = null ;

		/// <summary>
		/// Arc(ショートカット)
		/// </summary>
		virtual public Arc CArc
		{
			get
			{
				if( m_Arc == null )
				{
					m_Arc = gameObject.GetComponent<Arc>() ;
				}
				return m_Arc ;
			}
		}

		//----------
		
		// キャッシュ
		private Animator m_Animator = null ;

		/// <summary>
		/// Animator(ショートカット)
		/// </summary>
		virtual public Animator CAnimator
		{
			get
			{
				if( m_Animator == null )
				{
					m_Animator = gameObject.GetComponent<Animator>() ;
				}
				return m_Animator ;
			}
		}
		
		/// <summary>
		/// Animator の有無
		/// </summary>
		public bool IsAnimator
		{
			get
			{
				if( CAnimator == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					AddAnimator() ;
				}
				else
				{
#if UNITY_EDITOR

					if( Application.isPlaying == true )
					{
						RemoveAnimator() ;
					}
					else
					{
						m_RemoveAnimator = true ;
					}
#else
					RemoveAnimator() ;
#endif
				}
			}
		}
		
		/// <summary>
		/// Animator の追加
		/// </summary>
		public void AddAnimator()
		{
			if( CAnimator != null )
			{
				return ;
			}
		
			Animator animator ;
		
			animator = gameObject.AddComponent<Animator>() ;
			animator.speed = 1 ;
		}

#if UNITY_EDITOR
		private bool m_RemoveAnimator = false ;
#endif

		/// <summary>
		/// Animator の削除
		/// </summary>
		public void RemoveAnimator()
		{
			Animator animator = CAnimator ;
			if( animator == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( animator ) ;
			}
			else
			{
				Destroy( animator ) ;
			}
		}

		//----------------------------------------------------------------
	
		/// <summary>
		/// 指定の識別子の View を取得する
		/// </summary>
		/// <returns>The view.</returns>
		/// <param name="identity">T identity.</param>
		public UIView FindView( string identity )
		{
			if( Identity == identity )
			{
				// 自身がそうだった
				return this ;
			}
		
			return FindView_Private( gameObject, identity ) ;
		}
	
		private UIView FindView_Private( GameObject go, string identity )
		{
			// 直の子供を確認する
			UIView view ;
			int i, c = go.transform.childCount ;
		
			for( i  = 0 ; i <  c ; i ++ )
			{
				view = go.transform.GetChild( i ).gameObject.GetComponent<UIView>() ;
				if( view != null )
				{
					if( view.Identity == identity )
					{
						// 発見
						return view ;
					}
				}
			}
		
			// 直の子供を再帰的に検査する
			for( i  = 0 ; i <  c ; i ++ )
			{
				view = FindView_Private( go.transform.GetChild( i ).gameObject, identity ) ;
				if( view != null )
				{
					// 発見
					return view ;
				}
			}
		
			// このゲームオブジェクト以下には該当する名前のゲームオブジェクトは発見できなかった
			return null ;
		}
	
		/// <summary>
		/// 指定の識別子の View を取得する
		/// </summary>
		/// <returns>The view.</returns>
		/// <param name="identity">T identity.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T FindView<T>( string identity ) where T : UnityEngine.Component
		{
			T component ;
		
			if( Identity == identity )
			{
				// 自身がそうだった
				component = gameObject.GetComponent<T>() ;
				return component ;
			}
		
			component = FindView_Private<T>( gameObject, identity ) ;
			return component ;
		}
	
		private T FindView_Private<T>( GameObject go, string identity ) where T : UnityEngine.Component
		{
			T component ;
		
			// 直の子供を確認する
			UIView view ;
			int i, c = go.transform.childCount ;
		
			for( i  = 0 ; i <  c ; i ++ )
			{
				view = go.transform.GetChild( i ).gameObject.GetComponent<UIView>() ;
				if( view != null )
				{
					if( view.Identity == identity )
					{
						// 発見
						component = view.gameObject.GetComponent<T>() ;
						return component ;
					}
				}
			}
		
			// 直の子供を再帰的に検査する
			for( i  = 0 ; i <  c ; i ++ )
			{
				component = FindView_Private<T>( go.transform.GetChild( i ).gameObject, identity ) ;
				if( component != null )
				{
					// 発見
					return component ;
				}
			}
		
			// このゲームオブジェクト以下には該当する名前のゲームオブジェクトは発見できなかった
			return null ;
		}
	
		/// <summary>
		/// 指定の識別子の GameObject を取得する
		/// </summary>
		/// <returns>The game object.</returns>
		/// <param name="nodeName">T name.</param>
		public GameObject FindNode( string nodeName, bool isContain = false )
		{
			if( name == nodeName )
			{
				// 自身がそうだった
				return gameObject ;
			}
		
			return FindNode_Private( gameObject, nodeName, isContain ) ;
		}
	
		private GameObject FindNode_Private( GameObject go, string nodeName, bool isContain )
		{
			nodeName = nodeName.ToLower() ;

			Transform childNode ;
			string childNodeName ;
			bool result ;

			GameObject targetGameObject ;

			// 直の子供を再帰的に検査する
			int i, c = go.transform.childCount ;
		
			for( i  = 0 ; i <  c ; i ++ )
			{
				childNode = go.transform.GetChild( i ) ;
				childNodeName = childNode.name.ToLower() ;
				result = false ;
				if( isContain == false && childNodeName == nodeName )
				{
					result = true ;
				}
				else
				if( isContain == true && childNodeName.Contains( nodeName ) == true )
				{
					result = true ;
				}

				if( result == true )
				{
					// 発見
					targetGameObject = childNode.gameObject ;
					if( targetGameObject != null )
					{
						// 発見
						return targetGameObject ;
					}
				}
			
				if( childNode.childCount >  0 )
				{
					targetGameObject = FindNode_Private( childNode.gameObject, nodeName, isContain ) ;
					if( targetGameObject != null )
					{
						// 発見
						return targetGameObject ;
					}
				}
			}

			return null ;
		}
	
		/// <summary>
		/// 指定の識別子の GameObject 内の Component を取得する
		/// </summary>
		/// <returns>The game object.</returns>
		/// <param name="nodeName">T name.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T FindNode<T>( string nodeName, bool isContain = false ) where T : UnityEngine.Component
		{
			T component ;
		
			if( name == nodeName )
			{
				// 自身がそうだった
				component = gameObject.GetComponent<T>() ;
				if( component != null )
				{
					return component ;
				}
			}

			return FindNode_Private<T>( gameObject, nodeName, isContain ) ;
		}
	
		private T FindNode_Private<T>( GameObject go, string nodeName, bool isContain ) where T : UnityEngine.Component
		{
			nodeName = nodeName.ToLower() ;

			Transform childNode ;
			string childNodeName ;
			bool result ;

			T component ;
		
			// 直の子供を再帰的に検査する
			int i, c = go.transform.childCount ;
		
			for( i  = 0 ; i <  c ; i ++ )
			{
				childNode = go.transform.GetChild( i ) ;
				childNodeName = childNode.name.ToLower() ;
				result = false ;

				if( isContain == false && childNodeName == nodeName )
				{
					result = true ;
				}
				else
				if( isContain == true && childNodeName.Contains( nodeName ) == true )
				{
					result = true ;
				}
				
				if( result == true )
				{
					// 発見
					component = childNode.GetComponent<T>() ;
					if( component != null )
					{
						return component ;
					}
				}

				if( childNode.childCount >  0 )
				{
					component = FindNode_Private<T>( childNode.gameObject, nodeName, isContain ) ;
					if( component != null )
					{
						// 発見
						return component ;
					}
				}
			}
		
			// このゲームオブジェクト以下には該当する名前のゲームオブジェクトは発見できなかった
			return default ;
		}
	
		//--------------------------------------------------------------------

		// ライブラリで持つ基本的なインタラクションイベントを登録する

		// 無効なポインター識別子
		protected const int m_UnKnownCode = -1000000000 ;

		// 通常用のコールバックを登録する
		private bool AddInteractionCallback()
		{
			UIInteraction interaction = CInteraction ;
			if( interaction == null )
			{
				return false ;
			}

			RemoveInteractionCallback()	;	// 多重登録にならないように削除しておく
			
			interaction.onPointerEnter	+= OnPointerEnterBasic	;
			interaction.onPointerExit	+= OnPointerExitBasic	;
			interaction.onPointerDown	+= OnPointerDownBasic	;
			interaction.onPointerUp		+= OnPointerUpBasic		;
			interaction.onPointerClick	+= OnPointerClickBasic	;
			interaction.onDrag			+= OnDragBasic			;

			return true ;
		}

		// 通常用のコールバックを解除する
		private bool RemoveInteractionCallback()
		{
			UIInteraction interaction = CInteraction ;
			if( interaction == null )
			{
				return false ;
			}

			interaction.onPointerEnter	-= OnPointerEnterBasic	;
			interaction.onPointerExit	-= OnPointerExitBasic	;
			interaction.onPointerDown	-= OnPointerDownBasic	;
			interaction.onPointerUp		-= OnPointerUpBasic		;
			interaction.onPointerClick	-= OnPointerClickBasic	;
			interaction.onDrag			-= OnDragBasic			;

			return true ;
		}

		// スクロールビュー用のコールバックを登録する
		private bool AddInteractionForScrollViewCallback()
		{
			UIInteractionForScrollView interactionForScrollView = CInteractionForScrollView ;
			if( interactionForScrollView == null )
			{
				return false ;
			}

			RemoveInteractionForScrollViewCallback()	;	// 多重登録にならないように削除しておく
			
			interactionForScrollView.onPointerEnter	+= OnPointerEnterBasic	;
			interactionForScrollView.onPointerExit	+= OnPointerExitBasic	;
			interactionForScrollView.onPointerDown	+= OnPointerDownBasic	;
			interactionForScrollView.onPointerUp	+= OnPointerUpBasic		;
			interactionForScrollView.onPointerClick	+= OnPointerClickBasic	;
//			interactionForScrollView.onDrag			+= OnDragBasic			;

			return true ;
		}


		// スクロールビュー用のコールバックを解除する
		private bool RemoveInteractionForScrollViewCallback()
		{
			UIInteractionForScrollView interactionForScrollView = CInteractionForScrollView ;
			if( interactionForScrollView == null )
			{
				return false ;
			}

			interactionForScrollView.onPointerEnter	-= OnPointerEnterBasic	;
			interactionForScrollView.onPointerExit	-= OnPointerExitBasic	;
			interactionForScrollView.onPointerDown	-= OnPointerDownBasic	;
			interactionForScrollView.onPointerUp	-= OnPointerUpBasic		;
			interactionForScrollView.onPointerClick	-= OnPointerClickBasic	;
//			interactionForScrollView.onDrag			-= OnDragBasic			;
			
			return true ;
		}

		//--------------------------------------------------------------------

		private bool m_HoverAtFirst = false ;

		/// <summary>
		/// ホバー状態
		/// </summary>
		public  bool  IsHover
		{
			get
			{
				return UIEventSystem.IsHovering( gameObject ) ;
			}
		}

		/// <summary>
		/// プレス状態
		/// </summary>
		public  bool  IsPress
		{
			get
			{
				bool isPress = UIEventSystem.IsPressing( gameObject ) ;

				if( isPress == true && m_PressInvalidTime >  0 )
				{
					if( m_PressCountTime == Time.frameCount )
					{
						// 一番最初だけは有効にする
						return true ;
					}
					else
					if( ( Time.realtimeSinceStartup - m_PressStartTime ) <  m_PressInvalidTime )
					{
						return false ;
					}
				}

				return isPress ;
			}
		}

		private float m_PressInvalidTime = 0 ;
		private int   m_PressCountTime = 0 ;
		private float m_PressStartTime = 0 ;

		public float PressInvalidTime
		{
			get
			{
				return m_PressInvalidTime ;
			}
			set
			{
				m_PressInvalidTime = value ;
			}
		}

		protected bool m_Click = false ;

		public bool IsClick
		{
			get
			{
				return m_Click ;
			}
		}

		protected int			m_ClickCountTime = 0 ;

		protected bool			m_ClickState						= false ;
		protected int			m_ClickPointerId					= m_UnKnownCode ;
		protected bool			m_ClickInsideFlag					= false ;
		
		protected bool			m_SmartClickState					= false ;
		protected int			m_SmartClickPointerId				= m_UnKnownCode ;
		protected bool			m_SmartClickInsideFlag				= false ;
		protected Vector2		m_SmartClickBasePosition			= Vector2.zero ;
		protected int			m_SmartClickCount					= 0 ;
		protected float			m_SmartClickBaseTime				= 0 ;

		protected float			m_SmartClickReleaseLimitTime		= 0.5f ;
		protected float			m_SmartClickSecondPressLimitTime	= 0.25f ;	// シングルクリック終了後にこの時間以内に新しいクリックが開始されたらダブルクリック判定が始まる
		protected float			m_SmartClickLimitDistance			= 30.0f ;

		protected float			m_LongPressStartTime				= 0 ;
		protected float			m_LongPressDecisionTime				= 0.75f ;	// 長押しと判定する時間
		protected bool			m_LongPressEnabled					= false ;

		protected int			m_RepeatPressCount					= 0 ;
		protected float			m_RepeatPressStartTime				= 0 ;
		protected float			m_RepeatPressDecisionTime			= 0.75f ;
		protected float			m_RepeatPressIntervalTime			= 0.25f ;


		public enum PointerState
		{
			None = 0,
			Start = 1,
			Moving = 2,
			End = 3,
		}

		/// <summary>
		/// ドラッグ状態
		/// </summary>
		public PointerState DragState
		{
			get
			{
				return m_DragState ;
			}
		}

		protected PointerState	m_DragState						= PointerState.None ;
		protected int			m_DragPointerId					= m_UnKnownCode ;
		protected Vector2		m_DragBasePosition				= Vector2.zero ;


		protected bool			m_FlickState					= false ;
		protected int			m_FlickPointerId				= m_UnKnownCode ;
		protected float			m_FlickDecisionLimitTime		=  0.5f ;
		protected float			m_FlickDecisionDistance			=  60.0f ;
		protected Vector2		m_FlickBasePosition				= Vector2.zero ;
		protected float			m_FlickBaseTime					= 0 ;
		protected bool			m_FlickCheck					= false ;
		protected float			m_FlickLastTime					= 0 ;

		public class TouchState
		{
			public int			Index ;
			public int			Identity ;
			public Vector2		Position ;
			public PointerState	State ;

			public TouchState( int index, int identity, Vector2 position, PointerState state )
			{
				Index		= index ;
				Identity	= identity ;
				Position	= position ;
				State		= state ;
			}
		}

		protected TouchState[]		m_TouchState			= new TouchState[ 10 ] ;
		protected List<TouchState>	m_TouchStateExchange	= new List<TouchState>() ;

		protected bool		m_FromScrollView = false ;
		protected float		m_InteractionLimit = 8.0f ;
		protected Vector2	m_InteractionLimit_StartPoint ;

		//--------------------------------------------------------------------

		// Enter
		// 他の処理が無効化されてしまうため、メソッド内で return を使ってはダメ。本当は処理をメソッド単位に分離した方が良いのだが。
		virtual protected void OnPointerEnterBasic( PointerEventData pointer, bool fromScrollView )
		{
//			Debug.Log( "<color=#FF7F7F>OnPointerEnterBasic:" + name + "</color>" ) ;

			int identity = pointer.pointerId ;
			Vector2 position = GetLocalPosition( pointer ) ;

			m_FromScrollView = fromScrollView ;

			if( m_HoverAtFirst == false )
			{
				// 初めて入った
				m_HoverAtFirst = true ;
				OnHoverInner( PointerState.Start, position ) ;

				// クリック処理
				if( m_ClickState == true && m_ClickPointerId == identity )
				{
					// 中に入ったので以後は有効クリック扱いとなる
					m_ClickInsideFlag = true ;
				}
			}
			else
			{
				// ２回目以降
				if( UIEventSystem.IsPressing( gameObject ) == false && m_DragState == PointerState.None && UIEventSystem.ProcessType == StandaloneInputModuleWrapper.ProcessType.Custom )
				{
					OnHoverInner( PointerState.Moving, position ) ;
				}
			}
		}

		// Exit
		// 他の処理が無効化されてしまうため、メソッド内で return を使ってはダメ。本当は処理をメソッド単位に分離した方が良いのだが。
		virtual protected void OnPointerExitBasic( PointerEventData pointer, bool fromScrollView )
		{
//			Debug.Log( "<color=#7FFFFF>OnPointerExitBasic:" + name + "</color>" ) ;

			int identity = pointer.pointerId ;
			Vector2 position = GetLocalPosition( pointer ) ;

			m_FromScrollView = fromScrollView ;

			m_HoverAtFirst = false ;
			OnHoverInner( PointerState.End, position ) ;

			// クリック処理
//			if( onClickAction != null || onClickDelegate != null )
//			{
				if( m_ClickState == true && m_ClickPointerId == identity )
				{
					// 外に出たので以後は無効クリック扱いとなる
					m_ClickInsideFlag = false ;
				}
//			}

			// スマートクリック処理
//			if( onSmartClickDelegate != null )
//			{
				if( m_SmartClickState == true && m_SmartClickPointerId == identity )
				{
					// 外に出たので以後は無効クリック扱いとなる
					m_SmartClickInsideFlag = false ;
				}
//			}

			// ホールド判定終了
			CancelPress() ;
		}

		// Down
		// 他の処理が無効化されてしまうため、メソッド内で return を使ってはダメ。本当は処理をメソッド単位に分離した方が良いのだが。
		virtual protected void OnPointerDownBasic( PointerEventData pointer, bool fromScrollView )
		{
//			Debug.Log( "<color=#FF7F00>OnPointerDownBasic:" + name + "</color>" ) ;

			int identity = pointer.pointerId ;
			Vector2 position = GetLocalPosition( pointer ) ;

			m_FromScrollView = fromScrollView ;
			if( m_FromScrollView == true )
			{
				m_InteractionLimit_StartPoint = pointer.position ;
			}

			m_PressCountTime = Time.frameCount ;
			m_PressStartTime = Time.realtimeSinceStartup ;
			OnPressInner( true ) ;

			//----------------------------------------------------------

			// ロングプレス確認
			if( ( OnLongPressAction != null || OnLongPressDelegate != null ) && m_LongPressStartTime == 0 )
			{
				m_LongPressStartTime = Time.realtimeSinceStartup ;
			}

			// リピートプレス確認
			if( ( OnRepeatPressAction != null || OnRepeatPressDelegate != null ) && m_RepeatPressCount == 0 )
			{
				OnRepeatPressInner( 0 ) ;
			}

			//------------------------------------------------------------------------------------------

			if( pointer.eligibleForClick == false )
			{
				// このプレスではクリックとドラッグの処理はできない
				return ;
			}

			// クリック処理
//			if( onClickAction != null || onClickDelegate != null )
//			{
				m_ClickState = true ;
				m_ClickPointerId = identity ;
				m_ClickInsideFlag = true ;
//			}

			// 離れた時から計測を開始するので押した時は無効化
			m_SingleClickCheck = false ;

			// スマートクリック処理
//			if( onSmartClickDelegate != null )
///			{
				if( m_SmartClickState == false )
				{
					// １回目のクリック
					m_SmartClickState = true ;
					m_SmartClickPointerId = identity ;
					m_SmartClickInsideFlag = true ;

					m_SmartClickBasePosition = position ;
					m_SmartClickCount = 1 ;
					m_SmartClickBaseTime = Time.realtimeSinceStartup ;
				}
				else
				{
					// ２回目のクリック

					if( m_SmartClickLimitDistance == 0 || ( m_SmartClickLimitDistance >  0 && ( m_SmartClickBasePosition - position ).magnitude <= m_SmartClickLimitDistance ) )
					{
						// ダブルクリック判定に入る
						m_SmartClickPointerId = identity ;	// 識別子を新しくする(１回目と２回目では異なる可能性がある)

						m_SmartClickInsideFlag = true ;	// 重要:タッチパネルの実機の場合は Up すると Exit も発行されてしまうためもう一度有効扱いにする必要がある
						
						m_SmartClickCount = 2 ;	
						m_SmartClickBaseTime = Time.realtimeSinceStartup ;
					}
					else
					{
						m_SmartClickState = false ;
					}
				}
//			}


			//----------------------------------------------------------

			// スクロールビュー上では以下は無視する
			if( fromScrollView == true )
			{
				return ;
			}

			//----------------------------------------------------------

			// ドラッグ・フリック・タッチは処理が重いのでこの段階でデリゲートで抑制する

			// ドラッグ処理
			if( OnDragAction != null || OnDragDelegate != null )
			{
//				if( m_DragState == PointerState.None )
//				{
					m_DragPointerId = identity ;
					m_DragBasePosition = position ;
	
					m_DragState = PointerState.Start ;

					OnDragInner( PointerState.Start, m_DragBasePosition, m_DragBasePosition ) ;
//				}
			}

			// フリック処理
			if( OnFlickAction != null || OnFlickDelegate != null )
			{
				if( m_FlickState == false )
				{
					m_FlickState = true ;
	
					m_FlickPointerId = identity ;
					m_FlickBasePosition = position ;

					m_FlickCheck = false ;
				}
			}
		}

		// Up
		// 他の処理が無効化されてしまうため、メソッド内で return を使ってはダメ。本当は処理をメソッド単位に分離した方が良いのだが。
		virtual protected void OnPointerUpBasic( PointerEventData pointer, bool fromScrollView )
		{
//			Debug.Log( "<color=#00FFFF>OnPointerUpBasic:" + name + "</color>" ) ;

			int identity = pointer.pointerId ;
			Vector2 position = GetLocalPosition( pointer ) ;

			m_FromScrollView = fromScrollView ;

			m_PressCountTime = 0 ;
			m_PressStartTime = 0 ;
			OnPressInner( false ) ;

			//----------------------------------------------------------

			// スクロールビュー上では以下は無視する
			if( fromScrollView == true )
			{
				return ;
			}

			//----------------------------------------------------------

			// スクロールビューでなければ処理する
			// クリック処理
//			if( onClickAction != null || onClickDelegate != null )
//			{
				if( m_ClickState == true && m_ClickPointerId == identity )
				{
					if( m_ClickInsideFlag == true )
					{
						if( m_LongPressEnabled == false )
						{
							// クリックとみなす
							OnClickInner() ;
						}
					}
				}

				m_ClickState = false ;
//			}

			// スマートクリック処理
//			if( onSmartClickDelegate != null )
//			{
				if( m_SmartClickState == true && m_SmartClickPointerId == identity )
				{
					if( m_SmartClickInsideFlag == true )
					{
						if( m_SmartClickLimitDistance == 0 || ( m_SmartClickLimitDistance >  0 && ( m_SmartClickBasePosition - position ).magnitude <= m_SmartClickLimitDistance ) )
						{
							float time = Time.realtimeSinceStartup - m_SmartClickBaseTime ;

							if( m_SmartClickCount == 1 )
							{
								// シングルクリック判定
								if( m_SmartClickReleaseLimitTime <= 0 )
								{
									// 常にシングルクリック
									if( m_LongPressEnabled == false )
									{
										OnSmartClickInner( 1, m_SmartClickBasePosition, position ) ;
									}

									m_SmartClickState = false ;
								}
								else
								if( time <  m_SmartClickReleaseLimitTime )
								{
									// 一定時間以内に離していないと無効
									if( m_SmartClickSecondPressLimitTime <= 0 )
									{
										// シングルクリック決定
										if( m_LongPressEnabled == false )
										{
											OnSmartClickInner( 1, m_SmartClickBasePosition, position ) ;
										}

										m_SmartClickState = false ;
									}
									else
									{
										// シングルクリックかダブルクリックかを判定するルーチンを起動する
										if( m_LongPressEnabled == false )
										{
											SingleClickCheck( position, pointer.position ) ;
										}
									}
								}
								else
								{
									m_SmartClickState = false ;
								}
							}
							else
							if( m_SmartClickCount == 2 )
							{
								// ダブルクリック判定
	
								if( time <  m_SmartClickReleaseLimitTime )
								{
									// 一定時間以内に離していないと無効
	
									// ダブルクリック決定
									
									if( m_LongPressEnabled == false )
									{
										OnSmartClickInner( 2, m_SmartClickBasePosition, position ) ;
									}
	
									m_SmartClickState = false ;
								}
								else
								{
									m_SmartClickState = false ;
								}
							}
						}
						else
						{
							m_SmartClickState = false ;
						}
					}
					else
					{
						// 外に出たので無効クリック扱いとなる
						m_SmartClickState = false ;
					}
				}
//			}

			//----------------------------------------------------------

			// ドラッグ・フリック・タッチは処理が重いのでこの段階でデリゲートで抑制する

			// ドラッグ処理
			if( OnDragAction != null || OnDragDelegate != null )
			{
				if( ( m_DragState == PointerState.Start || m_DragState == PointerState.Moving ) && m_DragPointerId == identity )
				{
					m_DragState = PointerState.None ;
	
					OnDragInner( PointerState.End, m_DragBasePosition, position ) ;
				}
			}

			// フリック処理
			if( OnFlickAction != null || OnFlickDelegate != null )
			{
				if( m_FlickState == true && m_FlickPointerId == identity )
				{
					m_FlickState = false ;
	
					if( m_FlickCheck == true )
					{
						// １つ前のドラッグ位置と時間で最後に静止していたか判定する
						float lastTime = Time.realtimeSinceStartup - m_FlickLastTime ;

						if( lastTime <  0.1f )
						{
							// 基準位置からの移動量と時間で判定する
							Vector2 value = position - m_FlickBasePosition ;
							float time = Time.realtimeSinceStartup - m_FlickBaseTime ;
	
							if( time <  m_FlickDecisionLimitTime && value.magnitude >  m_FlickDecisionDistance )
							{
								// フリック有効
								OnFlickInner( value, m_FlickBasePosition ) ;
							}
						}
					}
				}
			}

			// 長押し判定終了
			CancelPress() ;
		}

		// スクロールビュー用のクリック判定(通常のクリック判定には使用していない)
		virtual protected void OnPointerClickBasic( PointerEventData pointer, bool fromScrollView )
		{
//			Debug.Log( "<color=#FFFF00>OnPointerClickBasic:" + name + "</color>" ) ;

			if( fromScrollView == false )
			{
				// スクロールビュー上で無い場合は以下は無視する
				return ;
			}

			// ちなみに
			// ScrollView 上のボタンが押しにくい＝少し動いただけで離された状態になりクリックができないのは、
			// EventSystem の DragThreshold の値で改善できる(デフォルトは 10)
			// この値を大きくすればその範囲内の移動なら離されたと判定はされない
			// この値を超えてしまうと PointerUp のコールバックが発生する(ただしスクロール方向と違う方向でも発生する)
			// PointerUp のコールバックが発生すると PointerClick のコールバックも発生しなくなる

			//----------------------------------------------------------

			int identity = pointer.pointerId ;
			Vector2 position = GetLocalPosition( pointer ) ;

			m_FromScrollView = fromScrollView ;

			//----------------------------------------------------------
			// Click

			if( m_ClickState == true && m_ClickPointerId == identity )
			{
				if( m_ClickInsideFlag == true )
				{
					if( m_LongPressEnabled == false )
					{
						// クリックとみなす
						OnClickInner() ;
					}
				}
			}

			m_ClickState = false ;

			//----------------------------------------------------------
			// SmartClick

			if( m_SmartClickState == true && m_SmartClickPointerId == identity )
			{
				if( m_SmartClickInsideFlag == true )
				{
					if( m_SmartClickLimitDistance == 0 || ( m_SmartClickLimitDistance >  0 && ( m_SmartClickBasePosition - position ).magnitude <= m_SmartClickLimitDistance ) )
					{
						float time = Time.realtimeSinceStartup - m_SmartClickBaseTime ;

						if( m_SmartClickCount == 1 )
						{
							// シングルクリック判定
							if( m_SmartClickReleaseLimitTime <= 0 )
							{
								// 常にシングルクリック
									
								if( m_LongPressEnabled == false )
								{
									OnSmartClickInner( 1, m_SmartClickBasePosition, position ) ;
								}

								m_SmartClickState = false ;
							}
							else
							if( time <  m_SmartClickReleaseLimitTime )
							{
								// 一定時間以内に離していないと無効
								if( m_SmartClickSecondPressLimitTime <= 0 )
								{
									// シングルクリック決定

									if( m_LongPressEnabled == false )
									{
										OnSmartClickInner( 1, m_SmartClickBasePosition, position ) ;
									}

									m_SmartClickState = false ;
								}
								else
								{
									// シングルクリックかダブルクリックかを判定するルーチンを起動する
									if( m_LongPressEnabled == false )
									{
										SingleClickCheck( position, pointer.position ) ;
									}
								}
							}
							else
							{
								m_SmartClickState = false ;
							}
						}
						else
						if( m_SmartClickCount == 2 )
						{
							// ダブルクリック判定
	
							if( time <  m_SmartClickReleaseLimitTime )
							{
								// 一定時間以内に離していないと無効
	
								// ダブルクリック決定
									
								if( m_LongPressEnabled == false )
								{
									OnSmartClickInner( 2, m_SmartClickBasePosition, position ) ;
								}
	
								m_SmartClickState = false ;
							}
							else
							{
								m_SmartClickState = false ;
							}
						}
					}
					else
					{
						m_SmartClickState = false ;
					}
				}
				else
				{
					// 外に出たので無効クリック扱いとなる
					m_SmartClickState = false ;
				}
			}

			//----------------------------------------------------------

			// ホールド判定終了
			CancelPress() ;
		}

		// Drag
		// 他の処理が無効化されてしまうため、メソッド内で return を使ってはダメ。本当は処理をメソッド単位に分離した方が良いのだが。
		virtual protected void OnDragBasic( PointerEventData pointer, bool fromScrollView )
		{
//			Debug.Log( "<color=#FFFF00>OnPointerDragBasic:" + name + "</color>" ) ;

			int identity = pointer.pointerId ;
			Vector2 position = GetLocalPosition( pointer ) ;
			
			m_FromScrollView = fromScrollView ;
			if( ( m_FromScrollView == true && Vector2.Distance( m_InteractionLimit_StartPoint, pointer.position ) >= m_InteractionLimit ) )
			{
				m_ClickState = false ;			// クリックキャンセル	
				m_SmartClickState = false ;	// スマートクリックキャンセル

				// 長押しをキャンセルする
				CancelPress() ;
			}

			// ドラッグ・フリック・タッチは処理が重いのでこの段階でデリゲートで抑制する

			// ドラッグ中の処理
			if( OnDragAction != null || OnDragDelegate != null )
			{
				if( m_DragPointerId == identity )
				{
					if( m_DragState == PointerState.Start || m_DragState == PointerState.Moving )
					{
						if( Input.touchCount == 1 || Input.touchCount == 0 )
						{
							// touchCount == 0  の場合は完全に PC(マウス) 環境
							m_DragState = PointerState.Moving ;
		
							OnDragInner( PointerState.Moving, m_DragBasePosition, position ) ;
						}
						else
						if( Input.touchCount >= 2 )
						{
							// ２点以上タッチされていたら一旦解除する
							m_DragState = PointerState.None ;
		
							OnDragInner( PointerState.End, m_DragBasePosition, position ) ;
						}
					}
					else
					if( m_DragState == PointerState.None )
					{
						if( Input.touchCount == 1 | Input.touchCount == 0 )
						{
							// touchCount == 0  の場合は完全に PC(マウス) 環境

							// １点タッチに戻ったので開始扱いとする
							m_DragBasePosition = position ;
	
							m_DragState = PointerState.Start ;

							OnDragInner( PointerState.Start, m_DragBasePosition, m_DragBasePosition ) ;
						}
					}
				}
			}

			// フリック処理
			if( OnFlickAction != null || OnFlickDelegate != null )
			{
				if( m_FlickState == true && m_FlickPointerId == identity )
				{
					if( m_FlickCheck == false )
					{
						Vector2 value = position - m_FlickBasePosition ;
	
						if( value.magnitude >   m_FlickDecisionDistance )
						{
							// フリック計測開始
							m_FlickCheck = true ;
							m_FlickBaseTime = Time.realtimeSinceStartup ;

							m_FlickLastTime = m_FlickBaseTime ;
						}
						else
						{
							// 基準の位置を更新
							m_FlickBasePosition = position ;
						}
					}
					else
					{
						// フリック判定チェック中のドラッグは最後の状態を保存しておく

						// １フレーム前の時間を保存しておく
						m_FlickLastTime = Time.realtimeSinceStartup ;
					}
				}
			}
		}

		// 長押しをキャンセルする
		private void CancelPress()
		{
			m_LongPressEnabled = false ;
			m_LongPressStartTime = 0 ;

			m_RepeatPressCount = 0 ;
			m_RepeatPressStartTime = 0 ;
		}

		//-----------------------------------

		private bool	m_SingleClickCheck ;
		private Vector2 m_SingleClickCheck_Position ;
		private Vector2 m_SingleClickCheck_GlobalPosition ;
		private float	m_SingleClickCheck_BaseTime ;
		private float   m_SingleClickCheck_TickTime ;
		
		// シングルクリックかダブルクリックかの判定用のコルーチン(ScrollView内でコルーチンを使用するのは危険なので＝アイテムの作り直し問題・Updateで処理するようにする)
		private void SingleClickCheck( Vector2 position, Vector2 globalPosition )
		{
			m_SingleClickCheck					= true ;

			m_SingleClickCheck_Position			= position ;
			m_SingleClickCheck_GlobalPosition	= globalPosition ;
			
			m_SingleClickCheck_BaseTime			= Time.realtimeSinceStartup ;
			m_SingleClickCheck_TickTime			= 0 ;
		}

		private void SingleClickCheckProcess()
		{
			if( m_SingleClickCheck == false )
			{
				return ;
			}

			//----------------------------------------------------------

			m_SingleClickCheck_TickTime += ( Time.realtimeSinceStartup - m_SingleClickCheck_BaseTime ) ;
			if( m_SingleClickCheck_TickTime <  m_SmartClickSecondPressLimitTime )
			{
				return ;
			}

			//----------------------------------------------------------
			// ダブルクリックと判定するまでに押さなければならない時間をオーバーした

			if( m_LongPressEnabled == false )
			{
				if( m_FromScrollView == false || ( m_FromScrollView == true && Vector2.Distance( m_InteractionLimit_StartPoint, m_SingleClickCheck_GlobalPosition ) <  m_InteractionLimit ) )
				{
					// シングルクリックとみなす
					OnSmartClickInner( 1, m_SmartClickBasePosition, m_SingleClickCheck_Position ) ;
				}
			}

			m_SmartClickState = false ;

			m_SingleClickCheck = false ;
		}

		//--------------------------------------------------------------------

		// Multi Touch

		/// <summary>
		/// 複数点タッチを処理する
		/// </summary>
		private void ProcessMultiTouch()
		{
			( int states, Vector2[] pointers ) = GetMultiPointer() ;

			if( OnPinchAction != null || OnPinchDelegate != null )
			{
				// コールバック発行
				OnPinchInner( states, pointers ) ;
			}

			if( OnTouchAction != null || OnTouchDelegate != null )
			{
				// コールバック発行
				OnTouchInner( states, pointers ) ;
			}
		}

		//--------------------------------------------------------------------

		// Hover

		/// <summary>
		/// ビューの範囲に入ったまたは出た際に呼び出されるアクション(マウス用)
		/// </summary>
		public Action<string, UIView, PointerState, Vector2> OnHoverAction ;
	
		/// <summary>
		/// ビューの範囲に入ったまたは出た際に呼び出されるデリゲートの定義(マウス用)
		/// </summary>
		/// <param name="identity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="view">ビューのインスタンス</param>
		/// <param name="state">状態</param>
		/// <param name="movePosition">現在位置</param>
		public delegate void OnHover( string identity, UIView view, PointerState state, Vector2 movePosition ) ;
		
		/// <summary>
		/// ビューの範囲に入ったまたは出た際に呼び出されるデリゲート(マウス用)
		/// </summary>
		public OnHover OnHoverDelegate ;

		// 内部リスナー
		private void OnHoverInner( PointerState state, Vector2 movePosition )
		{
			if( OnHoverAction != null || OnHoverDelegate != null) 
			{
				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				OnHoverAction?.Invoke( identity, this, state, movePosition ) ;
				OnHoverDelegate?.Invoke( identity, this, state, movePosition ) ;
			}
		}
		
		/// <summary>
		/// ビューの範囲に入ったまたは出た際に呼び出されるアクションを設定する → OnHover( string identity, UIView view, UIView.PointerState state, Vector2 movePosition )
		/// </summary>
		/// <param name="onHover">アクションメソッド</param>
		public void SetOnHover( Action<string, UIView, PointerState, Vector2> onHoverAction )
		{
			OnHoverAction = onHoverAction ;
		}

		/// <summary>
		/// ビューの範囲に入ったまたは出た際に呼び出されるデリゲートを追加する → OnHover( string identity, UIView view, UIView.PointerState state, Vector2 movePosition )
		/// </summary>
		/// <param name="onHOverelegate">デリゲートメソッド</param>
		public void AddOnHover( OnHover onHoverDelegate )
		{
			OnHoverDelegate += onHoverDelegate ;
		}

		/// <summary>
		/// ビューの範囲に入ったまたは出た際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="onHoverDelegate">デリゲートメソッド</param>
		public void RemoveOnHover( OnHover onHoverDelegate )
		{
			OnHoverDelegate -= onHoverDelegate ;
		}

		//----------------------

		// Press

		/// <summary>
		/// ビューを押したまたは離した際に呼び出されるアクション
		/// </summary>
		public Action<string, UIView, bool> OnPressAction ;
		
		/// <summary>
		/// ビューを押したまたは離した際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="identity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="view">ビューのインスタンス</param>
		/// <param name="isPressed">状態(true=プレス・false=リリース)</param>
		public delegate void OnPress( string identity, UIView view, bool isPressed ) ;

		/// <summary>
		/// ビューを押したまたは離した際に呼び出されるデリゲート
		/// </summary>
		public OnPress OnPressDelegate ;

		// 内部リスナー
		private void OnPressInner( bool state )
		{
			m_WaitForPress = true ;

			if( OnPressAction != null || OnPressDelegate != null || OnSimplePressAction != null || OnSimplePressDelegate != null )
			{
				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				OnPressAction?.Invoke( identity, this, state ) ;
				OnPressDelegate?.Invoke( identity, this, state ) ;

				OnSimplePressAction?.Invoke( state ) ;
				OnSimplePressDelegate?.Invoke( state ) ;
			}
		}
		
		/// <summary>
		/// ビューを押したまたは離した際に呼び出されるアクションを設定する → OnPress( string identity, UIView view, bool isPressed )
		/// </summary>
		/// <param name="onPress">アクションメソッド</param>
		public void SetOnPress( Action<string, UIView, bool> onPressAction )
		{
			OnPressAction = onPressAction ;
		}

		/// <summary>
		/// ビューを押したまたは離した際に呼び出されるデリゲートを追加する → OnPress( string identity, UIView view, bool isPressed )
		/// </summary>
		/// <param name="onPressDelegate">デリゲートメソッド</param>
		public void AddOnPress( OnPress onPressDelegate )
		{
			OnPressDelegate += onPressDelegate ;
		}

		/// <summary>
		/// ビューを押したまたは離した際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="onPressDelegate">デリゲートメソッド</param>
		public void RemoveOnPress( OnPress onPressDelegate )
		{
			OnPressDelegate -= onPressDelegate ;
		}

		//-----------

		/// <summary>
		/// ビューをプレスした際に呼び出されるアクション
		/// </summary>
		public Action<bool> OnSimplePressAction ;
		
		/// <summary>
		/// ビューをプレスした際に呼び出されるデリゲートの定義
		/// </summary>
		public delegate void OnSimplePress( bool state ) ;

		/// <summary>
		/// ビューをプレスした際に呼び出されるデリゲート
		/// </summary>
		public OnSimplePress OnSimplePressDelegate ;

		/// <summary>
		/// ビューをプレスした際に呼び出されるアクションを設定する → OnPress( bool state )
		/// </summary>
		public void SetOnSimplePress( Action<bool> onSimplePressAction )
		{
			OnSimplePressAction = onSimplePressAction ;
		}

		/// <summary>
		/// ビューをプレスした際に呼び出されるデリゲートを追加する → OnPress( bool state )
		/// </summary>
		/// <param name="onPressDelegate">デリゲートメソッド</param>
		public void AddOnSimplePress( OnSimplePress onSimplePressDelegate )
		{
			OnSimplePressDelegate += onSimplePressDelegate ;
		}

		/// <summary>
		/// ビューをプレスした際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="onPressDelegate">デリゲートメソッド</param>
		public void RemoveOnSimplePress( OnSimplePress onSimplePressDelegate )
		{
			OnSimplePressDelegate -= onSimplePressDelegate ;
		}

		//----------------------

		// Click

		/// <summary>
		/// ビューをクリックした際に呼び出されるアクション
		/// </summary>
		public Action<string, UIView> OnClickAction ;
		
		/// <summary>
		/// ビューをクリックした際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="identity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="view">ビューのインスタンス</param>
		public delegate void OnClick( string identity, UIView view ) ;

		/// <summary>
		/// ビューをクリックした際に呼び出されるデリゲート
		/// </summary>
		public OnClick OnClickDelegate ;

		/// <summary>
		/// ビューをクリックした際に呼び出されるアクションを設定する → OnClick( string identity, UIView view )
		/// </summary>
		/// <param name="onClick">アクションメソッド</param>
		public void SetOnClick( Action<string, UIView> onClickAction )
		{
			OnClickAction = onClickAction ;
		}

		/// <summary>
		/// ビューをクリックした際に呼び出されるデリゲートを追加する → OnClick( string identity, UIView view )
		/// </summary>
		/// <param name="onClickDelegate">デリゲートメソッド</param>
		public void AddOnClick( OnClick onClickDelegate )
		{
			OnClickDelegate += onClickDelegate ;
		}

		/// <summary>
		/// ビューをクリックした際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="onClickDelegate">デリゲートメソッド</param>
		public void RemoveOnClick( OnClick onClickDelegate )
		{
			OnClickDelegate -= onClickDelegate ;
		}

		//-----------

		/// <summary>
		/// ビューをクリックした際に呼び出されるアクション
		/// </summary>
		public Action OnSimpleClickAction ;
		
		/// <summary>
		/// ビューをクリックした際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="identity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="view">ビューのインスタンス</param>
		public delegate void OnSimpleClick() ;

		/// <summary>
		/// ビューをクリックした際に呼び出されるデリゲート
		/// </summary>
		public OnSimpleClick OnSimpleClickDelegate ;

		/// <summary>
		/// ビューをクリックした際に呼び出されるアクションを設定する → OnClick( string identity, UIView view )
		/// </summary>
		/// <param name="onClick">アクションメソッド</param>
		public void SetOnSimpleClick( Action onSimpleClickAction )
		{
			OnSimpleClickAction = onSimpleClickAction ;
		}

		/// <summary>
		/// ビューをクリックした際に呼び出されるデリゲートを追加する → OnClick( string identity, UIView view )
		/// </summary>
		/// <param name="onClickDelegate">デリゲートメソッド</param>
		public void AddOnSimpleClick( OnSimpleClick onSimpleClickDelegate )
		{
			OnSimpleClickDelegate += onSimpleClickDelegate ;
		}

		/// <summary>
		/// ビューをクリックした際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="onClickDelegate">デリゲートメソッド</param>
		public void RemoveOnSimpleClick( OnSimpleClick onSimpleClickDelegate )
		{
			OnSimpleClickDelegate -= onSimpleClickDelegate ;
		}

		//-----------

		/// <summary>
		/// クリックを強制的に実行する
		/// </summary>
		public void ExecuteClick()
		{
			OnClickInner() ;
		}

		// 内部リスナー
		private void OnClickInner()
		{
			m_Click = true ;
			m_ClickCountTime = Time.frameCount ;

			m_WaitForClick = true ;

			if( gameObject.activeInHierarchy == false )
			{
				return ;	// アクティブな場合のみ有効(非アクティブになった際のリリースでクリック判定されてしまうのを防ぐ
			}

			if( OnClickAction != null || OnClickDelegate != null )
			{
				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				OnClickAction?.Invoke( identity, this ) ;
				OnClickDelegate?.Invoke( identity, this ) ;
			}

			if( OnSimpleClickAction != null || OnSimpleClickDelegate != null )
			{
				OnSimpleClickAction?.Invoke() ;
				OnSimpleClickDelegate?.Invoke() ;
			}
		}
		
		//----------------------

		// SmartClick

		/// <summary>
		/// ビューをクリックした際に呼び出されるアクション
		/// </summary>
		public Action<string, UIView, int, Vector2, Vector2> OnSmartClickAction ;
		
		/// <summary>
		/// ビューをクリックした際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="identity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="view">ビューのインスタンス</param>
		/// <param name="count">クリック種別(1=シングル・2=ダブル)</param>
		/// <param name="basePosition">最初のカーソル座標</param>
		/// <param name="movePosition">最後のカーソル座標</param>
		public delegate void OnSmartClick( string identity, UIView view, int count, Vector2 basePosition, Vector2 movePosition ) ;

		/// <summary>
		/// ビューをクリックした際に呼び出されるデリゲート
		/// </summary>
		public OnSmartClick OnSmartClickDelegate ;
	
		// 内部リスナー
		private void OnSmartClickInner( int count, Vector2 basePosition, Vector2 movePosition )
		{
			if( gameObject.activeInHierarchy == false )
			{
				return ;	// アクティブな場合のみ有効(非アクティブになった際のリリースでクリック判定されてしまうのを防ぐ
			}

			if( OnSmartClickAction != null || OnSmartClickDelegate != null )
			{
				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				OnSmartClickAction?.Invoke( identity, this, count, basePosition, movePosition ) ;
				OnSmartClickDelegate?.Invoke( identity, this, count, basePosition, movePosition ) ;
			}
		}
		
		/// <summary>
		/// ビューをクリックした際に呼び出されるデリゲートを追加する → OnSmartClick( string identity, UIView view, int count, Vector2 basePosition, Vector2 movePosition )
		/// </summary>
		/// <param name="onSmartClickDelegate">デリゲートメソッド</param>
		public void SetOnSmartClick( Action<string, UIView, int, Vector2, Vector2> onSmartClickAction )
		{
			OnSmartClickAction  = onSmartClickAction ;
		}
		
		/// <summary>
		/// スマートクリックの判定用パラメータを設定する
		/// </summary>
		/// <param name="releaseLimitTime">シングルクリックと判定される押してから離すまでの時間(秒)[デフォルト0.5秒]</param>
		/// <param name="secondPressLimitTime">ダブルクリックと判定される離してから押すまでの時間(秒)[デフォルト0.25秒]</param>
		/// <param name="limitDistance">クリックと判定される限界の移動量[デフォルト30]</param>
		public void SetSmartClickParameter(  float releaseLimitTime, float secondPressLimitTime, float limitDistance )
		{
			if( releaseLimitTime >= 0 )
			{
				m_SmartClickReleaseLimitTime = releaseLimitTime ;
			}
			if( secondPressLimitTime >= 0 )
			{
				m_SmartClickSecondPressLimitTime = secondPressLimitTime ;
			}
			if( limitDistance >= 0 )
			{
				m_SmartClickLimitDistance = limitDistance ;	// 最初に押した位置からどの程度の距離まで有効か
			}
		}

		/// <summary>
		/// ビューをクリックした際に呼び出されるデリゲートを追加する → OnSmartClick( string identity, UIView view, int count, Vector2 basePosition, Vector2 movePosition )
		/// </summary>
		/// <param name="onSmartClickDelegate">デリゲートメソッド</param>
		public void AddOnSmartClick( OnSmartClick onSmartClickDelegate )
		{
			OnSmartClickDelegate += onSmartClickDelegate ;
		}

		/// <summary>
		/// ビューをクリックした際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="onSmartClickDelegate">デリゲートメソッド</param>
		public void RemoveOnSmartClick( OnSmartClick onSmartClickDelegate )
		{
			OnSmartClickDelegate -= onSmartClickDelegate ;
		}
		
		//----------------------

		// LongPress		

		/// <summary>
		/// ビューを長押しした際に呼び出されるアクション
		/// </summary>
		public Action<string, UIView> OnLongPressAction ;
		
		/// <summary>
		/// ビューを長押しした際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="identity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="view">ビューのインスタンス</param>
		public delegate void OnLongPress( string identity, UIView view ) ;

		/// <summary>
		/// ビューを長押しした際に呼び出されるデリゲート
		/// </summary>
		public OnLongPress OnLongPressDelegate ;

		// 内部リスナー
		private void OnLongPressInner()
		{
			if( m_LongPressEnabled == false && m_LongPressStartTime >  0 )
			{
				float time = Time.realtimeSinceStartup - m_LongPressStartTime ;

				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				if( time >= m_LongPressDecisionTime )
				{
					m_LongPressEnabled = true ;	// 押し続けている扱いとする
					m_LongPressStartTime = 0 ;

					// 最初に長押しと判定された時だけコールバックを呼ぶ
					OnLongPressAction?.Invoke( identity, this ) ;
					OnLongPressDelegate?.Invoke( identity, this ) ;

					UITransition transition = GetComponent<UITransition>() ;
					if( transition != null )
					{
						transition.OnPointerUp( null ) ;
					}
				}
			}
		}
		
		/// <summary>
		/// ビューを長押しした際に呼び出されるアクションを設定する → OnLongPress( string identity, UIView view )
		/// </summary>
		/// <param name="onLongPress">アクションメソッド</param>
		public void SetOnLongPress( Action<string, UIView> onLongPressAction, float longPressDecisionTime = 0.75f )
		{
			OnLongPressAction = onLongPressAction ;
			if( longPressDecisionTime >  0 )
			{
				m_LongPressDecisionTime = longPressDecisionTime ;
			}
		}

		/// <summary>
		/// ビューを長押しした際に呼び出されるデリゲートを追加する → OnLongPress( string identity, UIView view )
		/// </summary>
		/// <param name="onLongPressDelegate">デリゲートメソッド</param>
		public void AddOnLongPress( OnLongPress onLongPressDelegate, float longPressDecisionTime = 0.75f )
		{
			OnLongPressDelegate += onLongPressDelegate ;
			if( longPressDecisionTime >  0 )
			{
				m_LongPressDecisionTime = longPressDecisionTime ;
			}
		}

		/// <summary>
		/// ビューを長押しした際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="onLongPressDelegate">デリゲートメソッド</param>
		public void RemoveOnLongPress( OnLongPress onLongPressDelegate )
		{
			OnLongPressDelegate -= onLongPressDelegate ;
		}

		//----------------------

		// Repeat		

		/// <summary>
		/// ビューを長押しした際に呼び出されるアクション
		/// </summary>
		public Action<string, UIView, int> OnRepeatPressAction ;
		
		/// <summary>
		/// ビューを長押しした際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="identity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="view">ビューのインスタンス</param>
		public delegate void OnRepeatPress( string identity, UIView view, int repeatCount ) ;

		/// <summary>
		/// ビューを長押しした際に呼び出されるデリゲート
		/// </summary>
		public OnRepeatPress OnRepeatPressDelegate ;

		// 内部リスナー
		private void OnRepeatPressInner( int state )
		{
			// 識別子
			string identity = Identity ;
			if( string.IsNullOrEmpty( identity ) == true )
			{
				identity = name ;
			}

			if( state == 0 )
			{
				m_RepeatPressStartTime = Time.realtimeSinceStartup ;

				// 最初のコールバック
				OnRepeatPressAction?.Invoke( identity, this, m_RepeatPressCount ) ;
				OnRepeatPressDelegate?.Invoke( identity, this, m_RepeatPressCount ) ;

				m_RepeatPressCount ++ ;	// リピート回数
			}
			else
			{
				// それ以降のコールバック
				if( m_RepeatPressCount == 1 )
				{
					float startTime = Time.realtimeSinceStartup ;
					float deltaTime = startTime - m_RepeatPressStartTime ;

					if( deltaTime >= m_RepeatPressDecisionTime )
					{
						m_RepeatPressStartTime = startTime ;	// リピートへ

						// 最初に長押しと判定された時だけコールバックを呼ぶ
						OnRepeatPressAction?.Invoke( identity, this, m_RepeatPressCount ) ;
						OnRepeatPressDelegate?.Invoke( identity, this, m_RepeatPressCount ) ;

						m_RepeatPressCount ++ ;	// リピート回数
					}
				}
				else
				if( m_RepeatPressCount >= 2 )
				{
					float startTime = Time.realtimeSinceStartup ;
					float deltaTime = startTime - m_RepeatPressStartTime ;

					if( deltaTime >= m_RepeatPressIntervalTime )
					{
						m_RepeatPressStartTime = startTime ;	// リピートへ

						// 最初に長押しと判定された時だけコールバックを呼ぶ
						OnRepeatPressAction?.Invoke( identity, this, m_RepeatPressCount ) ;
						OnRepeatPressDelegate?.Invoke( identity, this, m_RepeatPressCount ) ;

						m_RepeatPressCount ++ ;	// リピート回数
					}
				}
			}
		}
		
		/// <summary>
		/// ビューを長押しした際に呼び出されるアクションを設定する → OnLongPress( string identity, UIView view )
		/// </summary>
		/// <param name="onRepeatPress">アクションメソッド</param>
		public void SetOnRepeatPress( Action<string, UIView, int> onRepeatPressAction, float repeatPressDecisionTime = 0.75f, float repeatPressIntervalTime = 0.25f )
		{
			OnRepeatPressAction = onRepeatPressAction ;
			if( repeatPressDecisionTime >  0 )
			{
				m_RepeatPressDecisionTime = repeatPressDecisionTime ;
				m_RepeatPressIntervalTime = repeatPressIntervalTime ;
			}
		}

		/// <summary>
		/// ビューを長押しした際に呼び出されるデリゲートを追加する → OnLongPress( string identity, UIView view )
		/// </summary>
		/// <param name="onRepeatPressDelegate">デリゲートメソッド</param>
		public void AddOnRepeatPress( OnRepeatPress onRepeatPressDelegate, float repeatPressDecisionTime = 0.75f, float repeatPressIntervalTime = 0.25f )
		{
			OnRepeatPressDelegate += onRepeatPressDelegate ;
			if( repeatPressDecisionTime >  0 )
			{
				m_RepeatPressDecisionTime = repeatPressDecisionTime ;
				m_RepeatPressIntervalTime = repeatPressIntervalTime ;
			}
		}

		/// <summary>
		/// ビューを長押しした際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="onRepeatPressDelegate">デリゲートメソッド</param>
		public void RemoveOnRepeatPress( OnRepeatPress onRepeatPressDelegate )
		{
			OnRepeatPressDelegate -= onRepeatPressDelegate ;
		}

		/// <summary>
		/// リピートの間隔時間を設定する
		/// </summary>
		/// <param name="repeatPressIntervalTime"></param>
		public void SetRepeatPressIntervalTime( float repeatPressIntervalTime )
		{
			if( repeatPressIntervalTime >  0 )
			{
				m_RepeatPressIntervalTime = repeatPressIntervalTime ;
			}
		}

		//----------------------

		// Drag

		/// <summary>
		/// ビューをドラッグした際に呼び出されるアクション
		/// </summary>
		public Action<string, UIView, PointerState, Vector2, Vector2> OnDragAction ;

		/// <summary>
		/// ビュー上でドラッグした際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="identity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="view">ビューのインスタンス</param>
		/// <param name="state">ドラッグの状態</param>
		/// <param name="basePosition">ドラッグ開始座標</param>
		/// <param name="movePosition">ドラッグ現在座標</param>
		public delegate void OnDrag( string identity, UIView view, PointerState state, Vector2 basePosition, Vector2 movePosition ) ;

		/// <summary>
		/// ビュー上でドラッグした際に呼び出されるデリゲート
		/// </summary>
		public OnDrag OnDragDelegate ;
	
		// 内部リスナー
		private void OnDragInner( PointerState state, Vector2 basePosition, Vector2 movePosition )
		{
			if( OnDragAction != null || OnDragDelegate != null )
			{
				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				OnDragAction?.Invoke( identity, this, state, basePosition, movePosition ) ;
				OnDragDelegate?.Invoke( identity, this, state, basePosition, movePosition ) ;
			}
		}
		
		/// <summary>
		/// ビュー上でドラッグした際に呼び出されるデリゲートを追加する → OnDrag( string identity, UIView view, PointerState state, Vector2 basePosition, Vector2 movePosition )
		/// </summary>
		/// <param name="onDragDelegate"></param>
		public void SetOnDrag( Action<string, UIView, PointerState, Vector2, Vector2> onDragAction )
		{
			OnDragAction  = onDragAction ;
		}

		/// <summary>
		/// ビュー上でドラッグした際に呼び出されるデリゲートを追加する → OnDrag( string identity, UIView view, PointerState state, Vector2 basePosition, Vector2 movePosition )
		/// </summary>
		/// <param name="onDragDelegate"></param>
		public void AddOnDrag( OnDrag onDragDelegate )
		{
			OnDragDelegate += onDragDelegate ;
		}

		/// <summary>
		/// ビュー上でドラッグした際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="onDragDelegate"></param>
		public void RemoveOnDrag( OnDrag onDragDelegate )
		{
			OnDragDelegate -= onDragDelegate ;
		}

		//----------------------

		// Flick

		/// <summary>
		/// ビューをフリックした際に呼び出されるアクション
		/// </summary>
		public Action<string, UIView, Vector2, Vector2> OnFlickAction ;
		
		/// <summary>
		/// ビューをフリックした際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="identity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="view">ビューのインスタンス</param>
		/// <param name="dstance">フリック移動量</param>
		/// <param name="basePosition">フリック開始座標</param>
		public delegate void OnFlick( string identity, UIView view, Vector2 distance, Vector2 basePosition ) ;

		/// <summary>
		/// ビューをフリックした際に呼び出されるデリゲート
		/// </summary>
		public OnFlick OnFlickDelegate ;

		// 内部リスナー
		private void OnFlickInner( Vector2 distance, Vector2 basePosition )
		{
			if( OnFlickAction != null || OnFlickDelegate != null )
			{
				if( Input.touchCount >= 2 )
				{
					return ;	// ２点以上タッチされていたら無視する
				}

				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				OnFlickAction?.Invoke( identity, this, distance, basePosition ) ;
				OnFlickDelegate?.Invoke( identity, this, distance, basePosition ) ;
			}
		}
		
		/// <summary>
		/// フリックの判定用パラメータを設定する
		/// </summary>
		/// <param name="decisionLimitTime">フリックと判定されるため押してから離すまでの最大時間(秒)[デフォルト0.5秒]</param>
		/// <param name="decisionDistance">フリックと判定されるための押してから離すまでの最小移動距離割合[デフォルト60]</param>
		public void SetFlickParameter( float decisionLimitTime, float decisionDistance )
		{
			m_FlickDecisionLimitTime	= decisionLimitTime ;	// この時間以内に離さないとフリックとはみなされない
			m_FlickDecisionDistance		= decisionDistance ;	// この距離以上移動していないとフリックとはみなされない
		}

		/// <summary>
		/// ビューをフリックした際に呼び出されるアクションを設定する → OnFlick( string identity, UIView view, Vector2 distance, Vector2 basePosition )
		/// </summary>
		/// <param name="onFlickAction">アクションメソッド</param>
		public void SetOnFlick( Action<string, UIView, Vector2, Vector2> onFlickAction )
		{
			OnFlickAction			= onFlickAction ;
		}

		/// <summary>
		/// ビューをフリックした際に呼び出されるアクションを追加する → OnFlick( string identity, UIView view, Vector2 distance, Vector2 basePosition )
		/// </summary>
		/// <param name="onFlickDelegate">デリゲートメソッド</param>
		public void AddOnFlick( OnFlick onFlickDelegate )
		{
			OnFlickDelegate += onFlickDelegate ;
		}

		/// <summary>
		/// ビューをフリックした際に呼び出されるアクションを削除する
		/// </summary>
		/// <param name="onFlickDelegate">デリゲートメソッド</param>
		public void RemoveOnFlick( OnFlick onFlickDelegate )
		{
			OnFlickDelegate -= onFlickDelegate ;
		}

		//----------------------

		// Pinch

		/// <summary>
		/// ピンチ(２箇所限定)があった際に呼び出されるアクション
		/// </summary>
		public Action<string, UIView, PointerState, float, float, Vector2, Vector2> OnPinchAction ;
		
		/// <summary>
		/// ピンチ(２箇所限定)があった際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="identity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="view">ビューのインスタンス</param>
		/// <param name="touchState">タッチ情報が格納された配列</param>
		public delegate void OnPinch( string identity, UIView view, PointerState state, float ratio, float distance, Vector2 p0, Vector2 p1 ) ;

		/// <summary>
		/// ピンチ( ２箇所限定)があった際に呼び出されるデリゲートの定義
		/// </summary>
		public OnPinch OnPinchDelegate ;

		private int m_PinchStates = 0 ;
		private float m_PinchDistanceBase = 0 ;

		// 内部リスナー
		private void OnPinchInner( int states, Vector2[] pointers )
		{
			int i, l = pointers.Length ;
			int c0 = 0, c1 = 0 ;
			Vector2[] p = { Vector2.zero, Vector2.zero } ;

			// タッチ数を数える

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( ( m_PinchStates & ( 1 << i ) ) != 0 )
				{
					c0 ++ ;
				}
			}

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( ( states & ( 1 << i ) ) != 0 )
				{
					if( c1 <  2 )
					{
						p[ c1 ] = pointers[ i ] ;
					}

					c1 ++ ;
				}
			}

			m_PinchStates = states ;

			//----------------------------------

			PointerState state = PointerState.None ;

			if( c1 == 2 )
			{
				// ピンチ有効
				if( c0 != 2 || states != m_PinchStates )
				{
					// 新規
					state = PointerState.Start ;
				}
				else
				{
					// 継続
					state = PointerState.Moving ;
				}
			}
			else
			{
				// ピンチ無効
				if( c0 == 2 )
				{
					state = PointerState.End ;
				}
			}

			float ratio = 0 ;
			float distance = 0 ;

			if( state == PointerState.Start )
			{
				// 開始
				distance = ( p[ 1 ] - p[ 0 ] ).magnitude ;

				if( distance >  0 )
				{
					ratio = 1 ;
					m_PinchDistanceBase = distance ;
				}
				else
				{
					// 無効
					m_PinchDistanceBase = 0 ;
					state = PointerState.None ;
				}
			}
			else
			if( state == PointerState.Moving )
			{
				// 継続
				if( m_PinchDistanceBase >  0 )
				{
					distance = ( p[ 1 ] - p[ 0 ] ).magnitude ;
					ratio = distance / m_PinchDistanceBase ;
				}
				else
				{
					// 異常
					state = PointerState.None ;
				}
			}
			else
			if( state == PointerState.End )
			{
				// 終了
				if( m_PinchDistanceBase >  0 )
				{
					m_PinchDistanceBase = 0 ;
				}
				else
				{
					// 異常
					state = PointerState.None ;
				}
			}

			if( state != PointerState.None )
			{
				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				OnPinchAction?.Invoke( identity, this, state, ratio, distance, p[ 0 ], p[ 1 ] ) ;
				OnPinchDelegate?.Invoke( identity, this, state, ratio, distance, p[ 0 ], p[ 1 ] ) ;
			}
		}
		
		/// <summary>
		/// ピンチ(２箇所限定)があった際に呼び出されるアクションを設定する → OnPinchDelegate( string identity, UIView view, PointerState state, float ratio, float distance, Vector2 p0, Vector2 p1 )
		/// </summary>
		/// <param name="onTouchAction">アクションメソッド</param>
		public void SetOnPinch( Action<string, UIView, PointerState, float, float, Vector2, Vector2> onPinchAction )
		{
			OnPinchAction = onPinchAction ;
		}

		/// <summary>
		/// ピンチ(２箇所限定)があった際に呼び出されるデリゲートを追加する → OnTouchDelegate( string tIdentity, UIView tView, TouchState[] tTouchState )
		/// </summary>
		/// <param name="onPinchDelegate">デリゲートメソッド</param>
		public void AddOnPinch( OnPinch onPinchDelegate )
		{
			OnPinchDelegate += onPinchDelegate ;
		}

		/// <summary>
		/// ピンチ(２箇所限定)があった際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="onPinchDelegate">デリゲートメソッド</param>
		public void RemoveOnPinch( OnPinch onPinchDelegate )
		{
			OnPinchDelegate -= onPinchDelegate ;
		}

		//----------------------

		// Touch

		/// <summary>
		/// タッチ(１箇所以上)があった際に呼び出されるアクション
		/// </summary>
		public Action<string, UIView, TouchState[]> OnTouchAction ;
		
		/// <summary>
		/// タッチ(１箇所以上)があった際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="identity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="view">ビューのインスタンス</param>
		/// <param name="touchState">タッチ情報が格納された配列</param>
		public delegate void OnTouch( string identity, UIView view, TouchState[] touchState ) ;	

		/// <summary>
		/// タッチ(１箇所以上)があった際に呼び出されるデリゲートの定義
		/// </summary>
		public OnTouch OnTouchDelegate ;

		// 内部リスナー
		private void OnTouchInner( int states, Vector2[] pointers )
		{
			int i, l = pointers.Length ;
			for( i = 0 ; i < l ; i ++ )
			{
				if( ( states & ( 1 << i ) ) != 0 )
				{
					if( m_TouchState[ i ] == null )
					{
						// 押された
						m_TouchState[ i ] = new TouchState( i, i, pointers[ i ], PointerState.Start ) ;
					}
					else
					{
						// 押されている
						if( m_TouchState[ i ].State == PointerState.Start )
						{
							// 状態変更
							m_TouchState[ i ].State = PointerState.Moving ;
							m_TouchState[ i ].Position = pointers[ i ] ;
						}
						else
						if( m_TouchState[ i ].State == PointerState.Moving )
						{
							// 位置更新
							m_TouchState[ i ].Position = pointers[ i ] ;
						}
						else
						if( m_TouchState[ i ].State == PointerState.End )
						{
							// 状態変更
							m_TouchState[ i ].State = PointerState.Start ;
							m_TouchState[ i ].Position = pointers[ i ] ;
						}
					}
				}
				else
				{
					if( m_TouchState[ i ] != null )
					{
						if( m_TouchState[ i ].State == PointerState.Start || m_TouchState[ i ].State == PointerState.Moving )
						{
							// 離された
							m_TouchState[ i ].State = PointerState.End ;
							m_TouchState[ i ].Position = pointers[ i ] ;
						}
						else
						if( m_TouchState[ i ].State == PointerState.End )
						{
							// 破棄
							m_TouchState[ i ] = null ;
						}
					}
				}
			}

			//----------------------------------------------------------

			string identity = Identity ;
			if( string.IsNullOrEmpty( identity ) == true )
			{
				identity = name ;
			}

			// 存在するもののみ抽出する
			m_TouchStateExchange.Clear() ;
			l = m_TouchState.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_TouchState[ i ] != null )
				{
					m_TouchStateExchange.Add( m_TouchState[ i ] ) ;
				}
			}

			OnTouchAction?.Invoke( identity, this, m_TouchStateExchange.ToArray() ) ;
			OnTouchDelegate?.Invoke( identity, this, m_TouchStateExchange.ToArray() ) ;
		}
		
		/// <summary>
		/// タッチ(１箇所以上)があった際に呼び出されるアクションを設定する → OnTouchDelegate( string tIdentity, UIView tView, TouchState[] tTouchState )
		/// </summary>
		/// <param name="onTouchAction">アクションメソッド</param>
		public void SetOnTouch( Action<string, UIView, TouchState[]> onTouchAction )
		{
			OnTouchAction = onTouchAction ;
		}

		/// <summary>
		/// タッチ(１箇所以上)があった際に呼び出されるデリゲートを追加する → OnTouchDelegate( string tIdentity, UIView tView, TouchState[] tTouchState )
		/// </summary>
		/// <param name="onTouchDelegate">デリゲートメソッド</param>
		public void AddOnTouch( OnTouch onTouchDelegate )
		{
			OnTouchDelegate += onTouchDelegate ;
		}

		/// <summary>
		/// タッチ(１箇所以上)があった際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="onTouchDelegate">デリゲートメソッド</param>
		public void RemoveOnTouch( OnTouch onTouchDelegate )
		{
			OnTouchDelegate -= onTouchDelegate ;
		}

		//--------------------------------------------------------------------

		private readonly Dictionary<EventTriggerType, Action<string,UIView,EventTriggerType>> m_EventTriggerCallbackList = new Dictionary<EventTriggerType, Action<string, UIView, EventTriggerType>>() ;
		private readonly Dictionary<EventTriggerType, EventTrigger.Entry>                     m_EventTriggerEntryList    = new Dictionary<EventTriggerType, EventTrigger.Entry>() ;

		/// <summary>
		/// EventTrigger のコールバックメソッドを登録する
		/// </summary>
		/// <param name="callback"></param>
		/// <param name="typeArray"></param>
		/// <returns></returns>
		public bool AddEventTriggerCallback( Action<string,UIView,EventTriggerType> callback, params EventTriggerType[] typeArray )
		{
			if( callback == null || typeArray == null || typeArray.Length == 0 )
			{
				// 引数が不正
#if UNITY_EDITOR
				Debug.LogWarning( name + " : " + "Bad parameter" ) ;
#endif
				return false ;
			}

			EventTrigger eventTrigger = CEventTrigger ;
			if( eventTrigger == null )
			{
				// イベントトリガーがアタッチされていない
				Debug.LogWarning( name + " : " + "Event Trigger not attached." ) ;
				return false ;
			}

			if( eventTrigger.triggers == null )
			{
				eventTrigger.triggers = new List<EventTrigger.Entry>() ;
			}
		
			int i ;
			EventTriggerType type ;
			EventTrigger.Entry entry ;

			for( i  = 0 ; i <  typeArray.Length ; i ++ )
			{
				// 既に登録されている場合は上書きになる
				type = typeArray[ i ] ;
				if( m_EventTriggerCallbackList.ContainsKey( type ) == false )
				{
					// 新規登録
					m_EventTriggerCallbackList.Add( type, callback ) ;
				}
				else
				{
					// 上書登録
					m_EventTriggerCallbackList[ type ] = callback ;
				}

				// エントリーが既に登録されているかを確認する
				if( m_EventTriggerEntryList.ContainsKey( type ) == true )
				{
					// 既に登録されている
					entry = m_EventTriggerEntryList[ type ] ;

					if( eventTrigger.triggers.Contains( entry ) == true )
					{
						// 登録済みなので一度破棄しておく
						eventTrigger.triggers.Remove( entry ) ;
					}
				}
				else
				{
					// 登録されていないので新規にエントリーを生成する
					entry = new EventTrigger.Entry()
					{
						eventID = type
					} ;
					UnityAction<BaseEventData> action  = null ;

					switch( type )
					{
						case EventTriggerType.PointerEnter				: action = OnPointerEnterInner				; break ;
						case EventTriggerType.PointerExit				: action = OnPointerExitInner				; break ;
						case EventTriggerType.PointerDown				: action = OnPointerDownInner				; break ;
						case EventTriggerType.PointerUp					: action = OnPointerUpInner					; break ;
						case EventTriggerType.PointerClick				: action = OnPointerClickInner				; break ;
						case EventTriggerType.Drag						: action = OnDragInner						; break ;
						case EventTriggerType.Drop						: action = OnDropInner						; break ;
						case EventTriggerType.Scroll					: action = OnScrollInner					; break ;
						case EventTriggerType.UpdateSelected			: action = OnUpdateSelectedInner			; break ;
						case EventTriggerType.Select					: action = OnSelectInner					; break ;
						case EventTriggerType.Deselect					: action = OnDeselectInner					; break ;
						case EventTriggerType.Move						: action = OnMoveInner						; break ;
						case EventTriggerType.InitializePotentialDrag	: action = OnInitializePotentialDragInner	; break ;
						case EventTriggerType.BeginDrag					: action = OnBeginDragInner					; break ;
						case EventTriggerType.EndDrag					: action = OnEndDragInner					; break ;
						case EventTriggerType.Submit					: action = OnSubmitInner					; break ;
						case EventTriggerType.Cancel					: action = OnCancelInner					; break ;
					}

					entry.callback.AddListener( action ) ;

					m_EventTriggerEntryList.Add( type, entry ) ;
				}

				// 改めてエントリーを登録する
				eventTrigger.triggers.Add( entry ) ;
			}

			return true ;
		}

		private void OnPointerEnterInner( BaseEventData data )
		{
			InvokeEventTriggerCallback( EventTriggerType.PointerEnter, data ) ;
		}

		private void OnPointerExitInner( BaseEventData data )
		{
			InvokeEventTriggerCallback( EventTriggerType.PointerExit, data ) ;
		}

		private void OnPointerDownInner( BaseEventData data )
		{
			InvokeEventTriggerCallback( EventTriggerType.PointerDown, data ) ;
		}

		private void OnPointerUpInner( BaseEventData data )
		{
			InvokeEventTriggerCallback( EventTriggerType.PointerUp, data ) ;
		}

		private void OnPointerClickInner( BaseEventData data )
		{
			InvokeEventTriggerCallback( EventTriggerType.PointerClick, data ) ;
		}

		private void OnDragInner( BaseEventData data )
		{
			InvokeEventTriggerCallback( EventTriggerType.Drag, data ) ;
		}

		private void OnDropInner( BaseEventData data )
		{
			InvokeEventTriggerCallback( EventTriggerType.Drop, data ) ;
		}

		private void OnScrollInner( BaseEventData data )
		{
			InvokeEventTriggerCallback( EventTriggerType.Scroll, data ) ;
		}

		private void OnUpdateSelectedInner( BaseEventData data )
		{
			InvokeEventTriggerCallback( EventTriggerType.UpdateSelected, data ) ;
		}

		private void OnSelectInner( BaseEventData data )
		{
			InvokeEventTriggerCallback( EventTriggerType.Select,data ) ;
		}

		private void OnDeselectInner( BaseEventData data )
		{
			InvokeEventTriggerCallback( EventTriggerType.Deselect, data ) ;
		}

		private void OnMoveInner( BaseEventData data )
		{
			InvokeEventTriggerCallback( EventTriggerType.Move, data ) ;
		}

		private void OnInitializePotentialDragInner( BaseEventData data )
		{
			InvokeEventTriggerCallback( EventTriggerType.InitializePotentialDrag, data ) ;
		}

		private void OnBeginDragInner( BaseEventData data )
		{
			InvokeEventTriggerCallback( EventTriggerType.BeginDrag, data ) ;
		}

		private void OnEndDragInner( BaseEventData data )
		{
			InvokeEventTriggerCallback( EventTriggerType.EndDrag, data ) ;
		}

		private void OnSubmitInner( BaseEventData data )
		{
			InvokeEventTriggerCallback( EventTriggerType.Submit, data ) ;
		}

		private void OnCancelInner( BaseEventData data )
		{
			InvokeEventTriggerCallback( EventTriggerType.Cancel, data ) ;
		}
	
		private void InvokeEventTriggerCallback( EventTriggerType type, BaseEventData data )
		{
			if( m_EventTriggerCallbackList.ContainsKey( type ) == false )
			{
				// コールバックが登録されていない
				return ;
			}
		
			string identity = Identity ;
			if( string.IsNullOrEmpty( identity ) == true )
			{
				identity = name ;
			}

			PointerEventData pointer = data as PointerEventData ;
			m_PointerId = pointer.pointerId ;
			m_PointerPosition = GetLocalPosition( pointer ) ;

			m_EventTriggerCallbackList[ type ]( identity, this, type ) ;
		}

		private int m_PointerId = -1 ;
		private Vector2 m_PointerPosition = Vector2.zero ;

		/// <summary>
		/// Pointer が Collider の内側にあるか判定する
		/// </summary>
		public bool IsPointerInside
		{
			get
			{
				Vector3 position ;


				if( m_PointerId == -1 )
				{
					position = Input.mousePosition ;
					position = GetLocalPosition( position ) ;
				}
				else
				{
					position = GetLocalPosition( m_PointerPosition ) ;
				}

				float x = position.x ;
				float y = position.y ;

				float w = this.Width ;
				float h = this.Height ;

				Vector2 p = Pivot ;

				float xMin = - ( w * p.x ) ;
				float xMax = w * ( 1.0f - p.x ) ;
				float yMin = - ( h * p.y ) ;
				float yMax = h * ( 1.0f - p.y ) ;

				if( x <  xMin || x >  xMax || y <  yMin || y >  yMax )
				{
					return false ;	// 外
				}

				return true ;	// 中
			}
		}

		// そのＵＩ上の座標を取得する
		protected Vector2 GetLocalPosition( PointerEventData pointer )
		{
			return GetLocalPosition( pointer.position ) ;
		}

		//---------------------------------------------------------------------

		/// <summary>
		/// 現在タッチされている数を取得する
		/// </summary>
		/// <returns></returns>
		public int GetTouchCount()
		{
			int count = Input.touchCount ;

			if( count == 0 )
			{
				// 本当にタッチが無い場合のみマウスの入力を使用する
				// 注意:タッチするとエミュレーション的に Input.GetMouseButton() が反応してしまう
				int i ;
				for( i  = 0 ; i <= 2 ; i ++ )
				{
					if( Input.GetMouseButton( i ) == true )
					{
						count = 1 ;
					}
				}
			}

			return count ;
		}


		private bool	m_SingleTouchState = false ;
		private int		m_SingleTouchFingerId = 0 ;

		/// <summary>
		/// レイキャストのブロックキングに関わらず現在のタッチ情報を取得する
		/// </summary>
		/// <param name="rPosition"></param>
		/// <returns></returns>
		public ( int, Vector2 ) GetSinglePointer()
		{
			int i ;
			int state = 0 ;
			Vector2 pointer = Vector2.zero ;
			
			if( Input.touchCount == 1 )
			{
				// 押された
				Touch touch = Input.GetTouch( 0 ) ;

				if( m_SingleTouchState == false )
				{
					m_SingleTouchState = true ;
					m_SingleTouchFingerId = touch.fingerId ;

					pointer = touch.position ;

					state = 1 ;
				}
				else
				{
					if( m_SingleTouchFingerId == touch.fingerId )
					{
						pointer = touch.position ;

						state = 1 ;
					}
					else
					{
						// ここに来ることは基本時にありえない
					}
				}
			}
			else
			if( Input.touchCount >= 2 )
			{
				// 複数押された
				state = 0 ;
			}
			else
			{
				// 離された
				m_SingleTouchState = false ;

				state = 0 ;
			}

			if( state != 0 )
			{
				// ローカル座標への変換を行う
				pointer = GetLocalPosition( pointer ) ;
			}

			//----------------------------------------------------------

			if( Input.touchCount == 0 )
			{
				// 本当にタッチが無い場合のみマウスの入力を使用する
				// 注意:タッチするとエミュレーション的に Input.GetMouseButton() が反応してしまう
				for( i  = 0 ; i <= 2 ; i ++ )
				{
					if( Input.GetMouseButton( i ) == true )
					{
						state |= ( 1 << i ) ;
						pointer = GetLocalPosition( Input.mousePosition ) ;
					}
				}
			}

			//----------------------------------------------------------
			
			return ( state, pointer ) ;
		}

		private readonly bool[]	m_MultiTouchState    = new bool[ 10 ] ;
		private readonly int[]	m_MultiTouchFingerId = new int[ 10 ] ;

		/// <summary>
		/// レイキャストのブロックキングに関わらず現在のタッチ情報を取得する
		/// </summary>
		/// <param name="rPointer"></param>
		/// <returns></returns>
		public ( int, Vector2[] ) GetMultiPointer()
		{
			int i, l ;
			int states = 0 ;
			bool[] entries = new bool[ 10 ] ;
			Vector2[] pointers = new Vector2[ 10 ] ;

			int j, c, e ;

			l = m_MultiTouchState.Length ;

			c = Input.touchCount ;
			if( c >  0 )
			{
				for( i  = 0 ; i <  c ; i ++ )
				{
					// ＩＤが同じものを検査して存在するなら上書きする
					// ＩＤが同じものが存在しないなら新規に追加する

					Touch touch = Input.GetTouch( i ) ;
					int fingerId = touch.fingerId ;
					Vector2 position = touch.position ;

					e = -1 ;
					for( j  = 0 ; j <  l ; j ++ )
					{
						if( m_MultiTouchState[ j ] == true )
						{
							if( m_MultiTouchFingerId[ j ] == fingerId )
							{
								// 既に登録済み
								entries[ j ] = true ;
								pointers[ j ] = position ;
								states |= ( 1 << j ) ;
								break ;
							}
						}
						else
						{
							// 空いているスロットを発見した
							if( e <  0 )
							{
								e  = j ;
							}
						}	
					}

					if( j >= l && e <  l )
					{
						// 新規登録
						m_MultiTouchState[ e ] = true ;
						m_MultiTouchFingerId[ e ] = fingerId ;

						entries[ e ] = true ;
						pointers[ e ] = position ;
						states |= ( 1 << e ) ;
					}
				}
			}

			// 新規登録または上書更新が無かったスロットを解放する
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( entries[ i ] == false )
				{
					m_MultiTouchState[ i ] = false ;
				}
			}

			if( states != 0 )
			{
				l = entries.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( entries[ i ] == true )
					{
						pointers[ i ] = GetLocalPosition( pointers[ i ] ) ;
					}
				}
			}

			//----------------------------------------------------------

			// マウスのダミー処理
			if( Input.touchCount == 0 )
			{
				// 本当にタッチが無い場合のみマウスの入力を使用する
				// 注意:タッチするとエミュレーション的に Input.GetMouseButton() が反応してしまう
				c = 0 ; j = -1 ;
				for( i  = 0 ; i <= 2 ; i ++ )
				{
					if( Input.GetMouseButton( i ) == true )
					{
						states |= ( 1 << i ) ;
						entries[ i ] = true ;
						pointers[ i ] = GetLocalPosition( Input.mousePosition ) ;

						c ++ ;
						j = i ;
					}
				}

				// 注意:Unity Remote 5 を起動しているとキーボードの押下が取れない

				if( c == 1 )
				{
#if UNITY_EDITOR
					if( ( Input.GetKey( KeyCode.LeftControl ) == true || Input.GetKey( KeyCode.RightControl ) == true ) && Input.GetKey( KeyCode.X ) == true )
					{
						j ++ ;
						states |= ( 1 << j ) ;
						pointers[ j ] = new Vector2( - pointers[ j - 1 ].x, pointers[ j - 1 ].y ) ;
					}
					if( ( Input.GetKey( KeyCode.LeftControl ) == true || Input.GetKey( KeyCode.RightControl ) == true ) && Input.GetKey( KeyCode.Y ) == true )
					{
						j ++ ;
						states |= ( 1 << j ) ;
						pointers[ j ] = new Vector2( pointers[ j - 1 ].x, - pointers[ j - 1 ].y ) ;
					}
#endif
				}
			}

			//----------------------------------------------------------

			return ( states, pointers ) ;
		}

		// そのＵＩ上の座標を取得する
		protected Vector2 GetLocalPosition( Vector2 position )
		{
			Canvas canvas = GetParentCanvas() ;
			if( canvas != null )
			{
				if( canvas.renderMode == RenderMode.ScreenSpaceOverlay )
				{
					position = transform.InverseTransformPoint( position ) ;
				}
				else
				if( canvas.renderMode == RenderMode.ScreenSpaceCamera || canvas.renderMode == RenderMode.WorldSpace )
				{
					if( canvas.worldCamera != null && canvas.worldCamera.isActiveAndEnabled == true )
					{
						Vector2 canvasSize = GetCanvasSize() ;

						position = canvas.worldCamera.ScreenToWorldPoint( position ) ;

						position.x -= transform.position.x ;
						position.y -= transform.position.y ;

						if( canvas.worldCamera.orthographic == true )
						{
							float height = canvas.worldCamera.orthographicSize ;
							float k = ( canvasSize.y * 0.5f ) / height ;
							position.x *= k ;
							position.y *= k ;
						}
					}
					else
					{
						Vector2 canvasSize = GetCanvasSize() ;

						// キャンバス上の座標に変換する
						position.x = ( ( position.x / ( float )Screen.width  ) - 0.5f ) * canvasSize.x ;
						position.y = ( ( position.y / ( float )Screen.height ) - 0.5f ) * canvasSize.y ;

						Vector2 center = PositionInCanvas ;

						position -= center ;
					}
				}

				return position ;
			}

			return Vector2.zero ;
		}

		/// <summary>
		/// マウスのホイール値を取得する
		/// </summary>
		/// <returns></returns>
		public float GetWheelDistance()
		{
			 return Input.GetAxis( "Mouse ScrollWheel" ) ;
		}


		/// <summary>
		/// ピンチの距離を取得する
		/// </summary>
		/// <returns></returns>
		public float GetPinchDistance()
		{
			( int state, Vector2[] pointers ) = GetMultiPointer() ;
			if( state == 3 )
			{
				// 最初の２点限定
				return ( pointers[ 1 ] - pointers[ 0 ] ).magnitude ;
			}

			return 0 ;
		}


		//--------------------------------------------------------------------

		/// <summary>
		/// GameObject を　Active にする(ショートカット)
		/// </summary>
		/// <param name="state"></param>
		public void SetActive( bool state )
		{
			gameObject.SetActive( state ) ;
		}

		/// <summary>
		/// GameObject の Active の有無(ショートカット)
		/// </summary>
		public bool ActiveSelf
		{
			get
			{
				return gameObject.activeSelf ;
			}
		}

		/// <summary>
		/// GameObject の ActiveInHierarchy の有無(ショートカット)
		/// </summary>
		public bool ActiveInHierarchy
		{
			get
			{
				return gameObject.activeInHierarchy ;
			}
		}

		/// <summary>
		/// 親を設定する(ショートカット)
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="flag"></param>
		public void SetParent( Transform parent, bool flag )
		{
			transform.SetParent( parent, flag ) ;
		}

		/// <summary>
		/// 親を設定する(ショートカット)
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="flag"></param>
		public void SetParent( UIView parentView, bool flag )
		{
			transform.SetParent( parentView.transform, flag ) ;
		}

		/// <summary>
		/// UI の表示順番(ショートカット)
		/// </summary>
		public int SiblingIndex
		{
			get
			{
				return transform.GetSiblingIndex() ;
			}
			set
			{
				transform.SetSiblingIndex( value ) ;
			}
		}

		/// <summary>
		/// UI の表示順番を取得する
		/// </summary>
		/// <returns></returns>
		public int GetSiblingIndex()
		{
			return SiblingIndex ;
		}

		/// <summary>
		/// UI の表示順番を設定する
		/// </summary>
		/// <param name="index"></param>
		public void SetSiblingIndex( int index )
		{
			SiblingIndex = index ;
		}

		/// <summary>
		/// UI の表示を最も奥にする(ショートカット)
		/// </summary>
		public void SetAsFirstSibling()
		{
			transform.SetAsFirstSibling() ;
		}

		/// <summary>
		/// UI の表示を最も手前にする(ショートカット)
		/// </summary>
		public void SetAsLastSibling()
		{
			transform.SetAsLastSibling() ;
		}

		/// <summary>
		/// Z 値に応じて表示順番を並び替える
		/// </summary>
		/// <param name="reverse"></param>
		public void SortChildByZ( bool reverse = false )
		{
			// 直接の子をＺ値に従ってソートする

			// 方法としては、最もＺが大きいものから順に手前に設定していく。

			int i, j, l = transform.childCount ;
			Transform[] t = new Transform[ l ] ;
			Transform s ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				t[ i ] = transform.GetChild( i ) ;
			}

			if( reverse == false )
			{
				// 昇順（値が小さいものを奥に）
				for( i  = 0 ; i <  ( l - 1 ) ; i ++  )
				{
					for( j  = ( i + 1 ) ; j <  l ; j ++ )
					{
						if( t[ j ].localPosition.z <  t[ i ].localPosition.z )
						{
							// 入れ替え
							s = t[ i ] ;
							t[ i ] = t[ j ] ;
							t[ j ] = s ;
						}
					}
				}
			}
			else
			{
				// 降順（値が大きいものを奥に）
				for( i  = 0 ; i <  ( l - 1 ) ; i ++  )
				{
					for( j  = ( i + 1 ) ; j <  l ; j ++ )
					{
						if( t[ j ].localPosition.z >  t[ i ].localPosition.z )
						{
							// 入れ替え
							s = t[ i ] ;
							t[ i ] = t[ j ] ;
							t[ j ] = s ;
						}
					}
				}
			}

			for( i  = 0 ; i <  l ; i ++ )
			{
				// 配列の最初のものから順に最前面に（最終的に最背面になる）
				t[ i ].SetAsLastSibling() ;
			}
		}

		/// <summary>
		/// ゲームオブジェクトの複製を行う(親や姿勢は引き継ぐ)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public GameObject Duplicate()
		{
			GameObject clone = Instantiate( gameObject, transform.parent ) ;
			return clone ;
		} 
		
		/// <summary>
		/// ゲームオブジェクトを複製し指定のコンポーネントを取得する(親や姿勢は引き継ぐ)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T Duplicate<T>() where T : UnityEngine.Component
		{
			GameObject clone = Instantiate( gameObject, transform.parent ) ;
			return clone.GetComponent<T>() ;
		}

		/// <summary>
		/// ゲームオブジェクトを破棄する
		/// </summary>
		public void Destroy()
		{
#if UNITY_EDITOR
			if( Application.isPlaying == false )
			{
				DestroyImmediate( gameObject ) ;
			}
			else
#endif
			{
				Destroy( gameObject ) ;
			}
		}

		/// <summary>
		/// 指定したビューからの相対的な距離を取得する
		/// </summary>
		/// <param name="baseView"></param>
		/// <returns></returns>
		public Vector2 GetDistance( UIView baseView )
		{
			Vector2 t1 = transform.position ;
			Vector2 t0 = baseView.transform.position ;

			return t1 - t0 ;
		}

		/// <summary>
		/// 指定したビューからの相対的な距離(X)を取得する
		/// </summary>
		/// <param name="baseView"></param>
		/// <returns></returns>
		public float GetDistanceX( UIView baseView )
		{
			return GetDistance( baseView ).x ;
		}

		/// <summary>
		/// 指定したビューからの相対的な距離(Y)を取得する
		/// </summary>
		/// <param name="baseView"></param>
		/// <returns></returns>
		public float GetDistanceY( UIView baseView )
		{
			return GetDistance( baseView ).y ;
		}

		/// <summary>
		/// ヒエラルキーでの階層パス名を取得する
		/// </summary>
		public string Path
		{
			get
			{
				string path = name ;

				Transform t = transform.parent ;
				while( t != null )
				{
					path = t.name + "/" + path ;
					t = t.parent ;
				}
				return path ;
			}
		}

		//-------------------------------------------------------------------
		// プレス待ち

		private bool m_WaitForPress = false ;

		public AsyncState WaitForPress()
		{
			if( m_WaitForPress == true )
			{
				return null ;
			}

			m_WaitForPress = false ;
			AsyncState state = new AsyncState( this ) ;
			StartCoroutine( WaitForPress_Private( state ) ) ;

			return state ;
		}

		private IEnumerator WaitForPress_Private( AsyncState state )
		{
			yield return new WaitWhile( () => m_WaitForPress == false ) ;

			m_WaitForPress = false ;
			state.IsDone = true ;
		}

		//-------------------------------------------------------------------
		// クリック待ち

		private bool m_WaitForClick = false ;

		public AsyncState WaitForClick()
		{
			if( m_WaitForClick == true )
			{
				return null ;
			}

			m_WaitForClick = false ;
			AsyncState state = new AsyncState( this ) ;
			StartCoroutine( WaitForClick_Private( state ) ) ;

			return state ;
		}

		private IEnumerator WaitForClick_Private( AsyncState state )
		{
			while( m_WaitForClick == false )
			{
				yield return null ;
			}

			m_WaitForClick = false ;
			state.IsDone = true ;
		}

		//-------------------------------------------------------------------------------------------
		// アニメーションの再生

		// アニメーション再生中の待機オブジェクト
//		private AsyncState m_ActiveAnimation = null ;

		// アニメーションの中断要求
//		private bool m_BreakAnimation = false ;

		public class ActiveAnimation
		{
			public AsyncState	State ;
			public bool			Break ;

			public string		Name ;		// ステート名
			public float		Duration ;	// 再生時間
			public bool			IsLoop ;	// ループするか
		}

		public class ActiveAnimatorState
		{
			public string		Name ;		// ステート名
			public float		Duration ;	// 再生時間
			public bool			IsLoop ;	// ループするか
		}

		// 実行中のアニメーション情報
		private Dictionary<int,ActiveAnimation>	m_ActiveAnimations ;

		// イベント
		private Action<string>	m_OnAnimationEvents ;

		/// <summary>
		/// アニメーションが再生中かどうか
		/// </summary>
		public bool IsPlayingAnimation( int layer = 0 )
		{
			if( m_ActiveAnimations == null )
			{
				return false ;
			}

			return m_ActiveAnimations.ContainsKey( layer ) ;
		}

		/// <summary>
		/// 全レイヤーの Animator を停止させる
		/// </summary>
		/// <returns></returns>
		public bool StopAllAnimators()
		{
			if( CAnimator == null )
			{
				Debug.LogWarning( "Not found Animator Component" ) ;
				return false ;
			}

//			if( m_Animator.enabled == false )
//			{
//				Debug.LogWarning( "[StopAllAnimators] Animator Component is disabled : " + Path ) ;
//				return false ;
//			}

			//----------------------------------------------------------

			if( m_ActiveAnimations != null && m_ActiveAnimations.Count >  0 )
			{
				int[] layers = new int[ m_ActiveAnimations.Count ] ;
				m_ActiveAnimations.Keys.CopyTo( layers, 0 ) ;

				foreach( var layer in layers )
				{
					if( m_ActiveAnimations.ContainsKey( layer ) == true )
					{
						// 強制中断
						m_ActiveAnimations[ layer ].Break  = false ;

						m_ActiveAnimations[ layer ].State.IsDone = true ;
						m_ActiveAnimations[ layer ].State = null ;

						m_ActiveAnimations.Remove( layer ) ;
					}
				}
			}

			m_Animator.enabled = false ;

			//----------------------------------------------------------

			return true ;
		}


		/// <summary>
		/// Animator の停止
		/// </summary>
		/// <returns></returns>
		public bool StopAnimator( int layer = 0 )
		{
			if( CAnimator == null )
			{
				Debug.LogWarning( "Not found Animator Component" ) ;
				return false ;
			}

//			if( m_Animator.enabled == false )
//			{
//				Debug.LogWarning( "[StopAnimator] Animator Component is disabled : " + Path ) ;
//				return false ;
//			}

			//----------------------------------------------------------

			if( m_ActiveAnimations != null )
			{
				if( m_ActiveAnimations.ContainsKey( layer ) == true )
				{
					// 強制中断
					m_ActiveAnimations[ layer ].Break  = false ;

					m_ActiveAnimations[ layer ].State.IsDone = true ;
					m_ActiveAnimations[ layer ].State = null ;

					m_ActiveAnimations.Remove( layer ) ;
				}
			}

//			m_Animator.Play( "Entry", layer ) ;
			m_Animator.enabled = false ;

			//----------------------------------------------------------

			return true ;
		}

		/// <summary>
		/// アクティブなアニメーターのステート情報を取得する
		/// </summary>
		/// <param name="layer"></param>
		/// <returns></returns>
		public ActiveAnimatorState GetAnimatorState( int layer = 0 )
		{
			if( m_ActiveAnimations.ContainsKey( layer ) == false )
			{
				return null ;	// アクティブなアニメーターのステート情報は存在しない
			}

			var animation = m_ActiveAnimations[ layer ] ;

			ActiveAnimatorState state = new ActiveAnimatorState()
			{
				Name		= animation.Name,
				Duration	= animation.Duration,
				IsLoop		= animation.IsLoop
			} ;

			return state ;
		}

		/// <summary>
		/// 現在のアニメーションはループするものか判定する
		/// </summary>
		/// <param name="layer"></param>
		/// <returns></returns>
		public bool IsAnimationLooping( int layer = 0 )
		{
			if( m_ActiveAnimations.ContainsKey( layer ) == false )
			{
				return false ;	// アクティブなアニメーターのステート情報は存在しない
			}

			var animation = m_ActiveAnimations[ layer ] ;
			return animation.IsLoop ;
		}

		/// <summary>
		/// アニメーターの速度を設定する
		/// </summary>
		/// <param name="speed"></param>
		/// <returns></returns>
		public bool SetAnimatorSpeed( float speed )
		{
			if( m_Animator == null )
			{
				return false ;
			}

			m_Animator.speed = speed ;

			return true ;
		}

		/// <summary>
		/// Animatorの再生
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public AsyncState PlayAnimator( string stateName, int layer = 0, float offset = 0, bool waitForFinish = true, Action<string> onEvent = null, Action onStarted = null, Action<bool> onFinished = null )
		{
			if( CAnimator == null )
			{
				Debug.LogWarning( "Not found Animator Component : [PlayAnimator] " + stateName ) ;
				return null ;
			}

//			if( m_Animator.enabled == false )
//			{
//				Debug.LogWarning( "Animator Component is disabled : [PlayAnimator] " + stateName ) ;
//				return null ;
//			}
			m_Animator.enabled = true ;

			if( offset >= 1.0f )
			{
				Debug.LogWarning( "Bad offset : [PlayAnimator] " + stateName + " offset = " + offset ) ;
				return null ;
			}

			if( gameObject.activeSelf == false )
			{
				gameObject.SetActive( true ) ;
			}

			if( gameObject.activeInHierarchy == false )
			{
				return null ;
			}

			if( m_ActiveAnimations == null )
			{
				m_ActiveAnimations  = new Dictionary<int, ActiveAnimation>() ;
			}

			//----------------------------------------------------------
			// １フレーム目からアニメーションの状態を反映させるため０フレームでアニメーションを実行する

			AsyncState state = new AsyncState( this ) ;
			StartCoroutine( PlayAnimator_Private( stateName, layer, offset, waitForFinish, onEvent, onStarted, onFinished, state ) ) ;
			return state ;
		}

		private IEnumerator PlayAnimator_Private( string stateName, int layer, float offset, bool waitForFinish, Action<string> onEvent, Action onStarted, Action<bool> onFinished, AsyncState state )
		{
			//------------------------------------------------------------------------------------------
			// 以前の再生の終了を待つ

			if( m_ActiveAnimations.ContainsKey( layer ) == true )
			{
//				Debug.Log( "----->別のアニメーションを再生中なのかもしれない:" + stateName + " -> " + m_ActiveAnimations[ layer ].Name + " " + m_ActiveAnimations[ layer ].IsLoop ) ;
				if( waitForFinish == false || m_ActiveAnimations[ layer ].IsLoop == true )
				{
					// 強制中断
					m_ActiveAnimations[ layer ].Break  = false ;

					m_ActiveAnimations[ layer ].State.IsDone = true ;
					m_ActiveAnimations[ layer ].State = null ;

					m_ActiveAnimations.Remove( layer ) ;
				}
				else
				{
					// 別のアニメーションを終了を待つ
					while( m_ActiveAnimations.ContainsKey( layer ) == true )
					{
						yield return null ;
					}
				}
			}

			//------------------------------------------------------------------------------------------
			// 新規に再生を行う

			//----------------------------------

//			Debug.Log( "アニメーション実行1:" + Path + " " + stateName ) ;

			// コールバックの設定は再生よりも前のタイミングで行う
			if( onEvent != null )
			{
				// 任意イベントのハンドラーを追加する
				m_OnAnimationEvents -= onEvent ;
				m_OnAnimationEvents += onEvent ;
			}

			// 新しいアニメショーンを再生する
			m_Animator.Play( stateName, layer, offset ) ;

			m_Animator.Update( 0 ) ;	// 強制的に設定したステートの開始状態にする(重要)

			AnimatorStateInfo stateInfo = m_Animator.GetCurrentAnimatorStateInfo( layer ) ;

			if( stateInfo.IsName( stateName ) == false )
			{
				// 再生できない

				// コールバックを除去する
				if( onEvent != null )
				{
					m_OnAnimationEvents -= onEvent ;
				}

				Debug.LogWarning( "[Animator] Can not play. StateName = " + stateName ) ;
				state.IsDone = true ;
				yield break ;
			}

			// 待機オブジェクトを保存する(全体で１つ)
			m_ActiveAnimations.Add( layer, new ActiveAnimation() ) ;
			m_ActiveAnimations[ layer ].State			= state ;
			m_ActiveAnimations[ layer ].State.option	= stateName ;
			m_ActiveAnimations[ layer ].Name			= stateName ;

			m_Animator.speed = m_TimeScale ;

			//------------------------------------------------------------------------------------------
			// 新規の再生が実際に始められたのを待つ(この中はおそらく実行されなくなったはず)

			// Update( 0 ) があれば以下は不要だが念の為
			if( stateInfo.IsName( stateName ) == false || stateInfo.normalizedTime >  1.0f )
			{
				Debug.Log( "<color=#FF00FF>[Animator]別のステートが実行中の可能性がある: 目的のステート = " + stateName + " 再生中になったか = " + stateInfo.IsName( stateName ) + " カレントのプログレス = " + stateInfo.normalizedTime + "</color>" ) ;

				// 前回のステート状態が残っている(Play実行直後ではまだ残っている)
				while( true )
				{
					if( m_ActiveAnimations[ layer ].Break == true )
					{
						// 中断
//						Debug.Log( "[中断0]" + stateName ) ;
						m_ActiveAnimations[ layer ].Break  = false ;

						m_ActiveAnimations[ layer ].State.IsDone = true ;
						m_ActiveAnimations[ layer ].State = null ;

						m_ActiveAnimations.Remove( layer ) ;
						yield break ;
					}

					// ステートを取り直す
					stateInfo = m_Animator.GetCurrentAnimatorStateInfo( layer ) ;
					if( stateInfo.IsName( stateName ) == true && stateInfo.normalizedTime <  1.0f )
					{
						// 新しいステートが開始された
						break ;
					}

					yield return null ;
				}
			}

			//--------------------------------------------------------------------------

			// 構造体なので毎フレーム取得して値を確認する必要がある
			stateInfo = m_Animator.GetCurrentAnimatorStateInfo( layer ) ;

			m_ActiveAnimations[ layer ].Duration	= stateInfo.length ;
			m_ActiveAnimations[ layer ].IsLoop		= stateInfo.loop ;

			// 開始コールバック
			onStarted?.Invoke() ;

//			Debug.Log( "アニメーション実行2:" + stateName + " " + stateInfo.IsName( stateName ) + " " + stateInfo.loop ) ;

			//------------------------------------------------------------------------------------------
			// 新規の再生の終了を待つ

			// ワンショットもループも関係なく終了待ちに入る
			while( true )
			{
//				Debug.Log( "ループ中:" + stateName + " " + m_ActiveAnimations[ layer ].IsLoop  ) ;

				if( m_ActiveAnimations == null || m_ActiveAnimations.Count == 0 )
				{
					// 別で強制停止させられた
//					Debug.Log( "[中断1]" + stateName ) ;

					if( onEvent != null )
					{
						// 任意イベントのハンドラーを削除する
						m_OnAnimationEvents -= onEvent ;
					}

					// 中断コールバック
					onFinished?.Invoke( false ) ;

					yield break ;
				}

				if( m_ActiveAnimations[ layer ].Break == true )
				{
					// 中断(ループの場合はここで中断を待つしか無い)
//					Debug.Log( "[中断2]" + stateName ) ;

					if( onEvent != null )
					{
						// 任意イベントのハンドラーを削除する
						m_OnAnimationEvents -= onEvent ;
					}

					// 中断コールバック
					onFinished?.Invoke( false ) ;

					//---------------------------------

					// 終了
					m_ActiveAnimations[ layer ].Break  = false ;

					m_ActiveAnimations[ layer ].State.IsDone = true ;	// コルーチン終了
					m_ActiveAnimations[ layer ].State = null ;

					m_ActiveAnimations.Remove( layer ) ;

					yield break ;
				}

				// ワンショットのみ終了時間をチェックする
				if( m_ActiveAnimations[ layer ].IsLoop == false )
				{
					// 構造体なので毎フレーム取得して値を確認する必要がある
					stateInfo = m_Animator.GetCurrentAnimatorStateInfo( layer ) ;
//					Debug.Log( "状態:" + stateInfo.normalizedTime ) ;
					if( stateInfo.normalizedTime >= 1.0f )
					{
						// 再生終了
						break ;
					}
				}

				yield return null ;
			}

//			Debug.Log( "アニメーション実行3:" + stateName ) ;

			if( onEvent != null )
			{
				// 任意イベントのハンドラーを削除する
				m_OnAnimationEvents -= onEvent ;
			}

			//---------------------------------

			// 終了
			m_ActiveAnimations[ layer ].Break  = false ;
	
			m_ActiveAnimations[ layer ].State.IsDone = true ;	// コルーチン終了
			m_ActiveAnimations[ layer ].State = null ;

			m_ActiveAnimations.Remove( layer ) ;

			//----------------------------------

			// 完了コールバック
			onFinished?.Invoke( true ) ;
		}

		/// <summary>
		/// アニメーションクリップのイベントハンドラー
		/// </summary>
		/// <param name="identity"></param>
		public void OnAnimationEvent( string identity )
		{
			m_OnAnimationEvents?.Invoke( identity ) ;
		}

		//-------------------------------------------------------------------------------------------

		//-------------------------------------------------------------------------------------------

		private PointerEventData	m_VRH_EventData		= null ;
		private List<RaycastResult>	m_VRH_Results		= null ;

		/// <summary>
		/// ポインターが上にあるリストビュー内のアイテムのインデックスを取得する
		/// </summary>
		/// <returns></returns>
		public bool IsRaycastHit()
		{
			if( EventSystem.current == null )
			{
				// まだ準備が整っていない
				return false ;
			}

			//----------------------------------------------------------

			Vector2 position ;

#if UNITY_EDITOR || UNITY_STANDALONE

			position = Input.mousePosition ;

#elif !UNITY_EDITOR && ( UNITY_ANDROID || UNITY_IOS )

			if( Input.touchCount == 1 )
			{
				position = Input.touches[ 0 ].position ;
			}
			else
			{
				return false ;
			}

#else
			return false ;
#endif

			// スクリーン座標からRayを飛ばす

			if( m_VRH_EventData == null )
			{
				m_VRH_EventData		= new PointerEventData( EventSystem.current ) ;
			}

			if( m_VRH_Results == null )
			{
				m_VRH_Results		= new List<RaycastResult>() ;
			}

			m_VRH_EventData.position = position ;
			m_VRH_Results.Clear() ;

			// レイキャストで該当するＵＩを探す
			EventSystem.current.RaycastAll( m_VRH_EventData, m_VRH_Results ) ;

			if( m_VRH_Results.Count >= 1 )
			{
				GameObject target = m_VRH_Results[ 0 ].gameObject ;

				while( true )
				{
					if( gameObject == target )
					{
						// 発見しました
						return true ;
					}

					if( target.transform.parent != null )
					{
						target = target.transform.parent.gameObject ;
					}
					else
					{
						break ;
					}
				}
			}

			return false ;
		}

	}
}

// メモ
// iTween でわかりやすい
// http://d.hatena.ne.jp/nakamura001/20121127/1354021902

