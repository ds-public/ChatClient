// #define USE_MICROPHONE

using UnityEngine ;
using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine.SceneManagement ;

#if UNITY_EDITOR
using UnityEditor ;
#endif



// シングルトンはエディタモードに対応させてはダメ
// なぜならシングルトンを判定するスタティックなインスタンス変数がシリアライズ化出来ないため


/// <summary>
/// オーディオヘルパーハッケージ
/// </summary>
namespace AudioHelper
{
	/// <summary>
	/// オーディオ全般の管理クラス Version 2021/02/19 0
	/// </summary>
	public class AudioManager : MonoBehaviour
	{
#if UNITY_EDITOR
		/// <summary>
		/// AudioManager を生成
		/// </summary>
		[MenuItem("GameObject/Helper/AudioHelper/AudioManager", false, 24)]
		public static void CreateAudioManager()
		{
			GameObject go = new GameObject( "AudioManager" ) ;
		
			Transform t = go.transform ;
			t.SetParent( null ) ;
			t.localPosition = Vector3.zero ;
			t.localRotation = Quaternion.identity ;
			t.localScale = Vector3.one ;
		
			go.AddComponent<AudioManager>() ;
			Selection.activeGameObject = go ;
		}
#endif

		// オーディオマネージャのインスタンス(シングルトン)
		private static AudioManager m_Instance = null ; 

		/// <summary>
		/// オーディオマネージャのインスタンス
		/// </summary>
		public  static AudioManager   Instance
		{
			get
			{
				return m_Instance ;
			}
		}
	
		//---------------------------------------------------------
	
		// リスナー（Awake 時にクリアされるのでシリアライズ化して保持する意味が無い）
		private AudioListener m_Listener = null ;

		/// <summary>
		/// オーディオリスナーのインスタンス
		/// </summary>
		public  AudioListener   Listener{ get{ return m_Listener ; } }
	
		/// <summary>
		/// チャンネル情報
		/// </summary>
		public List<AudioChannel> Channels = new List<AudioChannel>() ;

		/// <summary>
		/// 最大チャンネル数
		/// </summary>
		public int Max = 32 ;
		
		//-----------------------------------------------------------

		// 発音毎に割り振られるユニークな識別子
		private int m_PlayId = 0 ;

		// １増加した発音毎に割り振られるユニークな識別子
		private int GetPlayId()
		{
			m_PlayId = ( m_PlayId + 1 ) & 0x7FFFFFFF ;
			return m_PlayId ;
		}


		/// <summary>
		/// スタートフェードとストップフェードのエフェクト
		/// </summary>
		internal protected class FadeEffect
		{
			/// <summary>
			/// フェード時間
			/// </summary>
			internal protected float Duration ;

			/// <summary>
			/// 基準時間
			/// </summary>
			internal protected float StartTime ;

			/// <summary>
			/// 開始音量
			/// </summary>
			internal protected float StartVolume ;

			/// <summary>
			/// 終了音量
			/// </summary>
			internal protected float EndVolume ;
		}
	
	
		// プレイフェード用
		private readonly Dictionary<AudioChannel,FadeEffect> m_FadePlayList = new Dictionary<AudioChannel, FadeEffect>() ;
		private readonly Dictionary<AudioChannel,FadeEffect> m_FadeStopList = new Dictionary<AudioChannel, FadeEffect>() ;
	
		/// <summary>
		/// マスターボリューム
		/// </summary>
		public float MasterVolume = 1.0f ;
	
		// ミュート
		private bool m_Mute = false ;
	
		// ミュートリスト
		private readonly List<string> m_MuteList = new List<string>() ;
	
		/// <summary>
		/// バックグラウンド再生を有効にするかどうか
		/// </summary>
		public bool RunInBackground = false ;


		//-----------------------------------------------------

#if USE_MICROPHONE
		// マイク関係
		private AudioSource			m_MicrophoneRecordingAudioSource	= null ;
		private AudioClip			m_MicrophoneRecordingAudioClip		= null ;
		private	int					m_MicrophoneRecordingFrequency		= 44100 ;
		private int					m_MicrophoneRecordingTime			= 1 ; 
		private float[]				m_MicrophoneRecordingBuffer			= null ;
		private int					m_MicrophoneRecordingPosition		= 0 ;
		private Action<float[],int>	m_MicrophoneCallback				= null ;
		private bool				m_MicrophoneCallbackDataSkip		= false ;
		private float[]				m_MicrophoneCallbackBuffer			= null ;
#endif

		//-----------------------------------------------------

		/// <summary>
		/// いずれかのチャンネルが停止された際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="playId">発音毎に割り当てられるユニークな識別子</param>
		public delegate void OnStopped( int playId ) ;

		/// <summary>
		/// いずれかのチャンネルが停止された際に呼び出されるデリゲート
		/// </summary>
		public OnStopped OnStoppedDelegate ;

		/// <summary>
		/// いずれかのチャンネルが停止された際に呼び出されるデリゲートを追加する
		/// </summary>
		/// <param name="onStoppedDelegate">追加するデリゲートメソッド</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool AddOnStopped( OnStopped onStoppedDelegate )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_Instance.OnStoppedDelegate += onStoppedDelegate ;

			return true ;
		}
		
		/// <summary>
		/// いずれかのチャンネルが停止された際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="onStoppedDelegate">削除するデリゲートメソッド</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool RemoveOnStopped( OnStopped onStoppedDelegate )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_Instance.OnStoppedDelegate -= onStoppedDelegate ;

			return true ;
		}

		//---------------------------------------------------------
	
		/// <summary>
		/// オーディオマネージャのインスタンスを生成する
		/// </summary>
		/// <param name="runInbackground">バックグラウンドで再生させるようにするかどうか</param>
		/// <returns>オーディオマネージャのインスタンス</returns>
		public static AudioManager Create( Transform parent = null, bool isListener = true, bool runInbackground = false )
		{
			if( m_Instance != null )
			{
				return m_Instance ;
			}
		
			// オブジェクトが非アクティブだと検出されないのでオブジェクトを非アクティブにしてはならない
			// この判定は必須で mInstance は static であるためシーンの最初はオブジェクトが存在しても null になっている
			m_Instance = GameObject.FindObjectOfType( typeof( AudioManager ) ) as AudioManager ;
			if( m_Instance == null )
			{
				GameObject go = new GameObject( "AudioManager" ) ;
				if( parent != null )
				{
					go.transform.SetParent( parent, false ) ;
				}

				go.AddComponent<AudioManager>() ;
			}

			m_Instance.RunInBackground	= runInbackground ;

			// リスナーの有効無効の設定
			m_Instance.m_Listener.enabled = isListener ;

			return m_Instance ;
		}
	
		/// <summary>
		/// オーディオマネージャのインスタンスを破棄する
		/// </summary>
		public static void Delete()
		{	
			if( m_Instance != null )
			{
				ClearAll() ;
			
				if( Application.isPlaying == false )
				{
					DestroyImmediate( m_Instance.gameObject ) ;
				}
				else
				{
					Destroy( m_Instance.gameObject ) ;
				}
			
				m_Instance = null ;
			}
		}
	
		//-----------------------------------------------------------------
	
		internal void Awake()
		{
			// 既に存在し重複になる場合は自身を削除する
			if( m_Instance != null )
			{
				GameObject.DestroyImmediate( gameObject ) ;
				return ;
			}
		
			AudioManager instanceOther = GameObject.FindObjectOfType( typeof( AudioManager ) ) as AudioManager ;
			if( instanceOther != null )
			{
				if( instanceOther != this )
				{
					GameObject.DestroyImmediate( gameObject ) ;
					return ;
				}
			}
		
			//-----------------------------
			// Awake 内でマニュアルで実行した場合とスクリプトで実行した場合の共通の処理を行う必要がある
		
			m_Instance = this ;
			
			// シーンがロードされた際に呼び出されるデリゲートを登録する
			SceneManager.sceneLoaded += OnLevelWasLoaded_Private ;	// MonoBehaviour の OnLevelWasLoaded メソッドは廃止予定

			// シーン切り替え時に破棄されないようにする(ただし自身がルートである場合のみ有効)
			if( transform.parent == null )
			{
				DontDestroyOnLoad( gameObject ) ;
			}
		
	//		gameObject.hideFlags = HideFlags.HideInHierarchy ;
		
			//-----------------------------
		
			// 原点じゃないと気持ち悪い
			gameObject.transform.localPosition = Vector3.zero ;
			gameObject.transform.localRotation = Quaternion.identity ;
			gameObject.transform.localScale = Vector3.one ;
		
			//-----------------------------
			
			// リスナーを張り付けておく
			CreateAudioListener( true ) ;
		
			// チャンネルリストをクリアする
			Channels.Clear() ;
		
			// ミュートリストをクリアする
			m_MuteList.Clear() ;
		
			// フェードプレイリストをクリアする
			m_FadePlayList.Clear() ;
		
			// フェードストップリストをクリアする
			m_FadeStopList.Clear() ;
		}

		// 何等かのシーンがロードされた際に呼び出される
		private void OnLevelWasLoaded_Private( Scene sceneName, LoadSceneMode sceneMode )
		{
			if( sceneMode == LoadSceneMode.Single )
			{
				bool isEnabled = true ;
				if( m_Listener != null )
				{
					isEnabled = m_Listener.enabled ;
				}

				CreateAudioListener( isEnabled ) ;
			}
		}

		// オーディオリスナーを生成する
		private bool CreateAudioListener( bool isEnabled )
		{
			if( m_Listener != null )
			{
				m_Listener.enabled = isEnabled ;
				return true ;	// 既に自身のオーディオリスナーを生成済みになっている
			}

			AudioListener listener = GameObject.FindObjectOfType( typeof( AudioListener ) ) as AudioListener ;

			GameObject go = new GameObject( "Listener" ) ;
			go.transform.SetParent( transform, false ) ;

			// 存在しない(自身のオーディオリスナーを生成する)
			m_Listener = go.AddComponent<AudioListener>() ;
			AudioListener.volume = 1.0f ;

			if( listener == null )
			{
				m_Listener.enabled = isEnabled ;

				return true ;
			}
			else
			{
				// 存在する
#if UNITY_EDITOR
				Debug.LogWarning( "[AudioManager] 既に AudioListener が存在します" ) ;
#endif
				m_Listener.enabled = false ;

				return false ;
			}
		}

		internal void Update()
		{
			// ソースを監視して停止している（予定）
		
			// 一定時間ごとにコールされる
			Process() ;

#if USE_MICROPHONE
			// マイクの録音
			ProcessMicrophoneRecording() ;
#endif
		}
	
		internal void OnDestroy()
		{
			if( m_Instance == this )
			{
				// シーンがロードされた際に呼び出されるデリゲートを削除する
				SceneManager.sceneLoaded -= OnLevelWasLoaded_Private ;	// MonoBehaviour の OnLevelWasLoaded メソッドは廃止予定

				m_Listener = null ;
				m_Instance  = null ;
			}
		}

		//-----------------------------------------------------------------
