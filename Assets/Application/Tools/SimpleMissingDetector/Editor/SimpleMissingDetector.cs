using System ;
using System.IO ;
using System.Collections.Generic ;
using System.Linq ;
using UnityEngine ;
using UnityEditor ;


/// <summary>
/// シンプルミッシングディテクター
/// </summary>
namespace Tools.ForAssets
{
	/// <summary>
	/// ミッシングディテクタークラス(エディター用) Version 2021/09/10
	/// </summary>

	public class SimpleMissingDetector : EditorWindow
	{
		[ MenuItem( "Tools/Simple Missing Detector(ミッシング検出)" ) ]
		internal static void OpenWindow()
		{
			var window = EditorWindow.GetWindow<SimpleMissingDetector>( false, "Missing Detector", true ) ;
			window.minSize = new Vector2( 960, 640 ) ;
		}

		//-----------------------------------

		private int			m_TabIndex = 0 ;
		private readonly string[]	m_TabLabels = new string[]{ "Project", "Hierarchy" } ;

		//---------------------------------------------------------------------------------------------------------------------------

		// 選択中のファイルが変更された際に呼び出される
		internal void OnSelectionChange()
		{
			Repaint() ;
		}

		// 描画
		internal void OnGUI()
		{
			// タブでモードを選択する
			using( new EditorGUILayout.HorizontalScope( EditorStyles.toolbar ) )
			{
				m_TabIndex = GUILayout.Toolbar( m_TabIndex, m_TabLabels, new GUIStyle( EditorStyles.toolbarButton ), GUI.ToolbarButtonSize.FitToContents ) ;
			}

			//----------------------------------------------------------

			if( m_TabIndex == 0 )
			{
				// Project
				DrawProjectTab() ;
			}
			else
			if( m_TabIndex == 1 )
			{
				// Hierarchy
				DrawHierarchyTab() ;
			}
		}

		//---------------------------------------------------------------------------------------------------------------------------
		// Project

		private string			m_Project_RootAssetPath ;
		private List<string>	m_Project_TargetAssetPaths ;

		private bool			m_Project_FF_Prefab		= true ;
		private bool			m_Project_FF_Asset		= true ;
		private bool			m_Project_FF_Controller	= true ;

		private bool			m_Project_FF_Material	= true ;
		private bool			m_Project_FF_Shader		= true ;
		private bool			m_Project_FF_Mask		= true ;

		private bool			m_Project_VisiblePropertyOnly = true ;

		private bool			m_Project_PM_Mesh		= false ;
		private bool			m_Project_PM_Material	= false ;
		private bool			m_Project_PM_Shader		= false ;
		private bool			m_Project_PM_Texture	= false ;
		private bool			m_Project_PM_Sprite		= false ;
		private bool			m_Project_PM_Motion		= false ;

		// Missing 情報格納データクラス
		[Serializable]	// リコンパイルしてもリストを消さないために必要
		public class ProjectMissingData
		{
			public string		PropertyName ;
			public string		PropertyType ;
			public string		PropertyPath ;

			public UnityEngine.Object ComponentEntity ;
			public string		ComponentType ;

			public string		GameObjectPath ;

			public string		AssetType ;
			public string		AssetPath ;

			public int			AlertLevel ;
		}

		private List<ProjectMissingData>	m_ProjectMissings ;

		private Vector2			m_Project_ScrollPosition ;

		private int				m_Project_Index = -1 ;
		private string			m_Project_PropertyType		= string.Empty ;
		private string			m_Project_PropertyPath		= string.Empty ;
		private string			m_Project_ComponentType		= string.Empty ;
		private string			m_Project_GameObjectPath	= string.Empty ;
		private string			m_Project_AssetType			= string.Empty ;

		private int				m_Project_AssetPath_Index	= -1 ;
		private string			m_Project_AssetPath			= string.Empty ;

		private bool			m_Project_AllTargetSearch = true ;

		//-----------------------------------------------------------

