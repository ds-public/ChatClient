using System ;
using System.Collections.Generic ;
using System.IO ;

using UnityEngine ;

using UnityEditor ;
using UnityEditor.Build.Reporting ;

using Template ;


/// <summary>
/// アプリケーションのバッチビルド用クラス(Windows64用)
/// </summary>
public partial class Build_Application
{
	//--------------------------------------------------------------------------------------------
	// 設定

	//------------------------------------
	// Windows64用
	private const string	m_Path_Windows64								= "TEMPLATE/template.exe" ;

	private static string	VersionName_Windows64
	{
		get
		{
			( string versionName, _ ) = GetVersion( PlatformTypes.Windows64 ) ;
			return versionName ;
		}
	}

	private const string	m_ProductName_Windows64_Release					= "TEMPLATE" ;
	private const string	m_ProductName_Windows64_Staging					= "TEMPLATE(S)" ;
	private const string	m_ProductName_Windows64_Development				= "TEMPLATE(D)" ;
	private const string	m_ProductName_Windows64_Profiler				= "TEMPLATE(P)" ;

	private const string	m_Identifier_Windows64_Release					= "com.TEMPLATE" ;
	private const string	m_Identifier_Windows64_Staging					= "com.TEMPLATE" ;
	private const string	m_Identifier_Windows64_Development				= "com.TEMPLATE" ;
	private const string	m_Identifier_Windows64_Profiler					= "com.TEMPLATE" ;


	//--------------------------------------------------------------------------------------------
	// 処理

	//--------------------------------------------------------------------------------------------
	// for Mono

	/// <summary>
	/// Windows64用(Release)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Windows64/Mono/Release/BuildOnly", priority = 10)]
	internal static bool Execute_Windows64_Release_BuildOnly()
	{
		return Execute_Windows64_Release( false ) ;
	}

	/// <summary>
	/// Windows64用(Release)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Windows64/Mono/Release/BuildAndRun", priority = 11)]
	internal static bool Execute_Windows64_Release_BuildAndRun()
	{
		return Execute_Windows64_Release( true ) ;
	}

	/// <summary>
	/// Windows64用(Release)
	/// </summary>
	/// <returns></returns>
	internal static bool Execute_Windows64_Release( bool andRun )
	{
		var settings = PlayerSettings.GetScriptingBackend( BuildTargetGroup.Standalone ) ;
		if( settings != ScriptingImplementation.Mono2x )
		{
			EditorUtility.DisplayDialog( "注意", "Mono が有効化されてません", "閉じる" ) ;
			return false ;
		}

		//-----------------------------------

		PushSetting() ;	// 設定の退避

		SetDefaultEndPoint( Settings.EndPointNames.Release ) ;

		BuildOptions	options		= BuildOptions.None ;	// ひとまずビルド速度優先
		if( andRun == true )
		{
			options |= BuildOptions.AutoRunPlayer ;
		}

		bool result = Execute_Windows64_General
		(
			m_ProductName_Windows64_Release,
			m_Identifier_Windows64_Release,
			options
		) ;

		PopSetting() ;	// 設定の復帰

		return result ;
	}

	//----------------

	/// <summary>
	/// Windows64用(Staging)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Windows64/Mono/Staging/BuildOnly", priority = 20)]
	internal static bool Execute_Windows64_Staging_BuildOnly()
	{
		return Execute_Windows64_Staging( false ) ;
	}

	/// <summary>
	/// Windows64用(Staging)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Windows64/Mono/Staging/BuildAndRun", priority = 21)]
	internal static bool Execute_Windows64_Staging_BuildAndRun()
	{
		return Execute_Windows64_Staging( true ) ;
	}

