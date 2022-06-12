using System  ;
using System.Collections ;
using System.Text ;
using UnityEngine ;

namespace uGUIHelper
{
	/// <summary>
	/// 実機用デバッグログ Version 2020/11/16
	/// </summary>
	// スタティックにしたい場合
	[ ExecuteInEditMode ]
	public class DebugScreen : MonoBehaviour
	{
		/// <summary>
		/// 常駐するシングルトンインスタンス
		/// </summary>
		private static DebugScreen m_Instance = null ;

		public GUISkin Skin ;
		public ArrayList TextArray = new ArrayList() ;
		public string Text = string.Empty ;
		public bool Enable = true ;
		private bool View = true ;
		public int Max = 0 ;
		public int WithUnityLog = 0 ;	// LogWarning
		
		public static uint TextColor = 0xFFFFFFFF ;
		public static int FontSize = 16 ;
		public static bool WordWarp = true ;
		
		public bool QuitOnAndroid = false ;
		
		public static Action OnDownButton ;

		//------------------------------------

		private Texture2D m_Mask ;

		private Texture2D m_ButtonNormal ;
		private Texture2D m_ButtonHover ;
		private Texture2D m_ButtonActive ;


		private GUIStyle m_TextStyle ;
		private GUIStyle m_ButtonStyle ;

		private Color	m_TextColor ;
		private int		m_RealFontSize ;

		private Rect m_R ;
		
		private StringBuilder m_SB ;

		private bool m_IsWaiting ;

		//----------------------------------------------------------------------------

		/// <summary>
		/// 入力まち用の状態クラス
		/// </summary>
		public class OutWait : CustomYieldInstruction
		{
			private readonly MonoBehaviour m_Owner = default ;
			public OutWait( MonoBehaviour owner )
			{
				// 自身が削除された際にコルーチンの終了待ちをブレイクする施策
				m_Owner = owner ;
			}

			public override bool keepWaiting
			{
				get
				{
					if( IsDone == false && m_Owner != null && m_Owner.gameObject.activeInHierarchy == true )
					{
						return true ;    // 継続
					}
					return false ;   // 終了
				}
			}

			/// <summary>
			/// 通信が終了したかどうか
			/// </summary>
			public bool IsDone ;
		}

		//----------------------------------------------------------------------------

		/// <summary>
		/// Awake
		/// </summary>
		void Awake()
		{
			if( Application.isPlaying == true )
			{
				DontDestroyOnLoad( this ) ;
			}

			if( TextArray != null )
			{
				TextArray.Clear() ;
			}

			m_TextStyle = new GUIStyle() ;
			m_ButtonStyle = new GUIStyle( "button" ) ;

			m_R = new Rect() ;

			m_SB = new StringBuilder( 32, 64 ) ;
		}
		
		/// <summary>
		/// Start
		/// </summary>
		void Start()
		{
			if( Max <= 0 )
			{
				Max = ( int )( ( Screen.height * 0.5f ) / 16 ) + 30 ;
			}

			m_Mask = new Texture2D( 4, 4 ) ;
			int x, y ;
			for( y  = 0 ; y <  4 ; y ++ )
			{
				for( x  = 0 ; x <  4 ; x ++ )
				{
					m_Mask.SetPixel( x, y, new Color( 0, 0, 0, 0.5f ) ) ;
				}
			}
			m_Mask.Apply() ;

			m_ButtonNormal	= MakeButtonColor( 64, 64, new Color( 0.50f, 0.50f, 0.50f, 0.25f ), false ) ;
			m_ButtonHover	= MakeButtonColor( 64, 64, new Color( 0.25f, 0.50f, 0.50f, 0.25f ), false ) ;
			m_ButtonActive	= MakeButtonColor( 64, 64, new Color( 0.40f, 0.40f, 0.40f, 0.25f ), true  ) ;
		}
		
