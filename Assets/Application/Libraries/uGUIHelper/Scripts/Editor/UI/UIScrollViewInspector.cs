#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIScrollView のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIScrollView ) ) ]
	public class UIScrollViewInspector : UIImageInspector
	{
		override protected void DrawInspectorGUI()
		{
			UIScrollView tTarget = target as UIScrollView ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------

			// スクロール方向
			UIScrollView.DirectionTypes directionType = ( UIScrollView.DirectionTypes )EditorGUILayout.EnumPopup( "DirectionType",  tTarget.DirectionType ) ;
			if( directionType != tTarget.DirectionType )
			{
				Undo.RecordObject( tTarget, "UIScrollView : DirectionType Change" ) ;	// アンドウバッファに登録
				tTarget.DirectionType = directionType ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			// スクロールバーの有無
			DrawScrollbar( tTarget ) ;
		}

		// スクロールバーの有無を表示
		protected void DrawScrollbar( UIScrollView tTarget )
		{
			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			// 横スクロールバー
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool tIsHorizontalScrollbar = EditorGUILayout.Toggle( tTarget.IsHorizontalScrollber, GUILayout.Width( 16f ) ) ;
				if( tIsHorizontalScrollbar != tTarget.IsHorizontalScrollber )
				{
					Undo.RecordObject( tTarget, "UIScrollView : Is Horizontal Scrollbar Change" ) ;	// アンドウバッファに登録
					tTarget.IsHorizontalScrollber = tIsHorizontalScrollbar ;
					EditorUtility.SetDirty( tTarget ) ;
				}
				GUILayout.Label( "Horizontal Scrollbar", GUILayout.Width( 120f ) ) ;

				// 横位置
				UIScrollView.HorizontalScrollbarPositionTypes tHorizontalScrollbarPosition = ( UIScrollView.HorizontalScrollbarPositionTypes )EditorGUILayout.EnumPopup( "",  tTarget.HorizontalScrollbarPositionType, GUILayout.Width( 80f ) ) ;
				if( tHorizontalScrollbarPosition != tTarget.HorizontalScrollbarPositionType )
				{
					Undo.RecordObject( tTarget, "UIScrollView : Horizontal Scrollbar Position Change" ) ;	// アンドウバッファに登録
					tTarget.HorizontalScrollbarPositionType = tHorizontalScrollbarPosition ;
					EditorUtility.SetDirty( tTarget ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			UIScrollbar tHorizontalScrollbarElastic = EditorGUILayout.ObjectField( "Elastic", tTarget.HorizontalScrollbarElastic, typeof( UIScrollbar ), true ) as UIScrollbar ;
			if( tHorizontalScrollbarElastic != tTarget.HorizontalScrollbarElastic )
			{
				Undo.RecordObject( tTarget, "UIScrollView : Horizontal Scrollbar Elastic Change" ) ;	// アンドウバッファに登録
				tTarget.HorizontalScrollbarElastic = tHorizontalScrollbarElastic ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			// 縦スクロールバー
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool tIsVerticalScrollbar = EditorGUILayout.Toggle( tTarget.IsVerticalScrollber, GUILayout.Width( 16f ) ) ;
				if( tIsVerticalScrollbar != tTarget.IsVerticalScrollber )
				{
					Undo.RecordObject( tTarget, "UIScrollView : Is Horizontal Scrollbar Change" ) ;	// アンドウバッファに登録
					tTarget.IsVerticalScrollber = tIsVerticalScrollbar ;
					EditorUtility.SetDirty( tTarget ) ;
				}
				GUILayout.Label( "Vertical Scrollbar", GUILayout.Width( 120f ) ) ;

				// 縦位置
				UIScrollView.VerticalScrollbarPositionTypes tVerticalScrollbarPosition = ( UIScrollView.VerticalScrollbarPositionTypes )EditorGUILayout.EnumPopup( "",  tTarget.VerticalScrollbarPositionType, GUILayout.Width( 80f ) ) ;
				if( tVerticalScrollbarPosition != tTarget.VerticalScrollbarPositionType )
				{
					Undo.RecordObject( tTarget, "UIScrollView : Vertical Scrollbar Position Change" ) ;	// アンドウバッファに登録
					tTarget.VerticalScrollbarPositionType = tVerticalScrollbarPosition ;
					EditorUtility.SetDirty( tTarget ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			UIScrollbar tVerticalScrollbarElastic = EditorGUILayout.ObjectField( "Elastic", tTarget.VerticalScrollbarElastic, typeof( UIScrollbar ), true ) as UIScrollbar ;
			if( tVerticalScrollbarElastic != tTarget.VerticalScrollbarElastic )
			{
				Undo.RecordObject( tTarget, "UIScrollView : Vertical Scrollbar Elastic Change" ) ;	// アンドウバッファに登録
				tTarget.VerticalScrollbarElastic = tVerticalScrollbarElastic ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			//----------------------------------------------------------

			bool tWarning ;

			tWarning = false ;
			if( tTarget.HorizontalScrollbarElastic != null && tTarget.HorizontalScrollbarElastic.ScrollViewElastic == null )
			{
				tWarning = true ;
			}
			if( tTarget.VerticalScrollbarElastic != null && tTarget.VerticalScrollbarElastic.ScrollViewElastic == null )
			{
				tWarning = true ;
			}
			if( tWarning == true )
			{
				EditorGUILayout.HelpBox( GetMessage( "SetScrollView" ), MessageType.Warning, true ) ;
			}

			ScrollRectWrapper tScroll = tTarget.GetComponent<ScrollRectWrapper>() ;
			if( tScroll != null )
			{
				tWarning = false ;
				if( tTarget.HorizontalScrollbarElastic != null && tScroll.horizontalScrollbar!= null )
				{
					tWarning = true ;
				}
				if( tTarget.VerticalScrollbarElastic != null && tScroll.verticalScrollbar != null )
				{
					tWarning = true ;
				}
				if( tWarning == true )
				{
					EditorGUILayout.HelpBox( GetMessage( "ClearScrollBar" ), MessageType.Warning, true ) ;
				}
			}

			//----------------------------------------------------------

			if( tTarget.IsHorizontalScrollber == true || tTarget.HorizontalScrollbarElastic != null || tTarget.IsVerticalScrollber == true || tTarget.VerticalScrollbarElastic != null )
			{
				// スクロールバーのフェード処理
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					bool tScrollbarFadeEnebled = EditorGUILayout.Toggle( tTarget.ScrollbarFadeEnabled, GUILayout.Width( 16f ) ) ;
					if( tScrollbarFadeEnebled != tTarget.ScrollbarFadeEnabled )
					{
						Undo.RecordObject( tTarget, "UIScrollView : Scrollbar Fade Enabled Change" ) ;	// アンドウバッファに登録
						tTarget.ScrollbarFadeEnabled = tScrollbarFadeEnebled ;
						EditorUtility.SetDirty( tTarget ) ;
//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
					GUILayout.Label( "Scrollbar Fade Enabled" ) ;
				}
				GUILayout.EndHorizontal() ;		// 横並び終了


				if( tTarget.ScrollbarFadeEnabled == true )
				{
					if( tTarget.IsInteraction == false && tTarget.IsInteractionForScrollView == false )
					{
						EditorGUILayout.HelpBox( GetMessage( "InteractionNone" ), MessageType.Warning, true ) ;
					}

//					if( tTarget._scrollRect.verticalScrollbarVisibility != UnityEngine.UI.ScrollRect.ScrollbarVisibility.AutoHide )
//					{
//						EditorGUILayout.HelpBox( GetMessage( "SetAutoHide" ), MessageType.Warning, true ) ;
//					}

	//				EditorGUIUtility.LookLikeControls( 120f, 40f ) ;	// デフォルトの見た目で横幅８０

					float tScrollbarFadeInDuration = EditorGUILayout.FloatField( " Duration In", tTarget.ScrollbarFadeInDuration /*, GUILayout.Width( 120f ) */ ) ;
					if( tScrollbarFadeInDuration != tTarget.ScrollbarFadeInDuration )
					{
						Undo.RecordObject( tTarget, "UIScrollView : Scrollbar Fade In Duration" ) ;	// アンドウバッファに登録
						tTarget.ScrollbarFadeInDuration = tScrollbarFadeInDuration ;
						EditorUtility.SetDirty( tTarget ) ;
					}

					float tScrollbarFadeHoldDuration = EditorGUILayout.FloatField( " Duration Hold", tTarget.ScrollbarFadeHoldDuration /*, GUILayout.Width( 120f ) */ ) ;
					if( tScrollbarFadeHoldDuration != tTarget.ScrollbarFadeHoldDuration )
					{
						Undo.RecordObject( tTarget, "UIScrollView : Scrollbar Fade Hold Duration" ) ;	// アンドウバッファに登録
						tTarget.ScrollbarFadeHoldDuration = tScrollbarFadeHoldDuration ;
						EditorUtility.SetDirty( tTarget ) ;
					}

					float tScrollbarFadeOutDuration = EditorGUILayout.FloatField( " Duration Out", tTarget.ScrollbarFadeOutDuration /*, GUILayout.Width( 120f ) */ ) ;
					if( tScrollbarFadeOutDuration != tTarget.ScrollbarFadeOutDuration )
					{
						Undo.RecordObject( tTarget, "UIScrollView : Scrollbar Fade Out Duration" ) ;	// アンドウバッファに登録
						tTarget.ScrollbarFadeOutDuration = tScrollbarFadeOutDuration ;
						EditorUtility.SetDirty( tTarget ) ;
					}



	//				EditorGUIUtility.LookLikeControls( 120f, 40f ) ;	// デフォルトの見た目で横幅８０


					if( tTarget.HorizontalScrollbar != null )
					{
						if( tTarget.HorizontalScrollbar.GetComponent<CanvasGroup>() == null )
						{
							EditorGUILayout.HelpBox( GetMessage( "HSB_CanvasGroupNone" ), MessageType.Warning, true ) ;
						}
					}
					if( tTarget.VerticalScrollbar != null )
					{
						if( tTarget.VerticalScrollbar.GetComponent<CanvasGroup>() == null )
						{
							EditorGUILayout.HelpBox( GetMessage( "VSB_CanvasGroupNone" ), MessageType.Warning, true ) ;
						}
					}
				}

					GUILayout.BeginHorizontal() ;	// 横並び
					{
						bool tHidingScrollbarIfContentFew = EditorGUILayout.Toggle( tTarget.HidingScrollbarIfContentFew, GUILayout.Width( 16f ) ) ;
						if( tHidingScrollbarIfContentFew != tTarget.HidingScrollbarIfContentFew )
						{
							Undo.RecordObject( tTarget, "UIScrollView : Hiding Scrollbar If Content Few" ) ;	// アンドウバッファに登録
							tTarget.HidingScrollbarIfContentFew = tHidingScrollbarIfContentFew ;
							EditorUtility.SetDirty( tTarget ) ;
						}
						GUILayout.Label( " Hiding Scrollbar If Content Few" ) ;
					}
					GUILayout.EndHorizontal() ;		// 横並び終了

					GUILayout.BeginHorizontal() ;	// 横並び
					{
						bool tInvalidateScrollIfContentFew = EditorGUILayout.Toggle( tTarget.InvalidateScrollIfContentFew, GUILayout.Width( 16f ) ) ;
						if( tInvalidateScrollIfContentFew != tTarget.InvalidateScrollIfContentFew )
						{
							Undo.RecordObject( tTarget, "UIScrollView : Invalidate Scroll If Content Few" ) ;	// アンドウバッファに登録
							tTarget.InvalidateScrollIfContentFew = tInvalidateScrollIfContentFew ;
							EditorUtility.SetDirty( tTarget ) ;
						}
						GUILayout.Label( " Invalidate Scroll If Content Few" ) ;
					}
					GUILayout.EndHorizontal() ;		// 横並び終了
			}
		}

		//--------------------------------------------------------------------------

		private Dictionary<string,string> mJapanese_Message = new Dictionary<string, string>()
		{
			{ "SetScrollView",			"Elastic を 完全に有効にするには Scrollbar に ScrollView を設定する必要があります" },
			{ "ClearScrollBar",			"Elastic を 完全に有効にするには ScrollRect の Scrollbar を消去する必要があります" },
			{ "SetAutoHide",			"Visibility を 'Auto Hide' に設定する事をお勧めします" },
			{ "InteractionNone",		"UIInteraction クラスまたは UIInteractionForScrollView クラスが必要です" },
			{ "HSB_CanvasGroupNone",	"Horizontal Scrollbar に CanvasGroup クラスが必要です" },
			{ "VSB_CanvasGroupNone",	"Vertical Scrollbar に CanvasGroup クラスが必要です" },
		} ;
		private Dictionary<string,string> mEnglish_Message = new Dictionary<string, string>()
		{
			{ "SetScrollView",			"In order to fully enable Elastic you need to set ScrollView on Scrollbar" },
			{ "ClearScrollBar",			"In order to fully enable Elastic, you need to clear the Scrollbar of ScrollRect" },
			{ "SetAutoHide",			"Recommend to set Visibility to 'Auto Hide'" },
			{ "InteractionNone",		"'UIInteraction' or 'UIInteractionForScrollView' is necessary" },
			{ "HSB_CanvasGroupNone",	"'CanvasGroup' is necessary to horizontal scrollbar" },
			{ "VSB_CanvasGroupNone",	"'CanvasGroup' is necessary to vertical scrollbar" },
		} ;

		private string GetMessage( string tLabel )
		{
			if( Application.systemLanguage == SystemLanguage.Japanese )
			{
				if( mJapanese_Message.ContainsKey( tLabel ) == false )
				{
					return "指定のラベル名が見つかりません" ;
				}
				return mJapanese_Message[ tLabel ] ;
			}
			else
			{
				if( mEnglish_Message.ContainsKey( tLabel ) == false )
				{
					return "Specifying the label name can not be found" ;
				}
				return mEnglish_Message[ tLabel ] ;
			}
		}
	}
}

#endif
