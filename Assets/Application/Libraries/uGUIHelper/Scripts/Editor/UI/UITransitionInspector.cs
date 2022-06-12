#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;
using System.Linq ;

namespace uGUIHelper
{
	/// <summary>
	/// UITransition のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UITransition ) ) ]
	public class UITransitionInspector : Editor
	{
		// スンスペクター描画
		public override void OnInspectorGUI()
		{
			// とりあえずデフォルト
	//		DrawDefaultInspector() ;
		
			//--------------------------------------------

			// ターゲットのインスタンス
			UITransition transition = target as UITransition ;
		
			EditorGUILayout.Separator() ;   // 少し区切りスペース

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				if( transition.TransitionEnabled == false )
				{
					GUILayout.Label( "Transition Enabled" ) ;
				}
				else
				{
					transition.TransitionFoldOut = EditorGUILayout.Foldout( transition.TransitionFoldOut, "Transition Enabled" ) ;
				}

				bool transitionEnabled = EditorGUILayout.Toggle( transition.TransitionEnabled, GUILayout.Width( 24f ) ) ;
				if( transitionEnabled != transition.TransitionEnabled )
				{
					Undo.RecordObject( transition, "UITransition : Transition Enabled Change" ) ;	// アンドウバッファに登録
					transition.TransitionEnabled = transitionEnabled ;
					EditorUtility.SetDirty( transition ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			if( transition.TransitionEnabled == true && transition.TransitionFoldOut == true )
			{
				// ワイドモードを有効にする
				bool wideMode = EditorGUIUtility.wideMode ;
				EditorGUIUtility.wideMode = true ;

				//--------------------------------------------

//				GUILayout.BeginHorizontal() ;	// 横並び
//				{
//					bool spriteOverwriteEnabled = EditorGUILayout.Toggle( transition.SpriteOverwriteEnabled, GUILayout.Width( 24f ) ) ;
//					if( spriteOverwriteEnabled != transition.SpriteOverwriteEnabled )
//					{
//						Undo.RecordObject( transition, "UITransition : Sprite Overwrite Enabled Change" ) ;	// アンドウバッファに登録
//						transition.SpriteOverwriteEnabled = spriteOverwriteEnabled ;
//						EditorUtility.SetDirty( transition ) ;
//					}
//					GUILayout.Label( "Sprite Overwrite Enabled" ) ;
//				}
//				GUILayout.EndHorizontal() ;		// 横並び終了

				//---------------------------------

				if( transition.UseAnimator == false )
				{
					// アニメーターを使用しなければ有効
					if( transition.Transitions != null && transition.Transitions.Count >  0 )
					{
						// 選択中のトランジションタイプ
						UITransition.StateTypes editingState = ( UITransition.StateTypes )EditorGUILayout.EnumPopup( " State",  transition.EditingState ) ;
						if( editingState != transition.EditingState )
						{
							Undo.RecordObject( transition, "UITransition : Editing State Change" ) ;	// アンドウバッファに登録
							transition.EditingState = editingState ;
							EditorUtility.SetDirty( transition ) ;
						}

						// ステートに応じた値を描画する
						int si = ( int )transition.EditingState ;

#if false
						if( transition.SpriteOverwriteEnabled == true )
						{
							UIImage image = transition.GetComponent<UIImage>() ;
							if( image != null )
							{
								if( image.SpriteSet != null )
								{
									string[] spriteNames = image.SpriteSet.GetSpriteNames() ;
									if( spriteNames != null && spriteNames.Length >  0 )
									{
										if( transition.Transitions[ si ].Sprite == null && image.Sprite != null )
										{
											Undo.RecordObject( transition, "UITransition : Editing State Change" ) ;	// アンドウバッファに登録
											transition.Transitions[ si ].Sprite = image.Sprite ;
											EditorUtility.SetDirty( transition ) ;
										}

										string currentSpriteName = transition.Transitions[ si ].Sprite.name ;

										int indexBase = -1 ;

										int i, l = spriteNames.Length ;
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
											var _ = spriteNames.ToList() ; _.Insert( 0, "Unknown" ) ; spriteNames = _.ToArray() ;
											indexBase = 0 ;
										}

										// フレーム番号
										int index = EditorGUILayout.Popup( "  Selected Sprite", indexBase, spriteNames ) ;
										if( index != indexBase )
										{
											Undo.RecordObject( transition, "UIImage Sprite : Change" ) ;	// アンドウバッファに登録
											transition.Transitions[ si ].Sprite = image.GetSpriteInAtlas( spriteNames[ index ] ) ;
											EditorUtility.SetDirty( transition ) ;
										}

										// 確認用
										Sprite sprite = EditorGUILayout.ObjectField( "", image.GetSpriteInAtlas( spriteNames[ index ] ), typeof( Sprite ), false, GUILayout.Width( 60f ), GUILayout.Height( 60f ) ) as Sprite ;
										if( sprite != image.GetSpriteInAtlas( spriteNames[ index ] ) )
										{
										}
									}
								}
							}
						}
#endif
						UITransition.ProcessTypes processType = ( UITransition.ProcessTypes )EditorGUILayout.EnumPopup( "  Process Type",  transition.Transitions[ si ].ProcessType ) ;
						if( processType != transition.Transitions[ si ].ProcessType )
						{
							Undo.RecordObject( transition, "UIButton : Process Type Change" ) ;	// アンドウバッファに登録
							transition.Transitions[ si ].ProcessType = processType ;
							EditorUtility.SetDirty( transition ) ;
						}

	//					Vector3 fadePosition = EditorGUILayout.Vector3Field( "   Fade Position",  transition.transitions[ si ].fadePosition ) ;
	//					if( fadePosition != transition.transitions[ si ].fadePosition )
	//					{
	//						Undo.RecordObject( tTarget, "UIButton : Fade Position Change" ) ;	// アンドウバッファに登録
	//						transition.transitions[ si ].fadePosition = fadePosition ;
	//						EditorUtility.SetDirty( transition ) ;
	//					}

						Vector3 fadeRotation = EditorGUILayout.Vector3Field( "   Fade Rotation", transition.Transitions[ si ].FadeRotation ) ;
						if( fadeRotation != transition.Transitions[ si ].FadeRotation )
						{
							Undo.RecordObject( transition, "UIButton : Fade Rotation Change" ) ;	// アンドウバッファに登録
							transition.Transitions[ si ].FadeRotation = fadeRotation ;
							EditorUtility.SetDirty( transition ) ;
						}

						Vector3 fadeScale    = EditorGUILayout.Vector3Field( "   Fade Scale",      transition.Transitions[ si ].FadeScale   ) ;
						if( fadeScale    != transition.Transitions[ si ].FadeScale    )
						{
							Undo.RecordObject( transition, "UIButton : Fade Scale    Change" ) ;	// アンドウバッファに登録
							transition.Transitions[ si ].FadeScale    = fadeScale ;
							EditorUtility.SetDirty( transition ) ;
						}


						if( transition.Transitions[ si ].ProcessType == UITransition.ProcessTypes.Ease )
						{
							// イーズタイプ
							UITransition.EaseTypes fadeEaseType = ( UITransition.EaseTypes )EditorGUILayout.EnumPopup( "   Fade Ease Type",  transition.Transitions[ si ].FadeEaseType ) ;
							if( fadeEaseType != transition.Transitions[ si ].FadeEaseType )
							{
								Undo.RecordObject( transition, "UIButton : Fade Ease Type Change" ) ;	// アンドウバッファに登録
								transition.Transitions[ si ].FadeEaseType = fadeEaseType ;
								EditorUtility.SetDirty( transition ) ;
							}

							DrawCurve( transition, transition.Transitions[ si ].FadeEaseType ) ;

							// デュアレーション
							float fadeDuration = EditorGUILayout.FloatField( "   Fade Duration",  transition.Transitions[ si ].FadeDuration ) ;
							if( fadeDuration != transition.Transitions[ si ].FadeDuration )
							{
								Undo.RecordObject( transition, "UIButton : Fade Duration Change" ) ;	// アンドウバッファに登録
								transition.Transitions[ si ].FadeDuration = fadeDuration ;
							}
						}
						else
						if( transition.Transitions[ si ].ProcessType == UITransition.ProcessTypes.AnimationCurve )
						{
							transition.Transitions[ si ].FadeAnimationCurve = EditorGUILayout.CurveField( "   Animation Curve", transition.Transitions[ si ].FadeAnimationCurve, GUILayout.Width( 170f ), GUILayout.Height( 52f ) ) ;

							int l = transition.Transitions[ si ].FadeAnimationCurve.length ;
							Keyframe keyFrame = transition.Transitions[ si ].FadeAnimationCurve[ l - 1 ] ;	// 最終キー
							float fadeDuration = keyFrame.time ;
							EditorGUILayout.FloatField( "   Fade Duration", fadeDuration ) ;
						}
					}
					else
					{
						transition.InitializeTransitions() ;
					}
				}

				//--------------------------------------------

				EditorGUILayout.Separator() ;	// 少し区切りスペース

				// フィニッシュの後にポーズするかどうか
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					bool useAnimator = EditorGUILayout.Toggle( transition.UseAnimator, GUILayout.Width( 24f ) ) ;
					if( useAnimator != transition.UseAnimator )
					{
						Undo.RecordObject( transition, "UITransition : Use Animator Change" ) ;	// アンドウバッファに登録
						transition.UseAnimator = useAnimator ;
						EditorUtility.SetDirty( transition ) ;
					}
					GUILayout.Label( "Use Animator" ) ;
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				//--------------------------------------------

				EditorGUILayout.Separator() ;	// 少し区切りスペース
		
				// フィニッシュの後にポーズするかどうか
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					bool pauseAfterFinished = EditorGUILayout.Toggle( transition.PauseAfterFinished, GUILayout.Width( 24f ) ) ;
					if( pauseAfterFinished != transition.PauseAfterFinished )
					{
						Undo.RecordObject( transition, "UITransition : Pause After Finished Change" ) ;	// アンドウバッファに登録
						transition.PauseAfterFinished = pauseAfterFinished ;
						EditorUtility.SetDirty( transition ) ;
					}
					GUILayout.Label( "Pause After Finished" ) ;
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				// フィニッシュの後にポーズするかどうか
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					bool colorTransmission = EditorGUILayout.Toggle( transition.ColorTransmission, GUILayout.Width( 24f ) ) ;
					if( colorTransmission != transition.ColorTransmission )
					{
						Undo.RecordObject( transition, "UITransition : Color Transmission Change" ) ;	// アンドウバッファに登録
						transition.ColorTransmission = colorTransmission ;
						EditorUtility.SetDirty( transition ) ;
					}
					GUILayout.Label( "Color Transmission" ) ;
				}
				GUILayout.EndHorizontal() ;		// 横並び終了




				// ポーズ状態
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					bool isPausing = EditorGUILayout.Toggle( transition.IsPauseing, GUILayout.Width( 24f ) ) ;
					if( isPausing != transition.IsPauseing )
					{
						Undo.RecordObject( transition, "UITransition : IsPausing Change" ) ;	// アンドウバッファに登録
						transition.IsPauseing = isPausing ;
						EditorUtility.SetDirty( transition ) ;
					}
					GUILayout.Label( "Is Pausing" ) ;
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				//--------------------------------------------

				// ワイドモードを元に戻す
				EditorGUIUtility.wideMode = wideMode ;
			}
		}

