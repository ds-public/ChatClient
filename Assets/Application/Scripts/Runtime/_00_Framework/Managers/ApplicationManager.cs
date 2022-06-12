// UnityEditor 上で強制的に MessagePack の IL2CPP 用の自動生成コードの動作確認をするための定義
//#define CHECK_IL2CPP
//#define USE_ADX2

#if !UNITY_EDITOR
#define USE_EXCEPTION_DIALOG
#endif


using System ;
using System.Collections ;
using System.Collections.Generic ;

using System.IO ;

using Cysharp.Threading.Tasks ;

using UnityEngine ;
using UnityEngine.Assertions ;


#if UNITY_EDITOR
using UnityEditor ;
#endif



// 要 SceneManagementHelper パッケージ
using SceneManagementHelper ;

// 要 InputHelper パッケージ
using InputHelper ;

// 要 AudioHelper パッケージ
using AudioHelper ;

// 要 AssetBundlerHelper パッケージ
using AssetBundleHelper ;

// 要 StorageHealper パッケージ
using StorageHelper ;

using uGUIHelper ;

namespace Template
{
	/// <summary>
	/// アプリケーションマネージャクラス Version 2022/06/12 0
	/// </summary>
	public class ApplicationManager : SingletonManagerBase<ApplicationManager>
	{
		/// <summary>
		/// アプリケーションマネージャのインスタンスを生成する（スクリプトから生成する場合）
		/// </summary>
		/// <param name="runInBackground">バックグラウンド中でも実行するかどうか</param>
		/// <returns>アプリケーションマネージャのインスタンス</returns>
		public static ApplicationManager Create()
		{
			m_Instance = Create( null ) ;	// 基底クラスのものを利用する

			return m_Instance ;
		}

		//-----------------------------------------------------------------

		// アプリケーションマネージャが初期化済みかどうか
		protected bool m_IsInitialized ;

		/// <summary>
		/// アプリケーションマネージャが初期化済みかどうか
		/// </summary>
		public  static bool  IsInitialized
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}

				return m_Instance.m_IsInitialized ;
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// ダウンロードのフェーズごとの実行状態
		/// </summary>
		public enum DownloadingPhaseStates
		{
			None		= 0,
			Completed	= 1,
		}

		protected DownloadingPhaseStates m_DownloadingPhase1State = DownloadingPhaseStates.None ;

		/// <summary>
		/// ダウンロードのフェーズ１の状況
		/// </summary>
		public  static DownloadingPhaseStates  DownloadingPhase1State
		{
			get
			{
				if( m_Instance == null )
				{
					return 0 ;
				}

				return m_Instance.m_DownloadingPhase1State ;
			}
			set
			{
				if( m_Instance == null )
				{
					return ;
				}

				m_Instance.m_DownloadingPhase1State = value ;
			}
		}

		protected DownloadingPhaseStates m_DownloadingPhase2State = DownloadingPhaseStates.None ;

		/// <summary>
		/// ダウンロードのフェーズ２の状況
		/// </summary>
		public  static DownloadingPhaseStates  DownloadingPhase2State
		{
			get
			{
				if( m_Instance == null )
				{
					return 0 ;
				}

				return m_Instance.m_DownloadingPhase2State ;
			}
			set
			{
				if( m_Instance == null )
				{
					return ;
				}

				m_Instance.m_DownloadingPhase2State = value ;
			}
		}

		/// <summary>
		/// ダウンロードリクエストの種別
		/// </summary>
		public enum DownloadingRequestTypes
		{
			Unknown	= 0,
			Phase1	= 1,
			Phase2	= 2,
		}

		//-----------------------------------------------------------

		// バックグラウンド状態での動作
		[SerializeField]
		protected bool m_RunInBackground = true ;

		/// <summary>
		/// バックグラウンド状態での動作
		/// </summary>
		public static bool RunInBackground
		{
			get
			{
				if( m_Instance == null )
				{
#if UNITY_EDITOR
					Debug.LogError( "ApplicationManager is not create !" ) ;
#endif
					return false ;
				}
				return m_Instance.m_RunInBackground ;
			}
		}

