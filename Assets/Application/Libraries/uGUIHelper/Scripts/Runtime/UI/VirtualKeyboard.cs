using UnityEngine ;
using System ;
using System.Collections ;

using TMPro ;

namespace uGUIHelper
{
	/// <summary>
	/// バーチャルキーボード制御クラス
	/// </summary>
	public class VirtualKeyboard : MonoBehaviour
	{
		// シングルトンインスタンス
		private static VirtualKeyboard m_Instance = null ; 
		public  static VirtualKeyboard   Instance
		{
			get
			{
				return m_Instance ;
			}
		}
	
		//---------------------------------------------------------
	
		/// <summary>
		/// インスタンス生成（スクリプトから生成する場合）
		/// </summary>
		/// <returns></returns>
		public static VirtualKeyboard Create()
		{
			if( m_Instance != null )
			{
	//			Debug.Log( "生成済み:AudioManager" ) ;
				return m_Instance ;
			}
		
			// オブジェクトが非アクティブだと検出されないのでオブジェクトを非アクティブにしてはならない
			// この判定は必須で mInstance は static であるためシーンの最初はオブジェクトが存在しても null になっている
			m_Instance = GameObject.FindObjectOfType( typeof( VirtualKeyboard ) ) as VirtualKeyboard ;
			if( m_Instance == null )
			{
				GameObject go = new GameObject( "VirtualKeyboard" ) ;
				go.AddComponent<VirtualKeyboard>() ;
			}

			return m_Instance ;
		}
	
		/// <summary>
		/// インスタンスを取得する
		/// </summary>
		/// <returns></returns>
//		public static VirtualKeyboard Get()
//		{
//			return m_Instance ;
//		}
	
		/// <summary>
		/// インスタンスを破棄する
		/// </summary>
		public static void Delete()
		{	
			if( m_Instance != null )
			{			
				if( Application.isPlaying == false )
				{
					DestroyImmediate( m_Instance.gameObject ) ;
				}
				else
				{
					Destroy( m_Instance.gameObject ) ;
				}
			
				m_Instance = null ;
			}
		}
	
		//-----------------------------------------------------------------
	
		internal void Awake()
		{
			// 既に存在し重複になる場合は自身を削除する
			if( m_Instance != null )
			{
				GameObject.DestroyImmediate( gameObject ) ;
				return ;
			}
		
			VirtualKeyboard instanceOther = GameObject.FindObjectOfType( typeof( VirtualKeyboard ) ) as VirtualKeyboard ;
			if( instanceOther != null )
			{
				if( instanceOther != this )
				{
					GameObject.DestroyImmediate( gameObject ) ;
					return ;
				}
			}
		
			//-----------------------------
			// Awake 内でマニュアルで実行した場合とスクリプトで実行した場合の共通の処理を行う必要がある
		
			m_Instance = this ;
		
			//-----------------------------
		
			// 原点じゃないと気持ち悪い
			gameObject.transform.localPosition = Vector3.zero ;
			gameObject.transform.localRotation = Quaternion.identity ;
			gameObject.transform.localScale = Vector3.one ;
		
			//-----------------------------
		}

		internal void OnDestroy()
		{
			if( m_Instance == this )
			{
				m_Instance  = null ;
			}
		}

		//-------------------------------------------------------------------------------------

		public enum KeyboardTypes
		{
			Default					= 0,	// デフォルト
			ASCIICapable			= 1,	// ASCII配列のキーボード
			NumbersAndPunctuation	= 2,	// 数字と句読点が含まれるキーボード
			URL						= 3,	// URL入力を行うキーボード
			NumberPad				= 4,	// テンキーキーボード
			PhonePad				= 5,	// 電話番号入力を行うキーボード
			NamePhonePad			= 6,	// 電話番号と文字入力を行うキーボード
			EmailAddress			= 7,	// @を含んだメールアドレスを入力するキーボード
		}

		public enum StateTypes
		{
			Unknown					=  0,
			ValueChanged			=  1,	// 文字列が変化した
			Done					=  2,	// 文字列が決定された
			Cancel					=  3,	// 入力がキャンセルさせれた
		}


