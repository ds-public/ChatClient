using UnityEngine ;
using System;
using System.Collections ;

// Last Update 2020/04/25

/// <summary>
/// 乱数生成のパッケージ
/// </summary>
namespace MathHelper
{
	/// <summary>
	/// インスタンスを生成しなくても使えるランダムクラス
	/// </summary>
	public static class Random_XorShift
	{
		private static readonly XorShift m_XorShift = new XorShift() ;

		static Random_XorShift()
		{
			// 初期化時に現在時刻を元にランダムな回数（最大60回）乱数を読み捨てる
			int i, l = DateTime.Now.Second ;
			for( i = 0; i < l ; i ++ )
			{
				m_XorShift.Get() ;
			}
		}

		public static ulong Seed
		{
			get
			{
				return m_XorShift.Seed ;
			}
			set
			{
				m_XorShift.Seed = value ;
			}
		}

		/// <summary>
		/// 時間値をシードにする
		/// </summary>
		public static void SetSeed( ulong? seed = null )
		{
			m_XorShift.Seed = ( seed == null ? ( ulong )GetUnixTime() : seed.Value ) ;
		}

		// UNIXエポックを表すDateTimeオブジェクトを取得
		private static readonly DateTime UNIX_EPOCH = new DateTime( 1970, 1, 1, 0, 0, 0, 0 ) ;

		private static long GetUnixTime()
		{
			DateTime dt = DateTime.Now ;

			// UTC時間に変換
			dt.ToUniversalTime() ;

			// UNIXエポックからの経過時間を取得
			TimeSpan elapsedTime = dt - UNIX_EPOCH ;
			
			// 経過秒数に変換
			return ( long )elapsedTime.TotalSeconds ;
		}


		public static ulong Get()
		{
			return m_XorShift.Get() ;
		}

		public static int Get( int max, bool limit = true )
		{
			return m_XorShift.Get( max, limit ) ;
		}

		public static int Get( int min, int max, bool limit = true, bool swap = false )
		{
			return m_XorShift.Get( min, max, limit, swap ) ;
		}

		public static float Get( float min, float max, bool swap = false )
		{
			return m_XorShift.Get( min, max, swap ) ;
		}
	}

	/// <summary>
	/// XorShift アルゴリズムの乱数生成クラス
	/// </summary>
	public class XorShift
	{
		// 初期の根値
		private ulong m_RandomSeedX = 123456789L ;
		private ulong m_RandomSeedY = 362436069L ;
		private ulong m_RandomSeedZ = 521288629L ;
		private ulong m_RandomSeedW =  88675123L ;

		/// <summary>
		/// 疑似乱数根
		/// </summary>
		public ulong Seed
		{
			get
			{
				return m_RandomSeedX ;
			}
			set
			{
				m_RandomSeedX = ( ulong )value ;
				m_RandomSeedY = 362436069L ;
				m_RandomSeedZ = 521288629L ;
				m_RandomSeedW =  88675123L ;
			}
		}

		/// <summary>
		/// 整数値の範囲で乱数値を取得する(xorshift)
		/// </summary>
		/// <returns></returns>
		public ulong Get()
		{
			ulong t = ( m_RandomSeedX ^ ( m_RandomSeedX << 11 ) ) ;

//			Debug.LogWarning( "RX:" + mRandomSeedX + " RY:" + mRandomSeedX + " RZ:" + mRandomSeedZ + " RW:" + mRandomSeedW ) ;

			m_RandomSeedX = m_RandomSeedY ;
			m_RandomSeedY = m_RandomSeedZ ;
			m_RandomSeedZ = m_RandomSeedW ;
			m_RandomSeedW = m_RandomSeedW ^ ( m_RandomSeedW >> 19 ) ^ ( t ^ ( t >> 8 ) )  ;

			return m_RandomSeedW ; 
		}

		/// <summary>
		/// ０から最大値の範囲の整数型乱数値を返す
		/// </summary>
		/// <param name="max"></param>
		/// <returns></returns>
		public int Get( int max, bool limit = true )
		{
			if( max <  0 )
			{
				return 0 ; // 値が不正
			}

			return ( int )( Get() % ( ulong )( max + ( limit ? 1 : 0 ) ) ) ;
		}

		/// <summary>
		/// 最小値から最大値の範囲の整数型乱数値を返す
		/// </summary>
		/// <param name="min">最小値</param>
		/// <param name="max">最大値</param>
		/// <returns></returns>
		public int Get( int min, int max, bool limit = true, bool swap = false )
		{
			if( min >  max )
			{
				if( swap == true )
				{
					// 値を入れ替える
					int v = min ;
					min = max ;
					max = v ;
				}
				else
				{
					return 0 ;  // 値が不正
				}
			}

			return min + ( int )( Get() % ( ulong )( ( max - min ) + ( limit ? 1 : 0 ) ) ) ;
		}

		/// <summary>
		/// 最小値から最大値の範囲の小数型乱数を返す
		/// </summary>
		/// <param name="min">最小値</param>
		/// <param name="max">最大値</param>
		/// <returns></returns>
		public float Get( float min, float max, bool isSwap = false )
		{
			if( min >  max )
			{
				if( isSwap == true )
				{
					// 値を入れ替える
					float v = min ;
					min = max ;
					max = v ;
				}
				else
				{
					return 0f ;  // 値が不正
				}
			}

			ulong r = Get() ;
			float a = ( float )( r % ( 100000000L + 1L ) ) / ( float )100000000L ;
//			Debug.LogWarning( "r値:" + r + " a値:" + a ) ;

			return min + ( ( max - min ) * a ) ;
		}
	}
}