		// 接続先エンドポイント(受け渡しのみ)
		[SerializeField]
		protected string m_EndPoint = string.Empty ;

		// プラットフォームタイプ
		private Settings.PlatformType m_PlatformType = Settings.PlatformType.Default ;

		// アプリケーション全体の仮想解像度の横幅
		public float CanvasWidth ;

		// アプリケーション全体の仮想解像度の縦幅
		public float CanvasHeight ;

		//-----------------------------------------------------------------
#if !UNITY_SERVER
		// 最速で実行される特殊メソッド
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		internal static void OnRuntimeMethodLoad()
	    {
			// アサートの出力を有効化
			Assert.IsTrue( true ) ;

			// ログ出力を無効
//			Debug.unityLogger.logEnabled = false ;

			// スリープ禁止
			Screen.sleepTimeout = SleepTimeout.NeverSleep ;

			// デフォルトでマルチタッチを禁止しておく(必要があればシーンごとに許可)
			Input.multiTouchEnabled = false ;

			// 画面の向き設定(ProjectSettingの設定を使用する)
//			Screen.orientation = ScreenOrientation.AutoRotation ;	// 回転許可
#if !UNITY_IOS
			// iOSでは、この設定を後で変更できなかった
			Screen.orientation = ScreenOrientation.Portrait ;		// 回転禁止(デフォルト姿勢指定)
#endif
			Screen.autorotateToPortrait           = true ;			// 縦(順)許可
			Screen.autorotateToPortraitUpsideDown = true ;			// 縦(逆)許可
			Screen.autorotateToLandscapeLeft      = false ;			// 横(順)禁止
			Screen.autorotateToLandscapeRight     = false ;			// 横(逆)禁止

			//----------------------------------------------------------

			Settings settings = ApplicationManager.LoadSettings() ;

#if !UNITY_EDITOR

			// 参考
			// https://techblog.kayac.com/unity-fixed-dpi

			//----------------------------------------------------------
			// スクリプトで強制的に表示解像度を設定する

			float nativeWidth  = Screen.currentResolution.width ;
			float nativeHeight = Screen.currentResolution.height ;

			Debug.Log( "[Native Resolution] W = " + nativeWidth + " H = " + nativeHeight ) ;

			float screenWidth  = 1080 ;
			float screenHeight = 1920 ;

			if( settings != null )
			{
				screenWidth  = settings.BasicWidth ;
				screenHeight = settings.BasicHeight ;
			}

			bool fullScreen = false ;
			int  frameRate = 60 ;

			//---------------------------------------------------------

#if UNITY_STANDALONE

			// スタンドアロン環境

			float scale = 0.5f ;

			screenWidth  *= scale ;
			screenHeight *= scale ;
#endif
			//---------------------------------------------------------

#if UNITY_ANDROID || UNITY_IOS || UNITY_IPHONE

			// モバイル環境

			// 常にフルスクリーン
			fullScreen = true ;

			// 実解像度が表示解像度より低い場合はそのままとする
			if( nativeWidth <  screenWidth || nativeHeight <  screenHeight )
			{
				screenWidth  = nativeWidth ;
				screenHeight = nativeHeight ;
			}
			else
			{
				// 実解像度が表示解像度の1.5倍以上の場合は実解像度の短い辺の長さに合わせた表示解像度の調整を行う
				if( nativeWidth >= ( screenWidth * 1.5f ) && nativeHeight >= ( screenHeight * 1.5f ) )
				{
					if( nativeWidth <  nativeHeight )
					{
						// 横の方が短いので横を基準とした調整を行う
						screenHeight = screenWidth  * nativeHeight / nativeWidth ;
					}
					else
					{
						// 縦の方が短いので縦を基準とした調整を行う
						screenWidth  = screenHeight * nativeWidth  / nativeHeight ;
					}
				}
				else
				{
					// そのまでの差がなければ表示解像度は実解像度と同じにする
					screenWidth  = nativeWidth ;
					screenHeight = nativeHeight ;
				}
			}
#endif
			//---------------------------------------------------------

			Debug.Log( "[Screen Resolution] W = " + screenWidth + " H = " + screenHeight ) ;

			// 表示解像度を設定する
			Screen.SetResolution( ( int )screenWidth, ( int )screenHeight, fullScreen, frameRate ) ;
#endif
			//----------------------------------------------------------

#if UNITY_EDITOR || UNITY_STANDALONE

			// 入力モードをカスタム(ＰＣ用)にする
			UIEventSystem.ProcessType = StandaloneInputModuleWrapper.ProcessType.Custom ;

			// 入力モードをデフォルト(ＰＣ・モバイル)にする
//			UIEventSystem.ProcessType = StandaloneInputModuleWrapper.ProcessType.Default ;
#endif
			//----------------------------------------------------------
			// Unity の情報収集を無効化する

			UnityEngine.Analytics.Analytics.enabled				= false ;
			UnityEngine.Analytics.Analytics.deviceStatsEnabled	= false ;
			UnityEngine.Analytics.Analytics.limitUserTracking	= true ;
#if UNITY_2018_3_OR_NEWER
			UnityEngine.Analytics.Analytics.initializeOnStartup = false ;
#endif
		}
#endif
		//-----------------------------------------------------------------

