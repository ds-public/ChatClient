#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System ;
using System.IO ;
using System.Collections ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// デフォルトフォント設定の操作ウィンドウ
	/// </summary>
	
	public class FontFilterMenu : EditorWindow
	{
		[ MenuItem( "uGUIHelper/Tools/FontFilter" ) ]
		internal static void OpenWindow()
		{
			EditorWindow.GetWindow<FontFilterMenu>( false, "Font Filter", true ) ;
		}

		private Font m_Font = null ;

		// レイアウトを描画する
		internal void OnGUI()
		{
			string tPath = null ;
			string[] tId = AssetDatabase.FindAssets( "FontFilter" ) ;
			if( tId != null )
			{
				int i, l = tId.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					tPath = AssetDatabase.GUIDToAssetPath( tId[ i ] ) ;

					if( Directory.Exists( tPath ) == true )
					{
						break ;
					}
				}

				if( i >= l )
				{
					tPath = null ;
				}
			}

			if( string.IsNullOrEmpty( tPath ) == true )
			{
				EditorGUILayout.HelpBox( "状態が異常です", MessageType.Warning ) ;
				return ;
			}

			tPath += "/Resources/uGUIHelper" ;
			if( Directory.Exists( tPath ) == false )
			{
				EditorGUILayout.HelpBox( "保存フォルダが存在しません", MessageType.Warning ) ;
				return ;
			}

			FontFilter tFF ;

			tPath += "/FontFilter.asset" ;
			if( File.Exists( tPath ) == false )
			{
				// ファイルが存在しない
				tFF= ScriptableObject.CreateInstance<FontFilter>() ;
				tFF.name = "FontFilter" ;
		
				AssetDatabase.CreateAsset( tFF, tPath ) ;
				AssetDatabase.Refresh() ;
			}
			else
			{
				// ファイルが存在する
				tFF = AssetDatabase.LoadAssetAtPath<FontFilter>( tPath ) ;
			}

			Selection.activeObject = tFF ;

			//----------------------------------------------------------

			bool tDirty = false ;


			// フォント
			Font tFont = EditorGUILayout.ObjectField( "Font", m_Font, typeof( Font ), false ) as Font ;
			if( tFont != m_Font )
			{
				m_Font = tFont ;
			}

			if( m_Font != null )
			{
				if( GUILayout.Button( "Create Or Update" ) == true )
				{
					tPath = AssetDatabase.GetAssetPath(	m_Font.GetInstanceID() ) ;
	
					TrueTypeFontImporter tFontData = ( TrueTypeFontImporter )AssetImporter.GetAtPath( tPath ) ;
				
					FontTextureCase tOldFontTextureCase = tFontData.fontTextureCase ;

					if( tFontData.fontTextureCase != FontTextureCase.Unicode )
					{
						tFontData.fontTextureCase = FontTextureCase.Unicode ;
						tFontData.SaveAndReimport() ;
					}			

					//--------------------------------------------------------------------

					if( tFF.flag == null || tFF.flag.Length <  8192 )
					{
						tFF.flag = new byte[ 8192 ] ;
					}

					int v = 0 ;
					byte f ;
					bool e ;
					char c ;
					int i, j, l = 65536 ;
					for( i  = 0 ; i <  l ; i += 8 )
					{
						f = 0 ;
						for( j  = 0 ; j <  8 ; j ++ )
						{
							c = ( char )( i + j ) ;

							e = true ;
							if( c != ' ' && c != '　' )
							{
								if( m_Font.HasCharacter( c ) == true )
								{
									if( m_Font.GetCharacterInfo( c, out CharacterInfo tCI, 16 ) == true )
									{
										if( tCI.advance <= 0 )
										{
											e = false ;
										}
									}
									else
									{
										e = false ;
									}
								}
								else
								{
									e = false ;
								}
							}

							if( e == true )
							{
								f = ( byte )( f | ( 1 << j ) ) ;
								v ++ ;
							}
						}

						tFF.flag[ i >> 3 ] = f ;
					}

					//--------------------------------------------------------------------

					if( tOldFontTextureCase != FontTextureCase.Unicode )
					{
						tFontData.fontTextureCase = tOldFontTextureCase ;
						tFontData.SaveAndReimport() ;
					}			

					//--------------------------------------------------------------------

					tDirty = true ;

					EditorUtility.DisplayDialog( "Font Filter", "Completed !! -> " + v, "OK" ) ;

				}
			}

			//----------------------------------

			// 更新判定
			if( tDirty == true )
			{
				EditorUtility.SetDirty( tFF ) ; // 更新実行
//				AssetDatabase.Refresh() ;
			}
		}
	}
}

#endif
