#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UINumber のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UINumberMesh ) ) ]
	public class UINumberMeshInspector : UIViewInspector
	{
		override protected void DrawInspectorGUI()
		{
			UINumberMesh tTarget = target as UINumberMesh ;
		
			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------
		
			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			bool tAutoSizeFitting = EditorGUILayout.Toggle( "Auto Size Fitting", tTarget.AutoSizeFitting ) ;
			if( tAutoSizeFitting != tTarget.AutoSizeFitting )
			{
				Undo.RecordObject( tTarget, "UINumberMesh : Auto Size Fitting Change" ) ;	// アンドウバッファに登録
				tTarget.AutoSizeFitting = tAutoSizeFitting ;
				EditorUtility.SetDirty( tTarget ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}


			//-------------------------------------------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			bool tIsCustomized = EditorGUILayout.Toggle( "Customize", tTarget.IsCustomized ) ;
			if( tIsCustomized != tTarget.IsCustomized )
			{
				Undo.RecordObject( tTarget, "UITextMesh : Customize Change" ) ;	// アンドウバッファに登録
				tTarget.IsCustomized = tIsCustomized ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			if( tTarget.IsCustomized == true )
			{
				Color tOutlineColor_Old = CloneColor( tTarget.OutlineColor ) ;
				Color tOutlineColor_New = EditorGUILayout.ColorField( "Outline Color", tOutlineColor_Old ) ;
				if( CheckColor( tOutlineColor_Old, tOutlineColor_New ) == false )
				{
					Undo.RecordObject( tTarget, "UITextMesh : Outline Color Change" ) ;	// アンドウバッファに登録
					tTarget.OutlineColor = tOutlineColor_New ;
					EditorUtility.SetDirty( tTarget ) ;
				}
			}

			bool tRaycastTarget = EditorGUILayout.Toggle( "Raycast Target", tTarget.RaycastTarget ) ;
			if( tRaycastTarget != tTarget.RaycastTarget )
			{
				Undo.RecordObject( tTarget, "UITextMesh : RaycastTarget Change" ) ;	// アンドウバッファに登録
				tTarget.RaycastTarget = tRaycastTarget ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			//-------------------------------------------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			EditorGUIUtility.labelWidth =  60f ;
			EditorGUIUtility.fieldWidth =  30f ;

			// 表示する値
			GUI.backgroundColor = new Color( 0.0f, 1.0f, 1.0f, 1.0f ) ;	// ＧＵＩの下地を灰にする
			double tValue = EditorGUILayout.DoubleField( "Value", tTarget.Value, GUILayout.Width( 200f ) ) ;
			GUI.backgroundColor = Color.white ;
			if( tValue != tTarget.Value )
			{
				// 変化があった場合のみ処理する
				Undo.RecordObject( tTarget, "UINumberMesh : Value Change" ) ;	// アンドウバッファに登録
				tTarget.Value = tValue ;
				EditorUtility.SetDirty( tTarget ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}
		
			EditorGUIUtility.labelWidth = 116f ;
			EditorGUIUtility.fieldWidth =  40f ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-----------------------------------------------------
			
			EditorGUIUtility.labelWidth =  60f ;
			EditorGUIUtility.fieldWidth =  30f ;
		
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// 縦方向揃え
				GUILayout.Label( "Digit", GUILayout.Width( 40.0f ) ) ;	// null でないなら 74
				int tDigitInteger = EditorGUILayout.IntField( tTarget.DigitInteger, GUILayout.Width( 40f ) ) ;
				if( tDigitInteger != tTarget.DigitInteger )
				{
					// 変化があった場合のみ処理する
					Undo.RecordObject( tTarget, "UINumberMesh : Digit Integer Change" ) ;	// アンドウバッファに登録
					tTarget.DigitInteger = tDigitInteger ;
					EditorUtility.SetDirty( tTarget ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
			
				GUILayout.Label( ".", GUILayout.Width( 10.0f ) ) ;	// null でないなら 74
			
				int tDigitDecimal = EditorGUILayout.IntField( tTarget.DigitDecimal, GUILayout.Width( 40f ) ) ;
				if( tDigitDecimal != tTarget.DigitDecimal )
				{
					// 変化があった場合のみ処理する
					Undo.RecordObject( tTarget, "UINumberMesh : Digit Decimal Change" ) ;	// アンドウバッファに登録
					tTarget.DigitDecimal = tDigitDecimal ;
					EditorUtility.SetDirty( tTarget ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}

				// 適当なスペース
				GUILayout.Label( "", GUILayout.Width( 5f ) ) ;

				// カンマ
				GUILayout.Label( "Comma", GUILayout.Width( 50.0f ) ) ;	// null でないなら 74
				int tComma = EditorGUILayout.IntField( tTarget.Comma, GUILayout.Width( 40f ) ) ;
				if( tComma != tTarget.Comma )
				{
					// 変化があった場合のみ処理する
					Undo.RecordObject( tTarget, "UINumberMesh : Comma Change" ) ;	// アンドウバッファに登録
					tTarget.Comma = tComma ;
					EditorUtility.SetDirty( tTarget ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		
			EditorGUIUtility.labelWidth = 116f ;
			EditorGUIUtility.fieldWidth =  40f ;
		
			// 符号を表示するか否か
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// 各種ＧＵＩのデフォルトリソースのロードを有効にするか
				bool tPlusSign = EditorGUILayout.Toggle( tTarget.PlusSign, GUILayout.Width( 16f ) ) ;
				if( tPlusSign != tTarget.PlusSign )
				{
					Undo.RecordObject( tTarget, "UINumberMesh : Plus Sign Change" ) ;	// アンドウバッファに登録
					tTarget.PlusSign = tPlusSign ;
					EditorUtility.SetDirty( tTarget ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( "Plus Sign", GUILayout.Width( 80f ) ) ;
	//		}
	//		GUILayout.EndHorizontal() ;		// 横並び終了
			
			GUILayout.Label( "", GUILayout.Width( 10f ) ) ;

			// 符号を表示するか否か
//			GUILayout.BeginHorizontal() ;	// 横並び
//			{
				// 各種ＧＵＩのデフォルトリソースのロードを有効にするか
				bool tZeroSign = EditorGUILayout.Toggle( tTarget.ZeroSign, GUILayout.Width( 16f ) ) ;
				if( tZeroSign != tTarget.ZeroSign )
				{
					Undo.RecordObject( tTarget, "UINumberMesh : Zero Sign Change" ) ;	// アンドウバッファに登録
					tTarget.ZeroSign = tZeroSign ;
					EditorUtility.SetDirty( tTarget ) ;
				}
				GUILayout.Label( "Zero Sign", GUILayout.Width( 80f ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

//			GUILayout.Label( "", GUILayout.Width( 10f ) ) ;

			// ０埋め
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// 各種ＧＵＩのデフォルトリソースのロードを有効にするか
				bool tZeroPadding = EditorGUILayout.Toggle( tTarget.ZeroPadding, GUILayout.Width( 16f ) ) ;
				if( tZeroPadding != tTarget.ZeroPadding )
				{
					Undo.RecordObject( tTarget, "UINumberMesh : Zero Padding Change" ) ;	// アンドウバッファに登録
					tTarget.ZeroPadding = tZeroPadding ;
					EditorUtility.SetDirty( tTarget ) ;
				}
				GUILayout.Label( "Zero Padding", GUILayout.Width( 80f ) ) ;
//			}
//			GUILayout.EndHorizontal() ;		// 横並び終了
		
			GUILayout.Label( "", GUILayout.Width( 10f ) ) ;
		
			// パーセント
//			GUILayout.BeginHorizontal() ;	// 横並び
//			{
				// 各種ＧＵＩのデフォルトリソースのロードを有効にするか
				bool tPercent = EditorGUILayout.Toggle( tTarget.Percent, GUILayout.Width( 16f ) ) ;
				if( tPercent != tTarget.Percent )
				{
					Undo.RecordObject( tTarget, "UINumberMesh : Percent Change" ) ;	// アンドウバッファに登録
					tTarget.Percent = tPercent ;
					EditorUtility.SetDirty( tTarget ) ;
				}
				GUILayout.Label( "Percent", GUILayout.Width( 80f ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		
		
			// 全角
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// 各種ＧＵＩのデフォルトリソースのロードを有効にするか
				bool tZenkaku = EditorGUILayout.Toggle( tTarget.Zenkaku, GUILayout.Width( 16f ) ) ;
				if( tZenkaku != tTarget.Zenkaku )
				{
					Undo.RecordObject( tTarget, "UINumberMesh : Zenkaku Change" ) ;	// アンドウバッファに登録
					tTarget.Zenkaku = tZenkaku ;
					EditorUtility.SetDirty( tTarget ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( "Zenkaku", GUILayout.Width( 80f ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		}
	}
}

#endif
