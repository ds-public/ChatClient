using UnityEngine ;
using UnityEngine.UI ;
using System ;
using System.Collections ;

namespace uGUIHelper
{
	/// <summary>
	/// uGUI:Slider クラスの機能拡張コンポーネントクラス(複合)
	/// </summary>
	[ RequireComponent(typeof(UnityEngine.UI.Slider))]
	public class UISlider : UIView
	{
		/// <summary>
		/// 全体領域(ショートカット)
		/// </summary>
		public RectTransform FillRect
		{
			get
			{
				Slider slider = CSlider ;
				if( slider == null )
				{
					return null ;
				}
				return slider.fillRect ;
			}
			set
			{
				Slider slider = CSlider ;
				if( slider == null )
				{
					return ;
				}
				slider.fillRect = value ;
			}
		}

		/// <summary>
		/// 下地のイメージ(ショートカット)
		/// </summary>
		public Graphic TargetGraphic
		{
			get
			{
				Slider slider = CSlider ;
				if( slider == null )
				{
					return null ;
				}
				return slider.targetGraphic ;
			}
			set
			{
				Slider slider = CSlider ;
				if( slider == null )
				{
					return ;
				}
				slider.targetGraphic = value ;
			}
		}

		/// <summary>
		/// 移動領域(ショートカット)
		/// </summary>
		public RectTransform HandleRect
		{
			get
			{
				Slider slider = CSlider ;
				if( slider == null )
				{
					return null ;
				}
				return slider.fillRect ;
			}
			set
			{
				Slider slider = CSlider ;
				if( slider == null )
				{
					return ;
				}
				slider.handleRect = value ;
			}
		}

		public Slider.Direction Direction
		{
			get
			{
				Slider slider = CSlider ;
				if( slider == null )
				{
					return Slider.Direction.LeftToRight ;
				}
				return slider.direction ;
			}
			set
			{
				Slider slider = CSlider ;
				if( slider == null )
				{
					return ;
				}

				slider.direction = value ;
			}
		}

		/// <summary>
		/// <see cref="Slider.minValue"/>
		/// </summary>
		public float MinValue
		{
			get
			{
				Slider slider = CSlider ;
				if( slider == null )
				{
					return 0f ;
				}
				return slider.minValue ;
			}
			set
			{
				Slider slider = CSlider ;
				if( slider == null )
				{
					return ;
				}

				slider.minValue = value ;
			}
		}

		/// <summary>
		/// <see cref="Slider.maxValue"/>
		/// </summary>
		public float MaxValue
		{
			get
			{
				Slider slider = CSlider ;
				if( slider == null )
				{
					return 0f ;
				}
				return slider.maxValue ;
			}
			set
			{
				Slider slider = CSlider ;
				if( slider == null )
				{
					return ;
				}

				slider.maxValue = value ;
			}
		}

		/// <summary>
		/// <see cref="Slider.wholeNumbers/>
		/// </summary>
		public bool WholeNumbers
		{
			get
			{
				Slider slider = CSlider ;
				if( slider == null )
				{
					return false ;
				}
				return slider.wholeNumbers ;
			}
			set
			{
				Slider slider = CSlider ;
				if( slider == null )
				{
					return ;
				}

				slider.wholeNumbers = value ;
			}
		}

		/// <summary>
		/// 値
		/// </summary>
		public float Value
		{
			get
			{
				Slider slider = CSlider ;
				if( slider == null )
				{
					return 0f ;
				}
				return slider.value ;
			}
			set
			{
				Slider slider = CSlider ;
				if( slider == null )
				{
					return ;
				}

				if( slider.value != value )
				{
					slider.value = value ;
				}
			}
		}


		private bool m_CallbackDisable = false ;

		/// <summary>
		/// 値(0～1)を設定する(コールバックの発生も指定可能)
		/// </summary>
		/// <param name="tValue"></param>
		/// <param name="tCallback"></param>
		public bool SetValue( float value, bool callback = true )
		{
			Slider slider = CSlider ;
			if( slider == null )
			{
				return false ;
			}

			if( slider.value != value )
			{
				m_CallbackDisable = ! callback ;

				slider.value  = value ;

				m_CallbackDisable = false ;
			}

			return true ;
		}

