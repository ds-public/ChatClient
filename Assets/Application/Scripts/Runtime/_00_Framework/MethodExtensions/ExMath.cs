using UnityEngine ;

namespace Template
{
	/// <summary>
	/// Math 型のメソッド拡張
	/// </summary>
	public static class Mathi
	{
		/// <summary>
		/// 切り上げを行う(整数版)
		/// </summary>
		/// <returns>The ceiling.</returns>
		/// <param name="a">The alpha component.</param>
		/// <param name="b">The blue component.</param>
		public static int Ceiling( int a, int b )
		{
			if( b == 0 )
			{
				return 0 ;
			}

			int c = a / b ;
			if( ( a % b ) >  0 )
			{
				c ++ ;
			}

			return c ;
		}

		/// <summary>
		/// 浮動小数値がほぼ0かどうかを判定する
		/// </summary>
		/// <returns><c>true</c>, if zero was ised, <c>false</c> otherwise.</returns>
		/// <param name="v">V.</param>
		/// <param name="threshold">Threshold.</param>
		public static bool IsZero( float v, float threshold = 0.00001f )
		{
			v = Mathf.Abs( v ) ;
			return ( v <  threshold ) ;
		}

		/// <summary>
		/// ほぼ同じかどうか判定する
		/// </summary>
		/// <returns><c>true</c>, if same was ised, <c>false</c> otherwise.</returns>
		/// <param name="v0">V0.</param>
		/// <param name="v1">V1.</param>
		/// <param name="threshold">Threshold.</param>
		public static bool IsSame( float v0, float v1, float threshold = 0.00001f )
		{
			return ( v0 <= ( v1 + threshold ) && v0 >= ( v1 - threshold ) ) ;
		}

		/// <summary>
		/// 値を絶対値化する
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		public static int Abs( int v )
		{
			if( v <  0 )
			{
				return - v ;
			}

			return v ;
		}

		/// <summary>
		/// <see cref="a"/>, <see cref="b"/> の値から最大・最小範囲を計算します
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="min">最小値</param>
		/// <param name="max">最大値</param>
		public static void GetMinMaxVector3(
			in  Vector3 a,
			in  Vector3 b,
			out Vector3 min,
			out Vector3 max
		)
		{
			if ( a.x < b.x )
			{
				min.x = a.x ;
				max.x = b.x ;
			}
			else
			{
				min.x = b.x ;
				max.x = a.x ;
			}

			if ( a.y < b.y )
			{
				min.y = a.y ;
				max.y = b.y ;
			}
			else
			{
				min.y = b.y ;
				max.y = a.y ;
			}

			if ( a.z < b.z )
			{
				min.z = a.z ;
				max.z = b.z ;
			}
			else
			{
				min.z = b.z ;
				max.z = a.z ;
			}
		}
	}
}