		// Project タブを表示する
		private void DrawProjectTab()
		{
			bool isRefresh = false ;

			GUILayout.Space( 6f ) ;

			EditorGUILayout.HelpBox( "[Missing] を検査する Project 内の Root AssetPath を設定します", MessageType.Info ) ;
			GUILayout.BeginHorizontal() ;
			{
				string rootAssetPath = m_Project_RootAssetPath ;

				// 保存パスを選択する
				GUI.backgroundColor = new Color( 0, 1, 1, 1 ) ;
				if( GUILayout.Button( "Root AssetPath", GUILayout.Width( 120f ) ) == true )
				{
					if( Selection.objects != null && Selection.objects.Length == 1 && Selection.activeObject != null )
					{
						// １つだけ選択（複数選択には対応していない：フォルダかファイル）
						string path = AssetDatabase.GetAssetPath( Selection.activeObject.GetInstanceID() ) ;
						if( File.Exists( path ) == true )
						{
							// ファイルを指定
							rootAssetPath = path ;
						}
						else
						if( Directory.Exists( path ) == true )
						{
							// フォルダを指定
							rootAssetPath = path ;
						}
						else
						{
							rootAssetPath = string.Empty ;
						}
					}

					// 複数選択している場合は何もしない

					if( m_Project_RootAssetPath != rootAssetPath || m_Project_AllTargetSearch == true )
					{
						// 対象のルートパスを更新する
						m_Project_RootAssetPath = rootAssetPath ;

						// 検査対象のパス群を取得する
						m_Project_TargetAssetPaths = GetTargetAssetPaths( m_Project_RootAssetPath ) ;

						isRefresh = true ;
						m_Project_AllTargetSearch = false ;
					}
				}
				GUI.backgroundColor = Color.white ;

				//---------------------------------------------------------

				// ルートフォルダ
				EditorGUILayout.TextField( m_Project_RootAssetPath ) ;

				//---------------------------------------------------------

				// 対象のパスを消去する(全 Asset 対象)
				if( GUILayout.Button( "Clear", GUILayout.Width( 100f ) ) == true )
				{
					// 選択中のアセットを無しにする
					if( string.IsNullOrEmpty( m_Project_RootAssetPath ) == false )
					{
						m_Project_AllTargetSearch = true ;	// 全対象検査を有効にする
					}

					Selection.activeObject = null ;
					m_Project_RootAssetPath = string.Empty ;
				}
				GUI.backgroundColor = Color.white ;
			}
			GUILayout.EndHorizontal() ;

			//----------------------------------------------------------
			// ターゲットフィルタ

			GUI.contentColor = Color.yellow ;
			GUILayout.Label( "Asset Filtering", GUILayout.Width( 120f ) ) ;
			GUI.contentColor = Color.white;
			GUILayout.BeginHorizontal() ;	// 横並び開始
			{
				GUILayout.Label( " ", GUILayout.Width( 20f ) ) ;

				bool ff_Prefab = EditorGUILayout.Toggle( m_Project_FF_Prefab, GUILayout.Width( 10f ) ) ;
				if( ff_Prefab != m_Project_FF_Prefab )
				{
					m_Project_FF_Prefab = ff_Prefab ;
				}
				GUILayout.Label( ".prefab", GUILayout.Width( 80f ) ) ;

				GUILayout.Label( " " ) ;

				bool ff_Asset = EditorGUILayout.Toggle( m_Project_FF_Asset, GUILayout.Width( 10f ) ) ;
				if( ff_Asset != m_Project_FF_Asset )
				{
					m_Project_FF_Asset = ff_Asset ;
				}
				GUILayout.Label( ".asset", GUILayout.Width( 80f ) ) ;

				GUILayout.Label( " " ) ;

				bool ff_Controller = EditorGUILayout.Toggle( m_Project_FF_Controller, GUILayout.Width( 10f ) ) ;
				if( ff_Controller != m_Project_FF_Controller )
				{
					m_Project_FF_Controller = ff_Controller ;
				}
				GUILayout.Label( ".controller", GUILayout.Width( 80f ) ) ;

				GUILayout.Label( " " ) ;

				bool ff_Material = EditorGUILayout.Toggle( m_Project_FF_Material, GUILayout.Width( 10f ) ) ;
				if( ff_Material != m_Project_FF_Material )
				{
					m_Project_FF_Material = ff_Material ;
				}
				GUILayout.Label( ".mat", GUILayout.Width( 80f ) ) ;

				GUILayout.Label( " " ) ;

				bool ff_Shader = EditorGUILayout.Toggle( m_Project_FF_Shader, GUILayout.Width( 10f ) ) ;
				if( ff_Shader != m_Project_FF_Shader )
				{
					m_Project_FF_Shader = ff_Shader ;
				}
				GUILayout.Label( ".shader", GUILayout.Width( 80f ) ) ;

				GUILayout.Label( " " ) ;

				bool ff_Mask = EditorGUILayout.Toggle( m_Project_FF_Mask, GUILayout.Width( 10f ) ) ;
				if( ff_Mask != m_Project_FF_Mask )
				{
					m_Project_FF_Mask = ff_Mask ;
				}
				GUILayout.Label( ".mask", GUILayout.Width( 80f ) ) ;

				GUILayout.Label( " " ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			//----------------------------------------------------------

			GUI.contentColor = Color.yellow ;
			GUILayout.Label( "Property Masking", GUILayout.Width( 120f ) ) ;
			GUI.contentColor = Color.white ;

			GUILayout.BeginHorizontal() ;	// 横並び開始
			{
				GUILayout.Label( " ", GUILayout.Width( 20f ) ) ;
				bool visiblePropertyOnly = EditorGUILayout.Toggle( m_Project_VisiblePropertyOnly, GUILayout.Width( 10f ) ) ;
				if( visiblePropertyOnly != m_Project_VisiblePropertyOnly )
				{
					m_Project_VisiblePropertyOnly = visiblePropertyOnly ;
				}
				GUILayout.Label( "Visible Property Only", GUILayout.Width( 200f ) ) ;

			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUILayout.BeginHorizontal() ;	// 横並び開始
			{
				GUILayout.Label( " ", GUILayout.Width( 20f ) ) ;
				bool pm_Mesh = EditorGUILayout.Toggle( m_Project_PM_Mesh, GUILayout.Width( 10f ) ) ;
				if( pm_Mesh != m_Project_PM_Mesh )
				{
					m_Project_PM_Mesh = pm_Mesh ;
				}
				GUILayout.Label( "Mesh", GUILayout.Width( 120f ) ) ;

				GUILayout.Label( " " ) ;
				bool pm_Material = EditorGUILayout.Toggle( m_Project_PM_Material, GUILayout.Width( 10f ) ) ;
				if( pm_Material != m_Project_PM_Material )
				{
					m_Project_PM_Material = pm_Material ;
				}
				GUILayout.Label( "Material", GUILayout.Width( 120f ) ) ;

				GUILayout.Label( " " ) ;
				bool pm_Shader = EditorGUILayout.Toggle( m_Project_PM_Shader, GUILayout.Width( 10f ) ) ;
				if( pm_Shader != m_Project_PM_Shader )
				{
					m_Project_PM_Shader = pm_Shader ;
				}
				GUILayout.Label( "Shader", GUILayout.Width( 120f ) ) ;

				GUILayout.Label( " " ) ;
				bool pm_Texture = EditorGUILayout.Toggle( m_Project_PM_Texture, GUILayout.Width( 10f ) ) ;
				if( pm_Texture != m_Project_PM_Texture )
				{
					m_Project_PM_Texture = pm_Texture ;
				}
				GUILayout.Label( "Texture", GUILayout.Width( 120f ) ) ;

				GUILayout.Label( " " ) ;
				bool pm_Sprite = EditorGUILayout.Toggle( m_Project_PM_Sprite, GUILayout.Width( 10f ) ) ;
				if( pm_Sprite != m_Project_PM_Sprite )
				{
					m_Project_PM_Sprite = pm_Sprite ;
				}
				GUILayout.Label( "Sprite", GUILayout.Width( 120f ) ) ;

				GUILayout.Label( " " ) ;
				bool pm_Motion = EditorGUILayout.Toggle( m_Project_PM_Motion, GUILayout.Width( 10f ) ) ;
				if( pm_Motion != m_Project_PM_Motion )
				{
					m_Project_PM_Motion = pm_Motion ;
				}
				GUILayout.Label( "Motion", GUILayout.Width( 120f ) ) ;

				GUILayout.Label( " " ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			//------------------------------------------------------------------

			GUILayout.Space( 6f ) ;

			if( m_Project_TargetAssetPaths != null && m_Project_TargetAssetPaths.Count >  0 )
			{
				GUI.backgroundColor = new Color( 1, 0, 1, 1 ) ;
				if( GUILayout.Button( "Refresh" ) == true )
				{
					isRefresh = true ;
				}
				GUI.backgroundColor = Color.white ;

				// 対象が存在する
				EditorGUILayout.BeginHorizontal() ;
				GUI.contentColor = Color.cyan ;
				GUILayout.Label( "Target Asset : " + m_Project_TargetAssetPaths.Count, GUILayout.Width( 120f ) ) ;
				GUI.contentColor = Color.white ;
				if( m_ProjectMissings != null && m_ProjectMissings.Count >  0 )
				{
					GUILayout.Label( " ", GUILayout.Width( 10f ) ) ;
					GUI.contentColor = new Color32( 255, 127,   0, 255 ) ;
					GUILayout.Label( "Detected Missing : " + m_ProjectMissings.Count, GUILayout.Width( 160f ) ) ;
					GUI.contentColor = Color.white ;
				}
				EditorGUILayout.EndHorizontal() ;
			}
			else
			{
				GUILayout.Label( "Not found search target Assets.", GUILayout.Width( 400f ) ) ;
			}

			//------------------------------------------------------------------------------------------

			if( isRefresh == true )
			{
				SearchProjectMissingAll() ;

				m_Project_Index				= -1 ;
				m_Project_PropertyType		= string.Empty ;
				m_Project_PropertyPath		= string.Empty ;
				m_Project_GameObjectPath	= string.Empty ;
				m_Project_AssetType			= string.Empty ;

				m_Project_AssetPath_Index	= -1 ;
				m_Project_AssetPath			= string.Empty ;
			}

			//------------------------------------------------------------------------------------------
			// Missing 一覧を表示する

			if( m_Project_TargetAssetPaths != null && m_Project_TargetAssetPaths.Count >  0 )
			{
				if( m_ProjectMissings != null && m_ProjectMissings.Count >  0 )
				{
					string activeAssetPath = string.Empty ;
					if( Selection.objects != null && Selection.objects.Length == 1 && Selection.activeObject != null )
					{
						// １つだけ選択（複数選択には対応していない：フォルダかファイル）
						activeAssetPath = AssetDatabase.GetAssetPath( Selection.activeObject.GetInstanceID() ) ;
					}

					// 列見出し
					EditorGUILayout.BeginHorizontal() ;
					EditorGUILayout.LabelField( "Component", GUILayout.Width( 300 ) ) ;
					EditorGUILayout.LabelField( "Property", GUILayout.Width( 150 ) ) ;
					EditorGUILayout.LabelField( "Link", GUILayout.Width( 25 ) ) ;
					EditorGUILayout.LabelField( "AssetPath" ) ;
					EditorGUILayout.EndHorizontal() ;

					// リスト表示
					m_Project_ScrollPosition = EditorGUILayout.BeginScrollView( m_Project_ScrollPosition ) ;

					int i, l = m_ProjectMissings.Count ;
					ProjectMissingData missing ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						missing = m_ProjectMissings[ i ] ;

						EditorGUILayout.BeginHorizontal() ;

						// Component 情報
						UnityEngine.Object component = missing.ComponentEntity ;
						if( component != null )
						{
							EditorGUILayout.ObjectField( component, component.GetType(), true, GUILayout.Width( 300 ) ) ;
						}
						else
						{
							GUI.contentColor = new Color32( 255,  63,   0, 255 ) ;
							EditorGUILayout.TextField( "Missing", GUILayout.Width( 300 ) ) ;
							GUI.contentColor = Color.white ;
						}

						// Property 情報
						if( m_Project_Index >= 0 && i == m_Project_Index )
						{
							GUI.backgroundColor = new Color32(   0, 255, 255, 255 ) ;
						}
						else
						{
							GUI.backgroundColor = Color.white ;
						}

						// アラートレベルによって文字の色を変える
						if( missing.AlertLevel == 2 )
						{
							GUI.contentColor = new Color32( 255,  63,  63, 255 ) ;	// GameObject Missing
						}
						else
						if( missing.AlertLevel == 1 )
						{
							GUI.contentColor = new Color32( 255, 255,   0, 255 ) ;	// Component Missing
						}
						else
						{
							GUI.contentColor = Color.white ;						// Property Missing
						}

						if( GUILayout.Button( missing.PropertyName, GUILayout.Width( 150 ) ) == true )
						{
							m_Project_Index				= i ;
							m_Project_PropertyType		= missing.PropertyType ;
							m_Project_PropertyPath		= missing.PropertyPath ;
							m_Project_ComponentType		= missing.ComponentType ;
							m_Project_GameObjectPath	= missing.GameObjectPath ;
							m_Project_AssetType			= missing.AssetType ;
						}

						GUI.contentColor = Color.white ;
						GUI.backgroundColor = Color.white ;

						// AssetPath 情報
						if( i == m_Project_AssetPath_Index )
						{
							GUI.backgroundColor = new Color32( 255, 127, 255, 255 ) ;
						}
						else
						{
							GUI.backgroundColor = Color.white ;
						}

						string assetPath = missing.AssetPath ;
						if( GUILayout.Button( ">", GUILayout.Width( 25 ) ) == true )
						{
							UnityEngine.Object asset = AssetDatabase.LoadMainAssetAtPath( assetPath ) ;
							if( asset != null )
							{
								Selection.activeObject = asset ;
							}

							m_Project_AssetPath_Index	= i ;
							m_Project_AssetPath			= assetPath ;
						}

						GUI.backgroundColor = Color.white ;

						if( i == m_Project_AssetPath_Index && string.IsNullOrEmpty( m_Project_AssetPath ) == false && m_Project_AssetPath == assetPath && activeAssetPath == assetPath )
						{
							GUI.contentColor = new Color32( 255, 127, 255, 255 ) ;
						}
						else
						{
							GUI.contentColor = Color.white ;
						}

						EditorGUILayout.TextField( assetPath ) ;

						GUI.contentColor = Color.white ;

						EditorGUILayout.EndHorizontal() ;
					}
					EditorGUILayout.EndScrollView() ;

					//--------------------------------

					if( m_Project_Index >= 0 )
					{
						// 詳細情報を表示する
						EditorGUILayout.BeginHorizontal() ;

						GUILayout.Label( "ComponentType", GUILayout.Width( 100f ) ) ;
						GUI.contentColor = new Color32( 255, 255,   0, 255 ) ;
						EditorGUILayout.TextField( m_Project_ComponentType ) ;
						GUI.contentColor = Color.white ;

						GUILayout.Label( "GameObjectPath", GUILayout.Width( 105f ) ) ;
						GUI.contentColor = new Color32( 255, 255,   0, 255 ) ;
						EditorGUILayout.TextField( m_Project_GameObjectPath ) ;
						GUI.contentColor = Color.white ;

						GUILayout.Label( "AssetType", GUILayout.Width( 96f ) ) ;
						GUI.contentColor = new Color32( 255, 255,   0, 255 ) ;
						EditorGUILayout.TextField( m_Project_AssetType, GUILayout.Width( 200f ) ) ;
						GUI.contentColor = Color.white ;

						EditorGUILayout.EndHorizontal() ;


						EditorGUILayout.BeginHorizontal() ;

						GUILayout.Label( "PropertyPath", GUILayout.Width( 80f ) ) ;
						GUI.contentColor = new Color32( 255, 255,   0, 255 ) ;
						EditorGUILayout.TextField( m_Project_PropertyPath ) ;
						GUI.contentColor = Color.white ;

						GUILayout.Label( "PropertyType", GUILayout.Width( 96f ) ) ;
						GUI.contentColor = new Color32( 255, 255,   0, 255 ) ;
						EditorGUILayout.TextField( m_Project_PropertyType, GUILayout.Width( 200f ) ) ;
						GUI.contentColor = Color.white ;

						EditorGUILayout.EndHorizontal() ;
					}
				}
				else
				{
					GUILayout.Label( "Not found missing !!" ) ;
				}
			}
		}

		//---------------------------------------------------------------------------

		// 検査対象となる AssetPath 群を取得する
		private List<string> GetTargetAssetPaths( string currentPath )
		{
			// パスを生成する
			List<string> targetAssetPaths = new List<string>() ;

			// フィルターを設定する
			List<string> filter = new List<string>() ;
			if( m_Project_FF_Prefab		== true ){ filter.Add( ".prefab"		) ;	}
			if( m_Project_FF_Asset		== true ){ filter.Add( ".asset"			) ;	}
			if( m_Project_FF_Controller	== true ){ filter.Add( ".controller"	) ;	}
			if( m_Project_FF_Material	== true ){ filter.Add( ".mat"			) ;	}
			if( m_Project_FF_Shader		== true ){ filter.Add( ".shader"		) ;	}
			if( m_Project_FF_Mask		== true ){ filter.Add( ".mask"			) ;	}

			GetTargetAssetPaths( currentPath, ref targetAssetPaths, ref filter ) ;

			return targetAssetPaths ;
		}

		private void GetTargetAssetPaths( string currentPath, ref List<string> targetAssetPaths, ref List<string> filter )
		{
			// パスを生成する

			if( string.IsNullOrEmpty( currentPath ) == true )
			{
				// 全対象
				string[] allPaths = AssetDatabase.GetAllAssetPaths() ;
				if( allPaths != null && allPaths.Length >  0 )
				{
					int i, l = allPaths.Length ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						// プログレスバーを表示
						EditorUtility.DisplayProgressBar( "All target searching ...", string.Format( "{0}/{1}", i + 1, l ), ( float )i / l ) ;

						if( filter.Contains( Path.GetExtension( allPaths[ i ] ) ) == true )
						{
							// 有効なパス
							targetAssetPaths.Add( allPaths[ i ].Replace( "\\", "/" ) ) ;
						}
					}

					// プログレスバーを消す
					EditorUtility.ClearProgressBar() ;
				}

				return ;
			}

			if( File.Exists( currentPath ) == true )
			{
				// 対象はファイル
				if( filter.Contains( Path.GetExtension( currentPath ) ) == true )
				{
					// 有効なパス
					targetAssetPaths.Add( currentPath.Replace( "\\", "/" ) ) ;
				}
				return ;
			}

			if( Directory.Exists( currentPath ) == false )
			{
				// 不可
				return ;
			}

			string[] childPaths ;

			childPaths = Directory.GetFiles( currentPath ) ;
			if( childPaths != null && childPaths.Length >  0 )
			{
				// サブファイルが存在する
				string childPath ;
				int i, l = childPaths.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					childPath = childPaths[ i ] ;
					if( filter.Contains( Path.GetExtension( childPath ) ) == true )
					{
						// 有効なパス
						targetAssetPaths.Add( childPath.Replace( "\\", "/" ) ) ;
					}
				}
			}

			childPaths = Directory.GetDirectories( currentPath ) ;
			if( childPaths != null && childPaths.Length >  0 )
			{
				// サブフォルダが存在する
				string childPath ;
				int i, l = childPaths.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					childPath = childPaths[ i ] ;
					GetTargetAssetPaths( childPath, ref targetAssetPaths, ref filter ) ;
				}
			}

			return ;
		}

		//---------------------------------------------------------------------------

		// 対象全ての Asset で Missing を検出する
		private void SearchProjectMissingAll()
		{
			try
			{
				EditorApplication.LockReloadAssemblies() ;
				AssetDatabase.StartAssetEditing() ;

				List<string> propertyMasks = new List<string>() ;
				if ( m_Project_PM_Mesh == true )
				{
					propertyMasks.Add( "PPtr<Mesh>" ) ;
				}

				if ( m_Project_PM_Material == true )
				{
					propertyMasks.Add( "PPtr<$Material>" ) ;
				}

				if ( m_Project_PM_Shader == true )
				{
					propertyMasks.Add( "PPtr<Shader>" ) ;
				}

				if ( m_Project_PM_Texture == true )
				{
					propertyMasks.Add( "PPtr<Texture>" ) ;
				}

				if ( m_Project_PM_Sprite == true )
				{
					propertyMasks.Add( "PPtr<$Sprite>" ) ;
				}

				if ( m_Project_PM_Motion == true )
				{
					propertyMasks.Add( "PPtr<Motion>" ) ;
				}

				//----------------------------------

				if ( m_ProjectMissings == null )
				{
					m_ProjectMissings = new List<ProjectMissingData>() ;
				}
				else
				{
					m_ProjectMissings.Clear() ;
				}

				int i, l = m_Project_TargetAssetPaths.Count ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					// プログレスバーを表示
					EditorUtility.DisplayProgressBar
					(
						"missing searching ...",
						string.Format( "{0}/{1}", i + 1, l ),
						( float )( i + 1 ) / ( float )l
					) ;

					SearchProjectMissing( m_Project_TargetAssetPaths[ i ], ref propertyMasks ) ;
				}
			}
			catch( Exception e )
			{
				Debug.LogWarning( "{Project Seraching] " + e.Message ) ;
			}
			finally
			{
				// プログレスバーを消す
				EditorUtility.ClearProgressBar() ;

				AssetDatabase.StopAssetEditing() ;
				EditorApplication.UnlockReloadAssemblies() ;
			}
		}