		/// <summary>
		/// interactable(ショーシカット)
		/// </summary>
		public bool Interactable
		{
			get
			{
				Slider slider = CSlider ;
				if( slider == null )
				{
					return false ;
				}
				return slider.interactable ;
			}
			set
			{
				Slider slider = CSlider ;
				if( slider == null )
				{
					return ;
				}
				slider.interactable = value ;
			}
		}

		//-------------------------------------------------------------

		// 方向
		private enum DirectionTypes
		{
			Unknown    = 0,
			Horizontal = 1,
			Vertical   = 2,
		}


		// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		override protected void OnBuild( string option = "" )
		{
			Slider slider = CSlider ;

			if( slider == null )
			{
				slider = gameObject.AddComponent<Slider>() ;
			}
			if( slider == null )
			{
				// 異常
				return ;
			}

			//---------------------------------

			DirectionTypes direction = DirectionTypes.Unknown ;

			if( option.ToLower() == "h" )
			{
				direction = DirectionTypes.Horizontal ;
			}
			else
			if( option.ToLower() == "v" )
			{
				direction = DirectionTypes.Vertical ;
			}

			Vector2 size = GetCanvasSize() ;
			if( size.x >  0 && size.y >  0 )
			{
				float s ;
				if( size.x <= size.y )
				{
					s = size.x ;
				}
				else
				{
					s = size.y ;
				}

				if( direction == DirectionTypes.Horizontal )
				{
					SetSize( s * 0.25f, s * 0.05f ) ;
					slider.direction = Slider.Direction.LeftToRight ;
				}
				else
				if( direction == DirectionTypes.Vertical )
				{
					SetSize( s * 0.05f, s * 0.25f ) ;
					slider.direction = Slider.Direction.BottomToTop ;
				}
			}

			ResetRectTransform() ;

			UIImage background = AddView<UIImage>( "Background" ) ;
			if( direction == DirectionTypes.Horizontal )
			{
				background.SetAnchorMinAndMax( 0.00f, 0.25f, 1.00f, 0.75f ) ;
			}
			else
			if( direction == DirectionTypes.Vertical )
			{
				background.SetAnchorMinAndMax( 0.25f, 0.00f, 0.75f, 1.00f ) ;
			}
			background.SetMargin( 0, 0, 0, 0 ) ;
			background.Sprite = Resources.Load<Sprite>( "uGUIHelper/Textures/UIDefaultFrame" ) ;
			background.Type = Image.Type.Sliced ;
			background.FillCenter = true ;
			background.SetSize( 0, 0 ) ;
			background.RaycastTarget = true ;	// 重要

			UIView fillArea = AddView<UIView>( "Fill Area" ) ;
			if( direction == DirectionTypes.Horizontal )
			{
				fillArea.SetAnchorMinAndMax( 0.00f, 0.25f, 1.00f, 0.75f ) ;
				fillArea.SetMargin(  5, 15,  0,  0 ) ;
			}
			else
			if( direction == DirectionTypes.Vertical )
			{
				fillArea.SetAnchorMinAndMax( 0.25f, 0.00f, 0.75f, 1.00f ) ;
				fillArea.SetMargin(  0,  0,   5, 15 ) ;
			}

			UIImage fill = fillArea.AddView<UIImage>( "Fill" ) ;
			fill.SetAnchorMinAndMax( 0.00f, 0.00f, 1.00f, 1.00f ) ;
			fill.Sprite = Resources.Load<Sprite>( "uGUIHelper/Textures/UIDefaultButton" ) ;
			fill.Type = Image.Type.Sliced ;
			fill.FillCenter = true ;
			if( direction == DirectionTypes.Horizontal )
			{
				fill.SetMargin( -5, -5,  0,  0 ) ;
			}
			else
			if( direction == DirectionTypes.Vertical )
			{
				fill.SetMargin(  0,  0, -5, -5 ) ;
			}

			slider.fillRect = fill.GetRectTransform() ;


			UIView handleSlideArea = AddView<UIView>( "Handle Slide Area" ) ;
			handleSlideArea.SetAnchorToStretch() ;
			if( direction == DirectionTypes.Horizontal )
			{
				handleSlideArea.SetMargin( 10, 10,  0,  0 ) ;
			}
			else
			if( direction == DirectionTypes.Vertical )
			{
				handleSlideArea.SetMargin(  0,  0, 10, 10 ) ;
			}

			UIImage handle = handleSlideArea.AddView<UIImage>( "Handle" ) ;
			if( direction == DirectionTypes.Horizontal )
			{
				handle.SetAnchorToRightStretch() ;
				handle.Px =  0 ;
				handle.Width = this.Height * 1.0f ;
				handle.SetMarginY( 0, 0 ) ;
			}
			else
			if( direction == DirectionTypes.Vertical )
			{
				handle.SetAnchorToStretchTop() ;
				handle.Py =  0 ;
				handle.Height = this.Width * 1.0f ;
				handle.SetMarginX( 0, 0 ) ;
			}

			handle.Sprite = Resources.Load<Sprite>( "uGUIHelper/Textures/UIDefaultButton" ) ;
			handle.RaycastTarget = true ;	// 重要

			slider.targetGraphic = handle.CImage ;
			slider.handleRect = handle.GetRectTransform() ;
		}

