using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;
using UnityEngine.EventSystems ;
using UnityEngine.XR ;

namespace uGUIHelper
{
	/// <summary>
	/// uGUI:EventSystem の機能拡張コンポーネントクラス
	/// </summary>
	[ ExecuteInEditMode ]	
	public class UIEventSystem : MonoBehaviour
	{
		// シングルトンインスタンス
		private static UIEventSystem m_Instance ;

#if false

		// ＶＲ用の反応ボタン押下判定用のコールバック
		private static Func<GameObject,bool>	m_OnPressAction = null ;


		//-----------------------------------------------------------
		/// <summary>
		/// ＶＲ用の反応ボタン押下判定用のコールバックを設定する
		/// </summary>
		/// <param name="onPressAction"></param>
		public static void SetOnPressAction( Func<GameObject,bool> onPressAction )
		{
			m_OnPressAction = onPressAction ;

			if( m_Instance != null && m_Instance.m_StandaloneInputModule != null )
			{
				m_Instance.m_StandaloneInputModule.SetOnPressAction( onPressAction ) ;
			}
		}

		/// <summary>
		/// 注目しているオブジェクトを返す
		/// </summary>
		/// <returns></returns>
		public static GameObject GetLookingObject()
		{
			if( m_Instance != null && m_Instance.m_StandaloneInputModule != null )
			{
				return m_Instance.m_StandaloneInputModule.GetLookingObject() ;
			}
			return null ;
		}
#endif
		//---------------------------------------------------------
		
		// インプットモジュールのインスタンス
//		private StandaloneInputModuleWrapper	m_StandaloneInputModule = null ;
		private DxStandaloneInputModule			m_StandaloneInputModule = null ;

		/// <summary>
		/// インプットモジュールの動作タイプ
		/// </summary>
		public static StandaloneInputModuleWrapper.ProcessType	ProcessType = StandaloneInputModuleWrapper.ProcessType.Default ;

		//---------------------------------------------------------

		/// <summary>
		/// インスタンス生成（スクリプトから生成する場合）
		/// </summary>
		/// <returns></returns>
		public static UIEventSystem Create()
		{
			if( m_Instance != null )
			{
				return m_Instance ;
			}
		
#if UNITY_EDITOR
			if( GameObject.FindObjectOfType<EventSystem>() != null )
			{
				Debug.LogWarning( "既にシーン内に EventSystem が存在しています。UIEventSystm への差し替えを推奨します。" ) ;
			}
#endif

			// オブジェクトが非アクティブだと検出されないのでオブジェクトを非アクティブにしてはならない
			// この判定は必須で mInstance は static であるためシーンの最初はオブジェクトが存在しても null になっている
			m_Instance = GameObject.FindObjectOfType( typeof( UIEventSystem ) ) as UIEventSystem ;
			if( m_Instance == null )
			{
				GameObject go = new GameObject( "EventSystem" ) ;
				go.AddComponent<UIEventSystem>() ;
			
				go.transform.localPosition	= Vector3.zero ;
				go.transform.localRotation	= Quaternion.identity ;
				go.transform.localScale		= Vector3.one ;
			}
		
			return m_Instance ;
		}
	
		// インスタンス取得
		public static UIEventSystem Get()
		{
			return m_Instance ;
		}
	
		// インスタンス破棄
		public static void Delete()
		{	
			if( m_Instance != null )
			{
				if( Application.isPlaying == false )
				{
					DestroyImmediate( m_Instance.gameObject ) ;
				}
				else
				{
					Destroy( m_Instance.gameObject ) ;
				}
			}
		}
	
		//-----------------------------------------------------------------
	
		internal void Awake()
		{
			// 既に存在し重複になる場合は自身を削除する（シーンを加えようとした場合）
			if( m_Instance != null )
			{
				Debug.LogWarning( "Destroy Myself !!:" + name ) ;
				GameObject.DestroyImmediate( gameObject ) ;
				return ;
			}
		
			//-----------------------------
			// Awake 内でマニュアルで実行した場合とスクリプトで実行した場合の共通の処理を行う必要がある
		
			m_Instance = this ;
		
			//-----------------------------
		
			// 原点じゃないと気持ち悪い
			gameObject.transform.localPosition	= Vector3.zero ;
			gameObject.transform.localRotation	= Quaternion.identity ;
			gameObject.transform.localScale		= Vector3.one ;

			//-----------------------------

			EventSystem eventSystem = GetComponent<EventSystem>() ;
			if( eventSystem == null )
			{
				eventSystem = gameObject.AddComponent<EventSystem>() ;
			}

			// カスタマイズ版のインプットモジュールを使用する
/*			if( GetComponent<StandaloneInputModuleWrapper>() == null )
			{
				m_StandaloneInputModule = gameObject.AddComponent<StandaloneInputModuleWrapper>() ;
			}*/

			if( GetComponent<DxStandaloneInputModule>() == null )
			{
				m_StandaloneInputModule = gameObject.AddComponent<DxStandaloneInputModule>() ;
			}
		}

