#if UNITY_IOS || UNITY_IPHONE
#define BUILD_IOS
#endif

// デバッグ
//#define BUILD_IOS

//-------------------------------------

using System.IO ;

using UnityEngine ;
using UnityEditor ;

using Template ;

#if BUILD_IOS
using UnityEditor.Callbacks ;
using UnityEditor.iOS.Xcode ;
#endif

/// <summary>
/// アプリケーションのバッチビルド用クラス(iOS用)
/// </summary>
public partial class Build_Application
{
	//--------------------------------------------------------------------------------------------
	// 設定

#if BUILD_IOS
	private const string	m_Path_iOS = "Xcode" ;

	private static string	VersionName_iOS
	{
		get
		{
			( string versionName, _ ) = GetVersion( PlatformTypes.iOS ) ;
			return versionName ;
		}
	}

	private static int VersionCode_iOS
	{
		get
		{
			( _, int versionCode ) = GetVersion( PlatformTypes.iOS ) ;
			return versionCode ;
		}
	}

	private const string m_ProductName_iOS_Release     = "TEMPLATE" ;
	private const string m_ProductName_iOS_Review      = "TEMPLATE_R" ;
	private const string m_ProductName_iOS_Staging     = "TEMPLATE_S" ;
	private const string m_ProductName_iOS_Development = "TEMPLATE_D" ;
	private const string m_ProductName_iOS_Profiler    = "TEMPLATE_P" ;

	//TODO: デフォルトのIdentifier
	private const string m_Identifier_iOS_Release      = "com.TEMPLATE" ;
	private const string m_Identifier_iOS_Review       = "com.TEMPLATE" ;
	private const string m_Identifier_iOS_Staging      = "com.TEMPLATE" ;
	private const string m_Identifier_iOS_Development  = "com.TEMPLATE" ;
	private const string m_Identifier_iOS_Profiler     = "vom.TEMPLATE" ;
#endif

	/// <summary>
	/// XCode設定
	/// </summary>
	private sealed class XCodeSettings
	{
		public XCodeSettings
		(
			string					appleDeveloperTeamID,
			string					iOSProvisioningProfileId,
			ProvisioningProfileType	iOSProvisioningProfileType
		)
		{
			AppleDeveloperTeamID			= appleDeveloperTeamID ;
			this.iOSProvisioningProfileId   = iOSProvisioningProfileId ;
			this.iOSProvisioningProfileType	= iOSProvisioningProfileType ;
		}

		public string					AppleDeveloperTeamID       { get ; }
		public string					iOSProvisioningProfileId   { get ; }
		public ProvisioningProfileType	iOSProvisioningProfileType { get ; }
	}

	/// <summary>
	/// 開発用の設定
	/// </summary>
	private static XCodeSettings DevelopmentXCodeSettings { get; } = new XCodeSettings
	(
		appleDeveloperTeamID: "N425JQXF59",
		iOSProvisioningProfileId: "8a940106-7877-4116-9d7e-ac7694be818c",
		iOSProvisioningProfileType: ProvisioningProfileType.Development
	) ;

	
	//--------------------------------------------------------------------------------------------
	// 処理

	/// <summary>
	/// iOS用(Release)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/iOS/IL2CPP/Release/BuildOnly", priority = 10)]
	internal static bool Execute_iOS_Release_BuildOnly()
	{
		return Execute_iOS_Release( false ) ;
	}

	/// <summary>
	/// iOS用(Release)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/iOS/IL2CPP/Release/BuildAndRun", priority = 11)]
	internal static bool Execute_iOS_Release_BuildAndRun()
	{
		return Execute_iOS_Release( true ) ;
	}

