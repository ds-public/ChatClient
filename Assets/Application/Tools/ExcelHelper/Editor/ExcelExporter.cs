//#define ENV
//#define CRLF
#define LF

using System ;
using System.IO ;
using System.Text ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;
using UnityEditor ;

using NPOI.HSSF.UserModel ;
using NPOI.XSSF.UserModel ;
using NPOI.SS.UserModel ;

namespace Tools.ForExcel
{
	/// <summary>
	/// Excel Exporter Version 2022/01/03 0
	/// </summary>
	public class ExcelExporter : AssetPostprocessor
	{
#if ENV
		private static readonly string m_ReturnCode = Environment.NewLine ;
#endif
#if CRLF
		private static readonly string m_ReturnCode = "\x0D\x0A" ;
#endif
#if LF
		private static readonly string m_ReturnCode = "\x0A" ;
#endif

		private const string m_SymbolSgheetName = "@Export" ;

		public static void OnPostprocessAllAssets( string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths )
		{
//			Debug.LogWarning( "何らかのファイルがインポートされた" ) ;

			foreach( string importedAsset in importedAssets )
			{
				if( importedAsset.EndsWith( ".xls" ) == true )
				{
					ReadXLS( importedAsset ) ;
				}
				else
				if( importedAsset.EndsWith( ".xlsx" ) == true )
				{
					ReadXLSX( importedAsset ) ;
				}
			}
		}

		// .xls タイプのファイルを開く
		private static void ReadXLS( string path )
		{
			int i = path.LastIndexOf( '/' ) + 1 ;
			if( path.IndexOf( "~$" ) == i )
			{
//				Debug.LogWarning( "バックアップファイルは対象外:" + path ) ;
				return ;
			}

//			Debug.LogWarning( "-------------------------ＸＬＳが読み出される:" + path ) ;
			using( FileStream fs = new FileStream( path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) )
			{
//				Debug.Log( "ReadXLS:" + path ) ;

				ReadBook( path, new HSSFWorkbook( fs ) );
			}
		}

		// .xlsx タイプのファイルを開く
		private static void ReadXLSX( string path )
		{
			int i = path.LastIndexOf( '/' ) + 1 ;
			if( path.IndexOf( "~$" ) == i )
			{
//				Debug.LogWarning( "バックアップファイルは対象外:" + path ) ;
				return ;
			}

//			Debug.LogWarning( "-------------------------ＸＬＳＸが読み出される:" + path ) ;
			using( FileStream fs = new FileStream( path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) )
			{
//				Debug.Log( "ReadXLSX:" + path ) ;

				ReadBook( path, new XSSFWorkbook( fs ) );
			}
		}

		//-------------------------------------------------------------------------------------

		// 各シートを展開・保存する
		private static void ReadBook( string path, IWorkbook book )
		{
			Dictionary<string,string> overwritePath = new Dictionary<string, string>() ;

			string rootPath = GetPath( path, book, ref overwritePath ) ;

			if( string.IsNullOrEmpty( rootPath ) == true )
			{
				return ;	// 必要なシートが存在しない
			}

			//----------------------------------------------------------

			string symbolSheetName = m_SymbolSgheetName.ToLower() ;

			//----------------------------------------------------------

			int numberOfSheets = book.NumberOfSheets ;
			
			ISheet sheet ;
			
			string name, text ;

			int totalExecute = 0 ;

			int sheetIndex, folderIndex ;
			string folderName ;
			for( sheetIndex = 0 ; sheetIndex <  numberOfSheets ; sheetIndex ++ )
			{
				// シート情報を取得する
				sheet = book.GetSheetAt( sheetIndex ) ;
				if( sheet != null )
				{
					name = sheet.SheetName ;

//					Debug.Log( "===============シート名:" + name ) ;

					if( string.IsNullOrEmpty( name ) == false )
					{
						if( name[ 0 ] != '@' && name[ 0 ] != '#' )
						{
							// 無効化シートではない

							text = GetText( sheet ) ;

							if( string.IsNullOrEmpty( name ) == false && string.IsNullOrEmpty( text ) == false && name.ToLower() != symbolSheetName )
							{
								if( overwritePath.ContainsKey( name ) == false || ( overwritePath.ContainsKey( name ) == true && string.IsNullOrEmpty( overwritePath[ name ] ) == true ) )
								{
									path = rootPath + name ;

									if( name.IndexOf( '.' ) <  0 )
									{
										path += ".txt" ;
									}
								}
								else
								{
									path = overwritePath[ name ] ;
									if( path.Length >  0 && path[ path.Length - 1 ] != '/' )
									{
										path = rootPath + path ;
									}
								}

								// パスにフォルダが含まれているかチェックする
								folderIndex = path.LastIndexOf( '/' ) ;
								if( folderIndex >= 0 )
								{
									// フォルダが含まれている
									folderName = path.Substring( 0, folderIndex ) ;

									if( Directory.Exists( folderName ) == false )
									{
										// フォルダを生成する(多階層をまとめて生成出来る)
										Directory.CreateDirectory( folderName ) ;
									}
								}


								// 文字列をバイナリ化する(UTF-8N)
								byte[] data = Encoding.UTF8.GetBytes( text ) ;

								if( data != null && data.Length >= 3 )
								{
									if( data[ 0 ] == ( byte )0xEF && data[ 1 ] == ( byte )0xBB && data[ 2 ] == ( byte )0xBF )
									{
										// 先頭にＢＯＭが付与されていたら削る
										int l = data.Length - 3 ;
										byte[] work = new byte[ l ] ;
										if( l  >  0 )
										{
											Array.Copy( data, 3, work, 0, l ) ;
										}
										data = work ;
									}
								}

								bool execute = true ;

								if( File.Exists( path ) == true )
								{
									// 既にファイルが存在する場合は読み出して比較する
									byte[] work = File.ReadAllBytes( path ) ;
									if( work != null && data.Length == work.Length )
									{
										int i, l = work.Length ;
										for( i  = 0 ; i <  l ; i ++ )
										{
											if( data[ i ] != work[ i ] )
											{
												break ;
											}
										}
										if( i >= l )
										{
											execute = false ;	// 完全に同一ファイルなので書き込みを行わない
										}
									}
								}

								if( execute == true )
								{
									File.WriteAllBytes( path, data ) ;
									Debug.Log( "Sheet Exported : " + name +  " --> " + path ) ;
									totalExecute ++ ;
								}
							}
						}
					}
				}
			}

			if( totalExecute >  0 )
			{
				AssetDatabase.SaveAssets() ;
				AssetDatabase.Refresh() ;

				Debug.Log( "Excel Exporter Finished ... Total number of files : " + totalExecute ) ;
			}
		}


