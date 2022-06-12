using System ;
using System.IO ;
using System.Collections.Generic ;
using System.Reflection ;
using UnityEngine ;
using UnityEditor ;


/// <summary>
/// グリッドアトラススプライトメーカーパッケージ
/// </summary>
namespace Tools.AtlasSprite
{
	/// <summary>
	/// グリッドアトラススプライトメーカークラス(エディター用) Version 2020/04/13 0
	/// </summary>
	public class GridAtlasSpriteMaker : EditorWindow
	{
		[ MenuItem( "Tools/Grid Atlas Sprite Maker" ) ]
		protected static void OpenWindow()
		{
			EditorWindow.GetWindow<GridAtlasSpriteMaker>( false, "Grid Maker", true ) ;
		}

		//-------------------------------------------------------------------------------------------

		private string		m_TargetPath = "Assets/" ;
		private Texture2D	m_TargetTexture ;

		public enum ProcessingTypes
		{
			GridSplit	= 0,	// 分割数指定
			FrameSize	= 1,	// フレームサイズ指定
		}

		private ProcessingTypes	m_ProcessingType = ProcessingTypes.GridSplit ;

		private string	m_FrameName = "" ;

		private int	m_DigitOfIndex = 2 ;


		private bool m_ReverseHorizontal = false ;	// 横の順番判定
		private bool m_ReverseVertical   = true ;	// 縦の順番判定


		private Color	m_FrameColor = Color.white ;


		private int		m_GridSplitX = 1 ;
		private int		m_GridSplitY = 1 ;

		private int		m_FrameSizeW ;
		private int		m_FrameSizeH ;


		//----------------------------------------------------------

		// 塗りつぶしの無い四角を描画する
		private void DrawRect( Rect rect, Color color )
		{
			Rect r = new Rect() ;

			if( rect.height >  1 )
			{
				// 左
				r.x			= rect.x ;
				r.y			= rect.y ;
				r.width		= 1 ;
				r.height	= rect.height ;
				EditorGUI.DrawRect( r, color ) ;

				if( rect.width >  1 )
				{
					// 右
					r.x			= rect.xMax ;
					r.y			= rect.y ;
					r.width		= 1 ;
					r.height	= rect.height ;
					EditorGUI.DrawRect( r, color ) ;
				}
			}

			if( rect.width >  1 )
			{
				// 下
				r.x			= rect.x ;
				r.y			= rect.y ;
				r.width		= rect.width ;
				r.height	= 1 ;
				EditorGUI.DrawRect( r, color ) ;

				if( rect.height >  1 )
				{
					// 上
					r.x			= rect.x ;
					r.y			= rect.yMax ;
					r.width		= rect.width ;
					r.height	= 1 ;
					EditorGUI.DrawRect( r, color ) ;
				}
			}

			if( rect.width == 1 && rect.height == 1 )
			{
				// １ドット
				EditorGUI.DrawRect( rect, color ) ;
			}
		}


		//----------------------------------------------------------

