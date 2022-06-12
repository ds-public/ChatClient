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
		/// ダウンロード対象の総サイズを取得する
		/// </summary>
		/// <param name="targetAssetBundlePaths"></param>
		/// <returns></returns>
		public static long GetTotalDownloadSize( Dictionary<string,AssetBundleManager.DownloadEntity> targetAssetBundlePaths = null )
		{
			if( targetAssetBundlePaths == null )
			{
				targetAssetBundlePaths = GetTargetAssetBundlePaths() ;
			}

			if( targetAssetBundlePaths == null || targetAssetBundlePaths.Count == 0 )
			{
				return 0 ;
			}

			//----------------------------------------------------------

			// 既に重複は除去された状態になっている(なっていなければならない)
			AssetBundleManager.DownloadEntity[] targets = targetAssetBundlePaths.Values.ToArray() ;

			int i, l = targets.Length ;

			// トータルバイトサイズを算出する
			long totalSize = 0 ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				totalSize += ( long )Asset.GetSize( targets[ i ].Path ) ;
			}

			return totalSize ;
		}


		/// <summary>
		/// 指定したアセットバンドルのダウンロードを行う
		/// </summary>
		/// <returns></returns>
		public static async UniTask DownloadAssetBundleAsync( ExMonoBehaviour instance, Dictionary<string,AssetBundleManager.DownloadEntity> targetAssetBundlePaths = null, Action<long,int,long,int,int,AssetBundleManager.DownloadEntity[],int,int,int> onProgress = null, Action onCompleted = null )
		{
			bool	useParallelDownload			= true ;	// 並列ダウンロード有効
			int		maxParallelDownloadCount	= 6 ;		// 並列ダウンロード最大
			Settings settings = ApplicationManager.LoadSettings() ;
			if( settings != null )
			{
				useParallelDownload			= settings.UseParallelDownload ;
				maxParallelDownloadCount	= settings.MaxParallelDownloadCount ;
			}

			//----------------------------------------------------------

			if( targetAssetBundlePaths == null )
			{
				targetAssetBundlePaths = GetTargetAssetBundlePaths() ;
			}

			if( targetAssetBundlePaths == null || targetAssetBundlePaths.Count == 0 )
			{
				onCompleted?.Invoke() ;
				return ;
			}

			//----------------------------------------------------------

			// 既に重複は除去された状態になっている(なっていなければならない)
			AssetBundleManager.DownloadEntity[] targets = targetAssetBundlePaths.Values.ToArray() ;

			int i, l = targets.Length ;

			// トータルバイトサイズを算出する
			long totalSize = 0 ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				totalSize += Asset.GetSize( targets[ i ].Path ) ;
			}

			//------------------------------------------------------------------------------------------

			int httpVersion = AssetBundleManager.GetHttpVersion() ;

			// HTTP/2.0 想定で 16 を設定しておく(HTTP/1.1 でしか通信できない場合は自動的に 6 に下がる)
			int parallel = maxParallelDownloadCount ;

			if( useParallelDownload == true )
			{
				int maxParallel = AssetBundleManager.GetMaxParallel() ;
				if( parallel >  maxParallel )
				{
					parallel  = maxParallel ;
				}
			}
			else
			{
				// 並列ダウンロードは行わない
				parallel = 1 ;
			}

			// 複数のアセットバンドルをまとめてダウンロードする(エラーが発生した場合は内部でリトライを行う)
			await Asset.DownloadAssetBundlesAsync
			(
				targets,
				parallel,	// 並列ダウンロード数
				// １ファイルダウンロード最中のプログレス表示更新
				onProgress:( long downloadedSize, int writtenSize, int storedFile, int nowParallel ) =>
				{
					onProgress?.Invoke
					(
						downloadedSize, writtenSize, totalSize,
						storedFile, l,
						targets,
						nowParallel, parallel,
						httpVersion
					) ;
				}
			) ;

			// 完了
			onCompleted?.Invoke() ;
		}

	}
}