	/// <summary>
	/// iOS用(Release)
	/// </summary>
	/// <param name="andRun"></param>
	/// <returns></returns>
	private static bool Execute_iOS_Release( bool andRun )
	{
		var settings = PlayerSettings.GetScriptingBackend( BuildTargetGroup.iOS ) ;
		if( settings != ScriptingImplementation.IL2CPP )
		{
			EditorUtility.DisplayDialog( "注意", "IL2CPP が有効化されてません", "閉じる" ) ;
			return false ;
		}

		//-----------------------------------

		PushSetting() ; // 設定の退避

		SetDefaultEndPoint( Settings.EndPointNames.Release ) ;

		BuildOptions options = BuildOptions.None ; // ひとまずビルド速度優先と自動実行
		if( andRun == true )
		{
			options |= BuildOptions.AutoRunPlayer ;
		}

#if BUILD_IOS
		bool result = Execute_iOS_General
		(
			m_ProductName_iOS_Release,
			m_Identifier_iOS_Release,
			options
		) ;
#else
		bool result = false ;
#endif

		PopSetting() ; // 設定の復帰

		return result ;
	}

	//----------------

	/// <summary>
	/// iOS用(Review)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/iOS/IL2CPP/Review/BuildOnly", priority = 10)]
	internal static bool Execute_iOS_Review_BuildOnly()
	{
		return Execute_iOS_Review( false ) ;
	}

	/// <summary>
	/// iOS用(Review)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/iOS/IL2CPP/Review/BuildAndRun", priority = 11)]
	internal static bool Execute_iOS_Review_BuildAndRun()
	{
		return Execute_iOS_Review( true ) ;
	}

	/// <summary>
	/// iOS用(Review)
	/// </summary>
	/// <param name="andRun"></param>
	/// <returns></returns>
	private static bool Execute_iOS_Review( bool andRun )
	{
		var settings = PlayerSettings.GetScriptingBackend( BuildTargetGroup.iOS ) ;
		if( settings != ScriptingImplementation.IL2CPP )
		{
			EditorUtility.DisplayDialog( "注意", "IL2CPP が有効化されてません", "閉じる" ) ;
			return false ;
		}

		//-----------------------------------

		PushSetting() ; // 設定の退避

		SetDefaultEndPoint( Settings.EndPointNames.Review ) ;

		BuildOptions options = BuildOptions.None ; // ひとまずビルド速度優先と自動実行
		if( andRun == true )
		{
			options |= BuildOptions.AutoRunPlayer ;
		}

#if BUILD_IOS
		bool result = Execute_iOS_General
		(
			m_ProductName_iOS_Review,
			m_Identifier_iOS_Review,
			options
		) ;
#else
		bool result = false ;
#endif

		PopSetting() ; // 設定の復帰

		return result ;
	}

	//----------------

	/// <summary>
	/// iOS用(Staging)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/iOS/IL2CPP/Staging/BuildOnly", priority = 20)]
	internal static bool Execute_iOS_Staging_BuildOnly()
	{
		return Execute_iOS_Staging( false ) ;
	}


	/// <summary>
	/// iOS用(Staging)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/iOS/IL2CPP/Staging/BuildAndRun", priority = 21)]
	internal static bool Execute_iOS_Staging_BuildAndRun()
	{
		return Execute_iOS_Staging( true ) ;
	}


	/// <summary>
	/// iOS用(Staging)
	/// </summary>
	/// <param name="andRun"></param>
	/// <returns></returns>
	internal static bool Execute_iOS_Staging( bool andRun )
	{
		var settings = PlayerSettings.GetScriptingBackend( BuildTargetGroup.iOS ) ;
		if( settings != ScriptingImplementation.IL2CPP )
		{
			EditorUtility.DisplayDialog( "注意", "IL2CPP が有効化されてません", "閉じる" ) ;
			return false ;
		}

		//-----------------------------------

		PushSetting() ; // 設定の退避

		SetDefaultEndPoint( Settings.EndPointNames.Staging ) ;

		BuildOptions options = BuildOptions.None ; // ひとまずビルド速度優先と自動実行
		if( andRun == true )
		{
			options |= BuildOptions.AutoRunPlayer ;
		}

#if BUILD_IOS
		bool result = Execute_iOS_General
		(
			m_ProductName_iOS_Staging,
			m_Identifier_iOS_Staging,
			options
		) ;
#else
		bool result = false ;
#endif
		PopSetting() ; // 設定の復帰

		return result ;
	}

	//----------------

	/// <summary>
	/// iOS用(Development)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/iOS/IL2CPP/Development(NotDebug)/BuildOnly", priority = 30)]
	internal static bool Execute_iOS_Development_NotDebug_BuildOnly()
	{
		return Execute_iOS_Development( true, false ) ;
	}

