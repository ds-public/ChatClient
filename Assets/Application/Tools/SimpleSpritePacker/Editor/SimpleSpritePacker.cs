using System ;
using System.IO ;
using System.Collections.Generic ;
using UnityEngine ;
using UnityEditor ;

/// <summary>
/// シンプルスプライトパッカーパッケージ
/// </summary>
namespace Tools.ForSprite
{
	/// <summary>
	/// スプライトパッカークラス(エディター用) Version 2022/06/06 0
	/// </summary>
	public class SimpleSpritePacker : EditorWindow
	{
		[ MenuItem( "Tools/Simple Sprite Packer(SpriteSet画像作成)" ) ]
		internal static void OpenWindow()
		{
			EditorWindow.GetWindow<SimpleSpritePacker>( false, "Sprite Packer", true ) ;
		}

		// 要素がどのように変化するか
		public enum SpriteAction
		{
			None,		// 要素維持
			Add,		// 要素追加
			Update,		// 要素更新
			Delete,		// 要素削除
		} ;

		// 要素の情報
		public class SpriteElement
		{
			public	Texture2D		Texture ;	// 追加または更新が行われ場合の別テクスチャファイルのインスタンス
			public	SpriteMetaData	MetaData ;
			public	SpriteAction	Action ;
			public	int				Type ;      // 要素となる画像がテクスチャ・シングルタイプスプライト(0)なのかマルチプルタイプスプライト(1)なのか

			public	TextureSettings	TextureSettings ;
//			public TextureImporterCompression	Format ;
//			public TextureImporterNPOTScale		NPOTscale ;
		} ;

		private string m_AtlasPath = "Assets/" ;
		private string m_AtlasName = "A New Atlas" ;
		private string m_AtlasFullPath = "" ;

		private Dictionary<string,SpriteElement> m_SpriteElementHash = new Dictionary<string,SpriteElement>() ;
	
		private int	m_Padding = 2 ;
		private int	m_MaxTextureSize = 2048 ;

		private readonly static string[] m_MaxTextureSizeLabels = new string[]{   "64",  "128",  "256",  "512", "1024", "2048", "4096", "8192" } ;
		private readonly static int[]    m_MaxTextureSizeValues = new    int[]{    64,    128,    256,    512,   1024,   2048,   4096,   8192  } ;

		private bool m_OutputSourceTexture = false ;
		private string m_OutputSourceTexturePath = "" ;

		private Vector2 m_ScrollPosition = Vector2.zero ;
	
		//----------------------------------------------------------

		// レイアウトを描画する
		internal void OnGUI()
		{
			Texture2D atlas = null ;
			bool isSetPath = false ;
			bool isAtlas = false ;

			// 保存先のパスの設定
			GUILayout.BeginHorizontal() ;
			{
				// 保存パスを選択する
				if( GUILayout.Button( "Atlas Path", GUILayout.Width( 80f ) ) == true )
				{
					if( Selection.objects != null && Selection.objects.Length == 0 && Selection.activeObject == null )
					{
						// ルート
						m_AtlasPath = "Assets/" ;
					}
					else
					if( Selection.objects != null && Selection.objects.Length == 1 && Selection.activeObject != null )
					{
						string path = AssetDatabase.GetAssetPath( Selection.activeObject.GetInstanceID() ) ;
						if( Directory.Exists( path ) == true )
						{
							// フォルダを指定しています
						
							// ファイルかどうか判別するには System.IO.File.Exists
						
							// 有効なフォルダ
							path = path.Replace( "\\", "/" ) ;
							m_AtlasPath = path + "/" ;
						}
						else
						{
							// ファイルを指定しています
							path = path.Replace( "\\", "/" ) ;
							m_AtlasPath = path ;

							// 拡張子を見てアセットバンドルであればファイル名まで置き変える
							// ただしこれを読み出して含まれるファイルの解析などは行わない
							// なぜなら違うプラットフォームの場合は読み出せずにエラーになってしまうから
						
							// 最後のフォルダ区切り位置を取得する
							int index = m_AtlasPath.LastIndexOf( '/' ) ;
							if( index >= 0 )
							{
								m_AtlasPath = m_AtlasPath.Substring( 0, index ) + "/" ;
							}

							if( path.Length >  4 )
							{
								if( path.Substring( path.Length - 4, 4 ) == ".png" )
								{
									path = path.Substring( index + 1, path.Length - ( index + 1 ) ) ;
									m_AtlasName = path.Substring(  0, path.Length - 4 ) ;

									isSetPath = true ;	// パスを設定した
								}
							}
						}
					}
				}
			
				// 保存パス
				m_AtlasPath = EditorGUILayout.TextField( m_AtlasPath ) ;
			
				// 名前
				m_AtlasName = EditorGUILayout.TextField( m_AtlasName ) ;
			}
			GUILayout.EndHorizontal() ;

			// 指定しているパスのファイルがアトラスならばインスタンスを取得する
			string atlasFullPath = m_AtlasPath + m_AtlasName + ".png" ;
			if( string.IsNullOrEmpty( atlasFullPath ) == false )
			{
				TextureImporter textureImporter = AssetImporter.GetAtPath( atlasFullPath ) as TextureImporter ;
				if( textureImporter != null && textureImporter.textureType == TextureImporterType.Sprite )
				{
					atlas = AssetDatabase.LoadAssetAtPath<Texture2D>( atlasFullPath ) ;
					isAtlas = ( textureImporter.spriteImportMode == SpriteImportMode.Multiple ) ;
				}
			}

			GUILayout.Space( 6f ) ;

			//-----------------------------------------------------
		
			// アトラステクスチャ情報の表示を行う
			if( atlas != null )
			{
				GUILayout.BeginHorizontal() ;
				{
					GUILayout.Label( "Path", GUILayout.Width( 36f ) ) ;
					GUI.color = Color.cyan ;
					GUILayout.Label( atlasFullPath ) ;
					GUI.color = Color.white ;
				}
				GUILayout.EndHorizontal() ;

				// アトラスが選択されている時のみ表示編集が可能となる
				GUILayout.BeginHorizontal() ;
				{
					if( GUILayout.Button( "Sprite", GUILayout.Width( 76f ) ) )
					{
						Selection.activeObject = atlas ;
					}
					GUILayout.Label( " " + atlas.width + "x" + atlas.height ) ;
				}
				GUILayout.EndHorizontal() ;

				if( isSetPath == true && isAtlas == true )
				{
					// 最大テクスチャサイズもアトラスのサイズに合わせる(ただしパスをセットした直後だけ)
					int w = atlas.width ;
					int h = atlas.height ;
					int size = w ;
					if( h >  w )
					{
						size = h ;
					}

					int i, l = m_MaxTextureSizeValues.Length ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						if( size <= m_MaxTextureSizeValues[ i ] )
						{
							break ;
						}
					}
					if( i >= l )
					{
						i  = l - 1 ;
					}
					m_MaxTextureSize = m_MaxTextureSizeValues[ i ] ;
				}
			}

