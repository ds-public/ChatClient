//#define USE_BLUR

using System ;
using System.Collections ;
using System.Collections.Generic ;

using Cysharp.Threading.Tasks ;

using UnityEngine ;

using uGUIHelper ;

namespace Template.Dialogs
{
	/// <summary>
	/// ダイアログ基底クラス Version 2022/05/04
	/// </summary>
	public class DialogBase : ExMonoBehaviour
	{
		[SerializeField]
		protected UIImage			m_Background ;

		[SerializeField]
		protected UIView			m_Fade ;

		[SerializeField]
		protected UIImage			m_Window ;
		
		//-----------------------------------------------------------

		/// <summary>
		/// シーンを常駐させるかどうか
		/// </summary>
		public bool					IsResident = false ;

		//-------------------------------------------------------------------------------------------

		private string				m_DialogSceneName	= string.Empty ; 

		// 閉じる前に確認が必要な場合のコールバック
//		private Action<Action>		m_OnCloseConfirm ;

		// 閉じられた際に呼び出されるコールバック
//		private Action				m_OnClosed ;

		// 外側タッチでダイアログを閉じるかどうか
		private bool				m_OutsideEnabled = true ;

		// 最初からタッチ状態かの判定に利用するフラグ
		private bool				m_IsPressing ;

		// 外側がタッチされた際に呼ぶコールバック
		private Action				m_OnOutsidePress ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 外側タッチでダイアログを閉じるかどうか
		/// </summary>
		public bool OutsideEnabled
		{
			get
			{
				return m_OutsideEnabled ;
			}
			set
			{
				m_OutsideEnabled = value ;
			}
		}