		/// <summary>
		/// 派生クラスの Awake
		/// </summary>
		new protected void Awake()
		{
			base.Awake() ;

			//----------------------------------------------------------
			// フレームレートを設定する

			// デフォルト
			int frameRate_Rendering = 60 ;
			int frameRate_FixedTime = 60 ;

			Settings settings = LoadSettings() ;
			if( settings != null )
			{
				frameRate_Rendering = settings.FrameRate_Rendering ;
				frameRate_FixedTime = settings.FrameRate_FixedTime ;
			}

			SetFrameRate( frameRate_Rendering, frameRate_FixedTime ) ;

			//----------------------------------------------------------
			// ファサードクラス群を追加する

			SetFacadeClasses() ;

			//----------------------------------------------------------

			// 簡易デバッグログ表示を有効にする
//			DebugScreen.Create( 0xFFFFFF, 36, 48, 1 ) ;
		}

		/// <summary>
		/// フレームレートを設定する
		/// </summary>
		/// <param name="rendering"></param>
		/// <param name="fixedTime"></param>
		public static void SetFrameRate( int rendering, int fixedTime )
		{
			// 描画フレームレートの設定
			Application.targetFrameRate = rendering ;

			// 処理フレームレートの設定
			Time.fixedDeltaTime = 1.0f / ( float )fixedTime ;

			Debug.Log( "<color=#FFFF00>描画フレームレート:" + Application.targetFrameRate + " 処理フレームレート:" + fixedTime + "</color>" ) ;

		}

		/// <summary>
		/// ファサードクラス群を追加する
		/// </summary>
		private void SetFacadeClasses()
		{
			//----------------------------------------------------------
			// ファサードクラスを GameObject の生存期間に同期させる(UniTaskをコロスため)

			GameObject facade = new GameObject( "Facade Classes" ) ;
			facade.transform.SetParent( transform, false ) ;
			facade.transform.localPosition	= Vector3.zero ;
			facade.transform.localRotation	= Quaternion.identity ;
			facade.transform.localScale		= Vector3.one ;

			facade.AddComponent<Scene>() ;
			facade.AddComponent<Asset>() ;

			facade.AddComponent<BGM>() ;
			facade.AddComponent<Ambience>() ;
			facade.AddComponent<Jingle>() ;
			facade.AddComponent<SE>() ;
			facade.AddComponent<Voice>() ;

			facade.AddComponent<Zip>() ;
			facade.AddComponent<GZip>() ;

			facade.AddComponent<WebView>() ;
		}

		//-----------------------------------------------------------------

		// Start は async void が正しい
		internal async void Start()
		{
			_ = Prepare() ;

			await Yield() ;
		}

