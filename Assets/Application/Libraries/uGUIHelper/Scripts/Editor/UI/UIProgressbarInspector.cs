#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIProgressbar のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIProgressbar ) ) ]
	public class UIProgressbarInspector : UIViewInspector
	{
		override protected void DrawInspectorGUI()
		{
			UIProgressbar view = target as UIProgressbar ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			EditorGUIUtility.labelWidth =  60f ;
			EditorGUIUtility.fieldWidth =  40f ;

			// スライダーでアルファをコントロール出来るようにする
			float value = EditorGUILayout.Slider( "Value", view.Value, 0.0f, 1.0f ) ;
			if( value != view.Value )
			{
				Undo.RecordObject( target, "UIProgressbar : Value Change" ) ;	// アンドウバッファに登録
				view.Value = value ;
				EditorUtility.SetDirty( view ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			EditorGUIUtility.labelWidth = 116f ;
			EditorGUIUtility.fieldWidth =  40f ;

			// 即値
			float number = EditorGUILayout.FloatField( "Number",  view.Number ) ;
			if( number != view.Number )
			{
				Undo.RecordObject( target, "UIProgressbar : Number Change" ) ;	// アンドウバッファに登録
				view.Number = number ;
				EditorUtility.SetDirty( view ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			// 頂点密度
			UIProgressbar.DisplayTypes displayType = ( UIProgressbar.DisplayTypes )EditorGUILayout.EnumPopup( "Display Type",  view.DisplayType ) ;
			if( displayType != view.DisplayType )
			{
				Undo.RecordObject( view, "UIProgressbar : Display Type Change" ) ;	// アンドウバッファに登録
				view.DisplayType = displayType ;
				EditorUtility.SetDirty( view ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}



			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			UIImage scope = EditorGUILayout.ObjectField( "Scope", view.Scope, typeof( UIImage ), true ) as UIImage ;
			if( scope != view.Scope )
			{
				Undo.RecordObject( view, "UIProgressbar : Scope Change" ) ;	// アンドウバッファに登録
				view.Scope = scope ;
				EditorUtility.SetDirty( view ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			UIImage thumb = EditorGUILayout.ObjectField( "Thumb", view.Thumb, typeof( UIImage ), true ) as UIImage ;
			if( thumb != view.Thumb )
			{
				Undo.RecordObject( target, "UIProgressbar : Thumb Change" ) ;	// アンドウバッファに登録
				view.Thumb = thumb ;
				EditorUtility.SetDirty( view ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			UINumberMesh labelMesh = EditorGUILayout.ObjectField( "Label", view.LabelMesh, typeof( UINumberMesh ), true ) as UINumberMesh ;
			if( labelMesh != view.LabelMesh )
			{
				Undo.RecordObject( view, "UIProgressbar : LabelMesh Change" ) ;	// アンドウバッファに登録
				view.LabelMesh = labelMesh ;
				EditorUtility.SetDirty( view ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}
		}
	}
}

#endif
