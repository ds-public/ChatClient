#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// Circle のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( Circle ) ) ]
	public class CircleInspector : MaskableGraphicWrapperInspector
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
			Circle view = target as Circle ;
		
			// Graphic の基本情報を描画する
			DrawBasis( view ) ;

			EditorGUILayout.Separator() ;   // 少し区切りスペース

			// スプライト
			Sprite sprite = EditorGUILayout.ObjectField( "Sprite", view.Sprite, typeof( Sprite ), false ) as Sprite ;
			if( sprite != view.Sprite )
			{
				Undo.RecordObject( view, "Circle : Sprite Change" ) ;	// アンドウバッファに登録
				view.Sprite = sprite ;
				EditorUtility.SetDirty( view ) ;
			}

			// 内側の色
			Color innerColor = new Color( view.InnerColor.r, view.InnerColor.g, view.InnerColor.b, view.InnerColor.a ) ;
			innerColor = EditorGUILayout.ColorField( "Inner Color", innerColor ) ;
			if( innerColor.r != view.InnerColor.r || innerColor.g != view.InnerColor.g || innerColor.b != view.InnerColor.b || innerColor.a != view.InnerColor.a )
			{
				Undo.RecordObject( view, "Circle : Inner Color Change" ) ;	// アンドウバッファに登録
				view.InnerColor = innerColor ;
				EditorUtility.SetDirty( view ) ;
			}

			// 外側の色
			Color outerColor = new Color( view.OuterColor.r, view.OuterColor.g, view.OuterColor.b, view.OuterColor.a ) ;
			outerColor = EditorGUILayout.ColorField( "Outer Color", outerColor ) ;
			if( outerColor.r != view.OuterColor.r || outerColor.g != view.OuterColor.g || outerColor.b != view.OuterColor.b || outerColor.a != view.OuterColor.a )
			{
				Undo.RecordObject( view, "Circle : Outer Color Change" ) ;	// アンドウバッファに登録
				view.OuterColor = outerColor ;
				EditorUtility.SetDirty( view ) ;
			}

			// 分割数
			int split = EditorGUILayout.IntSlider( "Split", view.Split,   3, 360 ) ;
			if( split != view.Split && split >= 3 )
			{
				Undo.RecordObject( view, "Circle : Split Change" ) ;	// アンドウバッファに登録
				view.Split = split ;
				EditorUtility.SetDirty( view ) ;
			}

			// 内側の塗りつぶしの有無
			bool fillInner = EditorGUILayout.Toggle( "Fill Inner", view.FillInner ) ;
			if( fillInner != view.FillInner )
			{
				Undo.RecordObject( view, "Circle : Fill Inner Change" ) ;	// アンドウバッファに登録
				view.FillInner = fillInner ;
				EditorUtility.SetDirty( view ) ;
			}

			if( view.FillInner == false )
			{
				// 外周の線の太さ(塗りつぶし無し限定)
				float lineWidth = EditorGUILayout.Slider( "Line Width", view.LineWidth, 1, 20 ) ;
				if( lineWidth != view.LineWidth )
				{
					Undo.RecordObject( view, "Circle : Line Width Change" ) ;	// アンドウバッファに登録
					view.LineWidth = lineWidth ;
					EditorUtility.SetDirty( view ) ;
				}
			}
			else
			{
				// テクスチャの張り方(塗りつぶし有り限定)
				Circle.DecalTypes decalType = ( Circle.DecalTypes )EditorGUILayout.EnumPopup( "Decal Type",  view.DecalType ) ;
				if( decalType != view.DecalType )
				{
					Undo.RecordObject( view, "Circle : Decal Type Change" ) ;	// アンドウバッファに登録
					view.DecalType = decalType ;
					EditorUtility.SetDirty( view ) ;
				}
			}
			
			// 頂点距離スケール
			// データ配列
			SerializedObject so = new SerializedObject( view ) ;
			SerializedProperty sp = so.FindProperty( "m_VertexDistanceScales" ) ;
			if( sp != null )
			{
				EditorGUILayout.PropertyField( sp, new GUIContent( "VertexDistanceScales" ), true ) ;
			}
			so.ApplyModifiedProperties() ;
		}
	}
}

#endif

