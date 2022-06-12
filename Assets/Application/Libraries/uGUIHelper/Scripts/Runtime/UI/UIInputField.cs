using UnityEngine ;
using UnityEngine.UI ;
using System ;
using System.Collections ;

using TMPro ;

namespace uGUIHelper
{
	/// <summary>
	/// uGUI:InputField クラスの機能拡張コンポーネントクラス(複合)
	/// </summary>
	[RequireComponent(typeof(TMP_InputFieldPlus))]	
	public class UIInputField : UIImage
	{
		/// <summary>
		/// interactable(ショーシカット)
		/// </summary>
		public bool Interactable
		{
			get
			{
				TMP_InputFieldPlus inputField = CTMP_InputField ;
				if( inputField == null )
				{
					return false ;
				}
				return inputField.interactable ;
			}
			set
			{
				TMP_InputFieldPlus inputField = CTMP_InputField ;
				if( inputField == null )
				{
					return ;
				}
				inputField.interactable = value ;
			}
		}

		/// <summary>
		/// テキスト(ショートカット)
		/// </summary>
		public string Text
		{
			get
			{
				TMP_InputFieldPlus inputField = CTMP_InputField ;
				if( inputField == null )
				{
					return "" ;
				}

				return inputField.text ;
			}
			set
			{
				TMP_InputFieldPlus inputField = CTMP_InputField ;
				if( inputField == null )
				{
					return ;
				}

				if( inputField.text != value )
				{
					string text = value ;

					// 必要に応じてフィルターをかける
					text = ProcessInputFilter( text ) ;

					inputField.text = text ;
				}
			}
		}
		
		/// <summary>
		/// キャラクターリミット(ショートカット)
		/// </summary>
		public int CharacterLimit
		{
			get
			{
				TMP_InputFieldPlus inputField = CTMP_InputField ;
				if( inputField == null )
				{
					return 0 ;
				}
				return inputField.characterLimit ;
			}
			set
			{
				TMP_InputFieldPlus inputField = CTMP_InputField ;
				if( inputField == null )
				{
					return ;
				}
				inputField.characterLimit = value ;
			}
		}
	
		/// <summary>
		/// コンテントタイプ(ショートカット)
		/// </summary>
		public TMP_InputFieldPlus.ContentType ContentType
		{
			get
			{
				TMP_InputFieldPlus inputField = CTMP_InputField ;
				if( inputField == null )
				{
					return TMP_InputFieldPlus.ContentType.Standard ;
				}
				return inputField.contentType ;
			}
			set
			{
				TMP_InputFieldPlus inputField = CTMP_InputField ;
				if( inputField == null )
				{
					return ;
				}

				if( inputField.contentType != value )
				{
					inputField.contentType = value ;

					string text = inputField.text ;
					inputField.text = "" ;
					inputField.text = text ;
				}
			}
		}

		/// <summary>
		/// ラインタイプ(ショートカット)
		/// </summary>
		public TMP_InputFieldPlus.LineType LineType
		{
			get
			{
				TMP_InputFieldPlus inputField = CTMP_InputField ;
				if( inputField == null )
				{
					return TMP_InputFieldPlus.LineType.SingleLine ;
				}
				return inputField.lineType ;
			}
			set
			{
				TMP_InputFieldPlus inputField = CTMP_InputField ;
				if( inputField == null )
				{
					return ;
				}
				inputField.lineType = value ;
			}
		}

		/// <summary>
		/// テキストコンポーネント(ショートカット)
		/// </summary>
		public UITextMesh TextComponent
		{
			get
			{
				TMP_InputFieldPlus inputField = CTMP_InputField ;
				if( inputField == null )
				{
					return null ;
				}
				return inputField.textComponent.GetComponent<UITextMesh>() ;
			}
		}

		/// <summary>
		/// プレースホルダー(ショートカット)
		/// </summary>
		public UITextMesh Placeholder
		{
			get
			{
				TMP_InputFieldPlus inputField = CTMP_InputField ;
				if( inputField == null )
				{
					return null ;
				}
				return inputField.placeholder.GetComponent<UITextMesh>() ;
			}
		}

