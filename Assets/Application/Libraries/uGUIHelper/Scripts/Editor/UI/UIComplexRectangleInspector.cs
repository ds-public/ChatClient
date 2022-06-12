#if UNITY_EDITOR

using System ;
using System.IO ;
using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;
using UnityEditor ;

namespace uGUIHelper
{
	/// <summary>
	/// UIGridMap のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIComplexRectangle ) ) ]
	public class UIComplexRectangleInspector : UIViewInspector
	{
		private int m_SpriteIndex = 0 ;


		/// <summary>
		/// スンスペクター描画
		/// </summary>
		override protected void DrawInspectorGUI()
		{
			// ターゲットのインスタンス
			UIComplexRectangle complexRectangle = target as UIComplexRectangle ;
		
			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------
			
			// アトラススプライトの表示
			DrawAtlasForComplexRectangle( complexRectangle ) ;

			// マテリアル選択
			DrawMaterial( complexRectangle ) ;

			//-------------------------------------------------------------------
			//-------------------------------------------------------------------
		
/*			EditorGUILayout.Separator() ;	// 少し区切りスペース
			
			bool tAutoSizeFitting = EditorGUILayout.Toggle( "Auto Size Fitting", tTarget.autoSizeFitting ) ;
			if( tAutoSizeFitting != tTarget.autoSizeFitting )
			{
				Undo.RecordObject( tTarget, "UIComplexRectangle : Auto Size Fitting Change" ) ;	// アンドウバッファに登録
				tTarget.autoSizeFitting = tAutoSizeFitting ;
				EditorUtility.SetDirty( tTarget ) ;
			}*/
		}


