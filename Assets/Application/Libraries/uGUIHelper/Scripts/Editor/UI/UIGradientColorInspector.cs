#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIGradientColor のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIGradient ) ) ]
	public class UIGradientInspector : Editor
	{
		// スンスペクター描画
		public override void OnInspectorGUI()
		{
			// とりあえずデフォルト
	//		DrawDefaultInspector() ;
		
			//--------------------------------------------

			// ターゲットのインスタンス
			UIGradient tTarget = target as UIGradient ;
		
			EditorGUILayout.Separator() ;   // 少し区切りスペース

			// バリュータイプ
			UIGradient.GeometoryTypes tGeometory = ( UIGradient.GeometoryTypes )EditorGUILayout.EnumPopup( "Geometory",  tTarget.GeometoryType ) ;
			if( tGeometory != tTarget.GeometoryType )
			{
				Undo.RecordObject( tTarget, "UIGradientColor : Geometory Change" ) ;	// アンドウバッファに登録
				tTarget.GeometoryType = tGeometory ;
				tTarget.Refresh() ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			// バリュータイプ
			UIGradient.DirectionTypes tDirection = ( UIGradient.DirectionTypes )EditorGUILayout.EnumPopup( "Direction",  tTarget.DirectionType ) ;
			if( tDirection != tTarget.DirectionType )
			{
				Undo.RecordObject( tTarget, "UIGradientColor : Direction Change" ) ;	// アンドウバッファに登録
				tTarget.DirectionType = tDirection ;
				tTarget.Refresh() ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			if( tTarget.DirectionType == UIGradient.DirectionTypes.Vertical || tTarget.DirectionType == UIGradient.DirectionTypes.Both )
			{
				Color tTop = CloneColor( tTarget.Top ) ;
				tTop = EditorGUILayout.ColorField( "Top", tTop ) ;
				if( CheckColor( tTop, tTarget.Top ) == false )
				{
					Undo.RecordObject( tTarget, "UIGradientColor : Top Change" ) ;	// アンドウバッファに登録
					tTarget.Top = tTop ;
					tTarget.Refresh() ;
					EditorUtility.SetDirty( tTarget ) ;
				}

				if( tTarget.GeometoryType == UIGradient.GeometoryTypes.Image )
				{
					Color tMiddle = CloneColor( tTarget.Middle ) ;
					tMiddle = EditorGUILayout.ColorField( "Middle", tMiddle ) ;
					if( CheckColor( tMiddle, tTarget.Middle ) == false )
					{
						Undo.RecordObject( tTarget, "UIGradientColor : Middle Change" ) ;	// アンドウバッファに登録
						tTarget.Middle = tMiddle ;
						tTarget.Refresh() ;
						EditorUtility.SetDirty( tTarget ) ;
					}
				}

				Color tBottom = CloneColor( tTarget.Bottom ) ;
				tBottom = EditorGUILayout.ColorField( "Bottom", tBottom ) ;
				if( CheckColor( tBottom, tTarget.Bottom ) == false )
				{
					Undo.RecordObject( tTarget, "UIGradientColor : Bottom Change" ) ;	// アンドウバッファに登録
					tTarget.Bottom = tBottom ;
					tTarget.Refresh() ;
					EditorUtility.SetDirty( tTarget ) ;
				}

				if( tTarget.GeometoryType == UIGradient.GeometoryTypes.Image )
				{
					float tPivotMiddle = EditorGUILayout.Slider( "Pivot Middle", tTarget.PivotMiddle, 0, 1 ) ;
					if( tPivotMiddle != tTarget.PivotMiddle )
					{
						Undo.RecordObject( tTarget, "UIGradientColor : Pivot Middle Change" ) ;	// アンドウバッファに登録
						tTarget.PivotMiddle = tPivotMiddle ;
						tTarget.Refresh() ;
						EditorUtility.SetDirty( tTarget ) ;
					}
				}
			}

			if( tTarget.DirectionType == UIGradient.DirectionTypes.Horizontal || tTarget.DirectionType == UIGradient.DirectionTypes.Both )
			{
				Color tLeft = CloneColor( tTarget.Left ) ;
				tLeft = EditorGUILayout.ColorField( "Left", tLeft ) ;
				if( CheckColor( tLeft, tTarget.Left ) == false )
				{
					Undo.RecordObject( tTarget, "UIGradientColor : Left Change" ) ;	// アンドウバッファに登録
					tTarget.Left = tLeft ;
					tTarget.Refresh() ;
					EditorUtility.SetDirty( tTarget ) ;
				}

				if( tTarget.GeometoryType == UIGradient.GeometoryTypes.Image )
				{
					Color tCenter = CloneColor( tTarget.Center ) ;
					tCenter = EditorGUILayout.ColorField( "Center", tCenter ) ;
					if( CheckColor( tCenter, tTarget.Center ) == false )
					{
						Undo.RecordObject( tTarget, "UIGradientColor : Center Change" ) ;	// アンドウバッファに登録
						tTarget.Center = tCenter ;
						tTarget.Refresh() ;
						EditorUtility.SetDirty( tTarget ) ;
					}
				}

				Color tRight = CloneColor( tTarget.Right ) ;
				tRight = EditorGUILayout.ColorField( "Right", tRight ) ;
				if( CheckColor( tRight, tTarget.Right ) == false )
				{
					Undo.RecordObject( tTarget, "UIGradientColor : Right Change" ) ;	// アンドウバッファに登録
					tTarget.Right = tRight ;
					tTarget.Refresh() ;
					EditorUtility.SetDirty( tTarget ) ;
				}

				if( tTarget.GeometoryType == UIGradient.GeometoryTypes.Image )
				{
					float tPivotCenter = EditorGUILayout.Slider( "Pivot Center", tTarget.PivotCenter, 0, 1 ) ;
					if( tPivotCenter != tTarget.PivotCenter )
					{
						Undo.RecordObject( tTarget, "UIGradientColor : Pivot Center Change" ) ;	// アンドウバッファに登録
						tTarget.PivotCenter = tPivotCenter ;
						tTarget.Refresh() ;
						EditorUtility.SetDirty( tTarget ) ;
					}
				}
			}
		}

		private Color CloneColor( Color color )
		{
			Color clone = new Color()
			{
				r = color.r,
				g = color.g,
				b = color.b,
				a = color.a,
			} ;
			return clone ;
		}

		private bool CheckColor( Color c0, Color c1 )
		{
			if( c0.r != c1.r || c0.g != c1.g  || c0.b != c1.b || c0.a != c1.a )
			{
				return false ;
			}

			return true ;
		}
	}
}

#endif
