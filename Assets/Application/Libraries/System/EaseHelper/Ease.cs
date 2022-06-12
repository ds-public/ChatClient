using System ;
using System.Collections ;
using UnityEngine ;

/// <summary>
/// イーズの値を管理するパッケージ
/// </summary>
namespace EaseHelper
{
	/// <summary>
	/// イーズの種別
	/// </summary>
	public enum EaseTypes
	{
		EaseInQuad,
		EaseOutQuad,
		EaseInOutQuad,
		EaseInCubic,
		EaseOutCubic,
		EaseInOutCubic,
		EaseInQuart,
		EaseOutQuart,
		EaseInOutQuart,
		EaseInQuint,
		EaseOutQuint,
		EaseInOutQuint,
		EaseInSine,
		EaseOutSine,
		EaseInOutSine,
		EaseInExpo,
		EaseOutExpo,
		EaseInOutExpo,
		EaseInCirc,
		EaseOutCirc,
		EaseInOutCirc,
		Linear,
		Spring,
		EaseInBounce,
		EaseOutBounce,
		EaseInOutBounce,
		EaseInBack,
		EaseOutBack,
		EaseOutBackSharp,
		EaseInOutBack,
		EaseInElastic,
		EaseOutElastic,
		EaseInOutElastic,
//		punch
	}

	/// <summary>
	/// イーズの値を管理するクラス Version 2021/06/14 0
	/// </summary>
	public class Ease
	{
		/// <summary>
		/// 文字列を列挙子に変換する
		/// </summary>
		/// <param name="typeName"></param>
		/// <returns></returns>
		public static EaseTypes StringToType( string typeName )
		{
			if( string.IsNullOrEmpty( typeName ) == true )
			{
				return EaseTypes.Linear ;
			}

			typeName = typeName.ToLower() ;

			foreach( EaseTypes value in Enum.GetValues( typeof( EaseTypes ) ) )
			{
				if( value.ToString().ToLower() == typeName )
				{
					return value ;
				}
			}

			return EaseTypes.Linear ;
		}

		/// <summary>
		/// カーブの値を取得する
		/// </summary>
		/// <param name="factor"></param>
		/// <param name="easeType"></param>
		/// <returns></returns>
		public static float GetValue( float factor, EaseTypes easeType )
		{
			return GetValue( 0, 1, factor, easeType ) ;
		}

		public static Vector2 GetValue( Vector2 start, Vector2 end, float factor, EaseTypes easeType )
		{
			float x = GetValue( start.x, end.x, factor, easeType ) ;
			float y = GetValue( start.y, end.y, factor, easeType ) ;

			return new Vector2( x, y ) ;
		}

		public static Vector3 GetValue( Vector3 start, Vector3 end, float factor, EaseTypes easeType )
		{
			float x = GetValue( start.x, end.x, factor, easeType ) ;
			float y = GetValue( start.y, end.y, factor, easeType ) ;
			float z = GetValue( start.z, end.z, factor, easeType ) ;

			return new Vector3( x, y, z ) ;
		}