		// 指定したパスの Asset で Missing を検出する
		private void SearchProjectMissing( string assetPath, ref List<string> propertyMasks )
		{
			// メインアセットのみ取得
			UnityEngine.Object mainAsset = AssetDatabase.LoadMainAssetAtPath( assetPath ) ;
			string assetType ;
			if( mainAsset != null )
			{
				assetType = mainAsset.GetType().ToString() ;
			}
			else
			{
				assetType = "Unknown" ;
			}

			// プレハブかそうでないかで処理を分ける
			if( mainAsset != null && mainAsset is GameObject )
			{
				// プレハブである
				GameObject go = mainAsset as GameObject ;
				SearchProjectMissingInGameObject( go.transform, assetType, assetPath, ref propertyMasks ) ;
			}
			else
			{
				// プレハブではない

				UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath( assetPath ) ;
				foreach( UnityEngine.Object asset in assets )
				{
					if( asset == null )
					{
						// 参照がロストしている
						continue ;
					}

					if( asset.name == "Deprecated EditorExtensionImpl" )
					{
						// 非推奨(本当は検出に追加した方が良いのだろうが)
						continue ;
					}

					// マテリアルなどはコンポーネントではない
					//--------------------------------

					// プロパティでミッシングを起こしているものを追加する
					AddProjectMissingOfProperty( asset, string.Empty, assetType, assetPath, ref propertyMasks ) ;
				}
			}
		}