	/// <summary>
	/// Windows64用(Staging)
	/// </summary>
	/// <returns></returns>
	private static bool Execute_Windows64_Staging( bool andRun )
	{
		var settings = PlayerSettings.GetScriptingBackend( BuildTargetGroup.Standalone ) ;
		if( settings != ScriptingImplementation.Mono2x )
		{
			EditorUtility.DisplayDialog( "注意", "Mono が有効化されてません", "閉じる" ) ;
			return false ;
		}

		//-----------------------------------

		PushSetting() ;	// 設定の退避

		SetDefaultEndPoint( Settings.EndPointNames.Staging ) ;

		BuildOptions	options		= BuildOptions.None ;	// ひとまずビルド速度優先と自動実行
		if( andRun == true )
		{
			options |= BuildOptions.AutoRunPlayer ;
		}

		bool result = Execute_Windows64_General
		(
			m_ProductName_Windows64_Staging,
			m_Identifier_Windows64_Staging,
			options
		) ;

		PopSetting() ;	// 設定の復帰

		return result ;
	}

	//----------------

	/// <summary>
	/// Windows64用(Development)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Windows64/Mono/Development(ServerBuild)/BuildOnly", priority = 30)]
	internal static bool Execute_Windows64_Development_NotDebug_ServerMode_BuildOnly()
	{
		return Execute_Windows64_Development( true, true, false ) ;
	}


	/// <summary>
	/// Windows64用(Development)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Windows64/Mono/Development(NotDebug)/BuildOnly", priority = 30)]
	internal static bool Execute_Windows64_Development_NotDebug_BuildOnly()
	{
		return Execute_Windows64_Development( false, true, false ) ;
	}

	/// <summary>
	/// Windows64用(Development)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Windows64/Mono/Development(NotDebug)/BuildAndRun", priority = 31)]
	internal static bool Execute_Windows64_Development_NotDebug_BuildAndRun()
	{
		return Execute_Windows64_Development( false, true, true ) ;
	}

	/// <summary>
	/// Windows64用(Development)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Windows64/Mono/Development/BuildOnly", priority = 32)]
	internal static bool Execute_Windows64_Development_BuildOnly()
	{
		return Execute_Windows64_Development( false, false, false ) ;
	}

	/// <summary>
	/// Windows64用(Development)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Windows64/Mono/Development/BuildAndRun", priority = 33)]
	internal static bool Execute_Windows64_Development_BuildAndRun()
	{
		return Execute_Windows64_Development( false, false, true ) ;
	}

	/// <summary>
	/// Windows64用(Development)
	/// </summary>
	/// <returns></returns>
	private static bool Execute_Windows64_Development( bool isServer, bool notDebug, bool andRun )
	{
		var settings = PlayerSettings.GetScriptingBackend( BuildTargetGroup.Standalone ) ;
		if( settings != ScriptingImplementation.Mono2x )
		{
			EditorUtility.DisplayDialog( "注意", "Mono が有効化されてません", "閉じる" ) ;
			return false ;
		}

		//-----------------------------------

		PushSetting() ;	// 設定の退避

		SetDefaultEndPoint( Settings.EndPointNames.Development ) ;

		BuildOptions	options	;

		if( isServer == false )
		{
			if( notDebug == true )
			{
				options = BuildOptions.None ;
			}
			else
			{
				options = BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.WaitForPlayerConnection ;
			}
		}
		else
		{
			options = BuildOptions.EnableHeadlessMode ;
		}

		if( andRun == true )
		{
			options |= BuildOptions.AutoRunPlayer ;
		}

		bool result = Execute_Windows64_General
		(
			m_ProductName_Windows64_Development,
			m_Identifier_Windows64_Development,
			options
		) ;

		PopSetting() ;	// 設定の復帰

		return result ;
	}

	//----------------

	/// <summary>
	/// Windows64用(Profiler)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Windows64/Mono/Profiler/BuildOnly", priority = 40)]
	internal static bool Execute_Windows64_Profiler_BuildOnly()
	{
		return Execute_Windows64_Profiler( false ) ;
	}

	/// <summary>
	/// Windows64用(Profiler)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Windows64/Mono/Profiler/BuildAndRun", priority = 41)]
	internal static bool Execute_Windows64_Profiler_BuildAndRun()
	{
		return Execute_Windows64_Profiler( true ) ;
	}

