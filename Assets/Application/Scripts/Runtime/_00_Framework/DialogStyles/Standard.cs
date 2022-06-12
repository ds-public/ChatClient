using System ;
using System.Collections ;
using System.Collections.Generic ;

using Cysharp.Threading.Tasks ;

using UnityEngine ;

using uGUIHelper ;

namespace Template.DialogStyle
{
	/// <summary>
	/// スタンダードタイプのダイアログ
	/// </summary>
	public class Standard : DialogStyleBase
	{
		// タイトル
		[SerializeField]
		protected UITextMesh	m_Title ;

		/// <summary>
		/// タイトル
		/// </summary>
		public UITextMesh Title
		{
			get
			{
				return m_Title ;
			}
		}

		/// <summary>
		/// タイトル下のスペース
		/// </summary>
		public float SpaceUnderTitle = 8 ;

		//---------------

		// メッセージ
		[SerializeField]
		protected UITextMesh	m_Message ;

		/// <summary>
		/// メッセージ
		/// </summary>
		public UITextMesh	Message
		{
			get
			{
				return m_Message ;
			}
		}

		[SerializeField]
		protected float		m_MaximumMessageWidth = 840 ;

		/// <summary>
		/// メッセージ下のスペース
		/// </summary>
		public float SpaceUnderMessage = 20 ;

		//---------------

		// ボタンのテンプレート
		[SerializeField]
		protected UIButton	m_TemplateButton ;

		// 複製ボタン
		private UIButton[]	m_CloneButtons = null ;

		// ボタンの数とラベル
		private string[]	m_SelectionButtonLabels = null ;

		/// <summary>
		/// ボタンラベル
		/// </summary>
		public string[] SelectionButtonLabels
		{
			get
			{
				return m_SelectionButtonLabels ;
			}
			set
			{
				m_SelectionButtonLabels = value ;
			}
		}

		/// <summary>
		/// ボタン間のスペース
		/// </summary>
		public float SpaceBetweenButton = 12 ;

		//-----------------------------------------------------------
		
		/// <summary>
		/// コールバック用のメソッド
		/// </summary>
		public Action<Standard,int> Callback = null ;

		/// <summary>
		/// ボタンを押したら自動的にウィンドウを閉じる
		/// </summary>
		public bool AutoClose = false ;

		/// <summary>
		/// ボタンを押したら自動的にウィンドウを閉じる際に呼ぶコールバック用のメソッド
		/// </summary>
		public Action<int> AutoCloseCallback = null ;

		/// <summary>
		/// 効果音を鳴らさない
		/// </summary>
		public bool	IsSilent = false ;
		
		/// <summary>
		/// 外側にも反応するかどうか(ただしボタンが１つの場合のみ)
		/// </summary>
		public bool OutsideEnabled = true ;
		
		// 自動的にダイアログを閉じるまでの時間
		public float DisplayTime = 0 ;

		//-----------------------------------------------------------

		/// <summary>
		/// ボタンのスタイル
		/// </summary>
		[Serializable]
		public class ButtonStyle
		{
			public string	Frame ;
			public Material	Lebel;
		}


		[SerializeField]
		protected ButtonStyle[] m_ButtonStyles = new ButtonStyle[ 3 ] ;

		//-------------------------------------------------------------------------------------------

		private float	m_DisplayTime = 0 ;

		private bool	m_Closing = false ;

		private bool	m_IsPressing = false ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 各種準備を行う
		/// </summary>
//		override public void Prepare( int priority )
//		{
//			base.Prepare( priority ) ;
//		}