		// 各種準備処理を行う
		private async UniTask Prepare()
		{
			// ApplicationManager のセットアップに時間がかかるためプログレスだけ最優先でロードして表示する

			//----------------------------------------------------------

			// オーディオ関連のマネージャを生成する(Listenerが1つも無いとログで警告されるため)
			await CreateAudioManagers() ;

			// 常駐型のサブシーンをロードする
			await LoadResidentSubScenes() ;

			//------------------------------------------------------------------

			// プラットフォームタイプを読み出す
			Settings settings = LoadSettings() ;
			if( settings != null )
			{
				m_PlatformType = settings.SelectedPlatformType ;

				CanvasWidth  = settings.BasicWidth ;
				CanvasHeight = settings.BasicHeight ;

				// RunInBackground や ServerName は、いずれ Settings で設定できるようにする
//				m_RunInBackground = true ;
			}

			//-----------------------------

			// バックグラウンド動作設定(ＵｎｉｔｙＥｄｉｔｏｒのみ / Ｕｎｔｙには実機環境で値を書き込んだら落ちるバージョンが存在する)
#if UNITY_EDITOR
			Application.runInBackground = RunInBackground ;
#endif
			Debug.Log( "<color=#FFFF00>RunInBackground:" + Application.runInBackground + "</color>" ) ;

			//----------------------------------------------------------

			// プリファレンス情報保持用のゲームオブジェクトを生成する
			if( PreferenceManager.Instance == null )
			{
				PreferenceManager.Create( transform ) ;
			}

			// ローカルプッシユ管理用のゲームオブジェクトを生成する
			if( LocalPushManager.Instance == null )
			{
				LocalPushManager.Create( transform ) ;
			}

			//----------------------------------------------------------
			// このタイミングのエンドポイント設定は保険の意味合い程度で実際はブートで上書きされる

			if( Define.DevelopmentMode == true )
			{
				// デバッグ機能のエンドポイントが設定されていたら使用する
				string endPointKey = "EndPoint" ;
				if( Preference.HasKey( endPointKey ) == true )
				{
					m_EndPoint = Preference.GetValue<string>( endPointKey ) ;
				}
			}

			if( ( string.IsNullOrEmpty( m_EndPoint ) == true || m_EndPoint == "http://localhost/" ) && settings != null )
			{
				var endPoints = settings.WebAPI_EndPoints ;
				if( endPoints != null && endPoints.Length >  0 )
				{
					m_EndPoint = endPoints[ ( int )settings.WebAPI_DefaultEndPoint ].Path ;	// デフォルトは開発サーバー
				}
			}

			// ダウンロードマネージャを生成する
			if( DownloadManager.Instance == null )
			{
				DownloadManager.Create( transform ) ;
			}

			//--------------------------------------------------------------------------

			// 他のマネージャ系が生成されていなければ生成する

			// 汎用シーンマネージャの生成
			if( EnhancedSceneManager.Instance == null )
			{
				EnhancedSceneManager.Create( transform ) ;
			}

			// 汎用インプットマネージャの生成
			if( InputManager.Instance == null )
			{
				InputManager.Create( transform ) ;
			}

			// 汎用アセットバンドルマネージャの生成
			if( AssetBundleManager.Instance == null )
			{
				AssetBundleManager.Create( transform ) ;
			}

			//----------------------------------------------------------

			//----------------------------------------------------------

			// プログレスを非表示にする
			Progress.Off() ;

			// 初期化が完了した
			m_IsInitialized = true ;

			//----------------------------------------------------------

#if USE_EXCEPTION_DIALOG

			// 注意:
			// UnityEditor環境で試験的にダイアログを表示するには以下のコードを使用する
			//
			// Debug.LogException( new Exception() ) ;

			// 例外ダイアログの設定(実機且つデバッグビルドのみ)
			if( settings.EnableExceptionDialog == true && Define.DevelopmentMode == true )
			{
				AddExceptionCallback() ;
			}
#endif
		}

#if USE_EXCEPTION_DIALOG

