using System ;
using System.Collections.Generic ;
using System.IO ;

using UnityEditor;
using UnityEditor.Build.Reporting ;

using UnityEngine ;

using Template ;


/// <summary>
/// アプリケーションのバッチビルド用クラス Version 2022/06/12
/// </summary>
public partial class Build_Application
{
	//--------------------------------------------------------------------------------------------
	// 設定

	// 設定ファイルのパス
	private readonly static string	m_SettingsPath			= "ScriptableObjects/Settings" ;

	// 有効なシーンファイルのパスのフィルタ
	private readonly static string	m_SceneFileFilter		= "Assets/Application/Scenes" ;

	//----------------------------------------------------------------------------

	public enum PlatformTypes
	{
		Windows64,
		OSX,
		Android,
		iOS,
	}

	/// <summary>
	/// バージョン値を取得する
	/// </summary>
	/// <returns></returns>
	private static ( string, int ) GetVersion( PlatformTypes platformType )
	{
		string versionName = "0.0.0" ;
		int    versionCode = 0 ;

		Settings settings = Resources.Load<Settings>( m_SettingsPath ) ;
		if( settings != null )
		{
			if( platformType == PlatformTypes.Android )
			{
				// Android
				versionName = settings.ClientVersionName ;
				versionCode = settings.ClientVersionCode ;
			}
			else
			if( platformType == PlatformTypes.iOS )
			{
				// iOS
				versionName = settings.SystemVersionName ;
				versionCode = settings.ClientVersionCode ;
			}
			else
			{
				versionName = settings.SystemVersionName + "." + settings.Revision.ToString() ;
			}
		}

		return ( versionName, versionCode ) ;
	}

	//--------------------------------------------------------------------------------------------

	// 変更する可能性のある設定項目
	private static string	m_PlayerSettings_productName ;
	private static string	m_PlayerSettings_bundleVersion ;
	private static Settings.EndPointNames m_EndPoint ;

	// 変更する可能性のある設定を退避する
	private static void PushSetting()
	{
		m_PlayerSettings_productName	= PlayerSettings.productName ;
		m_PlayerSettings_bundleVersion	= PlayerSettings.bundleVersion ;

		Settings settings = Resources.Load<Settings>( m_SettingsPath ) ;
		if( settings != null )
		{
			m_EndPoint	= settings.WebAPI_DefaultEndPoint ;
		}
	}

	// デフォルトのエンドポイントを設定する
	private static void SetDefaultEndPoint( Settings.EndPointNames endPointName )
	{
		Settings settings = Resources.Load<Settings>( m_SettingsPath ) ;
		if( settings != null )
		{
			settings.WebAPI_DefaultEndPoint = endPointName ;
			
			EditorUtility.SetDirty( settings ) ;	// 重要

			// SaveAssets → Refresh の順でなければダメらしい(逆でもいける事もあるが)
			AssetDatabase.SaveAssets() ;

			settings = Resources.Load<Settings>( m_SettingsPath ) ;
			Debug.Log( "[デフォルトのエンドポイント] " + settings.WebAPI_DefaultEndPoint ) ;
		}
	}

	// 変更する可能性のある設定を復帰する
	private static void PopSetting()
	{
		Debug.Log( "[設定を復帰させる]" ) ;

		PlayerSettings.productName		= m_PlayerSettings_productName ;
		PlayerSettings.bundleVersion	= m_PlayerSettings_bundleVersion ;

		Settings settings = Resources.Load<Settings>( m_SettingsPath ) ;
		if( settings != null )
		{
			settings.WebAPI_DefaultEndPoint		= m_EndPoint ;

			EditorUtility.SetDirty( settings ) ;	// 重要
		}

		AssetDatabase.SaveAssets() ;
	}


	//--------------------------------------------------------------------------------------------
	// 処理

	/// <summary>
	/// ビルドタイプ
	/// </summary>
	public enum BuildTypes
	{
		Release		=  0,
		Staging		=  1,
		Development	=  2,
		Profiler	=  3,
		Unknown		= -1,
	}

