using System ;
using System.Collections.Generic ;
using System.Linq ;

using Template.Screens.BootClasses ;

using Cysharp.Threading.Tasks ;

using uGUIHelper ;

using UnityEngine ;

namespace Template.Screens
{
	/// <summary>
	/// 起動画面の制御クラス Version 2022/05/15 0
	/// </summary>
	public class Boot : ScreenBase
	{
		[SerializeField]
		protected	UIImage			m_Screen ;

		// 設定パネル
		[SerializeField]
		protected	UIView			m_SettingPanel ;

		//-----------------------------------
		// WebAPI のエンドポイント

		// エンドポイント入力
		[SerializeField]
		protected	UIInputField	m_EndPoint_Path ;

		// エンドポイント説明
		[SerializeField]
		protected	UITextMesh		m_EndPoint_Description ;

		// エンドポイント一覧
		[SerializeField]
		protected	UIDropdown		m_EndPoint_List ;

		[SerializeField]
		protected	UITextMesh		m_SystemVersion ;

		//-----------------------------------
		// アセットバンドルパスの強制指定

		[SerializeField]
		protected	UIToggle		m_AssetBundlePath_CheckBox ;

		[SerializeField]
		protected	UIInputField	m_AssetBundlePath_InputField ;

		//-----------------------------------
		// アカウント情報

		[SerializeField]
		protected	UIButton		m_AccountInfoButton ;

		//-----------------------------------
		// 画面遷移

		// 遷移先選択
		[SerializeField]
		protected	UIListView		m_Screen_List ;

		// 推奨ターゲット環境
		[SerializeField]
		protected	UITextMesh		m_TargetEndPoint ;

		//-----------------------------------

		// Awake の重い時に見えていけないものが見えてしまうのを防ぐ
		[SerializeField]
		protected	UIImage			m_Mask ;

		//-------------------------------------------------------------------------------------------

		// 遷移先の画面リスト
		private readonly ( string, string )[] m_ScreenNames =
		{
			( Scene.Screen.Title,				 "タイトル"					),
			( Scene.Screen.ChatClient,			 "チャットクライアント"		),
			( Scene.Screen.ChatServer,			 "チャットサーバー"			),
		} ;

		//-------------------------------------------------------------------------------------------

		protected override void OnAwake()
		{
			// ステータスバーを表示(Android の場合)
//			StatusBarController.Show() ;

			//----------------------------------------------------------

			// 見えてほしくない画面を隠す
			m_Mask.SetActive( true ) ;
		}

