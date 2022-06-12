using UnityEngine ;
using UnityEngine.UI ;
using System ;
using System.Collections ;

namespace uGUIHelper
{
	/// <summary>
	/// uGUI:Toggle クラスの機能拡張コンポーネントクラス(複合)
	/// </summary>
	[RequireComponent(typeof(UnityEngine.UI.Toggle))]
	public class UIToggle : UIView
	{
		/// <summary>
		/// ベースのビューのインスタンス
		/// </summary>
		[SerializeField]
		protected UIImage m_Background ;
		public UIImage	Background{ get{ return m_Background ; } set{ m_Background = value ; } }

		/// <summary>
		/// チェックのビューのインスタンス
		/// </summary>
		[SerializeField]
		protected UIImage m_Checkmark ;
		public UIImage	Checkmark{ get{ return m_Checkmark ; } set{ m_Checkmark = value ; } }

		/// <summary>
		/// ラベルのビューのインスタンス
		/// </summary>
		[SerializeField]
		protected UIText m_Label ;
		public UIText	   Label{ get{ return m_Label ; } set{ m_Label = value ; } }
	
		/// <summary>
		/// ラベルのビューのインスタンス
		/// </summary>
		[SerializeField]
		protected UITextMesh m_LabelMesh ;
		public UITextMesh	   LabelMesh{ get{ return m_LabelMesh ; } set{ m_LabelMesh = value ; } }
	
		//-------------------------------------------
		
		/// <summary>
		/// チェック状態(ショートカット)
		/// </summary>
		public bool IsOn
		{
			get
			{
				Toggle toggle = CToggle ;
				if( toggle == null )
				{
					return false ;
				}
			
				return toggle.isOn ;
			}
			set
			{
				Toggle toggle = CToggle ;
				if( toggle == null )
				{
					return ;
				}
			
				toggle.isOn = value ;
			}
		}
	
		/// <summary>
		/// チェック状態(ショートカット)
		/// </summary>
		public bool Value
		{
			get
			{
				Toggle toggle = CToggle ;
				if( toggle == null )
				{
					return false ;
				}
			
				return toggle.isOn ;
			}
			set
			{
				Toggle toggle = CToggle ;
				if( toggle == null )
				{
					return ;
				}
			
				toggle.isOn = value ;
			}
		}

		/// <summary>
		/// interactable(ショーシカット)
		/// </summary>
		public bool Interactable
		{
			get
			{
				Toggle toggle = CToggle ;
				if( toggle == null )
				{
					return false ;
				}
				return toggle.interactable ;
			}
			set
			{
				Toggle toggle = CToggle ;
				if( toggle == null )
				{
					return ;
				}
				toggle.interactable = value ;
			}
		}

		//---------------------------------------------

		// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		override protected void OnBuild( string option = "" )
		{
			Toggle toggle = CToggle ;
		
			if( toggle == null )
			{
				toggle = gameObject.AddComponent<Toggle>() ;
			}
			if( toggle == null )
			{
				// 異常
				return ;
			}

			//-------------------------

			Vector2 size = GetCanvasSize() ;
			if( size.x >  0 && size.y >  0 )
			{
				SetSize( size.y * 0.25f, size.y * 0.05f ) ;
			}
			
			// Background	
			m_Background = AddView<UIImage>( "Background" ) ;
			m_Background.SetAnchorToLeftMiddle() ;
			m_Background.SetPosition( this.Height * 0.5f, 0 ) ;
			m_Background.SetSize( this.Height, this.Height ) ;
			m_Background.Sprite = Resources.Load<Sprite>( "uGUIHelper/Textures/UIDefaultFrame" ) ;
			m_Background.Type = Image.Type.Sliced ;
			m_Background.FillCenter = true ;
			
			m_Background.RaycastTarget = true ;	// 必須

			if( IsCanvasOverlay == true )
			{
				m_Background.Material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}

			toggle.targetGraphic = m_Background.CImage ;
			
			// Checkmark	
			m_Checkmark = m_Background.AddView<UIImage>( "Checkmark" ) ;
			m_Checkmark.SetAnchorToCenter() ;
			m_Checkmark.SetSize( this.Height, this.Height ) ;
			m_Checkmark.Sprite = Resources.Load<Sprite>( "uGUIHelper/Textures/UIDefaultCheck" ) ;
			
			if( IsCanvasOverlay == true )
			{
				m_Checkmark.Material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}

			toggle.graphic = m_Checkmark.CImage ;

#if false
			// Label
			m_Label = AddView<UIText>( "Label" ) ;
			m_Label.SetAnchorToLeftMiddle() ;
			m_Label.SetPosition( this.Height * 1.2f, 0 ) ;
			m_Label.SetPivot( 0, 0.5f ) ;
			m_Label.FontSize = ( int )( this.Height * 0.75f ) ;
			
			if( IsCanvasOverlay == true )
			{
				m_Label.Material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}
#endif

			// LabelMesh
			m_LabelMesh = AddView<UITextMesh>( "Label" ) ;
			m_LabelMesh.SetAnchorToLeftMiddle() ;
			m_LabelMesh.SetPosition( this.Height * 1.2f, 0 ) ;
			m_LabelMesh.SetPivot( 0, 0.5f ) ;
			m_LabelMesh.FontSize = ( int )( this.Height * 0.75f ) ;
			
			//----------------------------------------

			if( option != "no group" )
			{
				// 親にグループがアタッチされていればグループとみなす
				ToggleGroup toggleGroup = GetComponentInParent<ToggleGroup>() ;
				if( toggleGroup != null )
				{
					toggle.group = toggleGroup ;
				}
			}

			//----------------------------------------

			ResetRectTransform() ;
		}

