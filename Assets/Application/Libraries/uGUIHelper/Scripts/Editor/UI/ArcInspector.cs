#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// Arc のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( Arc ) ) ]
	public class ArcInspector : MaskableGraphicWrapperInspector
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
			Arc tTarget = target as Arc ;
		
			// Graphic の基本情報を描画する
			DrawBasis( tTarget ) ;

			EditorGUILayout.Separator() ;   // 少し区切りスペース

			// スプライト
			Sprite tSprite = EditorGUILayout.ObjectField( "Sprite", tTarget.sprite, typeof( Sprite ), false ) as Sprite ;
			if( tSprite != tTarget.sprite )
			{
				Undo.RecordObject( tTarget, "Arc : Sprite Change" ) ;	// アンドウバッファに登録
				tTarget.sprite = tSprite ;
				EditorUtility.SetDirty( tTarget ) ;
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			// 内側の色
			Color tInnerColor = new Color( tTarget.innerColor.r, tTarget.innerColor.g, tTarget.innerColor.b, tTarget.innerColor.a ) ;
			tInnerColor = EditorGUILayout.ColorField( "Inner Color", tInnerColor ) ;
			if( tInnerColor.r != tTarget.innerColor.r || tInnerColor.g != tTarget.innerColor.g || tInnerColor.b != tTarget.innerColor.b || tInnerColor.a != tTarget.innerColor.a )
			{
				Undo.RecordObject( tTarget, "Arc : Inner Color Change" ) ;	// アンドウバッファに登録
				tTarget.innerColor = tInnerColor ;
				EditorUtility.SetDirty( tTarget ) ;
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			// 外側の色
			Color tOuterColor = new Color( tTarget.outerColor.r, tTarget.outerColor.g, tTarget.outerColor.b, tTarget.outerColor.a ) ;
			tOuterColor = EditorGUILayout.ColorField( "Outer Color", tOuterColor ) ;
			if( tOuterColor.r != tTarget.outerColor.r || tOuterColor.g != tTarget.outerColor.g || tOuterColor.b != tTarget.outerColor.b || tOuterColor.a != tTarget.outerColor.a )
			{
				Undo.RecordObject( tTarget, "Arc : Outer Color Change" ) ;	// アンドウバッファに登録
				tTarget.outerColor = tOuterColor ;
				EditorUtility.SetDirty( tTarget ) ;
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			// 開始角度
			float tStartAngle = EditorGUILayout.Slider( "Start Angle", tTarget.startAngle,   0, 360 ) ;
			if( tStartAngle != tTarget.startAngle )
			{
				Undo.RecordObject( tTarget, "Arc : Start Angle Change" ) ;	// アンドウバッファに登録
				tTarget.startAngle = tStartAngle ;
				EditorUtility.SetDirty( tTarget ) ;
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			// 終了角度
			float tEndAngle = EditorGUILayout.Slider( "End Angle", tTarget.endAngle,   0, 360 ) ;
			if( tEndAngle != tTarget.endAngle )
			{
				Undo.RecordObject( tTarget, "Arc : End Angle Change" ) ;	// アンドウバッファに登録
				tTarget.endAngle = tEndAngle ;
				EditorUtility.SetDirty( tTarget ) ;
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			// 円弧の方向
			Arc.Direction tDirection = ( Arc.Direction )EditorGUILayout.EnumPopup( "Direction",  tTarget.direction ) ;
			if( tDirection != tTarget.direction )
			{
				Undo.RecordObject( tTarget, "Arc : Direction Change" ) ;	// アンドウバッファに登録
				tTarget.direction = tDirection ;
				EditorUtility.SetDirty( tTarget ) ;
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}


			// 形状
			Arc.ShapeType tShapeType = ( Arc.ShapeType )EditorGUILayout.EnumPopup( "Shape Type",  tTarget.shapeType ) ;
			if( tShapeType != tTarget.shapeType )
			{
				Undo.RecordObject( tTarget, "Arc : Shape Type Change" ) ;	// アンドウバッファに登録
				tTarget.shapeType = tShapeType ;
				EditorUtility.SetDirty( tTarget ) ;
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			if( tTarget.shapeType == Arc.ShapeType.Circle )
			{
				// 分割数
				int tSplit = EditorGUILayout.IntSlider( "Split", tTarget.split,   3, 360 ) ;
				if( tSplit != tTarget.split && tSplit >= 3 )
				{
					Undo.RecordObject( tTarget, "Arc : Split Change" ) ;	// アンドウバッファに登録
					tTarget.split = tSplit ;
					EditorUtility.SetDirty( tTarget ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}

				// テクスチャの張り方
				Arc.DecalType tDecalType = ( Arc.DecalType )EditorGUILayout.EnumPopup( "Decal Type",  tTarget.decalType ) ;
				if( tDecalType != tTarget.decalType )
				{
					Undo.RecordObject( tTarget, "Arc : Decal Type Change" ) ;	// アンドウバッファに登録
					tTarget.decalType = tDecalType ;
					EditorUtility.SetDirty( tTarget ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
			}

			// これに形状タイプを追加する(以下も追加される)
			// ・分割数
			// ・デカール
		}
	}
}

#endif
