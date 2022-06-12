#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIText のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIRichText ) ) ]
	public class UIRichTextInspector : UIViewInspector
	{
		override protected void DrawInspectorGUI()
		{
			UIRichText view = target as UIRichText ;
		
			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------
		
			EditorGUILayout.Separator() ;	// 少し区切りスペース
			
			bool autoSizeFitting = EditorGUILayout.Toggle( "Auto Size Fitting", view.AutoSizeFitting ) ;
			if( autoSizeFitting != view.AutoSizeFitting )
			{
				Undo.RecordObject( view, "UIRichText : Auto Size Fitting Change" ) ;	// アンドウバッファに登録
				view.AutoSizeFitting = autoSizeFitting ;
				EditorUtility.SetDirty( view ) ;
			}

			string localizationKey = EditorGUILayout.TextField( "Localization Key", view.LocalizationKey ) ;
			if( localizationKey != view.LocalizationKey )
			{
				Undo.RecordObject( view, "UIRichText : Localization Key Change" ) ;	// アンドウバッファに登録
				view.LocalizationKey = localizationKey ;
				EditorUtility.SetDirty( view ) ;
			}

			// 全角
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// 各種ＧＵＩのデフォルトリソースのロードを有効にするか
				bool zenkaku = EditorGUILayout.Toggle( view.Zenkaku, GUILayout.Width( 16f ) ) ;
				if( zenkaku != view.Zenkaku )
				{
					Undo.RecordObject( view, "UIRichText : Zenkaku Change" ) ;	// アンドウバッファに登録
					view.Zenkaku = zenkaku ;
					EditorUtility.SetDirty( view ) ;
				}
				GUILayout.Label( "Zenkaku", GUILayout.Width( 80f ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		}
	}
}

#endif
