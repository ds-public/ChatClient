#define USE_UNITY_AUDIO

#if USE_UNITY_AUDIO
using System ;
using System.Collections ;
using System.Collections.Generic ;

using Cysharp.Threading.Tasks ;

using UnityEngine ;

// 要 AudioHelper パッケージ
using AudioHelper ;

namespace Template
{
	/// <summary>
	/// ＢＧＭクラス Version 2022/05/04 0
	/// </summary>
	public class BGM : ExMonoBehaviour
	{
		private static BGM	m_Instance ;
		internal void Awake()
		{
			m_Instance = this ;
		}
		internal void OnDestroy()
		{
			m_Instance = null ;
		}

		//-----------------------------------

		// ＢＧＭ名称の一覧(文字列からマスターのＩＤ値などに変わる可能性もある)

		public const string No001	= "BGM00_00" ;
		public const string No002	= "BGM00_01" ;
		public const string No003	= "BGM00_02" ;
		//public const string No004   = "BGM02_00" ;
		public const string No005   = "BGM01_02" ;
		public const string No006   = "BGM05_00" ;

		public const string Bgm_26   = "bgm_26" ;
		public const string Bgm_30   = "bgm_30" ;

		public const string Bgm2_19   = "bgm2_19" ;
		public const string Bgm2_40   = "bgm2_40" ;

		public const string Rest		= "BGM_10_Rest_NotLoop" ;
		public const string Working		= "BGM_11_Working_NotLoop" ;
		public const string Training	= "BGM_12_Training_NotLoop" ;


		//-----------------------------------------------------------

		// 基本パス(環境に合わせて書き換える事)
		private const string m_Path = "Sounds/BGM" ;

		//-----------------------------------------------------------

		// メインＢＧＭのオーディオチャンネルインスタンスを保持する
		private static string	m_MainBGM_Path		= string.Empty ;
		private static int		m_MainBGM_PlayId	= -1 ;
		private static float	m_MainBGM_Volume	=  1 ;

		//-----------------------------------------------------------

		public static async UniTask LoadAsync()
		{
			await m_Instance.Yield() ;
		}

		// パスの保険
		private static string CorrectPath( string path )
		{
			// 保険をかける
			if( path.Contains( "//" ) == false )
			{
				// アセットバンドルのパス指定が無い
				int p = path.LastIndexOf( '/' ) ;
				if( p >= 0 )
				{
					path = path.Substring( 0, p ) + "//" + path.Substring( p + 1, path.Length - ( p + 1 ) ) ;
				}
			}
			return path ;
		}
		
		/// <summary>
		/// メインＢＧＭを再生する(同期版:ファイルが存在しない場合は失敗)
		/// </summary>
		/// <param name="path">ファイル名</param>
		/// <param name="fade">フェード時間(秒)</param>
		/// <param name="volume">ボリューム係数(0～1)</param>
		/// <param name="pan">パン(-1=左～0=中～+1=右)</param>
		/// <param name="loop">ループ(true=する・false=しない)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool PlayMain( string path, float fade = 0, float volume = 1.0f, float pan = 0, bool loop = true, bool restart = false )
		{
			path = CorrectPath( path ) ;

			// 再生中以外のものは破棄されて構わないので一切キャッシュには積まない
			AudioClip audioClip = Asset.Load<AudioClip>( m_Path + "/" + path, Asset.CachingTypes.None ) ;
			if( audioClip == null )
			{
				// 失敗
				return false ;
			}

			if( restart == false )
			{
				// 既に同じ曲が鳴っていたらスルーする
				if( path == m_MainBGM_Path )
				{
					return true ;
				}
			}

			//----------------------------------------------------------

			int playId ;

			if( fade <= 0 )
			{
				// フェードなし再生
				if( m_MainBGM_PlayId >= 0 && AudioManager.IsPlaying( m_MainBGM_PlayId ) == true )
				{
					AudioManager.Stop( m_MainBGM_PlayId ) ;
					m_MainBGM_Path		= string.Empty ;
					m_MainBGM_PlayId	= -1 ;
				}

				// 再生する
				playId = AudioManager.Play( audioClip, loop, volume * GetVolume(), pan, 0, "BGM" ) ;
			}
			else
			{
				// フェードあり再生
				if( m_MainBGM_PlayId >= 0 && AudioManager.IsPlaying( m_MainBGM_PlayId ) == true )
				{
					AudioManager.StopFade( m_MainBGM_PlayId, fade ) ;
					m_MainBGM_Path		= string.Empty ;
					m_MainBGM_PlayId	= -1 ;
				}

				// 再生する
				playId = AudioManager.PlayFade( audioClip, fade, loop, volume * GetVolume(), pan, 0, "BGM" ) ;
			}

			if( playId <  0 )
			{
				// 失敗
				return false ;
			}

			m_MainBGM_Path		= path ;
			m_MainBGM_PlayId	= playId ;
			m_MainBGM_Volume	= volume ;

			return true ;
		}