		// 出力先情報を取得する
		private static string GetPath( string path, IWorkbook book, ref Dictionary<string,string> overwritePath )
		{
			string symbolSheetName = m_SymbolSgheetName.ToLower() ;
			string sheetName ;

			//----------------------------------------------------------

			int numberOfSheets = book.NumberOfSheets ;
 
			ISheet sheet = null ;
			
			int sheetIndex ;
			for( sheetIndex = 0 ; sheetIndex <  numberOfSheets ; sheetIndex ++ )
			{
				// シート情報を取得する
				sheet = book.GetSheetAt( sheetIndex ) ;
				
				sheetName = "" ;
				if( string.IsNullOrEmpty( sheet.SheetName ) == false )
				{
					sheetName = sheet.SheetName.ToLower() ;
				}

				if( sheetName == symbolSheetName )
				{
					break ;
				}
			}

			if( sheetIndex >= numberOfSheets )
			{
				return null ;	// 必要なシート情報が取得出来ず
			}

			// デフォルトの出力パス
			string rootPath = path ;
			if( rootPath.Length >  0 && rootPath[ 0 ] == '/' )
			{
				// 先頭にスラッシュがあれば削除する
				rootPath = rootPath.Remove( 0, 1 ) ;
			}

			int index = rootPath.LastIndexOf( '/' ) ;
			if( index >= 0 )
			{
				rootPath = rootPath.Substring( 0, index + 1 ) ;
			}

			//-------------------------------------------------

			// そのシートの最終行
			int lastRowNumber = sheet.LastRowNum ;

			IRow row ;
			int lastCellNumber ;
			ICell cell ;

			if( lastRowNumber >= 0 )
			{
				row = sheet.GetRow( 0 ) ;
				if( row != null )
				{
					lastCellNumber = row.LastCellNum ;
					if( lastCellNumber >= 0 )
					{
						cell = row.GetCell( 0 ) ;
						if( cell != null )
						{
							// ルートパスの指定があった
							rootPath = cell.ToString() ;

							if( rootPath.Length >  0 && rootPath[ 0 ] == '/' )
							{
								// 先頭にスラッシュがあれば削除する
								rootPath = rootPath.Remove( 0, 1 ) ;
							}
						}
					}
				}
			}

			int length = rootPath.Length ;
			if( length >  0 )
			{
				if( rootPath[ length - 1 ] != '/' )
				{
					rootPath += "/" ;
				}
			}
			else
			{
				rootPath = "/" ;
			}

			// 相対パス指定の場合はソースファイルのパスに連結する
			if( rootPath.IndexOf( "./" ) == 0 || rootPath.IndexOf( "../" ) == 0 )
			{
				string basePath = path ;
				int p = basePath.LastIndexOf( '/' ) ;
				if( p >= 0 )
				{
					basePath = basePath.Substring( 0, p ) ;
				}

				int l = rootPath.Length ;
				if( rootPath.IndexOf( "./" ) == 0 )
				{
					rootPath = basePath + rootPath.Substring( 1, l - 1 ) ;
				}
				else
				if( rootPath.IndexOf( "../" ) == 0 )
				{
					p = basePath.LastIndexOf( '/' ) ;
					if( p >= 0 )
					{
						basePath = basePath.Substring( 0, p ) ;
					}
					rootPath = basePath + rootPath.Substring( 2, l - 2 ) ;
				}
			}

			if( lastRowNumber >= 1 )
			{
				// 特殊パス指定あり
				string specificName, specificPath ;

				int rowIndex ;
				for( rowIndex  = 1 ; rowIndex <= lastRowNumber ; rowIndex ++ )
				{
					row = sheet.GetRow( rowIndex ) ;
					if( row != null )
					{
						lastCellNumber = row.LastCellNum ;
						if( lastCellNumber >= 1 )
						{
							specificName = "" ;
							cell = row.GetCell( 0 ) ;
							if( cell != null )
							{
								specificName = cell.ToString() ;
							}

							specificPath = "" ;
							cell = row.GetCell( 1 ) ;
							if( cell != null )
							{
								specificPath = cell.ToString() ;
								length = specificPath.Length ;
								if( length >  0 && specificPath[ length - 1 ] == '/' )
								{
									if( string.IsNullOrEmpty( specificName ) == false )
									{
										specificPath += specificName ;
									}

									if( specificName.IndexOf( '.' ) <  0 )
									{
										specificPath += ".txt" ;

									}
								}
							}

							if( string.IsNullOrEmpty( specificName ) == false && string.IsNullOrEmpty( specificPath ) == false )
							{
								if( overwritePath.ContainsKey( specificName ) == false )
								{
									overwritePath.Add( specificName, specificPath ) ;
								}
							}
						}
					}
				}
			}

			return rootPath ;
		}

