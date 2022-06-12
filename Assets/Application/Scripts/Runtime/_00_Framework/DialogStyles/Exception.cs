using System ;
using System.Collections ;
using System.Collections.Generic ;

using Cysharp.Threading.Tasks ;

using UnityEngine ;

using uGUIHelper ;

namespace Template.DialogStyle
{
	/// <summary>
	/// エクセプションタイプのダイアログ
	/// </summary>
	public class Exception : DialogStyleBase
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

		// スクロールビュー
		[SerializeField]
		protected UIScrollView	m_ScrollView ;

		// コンテント
		[SerializeField]
		protected UIView		m_Content ;

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

		/// <summary>
		/// メッセージ下のスペース
		/// </summary>
		public float SpaceUnderMessage = 20 ;

		//---------------

		// クリップボードへのコピーボタン
		[SerializeField]
		protected UIButton	m_CopyButton ;

		/// <summary>
		/// クリップボードへコピーボタンの下のスペース
		/// </summary>
		public float SpaceUnderCopyButton = 32 ;

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
		public Action<Exception,int> Callback = null ;

		/// <summary>
		/// ボタンを押したら自動的にウィンドウを閉じる
		/// </summary>
		public bool AutoClose = false ;

		/// <summary>
		/// ボタンを押したら自動的にウィンドウを閉じる際に呼ぶコールバック用のメソッド
		/// </summary>
		public Action<int> AutoCloseCallback = null ;
		
		/// <summary>
		/// 外側にも反応するかどうか(ただしボタンが１つの場合のみ)
		/// </summary>
		public bool OutsideEnabled = true ;
		
		// 自動的にダイアログを閉じるまでの時間
		public float DisplayTime = 0 ;

		//-------------------------------------------------------------------------------------------

		private float	m_DisplayTime = 0 ;

		private bool	m_Closing = false ;

		// メッセージ表示領域の最大縦幅
		public float	MaxMessageHeight = 1600 ;

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

			float h =   0 ;	// 縦幅
			float w = 320 ;	// 横幅

			h += Padding.top ;

			if( m_Title != null && m_Title.Length >  0 )
			{
				if( m_Title.TextWidth >  w )
				{
					w = m_Title.TextWidth ;
				}

				m_Title.Py = - h - ( m_Title.TextHeight * ( 1.0f - m_Title.Pivot.y ) ) ;

				h += m_Title.TextHeight ;

				if( ( m_Message != null && m_Message.Length >  0 || ( m_SelectionButtonLabels != null && m_SelectionButtonLabels.Length >  0 ) ) )
				{
					h += SpaceUnderTitle ;
				}
			}
			else
			{
				m_Title.SetActive( false ) ;
			}

			if( m_Message != null && m_Message.Length >  0 )
			{
				if( m_Message.TextWidth >  w )
				{
					w = m_Message.TextWidth ;
				}

				m_ScrollView.Py = - h ;
				m_Content.Py = 0 ;
				m_Message.Py = 0 ;

				float th = m_Message.TextHeight ;

				m_Content.Height = th ;
				if( th >  MaxMessageHeight )
				{
					th  = MaxMessageHeight ;
				}

				m_ScrollView.Height = th ;

				h += th ;

				if( m_SelectionButtonLabels != null && m_SelectionButtonLabels.Length >  0 )
				{
					h += SpaceUnderMessage ;
				}
			}

			//----------------------------------

			m_CopyButton.SetOnSimpleClick( () =>
			{
				GUIUtility.systemCopyBuffer = m_Message.Text ;

				_ = Dialog.Open( "コピー実行", "エラー内容を\nクリップボードにコピーしました", new string[]{ "閉じる" } ) ;
			} ) ;

			m_CopyButton.Py = - h - ( m_CopyButton.Height * ( 1.0f - m_CopyButton.Pivot.y ) ) ;
			h += m_CopyButton.Height ;

			h += SpaceUnderCopyButton ;

			//----------------------------------

			if( m_SelectionButtonLabels != null && m_SelectionButtonLabels.Length >  0 )
			{
				int i, l = m_SelectionButtonLabels.Length ;

				m_CloneButtons = new UIButton[ l ] ;

				m_TemplateButton.SetActive( false ) ;


				float bw = 0 ;
				float bh = 0 ;

				// 各ボタンの位置をサイズを決定する

				for( i  = 0 ; i <  l ; i ++ )
				{
					m_CloneButtons[ i ] = m_Window.AddPrefab<UIButton>( m_TemplateButton.gameObject ) ;

                    // ボタンの数によってボタンの画像を変更する
//					m_CloneButton[ i ].SetSpriteInAtlas( i == 0 && l <= 2 ? "CancelButton" : "DecideButton", false ) ;
					
					m_CloneButtons[ i ].name = m_SelectionButtonLabels[ i ] ;
					m_CloneButtons[ i ].Identity = i.ToString() ;
					m_CloneButtons[ i ].LabelMesh.Text = m_SelectionButtonLabels[ i ] ;
					m_CloneButtons[ i ].SetActive( true ) ;
					m_CloneButtons[ i ].SetOnButtonClick( OnButtonClick ) ;

					//--------------------------------------------------------

					ResizeButton( m_CloneButtons[ i ] ) ;

					bw += m_CloneButtons[ i ].Width ;
					if( i <  ( l - 1 ) )
					{
						bw += SpaceBetweenButton ;
					}

					if( m_CloneButtons[ i ].Height >  bh )
					{
						bh  = m_CloneButtons[ i ].Height ;
					}
				}

				float bx = - ( bw * 0.5f ) ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					m_CloneButtons[ i ].Px = bx + ( m_CloneButtons[ i ].Width * 0.5f ) ;
					m_CloneButtons[ i ].Py = - h - ( m_CloneButtons[ i ].Height * ( 1.0f - m_CloneButtons[ i ].Pivot.y ) ) ;

					bx = bx + m_CloneButtons[ i ].Width + SpaceBetweenButton ;
				}

				if( bw >  w )
				{
					w  = bw ;
				}

				h += bh ;

				// 特殊キャンセル処理
				if( m_SelectionButtonLabels.Length == 1 && OutsideEnabled == true )
				{
					if( m_Fade != null )
					{
						m_Fade.IsEventTrigger = true ;
						m_Fade.IsInteraction = true ;
						m_Fade.SetOnClick( OnOutside ) ;	// 外側タップの反応
					}
				}
			}
			else
			{
				m_TemplateButton.SetActive( false ) ;
			}

			w += Padding.horizontal ;
			h += Padding.bottom ;

			if( w >  976 )
			{
				w  = 976 ;
			}

			if( w <  m_Window.Width )
			{
				w  = m_Window.Width ;
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
			if( m_Closing == true )
			{
				return ;
			}

			int.TryParse( identity, out int index ) ;

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

		// 外側に触れた時の対応
		private void OnOutside( string identity, UIView view )
		{
			if( m_CloneButtons != null && m_CloneButtons.Length == 1 )
			{
				OnButtonClick( "-1", null ) ;
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

