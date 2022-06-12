using UnityEngine ;
using UnityEditor ;
using System ;
using System.IO ;
using System.Collections.Generic ;

/// <summary>
/// シンプルマスクメーカーパッケージ
/// </summary>
namespace Tools.AtlasSprite
{
	/// <summary>
	/// シンプルマスクメーカークラス(エディター用) Version 2020/04/13 0
	/// </summary>
	public class SimpleMaskMaker : EditorWindow
	{
		[ MenuItem( "Tools/Simple Mask Maker" ) ]
		protected static void OpenWindow()
		{
			EditorWindow.GetWindow<SimpleMaskMaker>( false, "Mask Maker", true ) ;
		}

		//-------------------------------------------------------------------------------------------

		private string	m_OutputPath = "Assets/" ;

		private bool	m_DoNotOverwritre = true ;

		public enum ProcessingTypes
		{
			CreateMaskAndOutline	= 0,
			AddOutlineToImage		= 1,
		}

		private ProcessingTypes	m_ProcessingType = ProcessingTypes.CreateMaskAndOutline ;

		private	bool	m_InnerRemoval = false ;

		private Color	m_OutlineColor = Color.black ;

		private int		m_OutlineColorWidth = 0 ;

		private bool	m_OutlineSkew = false ;

		private bool	m_MaintainSize = false ;

		private int		m_OutlineAlphaWidth = 0 ;


		private Vector2 m_ScrollPosition = Vector2.zero ;
	
		//----------------------------------------------------------

		// レイアウトを描画する
		internal void OnGUI()
		{
			// 保存先のパスの設定
			GUILayout.BeginHorizontal() ;
			{
				// 保存パスを選択する
				if( GUILayout.Button( "Output Path", GUILayout.Width( 80f ) ) == true )
				{
					if( Selection.objects != null && Selection.objects.Length == 0 && Selection.activeObject == null )
					{
						// ルート
						m_OutputPath = "Assets/" ;
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
							m_OutputPath = path + "/" ;
						}
						else
						{
							// ファイルを指定しています
							path = path.Replace( "\\", "/" ) ;
							m_OutputPath = path ;

							// 拡張子を見てアセットバンドルであればファイル名まで置き変える
							// ただしこれを読み出して含まれるファイルの解析などは行わない
							// なぜなら違うプラットフォームの場合は読み出せずにエラーになってしまうから
						
							// 最後のフォルダ区切り位置を取得する
							int index = m_OutputPath.LastIndexOf( '/' ) ;
							if( index >= 0 )
							{
								m_OutputPath = m_OutputPath.Substring( 0, index ) + "/" ;
							}
						}
					}
				}
			
				// 保存パス
				m_OutputPath = EditorGUILayout.TextField( m_OutputPath ) ;
			}
			GUILayout.EndHorizontal() ;

			// 上書するかしないか
			m_DoNotOverwritre = EditorGUILayout.Toggle( new GUIContent( "Do Not Overwrite", GetMessage( "Do Not Overwrite" ) ), m_DoNotOverwritre ) ;

			GUILayout.Space( 6f ) ;

			//----------------------------------------------------------

			m_ProcessingType = ( ProcessingTypes )EditorGUILayout.EnumPopup( new GUIContent( "Pocessing Type", GetMessage( "Processing Type" ) ), m_ProcessingType ) ;
			if( m_ProcessingType == ProcessingTypes.CreateMaskAndOutline )
			{
				m_InnerRemoval = EditorGUILayout.Toggle( new GUIContent( "Inner Removal", GetMessage( "Inner Removal" ) ), m_InnerRemoval ) ;
			}
			else
			{
				m_OutlineColor = EditorGUILayout.ColorField( new GUIContent( "Outline Color", GetMessage( "Outline Color" ) ), m_OutlineColor ) ;
			}

