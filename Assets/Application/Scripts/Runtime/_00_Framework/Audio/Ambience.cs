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
	/// Ａｍｂｉｅｎｃｅクラス Version 2022/05/04 0
	/// </summary>
	public class Ambience : ExMonoBehaviour
	{
		private static Ambience	m_Instance ;
		internal void Awake()
		{
			m_Instance = this ;
		}
		internal void OnDestroy()
		{
			m_Instance = null ;
		}

		//-----------------------------------

		// Ｊｉｎｇｌｅ名称の一覧(文字列からマスターのＩＤ値などに変わる可能性もある)
		
		public const string None		= null ;

		public const string No001		= "Battle//BGM05_00" ;
		public const string Bgm_20      = "Battle//bgm_20" ;
		public const string Bgm_24      = "Battle//bgm_24" ;
		public const string Win			= "Battle//Win" ;
		public const string Lose		= "Battle//Lose" ;

		//-----------------------------------------------------------

		// 基本パス(環境に合わせて書き換える事)
		private const string m_Path = "Sounds/Ambience" ;

		//-----------------------------------------------------------

		public static async UniTask LoadAsync()
		{
			// アセットバンドルの強制ダウンロードを行う(失敗してもダイアログは出さない)
//			await Asset.DownloadAssetBundleAsync( m_Path + "/Battle", true, true ) ;

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
		/// Ａｍｂｉｅｎｃｅを再生する(同期版:ファイルが存在しない場合は失敗)
		/// </summary>
		/// <param name="path">ファイル名</param>
		/// <param name="volume">ボリューム係数(0～1)</param>
		/// <param name="pan">パン(-1=左～0=中～+1=右)</param>
		/// <param name="loop">ループ(true=する・false=しない)</param>
		/// <returns>発音毎に割り当てられるユニークな識別子(-1で失敗)</returns>
		public static int Play( string path, float volume = 1.0f, float pan = 0, bool loop = false )
		{
			path = CorrectPath( path ) ;

			// 複数を１つのアセットバンドルにまとめており同じシーンの中で何度も再生されるケースがあるのでリソース・アセットバンドル両方にキャッシュする
			AudioClip audioClip = Asset.Load<AudioClip>( m_Path + "/" + path, Asset.CachingTypes.Same ) ;
			if( audioClip == null )
			{
				return -1 ;
			}

			// 再生する
			return AudioManager.Play( audioClip, loop, volume * GetVolume(), pan, 0, "Ambience" ) ;
		}

		/// <summary>
		/// Ａｍｂｉｅｎｃｅを再生する(非同期版:ファイルが存在しない場合はダウンロードを試みる)
		/// </summary>
		/// <param name="path">ファイル名</param>
		/// <param name="volume">ボリューム係数(0～1)</param>
		/// <param name="pan">パン(-1=左～0=中～+1=右)</param>
		/// <param name="loop">ループ(true=する・false=しない)</param>
		/// <returns>列挙子</returns>
		public static async UniTask<int> PlayAsync( string path, float volume = 1.0f, float pan = 0, bool loop = false )
		{
			path = CorrectPath( path ) ;

			int playId ;

			if( Asset.Exists( m_Path + "/" + path ) == true )
			{
				// 既にあるなら同期で高速再生
				playId = Play( path, volume, pan, loop ) ;
				return playId ;
			}

			// 複数を１つのアセットバンドルにまとめており同じシーンの中で何度も再生されるケースがあるのでリソース・アセットバンドル両方にキャッシュする
			AudioClip audioClip = await Asset.LoadAsync<AudioClip>( m_Path + "/" + path, Asset.CachingTypes.Same ) ;
			if( audioClip == null )
			{
				// 失敗
				Debug.LogWarning( "Could not load : " + path ) ;
				return -1 ;
			}

			// 再生する
			playId = AudioManager.Play( audioClip, loop, volume * GetVolume(), pan, 0, "Ambience" ) ;
			if( playId <  0 )
			{
				// 失敗
				Debug.LogWarning( "Could not play : " + path ) ;
				return -1 ;
			}

			// 成功
			return playId ;
		}

		/// <summary>
		/// ３Ｄ空間想定でＡｍｂｉｅｎｃｅを再生する(同期版:ファイルが存在しない場合は失敗)
		/// </summary>
		/// <param name="path">ファイル名</param>
		/// <param name="position">音源の位置(ワールド座標系)</param>
		/// <param name="listener">リスナーのトランスフォーム</param>
		/// <param name="scale">距離係数(リスナーから音源までの距離にこの係数を掛け合わせたものが最終的な距離になる)</param>
		/// <param name="volume">ボリューム係数(0～1)</param>
		public static bool Play3D( string path, Vector3 position, Transform listener = null, float scale = 1, float volume = 1.0f )
		{
			path = CorrectPath( path ) ;

			// 複数を１つのアセットバンドルにまとめており同じシーンの中で何度も再生されるケースがあるのでリソース・アセットバンドル両方にキャッシュする
			AudioClip audioClip = Asset.Load<AudioClip>( m_Path + "/" + path, Asset.CachingTypes.Same ) ;
			if( audioClip == null )
			{
				return false ;
			}
			
			// 再生する
			return AudioManager.Play3D( audioClip, position, listener, scale, volume * GetVolume() ) ;
		}
		
		/// <summary>
		/// ３Ｄ空間想定でＡｍｂｉｅｎｃｅを再生する(非同期版:ファイルが存在しない場合はダウンロードを試みる)
		/// </summary>
		/// <param name="path">ファイル名</param>
		/// <param name="position">音源の位置(ワールド座標系)</param>
		/// <param name="listener">リスナーのトランスフォーム</param>
		/// <param name="scale">距離係数(リスナーから音源までの距離にこの係数を掛け合わせたものが最終的な距離になる)</param>
		/// <param name="volume">ボリューム係数(0～1)</param>
		/// <returns>列挙子</returns>
		public static async UniTask<bool> Play3DAsync( string path, Vector3 position, Transform listener = null, float scale = 1, float volume = 1.0f )
		{
			path = CorrectPath( path ) ;

			if( Asset.Exists( m_Path + "/" + path ) == true )
			{
				// 既にあるなら高速再生
				return Play3D( path, position, listener, scale, volume ) ;
			}

			// 複数を１つのアセットバンドルにまとめており同じシーンの中で何度も再生されるケースがあるのでリソース・アセットバンドル両方にキャッシュする
			AudioClip audioClip = await Asset.LoadAsync<AudioClip>( m_Path + "/" + path, Asset.CachingTypes.Same ) ;
			if( audioClip == null )
			{
				// 失敗
				Debug.LogWarning( "Could not load : " + path ) ;
				return false ;
			}

			// 再生する
			if( AudioManager.Play3D( audioClip, position, listener, scale, volume * GetVolume() ) == false )
			{
				// 失敗
				Debug.LogWarning( "Could not play : " + path ) ;
				return false ;
			}

			// 成功
			return true ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// Ａｍｂｉｅｎｃｅを停止する
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
		/// Ａｍｂｉｅｎｃｅが停止するまで待つ
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
		/// 識別子で指定するＡｍｂｉｅｎｃｅが再生中か確認する(一時停止中でも再生扱いになる)
		/// </summary>
		/// <param name="playId">発音毎に割り当てられるユニークな識別子</param>
		/// <returns></returns>
		public static bool IsRunning( int playId )
		{
			if( playId <  0 || AudioManager.IsRunning( playId ) == false )
			{
				return false ;	// 識別子が不正か鳴っていない
			}

			// 再生中
			return true ;
		}

		/// <summary>
		/// 識別子で指定するＡｍｂｉｅｎｃｅが再生中か確認する(一時停止中は停止扱いになる)
		/// </summary>
		/// <param name="playId">発音毎に割り当てられるユニークな識別子</param>
		/// <returns></returns>
		public static bool IsPlaying( int playId )
		{
			if( playId <  0 || AudioManager.IsPlaying( playId )== false )
			{
				return false ;	// 識別子が不正か鳴っていない
			}

			// 再生中
			return true ;
		}

		/// <summary>
		/// Ａｍｂｉｅｎｃｅを一時停止する
		/// </summary>
		/// <param name="playId">発音毎に割り当てられるユニークな識別子</param>
		/// <param name="fade">フェード時間(秒)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Pause( int playId )
		{
			return AudioManager.Pause( playId ) ;
		}

		/// <summary>
		/// Ａｍｂｉｅｎｃｅを一時停止する
		/// </summary>
		/// <param name="playId">発音毎に割り当てられるユニークな識別子</param>
		/// <param name="fade">フェード時間(秒)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Unpause( int playId )
		{
			return AudioManager.Unpause( playId ) ;
		}

		/// <summary>
		/// 識別子で指定するＡｍｂｉｅｎｃｅが再生中か確認する
		/// </summary>
		/// <param name="playId">発音毎に割り当てられるユニークな識別子</param>
		/// <returns></returns>
		public static bool IsPausing( int playId )
		{
			if( playId <  0 || AudioManager.IsPausing( playId )== false )
			{
				return false ;	// 識別子が不正か鳴っていない
			}

			// 再生中
			return true ;
		}

		/// <summary>
		/// 全Ａｍｂｉｅｎｃｅを完全停止する
		/// </summary>
		/// <param name="playId">発音毎に割り当てられるユニークな識別子</param>
		/// <param name="fade">フェード時間(秒)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool StopAll()
		{
			return AudioManager.StopAll( "Ambience" ) ;
		}

		/// <summary>
		/// 全Ａｍｂｉｅｎｃｅを一時停止する
		/// </summary>
		/// <param name="playId">発音毎に割り当てられるユニークな識別子</param>
		/// <param name="fade">フェード時間(秒)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool PauseAll()
		{
			return AudioManager.PauseAll( "Ambience" ) ;
		}

		/// <summary>
		/// 全Ａｍｂｉｅｎｃｅを一時再開する
		/// </summary>
		/// <param name="playId">発音毎に割り当てられるユニークな識別子</param>
		/// <param name="fade">フェード時間(秒)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool UnpauseAll()
		{
			return AudioManager.UnpauseAll( "Ambience" ) ;
		}

		//-------------------------------------------------------------------------------------------

		// コンフィグのボリューム値を取得する
		private static float GetVolume()
		{
//			return Player.jingleVolume ;
			return 1 ;
		}
	}
}
#endif