		/// <summary>
		/// キャレットブリンクレート(ショートカット)
		/// </summary>
		public float CaretBlinkRate
		{
			get
			{
				TMP_InputFieldPlus inputField = CTMP_InputField ;
				if( inputField == null )
				{
					return 0 ;
				}
				return inputField.caretBlinkRate ;
			}
			set
			{
				TMP_InputFieldPlus inputField = CTMP_InputField ;
				if( inputField == null )
				{
					return ;
				}
				inputField.caretBlinkRate = value ;
			}
		}

		/// <summary>
		/// キャレットウィドス(ショートカット)
		/// </summary>
		public int CaretWidth
		{
			get
			{
				TMP_InputFieldPlus inputField = CTMP_InputField ;
				if( inputField == null )
				{
					return 0 ;
				}
				return inputField.caretWidth ;
			}
			set
			{
				TMP_InputFieldPlus inputField = CTMP_InputField ;
				if( inputField == null )
				{
					return ;
				}
				inputField.caretWidth = value ;
			}
		}

		/// <summary>
		/// カスタムキャレットカラー(ショートカット)
		/// </summary>
		public bool CustomCaretColor
		{
			get
			{
				TMP_InputFieldPlus inputField = CTMP_InputField ;
				if( inputField == null )
				{
					return false ;
				}
				return inputField.customCaretColor ;
			}
			set
			{
				TMP_InputFieldPlus inputField = CTMP_InputField ;
				if( inputField == null )
				{
					return ;
				}
				inputField.customCaretColor = value ;
			}
		}

		/// <summary>
		/// キャレットカラー(ショートカット)
		/// </summary>
		public Color CaretColor
		{
			get
			{
				TMP_InputFieldPlus inputField = CTMP_InputField ;
				if( inputField == null )
				{
					return Color.black ;
				}
				return inputField.caretColor ;
			}
			set
			{
				TMP_InputFieldPlus inputField = CTMP_InputField ;
				if( inputField == null )
				{
					return ;
				}
				inputField.caretColor = value ;
			}
		}

		/// <summary>
		/// セレクションカラー(ショートカット)
		/// </summary>
		public Color SelectionColor
		{
			get
			{
				TMP_InputFieldPlus inputField = CTMP_InputField ;
				if( inputField == null )
				{
					return Color.black ;
				}
				return inputField.selectionColor;
			}
			set
			{
				TMP_InputFieldPlus inputField = CTMP_InputField ;
				if( inputField == null )
				{
					return ;
				}
				inputField.selectionColor = value ;
			}
		}

		/// <summary>
		/// ハイドモバイルインプット(ショートカット)
		/// </summary>
		public bool HideMobileInput
		{
			get
			{
				TMP_InputFieldPlus inputField = CTMP_InputField ;
				if( inputField == null )
				{
					return false ;
				}
				return inputField.shouldHideMobileInput ;
			}
			set
			{
				TMP_InputFieldPlus inputField = CTMP_InputField ;
				if( inputField == null )
				{
					return ;
				}
				inputField.shouldHideMobileInput = value ;
			}
		}


		/// <summary>
		/// リードオンリー(ショートカット)
		/// </summary>
		public bool ReadOnly
		{
			get
			{
				TMP_InputFieldPlus inputField = CTMP_InputField ;
				if( inputField == null )
				{
					return false ;
				}
				return inputField.readOnly ;
			}
			set
			{
				TMP_InputFieldPlus inputField = CTMP_InputField ;
				if( inputField == null )
				{
					return ;
				}
				inputField.readOnly = value ;
			}
		}

		/// <summary>
		/// フォントサイズを設定する
		/// </summary>
		/// <param name="fontSize"></param>
		public void SetFontSize( int fontSize )
		{
			if( TextComponent != null )
			{
				TextComponent.FontSize = fontSize ;
			}

			if( Placeholder != null )
			{
				Placeholder.FontSize = fontSize ;
			}
		}

		[SerializeField]
		protected FontFilter	m_FontFilter = null ;
		public FontFilter		  FontFilter{ get{ return m_FontFilter ; } set{ m_FontFilter = value ; } }

		[SerializeField]
		protected char			m_FontAlternateCode = ( char )0 ;
		public char				  FontAlternateCode{ get{ return m_FontAlternateCode ; } set{ m_FontAlternateCode = value ; } }