		// AtlasSprite の項目を描画する
		protected void DrawAtlasForComplexRectangle( UIView view )
		{
			UIComplexRectangle complexRectangle ;
			if( view is UIComplexRectangle )
			{
				complexRectangle = view as UIComplexRectangle ;
			}
			else
			{
				return ;
			}

			Texture atlasTextureBase = null ;
			if( complexRectangle.SpriteSet != null )
			{
				atlasTextureBase = complexRectangle.SpriteSet.Texture ;
			}

			bool atlasTextureRefresh = false ;

			Texture atlasTexture = EditorGUILayout.ObjectField( "Atlas Texture", atlasTextureBase, typeof( Texture ), false ) as Texture ;
			if( atlasTexture != atlasTextureBase )
			{
				Undo.RecordObject( complexRectangle, "UIAtlasSprite Texture : Change" ) ;	// アンドウバッファに登録

				RefreshAtlasSpriteForComplexRectangle( complexRectangle, atlasTexture ) ;

				complexRectangle.Texture = complexRectangle.SpriteSet.Texture ;

				EditorUtility.SetDirty( complexRectangle ) ;

				atlasTextureRefresh = true ;
			}

			GUILayout.Label( "Atlas Path" ) ;
			GUILayout.BeginHorizontal() ;
			{
				GUI.color = Color.cyan ;
				GUILayout.Label( "Resources/" ) ;
				GUI.color = Color.white ;

				string atlasPath = EditorGUILayout.TextField( "", complexRectangle.SpriteSet.Path ) ;
				if( atlasPath != complexRectangle.SpriteSet.Path )
				{
					Undo.RecordObject( complexRectangle, "UIAtlasSprite Path : Change" ) ;	// アンドウバッファに登録
					complexRectangle.SpriteSet.Path = atlasPath ;
					EditorUtility.SetDirty( complexRectangle ) ;
				}

				atlasPath = "" ;
				if( complexRectangle.SpriteSet.Texture != null )
				{
					string path = AssetDatabase.GetAssetPath( complexRectangle.SpriteSet.Texture.GetInstanceID() ) ;
					if( File.Exists( path ) == true )
					{
						string c = "/Resources/" ;
						int p = path.IndexOf( c ) ;
						if( p >= 0 )
						{
							// 有効なパス
							atlasPath = path.Substring( p + c.Length, path.Length - ( p + c.Length ) ) ;
							p = atlasPath.IndexOf( "." ) ;
							if( p >= 0 )
							{
								atlasPath = atlasPath.Substring( 0, p ) ;
							}
						}
					}
				}

				if( string.IsNullOrEmpty( atlasPath ) == false )
				{
					if( atlasPath != complexRectangle.SpriteSet.Path )
					{
						GUI.backgroundColor = Color.yellow ;
					}
					else
					{
						GUI.backgroundColor = Color.white ;
					}
					if( GUILayout.Button( "Set", GUILayout.Width( 50f ) ) == true || atlasTextureRefresh == true )
					{
						Undo.RecordObject( complexRectangle, "UIAtlasSprite Path : Change" ) ;	// アンドウバッファに登録
						complexRectangle.SpriteSet.Path = atlasPath ;
						EditorUtility.SetDirty( complexRectangle ) ;
					}
					GUI.backgroundColor = Color.white ;
				}
			}
			GUILayout.EndHorizontal() ;

			//-----------------------------------------------------
			
			// 一覧から選択出来るようにする


//			if( tTarget.atlasSprite != null && tTarget.atlasSprite.texture != null )
//			{
//				GUI.backgroundColor = Color.yellow ;
//				if( GUILayout.Button( "Refresh", GUILayout.Width( 140f ) ) == true )
//				{
//					Undo.RecordObject( tTarget, "UIAtlasSprite Texture : Change" ) ;	// アンドウバッファに登録
//					RefreshAtlasSprite( tTarget, tTarget.atlasSprite.texture ) ;
//					EditorUtility.SetDirty( tTarget ) ;
//				}
//				GUI.backgroundColor = Color.white ;
//			}

			if( complexRectangle.SpriteSet != null )
			{
				EditorGUILayout.Separator() ;	// 少し区切りスペース

				if( GUILayout.Button( "Reload", GUILayout.Width( 50f ) ) == true || ( complexRectangle.SpriteSet.IsAvailable == false && complexRectangle.SpriteSet.Texture != null && Application.isPlaying == false ) )
				{
					// データに異常が発生しているので自動的に更新する
					Debug.LogWarning( "Atlas を自動的に更新:" + complexRectangle.SpriteSet.Texture.name ) ;
					RefreshAtlasSpriteForComplexRectangle( complexRectangle, complexRectangle.SpriteSet.Texture ) ;
					EditorUtility.SetDirty( complexRectangle ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}

				string[] spriteName = complexRectangle.SpriteSet.GetSpriteNames() ;
				if( spriteName != null && spriteName.Length >  0 )
				{
					// ソートする
					List<string> sortSpriteName = new List<string>() ;

					int i, l = spriteName.Length ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						sortSpriteName.Add( spriteName[ i ] ) ;
					}
					sortSpriteName.Sort() ;
					spriteName = sortSpriteName.ToArray() ;

					if( m_SpriteIndex >= spriteName.Length )
					{
						m_SpriteIndex  = spriteName.Length - 1 ;
					}

					// フレーム番号
					m_SpriteIndex = EditorGUILayout.Popup( "Selected Sprite", m_SpriteIndex, spriteName ) ;

					// 確認用
					EditorGUILayout.ObjectField( "", complexRectangle.SpriteSet[ spriteName[ m_SpriteIndex ] ], typeof( Sprite ), false, GUILayout.Width( 60f ), GUILayout.Height( 60f ) ) ;
				}
			}
		}

		private void RefreshAtlasSpriteForComplexRectangle( UIComplexRectangle complexRectangle, Texture atlasTexture )
		{
			List<Sprite> sprites = new List<Sprite>() ;

			if( atlasTexture != null )
			{
				string path = AssetDatabase.GetAssetPath( atlasTexture.GetInstanceID() ) ;

				// テクスチャからパスを取得してマルチタイプスプライトとしてロードする
				UnityEngine.Object[] objects = AssetDatabase.LoadAllAssetsAtPath( path ) ;

				if( objects != null )
				{
					int i, l = objects.Length ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						if( objects[ i ] is Sprite )
						{
							sprites.Add( objects[ i ] as Sprite ) ;
						}
					}
				}

				if( sprites.Count >  0 )
				{
					// 存在するので更新する

					complexRectangle.SpriteSet.ClearSprites() ;
					complexRectangle.SpriteSet.SetSprites( sprites.ToArray() ) ;
				}
				else
				{
					// 存在しないのでクリアする
					if(	complexRectangle.SpriteSet != null )
					{
						complexRectangle.SpriteSet.ClearSprites() ;
					}
				}
			}
			else
			{
				if(	complexRectangle.SpriteSet != null )
				{
					complexRectangle.SpriteSet.ClearSprites() ;
				}
			}
		}
	}
}

#endif
