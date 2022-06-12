using System ;
using System.Collections.Generic ;
using System.Linq ;
using UnityEngine ;

// Version 2020/03/31 0
namespace JsonHelper
{
	public class JsonObject
	{
		protected Dictionary<string, object> m_Entities = new Dictionary<string, object>() ;	
	
		public object this[ string label ]
		{
			get
			{
				return Get( label ) ;
			}
			set
			{
				Put( label, ( object )value ) ;
			}
		}
	
		/// <summary>
		/// ラベル名でソートを行う
		/// </summary>
		/// <param name="reverse"></param>
		public void Sort( bool reverse = false )
		{
			List<KeyValuePair<string,object>> entities = m_Entities.ToList() ;

			if( reverse == false )
			{
				// 昇順
				entities.Sort( ( a, b ) => ( string.CompareOrdinal( a.Key, b.Key ) ) ) ;
			}
			else
			{
				// 降順
				entities.Sort( ( a, b ) => ( string.CompareOrdinal( b.Key, a.Key ) ) ) ;
			}

			m_Entities = entities.ToDictionary( d => d.Key, d => d.Value ) ;
		}

		/// <summary>
		/// 指定したラベルの要素を指定した分移動させる
		/// </summary>
		/// <param name="label"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool Move( string label, int value )
		{
			if( m_Entities.ContainsKey( label ) == false )
			{
				// そのような名前の要素は存在しない
				return false ;
			}

			if( value == 0 )
			{
				// 移動しないので意味無し
				return true ;
			}

			// 一度リストにする
			List<KeyValuePair<string,object>> entities = m_Entities.ToList() ;

			// 現在何番目にあるか確認する
			int i, l = entities.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( entities[ i ].Key == label )
				{
					break ;
				}
			}

			int index = i + value ;

			if( index <  0 )
			{
				index  = 0 ;
			}
			else
			if( index >  ( l - 1 ) )
			{
				index   =  l - 1 ;
			}

			if( index == i )
			{
				// 結局同じ位置なので移動させる意味無し
				return true ;
			}

			// 移動を実行する
			KeyValuePair<string,object> target = entities[ i ] ;
			entities.RemoveAt( i ) ;
			entities.Insert( index, target ) ;

			// ディクショナリに戻す
			m_Entities = entities.ToDictionary( d => d.Key, d => d.Value ) ;

			return true ;
		}
		//-----------------------------------
	
		public JsonObject()
		{
		}
		
