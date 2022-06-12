using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

namespace Template
{
	/// <summary>
	/// List 型のメソッド拡張 Version 2022/06/09
	/// </summary>
	public static class ExList
	{
		/// <summary>
		/// 配列が null もしくは要素数が 0 の時に true を返す
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public static bool IsNullOrEmpty<T>( this List<T> list )
		{
			if( list == null || list.Count == 0 )
			{
				return true ;
			}

			return false ;
		}

		/// <summary>
		/// 要素をまとめて破棄する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <param name="collection"></param>
		public static void RemoveRange<T>( this List<T> list, IEnumerable<T> collection )
		{
			if( collection == null )
			{
				return ;
			}

			foreach( T element in collection )
			{
				list.Remove( element ) ;
			}
		}

		/// <summary>
		/// リストの末尾を取得する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <returns></returns>
		public static T GetEnd<T>( this List<T> list )
		{
			if( list == null || list.Count == 0 )
			{
				return default ;
			}

			return list[ list.Count - 1 ] ;
		}

		/// <summary>
		/// リストの末尾を削除する
		/// </summary>
		/// <param name="list"></param>
		public static T RemoveEnd<T>( this List<T> list )
		{
			if( list == null || list.Count == 0 )
			{
				return default ;
			}

			int index = list.Count - 1 ;
			T last = list[ index ] ;
			list.RemoveAt( index ) ;

			return last ;	// 末尾を返す
		}

		/// <summary>
		/// リストを複製する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <returns></returns>
		public static List<T> Clone<T>( this List<T> list )
		{
			if( list == null )
			{
				return default ;
			}

			List<T> clone = new List<T>() ;

			clone.AddRange( list ) ;

			return clone ;
		}

		/// <summary>
		/// ２つのリストが完全に同一内容か比較する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static bool Compare<T>( this List<T> list, List<T> collection ) where T : class
		{
			if( list.Count != collection.Count )
			{
				return false ;
			}

			int i, l = list.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( list[ i ] != collection[ i ] )
				{
					return false ;
				}
			}

			return true ;
		}

		/// <summary>
		/// ソートする(true であれば a を優先する)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <param name="onCompare"></param>
		/// <param name="isReserve"></param>
		public static void Sort<T>( this List<T> list, Func<T,T,bool> onCompare, bool isReserve = false ) where T : class
		{
			if( list.Count <= 1 || onCompare == null )
			{
				// ソートの必要無し
				return ;
			}

			T swap ;
			int i, j ;
			for( i  = 0 ; i <  ( list.Count - 1 ) ; i ++ )
			{
				for( j  = ( i + 1 ) ; j <  list.Count ; j ++ )
				{
					bool r = onCompare( list[ i ], list[ j ] ) ;
					if( ( isReserve == false && r == false ) || ( isReserve == true && r == true ) )
					{
						swap = list[ i ] ;
						list[ i ] = list[ j ] ;
						list[ j ] = swap ;
					}
				}
			}
		}

