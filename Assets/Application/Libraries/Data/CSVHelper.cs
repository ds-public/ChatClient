using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Reflection ;
using System.Text ;

using UnityEngine ;

/// <summary>
/// ＣＳＶヘルパーパッケージ
/// </summary>
namespace CSVHelper
{
	/// <summary>
	/// ＣＳＶデータ管理クラス	Version 2021/06/15 0
	/// </summary>
	
	[Serializable]
	public class CSVObject
	{
		[SerializeField]
		private CSVLineObject[] m_Values ;
	
		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="count">行数</param>
		public CSVObject( int count = 0 )
		{
			if( count >= 0 )
			{
				m_Values = new CSVLineObject[ count ] ;
			}
		}
		
		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="count0">行数</param>
		/// <param name="count1">列数</param>
		public CSVObject( int count0, int count1 )
		{
			if( count0 >= 0 )
			{
				m_Values = new CSVLineObject[ count0 ] ;

				int i ;

				for( i  = 0 ; i <  count0 ; i ++ )
				{
					m_Values[ i ] = new CSVLineObject( count1 ) ;
				}
			}
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="text">ＣＳＶテキスト(区切りはカンマまたはタブ)</param>
		/// <param name="reduce">行の最後の空白の要素を削除するかどうか(true=削除する・false=削除しない)</param>
		public CSVObject( string text, bool reduce = true, int startColumn = 0, int startRow = 0, bool disableComma = false )
		{
			SetData( text, reduce, startColumn, startRow, disableComma ) ;
		}
		
		/// <summary>
		/// １行分のデータを追加する
		/// </summary>
		/// <param name="data"></param>
		public void AddLine( params System.Object[] data )
		{
			if( data == null || data.Length == 0 )
			{
				return ;
			}

			//----------------------------------

			List<CSVLineObject> list = new List<CSVLineObject>() ;

			if( m_Values != null && m_Values.Length >  0 )
			{
				list.AddRange( m_Values ) ;
			}

			CSVLineObject line = new CSVLineObject( data ) ;

			list.Add( line ) ;

			m_Values = list.ToArray() ;
		}

		/// <summary>
		/// インデクサ(各行に直接アクセス可能)
		/// </summary>
		/// <param name="line">行番号(0～)</param>
		/// <returns>ＣＳＶの行単位の管理オブジェクトのインスタンス</returns>
		public CSVLineObject this[ int line ]
		{
			get
			{
				if( m_Values == null || line <  0 || line >= m_Values.Length )
				{
					return new CSVLineObject() ;	// 空のオブジェクトを返す
				}

				return m_Values[ line ] ;
			}
		}

		/// <summary>
		/// ＣＳＶオブジェクトのＣＳＶテキスト化を行う
		/// </summary>
		/// <param name="splitCode"></param>
		/// <returns></returns>
		public string ToString( string splitCode = "\t" )
		{
			if( m_Values == null )
			{
				return "" ;
			}

			string text = "" ;

			int i, l = m_Values.Length ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				text += m_Values[ i ].ToString( splitCode ) ;

				if( i <  ( l - 1 ) )
				{
					text += "\n" ;
				}
			}

			return text ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ＣＳＶテキストの各要素ををパースして格納する
		/// </summary>
		/// <param name="text">ＣＳＶテキスト(区切りはカンマまたはタブ)</param>
		/// <param name="reduce">行の最後の空白の要素を削除するかどうか(true=削除する・false=削除しない)</param>
		/// <returns>パース結果(true=成功・false=失敗)</returns>
		public bool SetData( string text, bool reduce = true, int startColumn = 0, int startRow = 0, bool disableComma = false )
		{
			if( string.IsNullOrEmpty( text ) == true )
			{
				return false ;
			}
		
			// まずコメント部分を削る
		
			int o, p, L, i, k, r ;
			char c ;
		
			//-------------------------------------------------------------
		
			// ダブルクォート内にある改行・タブを置き換える
		
			L = text.Length ;
		
			char[] ca = new char[ L ] ;
		
			// 改行を \n に統一する（" 内外関係無し）
			p = 0 ;
			for( i  = 0 ; i <  L ; i ++ )
			{
				c = text[ i ] ;
			
				if( c == 13 )
				{
					// CR
					if( ( i + 1 ) <  L )
					{
						if( text[ i + 1 ] == 10 )
						{
							// 改行
							ca[ p ] = '\n' ;
							p ++ ;
						
							i ++ ;	// １文字スキップ
						}
					}
				}
				else
				{
					ca[ p ] = c ;
					p ++ ;
				}
			}
		
			text = new string( ca, 0, p ) ;
		
			L = text.Length ;
		
			// この段階で CR は存在しない
		
			// ダブルクォートの外のカンマまたはタブの数をカウントする
			int lc = 0 ;
		
			int f = 0 ;	// ダブルクォートの内側か外側か（デフォルトは外側）
			bool d = false ;
			bool escape = false ;
		
			r = 0 ;
			for( i  = 0 ; i <  L ; i ++ )
			{
				c = text[ i ] ;
			
				if( r == 0 )
				{
					// 行開始
					r = 1 ;
					f = 0 ;
				}
			
				if( f == 0 )
				{
					// 外側
					if( c == ' ' )
					{
						// スペースはスルー
					}
					else
					if( c == '\n' )
					{
						lc ++ ;	// 行増加
						r = 0 ;
					}
					else
						if( c == '"' )
					{
						// ダブルクォートを発見した
						f = 1 ;	// 状態を反転させる
						d = true ;	// ダブルクォート系
						escape = false ;
					}
					else
					{
						// そのまま文字列
						f = 1 ;
						d = false ;	// 非ダブルクォート系
						escape = false ;
					}
				}
				else
				if( f == 1 )
				{
					// 内側
					if( d == true )
					{
						// ダブルクォートの文字列
						if( c == '\\' )
						{
							escape = true ;
						}
						else
						{
							if( escape == false )
							{
								if( c == '"' )
								{
									// 終了
									f = 2 ;
								}
							}
							else
							{
								escape = false ;
							}
						}
					}
					else
					{
						// 非ダブルクォート系の文字
						if( c == '\\' )
						{
							escape = true ;
						}
						else
						{
							if( escape == false )
							{
								if( c == '\t' || ( c == ',' && disableComma == false ) || c == '\n' )
								{
									// 終了
									if( c == '\t' || ( c == ',' && disableComma == false ) )
									{
										f = 0 ;
									}
									else
									if( c == '\n' )
									{
										f = 0 ;
									
										lc ++ ;
										r = 0 ;
									}
								}
							}
							else
							{
								escape = false ;
							}
						}
					}
				}
				else
				if( f == 2 )
				{
					if( c == '\t' || ( c == ',' && disableComma == false ) || c == '\n' )
					{
						// 終わり
						f = 0 ;
					
						if( c == '\n' )
						{
							lc ++ ;
							r = 0 ;
						}
					}
				}
			}
		
			if( r == 1 )
			{
				// 中途半端に終わった
				lc ++ ;
			}
		
			//--------------------------
		
			if( ( lc - startRow ) <= 0 )
			{
				// 要素が存在しない
				return false ;
			}
		
			// 配列を確保する
			m_Values = new CSVLineObject[ lc ] ;
			for( i  = 0 ; i <  lc ; i ++ )
			{
				m_Values[ i ] = new CSVLineObject() ;
			}

			//-----------------------------------------------------
		
			// 実際に値を取得する
			List<string> wordList = new List<string>() ;
		
			lc = 0 ;
		
			c = ' ' ;
			o = -1 ;
		
			r = 0 ;
			for( i  = 0 ; i <  L ; i ++ )
			{
				c = text[ i ] ;
			
				if( r == 0 )
				{
					// 行開始
					wordList.Clear() ;
					r = 1 ;
					f = 0 ;
				}
			
				if( f == 0 )
				{
					// 外側
					if( c == ' ' )
					{
						// スペースはスルー
					}
					else
					if( c == '\n' )
					{
						m_Values[ lc ].SetValueArray( wordList.ToArray() ) ;
					
						lc ++ ;	// 行増加
						r = 0 ;
					}	
					else
					if( c == '\t' || ( c == ',' && disableComma == false ) )
					{
						// 区切り
						wordList.Add( "" ) ;
					}
					else
					if( c == '"' )
					{
						// ダブルクォートを発見した
						o = i + 1 ;
						f = 1 ;		// 文字の内側状態に
						d = true ;	// ダブルクォート系
						escape = false ;
					}
					else
					{
						// そのまま文字列
						o = i ;
						f = 1 ;		// 文字の内側状態に
						d = false ;	// 非ダブルクォート系
						escape = false ;
					}
				}
				else
				if( f == 1 )
				{
					// 内側
					if( d == true )
					{
						// ダブルクォートの文字列
						if( c == '\\' )
						{
							escape = true ;
						}
						else
						{
							if( escape == false )
							{
								if( c == '"' )
								{
									// 終了
									if( i >  o )
									{
										wordList.Add( text.Substring( o, i - o ) ) ;
									}
									else
									{
										wordList.Add( "" ) ;
									}
									f =  2 ;
								}
							}
							else
							{
								escape = false ;
							}
						}
					}
					else
					{
						// 非ダブルクォート系の文字
						if( c == '\\' )
						{
							escape = true ;
						}
						else
						{
							if( escape == false )
							{
								if( c == '\t' || ( c == ',' && disableComma == false ) || c == '\n' )
								{
									// 終了
									if( i >  o )
									{
										// 末尾のスペースは削除する
										for( k  = i - 1 ; k >= o ; k -- )
										{
											if( text[ k ] != ' ' )
											{
												break ;
											}
										}
										if( k >= o )
										{
											wordList.Add( text.Substring( o, k - o + 1 ) ) ;
										}
										else
										{
											wordList.Add( "" ) ;
										}
									}
									else
									{
										if( c != '\n' )
										{
											wordList.Add( "" ) ;
										}
									}
								
									if( c == '\t' || ( c == ',' && disableComma == false ) )
									{
										f = 0 ;
									}
									else
									if( c == '\n' )
									{
										f = 0 ;
									
										m_Values[ lc ].SetValueArray( wordList.ToArray() ) ;
									
										lc ++ ;
										r = 0 ;
									}
								}
							}
							else
							{
								escape = false ;
							}
						}
					}
				}
				else
				if( f == 2 )
				{
					if( c == '\t' || ( c == ',' && disableComma == false ) || c == '\n' )
					{
						// 終わり
						f = 0 ;
					
						if( c == '\n' )
						{
							m_Values[ lc ].SetValueArray( wordList.ToArray() ) ;
							o = -1 ;
						
							lc ++ ;
							r = 0 ;
						}
					}
				}
			}
		
		
			if( r == 1 )
			{
				if( f == 1 && d == false )
				{
					// 中途半端に終わった
					if( i >  o )
					{
						// 末尾のスペースは削除する
						for( k  = i - 1 ; k >= o ; k -- )
						{
							if( text[ k ] != ' ' )
							{
								break ;
							}
						}
						if( k >= o )
						{
							wordList.Add( text.Substring( o, k - o + 1 ) ) ;
						}
						else
						{
							wordList.Add( "" ) ;
						}
					}
					else
					{
						if( c != '\n' )
						{
							wordList.Add( "" ) ;
						}
					}
				}
		
				m_Values[ lc ].SetValueArray( wordList.ToArray() ) ;
			
				lc ++ ;
			}
		
			//-------------------------------------------------------------

			if( startRow >  0 )
			{
				// 上の部分を切り捨てる
				CSVLineObject[] values = new CSVLineObject[ lc - startRow ] ;
				
				for( i  = 0 ; i <  ( lc - startRow ) ; i ++ )
				{
					values[ i ] = m_Values[ startRow + i ] ;
				}
				
				m_Values = values ;
				lc -= startRow ;
			}

			if( startColumn >  0 )
			{
				// 左の部分を切り捨てる
				for( i  = 0 ; i <  lc ; i ++ )
				{
					m_Values[ i ].Trim( startColumn ) ;
				}
			}
			
			if( reduce == true )
			{
				// 空行を切り捨てる

				p = 0 ;
				for( i  = 0 ; i <  lc ; i ++ )
				{
					if( m_Values[ i ].Length >   0 )
					{
						p ++ ;
					}
				}

				if( p == 0 )
				{
					m_Values = null ;
					return true ;	// 基本ありえない
				}

				if( p <  lc )
				{
					// 空行が１行以上存在する
					CSVLineObject[] values = new CSVLineObject[ p ] ;
					p = 0 ;
					for( i  = 0 ; i <  lc ; i ++ )
					{
						if( m_Values[ i ].Length >  0 )
						{
							values[ p ] = m_Values[ i ] ;
							p ++ ;
						}
					}

					m_Values = values ;
				}
			}
		
			//-------------------------------------------------------------
		
			return true ;
		}

		/// <summary>
		/// 行数を取得する
		/// </summary>
		/// <returns>行数</returns>
		public int GetLength()
		{
			if( m_Values == null )
			{
				return 0 ;
			}
		
			return m_Values.Length ;
		}
	
		/// <summary>
		/// 行数
		/// </summary>
		public int Length
		{
			get
			{
				return GetLength() ;
			}
		}

		/// <summary>
		/// 最大の列数を取得する
		/// </summary>
		/// <returns></returns>
		public int GetMaximumLineLength()
		{
			if( m_Values == null )
			{
				return 0 ;
			}

			int i, l = m_Values.Length ;
			int k ;
			int m = 0 ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_Values[ i ] != null )
				{
					k = m_Values[ i ].GetLength() ;
					if( k >  m )
					{
						m  = k ;
					}
				}
			}

