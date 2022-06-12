using System ;
using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;

namespace TimeHelper
{
	/// <summary>
	/// 時間計測クラス Version 2022/04/28
	/// </summary>
	public class TimeWatch
	{
		// 計測識別子
		private static long m_WatchIdentity = 0 ;

		// 計測開始時間
		private static Dictionary<long,float> m_StartTimes = new Dictionary<long,float>() ;

		/// <summary>
		/// スタックを消去する
		/// </summary>
		public static void Clear()
		{
			m_StartTimes.Clear() ;
		}

		/// <summary>
		/// 計測を開始する
		/// </summary>
		public static long Start( bool isClear = false )
		{
			if( isClear == true )
			{
				m_StartTimes.Clear() ;
			}

			//----------------------------------

			float startTime = Time.realtimeSinceStartup ;

			long watchIdentity = m_WatchIdentity ;
			m_StartTimes.Add( m_WatchIdentity, startTime ) ;
			m_WatchIdentity ++ ;

			return watchIdentity ;
		}

		/// <summary>
		/// 時間計測を終了する
		/// </summary>
		public static float Stop( long watchIdentity )
		{
			if( m_StartTimes.Count == 0 )
			{
				Debug.LogWarning( "時間計測は開始されていません" ) ;
				return -1 ;	// 異常
			}

			if( m_StartTimes.ContainsKey( watchIdentity ) == false )
			{
				Debug.LogWarning( "計測識別子の値が異常です : " + watchIdentity ) ;
				return -1 ;	// 異常
			}

			//----------------------------------

			float startTime = m_StartTimes[ watchIdentity ] ;
			m_StartTimes.Remove( watchIdentity ) ;

			float deltatTime = ( Time.realtimeSinceStartup - startTime ) ;
			if( deltatTime <  0 )
			{
				deltatTime  = 0 ;
			}

			return deltatTime ;
		}

		/// <summary>
		/// 時間計測を終了する
		/// </summary>
		/// <returns></returns>
		public static string StopToString( long watchIdentity, string label = null, bool isOut = true )
		{
			float deltaTime = Stop( watchIdentity ) ;
			if( deltaTime <  0 )
			{
				return "<color=#FFFF00>時間計測は開始されていません" ;
			}

			//----------------------------------

			string time = string.Empty ;

			if( deltaTime >= 1.0f )
			{
				if( deltaTime >= ( 60 * 60 ) )
				{
					int hour = ( int )( deltaTime / ( 60 * 60 ) ) ;

					time += hour.ToString() + "時間" ;

					deltaTime %= ( 60 * 60 ) ;
				}

				if( deltaTime >= 60 )
				{
					int minute = ( int )( deltaTime / 60 ) ;

					time += minute.ToString() + "分" ;

					deltaTime %= 60 ;
				}

				int second = ( int )deltaTime ;

				time += second.ToString() + "秒" ;
			}
			else
			{
				double millisecond = ( double )( ( int )( deltaTime * 1000.0f ) ) / ( double )1000.0f ;

				time = millisecond.ToString() + "秒" ;
			}

			if( string.IsNullOrEmpty( label ) == true )
			{
				label = string.Empty ;
			}
			else
			{
				label = "< " + label + " > " ;
			}

			time = "<color=#FF7FFF>******* " + label + "計測結果 : " + time + " *******</color>" ;

			if( isOut == true )
			{
				Debug.Log( time ) ;
				uGUIHelper.DebugScreen.Out( time ) ;
			}

			return time ;
		}
	}
}