		//-----------------------------------

		/// <summary>
		/// 入力制限種別
		/// </summary>
		public enum InputFilterTypes
		{
			String,		// 文字列(制限無し)
			Integer,	// 整数限定
			Decimal,	// 少数限定
		}

		[SerializeField]
		protected InputFilterTypes m_InputFilterType = InputFilterTypes.String ;

		public    InputFilterTypes   InputFilterType
		{
			get
			{
				return m_InputFilterType ;
			}
			set
			{
				if( m_InputFilterType != value )
				{
					m_InputFilterType  = value ;
				}
			}
		}

		// 数値の場合の範囲制限(最小)
		[SerializeField]
		protected double m_MinValue = - Mathf.Infinity ;

		public    double   MinValue
		{
			get{ return m_MinValue ; }
			set
			{
				if( m_MinValue != value && value <= m_MaxValue )
				{
					m_MinValue  = value ;
				}
			}
		}

		// 数値の場合の範囲制限
		[SerializeField]
		protected double m_MaxValue =   Mathf.Infinity ;

		public    double   MaxValue
		{
			get{ return m_MaxValue ; }
			set
			{
				if( m_MaxValue != value && value >= m_MinValue )
				{
					m_MaxValue  = value ;
				}
			}
		}

		//-----------------------------------

		// 数値
		protected string	m_NumericValue = "0" ;

		//-----------------------------------

		/// <summary>
		/// フォーカスを持たせる
		/// </summary>
		public bool Activate()
		{
			TMP_InputFieldPlus inputField = CTMP_InputField ;
			if( inputField == null )
			{
				return false ;
			}
			inputField.ActivateInputField() ;

			return true ;
		}
		
		//-------------------------------------------------------------------------------------------

		// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		override protected void OnBuild( string option = "" )
		{
			TMP_InputFieldPlus inputField = CTMP_InputField ;

			if( inputField == null )
			{
				inputField = gameObject.AddComponent<TMP_InputFieldPlus>() ;
			}
			if( inputField == null )
			{
				// 異常
				return ;
			}

			Image image = CImage ;
			if( image != null )
			{
				inputField.targetGraphic = image ;
			}

			//-------------------------------
			
			bool isMultiLine = false ;
			if( string.IsNullOrEmpty( option ) == false && option.ToLower() == "multiline" )
			{
				// マルチ
				isMultiLine = true ;
			}


			Vector2 size = GetCanvasSize() ;

			int fontSize = 16 ;
			if( size.x >  0 && size.y >  0 )
			{
				if( isMultiLine == false )
				{
					// シングル
					SetSize( size.y * 0.5f, size.y * 0.1f ) ;
				}
				else
				{
					// マルチ
					SetSize( size.y * 0.5f, size.y * 0.5f ) ;
				}

				fontSize = ( int )( size.y * 0.1f * 0.6f ) ;
			}
				
			// Image
			image.sprite = Resources.Load<Sprite>( "uGUIHelper/Textures/UIDefaultFrame" ) ;
			image.type = Image.Type.Sliced ;
				
			if( IsCanvasOverlay == true )
			{
				image.material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}

			ResetRectTransform() ;

			// Viewport
			UIImage viewport = AddView<UIImage>( "Viewport" ) ;
			viewport.SetAnchorToStretch() ;
			viewport.SetMargin( 16, 16, 2, 2 ) ;
//			viewport.IsRectMask2D = true ;
//			viewport.IsAlphaMaskWindow = true ;
			viewport.IsMask = true ;
			viewport.ShowMaskGraphic = false ;
			inputField.textViewport = viewport.GetRectTransform() ;

			// Text
			UITextMesh textComponent = viewport.AddView<UITextMesh>( "Text", "SIMPLE" ) ;
			textComponent.IsContentSizeFitter = false ;
			textComponent.FontSize = fontSize ;
			textComponent.SupportRichText = false ;
			textComponent.Color = new Color32(  50,  50,  50,  255 ) ;
			textComponent.SetAnchorToStretch() ;
			textComponent.SetMargin( 0, 0, 0, 0 ) ;
//			text.position = new Vector2( 0, -2 ) ;
//			text.SetSize( -24, -28 ) ;
//			text.resizeTextForBestFit = true ;
			inputField.textComponent = textComponent.CTextMesh ;
			if( isMultiLine == false )
			{
				textComponent.Alignment = TextAlignmentOptions.MidlineLeft ;
			}
			else
			{
				textComponent.Alignment = TextAlignmentOptions.TopLeft ;
			}
			textComponent.SupportRichText = true ;

			inputField.fontAsset = textComponent.Font ;
			inputField.pointSize = textComponent.FontSize ;


//			if( IsCanvasOverlay == true )
//			{
//				textComponent.Material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
//			}

			// Placeholder
			UITextMesh placeholder = viewport.AddView<UITextMesh>( "Placeholder", "SIMPLE" ) ;
			placeholder.FontSize = fontSize ;
			placeholder.FontStyle = FontStyles.Italic ;
			placeholder.Text = "Enter text..." ;
			placeholder.Color = new Color32(  50,  50,  50, 128 ) ;
			placeholder.SetAnchorToStretch() ;
			placeholder.SetMargin( 0, 0, 0, 0 ) ;
//			placeholder.position = new Vector2( 0, -2 ) ;
//			placeholder.SetSize( -24, -28 ) ;
//			placeholder.resizeTextForBestFit = true ;
			inputField.placeholder = placeholder.CTextMesh ;
			if( isMultiLine == false )
			{
				placeholder.Alignment = TextAlignmentOptions.MidlineLeft ;
			}
			else
			{
				placeholder.Alignment = TextAlignmentOptions.TopLeft ;
			}

//			if( IsCanvasOverlay == true )
//			{
//				placeholder.Material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
//			}

			if( isMultiLine == true )
			{
				// マルチラインで生成する
				inputField.lineType = TMP_InputFieldPlus.LineType.MultiLineNewline ;
				inputField.textComponent.enableWordWrapping = true ;
			}

			inputField.caretWidth = 4 ;
			inputField.customCaretColor = true ;
			inputField.caretColor = Color.blue ;

			//----------------------------------------------------------

			FontFilter	fontFilter = null ;
			char		fontAlternateCode = '？' ;

#if UNITY_EDITOR

			if( Application.isPlaying == false )
			{
				// メニューから操作した場合のみ自動設定を行う
				DefaultSettings ds = Resources.Load<DefaultSettings>( "uGUIHelper/DefaultSettings" ) ;
				if( ds != null )
				{
					fontFilter			= ds.FontFilter ;
					fontAlternateCode	= ds.FontAlternateCode ;
				}
			}
			
#endif
			if( fontFilter == null )
			{

			}
			else
			{
				m_FontFilter = fontFilter ;
			}

			if( fontAlternateCode == 0 )
			{
				m_FontAlternateCode = '？' ;
			}
			else
			{
				m_FontAlternateCode = fontAlternateCode ;
			}
		}

		// 派生クラスの Start
		override protected void OnStart()
		{
			base.OnStart() ;

			// 注意:実行のみにしておかないと ExecuteInEditMode で何度も登録されてしまう
			if( Application.isPlaying == true )
			{
				// カスタムリスナー登録（Awake 起動毎に実行する必要がある）
				if( CTMP_InputField != null )
				{
					CTMP_InputField.onValueChanged.AddListener( OnValueChangedInner ) ;
					CTMP_InputField.onEndEdit.AddListener( OnEndEditInner ) ;

					//--------------------------------

					if( string.IsNullOrEmpty( CTMP_InputField.text ) == false )
					{
						string text = CTMP_InputField.text ;
						if( m_InputFilterType == InputFilterTypes.Integer )
						{
							if( double.TryParse( text, out double numericValue ) == true )
							{
								string value = ( ( long )numericValue ).ToString() ;
								m_NumericValue   = value ;
								CTMP_InputField.text = value ;
							}
						}
						else
						if( m_InputFilterType == InputFilterTypes.Decimal )
						{
							if( double.TryParse( text, out double numericValue ) == true )
							{
								int digit = CTMP_InputField.text.Length ;
								string value = numericValue.ToString() ;
								if( value.Length >  digit )
								{
									value = value.Substring( 0, digit ) ;
								}
								m_NumericValue   = value ;
								CTMP_InputField.text = value ;
							}
						}
					}
					else
					{
						// 数値なら初期値は入れる
						if( m_InputFilterType != InputFilterTypes.String )
						{
							CTMP_InputField.text = m_NumericValue ;
						}
					}
				}
			}
		}