		// float の変化中の値を取得
		public static float GetValue( float start, float end, float factor, EaseTypes easeType )
		{
			float value = 0 ;
			switch( easeType )
			{
				case EaseTypes.EaseInQuad		: value = EaseInQuad(			start, end, factor )	; break ;
				case EaseTypes.EaseOutQuad		: value = EaseOutQuad(			start, end, factor )	; break ;
				case EaseTypes.EaseInOutQuad	: value = EaseInOutQuad(		start, end, factor )	; break ;
				case EaseTypes.EaseInCubic		: value = EaseInCubic(			start, end, factor )	; break ;
				case EaseTypes.EaseOutCubic		: value = EaseOutCubic(			start, end, factor )	; break ;
				case EaseTypes.EaseInOutCubic	: value = EaseInOutCubic(		start, end, factor )	; break ;
				case EaseTypes.EaseInQuart		: value = EaseInQuart(			start, end, factor )	; break ;
				case EaseTypes.EaseOutQuart		: value = EaseOutQuart(			start, end, factor )	; break ;
				case EaseTypes.EaseInOutQuart	: value = EaseInOutQuart(		start, end, factor )	; break ;
				case EaseTypes.EaseInQuint		: value = EaseInQuint(			start, end, factor )	; break ;
				case EaseTypes.EaseOutQuint		: value = EaseOutQuint(			start, end, factor )	; break ;
				case EaseTypes.EaseInOutQuint	: value = EaseInOutQuint(		start, end, factor )	; break ;
				case EaseTypes.EaseInSine		: value = EaseInSine(			start, end, factor )	; break ;
				case EaseTypes.EaseOutSine		: value = EaseOutSine(			start, end, factor )	; break ;
				case EaseTypes.EaseInOutSine	: value = EaseInOutSine(		start, end, factor )	; break ;
				case EaseTypes.EaseInExpo		: value = EaseInExpo(			start, end, factor )	; break ;
				case EaseTypes.EaseOutExpo		: value = EaseOutExpo(			start, end, factor )	; break ;
				case EaseTypes.EaseInOutExpo	: value = EaseInOutExpo(		start, end, factor )	; break ;
				case EaseTypes.EaseInCirc		: value = EaseInCirc(			start, end, factor )	; break ;
				case EaseTypes.EaseOutCirc		: value = EaseOutCirc(			start, end, factor )	; break ;
				case EaseTypes.EaseInOutCirc	: value = EaseInOutCirc(		start, end, factor )	; break ;
				case EaseTypes.Linear			: value = Linear(				start, end, factor )	; break ;
				case EaseTypes.Spring			: value = Spring(				start, end, factor )	; break ;
				case EaseTypes.EaseInBounce		: value = EaseInBounce(			start, end, factor )	; break ;
				case EaseTypes.EaseOutBounce	: value = EaseOutBounce(		start, end, factor )	; break ;
				case EaseTypes.EaseInOutBounce	: value = EaseInOutBounce(		start, end, factor )	; break ;
				case EaseTypes.EaseInBack		: value = EaseInBack(			start, end, factor )	; break ;
				case EaseTypes.EaseOutBack		: value = EaseOutBack(			start, end, factor )	; break ;
				case EaseTypes.EaseOutBackSharp	: value = EaseOutBackSharp(		start, end, factor )	; break ;
				case EaseTypes.EaseInOutBack	: value = EaseInOutBack(		start, end, factor )	; break ;
				case EaseTypes.EaseInElastic	: value = EaseInElastic(		start, end, factor )	; break ;
				case EaseTypes.EaseOutElastic	: value = EaseOutElastic(		start, end, factor )	; break ;
				case EaseTypes.EaseInOutElastic	: value = EaseInOutElastic(		start, end, factor )	; break ;
			}
			return value ;
		}

		//------------------------

		private static float EaseInQuad( float start, float end, float value )
		{
			end -= start ;
			return end * value * value + start ;
		}

		private static float EaseOutQuad( float start, float end, float value )
		{
			end -= start ;
			return - end * value * ( value - 2 ) + start ;
		}

		private static float EaseInOutQuad( float start, float end, float value )
		{
			value /= 0.5f ;
			end -= start ;
			if( value <  1 ) return end * 0.5f * value * value + start ;
			value -- ;
			return - end * 0.5f * ( value * ( value - 2 ) - 1 ) + start ;
		}

		private static float EaseInCubic( float start, float end, float value )
		{
			end -= start ;
			return end * value * value * value + start ;
		}

		private static float EaseOutCubic( float start, float end, float value )
		{
			value -- ;
			end -= start ;
			return end * ( value * value * value + 1 ) + start ;
		}

		private static float EaseInOutCubic( float start, float end, float value )
		{
			value /= 0.5f ;
			end -= start ;
			if( value <  1 ) return end * 0.5f * value * value * value + start ;
			value -= 2 ;
			return end * 0.5f * ( value * value * value + 2 ) + start ;
		}

		private static float EaseInQuart( float start, float end, float value )
		{
			end -= start ;
			return end * value * value * value * value + start ;
		}

		private static float EaseOutQuart( float start, float end, float value )
		{
			value -- ;
			end -= start ;
			return - end * ( value * value * value * value - 1 ) + start ;
		}

		private static float EaseInOutQuart( float start, float end, float value )
		{
			value /= 0.5f ;
			end -= start ;
			if( value <  1 ) return end * 0.5f * value * value * value * value + start ;
			value -= 2 ;
			return - end * 0.5f * ( value * value * value * value - 2 ) + start ;
		}

		private static float EaseInQuint( float start, float end, float value )
		{
			end -= start ;
			return end * value * value * value * value * value + start ;
		}
	
		private static float EaseOutQuint( float start, float end, float value )
		{
			value -- ;
			end -= start ;
			return end * ( value * value * value * value * value + 1 ) + start ;
		}

		private static float EaseInOutQuint( float start, float end, float value )
		{
			value /= 0.5f ;
			end -= start ;
			if( value <  1 ) return end * 0.5f * value * value * value * value * value + start ;
			value -= 2 ;
			return end * 0.5f * ( value * value * value * value * value + 2 ) + start ;
		}

