using System ;
using System.Collections ;
using System.Collections.Generic ;

using Cysharp.Threading.Tasks ;

using UnityEngine ;

// 要 uGUIHelper パッケージ
using uGUIHelper ;

namespace Template
{
	/// <summary>
	/// フェードクラス(画面全体のフェード演出に使用する) Version 2022/05/04 0
	/// </summary>
	public class Fade : ExMonoBehaviour
	{
		// シングルトンインスタンス
		private static Fade m_Instance ; 

		/// <summary>
		/// フェードクラスのインスタンス
		/// </summary>
		public  static Fade   Instance
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

		// フェード部分のインスタンス

		[SerializeField]
		protected UIView		m_Screen ;

		[SerializeField]
		protected UIImage		m_Rectangle ;

		//-----------------------------------------------------------

		/// <summary>
		/// デフォルトの色
		/// </summary>
		public Color DefaultColor = new Color( 0, 0, 0, 1 ) ;

		/// <summary>
		/// デフォルトの遅延時間(秒)　※0 未満でインスタペクターに設定された値を使用する
		/// </summary>
		public float DefaultDelay ;

		/// <summary>
		/// デフォルトの実行時間(秒)　※0 未満でインスタペクターに設定された値を使用する
		/// </summary>
		public float DefaultDuration = -1 ;

		//---------------------------------------------------------------------------

		private bool m_Showing ;

		/// <summary>
		/// 表示状態
		/// </summary>
		public static bool IsShowing
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}

