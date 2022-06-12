using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

namespace Template
{
	/// <summary>
	/// Color32 型のメソッド拡張(注意:構造体の拡張メソッドには ref が必要) Version 2021/06/21
	/// </summary>
	public static class ExColor
	{
		/// <summary>
		/// 完全透明
		/// </summary>
		public static Color Transparency = new Color( 0, 0, 0, 0 ) ;

		/// <summary>
		/// 32ビット符号なし整数から色を設定する
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static void SetARGB( ref this Color32 c, uint argb )
		{
			c.r = ( byte )( ( argb >> 16 ) & 0xFF ) ;
			c.g = ( byte )( ( argb >>  8 ) & 0xFF ) ;
			c.b = ( byte )(   argb         & 0xFF ) ;
			c.a = ( byte )( ( argb >> 24 ) & 0xFF ) ;
		}

		/// <summary>
		/// RGB を Color に変換する
		/// </summary>
		/// <param name="argb"></param>
		/// <returns></returns>
		public static Color32 RGB( uint argb )
		{
			return new Color32
			(
				( byte )( ( argb >> 16 ) & 0xFF ),
				( byte )( ( argb >>  8 ) & 0xFF ),
				( byte )(   argb         & 0xFF ),
				0xFF
			) ;
		}

		/// <summary>
		/// ARGB を Color に変換する
		/// </summary>
		/// <param name="argb"></param>
		/// <returns></returns>
		public static Color32 ARGB( uint argb )
		{
			return new Color32
			(
				( byte )( ( argb >> 16 ) & 0xFF ),
				( byte )( ( argb >>  8 ) & 0xFF ),
				( byte )(   argb         & 0xFF ),
				( byte )( ( argb >> 24 ) & 0xFF )
			) ;
		}

		/// <summary>
		/// ARGB を Color に変換する
		/// </summary>
		/// <param name="argb"></param>
		/// <returns></returns>
		public static Color32 RGBA( uint rgba )
		{
			return new Color32
			(
				( byte )( ( rgba >> 24 ) & 0xFF ),
				( byte )( ( rgba >> 16 ) & 0xFF ),
				( byte )( ( rgba >>  8 ) & 0xFF ),
				( byte )(   rgba         & 0xFF )
			) ;
		}

		/// <summary>
		/// 色を設定する
		/// </summary>
		/// <param name="c"></param>
		/// <param name="r"></param>
		/// <param name="g"></param>
		/// <param name="b"></param>
		/// <param name="a"></param>
		public static void SetRGBA( ref this Color32 c, byte r, byte g, byte b, byte a )
		{
			c.r = r ;
			c.g = g ;
			c.b = b ;
			c.a = a ;
		}

		/// <summary>
		/// 別の色を上書きする
		/// </summary>
		/// <param name="c0"></param>
		/// <param name="c1"></param>
		public static void Set( ref this Color32 c0, Color32 c1 )
		{
			c0.r = c1.r ;
			c0.g = c1.g ;
			c0.b = c1.b ;
			c0.a = c1.a ;
		}

		/// <summary>
		/// 状態値による変化で既存のインスタンスに書き込む
		/// </summary>
		/// <param name="c0"></param>
		/// <param name="c1"></param>
		/// <param name="factor"></param>
		/// <param name="c2"></param>
		public static void Lerp( Color c0, Color c1, float factor, ref Color c2 )
		{
			float inverse = factor ;
			factor = 1 - factor ;

			c2.r = c0.r * factor + c1.r * inverse ;
			c2.g = c0.g * factor + c1.g * inverse ;
			c2.b = c0.b * factor + c1.b * inverse ;
			c2.a = c0.a * factor + c1.a * inverse ;
		}

		/// <summary>
		/// 状態値による変化で既存のインスタンスに書き込む
		/// </summary>
		/// <param name="c0"></param>
		/// <param name="c1"></param>
		/// <param name="factor"></param>
		/// <param name="c2"></param>
		public static void Lerp( Color32 c0, Color32 c1, float factor, ref Color32 c2 )
		{
			float inverse = factor ;
			factor = 1 - factor ;

			c2.r = ( byte )( c0.r * factor + c1.r * inverse ) ;
			c2.g = ( byte )( c0.g * factor + c1.g * inverse ) ;
			c2.b = ( byte )( c0.b * factor + c1.b * inverse ) ;
			c2.a = ( byte )( c0.a * factor + c1.a * inverse ) ;
		}

	}
}

