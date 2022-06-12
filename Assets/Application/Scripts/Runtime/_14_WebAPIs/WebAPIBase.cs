using System ;
using UnityEngine ;
using Cysharp.Threading.Tasks ;

using Template.Enumerator ;

namespace Template.WebAPIs
{
	/// <summary>
	/// 機能拡張 Version 2022/05/04
	/// </summary>
	public partial class RequestBase
	{
		/// <summary>
		/// コンストラクタ(共通リクエストパラメータを設定する)
		/// </summary>
		public RequestBase()
		{
			SystemVersion		= Define.SystemVersion ;

			Debug.Log( "<color=#00FF00>[システムバージョン] " + SystemVersion + "</color>" ) ;

			MasterDataVersion	= PlayerData.MasterDataVersion ;

			Debug.Log( "<color=#00BF00>[マスターデータバージョン] " + MasterDataVersion + "</color>" ) ;

			PlatformCode		= Define.PlatformCode ;
			DeviceName			= Define.PlatformName ;

			AccessToken			= "" ;
		}
	}

	//----------------------------------------------------------------------------

	/// <summary>
	/// WebAPIの基底クラス
	/// </summary>
	public partial class WebAPIBase
	{
		/// <summary>
		/// リクエストデータをシリアライズする
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public byte[] Serialize<T>( T request )
		{
			// 基本的に HTTPS で暗号化されるので暗号化は必須ではない

			return DataPacker.Serialize( request, false ) ;
		}

		/// <summary>
		/// レスポンスデータをデシリアライズする
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="response"></param>
		/// <returns></returns>
		public async UniTask<T> Deserialize<T>( byte[] response, bool isCheckVersion ) where T : class
		{
			T responseData ;

			try
			{
				responseData = DataPacker.Deserialize<T>( response, false ) ;
			}
			catch( Exception e )
			{
				// パース出来ない
				responseData = JsonUtility.FromJson<T>( "{\"Code\":99999,\"Error\":\"" + e.Message + "\"}" ) ;
			}

			if( responseData != null && responseData is ResponseBase )
			{
				ResponseBase responseBase = responseData as ResponseBase ;

				var responseCode		= responseBase.ResponseCode ;
				string responseMessage	= responseBase.ResponseMessage ;

				// デバッグ
//				responseCode = ResponseCodes.S_Maintenance ;
//				errorMessage = "メンテナンス中です" ;

				// デバッグ
//				responseCode = ResponseCodes.S_SystemVersionIsOld ;
//				errorMessage = "アプリが古いです" ;


				if( responseCode != ResponseCodes.S_Succeeded )
				{
					// エラーが発生した

					if( isCheckVersion == false )
					{
						// WebAPI.Maintanance.CheckVersion() 通信

						if
						(
							responseCode == ResponseCodes.S_Redirect				||
							responseCode == ResponseCodes.S_MasterVersionNotEqual
						)
						{
							// リダイレクトまたはマスターデータの更新必須であればそのまま戻って良い
							return responseData ;
						}
					}

					//--------------------------------

					if( responseCode == ResponseCodes.S_SystemVersionIsOld )
					{
						// クライアントバージョンの確認
#if UNITY_EDITOR
						// クライアントバージョンが古い
						responseMessage = "クライアントバージョンが古いです" ;
						responseMessage += "\n\n" + "<color=#FF7F00>作業中のブランチがあればマージ</color>して\n<color=#00BF00>beast-client</color>レポジトリの\n<color=#00BF00>development</color>ブランチの\n<color=#00BFBF>最新の状態のものをpull</color>し直してください" ;
						Debug.LogWarning( responseMessage ) ;
#endif
						Scene.SetParameter( "MaintenanceDescription", responseMessage ) ;
						Scene.SetParameter( "IsVersionOld", true ) ;

						ApplicationManager.Reboot() ;

						// タスクをまとめてキャンセルする
						throw new OperationCanceledException() ;
					}
					else
					if( ( int )responseCode >= 50000 )
					{
						// 致命的エラー

						if( Define.DevelopmentMode == true )
						{
							// リリース版以外では細かいエラー内容を表示する
							if( string.IsNullOrEmpty( responseMessage ) == true )
							{
								responseMessage = "エラーメッセージ無し" ;
							}

							string playerId = "不明" ;

							responseMessage =
								responseCode + " ( " + ( int )responseCode + " )" + "\n" +
								"[PlayerId = " + playerId + " ]" + "\n" +
								"================" + "\n" +
								responseMessage + "\n" +
								"================" ;

							Debug.LogWarning( "WebAPI でエラー発生" + responseMessage ) ;
						}
						else
						{
							// リリース版ではシンプル
							responseMessage = "不正行為を確認しました\n( " + ( ( int )responseCode ).ToString() + " )" ;
						}

						// アプリを再起動する事は変わらず
						responseMessage += "\n\nアプリを再起動します" ;

						await Dialog.Open( "通信エラー", responseMessage, new string[]{ "再起動" } ) ;

						// リブートを実行する
						ApplicationManager.Reboot() ;

						// タスクをまとめてキャンセルする
						throw new OperationCanceledException() ;
					}
				}

				//---------------------------------------------------------
				// 成功

//				if( responseBase.Date != null )
//				{
					// サーバー時間を更新する
					ServerTime.SetServerBaseTime( responseBase.Date ) ;

#if UNITY_EDITOR
					Debug.Log( "<color=#FF00FF>[ServerTime] " + ServerTime.GetDateName() + " " + ServerTime.GetTimeName() + " ( " + ServerTime.BaseTime + " )</color>" ) ;
#endif
//				}

				//---------------------------------------------------------

				if( responseBase.Updater != null )
				{
					// プレイヤーデータを更新する
					if( PlayerData.Update( responseBase.Updater ) == true )
					{
						// プレイヤーデータのラッパーを更新する
						PlayerData.Adjust( responseBase.Updater ) ;
					}
				}
			}
			else
			{
				string message = "データのデシリアライズに失敗しました\nサイズ : " + response.Length + "\n\nアプリを再起動します" ;
				await Dialog.Open( "致命的エラー", message, new string[]{ "再起動" } ) ;

				// リブートを実行する
				ApplicationManager.Reboot() ;

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			return responseData ;
		}
	}


