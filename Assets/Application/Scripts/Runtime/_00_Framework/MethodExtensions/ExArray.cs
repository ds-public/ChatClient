using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

namespace Template
{
	/// <summary>
	/// Array 型のメソッド拡張 Version 2021/08/18
	/// </summary>
	public static class ExArray
	{
		/// <summary>
		/// 配列が null もしくは要素数が 0 の時に true を返す
		/// </summary>
		/// <param name="array"></param>
		/// <returns></returns>
		public static bool IsNullOrEmpty( this Array array )
		{
			if( array == null || array.Length == 0 )
			{
				return true ;
			}

			return false ;
		}

		/// <summary>
		/// 配列をリスト化する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="array"></param>
		/// <returns></returns>
		public static List<T> ToList<T>( this T[] array )
		{
			List<T> list = new List<T>() ;
			list.AddRange( array ) ;

			return list ;
		}

		/// <summary>
		/// 配列内の要素全てに処理を適用する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="array"></param>
		/// <param name="action"></param>
		/// <returns></returns>
		public static T[] ForEach<T>( this T[] array, Action<T> action )
		{
			if( array == null || array.Length == 0 || action == null )
			{
				return array ;
			}


			foreach( T v in array )
			{
				action( v ) ;
			}

			return array ;
		}

		/// <summary>
		/// 配列に含まれるか確認する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="array"></param>
		/// <param name="pattern"></param>
		/// <returns></returns>
		public static bool Contains<T>( this T[] array, T pattern ) where T : class
		{
			if( array == null || array.Length == 0 )
			{
				return false ;
			}

			if( typeof( T ) == typeof( string ) )
			{
				// 文字列
				foreach( T element in array )
				{
					string p0 = element as string ;
					string p1 = pattern as string ;

					if( p0 == p1 )
					{
						return true ;
					}
				}
			}
			else
			{
				// その他
				foreach( T element in array )
				{
					if( element == ( object )pattern )
					{
						return true ;
					}
				}
			}

			return false ;
		}

		/// <summary>
		/// そのオブジェクトが含まれる場合のインデックス値を取得する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="array"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static int IndexOf<T>( this T[] array, T pattern )
		{
			if( array == null || array.Length == 0 )
			{
				return -1 ;
			}

			int i, l = array.Length ;

			if( typeof( T ) == typeof( string ) )
			{
				// 文字列
				T element ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					element = array[ i ] ;

					string p0 = element as string ;
					string p1 = pattern as string ;

					if( p0 == p1 )
					{
						return i ;
					}
				}
			}
			else
			{
				// その他
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( ( object )array[ i ] == ( object )pattern )
					{
						return i ;
					}
				}
			}