		// キャレットのＶＲモード対応
		private LayoutElement m_LE = null ;

		protected override void OnLateUpdate()
		{
			base.OnLateUpdate() ;

			if( Application.isPlaying == true )
			{
				// キャレットが表示されなくなるバグ対策
				if( CTMP_InputField != null && m_LE == null )
				{
					m_LE =	CTMP_InputField.GetComponentInChildren<LayoutElement>() ;
					if( m_LE != null )
					{
						CanvasRenderer cr0 = m_LE.GetComponent<CanvasRenderer>() ;
						CanvasRenderer cr1 = CTMP_InputField.GetComponent<CanvasRenderer>() ;
						if( cr0 != null && cr1 != null )
						{
							cr0.SetMaterial( cr1.GetMaterial(), 0 ) ;
						}
					}
				}
			}
		}

		//-------------------------------------------------------------------

		/// <summary>
		/// 状態が変化した際に呼び出されるアクション
		/// </summary>
		public Action<string, UIInputField, string> OnValueChangedAction ;

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="identity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="view">ビューのインスタンス</param>
		/// <param name="value">変化後の値</param>
		public delegate void OnValueChanged( string identity, UIView view, string value ) ;

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲート
		/// </summary>
		public OnValueChanged OnValueChangedDelegate ;
		
		/// <summary>
		/// 状態が変化した際に呼び出されるアクションを設定する
		/// </summary>
		/// <param name="onValueChangedAction">アクションメソッド</param>
		public void SetOnValueChanged( Action<string, UIInputField, string> onValueChangedAction )
		{
			OnValueChangedAction = onValueChangedAction ;
		}

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートを追加する
		/// </summary>
		/// <param name="onValueChangedDelegate">デリゲートメソッド</param>
		public void AddOnValueChanged( OnValueChanged onValueChangedDelegate )
		{
			OnValueChangedDelegate += onValueChangedDelegate ;
		}
		
		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="onValueChangedDelegate">デリゲートメソッド</param>
		public void RemoveOnValueChanged( OnValueChanged onValueChangedDelegate )
		{
			OnValueChangedDelegate -= onValueChangedDelegate ;
		}
		
		// 内部リスナー
		private void OnValueChangedInner( string value )
		{
//			Debug.LogWarning( "状態変化:" + tValue + " : " + Input.compositionString + " : " + Input.inputString ) ;

			//----------------------------------------------------------

			if( m_FontFilter != null && m_FontAlternateCode != 0 && string.IsNullOrEmpty( value ) == false )
			{
				// 内部の文字を検査して表示できない文字が入っていたら強制的に補正する
				int i, l = value.Length ;
				char[] s = new char[ l ] ;
				int c ;
				bool dirty = false ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					c = value[ i ] ;

					if( CTMP_InputField.lineType == TMP_InputFieldPlus.LineType.MultiLineNewline && ( c == 0x0D || c == 0x0A ) )
					{
						// 改行コードは許容する
						s[ i ] = value[ i ] ;
						dirty = true ;
					}
					else
					{
						if( ( m_FontFilter.flag[ c >> 3 ] & ( 1 << ( c & 0x0007 ) ) ) == 0 )
						{
							// この文字は置き換える必要がある
							s[ i ] = m_FontAlternateCode ;
						}
						else
						{
							s[ i ] = value[ i ] ;
							dirty = true ;
						}
					}
				}

				if( dirty == true )
				{
					CTMP_InputField.text = new string( s ) ;
				}
			}

			//----------------------------------------------------------

			if( OnValueChangedAction != null || OnValueChangedDelegate != null )
			{
				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				OnValueChangedAction?.Invoke( identity, this, value ) ;
				OnValueChangedDelegate?.Invoke( identity, this, value ) ;
			}
		}
	
