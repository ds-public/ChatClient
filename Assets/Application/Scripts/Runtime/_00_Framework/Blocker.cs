using System ;
using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;

using Cysharp.Threading.Tasks ;

using uGUIHelper ;

namespace Template
{
	/// <summary>
	/// 単色の画面マスク制御クラス Version 2022/02/16 0
	/// </summary>
	public class Blocker : ExMonoBehaviour
	{
		// シングルトンインスタンス
		private static Blocker m_Instance = null ; 

		/// <summary>
		/// ブロッキングマスクのインスタンス
		/// </summary>
		public  static Blocker   Instance
		{
			get
			{
				return m_Instance ;
			}
		}

		//-----------------------------------------------------------

		// キャンバス部分のインスタンス
		[SerializeField]
		protected UICanvas		m_Canvas ;

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

			return true ;
		}

		//-------------------------------------

		// フェード部分のインスタンス
		[SerializeField]
		protected UIView		m_MaskEmpty ;

		[SerializeField]
		protected UIImage		m_MaskColor ;

		//-----------------------------------

		private bool			m_IsClick = false ;
		private bool			m_IsPress = false ;
		private bool			m_IsPressing = false ;

		private Action			m_OnClick = null ;
		private Action			m_OnAddableClick = null ;

		private Action<bool>	m_OnPress = null ;
		private Action<bool>	m_OnAddablePress = null ;

		private bool			m_Execute = false ;

