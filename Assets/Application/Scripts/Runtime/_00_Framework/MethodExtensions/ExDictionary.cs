using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

namespace Template
{
	/// <summary>
	/// Dictionary 型のメソッド拡張 Version 2021/12/13
	/// </summary>
	public static class ExDictionary
	{
		/// <summary>
		/// 値を取得します(キーに該当する値が無い場合はデフォルト値を返します)
		/// </summary>
		/// <param name="dictionary"></param>
		/// <returns></returns>
		public static V GetValue<K,V>( this Dictionary<K,V> dictionary, K key, V value = default )
		{
			if( dictionary == null || dictionary.Count == 0 )
			{
				return default ;
			}

			if( dictionary.ContainsKey( key ) == false )
			{
				return value ;
			}

			return dictionary[ key ] ;
		}

		/// <summary>
		/// 配列が null もしくは要素数が 0 の時に true を返す
		/// </summary>
		/// <param name="dictionary"></param>
		/// <returns></returns>
		public static bool IsNullOrEmpty<K,V>( this Dictionary<K,V> dictionary )
		{
			if( dictionary == null || dictionary.Count == 0 )
			{
				return true ;
			}

			return false ;
		}

		/// <summary>
		/// キーのみを配列で取得する
		/// </summary>
		/// <typeparam name="K"></typeparam>
		/// <typeparam name="V"></typeparam>
		/// <param name="dictionary"></param>
		/// <returns></returns>
		public static K[] GetKeys<K,V>( this Dictionary<K,V> dictionary )
		{
			if( dictionary == null || dictionary.Count == 0 )
			{
				return default ;
			}

			int l = dictionary.Count ;
			K[] keys = new K[ l ] ;
			dictionary.Keys.CopyTo( keys, 0 ) ;

			return keys ;
		}

		/// <summary>
		/// バリューのみを配列で取得する
		/// </summary>
		/// <typeparam name="K"></typeparam>
		/// <typeparam name="V"></typeparam>
		/// <param name="dictionary"></param>
		/// <returns></returns>
		public static V[] GetValues<K,V>( this Dictionary<K,V> dictionary )
		{
			if( dictionary == null || dictionary.Count == 0 )
			{
				return default ;
			}

			int l = dictionary.Count ;
			V[] values = new V[ l ] ;
			dictionary.Values.CopyTo( values, 0 ) ;

			return values ;
		}

