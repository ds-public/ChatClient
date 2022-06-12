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
		/// アセットバンドルを消去する必要があるか判定する
		/// </summary>
		/// <returns></returns>
		public static bool IsForceClear()
		{
			bool isForceClear = false ;

			// 設定情報を取得する
			Settings settings = ApplicationManager.LoadSettings() ;

			// デバッグ機能としてアセットバンドルバージョン値が異なったらアセットバンドルファイルを全てクリアする
			if( settings.EnableAssetBundleCleaning == true )
			{
				string key = "AssetBundleVersion" ;
				if( Preference.HasKey( key ) == true )
				{
					// キーが存在する場合はバージョンチェックしてキーが新しくなっていたらアセットバンドルを全消去する
					int assetBundleVaersion = Preference.GetValue<int>( key ) ;
					if( assetBundleVaersion != settings.AssetBundleVersion )
					{
						isForceClear = true ;
					}
				}
			}

			// 修復の強制アセットバンドル削除判定
			if( Scene.ContainsParameter( "AssetBundleAllClear" ) == true )
			{
				if( Scene.GetParameter<bool>( "AssetBundleAllClear", false ) == true )
				{
					isForceClear = true ;
				}
			}

			return isForceClear ;
		}

		/// <summary>
		/// アセットバンドル消去フラグを完結させる
		/// </summary>
		public static async UniTask RefreshClearFlag()
		{
			// 設定情報を取得する
			Settings settings = ApplicationManager.LoadSettings() ;

			// デバッグ機能としてアセットバンドルバージョン値が異なったらアセットバンドルファイルを全てクリアする
			if( settings.EnableAssetBundleCleaning == true )
			{
				string key = "AssetBundleVersion" ;

				if( Preference.HasKey( key ) == true )
				{
					int assetBundleVaersion = Preference.GetValue<int>( key ) ;
					if( assetBundleVaersion != settings.AssetBundleVersion )
					{
						string message = "<color=#FF7F00>Settingの指定により全アセットバンドルを強制消去しました</color>" ;
						await Dialog.Open( "注意",message, new string[]{ "閉じる" } ) ;
					}
				}

				Preference.SetValue<int>( key, settings.AssetBundleVersion ) ;
				Preference.Save() ;
			}

			Scene.RemoveParameter( "AssetBundleAllClear" ) ;
		}

		// 消去する必要があればタイトル後に必要にうるアセットバンドル群を消去する
		private static void ClearConstantAssetBundle()
		{
			if( IsForceClear() == true )
			{
				Debug.Log( "<color=#FF7F00>マニフェスト Default External のアセットバンドル群を消去しました</color>" ) ;
				Asset.Cleanup( "Default" ) ;
				Asset.Cleanup( "External" ) ;
			}
		}
	}
}
