﻿using System ;
using System.Collections.Generic ;
using Cysharp.Threading.Tasks ;
using UnityEngine ;

//<auto-generated>
//Beast.ShareClientGeneratorによる自動生成されたファイルです。手動変更禁止
//<auto-generated>
namespace Template.WebAPIs
{
	/// <summary>
	///  Maintenance カテゴリの通信 API 群
	/// </summary>
	public partial class  Maintenance : WebAPIBase
	{
		/// <summary>
		/// WebAPI:Maintenanceのリクエストデータクラス
		/// </summary>
		[Serializable]
		public partial class CheckVersion_Request : RequestBase
		{
		}

		/// <summary>
		/// WebAPI:Maintenanceのレスポンスデータクラス
		/// </summary>
		[Serializable]
		public partial class CheckVersion_Response : ResponseBase
		{
			[SerializeField]
			public Int64 masterDataVersion ;
			public Int64 MasterDataVersion { get { return masterDataVersion ; } }

			[SerializeField]
			public String masterDataPath ;
			public String MasterDataPath { get { return masterDataPath ; } }

			[SerializeField]
			public String masterDataKey;
			public String MasterDataKey { get { return masterDataKey ; } }

			[SerializeField]
			public String endPoint ;
			public String EndPoint { get { return endPoint ; } }

			[SerializeField]
			public String assetBundlePath ;
			public String AssetBundlePath { get { return assetBundlePath; } }

			[SerializeField]
			public String webViewPath ;
			public String WebViewPath { get { return webViewPath; } }

			[SerializeField]
			public String cmsPath ;
			public String CmsPath { get { return cmsPath; } }

			[SerializeField]
			public String storeUrl ;
			public String StoreUrl { get { return storeUrl ; } }
		}

		// <summary>
		/// 通信 API:クライアントバージョン、マスタデータバージョンチェックを行う
		/// </summary>
		/// <returns></returns>
		public async UniTask<CheckVersion_Response> CheckVersion( Action<int, string, CheckVersion_Response> onReceived = null, Action<int, int> onProgress = null, bool useProgress = true, bool useDialog = true, bool isCheckVersion = true )
		{
			// リクエストデータを生成する
			CheckVersion_Request request = new CheckVersion_Request()
			{
				
			} ;
			// リクエストデータをシリアライズする
			byte[] requestData = Serialize( request ) ;
			// 正常系の対応のみ考えれば良い(エラーはWebAPIManager内で処理される)
			( int httpStatus, string errorMessage, byte[] responseData ) = await WebAPIManager.SendRequest
			(
				"maintenance/checkversion", // API
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
			var response = await Deserialize<CheckVersion_Response>( responseData, isCheckVersion );
			onReceived?.Invoke( httpStatus, errorMessage, response ) ;
			return response ;
		}
	}
}