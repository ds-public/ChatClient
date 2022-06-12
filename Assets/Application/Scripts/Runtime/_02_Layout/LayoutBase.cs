using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;
using Cysharp.Threading.Tasks ;

using uGUIHelper ;

namespace Template.Layouts
{
	/// <summary>
	/// レイアウト制御の基底クラス Version 2022/05/04
	/// </summary>
	public partial class LayoutBase : ExMonoBehaviour
	{
		[SerializeField]
		protected UIImage			m_Screen ;

		//-------------------------------------------------------------------------------------------

		protected bool				m_IsInitialized ;

		//-------------------------------------------------------------------------------------------

		private string				m_LayoutSceneName	= string.Empty ; 
		
		//-------------------------------------------------------------------------------------------

		virtual protected string SetLayoutSceneName(){ return string.Empty ; }
		
		internal void Awake()
		{
			//----------------------------------------------------------

			m_LayoutSceneName = SetLayoutSceneName() ;

			//----------------------------------------------------------

			// ApplicationManager を起動する(最初からヒエラルキーにインスタンスを生成しておいても良い)
			ApplicationManager.Create() ;

			if( m_Screen != null )
			{
				// 基本的に土台となっているスクリーンは透過
				Color color = m_Screen.Color ;
				color.a = 0.0f ;	// 透明にする
				m_Screen.Color = color ;
			}

			OnAwake() ;

			//----------------------------------------------------------

			// カメラは Awake() で設定しないと一瞬表示されてしまうので注意
			if( UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == m_LayoutSceneName )
			{
				if( m_Screen != null )
				{
					Color color = m_Screen.Color ;
					color.a = 1.0f ;	// 不透明にする
					m_Screen.Color = color ;
					m_Screen.Enabled = true ;
				}
			}
		}

		virtual protected void OnAwake(){}

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

			// 準備が整った
			m_IsInitialized = true ;

			//----------------------------------------------------------

			await OnStart() ;

			//----------------------------------------------------------

#if UNITY_EDITOR
			if( UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == m_LayoutSceneName )
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