		// 派生クラスの Start
		override protected void OnStart()
		{
			base.OnStart() ;

			// 注意:実行のみにしておかないと ExecuteInEditMode で何度も登録されてしまう
			if( Application.isPlaying == true )
			{
				// カスタムリスナー登録（Awake 起動毎に実行する必要がある）
				if( CSlider != null )
				{
					CSlider.onValueChanged.AddListener( OnValueChangedInner ) ;
				}
			}
		}

		//---------------------------------------------

		/// <summary>
		/// 状態が変化した際に呼び出されるアクション
		/// </summary>
		public Action<string, UISlider, float> OnValueChangedAction ;

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="tIdentity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="tView">ビューのインスタンス</param>
		/// <param name="tValue">変化後の値</param>
		public delegate void OnValueChanged( string identity, UISlider view, float value ) ;

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲート
		/// </summary>
		public OnValueChanged OnValueChangedDelegate ;

		/// <summary>
		/// 状態変化時に呼ばれるアクションを設定する
		/// </summary>
		/// <param name="onValueChangedAction">アクションメソッド</param>
		public void SetOnValueChanged( Action<string, UISlider, float> onValueChangedAction )
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

		// 内部リスナー登録
		private void OnValueChangedInner( float value )
		{
			if( m_CallbackDisable == true )
			{
				return ;
			}

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

		//---------------------------------------------

		/// <summary>
		/// 状態が変化した際に呼び出されるリスナーを追加する
		/// </summary>
		/// <param name="onValueChanged">リスナーメソッド</param>
		public void AddOnValueChangedListener( UnityEngine.Events.UnityAction<float> onValueChanged )
		{
			Slider slider = CSlider ;
			if( slider != null )
			{
				slider.onValueChanged.AddListener( onValueChanged ) ;
			}
		}

		/// <summary>
		/// 状態が変化した際に呼び出されるリスナーを削除する
		/// </summary>
		/// <param name="onValueChanged">リスナーメソッド</param>
		public void RemoveOnValueChangedListener( UnityEngine.Events.UnityAction<float> onValueChanged )
		{
			Slider slider = CSlider ;
			if( slider != null )
			{
				slider.onValueChanged.RemoveListener( onValueChanged ) ;
			}
		}

		/// <summary>
		/// 状態が変化した際に呼び出されるリスナーを全て削除する
		/// </summary>
		public void RemoveOnValueChangedAllListeners()
		{
			Slider slider = CSlider ;
			if( slider != null )
			{
				slider.onValueChanged.RemoveAllListeners() ;
			}
		}
	}
}
