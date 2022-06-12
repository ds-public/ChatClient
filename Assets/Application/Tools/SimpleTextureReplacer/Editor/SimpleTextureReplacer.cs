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
/// シンプルテクスチャリプレーサー
/// </summary>
namespace Tools.ForAssets
{
	/// <summary>
	/// シンプルテクスチャリプレーサークラス(エディター用) Version 2021/05/05
	/// </summary>

	public class SimpleTextureReplacer : EditorWindow
	{
		[ MenuItem( "Tools/Simple Texture Replacer(別テクスチャの貼り直し)" ) ]
		internal static void OpenWindow()
		{
			var window = EditorWindow.GetWindow<SimpleTextureReplacer>( false, "Texture Replacer", true ) ;
			window.minSize = new Vector2( 640, 480 ) ;
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

		[Serializable]
		public class TextureData
		{
			public	TextureImporterType				TextureType ;	// Texture Texture2D Sprite(S) Sprite(M)
			public	string							DisplayType ;

			public	string							Name ;
			public	Vector2							Size ;

			public	string							Path ;

			public	bool							Same ;

			public	UnityEngine.Object				View ;	// 差し替え用インスタンス
		}

		/// <summary>
		/// 検査対象となるテクスチャファイル群
		/// </summary>
		[SerializeField]
		private List<TextureData>					m_Textures ;

		[SerializeField]
		private bool								m_ShowTextures = true ;

		[SerializeField]
		private List<UnityEngine.Object>			m_TexturePaths ;

		/// <summary>
		/// 貼り直し対象情報(ファイル単位)
		/// </summary>
		[Serializable]
		public class TargetAssetData
		{
			public	string							AssetPath ;
			public	string							AssetType ;				// Asset Prefab Scene
			public	List<TargetAssetReferenceData>	References = new List<TargetAssetReferenceData>() ;
		}

		/// <summary>
		/// 貼り直し対象情報
		/// </summary>
		[Serializable]
		public class TargetAssetReferenceData
		{
			public	string							HierarchyPath ;			// シーンまたはプレハブ内部のパス
			public	string							ComponentType ;			// コンポーネントの型
			public	string							PropertyPath ;			// プロパティのパス
			public	string							PropertyType ;			// プロパティの型(これはテクスチャしかありえないので不要)

			public	string							Type ;
			public	string							Name ;
			public	Vector2							Size ;

			public	UnityEngine.Object				CurrentTexture ;
			public	UnityEngine.Object				ReplaceTexture ;
			public	bool							Same ;					// 参照が差し替え対象と同じかどうか
		}

		[SerializeField]
		private List<TargetAssetData>				m_TargetAssets ;

		[SerializeField]
		private bool								m_ShowTargetAssets = true ;

		[SerializeField]
		private List<UnityEngine.Object>			m_TargetAssetPaths ;

		[SerializeField]
		private bool								m_IsSizeMatching = false ;

		//-----------------------------------

		private int									m_SelectedTextureIndex		= -1 ;
		private int									m_SelectedTargetAssetIndex	= -1 ;

		private int									m_SelectedDetailIndex		= -1 ;
		private int									m_SelectedWindowIndex		=  0 ;

		private Vector2								m_ScrollPosition_T ;
		private Vector2								m_ScrollPosition_TA ;

		private UnityEngine.Object					m_ActiveObject = null ;

		//-------------------------------------------------------------------------------------------

		// アセット種別の識別色
		private readonly Dictionary<string, Color> m_AssetTypeColors = new Dictionary<string, Color>()
		{
			{ "Asset",	new Color32( 255, 127,   0, 255 )	},
			{ "Prefab", Color.cyan	},
			{ "Scene",	Color.green	},
		} ;

		// ＧＵＩ表示する
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

			bool isRefresh = false ;

			int i, l ;

			GUILayout.Space( 6f ) ;
			EditorGUILayout.HelpBox( "テクスチャファイルまたテクスチャファイルが含まれたフォルダを選択して\n[Set]ボタンを押してください", MessageType.Info ) ;

			if( m_Textures != null && m_Textures.Count >  0 )
			{
				GUILayout.BeginHorizontal() ;
				{
					GUI.backgroundColor = new Color32( 255, 255,   0, 255 ) ;
					if( GUILayout.Button( "Refresh", GUILayout.Width( 120f ) ) == true )
					{
						isRefresh = true ;
					}
					GUI.backgroundColor = Color.white ;
				}
				GUILayout.EndHorizontal() ;
			}

			GUILayout.BeginHorizontal() ;
			{
				string count = string.Empty ;
				if( m_Textures != null && m_Textures.Count >  0 )
				{
					// 選択中の検査対象ファイルが１つ以上存在する
					count = " (" + m_Textures.Count + ")" ;
				}

				GUILayout.Label( "Textures" + count, GUILayout.Width( 160f ) ) ;

				GUI.backgroundColor = new Color( 0, 1, 1, 1 ) ;
				if( GUILayout.Button( "Set", GUILayout.Width( 120f ) ) == true )
				{
					if( Selection.objects != null && Selection.objects.Length >  0 )
					{
						m_TexturePaths = new List<UnityEngine.Object>() ;
						foreach( var _ in Selection.objects )
						{
							m_TexturePaths.Add( _ ) ;
						}
					}
					else
					{
						m_TexturePaths = null ;
					}

					isRefresh = true ;
				}
				GUI.backgroundColor = Color.white ;

				if( isRefresh == true )
				{
					m_Textures = GetTextures( m_TexturePaths ) ;
					m_SelectedTextureIndex = -1 ;

					m_TargetAssets = null ;
				}

				//---------------------------------------------------------

				if( m_Textures != null && m_Textures.Count >  0 )
				{
					// 対象のパスを消去する(全 Asset 対象)
					GUI.backgroundColor = new Color32( 255,   0,   0, 255 ) ;
					if( GUILayout.Button( "Clear", GUILayout.Width( 100f ) ) == true )
					{
						// 選択中のアセットを無しにする
						m_Textures = null ;
						m_TargetAssets = null ;
					}
					GUI.backgroundColor = Color.white ;

					// 選択中の検査対象ファイルが１つ以上存在する
					bool showTextures = EditorGUILayout.Toggle( m_ShowTextures, GUILayout.Width( 10f ) ) ;
					if( m_ShowTextures != showTextures )
					{
						m_ShowTextures  = showTextures ;
					}
					GUILayout.Label( " Show Textures", GUILayout.Width( 200f ) ) ;

					// サイズも判定条件にするか
					bool isSizeMatching = EditorGUILayout.Toggle( m_IsSizeMatching, GUILayout.Width( 10f ) ) ;
					if( m_IsSizeMatching != isSizeMatching )
					{
						m_IsSizeMatching  = isSizeMatching ;
						isRefresh = true ;
					}
					GUILayout.Label( " Is Size Matching", GUILayout.Width( 200f ) ) ;
				}
			}
			GUILayout.EndHorizontal() ;

			//--------------------------------------------------------------------------

			if( m_Textures != null && m_Textures.Count >  0 )
			{
				if( m_ShowTextures == true )
				{
					// 対象のシーンファイル一覧を表示する

					string activeTexturePath = string.Empty ;
					if( Selection.objects != null && Selection.objects.Length == 1 && Selection.activeObject != null )
					{
						// １つだけ選択（複数選択には対応していない：フォルダかファイル）
						activeTexturePath = AssetDatabase.GetAssetPath( Selection.activeObject ) ;
					}

					// リスト表示  
					m_ScrollPosition_T = EditorGUILayout.BeginScrollView( m_ScrollPosition_T, GUILayout.Height( 60 ) ) ;
					{
						l = m_Textures.Count ;
						for( i  = 0 ; i <  l ; i ++ )
						{
							GUILayout.BeginHorizontal() ;	// 横並び開始
							{
								var texture = m_Textures[ i ] ;

								EditorGUILayout.TextField( texture.DisplayType, GUILayout.Width( 60 ) ) ;
								if( texture.Same == false )
								{
									GUI.contentColor = Color.white ;
								}
								else
								{
									GUI.contentColor = new Color32( 255, 255,   0, 255 ) ;
								}
								EditorGUILayout.TextField( texture.Name, GUILayout.Width( 80 ) ) ;
								GUI.contentColor = Color.white ;
								textStyle.alignment = TextAnchor.MiddleCenter ;
								EditorGUILayout.TextField( texture.Size.x + " x " + texture.Size.y, textStyle, GUILayout.Width( 85 ) ) ;

								if( m_SelectedTextureIndex >= 0 && i == m_SelectedTextureIndex && texture.Path == activeTexturePath )
								{
									GUI.backgroundColor = new Color32( 255, 127, 255, 255 ) ;
								}
								else
								{
									GUI.backgroundColor = Color.white ;
								}
								if( GUILayout.Button( ">", GUILayout.Width( 25 ) ) == true )
								{
									UnityEngine.Object asset = AssetDatabase.LoadMainAssetAtPath( texture.Path ) ;
									if( asset != null )
									{
										Selection.activeObject = asset ;
									}
									m_SelectedTextureIndex = i ;
								}
								GUI.backgroundColor = Color.white ;

								// パス
								if( string.IsNullOrEmpty( activeTexturePath ) == false && activeTexturePath == texture.Path )
								{
									GUI.contentColor = new Color32( 255, 127, 255, 255 ) ;
								}
								else
								{
									GUI.contentColor = Color.white ;
								}
								EditorGUILayout.TextField( texture.Path ) ;

								if( texture.Same == true )
								{
									GUI.contentColor = new Color32( 255, 127,   0, 255 ) ;
									textStyle.alignment = TextAnchor.MiddleCenter ;
									EditorGUILayout.TextField( "BAD", textStyle, GUILayout.Width( 40 ) ) ;
									GUI.contentColor = Color.white ;
								}
								GUI.contentColor = Color.white ;
							}
							GUILayout.EndHorizontal() ;		// 横並び終了
						}
					}
					EditorGUILayout.EndScrollView() ;
				}
			}
			else
			{
				m_SelectedTextureIndex = -1 ;
			}

			//------------------------------------------------------------------

			if( m_Textures != null && m_Textures.Count >  0 )
			{
				GUILayout.Space( 6f ) ;
				EditorGUILayout.HelpBox( "テクスチャの貼り直しを適用する\nアセットファイルまたアセットファイルが含まれたフォルダを選択して[Set]ボタンを押してください", MessageType.Info ) ;

				GUILayout.BeginHorizontal() ;
				{
					string count = String.Empty ;
					if( m_TargetAssets != null && m_TargetAssets.Count >  0 )
					{
						int total = m_TargetAssets.Sum( _ => _.References.Count ) ;
						count = " (" + m_TargetAssets.Count + ":" + total + ")" ;
					}

					GUILayout.Label( "Target Assets" + count, GUILayout.Width( 160f ) ) ;

					GUI.backgroundColor = new Color( 0, 1, 1, 1 ) ;
					if( GUILayout.Button( "Set", GUILayout.Width( 120f ) ) == true )
					{
						if( Selection.objects != null && Selection.objects.Length >= 1  )
						{
							m_TargetAssetPaths = new List<UnityEngine.Object>() ;
							foreach( var _ in Selection.objects )
							{
								m_TargetAssetPaths.Add( _ ) ;
							}
						}
						isRefresh = true ;
					}
					GUI.backgroundColor = Color.white ;

					if( isRefresh == true )
					{
						m_TargetAssets = GetTargetAssets( m_TargetAssetPaths, ref m_Textures ) ;

						m_SelectedDetailIndex = -1 ;
						m_SelectedTargetAssetIndex = -1 ;
					}

					//---------------------------------------------------------

					if( m_TargetAssets != null && m_TargetAssets.Count >  0 )
					{
						// 対象のパスを消去する(全 Asset 対象)
						GUI.backgroundColor = new Color32( 255,   0,   0, 255 ) ;
						if( GUILayout.Button( "Clear", GUILayout.Width( 100f ) ) == true )
						{
							// 選択中のアセットを無しにする
							m_TargetAssets = null ;
						}
						GUI.backgroundColor = Color.white ;

						bool showTargetAssets = EditorGUILayout.Toggle( m_ShowTargetAssets, GUILayout.Width( 10f ) ) ;
						if( m_ShowTargetAssets != showTargetAssets )
						{
							m_ShowTargetAssets  = showTargetAssets ;
						}
						GUILayout.Label( " Show TargetAssets", GUILayout.Width( 200f ) ) ;
					}
				}
				GUILayout.EndHorizontal() ;

				if( m_TargetAssets != null && m_TargetAssets.Count >  0 )
				{
					if( m_ShowTargetAssets == true )
					{
						// 対象のシーンファイル一覧を表示する

						string activeTargetAssetPath = string.Empty ;
						if( Selection.objects != null && Selection.objects.Length == 1 && Selection.activeObject != null )
						{
							// １つだけ選択（複数選択には対応していない：フォルダかファイル）
							activeTargetAssetPath = AssetDatabase.GetAssetPath( Selection.activeObject ) ;
						}

						// リスト表示  
						m_ScrollPosition_TA = EditorGUILayout.BeginScrollView( m_ScrollPosition_TA, GUILayout.Height( 120 ) ) ;
						{
							l = m_TargetAssets.Count ;
							for( i  = 0 ; i <  l ; i ++ )
							{
								GUILayout.BeginHorizontal() ;	// 横並び開始
								{
									var targetAsset = m_TargetAssets[ i ] ;

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
									GUI.contentColor = m_AssetTypeColors[ targetAsset.AssetType ] ;
									textStyle.alignment = TextAnchor.MiddleCenter ;
									EditorGUILayout.TextField( targetAsset.AssetType, textStyle, GUILayout.Width( 60 ) ) ;
									GUI.contentColor = Color.white ;

									textStyle.alignment = TextAnchor.MiddleCenter ;
									EditorGUILayout.TextField( targetAsset.References.Count.ToString(), textStyle, GUILayout.Width( 25 ) ) ;

									//----------------------------------------------------
									
									if( m_SelectedTargetAssetIndex >= 0 && i == m_SelectedTargetAssetIndex && targetAsset.AssetPath == activeTargetAssetPath )
									{
										GUI.backgroundColor = new Color32( 255, 127, 255, 255 ) ;
									}
									else
									{
										GUI.backgroundColor = Color.white ;
									}
									if( GUILayout.Button( ">", GUILayout.Width( 25 ) ) == true )
									{
										UnityEngine.Object asset = AssetDatabase.LoadMainAssetAtPath( targetAsset.AssetPath ) ;
										if( asset != null )
										{
											Selection.activeObject = asset ;
										}
										m_SelectedTextureIndex = i ;
									}
									GUI.backgroundColor = Color.white ;

									// パス
									if( string.IsNullOrEmpty( activeTargetAssetPath ) == false && activeTargetAssetPath == targetAsset.AssetPath )
									{
										GUI.contentColor = new Color32( 255, 127, 255, 255 ) ;
									}
									else
									{
										GUI.contentColor = Color.white ;
									}
									EditorGUILayout.TextField( targetAsset.AssetPath ) ;
									GUI.contentColor = Color.white ;
								}
								GUILayout.EndHorizontal() ;		// 横並び終了
							}
						}
						EditorGUILayout.EndScrollView() ;

						//---------------------------------------------------------------------------------------
						// 詳細

						if( m_SelectedDetailIndex >= 0 )
						{
							// 詳細情報を表示する

							GUILayout.Space( 6f ) ;

							TargetAssetData targetAsset = m_TargetAssets[ m_SelectedDetailIndex ] ;
							TargetAssetReferenceData item = targetAsset.References[ m_SelectedWindowIndex ] ;

							EditorGUILayout.BeginHorizontal() ;
							{
								// ページング
								i = 1 + m_SelectedWindowIndex ;
								l = targetAsset.References.Count ;

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
									UnityEngine.Object activeObject = AssetDatabase.LoadMainAssetAtPath( targetAsset.AssetPath ) ;
									if( activeObject != null )
									{
										Selection.activeObject = activeObject ;
									}
								}

								GUI.contentColor = new Color32( 255, 255, 127, 255 ) ;
								EditorGUILayout.TextField( targetAsset.AssetPath ) ;
								GUI.contentColor = Color.white ;
							}
							EditorGUILayout.EndHorizontal() ;


							EditorGUILayout.BeginHorizontal() ;
							{
								GUILayout.Label( "AssetType", GUILayout.Width( 70f ) ) ;
								GUI.contentColor =  m_AssetTypeColors[ targetAsset.AssetType ] ;
								EditorGUILayout.TextField( targetAsset.AssetType, GUILayout.Width( 60f ) ) ;
								GUI.contentColor = Color.white ;

								if( string.IsNullOrEmpty( item.HierarchyPath ) == false )
								{
									GUILayout.Label( "HierarchyPath", GUILayout.Width( 90f ) ) ;

									if( GUILayout.Button( ">", GUILayout.Width( 25 ) ) == true )
									{
										if( targetAsset.AssetType == "Prefab" )
										{
											GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>( targetAsset.AssetPath ) ;
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
										if( targetAsset.AssetType == "Scene" )
										{
											// 必ず一旦シーン自体をアクティブにすべし
											UnityEngine.Object activeObject = AssetDatabase.LoadMainAssetAtPath( targetAsset.AssetPath ) ;
											if( activeObject != null )
											{
												Selection.activeObject = activeObject ;
											}

											// 必ず強制ロードする
											bool isSceneLoaded = true ;
											var scene = EditorSceneManager.OpenScene( targetAsset.AssetPath ) ;
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

							//------------------------------

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

							//------------------------------

							EditorGUILayout.BeginHorizontal() ;
							{
								GUILayout.Label( "TextureType", GUILayout.Width( 105f ) ) ;
//								GUI.contentColor = new Color32( 255, 127,   0, 255 ) ;
								EditorGUILayout.TextField( item.Type, GUILayout.Width( 120f ) ) ;
								GUI.contentColor = Color.white ;

								GUILayout.Label( "  Name", GUILayout.Width( 50f ) ) ;
//								GUI.contentColor = new Color32(   0, 255, 255, 255 ) ;
								EditorGUILayout.TextField( item.Name, GUILayout.Width( 160f ) ) ;
								GUI.contentColor = Color.white ;

								GUILayout.Label( "  Size", GUILayout.Width( 40f ) ) ;
//								GUI.contentColor = new Color32(   0, 255, 255, 255 ) ;
								textStyle.alignment = TextAnchor.MiddleCenter ;
								EditorGUILayout.TextField( item.Size.x + " x " + item.Size.y, textStyle, GUILayout.Width( 100f ) ) ;
								GUI.contentColor = Color.white ;
							}
							EditorGUILayout.EndHorizontal() ;

							EditorGUILayout.BeginHorizontal() ;
							{
								GUILayout.Label( "  Current", GUILayout.Width( 60f ) ) ;
//								GUI.contentColor = new Color32(   0, 255, 255, 255 ) ;
								_ = EditorGUILayout.ObjectField( "", item.CurrentTexture, typeof( UnityEngine.Object ), false ) as UnityEngine.Object ;
								GUI.contentColor = Color.white ;
//								GUILayout.Label( " ", GUILayout.Width( 10f ) ) ;
							}
							EditorGUILayout.EndHorizontal() ;

							EditorGUILayout.BeginHorizontal() ;
							{
								GUILayout.Label( "  Replace", GUILayout.Width( 60f ) ) ;
//								GUI.contentColor = new Color32(   0, 255, 255, 255 ) ;
								_ = EditorGUILayout.ObjectField( "", item.ReplaceTexture, typeof( UnityEngine.Object ), false ) as UnityEngine.Object ;
								GUI.contentColor = Color.white ;
//								GUILayout.Label( " ", GUILayout.Width( 10f ) ) ;

								if( item.Same == true )
								{
									GUI.contentColor = new Color32(   0, 255, 255, 255 ) ;
									textStyle.alignment = TextAnchor.MiddleCenter ;
									EditorGUILayout.TextField( "Same", textStyle, GUILayout.Width( 40f ) ) ;
									GUI.contentColor = Color.white ;
								}
							}
							EditorGUILayout.EndHorizontal() ;
						}
					}
				}
				else
				{
					m_SelectedWindowIndex =  0 ;
					m_SelectedDetailIndex = -1 ;
					m_SelectedTargetAssetIndex = -1 ;
				}
			}

			//------------------------------------------------------------------------------------------
			// 実行

			if( m_Textures != null && m_Textures.Count >  0 &&  m_TargetAssets != null && m_TargetAssets.Count >  0 )
			{
				GUILayout.Space( 6f ) ;
				EditorGUILayout.HelpBox( "テクスチャの貼り直しを実行するには[Execute]ボタンを押してください", MessageType.Info ) ;
				
				GUI.backgroundColor = new Color( 0, 1, 1, 1 ) ;
				if( GUILayout.Button( "Execute", GUILayout.Width( 160f ) ) == true )
				{
					// 貼り直しを実行する
					if( EditorUtility.DisplayDialog( "確認", "貼り直し対象になっているテクスチャの\n貼り直しを実行します\nよろしいですか？", "Yes", "No" ) == true )
					{
						m_TargetAssets = Replace( ref m_TargetAssets, ref m_Textures ) ;

						m_SelectedDetailIndex = -1 ;
						m_SelectedTargetAssetIndex = -1 ;
					}
				}
				GUI.backgroundColor = Color.white ;
			}
		}

		//-------------------------------------------------------------------------------------------

		private readonly string[] m_ValidTextureExtensions =
		{
			".png", ".jpg", ".tga", ".psd", ".gif", ".bmp", ".tif", ".tiff", ".iff", ".pict", ".exr",
		} ;

		// 張り替えるテクスチャをピックアップする
		private List<TextureData> GetTextures( List<UnityEngine.Object> selection )
		{
			if( selection == null || selection.Count == 0 )
			{
				return null ;
			}

			//----------------------------------

			List<TextureData> textures = new List<TextureData>() ;
			List<string>	textureNames = new List<string>() ;

			foreach( var selectedObject in selection )
			{
				if( AssetDatabase.Contains( selectedObject ) == true )
				{
					string path = AssetDatabase.GetAssetPath( selectedObject ) ;
					path = path.Replace( "\\", "/" ).TrimStart( '/' ).TrimEnd( '/' ) ;
					if( File.Exists( path ) == true )
					{
						// ファイル
						string extension = Path.GetExtension( path ) ;
						if( m_ValidTextureExtensions.Contains( extension ) == true )
						{
							// テクスチャとして有効なファイル
							AddTexture( path, ref textures, ref textureNames ) ;
						}
					}
					else
					if( Directory.Exists( path ) == true )
					{
						// フォルダ
						GetTextures( path, ref textures, ref textureNames ) ;
					}
				}
			}

			return textures ;
		}

		private void GetTextures( string currentPath, ref List<TextureData> textures, ref List<string> textureNames )
		{
			int i, l ;

			// ファイルを検査する
			string[] files = Directory.GetFiles( currentPath ) ;
			if( files != null && files.Length >  0 )
			{
				// ファイル
				string path ;
				l = files.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					path = files[ i ] ;
					path = path.Replace( "\\", "/" ).TrimStart( '/' ).TrimEnd( '/' ) ;
					string extension = Path.GetExtension( path ) ;
					if( m_ValidTextureExtensions.Contains( extension ) == true )
					{
						// テクスチャとして有効なファイル
						AddTexture( path, ref textures, ref textureNames ) ;
					}
				}
			}

			// フォルダを検査する
			string[] folders = Directory.GetDirectories( currentPath ) ;
			if( folders != null && folders.Length >  0 )
			{
				string path ;
				l = folders.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					path = folders[ i ] ;
					path = path.Replace( "\\", "/" ).TrimStart( '/' ).TrimEnd( '/' ) ;

					GetTextures( path, ref textures, ref textureNames ) ;
				}
			}
		}

		// テクスチャ情報を１つまたは複数追加する
		private void AddTexture( string path, ref List<TextureData> textures, ref List<string> textureNames )
		{
			TextureImporter ti = AssetImporter.GetAtPath( path ) as TextureImporter ;

			if( ti.textureType != TextureImporterType.Sprite )
			{
				// Sprite 以外
				Texture texture = AssetDatabase.LoadAssetAtPath( path, typeof( Texture ) ) as Texture ;

				// Sprite Multiple タイプは分解して登録する
				TextureData textureData = new TextureData()
				{
					Path = path
				} ;
				textureData.TextureType = ti.textureType ;

				textureData.DisplayType = ti.textureType.ToString() ;
				textureData.Name = texture.name ;
				textureData.View = texture ;
				textureData.Size = new Vector2( texture.width, texture.height ) ;

				AddTexture( textureData, ref textures, ref textureNames ) ;
			}
			else
			{
				// Sprite
				if( ti.spriteImportMode != SpriteImportMode.Multiple )
				{
					// Single 扱い
					Sprite sprite = AssetDatabase.LoadAssetAtPath( path, typeof( Sprite ) ) as Sprite ;

					// Sprite Multiple タイプは分解して登録する
					TextureData textureData = new TextureData()
					{
						Path = path
					} ;
					textureData.TextureType = ti.textureType ;

					textureData.DisplayType = "Sprite(S)" ;
					textureData.Name = sprite.name ;
					textureData.View = sprite ;
					textureData.Size = new Vector2( sprite.rect.width, sprite.rect.height ) ;

					AddTexture( textureData, ref textures, ref textureNames ) ;
				}
				else
				{
					// Multiple
					// 注意:LoadAllAssetsAtPath と LoadAllAssetRepresentationsAtPath の違い
					//  LoadAllAssetsAtPath → 内包される全てのコンポーネントを取得する(GameObject Transform Image …)
					//  LoadAllAssetRepresentationsAtPath → 直接の子コンポーネントのみを取得する(GameObject)


					UnityEngine.Object[] objects = AssetDatabase.LoadAllAssetRepresentationsAtPath( path ) ;
					foreach( var _ in objects )
					{
						if( _ is Sprite )
						{
							// 念の為 Sprite 確認を行う
							Sprite sprite = _ as Sprite ;

							// Sprite Multiple タイプは分解して登録する
							TextureData textureData = new TextureData()
							{
								Path = path
							} ;
							textureData.TextureType = ti.textureType ;

							textureData.DisplayType = "Sprite(M)" ;
							textureData.Name = sprite.name ;
							textureData.View = sprite ;
							textureData.Size = new Vector2( sprite.rect.width, sprite.rect.height ) ;

							AddTexture( textureData, ref textures, ref textureNames ) ;
						}
					}
				}
			}
		}

		private void AddTexture( TextureData textureData, ref List<TextureData> textures, ref List<string> textureNames )
		{
			if( textureNames.Contains( textureData.Name ) == false )
			{
				textureNames.Add( textureData.Name ) ;
			}
			else
			{
				textureData.Same = true ;
			}

			textures.Add( textureData ) ;
		}

		//-------------------------------------------------------------------------------------------
		// 張替え対象を取得する

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

		// 張替え対象となるアセットのプロパティをピックアップする
		private List<TargetAssetData> GetTargetAssets( List<UnityEngine.Object> selection, ref List<TextureData> textures )
		{
			if( selection == null || selection.Count == 0 )
			{
				return null ;
			}

			//----------------------------------------------------------

			// まずは拡張子で候補となる絞り込みを行う
			List<string> targetAssetPaths = new List<string>() ;

			foreach( var selectedObject in selection )
			{
				if( AssetDatabase.Contains( selectedObject ) == true )
				{
					string path = AssetDatabase.GetAssetPath( selectedObject ) ;
					path = path.Replace( "\\", "/" ).TrimStart( '/' ).TrimEnd( '/' ) ;
					if( File.Exists( path ) == true )
					{
						// ファイル
						string extension = Path.GetExtension( path ) ;
						if( m_ExclusionExtensions.Contains( extension ) == false )
						{
							// 張替え対象として有効なファイル
							targetAssetPaths.Add( path ) ;
						}
					}
					else
					if( Directory.Exists( path ) == true )
					{
						// フォルダ
						GetTargetAssetPaths( path, ref targetAssetPaths ) ;
					}
				}
			}
			
			//--------------------------------------------------------------------------

			return SearchOrReplace( ref targetAssetPaths, ref textures, false ) ;
		}

		private void GetTargetAssetPaths( string currentPath, ref List<string> targetAssetPaths )
		{
			int i, l ;

			// ファイルを検査する
			string[] files = Directory.GetFiles( currentPath ) ;
			if( files != null && files.Length >  0 )
			{
				// ファイル
				string path ;
				l = files.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					path = files[ i ] ;
					path = path.Replace( "\\", "/" ).TrimStart( '/' ).TrimEnd( '/' ) ;
					string extension = Path.GetExtension( path ) ;
					if( m_ExclusionExtensions.Contains( extension ) == false )
					{
						// 張替え対象として有効なファイル
						targetAssetPaths.Add( path ) ;
					}
				}
			}

			// フォルダを検査する
			string[] folders = Directory.GetDirectories( currentPath ) ;
			if( folders != null && folders.Length >  0 )
			{
				string path ;
				l = folders.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					path = folders[ i ] ;
					path = path.Replace( "\\", "/" ).TrimStart( '/' ).TrimEnd( '/' ) ;

					GetTargetAssetPaths( path, ref targetAssetPaths ) ;
				}
			}
		}