		private Texture2D MakeButtonColor( int w, int h, Color color, bool r )
		{
			Texture2D t = new Texture2D( w, h ) ;

			int x, y ;
			for( y  = 1 ; y <= ( h - 2 ) ; y ++ )
			{
				for( x  = 1 ; x <  ( w - 2 ) ; x ++ )
				{
					t.SetPixel( x, y, color ) ;
				}
			}

			Color c0 = new Color( 0.75f, 0.75f, 0.75f, color.a ) ;
			Color c1 = new Color( 0.25f, 0.25f, 0.25f, color.a ) ;
			Color c2 = new Color( 0.50f, 0.50f, 0.50f, color.a ) ;

			if( r == true )
			{
				Color ct ;
				ct = c0 ; 
				c0 = c1 ;
				c1 = ct ;
			}

			for( x  = 0 ; x <= ( w - 2 ) ; x ++ )
			{
				t.SetPixel( x, h - 1, c0 ) ;
			}

			t.SetPixel( w - 1, h - 1, c2 ) ;

			for( y  = ( h - 2 ) ; y >= 0 ; y -- )
			{
				t.SetPixel( w - 1, y, c1 ) ;
			}

			for( x  = ( w - 2 ) ; x >= 1 ; x -- )
			{
				t.SetPixel( x, 0, c1 ) ;
			}

			t.SetPixel( 0, 0, c2 ) ;

			for( y  = 1 ; y <= ( h - 2 ) ; y ++ )
			{
				t.SetPixel( 0, y, c0 ) ;
			}

			t.Apply() ;

			return t ;
		}

		/// <summary>
		/// Update
		/// </summary>
		void Update()
		{	
			// Androidで戻るキーでアプリを終了させる
			if( QuitOnAndroid == true )
			{
				if( Application.platform == RuntimePlatform.Android && Input.GetKey( KeyCode.Escape ) )
				{
					Application.Quit() ;
				}
			}
		}
		
		//--------------------------------------------------------

		private int m_BeforeUM = 0 ;
		private int m_MinUM = 0 ;
		private int m_MaxUM = 0 ;


		private const string _ANDROID	= "Android" ;
		private const string _CLEAR		= "CLEAR" ;
		private const string _HIDE		= "HIDE" ;
		private const string _SHOW		= "SHOW" ;
		private const string _SKIP		= "SKIP" ;
		private const string _GC		= "GC" ;
		private const string _HIER		= "HIER" ;
		private const string _WEB1		= "WEB1" ;
		private const string _WEB2		= "WEB2" ;
		private const string _WEB3		= "WEB3" ;
		private const string _UMT		= " {0:D}.{1:D} ( {2:D}.{3:D} - {4:D}.{5:D} ) / {6:D} MB " ;

