using System ;
using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;

using Cysharp.Threading.Tasks ;

using uGUIHelper ;

namespace Template
{
	/// <summary>
	/// サーバーの時刻制御クラス Version 2022/02/17
	/// </summary>
	public class ServerTime : ExMonoBehaviour
	{
		private static ServerTime	m_Instance ;
		internal void Awake()
		{
			m_Instance = this ;
		}
		internal void OnDestroy()
		{
			m_Instance = null ;
		}

#if UNITY_EDITOR
		[SerializeField]
		protected int m_Year ;

		[SerializeField]
		protected int m_Month ;

		[SerializeField]
		protected int m_Day ;

		[SerializeField]
		protected DayOfWeek m_Week ;

		[SerializeField]
		protected int m_Hour ;

		[SerializeField]
		protected int m_Minute ;

		[SerializeField]
		protected int m_Second ;
#endif
		//-----------------------------------

		// サーバーの時間(クライアントは１秒遅れで処理する)
		[SerializeField]
		protected long	m_ServerBaseTime ;	

		// サーバーの時刻と同期を取るためのローカルの基準時刻
		private  float	m_ClientBaseTime ;

		//-----------------------------------

		// 次の日の始まりの時間
		[SerializeField]
		protected long	m_ServerNextDayTime ;

		// 次の週の始まりの時間
		[SerializeField]
		protected long	m_ServerNextWeekTime ;

		// 次の月の始まりの時間
		[SerializeField]
		protected long	m_ServerNextMonthTime ;

		//---------------------------------------------------------------------------

		// 1970年01月01日 00時00分00秒 からの経過秒数を計算するためのエポック時間
		private static readonly DateTime m_UNIX_EPOCH = new DateTime( 1970, 1, 1, 0, 0, 0, 0 ) ;

		//---------------------------------------------------------------------------

		/// <summary>
		/// Unixエポックを取得する
		/// </summary>
		public static DateTime UnixEpoch
		{
			get
			{
				return m_UNIX_EPOCH ;
			}
		}

		/// <summary>
		/// サーバーの時間を更新する
		/// </summary>
		/// <param name=""></param>
		public static bool SetServerBaseTime( long serverBaseTime )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			// サーバーの時間を更新する
			m_Instance.m_ServerBaseTime			= serverBaseTime ;

#if UNITY_EDITOR
			// デバッグ用のモニタリング
			( int year, int month, int day, DayOfWeek week, int hour, int minute, int second ) = GetFull() ;
			m_Instance.m_Year	= year ;
			m_Instance.m_Month	= month ;
			m_Instance.m_Day	= day ;
			m_Instance.m_Week	= week ;
			m_Instance.m_Hour	= hour ;
			m_Instance.m_Minute	= minute ;
			m_Instance.m_Second	= second ;
#endif

			// ローカルの基準時刻を更新する
			m_Instance.m_ClientBaseTime = Time.realtimeSinceStartup ;

			return true ;
		}

#if false
		/// <summary>
		/// サーバー時間のエポックタイムからの経過秒数を取得する
		/// </summary>
		/// <returns></returns>
		public static long Tick
		{
			get
			{
				if( m_Instance == null )
				{
					return 0 ;
				}

				TimeSpan time = ToDateTime( m_Instance.m_ServerBaseTime ) - m_UNIX_EPOCH ;

				return ( long )time.TotalSeconds + ( long )( Time.realtimeSinceStartup - m_Instance.m_ClientBaseTime ) ;
			}
		}
#endif

		/// <summary>
		/// 基準の時刻値
		/// </summary>
		public static long BaseTime
		{
			get
			{
				if( m_Instance == null )
				{
					return 0 ;
				}

				return m_Instance.m_ServerBaseTime ;
			}
		}

		/// <summary>
		/// 現在の時刻値を取得する
		/// </summary>
		public static long Now
		{
			get
			{
				if( m_Instance == null )
				{
					return 0 ;
				}

				return m_Instance.m_ServerBaseTime + ( long )( Time.realtimeSinceStartup - m_Instance.m_ClientBaseTime ) ;
			}
		}

#if false
		/// <summary>
		/// Unixエポックからの経過秒数に変換する
		/// </summary>
		/// <param name="dateTime"></param>
		/// <returns></returns>
		public static long ToTick( long dateTime )
		{
			TimeSpan time = ToDateTime( dateTime ) - m_UNIX_EPOCH ;
			return ( long )time.TotalSeconds ;
		}
#endif


