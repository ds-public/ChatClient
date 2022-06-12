#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIImageNumber のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIImageNumber ) ) ]
	public class UIImageNumberInspector : UIViewInspector
	{
		/// <summary>
		/// スンスペクター描画
		/// </summary>
		override protected void DrawInspectorGUI()
		{
			// ターゲットのインスタンス
			UIImageNumber view = target as UIImageNumber ;
		
			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------
			
			// マテリアル選択
			DrawMaterial( view ) ;

			//-------------------------------------------------------------------

			EditorGUILayout.Separator() ;   // 少し区切りスペース

			// 表示する値
			GUI.backgroundColor = new Color( 0.0f, 1.0f, 1.0f, 1.0f ) ;	// ＧＵＩの下地を灰にする
			double value = EditorGUILayout.DoubleField( "Value", view.Value, GUILayout.Width( 200f ) ) ;
			GUI.backgroundColor = Color.white ;
			if( value != view.Value )
			{
				// 変化があった場合のみ処理する
				Undo.RecordObject( view, "ImageNumber : Value Change" ) ;	// アンドウバッファに登録
				view.Value = value ;
				EditorUtility.SetDirty( view ) ;
			}

			EditorGUILayout.Separator() ;	// 少し区切りスペース
			
			bool autoSizeFitting = EditorGUILayout.Toggle( "Auto Size Fitting", view.AutoSizeFitting ) ;
			if( autoSizeFitting != view.AutoSizeFitting )
			{
				Undo.RecordObject( view, "UIImageNumber : Auto Size Fitting Change" ) ;	// アンドウバッファに登録
				view.AutoSizeFitting = autoSizeFitting ;
				EditorUtility.SetDirty( view ) ;
			}
		}
	}
}

#endif