		// 例外発生時のコールバックを追加する
		private void AddExceptionCallback()
		{
			// メインスレッド用のフック
			Application.logMessageReceived -= OnExceptionOccurred ;
			Application.logMessageReceived += OnExceptionOccurred ;

			// サブスレッド用のフック
			Application.logMessageReceivedThreaded -= OnExceptionOccurred ;
			Application.logMessageReceivedThreaded += OnExceptionOccurred ;
		}

		/// <summary>
		/// 例外が発生した際に例外ダイアログを表示する(実機且つデバッグビルドのみ)
		/// </summary>
		/// <param name="condition"></param>
		/// <param name="stackTrace"></param>
		/// <param name="type"></param>
		private void OnExceptionOccurred( string condition, string stackTrace, LogType type )
		{
			if( type == LogType.Exception )
			{
				// 例外情報を追加する
				m_Exceptions.Add( ( condition, stackTrace ) ) ;
			}
		}

		// 例外情報スタック
		private List<( string, string )> m_Exceptions = new List<(string, string)>() ;

		// 例外情報ダイアログが開かれた状態かどうか
		private bool m_IsExceptionDialogOpening = false ;

		// 例外情報ダイアログを開く
		private async UniTask OpenExceptionDialog()
		{
			m_IsExceptionDialogOpening = true ;			// ダイアログが開かれている

			// 全ての例外を表示しきるまで繰り返す
			while( m_Exceptions.Count >  0 )
			{
				var exception = m_Exceptions[ 0 ] ;

				// 例外情報ダイアログを開く
				await Dialog.OpenException( "Exception", exception.Item1 + "\n" + exception.Item2, new string[]{ "CLOSE" } ) ;

				m_Exceptions.RemoveAt( 0 ) ;
			}

			m_IsExceptionDialogOpening = false ;	// ダイアログが閉じられている
		}

#endif
		//-------------------------------------------------------------------------------------------

#if USE_EXCEPTION_DIALOG
		internal void Update()
		{
			// サブスレッドで発生した例外もキャッチして表示したいため例外情報はリストにためてメインスレッドで表示する
			if( m_Exceptions.Count >  0 && m_IsExceptionDialogOpening == false )
			{
				// 例外発生情報ダイアログを開く
				_ = OpenExceptionDialog() ;
			}
		}
#endif

		//-----------------------------------------------------------------

		/// <summary>
		/// オーディオ関連のマネージャを生成する
		/// </summary>
		/// <returns></returns>
		private async UniTask CreateAudioManagers()
		{
			//----------------------------------------------------------

			bool audioRunInBackground ;

#if UNITY_EDITOR
			audioRunInBackground = RunInBackground ;
#else
			audioRunInBackground = false ;	// 実機ではバックグラウンドでは必ず止める
#endif

			// 汎用オーディオマネージャ(Unity-Audio)の生成
			if( AudioManager.Instance == null )
			{
				AudioManager.Create( transform, true, audioRunInBackground ) ;
			}

			await Yield() ;
		}