	/// <summary>
	/// Windows64用(Profiler)
	/// </summary>
	/// <returns></returns>
	private static bool Execute_Windows64_Profiler( bool andRun )
	{
		var settings = PlayerSettings.GetScriptingBackend( BuildTargetGroup.Standalone ) ;
		if( settings != ScriptingImplementation.Mono2x )
		{
			EditorUtility.DisplayDialog( "注意", "Mono が有効化されてません", "閉じる" ) ;
			return false ;
		}

		//-----------------------------------

		PushSetting() ;	// 設定の退避

		SetDefaultEndPoint( Settings.EndPointNames.Development ) ;

		BuildOptions	options		= BuildOptions.Development | BuildOptions.ConnectWithProfiler | BuildOptions.EnableDeepProfilingSupport | BuildOptions.AllowDebugging | BuildOptions.WaitForPlayerConnection ;
		if( andRun == true )
		{
			options |= BuildOptions.AutoRunPlayer ;
		}

		bool result = Execute_Windows64_General
		(
			m_ProductName_Windows64_Profiler,
			m_Identifier_Windows64_Profiler,
			options
		) ;

		PopSetting() ;	// 設定の復帰

		return result ;
	}

	//--------------------------------------------------------------------------------------------
	// for IL2CPP

	/// <summary>
	/// Windows64用(Release)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Windows64/IL2CPP/Release/BuildOnly", priority = 10)]
	internal static bool Execute_Windows64_IL2CPP_Release_BuildOnly()
	{
		return Execute_Windows64_IL2CPP_Release( false ) ;
	}

	/// <summary>
	/// Windows64用(Release)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Windows64/IL2CPP/Release/BuildAndRun", priority = 11)]
	internal static bool Execute_Windows64_IL2CPP_Release_BuildAndRun()
	{
		return Execute_Windows64_IL2CPP_Release( true ) ;
	}

	/// <summary>
	/// Windows64用(Release)
	/// </summary>
	/// <returns></returns>
	internal static bool Execute_Windows64_IL2CPP_Release( bool andRun )
	{
		var settings = PlayerSettings.GetScriptingBackend( BuildTargetGroup.Standalone ) ;
		if( settings != ScriptingImplementation.IL2CPP )
		{
			EditorUtility.DisplayDialog( "注意", "IL2CPP が有効化されてません", "閉じる" ) ;
			return false ;
		}

		//-----------------------------------

		PushSetting() ;	// 設定の退避

		SetDefaultEndPoint( Settings.EndPointNames.Release ) ;

		BuildOptions	options		= BuildOptions.None ;	// ひとまずビルド速度優先
		if( andRun == true )
		{
			options |= BuildOptions.AutoRunPlayer ;
		}

		bool result = Execute_Windows64_General
		(
			m_ProductName_Windows64_Release,
			m_Identifier_Windows64_Release,
			options
		) ;

		PopSetting() ;	// 設定の復帰

		return result ;
	}

	//----------------

	/// <summary>
	/// Windows64用(Staging)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Windows64/IL2CPP/Staging/BuildOnly", priority = 20)]
	internal static bool Execute_Windows64_IL2CPP_Staging_BuildOnly()
	{
		return Execute_Windows64_IL2CPP_Staging( false ) ;
	}

	/// <summary>
	/// Windows64用(Staging)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Windows64/IL2CPP/Staging/BuildAndRun", priority = 21)]
	internal static bool Execute_Windows64_IL2CPPStaging_BuildAndRun()
	{
		return Execute_Windows64_IL2CPP_Staging( true ) ;
	}

	// Windows64 - Staging
	private static bool Execute_Windows64_IL2CPP_Staging( bool andRun )
	{
		var settings = PlayerSettings.GetScriptingBackend( BuildTargetGroup.Standalone ) ;
		if( settings != ScriptingImplementation.IL2CPP )
		{
			EditorUtility.DisplayDialog( "注意", "IL2CPP が有効化されてません", "閉じる" ) ;
			return false ;
		}

		//-----------------------------------

		PushSetting() ;	// 設定の退避

		SetDefaultEndPoint( Settings.EndPointNames.Staging ) ;

		BuildOptions	options		= BuildOptions.None ;	// ひとまずビルド速度優先と自動実行
		if( andRun == true )
		{
			options |= BuildOptions.AutoRunPlayer ;
		}

		bool result = Execute_Windows64_General
		(
			m_ProductName_Android_Staging,
			m_Identifier_Android_Staging,
			options
		) ;	

		PopSetting() ;	// 設定の復帰

		return result ;
	}

