using System ;
using System.IO ;
using System.Collections.Generic ;
using System.Linq ;
using UnityEngine ;
using UnityEngine.SceneManagement ;
using UnityEditor ;
using UnityEditor.SceneManagement ;
using UnityEditor.Experimental.SceneManagement ;	// 将来的に変更または削除される可能性がある事に注意

/// <summary>
/// シンプルリファレンスディテクター
/// </summary>
namespace Tools.ForAssets
{
	/// <summary>
	/// シンプルリファレンスディテクタークラス(エディター用) Version 2021/09/14
	/// </summary>
	public class SimpleReferenceDetector : EditorWindow
	{
		[ MenuItem( "Tools/Simple Reference Detector(指定アセットへの参照検出)" ) ]
		[ MenuItem( "Assets/Simple Reference Detector(指定アセットへの参照検出)" ) ]
		internal static void OpenWindow()
		{
			var window = EditorWindow.GetWindow<SimpleReferenceDetector>( false, "Reference Detector", true ) ;
			window.minSize = new Vector2( 640, 640 ) ;

			//----------------------------------------------------------

			if( Selection.objects != null && Selection.objects.Length == 1 && Selection.activeObject != null )
			{
				// １つだけ選択（複数選択には対応していない：フォルダかファイル）
				if( AssetDatabase.Contains( Selection.activeObject ) == true )
				{
					string path = AssetDatabase.GetAssetPath( Selection.activeObject ) ;
					path = path.Replace( "\\", "/" ).TrimStart( '/' ).TrimEnd( '/' ) ;
					if( File.Exists( path ) == true )
					{
						// ファイル限定
						window.SetTargetAssetPath( path ) ;
					}
				}
			}
		}

		//---------------------------------------------------------------------------------------------------------------------------

		// 選択中のファイルが変更された際に呼び出される
		internal void OnSelectionChange()
		{
			Repaint() ;
		}

		// 描画
		internal void OnGUI()
		{
			DrawGUI() ;
		}

		//---------------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// 初期の検査対象のパスを設定する
		/// </summary>
		/// <param name="targetAssetPath"></param>
		public void SetTargetAssetPath( string targetAssetPath )
		{
			m_TargetAssetPath = targetAssetPath ;
		}

		/// <summary>
		/// 検査対象のパス
		/// </summary>
		[SerializeField]
		private string									m_TargetAssetPath ;

		/// <summary>
		/// 参照を検査する範囲のルートフォルダのパス
		/// </summary>
		[SerializeField]
		private List<string>							m_SearchTargetPaths = new List<string>(){ "Assets" } ;

		/// <summary>
		/// 参照を検査するファイル一覧
		/// </summary>
		[SerializeField]
		private List<string>							m_SearchTargetFiles ;


		/// <summary>
		/// 貼り直し対象情報(ファイル単位)
		/// </summary>
		[Serializable]
		public class ReferenceAssetData
		{
			public	string								AssetPath ;
			public	string								AssetType ;				// Asset Prefab Scene
			public	List<ReferenceAssetReferenceData>	References = new List<ReferenceAssetReferenceData>() ;
		}

		/// <summary>
		/// 発見した参照情報
		/// </summary>
		[Serializable]
		public class ReferenceAssetReferenceData
		{
			public	string								HierarchyPath ;			// シーンまたはプレハブ内部のパス
			public	string								ComponentType ;			// コンポーネントの型
			public	string								PropertyPath ;			// プロパティのパス
			public	string								PropertyType ;			// プロパティの型
			public	string								ReferencedAssetPath ;	// 参照している不要なアセットのパス
		}

		[SerializeField]
		private List<ReferenceAssetData>				m_ReferenceAssets ;

		//-----------------------------------

		private int										m_SelectedActiveIndex = -1 ;

		private int										m_SelectedDetailIndex = -1 ;
		private int										m_SelectedWindowIndex =  0 ;


		private Vector2									m_ScrollPosition_VF0 ;
		private Vector2									m_ScrollPosition_VF1 ;
		private Vector2									m_ScrollPosition_BD ;

		private UnityEngine.Object						m_ActiveObject = null ;

		//-------------------------------------------------------------------------------------------

		// アセット種別の識別色
		private readonly Dictionary<string, Color> m_AssetTypeColors = new Dictionary<string, Color>()
		{
			{ "Asset",	new Color32( 255, 127,   0, 255 )	},
			{ "Prefab", Color.cyan	},
			{ "Scene",	Color.green	},
		} ;

