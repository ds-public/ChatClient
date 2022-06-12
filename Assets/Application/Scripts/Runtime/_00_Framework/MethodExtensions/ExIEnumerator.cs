using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Template
{
	/// <summary>
	/// MonoBehaviour のメソッド拡張
	/// </summary>
	public static class ExIEnumerator
	{
		/// <summary>
		/// コルーチンからの戻り値を取得する
		/// </summary>
		/// <returns>The current.</returns>
		/// <param name="e">E.</param>
		/// <param name="defaultValue">Default value.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T GetCurrent<T>( this IEnumerator e, T defaultValue = default )
		{
			if( e.Current == null || !( e.Current is T ) )
			{
				return defaultValue ;
			}

			return ( T )e.Current ;
		}
	}
}

