using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.Events ;
using UnityEngine.EventSystems ;
using System ;
using System.Collections ;
using System.Collections.Generic ;

#if UNITY_EDITOR
using UnityEditor ;
#endif

namespace uGUIHelper
{
	/// <summary>
	/// uGUI:ScrollRect クラスの機能拡張コンポーネントクラス(複合)
	/// </summary>
//	[RequireComponent(typeof(UnityEngine.UI.ScrollRect))]
	[RequireComponent(typeof(ScrollRectWrapper))]
	public class UIScrollView : UIImage
	{
/*
#if UNITY_EDITOR
		[MenuItem( "Tools/UIScrollView/FieldRefactor" )]
		private static void FieldRefactor()
		{
			int c = 0 ;
			UIListView[] views = UIEditorUtility.FindComponents<UIListView>
			(
				"Assets/Application",
				( _ ) =>
				{
					_.m_BuildType = _.buildType ;

					c ++ ;
				}
			) ;
			Debug.LogWarning( "------> UIScrollViewの数:" + c ) ;
		}
#endif
*/

		/// <summary>
		/// ビューポート
		/// </summary>
		[SerializeField]
		protected UIImage		m_Viewport ;
		
		/// <summary>
		/// ビューポート
		/// </summary>
		public UIImage Viewport
		{
			get
			{
				if( m_Viewport == null )
				{
					ScrollRect scrollRect = CScrollRect ;
					if( scrollRect != null )
					{
						m_Viewport = scrollRect.viewport.GetComponent<UIImage>() ;
					}
				}
				return m_Viewport ;
			}
		}

		/// <summary>
		/// コンテント(UIScrollView UIListView を設定した際に設定されている[キャッシュ])
		/// </summary>
		[SerializeField]
		protected UIView		m_Content ;

		/// <summary>
		/// Content のインスタンス
		/// </summary>
		public UIView			Content
		{
			get
			{
				if( m_Content == null )
				{
					// m_Content が null になっている場合の保険
					ScrollRect scrollRect = CScrollRect ;
					if( scrollRect != null && scrollRect.content != null )
					{
						m_Content = scrollRect.content.GetComponent<UIView>() ;
					}
				}
#if UNITY_EDITOR
				if( m_Content == null )
				{
					Debug.LogWarning( "Content is null : Path = " + Path ) ;
				}
#endif
				return m_Content ;
			}
		}
		

		/// <summary>
		/// 水平方向スクロール設定(ショートカット)
		/// </summary>
		public bool Horizontal
		{
			get
			{
				ScrollRectWrapper scrollRect = CScrollRect ;
				if( scrollRect == null )
				{
					return false ;
				}
				return scrollRect.horizontal ;
			}
			set
			{
				ScrollRectWrapper scrollRect = CScrollRect ;
				if( scrollRect == null )
				{
					return ;
				}
				scrollRect.horizontal = value ;
			}
		}

		/// <summary>
		/// 垂直方向スクロール設定(ショートカット)
		/// </summary>
		public bool Vertical
		{
			get
			{
				ScrollRectWrapper scrollRect = CScrollRect ;
				if( scrollRect == null )
				{
					return false ;
				}
				return scrollRect.vertical ;
			}
			set
			{
				ScrollRectWrapper scrollRect = CScrollRect ;
				if( scrollRect == null )
				{
					return ;
				}
				scrollRect.vertical = value ;
			}
		}

		/// <summary>
		/// 水平スクロールバー(ショートカット)
		/// </summary>
		public ScrollbarWrapper HorizontalScrollbar
		{
			get
			{
				ScrollRectWrapper scrollRect = CScrollRect ;
				if( scrollRect == null )
				{
					return null ;
				}
				ScrollbarWrapper scrollbar = ( ScrollbarWrapper )scrollRect.horizontalScrollbar ;
				if( scrollbar == null )
				{
					if( m_HorizontalScrollbarElastic != null )
					{
						return m_HorizontalScrollbarElastic.GetComponent<ScrollbarWrapper>() ;
					}
				}
				return scrollbar ;
			}
			set
			{
				ScrollRectWrapper scrollRect = CScrollRect ;
				if( scrollRect == null )
				{
					return ;
				}
				scrollRect.horizontalScrollbar = value ;
			}
		}
	
		/// <summary>
		/// 垂直スクロールバー(ショートカット)
		/// </summary>
		public ScrollbarWrapper VerticalScrollbar
		{
			get
			{
				ScrollRectWrapper scrollRect = CScrollRect ;
				if( scrollRect == null )
				{
					return null ;
				}
				ScrollbarWrapper scrollbar = ( ScrollbarWrapper )scrollRect.verticalScrollbar ;
				if( scrollbar == null )
				{
					if( m_VerticalScrollbarElastic != null )
					{
						scrollbar = m_VerticalScrollbarElastic.GetComponent<ScrollbarWrapper>() ;
					}
				}
				return scrollbar ;
			}
			set
			{
				ScrollRectWrapper scrollRect = CScrollRect ;
				if( scrollRect == null )
				{
					return ;
				}
				scrollRect.verticalScrollbar = value ;
			}
		}

	
		public enum HorizontalScrollbarPositionTypes
		{
			Top    = 0,
			Bottom = 1,
		}

		/// <summary>
		/// 水平スクロールバーの基準位置(ショートカット)
		/// </summary>
		[SerializeField][HideInInspector]
		private HorizontalScrollbarPositionTypes m_HorizontalScrollbarPositionType = HorizontalScrollbarPositionTypes.Bottom ;
		public  HorizontalScrollbarPositionTypes   HorizontalScrollbarPositionType
		{
			get
			{
				return m_HorizontalScrollbarPositionType ;
			}
			set
			{
				if( m_HorizontalScrollbarPositionType != value )
				{
					m_HorizontalScrollbarPositionType  = value ;
					ResetHorizontalScrollbar() ;
				}
			}
		}

		public enum VerticalScrollbarPositionTypes
		{
			Left  = 0,
			Right = 1,
		}

		/// <summary>
		/// 垂直スクロールバーの基準位置(ショートカット)
		/// </summary>
		[SerializeField][HideInInspector]
		private VerticalScrollbarPositionTypes m_VerticalScrollbarPositionType = VerticalScrollbarPositionTypes.Right ;
		public  VerticalScrollbarPositionTypes   VerticalScrollbarPositionType
		{
			get
			{
				return m_VerticalScrollbarPositionType ;
			}
			set
			{
				if( m_VerticalScrollbarPositionType != value )
				{
					m_VerticalScrollbarPositionType  = value ;
					ResetVerticalScrollbar() ;
				}
			}
		}

		/// <summary>
		/// スクロールバーのフェード効果
		/// </summary>
		[SerializeField][HideInInspector]
		private bool m_ScrollbarFadeEnabled      = true ;
		public  bool   ScrollbarFadeEnabled
		{
			get
			{
				return m_ScrollbarFadeEnabled ;
			}
			set
			{
				m_ScrollbarFadeEnabled = value ;

				if( m_ScrollbarFadeEnabled == false )
				{
					float alpha = 1 ;

					if( m_HorizontalScrollbarCanvasGroup != null )
					{
						m_HorizontalScrollbarCanvasGroup.alpha = alpha ;
						m_HorizontalScrollbarCanvasGroup.blocksRaycasts = ( alpha == 1 ) ;
					}

					if( m_VerticalScrollbarCanvasGroup != null )
					{
						m_VerticalScrollbarCanvasGroup.alpha = alpha ;
						m_VerticalScrollbarCanvasGroup.blocksRaycasts = ( alpha == 1 ) ;
					}
				}
			}
		} 

