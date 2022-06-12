using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using uGUIHelper ;

namespace Template
{
	/// <summary>
	/// 画面外の外枠クラス Version 2022/04/05 0
	/// </summary>
	public class OuterFrame : ExMonoBehaviour
	{
		// シングルトンインスタンス
		private static OuterFrame m_Instance ;

		/// <summary>
		/// インスタンス
		/// </summary>
		public  static OuterFrame   Instance
		{
			get
			{
				return m_Instance ;
			}
		}

		//-------------------------------------

		[SerializeField]
		protected UICanvas	m_Canvas ;

		/// <summary>
		/// ＶＲ対応用にキャンバスを取得できるようにする
		/// </summary>
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
			m_Instance.m_Screen.SetSize( width, height ) ;

			return true ;
		}

		//-------------------------------------

		// セーフエリアの処理を有効にするか
		[SerializeField]
		protected bool		m_SafeAreaEnabled = true ;

		//---------------------------------------------------------------------------

		[SerializeField]
		protected UIView	m_Screen ;

		[SerializeField]
		protected UIImage	m_Frame_U ;

		[SerializeField]
		protected UIImage	m_Frame_D ;

		[SerializeField]
		protected UIImage	m_Frame_L ;

		[SerializeField]
		protected UIImage	m_Frame_R ;

		//-----------------------------------------------------------

		public UICanvas TargetCanvas
		{
			get
			{
				return m_Canvas ;
			}
		}
		
		//---------------------------------------------------------------------------


		private float	m_BasicWidth ;
		private float	m_BasicHeight ;

		private float	m_LimitWidth ;
		private float	m_LimitHeight ;

		//-----------------------------------
		
		private int		m_ScreenWidth ;
		private int		m_ScreenHeight ;

		private float	m_CanvasWidth ;
		private float	m_CanvasHeight ;

		//---------------------------------------------------------------------------

		internal void Awake()
		{
			m_Instance = this ;

			// そのシーンをシーンが切り替わっても永続的に残すようにする場合、そのシーン側で DontDestroyOnLoad を実行してはならない。
			// DontDestroyOnLoad は、呼び出し側のシーンで、そのシーンに対して実行すること。

			m_ScreenWidth  = 0 ;
			m_ScreenHeight = 0 ;

			//----------------------------------------------------------

			// キャンバスの解像度を設定する
			m_BasicWidth  = 1080 ;
			m_BasicHeight = 1920 ;

			m_LimitWidth  = 1440 ;
			m_LimitHeight = 2560 ;

			Settings settings =	ApplicationManager.LoadSettings() ;
			if( settings != null )
			{
				m_BasicWidth  = settings.BasicWidth ;
				m_BasicHeight = settings.BasicHeight ;

				m_LimitWidth  = settings.LimitWidth ;
				m_LimitHeight = settings.LimitHeight ;
			}

			SetCanvasResolution( m_BasicWidth, m_BasicHeight ) ;
		}

		internal void Start()
		{
			float canvasWidth  = m_Canvas.Size.x ;
			float canvasHeight = m_Canvas.Size.y ;

			Refresh() ;

			//---------------------------------
			// 現在の値を保存する

			m_ScreenWidth  = Screen.width ;
			m_ScreenHeight = Screen.height ;

			m_CanvasWidth  = canvasWidth ;
			m_CanvasHeight = canvasHeight ;
		}

		internal void Update()
		{
			float canvasWidth  = m_Canvas.Size.x ;
			float canvasHeight = m_Canvas.Size.y ;

			if( m_ScreenWidth != Screen.width || m_ScreenHeight != Screen.height || canvasWidth != m_CanvasWidth || canvasHeight != m_CanvasHeight )
			{
				Refresh() ;

				//---------------------------------
				// 現在の値を保存する

				m_ScreenWidth  = Screen.width ;
				m_ScreenHeight = Screen.height ;

				m_CanvasWidth  = canvasWidth ;
				m_CanvasHeight = canvasHeight ;
			}
		}