		/// <summary>
		/// 状態が変化した際に呼び出されるリスナーを追加する
		/// </summary>
		/// <param name="onValueChanged">リスナーメソッド</param>
		public void AddOnValueChangeListener( UnityEngine.Events.UnityAction<string> onValueChanged )
		{
			TMP_InputFieldPlus inputField = CTMP_InputField ;
			if( inputField != null )
			{
				inputField.onValueChanged.AddListener( onValueChanged ) ;
			}
		}

		/// <summary>
		/// 状態が変化した際に呼び出されるリスナーを削除する
		/// </summary>
		/// <param name="onValueChanged">リスナーメソッド</param>
		public void RemoveOnValueChangeListener( UnityEngine.Events.UnityAction<string> onValueChanged )
		{
			TMP_InputFieldPlus inputField = CTMP_InputField ;
			if( inputField != null )
			{
				inputField.onValueChanged.RemoveListener( onValueChanged ) ;
			}
		}
		
		/// <summary>
		/// 状態が変化した際に呼び出されるリスナーを全て削除する
		/// </summary>
		public void RemoveOnValueChangeAllListeners()
		{
			TMP_InputFieldPlus inputField = CTMP_InputField ;
			if( inputField != null )
			{
				inputField.onValueChanged.RemoveAllListeners() ;
			}
		}

		//-----------------------------------------------------------
		
		/// <summary>
		/// 入力が終了した際に呼び出されるアクション
		/// </summary>
		public Action<string, UIInputField, string> OnEndEditAction ;

		/// <summary>
		/// 入力が終了した際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="identity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="view">ビューのインスタンス</param>
		/// <param name="value">変化後の値</param>
		public delegate void OnEndEdit( string identity, UIInputField view, string value ) ;

		/// <summary>
		/// 入力が終了した際に呼び出されるデリゲート
		/// </summary>
		public OnEndEdit OnEndEditDelegate ;

		/// <summary>
		/// 入力が終了した際に呼び出されるアクションを設定する
		/// </summary>
		/// <param name="onEndEditAction">アクションメソッド</param>
		public void SetOnEndEdit( Action<string, UIInputField, string> onEndEditAction )
		{
			OnEndEditAction = onEndEditAction ;
		}
		
		/// <summary>
		/// 入力が終了した際に呼び出されるデリゲートを追加する
		/// </summary>
		/// <param name="onEndEditDelegate">デリゲートメソッド</param>
		public void AddOnEndEdit( OnEndEdit onEndEditDelegate )
		{
			OnEndEditDelegate += onEndEditDelegate ;
		}
		
		/// <summary>
		/// 入力が終了した際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="onEndEditDelegate">デリゲートメソッド</param>
		public void RemoveOnEndEdit( OnEndEdit onEndEditDelegate )
		{
			OnEndEditDelegate -= onEndEditDelegate ;
		}
		
		//---------------------------

		/// <summary>
		/// エンターキーが押された際に呼び出されるアクション
		/// </summary>
		public Action<string, UIInputField, string> OnEnterKeyPressedAction ;

		/// <summary>
		/// エンターキーが押された際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="identity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="view">ビューのインスタンス</param>
		/// <param name="value">変化後の値</param>
		public delegate void OnEnterKeyPressed( string identity, UIInputField view, string value ) ;

		/// <summary>
		/// エンターキーが押された際に呼び出されるデリゲート
		/// </summary>
		public OnEnterKeyPressed OnEnterKeyPressedDelegate ;

		/// <summary>
		/// エンターキーが押された際に呼び出されるアクションを設定する(ただしフォーカルを持っている必要がある)
		/// </summary>
		/// <param name="onPressEnterKeyAction">アクションメソッド</param>
		public void SetOnEnterKeyPressed( Action<string, UIInputField, string> onEnterKeyPressedAction )
		{
			OnEnterKeyPressedAction = onEnterKeyPressedAction ;
		}

		/// <summary>
		/// エンターキーが押された際に呼び出されるアクションを追加する(ただしフォーカルを持っている必要がある)
		/// </summary>
		/// <param name="onPressEnterKeyDelegate">デリゲートメソッド</param>
		public void AddOnEnterKeyPressed( OnEnterKeyPressed onEnterKeyPressedDelegate )
		{
			OnEnterKeyPressedDelegate += onEnterKeyPressedDelegate ;
		}