		/// <summary>
		/// ＵＩの状態を完成させる
		/// </summary>
		override public void Commit()
		{
			if( m_Dirty == false )
			{
				return ;	// 既に実行済み
			}

			float w = m_Window.Width ;
			float h =   0 ;	// 縦幅

			if( m_Title != null && m_Title.Length >  0 )
			{
				m_Title.Py = -64  ;
			}
			else
			{
				m_Title.SetActive( false ) ;
			}

			h = 160 ;

			if( m_Message != null && m_Message.Length >  0 )
			{
				m_Message.Py = -160 ;

				float th = m_Message.TextHeight ;
				if( th <  386 )
				{
					m_Message.Py -= ( ( 386 - th ) / 2 ) ;
				}

				h += m_Message.TextHeight ;
			}
			
			if( m_SelectionButtonLabels != null && m_SelectionButtonLabels.Length >  0 )
			{
				int i, l = m_SelectionButtonLabels.Length ;

				m_CloneButtons = new UIButton[ l ] ;

				m_TemplateButton.SetActive( false ) ;


				float bw = 0 ;

				// 各ボタンの位置をサイズを決定する

				for( i  = 0 ; i <  l ; i ++ )
				{
					m_CloneButtons[ i ] = m_Window.AddPrefab<UIButton>( m_TemplateButton.gameObject ) ;

                    // ボタンの数によってボタンの画像を変更する
//					m_CloneButton[ i ].SetSpriteInAtlas( i == 0 && l <= 2 ? "CancelButton" : "DecideButton", false ) ;


					string	label = m_SelectionButtonLabels[ i ] ;
					int		style = -1 ;
					if( label.IndexOf( '~' ) >= 0 )
					{
						// 文字列と見た目に分かれる
						var texts = label.Split( '~' ) ;
						if( texts != null && texts.Length >= 2 )
						{
							label		= texts[ 0 ].TrimEnd( ' ' ) ;
							var check	= texts[ 1 ].TrimStart( ' ' ) ;
							check = check.ToLower() ;
							if( check == "0" || check == "negative" )
							{
								style = 0 ;
							}
							else
							if( check == "1" || check == "positive" )
							{
								style = 1 ;
							}
							else
							if( check == "2" || check == "optional" )
							{
								style = 2 ;
							}
						}
					}

					m_CloneButtons[ i ].name = label ;
					m_CloneButtons[ i ].Identity = i.ToString() ;
					m_CloneButtons[ i ].LabelMesh.Text = label ;
					m_CloneButtons[ i ].SetActive( true ) ;
					m_CloneButtons[ i ].SetOnButtonClick( OnButtonClick ) ;

					if( style <  0 )
					{
						// 見た目指定なし
						if( i == 0 )
						{
							var buttonStyle = m_ButtonStyles[ 1 ] ;
							m_CloneButtons[ i ].SetSpriteInAtlas( buttonStyle.Frame ) ;
//							m_CloneButtons[ i ].LabelMesh.Material = buttonStyle.Lebel ;
						}
						else
						{
							var buttonStyle = m_ButtonStyles[ 0 ] ;
							m_CloneButtons[ i ].SetSpriteInAtlas( buttonStyle.Frame ) ;
//							m_CloneButtons[ i ].LabelMesh.Material = buttonStyle.Lebel ;
						}
					}
					else
					{
						// 見た目指定あり
						var buttonStyle = m_ButtonStyles[ style ] ;
						m_CloneButtons[ i ].SetSpriteInAtlas( buttonStyle.Frame ) ;
//						m_CloneButtons[ i ].LabelMesh.Material = buttonStyle.Lebel ;
					}

					//--------------------------------------------------------

					ResizeButton( m_CloneButtons[ i ] ) ;

					bw += m_CloneButtons[ i ].Width ;
					if( i <  ( l - 1 ) )
					{
						bw += ( SpaceBetweenButton * 2 / l ) ;
					}
				}

				float bx = ( bw * 0.5f ) ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					m_CloneButtons[ i ].Px = bx - ( m_CloneButtons[ i ].Width * 0.5f ) ;

					bx -= ( m_CloneButtons[ i ].Width + ( SpaceBetweenButton * 2 / l ) ) ;
				}

				bw += 160 ;
				if( bw >  w )
				{
					w  = bw ;
				}

				// 特殊キャンセル処理
				if( m_Fade != null )
				{
					if( m_SelectionButtonLabels.Length >  0 && OutsideEnabled == true )
					{
						m_Fade.IsInteraction = true ;
						m_Fade.RaycastTarget = true ;
						m_Fade.SetOnSimplePress( OnOutside ) ;	// 外側タップの反応

						// 押しっぱなしで閉じてしまうのを防ぐ
						m_IsPressing = Input.GetMouseButton( 0 ) ;
					}
					else
					{
						m_Fade.IsInteraction = false ;
						m_Fade.RaycastTarget = false ;
						m_Fade.SetOnSimplePress( null ) ;	// 外側タップの反応
					}
				}

				h += 320 ;
			}
			else
			{
				m_TemplateButton.SetActive( false ) ;

				h += 160 ;
			}

