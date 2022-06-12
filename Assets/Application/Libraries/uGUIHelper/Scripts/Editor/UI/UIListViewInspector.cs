#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIListView のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIListView ) ) ]
	public class UIListViewInspector : UIScrollViewInspector
	{
		override protected void DrawInspectorGUI()
		{
			UIListView view = target as UIListView ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------
		
			// スクロール方向
			UIScrollView.DirectionTypes directionType = ( UIScrollView.DirectionTypes )EditorGUILayout.EnumPopup( "DirectionType",  view.DirectionType ) ;
			if( directionType != view.DirectionType )
			{
				Undo.RecordObject( view, "UIScrollView : DirectionType Change" ) ;	// アンドウバッファに登録
				view.DirectionType = directionType ;
				EditorUtility.SetDirty( view ) ;
			}

			// スクロールバーの有無
			DrawScrollbar( view ) ;
		
			// アイテムの追加と削除
			DrawItem( view ) ;
		}

		// Item 関係
		protected void DrawItem( UIListView view )
		{
			if( view.CScrollRect == null || view.CScrollRect.content == null )
			{
				return ;
			}


			EditorGUILayout.Separator() ;	// 少し区切りスペース

//			GUILayout.Label( "Type " + tTarget.buildType, GUILayout.Width( 120f ) ) ;

			EditorGUIUtility.labelWidth = 144f ;
			EditorGUIUtility.fieldWidth =  40f ;

			// テンプレート
			UIView contentItem = EditorGUILayout.ObjectField( "Item(Template)", view.Item, typeof( UIView ), true ) as UIView ;
			if( contentItem != view.Item )
			{
				Undo.RecordObject( view, "UIListView Item(Template) : Change" ) ;	// アンドウバッファに登録
				view.Item = contentItem ;
				EditorUtility.SetDirty( view ) ;
			}

			if( view.Item != null )
			{
				var components = view.Item.GetComponents<MonoBehaviour>() ;
				if( components != null && components.Length >  0 )
				{
					List<MonoBehaviour> itemComponents = new List<MonoBehaviour>(){ null } ;
					List<string> itemComponentNames = new List<string>(){ "Unknown" } ;

					MonoBehaviour itemComponent ;

					int i, l = components.Length ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						itemComponent = components[ i ] ;

						itemComponents.Add( itemComponent ) ;
						itemComponentNames.Add( itemComponent.GetType().ToString() ) ;
					}

					int indexOld = 0 ;
					if( view.ItemComponent != null )
					{
						for( i  = 1 ; i <  itemComponents.Count ; i ++ )
						{
							if( itemComponents[ i ] == view.ItemComponent )
							{
								indexOld = i ;
								break ;
							}
						}
					}

					int indexNew  = EditorGUILayout.Popup( "ItemComponentType", indexOld, itemComponentNames.ToArray() ) ;	// フィールド名有りタイプ
					if( indexNew != indexOld )
					{
						Undo.RecordObject( view, "UIListView Item Component Type : Change" ) ;	// アンドウバッファに登録
						view.ItemComponent = itemComponents[ indexNew ] ;
						EditorUtility.SetDirty( view ) ;
					}
				}
			}

			//------------------------------------------------

			EditorGUIUtility.labelWidth = 144f ;
			EditorGUIUtility.fieldWidth =  40f ;

			GUILayout.BeginHorizontal() ;	// 横並び開始
			{
				int workingItemCount = EditorGUILayout.IntField( "Working Item Count", view.WorkingItemCount ) ;
				if( workingItemCount!= view.WorkingItemCount )
				{
					Undo.RecordObject( view, "UIListView : Working Item Count Change" ) ;	// アンドウバッファに登録
					view.WorkingItemCount = workingItemCount ;
					EditorUtility.SetDirty( view ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			float workingMargin = EditorGUILayout.FloatField( "Working Margin", view.WorkingMargin ) ;
			if( workingMargin!= view.WorkingMargin )
			{
				Undo.RecordObject( view, "UIListView : Working Margin Change" ) ;	// アンドウバッファに登録
				view.WorkingMargin = workingMargin ;
				EditorUtility.SetDirty( view ) ;
			}

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool infinity = EditorGUILayout.Toggle( view.Infinity, GUILayout.Width( 16f ) ) ;
				if( infinity != view.Infinity )
				{
					Undo.RecordObject( view, "UIListView : Infinity Change" ) ;	// アンドウバッファに登録
					view.Infinity = infinity ;
					EditorUtility.SetDirty( view ) ;
				}
				GUILayout.Label( "Infinity", GUILayout.Width( 80f ) ) ;


				if( view.Infinity == false )
				{
//					GUILayout.Label( "Max", GUILayout.Width( 40f ) ) ;

					EditorGUIUtility.labelWidth =  80f ;
					EditorGUIUtility.fieldWidth =  40f ;

					int itemCount = EditorGUILayout.IntField( "Item Count", view.ItemCount ) ;
					if( itemCount!= view.ItemCount )
					{
						Undo.RecordObject( view, "UIListView : Item Count Change" ) ;	// アンドウバッファに登録
						view.ItemCount = itemCount ;
						EditorUtility.SetDirty( view ) ;
//						if( Application.isPlaying == false )
//						{
//							UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
//						}
					}

					EditorGUIUtility.labelWidth = 116f ;
					EditorGUIUtility.fieldWidth =  40f ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool snap = EditorGUILayout.Toggle( view.Snap, GUILayout.Width( 16f ) ) ;
				if( snap != view.Snap )
				{
					Undo.RecordObject( view, "UIListView : Snap Change" ) ;	// アンドウバッファに登録
					view.Snap = snap ;
					EditorUtility.SetDirty( view ) ;
				}
				GUILayout.Label( "Snap", GUILayout.Width( 80f ) ) ;

				if( view.Snap == true )
				{
					EditorGUIUtility.labelWidth =  80f ;
					EditorGUIUtility.fieldWidth =  40f ;

					float snapThreshold = EditorGUILayout.FloatField( "Threshold", view.SnapThreshold ) ;
					if( snapThreshold != view.SnapThreshold )
					{
						Undo.RecordObject( view, "UIListView : Snap Threshold Change" ) ;	// アンドウバッファに登録
						view.SnapThreshold = snapThreshold ;
						EditorUtility.SetDirty( view ) ;
					}

					EditorGUIUtility.labelWidth =  40f ;
					EditorGUIUtility.fieldWidth =  40f ;

					float snapTime = EditorGUILayout.FloatField( "Time", view.SnapTime ) ;
					if( snapTime != view.SnapTime )
					{
						Undo.RecordObject( view, "UIListView : Snap Time Change" ) ;	// アンドウバッファに登録
						view.SnapTime = snapTime ;
						EditorUtility.SetDirty( view ) ;
					}

					EditorGUIUtility.labelWidth = 116f ;
					EditorGUIUtility.fieldWidth =  40f ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			if( view.Snap == true )
			{
				UIListView.SnapAnchorTypes snapAnchorType = ( UIListView.SnapAnchorTypes )EditorGUILayout.EnumPopup( "Snap Anchor", view.SnapAnchorType ) ;
				if( snapAnchorType != view.SnapAnchorType )
				{
					Undo.RecordObject( view, "UIListView : Snap Anchor Type Change" ) ;	// アンドウバッファに登録
					view.SnapAnchorType = snapAnchorType ;
					EditorUtility.SetDirty( view ) ;
				}
			}


			// アイテムの更新が必要な時に呼び出されるイベント

			// デリゲートの設定状況
			SerializedObject so = new SerializedObject( view ) ;

			SerializedProperty sp = so.FindProperty( "onUpdateItem" ) ;
			if( sp != null )
			{
				EditorGUILayout.PropertyField( sp ) ;
			}
			so.ApplyModifiedProperties() ;
		}


		//--------------------------------------------------------------------------

		private readonly Dictionary<string,string> mJapanese_Message = new Dictionary<string, string>()
		{
			{ "SetAutoHide",			"Visibility を 'Auto Hide' に設定する事をお勧めします" },
			{ "InteractionNone",		"UIInteraction クラスが必要です" },
			{ "HSB_CanvasGroupNone",	"Horizontal Scrollbar に CanvasGroup クラスが必要です" },
			{ "VSB_CanvasGroupNone",	"Vertical Scrollbar に CanvasGroup クラスが必要です" },
		} ;
		private readonly Dictionary<string,string> mEnglish_Message = new Dictionary<string, string>()
		{
			{ "SetAutoHide",			"Recommend to set Visibility to 'Auto Hide'" },
			{ "InteractionNone",		"'UIInteraction' is necessary" },
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
