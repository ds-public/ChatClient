#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UITween のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UITween ) ) ]
	public class UITweenInspector : Editor
	{
		// スンスペクター描画
		public override void OnInspectorGUI()
		{
			// とりあえずデフォルト
	//		DrawDefaultInspector() ;
		
			//--------------------------------------------

			// ターゲットのインスタンス
			UITween view = target as UITween ;
		
			EditorGUILayout.Separator() ;	// 少し区切りスペース

			// 識別子
			GUI.backgroundColor = Color.cyan ;
			string identity = EditorGUILayout.TextField( "Identity",  view.Identity ) ;
			GUI.backgroundColor = Color.white ;
			if( identity != view.Identity )
			{
				Undo.RecordObject( view, "UITween : Identity Change" ) ;	// アンドウバッファに登録
				view.Identity = identity ;
				EditorUtility.SetDirty( view ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}
		
			// ディレイ
			float delay = EditorGUILayout.FloatField( "Delay", view.Delay ) ;
			if( delay != view.Delay )
			{
				Undo.RecordObject( view, "UITween : Delay Change" ) ;	// アンドウバッファに登録
				view.Delay = delay ;
				EditorUtility.SetDirty( view ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			if( view.AnimationClip == null )
			{
				// デュアレーション
				float duration = EditorGUILayout.FloatField( "Duration", view.Duration ) ;
				if( duration != view.Duration )
				{
					Undo.RecordObject( view, "UITween : Duration Change" ) ;	// アンドウバッファに登録
					view.Duration = duration ;
					EditorUtility.SetDirty( view ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
			}

			//------------------------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			// ワイドモードを有効にする
			bool wideMode = EditorGUIUtility.wideMode ;
			EditorGUIUtility.wideMode = true ;

			if( view.AnimationClip == null )
			{
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					if( view.PositionEnabled == false )
					{
						GUILayout.Label( "Position Enabled" /*, GUILayout.Width( 116f ) */ ) ;
					}
					else
					{
						view.PositionFoldOut = EditorGUILayout.Foldout( view.PositionFoldOut, "Position Enabled" ) ;
					}

					bool positionEnabled = EditorGUILayout.Toggle( view.PositionEnabled, GUILayout.Width( 24f ) ) ;
					if( positionEnabled != view.PositionEnabled )
					{
						Undo.RecordObject( view, "UITween : Position Enabled Change" ) ;	// アンドウバッファに登録
						view.PositionEnabled = positionEnabled ;
						EditorUtility.SetDirty( view ) ;
	//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				if( view.PositionEnabled == true && view.PositionFoldOut == true )
				{
					// ポジション

					Vector3 positionFrom = EditorGUILayout.Vector3Field( " From",  view.PositionFrom /*, GUILayout.MaxWidth( 100f ) */ ) ;
					if( positionFrom != view.PositionFrom )
					{
						Undo.RecordObject( view, "UITween : Position From Change" ) ;	// アンドウバッファに登録
						view.PositionFrom = positionFrom ;
						EditorUtility.SetDirty( view ) ;
	//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}

					Vector3 positionTo = EditorGUILayout.Vector3Field( " To",  view.PositionTo /*, GUILayout.MaxWidth( 100f ) */ ) ;
					if( positionTo != view.PositionTo )
					{
						Undo.RecordObject( view, "UITween : Position To Change" ) ;	// アンドウバッファに登録
						view.PositionTo = positionTo ;
						EditorUtility.SetDirty( view ) ;
	//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}

					// プロセスタイプ
					UITween.ProcessTypes positionProcessType = ( UITween.ProcessTypes )EditorGUILayout.EnumPopup( " Process Type",  view.PositionProcessType ) ;
					if( positionProcessType != view.PositionProcessType )
					{
						Undo.RecordObject( view, "UITween : Position Process Type Change" ) ;	// アンドウバッファに登録
						view.PositionProcessType = positionProcessType ;
						EditorUtility.SetDirty( view ) ;
	//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}

					if( view.PositionProcessType == UITween.ProcessTypes.Ease )
					{
						// イーズタイプ
						UITween.EaseTypes positionEaseType = ( UITween.EaseTypes )EditorGUILayout.EnumPopup( " EaseType",  view.PositionEaseType ) ;
						if( positionEaseType != view.PositionEaseType )
						{
							Undo.RecordObject( view, "UITween : Position Ease Type Change" ) ;	// アンドウバッファに登録
							view.PositionEaseType = positionEaseType ;
							EditorUtility.SetDirty( view ) ;
	//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
						}
					}
					else
					if( view.PositionProcessType == UITween.ProcessTypes.AnimationCurve )
					{
						AnimationCurve animationCurve = new AnimationCurve( view.PositionAnimationCurve.keys ) ;
						view.PositionAnimationCurve = EditorGUILayout.CurveField( " Animation Curve", animationCurve /*, GUILayout.Width( 170f ), GUILayout.Height( 52f )*/ ) ;
					}

					if( view.IsChecker == true )
					{
						DrawCurve( view, view.CheckFactor, view.PositionProcessType, view.PositionEaseType, view.PositionAnimationCurve ) ;
					}

					if( view.GetComponent<RectTransform>() == null )
					{
						EditorGUILayout.HelpBox( GetMessage( "RectTransformNone" ), MessageType.Warning, true ) ;		
					}
				}

				//--------------

				GUILayout.BeginHorizontal() ;	// 横並び
				{
					if( view.RotationEnabled == false )
					{
						GUILayout.Label( "Rotation Enabled" /*, GUILayout.Width( 116f ) */ ) ;
					}
					else
					{
						view.RotationFoldOut = EditorGUILayout.Foldout( view.RotationFoldOut, "Rotation Enabled" ) ;
					}

					bool rotationEnabled = EditorGUILayout.Toggle( view.RotationEnabled, GUILayout.Width( 24f ) ) ;
					if( rotationEnabled != view.RotationEnabled )
					{
						Undo.RecordObject( view, "UITween : Rotation Enabled Change" ) ;	// アンドウバッファに登録
						view.RotationEnabled = rotationEnabled ;
						EditorUtility.SetDirty( view ) ;
	//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				if( view.RotationEnabled == true && view.RotationFoldOut == true )
				{
					// ローテーション

					Vector3 rotationFrom = EditorGUILayout.Vector3Field( " From",  view.RotationFrom ) ;
					if( rotationFrom != view.RotationFrom )
					{
						Undo.RecordObject( view, "UITween : Rotation From Change" ) ;	// アンドウバッファに登録
						view.RotationFrom = rotationFrom ;
						EditorUtility.SetDirty( view ) ;
	//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}

					Vector3 rotationTo = EditorGUILayout.Vector3Field( " To",  view.RotationTo ) ;
					if( rotationTo != view.RotationTo )
					{
						Undo.RecordObject( view, "UITween : Rotation To Change" ) ;	// アンドウバッファに登録
						view.RotationTo = rotationTo ;
						EditorUtility.SetDirty( view ) ;
	//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}

					// プロセスタイプ
					UITween.ProcessTypes rotationProcessType = ( UITween.ProcessTypes )EditorGUILayout.EnumPopup( " Process Type",  view.RotationProcessType ) ;
					if( rotationProcessType != view.RotationProcessType )
					{
						Undo.RecordObject( view, "UITween : Rotation Process Type Change" ) ;	// アンドウバッファに登録
						view.RotationProcessType = rotationProcessType ;
						EditorUtility.SetDirty( view ) ;
	//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}

					if( view.RotationProcessType == UITween.ProcessTypes.Ease )
					{
						// イーズタイプ
						UITween.EaseTypes rotationEaseType = ( UITween.EaseTypes )EditorGUILayout.EnumPopup( " EaseType",  view.RotationEaseType ) ;
						if( rotationEaseType != view.RotationEaseType )
						{
							Undo.RecordObject( view, "UITween : Rotation Ease Type Change" ) ;	// アンドウバッファに登録
							view.RotationEaseType = rotationEaseType ;
							EditorUtility.SetDirty( view ) ;
	//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
						}
					}
					else
					if( view.RotationProcessType == UITween.ProcessTypes.AnimationCurve )
					{
						AnimationCurve animationCurve = new AnimationCurve(  view.RotationAnimationCurve.keys ) ;
						view.RotationAnimationCurve = EditorGUILayout.CurveField( " Animation Curve", animationCurve /*, GUILayout.Width( 170f ), GUILayout.Height( 52f )*/ ) ;
					}

					if( view.IsChecker == true )
					{
						DrawCurve( view, view.CheckFactor, view.RotationProcessType, view.RotationEaseType, view.RotationAnimationCurve ) ;
					}

					if( view.GetComponent<RectTransform>() == null )
					{
						EditorGUILayout.HelpBox( GetMessage( "RectTransformNone" ), MessageType.Warning, true ) ;		
					}
				}

				//--------------

				GUILayout.BeginHorizontal() ;	// 横並び
				{
					if( view.ScaleEnabled == false )
					{
						GUILayout.Label( "Scale Enabled" /*, GUILayout.Width( 116f ) */ ) ;
					}
					else
					{
						view.ScaleFoldOut = EditorGUILayout.Foldout( view.ScaleFoldOut, "Scale Enabled" ) ;
					}

					bool scaleEnabled = EditorGUILayout.Toggle( view.ScaleEnabled, GUILayout.Width( 24f ) ) ;
					if( scaleEnabled != view.ScaleEnabled )
					{
						Undo.RecordObject( view, "UITween : Scale Enabled Change" ) ;	// アンドウバッファに登録
						view.ScaleEnabled = scaleEnabled ;
						EditorUtility.SetDirty( view ) ;
	//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				if( view.ScaleEnabled == true && view.ScaleFoldOut == true )
				{
					// スケール

					Vector3 scaleFrom = EditorGUILayout.Vector3Field( " From",  view.ScaleFrom ) ;
					if( scaleFrom != view.ScaleFrom )
					{
						Undo.RecordObject( view, "UITween : Scale From Change" ) ;	// アンドウバッファに登録
						view.ScaleFrom = scaleFrom ;
						EditorUtility.SetDirty( view ) ;
	//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}

					Vector3 scaleTo = EditorGUILayout.Vector3Field( " To",  view.ScaleTo ) ;
					if( scaleTo != view.ScaleTo )
					{
						Undo.RecordObject( view, "UITween : Scale To Change" ) ;	// アンドウバッファに登録
						view.ScaleTo = scaleTo ;
						EditorUtility.SetDirty( view ) ;
	//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}

					// プロセスタイプ
					UITween.ProcessTypes scaleProcessType = ( UITween.ProcessTypes )EditorGUILayout.EnumPopup( " Process Type",  view.ScaleProcessType ) ;
					if( scaleProcessType != view.ScaleProcessType )
					{
						Undo.RecordObject( view, "UITween : Scale Process Type Change" ) ;	// アンドウバッファに登録
						view.ScaleProcessType = scaleProcessType ;
						EditorUtility.SetDirty( view ) ;
	//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}

					if( view.ScaleProcessType == UITween.ProcessTypes.Ease )
					{
						// イーズタイプ
						UITween.EaseTypes scaleEaseType = ( UITween.EaseTypes )EditorGUILayout.EnumPopup( " EaseType",  view.ScaleEaseType ) ;
						if( scaleEaseType != view.ScaleEaseType )
						{
							Undo.RecordObject( view, "UITween : Scale Ease Type Change" ) ;	// アンドウバッファに登録
							view.ScaleEaseType = scaleEaseType ;
							EditorUtility.SetDirty( view ) ;
	//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
						}
					}
					else
					if( view.ScaleProcessType == UITween.ProcessTypes.AnimationCurve )
					{
						AnimationCurve animationCurve = new AnimationCurve(  view.ScaleAnimationCurve.keys ) ;
						view.ScaleAnimationCurve = EditorGUILayout.CurveField( " Animation Curve", animationCurve /*, GUILayout.Width( 170f ), GUILayout.Height( 52f )*/ ) ;
					}

					if( view.IsChecker == true )
					{
						DrawCurve( view, view.CheckFactor, view.ScaleProcessType, view.ScaleEaseType, view.ScaleAnimationCurve ) ;
					}

					if( view.GetComponent<RectTransform>() == null )
					{
						EditorGUILayout.HelpBox( GetMessage( "RectTransformNone" ), MessageType.Warning, true ) ;		
					}
				}

				//--------------

				GUILayout.BeginHorizontal() ;	// 横並び
				{
					if( view.AlphaEnabled == false )
					{
						GUILayout.Label( "Alpha Enabled" /*, GUILayout.Width( 116f ) */ ) ;
					}
					else
					{
						view.AlphaFoldOut = EditorGUILayout.Foldout( view.AlphaFoldOut, "Alpha Enabled" ) ;
					}

					bool alphaEnabled = EditorGUILayout.Toggle( view.AlphaEnabled, GUILayout.Width( 24f ) ) ;
					if( alphaEnabled != view.AlphaEnabled )
					{
						Undo.RecordObject( view, "UITween : Alpha Enabled Change" ) ;	// アンドウバッファに登録
						view.AlphaEnabled = alphaEnabled ;
						EditorUtility.SetDirty( view ) ;
	//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				if( view.AlphaEnabled == true && view.AlphaFoldOut == true )
				{
					// アルファ

					GUILayout.BeginHorizontal() ;	// 横並び
					{
						GUILayout.Label( " From", GUILayout.Width( 40f ) ) ;

						float alphaFrom = EditorGUILayout.Slider( view.AlphaFrom, 0, 1 ) ;
						if( alphaFrom != view.AlphaFrom )
						{
							Undo.RecordObject( view, "UITween : Alpha From Change" ) ;	// アンドウバッファに登録
							view.AlphaFrom = alphaFrom ;
							EditorUtility.SetDirty( view ) ;
	//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
						}
					}
					GUILayout.EndHorizontal() ;		// 横並び終了

					GUILayout.BeginHorizontal() ;	// 横並び
					{
						GUILayout.Label( " To", GUILayout.Width( 40f ) ) ;

						float alphaTo = EditorGUILayout.Slider( view.AlphaTo, 0, 1 ) ;
						if( alphaTo != view.AlphaTo )
						{
							Undo.RecordObject( view, "UITween : Alpha To Change" ) ;	// アンドウバッファに登録
							view.AlphaTo = alphaTo ;
							EditorUtility.SetDirty( view ) ;
	//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
						}
					}
					GUILayout.EndHorizontal() ;		// 横並び終了

					// プロセスタイプ
					UITween.ProcessTypes alphaProcessType = ( UITween.ProcessTypes )EditorGUILayout.EnumPopup( " Process Type",  view.AlphaProcessType ) ;
					if( alphaProcessType != view.AlphaProcessType )
					{
						Undo.RecordObject( view, "UITween : Alpha Process Type Change" ) ;	// アンドウバッファに登録
						view.AlphaProcessType = alphaProcessType ;
						EditorUtility.SetDirty( view ) ;
	//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}

					if( view.AlphaProcessType == UITween.ProcessTypes.Ease )
					{
						// イーズタイプ
						UITween.EaseTypes alphaEaseType = ( UITween.EaseTypes )EditorGUILayout.EnumPopup( " EaseType",  view.AlphaEaseType ) ;
						if( alphaEaseType != view.AlphaEaseType )
						{
							Undo.RecordObject( view, "UITween : Alpha Ease Type Change" ) ;	// アンドウバッファに登録
							view.AlphaEaseType = alphaEaseType ;
							EditorUtility.SetDirty( view ) ;
	//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
						}
					}
					else
					if( view.AlphaProcessType == UITween.ProcessTypes.AnimationCurve )
					{
						AnimationCurve animationCurve = new AnimationCurve( view.AlphaAnimationCurve.keys ) ;
						view.AlphaAnimationCurve = EditorGUILayout.CurveField( " Animation Curve", animationCurve /*, GUILayout.Width( 170f ), GUILayout.Height( 52f )*/ ) ;
					}

					if( view.IsChecker == true )
					{
						DrawCurve( view, view.CheckFactor, view.AlphaProcessType, view.AlphaEaseType, view.AlphaAnimationCurve ) ;
					}

					if( view.AlphaEnabled == true && view.GetComponent<CanvasGroup>() == null )
					{
						EditorGUILayout.HelpBox( GetMessage( "CanvasGroupNone" ), MessageType.Warning, true ) ;		
					}
				}

				EditorGUILayout.Separator() ;	// 少し区切りスペース
			}

			//----------------------------------------------------------

			// AnimationClip
			AnimationClip animationClip = EditorGUILayout.ObjectField( "Animation Clip", view.AnimationClip, typeof( AnimationClip ), false ) as AnimationClip ;
			if( animationClip != view.AnimationClip )
			{
				Undo.RecordObject( view, "UITween : AnimationClip Change" ) ;	// アンドウバッファに登録
				view.AnimationClip = animationClip ;
				EditorUtility.SetDirty( view ) ;
			}

			//----------------------------------------------------------

			// ワイドモードを元に戻す
			EditorGUIUtility.wideMode = wideMode ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//--------------------------------------------------------------------

			if( view.AnimationClip == null )
			{
				if( view.enabled == true )
				{
					GUILayout.BeginHorizontal() ;	// 横並び
					{
						// チェック
						GUILayout.Label( "Checker (Editor Only)", GUILayout.Width( 150f ) ) ;

						bool isChecker = EditorGUILayout.Toggle( view.IsChecker ) ;
						if( isChecker != view.IsChecker )
						{
							if( isChecker == true )
							{
								UITween[] tweens = view.gameObject.GetComponents<UITween>() ;
								if( tweens != null && tweens.Length >  0 )
								{
									for( int i  = 0 ; i <  tweens.Length ; i ++ )
									{
										if( tweens[ i ] != view )
										{
											if( tweens[ i ].IsChecker == true )
											{
												tweens[ i ].IsChecker  = false ;
											}
										}
									}
								}
							}

							Undo.RecordObject( view, "UITween : Checker Change" ) ;	// アンドウバッファに登録
							view.IsChecker = isChecker ;
							EditorUtility.SetDirty( view ) ;
	//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
						}
					}
					GUILayout.EndHorizontal() ;		// 横並び終了

					if( view.IsChecker == true )
					{
						GUILayout.BeginHorizontal() ;	// 横並び
						{
							float checkFactor = EditorGUILayout.Slider( view.CheckFactor, 0, 1 ) ;
							if( checkFactor != view.CheckFactor )
							{
								view.CheckFactor = checkFactor ;
							}
						}
						GUILayout.EndHorizontal() ;		// 横並び終了
					}
				}

				//--------------------------------------------------------------------

				EditorGUILayout.Separator() ;	// 少し区切りスペース

				// バリュータイプ
				UITween.ValueTypes valueType = ( UITween.ValueTypes )EditorGUILayout.EnumPopup( "ValueType",  view.ValueType ) ;
				if( valueType != view.ValueType )
				{
					Undo.RecordObject( view, "UITween : Value Type Change" ) ;	// アンドウバッファに登録
					view.ValueType = valueType ;
					EditorUtility.SetDirty( view ) ;
	//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}

				GUILayout.BeginHorizontal() ;	// 横並び
				{
					// ループ
					GUILayout.Label( "Loop", GUILayout.Width( 116f ) ) ;

					bool loop = EditorGUILayout.Toggle( view.Loop ) ;
					if( loop != view.Loop )
					{
						Undo.RecordObject( view, "UITween : Loop Change" ) ;	// アンドウバッファに登録
						view.Loop = loop ;
						EditorUtility.SetDirty( view ) ;
	//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				if( view.Loop == true )
				{
					GUILayout.BeginHorizontal() ;	// 横並び
					{
						// リバース
						GUILayout.Label( "Reverse", GUILayout.Width( 116f ) ) ;

						bool reverse = EditorGUILayout.Toggle( view.Reverse ) ;
						if( reverse != view.Reverse )
						{
							Undo.RecordObject( view, "UITween : Reverse Change" ) ;	// アンドウバッファに登録
							view.Reverse = reverse ;
							EditorUtility.SetDirty( view ) ;
	//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
						}
					}
					GUILayout.EndHorizontal() ;		// 横並び終了
				}

				GUILayout.BeginHorizontal() ;	// 横並び
				{
					// イグノアタイムスケール
					GUILayout.Label( "Ignore Time Scale", GUILayout.Width( 116f ) ) ;

					bool ignoreTimeScale = EditorGUILayout.Toggle( view.IgnoreTimeScale ) ;
					if( ignoreTimeScale != view.IgnoreTimeScale )
					{
						Undo.RecordObject( view, "UITween : Ignore Time Scale Change" ) ;	// アンドウバッファに登録
						view.IgnoreTimeScale = ignoreTimeScale ;
						EditorUtility.SetDirty( view ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// プレイオンアウェイク
				GUILayout.Label( "Play On Awake", GUILayout.Width( 116f ) ) ;

				bool playOnAwake = EditorGUILayout.Toggle( view.PlayOnAwake ) ;
				if( playOnAwake != view.PlayOnAwake )
				{
					Undo.RecordObject( view, "UITween : Play On Awake Change" ) ;	// アンドウバッファに登録
					view.PlayOnAwake = playOnAwake ;
					EditorUtility.SetDirty( view ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// デストロイアットエンド
				GUILayout.Label( "Destroy At End", GUILayout.Width( 116f ) ) ;

				bool destroyAtEnd = EditorGUILayout.Toggle( view.DestroyAtEnd ) ;
				if( destroyAtEnd != view.DestroyAtEnd )
				{
					Undo.RecordObject( view, "UITween : Destroy At End Change" ) ;	// アンドウバッファに登録
					view.DestroyAtEnd = destroyAtEnd ;
					EditorUtility.SetDirty( view ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// インタラクションディスエブルインプレイング
				GUILayout.Label( "Interaction Disable In Playing", GUILayout.Width( 180f ) ) ;

				bool interactionDisableInPlaying = EditorGUILayout.Toggle( view.InteractionDisableInPlaying ) ;
				if( interactionDisableInPlaying != view.InteractionDisableInPlaying )
				{
					Undo.RecordObject( view, "UITween : Interaction Disable In Playing Change" ) ;	// アンドウバッファに登録
					view.InteractionDisableInPlaying = interactionDisableInPlaying ;
					EditorUtility.SetDirty( view ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			//----------------------------------------------------------

			if( view.InteractionDisableInPlaying == true && view.GetComponent<CanvasGroup>() == null )
			{
				EditorGUILayout.Separator() ;	// 少し区切りスペース
				EditorGUILayout.HelpBox( GetMessage( "CanvasGroupNone" ), MessageType.Warning, true ) ;		
			}

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// イズプレイング
				GUILayout.Label( "Is Playing", GUILayout.Width( 116f ) ) ;

				EditorGUILayout.Toggle( view.IsPlaying ) ;
			}
			GUILayout.EndHorizontal() ;     // 横並び終了

			//----------------------------------
#if false
			EditorGUILayout.Separator() ;   // 少し区切りスペース

			// デリゲートの設定状況
			SerializedObject so = new SerializedObject( view ) ;

			SerializedProperty sp = so.FindProperty( "OnFinishedDelegate" ) ;
			if( sp != null )
			{
				EditorGUILayout.PropertyField( sp ) ;
			}
			so.ApplyModifiedProperties() ;
#endif
		}

		// 曲線を描画する
		private void DrawCurve( UITween view, float checkFactor, UITween.ProcessTypes processType, UITween.EaseTypes easeType, AnimationCurve animationCurve )
		{
			Rect rect = GUILayoutUtility.GetRect( Screen.width - 160, 102f ) ;
		
			float x ;
			x = ( rect.width - 102f ) * 0.5f ;
			if( x <  0 )
			{
				x  = 0 ;
			}
			rect.x = x ;
			rect.width = 102f ;
		
			EditorGUI.DrawRect( new Rect( rect.x + 0, rect.y + 0, rect.width - 0, rect.height - 0 ), new Color( 1.0f, 1.0f, 1.0f, 1.0f ) ) ;
			EditorGUI.DrawRect( new Rect( rect.x + 1, rect.y + 1, rect.width - 2, rect.height - 2 ), new Color( 0.2f, 0.2f, 0.2f, 1.0f ) ) ;

			DrawLine(   0,  25, 99,  25, 0xFF7F7F7F, rect.x + 1.0f, rect.y + 1.0f ) ;
			DrawLine(   0,  74, 99,  74, 0xFF7F7F7F, rect.x + 1.0f, rect.y + 1.0f ) ;
			DrawLine(  50,  99, 50,   0, 0xFF4F4F4F, rect.x + 1.0f, rect.y + 1.0f ) ;
			DrawLine(   0,  49, 99,  49, 0xFF4F4F4F, rect.x + 1.0f, rect.y + 1.0f ) ;

			int px, py ;
			int ox = 0, oy = 0 ;
			for( px  =   0 ; px < 100 ; px ++  )
			{
				py = ( int )UITween.GetValue(   0,  50, ( float )px * 0.01f, processType, easeType, animationCurve ) ;

				if( px == 0 )
				{
					ox = px ;
					oy = py ;
				}
				else
				{
					DrawLine( ox, ( ( 74 - oy ) / 1 ) + 0, px, ( ( 74 - py ) / 1 ) + 0, 0xFF00FF00, rect.x + 1.0f, rect.y + 1.0f ) ;

					ox = px ;
					oy = py ;
				}
			}

			px = ( int )( ( 100.0f * checkFactor ) + 0.5f ) ;
			DrawLine( px, 99, px,  0, 0xFFFF0000, rect.x + 1.0f, rect.y + 1.0f ) ;
		}


	//	private AnimationCurve mCurve  = AnimationCurve.EaseInOut( 0, 0, 1, 1 ) ;

		// 直線を描画する
		private void DrawLine( int x0, int y0, int x1, int y1, uint color, float screenX, float screenY )
		{
			int dx = x1 - x0 ;
			int dy = y1 - y0 ;

			int sx = 0 ;
			if( dx <  0 )
			{
				dx  = - dx ;
				sx  = -1 ;
			}
			else
			if( dx >  0 )
			{
				sx  =  1 ;
			}

			int sy = 0 ;
			if( dy <  0 )
			{
				dy  = - dy ;
				sy  = -1 ;
			}
			else
			if( dy >  0 )
			{
				sy  =  1 ;
			}

			dx ++ ;
			dy ++ ;

			Color32 c = new Color32( ( byte )( ( color >> 16 ) & 0xFF ), ( byte )( ( color >>  8 ) & 0xFF ),  ( byte )( ( color >>   0 ) & 0xFF ), ( byte )( ( color >> 24 ) & 0xFF ) ) ;
			Rect r = new Rect( 0, 0, 1, 1 ) ;

			int lx, ly ;
			int px, py ;
			int cx, cy ;

			px = x0 ;
			py = y0 ;

			if( dx == 1 && dy == 1 )
			{
				r.x = ( float )px + screenX ;
				r.y = ( float )py + screenY ;
				EditorGUI.DrawRect( r, c ) ;
			}
			else
			if( dx >  1 && dy == 1 )
			{
				if( x1 <  x0 )
				{
					px = x1 ;
				}

				r.x = ( float )px + screenX ;
				r.y = ( float )py + screenY ;
				r.width = dx ;
				EditorGUI.DrawRect( r, c ) ;
			}
			else
			if( dx == 1 && dy >  1 )
			{
				if( y1 <  y0 )
				{
					py = y1 ;
				}

				r.x = ( float )px + screenX ;
				r.y = ( float )py + screenY ;
				r.height = dy ;
				EditorGUI.DrawRect( r, c ) ;
			}
			else
			if( dx >= dy )
			{
				cy = 0 ;
				for( lx  = 0 ; lx <  dx ; lx ++ )
				{
					r.x = ( float )px + screenX ;
					r.y = ( float )py + screenY ;
					EditorGUI.DrawRect( r, c ) ;

					cy += dy ;
					if( cy >= dx )
					{
						cy -= dx ;
						py += sy ;
					}

					px += sx ;
				}
			}
			else
			{
				cx = 0 ;
				for( ly  = 0 ; ly <  dy ; ly ++ )
				{
					r.x = ( float )px + screenX ;
					r.y = ( float )py + screenY ;
					EditorGUI.DrawRect( r, c ) ;

					cx += dx ;
					if( cx >= dy )
					{
						cx -= dy ;
						px += sx ;
					}

					py += sy ;
				}
			}
		}

		//--------------------------------------------------------------------------

		private readonly Dictionary<string,string> m_Japanese_Message = new Dictionary<string, string>()
		{
			{ "RectTransformNone", "RectRansform クラスが必要です" },
			{ "CanvasGroupNone",   "CanvasGroup クラスが必要です" },
		} ;
		private readonly Dictionary<string,string> m_English_Message = new Dictionary<string, string>()
		{
			{ "RectTransformNone", "'RectTransorm' is necessary." },
			{ "CanvasGroupNone",   "'CanvasGroup' is necessary." },
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