	//----------------

	/// <summary>
	/// Windows64用(Development)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Windows64/IL2CPP/Development(NotDebug)/BuildOnly", priority = 30)]
	internal static bool Execute_Windows64_IL2CPP_Development_NotDebug_BuildOnly()
	{
		return Execute_Windows64_IL2CPP_Development( true, false ) ;
	}

	/// <summary>
	/// Windows64用(Development)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Windows64/IL2CPP/Development(NotDebug)/BuildAndRun", priority = 31)]
	internal static bool Execute_Windows64_IL2CPP_Development_NotDebug_BuildAndRun()
	{
		return Execute_Windows64_IL2CPP_Development( true, true ) ;
	}

	/// <summary>
	/// Windows64用(Development)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Windows64/IL2CPP/Development/BuildOnly", priority = 32)]
	internal static bool Execute_Windows64_IL2CPP_Development_BuildOnly()
	{
		return Execute_Windows64_IL2CPP_Development( false, false ) ;
	}

	/// <summary>
	/// Windows64用(Development)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Windows64/IL2CPP/Development/BuildAndRun", priority = 33)]
	internal static bool Execute_Windows64_IL2CPP_Development_BuildAndRun()
	{
		return Execute_Windows64_IL2CPP_Development( false, true ) ;
	}

	// Windows64 - Development
	private static bool Execute_Windows64_IL2CPP_Development( bool notDebug, bool andRun )
	{
		var settings = PlayerSettings.GetScriptingBackend( BuildTargetGroup.Standalone ) ;
		if( settings != ScriptingImplementation.IL2CPP )
		{
			EditorUtility.DisplayDialog( "注意", "IL2CPP が有効化されてません", "閉じる" ) ;
			return false ;
		}

		//-----------------------------------

		PushSetting() ;	// 設定の退避

		SetDefaultEndPoint( Settings.EndPointNames.Development ) ;

		BuildOptions	options	;

		if( notDebug == true )
		{
			options = BuildOptions.None ;
		}
		else
		{
			options = BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.WaitForPlayerConnection ;
		}

		if( andRun == true )
		{
			options |= BuildOptions.AutoRunPlayer ;
		}

		bool result = Execute_Windows64_General
		(
			m_ProductName_Android_Development,
			m_Identifier_Android_Development,
			options
		) ;

		PopSetting() ;	// 設定の復帰

		//-----------------------------------

		return result ;
	}

	//----------------

	/// <summary>
	/// Windows64用(Profiler)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Windows64/IL2CPP/Profiler/BuildOnly", priority = 40)]
	internal static bool Execute_Windows64_IL2CPP_Profiler_BuildOnly()
	{
		return Execute_Windows64_IL2CPP_Profiler( false ) ;
	}

	/// <summary>
	/// Windows64用(Profiler)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Windows64/IL2CPP/Profiler/BuildAndRun", priority = 41)]
	internal static bool Execute_Windows64_IL2CPP_Profiler_BuildAndRun()
	{
		return Execute_Windows64_IL2CPP_Profiler( true ) ;
	}