		/// <summary>
		/// スクロールバーのフェードイン時間
		/// </summary>
		[SerializeField][HideInInspector]
		private float m_ScrollbarFadeInDuration   = 0.2f ;
		public  float   ScrollbarFadeInDuration
		{
			get
			{
				return m_ScrollbarFadeInDuration ;
			}
			set
			{
				m_ScrollbarFadeInDuration = value ;
			}
		}

		/// <summary>
		/// スクロールバーのホールド時間
		/// </summary>
		[SerializeField][HideInInspector]
		private float m_ScrollbarFadeHoldDuration = 1.0f ;
		public  float   ScrollbarFadeHoldDuration
		{
			get
			{
				return m_ScrollbarFadeHoldDuration ;
			}
			set
			{
				m_ScrollbarFadeHoldDuration = value ;
			}
		}
	
		/// <summary>
		/// スクロールバーのフェードアウト時間
		/// </summary>
		[SerializeField][HideInInspector]
		private float m_ScrollbarFadeOutDuration  = 0.2f ;
		public  float   ScrollbarFadeOutDuration
		{
			get
			{
				return m_ScrollbarFadeOutDuration ;
			}
			set
			{
				m_ScrollbarFadeOutDuration = value ;
			}
		}
		

		/// <summary>
		/// コンイントがビューより小さい場合にスクロールバーを非表示にする
		/// </summary>
		[SerializeField][HideInInspector]
		private bool m_HidingScrollbarIfContentFew  = true ;
		public  bool   HidingScrollbarIfContentFew
		{
			get
			{
				return m_HidingScrollbarIfContentFew ;
			}
			set
			{
				m_HidingScrollbarIfContentFew = value ;
			}
		}
		
		/// <summary>
		/// コンイントがビューより小さい場合にスクロールバーを非表示にする
		/// </summary>
		[SerializeField][HideInInspector]
		private bool m_InvalidateScrollIfContentFew  = true ;
		public  bool   InvalidateScrollIfContentFew
		{
			get
			{
				return m_InvalidateScrollIfContentFew ;
			}
			set
			{
				m_InvalidateScrollIfContentFew = value ;
			}
		}
		


		private int   m_ScrollbarFadeState = 0 ;
		private float m_ScrollbarFadeTime  = 0 ;
		private float m_ScrollbarFadeAlpha = 0 ;
		private float m_ScrollbarBaseTime  = 0 ;

		// デフォルトのスクロールバーは Elastic にまともに対応していないので独自に制御する
		[SerializeField][HideInInspector]
		private UIScrollbar	m_HorizontalScrollbarElastic = null ;
		public  UIScrollbar   HorizontalScrollbarElastic
		{
			get
			{
				return m_HorizontalScrollbarElastic ;
			}
			set
			{
				m_HorizontalScrollbarElastic = value ;
			}
		}

		// デフォルトのスクロールバーは Elastic にまともに対応していないので独自に制御する
		[SerializeField][HideInInspector]
		private UIScrollbar	m_VerticalScrollbarElastic = null ;
		public  UIScrollbar   VerticalScrollbarElastic
		{
			get
			{
				return m_VerticalScrollbarElastic ;
			}
			set
			{
				m_VerticalScrollbarElastic = value ;
			}
		}


		/// <summary>
		/// スクロールを有効にするかどうか
		/// </summary>
		public		bool	  ScrollEnabled
		{
			get
			{
				return m_ScrollEnabled ;
			}
			set
			{
				m_ScrollEnabled = value ;
				CScrollRect.enabled = value ;
			}
		}

		protected	bool	m_ScrollEnabled = true ;


		//--------------------------------------------------
	
		// 種別
		public enum BuildTypes
		{
			Unknown		= 0,
			ScrollView	= 1,
			ListView	= 2,
			Dropdown	= 3,
		}

		[SerializeField]
		protected BuildTypes m_BuildType = BuildTypes.Unknown ;


		// 方向
		public enum DirectionTypes
		{
			Unknown		= 0,
			Horizontal	= 1,
			Vertical	= 2,
			Both		= 3,
		}

		[SerializeField]
		protected DirectionTypes m_DirectionType = DirectionTypes.Unknown ;

		public    DirectionTypes DirectionType
		{
			get
			{
				if( m_DirectionType == DirectionTypes.Unknown )
				{
					ScrollRectWrapper scrollView = CScrollRect ;

					// 横スクロール
					if( scrollView.horizontal == true  && scrollView.vertical == false )
					{
						return  DirectionTypes.Horizontal ;
					}

					// 縦スクロール
					if( scrollView.horizontal == false && scrollView.vertical == true  )
					{
						return DirectionTypes.Vertical ;
					}

					return DirectionTypes.Unknown ;
				}

				return m_DirectionType ;
			}
			set
			{
				m_DirectionType = value ;
			}
		}

		/// <summary>
		/// スクロールビューが動いているかどうか
		/// </summary>
		public bool IsMoving
		{
			get
			{
				if( CScrollRect.velocity.magnitude >  0 || IsSnapping() == true || m_IsAutoMoving == true )
				{
					return true ;
				}
				return false ;
			}
		}

		private bool m_IsContentDrag = false ;
		private bool m_IsMoving = false ;

		private bool m_IsAutoMoving = false ;

		//------------------------------------------------

		/// <summary>
		/// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		/// </summary>
		/// <param name="option"></param>
		override protected void OnBuild( string option = "" )
		{
			ScrollRectWrapper scrollRect = CScrollRect ;

			if( scrollRect == null )
			{
				scrollRect = gameObject.AddComponent<ScrollRectWrapper>() ;
			}
			if( scrollRect == null )
			{
				// 異常
				return ;
			}
			
			Image image = CImage ;

			//-------------------------------------

			BuildTypes buildType ;
			DirectionTypes directionType ;

			if( option.ToLower() == "sh" )
			{
				buildType = BuildTypes.ScrollView ;
				directionType = DirectionTypes.Horizontal ;
			}
			else
			if( option.ToLower() == "sv" )
			{
				buildType = BuildTypes.ScrollView ;
				directionType = DirectionTypes.Vertical ;
			}
			else
			if( option.ToLower() == "dropdown" )
			{
				buildType = BuildTypes.Dropdown ;
				directionType = DirectionTypes.Vertical ;
			}
			else
			{
				// デフォルト
				buildType = BuildTypes.ScrollView;
				directionType = DirectionTypes.Both ;
			}

			m_BuildType			= buildType ;		// 後から変更は出来ない
			m_DirectionType		= directionType ;	// 後から変更は出来ない

			// 基本的な大きさを設定
			float s = 100.0f ;
			Vector2 size = GetCanvasSize() ;
			if( size.x >  0 && size.y >  0 )
			{
				if( size.x <= size.y )
				{
					s = size.x ;
				}
				else
				{
					s = size.y ;
				}
				s *= 0.5f ;
			}
				
			ResetRectTransform() ;

			// 方向を設定
			if( directionType == DirectionTypes.Horizontal )
			{
				scrollRect.horizontal = true ;
				scrollRect.vertical   = false ;
				SetSize( s, s * 0.75f ) ;
			}
			else
			if( directionType == DirectionTypes.Vertical )
			{
				scrollRect.horizontal = false ;
				scrollRect.vertical   = true ;
				SetSize( s * 0.75f, s ) ;
			}
			else
			{
				scrollRect.horizontal = true ;
				scrollRect.vertical   = true ;
				SetSize( s, s ) ;
			}

			if( buildType == BuildTypes.Dropdown )
			{
				scrollRect.movementType = ScrollRect.MovementType.Clamped ;
			}

			// Mask 等を設定する Viewport を設定(スクロールバーは表示したいので ScrollRect と Mask は別の階層に分ける)
			m_Viewport = AddView<UIImage>( "Viewport" ) ;
			m_Viewport.SetAnchorToStretch() ;
			m_Viewport.SetMargin( 0, 0, 0, 0 ) ;
			m_Viewport.SetPivot( 0, 1 ) ;
			scrollRect.viewport = m_Viewport.GetRectTransform() ;

			// マスクは CanvasRenderer と 何等かの表示を伴うコンポートと一緒でなければ使用できない
//			Mask mask = m_Viewport.gameObject.AddComponent<Mask>() ;
//			mask.showMaskGraphic = false ;
			m_Viewport.gameObject.AddComponent<RectMask2D>() ;
//			m_Viewport.color = new Color( 0, 0, 0, 0 ) ;
			m_Viewport.CImage.enabled = false ;

			// Content を追加する
			UIView content = CreateContent( m_Viewport, buildType, directionType ) ;
			if( content != null )
			{
				scrollRect.content = content.GetRectTransform() ;
				m_Content = content ;
				
				if( buildType == BuildTypes.Dropdown )
				{
					CreateDropdownItem( content ) ;
				}
			}

			// 自身の Image
			image.sprite = Resources.Load<Sprite>( "uGUIHelper/Textures/UIBlank" ) ;
			image.color = new Color32( 255, 255, 255,  63 ) ;
			image.type = Image.Type.Sliced ;

			// ホイール感度
			scrollRect.scrollSensitivity = 10 ;
		}