			//------------------------------------------

			bool clear = false ;
			if( m_AtlasFullPath != atlasFullPath )
			{
				m_AtlasFullPath  = atlasFullPath ;
				clear  = true ;
			}

			// リストを更新する
			UpdateList( atlas, clear ) ;

			//-------------------------------------------------

			bool execute  = false ;

			if( m_SpriteElementHash.Count >  0 )
			{
				// マルチタイプのスプライトで要素が１以上存在する

				GUILayout.BeginHorizontal() ;	// 横一列開始
				{
					GUILayout.Label( "Padding", GUILayout.Width( 56f ) ) ;
					m_Padding = Mathf.Clamp( EditorGUILayout.IntField( m_Padding, GUILayout.Width( 50f ) ),  0, 10 ) ;

					GUILayout.Label( "", GUILayout.Width( 16f ) ) ;

					GUILayout.Label( "MaxTextureSize", GUILayout.Width( 104f ) ) ;
					m_MaxTextureSize = EditorGUILayout.IntPopup( m_MaxTextureSize, m_MaxTextureSizeLabels, m_MaxTextureSizeValues ) ;
				}
				GUILayout.EndHorizontal() ;		// 横一列終了


				GUILayout.BeginHorizontal() ;
				{
					if( atlas == null )
					{
						// 新規作成
						// 新規作成で生成可能
						GUI.backgroundColor = Color.green ;
						execute = GUILayout.Button( "Create" ) ;
						GUI.backgroundColor = Color.white ;
					}
					else
					{
						// 維持更新
						GUI.backgroundColor = Color.cyan ;
						execute = GUILayout.Button( "Update" ) ;		// 更新または追加
						GUI.backgroundColor = Color.white ;
					}
				}
				GUILayout.EndHorizontal() ;
				

				// テクスチャ分解出力
				if( atlas != null )
				{
					// テクスチャ分解出力パス
					GUILayout.BeginHorizontal() ;
					{
						EditorGUIUtility.labelWidth = 140f ;
						EditorGUIUtility.fieldWidth =  40f ;
						m_OutputSourceTexture = EditorGUILayout.Toggle( "Output Divided Sprite", m_OutputSourceTexture ) ;
						EditorGUIUtility.labelWidth =  80f ;
						EditorGUIUtility.fieldWidth =  50f ;
					}
					GUILayout.EndHorizontal() ;
			
					if( m_OutputSourceTexture == true )
					{
						// 保存先のパスの設定
						GUILayout.BeginHorizontal() ;
						{
							// 保存パスを選択する
							if( GUILayout.Button( "Output Path", GUILayout.Width( 100f ) ) == true )
							{
								if( Selection.objects != null && Selection.objects.Length == 0 && Selection.activeObject == null )
								{
									// ルート
									m_OutputSourceTexturePath = "Assets/" ;
								}
								else
								if( Selection.objects != null && Selection.objects.Length == 1 && Selection.activeObject != null )
								{
									string path = AssetDatabase.GetAssetPath( Selection.activeObject.GetInstanceID() ) ;
									if( Directory.Exists( path ) == true )
									{
										// フォルダを指定しています
								
										// ファイルかどうか判別するには System.IO.File.Exists
								
										// 有効なフォルダ
										path = path.Replace( "\\", "/" ) ;
										m_OutputSourceTexturePath = path + "/" ;
									}
									else
									{
										// ファイルを指定しています
										path = path.Replace( "\\", "/" ) ;
								
										// 拡張子を見てアセットバンドルであればファイル名まで置き変える
										// ただしこれを読み出して含まれるファイルの解析などは行わない
										// なぜなら違うプラットフォームの場合は読み出せずにエラーになってしまうから
								
										// 最後のフォルダ区切り位置を取得する
										int index = path.LastIndexOf( '/' ) ;
								
										m_OutputSourceTexturePath = path.Substring( 0, index ) + "/" ;
									}
								}
							}
					
							// 保存パス
							m_OutputSourceTexturePath = EditorGUILayout.TextField( m_OutputSourceTexturePath ) ;
					
							if( string.IsNullOrEmpty( m_OutputSourceTexturePath ) == false && atlas != null )
							{
								if( Directory.Exists( m_OutputSourceTexturePath ) == true )
								{
									GUI.backgroundColor = Color.red ;
									if( GUILayout.Button( "Execute", GUILayout.Width( 80f ) ) == true )
									{
										OutputDividedSprite( atlas, m_OutputSourceTexturePath ) ;
									}
									GUI.backgroundColor = Color.white ;
								}
							}
						}
						GUILayout.EndHorizontal() ;
					}
				}
			}

			//------------------------------------------
		