		// テクスチャの貼り直しを実行する
		private List<TargetAssetData> Replace( ref List<TargetAssetData> targetAssets, ref List<TextureData> textures )
		{
			List<string> targetAssetPaths = targetAssets.Select( _ => _.AssetPath ).ToList() ;

			var result = SearchOrReplace( ref targetAssetPaths, ref textures, true ) ;

			AssetDatabase.Refresh() ;
			AssetDatabase.SaveAssets() ;

			return result ;
		}

		//-------------------------------------------------------------------------------------------

		// 張替え対象の取得または実行を行う
		private List<TargetAssetData> SearchOrReplace( ref List<string> targetAssetPaths, ref List<TextureData> textures, bool execute )
		{
			if( targetAssetPaths == null || targetAssetPaths.Count == 0 )
			{
				return null ;
			}

			//----------------------------------------------------------

			Dictionary<string, TargetAssetData> targetAssets = new Dictionary<string, TargetAssetData>() ;

			try
			{
				// コンパイル禁止
				EditorApplication.LockReloadAssemblies() ;

				//---------------------------------------------------------

				Dictionary<string, TextureData> textures_hash = new Dictionary<string, TextureData>() ;
				foreach( var texture in textures )
				{
					if( textures_hash.ContainsKey( texture.Name ) == false )
					{
						textures_hash.Add( texture.Name, texture ) ;
					}
				}

				//------------------------------------------------------------------
				// 開いていたシーン名を保存する
				bool isChangeScene = false ;
				var activeScene = EditorSceneManager.GetActiveScene() ;
				string activeScenepath = activeScene.path ;
				//------------------------------------------------------------------

				string message ;

				if( execute == false )
				{
					message = "texture reference searching ..." ;
				}
				else
				{
					message = "texture reference replacing ..." ;

				}

				int i, l = targetAssetPaths.Count ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					// プログレスバーを表示
					EditorUtility.DisplayProgressBar
					(
						message,
						string.Format( "{0}/{1}", i + 1, l ),
						( float )( i + 1 ) / ( float )l
					) ;

					string targetAssetPath = targetAssetPaths[ i ] ;
					string extension = Path.GetExtension( targetAssetPath ) ;

					if( extension != ".prefab" && extension != ".unity" )
					{
						// アセットファイル
						SearchOrReplaceAsset( targetAssetPath, ref targetAssets, ref textures_hash, execute ) ;
					}
					else
					if( extension == ".prefab" )
					{
						// プレハブファイル
						SearchOrReplacePrefab( targetAssetPath, ref targetAssets, ref textures_hash, execute ) ;
					}
					else
					if( extension == ".unity" )
					{
						// シーンファイル
						if( SearchOrReplaceScene( targetAssetPath, ref targetAssets, ref textures_hash, execute ) == true )
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
			}
			catch( Exception e )
			{
				EditorUtility.DisplayDialog( "結果", "失敗しました", "閉じる" ) ;

				Debug.LogWarning( "[SearchOrReplace Error] " + e.Message ) ;
			}
			finally
			{
				// プログレスバーを消す
				EditorUtility.ClearProgressBar() ;

				// コンパイル許可
				EditorApplication.UnlockReloadAssemblies() ;
			}

			if( targetAssets.Count == 0 )
			{
				return null ;
			}

			return targetAssets.Values.ToList() ;
		}

		// アセットを検査するか張替えを実行する
		private void SearchOrReplaceAsset( string targetAssetPath, ref Dictionary<string, TargetAssetData> targetAssets, ref Dictionary<string, TextureData> textures_hash, bool execute )
		{
			UnityEngine.Object[] components = AssetDatabase.LoadAllAssetsAtPath( targetAssetPath ) ;
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
				AddProjectMissingOfProperty( component, targetAssetPath, "Asset", string.Empty, ref targetAssets, ref textures_hash, execute ) ;
			}
		}