			return m ;
		}

		/// <summary>
		/// 最大の列数
		/// </summary>
		public int MaximumLineLength
		{
			get
			{
				return GetMaximumLineLength() ;
			}
		}
		

		/// <summary>
		/// 行列の配列の形で取得する
		/// </summary>
		/// <returns>行列の配列</returns>
		public string[,] GetMatrix()
		{
			if( m_Values == null )
			{
				return null ;
			}

			int l = m_Values.Length ;
			int m = GetMaximumLineLength() ;

			string[,] matrix = new string[ l, m ] ;
			if( l  == 0 || m == 0 )
			{
				return matrix ;
			}

			int x, y ;
			for( y  = 0 ; y <  l ; y ++ )
			{
				for( x  = 0 ; x <  m ; x ++ )
				{
					matrix[ y, x ] = m_Values[ y ][ x ] ;
				}
			}

			return matrix ;
		}

		// 行列の配列
		public string[,] Matrix
		{
			get
			{
				return GetMatrix() ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ＣＳＶの行単位の管理クラス
		/// </summary>
		[Serializable]
		public class CSVLineObject
		{
			[SerializeField]
			private string[] m_Values = null ;

			public CSVLineObject(){}

			public CSVLineObject( params System.Object[] data )
			{
				Set( data ) ;
			}

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="count">列数</param>
			public CSVLineObject( int count = 0 )
			{
				if( count >= 0 )
				{
					m_Values = new string[ count ] ;

					int i ;

					for( i  = 0 ; i <  count ; i ++ )
					{
						m_Values[ i ] = "" ;
					}
				}
			}

			/// <summary>
			/// データを設定する
			/// </summary>
			/// <param name="data"></param>
			public void Set( params System.Object[] data )
			{
				if( data == null || data.Length == 0 )
				{
					m_Values = null ;
					return ;
				}

				int i, l = data.Length ;

				m_Values = new string[ l ] ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					m_Values[ i ] = data[ i ].ToString() ;
				}
			}

			/// <summary>
			/// データを追加する
			/// </summary>
			/// <param name="data"></param>
			public void Add( params System.Object[] data )
			{
				if( data == null || data.Length == 0 )
				{
					return ;
				}

				//---------------------------------

				List<string> list = new List<string>() ;

				if( m_Values != null && m_Values.Length >  0 )
				{
					list.AddRange( m_Values ) ;
				}

				int i, l = data.Length ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					list.Add( data[ i ].ToString() ) ;
				}

				m_Values = list.ToArray() ;
			}

			/// <summary>
			/// 左側のカラムを指定数削る
			/// </summary>
			/// <param name="count"></param>
			public void Trim( int count )
			{
				if( m_Values != null && m_Values.Length >  0 )
				{
					if( m_Values.Length >  count )
					{
						List<string> list = new List<string>() ;
						list.AddRange( m_Values ) ;
						list.RemoveRange( 0, count ) ;
						m_Values = list.ToArray() ;
					}
					else
					{
						m_Values = new string[ 0 ] ;
					}
				}
			}

			/// <summary>
			/// インデクサ(各列に直接アクセス可能)
			/// </summary>
			/// <param name="index">列番号(0～)</param>
			/// <returns>指定した列に格納された値(文字列で取得)</returns>
			public string this[ int index ]
			{
				get
				{
					if( m_Values == null || index <  0 || index >= m_Values.Length )
					{
						return "" ;
					}

					return m_Values[ index ] ;
				}
				set
				{
					if( m_Values == null || index <  0 || index >= m_Values.Length )
					{
						return ;
					}

					m_Values[ index ] = value ;
				}
			}

			/// <summary>
			/// 行をテキスト化する
			/// </summary>
			/// <param name="splitCode">区切り記号の文字列</param>
			/// <returns>行の文字列</returns>
			public string ToString( string splitCode = "\t" )
			{
				if( m_Values == null )
				{
					return "" ;
				}

				string text = "" ;

				int i, l = m_Values.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( string.IsNullOrEmpty( m_Values[ i ] ) == false )
					{
						text += m_Values[ i ]  ;
					}

					if( i <  ( l - 1 ) )
					{
						text += splitCode ;
					}
				}

				return text ;
			}


			/// <summary>
			/// 列の全要素を設定する
			/// </summary>
			/// <param name="array">列の全要素が格納された文字列型の配列</param>
			public void SetValueArray( string[] array )
			{
				m_Values = array ;
			}

			/// <summary>
			/// 列の要素を設定する
			/// </summary>
			/// <param name="index">列番号(0～)</param>
			/// <param name="value">列の値</param>
			public void SetValue( int index, string value )
			{
				if( m_Values == null || index <  0 || index >= m_Values.Length )
				{
					return ;
				}

				m_Values[ index ] = value ;
			}

			// 0xb や 0x から値を取得する
			private ulong GetSpecialValue( ref string value, out bool result )
			{
				result = false ;
				if( string.IsNullOrEmpty( value ) == true )
				{
					return 0 ;
				}

				value = value.ToLower() ;

				int l = value.Length - 1 ;

				if( value.Length >  2 && value[ 0 ] == '|' && value.LastIndexOf( '|' ) >  0 )
				{
					// ラベル付き数値
					value = value.TrimStart( '|' ) ;
					int i = value.IndexOf( '|' ) ;
					value = value.Substring( 0, i ) ;

					l = value.Length - 1 ;
				}

				if( value.Length >  1 && value[ 0 ] == 'b' )
				{
					return GetSpecialValueFromB( value.Substring( 1, l ), out result ) ;
				}
				else
				if( value.Length >  2 && value.IndexOf( "0b" ) == 0 )
				{
					return GetSpecialValueFromB( value.Substring( 2, l - 1 ), out result ) ;
				}
				else
				if( value.Length >  1 && value[ l ] == 'b' )
				{
					return GetSpecialValueFromB( value.Substring( 0, l ), out result ) ;
				}
				else
				if( value.Length >  1 && value[ 0 ] == 'o' )
				{
					return GetSpecialValueFromO( value.Substring( 1, l ), out result ) ;
				}
				else
				if( value.Length >  2 && value.IndexOf( "0o" ) == 0 )
				{
					return GetSpecialValueFromO( value.Substring( 2, l - 1 ), out result ) ;
				}
				else
				if( value.Length >  1 && value[ l ] == 'o' )
				{
					return GetSpecialValueFromO( value.Substring( 0, l ), out result ) ;
				}
				else
				if( value.Length >  1 && value[ 0 ] == 'x' )
				{
					return GetSpecialValueFromH( value.Substring( 1, l ), out result ) ;
				}
				else
				if( value.Length >  2 && value.IndexOf( "0x" ) == 0 )
				{
					return GetSpecialValueFromH( value.Substring( 2, l - 1 ), out result ) ;
				}
				else
				if( value.Length >  1 && value[ l ] == 'x' )
				{
					return GetSpecialValueFromH( value.Substring( 0, l ), out result ) ;
				}

				// 不明
				return 0 ;
			}

			// ２進数
			private ulong GetSpecialValueFromB( string value, out bool result )
			{
				result = false ;
				if( string.IsNullOrEmpty( value ) == true )
				{
					return 0 ;
				}

				ulong v =  0 ;
				ulong a =  1 ;
				ulong b =  2 ;
				char c ;

				int i, l = value.Length ;

				if( l >  64 )
				{
					l  = 64 ;
				}

				for( i  = 0 ; i <  l ; i ++ )
				{
					c = value[ l - 1 - i ] ;
					if( c >= '0' && c <= '1' )
					{
						v += ( ulong )( c - '0' ) * a ;
					}
					else
					{
						return 0 ;	// エラー
					}

					a *= b ;
				}

				result = true ;
				return v ;
			}

			// ８進数
			private ulong GetSpecialValueFromO( string value, out bool result )
			{
				result = false ;
				if( string.IsNullOrEmpty( value ) == true )
				{
					return 0 ;
				}

				ulong v =  0 ;
				ulong a =  1 ;
				ulong b =  8 ;
				char c ;

				int i, l = value.Length ;

				if( l >  21 )
				{
					l  = 21 ;
				}

				for( i  = 0 ; i <  l ; i ++ )
				{
					c = value[ l - 1 - i ] ;
					if( c >= '0' && c <= '7' )
					{
						v += ( ulong )( c - '0' ) * a ;
					}
					else
					{
						return 0 ;	// エラー
					}

					a *= b ;
				}

				result = true ;
				return v ;
			}

