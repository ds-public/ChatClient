using System ;
using System.Collections ;
using System.Collections.Generic ;

using System.Threading ;
using Cysharp.Threading.Tasks ;

using UnityEngine ;

// 要 uGUIHelper パッケージ
using uGUIHelper ;
using UnityEngine.SceneManagement ;

namespace Template
{
	/// <summary>
	/// プログレスクラス(待ち演出などに使用する) Version 2022/06/12 0
	/// </summary>
	public class Progress : ExMonoBehaviour
	{
		// シングルトンインスタンス
		private static Progress m_Instance ;

		/// <summary>
		/// プログレスクラスのインスタンス
		/// </summary>
		public  static Progress   Instance
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

			return true ;
		}

		[SerializeField]
		protected UIView	m_Screen ;

		//---------------------------------------------------------------------------

		public enum Styles
		{
			Default,
			LoadingLong,
			WebAPI,
		}

		// 選択中のスタイル
		[SerializeField]
		protected	Styles	m_Style = Styles.WebAPI ;

		//---------------------------------------------------------------------------
		// 各スタイルの要素

		[Serializable]
		public class Style_WebAPI
		{
			public	UIView				View ;
			public	UIImage				Fade ;
			public	UIImage				Animation ;
			public	UITextMesh			Message ;
		}

		[SerializeField]
		protected	Style_WebAPI		m_Style_WebAPI ;

		private		bool				m_Style_WebAPI_IsFade = true ;
		private		string				m_Style_WebAPI_Message = null ;

		//---------------------------------------------------------------------------

		// プログレス継続中のフラグ
		private bool m_On ;

		/// <summary>
		/// プログレス継続中のフラグ
		/// </summary>
		public static bool IsOn
		{
			get
			{
				if( m_Instance == null )
				{
					Debug.LogError( "Progress is not create !" ) ;
					return false ;
				}

				return m_Instance.m_On ;
			}

			set
			{
				if( m_Instance == null )
				{
					Debug.LogError( "Progress is not create !" ) ;
					return ;
				}

				m_Instance.m_On = value ;
			}
		}

		// プログレスを消去中かどうか(コルーチンを使用しているので連続実行を抑制する必要がある)
//		private CancellationTokenSource m_ActiveTask ;
		private IEnumerator m_ActiveTask ;

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

			// デフォルトを設定する

			SetStyle_WebAPI( false, "通信中" ) ;

			gameObject.SetActive( false ) ;

			//----------------------------------------------------------
#if UNITY_EDITOR
			if( SceneManager.GetActiveScene().name == "Progress" )
			{
				gameObject.SetActive( true ) ;
			}
#endif
		}

#if UNITY_EDITOR
		internal IEnumerator Start()
		{
			// デバッグ
			if( SceneManager.GetActiveScene().name == "Progress" )
			{
				// 全てのスタイルを一旦非表示
				HideAllStyles() ;

				int i, l = 2 ;	// アニメーションの繋がりに違和感が無いか２回確認する
				for( i  = 0 ; i <  l ; i ++ )
				{
					// フェードインを確認したいので少し待機
					yield return new WaitForSeconds( 1 ) ;

					// 動作を確認したいスタイル
					SetStyle_WebAPI() ;

					On() ;

					// フェードアウトを確認したいので少し待機
					yield return new WaitForSeconds( 3 ) ;

					Off() ;
				}
			}
			yield break ;
		}
#endif

