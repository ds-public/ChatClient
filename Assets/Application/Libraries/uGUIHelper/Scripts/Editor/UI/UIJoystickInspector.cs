#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIJoystick のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIJoystick ) ) ]
	public class UIJoystickInspector : UIViewInspector
	{
		override protected void DrawInspectorGUI()
		{
			UIJoystick view = target as UIJoystick ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------

			UIImage frame = EditorGUILayout.ObjectField( "Frame", view.Frame, typeof( UIImage ), true ) as UIImage ;
			if( frame != view.Frame )
			{
				Undo.RecordObject( view, "UIJoystick Frame : Change" ) ;	// アンドウバッファに登録
				view.Frame = frame ;
				EditorUtility.SetDirty( view ) ;
			}

			UIImage thumb = EditorGUILayout.ObjectField( "Thumb", view.Thumb, typeof( UIImage ), true ) as UIImage ;
			if( thumb != view.Thumb )
			{
				Undo.RecordObject( view, "UIJoystick Thumb : Change" ) ;	// アンドウバッファに登録
				view.Thumb = thumb ;
				EditorUtility.SetDirty( view ) ;
			}

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool yAxisInversion = EditorGUILayout.Toggle( view.YAxisInversion, GUILayout.Width( 16f ) ) ;
				if( yAxisInversion != view.YAxisInversion )
				{
					Undo.RecordObject( view, "UIJoystick : Y Axis Inversion Change" ) ;	// アンドウバッファに登録
					view.YAxisInversion = yAxisInversion ;
					EditorUtility.SetDirty( view ) ;
				}
				GUILayout.Label( "Y Axis Inversion" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool horizontalFunctionStop = EditorGUILayout.Toggle( view.HorizontalFunctionStop, GUILayout.Width( 16f ) ) ;
				if( horizontalFunctionStop != view.HorizontalFunctionStop )
				{
					Undo.RecordObject( view, "UIJoystick : Horizontal Function Stop Change" ) ;	// アンドウバッファに登録
					view.HorizontalFunctionStop = horizontalFunctionStop ;
					EditorUtility.SetDirty( view ) ;
				}
				GUILayout.Label( "Horizontal Function Stop" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool verticalFunctionStop = EditorGUILayout.Toggle( view.VerticalFunctionStop, GUILayout.Width( 16f ) ) ;
				if( verticalFunctionStop != view.VerticalFunctionStop )
				{
					Undo.RecordObject( view, "UIJoystick : Vertical Function Stop Change" ) ;	// アンドウバッファに登録
					view.VerticalFunctionStop = verticalFunctionStop ;
					EditorUtility.SetDirty( view ) ;
				}
				GUILayout.Label( "Vertical Function Stop" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool alwaysDisplay = EditorGUILayout.Toggle( view.AlwaysDisplay, GUILayout.Width( 16f ) ) ;
				if( alwaysDisplay != view.AlwaysDisplay )
				{
					Undo.RecordObject( view, "UIJoystick : Always Display Change" ) ;	// アンドウバッファに登録
					view.AlwaysDisplay = alwaysDisplay ;
					EditorUtility.SetDirty( view ) ;
				}
				GUILayout.Label( "Always Display" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			if( view.AlwaysDisplay == true )
			{
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					bool interactionRangeEnabled = EditorGUILayout.Toggle( view.InteractionRangeEnabled, GUILayout.Width( 16f ) ) ;
					if( interactionRangeEnabled != view.InteractionRangeEnabled )
					{
						Undo.RecordObject( view, "UIJoystick : Interaction Range Enabled Change" ) ;	// アンドウバッファに登録
						view.InteractionRangeEnabled = interactionRangeEnabled ;
						EditorUtility.SetDirty( view ) ;
					}
					GUILayout.Label( "Inateraction Range Enabled" ) ;
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				UIJoystick.ShapeTypes shapeType = ( UIJoystick.ShapeTypes )EditorGUILayout.EnumPopup( "Shape Type", view.ShapeType ) ;
				if( shapeType != view.ShapeType )
				{
					Undo.RecordObject( view, "UIJoystick : ShapeType Change" ) ;	// アンドウバッファに登録
					view.ShapeType = shapeType ;
					EditorUtility.SetDirty( view ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		}
	}
}

#endif