			// 16進数
			private ulong GetSpecialValueFromH( string value, out bool result )
			{
				result = false ;
				if( string.IsNullOrEmpty( value ) == true )
				{
					return 0 ;
				}

				ulong v =  0 ;
				ulong a =  1 ;
				ulong b = 16 ;
				char c ;

				int i, l = value.Length ;

				if( l >  16 )
				{
					l  = 16 ;
				}

				for( i  = 0 ; i <  l ; i ++ )
				{
					c = value[ l - 1 - i ] ;
					if( c >= '0' && c <= '9' )
					{
						v += ( ulong )( c - '0' ) * a ;
					}
					else
					if( c >= 'a' && c <= 'f' )
					{
						v += ( ulong )( c - 'a' + 10 ) * a ;
					}
					else
					{
						return 0 ;	// エラー
					}

					a *= b ;
				}

				result = true ;
				return v ;
			}

			/// <summary>
			/// 列の要素を取得する
			/// </summary>
			/// <param name="index">列番号(0～)</param>
			/// <returns>列の要素(文字列型)</returns>
			public string GetValue( int index )
			{
				if( m_Values == null || index <  0 || index >= m_Values.Length )
				{
					return "" ;
				}

				return m_Values[ index ] ;
			}

			/// <summary>
			/// 列の要素をブール型の値として取得する
			/// </summary>
			/// <param name="index">列番号(0～)</param>
			/// <returns>列の要素(ブール型)</returns>
			public bool GetBoolean( int index ) 
			{
				if( m_Values == null )
				{
					return false ;
				}
				
				if( index <  0 || index >= m_Values.Length )
				{
					return false ;
				}
		
				string value = m_Values[ index ] ;
				if( string.IsNullOrEmpty( value ) == true )
				{
					return false ;
				}
				
				ulong v = GetSpecialValue( ref value, out bool r ) ;
				if( r == true )
				{
					return ( v != 0 ) ;
				}

				value = value.ToLower() ;
		
				if( value.Equals( "true" ) == true || value.Equals( "1" ) == true )
				{
					return true ;
				}
		
				return false ;
			}
			
			/// <summary>
			/// 列の要素を符号なし8bit整数型の値として取得する
			/// </summary>
			/// <param name="index">列番号(0～)</param>
			/// <returns>列の要素(符号あり32bit整数型)</returns>
			public byte GetByte( int index )
			{
				if( m_Values == null )
				{
					return 0 ;
				}
		
				if( index <  0 || index >= m_Values.Length )
				{
					return 0 ;
				}
		
				string value = m_Values[ index ] ; 
				if( string.IsNullOrEmpty( value ) == true )
				{
					return 0 ;
				}
		
				ulong v = GetSpecialValue( ref value, out bool r ) ;
				if( r == true )
				{
					return ( byte )( v & 0xFF ) ;
				}

				if( value[ value.Length - 1 ] == 'f' || value[ value.Length - 1 ] == 'F' )
				{
					value = value.Substring( 0, value.Length - 1 ) ;
				}
		
				if( double.TryParse( value, out double byteValue ) == false )
				{
					Debug.LogWarning( "Byte Parse Error : " + value ) ;
					return 0 ;
				}
		
				return ( byte )byteValue ;
			}

			/// <summary>
			/// 列の要素を符号なし16bit整数型の値として取得する
			/// </summary>
			/// <param name="index">列番号(0～)</param>
			/// <returns>列の要素(符号あり32bit整数型)</returns>
			public char GetChar( int index )
			{
				if( m_Values == null )
				{
					return ( char )0 ;
				}
		
				if( index <  0 || index >= m_Values.Length )
				{
					return ( char )0 ;
				}
		
				string value = m_Values[ index ] ; 
				if( string.IsNullOrEmpty( value ) == true )
				{
					return ( char )0 ;
				}
		
				ulong v = GetSpecialValue( ref value, out bool r ) ;
				if( r == true )
				{
					return ( char )( v & 0xFFFF ) ;
				}

				if( value[ value.Length - 1 ] == 'f' || value[ value.Length - 1 ] == 'F' )
				{
					value = value.Substring( 0, value.Length - 1 ) ;
				}
		
				if( double.TryParse( value, out double charValue ) == false )
				{
					Debug.LogWarning( "Char Parse Error : " + value ) ;
					return ( char )0 ;
				}
		
				return ( char )charValue ;
			}

			/// <summary>
			/// 列の要素を符号あり16bit整数型の値として取得する
			/// </summary>
			/// <param name="index">列番号(0～)</param>
			/// <returns>列の要素(符号あり32bit整数型)</returns>
			public short GetShort( int index )
			{
				if( m_Values == null )
				{
					return 0 ;
				}
		
				if( index <  0 || index >= m_Values.Length )
				{
					return 0 ;
				}
		
				string value = m_Values[ index ] ; 
				if( string.IsNullOrEmpty( value ) == true )
				{
					return 0 ;
				}
		
				ulong v = GetSpecialValue( ref value, out bool r ) ;
				if( r == true )
				{
					return ( short )( v & 0xFFFF ) ;
				}

				if( value[ value.Length - 1 ] == 'f' || value[ value.Length - 1 ] == 'F' )
				{
					value = value.Substring( 0, value.Length - 1 ) ;
				}
		
				if( double.TryParse( value, out double shortValue ) == false )
				{
					Debug.LogWarning( "Short Parse Error : " + value ) ;
					return 0 ;
				}
		
				return ( short )shortValue ;
			}

			/// <summary>
			/// 列の要素を符号なし16bit整数型の値として取得する
			/// </summary>
			/// <param name="index">列番号(0～)</param>
			/// <returns>列の要素(符号あり32bit整数型)</returns>
			public ushort GetUshort( int index )
			{
				if( m_Values == null )
				{
					return 0 ;
				}
		
				if( index <  0 || index >= m_Values.Length )
				{
					return 0 ;
				}
		
				string value = m_Values[ index ] ; 
				if( string.IsNullOrEmpty( value ) == true )
				{
					return 0 ;
				}
		
				ulong v = GetSpecialValue( ref value, out bool r ) ;
				if( r == true )
				{
					return ( ushort )( v & 0xFFFF ) ;
				}

				if( value[ value.Length - 1 ] == 'f' || value[ value.Length - 1 ] == 'F' )
				{
					value = value.Substring( 0, value.Length - 1 ) ;
				}
		
				if( double.TryParse( value, out double ushortValue ) == false )
				{
					Debug.LogWarning( "Ushort Parse Error : " + value ) ;
					return 0 ;
				}
		
				return ( ushort )ushortValue ;
			}

			/// <summary>
			/// 列の要素を符号あり32bit整数型の値として取得する
			/// </summary>
			/// <param name="index">列番号(0～)</param>
			/// <returns>列の要素(符号あり32bit整数型)</returns>
			public int GetInt( int index )
			{
				if( m_Values == null )
				{
					return 0 ;
				}
		
				if( index <  0 || index >= m_Values.Length )
				{
					return 0 ;
				}
		
				string value = m_Values[ index ] ; 
				if( string.IsNullOrEmpty( value ) == true )
				{
					return 0 ;
				}
		
				ulong v = GetSpecialValue( ref value, out bool r ) ;
				if( r == true )
				{
					return ( int )( v & 0xFFFFFFFF ) ;
				}

				if( value[ value.Length - 1 ] == 'f' || value[ value.Length - 1 ] == 'F' )
				{
					value = value.Substring( 0, value.Length - 1 ) ;
				}
		
				if( double.TryParse( value, out double intValue ) == false )
				{
					Debug.LogWarning( "Int Parse Error : " + value ) ;
					return 0 ;
				}
		
				return ( int )intValue ;
			}
	
			/// <summary>
			/// 列の要素を符号なし32bit整数型の値として取得する
			/// </summary>
			/// <param name="index">列番号(0～)</param>
			/// <returns>列の要素(符号なし32bit整数型)</returns>
			public uint GetUint( int index )
			{
				if( m_Values == null )
				{
					return 0 ;
				}
		
				if( index <  0 || index >= m_Values.Length )
				{
					return 0 ;
				}
		
				string value = m_Values[ index ] ;
				if( string.IsNullOrEmpty( value ) == true )
				{
					return 0 ;
				}
		
				ulong v = GetSpecialValue( ref value, out bool r ) ;
				if( r == true )
				{
					return ( uint )( v & 0xFFFFFFFF ) ;
				}

				if( value[ value.Length - 1 ] == 'f' || value[ value.Length - 1 ] == 'F' )
				{
					value = value.Substring( 0, value.Length - 1 ) ;
				}
		
				if( double.TryParse( value, out double uintValue ) == false )
				{
					Debug.LogWarning( "Uint Parse Error : " + value ) ;
					return 0 ;
				}
		
				return ( uint )uintValue ;
			}
	
			/// <summary>
			/// 列の要素を符号あり64bit整数型の値として取得する
			/// </summary>
			/// <param name="index">列番号(0～)</param>
			/// <returns>列の要素(符号あり64bit整数型)</returns>
			public long GetLong( int index )
			{
				if( m_Values == null )
				{
					return 0 ;
				}
		
				if( index <  0 || index >= m_Values.Length )
				{
					return 0 ;
				}
		
				string value = m_Values[ index ] ; 
				if( string.IsNullOrEmpty( value ) == true )
				{
					return 0 ;
				}
		
				ulong v = GetSpecialValue( ref value, out bool r ) ;
				if( r == true )
				{
					return ( long )v ;
				}

				if( value[ value.Length - 1 ] == 'f' || value[ value.Length - 1 ] == 'F' )
				{
					value = value.Substring( 0, value.Length - 1 ) ;
				}
		
				if( double.TryParse( value, out double longValue ) == false )
				{
					Debug.LogWarning( "Long Parse Error : " + value ) ;
					return 0 ;
				}
		
				return ( long )longValue ;
			}
	
			/// <summary>
			/// 列の要素を符号なし64bit整数型の値として取得する
			/// </summary>
			/// <param name="index">列番号(0～)</param>
			/// <returns>列の要素(符号なし64bit整数型)</returns>
			public ulong GetUlong( int index )
			{
				if( m_Values == null )
				{
					return 0 ;
				}
		
				if( index <  0 || index >= m_Values.Length )
				{
					return 0 ;
				}
		
				string value = m_Values[ index ] ; 
				if( string.IsNullOrEmpty( value ) == true )
				{
					return 0 ;
				}
		
				ulong v = GetSpecialValue( ref value, out bool r ) ;
				if( r == true )
				{
					return v ;
				}

				if( value[ value.Length - 1 ] == 'f' || value[ value.Length - 1 ] == 'F' )
				{
					value = value.Substring( 0, value.Length - 1 ) ;
				}
		
				if( double.TryParse( value, out double ulongValue ) == false )
				{
					Debug.LogWarning( "Long Parse Error : " + value ) ;
					return 0 ;
				}
		
				return ( ulong )ulongValue ;
			}
			
