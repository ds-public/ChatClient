#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// GridMap のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( GridMap ) ) ]
	public class GridMapInspector : MaskableGraphicWrapperInspector
	{
		/// <summary>
		/// スンスペクター描画
		/// </summary>
		public override void OnInspectorGUI()
		{
			// とりあえずデフォルト
//			DrawDefaultInspector() ;
		
			//--------------------------------------------
		
			// ターゲットのインスタンス
			GridMap tTarget = target as GridMap ;
		
			// Graphic の基本情報を描画する
			DrawBasis( tTarget ) ;

			EditorGUILayout.Separator() ;   // 少し区切りスペース

			// 頂点の横のグリッド
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				GUILayout.Label( "Vertex Horizontal Grid" ) ;

				int tVertexHorizontalGrid = EditorGUILayout.IntField( tTarget.vertexHorizontalGrid ) ;
				if( tVertexHorizontalGrid != tTarget.vertexHorizontalGrid && tVertexHorizontalGrid >  0 )
				{
					Undo.RecordObject( tTarget, "GridMap : Vertex Horizontal Grid Change" ) ;	// アンドウバッファに登録
					tTarget.vertexHorizontalGrid = tVertexHorizontalGrid ;
					EditorUtility.SetDirty( tTarget ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			// 頂点の縦のグリッド
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				GUILayout.Label( "Vertex Vertical Grid" ) ;

				int tVertexVerticalGrid = EditorGUILayout.IntField( tTarget.vertexVerticalGrid ) ;
				if( tVertexVerticalGrid != tTarget.vertexVerticalGrid && tVertexVerticalGrid >  0 )
				{
					Undo.RecordObject( tTarget, "GridMap : Vertex Vertical Grid Change" ) ;	// アンドウバッファに登録
					tTarget.vertexVerticalGrid = tVertexVerticalGrid ;
					EditorUtility.SetDirty( tTarget ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			if( tTarget.texture != null && tTarget.textureHorizontalGrid >  0 && tTarget.textureVerticalGrid >  0 )
			{
				// データ配列
				SerializedObject tSO = new SerializedObject( tTarget ) ;
				SerializedProperty tSP = tSO.FindProperty( "data" ) ;
				if( tSP != null )
				{
					EditorGUILayout.PropertyField( tSP, new GUIContent( "Data" ), true ) ;
				}
				tSO.ApplyModifiedProperties() ;
			}

			// 頂点密度
			GridMap.VertexDensity tVertexDensity = ( GridMap.VertexDensity )EditorGUILayout.EnumPopup( "Vertex Density",  tTarget.vertexDensity ) ;
			if( tVertexDensity != tTarget.vertexDensity )
			{
				Undo.RecordObject( tTarget, "GridMap : Vertex Density Change" ) ;	// アンドウバッファに登録
				tTarget.vertexDensity = tVertexDensity ;
				EditorUtility.SetDirty( tTarget ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}


			// テクスチャ
			Texture2D tTexture = EditorGUILayout.ObjectField( "Texture", tTarget.texture, typeof( Texture2D ), false ) as Texture2D ;
			if( tTexture != tTarget.texture )
			{
				Undo.RecordObject( tTarget, "GridMap : Texture Change" ) ;	// アンドウバッファに登録
				tTarget.texture = tTexture ;
				EditorUtility.SetDirty( tTarget ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			if( tTarget.texture != null )
			{
				// ＵＶの横のグリッド
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					GUILayout.Label( "Texture Horizontal Grid" ) ;

					int tTextureHorizontalGrid = EditorGUILayout.IntField( tTarget.textureHorizontalGrid ) ;
					if( tTextureHorizontalGrid != tTarget.textureHorizontalGrid && tTextureHorizontalGrid >= 0 )
					{
						Undo.RecordObject( tTarget, "GridMap : Texture Horizontal Grid Change" ) ;	// アンドウバッファに登録
						tTarget.textureHorizontalGrid = tTextureHorizontalGrid ;
						EditorUtility.SetDirty( tTarget ) ;
//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				// ＵＶの縦のグリッド
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					GUILayout.Label( "Texture Vertical Grid" ) ;

					int tTextureVerticalGrid = EditorGUILayout.IntField( tTarget.textureVerticalGrid ) ;
					if( tTextureVerticalGrid != tTarget.textureVerticalGrid && tTextureVerticalGrid >= 0 )
					{
						Undo.RecordObject( tTarget, "GridMap : Texture Vertical Grid Change" ) ;	// アンドウバッファに登録
						tTarget.textureVerticalGrid = tTextureVerticalGrid ;
						EditorUtility.SetDirty( tTarget ) ;
//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				// パディング値
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					GUILayout.Label( "Texture Grid Padding" ) ;

					int tTextureGridPadding = EditorGUILayout.IntField( tTarget.textureGridPadding ) ;
					if( tTextureGridPadding != tTarget.textureGridPadding && tTextureGridPadding >= 0 )
					{
						Undo.RecordObject( tTarget, "GridMap : Texture Grid Padding Change" ) ;	// アンドウバッファに登録
						tTarget.textureGridPadding = tTextureGridPadding ;
						EditorUtility.SetDirty( tTarget ) ;
//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}
			else
			{
				EditorGUILayout.HelpBox( GetMessage( "SetTexture" ), MessageType.Warning, true ) ;
			}


			EditorGUILayout.Separator() ;   // 少し区切りスペース

			// トランジションタイプ
			GridMap.TransitionTypes tTransitionType = ( GridMap.TransitionTypes )EditorGUILayout.EnumPopup( "Transition Type",  tTarget.transitionType ) ;
			if( tTransitionType != tTarget.transitionType )
			{
				Undo.RecordObject( tTarget, "GridMap : Transition Type Change" ) ;	// アンドウバッファに登録
				tTarget.transitionType = tTransitionType ;
				EditorUtility.SetDirty( tTarget ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			if( tTarget.transitionType != GridMap.TransitionTypes.None )
			{
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					GUILayout.Label( "Transition Factor" ) ;

					float tTransitionFactor = EditorGUILayout.Slider( tTarget.transitionFactor, 0.0f, 2.0f ) ;
					if( tTransitionFactor != tTarget.transitionFactor )
					{
						Undo.RecordObject( tTarget, "GridMap : Transition Factor Change" ) ;	// アンドウバッファに登録
						tTarget.transitionFactor = tTransitionFactor ;
						EditorUtility.SetDirty( tTarget ) ;
//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				GUILayout.BeginHorizontal() ;	// 横並び
				{
					GUILayout.Label( "Transition Intensity" ) ;

					float tTransitionIntensity = EditorGUILayout.Slider( tTarget.transitionIntensity, 0.0f, 1.0f ) ;
					if( tTransitionIntensity != tTarget.transitionIntensity )
					{
						Undo.RecordObject( tTarget, "GridMap : Transition Intensity Change" ) ;	// アンドウバッファに登録
						tTarget.transitionIntensity = tTransitionIntensity ;
						EditorUtility.SetDirty( tTarget ) ;
//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				GUILayout.BeginHorizontal() ;	// 横並び
				{
					bool tTransitionReverse = EditorGUILayout.Toggle( tTarget.transitionReverse, GUILayout.Width( 16f ) ) ;
					if( tTransitionReverse != tTarget.transitionReverse )
					{
						Undo.RecordObject( tTarget, "GridMap : Transition Reverse Change" ) ;	// アンドウバッファに登録
						tTarget.transitionReverse = tTransitionReverse ;
						EditorUtility.SetDirty( tTarget ) ;
//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
					GUILayout.Label( "Transition Reverse" ) ;
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}
		}

		//--------------------------------------------------------------------------

		private Dictionary<string,string> mJapanese_Message = new Dictionary<string, string>()
		{
			{ "SetTexture",				"テクスチャを設定してください" },
			{ "InteractionNone",		"UIInteraction クラスが必要です" },
			{ "HSB_CanvasGroupNone",	"Horizontal Scrollbar に CanvasGroup クラスが必要です" },
			{ "VSB_CanvasGroupNone",	"Vertical Scrollbar に CanvasGroup クラスが必要です" },
		} ;
		private Dictionary<string,string> mEnglish_Message = new Dictionary<string, string>()
		{
			{ "SetTexture",				"Please set the texture" },
			{ "InteractionNone",		"'UIInteraction' is necessary" },
			{ "HSB_CanvasGroupNone",	"'CanvasGroup' is necessary to horizontal scrollbar" },
			{ "VSB_CanvasGroupNone",	"'CanvasGroup' is necessary to vertical scrollbar" },
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