#if USE_MICROPHONE
		/// <summary>
		/// マイク入力を開始する
		/// </summary>
		/// <param name="rMicrophone"></param>
		/// <returns></returns>
		public static IEnumerator StartMicrophone( int recordingTime, int recordingFrequency, int bufferSize = 0, bool dataSkip = false, Action<float[],int> callback = null )
		{
			if( m_Instance == null )
			{
				yield break ;
			}

			yield return m_Instance.StartCoroutine( m_Instance.StartMicrophone_Private( recordingTime, recordingFrequency, bufferSize, dataSkip, callback ) ) ;
		}

		// マイク入力を開始する
		private IEnumerator StartMicrophone_Private( int recordingTime, int recordingFrequency, int bufferSize, bool dataSkip, Action<float[],int> callback )
		{
			if( Microphone.devices == null || Microphone.devices.Length == 0 )
			{
				// マイクが接続されていない模様
				yield break ;
			}

			//----------------------------------------------------------

			if( m_MicrophoneRecordingAudioSource == null )
			{
				GameObject go = new GameObject( "Microphone" ) ; 
				go.transform.SetParent( transform, false ) ;

				// AudioManager に直で AudioSource を Add してはダメ
				m_MicrophoneRecordingAudioSource = tGameObject.AddComponent<AudioSource>() ;
				m_MicrophoneRecordingAudioSource.playOnAwake = false ;
			}

			m_MicrophoneRecordingAudioSource.enabled = true ;

			m_MicrophoneRecordingTime		= recordingTime ;
			m_MicrophoneRecordingFrequency	= recordingFrequency ;

			AudioClip audioClip = Microphone.Start( null, true, m_MicrophoneRecordingTime, m_MicrophoneRecordingFrequency ) ;

			m_MicrophoneCallback				= callback ;
			if( callback != null && bufferSize >  0 )
			{
				m_MicrophoneRecordingAudioClip	= audioClip ;
				m_MicrophoneRecordingBuffer		= new float[ m_MicrophoneRecordingTime * m_MicrophoneRecordingFrequency ] ; 

				m_MicrophoneCallbackBuffer		= new float[ bufferSize ] ;

				m_MicrophoneCallbackDataSkip	= dataSkip ;
				m_MicrophoneRecordingPosition	= 0 ;
			}

			m_MicrophoneRecordingAudioSource.clip = audioClip ;

			// 準備が整うまで待つ
			yield return new WaitWhile( () => Microphone.GetPosition( null ) <= 0 ) ;

			m_MicrophoneRecordingAudioSource.timeSamples = Microphone.GetPosition( null ) ;
			m_MicrophoneRecordingAudioSource.loop = true ;
			m_MicrophoneRecordingAudioSource.Play() ;
		}

		/// <summary>
		/// マイク入力を停止する
		/// </summary>
		/// <param name="tMicrophone"></param>
		/// <returns></returns>
		public static bool StopMicrophone()
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.StopMicrophone_Private() ;
		}

		private bool StopMicrophone_Private()
		{
			if( m_MicrophoneRecordingAudioSource != null )
			{
				m_MicrophoneRecordingAudioSource.Stop() ;
				m_MicrophoneRecordingAudioSource.enabled = false ;
			}

			Microphone.End( null ) ;

			if( m_MicrophoneCallback != null )
			{
				m_MicrophoneCallbackBuffer		= null ;
				m_MicrophoneRecordingBuffer		= null ;
				m_MicrophoneRecordingAudioClip	= null ;

				m_MicrophoneCallback			= null ;
			}

			return true ;
		}

		/// <summary>
		/// マイクのオーディオソースを取得する
		/// </summary>
		/// <returns></returns>
		public static AudioSource GetMicrophone()
		{
			if( m_Instance == null )
			{
				return null ;
			}

			return m_Instance.m_MicrophoneRecordingAudioSource ;
		}

		// マイクの録音処理
		private void ProcessMicrophoneRecording()
		{
			if( m_MicrophoneRecordingAudioClip == null || m_MicrophoneRecordingBuffer == null || m_MicrophoneCallback == null || m_MicrophoneCallbackBuffer == null )
			{
				return ;
			}

			//----------------------------------------------------------

			int position = Microphone.GetPosition( null ) ;
			if( position <  0 || position == m_MicrophoneRecordingPosition )
			{
				return ;
			}

			int cb_Size = m_MicrophoneCallbackBuffer.Length ; 
			int rb_Size = m_MicrophoneRecordingBuffer.Length ;

			int size ;

			// サンプルデータをコピーする
			m_MicrophoneRecordingAudioClip.GetData( m_MicrophoneRecordingBuffer, 0 ) ;

			while( true )
			{
				if( m_MicrophoneRecordingPosition <  position )
				{
					// そのまま引き算で録音サイズ数が計算できる
					size = position - m_MicrophoneRecordingPosition ;
				}
				else
				{
					// バッファの反対側に回っているので若干計算が複雑になる
					size = ( rb_Size - m_MicrophoneRecordingPosition ) + position ;
				}
	
				if( size <  cb_Size )
				{
					// 必要量に足りない
					return ;
				}

				// レコーディングバッファのサンプルデータをコルーバックバッファにコピーする
				if( ( m_MicrophoneRecordingPosition + cb_Size ) <= rb_Size )
				{
					// １回でコピー可能
					Array.Copy( m_MicrophoneRecordingBuffer, m_MicrophoneRecordingPosition, m_MicrophoneCallbackBuffer, 0, cb_Size ) ;
				}
				else
				{
					// ２回でコピー可能
					int o = tRB_Size - m_MicrophoneRecordingPosition ;
					Array.Copy( m_MicrophoneRecordingBuffer, m_MicrophoneRecordingPosition, m_MicrophoneCallbackBuffer, 0, o ) ;
					Array.Copy( m_MicrophoneRecordingBuffer, 0,                             m_MicrophoneCallbackBuffer, 0, cb_Size - o ) ;
				}
				
				// コールバック実行
				m_MicrophoneCallback( m_MicrophoneCallbackBuffer, m_MicrophoneRecordingFrequency ) ;

				// 読み取り位置を変化させる
				if( m_MicrophoneCallbackDataSkip == false )
				{
					// データスキップ無し
					m_MicrophoneRecordingPosition = ( m_MicrophoneRecordingPosition + cb_Size ) % rb_Size ;
				}
				else
				{
					// データスキップ有り
					m_MicrophoneRecordingPosition = position ;
					return ;	// 終了
				}
			}
		}