		/// <summary>
		/// 列挙メソッド
		/// </summary>
		/// <typeparam name="K"></typeparam>
		/// <typeparam name="V"></typeparam>
		/// <param name="dictionary"></param>
		/// <param name="onAction"></param>
		public static void ForEach<K,V>( this Dictionary<K,V> dictionary, Action<K,V> onAction )
		{
			if( dictionary == null || dictionary.Count == 0 )
			{
				return ;
			}

			foreach( var item in dictionary )
			{
				onAction?.Invoke( item.Key, item.Value ) ;
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 条件を満たす要素を１つ返す
		/// </summary>
		/// <typeparam name="K"></typeparam>
		/// <typeparam name="V"></typeparam>
		/// <param name="dictionary"></param>
		/// <returns></returns>
		public static KeyValuePair<K,V> FirstOrDefault<K,V>( this Dictionary<K,V> dictionary, Func<V,bool> predecate )
		{
			if( dictionary == null || dictionary.Count == 0 || predecate == null )
			{
				return default ;
			}

			K[] keys = dictionary.GetKeys() ;

			foreach( K key in keys )
			{
				if( predecate( dictionary[ key ] ) == true )
				{
					// 発見
					return new KeyValuePair<K,V>( key, dictionary[ key ] ) ;
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
		public static Dictionary<K,V> Where<K,V>( this Dictionary<K,V> dictionary, Func<V,bool> predecate )
		{
			if( dictionary == null || dictionary.Count == 0 || predecate == null )
			{
				return default ;
			}

			K[] keys = dictionary.GetKeys() ;

			Dictionary<K,V> results = new Dictionary<K,V>() ;

			foreach( K key in keys )
			{
				if( predecate( dictionary[ key ] ) == true )
				{
					// 発見
					results.Add( key, dictionary[ key ] ) ;
				}
			}

			return results ;
		}

		/// <summary>
		/// １つ以上の条件を満たす要素があるか判定する
		/// </summary>
		/// <typeparam name="K"></typeparam>
		/// <typeparam name="V"></typeparam>
		/// <param name="dictionary"></param>
		/// <returns></returns>
		public static bool Any<K,V>( this Dictionary<K,V> dictionary, Func<V,bool> predecate )
		{
			if( dictionary == null || dictionary.Count == 0 || predecate == null )
			{
				return default ;
			}

			K[] keys = dictionary.GetKeys() ;

			foreach( K key in keys )
			{
				if( predecate( dictionary[ key ] ) == true )
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
		public static bool All<K,V>( this Dictionary<K,V> dictionary, Func<V,bool> predecate )
		{
			if( dictionary == null || dictionary.Count == 0 || predecate == null )
			{
				return default ;
			}

			K[] keys = dictionary.GetKeys() ;

			foreach( K key in keys )
			{
				if( predecate( dictionary[ key ] ) == false )
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
		public static int Count<K,V>( this Dictionary<K,V> dictionary, Func<V,bool> predecate )
		{
			if( dictionary == null || dictionary.Count == 0 || predecate == null )
			{
				return default ;
			}

			K[] keys = dictionary.GetKeys() ;

			int count = 0 ;
			foreach( K key in keys )
			{
				if( predecate( dictionary[ key ] ) == true )
				{
					// 発見
					count ++ ;
				}
			}

			return count ;
		}

		/// <summary>
		/// 要素を変更した連想配列を取得する
		/// </summary>
		/// <typeparam name="K"></typeparam>
		/// <typeparam name="V"></typeparam>
		/// <typeparam name="T"></typeparam>
		/// <param name="dictionary"></param>
		/// <param name="selector"></param>
		/// <returns></returns>
		public static Dictionary<K,T> Select<K,V,T>( this Dictionary<K,V> dictionary, Func<V,T> selector )
		{
			if( dictionary == null || dictionary.Count == 0 || selector == null )
			{
				return default ;
			}

			K[] keys = dictionary.GetKeys() ;

			Dictionary<K,T> results = new Dictionary<K,T>() ;

			foreach( K key in keys )
			{
				results.Add( key, selector( dictionary[ key ] ) ) ;
			}

			return results ;
		}

		/// <summary>
		/// 連想配列をグループ分けする
		/// </summary>
		/// <typeparam name="K"></typeparam>
		/// <typeparam name="V"></typeparam>
		/// <typeparam name="T"></typeparam>
		/// <param name="dictionary"></param>
		/// <param name="selector"></param>
		/// <returns></returns>
		public static Dictionary<T,Dictionary<K,V>> GroupBy<K,V,T>( this Dictionary<K,V> dictionary, Func<V,T> keySelector )
		{
			if( dictionary == null || dictionary.Count == 0 || keySelector == null )
			{
				return default ;
			}

			K[] keys = dictionary.GetKeys() ;

			Dictionary<T,Dictionary<K,V>> results = new Dictionary<T,Dictionary<K,V>>() ;
			T groupKey ;

			foreach( K key in keys )
			{
				groupKey = keySelector( dictionary[ key ] ) ;
				if( results.ContainsKey( groupKey ) == false )
				{
					results.Add( groupKey, new Dictionary<K,V>() ) ;
				}
				results[ groupKey ].Add( key, dictionary[ key ] ) ;
			}

			return results ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 配列の中の要素を１つランダムで抽出する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="array"></param>
		/// <returns></returns>
		public static KeyValuePair<K,V> GetRandom<K,V>( this Dictionary<K,V> dictionary )
		{
			if( dictionary == null || dictionary.Count == 0 )
			{
				return default ;
			}

			K[] keys = dictionary.GetKeys() ;

			K k = keys[ GetRandomRange( 0, dictionary.Count - 1 ) ] ;
			return new KeyValuePair<K,V>( k, dictionary[ k ] ) ;
		}

		/// <summary>
		/// 配列の中の要素を複数ランダムで抽出する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="array"></param>
		/// <returns></returns>
		public static Dictionary<K,V> GetRandom<K,V>( this Dictionary<K,V> dictionary, int count )
		{
			if( dictionary == null || dictionary.Count == 0 )
			{
				return default ;
			}

			int i, l = dictionary.Count ;
			K[] keys = dictionary.GetKeys() ;

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

			Dictionary<K,V> results = new Dictionary<K,V>() ;
			K k ;
			for( i  = 0 ; i <  count ; i ++ )
			{
				k = keys[ indices[ i ] ] ;
				results.Add( k, dictionary[ k ] ) ;
			}

			return results ;
		}

		// 最小～最大の範囲でランダム値を取得する
		private static int GetRandomRange( int min, int max )
		{
//			return UnityEngine.Random.Range( min, max + 1 ) ;
			return MathHelper.Random_XorShift.Get( min, max ) ;
		}

		//---------------------------------------------------------------------------

		// 以下、必要になったら順次メソッドを追加する
	}
}