		// 常駐型のサブシーンをロードする
		private async UniTask LoadResidentSubScenes()
		{
			///  999
			if( OuterFrame.Instance == null )
			{
				// 外枠のシーンをロードする
				OuterFrame outerFrame = null ;
				await When( EnhancedSceneManager.AddAsync<OuterFrame>( "OuterFrame", ( _ ) => { outerFrame = _[ 0 ] ; } ) ) ;
				if( outerFrame != null )
				{
					DontDestroyOnLoad( outerFrame.gameObject ) ;
				}
			}

			//  990
			if( Blocker.Instance == null )
			{
				// 入力禁止用のシーンをロードする
				Blocker blocker = null ;
				await When( EnhancedSceneManager.AddAsync<Blocker>( "Blocker", ( _ ) => { blocker = _[ 0 ] ; } ) ) ;
				if( blocker != null )
				{
					DontDestroyOnLoad( blocker.gameObject ) ;
//					sceneMask.gameObject.SetActive( false ) ;
					Blocker.On( null, 0xFF000000 ) ;
				}
			}

			// 995
			if( Progress.Instance == null )
			{
				// プログレス演出用のシーンをロードする
				Progress progress = null ;
				await When( EnhancedSceneManager.AddAsync<Progress>( "Progress", ( _ ) => { progress = _[ 0 ] ; } ) ) ;
				if( progress != null )
				{
					DontDestroyOnLoad( progress.gameObject ) ;
					progress.gameObject.SetActive( false ) ;
				}
			}

			Blocker.Off() ;

			Progress.On( Progress.Styles.WebAPI, false, "準備中" ) ;

			//----------------------------------------------------------

			//  940
			if( Toast.Instance == null )
			{
				// トーストのシーンをロードする
				Toast toast = null ;
				await When( EnhancedSceneManager.AddAsync<Toast>( "Toast", ( _ ) => { toast = _[ 0 ] ; } ) ) ;
				if( toast != null )
				{
					DontDestroyOnLoad( toast.gameObject ) ;
					toast.gameObject.SetActive( false ) ;
				}
			}

			//  993
//			if( Movie.Instance == null )
//			{
//				// ムービー再生用のシーンをロードする
//				Movie movie = null ;
//				await When( EnhancedSceneManager.AddAsync<Movie>( "Movie", ( _ ) =>{ movie = _[ 0 ] ; } ) ) ;
//				if( movie != null )
//				{
//					DontDestroyOnLoad( movie.gameObject ) ;	// DontDestroyOnLoad は、完全にロードが終わった後に実行しないと、目的のコンポーネントのインスタンスが取得出来なくなってしまう。
//					movie.gameObject.SetActive( false ) ;
//				}
//			}

			//  994
			if( Fade.Instance == null )
			{
				// フェード演出用のシーンをロードする
				Fade fade = null ;
				await When( EnhancedSceneManager.AddAsync<Fade>( "Fade", ( _ ) => { fade = _[ 0 ] ; } ) ) ;
				if( fade != null )
				{
					DontDestroyOnLoad( fade.gameObject ) ;
					fade.gameObject.SetActive( false ) ;
				}
			}

			//  996
			if( Dialog.Instance == null )
			{
				// ダイアログ用のシーンをロードする
				Dialog dialog = null ;
				await When( EnhancedSceneManager.AddAsync<Dialog>( "Dialog", ( _ ) =>{ dialog = _[ 0 ] ; } ) ) ;
				if( dialog != null )
				{
					DontDestroyOnLoad( dialog.gameObject ) ;	// DontDestroyOnLoad は、完全にロードが終わった後に実行しないと、目的のコンポーネントのインスタンスが取得出来なくなってしまう。
					dialog.gameObject.SetActive( false ) ;
				}
			}

			//  998
			if( Ripple.Instance == null )
			{
				// タッチ演出用のシーンをロードする
				Ripple ripple = null ;
				await When( EnhancedSceneManager.AddAsync<Ripple>( "Ripple", ( _ ) => { ripple = _[ 0 ] ; } ) ) ;
				if( ripple != null )
				{
					DontDestroyOnLoad( ripple.gameObject ) ;
				}
			}

			// 1000
			if( Profile.Instance == null )
			{
				// プロファイルのシーンをロードする
				Profile profile = null ;
				await When( EnhancedSceneManager.AddAsync<Profile>( "Profile", ( _ ) => { profile = _[ 0 ] ; } ) ) ;
				if( profile != null )
				{
					DontDestroyOnLoad( profile.gameObject ) ;
				}
			}

			Progress.Off() ;
		}

		/// <summary>
		/// アプリケーション全体の仮想解像度を設定する
		/// </summary>
		/// <param name="width"></param>
		/// <param name=""></param>
		/// <returns></returns>
		public static bool SetResolution( float width, float height )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			// 確認用
			m_Instance.CanvasWidth  = width ;
			m_Instance.CanvasHeight = height ;

