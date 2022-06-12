using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Globalization;
using System.Linq ;

using UnityEngine ;

using Cysharp.Threading.Tasks ;

using TimeHelper ;

using Template.Enumerator ;

namespace Template
{
	/// <summary>
	/// ユーザー系の情報を保持したり処理したりするクラス Version 2022/04/29 0
	/// </summary>
	public partial class PlayerDataManager : SingletonManagerBase<PlayerDataManager>
	{
		//---------------------------------------------------------------------------
		// モニター用

#if UNITY_EDITOR

		[Header( "BEAST 関連" )]

		[SerializeField]
		protected string						m_EndPoint ;

		[SerializeField]
		protected long							m_MasterDataVersion ;

		[SerializeField]
		protected string						m_MasterDataPath ;

		[SerializeField]
		protected string						m_MasterDataKey ;

		[SerializeField]
		protected string						m_AssetBundlePath ;

		[SerializeField]
		protected string						m_WebViewPath ;

		[SerializeField]
		protected string						m_CmsPath ;

		[SerializeField]
		protected string						m_StoreUrl ;

		//-----------------------------------

		[SerializeField]
		protected long							m_PlayerId ;

		[SerializeField]
		protected string						m_AccessToken ;

		//---------------

		[Header( "ローカル情報" )]

		[SerializeField]
		protected PlayerData.LocalData			m_PlayerLocal ;

#endif

		//---------------------------------------------------------------------------

		/// <summary>
		/// ログインする(プレイヤー未登録で利用規約に同意させるのたタイトル画面側で行う)
		/// </summary>
		/// <returns></returns>
		public static async UniTask<bool> Login()
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return await m_Instance.Login_Private() ;
		}

		private async UniTask<bool> Login_Private()
		{
			//------------------------------------------------------------------------------------------
			// ログイン

			bool isNewGame = false ;

			int httpStatus = default ;
			string errorMessage = default ;

			// オプション１はマスクの表示の有無
			Progress.On( Progress.Styles.WebAPI, false, "ログイン中" ) ;

			// ログイン通信を実行する
			var rd = await WebAPI.Player.Login( ( r1, r2, r3 ) => { httpStatus = r1 ; errorMessage = r2 ; }, useProgress:false ) ;

			if( rd.ResponseCode == ResponseCodes.S_PlayerNotFound )
			{
				// PlayerData が作られていないので生成する
				while( true )
				{
					rd = await WebAPI.Player.Create( ( r1, r2, r3 ) => { httpStatus = r1 ; errorMessage = r2 ; }, useProgress:false ) ;
					if( rd.ResponseCode == ResponseCodes.S_Succeeded )
					{
						Debug.Log( "<color=#7FFFFF>[ログイン成功] PlayerId = " + rd.PlayerId + " AccessToken = " + rd.AccessToken + "</color>" ) ;
						break ;
					}
				}

				isNewGame = true ;
			}
			else
			{
				// PlayerData は既に生成済み
				Debug.Log( "<color=#7FFFFF>[ログイン成功] PlayerId = " + rd.PlayerId + " AccessToken = " + rd.AccessToken + "</color>" ) ;
			}

			Progress.Off() ;

			//----------------------------------------------------------

			// プレイヤーの作り直しが起こったら以下のものは初期化する

			bool isReset = false ;

			string key_LoginTimes =  "LoginTimes" ;
			int loginTimes = Preference.GetValue( key_LoginTimes, 0 ) ;
			if( loginTimes == 0 )
			{
				isReset = true ;
			}

			if( PlayerData.General.LoginTimes <= loginTimes )
			{
				// サーバー側のログイン回数が端末ストレージ保存のログイン回数以下の場合はプレイヤーの作り直しが行われたと判断する
				isReset = true ;
			}

			// 新しいログイン回数を保存する
			Preference.SetValue<int>( key_LoginTimes, PlayerData.General.LoginTimes ) ;
			Preference.Save() ;

			if( isNewGame == true || isReset == true )
			{
				// 初回

				Debug.Log( "<color=#FFFF00>プレイヤーデータの作り直しが行われたためローカル保存のプレイヤーデータを初期化します</color>" ) ;

				// プレイヤーデータのローカル部分を初期化する
				PlayerData.ClearLocal() ;
			}
			else
			{
				// 継続
			}

			//----------------------------------------------------------
			// モニタリング

#if UNITY_EDITOR

			m_PlayerLocal		= PlayerData.Local ;

			SetMonitor() ;
#endif
			//----------------------------------------------------------

			// プレイヤー識別子は保存しておく
			PlayerData.PlayerId		= rd.PlayerId ;
			PlayerData.AccessToken	= rd.AccessToken ;

#if UNITY_EDITOR
			// モニター用
			m_PlayerId		= PlayerData.PlayerId ;
			m_AccessToken	= PlayerData.AccessToken ;
#endif

			//------------------------------------------------------------------------------------------

			// 成功
			return true ;
		}

		/// <summary>
		/// ログアウトする
		/// </summary>
		public static bool LogOut()
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.LogOut_Private() ;
		}

		private bool LogOut_Private()
		{
			PlayerData.Clear() ;	// サーバーから送られた情報を消去する
			PlayerData.Cleanup() ;	// メモリ内の情報を消去する
#if UNITY_EDITOR
			PlayerData.RefreshMonitor() ;

			m_PlayerId		= PlayerData.PlayerId ;
			m_AccessToken	= PlayerData.AccessToken ;
#endif
			return true ;
		}

		//-------------------------------------------------------------------------------------------

#if UNITY_EDITOR

		/// <summary>
		/// マスターデータの情報をモニター用にセットする
		/// </summary>
		/// <param name="masterDataVersion"></param>
		/// <param name="masterDataKey"></param>
		public static void SetMasterDataInfomation
		(
			string	endPoint,
			long	masterDataVersion,
			string	masterDataPath,
			string	masterDataKey,
			string	assetBundlePath,
			string	webViewPath,
			string	cmsPath,
			string	storeUrl
		)
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.m_EndPoint			= endPoint ;
			m_Instance.m_MasterDataVersion	= masterDataVersion ;
			m_Instance.m_MasterDataPath		= masterDataPath ;
			m_Instance.m_MasterDataKey		= masterDataKey ;
			m_Instance.m_AssetBundlePath	= assetBundlePath ;
			m_Instance.m_WebViewPath		= webViewPath ;
			m_Instance.m_CmsPath			= cmsPath ;
			m_Instance.m_StoreUrl			= storeUrl ;
		}
#endif
	}
}