	/// <summary>
	/// Windows64用(Profiler)
	/// </summary>
	/// <returns></returns>
	private static bool Execute_Windows64_IL2CPP_Profiler( bool andRun )
	{
		var settings = PlayerSettings.GetScriptingBackend( BuildTargetGroup.Standalone ) ;
		if( settings != ScriptingImplementation.IL2CPP )
		{
			EditorUtility.DisplayDialog( "注意", "IL2CPP が有効化されてません", "閉じる" ) ;
			return false ;
		}

		//-----------------------------------

		PushSetting() ;	// 設定の退避

		SetDefaultEndPoint( Settings.EndPointNames.Development ) ;

		BuildOptions	options		= BuildOptions.Development | BuildOptions.ConnectWithProfiler | BuildOptions.EnableDeepProfilingSupport | BuildOptions.AllowDebugging | BuildOptions.WaitForPlayerConnection ;
		if( andRun == true )
		{
			options |= BuildOptions.AutoRunPlayer ;
		}

		bool result = Execute_Windows64_General
		(
			m_ProductName_Windows64_Profiler,
			m_Identifier_Windows64_Profiler,
			options
		) ;

		PopSetting() ;	// 設定の復帰

		return result ;
	}

	//--------------------------------------------------------------------------------------------

	/// <summary>
	/// Windows64用でMonoを有効化する
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Windows64/Switch Mono", priority = 80)]
	internal static bool Execute_Windows64_Switch_Mono()
	{
		// Mono を有効化する
		PlayerSettings.SetScriptingBackend( BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x ) ;

		AssetDatabase.SaveAssets() ;

		EditorUtility.DisplayDialog( "ビルドモード変更", "Windows64 or OSX を Mono でビルドします", "閉じる" ) ;

		return true ;
	}

	/// Windows64用でIL2CPPを有効化する
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Windows64/Switch IL2CPP", priority = 80)]
	internal static bool Execute_Windows64_Switch_IL2CPP()
	{
		// IL2CPP を有効化する
		PlayerSettings.SetScriptingBackend( BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP ) ;

		AssetDatabase.SaveAssets() ;

		EditorUtility.DisplayDialog( "ビルドモード変更", "Windows64 or OSX を IL2CPP でビルドします", "閉じる" ) ;

		return true ;
	}

	//------------------------------------------------------------

	// Windows64 共通
	private static bool Execute_Windows64_General( string productName, string identifier, BuildOptions options )
	{
		BuildTarget		target		= BuildTarget.StandaloneWindows64 ;
		string			path		= m_Path_Windows64 ;

		// 処理時間計測開始
		StartClock() ;

		// ビルドターゲットを変更する
		BuildTarget activeBuildTarget = ChangeBuildTarget( target, "Change" ) ;

		//-----------------------------------------------------------

		// 固有の設定
		PlayerSettings.productName					= productName ;
		PlayerSettings.SetApplicationIdentifier( BuildTargetGroup.Standalone, identifier ) ;

		PlayerSettings.bundleVersion				= VersionName_Windows64 ;

		// ビルドプラットフォームのモジュールをインストールしていないと固有クラスはコンパイルエラーになるのでプリプロセッサで抑制すること
#if UNITY_STANDALONE_WIN
#if false
		DisableUnityAudio( false ) ;	// CriWareに無効化される対策
#endif
#endif
		//-----------------------------------------------------------

		// ビルドを実行する
		bool result = Process( path, target, options ) ;

		// ビルドターゲットを復帰する
		ChangeBuildTarget( activeBuildTarget, "Revert" ) ;

		// 処理時間計測終了
		StopClock() ;

		Debug.Log( "-------> Target : " + target + " " + productName ) ;

		return result ;
	}

	//------------------------------------
	// 固有の設定群

	// ビルドプラットフォームのモジュールをインストールしていないと固有クラスはコンパイルエラーになるのでプリプロセッサで抑制すること
#if UNITY_STANDALONE_WIN
#if false
	// Unityオーディオの再生の有無を設定する
	private static void DisableUnityAudio( bool state )
	{
		string				path		= "ProjectSettings/AudioManager.asset";
		UnityEngine.Object	manager		= AssetDatabase.LoadAllAssetsAtPath( path ).FirstOrDefault() ;
		SerializedObject	target		= new SerializedObject( manager ) ;
		SerializedProperty	property	= target.FindProperty( "m_DisableAudio" ) ;

		property.boolValue = state ;
		target.ApplyModifiedProperties() ;
	}
#endif
#endif
}
