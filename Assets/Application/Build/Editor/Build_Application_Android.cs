#if !UNITY_EDITOR && UNITY_ANDROID

#define BUILD_ANDROID

#endif

//#define BUILD_ANDROID


//---------------------------------------------------------------------------------------------

using System ;
using System.Collections.Generic ;
using System.IO ;

using UnityEngine ;
using UnityEditor ;

using Template ;


//---------------------------------------------------------------------------------------------

/// <summary>
/// アプリケーションのバッチビルド用クラス(Android用)
/// </summary>
public partial class Build_Application
{
	//--------------------------------------------------------------------------------------------
	// 設定

	//------------------------------------
	// Android用
	private const string	m_Path_Android									= "template.apk" ;

	private static string	VersionName_Android
	{
		get
		{
			( string versionName, _ ) = GetVersion( PlatformTypes.Android ) ;
			return versionName ;
		}
	}

#if BUILD_ANDROID
	private static int VersionCode_Android
	{
		get
		{
			( _, int versionCode ) = GetVersion( PlatformTypes.Android ) ;
			return versionCode ;
		}
	}
#endif
	//----------------

	private const string	m_ProductName_Android_Release					= "TEMPLATE" ;
	private const string	m_ProductName_Android_Review					= "TEMPLATE(R)" ;
	private const string	m_ProductName_Android_Staging					= "TEMPLATE(S)" ;
	private const string	m_ProductName_Android_Development				= "TEMPLATE(D)" ;
	private const string	m_ProductName_Android_Profiler					= "TEMPLATE(P)" ;

	private const string	m_Identifier_Android_Release					= "com.TEMPLATE" ;
	private const string	m_Identifier_Android_Review						= "com.TEMPLATE" ;
	private const string	m_Identifier_Android_Staging					= "com.TEMPLATE" ;
	private const string	m_Identifier_Android_Development				= "com.TEMPLATE" ;
	private const string	m_Identifier_Android_Profiler					= "com.TEMPLATE" ;

	//----------------
#if BUILD_ANDROID
	private const string	m_KeyStorePath_Android							= "Build/Android/template.keystore" ;
	private const string	m_KeyStorePassword_Android						= "template_pw" ;
	private const string	m_KeyStoreAlias_Android							= "template_alias" ;
	private const string	m_KeyStoreAliasPassword_Android					= "template_pw" ;
#endif

	//--------------------------------------------------------------------------------------------
	// 処理

	//--------------------------------------------------------------------------------------------
	// for Mono

	/// <summary>
	/// Android用(Release) - Mono
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Android/Mono/Release/BuildOnly", priority = 10)]
	internal static bool Execute_Android_Release_BuildOnly()
	{
		return Execute_Android_Release( false ) ;
	}

	/// <summary>
	/// Android用(Release) - Mono
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Android/Mono/Release/BuildAndRun", priority = 11)]
	internal static bool Execute_Android_Release_BuildAndRun()
	{
		return Execute_Android_Release( true ) ;
	}

