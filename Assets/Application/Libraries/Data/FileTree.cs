using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

namespace FileHelper
{
	/// <summary>
	/// ファイルのツリー構造を表現するデータクラス
	/// </summary>
	[SerializeField]
	public class FileTree
	{
		/// <summary>
		/// ファイル・フォルダ名
		/// </summary>
		public string Name ;

		/// <summary>
		/// パス名
		/// </summary>
		public string Path ;

		/// <summary>
		/// 親フォルダ
		/// </summary>
		public FileTree			Parent ;

		/// <summary>
		/// フォルダであるかどうか(null であればファイル)
		/// </summary>
		public List<FileTree>	Child ;

		//-----------------------------------------------------------

		/// <summary>
		/// パスのリストからツリー構造を生成する
		/// </summary>
		/// <param name="Paths"></param>
		/// <returns></returns>
		public static FileTree Make( string[] paths )
		{
			FileTree		root = new FileTree()
			{
				Child = new List<FileTree>()
			} ;

			List<FileTree>	list ;
			string			work ;

			string			name ;
			string			path ;
			FileTree		parent ;

			FileTree node ;

			int i, l = paths.Length, j, m ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				work = paths[ i ] ;
				work = work.Replace( "\\", "/" ) ;

				work = work.TrimStart( '/' ) ;
				work = work.TrimEnd( '/' ) ;

				list	= root.Child ;
				path	= "" ;
				parent	= root ;

				while( true )
				{
					int p = work.IndexOf( '/' ) ;
					if( p >= 0 )
					{
						// フォルダ

						name = work.Substring( 0, p ) ;
						p ++ ;
						work = work.Substring( p, work.Length - p ) ;

						// フォルダの場合は既に登録済みが確認する
						m = list.Count ;
						for( j  = 0 ; j <  m ; j ++ )
						{
							if( list[ j ].Name == name && list[ j ].Child != null )
							{
								// 既に登録済み
								break ;
							}
						}

						path += ( name + "/" ) ;

						if( j >= m )
						{
							// 新規登録
							node = new FileTree()
							{
								Name	= name,
								Path	= path,
								Parent	= parent,
								Child	= new List<FileTree>()
							} ;

							list.Add( node ) ;
						}
						else
						{
							// 既存登録
							node = list[ j ] ;
						}

						parent	= node ;
						list	= node.Child ;
					}
					else
					{
						// ファイル

						name = work ;
						path += name ;

						list.Add( new FileTree()
						{
							Name	= name,
							Path	= path,
							Parent	= parent,
							Child	= null
						} ) ;

						// 次のパスへ
						break ;
					}
				}
			}

			return root ;
		}
	}
}

