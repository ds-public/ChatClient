#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// Line のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( Line ) ) ]
	public class LineInspector : MaskableGraphicWrapperInspector
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
			Line tTarget = target as Line ;
		
			// Graphic の基本情報を描画する
			DrawBasis( tTarget ) ;

			EditorGUILayout.Separator() ;   // 少し区切りスペース

			// スプライト
			Sprite tSprite = EditorGUILayout.ObjectField( "Sprite", tTarget.sprite, typeof( Sprite ), false ) as Sprite ;
			if( tSprite != tTarget.sprite )
			{
				Undo.RecordObject( tTarget, "Line : Sprite Change" ) ;	// アンドウバッファに登録
				tTarget.sprite = tSprite ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			// 最初の太さ
			float tStartWidth = EditorGUILayout.FloatField( "Start Width", tTarget.startWidth ) ;
			if( tStartWidth != tTarget.startWidth && tStartWidth >  0 )
			{
				Undo.RecordObject( tTarget, "Line : Start Width Change" ) ;	// アンドウバッファに登録
				tTarget.startWidth = tStartWidth ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			// 最後の太さ
			float tEndWidth = EditorGUILayout.FloatField( "End Width", tTarget.endWidth ) ;
			if( tEndWidth != tTarget.endWidth && tEndWidth >  0 )
			{
				Undo.RecordObject( tTarget, "Line : Start Width Change" ) ;	// アンドウバッファに登録
				tTarget.endWidth = tEndWidth ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			// 最初の色
			Color tStartColor = new Color( tTarget.startColor.r, tTarget.startColor.g, tTarget.startColor.b, tTarget.startColor.a ) ;
			tStartColor = EditorGUILayout.ColorField( "Start Color", tStartColor ) ;
			if( tStartColor.r != tTarget.startColor.r || tStartColor.g != tTarget.startColor.g || tStartColor.b != tTarget.startColor.b || tStartColor.a != tTarget.startColor.a )
			{
				Undo.RecordObject( tTarget, "Line : Start Color Change" ) ;	// アンドウバッファに登録
				tTarget.startColor = tStartColor ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			// 最後の色
			Color tEndColor = new Color( tTarget.endColor.r, tTarget.endColor.g, tTarget.endColor.b, tTarget.endColor.a ) ;
			tEndColor = EditorGUILayout.ColorField( "End Color", tEndColor ) ;
			if( tEndColor.r != tTarget.endColor.r || tEndColor.g != tTarget.endColor.g || tEndColor.b != tTarget.endColor.b || tEndColor.a != tTarget.endColor.a )
			{
				Undo.RecordObject( tTarget, "Line : End Color Change" ) ;	// アンドウバッファに登録
				tTarget.endColor = tEndColor ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			// オフセット
			Vector2 tOffset = EditorGUILayout.Vector2Field( "Offset", tTarget.offset ) ;
			if( tOffset.Equals( tTarget.offset ) == false )
			{
				Undo.RecordObject( tTarget, "Line : Offset Change" ) ;	// アンドウバッファに登録
				tTarget.offset = tOffset ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			// データ配列
			SerializedObject tSO = new SerializedObject( tTarget ) ;
			SerializedProperty tSP = tSO.FindProperty( "vertices" ) ;
			if( tSP != null )
			{
				EditorGUILayout.PropertyField( tSP, new GUIContent( "Vertices" ), true ) ;
			}
			tSO.ApplyModifiedProperties() ;

			// ポジションタイプ
			Line.PositionType tPositionType = ( Line.PositionType )EditorGUILayout.EnumPopup( "Position Type",  tTarget.positionType ) ;
			if( tPositionType != tTarget.positionType )
			{
				Undo.RecordObject( tTarget, "Line : Position Type Change" ) ;	// アンドウバッファに登録
				tTarget.positionType = tPositionType ;
				EditorUtility.SetDirty( tTarget ) ;
			}
		}
	}
}

#endif
