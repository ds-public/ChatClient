using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using Cysharp.Threading.Tasks ;

using UnityEngine ;

using StorageHelper ;
using CSVHelper ;

using Template.Enumerator ;
using Template.MasterData ;

namespace Template
{
	/// <summary>
	/// ユーザー系の情報を保持したり処理したりするクラス Version 2022/05/04 0
	/// </summary>
	public partial class MasterDataManager : SingletonManagerBase<MasterDataManager>
	{
		//---------------------------------------------------------------------------

		/// <summary>
		/// ＣＳＶからマスターデータを展開する(同期)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path"></param>
		/// <returns></returns>
		public static List<T> LoadFromCsv<T>( string path, int startColumn, int startRow, int nameRow, int typeRow, int dataRow, int keyColumn ) where T : class, new()
		{
			TextAsset ta = Asset.Load<TextAsset>( path ) ;
			if( ta != null )
			{
				List<T> list = new List<T>() ;
				if( CSVObject.Load<T>( ta.text, ref list, startColumn, startRow, nameRow, typeRow, dataRow, keyColumn ) == true )
				{
					// 展開成功
					return list ;
				}
			}
			return null ;
		}

		/// <summary>
		/// ＣＳＶからマスターデータを展開する(非同期)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path"></param>
		/// <param name="onAction"></param>
		/// <returns></returns>
		public static async UniTask<List<T>> LoadFromCsvAsync<T>( string path, int startColumn, int startRow, int nameRow, int typeRow, int dataRow, int keyColumn, Action<List<T>> onAction ) where T : class, new()
		{
			return await m_Instance.LoadFromCsvAsync_Private<T>( path, startColumn, startRow, nameRow, typeRow, dataRow, keyColumn, onAction ) ;
		}

		private async UniTask<List<T>> LoadFromCsvAsync_Private<T>( string path, int startColumn, int startRow, int nameRow, int typeRow, int dataRow, int keyColumn, Action<List<T>> onAction ) where T : class, new()
		{
			TextAsset ta = Asset.Load<TextAsset>( path ) ;
			if( ta == null )
			{
				ta = await Asset.LoadAsync<TextAsset>( path ) ;
			}
			if( ta != null && string.IsNullOrEmpty( ta.text ) == false )
			{
				List<T> list = new List<T>() ;
				if( CSVObject.Load<T>( ta.text, ref list, startColumn, startRow, nameRow, typeRow, dataRow, keyColumn ) == true )
				{
					// 展開成功
					onAction?.Invoke( list ) ;
					return list ;
				}
			}

			Debug.LogWarning( "Failed to load = " + typeof( T ).ToString() ) ;
			return null ;
		}

		//-------------------------------------------------------------------------------------------

		private const string m_FilePath = "MasterData" ;

		// パスを取得する
		private string GetPath()
		{
			string path = m_FilePath ;

			// 暗号化
			if( Define.SecurityEnabled == true )
			{
				path = Security.GetHash( path ) ;
			}

			path = "System/" + path ;

			return path ;
		}

		/// <summary>
		/// マスターデータをダウンロードする
		/// </summary>
		/// <returns></returns>
		public static async UniTask<bool> DownloadAsync( bool useProgress = false, Action<float> onProgress = null )
		{
			return await m_Instance.DownloadAsync_Private( useProgress, onProgress ) ;
		}