		// ＧＵＩ描画
		void OnGUI()
		{
			if( Enable == true )
			{
				if( Skin != null )
				{
					if( SystemInfo.operatingSystem.Contains( _ANDROID ) == true )
					{
						GUI.skin = Skin ;
					}
				}
				
				int sw = Screen.width ;
				int sh = Screen.height ;
				int fb ;
				if( sw <  sh )
				{
					fb = sw ;
				}
				else
				{
					fb = sh ;
				}

				m_RealFontSize = ( int )( FontSize * fb / 1080 ) ;

				//----------------------------------

				m_TextStyle.fontSize = m_RealFontSize ;

				m_TextColor.r = ( float )( ( TextColor >> 16 ) & 0xFF ) / 255.0f ;
				m_TextColor.g = ( float )( ( TextColor >>  8 ) & 0xFF ) / 255.0f ;
				m_TextColor.b = ( float )( ( TextColor >>  0 ) & 0xFF ) / 255.0f ;
				m_TextColor.a = ( float )( ( TextColor >> 24 ) & 0xFF ) / 255.0f ;

				m_TextStyle.normal.textColor = m_TextColor ;
				m_TextStyle.wordWrap = WordWarp ;

				//----------------------------------
				
				if( View == true )
				{
					// マスク画像
					m_R.x = 0 ; m_R.y = 0 ; m_R.width = Screen.width ; m_R.height = Screen.height ;
					GUI.DrawTexture( m_R, m_Mask ) ;

					m_R.x = 0 ; m_R.y = 0 ; m_R.width = Screen.width ; m_R.height = Screen.height * 0.5f ;
					m_TextStyle.alignment = TextAnchor.UpperLeft ;
					GUI.Label( m_R, Text, m_TextStyle ) ;
				}
				
				//----------------------------------

				int w = m_RealFontSize * 5 ;
				int h = m_RealFontSize * 2 ;
				
				m_ButtonStyle.alignment			= TextAnchor.MiddleCenter ;
				m_ButtonStyle.fontSize			= m_RealFontSize ;
				m_ButtonStyle.normal.textColor	= Color.white ;
				m_ButtonStyle.hover.textColor	= m_ButtonStyle.normal.textColor ;
				m_ButtonStyle.active.textColor	= m_ButtonStyle.normal.textColor ;

				m_ButtonStyle.normal.background	= m_ButtonNormal ;
				m_ButtonStyle.hover.background	= m_ButtonHover ;
				m_ButtonStyle.active.background	= m_ButtonActive ;

				//--------------
				
				m_R.x = Screen.width - w ; m_R.y = 0 ; m_R.width = w ; m_R.height = h ;
				if( GUI.Button( m_R, _CLEAR, m_ButtonStyle ) == true )
				{
					TextArray.Clear() ;
					Text = string.Empty ;
				}
				
				string ln ;
				if( View == true )
				{
					ln = _HIDE ;
				}
				else
				{
					ln = _SHOW ;
				}
				
				m_R.x = Screen.width - w ; m_R.y = h *  1.5f ; m_R.width = w ; m_R.height = h ;
				if( GUI.Button( m_R, ln, m_ButtonStyle ) == true )
				{
					if( View == true )
					{
						View  = false ;
					}
					else
					{
						View  = true ;
					}
				}
				
				m_R.x = Screen.width - w ; m_R.y = h *  3.0f ; m_R.width = w ; m_R.height = h ;
				m_ButtonStyle.normal.textColor	= m_IsWaiting ? Color.green : Color.white ;
				m_ButtonStyle.hover.textColor	= m_ButtonStyle.normal.textColor ;
				m_ButtonStyle.active.textColor	= m_ButtonStyle.normal.textColor ;
				if( GUI.Button( m_R, _SKIP, m_ButtonStyle ) == true )
				{
					if( OnDownButton != null )
					{
						OnDownButton() ;
						OnDownButton = null ;
					}

					m_IsWaiting = false ;
				}
				m_ButtonStyle.normal.textColor	= Color.white ;
				m_ButtonStyle.hover.textColor	= m_ButtonStyle.normal.textColor ;
				m_ButtonStyle.active.textColor	= m_ButtonStyle.normal.textColor ;


				m_R.x = Screen.width - w ; m_R.y = h *  4.5f ; m_R.width = w ; m_R.height = h ;
				if( GUI.Button( m_R, _GC, m_ButtonStyle ) == true )
				{
					System.GC.Collect() ;
				}

				m_R.x = Screen.width - w ; m_R.y = h *  6.0f ; m_R.width = w ; m_R.height = h ;
				if( GUI.Button( m_R, _HIER, m_ButtonStyle ) == true )
				{
					GameObject[] gos =  Array.FindAll( GameObject.FindObjectsOfType<GameObject>(), ( item ) => item.transform.parent == null ) ;
					int i, l = 0 ;
					if( gos != null )
					{
						l = gos.Length ;
					}

					AddText( "-----------Hierarchy[ " + l + " ]", 0 ) ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						AddText( " " + gos[ i ].name, 0 ) ;
					}
					AddText( "-----------", 0 ) ;
				}

				m_R.x = Screen.width - w ; m_R.y = h *  7.5f ; m_R.width = w ; m_R.height = h ;
				if( GUI.Button( m_R, _WEB1, m_ButtonStyle ) == true )
				{
					// ウェブビューを表示する
					OpenWebView( "https://webview-3vwzikit-qa01.fate-go.us/" ) ;
				}

				m_R.x = Screen.width - w ; m_R.y = h *  9.0f ; m_R.width = w ; m_R.height = h ;
				if( GUI.Button( m_R, _WEB2, m_ButtonStyle ) == true )
				{
					// ウェブビューを表示する
					OpenWebView( "https://webview-3vwzikit-qa01.fate-go.us/webview/help/index.html" ) ;
				}

				m_R.x = Screen.width - w ; m_R.y = h * 10.5f ; m_R.width = w ; m_R.height = h ;
				if( GUI.Button( m_R, _WEB3, m_ButtonStyle ) == true )
				{
					// ウェブビューを表示する
					OpenWebView( "https://www.google.co.jp/" ) ;
				}

				//----------------------------------

				// ウェブビュー表示時限定のＧＵＩ
	/*			if( m_WebViewObject != null )
				{
					GUI.enabled = m_WebViewObject.CanGoBack() ;
					m_R.x =  10 ; m_R.y =  10 ; m_R.width =  80 ; m_R.height =  80 ;
					if( GUI.Button( m_R, "<" ) )
					{
						m_WebViewObject.GoBack() ;
					}
					GUI.enabled = true ;

					GUI.enabled = m_WebViewObject.CanGoForward();
					m_R.x = 100 ; m_R.y =  10 ; m_R.width =  80 ; m_R.height =  80 ;
					if( GUI.Button( m_R, ">" ) )
					{
						m_WebViewObject.GoForward() ;
					}
					GUI.enabled = true ;

					m_R.x = 600 ; m_R.y =  10 ; m_R.width =  80 ; m_R.height =  80 ;
					if( GUI.Button( m_R, "x") )
					{
						GameObject.Destroy( m_WebViewObject.gameObject ) ;
						m_WebViewObject = null ;
					}
				}*/

				//----------------------------------
				// 使用メモリー表示

				int mu = ( int )System.GC.GetTotalMemory( false ) ;

				if( mu <  m_BeforeUM )
				{
					m_MinUM = mu ;
					m_MaxUM = m_BeforeUM ;
				}

				m_BeforeUM = mu ;

				if( m_SB != null )
				{
					m_SB.Length = 0 ;
					m_SB.AppendFormat
					(
						_UMT,
						  mu / 1048576,
						( mu % 1048576 ) * 10 / 1048576,
						  m_MinUM / 1048576,
						( m_MinUM % 1048576 ) * 10 / 1048576,
						  m_MaxUM / 1048576,
						( m_MaxUM % 1048576 ) * 10 / 1048576,
						SystemInfo.systemMemorySize
					) ;

					string mt = m_SB.ToString() ;
		
					// マスク画像
					m_R.x = Screen.width * 0.5f ; m_R.y = Screen.height - ( m_RealFontSize * 1.5f ) ; m_R.width = Screen.width * 0.5f ; m_R.height = ( m_RealFontSize * 1.2f ) ;
					GUI.DrawTexture( m_R, m_Mask ) ;
					m_TextStyle.alignment = TextAnchor.MiddleCenter ;
					GUI.Label( m_R, mt, m_TextStyle ) ;
				}
			}
		}
		