			// 素材情報のリストを表示する
			if( m_SpriteElementHash.Count >  0 )
			{
				EditorGUILayout.Separator() ;
			
				EditorGUILayout.LabelField( "Target (" + m_SpriteElementHash.Count + ")" ) ;

				GUILayout.BeginVertical() ;
				{
					m_ScrollPosition = GUILayout.BeginScrollView( m_ScrollPosition ) ;
					{
						foreach( KeyValuePair<string,SpriteElement> spriteElementKeyValue in m_SpriteElementHash )
						{
							string spriteName = spriteElementKeyValue.Key ;
							SpriteElement spriteElement = spriteElementKeyValue.Value ;
						
							GUILayout.BeginHorizontal( "TextArea", GUILayout.MinHeight( 20f ) ) ;	// 横一列開始
							{
								GUILayout.Label( spriteName, GUILayout.Height( 20f ) ) ;
							
								switch( spriteElement.Action )
								{
									// 通常状態
									case SpriteAction.None :
										if( GUILayout.Button( "Delete", GUILayout.Width( 60f ) ) )
										{
											spriteElement.Action = SpriteAction.Delete ;	// 破棄対象にする
										}
									break ;

									// 追加対象
									case SpriteAction.Add :
										GUI.backgroundColor = Color.green ;
										GUILayout.Box( "Add", GUILayout.Width( 60f ) ) ;
										GUI.backgroundColor = Color.white ;
									break ;

									// 更新対象
									case SpriteAction.Update :
										GUI.backgroundColor = Color.yellow ;
										GUILayout.Box( "Update", GUILayout.Width( 60f ) ) ;
										GUI.backgroundColor = Color.white ;
									break ;

									// 削除対象
									case SpriteAction.Delete :
										GUI.backgroundColor = Color.red ;
										if( GUILayout.Button( "Delete", GUILayout.Width( 60f ) ) )
										{
											spriteElement.Action = SpriteAction.None ;
										}
										GUI.backgroundColor = Color.white ;
									break ;
								}
							}
							GUILayout.EndHorizontal() ;		// 横一列終了
						}
					}
					GUILayout.EndScrollView() ;
				}
				GUILayout.EndVertical() ;
			}
			else
			{
				EditorGUILayout.HelpBox( GetMessage( "SelectTexture" ), MessageType.Info ) ;
			}

			//-------------------------------------------

			if( execute == true )
			{
				// アトラスを生成する
				BuildAtlas( atlas ) ;
			}
		}

		internal void OnSelectionChange() 
		{
			Repaint() ;
		}
	
		//---------------------------------------------------------------

		// 素材リストの情報を生成する
		private void UpdateList( Texture2D atlas, bool clear )
		{
			if( clear == true )
			{
				m_SpriteElementHash.Clear() ;
			}
		
			Dictionary<string,SpriteElement> spriteElementHash = new Dictionary<string,SpriteElement>() ;

			if( atlas != null )
			{
				// 既にある場合はアトラス内のスプライト情報を展開する
				string atlasPath = AssetDatabase.GetAssetPath( atlas.GetInstanceID() ) ;
			
				TextureImporter textureImporter = AssetImporter.GetAtPath( atlasPath ) as TextureImporter ;
			
				foreach( SpriteMetaData spriteMetaData in textureImporter.spritesheet )
				{
					string spriteName = spriteMetaData.name ;

					if( m_SpriteElementHash.TryGetValue( spriteName, out SpriteElement spriteElement ) )
					{
						// 既にリストに登録済みの情報
						spriteElement.Texture = null ;

						spriteElement.MetaData = spriteMetaData ;

						if( spriteElement.Action == SpriteAction.Delete )
						{
							spriteElement.Action  = SpriteAction.Delete ;
						}
						else
						{
							spriteElement.Action  = SpriteAction.None ;
						}
						spriteElement.Type = 0 ;
					}
					else
					{
						// リストに存在しない情報
						spriteElement = new SpriteElement()
						{
							Texture = null,
							MetaData = spriteMetaData,
							Action = SpriteAction.None,
							Type = 0
						} ;
					}

					spriteElementHash.Add( spriteName, spriteElement ) ;
				}
			}

			// 選択中の素材を追加する
			foreach( UnityEngine.Object target in Selection.objects )
			{
				Texture2D texture = target as Texture2D ;
			
				if( texture == null || texture == atlas )
				{
					continue ;
				}

				//-----------------------------------------

				SpriteElement spriteElement = null ;
			
				string spriteName ;

				// 素材となる画像がスプライト（アトラス）かそれ以外（テクスチャ）かで処理が異なる
				string texturePath = AssetDatabase.GetAssetPath( texture.GetInstanceID() ) ;
				TextureImporter textureImporter = AssetImporter.GetAtPath( texturePath ) as TextureImporter ;
				if( textureImporter != null && textureImporter.textureType == TextureImporterType.Sprite )
				{
					// スプライト扱い
					if( textureImporter.spriteImportMode == SpriteImportMode.Single )
					{
						// シングルタイプ
						spriteName = texture.name ;

						if( spriteElementHash.TryGetValue( spriteName, out spriteElement ) )
						{
							// 既に存在するのでアップデートになる
							spriteElement.Texture = texture ;
						
							spriteElement.MetaData.border	= textureImporter.spriteBorder ;
							spriteElement.MetaData.pivot	= textureImporter.spritePivot ;
						
							spriteElement.Action = SpriteAction.Update ;
							spriteElement.Type = 0 ;
						}
						else
						{
							// 存在しないため追加となる
							spriteElement = new SpriteElement()
							{
								Texture = texture
							} ;
						
							spriteElement.MetaData = new SpriteMetaData()
							{
								name	= spriteName,
								border	= textureImporter.spriteBorder,
								pivot	= textureImporter.spritePivot
							} ;

							spriteElement.Action = SpriteAction.Add ;
							spriteElement.Type = 0 ;

							spriteElementHash.Add( spriteName, spriteElement ) ;
						}
					}
					else
					if( textureImporter.spriteImportMode == SpriteImportMode.Multiple )
					{
						// マルチプルタイプ
						foreach( SpriteMetaData spriteMetaData in textureImporter.spritesheet )
						{
							spriteName = spriteMetaData.name ;
						
							if( spriteElementHash.TryGetValue( spriteName, out spriteElement ) )
							{
								// 既に存在するのでアップデートになる
								spriteElement.Texture = texture ;
							
								spriteElement.MetaData = spriteMetaData ;

								spriteElement.Action = SpriteAction.Update ;
								spriteElement.Type = 1 ;
							}
							else
							{
								// 存在しないため追加となる
								spriteElement = new SpriteElement()
								{
									Texture = texture
								} ;
							
								spriteElement.MetaData = spriteMetaData ;

								spriteElement.Action = SpriteAction.Add ;
								spriteElement.Type = 1 ;

								spriteElementHash.Add( spriteName, spriteElement ) ;
							}
						}
					}
				}
				else
				{
					// テクスチャ扱い
					spriteName = texture.name ;
				
					if( spriteElementHash.TryGetValue( spriteName, out spriteElement ) )
					{
						// 既に存在するのでアップデートになる
						spriteElement.Texture = texture ;
					
						spriteElement.Action = SpriteAction.Update ;
						spriteElement.Type = 0 ;
					}
					else
					{
						// 存在しないため追加となる
						spriteElement = new SpriteElement()
						{
							Texture = texture
						} ;
						
						spriteElement.MetaData = new SpriteMetaData()
						{
							name = spriteName
						} ;
						
						spriteElement.Action = SpriteAction.Add ;
						spriteElement.Type = 0 ;

						spriteElementHash.Add( spriteName, spriteElement ) ;
					}
				}
			}

			m_SpriteElementHash = spriteElementHash ;
		}

