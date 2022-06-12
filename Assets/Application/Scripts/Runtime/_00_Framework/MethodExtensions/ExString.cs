using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;
using System.Text ;
using UnityEngine ;

namespace Template
{
	/// <summary>
	/// String 型のメソッド拡張 Version 2022/05/10
	/// </summary>
	public static class ExString
	{
		/// <summary>
		/// 配列が null もしくは要素数が 0 の時に true を返す
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static bool IsNullOrEmpty( this String s )
		{
			return string.IsNullOrEmpty( s ) ;
		}

		/// <summary>
		/// 文字列中の半角数値を全角数値に置き換える
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static string ToLarge( this string s, bool isFull = false )
		{
			if( string.IsNullOrEmpty( s ) == true )
			{
				return "" ;
			}

			int i, l = s.Length ;

			char[] code = new char[ l ] ;

			char c ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				c = s[ i ] ;

				if( isFull == false )
				{
					// 数値・文字
					if( c >= '0' && c <= '9' )
					{
						c = ( char )( ( int )c - ( int )'0' + ( int )'０' ) ;
					}
					else
					if( c >= 'a' && c <= 'z' )
					{
						c = ( char )( ( int )c - ( int )'a' + ( int )'ａ' ) ;
					}
					else
					if( c >= 'A' && c <= 'Z' )
					{
						c = ( char )( ( int )c - ( int )'A' + ( int )'Ａ' ) ;
					}
				}
				else
				{
					// 全て(全角スペースはルールと違うので注意)
					if( c >= '!' && c <= '~' )
					{
						c = ( char )( ( int )c - ( int )'!' + ( int )'！' ) ;
					}
				}
				
				code[ i ] = c ;
			}

			return new string( code ) ;
		}

		/// <summary>
		/// 文字列中の半角数値を全角数値に置き換える
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static string ToSmall( this string s, bool isFull = false )
		{
			if( string.IsNullOrEmpty( s ) == true )
			{
				return "" ;
			}

			int i, l = s.Length ;

			char[] code = new char[ l ] ;

			char c ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				c = s[ i ] ;

				if( isFull == false )
				{
					// 数値・文字
					if( c >= '０' && c <= '９' )
					{
						c = ( char )( ( int )c - ( int )'０' + ( int )'0' ) ;
					}
					else
					if( c >= 'ａ' && c <= 'ｚ' )
					{
						c = ( char )( ( int )c - ( int )'ａ' + ( int )'a' ) ;
					}
					else
					if( c >= 'Ａ' && c <= 'Ｚ' )
					{
						c = ( char )( ( int )c - ( int )'Ａ' + ( int )'A' ) ;
					}
				}
				else
				{
					// 全て(全角スペースはルールと違うので注意)
					if( c >= '！' && c <= '～' )
					{
						c = ( char )( ( int )c - ( int )'！' + ( int )' ' ) ;
					}
				}
				
				code[ i ] = c ;
			}

