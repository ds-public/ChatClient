#if UNITY_EDITOR

using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.U2D ;
using UnityEditor ;
using System.Collections.Generic ;

using TMPro ;

namespace uGUIHelper
{
	/// <summary>
	/// UIView のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIView ), true ) ]
	public class UIViewInspector : Editor
	{
		/// <summary>
		/// スンスペクター描画
		/// </summary>
		public override void OnInspectorGUI()
		{
			serializedObject.UpdateIfRequiredOrScript() ;

			// とりあえずデフォルト
//			DrawDefaultInspector() ;

			//--------------------------------------------

			// ターゲットのインスタンス
			UIView view = target as UIView ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			//-----------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			EditorGUIUtility.labelWidth =  60f ;
			EditorGUIUtility.fieldWidth =  40f ;

			GUI.backgroundColor = Color.cyan ;
			string identity = EditorGUILayout.TextField( "Identity", view.Identity ) ;
			GUI.backgroundColor = Color.white ;
			if( identity != view.Identity )
			{
				Undo.RecordObject( view, "UIView : Identity Change" ) ;	// アンドウバッファに登録
				view.Identity = identity ;
				EditorUtility.SetDirty( view ) ;
			}

			EditorGUIUtility.labelWidth = 116f ;
			EditorGUIUtility.fieldWidth =  40f ;

			// タイムスケール
			float timeScale = EditorGUILayout.FloatField( "TimeScale", view.TimeScale ) ;
			if( timeScale != view.TimeScale )
			{
				Undo.RecordObject( view, "UIView : TimeScale Change" ) ;	// アンドウバッファに登録
				view.TimeScale = timeScale ;
				EditorUtility.SetDirty( view ) ;
			}

			//------------------------------------------

			// キャンバスグループを有効にするかどうか
			DrawCanvasGroup( view ) ;

			// イベントトリガーを有効にするかどうか
			DrawEventTrigger( view ) ;

			// インタラクション
			DrawInteraction( view ) ;

			// トランジション
			DrawTransition( view ) ;

			// Tween の追加と削除
			DrawTween( view ) ;

			//------------------------------------------

			DrawInspectorGUI() ;

			//------------------------------------------

			// エフェクトカテゴリ
			DrawEffect( view ) ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			//------------------------------------------

			// トグルカテゴリ
			DrawToggleCategory( view ) ;

			// レクトマスト２Ｄ
			DrawRectMask2D( view ) ;

			// アルファマスク
			DrawAlphaMask( view ) ;

			// コンテンツサイズフィッター
			DrawContentSizeFitter( view ) ;

			// アニメーター
			DrawAnimator( view ) ;

			if ( serializedObject.hasModifiedProperties )
			{
				serializedObject.ApplyModifiedProperties() ;
			}
		}

		// 派生クラスの個々のＧＵＩを描画する
		virtual protected void DrawInspectorGUI(){}