		// アトラステクスチャの更新を実行する
		private void BuildAtlas( Texture2D atlas )
		{
			Texture2D texture = new Texture2D( 1, 1, TextureFormat.ARGB32, false ) ;
		
			List<SpriteMetaData> spriteSheetList = new List<SpriteMetaData>() ;
			List<Texture2D> spriteTextureList = new List<Texture2D>() ;
			List<string> deleteSpriteList = new List<string>() ;
		
			string path ;

			// 元の状態を保存する(バックアップが必要な項目)
			TextureSettings	atlasSettings = null ;

			if( atlas != null )
			{
				// 既に作成済みのアトラスの更新
				path = AssetDatabase.GetAssetPath( atlas.GetInstanceID() ) ;
			
				atlasSettings = GetOrSetTextureSettings( path, null ) ;	// 書き込み属性を有効にする
			
				atlas = AssetDatabase.LoadAssetAtPath( path, typeof( Texture2D ) ) as Texture2D ;
			}

			int pn = 0 ;
			int pm = m_SpriteElementHash.Count ;

			foreach( KeyValuePair<string,SpriteElement> spriteElementKeyValue in m_SpriteElementHash )
			{
				// プログレスバーを表示
				EditorUtility.DisplayProgressBar
				(
					"Loading ...",
					string.Format( "{0}/{1}", pn + 1, pm ),
					( float )( pn + 1 ) / ( float )pm
				) ;
				pn ++ ;

				//---------------------------------------------------------

				string spriteName = spriteElementKeyValue.Key ;
				SpriteElement spriteElement = spriteElementKeyValue.Value ;

				Texture2D elementTexture ;
				int x, y, w, h ;

				switch( spriteElement.Action )
				{
					// アトラススプライトに内包される領域
					case SpriteAction.None :
						spriteSheetList.Add( spriteElement.MetaData ) ;
					
						x = ( int )spriteElement.MetaData.rect.x ;
						y = ( int )spriteElement.MetaData.rect.y ;
						w = ( int )spriteElement.MetaData.rect.width ;
						h = ( int )spriteElement.MetaData.rect.height ;
					
						elementTexture = new Texture2D( w, h, TextureFormat.ARGB32, false ) ;
						elementTexture.SetPixels( atlas.GetPixels( x, y, w, h ) ) ;
						elementTexture.Apply() ;

						spriteTextureList.Add( elementTexture ) ;
					break ;

					// 新規追加
					case SpriteAction.Add :
						spriteSheetList.Add( spriteElement.MetaData ) ;
					
						path = AssetDatabase.GetAssetPath( spriteElement.Texture.GetInstanceID() ) ;
						
						// 読み込む前にフォーマットを強制的に ARGB32 NPOT にしてやる(アルファテクスチャなどだとオリジナルの状態が正しく読み込めない)
						spriteElement.TextureSettings = GetOrSetTextureSettings( path, null ) ;
					
						if( spriteElement.Type == 0 )
						{
							// テクスチャまたはシングルスプライトタイプ
							spriteElement.Texture = AssetDatabase.LoadAssetAtPath( path, typeof( Texture2D ) ) as Texture2D ;
							spriteTextureList.Add( spriteElement.Texture ) ;
						}
						else
						{
							// マルチプルスプライトタイプ
							x = ( int )spriteElement.MetaData.rect.x ;
							y = ( int )spriteElement.MetaData.rect.y ;
							w = ( int )spriteElement.MetaData.rect.width ;
							h = ( int )spriteElement.MetaData.rect.height ;
						
							elementTexture = new Texture2D( w, h, TextureFormat.ARGB32, false ) ;
							elementTexture.SetPixels( spriteElement.Texture.GetPixels( x, y, w, h ) ) ;
							elementTexture.Apply() ;
					
							spriteTextureList.Add( elementTexture ) ;
						}
					break ;

					// 領域更新
					case SpriteAction.Update :
						spriteSheetList.Add( spriteElement.MetaData ) ;
				
						path = AssetDatabase.GetAssetPath( spriteElement.Texture.GetInstanceID() ) ;

						// 読み込む前にフォーマットを強制的に ARGB32 NPOT にしてやる(アルファテクスチャなどだとオリジナルの状態が正しく読み込めない)
						spriteElement.TextureSettings = GetOrSetTextureSettings( path, null ) ;
					
						if( spriteElement.Type == 0 )
						{
							// テクスチャまたはシングルスプライトタイプ
							spriteElement.Texture = AssetDatabase.LoadAssetAtPath( path, typeof( Texture2D ) ) as Texture2D ;
							spriteTextureList.Add( spriteElement.Texture ) ;
						}
						else
						{
							// マルチプルスプライトタイプ
							x = ( int )spriteElement.MetaData.rect.x ;
							y = ( int )spriteElement.MetaData.rect.y ;
							w = ( int )spriteElement.MetaData.rect.width ;
							h = ( int )spriteElement.MetaData.rect.height ;
						
							elementTexture = new Texture2D( w, h, TextureFormat.ARGB32, false ) ;
							elementTexture.SetPixels( spriteElement.Texture.GetPixels( x, y, w, h ) ) ;
							elementTexture.Apply() ;
						
							spriteTextureList.Add( elementTexture ) ;
						}
					break ;

					case SpriteAction.Delete :
						deleteSpriteList.Add( spriteName ) ;
					break ;
				}

				elementTexture = null ;
			}

			// プログレスバーを消す
			EditorUtility.ClearProgressBar() ;

			//--------------------------------------------------------------------------

			if( spriteSheetList.Count >  0 && spriteTextureList.Count >  0 )
			{
				int maxSize = Mathf.Min( SystemInfo.maxTextureSize, m_MaxTextureSize ) ;

				int padding = m_Padding ;
				if( spriteTextureList.Count == 1 )
				{
					// 要素が１つだけの場合はパディングは強制的に０にする
					padding = 0 ;
				}

				Rect[] rectList = texture.PackTextures( spriteTextureList.ToArray(), padding, maxSize ) ;

				pn = 0 ;
				pm = m_SpriteElementHash.Count ;

				// 後始末
				foreach( KeyValuePair<string,SpriteElement> spriteElementKeyValue in m_SpriteElementHash )
				{
					// プログレスバーを表示
					EditorUtility.DisplayProgressBar
					(
						"Restoring ...",
						string.Format( "{0}/{1}", pn + 1, pm ),
						( float )( pn + 1 ) / ( float )pm
					) ;
					pn ++ ;

					SpriteElement spriteElement = spriteElementKeyValue.Value ;
				
					switch( spriteElement.Action )
					{
						// 新規追加の後始末
						case SpriteAction.Add :
							path = AssetDatabase.GetAssetPath( spriteElement.Texture.GetInstanceID() ) ;
							GetOrSetTextureSettings( path, spriteElement.TextureSettings ) ;	// 設定を元に戻す
						
							spriteElement.Texture = null ;
						
							spriteElement.Action = SpriteAction.None ;
						break ;

						// 領域更新の後始末
						case SpriteAction.Update :
							path = AssetDatabase.GetAssetPath( spriteElement.Texture.GetInstanceID() ) ;
							GetOrSetTextureSettings( path, spriteElement.TextureSettings ) ;
						
							spriteElement.Texture = null ;
						
							spriteElement.Action = SpriteAction.None ;
						break ;
					}
				}

				// プログレスバーを消す
				EditorUtility.ClearProgressBar() ;

				//---------------------------------------------------

				// 出来上がったテクスチャに無駄が無いか確認し無駄があれば除去する

				int i, l ;

				float tw = texture.width ;
				float th = texture.height ;

				l = spriteSheetList.Count ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					Rect rect = rectList[ i ] ;
					rect.x      *= tw ;
					rect.y      *= th ;
					rect.width  *= tw ;
					rect.height *= th ;
					rectList[ i ] = rect ;
				}

				// ソースのリージョンを検査して無駄が無いか確認する
				float xMin = tw ;
				float yMin = th ;
				float xMax = 0 ;
				float yMax = 0 ;
			
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( rectList[ i ].xMin <  xMin )
					{
						xMin  = rectList[ i ].xMin ;
					}
				
					if( rectList[ i ].yMin <  yMin )
					{
						yMin  = rectList[ i ].yMin ;
					}
				
					if( rectList[ i ].xMax >  xMax )
					{
						xMax  = rectList[ i ].xMax ;
					}
				
					if( rectList[ i ].yMax >  yMax )
					{
						yMax  = rectList[ i ].yMax ;
					}
				}