		// レイアウトを描画する
		internal void OnGUI()
		{
			// 保存先のパスの設定
			GUILayout.BeginHorizontal() ;
			{
				// 対象パスを選択する
				if( GUILayout.Button( "Select", GUILayout.Width( 80f ) ) == true )
				{
					m_GridSplitX = 1 ;
					m_GridSplitY = 1 ;
					m_FrameSizeW = 0 ;
					m_FrameSizeH = 0 ;

					if( m_TargetTexture != null )
					{
						m_TargetTexture	= null ;
						Resources.UnloadUnusedAssets() ;
					}

					m_TargetPath	= string.Empty ;
					if( Selection.objects != null && Selection.objects.Length == 1 && Selection.activeObject != null )
					{
						string path = AssetDatabase.GetAssetPath( Selection.activeObject.GetInstanceID() ) ;
						if( File.Exists( path ) == true && Selection.activeObject.GetType() == typeof( Texture2D ) )
						{
							// ファイルを指定している・基本的にタイプは全て Texture2D 扱いになる
							path = path.Replace( "\\", "/" ) ;

							m_TargetPath = path ;
							m_TargetTexture = AssetDatabase.LoadAssetAtPath( path, typeof( Texture2D ) ) as Texture2D ;
						}
					}
				}
			
				// 保存パス
				EditorGUILayout.TextField( m_TargetPath ) ;
			}
			GUILayout.EndHorizontal() ;

			if( m_TargetTexture == null )
			{
				EditorGUILayout.HelpBox( GetMessage( "Select Texture" ), MessageType.Info ) ;
			}

			GUILayout.Space( 6f ) ;

			//----------------------------------------------------------

			// 画像が選択されていれば表示する
			if( m_TargetTexture != null )
			{
				int frameSizeW ;
				int frameSizeH ;

				int gridSplitX ;
				int gridSplitY ;

				TextureImporter ti = AssetImporter.GetAtPath( m_TargetPath ) as TextureImporter ;

				// 画像タイプ
				if( ti.textureType == TextureImporterType.Sprite )
				{
					// Sprite

					if( ti.spriteImportMode == SpriteImportMode.Multiple )
					{
						// Sprite Multiple
						int spriteCount = 0 ;
						if( ti.spritesheet != null && ti.spritesheet.Length >  0 )
						{
							spriteCount = ti.spritesheet.Length ;
						}
						EditorGUILayout.LabelField( "Type : Sprite Multiple : " + " ( " + spriteCount + " ) " ) ;
					}
					else
					if( ti.spriteImportMode == SpriteImportMode.Single )
					{
						// Single Single
						EditorGUILayout.LabelField( "Type : Sprite Single" ) ;
					}
					else
					{
						EditorGUILayout.LabelField( "Type : Sprite" ) ;
					}
				}
				else
				{
					// Texture
					EditorGUILayout.LabelField( "Type : Texture" ) ;
				}

				//---------------------------------

				// 処理タイプ
				m_ProcessingType = ( ProcessingTypes )EditorGUILayout.EnumPopup( new GUIContent( "Pocessing Type", GetMessage( "Processing Type" ) ), m_ProcessingType ) ;

				//---------------------------------

				// 裏技を使ってオリジナルのテクスチャのサイズを取得する(maxTextureSizeにリサイズされてしまうため)
				object[] args = new object[ 2 ]{ 0, 0 };
				MethodInfo method = typeof( TextureImporter ).GetMethod( "GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance ) ;
				method.Invoke( ti, args ) ;				
				int rtw = ( int )args[ 0 ] ;
				int rth = ( int )args[ 1 ] ;

				if( m_FrameSizeW == 0 )
				{
					m_FrameSizeW = rtw ;
				}
				if( m_FrameSizeH == 0 )
				{
					m_FrameSizeH = rth ;
				}

				//---------------------------------

				if( m_ProcessingType == ProcessingTypes.GridSplit )
				{
					// 分割数を設定
					GUILayout.BeginHorizontal() ;
					{
						GUILayout.Label( "X", GUILayout.Width( 20f ) ) ;
						gridSplitX = EditorGUILayout.IntField( m_GridSplitX, GUILayout.Width( 48f ) ) ;
						if( m_GridSplitX != gridSplitX && gridSplitX >= 1 && gridSplitX <= rtw )
						{
							m_GridSplitX  = gridSplitX ;
						}
						GUILayout.Label( " ", GUILayout.Width( 10f ) ) ;
						GUILayout.Label( "Y", GUILayout.Width( 20f ) ) ;
						gridSplitY = EditorGUILayout.IntField( m_GridSplitY, GUILayout.Width( 48f ) ) ;
						if( m_GridSplitY != gridSplitY && gridSplitY >= 1 && gridSplitY <= rth )
						{
							m_GridSplitY  = gridSplitY ;
						}
					}
					GUILayout.EndHorizontal() ;
				}
				else
				{
					// 横縦幅を設定
					GUILayout.BeginHorizontal() ;
					{
						GUILayout.Label( "W", GUILayout.Width( 20f ) ) ;
						frameSizeW = EditorGUILayout.IntField( m_FrameSizeW, GUILayout.Width( 48f ) ) ;
						if( m_FrameSizeW != frameSizeW && frameSizeW >= 1 && frameSizeW <= rtw )
						{
							m_FrameSizeW  = frameSizeW ;
						}
						GUILayout.Label( " ", GUILayout.Width( 10f ) ) ;
						GUILayout.Label( "H", GUILayout.Width( 20f ) ) ;
						frameSizeH = EditorGUILayout.IntField( m_FrameSizeH, GUILayout.Width( 48f ) ) ;
						if( m_FrameSizeH != frameSizeH && frameSizeH >= 1 && frameSizeH <= rth )
						{
							m_FrameSizeH  = frameSizeH ;
						}
					}
					GUILayout.EndHorizontal() ;
				}

				//---------------------------------

				// オリジナルサイズを表示する
				EditorGUILayout.LabelField( "Texture Size : " + rtw + " x " + rth ) ;

				//---------------------------------

				GUILayout.Space( 4 ) ;

				//---------------------------------

				// 次にGUIを描画する座標を取得する(引数のサイズは特に意味なし)
				Rect r = GUILayoutUtility.GetRect( 16, 16 ) ;

				// Screen.widthは、Windowsの横幅(またはInspectorの横幅)を示す。
				r.x += 8 ;
				float windowWidth = position.width - 16 ;	// 少しだけ小さくする

				// EditorWindow のサイズ取得は position.width と position.height

				float tw = m_TargetTexture.width ;
				float th = m_TargetTexture.height ;
				float scaleW = 1.0f ;
				float scaleH = 1.0f ;

				// サイズ調整
				if( tw != windowWidth )
				{
					scaleW = windowWidth / tw ;
				}
				if( th != 128 )
				{
					scaleH = 128 / th ;	// 縦幅は最大256
				}

				// 小さい方のスケールを使用する
				float scale ;
				if( scaleW <  scaleH )
				{
					scale = scaleW ;
				}
				else
				{
					scale = scaleH ;
				}

				tw *= scale ;
				th *= scale ;

				r.width  = tw ;
				r.height = th ;

				GUI.DrawTexture( r, m_TargetTexture ) ;
				DrawRect( r, Color.yellow ) ;

				//---------------------------------------------------------

				// 基準座標
				float sx = r.x ;
				float sy = r.y ;
				float sw = r.width ;
				float sh = r.height ;

				// スケーリング値を更に調整する(maxTextureSize対策)
				scaleW = scale * ( m_TargetTexture.width  / ( float )rtw ) ;
				scaleH = scale * ( m_TargetTexture.height / ( float )rth ) ;

				if( ti.textureType == TextureImporterType.Sprite )
				{
					// Sprite

					if( ti.spriteImportMode == SpriteImportMode.Multiple )
					{
						// Multiple
						foreach( SpriteMetaData spriteMetaData in ti.spritesheet )
						{
							r.x			= sx + spriteMetaData.rect.x * scaleW ;
							r.y			= sy + spriteMetaData.rect.y * scaleH ;
							r.width		= spriteMetaData.rect.width  * scaleW ;
							r.height	= spriteMetaData.rect.height * scaleH ;
							DrawRect( r, m_FrameColor ) ;
						}
					}
					else
					if( ti.spriteImportMode == SpriteImportMode.Single )
					{
						// Single

						Vector4 border	= ti.spriteBorder ;

						if( border.x >  0 )
						{
							// 左
							r.x			= sx + border.x * scaleW ;
							r.y			= sy ;
							r.width		= 1 ;
							r.height	= sh ;
							DrawRect( r, m_FrameColor ) ;
						}

						if( border.y >  0 )
						{
							// 下
							r.x			= sx ;
							r.y			= sy + border.y * scaleH ;
							r.width		= sw ;
							r.height	= 1 ;
							DrawRect( r, m_FrameColor ) ;
						}

						if( border.z >  0 )
						{
							// 右
							r.x			= sx + sw - 1 - border.z * scaleW ;
							r.y			= sy ;
							r.width		= 1 ;
							r.height	= sh ;
							DrawRect( r, m_FrameColor ) ;
						}

						if( border.w >  0 )
						{
							// 上
							r.x			= sx ;
							r.y			= sy + sh - 1 - border.w * scaleH ;
							r.width		= sw ;
							r.height	= 1 ;
							DrawRect( r, m_FrameColor ) ;
						}
					}
				}

				//---------------------------------------------------------

				// 注意：縦(Y)の座標系はGUI(上=-～下=+)とTexture(上=+～下=-)とで逆になる

				int totalSprite = 0 ;

				// グリッドを設定する
				if( m_ProcessingType == ProcessingTypes.GridSplit )
				{
					// 分割数
					frameSizeW = rtw / m_GridSplitX ;
					frameSizeH = rth / m_GridSplitY ;

					gridSplitX = m_GridSplitX ;
					gridSplitY = m_GridSplitY ;

				}
				else
				{
					// 横縦幅
					gridSplitX = rtw / m_FrameSizeW ;
					gridSplitY = rth / m_FrameSizeH ;

					frameSizeW = m_FrameSizeW ;
					frameSizeH = m_FrameSizeH ;
				}

				int x, y ;
				int ox, oy, bx,  ax, ay ;
				int cx, cy ;

				if( m_ReverseHorizontal == false )
				{
					ox = 0 ; bx = ox ;
					ax = 1 ;
					cx = 0 ;
				}
				else
				{
					ox = gridSplitX - 1 ; bx = ox ;
					ax = -1 ;
					cx = rtw - ( gridSplitX * frameSizeW ) ;
				}

				if( m_ReverseVertical == false )
				{
					oy = gridSplitY - 1 ;
					ay = -1 ;
					cy = rth - ( gridSplitY * frameSizeH ) ;
				}
				else
				{
					oy = 0 ;
					ay = 1 ;
					cy = 0 ;
				}

				for( y  = 0 ; y <  gridSplitY ; y ++ )
				{
					for( x  = 0 ; x <  gridSplitX ; x ++ )
					{
						r.x			= sx + ( ( frameSizeW * ox ) + cx ) * scaleW ;
						r.y			= sy + ( ( frameSizeH * oy ) + cy ) * scaleH ;
						r.width		= frameSizeW * scaleW ;
						r.height	= frameSizeH * scaleH ;
						DrawRect( r, Color.cyan ) ;

						totalSprite ++ ;

						ox += ax ;
					}

					ox = bx ;
					oy += ay ;
				}

				//---------------------------------------------------------

				GUILayout.Space( th ) ;

				//---------------------------------------------------------

				GUILayout.Label( "Total Frame : " + totalSprite ) ;

				GUILayout.BeginHorizontal() ;
				{
					GUILayout.Label( new GUIContent( "Frame Name", GetMessage( "Frame Name" ) ), GUILayout.Width( 128f ) ) ;
					m_FrameName = EditorGUILayout.TextField( m_FrameName, GUILayout.Width( 64f ) ) ;
					GUILayout.Label( " ", GUILayout.Width( 10f ) ) ;
					GUILayout.Label( new GUIContent( "Digit Of Index", GetMessage( "Digit Of Index" ) ), GUILayout.Width( 128f ) ) ;
					m_DigitOfIndex = EditorGUILayout.IntPopup( m_DigitOfIndex, new string[]{ "1", "2", "3", "4" }, new int[]{ 1, 2, 3, 4 } ) ;
				}
				GUILayout.EndHorizontal() ;


				m_ReverseHorizontal = EditorGUILayout.Toggle( new GUIContent( "Reverse Horizontal", GetMessage( "Reverse Horizontal" ) ), m_ReverseHorizontal ) ;
				m_ReverseVertical   = EditorGUILayout.Toggle( new GUIContent( "Reverse Vertical",   GetMessage( "Reverse Vertical"   ) ), m_ReverseVertical ) ;

				//---------------------------------------------------------

				EditorGUILayout.Separator() ;
			
				bool execute ;

				GUI.backgroundColor = Color.green ;
				execute = GUILayout.Button( "Convert" ) ;
				GUI.backgroundColor = Color.white ;

				//---------------------------------------------------------

				if( execute == true )
				{
					if( EditorUtility.DisplayDialog( GetMessage( "Dialog Title" ), GetMessage( "Dialog Message" ), GetMessage( "Yes" ), GetMessage( "No" ) ) == true )
					{
						// GridAtlasSpriteに変換する
						Convert( m_TargetTexture, rtw, rth ) ;
					}
				}
			}
		}

		internal void OnSelectionChange() 
		{
			Repaint() ;
		}
	
		//-------------------------------------------------------------------------------------------

		private void Convert( Texture2D texture, int rtw, int rth )
		{
			TextureImporter ti = AssetImporter.GetAtPath( m_TargetPath ) as TextureImporter ;

			bool isReadable = ti.isReadable ;
			ti.isReadable	= true ;	// 書き込みを一旦有効にする

			AssetDatabase.ImportAsset( m_TargetPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport ) ;

			//----------------------------------

			if( ti.textureType != TextureImporterType.Sprite )
			{
				ti.textureType			= TextureImporterType.Sprite ;
				ti.spriteImportMode		= SpriteImportMode.Multiple ;

				ti.npotScale			= TextureImporterNPOTScale.None ;
			}
			else
			{
				if( ti.spriteImportMode != SpriteImportMode.Multiple )
				{
					ti.spriteImportMode		= SpriteImportMode.Multiple ;

					ti.spriteBorder			= Vector4.zero ;
					ti.spritePivot			= new Vector2( 0.5f, 0.5f ) ;
				}
			}

			List<SpriteMetaData> list = new List<SpriteMetaData>() ;
			SpriteMetaData data ;
			Rect r ;
			int index = 0 ;

			int frameSizeW ;
			int frameSizeH ;

			int gridSplitX ;
			int gridSplitY ;

			if( m_ProcessingType == ProcessingTypes.GridSplit )
			{
				// 分割数
				frameSizeW = rtw / m_GridSplitX ;
				frameSizeH = rth / m_GridSplitY ;

				gridSplitX = m_GridSplitX ;
				gridSplitY = m_GridSplitY ;
			}
			else
			{
				// 横縦幅
				gridSplitX = rtw / m_FrameSizeW ;
				gridSplitY = rth / m_FrameSizeH ;

				frameSizeW = m_FrameSizeW ;
				frameSizeH = m_FrameSizeH ;
			}

			int x, y ;
			int ox, oy, bx,  ax, ay ;
			int cx, cy ;

			if( m_ReverseHorizontal == false )
			{
				ox = 0 ; bx = ox ;
				ax = 1 ;
				cx = 0 ;
			}
			else
			{
				ox = gridSplitX - 1 ; bx = ox ;
				ax = -1 ;
				cx = rtw - ( gridSplitX * frameSizeW ) ;
			}

			if( m_ReverseVertical == false )
			{
				oy = 0 ;
				ay = 1 ;
				cy = 0 ;
			}
			else
			{
				oy = gridSplitY - 1 ;
				ay = -1 ;
				cy = rth - ( gridSplitY * frameSizeH ) ;
			}

			for( y  = 0 ; y <  gridSplitY ; y ++ )
			{
				for( x  = 0 ; x <  gridSplitX ; x ++ )
				{
					r = new Rect()
					{
						x		= ( frameSizeW * ox ) + cx,
						y		= ( frameSizeH * oy ) + cy,
						width	= frameSizeW,
						height	= frameSizeH
					} ;

					data = new SpriteMetaData()
					{
						name = string.Format( m_FrameName + "{0:D" + m_DigitOfIndex.ToString() + "}", index ),
						rect = r,
						border = Vector4.zero,
						pivot	= new Vector2( 0.5f, 0.5f )
					} ;

					list.Add( data ) ;
					index ++ ;

					ox += ax ;
				}

				ox = bx ;
				oy += ay ;
			}


			// スプライト情報を設定
			ti.spritesheet = list.ToArray() ;

			texture.Apply() ;

			// 設定を更新する
			ti.isReadable = isReadable ;
			AssetDatabase.ImportAsset( m_TargetPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport ) ;

			AssetDatabase.SaveAssets() ;
			AssetDatabase.Refresh( ImportAssetOptions.ForceUpdate ) ;

			Resources.UnloadUnusedAssets() ;
		}

		//--------------------------------------------------------------------------

		private readonly Dictionary<string,string> m_Japanese_Message = new Dictionary<string, string>()
		{
			{ "Select Texture",		"GridAtlasSprite化したいテクスチャを選択してSelectボタンを押してください" },
			{ "Processing Type",	"処理方法\nGridSplit:分割数を指定する・FrameSize:横縦幅を指定する" },
			{ "Frame Name",			"各フレームの名前を指定してください(インデックス番号が自動的に付きます)" },
			{ "Digit Of Index",		"各フレームのインデックス番号の桁数を指定してください" },
			{ "Reverse Horizontal",	"フレームの横方向の順番を反転します" },
			{ "Reverse Vertical",	"フレームの縦方向の順番を反転します" },
			{ "Dialog Title",		"コンバート確認" },
			{ "Dialog Message",		"GridAtlasSpriteに変換します\n本当によろしいですか？" },
			{ "Yes",				"はい" },
			{ "No",					"いいえ" },
		} ;
		private readonly Dictionary<string,string> m_English_Message = new Dictionary<string, string>()
		{
			{ "Select Texture",		"Select the texture you want to turn into GridAtlasSprite and press the Select button." },
			{ "Processing Type",	"Processing method\nGridSplit: Specify the number of divisions・FrameSize: Specify the width and height." },
			{ "Frame Name",			"Specify the name of each frame (index number is automatically added)." },
			{ "Digit Of Index",		"Please specify the number of digits of the index number of each frame." },
			{ "Reverse Horizontal",	"Reverse the horizontal order of the frame." },
			{ "Reverse Vertical",	"Reverse the vertical order of the frame." },
			{ "Dialog Title",		"Conversion confirmation" },
			{ "Dialog Message",		"Are you sure you want to convert to GridAtlasSprite ?" },
			{ "Yes",				"Yes" },
			{ "No",					"No" },
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