			// 以下共通設定
			m_OutlineColorWidth = EditorGUILayout.IntSlider( new GUIContent( "Outline Width", GetMessage( "Outline Width" ) ), m_OutlineColorWidth,   0, 256 ) ;
			if( m_OutlineColorWidth >  0 )
			{
				m_OutlineAlphaWidth = EditorGUILayout.IntSlider( new GUIContent( "Outline Alpha", GetMessage( "Outline Alpha" ) ), m_OutlineAlphaWidth,   0, m_OutlineColorWidth ) ;
				m_OutlineSkew = EditorGUILayout.Toggle( new GUIContent( "Outline Skew", GetMessage( "Outline Skew" ) ), m_OutlineSkew ) ;
				m_MaintainSize = EditorGUILayout.Toggle( new GUIContent( "Maintain Size", GetMessage( "Maintain Size" ) ), m_MaintainSize ) ;
			}

			//----------------------------------------------------------
			
			bool execute = false ;

			string[] targetList = GetTargetList() ;

			// 素材情報のリストを表示する
			if( targetList != null && targetList.Length >  0 )
			{
				EditorGUILayout.Separator() ;
			
				GUI.backgroundColor = Color.green ;
				execute = GUILayout.Button( "Create" ) ;
				GUI.backgroundColor = Color.white ;

				EditorGUILayout.Separator() ;

				EditorGUILayout.LabelField( "Target (" + targetList.Length + ")" ) ;

				GUILayout.BeginVertical() ;
				{
					m_ScrollPosition = GUILayout.BeginScrollView( m_ScrollPosition ) ;
					{
						int i, l = targetList.Length ;
						for( i  = 0 ; i <  l ; i ++ )
						{
							GUILayout.BeginHorizontal( "TextArea", GUILayout.MinHeight( 20f ) ) ;	// 横一列開始
							{
								GUILayout.Label( targetList[ i ], GUILayout.Height( 20f ) ) ;
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
				EditorGUILayout.HelpBox( GetMessage( "Select Texture" ), MessageType.Info ) ;
			}

			//-------------------------------------------

			if( execute == true )
			{
				// アトラスを生成する
				MakeMaskAll( targetList ) ;
			}
		}

		internal void OnSelectionChange() 
		{
			Repaint() ;
		}
	
		//-------------------------------------------------------------------------------------------

		private string[] GetTargetList()
		{
			List<string> targets = new List<string>() ;

			// 選択中の素材を追加する
			foreach( UnityEngine.Object selectedObject in Selection.objects )
			{
				Texture2D texture = selectedObject as Texture2D ;
			
				if( texture == null )
				{
					continue ;
				}

				//-----------------------------------------

				// 素材となる画像がスプライト（アトラス）かそれ以外（テクスチャ）かで処理が異なる
				string texturePath = AssetDatabase.GetAssetPath( texture.GetInstanceID() ) ;
				TextureImporter textureImporter = AssetImporter.GetAtPath( texturePath ) as TextureImporter ;
				if( textureImporter != null && textureImporter.textureType == TextureImporterType.Sprite )
				{
					// スプライト扱い
					targets.Add( texturePath ) ;
				}
				else
				{
					// テクスチャ扱い
					targets.Add( texturePath ) ;
				}
			}

			if( targets.Count == 0 )
			{
				return null ;
			}

			return targets.ToArray() ;
		}

		//-------------------------------------------------------------------------------------------

		private void MakeMaskAll( string[] texturePaths )
		{
			int i, l = texturePaths.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				MakeMask( texturePaths[ i ] ) ;
			}
		}

		private void MakeMask( string inputPath )
		{
			TextureImporter inputTextureImporter = AssetImporter.GetAtPath( inputPath ) as TextureImporter ;

			bool isReadable = inputTextureImporter.isReadable ;

			inputTextureImporter.isReadable = true ;	// 書き込み属性を有効にする

			AssetDatabase.ImportAsset( inputPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport ) ;

			AssetDatabase.SaveAssets() ;
			AssetDatabase.Refresh() ;

			Texture2D inputTexture = AssetDatabase.LoadAssetAtPath( inputPath, typeof( Texture2D ) ) as Texture2D ;
			
			//----------------------------------------------------------

			int x, y, o ;

			int sw = inputTexture.width ;	// 横幅
			int sh = inputTexture.height ;	// 縦幅

			Color32[] inputPixels = inputTexture.GetPixels32() ;

			int dx ;
			int dy ;

			int dw ;
			int dh ;

			// アウトライン用に画像を大きくするか
			if( m_MaintainSize == false )
			{
				// 場合によっては拡張する

				int xMin, xMax ;
				int yMin, yMax ;

				xMin = sw ;
				xMax =  0 ;
				yMin = sh ;
				yMax =  0 ;

				for( y  = 0 ; y <  sh ; y ++ )
				{
					for( x  = 0 ; x <  sw ; x ++ )
					{
						o = y * sw + x ;

						if( inputPixels[ o ].a >  0 )
						{
							if( x <  xMin )
							{
								xMin  = x ;
							}
							if( x >  xMax )
							{
								xMax  = x ;
							}
							if( y <  yMin )
							{
								yMin  = y ;
							}
							if( y >  yMax )
							{
								yMax  = y ;
							}
						}
					}
				}

				if( xMin <  m_OutlineColorWidth || ( sw - 1 - xMax ) <  m_OutlineColorWidth || yMin <  m_OutlineColorWidth || ( sh - 1 - yMax ) <  m_OutlineColorWidth )
				{
					// スペースが足りない箇所が存在する

					dx = m_OutlineColorWidth ;
					dy = m_OutlineColorWidth ;

					dw = sw + m_OutlineColorWidth * 2 ;
					dh = sh + m_OutlineColorWidth * 2 ;
				}
				else
				{
					dx =  0 ;
					dy =  0 ;

					dw = sw ;
					dh = sh ;
				}
			}
			else
			{
				// 維持する
				dx =  0 ;
				dy =  0 ;

				dw = sw ;
				dh = sh ;
			}
			
			Texture2D outputTexture = new Texture2D( dw, dh, TextureFormat.ARGB32, false ) ;

			Color32[]	outputPixels	= new Color32[ dw * dh ] ;
			int[]		code			= new int[ dw * dh ] ;

			for( y  = 0 ; y <  dh ; y ++ )
			{
				for( x  = 0 ; x <  dw ; x ++ )
				{
					o = y * dw + x ;
					outputPixels[ o ] = new Color32( 0, 0, 0, 0 ) ;
					code[ o ] = -1 ;
				}
			}
			
			//----------------------------------------------------------

			int o0, o1 ;

			for( y  = 0 ; y <  sh ; y ++ )
			{
				for( x  = 0 ; x <  sw ; x ++ )
				{
					o0 = y * sw + x ;
					o1 = ( dy + y ) * dw + ( dx + x ) ;

					if( inputPixels[ o0 ].a >  0 )
					{
						// 色が存在する箇所
						code[ o1 ] = 0 ;
					}
				}
			}

			if( m_OutlineColorWidth >  0 )
			{
				// アウトラインの箇所にチェックを入れる

				int level ;

				for( level  = 0 ; level <  m_OutlineColorWidth ; level ++ )
				{
					for( y  = 1 ; y <  ( dh - 1 ) ; y ++ )
					{
						for( x  = 1 ; x <  ( dw - 1 ) ; x ++ )
						{
							o = y * dw + x ;

							if( code[ o ] <  0 )
							{
								if
								(
									Check( code, x,     y - 1, level, dw ) == true ||
									Check( code, x,     y + 1, level, dw ) == true ||
									Check( code, x - 1, y,     level, dw ) == true ||
									Check( code, x + 1, y,     level, dw ) == true
								)
								{
									// 上下左右のいずれかにドットがあれば有効
									code[ o ] = 1 + level ;
								}
	
								if( m_OutlineSkew == true )
								{
									// 斜めも有効
									if
									(
										Check( code, x - 1, y - 1, level, dw ) == true ||
										Check( code, x + 1, y - 1, level, dw ) == true ||
										Check( code, x - 1, y + 1, level, dw ) == true ||
										Check( code, x + 1, y + 1, level, dw ) == true
									)
									{
										code[ o ] = 1 + level ;
									}
								}
							}
						}
					}
				}
			}

			//----------------------------------------------------------

			// 中

			// 絵の部分(モードによって処理を変える)
			if( m_ProcessingType == ProcessingTypes.CreateMaskAndOutline )
			{
				// 色は白限定
				if( m_InnerRemoval == false )
				{
					for( y  = 0 ; y <  sh ; y ++ )
					{
						for( x  = 0 ; x <  sw ; x ++ )
						{
							o0 = y * sw + x ;
							o1 = ( dy + y ) * dw + ( dx + x ) ;
		
							if( inputPixels[ o0 ].a >  0 )
							{
								// 色が存在する箇所
								outputPixels[ o1 ] = Color.white ;
							}
						}
					}
				}
			}
			else
			{
				// 絵にアウトラインを加える
				for( y  = 0 ; y <  sh ; y ++ )
				{
					for( x  = 0 ; x <  sw ; x ++ )
					{
						o0 = y * sw + x ;
						o1 = ( dy + y ) * dw + ( dx + x ) ;
	
						if( inputPixels[ o0 ].a >  0 )
						{
							// 色が存在する箇所
							outputPixels[ o1 ] = inputPixels[ o0 ] ;
						}
					}
				}
			}

			//----------------------------------------------------------

			// 外

			if( m_OutlineColorWidth >  0 )
			{
				// コード値が１以上の箇所
				Color32 color ;

				if( m_ProcessingType == ProcessingTypes.CreateMaskAndOutline )
				{
					// 色
					color = Color.white ;
				}
				else
				{
					color = m_OutlineColor ;
				}

				int outlineAlphaWidth = m_OutlineAlphaWidth ;

				if( outlineAlphaWidth >  m_OutlineColorWidth )
				{
					outlineAlphaWidth  = m_OutlineColorWidth ;
				}

				Color32[] colorTable = new Color32[ m_OutlineColorWidth ] ;

				int i, p ;
				for( i  = 0 ; i <  m_OutlineColorWidth ; i ++ )
				{
					colorTable[ i ] = color ;
					if( ( outlineAlphaWidth + i ) <  m_OutlineColorWidth )
					{
//						colorTable[ i ].a = ( byte )( ( colorTable[ i ].a * 255 ) / 255 ) ;
					}
					else
					{
						// 半透明になっていく(0～outlineAlphaWidth-1)
						p = ( outlineAlphaWidth + i ) - m_OutlineColorWidth ;
						colorTable[ i ].a = ( byte )( ( colorTable[ i ].a * ( outlineAlphaWidth - p ) * ( 255 / ( outlineAlphaWidth + 1 ) ) ) / 255 ) ;
					}
				}

				for( y  = 0 ; y <  dh ; y ++ )
				{
					for( x  = 0 ; x <  dw ; x ++ )
					{
						o = y * dw + x ;

						p = code[ o ] ;
						if( p >= 1 )
						{
							outputPixels[ o ] = colorTable[ p - 1 ] ;
						}
					}
				}
			}

			//----------------------------------------------------------

			outputTexture.SetPixels32( outputPixels ) ;

			//----------------------------------------------------------
			
			string outputPath ;
			if( string.IsNullOrEmpty( m_OutputPath ) == true )
			{
				// 上書
				outputPath = inputPath ;
			}
			else
			{
				// 新規
				outputPath = m_OutputPath ;
				if( outputPath[ outputPath.Length - 1 ] != '/' )
				{
					outputPath += "/" ;
				}

				string fileName ;

				int i = inputPath.LastIndexOf( "/" ) ;
				if( i >= 0 )
				{
					i ++ ;
					fileName = inputPath.Substring( i, inputPath.Length - i ) ;
				}
				else
				{
					fileName = inputPath ;
				}

				outputPath += fileName ;
			}

			if( outputPath == inputPath )
			{
				// 出力パスが同一の場合の処理
				if( m_DoNotOverwritre == true )
				{
					// 強制的に別のパスにする
					int i = outputPath.LastIndexOf( "." ) ;
					if( i >= 0 )
					{
						outputPath = outputPath.Substring( 0, i ) ;
					}

					if( m_ProcessingType == ProcessingTypes.CreateMaskAndOutline )
					{
						outputPath += "_MaskAndOutline.png" ;
					}
					else
					{
						outputPath += "_Outline.png" ;
					}
				}
			}


			// テクスチャをＰＮＧ画像として保存する
			byte[] data = outputTexture.EncodeToPNG() ;
			File.WriteAllBytes( outputPath, data ) ;

			AssetDatabase.SaveAssets() ;
			AssetDatabase.Refresh() ;

			if( outputPath != inputPath )
			{
				// 新規のケース(保存後に元の読み込み属性を禁止にする

				TextureImporter outputTextureImporter = AssetImporter.GetAtPath( outputPath ) as TextureImporter ;

				outputTextureImporter.npotScale				= TextureImporterNPOTScale.None ;

				outputTextureImporter.textureType			= TextureImporterType.Sprite ;
				outputTextureImporter.spriteImportMode		= SpriteImportMode.Single ;

				outputTextureImporter.spriteBorder			= inputTextureImporter.spriteBorder ;
				outputTextureImporter.spritePivot			= inputTextureImporter.spritePivot ;

				outputTextureImporter.alphaIsTransparency	= inputTextureImporter.alphaIsTransparency ;
				outputTextureImporter.mipmapEnabled			= inputTextureImporter.mipmapEnabled ;
				outputTextureImporter.wrapMode				= inputTextureImporter.wrapMode ;
				outputTextureImporter.filterMode			= inputTextureImporter.filterMode ;
				outputTextureImporter.textureCompression	= inputTextureImporter.textureCompression ;

				outputTextureImporter.isReadable			= false ;	// 読み込み禁止

				AssetDatabase.ImportAsset( outputPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport ) ;
			}

			if( isReadable != inputTextureImporter.isReadable )
			{
				inputTextureImporter.isReadable = isReadable ;	// 書き込み属性を無効にする

				AssetDatabase.ImportAsset( inputPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport ) ;
			}

			AssetDatabase.SaveAssets() ;
			AssetDatabase.Refresh() ;

			Resources.UnloadUnusedAssets() ;
		}

		private bool Check( int[] code, int x, int y, int c, int dw )
		{
			return code[ y * dw + x ] == c ;
		}

		//--------------------------------------------------------------------------

		private readonly Dictionary<string,string> m_Japanese_Message = new Dictionary<string, string>()
		{
			{ "Select Texture",		"アウトラインを追加したいテクスチャを選択してください(複数可)" },
			{ "Do Not Overwrite",	"ソース画像に上書きを行う" },
			{ "Processing Type",	"処理方法(CreateMaskAndOutline:マスク・アウトラインを新たに生成する・AddOutlineToImage:画像にアウトラインを加える)" },
			{ "Inner Removal",		"マスク部分を除外する" },
			{ "Outline Color",		"アウトラインの色" },
			{ "Outline Width",		"アウトラインの幅" },
			{ "Outline Skew",		"アウトラインの角を描画する" },
			{ "Maintain Size",		"生成画像のサイズをソース画像のサイズと同じにする" },
			{ "Outline Alpha",		"アウトラインの半透明部分の幅" },
		} ;
		private readonly Dictionary<string,string> m_English_Message = new Dictionary<string, string>()
		{
			{ "Select Texture",		"Please select one or more textures you want to add outline." },
			{ "Do Not Overwrite",	"Overwrite source image." },
			{ "Processing Type",	"Processing method (CreateMaskAndOutline: Create new mask / outline ・ AddOutlineToImage: Add outline to image)." },
			{ "Inner Removal",		"Exclude mask part." },
			{ "Outline Color",		"Outline color." },
			{ "Outline Width",		"Outline width." },
			{ "Outline Skew",		"Draw outline corners." },
			{ "Maintain Size",		"Make the size of the generated image the same as the size of the source image." },
			{ "Outline Alpha",		"Width of translucent part of outline." },
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