		// Project タブを表示する
		private void DrawGUI()
		{
			GUIStyle textStyle = new GUIStyle( EditorStyles.textField ) ;

			// １フレーム遅らせてアクティブオブジェクトを設定する
			if( m_ActiveObject != null )
			{
				Selection.activeObject = m_ActiveObject ;
				m_ActiveObject = null ;
			}

			//----------------------------------------------------------

			int i, l ;

			bool isRefresh = false ;

			GUILayout.Space( 6f ) ;
			EditorGUILayout.HelpBox( "参照を探し出したいアセットファイルを選択して[Target AssetPath]ボタンを押してください", MessageType.Info ) ;

			GUILayout.BeginHorizontal() ;
			{
				// 保存パスを選択する
				GUI.backgroundColor = new Color( 0, 1, 1, 1 ) ;
				if( GUILayout.Button( "Target AssetPath", GUILayout.Width( 120f ) ) == true )
				{
					string targetAssetPath = m_TargetAssetPath ;

					if( Selection.objects != null && Selection.objects.Length == 1 && Selection.activeObject != null )
					{
						// １つだけ選択（複数選択には対応していない：フォルダかファイル）
						if( AssetDatabase.Contains( Selection.activeObject ) == true )
						{
							string path = AssetDatabase.GetAssetPath( Selection.activeObject ) ;
							path = path.Replace( "\\", "/" ).TrimStart( '/' ).TrimEnd( '/' ) ;
							if( File.Exists( path ) == true )
							{
								// ファイル限定
								targetAssetPath = path ;
							}
						}
					}

					// 複数選択している場合は何もしない

					//----------------------------------------------------------------

					if( m_TargetAssetPath != targetAssetPath )
					{
						// 対象のルートパスを更新する
						m_TargetAssetPath = targetAssetPath ;

						isRefresh = true ;
					}
				}
				GUI.backgroundColor = Color.white ;

				//---------------------------------------------------------

				// ルートフォルダ
				EditorGUILayout.TextField( m_TargetAssetPath ) ;

				//---------------------------------------------------------

				// 対象のパスを消去する(全 Asset 対象)
				GUI.backgroundColor = new Color32( 255,   0,   0, 255 ) ;
				if( GUILayout.Button( "Clear", GUILayout.Width( 100f ) ) == true )
				{
					// 選択中のアセットを無しにする
					Selection.activeObject = null ;
					m_TargetAssetPath = string.Empty ;

					isRefresh = true ;
				}
				GUI.backgroundColor = Color.white ;
			}
			GUILayout.EndHorizontal() ;

			//------------------------------------------------------------------

			GUILayout.Space( 6f ) ;
			EditorGUILayout.HelpBox( "参照を検査するフォルダを選択して[Increase All]ボタンを押してください", MessageType.Info ) ;

			if( m_SearchTargetPaths != null && m_SearchTargetPaths.Count >  0 )
			{
				// 有効参照範囲の指定がある

				GUILayout.Space( 6f ) ;

				// 列見出し  
				EditorGUILayout.BeginHorizontal() ;
				EditorGUILayout.LabelField( "Search Target Paths (" + m_SearchTargetPaths.Count + ")", GUILayout.Width( 640 ) ) ;
				EditorGUILayout.EndHorizontal() ;

				// リスト項目削除インデックス
				int deleteIndex = -1 ;

				// リスト表示  
				m_ScrollPosition_VF0 = EditorGUILayout.BeginScrollView( m_ScrollPosition_VF0, GUILayout.Height( 60 ) ) ;
				{
					l = m_SearchTargetPaths.Count ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						string path = m_SearchTargetPaths[ i ] ;

						EditorGUILayout.BeginHorizontal() ;
						{
							if( GUILayout.Button( "-", GUILayout.Width( 25 ) ) == true )
							{
								deleteIndex = i ;
							}

							EditorGUILayout.TextField( path ) ;
						}
						EditorGUILayout.EndHorizontal() ;
					}
				}
				EditorGUILayout.EndScrollView() ;

				if( deleteIndex >= 0 )
				{
					// 項目削除
					m_SearchTargetPaths.RemoveAt( deleteIndex ) ;
					isRefresh = true ;
				}

				if( m_SearchTargetPaths.Count >  0 )
				{
					// 対象のパスを消去する(全 Asset 対象)
					GUI.backgroundColor = new Color32( 255,   0,   0, 255 ) ;
					if( GUILayout.Button( "Decrease All", GUILayout.Width( 100f ) ) == true )
					{
						// 選択中のアセットを無しにする
						Selection.activeObject = null ;

						m_SearchTargetPaths = null ;
						m_ReferenceAssets = null ;

						isRefresh = true ;
					}
					GUI.backgroundColor = Color.white ;
				}
			}
			else
			{
				m_ScrollPosition_VF0 = Vector2.zero ;
			}

			//----------------------------------------------------------
			