		private static float EaseInSine( float start, float end, float value )
		{
			end -= start ;
			return - end * Mathf.Cos( value * ( Mathf.PI * 0.5f ) ) + end + start ;
		}

		private static float EaseOutSine( float start, float end, float value )
		{
			end -= start ;
			return end * Mathf.Sin( value * ( Mathf.PI * 0.5f ) ) + start ;
		}

		private static float EaseInOutSine( float start, float end, float value )
		{
			end -= start ;
			return - end * 0.5f * ( Mathf.Cos( Mathf.PI * value ) - 1 ) + start ;
		}

		private static float EaseInExpo( float start, float end, float value )
		{
			end -= start ;
			return end * Mathf.Pow( 2, 10 * ( value - 1 ) ) + start ;
		}

		private static float EaseOutExpo( float start, float end, float value )
		{
			end -= start ;
			return end * ( - Mathf.Pow( 2, - 10 * value ) + 1 ) + start ;
		}

		private static float EaseInOutExpo( float start, float end, float value )
		{
			value /= 0.5f;
			end -= start ;
			if( value <  1 ) return end * 0.5f * Mathf.Pow( 2, 10 * ( value - 1 ) ) + start ;
			value -- ;
			return end * 0.5f * ( - Mathf.Pow( 2, - 10 * value ) + 2 ) + start ;
		}

		private static float EaseInCirc( float start, float end, float value )
		{
			end -= start ;
			return - end * ( Mathf.Sqrt( 1 - value * value ) - 1 ) + start ;
		}

		private static float EaseOutCirc( float start, float end, float value )
		{
			value -- ;
			end -= start ;
			return end * Mathf.Sqrt( 1 - value * value ) + start ;
		}

		private static float EaseInOutCirc( float start, float end, float value )
		{
			value /= 0.5f ;
			end -= start ;
			if( value <  1 ) return - end * 0.5f * ( Mathf.Sqrt( 1 - value * value ) - 1 ) + start ;
			value -= 2 ;
			return end * 0.5f * ( Mathf.Sqrt( 1 - value * value ) + 1 ) + start ;
		}

		private static float Linear( float start, float end, float value )
		{
			return Mathf.Lerp( start, end, value ) ;
		}
	
		private static float Spring( float start, float end, float value )
		{
			value = Mathf.Clamp01( value ) ;
			value = ( Mathf.Sin( value * Mathf.PI * ( 0.2f + 2.5f * value * value * value ) ) * Mathf.Pow( 1f - value, 2.2f ) + value ) * ( 1f + ( 1.2f * ( 1f - value ) ) ) ;
			return start + ( end - start ) * value ;
		}

		private static float EaseInBounce( float start, float end, float value )
		{
			end -= start ;
			float d = 1f ;
			return end - EaseOutBounce( 0, end, d - value ) + start ;
		}
	
		private static float EaseOutBounce( float start, float end, float value )
		{
			value /= 1f ;
			end -= start ;
			if( value <  ( 1 / 2.75f ) )
			{
				return end * ( 7.5625f * value * value ) + start ;
			}
			else
			if( value <  ( 2 / 2.75f ) )
			{
				value -= ( 1.5f / 2.75f ) ;
				return end * ( 7.5625f * ( value ) * value + .75f ) + start ;
			}
			else
			if( value <  (  2.5  / 2.75 ) )
			{
				value -= ( 2.25f / 2.75f ) ;
				return end * ( 7.5625f * ( value ) * value + .9375f ) + start ;
			}
			else
			{
				value -= ( 2.625f / 2.75f ) ;
				return end * ( 7.5625f * ( value ) * value + .984375f ) + start ;
			}
		}

		private static float EaseInOutBounce( float start, float end, float value )
		{
			end -= start ;
			float d = 1f ;
			if( value <  d * 0.5f ) return EaseInBounce( 0, end, value * 2 ) * 0.5f + start ;
			else return EaseOutBounce( 0, end, value * 2 - d ) * 0.5f + end * 0.5f + start ;
		}

		private static float EaseInBack( float start, float end, float value )
		{
			end -= start ;
			value /= 1 ;
			float s = 1.70158f ;
			return end * ( value ) * value * ( ( s + 1 ) * value - s ) + start ;
		}

		private static float EaseOutBack( float start, float end, float value )
		{
			float s = 1.70158f ;
			end -= start ;
			value = ( value ) - 1 ;
			return end * ( ( value ) * value * ( ( s + 1 ) * value + s ) + 1 ) + start ;
		}

