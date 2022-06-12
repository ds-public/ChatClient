using System ;
using System.Collections.Generic ;
using UnityEngine ;

// Version 2020/03/31 0
namespace JsonHelper
{
	public class JsonArray
	{
		protected List<object> m_Entities = new List<object>() ;
		
		public object this[ int index ]
		{
			get
			{
				return Get( index ) ;
			}
			set
			{
				Set( index, value ) ;
			}
		}
		
		//-----------------------------------

		public JsonArray()
		{
		}
		
		public JsonArray( params object[] entities )
		{
			if( entities != null && entities.Length >  0 )
			{
				Put( entities ) ;
			}
		}

		public JsonArray( string source )
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
				m_Entities = new List<object>() ;
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

			if( source[ offset ] != '[' )
			{
				// 構文エラー
				return -1 ;
			}
		
			offset ++ ;
		
			//-----------------------------------------------------
			
			object value = null ;
			int w = 0 ;
			int f = -1 ;
			char c ;
			int i, o = -1, m ;
			bool escape = false ;
		
			for( i  = offset ; i <  l ; i ++ )
			{
				// 値
				if( w == 0 )
				{
					// Value 前
					i = Skip( source, i ) ;
					if( i >= l )
					{
						// 構文エラー([ が ] で閉じられていない)
						m_Entities.Clear() ;
						return -1 ;
					}
					
					c = source[ i ] ;

					// ラベルの外側
					if( c == ']' )
					{
						// いきなり終了
						f = i + 1 ;
						break ;
					}
					else
					if( c == ',' )
					{
						// 次の項目へ
						m_Entities.Add( "" ) ;
					}
					else
					if( c == '{' )
					{
						// 値は JsonObject
						JsonObject jo = new JsonObject() ;
					
						i = jo.Parse( source, i ) ;
					
						if( i <  0 || i >= l )
						{
							// 構文エラー([ が ] で閉じられていない)
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
							// 構文エラー([ が ] で閉じられていない)
							m_Entities.Clear() ;
							return -1 ;
						}
					
						i -- ;
					
						value = ja ;
					
						w = 3 ;	// 閉じへ
					}
					else
//					if( c == ' ' || c == '\r' || c == '\n' || c == '\t' )
//					{
//						// 読み飛ばし
//					}
//					else
					{
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

					c = source[ i ] ;

					if( c == ',' || c == ']' || c <  0x21 || c >  0x7E )
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
									s = "" ;
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
						// 構文エラー([ が ] で閉じられていない)
						m_Entities.Clear() ;
						return -1 ;
					}

					c = source[ i ] ;
					
					if( c == ',' || c == ']' )
					{
						// 次の項目へ
					
						// ここで tValue が null 以外であることはありえない					
						m_Entities.Add( value ) ;
					
						value = null ;
					
						if( c == ']' )
						{
							// 無事終了
							f = i + 1 ;
							break ;
						}
					
						w = 0 ;	// 選別へ
					}
					else
	//				if( c == ' ' || c == '\r' || c == '\n' || c == '\t' )
	//				{
	//					// 読み飛ばし
	//				}
	//				else
					{
						// 構文エラー(不正な文字)
						m_Entities.Clear() ;
						return -1 ;
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
		
		public bool Has( int index )
		{
			if( index <  0 || index >= m_Entities.Count )
			{
				return false ;
			}
		
			return true ;
		}
	
		public bool IsNull( int index )
		{
			if( index <  0 || index >= m_Entities.Count )
			{
	//			Debug.LogWarning( "JsonArray IsNull : Out of renge [ " + tIndex + " ] / " + list.Count ) ;
				return true ;
			}
		
			if( m_Entities[ index ] is string )
			{
				string s = m_Entities[ index ] as string ;
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
				if( m_Entities[ index ] == null )
				{
					return true ;
				}
			
				if( m_Entities[ index ] is JsonObject )
				{
					JsonObject jo = m_Entities[ index ] as JsonObject ;
					if( jo.Length == 0 )
					{
						return true ;
					}
				}
			
				if( m_Entities[ index ] is JsonArray )
				{
					JsonArray ja = m_Entities[ index ] as JsonArray ;
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
	
		public object Get( int index )
		{
			if( index <  0 || index >= m_Entities.Count )
			{
				Debug.LogWarning( "JsonArray Get : Out of renge [ " + index + " ] / " + m_Entities.Count ) ;
				return null ;
			}
		
			if( m_Entities[ index ] == null )
			{
				return null ;
			}
			
			object value = m_Entities[ index ] ;

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
						Debug.LogError( "JsonArray Get : [ " + index + " ] Parse Error : " + e.Message + " Value = " + s ) ;
					}
					
					return result ;
				}
			}
		
			return value ;
		}
	
		//-----------------------------------
		// 以下は Java と互換
		
		public bool GetBool( int index )
		{
			return GetBoolean( index ) ;
		}
		
		public bool GetBoolean( int index )
		{
			if( index <  0 || index >= m_Entities.Count )
			{
				Debug.LogWarning( "JsonArray GetBoolean : Out of renge [ " + index + " ] / " + m_Entities.Count ) ;
				return false ;
			}
		
			if( m_Entities[ index ] == null )
			{
				return false ;
			}
		
			if( m_Entities[ index ] is string )
			{
				string s = m_Entities[ index ] as string ;
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
				// JsonObject か JsonArray に対して行おうとした
				return false ;
			}
		}
		
		public int GetInt( int index )
		{
			if( index <  0 || index >= m_Entities.Count )
			{
				Debug.LogWarning( "JsonArray GetInt : Out of renge [ " + index + " ] / " + m_Entities.Count ) ;
				return 0 ;
			}
		
			if( m_Entities[ index ] == null )
			{
				return 0 ;
			}
		
			if( m_Entities[ index ] is string )
			{
				string s = m_Entities[ index ] as string ;
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
					Debug.LogError( "JsonArray GetInt : [ " + index + " ] Parse Error : " + e.Message + " Value = " + s ) ;
				}
			
				return result ;
			}
			else
			{
				// JsonObject か JsonArray に対して行おうとした
				return 0 ;
			}
		}
		
		public long GetLong( int index )
		{
			if( index <  0 || index >= m_Entities.Count )
			{
				Debug.LogWarning( "JsonArray GetLong : Out of renge [ " + index + " ] / " + m_Entities.Count ) ;
				return 0 ;
			}
		
			if( m_Entities[ index ] == null )
			{
				return 0 ;
			}
		
			if( m_Entities[ index ] is string )
			{
				string s = m_Entities[ index ] as string ;
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
					Debug.LogError( "JsonArray GetLong : [ " + index + " ] Parse Error : " + e.Message + " Value = " + s ) ;
				}
			
				return result ;
			}
			else
			{
				// JsonObject か JsonArray に対して行おうとした
				return 0 ;
			}
		}
		
		public float GetFloat( int index )
		{
			if( index <  0 || index >= m_Entities.Count )
			{
				Debug.LogWarning( "JsonArray GetFloat : Out of renge [ " + index + " ] / " + m_Entities.Count ) ;
				return 0 ;
			}
		
			if( m_Entities[ index ] == null )
			{
				return 0 ;
			}
		
			if( m_Entities[ index ] is string )
			{
				string s = m_Entities[ index ] as string ;
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
					Debug.LogError( "JsonArray GetFloat : [ " + index + " ] Parse Error : " + e.Message + " Value = " + s ) ;
				}
			
				return result ;
			}
			else
			{
				// JsonObject か JsonArray に対して行おうとした
				return 0 ;
			}
		}
		