		protected override async UniTask OnStart()
		{
			//------------------------------------------------------------------------------------------
			// ストレージ空き容量のチェックを行う

//			long freeSize = StorageMonitor.GetFree() ;
//			string sizeName = ExString.GetSizeName( freeSize ) ;
//			await Dialog.Open( "ストレージ状況", "ストレージの空き容量は\n現在 <color=#FF7F00>" + sizeName + "</color> です", new string[]{ "閉じる" } ) ;

#if !UNITY_EDITOR && ( UNITY_ANDROID || UNITY_IOS )
			// 空き容量を確認する
			long requiredSize = 10 * 1024 * 1024 ;	// マスターデータ・ローカルのプレイヤーデータ保存・ローカルアセットバンドル複製分
			if( await ApplicationManager.CheckStorage( requiredSize ) == false )
			{
				return ;
			}
#endif
			//-------------------------------------------------------------------------------------------

#if false
			// ウェブビューの表示テスト
			bool isVisible = true ;
			WebView.Open( "https://www.google.co.jp", ( webViewObject, text ) =>
			{
				isVisible = false ;
			}, m_Screen ) ;
			await WaitWhile( () => isVisible ) ;
#endif
			//----------------------------------------------------------

			// 準備処理
			Prepare() ;

			//----------------------------------------------------------
			// 画面が暗転中にこの画面の準備を整える

			var settings = ApplicationManager.LoadSettings() ;

			if( Define.DevelopmentMode == true )
			{
				m_SettingPanel.SetActive( true ) ;

				//----------------------------------
				// エンドポイント

				string endPoint = string.Empty ;

				string endPointKey = "EndPoint" ;
				if( Preference.HasKey( endPointKey ) == true )
				{
					endPoint = Preference.GetValue<string>( endPointKey ) ;
				}

				var endPoints = settings.WebAPI_EndPoints ;

				List<string> endPointNames = new List<string>(){ "任意サーバー" } ;
				endPointNames.AddRange( endPoints.Select( _ => _.DisplayName ) ) ;

				List<string> endPointDescriptions = new List<string>{ "入力した任意の\nエンドポイントに接続します" } ;
				endPointDescriptions.AddRange( endPoints.Select( _ => _.Description ) ) ;

				string endPointAnyKey = "EndPointAny" ;
				if( string.IsNullOrEmpty( endPoint ) == true )
				{
					// 任意エンドポイントが保存されていたらロードする
					if( Preference.HasKey( endPointAnyKey ) == true )
					{
						endPoint = Preference.GetValue<string>( endPointAnyKey ) ;
					}
				}

				if( string.IsNullOrEmpty( endPoint ) == true )
				{
					// 空だったら推奨ターゲット環境を設定する
					endPoint = endPoints[ ( int )settings.WebAPI_DefaultEndPoint ].Path ;
				}

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
				//---------------------------------

				m_EndPoint_Path.Text = endPoint ;
				m_EndPoint_Description.Text = endPointDescriptions[ endPointIndex ] ;

				m_EndPoint_Path.SetOnEndEdit( ( string identity, UIInputField inputField, string text ) =>
				{
					if( string.IsNullOrEmpty( text ) == true )
					{
						_ = Dialog.Open( "", "エンドポイントが入力されていません", new string[]{ "閉じる" } ) ;
						text = endPoints[ ( int )settings.WebAPI_DefaultEndPoint ].Path ;
						m_EndPoint_Path.Text = text ;
					}

					int selectedIndex = GetIndex( text ) ;

					if( selectedIndex == 0 )
					{
						// 任意
						Preference.SetValue<string>( endPointAnyKey, text ) ;
						Preference.SetValue<string>( endPointKey, text ) ;
						Preference.Save() ;
					}

					m_EndPoint_List.Value = selectedIndex ;
				} ) ;

				m_EndPoint_List.Set( endPointNames.ToArray(), endPointIndex ) ;
				m_EndPoint_List.SetOnValueChanged( ( string identity, UIDropdown ropdown, int selectedIndex ) =>
				{
					string path = string.Empty ;
					if( selectedIndex == 0 )
					{
						// 任意

						// 任意エンドポイントが保存されていたらロードする
						if( Preference.HasKey( endPointAnyKey ) == true )
						{
							path = Preference.GetValue<string>( endPointAnyKey ) ;
						}
					}
					else
					{
						// プリセット
						path = endPoints[ selectedIndex - 1 ].Path ;
					}

					m_EndPoint_Path.Text = path ;

					Preference.SetValue<string>( endPointKey, path ) ;
					Preference.Save() ;

					m_EndPoint_Description.Text = endPointDescriptions[ selectedIndex ] ;
				} ) ;

				// システムバージョン
				m_SystemVersion.Text = "システムバージョン " + settings.SystemVersionName + " : リビジョン " + settings.Revision.ToString() ;

				//----------------------------------
				// アセットバンドルパス

				string abCBKey = "AssetBundlePath_CheckBox" ;
				if( Preference.HasKey( abCBKey ) == true )
				{
					m_AssetBundlePath_CheckBox.IsOn = Preference.GetValue<bool>( abCBKey ) ;
				}

				string abIFKey = "AssetBundlePath_InputField" ;
				if( Preference.HasKey( abIFKey ) == true )
				{
					m_AssetBundlePath_InputField.Text = Preference.GetValue<string>( abIFKey ) ;
				}
				else
				{
					string remoteAssetBundlePath = settings.RemoteAssetBundlePath ;
					if( string.IsNullOrEmpty( remoteAssetBundlePath ) == false )
					{
						// Setting にあるので設定する
						m_AssetBundlePath_CheckBox.IsOn = true ;
						m_AssetBundlePath_InputField.Text = remoteAssetBundlePath ;

						Preference.SetValue<bool>( abCBKey, true ) ;
						Preference.SetValue<string>( abIFKey, remoteAssetBundlePath ) ;
						Preference.Save() ;
					}
				}

				m_AssetBundlePath_CheckBox.Interactable = ! string.IsNullOrEmpty( m_AssetBundlePath_InputField.Text ) ;

				m_AssetBundlePath_CheckBox.SetOnValueChanged( ( string identity, UIToggle toggle, bool state ) =>
				{
					Preference.SetValue<bool>( abCBKey, state ) ;
					Preference.Save() ;
				} ) ;

				m_AssetBundlePath_InputField.SetOnEndEdit( ( string identity, UIInputField inputField, string text ) =>
				{
					if( string.IsNullOrEmpty( text ) == true )
					{
						m_AssetBundlePath_CheckBox.IsOn = false ;
						Preference.SetValue<bool>( abCBKey, false ) ;
						Preference.Save() ;
					}

					m_AssetBundlePath_CheckBox.Interactable = ! string.IsNullOrEmpty( text ) ;

					Preference.SetValue<string>( abIFKey, text ) ;
					Preference.Save() ;
				} ) ;

				//---------------------------------------------------------
				// スクリーン

				m_Screen_List.SetOnItemUpdated<ListView_Item>( ( identity, listView, index, component ) =>
				{
					if( component != null )
					{
						var item = component as ListView_Item ;
						item.SetStyle( $"{m_ScreenNames[ index ].Item2}へ遷移", index, OnSelected ) ;
					}
					return 0 ; // アイテムの縦幅はデフォルトを使用する
				} ) ;
				m_Screen_List.ItemCount = m_ScreenNames.Length ;	// アイテム数を設定する

				//----------------------------------
				// 推奨ターゲット環境

				m_TargetEndPoint.Text = endPointNames[ 1 + ( int )settings.WebAPI_DefaultEndPoint ] + "(推奨)アプリ" ;

				//-------------------------------------------------------------------------
			}
			else
			{
				m_SettingPanel.SetActive( false ) ;
			}

			//----------------------------------------------------------

			// マスク解除
			m_Mask.SetActive( false ) ;

			// フェードイン(画面表示)を許可する
			Scene.Ready = true ;

			// フェード完了を待つ
			await Scene.WaitForFading() ;

			//----------------------------------------------------------
			// ブート画面のデバッグ機能が無効なケースではそのままダウンロード経由でタイトル画面に遷移させる

			if( m_SettingPanel.ActiveSelf == false )
			{
				// エンドポイントを設定する(リリース)
//				PlayerData.EndPoint = settings.WebAPI_EndPoints[ ( int )settings.WebAPI_DefaultEndPoint ].Path ;
//				WebAPIManager.EndPoint = PlayerData.EndPoint ;

				ToStartScreen( Scene.Screen.Title ) ;
			}
		}

