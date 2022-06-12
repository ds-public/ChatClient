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

namespace Template.Screens
{
	/// <summary>
	/// 起動直後のダウンロード処理
	public partial class Downloading
	{
		/// <summary>
		/// 実際にダウンロードを行う
		/// </summary>
		/// <returns></returns>
		private async UniTask<bool> Process()
		{
			// 必要なアセットバンドルを取得する(チュートリアル終了前はチュートリアルで必要な分のみ取得する)
			var targetAssetBundlePaths = GetTargetAssetBundlePaths() ;
			if( targetAssetBundlePaths == null || targetAssetBundlePaths.Count == 0 )
			{
				// 必要なアセットバンドルは全て揃っている
				return true ;	// 処理を継続して良い
			}

			//------------------------------------------------------------------------------------------

			//----------------------------------------------------------

			// 改めてダウンロードするアセットバンドルを取得する(ムービーが除外されている)
			targetAssetBundlePaths = GetTargetAssetBundlePaths() ;
			if( targetAssetBundlePaths == null || targetAssetBundlePaths.Count == 0 )
			{
				// 必要なアセットバンドルは全て揃っている(ダウンロードは不要)
				return true ;	// 処理を継続して良い
			}

			// ダウンロード確認
			if( await OpenDownloadConfirmDialog( targetAssetBundlePaths ) == false )
			{
				// キャンセル
				return false ;
			}

			//----------------------------------------------------------

			// チュートリアル前ではムービー再生ダウンロード
			// ムービー再生中にダウンロードが終わらなかった場合はロアに切り替わる

			// チュートリアル後では最初からロアダウンロード

			await Execute( targetAssetBundlePaths, "ゲームを開始できます" ) ;

			// 処理継続
			return true ;
		}

		//-------------------------------------------------------------------------------------------

		// アセットバンドル群のダウンロードを実行する
		private async UniTask Execute( Dictionary<string,AssetBundleManager.DownloadEntity> targetAssetBundlePaths, string completedMessage )
		{
			// プログレスバネルを準備する
			m_ProgressPanel.Prepare() ;

			//----------------------------------

			// プログレスをフェードインする
			await m_ProgressPanel.FadeIn() ;

			// ダウンロード実行
			bool downloadCompleted = false ;

			//--------------

			// 指定したアセットバンドルをダウンロードする
			_ = DownloadAssetBundleAsync
			(
				this, targetAssetBundlePaths,
				( long downloadedSize, int writtenSize, long totalSize, int storedFile, int totalFile, AssetBundleManager.DownloadEntity[] targets, int nowParallel, int maxParallel, int httpVersion ) =>
				{
					m_ProgressPanel.Set( downloadedSize, writtenSize, totalSize, storedFile, totalFile, targets, nowParallel, maxParallel, httpVersion ) ;
				},
				() =>
				{
					// ダウンロード完了
					downloadCompleted = true ;
				}
			) ;

			//----------------------------------------------------------

			Blocker.Off() ;

			// ダウンロード完了まで待機する
			while( true )
			{
				if( downloadCompleted == true )
				{
					// ダウンロード完了
					break ;
				}

				await Yield() ;	// これが無いとフリーズするので注意
			}

			//----------------------------------------------------------
			// ダウンロード終了

			// ダウンロード終了の形に表示を変える
			await m_ProgressPanel.Complete( completedMessage ) ;

			// ダウンロードが終了したのでムービー再生中であればスキップボタンを出す

			await WhenAll
			(
				m_ProgressPanel.FadeOut()
			) ;

			//----------------------------------------------------------
			// 画面はそのままに黒フェードさせる

			Blocker.On() ;
		}

		//-------------------------------------------------------------------------------------------

		// アセットバンドルをダウンロードして良いか確認する
		private async UniTask<bool> OpenDownloadConfirmDialog( Dictionary<string,AssetBundleManager.DownloadEntity> targetAssetBundlePaths )
		{
			// ダウンロードに必要なサイズ
			long totalSize = GetTotalDownloadSize( targetAssetBundlePaths ) ;

			if( totalSize <  0 )
			{
				Blocker.Off() ;
				await Dialog.Open( "注意", "ダウンロードサイズが異常です\n\nタイトル画面に戻ります" + totalSize, new string[]{ "閉じる" } ) ;
				Blocker.On() ;
				return false ;
			}

			//------------------------------------------------------------------------------------------
			// ストレージ空き容量のチェックを行う

#if !UNITY_EDITOR && ( UNITY_ANDROID || UNITY_IOS )
			// 空き容量を確認する
			if( await ApplicationManager.CheckStorage( totalSize ) == false )
			{
				return false ;
			}
#endif
			//----------------------------------------------------------

			// ダウンロードを行ってよいか確認する
			string message = "<color=#FF7F00>" + ExString.GetSizeName( totalSize ) + " </color>のデータを\nダウンロードします\n\nよろしいですか？" ;

			Blocker.Off() ;
			int index = await Dialog.Open( "ダウンロード確認", message, new string[]{ "ダウンロード", "キャンセル" } ) ;
			Blocker.On() ;
			if( index == 1 )
			{
				// ダウンロードキャンセルでタイトルへ戻る
				return false ;
			}

			// ダウンロード実行
			return true ;
		}


	}
}