		/// <summary>
		/// バーチャルキーボードを開く
		/// </summary>
		/// <param name="text"></param>
		/// <param name="placeholder"></param>
		/// <param name="keyboardType"></param>
		/// <param name="isMultiLine"></param>
		/// <param name="isSecureCode"></param>
		/// <param name="onValueChaned"></param>
		/// <returns></returns>
		public static bool Open( string text, string textPlaceholder, KeyboardTypes keyboardType, bool isMultiLine, bool isSecure, bool hideInput, Action<StateTypes,string> onValueChaned )
		{
			VirtualKeyboard instane = Create() ;
			if( instane == null )
			{
				return false ;
			}

			instane.Open_Private( text, textPlaceholder, keyboardType, isMultiLine, isSecure, hideInput, onValueChaned ) ;

			return true ;
		}

		private Action<StateTypes,string>	m_OnValueChanged = null ;
		private string						m_InitialText = "" ;
		private string						m_ActiveText = "" ;



#if !UNITY_EDITOR && ( UNITY_ANDROID || UNITY_IPHONE || UNITY_IOS )

		private TouchScreenKeyboard m_VirtualKeyboard = null ;
		
#else
		private UIImage					m_BasePlane = null ;
#endif

		private void Open_Private( string text, string textPlaceholder, KeyboardTypes keyboardType, bool isMultiLine, bool isSecure, bool hideInput, Action<StateTypes,string> onValueChaned )
		{

			// コールバックを登録する
			m_OnValueChanged = onValueChaned ;

			// 初期の文字列を保存する
			m_InitialText = text ;

			// 現在の文字列を保存する
			m_ActiveText = text ;

			//-------------------------------------------------

#if !UNITY_EDITOR && ( UNITY_ANDROID || UNITY_IPHONE || UNITY_IOS )

			// 実機
			TouchScreenKeyboardType	mobileKeyboardType = TouchScreenKeyboardType.Default ;
			if( keyboardType == KeyboardTypes.Default )
			{
				mobileKeyboardType = TouchScreenKeyboardType.Default ;
			}
			else
			if( keyboardType == KeyboardTypes.ASCIICapable )
			{
				mobileKeyboardType = TouchScreenKeyboardType.ASCIICapable ;
			}
			else
			if( keyboardType == KeyboardTypes.NumbersAndPunctuation )
			{
				mobileKeyboardType = TouchScreenKeyboardType.NumbersAndPunctuation ;
			}
			else
			if( keyboardType == KeyboardTypes.URL )
			{
				mobileKeyboardType = TouchScreenKeyboardType.URL ;
			}
			else
			if( keyboardType == KeyboardTypes.NumberPad )
			{
				mobileKeyboardType = TouchScreenKeyboardType.NumberPad ;
			}
			else
			if( keyboardType == KeyboardTypes.PhonePad )
			{
				mobileKeyboardType = TouchScreenKeyboardType.PhonePad ;
			}
			else
			if( keyboardType == KeyboardTypes.NamePhonePad )
			{
				mobileKeyboardType = TouchScreenKeyboardType.NamePhonePad ;
			}
			else
			if( keyboardType == KeyboardTypes.EmailAddress )
			{
				mobileKeyboardType = TouchScreenKeyboardType.EmailAddress ;
			}

			TouchScreenKeyboard.hideInput = hideInput ;

			m_VirtualKeyboard = TouchScreenKeyboard.Open
			(
				text,
				mobileKeyboardType,
				true,
				isMultiLine,
				isSecure,
				false,
				textPlaceholder
			) ;


#else
			// ＰＣ

			// フルスクリーンのキャンバスを生成する
			UICanvas canvas = UICanvas.CreateWithCamera( transform, 0, 0 ) ;


			// キャンバスのカメラを取得する
			Camera canvasCamera = canvas.GetCanvasCamera() ;

			// キャンバスのカメラをかなり手前に設定する
			canvasCamera.depth = 32767 ;

			// カメラを塗りつぶさないようにする
			canvasCamera.clearFlags = CameraClearFlags.Nothing ;

			// 全画面用のイメージを追加する
			UIImage screen = canvas.AddView<UIImage>( "Virtaul Keyboard Screen" ) ;
			screen.SetAnchorToStretch() ;	// 全画面表示にする
			screen.IsMask = true ;
			screen.CMask.showMaskGraphic = false ;
				
			Vector2 canvasSize = screen.GetCanvasSize() ;

			float cw = canvasSize.x ; // tCanvas._w ;
			float ch = canvasSize.y ; // tCanvas._h ;

			// 最も基本的な縦の長さ
			float bw = cw ;
			float bh = ch * 0.1f ;

			int tFontSize = ( int )( bh * 0.4f ) ;

			//---------------------------------------------------------

			// 土台部分を作る
			UIImage basePlane = screen.AddView<UIImage>( "Base Plane" ) ;
			basePlane.SetAnchorToStretchBottom() ;
			basePlane.SetPivotToCenterBottom() ;

			if( isMultiLine == false )
			{
				// シングル
				basePlane.Height = bh * 2 ;
			}
			else
			{
				// マルチ
				basePlane.Height = bh * 6 ;
			}

			// テキスト入力部を作る
			string option = "" ;
			if( isMultiLine == true )
			{
				option = "MultiLine" ;
			}
			UIInputField inputField = basePlane.AddView<UIInputField>( "Input", option ) ;
			inputField.SetAnchorToStretchTop() ;
			inputField.SetPivotToCenterTop() ;
			inputField.SetMarginX( 2, 2 ) ;
			inputField.Py = -2 ;

			inputField.Text = text ;
			inputField.Placeholder.Text = textPlaceholder ;

			if( isMultiLine == false )
			{
				inputField.Height = ( bh * 1 ) - 4 ;
			}
			else
			{
				inputField.Height = ( bh * 5 ) - 4 ;

				// デフォルトでニューラインになっているのでサブミットに変える
				inputField.LineType = TMP_InputFieldPlus.LineType.MultiLineSubmit ;
			}
			inputField.SetFontSize( tFontSize ) ;

			if( keyboardType == KeyboardTypes.Default )
			{
				inputField.ContentType = TMP_InputFieldPlus.ContentType.Standard ;
			}
			else
			if( keyboardType == KeyboardTypes.ASCIICapable )
			{
				inputField.ContentType = TMP_InputFieldPlus.ContentType.Alphanumeric ;
			}
			else
			if( keyboardType == KeyboardTypes.NumbersAndPunctuation )
			{
				inputField.ContentType = TMP_InputFieldPlus.ContentType.IntegerNumber ;
			}
			else
			if( keyboardType == KeyboardTypes.URL )
			{
				inputField.ContentType = TMP_InputFieldPlus.ContentType.Autocorrected ;
			}
			else
			if( keyboardType == KeyboardTypes.NumberPad )
			{
				inputField.ContentType = TMP_InputFieldPlus.ContentType.DecimalNumber ;
			}
			else
			if( keyboardType == KeyboardTypes.PhonePad )
			{
				inputField.ContentType = TMP_InputFieldPlus.ContentType.Pin ;
			}
			else
			if( keyboardType == KeyboardTypes.NamePhonePad )
			{
				inputField.ContentType = TMP_InputFieldPlus.ContentType.Name ;
			}
			else
			if( keyboardType == KeyboardTypes.EmailAddress )
			{
				inputField.ContentType = TMP_InputFieldPlus.ContentType.EmailAddress ;
			}

			if( isSecure == true )
			{
				inputField.ContentType = TMP_InputFieldPlus.ContentType.Password ;
			}

			// 状態が変化した際に呼び出される
			inputField.SetOnValueChanged( OnValueChanged ) ;

			// リターンキーによる決定時に呼び出される
		//	inputField.SetOnEndEditDelegate( OnEndEdit ) ;
			inputField.SetOnEnterKeyPressed( OnEndEdit ) ;


			// ＯＫボタンを作る
			UIButton button_OK = basePlane.AddView<UIButton>( "OK" ) ;
			button_OK.SetAnchorToLeftBottom() ;
			button_OK.SetSize( bw * 0.5f - 4, bh - 4 ) ;
			button_OK.SetPosition( bw * 0.25f, bh * 0.5f ) ;
			button_OK.AddLabel( "OK", 0xFFFFFFFF, tFontSize ) ;
			button_OK.SetOnButtonClick( OnClick ) ;

			// ＣＡＮＣＥＬボタンを作る
			UIButton button_Cancel = basePlane.AddView<UIButton>( "Cancel" ) ;
			button_Cancel.SetAnchorToRightBottom() ;
			button_Cancel.SetSize( bw * 0.5f - 4, bh - 4 ) ;
			button_Cancel.SetPosition( - bw * 0.25f, bh * 0.5f ) ;
			button_Cancel.AddLabel( "CANCEL", 0xFFFFFFFF, tFontSize ) ;
			button_Cancel.SetOnButtonClick( OnClick ) ;

			float h = basePlane.Height ;

			UITween tweenUp = basePlane.AddTween( "Up" ) ;
			tweenUp.Delay = 0 ;
			tweenUp.Duration = 0.25f ;
			tweenUp.PositionEnabled = true ;
			tweenUp.PositionEaseType = UITween.EaseTypes.EaseOutQuad ;
			tweenUp.PositionFromY = - h ;
			tweenUp.PlayOnAwake = false ;

			UITween tweenDown = basePlane.AddTween( "Down" ) ;
			tweenDown.Delay = 0 ;
			tweenDown.Duration = 0.25f ;
			tweenDown.PositionEnabled = true ;
			tweenDown.PositionEaseType = UITween.EaseTypes.EaseOutQuad ;
			tweenDown.PositionToY = - h ;
			tweenDown.PlayOnAwake = false ;

			tweenDown.SetOnFinished( ( string identity, UITween tween ) =>
			{
				Destroy( gameObject ) ;
			} ) ;

			basePlane.PlayTween( "Up" ) ;

			m_BasePlane = basePlane ;
#endif

		}


#if !UNITY_EDITOR && ( UNITY_ANDROID || UNITY_IPHONE || UNITY_IOS )
		internal void Update()
		{
			// ソースを監視して停止している（予定）
			if( m_VirtualKeyboard != null )
			{
				StateTypes stateType = StateTypes.Unknown ;

				string activeText = m_VirtualKeyboard.text ;

				if( m_VirtualKeyboard.status != TouchScreenKeyboard.Status.Done )
				{
					if( m_ActiveText != activeText )
					{
						m_ActiveText  = activeText ;
						stateType = StateTypes.ValueChanged ;
					}
				}

				if( m_VirtualKeyboard.status == TouchScreenKeyboard.Status.Done )
				{
					if( m_VirtualKeyboard.status != TouchScreenKeyboard.Status.Canceled )
					{
						// キャンセルではない
						m_ActiveText = activeText ;
						stateType = StateTypes.Done ;
					}
					else
					{
						// キャンセルされた
						m_ActiveText = m_InitialText ;
						stateType = StateTypes.Cancel ;
					}
				}
				else
				if( m_VirtualKeyboard.status == TouchScreenKeyboard.Status.Canceled || m_VirtualKeyboard.active == false )
				{
					// キャンセルされた
					m_ActiveText = m_InitialText ;
					stateType = StateTypes.Cancel ;
				}

				if( stateType != StateTypes.Unknown )
				{
					// コールバック呼び出し
					m_OnValueChanged?.Invoke( stateType, m_ActiveText ) ;

					if( stateType != StateTypes.ValueChanged )
					{
						// バーチャルキーボードが閉じられた
						m_VirtualKeyboard = null ;

						Destroy( gameObject ) ;
					}
				}
			}
		}
#endif		