		// プレハブを検査するか張替えを実行する
		private void SearchOrReplacePrefab( string targetAssetPath, ref Dictionary<string, TargetAssetData> targetAssets, ref Dictionary<string, TextureData> textures_hash, bool execute )
		{
			// メインアセットのみ取得
			GameObject prefab = AssetDatabase.LoadMainAssetAtPath( targetAssetPath ) as GameObject ;
			if( prefab != null )
			{
				SearchHierarchy( prefab.transform, targetAssetPath, "Prefab", ref targetAssets, ref textures_hash, execute ) ;

				// プレハブの変更を反映する(特に特別な事をする必要はない)
			}
			else
			{
				Debug.LogWarning( "[Prefab] Could not load : " + targetAssetPath ) ;
			}
		}
		
		// シーンを検査するか張替えを実行する
		private bool SearchOrReplaceScene( string targetAssetPath, ref Dictionary<string, TargetAssetData> targetAssets, ref Dictionary<string, TextureData> textures_hash, bool execute )
		{
			var scene = EditorSceneManager.OpenScene( targetAssetPath ) ;
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
					SearchHierarchy( rootObject.transform, targetAssetPath, "Scene", ref targetAssets, ref textures_hash, execute ) ;
				}
			}

			if( execute == true )
			{
				// 変更したシーンを保存する
				if( EditorSceneManager.SaveScene( scene ) == false )
				{
					Debug.LogWarning( "[Failed] Save Scene : " + targetAssetPath ) ;
				}
			}

