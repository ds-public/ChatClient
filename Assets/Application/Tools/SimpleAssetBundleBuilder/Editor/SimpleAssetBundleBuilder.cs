using System ;
using System.IO ;
using System.Collections.Generic ;
using System.Linq ;
using System.Reflection ;
using System.Text ;
using System.Xml ;
using UnityEngine ;
using UnityEditor ;
using UnityEditor.SceneManagement ;

/// <summary>
/// シンプルアセットバンドルビルダーパッケージ
/// </summary>
namespace Tools.ForAssetBundle
{
	/// <summary>
	/// アセットバンドルビルダークラス(エディター用) Version 2022/05/15 0
	/// </summary>
	public class SimpleAssetBundleBuilder : EditorWindow
	{
		[ MenuItem( "Tools/Simple AssetBundle Builder(アセットバンドル作成)" ) ]
		public static void OpenWindow()
		{
			EditorWindow.GetWindow<SimpleAssetBundleBuilder>( false, "AB Builder", true ) ;
		}

		//------------------------------------------------------------

		// アセットバンドル化するファイル単位の情報
		[System.Serializable]
		class AssetBundleFile
		{
			[System.Serializable]
			public class AssetFile
			{
				public string	AssetPath ;
				public int		AssetType ;	// 0だと直接含まれるアセットを表す

				public AssetFile( string assetPath, int assetType )
				{
					AssetPath = assetPath ;
					AssetType = assetType ;
				}
			}

			public string			AssetBundlePath ;						// 書き出すアセットバンドルのパス
			public List<AssetFile>	AssetFiles = new List<AssetFile>() ;	// 含めるアセットのパスのリスト

			public int				SourceType ;							// 0=ファイル　1=フォルダ

			public bool				NoConvert ;								// true=アセットバンドル化を行わない単独ファイル
			public string[]			Tags ;									// アセットバンドルごとのタグ

			public void AddAssetFile( string assetPath, int assetType )
			{
				AssetFiles.Add( new AssetFile( assetPath, assetType ) ) ;
			}

			// 依存関係のあるアセットも対象に追加する
			public int CollectDependencies()
			{
				// 注意：必ず基本アセットを全て追加した後にこのメソッドを呼び出す事

				if( AssetFiles.Count == 0 || NoConvert == true )
				{
					return 0 ;
				}

				List<string> assetPaths = new List<string>() ;

				foreach( var assetFile in AssetFiles )
				{
					// 依存関係にあるアセットを検出する
					string[] checkPaths = AssetDatabase.GetDependencies( assetFile.AssetPath ) ;
					if( checkPaths != null && checkPaths.Length >  0 )
					{
						foreach( var checkPath in checkPaths )
						{
							if( AssetFiles.Any( _ => _.AssetPath == checkPath ) == false )
							{
								// 同アセットバンドルに含まれないものに限定する
								if( CheckFolderType( checkPath ) == true && CheckFileType( checkPath ) == true )
								{
									// 有効なパスで且つソースファイル以外に限定する
									assetPaths.Add( checkPath ) ;
								}
							}
						}
					}
				}

				if( assetPaths.Count >  0 )
				{
					foreach( var assetPath in assetPaths )
					{
						AddAssetFile( assetPath, 1 ) ;	// 依存系のアセットのパスを追加する
					}
				}

				return assetPaths.Count ;
			}

			// 無効なフォルダかどうか判別する
			private bool CheckFolderType( string path )
			{
				path = path.Replace( '\\', '/' ).TrimStart( '/' ) ;

				string[] folderNames = path.Split( '/' ) ;
				int i, l = folderNames.Length ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					if( string.IsNullOrEmpty( folderNames[ i ] ) == false )
					{
						if( folderNames[ i ][ 0 ] == '#' || folderNames[ i ][ 0 ] == '!' )
						{
							return false ;
						}
					}
				}

				return true ;
			}
			
			// 拡張子をチェックして有効なファイルかどうか判別する
			private bool CheckFileType( string path )
			{
				path = path.Replace( '\\', '/' ) ;

				int l = path.Length ;

				int i ;
				i = path.LastIndexOf( '/' ) ;
				if( ( i == -1 && ( i + 1 ) <  l ) || ( i >= 0 && ( i + 1 ) <  l ) )
				{
					// ファイル名
					i ++ ;
					if( path[ i ] == '#' || path[ i ] == '!' )
					{
						return false ;	// 無効なファイル
					}
				}

				i = path.LastIndexOf( '.' ) ;
				if( i >= 0 )
				{
					// 拡張子あり
					i ++ ;
					string extension = path.Substring( i, l - i ) ;
					if( string.IsNullOrEmpty( extension ) == false )
					{
						if( extension == "meta" || extension == "cs" || extension == "js" )
						{
							return false ;	// meta と cs と js 以外はＯＫ
						}
					}
				}

				return true ;
			}
		}

		//------------------------------------------------------------

		public class TargetData
		{
			public string	ListFilePath ;
			public string	RootFolderPath ;
		}

		private string						m_LocalAssetsListFilePath		= string.Empty ;	// リストファイルパス
		private string						m_LocalAssetsRootFolderPath		= string.Empty ;	// リソースルートパス
		private string						m_AssetBundleRootFolderPath		= string.Empty ;	// アセットバンドルルートパス

		private BuildTarget					m_BuildTarget					= BuildTarget.Android ;		// ターゲットプラットフォーム
		private bool						m_ChunkBasedCompression			= true ;					// チャンクベースのフォーマットにするか
		private bool						m_ForceRebuildAssetBundle		= false ;					// 強制的に再生成するか
		private bool						m_IgnoreTypeTreeChanges			= false ;					// タイプツリーが変わっても無視(ソースコードを変更しても変化したとはみなされない)
		private bool						m_DisableWriteTypeTree			= false ;					// タイプツリー自体を削除(サイズ削減用途として)

		/// <summary>
		/// ビルド中にエラーが報告されている場合、ビルドを失敗させる
		/// </summary>
		/// <seealso cref="BuildAssetBundleOptions.StrictMode"/>
		private bool m_StrictMode = false ;

		private bool						m_CollectDependencies			= false ;					// 全てのアセットバンドルでそのアセットバンドルで使用するアセットを全て含ませる

		private bool						m_GenerateVersionFile			= true ;
		private bool						m_GenerateCrcFile_Csv			= true ;					// アセットバンドルファイルのＣＲＣファイルを出力する(CSV版)
		private bool						m_GenerateCrcFile_Json			= true ;					// アセットバンドルファイルのＣＲＣファイルを出力する(Json版)

		private bool						m_GenerateLinkFile				= true ;
		private string						m_LinkFilePath					= "Assets/link.xml" ;		// 参照アセンブリの定義ファイルのパス

		//--------------------------------------------------


		private AssetBundleFile[]			m_AssetBundleFiles = null ;

//		private List<string>				m_IgnoreFilePaths = null ;

		//--------------------------------------------------

		private bool m_Clean				= true ;

		private Vector2 m_Scroll			= Vector2.zero ;

		private bool m_Refresh				= true ;

		private bool m_ShowAssetElements	= false ;


		//-----------------------------------------------------------------

