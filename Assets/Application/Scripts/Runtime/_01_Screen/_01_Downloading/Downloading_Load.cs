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
		/// 最初から展開しておく必要のアセットを展開する(外部[Layout Dialog]から呼ばれる可能性があるので static メソッド)
		/// </summary>
		/// <returns></returns>
		public static async UniTask LoadConstantAsset( int phase, ExMonoBehaviour _ )
		{
			// 設定ファイルを読み出す
			var settings = ApplicationManager.LoadSettings() ;

			//--------------------------------------------------------------------------
			// Pahse 1

			if( ( phase & 1 ) != 0 )
			{
				Debug.Log( "<color=#00FF00>[Downloading] Phase 1 Excution</color>" ) ;

				//----------------------------------------------------------
				// ＡＤＸ２を完全に除去する

				AudioManager.ClearAll() ;

				//----------------------------------------------------------

			}

			//--------------------------------------------------------------------------
			// Pahse 2

			if( ( phase & 2 ) != 0 )
			{
				Debug.Log( "<color=#00FF00>[Downloading] Phase 2 Excution</color>" ) ;

				//---------------------------------------------------------
				// アセットバンドル側にあるシーンを展開できる状態にする

				if( AssetBundleManager.UseLocalAssets == false )
				{
					// シーンが切り替わっても破棄されない常駐させるアセットバンドルを展開する
					await SE.LoadAsync() ;

					ApplicationManager.LoadResidentAssetBundle() ;
				}

				//---------------------------------------------------------
			}

			await _.Yield() ;
		}
	}
}