		// デフォルトの Content を生成する
		private UIView CreateContent( UIView parent, BuildTypes buildType, DirectionTypes directionType )
		{
			UIView content = null ;

			if( buildType == BuildTypes.ScrollView )
			{
				// ScrollView

				content = parent.AddView<UIView>( "Content" ) ;

				if( directionType == DirectionTypes.Horizontal )
				{
					// 横スクロール
					content.SetAnchorToLeftStretch() ;
					content.SetMargin( 0, 0, 0, 0 ) ;
				
					content.Width = this.Width ;

					content.Height =  0 ;
					content.Px =  0 ;
					content.Py =  0 ;

					content.SetPivot( 0.0f, 1.0f ) ;
						
//					content._contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize ;
//					content._contentSizeFitter.verticalFit   = ContentSizeFitter.FitMode.Unconstrained ;

//					content.AddComponent<HorizontalLayoutGroup>() ;
//					content._horizontalLayoutGroup.childAlignment = TextAnchor.UpperLeft ;
				}
				else
				if( directionType == DirectionTypes.Vertical )
				{
					// 縦スクロール
					content.SetAnchorToStretchTop() ;
					content.SetMargin( 0, 0, 0, 0 ) ;
					
					content.Height = this.Height ;

					content.Width =  0 ;
					content.Py =  0 ;
					content.Px =  0 ;
						
					content.SetPivot( 0.0f, 1.0f ) ;

//					content._contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained ;
//					content._contentSizeFitter.verticalFit   = ContentSizeFitter.FitMode.PreferredSize ;

//					content.AddComponent<VerticalLayoutGroup>() ;
//					content._verticalLayoutGroup.childAlignment = TextAnchor.UpperLeft ;
				}
				else
				{
					content.SetAnchorToLeftTop() ;
					content.SetMargin( 0, 0, 0, 0 ) ;
					
					content.Width  = this.Width ;
					content.Height = this.Height ;
					content.Px =  0 ;
					content.Py =  0 ;

					content.SetPivot( 0.0f, 1.0f ) ;
				}
			}
			else
			if( buildType == BuildTypes.Dropdown )
			{
				// Dropdown
				UIDropdown dropdown = null ;
				if( transform.parent != null )
				{
					dropdown = transform.parent.gameObject.GetComponent<UIDropdown>() ;
				}

				if( dropdown == null )
				{
					return null ;
				}

				content = parent.AddView<UIView>( "Content" ) ;

				content.SetAnchorToStretchTop() ;
				content.SetMargin( 0, 0, 0, 0 ) ;
					
				content.Width  =  0 ;
				content.Height = dropdown.Height ;
				content.Py =  0 ;
				content.Px =  0 ;

				content.SetPivot( 0.5f, 1.0f ) ;
			}

			return content ;
		}

		public UIView dropdownItem = null ;

		// テンプレートのアイテムを生成する
		private void CreateDropdownItem( UIView tParent )
		{
			// Dropdown 専用

			UIDropdown dropdown = null ;
			if( transform.parent != null )
			{
				dropdown = transform.parent.gameObject.GetComponent<UIDropdown>() ;
			}

			if( dropdown == null )
			{
				return ;
			}

			UIToggle item = tParent.AddView<UIToggle>( "Item(Template)", "no group" ) ;

			item.SetAnchorToStretchMiddle() ;
			item.Height = dropdown.Height ;

			item.Background.SetAnchorToStretch() ;
			item.Background.Sprite = Resources.Load<Sprite>( "uGUIHelper/Textures/UIBlank" ) ;
			item.Background.name = "Item Background" ;
				
			item.Checkmark.SetAnchorToLeftMiddle() ;
			item.Checkmark.SetPosition( 36,  0 ) ;
			item.Checkmark.SetSize( item.Height * 0.66f,item.Height * 0.66f ) ;
			item.Checkmark.name = "Item Chackmark" ;

			item.LabelMesh.SetAnchorToStretch() ;
			item.LabelMesh.SetMargin( 64, 10,  2,  1 ) ;
			item.LabelMesh.Alignment = TMPro.TextAlignmentOptions.MidlineLeft ;
			item.LabelMesh.Color = new Color32(  50,  50,  50, 255 ) ;
			item.LabelMesh.FontSize = dropdown.CaptionText.FontSize ;
			item.LabelMesh.Text = "Option A" ;
			item.LabelMesh.name = "Item Label" ;

			dropdown.CTMP_Dropdown.itemText = item.LabelMesh.CTextMesh ;
				
			dropdownItem = item ;	// 外部からアクセス可能な変数にインスタンスを保持する
		}