		public double GetDouble( int index )
		{
			if( index <  0 || index >= m_Entities.Count )
			{
				Debug.LogWarning( "JsonArray GetDouble : Out of renge [ " + index + " ] / " + m_Entities.Count ) ;
				return 0 ;
			}
		
			if( m_Entities[ index ] == null )
			{
				return 0 ;
			}
		
			if( m_Entities[ index ] is string )
			{
				string s = m_Entities[ index ] as string ;
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
					Debug.LogError( "JsonArray GetDouble : [ " + index + " ] Parse Error : " + e.Message + " Value = " + s ) ;
				}
			
				return result ;
			}
			else
			{
				// JsonObject か JsonArray に対して行おうとした
				return 0 ;
			}
		}
		
		public string GetString( int index )
		{
			if( index <  0 || index >= m_Entities.Count )
			{
				Debug.LogWarning( "JsonArray GetString : Out of renge [ " + index + " ] / " + m_Entities.Count ) ;
				return "" ;
			}
			
			if( m_Entities[ index ] == null )
			{
				return "" ;
			}
		
			if( m_Entities[ index ] is string )
			{
				string s = m_Entities[ index ] as string ;
			
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
				// JsonObject か JsonArray に対して行おうとした
				return "" ;
			}
		}
		
		public JsonObject GetObject( int index )
		{
			if( index <  0 || index >= m_Entities.Count )
			{
				Debug.LogWarning( "JsonArray GetObject : Out of renge [ " + index + " ] / " + m_Entities.Count ) ;
				return null ;
			}
		
			if( m_Entities[ index ] == null )
			{
				return null ;
			}
		
			if( m_Entities[ index ] is JsonObject )
			{
				return m_Entities[ index ] as JsonObject ;
			}
		
			return null ;
		}
		
