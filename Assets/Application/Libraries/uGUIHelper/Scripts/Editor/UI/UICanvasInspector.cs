#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UICanvas のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UICanvas ) ) ]
	public class UICanvasInspector : Editor
	{
//		override protected void DrawInspectorGUI()
		public override void OnInspectorGUI()
		{
			UICanvas tTarget = target as UICanvas ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------
		
			// キャンバスグループを有効にするかどうか
			DrawCanvasGroup( tTarget ) ;
		}

		// キャンバスグループの設定項目を描画する
		private void DrawCanvasGroup( UICanvas tTarget )
		{
			// キャンバスグループを有効にするかどうか

			EditorGUILayout.Separator() ;   // 少し区切りスペース
#if false
			UICanvas.AutomaticRenderMode tRenderMode = ( UICanvas.AutomaticRenderMode )EditorGUILayout.EnumPopup( "Effect Mode", tTarget.renderMode ) ;
			if( tRenderMode != tTarget.renderMode )
			{
				Undo.RecordObject( tTarget, "UICanvas : Render Mode Change" ) ;	// アンドウバッファに登録
				tTarget.renderMode = tRenderMode ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			float tWidth = EditorGUILayout.FloatField( " Width", tTarget.width /*, GUILayout.Width( 120f ) */ ) ;
			if( tWidth != tTarget.width )
			{
				Undo.RecordObject( tTarget, "UICanvas : Width Change" ) ;	// アンドウバッファに登録
				tTarget.width = tWidth ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			float tHeight = EditorGUILayout.FloatField( " Height", tTarget.height /*, GUILayout.Width( 120f ) */ ) ;
			if( tHeight != tTarget.height )
			{
				Undo.RecordObject( tTarget, "UICanvas : Height Change" ) ;	// アンドウバッファに登録
				tTarget.height = tHeight ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			int tDepth = EditorGUILayout.IntField( " Depth", tTarget.depth /*, GUILayout.Width( 120f ) */ ) ;
			if( tDepth != tTarget.depth )
			{
				Undo.RecordObject( tTarget, "UICanvas : Depth Change" ) ;	// アンドウバッファに登録
				tTarget.depth = tDepth ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			Camera tRenderCamera = EditorGUILayout.ObjectField( " Render Camera", tTarget.RenderCamera, typeof( Camera ), true ) as Camera ;
			if( tRenderCamera != tTarget.RenderCamera )
			{
				Undo.RecordObject( tTarget, "UICanvas : Render Camera Change" ) ;	// アンドウバッファに登録
				tTarget.RenderCamera = tRenderCamera ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			float tVrDistance = EditorGUILayout.FloatField( " VR Distance", tTarget.vrDistance /*, GUILayout.Width( 120f ) */ ) ;
			if( tVrDistance != tTarget.vrDistance )
			{
				Undo.RecordObject( tTarget, "UICanvas : VR Distance Change" ) ;	// アンドウバッファに登録
				tTarget.vrDistance = tVrDistance ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			float tVrScale = EditorGUILayout.FloatField( " VR Scale", tTarget.vrScale /*, GUILayout.Width( 120f ) */ ) ;
			if( tVrScale != tTarget.vrScale )
			{
				Undo.RecordObject( tTarget, "UICanvas : VR Scale Change" ) ;	// アンドウバッファに登録
				tTarget.vrScale = tVrScale ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			EditorGUILayout.Separator() ;	// 少し区切りスペース
#endif
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool tIsOverlay = EditorGUILayout.Toggle( tTarget.IsOverlay, GUILayout.Width( 16f ) ) ;
				if( tIsOverlay != tTarget.IsOverlay )
				{
					Undo.RecordObject( tTarget, "UICanvas : Overlay Change" ) ;	// アンドウバッファに登録
					tTarget.IsOverlay = tIsOverlay ;
					EditorUtility.SetDirty( tTarget ) ;
				}
				GUILayout.Label( "Overlay" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了


			EditorGUILayout.Separator() ;	// 少し区切りスペース

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool isCanvasGroup = EditorGUILayout.Toggle( tTarget.IsCanvasGroup, GUILayout.Width( 16f ) ) ;
				if( isCanvasGroup != tTarget.IsCanvasGroup )
				{
					Undo.RecordObject( tTarget, "UICanvas : Canvas Group Change" ) ;	// アンドウバッファに登録
					tTarget.IsCanvasGroup = isCanvasGroup ;
					EditorUtility.SetDirty( tTarget ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( "Canvas Group" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			if( tTarget.IsCanvasGroup == true )
			{
				EditorGUIUtility.labelWidth =  60f ;
				EditorGUIUtility.fieldWidth =  40f ;

				// スライダーでアルファをコントロール出来るようにする
				float alpha = EditorGUILayout.Slider( "Alpha", tTarget.GetCanvasGroup().alpha, 0.0f, 1.0f ) ;
				if( alpha != tTarget.GetCanvasGroup().alpha )
				{
					Undo.RecordObject( tTarget, "UIView : Alpha Change" ) ;	// アンドウバッファに登録
					tTarget.GetCanvasGroup().alpha = alpha ;
					EditorUtility.SetDirty( tTarget ) ;
				}

				EditorGUIUtility.labelWidth = 116f ;
				EditorGUIUtility.fieldWidth =  40f ;

				// スライダーでアルファをコントロール出来るようにする
				float tDisableRaycastUnderAlpha = EditorGUILayout.Slider( "Disable Raycast Under Alpha", tTarget.DisableRaycastUnderAlpha, 0.0f, 1.0f ) ;
				if( tDisableRaycastUnderAlpha != tTarget.DisableRaycastUnderAlpha )
				{
					Undo.RecordObject( tTarget, "UIView : Disable Raycast Under Alpha Change" ) ;	// アンドウバッファに登録
					tTarget.DisableRaycastUnderAlpha = tDisableRaycastUnderAlpha ;
					EditorUtility.SetDirty( tTarget ) ;
				}

			}
		}
	}
}

#endif