				return m_Instance.m_Showing ;
			}
		}

		// 実行状態
		private bool m_Playing ;

		/// <summary>
		/// 実行状態
		/// </summary>
		public static bool IsPlaying
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}

				return m_Instance.m_Playing ;
			}
		}

		//-------------------------------------

		/// <summary>
		/// フェードのタイプ
		/// </summary>
		public enum FadeTypes
		{
			Unknown,
			Color,
		}

		private FadeTypes	m_FadeType = FadeTypes.Unknown ;

		//-----------------------------------------------------------

		public class AsyncState : CustomYieldInstruction
		{
			public override bool keepWaiting
			{
				get
				{
					if( IsDone == false )
					{
						return true ;    // 継続
					}
					return false ;   // 終了
				}
			}

			/// <summary>
			/// 通信が終了したかどうか
			/// </summary>
			public bool			IsDone ;
		}

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

			m_Rectangle.SetActive( false ) ;

			//----------------------------------------------------------

			// ブロッカーによりフェード表示中は全面的に入力を禁止する
			m_Screen.SetActive( true ) ;
			m_Screen.RaycastTarget = true ;
		}

		//-----------------------------------

		/// <summary>
		/// フェードアウトを実行する
		/// </summary>
		/// <param name="delay">遅延時間(秒)</param>
		/// <param name="duration">実行時間(秒)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static async UniTask Out( FadeTypes fadeType = FadeTypes.Color, float delay = 0, float duration = -1 )
		{
			if( m_Instance == null )
			{
				return ;
			}

			AsyncState state = new AsyncState() ;
			m_Instance.gameObject.SetActive( true ) ;
			m_Instance.StartCoroutine( m_Instance.Out_Private( fadeType, delay, duration, state ) ) ;

			await m_Instance.WaitUntil( () => state.IsDone ) ;
		}

		/// フェードアウトを実行する
		private IEnumerator Out_Private( FadeTypes fadeType, float delay, float duration, AsyncState state )
		{
			if( fadeType == FadeTypes.Unknown )
			{
				// 問題あり
				state.IsDone = true ;
				yield break ;
			}

			//----------------------------------

			if( m_Rectangle == null )
			{
				// 問題あり
				state.IsDone = true ;
				yield break ;
			}

			if( fadeType != FadeTypes.Unknown && fadeType != FadeTypes.Color )
			{
				// 問題あり
				state.IsDone = true ;
				yield break ;
			}

			if( m_Rectangle.IsAnyTweenPlaying == true )
			{
				// 既に何らかの Tween が実行されているので強制的に停止させる
				m_Rectangle.StopAllTweens() ;
			}

			//----------------------------------

			m_FadeType = fadeType ;

			m_Playing = true ;

			//----------------------------------

			if( fadeType == FadeTypes.Color )
			{
				// 単色

				m_Rectangle.SetActive( true ) ;	// 入力をブロックするために使用する
				m_Rectangle.Color = DefaultColor ;

				if( delay <  0 )
				{
					delay  = DefaultDelay ;
				}

				if( duration <  0 )
				{
					duration = DefaultDuration ;
				}

	//			Debug.Log( "<color=#FF7F00>---------------->FadeOut開始</color>" ) ;

				if( duration != 0 )
				{
					yield return m_Rectangle.PlayTween( "FadeOut", delay, duration ) ;
				}
				else
				{
					m_Rectangle.SetActive( true ) ;
				}
			}

			//----------------------------------

			m_Playing = false ;
			m_Showing = true ;

			state.IsDone = true ;
		}

		//---------------

		/// <summary>
		/// フェードインを実行する
		/// </summary>
		/// <param name="delay">遅延時間(秒)</param>
		/// <param name="duration">実行時間(秒)</param>
		/// <returns>列挙子</returns>
		public static async UniTask In( float delay = 0, float duration = -1 )
		{
			if( m_Instance == null )
			{
				return ;
			}

			if( IsPlaying == true )
			{
				return ;
			}

			AsyncState state = new AsyncState() ;
			m_Instance.gameObject.SetActive( true ) ;
			m_Instance.StartCoroutine( m_Instance.In_Private( delay, duration, state ) ) ;

			await m_Instance.WaitUntil( () => state.IsDone ) ;
		}

		/// フェードインを実行する
		private IEnumerator In_Private( float delay, float duration, AsyncState state )
		{
			if( m_FadeType == FadeTypes.Unknown )
			{
				// 問題あり
				state.IsDone = true ;
				yield break ;
			}

			//----------------------------------

			if( m_Rectangle == null )
			{
				// 問題有り
				state.IsDone = true ;
				yield break ;
			}

			if( m_FadeType != FadeTypes.Unknown && m_FadeType != FadeTypes.Color )
			{
				// 問題あり
				state.IsDone = true ;
				yield break ;
			}

			if( m_Rectangle.IsAnyTweenPlaying == true )
			{
				// 既に何らかの Tween が実行されているので強制的に停止させる
				m_Rectangle.StopAllTweens() ;
			}

			//----------------------------------

			m_Playing = true ;

			//----------------------------------

			if( m_FadeType == FadeTypes.Color )
			{
				// 単色

				if( delay <  0 )
				{
					delay  = DefaultDelay ;
				}

				if( duration <  0 )
				{
					duration = DefaultDuration ;
				}

	//			Debug.Log( "<color=#FF7F00>---------------->FadeIn開始</color>" ) ;

				if( duration != 0 )
				{
					yield return m_Rectangle.PlayTween( "FadeIn", delay, duration ) ;
				}

				m_Rectangle.SetActive( false ) ;

	//			Debug.Log( "<color=#FF7F00>---------------->FadeIn終了</color>" ) ;
			}

			//------------------------------------------------------------------------------------------

			m_FadeType = FadeTypes.Unknown ;

			m_Playing = false ;
			m_Showing = false ;

			state.IsDone = true ;

			// state.IsDone より後に実行する事
			gameObject.SetActive( false ) ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// フェードのデフォルトの色を設定する
		/// </summary>
		/// <param name="aarrggbb">色値(ＡＡＲＲＧＧＢＢ)</param>
		public static void SetDefaultColor( uint aarrggbb )
		{
			Color32 color = new Color32
			(
				( byte )( ( aarrggbb >> 16 ) & 0xFF ),
				( byte )( ( aarrggbb >>  8 ) & 0xFF ),
				( byte )( ( aarrggbb >>  0 ) & 0xFF ),
				( byte )( ( aarrggbb >> 24 ) & 0xFF )
			) ;

			SetDefaultColor( color ) ;
		}

		/// <summary>
		/// フェードのデフォルトの色を設定する
		/// </summary>
		/// <param name="color">色</param>
		public static void SetDefaultColor( Color color )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.DefaultColor = color ;
		}

		/// <summary>
		/// フェードのデフォルトの遅延時間を設定する
		/// </summary>
		/// <param name="delay">遅延時間(秒)</param>
		public static void SetDefaultDelay( float delay )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.DefaultDelay = delay ;
		}

		/// <summary>
		/// フェードのデフォルトの実行時間を設定する
		/// </summary>
		/// <param name="duration">実行時間(秒)</param>
		public static void SetDefaultDuration( float duration )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.DefaultDuration = duration ;
		}
	}
}