		// プレハブ内の Missing を検出する
		private void SearchProjectMissingInGameObject( Transform t, string assetType, string assetPath, ref List<string> propertyMasks )
		{
			// プレハブ内でのパスを取得する
			string gameObjectPath = GetGameObjectPath( t ) ;

			// GameObject 自体が Missing になっているか検査する
		    var status		= PrefabUtility.GetPrefabInstanceStatus( t.gameObject ) ;
			var isMissing	= ( status == PrefabInstanceStatus.MissingAsset ) ;

			if( isMissing == true )
			{
				// GameObject が Missing の場合は子の検査は行わない(意味が無い)
				m_ProjectMissings.Add( new ProjectMissingData()
				{
					PropertyName		= "GameObject is Missing",
					PropertyPath		= string.Empty,
					PropertyType		= string.Empty,
					ComponentEntity		= t,
					ComponentType		= "Unknown",
					GameObjectPath		= gameObjectPath,
					AssetType			= assetType,
					AssetPath			= assetPath,
					AlertLevel			= 2
				} ) ;
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
						// 参照がロストしている(一番いけないやつ)

						// Component が Missing 状態をプロパティリストに追加する
						m_ProjectMissings.Add( new ProjectMissingData()
						{
							PropertyName	= "Component is missing",
							PropertyPath	= string.Empty,
							PropertyType	= string.Empty,
							ComponentEntity	= t,
							ComponentType	= "Unknown",
							GameObjectPath	= gameObjectPath,
							AssetType		= assetType,
							AssetPath		= assetPath,
							AlertLevel		= 1
						} ) ;

						continue ;
					}

					if( component.name == "Deprecated EditorExtensionImpl" )
					{
						// 無効になったもの(検査の必要無し)
						continue ;
					}

					// プロパティでミッシングを起こしているものを追加する
					AddProjectMissingOfProperty( component, gameObjectPath, assetType, assetPath, ref propertyMasks ) ;
				}
			}