#endif
		//-----------------------------------------------------------------

		// 以下スクリプトから利用するメソッド群

		/// <summary>
		/// リスナーの状態
		/// </summary>
		public static bool ListenerEnabled
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}

				return m_Instance.ListenerEnabled_Private ;
			}
			set
			{
				if( m_Instance == null )
				{
					return ;
				}

				m_Instance.ListenerEnabled_Private = value ;
			}
		}
		
		// リスナーの状態
		private bool ListenerEnabled_Private
		{
			get
			{
				if( m_Listener == null )
				{
					return false ;
				}

				return m_Listener.enabled ;
			}
			set
			{
				if( m_Listener == null )
				{
					return ;
				}

				m_Listener.enabled = value ;
			}
		}
		

		/// <summary>
		/// いずれかの空いているオーディオチャンネルを１つ取得する
		/// </summary>
		/// <returns>オーディオチャンネルのインスタンス</returns>
		public static AudioChannel GetChannel()
		{
			if( m_Instance == null )
			{
				return null ;
			}
		
			return m_Instance.GetChannel_Private() ;
		}
		
		// いずれかの空いているオーディオチャンネルを１つ取得する
		private AudioChannel GetChannel_Private()
		{
			int i, l = Channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Channels[ i ] != null && Channels[ i ].IsPlaying == false && Channels[ i ].IsPausing == false && Channels[ i ].IsLocking == false )
				{
					// 空いているチャンネルを発見した
					return Channels[ i ] ;
				}
			}
		
			if( l >= Max )
			{
				return null ;	// 最大チャンネルまで使用済み
			}
		
			// 空いているソースが無いので追加する
		
			AudioChannel audioChannel = new AudioChannel( gameObject.AddComponent<AudioSource>() ) ;
			Channels.Add( audioChannel ) ;
		
			return audioChannel ;
		}
	
		/// <summary>
		/// 使用中の全てのチャンネルの名前(再生中のオーディオクリップ名)を取得する
		/// </summary>
		/// <returns>使用中の全てのチャンネルのインスタンス</returns>
		public static string[] GetUsedChannel()
		{
			if( m_Instance == null )
			{
				return null ;
			}
		
			return m_Instance.GetUsedChannel_Private() ;
		}
	
		// 使用中の全てのチャンネルの名前(再生中のオーディオクリップ名)を取得する
		private string[] GetUsedChannel_Private()
		{
			List<string> list = new List<string>() ;
		
			int i, l = Channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Channels[ i ] != null )
				{
					if( Channels[ i ].IsPlaying == true || Channels[ i ].IsPausing == true || Channels[ i ].IsLocking == true )
					{
						// 使用中のチャンネルを発見した
						list.Add( Channels[ i ].Name ) ;
					}
				}
			}
		
			if( list.Count == 0 )
			{
				return null ;
			}
		
			return list.ToArray() ;
		}
		
		// 指定した識別子に該当する再生中のオーディオチャンネルを取得する
		private AudioChannel GetChannelByPlayId( int playId )
		{
			int i, l = Channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Channels[ i ] != null )
				{
					if( ( Channels[ i ].IsPlaying == true || Channels[ i ].IsPausing == true || Channels[ i ].IsLocking == true ) && Channels[ i ].PlayId == playId )
					{
						// 該当するチャンネルを発見した
						return Channels[ i ] ;
					}
				}
			}
			
			// 該当するチャンネルが存在しない1
			return null ;
		}

		//---------------------------------------------------------
	
		/// <summary>
		/// サウンドを再生する(リソースパスから)
		/// </summary>
		/// <param name="path">リソースパス</param>
		/// <param name="loop">ルーブの有無(true=有効・false=無効)</param>
		/// <param name="volume">ボリューム(0～1)</param>
		/// <param name="pan">パン(-1=左～0=中～+1=右)</param>
		/// <param name="pitch">ピッチ(-1=1オクターブ下～0=通常～+1=１オクターブ上)</param>
		/// <param name="tag">複数のチャンネルを同時に操作するための区別用のタグ名</param>
		/// <returns>発音毎に割り振られるユニークな識別子(-1で失敗)</returns>
		public static int Play( string path, bool loop = false, float volume = 1.0f, float pan = 0.0f, float pitch = 0.0f, string tag = "" )
		{
			if( m_Instance == null )
			{
				return -1 ;
			}
		
			AudioClip audioClip = Resources.Load( path ) as AudioClip ;
			if( audioClip == null )
			{
				return -1 ;
			}
		
			return m_Instance.Play_Private( audioClip, loop, volume, pan, pitch, tag ) ;
		}
		
		/// <summary>
		/// サウンドを再生する(オーディオクリップから)
		/// </summary>
		/// <param name="audioClip">オーディオクリップのインスタンス</param>
		/// <param name="loop">ルーブの有無(true=有効・false=無効)</param>
		/// <param name="volume">ボリューム(0～1)</param>
		/// <param name="pan">パン(-1=左～0=中～+1=右)</param>
		/// <param name="pitch">ピッチ(-1=1オクターブ下～0=通常～+1=１オクターブ上)</param>
		/// <param name="tag">複数のチャンネルを同時に操作するための区別用のタグ名</param>
		/// <returns>発音毎に割り振られるユニークな識別子(-1で失敗)</returns>
		public static int Play( AudioClip audioClip, bool loop = false, float volume = 1.0f, float pan = 0.0f, float pitch = 0.0f, string tag = "" )
		{
			if( m_Instance == null )
			{
				return -1 ;
			}
		
			return m_Instance.Play_Private( audioClip, loop, volume, pan, pitch, tag ) ;
		}
	
		//----------------
	
		// サウンドを再生する(オーディオクリップから)
		private int Play_Private( AudioClip audioClip, bool loop, float volume, float pan, float pitch, string tag )
		{
			AudioChannel audioChannel = GetChannel_Private() ;
			if( audioChannel == null )
			{
				// 空きがありません（古いやつから停止させるようにするかどうか）
				return -1 ;
			}
		
			//-----------------------------------------------------
			
			int playId = GetPlayId() ;

			audioChannel.Play( playId, audioClip, loop, volume, pan, pitch, tag ) ;
		
			if( string.IsNullOrEmpty( tag ) == false && m_MuteList.Contains( tag ) == true )
			{
				// ミュート対象のタグ
				audioChannel.Mute = true ;
			}
			
			// 成功したらソースのインスタンスを返す
			return playId ;
		}
	
		//---------------------------------
	
		/// <summary>
		/// フェード付きでサウンドを再生する(リソースパスから)
		/// </summary>
		/// <param name="path">リソースパス</param>
		/// <param name="duration">フェードの時間(秒)</param>
		/// <param name="loop">ルーブの有無(true=有効・false=無効)</param>
		/// <param name="volume">ボリューム(0～1)</param>
		/// <param name="pan">パンの位置(-1=左～0=中～+1=右)</param>
		/// <param name="pitch">ピッチ(-1=1オクターブ下～0=通常～+1=１オクターブ上</param>
		/// <param name="tag">複数のチャンネルを同時に操作するための区別用のタグ名</param>
		/// <returns>発音毎に割り振られるユニークな識別子(-1で失敗)</returns>
		public static int PlayFade( string path, float duration = 1.0f, bool loop = false, float volume = 1.0f, float pan = 0.0f, float pitch = 0.0f, string tag = "" )
		{
			if( m_Instance == null )
			{
				return -1 ;
			}
		
			AudioClip audioClip = Resources.Load( path ) as AudioClip ;
			if( audioClip == null )
			{
				return -1 ;
			}
		
			return m_Instance.PlayFade_Private( audioClip, duration, loop, volume, pan, pitch, tag ) ;
		}
	
		/// <summary>
		/// フェード付きでサウンドを再生する(オーディオクリップから)
		/// </summary>
		/// <param name="audioClip">オーディオクリップのインスタンス</param>
		/// <param name="duration">フェードの時間(秒)</param>
		/// <param name="loop">ルーブの有無(true=有効・false=無効)</param>
		/// <param name="volume">ボリューム(0～1)</param>
		/// <param name="pan">パンの位置(-1=左～0=中～+1=右)</param>
		/// <param name="pitch">ピッチ(-1=1オクターブ下～0=通常～+1=１オクターブ上</param>
		/// <param name="tag">複数のチャンネルを同時に操作するための区別用のタグ名</param>
		/// <returns>発音毎に割り振られるユニークな識別子(-1で失敗)</returns>
		public static int PlayFade( AudioClip audioClip, float duration = 1.0f, bool loop = false, float volume = 1.0f, float pan = 0.0f, float pitch = 0.0f, string tag = "" )
		{
			if( m_Instance == null )
			{
				return -1 ;
			}
		
			return m_Instance.PlayFade_Private( audioClip, duration, loop, volume, pan, pitch, tag ) ;
		}
	
		//--------------------
	
		// フェード付きでサウンドを再生する(オーディオクリップから)
		private int PlayFade_Private( AudioClip audioClip, float duration, bool loop, float volume, float pan, float pitch, string tag )
		{
			AudioChannel audioChannel = GetChannel_Private() ;
			if( audioChannel == null )
			{
				// 空きがありません（古いやつから停止させるようにするかどうか）
				return -1 ;
			}
		
			//-----------------------------------------------------
		
			// プレイフェードに登録する
		
			// 一旦破棄
			RemoveFadeList( audioChannel ) ;
		
			FadeEffect effect = new FadeEffect()
			{
				Duration	= duration,
				StartTime   = Time.realtimeSinceStartup,
				StartVolume = 0,
				EndVolume   = 1
			} ;
		
			m_FadePlayList.Add( audioChannel, effect ) ;
		
			// フェード処理中であるというロックをかける
			audioChannel.Lock() ;
		
			//-----------------------------------------------------
			
			int playId = GetPlayId() ;

			// 再生する
			audioChannel.Play( playId, audioClip, loop, volume, pan, pitch, tag ) ;
		
			if( string.IsNullOrEmpty( tag ) == false && m_MuteList.Contains( tag ) == true )
			{
				// ミュート対象のタグ
				audioChannel.Mute = true ;
			}
		
			// 成功したらソースのインスタンスを返す
			return playId ;
		}


		/// <summary>
		/// ３Ｄ空間想定でサウンドをワンショット再生する(リソースパスから)　※３Ｄ効果音用
		/// </summary>
		/// <param name="path">リソースパス</param>
		/// <param name="position">音源のワールド空間座標</param>
		/// <param name="listener">リスナーのトランスフォーム</param>
		/// <param name="scale">距離の係数</param>
		/// <param name="volume">ボリューム(0～1)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Play3D( string path, Vector3 position, Transform listener = null, float scale = 1, float volume = 1.0f )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			AudioClip audioClip = Resources.Load( path ) as AudioClip ;
			if( audioClip == null )
			{
				return false ;
			}
		
			return m_Instance.Play3D_Private( audioClip, position, listener, scale, volume ) ;
		}

		/// <summary>
		/// ３Ｄ空間想定でサウンドをワンショット再生する(オーディオクリップから)　※３Ｄ効果音用
		/// </summary>
		/// <param name="audioClip">オーディオクリップのインスタンス</param>
		/// <param name="position">音源のワールド空間座標</param>
		/// <param name="listener">リスナーのトランスフォーム</param>
		/// <param name="scale">距離の係数</param>
		/// <param name="volume">ボリューム(0～1)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Play3D( AudioClip audioClip, Vector3 position, Transform listener = null, float scale = 1, float volume = 1.0f )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.Play3D_Private( audioClip, position, listener, scale, volume ) ;
		}

		// ３Ｄ空間想定でサウンドをワンショット再生する(オーディオクリップから)　※３Ｄ効果音用
		private bool Play3D_Private( AudioClip audioClip, Vector3 position, Transform listener, float scale, float volume )
		{
			if( listener != null )
			{
				// リスナーのローカル座標系に変換する
				position = listener.InverseTransformPoint( position ) ;
			}

			position *= scale ;

			AudioSource.PlayClipAtPoint( audioClip, position, volume ) ;

			return true ;
		}

		//-----------------------------------------------------------



		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 指定された発音毎に割り振られるユニークな識別子で示される発音のオーディオクリップ名を取得する
		/// </summary>
		/// <param name="playId">発音毎に割り振られるユニークな識別子</param>
		/// <returns></returns>
		public static string GetName( int playId )
		{
			if( m_Instance == null )
			{
				return null ;
			}
		
			return m_Instance.GetName_Private( playId ) ;

		}

		// 指定された発音毎に割り振られるユニークな識別子で示される発音が継続しているかを取得する
		private string GetName_Private( int playId )
		{
			AudioChannel audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return null ;
			}

			//----------------------------------

			return audioChannel.Name ;
		}

		/// <summary>
		/// 再生中のオーディオクリップを取得する
		/// </summary>
		/// <param name="playId">発音識別子</param>
		/// <returns></returns>
		public static AudioClip GetClip( int playId )
		{
			if( m_Instance == null )
			{
				return null ;
			}
		
			return m_Instance.GetClip_Private( playId ) ;
		}
	
		//----------------
	
		// 再生中のオーディオクリップを取得する
		private AudioClip GetClip_Private( int playId )
		{
			AudioChannel audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return null ;
			}

			//-----------------------------------------------------
			
			return audioChannel.AudioClip ;
		}

		/// <summary>
		/// 指定された発音毎に割り振られるユニークな識別子で示される発音が継続しているかを取得する
		/// </summary>
		/// <param name="playId">発音毎に割り振られるユニークな識別子</param>
		/// <returns></returns>
		public static bool IsRunning( int playId )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.IsRunning_Private( playId ) ;

		}

		// 指定された発音毎に割り振られるユニークな識別子で示される発音が継続しているかを取得する
		private bool IsRunning_Private( int playId )
		{
			AudioChannel audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return false ;
			}

			//----------------------------------

			return audioChannel.IsRunning ;
		}
		

		/// <summary>
		/// 指定された発音毎に割り振られるユニークな識別子で示される発音が継続しているかを取得する
		/// </summary>
		/// <param name="playId">発音毎に割り振られるユニークな識別子</param>
		/// <returns></returns>
		public static bool IsPlaying( int playId )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.IsPlaying_Private( playId ) ;

		}

		// 指定された発音毎に割り振られるユニークな識別子で示される発音が継続しているかを取得する
		private bool IsPlaying_Private( int playId )
		{
			AudioChannel audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return false ;
			}

			//----------------------------------

			return audioChannel.IsPlaying ;
		}

		/// <summary>
		/// 指定された発音毎に割り振られるユニークな識別子で示される発音が継続しているかを取得する
		/// </summary>
		/// <param name="playId">発音毎に割り振られるユニークな識別子</param>
		/// <returns></returns>
		public static bool IsPausing( int playId )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.IsPausing_Private( playId ) ;

		}

		// 指定された発音毎に割り振られるユニークな識別子で示される発音が継続しているかを取得する
		private bool IsPausing_Private( int playId )
		{
			AudioChannel audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return false ;
			}

			//----------------------------------

			return audioChannel.IsPausing ;
		}

		/// <summary>
		/// 指定された発音毎に割り振られるユニークな識別子で示される発音が継続しているかを取得する
		/// </summary>
		/// <param name="playId">発音毎に割り振られるユニークな識別子</param>
		/// <returns></returns>
		public static bool IsUsing( int playId )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.IsUsing_Private( playId ) ;

		}

		// 指定された発音毎に割り振られるユニークな識別子で示される発音が継続しているかを取得する
		private bool IsUsing_Private( int playId )
		{
			AudioChannel audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return false ;
			}

			//----------------------------------

			if( audioChannel.IsPlaying == true || audioChannel.IsPausing == true || audioChannel.IsLocking == true )
			{
				return true ;
			}

			return false ;
		}
	
		/// <summary>
		/// オーディオチャンネルごとのミュート状態を設定する
		/// </summary>
		/// <param name="playId">発音毎に割り振られるユニークな識別子</param>
		/// <param name="state">ミュート状態(true=オン・false=オフ)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Mute( int playId, bool state )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.Mute_Private( playId, state ) ;
		}
		
		// オーディオチャンネルごとのミュート状態を設定する
		private bool Mute_Private( int playId, bool state )
		{
			AudioChannel audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return false ;
			}

			//----------------------------------

			audioChannel.Mute = state ;	// 完全停止
				
			return true ;	// 発見
		}
		
		/// <summary>
		/// オーディオチャンネルごとにサウンドを完全停止させる
		/// </summary>
		/// <param name="playId">発音毎に割り振られるユニークな識別子</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Stop( int playId )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.Stop_Private( playId ) ;
		}
		
		// オーディオチャンネルごとにサウンドを完全停止させる
		private bool Stop_Private( int playId )
		{
			AudioChannel audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return false ;
			}

			//----------------------------------

			audioChannel.Stop() ;	// 完全停止
				
			// フェードリストから除外する
			RemoveFadeList( audioChannel ) ;
					
			// 即時呼び出す
			if( audioChannel.callback == true )
			{
				OnStoppedDelegate?.Invoke( playId ) ;
				audioChannel.callback = false ;
			}

			return true ;	// 成功
		}

		/// <summary>
		/// オーディオチャンネルごとにフェードでサウンドを完全停止させる
		/// </summary>
		/// <param name="playId">発音毎に割り振られるユニークな識別子</param>
		/// <param name="duration">フェード時間(秒)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool StopFade( int playId, float duration = 1.0f )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return  m_Instance.StopFade_Private( playId, duration ) ;
		}
	
		// オーディオチャンネルごとにフェードでサウンドを完全停止させる
		private bool StopFade_Private( int playId, float duration )
		{
			AudioChannel audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return false ;
			}

			//----------------------------------

			// 再生中のチャンネルを発見した（ただし既にフェード対象に対する多重の効果はかけられない）
				
			if( audioChannel.IsPlaying == true && m_FadeStopList.ContainsKey( audioChannel ) == false )
			{
				// 一旦破棄
				RemoveFadeList( audioChannel ) ;	// 多重実行は禁止したが保険
					
				FadeEffect effect = new FadeEffect()
				{
					Duration	= duration,
					StartTime	= Time.realtimeSinceStartup,
					StartVolume	= 1,
					EndVolume   = 0
				} ;
					
				m_FadeStopList.Add( audioChannel, effect ) ;
					
				// フェード処理中なのでロックする
				audioChannel.Lock() ;
					
				return true ;   // 発見
			}
			else
			{
				// ポーズ中であれば即時停止させる
				audioChannel.Stop() ;
					
				// リストから除外する
				RemoveFadeList( audioChannel ) ;

				// 即時呼び出す
				if( audioChannel.callback == true )
				{
					OnStoppedDelegate?.Invoke( playId ) ;
					audioChannel.callback = false ;
				}
			}
		
			return false ;
		}
	
	
		/// <summary>
		/// オーディオチャンネルごとにサウンドを一時停止させる
		/// </summary>
		/// <param name="playId">発音毎に割り振られるユニークな識別子</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Pause( int playId )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.Pause_Private( playId ) ;
		}
		
		// オーディオチャンネルごとにでサウンドを一時停止させる
		private bool Pause_Private( int playId )
		{
			AudioChannel audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return false ;
			}

			//----------------------------------

			// 再生中のソースを発見した
				
			if( audioChannel.IsPlaying == true )
			{
				// ポーズ状態になるのでベースタイムとベースボリュームを更新する
				PauseFadeList( audioChannel ) ;
					
				audioChannel.Pause() ;	// 一時停止
			}
			
			return true ;
		}
	
		/// <summary>
		/// オーディオチャンネルごとに一時停止を解除させる
		/// </summary>
		/// <param name="playId">発音毎に割り振られるユニークな識別子</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Unpause( int playId )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.Unpause_Private( playId ) ;
		}
		
		// オーディオチャンネルごとに一時停止を解除させる
		private bool Unpause_Private( int playId )
		{
			AudioChannel audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return false ;
			}

			//----------------------------------

			// 再生中のソースを発見した
				
			// 注意：AudioChannel では、終了中なのか中断中なのか区別がつかない
			if( audioChannel.IsPausing == true )
			{
				// ポーズ状態になるのでベースタイムとベースボリュームを更新する
				UnpauseFadeList( audioChannel ) ;
					
				audioChannel.Unpause() ;	// 再開
			}
			else
			{
				// ポーズ中でなく普通に終わっているだけなら再生する
				audioChannel.Unpause() ;
			}
				
			return true ;	// 発見
		}
		
		//-----------------------------------------------------------

		/// <summary>
		/// 再生中の位置(秒)を取得する
		/// </summary>
		/// <param name="playId">発音識別子</param>
		/// <returns></returns>
		public static float GetTime( int playId )
		{
			if( m_Instance == null )
			{
				return 0 ;
			}
		
			return m_Instance.GetTime_Private( playId ) ;
		}
	
		// 再生中の位置(秒)を取得する
		private float GetTime_Private( int playId )
		{
			AudioChannel audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return 0 ;
			}

			//-----------------------------------------------------
			
			return audioChannel.Time ;
		}

		/// <summary>
		/// 再生中の位置(サンプル数)を取得する
		/// </summary>
		/// <param name="playId">発音識別子</param>
		/// <returns></returns>
		public static int GetTimeSamples( int playId )
		{
			if( m_Instance == null )
			{
				return 0 ;
			}
		
			return m_Instance.GetTimeSamples_Private( playId ) ;
		}
	
		// 再生中の位置(秒)を取得する
		private int GetTimeSamples_Private( int playId )
		{
			AudioChannel audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return 0 ;
			}

			//-----------------------------------------------------
			
			return audioChannel.TimeSamples ;
		}

		//-----------------------------------------------------------