		public void AddText( string s, int withUnityLog )
		{
			s = s.Replace( "\n", "" ) ;

			TextArray.Add( s + "\n" ) ;
			
			int i ;
			
			int i1 = TextArray.Count - 1 ;
			int i0 = i1 - Max ;
			if( i0 <  0 )
			{
				i0  = 0 ;
			}
			
			Text = "" ;
			for( i  = i0 ; i <= i1 ; i ++  )
			{
				Text += ( TextArray[ i ] as string ) ;
			}
			
			if( i1 >  Max )
			{
				TextArray.Remove( 0 ) ;
			}

			if( withUnityLog == 0 )
			{
				withUnityLog  = WithUnityLog ;
			}

			if( withUnityLog == 1 ){ UnityEngine.Debug.Log( "[DebugScreen] " + s ) ;		}
			else
			if( withUnityLog == 2 ){ UnityEngine.Debug.LogWarning( "[DebugScreen] " + s ) ;	}
			else
			if( withUnityLog == 3 ){ UnityEngine.Debug.LogError( "[DebugScreen] " + s ) ;	}
		}
		
		public static OutWait OutAndWait( string s )
		{
			Out( s ) ;

			if( m_Instance == null )
			{
				return null ;
			}

			OutWait ow = new OutWait( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.OutAndWait_Private( ow ) ) ;
			return ow ;
		}

