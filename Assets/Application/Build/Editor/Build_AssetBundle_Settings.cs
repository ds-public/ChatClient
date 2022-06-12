/// <summary>
/// アセットバンドルのバッチビルド用クラス(設定部)
/// </summary>
public partial class Build_AssetBundle
{
	// Local Internal
	private const string m_StreamingAssetsListFilePath_Internal		= "Assets/Application/AssetBundle/list_local.txt" ;

	// RemoteAssets ListFilePath Default
	private const string m_RemoteAssetsListFilePath_Default			= "Assets/Application/AssetBundle/list_remote.txt" ;

	// Assets RootFolderPath Default
	private const string m_AssetsRootFolderPath_Default				= "Assets/Application/AssetBundle" ;

	//------------------------------------------------------------

	//------------------------------------------------------------
	// Common

	private const string m_AssetBundleRootFolderPath_StreamingAssets_Common_Internal
		= "Assets/StreamingAssets/Template/Common/Internal" ;

	//------------------------------------------------------------

	// StandaloneWindows

	private const string m_AssetBundleRootFolderPath_StreamingAssets_Windows_Default
		= "Assets/StreamingAssets/Template/Windows/Default" ;

	private const string m_AssetBundleRootFolderPath_RemoteAssets_Windows_Default
		= "AssetBundle/Windows/Default" ;

	//------------------------------------------------------------

	// StandaloneOSX

	private const string m_AssetBundleRootFolderPath_StreamingAssets_OSX_Default
		= "Assets/StreamingAssets/Template/OSX/Default" ;

	private const string m_AssetBundleRootFolderPath_RemoteAssets_OSX_Default
		= "AssetBundle/OSX/Default" ;

	//------------------------------------

	// Android

	private const string m_AssetBundleRootFolderPath_StreamingAssets_Android_Default
		= "Assets/StreamingAssets/Template/Android/Default" ;

	private const string m_AssetBundleRootFolderPath_RemoteAssets_Android_Default
		= "AssetBundle/Android/Default" ;

	//------------------------------------

	// iOS

	private const string m_AssetBundleRootFolderPath_StreamingAssets_iOS_Default
		= "Assets/StreamingAssets/Template/iOS/Default" ;

	private const string m_AssetBundleRootFolderPath_RemoteAssets_iOS_Default
		= "AssetBundle/iOS/Default" ;

	//------------------------------------------------------------
	// Remote Only

	// link.xml
	private const string m_LinkFilePath			= "Assets/link.xml" ;

	//------------------------------------------------------------
}