		// 曲線を描画する
		private void DrawCurve( UITransition transition, UITransition.EaseTypes easeType )
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

/*			int px = 0, py = 0 ;
			int ox = 0, oy = 0 ;
			for( px  =   0 ; px <  25 ; px ++  )
			{
				py = ( int )tTarget.GetValue(   0,  25, ( float )px * 0.04f, tEaseType ) ;

				if( px == 0 )
				{
					ox = px ;
					oy = py ;
				}
				else
				{
					DrawLine( ox, ( ( 37 - oy ) / 1 ) + 0, px, ( ( 37 - py ) / 1 ) + 0, 0xFF00FF00, tRect.x + 1.0f, tRect.y + 1.0f ) ;

					ox = px ;
					oy = py ;
				}
			}*/

			int px, py ;
			int ox = 0, oy = 0 ;
			for( px  =   0 ; px < 100 ; px ++  )
			{
				py = ( int )transition.GetValue(   0,  50, ( float )px * 0.01f, easeType ) ;

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



		}

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

		private readonly Dictionary<string,string> m_Japanese_Message = new Dictionary<string, string>()
		{
			{ "EventTriggerNone", "EventTrigger クラスが必要です" },
		} ;
		private readonly Dictionary<string,string> m_English_Message = new Dictionary<string, string>()
		{
			{ "EventTriggerNone", "'EventTrigger' is necessary." },
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