	// 外部からはこれを叩いてアプリをビルドする。パラメータはコマンドライン引数から渡す。
	public static void Execute()
	{
		BuildTypes		build			= BuildTypes.Unknown ;
		BuildTarget		platform		= BuildTarget.NoTarget ;

		//-----------------------------------

		string[] args = Environment.GetCommandLineArgs() ;

		for( int i  = 0 ; i <  args.Length ; i ++ )
		{
			Debug.Log( args[ i ] ) ;
			switch( args[ i ] )
			{
				//---------------------------------

				// Windows用
				case "--platform-windows64" :
				case "--platform-windows" :
					platform	= BuildTarget.StandaloneWindows64 ;
				break ;

				// OSX用
				case "--platform-osx" :
				case "--platform-machintosh" :
					platform	= BuildTarget.StandaloneOSX ;
				break ;

				// Android用
				case "--platform-android" :
					platform	= BuildTarget.Android ;
				break ;

				// iOS用
				case "--platform-ios" :
					platform	= BuildTarget.iOS ;
				break ;

				//---------------------------------

				// Release版
				case "--build-release" :
					build = BuildTypes.Release ;
				break ;

				// Staging版
				case "--build-staging" :
					build = BuildTypes.Staging ;
				break ;

				// Development版
				case "--build-development" :
					build = BuildTypes.Development ;
				break ;

				// Profiler版
				case "--build-profiler" :
					build = BuildTypes.Profiler ;
				break ;

				//---------------------------------
			}
		}

		//-----------------------------------------------------------

		Debug.Log( "=================================================" ) ;
		Debug.Log( "------- Platform : " + platform.ToString() ) ;
		Debug.Log( "------- Build    : " + build.ToString() ) ;
		Debug.Log( "=================================================" ) ;

		// プラットフォームごとのビルドを実行する
		bool result = false ;

		switch( platform )
		{
			case BuildTarget.StandaloneWindows64	:
				switch( build )
				{
					case BuildTypes.Release		: result = Execute_Windows64_Release_BuildOnly()		; break ;
					case BuildTypes.Staging		: result = Execute_Windows64_Staging_BuildOnly()		; break ;
					case BuildTypes.Development	: result = Execute_Windows64_Development_BuildOnly()	; break ;
					case BuildTypes.Profiler	: result = Execute_Windows64_Profiler_BuildOnly()		; break ;
				}
			break ;

			case BuildTarget.StandaloneOSX			:
				switch( build )
				{
					case BuildTypes.Release		: result = Execute_OSX_Release_BuildOnly()				; break ;
					case BuildTypes.Staging		: result = Execute_OSX_Staging_BuildOnly()				; break ;
					case BuildTypes.Development	: result = Execute_OSX_Development_BuildOnly()			; break ;
					case BuildTypes.Profiler	: result = Execute_OSX_Profiler_BuildOnly()				; break ;
				}
			break ;

			case BuildTarget.Android				:
				switch( build )
				{
					case BuildTypes.Release		: result = Execute_Android_Release_BuildOnly()			; break ;
					case BuildTypes.Staging		: result = Execute_Android_Staging_BuildOnly()			; break ;
					case BuildTypes.Development : result = Execute_Android_Development_BuildOnly()		; break ;
					case BuildTypes.Profiler	: result = Execute_Android_Profiler_BuildOnly()			; break ;
				}
			break ;

			case BuildTarget.iOS					:
				switch( build )
				{
					case BuildTypes.Release		: result = Execute_iOS_Release_BuildOnly()				; break ;
					case BuildTypes.Staging		: result = Execute_iOS_Staging_BuildOnly()				; break ;
					case BuildTypes.Development : result = Execute_iOS_Development_BuildOnly()			; break ;
					case BuildTypes.Profiler	: result = Execute_iOS_Profiler_BuildOnly()				; break ;
				}
			break ;
		}

		//-----------------------------------------------------------

		if( result == true )
		{
			// ビルド成功
			EditorApplication.Exit( 0 ) ;
		}
		else
		{
			// ビルド失敗
			EditorApplication.Exit( 1 ) ;
		}
	}

	//--------------------------------------------------------------------------------------------

	// ビルドターゲットとビルドターゲットグループの関係
	private static readonly Dictionary<BuildTarget,BuildTargetGroup> m_BuildTargetGroups = new Dictionary<BuildTarget, BuildTargetGroup>()
	{
		{ BuildTarget.StandaloneWindows,		BuildTargetGroup.Standalone	},
		{ BuildTarget.StandaloneWindows64,		BuildTargetGroup.Standalone	},
		{ BuildTarget.StandaloneOSX,			BuildTargetGroup.Standalone	},
		{ BuildTarget.Android,					BuildTargetGroup.Android	},
		{ BuildTarget.iOS,						BuildTargetGroup.iOS		},
	} ;