/*		internal void OnDisable()
		{
			Debug.Log( "プログレス終了" ) ;
			if( m_ActiveTask != null )
			{
				m_ActiveTask.Cancel() ;
				m_ActiveTask.Dispose() ;
				m_ActiveTask  = null ;
			}
		}*/

		//---------------------------------------------------------------------------

		/// <summary>
		/// プログレスを表示する
		/// </summary>
		public static bool Show()
		{
			if( m_Instance == null )
			{
				Debug.LogError( "Progress is not create !" ) ;
				return false ;
			}

			//----------------------------------------------------------

			m_Instance.Show_Private() ;

			return true ;
		}

		// プログレスを表示する(実装部)　※ここに別のタイプを実装する(表示するタイプを切り替えられるようにする)
		private void Show_Private()
		{
			if( m_Style == Styles.WebAPI )
			{
				// Style : WebAPI

				Style_WebAPI style = m_Style_WebAPI ;
				style.View.SetActive( true ) ;

				UITween tween ;

				if( m_ActiveTask != null )
				{
//					m_ActiveTask.Cancel() ;
//					m_ActiveTask = null ;
					StopCoroutine( m_ActiveTask ) ;
					m_ActiveTask = null ;

					if( m_Instance.gameObject.activeSelf == true )
					{
						tween = style.Fade.GetTween( "FadeOut" ) ;
						if( tween != null && ( tween.IsRunning == true || tween.IsPlaying == true ) )
						{
							// フェードアウト表示中なので終了させる
							tween.Finish() ;
						}
					}
				}

				//----------------------------------------------------------

				if( m_Instance.gameObject.activeSelf == false )
				{
					m_Instance.gameObject.SetActive( true ) ;
				}

				//----------------------------------------------------------

				tween = style.Fade.GetTween( "FadeIn" ) ;
				if( tween != null && tween.IsPlaying == true )
				{
					return ;	// 既に表示最中
				}

				// 改めてフェードイン再生
				style.Fade.PlayTween( "FadeIn" ) ;

				// アニメーション再生
				style.Animation.PlayFlipper( "Move" ) ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// プログレスを消去する
		/// </summary>
		public static bool Hide()
		{
			if( m_Instance == null )
			{
				Debug.LogError( "Progress is not create !" ) ;
				return false ;
			}

			//----------------------------------------------------------

			m_Instance.Hide_Private() ;

			return true ;
		}

		// プログレスを消去する(実装部)　※ここに別のタイプを実装する(表示するタイプを切り替えられるようにする)
		private void Hide_Private()
		{
			if( gameObject.activeSelf == false || m_ActiveTask != null )
			{
				// 既に非表示中
				return ;
			}
			
			if( m_Style == Styles.WebAPI )
			{
				// Style : WebAPI

				Style_WebAPI style = m_Style_WebAPI ;

				UITween tween ;

				tween = style.Fade.GetTween( "FadeIn" ) ;
				if( tween != null && ( tween.IsRunning == true || tween.IsPlaying == true ) )
				{
					// フェードイン中なので終了させる
					tween.Finish() ;
				}

//				m_ActiveTask = new CancellationTokenSource() ;
				m_ActiveTask = Hiding_Private() ;
				StartCoroutine( m_ActiveTask ) ;
			}
		}

		// プログレスを消去する(実装部・コルーチン)
		private IEnumerator Hiding_Private()
		{
			if( m_Style == Styles.WebAPI )
			{
				// Style : WebAPI

				Style_WebAPI style = m_Style_WebAPI ;

				yield return style.Fade.PlayTween( "FadeOut" ) ;
//				do
//				{
//					if( m_ActiveTask == null || m_ActiveTask.IsCancellationRequested == true )
//					{
//						Debug.Log( "--------プログレスキャンセル" ) ;
//						return ;	// 中断
//					}
//					await Yield() ;
//				}
//				while( style.Fade.IsAnyTweenPlaying == true ) ;

				// アニメーション停止
				style.Animation.StopFlipper( "Move" ) ;

				// プログレス自体を非アクティブにする(負荷軽減)
				gameObject.SetActive( false ) ;

				m_ActiveTask = null ;
//				if( m_ActiveTask != null )
//				{
//					m_ActiveTask.Dispose() ;
//					m_ActiveTask = null ;	// 非表示終了
//				}
			}
		}

		//-------------------------------------------------------------------------------------------

		private void HideAllStyles()
		{
			m_Instance.m_Style_WebAPI.View.SetActive( false ) ;
		}

		/// <summary>
		/// スタイル選択(WebWAPI)
		/// </summary>
		/// <param name="isFade"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public static bool SetStyle_WebAPI( bool isFade = true, string message = null )
		{
			// 全てのスタイルを非アクティブにする
			m_Instance.HideAllStyles() ;

			m_Instance.m_Style = Styles.WebAPI ;

			Style_WebAPI style = m_Instance.m_Style_WebAPI ;

			style.Fade.Enabled = isFade ;

			// null の場合は前回を踏襲する
			if( message != null )
			{
				if( message == string.Empty )
				{
					style.Message.SetActive( false ) ;
				}
				else
				{
					style.Message.SetActive( true ) ;
					style.Message.Text = message ;
				}
			}

			m_Instance.m_Style_WebAPI_IsFade	= isFade ;
			m_Instance.m_Style_WebAPI_Message	= message ;

			return true ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// プログレスを表示する
		/// </summary>
		/// <returns></returns>
		public static bool On( Styles style = Styles.Default, params System.Object[] options )
		{
//			Debug.Log( "[Progrees On] " + style ) ;

			if( style != Styles.Default )
			{
				SetStyle( style, options ) ;
			}

			//----------------------------------------------------------

			m_Instance.m_On = true ;

			m_Instance.Show_Private() ;

			return true ;
		}

		/// <summary>
		/// プログレスのスタイルを設定する
		/// </summary>
		/// <param name="style"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static bool SetStyle( Styles style, params System.Object[] options )
		{
//			Debug.Log( "[Progrees SetStyle] " + style ) ;

			if( m_Instance == null )
			{
				Debug.LogError( "Progress is not create !" ) ;
				return false ;
			}
			
			//----------------------------------------------------------

			if( style == Styles.WebAPI )
			{
				bool isFade		= m_Instance.m_Style_WebAPI_IsFade ;
				string message	= m_Instance.m_Style_WebAPI_Message ;
				if( options != null && options.Length >= 1 && options[ 0 ] != null )
				{
					if( options[ 0 ] is bool single )
					{
						isFade = single ;
					}
				}
				if( options != null && options.Length >= 2 && options[ 1 ] != null && options[ 1 ] is string )
				{
					if( options[ 1 ] is string single )
					{
						message = single ;
					}
				}

				SetStyle_WebAPI( isFade, message ) ;
			}

			return true ;
		}

		/// <summary>
		/// プログレスを消去する
		/// </summary>
		/// <returns></returns>
		public static bool Off()
		{
			if( m_Instance == null )
			{
				Debug.LogError( "Progress is not create !" ) ;
				return false ;
			}
			
			//----------------------------------------------------------

			m_Instance.Hide_Private() ;

			m_Instance.m_On = false ;

			return true ;
		}

		/// <summary>
		/// プログレスを消去する(終了を待てる)
		/// </summary>
		/// <returns></returns>
		public static async UniTask OffAsync()
		{
			if( Off() == false )
			{
				return ;
			}

			if( m_Instance.gameObject.activeSelf == false )
			{
				return ;
			}

			//----------------------------------

			await m_Instance.WaitWhile( () => m_Instance.gameObject.activeSelf == true ) ;
		}

	}
}