			return new string( code ) ;
		}


		/// <summary>
		/// 同じ長さの全て同じ文字の文字列に変換する
		/// </summary>
		/// <returns>The secret.</returns>
		/// <param name="s">S.</param>
		/// <param name="c">C.</param>
		public static string ToSecret( this string s, char c )
		{
			if( string.IsNullOrEmpty( s ) == true )
			{
				return s ;
			}
			
			int i, l = s.Length ;
			char[] a = new char[ l ] ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				a[ i ] = c ;
			}
			
			return new string( a ) ;
		}

		/// <summary>
		/// スネークをキャメルに変換する
		/// </summary>
		/// <param name="self">string自身のインスタンス</param>
		/// <param name="isUpper">アッパーキャメルにするかどうか</param>
		/// <returns></returns>
		public static string ToCamel( this string self, bool isUpper = true )
		{
			if( string.IsNullOrEmpty( self ) == true )
			{
				return self ;
			}

			StringBuilder sb = new StringBuilder() ;
			bool atFirst = false ;
			bool wordStarted = false ;

			foreach( char c in self )
			{
				if( atFirst == false )
				{
					// まだ最初の文字が見つかっていない
					if( c == '_' )
					{
						// _ はそのまま使用
						sb.Append( c ) ;
					}
					else
					{
						// 最初の文字が見つかった
						if( isUpper == true )
						{
							// アッパーキャメル
							sb.Append( char.ToUpperInvariant( c ) ) ;
						}
						else
						{
							// ローワーキャメル
							sb.Append( char.ToLowerInvariant( c ) ) ;
						}

						// 最初の文字は見つかった
						atFirst = true ;

						// 状態を単語の２文字目以降とする
						wordStarted = true ;
					}
				}
				else
				{
					if( c != '_' )
					{
						// 単語の区切りではない
						if( wordStarted == false )
						{
							// 単語の１文字目

							// 必ず大文字
							sb.Append( char.ToUpperInvariant( c ) ) ;

							// 状態を単語の２文字目以降とする
							wordStarted = true ;
						}
						else
						{
							// 単語の２文字目以降

							// そのままコピー
							sb.Append( c ) ;
						}
					}
					else
					{
						// 単語の区切り

						// 単語の位置をリセット
						wordStarted = false ;
					}
				}
			}

			return sb.ToString() ;
		}

		/// <summary>
		/// キャメルをスネークに変換する
		/// </summary>
		/// <param name="self">string自身のインスタンス</param>
		/// <returns></returns>
		public static string ToSnake( this string self )
		{
			if( string.IsNullOrEmpty( self ) == true )
			{
				return self ;
			}

			StringBuilder sb = new StringBuilder() ;
			bool atFirst = false ;
			bool wordStarted = false ;

			foreach( char c in self )
			{
				if( atFirst == false )
				{
					// まだ最初の文字が見つかっていない
					if( c == '_' )
					{
						// _ はそのまま使用
						sb.Append( c ) ;
					}
					else
					{
						// 最初の文字が見つかった

						// 必ず小文字
						sb.Append( char.ToLowerInvariant( c ) ) ;

						// 最初の文字は見つかった
						atFirst = true ;

						// 状態を単語の２文字目以降とする
						wordStarted = true ;
					}
				}
				else
				{
					if( Char.IsUpper( c ) == false )
					{
						// 小文字または記号(２バイトコード)はそのままコピー
						sb.Append( c ) ;

						// 状態を単語の１文字目にリセットする(大文字が出たら前に区切り記号を挿入)
						wordStarted = false ;
					}
					else
					{
						// 大文字の場合
						if( wordStarted == false )
						{
							// 小文字→大文字なら区切り記号の挿入
							sb.Append( '_' ) ;

							// 状態を単語の２文字目以降とする
							wordStarted = true ;
						}

						// 必ず小文字
						sb.Append( char.ToLowerInvariant( c ) ) ;
					}
				}
			}

			return sb.ToString() ;
		}


		/// <summary>
		/// 半角単位での長さを取得する
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static int GetWidth( this String s )
		{
			if( string.IsNullOrEmpty( s ) == true )
			{
				return 0 ;
			}

			int i, l = s.Length ;
			char c ;

			int w = 0 ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				c = s[ i ] ;
				if( IsHankaku( c ) == true )
				{
					w += 1 ;
				}
				else
				{
					w += 2 ;
				}
			}

			return w ;
		}

		//-------------------------------------------------------------------------------------------

		public class CodeData
		{
			public char		Code ;
			public string	ColorValue ;
			public string	SizeValue ;
			public string	TimeValue ;
			public string	WaitValue ;
		}

		/// <summary>
		/// 指定した半角文字数で分割する
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static string[] Split( this String s, int length, bool isRichText = true )
		{
			if( string.IsNullOrEmpty( s ) == true )
			{
				return new string[]{ s } ;
			}

			s = s.Replace( "\\n", "\n" ) ;

			//----------------------------------

			List<string> lines = new List<string>() ;

			//----------------------------------------------------------

			if( isRichText == false )
			{
				// タグ無効

				string[] texts = s.Split( '\n' ) ;

				foreach( var text in texts )
				{
					int i, l = text.Length, o, c, w ;

					o = 0 ;
					c = 0 ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						if( IsHankaku( text[ i ] ) == true )
						{
							w = 1 ;	// 半角
						}
						else
						{
							w = 2 ;	// 全角
						}

						if( ( c + w ) >  length )
						{
							// オーバーする
							lines.Add( text.Substring( o, i - o ) ) ;
							
							o = i ;
							c = 0 ;
						}

						c += w ;
					}

					if( o <  l )
					{
						lines.Add( text.Substring( o, l - o ) ) ;
					}
				}
			}
			else
			{
				// タグ有効

				// 文字単位にバラす
				List<List<CodeData>>	lineCodes = new List<List<CodeData>>() ;
				List<CodeData> codes ;

				bool isControl ;

				char code ;

				string colorValue	= null ;
				string sizeValue	= null ;
				string timeValue	= null ;
				string timeStore ;
				string waitValue	= null ;

				int i, l = s.Length, c, w ;

				int count = 0 ;	// 文字数のカウント

				codes = new List<CodeData>() ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					code = s[ i ] ;

					isControl = false ;
					if( code == '<' )
					{
						// カラー判定
						if( colorValue == null )
						{
							string v = IsTagStart( s, ref i, false, "<color=", "<c=", "<C=" ) ;
							if( string.IsNullOrEmpty( v ) == false )
							{
								colorValue = v ;
								isControl = true ;
							}
						}
						else
						{
							if( IsTagEnd( s, ref i, "</color>", "</c>", "</C>" ) == true )
							{
								colorValue = null ;
								isControl = true ;
							}
						}

						// サイズ判定
						if( sizeValue == null )
						{
							string v = IsTagStart( s, ref i, false, "<size=", "<s=", "<S=" ) ;
							if( string.IsNullOrEmpty( v ) == false )
							{
								sizeValue = v ;
								isControl = true ;
							}
						}
						else
						{
							if( IsTagEnd( s, ref i, "</size>", "</s>", "</S>" ) == true )
							{
								sizeValue = null ;
								isControl = true ;
							}
						}

						// タイム判定
						timeStore = IsTagStart( s, ref i, true, "<time=", "<t=" ) ;
						if( string.IsNullOrEmpty( timeStore ) == false )
						{
							timeValue = timeStore ;
							isControl = true ;
						}

						// ウェイト判定
						waitValue = IsTagStart( s, ref i, true, "<wait=", "<w=" ) ;
						if( string.IsNullOrEmpty( waitValue ) == false )
						{
							isControl = true ;
						}
					}
					else
					if( code == '\n' )
					{
						// １行分完了
						lineCodes.Add( codes ) ;

						codes = new List<CodeData>() ;
						count = 0 ;

						isControl = true ;
					}

					if( isControl == false )
					{
						// コントロールではない
						if( ( int )code >= 0x20 )
						{
							codes.Add( new CodeData()
							{
								Code		= code,
								ColorValue	= colorValue,
								SizeValue	= sizeValue,
								TimeValue	= timeValue,
							} ) ;

							// 文カウントアップ
							count ++ ;
						}
					}
					else
					{
						// コントロール
						if( string.IsNullOrEmpty( waitValue ) == false )
						{
							// ウェイト指定がある(一番最初につは付けられない)
							if( count >  0 )
							{
								codes[ count - 1 ].WaitValue = waitValue ;
							}

							waitValue = null ;
						}
					}
				}

				if( codes.Count >  0 )
				{
					lineCodes.Add( codes ) ;
				}

				//---------------------------------------------------------
				// 再び文字列を生成する

				string text ;
				CodeData cd ;

				//---------------------------------

				int hi, hl = lineCodes.Count ;
				for( hi  = 0 ; hi <  hl ; hi ++ )
				{
					codes = lineCodes[ hi ] ;

					//------------

					text = string.Empty ;

					colorValue	= null ;
					sizeValue	= null ;
					timeValue	= null ;

					c = 0 ;

					//------------

					l = codes.Count ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						cd = codes[ i ] ;

						//-------------------------------

						if( IsHankaku( cd.Code ) == true )
						{
							w = 1 ;	// 半角
						}
						else
						{
							w = 2 ;	// 全角
						}

						if( ( c + w ) >  length )
						{
							// オーバーする

							// サイズ終了
							if( sizeValue != null )
							{
								text += "</size>" ;
							}

							// カラー終了
							if( colorValue != null )
							{
								text += "</color>" ;
							}

							//----------

							lines.Add( text ) ;

							//----------

							text = string.Empty ;

							colorValue	= null ;
							sizeValue	= null ;
							timeValue	= null ;

							c = 0 ;
						}
						else
						{
							c += w ;
						}

						//-------------------------------------------------------

						// サイズ終了
						if( cd.SizeValue != sizeValue )
						{
							if( sizeValue != null )
							{
								text += "</size>" ;
							}
						}

						// カラー終了
						if( cd.ColorValue != colorValue )
						{
							if( colorValue != null )
							{
								text += "</color>" ;
							}
						}

						//-------------------------------

						// カラー開始
						if( cd.ColorValue != colorValue )
						{
							if( cd.ColorValue != null )
							{
								text += "<color=" + cd.ColorValue + ">" ;
							}
						}

						// サイズ開始
						if( cd.SizeValue != sizeValue )
						{
							if( cd.SizeValue != null )
							{
								text += "<size=" + cd.SizeValue + ">" ;
							}
						}

						// タイム挿入
						if( cd.TimeValue != timeValue )
						{
							if( cd.TimeValue != null )
							{
								text += "<time=" + cd.TimeValue + "/>" ;
							}
						}

						//-------------------------------------------------------

						// カラー更新
						colorValue	= cd.ColorValue ;

						// サイズ更新
						sizeValue	= cd.SizeValue ;

						// タイム更新
						timeValue	= cd.TimeValue ;

						//-------------------------------------------------------

						// 文字追加
						text += cd.Code ;

						// ウェイト挿入
						if( cd.WaitValue != null )
						{
							text += "<wait=" + cd.WaitValue + "/>" ;
						}
					}

					//--------------------------------
					// 改行まで終了

					// サイズ終了
					if( sizeValue != null )
					{
						text += "</size>" ;
					}

					// カラー終了
					if( colorValue != null )
					{
						text += "</color>" ;
					}

					if( string.IsNullOrEmpty( text ) == false )
					{
						lines.Add( text ) ;
					}
				}
			}

			//----------------------------------------------------------

			return lines.ToArray() ;
		}

		// タグかどうか判定し内容を取得する
		private static string IsTagStart( string s, ref int o, bool isClosed, params string[] labels )
		{
			int i, p, m ;
			int l = s.Length ;
			string value ;

			int k, n = labels.Length ;
			string label ;

			for( k  = 0 ; k <  n ; k ++ )
			{
				label = labels[ k ] ;
				m = label.Length ;

				p = o ;
				if( ( p + m ) <  l && s.Substring( p, m ).ToLower() == label )
				{
					// 値を取得する
					p += m ;

					for( i  = p ; i <  l ; i ++ )
					{
						if( s[ i ] == '>' )
						{
							// 決定
							o = i ;
							value = s.Substring( p, i - p ) ;

							if( string.IsNullOrEmpty( value ) == false )
							{
								return value ;
							}
							else
							{
								return string.Empty ;	// 値が空文字
							}
						}
						else
						if( isClosed == true )
						{
							if( s[ i ] == '/' && ( ( i + 1 ) <  l ) && s[ i + 1 ] == '>' )
							{
								// 決定
								o = i + 1 ;
								value = s.Substring( p, i - p ) ;

								if( string.IsNullOrEmpty( value ) == false )
								{
									return value ;
								}
								else
								{
									return string.Empty ;	// 値が空文字
								}

							}
						}
					}
				}
			}

			// いずれにも該当しなかった
			return null ;
		}

		// タグかどうか判定し内容を取得する
		private static bool IsTagEnd( string s, ref int o, params string[] labels )
		{
			int m ;
			int l = s.Length ;

			int k, n = labels.Length ;
			string label ;

			for( k  = 0 ; k <  n ; k ++ )
			{
				label = labels[ k ] ;
				m = label.Length ;

				if( ( o + m ) <= l && s.Substring( o, m ).ToLower() == label )
				{
					o += m - 1 ;
					return true ;
				}
			}

			return false ;
		}





		// 文字が半角かどうかの判定
		private static bool IsHankaku( char code )
		{
			if
			(
				  code <  0x007E ||							// 英数字
				  code == 0x00A5 ||							// 記号 \
				  code == 0x203E ||							// 記号 ~
				( code >= 0xFF61 && code <= 0xFF9F )			// 半角カナ
			)
			{
				// 半角
				return true ;
			}

			// 全角
			return false ;
		}


		/// <summary>
		/// 数値をコンピューターのサイズ表記に変換する
		/// </summary>
		/// <param name="size"></param>
		/// <returns></returns>
		public static string GetSizeName( long size )
		{
			string sizeName = "Value Overflow" ;

			if( size <  1024L )
			{
				sizeName = size + " byte" ;
			}
			else
			if( size <  ( 1024L * 1024L ) )
			{
				sizeName = ( size / 1024L ) + " KB" ;
			}
			else
			if( size <  ( 1024L * 1024L * 1024L ) )
			{
				sizeName = ( size / ( 1024L * 1024L ) ) + " MB" ;
			}
			else
			if( size <  ( 1024L * 1024L * 1024L * 1024L ) )
			{
				double value = ( double )size / ( double )( 1024L * 1024L * 1024L ) ;
				value = ( double )( ( int )( value * 1000 ) ) / 1000 ;	// 少数までわかるようにする
				sizeName = value + " GB" ;
			}
			else
			if( size <  ( 1024L * 1024L * 1024L * 1024L * 1024L ) )
			{
				sizeName = ( size / ( 1024L * 1024L * 1024L * 1024L ) ) + " TB" ;
			}
			else
			if( size <  ( 1024L * 1024L * 1024L * 1024L * 1024L * 1024L ) )
			{
				sizeName = ( size / ( 1024L * 1024L * 1024L * 1024L * 1024L ) ) + " PB" ;
			}

			return sizeName ;
		}
	}
}