		/// <summary>
		/// エンターキーが押された際に呼び出されるアクションを削除する(ただしフォーカルを持っている必要がある)
		/// </summary>
		/// <param name="onPressEnterKeyDelegate">デリゲートメソッド</param>
		public void RemoveOnEnterKeyPressed( OnEnterKeyPressed onEnterKeyPressedDelegate )
		{
			OnEnterKeyPressedDelegate -= onEnterKeyPressedDelegate ;
		}

		//---------------------------
		
		// 内部リスナー
		private void OnEndEditInner( string value )
		{
			// 必要に応じてフィルターをかける
			value = ProcessInputFilter( value ) ;

			CTMP_InputField.text = value ;

			//----------------------------------------------------------

			if( OnEndEditAction != null || OnEndEditDelegate != null )
			{
				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				OnEndEditAction?.Invoke( identity, this, value ) ;
				OnEndEditDelegate?.Invoke( identity, this, value ) ;
			}

			if( Input.GetKey( KeyCode.Return ) == true )
			{
				if( OnEnterKeyPressedAction != null || OnEnterKeyPressedDelegate != null )
				{
					string identity = Identity ;
					if( string.IsNullOrEmpty( identity ) == true )
					{
						identity = name ;
					}

					OnEnterKeyPressedAction?.Invoke( identity, this, value ) ;
					OnEnterKeyPressedDelegate?.Invoke( identity, this, value ) ;
				}
			}
		}

		//---------------------------------------------
		
		/// <summary>
		/// 入力が終了した際に呼び出されるリスナーを追加する
		/// </summary>
		/// <param name="onEndEdit">リスナーメソッド</param>
		public void AddOnEndEditListener( UnityEngine.Events.UnityAction<string> onEndEdit )
		{
			TMP_InputFieldPlus inputField = CTMP_InputField ;
			if( inputField != null )
			{
				inputField.onEndEdit.AddListener( onEndEdit ) ;
			}
		}
		
		/// <summary>
		/// 入力が終了した際に呼び出されるリスナーを削除する
		/// </summary>
		/// <param name="onEndEdit">リスナーメソッド</param>
		public void RemoveOnEndEditListener( UnityEngine.Events.UnityAction<string> onEndEdit )
		{
			TMP_InputFieldPlus inputField = CTMP_InputField ;
			if( inputField != null )
			{
				inputField.onEndEdit.RemoveListener( onEndEdit ) ;
			}
		}
		
		/// <summary>
		/// 入力が終了した際に呼び出されるリスナーを全て削除する
		/// </summary>
		public void RemoveOnEndEditAllListeners()
		{
			TMP_InputFieldPlus inputField = CTMP_InputField ;
			if( inputField != null )
			{
				inputField.onEndEdit.RemoveAllListeners() ;
			}
		}

		//-----------------------------------------------------------

		// 値タイプによるフィルターをかける
		private string ProcessInputFilter( string value )
		{
			// 必要に応じてフィルターをかける
			if( string.IsNullOrEmpty( value ) == false )
			{
				if( m_InputFilterType != InputFilterTypes.String )
				{
					if( double.TryParse( value, out double newValue ) == true )
					{
						if( newValue <  m_MinValue )
						{
							newValue  = m_MinValue ;
						}
						if( newValue >  m_MaxValue )
						{
							newValue  = m_MaxValue ;
						}

						if( m_InputFilterType == InputFilterTypes.Integer )
						{
							m_NumericValue = ( ( long )newValue ).ToString() ;
						}
						else
						{
							int digit = value.Length ;
							m_NumericValue = newValue.ToString() ;
							if( m_NumericValue.Length >  digit )
							{
								m_NumericValue = m_NumericValue.Substring( 0, digit ) ;
							}
						}
					}
					
					value = m_NumericValue ;
				}
			}
			else
			{
				if( m_InputFilterType == InputFilterTypes.Integer || m_InputFilterType == InputFilterTypes.Decimal )
				{
					// 数値タイプの場合は空白にはできない
					m_NumericValue = "0" ;
					value = m_NumericValue ;
				}
			}

			return value ;
		}
	}
}