#if false
		/// <summary>
		/// オーディオチャンネルごとの現在の状態を取得する
		/// </summary>
		/// <param name="audioChannel">オーディオチャンネルのインスタンス</param>
		/// <returns>現在の状態(-1=不正な引数・0=一時停止か完全停止・1=再生</returns>
		public static int GetStatus( AudioChannel audioChannel )
		{
			if( audioChannel == null )
			{
				return -1 ;
			}
		
			if( audioChannel.IsPlaying() == true )
			{
				return 1 ;	// 再生中
			}
		
			return 0 ;	// 停止中か一時停止中
		}
#endif
		//---------------------------------------------------------
	
		// 個別に停止したい場合は再生時に取得したオーディオソースに対して Stop() を実行する
	
		// オーディオに関しては実装が不完全と言える
		// 音自体の再生停止は runInBackground とは別に管理しなくてはならない（別に管理出来るようにする）
		// （あまり使い道は無いかもしれないがゲームが止まっても音は流し続ける事も可能）
		// しかしフェード効果は Update で処理しているためバックグウンド時はフェード効果が正しく処理されなくなってしまう
	
		// サスペンド・レジューム
		internal void OnApplicationPause( bool state )
		{
			if( state == true )
			{
				// サスペンド
				SuspendAll_Private() ;
			}
			else
			{
				// レジューム
				ResumeAll_Private() ;
			}
		}	
	
		// フォーカス
		internal void OnApplicationFocus( bool state )
		{
			if( state == false )
			{
				// サスペンド
				SuspendAll_Private() ;
			}
			else
			{
				// レジューム
				ResumeAll_Private() ;
			}
		}
	
		// 全サスペンド
		private bool SuspendAll_Private()
		{
			// マネージャを生成するのは一ヶ所だけなのでひとまず Application.runInBackground とは独立管理出来るようにしておく
			if( RunInBackground == true )
			{
				return true ;
			}
		
			int i, l = Channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Channels[ i ] != null )
				{
					if( Channels[ i ].IsPlaying == true )
					{
						// ポーズ状態になるのでベースタイムとベースボリュームを更新する
						PauseFadeList( Channels[ i ] ) ;
					
						Channels[ i ].Pause() ;	// 一時停止
					
						Channels[ i ].Suspend() ;
					}
				}
			}
		
			return true ;
		}
	
		// 全レジューム
		private bool ResumeAll_Private()
		{
			// マネージャを生成するのは一ヶ所だけなのでひとまず Application.runInBackground とは独立管理出来るようにしておく
			if( RunInBackground == true )
			{
				return true ;
			}
		
			int i, l = Channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Channels[ i ] != null )
				{
					if( Channels[ i ].IsPausing == true && Channels[ i ].IsSuspending == true )
					{
						// リプレイ状態になるのでベースタイムとベースボリュームを更新する
						UnpauseFadeList( Channels[ i ] ) ;
					
						Channels[ i ].Unpause() ;	// 一時停止解除
					
						Channels[ i ].Resume() ;
					}
				}
			}
		
			return true ;
		}
		
		//---------------------------------------------------------------------------
	
		/// <summary>
		/// 全オーディオチャンネルのミュート状態を設定する
		/// </summary>
		/// <param name="state">ミュート状態(true=オン・false=オフ)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool MuteAll( bool state )
		{
			return MuteWithTag( state, null ) ;
		}

		/// <summary>
		/// タグで指定されたオーディオチャンネルのミュート状態を設定する
		/// </summary>
		/// <param name="state">ミュート状態(true=オン・false=オフ)</param>
		/// <param name="tag">タグ名</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool MuteWithTag( bool state, string tag )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.MuteWithTag_Private( state, tag ) ;
		}
		
		// タグで指定されたオーディオチャンネルのミュート状態を設定する
		private bool MuteWithTag_Private( bool state, string tag )
		{
			int i, l = Channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Channels[ i ] != null )
				{
					if( Channels[ i ].IsPlaying == true )
					{
						if( string.IsNullOrEmpty( tag ) == true || ( string.IsNullOrEmpty( tag ) == false && Channels[ i ].Tag == tag ) )
						{
							Channels[ i ].Mute = state ;	// ミュート設定
						}
					}
				}
			}
		
			// タグ指定でミュートリストに保存する
			if( string.IsNullOrEmpty( tag ) == false )
			{
				if( state == true )
				{
					// オン
					if( m_MuteList.Contains( tag ) == false )
					{
						m_MuteList.Add( tag ) ;
					}
				}
				else
				{
					// オフ
					if( m_MuteList.Contains( tag ) == true )
					{
						m_MuteList.Remove( tag ) ;
					}
				}
			}
		
			return true ;
		}
		
		/// <summary>
		/// 全オーディオチャンネルを完全停止させる
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool StopAll()
		{
			return StopAll( null ) ;
		}

		/// <summary>
		/// タグで指定されたオーディオチャンネルを完全停止させる
		/// </summary>
		/// <param name="tag">タグ名</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool StopAll( string tag )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.StopAll_Private( tag ) ;
		}
		
		// タグで指定されたオーディオチャンネルを完全停止させる
		private bool StopAll_Private( string tag )
		{
			AudioChannel audioChannel ;
			int i, l = Channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Channels[ i ] != null )
				{
					audioChannel = Channels[ i ] ;
					if( string.IsNullOrEmpty( tag ) == true || ( string.IsNullOrEmpty( tag ) == false && audioChannel.Tag == tag ) )
					{
						// 再生中のチャンネルを発見した
						audioChannel.Stop() ;
					
						// フェードリストから除外する
						RemoveFadeList( audioChannel ) ;

						// 即時呼び出す
						if( audioChannel.callback == true )
						{
							OnStoppedDelegate?.Invoke( audioChannel.PlayId ) ;
							audioChannel.callback = false ;
						}
					}
				}
			}
		
			return true ;
		}

		/// <summary>
		/// 全オーディオチャンネルを一時停止させる
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool PauseAll()
		{
			return PauseAll( null ) ;
		}

		/// <summary>
		/// タグで指定されたオーディオチャンネルを一時停止させる
		/// </summary>
		/// <param name="tag">タグ名</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool PauseAll( string tag )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.PauseAll_Private( tag ) ;
		}
		
		// タグで指定されたオーディオチャンネルを一時停止させる
		private bool PauseAll_Private( string tag )
		{
			int i, l = Channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Channels[ i ] != null )
				{
					if( Channels[ i ].IsPlaying == true )
					{
						if( string.IsNullOrEmpty( tag ) == true || ( string.IsNullOrEmpty( tag ) == false && Channels[ i ].Tag == tag ) )
						{
							// ポーズ状態になるのでベースタイムとベースボリュームを更新する
							PauseFadeList( Channels[ i ] ) ;
						
							Channels[ i ].Pause() ;	// 一時停止
						}
					}
				}
			}
		
			return true ;
		}
		
		/// <summary>
		/// 全オーディオチャンネルの一時停止を解除させる
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool UnpauseAll()
		{
			return UnpauseAll( null ) ;
		}

		/// <summary>
		/// タグで指定されたオーディオチャンネルの一時停止を解除させる
		/// </summary>
		/// <param name="tag">タグ名</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool UnpauseAll( string tag )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.UnpauseAll_Private( tag ) ;
		}
		
		// タグで指定されたオーディオチャンネルの一時停止を解除させる
		private bool UnpauseAll_Private( string tag )
		{
			int i, l = Channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Channels[ i ] != null )
				{
					if( Channels[ i ].IsPlaying == false && Channels[ i ].IsRunning == true )
					{
						if( string.IsNullOrEmpty( tag ) == true || ( string.IsNullOrEmpty( tag ) == false && Channels[ i ].Tag == tag ) )
						{
							// ポーズ解除状態になるのでベースタイムとベースボリュームを更新する
							UnpauseFadeList( Channels[ i ] ) ;
							
							Channels[ i ].Unpause() ;	// 再開
						}
					}
				}
			}
		
			return true ;
		}

		/// <summary>
		/// 全オーディオチャンネルを完全にクリアする
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool ClearAll()
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.ClearAll_Private() ;
		}
		
		// 全オーディオチャンネルを完全にクリアする
		private bool ClearAll_Private()
		{
			StopAll_Private( null ) ;
		
			int i, l = Channels.Count ;
			for( i  = l - 1 ; i >= 0 ; i -- )
			{
				if( Channels[ i ] != null )
				{
					Channels[ i ].Destroy() ;
				}
			}
		
			m_FadeStopList.Clear() ;
		
			m_FadePlayList.Clear() ;
		
			m_MuteList.Clear() ;
		
			Channels.Clear() ;
		
			return true ;
		}
	
		// --------------------------------------------------------
	
		/// <summary>
		/// 全体共通のミュート状態を設定する
		/// </summary>
		/// <param name="state">全体共通のミュート状態(true=オン・false=オフ)</param>
		public static void Mute( bool state )
		{
			if( m_Instance == null )
			{
				return ;
			}
		
			m_Instance.m_Mute = state ;
		
			if( m_Instance.m_Mute == false )
			{
				AudioListener.volume = m_Instance.MasterVolume ;
			}
			else
			{
				AudioListener.volume = 0 ;
			}
		}
	
		/// <summary>
		/// マスターボリュームを設定する
		/// </summary>
		/// <param name="volume">マスターボリューム(0～1)</param>
		public static void SetMasterVolume( float volume )
		{
			if( m_Instance == null )
			{
				return ;
			}
		
			if( volume >  1 )
			{
				volume  = 1 ;
			}
			else
			if( volume <  0 )
			{
				volume  = 0 ;
			}
		
			m_Instance.MasterVolume = volume ;
		
			if( m_Instance.m_Mute == false )
			{
				AudioListener.volume = m_Instance.MasterVolume ;
			}
		}
	
		/// <summary>
		/// 特定チャンネルのボリュームを設定する
		/// </summary>
		/// <param name="volume">発音識別子</param>
		/// <param name="volume">マスターボリューム(0～1)</param>
		public static void SetVolume( int playId, float volume )
		{
			if( m_Instance == null )
			{
				return ;
			}
		
			if( volume >  1 )
			{
				volume  = 1 ;
			}
			else
			if( volume <  0 )
			{
				volume  = 0 ;
			}
			
			AudioChannel audioChannel = m_Instance.GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return ;
			}

			audioChannel.Volume = volume ;
		}

		/// <summary>
		/// 全オーディオチャンネルのボリュームを設定する
		/// </summary>
		/// <param name="volume">ボリューム(0～1)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool SetAllVolumes( float volume )
		{
			return SetAllVolumes( volume, null ) ;
		}

		/// <summary>
		/// タグで指定されたオーディオチャンネルのボリュームを設定する
		/// </summary>
		/// <param name="volume">ボリューム(0～1)</param>
		/// <param name="tag">タグ名</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool SetAllVolumes( float volume, string tag )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.SetAllVolumes_Private( volume, tag ) ;
		}
		
		// タグで指定されたオーディオチャンネルのボリュームを設定する
		private bool SetAllVolumes_Private( float volume, string tag )
		{
			int i, l = Channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Channels[ i ] != null )
				{
					if( string.IsNullOrEmpty( tag ) == true || ( string.IsNullOrEmpty( tag ) == false && Channels[ i ].Tag == tag ) )
					{
						// ポーズ状態になるのでベースタイムとベースボリュームを更新する
						Channels[ i ].Volume = volume ;	// 一時停止
					}
				}
			}
		
			return true ;
		}
	
		//---------------------------------------------------------
		
		// フェードを処理する
		private void Process()
		{
			// 現在の時間を取得する
			float currentTime = Time.realtimeSinceStartup ;
		
			//-----------------------------------------------------
		
			// プレイフェードを処理する
		
			if( m_FadePlayList.Count >  0 )
			{
				int i, l = m_FadePlayList.Count ;
				AudioChannel[] audioChannelList = new AudioChannel[ l ] ;
			
				m_FadePlayList.Keys.CopyTo( audioChannelList, 0 ) ;
			
				AudioChannel audioChannel ;
				float deltaTime ;
			
				for( i  = 0 ; i <  l ; i ++ )
				{
					audioChannel = audioChannelList[ i ] ;
				
					if( audioChannel.IsPlaying == true )
					{
						// 再生中のみ処理する
					
						deltaTime = currentTime - m_FadePlayList[ audioChannel ].StartTime ;
						if( deltaTime <  m_FadePlayList[ audioChannel ].Duration )
						{
							// まだ続く
							audioChannel.FadeVolume = m_FadePlayList[ audioChannel ].StartVolume + ( ( m_FadePlayList[ audioChannel ].EndVolume - m_FadePlayList[ audioChannel ].StartVolume ) * deltaTime / m_FadePlayList[ audioChannel ].Duration ) ;
						}
						else
						{
							// 終了
							audioChannel.FadeVolume = m_FadePlayList[ audioChannel ].EndVolume ;
						
							// リストから除外する
							m_FadePlayList.Remove( audioChannel ) ;
						
				//			Debug.Log( "フェードプレイが終わったので破棄:" + mFadePlayList.Count ) ;
						}
					}
				}
			}
		
			//-----------------------------
		
			// ストップフェードを処理する
		
			if( m_FadeStopList.Count >  0 )
			{
				int i, l = m_FadeStopList.Count ;
				AudioChannel[] audioChannelList = new AudioChannel[ l ] ;
			
				m_FadeStopList.Keys.CopyTo( audioChannelList, 0 ) ;
			
				AudioChannel audioChannel ;
				float deltaTime ;
			
				for( i  = 0 ; i <  l ; i ++ )
				{
					audioChannel = audioChannelList[ i ] ;
				
					if( audioChannel.IsPlaying == true )
					{
						// 再生中のみ処理する
					
						deltaTime = currentTime - m_FadeStopList[ audioChannel ].StartTime ;
						if( deltaTime <  m_FadeStopList[ audioChannel ].Duration )
						{
							// まだ続く
							audioChannel.FadeVolume = m_FadeStopList[ audioChannel ].StartVolume + ( ( m_FadeStopList[ audioChannel ].EndVolume - m_FadeStopList[ audioChannel ].StartVolume ) * deltaTime / m_FadeStopList[ audioChannel ].Duration ) ;
						}
						else
						{
							// 終了
						
							// 再生も停止する
							audioChannel.Stop() ;
						
							audioChannel.FadeVolume = m_FadeStopList[ audioChannel ].EndVolume ;
						
							// リストから除外する
							m_FadeStopList.Remove( audioChannel ) ;
						
				//			Debug.Log( "フェードストップが終わったので破棄:" + mFadeStopList.Count ) ;
						}
					}
				}
			}

			//----------------------------------------------------------

			// 終了したチャンネルを監視してコールバックを発生させる
			if( OnStoppedDelegate != null )
			{
				AudioChannel audioChannel ;
				int i, l = Channels.Count ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( Channels[ i ] != null )
					{
						audioChannel = Channels[ i ] ;
						if( audioChannel.callback == true && audioChannel.IsPlaying == false && audioChannel.IsPausing == false )
						{
							OnStoppedDelegate( audioChannel.PlayId ) ;
							audioChannel.callback = false ;
						}
					}
				}
			}
		}
	
		// ポーズ状態になるのでベースタイムとベースボリュームを更新する
		private void PauseFadeList( AudioChannel audioChannel )
		{
			float currentTime = Time.realtimeSinceStartup ;
			float deltaTime ;
		
			// プレイリスト
			if( m_FadePlayList.ContainsKey( audioChannel ) == true )
			{
				// 対象のソース
				deltaTime = currentTime - m_FadePlayList[ audioChannel ].StartTime ;
			
				if( deltaTime <  m_FadePlayList[ audioChannel ].Duration )
				{
					// まだ続く
					m_FadePlayList[ audioChannel ].Duration   = m_FadePlayList[ audioChannel ].Duration - deltaTime ;
				
					m_FadePlayList[ audioChannel ].StartTime   = currentTime ;
					m_FadePlayList[ audioChannel ].StartVolume = audioChannel.Volume ;
				}
				else
				{
					// 終了
					audioChannel.FadeVolume = m_FadePlayList[ audioChannel ].EndVolume ;	// 最終的なボリューム
					m_FadePlayList.Remove( audioChannel ) ;	// リストから破棄
				}
			}
		
			// ストップリスト
			if( m_FadeStopList.ContainsKey( audioChannel ) == true )
			{
				// 対象のソース
				deltaTime = currentTime - m_FadeStopList[ audioChannel ].StartTime ;
			
				if( deltaTime <  m_FadeStopList[ audioChannel ].Duration )
				{
					// まだ続く
					m_FadeStopList[ audioChannel ].Duration   = m_FadeStopList[ audioChannel ].Duration - deltaTime ;
				
					m_FadeStopList[ audioChannel ].StartTime   = currentTime ;
					m_FadeStopList[ audioChannel ].StartVolume = audioChannel.Volume ;
				}
				else
				{
					// 終了
					audioChannel.FadeVolume = m_FadeStopList[ audioChannel ].EndVolume ;	// 最終的なボリューム（ストップの場合は０）
				
					m_FadeStopList.Remove( audioChannel ) ;
				
					// ここに来る事はあまり考えられないが念のため
					audioChannel.Stop() ;
				}
			}
		}
	
		// リプレイ状態になるのでベースタイムとベースボリュームを更新する
		private void UnpauseFadeList( AudioChannel audioChannel )
		{
			float currentTime = Time.realtimeSinceStartup ;
		
			// プレイリスト
			if( m_FadePlayList.ContainsKey( audioChannel ) == true )
			{
				// 対象のソース
				m_FadePlayList[ audioChannel ].StartTime   = currentTime ;
			}
		
			// ストップリスト
			if( m_FadeStopList.ContainsKey( audioChannel ) == true )
			{
				// 対象のソース
				m_FadeStopList[ audioChannel ].StartTime   = currentTime ;
			}
		}
		
		// フェードリストから除外する
		private void RemoveFadeList( AudioChannel audioChannel )
		{
			if( m_FadePlayList.ContainsKey( audioChannel ) == true )
			{
				m_FadePlayList.Remove( audioChannel ) ;
			}
						
			if( m_FadeStopList.ContainsKey( audioChannel ) == true )
			{
				m_FadeStopList.Remove( audioChannel ) ;
			}
		}
	}

	//--------------------------------------------------------------------------------------------

	// AudioChannel

	/// <summary>
	/// オーディオチャンネルクラス(オーディオソースの機能拡張クラス)
	/// </summary>
	[Serializable]
	public class AudioChannel
	{
		/// <summary>
		/// オーディオクリップの名称
		/// </summary>
		public string Name ;
		
		/// <summary>
		/// 発音中の音のユニークな識別子
		/// </summary>
		public int PlayId ;

		/// <summary>
		/// ボリーム
		/// </summary>
		private float m_Volume = 1.0f ;

		/// <summary>
		/// フェードボリューム
		/// </summary>
		private float m_FadeVolume = 1.0f ;

		/// <summary>
		/// タグ名
		/// </summary>
		public string Tag ;	// タグ
	
		//---------------------------------
		
		// オーディオソースのインスタンス
		private AudioSource m_AudioSource ;
		
		/// <summary>
		/// ポーズ状態
		/// </summary>
		private bool m_IsPausing ;
		
		/// <summary>
		/// ロック状態
		/// </summary>
		private bool m_IsLocking ;

		/// <summary>
		/// サスペンド状態
		/// </summary>
		private bool m_IsSuspending ;

		/// <summary>
		/// このチャンネル再生終了時にコールバックを呼び出すかどうか
		/// </summary>
		internal protected bool callback = true ;

		//---------------------------------------------------------
	
		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="audioSource">オーディオソースのインスタンス</param>
		internal protected AudioChannel( AudioSource audioSource )
		{
			m_AudioSource = audioSource ;
		}
		
		/// <summary>
		/// オーディオチャンネルの破棄を行う
		/// </summary>
		internal protected void Destroy()
		{
			if( m_AudioSource != null )
			{
				UnityEngine.GameObject.Destroy( m_AudioSource ) ;
				m_AudioSource = null ;
			}
		}
	
		/// <summary>
		/// オーディオクリップ
		/// </summary>
		internal protected AudioClip AudioClip
		{
			get
			{
				if( m_AudioSource == null )
				{
					return null ;
				}
			
				return m_AudioSource.clip ;
			}
		}

		/// <summary>
		/// ミュート状態
		/// </summary>
		internal protected bool Mute
		{
			get
			{
				if( m_AudioSource == null )
				{
					return false ;
				}
			
				return m_AudioSource.mute ;
			}
			set
			{
				if( m_AudioSource == null )
				{
					return ;
				}
			
				m_AudioSource.mute = value ;
			}
		}
	
		/// <summary>
		/// ボリューム(0～1)
		/// </summary>
		internal protected float Volume
		{
			get
			{
				if( m_AudioSource == null )
				{
					return 0 ;
				}
				
				return m_Volume ;
			}
			set
			{
				if( m_AudioSource == null )
				{
					return ;
				}
				
				m_Volume = value ;
				m_FadeVolume = 1 ;
				m_AudioSource.volume = m_Volume * m_FadeVolume ;
			}
		}

		/// <summary>
		/// ボリューム(0～1)
		/// </summary>
		internal protected float FadeVolume
		{
			get
			{
				return m_FadeVolume ;
			}
			set
			{
				if( m_AudioSource == null )
				{
					return ;
				}
				
				m_FadeVolume = value ;
				m_AudioSource.volume = m_Volume * m_FadeVolume ;
			}
		}

		/// <summary>
		/// 再生位置(秒)
		/// </summary>
		internal protected float Time
		{
			get
			{
				if( m_AudioSource == null )
				{
					return 0 ;
				}
			
				return m_AudioSource.time ;
			}
		}

		/// <summary>
		/// 再生位置(サンプル)
		/// </summary>
		internal protected int TimeSamples
		{
			get
			{
				if( m_AudioSource == null )
				{
					return 0 ;
				}
			
				return m_AudioSource.timeSamples ;
			}
		}

		//-----------------------------------------------------------
	
		/// <summary>
		/// サウンドを再生する
		/// </summary>
		/// <param name="playId">発音毎に割り当てられるユニークな識別子</param>
		/// <param name="audioClip">オーディオクリップのインスタンス</param>
		/// <param name="loop">ループの有無(true=する・false=しない)</param>
		/// <param name="volume">ボリューム(0～1)</param>
		/// <param name="pan">パン(-1=左～0=中～+1=右)</param>
		/// <param name="pitch">ピッチ(-1=1オクターブ下～0=通常～+1=1オクターブ上)</param>
		/// <param name="tag">タグ名</param>
		internal protected void Play( int playId, AudioClip audioClip, bool loop, float volume, float pan, float pitch, string tag )
		{
			if( m_AudioSource == null || audioClip == null )
			{
				return ;
			}

			PlayId = playId ;

			Name = audioClip.name ;
			Tag = tag ;

			m_AudioSource.spatialBlend = 0 ;	// 2D

			m_AudioSource.pitch		= Mathf.Pow( 2.0f, pitch ) ;
		
			m_AudioSource.loop		= loop ;

			m_AudioSource.clip		= null ;

			m_AudioSource.clip		= audioClip ;

			m_AudioSource.Play() ;

			if( pan >   1 )
			{
				pan  =  1 ;
			}
			else
			if( pan <  -1 )
			{
				pan  = -1 ;
			}
		
			// ボリューム
			Volume	= volume ;

			// パン
			m_AudioSource.panStereo = pan ;

	//		mAudioSource.bypassEffects = true ;
	//		mAudioSource.bypassListenerEffects = true ;
		
			//-----------------------------------------------------
		
			m_IsPausing = false ;

			callback = true ;
		}
	
		/// <summary>
		/// サウンドを完全停止する
		/// </summary>
		internal protected void Stop()
		{
			if( m_AudioSource == null )
			{
				return ;
			}
			
			m_AudioSource.Stop() ;
			
			if( m_AudioSource.clip != null )
			{
				// 他のチャンネルからの参照があるかもしれないので破棄はしない
//				Resources.UnloadAsset( m_AudioSource.clip ) ;	// アセットバンドルから展開したリソースの場合は再度同じリソースを異なるインスタンスで展開しようとすると以前のインスタンスが誤動作を引き起こす事があるので以前のインスタンスは完全にシステムリソースキャッシュから破棄する必要がある。
				m_AudioSource.clip = null ; // 参照を完全になくすため(これをしないと AudioClip がアセットバンドルから生成されたものの際にバグるので超重要)
			}

			// ストップでボーズもビジィも解除される
			m_IsPausing	= false ;
			m_IsLocking	= false ;
		
			Tag = null ;
		}
	
		/// <summary>
		/// サウンドを一時停止する
		/// </summary>
		internal protected void Pause()
		{
			if( m_AudioSource == null )
			{
				return ;
			}
		
			m_AudioSource.Pause() ;
		
			m_IsPausing	= true ;
		}
	
		/// <summary>
		/// サウンドの一時停止を解除する
		/// </summary>
		internal protected void Unpause()
		{
			if( m_AudioSource == null )
			{
				return ;
			}
		
			m_AudioSource.Play() ;
		
			m_IsPausing	= false ;
		}
		
		internal protected void Lock()
		{
			m_IsLocking = true ;
		}

		internal protected void Unlock()
		{
			m_IsLocking = false ;
		}

		internal protected void Suspend()
		{
			m_IsSuspending = true ;
		}

		internal protected void Resume()
		{
			m_IsSuspending = false ;
		}


		/// <summary>
		/// 実際に再生中かの判定を行う（ポーズ中は true とみなされる）
		/// </summary>
		/// <returns>結果(true=再生中・false=再生中ではない)</returns>
		internal protected bool IsRunning
		{
			get
			{
				if( m_AudioSource == null )
				{
					return false ;
				}
			
				if( m_AudioSource.isPlaying == false && m_IsPausing == false )
				{
					return false ;
				}

				return true ;
			}
		}
		
		/// <summary>
		/// 実際に再生中かの判定を行う（ポーズ中は false とみなされる）
		/// </summary>
		/// <returns>結果(true=再生中・false=再生中ではない)</returns>
		internal protected bool IsPlaying
		{
			get
			{
				if( m_AudioSource == null )
				{
					return false ;
				}
				
				return m_AudioSource.isPlaying ;
			}
		}
		
		/// <summary>
		/// 一時停止中か判定を行う
		/// </summary>
		/// <returns>結果(true=一時停止中・false=一時停止中ではない)</returns>
		internal protected bool IsPausing
		{
			get
			{
				return m_IsPausing ;
			}
		}

		/// <summary>
		/// ロック中かどうか
		/// </summary>
		internal protected bool IsLocking
		{
			get
			{
				return m_IsLocking ;
			}
		}

		/// <summary>
		/// 中断中かどうか
		/// </summary>
		internal protected bool IsSuspending
		{
			get
			{
				return m_IsSuspending ;
			}
		}
	}
}

