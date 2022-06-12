using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

namespace Template
{
	/// <summary>
	/// JsonUtility 型のメソッド拡張
	/// </summary>
	public static class JsonUtility
	{
		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 配列からオブジェクトに変換するクラス
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public class JsonArry<T>
		{
			public T Items ;
		}

		/// <summary>
		/// デシリアライズ
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="text"></param>
		/// <returns></returns>
		public static T FromJson<T>( string text )
		{
			Type type = typeof( T ) ;
			if( type.IsArray == true || ( type.IsGenericType == true && type.GetGenericTypeDefinition() == typeof( List<> ) ) )
			{
				// 配列タイプ
				return UnityEngine.JsonUtility.FromJson<JsonArry<T>>( text ).Items ;
			}
			else
			{
				// 単体タイプ
				return UnityEngine.JsonUtility.FromJson<T>( text ) ;
			}
		}

		/// <summary>
		/// デシリアライズ
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="text"></param>
		/// <returns></returns>
		public static System.Object FromJson( string text, Type type )
		{
			return UnityEngine.JsonUtility.FromJson( text, type ) ;
		}

		/// <summary>
		/// デシリアライズ(上書き)
		/// </summary>
		/// <param name="text"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static bool FromJsonOverwrite<T>( string text, T target )
		{
			if( string.IsNullOrEmpty( text ) == true || target == null )
			{
				return false ;
			}

			Type type = typeof( T ) ;
			if( type.IsArray == true || ( type.IsGenericType == true && type.GetGenericTypeDefinition() == typeof( List<> ) ) )
			{
				// 配列タイプ
				JsonArry<T> array = new JsonArry<T>()
				{
					Items = target
				} ;

				UnityEngine.JsonUtility.FromJsonOverwrite( text, array ) ;
			}
			else
			{
				// 単体タイプ
				UnityEngine.JsonUtility.FromJsonOverwrite( text, target ) ;
			}

			return true ;
		}

		/// <summary>
		/// デシリアライズ(上書き)
		/// </summary>
		/// <param name="text"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static bool FromJsonOverwrite( string text, System.Object target )
		{
			if( string.IsNullOrEmpty( text ) == true || target == null )
			{
				return false ;
			}

			UnityEngine.JsonUtility.FromJsonOverwrite( text, target ) ;

			return true ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// シリアライズ
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public static string ToJson<T>( T target, bool prettyPrint = false )
		{
			Type type = typeof( T ) ;
			if( type.IsArray == true || ( type.IsGenericType == true && type.GetGenericTypeDefinition() == typeof( List<> ) ) )
			{
				// 配列タイプ
				JsonArry<T> array = new JsonArry<T>()
				{
					Items = target
				} ;

				return UnityEngine.JsonUtility.ToJson( array, prettyPrint ) ;
			}
			else
			{
				// 単体タイプ
				return UnityEngine.JsonUtility.ToJson( target, prettyPrint ) ;
			}
		}

		/// <summary>
		/// シリアライズ
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public static string ToJson( System.Object target, bool prettyPrint = false )
		{
			return UnityEngine.JsonUtility.ToJson( target, prettyPrint ) ;
		}
	}
}