	/// <summary>
	/// iOS用(Development)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/iOS/IL2CPP/Development(NotDebug)/BuildAndRun", priority = 31)]
	internal static bool Execute_iOS_Development_NotDebug_BuildAndRun()
	{
		return Execute_iOS_Development( true, true ) ;
	}

	/// <summary>
	/// iOS用(Development)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/iOS/IL2CPP/Development/BuildOnly", priority = 32)]
	internal static bool Execute_iOS_Development_BuildOnly()
	{
		return Execute_iOS_Development( false, false ) ;
	}

	/// <summary>
	/// iOS用(Development)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/iOS/IL2CPP/Development/BuildAndRun", priority = 33)]
	internal static bool Execute_iOS_Development_BuildAndRun()
	{
		return Execute_iOS_Development( false, true ) ;
	}

	/// <summary>
	/// iOS用(Development)
	/// </summary>
	/// <param name="andRun"></param>
	/// <returns></returns>
	private static bool Execute_iOS_Development( bool notDebug, bool andRun )
	{
		var settings = PlayerSettings.GetScriptingBackend( BuildTargetGroup.iOS ) ;
		if( settings != ScriptingImplementation.IL2CPP )
		{
			EditorUtility.DisplayDialog( "注意", "IL2CPP が有効化されてません", "閉じる" ) ;
			return false ;
		}

		//-----------------------------------

		PushSetting() ; // 設定の退避

		SetDefaultEndPoint( Settings.EndPointNames.Development ) ;

		BuildOptions options ;

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

#if BUILD_IOS
		bool result = Execute_iOS_General
		(
			m_ProductName_iOS_Development,
			m_Identifier_iOS_Development,
			options
		) ;
#else
		bool result = false ;
#endif

		PopSetting() ; // 設定の復帰

		return result ;
	}

	//----------------

	/// <summary>
	/// iOS用(Profiler)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/iOS/IL2CPP/Profiler/BuildOnly", priority = 40)]
	internal static bool Execute_iOS_Profiler_BuildOnly()
	{
		return Execute_iOS_Profiler( false ) ;
	}

	/// <summary>
	/// iOS用(Profiler)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/iOS/IL2CPP/Profiler/BuildAndRun", priority = 41)]
	internal static bool Execute_iOS_Profiler_BuildAndRun()
	{
		return Execute_iOS_Profiler( true ) ;
	}

	/// <summary>
	/// iOS用(Profiler)
	/// </summary>
	/// <param name="andRun"></param>
	/// <returns></returns>
	private static bool Execute_iOS_Profiler( bool andRun )
	{
		var settings = PlayerSettings.GetScriptingBackend( BuildTargetGroup.iOS ) ;
		if( settings != ScriptingImplementation.IL2CPP )
		{
			EditorUtility.DisplayDialog( "注意", "IL2CPP が有効化されてません", "閉じる" ) ;
			return false ;
		}

		//-----------------------------------

		PushSetting() ; // 設定の退避

		SetDefaultEndPoint( Settings.EndPointNames.Development ) ;

		BuildOptions options	= BuildOptions.Development | BuildOptions.ConnectWithProfiler | BuildOptions.EnableDeepProfilingSupport | BuildOptions.AllowDebugging | BuildOptions.WaitForPlayerConnection ;

		if( andRun == true )
		{
			options |= BuildOptions.AutoRunPlayer ;
		}

#if BUILD_IOS
		bool result = Execute_iOS_General
		(
			m_ProductName_iOS_Profiler,
			m_Identifier_iOS_Profiler,
			options
		) ;
#else
		bool result = false ;
#endif

		PopSetting() ; // 設定の復帰

		return result ;
	}

	//--------------------------------------------------------------------------------------------

	/// <summary>
	/// iOS用でIL2CPPを有効化する
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/iOS/Switch IL2CPP", priority = 80)]
	internal static bool Execute_iOS_Switch_IL2CPP()
	{
		// IL2CPP を有効化する
		PlayerSettings.SetScriptingBackend( BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP ) ;

		AssetDatabase.SaveAssets() ;

		EditorUtility.DisplayDialog( "ビルドモード変更", "iOS を IL2CPP でビルドします", "閉じる" ) ;

		return true ;
	}

