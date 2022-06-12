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
	/// ミッシングディテクタークラス(エディター用) Version 2021/04/28
	/// </summary>

	public class SimplePNGIntegrator : EditorWindow
	{
		[ MenuItem( "Tools/Simple PNG Integrator(TGA→PNG)" ) ]
		internal static void OpenWindow()
		{
			var window = EditorWindow.GetWindow<SimplePNGIntegrator>( false, "PNG Integrator", true ) ;
			window.minSize = new Vector2( 640, 320 ) ;
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
		// Project

		private string			m_RootAssetPath ;

		[Serializable]	// リコンパイルしてもリストを消さないために必要
		public class TextureFile
		{
			public	string				Path ;					// テクスチャのパス
			public	string				Type ;					// テクスチャのタイプ
			public	UnityEngine.Object	Instance ;				// テクスチャのインスタンス
			public	List<string>		ReferencedAssetPaths ;	// 参照アセットリスト
			public	int					ReferencedCount ;		// 参照数

			public	bool				Enable ;				// 処理対象

			public	UnityEngine.Object	NewInstance ;			// 変換後のインスタンス
		}


		private List<TextureFile>	m_TextureFiles ;

		private Vector2				m_ScrollPosition ;

		private bool				m_AllTargetSearch = true ;

		private bool				m_RemoveFileIfReferenceIsZero = false ;

		private int					m_ContentCount = 0 ;
		private int					m_ReferencedContentCount = 0 ;

		//-----------------------------------------------------------

		public static bool			IsProcessing = false ;
		//-----------------------------------------------------------

		// Project タブを表示する
		private void DrawGUI()
		{
			bool isRefresh = false ;

			GUILayout.Space( 6f ) ;

			EditorGUILayout.HelpBox( "無圧縮または可逆圧縮系のフォーマットの画像をPNGフォーマットに統合します\n統合化対象のルートパスを選択して[Root AssetPath]ボタンを押してください", MessageType.Info ) ;

			GUILayout.BeginHorizontal() ;
			{
				string rootAssetPath = m_RootAssetPath ;

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

					if( m_RootAssetPath != rootAssetPath || m_AllTargetSearch == true )
					{
						// 対象のルートパスを更新する
						m_RootAssetPath = rootAssetPath ;

						isRefresh = true ;
						m_AllTargetSearch = false ;
					}
				}
				GUI.backgroundColor = Color.white ;

				//---------------------------------------------------------

				// ルートフォルダ
				EditorGUILayout.TextField( m_RootAssetPath ) ;

				//---------------------------------------------------------

				// 対象のパスを消去する(全 Asset 対象)
				if( GUILayout.Button( "Clear", GUILayout.Width( 100f ) ) == true )
				{
					// 選択中のアセットを無しにする
					if( string.IsNullOrEmpty( m_RootAssetPath ) == false )
					{
						m_AllTargetSearch = true ;	// 全対象検査を有効にする
					}

					Selection.activeObject = null ;
					m_RootAssetPath = string.Empty ;
				}
				GUI.backgroundColor = Color.white ;
			}
			GUILayout.EndHorizontal() ;

			//------------------------------------------------------------------

			GUILayout.Space( 6f ) ;

			if( m_TextureFiles != null && m_TextureFiles.Count >  0 )
			{
				GUI.backgroundColor = new Color( 1, 0, 1, 1 ) ;
				if( GUILayout.Button( "Refresh" ) == true )
				{
					isRefresh = true ;
				}
				GUI.backgroundColor = Color.white ;

				if( m_ContentCount == 0 )
				{
					GUILayout.Space( 6f ) ;
					EditorGUILayout.HelpBox( "[Root AssetPath]で指定した範囲内にマテリアル等のテクスチャを参照するファイルが見つかりませんでした\nこのままテクスチャを削除すると[Root AssetPath]外にあるマテリアル等のファイルのテクスチャへの参照が失われる可能性があります\n[Root AssetPath]には基本的に参照元のマテリアル等のファイルと参照先のテクスチャがセットで含まれる場所を指定してください", MessageType.Warning ) ;
					GUILayout.Space( 6f ) ;
				}

				// 対象が存在する
				EditorGUILayout.BeginHorizontal() ;

				GUI.contentColor = Color.cyan ;
				GUILayout.Label( "Target Texture : " + m_TextureFiles.Count, GUILayout.Width( 120f ) ) ;
				GUI.contentColor = Color.white ;

				GUI.contentColor = Color.white ;
				GUILayout.Label( "Total Reference : " + m_ReferencedContentCount, GUILayout.Width( 120f ) ) ;
				GUI.contentColor = Color.white ;

				EditorGUILayout.EndHorizontal() ;	
			}
			else
			{
				GUILayout.Label( "Not found search target Assets.", GUILayout.Width( 400f ) ) ;
			}

			//------------------------------------------------------------------------------------------

			if( isRefresh == true )
			{
				// 検査対象のパス群を取得する
				Search() ;
			}

			//------------------------------------------------------------------------------------------
			// 変換対象一覧を表示する

			if( m_TextureFiles != null && m_TextureFiles.Count >  0 )
			{
				int i, l ;
				TextureFile file ;

				GUILayout.BeginHorizontal() ;	// 横並び開始
				{
//					GUILayout.Label( " ", GUILayout.Width( 20f ) ) ;
					bool removeFileIfReferenceIsZero = EditorGUILayout.Toggle( m_RemoveFileIfReferenceIsZero, GUILayout.Width( 10f ) ) ;
					if( removeFileIfReferenceIsZero != m_RemoveFileIfReferenceIsZero )
					{
						m_RemoveFileIfReferenceIsZero  = removeFileIfReferenceIsZero ;

						l = m_TextureFiles.Count ;
						for( i  = 0 ; i <  l ; i ++ )
						{
							file = m_TextureFiles[ i ] ;

							if( file.ReferencedCount == 0 )
							{
								file.Enable = m_RemoveFileIfReferenceIsZero ;
							}
						}
					}
					GUI.contentColor = new Color32( 255, 255,   0, 255 ) ;
					GUILayout.Label( " Delete files with 0 references.", GUILayout.Width( 320f ) ) ;
					GUI.contentColor = Color.white ;
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				if( GUILayout.Button( "All", GUILayout.Width( 80f ) ) == true )
				{
					l = m_TextureFiles.Count ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						file = m_TextureFiles[ i ] ;
						file.Enable = true ;
					}
				}

				GUILayout.Space( 6f ) ;

				//---------------------------------------------------------

				string activeAssetPath = string.Empty ;
				if( Selection.objects != null && Selection.objects.Length == 1 && Selection.activeObject != null )
				{
					// １つだけ選択（複数選択には対応していない：フォルダかファイル）
					activeAssetPath = AssetDatabase.GetAssetPath( Selection.activeObject.GetInstanceID() ) ;
				}

				// 列見出し  
				EditorGUILayout.BeginHorizontal() ;
				EditorGUILayout.LabelField( "Target", GUILayout.Width( 100 ) ) ;
				EditorGUILayout.LabelField( "Reference", GUILayout.Width( 75 ) ) ;
				EditorGUILayout.LabelField( "Type", GUILayout.Width( 50 ) ) ;
				EditorGUILayout.LabelField( "Link", GUILayout.Width( 30 ) ) ;
				EditorGUILayout.LabelField( "Path" ) ;
				EditorGUILayout.EndHorizontal() ;

				//---------------------------------------------------------

				// リスト表示  
				m_ScrollPosition = EditorGUILayout.BeginScrollView( m_ScrollPosition ) ;

				int modifyCount = 0 ;
				int removeCount = 0 ;

				l = m_TextureFiles.Count ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					file = m_TextureFiles[ i ] ;

					EditorGUILayout.BeginHorizontal() ;
					{	
						file.Enable = EditorGUILayout.Toggle( file.Enable, GUILayout.Width( 20f ) ) ;
						GUI.contentColor =  file.ReferencedCount != 0 ? new Color( 1, 1, 1, 1 ) : new Color( 1, 0.5f, 0, 1 ) ;
						if( GUILayout.Button( file.ReferencedCount != 0 ? "Modify" : "Delete", GUILayout.Width( 80f ) ) == true )
						{
							file.Enable = !file.Enable ;
						}
						GUI.contentColor = Color.white ;

						// Reference
						GUI.contentColor =  file.ReferencedCount != 0 ? Color.white : Color.green ;
						EditorGUILayout.TextField( file.ReferencedCount.ToString(), GUILayout.Width(  75 ) ) ;
						GUI.contentColor = Color.white ;

						// Type
						GUI.contentColor = Color.cyan ;
						EditorGUILayout.TextField( file.Type, GUILayout.Width(  50 ) ) ;
						GUI.contentColor = Color.white ;

						//--------------------------------

						// Link
						string assetPath = m_TextureFiles[ i ].Path ;

						if( activeAssetPath == assetPath )
						{
							GUI.backgroundColor = Color.cyan ;
						}
						else
						{
							GUI.backgroundColor = Color.white ;
						}

						if( GUILayout.Button( ">", GUILayout.Width( 30 ) ) == true )
						{
							UnityEngine.Object asset = AssetDatabase.LoadMainAssetAtPath( assetPath ) ;
							if( asset != null )
							{
								Selection.activeObject = asset ;
							}
						}

						GUI.backgroundColor = Color.white ;

						// Path
						EditorGUILayout.TextField( assetPath ) ;
					}
					EditorGUILayout.EndHorizontal() ;

					// 変換対象をカウントする
					if( file.ReferencedCount >  0 && file.Enable == true )
					{
						modifyCount ++ ;
					}
					if( file.ReferencedCount == 0 && file.Enable == true )
					{
						removeCount ++ ;
					}
				}
				EditorGUILayout.EndScrollView() ;

				//---------------------------------------------------------
				// 変換実行ボタン

				if( modifyCount >  0 )
				{
					GUI.backgroundColor = new Color( 0, 1, 1, 1 ) ;
					if( GUILayout.Button( "Modify"  ) == true )
					{
						// 変換
						Modify() ;

						// 更新
						Search() ;

						if( m_RemoveFileIfReferenceIsZero == true )
						{
							// 削除
							Remove() ;

							// 更新
							Search() ;
						}
					}
					GUI.backgroundColor = Color.white ;
				}
				else
				if( removeCount >  0 )
				{
					GUI.backgroundColor = new Color( 1, 0, 0, 1 ) ;
					if( GUILayout.Button( "Delete" ) == true )
					{
						// 削除
						Remove() ;

						// 更新
						Search() ;
					}
					GUI.backgroundColor = Color.white ;
				}
			}
		}

		//---------------------------------------------------------------------------

		// 情報を収集する
		private void Search()
		{
			m_TextureFiles = null ;

			List<string> texturePaths = GetTexturePaths( m_RootAssetPath ) ;
			if( texturePaths == null || texturePaths.Count == 0 )
			{
				return ;
			}

			List<string> contentPaths = GetContentPaths( m_RootAssetPath ) ;

			m_ContentCount = 0 ;
			if( contentPaths != null && contentPaths.Count >  0 )
			{
				m_ContentCount = contentPaths.Count ;
			}

			SearchReference( ref texturePaths, ref contentPaths ) ;
		}


		// 検査対象となる AssetPath 群を取得する
		private List<string> GetTexturePaths( string currentPath )
		{
			// パスを生成する
			List<string> targetAssetPaths = new List<string>() ;

			// フィルターを設定する(JPG PNG を除く)
			List<string> filter = new List<string>()
			{
				".bmp",
				".exr",
				".gif",
				".hdr",
				".iff",
				".pict",
				".psd",
				".tga",
				".tiff",
			} ;

			GetTargetAssetPaths( currentPath, ref targetAssetPaths, ref filter ) ;

			return targetAssetPaths ;
		}

		// 変更対象となる AssetPath 群を取得する
		private List<string> GetContentPaths( string currentPath )
		{
			// パスを生成する
			List<string> targetAssetPaths = new List<string>() ;

			// フィルターを設定する
			List<string> filter = new List<string>()
			{
				".mat",
				".prefab",
				".asset",
			} ;

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
						if( filter.Contains( Path.GetExtension( allPaths[ i ] ) ) == true )
						{
							// 有効なパス
							targetAssetPaths.Add( allPaths[ i ].Replace( "\\", "/" ) ) ;
						}
					}
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

		// 指定したフォルダ以下のアセットの全プロパティで指定のGUIを指しているものをピックアップする
		private void SearchReference( ref List<string> texturePaths, ref List<string> contentPaths )
		{
			//----------------------------------------------------------

			m_TextureFiles = new List<TextureFile>() ;

			int i, l ;
			
			TextureFile textureFile ;
			string path ;

			l = texturePaths.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				textureFile = new TextureFile() ;

				path = texturePaths[ i ] ;
				textureFile.Path					= path ;
				textureFile.Type					= Path.GetExtension( path ).TrimStart( '.' ).ToUpper() ;
				textureFile.Instance				= AssetDatabase.LoadAssetAtPath( path, typeof( UnityEngine.Object ) ) ;
				textureFile.ReferencedAssetPaths	= new List<string>() ;
				textureFile.ReferencedCount			= 0 ;

				m_TextureFiles.Add( textureFile ) ;
			}

			//----------------------------------------------------------

			if( contentPaths != null && contentPaths.Count >  0 )
			{
				List<string> propertyMasks = new List<string>()
				{
					"PPtr<Texture>",
					"PPtr<$Sprite>",
				} ;

				l = contentPaths.Count ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					// プログレスバーを表示  
					EditorUtility.DisplayProgressBar( "searching ...", string.Format( "{0}/{1}", i + 1, l ), ( float )i / l ) ;

					path = contentPaths[ i ] ;
					SearchReference( path, ref propertyMasks, ref m_TextureFiles ) ;
				}

				// プログレスバーを消す
				EditorUtility.ClearProgressBar() ;
			}

			m_ReferencedContentCount = 0 ;

			if( m_TextureFiles != null && m_TextureFiles.Count >  0 )
			{
				TextureFile file ;

				l = m_TextureFiles.Count ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					file = m_TextureFiles[ i ] ;
					if( file.ReferencedCount >  0 )
					{
						file.Enable = true ;
						m_ReferencedContentCount += file.ReferencedCount ;
					}
					else
					{
						file.Enable = m_RemoveFileIfReferenceIsZero ;
					}
				}
			}
		}

		private void SearchReference( string path, ref List<string> propertyMasks, ref List<TextureFile> textureFiles )
		{
			UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath( path ) ;
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

				// プロパティのテクスチャ参照を追加する
				AddReferenceInProperty( path, asset, ref propertyMasks, ref textureFiles ) ;
			}
		}

		// 該当のテクスチャを参照している箇所を追加する
		private void AddReferenceInProperty( string path, UnityEngine.Object asset, ref List<string> propertyMasks, ref List<TextureFile> textureFiles )
		{
			// SerializedObjectを通してアセットのプロパティを取得する  
			SerializedObject so = new SerializedObject( asset ) ;
			if( so != null )
			{
				SerializedProperty property = so.GetIterator() ;
				if( property != null )
				{
					while( true )
					{
						// プロパティの種類がオブジェクト（アセット）への参照で、  
						// その参照が null なのにもかかわらず、参照先インスタンス識別子が 0 でないものは Missing 状態！  
						if( property.propertyType == SerializedPropertyType.ObjectReference )
						{
							// オブジェクトを参照して箇所

							if( propertyMasks.Contains( property.type ) == true )
							{
								// 検査対象(Texture or Sprite)
								foreach( var textureFile in textureFiles )
								{
									if( property.objectReferenceValue == textureFile.Instance )
									{
										// 発見した
										// プロパティへの参照は保存できないのでアセットのパスを保存する
										if( textureFile.ReferencedAssetPaths.Contains( path ) == false )
										{
											textureFile.ReferencedAssetPaths.Add( path ) ;
										}
										textureFile.ReferencedCount ++ ;	// １つのアセットで２つ以上の参照がある場合もある
									}
								}
							}
						}

						// 非表示プロパティも表示する
						if( property.Next( true ) == false )
						{
							break ;	// プロパティを全て検査した
						}
					}
				}
			}
		}

		//---------------------------------------------------------------------------------------------------------------------------
		// 変換を実行する

		private void Modify()
		{
			// 処理開始(TextureImporter 側の処理を無効化するため)
			IsProcessing = true ;

			int i, l ;
			TextureFile file ;

			List<TextureFile> targets = new List<TextureFile>() ;

			l = m_TextureFiles.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				file = m_TextureFiles[ i ] ;
				if( file.ReferencedCount >  0 && file.Enable == true )
				{
					// 参照のあるファイルの PNG 版を生成する
					targets.Add( file ) ;
				}
			}

			if( targets.Count >  0 )
			{
				l = targets.Count ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					// プログレスバーを表示  
					EditorUtility.DisplayProgressBar( "modifying ...", string.Format( "{0}/{1}", i + 1, l ), ( float )i / l ) ;

					ModifyTexture( targets[ i ] ) ;
				}	

				// プログレスバーを消す
				EditorUtility.ClearProgressBar() ;

				// 保存されていないアセットの変更点をすべてディスクに書き出します
				AssetDatabase.SaveAssets() ;

				// 何かしら変更があったアセットをすべてインポートします。
				AssetDatabase.Refresh() ;
			}

			// 処理終了
			IsProcessing = false ;
		}

		// テクスチャを変換する
		private void ModifyTexture( TextureFile file )
		{
			string oldPath = file.Path ;

			byte[] data ;

//			Debug.Log( "[処理対象]" + oldPath ) ;

			//----------------------------------------------------------
			// オリジナルのテクスチャを別の場所に保存する

			// そのままの場所だとインポート設定が実行されてしまい無劣化状態で PNG が生成できない

			string path = "Assets/TemporaryTexture" + Path.GetExtension( oldPath ) ;

			data = File.ReadAllBytes( oldPath ) ;
			File.WriteAllBytes( path, data ) ;

			// 一旦インポートを実行しないとインポーターが取得できない
			AssetDatabase.ImportAsset( path, ImportAssetOptions.ForceUpdate ) ;

			//----------------------------------

			// テンポラリテクスチャの設定を無劣化状態に強制的に書き換える

			TextureImporter textureImporter = AssetImporter.GetAtPath( path ) as TextureImporter ;

			textureImporter.textureType						= TextureImporterType.Default ;				//RGB
			textureImporter.isReadable						= true ;									// 読み出し可能
			textureImporter.mipmapEnabled					= false ;
			textureImporter.maxTextureSize					= 8192 ;									// 縮められないように 2048 最大で
			textureImporter.textureCompression				= TextureImporterCompression.Uncompressed ;	// 無圧縮
			textureImporter.compressionQuality				= 100 ;										// 無圧縮

			TextureImporterPlatformSettings ps ;

			// Standalone
			ps												= textureImporter.GetPlatformTextureSettings( "Standalone" ) ;
			ps.overridden									= true ;
			ps.maxTextureSize								= textureImporter.maxTextureSize;
			ps.format										= TextureImporterFormat.Automatic ;
			ps.textureCompression							= textureImporter.textureCompression ;
			textureImporter.SetPlatformTextureSettings( ps ) ;

			// Android
			ps												= textureImporter.GetPlatformTextureSettings( "Android" ) ;
			ps.overridden									= true ;
			ps.maxTextureSize								= textureImporter.maxTextureSize;
			ps.format										= TextureImporterFormat.Automatic ;
			ps.textureCompression							= textureImporter.textureCompression ;
			textureImporter.SetPlatformTextureSettings( ps ) ;

			// iOS
			ps												= textureImporter.GetPlatformTextureSettings( "iPhone" ) ;
			ps.overridden									= true ;
			ps.maxTextureSize								= textureImporter.maxTextureSize;
			ps.format										= TextureImporterFormat.Automatic ;
			ps.textureCompression							= textureImporter.textureCompression ;
			textureImporter.SetPlatformTextureSettings( ps ) ;

			// 設定を反映する
//			AssetDatabase.ImportAsset( path, ImportAssetOptions.ForceUpdate ) ;
			textureImporter.SaveAndReimport() ;

			//----------------------------------------------------------

			// 設定を変更した状態で読み出し

			Texture2D oldTexture = AssetDatabase.LoadAssetAtPath( path, typeof( Texture2D ) ) as Texture2D ;

//			Debug.Log( "サイズ W=" + texture.width + " H="+ texture.height ) ;

			// 一度時的にオリジナルのテクスチャを読み書き許可でロードする

			string extension = Path.GetExtension( oldPath ) ;
			string newPath = oldPath.Replace( extension, ".png" ) ;

//			oldTexture.Compress( false ) ;
			Texture2D newTexture = Decompress( oldTexture ) ;

			data = newTexture.EncodeToPNG() ;
			File.WriteAllBytes( newPath, data ) ;

			// 新規生成したファイルは必ず ImportAsset を実行する必要がある
			AssetDatabase.ImportAsset( newPath, ImportAssetOptions.ForceUpdate ) ;

			//----------------------------------------------------------

			// 不要ななった一時作成テクスチャを削除する
			File.Delete( path ) ;
			File.Delete( path + ".meta" ) ;

			//----------------------------------------------------------

			// 設定を引き継ぐ
			SetTextureSetting( oldPath, newPath ) ;

			file.NewInstance = AssetDatabase.LoadAssetAtPath( newPath, typeof( UnityEngine.Object ) ) ;

			//----------------------------------------------------------

			// 参照を変更する
			ChangeReference( file ) ;
		}

		// 複製テクスチャを生成する(圧縮状態だと複製できないので読み書きが有効な一時テクスチャを生成してテクセルをコピーする)
		private Texture2D Decompress( Texture2D sourceTexture )
		{
			//----------------------------------
			// RenderTexture に sourceTexture を描き込む
			
			RenderTexture renderTexture = RenderTexture.GetTemporary
			(
				sourceTexture.width,
				sourceTexture.height,
				0,
				RenderTextureFormat.Default,
				RenderTextureReadWrite.Linear
			) ;

			Graphics.Blit( sourceTexture, renderTexture ) ;

			//----------------------------------
			// RenderTexture を画面扱いにして、readableTexture に RenderTexture を描画する

			RenderTexture previous = RenderTexture.active ;
			RenderTexture.active = renderTexture ;

			Texture2D readableTexture = new Texture2D( sourceTexture.width, sourceTexture.height ) ;
			readableTexture.ReadPixels( new Rect( 0, 0, renderTexture.width, renderTexture.height ), 0, 0 ) ;
			readableTexture.Apply() ;

			RenderTexture.active = previous ;
			RenderTexture.ReleaseTemporary( renderTexture ) ;

			//----------------------------------

			return readableTexture ;
		}

		// テクスチャの属性を設定する（読み出し・書き込み）
		private void SetTextureSetting( string oldPath, string newPath )
		{
			TextureImporter oldTextureImporter = AssetImporter.GetAtPath( oldPath ) as TextureImporter ;
			TextureImporter newTextureImporter = AssetImporter.GetAtPath( newPath ) as TextureImporter ;

			//----------------------------------
			// 設定の上書き

			newTextureImporter.npotScale			= oldTextureImporter.npotScale ;

			newTextureImporter.textureType			= oldTextureImporter.textureType ;
			newTextureImporter.textureShape			= oldTextureImporter.textureShape ;

			newTextureImporter.spriteImportMode		= oldTextureImporter.spriteImportMode ;

			newTextureImporter.spriteBorder			= oldTextureImporter.spriteBorder ;
			newTextureImporter.spritePivot			= oldTextureImporter.spritePivot ;

			newTextureImporter.sRGBTexture			= oldTextureImporter.sRGBTexture ;
			newTextureImporter.alphaSource			= oldTextureImporter.alphaSource ;
			newTextureImporter.alphaIsTransparency	= oldTextureImporter.alphaIsTransparency ;

			newTextureImporter.isReadable			= oldTextureImporter.isReadable ;
			newTextureImporter.streamingMipmaps		= oldTextureImporter.streamingMipmaps ;
			newTextureImporter.mipmapEnabled		= oldTextureImporter.mipmapEnabled ;

			newTextureImporter.wrapMode				= oldTextureImporter.wrapMode ;
			newTextureImporter.filterMode			= oldTextureImporter.filterMode ;
			newTextureImporter.anisoLevel			= oldTextureImporter.anisoLevel ;

			newTextureImporter.maxTextureSize		= oldTextureImporter.maxTextureSize ;
			newTextureImporter.compressionQuality	= oldTextureImporter.compressionQuality ;
			newTextureImporter.textureCompression	= oldTextureImporter.textureCompression ;

			//----------------------------------------------------------

			TextureImporterPlatformSettings ops ;
			TextureImporterPlatformSettings nps ;

			//----------------------------------

			ops						= oldTextureImporter.GetPlatformTextureSettings( "Standalone" ) ;
			nps						= newTextureImporter.GetPlatformTextureSettings( "Standalone" ) ;
			nps.overridden			= ops.overridden ;
			nps.maxTextureSize		= ops.maxTextureSize ;
			nps.format				= ops.format ;
			nps.textureCompression	= ops.textureCompression ;
			nps.resizeAlgorithm		= nps.resizeAlgorithm ;
			newTextureImporter.SetPlatformTextureSettings( nps ) ;

			//----------------------------------

			ops						= oldTextureImporter.GetPlatformTextureSettings( "Android" ) ;
			nps						= newTextureImporter.GetPlatformTextureSettings( "Android" ) ;
			nps.overridden			= ops.overridden ;
			nps.maxTextureSize		= ops.maxTextureSize ;
			nps.format				= ops.format ;
			nps.textureCompression	= ops.textureCompression ;
			nps.resizeAlgorithm		= nps.resizeAlgorithm ;
			newTextureImporter.SetPlatformTextureSettings( nps ) ;

			//----------------------------------

			ops						= oldTextureImporter.GetPlatformTextureSettings( "iPhone" ) ;
			nps						= newTextureImporter.GetPlatformTextureSettings( "iPhone" ) ;
			nps.overridden			= ops.overridden ;
			nps.maxTextureSize		= ops.maxTextureSize ;
			nps.format				= ops.format ;
			nps.textureCompression	= ops.textureCompression ;
			nps.resizeAlgorithm		= nps.resizeAlgorithm ;
			newTextureImporter.SetPlatformTextureSettings( nps ) ;

			//----------------------------------

			// 改めて設定を反映する
//			AssetDatabase.ImportAsset( newPath, ImportAssetOptions.ForceUpdate ) ;
			newTextureImporter.SaveAndReimport() ;
		}

		private void ChangeReference( TextureFile file )
		{
			List<string> propertyMasks = new List<string>()
			{
				"PPtr<Texture>",
				"PPtr<$Sprite>",
			} ;

			foreach( var path in file.ReferencedAssetPaths )
			{
				ChangeReference( path, ref propertyMasks, file.Instance, file.NewInstance ) ;
			}
		}

		private void ChangeReference( string path, ref List<string> propertyMasks, UnityEngine.Object oldTexture, UnityEngine.Object newTexture )
		{
			UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath( path ) ;
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

				// プロパティのテクスチャ参照を追加する
				SetReferenceInProperty( asset, ref propertyMasks, oldTexture, newTexture ) ;
			}
		}

		// 該当のテクスチャを参照している箇所を追加する
		private void SetReferenceInProperty( UnityEngine.Object asset, ref List<string> propertyMasks, UnityEngine.Object oldTexture, UnityEngine.Object newTexture )
		{
			// SerializedObjectを通してアセットのプロパティを取得する  
			SerializedObject so = new SerializedObject( asset ) ;
			if( so != null )
			{
				SerializedProperty property = so.GetIterator() ;
				if( property != null )
				{
					while( true )
					{
						// プロパティの種類がオブジェクト（アセット）への参照で、  
						// その参照が null なのにもかかわらず、参照先インスタンス識別子が 0 でないものは Missing 状態！  
						if( property.propertyType == SerializedPropertyType.ObjectReference )
						{
							// オブジェクトを参照して箇所

							if( propertyMasks.Contains( property.type ) == true )
							{
								// 検査対象(Texture or Sprite)
								if( property.objectReferenceValue == oldTexture )
								{
									// 発見した
									property.objectReferenceValue  = newTexture ;
								}
							}
						}

						// 非表示プロパティも表示する
						if( property.Next( true ) == false )
						{
							break ;	// プロパティを全て検査した
						}
					}
				}

				// 変更を反映する
				so.ApplyModifiedProperties() ;
			}
		}

		//---------------------------------------------------------------------------------------------------------------------------
		// 削除を実行する

		private void Remove()
		{
			int i, l ;
			TextureFile file ;

			List<TextureFile> targets = new List<TextureFile>() ;

			l = m_TextureFiles.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				file = m_TextureFiles[ i ] ;
				if( file.ReferencedCount == 0 && file.Enable == true )
				{
					// 参照の無くなった画像を削除対象とする
					targets.Add( file ) ;
				}
			}

			if( targets.Count >  0 )
			{
				l = targets.Count ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					// プログレスバーを表示  
					EditorUtility.DisplayProgressBar( "removing ...", string.Format( "{0}/{1}", i + 1, l ), ( float )i / l ) ;

					RemoveTexture( targets[ i ] ) ;
				}	

				// プログレスバーを消す
				EditorUtility.ClearProgressBar() ;

				AssetDatabase.Refresh() ;
			}
		}

		private void RemoveTexture( TextureFile file )
		{
			string path = file.Path ;
			if( File.Exists( path ) == true )
			{
				File.Delete( path ) ;
			}

			path += ".meta" ;
			if( File.Exists( path ) == true )
			{
				File.Delete( path ) ;
			}
		}
	}
}
