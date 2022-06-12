using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;
using Cysharp.Threading.Tasks ;

namespace Template.Screens
{
	/// <summary>
	/// スクリーン制御の基底クラス Version 2022/05/04
	/// </summary>
	public class ScreenBase : ExMonoBehaviour
	{
		protected bool m_IsInitialized ;

		internal void Awake()
		{
			// ApplicationManager を起動する(最初からヒエラルキーにインスタンスを生成しておいても良い)
			ApplicationManager.Create() ;

			OnAwake() ;
		}

		virtual protected void OnAwake(){}

		// Start は async void が正しい
		internal async void Start()
		{
			PrepareBase().Forget() ;

			await Yield() ;
		}

		// 準備
		private async UniTask PrepareBase()
		{
			// ApplicationManager の準備が整うのを待つ
			if( ApplicationManager.IsInitialized == false )
			{
				await WaitUntil( () => ApplicationManager.IsInitialized ) ;
			}

			// 準備が整った
			m_IsInitialized = true ;

			//----------------------------------------------------------
			
			// 現在アクティブになっているシーン名
			string activeSceneName = Scene.GetActiveName() ;

			if( activeSceneName == Scene.Screen.Boot )
			{
				// 何もしない
			}
			else
			if( activeSceneName == Scene.Screen.Downloading )
			{
				// そのままダウンロード

				// Phase 1 まで実行する
				if( Scene.ContainsParameter( "DownloadingRequestType" ) == false )
				{
					// Download から実行された
					if( ApplicationManager.DownloadingPhase1State == ApplicationManager.DownloadingPhaseStates.None )
					{
						Scene.SetParameter( "DownloadingRequestType", ApplicationManager.DownloadingRequestTypes.Phase1 ) ;
					}
				}
			}
			else
			if( activeSceneName == Scene.Screen.Title )
			{
				// タイトルに遷移してきた
				if( ApplicationManager.DownloadingPhase1State == ApplicationManager.DownloadingPhaseStates.None )
				{
					// Phase 1 が実行されていないため Phase 1 だけ実行する
					Scene.SetParameter( "DownloadingRequestType", ApplicationManager.DownloadingRequestTypes.Phase1 ) ;

					// タイトルに戻す形でダウンローディングに飛ばす
					Scene.LoadWithFade( Scene.Screen.Downloading, "StartScreenName", Scene.Screen.Title ).Forget() ;
					return ;
				}
			}
			else
			{
				// その他
				if
				(
					ApplicationManager.DownloadingPhase1State == ApplicationManager.DownloadingPhaseStates.None ||
					ApplicationManager.DownloadingPhase2State == ApplicationManager.DownloadingPhaseStates.None
				)
				{
					// ダウンロードの Phase 1 か Phase 2 またはその両方が完了していない

					// Phase 2 まで実行する
					Scene.SetParameter( "DownloadingRequestType", ApplicationManager.DownloadingRequestTypes.Phase2 ) ;

					// ブートシーンでなければダウンローディングシーンに飛ばす
					Scene.LoadWithFade( Scene.Screen.Downloading, "StartScreenName", activeSceneName ).Forget() ;
					return ;
				}
			}

			//----------------------------------------------------------

			OnStart().Forget() ;
			// MonoBehaviour.Start() は完全に終了させる必要がある(特例)
		}

		virtual protected async UniTask OnStart(){ await Yield() ; }

		/// <summary>
		/// Update の基底メソッド
		/// </summary>
		internal void Update()
		{
			if( m_IsInitialized == false )
			{
				return ;	// 準備が整うまで繰り返し呼び出すメソッドは呼ばないようにする
			}

			OnUpdate( Time.deltaTime ) ;
		}

		virtual protected void OnUpdate( float deltaTime ){}

		/// <summary>
		/// LateUpdate の基底メソッド
		/// </summary>
		internal void LateUpdate()
		{
			if( m_IsInitialized == false )
			{
				return ;	// 準備が整うまで繰り返し呼び出すメソッドは呼ばないようにする
			}

			OnLateUpdate( Time.deltaTime ) ;
		}

		virtual protected void OnLateUpdate( float deltaTime ){}

		/// <summary>
		/// FixedUpdate の基底メソッド
		/// </summary>
		internal void FixedUpdate()
		{
			if( m_IsInitialized == false )
			{
				return ;	// 準備が整うまで繰り返し呼び出すメソッドは呼ばないようにする
			}

			OnFixedUpdate( Time.fixedDeltaTime ) ;
		}

		virtual protected void OnFixedUpdate( float fixedDeltaTime ){}
	}
}