			/// <summary>
			/// 列の要素を単精度浮動小数型の値として取得する
			/// </summary>
			/// <param name="index">列番号(0～)</param>
			/// <returns>列の要素(単精度浮動小数型)</returns>
			public float GetFloat( int index )
			{
				if( m_Values == null )
				{
					return 0 ;
				}
		
				if( index <  0 || index >= m_Values.Length )
				{
					return 0 ;
				}
		
				string value = m_Values[ index ] ;
				if( string.IsNullOrEmpty( value ) == true )
				{
					return 0 ;
				}
		
				ulong v = GetSpecialValue( ref value, out bool r ) ;
				if( r == true )
				{
					return ( float )v ;
				}

				if( value[ value.Length - 1 ] == 'f' || value[ value.Length - 1 ] == 'F' )
				{
					value = value.Substring( 0, value.Length - 1 ) ;
				}
		
				if( float.TryParse( value, out float floatValue ) == false )
				{
					Debug.LogWarning( "Float Parse Error : " + value ) ;
					return 0 ;
				}
		
				return floatValue ;
			}
	
			/// <summary>
			/// 列の要素を倍精度浮動小数型の値として取得する
			/// </summary>
			/// <param name="index">列番号(0～)</param>
			/// <returns>列の要素(倍精度浮動小数型)</returns>
			public double GetDouble( int index )
			{
				if( m_Values == null )
				{
					return 0 ;
				}
		
				if( index <  0 || index >= m_Values.Length )
				{
					return 0 ;
				}
		
				string value = m_Values[ index ] ;
				if( string.IsNullOrEmpty( value ) == true )
				{
					return 0 ;
				}
		
				ulong v = GetSpecialValue( ref value, out bool r ) ;
				if( r == true )
				{
					return ( double )v ;
				}

				if( value[ value.Length - 1 ] == 'f' || value[ value.Length - 1 ] == 'F' )
				{
					value = value.Substring( 0, value.Length - 1 ) ;
				}
		
				if( double.TryParse( value, out double doubleValue ) == false )
				{
					Debug.LogWarning( "Double Parse Error : " + value ) ;
					return 0 ;
				}
		
				return doubleValue ;
			}
			
			/// <summary>
			/// 列の要素を文字列型の値として取得する
			/// </summary>
			/// <param name="index">列番号(0～)</param>
			/// <returns>列の要素(文字列)</returns>
			public string GetString( int index )
			{
				if( m_Values == null )
				{
					return "" ;
				}
				
				if( index <  0 || index >= m_Values.Length )
				{
					return "" ;
				}
		
				// \n \t とを改行コードとタブコードに直す
				string value = m_Values[ index ] ;
				if( string.IsNullOrEmpty( value ) == true )
				{
					return "" ;
				}
		
				int i, l = value.Length ;
				char[] a = new char[ l ] ;
				char w ;
				int p = 0 ;
		
				for( i  = 0 ; i <  l ; i ++ )
				{
					w = value[ i ] ;
					if( w == '\\' && i <  ( l - 1 ) && value[ i + 1 ] == 'n' )
					{
						// 改行発見
						a[ p ] = '\n' ;
						p ++ ;
						i ++ ;
					}
					else
					if( w == '\\' && i <  ( l - 1 ) && value[ i + 1 ] == 't' )
					{
						// タブ発見
						a[ p ] = '\t' ;
						p ++ ;
						i ++ ;
					}
					else
					if( w == '\\' && i <  ( l - 1 ) && value[ i + 1 ] == '"' )
					{
						// ダブルクォート発見
						a[ p ] = '"' ;
						p ++ ;
						i ++ ;
					}
					else
					{
						a[ p ] = w ;
						p ++ ;
					}
				}
		
				return new string( a, 0, p ) ;
			}
			
			/// <summary>
			/// 列の要素数を取得する
			/// </summary>
			/// <returns>列の要素数</returns>
			public int GetLength()
			{
				if( m_Values == null )
				{
					return 0 ;
				}
		
				return m_Values.Length ;
			}

