//#define USE_MESSAGEPACK

using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using Cysharp.Threading.Tasks ;

using UnityEngine ;

using JsonHelper ;
using StorageHelper ;

namespace Template
{
	/// <summary>
	/// ゲーム全体から参照されるプレイヤー系データを保持するクラス Version 2022/01/17
	/// </summary>
	public partial class PlayerData
	{
		//---------------------------------------------------------------------------
		// 一時保存対象

		/// <summary>
		/// アクティブなエンドポイント
		/// </summary>
		public static string	EndPoint ;

		/// <summary>
		/// マスターデータバージョン
		/// </summary>
		public static long		MasterDataVersion ;

		/// <summary>
		/// マスターデータのキー
		/// </summary>
		public static string	MasterDataKey ;

		/// <summary>
		/// マスターデータのキーの亜種版
		/// </summary>
		public static string	MasterDataKeyAlternate
		{
			get
			{
				if( string.IsNullOrEmpty( MasterDataKey ) == true )
				{
					return "Unknown" ;
				}

				// 順番を逆にする
				string key = string.Empty ;
				int i, l = MasterDataKey.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					key += MasterDataKey[ l - 1 - i ] ;
				}

				return key ;
			}
		}

		/// <summary>
		/// 変形キーを取得する
		/// </summary>
		public static string KeyAlternate
		{
			get
			{
				return StorageAccessor.GetMD5Hash( MasterDataKeyAlternate + "_key" ) ;
			}
		}

		/// <summary>
		/// 変形ベクターを取得する
		/// </summary>
		public static string VectorAlternate
		{
			get
			{
				return StorageAccessor.GetMD5Hash( MasterDataKeyAlternate + "_vector" ) ;
			}
		}

		/// <summary>
		/// アセットバンドルのパス
		/// </summary>
		public static string	AssetBundlePath ;
		
		/// <summary>
		/// ウェブビューのルートパス
		/// </summary>
		public static string	WebViewPath ;

		/// <summary>
		/// ＣＭＳパス
		/// </summary>
		public static string	CmsPath ;

		/// <summary>
		/// ストアのＵＲＬ
		/// </summary>
		public static string	StoreUrl ;

		//-----------------------------------------------------------

		/// <summary>
		/// データがアップデートされたタイミングで呼び出すコールバック
		/// </summary>
		public static Action	OnDataUpdated ;

		//---------------

		/// <summary>
		/// プレイヤー識別子
		/// </summary>
		public static long		PlayerId ;

		/// <summary>
		/// アクセストークン
		/// </summary>
		public static string	AccessToken = string.Empty ;


		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// メモリ内の情報を消去する
		/// </summary>
		public static void Cleanup()
		{
			PlayerId	= 0 ;
			AccessToken	= null ;
		}


		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// プレイヤーデータを更新する
		/// </summary>
		/// <param name="updater"></param>
		public static void Adjust( PlayerData.Updater updater )
		{
			Debug.Log( "<color=#00FFFF>[PlayerData Update] Execute</color>" ) ;

			//--------------

			// 更新通知のコールバックを呼び出す
			OnDataUpdated?.Invoke() ;

			//----------------------------------------------------------

#if UNITY_EDITOR
			// モニター情報を更新する
			RefreshMonitor() ;
#endif
			//----------------------------------------------------------
		}

#if UNITY_EDITOR
		/// <summary>
		/// デバッグ用のモニター情報を更新する
		/// </summary>
		public static void RefreshMonitor()
		{
			// モニター情報を更新する
			PlayerDataManager.Instance.SetMonitor() ;
		}
#endif

		/// <summary>
		/// ログイン中かどうか
		/// </summary>
		public static bool IsLogin
		{
			get
			{
				return ( Ids != null ) ;
			}
		}

		//-------------------------------------------------------------------------------------------
		// メモリのみの情報(アプリ起動時にリセットされる)


		//-------------------------------------------------------------------------------------------
		// 注意：PlayerData 自体は Serializable にはなっていない(ただのネームスペースクラス)

		/// <summary>
		/// PlayerData のうち端末ストレージに保存するもの
		/// </summary>
		[Serializable]
		public class LocalData
		{
			/// <summary>
			/// 最後にログインボーナスをチェックした日時(サーバー時
			/// </summary>
			public long		LastLoginAt	= 0 ;
		}

		/// <summary>
		/// プレイヤーデータのローカル保存部分
		/// </summary>
		public static LocalData	Local = new LocalData() ;

		//-------------------------------------------------------------------------------------------

		private const string m_FilePath = "LocalPlayer" ;

		/// <summary>
		/// ロードする
		/// </summary>
		public static void LoadLocal()
		{
			( string key, string vector ) = GetCryptoKeyAndVector() ;
			string text = StorageAccessor.LoadText( GetPath(), key, vector ) ;

			if( string.IsNullOrEmpty( text ) == false )
			{
				Local = JsonUtility.FromJson<LocalData>( text ) ;
			}
		}

		/// <summary>
		/// セーブする
		/// </summary>
		public static void SaveLocal()
		{
			string text = JsonUtility.ToJson( Local ) ;
			if( string.IsNullOrEmpty( text ) == false )
			{
				( string key, string vector ) = GetCryptoKeyAndVector() ;
				StorageAccessor.SaveText( GetPath(), text, true, key, vector ) ;
			}
		}

		// パスを取得する
		private static string GetPath()
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

		// 暗号化のキーとベクターを取得する
		private static ( string, string ) GetCryptoKeyAndVector()
		{
			string key		= null ;
			string vector	= null ;

			// 暗号化
			if( Define.SecurityEnabled == true )
			{
				key		= Define.CryptoKey ;
				vector	= Define.CryptoVector ;
			}

			return ( key, vector ) ;
		}

		/// <summary>
		/// 全ての情報を消去する
		/// </summary>
		public static void ClearLocal( bool withSave = true )
		{
			Local = new LocalData() ;

			if( withSave == true )
			{
				SaveLocal() ;
			}
		}
	}
}
