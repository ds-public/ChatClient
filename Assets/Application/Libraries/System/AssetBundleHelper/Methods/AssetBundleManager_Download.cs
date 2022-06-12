using System ;
using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;

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
		// 一括ダウンロード中かどうか
		private bool m_IsBlockDownloading = false ;

		//-------------------------------------------------------------------------------------------
		// General

		/// <summary>
		/// 処理中のファイル情報
		/// </summary>
		public class ProcessingEntity
		{
			public	int	DownloadingSize ;
			public	int	WritingSize ;
		}


		/// <summary>
		/// アセットバンドルのダウンロードを行う(非同期)
		/// </summary>
		/// <param name="path">アセットバンドルのパス</param>
		/// <param name="keep">キャッシュオーバー時の動作(true=キャッシュオーバー時に保持する・false=キャッシュオーバー時に破棄する)</param>
		/// <returns>アセットバンドルのダウンロードリクエストクラスのインスタンス</returns>
		public static Request DownloadAssetBundleAsync( string path, bool keep = false, Action<int,float,float> onProgress = null, Action<string> onResult = null )
		{
			if( m_Instance == null )
			{
				// インスタンスが生成されていない
				return null ;
			}

			Request request = new Request( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.DownloadAssetBundleAsync_Private( path, keep, onProgress, onResult, request ) ) ;
			return request ;
		}

		// アセットバンドルのダウンロードを行う
		private IEnumerator DownloadAssetBundleAsync_Private( string path, bool keep, Action<int,float,float> onProgress, Action<string> onResult, Request request )
		{
			string message = string.Empty ;

			if( GetManifestNameAndAssetBundleName( path, out string manifestName, out string assetBundlePath, out _ ) == true )
			{
				if( string.IsNullOrEmpty( manifestName ) == false && m_ManifestHash.ContainsKey( manifestName ) == true )
				{
					if( string.IsNullOrEmpty( assetBundlePath ) == false )
					{
						yield return StartCoroutine( m_ManifestHash[ manifestName ].DownloadAssetBundle_Coroutine
						(
							assetBundlePath,
							keep,
							onProgress,
							( int length, string error ) =>
							{
								message = error ;
								onResult?.Invoke( error ) ;
							},
							true,
							request,
							this
						) ) ;
					}
				}
			}

			if( string.IsNullOrEmpty( message ) == false )
			{
				// エラー発生
				request.Error = message ;
				yield break ;
			}

			request.IsDone = true ;
		}

		/// <summary>
		/// 複数のアセットバンドルのダウンロードを行う(非同期)
		/// </summary>
		/// <param name="path">アセットバンドルのパス</param>
		/// <param name="keep">キャッシュオーバー時の動作(true=キャッシュオーバー時に保持する・false=キャッシュオーバー時に破棄する)</param>
		/// <returns>アセットバンドルのダウンロードリクエストクラスのインスタンス</returns>
		public static Request DownloadAssetBundlesAsync( DownloadEntity[] entities, int parallel = 0, Action<long,int,int,int> onPregress = null, Action<string> onResult = null )
		{
			if( m_Instance == null )
			{
				// インスタンスが生成されていない
				return null ;
			}

			Request request = new Request( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.DownloadAssetBundlesAsync_Private( entities, parallel, onPregress, onResult, request ) ) ;
			return request ;
		}

		// 複数のアセットバンドルのダウンロードを行う
		private IEnumerator DownloadAssetBundlesAsync_Private( DownloadEntity[] entities, int parallel, Action<long,int,int,int> onProgress, Action<string> onResult, Request request )
		{
			if( m_IsBlockDownloading == true )
			{
				Debug.LogWarning( "既に一括ダウンロードが実行されています。一括ダウンロードを複数同時に行う事はできません。" ) ;

				string message = "Already execute DownloadAssetBundlesAsync." ;

				onResult?.Invoke( message ) ;
				request.Error = message ;

				yield break ;
			}

			//----------------------------------------------------------

			// 一括ダウンロード中
			m_IsBlockDownloading = true ;

			//----------------------------------
			// ＨＴＴＰのバージョンによって並列最大数に制限をかける

			int maxParallel = GetMaxParallel_Private() ;
			if( parallel >  maxParallel )
			{
				parallel  = maxParallel ;
			}

#if UNITY_EDITOR
			int httpVersion = GetHttpVersion_Private() ;
			Debug.Log( "<color=#FF7F00>HTTP Version = " + httpVersion + " | 並列ダウンロード最大数:" + parallel + "</color>" ) ;
#endif
			//----------------------------------

			int fileCursor = -1 ;
			int fileAmount = entities.Length ;
			int fileStored = 0 ;
			int totalDownloadingSize = 0 ;
			int totalWritingSize = 0 ;

			//----------------------------------------------------------
			// ダウンロード済みのものを確認する

			int fileOffset ;
			for( fileOffset  = 0 ; fileOffset <  fileAmount ; fileOffset ++ )
			{
				var entity = entities[ fileOffset ] ;

				string	path = entity.Path ;

				if( Exists_Private( path ) == false )
				{
					// 存在しないものを発見
					if( fileCursor <  0 )
					{
						// 最初のダウンロード対象
						fileCursor = fileOffset ;
					}
				}
				else
				{
					// 既にダウンロード済み
					fileStored ++ ;
					int size = GetSize_Private( path ) ;
					totalDownloadingSize	+= size ;
					totalWritingSize		+= size ;
				}
			}

			onProgress?.Invoke( totalDownloadingSize, totalWritingSize, fileStored, 0 ) ;

			//------------------------------------------------------------------------------------------

			if( parallel <= 1 )
			{
				//------------------------------------------------------------------------------------------
				// 直列バージョン

				while( fileStored <  fileAmount )
				{
					if( fileCursor <  fileAmount )
					{
						var entity = entities[ fileCursor ] ;

						string	path = entity.Path ;

						if( Exists_Private( path ) == false )
						{
							bool	keep = entity.Keep ;

							string message = string.Empty ;

							// 直列ダウンロードを実行する
							yield return StartCoroutine( DownloadExecute_Private
							(
								path,
								keep,
								( string _, int length, float downloadingProgress, float writingProgress ) =>
								{
									int downloadingSize = ( int )( length * downloadingProgress	) ;
									int writingSize		= ( int )( length * writingProgress		) ;

									onProgress?.Invoke( totalDownloadingSize + downloadingSize, totalWritingSize + writingSize, fileStored, 1 ) ;
								},
								( string _, int length, string error ) =>
								{
									if( string.IsNullOrEmpty( error ) == true )
									{
										// 成功
										totalDownloadingSize	+= length ;
										totalWritingSize		+= length ;	
										fileStored ++ ;

										onProgress( totalDownloadingSize, totalWritingSize, fileStored, 1 ) ;
									}
									else
									{
										// 失敗
										message = "Can not download files.\n" + path ;
									}
								}
							) ) ;

							if( string.IsNullOrEmpty( message ) == false )
							{
								// エラー発生
								SaveAllManifestInfo_Private() ;	// 全マニフェスト情報を保存する
								m_IsBlockDownloading = false ;	// 一括ダウンロード終了

								onResult?.Invoke( message ) ;
								request.Error = message ;

								yield break ;
							}

	//						Debug.Log( "<color=#FF7F00>並列ダウンロード数:" + entities.Count + "</color>" ) ;
						}
						else
						{
							Debug.LogWarning( "<color=#FF7F00>重複ダウンロード : " + path + "</color>" ) ;
						}

						// ダウンロード対象を次のファイルへ
						fileCursor ++ ;
					}

					yield return null ;
				}
			}
			else
			{
				//------------------------------------------------------------------------------------------
				// 並列バージョン

				// 注意:エラーが発生したら一旦通信中のものが全て終了するのを待って呼び出し元で最初からリトライするかリブートするか判断してもらう(既に通信に成功しているものはダウンロードはスキップされる)

				// ダウンロード中のアセットバンドル情報
				Dictionary<string,ProcessingEntity> processingEntities = new Dictionary<string,ProcessingEntity>() ;

				// 発生中のエラー
				List<string> errors = new List<string>() ;
			
				//----------------------------------------------------------
				// ダウンロードを行う

				while( fileStored <  fileAmount )
				{
					if( fileCursor <  fileAmount && processingEntities.Count <  parallel && errors.Count == 0 )
					{
						// まだ並列ダウンロードは可能なので新たにダウンロード中として登録する
						var entity = entities[ fileCursor ] ;

						string	path = entity.Path ;

						if( Exists_Private( path ) == false && processingEntities.ContainsKey( path ) == false )
						{
							bool	keep = entity.Keep ;

			//				Debug.Log( "<color=#00FF00>ダウンロードする:" + path + " i = " + fileCursor + "</color>" ) ;
							processingEntities.Add( path, new ProcessingEntity() ) ;

							// 並列ダウンロードを実行する
							StartCoroutine( DownloadExecute_Private( path, keep, OnProgress, OnResult ) ) ;
#if UNITY_EDITOR
//							Debug.Log( "<color=#FF7F00>並列ダウンロード数:" + processingEntities.Count + " / " + parallel + "</color>" ) ;
#endif
						}
						else
						{
							Debug.LogWarning( "<color=#FF7F00>重複ダウンロード : " + path + "</color>" ) ;
						}

						// ダウンロード対象を次のファイルへ
						fileCursor ++ ;
					}

					if( errors.Count >  0 )
					{
						// エラー発生中

						// 通信中のものが全て無くなるまで待つ
						if( processingEntities.Count == 0 )
						{
							// 通信中のものが全て無くなった
							string message = "Can not download files.\n" ;
							foreach( var error in errors )
							{
								message += error + "\n" ;
							}

							SaveAllManifestInfo_Private() ;	// 全マニフェスト情報を保存する
							m_IsBlockDownloading = false ;	// 一括ダウンロード終了

							onResult?.Invoke( message ) ;
							request.Error = message ;

							yield break ;
						}
					}

					// １フレーム待ち(これがないとフリーズするので注意)
					yield return null ;
				}

				//---------------------------------------------------------

				// ダウンロードの状態に更新がある場合に呼び出されるインナーメソッド
				void OnProgress( string path, int length, float downloadingProgress, float writingProgress )
				{
					// ダウンロード状況に変化があった

					// ダウンロード済みのサイズを更新する
					processingEntities[ path ].DownloadingSize	= ( int )( length * downloadingProgress	) ;
					processingEntities[ path ].WritingSize		= ( int )( length * writingProgress		) ;	

					//------------

					// プログレスの表示更新(確定を除くダウンロード中のファイルサイズの合計)
					int downloadingSize	= 0 ;
					int writingSize		= 0 ; 
					foreach( var processingEntity in processingEntities )
					{
						downloadingSize	+= processingEntity.Value.DownloadingSize ;
						writingSize		+= processingEntity.Value.WritingSize ;
					} ;

					// プログレスの表示を更新する
					onProgress?.Invoke( totalDownloadingSize + downloadingSize, totalWritingSize + writingSize, fileStored, processingEntities.Count ) ;
				} ;

				// ダウンロードが終了(成功・失敗)した場合に呼び出されるインナーメソッド
				void OnResult( string path, int length, string error )
				{
					// ダウンロードが完了した
					if( string.IsNullOrEmpty( error ) == true )
					{
						// 成功

						// ダウンロード済みの確定サイズを増やす
						totalDownloadingSize	+= length ;

						// ストレージへ書き込みの確定サイズを増やす
						totalWritingSize		+= length ;

						// 完了したものを除去する
						processingEntities.Remove( path ) ;

						// ダウンロード完了ファイルを１つ増やす
						fileStored ++ ;

						//------------

						// プログレスの表示更新(確定を除くダウンロード中のファイルサイズの合計)
						int downloadingSize	= 0 ;
						int writingSize		= 0 ;
						foreach( var processingEntity in processingEntities )
						{
							downloadingSize += processingEntity.Value.DownloadingSize ;
							writingSize		+= processingEntity.Value.WritingSize ;
						} ;

						// プログレスの表示を更新する
						onProgress?.Invoke( totalDownloadingSize + downloadingSize, totalWritingSize + writingSize, fileStored, processingEntities.Count ) ;
					}
					else
					{
						// 失敗

						// エラーを追加する
						errors.Add( path ) ;

						// 完了したものを除去する
						processingEntities.Remove( path ) ;
					}
				}
			}


			SaveAllManifestInfo_Private() ;	// 全マニフェスト情報を保存する
			m_IsBlockDownloading = false ;	// 一括ダウンロード終了

			// 成功
			onResult?.Invoke( string.Empty ) ;
			request.IsDone = true ;
		}

		// シンプルにアセットバンドルをダウンロードする
		private IEnumerator DownloadExecute_Private( string path, bool keep, Action<string,int,float,float> onProgress, Action<string,int,string> onResult )
		{
			if( GetManifestNameAndAssetBundleName( path, out string manifestName, out string assetBundlePath, out _ ) == true )
			{
				if( string.IsNullOrEmpty( manifestName ) == false && m_ManifestHash.ContainsKey( manifestName ) == true )
				{
					if( string.IsNullOrEmpty( assetBundlePath ) == false )
					{
						yield return StartCoroutine
						(
							m_ManifestHash[ manifestName ].DownloadAssetBundle_Coroutine
							(
								assetBundlePath,
								keep,
								( int length, float downloadingProgress, float writingProgress ) =>
								{
									onProgress?.Invoke( path, length, downloadingProgress, writingProgress ) ;
								},
								( int length, string error ) =>
								{
									onResult?.Invoke( path, length, error ) ;
								},
								false,
								null,
								this
							)
						) ;
					}
				}
			}
		}

		internal void OnDisable()
		{
			if( m_IsBlockDownloading == true )
			{
				// 一括ダウンロード中にアセットバンドルマネージャが停止させられたらマニフェスト情報を全て保存する
#if UNITY_EDITOR
				Debug.Log( "<color=#FFFF00>アセットバンドルを一括ダウンロード中にアセットマネージャに停止要求が出されたため全マニフェスト情報を保存する</color>" ) ;
#endif
				if( SaveAllManifestInfo_Private() == true )
				{
#if UNITY_EDITOR
					Debug.Log( "<color=#00FFFF>保存成功</color>" ) ;
#endif
				}
				else
				{
#if UNITY_EDITOR
					Debug.LogWarning( "<color=#FF7F00>保存失敗</color>" ) ;
#endif
				}

				m_IsBlockDownloading = false ;
			}
		}
	}
}