	// ビルドターゲットを変更する
	private static BuildTarget ChangeBuildTarget( BuildTarget target, string state )
	{
		// 現在のビルドターゲットを取得する
		BuildTarget activeBuildTarget =	EditorUserBuildSettings.activeBuildTarget ;
		if( target != activeBuildTarget )
		{
			if( state == "Change" )
			{
				string s = "[Bad build target] " + target + "\n" ;

				// 動的な変更はプリプロセッサに対応出来ないためワーニングを表示する(一応ビルドは出来るようにしておく)
				s += "================================" + "\n" ;
				s += "The specified build target is different from the current build target." + "\n" ;
				s += "[Specified build target] " + target + "\n" ;
				s += "[Current build target] " + activeBuildTarget + "\n" ;
				s += "" + "\n" ;
				s += "[Case] UnityEditor" + "\n" ;
				s += "Please change the current build target from the menu below ..." + "\n" ;
				s += " File -> Build Settings -> Platform" + "\n" ;
				s += " Set to [ " + target + " ]" + "\n" ;
				s += "" + "\n" ;
				s += "[Case] Commandline build" + "\n" ;
				s += "Please add the following options ..." + "\n" ;

				if( target == BuildTarget.StandaloneWindows64 )
				{
					s += " -buildTarget win64" + "\n" ;
				}
				else
				if( target == BuildTarget.StandaloneOSX )
				{
					s += " -buildTarget osx" + "\n" ;
				}
				else
				if( target == BuildTarget.Android )
				{
					s += " -buildTarget android" + "\n" ;
				}
				else
				if( target == BuildTarget.iOS )
				{
					s += " -buildTarget ios" + "\n" ;
				}

				s += "================================" + "\n" ;

				Debug.LogWarning( s ) ;
			}

			// ビルドターゲットが現在のビルドターゲットと異なる場合にビルドターゲットを切り替える
			EditorUserBuildSettings.SwitchActiveBuildTarget( m_BuildTargetGroups[ target ], target ) ;
			Debug.Log( "[" + state + "Platform] " + activeBuildTarget + " -> " + target ) ;
		}

		return activeBuildTarget ;
	}

	//--------------------------------------------------------------------------------------------

	// 実際にクライアントをビルドする処理
	private static bool Process
	(
		string			path,
		BuildTarget		target,
		BuildOptions	options
	)
	{
		//-----------------------------------------------------------

		// 有効なシーンを取得する
		string[] scenes = GetAvailableScenes() ;
		if( scenes == null || scenes.Length == 0 )
		{
			Debug.Log( "Build ERROR : Target scene could not found." ) ;
			return false ;
		}

		//-----------------------------------

		// ビルド実行
		BuildReport report = BuildPipeline.BuildPlayer( scenes, path, target, options ) ;
		BuildSummary summary = report.summary ;

		bool result = false ;

		if( summary.result == BuildResult.Failed	){ Debug.LogWarning( "Build failed"		) ; }
		else
		if( summary.result == BuildResult.Cancelled	){ Debug.LogWarning( "Build canceled"	) ; }
		else
		if( summary.result == BuildResult.Unknown	){ Debug.LogWarning( "Build unknown"	) ;	}
		else
		if( summary.result == BuildResult.Succeeded	)
		{
			// 成功(コンソールの Clear In Build にチェックが入っているとビルド前のコンソールは全て消去されてしまうのでビルド後のタイミングでログを表示する方が安全
			result = true ;

			// 対象のシーン
			string s = "Build Target Scene -> " + scenes.Length + "\n" ;
			foreach( var scene in scenes )
			{
				s += " + " + scene + "\n" ;
			}
			Debug.Log( s ) ;

			// 出力されたファイルのサイズ
			ulong size = 0 ;
			if( target == BuildTarget.iOS )
			{
				Debug.Log( "Build succeeded -> " + path + " | Total Size = " + GetSizeName( summary.totalSize ) ) ;
			}
			else
			{
				FileInfo fi = new FileInfo( path ) ;
				if( fi != null )
				{
					size = ( ulong )fi.Length ;
				}
				Debug.Log( "Build succeeded -> " + path + " : File Size = " + GetSizeName( size ) + " | Total Size = " + GetSizeName( summary.totalSize ) ) ;
			}
		}

		//-----------------------------------------------------------

		// 結果を返す
		return result ;
	}

