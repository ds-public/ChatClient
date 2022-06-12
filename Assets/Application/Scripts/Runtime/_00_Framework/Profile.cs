using UnityEngine ;
using System.Collections ;

// 要 uGUIHelper パッケージ
using uGUIHelper ;

namespace Template
{
	/// <summary>
	/// プロファイルクラス(実行中の様々な情報をオーバーレイで画面に表示する) Version 2021/11/08 0
	/// </summary>
	public class Profile : ExMonoBehaviour
	{
		// シングルトンインスタンス
		private static Profile m_Instance ; 

		/// <summary>
		/// クラスのインスタンス
		/// </summary>
		public  static Profile   Instance
		{
			get
			{
				return m_Instance ;
			}
		}

		//-----------------------------------------------------------

		// キャンバス部分のインスタンス
		[SerializeField]
		protected UICanvas m_Canvas ;

		public UICanvas Canvas
		{
			get
			{
				return m_Canvas ;
			}
		}

		/// <summary>
		/// キャンバスの仮想解像度を設定する
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns></returns>
		public static bool SetCanvasResolution( float width, float height )
		{
			if( m_Instance == null || m_Instance.m_Canvas == null )
			{
				return false ;
			}

			m_Instance.m_Canvas.SetResolution( width, height, true ) ;

			return true ;
		}

		//-----------------------------------------------------------

		[SerializeField]
		protected UIImage		m_Screen ;

		[SerializeField]
		protected UIView		m_FPSBase ;

		[SerializeField]
		protected UITextMesh	m_FPS ;

		//-----------------------------------------------------------

		private int m_FPS_R_Count ;
		private int m_FPS_E_Count ;

		private float m_FPS_DeltaTime ;

		public enum FpsAlignment
		{
			LeftTop,
			CenterTop,
			RightTop,

			LeftBottom,
			CenterBottom,
			RightBottom,
		}

		[SerializeField]
		protected FpsAlignment	m_FpsAlignment = FpsAlignment.CenterBottom ;

		//-------------------------------------------------------------------------------------------
		
		internal void Awake()
		{
			m_Instance = this ;

			// そのシーンをシーンが切り替わっても永続的に残すようにする場合、そのシーン側で DontDestroyOnLoad を実行してはならない。
			// DontDestroyOnLoad は、呼び出し側のシーンで、そのシーンに対して実行すること。

			//----------------------------------------------------------

			// キャンバスの解像度を設定する
			float width  = 1080 ;
			float height = 1920 ;

			Settings settings =	ApplicationManager.LoadSettings() ;
			if( settings != null )
			{
				width  = settings.BasicWidth ;
				height = settings.BasicHeight ;
			}

			SetCanvasResolution( width, height ) ;

			//--------------------------------------------------------------------------

			m_Screen.Enabled = false ;

			// ＦＰＳの表示を設定する(デフォルトの表示状態は開発モードに依存する)
			m_FPSBase.SetActive( Define.DevelopmentMode ) ;

			SetFpsAlignment( m_FpsAlignment ) ;
		}

		internal void Start()
		{
			if( m_FPSBase.ActiveSelf == true )
			{
				m_FPS_R_Count = 0 ;
				m_FPS_E_Count = 0 ;
				m_FPS_DeltaTime = 0 ;
				m_FPS.Text = "FPS Waiting..." ;
			}
		}

		//-----------------------------------

		/// <summary>
		/// ＦＰＳを表示する
		/// </summary>
		public static void ShowFPS()
		{
			// リリース版では無視する

			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.ShowFPS_Private() ;
		}

		private void ShowFPS_Private()
		{
			m_FPSBase.SetActive( true ) ;

			m_FPS_R_Count = 0 ;
			m_FPS_E_Count = 0 ;
			m_FPS_DeltaTime = 0 ;
			m_FPS.Text = "FPS Waiting..." ;
		}

		/// <summary>
		/// ＦＰＳを消去する
		/// </summary>
		public static void HideFPS()
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.HideFPS_Private() ;
		}

		private void HideFPS_Private()
		{
			m_FPSBase.SetActive( false ) ;
		}

		/// <summary>
		/// FPSの表示状態
		/// </summary>
		public static bool IsFPS
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}
				return m_Instance.m_FPSBase.ActiveSelf ;
			}
		}


		//---------------

		internal void  Update()
		{
#if UNITY_EDITOR
			if( Input.GetKeyDown( KeyCode.F3 ) == true )
			{
				m_FPSBase.SetActive( !m_FPSBase.ActiveSelf ) ;
			}
#endif
			if( m_FPSBase.ActiveSelf == true )
			{
				m_FPS_R_Count ++ ;
				m_FPS_DeltaTime += Time.unscaledDeltaTime ;

				if( m_FPS_DeltaTime >= 1 )
				{
					m_FPS.Text = "FPS " + m_FPS_R_Count + " (" + ( int )( 1000 * m_FPS_DeltaTime / m_FPS_R_Count ) + "ms) FU = " + m_FPS_E_Count ;
					m_FPS_R_Count = 0 ;
					m_FPS_E_Count = 0 ;
					m_FPS_DeltaTime = 0 ;
				}
			}
		}

		internal void FixedUpdate()
		{
			if( m_FPSBase.ActiveSelf == true )
			{
				m_FPS_E_Count ++ ;
			}
		}

		//-------------------------------------------------------------------------------------------

		private void SetFpsAlignment( FpsAlignment fpsAlignment )
		{
			if( fpsAlignment == FpsAlignment.LeftTop )
			{
				m_FPSBase.SetPivot( 0, 1 ) ;
				m_FPSBase.SetAnchor( 0, 1 ) ;
			}
			else
			if( fpsAlignment == FpsAlignment.CenterTop )
			{
				m_FPSBase.SetPivot( 0.5f, 1 ) ;
				m_FPSBase.SetAnchor( 0.5f, 1 ) ;
			}
			else
			if( fpsAlignment == FpsAlignment.RightTop )
			{
				m_FPSBase.SetPivot( 1, 1 ) ;
				m_FPSBase.SetAnchor( 1, 1 ) ;
			}
			else
			if( fpsAlignment == FpsAlignment.LeftBottom )
			{
				m_FPSBase.SetPivot( 0, 0 ) ;
				m_FPSBase.SetAnchor( 0, 0 ) ;
			}
			else
			if( fpsAlignment == FpsAlignment.CenterBottom )
			{
				m_FPSBase.SetPivot( 0.5f, 0 ) ;
				m_FPSBase.SetAnchor( 0.5f, 0 ) ;
			}
			else
			if( fpsAlignment == FpsAlignment.RightBottom )
			{
				m_FPSBase.SetPivot( 1, 0 ) ;
				m_FPSBase.SetAnchor( 1, 0 ) ;
			}
		}
	}
}
