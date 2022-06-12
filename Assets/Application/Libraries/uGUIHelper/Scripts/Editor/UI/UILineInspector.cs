#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UILine のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UILine ) ) ]
	public class UILineInspector : UIViewInspector
	{
		/// <summary>
		/// スンスペクター描画
		/// </summary>
		override protected void DrawInspectorGUI()
		{
			// ターゲットのインスタンス
			UILine tTarget = target as UILine ;
		
			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------
			
			// マテリアル選択
			DrawMaterial( tTarget ) ;

			//-------------------------------------------------------------------

			EditorGUILayout.Separator() ;   // 少し区切りスペース

			bool trailEnabled = EditorGUILayout.Toggle( "Trail Enabled", tTarget.TrailEnabled ) ;
			if( trailEnabled != tTarget.TrailEnabled )
			{
				Undo.RecordObject( tTarget, "UILine : Trail Enabled Change" ) ;	// アンドウバッファに登録
				tTarget.TrailEnabled = trailEnabled ;
				EditorUtility.SetDirty( tTarget ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			if( tTarget.TrailEnabled == true )
			{
				// 頂点が消えるまでの時間
				float trailKeepTime = EditorGUILayout.FloatField( " Trail Keep Time", tTarget.TrailKeepTime ) ;
				if( trailKeepTime != tTarget.TrailKeepTime )
				{
					Undo.RecordObject( tTarget, "UILine : Trail Keep Time Change" ) ;	// アンドウバッファに登録
					tTarget.TrailKeepTime = trailKeepTime ;
					EditorUtility.SetDirty( tTarget ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
			}

/*			bool tAutoSizeFitting = EditorGUILayout.Toggle( "Auto Size Fitting", tTarget.autoSizeFitting ) ;
			if( tAutoSizeFitting != tTarget.autoSizeFitting )
			{
				Undo.RecordObject( tTarget, "UIImageNumber : Auto Size Fitting Change" ) ;	// アンドウバッファに登録
				tTarget.autoSizeFitting = tAutoSizeFitting ;
				EditorUtility.SetDirty( tTarget ) ;
			}*/
		}
	}
}

#endif