	// Android - Release
	private static bool Execute_Android_Release( bool andRun )
	{
		var settings = PlayerSettings.GetScriptingBackend( BuildTargetGroup.Android ) ;
		if( settings != ScriptingImplementation.Mono2x )
		{
			EditorUtility.DisplayDialog( "注意", "Mono が有効化されてません", "閉じる" ) ;
			return false ;
		}

		if( PlayerSettings.Android.targetArchitectures != AndroidArchitecture.ARMv7 )
		{
			EditorUtility.DisplayDialog( "注意", "アーキテクチャは ARM7 を指定している必要があります", "閉じる" ) ;
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

		bool result = Execute_Android_General
		(
			m_ProductName_Android_Release,
			m_Identifier_Android_Release,
			options,
			false	// AAB 化しない
		) ;

		PopSetting() ;	// 設定の復帰

		return result ;
	}

	//----------------

	/// <summary>
	/// Android用(Review) - Mono
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Android/Mono/Review/BuildOnly", priority = 10)]
	internal static bool Execute_Android_Review_BuildOnly()
	{
		return Execute_Android_Review( false ) ;
	}

	/// <summary>
	/// Android用(Review) - Mono
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Android/Mono/Review/BuildAndRun", priority = 11)]
	internal static bool Execute_Android_Review_BuildAndRun()
	{
		return Execute_Android_Review( true ) ;
	}

	// Android - Review
	private static bool Execute_Android_Review( bool andRun )
	{
		var settings = PlayerSettings.GetScriptingBackend( BuildTargetGroup.Android ) ;
		if( settings != ScriptingImplementation.Mono2x )
		{
			EditorUtility.DisplayDialog( "注意", "Mono が有効化されてません", "閉じる" ) ;
			return false ;
		}

		if( PlayerSettings.Android.targetArchitectures != AndroidArchitecture.ARMv7 )
		{
			EditorUtility.DisplayDialog( "注意", "アーキテクチャは ARM7 を指定している必要があります", "閉じる" ) ;
			return false ;
		}

		//-----------------------------------

		PushSetting() ;	// 設定の退避

		SetDefaultEndPoint( Settings.EndPointNames.Review ) ;

		BuildOptions	options		= BuildOptions.None ;	// ひとまずビルド速度優先と自動実行
		if( andRun == true )
		{
			options |= BuildOptions.AutoRunPlayer ;
		}

		bool result = Execute_Android_General
		(
			m_ProductName_Android_Review,
			m_Identifier_Android_Review,
			options,
			false	// AAB 化しない
		) ;

		PopSetting() ;	// 設定の復帰

		return result ;
	}

	//----------------

	/// <summary>
	/// Android用(Staging) - Mono
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Android/Mono/Staging/BuildOnly", priority = 20)]
	internal static bool Execute_Android_Staging_BuildOnly()
	{
		return Execute_Android_Staging( false ) ;
	}

	/// <summary>
	/// Android用(Staging) - Mono
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Android/Mono/Staging/BuildAndRun", priority = 21)]
	internal static bool Execute_Android_Staging_BuildAndRun()
	{
		return Execute_Android_Staging( true ) ;
	}

	// Android - Staging
	private static bool Execute_Android_Staging( bool andRun )
	{
		var settings = PlayerSettings.GetScriptingBackend( BuildTargetGroup.Android ) ;
		if( settings != ScriptingImplementation.Mono2x )
		{
			EditorUtility.DisplayDialog( "注意", "Mono が有効化されてません", "閉じる" ) ;
			return false ;
		}

		if( PlayerSettings.Android.targetArchitectures != AndroidArchitecture.ARMv7 )
		{
			EditorUtility.DisplayDialog( "注意", "アーキテクチャは ARM7 を指定している必要があります", "閉じる" ) ;
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

		bool result = Execute_Android_General
		(
			m_ProductName_Android_Staging,
			m_Identifier_Android_Staging,
			options,
			false	// AAB 化しない
		) ;

		PopSetting() ;	// 設定の復帰

		return result ;
	}

	//----------------

	/// <summary>
	/// Android用(Development NoDebug) - Mono
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Android/Mono/Development(NotDebug)/BuildOnly", priority = 30)]
	internal static bool Execute_Android_Development_NotDebug_BuildOnly()
	{
		return Execute_Android_Development( true, false ) ;
	}

	/// <summary>
	/// Android用(Development NoDebug) - Mono
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Android/Mono/Development(NotDebug)/BuildAndRun", priority = 31)]
	internal static bool Execute_Android_Development_NotDebug_BuildAndRun()
	{
		return Execute_Android_Development( true, true ) ;
	}

	/// <summary>
	/// Android用(Development) - Mono
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Android/Mono/Development/BuildOnly", priority = 32)]
	internal static bool Execute_Android_Development_BuildOnly()
	{
		return Execute_Android_Development( false, false ) ;
	}

	/// <summary>
	/// Android用(Development) - Mono
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Android/Mono/Development/BuildAndRun", priority = 33)]
	internal static bool Execute_Android_Development_BuildAndRun()
	{
		return Execute_Android_Development( false, true ) ;
	}

	// Android - Development
	private static bool Execute_Android_Development( bool notDebug, bool andRun )
	{
		var settings = PlayerSettings.GetScriptingBackend( BuildTargetGroup.Android ) ;
		if( settings != ScriptingImplementation.Mono2x )
		{
			EditorUtility.DisplayDialog( "注意", "Mono が有効化されてません", "閉じる" ) ;
			return false ;
		}

		if( PlayerSettings.Android.targetArchitectures != AndroidArchitecture.ARMv7 )
		{
			EditorUtility.DisplayDialog( "注意", "アーキテクチャは ARM7 を指定している必要があります", "閉じる" ) ;
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

		bool result = Execute_Android_General
		(
			m_ProductName_Android_Development,
			m_Identifier_Android_Development,
			options,
			false	// AAB 化しない
		) ;

		PopSetting() ;	// 設定の復帰

		return result ;
	}

	//----------------

	/// <summary>
	/// Android用(Profiler) - Mono
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Android/Mono/Profiler/BuildOnly", priority = 40)]
	internal static bool Execute_Android_Profiler_BuildOnly()
	{
		return Execute_Android_Profiler( false ) ;
	}

	/// <summary>
	/// Android用(Profiler) - Mono
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Android/Mono/Profiler/BuildAndRun", priority = 41)]
	internal static bool Execute_Android_Profiler_BuildAndRun()
	{
		return Execute_Android_Profiler( true ) ;
	}

	private static bool Execute_Android_Profiler( bool andRun )
	{
		var settings = PlayerSettings.GetScriptingBackend( BuildTargetGroup.Android ) ;
		if( settings != ScriptingImplementation.Mono2x )
		{
			EditorUtility.DisplayDialog( "注意", "Mono が有効化されてません", "閉じる" ) ;
			return false ;
		}

		if( PlayerSettings.Android.targetArchitectures != AndroidArchitecture.ARMv7 )
		{
			EditorUtility.DisplayDialog( "注意", "アーキテクチャは ARM7 を指定している必要があります", "閉じる" ) ;
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

		bool result = Execute_Android_General
		(
			m_ProductName_Android_Profiler,
			m_Identifier_Android_Profiler,
			options,
			false	// AAB 化しない
		) ;

		PopSetting() ;	// 設定の復帰

		return result ;
	}

	//--------------------------------------------------------------------------------------------
	// for IL2CPP

	/// <summary>
	/// Android用(Release) - IL2CPP
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Android/IL2CPP/Release/BuildOnly", priority = 10)]
	internal static bool Execute_Android_IL2CPP_Release_BuildOnly()
	{
		return Execute_Android_IL2CPP_Release( false ) ;
	}

	/// <summary>
	/// Android用(Release) - IL2CPP
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Android/IL2CPP/Release/BuildAndRun", priority = 11)]
	internal static bool Execute_Android_IL2CPP_Release_BuildAndRun()
	{
		return Execute_Android_IL2CPP_Release( true ) ;
	}

	// Android - Release
	private static bool Execute_Android_IL2CPP_Release( bool andRun )
	{
		var settings = PlayerSettings.GetScriptingBackend( BuildTargetGroup.Android ) ;
		if( settings != ScriptingImplementation.IL2CPP )
		{
			EditorUtility.DisplayDialog( "注意", "IL2CPP が有効化されてません", "閉じる" ) ;
			return false ;
		}

		if( PlayerSettings.Android.targetArchitectures != AndroidArchitecture.ARM64 )
		{
			EditorUtility.DisplayDialog( "注意", "アーキテクチャは ARM64 を指定している必要があります", "閉じる" ) ;
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

		bool result = Execute_Android_General
		(
			m_ProductName_Android_Release,
			m_Identifier_Android_Release,
			options,
			true	// AAB 化する
		) ;

		PopSetting() ;	// 設定の復帰

		return result ;
	}

	//----------------

	/// <summary>
	/// Android用(Review) - IL2CPP
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Android/IL2CPP/Review/BuildOnly", priority = 10)]
	internal static bool Execute_Android_IL2CPP_Review_BuildOnly()
	{
		return Execute_Android_IL2CPP_Review( false ) ;
	}

	/// <summary>
	/// Android用(Review) - IL2CPP
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Android/IL2CPP/Review/BuildAndRun", priority = 11)]
	internal static bool Execute_Android_IL2CPP_Review_BuildAndRun()
	{
		return Execute_Android_IL2CPP_Review( true ) ;
	}

	// Android - Review
	private static bool Execute_Android_IL2CPP_Review( bool andRun )
	{
		var settings = PlayerSettings.GetScriptingBackend( BuildTargetGroup.Android ) ;
		if( settings != ScriptingImplementation.IL2CPP )
		{
			EditorUtility.DisplayDialog( "注意", "IL2CPP が有効化されてません", "閉じる" ) ;
			return false ;
		}

		if( PlayerSettings.Android.targetArchitectures != AndroidArchitecture.ARM64 )
		{
			EditorUtility.DisplayDialog( "注意", "アーキテクチャは ARM64 を指定している必要があります", "閉じる" ) ;
			return false ;
		}

		//-----------------------------------

		PushSetting() ;	// 設定の退避

		SetDefaultEndPoint( Settings.EndPointNames.Review ) ;

		BuildOptions	options		= BuildOptions.None ;	// ひとまずビルド速度優先と自動実行
		if( andRun == true )
		{
			options |= BuildOptions.AutoRunPlayer ;
		}

		bool result = Execute_Android_General
		(
			m_ProductName_Android_Review,
			m_Identifier_Android_Review,
			options,
			true	// AAB 化する
		) ;

		PopSetting() ;	// 設定の復帰

		return result ;
	}

	//----------------

	/// <summary>
	/// Android用(Staging) - IL2CPP
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Android/IL2CPP/Staging/BuildOnly", priority = 20)]
	internal static bool Execute_Android_IL2CPP_Staging_BuildOnly()
	{
		return Execute_Android_IL2CPP_Staging( false ) ;
	}

	/// <summary>
	/// Android用(Staging) - IL2CPP
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Android/IL2CPP/Staging/BuildAndRun", priority = 21)]
	internal static bool Execute_Android_IL2CPPStaging_BuildAndRun()
	{
		return Execute_Android_IL2CPP_Staging( true ) ;
	}

	// Android - Staging
	private static bool Execute_Android_IL2CPP_Staging( bool andRun )
	{
		var settings = PlayerSettings.GetScriptingBackend( BuildTargetGroup.Android ) ;
		if( settings != ScriptingImplementation.IL2CPP )
		{
			EditorUtility.DisplayDialog( "注意", "IL2CPP が有効化されてません", "閉じる" ) ;
			return false ;
		}

		if( PlayerSettings.Android.targetArchitectures != AndroidArchitecture.ARM64 )
		{
			EditorUtility.DisplayDialog( "注意", "アーキテクチャは ARM64 を指定している必要があります", "閉じる" ) ;
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

		bool result = Execute_Android_General
		(
			m_ProductName_Android_Staging,
			m_Identifier_Android_Staging,
			options,
			true	// AAB 化する
		) ;

		PopSetting() ;	// 設定の復帰

		return result ;
	}

	//----------------

	/// <summary>
	/// Android用(Development NoDebug) - IL2CPP
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Android/IL2CPP/Development(NotDebug)/BuildOnly", priority = 30)]
	internal static bool Execute_Android_IL2CPP_Development_NotDebug_BuildOnly()
	{
		return Execute_Android_IL2CPP_Development( true, false ) ;
	}

	/// <summary>
	/// Android用(Development NoDebug) - IL2CPP
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Android/IL2CPP/Development(NotDebug)/BuildAndRun", priority = 31)]
	internal static bool Execute_Android_IL2CPP_Development_NotDebug_BuildAndRun()
	{
		return Execute_Android_IL2CPP_Development( true, true ) ;
	}

	/// <summary>
	/// Android用(Development) - IL2CPP でビルド出来ないので一旦閉じる
	/// </summary>
	/// <returns></returns>
//	[MenuItem("Build/Application/Android/IL2CPP/Development/BuildOnly", priority = 32)]
	internal static bool Execute_Android_IL2CPP_Development_BuildOnly()
	{
		return Execute_Android_IL2CPP_Development( false, false ) ;
	}

	/// <summary>
	/// Android用(Development) - IL2CPP でビルド出来ないので一旦閉じる
	/// </summary>
	/// <returns></returns>
//	[MenuItem("Build/Application/Android/IL2CPP/Development/BuildAndRun", priority = 33)]
	internal static bool Execute_Android_IL2CPP_Development_BuildAndRun()
	{
		return Execute_Android_IL2CPP_Development( false, true ) ;
	}

	// Android - Development
	private static bool Execute_Android_IL2CPP_Development( bool notDebug, bool andRun )
	{
		var settings = PlayerSettings.GetScriptingBackend( BuildTargetGroup.Android ) ;
		if( settings != ScriptingImplementation.IL2CPP )
		{
			EditorUtility.DisplayDialog( "注意", "IL2CPP が有効化されてません", "閉じる" ) ;
			return false ;
		}

		if( PlayerSettings.Android.targetArchitectures != AndroidArchitecture.ARM64 )
		{
			EditorUtility.DisplayDialog( "注意", "アーキテクチャは ARM64 を指定している必要があります", "閉じる" ) ;
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

		bool result = Execute_Android_General
		(
			m_ProductName_Android_Development,
			m_Identifier_Android_Development,
			options,
			true	// AAB 化する
		) ;

		PopSetting() ;	// 設定の復帰

		//-----------------------------------

		return result ;
	}

	//----------------

	/// <summary>
	/// Android用(Profiler) - IL2CPP でビルド出来ないので一旦閉じる
	/// </summary>
	/// <returns></returns>
//	[MenuItem("Build/Application/Android/IL2CPP/Profiler/BuildOnly", priority = 40)]
	internal static bool Execute_Android_IL2CPP_Profiler_BuildOnly()
	{
		return Execute_Android_IL2CPP_Profiler( false ) ;
	}

	/// <summary>
	/// Android用(Profiler) - IL2CPP でビルド出来ないので一旦閉じる
	/// </summary>
	/// <returns></returns>
//	[MenuItem("Build/Application/Android/IL2CPP/Profiler/BuildAndRun", priority = 41)]
	internal static bool Execute_Android_IL2CPP_Profiler_BuildAndRun()
	{
		return Execute_Android_IL2CPP_Profiler( true ) ;
	}

	private static bool Execute_Android_IL2CPP_Profiler( bool andRun )
	{
		var settings = PlayerSettings.GetScriptingBackend( BuildTargetGroup.Android ) ;
		if( settings != ScriptingImplementation.IL2CPP )
		{
			EditorUtility.DisplayDialog( "注意", "IL2CPP が有効化されてません", "閉じる" ) ;
			return false ;
		}

		if( PlayerSettings.Android.targetArchitectures != AndroidArchitecture.ARM64 )
		{
			EditorUtility.DisplayDialog( "注意", "アーキテクチャは ARM64 を指定している必要があります", "閉じる" ) ;
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

		bool result = Execute_Android_General
		(
			m_ProductName_Android_Profiler,
			m_Identifier_Android_Profiler,
			options,
			true	// AAB 化する
		) ;

		PopSetting() ;	// 設定の復帰

		return result ;
	}

	//--------------------------------------------------------------------------------------------

	/// <summary>
	/// Android用でMonoを有効化する
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Android/Switch Mono", priority = 80)]
	internal static bool Execute_Android_Switch_Mono()
	{
		// Mono を有効化する
		PlayerSettings.SetScriptingBackend( BuildTargetGroup.Android, ScriptingImplementation.Mono2x ) ;
		PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 ;

		AssetDatabase.SaveAssets() ;

		EditorUtility.DisplayDialog( "ビルドモード変更", "Android を Mono でビルドします", "閉じる" ) ;

		return true ;
	}

	/// <summary>
	/// Android用でIL2CPPを有効化する
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/Android/Switch IL2CPP", priority = 80)]
	internal static bool Execute_Android_Switch_IL2CPP()
	{
		// IL2CPP を有効化する
		PlayerSettings.SetScriptingBackend( BuildTargetGroup.Android, ScriptingImplementation.IL2CPP ) ;
		PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 ;

		AssetDatabase.SaveAssets() ;

		EditorUtility.DisplayDialog( "ビルドモード変更", "Android を IL2CPP でビルドします", "閉じる" ) ;

		return true ;
	}


	//--------------------------------------------------------------------------------------------
	// Android 共通
	private static bool Execute_Android_General( string productName, string identifier, BuildOptions options, bool isAAB )
	{
		string			path		= m_Path_Android ;
		BuildTarget		target		= BuildTarget.Android ;

		// 処理時間計測開始
		StartClock() ;

		// ビルドターゲットを変更する
		BuildTarget activeBuildTarget = ChangeBuildTarget( target, "Change" ) ;

		//-----------------------------------------------------------
		// 固有の設定

		PlayerSettings.productName					= productName ;
		PlayerSettings.SetApplicationIdentifier( BuildTargetGroup.Android, identifier ) ;

		PlayerSettings.bundleVersion				= VersionName_Android ;

		// ビルドプラットフォームのモジュールをインストールしていないと固有クラスはコンパイルエラーになるのでプリプロセッサで抑制すること
#if BUILD_ANDROID
		PlayerSettings.Android.bundleVersionCode	= VersionCode_Android ;	// ひとまず ProjectSettings のものをそのまま使用する
		SetAndroidKeyStore() ;  // KeyStoreはここで強制的に設定する(パスワードを毎回入力するのが面倒なため)

		// Build App Bundle (Google Play) を有効にするか
		EditorUserBuildSettings.buildAppBundle = isAAB ;

		if( isAAB == true )
		{
			path = path.Replace( "apk", "aab" ) ;
		}
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
#if BUILD_ANDROID
	// Android限定
	private static void SetAndroidKeyStore()
	{
		// カスタムを有効にしないと以下の設定が反映されない
		PlayerSettings.Android.useCustomKeystore = true ;

		// keystoreファイルのパス
		PlayerSettings.Android.keystoreName = Path.Combine( Directory.GetCurrentDirectory(), m_KeyStorePath_Android ).Replace( "\\", "/" ) ;

		// keystore作成時に設定したkestoreのパスワード
		PlayerSettings.Android.keystorePass = m_KeyStorePassword_Android ;

		// keystore作成時に設定したalias名
		PlayerSettings.Android.keyaliasName = m_KeyStoreAlias_Android ;

		// keystore作成時に設定したaliasのパスワード
		PlayerSettings.Android.keyaliasPass = m_KeyStoreAliasPassword_Android ;
	}
#endif
	//--------------------------------------------------------------------------------------------
}