	//-------------------------------------------------------------------------------------------------
#if BUILD_IOS
	/// <summary>
	/// iOSビルド設定をしてビルドを行います
	/// </summary>
	/// <param name="productName"></param>
	/// <param name="identifier"></param>
	/// <param name="options"></param>
	/// <returns></returns>
	private static bool Execute_iOS_General
	(
		string        productName,
		string        identifier,
		BuildOptions  options
	)
	{
		string      path   = m_Path_iOS ;
		BuildTarget target = BuildTarget.iOS ;

		// 処理時間計測開始
		StartClock() ;

		// ビルドターゲットを変更する
		BuildTarget activeBuildTarget = ChangeBuildTarget( target, "Change" ) ;

		//-----------------------------------------------------------
		// 固有の設定

		PlayerSettings.productName = productName ;
		PlayerSettings.SetApplicationIdentifier( BuildTargetGroup.iOS, identifier ) ;

		PlayerSettings.bundleVersion = VersionName_iOS ;

		// ビルドプラットフォームのモジュールをインストールしていないと固有クラス(PlayerSettings.iOS)はコンパイルエラーになるのでプリプロセッサで抑制すること
		string oldAppleDeveloperTeamID				= PlayerSettings.iOS.appleDeveloperTeamID ;
		string oldiOSManualProvisioningProfileID	= PlayerSettings.iOS.iOSManualProvisioningProfileID ;
		ProvisioningProfileType oldiOSManualProvisioningProfileType =
			PlayerSettings.iOS.iOSManualProvisioningProfileType ;

		PlayerSettings.iOS.buildNumber						= VersionCode_iOS.ToString() ; // ひとまず ProjectSettings のものをそのまま使用する
		PlayerSettings.iOS.applicationDisplayName			= productName ;
		PlayerSettings.iOS.appleDeveloperTeamID				= null ;
		PlayerSettings.iOS.appleEnableAutomaticSigning		= false ;
		PlayerSettings.iOS.iOSManualProvisioningProfileID	= null ;
		PlayerSettings.iOS.iOSManualProvisioningProfileType	= ProvisioningProfileType.Automatic ;

		//-----------------------------------------------------------

		// ビルドを実行する
		bool result = Process
		(
			path,
			target,
			options
		) ;

		// ビルドターゲットを復帰する
		ChangeBuildTarget( activeBuildTarget, "Revert" ) ;

		// Gitにコミットすべきない情報を元に戻す
		PlayerSettings.iOS.appleDeveloperTeamID				= oldAppleDeveloperTeamID ;
		PlayerSettings.iOS.iOSManualProvisioningProfileID	= oldiOSManualProvisioningProfileID ;
		PlayerSettings.iOS.iOSManualProvisioningProfileType	= oldiOSManualProvisioningProfileType ;

		// 処理時間計測終了
		StopClock() ;

		Debug.Log( "-------> Target : " + target + " " + productName ) ;

		return result ;
	}

	/// <summary>
	/// ビルド後の設定
	/// </summary>
	/// <param name="buildTarget"></param>
	/// <param name="pathToBuiltProject"></param>
	[PostProcessBuild]
	internal static void OnPostProcessBuild
	(
	 	BuildTarget buildTarget,
	 	string      pathToBuiltProject
	)
	{
	 	if( buildTarget != BuildTarget.iOS )
	 	{
	 		return ;
	 	}
	
	 	var path  = pathToBuiltProject + "/Info.plist";
	 	var plist = new PlistDocument() ;
	
	 	plist.ReadFromFile( path ) ;
	 	var root = plist.root ;
	
	 	// ビルド後に出力された Xcode プロジェクトの設定(plist)を書き換える 
	 	//-----------------------------------
	
	 	// ローカライズ対象地域を日本にする(将来、多言語対応になったら、ここに処理を追加する)
	 	root.SetString( "CFBundleDevelopmentRegion", "ja_JP" ) ;
	
	 	//-----------------------------------
	
	 	plist.WriteToFile( path ) ;
	}
#endif
}