	//--------------------------------------------------------------------
	// エラーを意図的に起こさせる

	/// <summary>
	///  House カテゴリの通信 API 群
	/// </summary>
	public partial class Debugger : WebAPIBase
	{
		// <summary>
		/// 通信 API:意図的にエラーを発生させる
		/// </summary>
		/// <returns></returns>
		public async UniTask<ResponseBase> Error( Action<int, string, ResponseBase> onReceived = null, Action<int, int> onProgress = null, bool useProgress = true, bool useDialog = true, bool isCheckVersion = true )
		{
			// リクエストデータを生成する
			RequestBase request = new RequestBase()
			{
			} ;

			// リクエストデータをシリアライズする
			byte[] requestData = Serialize( request ) ;

			// 正常系の対応のみ考えれば良い(エラーはWebAPIManager内で処理される)
			( int httpStatus, string errorMessage, byte[] responseData ) = await WebAPIManager.SendRequest
			(
				"dummy", // API
				requestData,	// リクエストデータ
				onProgress,
				useProgress,
				useDialog
			) ;

			if( responseData == null )
			{
				// エラーの場合のHttpStatusとErrorMessageはコールバックで取得する(通常は必要としないため)
				onReceived?.Invoke( httpStatus, errorMessage, null ) ;
				return null ;	// エラー
			}

            // レスポンスデータをデシリアライズする
			var response = await Deserialize<ResponseBase>( responseData, isCheckVersion );
			onReceived?.Invoke( httpStatus, errorMessage, response ) ;
			return response ;
		}
	}
}