	//----------------------------------------------------------------------------
	// プラットフォーム共通の処理メソッド群

	// 有効なシーンのパスのみ取得する
	private static string[] GetAvailableScenes()
	{
		EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes ;

		List<string> targetScenePaths = new List<string>() ;

		// 実際に存在するシーンファイルのみ対象とする
		string path ;
		string filterPath = m_SceneFileFilter ;

		if( string.IsNullOrEmpty( filterPath ) == false )
		{
			filterPath = filterPath.Replace( "\\", "/" ) ;
			filterPath = filterPath.TrimStart( '/' ) ;
		}

		bool isEnable ;
		foreach( var scene in scenes )
		{
			if( scene.enabled == true && string.IsNullOrEmpty( scene.path ) == false && File.Exists( scene.path ) == true )
			{
				isEnable = true ;

				if( string.IsNullOrEmpty( filterPath ) == false )
				{
					// シーンファイルのパスのフィルタが有効
					path = scene.path ;
					path = path.Replace( "\\", "/" ) ;
					path = path.TrimStart( '/' ) ;

					if( path.IndexOf( filterPath ) != 0 )
					{
						isEnable = false ;
					}
				}

				if( isEnable == true )
				{
					targetScenePaths.Add( scene.path ) ;
				}
			}
		}

		if( targetScenePaths.Count == 0 )
		{
			Debug.LogWarning( "Not Found Build Target Scene." ) ;
			return null ;
		}

		return targetScenePaths.ToArray() ;
	}

	// ファイルサイズを見やすい形に変える
	private static string GetSizeName( ulong size )
	{
		string sizeName = "Value Overflow" ;

		if( size <  1024L )
		{
			sizeName = size + " byte" ;
		}
		else
		if( size <  ( 1024L * 1024L ) )
		{
			sizeName = ( size / 1024L ) + " KB" ;
		}
		else
		if( size <  ( 1024L * 1024L * 1024L ) )
		{
			sizeName = ( size / ( 1024L * 1024L ) ) + " MB" ;
		}
		else
		if( size <  ( 1024L * 1024L * 1024L * 1024L ) )
		{
			double value = ( double )size / ( double )( 1024L * 1024L * 1024L ) ;
			value = ( double )( ( int )( value * 1000 ) ) / 1000 ;	// 少数までわかるようにする
			sizeName = value + " GB" ;
		}
		else
		if( size <  ( 1024L * 1024L * 1024L * 1024L * 1024L ) )
		{
			sizeName = ( size / ( 1024L * 1024L * 1024L * 1024L ) ) + " TB" ;
		}
		else
		if( size <  ( 1024L * 1024L * 1024L * 1024L * 1024L * 1024L ) )
		{
			sizeName = ( size / ( 1024L * 1024L * 1024L * 1024L * 1024L ) ) + " PB" ;
		}

		return sizeName ;
	}

	//--------------------------------------------------------------------------------------------

	// 1970年01月01日 00時00分00秒 からの経過秒数を計算するためのエポック時間
	private static readonly DateTime m_UNIX_EPOCH = new DateTime( 1970, 1, 1, 0, 0, 0, 0 ) ;

	private static long m_ProcessingTime ;

	// 時間計測を開始する
	private static void StartClock()
	{
		DateTime dt = DateTime.Now ;
		TimeSpan time = dt.ToUniversalTime() - m_UNIX_EPOCH ;
		m_ProcessingTime = ( long )time.TotalSeconds ;
	}

	// 時間計測を終了する
	private static void StopClock()
	{
		DateTime dt = DateTime.Now ;
		TimeSpan time = dt.ToUniversalTime() - m_UNIX_EPOCH ;
		long processtingTime = ( long )time.TotalSeconds - m_ProcessingTime ;

		long hour   = ( processtingTime / 3600L ) ;
		processtingTime %= 3600 ;
		long minute = ( processtingTime /   60L ) ;
		processtingTime %=   60 ;
		long second =   processtingTime ;

		Debug.Log( "Processing Time -> " + hour.ToString( "D2" ) + ":" + minute.ToString( "D2" ) + ":" + second.ToString( "D2" ) ) ;
	}
}