		/// <summary>
		/// Unixエポックからの日時に変換する
		/// </summary>
		/// <param name="tick"></param>
		/// <returns></returns>
		public static DateTime ToDateTime( long tickTime = 0 )
		{
			if( tickTime == 0 )
			{
				tickTime = Now ;
			}

			// 1970/01/01 を加算する
			DateTime dateTime = m_UNIX_EPOCH.AddSeconds( tickTime ) ;

			// タイムゾーンの補正を加算する(一旦不要)
//			TimeZone zone = TimeZone.CurrentTimeZone ;
//			TimeSpan offset = zone.GetUtcOffset( DateTime.Now ) ;
//			dateTime += offset ;

			return dateTime ;
		}

		/// <summary>
		/// Unixエポックからの経過秒数に変換する
		/// </summary>
		/// <param name="dateTime"></param>
		/// <returns></returns>
		public static long ToTickTime( DateTime dateTime )
		{
			// タイムゾーンの補正を減算する(一旦不要)
//			TimeZone zone = TimeZone.CurrentTimeZone ;
//			TimeSpan offset = zone.GetUtcOffset( DateTime.Now ) ;
//			dateTime -= offset ;

			// 1970/01/01 を減算する
			TimeSpan time = dateTime - m_UNIX_EPOCH ;

			return ( long )time.TotalSeconds ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 現在の日付を取得する
		/// </summary>
		/// <returns></returns>
		public static ( int, int, int ) GetDate()
		{
			DateTime dt = ToDateTime() ;

			// 日曜日が 0 
			return ( dt.Year, dt.Month, dt.Day ) ;
		}

		/// <summary>
		/// 現在の日付を取得する
		/// </summary>
		/// <returns></returns>
		public static DayOfWeek GetWeek()
		{
			DateTime dt = ToDateTime() ;

			// 日曜日が 0 
			return dt.DayOfWeek ;
		}

		/// <summary>
		/// 現在の時刻を取得する
		/// </summary>
		/// <returns></returns>
		public static ( int, int, int ) GetTime()
		{
			DateTime dt = ToDateTime() ;

			// 日曜日が 0 
			return ( dt.Hour, dt.Minute, dt.Second ) ;
		}

		/// <summary>
		/// 現在の日付と時刻を取得する
		/// </summary>
		/// <returns></returns>
		public static ( int, int, int, DayOfWeek, int, int, int ) GetFull()
		{
			DateTime dt = ToDateTime() ;

			// 日曜日が 0 
			return ( dt.Year, dt.Month, dt.Day, dt.DayOfWeek, dt.Hour, dt.Minute, dt.Second ) ;
		}

		/// <summary>
		/// 現在の日付名を取得する
		/// </summary>
		/// <returns></returns>
		public static string GetDateName()
		{
			( int year, int month, int day ) = GetDate() ;

			return string.Format( "{0:D4}/{1:D2}/{2:D2}", year, month, day ) ;
		}

		/// <summary>
		/// 現在の時刻名を取得する
		/// </summary>
		/// <returns></returns>
		public static string GetTimeName()
		{
			( int hour, int minute, int second ) = GetTime() ;

			return string.Format( "{0:D2}:{1:D2}:{2:D2}", hour, minute, second ) ;
		}

		/// <summary>
		/// 日付と時刻を文字列化して取得する
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public static string YYYYMM
		{
			get
			{
				return ToDateTime().YYYYMM() ;
			}
		}

		/// <summary>
		/// 日付と時刻を文字列化して取得する
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public static string YYYYMMDD
		{
			get
			{
				return ToDateTime().YYYYMMDD() ;
			}
		}

		/// <summary>
		/// 日付と時刻を文字列化して取得する
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public static string HHMM
		{
			get
			{
				return ToDateTime().HHMM() ;
			}
		}

		/// <summary>
		/// 日付と時刻を文字列化して取得する
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public static string HHMMSS
		{
			get
			{
				return ToDateTime().HHMMSS() ;
			}
		}

		/// <summary>
		/// 日付と時刻を文字列化して取得する
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public static string YYYYMMDD_HHMM
		{
			get
			{
				return ToDateTime().YYYYMMDD_HHMM() ;
			}
		}

		/// <summary>
		/// 日付と時刻を文字列化して取得する
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public static string YYYYMMDD_HHMMSS
		{
			get
			{
				return ToDateTime().YYYYMMDD_HHMMSS() ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 今日の始まりの日時
		/// </summary>
		public static long TodayTime
		{
			get
			{
				if( m_Instance == null )
				{
					return 0 ;
				}

				return m_Instance.m_ServerNextDayTime - ( 24 * 60 * 60 ) ;
			}
		}

		/// <summary>
		/// 今日の始まりの日時
		/// </summary>
		public static DateTime TodayTime_ToDateTime
		{
			get
			{
				return ToDateTime( TodayTime ) ;
			}
		}

		//-----------------------------------

		/// <summary>
		/// 次に日が変わる日時
		/// </summary>
		public static long NextDayTime
		{
			get
			{
				if( m_Instance == null )
				{
					return 0 ;
				}

				return m_Instance.m_ServerNextDayTime ;
			}
		}

		/// <summary>
		/// 次に日が変わる日時
		/// </summary>
		public static DateTime NextDayTime_ToDateTime
		{
			get
			{
				return ToDateTime( NextDayTime ) ;
			}
		}

		/// <summary>
		/// 次に日が変わる時間を返す
		/// </summary>
		/// <param name="am"></param>
		/// <returns></returns>
/*		public static long GetNextDayTime( int am = 5 )
		{
			// 現在の日時
			DateTime dt = ToDateTime() ;

			DateTime next ;

			if( dt.Hour <  am )
			{
				// 同じの日付
				next = new DateTime( dt.Year, dt.Month, dt.Day, am, 0, 0 ) ;
			}
			else
			{
				// 次の日付
				dt = ToDateTime( Now + ( 24 * 60 * 60 ) ) ;
				next = new DateTime( dt.Year, dt.Month, dt.Day, am, 0, 0 ) ;
			}

			return ToTickTime( next ) ;
		}*/

		/// <summary>
		/// 次に週が変わる日時
		/// </summary>
		public static long NextWeekTime
		{
			get
			{
				if( m_Instance == null )
				{
					return 0 ;
				}

				return m_Instance.m_ServerNextWeekTime ;
			}
		}

		/// <summary>
		/// 次に週が変わる日時
		/// </summary>
		public static DateTime NextWeekTime_ToDateTime
		{
			get
			{
				return ToDateTime( NextWeekTime ) ;
			}
		}

		/// <summary>
		/// 次に週が変わる時間を返す
		/// </summary>
		/// <param name="am"></param>
		/// <returns></returns>
/*		public static long GetNextWeekTime( int am = 5 )
		{
			// 現在の日時
			DateTime dt = ToDateTime() ;

			DateTime next  ;

			Dictionary<DayOfWeek,int>	additionalDays = new Dictionary<DayOfWeek, int>()
			{
				{ DayOfWeek.Monday,		7 },
				{ DayOfWeek.Tuesday,	6 },
				{ DayOfWeek.Wednesday,	5 },
				{ DayOfWeek.Thursday,	4 },
				{ DayOfWeek.Friday,		3 },
				{ DayOfWeek.Saturday,	2 },
				{ DayOfWeek.Sunday,		1 },
			} ;

			int additionalDay = additionalDays[ dt.DayOfWeek ] ;

			if( dt.DayOfWeek == DayOfWeek.Monday )
			{
				// 月曜(だけ例外)
				if( dt.Hour <  am )
				{
					// ０日後の日付
					next = new DateTime( dt.Year, dt.Month, dt.Day, am, 0, 0 ) ;
				}
				else
				{
					// ７日後の日付
					dt = ToDateTime( Now + ( additionalDay * 24 * 60 * 60 ) ) ;
					next = new DateTime( dt.Year, dt.Month, dt.Day, am, 0, 0 ) ;
				}
			}
			else
			{
				// 火曜～日曜
				dt = ToDateTime( Now + ( additionalDay * 24 * 60 * 60 ) ) ;
				next = new DateTime( dt.Year, dt.Month, dt.Day, am, 0, 0 ) ;
			}

			return ToTickTime( next ) ;
		}*/

		/// <summary>
		/// 次に月が変わる日時
		/// </summary>
		public static long NextMonthTime
		{
			get
			{
				if( m_Instance == null )
				{
					return 0 ;
				}

				return m_Instance.m_ServerNextMonthTime ;
			}
		}

		/// <summary>
		/// 次に月が変わる日時
		/// </summary>
		public static DateTime NextMonthTime_ToDateTime
		{
			get
			{
				return ToDateTime( NextMonthTime ) ;
			}
		}

		/// <summary>
		/// 次に月が変わる時間を返す
		/// </summary>
		/// <param name="am"></param>
		/// <returns></returns>
/*		public static long GetNextMonthTime( int am = 5 )
		{
			// 現在の日時
			DateTime dt = ToDateTime() ;

			DateTime next  ;

			// １日だけ例外
			if( dt.Day == 1 && dt.Hour <  am )
			{
				// 今日の日付
				next = new DateTime( dt.Year, dt.Month, dt.Day, am, 0, 0 ) ;
			}
			else
			{
				int year	= dt.Year ;
				int month	= dt.Month ;
				if( month <  12 )
				{
					month ++ ;
				}
				else
				{
					year ++ ;
					month = 1 ;
				}

				next = new DateTime( year, month, 1, am, 0, 0 ) ;
			}

			return ToTickTime( next ) ;
		}*/
	}
}
