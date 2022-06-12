// 実機且つリリースビルドの際はデバッグログを抑制する Version 2021/03/09
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD

using System ;
using System.Diagnostics ;


public static class Debug
{
	public static bool developerConsoleVisible ;

	public static bool isDebugBuild ;

	public static UnityEngine.ILogger unityLogger ;

	//------------------------------------------------------------

	[Conditional("DEVELOPMENT_BUILD")]
	public static void Assert( bool condition )
	{
	}

	[Conditional("DEVELOPMENT_BUILD")]
	public static void Assert( bool condition, object message )
	{
	}

	[Conditional("DEVELOPMENT_BUILD")]
	public static void Assert( bool condition, UnityEngine.Object context )
	{
	}

	[Conditional("DEVELOPMENT_BUILD")]
	public static void Assert( bool condition, object message, UnityEngine.Object context )
	{
	}

	[Conditional("DEVELOPMENT_BUILD")]
	public static void Assert( bool condition, string format, params object[] args )
	{
	}

	[Conditional("DEVELOPMENT_BUILD")]
	public static void AssertFormat( bool condition, string format, params object[] args )
	{
	}

	[Conditional("DEVELOPMENT_BUILD")]
	public static void AssertFormat( bool condition, UnityEngine.Object context, string format, params object[] args )
	{
	}

	[Conditional("DEVELOPMENT_BUILD")]
	public static void Break()
	{
	}

	[Conditional("DEVELOPMENT_BUILD")]
	public static void  ClearDeveloperConsole()
	{
	}

	//------------------------------------

	[Conditional("DEVELOPMENT_BUILD")]
	public static void Log( object message, UnityEngine.Object context = null )
	{
	}

	[Conditional("DEVELOPMENT_BUILD")]
	public static void LogFormat( string format, params object[] args )
	{
	}

	[Conditional("DEVELOPMENT_BUILD")]
	public static void LogFormat( UnityEngine.Object context, string format, params object[] args )
	{
	}

	[Conditional("DEVELOPMENT_BUILD")]
	public static void LogFormat( UnityEngine.LogType logType, UnityEngine.LogOption logOption, UnityEngine.Object context, string format, params object[] args )
	{
	}

	[Conditional("DEVELOPMENT_BUILD")]
	public static void LogWarning( object message, UnityEngine.Object context = null )
	{
	}

	[Conditional("DEVELOPMENT_BUILD")]
	public static void LogWarningFormat( string format, params object[] args )
	{
	}

	[Conditional("DEVELOPMENT_BUILD")]
	public static void LogWarningFormat( UnityEngine.Object context, string format, params object[] args )
	{
	}

	[Conditional("DEVELOPMENT_BUILD")]
	public static void LogError( object message, UnityEngine.Object context = null )
	{
	}

	[Conditional("DEVELOPMENT_BUILD")]
	public static void LogErrorFormat( string format, params object[] args )
	{
	}

	[Conditional("DEVELOPMENT_BUILD")]
	public static void LogErrorFormat( UnityEngine.Object context, string format, params object[] args )
	{
	}

	[Conditional("DEVELOPMENT_BUILD")]
	public static void LogException( Exception exception, UnityEngine.Object context = null )
	{
	}

	[Conditional("DEVELOPMENT_BUILD")]
	public static void LogAssertion( object message, UnityEngine.Object context = null )
	{
	}

	[Conditional("DEVELOPMENT_BUILD")]
	public static void LogAssertionFormat( string format, params object[] args )
	{
	}

	[Conditional("DEVELOPMENT_BUILD")]
	public static void LogAssertionFormat( UnityEngine.Object context, string format, params object[] args )
	{
	}

	[Conditional("DEVELOPMENT_BUILD")]
	public static void  DrawLine( UnityEngine.Vector3 start, UnityEngine.Vector3 end, UnityEngine.Color color, float duration = 0.0f, bool depthTest = true  )
	{
	}

	[Conditional("DEVELOPMENT_BUILD")]
	public static void  DrawDay( UnityEngine.Vector3 start, UnityEngine.Vector3 dir, UnityEngine.Color color, float duration = 0.0f, bool depthTest = true  )
	{
	}

	// 以下は廃止されたかもしれない

	[Conditional("DEVELOPMENT_BUILD")]
	public static void LogAssert( bool condition )
	{
	}

	[Conditional("DEVELOPMENT_BUILD")]
	public static void LogAssert( bool condition, object message )
	{
	}

	[Conditional("DEVELOPMENT_BUILD")]
	public static void LogAssert( bool condition, UnityEngine.Object context )
	{
	}

	[Conditional("DEVELOPMENT_BUILD")]
	public static void LogAssert( bool condition, object message, UnityEngine.Object context )
	{
	}

	[Conditional("DEVELOPMENT_BUILD")]
	public static void Fail( string message, UnityEngine.Object context = null )
	{
	}
}
#endif