		public JsonObject( Dictionary<string,object> entities )
		{
			if( entities != null && entities.Count >  0 )
			{
				int i, l = entities.Count ;
				string[] key = new string[ l ] ;
				entities.Keys.CopyTo( key, 0 ) ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					Put( key[ i ], entities[ key[ i ] ] ) ;
				}
			}
		}

		public JsonObject( string source )
		{
			Parse( source, 0 ) ;
		}
	
		public int Parse( string source )
		{
			return Parse( source, 0 ) ;
		}
		
		// スペース・タブ・改行などをスキップする
		private int Skip( string source, int offset )
		{
			int i, l = source.Length, j ;
			char c ;

			if( offset >= l )
			{
				return l ;	// オーバー
			}

			for( i  = offset ; i <  l ; i ++ )
			{
				c = source[ i ] ;

				if( c == ' ' || c == '\r' || c == '\n' || c == '\t' )
				{
					// 読み飛ばし
				}
				else
				if( c == '/' && ( i + 1 ) <  l && source[ i + 1 ] == '/' )
				{
					// コメント発見
					for( j  = i + 2 ; j <  l ; j ++ )
					{
						c = source[ j ] ;
						if( c == '\r' || c == '\n' )
						{
							// コメント終了
							j ++ ;
							break ;
						}
					}

					if( j >= l )
					{
						return l ;	// オーバー
					}

					i = j - 1 ;	// コメントの最後位置へ
				}
				else
				{
					// コメント以外の何らかの文字
					return i ;
				}
			}

			return l ;	// オーバー
		}


		// 戻り値は次のインデックス位置
		public int Parse( string source, int offset )
		{
			if( string.IsNullOrEmpty( source ) == true || source.Length == 0 )
			{
				// 構文エラー
				return -1 ;
			}
			
			//----------------------------------

			if( m_Entities == null )
			{
				m_Entities = new Dictionary<string, object>() ;
			}
			else
			{
				m_Entities.Clear() ;
			}
		
			//----------------------------------

			int l = source.Length ;

			offset = Skip( source, offset ) ;
			if( offset >= l )
			{
				return offset ;	// 終了(結果 null)
			}

			if( source[ offset ] != '{' )
			{
				// 構文エラー
				return -1 ;
			}
		
			offset ++ ;
		
			//-----------------------------------------------------
		
			string label = "" ;
			object value = null ;
			int p = 0 ;
			int w = 0 ;
			int f = -1 ;
			char c ;
			int i, o = -1, m ;
			bool escape = false ;
		
			for( i  = offset ; i <  l ; i ++ )
			{
				if( p == 0 )
				{
					// 名
					if( w == 0 )
					{
						// Label 前

						i = Skip( source, i ) ;
						if( i >= l )
						{
							// 構文エラー({ が } で閉じられていない)
							m_Entities.Clear() ;
							return -1 ;
						}

						c = source[ i ] ;
			
						// ラベルの外側
						if( c == '}' )
						{
							// いきなり終了
							f = i + 1 ;
							break ;
						}
						else
						if( c == ',' )
						{
							// 項目省略
							// JsonObject の場合は何もしない
						}
						else
						if( c == '"' )
						{
							o = i ;
							w = 1 ;	// キーワードの内側
						}
						else
//						if( c == ' ' || c == '\r' || c == '\n' || c == '\t' )
//						{
//							// 読み飛ばし
//						}
//						else
						{
							// 構文エラー(不正な文字)
							m_Entities.Clear() ;
							return -1 ;
						}
					}
					else
					if( w == 1 )
					{
						// Label 中

						c = source[ i ] ;

						// ラベルの内側
						if( c == '\\' )
						{
							// エスケープ
							escape = true ;
						}
						else
						{
							if( escape == true )
							{
								// エスケープ解除
								escape  = false ;
							}
							else
							{
								if( c == '"' )
								{
									m = ( i - 1 ) - ( o + 1 ) + 1 ;
									if( m <  1 )
									{
										// 構文エラー(ラベル名が定義されていない)
										m_Entities.Clear() ;
										return -1 ;
									}
								
									label = source.Substring( o + 1, m ) ;
								
									w = 2 ;	// キーワードの外側
								}
							}
						}
					}
					else
					if( w == 2 )
					{
						// Label 後
						i = Skip( source, i ) ;
						if( i >= l )
						{
							// 構文エラー({ が } で閉じられていない)
							m_Entities.Clear() ;
							return -1 ;
						}

						c = source[ i ] ;

						// 名が終わった
						if( c == ':' )
						{
							p = 1 ;	// 値へ
							w = 0 ;
						}
						else
//						if( c == ' ' || c == '\r' || c == '\n' || c == '\t' )
//						{
//							// 読み飛ばし
//						}
//						else
						{
							// 構文エラー(不正な文字)
							m_Entities.Clear() ;
							return -1 ;
						}
					}
				}
				else
				{
					// 値
					if( w == 0 )
					{
						// Value 前

						i = Skip( source, i ) ;
						if( i >= l )
						{
							// 構文エラー({ が } で閉じられていない)
							m_Entities.Clear() ;
							return -1 ;
						}

						c = source[ i ] ;

						if( c == ',' || c == '}' )
						{
							// 値は空
							value = "" ;
						
							if( m_Entities.ContainsKey( label ) == true )
							{
								m_Entities.Remove( label ) ;
							}
						
							m_Entities.Add( label, value ) ;
						
							if( c == '}' )
							{
								// 無事終了
								f = i + 1 ;
								break ;
							}
						
							p = 0 ;	// 名へ
							w = 0 ;
						}
						else
						if( c == '{' )
						{
							// 値は JsonObject
							JsonObject jo = new JsonObject() ;
						
							i = jo.Parse( source, i ) ;
						
							if( i <  0 || i >= l )
							{
								// 構文エラー({ が } で閉じられていない)
								m_Entities.Clear() ;
								return -1 ;
							}
						
							i -- ;
						
							value = jo ;
						
							w = 3 ;	// 閉じへ
						}
						else
						if( c == '[' )
						{
							// 値は JsonArray
							JsonArray ja = new JsonArray() ;
						
							i = ja.Parse( source, i ) ;
						
							if( i <  0 || i >= l )
							{
								// 構文エラー({ が } で閉じられていない)
								m_Entities.Clear() ;
								return -1 ;
							}
						
							i -- ;
						
							value = ja ;
						
							w = 3 ;	//　閉じへ
						}
						else
//						if( c == ' ' || c == '\r' || c == '\n' || c == '\t' )
//						{
//							// 読み飛ばし
//						}
//						else
						{
							// 数値か文字
							o = i ;
							if( c != '"' )
							{
								// 数値
								w = 1 ;
							}
							else
							{
								// 文字
								w = 2 ;
							}
						}
					}
					else
					if( w == 1 )
					{
						// Value 中(数値)

						// 数値
						c = source[ i ] ;

						if( c == ',' || c == '}' || c <  0x21 || c >  0x7E )
						{
							// 数値切り出し
							m = i - o ;
							string s ;
							if( m >  0 )
							{
								s = source.Substring( o, m ) ;
							}
							else
							{
								s = "" ;
							}
								
							value = s ;

							i -- ;	// １文字前へ
							w = 3 ;	// 閉じへ
						}
					}
					else
					if( w == 2 )
					{
						// Value 中(文字)

						// 文字
						c = source[ i ] ;

						if( c == '\\' )
						{
							// エスケープ
							escape = true ;
						}
						else
						{
							if( escape == true )
							{
								// エスケープ解除
								escape  = false ;
							}
							else
							{
								if( c == '"' )
								{
									// 文字列切り出し
									m = ( i + 1 ) - o ;
									string s ;
									if( m >  0 )
									{
										s = source.Substring( o, m ) ;
									}
									else
									{
										s = "\"\"" ;
									}
								
									value = s ;
								
									w = 3 ;	// 閉じへ
								}
							}
						}
					}
					else
					if( w == 3 )
					{
						// Value 後

						i = Skip( source, i ) ;
						if( i >= l )
						{
							// 構文エラー({ が } で閉じられていない)
							m_Entities.Clear() ;
							return -1 ;
						}

						c = source[ i ] ;

						if( c == ',' || c == '}' )
						{
							// 次の項目へ
						
							if( m_Entities.ContainsKey( label ) == true )
							{
								m_Entities.Remove( label ) ;
							}
												
							m_Entities.Add( label, value ) ;
						
							if( c == '}' )
							{
								// 無事終了
								f = i + 1 ;
								break ;
							}
						
							p = 0 ;	// 名へ
							w = 0 ;
						}
						else
//						if( c == ' ' || c == '\r' || c == '\n' || c == '\t' )
//						{
//							// 読み飛ばし
//						}
//						else
						{
							// 構文エラー(不正な文字)
							m_Entities.Clear() ;
							return -1 ;
						}
					}
				}
			}
		
			if( f <  0 )
			{
				// 構文エラー
				m_Entities.Clear() ;
				return -1 ;
			}
		
			// 成功
			return f ;
		}
		
		//-----------------------------------
		
		public bool Has( string label )
		{
			if( m_Entities.ContainsKey( label ) == true )
			{
				return true ;
			}
		
			return false ;
		}
	
		public bool IsNull( string label )
		{
			if( m_Entities.ContainsKey( label ) == false )
			{
	//			Debug.LogWarning( "JsonObject IsNull : Not found [ " + tLabel + " ]" ) ;
				return true ;
			}
		
			if( m_Entities[ label ] == null )
			{
				return true ;
			}
		
			if( m_Entities[ label ] is string )
			{
				string s = m_Entities[ label ] as string ;
				if( string.IsNullOrEmpty( s ) == true )
				{
					return true ;
				}
			
				if( s.Equals( "\"\"" ) == true )
				{
					return true ;
				}
			
				if( s.Equals( "null" ) == true || s.Equals( "NULL" ) == true )
				{
					return true ;
				}
			}
			else
			{
				if( m_Entities[ label ] == null )
				{
					return true ;
				}
			
				if( m_Entities[ label ] is JsonObject )
				{
					JsonObject jo = m_Entities[ label ] as JsonObject ;
					if( jo.Length == 0 )
					{
						return true ;
					}
				}
			
				if( m_Entities[ label ] is JsonArray )
				{
					JsonArray ja = m_Entities[ label ] as JsonArray ;
					if( ja.Length == 0 )
					{
						return true ;
					}
				}
			}
			
			return false ;
		}
	
		public int Length
		{
			get
			{
				return m_Entities.Count ;
			}
		}
		
		//-----------------------------------
		// 短縮版
		
		public object Get( string label )
		{
			if( m_Entities.ContainsKey( label ) == false )
			{
				Debug.LogWarning( "JsonObject Get : Not found [ " + label + " ]" ) ;
				return null ;
			}
		
			if( m_Entities[ label ] == null )
			{
				return null ;
			}
			
			object value = m_Entities[ label ] ;

			if( value is string )
			{
				string s = value as string ;
				bool isString = false ;
			
				if( s.Length >= 2 )
				{
					int l = s.Length ;
					if( s[ 0 ] == '"' && s[ l - 1 ] == '"' )
					{
						isString = true ;
						s = s.Substring( 1, l - 2 ) ;
					}
				}
			
				if( isString == true )
				{
					return s ;
				}
				else
				{
					double result = 0 ;
				
					try
					{
						result = double.Parse( s ) ;
					}
					catch( Exception e )
					{
						Debug.LogError( "JsonObject Get : [ " + label + " ] Parse Error : " + e.Message ) ;
					}
				
					return result ;
				}
			}
		
			return value ;
		}
		
		//-----------------------------------
		// 以下は Java と互換

		public bool GetBool( string label )
		{
			return GetBoolean( label ) ;
		}
		
		public bool GetBoolean( string label )
		{
			if( m_Entities.ContainsKey( label ) == false )
			{
				Debug.LogWarning( "JsonObject GetBoolean : Not found [ " + label + " ]" ) ;
				return false ;
			}
		
			if( m_Entities[ label ] == null )
			{
				return false ;
			}
		
			if( m_Entities[ label ] is string )
			{
				string s = m_Entities[ label ] as string ;
				if( string.IsNullOrEmpty( s ) == true )
				{
					return false ;
				}
			
				if( s.Length >= 2 )
				{
					int l = s.Length ;
					if( s[ 0 ] == '"' && s[ l - 1 ] == '"' )
					{
						s = s.Substring( 1, l - 2 ) ;
					}
				}
				
				s = s.ToLower() ;
				if( s.Equals( "true" ) == true || s.Equals( "1" ) == true )
				{
					return true ;
				}
			
				return false ;
			}
			else
			{
				// JsonObject か JSONArray に対して行おうとした
				return false ;
			}
		}
		
		public int GetInt( string label )
		{
			if( m_Entities.ContainsKey( label ) == false )
			{
				Debug.LogWarning( "JsonObject GetInt : Not found [ " + label + " ]" ) ;
				return 0 ;
			}
		
			if( m_Entities[ label ] == null )
			{
				return 0 ;
			}
		
			if( m_Entities[ label ] is string )
			{
				string s = m_Entities[ label ] as string ;
				if( string.IsNullOrEmpty( s ) == true )
				{
					return 0 ;
				}
			
				if( s.Length >= 2 )
				{
					int l = s.Length ;
					if( s[ 0 ] == '"' && s[ l - 1 ] == '"' )
					{
						s = s.Substring( 1, l - 2 ) ;
					}
				}
			
				int result = 0 ;
			
				try
				{
					result = ( int )double.Parse( s ) ;
				}
				catch( Exception e )
				{
					Debug.LogError( "JsonObject GetInt : [ " + label + " ] Parse Error : " + e.Message + " Value=" + s ) ;
				}
			
				return result ;
			}
			else
			{
				// JsonObject か JSONArray に対して行おうとした
				return 0 ;
			}
		}
		
		public long GetLong( string label )
		{
			if( m_Entities.ContainsKey( label ) == false )
			{
				Debug.LogWarning( "JsonObject GetLong : Not found [ " + label + " ]" ) ;
				return 0 ;
			}
		
			if( m_Entities[ label ] == null )
			{
				return 0 ;
			}
		
			if( m_Entities[ label ] is string )
			{
				string s = m_Entities[ label ] as string ;
				if( string.IsNullOrEmpty( s ) == true )
				{
					return 0 ;
				}
			
				if( s.Length >= 2 )
				{
					int l = s.Length ;
					if( s[ 0 ] == '"' && s[ l - 1 ] == '"' )
					{
						s = s.Substring( 1, l - 2 ) ;
					}
				}
			
				long result = 0 ;
			
				try
				{
					result = ( long )double.Parse( s ) ;
				}
				catch( Exception e )
				{
					Debug.LogError( "JsonObject GetDouble : [ " + label + " ] Parse Error : " + e.Message + " Value=" + s ) ;
				}
			
				return result ;
			}
			else
			{
				// JsonObject か JSONArray に対して行おうとした
				return 0 ;
			}
		}
		
		public float GetFloat( string label )
		{
			if( m_Entities.ContainsKey( label ) == false )
			{
				Debug.LogWarning( "JsonObject GetFloat : Not found [ " + label + " ]" ) ;
				return 0 ;
			}
		
			if( m_Entities[ label ] == null )
			{
				return 0 ;
			}
		
			if( m_Entities[ label ] is string )
			{
				string s = m_Entities[ label ] as string ;
				if( string.IsNullOrEmpty( s ) == true )
				{
					return 0 ;
				}
			
				if( s.Length >= 2 )
				{
					int l = s.Length ;
					if( s[ 0 ] == '"' && s[ l - 1 ] == '"' )
					{
						s = s.Substring( 1, l - 2 ) ;
					}
				}
			
				float result = 0 ;
			
				try
				{
					result = ( float )double.Parse( s ) ;
				}
				catch( Exception e )
				{
					Debug.LogError( "JsonObject GetFloat : [ " + label + " ] Parse Error : " + e.Message  + " Value=" + s ) ;
				}
			
				return result ;
			}
			else
			{
				// JsonObject か JSONArray に対して行おうとした
				return 0 ;
			}
		}
		
		public double GetDouble( string label )
		{
			if( m_Entities.ContainsKey( label ) == false )
			{
				Debug.LogWarning( "JsonObject GetDouble : Not found [ " + label + " ]" ) ;
				return 0 ;
			}
		
			if( m_Entities[ label ] == null )
			{
				return 0 ;
			}
		
			if( m_Entities[ label ] is string )
			{
				string s = m_Entities[ label ] as string ;
				if( string.IsNullOrEmpty( s ) == true )
				{
					return 0 ;
				}
			
				if( s.Length >= 2 )
				{
					int l = s.Length ;
					if( s[ 0 ] == '"' && s[ l - 1 ] == '"' )
					{
						s = s.Substring( 1, l - 2 ) ;
					}
				}
			
				double result = 0 ;
			
				try
				{
					result = double.Parse( s ) ;
				}
				catch( Exception e )
				{
					Debug.LogError( "JsonObject GetDouble : [ " + label + " ] Parse Error : " + e.Message  + " Value=" + s ) ;
				}
			
				return result ;
			}
			else
			{
				// JsonObject か JSONArray に対して行おうとした
				return 0 ;
			}
		}
		
		public string GetString( string label )
		{
			if( m_Entities.ContainsKey( label ) == false )
			{
				Debug.LogWarning( "JsonObject GetString : Not found [ " + label + " ]" ) ;
				return "" ;
			}
		
			if( m_Entities[ label ] == null )
			{
				return "" ;
			}
		
			if( m_Entities[ label ] is string )
			{
				string s = m_Entities[ label ] as string ;
				
				if( s.Length >= 2 )
				{
					int l = s.Length ;
					if( s[ 0 ] == '"' && s[ l - 1 ] == '"' )
					{
						s = s.Substring( 1, l - 2 ) ;
					}
				}
			
				return s ;
			}
			else
			{
				// JsonObject か JSONArray に対して行おうとした
				return "" ;
			}
		}
		
		public JsonObject GetObject( string label )
		{
			if( m_Entities.ContainsKey( label ) == false )
			{
				Debug.LogWarning( "JsonObject GetObject : Not found [ " + label + " ]" ) ;
				return null ;
			}
		
			if( m_Entities[ label ] == null )
			{
				return null ;
			}
		
			if( m_Entities[ label ] is JsonObject )
			{
				return m_Entities[ label ] as JsonObject ;
			}
		
			return null ;
		}
		
		public JsonArray GetArray( string label )
		{
			if( m_Entities.ContainsKey( label ) == false )
			{
				Debug.LogWarning( "JsonObject GetArray : Not found [ " + label + " ]" ) ;
				return null ;
			}
		
			if( m_Entities[ label ] == null )
			{
				return null ;
			}
		
			if( m_Entities[ label ] is JsonArray )
			{
				return m_Entities[ label ] as JsonArray ;
			}
		
			return null ;
		}
		
		//-----------------------------------
		// 短縮版
		
		public JsonObject Put( string label, object value )
		{
			if( value != null )
			{
				if( value is bool )
				{
					value = ( ( bool )value ).ToString().ToLower() ;
				}
				else
				if( value is int )
				{
					value = ( ( int )value ).ToString() ;
				}
				else
				if( value is long )
				{
					value = ( ( long )value ).ToString() ;
				}
				else
				if( value is float )
				{
					value = ( ( float )value ).ToString() ;
				}
				else
				if( value is double )
				{
					value = ( ( double )value ).ToString() ;
				}
				else
				if( value is string )
				{
					value = "\"" + ( string )value + "\"" ;
				}
				else
				if( value is bool[] )
				{
					value = new JsonArray().PutBoolean( ( bool[] )value ) ;
				}
				else
				if( value is int[] )
				{
					value = new JsonArray().PutInt( ( int[] )value ) ;
				}
				else
				if( value is long[] )
				{
					value = new JsonArray().PutLong( ( long[] )value ) ;
				}
				else
				if( value is float[] )
				{
					value = new JsonArray().PutFloat( ( float[] )value ) ;
				}
				else
				if( value is Double[] )
				{
					value = new JsonArray().PutDouble( ( double[] )value ) ;
				}
				else
				if( value is string[] )
				{
					value = new JsonArray().PutString( ( string[] )value ) ;
				}
				else
				if( value is JsonObject[] )
				{
					value = new JsonArray().PutObject( ( JsonObject[] )value ) ;
				}
				else
				if( value is JsonArray[] )
				{
					value = new JsonArray().PutArray( ( JsonArray[] )value ) ;
				}
			}
			
			if( m_Entities.ContainsKey( label ) == false )
			{
				m_Entities.Add( label, value ) ;
			}
			else
			{
				m_Entities[ label ] = value ;
			}
			return this ;
		}

		public JsonObject Put( string label, bool value )
		{
			return PutBoolean( label, value ) ;
		}
	
		public JsonObject Put( string label, int value )
		{
			return PutInt( label, value ) ;
		}
	
		public JsonObject Put( string label, long value )
		{
			return PutLong( label, value ) ;
		}
	
		public JsonObject Put( string label, float value )
		{
			return PutFloat( label, value ) ;
		}
	
		public JsonObject Put( string label, double value )
		{
			return PutDouble( label, value ) ;
		}
	
		public JsonObject Put( string label, string value )
		{
			return PutString( label, value ) ;
		}
	
		public JsonObject Put( string label, JsonObject value )
		{
			return PutObject( label, value ) ;
		}
	
		public JsonObject Put( string label, JsonArray value )
		{
			return PutArray( label, value ) ;
		}
		
		//-----------------------------------
		// 以下は Java と互換

		private void ToLast( string label )
		{
			Move( label, m_Entities.Count ) ;
		}

		public JsonObject PutBoolean( string label, bool value )
		{
			if( m_Entities.ContainsKey( label ) == false )
			{
				m_Entities.Add( label, value.ToString().ToLower() ) ;

				// キャッシュから消えないと内部的には上書きされてしまうので明示的に最後に移動させる
				ToLast( label ) ;
			}
			else
			{
				m_Entities[ label ] = value.ToString() ;
			}
			return this ;
		}
	
		public JsonObject PutInt( string label, int value )
		{
			if( m_Entities.ContainsKey( label ) == false )
			{
				m_Entities.Add( label, value.ToString() ) ;

				// キャッシュから消えないと内部的には上書きされてしまうので明示的に最後に移動させる
				ToLast( label ) ;
			}
			else
			{
				m_Entities[ label ] = value.ToString() ;
			}
			return this ;
		}
	
		public JsonObject PutLong( string label, long value )
		{
			if( m_Entities.ContainsKey( label ) == false )
			{
				m_Entities.Add( label, value.ToString() ) ;

				// キャッシュから消えないと内部的には上書きされてしまうので明示的に最後に移動させる
				ToLast( label ) ;
			}
			else
			{
				m_Entities[ label ] = value.ToString() ;
			}
			return this ;
		}
	
		public JsonObject PutFloat( string label, float value )
		{
			if( m_Entities.ContainsKey( label ) == false )
			{
				m_Entities.Add( label, value.ToString() ) ;

				// キャッシュから消えないと内部的には上書きされてしまうので明示的に最後に移動させる
				ToLast( label ) ;
			}
			else
			{
				m_Entities[ label ] = value.ToString() ;
			}
			return this ;
		}
	
		public JsonObject PutDouble( string label, double value )
		{
			if( m_Entities.ContainsKey( label ) == false )
			{
				m_Entities.Add( label, value.ToString() ) ;

				// キャッシュから消えないと内部的には上書きされてしまうので明示的に最後に移動させる
				ToLast( label ) ;
			}
			else
			{
				m_Entities[ label ] = value.ToString() ;
			}
			return this ;
		}
	
		public JsonObject PutString( string label, string value )
		{
			if( m_Entities.ContainsKey( label ) == false )
			{
				m_Entities.Add( label, "\"" + value + "\"" ) ;

				// キャッシュから消えないと内部的には上書きされてしまうので明示的に最後に移動させる
				ToLast( label ) ;
			}
			else
			{
				m_Entities[ label ] = "\"" + value + "\"" ;
			}
			return this ;
		}
	
		public JsonObject PutObject( string label, JsonObject value )
		{
			if( m_Entities.ContainsKey( label ) == false )
			{
				m_Entities.Add( label, value ) ;

				// キャッシュから消えないと内部的には上書きされてしまうので明示的に最後に移動させる
				ToLast( label ) ;
			}
			else
			{
				m_Entities[ label ] = value ;
			}
			return this ;
		}
	
		public JsonObject PutArray( string label, JsonArray value )
		{
			if( m_Entities.ContainsKey( label ) == false )
			{
				m_Entities.Add( label, value ) ;

				// キャッシュから消えないと内部的には上書きされてしまうので明示的に最後に移動させる
				ToLast( label ) ;
			}
			else
			{
				m_Entities[ label ] = value ;
			}
			return this ;
		}
		
		//-----------------------------------
		
		public bool Remove( string label )
		{
			if( m_Entities.ContainsKey( label ) == false )
			{
				return false ;
			}
		
			m_Entities.Remove( label ) ;
		
			return true ;
		}
	
		public string[] GetLabels()
		{
			if( m_Entities.Count == 0 )
			{
				return new string[ 0 ] ;
			}
		
			string[] labels = new string[ m_Entities.Count ] ;
		
			m_Entities.Keys.CopyTo( labels, 0 ) ;
		
			return labels ;
		}
		
		public bool Contains( string label )
		{
			string[] labels = GetLabels() ;
			if( labels == null || labels.Length == 0 )
			{
				return false ;
			}

			return labels.Contains( label ) ;
		}
		//-----------------------------------

		public string ToString( string indented = "", bool nullHandling = false )
		{
			string indent = "" ;

			return Serialize( indented, ref indent, nullHandling ) ;
		}

		public string Serialize( string indented, ref string indent, bool nullHandling )
		{
			string s = "" ;
		
			if( m_Entities.Count == 0 )
			{
				if( nullHandling == true )
				{
					if( string.IsNullOrEmpty( indented ) == false )
					{
						s += indent ;
					}
					s += "{}" ;
					return s ;
				}
				else
				{
					return "" ;
				}
			}
			
			//----------------------------------

			string[] labels = new string[ m_Entities.Count ] ;
		
			m_Entities.Keys.CopyTo( labels, 0 ) ;
		
			object entity ;

			string label ;
			string value ;

			List<string> texts = new List<string>() ;
			string t ;

			int i, l ;

			//----------------------------------

			if( string.IsNullOrEmpty( indented ) == false )
			{
				indent += indented ;
			}
			
			l = m_Entities.Count ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				label = labels[ i ] ;
				entity = m_Entities[ label ] ;
				
				t = "" ;

				if( string.IsNullOrEmpty( indented ) == false )
				{
					t += indent ;
				}
				// Key と value の間に改行を挿入するかどうか
				t += "\"" + label + "\":" ;

				if( entity == null )
				{
					value = "" ;

					t += value ;
				}
				else
				if( entity is JsonObject )
				{
					if( string.IsNullOrEmpty( indented ) == false )
					{
						t += "\n" ;
					}

					value = ( entity as JsonObject ).Serialize( indented, ref indent, nullHandling ) ;
					t += value ;
				}
				else
				if( entity is JsonArray )
				{
					if( string.IsNullOrEmpty( indented ) == false )
					{
						t += "\n" ;
					}

					value = ( entity as JsonArray ).Serialize( indented, ref indent, nullHandling ) ;
					t += value ;
				}
				else
				{
					value = ( string )entity ;

					if( nullHandling == false && value.Length == 2 && value[ 0 ] == '"' && value[ 1 ] == '"' )
					{
						// 文字且つ値が無い
						value = "" ;
					}

					t += value ;
				}
				
				if( string.IsNullOrEmpty( value ) == false )
				{
					texts.Add( t ) ;
				}
				else
				{
					if( nullHandling == true )
					{
						texts.Add( t ) ;
					}
				}
			}

			if( string.IsNullOrEmpty( indented ) == false )
			{
				indent = indent.Substring( 0, indent.Length - indented.Length ) ;
			}

			//----------------------------------

			if( texts.Count >  0 )
			{
				if( string.IsNullOrEmpty( indented ) == false )
				{
					s += indent ;
				}
				s += "{" ;
				if( string.IsNullOrEmpty( indented ) == false )
				{
					indent += indented ;
					s += "\n" ;
				}

				l = texts.Count ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					s += texts[ i ] ;
					if( i <  ( l - 1 ) )
					{
						s += "," ;
					}
					if( string.IsNullOrEmpty( indented ) == false )
					{
						s += "\n" ;
					}
				}

				if( string.IsNullOrEmpty( indented ) == false )
				{
					indent = indent.Substring( 0, indent.Length - indented.Length ) ;
					s += indent ;
				}
				s += "}" ;
			}
			else
			{
				if( nullHandling == true )
				{
					if( string.IsNullOrEmpty( indented ) == false )
					{
						s += indent ;
					}
					s += "{}" ;
					return s ;
				}
				else
				{
					return "" ;
				}
			}
			
			//----------------------------------
			
			return s ;
		}
	}
}