		/// <summary>
		/// ＢＧＭを再生する(非同期版:ファイルが存在しない場合はダウンロードを試みる)
		/// </summary>
		/// <param name="path">ファイル名</param>
		/// <param name="fade">フェード時間(秒)</param>
		/// <param name="volume">ボリューム係数(0～1)</param>
		/// <param name="pan">パン(-1=左～0=中～+1=右)</param>
		/// <param name="loop">ループ(true=する・false=しない)</param>
		/// <returns>列挙子</returns>
		public static async UniTask<int> PlayMainAsync( string path, float fade = 0, float volume = 1.0f, float pan = 0, bool loop = true, bool restart = false )
		{
			path = CorrectPath( path ) ;

			if( Asset.Exists( m_Path + "/" + path ) == true )
			{
				// 既にあるなら同期で高速再生
				PlayMain( path, fade, volume, pan, loop ) ;
				return m_MainBGM_PlayId ;
			}

			// 再生中以外のものは破棄されて構わないので一切キャッシュには積まない
			AudioClip audioClip = await Asset.LoadAsync<AudioClip>( m_Path + "/" + path, Asset.CachingTypes.None ) ;
			if( audioClip == null )
			{
				// 失敗
				Debug.LogWarning( "Could not load : " + path ) ;
				return -1 ;
			}

			if( restart == false )
			{
				// 既に同じ曲が鳴っていたらスルーする
				if( path == m_MainBGM_Path )
				{
					return -1 ;
				}
			}

			//----------------------------------------------------------
			// ＢＧＭを再生する
			int playId ;
			
			if( fade <= 0 )
			{
				// フェードなし再生
				if( m_MainBGM_PlayId >= 0 && AudioManager.IsPlaying( m_MainBGM_PlayId ) == true )
				{
					AudioManager.Stop( m_MainBGM_PlayId ) ;
					m_MainBGM_Path		= string.Empty ;
					m_MainBGM_PlayId	= -1 ;
				}

				// 再生する
				playId = AudioManager.Play( audioClip, loop, volume * GetVolume(), pan, 0, "BGM" ) ;
			}
			else
			{
				// フェードあり再生
				if( m_MainBGM_PlayId >= 0 && AudioManager.IsPlaying( m_MainBGM_PlayId ) == true )
				{
					AudioManager.StopFade( m_MainBGM_PlayId, fade ) ;
					m_MainBGM_Path		= string.Empty ;
					m_MainBGM_PlayId	= -1 ;
				}

				// 再生する
				playId = AudioManager.PlayFade( audioClip, fade, loop, volume * GetVolume(), pan, 0, "BGM" ) ;
			}

			if( playId <  0 )
			{
				// 失敗
				Debug.LogWarning( "Could not play : " + path ) ;
				return -1 ;
			}

			m_MainBGM_Path		= path ;
			m_MainBGM_PlayId	= playId ;
			m_MainBGM_Volume	= volume ;
			
			// 成功
			return m_MainBGM_PlayId ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// メインＢＧＭを停止する
		/// </summary>
		/// <param name="fade">フェード時間(秒)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool StopMain( float fade = 0 )
		{
			if( m_MainBGM_PlayId <  0 )
			{
				return false ;	// 元々鳴っていない
			}

			if( AudioManager.IsPlaying( m_MainBGM_PlayId ) == false )
			{
				m_MainBGM_Path		= string.Empty ;
				m_MainBGM_PlayId	= -1 ;
				return false ;	// 元々鳴っていない
			}

			if( fade <= 0 )
			{
				// フェードなし停止
				AudioManager.Stop( m_MainBGM_PlayId ) ;
			}
			else
			{
				// フェードあり停止
				AudioManager.StopFade( m_MainBGM_PlayId, fade ) ;
			}

			m_MainBGM_Path		= string.Empty ;
			m_MainBGM_PlayId	= -1 ;

			return true ;
		}

		/// <summary>
		/// ＢＧＭが停止するまで待つ
		/// </summary>
		/// <param name="fade"></param>
		/// <returns></returns>
		public static async UniTask<bool> StopMainAsync( float fade = 0 )
		{
			if( m_MainBGM_PlayId <  0 )
			{
				return false ;	// 元々鳴っていない
			}

			if( AudioManager.IsPlaying( m_MainBGM_PlayId ) == false )
			{
				m_MainBGM_Path		= string.Empty ;
				m_MainBGM_PlayId	= -1 ;
				return false ;	// 元々鳴っていない
			}

			if( fade <= 0 )
			{
				// フェードなし停止
				AudioManager.Stop( m_MainBGM_PlayId ) ;
			}
			else
			{
				// フェードあり停止
				AudioManager.StopFade( m_MainBGM_PlayId, fade ) ;
				
				await m_Instance.WaitWhile( () => AudioManager.IsPlaying( m_MainBGM_PlayId ) == true ) ;
			}

			m_MainBGM_Path		= string.Empty ;
			m_MainBGM_PlayId	= -1 ;
			return true ;
		}

		/// <summary>
		/// メインＢＧＭのボリュームを設定する(オプションからリアルタイムに変更するケースで使用する value = GetVolume() )
		/// </summary>
		public static float Volume
		{
			set
			{
				if( m_MainBGM_PlayId >= 0 )
				{
					AudioManager.SetVolume( m_MainBGM_PlayId, m_MainBGM_Volume * value ) ;
				}
			}
		}
		
		/// <summary>
		/// メインＢＧＭが再生中か確認する
		/// </summary>
		/// <param name="audioClipName">曲の名前(再生中の曲の種類を限定したい場合は指定する)</param>
		/// <returns>再生状況(true=再生中・false=停止中)</returns>
		public static bool IsPlayingMain( string path = null )
		{
			if( m_MainBGM_PlayId <  0 || AudioManager.IsPlaying( m_MainBGM_PlayId ) == false )
			{
				return false ;
			}

			// 何かしらの曲は鳴っている
			if( string.IsNullOrEmpty( path ) == false )
			{
				// 曲の名前の指定がある
//				if( AudioManager.GetName( m_MainBGM_PlayId ) != audioClipName )
				if( m_MainBGM_Path != path )
				{
					// 指定した曲は鳴っていない
					return false ;
				}
			}

			return true ;
		}

		/// <summary>
		/// 再生中のメインＢＧＭの名前を取得する
		/// </summary>
		/// <returns></returns>
		public static string GetMainName()
		{
			if( m_MainBGM_PlayId <  0 || AudioManager.IsPlaying( m_MainBGM_PlayId )== false )
			{
				return string.Empty ;
			}

			return AudioManager.GetName( m_MainBGM_PlayId ) ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// ＢＧＭを再生する(同期版:ファイルが存在しない場合は失敗)
		/// </summary>
		/// <param name="path">ファイル名</param>
		/// <param name="fade">フェード時間(秒)</param>
		/// <param name="volume">ボリューム係数(0～1)</param>
		/// <param name="pan">パン(-1=左～0=中～+1=右)</param>
		/// <param name="loop">ループ(true=する・false=しない)</param>
		/// <returns>発音毎に割り当てられるユニークな識別子</returns>
		public static int Play( string path, float fade = 0, float volume = 1.0f, float pan = 0, bool loop = true )
		{
			path = CorrectPath( path ) ;

			// 再生中以外のものは破棄されて構わないので一切キャッシュには積まない
			AudioClip audioClip = Asset.Load<AudioClip>( m_Path + "/" + path, Asset.CachingTypes.None ) ;
			if( audioClip == null )
			{
				// 失敗
				Debug.LogWarning( "Could not play : " + path ) ;
				return -1 ;
			}

			int playId ;

			if( fade <= 0 )
			{
				// フェードなし再生
				playId = AudioManager.Play( audioClip, loop, volume * GetVolume(), pan, 0, "BGM" ) ;
			}
			else
			{
				// フェードあり再生
				playId = AudioManager.PlayFade( audioClip, fade, loop, volume * GetVolume(), pan, 0, "BGM" ) ;
			}

			if( playId <  0 )
			{
				// 失敗
				Debug.LogWarning( "Could not play : " + path ) ;
				return -1 ;
			}

			return playId ;
		}

		/// <summary>
		/// ＢＧＭを再生する(非同期版:ファイルが存在しない場合はダウンロードを試みる)
		/// </summary>
		/// <param name="path">ファイル名</param>
		/// <param name="fade">フェード時間(秒)</param>
		/// <param name="volume">ボリューム係数(0～1)</param>
		/// <param name="pan">パン(-1=左～0=中～+1=右)</param>
		/// <param name="loop">ループ(true=する・false=しない)</param>
		/// <returns>列挙子</returns>
		public static async UniTask<int> PlayAsync( string path, float fade = 0, float volume = 1.0f, float pan = 0, bool loop = true )
		{
			path = CorrectPath( path ) ;

			int playId ;

			if( Asset.Exists( m_Path + "/" + path ) == true )
			{
				// 既にあるなら高速再生
				playId = Play( path, fade, volume, pan, loop ) ;
				return playId ;
			}

			// 再生中以外のものは破棄されて構わないので一切キャッシュには積まない
			AudioClip audioClip = await Asset.LoadAsync<AudioClip>( m_Path + "/" + path, Asset.CachingTypes.Same ) ;
			if( audioClip == null )
			{
				// 失敗
				Debug.LogWarning( "Could not load : " + path ) ;
				return -1 ;
			}

			if( fade <= 0 )
			{
				// フェードなし再生
				playId = AudioManager.Play( audioClip, loop, volume * GetVolume(), pan, 0, "BGM" ) ;
			}
			else
			{
				// フェードあり再生
				playId = AudioManager.PlayFade( audioClip, fade, loop, volume * GetVolume(), pan, 0, "BGM" ) ;
			}

			if( playId <  0 )
			{
				// 失敗
				Debug.LogWarning( "Could not play : " + path ) ;
				return -1 ;
			}

			// 成功
			return playId ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ＢＧＭを停止する
		/// </summary>
		/// <param name="playId">発音毎に割り当てられるユニークな識別子</param>
		/// <param name="fade">フェード時間(秒)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Stop( int playId, float fade = 0 )
		{
			if( fade <= 0 )
			{
				// フェードなし停止
				return AudioManager.Stop( playId ) ;
			}
			else
			{
				// フェードあり停止
				return AudioManager.StopFade( playId, fade ) ;
			}
		}

		/// <summary>
		/// ＢＧＭが停止するまで待つ
		/// </summary>
		/// <param name="playId">発音毎に割り当てられるユニークな識別子</param>
		/// <param name="fade"></param>
		/// <returns></returns>
		public static async UniTask<bool> StopAsync( int playId, float fade = 0 )
		{
			if( playId <  0 || AudioManager.IsPlaying( playId ) == false )
			{
				return false ;	// 識別子が不正か元々鳴っていない
			}

			if( fade <= 0 )
			{
				// フェードなし停止
				AudioManager.Stop( playId ) ;
			}
			else
			{
				// フェードあり停止
				AudioManager.StopFade( playId, fade ) ;
				
				await m_Instance.WaitWhile( () => AudioManager.IsPlaying( playId ) == true ) ;
			}

			return true ;
		}

		/// <summary>
		/// 識別子で指定するＢＧＭが再生中か確認する
		/// </summary>
		/// <param name="playId">発音毎に割り当てられるユニークな識別子</param>
		/// <returns></returns>
		public static bool IsPlaying( int playId )
		{
			if( playId <  0 || AudioManager.IsPlaying( playId )== false )
			{
				return false ;  // 識別子が不正か鳴っていない
			}

			// 再生中
			return true ;
		}

		//-------------------------------------------------------------------------------------------

		// コンフィグのボリューム値を取得する
		private static float GetVolume()
		{
//			return Player.bgmVolume ;
			return 1 ;
		}
	}
}
#endif
