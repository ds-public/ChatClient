using System ;
using System.Collections.Generic ;
using System.IO ;

using UnityEngine ;
using UnityEditor ;

using Template ;


/// <summary>
/// アプリケーションのバッチビルド用クラス(OSX用)
/// </summary>
public partial class Build_Application
{
	//--------------------------------------------------------------------------------------------
	// 設定

	//------------------------------------
	// OSX用
	private const string	m_Path_OSX										= "TEMPLATE/template.app" ;

	private static string	VersionName_OSX
	{
		get
		{
			( string versionName, _ ) = GetVersion( PlatformTypes.OSX ) ;
			return versionName ;
		}
	}

	private const string	m_ProductName_OSX_Release						= "TEMPLATE" ;
	private const string	m_ProductName_OSX_Staging						= "TEMPLATE(S)" ;
	private const string	m_ProductName_OSX_Development					= "TEMPLATE(D)" ;
	private const string	m_ProductName_OSX_Profiler						= "TEMPLATE(P)" ;

	private const string	m_Identifier_OSX_Release						= "com.TEMPLATE" ;
	private const string	m_Identifier_OSX_Staging						= "com.TEMPLATE" ;
	private const string	m_Identifier_OSX_Development					= "com.TEMPLATE" ;
	private const string	m_Identifier_OSX_Profiler						= "com.TEMPLATE" ;

	private const string	m_DevelopmentTeam_OSX							= "ABCDEFGHIJ" ;
	private const string	m_CodeSignIdentity_OSX							= "iPhone Distribution: DSW Co.,Ltd." ;
	private const string	m_ProvisioningProfileSpecifier_OSX				= "DSW_Enterprise" ;


	//--------------------------------------------------------------------------------------------
	// 処理

	//--------------------------------------------------------------------------------------------
	// for Mono

	/// <summary>
	/// OSX用(Release)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/OSX/Mono/Release/BuildOnly", priority = 10)]
	internal static bool Execute_OSX_Release_BuildOnly()
	{
		return Execute_OSX_Release( false ) ;
	}

	/// <summary>
	/// OSX用(Release)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/OSX/Mono/Release/BuildAndRun", priority = 11)]
	internal static bool Execute_OSX_Release_BuildAndRun()
	{
		return Execute_OSX_Release( true ) ;
	}

	/// <summary>
	/// OSX用(Release)
	/// </summary>
	/// <returns></returns>
	private static bool Execute_OSX_Release( bool andRun )
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

		BuildOptions	options		= BuildOptions.None ;	// ひとまずビルド速度優先と自動実行
		if( andRun == true )
		{
			options |= BuildOptions.AutoRunPlayer ;
		}

		bool result =  Execute_OSX_General
		(
			m_ProductName_OSX_Release,
			m_Identifier_OSX_Release,
			options
		) ;

		PopSetting() ;	// 設定の復帰

