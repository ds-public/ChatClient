using UnityEngine ;
using System.Runtime.InteropServices ;
 

/// <summary>
/// ストレージ確認
/// </summary>
public class StorageMonitor
{
#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern long _GetAvailableStorage() ;
#endif
 
	/// <summary>
	/// ストレージの空き容量を取得します
	/// </summary>
	public static long GetFree()
	{
		long free = 0 ;
#if UNITY_EDITOR
		free = 0 ;
#elif UNITY_ANDROID
		free = GetAvailableStorage() ;
#elif UNITY_IOS
		free = GetAvailableStorage() ;
#else
		// 未実装
		Debug.Assert( false ) ;
#endif
		return free ;
	}
 
#if !UNITY_EDITOR && UNITY_ANDROID
	private static long GetAvailableStorage()
	{
		var statFs = new AndroidJavaObject( "android.os.StatFs", Application.temporaryCachePath ) ;
		var availableBlocks = statFs.Call<long>( "getAvailableBlocksLong" ) ;
		var blockSize = statFs.Call<long>( "getBlockSizeLong" ) ;
		var freeBytes = availableBlocks * blockSize ;
		return freeBytes ;
	}
 
#elif !UNITY_EDITOR && UNITY_IOS
	private static long GetAvailableStorage()
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
		{
			return _GetAvailableStorage() ;
		}
		throw new System.NotSupportedException() ;
	}
#endif
}