		// 派生クラスの Start
		override protected void OnStart()
		{
			base.OnStart() ;
		
			// 注意:実行のみにしておかないと ExecuteInEditMode で何度も登録されてしまう
			if( Application.isPlaying == true )
			{
				// カスタムリスナー登録（Awake 起動毎に実行する必要がある）
				if( CToggle != null )
				{
					CToggle.onValueChanged.AddListener( OnValueChangedInner ) ;
				}
			}
		}

		//---------------------------------------------
		
		/// <summary>
		/// 状態が変化した際に呼び出されるアクション
		/// </summary>
		public Action<string, UIToggle, bool> OnValueChangedAction ;

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="tIdentity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="tView">ビューのインスタンス</param>
		/// <param name="tValue">変化後の値</param>
		public delegate void OnValueChanged( string identity, UIToggle view, bool value ) ;

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲート
		/// </summary>
		public OnValueChanged OnValueChangedDelegate ;

		/// <summary>
		/// 状態が変化した際に呼び出されるアクションを設定する
		/// </summary>
		/// <param name="tOnValueChangedAction">アクションメソッド</param>
		public void SetOnValueChanged( Action<string, UIToggle, bool> onValueChangedAction )
		{
			OnValueChangedAction = onValueChangedAction ;
		}
		
		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートを追加する
		/// </summary>
		/// <param name="tOnValueChangedDelegate">デリゲートメソッド</param>
		public void AddOnValueChanged( OnValueChanged onValueChangedDelegate )
		{
			OnValueChangedDelegate += onValueChangedDelegate ;
		}
		
		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="tOnValueChangedDelegate">デリゲートメソッド</param>
		public void RemoveOnValueChanged( OnValueChanged onValueChangedDelegate )
		{
			OnValueChangedDelegate -= onValueChangedDelegate ;
		}

		// 内部リスナー
		private void OnValueChangedInner( bool value )
		{
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
	
		//-----------------------------------------------------------
		
		/// <summary>
		/// 状態が変化した際に呼び出されるリスナーを追加する
		/// </summary>
		/// <param name="tOnValueChanged">リスナーメソッド</param>
		public void AddOnValueChangedListener( UnityEngine.Events.UnityAction<bool> onValueChanged )
		{
			Toggle toggle = CToggle ;
			if( toggle != null )
			{
				toggle.onValueChanged.AddListener( onValueChanged ) ;
			}
		}
		
		/// <summary>
		/// 状態が変化した際に呼び出されるリスナーを削除する
		/// </summary>
		/// <param name="tOnValueChanged">リスナーメソッド</param>
		public void RemoveOnValueChangedListener( UnityEngine.Events.UnityAction<bool> onValueChanged )
		{
			Toggle toggle = CToggle ;
			if( toggle != null )
			{
				toggle.onValueChanged.RemoveListener( onValueChanged ) ;
			}
		}
		
		/// <summary>
		/// 状態が変化した際に呼び出されるリスナーを全て削除する
		/// </summary>
		public void RemoveOnValueChangedAllListeners()
		{
			Toggle toggle = CToggle ;
			if( toggle != null )
			{
				toggle.onValueChanged.RemoveAllListeners() ;
			}
		}
	}
}