		private async UniTask<bool> DownloadAsync_Private( bool useProgress, Action<float> onProgress )
		{
			string key = "MasterDataVersion" ;

			// マスターデータのバージョンを取得する(取得できればマスターデータが存在する)
			long masterDataVersion = 0 ;
			if( Preference.HasKey( key ) == true )
			{
				masterDataVersion = Preference.GetValue<long>( key ) ;
			}
			if( StorageAccessor.GetSize( GetPath() ) <= 0 )
			{
				// ファイルが存在しないためバージョンを強制的に 0 にする
				masterDataVersion = 0 ;
			}

			// 現在ストレージに保存されているマスターデータのバージョンをオンメモリにも保存する(あまり意味は無い)
			PlayerData.MasterDataVersion = masterDataVersion ;

			//----------------------------------------------------------

			// バージョンチェック
			int httpStatus		= default ;
			string errorMessage	= default ;
			WebAPIs.Maintenance.CheckVersion_Response response = default ;
			string assetBundlePath = string.Empty ;

			while( true )
			{
				httpStatus		= default ;
				errorMessage	= default ;

				// 最初の通信実行
				response = await WebAPI.Maintenance.CheckVersion( ( r1, r2, r3 ) => { httpStatus = r1 ; errorMessage = r2 ; }, useProgress:useProgress, isCheckVersion:false ) ;

				// 通信に成功しない限りここにはこない

				Debug.Log( "<color=#00FFFF>[通信成功] ResponseCode : " + response.ResponseCode + "</color>" ) ;

				//-----------------------------------------------------------------------------------------

				assetBundlePath = response.AssetBundlePath ;
				if( string.IsNullOrEmpty( assetBundlePath ) == false )
				{
					// 最後にスラッシュがあれば削る
					assetBundlePath = assetBundlePath.TrimEnd( '/' ) ;
				}

				Debug.Log( "<color=#00DF3F>MasterDataVersion : "	+ response.MasterDataVersion	+ "</color>" ) ;
				Debug.Log( "<color=#00DF3F>MasterDataPath : "		+ response.MasterDataPath		+ "</color>" ) ;
				Debug.Log( "<color=#00DF3F>MasterDataKey : "		+ response.MasterDataKey		+ "</color>" ) ;
				Debug.Log( "<color=#00DF3F>AssetBundlePath : "		+ assetBundlePath				+ "</color>" ) ;
				Debug.Log( "<color=#00DF3F>WebViewPath : "			+ response.WebViewPath			+ "</color>" ) ;
				Debug.Log( "<color=#00DF3F>CmsPath : "				+ response.CmsPath				+ "</color>" ) ;
				Debug.Log( "<color=#00DF3F>StoreUrl          : "	+ response.StoreUrl				+ "</color>" ) ;

				//----------------------------------

				// マスターデータのバージョンをオンメモリにも保存する
				PlayerData.MasterDataVersion	= response.MasterDataVersion ;

				// マスターデータのキーをオンメモリにのみ保存する(アプリ起動中のみ有効)
				PlayerData.MasterDataKey		= response.MasterDataKey ;

				// アセットバンドルパスをオンメモリのみに保存する(アプリ起動中のみ有効)
				PlayerData.AssetBundlePath		= assetBundlePath ;

				// ウェブビューのルートパスを保存する
				PlayerData.WebViewPath			= response.WebViewPath ;

				// ＣＭＳパスをオンメモリのみに保存する(アプリ起動中のみ有効)
				PlayerData.CmsPath				= response.CmsPath ;

				// ストアのＵＲＬをオンメモリにも保存する
				PlayerData.StoreUrl				= response.StoreUrl ;

				//-----------------------------------------------------------------------------------------

				// リダイレクトチェック
				if( response.ResponseCode == ResponseCodes.S_Redirect )
				{
					// クライアントのシステムバージョンがサーバー環境よりも新しいので別のサーバー環境にリダイレクトする(Apple審査用)
					string redirectEndPoint = response.EndPoint ;
					if( redirectEndPoint.IsNullOrEmpty() == true )
					{
						// クライアントバージョンが古い
						await Dialog.Open( "注意", "リダイレクト用のエンドポイントが異常です", new string[]{ "再起動" } ) ;

						// リブートを実行する
						ApplicationManager.Reboot() ;

						// タスクをまとめてキャンセルする
						throw new OperationCanceledException() ;
					}

					Debug.LogWarning( "<color=#FFFF00><リダイレクトを行う> : " + redirectEndPoint + "</color>" ) ;

					// リダイレクト用のエンドポイントを設定して再度エンドポイントにアクセスする
					PlayerData.EndPoint = redirectEndPoint ;
					WebAPIManager.EndPoint = PlayerData.EndPoint ;
				}
				else
				{
					// 通常のエラーチェックフェーズへ
					break ;
				}
			}

			//----------------------------------------------------------

			byte[] masterData ;

			// マスターデータバージョンの確認
			if( response.ResponseCode == ResponseCodes.S_MasterVersionNotEqual )
			{
				// マスターデータの更新が必要

				// マスターデータのダウンロードを行う(失敗したらリブートになる)
				masterData = await Download.ToBytes
				(
					response.MasterDataPath,
					onProgress : ( int offset, int length ) =>
					{
						if( length >  0 )
						{
							onProgress?.Invoke( ( float )offset / ( float )length ) ;
						}
					},
					useProgress: false,
					title:"マスターデータのダウンロード",
					message:"マスタデータのダウンロードに\n失敗しました"
				) ;

				// 成功した場合のみここに来る
				Debug.Log( "<color=#00FF00>マスターデータを保存します[ Size = " + masterData.Length + " ( " + ExString.GetSizeName( masterData.Length ) + " ) ]</color>" ) ;
				
				// マスターデータを保存する
				bool result = false ;

				string	keyAlternane    = null ;
				string	vectorAlternane = null ;

				// 暗号化
				if( Define.SecurityEnabled == true )
				{
					keyAlternane    = PlayerData.KeyAlternate ;
					vectorAlternane = PlayerData.VectorAlternate ;
				}

				// Key と Vector は 16文字である必要がある
				if( StorageAccessor.Save( GetPath(), masterData, true, keyAlternane, vectorAlternane ) == true )
				{
					// マスターデータのバージョンを保存する
					Preference.SetValue<long>( key, response.MasterDataVersion ) ;
					if( Preference.Save() == true )
					{
						result = true ;	// マスターデータの保存に成功した
					}
					else
					{
						Debug.LogError( "プリファレンスの保存に失敗" ) ;
					}
				}
				else
				{
					Debug.LogError( "マスターデータの保存に失敗" ) ;
				}

				if( result == false )
				{
					await Dialog.Open( "マスターデータの保存", "マスターデータの保存に失敗しました\n\n再起動します", new string[]{ "再起動" } ) ;

					// リブートを実行する
					ApplicationManager.Reboot() ;

					// タスクをまとめてキャンセルする
					throw new OperationCanceledException() ;
				}
			}
			else
			{
				Debug.Log( "<color=#7FFF7F>< マスターデータの更新不要 ></color>" ) ;
			}

			//----------------------------------------------------------

			if( Define.DevelopmentMode == true )
			{
				string endPoint = WebAPIManager.EndPoint ;

				// サーバー環境の表示を設定する
				SetEnvironment( endPoint ) ;
			}

#if UNITY_EDITOR

			// モニター用にエンドポイントとマスターデータバージョンとマスターデータパスとマスターデータキーを設定する
			PlayerDataManager.SetMasterDataInfomation
			(
				WebAPIManager.EndPoint,
				response.MasterDataVersion,
				response.MasterDataPath,
				response.MasterDataKey,
				assetBundlePath,
				response.WebViewPath,
				response.CmsPath,
				response.StoreUrl
			) ;

#endif
			//----------------------------------------------------------

			// 成功
			return true ;
		}

