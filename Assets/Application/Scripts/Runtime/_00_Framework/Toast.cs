using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Threading ;
using System.Threading.Tasks ;

using Cysharp.Threading.Tasks ;

using UnityEngine ;

using uGUIHelper ;

namespace Template
{
	/// <summary>
	/// トーストクラス(画面全体のフェード演出に使用する) Version 2022/05/04 0
	/// </summary>
	public class Toast : ExMonoBehaviour
	{
		// シングルトンインスタンス
		private static Toast m_Instance ; 

		/// <summary>
		/// ナビゲータクラスのインスタンス
		/// </summary>
		public  static Toast   Instance
		{
			get
			{
				return m_Instance ;
			}
		}

		/// <summary>
		/// 表示状態
		/// </summary>
		public	static bool			IsVisible
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}

				return m_Instance.gameObject.activeSelf ;
			}
		}

		//-------------------------------------

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

		// スクリーン
		[SerializeField]
		protected UIImage		m_Screen ;

		// ウィンドウ
		[SerializeField]
		protected UIImage		m_Window ;
		
		// メッセージ
		[SerializeField]
		protected UITextMesh	m_Message ;


		[Header( "最低横幅" )]

		[SerializeField]
		protected float			m_MinWidth = 480 ;

		//-------------------------------------------------------------------------------------------

		private float m_DisplayKeepTime ;
		private float m_DisplayTickTime ;

		//-------------------------------------------------------------------------------------------
		
		// 重要
		// FadeのシーンファイルのCanvasのカメラは、
		// 必ずDepthBufferをクリアするようにすること
		// 戦闘→迷宮の画面切り替えの際に
		// 迷宮画面は維持・非アクティブにしているが
		// 戦闘画面が加算で追加されるまでの間、
		// ヒエラルキーには画面を描画するカメラが存在しなくなり、
		// DepthBufferにゴミが残る状態になる。
		// それによりFadeがDepthBufferをクリアしていないと、
		// Fadeのレンダリングがおかしくなる。

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

			//----------------------------------------------------------
			// 初期状態で見えてほしくないものを非表示にする

			m_Screen.Enabled = false ;

			m_Window.SetActive( false ) ;
		}
		
		//-----------------------------------

		/// <summary>
		/// 表示する
		/// </summary>
		/// <param name="message"></param>
		/// <param name="displayKeepTime"></param>
		public static void Show( string message, float displayKeepTime = 3 )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.Show_Private( message, displayKeepTime ) ;
		}

		// 表示する
		private void Show_Private( string message, float displayKeepTime )
		{
			if( gameObject.activeSelf == true )
			{
				return ;	// 表示済み
			}

			//----------------------------------

			gameObject.SetActive( true ) ;

			if( displayKeepTime <  1 )
			{
				displayKeepTime  = 1 ;
			}

			m_Message.Text = message ;
			m_DisplayKeepTime = displayKeepTime ;
			m_DisplayTickTime = 0 ;

			float textWidth = m_Message.TextWidth ;
			float width = textWidth + 64 ;
			if( width <  m_MinWidth )
			{
				width  = m_MinWidth ;
			}
			m_Window.Width = width ;

			m_Window.StopAllTweens() ;
			m_Window.PlayTween( "FadeIn" ) ;
		}

		/// <summary>
		/// 隠蔽する(強制)
		/// </summary>
		public static void Hide()
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.Hide_Private() ;
		}

		// 隠蔽する
		private void Hide_Private()
		{
			if( gameObject.activeSelf == false )
			{
				return ;
			}

			//----------------------------------

			m_Window.StopAndResetAllTweens() ;
			m_Window.SetActive( false ) ;

			gameObject.SetActive( false ) ;
		}

		/// <summary>
		/// 時間監視
		/// </summary>
		internal void Update()
		{
			// コンポーネントの実行順が不確定であるため GameObject のアクティブ/非アクティブでのみの判定は危険(PlayTweenAndHideで非アクティブにしたつもりでも、もう1フレームUpdateが呼ばれて再度FadeOutが実行されてしまう可能性がある)
			if( m_Window.ActiveInHierarchy == true )
			{
				if( m_Window.IsAnyTweenPlaying == false )
				{
					m_DisplayTickTime += Time.unscaledDeltaTime ;
					if( m_DisplayKeepTime >  0 && m_DisplayTickTime >  m_DisplayKeepTime )
					{
						m_DisplayKeepTime = 0 ;
						m_Window.PlayTweenAndHide( "FadeOut", onFinishedAction:( string identity, UITween tween ) =>
						{
							gameObject.SetActive( false ) ;
						} ) ;
					}
				}
			}
		}



	}
}