		public JsonArray GetArray( int index )
		{
			if( index <  0 || index >= m_Entities.Count )
			{
				Debug.LogWarning( "JsonArray GetArray : Out of renge [ " + index + " ] / " + m_Entities.Count ) ;
				return null ;
			}
		
			if( m_Entities[ index ] == null )
			{
				return null ;
			}
		
			if( m_Entities[ index ] is JsonArray )
			{
				return m_Entities[ index ] as JsonArray ;
			}
		
			return null ;
		}
		
		//-----------------------------------
		// Set

		private void Increase( int index )
		{
			if( m_Entities.Count <= index )
			{
				int i, l = 1 + index - m_Entities.Count ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					m_Entities.Add( null ) ;
				}
			}
		}

		public JsonArray Set( int index, object value )
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
			}
			
			Increase( index ) ;

			if( index >= 0 || index <  m_Entities.Count )
			{
				m_Entities[ index ] = value ;
			}
			return this ;
		}

		public JsonArray Set( int index, bool value )
		{
			return SetBoolean( index, value ) ;
		}
		
		public JsonArray Set( int index, int value )
		{
			return SetInt( index, value ) ;
		}
		
		public JsonArray Set( int index, long value )
		{
			return SetLong( index, value ) ;
		}
		
		public JsonArray Set( int index, float value )
		{
			return SetFloat( index, value ) ;
		}
		
		public JsonArray Set( int index, double value )
		{
			return SetDouble( index, value ) ;
		}
		
		public JsonArray Set( int index, string value )
		{
			return SetString( index, value ) ;
		}
		
		public JsonArray Set( int index, JsonObject value )
		{
			return SetObject( index, value ) ;
		}
		
		public JsonArray Set( int index, JsonArray value )
		{
			return SetArray( index, value ) ;
		}
		
		public JsonArray SetBool( int index, bool value )
		{
			return SetBoolean( index, value ) ;
		}

		public JsonArray SetBoolean( int index, bool value )
		{
			Increase( index ) ;

			if( index >= 0 || index <  m_Entities.Count )
			{
				m_Entities[ index ] = value.ToString().ToLower() ;
			}
			return this ;
		}
		
		public JsonArray SetInt( int index, int value )
		{
			Increase( index ) ;

			if( index >= 0 || index <  m_Entities.Count )
			{
				m_Entities[ index ] = value.ToString() ;
			}
			return this ;
		}
		
		public JsonArray SetLong( int index, long value )
		{
			Increase( index ) ;

			if( index >= 0 || index <  m_Entities.Count )
			{
				m_Entities[ index ] = value.ToString() ;
			}
			return this ;
		}
		
		public JsonArray SetFloat( int index, float value )
		{
			Increase( index ) ;

			if( index >= 0 || index <  m_Entities.Count )
			{
				m_Entities[ index ] = value.ToString() ;
			}
			return this ;
		}
		
		public JsonArray SetDouble( int index, double value )
		{
			Increase( index ) ;

			if( index >= 0 || index <  m_Entities.Count )
			{
				m_Entities[ index ] = value.ToString() ;
			}
			return this ;
		}
		
		public JsonArray SetString( int index, string value )
		{
			Increase( index ) ;

			if( index >= 0 || index <  m_Entities.Count )
			{
				m_Entities[ index ] = "\"" + value + "\"" ;
			}
			return this ;
		}
		
		public JsonArray SetObject( int index, JsonObject value )
		{
			Increase( index ) ;

			if( index >= 0 || index <  m_Entities.Count )
			{
				m_Entities[ index ] = value ;
			}
			return this ;
		}
		
		public JsonArray SetArray( int index, JsonArray value )
		{
			Increase( index ) ;

			if( index >= 0 || index <  m_Entities.Count )
			{
				m_Entities[ index ] = value ;
			}
			return this ;
		}

		//-----------------------------------
		// Put
		
		public JsonArray Put( object value )
		{
			if( value == null )
			{
				return this ;
			}

			if( value is bool )
			{
				PutBoolean( ( bool )value ) ;
			}
			else
			if( value is int )
			{
				PutInt( ( int )value ) ;
			}
			else
			if( value is long )
			{
				PutLong( ( long )value ) ;
			}
			else
			if( value is float )
			{
				PutFloat( ( float )value ) ;
			}
			else
			if( value is double )
			{
				PutDouble( ( double )value ) ;
			}
			else
			if( value is string )
			{
				PutString( ( string )value ) ;
			}
			else
			if( value is JsonObject )
			{
				PutObject( ( JsonObject )value ) ;
			}
			else
			if( value is JsonArray )
			{
				PutArray( ( JsonArray )value ) ;
			}
			else
			if( value is bool[] )
			{
				PutBoolean( ( bool[] )value ) ;
			}
			else
			if( value is int[] )
			{
				PutInt( ( int[] )value ) ;
			}
			else
			if( value is long[] )
			{
				PutLong( ( long[] )value ) ;
			}
			else
			if( value is float[] )
			{
				PutFloat( ( float[] )value ) ;
			}
			else
			if( value is double[] )
			{
				PutDouble( ( double[] )value ) ;
			}
			else
			if( value is string[] )
			{
				PutString( ( string[] )value ) ;
			}
			else
			if( value is JsonObject[] )
			{
				PutObject( ( JsonObject[] )value ) ;
			}
			else
			if( value is JsonArray[] )
			{
				PutArray( ( JsonArray[] )value ) ;
			}

			return this ;
		}
		
		public JsonArray Put( params bool[] value )
		{
			return PutBoolean( value ) ;
		}
	
		public JsonArray Put( params int[] value )
		{
			return PutInt( value ) ;
		}
	
		public JsonArray Put( params long[] value )
		{
			return PutLong( value ) ;
		}
	
		public JsonArray Put( params float[] value )
		{
			return PutFloat( value ) ;
		}
	
		public JsonArray Put( params double[] value )
		{
			return PutDouble( value ) ;
		}
	
		public JsonArray Put( params string[] value )
		{
			return PutString( value ) ;
		}
	
		public JsonArray Put( params JsonObject[] value )
		{
			return PutObject( value ) ;
		}
	
		public JsonArray Put( params JsonArray[] value )
		{
			return PutArray( value ) ;
		}

		public JsonArray PutBool( params bool[] value )
		{
			return PutBoolean( value ) ;
		}

		public JsonArray PutBoolean( params bool[] value )
		{
			if( value == null || value.Length == 0 )
			{
				return this ;
			}

			int i, l = value.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				m_Entities.Add( value[ i ].ToString().ToLower() ) ;
			}

			return this ;
		}
	
		public JsonArray PutInt( params int[] value )
		{
			if( value == null || value.Length == 0 )
			{
				return this ;
			}

			int i, l = value.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				m_Entities.Add( value[ i ].ToString() ) ;
			}

			return this ;
		}
	
		public JsonArray PutLong( params long[] value )
		{
			if( value == null || value.Length == 0 )
			{
				return this ;
			}

			int i, l = value.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				m_Entities.Add( value[ i ].ToString() ) ;
			}

			return this ;
		}
	
		public JsonArray PutFloat( params float[] value )
		{
			if( value == null || value.Length == 0 )
			{
				return this ;
			}

			int i, l = value.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				m_Entities.Add( value[ i ].ToString() ) ;
			}

			return this ;
		}
	
		public JsonArray PutDouble( params double[] value )
		{
			if( value == null || value.Length == 0 )
			{
				return this ;
			}

			int i, l = value.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				m_Entities.Add( value[ i ].ToString() ) ;
			}

			return this ;
		}
	
		public JsonArray PutString( params string[] value )
		{
			if( value == null || value.Length == 0 )
			{
				return this ;
			}

			int i, l = value.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				m_Entities.Add( "\"" + value[ i ] + "\"" ) ;
			}

			return this ;
		}
	
		public JsonArray PutObject( params JsonObject[] value )
		{
			if( value == null || value.Length == 0 )
			{
				return this ;
			}

			int i, l = value.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				m_Entities.Add( value[ i ] ) ;
			}

			return this ;
		}
	
		public JsonArray PutArray( params JsonArray[] value )
		{
			if( value == null || value.Length == 0 )
			{
				return this ;
			}

			int i, l = value.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				m_Entities.Add( value[ i ] ) ;
			}

			return this ;
		}

		//---------------
		
		public JsonArray Put( int index, params bool[] value )
		{
			return PutBoolean( index, value ) ;
		}
	
		public JsonArray Put( int index, params int[] value )
		{
			return PutInt( index, value ) ;
		}
	
		public JsonArray Put( int index, params long[] value )
		{
			return PutLong( index, value ) ;
		}
	
		public JsonArray Put( int index, params float[] value )
		{
			return PutFloat( index, value ) ;
		}
	
		public JsonArray Put( int index, params double[] value )
		{
			return PutDouble( index, value ) ;
		}
	
		public JsonArray Put( int index, params string[] value )
		{
			return PutString( index, value ) ;
		}
	
		public JsonArray Put( int index, params JsonObject[] value )
		{
			return PutObject( index, value ) ;
		}
	
		public JsonArray Put( int index, params JsonArray[] value )
		{
			return PutArray( index, value ) ;
		}
		
		public JsonArray PutBool( int index, params bool[] value )
		{
			return PutBoolean( index, value ) ;
		}

		public JsonArray PutBoolean( int index, params bool[] value )
		{
			if( value == null || value.Length == 0 )
			{
				return this ;
			}

			if( index <  0 )
			{
				index  = 0 ;
			}

			if( index <  m_Entities.Count )
			{
				// 途中に挿入
				int i, l = value.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					m_Entities.Insert( index + i, value[ i ].ToString().ToLower() ) ;
				}
			}
			else
			{
				// 最後に追加
				int i, l = value.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					m_Entities.Add( value[ i ].ToString().ToLower() ) ;
				}
			}

			return this ;
		}
		
		public JsonArray PutInt( int index, params int[] value )
		{
			if( value == null || value.Length == 0 )
			{
				return this ;
			}

			if( index <  0 )
			{
				index  = 0 ;
			}

			if( index <  m_Entities.Count )
			{
				int i, l = value.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					m_Entities.Insert( index + i, value[ i ].ToString() ) ;
				}
			}
			else
			{
				int i, l = value.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					m_Entities.Add( value[ i ].ToString() ) ;
				}
			}

			return this ;
		}
		
		public JsonArray PutLong( int index, params long[] value )
		{
			if( value == null || value.Length == 0 )
			{
				return this ;
			}

			if( index <  0 )
			{
				index  = 0 ;
			}

			if( index <  m_Entities.Count )
			{
				int i, l = value.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					m_Entities.Insert( index + i, value[ i ].ToString() ) ;
				}
			}
			else
			{
				int i, l = value.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					m_Entities.Add( value[ i ].ToString() ) ;
				}
			}

			return this ;
		}
		
		public JsonArray PutFloat( int index, params float[] value )
		{
			if( value == null || value.Length == 0 )
			{
				return this ;
			}

			if( index <  0 )
			{
				index  = 0 ;
			}

			if( index <  m_Entities.Count )
			{
				int i, l = value.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					m_Entities.Insert( index + i, value[ i ].ToString() ) ;
				}
			}
			else
			{
				int i, l = value.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					m_Entities.Add( value[ i ].ToString() ) ;
				}
			}

			return this ;
		}
		
		public JsonArray PutDouble( int index, params double[] value )
		{
			if( value == null || value.Length == 0 )
			{
				return this ;
			}

			if( index <  0 )
			{
				index  = 0 ;
			}

			if( index <  m_Entities.Count )
			{
				int i, l = value.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					m_Entities.Insert( index + i, value[ i ].ToString() ) ;
				}
			}
			else
			{
				int i, l = value.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					m_Entities.Add( value[ i ].ToString() ) ;
				}
			}

			return this ;
		}
		
		public JsonArray PutString( int index, params string[] value )
		{
			if( value == null || value.Length == 0 )
			{
				return this ;
			}

			if( index <  0 )
			{
				index  = 0 ;
			}

			if( index <  m_Entities.Count )
			{
				int i, l = value.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					m_Entities.Insert( index + i, "\"" + value[ i ] + "\"" ) ;
				}
			}
			else
			{
				int i, l = value.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					m_Entities.Add( "\"" + value[ i ] + "\"" ) ;
				}
			}

			return this ;
		}
	
		public JsonArray PutObject( int index, params JsonObject[] value )
		{
			if( value == null || value.Length == 0 )
			{
				return this ;
			}

			if( index <  0 )
			{
				index  = 0 ;
			}

			if( index <  m_Entities.Count )
			{
				int i, l = value.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					m_Entities.Insert( index + i, value[ i ] ) ;
				}
			}
			else
			{
				int i, l = value.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					m_Entities.Add( value[ i ] ) ;
				}
			}

			return this ;
		}
	
		public JsonArray PutArray( int index, params JsonArray[] value )
		{
			if( value == null || value.Length == 0 )
			{
				return this ;
			}

			if( index <  0 )
			{
				index  = 0 ;
			}

			if( index <  m_Entities.Count )
			{
				int i, l = value.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					m_Entities.Insert( index + i, value[ i ] ) ;
				}
			}
			else
			{
				int i, l = value.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					m_Entities.Add( value[ i ] ) ;
				}
			}

			return this ;
		}
		
		//-----------------------------------
		
		public bool Remove( int index )
		{
			if( index <  0 || index >= m_Entities.Count )
			{
				return false ;
			}
		
			m_Entities.Remove( index ) ;
		
			return true ;
		}
		
		//-----------------------------------------------------------
		
		public string ToString( string indented = "", bool nullHandling = false )
		{
			string indent = "" ;
			return Serialize( indented, ref indent, nullHandling ) ;
		}

		public string Serialize( string indented, ref string indent, bool nullHandling = false )
		{
			string s = "" ;

			if( m_Entities.Count == 0 )
			{
				// 要素数が 0 の場合は出力が省略される可能性がある
				if( nullHandling == true )
				{
					if( string.IsNullOrEmpty( indented ) == false )
					{
						s += indent ;
					}
					s += "[]" ;
					return s ;
				}
				else
				{
					// 出力を省略する
					return "" ;
				}
			}
		
			//----------------------------------
		
			object entity ;

			string value ;

			int i, l ;

			//----------------------------------

			// 要素内に null があっても 1 つ以上 null でないものがあれば全て配列として出力する
			
			if( string.IsNullOrEmpty( indented ) == false )
			{
				s += indent ;
			}
			s += "[" ;
			if( string.IsNullOrEmpty( indented ) == false )
			{
				indent += indented ;
				s += "\n" ;
			}

			l = m_Entities.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				entity = m_Entities[ i ] ;

				if( entity == null )
				{
					if( string.IsNullOrEmpty( indented ) == false )
					{
						s += indent ;
					}

					value = "" ;

					s += value ;
				}
				else
				if( entity is JsonObject )
				{
					value = ( entity as JsonObject ).Serialize( indented, ref indent, nullHandling ) ;
					s += value ;
				}
				else
				if( entity is JsonArray )
				{
					value = ( entity as JsonArray ).Serialize( indented, ref indent, nullHandling ) ;
					s += value ;
				}
				else
				{
					if( string.IsNullOrEmpty( indented ) == false )
					{
						s += indent ;
					}

					value = ( string )entity ;

					if( nullHandling == false && value.Length == 2 && value[ 0 ] == '"' && value[ 1 ] == '"' )
					{
						// 文字且つ値が無い
						value = "" ;
					}
					
					s += value ;
				}

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
			s += "]" ;
			
			//----------------------------------
		
			return s ;
		}
	}
}