			//----------------------------------------------------------
			// 子の処理を行う

			if( t.childCount >  0 )
			{
				int i, l = t.childCount ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					SearchProjectMissingInGameObject( t.GetChild( i ), assetType, assetPath, ref propertyMasks ) ;
				}
			}
		}

		// プロパティでミッシングを起こしているものを追加する
		private void AddProjectMissingOfProperty( UnityEngine.Object component, string gameObjectPath, string assetType, string assetPath, ref List<string> propertyMasks )
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
						( property.objectReferenceValue				== null										) &&
						( property.objectReferenceInstanceIDValue	!= 0										)
					)
					{
						if( propertyMasks.Contains( property.type ) == false )
						{
							// Property が Missing 状態をプロパティリストに追加する
							m_ProjectMissings.Add
							(
								new ProjectMissingData()
								{
									PropertyName = property.displayName + " (" + property.name + ")",
									PropertyPath = property.propertyPath,
									PropertyType = property.type,
									ComponentEntity = component,
									ComponentType = component.GetType().ToString(),
									GameObjectPath = gameObjectPath,
									AssetType = assetType,
									AssetPath = assetPath,
									AlertLevel = 0,
								}
							) ;
						}
					}

					if( m_Project_VisiblePropertyOnly == true )
					{
						// 非表示プロパティは無視する
						if( property.NextVisible( true ) == false )
						{
							break ;
						}
					}
					else
					{
						// 非表示プロパティも表示する
						if( property.Next( true ) == false )
						{
							break ;
						}
					}
				}

				so.Dispose() ;
			}
		}

		//---------------------------------------------------------------------------------------------------------------------------

		private GameObject				m_Hierarchy_RootGameObject ;
		private List<GameObject>		m_Hierarchy_TargetGameObjects ;

		private bool					m_Hierarchy_VisiblePropertyOnly = true ;

		private bool					m_Hierarchy_PM_Mesh		= false ;
		private bool					m_Hierarchy_PM_Material	= false ;
		private bool					m_Hierarchy_PM_Shader	= false ;
		private bool					m_Hierarchy_PM_Texture	= false ;
		private bool					m_Hierarchy_PM_Sprite	= false ;
		private bool					m_Hierarchy_PM_Motion	= false ;

		[Serializable]	// リコンパイルしてもリストを消さないために必要
		public class HierarchyMissingData
		{
			public string				PropertyName ;
			public string				PropertyType ;
			public string				PropertyPath ;

			public UnityEngine.Object	ComponentEntity ;
			public string				ComponentType ;

			public GameObject			GameObjectEntity ;
			public string				GameObjectPath ;

			public int					AlertLevel ;
		}

		private List<HierarchyMissingData>	m_HierarchyMissings ;

		private Vector2					m_Hierarchy_ScrollPosition ;

		private int						m_Hierarchy_Index = -1 ;
		private string					m_Hierarchy_PropertyType		= string.Empty ;
		private string					m_Hierarchy_PropertyPath		= string.Empty ;
		private string					m_Hierarchy_ComponentType		= string.Empty ;

		private int						m_Hierarchy_GameObject_Index	= -1 ;
		private GameObject				m_Hierarchy_GameObject			= null ;