				xMin -= padding ;
				if( xMin <  0 )
				{
					xMin  = 0 ;
				}
				yMin -= padding ;
				if( yMin <  0 )
				{
					yMin  = 0 ;
				}
				xMax += padding ;
				if( xMax >  tw )
				{
					xMax  = tw ;
				}
				yMax += padding ;
				if( yMax >  th )
				{
					yMax  = th ;
				}
				int pw = ( int )( xMax - xMin ) ;
				int ph = ( int )( yMax - yMin ) ;

				int rw = 1 ;
				for( i  =  0 ; i <  16 ; i ++ )
				{
					if( rw >= pw )
					{
						break ;
					}
					rw <<= 1 ;
				}

				int rh = 1 ;
				for( i  =  0 ; i <  16 ; i ++ )
				{
					if( rh >= ph )
					{
						break ;
					}
					rh <<= 1 ;
				}

				if( rw <= ( int )( tw / 2 ) || rh <= ( int )( th / 2 ) )
				{
					// 無駄がある

	//				Debug.LogWarning( "無駄がある:" + rw + " / " + tw + " " + rh + " / " + th ) ;

					int rx = ( int )xMin ;
					int ry = ( int )yMin ;

	//				Debug.LogWarning( "取得範囲:" + rx + " " + ry + " " + rw + " " + rh ) ;

					Texture2D reduceTexture = new Texture2D( rw, rh, TextureFormat.ARGB32, false ) ;
					reduceTexture.SetPixels( texture.GetPixels( rx, ry, rw, rh ) ) ;
					reduceTexture.Apply() ;
				
					texture = reduceTexture ;

					for( i  = 0 ; i <  l ; i ++ )
					{
						Rect rect = rectList[ i ] ;
						rect.x      -= rx ;
						rect.y      -= ry ;
						rectList[ i ] = rect ;
					}
				}
			