			if( h <  770 )
			{
				h  = 770 ;
			}

			m_Window.SetSize( w, h ) ;
			
			//----------------------------------------------------------

			// 表示時間
			m_DisplayTime = DisplayTime ;
			m_Closing = false ;

			//----------------------------------------------------------

			m_Dirty = false ;	// 完了
		}

		// ボタンが押された際に呼び出されるローカルのコールバックメソッド
		private void OnButtonClick( string identity, UIButton button )
		{
			int.TryParse( identity, out int index ) ;

			if( index == 0 || index <  ( m_SelectionButtonLabels.Length - 1 ) )
			{
				Close( index, SE.Decision ) ;
			}
			else
			{
				Close( index, SE.Cancel ) ;
			}
		}

		// 外側に触れた時の対応
		private void OnOutside( bool state )
		{
			if( OutsideEnabled == true )
			{
				if( state == true )
				{
					if( m_IsPressing == false )
					{
						if( m_CloneButtons != null && m_CloneButtons.Length >  0 )
						{
							Close( ( m_CloneButtons.Length - 1 ), SE.Cancel ) ;
						}
						m_IsPressing = true ;
					}
				}
				else
				{
					m_IsPressing = false ;
				}
			}
		}

		// ウィンドウを閉じる
		private void Close( int index, string se )
		{
			if( m_Closing == true )
			{
				return ;
			}

			if( string.IsNullOrEmpty( se ) == false )
			{
				if( IsSilent == false ){ _ = SE.PlayAsync( se ) ; }
			}

			if( AutoClose == false )
			{
				Callback?.Invoke( this, index ) ;
			}
			else
			{
				m_Closing = true ;
				Close( index ) ;
			}
		}

		internal void Update()
		{
			if( m_DisplayTime >  0 && m_Closing == false )
			{
				m_DisplayTime -= Time.deltaTime ;
				if( m_DisplayTime <= 0 )
				{
					// ダイアログを閉じる
					m_Closing = true ;
					Close( -1 ) ;
				}
			}
		}

		// ダイアログをフォードアウト効果付きで非表示にして最後にクローズ時のデリゲードを呼び出す
		override protected async UniTask FadeOut()
		{
			// 外側の反応を無効化する
			m_Fade.IsInteraction = false ;
			m_Fade.RaycastTarget = false ;
			m_Fade.SetOnSimplePress( null ) ;	// 外側タップの反応

			//----------------------------------

			await base.FadeOut() ;

			// 最後にコールバックを呼び出して複製された自身も破棄する
			Callback?.Invoke( this, m_Result ) ;	// 最後に閉じられた事を通知する

			if( AutoClose == true )
			{
				AutoCloseCallback?.Invoke( m_Result ) ;
			}

			m_IsClosed = true ;		// 閉じられた事をタスクに通知する
			await Yield() ;	// 結果を正常に受け渡すために必要

			Destroy( gameObject ) ;
		}

		/// <summary>
		/// 色を設定する
		/// </summary>
		/// <param name="c"></param>
		public void SetTextColor( Color c )
		{
			// 全てのテキストのデフォルトカラーを設定する
			if( m_Title != null )
			{
				m_Title.Color = c ;
			}

			if( m_Message != null )
			{
				m_Message.Color = c ;
			}

			if( m_CloneButtons != null && m_CloneButtons.Length >  0 )
			{
				int i, l = m_CloneButtons.Length ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					if( m_CloneButtons[ i ].Label != null )
					{
						m_CloneButtons[ i ].Label.Color = c ;
					}
				}
			}
		}
	}
}

