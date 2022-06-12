#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIRawImage のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIRawImage ) ) ]
	public class UIRawImageInspector : UIViewInspector
	{
		override protected void DrawInspectorGUI()
		{
			UIRawImage tTarget = target as UIRawImage ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------
		
			// マテリアル選択
			DrawMaterial( tTarget ) ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool tAutoCreateDrawableTexture = EditorGUILayout.Toggle( tTarget.AutoCreateDrawableTexture, GUILayout.Width( 16f ) ) ;
				if( tAutoCreateDrawableTexture != tTarget.AutoCreateDrawableTexture )
				{
					Undo.RecordObject( tTarget, "UIRawImage : Auto Create DrawableTexture Change" ) ;	// アンドウバッファに登録
					tTarget.AutoCreateDrawableTexture = tAutoCreateDrawableTexture ;
					EditorUtility.SetDirty( tTarget ) ;
				}
				GUILayout.Label( "Auto Create Drawable Texture" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool tIsFlipVertical = EditorGUILayout.Toggle( tTarget.IsFlipVertical, GUILayout.Width( 16f ) ) ;
				if( tIsFlipVertical != tTarget.IsFlipVertical )
				{
					Undo.RecordObject( tTarget, "UIRawImage : Is Flip Vertical Change" ) ;	// アンドウバッファに登録
					tTarget.IsFlipVertical = tIsFlipVertical ;
					EditorUtility.SetDirty( tTarget ) ;
				}
				GUILayout.Label( "Is Flip Vertical" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		}
	}
}

#endif