			   Blocker.SetCanvasResolution( width, height ) ;
			     Movie.SetCanvasResolution( width, height ) ;
			    Dialog.SetCanvasResolution( width, height ) ;
			      Fade.SetCanvasResolution( width, height ) ;
			  Progress.SetCanvasResolution( width, height ) ;
			    Ripple.SetCanvasResolution( width, height ) ;
			OuterFrame.SetCanvasResolution( width, height ) ;
			   Profile.SetCanvasResolution( width, height ) ;

			return true ;
		}

		//-----------------------------------------------------------------

		/// <summary>
		/// 設定をロードする
		/// </summary>
		public static Settings LoadSettings()
		{
			return Resources.Load<Settings>( "ScriptableObjects/Settings" ) ;
		}

		//-------------------------------------------------------------------------------------------

		// 常駐するタイプのアセットバンドル(シーンが変わっても維持)のバス群
		private readonly static string[] m_ResidentAssetBundlePaths =
		{
//			"Shaders",
		} ;

		/// <summary>
		/// アセットバンドルに追い出したシーン関連のアセットバンドルを展開する
		/// </summary>
		public static void LoadResidentAssetBundle()
		{
			Debug.Log( "[Load Resident AssetBundle]" ) ;

			if( m_ResidentAssetBundlePaths == null || m_ResidentAssetBundlePaths.Length == 0 )
			{
				return ;	// 対象は無し
			}

			//----------------------------------

			foreach( var path in m_ResidentAssetBundlePaths )
			{
				if( Asset.LoadAssetBundle( path, true ) == false )
				{
					Debug.LogError( "Could not load AssetBundle : " + path ) ;
				}
			}
		}

		/// <summary>
		/// アセットバンドルに追い出したシーン関連のアセットバンドルを破棄する
		/// </summary>
		public static void FreeAssetBundleOfScenes()
		{
			Debug.Log( "[Free Resident AssetBundle]" ) ;

			// シーン関連のアセットバンドルが展開中であれば破棄する
			foreach( var path in m_ResidentAssetBundlePaths )
			{
				Asset.FreeAssetBundle( path ) ;
			}
		}

		//-------------------------------------------------------------------------------------------

		// リブート前のクリーンアップ
		private static void Cleanup()
		{
			// BGMが再生されている可能性があるので止める
			BGM.StopMain( 0.5f ) ;

			//----------------------------------

			// アセットバンドルに追い出したシーン関連のアセットバンドルを破棄する
			FreeAssetBundleOfScenes() ;

			//----------------------------------

			// トーストを消す
			Toast.Hide() ;

			// ブロッカーを無効化する
			Blocker.Off() ;

			//----------------------------------

			// 開いている汎用ダイアログがあれば全て破棄する
			Dialog.CloseAll() ;
		}

		/// <summary>
		/// リブートを実行する
		/// </summary>
		public static void Reboot()
		{
			Cleanup() ;

			//----------------------------------

			// リブート対象シーンをロードする
			Scene.LoadWithFade( Scene.Screen.Reboot, blockingFadeIn:true ).Forget() ;
		}

		/// <summary>
		/// 外部ブラウザを開く
		/// </summary>
		/// <param name="url"></param>
		public static void OpenURL( string url )
		{
			Application.OpenURL( url ) ;
		}

		/// <summary>
		/// メーラーを起動する
		/// </summary>
		/// <param name="address"></param>
		/// <param name="title"></param>
		/// <param name="message"></param>
		public static void OpenMailer( string address, string title, string message )
		{
			if( string.IsNullOrEmpty( title ) == false )
			{
				title		= Uri.EscapeDataString( title	) ;
			}

			if( string.IsNullOrEmpty( message ) == false )
			{
				message	= Uri.EscapeDataString( message	) ;
			}

			Application.OpenURL( "mailto:" + address + "?subject=" + title + "&body=" + message ) ;
		}


		/// <summary>
		/// UnityEditor を一時停止する(UnityEditor専用)
		/// </summary>
		/// <param name="message"></param>
		public static void Pause( string message = null )
		{
			if( string.IsNullOrEmpty( message ) == false )
			{
				Debug.Log( "[PAUSE実行] " + message ) ;
			}

#if UNITY_EDITOR
			EditorApplication.isPaused = true ;
#endif
		}

		/// <summary>
		/// UnityEditor を動作再開する(UnityEditor専用)
		/// </summary>
		/// <param name="message"></param>
		public static void Unpause( string message = null )
		{
			if( string.IsNullOrEmpty( message ) == false )
			{
				Debug.Log( "[PAUSE解除] " + message ) ;
			}

#if UNITY_EDITOR
			EditorApplication.isPaused = false ;
#endif
		}

		/// <summary>
		/// UnityEditor がポーズ中かどうかを返す(UnityEditor専用)
		/// </summary>
		/// <returns></returns>
		public static bool IsPausing
		{
			get
			{
#if UNITY_EDITOR
				return EditorApplication.isPaused ;
#else
				return false ;
#endif
			}
		}

		/// <summary>
		/// モバイル環境かどうか判定する
		/// </summary>
		/// <returns></returns>
		public static bool IsMobile()
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return ( m_Instance.m_PlatformType == Settings.PlatformType.Mobile || Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer ) ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// LocalAssets が存在するか確認する
		/// </summary>
		/// <returns></returns>
		public static bool HasLocalAssets
		{
			get
			{
#if !UNITY_EDITOR
				return false ;
#else
				string path = "Assets/Application/AssetBundle/list_remote.txt" ;
				return File.Exists( path ) ;
#endif
			}
		}

		/// <summary>
		/// LocalAssets が存在するか確認する
		/// </summary>
		/// <returns></returns>
		public static bool HasLocalAssetBundle
		{
			get
			{
#if !UNITY_EDITOR
				return false ;
#else
				string path = "AssetBundle/" + Define.PlatformName ;
				return Directory.Exists( path ) ;
#endif
			}
		}

		/// <summary>
		/// StreamingAssets にアセットバンドルが存在するか確認する
		/// </summary>
		/// <param name="path"></param>
		/// <param name="onResult"></param>
		/// <returns></returns>
		public static AsyncState HasAssetBundleInStreamingAssets( string path, Action<bool> onResult )
		{
			if( m_Instance == null )
			{
				return null ;
			}

			AsyncState state = new AsyncState( m_Instance ) ;

			m_Instance.StartCoroutine( m_Instance.HasAssetBundleInStreamingAssets_Private( path, onResult, state ) ) ;
			return state ;
		}

		private IEnumerator HasAssetBundleInStreamingAssets_Private( string path, Action<bool> onResult, AsyncState state )
		{
			bool hasAssetBundle = false ;
			yield return StartCoroutine( StorageAccessor.ExistsInStreamingAssetsAsync( path, ( bool result ) =>
			{
				hasAssetBundle = result ;
			} ) ) ;

			onResult?.Invoke( hasAssetBundle ) ;

			state.IsDone = true ;
		}

		//-------------------------------------------------------------------------------------------
		// ストレージの空き容量チェック

		/// <summary>
		/// ストレージの空き容量をチェックして足りなければ警告ダイアログを出す
		/// </summary>
		/// <param name="needSize"></param>
		/// <returns></returns>
		public static async UniTask<bool> CheckStorage( long requiredSize )
		{
			long reserveSize = 10 * 1024 * 1024 ;	// 予備に 10MB

			// 空き容量を確認する(単位はMB)
			long freeSize = StorageMonitor.GetFree() ;
			long needSize = requiredSize + reserveSize ;	// 予備に 30MB 確保(ローカルへの情報保存分)

			if( freeSize <  needSize )
			{
				string sizeName = ExString.GetSizeName( needSize ) ;

				await Dialog.Open( "注意", "ストレージの空き容量が足りません\nあと <color=#FF7F00>" + sizeName + "</color> 必要です\nストレージの空き容量を確保してから\nアプリを起動するようお願い致します", new string[]{ "閉じる" } ) ;
				return false ;
			}

			// 空き容量は足りている
			return true ;
		}
	}
}