		// キャンバスグループの設定項目を描画する
		protected void DrawCanvasGroup( UIView view )
		{
			// キャンバスグループを有効にするかどうか

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			// Image が無ければ CanvasRender の追加・削除ができる UI を追加する
			if
			(
				view.GetComponent<Image>() == null				&&
				view.GetComponent<TextMeshProUGUI>() == null	&&
				view.GetComponent<ImageNumber>() == null		&&
				view.GetComponent<Circle>() == null				&&
				view.GetComponent<Line>() == null				&&
				view.GetComponent<Arc>() == null				&&
				view.GetComponent<GridMap>() == null			&&
				view.GetComponent<ComplexRectangle>() == null
			)
			{
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					bool isCanvasRenderer = EditorGUILayout.Toggle( view.IsCanvasRenderer, GUILayout.Width( 16f ) ) ;
					if( isCanvasRenderer != view.IsCanvasRenderer )
					{
						Undo.RecordObject( view, "UIView : Canvas Renderer Change" ) ;	// アンドウバッファに登録
						view.IsCanvasRenderer = isCanvasRenderer ;
						EditorUtility.SetDirty( view ) ;
						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
					GUILayout.Label( "CanvasRenderer" ) ;

					bool isGraphicEmpty = EditorGUILayout.Toggle( view.IsGraphicEmpty, GUILayout.Width( 16f ) ) ;
					if( isGraphicEmpty != view.IsGraphicEmpty )
					{
						Undo.RecordObject( view, "UIView : Graphic Empty Change" ) ;	// アンドウバッファに登録
						view.IsGraphicEmpty = isGraphicEmpty ;
						EditorUtility.SetDirty( view ) ;
						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
					GUILayout.Label( "GraphicEmpty" ) ;
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

//				EditorGUILayout.Separator() ;	// 少し区切りスペース
			}

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool isCanvasGroup = EditorGUILayout.Toggle( view.IsCanvasGroup, GUILayout.Width( 16f ) ) ;
				if( isCanvasGroup != view.IsCanvasGroup )
				{
					Undo.RecordObject( view, "UIView : Canvas Group Change" ) ;	// アンドウバッファに登録
					view.IsCanvasGroup = isCanvasGroup ;
					EditorUtility.SetDirty( view ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( "CanvasGroup" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			if( view.IsCanvasGroup == true )
			{
				EditorGUIUtility.labelWidth =  60f ;
				EditorGUIUtility.fieldWidth =  40f ;

				// スライダーでアルファをコントロール出来るようにする
				float alpha = EditorGUILayout.Slider( "Alpha", view.GetCanvasGroup().alpha, 0.0f, 1.0f ) ;
				if( alpha != view.GetCanvasGroup().alpha )
				{
					Undo.RecordObject( view, "UIView : Alpha Change" ) ;	// アンドウバッファに登録
					view.GetCanvasGroup().alpha = alpha ;
					EditorUtility.SetDirty( view ) ;
				}

				EditorGUIUtility.labelWidth = 116f ;
				EditorGUIUtility.fieldWidth =  40f ;

				// スライダーでアルファをコントロール出来るようにする
				float tDisableRaycastUnderAlpha = EditorGUILayout.Slider( "Disable Raycast Under Alpha", view.DisableRaycastUnderAlpha, 0.0f, 1.0f ) ;
				if( tDisableRaycastUnderAlpha != view.DisableRaycastUnderAlpha )
				{
					Undo.RecordObject( view, "UIView : Disable Raycast Under Alpha Change" ) ;	// アンドウバッファに登録
					view.DisableRaycastUnderAlpha = tDisableRaycastUnderAlpha ;
					EditorUtility.SetDirty( view ) ;
				}
			}
		}

		// イベントトリガーの生成破棄チェックボックスを描画する
		protected void DrawEventTrigger( UIView tTarget )
		{
			EditorGUILayout.Separator() ;	// 少し区切りスペース

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool tIsEventTrigger = EditorGUILayout.Toggle( tTarget.IsEventTrigger, GUILayout.Width( 16f ) ) ;
				if( tIsEventTrigger != tTarget.IsEventTrigger )
				{
					Undo.RecordObject( tTarget, "UIView : EventTrigger Change" ) ;	// アンドウバッファに登録
					tTarget.IsEventTrigger = tIsEventTrigger ;
					EditorUtility.SetDirty( tTarget ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( "EventTrigger" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		}

		// インタラクションの生成破棄チェックボックスを描画する
		protected void DrawInteraction( UIView tTarget )
		{
			EditorGUILayout.Separator() ;	// 少し区切りスペース

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool tIsInteraction = EditorGUILayout.Toggle( tTarget.IsInteraction, GUILayout.Width( 16f ) ) ;
				if( tIsInteraction != tTarget.IsInteraction )
				{
					Undo.RecordObject( tTarget, "UIView : Interaction Change" ) ;	// アンドウバッファに登録
					tTarget.IsInteraction = tIsInteraction ;
					EditorUtility.SetDirty( tTarget ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( "Interaction" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool tIsInteractionForScrollView = EditorGUILayout.Toggle( tTarget.IsInteractionForScrollView, GUILayout.Width( 16f ) ) ;
				if( tIsInteractionForScrollView != tTarget.IsInteractionForScrollView )
				{
					Undo.RecordObject( tTarget, "UIView : Interaction For ScrollView Change" ) ;	// アンドウバッファに登録
					tTarget.IsInteractionForScrollView = tIsInteractionForScrollView ;
					EditorUtility.SetDirty( tTarget ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( "Interaction For ScrollView" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

		}

		// トランジションの生成破棄チェックボックスを描画する
		protected void DrawTransition( UIView tTarget )
		{
			EditorGUILayout.Separator() ;	// 少し区切りスペース

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool tIsTransition = EditorGUILayout.Toggle( tTarget.IsTransition, GUILayout.Width( 16f ) ) ;
				if( tIsTransition != tTarget.IsTransition )
				{
					Undo.RecordObject( tTarget, "UIView : Transition Change" ) ;	// アンドウバッファに登録
					tTarget.IsTransition = tIsTransition ;
					EditorUtility.SetDirty( tTarget ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( "Transition" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool tIsTransitionForScrollView = EditorGUILayout.Toggle( tTarget.IsTransitionForScrollView, GUILayout.Width( 16f ) ) ;
				if( tIsTransitionForScrollView != tTarget.IsTransitionForScrollView )
				{
					Undo.RecordObject( tTarget, "UIView : Transition For ScrollView Change" ) ;	// アンドウバッファに登録
					tTarget.IsTransitionForScrollView = tIsTransitionForScrollView ;
					EditorUtility.SetDirty( tTarget ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( "Transition For ScrollView" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

		}

		// レクトマスク２Ｄの生成破棄チェックボックスを描画する
		protected void DrawRectMask2D( UIView tTarget )
		{
			// オブジェクトの維持フラグ
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool tIsRectMask2D = EditorGUILayout.Toggle( tTarget.IsRectMask2D, GUILayout.Width( 16f ) ) ;
				if( tIsRectMask2D != tTarget.IsRectMask2D )
				{
					Undo.RecordObject( tTarget, "UIView : RectMask2D Change" ) ;	// アンドウバッファに登録
					tTarget.IsRectMask2D = tIsRectMask2D ;
					EditorUtility.SetDirty( tTarget ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( "RectMask2D" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		}

		// アルファマスクの生成破棄チェックボックスを描画する
		protected void DrawAlphaMask( UIView tTarget )
		{
			// オブジェクトの維持フラグ
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool tIsAlphaMaskWindow = EditorGUILayout.Toggle( tTarget.IsAlphaMaskWindow, GUILayout.Width( 16f ) ) ;
				if( tIsAlphaMaskWindow != tTarget.IsAlphaMaskWindow )
				{
					Undo.RecordObject( tTarget, "UIView : AlphaMaskWindow Change" ) ;	// アンドウバッファに登録
					tTarget.IsAlphaMaskWindow = tIsAlphaMaskWindow ;
					EditorUtility.SetDirty( tTarget ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( "AlphaMaskWindow" ) ;

				bool tIsAlphaMaskTarget = EditorGUILayout.Toggle( tTarget.IsAlphaMaskTarget, GUILayout.Width( 16f ) ) ;
				if( tIsAlphaMaskTarget != tTarget.IsAlphaMaskTarget )
				{
					Undo.RecordObject( tTarget, "UIView : AlphaMaskTarget Change" ) ;	// アンドウバッファに登録
					tTarget.IsAlphaMaskTarget = tIsAlphaMaskTarget ;
					EditorUtility.SetDirty( tTarget ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( "AlphaMaskTarget" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		}


		// コンテントサイズフィッターの生成破棄チェックボックスを描画する
		protected void DrawContentSizeFitter( UIView view )
		{
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool isHorizontalLayoutGroup = EditorGUILayout.Toggle( view.IsHorizontalLayoutGroup, GUILayout.Width( 16f ) ) ;
				if( isHorizontalLayoutGroup != view.IsHorizontalLayoutGroup )
				{
					Undo.RecordObject( view, "UIView : HorizontalLayoutGroup Change" ) ;	// アンドウバッファに登録
					view.IsHorizontalLayoutGroup = isHorizontalLayoutGroup ;
					EditorUtility.SetDirty( view ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( "HorizontalLayoutGroup" ) ;

				bool isVerticalLayoutGroup = EditorGUILayout.Toggle( view.IsVerticalLayoutGroup, GUILayout.Width( 16f ) ) ;
				if( isVerticalLayoutGroup != view.IsVerticalLayoutGroup )
				{
					Undo.RecordObject( view, "UIView : VerticalLayoutGroup Change" ) ;	// アンドウバッファに登録
					view.IsVerticalLayoutGroup = isVerticalLayoutGroup ;
					EditorUtility.SetDirty( view ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( "VerticalLayoutGroup" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool isGridLayoutGroup = EditorGUILayout.Toggle( view.IsGridLayoutGroup, GUILayout.Width( 16f ) ) ;
				if( isGridLayoutGroup != view.IsGridLayoutGroup )
				{
					Undo.RecordObject( view, "UIView : GridLayoutGroup Change" ) ;	// アンドウバッファに登録
					view.IsGridLayoutGroup = isGridLayoutGroup ;
					EditorUtility.SetDirty( view ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( "GridLayoutGroup" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool isContentSizeFitter = EditorGUILayout.Toggle( view.IsContentSizeFitter, GUILayout.Width( 16f ) ) ;
				if( isContentSizeFitter != view.IsContentSizeFitter )
				{
					Undo.RecordObject( view, "UIView : ContentSizeFitter Change" ) ;	// アンドウバッファに登録
					view.IsContentSizeFitter = isContentSizeFitter ;
					EditorUtility.SetDirty( view ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( "ContentSizeFitter" ) ;

				bool isLayoutElement = EditorGUILayout.Toggle( view.IsLayoutElement, GUILayout.Width( 16f ) ) ;
				if( isLayoutElement != view.IsLayoutElement )
				{
					Undo.RecordObject( view, "UIView : LayoutElement Change" ) ;	// アンドウバッファに登録
					view.IsLayoutElement = isLayoutElement ;
					EditorUtility.SetDirty( view ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( "LayoutElement" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		}

		// トグルグループの生成破棄チェックボックスを描画する
		protected void DrawToggleCategory( UIView view )
		{
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool isToggleGroup = EditorGUILayout.Toggle( view.IsToggleGroup, GUILayout.Width( 16f ) ) ;
				if( isToggleGroup != view.IsToggleGroup )
				{
					Undo.RecordObject( view, "UIView : ToggleGroup Change" ) ;	// アンドウバッファに登録
					view.IsToggleGroup = isToggleGroup ;
					EditorUtility.SetDirty( view ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( "ToggleGroup" ) ;

				bool isButtonGroup = EditorGUILayout.Toggle( view.IsButtonGroup, GUILayout.Width( 16f ) ) ;
				if( isButtonGroup != view.IsButtonGroup )
				{
					Undo.RecordObject( view, "UIView : ButtonGroup Change" ) ;	// アンドウバッファに登録
					view.IsButtonGroup = isButtonGroup ;
					EditorUtility.SetDirty( view ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( "ButtonGroup" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		}

		// アニメーターの生成破棄チェックボックスを描画する
		protected void DrawAnimator( UIView view )
		{
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool isAnimator = EditorGUILayout.Toggle( view.IsAnimator, GUILayout.Width( 16f ) ) ;
				if( isAnimator != view.IsAnimator )
				{
					Undo.RecordObject( view, "UIView : Animator Change" ) ;	// アンドウバッファに登録
					view.IsAnimator = isAnimator ;
					EditorUtility.SetDirty( view ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( "Animator" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		}

		// Tween の追加と削除
		private string mAddTweenIdentity = "" ;
		private int    mRemoveTweenIndex = 0 ;
		private int    mRemoveTweenIndexAnswer = -1 ;

		protected void DrawTween( UIView view )
		{
			EditorGUILayout.Separator() ;	// 少し区切りスペース

			// 存在している Tween コンポーネントを取得する
			UITween[] tweens = view.GetComponents<UITween>() ;

			// １つ以上存在していればリストとして描画する
			int i, l = tweens.Length, j, c ;
			string identity ;
			string[] tweenIdentities = new string[ l ] ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				tweenIdentities[ i ] = tweens[ i ].Identity ;
			}
			for( i  = 0 ; i <  l ; i ++ )
			{
				// 既に同じ名前が存在する場合は番号を振る
				identity = tweenIdentities[ i ] ;

				c = 0 ;
				for( j  = i + 1 ; j <  l ; j ++ )
				{
					if( tweenIdentities[ j ] == identity )
					{
						// 同じ名前を発見した
						c ++ ;
						tweenIdentities[ j ] = tweenIdentities[ j ] + "(" + c + ")" ;
					}
				}
			}

			//----------------------------------------------------

			if( mRemoveTweenIndexAnswer <  0 )
			{
				GUILayout.BeginHorizontal() ;	// 横並び開始
				{
					bool isAdd = false ;

					GUI.backgroundColor = Color.cyan ;
					if( GUILayout.Button( "Add Tween", GUILayout.Width( 140f ) ) == true )
					{
						isAdd = true ;
					}
					GUI.backgroundColor = Color.white ;

					GUI.backgroundColor = Color.cyan ;
					mAddTweenIdentity = EditorGUILayout.TextField( "", mAddTweenIdentity, GUILayout.Width( 120f ) ) ;
					GUI.backgroundColor = Color.white ;

					if( isAdd == true )
					{
						if( string.IsNullOrEmpty( mAddTweenIdentity ) == false )
						{
							// Tween を追加する
							UITween tween = view.AddComponent<UITween>() ;
							tween.Identity = mAddTweenIdentity ;

							// 追加後の全ての Tween を取得する
							UITween[] temporaryTweens = view.gameObject.GetComponents<UITween>() ;
							if( temporaryTweens != null && temporaryTweens.Length >  0 )
							{
								for( i  = 0 ; i <  temporaryTweens.Length ; i ++ )
								{
									if( temporaryTweens[ i ] != tween )
									{
										break ;
									}
								}
								if( i <  temporaryTweens.Length )
								{
									// 既にトゥイーンコンポーネントがアタッチされているので enable と PlayOnAwake を false にする
									tween.enabled = false ;
									tween.PlayOnAwake = false ;
								}
							}
						}
						else
						{
							EditorUtility.DisplayDialog( "Add Tween", GetMessage( "InputIdentity" ), "Close" ) ;
						}
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				if( tweens != null && tweens.Length >  0 )
				{
					GUILayout.BeginHorizontal() ;	// 横並び開始
					{
						bool isRemove = false ;
						GUI.backgroundColor = Color.red ;	// ボタンの下地を緑に
						if( GUILayout.Button( "Remove Tween", GUILayout.Width( 140f ) ) == true )
						{
							isRemove = true ;
						}
						GUI.backgroundColor = Color.white ;	// ボタンの下地を緑に

						if( mRemoveTweenIndex >= tweenIdentities.Length )
						{
							mRemoveTweenIndex  = tweenIdentities.Length - 1 ;
						}
						mRemoveTweenIndex = EditorGUILayout.Popup( "", mRemoveTweenIndex, tweenIdentities, GUILayout.Width( 120f ) ) ;	// フィールド名有りタイプ

						if( isRemove == true )
						{
							// 削除する
							mRemoveTweenIndexAnswer = mRemoveTweenIndex ;
						}
					}
					GUILayout.EndHorizontal() ;		// 横並び終了
				}
			}
			else
			{
				string message = GetMessage( "RemoveTweenOK?" ).Replace( "%1", tweenIdentities[ mRemoveTweenIndexAnswer ] ) ;
				GUILayout.Label( message ) ;
	//			GUILayout.Label( "It does really may be to remove tween '" + tTweenIdentityArray[ mRemoveTweenIndexAnswer ] + "' ?" ) ;
				GUILayout.BeginHorizontal() ;	// 横並び開始
				{
					GUI.backgroundColor = Color.red ;
					if( GUILayout.Button( "OK", GUILayout.Width( 100f ) ) == true )
					{
						// 本当に削除する
						Undo.RecordObject( view, "UIView : Tween Remove" ) ;	// アンドウバッファに登録
						view.RemoveTweenIdentity = tweens[ mRemoveTweenIndexAnswer ].Identity ;
						view.RemoveTweenInstance = tweens[ mRemoveTweenIndexAnswer ].GetInstanceID() ;
						EditorUtility.SetDirty( view ) ;
						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;

						mRemoveTweenIndexAnswer = -1 ;
					}
					GUI.backgroundColor = Color.white ;
					if( GUILayout.Button( "Cancel", GUILayout.Width( 100f ) ) == true )
					{
						mRemoveTweenIndexAnswer = -1 ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}
		}



		// Filipper の追加と削除
		private string mAddFlipperIdentity = "" ;
		private int    mRemoveFlipperIndex = 0 ;
		private int    mRemoveFlipperIndexAnswer = -1 ;

		protected void DrawFlipper( UIView tTarget )
		{
			EditorGUILayout.Separator() ;	// 少し区切りスペース

			// 存在している Tween コンポーネントを取得する
			UIFlipper[] tFlipperArray = tTarget.GetComponents<UIFlipper>() ;

			// １つ以上存在していればリストとして描画する
			int i, l = tFlipperArray.Length, j, c ;
			string tIdentity ;
			string[] tFlipperIdentityArray = new string[ l ] ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				tFlipperIdentityArray[ i ] = tFlipperArray[ i ].Identity ;
			}
			for( i  = 0 ; i <  l ; i ++ )
			{
				// 既に同じ名前が存在する場合は番号を振る
				tIdentity = tFlipperIdentityArray[ i ] ;

				c = 0 ;
				for( j  = i + 1 ; j <  l ; j ++ )
				{
					if( tFlipperIdentityArray[ j ] == tIdentity )
					{
						// 同じ名前を発見した
						c ++ ;
						tFlipperIdentityArray[ j ] = tFlipperIdentityArray[ j ] + "(" + c + ")" ;
					}
				}
			}

			//----------------------------------------------------

			if( mRemoveFlipperIndexAnswer <  0 )
			{
				GUILayout.BeginHorizontal() ;	// 横並び開始
				{
					bool tAdd = false ;

					GUI.backgroundColor = Color.cyan ;
					if( GUILayout.Button( "Add Flipper", GUILayout.Width( 140f ) ) == true )
					{
						tAdd = true ;
					}
					GUI.backgroundColor = Color.white ;

					GUI.backgroundColor = Color.cyan ;
					mAddFlipperIdentity = EditorGUILayout.TextField( "", mAddFlipperIdentity, GUILayout.Width( 120f ) ) ;
					GUI.backgroundColor = Color.white ;

					if( tAdd == true )
					{
						if( string.IsNullOrEmpty( mAddFlipperIdentity ) == false )
						{
							// Flipper を追加する
							UIFlipper tFlipper = tTarget.AddComponent<UIFlipper>() ;
							tFlipper.Identity = mAddFlipperIdentity ;

							UIFlipper[] tFlipperList = tTarget.gameObject.GetComponents<UIFlipper>() ;
							if( tFlipperList != null && tFlipperList.Length >  0 )
							{
								for( i  = 0 ; i <  tFlipperList.Length ; i ++ )
								{
									if( tFlipperList[ i ] != tFlipper )
									{
										break ;
									}
								}
								if( i <  tFlipperList.Length )
								{
									// 既にトゥイーンコンポーネントがアタッチされているので enable と PlayOnAwake を false にする
									tFlipper.enabled = false ;
									tFlipper.PlayOnAwake = false ;
								}
							}
						}
						else
						{
							EditorUtility.DisplayDialog( "Add Flipper", GetMessage( "InputIdentity" ), "Close" ) ;
						}
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				if( tFlipperArray != null && tFlipperArray.Length >  0 )
				{
					GUILayout.BeginHorizontal() ;	// 横並び開始
					{
						bool tRemove = false ;
						GUI.backgroundColor = Color.red ;	// ボタンの下地を緑に
						if( GUILayout.Button( "Remove Flipper", GUILayout.Width( 140f ) ) == true )
						{
							tRemove = true ;
						}
						GUI.backgroundColor = Color.white ;	// ボタンの下地を緑に

						if( mRemoveFlipperIndex >= tFlipperIdentityArray.Length )
						{
							mRemoveFlipperIndex  = tFlipperIdentityArray.Length - 1 ;
						}
						mRemoveFlipperIndex = EditorGUILayout.Popup( "", mRemoveFlipperIndex, tFlipperIdentityArray, GUILayout.Width( 120f ) ) ;	// フィールド名有りタイプ

						if( tRemove == true )
						{
							// 削除する
							mRemoveFlipperIndexAnswer = mRemoveFlipperIndex ;
						}
					}
					GUILayout.EndHorizontal() ;		// 横並び終了
				}
			}
			else
			{
				string tMessage = GetMessage( "RemoveFlipperOK?" ).Replace( "%1", tFlipperIdentityArray[ mRemoveFlipperIndexAnswer ] ) ;
				GUILayout.Label( tMessage ) ;
				GUILayout.BeginHorizontal() ;	// 横並び開始
				{
					GUI.backgroundColor = Color.red ;
					if( GUILayout.Button( "OK", GUILayout.Width( 100f ) ) == true )
					{
						// 本当に削除する
						Undo.RecordObject( tTarget, "UIView : Flipper Remove" ) ;	// アンドウバッファに登録
						tTarget.RemoveFlipperIdentity = tFlipperArray[ mRemoveFlipperIndexAnswer ].Identity ;
						tTarget.RemoveFlipperInstance = tFlipperArray[ mRemoveFlipperIndexAnswer ].GetInstanceID() ;
						EditorUtility.SetDirty( tTarget ) ;
						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;

						mRemoveFlipperIndexAnswer = -1 ;
					}
					GUI.backgroundColor = Color.white ;
					if( GUILayout.Button( "Cancel", GUILayout.Width( 100f ) ) == true )
					{
						mRemoveFlipperIndexAnswer = -1 ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}
		}

		//--------------------------------------------------------------------------

		// エフェクト系のコンポーネントを追加するかどうか
		protected void DrawEffect( UIView tTarget )
		{
			if( tTarget.GetCanvasRenderer() != null )
			{
				EditorGUILayout.Separator() ;	// 少し区切りスペース

				EditorGUIUtility.labelWidth =  60f ;
				EditorGUIUtility.fieldWidth =  40f ;

				GUILayout.BeginHorizontal() ;	// 横並び
				{
					bool tIsMask = EditorGUILayout.Toggle( tTarget.IsMask, GUILayout.Width( 16f ) ) ;
					if( tIsMask != tTarget.IsMask )
					{
						Undo.RecordObject( tTarget, "UIView : Mask Change" ) ;	// アンドウバッファに登録
						tTarget.IsMask = tIsMask ;
						EditorUtility.SetDirty( tTarget ) ;
					}
					GUILayout.Label( "Mask" ) ;

					bool tIsInversion = EditorGUILayout.Toggle( tTarget.IsInversion, GUILayout.Width( 16f ) ) ;
					if( tIsInversion != tTarget.IsInversion )
					{
						Undo.RecordObject( tTarget, "UIView : Inversion Change" ) ;	// アンドウバッファに登録
						tTarget.IsInversion = tIsInversion ;
						EditorUtility.SetDirty( tTarget ) ;
					}
					GUILayout.Label( "Inversion" ) ;

				}
				GUILayout.EndHorizontal() ;		// 横並び終了


				//-----------------------------------------------------------------------------------------

				bool tShow = true ;

				if( tTarget.GetComponent<TextMeshProUGUI>() != null )
				{
					tShow = false ;
				}

				if( tShow == true )
				{
					EditorGUIUtility.labelWidth =  60f ;
					EditorGUIUtility.fieldWidth =  40f ;

					GUILayout.BeginHorizontal() ;	// 横並び
					{
		//				GUILayout.Label( "UI Effect" ) ;

						bool tIsShadow = EditorGUILayout.Toggle( tTarget.IsShadow, GUILayout.Width( 16f ) ) ;
						if( tIsShadow != tTarget.IsShadow )
						{
							Undo.RecordObject( tTarget, "UIView : Shadow Change" ) ;	// アンドウバッファに登録
							tTarget.IsShadow = tIsShadow ;
							EditorUtility.SetDirty( tTarget ) ;
						}
						GUILayout.Label( "Shadow" ) ;

						bool tIsOutline = EditorGUILayout.Toggle( tTarget.IsOutline, GUILayout.Width( 16f ) ) ;
						if( tIsOutline != tTarget.IsOutline )
						{
							Undo.RecordObject( tTarget, "UIView : Outline Change" ) ;	// アンドウバッファに登録
							tTarget.IsOutline = tIsOutline ;
							EditorUtility.SetDirty( tTarget ) ;
						}
						GUILayout.Label( "Outline" ) ;


						bool tIsGradient = EditorGUILayout.Toggle( tTarget.IsGradient, GUILayout.Width( 16f ) ) ;
						if( tIsGradient != tTarget.IsGradient )
						{
							Undo.RecordObject( tTarget, "UIView : Gradient Change" ) ;	// アンドウバッファに登録
							tTarget.IsGradient = tIsGradient ;
							EditorUtility.SetDirty( tTarget ) ;
						}
						GUILayout.Label( "Gradient" ) ;
					}
					GUILayout.EndHorizontal() ;     // 横並び終了
				}

				EditorGUIUtility.labelWidth = 116f ;
				EditorGUIUtility.fieldWidth =  40f ;
			}
		}

		// マテリアルの描画
		protected void DrawMaterial( UIView view )
		{
			if( view.GetCanvasRenderer() != null )
			{
				EditorGUILayout.Separator() ;	// 少し区切りスペース

				// タイプ
				UIView.MaterialTypes materialType = ( UIView.MaterialTypes )EditorGUILayout.EnumPopup( "Material Type",  view.MaterialType ) ;
				if( materialType != view.MaterialType )
				{
					Undo.RecordObject( view, "UIView : Material Type Change" ) ;	// アンドウバッファに登録
					view.MaterialType = materialType ;
					EditorUtility.SetDirty( view ) ;
				}

				// Sepia
				if( view.MaterialType == UIView.MaterialTypes.Sepia )
				{
					EditorGUIUtility.labelWidth = 120f ;
					EditorGUIUtility.fieldWidth =  40f ;

					float sepiaDark = EditorGUILayout.Slider( "Sepia Dark", view.SepiaDark, 0.0f, 0.2f ) ;
					if( sepiaDark != view.SepiaDark )
					{
						Undo.RecordObject( view, "UIView : Sepia Dark Change" ) ;	// アンドウバッファに登録
						view.SepiaDark = sepiaDark ;
						EditorUtility.SetDirty( view ) ;
					}

					float sepiaStrength = EditorGUILayout.Slider( "Sepia Strength", view.SepiaStrength, 0.05f, 0.15f ) ;
					if( sepiaStrength != view.SepiaStrength )
					{
						Undo.RecordObject( view, "UIView : Sepia Strength Change" ) ;	// アンドウバッファに登録
						view.SepiaStrength = sepiaStrength ;
						EditorUtility.SetDirty( view ) ;
					}

					float sepiaInterpolation = EditorGUILayout.Slider( "Sepia Intepolation", view.SepiaInterpolation, 0f, 1f ) ;
					if( sepiaInterpolation != view.SepiaInterpolation )
					{
						Undo.RecordObject( view, "UIView : Sepia Interpolation Change" ) ;	// アンドウバッファに登録
						view.SepiaInterpolation = sepiaInterpolation ;
						EditorUtility.SetDirty( view ) ;
					}

					EditorGUIUtility.labelWidth = 116f ;
					EditorGUIUtility.fieldWidth =  40f ;
				}

				// Interpolation
				if( view.MaterialType == UIView.MaterialTypes.Interpolation )
				{
					EditorGUIUtility.labelWidth = 120f ;
					EditorGUIUtility.fieldWidth =  40f ;

					float interpolationValue = EditorGUILayout.Slider( "Interpolation Value", view.InterpolationValue, 0.0f, 1.0f ) ;
					if( interpolationValue != view.InterpolationValue )
					{
						Undo.RecordObject( view, "UIView : Interpolation Value Change" ) ;	// アンドウバッファに登録
						view.InterpolationValue = interpolationValue ;
						EditorUtility.SetDirty( view ) ;
					}

					Color interpolationColor = new Color( view.InterpolationColor.r, view.InterpolationColor.g, view.InterpolationColor.b, view.InterpolationColor.a ) ;
					interpolationColor = EditorGUILayout.ColorField( "Interpolation Color", interpolationColor ) ;
					if( interpolationColor.Equals( view.InterpolationColor ) == false )
					{
						Undo.RecordObject( view, "UIView : Interpolation Color Change" ) ;	// アンドウバッファに登録
						view.InterpolationColor = interpolationColor ;
						EditorUtility.SetDirty( view ) ;
					}

					EditorGUIUtility.labelWidth = 116f ;
					EditorGUIUtility.fieldWidth =  40f ;
				}

				// Mosaic
				if( view.MaterialType == UIView.MaterialTypes.Mosaic )
				{
					EditorGUIUtility.labelWidth = 120f ;
					EditorGUIUtility.fieldWidth =  40f ;

					float mosaicIntensity = EditorGUILayout.Slider( "Mosaic Intensity", view.MosaicIntensity, 0.0f, 1.0f ) ;
					if( mosaicIntensity != view.MosaicIntensity )
					{
						Undo.RecordObject( view, "UIView : Mosaic Intensity Change" ) ;	// アンドウバッファに登録
						view.MosaicIntensity = mosaicIntensity ;
						EditorUtility.SetDirty( view ) ;
					}

					GUILayout.BeginHorizontal() ;	// 横並び
					{
						bool mosaicSquareization = EditorGUILayout.Toggle( view.MosaicSquareization, GUILayout.Width( 16f ) ) ;
						if( mosaicSquareization != view.MosaicSquareization )
						{
							Undo.RecordObject( view, "UIView : Mosaic Squareization Change" ) ;	// アンドウバッファに登録
							view.MosaicSquareization = mosaicSquareization ;
							EditorUtility.SetDirty( view ) ;
						}
						GUILayout.Label( "Mosaic Squareization" ) ;
					}
					GUILayout.EndHorizontal() ;     // 横並び終了


					EditorGUIUtility.labelWidth = 116f ;
					EditorGUIUtility.fieldWidth =  40f ;
				}
			}
		}

		// AtlasSprite の項目を描画する
		protected void DrawAtlas( UIView view )
		{
			UIImage image ;
			if( view is UIImage )
			{
				image = view as UIImage ;
			}
			else
			{
				return ;
			}

			//----------------------------------------------------------

			// スプライトアトラス
			SpriteAtlas spriteAtlas = EditorGUILayout.ObjectField( "Sprite Atlas", image.SpriteAtlas, typeof( SpriteAtlas ), false ) as SpriteAtlas ;
			if( spriteAtlas != image.SpriteAtlas )
			{
				Undo.RecordObject( image, "Sprite Atlas : Change" ) ;	// アンドウバッファに登録

				// SpriteAtlas 側を設定する
				image.SpriteAtlas = spriteAtlas ;

				// SpriteSet 側を消去する
				image.SpriteSet = null ;

				if( image.Sprite == null )
				{
					// スプライトが設定されていなければデフォルトを設定する
					Sprite[] sprites = GetSprites( image.SpriteAtlas ) ;
					if( sprites != null && sprites.Length >  0 )
					{
						image.Sprite = sprites[ 0 ] ;
					}
				}

				EditorUtility.SetDirty( image ) ;
			}

			if( image.SpriteAtlas != null )
			{
				// スプライトアトラスのテクスチャ(表示のみ)
//				EditorGUILayout.ObjectField( "Sprite Atlas Texture", image.SpriteAtlasTexture, typeof( Texture2D ), true ) ;

				//---------------------------------

				Sprite[] sprites = GetSprites( image.SpriteAtlas ) ;
				if( sprites != null && sprites.Length >  0 )
				{
					int i, l = sprites.Length ;
					List<string> spriteNames = new List<string>() ;
					foreach( var sprite in sprites )
					{
						spriteNames.Add( sprite.name ) ;
					}

					string currentSpriteName = null ;
					if( image.Sprite != null )
					{
						currentSpriteName = image.Sprite.name ;
					}

					int indexBase = -1 ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						if( spriteNames[ i ] == currentSpriteName )
						{
							indexBase = i ;
							break ;
						}
					}

					int indexMove = 0 ;
					if( indexBase <  0 )
					{
						spriteNames.Insert( 0, "Unknown" ) ;
						indexBase = 0 ;
						indexMove = 1 ;
					}

					// フレーム番号
					int index = EditorGUILayout.Popup( "Selected Sprite", indexBase, spriteNames.ToArray() ) ;
					if( index != indexBase )
					{
						Undo.RecordObject( image, "UIImage Sprite : Change" ) ;	// アンドウバッファに登録
						image.Sprite = sprites[ index - indexMove ] ;
						EditorUtility.SetDirty( image ) ;
					}
				}

				// 確認用
				EditorGUILayout.ObjectField( " ", image.Sprite, typeof( Sprite ), false ) ;
			}

			//----------------------------------------------------------
			// 以下はレガシー

			EditorGUILayout.Separator() ;	// 少し区切りスペース
			EditorGUILayout.Separator() ;	// 少し区切りスペース

			Texture spriteSetTextureActive = null ;
			if( image.SpriteSet != null )
			{
				spriteSetTextureActive= image.SpriteSet.Texture ;
			}

			Texture spriteSetTextureChange = EditorGUILayout.ObjectField( "Sprite Set", spriteSetTextureActive, typeof( Texture ), false ) as Texture ;
			if( spriteSetTextureChange != spriteSetTextureActive )
			{
				Undo.RecordObject( image, "SpriteSet Texture : Change" ) ;	// アンドウバッファに登録

				// SpriteSet 側を設定する
				RefreshSpriteSet( image, spriteSetTextureChange ) ;

				// SpriteAtlas 側を消去する
				image.SpriteAtlas = null ;

				if( image.Sprite == null )
				{
					// スプライトが設定されていなければデフォルトを設定する
					Sprite[] sprites = image.SpriteSet.GetSprites() ;
					if( sprites != null && sprites.Length >  0 )
					{
						image.Sprite = sprites[ 0 ] ;
					}
				}
				EditorUtility.SetDirty( image ) ;
			}

			if( image.SpriteSet != null )
			{
				spriteSetTextureActive = image.SpriteSet.Texture ;
			}

			if( spriteSetTextureActive != null )
			{
				//-----------------------------------------------------

				// 一覧から選択出来るようにする

				EditorGUILayout.Separator() ;	// 少し区切りスペース

				if( GUILayout.Button( "Reload Sprites In SpriteSet", GUILayout.Width( 240f ) ) == true || ( image.SpriteSet.IsAvailable == false && image.SpriteSet.Texture != null && Application.isPlaying == false ) )
				{
					// データに異常が発生しているので自動的に更新する
					if( image.SpriteSet.IsAvailable == false && image.SpriteSet.Texture != null && Application.isPlaying == false )
					{
						Debug.LogWarning( "SpriteSet に内包される Sprites を自動的に更新:" + image.SpriteSet.Texture.name ) ;
					}

					RefreshSpriteSet( image, image.SpriteSet.Texture ) ;

					EditorUtility.SetDirty( image ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}

				string[] spriteNames = image.SpriteSet.GetSpriteNames() ;
				if( spriteNames != null && spriteNames.Length >  0 )
				{
					// ソートする
					List<string> sortedSpriteNames = new List<string>() ;

					int i, l = spriteNames.Length ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						sortedSpriteNames.Add( spriteNames[ i ] ) ;
					}
					sortedSpriteNames.Sort() ;
					spriteNames = sortedSpriteNames.ToArray() ;

					string currentSpriteName = null ;
					if( image.Sprite != null )
					{
						currentSpriteName = image.Sprite.name ;
					}

					int indexBase = -1 ;

					for( i  = 0 ; i <  l ; i ++ )
					{
						if( spriteNames[ i ] == currentSpriteName )
						{
							indexBase = i ;
							break ;
						}
					}

					if( indexBase <  0 )
					{
						List<string> temporarySpriteNames = new List<string>()
						{
							"Unknown"
						} ;

						for( i  = 0 ; i <  l ; i ++ )
						{
							temporarySpriteNames.Add( spriteNames[ i ] ) ;
						}

						spriteNames = temporarySpriteNames.ToArray() ;

						indexBase = 0 ;
					}

					// フレーム番号
					int index = EditorGUILayout.Popup( "Selected Sprite", indexBase, spriteNames ) ;
					if( index != indexBase )
					{
						Undo.RecordObject( image, "UIImage Sprite : Change" ) ;	// アンドウバッファに登録
						image.SetSpriteInAtlas( spriteNames[ index ] ) ;
						EditorUtility.SetDirty( image ) ;
					}

					// 確認用
					EditorGUILayout.ObjectField( " ", image.Sprite, typeof( Sprite ), false ) ;
				}
			}
		}

#if UNITY_EDITOR
		/// <summary>
		/// エディター専用のスプライトアトラスからオリジナルパーツスプライトのインスタンスを取得する
		/// </summary>
		/// <param name="spriteAtlaa"></param>
		/// <returns></returns>
		private Sprite[] GetSprites( SpriteAtlas spriteAtlas )
		{
			SerializedObject so = new SerializedObject( spriteAtlas ) ;
			if( so == null )
			{
				return null ;
			}

			//----------------------------------

			List<Sprite> sprites = new List<Sprite>() ;

			// VSの軽度ワーニングが煩わしいので using は使わず Dispose() を使用 
			SerializedProperty property = so.GetIterator() ;
			while( property != null )
			{
				// 有効な参照のみピックアップする
				if
				(
					( property.propertyType						== SerializedPropertyType.ObjectReference	) &&
					( property.objectReferenceValue				!= null										) &&
					( property.objectReferenceInstanceIDValue	!= 0										)
				)
				{
					if( property.propertyPath.IndexOf( "m_PackedSprites.Array.data" ) == 0 && property.type == "PPtr<Sprite>" )
					{
						// オリジナルパーツスプライトへの直接参照を発見した
						sprites.Add( property.objectReferenceValue as Sprite ) ;
					}
				}

				if( property.Next( true ) == false )
				{
					break ;
				}
			}
			so.Dispose() ;

			if( sprites.Count == 0 )
			{
				return null ;
			}

			// ソート
			sprites.Sort( ( a, b ) => string.Compare( a.name, b.name ) ) ;

			return sprites.ToArray() ;
		}
#endif

		// いずれ削除する
		private void RefreshSpriteSet( UIImage image, Texture atlasTexture )
		{
			List<Sprite> targetSprites = new List<Sprite>() ;

			if( atlasTexture != null )
			{
				string path = AssetDatabase.GetAssetPath( atlasTexture.GetInstanceID() ) ;

				// テクスチャからパスを取得してマルチタイプスプライトとしてロードする
				UnityEngine.Object[] allSprites = AssetDatabase.LoadAllAssetsAtPath( path ) ;

				if( allSprites != null && allSprites.Length >  0 )
				{
					int i, l = allSprites.Length ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						if( allSprites[ i ] is UnityEngine.Sprite )
						{
							targetSprites.Add( allSprites[ i ] as UnityEngine.Sprite ) ;
						}
					}
				}

				if( targetSprites.Count >  0 )
				{
					// 存在するので更新する

//					UIAtlasSprite tAtlasSprite = UIAtlasSprite.Create() ;
					image.SpriteSet.ClearSprites() ;
					image.SpriteSet.SetSprites( targetSprites.ToArray() ) ;
//					tTarget.atlasSprite = tAtlasSprite ;
				}
				else
				{
					// 存在しないのでクリアする
					if(	image.SpriteSet != null )
					{
						image.SpriteSet.ClearSprites() ;
					}
				}

				// SpriteAtlas 側を消去する
				image.SpriteAtlas = null ;
			}
			else
			{
				if(	image.SpriteSet != null )
				{
					image.SpriteSet.ClearSprites() ;
				}
			}
		}

		//--------------------------------------------------------------------------------

		// 整数タイプの４次元値の表示と選択
		private Vector4 Float4Field( string tPrefix, float tPrefixLength, string l0, string l1, string l2, string l3, float tCaptionLength, Vector4 vi )
		{
			Vector4 vo = new Vector4() ;

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				if( string.IsNullOrEmpty( tPrefix ) )
				{
					if( tPrefixLength >  0 )
					{
						GUILayout.Space( tPrefixLength ) ;        // null なら 82
					}
				}
				else
				{
					GUILayout.Label( tPrefix, GUILayout.Width( tPrefixLength ) ) ;	// null でないなら 74
				}

				EditorGUIUtility.labelWidth = tCaptionLength ;
				EditorGUIUtility.fieldWidth =  50f ;

				vo.x = ( float )EditorGUILayout.FloatField( l0, ( int )vi.x, GUILayout.MinWidth( 10f ) ) ;
				vo.y = ( float )EditorGUILayout.FloatField( l1, ( int )vi.y, GUILayout.MinWidth( 10f ) ) ;
				vo.z = ( float )EditorGUILayout.FloatField( l2, ( int )vi.z, GUILayout.MinWidth( 10f ) ) ;
				vo.w = ( float )EditorGUILayout.FloatField( l3, ( int )vi.w, GUILayout.MinWidth( 10f ) ) ;

				EditorGUIUtility.labelWidth = 116f ;
				EditorGUIUtility.fieldWidth =  50f ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			return vo ;
		}

		protected Color CloneColor( Color c1 )
		{
			return new Color( c1.r, c1.g, c1.b, c1.a ) ;
		}

		protected bool CheckColor( Color c1, Color c2 )
		{
			if( c1.r != c2.r || c1.g != c2.g || c1.b != c2.b || c1.a != c2.a )
			{
				return false ;
			}

			return true ;
		}

		//--------------------------------------------------------------------------

		private readonly Dictionary<string,string> m_Japanese_Message = new Dictionary<string, string>()
		{
			{ "RemoveTweenOK?",   "Tween [ %1 ] を削除してもよろしいですか？" },
			{ "RemoveFlipperOK?", "Flipper [ %1 ] を削除してもよろしいですか？" },
			{ "EventTriggerNone", "EventTrigger クラスが必要です" },
			{ "InputIdentity",   "識別子を入力してください" },
		} ;
		private readonly Dictionary<string,string> m_English_Message = new Dictionary<string, string>()
		{
			{ "RemoveTweenOK?",   "It does really may be to remove tween %1 ?" },
			{ "RemoveFlipperOK?", "It does really may be to remove flipper %1 ?" },
			{ "EventTriggerNone", "'EventTrigger' is necessary." },
			{ "InputIdentity",   "Input identity !" },
		} ;

		private string GetMessage( string tLabel )
		{
			if( Application.systemLanguage == SystemLanguage.Japanese )
			{
				if( m_Japanese_Message.ContainsKey( tLabel ) == false )
				{
					return "指定のラベル名が見つかりません" ;
				}
				return m_Japanese_Message[ tLabel ] ;
			}
			else
			{
				if( m_English_Message.ContainsKey( tLabel ) == false )
				{
					return "Specifying the label name can not be found" ;
				}
				return m_English_Message[ tLabel ] ;
			}
		}
	}
}

#endif
