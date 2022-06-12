using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

namespace Template
{
	/// <summary>
	/// DateTime 型のメソッド拡張 Version 2022/02/08
	/// </summary>
	public static class ExDateTime
	{
		/// <summary>
		/// インスタンスを複製する
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		public static DateTime Clone( this DateTime dt )
		{
			return new DateTime( dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second ) ;
		}

		//-----------------------------------

		/// <summary>
		/// 日付と時刻を文字列化して取得する
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public static string YYYYMM( this DateTime dt )
		{
			return $"{dt:yyyy/MM}" ;
		}

		/// <summary>
		/// 日付と時刻を文字列化して取得する
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public static string YYYYMMDD( this DateTime dt )
		{
			return $"{dt:yyyy/MM/dd}" ;
		}

		/// <summary>
		/// 日付と時刻を文字列化して取得する
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public static string HHMM( this DateTime dt )
		{
			return $"{dt:hh:mm}" ;
		}

		/// <summary>
		/// 日付と時刻を文字列化して取得する
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public static string HHMMSS( this DateTime dt )
		{
			return $"{dt:hh:mm:ss}" ;
		}


		/// <summary>
		/// 日付と時刻を文字列化して取得する
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public static string YYYYMMDD_HHMM( this DateTime dt )
		{
			return $"{dt:yyyy/MM/dd HH:mm}" ;
		}

		/// <summary>
		/// 日付と時刻を文字列化して取得する
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public static string YYYYMMDD_HHMMSS( this DateTime dt )
		{
			return $"{dt:yyyy/MM/dd HH:mm:ss}" ;
		}

	}
}