			//----------------------------------------------------------

			return true ;
		}


		//-----------------------------------

		// 再帰的に検査する
		private void SearchHierarchy( Transform t, string targetAssetPath, string assetType, ref Dictionary<string, TargetAssetData> targetAssets, ref Dictionary<string, TextureData> textures_hash, bool execute )
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
					AddProjectMissingOfProperty( component, targetAssetPath, assetType, hierarchyPath, ref targetAssets, ref textures_hash, execute ) ;
				}
			}

			//----------------------------------------------------------
			// 子の処理を行う

			if( t.childCount >  0 )
			{
				int i, l = t.childCount ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					SearchHierarchy( t.GetChild( i ), targetAssetPath, assetType, ref targetAssets, ref textures_hash, execute ) ;
				}
			}
		}

		//-----------------------------------------------------------

		// プロパティでミッシングを起こしているものを追加する
		private void AddProjectMissingOfProperty( UnityEngine.Object component, string targetAssetPath, string assetType, string hierarchyPath, ref Dictionary<string, TargetAssetData> targetAssets, ref Dictionary<string, TextureData> textures_hash, bool execute )
		{
			// SerializedObjectを通してアセットのプロパティを取得する
			SerializedObject so = new SerializedObject( component ) ;
			if( so != null )
			{
				so.Update() ;	// 更新

				bool isDirty = false ;

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


							// 且つテクスチャのみ有効
							if
							(
								(
									property.objectReferenceValue is Texture ||
									property.objectReferenceValue is Sprite
								) &&
								textures_hash.ContainsKey( property.objectReferenceValue.name ) == true
							)
							{
								// 現在参照しているパス
								string referencedAssetPath = AssetDatabase.GetAssetPath( property.objectReferenceValue ) ;
								referencedAssetPath = referencedAssetPath.Replace( "\\", "/" ).TrimStart( '/' ).TrimEnd( '/' ) ;

								TextureImporter ti = AssetImporter.GetAtPath( referencedAssetPath ) as TextureImporter ;

								// タイプを確認する
								if( ti.textureType == textures_hash[ property.objectReferenceValue.name ].TextureType )
								{
//									UnityEngine.Object currentTexture ;
									Vector2 size ;

									if( ti.textureType != TextureImporterType.Sprite )
									{
										// Sprite 以外
										Texture texture = property.objectReferenceValue as Texture ;

//										currentTexture = texture ;
										size = new Vector2( texture.width, texture.height ) ;
									}
									else
									{
										// Sprite
										Sprite sprite = property.objectReferenceValue as Sprite ;

//										currentTexture = sprite ;
										size = new Vector2( sprite.rect.width, sprite.rect.height ) ;
									}


									bool isOk = true ;
									if( m_IsSizeMatching == true )
									{
										// サイズが同じ場合のみ張替え対象とする

										if( textures_hash[ property.objectReferenceValue.name ].Size != size )
										{
											isOk = false ;
										}
									}

									if( isOk == true )
									{
										if( execute == true )
										{
											// 貼り直しを実行する
											if( property.objectReferenceValue != textures_hash[ property.objectReferenceValue.name ].View )
											{
//												Debug.Log( "貼り直しを行う:" + textures_hash[ property.objectReferenceValue.name ].Path ) ;
												property.objectReferenceValue			= textures_hash[ property.objectReferenceValue.name ].View ;
//												property.objectReferenceInstanceIDValue	= AssetDatabase.AssetPathToGUID( textures_hash[ property.objectReferenceValue.name ].Path ) ;
												property.objectReferenceInstanceIDValue	= textures_hash[ property.objectReferenceValue.name ].View.GetInstanceID() ;

												isDirty = true ;
											}
										}

										//---------------------------------------------------

										// スクリプトの参照は無視する・パッケージへの参照は無視する
										TargetAssetData targetAssetData ;

										if( targetAssets.ContainsKey( targetAssetPath ) == false )
										{
											// まだ登録は無い

											targetAssetData = new TargetAssetData()
											{
												AssetPath		= targetAssetPath,
												AssetType		= assetType,
											} ;

											targetAssets.Add( targetAssetPath, targetAssetData ) ;
										}
										else
										{
											// 既に登録がある
											targetAssetData = targetAssets[ targetAssetPath ] ;
										}

										TargetAssetReferenceData referenceData = new TargetAssetReferenceData()
										{
											HierarchyPath	= hierarchyPath,
											ComponentType	= component.GetType().ToString().Replace( "UnityEngine.", "" ),
											PropertyType	= property.objectReferenceValue.GetType().ToString().Replace( "UnityEngine.", "" ),
											PropertyPath	= property.propertyPath,
										} ;

										if( ti.textureType != TextureImporterType.Sprite )
										{
											// Sprite 以外
											referenceData.Type = ti.textureType.ToString() ;
											referenceData.Name = property.objectReferenceValue.name ;
											referenceData.Size = size ;
										}
										else
										{
											// Sprite
											if( ti.spriteImportMode != SpriteImportMode.Multiple )
											{
												// Single 扱い
												referenceData.Type = "Sprite(S)" ;
											}
											else
											{
												// Multiple
												// 注意:LoadAllAssetsAtPath と LoadAllAssetRepresentationsAtPath の違い
												//  LoadAllAssetsAtPath → 内包される全てのコンポーネントを取得する(GameObject Transform Image …)
												//  LoadAllAssetRepresentationsAtPath → 直接の子コンポーネントのみを取得する(GameObject)
												referenceData.Type = "Sprite(M)" ;
											}

											referenceData.Name = property.objectReferenceValue.name ;
											referenceData.Size = size ;
										}


										// 張り替えるテクスチャ
										referenceData.CurrentTexture = property.objectReferenceValue ;
										referenceData.ReplaceTexture = textures_hash[ property.objectReferenceValue.name ].View ;

										// 現在のテクスチャと張替え対象のテクスチャが同じかどうか
										referenceData.Same = ( property.objectReferenceValue == textures_hash[ property.objectReferenceValue.name ].View ) ;

										targetAssetData.References.Add( referenceData ) ;
									}
								}
							}
						}
					}

					// 非表示プロパティも表示する
					if( property.Next( true ) == false )
					{
						break ;
					}
				}

				if( execute == true && isDirty == true )
				{
					// 変更点を反映する
//					Debug.Log( "変更したプロパティを反映する" ) ;
					if( so.ApplyModifiedProperties() == false )
					{
						Debug.LogWarning( "[Replace Failed]" + targetAssetPath + " " + hierarchyPath + " " + component.GetType().ToString() ) ;
//						Debug.Log( "------->しかし失敗" ) ;
					}
				}

				so.Dispose() ;
			}
		}

		//-----------------------------------------------------------

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