		/// <summary>
		/// 派生クラスの Start
		/// </summary>
		override protected void OnStart()
		{
			base.OnStart() ;

			// イベントトリガーにトランジション用のコールバックを登録する
			if( Application.isPlaying == true )
			{
				// カスタムリスナー登録（Awake 起動毎に実行する必要がある）
				// カスタムリスナー登録（Awake 起動毎に実行する必要がある）
				if( CScrollRect != null )
				{
					CScrollRect.onValueChanged.AddListener( OnValueChangedInner ) ;
				}

				if( m_BuildType == BuildTypes.Dropdown )
				{
					// ドロップダウン用のスクロールビューの場合にリストの表示位置を設定する
					UIDropdown dropdown = transform.parent.GetComponent<UIDropdown>() ;

					// ドロップダウン用のスクロールビュー
					if( dropdown != null )
					{
						float itemSize = 0 ;

						if( dropdown.CTMP_Dropdown.itemText != null )
						{
							UIView view = dropdown.CTMP_Dropdown.itemText.transform.parent.GetComponent<UIView>() ;
							if( view != null )
							{
								itemSize = view.Height ;
							}
						}
						else
						if( dropdown.CTMP_Dropdown.itemImage != null )
						{
							UIView view = dropdown.CTMP_Dropdown.itemImage.transform.parent.GetComponent<UIView>() ;
							if( view != null )
							{
								itemSize = view.Height ;
							}
						}

						if( itemSize >  0 )
						{
							float offset =  dropdown.Value * itemSize ;

							float viewSize = ViewSize ;
							float contentSize = ContentSize ;

							if( ( offset + viewSize ) >  contentSize )
							{
								offset = contentSize - viewSize ;
							}

							Content.Ry = offset ;
						}
					}
				}

				//---------------------------------------------------------
				// スクロールバー関係

				if( m_HorizontalScrollbarElastic != null )
				{
					m_HorizontalScrollbarElasticLength = m_HorizontalScrollbarElastic.Length ;
				}

				if( m_VerticalScrollbarElastic != null )
				{
					m_VerticalScrollbarElasticLength = m_VerticalScrollbarElastic.Length ;
				}

				// スクロールバーのフェード関係
				if( HorizontalScrollbar != null )
				{
					m_HorizontalScrollbarCanvasGroup = HorizontalScrollbar.GetComponent<CanvasGroup>() ;
				}

				if( VerticalScrollbar != null )
				{
					m_VerticalScrollbarCanvasGroup   = VerticalScrollbar.GetComponent<CanvasGroup>() ;
				}

				m_ScrollbarFadeState = 0 ;
				m_ScrollbarFadeTime  = 0 ;
				m_ScrollbarFadeAlpha = 0 ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 表示領域の幅を取得する
		/// </summary>
		public float ViewSize
		{
			get
			{
				if( m_Viewport == null )
				{
					if( CScrollRect != null && CScrollRect.viewport != null )
					{
						m_Viewport = CScrollRect.viewport.GetComponent<UIImage>() ;
					}
				}

				if( m_Viewport == null )
				{
					return 0 ;
				}

				// 横スクロール
				if( DirectionType == DirectionTypes.Horizontal )
				{
					return m_Viewport.Width ;
				}

				// 縦スクロール
				if( DirectionType == DirectionTypes.Vertical )
				{
					return m_Viewport.Height ;
				}
				
				return 0 ;
			}
		}

		/// <summary>
		/// コンテントの幅を取得する
		/// </summary>
		public float ContentSize
		{
			get
			{
				// 横スクロール
				if( DirectionType == DirectionTypes.Horizontal )
				{
					if( Content == null ){ return 0 ; }
					return Content.Width ;
				}

				// 縦スクロール
				if( DirectionType == DirectionTypes.Vertical   )
				{
					if( Content == null ){ return 0 ; }
					return Content.Height ;
				}

				return 0 ;
			}
			set
			{
				// 横スクロール
				if( DirectionType == DirectionTypes.Horizontal )
				{
					if( Content == null ){ return ; }
					Content.Width = value ;
				}

				// 縦スクロール
				if( DirectionType == DirectionTypes.Vertical   )
				{
					if( Content == null ){ return ; }
					Content.Height = value ;
				}
			}
		}

		/// <summary>
		/// コンテントの現在位置を取得する
		/// </summary>
		virtual public float ContentPosition
		{
			get
			{
				// 横スクロール
				if( DirectionType == DirectionTypes.Horizontal )
				{
					if( Content == null ){ return 0 ; }
					return - Content.Rx ;
				}

				// 縦スクロール
				if( DirectionType == DirectionTypes.Vertical )
				{
					if( Content == null ){ return 0 ; }
					return   Content.Ry ;
				}
				
				return 0 ;
			}
			set
			{
				// 横スクロール
				if( DirectionType == DirectionTypes.Horizontal )
				{
					if( Content == null ){ return ; }
					Content.Rx = - value ;
				}

				// 縦スクロール
				if( DirectionType == DirectionTypes.Vertical )
				{
					if( Content == null ){ return ; }
					Content.Ry =   value ;
				}
			}
		}

		/// <summary>
		/// 有効範囲に収めたココンテントの現在位置を取得する
		/// </summary>
		public float RoundingContentPosition
		{
			get
			{
				float contentPosition	= ContentPosition ;
				float contentSize		= ContentSize ;
				float viewSize			= ViewSize ;

				if( contentSize <= viewSize )
				{
					contentPosition  = 0 ;
				}
				else
				{
					if( contentPosition <  0 )
					{
						contentPosition  = 0 ;
					}
					else
					if( contentPosition >  ( contentSize - viewSize ) )
					{
						contentPosition  = ( contentSize - viewSize ) ;
					}
				}

				return contentPosition ;
			}
		}

		//------------------------------------------------------

		/// <summary>
		/// 派生クラスの Update
		/// </summary>
		protected override void OnUpdate()
		{
			base.OnUpdate() ;

			if( m_ScrollEnabled == false )
			{
				return ;	// スクロールさせない
			}

			if( Application.isPlaying == true )
			{
				ScrollRectWrapper scrollRect = CScrollRect ;

				//---------------------------------------------------------

				// スクロールバーのフェード関係
				if( HorizontalScrollbar != null && m_HorizontalScrollbarCanvasGroup != null )
				{
					if( m_HidingScrollbarIfContentFew == true )
					{
						if( this is UIListView )
						{
							UIListView listView = this as UIListView ;
							if( DirectionType == DirectionTypes.Horizontal )
							{
								if( listView.Infinity == false )
								{
									if( listView.ContentSize <= Viewport.Width )
									{
										m_HorizontalScrollbarCanvasGroup.gameObject.SetActive( false ) ;
									}
									else
									{
										m_HorizontalScrollbarCanvasGroup.gameObject.SetActive( true ) ;
									}
								}
							}
						}
						else
						if( this is UIScrollView )
						{
							if( Content.Width <= Viewport.Width )
							{
								m_HorizontalScrollbarCanvasGroup.gameObject.SetActive( false ) ;
							}
							else
							{
								m_HorizontalScrollbarCanvasGroup.gameObject.SetActive( true ) ;
							}
						}
					}
				}

				if( VerticalScrollbar != null && m_VerticalScrollbarCanvasGroup != null )
				{
					if( m_HidingScrollbarIfContentFew == true )
					{
						if( this is UIListView )
						{
							UIListView listView = this as UIListView ;
							if( DirectionType == DirectionTypes.Vertical )
							{
								if( listView.Infinity == false )
								{
									if( listView.ContentSize <= Viewport.Height )
									{
										m_VerticalScrollbarCanvasGroup.gameObject.SetActive( false ) ;
									}
									else
									{
										m_VerticalScrollbarCanvasGroup.gameObject.SetActive( true ) ;
									}
								}
							}
						}
						else
						if( this is UIScrollView )
						{
							if( Content.Height <= Viewport.Height )
							{
								m_VerticalScrollbarCanvasGroup.gameObject.SetActive( false ) ;
							}
							else
							{
								m_VerticalScrollbarCanvasGroup.gameObject.SetActive( true ) ;
							}
						}
					}
				}

				if( m_InvalidateScrollIfContentFew == true && scrollRect != null )
				{
					if( this is UIListView )
					{
						UIListView listView = this as UIListView ;

						if( DirectionType == DirectionTypes.Horizontal )
						{
							if( listView.Infinity == false )
							{
								if( listView.ContentSize <= Viewport.Width )
								{
									scrollRect.enabled = false ;
								}
								else
								{
									scrollRect.enabled = true ;
								}
							}
						}
						else
						if( DirectionType == DirectionTypes.Vertical )
						{
							if( listView.Infinity == false )
							{
								if( listView.ContentSize <= Viewport.Height )
								{
									scrollRect.enabled = false ;
								}
								else
								{
									scrollRect.enabled = true ;
								}
							}
						}
					}
					else
					if( this is UIScrollView && m_BuildType != BuildTypes.Dropdown )
					{
						// ※ドロップダウンの場合にはスクロール禁止を無視する
						if( Content.Width <= Viewport.Width )
						{
							scrollRect.horizontal = false ;
						}
						else
						{
							scrollRect.horizontal = true ;
						}

						if( Content.Height <= Viewport.Height )
						{
							scrollRect.vertical = false ;
						}
						else
						{
							scrollRect.vertical = true ;
						}
					}
				}

				//---------------------------------------------------------
				// 移動処理

				ProcessScrollbarElastic() ;

				if( m_ScrollbarFadeEnabled == true && ( IsInteraction == true || IsInteractionForScrollView == true ) )
				{
					ProcessScrollbarFade() ;
				}

				if( scrollRect != null )
				{
					if( m_IsContentDrag != scrollRect.IsDrag )
					{
						m_IsContentDrag  = scrollRect.IsDrag ;

						OnContentDragInner( m_IsContentDrag ) ;	// ドラッグ
					}
				}

				bool isMoving = IsMoving ;
				if( m_IsMoving != isMoving )
				{
					m_IsMoving  = isMoving ;

					OnContentMoveInner( m_IsMoving ) ;

					if( isMoving == false )
					{
						OnStoppedInner( ContentPosition ) ;	// 停止
					}
				}
			}

			//-----------------------------------------

			if( m_RemoveHorizontalScrollbar == true )
			{
				RemoveHorizontalScrollbar() ;
				m_RemoveHorizontalScrollbar = false ;
			}
		
			if( m_RemoveVerticalScrollbar == true )
			{
				RemoveVerticalScrollbar() ;
				m_RemoveVerticalScrollbar = false ;
			}
		}


		//--------------------------------------------------------

		virtual protected bool IsSnapping()
		{
			return false ;
		}
		
		//--------------------------------------------------------

		private bool m_RemoveHorizontalScrollbar = false ;
		public bool IsHorizontalScrollber
		{
			get
			{
				if( HorizontalScrollbar == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					AddHorizontalScrollbar() ;
				}
				else
				{
					m_RemoveHorizontalScrollbar = true ;
				}
			}
		}

		/// <summary>
		/// 水平スクロールバーを追加する
		/// </summary>
		public void AddHorizontalScrollbar()
		{
			if( HorizontalScrollbar != null )
			{
				return ;
			}
		
			ScrollRectWrapper scrollRect = CScrollRect ;
			if( scrollRect != null )
			{
				UIScrollbar scrollbar = AddView<UIScrollbar>( "Scrollber(H)", "H" ) ;
				scrollbar.IsCanvasGroup = true ;
				scrollRect.horizontalScrollbar = scrollbar.CScrollbar ;
//				HorizontalScrollbar = scrollbar ;

				ResetHorizontalScrollbar() ;
			}
		}
		
		/// <summary>
		/// 水平スクロールバーをリセットする
		/// </summary>
		public void ResetHorizontalScrollbar()
		{
			if( HorizontalScrollbar == null )
			{
				return ;
			}
		
			UIScrollbar scrollbar = HorizontalScrollbar.GetComponent<UIScrollbar>() ;
//			UIScrollbar scrollbar = horizontalScrollbar ;
			if( scrollbar == null )
			{
				return ;
			}

			if( m_HorizontalScrollbarPositionType == HorizontalScrollbarPositionTypes.Top )
			{
				scrollbar.SetAnchorToStretchTop() ;
				scrollbar.SetMargin( 0, 0, 0, 0 ) ;
				scrollbar.Py =  0 ;
				scrollbar.Height = 30 ;
				scrollbar.Width  =  0 ;
				scrollbar.SetPivot( 0, 1 ) ;
			}
			else
			if( m_HorizontalScrollbarPositionType == HorizontalScrollbarPositionTypes.Bottom )
			{
				scrollbar.SetAnchorToStretchBottom() ;
				scrollbar.SetMargin( 0, 0, 0, 0 ) ;
				scrollbar.Py =  0 ;
				scrollbar.Height = 30 ;
				scrollbar.Width  =  0 ;
				scrollbar.SetPivot( 0, 0 ) ;
			}

			CScrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport ;
	//		CScrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide ;
			CScrollRect.horizontalScrollbarSpacing = 0 ;
		}

		/// <summary>
		/// 水平スクロールバーを削除する
		/// </summary>
		public void RemoveHorizontalScrollbar()
		{
			if( HorizontalScrollbar == null )
			{
				return ;
			}
		
			Scrollbar h = HorizontalScrollbar ;

			HorizontalScrollbar = null ;

			if( Application.isPlaying == false )
			{
				DestroyImmediate( h.gameObject ) ;
			}
			else
			{
				Destroy( h.gameObject ) ;
			}
		}

		//-----------------------------------------------------------

		private bool m_RemoveVerticalScrollbar = false ;
		public bool IsVerticalScrollber
		{
			get
			{
				if( VerticalScrollbar == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					AddVerticalScrollbar() ;
				}
				else
				{
					m_RemoveVerticalScrollbar = true ;
				}
			}
		}

		/// <summary>
		/// 垂直スクロールバーを追加する
		/// </summary>
		public void AddVerticalScrollbar()
		{
			if( VerticalScrollbar != null )
			{
				return ;
			}
		
			ScrollRectWrapper scrollRect = CScrollRect ;
			if( scrollRect != null )
			{
				UIScrollbar scrollbar = AddView<UIScrollbar>( "Scrollbar(V)", "V" ) ;
				scrollbar.IsCanvasGroup = true ;
				scrollRect.verticalScrollbar = scrollbar.CScrollbar ;
//				VerticalScrollbar = scrollbar ;

				ResetVerticalScrollbar() ;
			}
		}

		/// <summary>
		/// 垂直スクロールバーをリセットする
		/// </summary>
		public void ResetVerticalScrollbar()
		{
			if( VerticalScrollbar == null )
			{
				return ;
			}
		
			UIScrollbar scrollbar = VerticalScrollbar.GetComponent<UIScrollbar>() ;
//			UIScrollbar scrollbar = verticalScrollbar ;
			if( scrollbar == null )
			{
				return ;
			}

			if( m_VerticalScrollbarPositionType == VerticalScrollbarPositionTypes.Left )
			{
				scrollbar.SetAnchorToLeftStretch() ;
				scrollbar.SetMargin( 0, 0, 0, 0 ) ;
				scrollbar.Px =  0 ;
				scrollbar.Width  = 30 ;
				scrollbar.Height =  0 ;
				scrollbar.SetPivot( 0, 1 ) ;
			}
			else
			if( m_VerticalScrollbarPositionType == VerticalScrollbarPositionTypes.Right )
			{
				scrollbar.SetAnchorToRightStretch() ;
				scrollbar.SetMargin( 0, 0, 0, 0 ) ;
				scrollbar.Px =  0 ;
				scrollbar.Width  = 30 ;
				scrollbar.Height =  0 ;
				scrollbar.SetPivot( 1, 1 ) ;
			}

			CScrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport ;
	//		CScrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide ;
			CScrollRect.verticalScrollbarSpacing = 0 ;
		}


		/// <summary>
		/// 垂直スクロールバーを削除する
		/// </summary>
		public void RemoveVerticalScrollbar()
		{
			if( VerticalScrollbar == null )
			{
				return ;
			}
			
			Scrollbar v = VerticalScrollbar ;

			VerticalScrollbar = null ;

			if( Application.isPlaying == false )
			{
				DestroyImmediate( v.gameObject ) ;
			}
			else
			{
				Destroy( v.gameObject ) ;
			}
		}

		//---------------------------------------------

		/// <summary>
		/// 状態が変化した際に呼び出されるアクション
		/// </summary>
		private Action<string, UIScrollView, Vector2> OnValueChangedAction ;

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="identity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="view">ビューのインスタンス</param>
		/// <param name="state">変化後の値</param>
		public delegate void OnValueChanged( string identity, UIScrollView view, Vector2 value ) ;

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲート
		/// </summary>
		public OnValueChanged OnValueChangedDelegate ;
		
		/// <summary>
		/// 状態が変化した際に呼び出されるアクションを設定する
		/// </summary>
		/// <param name="onValueChangedAction">アクションメソッド</param>
		public void SetOnValueChanged( Action<string, UIScrollView, Vector2> onValueChangedAction )
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
		private void OnValueChangedInner( Vector2 value )
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
		
		/// <summary>
		/// 状態が変化した際に呼び出されるリスナーを追加する
		/// </summary>
		/// <param name="onValueChanged">リスナーメソッド</param>
		public void AddOnValueChangedListener( UnityEngine.Events.UnityAction<Vector2> onValueChanged )
		{
			ScrollRectWrapper scrollRect = CScrollRect ;
			if( scrollRect != null )
			{
				scrollRect.onValueChanged.AddListener( onValueChanged ) ;
			}
		}

		/// <summary>
		/// 状態が変化した際に呼び出されるリスナーを削除する
		/// </summary>
		/// <param name="onValueChanged">リスナーメソッド</param>
		public void RemoveOnValueChangeListener( UnityEngine.Events.UnityAction<Vector2> onValueChanged )
		{
			ScrollRectWrapper scrollRect = CScrollRect ;
			if( scrollRect != null )
			{
				scrollRect.onValueChanged.RemoveListener( onValueChanged ) ;
			}
		}
		
		/// <summary>
		/// 状態が変化した際に呼び出されるリスナーを全て削除する
		/// </summary>
		public void RemoveOnValueChangedAllListeners()
		{
			ScrollRectWrapper scrollRect = CScrollRect ;
			if( scrollRect != null )
			{
				scrollRect.onValueChanged.RemoveAllListeners() ;
			}
		}

		//---------------------------------------------------------------------------

		/// <summary>
		/// 状態が変化した際に呼び出されるアクション
		/// </summary>
		private Action<string, UIScrollView, bool> OnContentMoveAction ;

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="identity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="view">ビューのインスタンス</param>
		/// <param name="state">変化後の値</param>
		public delegate void OnContentMove( string identity, UIScrollView view, bool state ) ;

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲート
		/// </summary>
		public OnContentMove OnContentMoveDelegate ;
		
		/// <summary>
		/// 状態が変化した際に呼び出されるアクションを設定する
		/// </summary>
		/// <param name="onValueChangedAction">アクションメソッド</param>
		public void SetOnContentMove( Action<string, UIScrollView, bool> onContentMoveAction )
		{
			OnContentMoveAction = onContentMoveAction ;
		}
	
		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートを追加する
		/// </summary>
		/// <param name="onValueChangedDelegate">デリゲートメソッド</param>
		public void AddOnContentMove( OnContentMove onContentMoveDelegate )
		{
			OnContentMoveDelegate += onContentMoveDelegate ;
		}
		
		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="onValueChangedDelegate">デリゲートメソッド</param>
		public void RemoveOnContentMove( OnContentMove onContentMoveDelegate )
		{
			OnContentMoveDelegate -= onContentMoveDelegate ;
		}

		// 内部リスナー
		private void OnContentMoveInner( bool state )
		{
			if( OnContentMoveAction != null || OnContentMoveDelegate != null )
			{
				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				OnContentMoveAction?.Invoke( identity, this, state ) ;
				OnContentMoveDelegate?.Invoke( identity, this, state ) ;
			}
		}
		
		//---------------------------------------------------------------------------

		/// <summary>
		/// 状態が変化した際に呼び出されるアクション
		/// </summary>
		private Action<string, UIScrollView, bool> OnContentDragAction ;

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="tIdentity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="tView">ビューのインスタンス</param>
		/// <param name="tValue">変化後の値</param>
		public delegate void OnContentDrag( string identity, UIScrollView view, bool state ) ;

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲート
		/// </summary>
		public OnContentDrag OnContentDragDelegate ;
		
		/// <summary>
		/// 状態が変化した際に呼び出されるアクションを設定する
		/// </summary>
		/// <param name="onValueChangedAction">アクションメソッド</param>
		public void SetOnContentDrag( Action<string, UIScrollView, bool> onContentDragAction )
		{
			OnContentDragAction = onContentDragAction ;
		}
	
		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートを追加する
		/// </summary>
		/// <param name="onValueChangedDelegate">デリゲートメソッド</param>
		public void AddOnContentDrag( OnContentDrag onContentDragDelegate )
		{
			OnContentDragDelegate += onContentDragDelegate ;
		}
		
		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="onValueChangedDelegate">デリゲートメソッド</param>
		public void RemoveOnContentDrag( OnContentDrag onContentDragDelegate )
		{
			OnContentDragDelegate -= onContentDragDelegate ;
		}

		// 内部リスナー
		private void OnContentDragInner( bool state )
		{
			if( OnContentDragAction != null || OnContentDragDelegate != null )
			{
				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				OnContentDragAction?.Invoke( identity, this, state ) ;
				OnContentDragDelegate?.Invoke( identity, this, state ) ;
			}
		}
		
		//---------------------------------------------------------------------------

		/// <summary>
		/// 状態が変化した際に呼び出されるアクション
		/// </summary>
		private Action<string, UIScrollView, float> OnStoppedAction ;

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="identity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="view">ビューのインスタンス</param>
		/// <param name="state">変化後の値</param>
		public delegate void OnStopped( string identity, UIScrollView view, float contentPosition ) ;

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲート
		/// </summary>
		public OnStopped OnStoppedDelegate ;
		
		/// <summary>
		/// 状態が変化した際に呼び出されるアクションを設定する
		/// </summary>
		/// <param name="onValueChangedAction">アクションメソッド</param>
		public void SetOnStopped( Action<string, UIScrollView, float> onStoppedAction )
		{
			OnStoppedAction = onStoppedAction ;
		}
	
		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートを追加する
		/// </summary>
		/// <param name="onValueChangedDelegate">デリゲートメソッド</param>
		public void AddOnStopped( OnStopped onStoppedDelegate )
		{
			OnStoppedDelegate += onStoppedDelegate ;
		}
		
		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="onValueChangedDelegate">デリゲートメソッド</param>
		public void RemoveOnStopped( OnStopped onStoppedDelegate )
		{
			OnStoppedDelegate -= onStoppedDelegate ;
		}

		// 内部リスナー
		private void OnStoppedInner( float contentPosition )
		{
			if( OnStoppedAction != null || OnStoppedDelegate != null )
			{
				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				OnStoppedAction?.Invoke( identity, this, contentPosition ) ;
				OnStoppedDelegate?.Invoke( identity, this, contentPosition ) ;
			}
		}
		
		//---------------------------------------------------------------------------

		// スロールバーのフェード関係

		// スクロールバーの状態変化
		private void ChangeScrollbarFadeState( bool state )
		{
			if( state == true )
			{
				// 押された
				if( m_ScrollbarFadeState == 0 || m_ScrollbarFadeState == 4 )
				{
					// 完全非表示中かフェードアウト中

					m_ScrollbarFadeState  = 1 ;	// フェードイン

					// 現在のアルファ値から初期の時間値を算出する
					m_ScrollbarFadeTime = m_ScrollbarFadeInDuration * m_ScrollbarFadeAlpha ;
					m_ScrollbarBaseTime = Time.realtimeSinceStartup ;
				}
				else
				if( m_ScrollbarFadeState == 3 )
				{
					// 一定時間待ってフェードアウト中
					m_ScrollbarFadeState = 2 ;	// リセット
				}
			}
			else
			{
				// 離された
				if( m_ScrollbarFadeState == 1 )
				{
					// フェードイン中

					if( m_ScrollbarFadeHoldDuration == 0 )
					{
						m_ScrollbarFadeState = 4 ;	// フェードアウト
	
						// 現在のアルファ値から初期の時間値を算出する
						m_ScrollbarFadeTime = m_ScrollbarFadeOutDuration * ( 1 - m_ScrollbarFadeAlpha ) ;
						m_ScrollbarBaseTime = Time.realtimeSinceStartup ;
					}
				}
				else
				if( m_ScrollbarFadeState == 2 )
				{
					// 表示中

					if( m_ScrollbarFadeHoldDuration >  0 )
					{
						m_ScrollbarFadeState = 3 ;	// 一定時間待ってフェードアウト
		
						m_ScrollbarFadeTime = 0 ;
						m_ScrollbarBaseTime = Time.realtimeSinceStartup ;
					}
					else
					{
						m_ScrollbarFadeState = 4 ;	// フェードアウト
		
						// 現在のアルファ値から初期の時間値を算出する
						m_ScrollbarFadeTime = 0 ;
						m_ScrollbarBaseTime = Time.realtimeSinceStartup ;
					}
				}
			}
		}

		private float m_HorizontalScrollbarElasticLength = 0 ;
		private float m_VerticalScrollbarElasticLength = 0 ;

		// 移動処理
		private void ProcessScrollbarElastic()
		{
			if( Content == null )
			{
				return ;
			}

			if( m_HorizontalScrollbarElastic != null && this.Width >  0 && Content.Width >  0 )
			{
				// 横方向
				if( Content.Width <= this.Width )
				{
					m_HorizontalScrollbarElastic.Offset = 0 ;
					m_HorizontalScrollbarElastic.Length = 1 ;
				}
				else
				{
					if( m_HorizontalScrollbarElasticLength == 0 )
					{
						m_HorizontalScrollbarElasticLength = m_HorizontalScrollbarElastic.Length ;
					}

					if( m_HorizontalScrollbarElasticLength >  0 )
					{
						float l = this.Width / Content.Width ;

						bool fixedSize = m_HorizontalScrollbarElastic.FixedSize ;
						if( m_HorizontalScrollbarElasticLength == 0 || m_HorizontalScrollbarElasticLength == 1 )
						{
							fixedSize = false ;	// 長さの固定化は出来ない
						}

						if( fixedSize == false )
						{
							if( l <  m_HorizontalScrollbarElasticLength )
							{
								l  = m_HorizontalScrollbarElasticLength ;	// 最低保証
							}
						}
						else
						{
							l  = m_HorizontalScrollbarElasticLength ;		// 長さ固定
						}

						float w = Viewport.Width ;

						float o = ( - Content.Rx ) / ( Content.Width - w ) ;	// 位置

						if( fixedSize == false )
						{
							if( o <  0 )
							{
								l += o ;
							}
							else
							if( o >  1 )
							{
								l -= ( o - 1 ) ;
							}
						}
						else
						{
							if( o <  0 )
							{
								o  = 0 ;
							}
							else
							if( o >  1 )
							{
								o  = 1 ;
							}
						}

						m_HorizontalScrollbarElastic.Offset = o ;
						m_HorizontalScrollbarElastic.Length = l ;
					}
				}
			}

			if( m_VerticalScrollbarElastic != null && this.Height >  0 && Content.Height >  0 )
			{
				// 縦方向
				if( Content.Height <= this.Height )
				{
					m_VerticalScrollbarElastic.Offset = 0 ;
					m_VerticalScrollbarElastic.Length = 1 ;
				}
				else
				{
					if( m_VerticalScrollbarElasticLength == 0 )
					{
						m_VerticalScrollbarElasticLength = m_VerticalScrollbarElastic.Length ;
					}

					if( m_VerticalScrollbarElasticLength >  0 )
					{
						float l = this.Height / Content.Height ;

						bool fixedSize = m_VerticalScrollbarElastic.FixedSize ;
						if( m_VerticalScrollbarElasticLength == 0 || m_VerticalScrollbarElasticLength == 1 )
						{
							fixedSize = false ;	// 長さの固定化は出来ない
						}

						if( fixedSize == false )
						{
							if( l <  m_VerticalScrollbarElasticLength )
							{
								l  = m_VerticalScrollbarElasticLength ;	// 最低保証
							}
						}
						else
						{
							l  = m_VerticalScrollbarElasticLength ;		// 長さ固定
						}

						float h = Viewport.Height ;

						float o = (   Content.Ry ) / ( Content.Height - h ) ;	// 位置

						if( fixedSize == false )
						{
							if( o <  0 )
							{
								l += (   Content.Ry                            / h ) * 1.0f ;
							}
							else
							if( o >  1 )
							{
								l -= ( ( Content.Ry - ( Content.Height - h ) ) / h ) * 1.0f ;
							}
						}
						else
						{
							if( o <  0 )
							{
								o  = 0 ;
							}
							else
							if( o >  1 )
							{
								o  = 1 ;
							}
						}

						m_VerticalScrollbarElastic.Offset = 1 - o ;
						m_VerticalScrollbarElastic.Length = l ;
					}
				}
			}
		}

		/// <summary>
		/// スクロールバーのフェード処理を停止する
		/// </summary>
		public void StopScrollbarFade()
		{
			m_ScrollbarFadeState = 0 ;

			float alpha = 0 ;

			if( m_HorizontalScrollbarCanvasGroup != null )
			{
				m_HorizontalScrollbarCanvasGroup.alpha = alpha ;
				m_HorizontalScrollbarCanvasGroup.blocksRaycasts = ( alpha == 1 ) ;
			}

			if( m_VerticalScrollbarCanvasGroup != null )
			{
				m_VerticalScrollbarCanvasGroup.alpha = alpha ;
				m_VerticalScrollbarCanvasGroup.blocksRaycasts = ( alpha == 1 ) ;
			}
		}

		private CanvasGroup m_HorizontalScrollbarCanvasGroup = null ;
		private CanvasGroup m_VerticalScrollbarCanvasGroup   = null ;

		private bool m_Drag = false ;

		// スクロールバーのフェード処理
		private void ProcessScrollbarFade()
		{
			if( CScrollRect.IsDrag == true )
			{
				if( m_Drag == false )
				{
					if( ContentSize >  ViewSize )
					{
						ChangeScrollbarFadeState( true ) ;
					}
					m_Drag = true ;
				}
			}
			else
			{
				if( CScrollRect.IsPress == true )
				{
					if( m_Drag == false )
					{
						if( ContentSize >  ViewSize )
						{
							ChangeScrollbarFadeState( true ) ;
						}
						m_Drag = true ;
					}
				}
				else
				{
					if( m_Drag == true )
					{
						if( ContentSize >  ViewSize )
						{
							ChangeScrollbarFadeState( true ) ;
						}
						m_Drag = false ;
					}
				}
			}

			float alpha = 0 ;

			if( m_ScrollbarFadeState == 1 )
			{
				// フェードイン中
				if( m_ScrollbarFadeInDuration >  0 )
				{
					// フェードインを実行
					float time = ( Time.realtimeSinceStartup - m_ScrollbarBaseTime ) + m_ScrollbarFadeTime ;
					if( time >= m_ScrollbarFadeInDuration )
					{
						time  = m_ScrollbarFadeInDuration ;
					}

					alpha = time / m_ScrollbarFadeInDuration ;
					if( time >= m_ScrollbarFadeInDuration )
					{
						m_ScrollbarFadeState = 2 ;	// 表示状態へ
	
						m_ScrollbarFadeTime = 0 ;
						m_ScrollbarBaseTime = Time.realtimeSinceStartup ;
					}
				}
				else
				{
					// 即時表示状態へ
					alpha = 1 ;

					m_ScrollbarFadeState = 2 ;	// 表示状態へ

					m_ScrollbarFadeTime = 0 ;
					m_ScrollbarBaseTime = Time.realtimeSinceStartup ;
				}
			}
			else
			if( m_ScrollbarFadeState == 2 )
			{
				alpha = 1 ;

				bool scrollbar = false ;
				if( HorizontalScrollbar != null && HorizontalScrollbar.IsPress == true )
				{
					scrollbar = true ;
				}
				if( VerticalScrollbar != null && VerticalScrollbar.IsPress == true )
				{
					scrollbar = true ;
				}

				if( UIEventSystem.IsPressing( gameObject ) == false && IsMoving == false && scrollbar == false )
				{
					// クリックで表示されたケース
					if( m_ScrollbarFadeHoldDuration >  0 )
					{
						float time = ( Time.realtimeSinceStartup - m_ScrollbarBaseTime ) ;
						if( time >= m_ScrollbarFadeHoldDuration )
						{
							// 時間が経過したのでフェードアウトへ
	
							m_ScrollbarFadeState = 4 ;	// 表示状態へ
	
							m_ScrollbarFadeTime = 0 ;
							m_ScrollbarBaseTime = Time.realtimeSinceStartup ;
						}
					}
					else
					{
						// 即時フェードアウトへ
						m_ScrollbarFadeState = 4 ;	// 表示状態へ
	
						m_ScrollbarFadeTime = 0 ;
						m_ScrollbarBaseTime = Time.realtimeSinceStartup ;
					}
				}
				else
				{
					m_ScrollbarBaseTime = Time.realtimeSinceStartup ;
				}
			}
			else
			if( m_ScrollbarFadeState == 3 )
			{
				// 一定時間待ってフェードアウト
				alpha = 1 ;

				bool scrollbar = false ;
				if( HorizontalScrollbar != null && HorizontalScrollbar.IsPress == true )
				{
					scrollbar = true ;
				}
				if( VerticalScrollbar != null && VerticalScrollbar.IsPress == true )
				{
					scrollbar = true ;
				}

				if( IsMoving == false && scrollbar == false )
				{
					float time = ( Time.realtimeSinceStartup - m_ScrollbarBaseTime ) ;
					if( time >= m_ScrollbarFadeHoldDuration )
					{
						// 時間が経過したのでフェードアウトへ

						m_ScrollbarFadeState = 4 ;	// 表示状態へ

						m_ScrollbarFadeTime = 0 ;
						m_ScrollbarBaseTime = Time.realtimeSinceStartup ;
					}
				}
				else
				{
					m_ScrollbarBaseTime = Time.realtimeSinceStartup ;
				}
			}
			else
			if( m_ScrollbarFadeState == 4 )
			{
				// フェードウト中
				if( m_ScrollbarFadeOutDuration >  0 )
				{
					// フェードアウトを実行
					float time = ( Time.realtimeSinceStartup - m_ScrollbarBaseTime ) + m_ScrollbarFadeTime ;
					if( time >= m_ScrollbarFadeOutDuration )
					{
						time  = m_ScrollbarFadeOutDuration ;
					}
	
					alpha = 1 - ( time / m_ScrollbarFadeInDuration ) ;
					if( time >= m_ScrollbarFadeOutDuration )
					{
						m_ScrollbarFadeState = 0 ;	// 非表示状態へ
					}
				}
				else
				{
					// 即時非表示へ

					alpha = 0 ;

					m_ScrollbarFadeState = 0 ;	// 非表示状態へ
				}

			}

			m_ScrollbarFadeAlpha = alpha ;

			//------------------------------------------

			if( m_HorizontalScrollbarCanvasGroup != null )
			{
				m_HorizontalScrollbarCanvasGroup.alpha = alpha ;
				m_HorizontalScrollbarCanvasGroup.blocksRaycasts = ( alpha == 1 ) ;
			}

			if( m_VerticalScrollbarCanvasGroup != null )
			{
				m_VerticalScrollbarCanvasGroup.alpha = alpha ;
				m_VerticalScrollbarCanvasGroup.blocksRaycasts = ( alpha == 1 ) ;
			}
		}

		// スクロールバーの位置を設定する
		virtual internal protected void SetPositionFromScrollbar( DirectionTypes directionType, float value )
		{
			if( Content == null )
			{
				return ;
			}

			if( value <  0 )
			{
				value  = 0 ;
			}
			else
			if( value >  1 )
			{
				value  = 1 ;
			}

			if( directionType == DirectionTypes.Horizontal )
			{
				// 横位置を設定する
				if( Content.Width <= 0 || this.Width <= 0 )
				{
					return ;
				}

				if( Content.Width <= this.Width )
				{
					return ;
				}

				float l = Content.Width - this.Width ;
				float o = l * value ;

				Content.Px = - o ;
			}
			else
			if( directionType == DirectionTypes.Vertical )
			{
				// 縦位置を設定する
				if( Content.Height <= 0 || this.Height <= 0 )
				{
					return ;
				}

				if( Content.Height <= this.Height )
				{
					return ;
				}

				float l = Content.Height - this.Height ;
				float o = l * ( 1 - value ) ;

				Content.Py =   o ;
			}

			ChangeScrollbarFadeState( true ) ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 指定の位置まで移動させる
		/// </summary>
		/// <param name="tContentPosition"></param>
		/// <param name="tDuration"></param>
		/// <returns></returns>
		public AsyncState MoveToPosition( float contentPosition, float duration, UITween.EaseTypes easeType = UITween.EaseTypes.EaseOutQuad )
		{
			if( duration <= 0 )
			{
				return null ;
			}

			AsyncState state = new AsyncState( this ) ;
			StartCoroutine( MoveToPosition_Private( contentPosition, duration, easeType, state ) ) ;
			return state ;
		}

		protected IEnumerator MoveToPosition_Private( float contentPositionTo, float duration, UITween.EaseTypes easeType, AsyncState state )
		{
			// 自動移動開始
			m_IsAutoMoving = true ;

			float contentPositionFrom = ContentPosition ;

			float baseTime = Time.realtimeSinceStartup ;
			
			float time, factor, delta ;


			// ムーブ処理中
			while( true )
			{
				time = Time.realtimeSinceStartup - baseTime ;

				factor = time / duration ;
				if( factor >  1 )
				{
					factor  = 1 ;
				}

				delta = contentPositionTo - contentPositionFrom ;
				delta = UITween.GetValue( 0, delta, factor, UITween.ProcessTypes.Ease, easeType ) ;
				
				ContentPosition = contentPositionFrom + delta ;

				if( factor >= 1 || m_IsContentDrag == true || UIEventSystem.IsPressing( gameObject ) == true )
				{
					break ;
				}

				yield return null ;
			}
			
			if( state != null )
			{
				state.IsDone = true ;
			}

			// 自動移動終了
			m_IsAutoMoving = false ;
		}

		//-----------------------------------------------------------

	}
}