		/// <summary>
		/// 外側がタッチされた際に呼ぶコールバックを登録する
		/// </summary>
		/// <param name="onOutsidePress"></param>
		protected void SetOnOutsidePress( Action onOutsidePress )
		{
			m_OnOutsidePress = onOutsidePress ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ウィンドウのフェードタイプ
		/// </summary>
		public enum WindowFadeTypes
		{
			Tween,
			Animator
		}

		private WindowFadeTypes	m_WindowFadeType = WindowFadeTypes.Tween ;

		/// <summary>
		/// ウィンドウのフェードタイプ
		/// </summary>
		public WindowFadeTypes WindowFadeType
		{
			get
			{
				return m_WindowFadeType ;
			}
			set
			{
				m_WindowFadeType = value ;
			}
		}
		
		//-------------------------------------------------------------------------------------------

		virtual protected string SetDialogSceneName(){ return string.Empty ; }
		
		internal void Awake()
		{
			// 初期状態では非表示にしておく
			if( m_Fade != null )
			{
				m_Fade.SetActive( false ) ;
			}

			if( m_Window != null )
			{
				m_Window.SetActive( false ) ;
			}

			//----------------------------------------------------------

			m_DialogSceneName = SetDialogSceneName() ;

			//----------------------------------------------------------

			// ApplicationManager を起動する(最初からヒエラルキーにインスタンスを生成しておいても良い)
			ApplicationManager.Create() ;

			OnAwake() ;

			//----------------------------------------------------------

			// カメラは Awake() で設定しないと一瞬表示されてしまうので注意
			if( m_Background != null )
			{
				m_Background.SetActive( UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == m_DialogSceneName ) ;
			}
		}

		virtual protected void OnAwake(){}

		// 呼ばれる順番は、Awake -> Start -> Setup

		// Start は async void が正しい
		internal async void Start()
		{
			PrepareBase().Forget() ;

			await Yield() ;
			// Start は完全に終了させる必要がある
		}

		// 準備
		private async UniTask PrepareBase()
		{
			// ApplicationManager の準備が整うのを待つ
			if( ApplicationManager.IsInitialized == false )
			{
				await WaitUntil( () => ApplicationManager.IsInitialized ) ;
			}

			//----------------------------------------------------------

			await OnStart() ;

			//----------------------------------------------------------

#if UNITY_EDITOR
			if( UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == m_DialogSceneName )
			{
				// デバッグ

				//---------------------------------------------------------

				// デバッグ用の準備処理を行う
				if( await Screens.Downloading.SimpleSetup( this ) == false )
				{
					// 失敗
					Debug.LogWarning( "セットアップに失敗しました" ) ;
					return ;
				}

				//---------------------------------------------------------

				await RunDebug() ;
			}
#endif
		}

		virtual protected async UniTask OnStart(){ await Yield() ; }
		virtual protected async UniTask RunDebug(){ await Yield() ; }

		//-------------------------------------------------------------------------------------------

		protected UIView GetFade()
		{
			return m_Fade ;
		}

		protected UIImage GetWindow()
		{
			return m_Window ;
		}
		
		/// <summary>
		/// ダイアログを開く(ダイアログが閉じられるまでコルーチンは終了しない)
		/// </summary>
		/// <returns></returns>
		protected async UniTask OpenBase( Action finishAction = null, Func<UniTask> waitingAction = null )
		{
			gameObject.SetActive( true ) ;

			if( m_Fade != null )
			{
				m_Fade.SetActive( true ) ;

				if( m_Fade is UIRawImage )
				{
					UIRawImage rawImage = m_Fade as UIRawImage ;  
					rawImage.Color = new Color( 0, 0, 0, 0.5f ) ;
				}

				_ = m_Fade.PlayTween( "FadeIn" ) ;
			}

			if( m_Window != null )
			{
				m_Window.SetActive( true ) ;

				if( m_WindowFadeType == WindowFadeTypes.Tween )
				{
					_ = m_Window.PlayTween( "FadeIn" ) ;
				}
				else
				if( m_WindowFadeType == WindowFadeTypes.Animator )
				{
					_ = m_Window.PlayAnimator( "In" ) ;
				}
			}

			if( m_Fade != null || m_Window != null )
			{
				while
				(
					( m_Fade != null && m_Fade.IsAnyTweenPlaying == true ) ||
					( m_Window != null && ( m_Window.IsAnyTweenPlaying == true || m_Window.IsPlayingAnimation() == true ) )
				)
				{
					await Yield() ;
				}
			}

			if( waitingAction != null )
			{
				await WhenAll( waitingAction() ) ;
			}

			//----------------------------------

			// 外側タッチで閉じる処理

			// ウィンドウ部分はタッチしても無効化するためレイキャストを有効にする
			m_Window.RaycastTarget = true ;

			//--------------

			if( m_Fade is UIRawImage )
			{
				UIRawImage fade = m_Fade as UIRawImage ;

				// 最初からタッチしていた場合は一度タッチを解除する必要がありそのためのフラグ
				m_IsPressing = Input.GetMouseButton( 0 );

				fade.IsInteraction = true ;
				fade.RaycastTarget = true ;
				fade.SetOnSimplePress( ( bool state ) =>
				{
					if( m_OutsideEnabled == true )
					{
						if( state == true  )
						{
							if( m_IsPressing == false )
							{
								// 外側を触れたら閉じる扱いとする

								// 外側がタッチされた際のコールバックを呼ぶ
								m_OnOutsidePress?.Invoke() ;

								m_IsPressing = true ;
							}
						}
						else
						{
							m_IsPressing = false ;
						}
					}
				} ) ;
			}
			else
			if( m_Fade is UIImage )
			{
				UIImage fade = m_Fade as UIImage ;

				// 最初からタッチしていた場合は一度タッチを解除する必要がありそのためのフラグ
				m_IsPressing = Input.GetMouseButton( 0 );

				fade.IsInteraction = true ;
				fade.RaycastTarget = true ;
				fade.SetOnSimplePress( ( bool state ) =>
				{
					if( m_OutsideEnabled == true )
					{
						if( state == true  )
						{
							if( m_IsPressing == false )
							{
								// 外側を触れたら閉じる扱いとする

								// 外側がタッチされた際のコールバックを呼ぶ
								m_OnOutsidePress?.Invoke() ;

								m_IsPressing = true ;
							}
						}
						else
						{
							m_IsPressing = false ;
						}
					}
				} ) ;
			}

			//----------------------------------

			finishAction?.Invoke() ;
		}

		// GamePad 系の入力処理を行う
		internal void Update()
		{
			// 毎フレーム更新処理を呼び出す
			OnUpdate( Time.unscaledDeltaTime ) ;
		}

		virtual protected void OnUpdate( float deltaTime ){}

		// 毎フレームの最後に呼ばれる
		internal void LateUpdate()
		{
			OnLateUpdate( Time.unscaledDeltaTime ) ;
		}

		virtual protected void OnLateUpdate( float deltaTime ){}
		
		// 閉じる前に確認が必要な際に閉じる事が問題無い場合に呼んでもらう
//		private	void OnCloseConfirm()
//		{
//			// 閉じる
//			_ = CloseBase() ;
//		}

		/// <summary>
		/// ダイアログを閉じる
		/// </summary>
		/// <returns>The base.</returns>
		/// <param name="finishAction">T finish action.</param>
		/// <param name="waitingAction">T waiting action.</param>
		protected async UniTask CloseBase( Action finishAction = null, Func<UniTask> waitingAction = null )
		{
			// 外側タッチによるダイアログを閉じる処理を無効化する(これをしないとフリーズ要因となる)
			m_OnOutsidePress = null ;

			if( m_Fade != null )
			{
				_ = m_Fade.PlayTween( "FadeOut" ) ;
			}
			if( m_Window != null )
			{
				if( m_WindowFadeType == WindowFadeTypes.Tween )
				{
					_ = m_Window.PlayTween( "FadeOut" ) ;
				}
				else
				if( m_WindowFadeType == WindowFadeTypes.Animator )
				{
					_ = m_Window.PlayAnimator( "Out" ) ;
				}
			}

			if( m_Fade != null || m_Window != null )
			{
				while
				(
					( m_Fade != null && m_Fade.IsAnyTweenPlaying == true ) ||
					( m_Window != null && ( m_Window.IsAnyTweenPlaying == true || m_Window.IsPlayingAnimation() == true ) )
				)
				{
					await Yield() ;
				}
			}

			if( waitingAction != null )
			{
				await WhenAll( waitingAction() ) ;
			}

			finishAction?.Invoke() ;

			if( IsResident == false )
			{
				// 常駐しない
				if( gameObject.scene.name == m_DialogSceneName )
				{
					// シーンを削除
					await Yield() ;		// １フレームだけ待つ(IsClosedで待っているところを抜けさせる)
					Scene.Remove( m_DialogSceneName ) ;
				}
				else
				{
					Debug.LogWarning( "Can not remove dialog scene : " + m_DialogSceneName + " ( " + gameObject.scene.name + " ) " ) ;
				}
			}
			else
			{
				// 常駐する
				gameObject.SetActive( false ) ;
			}
		}
	}
}