			List<string> searchTargetPaths = new List<string>() ;
			if( Selection.objects != null && Selection.objects.Length >  0 )
			{
				l = Selection.objects.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( Selection.objects[ i ] != null && AssetDatabase.Contains( Selection.objects[ i ] ) == true )
					{
						string path = AssetDatabase.GetAssetPath( Selection.objects[ i ] ) ;
						path = path.Replace( "\\", "/" ).TrimStart( '/' ).TrimEnd( '/' ) ;

						if( path != m_TargetAssetPath )
						{
							// 有効なパス
							if( m_SearchTargetPaths == null || m_SearchTargetPaths.Count == 0  )
							{
								searchTargetPaths.Add( path ) ;
							}
							else
							if( m_SearchTargetPaths != null && m_SearchTargetPaths.Count >  0 )
							{
								if( m_SearchTargetPaths.Contains( path ) == false )
								{
									searchTargetPaths.Add( path ) ;
								}
							}
						}
					}
				}
			}

			// 候補の表示
			if( searchTargetPaths != null && searchTargetPaths.Count >  0 )
			{
				GUILayout.Space( 6f ) ;

				// リスト表示  
				m_ScrollPosition_VF1 = EditorGUILayout.BeginScrollView( m_ScrollPosition_VF1, GUILayout.Height( 60 ) ) ;
				{
					l = searchTargetPaths.Count ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						string path = searchTargetPaths[ i ] ;

						EditorGUILayout.BeginHorizontal() ;
						{
							if( GUILayout.Button( "+", GUILayout.Width( 25 ) ) == true )
							{
								if( m_SearchTargetPaths == null )
								{
									m_SearchTargetPaths = new List<string>() ;
								}
								m_SearchTargetPaths.Add( path ) ;

								isRefresh = true ;
							}
							EditorGUILayout.TextField( path ) ;
						}
						EditorGUILayout.EndHorizontal() ;
					}
				}
				EditorGUILayout.EndScrollView() ;

				// 対象のパスを消去する(全 Asset 対象)
				GUI.backgroundColor = new Color32(   0, 255, 255, 255 ) ;
				if( GUILayout.Button( "Increase All", GUILayout.Width( 100f ) ) == true )
				{
					// 選択中の項目を全て追加する

					if( m_SearchTargetPaths == null )
					{
						m_SearchTargetPaths = new List<string>() ;
					}

					l = searchTargetPaths.Count ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						string path = searchTargetPaths[ i ] ;

						m_SearchTargetPaths.Add( path ) ;
					}

