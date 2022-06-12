#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIInputField のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIInputField ) ) ]
	public class UIInputFieldInspector : UIViewInspector
	{
		override protected void DrawInspectorGUI()
		{
			UIInputField view = target as UIInputField ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------

			// インプットフィルタータイプ
			UIInputField.InputFilterTypes inputFilterType = ( UIInputField.InputFilterTypes )EditorGUILayout.EnumPopup( "Input Filter Type", view.InputFilterType ) ;
			if( inputFilterType != view.InputFilterType )
			{
				Undo.RecordObject( view, "UIInputField : Input Filter Type Change" ) ;	// アンドウバッファに登録
				view.InputFilterType = inputFilterType ;
				EditorUtility.SetDirty( view ) ;
			}

			if( view.InputFilterType != UIInputField.InputFilterTypes.String )
			{
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					float minValue = EditorGUILayout.FloatField( "Min Value", ( float )view.MinValue ) ;
					if( minValue != view.MinValue )
					{
						Undo.RecordObject( view, "UIInputField : Min Value Change" ) ;	// アンドウバッファに登録
						view.MinValue = minValue ;
						EditorUtility.SetDirty( view ) ;
					}

					float maxValue = EditorGUILayout.FloatField( "Max Value", ( float )view.MaxValue ) ;
					if( maxValue != view.MaxValue )
					{
						Undo.RecordObject( view, "UIInputField : Max Value Change" ) ;	// アンドウバッファに登録
						view.MaxValue = maxValue ;
						EditorUtility.SetDirty( view ) ;
					}
				}
				GUILayout.EndHorizontal() ;     // 横並び終了
			}

			// 文字コードフィルターを有効にするにはフィルター用のファイルの指定が必要
			FontFilter fontFilter = EditorGUILayout.ObjectField( "Font Filter", view.FontFilter, typeof( FontFilter ), false ) as FontFilter ;
			if( fontFilter != view.FontFilter )
			{
				Undo.RecordObject( view, "UIInputField : Font Filter Change " ) ;	// アンドウバッファに登録
				view.FontFilter = fontFilter ;
				EditorUtility.SetDirty( view ) ;
			}

			// 最初の一文字だけ使用する
			string fontAlternateCodeOld = "" + view.FontAlternateCode ;
			string fontAlternateCodeNew = EditorGUILayout.TextField( "Font Alternate Code", fontAlternateCodeOld ) ;
			if( string.IsNullOrEmpty( fontAlternateCodeNew ) == false && ( fontAlternateCodeNew[ 0 ] != fontAlternateCodeOld[ 0 ] ) )
			{
				Undo.RecordObject( view, "UIInputField : Font Alternate Code Change " ) ;	// アンドウバッファに登録
				view.FontAlternateCode = fontAlternateCodeNew[ 0 ] ;
				EditorUtility.SetDirty( view ) ;
			}
		}
	}
}

#endif