		return result ;
	}

	//----------------

	/// <summary>
	/// OSX用(Staging)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/OSX/Mono/Staging/BuildOnly", priority = 20)]
	internal static bool Execute_OSX_Staging_BuildOnly()
	{
		return Execute_OSX_Staging( false ) ;
	}

	/// <summary>
	/// OSX用(Staging)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/OSX/Mono/Staging/BuildAndRun", priority = 21)]
	internal static bool Execute_OSX_Staging_BuildAndRun()
	{
		return Execute_OSX_Staging( true ) ;
	}

	/// <summary>
	/// OSX用(Staging)
	/// </summary>
	/// <returns></returns>
	private static bool Execute_OSX_Staging( bool andRun )
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

		bool result = Execute_OSX_General
		(
			m_ProductName_OSX_Staging,
			m_Identifier_OSX_Staging,
			options
		) ;

		PopSetting() ;	// 設定の復帰

		return result ;
	}

	//----------------

	/// <summary>
	/// OSX用(Development)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/OSX/Mono/Development(NotDebug)/BuildOnly", priority = 30)]
	internal static bool Execute_OSX_Development_NotDebug_BuildOnly()
	{
		return Execute_OSX_Development( true, false ) ;
	}

	/// <summary>
	/// OSX用(Development)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/OSX/Mono/Development(NotDebug)/BuildAndRun", priority = 31)]
	internal static bool Execute_OSX_Development_NotDebug_BuildAndRun()
	{
		return Execute_OSX_Development( true, true ) ;
	}

	/// <summary>
	/// OSX用(Development)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/OSX/Mono/Development/BuildOnly", priority = 32)]
	internal static bool Execute_OSX_Development_BuildOnly()
	{
		return Execute_OSX_Development( false, false ) ;
	}

	/// <summary>
	/// OSX用(Development)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/OSX/Mono/Development/BuildAndRun", priority = 33)]
	internal static bool Execute_OSX_Development_BuildAndRun()
	{
		return Execute_OSX_Development( false, true ) ;
	}

	/// <summary>
	/// OSX用(Development)
	/// </summary>
	/// <returns></returns>
	private static bool Execute_OSX_Development( bool notDebug, bool andRun )
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

		BuildOptions	options ;

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

		bool result = Execute_OSX_General
		(
			m_ProductName_OSX_Development,
			m_Identifier_OSX_Development,
			options
		) ;

		PopSetting() ;	// 設定の復帰

		return result ;
	}

	//----------------

	/// <summary>
	/// OSX用(Profiler)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/OSX/Mono/Profiler/BuildOnly", priority = 40)]
	internal static bool Execute_OSX_Profiler_BuildOnly()
	{
		return Execute_OSX_Profiler( false ) ;
	}

	/// <summary>
	/// OSX用(Profiler)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/OSX/Mono/Profiler/BuildAndRun", priority = 41)]
	internal static bool Execute_OSX_Profiler_BuildAndRun()
	{
		return Execute_OSX_Profiler( true ) ;
	}

	/// <summary>
	/// OSX用(Profiler)
	/// </summary>
	/// <returns></returns>
	private static bool Execute_OSX_Profiler( bool andRun )
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

		bool result = Execute_OSX_General
		(
			m_ProductName_OSX_Profiler,
			m_Identifier_OSX_Profiler,
			options
		) ;

		PopSetting() ;	// 設定の復帰

		return result ;
	}

	//--------------------------------------------------------------------------------------------
	// for IL2CPP

	/// <summary>
	/// OSX用(Release)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/OSX/IL2CPP/Release/BuildOnly", priority = 10)]
	internal static bool Execute_OSX_IL2CPP_Release_BuildOnly()
	{
		return Execute_OSX_IL2CPP_Release( false ) ;
	}

	/// <summary>
	/// OSX用(Release)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/OSX/IL2CPP/Release/BuildAndRun", priority = 11)]
	internal static bool Execute_OSX_IL2CPP_Release_BuildAndRun()
	{
		return Execute_OSX_IL2CPP_Release( true ) ;
	}

	/// <summary>
	/// OSX用(Release)
	/// </summary>
	/// <returns></returns>
	private static bool Execute_OSX_IL2CPP_Release( bool andRun )
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

		BuildOptions	options		= BuildOptions.None ;	// ひとまずビルド速度優先と自動実行
		if( andRun == true )
		{
			options |= BuildOptions.AutoRunPlayer ;
		}

		bool result =  Execute_OSX_General
		(
			m_ProductName_OSX_Release,
			m_Identifier_OSX_Release,
			options
		) ;

		PopSetting() ;	// 設定の復帰

		return result ;
	}

	//----------------

	/// <summary>
	/// OSX用(Staging)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/OSX/IL2CPP/Staging/BuildOnly", priority = 20)]
	internal static bool Execute_OSX_IL2CPP_Staging_BuildOnly()
	{
		return Execute_OSX_IL2CPP_Staging( false ) ;
	}

	/// <summary>
	/// OSX用(Staging)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/OSX/IL2CPP/Staging/BuildAndRun", priority = 21)]
	internal static bool Execute_OSX_IL2CPPStaging_BuildAndRun()
	{
		return Execute_OSX_IL2CPP_Staging( true ) ;
	}

	// OSX - Staging
	private static bool Execute_OSX_IL2CPP_Staging( bool andRun )
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
	/// OSX用(Development)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/OSX/IL2CPP/Development(NotDebug)/BuildOnly", priority = 30)]
	internal static bool Execute_OSX_IL2CPP_Development_NotDebug_BuildOnly()
	{
		return Execute_OSX_IL2CPP_Development( true, false ) ;
	}

	/// <summary>
	/// Windows64用(Development)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/OSX/IL2CPP/Development(NotDebug)/BuildAndRun", priority = 31)]
	internal static bool Execute_OSX_IL2CPP_Development_NotDebug_BuildAndRun()
	{
		return Execute_OSX_IL2CPP_Development( true, true ) ;
	}

	/// <summary>
	/// OSX用(Development)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/OSX/IL2CPP/Development/BuildOnly", priority = 32)]
	internal static bool Execute_OSX_IL2CPP_Development_BuildOnly()
	{
		return Execute_OSX_IL2CPP_Development( false, false ) ;
	}

	/// <summary>
	/// OSX用(Development)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/OSX/IL2CPP/Development/BuildAndRun", priority = 33)]
	internal static bool Execute_OSX_IL2CPP_Development_BuildAndRun()
	{
		return Execute_OSX_IL2CPP_Development( false, true ) ;
	}

	// OSX - Development
	private static bool Execute_OSX_IL2CPP_Development( bool notDebug, bool andRun )
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

		bool result = Execute_OSX_General
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
	/// OSX用(Profiler)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/OSX/IL2CPP/Profiler/BuildOnly", priority = 40)]
	internal static bool Execute_OSX_IL2CPP_Profiler_BuildOnly()
	{
		return Execute_OSX_IL2CPP_Profiler( false ) ;
	}

	/// <summary>
	/// OSX用(Profiler)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/OSX/IL2CPP/Profiler/BuildAndRun", priority = 41)]
	internal static bool Execute_OSX_IL2CPP_Profiler_BuildAndRun()
	{
		return Execute_OSX_IL2CPP_Profiler( true ) ;
	}

	/// <summary>
	/// OSX用(Profiler)
	/// </summary>
	/// <returns></returns>
	private static bool Execute_OSX_IL2CPP_Profiler( bool andRun )
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

		bool result = Execute_OSX_General
		(
			m_ProductName_OSX_Profiler,
			m_Identifier_OSX_Profiler,
			options
		) ;

		PopSetting() ;	// 設定の復帰

		return result ;
	}

	//--------------------------------------------------------------------------------------------

	/// <summary>
	/// OSX用でMonoを有効化する
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/OSX/Switch Mono", priority = 80)]
	internal static bool Execute_OSX_Switch_Mono()
	{
		// Mono を有効化する
		PlayerSettings.SetScriptingBackend( BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x ) ;

		AssetDatabase.SaveAssets() ;

		EditorUtility.DisplayDialog( "ビルドモード変更", "Windows64 or OSX を Mono でビルドします", "閉じる" ) ;

		return true ;
	}

	/// OSX用でIL2CPPを有効化する
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/OSX/Switch IL2CPP", priority = 80)]
	internal static bool Execute_OSX_Switch_IL2CPP()
	{
		// IL2CPP を有効化する
		PlayerSettings.SetScriptingBackend( BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP ) ;

		AssetDatabase.SaveAssets() ;

		EditorUtility.DisplayDialog( "ビルドモード変更", "Windows64 or OSX を IL2CPP でビルドします", "閉じる" ) ;

		return true ;
	}

	//------------------------------------------------------------

	// OSX 共通
	private static bool Execute_OSX_General( string productName, string identifier, BuildOptions options )
	{
		string			path		= m_Path_OSX ;
		BuildTarget		target		= BuildTarget.StandaloneOSX ;

		// 処理時間計測開始
		StartClock() ;

		// ビルドターゲットを変更する
		BuildTarget activeBuildTarget = ChangeBuildTarget( target, "Change" ) ;

		//-----------------------------------------------------------
		// 固有の設定

		PlayerSettings.productName					= productName ;
		PlayerSettings.SetApplicationIdentifier( BuildTargetGroup.Standalone, identifier ) ;

		PlayerSettings.bundleVersion				= VersionName_OSX ;

		// ビルドプラットフォームのモジュールをインストールしていないと固有クラスはコンパイルエラーになるのでプリプロセッサで抑制すること
#if UNITY_STANDALONE_OSX
		PlayerSettings.macOS.buildNumber			= VersionName_OSX ;
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
#if UNITY_STANDALONE_OSX
#if false
	[PostProcessBuild]
	internal static void OnPostProcessBuild_OSX( BuildTarget buildTarget, string path )
	{
		string projectPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";

		PBXProject pbx = new PBXProject();

		pbx.ReadFromString( File.ReadAllText( projectPath ) ) ;

		string target = pbx.GetUnityMainTargetGuid() ;

		// フレームワークを追加する場合はこちら(Unity2019.3以上)
//		pbx.GetUnityFrameworkTargetGuid() ;

		// 色々設定更新(実際はJenkinsのオプション引数で各種情報を取得)
		pbx.SetBuildProperty( target, "DEVELOPMENT_TEAM",					m_DevelopmentTeam_Machintosh				) ; // チーム名はADCで確認できるPrefix値を設定する
		pbx.SetBuildProperty( target, "CODE_SIGN_IDENTITY",					m_CodeSignIdentity_Machintosh				) ;
		pbx.SetBuildProperty( target, "PROVISIONING_PROFILE_SPECIFIER",		m_ProvisioningProfileSpecifier_Machintosh	) ; // XCode8からProvisioning名で指定できる

		pbx.WriteToFile( projectPath ) ;
	}
#endif
#endif
	//--------------------------------------------------------------------------------------------
}
