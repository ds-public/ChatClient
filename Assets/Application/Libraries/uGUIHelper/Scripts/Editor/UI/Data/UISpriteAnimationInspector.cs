#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UISpriteAnimation のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UISpriteAnimation ) ) ]
	public class UISpriteAnimationInspector : Editor
	{
		private bool	m_Confirm = false ;
		private Texture m_TemporaryTexture = null ;


		// スンスペクター描画
		public override void OnInspectorGUI()
		{
			// とりあえずデフォルト
	//		DrawDefaultInspector() ;

			//-----------------------------------------------------

			// ターゲットのインスタンス
			UISpriteAnimation spriteAnimation = target as UISpriteAnimation ;
		
			//-----------------------------------------------------
		
			EditorGUILayout.Separator() ;	// 少し区切りスペース
		

			Texture texture = EditorGUILayout.ObjectField( "Atlas Sprite", spriteAnimation.Texture, typeof( Texture ), false ) as Texture ;

			bool execute = false ;

			if( m_Confirm == false )
			{
				// 非確認中
				if( texture != null )
				{
					// 設定
					if( spriteAnimation.Texture == null )
					{
						m_Confirm = false ;	// 確認無し
						m_TemporaryTexture = texture ;

						execute = true ;	// 即時設定
					}
					else
					{
						// 既にテクスチャは設定済み
						if( texture != spriteAnimation.Texture )
						{
							// １フレーム後に本当に変更して良いか確認する
							m_Confirm = true ;	// 確認有り
							m_TemporaryTexture = texture ;

							execute = false ;	// 確認設定
						}
						else
						{
							// 同じである場合は無視する
							m_Confirm = false ;
							m_TemporaryTexture = null ;

							execute = false ;
						}
					}
				}
				else
				{
					// 消去
					if( spriteAnimation.Texture != null )
					{
						// 既にテクスチャは設定済み
						m_Confirm = true ;	// 確認有り
						m_TemporaryTexture = null ;

						execute = false ;	// 確認設定
					}
					else
					{
						// 同じである場合は無視する
						m_Confirm = false ;
						m_TemporaryTexture = null ;

						execute = false ;
					}
				}
			}

			if( m_Confirm == true )
			{
				// 変更確認中
				string message = GetMessage( "ChangeOK?" ) ;
				GUILayout.Label( message ) ;
				GUILayout.BeginHorizontal() ;	// 横並び開始
				{
					GUI.backgroundColor = Color.red ;
					if( GUILayout.Button( "OK", GUILayout.Width( 100f ) ) == true )
					{
						execute = true ;
					}
					GUI.backgroundColor = Color.white ;
					if( GUILayout.Button( "Cancel", GUILayout.Width( 100f ) ) == true )
					{
						m_Confirm = false ;
						execute = false ;
					}
				}
				GUILayout.EndHorizontal() ;     // 横並び終了
			}
			
			if( execute == true )
			{
				// 本当に変更する
				Undo.RecordObject( spriteAnimation, "UISpriteAnimation : Change" ) ;	// アンドウバッファに登録

				if( m_TemporaryTexture != null )
				{
					// 設定
					List<Sprite> sprites = new List<Sprite>() ;

					string path = AssetDatabase.GetAssetPath( m_TemporaryTexture.GetInstanceID() ) ;

					// テクスチャからパスを取得してマルチタイプスプライトとしてロードする
					UnityEngine.Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath( path ) ;

					if( allAssets != null )
					{
						int i, l = allAssets.Length ;

						for( i  = 0 ; i <  l ; i ++ )
						{
							if( allAssets[ i ] is UnityEngine.Sprite )
							{
								sprites.Add( allAssets[ i ] as UnityEngine.Sprite ) ;
							}
						}
					}

					if( sprites.Count >  0 )
					{
						spriteAnimation.Texture = m_TemporaryTexture ;
						spriteAnimation.SetSprites( sprites.ToArray() ) ;
					}

					spriteAnimation.Frames.Clear() ;

					m_TemporaryTexture = null ;
				}
				else
				{
					// 消去
					spriteAnimation.Texture = null ;
					spriteAnimation.ClearSprite() ;
					spriteAnimation.Frames.Clear() ;
				}

				EditorUtility.SetDirty( spriteAnimation ) ;

				// 変更確認終了
				m_Confirm = false ;
			}

			//-----------------------------------------------------
		
			if( spriteAnimation.Exist == true )
			{
				DrawFrame( spriteAnimation ) ;
			}
		}


		private int		m_InsertFrameIndex = -1 ;
		private int		m_InsertSpriteIndex = 0 ;
		private float	m_InsertDuration = 0.1f ;

		private bool[]	m_RemoveFrameIndices = null ;
		private bool    m_ExecuteRemove = false ;

		private Vector2 m_Scroll = Vector2.zero ;
	
		private bool	m_Thumbnail = false ;


		protected void DrawFrame( UISpriteAnimation spriteAnimation )
		{
			EditorGUILayout.Separator() ;	// 少し区切りスペース

			float timeScale = EditorGUILayout.FloatField( "Time Scale", spriteAnimation.TimeScale ) ;
			if( timeScale >  0 && timeScale != spriteAnimation.TimeScale )
			{
				Undo.RecordObject( spriteAnimation, "UISpriteAnimation : Time Scale Change" ) ;	// アンドウバッファに登録
				spriteAnimation.TimeScale = timeScale ;
				EditorUtility.SetDirty( spriteAnimation ) ;	// アセットデータベースに変更を通知
			}

			int i, l ;

			l = spriteAnimation.Frames.Count + 1 ;
			string[] frameNumbers = new string[ l ] ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				frameNumbers[ i ] = i.ToString() ;	// フレーム番号(0～)
			}

			// 挿入位置インデックス設定
			if( m_InsertFrameIndex <  0 )
			{
				m_InsertFrameIndex  = l - 1 ;		// 挿入は最終フレーム指定
			}
			else
			if( m_InsertFrameIndex >= l )
			{
				m_InsertFrameIndex  = l - 1 ;
			}

			// スプライトのリストを生成する
			l = spriteAnimation.GetSpriteCount() ;
			string[] spriteNames = new string[ l ] ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( spriteAnimation.GetSprite( i ) != null )
				{
					spriteNames[ i ] = spriteAnimation.GetSprite( i ).name ;
				}
				else
				{
					spriteNames[ i ] = "<Missing>" ;
				}
			}
			if( m_InsertSpriteIndex <  0 )
			{
				m_InsertSpriteIndex  = l - 1 ;	// 最終フレーム指定
			}
			else
			if( m_InsertSpriteIndex >= l )
			{
				m_InsertSpriteIndex  = l - 1 ;
			}

			//----------------------------------------------------

			GUILayout.BeginHorizontal() ;	// 横並び開始
			{
				bool insert = false ;

				// 挿入ボタン
				GUI.backgroundColor = Color.cyan ;
				if( GUILayout.Button( "Insert", GUILayout.Width( 60f ) ) == true )
				{
					insert = true ;
				}
				GUI.backgroundColor = Color.white ;

				// フレーム番号
				m_InsertFrameIndex = EditorGUILayout.Popup( "", m_InsertFrameIndex, frameNumbers, GUILayout.Width( 50f ) ) ;

				// スプライト番号
				m_InsertSpriteIndex = EditorGUILayout.Popup( "", m_InsertSpriteIndex, spriteNames, GUILayout.Width( 120f ) ) ;

				// 表示時間
				m_InsertDuration = EditorGUILayout.FloatField( "", m_InsertDuration, GUILayout.Width( 50f ) ) ;

				if( insert == true && spriteNames[ m_InsertSpriteIndex ] != "<Missing>" && m_InsertDuration >  0 )
				{
					// Frame を追加する
					Undo.RecordObject( spriteAnimation, "UISpriteAnimation : Insert" ) ;	// アンドウバッファに登録
					spriteAnimation.Insert( m_InsertFrameIndex, m_InsertSpriteIndex, m_InsertDuration ) ;
					EditorUtility.SetDirty( spriteAnimation ) ;

					m_InsertFrameIndex ++ ;
					m_InsertSpriteIndex ++ ;

					m_RemoveFrameIndices = null ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
			
			// 以下１つ以上フレームが登録されている場合のみ表示する
			l = spriteAnimation.Frames.Count ;
			if( l >  0 )
			{
				// 削除用フラグ
				if( m_RemoveFrameIndices == null || m_RemoveFrameIndices.Length == 0 )
				{
					m_RemoveFrameIndices = new bool[ l ] ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						m_RemoveFrameIndices[ i ] = false ;
					}
				}
			
				//------------------------------------------------

				// 削除確認
				if( m_RemoveFrameIndices != null )
				{
					if( m_ExecuteRemove == false )
					{
						// 削除ではない

						bool remove = false ;
						for( i  = 0 ; i <  l ; i ++  )
						{
							if( m_RemoveFrameIndices[ i ] == true )
							{
								remove = true ;
								break ;
							}
						}

						if( remove == true )
						{
							GUI.backgroundColor = Color.red ;
							if( GUILayout.Button( "Execute Remove",  GUILayout.Height( 36f ) ) == true )
							{
								m_ExecuteRemove = true ;
							}
							GUI.backgroundColor = Color.white ;
						}
						else
						{
							GUILayout.Label( "",  GUILayout.Height( 36f ) ) ;
						}
					}
					else
					{
						// 削除の最終確認

						string message = GetMessage( "RemoveOK?" ) ;
						GUILayout.Label( message ) ;
						GUILayout.BeginHorizontal() ;	// 横並び開始
						{
							GUI.backgroundColor = Color.red ;
							if( GUILayout.Button( "OK", GUILayout.Width( 100f ) ) == true )
							{
								// 本当に削除する
								Undo.RecordObject( spriteAnimation, "UISpriteAnimation : Remove" ) ;	// アンドウバッファに登録

								List<int> removeFrameIndices = new List<int>() ;
								for( i  = 0 ; i <  l ; i ++ )
								{
									if( m_RemoveFrameIndices[ i ] == true )
									{
										removeFrameIndices.Add( i ) ;
									}
								}

								if( removeFrameIndices.Count >  0 )
								{
									spriteAnimation.Remove( removeFrameIndices.ToArray() ) ;
								}

								EditorUtility.SetDirty( spriteAnimation ) ;
							
								m_RemoveFrameIndices = null ;

								m_ExecuteRemove = false ;
							}
							GUI.backgroundColor = Color.white ;
							if( GUILayout.Button( "Cancel", GUILayout.Width( 100f ) ) == true )
							{
								m_ExecuteRemove = false ;
							}
						}
						GUILayout.EndHorizontal() ;		// 横並び終了

						if( m_RemoveFrameIndices == null )
						{
							// フレームの数が変動しているので以下のリストを表示してはならない
							return ;
						}
					}
				}
					
				//---------------------------

				EditorGUILayout.Separator() ;	// 少し区切りスペース

				GUILayout.BeginHorizontal() ;	// 横並び
				{
					GUILayout.Label( "Frame" ) ;

					m_Thumbnail = EditorGUILayout.Toggle( m_Thumbnail, GUILayout.Width( 16f ) ) ;
					GUILayout.Label( "Thumbnail" ) ;
				}
				GUILayout.EndHorizontal() ;     // 横並び終了

				EditorGUILayout.Separator() ;	// 少し区切りスペース


				// 登録されているフレーム情報を表示する
				m_Scroll = GUILayout.BeginScrollView( m_Scroll ) ;
				{
					for( i  = 0 ; i <  l ; i ++ )
					{
						GUILayout.BeginHorizontal() ;
						{
							// サムネイルアイコン

							// フレーム番号（右寄せ）
							GUILayout.Label( i.ToString(), GUILayout.Width( 30f ) ) ;


							// スプライト選択
							int spriteIndex = EditorGUILayout.Popup( "", spriteAnimation.Frames[ i ].SpriteIndex, spriteNames, GUILayout.Width( 120f ) ) ;	// フィールド名有りタイプ
							if( spriteIndex != spriteAnimation.Frames[ i ].SpriteIndex )
							{
								if( spriteNames[ spriteIndex ] != "<Missing>" )
								{
									Undo.RecordObject( spriteAnimation, "UISpriteAnimation : Change" ) ;	// アンドウバッファに登録
									spriteAnimation.Frames[ i ].SpriteIndex = spriteIndex ;
									EditorUtility.SetDirty( spriteAnimation ) ;
								}
							}


							// 表示時間
							float duration = EditorGUILayout.FloatField( "", spriteAnimation.Frames[ i ].Duration, GUILayout.Width( 50f ) ) ;
							if( duration != spriteAnimation.Frames[ i ].Duration && duration >  0 )
							{
								Undo.RecordObject( spriteAnimation, "UISpriteAnimation : Change" ) ;	// アンドウバッファに登録
								spriteAnimation.Frames[ i ].Duration = duration ;
								EditorUtility.SetDirty( spriteAnimation ) ;
							}


							// 削除用ボタン
							if( m_RemoveFrameIndices[ i ] == false )
							{
								GUI.backgroundColor = Color.white ;
							}
							else
							{
								GUI.backgroundColor = Color.red ;
							}
							if( GUILayout.Button( "Remove", GUILayout.Width( 60f ) ) )
							{
								m_RemoveFrameIndices[ i ] = ! m_RemoveFrameIndices[ i ] ;
							}
							GUI.backgroundColor = Color.white ;
						}
						GUILayout.EndHorizontal() ;
						
						// サムネイル
						if( m_Thumbnail == true )
						{
							GUILayout.BeginHorizontal() ;
							{
								GUILayout.Label( "", GUILayout.Width( 30f ) ) ;
								Sprite sprite = EditorGUILayout.ObjectField( "", spriteAnimation.GetSprite( i ), typeof( Sprite ), false, GUILayout.Width( 60f ), GUILayout.Height( 60f ) ) as Sprite ;
								if( sprite != spriteAnimation.GetSprite( i ) )
								{
								}
							}
							GUILayout.EndHorizontal() ;
						}
					}
				}
				GUILayout.EndScrollView() ;
			}
		}

		//--------------------------------------------------------------------------

		private readonly Dictionary<string,string> m_Japanese_Message = new Dictionary<string, string>()
		{
			{ "ChangeOK?", "本当に変更してもよろしいですか？\n※フレーム情報が全て消去されます" },
			{ "RemoveOK?", "本当に削除してもよろしいですか？" },
		} ;
		private readonly Dictionary<string,string> m_English_Message = new Dictionary<string, string>()
		{
			{ "ChangeOK?", "It does really may be to change ?\n※Frame information all will be erased." },
			{ "RemoveOK?", "It does really may be to remove ?" },
		} ;

		private string GetMessage( string label )
		{
			if( Application.systemLanguage == SystemLanguage.Japanese )
			{
				if( m_Japanese_Message.ContainsKey( label ) == false )
				{
					return "指定のラベル名が見つかりません" ;
				}
				return m_Japanese_Message[ label ].Replace( "\\n", "\n" ) ;
			}
			else
			{
				if( m_English_Message.ContainsKey( label ) == false )
				{
					return "Specifying the label name can not be found" ;
				}
				return m_English_Message[ label ].Replace( "\\n", "\n" )  ;
			}
		}
	}
}

#endif
