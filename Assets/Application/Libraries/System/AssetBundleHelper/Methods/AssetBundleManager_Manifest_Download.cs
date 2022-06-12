using System ;
using System.Text ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using UnityEngine ;
using UnityEngine.Networking ;

using StorageHelper ;

/// <summary>
/// アセットバンドルヘルパーパッケージ
/// </summary>
namespace AssetBundleHelper
{
	/// <summary>
	/// アセットバンドルマネージャクラス(メソッド)
	/// </summary>
	public partial class AssetBundleManager
	{
		/// <summary>
		/// マニフェスト情報クラス
		/// </summary>
		public partial class ManifestInfo
		{
			// ローカルにアセットバンドルが存在しない場合にリモートから取得しローカルに保存する
			private IEnumerator LoadAssetBundleFromRemote_Coroutine( AssetBundleInfo assetBundleInfo, LocationTypes locationType, bool keep, Action<float,float> onProgress, Action<string> onResult, bool isManifestSaving, Request request, AssetBundleManager instance )
			{
				// ダウンロード中の数をカウントする
				DownloadingCount ++ ;

				//---------------------------------

				// アセットバンドルのファイルサイズ(ただし CRC ファイルがない場合は 0 にになっている)
				int assetBundleFileSize = assetBundleInfo.Size ;

				if( request != null )
				{
					request.EntireDataSize = assetBundleFileSize ;
					request.EntireFileCount = 1 ;
				}

				string path ;
				byte[] data = null ;

				//-----------------------------------------------------------------------------------------

				// ストリーミングアセッツに配置されたアセットバンドルをロードする
				if
				(
					data == null &&
					string.IsNullOrEmpty( StreamingAssetsRootPath ) == false &&
					(
						locationType == LocationTypes.StreamingAssets ||
						( locationType == LocationTypes.StorageAndStreamingAssets && assetBundleInfo.LocationPriority == LocationPriorities.StreamingAssets )
					)
				)
				{
#if UNITY_EDITOR
					// ログの出力
					if( AssetBundleManager.LogEnabled == true )
					{
						string logMessage = "<color=#FFFF00>[Download(StreamingAssets)] " + assetBundleInfo.Path + " (" + instance.GetSizeName( assetBundleInfo.Size ) + ")</color>" ;
						logMessage += "\n" + StreamingAssetsRootPath + assetBundleInfo.Path ;
						Debug.Log( logMessage ) ;
					}
#endif
					//--------------------------------

					// StreamingAssets からダウンロードを試みる
					yield return instance.StartCoroutine( StorageAccessor.LoadFromStreamingAssetsAsync( StreamingAssetsRootPath + assetBundleInfo.Path, ( _1, _2 ) => { data = _1 ; } ) ) ;
					if( data != null && data.Length >  0 )
					{
						onProgress?.Invoke( 1.0f, 0.0f ) ;

						if( request != null )
						{
							request.StoredDataSize = data.Length ;
							request.StoredFileCount = 1 ;
							request.Progress = 1.0f ;
						}
					}
					else
					{
						// 極めて危険な現象(ストレージからのロードが間に合っていない)
						Debug.LogWarning( "[危険]ストリーミングアセットからのロード失敗 : " + StreamingAssetsRootPath + assetBundleInfo.Path ) ;
					}
				}

				//-----------------------------------------------------------------------------------------

				// リモートストレージに配置されたアセットバンドルをダウンロードする
				if
				(
					data == null &&
					string.IsNullOrEmpty( RemoteAssetBundleRootPath ) == false &&
					string.IsNullOrEmpty( LocalAssetBundleRootPath  ) == true  &&
					(
						locationType == LocationTypes.Storage ||
						( locationType == LocationTypes.StorageAndStreamingAssets && assetBundleInfo.LocationPriority == LocationPriorities.Storage )
					)
				)
				{
#if UNITY_EDITOR
					// ログの出力
					if( AssetBundleManager.LogEnabled == true )
					{
						string logMessage = "<color=#FFFF00>[Download(Remote)] " + assetBundleInfo.Path + " (" + instance.GetSizeName( assetBundleInfo.Size ) + ")</color>" ;
						logMessage += "\n" + RemoteAssetBundleRootPath + assetBundleInfo.Path ;
						Debug.Log( logMessage ) ;
					}
#endif
					//--------------------------------

					// ネットワークからダウンロードを試みる
					path = RemoteAssetBundleRootPath + assetBundleInfo.Path + "?time=" + GetClientTime() ;

					yield return instance.StartCoroutine
					(
						DownloadFromRemote
						(
							path,
							( DownloadStates state, byte[] downloadedData, float progress, long downloadedSize, string errorMessage, int version ) =>
							{
								Progress = progress ;
								onProgress?.Invoke( Progress, 0.0f ) ;

								if( request != null )
								{
									request.StoredDataSize = ( int )downloadedSize ;
									request.Progress = Progress ;
								}

								if( state == DownloadStates.Successed )
								{
									// 成功
									if( request != null )
									{
										request.StoredFileCount = 1 ;
									}
									data = downloadedData ;
								}
								else
								if( state == DownloadStates.Failed )
								{
									// 失敗
									Error = errorMessage ;
								}
							},
							instance
						)
					) ;
				}

				//---------------------------------

#if UNITY_EDITOR
				// ローカルストレージにビルドされたアセットバンドルをロードする
				if
				(
					data == null &&
					string.IsNullOrEmpty( LocalAssetBundleRootPath ) == false &&
					(
						locationType == LocationTypes.Storage ||
						( locationType == LocationTypes.StorageAndStreamingAssets && assetBundleInfo.LocationPriority == LocationPriorities.Storage )
					)
				)
				{
					// ログの出力
					if( AssetBundleManager.LogEnabled == true )
					{
						string logMessage = "<color=#FFFF00>[Download(Local)] " + assetBundleInfo.Path + " (" + instance.GetSizeName( assetBundleInfo.Size ) + ")</color>" ;
						logMessage += "\n" + LocalAssetBundleRootPath + assetBundleInfo.Path ;
						Debug.Log( logMessage ) ;
					}

					//--------------------------------

					// ローカルファイル からダウンロードを試みる
					data = File_Load( LocalAssetBundleRootPath + assetBundleInfo.Path ) ;
				}
#endif

				//-----------------------------------------------------------------------------------------

				if( data == null )
				{
					// 失敗
					DownloadingCount -- ;	// ダウンロード中の数を減少させる

					Error = "Could not load data" ;
					onResult?.Invoke( Error ) ;    // 失敗
					yield break ;
				}

				if( assetBundleInfo.Crc != 0 )
				{
					// ＣＲＣのチェックが必要
//					float tt = UnityEngine.Time.realtimeSinceStartup ;

					uint crc = 0 ;

					if( data.Length <  ( 1024 * 10124 ) )
					{
						// 同スレッドでＣＲＣを計算する
						crc = GetCRC32( data ) ;
					}
					else
					{
						// 別スレッドでＣＲＣを計算する
						yield return instance.StartCoroutine( GetCRC32Async( data, _ => { crc = _ ; }, instance.m_WritingCancellationSource.Token ) ) ;
					}

//					Debug.Log( "<color=#FFFF00>CRC : " + ( Time.realtimeSinceStartup - tt ) + " size = " + data.Length + " path = " + RemoteAssetBundleRootPath + assetBundleInfo.Path + "</color>" + " CRC_0 = " + crc + " CRC_1 = " + assetBundleInfo.Crc ) ;

					if( crc != assetBundleInfo.Crc )
					{
						// 失敗
						Debug.LogWarning( "[AssetBundle Downloading ]Bad CRC : Path = " + RemoteAssetBundleRootPath + assetBundleInfo.Path + " Size = " + data.Length + " Crc[F] = " + crc + " ! Crc[A] = " + assetBundleInfo.Crc ) ;

						DownloadingCount -- ;	// ダウンロード中の数を減少させる

						Error = "Bad CRC" ;
						onResult?.Invoke( Error ) ;    // 失敗
						yield break ;
					}
				}

				// ダウンロード成功

				//---------------------------------------------------------
				// マニフェストの管理ファイルを更新する

				//---------------------------------

				if( CacheSize >  0 )
				{
					// キャッシュサイズ制限があるので足りなければ破棄可能で古いアセットバンドルを破棄する
					if( Cleanup( data.Length ) == false )
					{
						// 空き領域が確保出来ない
						DownloadingCount -- ;	// ダウンロード中の数を減少させる

						Error = "Could not alocate space" ;
						onResult?.Invoke( Error ) ;    // 失敗

						yield break ;
					}
				}

				//-----------------------------------------------------------------------------------------
				// 保存可能な空き領域が存在する

				// セーブはしないがこのファイル分の領域をキャッシュに確保する
				assetBundleInfo.IsDownloading = true ;

				// ダウンロード中のサイズもキャッシュ使用分に含めるためサイズを保存する
				assetBundleInfo.Size = data.Length ;

				// アセットバンドルファイルの保存パス
				string storagePath = StorageCacheRootPath + ManifestName + "/" + assetBundleInfo.Path ;

				//--------------------------------------------------------
				// ここは並列に実行される(ファイルサイズによって完了時間が異なるためマニフェスト更新時はキャッシュ整理時の実行順に合わせるようにする)

				bool result = false ;

				if( instance.m_AsynchronousWritingEnabled == false )
				{
					// 同期保存
					result = StorageAccessor_Save( storagePath, data, true ) ;
					if( result == true )
					{
						onProgress?.Invoke( 1.0f, 1.0f ) ;
					}
				}
				else
				{
					// 非同期保存
					yield return instance.StartCoroutine
					(
						// 非同期でストレージに保存する
						StorageAccessor_SaveAsync
						(
							storagePath,
							data,
							true,
							null,
							null,
							( float writingProgress ) =>
							{
								onProgress?.Invoke( 1.0f, writingProgress ) ;
							},
							( bool _ ) => { result = _ ; },
							instance.m_WritingCancellationSource.Token
						)
					) ;
				}

				//--------------------------------------------------------
				// ストレージへの保存完了

				// ダウンロード中のフラグをクリアする
				assetBundleInfo.IsDownloading = false ;

				//---------------------------------------------------------

				// ファイルを保存しアセットバンドルインフォを更新する
				if( result == true )
				{
					// これまでの処理が成功していれば実行

					assetBundleInfo.Size = data.Length ;		// ＣＲＣファイルが存在しない場合はここで初めてサイズが書き込まれる

					assetBundleInfo.LastUpdateTime = GetClientTime() ;	// タイムスタンプ更新

					assetBundleInfo.Keep = keep ;
					assetBundleInfo.UpdateRequired = false ;

					//--------------------------------
					// まとめてダウンロードの場合は１ファイル単位ではマニフェストのセーブは行わない

					if( isManifestSaving == true )
					{
						// マニフェストをセーブする
						result = Save() ;
					}
				}

				if( result == false )
				{
					// 情報が保存出来ない

					Error = "Could not save" ;
					onResult?.Invoke( Error ) ;    // 失敗
				}

				//---------------------------------------------------------

				DownloadingCount -- ;	// ダウンロード中の数を減少させる

				if( string.IsNullOrEmpty( Error ) == false )
				{
					yield break ;
				}

				// 成功
				onResult?.Invoke( string.Empty ) ;
			}			
		}
	}
}
