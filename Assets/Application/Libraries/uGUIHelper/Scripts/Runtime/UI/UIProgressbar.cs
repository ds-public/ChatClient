using UnityEngine ;
using UnityEngine.UI ;
using System.Collections ;

namespace uGUIHelper
{
	/// <summary>
	/// プログレスバークラス(複合UI)
	/// </summary>
	public class UIProgressbar : UIImage
	{
		/// <summary>
		/// 領域部のインスタンス
		/// </summary>
		[SerializeField]
		protected UIImage m_Scope ;
		public UIImage		Scope{ get{ return m_Scope ; } set{ m_Scope = value ; } }

		/// <summary>
		/// 画像部のインスタンス
		/// </summary>
		[SerializeField]
		protected UIImage m_Thumb ;
		public UIImage		Thumb{ get{ return m_Thumb ; } set{ m_Thumb = value ; } }

		/// <summary>
		/// ラベルのビューのインスタンス
		/// </summary>
		[SerializeField]
		protected UINumberMesh m_LabelMesh ;
		public    UINumberMesh   LabelMesh{ get{ return m_LabelMesh ; } set{ m_LabelMesh = value ; } }
	
		/// <summary>
		/// バーの表示タイプ
		/// </summary>
		public enum DisplayTypes
		{
			Stretch = 0,
			Mask = 1,
		}

		[SerializeField][HideInInspector]
		private DisplayTypes m_DisplayType = DisplayTypes.Stretch ; 
		public  DisplayTypes   DisplayType
		{
			get
			{
				return m_DisplayType ;
			}
			set
			{
				if( m_DisplayType != value )
				{
					m_DisplayType  = value ;
					UpdateThumb() ;
				}
			}
		}


		/// <summary>
		/// 値(係数)
		/// </summary>
		[SerializeField][HideInInspector]
		private float m_Value = 1 ;
		public  float   Value
		{
			get
			{
				return m_Value ;
			}
			set
			{
				if( m_Value != value )
				{
					m_Value = value ;
					UpdateThumb() ;
					UpdateLabel() ;
				}
			}
		}

		/// <summary>
		/// 値(即値)
		/// </summary>
		[SerializeField][HideInInspector]
		private float m_Number = 100.0f ;
		public  float   Number
		{
			get
			{
				return m_Number ;
			}
			set
			{
				if( m_Number != value )
				{
					m_Number = value ;
					UpdateThumb() ;
					UpdateLabel() ;
				}
			}
		}

		//-----------------------------------------------------
	
		/// <summary>
		/// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		/// </summary>
		/// <param name="option"></param>
		override protected void OnBuild( string option = "" )
		{
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
				SetSize( s * 0.5f, s * 0.05f ) ;
			}


			Sprite defaultFrameSprite = null ;
			Sprite defaultThumbSprite = null ;

#if UNITY_EDITOR

			if( Application.isPlaying == false )
			{
				// メニューから操作した場合のみ自動設定を行う
				DefaultSettings ds = Resources.Load<DefaultSettings>( "uGUIHelper/DefaultSettings" ) ;
				if( ds != null )
				{
					defaultFrameSprite		= ds.ProgressbarFrame ;
					defaultThumbSprite		= ds.ProgressbarThumb ;
				}
			}
			
#endif

			SpriteSet spriteSet = SpriteSet.Create( "uGUIHelper/Textures/UIProgressbar" ) ;

			// Frame
			Image frame = CImage ;

			if( defaultFrameSprite == null )
			{
				frame.sprite = spriteSet[ "UIProgressbar_Frame" ] ;
			}
			else
			{
				frame.sprite = defaultFrameSprite ;
			}
			frame.type = Image.Type.Sliced ;
			frame.fillCenter = true ;

			if( IsCanvasOverlay == true )
			{
				frame.material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}

			UIView fillArea = AddView<UIView>( "Fill Area" ) ;
			fillArea.SetAnchorToStretch() ;

			// Mask
			m_Scope = fillArea.AddView<UIImage>( "Scope" ) ;
			m_Scope.SetAnchorToStretch() ;
			m_Scope.SetMargin( 0, 0, 0, 0 ) ;

			m_Scope.IsMask = true ;
			m_Scope.ShowMaskGraphic = false ;

			if( IsCanvasOverlay == true )
			{
				m_Scope.Material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}

			// Thumb
			m_Thumb = m_Scope.AddView<UIImage>( "Thumb" ) ;
			m_Thumb.SetAnchorToStretch() ;
			m_Thumb.SetMargin( 0, 0, 0, 0 ) ;

			if( defaultThumbSprite == null )
			{
				m_Thumb.Sprite = spriteSet[ "UIProgressbar_Thumb" ] ;
			}
			else
			{
				m_Thumb.Sprite = defaultThumbSprite ;
			}
			m_Thumb.Type = Image.Type.Sliced ;
			m_Thumb.FillCenter = true ;

			if( IsCanvasOverlay == true )
			{
				m_Thumb.Material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}

			UpdateThumb() ;

			// Label
			m_LabelMesh = AddView<UINumberMesh>( "Label" ) ;
			m_LabelMesh.FontSize = ( int )( this.Height * 0.6f ) ;
			m_LabelMesh.IsOutline = true ;
			m_LabelMesh.Percent = true ;

//			if( IsCanvasOverlay == true )
//			{
//				m_Label.Material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
//			}

			UpdateLabel() ;

//			DestroyImmediate( atlas ) ;
		}

		//----------------------------------------------------

		/// <summary>
		/// Thumb 更新
		/// </summary>
		private void UpdateThumb()
		{
			if( m_Scope == null || m_Thumb == null )
			{
				return ;
			}

			if( m_DisplayType == DisplayTypes.Stretch )
			{
				if( m_Value <= 0 )
				{
					m_Scope.SetActive( false ) ;
				}
				else
				{
					m_Scope.SetActive( true ) ;
	
					m_Scope.SetAnchorToStretch() ;
					m_Scope.SetAnchorMin(       0, 0 ) ;
					m_Scope.SetAnchorMax( m_Value, 1 ) ;

					m_Thumb.SetAnchorToStretch() ;
					m_Thumb.SetMargin(   0,   0,   0,   0 ) ;
				}
			}
			else
			if( m_DisplayType == DisplayTypes.Mask )
			{
				if( m_Value <= 0 )
				{
					m_Scope.SetActive( false ) ;
				}
				else
				{
					m_Scope.SetActive( true ) ;
					m_Scope.SetAnchorToStretch() ;

					float d = m_Scope.Width * ( 1.0f - m_Value ) ;

					m_Scope.SetMargin(  0, d,  0,  0 ) ;
	
					m_Thumb.SetAnchorToStretch() ;
					m_Thumb.SetMargin(  0,-d,  0,  0 ) ;
				}
			}
		}

		/// <summary>
		/// Label 更新
		/// </summary>
		private void UpdateLabel()
		{
			if( m_LabelMesh != null )
			{
				m_LabelMesh.Value = m_Value * m_Number ;
			}
		}
	}
}