		private IEnumerator OutAndWait_Private( OutWait ow )
		{
			m_IsWaiting = true ;
			
			while( true )
			{
				if( m_IsWaiting == false )
				{
					break ;
				}
				yield return null ;
			}

			DebugScreen.OnDownButton = null ;

			ow.IsDone = true ;
		}

		// デバッグスクリーンに文字列を追加する
		public static void Log( string s, int withUnityLog = 0 )
		{
			Out( s, withUnityLog ) ;
		}

		// デバッグスクリーンに文字列を追加する
		public static void Out( string s, int withUnityLog = 0 )
		{
			DebugScreen ds ;
			
			if( m_Instance == null )
			{
				ds = ( DebugScreen )GameObject.FindObjectOfType( typeof( DebugScreen ) ) ;
				if( ds == null )
				{
					return ;
				}
			}
			else
			{
				ds = m_Instance ;
			}

			ds.AddText( s, withUnityLog ) ;
		}
		
		public static void SetTextColor( uint textColor )
		{
			TextColor = textColor ;
		}
		
		public static void SetFontSize( int fontSize )
		{
			FontSize = fontSize ;
		}
		
		public static void Create()
		{
			Create( TextColor, FontSize, 0, 2 ) ;
		}
		
		public static void Create( uint textColor )
		{
			Create( textColor, FontSize, 0, 2 ) ;
		}
		
		public static void Create( uint textColor, int fontSize, int max, int withUnityLog = 0 )
		{
			TextColor = textColor | 0xFF000000 ;
			FontSize  = fontSize ;
			
			DebugScreen ds = ( DebugScreen )GameObject.FindObjectOfType( typeof( DebugScreen ) ) ;
			if( ds == null )
			{
				GameObject go = new GameObject( "DebugScreen" ) ;
				ds = go.AddComponent<DebugScreen>() ;
			}

			m_Instance = ds ;

			m_Instance.Max = max ;

			m_Instance.WithUnityLog = withUnityLog ;
		}
		
		public static void Terminate()
		{
			DebugScreen ds = ( DebugScreen )GameObject.FindObjectOfType( typeof( DebugScreen ) ) ;
			if( ds != null )
			{
				Destroy( ds.gameObject ) ;
			}
		}

		void OnDestroy()
		{
			if( this == m_Instance )
			{
				m_Instance = null ;
			}		
		}

		//----------------------------------------------------------------------------

	//	private static WebViewObject m_WebViewObject ;

		/// <summary>
		/// ウェブビューを表示する(一時的)
		/// </summary>
		/// <param name="url"></param>
		private static void OpenWebView( string url )
		{
	/*
	#if UNITY_EDITOR
	*/		
			Application.OpenURL( url ) ;

			return ;
	/*
	#else
			if( m_WebViewObject != null )
			{
				return ;	// 既に開いている
			}

			m_WebViewObject = ( new GameObject( "WebViewObject" )).AddComponent<WebViewObject>() ;
			m_WebViewObject.Init
			(
				cb: ( msg ) =>
				{
				},
				err: ( msg ) =>
				{
				},
				started: ( msg ) =>
				{
				},
				ld: ( msg ) =>
				{
					m_WebViewObject.EvaluateJS( @"Unity.call('ua=' + navigator.userAgent)" ) ;
				},
				//ua: "custom user agent string",
				enableWKWebView: true
			) ;

			m_WebViewObject.SetMargins( 5, 100, 5, Screen.height / 4 ) ;
			m_WebViewObject.SetVisibility( true ) ;

			if( url.StartsWith( "http" ) == true )
			{
				m_WebViewObject.LoadURL( url.Replace( " ", "%20") ) ;
			}
	#endif
	*/
		}
	}
}
