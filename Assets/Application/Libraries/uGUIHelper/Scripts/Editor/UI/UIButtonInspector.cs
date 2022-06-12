#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIButton のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIButton ) ) ]
	public class UIButtonInspector : UIViewInspector
	{
		override protected void DrawInspectorGUI()
		{
			UIButton view = target as UIButton ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------

			// アトラススプライトの表示
			DrawAtlas( view ) ;

			// マテリアル選択
			DrawMaterial( view ) ;

			//-------------------------------------------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			UIText label = EditorGUILayout.ObjectField( "Label", view.Label, typeof( UIText ), true ) as UIText ;
			if( label != view.Label )
			{
				Undo.RecordObject( view, "UIButton : Label Change" ) ;	// アンドウバッファに登録
				view.Label = label ;
				EditorUtility.SetDirty( view ) ;
			}

			UIRichText richLabel = EditorGUILayout.ObjectField( "RichLabel", view.RichLabel, typeof( UIRichText ), true ) as UIRichText ;
			if( richLabel != view.RichLabel )
			{
				Undo.RecordObject( view, "UIButton : Rich Label Change" ) ;	// アンドウバッファに登録
				view.RichLabel = richLabel ;
				EditorUtility.SetDirty( view ) ;
			}

			UITextMesh labelMesh = EditorGUILayout.ObjectField( "LabelMesh", view.LabelMesh, typeof( UITextMesh ), true ) as UITextMesh ;
			if( labelMesh != view.LabelMesh )
			{
				Undo.RecordObject( view, "UIButton : Label Mesh Change" ) ;	// アンドウバッファに登録
				view.LabelMesh = labelMesh ;
				EditorUtility.SetDirty( view ) ;
			}

			UIImage icon = EditorGUILayout.ObjectField( "Icon", view.Icon, typeof( UIImage ), true ) as UIImage ;
			if( icon != view.Icon )
			{
				Undo.RecordObject( view, "UIButton : Icon Change" ) ;	// アンドウバッファに登録
				view.Icon = icon ;
				EditorUtility.SetDirty( view ) ;
			}

			bool isApplyColorToChildren = EditorGUILayout.Toggle( "Is Apply Color To Children", view.IsApplyColorToChildren ) ;
			if( isApplyColorToChildren != view.IsApplyColorToChildren )
			{
				Undo.RecordObject( view, "UIButton : Is Apply Color To Children Change" ) ;	// アンドウバッファに登録
				view.IsApplyColorToChildren = isApplyColorToChildren ;
				EditorUtility.SetDirty( view ) ;
			}

			if( view.IsApplyColorToChildren == true )
			{
				bool effeciveColorReplcaing = EditorGUILayout.Toggle( "Effective Color Replacing", view.EffectiveColorReplacing ) ;
				if( effeciveColorReplcaing != view.EffectiveColorReplacing )
				{
					Undo.RecordObject( view, "UIButton : Effective Color Replacing Change" ) ;	// アンドウバッファに登録
					view.EffectiveColorReplacing = effeciveColorReplcaing ;
					EditorUtility.SetDirty( view ) ;
				}

				if( view.EffectiveColorReplacing == false )
				{
					// 影響色
					Color effectiveColor = new Color( view.EffectiveColor.r, view.EffectiveColor.g, view.EffectiveColor.b, view.EffectiveColor.a ) ;
					effectiveColor = EditorGUILayout.ColorField( "Effective Color", effectiveColor ) ;
					if( effectiveColor.Equals( view.EffectiveColor ) == false )
					{
						Undo.RecordObject( view, "UIButton : Effective Color Change" ) ;	// アンドウバッファに登録
						view.EffectiveColor = effectiveColor ;
						EditorUtility.SetDirty( view ) ;
					}
				}
			}

			UIImage disableMask = EditorGUILayout.ObjectField( "DisableMask", view.DisableMask, typeof( UIImage ), true ) as UIImage ;
			if( disableMask != view.DisableMask )
			{
				Undo.RecordObject( view, "UIButton : Disable Mask Change" ) ;	// アンドウバッファに登録
				view.DisableMask = disableMask ;
				EditorUtility.SetDirty( view ) ;
			}

			bool clickTransitionEnabled = EditorGUILayout.Toggle( "Click Transition Enabled", view.ClickTransitionEnabled ) ;
			if( clickTransitionEnabled != view.ClickTransitionEnabled )
			{
				Undo.RecordObject( view, "UIButton : Click Transition Enabled Change" ) ;	// アンドウバッファに登録
				view.ClickTransitionEnabled = clickTransitionEnabled ;
				EditorUtility.SetDirty( view ) ;
			}

			bool waitForTransition = EditorGUILayout.Toggle( "Wait For Transition", view.WaitForTransition ) ;
			if( waitForTransition != view.WaitForTransition )
			{
				Undo.RecordObject( view, "UIButton : Wait For Transition Change" ) ;	// アンドウバッファに登録
				view.WaitForTransition = waitForTransition ;
				EditorUtility.SetDirty( view ) ;
			}

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			bool setPivotToCenter = EditorGUILayout.Toggle( "Set Pivot To Center", view.AutoPivotToCenter ) ;
			if( setPivotToCenter != view.AutoPivotToCenter )
			{
				Undo.RecordObject( view, "UIButton : Set Pivot To Center Change" ) ;	// アンドウバッファに登録
				view.AutoPivotToCenter = setPivotToCenter ;
				EditorUtility.SetDirty( view ) ;
			}

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			UIButtonGroup targetButtonGroup = EditorGUILayout.ObjectField( "ButtonGroup", view.TargetButtonGroup, typeof( UIButtonGroup ), true ) as UIButtonGroup ;
			if( targetButtonGroup != view.TargetButtonGroup )
			{
				Undo.RecordObject( view, "UIButton : Target Button Group Change" ) ;	// アンドウバッファに登録
				view.TargetButtonGroup = targetButtonGroup ;
				EditorUtility.SetDirty( view ) ;
			}

			// バックキー
			bool backKeyEnabled = EditorGUILayout.Toggle( "Back Key Enabled", view.BackKeyEnabled ) ;
			if( backKeyEnabled != view.BackKeyEnabled )
			{
				Undo.RecordObject( view, "UIButton : Back Key Enabled Change" ) ;	// アンドウバッファに登録
				view.BackKeyEnabled = backKeyEnabled ;
				EditorUtility.SetDirty( view ) ;
			}
		}
	}
}

#endif