		//-------------------------------------------------------------------------------------

#if !UNITY_EDITOR && ( UNITY_ANDROID || UNITY_IPHONE || UNITY_IOS )
#else
		// 状態が変化した
		private void OnValueChanged( string identity, UIView view, string value )
		{
			m_ActiveText = value ;
			CallHandler( StateTypes.ValueChanged ) ;
		}

		// リターンキーが押された
		private void OnEndEdit( string identity, UIInputField inputField, string value )
		{
			m_ActiveText = value ;
			CallHandler( StateTypes.Done ) ;
		}

		// ボタンが押された
		private void OnClick( string identity, UIButton button )
		{
			if( identity == "OK" )
			{
				CallHandler( StateTypes.Done ) ;
			}
			else
			if( identity == "Cancel" )
			{
				CallHandler( StateTypes.Cancel ) ;
			}
		}

		// コールバック呼び出し
		private void CallHandler( StateTypes stateType )
		{
			if( stateType == StateTypes.Cancel )
			{
				m_ActiveText = m_InitialText ;
			}

			m_OnValueChanged?.Invoke( stateType, m_ActiveText ) ;

			if( stateType != StateTypes.ValueChanged )
			{
				m_BasePlane.PlayTween( "Down" ) ;
			}
		}
#endif

	}
}
