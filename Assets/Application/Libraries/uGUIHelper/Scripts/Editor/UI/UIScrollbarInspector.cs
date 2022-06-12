#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIScrollbar のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIScrollbar ) ) ]
	public class UIScrollbarInspector : UIViewInspector
	{
		override protected void DrawInspectorGUI()
		{
			UIScrollbar view = target as UIScrollbar ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------
		
			UIScrollView scrollViewElastic = EditorGUILayout.ObjectField( "ScrollView Elastic", view.ScrollViewElastic, typeof( UIScrollView ), true ) as UIScrollView ;
			if( scrollViewElastic != view.ScrollViewElastic )
			{
				Undo.RecordObject( view, "UIScrollbar : ScrollbarViewElastic Change" ) ;	// アンドウバッファに登録
				view.ScrollViewElastic = scrollViewElastic ;
				EditorUtility.SetDirty( view ) ;
			}

			if( view.ScrollViewElastic != null )
			{
				if( view.IsInteraction == false )
				{
					EditorGUILayout.HelpBox( GetMessage( "InteractionNone" ), MessageType.Warning, true ) ;
				}
			}

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool fixedSize = EditorGUILayout.Toggle( view.FixedSize, GUILayout.Width( 16f ) ) ;
				if( fixedSize != view.FixedSize )
				{
					Undo.RecordObject( view, "UIView : EventTrigger Change" ) ;	// アンドウバッファに登録
					view.FixedSize = fixedSize ;
					EditorUtility.SetDirty( view ) ;
				}
				GUILayout.Label( "FixedSize" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		}

		//--------------------------------------------------------------------------

		private readonly Dictionary<string,string> m_Japanese_Message = new Dictionary<string, string>()
		{
			{ "SetAutoHide",			"Visibility を 'Auto Hide' に設定する事をお勧めします" },
			{ "InteractionNone",		"UIInteraction クラスが必要です" },
			{ "HSB_CanvasGroupNone",	"Horizontal Scrollbar に CanvasGroup クラスが必要です" },
			{ "VSB_CanvasGroupNone",	"Vertical Scrollbar に CanvasGroup クラスが必要です" },
		} ;
		private readonly Dictionary<string,string> m_English_Message = new Dictionary<string, string>()
		{
			{ "SetAutoHide",			"Recommend to set Visibility to 'Auto Hide'" },
			{ "InteractionNone",		"'UIInteraction' is necessary" },
			{ "HSB_CanvasGroupNone",	"'CanvasGroup' is necessary to horizontal scrollbar" },
			{ "VSB_CanvasGroupNone",	"'CanvasGroup' is necessary to vertical scrollbar" },
		} ;

		private string GetMessage( string label )
		{
			if( Application.systemLanguage == SystemLanguage.Japanese )
			{
				if( m_Japanese_Message.ContainsKey( label ) == false )
				{
					return "指定のラベル名が見つかりません" ;
				}
				return m_Japanese_Message[ label ] ;
			}
			else
			{
				if( m_English_Message.ContainsKey( label ) == false )
				{
					return "Specifying the label name can not be found" ;
				}
				return m_English_Message[ label ] ;
			}
		}

	}
}

#endif
