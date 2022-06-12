using System ;
using System.Collections ;
using System.Collections.Generic ;

using Cysharp.Threading.Tasks ;

using UnityEngine ;

using uGUIHelper ;
using System.Linq ;

using AudioHelper ;
using AssetBundleHelper ;
using StorageHelper ;

using TimeHelper ;

using Template.Screens.DownloadingClasses.UI ;

namespace Template.Screens
{
	/// <summary>
	/// 起動直後のダウンロード処理(マスターデータ・アセットバンドルの更新)
	/// </summary>
	public partial class Downloading
	{
		/// <summary>
		/// 更新が必要なアセットバンドルのパスを取得する
		/// </summary>
		/// <returns></returns>
		private static Dictionary<string,AssetBundleManager.DownloadEntity> GetTargetAssetBundlePaths()
		{
			// 事前ダウンロードを行うアセットバンドルパスを追加する
			Dictionary<string,AssetBundleManager.DownloadEntity> targetAssetBundlePaths = new Dictionary<string, AssetBundleManager.DownloadEntity>() ;

			// 更新が必要なファイルのパスのみ取得する
			string[] paths = AssetBundleManager.GetAllAssetBundlePaths( true ) ;
			if( paths == null || paths.Length == 0 )
			{
				return null ;
			}

			foreach( var path in paths )
			{
				int size = AssetBundleManager.GetSize( path ) ;

				targetAssetBundlePaths.Add( path, new AssetBundleManager.DownloadEntity(){ Path = path, Keep = false } ) ;
			}

			//----------------------------------------------------------

			return targetAssetBundlePaths ;
		}

#if false
		/// <summary>
		/// 更新が必要なアセットバンドルのパスを取得する
		/// </summary>
		/// <returns></returns>
		private static Dictionary<string,AssetBundleManager.DownloadEntity> GetTargetAssetBundlePaths( string manifestName, ( string, bool )[] folders, ( string, bool )[] files, ref Dictionary<string,AssetBundleManager.DownloadEntity> targetAssetBundlePaths )
		{
			// タイトル後に一括ダウンロードの対象となるファイルに限ってのみピックアップする
			string[] paths ;

			// 注意:Use Local Assests が有効な場合は、マニフェスト情報のダウンロードと展開を行わないため、一切のアセットバンドル情報が取得できない。

			if( string.IsNullOrEmpty( manifestName ) == true )
			{
				// Manifest : Defualt
				paths = AssetBundleManager.GetAllAssetBundlePaths( true ) ;
			}
			else
			{
				// Manefest : manifestName
				paths = AssetBundleManager.GetAllAssetBundlePaths( manifestName, true ) ;
				manifestName +="|" ;
			}

			if( paths != null && paths.Length >  0 )
			{
				// 更新が必要なファイルはある

				if( folders != null && folders.Length >  0 )
				{
					foreach( var ( folder, keep ) in folders )
					{
						string folderName = folder.ToLower() ;	// アセットバンドルパスは全て小文字になっている
						string[] pickupPaths = paths.Where( _ => _.IndexOf( folderName ) == 0 ).ToArray() ;
						if( pickupPaths != null && pickupPaths.Length >  0 )
						{
							foreach( var path in pickupPaths )
							{
								if( targetAssetBundlePaths.ContainsKey( manifestName + path ) == false )
								{
									// 重複は禁止
									targetAssetBundlePaths.Add( manifestName + path, new AssetBundleManager.DownloadEntity(){ Path = manifestName + path, Keep = keep } ) ;
								}
							}
						}
					}
				}

				if( files != null && files.Length >  0 )
				{
					foreach( var ( file, keep ) in files )
					{
						string fileName = file.ToLower() ;
						string pickupPath = paths.FirstOrDefault( _ => _ == fileName ) ;
						if( string.IsNullOrEmpty( pickupPath ) == false )
						{
							if( targetAssetBundlePaths.ContainsKey( manifestName + pickupPath ) == false )
							{
								// 重複は禁止
								targetAssetBundlePaths.Add( manifestName + pickupPath, new AssetBundleManager.DownloadEntity(){ Path = manifestName + pickupPath, Keep = keep } ) ;
							}
						}
					}
				}
			}

			return targetAssetBundlePaths ;
		}
#endif

		//-------------------------------------------------------------------------------------------
	}
}
