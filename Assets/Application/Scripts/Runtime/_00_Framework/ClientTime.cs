using System ;
using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;

namespace Template
{
	/// <summary>
	/// 端末時刻の取得クラス Version 2022/05/01
	/// </summary>
	public class ClientTime
	{
		private static readonly DateTime UNIX_EPOCH = new DateTime( 1970, 1, 1, 0, 0, 0, 0 ) ;

		private static int m_CorrectTime = 0 ;  // サーバータイムとクライアントタイムの補正用の差分値

		/// <summary>
		/// 現在時刻(JST)を取得する
		/// </summary>
		/// <returns></returns>
		public static int GetRealCurrentUnixTime()
		{
			return GetUnixTime( DateTime.Now ) ;
		}

		/// <summary>
		/// サーバータイムとクライアントタイムの補正用の差分値を保存する
		/// </summary>
		/// <param name="correctTime"></param>
		public static void SetCorrectTime( int correctTime )
		{
			m_CorrectTime = correctTime ;
		}

		/// <summary>
		/// 現在時刻(JST)を取得する
		/// </summary>
		/// <returns></returns>
		public static int GetCurrentUnixTime()
		{
			return GetUnixTime( DateTime.Now, m_CorrectTime ) ;
		}

		private static int GetUnixTime( DateTime targetTime, int correctTime = 0 )
		{
			// UTC時間に変換
			targetTime = targetTime.ToUniversalTime() ;

			// UNIXエポックからの経過時間を取得
			TimeSpan elapsedTime = targetTime - UNIX_EPOCH ;

			// 経過秒数に変換
			return ( int )elapsedTime.TotalSeconds + correctTime ;
		}

		public static int GetTodayUnixTime( long unixTime = 0 )
		{
			if( unixTime >  0 )
			{
				DateTime now = UnixTimeToDateTime( unixTime ) ;
				return GetUnixTime( new DateTime( now.Year, now.Month, now.Day, 0, 0, 0 ) ) ;
			}
			else
			{
				return GetUnixTime( DateTime.Today ) ;
			}
		}

		/// <summary>
		/// 日時クラスのインスタンスを取得する
		/// </summary>
		/// <param name="tUnixTime"></param>
		/// <returns></returns>
		public static DateTime UnixTimeToDateTime( long unixTime = 0 )
		{
			if( unixTime == 0 )
			{
				unixTime = GetCurrentUnixTime() ;
			}

			TimeSpan elapsedTime = new TimeSpan( 0, 0, ( int )unixTime ) ;
			
			return new DateTime( elapsedTime.Ticks + UNIX_EPOCH.Ticks ).ToLocalTime() ;
		}


		/// <summary>
		/// 日時文字列を取得する(デバッグ用)
		/// </summary>
		/// <param name="tUnixTime"></param>
		/// <returns></returns>
		public static string GetDateTimeString( long unixTime  = 0 )
		{
			if( unixTime == 0 )
			{
				unixTime = GetCurrentUnixTime() ;
			}

			DateTime dt = UnixTimeToDateTime( unixTime ) ;

			return string.Format( "{0,0:D4}/{1,0:D2}/{2,0:D2} {3,0:D2}:{4,0:D2}:{5,0:D2}", dt.Year, dt.Month, dt.DayOfYear, dt.Hour, dt.Minute, dt.Second ) ;
		}
	}
}
