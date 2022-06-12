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
	/// 起動直後のダウンロード処理(マスターデータ・アセットバンドルの更新) Version 2022/04/24
	/// </summary>
	public partial class Downloading : ScreenBase
	{
		[Header( "各種パネル" )]

		[SerializeField]
		protected	ProgressPanel	m_ProgressPanel ;

		//-------------------------------------------------------------------------------------------

		override protected void OnAwake()
		{
			// 最初に見えてはいけない表示物を非表示にする

			m_ProgressPanel.View.SetActive( false ) ;
		}

		override protected async UniTask OnStart()
		{
			//----------------------------------------------------------
			// 画面が暗転中にこの画面の準備を整える

			// 設定を読み出す
			Settings settings = ApplicationManager.LoadSettings() ;

			// スリープ禁止
			Screen.sleepTimeout = SleepTimeout.NeverSleep ;

			//----------------------------------------------------------

			// フェードイン(画面表示)を許可する
			Scene.Ready = true ;

			// フェード完了を待つ
			await Scene.WaitForFading() ;

			//------------------------------------------------------------------------------------------

			// ダウンロードのリクエストタイプを取得する
			ApplicationManager.DownloadingRequestTypes downloadingRequestType =
				Scene.GetParameter<ApplicationManager.DownloadingRequestTypes>( "DownloadingRequestType" ) ;

			if( downloadingRequestType != ApplicationManager.DownloadingRequestTypes.Phase1 && downloadingRequestType != ApplicationManager.DownloadingRequestTypes.Phase2 )
			{
				Debug.LogError( "Unknown DownloadingRequestType : " + downloadingRequestType ) ;
				return ;
			}

			//----------------------------------------------------------
			// Phase 1

			if
			(
				ApplicationManager.DownloadingPhase1State == ApplicationManager.DownloadingPhaseStates.None &&
				( downloadingRequestType == ApplicationManager.DownloadingRequestTypes.Phase1 || downloadingRequestType == ApplicationManager.DownloadingRequestTypes.Phase2 )
			)
			{
				Progress.On( Progress.Styles.WebAPI, false, "準備中" ) ;

				//---------------------------------------------------------
#if false
				// マスターデータのダウンロード(CheckVersion があるのでアセットバンドルのセットアップより前に行う必要がある)
				bool result = await MasterDataManager.DownloadAsync( false, ( float progress ) =>
				{
//					SetProgress( progressMin, progressMax, progress ) ;
				} ) ;

				if( result == false )
				{
					// 失敗
					return ;
				}

				// マスターデータをメモリに展開する
				await MasterDataManager.LoadAsync() ;
#endif
				//---------------------------------------------------------

				// アセットバンドル全体共通の設定を行う
				SetupGeneralAssetBundleSettings() ;

				//---------------------------------------------------------
				// ローカルのアセットバンドル情報をロードする

				await SetupInternalAssetBundleSettings( this ) ;

				//---------------------------------------------------------

				// タイトル前から展開しておきたいアセットを展開する
				await LoadConstantAsset( 1, this ) ;

				//---------------------------------------------------------

				Progress.Off() ;

				//---------------------------------

				// Phase 1 完了
				ApplicationManager.DownloadingPhase1State = ApplicationManager.DownloadingPhaseStates.Completed ;
				Debug.Log( "<color=#00FF00>[Downloading] Phase 1 Completed !!</color>" ) ;
			}

			//------------------------------------------------------------------------------------------

			// Phase 2

			string startScreenName = Scene.GetParameter<string>( "StartScreenName" ) ;

			if
			(
				ApplicationManager.DownloadingPhase2State == ApplicationManager.DownloadingPhaseStates.None &&
				downloadingRequestType == ApplicationManager.DownloadingRequestTypes.Phase2
			)
			{
				//---------------------------------------------------------
				// このタイミングでログインしていないければログインする(本来はタイトル画面で行われている)
#if false
				// ただしログインを無視しないシーンであれば
				if( PlayerData.IsLogin == false )
				{
					// 未ログインであればここでログインする
					if( await PlayerDataManager.Login() == false )
					{
						return ;	// ログイン出来なければこれ以上先には進ませない
					}
					Debug.Log( "<color=\"#00FFFF\">[Downloading] Login OK !!</color>" ) ;
				}
#endif
				//---------------------------------------------------------
				// リモートのアセットバンドル情報をダウンロードする

				Progress.On( Progress.Styles.WebAPI, false, "データ確認中" ) ;

//				await SetupExternalAssetBundleSettings( this ) ;

				Progress.Off() ;

				//---------------------------------------------------------
				// リモートのアセットバンドルを必要に応じて消去する

				ClearConstantAssetBundle() ;

				//-----------------------------------------------------------------------------------------
#if false
				// 実際にダウンロードを行う
				if( await Process() == false )
				{
					// キャンセルしてタイトル画面に戻る

					// スリープ許可
					Screen.sleepTimeout = SleepTimeout.SystemSetting ;

					Blocker.Off() ;

					// タイトル画面へ
					Scene.LoadWithFade( Scene.Screen.Title, blockingFadeIn:true, fadeType:Fade.FadeTypes.Color ).Forget() ;

					throw new OperationCanceledException() ;	// タスクキャンセル
				}
#endif
				//-----------------------------------------------------------------------------------------
				// ダウンロード完了後

				// タイトル前から展開しておきたいアセットを展開する(再ダウンロードの都合で全て破棄されてしまうのでもう一度ロードする)
				await LoadConstantAsset( 1, this ) ;

				// 最初から展開しておきたいアセットを展開する
				await LoadConstantAsset( 2, this ) ;

				//---------------------------------------------------------

				// 強制消去フラグを完結させる
				await RefreshClearFlag() ;

				//---------------------------------------------------------

				// Phase 2 完了
				ApplicationManager.DownloadingPhase2State = ApplicationManager.DownloadingPhaseStates.Completed ;
				Debug.Log( "<color=#00FF00>[Downloading] Phase 2 Completed !!</color>" ) ;
			}

			//------------------------------------------------------------------------------------------

			// スリープ許可
			Screen.sleepTimeout = SleepTimeout.SystemSetting ;

			//------------------------------------------------------------------------------------------

			// 終わったら指定のシーンに遷移する
			if( string.IsNullOrEmpty( startScreenName ) == true || startScreenName == Scene.Screen.Downloading || startScreenName == Scene.Screen.Title )
			{
				// いきなりこのシーンから始まった場合のケース(特殊なケースはダウンロードとタイトルのみ)
				// タイトルへ
				startScreenName = Scene.Screen.Title ;
			}

			Debug.Log( "<color=#FFFFFF>ダウンロードフェーズ後の遷移先のシーン:" + startScreenName + "</color>" ) ;

			Blocker.Off() ;

			Scene.LoadWithFade( startScreenName, blockingFadeIn:true, fadeType:Fade.FadeTypes.Color ).Forget() ;
		}
	}
}