		// テキストを取得する
		private static string GetText( ISheet sheet, string splitCode = "\t" )
		{
			string text = "" ;

			int lastRowNumber = sheet.LastRowNum ;
			if( lastRowNumber <  0 )
			{
				return text ;
			}

			if( string.IsNullOrEmpty( splitCode ) == true )
			{
				splitCode = "\t" ;
			}

			IRow row ;
			int lastCellNumber ;
			ICell cell ;

			string word ;
			List<List<string>> table = new List<List<string>>() ;
			List<string> rows ;

			int rowIndex, columnIndex, maxColumn = 0 ;
			bool isAdded ;

			for( rowIndex  = 0 ; rowIndex <= lastRowNumber ; rowIndex ++ )
			{
				// 行ループ
				isAdded = false ;
				row = sheet.GetRow( rowIndex ) ;
				if( row != null )
				{
					lastCellNumber = row.LastCellNum ;
					if( lastCellNumber >= 0 )
					{
						rows = new List<string>() ;

						for( columnIndex  = 0 ; columnIndex <= lastCellNumber ; columnIndex ++ )
						{
							// 列ループ
							cell = row.GetCell( columnIndex ) ;
							if( cell != null )
							{
//								Debug.Log( "Type:" + cell.CellType.ToString() + " " + cell.ToString() ) ;
								if( cell.CellType == CellType.Formula )
								{
									if( cell.CachedFormulaResultType == CellType.Boolean )
									{
										word = cell.BooleanCellValue.ToString() ;
									}
									else
									if( cell.CachedFormulaResultType == CellType.Numeric )
									{
										word = cell.NumericCellValue.ToString() ;
									}
									else
									{
										word = cell.StringCellValue ;
									}
								}
								else
								{
									word = cell.ToString() ;
								}
								word = word.Replace( "\x0A", "\\n" ) ;				// Environment.NewLine Windows 環境は 0x0D 0x0A Machintosh 環境は 0x0A だが、Machintosh 環境でも Excel は 0x0D 0x0A を出力する
								word = word.Replace( "\x0D", "" ) ;					// Machintosh 環境の対策
								rows.Add( word ) ;
							}
							else
							{
								rows.Add( string.Empty ) ;
							}
						}

						// 後ろの空白を全て削る
						while( rows.Count >  0 )
						{
							if( string.IsNullOrEmpty( rows[ rows.Count - 1 ] ) == true )
							{
								rows.RemoveAt( rows.Count - 1 ) ;
							}
							else
							{
								break ;
							}
						}

						if( rows.Count >  0 )
						{
							table.Add( rows ) ;
							isAdded = true ;

							if( rows.Count >  maxColumn )
							{
								maxColumn  = rows.Count ;	// 最も列数が多い行に全体の列数を合わせる
							}
						}
					}
				}

				if( isAdded == false )
				{
					table.Add( null ) ;
				}
			}

			// 末尾から空行を削る
			while( table.Count >  0 )
			{
				int lastIndex = table.Count - 1 ;
				if( table[ lastIndex ] == null )
				{
					table.RemoveAt( lastIndex ) ;
				}
				else
				{
					break ;
				}
			}

			// 実際のデータ化
			for( rowIndex  = 0 ; rowIndex <  table.Count ; rowIndex ++ )
			{
				rows = table[ rowIndex ] ;

				if( rows != null )
				{
					for( columnIndex  = 0 ; columnIndex <  maxColumn ; columnIndex ++ )
					{
						if( columnIndex <  rows.Count )
						{
							text += rows[ columnIndex ] ;
						}
						else
						{
							text += string.Empty ;
						}

						if( columnIndex <  ( maxColumn - 1 ) )
						{
							text += splitCode ;
						}
					}
				}

				text += m_ReturnCode ;	// 改行を環境ごとのものにする(C#での\nはLF=x0A)
			}

			return text ;
		}
	}
}