				//---------------------------------------------------

				string atlasFullPath ;
				if( atlas == null )
				{
					// 生成
					atlasFullPath = m_AtlasFullPath ;
				}
				else
				{
					// 更新
					atlasFullPath = AssetDatabase.GetAssetPath( atlas.GetInstanceID() ) ;
				}

				// テクスチャをＰＮＧ画像として保存する
				// 圧縮状態だと保存出来ないので一旦無圧縮フォーマットにする
				texture.Compress( false ) ;
				texture = Decompress( texture ) ;

				byte[] data = texture.EncodeToPNG() ;
				File.WriteAllBytes( atlasFullPath, data ) ;
			
				AssetDatabase.SaveAssets() ;
				if( atlas == null )
				{
					AssetDatabase.Refresh() ;
				}
			
				TextureImporter textureImporter = AssetImporter.GetAtPath( atlasFullPath ) as TextureImporter ;

				// 既存の場合は以前の設定を引き継がせる
				if( atlas == null )
				{
					// 新規作成の場合に設定する
					atlasSettings = new TextureSettings() ;	// デフォルトはスプライト用の設定

					ApplyTextureSettings( textureImporter, atlasSettings ) ;

					textureImporter.spriteImportMode	= SpriteImportMode.Multiple ;
				}
				else
				{
					// 既存更新の場合は反映する
					ApplyTextureSettings( textureImporter, atlasSettings ) ;
				}
			
				for( i  = 0 ; i <  l ; i ++ )
				{
					SpriteMetaData spriteMetaData = spriteSheetList[ i ] ;

					spriteMetaData.rect = rectList[ i ] ;

					spriteSheetList[ i ] = spriteMetaData ;
				}
			
				textureImporter.spritesheet = spriteSheetList.ToArray() ;
			
				textureImporter.SaveAndReimport() ;
			
				atlas = AssetDatabase.LoadAssetAtPath( atlasFullPath, typeof( Texture2D ) ) as Texture2D ;

				spriteSheetList.Clear() ;
				spriteTextureList.Clear() ;