			/// <summary>
			/// 列の要素数
			/// </summary>
			public int Length
			{
				get
				{
					return GetLength() ;
				}
			}
		}

		//-----------------------------------------------------------------------------------------

		/// <summary>
		/// ＣＳＶオブジェクトからバイナリ配列化したテーブル情報を取得する
		/// </summary>
		/// <param name="encode">文字列のエンコードタイプ(デフォルトは unicode)</param>
		/// <returns>バイナリ配列化したテーブル情報</returns>
		public byte[] ToBinaryTable( string encode = "unicode" )
		{
			if( m_Values == null || ( m_Values.Length ) <  3 )
			{
				return null ;
			}

			int r = m_Values.Length ;
			int c = m_Values[ 0 ].Length ;	// カラム数

			if( c == 0 )
			{
				return null ;
			}

			//-----------------------------------

			encode = encode.ToLower() ;
			Encoding utf8 = Encoding.GetEncoding( "UTF-8" ) ;

			int x, y, l, n, p, t, i, v ;
			uint u ;
			long lv ;
			ulong lu ;
			float f ;
			double d ;
			string w ;
			byte[] k ;

			// まずは必要となる総配列サイズ量を計算する

			p = 0 ;

			// カラム数
			v = c ;
			for( i  = 0 ; i <  4 ; i ++ )
			{
				p ++ ;
				v >>= 7 ;
				if( v == 0 )
				{
					break ;
				}
			}

			// レコード数
			v = r ;
			for( i  = 0 ; i <  4 ; i ++ )
			{
				p ++ ;
				v >>= 7 ;
				if( v == 0 )
				{
					break ;
				}
			}

			// カラム名
			if( encode == "utf8" || encode == "utf-8" )
			{
				// UTF-8
				for( x  = 0 ; x <  c ; x ++ )
				{
					n = utf8.GetByteCount( m_Values[  0 ][ x ] ) ;
					v = n ;
					for( i  = 0 ; i <  4 ; i ++ )
					{
						p ++ ;
						v >>= 7 ;
						if( v == 0 )
						{
							break ;
						}
					}
					p += n ;
				}
			}
			else
			{
				// Unicode
				for( x  = 0 ; x <  c ; x ++ )
				{
					n = m_Values[ 0 ][ x ].Length ;
					v = n ;
					for( i  = 0 ; i <  4 ; i ++ )
					{
						p ++ ;
						v >>= 7 ;
						if( v == 0 )
						{
							break ;
						}
					}
					p += n * 2 ;
				}
			}

			// タイプ
			p += c ;

			int[] type = new int[ c ] ;
			for( x  = 0 ; x <  c ; x ++ )
			{
				w = m_Values[ 1 ][ x ].ToLower() ;

				if( w == "bool" || w == "boolean" )
				{
					type[ x ] =  1 ;	// bool
				}
				else
				if( w == "byte" )
				{
					type[ x ] =  2 ;	// byte
				}
				else
				if( w == "char" )
				{
					type[ x ] =  3 ;	// char
				}
				else
				if( w == "short" || w == "int16" )
				{
					type[ x ] =  4 ;	// short
				}
				else
				if( w == "ushort" )
				{
					type[ x ] =  5 ;	// ushort
				}
				else
				if( w == "int" || w == "int32" )
				{
					type[ x ] =  6 ;	// int
				}
				else
				if( w == "uint" )
				{
					type[ x ] =  7 ;	// uint
				}
				else
				if( w == "long" || w == "int64" )
				{
					type[ x ] =  8 ;	// long
				}
				else
				if( w == "ulong" )
				{
					type[ x ] =  9 ;	// ulong
				}
				else
				if( w == "float" )
				{
					type[ x ] = 10 ;	// float
				}
				else
				if( w == "double" )
				{
					type[ x ] = 11 ;	// double
				}
				else
				if( w == "string" || w == "text" )
				{
					type[ x ] = 12 ;	// string
				}
			}

			l = r - 2 ;

			// データ部
			for( x  = 0 ; x <  c ; x ++ )
			{
				t = type[ x ] ;

				if( t ==  1 || t ==  2 )
				{
					// bool byte
					p += ( 1 * l ) ;
				}
				else
				if( t ==  3 || t ==  4 || t ==  5 )
				{
					// char short ushort
					p += ( 2 * l ) ;
				}
				else
				if( t ==  6 || t ==  7 || t == 10 )
				{
					// int uint float
					p += ( 4 * l ) ;
				}
				else
				if( t ==  8 || t ==  9 || t == 11 )
				{
					// long ulong
					p += ( 8 * l ) ;
				}
				else
				if( t == 12 )
				{
					// string

					if( encode == "utf8" || encode == "utf-8" )
					{
						// UTF-8
						for( y  = 2 ; y <  r ; y ++ )
						{
							n = utf8.GetByteCount( m_Values[ y ][ x ] ) ;
							v = n ;
							for( i  = 0 ; i <  4 ; i ++ )
							{
								p ++ ;
								v >>= 7 ;
								if( v == 0 )
								{
									break ;
								}
							}
							p += n ;
						}
					}
					else
					{
						// Unicode
						for( y  = 2 ; y <  r ; y ++ )
						{
							n = m_Values[ y ][ x ].Length ;
							v = n ;
							for( i  = 0 ; i <  4 ; i ++ )
							{
								p ++ ;
								v >>= 7 ;
								if( v == 0 )
								{
									break ;
								}
							}
							p += n * 2 ;
						}
					}
				}
			}

			// 必要サイズが決定しました
	//		Debug.LogWarning( "必要サイズ:" + p ) ;

			//-------------------------------------------------------------------------------------

			byte[] data = new byte[ p ] ;
			p = 0 ;

			// データを格納していく

			// カラム数
			v = c ;
			for( i  = 0 ; i <  4 ; i ++ )
			{
				data[ p ] = ( byte )( v & 0x7F ) ;
				v >>= 7 ;

				if( v >  0 )
				{
					data[ p ] = ( byte )( data[ p ] | 0x80 ) ;
					p ++ ;
				}
				else
				{
					p ++ ;
					break ;
				}
			}

			// レコード数
			v = r ;
			for( i  = 0 ; i <  4 ; i ++ )
			{
				data[ p ] = ( byte )( v & 0x7F ) ;
				v >>= 7 ;

				if( v >  0 )
				{
					data[ p ] = ( byte )( data[ p ] | 0x80 ) ;
					p ++ ;
				}
				else
				{
					p ++ ;
					break ;
				}
			}

			// カラム名
			if( encode == "utf8" || encode == "utf-8" )
			{
				// UTF-8
				for( x  = 0 ; x <  c ; x ++ )
				{
					k = utf8.GetBytes( m_Values[ 0 ][ x ] ) ;
					if( k != null )
					{
						n = k.Length ;
					}
					else
					{
						n = 0 ;
					}
					v = n ;
					for( i  = 0 ; i <  4 ; i ++ )
					{
						data[ p ] = ( byte )( v & 0x7F ) ;
						v >>= 7 ;

						if( v >  0 )
						{
							data[ p ] = ( byte )( data[ p ] | 0x80 ) ;
							p ++ ;
						}
						else
						{
							p ++ ;
							break ;
						}
					}

					Array.Copy( k, 0, data, p, n ) ;
					p += n ;
				}
			}
			else
			{
				// Unicode
				for( x  = 0 ; x <  c ; x ++ )
				{
					w = m_Values[ 0 ][ x ] ;

					n = w.Length ;
					v = n ;
					for( i  = 0 ; i <  4 ; i ++ )
					{
						data[ p ] = ( byte )( v & 0x7F ) ;
						v >>= 7 ;

						if( v >  0 )
						{
							data[ p ] = ( byte )( data[ p ] | 0x80 ) ;
							p ++ ;
						}
						else
						{
							p ++ ;
							break ;
						}
					}

					for( i  = 0 ; i <  n ; i ++ )
					{
						data[ p ] = ( byte )( w[ i ] & 0xFF ) ;
						p ++ ;
						data[ p ] = ( byte )( ( w[ i ] >> 8 ) & 0xFF ) ;
						p ++ ;
					}
				}
			}

			// タイプ
			for( x  = 0 ; x <  c ; x ++ )
			{
				data[ p ] = ( byte )type[ x ] ;
				p ++ ;
			}

			// データ部
			for( y  = 2 ; y <  r ; y ++ )
			{
				for( x  = 0 ; x <  c ; x ++ )
				{
					t = type[ x ] ;
					w = m_Values[ y ][ x ] ;

					if( t ==  1 )
					{
						// bool
						w = w.ToLower() ;
						if( w == "true" || w != "0" )
						{
							data[ p ] = 1 ;
						}
						else
						{
							data[ p ] = 0 ;
						}
						p ++ ;
					}
					else
					if( t ==  2 )
					{
						// bool byte
						data[ p ] = ( byte )( byte.Parse( w ) ) ;
						p ++ ;
					}
					else
					if( t ==  3 )
					{
						// char
						u = char.Parse( w ) ;
						data[ p ] = ( byte )( u & 0xFF ) ;
						p ++ ;
						data[ p ] = ( byte )( ( u >> 8 ) & 0xFF ) ;
						p ++ ;
					}
					else
					if( t ==  4 )
					{
						// short
						v = short.Parse( w ) ;
						data[ p ] = ( byte )( v & 0xFF ) ;
						p ++ ;
						data[ p ] = ( byte )( ( v >> 8 ) & 0xFF ) ;
						p ++ ;
					}
					else
					if( t ==  5 )
					{
						// ushort
						u = ushort.Parse( w ) ;
						data[ p ] = ( byte )( u & 0xFF ) ;
						p ++ ;
						data[ p ] = ( byte )( ( u >> 8 ) & 0xFF ) ;
						p ++ ;
					}
					else
					if( t ==  6 )
					{
						// int
						v = int.Parse( w ) ;
						for( i  = 0 ; i <  4 ; i ++ )
						{
							data[ p ] = ( byte )( ( v >> ( i * 8 ) ) & 0xFF ) ;
							p ++ ;
						}
					}
					else
					if( t ==  7 )
					{
						// uint
						u = uint.Parse( w ) ;
						for( i  = 0 ; i <  4 ; i ++ )
						{
							data[ p ] = ( byte )( ( u >> ( i * 8 ) ) & 0xFF ) ;
							p ++ ;
						}
					}
					else
					if( t ==  8 )
					{
						// long
						lv = long.Parse( w ) ;
						for( i  = 0 ; i <  8 ; i ++ )
						{
							data[ p ] = ( byte )( ( lv >> ( i * 8 ) ) & 0xFF ) ;
							p ++ ;
						}
					}
					else
					if( t ==  9 )
					{
						// ulong
						lu = ulong.Parse( w ) ;
						for( i  = 0 ; i <  8 ; i ++ )
						{
							data[ p ] = ( byte )( ( lu >> ( i * 8 ) ) & 0xFF ) ;
							p ++ ;
						}
					}
					else
					if( t == 10 )
					{
						// float
						f = float.Parse( w ) ;
						byte[] b = BitConverter.GetBytes( f ) ;
						for( i  = 0 ; i <  4 ; i ++ )
						{
							data[ p ] = b [ i ] ;
							p ++ ;
						}
					}
					else
					if( t == 11 )
					{
						// double
						d = double.Parse( w ) ;
						byte[] b = BitConverter.GetBytes( d ) ;
						for( i  = 0 ; i <  8 ; i ++ )
						{
							data[ p ] = b [ i ] ;
							p ++ ;
						}
					}
					else
					if( t == 12 )
					{
						// string
						if( encode == "utf8" || encode == "utf-8" )
						{
							// UTF-8
							k = utf8.GetBytes( w ) ;
							if( k != null )
							{
								n = k.Length ;
							}
							else
							{
								n = 0 ;
							}
							v = n ;
							for( i  = 0 ; i <  4 ; i ++ )
							{
								data[ p ] = ( byte )( v & 0x7F ) ;
								v >>= 7 ;

								if( v >  0 )
								{
									data[ p ] = ( byte )( data[ p ] | 0x80 ) ;
									p ++ ;
								}
								else
								{
									p ++ ;
									break ;
								}
							}

							Array.Copy( k, 0, data, p, n ) ;
							p += n ;
						}
						else
						{
							// Unicode
							n = w.Length ;
							v = n ;
							for( i  = 0 ; i <  4 ; i ++ )
							{
								data[ p ] = ( byte )( v & 0x7F ) ;
								v >>= 7 ;

								if( v >  0 )
								{
									data[ p ] = ( byte )( data[ p ] | 0x80 ) ;
									p ++ ;
								}
								else
								{
									p ++ ;
									break ;
								}
							}

							for( i  = 0 ; i <  n ; i ++ )
							{
								data[ p ] = ( byte )( w[ i ] & 0xFF ) ;
								p ++ ;
								data[ p ] = ( byte )( ( w[ i ] >> 8 ) & 0xFF ) ;
								p ++ ;
							}
						}
					}
				}
			}

	//		Debug.LogWarning( "格納サイズ:" + p ) ;

			return data ;
		}


		//--------------------------------------------------------------------------------------------

		/// <summary>
		/// ＣＳＶテキストから任意クラスのリストとハッシュオブジェクトを生成する(必ず０行目を識別名・１行目を型とし、識別名id・型intを含めること)
		/// </summary>
		/// <typeparam name="K">キー値の型</typeparam>
		/// <typeparam name="T">任意クラスの型</typeparam>
		/// <param name="text">ＣＳＶテキスト(区切りはカンマまたはタブ)</param>
		/// <param name="rList">(出力)リストオブジェクト</param>
		/// <param name="rHash">(出力)ハッシュオブジェクト</param>
		/// <param name="key">キー値の名前</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Load<T>( string text, ref List<T> rList, int startColumn = 0, int startRow = 0, int nameRow = 0, int typeRow = 1, int dataRow = 2, int keyColumn = -1 ) where T : class, new()
		{
			List<Dictionary<string,System.Object>> data = LoadFromText( text, startColumn, startRow, nameRow, typeRow, dataRow, keyColumn ) ;
			if( data == null )
			{
				// 失敗
				return false ;
			}
		
			//------------------------------------------------------------------

			if( rList == null )
			{
				rList	= new List<T>() ;
			}
			else
			{
				rList.Clear() ;
			}

			// リフレクションで値を格納する
			int i, l = data.Count ;
			
			T record ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				record = new T() ;
				Initialize<T>( ref record, data[ i ] ) ;
				
				rList.Add( record ) ;
			}

			return true ;
		}

		// テキストから配列を生成する
		private static List<Dictionary<string,System.Object>> LoadFromText( string text, int startColumn, int startRow, int nameRow, int typeRow, int dataRow, int keyColumn )
		{
			if( string.IsNullOrEmpty( text ) == true )
			{
				// 失敗
				return null ;
			}

			if( startRow >  0 )
			{
				nameRow -= startRow ;
				typeRow -= startRow ;
				dataRow -= startRow ;
			}

			if( startColumn >  0 && keyColumn >= startColumn )
			{
				keyColumn -= startColumn ;
			}

			CSVObject csv = new CSVObject( text, true, startColumn, startRow ) ;
			if( csv.Length <= nameRow || csv.Length <= typeRow || csv.Length <= dataRow )
			{
				return null ;
			}

			//------------------------------------------------------------------

			int r, c, m, l = csv.Length ;
			string name ;
			string type ;


			List<Dictionary<string,System.Object>> data = new List<Dictionary<string, object>>() ;

			m = csv[ nameRow ].Length ;
			for( r  = dataRow ; r <  l ; r ++ )
			{
				if( keyColumn >= 0 && keyColumn <  csv[ r ].Length )
				{
					if( string.IsNullOrEmpty( csv[ r ][ keyColumn ] ) == true )
					{
						continue ;	// このレコードはスキップする
					}
				}

				Dictionary<string,System.Object> line = new Dictionary<string, object>() ;
				for( c  = 0 ; c <  m ; c ++ )
				{
					name = csv[ nameRow ].GetString( c ) ;
					if( string.IsNullOrEmpty( name ) == false )
					{
						if( line.ContainsKey( name ) == true )
						{
#if UNITY_EDITOR
							Debug.LogWarning( "カラム名が重複している:" + name ) ;
#endif
						}

						type  = csv[ typeRow ].GetString( c ).ToLower() ;

						if( type == "bool" )
						{
							line.Add( name, csv[ r ].GetBoolean( c ) ) ;
						}
						else
						if( type == "byte" )
						{
							line.Add( name, csv[ r ].GetByte( c ) ) ;
						}
						else
						if( type == "char" )
						{
							line.Add( name, csv[ r ].GetChar( c ) ) ;
						}
						else
						if( type == "short" )
						{
							line.Add( name, csv[ r ].GetShort( c ) ) ;
						}
						else
						if( type == "ushort" )
						{
							line.Add( name, csv[ r ].GetUshort( c ) ) ;
						}
						else
						if( type == "int" )
						{
							line.Add( name, csv[ r ].GetInt( c ) ) ;
						}
						else
						if( type == "uint" )
						{
							line.Add( name, csv[ r ].GetUint( c ) ) ;
						}
						else
						if( type == "long" )
						{
							line.Add( name, csv[ r ].GetLong( c ) ) ;
						}
						else
						if( type == "ulong" )
						{
							line.Add( name, csv[ r ].GetUlong( c ) ) ;
						}
						else
						if( type == "float" )
						{
							line.Add( name, csv[ r ].GetFloat( c ) ) ;
						}
						else
						if( type == "double" )
						{
							line.Add( name, csv[ r ].GetDouble( c ) ) ;
						}
						else
						{
							line.Add( name, csv[ r ].GetString( c ) ) ;
						}
					}
				}

				data.Add( line ) ;
			}

			return data ;
		}
		
		/// <summary>
		/// ＣＳＶテキストから任意クラスのリストとハッシュオブジェクトを生成する(必ず０行目を識別名・１行目を型とし、識別名id・型intを含めること)
		/// </summary>
		/// <typeparam name="K">キー値の型</typeparam>
		/// <typeparam name="T">任意クラスの型</typeparam>
		/// <param name="text">ＣＳＶテキスト(区切りはカンマまたはタブ)</param>
		/// <param name="rList">(出力)リストオブジェクト</param>
		/// <param name="rHash">(出力)ハッシュオブジェクト</param>
		/// <param name="key">キー値の名前</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Load<K,T>( string text, ref List<T> rList, ref Dictionary<K,T> rHash, string key = "id", int startColumn = 0, int startRow = 0, int nameRow = 0, int typeRow = 1, int dataRow = 2, int keyColumn = -1 ) where T : class, new()
		{
			Dictionary<K,Dictionary<string,System.Object>> data = LoadFromText<K>( text, key, startColumn, startRow, nameRow, typeRow, dataRow, keyColumn ) ;
			if( data == null )
			{
				// 失敗
				return false ;
			}
		
			//------------------------------------------------------------------

			if( rList == null )
			{
				rList	= new List<T>() ;
			}
			else
			{
				rList.Clear() ;
			}

			if( rHash == null )
			{
				rHash	= new Dictionary<K,T>() ;
			}
			else
			{
				rHash.Clear() ;
			}

			// リフレクションで値を格納する
			int i, l = data.Count ;
			
			K[] ids = new K[ l ] ;
			data.Keys.CopyTo( ids, 0 ) ;
		
			T record ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( rHash.ContainsKey( ids[ i ] ) == false )
				{
					record = new T() ;
					Initialize<T>( ref record, data[ ids[ i ] ] ) ;
				
					rList.Add( record ) ;
					rHash.Add( ids[ i ], record ) ;
				}
			}

			return true ;
		}


		// テキストから配列を生成する
		private static Dictionary<K,Dictionary<string,System.Object>> LoadFromText<K>( string text, string key, int startColumn, int startRow, int nameRow, int typeRow, int dataRow, int keyColumn )
		{
			if( string.IsNullOrEmpty( text ) == true )
			{
				// 失敗
				return null ;
			}

			if( startRow >  0 )
			{
				nameRow -= startRow ;
				typeRow -= startRow ;
				dataRow -= startRow ;
			}

			if( startColumn >  0 && keyColumn >= startColumn )
			{
				keyColumn -= startColumn ;
			}

			CSVObject csv = new CSVObject( text, true, startColumn, startRow ) ;
			if( csv.Length <= nameRow || csv.Length <= typeRow || csv.Length <= dataRow )
			{
				return null ;
			}

			//------------------------------------------------------------------

			int r, c, m, l = csv.Length ;
			string name ;
			string type ;
			K id ;

			Dictionary<K,Dictionary<string,System.Object>> data = new Dictionary<K, Dictionary<string, object>>() ;

			m = csv[ nameRow ].Length ;
			for( r  = dataRow ; r <  l ; r ++ )
			{
				if( keyColumn >= 0 && keyColumn <  csv[ r ].Length )
				{
					if( string.IsNullOrEmpty( csv[ r ][ keyColumn ] ) == true )
					{
						continue ;	// このレコードはスキップする
					}
				}

				Dictionary<string,System.Object> line = new Dictionary<string, object>() ;
				for( c  = 0 ; c <  m ; c ++ )
				{
					name = csv[ nameRow ].GetString( c ) ;
					if( string.IsNullOrEmpty( name ) == false )
					{
						if( line.ContainsKey( name ) == true )
						{
#if UNITY_EDITOR
							Debug.LogWarning( "カラム名が重複している:" + name ) ;
#endif
						}

						type = csv[ typeRow ].GetString( c ).ToLower() ;

						if( type == "bool" )
						{
							line.Add( name, csv[ r ].GetBoolean( c ) ) ;
						}
						else
						if( type == "byte" )
						{
							line.Add( name, csv[ r ].GetByte( c ) ) ;
						}
						else
						if( type == "char" )
						{
							line.Add( name, csv[ r ].GetChar( c ) ) ;
						}
						else
						if( type == "short" )
						{
							line.Add( name, csv[ r ].GetShort( c ) ) ;
						}
						else
						if( type == "ushort" )
						{
							line.Add( name, csv[ r ].GetUshort( c ) ) ;
						}
						else
						if( type == "int" )
						{
							line.Add( name, csv[ r ].GetInt( c ) ) ;
						}
						else
						if( type == "uint" )
						{
							line.Add( name, csv[ r ].GetUint( c ) ) ;
						}
						else
						if( type == "long" )
						{
							line.Add( name, csv[ r ].GetLong( c ) ) ;
						}
						else
						if( type == "ulong" )
						{
							line.Add( name, csv[ r ].GetUlong( c ) ) ;
						}
						else
						if( type == "float" )
						{
							line.Add( name, csv[ r ].GetFloat( c ) ) ;
						}
						else
						if( type == "double" )
						{
							line.Add( name, csv[ r ].GetDouble( c ) ) ;
						}
						else
						{
							line.Add( name, csv[ r ].GetString( c ) ) ;
						}
					}
				}

				if( line.ContainsKey( key ) == true )
				{
					id = ( K )line[ key ] ;

					if( data.ContainsKey( id ) == false )
					{
						data.Add( id, line ) ;
					}
					else
					{
						Debug.LogWarning( "[CSVHelper] キー値が重複します: Key = ( " + key  + " ) Value = ( " + id + " )\nText = " + text ) ;
					}
				}
				else
				{
					Debug.LogWarning( "[CSVHelper] キー値が存在しません: Key = " + key ) ;
				}
			}

			return data ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// バイナリ配列化したテーブル情報から任意クラスのリストとハッシュオブジェクトを生成する(必ず０行目を識別名・１行目を型とし、識別名id・型intを含めること)
		/// </summary>
		/// <typeparam name="K">キー値の型</typeparam>
		/// <typeparam name="T">任意クラスの型</typeparam>
		/// <param name="binary">バイナリ配列化したテーブル情報</param>
		/// <param name="rList">(出力)リストオブジェクト</param>
		/// <param name="rHash">(出力)ハッシュオブジェクト</param>
		/// <param name="key">キー値の名前</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Load<T>( byte[] binary, ref List<T> rList ) where T : class, new()
		{
			// ロードする
			List<Dictionary<string,System.Object>> data = LoadFromBinary( binary ) ;
			if( data == null )
			{
				// 失敗
				return false ;
			}

			//------------------------------------------------------------------

			if( rList == null )
			{
				rList	= new List<T>() ;
			}
			else
			{
				rList.Clear() ;
			}

			// リフレクションで値を格納する
			int i, l = data.Count ;

			T record ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				record = new T() ;
				Initialize<T>( ref record, data[ i ] ) ;
					
				rList.Add( record ) ;
			}

			return true ;
		}

		// バイナリ型ＣＳＶからデータ配列を生成する
		private static List<Dictionary<string,System.Object>> LoadFromBinary( byte[] binary, string encode = "unicode" )
		{
			if( binary == null )
			{
				return null ;
			}

			encode = encode.ToLower() ;
			Encoding utf8 = Encoding.GetEncoding( "UTF-8" ) ;

			byte b, t ;
			int i, p, x, y, n ;
			char[] w ;

			p = 0 ;

			// カラム数を取得
			int c = 0 ;
			for( i  = 0 ; i <  4 ; i ++ )
			{
				b = binary[ p ] ;
				p ++ ;

				c |= ( ( b & 0x7F ) << ( 7 * i ) ) ;
				if( ( b & 0x80 ) == 0 )
				{
					break ;
				}
			}

			// レコード数を取得
			int r = 0 ;
			for( i  = 0 ; i <  4 ; i ++ )
			{
				b = binary[ p ] ;
				p ++ ;

				r |= ( ( b & 0x7F ) << ( 7 * i ) ) ;
				if( ( b & 0x80 ) == 0 )
				{
					break ;
				}
			}

			// ラベル名を取得
			string[] labels = new string[ c ] ;

			if( encode == "utf8" || encode == "utf-8" )
			{
				// UTF-8
				for( x  = 0 ; x <  c ; x ++ )
				{
					// 文字数
					n = 0 ;
					for( i  = 0 ; i <  4 ; i ++ )
					{
						b = binary[ p ] ;
						p ++ ;

						n |= ( ( b & 0x7F ) << ( 7 * i ) ) ;
						if( ( b & 0x80 ) == 0 )
						{
							break ;
						}
					}

					// Unicode 想定
					labels[ x ] = utf8.GetString( binary, p, n ) ;
					p += n ;
				}
			}
			else
			{
				// Unicode
				for( x  = 0 ; x <  c ; x ++ )
				{
					// 文字数
					n = 0 ;
					for( i  = 0 ; i <  4 ; i ++ )
					{
						b = binary[ p ] ;
						p ++ ;

						n |= ( ( b & 0x7F ) << ( 7 * i ) ) ;
						if( ( b & 0x80 ) == 0 )
						{
							break ;
						}
					}

					w = new char[ n ] ;
					for( i  = 0 ; i <  n ; i ++ )
					{
						w[ i ] = ( char )( ( binary[ p + 1 ] << 8 ) | binary[ p ] ) ;
						p += 2 ;
					}
					labels[ x ] = new string( w ) ;

	//					Debug.LogWarning( "ラベル名:" + tLabel[ x ] ) ;
				}
			}


			// タイプを取得
			byte[] type = new byte[ x ] ;
			for( x  = 0 ; x <  c ; x ++ )
			{
				type[ x ] = binary[ p ] ;
				p ++ ;
			}


			List<Dictionary<string,System.Object>> data = new List<Dictionary<string, object>>() ;

			for( y  = 2 ; y <  r ; y ++ )
			{
				Dictionary<string,System.Object> line = new Dictionary<string, object>() ;
				for( x  = 0 ; x <  c ; x ++ )
				{
					t = type[ x ] ;

					if( t ==  1 )
					{
						// bool
						if( binary[ p ] == 0 )
						{
							line.Add( labels[ x ], false ) ;
						}
						else
						{
							line.Add( labels[ x ], true ) ;
						}
						p ++ ;
					}
					else
					if( t ==  2 )
					{
						// byte
						line.Add( labels[ x ], binary[ p ] ) ;
						p ++ ;
					}
					else
					if( t ==  3 )
					{
						// char
						line.Add( labels[ x ], ( char )( ( binary[ p + 1 ] <<  8 ) | binary[ p ] ) ) ;
						p += 2 ;
					}
					else
					if( t ==  4 )
					{
						// short
						line.Add( labels[ x ], ( short )( ( binary[ p + 1 ] <<  8 ) | binary[ p ] ) ) ;
						p += 2 ;
					}
					else
					if( t ==  5 )
					{
						// ushort
						line.Add( labels[ x ], ( ushort )( ( binary[ p + 1 ] <<  8 ) | binary[ p ] ) ) ;
						p += 2 ;
					}
					else
					if( t ==  6 )
					{
						// int
						line.Add( labels[ x ], ( int )( ( binary[ p + 3 ] << 24 ) |  ( binary[ p + 2 ] << 16 ) | ( binary[ p + 1 ] <<  8 ) | binary[ p ] ) ) ;
						p += 4 ;
					}
					else
					if( t ==  7 )
					{
						// uint
						line.Add( labels[ x ], ( uint )( ( binary[ p + 3 ] << 24 ) | ( binary[ p + 2 ] << 16 ) | ( binary[ p + 1 ] <<  8 ) | binary[ p ] ) ) ;
						p += 4 ;
					}
					else
					if( t ==  8 )
					{
						// long
						line.Add( labels[ x ], ( long )( ( binary[ p + 7 ] << 56 ) | ( binary[ p + 6 ] << 48 ) | ( binary[ p + 5 ] << 40 ) | ( binary[ p + 4 ] << 32 ) | ( binary[ p + 3 ] << 24 ) | ( binary[ p + 2 ] << 16 ) | ( binary[ p + 1 ] <<  8 ) | binary[ p ] ) ) ;
						p += 8 ;
					}
					else
					if( t ==  9 )
					{
						// ulong
						line.Add( labels[ x ], ( ulong )( ( binary[ p + 7 ] << 56 ) | ( binary[ p + 6 ] << 48 ) | ( binary[ p + 5 ] << 40 ) | ( binary[ p + 4 ] << 32 ) | ( binary[ p + 3 ] << 24 ) | ( binary[ p + 2 ] << 16 ) | ( binary[ p + 1 ] <<  8 ) | binary[ p ] ) ) ;
						p += 8 ;
					}
					else
					if( t == 10 )
					{
						// float
						line.Add( labels[ x ], BitConverter.ToSingle( binary, p ) ) ;
						p += 4 ;
					}
					else
					if( t == 11 )
					{
						// double
						line.Add( labels[ x ], BitConverter.ToDouble( binary, p ) ) ;
						p += 8 ;
					}
					else
					if( t == 12 )
					{
						// string
						if( encode == "utf8" || encode == "utf-8" )
						{
							n = 0 ;
							for( i  = 0 ; i <  4 ; i ++ )
							{
								b = binary[ p ] ;
								p ++ ;

								n |= ( ( b & 0x7F ) << ( 7 * i ) ) ;
								if( ( b & 0x80 ) == 0 )
								{
									break ;
								}
							}

							line.Add( labels[ x ], utf8.GetString( binary, p, n ) ) ;
							p += n ;
						}
						else
						{
							// Unicode
							n = 0 ;
							for( i  = 0 ; i <  4 ; i ++ )
							{
								b = binary[ p ] ;
								p ++ ;

								n |= ( ( b & 0x7F ) << ( 7 * i ) ) ;
								if( ( b & 0x80 ) == 0 )
								{
									break ;
								}
							}

							// Unicode 想定
							w = new char[ n ] ;
							for( i  = 0 ; i <  n ; i ++ )
							{
								w[ i ] = ( char )( ( binary[ p + 1 ] << 8 ) | binary[ p ] ) ;
								p += 2 ;
							}
							line.Add( labels[ x ], new string( w ) ) ;
						}
					}
				}

				data.Add( line ) ;
			}

			return data ;
		}
		//-----------------------------------------------------------

		/// <summary>
		/// バイナリ配列化したテーブル情報から任意クラスのリストとハッシュオブジェクトを生成する(必ず０行目を識別名・１行目を型とし、識別名id・型intを含めること)
		/// </summary>
		/// <typeparam name="K">キー値の型</typeparam>
		/// <typeparam name="T">任意クラスの型</typeparam>
		/// <param name="binary">バイナリ配列化したテーブル情報</param>
		/// <param name="rList">(出力)リストオブジェクト</param>
		/// <param name="rHash">(出力)ハッシュオブジェクト</param>
		/// <param name="key">キー値の名前</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Load<K,T>( byte[] binary, ref List<T> rList, ref Dictionary<K,T> rHash, string key = "id" ) where T : class, new()
		{
			// ロードする
			Dictionary<K,Dictionary<string,System.Object>> data = LoadFromBinary<K>( binary, key ) ;
			if( data == null )
			{
				// 失敗
				return false ;
			}

			//------------------------------------------------------------------

			if( rList == null )
			{
				rList	= new List<T>() ;
			}
			else
			{
				rList.Clear() ;
			}

			if( rHash == null )
			{
				rHash	= new Dictionary<K,T>() ;
			}
			else
			{
				rHash.Clear() ;
			}

			// リフレクションで値を格納する
			int i, l = data.Count ;

			K[] ids = new K[ l ] ;
			data.Keys.CopyTo( ids, 0 ) ;
		
			T record ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( rHash.ContainsKey( ids[ i ] ) == false )
				{
					record = new T() ;
					Initialize<T>( ref record, data[ ids[ i ] ] ) ;
					
					rList.Add( record ) ;
					rHash.Add( ids[ i ], record ) ;
				}
			}

			return true ;
		}

		// バイナリ型ＣＳＶからデータ配列を生成する
		private static Dictionary<K,Dictionary<string,System.Object>> LoadFromBinary<K>( byte[] binary, string key, string encode = "unicode" )
		{
			if( binary == null )
			{
				return null ;
			}

			encode = encode.ToLower() ;
			Encoding utf8 = Encoding.GetEncoding( "UTF-8" ) ;

			byte b, t ;
			int i, p, x, y, n ;
			char[] w ;

			p = 0 ;

			// カラム数を取得
			int c = 0 ;
			for( i  = 0 ; i <  4 ; i ++ )
			{
				b = binary[ p ] ;
				p ++ ;

				c |= ( ( b & 0x7F ) << ( 7 * i ) ) ;
				if( ( b & 0x80 ) == 0 )
				{
					break ;
				}
			}

			// レコード数を取得
			int r = 0 ;
			for( i  = 0 ; i <  4 ; i ++ )
			{
				b = binary[ p ] ;
				p ++ ;

				r |= ( ( b & 0x7F ) << ( 7 * i ) ) ;
				if( ( b & 0x80 ) == 0 )
				{
					break ;
				}
			}

			// ラベル名を取得
			string[] labels = new string[ c ] ;

			if( encode == "utf8" || encode == "utf-8" )
			{
				// UTF-8
				for( x  = 0 ; x <  c ; x ++ )
				{
					// 文字数
					n = 0 ;
					for( i  = 0 ; i <  4 ; i ++ )
					{
						b = binary[ p ] ;
						p ++ ;

						n |= ( ( b & 0x7F ) << ( 7 * i ) ) ;
						if( ( b & 0x80 ) == 0 )
						{
							break ;
						}
					}

					// Unicode 想定
					labels[ x ] = utf8.GetString( binary, p, n ) ;
					p += n ;
				}
			}
			else
			{
				// Unicode
				for( x  = 0 ; x <  c ; x ++ )
				{
					// 文字数
					n = 0 ;
					for( i  = 0 ; i <  4 ; i ++ )
					{
						b = binary[ p ] ;
						p ++ ;

						n |= ( ( b & 0x7F ) << ( 7 * i ) ) ;
						if( ( b & 0x80 ) == 0 )
						{
							break ;
						}
					}

					w = new char[ n ] ;
					for( i  = 0 ; i <  n ; i ++ )
					{
						w[ i ] = ( char )( ( binary[ p + 1 ] << 8 ) | binary[ p ] ) ;
						p += 2 ;
					}
					labels[ x ] = new string( w ) ;

	//					Debug.LogWarning( "ラベル名:" + tLabel[ x ] ) ;
				}
			}


			// タイプを取得
			byte[] type = new byte[ x ] ;
			for( x  = 0 ; x <  c ; x ++ )
			{
				type[ x ] = binary[ p ] ;
				p ++ ;
			}

			K id ;


			Dictionary<K,Dictionary<string,System.Object>> data = new Dictionary<K, Dictionary<string, object>>() ;

			for( y  = 2 ; y <  r ; y ++ )
			{
				Dictionary<string,System.Object> line = new Dictionary<string, object>() ;
				for( x  = 0 ; x <  c ; x ++ )
				{
					t = type[ x ] ;

					if( t ==  1 )
					{
						// bool
						if( binary[ p ] == 0 )
						{
							line.Add( labels[ x ], false ) ;
						}
						else
						{
							line.Add( labels[ x ], true ) ;
						}
						p ++ ;
					}
					else
					if( t ==  2 )
					{
						// byte
						line.Add( labels[ x ], binary[ p ] ) ;
						p ++ ;
					}
					else
					if( t ==  3 )
					{
						// char
						line.Add( labels[ x ], ( char )( ( binary[ p + 1 ] <<  8 ) | binary[ p ] ) ) ;
						p += 2 ;
					}
					else
					if( t ==  4 )
					{
						// short
						line.Add( labels[ x ], (  short )( ( binary[ p + 1 ] <<  8 ) | binary[ p ] ) ) ;
						p += 2 ;
					}
					else
					if( t ==  5 )
					{
						// ushort
						line.Add( labels[ x ], ( ushort )( ( binary[ p + 1 ] <<  8 ) | binary[ p ] ) ) ;
						p += 2 ;
					}
					else
					if( t ==  6 )
					{
						// int
						line.Add( labels[ x ], (  int )( ( binary[ p + 3 ] << 24 ) | ( binary[ p + 2 ] << 16 ) | ( binary[ p + 1 ] <<  8 ) | binary[ p ] ) ) ;
						p += 4 ;
					}
					else
					if( t ==  7 )
					{
						// uint
						line.Add( labels[ x ], ( uint )( ( binary[ p + 3 ] << 24 ) | ( binary[ p + 2 ] << 16 ) | ( binary[ p + 1 ] <<  8 ) | binary[ p ] ) ) ;
						p += 4 ;
					}
					else
					if( t ==  8 )
					{
						// long
						line.Add( labels[ x ], (  long )( ( binary[ p + 7 ] << 56 ) | ( binary[ p + 6 ] << 48 ) | ( binary[ p + 5 ] << 40 ) | ( binary[ p + 4 ] << 32 ) | ( binary[ p + 3 ] << 24 ) | ( binary[ p + 2 ] << 16 ) | ( binary[ p + 1 ] <<  8 ) | binary[ p ] ) ) ;
						p += 8 ;
					}
					else
					if( t ==  9 )
					{
						// ulong
						line.Add( labels[ x ], ( ulong )( ( binary[ p + 7 ] << 56 ) | ( binary[ p + 6 ] << 48 ) | ( binary[ p + 5 ] << 40 ) | ( binary[ p + 4 ] << 32 ) | ( binary[ p + 3 ] << 24 ) | ( binary[ p + 2 ] << 16 ) | ( binary[ p + 1 ] <<  8 ) | binary[ p ] ) ) ;
						p += 8 ;
					}
					else
					if( t == 10 )
					{
						// float
						line.Add( labels[ x ], BitConverter.ToSingle( binary, p ) ) ;
						p += 4 ;
					}
					else
					if( t == 11 )
					{
						// double
						line.Add( labels[ x ], BitConverter.ToDouble( binary, p ) ) ;
						p += 8 ;
					}
					else
					if( t == 12 )
					{
						// string
						if( encode == "utf8" || encode == "utf-8" )
						{
							n = 0 ;
							for( i  = 0 ; i <  4 ; i ++ )
							{
								b = binary[ p ] ;
								p ++ ;

								n |= ( ( b & 0x7F ) << ( 7 * i ) ) ;
								if( ( b & 0x80 ) == 0 )
								{
									break ;
								}
							}

							line.Add( labels[ x ], utf8.GetString( binary, p, n ) ) ;
							p += n ;
						}
						else
						{
							// Unicode
							n = 0 ;
							for( i  = 0 ; i <  4 ; i ++ )
							{
								b = binary[ p ] ;
								p ++ ;

								n |= ( ( b & 0x7F ) << ( 7 * i ) ) ;
								if( ( b & 0x80 ) == 0 )
								{
									break ;
								}
							}

							// Unicode 想定
							w = new char[ n ] ;
							for( i  = 0 ; i <  n ; i ++ )
							{
								w[ i ] = ( char )( ( binary[ p + 1 ] << 8 ) | binary[ p ] ) ;
								p += 2 ;
							}
							line.Add( labels[ x ], new string( w ) ) ;
						}
					}
				}

				if( line.ContainsKey( key ) == true )
				{
					id = ( K )line[ key ] ;

					if( data.ContainsKey( id ) == false )
					{
						data.Add( id, line ) ;
					}
					else
					{
						Debug.LogWarning( "キー名が重複します:" + id ) ;
					}
				}
				else
				{
					Debug.LogWarning( "キー名が存在しません:" + key ) ;
				}
			}

			return data ;
		}

		// リフレクションで値を格納する
		private static  bool Initialize<T>( ref T rTarget, Dictionary<string,System.Object> o )
		{
			if( o == null )
			{
				return false ;	// オブジェクトが空であれば展開しない
			}

			//--------------------------

			System.Object thisObject = ( System.Object )rTarget ;	// 自身のオブジェクト
			System.Type   thisType   = rTarget.GetType() ;		// 自身のタイプ（クラス）
		
			// メンバ情報を取得する
			MemberInfo[] memberInfoArray = thisType.GetMembers
			(
				BindingFlags.Public |
				BindingFlags.NonPublic |
				BindingFlags.Instance |
				// BindingFlags.Static |
				BindingFlags.DeclaredOnly |
				BindingFlags.FlattenHierarchy			// 継承元のメンバも含める
			) ;

			BindingFlags flags = ( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance ) ;

			// メンバごとに処理を行う
			foreach( MemberInfo m in memberInfoArray )
			{
				if( m.MemberType == MemberTypes.Field )
				{
					//---------------------------
				
					// このメンバの型を取得する
					System.Type fieldType = thisType.GetField( m.Name, flags ).FieldType ;
				
					// フィールドタイプ（メンバの型）の種類に応じて値を格納する
					if( fieldType.IsArray == false )
					{
						// スカラー型
					
						if( o.ContainsKey( m.Name ) == true )
						{
							if( fieldType.IsEnum == true )
							{
								// 列挙子
								thisType.GetField( m.Name, flags ).SetValue( thisObject, Enum.ToObject( fieldType, o[ m.Name ] ) ) ;
							}
							else
							{
								if( fieldType == typeof( bool ) )
								{
									// ブーリアン型のスカラー値
									thisType.GetField( m.Name, flags ).SetValue( thisObject, ( bool )o[ m.Name ] ) ;
								}
								else
								if( fieldType == typeof( byte ) )
								{
									// インテジャー型のスカラー値
									thisType.GetField( m.Name, flags ).SetValue( thisObject, ( byte )o[ m.Name ] ) ;
								}
								else
								if( fieldType == typeof( char ) )
								{
									// インテジャー型のスカラー値
									thisType.GetField( m.Name, flags ).SetValue( thisObject, ( char )o[ m.Name ] ) ;
								}
								else
								if( fieldType == typeof( short ) )
								{
									// インテジャー型のスカラー値
									thisType.GetField( m.Name, flags ).SetValue( thisObject, ( short )o[ m.Name ] ) ;
								}
								else
								if( fieldType == typeof( ushort ) )
								{
									// インテジャー型のスカラー値
									thisType.GetField( m.Name, flags ).SetValue( thisObject, ( ushort )o[ m.Name ] ) ;
								}
								else
								if( fieldType == typeof( int ) )
								{
									// インテジャー型のスカラー値
									thisType.GetField( m.Name, flags ).SetValue( thisObject, ( int )o[ m.Name ] ) ;
								}
								else
								if( fieldType == typeof( uint ) )
								{
									// インテジャー型のスカラー値
									thisType.GetField( m.Name, flags ).SetValue( thisObject, ( uint )o[ m.Name ] ) ;
								}
								else
								if( fieldType == typeof( long ) )
								{
									// インテジャー型のスカラー値
									thisType.GetField( m.Name, flags ).SetValue( thisObject, ( long )o[ m.Name ] ) ;
								}
								else
								if( fieldType == typeof( ulong ) )
								{
									// インテジャー型のスカラー値
									thisType.GetField( m.Name, flags ).SetValue( thisObject, ( ulong )o[ m.Name ] ) ;
								}
								else
								if( fieldType == typeof( float ) )
								{
									// フロート型のスカラー値
									thisType.GetField( m.Name, flags ).SetValue( thisObject, ( float )o[ m.Name ] ) ;
								}
								else
								if( fieldType == typeof( double ) )
								{
									// ダブル型のスカラー値
									thisType.GetField( m.Name, flags ).SetValue( thisObject, ( double )o[ m.Name ] ) ;
								}
								else
								if( fieldType == typeof( string ) )
								{
									// ストリング型のスカラー値
									thisType.GetField( m.Name, flags ).SetValue( thisObject, ( string )o[ m.Name ] ) ;
								}
							}
						}
					}
					else
					{
						// アレイ型

						// １レコードのキーを全て取得する
						int i, l = o.Count ;
		
						string[] keys = new string[ l ] ;
		
						o.Keys.CopyTo( keys, 0 ) ;

						List<string> arrayKey = new List<string>() ;

						// キーの配列名を持つものを挙列する
						int p, s, e, c ;
						for( i  = 0 ; i <  l ; i ++ )
						{
							if( keys[ i ].IndexOf( m.Name ) >= 0 )
							{
								// 配列名をキー名の一部にものものを発見した
								arrayKey.Add( keys[ i ] ) ;
							}
						}

						if( arrayKey.Count >  0 )
						{
							// １つ以上あり

							string k ;
							l = arrayKey.Count ;
							int[] indices = new int[ l ] ;
							for( i  = 0 ; i <  l ; i ++ )
							{
								indices[ i ] = -1 ;
							}

							c = 0 ;
							for( i  = 0 ; i <  l ; i ++ )
							{
								k = arrayKey[ i ].Replace( " ", "" ) ;

								// 配列の添字で最も大きい値を取得する
								s = k.IndexOf( "[" ) ;
								e = k.IndexOf( "]" ) ;
								if( s >= 0 && e >= 0 )
								{
									k = k.Substring( s + 1, ( e - s - 1 ) ) ;
									if( k.Length >  0 )
									{
										p = int.Parse( k ) ;
										indices[ i ] = p ;
										if( p >  c )
										{
											c  = p ;
										}
									}
								}
							}

							// アレイの型を取得する
							Type elementType = fieldType.GetElementType() ;
					
							l = c + 1 ;
						
							Array array = Array.CreateInstance( elementType, l ) ;
					
							// 要素の値数分ループ
							for( i  = 0 ; i <  arrayKey.Count ; i ++ )
							{
								if( indices[ i ] >= 0 )
								{
									// エレメントタイプ（アレイの型）の種類に応じて値を格納する
									if( elementType.IsEnum == true )
									{
										// 列挙子
										array.SetValue( Enum.ToObject( fieldType, o[ arrayKey[ i ] ] ), indices[ i ] ) ;
									}
									else
									{
										if( elementType == typeof( bool ) ) 
										{
											array.SetValue( ( bool    )o[ arrayKey[ i ] ], indices[ i ] ) ;
										}
										else
										if( elementType == typeof( byte ) )
										{
											array.SetValue( ( byte    )o[ arrayKey[ i ] ], indices[ i ] ) ;
										}
										else
										if( elementType == typeof( char ) )
										{
											array.SetValue( ( char    )o[ arrayKey[ i ] ], indices[ i ] ) ;
										}
										else
										if( elementType == typeof( short ) )
										{
											array.SetValue( ( short   )o[ arrayKey[ i ] ], indices[ i ] ) ;
										}
										else
										if( elementType == typeof( ushort ) )
										{
											array.SetValue( ( ushort  )o[ arrayKey[ i ] ], indices[ i ] ) ;
										}
										else
										if( elementType == typeof( int ) )
										{
											array.SetValue( ( int    )o[ arrayKey[ i ] ], indices[ i ] ) ;
										}
										else
										if( elementType == typeof( uint ) )
										{
											array.SetValue( ( uint   )o[ arrayKey[ i ] ], indices[ i ] ) ;
										}
										else
										if( elementType == typeof( long ) )
										{
											array.SetValue( ( long   )o[ arrayKey[ i ] ], indices[ i ] ) ;
										}
										else
										if( elementType == typeof( ulong ) )
										{
											array.SetValue( ( ulong  )o[ arrayKey[ i ] ], indices[ i ] ) ;
										}
										else
										if( elementType == typeof( float ) )
										{
											array.SetValue( ( float  )o[ arrayKey[ i ] ], indices[ i ] ) ;
										}
										else
										if( elementType == typeof( double ) )
										{
											array.SetValue( ( double )o[ arrayKey[ i ] ], indices[ i ] ) ;
										}
										else
										if( elementType == typeof( string ) )
										{
											array.SetValue( ( string )o[ arrayKey[ i ] ], indices[ i ] ) ;
										}
									}
								}
							}

							// 最後に配列全体をメンバにセットする
							thisType.GetField( m.Name, flags ).SetValue( thisObject, array ) ;
						}
					}
				}
			}

			return true ;
		}
	}
}