		/// <summary>
		/// ソートする
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static List<T> Sort<T,K>( this List<T> list, Func<T,K> onCompare, bool isReverse = false ) where T : class
		{
			if( list.Count <= 1 || onCompare == null )
			{
				// ソートの必要無し
				return list ;
			}

			Type t = typeof( K ) ;
			if
			(
				t != typeof( byte ) &&
				t != typeof( char ) &&
				t != typeof( short ) &&
				t != typeof( ushort ) &&
				t != typeof( int ) &&
				t != typeof( uint ) &&
				t != typeof( long ) &&
				t != typeof( ulong ) &&
				t != typeof( float ) &&
				t != typeof( double ) &&
				t != typeof( string )
			)
			{
				// ソートできない
				return list ;
			}

			bool Compare( K a, K b, bool isReverse )
			{
				if( t == typeof( byte ) )
				{
					byte av = ( byte )( object )a ;
					byte bv = ( byte )( object )b ;

					return isReverse == false ? ( bv <  av ) : ( bv >  av ) ; 
				}
				else
				if( t == typeof( char ) )
				{
					char av = ( char )( object )a ;
					char bv = ( char )( object )b ;

					return isReverse == false ? ( bv <  av ) : ( bv >  av ) ; 
				}
				else
				if( t == typeof( short ) )
				{
					short av = ( short )( object )a ;
					short bv = ( short )( object )b ;

					return isReverse == false ? ( bv <  av ) : ( bv >  av ) ; 
				}
				else
				if( t == typeof( ushort ) )
				{
					ushort av = ( ushort )( object )a ;
					ushort bv = ( ushort )( object )b ;

					return isReverse == false ? ( bv <  av ) : ( bv >  av ) ; 
				}
				else
				if( t == typeof( int ) )
				{
					int av = ( int )( object )a ;
					int bv = ( int )( object )b ;

					return isReverse == false ? ( bv <  av ) : ( bv >  av ) ; 
				}
				else
				if( t == typeof( uint ) )
				{
					uint av = ( uint )( object )a ;
					uint bv = ( uint )( object )b ;

					return isReverse == false ? ( bv <  av ) : ( bv >  av ) ; 
				}
				else
				if( t == typeof( long ) )
				{
					long av = ( long )( object )a ;
					long bv = ( long )( object )b ;

					return isReverse == false ? ( bv <  av ) : ( bv >  av ) ; 
				}
				else
				if( t == typeof( ulong ) )
				{
					ulong av = ( ulong )( object )a ;
					ulong bv = ( ulong )( object )b ;

					return isReverse == false ? ( bv <  av ) : ( bv >  av ) ; 
				}
				else
				if( t == typeof( float ) )
				{
					float av = ( float )( object )a ;
					float bv = ( float )( object )b ;

					return isReverse == false ? ( bv <  av ) : ( bv >  av ) ; 
				}
				else
				if( t == typeof( double ) )
				{
					double av = ( double )( object )a ;
					double bv = ( double )( object )b ;

					return isReverse == false ? ( bv <  av ) : ( bv >  av ) ; 
				}
				else
				if( t == typeof( string ) )
				{
					string av = ( string )( object )a ;
					string bv = ( string )( object )b ;

					int r = string.Compare( av, bv ) ;
					return isReverse == false ? r > 0 : r < 0 ; 
				}
				return false ;
			}


			T swap ;
			int i, j ;
			for( i  = 0 ; i <  ( list.Count - 1 ) ; i ++ )
			{
				for( j  = ( i + 1 ) ; j <  list.Count ; j ++ )
				{
					K a = onCompare( list[ i ] ) ;
					K b = onCompare( list[ j ] ) ;

					if( Compare( a, b, isReverse ) == true )
					{
						swap = list[ i ] ;
						list[ i ] = list[ j ] ;
						list[ j ] = swap ;
					}
				}
			}

			return list ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 配列の中の要素を１つランダムで抽出する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="array"></param>
		/// <returns></returns>
		public static T GetRandom<T>( this List<T> list )
		{
			if( list == null || list.Count == 0 )
			{
				return default ;
			}

			return list[ GetRandomRange( 0, list.Count - 1 ) ] ;
		}

		/// <summary>
		/// 配列の中の要素を複数ランダムで抽出する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="array"></param>
		/// <returns></returns>
		public static List<T> GetRandom<T>( this List<T> list, int count, bool isDupe = false )
		{
			if( list == null || list.Count == 0 )
			{
				return default ;
			}

			int i, l = list.Count ;

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

				List<T> results = new List<T>() ;
				for( i  = 0 ; i <  count ; i ++ )
				{
					results.Add( list[ indices[ i ] ] ) ;
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

				List<T> results = new List<T>() ;

				int r ;
				for( i  = 0 ; i <  count ; i ++ )
				{
					r = GetRandomRange( 0, l - 1 ) ;
					results.Add( list[ r ] ) ;
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
		public static int IndexOf<T>( this List<T> list, Func<T,bool> onCheck )
		{
			if( list == null || list.Count == 0 )
			{
				return -1 ;
			}

			int index ;
			for( index  = 0 ; index <  list.Count ; index ++ )
			{
				if( onCheck( list[ index ] ) == true )
				{
					break ;
				}
			}

			if( index >= list.Count )
			{
				return -1 ;
			}

			return index ;
		}

		/// <summary>
		/// リスト内の重複している内容を削除します
		/// </summary>
		/// <param name="list"></param>
		/// <typeparam name="T"></typeparam>
		public static void DistinctSelf<T>( this List<T> list )
		{
			var oldValue = list.ToArray() ;
			list.Clear();
			foreach ( var value in oldValue )
			{
				if ( list.Contains( value ) )
				{
					continue ;
				}

				list.Add( value ) ;
			}
		}
	}
}