//		private bool					m_Hierarchy_AllTargetSearch = true ;

		//-----------------------------------------------------------

		// Hierarchy タブを表示する
		private void DrawHierarchyTab()
		{
			bool isRefresh = false ;

			//----------------------------------------------------------

			if( m_Hierarchy_RootGameObject == null )
			{
				// ロストした
				m_Hierarchy_TargetGameObjects = null ;
				m_HierarchyMissings = null ;
			}

			//----------------------------------------------------------

			GUILayout.Space( 6f ) ;

			EditorGUILayout.HelpBox( "[Missing] を検査する Hierarchy 上の Root GameObject を選択します", MessageType.Info ) ;
			GUILayout.BeginHorizontal() ;
			{
				GameObject rootGameObject = m_Hierarchy_RootGameObject ;

				// 保存パスを選択する
				GUI.backgroundColor = new Color( 0, 1, 1, 1 ) ;
				if( GUILayout.Button( "Root GameObject", GUILayout.Width( 120f ) ) == true )
				{
					if( Selection.gameObjects != null && Selection.gameObjects.Length == 1 )
					{
						rootGameObject = Selection.gameObjects[ 0 ] ;
					}

					// 複数選択している場合は何もしない

//					if( m_Hierarchy_RootGameObject != rootGameObject || m_Hierarchy_AllTargetSearch == true )
					if( m_Hierarchy_RootGameObject != rootGameObject )
					{
						// 対象の Root GameObject を更新する
						m_Hierarchy_RootGameObject  = rootGameObject ;

						// 検査対象となる GameObject 群を取得する
						m_Hierarchy_TargetGameObjects = GetTargetGameObjects( m_Hierarchy_RootGameObject ) ;

						isRefresh = true ;
//						m_Hierarchy_AllTargetSearch = false ;
					}
				}
				GUI.backgroundColor = Color.white ;

				//---------------------------------------------------------

				// ルートフォルダ
				string path = string.Empty ;
				if( m_Hierarchy_RootGameObject != null )
				{
					path = GetGameObjectPath( m_Hierarchy_RootGameObject.transform ) ;
				}
				EditorGUILayout.TextField( path ) ;

				//---------------------------------------------------------

				// 対象のパスを消去する(全 Asset 対象)
				if( GUILayout.Button( "Clear", GUILayout.Width( 100f ) ) == true )
				{
					// 選択中のアセットを無しにする
//					if( m_Hierarchy_RootGameObject != null )
//					{
//						m_Hierarchy_AllTargetSearch = true ;	// 全対象検査を有効にする
//					}

					m_Hierarchy_RootGameObject = null ;
				}
				GUI.backgroundColor = Color.white ;
			}
			GUILayout.EndHorizontal() ;

			//----------------------------------------------------------

			GUI.contentColor = Color.yellow ;
			GUILayout.Label( "Property Masking", GUILayout.Width( 120f ) ) ;
			GUI.contentColor = Color.white ;

			GUILayout.BeginHorizontal() ;	// 横並び開始
			{
				GUILayout.Label( " ", GUILayout.Width( 20f ) ) ;
				bool visiblePropertyOnly = EditorGUILayout.Toggle( m_Hierarchy_VisiblePropertyOnly, GUILayout.Width( 10f ) ) ;
				if( visiblePropertyOnly != m_Project_VisiblePropertyOnly )
				{
					m_Hierarchy_VisiblePropertyOnly = visiblePropertyOnly ;
				}
				GUILayout.Label( "Visible Property Only", GUILayout.Width( 200f ) ) ;

			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUILayout.BeginHorizontal() ;	// 横並び開始
			{
				GUILayout.Label( " ", GUILayout.Width( 20f ) ) ;
				bool pm_Mesh = EditorGUILayout.Toggle( m_Hierarchy_PM_Mesh, GUILayout.Width( 10f ) ) ;
				if( pm_Mesh != m_Project_PM_Mesh )
				{
					m_Hierarchy_PM_Mesh = pm_Mesh ;
				}
				GUILayout.Label( "Mesh", GUILayout.Width( 120f ) ) ;

				GUILayout.Label( " " ) ;
				bool pm_Material = EditorGUILayout.Toggle( m_Hierarchy_PM_Material, GUILayout.Width( 10f ) ) ;
				if( pm_Material != m_Hierarchy_PM_Material )
				{
					m_Hierarchy_PM_Material = pm_Material ;
				}
				GUILayout.Label( "Material", GUILayout.Width( 120f ) ) ;

				GUILayout.Label( " " ) ;
				bool pm_Shader = EditorGUILayout.Toggle( m_Hierarchy_PM_Shader, GUILayout.Width( 10f ) ) ;
				if( pm_Shader != m_Hierarchy_PM_Shader )
				{
					m_Hierarchy_PM_Shader = pm_Shader ;
				}
				GUILayout.Label( "Shader", GUILayout.Width( 120f ) ) ;

				GUILayout.Label( " " ) ;
				bool pm_Texture = EditorGUILayout.Toggle( m_Hierarchy_PM_Texture, GUILayout.Width( 10f ) ) ;
				if( pm_Texture != m_Hierarchy_PM_Texture )
				{
					m_Hierarchy_PM_Texture = pm_Texture ;
				}
				GUILayout.Label( "Texture", GUILayout.Width( 120f ) ) ;

				GUILayout.Label( " " ) ;
				bool pm_Sprite = EditorGUILayout.Toggle( m_Hierarchy_PM_Sprite, GUILayout.Width( 10f ) ) ;
				if( pm_Sprite != m_Hierarchy_PM_Sprite )
				{
					m_Hierarchy_PM_Sprite = pm_Sprite ;
				}
				GUILayout.Label( "Sprite", GUILayout.Width( 120f ) ) ;

				GUILayout.Label( " " ) ;
				bool pm_Motion = EditorGUILayout.Toggle( m_Hierarchy_PM_Motion, GUILayout.Width( 10f ) ) ;
				if( pm_Motion != m_Hierarchy_PM_Motion )
				{
					m_Hierarchy_PM_Motion = pm_Motion ;
				}
				GUILayout.Label( "Motion", GUILayout.Width( 120f ) ) ;

				GUILayout.Label( " " ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			//------------------------------------------------------------------

			GUILayout.Space( 6f ) ;

			if( m_Hierarchy_TargetGameObjects != null && m_Hierarchy_TargetGameObjects.Count >  0 )
			{
				GUI.backgroundColor = new Color( 1, 0, 1, 1 ) ;
				if( GUILayout.Button( "Refresh" ) == true )
				{
					isRefresh = true ;
				}
				GUI.backgroundColor = Color.white ;

				// 対象が存在する
				EditorGUILayout.BeginHorizontal() ;
//				GUI.contentColor = Color.cyan ;
//				GUILayout.Label( "Target GameObject : " + m_Hierarchy_TargetGameObjects.Count, GUILayout.Width( 200f ) ) ;
//				GUI.contentColor = Color.white ;
				if( m_HierarchyMissings != null && m_HierarchyMissings.Count >  0 )
				{
//					GUILayout.Label( " ", GUILayout.Width( 10f ) ) ;
					GUI.contentColor = new Color32( 255, 127,   0, 255 ) ;
					GUILayout.Label( "Detected Missing : " + m_HierarchyMissings.Count, GUILayout.Width( 160f ) ) ;
					GUI.contentColor = Color.white ;
				}
				EditorGUILayout.EndHorizontal() ;
			}
			else
			{
				GUILayout.Label( "Not found search target GameObjects.", GUILayout.Width( 400f ) ) ;
			}

			//----------------------------------------------------------

			if( isRefresh == true )
			{
				SearchHierarchyMissingAll() ;

				m_Hierarchy_Index				= -1 ;
				m_Hierarchy_PropertyType		= string.Empty ;
				m_Hierarchy_PropertyPath		= string.Empty ;

				m_Hierarchy_GameObject_Index	= -1 ;
				m_Hierarchy_GameObject			= null ;
			}

			//------------------------------------------------------------------------------------------
			// Missing 一覧を表示する

			if( m_Hierarchy_TargetGameObjects != null && m_Hierarchy_TargetGameObjects.Count >  0 )
			{
				if( m_HierarchyMissings != null && m_HierarchyMissings.Count >  0 )
				{
					GameObject activeGameObject = null ;
					if( Selection.gameObjects != null && Selection.gameObjects.Length == 1 )
					{
						// １つだけ選択（複数選択には対応していない：フォルダかファイル）
						activeGameObject = Selection.gameObjects[ 0 ] ;
					}

					// 列見出し
					EditorGUILayout.BeginHorizontal() ;
					EditorGUILayout.LabelField( "Component", GUILayout.Width( 300 ) ) ;
					EditorGUILayout.LabelField( "Property", GUILayout.Width( 150 ) ) ;
					EditorGUILayout.LabelField( "Link", GUILayout.Width( 25 ) ) ;
					EditorGUILayout.LabelField( "GameObjectPath" ) ;
					EditorGUILayout.EndHorizontal() ;

					// リスト表示
					m_Hierarchy_ScrollPosition = EditorGUILayout.BeginScrollView( m_Hierarchy_ScrollPosition ) ;

					int i, l = m_HierarchyMissings.Count ;
					HierarchyMissingData missing ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						missing = m_HierarchyMissings[ i ] ;

						EditorGUILayout.BeginHorizontal() ;

						// Component 情報
						UnityEngine.Object component = missing.ComponentEntity ;
						if( component != null )
						{
							EditorGUILayout.ObjectField( missing.ComponentEntity, missing.ComponentEntity.GetType(), true, GUILayout.Width( 300 ) ) ;
						}
						else
						{
							GUI.contentColor = new Color32( 255,  63,   0, 255 ) ;
							EditorGUILayout.TextField( "Missing", GUILayout.Width( 300 ) ) ;
							GUI.contentColor = Color.white ;
						}

						// Property 情報
						if( m_Hierarchy_Index >= 0 && i == m_Hierarchy_Index )
						{
							GUI.backgroundColor = new Color32(   0, 255, 255, 255 ) ;
						}
						else
						{
							GUI.backgroundColor = Color.white ;
						}

						// アラートレベルによって文字の色を変える
						if( missing.AlertLevel == 2 )
						{
							GUI.contentColor = new Color32( 255,  63,  63, 255 ) ;	// GameObject Missing
						}
						else
						if( missing.AlertLevel == 1 )
						{
							GUI.contentColor = new Color32( 255, 255,   0, 255 ) ;	// Component Missing
						}
						else
						{
							GUI.contentColor = Color.white ;						// Property Missing
						}

						if( GUILayout.Button( missing.PropertyName, GUILayout.Width( 150 ) ) == true )
						{
							m_Hierarchy_Index = i ;
							m_Hierarchy_PropertyType		= missing.PropertyType ;
							m_Hierarchy_PropertyPath		= missing.PropertyPath ;
							m_Hierarchy_ComponentType		= missing.ComponentType ;
						}

						GUI.contentColor = Color.white ;
						GUI.backgroundColor = Color.white ;

						if( i == m_Hierarchy_GameObject_Index )
						{
							GUI.backgroundColor = new Color32( 255, 127, 255, 255 ) ;
						}
						else
						{
							GUI.backgroundColor = Color.white ;
						}

						if( GUILayout.Button( ">", GUILayout.Width( 25 ) ) == true )
						{
							Selection.activeObject = missing.GameObjectEntity ;

							m_Hierarchy_GameObject_Index	= i ;
							m_Hierarchy_GameObject			= missing.GameObjectEntity ;
						}

						GUI.backgroundColor = Color.white ;

						if( i == m_Hierarchy_GameObject_Index && m_Hierarchy_GameObject != null && m_Hierarchy_GameObject == activeGameObject )
						{
							GUI.contentColor = new Color32( 255, 127, 255, 255 ) ;
						}
						else
						{
							GUI.contentColor = Color.white ;
						}

						EditorGUILayout.TextField( missing.GameObjectPath ) ;

						GUI.contentColor = Color.white ;

						EditorGUILayout.EndHorizontal() ;
					}
					EditorGUILayout.EndScrollView() ;

					//--------------------------------

					if( m_Hierarchy_Index >= 0 )
					{
						// 詳細情報を表示する

						// 詳細情報を表示する
						EditorGUILayout.BeginHorizontal() ;

						GUILayout.Label( "ComponentType", GUILayout.Width( 100f ) ) ;
						GUI.contentColor = new Color32( 255, 255,   0, 255 ) ;
						EditorGUILayout.TextField( m_Hierarchy_ComponentType ) ;
						GUI.contentColor = Color.white ;

						EditorGUILayout.EndHorizontal() ;

						EditorGUILayout.BeginHorizontal() ;

						GUILayout.Label( "PropertyPath", GUILayout.Width( 80f ) ) ;
						GUI.contentColor = new Color32( 255, 255,   0, 255 ) ;
						EditorGUILayout.TextField( m_Hierarchy_PropertyPath ) ;
						GUI.contentColor = Color.white ;

						GUILayout.Label( "PropertyType", GUILayout.Width( 96f ) ) ;
						GUI.contentColor = new Color32( 255, 255,   0, 255 ) ;
						EditorGUILayout.TextField( m_Hierarchy_PropertyType, GUILayout.Width( 200f ) ) ;
						GUI.contentColor = Color.white ;

						EditorGUILayout.EndHorizontal() ;
					}
				}
				else
				{
					GUILayout.Label( "Not found missing !!" ) ;
				}
			}
		}

		//---------------------------------------------------------------------------

		// 検査対象となる GameObject 群を取得する
		private List<GameObject> GetTargetGameObjects( GameObject rootGameObject )
		{
			List<GameObject> targetGameObjects = new List<GameObject>() ;

			GetTargetGameObjects( rootGameObject, ref targetGameObjects ) ;

			return targetGameObjects ;
		}

		private void GetTargetGameObjects( GameObject currentGameObject, ref List<GameObject> targetGameObjects )
		{
			// パスを生成する

			if( currentGameObject == null )
			{
				// 全対象

				// Typeで指定した型の全てのオブジェクトを配列で取得し,その要素数分繰り返す.
				foreach( GameObject go in UnityEngine.Resources.FindObjectsOfTypeAll( typeof( GameObject ) ) )
				{
					// アセットからパスを取得.シーン上に存在するオブジェクトの場合,シーンファイル（.unity）のパスを取得
					string path = AssetDatabase.GetAssetOrScenePath( go ) ;

					// シーン上に存在するオブジェクトかどうか文字列で判定
					bool isScene = path.Contains( ".unity" ) ;

					// シーン上に存在するオブジェクトならば処理
					if( isScene == true )
					{
						// GameObject を追加
						targetGameObjects.Add( go ) ;
					}
				}
				return ;
			}

			// 以下のゲームオブジェクトを取得する
			targetGameObjects.Add( currentGameObject ) ;

			// GameObject の場合は 1 つだけで良い
//			int i, l = currentGameObject.transform.childCount ;
//			if( l >  0 )
//			{
//				for( i  = 0 ; i <  l ; i ++ )
//				{
//					GetTargetGameObjects( currentGameObject.transform.GetChild( i ).gameObject, ref targetGameObjects ) ;
//				}
//			}
		}

		//---------------------------------------------------------------------------

		// 対象全ての GameObject で Missing を検出する
		private void SearchHierarchyMissingAll()
		{
			try
			{
				EditorApplication.LockReloadAssemblies() ;
				AssetDatabase.StartAssetEditing() ;

				List<string> propertyMasks = new List<string>() ;
				if( m_Hierarchy_PM_Mesh == true )
				{
					propertyMasks.Add( "PPtr<Mesh>" ) ;
				}
				if( m_Hierarchy_PM_Material == true )
				{
					propertyMasks.Add( "PPtr<$Material>" ) ;
				}
				if( m_Hierarchy_PM_Shader == true )
				{
					propertyMasks.Add( "PPtr<Shader>" ) ;
				}
				if( m_Hierarchy_PM_Texture == true )
				{
					propertyMasks.Add( "PPtr<Texture>" ) ;
				}
				if( m_Hierarchy_PM_Sprite == true )
				{
					propertyMasks.Add( "PPtr<$Sprite>" ) ;
				}
				if( m_Hierarchy_PM_Motion == true )
				{
					propertyMasks.Add( "PPtr<Motion>" ) ;
				}

				//----------------------------------

				if( m_HierarchyMissings == null )
				{
					m_HierarchyMissings = new List<HierarchyMissingData>() ;
				}
				else
				{
					m_HierarchyMissings.Clear() ;
				}

				int i, l = m_Hierarchy_TargetGameObjects.Count ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					// プログレスバーを表示
					EditorUtility.DisplayProgressBar
					(
						"missing searching ...",
						string.Format( "{0}/{1}", i + 1, l ),
						( float )( i + 1 ) / ( float )l
					) ;

					SearchHierarchyMissing( m_Hierarchy_TargetGameObjects[ i ].transform, ref propertyMasks ) ;
				}
			}
			catch( Exception e )
			{
				Debug.LogWarning( "{Hierarchy Seraching] " + e.Message ) ;
			}
			finally
			{
				// プログレスバーを消す
				EditorUtility.ClearProgressBar() ;

				AssetDatabase.StopAssetEditing() ;
				EditorApplication.UnlockReloadAssemblies() ;
			}
		}

		// 指定した GameObject で Missing を検出する
		private void SearchHierarchyMissing( Transform t, ref List<string> propertyMasks )
		{
			// プレハブ内でのパスを取得する
			string gameObjectPath = GetGameObjectPath( t ) ;

			// GameObject 自体が Missing になっているか検査する
		    var status		= PrefabUtility.GetPrefabInstanceStatus( t.gameObject ) ;
			var isMissing	= ( status == PrefabInstanceStatus.MissingAsset ) ;

			if( isMissing == true )
			{
				// GameObject の Missing の場合は子の検査は行わない(意味が無い)
				m_HierarchyMissings.Add( new HierarchyMissingData()
				{
					PropertyName		= "GameObject is Missing",
					PropertyPath		= string.Empty,
					PropertyType		= string.Empty,
					ComponentEntity		= t,
					ComponentType		= "Unknown",
					GameObjectEntity	= t.gameObject,
					GameObjectPath		= gameObjectPath,
					AlertLevel			= 2
				} ) ;
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
						// 参照がロストしている(一番いけないやつ)

						// Component が Missing 状態をプロパティリストに追加する
						m_HierarchyMissings.Add( new HierarchyMissingData()
						{
							PropertyName		= "Component is Missing",
							PropertyPath		= string.Empty,
							PropertyType		= string.Empty,
							ComponentEntity		= t,
							ComponentType		= "Unknown",
							GameObjectEntity	= t.gameObject,
							GameObjectPath		= gameObjectPath,
							AlertLevel			= 1
						} ) ;

						continue ;
					}

					// コンポーネント内のミッシングを検査する
					if( component.name == "Deprecated EditorExtensionImpl" )
					{
						continue ;
					}

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
								( property.objectReferenceValue				== null										) &&
								( property.objectReferenceInstanceIDValue	!= 0										)
							)
							{
								if( propertyMasks.Contains( property.type ) == false )
								{
									// Property が Missing 状態をプロパティリストに追加する
									m_HierarchyMissings.Add( new HierarchyMissingData()
									{
										PropertyName		= property.displayName + " (" + property.name + ")",
										PropertyPath		= property.propertyPath,
										PropertyType		= property.type,
										ComponentEntity		= component,
										ComponentType		= component.GetType().ToString(),
										GameObjectEntity	= t.gameObject,
										GameObjectPath		= gameObjectPath,
										AlertLevel			= 0
									} ) ;
								}
							}

							if( m_Hierarchy_VisiblePropertyOnly == true )
							{
								// 非表示プロパティは無視する
								if( property.NextVisible( true ) == true )
								{
									break ;
								}
							}
							else
							{
								// 非表示プロパティも表示する
								if( property.Next( true ) == false )
								{
									break ;
								}
							}
						}

						so.Dispose() ;
					}
				}
			}

			//----------------------------------------------------------
			// 子の処理を行う

			if( t.childCount >  0 )
			{
				int i, l = t.childCount ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					SearchHierarchyMissing( t.GetChild( i ), ref propertyMasks ) ;
				}
			}
		}

		//-------------------------------------------------------------------------------------------

		// ヒエラルキーのパスを取得する
		private string GetGameObjectPath( Transform self )
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
	}
}