		private static float EaseOutBackSharp( float start, float end, float value )
		{
			value *= value ;
			float s = 1.70158f ;
			end -= start ;
			value = ( value ) - 1 ;
			return end * ( ( value ) * value * ( ( s + 1 ) * value + s ) + 1 ) + start ;
		}

		private static float EaseInOutBack( float start, float end, float value )
		{
			float s = 1.70158f ;
			end -= start ;
			value /= 0.5f ;
			if( ( value ) <  1 )
			{
				s *= ( 1.525f ) ;
				return end * 0.5f * ( value * value * ( ( ( s ) + 1 ) * value - s ) ) + start ;
			}
			value -= 2 ;
			s *= ( 1.525f ) ;
			return end * 0.5f * ( ( value ) * value * ( ( ( s ) + 1 ) * value + s ) + 2 ) + start ;
		}

		private static float EaseInElastic( float start, float end, float value )
		{
			end -= start ;
		
			float d = 1f ;
			float p = d * 0.3f ;
			float s ;
			float a = 0 ;
		
			if( value == 0 ) return start ;
		
			if( ( value /= d ) == 1 ) return start + end ;
		
			if( a == 0f || a <  Mathf.Abs( end ) )
			{
				a = end ;
				s = p / 4 ;
			}
			else
			{
				s = p / ( 2 * Mathf.PI ) * Mathf.Asin( end / a ) ;
			}
		
			return - ( a * Mathf.Pow( 2, 10 * ( value -= 1 ) ) * Mathf.Sin( ( value * d - s ) * ( 2 * Mathf.PI ) / p ) ) + start ;
		}		

		private static float EaseOutElastic( float start, float end, float value )
		{
			end -= start ;
		
			float d = 1f ;
			float p = d * 0.3f ;
			float s ;
			float a = 0 ;
		
			if( value == 0 ) return start ;
		
			if( ( value /= d ) == 1 ) return start + end ;
		
			if( a == 0f || a <  Mathf.Abs( end ) )
			{
				a = end ;
				s = p * 0.25f ;
			}
			else
			{
				s = p / ( 2 * Mathf.PI ) * Mathf.Asin( end / a ) ;
			}
		
			return ( a * Mathf.Pow( 2, - 10 * value ) * Mathf.Sin( ( value * d - s ) * ( 2 * Mathf.PI ) / p ) + end + start ) ;
		}		

		private static float EaseInOutElastic( float start, float end, float value )
		{
			end -= start ;
		
			float d = 1f ;
			float p = d * 0.3f ;
			float s ;
			float a = 0 ;
		
			if( value == 0 ) return start ;
		
			if( ( value /= d * 0.5f ) == 2 ) return start + end ;
		
			if( a == 0f || a <  Mathf.Abs( end ) )
			{
				a = end ;
				s = p / 4 ;
			}
			else
			{
				s = p / ( 2 * Mathf.PI ) * Mathf.Asin( end / a ) ;
			}
		
			if( value <  1 ) return - 0.5f * ( a * Mathf.Pow( 2, 10 * ( value -= 1 ) ) * Mathf.Sin( ( value * d - s ) * ( 2 * Mathf.PI ) / p ) ) + start ;
			return a * Mathf.Pow( 2, - 10 * ( value -= 1 ) ) * Mathf.Sin( ( value * d - s ) * ( 2 * Mathf.PI ) / p ) * 0.5f + end + start ;
		}
#if false
		private static float Punch( float amplitude, float value )
		{
			float s ;
			if( value == 0 )
			{
				return 0 ;
			}
			else
			if( value == 1 )
			{
				return 0 ;
			}
			float period = 1 * 0.3f ;
			s = period / ( 2 * Mathf.PI ) * Mathf.Asin( 0 ) ;
			return ( amplitude * Mathf.Pow( 2, - 10 * value ) * Mathf.Sin( ( value * 1 - s ) * ( 2 * Mathf.PI ) / period ) ) ;
		}

		private static float Clerp( float start, float end, float value )
		{
			float min = 0.0f ;
			float max = 360.0f ;
			float half = Mathf.Abs( ( max - min ) * 0.5f ) ;
			float retval ;
			float diff ;
			if( ( end - start ) <  - half )
			{
				diff =   ( ( max - start ) + end   ) * value ;
				retval = start + diff ;
			}
			else
			if( ( end - start ) >    half )
			{
				diff = - ( ( max - end   ) + start ) * value ;
				retval = start + diff ;
			}
			else retval = start + ( end - start ) * value ;
			return retval ;
		}
#endif
	}
}
