#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIImage のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIImage ) ) ]
	public class UIImageInspector : UIViewInspector
	{
		override protected void DrawInspectorGUI()
		{
			UIImage view = target as UIImage ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------
			
			// アトラススプライトの表示
			DrawAtlas( view ) ;

			// Flipper の追加と削除
			DrawFlipper( view ) ;

			// マテリアル選択
			DrawMaterial( view ) ;

			//----------------------------------------------------------

			bool isApplyColorToChildren = EditorGUILayout.Toggle( "Is Apply Color To Children", view.IsApplyColorToChildren ) ;
			if( isApplyColorToChildren != view.IsApplyColorToChildren )
			{
				Undo.RecordObject( view, "UIImage : Is Apply Color To Children Change" ) ;	// アンドウバッファに登録
				view.IsApplyColorToChildren = isApplyColorToChildren ;
				EditorUtility.SetDirty( view ) ;
			}

			if( view.IsApplyColorToChildren == true )
			{
				// 影響色
				Color effectiveColor = new Color( view.EffectiveColor.r, view.EffectiveColor.g, view.EffectiveColor.b, view.EffectiveColor.a ) ;
				effectiveColor = EditorGUILayout.ColorField( "Effective Color", effectiveColor ) ;
				if( effectiveColor.Equals( view.EffectiveColor ) == false )
				{
					Undo.RecordObject( view, "UIImage : Effective Color Change" ) ;	// アンドウバッファに登録
					view.EffectiveColor = effectiveColor ;
					EditorUtility.SetDirty( view ) ;
				}
			}

			//----------------------------------------------------------

			// バックキー
			bool backKeyEnabled = EditorGUILayout.Toggle( "Back Key Enabled", view.BackKeyEnabled ) ;
			if( backKeyEnabled != view.BackKeyEnabled )
			{
				Undo.RecordObject( view, "UIImage : Back Key Enabled Change" ) ;	// アンドウバッファに登録
				view.BackKeyEnabled = backKeyEnabled ;
				EditorUtility.SetDirty( view ) ;
			}
		}
	}
}

#endif