		// 描画
		internal void OnGUI()
		{
			GUILayout.Space( 6f ) ;

			string path ;

			// リスト更新フラグ
			m_Refresh = false ;

			//-------------------------------------------------------------

			EditorGUILayout.HelpBox( GetMessage( "SelectLocalAssetsPath" ), MessageType.Info ) ;
			GUILayout.BeginHorizontal() ;
			{
				// リストファイルのパスを選択する
				GUI.backgroundColor = new Color( 0, 1, 1, 1 ) ;
				if( GUILayout.Button( "LocalAssets ListFile Path", GUILayout.Width( 200f ) ) == true )
				{
					m_Refresh = true ;

					m_LocalAssetsListFilePath = string.Empty ;
					m_LocalAssetsRootFolderPath = string.Empty ;
					m_AssetBundleFiles = null ;

					if( Selection.objects.Length == 1 && Selection.activeObject != null )
					{
						// １つだけ選択（複数選択には対応していない：フォルダかファイル）
						path = AssetDatabase.GetAssetPath( Selection.activeObject.GetInstanceID() ) ;
						if( File.Exists( path ) == true )
						{
							// ファイルを指定

							TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>( path ) ;
							if( textAsset != null && string.IsNullOrEmpty( textAsset.text ) == false )
							{
								m_LocalAssetsListFilePath = path ;

								path = path.Replace( "\\", "/" ) ;

								// 最後のフォルダ区切り位置を取得する
								int s = path.LastIndexOf( '.' ) ;
								if( s >= 0 )
								{
									path = path.Substring( 0, s ) ;
								}

								// 最後のフォルダ区切り位置を取得する
								s = path.LastIndexOf( '/' ) ;
								if( s >= 0 )
								{
									path = path.Substring( 0, s ) ;
								}

								// ファイルかどうか判別するには System.IO.File.Exists
								m_LocalAssetsRootFolderPath = path + "/" ;
							}
						}
					}
				}
				GUI.backgroundColor = Color.white ;

				//---------------------------------------------------------

				// ルートフォルダ
				EditorGUILayout.TextField( m_LocalAssetsListFilePath ) ;
			}
			GUILayout.EndHorizontal() ;

			if( string.IsNullOrEmpty( m_LocalAssetsRootFolderPath ) == false )
			{
				GUILayout.BeginHorizontal() ;
				{
					// ルートフォルダも表示する
					GUILayout.Label( "     LocalAssets RootFolder Path     ", GUILayout.Width( 200f ) ) ;
					GUI.color = Color.yellow ;
					GUILayout.Label( m_LocalAssetsRootFolderPath ) ;
					GUI.color = Color.white ;
				}
				GUILayout.EndHorizontal() ;
			}

			if( string.IsNullOrEmpty( m_LocalAssetsListFilePath ) == false && string.IsNullOrEmpty( m_LocalAssetsRootFolderPath ) == false )
			{
				// 更新
				GUI.backgroundColor = new Color( 1, 0, 1, 1 ) ;
				if( GUILayout.Button( "Refresh" ) == true )
				{
					m_Refresh = true ;	// 対象更新
				}
				GUI.backgroundColor = Color.white ;
			}

			//-------------------------------------------------------------

			GUILayout.Space( 12f ) ;

			//-------------------------------------------------------------

			EditorGUILayout.HelpBox( GetMessage( "SelectAssetBundlePath" ), MessageType.Info ) ;
			GUILayout.BeginHorizontal() ;
			{
				// 保存パスを選択する
				GUI.backgroundColor = new Color( 1, 0.5f, 0, 1 ) ;
				if( GUILayout.Button( "AssetBundle RootFolder Path", GUILayout.Width( 220f ) ) == true )
				{
					if( Selection.objects != null && Selection.objects.Length == 0 && Selection.activeObject == null )
					{
						// ルート
						m_AssetBundleRootFolderPath = "Assets/" ;
					}
					else
					if( Selection.objects != null && Selection.objects.Length == 1 && Selection.activeObject != null )
					{
						path = AssetDatabase.GetAssetPath( Selection.activeObject.GetInstanceID() ) ;
						if( Directory.Exists( path ) == true )
						{
							// フォルダを指定しています

							// ファイルかどうか判別するには System.IO.File.Exists

							// 有効なフォルダ
							path = path.Replace( "\\", "/" ) ;
						}
						else
						{
							// ファイルを指定しています
							path = path.Replace( "\\", "/" ) ;

							// 拡張子を見てアセットバンドルであればファイル名まで置き変える
							// ただしこれを読み出して含まれるファイルの解析などは行わない
							// なぜなら違うプラットフォームの場合は読み出せずにエラーになってしまうから

							// 最後のフォルダ区切り位置を取得する
							int s = path.LastIndexOf( '.' ) ;
							if( s >= 0 )
							{
								path = path.Substring( 0, s ) ;
							}

							// 最後のフォルダ区切り位置を取得する
							s = path.LastIndexOf( '/' ) ;
							if( s >= 0 )
							{
								path = path.Substring( 0, s ) ;
							}
						}

						m_AssetBundleRootFolderPath = path + "/" ;
					}

					// プラットフォーム自動設定
					path = m_AssetBundleRootFolderPath ;
					if( string.IsNullOrEmpty( path ) == false )
					{
						string[] folderNameElements = path.Split( '/' ) ;
						string smallFolderName ;
						if( folderNameElements != null && folderNameElements.Length >= 2 )
						{
							for( int i  = 0 ; i <  ( folderNameElements.Length - 1 ) ; i ++ )
							{
								if( string.IsNullOrEmpty( folderNameElements[ i ] ) == false )
								{
									smallFolderName = folderNameElements[ i ].ToLower() ;

									if( smallFolderName == "windows" )
									{
										m_BuildTarget = BuildTarget.StandaloneWindows ;
										break ;
									}
									else
									if( smallFolderName == "osx" || smallFolderName == "machintosh" )
									{
										m_BuildTarget = BuildTarget.StandaloneOSX ;
										break ;
									}
									else
									if( smallFolderName == "android" )
									{
										m_BuildTarget = BuildTarget.Android ;
										break ;
									}
									else
									if( smallFolderName == "ios" || smallFolderName == "iphone" )
									{
										m_BuildTarget = BuildTarget.iOS ;
										break ;
									}
								}
							}
						}
					}
				}
				GUI.backgroundColor = Color.white ;

				// 保存パス
				if( string.IsNullOrEmpty( m_AssetBundleRootFolderPath ) == true )
				{
					GUI.color = Color.yellow ;
					GUILayout.Label( "Select AssetBundle Root Folder Path." ) ;
					GUI.color = Color.white ;
				}
				else
				{
					m_AssetBundleRootFolderPath = EditorGUILayout.TextField( m_AssetBundleRootFolderPath ) ;
				}
			}
			GUILayout.EndHorizontal() ;

			//-----------------------------------------------------

			// ターゲットプラットフォームと圧縮指定

			GUILayout.BeginHorizontal() ;	// 横並び開始
			{
				GUILayout.Label( "Build Target", GUILayout.Width( 80 ) ) ;	// null でないなら 74

				BuildTarget buildTarget = ( BuildTarget )EditorGUILayout.EnumPopup( m_BuildTarget ) ;
				if( buildTarget != m_BuildTarget )
				{
					m_BuildTarget  = buildTarget ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUILayout.BeginHorizontal() ;	// 横並び開始
			{
				bool chunkBasedCompression = EditorGUILayout.Toggle( m_ChunkBasedCompression, GUILayout.Width( 10f ) ) ;
				if( chunkBasedCompression != m_ChunkBasedCompression )
				{
					m_ChunkBasedCompression = chunkBasedCompression ;
				}
				GUILayout.Label( "Chunk Based Compression", GUILayout.Width( 160f ) ) ;

				GUILayout.Label( " " ) ;

				bool forceRebuildAssetBundle = EditorGUILayout.Toggle( m_ForceRebuildAssetBundle, GUILayout.Width( 10f ) ) ;
				if( forceRebuildAssetBundle != m_ForceRebuildAssetBundle )
				{
					m_ForceRebuildAssetBundle = forceRebuildAssetBundle ;
				}
				GUILayout.Label( "Force Rebuild AssetBundle", GUILayout.Width( 160f ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUILayout.BeginHorizontal() ;	// 横並び開始
			{
				bool ignoreTypeTreeChanges = EditorGUILayout.Toggle( m_IgnoreTypeTreeChanges, GUILayout.Width( 10f ) ) ;
				if( ignoreTypeTreeChanges != m_IgnoreTypeTreeChanges )
				{
					m_IgnoreTypeTreeChanges = ignoreTypeTreeChanges ;
					if( m_IgnoreTypeTreeChanges == true )
					{
						m_DisableWriteTypeTree = false ;
					}
				}
				GUILayout.Label( "Ignore Type Tree Changes",  GUILayout.Width( 160f ) ) ;

				GUILayout.Label( " " ) ;

				bool disableWriteTypeTree = EditorGUILayout.Toggle( m_DisableWriteTypeTree, GUILayout.Width( 10f ) ) ;
				if( disableWriteTypeTree != m_DisableWriteTypeTree )
				{
					m_DisableWriteTypeTree = disableWriteTypeTree ;
					if( m_DisableWriteTypeTree == true )
					{
						m_IgnoreTypeTreeChanges = false ;
					}
				}
				GUILayout.Label( "Disable Write TypeTree", GUILayout.Width( 160f ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUILayout.Space(  6f ) ;

			GUILayout.BeginHorizontal() ;	// 横並び開始
			{
				bool generateVersionFile = EditorGUILayout.Toggle( m_GenerateVersionFile, GUILayout.Width( 10f ) ) ;
				if( generateVersionFile != m_GenerateVersionFile )
				{
					m_GenerateVersionFile = generateVersionFile ;
				}
				GUILayout.Label( "Generate Version File", GUILayout.Width( 160f ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUILayout.BeginHorizontal() ;	// 横並び開始
			{
				bool generateCrcFile_Csv = EditorGUILayout.Toggle( m_GenerateCrcFile_Csv, GUILayout.Width( 10f ) ) ;
				if( generateCrcFile_Csv != m_GenerateCrcFile_Csv )
				{
					m_GenerateCrcFile_Csv = generateCrcFile_Csv ;
				}
				GUILayout.Label( "Generate CRC File(CSV)", GUILayout.Width( 160f ) ) ;

				GUILayout.Label( " " ) ;

				bool generateCrcFile_Json = EditorGUILayout.Toggle( m_GenerateCrcFile_Json, GUILayout.Width( 10f ) ) ;
				if( generateCrcFile_Json != m_GenerateCrcFile_Json )
				{
					m_GenerateCrcFile_Json = generateCrcFile_Json ;
				}
				GUILayout.Label( "Generate CRC File(JSON)", GUILayout.Width( 160f ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了


			GUILayout.BeginHorizontal() ;	// 横並び開始
			{
				bool collectDependencies = EditorGUILayout.Toggle( m_CollectDependencies, GUILayout.Width( 10f ) ) ;
				if( collectDependencies != m_CollectDependencies )
				{
					m_CollectDependencies = collectDependencies ;
					m_Refresh = true ;	// リスト更新
				}
				GUI.color = Color.yellow ;
				GUILayout.Label( "Collect Dependencies ( Legacy Type )", GUILayout.Width( 240f ) ) ;
				GUI.color = Color.white ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUILayout.BeginHorizontal() ;	// 横並び開始
			{
				bool generateLinkFile = EditorGUILayout.Toggle( m_GenerateLinkFile, GUILayout.Width( 10f ) ) ;
				if( generateLinkFile != m_GenerateLinkFile )
				{
					m_GenerateLinkFile = generateLinkFile ;
				}
				GUILayout.Label( "Generate Link File", GUILayout.Width( 160f ) ) ;

			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			//-----------------------------------------------------

			if( m_GenerateLinkFile == true )
			{
				GUILayout.Space( 12f ) ;

				//-----------------------------------------------------
				// link.xml ファイルの出力先

				EditorGUILayout.HelpBox( GetMessage( "SelectLinkXmlFilePath" ), MessageType.Info ) ;
				GUILayout.BeginHorizontal() ;	// 横並び開始
				{
					// 保存パスを選択する
					GUI.backgroundColor = new Color( 1, 1, 0, 1 ) ;
					if( GUILayout.Button( "LinkXmlFile Path", GUILayout.Width( 200f ) ) == true )
					{
						if( Selection.objects.Length == 1 && Selection.activeObject != null )
						{
							// １つだけ選択（複数選択には対応していない：フォルダかファイル）
							path = AssetDatabase.GetAssetPath( Selection.activeObject.GetInstanceID() ) ;
							if( File.Exists( path ) == true )
							{
								// ファイルを指定

								TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>( path ) ;
								if( textAsset != null && string.IsNullOrEmpty( textAsset.text ) == false )
								{
									m_LocalAssetsListFilePath = path ;

									path = path.Replace( "\\", "/" ) ;

									// 最後のフォルダ区切り位置を取得する
									int s = path.LastIndexOf( '.' ) ;
									if( s >= 0 )
									{
										path = path.Substring( 0, s ) ;
									}

									// 最後のフォルダ区切り位置を取得する
									s = path.LastIndexOf( '/' ) ;
									if( s >= 0 )
									{
										path = path.Substring( 0, s ) ;
									}

									// ファイルかどうか判別するには System.IO.File.Exists
									m_LinkFilePath = path + "/link.xml" ;
								}
							}
							else
							if( Directory.Exists( path ) == true )
							{
								m_LinkFilePath = path + "/link.xml" ;
							}
						}
					}

					GUI.backgroundColor = Color.white ;
					EditorGUILayout.TextField( m_LinkFilePath ) ;
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}

			//-----------------------------------------------------

			//-------------------------------------------------------------

			if( string.IsNullOrEmpty( m_LocalAssetsRootFolderPath ) == false && m_LocalAssetsRootFolderPath == m_AssetBundleRootFolderPath )
			{
				GUILayout.Space( 24f ) ;

				// 同じパスを指定するのはダメ
				EditorGUILayout.HelpBox( GetMessage( "SamePath" ), MessageType.Warning ) ;

				return ;
			}

			//-------------------------------------------------------------

			// ここからが表示と出力

			if( string.IsNullOrEmpty( m_LocalAssetsRootFolderPath ) == true )
			{
				return ;
			}

			//----------------------------------

			//　アップデートフラグを更新する
			if( m_Refresh == true )
			{
				m_Refresh = false ;

				// アセットバンドル情報を読み出す
				( m_AssetBundleFiles, _ ) = GetAssetBundleFiles( m_LocalAssetsListFilePath, m_LocalAssetsRootFolderPath ) ;
			}

			// アセットバンドル化対象リストを表示する
			if( m_AssetBundleFiles == null || m_AssetBundleFiles.Length == 0 )
			{
				return ;
			}

			// トータルのアセットバンドル数とリソース数を計算する
			int acount = m_AssetBundleFiles.Length ;
			int rcount = 0 ;
			foreach( var assetBundleFile in m_AssetBundleFiles )
			{
				rcount += assetBundleFile.AssetFiles.Count ;
			}

			//-------------------------------------------------

			if( string.IsNullOrEmpty( m_LinkFilePath ) == false && m_AssetBundleFiles != null && m_AssetBundleFiles.Length >  0 )
			{
				// link.xml ファイル作成ボタン

				// 生成
				GUI.backgroundColor = new Color( 1, 1, 0.5f, 1 ) ;
				if( GUILayout.Button( "Create LinkXmlFile Only" ) == true )
				{
					TargetData[] targets = new TargetData[]
					{
						new TargetData()
						{
							ListFilePath	= m_LocalAssetsListFilePath,
							RootFolderPath	= m_LocalAssetsRootFolderPath
						}
					} ;

					bool result = CreateLinkXmlFile( targets, m_LinkFilePath ) ;
					if( result == true )
					{
						// 結果のアラートダイアログを表示する
						EditorUtility.DisplayDialog( "Make LinkXmlFile", GetMessage( "Succeed" ), GetMessage( "OK" ) ) ;
					}
				}
				GUI.backgroundColor = Color.white ;
			}

			GUILayout.Space( 24f ) ;

			//---------------------------------------------------------

			// 生成ボタン（Create , Create And Replace , Replace
			bool execute = false ;

			if( string.IsNullOrEmpty( m_AssetBundleRootFolderPath ) == false && Directory.Exists( m_AssetBundleRootFolderPath ) == true )
			{
				// 更新または新規作成対象が存在する
				GUILayout.BeginHorizontal() ;
				{
					// 生成
					GUI.backgroundColor = new Color( 0, 1, 0, 1 ) ;
					if( GUILayout.Button( "Create Or Update" ) == true )
					{
						execute = true ;
					}
					GUI.backgroundColor = Color.white ;

					// 同時にリソースリスト外のファイル・フォルダを削除する
					GUILayout.Label( "", GUILayout.Width( 10 ) ) ;
					m_Clean = EditorGUILayout.Toggle( m_Clean, GUILayout.Width( 10f ) ) ;
					GUILayout.Label( "Clean", GUILayout.Width( 40 ) ) ;
					GUILayout.Label( "", GUILayout.Width( 10 ) ) ;
				}
				GUILayout.EndHorizontal() ;
			}

			//---------------------------------------------------------

			// リストを表示する

			GUILayout.BeginHorizontal() ;	// 横並び開始
			{
				m_ShowAssetElements = EditorGUILayout.Toggle( m_ShowAssetElements, GUILayout.Width( 10f ) ) ;
				GUILayout.Label( "Show AssetElements" ) ;	// null でないなら
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUILayout.Space(  6f ) ;

			GUILayout.Label( "AssetBundle : " + acount + "  from AssetElement : " + rcount ) ;

			string aformat = "{0,0:d" + acount.ToString ().Length +"}" ;
			string rformat = "{0,0:d" + rcount.ToString ().Length +"}" ;
			// 0 無しは "{0," + tNumber.Length + "}"

			//-------------------------------------------------

			Color c0 = new Color( 0.0f, 1.0f, 1.0f, 1.0f ) ;
			Color c1 = new Color( 1.0f, 1.0f, 1.0f, 1.0f ) ;

			// スクロールビューで表示する
			m_Scroll = GUILayout.BeginScrollView( m_Scroll ) ;
			{
				// 表示が必要な箇所だけ表示する
				int aline = 0 ;
				int rline = 0 ;
				foreach( var assetBundleFile in m_AssetBundleFiles )
				{
					// アセット情報
					GUILayout.BeginHorizontal() ;
					{
						// 横一列
						GUI.color = c0 ;

						GUILayout.Label( string.Format( aformat, aline ) + " : ", GUILayout.Width( 40f ) ) ;
						string ac ;

						if( m_CollectDependencies == false )
						{
							ac = " [ " + assetBundleFile.AssetFiles.Count +" ]" ;
						}
						else
						{
							int[] st = { 0, 0 } ;

							if( assetBundleFile.AssetFiles.Count >  0 )
							{
								foreach( var assetFile in assetBundleFile.AssetFiles )
								{
									st[ assetFile.AssetType ] ++ ;
								}
							}

							ac = " [ " + st[ 0 ] + " + " + st[ 1 ] +" ]" ;
						}

						string assetBundlePath = assetBundleFile.AssetBundlePath ;
						if( assetBundleFile.SourceType == 1 )
						{
							assetBundlePath += "/" ;
						}

						GUILayout.Label( assetBundlePath + ac ) ;

						GUI.color = Color.white ;
					}
					GUILayout.EndHorizontal() ;

					if( m_ShowAssetElements == true )
					{
						if( assetBundleFile.AssetFiles.Count >  0 )
						{
							GUI.color = c1 ;
							foreach( var assetFile in assetBundleFile.AssetFiles )
							{
								GUILayout.BeginHorizontal() ;
								{
									// 横一列
									if( m_CollectDependencies == true )
									{
										if( assetFile.AssetType == 0 )
										{
											GUI.color = Color.white ;
										}
										else
										{
											GUI.color = Color.yellow ;
										}
									}

									GUILayout.Label( "", GUILayout.Width( 10f ) ) ;
									GUILayout.Label( string.Format( rformat, rline ) + " : ", GUILayout.Width( 40f ) ) ;
									GUILayout.Label( assetFile.AssetPath ) ;
									rline ++ ;
								}
								GUILayout.EndHorizontal() ;
							}
							GUI.color = Color.white ;
						}
					}
					aline ++ ;
				}
			}
			GUILayout.EndScrollView() ;

			//-------------------------------------------------

			if( execute == true )
			{
				// アセットバンドル群を生成する
				if( m_AssetBundleRootFolderPath == "Assets/" )
				{
					execute = EditorUtility.DisplayDialog( "Build Asset Bundle", GetMessage( "RootPath" ).Replace( "%1", m_AssetBundleRootFolderPath ), GetMessage( "Yes" ), GetMessage( "No" ) ) ;
				}

				if( execute == true )
				{
					bool result = CreateAssetBundleAll() ;	// 表示と状態が変わっている可能性があるのでリストは作り直す

					// 表示を更新
					m_Refresh = true ;
					Repaint() ;

					if( result == true )
					{
						// 結果のアラートダイアログを表示する
						EditorUtility.DisplayDialog( "Build Asset Bundle", GetMessage( "Succeed" ), GetMessage( "OK" ) ) ;
					}
				}
			}

			// EditorUserBuildSettings.activeBuildTarget
		}

		// 選択中のファイルが変更された際に呼び出される
		internal void OnSelectionChange()
		{
			Repaint() ;
		}

		//-----------------------------------------------------------------------------------------------------

		// アセットバンドルの生成リストを取得する
		private ( AssetBundleFile[], string[] ) GetAssetBundleFiles( string listFilePath, string rootFolderPath )
		{
			//-------------------------------------------------------------

			if( File.Exists( listFilePath ) == false )
			{
				Debug.Log( "[Log]Error : File not found !! : " + listFilePath ) ;
				return ( null, null ) ;
			}

			// リストファイルを読み出す
			TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>( listFilePath ) ;
			string text = textAsset.text ;

			if( string.IsNullOrEmpty( text ) == true )
			{
				Debug.Log( "[Log]Error : Bad list file !! : " + listFilePath ) ;
				return ( null, null ) ;
			}

			string[] elements = text.Split( '\n', ( char )0x0D, ( char )0x0A ) ;
			if( elements == null || elements.Length == 0 )
			{
				return ( null, null ) ;
			}

			//-------------------------------------------------------------

			List<AssetBundleFile> assetBundleFiles = new List<AssetBundleFile>() ;
			List<string> ignoreFilePaths = new List<string>() ;

			string		path ;
			string[]	tags ;

			string[] wildPaths ;

			foreach( var element in elements )
			{
				path = element ;

				path = path.TrimEnd( '\n', ( char )0x0D, ( char )0x0A ) ;
				path = path.Trim( ' ' ) ;	// 前後のスペースを削除する

				tags = null ;

				if( path.IndexOf( ',' ) >= 0 )
				{
					// タグの指定あり
					string[] words = path.Split( ',' ) ;
					path = words[ 0 ] ;
					if( words.Length >= 2 && string.IsNullOrEmpty( words[ 1 ] ) == false )
					{
						// タグあり
						if( words[ 1 ].IndexOf( ' ' ) <  0 )
						{
							tags = new string[]{ words[ 1 ] } ;	// タグは単一
						}
						else
						{
							tags = words[ 1 ].Split( ' ' ) ;	// タグは複数
						}
					}
				}

				if( string.IsNullOrEmpty( path ) == false )
				{
					path = GetLowerPath( path, out bool wildCard, out bool folderOnly, out bool noConvert, out bool isIgnore ) ;
					if( path != null )
					{
						// 有効なパス指定
						wildPaths = GetUpperPath( path, rootFolderPath ) ;
						if( wildPaths != null && wildPaths.Length >  0 )
						{
							foreach( var wildPath in wildPaths )
							{
								// 生成するアセットバンドル情報を追加する
								if( isIgnore == false )
								{
									// 有効対象
									AddAssetBundleFile( wildPath, rootFolderPath + wildPath, wildCard, folderOnly, noConvert, tags, ref assetBundleFiles ) ;
								}
								else
								{
									// 無効対象
									AddIgnoreFilePath( wildPath, rootFolderPath + wildPath, wildCard, folderOnly, ref ignoreFilePaths ) ;
								}
							}
						}
					}
				}
			}

			//-------------------------------------------------------------

			AssetBundleFile[]	result_AssetBundleFiles = null ;
			string[]			result_IgnoreFilePaths	= null ;

			if( assetBundleFiles.Count >  0 )
			{
				result_AssetBundleFiles	= assetBundleFiles.ToArray() ;
			}

			if( ignoreFilePaths.Count >  0 )
			{
				result_IgnoreFilePaths	= ignoreFilePaths.ToArray() ;
			}

			//-----------------------------------------------------

			return ( result_AssetBundleFiles, result_IgnoreFilePaths ) ;
		}

		// 生成するアセットバンドル情報を追加する
		private void AddAssetBundleFile( string path, string localAssetsPath, bool wildCard, bool folderOnly, bool noConvert, string[] tags, ref List<AssetBundleFile> assetBundleFiles )
		{
			if( CheckFolderType( localAssetsPath ) == false )
			{
				return ;	// 無効
			}

			int i, l, p ;

			string parentPath, assetName, folderName ;
			string[] targetPaths ;

			if( wildCard == false )
			{
				// １つのファイルまたは１つのフォルダ
				// 注意：パス内にワイルドカードは含まれない

				// １つ親のフォルダを取得する
				p = localAssetsPath.LastIndexOf( '/' ) ;
				if( p <  0 )
				{
					// ありえない
					return ;
				}

				parentPath = localAssetsPath.Substring( 0, p ) ;
				if( parentPath.Length <  0 )
				{
					// ありえない
					return ;
				}

				if( CheckFileType( parentPath ) == false )
				{
					// 無効
					return ;
				}

				// 親フォルダ内の全てのフォルダまたはファイルのパスを取得する
				if( folderOnly == false && Directory.Exists( parentPath ) == true )
				{
					// ファイル(1のケース)
					targetPaths = Directory.GetFiles( parentPath ) ;
					if( targetPaths != null && targetPaths.Length >  0 )
					{
						l = targetPaths.Length ;
						for( i  = 0 ; i <  l ; i ++ )
						{
							// 拡張子は関係無く最初にヒットしたものを対象とする(基本的に同名のフォルダとファイルを同一フォルダ内に置いてはならない)

							targetPaths[ i ] = targetPaths[ i ].Replace( "\\", "/" ) ;
							if( targetPaths[ i ].Contains( path ) == true )
							{
								// 対象はファイル
								if( CheckFileType( targetPaths[ i ] ) == true )
								{
									// 決定(単独ファイル)

									AssetBundleFile assetBundleFile = new AssetBundleFile()
									{
										NoConvert		= noConvert,	// アセットバンドル化を行うかどうか
										Tags			= tags			// タグ
									} ;

									if( noConvert == false )
									{
										assetBundleFile.AssetBundlePath = path ;	// 出力パス(相対)
									}
									else
									{
										assetName = targetPaths[ i ] ;
										p = assetName.LastIndexOf( '/' ) ;
										if( p >= 0 )
										{
											p ++ ;
											assetName = assetName.Substring( p, assetName.Length - p ) ;
										}

										folderName = path ;
										p = folderName.LastIndexOf( '/' ) ;
										if( p >= 0 )
										{
											folderName = path.Substring( 0, p ) ;
										}
										assetBundleFile.AssetBundlePath = folderName + "/" + assetName ;	// 出力パス(相対)
									}

									// コードで対象指定：単独ファイルのケース
									assetBundleFile.AddAssetFile( targetPaths[ i ], 0 ) ;

									assetBundleFile.SourceType = 0 ;	// 単独ファイル

									if( m_CollectDependencies == true )
									{
										// 依存対象のアセットも内包対象に追加する
										assetBundleFile.CollectDependencies() ;
									}

									// リストに加える
									assetBundleFiles.Add( assetBundleFile ) ;

									// 終了
									return ;
								}
							}
						}
					}
				}

				if( noConvert == false && Directory.Exists( localAssetsPath ) == true )
				{
					// フォルダ(1・3のケース)
					AssetBundleFile assetBundleFile = new AssetBundleFile()
					{
						AssetBundlePath	= path,		// 出力パス
						NoConvert		= false,	// アセットバンドル化を行うかどうか
						Tags			= tags		// タグ
					} ;

					// 再帰的に素材ファイルを加える
					AddAssetBundleFile( assetBundleFile, localAssetsPath ) ;

					if( assetBundleFile.AssetFiles.Count >  0 )
					{
						assetBundleFile.SourceType = 1 ;	// 複数ファイル

						if( m_CollectDependencies == true )
						{
							// 依存対象のアセットも内包対象に追加する
							assetBundleFile.CollectDependencies() ;
						}

						// リストに加える
						assetBundleFiles.Add( assetBundleFile ) ;
					}
				}
			}
			else
			{
				// 複数のファイルまたは複数のフォルダ
				// 注意：パス内にワイルドカードは含まれない

				if( Directory.Exists( localAssetsPath ) == false )
				{
					return ;
				}

				if( folderOnly == false )
				{
					// ファイル(2のケース)
					targetPaths = Directory.GetFiles( localAssetsPath ) ;
					if( targetPaths != null && targetPaths.Length >  0 )
					{
						l = targetPaths.Length ;
						for( i  = 0 ; i <  l ; i ++ )
						{
							targetPaths[ i ] = targetPaths[ i ].Replace( "\\", "/" ) ;

							// 対象はファイル
							if( CheckFileType( targetPaths[ i ] ) == true )
							{
								// 決定(単独ファイル)

								AssetBundleFile assetBundleFile = new AssetBundleFile()
								{
									NoConvert	= noConvert,	// アセットバンドル化を行うかどうか
									Tags		= tags			// タグ
								} ;

								assetName = targetPaths[ i ] ;
								p = assetName.LastIndexOf( '/' ) ;
								if( p >= 0 )
								{
									p ++ ;
									assetName = assetName.Substring( p, assetName.Length - p ) ;
								}

								if( noConvert == false )
								{
									// 拡張子除去
									p = assetName.IndexOf( '.' ) ;
									if( p >= 0 )
									{
										assetName = assetName.Substring( 0, p ) ;
									}
								}

								assetBundleFile.AssetBundlePath	= path + "/" + assetName ;	// 出力パス(相対)

								// コードで対象指定：単独ファイルのケース
								assetBundleFile.AddAssetFile( targetPaths[ i ], 0 ) ;

								assetBundleFile.SourceType = 0 ;	// 単独ファイル

								if( m_CollectDependencies == true )
								{
									// 依存対象のアセットも内包対象に追加する
									assetBundleFile.CollectDependencies() ;
								}

								// リストに加える
								assetBundleFiles.Add( assetBundleFile ) ;
							}
						}
					}
				}

				if( noConvert == false )
				{
					// フォルダ(2・4のケース)
					targetPaths = Directory.GetDirectories( localAssetsPath ) ;
					if( targetPaths != null && targetPaths.Length >  0 )
					{
						l = targetPaths.Length ;
						for( i  = 0 ; i <  l ; i ++ )
						{
							targetPaths[ i ] = targetPaths[ i ].Replace( "\\", "/" ) ;

							// 対象はフォルダ
							if( CheckFolderType( targetPaths[ i ] ) == true )
							{
								AssetBundleFile assetBundleFile = new AssetBundleFile()
								{
									Tags = tags		// タグ
								} ;

								assetName = targetPaths[ i ] ;
								p = assetName.LastIndexOf( '/' ) ;
								if( p >= 0 )
								{
									p ++ ;
									assetName = assetName.Substring( p, assetName.Length - p ) ;
								}

								assetBundleFile.AssetBundlePath = path + "/" + assetName ;	// 出力パス

								// 再帰的に素材ファイルを加える
								AddAssetBundleFile( assetBundleFile, targetPaths[ i ] ) ;

								if( assetBundleFile.AssetFiles.Count >  0 )
								{
									assetBundleFile.SourceType = 1 ;	// 複数ファイル

									if( m_CollectDependencies == true )
									{
										// 依存対象のアセットも内包対象に追加する
										assetBundleFile.CollectDependencies() ;
									}

									// リストに加える
									assetBundleFiles.Add( assetBundleFile ) ;
								}
							}
						}
					}
				}
			}
		}

		// アセットバンドルの要素をリストに追加していく（再帰版）
		private void AddAssetBundleFile( AssetBundleFile assetBundleFile, string currentPath )
		{
			if( Directory.Exists( currentPath ) == false )
			{
				return ;
			}

			//-----------------------------------------------------

			// フォルダ
			string[] da = Directory.GetDirectories( currentPath ) ;
			if( da != null && da.Length >  0 )
			{
				// サブフォルダがあるのでさらに検査していく
				for( int i  = 0 ; i <  da.Length ; i ++ )
				{
					// サブフォルダを検査
					da[ i ] = da[ i ].Replace( "\\", "/" ) ;
					if( CheckFolderType( da[ i ] ) == true )
					{
						AddAssetBundleFile( assetBundleFile, da[ i ] + "/" ) ;	// 再帰版
					}
				}
			}

			// ファイル
			string[] fa = Directory.GetFiles( currentPath ) ;
			if( fa != null && fa.Length >  0 )
			{
				for( int i  = 0 ; i <  fa.Length ; i ++ )
				{
					// 対象化コードで反転無効化（は止める）
					fa[ i ] = fa[ i ].Replace( "\\", "/" ) ;
					if( CheckFileType( fa[ i ] ) == true )
					{
						// コードで対象指定：複数ファイルのケース
						assetBundleFile.AddAssetFile( fa[ i ], 0 ) ;
					}
				}
			}
		}

		// 無視するファイルパスを追加する
		private void AddIgnoreFilePath( string path, string localAssetsPath, bool wildCard, bool folderOnly, ref List<string> ignoreFilePaths )
		{
			if( CheckFolderType( localAssetsPath ) == false )
			{
				return ;	// 無効
			}

			int i, l, p ;

			string parentPath ;
			string[] targetPaths ;

			if( wildCard == false )
			{
				// １つのファイルまたは１つのフォルダ
				// 注意：パス内にワイルドカードは含まれない

				// １つ親のフォルダを取得する
				p = localAssetsPath.LastIndexOf( '/' ) ;
				if( p <  0 )
				{
					// ありえない
					return ;
				}

				parentPath = localAssetsPath.Substring( 0, p ) ;
				if( parentPath.Length <  0 )
				{
					// ありえない
					return ;
				}

				if( CheckFileType( parentPath ) == false )
				{
					// 無効
					return ;
				}

				// 親フォルダ内の全てのフォルダまたはファイルのパスを取得する
				if( folderOnly == false && Directory.Exists( parentPath ) == true )
				{
					// ファイル(1のケース)
					targetPaths = Directory.GetFiles( parentPath ) ;
					if( targetPaths != null && targetPaths.Length >  0 )
					{
						l = targetPaths.Length ;
						for( i  = 0 ; i <  l ; i ++ )
						{
							// 拡張子は関係無く最初にヒットしたものを対象とする(基本的に同名のフォルダとファイルを同一フォルダ内に置いてはならない)

							targetPaths[ i ] = targetPaths[ i ].Replace( "\\", "/" ) ;
							if( targetPaths[ i ].Contains( path ) == true )
							{
								// 対象はファイル
								if( CheckFileType( targetPaths[ i ] ) == true )	// meta ファイルかどうかチェックする
								{
									// 決定(単独ファイル)
									if( ignoreFilePaths.Contains( targetPaths[ i ] ) == false )
									{
										ignoreFilePaths.Add( targetPaths[ i ] ) ;
									}

									// 終了
									return ;
								}
							}
						}
					}
				}

				if( Directory.Exists( localAssetsPath ) == true )
				{
					// フォルダ(1・3のケース)

					// 再帰的に素材ファイルを加える
					AddIgnoreFilePath( ref ignoreFilePaths, localAssetsPath ) ;
				}
			}
			else
			{
				// 複数のファイルまたは複数のフォルダ
				// 注意：パス内にワイルドカードは含まれない

				if( Directory.Exists( localAssetsPath ) == false )
				{
					return ;
				}

				if( folderOnly == false )
				{
					// ファイル(2のケース)
					targetPaths = Directory.GetFiles( localAssetsPath ) ;
					if( targetPaths != null && targetPaths.Length >  0 )
					{
						l = targetPaths.Length ;
						for( i  = 0 ; i <  l ; i ++ )
						{
							targetPaths[ i ] = targetPaths[ i ].Replace( "\\", "/" ) ;

							// 対象はファイル
							if( CheckFileType( targetPaths[ i ] ) == true )	// meta ファイルかどうかのチェック
							{
								// 決定(単独ファイル)
								ignoreFilePaths.Add( targetPaths[ i ] ) ;
							}
						}
					}
				}

				// フォルダ(2・4のケース)
				targetPaths = Directory.GetDirectories( localAssetsPath ) ;
				if( targetPaths != null && targetPaths.Length >  0 )
				{
					l = targetPaths.Length ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						// 再帰的に無視ファイルを加える
						targetPaths[ i ] = targetPaths[ i ].Replace( "\\", "/" ) ;
						if( CheckFolderType( targetPaths[ i ] ) == true )
						{
							AddIgnoreFilePath( ref ignoreFilePaths, targetPaths[ i ] ) ;
						}
					}
				}
			}
		}

		// 無視するファイルパスをリストに追加していく（再帰版）
		private void AddIgnoreFilePath( ref List<string> ignoreFilePaths, string currentPath )
		{
			if( Directory.Exists( currentPath ) == false )
			{
				return ;
			}

			//-----------------------------------------------------

			// フォルダ
			string[] da = Directory.GetDirectories( currentPath ) ;
			if( da != null && da.Length >  0 )
			{
				// サブフォルダがあるのでさらに検査していく
				for( int i  = 0 ; i <  da.Length ; i ++ )
				{
					// サブフォルダを検査
					da[ i ] = da[ i ].Replace( "\\", "/" ) ;
					if( CheckFolderType( da[ i ] ) == true )
					{
						AddIgnoreFilePath( ref ignoreFilePaths, da[ i ] + "/" ) ;	// 再帰版
					}
				}
			}

			// ファイル
			string[] fa = Directory.GetFiles( currentPath ) ;
			if( fa != null && fa.Length >  0 )
			{
				for( int i  = 0 ; i <  fa.Length ; i ++ )
				{
					// 対象化コードで反転無効化（は止める）
					fa[ i ] = fa[ i ].Replace( "\\", "/" ) ;
					if( CheckFileType( fa[ i ] ) == true )
					{
						// コードで対象指定：複数ファイルのケース
						ignoreFilePaths.Add( fa[ i ] ) ;
					}
				}
			}
		}

		//---------------------------------------------------------------------


		// パスを解析して最終的なターゲットパスを取得する
		private string GetLowerPath( string path, out bool wildCard, out bool folderOnly, out bool noConvert, out bool isIgnore )
		{
			wildCard	= false ;
			folderOnly	= false ;
			noConvert	= false ;
			isIgnore	= false ;

			path = path.Replace( "**", "*" ) ;
			path = path.Replace( "**", "*" ) ;

			if( path.Length == 0 )
			{
				// 不可
				return null ;
			}

			// 先頭が#マークならこのパス指定は無視する
			if( path[ 0 ] == '#' )
			{
				// 不可
				return null ;
			}

			// 先頭が~マークなら変換無視
			if( path[ 0 ] == '~' )
			{
				// 不可
				noConvert = true ;
				path = path.TrimStart( '~' ) ;
				path = path.TrimStart( ' ' ) ;
			}

			// 先頭が!マークならアセットバンドルに含めないようにする
			if( path[ 0 ] == '!' )
			{
				// 除外
				isIgnore = true ;
				path = path.TrimStart( '!' ) ;
				path = path.TrimStart( ' ' ) ;
			}

			if( path.Length == 0 )
			{
				// 不可
				return null ;
			}

			// 先頭にスラッシュが付いていれば外す
			path = path.TrimStart( '/' ) ;
			if( path.Length == 0 )
			{
				// 不可
				return null ;
			}

			// 最後にスラッシュが付いていればフォルダ限定
			if( path[ path.Length - 1 ] == '/' )
			{
				if( noConvert == true )
				{
					// フォルダを対象とした変換無視は無効
					return null ;
				}

				folderOnly = true ;	// フォルダ限定
				path = path.TrimEnd( '/' ) ;
			}

			if( path.Length == 0 )
			{
				// 不可
				return null ;
			}

			// 最後にアスタリスクが付いていれば複数対象
			if( path[ path.Length - 1 ] == '*' )
			{
				wildCard = true ;	// 対象は親フォルダ内の全て
				path = path.TrimEnd( '*' ) ;
			}

			// 最後にスラッシュになってしまうようなら除外する
			while( path.Length >= 1 )
			{
				if( path[ path.Length - 1 ] == '/' )
				{
					path = path.Trim( '/' ) ;
				}
				else
				{
					break ;
				}
			}

			// パスが空文字の場合もありえる(それは有効)
			return path ;
		}

		// アセットバンドル化対象となるパスを取得する
		private string[] GetUpperPath( string path, string rootFolderPath )
		{
			if( string.IsNullOrEmpty( path ) == true || path.Contains( "*" ) == false )
			{
				return new string[]{ path } ;
			}

			// ワイルドカード部分を展開して全て個別のパスにする

			List<string> stackedPaths = new List<string>() ;

			// 一時的に最後にスラッシュを付ける
			path += "/" ;

			string currentPath = string.Empty ;

			// 再帰メソッドを呼ぶ
			GetUpperPath( path, currentPath, rootFolderPath, ref stackedPaths ) ;

			if( stackedPaths.Count == 0 )
			{
				// ワイルドカード内で有効なパスは存在しない
				return null ;
			}

			return stackedPaths.ToArray() ;
		}

		// 再帰処理側
		private void GetUpperPath( string path, string currentPath, string rootFolderPath, ref List<string> stackedPaths )
		{
			int p ;
			string token,  fixedPath ;

			string[]		folderPaths ;
			List<string>	assetPaths = new List<string>() ;

			//----------------------------------

			p = path.IndexOf( "/" ) ;

			// 最初のスラッシュまで切り出し(絶対に０より大きい値になる
			token = path.Substring( 0, p ) ;

			p ++ ;
			path = path.Substring( p, path.Length - p ) ;

			if( token != "*" )
			{
				// 固定
				fixedPath = currentPath ;
				if( string.IsNullOrEmpty( fixedPath ) == false )
				{
					fixedPath += "/" ;
				}
				fixedPath += token ;

				if( string.IsNullOrEmpty( path ) == false )
				{
					// まだ続きがある

					// 再帰的に処理する
					GetUpperPath( path, fixedPath, rootFolderPath, ref stackedPaths ) ;
				}
				else
				{
					// 最終的なパスが決定した
					stackedPaths.Add( fixedPath ) ;
				}
			}
			else
			{
				// 可変

				if( Directory.Exists( rootFolderPath + currentPath ) == false )
				{
					// フォルダが存在しない
					return ;
				}

				assetPaths.Clear() ;
				folderPaths = Directory.GetDirectories( rootFolderPath + currentPath ) ;
				if( folderPaths != null && folderPaths.Length >  0 )
				{
					foreach( var folderPath in folderPaths )
					{
						string assetPath = folderPath.Replace( "\\", "/" ) ;
						if( Directory.Exists( assetPath ) == true )
						{
							// フォルダ
							assetPaths.Add( assetPath.Replace( rootFolderPath + currentPath + "/", "" ) ) ;
						}
					}
				}

				if( assetPaths.Count == 0 )
				{
					// この枝は打ち止め
					return ;
				}

				if( string.IsNullOrEmpty( path ) == false )
				{
					// まだ続きがある

					// 再帰的に処理する
					foreach( var assetPath in assetPaths )
					{
						fixedPath = currentPath ;
						if( string.IsNullOrEmpty( fixedPath ) == false )
						{
							fixedPath += "/" ;
						}
						fixedPath += assetPath ;

						GetUpperPath( path, fixedPath, rootFolderPath, ref stackedPaths ) ;
					}
				}
				else
				{
					// 最終的なパスが決定した
					foreach( var assetPath in assetPaths )
					{
						fixedPath = currentPath ;
						if( string.IsNullOrEmpty( fixedPath ) == false )
						{
							fixedPath += "/" ;
						}
						fixedPath += assetPath ;

						// 追加
						stackedPaths.Add( fixedPath ) ;
					}
				}
			}
		}

		//-----------------------------------------------------------

		// 無効なフォルダかどうか判別する
		private bool CheckFolderType( string path )
		{
			path = path.Replace( '\\', '/' ).TrimStart( '/' ) ;

			string[] folderNames = path.Split( '/' ) ;
			int i, l = folderNames.Length ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( string.IsNullOrEmpty( folderNames[ i ] ) == false )
				{
					if( folderNames[ i ][ 0 ] == '#' || folderNames[ i ][ 0 ] == '!' )
					{
						return false ;
					}
				}
			}

			return true ;
		}

		//-----------------------------------

		// 拡張子をチェックして有効なファイルかどうか判別する
		private bool CheckFileType( string path )
		{
			int l = path.Length ;

			int i ;
			i = path.LastIndexOf( '/' ) ;
			if( ( i == -1 && ( i + 1 ) <  l ) || ( i >= 0 && ( i + 1 ) <  l ) )
			{
				// ファイル名
				i ++ ;
				if( path[ i ] == '#' || path[ i ] == '!' )
				{
					return false ;	// 無効なファイル
				}
			}

			i = path.LastIndexOf( '.' ) ;
			if( i >= 0 )
			{
				// 拡張子あり
				i ++ ;
				string extension = path.Substring( i, l - i ) ;
				if( string.IsNullOrEmpty( extension ) == false )
				{
					if( extension == "meta" || extension == "cs" || extension == "js" )
					{
						return false ;	// meta と cs と js 以外はＯＫ
					}
				}
			}

			return true ;
		}

		//-----------------------------------------------------------------
		// link.xml を出力する

		// コマンドライン専用(複数のマニフェストから生成)
		private bool CreateLinkXmlFile( TargetData[] targets, string linkFilePath )
		{
			System.GC.Collect() ;

			string listFilePath ;
			string rootFolderPath ;

			List<AssetBundleFile>	fullAssetBundleFiles = new List<AssetBundleFile>() ;

			AssetBundleFile[] assetBundleFiles ;

			int i, l = targets.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				listFilePath	= targets[ i ].ListFilePath ;
				rootFolderPath	= targets[ i ].RootFolderPath ;

				( assetBundleFiles, _ ) = GetAssetBundleFiles( listFilePath, rootFolderPath ) ;

				fullAssetBundleFiles.AddRange( assetBundleFiles ) ;
			}

			return CreateLinkXmlFile( fullAssetBundleFiles.ToArray(), linkFilePath ) ;
		}

		private bool CreateLinkXmlFile( AssetBundleFile[] assetBundleFiles, string linkFilePath )
		{
			//----------------------------------

			bool result = false ;

			var generator = new SimpleLinkXMLGenerator() ;

			List<string> assetPaths = new List<string>() ;
			foreach( var assetBundleFile in assetBundleFiles )
			{
				assetPaths.AddRange( assetBundleFile.AssetFiles.Select( value => value.AssetPath ) ) ;
			}

			Debug.Log( "<color=#FFFFFF>=======================================</color>" ) ;
			Debug.Log( "<color=#FF7FFF>総アセットハンドル数:" + assetBundleFiles.Length + "</color>" ) ;
			Debug.Log( "<color=#FF7FFF>総アセット数:" + assetPaths.Count + "</color>" ) ;
			Debug.Log( "<color=#FFFFFF>=======================================</color>" ) ;

			generator.AddAssets( assetPaths ) ;

			try
			{
				generator.Save( linkFilePath ) ;
				Debug.Log( "<color=#FF7F00>" + $"Create {linkFilePath}" + "</color>" ) ;

				result = true ;
			}
			catch( Exception e )
			{
				Debug.LogException( e ) ;

				result = false ;
			}

			return result ;
		}

		//-----------------------------------------------------------------

		// 必要なアセットバンドルを全て生成する
		private bool CreateAssetBundleAll()
		{
			bool result = false ;

			AssetBundleFile[]	assetBundleFiles ;
			string[]			ignoreFilePaths ;

			( assetBundleFiles, ignoreFilePaths ) = GetAssetBundleFiles( m_LocalAssetsListFilePath, m_LocalAssetsRootFolderPath ) ;

			if( assetBundleFiles != null && assetBundleFiles.Length >  0 )
			{
				result = CreateAssetBundleAll( assetBundleFiles, ignoreFilePaths ) ;
			}

			if( result == true && m_GenerateLinkFile == true )
			{
				//----------------------------------------------------------
				// link.xmlを生成する
				Debug.Log( "<color=#00FF00>自動で link.xml を生成する</color>" ) ; 
				CreateLinkXmlFile( assetBundleFiles, m_LinkFilePath ) ;

				//----------------------------------------------------------
			}

			return result ;
		}

		// 必要なアセットバンドルを全て生成する
		private bool CreateAssetBundleAll( AssetBundleFile[] assetBundleFiles, string[] ignoreFilePaths )
		{
			if( assetBundleFiles == null || assetBundleFiles.Length == 0 )
			{
				return false ;
			}

			int i, l, c, m ;
			bool result ;
			string[] assetBundleNames = null ;
			string[] assetNames = null ;

			//-----------------------------------------------------------------------------

			// アセットバンドルファイルの階層が浅い方から順にビルドするようにソートする(小さい値の方が先)

			//-----------------------------------------------------------------------------

			// 保存先ルートフォルダ
			string assetBundleRootFolderPath = m_AssetBundleRootFolderPath.TrimStart( '/' ).TrimEnd( '/' ) ;

			if( Directory.Exists( assetBundleRootFolderPath ) == false )
			{
				// 出力フォルダが存在しない場合はエラーにはせず生成する
				if( Directory.CreateDirectory( assetBundleRootFolderPath ) != null )
				{
					AssetDatabase.Refresh() ;
				}
			}

			//----------------------------------

			// アセットバンドルの出力構造が変化しフォルダが必要な箇所に既に同名のファイルが存在する場合に事前に削除する
			ReduceDiscardedAssetBundleFiles( assetBundleRootFolderPath, assetBundleFiles ) ;

			//----------------------------------

			BuildAssetBundleOptions options = BuildAssetBundleOptions.DeterministicAssetBundle ;

			if( m_ChunkBasedCompression == true )
			{
				options |= BuildAssetBundleOptions.ChunkBasedCompression ;
			}

			if( m_ForceRebuildAssetBundle == true )
			{
				options |= BuildAssetBundleOptions.ForceRebuildAssetBundle ;
			}

			if( m_IgnoreTypeTreeChanges == true && m_DisableWriteTypeTree == false )
			{
				options |= BuildAssetBundleOptions.IgnoreTypeTreeChanges ;
			}

			if( m_DisableWriteTypeTree == true && m_IgnoreTypeTreeChanges == false  )
			{
				options |= BuildAssetBundleOptions.DisableWriteTypeTree ;
			}

			if ( m_StrictMode )
			{
				options |= BuildAssetBundleOptions.StrictMode ;
			}

			//---------------------------------------------------------

			// 無視リスト
//			foreach( var ignoreFilePath in m_IgnoreFilePaths )
//			{
//				Debug.Log( "[無視パス]" + ignoreFilePath ) ;
//			}

			//---------------------------------------------------------

			if( m_CollectDependencies == false )
			{
				// 新版(依存アセット除外あり)
				l = assetBundleFiles.Length ;

				// ここからが新版のメイン生成処理
				List<AssetBundleBuild> maps = new List<AssetBundleBuild>() ;
				AssetBundleBuild map ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					if( assetBundleFiles[ i ].NoConvert == false )
					{
						map = PostAssetBundleFile( assetBundleFiles[ i ], ignoreFilePaths ) ;
						if( map.assetNames != null && map.assetNames.Length >  0 )
						{
							maps.Add( map ) ;
						}
					}
				}

				//--------------------

				if( maps.Count >  0 )
				{
					//--------------------------------------------------------
					// 出力するアセットバンドルファイル名と同名のフォルダが既に存在する場合は削除しておかないとビルドに失敗する

					string path ;

					l = maps.Count ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						path = assetBundleRootFolderPath + "/" + maps[ i ].assetBundleName ;

						if( Directory.Exists( path ) == true )
						{
							Directory.Delete( path, true ) ;
							path += ".meta" ;
							if( File.Exists( path ) == true )
							{
								// フォルダのメタファイルも削除する
								File.Delete( path ) ;
							}
						}
					}

					//--------------------------------------------------------

					AssetBundleManifest manifest ;

					// アセットバンドルの生成
					manifest = BuildPipeline.BuildAssetBundles
					(
						assetBundleRootFolderPath,
						maps.ToArray(),
						options,
						m_BuildTarget
					) ;

					if( manifest != null )
					{
						result = true ;
						assetBundleNames = manifest.GetAllAssetBundles() ;
					}
					else
					{
						// 失敗
						result = false ;
					}
				}
				else
				{
					result = true ;
				}
			}
			else
			{
				// 旧版(依存アセット除外なし)
				l = assetBundleFiles.Length ;

				// ここからが新版のメイン生成処理
				AssetBundleBuild map ;

				AssetBundleManifest manifest ;

				string[] nameArray ;
				List<string> nameList = new List<string>() ;
				List<string> hashList = new List<string>() ;

				string path ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					if( assetBundleFiles[ i ].NoConvert == false )
					{
						map = PostAssetBundleFile( assetBundleFiles[ i ], ignoreFilePaths ) ;

						if( map.assetNames != null && map.assetNames.Length >  0 )
						{
							//------------------------------
							// 出力するアセットバンドルファイル名と同名のフォルダが既に存在する場合は削除しておかないとビルドに失敗する

							path = assetBundleRootFolderPath + "/" + map.assetBundleName ;

							if( Directory.Exists( path ) == true )
							{
								Directory.Delete( path, true ) ;
								path += ".meta" ;
								if( File.Exists( path ) == true )
								{
									// フォルダのメタファイルも削除する
									File.Delete( path ) ;
								}
							}

							//------------------------------

							// アセットバンドルの生成
							manifest = BuildPipeline.BuildAssetBundles
							(
								assetBundleRootFolderPath,
								new AssetBundleBuild[]{ map },
								options,
								m_BuildTarget
							) ;

							if( manifest == null )
							{
								break ;
							}

							nameArray = manifest.GetAllAssetBundles() ;
							if( nameArray != null && nameArray.Length >  0 )
							{
								nameList.Add( nameArray[ 0 ] ) ;	// 常に１つのはず
								hashList.Add( manifest.GetAssetBundleHash( nameArray[ 0 ] ).ToString() ) ;
							}
						}
					}
				}

				if( i >= l )
				{
					// 全て成功
					result = true ;

					assetBundleNames = nameList.ToArray() ;

					// ハッシュリストを生成して保存する
					l = nameList.Count ;
					string text = string.Empty ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						text += ( nameList[ i ] + "," + hashList[ i ] + "\n" ) ;
					}

					File.WriteAllText( m_AssetBundleRootFolderPath + GetAssetBundleRootName() + ".list", text ) ;
				}
				else
				{
					// 失敗
					result = false ;
				}
			}

			//-------------------------------------------------------------------------------

			if( result == true )
			{
				// アセットバンドル化しないファイルを出力する

				List<string> nameList = new List<string>() ;
				l = assetBundleFiles.Length ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					if( assetBundleFiles[ i ].NoConvert == true )
					{
						if( ignoreFilePaths == null || ignoreFilePaths.Length == 0 || ignoreFilePaths.Contains( assetBundleFiles[ i ].AssetBundlePath ) == false )
						{
							nameList.Add( assetBundleFiles[ i ].AssetBundlePath.ToLower() ) ;	// 全て小文字化(アセットバンドルの動作に合わせる)
							CopyAssetBundleFile( assetBundleFiles[ i ] ) ;
						}
					}
				}

				assetNames = nameList.ToArray() ;
			}

			//-------------------------------------------------------------------------------

			// バージョンファイルを出力する
			if( m_GenerateVersionFile == true )
			{
				DateTime dt = DateTime.Now ;
				string version = "Last Update : " + dt.ToString( "yyyy/MM/dd HH:mm:ss" ) ;
//				Debug.Log( "パス:" +  m_AssetBundleRootFolderPath + GetAssetBundleRootName() + ".txt" ) ;
				Debug.Log( "<color=#00FFFF>" + version + "</color>" ) ;
				File.WriteAllText( m_AssetBundleRootFolderPath + GetAssetBundleRootName() + ".txt", version ) ;
			}

			// ＣＲＣファイルを出力する(CSV版)
			if( m_GenerateCrcFile_Csv == true || m_GenerateCrcFile_Json == true )
			{
//				Debug.LogWarning( "保存先ルートフォルダ:" + m_AssetBundleRootFolderPath ) ;
//				Debug.LogWarning( "保存マニフェスト名:" + GetAssetBundleRootName() ) ;

				byte[] data ;
				int size ;
				uint crc ;
				string path ;

				long totalSize = 0 ;

				string textCsv	= string.Empty ;
				string textJson	= string.Empty ;

				AssetBundleFile assetBundleFile ;

				l = assetBundleFiles.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					assetBundleFile = assetBundleFiles[ i ] ;

					path = m_AssetBundleRootFolderPath + assetBundleFile.AssetBundlePath ;
					if( File.Exists( path ) == true )
					{
						data = File.ReadAllBytes( m_AssetBundleRootFolderPath + assetBundleFile.AssetBundlePath ) ;
						if( data != null && data.Length >  0 )
						{
							size  = data.Length ;
							totalSize += size ;

							crc   = GetCRC32( data ) ;

							//------------------------------------------------------
							// Csv 版

							textCsv += ( assetBundleFile.AssetBundlePath + "," + size + "," + crc ) ;
							if( assetBundleFile.Tags != null && assetBundleFile.Tags.Length >  0 )
							{
								textCsv += "," ;

								m = assetBundleFile.Tags.Length ;
								for( c  = 0 ; c <   m ; c ++ )
								{
									textCsv += assetBundleFile.Tags[ c ] ;
									if( i <  ( m - 1 ) )
									{
										textCsv += " " ;	// タグの区切り記号はスペース
									}
								}
							}
							textCsv += "\n" ;

							//------------------------------------------------------
							// Json 版

							textJson += "{" ;

							textJson +=
								"\"Path\":\"" + assetBundleFile.AssetBundlePath + "\"," +
								"\"Size\":" + size.ToString() + "," +
								"\"Crc\":" + crc.ToString() ;

							if( assetBundleFile.Tags != null && assetBundleFile.Tags.Length >  0 )
							{
								textJson += ",\"Tags\":[" ;

								m = assetBundleFile.Tags.Length ;
								for( c  = 0 ; c <  m ; c ++ )
								{
									textJson += ( "\"" + assetBundleFile.Tags[ c ] + "\"" ) ;
									if( i <  ( m - 1 ) )
									{
										textJson += "," ;	// タグの区切り記号はスペース
									}
								}

								textJson += "]" ;
							}

							textJson += "}" ;

							if( i <  ( l - 1 ) )
							{
								textJson += ",\n" ;
							}
						}
					}
				}

				if( m_GenerateCrcFile_Csv  == true  & string.IsNullOrEmpty( textCsv  ) == false )
				{
					File.WriteAllText( m_AssetBundleRootFolderPath + GetAssetBundleRootName() + ".csv",  textCsv ) ;
				}

				if( m_GenerateCrcFile_Json == true  & string.IsNullOrEmpty( textJson ) == false )
				{
					textJson = "{\"AssetBundleFiles\":[\n" + textJson + "\n]}" ;

					File.WriteAllText( m_AssetBundleRootFolderPath + GetAssetBundleRootName() + ".json", textJson ) ;
				}

				Debug.Log( "<color=#FFFFFF>=======================================</color>" ) ;
				Debug.Log( "<color=#FF00FF>CRC File Saved</color>" ) ;
				Debug.Log( "<color=#FF00FF>Manifest Name : " + GetAssetBundleRootName() + "</color>" ) ;
				Debug.Log( "<color=#FFFF00>File Count : " + assetBundleFiles.Length + "</color>" ) ;
				Debug.Log( "<color=#00FF00>Total Size : " + GetSizeName( ( ulong )totalSize ) + "</color>" ) ;
			}

			//-------------------------------------------------------------------------------

			if( m_Clean == true && result == true && ( ( assetBundleNames != null && assetBundleNames.Length >  0 ) || ( assetNames != null && assetNames.Length >  0 ) ) )
			{
				// 余計なファイルを削除する
				CleanAssetBundle( assetBundleNames, assetNames ) ;
			}

			AssetDatabase.Refresh() ;

			return result ;
		}

	// ファイルサイズを見やすい形に変える
		private static string GetSizeName( ulong size )
		{
			string sizeName = "Value Overflow" ;

			if( size <  1024L )
			{
				sizeName = size + " byte" ;
			}
			else
			if( size <  ( 1024L * 1024L ) )
			{
				sizeName = ( size / 1024L ) + " KB" ;
			}
			else
			if( size <  ( 1024L * 1024L * 1024L ) )
			{
				sizeName = ( size / ( 1024L * 1024L ) ) + " MB" ;
			}
			else
			if( size <  ( 1024L * 1024L * 1024L * 1024L ) )
			{
				double value = ( double )size / ( double )( 1024L * 1024L * 1024L ) ;
				value = ( double )( ( int )( value * 1000 ) ) / 1000 ;	// 少数までわかるようにする
				sizeName = value + " GB" ;
			}
			else
			if( size <  ( 1024L * 1024L * 1024L * 1024L * 1024L ) )
			{
				sizeName = ( size / ( 1024L * 1024L * 1024L * 1024L ) ) + " TB" ;
			}
			else
			if( size <  ( 1024L * 1024L * 1024L * 1024L * 1024L * 1024L ) )
			{
				sizeName = ( size / ( 1024L * 1024L * 1024L * 1024L * 1024L ) ) + " PB" ;
			}

			return sizeName ;
		}

		// 不要になる既存のアセットバンドルファイルを削除する
		private void ReduceDiscardedAssetBundleFiles( string assetBundleRootFolderPath, AssetBundleFile[] assetBundleFiles )
		{
			foreach( var assetBundleFile in assetBundleFiles )
			{
				string path = assetBundleFile.AssetBundlePath.ToLower() ;
				int i = path.LastIndexOf( "/" ) ;
				if( i >= 0 )
				{
					// フォルダ内のファイルなので処理を行う
					path = path.Substring( 0, i ) ;	// フォルダ部分のみにする
					string[] folderNames ;
					if( path.Contains( "/" ) == false )
					{
						folderNames = new string[]{ path } ;
					}
					else
					{
						folderNames = path.Split( '/' ) ;
					}

					path = assetBundleRootFolderPath ;

					foreach( var folderName in folderNames )
					{
						path += "/" + folderName ;
						if( File.Exists( path ) == true )
						{
							Debug.LogWarning( "Remove File : " + path ) ;
							File.Delete( path ) ;

							string fileName ;

							string[] extensions = { ".meta", ".manifest", ".manifest.meta" } ;
							foreach( var extension in extensions )
							{
								fileName = path + extension ;
								if( File.Exists( fileName ) == true )
								{
									File.Delete( fileName ) ;
								}
							}
						}
					}
				}
			}
		}

		// アセットバンドルの情報を格納する
		private AssetBundleBuild PostAssetBundleFile( AssetBundleFile assetBundleFile, string[] ignoreFilePaths )
		{
			AssetBundleBuild map = new AssetBundleBuild() ;

			string assetBundlePath = assetBundleFile.AssetBundlePath ;

			if( ignoreFilePaths != null && ignoreFilePaths.Length >  0 && ignoreFilePaths.Contains( assetBundlePath ) == true )
			{
				// そもそもこのアセットバンドル自体が生成対象外
				return map ;
			}

			//----------------------------------------------------------

			map.assetBundleName = assetBundlePath ;
			map.assetBundleVariant = string.Empty ;

//			Debug.LogWarning( "Map assetBundleName:" + map.assetBundleName ) ;

			List<string> assetPaths = new List<string>() ;

			foreach( var assetFile in assetBundleFile.AssetFiles )
			{
				// このアセット自体は確実にアセットバンドルに含まれる
				if( assetFile.AssetType == 0 )
				{
					if( ( ignoreFilePaths == null || ignoreFilePaths.Length == 0 ) || ignoreFilePaths.Contains( assetFile.AssetPath ) == false )
					{
						assetPaths.Add( assetFile.AssetPath ) ;
					}
				}
			}

			if( assetPaths.Count == 0 )
			{
				// このアセットバンドル生成は無効
				return map ;
			}

			map.assetNames = assetPaths.ToArray() ;

			return map ;
		}

		// 無変換アセットバンドルファイルをコピーする
		private void CopyAssetBundleFile( AssetBundleFile assetBundleFile )
		{
			string assetBundlePath = assetBundleFile.AssetBundlePath.ToLower() ;
			string assetPath = assetBundleFile.AssetFiles[ 0 ].AssetPath ;

			string storePath = m_AssetBundleRootFolderPath + "/" + assetBundlePath ;

			// 名前にフォルダが含まれているかチェックする
			int i = storePath.LastIndexOf( '/' ) ;
			if( i >= 0 )
			{
				// フォルダが含まれている
				string folderName = storePath.Substring( 0, i ) ;

				if( Directory.Exists( folderName ) == false )
				{
					// フォルダを生成する(多階層をまとめて生成出来る)
					Directory.CreateDirectory( folderName ) ;
				}
			}

			File.Copy( assetPath, m_AssetBundleRootFolderPath + "/" + assetBundlePath, true ) ;
		}

		//---------------------------------------------------------------------------

		private const uint CRC32_MASK = 0xffffffff ;

		// ＣＲＣ値を取得する
		public static uint GetCRC32( byte[] data )
		{
			uint value = CRC32_MASK  ;

			foreach( var code in data )
			{
				value = m_CRC32_Table[ ( value ^ code ) & 0xFF ] ^ ( value >> 8 ) ;
			}

			return value ^ CRC32_MASK ;
		}

		private readonly static uint[] m_CRC32_Table = new uint[]
		{
			0x00000000, 0x77073096, 0xee0e612c, 0x990951ba, 0x076dc419,
			0x706af48f, 0xe963a535, 0x9e6495a3, 0x0edb8832, 0x79dcb8a4,
			0xe0d5e91e, 0x97d2d988, 0x09b64c2b, 0x7eb17cbd, 0xe7b82d07,
			0x90bf1d91, 0x1db71064, 0x6ab020f2, 0xf3b97148, 0x84be41de,
			0x1adad47d, 0x6ddde4eb, 0xf4d4b551, 0x83d385c7, 0x136c9856,
			0x646ba8c0, 0xfd62f97a, 0x8a65c9ec, 0x14015c4f, 0x63066cd9,
			0xfa0f3d63, 0x8d080df5, 0x3b6e20c8, 0x4c69105e, 0xd56041e4,
			0xa2677172, 0x3c03e4d1, 0x4b04d447, 0xd20d85fd, 0xa50ab56b,
			0x35b5a8fa, 0x42b2986c, 0xdbbbc9d6, 0xacbcf940, 0x32d86ce3,
			0x45df5c75, 0xdcd60dcf, 0xabd13d59, 0x26d930ac, 0x51de003a,
			0xc8d75180, 0xbfd06116, 0x21b4f4b5, 0x56b3c423, 0xcfba9599,
			0xb8bda50f, 0x2802b89e, 0x5f058808, 0xc60cd9b2, 0xb10be924,
			0x2f6f7c87, 0x58684c11, 0xc1611dab, 0xb6662d3d, 0x76dc4190,
			0x01db7106, 0x98d220bc, 0xefd5102a, 0x71b18589, 0x06b6b51f,
			0x9fbfe4a5, 0xe8b8d433, 0x7807c9a2, 0x0f00f934, 0x9609a88e,
			0xe10e9818, 0x7f6a0dbb, 0x086d3d2d, 0x91646c97, 0xe6635c01,
			0x6b6b51f4, 0x1c6c6162, 0x856530d8, 0xf262004e, 0x6c0695ed,
			0x1b01a57b, 0x8208f4c1, 0xf50fc457, 0x65b0d9c6, 0x12b7e950,
			0x8bbeb8ea, 0xfcb9887c, 0x62dd1ddf, 0x15da2d49, 0x8cd37cf3,
			0xfbd44c65, 0x4db26158, 0x3ab551ce, 0xa3bc0074, 0xd4bb30e2,
			0x4adfa541, 0x3dd895d7, 0xa4d1c46d, 0xd3d6f4fb, 0x4369e96a,
			0x346ed9fc, 0xad678846, 0xda60b8d0, 0x44042d73, 0x33031de5,
			0xaa0a4c5f, 0xdd0d7cc9, 0x5005713c, 0x270241aa, 0xbe0b1010,
			0xc90c2086, 0x5768b525, 0x206f85b3, 0xb966d409, 0xce61e49f,
			0x5edef90e, 0x29d9c998, 0xb0d09822, 0xc7d7a8b4, 0x59b33d17,
			0x2eb40d81, 0xb7bd5c3b, 0xc0ba6cad, 0xedb88320, 0x9abfb3b6,
			0x03b6e20c, 0x74b1d29a, 0xead54739, 0x9dd277af, 0x04db2615,
			0x73dc1683, 0xe3630b12, 0x94643b84, 0x0d6d6a3e, 0x7a6a5aa8,
			0xe40ecf0b, 0x9309ff9d, 0x0a00ae27, 0x7d079eb1, 0xf00f9344,
			0x8708a3d2, 0x1e01f268, 0x6906c2fe, 0xf762575d, 0x806567cb,
			0x196c3671, 0x6e6b06e7, 0xfed41b76, 0x89d32be0, 0x10da7a5a,
			0x67dd4acc, 0xf9b9df6f, 0x8ebeeff9, 0x17b7be43, 0x60b08ed5,
			0xd6d6a3e8, 0xa1d1937e, 0x38d8c2c4, 0x4fdff252, 0xd1bb67f1,
			0xa6bc5767, 0x3fb506dd, 0x48b2364b, 0xd80d2bda, 0xaf0a1b4c,
			0x36034af6, 0x41047a60, 0xdf60efc3, 0xa867df55, 0x316e8eef,
			0x4669be79, 0xcb61b38c, 0xbc66831a, 0x256fd2a0, 0x5268e236,
			0xcc0c7795, 0xbb0b4703, 0x220216b9, 0x5505262f, 0xc5ba3bbe,
			0xb2bd0b28, 0x2bb45a92, 0x5cb36a04, 0xc2d7ffa7, 0xb5d0cf31,
			0x2cd99e8b, 0x5bdeae1d, 0x9b64c2b0, 0xec63f226, 0x756aa39c,
			0x026d930a, 0x9c0906a9, 0xeb0e363f, 0x72076785, 0x05005713,
			0x95bf4a82, 0xe2b87a14, 0x7bb12bae, 0x0cb61b38, 0x92d28e9b,
			0xe5d5be0d, 0x7cdcefb7, 0x0bdbdf21, 0x86d3d2d4, 0xf1d4e242,
			0x68ddb3f8, 0x1fda836e, 0x81be16cd, 0xf6b9265b, 0x6fb077e1,
			0x18b74777, 0x88085ae6, 0xff0f6a70, 0x66063bca, 0x11010b5c,
			0x8f659eff, 0xf862ae69, 0x616bffd3, 0x166ccf45, 0xa00ae278,
			0xd70dd2ee, 0x4e048354, 0x3903b3c2, 0xa7672661, 0xd06016f7,
			0x4969474d, 0x3e6e77db, 0xaed16a4a, 0xd9d65adc, 0x40df0b66,
			0x37d83bf0, 0xa9bcae53, 0xdebb9ec5, 0x47b2cf7f, 0x30b5ffe9,
			0xbdbdf21c, 0xcabac28a, 0x53b39330, 0x24b4a3a6, 0xbad03605,
			0xcdd70693, 0x54de5729, 0x23d967bf, 0xb3667a2e, 0xc4614ab8,
			0x5d681b02, 0x2a6f2b94, 0xb40bbe37, 0xc30c8ea1, 0x5a05df1b,
			0x2d02ef8d
		} ;

		//-----------------------------------------------------------------

		// 出力パスのフォルダ名を取得する
		private string GetAssetBundleRootName()
		{
			if( string.IsNullOrEmpty( m_AssetBundleRootFolderPath ) == true )
			{
				return string.Empty ;
			}

			// 出力パスのアセットバンドル
			string path = m_AssetBundleRootFolderPath ;
			if( path[ path.Length - 1 ] == '/' )
			{
				path = path.Substring( 0, path.Length - 1 ) ;
			}
			int i = path.LastIndexOf( '/' ) ;
			if( i >= 0 )
			{
				path = path.Substring( i + 1, path.Length - ( i + 1 ) ) ;
			}

			return path ;
		}

		// 不要になったアセットバンドルファイルを削除する
		private void CleanAssetBundle( string[] assetBundleNames, string[] assetNames )
		{
			List<string> list = new List<string>() ;

			//-------------------------------------------------

			// 削除対象から除外するファイル名を登録する

			// 出力パスのアセットバンドル
			string path = GetAssetBundleRootName() ;

			// シングルマニフェストアセットバンドル
			if( m_CollectDependencies == false )
			{
				// 依存ありタイプ
				list.Add( m_AssetBundleRootFolderPath + path ) ;
				list.Add( m_AssetBundleRootFolderPath + path + ".meta" ) ;
				list.Add( m_AssetBundleRootFolderPath + path + ".manifest" ) ;
				list.Add( m_AssetBundleRootFolderPath + path + ".manifest.meta" ) ;
			}
			else
			{
				// 依存なしタイプ
				list.Add( m_AssetBundleRootFolderPath + path + ".list" ) ;
				list.Add( m_AssetBundleRootFolderPath + path + ".list.meta" ) ;
			}

			// バージョンファイル
			if( m_GenerateVersionFile == true )
			{
				list.Add( m_AssetBundleRootFolderPath + path + ".txt" ) ;
				list.Add( m_AssetBundleRootFolderPath + path + ".txt.meta" ) ;
			}

			// ＣＲＣファイル(Csv版)
			if( m_GenerateCrcFile_Csv == true )
			{
				list.Add( m_AssetBundleRootFolderPath + path + ".csv" ) ;
				list.Add( m_AssetBundleRootFolderPath + path + ".csv.meta" ) ;
			}

			// ＣＲＣファイル(Json版)
			if( m_GenerateCrcFile_Json == true )
			{
				list.Add( m_AssetBundleRootFolderPath + path + ".json" ) ;
				list.Add( m_AssetBundleRootFolderPath + path + ".json.meta" ) ;
			}

			// 各アセットバンドル
			if( assetBundleNames != null && assetBundleNames.Length >  0 )
			{
				foreach( var assetBundleName in assetBundleNames )
				{
					list.Add( m_AssetBundleRootFolderPath + assetBundleName ) ;
					list.Add( m_AssetBundleRootFolderPath + assetBundleName + ".meta" ) ;
					list.Add( m_AssetBundleRootFolderPath + assetBundleName + ".manifest" ) ;
					list.Add( m_AssetBundleRootFolderPath + assetBundleName + ".manifest.meta" ) ;
				}
			}

			// 各アセット
			if( assetNames != null && assetNames.Length >  0 )
			{
				foreach( var assetName in assetNames )
				{
					list.Add( m_AssetBundleRootFolderPath + assetName ) ;
					list.Add( m_AssetBundleRootFolderPath + assetName + ".meta" ) ;
				}
			}

			// 上記の登録されたファイル以外は全て削除される
			//---------------------------------------------------------

			// 再帰を使って不要になったアセットバンドルファイルを全て削除する
			int sweptFileCount = 0 ;
			CleanAssetBundle( list, m_AssetBundleRootFolderPath, ref sweptFileCount ) ;

			Debug.Log( "<color=#FFFFBF>[SimplaAssitBundleBuilder] Swept File Count : " + sweptFileCount + "</color>" ) ;
		}

		// 不要になったアセットバンドルファイルを削除(再帰)
		private int CleanAssetBundle( List<string> list, string currentPath, ref int sweptFileCount )
		{
			int i ;
			string path ;

			//-----------------------------------------------------

			if( Directory.Exists( currentPath ) == false )
			{
				return 0 ;
			}

			//-----------------------------------------------------

			int c = 0 ;	// フォルダ・ファイルを残すカウント
			int d = 0 ;	// フォルダ・ファイルを消すカウント

			// フォルダ
			string[] da = Directory.GetDirectories( currentPath ) ;
			if( da != null && da.Length >  0 )
			{
				// サブフォルダがあるのでさらに検査していく
				for( i  = 0 ; i <  da.Length ; i ++ )
				{
					// サブフォルダを検査
					path = da[ i ] + "/" ;
					if( CleanAssetBundle( list, path, ref sweptFileCount ) == 0 )
					{
						// このサブフォルダは削除する
						Debug.Log( "<color=#FFFFBF>[SimplaAssitBundleBuilder] Swept Folder Path = " + list + "</color>" ) ;

						Directory.Delete( path, true ) ;

						d ++ ;	// 削除したフォルダ＋ファイル数
					}
					else
					{
						// フォルダのメタファイルを削除対象から除外する
						if( path[ path.Length - 1 ] == '/' )
						{
							path = path.Substring( 0, path.Length - 1 ) ;
						}
						path += ".meta" ;

						if( list.Contains( path ) == false )
						{
							list.Add( path ) ;
						}

						c ++ ;	// 残すファイル数
					}
				}
			}

			// ファイル
			string[] fa = Directory.GetFiles( currentPath ) ;
			if( fa != null && fa.Length >  0 )
			{
				for( i  = 0 ; i <  fa.Length ; i ++ )
				{
					if( list.Contains( fa[ i ] ) == false )
					{
						// 削除対象

						Debug.Log( "<color=#FFFFBF>[SimplaAssitBundleBuilder] Swept File Path = " + fa[ i ] + "</color>" ) ;

						File.Delete( fa[ i ] ) ;

						d ++ ;	// 削除したフォルダ＋ファイル数

						sweptFileCount ++ ;	// 削除したファイル数
					}
					else
					{
						c ++ ;	// 残すファイル数
					}
				}
			}

//			Debug.Log( "<color=#FFFFBF>[SimplaAssitBundleBuilder] Swept File Count : " + currentPath + " = " + d + "</color>" ) ;

			return c ;
		}
#if false
		// 依存するアセットをコンソールに表示する
		private void DebugPrintDependencies( string path )
		{
			// 依存関係にあるアセットを検出する
			string[] dependenciesPaths = AssetDatabase.GetDependencies( path ) ;
			if( dependenciesPaths!= null && dependenciesPaths.Length >  0 )
			{
				foreach( var dependenciesPath in dependenciesPaths )
				{
					Debug.LogWarning( "依存:" + dependenciesPath ) ;
				}
			}
		}
#endif
		public void OnPostprocessAssetbundleNameChanged( string assetPath, string previousAssetBundleName, string newAssetBundleName )
		{
			Debug.Log( "Asset " + assetPath + " has been moved from assetBundle " + previousAssetBundleName + " to assetBundle " + newAssetBundleName + "." ) ;
		}

		//----------------------------------------------------------------------------------------------

		private static readonly Dictionary<string,string> m_Japanese_Message = new Dictionary<string, string>()
		{
			{ "SelectLocalAssetsPath",	"AssetBundle化したいファイル一覧が記述されたリストファイルを設定してください" },
			{ "SelectAssetBundlePath",	"生成したAssetBundleを格納するフォルダを設定してください" },
			{ "SelectLinkXmlFilePath",	"アセンブリ参照ファイルのパスを設定してください" },
			{ "SelectAllLocalAssts",	"AssetBundle化対象はプロジェクト全体のAssetLabel入力済みファイルとなります" },
			{ "SamePath",				"LocalAssetsフォルダとAssetBundleフォルダに同じものは指定できません" },
			{ "RootPath",				"プロジェクトのルートフォルダ\n\n%1\n\nにAssetBundleを生成します\n\n本当によろしいですか？" },
			{ "Succeed",				"成功しました" },
			{ "Yes",					"はい" },
			{ "No",						"いいえ" },
			{ "OK",						"閉じる" },
		} ;
		private static readonly Dictionary<string,string> m_English_Message = new Dictionary<string, string>()
		{
			{ "SelectLocalAssetsPath",	"Please set up a list file that lists the files you want AssetBundle." },
			{ "SelectAssetBundlePath",	"Please set the folder in which to store the generated AssetBundle." },
			{ "SelectLinkXmlFilePath",	"Please set the path of the assembly reference file." },
			{ "SelectAllLocalAssets",	"AssetBundle target will be AssetLabel entered file of the entire project." },
			{ "SamePath",				"The same thing can not be specified in the LocalAssets folder and AssetBundle folder." },
			{ "RootPath",				"Asset Bundle Root Path is \n\n '%1'\n\nReally ?" },
			{ "Succeed",				"All Succeed !!" },
			{ "Yes",					"Yes" },
			{ "No",						"No" },
			{ "OK",						"OK" },
		} ;

		private string GetMessage( string label )
		{
			if( Application.systemLanguage == SystemLanguage.Japanese )
			{
				if( m_Japanese_Message.ContainsKey( label ) == false )
				{
					return "指定のラベル名が見つかりません" ;
				}
				return m_Japanese_Message[ label ] ;
			}
			else
			{
				if( m_English_Message.ContainsKey( label ) == false )
				{
					return "Specifying the label name can not be found." ;
				}
				return m_English_Message[ label ] ;
			}
		}

		//----------------------------------------------------------------------------

		// コマンドラインからの実行可能版(設定ファイルでパラメータを設定する)
		public static bool Build()
		{
			string path = GetConfigurationFilePtah() ;
			if( string.IsNullOrEmpty( path ) == true )
			{
				Debug.Log( "Error : Bad Configuration File !!" ) ;
				return false ;
			}

			return Build( path ) ;
		}

		public static bool Build( string path )
		{
			SimpleAssetBundleBuilder sabb = ScriptableObject.CreateInstance<SimpleAssetBundleBuilder>() ;

			if( sabb.LoadConfiguration( path ) == false )
			{
				DestroyImmediate( sabb ) ;

				Debug.Log( "Error : Bad Configuration File !!" ) ;

				return false ;
			}

			//-----------------------------------------

			// アセットバンドルをビルドする
			sabb.CreateAssetBundleAll() ;

			// 最後にオブジェクトを破棄する
			DestroyImmediate( sabb ) ;

			return true ;
		}

		// コンフィギュレーションファイルのパスを取得する
		private static string GetConfigurationFilePtah()
		{
			string[] args = Environment.GetCommandLineArgs() ;
			if( args == null || args.Length == 0 )
			{
				return null ;
			}

			string path = "" ;

			int i, l = args.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( args[ i ].ToLower() == "-settings" && ( i + 1 ) <  l )
				{
					// 発見した
					path = args[ i + 1 ] ;
					break ;
				}
			}

			if( i >= l || string.IsNullOrEmpty( path ) == true )
			{
				return null ;
			}

			return path ;
		}

		// コンフィグ情報を読み出す
		private bool LoadConfiguration( string path )
		{
			if( File.Exists( path ) == false )
			{
				return false ;
			}

			string code = File.ReadAllText( path ) ;

			if( string.IsNullOrEmpty( code ) == true )
			{
				return false ;
			}

			//-------------------------------------------------

			string[] line = code.Split( '\n' ) ;
			int i, l = line.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				line[ i ] = line[ i ].Replace( " ", "" ) ;
				line[ i ] = line[ i ].Replace( "\n", "" ) ;
				line[ i ] = line[ i ].Replace( "\t", "" ) ;
				line[ i ] = line[ i ].Replace( "\r", "" ) ;	// 超重要
				string[] data = line[ i ].Split( '=' ) ;

				if( data.Length == 2 )
				{
					string label = data[ 0 ].ToLower() ;
					string value = data[ 1 ] ;

					if( label == "LocalAssetsListFilePath".ToLower() )
					{
						m_LocalAssetsListFilePath = CorrectFilePath( value ) ;
						Debug.Log( "[Log]LocalAssetsListFilePath:" + m_LocalAssetsListFilePath ) ;
					}

					if( label == "LocalAssetsRootFolderPath".ToLower() )
					{
						m_LocalAssetsRootFolderPath = CorrectFolderPath( value ) ;
						Debug.Log( "[Log]LocalAssetsRootFolderPath:" + m_LocalAssetsRootFolderPath ) ;
					}

					if( label == "AssetBundleRootFolderPath".ToLower() )
					{
						m_AssetBundleRootFolderPath = CorrectFolderPath( value ) ;
						Debug.Log( "[Log]AssetBundleRootFolderPath:" + m_AssetBundleRootFolderPath ) ;
					}

					if( label == "BuildTarget".ToLower() )
					{
						m_BuildTarget = GetBuildTarget( value ) ;
					}

					if( label == "ChunkBasedCompression".ToLower() )
					{
						m_ChunkBasedCompression = GetBoolean( value ) ;
					}

					if( label == "ForceRebuildAssetBundle".ToLower() )
					{
						m_ForceRebuildAssetBundle = GetBoolean( value ) ;
					}

					if( label == "IgnoreTypeTreeChanges".ToLower() )
					{
						m_IgnoreTypeTreeChanges = GetBoolean( value ) ;
					}

					if( label == "DisableWriteTypeTree".ToLower() )
					{
						m_DisableWriteTypeTree = GetBoolean( value ) ;
					}

					if( label == "CollectDependencies".ToLower() )
					{
						m_CollectDependencies = GetBoolean( value ) ;
					}
				}
			}

			return true ;
		}

		// パスの整形を行う
		private static string CorrectFolderPath( string path )
		{
			if( string.IsNullOrEmpty( path ) == true )
			{
				return path ;
			}

			path = path.Replace( "\\", "/" ) ;

			if( path[ 0 ] == '/' )
			{
				path = path.Substring( 1, path.Length - 1 ) ;
			}

			if( path.Length <  1 )
			{
				return path ;
			}

			if( path[ path.Length - 1 ] != '/' )
			{
				path += "/" ;
			}

			return path ;
		}

		// パスの整形を行う
		private static string CorrectFilePath( string path )
		{
			if( string.IsNullOrEmpty( path ) == true )
			{
				return path ;
			}

			path = path.Replace( "\\", "/" ) ;

			if( path[ 0 ] == '/' )
			{
				path = path.Substring( 1, path.Length - 1 ) ;
			}

			return path ;
		}

		// ブーリアン結果を取得する
		private static bool GetBoolean( string value )
		{
			value = value.ToLower() ;

			bool boolean = false ;

			if( value == "false".ToLower() )
			{
				boolean = false ;
			}
			else
			if( value == "true".ToLower() )
			{
				boolean = true ;
			}

			return boolean ;
		}

		// ビルドターゲットを取得する
		private static BuildTarget GetBuildTarget( string value )
		{
			value = value.ToLower() ;

			BuildTarget buildTarget = BuildTarget.Android ;

			if( value == "StandaloneWindows".ToLower() || value == "Windows".ToLower() )
			{
				buildTarget = BuildTarget.StandaloneWindows ;
			}
			if( value == "StandaloneWindows64".ToLower() || value == "Windows64".ToLower() )
			{
				buildTarget = BuildTarget.StandaloneWindows64 ;
			}
			else
			if( value == "StandaloneOSX".ToLower() || value == "OSX".ToLower() )
			{
				buildTarget = BuildTarget.StandaloneOSX ;
			}
			else
			if( value == "Android".ToLower() )
			{
				buildTarget = BuildTarget.Android ;
			}
			else
			if( value == "iOS".ToLower() || value == "iPhone".ToLower() )
			{
				buildTarget = BuildTarget.iOS ;
			}

			return buildTarget ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 他のクラスから呼ぶタイプ
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static bool Build
		(
			string      localAssetsListFilePath,			// 必須
			string      localAssetsRootFolderPath,			// 必須
			string      assetBundleRootFolderPath,			// 必須
			BuildTarget buildTarget,						// 必須
			bool?       chunkBasedCompression   = null,		// 任意
			bool?       forceRebuildAssetBundle = null,		// 任意
			bool?       ignoreTypeTreeChanges   = null,		// 任意
			bool?       disableWriteTypeTree    = null,		// 任意
			bool?       collectDependencies     = null,		// 任意
			bool?       generateCrcFile_Csv     = null,		// 任意
			bool?       generateCrcFile_Json    = null,		// 任意
			bool?       generateLinkFile        = null,		// 任意
			bool?		strictMode				= false,	// 任意
			string		linkFilePath			= null		// 任意
		)
		{
			if
			(
				string.IsNullOrEmpty( localAssetsListFilePath	) == true ||
				string.IsNullOrEmpty( localAssetsRootFolderPath	) == true ||
				string.IsNullOrEmpty( assetBundleRootFolderPath	) == true
			)
			{
				Debug.LogWarning( "Bad Parameter" ) ;
				return false ;
			}

			//-----------------------------------------

			SimpleAssetBundleBuilder sabb = ScriptableObject.CreateInstance<SimpleAssetBundleBuilder>() ;

			//-----------------------------------------

			// パラメータを設定する

			// 必須
			sabb.m_LocalAssetsListFilePath		= CorrectFilePath( localAssetsListFilePath ) ;
			sabb.m_LocalAssetsRootFolderPath	= CorrectFolderPath( localAssetsRootFolderPath ) ;
			sabb.m_AssetBundleRootFolderPath	= CorrectFolderPath( assetBundleRootFolderPath ) ;
			sabb.m_BuildTarget					= buildTarget ;

			// 任意
			if( chunkBasedCompression	!= null ){ sabb.m_ChunkBasedCompression		= chunkBasedCompression.Value	;	}
			if( forceRebuildAssetBundle	!= null ){ sabb.m_ForceRebuildAssetBundle	= forceRebuildAssetBundle.Value	;	}
			if( ignoreTypeTreeChanges	!= null ){ sabb.m_IgnoreTypeTreeChanges		= ignoreTypeTreeChanges.Value	;	}
			if( disableWriteTypeTree	!= null ){ sabb.m_DisableWriteTypeTree		= disableWriteTypeTree.Value	;	}
			if( collectDependencies		!= null ){ sabb.m_CollectDependencies		= collectDependencies.Value		;	}
			if( generateCrcFile_Csv		!= null ){ sabb.m_GenerateCrcFile_Csv		= generateCrcFile_Csv.Value		;	}
			if( generateCrcFile_Json	!= null ){ sabb.m_GenerateCrcFile_Json		= generateCrcFile_Json.Value	;	}
			if( generateLinkFile		!= null ){ sabb.m_GenerateLinkFile			= generateLinkFile.Value		;	}
			if ( strictMode				!= null ) { sabb.m_StrictMode				= strictMode.Value ; }

			sabb.m_LinkFilePath					= CorrectFilePath( linkFilePath ) ;

			//-----------------------------------------

			// アセットバンドルをビルドする
			bool result = sabb.CreateAssetBundleAll() ;

			// 最後にオブジェクトを破棄する
			DestroyImmediate( sabb ) ;

			return result ;
		}

		/// <summary>
		/// 他のクラスから呼ぶ(LinkXmlFile生成)
		/// </summary>
		/// <returns></returns>
		public static bool MakeLinkXmlFile
		(
			( string, string )[]	targets,
			string					linkFilePath
		)
		{
			if
			(
				targets == null || targets.Length == 0 ||
				string.IsNullOrEmpty( linkFilePath				) == true
			)
			{
				Debug.LogWarning( "Bad Parameter" ) ;
				return false ;
			}

			//-----------------------------------------

			SimpleAssetBundleBuilder sabb = ScriptableObject.CreateInstance<SimpleAssetBundleBuilder>() ;

			//-----------------------------------------

			// パラメータを設定する

			List<TargetData> correctTargets = new List<TargetData>() ;
			TargetData target ;

			string listFilePath ;
			string rootFolderPath ;

			int i, l = targets.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				listFilePath	= targets[ i ].Item1 ;
				rootFolderPath	= targets[ i ].Item2 ;

				target = new TargetData()
				{
					ListFilePath	= CorrectFilePath( listFilePath ),
					RootFolderPath	= CorrectFolderPath( rootFolderPath )
				} ;

				correctTargets.Add( target ) ;
			}

			// 出力パス
			string collectLinkFilePath		= CorrectFilePath( linkFilePath ) ;

			//-----------------------------------------

			// LinkXmlFileを生成する
			bool result = sabb.CreateLinkXmlFile( correctTargets.ToArray(), collectLinkFilePath ) ;

			// 最後にオブジェクトを破棄する
			DestroyImmediate( sabb ) ;

			return result ;
		}
	}

	//--------------------------------------------------------------------------------------------

	/// <summary>
	/// link.xml を出力するためのクラス
	/// </summary>
	public class SimpleLinkXMLGenerator
	{
		private readonly Dictionary<Type, Type> m_TypeConversion = new Dictionary<Type, Type>() ;
		private readonly HashSet<Type> m_Types = new HashSet<Type>() ;

		//-------------------------------------------------------------------------------------------

		// 有効扱いする拡張子を絞り込む
		private static readonly List<string> m_ValidExtensions = new List<string>()
		{
			".prefab",			// Prefab
			".asset",			// ScriptableObject

//			".mat",				// Material

//			".spriteatlas",		// SpriteAtlas

			".controller",		// AnimationController
//			".anim",			// AnimationClip

			".playable",		// Playable
		} ;

		/// <summary>
		/// link.xml に登録するアセットを追加する(複数)
		/// </summary>
		/// <param name="assetPaths"></param>
		public void AddAssets( IEnumerable<string> assetPaths )
		{
			if( assetPaths == null )
			{
				Debug.LogException( new ArgumentNullException( nameof( assetPaths ) ) ) ;
				return ;
			}

			//------------------------------------------------------------------

			// 開いていたシーンを保存する
			bool isChangeScene = false ;
			var activeScene = EditorSceneManager.GetActiveScene() ;
			string activeScenepath = activeScene.path ;

			//------------------------------------------------------------------

			// 検出した型を格納する
			List<Type> usedAssetTypes = new List<Type>() ;

			AssetDatabase.StartAssetEditing() ;
			EditorApplication.LockReloadAssemblies() ;
			try
			{
				string log = string.Empty ;
				int assetCountNow = 0 ;
				int assetCountMax = 0 ;
				int sceneCount = 0 ;

				foreach( var assetPath in assetPaths )
				{
					var extension = Path.GetExtension( assetPath ) ;
					if( extension != ".unity" )
					{
						// アセットファイルである
						if( m_ValidExtensions.Contains( extension ) == true )
						{
							var dependencyAssetPaths = AssetDatabase.GetDependencies( assetPath ) ;
							foreach( var dependencyAssetPath in dependencyAssetPaths )
							{
								extension = Path.GetExtension( dependencyAssetPath ) ;
								if( extension != ".unity" && m_ValidExtensions.Contains( extension ) == true )
								{
									// 有効な拡張子のみ検査する
									var assets = AssetDatabase.LoadAllAssetsAtPath( dependencyAssetPath ) ;
									if( assets != null && assets.Length >  0 )
									{
										var assetTypes = assets.Where( _ => _ != null ).Select( _ => _.GetType() ).ToArray() ;
										if( assetTypes != null && assetTypes.Length >  0 )
										{
											usedAssetTypes.AddRange( assetTypes ) ;
										}
									}
									assetCountNow ++ ;
								}
							}
						}
						assetCountMax ++ ;
					}
					else
					{
						// シーンファイルである(※LoadAllAssetsAtPathが使用できないので一度ヒエラルキーに展開する)
						log += "\n"+ assetPath ;
						sceneCount ++ ;

						isChangeScene = true ;	// シーンの変更が行われた
						var scene = EditorSceneManager.OpenScene( assetPath ) ;
						if( scene != null )
						{
							var rootObjects = scene.GetRootGameObjects() ;
							foreach( var rootObject in rootObjects )
							{
								var components = rootObject.GetComponentsInChildren<Component>( true ) ;
								var assetTypes = components.Where( _ => _ != null ).Select( _ => _.GetType() ).ToArray() ;
								if( assetTypes != null && assetTypes.Length >  0 )
								{
									usedAssetTypes.AddRange( assetTypes ) ;
								}
								else
								{
									Debug.LogWarning( $"Not found types : {assetPath}" ) ;
								}
							}
						}
						else
						{
							Debug.LogWarning( $"Could not open scene : {assetPath}" ) ;
						}
					}

					// 型の追加
					AddTypes( usedAssetTypes ) ;
					usedAssetTypes.Clear() ;

					//--------------------------------------------------------
				}

				Debug.Log( "<color=#00BFFF>Search Asset : " + assetCountNow + " / " + assetCountMax + "</color>" ) ;

				if( sceneCount >  0 )
				{
					log = "<color=#00BF00>Search Scene : " + sceneCount + "</color>" + log ;
					Debug.Log( log ) ;
				}
			}
			catch( Exception e )
			{
				Debug.LogException( e ) ;
			}
			finally
			{
				AssetDatabase.StopAssetEditing() ;
				EditorApplication.UnlockReloadAssemblies() ;
				AssetDatabase.Refresh( ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceSynchronousImport ) ;
				AssetDatabase.ReleaseCachedFileHandles() ;
			}

			//----------------------------------------------------------

			if( isChangeScene == true && string.IsNullOrEmpty( activeScenepath ) == false )
			{
				// 変更されたシーンを元に戻す
				activeScene = EditorSceneManager.GetActiveScene() ;
				if( activeScene.path != activeScenepath )
				{
					Debug.Log( "Reload Scene : " + activeScenepath ) ;
					EditorSceneManager.OpenScene( activeScenepath ) ;
				}
			}
		}

		/// <summary>
		/// link.xmlに登録する型を追加する
		/// </summary>
		/// <param name="types"></param>
		public void AddTypes( IEnumerable<Type> types )
		{
			if( types == null )
			{
				Debug.LogException( new ArgumentNullException( nameof( types ) ) ) ;
				return ;
			}

			foreach( var type in types )
			{
				AddTypeInternal( type ) ;
			}
		}

		/// <summary>
		/// link.xmlファイルを保存する
		/// </summary>
		/// <param name="savePath"></param>
		public void Save( string savePath )
		{
			if( string.IsNullOrWhiteSpace( savePath ) == true )
			{
				Debug.LogException( new ArgumentNullException( nameof( savePath ) ) ) ;
				return ;
			}

			var assemblyMap = GetTypeGroupByAssembly() ;

			var xmlDocument = new XmlDocument() ;
			xmlDocument.AppendChild( xmlDocument.CreateComment( CreateDocumentHeader() ) ) ;

			var linker = xmlDocument.AppendChild( xmlDocument.CreateElement( "linker" ) ) ;
			foreach( var keyValuePair in assemblyMap )
			{
				if( keyValuePair.Key.FullName.Contains( "UnityEditor" ) == true )
				{
					continue ;
				}

				var assembly = linker.AppendChild( xmlDocument.CreateElement( "assembly" ) ) ;
				var attribute = xmlDocument.CreateAttribute( "fullname" ) ;
				attribute.Value = keyValuePair.Key.GetName().Name ;
				if( assembly.Attributes == null )
				{
					continue ;
				}

				assembly.Attributes.Append( attribute ) ;

				foreach( var t in keyValuePair.Value )
				{
					var typeElement = assembly.AppendChild( xmlDocument.CreateElement( "type" ) ) ;
					var fullNameAttribute = xmlDocument.CreateAttribute( "fullname" ) ;
					fullNameAttribute.Value = t.FullName ;
					if( typeElement.Attributes != null )
					{
						typeElement.Attributes.Append( fullNameAttribute ) ;
						var preserveAttribute = xmlDocument.CreateAttribute( "preserve" ) ;
						preserveAttribute.Value = "all" ;
						typeElement.Attributes.Append( preserveAttribute ) ;
					}
				}
			}

			xmlDocument.Save( savePath ) ;
			AssetDatabase.ImportAsset
			(
				path: savePath,
				options: ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceSynchronousImport
			) ;
		}

		//---------------------------------------------------------------------------

		private void AddTypeInternal( Type type )
		{
			if( type == null )
			{
				return ;
			}

			m_Types.Add( m_TypeConversion.TryGetValue( key: type, value: out var convertedType ) ? convertedType : type ) ;
		}

		/// <summary>
		/// <seealso cref="Types" /> を <seealso cref="Assembly" />毎に分けます
		/// </summary>
		/// <returns></returns>
		private Dictionary<Assembly, IEnumerable<Type>> GetTypeGroupByAssembly()
		{
			return m_Types
				.GroupBy( type => type.Assembly )
				.ToDictionary( keySelector: value => value.Key, elementSelector: value => value.Distinct() ) ;
		}

		/// <summary>
		/// ドキュメントヘッダー的なコメントの作成
		/// </summary>
		/// <returns></returns>
		private string CreateDocumentHeader()
		{
			var stringBuilder = new StringBuilder() ;
			return stringBuilder
				.AppendLine()
				.AppendLine( "Preserve types and members in an assembly" )
				.AppendLine( "https://docs.unity3d.com/Manual/ManagedCodeStripping.html" )
				.AppendLine()
				.AppendLine( "this file is auto generated." )
				.ToString() ;
		}
	}
}

