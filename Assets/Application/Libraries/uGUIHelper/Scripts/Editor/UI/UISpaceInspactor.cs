#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UICanvas のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UISpace ) ) ]
	public class UISpaceInspector : UIViewInspector
	{
		override protected void DrawInspectorGUI()
		{
			UISpace tTarget = target as UISpace ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------
		
			// キャンバスグループを有効にするかどうか
			DrawCanvasGroup( tTarget ) ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			GUI.backgroundColor = Color.magenta ;
			Camera tTargetCamera = EditorGUILayout.ObjectField( "Target Camera", tTarget.TargetCamera, typeof( Camera ), true ) as Camera ;
			GUI.backgroundColor = Color.white ;
			if( tTargetCamera != tTarget.TargetCamera )
			{
				Undo.RecordObject( tTarget, "UISpace : Target Camera Change" ) ;	// アンドウバッファに登録
				tTarget.TargetCamera = tTargetCamera ;
				EditorUtility.SetDirty( tTarget ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			if( tTarget.TargetCamera != null )
			{
				Vector2 tCameraOffset = EditorGUILayout.Vector2Field( "Camera Offset", tTarget.CameraOffset ) ;
				if( tCameraOffset.Equals( tTarget.CameraOffset ) == false )
				{
					Undo.RecordObject( tTarget, "UISpace : Camera Offset Change" ) ;	// アンドウバッファに登録
					tTarget.CameraOffset = tCameraOffset ;
					EditorUtility.SetDirty( tTarget ) ;
				}
			}


			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool tFlexibleFieldOfView = EditorGUILayout.Toggle( tTarget.FlexibleFieldOfView, GUILayout.Width( 16f ) ) ;
				if( tFlexibleFieldOfView != tTarget.FlexibleFieldOfView )
				{
					Undo.RecordObject( tTarget, "UISpace : Flexible Field Of View Change" ) ;	// アンドウバッファに登録
					tTarget.FlexibleFieldOfView = tFlexibleFieldOfView ;
					EditorUtility.SetDirty( tTarget ) ;
				}
				GUILayout.Label( "Flexible Field Of View" ) ;

				if( tTarget.FlexibleFieldOfView == true )
				{
					float tBasisHeight = EditorGUILayout.FloatField( "Basis Height", tTarget.BasisHeight ) ;
					if( tBasisHeight != tTarget.BasisHeight )
					{
						Undo.RecordObject( tTarget, "UISpace : Basis Height Change" ) ;	// アンドウバッファに登録
						tTarget.BasisHeight = tBasisHeight ;
						EditorUtility.SetDirty( tTarget ) ;
					}
				}

			}
			GUILayout.EndHorizontal() ;		// 横並び終了


			//----------------------------------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool tRenderTextureEnabled = EditorGUILayout.Toggle( tTarget.RenderTextureEnabled, GUILayout.Width( 16f ) ) ;
				if( tRenderTextureEnabled != tTarget.RenderTextureEnabled )
				{
					Undo.RecordObject( tTarget, "UISpace : Render Texture Enabled Change" ) ;	// アンドウバッファに登録
					tTarget.RenderTextureEnabled = tRenderTextureEnabled ;
					EditorUtility.SetDirty( tTarget ) ;
				}
				GUILayout.Label( "Render Texture Enabled" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			//----------------------------------------------------------

			if( tTarget.RenderTextureEnabled == true )
			{
				EditorGUILayout.Separator() ;	// 少し区切りスペース

				EditorGUIUtility.labelWidth =  60f ;
				EditorGUIUtility.fieldWidth =  40f ;

				GUILayout.BeginHorizontal() ;	// 横並び
				{
					bool tIsMask = EditorGUILayout.Toggle( tTarget.ImageMask, GUILayout.Width( 16f ) ) ;
					if( tIsMask != tTarget.ImageMask )
					{
						Undo.RecordObject( tTarget, "UISpace : Mask Change" ) ;	// アンドウバッファに登録
						tTarget.ImageMask = tIsMask ;
						EditorUtility.SetDirty( tTarget ) ;
					}
					GUILayout.Label( "Mask" ) ;

					bool tIsInversion = EditorGUILayout.Toggle( tTarget.ImageInversion, GUILayout.Width( 16f ) ) ;
					if( tIsInversion != tTarget.ImageInversion )
					{
						Undo.RecordObject( tTarget, "UISpace : Image Inversion Change" ) ;	// アンドウバッファに登録
						tTarget.ImageInversion = tIsInversion ;
						EditorUtility.SetDirty( tTarget ) ;
					}
					GUILayout.Label( "Inversion" ) ;
				}
				GUILayout.EndHorizontal() ;		// 横並び終了


				GUILayout.BeginHorizontal() ;	// 横並び
				{
					bool tIsShadow = EditorGUILayout.Toggle( tTarget.ImageShadow, GUILayout.Width( 16f ) ) ;
					if( tIsShadow != tTarget.IsShadow )
					{
						Undo.RecordObject( tTarget, "UISpace : Shadow Change" ) ;	// アンドウバッファに登録
						tTarget.ImageShadow = tIsShadow ;
						EditorUtility.SetDirty( tTarget ) ;
					}
					GUILayout.Label( "Shadow" ) ;

					bool tIsOutline = EditorGUILayout.Toggle( tTarget.ImageOutline, GUILayout.Width( 16f ) ) ;
					if( tIsOutline != tTarget.ImageOutline )
					{
						Undo.RecordObject( tTarget, "UISpace : Outline Change" ) ;	// アンドウバッファに登録
						tTarget.ImageOutline = tIsOutline ;
						EditorUtility.SetDirty( tTarget ) ;
					}
					GUILayout.Label( "Outline" ) ;

					bool tIsGradient = EditorGUILayout.Toggle( tTarget.ImageGradient, GUILayout.Width( 16f ) ) ;
					if( tIsGradient != tTarget.ImageGradient )
					{
						Undo.RecordObject( tTarget, "UISpace : Gradient Change" ) ;	// アンドウバッファに登録
						tTarget.ImageGradient = tIsGradient ;
						EditorUtility.SetDirty( tTarget ) ;
					}
					GUILayout.Label( "Gradient" ) ;
				}
				GUILayout.EndHorizontal() ;     // 横並び終了

				EditorGUIUtility.labelWidth = 116f ;
				EditorGUIUtility.fieldWidth =  40f ;
			}

		}
	}
}

#endif