		//---------------------------------------------------------------------------

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
		}

		//-----------------------------------

		/// <summary>
		/// 単色のフェードフェクトを表示する(フェードイン前の準備)
		/// </summary>
		/// <param name="aarrggbb">色値(ＡＡＲＲＧＧＢＢ)</param>
		public static void On( Action onClick = null, uint aarrggbb = 0x00000000 )
		{
			Color32 color= new Color32
			(
				( byte )( ( aarrggbb >> 16 ) & 0xFF ),
				( byte )( ( aarrggbb >>  8 ) & 0xFF ),
				( byte )( ( aarrggbb >>  0 ) & 0xFF ),
				( byte )( ( aarrggbb >> 24 ) & 0xFF )
			) ;

			On( onClick, color ) ;
		}

		/// <summary>
		/// 単色のフェードフェクトを表示する(フェードイン前の準備)
		/// </summary>
		/// <param name="color">色</param>
		public static void On( Action onClick, Color color )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.On_Private( onClick, color ) ;
		}

		// 単色のフェードフェクトを表示する(フェードイン前の準備)
		private void On_Private( Action onClick, Color color )
		{
			if( m_MaskEmpty == null || m_MaskColor == null )
			{
				return ;
			}

			m_OnClick = onClick ;

			m_MaskEmpty.IsInteraction = true ;
			m_MaskEmpty.RaycastTarget = true ;

			m_MaskEmpty.SetOnClick( ( string identity, UIView view ) =>
			{
				m_IsClick = true ;
				m_Execute = true ;

				m_OnClick?.Invoke() ;
				m_OnClick = null ;

				m_OnAddableClick?.Invoke() ;
			} ) ;

			m_MaskEmpty.SetOnPress( ( string identity, UIView view, bool state ) =>
			{
				if( state == true )
				{
					m_IsPress = true ;	// 離しても false にしてはならない
					m_Execute = true ;
				}

				m_IsPress = state ;

				m_OnPress?.Invoke( state ) ;

				m_OnAddablePress?.Invoke( state ) ;
			} ) ;

			m_IsClick = false ;
			m_IsPress = false ;
			m_Execute = false ;

			//----------------------------------

			m_MaskColor.IsInteraction = false ;
			m_MaskColor.RaycastTarget = false ;

			if( color.a != 0 )
			{
				m_MaskColor.SetActive( true ) ;
				m_MaskColor.Color = color ;
			}
			else
			{
				m_MaskColor.SetActive( false ) ;
			}

			//----------------------------------

			gameObject.SetActive( true ) ;
		}

		/// <summary>
		/// 単色のフェードフェクトを表示する(フェードイン前の準備)
		/// </summary>
		public static void Off()
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.Off_Private() ;
		}


		// 単色のフェードフェクトを表示する(フェードイン前の準備)
		private void Off_Private()
		{
			// タスクが動いているようなら止める
			m_IsClick = true ;
			m_IsPress = true ;

			m_OnClick = null ;
			m_OnPress = null ;

			if( m_MaskEmpty != null )
			{
				m_MaskEmpty.SetOnClick( null ) ;
				m_MaskEmpty.SetOnPress( null ) ;
			}

			gameObject.SetActive( false ) ;
		}
		
		/// <summary>
		/// ブロッカーか有効な状態でクリックされた際に呼び出すコールバックを登録する
		/// </summary>
		/// <param name="onClick"></param>
		public static void SetOnClick( Action onClick )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.m_OnClick = onClick ;
		}

		/// <summary>
		/// ブロッカーか有効な状態でクリックされた際に呼び出すコールバックを追加する
		/// </summary>
		/// <param name="onClick"></param>
		public static void AddOnClick( Action onClick )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.m_OnAddableClick += onClick ;
		}

		/// <summary>
		/// ブロッカーか有効な状態でクリックされた際に呼び出すコールバックを削除する
		/// </summary>
		/// <param name="onClick"></param>
		public static void RemoveOnClick( Action onClick )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.m_OnAddableClick -= onClick ;
		}

		/// <summary>
		/// ブロッカーか有効な状態でクリックされた際に呼び出すコールバックを追加する
		/// </summary>
		/// <param name="onClick"></param>
		public static void AddOnPress( Action<bool> onPress )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.m_OnAddablePress += onPress ;
		}

		/// <summary>
		/// ブロッカーか有効な状態でクリックされた際に呼び出すコールバックを削除する
		/// </summary>
		/// <param name="onClick"></param>
		public static void RemoveOnPress( Action<bool> onPress )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.m_OnAddablePress -= onPress ;
		}

		/// <summary>
		/// ブロッカーが有効か判定する
		/// </summary>
		/// <returns></returns>
		public static bool IsEnabled
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

		/// <summary>
		///  現在押されている最中かどうか
		/// </summary>
		public static bool IsPress
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}
				return m_Instance.m_IsPressing ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ブロッカー上でクリックされるのを待つ
		/// </summary>
		/// <returns></returns>
		public static UniTask WaitForClick( Action onClick = null )
		{
			if( m_Instance == null )
			{
				return new UniTask() ;
			}

			return m_Instance.WaitForClick_Private( onClick ) ;
		}

		private async UniTask WaitForClick_Private( Action onClick )
		{
			if( gameObject.activeSelf == false )
			{
				return ;
			}

			m_IsClick = false ;
			m_Execute = false ;
			await WaitUntil( () => m_IsClick ) ;

			if( m_Execute == true )
			{
				// 実際にアクションが実行された場合のみコールバックを実行する
				onClick?.Invoke() ;
			}
		}

		/// <summary>
		/// ブロッカー上でプレスされるのを待つ
		/// </summary>
		/// <returns></returns>
		public static UniTask WaitForPress( Action onPress = null )
		{
			if( m_Instance == null )
			{
				return new UniTask() ;
			}

			return m_Instance.WaitForPress_Private( onPress ) ;
		}

		private async UniTask WaitForPress_Private( Action onPress )
		{
			if( gameObject.activeSelf == false )
			{
				return ;
			}

			m_IsPress = false ;
			m_Execute = false ;
			await WaitUntil( () => m_IsPress ) ;

			if( m_Execute == true )
			{
				// 実際にアクションが実行された場合のみコールバックを実行する
				onPress?.Invoke() ;
			}
		}
	}
}