			return -1 ;
		}

		/// <summary>
		/// 途中に挿入する(最後は押し出される)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="array"></param>
		/// <param name="pattern"></param>
		/// <returns></returns>
		public static bool Insert<T>( this T[] array, int index, T element )
		{
			if( array == null || array.Length == 0 )
			{
				return false ;
			}

			if( index <  0 || index >= array.Length )
			{
				return false ;
			}

			//----------------------------------

			int i, l = array.Length - ( index + 1 ) ;
			if( l >  0 )
			{
				for( i  = 0 ; i <  l ; i ++ )
				{
					array[ array.Length - 1 - i ] = array[ array.Length - 2 - i ] ;
				}
			}

			array[ index ] = element ;

			return true ;
		}

		/// <summary>
		/// 指定したインデックスの値を消去し前詰めを行う(最後は消去)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="array"></param>
		/// <param name="pattern"></param>
		/// <returns></returns>
		public static bool RemoveAt<T>( this T[] array, int index )
		{
			if( array == null || array.Length == 0 )
			{
				return false ;
			}

			if( index <  0 || index >= array.Length )
			{
				return false ;
			}

			//----------------------------------

			int i, l = array.Length - ( index + 1 ) ;
			if( l >  0 )
			{
				for( i  = 0 ; i <  l ; i ++ )
				{
					array[ index + i ] = array[ index + i + 1 ] ;
				}
			}

			array[ array.Length - 1 ] = default ;

			return true ;
		}

		/// <summary>
		/// 配列を指定の値で塗りつぶす
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="array"></param>
		/// <param name="fillValue"></param>
		/// <returns></returns>
		public static T[] Fill<T>( this T[] array, T fillValue = default )
		{
			if( array == null || array.Length == 0 )
			{
				return array ;
			}

			int i, l = array.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				array[ i ] = fillValue ;	// 配列への代入は foreach は使用できない
			}

			return array ;
		}


		/// <summary>
		/// 配列の要素を全て初期化する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="array"></param>
		/// <returns></returns>
		public static T[] Clear<T>( this T[] array )
		{
			if( array == null || array.Length == 0 )
			{
				return array ;
			}

			Array.Clear( array, 0, array.Length ) ;

			return array ;
		}

		/// <summary>
		/// 簡易コピーを生成する(配列内部の参照は元のまま)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="array"></param>
		/// <returns></returns>
		public static T[] Duplicate<T>( this T[] array )
		{
			if( array == null || array.Length == 0 )
			{
				return array ;
			}

			return array.Clone() as T[] ;
		}

		/// <summary>
		/// ２次元配列の指定の列を取得する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="array"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public static T[] GetColumns<T>( this T[,] array, int index )
		{
			if( array == null || array.GetLength( 0 ) == 0 || array.GetLength( 1 ) == 0 || index <  0 || index >= array.GetLength( 1 ) )
			{
				return null ;
			}

			int i, l = array.GetLength( 0 ) ;
			T[] columns = new T[ l ] ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				columns[ i ] = array[ i, index ] ;
			}

			return columns ;
		}

		/// <summary>
		/// 配列が完全に同一か比較する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="array"></param>
		/// <param name="action"></param>
		/// <returns></returns>
		public static bool Compare( byte[] a0, byte[] a1 )
		{
			if( ( a0 == null && a1 != null ) || ( a0 != null && a1 == null ) )
			{
				return false ;
			}

			if( a0 == null && a1 == null )
			{
				return true ;
			}

			if( a0.Length != a1.Length )
			{
				return false ;
			}

			int i, l = a0.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( a0[ i ] != a1[ i ] )
				{
					return false ;
				}
			}

			return true ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 条件を満たす最初の要素を１つ返す
		/// </summary>
		/// <typeparam name="K"></typeparam>
		/// <typeparam name="V"></typeparam>
		/// <param name="dictionary"></param>
		/// <returns></returns>
		public static T FirstOrDefault<T>( this T[] array, Func<T,bool> predecate )
		{
			if( array == null || array.Length == 0 || predecate == null )
			{
				return default ;
			}

			int i, l = array.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( predecate( array[ i ] ) == true )
				{
					// 発見
					return array[ i ] ;
				}
			}

			return default ;
		}

		/// <summary>
		/// 条件を満たす最後の要素を１つ返す
		/// </summary>
		/// <typeparam name="K"></typeparam>
		/// <typeparam name="V"></typeparam>
		/// <param name="dictionary"></param>
		/// <returns></returns>
		public static T LastOrDefault<T>( this T[] array, Func<T,bool> predecate )
		{
			if( array == null || array.Length == 0 || predecate == null )
			{
				return default ;
			}

			int i, l = array.Length ;
			for( i  = l - 1 ; i >= 0 ; i -- )
			{
				if( predecate( array[ i ] ) == true )
				{
					// 発見
					return array[ i ] ;
				}
			}

			return default ;
		}

		/// <summary>
		/// 条件を満たす要素を全て返す
		/// </summary>
		/// <typeparam name="K"></typeparam>
		/// <typeparam name="V"></typeparam>
		/// <param name="dictionary"></param>
		/// <returns></returns>
		public static T[] Where<T>( this T[] array, Func<T,bool> predecate )
		{
			if ( array == null )
			{
				return default ;
			}

			if( ( array.Length == 0 ) || ( predecate == null ) )
			{
				return Array.Empty<T>() ;
			}

			List<T> results = new List<T>() ;

			foreach( T value in array )
			{
				if( predecate( value ) == true )
				{
					// 発見
					results.Add( value ) ;
				}
			}

			return results.ToArray() ;
		}

		/// <summary>
		/// １つ以上の条件を満たす要素があるか判定する
		/// </summary>
		/// <typeparam name="K"></typeparam>
		/// <typeparam name="V"></typeparam>
		/// <param name="dictionary"></param>
		/// <returns></returns>
		public static bool Any<T>( this T[] array, Func<T,bool> predecate )
		{
			if( array == null || array.Length == 0 || predecate == null )
			{
				return default ;
			}

			foreach( T value in array )
			{
				if( predecate( value ) == true )
				{
					// 発見
					return true ;
				}
			}

			return false ;
		}

		/// <summary>
		/// 要素が全て条件を満たすか判定する
		/// </summary>
		/// <typeparam name="K"></typeparam>
		/// <typeparam name="V"></typeparam>
		/// <param name="dictionary"></param>
		/// <returns></returns>
		public static bool All<T>( this T[] array, Func<T,bool> predecate )
		{
			if( array == null || array.Length == 0 || predecate == null )
			{
				return default ;
			}

			foreach( T value in array )
			{
				if( predecate( value ) == false )
				{
					// 発見
					return false ;
				}
			}

			return true ;
		}

		/// <summary>
		/// 条件を満たす要素数を返す
		/// </summary>
		/// <typeparam name="K"></typeparam>
		/// <typeparam name="V"></typeparam>
		/// <param name="dictionary"></param>
		/// <returns></returns>
		public static int Count<T>( this T[] array, Func<T,bool> predecate )
		{
			if( array == null || array.Length == 0 || predecate == null )
			{
				return default ;
			}

			int count = 0 ;
			foreach( T value in array )
			{
				if( predecate( value ) == true )
				{
					// 発見
					count ++ ;
				}
			}

			return count ;
		}

		/// <summary>
		/// 要素を変更した配列を取得する
		/// </summary>
		/// <typeparam name="K"></typeparam>
		/// <typeparam name="V"></typeparam>
		/// <typeparam name="T"></typeparam>
		/// <param name="dictionary"></param>
		/// <param name="selector"></param>
		/// <returns></returns>
		public static T[] Select<T>( this T[] array, Func<T,T> selector )
		{
			if ( array == null )
			{
				return default ;
			}

			if( ( array.Length == 0 ) || ( selector == null ) )
			{
				return Array.Empty<T>() ;
			}

			List<T> results = new List<T>() ;

			foreach( T value in array )
			{
				results.Add( selector( value ) ) ;
			}

			return results.ToArray() ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 配列の中の要素を１つランダムで抽出する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="array"></param>
		/// <returns></returns>
		public static T GetRandom<T>( this T[] array )
		{
			if( array == null || array.Length == 0 )
			{
				return default ;
			}

			return array[ GetRandomRange( 0, array.Length - 1 ) ] ;
		}

		/// <summary>
		/// 配列の中の要素を複数ランダムで抽出する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="array"></param>
		/// <returns></returns>
		public static T[] GetRandom<T>( this T[] array, int count, bool isDupe = false )
		{
			if ( array == null )
			{
				return default ;
			}

			if( array.Length == 0 )
			{
				return Array.Empty<T>() ;
			}

			int i, l = array.Length ;

			if( isDupe == false )
			{
				// 重複なし
				if( count <  1 || count >  l )
				{
					count  = l ;
				}

				int[] indices = new int[ l ] ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					indices[ i ] = i ;
				}

				int r, v ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					// ランダムに位置を入れ替え
					r = GetRandomRange( 0, l - 1 ) ;
					v = indices[ i ] ;
					indices[ i ] = indices[ r ] ;
					indices[ r ] = v ;
				}

				T[] results = new T[ count ] ;
				for( i  = 0 ; i <  count ; i ++ )
				{
					results[ i ] = array[ indices[ i ] ] ;
				}

				return results ;
			}
			else
			{
				// 重複あり
				if( count <  1 )
				{
					count  = l ;
				}

				T[] results = new T[ count ] ;

				int r ;
				for( i  = 0 ; i <  count ; i ++ )
				{
					r = GetRandomRange( 0, l - 1 ) ;
					results[ i ] = array[ r ] ;
				}

				return results ;
			}
		}

		// 最小～最大の範囲でランダム値を取得する
		private static int GetRandomRange( int min, int max )
		{
//			return UnityEngine.Random.Range( min, max + 1 ) ;
			return MathHelper.Random_XorShift.Get( min, max ) ;
		}

		//---------------------------------------------------------------------------

		// 以下、必要になったら順次メソッドを追加する

		/// <summary>
		/// 条件に合致するインデックス番号を取得する
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public static int IndexOf<T>( this T[] array, Func<T,bool> onCheck )
		{
			if( array == null || array.Length == 0 )
			{
				return -1 ;
			}

			int index ;
			for( index  = 0 ; index <  array.Length ; index ++ )
			{
				if( onCheck( array[ index ] ) == true )
				{
					break ;
				}
			}

			if( index >= array.Length )
			{
				return -1 ;
			}

			return index ;
		}
	}
}

