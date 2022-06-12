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

#if UNITY_EDITOR
		//-------------------------------------------------------------------------------------------
		
		/// <summary>
		/// LayoutBase と DialogBase 用の簡易セットアップ処理
		/// </summary>
		/// <param name="owner"></param>
		/// <returns></returns>
		public static async UniTask<bool> SimpleSetup( ExMonoBehaviour owner )
		{
			// 注意
			// このメソッドは、Layout シーン・Dialog シーンの、デバッグ機能として呼ばれるため、
			// 基本的に PlayerData を無視しない(BEASTログインを実行する)
			// Viewer 系の Screen シーンであっても、そこに Layout シーン・Dialog シーンを追加する場合、
			// Layout シーン・Dialog シーンに関しては、単体デバッグする際、
			// PlayerData が展開される(BEASTログイン)が行われる事に注意する。
			// (すなわち、PlayerData が展開されていても、PlayerData を参照するようなコードを書いてはならない)

			//---------------------------------------------------------
			// Phase 1

#if false
			// マスターデータをダウンロードする(CheckVersion で RemoteAssets のパスを取得する必要があるので最初に実行)
			if( await MasterDataManager.DownloadAsync( false ) == false )
			{
				return false ;	// 失敗
			}

			// マスターデータをメモリに展開する
			await MasterDataManager.LoadAsync() ;
#endif
			// アセットバンドル全体共通の設定を行う
			SetupGeneralAssetBundleSettings() ;

			// ローカルのアセットハンドルのマニフェストのダウンロードと展開
			await SetupInternalAssetBundleSettings( owner ) ;

			// 最初から展開しておきたいアセットを展開する
			await LoadConstantAsset( 1, owner ) ;

			// Phase 1 完了
			ApplicationManager.DownloadingPhase1State = ApplicationManager.DownloadingPhaseStates.Completed ;
			Debug.Log( "<color=#00FF00>[Downloading] Phase 1 Completed !!</color>" ) ;

			//---------------------------------------------------------
			// Title想定
#if false

			// ログインしてプレイヤーデータを取得する
			await PlayerDataManager.Login() ;
#endif
			//---------------------------------------------------------
			// Phase 2

			// リモートのアセットハンドルのマニフェストのダウンロードと展開
//			await SetupExternalAssetBundleSettings( owner ) ;

			// 場合によりアセットバンドル群を消去する
			ClearConstantAssetBundle() ;

			// 指定したアセットバンドルをダウンロードする
			await DownloadAssetBundleAsync( owner ) ;

			// 最初から展開しておきたいアセットを展開する
			await LoadConstantAsset( 2, owner ) ;

			//----------------------------------------------------------

			// 強制消去フラグを完結させる
			await RefreshClearFlag() ;

			//---------------------------------------------------------

			// Phase 2 完了
			ApplicationManager.DownloadingPhase2State = ApplicationManager.DownloadingPhaseStates.Completed ;
			Debug.Log( "<color=#00FF00>[Downloading] Phase 2 Completed !!</color>" ) ;

			//----------------------------------------------------------

			// 成功
			return true ;
		}
#endif

		// デバッグ用のリモートアセットバンドルパスを取得する(空で無効)
		private static string GetDebugRemotoAssetBundlePath()
		{
			string abCBKey = "AssetBundlePath_CheckBox" ;
			if( Preference.HasKey( abCBKey ) == false )
			{
				return null ;	// 無効
			}
			bool checkBox = Preference.GetValue<bool>( abCBKey ) ;
			if( checkBox == false )
			{
				return null ;	// 無効
			}

			string abIFKey = "AssetBundlePath_InputField" ;
			if( Preference.HasKey( abIFKey ) == false )
			{
				return null ;	// 無効
			}

			return Preference.GetValue<string>( abIFKey ) ;
		}
	}
}