					isRefresh = true ;
				}
				GUI.backgroundColor = Color.white ;
			}
			else
			{
				m_ScrollPosition_VF1 = Vector2.zero ;
			}

			//------------------------------------------------------------------------------------------

			if( string.IsNullOrEmpty( m_TargetAssetPath ) == false && m_SearchTargetPaths != null && m_SearchTargetPaths.Count >  0 )
			{
				// 準備が整ったのだ検査ボタンを表示する
				GUILayout.Space( 6f ) ;
				EditorGUILayout.HelpBox( "参照の検出を開始するには[Search]ボタンを押します", MessageType.Info ) ;

				// 検査実行ボタン
				GUI.backgroundColor = new Color32(   0, 255,   0, 255 ) ;
				if( GUILayout.Button( "Search"  ) == true )
				{
					isRefresh = true ;
				}
				GUI.backgroundColor = Color.white ;

				//------------------------------------------------------------------------------------------

				// 検査
				if( isRefresh == true )
				{
					bool execute = true ;
					if( m_SearchTargetPaths.Contains( "Assets" ) == true )
					{
						if( EditorUtility.DisplayDialog( "注意", "参照を検査する対象範囲に\nAssets フォルダが含まれています\nAssets フォルダが含まれる場合\nUnityプロジェクトの全てのアセットファイルに対し\n参照の検査が行われる事になり\nUnityEditorの操作が\nしばらく出来なくなる恐れがあります。\n\nそれでも参照の検査を実行しますか？", "Yes", "No" ) == false )
						{
							execute = false ;
						}
					}

					if( execute == true )
					{
						m_ReferenceAssets = Search( m_TargetAssetPath, m_SearchTargetPaths ) ;
						m_SelectedDetailIndex = -1 ;
						m_SelectedWindowIndex =  0 ;
					}
				}

				//------------------------------------------------------------------------------------------

				// 検出結果

				if( m_ReferenceAssets != null && m_ReferenceAssets.Count >  0 )
				{
					GUILayout.Space( 6f ) ;

					string activeAssetPath = string.Empty ;
					if( Selection.objects != null && Selection.objects.Length == 1 && Selection.activeObject != null )
					{
						// １つだけ選択（複数選択には対応していない：フォルダかファイル）
						activeAssetPath = AssetDatabase.GetAssetPath( Selection.activeObject.GetInstanceID() ) ;
					}

					// 列見出し  
					EditorGUILayout.BeginHorizontal() ;
					{
						string count = String.Empty ;
						if( m_ReferenceAssets != null && m_ReferenceAssets.Count >  0 )
						{
							int total = m_ReferenceAssets.Sum( _ => _.References.Count ) ;
							count = " (" + m_ReferenceAssets.Count + ":" + total + ")" ;
						}

						EditorGUILayout.LabelField( "References " + count, GUILayout.Width( 640 ) ) ;
					}
					EditorGUILayout.EndHorizontal() ;

					// リスト表示  
					m_ScrollPosition_BD = EditorGUILayout.BeginScrollView( m_ScrollPosition_BD ) ;
					{
						l = m_ReferenceAssets.Count ;
						for( i  = 0 ; i <  l ; i ++ )
						{
							var referenceAsset = m_ReferenceAssets[ i ] ;

							EditorGUILayout.BeginHorizontal() ;
							{
								// 詳細
								if( m_SelectedDetailIndex >= 0 && i == m_SelectedDetailIndex )
								{
									GUI.backgroundColor = new Color32(   0, 255, 255, 255 ) ;
								}
								else
								{
									GUI.backgroundColor = Color.white ;
								}
								if( GUILayout.Button( "Detail", GUILayout.Width( 60 ) ) == true )
								{
									m_SelectedDetailIndex = i ;
									m_SelectedWindowIndex = 0 ;
								}
								GUI.backgroundColor = Color.white ;

								// アセット種別(Asset Prefab Scene)
								GUI.contentColor = m_AssetTypeColors[ referenceAsset.AssetType ] ;
								textStyle.alignment = TextAnchor.MiddleCenter ;
								EditorGUILayout.TextField( referenceAsset.AssetType, textStyle, GUILayout.Width( 60 ) ) ;
								GUI.contentColor = Color.white ;

								// 参照数
								textStyle.alignment = TextAnchor.MiddleCenter ;
								EditorGUILayout.TextField( referenceAsset.References.Count.ToString(), textStyle, GUILayout.Width( 25 ) ) ;

								// 強調
								if( m_SelectedActiveIndex >= 0 && i == m_SelectedActiveIndex && string.IsNullOrEmpty( activeAssetPath ) == false && activeAssetPath == referenceAsset.AssetPath )
								{
									GUI.backgroundColor = new Color32( 255, 127, 255, 255 ) ;
								}
								else
								{
									GUI.backgroundColor = Color.white ;
								}
								if( GUILayout.Button( ">", GUILayout.Width( 25 ) ) == true )
								{
									UnityEngine.Object asset = AssetDatabase.LoadMainAssetAtPath( referenceAsset.AssetPath ) ;
									if( asset != null )
									{
										Selection.activeObject = asset ;
									}
									m_SelectedActiveIndex = i ;
								}
								GUI.backgroundColor = Color.white ;

								// パス
								if( string.IsNullOrEmpty( activeAssetPath ) == false && activeAssetPath == referenceAsset.AssetPath )
								{
									GUI.contentColor = new Color32( 255, 127, 255, 255 ) ;
								}
								else
								{
									GUI.contentColor = Color.white ;
								}
								EditorGUILayout.TextField( referenceAsset.AssetPath ) ;
								GUI.contentColor = Color.white ;

							}
							EditorGUILayout.EndHorizontal() ;
						}
					}
					EditorGUILayout.EndScrollView() ;

					//---------------------------------------------------------
					// 詳細の表示

					if( m_SelectedDetailIndex >= 0 )
					{
						// 詳細情報を表示する

						ReferenceAssetData referenceAsset = m_ReferenceAssets[ m_SelectedDetailIndex ] ;
						ReferenceAssetReferenceData	item = referenceAsset.References[ m_SelectedWindowIndex ] ;

						EditorGUILayout.BeginHorizontal() ;
						{
							// ページング
							i = 1 + m_SelectedWindowIndex ;
							l = referenceAsset.References.Count ;

							if( GUILayout.Button( "←", GUILayout.Width( 25 ) ) == true )
							{
								m_SelectedWindowIndex = ( m_SelectedWindowIndex - 1 + l ) % l ;
							}

							textStyle.alignment = TextAnchor.MiddleCenter ;
							EditorGUILayout.TextField( i + " / " + l, textStyle, GUILayout.Width( 50 ) ) ;

							if( GUILayout.Button( "→", GUILayout.Width( 25 ) ) == true )
							{
								m_SelectedWindowIndex = ( m_SelectedWindowIndex + 1 + l ) % l ;
							}

							//------------------------------

							GUILayout.Label( "AssetPath", GUILayout.Width( 70f ) ) ;

							if( GUILayout.Button( ">", GUILayout.Width( 25 ) ) == true )
							{
								UnityEngine.Object activeObject = AssetDatabase.LoadMainAssetAtPath( referenceAsset.AssetPath ) ;
								if( activeObject != null )
								{
									Selection.activeObject = activeObject ;
								}
							}

							GUI.contentColor = new Color32( 255, 255, 127, 255 ) ;
							EditorGUILayout.TextField( referenceAsset.AssetPath ) ;
							GUI.contentColor = Color.white ;
						}
						EditorGUILayout.EndHorizontal() ;

						EditorGUILayout.BeginHorizontal() ;
						{
							GUILayout.Label( "AssetType", GUILayout.Width( 70f ) ) ;
							GUI.contentColor = m_AssetTypeColors[ referenceAsset.AssetType ] ;
							textStyle.alignment = TextAnchor.MiddleCenter ;
							EditorGUILayout.TextField( referenceAsset.AssetType, textStyle, GUILayout.Width( 60f ) ) ;
							GUI.contentColor = Color.white ;

							if( string.IsNullOrEmpty( item.HierarchyPath ) == false )
							{
								GUILayout.Label( "HierarchyPath", GUILayout.Width( 90f ) ) ;

								if( GUILayout.Button( ">", GUILayout.Width( 25 ) ) == true )
								{
									if( referenceAsset.AssetType == "Prefab" )
									{
										GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>( referenceAsset.AssetPath ) ;
										AssetDatabase.OpenAsset( prefab ) ;

										//-----------------------------------------------------
										// まだ試験導入中のパッケージを使っている事に注意
										var stage = PrefabStageUtility.GetCurrentPrefabStage() ;
										if( stage != null )
										{
											Debug.LogWarning( "[注意]試験導入機能を用いてプレハブモード内のオブジェクトを取得する\n" + item.HierarchyPath ) ;
											prefab = stage.prefabContentsRoot ;	// プレハブモード内のルートのゲームオブジェクト
										}
										//-----------------------------------------------------

										m_ActiveObject = GetGameObjectByHierarchyPath( item.HierarchyPath, prefab ) as UnityEngine.Object ;
									}
									else
									if( referenceAsset.AssetType == "Scene" )
									{
										// 必ず一旦シーン自体をアクティブにすべし
										UnityEngine.Object activeObject = AssetDatabase.LoadMainAssetAtPath( referenceAsset.AssetPath ) ;
										if( activeObject != null )
										{
											Selection.activeObject = activeObject ;
										}

										// 必ず強制ロードする
										bool isSceneLoaded = true ;
										var scene = EditorSceneManager.OpenScene( referenceAsset.AssetPath ) ;
										if( scene == null )
										{
											isSceneLoaded = false ;
										}

										if( isSceneLoaded == true )
										{
											m_ActiveObject = GetGameObjectByHierarchyPath( item.HierarchyPath ) as UnityEngine.Object ;
										}
									}
								}

								GUI.contentColor = new Color32( 192, 255, 192, 255 ) ;
								EditorGUILayout.TextField( item.HierarchyPath ) ;
								GUI.contentColor = Color.white ;
							}
						}
						EditorGUILayout.EndHorizontal() ;

						//--------------------------------

						EditorGUILayout.BeginHorizontal() ;
						{
							GUILayout.Label( "ComponentType", GUILayout.Width( 105f ) ) ;
							EditorGUILayout.TextField( item.ComponentType ) ;
						}
						EditorGUILayout.EndHorizontal() ;

						//--------------------------------

						EditorGUILayout.BeginHorizontal() ;
						{
							GUILayout.Label( "PropertyType", GUILayout.Width( 105f ) ) ;
							GUI.contentColor = new Color32( 255, 127,   0, 255 ) ;
							EditorGUILayout.TextField( item.PropertyType, GUILayout.Width( 200f ) ) ;
							GUI.contentColor = Color.white ;

							GUILayout.Label( "PropertyPath", GUILayout.Width( 85f ) ) ;
							GUI.contentColor = new Color32( 192, 255, 255, 255 ) ;
							EditorGUILayout.TextField( item.PropertyPath ) ;
							GUI.contentColor = Color.white ;
						}
						EditorGUILayout.EndHorizontal() ;
					}
				}
				else
				{
					GUILayout.Label( "No Reference", GUILayout.Width( 320f ) ) ;

					m_ScrollPosition_BD = Vector2.zero ;
					m_SelectedWindowIndex =  0 ;
					m_SelectedDetailIndex = -1 ;
					m_SelectedActiveIndex = -1 ;
				}
			}
		}


		//-------------------------------------------------------------------------------------------

		// 検査から除外する拡張子(他のアセットへの参照をを持ち得ないもの)
		private readonly string[] m_ExclusionExtensions =
		{
			".meta",	// 元々無関係
			".cs", ".js",
			".png", ".jpg", ".tga", ".psd", ".gif", ".bmp", ".tif", ".tiff", ".iff", ".pict", ".exr",
			".wav", ".ogg", ".mp3", ".aif", ".aiff", ".xm", ".mod", ".it", ".s3m",
			".acf", ".acb", ".awb", ".cpk",
			".txt", ".json", ".bytes", ".csv", ".html", ".xml",  ".yml", ".htm", ".fnt",
			".anim",
			".ttf", ".otf", ".dfont",
			".fbx", ".dae", ".obj", ".max", ".blend",
			".shader",
			".mask",
			".mp4", ".mov", ".asf", ".avi", ".mpg", ".mpeg"
		} ;

		// 検査対象のファイル一覧を取得する
		private List<string> GetSearchTargetFiles( string targetAssetPath, List<string> searchTargetPaths )
		{
			List<string> searchTargetFiles = new List<string>() ;

			foreach( var path in searchTargetPaths )
			{
				if( File.Exists( path ) == true )
				{
					AddValidPath( targetAssetPath, path, ref searchTargetFiles ) ;
				}
				else
				if( Directory.Exists( path ) == true )
				{
					GetSearchTargetFiles( targetAssetPath, path, ref searchTargetFiles ) ;
				}
			}

			return searchTargetFiles ;
		}

		private void GetSearchTargetFiles( string targetAssetPath, string currentPath, ref List<string> searchTargetFiles )
		{
			int i, l ;

			// ファイルを検査する
			string[] files = Directory.GetFiles( currentPath ) ;
			if( files != null && files.Length >  0 )
			{
				string file ;
				l = files.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					file = files[ i ] ;
					file = file.Replace( "\\", "/" ).TrimStart( '/' ).TrimEnd( '/' ) ;

					AddValidPath( targetAssetPath, file, ref searchTargetFiles ) ;
				}
			}

			// フォルダを検査する
			string[] folders = Directory.GetDirectories( currentPath ) ;
			if( folders != null && folders.Length >  0 )
			{
				string folder ;
				l = folders.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					folder = folders[ i ] ;
					folder = folder.Replace( "\\", "/" ).TrimStart( '/' ).TrimEnd( '/' ) ;

					GetSearchTargetFiles( targetAssetPath, folder, ref searchTargetFiles ) ;
				}
			}
		}

		// 最終的に有効なファイルであれば検査対象リストに追加する
		private bool AddValidPath( string targetAssetPath, string path, ref List<string> searchTargetFiles )
		{
			string extension = Path.GetExtension( path ) ;
			if( string.IsNullOrEmpty( extension ) == true )
			{
				return false ;	// 拡張子なしは対象外
			}

			extension = extension.ToLower() ;
			if( m_ExclusionExtensions.Contains( extension ) == true )
			{
				return false ;	// 除外対象の拡張子
			}

			if( searchTargetFiles.Contains( path ) == true )
			{
				return false ;	// 既に追加済み
			}

			searchTargetFiles.Add( path ) ;

			return true ;
		}

		//-------------------------------------------------------------------------------------------
		// 検査

		// 有効範囲外のアセットを参照している箇所がないか検査する
		private List<ReferenceAssetData> Search( string targetAssetPath, List<string> searchTargetPaths )
		{
			// 検査するファイル群を取得する
			List<string> searchTargetFiles = GetSearchTargetFiles( targetAssetPath, searchTargetPaths ) ;
			if( searchTargetFiles == null || searchTargetFiles.Count == 0 )
			{
				return null ;	// 検査対象が存在しない
			}

			//------------------------------------------------------------------

			Dictionary<string, ReferenceAssetData> referenceAssets = new Dictionary<string, ReferenceAssetData>() ;

			//------------------------------------------------------------------
			// 開いていたシーン名を保存する
			bool isChangeScene = false ;
			var activeScene = EditorSceneManager.GetActiveScene() ;
			string activeScenepath = activeScene.path ;
			//------------------------------------------------------------------

			int i, l = searchTargetFiles.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				// プログレスバーを表示
				EditorUtility.DisplayProgressBar
				(
					"reference searching ...",
					string.Format( "{0}/{1}", i + 1, l ),
					( float )( i + 1 ) / ( float )l
				) ;

				string assetPath = searchTargetFiles[ i ] ;

				//---------------------------------

				bool exists = false ;

				string[] dependencyPaths = AssetDatabase.GetDependencies( assetPath ) ;
				if( dependencyPaths != null && dependencyPaths.Length >  0 )
				{
					string dependencyPath ;
					int j, m = dependencyPaths.Length ;
					for( j  = 0 ; j <  m ; j ++ )
					{
						dependencyPath = dependencyPaths[ j ].Replace( "\\", "/" ).TrimStart( '/' ).TrimEnd( '/' ) ;
						if( dependencyPath == targetAssetPath )
						{
							exists = true ;
							break ;
						}
					}
				}

				if( exists == false )
				{
					continue ;	// そのファイルからの直接の参照は存在しない
				}

				//---------------------------------

				string extension = Path.GetExtension( assetPath ) ;

				if( extension != ".prefab" && extension != ".unity" )
				{
					// アセットファイル
					SearchAsset( assetPath, targetAssetPath, ref referenceAssets ) ;
				}
				else
				if( extension == ".prefab" )
				{
					// プレハブファイル
					SearchPrefab( assetPath, targetAssetPath, ref referenceAssets ) ;
				}
				else
				if( extension == ".unity" )
				{
					// シーンファイル
					if( SearchScene( assetPath, targetAssetPath, ref referenceAssets ) == true )
					{
						isChangeScene = true ;
					}
				}
			}

			// プログレスバーを消す
			EditorUtility.ClearProgressBar() ;

			//----------------------------------------------------------
			if( isChangeScene == true && string.IsNullOrEmpty( activeScenepath ) == false )
			{
				// 変更されたシーンを元に戻す
				activeScene = EditorSceneManager.GetActiveScene() ;
				if( activeScene.path != activeScenepath )
				{
//					Debug.Log( "Reload Scene : " + activeScenepath ) ;
					EditorSceneManager.OpenScene( activeScenepath ) ;
				}
			}
			//----------------------------------------------------------

			return referenceAssets.Values.ToList() ;
		}

		//-----------------------------------------------------------

		// アセットを検査する
		private void SearchAsset( string assetPath, string targetAssetPath, ref Dictionary<string, ReferenceAssetData> referenceAssets )
		{
			UnityEngine.Object[] components = AssetDatabase.LoadAllAssetsAtPath( assetPath ) ;
			foreach( UnityEngine.Object component in components )
			{
				if( component == null )
				{
					// 参照がロストしている
					continue ;
				}

//				if( asset.name == "Deprecated EditorExtensionImpl" )
//				{
//					// 非推奨(本当は検出に追加した方が良いのだろうが)
//					continue ;
//				}

				// マテリアルなどはコンポーネントではない
				//--------------------------------

				// プロパティでミッシングを起こしているものを追加する
				AddProjectMissingOfProperty( component, assetPath, "Asset", string.Empty, targetAssetPath, ref referenceAssets ) ;
			}
		}

		//-----------------------------------

		// プレハブを検査する
		private void SearchPrefab( string assetPath, string targetAssetPath, ref Dictionary<string, ReferenceAssetData> referenceAssets )
		{
			// メインアセットのみ取得
			GameObject prefab = AssetDatabase.LoadMainAssetAtPath( assetPath ) as GameObject ;
			if( prefab != null )
			{
				SearchHierarchy( prefab.transform, assetPath, "Prefab", targetAssetPath, ref referenceAssets ) ;
			}
			else
			{
				Debug.LogWarning( "[Prefab] Could not load : " + assetPath ) ;
			}
		}

		//-----------------------------------

		// シーンを検査する

		// プレハブを検査する
		private bool SearchScene( string assetPath, string targetAssetPath, ref Dictionary<string, ReferenceAssetData> referenceAssets )
		{
			var scene = EditorSceneManager.OpenScene( assetPath ) ;
			if( scene == null )
			{
				return false ;	// シーンがロードできない
			}

			//----------------------------------------------------------
			// ルートのゲームオブジェクトを取得する

			GameObject[] rootObjects = scene.GetRootGameObjects() ;
			if( rootObjects != null && rootObjects.Length >  0 )
			{
				foreach( var rootObject in rootObjects )
				{
					SearchHierarchy( rootObject.transform, assetPath, "Scene", targetAssetPath, ref referenceAssets ) ;
				}
			}

			return true ;
		}

		//-----------------------------------

		// 再帰的に検査する
		private void SearchHierarchy( Transform t, string assetPath, string assetType, string targetAssetPath, ref Dictionary<string, ReferenceAssetData> referenceAssets )
		{
			// プレハブ内でのパスを取得する
			string hierarchyPath = GetHierarchyPath( t ) ;

			// GameObject 自体が Missing になっているか検査する
		    var status		= PrefabUtility.GetPrefabInstanceStatus( t.gameObject ) ;
			var isMissing	= ( status == PrefabInstanceStatus.MissingAsset ) ;

			if( isMissing == true )
			{
				// GameObject が Missing の場合は子の検査は行わない(意味が無い)
				return ;
			}

			//----------------------------------
			// コンポーネントとプロパティを検査する

			Component[] components = t.GetComponents<Component>() ;
			if( components != null && components.Length >  0 )
			{
				foreach( Component component in components )
				{
					if( component == null )
					{
						// 参照がロストしている
						continue ;
					}

//					if( component.name == "Deprecated EditorExtensionImpl" )
//					{
//						// 無効になったもの(検査の必要無し)
//						continue ;
//					}

					// プロパティでミッシングを起こしているものを追加する
					AddProjectMissingOfProperty( component, assetPath, assetType, hierarchyPath, targetAssetPath, ref referenceAssets ) ;
				}
			}

			//----------------------------------------------------------
			// 子の処理を行う

			if( t.childCount >  0 )
			{
				int i, l = t.childCount ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					SearchHierarchy( t.GetChild( i ), assetPath, assetType, targetAssetPath, ref referenceAssets ) ;
				}
			}
		}

		//-----------------------------------------------------------

		// プロパティでミッシングを起こしているものを追加する
		private void AddProjectMissingOfProperty( UnityEngine.Object component, string assetPath, string assetType, string hierarchyPath, string targetAssetPath, ref Dictionary<string,ReferenceAssetData> referenceAssets )
		{
			// SerializedObjectを通してアセットのプロパティを取得する
			SerializedObject so = new SerializedObject( component ) ;
			if( so != null )
			{
				// VSの軽度ワーニングが煩わしいので using は使わず Dispose() を使用 
				SerializedProperty property = so.GetIterator() ;
				while( property != null )
				{
					// プロパティの種類がオブジェクト（アセット）への参照で、
					// その参照が null なのにもかかわらず、参照先インスタンス識別子が 0 でないものは Missing 状態！
					if
					(
						( property.propertyType						== SerializedPropertyType.ObjectReference	) &&
						( property.objectReferenceValue				!= null										) &&
						( property.objectReferenceInstanceIDValue	!= 0										)
					)
					{
						if( AssetDatabase.Contains( property.objectReferenceValue ) == true )
						{
							// アセットファイルのみ有効

							string referencedAssetPath = AssetDatabase.GetAssetPath( property.objectReferenceValue ) ;
							referencedAssetPath = referencedAssetPath.Replace( "\\", "/" ).TrimStart( '/' ).TrimEnd( '/' ) ;

							if( referencedAssetPath == targetAssetPath && assetPath != targetAssetPath )
							{
								// 参照を発見
								ReferenceAssetData	referenceAsset ;

								if( referenceAssets.ContainsKey( assetPath ) == false )
								{
									// 登録はまだ無い

									referenceAsset = new ReferenceAssetData()
									{
										AssetPath			= assetPath,
										AssetType			= assetType,
									} ;

									referenceAssets.Add( assetPath, referenceAsset ) ;
								}
								else
								{
									// 登録が既にある
									referenceAsset = referenceAssets[ assetPath ] ;
								}

								ReferenceAssetReferenceData reference = new ReferenceAssetReferenceData() ;

								if( assetType == "Prefab" || assetType == "Scene" )
								{
									// プレハブ
									reference.HierarchyPath		= hierarchyPath ;
								}

								reference.ComponentType			= component.GetType().ToString().Replace( "UnityEngine.", "" ) ;

								reference.PropertyType			= property.objectReferenceValue.GetType().ToString().Replace( "UnityEngine.", "" ) ;
								reference.PropertyPath			= property.propertyPath ;
								reference.ReferencedAssetPath	= referencedAssetPath ;

								// 情報追加
								referenceAsset.References.Add( reference ) ;
							}
						}
					}

					// 非表示プロパティも表示する
					if( property.Next( true ) == false )
					{
						break ;
					}
				}

				so.Dispose() ;
			}
		}

		//-------------------------------------------------------------------------------------------

		// ヒエラルキーのパスを取得する
		private string GetHierarchyPath( Transform self )
		{
			string path = self.gameObject.name ;
			Transform parent = self.parent ;
			while( parent != null )
			{
				path = parent.name + "/" + path ;
				parent = parent.parent ;
			}
			return path ;
		}

		// ヒエラルキーの中の指定したパスの GameObject を取得する
		private GameObject GetGameObjectByHierarchyPath( string hierarchyPath, GameObject prefabObject = null )
		{
			List<GameObject> rootObjects = new List<GameObject>() ;

			if( prefabObject == null )
			{
				// 現在のシーンから取得する
				Scene activeScene = EditorSceneManager.GetActiveScene() ;
				GameObject[] gameObjects = activeScene.GetRootGameObjects() ;
				if( gameObjects != null && gameObjects.Length >  0 )
				{
					rootObjects.AddRange( gameObjects ) ;
				}
			}
			else
			{
				// プレハブ
				rootObjects.Add( prefabObject ) ;
			}

			if( rootObjects.Count == 0 )
			{
				return null ;	// 取得できない
			}

			//----------------------------------------------------------

			string path = hierarchyPath.Replace( "\\", "" ).TrimStart( '/' ).TrimEnd( '/' ) ;

			List<Transform> allTransforms = new List<Transform>() ;

			foreach( var rootObject in rootObjects )
			{
				allTransforms.AddRange( rootObject.GetComponentsInChildren<Transform>( true ) ) ;	
			}

			foreach( var t in allTransforms )
			{
				if( GetHierarchyPath( t ) == path )
				{
					// 該当を発見した
					return t.gameObject ;
				}
			}

			return null ;
		}
	}
}