		// 表示更新する
		private void Refresh()
		{
			float canvasWidth  = m_Canvas.Size.x ;
			float canvasHeight = m_Canvas.Size.y ;

			float y ;
			float width ;
			float height ;

			if( ( canvasHeight / canvasWidth ) >  ( m_BasicHeight / m_BasicWidth ) )
			{
				// 縦長
				float h = canvasHeight ;

				if( h >  m_LimitHeight )
				{
					h  = m_LimitHeight ;
				}

				y		= 0 ;
				width	= m_BasicWidth ;
				height	= h ;

				//--------------------------------
				// 外枠は上下を表示する

				m_Screen.SetActive( true ) ;

				m_Frame_U.SetActive( true ) ;
				m_Frame_D.SetActive( true ) ;

				m_Frame_L.SetActive( false ) ;
				m_Frame_R.SetActive( false ) ;
			}
			else
			if( ( canvasHeight / canvasWidth ) <  ( m_BasicHeight / m_BasicWidth ) )
			{
				// 横長
				float w = canvasWidth ;

				if( w >  m_LimitWidth )
				{
					w  = m_LimitWidth ;
				}

				y		= 0 ;
				width	= w ;
				height	= m_BasicHeight ;

				//--------------------------------
				// 外枠は左右を表示する

				m_Screen.SetActive( true ) ;

				m_Frame_U.SetActive( false ) ;
				m_Frame_D.SetActive( false ) ;

				m_Frame_L.SetActive( true ) ;
				m_Frame_R.SetActive( true ) ;
			}
			else
			{
				// 丁度
				y		= 0 ;
				width	= m_BasicWidth ;
				height	= m_BasicHeight ;

				//--------------------------------
				// 外枠は表示しない

				m_Screen.SetActive( false ) ;

				m_Frame_U.SetActive( false ) ;
				m_Frame_D.SetActive( false ) ;

				m_Frame_L.SetActive( false ) ;
				m_Frame_R.SetActive( false ) ;
			}

			//----------------------------------------------------------

			if( m_SafeAreaEnabled == true )
			{
				// セーフエリアの外にはみ出た部分を削る

				var safeArea = Screen.safeArea ;

				float yMin = canvasHeight * ( float )safeArea.yMin / ( float )Screen.height ;
				float yMax = canvasHeight * ( float )safeArea.yMax / ( float )Screen.height ;

				// 画面上部のマージン幅
				float marginUpper = yMin ;
//				marginUpper = 128 ;	// デバッグ

				// 画面下部のマージン幅
				float marginLower = canvasHeight - yMax ;
//				marginLower = 128 ;	// デバッグ

				// 現在のマージン幅
				float margin = ( canvasHeight - height ) * 0.5f ;

				if( marginUpper <= margin )
				{
					// 画面上部のマージンは現在のままで良い
					marginUpper  = margin ; 
				}

				if( marginLower <= margin )
				{
					// 画面下部のマージンは現在のままで良い
					marginLower  = margin ;
				}

				// 画面上部と画面下部でマージン量が異なる場合に縦位置の補正をかける
				// 画面上部の方が太ければ下へ・画面下部の方が太ければ上へ
				y = ( marginLower - marginUpper ) * 0.5f ;

				// 画面の縦幅をセーフエリアを反映したものに変更
				height = canvasHeight - ( marginUpper + marginLower ) ;

				//---------------------------------

				if( marginUpper >  0 )
				{
					m_Screen.SetActive( true ) ;
					m_Frame_U.SetActive( true ) ;
				}

				if( marginLower >  0 )
				{
					m_Screen.SetActive( true ) ;
					m_Frame_D.SetActive( true ) ;
				}
			}

			//----------------------------------------------------------

			m_Screen.SetPositionY( y ) ;
			m_Screen.SetSize( width, height ) ;
		}
	}
}