		/// <summary>
		/// 接続サーバー環境の情報を設定する
		/// </summary>
		private void SetEnvironment( string endPoint )
		{
			var settings = ApplicationManager.LoadSettings() ;
			if( settings == null )
			{
				return ;
			}

			var endPoints = settings.WebAPI_EndPoints ;

			List<string> endPointNames = new List<string>(){ "任意サーバー" } ;
			endPointNames.AddRange( endPoints.Select( _ => _.DisplayName ) ) ;

			int endPointIndex = GetIndex( endPoint ) ;

			//---------------------------------
			// インナーメソッド：インデックスを検査する

			int GetIndex( string path )
			{
				int p = 0 ;

				int i, l = endPoints.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( path == endPoints[ i ].Path )
					{
						p = 1 + i ;	// 発見
						break ;
					}
				}

				return p ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// マスターデータをメモリに展開する
		/// </summary>
		/// <returns></returns>
		public static async UniTask<bool> LoadAsync( Action<float> onProgress = null )
		{
			return await m_Instance.LoadAsync_Private( onProgress ) ;
		}

		private async UniTask<bool> LoadAsync_Private( Action<float> onProgress )
		{
			// マスターデータを展開する
			string keyAlternane    = null ;
			string vectorAlternane = null ;

			// 暗号化
			if( Define.SecurityEnabled == true )
			{
				keyAlternane    = PlayerData.KeyAlternate ;
				vectorAlternane = PlayerData.VectorAlternate ;
			}

			// Key と Vector は 16文字である必要がある
			byte[] masterData = StorageAccessor.Load( GetPath(), keyAlternane, vectorAlternane ) ;
			if( masterData == null )
			{
				// 失敗(ありえない)
				return false ;
			}

			( string, long )[] files = Zip.GetFiles( masterData, PlayerData.MasterDataKey ) ;
			if( files == null || files.Length == 0 )
			{
				// 失敗(ありえない)
				return false ;
			}

			//----------------------------------------------------------

			int index, count = files.Length ;

#if UNITY_EDITOR
			string lm = "-----マスターデータの展開状況↓: 対象ファイル数[ " + count + " ]" ;
#endif
			for( index  = 0 ; index <  count ; index ++ )
			{
				var file = files[ index ] ;
				( string fileName, long fileSize ) = ( file.Item1, file.Item2 ) ;
//				Debug.Log( "ファイル:" + fileName + " " + fileSize ) ;

				if( m_MasterDataFiles.ContainsKey( fileName ) == true )
				{
					if( DecompressAndDeserialize( masterData, fileName, PlayerData.MasterDataKey, m_MasterDataFiles[ fileName ] ) == true )
					{
						// マスターデータの展開成功
#if UNITY_EDITOR
						lm += "\n" + "成功 -> " + fileName ;
#endif
						onProgress?.Invoke( ( float )( index + 1 ) / ( float )count ) ;
					}
					else
					{
						await Dialog.Open( "ERROR", "マスターデータデシリアライズに失敗しました\n" + fileName, new string[]{ "閉じる" } ) ;
					}
				}
				else
				{
					await Dialog.Open( "WARNING", "マスターデータに対応するデシリアライザが\n登録されていません\n" + fileName, new string[]{ "閉じる" } ) ;
				}

//				await WaitForSeconds( 1 ) ;
			}

#if UNITY_EDITOR
			Debug.Log( lm ) ;
#endif

#if false
			int i, l ;

			Debug.Log( "-----モンスターデータのレコード数:" + MasterData.MonsterData.Records.Count ) ;

			l = MasterData.MonsterData.Records.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				Debug.LogWarning( "名前:[" + i + "] " + MasterData.MonsterData.Records[ i ].Name ) ;
			}
#endif
			//------------------------------------------------------------------------------------------

			// インデックス生成が必要なテーブルはインデックスの生成を行う



			//------------------------------------------------------------------------------------------
			// 以下はローカルのデバッグ用(強制的にローカルのマスターデータを上書きする)

			//----------------------------------------------------------
			// モニター用

#if UNITY_EDITOR
			SetMonitor() ;
#endif
			// 成功
			return true ;
		}

		// マスターデータをデシリアライズする
		private bool DecompressAndDeserialize( byte[] masterData, string fileName, string masterDataKey, Func<byte[],bool> onDecompressed )
		{
			byte[] data = Zip.Decompress( masterData, fileName, masterDataKey ) ;
			if( data == null || data.Length == 0 )
			{
				return false ;	// 伸長失敗
			}

			if( onDecompressed == null )
			{
				return false ;	// 伸長失敗
			}

			return onDecompressed( data ) ;
		}

	}
}
