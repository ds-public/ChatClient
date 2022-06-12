#if UNITY_EDITOR

using UnityEngine ;
using UnityEngine.UI ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIFlipper のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIFlipper ) ) ]
	public class UIFlipperInspector : Editor
	{
		// スンスペクター描画
		public override void OnInspectorGUI()
		{
			// とりあえずデフォルト
	//		DrawDefaultInspector() ;
		
			//--------------------------------------------

			// ターゲットのインスタンス
			UIFlipper tTarget = target as UIFlipper ;
		
			EditorGUILayout.Separator() ;	// 少し区切りスペース

			// 識別子
			GUI.backgroundColor = Color.cyan ;
			string tIdentity = EditorGUILayout.TextField( "Identity",  tTarget.Identity ) ;
			GUI.backgroundColor = Color.white ;
			if( tIdentity != tTarget.Identity )
			{
				Undo.RecordObject( tTarget, "UIFlipper : Identity Change" ) ;	// アンドウバッファに登録
				tTarget.Identity = tIdentity ;
				EditorUtility.SetDirty( tTarget ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}
		
			// ディレイ
			float tDelay = EditorGUILayout.FloatField( "Delay",  tTarget.Delay ) ;
			if( tDelay != tTarget.Delay )
			{
				Undo.RecordObject( tTarget, "UIFlipper : Delay Change" ) ;	// アンドウバッファに登録
				tTarget.Delay = tDelay ;
				EditorUtility.SetDirty( tTarget ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			//------------------------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			if( tTarget.GetComponent<Image>() == null )
			{
				EditorGUILayout.HelpBox( GetMessage( "ImageNone" ), MessageType.Warning, true ) ;		
			}

			// ワイドモードを有効にする
	//		bool tWideMode = EditorGUIUtility.wideMode ;
	//		EditorGUIUtility.wideMode = true ;


			//--------------------------------------------------------------------

			// 一番肝心なスプライトアニメーションファイル
			UISpriteAnimation tSpriteAnimation = EditorGUILayout.ObjectField( "Sprite Animation", tTarget.SpriteAnimation, typeof( UISpriteAnimation ), false ) as UISpriteAnimation ;
			if( tSpriteAnimation != tTarget.SpriteAnimation )
			{
				Undo.RecordObject( tTarget, "UIFlipper : Sprite Animation Change " ) ;	// アンドウバッファに登録
				tTarget.SpriteAnimation = tSpriteAnimation ;
				EditorUtility.SetDirty( tTarget ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			if( tTarget.enabled == true && tTarget.SpriteAnimation != null )
			{
				EditorGUILayout.Separator() ;	// 少し区切りスペース

				GUILayout.BeginHorizontal() ;	// 横並び
				{
					// チェック
					GUILayout.Label( "Checker (Editor Only)", GUILayout.Width( 150f ) ) ;

					bool tIsChecker = EditorGUILayout.Toggle( tTarget.IsChecker ) ;
					if( tIsChecker != tTarget.IsChecker )
					{
						if( tIsChecker == true )
						{
							UIFlipper[] tFlipperList = tTarget.gameObject.GetComponents<UIFlipper>() ;
							if( tFlipperList != null && tFlipperList.Length >  0 )
							{
								for( int i  = 0 ; i <  tFlipperList.Length ; i ++ )
								{
									if( tFlipperList[ i ] != tTarget )
									{
										if( tFlipperList[ i ].IsChecker == true )
										{
											tFlipperList[ i ].IsChecker  = false ;
										}
									}
								}
							}
						}


						Undo.RecordObject( tTarget, "UIFlipper : Checker Change" ) ;	// アンドウバッファに登録
						tTarget.IsChecker = tIsChecker ;
						EditorUtility.SetDirty( tTarget ) ;
//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				if( tTarget.IsChecker == true )
				{
					GUILayout.BeginHorizontal() ;	// 横並び
					{
						int tCheckFactor = EditorGUILayout.IntSlider( tTarget.CheckFactor, 0, tTarget.SpriteAnimation.Length - 1 ) ;
						if( tCheckFactor != tTarget.CheckFactor )
						{
							Undo.RecordObject( tTarget, "UIFlipper : Check Factor Change " ) ;	// アンドウバッファに登録
							tTarget.CheckFactor = tCheckFactor ;
							EditorUtility.SetDirty( tTarget ) ;
//							UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;

	/*						Image tImage = tTarget.GetComponent<Image>() ;
							if( tImage != null )
							{
								EditorUtility.SetDirty( tImage ) ;
							}*/
						}
					}
					GUILayout.EndHorizontal() ;		// 横並び終了
				}


				EditorGUILayout.Separator() ;	// 少し区切りスペース

				// Begin
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					GUILayout.Label( "Begin", GUILayout.Width( 60f ) ) ;
					int tBegin = EditorGUILayout.IntSlider( tTarget.Begin, 0, tTarget.SpriteAnimation.Length - 1 ) ;
					if( tBegin != tTarget.Begin )
					{
						Undo.RecordObject( tTarget, "UIFlipper : Begin Change " ) ;	// アンドウバッファに登録
						tTarget.Begin = tBegin ;
						EditorUtility.SetDirty( tTarget ) ;
//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				// end
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					GUILayout.Label( "End", GUILayout.Width( 60f ) ) ;
					int tEnd = EditorGUILayout.IntSlider( tTarget.End, 0, tTarget.SpriteAnimation.Length - 1 ) ;
					if( tEnd != tTarget.End )
					{
						Undo.RecordObject( tTarget, "UIFlipper : End Change " ) ;	// アンドウバッファに登録
						tTarget.End = tEnd ;
						EditorUtility.SetDirty( tTarget ) ;
//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}

			//--------------------------------------------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// バック
				GUILayout.Label( "Back", GUILayout.Width( 116f ) ) ;

				bool tBack = EditorGUILayout.Toggle( tTarget.Back ) ;
				if( tBack != tTarget.Back )
				{
					Undo.RecordObject( tTarget, "UIFlipper : Back Change" ) ;	// アンドウバッファに登録
					tTarget.Back = tBack ;
					EditorUtility.SetDirty( tTarget ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了


			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// ループ
				GUILayout.Label( "Loop", GUILayout.Width( 116f ) ) ;

				bool tLoop = EditorGUILayout.Toggle( tTarget.Loop ) ;
				if( tLoop != tTarget.Loop )
				{
					Undo.RecordObject( tTarget, "UIFlipper : Loop Change" ) ;	// アンドウバッファに登録
					tTarget.Loop = tLoop ;
					EditorUtility.SetDirty( tTarget ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			if( tTarget.Loop == true )
			{
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					// リバース
					GUILayout.Label( "Reverse", GUILayout.Width( 116f ) ) ;

					bool tReverse = EditorGUILayout.Toggle( tTarget.Reverse ) ;
					if( tReverse != tTarget.Reverse )
					{
						Undo.RecordObject( tTarget, "UIFlipper : Reverse Change" ) ;	// アンドウバッファに登録
						tTarget.Reverse = tReverse ;
						EditorUtility.SetDirty( tTarget ) ;
//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}

			// スピード
			float tSpeed = EditorGUILayout.FloatField( "Speed",  tTarget.Speed ) ;
			if( tSpeed != tTarget.Speed )
			{
				Undo.RecordObject( tTarget, "UIFlipper : Speed Change" ) ;	// アンドウバッファに登録
				tTarget.Speed = tSpeed ;
				EditorUtility.SetDirty( tTarget ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// イグノアタイムスケール
				GUILayout.Label( "Ignore Time Scale", GUILayout.Width( 116f ) ) ;

				bool tIgnoreTimeScale = EditorGUILayout.Toggle( tTarget.IgnoreTimeScale ) ;
				if( tIgnoreTimeScale != tTarget.IgnoreTimeScale )
				{
					Undo.RecordObject( tTarget, "UIFlipper : Ignore Time Scale Change" ) ;	// アンドウバッファに登録
					tTarget.IgnoreTimeScale = tIgnoreTimeScale ;
					EditorUtility.SetDirty( tTarget ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// プレイオンアウェイク
				GUILayout.Label( "Play On Awake", GUILayout.Width( 116f ) ) ;

				bool tPlayOnAwake = EditorGUILayout.Toggle( tTarget.PlayOnAwake ) ;
				if( tPlayOnAwake != tTarget.PlayOnAwake )
				{
					Undo.RecordObject( tTarget, "UIFlipper : Play On Awake Change" ) ;	// アンドウバッファに登録
					tTarget.PlayOnAwake = tPlayOnAwake ;
					EditorUtility.SetDirty( tTarget ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
			}
			GUILayout.EndHorizontal() ;     // 横並び終了


			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// デストロイアットエンド
				GUILayout.Label( "Destroy At End", GUILayout.Width( 116f ) ) ;

				bool tDestroyAtEnd = EditorGUILayout.Toggle( tTarget.DestroyAtEnd ) ;
				if( tDestroyAtEnd != tTarget.DestroyAtEnd )
				{
					Undo.RecordObject( tTarget, "UIFlipper : Destroy At End Change" ) ;	// アンドウバッファに登録
					tTarget.DestroyAtEnd = tDestroyAtEnd ;
					EditorUtility.SetDirty( tTarget ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了


			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// デストロイアットエンド
				GUILayout.Label( "Auto Resize", GUILayout.Width( 116f ) ) ;

				bool tAutoResize = EditorGUILayout.Toggle( tTarget.AutoResize ) ;
				if( tAutoResize != tTarget.AutoResize )
				{
					Undo.RecordObject( tTarget, "UIFlipper : Auto Resize Change" ) ;	// アンドウバッファに登録
					tTarget.AutoResize = tAutoResize ;
					EditorUtility.SetDirty( tTarget ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// イズプレイング
				GUILayout.Label( "Is Playing", GUILayout.Width( 116f ) ) ;

				EditorGUILayout.Toggle( tTarget.IsPlaying ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			EditorGUILayout.Separator() ;   // 少し区切りスペース


			// デリゲートの設定状況
			SerializedObject tSO = new SerializedObject( tTarget ) ;

			SerializedProperty tSP = tSO.FindProperty( "onFinished" ) ;
			if( tSP != null )
			{
				EditorGUILayout.PropertyField( tSP ) ;
			}
			tSO.ApplyModifiedProperties() ;
		}


		//--------------------------------------------------------------------------

		private readonly Dictionary<string,string> mJapanese_Message = new Dictionary<string, string>()
		{
			{ "ImageNone", "Image クラスが必要です" },
		} ;
		private readonly Dictionary<string,string> mEnglish_Message = new Dictionary<string, string>()
		{
			{ "ImageNone", "'Image' is necessary." },
		} ;

		private string GetMessage( string tLabel )
		{
			if( Application.systemLanguage == SystemLanguage.Japanese )
			{
				if( mJapanese_Message.ContainsKey( tLabel ) == false )
				{
					return "指定のラベル名が見つかりません" ;
				}
				return mJapanese_Message[ tLabel ] ;
			}
			else
			{
				if( mEnglish_Message.ContainsKey( tLabel ) == false )
				{
					return "Specifying the label name can not be found" ;
				}
				return mEnglish_Message[ tLabel ] ;
			}
		}
	}
}

#endif