				Resources.UnloadUnusedAssets() ;
			}
			else
			{
				// 要素が全て無くなるためファイル自体を削除する
				AssetDatabase.DeleteAsset( m_AtlasFullPath ) ;
				atlas = null ;
			}

			AssetDatabase.SaveAssets() ;
			AssetDatabase.Refresh() ;
		
			foreach( string spriteName in deleteSpriteList )
			{
				m_SpriteElementHash.Remove( spriteName ) ;
			}
			UpdateList( atlas, true ) ;
		}

		private Texture2D Decompress( Texture2D sourceTexture )
		{
			RenderTexture renderTexture = RenderTexture.GetTemporary
			(
				sourceTexture.width,
				sourceTexture.height,
				0,
				RenderTextureFormat.Default,
				RenderTextureReadWrite.Linear
			) ;

			Graphics.Blit( sourceTexture, renderTexture ) ;

			RenderTexture previous = RenderTexture.active ;
			RenderTexture.active = renderTexture ;

			Texture2D readableTexture = new Texture2D( sourceTexture.width, sourceTexture.height ) ;
			readableTexture.ReadPixels( new Rect( 0, 0, renderTexture.width, renderTexture.height ), 0, 0 ) ;
			readableTexture.Apply() ;

			RenderTexture.active = previous ;
			RenderTexture.ReleaseTemporary( renderTexture ) ;

			return readableTexture ;
		}

		// 個々の要素をシングルタイプスプライトして書き出す
		private void OutputDividedSprite( Texture2D atlas, string folderPath )
		{
			if( string.IsNullOrEmpty( folderPath ) == true )
			{
				return ;
			}

			if( Directory.Exists( folderPath ) == false )
			{
				return ;
			}
			
			int l = folderPath.Length ;
			if( folderPath[ l - 1 ] != '/' )
			{
				folderPath += '/' ;
			}

			//----------------------------------------------------------

			// アトラススプライトを読み出し可能状態にする
			string atlasPath = AssetDatabase.GetAssetPath( atlas.GetInstanceID() ) ;

			// 元の状態を保存する
			TextureSettings atlasSettings ;

			atlasSettings =	GetOrSetTextureSettings( atlasPath, null ) ;	// 読み込み属性を有効にする
		
			atlas = AssetDatabase.LoadAssetAtPath( atlasPath, typeof( Texture2D ) ) as Texture2D ;

			TextureImporter atlasTextureImporter = AssetImporter.GetAtPath( atlasPath ) as TextureImporter ;
			int x, y, w, h ;

			Texture2D elementTexture ;
			string elementPath ;
			TextureImporter elementTextureImporter ;

//			TextureSettings elementSettings = new TextureSettings() ;

			try
			{
//				EditorApplication.LockReloadAssemblies() ;
//				AssetDatabase.StartAssetEditing() ;

				int pn = 0 ;
				int pm = atlasTextureImporter.spritesheet.Length ;

				foreach( SpriteMetaData spriteMetaData in atlasTextureImporter.spritesheet )
				{
					// プログレスバーを表示
					EditorUtility.DisplayProgressBar
					(
						"Saving ...",
						string.Format( "{0}/{1}", pn + 1, pm ),
						( float )( pn + 1 ) / ( float )pm
					) ;
					pn ++ ;

					x = ( int )spriteMetaData.rect.x ;
					y = ( int )spriteMetaData.rect.y ;
					w = ( int )spriteMetaData.rect.width ;
					h = ( int )spriteMetaData.rect.height ;
			
					elementTexture = new Texture2D( w, h, TextureFormat.ARGB32, false ) ;
					elementTexture.SetPixels( atlas.GetPixels( x, y, w, h ) ) ;
					elementTexture.Apply() ;

					elementPath = folderPath + spriteMetaData.name + ".png" ;

					// テクスチャをＰＮＧ画像として保存する
					byte[] data = elementTexture.EncodeToPNG() ;
					File.WriteAllBytes( elementPath, data ) ;
					data = null ;

					AssetDatabase.SaveAssets() ;
					AssetDatabase.Refresh() ;

					// 新規生成

					elementTextureImporter = AssetImporter.GetAtPath( elementPath ) as TextureImporter ;

					ApplyTextureSettings( elementTextureImporter, atlasSettings ) ;

					elementTextureImporter.spriteImportMode		= SpriteImportMode.Single ;

					elementTextureImporter.spriteBorder			= spriteMetaData.border ;
					elementTextureImporter.spritePivot			= spriteMetaData.pivot ;

					elementTextureImporter.SaveAndReimport() ;

					elementTexture = null ;
				}
			}
			finally
			{
				// プログレスバーを消す
				EditorUtility.ClearProgressBar() ;

//				AssetDatabase.StopAssetEditing() ;
//				EditorApplication.UnlockReloadAssemblies() ;
			}

			// 後始末
			GetOrSetTextureSettings( atlasPath, atlasSettings ) ;	// 読み込み属性を無効にする

			AssetDatabase.SaveAssets() ;
			AssetDatabase.Refresh() ;
		}

		//------------------------------------------------------------

		/// <summary>
		/// テクスチャの設定
		/// </summary>
		public class TextureSettings
		{
			// General
			public	TextureImporterType				TextureType				= TextureImporterType.Sprite ;
			public	TextureImporterNPOTScale		NPOTScale				= TextureImporterNPOTScale.None ;
			public	bool							IsReadable				= false ;
			public	bool							MipmapEnabled			= false ;
			public	bool							AlphaIsTransparency		= true ;
			public	TextureWrapMode					WrapMode				= TextureWrapMode.Clamp ;
			public	FilterMode						FilterMode				= FilterMode.Bilinear ;

			// Default
			public class DefaultSettings
			{
				public	int							MaxTextureSize		= 2048 ;
				public	TextureImporterCompression	TextureCompression	= TextureImporterCompression.Compressed ;
				public	int							CompressionQuality	= 100 ;
			}
			public	DefaultSettings					Default		= new DefaultSettings() ;

			public class PlatformSettings
			{
				public	bool						Overridden			= false ;
				public	int							MaxTextureSize		= 2048 ;
				public	TextureImporterFormat		Format				= TextureImporterFormat.Automatic ;
				public	TextureImporterCompression	TextureCompression	= TextureImporterCompression.Compressed ;
			}

			// Standalone
			public	PlatformSettings				Standalone	= new PlatformSettings() ;

			// Android
			public	PlatformSettings				Android		= new PlatformSettings() ;

			// iOS
			public	PlatformSettings				iOS			= new PlatformSettings() ;
		}

		// テクスチャの設定を取得または設定する
		private TextureSettings GetOrSetTextureSettings( string path, TextureSettings textureSettings )
		{
			TextureImporter textureImporter = AssetImporter.GetAtPath( path ) as TextureImporter ;

			TextureImporterPlatformSettings ps ;

			if( textureSettings == null )
			{
				// 取得しつつ無圧縮状態にする

				textureSettings = new TextureSettings() ;

				//---------------------------------------------------------
				// 取得

				// General
				textureSettings.TextureType						= textureImporter.textureType ;
				textureSettings.NPOTScale						= textureImporter.npotScale ;
				textureSettings.IsReadable						= textureImporter.isReadable ;
				textureSettings.MipmapEnabled					= textureImporter.mipmapEnabled ;
				textureSettings.AlphaIsTransparency				= textureImporter.alphaIsTransparency ;
				textureSettings.WrapMode						= textureImporter.wrapMode ;
				textureSettings.FilterMode						= textureImporter.filterMode ;

				// Default
				textureSettings.Default.MaxTextureSize			= textureImporter.maxTextureSize ;
				textureSettings.Default.TextureCompression		= textureImporter.textureCompression ;
				textureSettings.Default.CompressionQuality		= textureImporter.compressionQuality ;

				// Standalone
				ps												= textureImporter.GetPlatformTextureSettings( "Standalone" ) ;
				textureSettings.Standalone.Overridden			= ps.overridden ;
				textureSettings.Standalone.MaxTextureSize		= ps.maxTextureSize ;
				textureSettings.Standalone.Format				= ps.format ;
				textureSettings.Standalone.TextureCompression	= ps.textureCompression ;

				// Android
				ps												= textureImporter.GetPlatformTextureSettings( "Android" ) ;
				textureSettings.Android.Overridden				= ps.overridden ;
				textureSettings.Android.MaxTextureSize			= ps.maxTextureSize ;
				textureSettings.Android.Format					= ps.format ;
				textureSettings.Android.TextureCompression		= ps.textureCompression ;

				// iOS
				ps												= textureImporter.GetPlatformTextureSettings( "iPhone" ) ;
				textureSettings.iOS.Overridden					= ps.overridden ;
				textureSettings.iOS.MaxTextureSize				= ps.maxTextureSize ;
				textureSettings.iOS.Format						= ps.format ;
				textureSettings.iOS.TextureCompression			= ps.textureCompression ;

				//---------------------------------------------------------
				// 無圧縮

				// General
				textureImporter.textureType						= TextureImporterType.Sprite ;				//RGB
				textureImporter.npotScale						= TextureImporterNPOTScale.None ;				//RGB
				textureImporter.isReadable						= true ;									// 読み出し可能
				textureImporter.mipmapEnabled					= false ;
				textureSettings.AlphaIsTransparency				= true ;
				textureSettings.WrapMode						= TextureWrapMode.Clamp ;
				textureSettings.FilterMode						= FilterMode.Point ;

				// Default
				textureImporter.maxTextureSize					= 8192 ;									// 縮められないように 2048 最大で
				textureImporter.textureCompression				= TextureImporterCompression.Uncompressed ;	// 無圧縮
				textureImporter.compressionQuality				= 100 ;										// 無圧縮

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

				//---------------------------------------------------------
				// 反映
				
				textureImporter.SaveAndReimport() ;
			}
			else
			{
				// 設定

				// General
				textureImporter.textureType						= textureSettings.TextureType ;
				textureImporter.npotScale						= textureSettings.NPOTScale ;
				textureImporter.isReadable						= textureSettings.IsReadable ;
				textureImporter.mipmapEnabled					= textureSettings.MipmapEnabled ;
				textureImporter.alphaIsTransparency				= textureSettings.AlphaIsTransparency ;
				textureImporter.wrapMode						= textureSettings.WrapMode ;
				textureImporter.filterMode						= textureSettings.FilterMode ;

				// Default
				textureImporter.maxTextureSize					= textureSettings.Default.MaxTextureSize ;
				textureImporter.textureCompression				= textureSettings.Default.TextureCompression ;
				textureImporter.compressionQuality				= textureSettings.Default.CompressionQuality ;

				// Standalone
				ps												= textureImporter.GetPlatformTextureSettings( "Standalone" ) ;
				ps.overridden									= textureSettings.Standalone.Overridden ;
				ps.maxTextureSize								= textureSettings.Standalone.MaxTextureSize ;
				ps.format										= textureSettings.Standalone.Format ;
				ps.textureCompression							= textureSettings.Standalone.TextureCompression ;
				textureImporter.SetPlatformTextureSettings( ps ) ;

				// Android
				ps												= textureImporter.GetPlatformTextureSettings( "Android" ) ;
				ps.overridden									= textureSettings.Android.Overridden ;
				ps.maxTextureSize								= textureSettings.Android.MaxTextureSize ;
				ps.format										= textureSettings.Android.Format ;
				ps.textureCompression							= textureSettings.Android.TextureCompression ;
				textureImporter.SetPlatformTextureSettings( ps ) ;

				// iOS
				ps												= textureImporter.GetPlatformTextureSettings( "iPhone" ) ;
				ps.overridden									= textureSettings.iOS.Overridden ;
				ps.maxTextureSize								= textureSettings.iOS.MaxTextureSize ;
				ps.format										= textureSettings.iOS.Format ;
				ps.textureCompression							= textureSettings.iOS.TextureCompression ;
				textureImporter.SetPlatformTextureSettings( ps ) ;

				//---------------------------------------------------------
				// 反映
				
				textureImporter.SaveAndReimport() ;
			}

			return textureSettings ;
		}

		// インポーターに設定を反映させる
		private void ApplyTextureSettings( TextureImporter textureImporter, TextureSettings textureSettings )
		{
			TextureImporterPlatformSettings ps ;

			// General
			textureImporter.textureType						= textureSettings.TextureType ;
			textureImporter.npotScale						= textureSettings.NPOTScale ;
			textureImporter.isReadable						= textureSettings.IsReadable ;
			textureImporter.mipmapEnabled					= textureSettings.MipmapEnabled ;

			// Default
			textureImporter.maxTextureSize					= textureSettings.Default.MaxTextureSize ;
			textureImporter.textureCompression				= textureSettings.Default.TextureCompression ;
			textureImporter.compressionQuality				= textureSettings.Default.CompressionQuality ;

			// Standalone
			ps												= textureImporter.GetPlatformTextureSettings( "Standalone" ) ;
			ps.overridden									= textureSettings.Standalone.Overridden ;
			ps.maxTextureSize								= textureSettings.Standalone.MaxTextureSize ;
			ps.format										= textureSettings.Standalone.Format ;
			ps.textureCompression							= textureSettings.Standalone.TextureCompression ;
			textureImporter.SetPlatformTextureSettings( ps ) ;

			// Android
			ps												= textureImporter.GetPlatformTextureSettings( "Android" ) ;
			ps.overridden									= textureSettings.Android.Overridden ;
			ps.maxTextureSize								= textureSettings.Android.MaxTextureSize ;
			ps.format										= textureSettings.Android.Format ;
			ps.textureCompression							= textureSettings.Android.TextureCompression ;
			textureImporter.SetPlatformTextureSettings( ps ) ;

			// iOS
			ps												= textureImporter.GetPlatformTextureSettings( "iPhone" ) ;
			ps.overridden									= textureSettings.iOS.Overridden ;
			ps.maxTextureSize								= textureSettings.iOS.MaxTextureSize ;
			ps.format										= textureSettings.iOS.Format ;
			ps.textureCompression							= textureSettings.iOS.TextureCompression ;
			textureImporter.SetPlatformTextureSettings( ps ) ;
		}

		//--------------------------------------------------------------------------

		private readonly Dictionary<string,string> m_Japanese_Message = new Dictionary<string, string>()
		{
			{ "SelectTexture",   "パック対象にしたいテクスチャを選択してください(複数可)" },
		} ;
		private readonly Dictionary<string,string> m_English_Message = new Dictionary<string, string>()
		{
			{ "SelectTexture",   "Please select one or more textures in the Project View window." },
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
					return "Specifying the label name can not be found" ;
				}
				return m_English_Message[ label ] ;
			}
		}
	}
}