		internal void Update()
		{
			// バックキーの処理
			if( Input.GetKeyDown( KeyCode.Escape ) == true )
			{
				ProcessBackKey() ;
			}
		}

		internal void OnDestroy()
		{
			if( m_Instance == this )
			{
				m_Instance  = null ;
			}
		}

		/// <summary>
		/// イベントシステムを有効化する
		/// </summary>
		/// <returns></returns>
		public static bool Enable()
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.SetState( true ) ;
		}

		/// <summary>
		/// イベントシステムを無効化する
		/// </summary>
		/// <returns></returns>
		public static bool Disable()
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.SetState( false ) ;
		}

		/// <summary>
		/// イベントシステムの状態を取得する
		/// </summary>
		/// <returns></returns>
		public static bool State
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}
	
				EventSystem eventSystem = m_Instance.gameObject.GetComponent<EventSystem>() ;
				if( eventSystem == null )
				{
					return false ;
				}
	
				return eventSystem.enabled ;
			}
		}

		private bool SetState( bool state )
		{
			EventSystem eventSystem = gameObject.GetComponent<EventSystem>() ;
			if( eventSystem == null )
			{
				return false ;
			}
			
			eventSystem.enabled = state ;

			if( m_StandaloneInputModule != null )
			{
				m_StandaloneInputModule.UpdateModule() ;
			}

			return eventSystem.enabled ;
		}

		/// <summary>
		/// 指定のゲームオブジェクト(UI)がホバー状態か判定する
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public static bool IsHovering( GameObject target )
		{
			if( m_Instance == null || m_Instance.m_StandaloneInputModule == null )
			{
				return false ;
			}

			return m_Instance.m_StandaloneInputModule.IsHovering( target ) ;
		}

		/// <summary>
		/// 指定のゲームオブジェクト(UI)がプレス状態か判定する
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public static bool IsPressing( GameObject target )
		{
			if( m_Instance == null || m_Instance.m_StandaloneInputModule == null )
			{
				return false ;
			}

			return m_Instance.m_StandaloneInputModule.IsPressing( target ) ;
		}

		//---------------------------------------------------------------------------

		// バックキーの監視対象
		private readonly List<UIView>	m_BackKeyTargets = new List<UIView>() ;

		/// <summary>
		/// バックキーの監視対象を登録する
		/// </summary>
		/// <param name="view"></param>
		/// <returns></returns>
		public static bool AddBackKeyTarget( UIView view )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_Instance.m_BackKeyTargets.Add( view ) ;

			return true ;
		}

		/// <summary>
		/// バックキーの監視対象を削除する
		/// </summary>
		/// <param name="view"></param>
		/// <returns></returns>
		public static bool RemoveBackKeyTarget( UIView view )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			if( m_Instance.m_BackKeyTargets.Contains( view ) == false )
			{
				return false ;
			}

			m_Instance.m_BackKeyTargets.Remove( view ) ;

			return true ;
		}

		//-----------------------------------------------------------

		private float m_BK_Timer = 0 ;

		// バックキーを処理する
		private void ProcessBackKey()
		{
			// 前回のバックキー押下から 0.5 秒以上の経過が必要になる
			if( m_BK_Timer == 0 )
			{
				m_BK_Timer = Time.realtimeSinceStartup ;
			}
			else
			{
				if( ( Time.realtimeSinceStartup - m_BK_Timer ) <  0.5f )
				{
					return ;
				}
				m_BK_Timer = Time.realtimeSinceStartup ;
			}

			//----------------------------------------------------------
			// バックキーが押された

			if( m_BackKeyTargets.Count == 0 )
			{
				// 監視対象が存在しない
				return ;
			}

			if( CorrectBackKey() == false )
			{
				// 整理した結果監視対象が無くなった
				return ;
			}

			// 有効なボタンのうちレイキャスト的に押せるものを押す
			UIView firstHitView = null ;
			foreach( var view in m_BackKeyTargets )
			{
				if( view.ActiveInHierarchy == true && view.Alpha >  0 && view.IsAnyTweenPlayingInParents == false && CheckBackBeyTarget( view ) == true )
				{
					// ヒットした
					if( view is UIButton )
					{
						UIButton button = view as UIButton ;
						if( button.Interactable == true )
						{
							if( firstHitView == null )
							{
								firstHitView = view ;
							}
							else
							{
								Debug.LogWarning( "[Duplicate back key target] (NG)" + view.Path + " (OK)" + firstHitView.Path ) ;
							}
						}
					}
					else
					if( view is UIImage )
					{
						UIImage image = view as UIImage ;
						if( view.IsInteraction == true )
						{
							if( firstHitView == null )
							{
								firstHitView = view ;
							}
							else
							{
								Debug.LogWarning( "[Duplicate back key target] (NG)" + view.Path + " (OK)" + firstHitView.Path ) ;
							}
						}
					}
				}
			}

			// 有効な対象が存在する
			if( firstHitView != null )
			{
				if( firstHitView is UIButton )
				{
					UIButton button = firstHitView as UIButton ;
					button.ExecuteButtonClick() ;
				}
				else
				if( firstHitView is UIImage )
				{
					UIImage image = firstHitView as UIImage ;
					image.ExecuteClick() ;
				}
			}
		}

		// バックキーの不要な登録を削除する
		private bool CorrectBackKey()
		{
			UIView view ;
			int i, l ;
			while( m_BackKeyTargets.Count >  0 )
			{
				l =  m_BackKeyTargets.Count ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					view = m_BackKeyTargets[ i ] ;
					if( view == null )
					{
						m_BackKeyTargets.RemoveAt( i ) ;
						break ;
					}
				}
				if( m_BackKeyTargets.Count == l )
				{
					// 削除対象が無かった
					break ;
				}
			}

			return ( m_BackKeyTargets.Count >  0 ) ;
		}

		// バックキーが押せるか確認する
		private readonly Vector3[]				m_BK_Positions = new Vector3[ 5 ] ;
		private readonly PointerEventData		m_BK_EventDataCurrentPosition = new PointerEventData( EventSystem.current ) ;
		private readonly List<RaycastResult>	m_BK_Results = new List<RaycastResult>() ;

		private bool CheckBackBeyTarget( UIView view )
		{
			// 親キャンバスを取得
			Transform canvasTransform = view.transform ;
			while( canvasTransform.GetComponent<Canvas>() == null )
			{
				canvasTransform = canvasTransform.parent ;
				if( canvasTransform == null )
				{
					return false ;
				}
			}

			//----------------------------------------------------------

			// 親キャンバスからスクリーン座標を求める
			RectTransform r = view.gameObject.GetComponent<RectTransform>() ;
			if( r == null )
			{
				return false ;
			}

			// キャンバス上の座標を取得する
			Vector3 pivotPosition = view.PositionInCanvas ;
			float w = r.sizeDelta.x ;
			float h = r.sizeDelta.y ;
			Vector2 pivot = r.pivot ;

			Vector3 centerPosition = pivotPosition + new Vector3( ( 0.5f - pivot.x ) * w, ( 0.5f - pivot.y ) * h, 0 ) ;

			// 少し内側でないと範囲外をチェックしてしまう
			w *= 0.4f ;
			h *= 0.4f ;

			// 四隅と中心の計５点のいずれかがレイキャストヒットすれば有効と判定する
			m_BK_Positions[ 0 ] = centerPosition ;
			m_BK_Positions[ 1 ] = centerPosition + new Vector3( - w, - h, 0 ) ;
			m_BK_Positions[ 2 ] = centerPosition + new Vector3( + w, - h, 0 ) ;
			m_BK_Positions[ 3 ] = centerPosition + new Vector3( - w, + h, 0 ) ;
			m_BK_Positions[ 4 ] = centerPosition + new Vector3( + w, + h, 0 ) ;

			Canvas canvas = canvasTransform.GetComponent<Canvas>() ;

			RectTransform canvasArea = canvas.GetComponent<RectTransform>() ;
			float cw = canvasArea.sizeDelta.x ;
			float ch = canvasArea.sizeDelta.y ;
			Vector2 canvasPivot = canvasArea.pivot ;
			float sw = Screen.width ;
			float sh = Screen.height ;

			Vector2 screenPosition ;
			foreach( var currentPosition in m_BK_Positions )
			{
				screenPosition.x = ( sw * currentPosition.x / cw ) + ( sw * canvasPivot.x ) ;
				screenPosition.y = ( sh * currentPosition.y / ch ) + ( sh * canvasPivot.y ) ;

				// スクリーン座標からRayを飛ばす
				if( screenPosition.x >= 0 && screenPosition.x <= sw && screenPosition.y >= 0 && screenPosition.y <= sh )
				{
					m_BK_EventDataCurrentPosition.position = screenPosition ;
					m_BK_Results.Clear() ;
					EventSystem.current.RaycastAll( m_BK_EventDataCurrentPosition, m_BK_Results ) ;

					// 自分自身でない場合は他のUIの下なのでタップできない
					if( m_BK_Results.Count >= 1 && m_BK_Results[ 0 ].gameObject == view.gameObject )
					{
						// ヒットした
						return true ;
					}
				}
			}

			// ヒットしなかった
			return false ;
		}
	}
}