		// リストビューのスクリーンが選択されたら呼ばれる
		private void OnSelected( int index )
		{
			// エンドポイントを設定する(デバッグ)
//			PlayerData.EndPoint = m_EndPoint_Path.Text ;
//			WebAPIManager.EndPoint = PlayerData.EndPoint ;

			//----------------------------------------------------------

			// ダウンロード画面経由で選択された画面に遷移する
			ToStartScreen( m_ScreenNames[ index ].Item1 ) ;
		}

		//-------------------------------------------------------------------------------------

		// ダウンロードシーン経由で最初のシーンへ遷移する
		private void ToStartScreen( string startScreenName )
		{
			ApplicationManager.DownloadingRequestTypes downloadingRequestType = ApplicationManager.DownloadingRequestTypes.Phase2 ;

			if( startScreenName == Scene.Screen.Title )
			{
				// タイトルのみフェーズ１のダウンロードを行う
				downloadingRequestType = ApplicationManager.DownloadingRequestTypes.Phase1 ;
			}

			// ダウンロードのフェーズを保存する
			Scene.SetParameter( "DownloadingRequestType", downloadingRequestType ) ;

			// ダウンロード画面経由で選択された画面に遷移する
			_ = Scene.LoadWithFade( Scene.Screen.Downloading, "StartScreenName", startScreenName, blockingFadeIn:true ) ;
		}

		/// <summary>
		/// 各種準備
		/// </summary>
		private void Prepare()
		{
			//----------------------------------------------------------

			// ダウンロードの各フェーズの状態をクリアする
			ApplicationManager.DownloadingPhase1State = ApplicationManager.DownloadingPhaseStates.None ;
			ApplicationManager.DownloadingPhase2State = ApplicationManager.DownloadingPhaseStates.None ;
		}
	}
}
