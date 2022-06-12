using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.Events ;
using UnityEngine.EventSystems ;
using System ;
using System.Collections ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// uGUI:Image クラスの機能拡張コンポーネントクラス
	/// </summary>
	[ RequireComponent( typeof( ImageNumber ) ) ]
	public class UIImageNumber : UIView
	{
		/// <summary>
		/// カラー(ショートカット)
		/// </summary>
		public Color Color
		{
			get
			{
				ImageNumber imageNumber = CImageNumber ;
				if( imageNumber == null )
				{
					return Color.white ;
				}
				return imageNumber.Color ;
			}
			set
			{
				ImageNumber imageNumber = CImageNumber ;
				if( imageNumber == null )
				{
					return ;
				}
				imageNumber.Color = value ;
			}
		}
	
		/// <summary>
		/// マテリアル(ショートカット)
		/// </summary>
		public Material Material
		{
			get
			{
				ImageNumber imageNumber = CImageNumber ;
				if( imageNumber == null )
				{
					return null ;
				}
				return imageNumber.material ;
			}
			set
			{
				ImageNumber imageNumber = CImageNumber ;
				if( imageNumber == null )
				{
					return ;
				}
				imageNumber.material = value ;
			}
		}

		/// <summary>
		/// 値(ショートカット)
		/// </summary>
		public double Value
		{
			get
			{
				ImageNumber imageNumber = CImageNumber ;
				if( imageNumber == null )
				{
					return 0 ;
				}
				return imageNumber.Value ;
			}
			set
			{
				ImageNumber imageNumber = CImageNumber ;
				if( imageNumber == null )
				{
					return ;
				}
				imageNumber.Value = value ;

				if( m_AutoSizeFitting == true )
				{
					SetSize( imageNumber.PreferredWidth, imageNumber.PreferredHeight ) ;
				}
			}
		}

		/// <summary>
		/// 文字のアンカー(ショートカット)
		/// </summary>
		public TextAnchor Alignment
		{
			get
			{
				ImageNumber imageNumber = CImageNumber ;
				if( imageNumber == null )
				{
					return 0 ;
				}
				return imageNumber.Alignment ;
			}
			set
			{
				ImageNumber imageNumber = CImageNumber ;
				if( imageNumber == null )
				{
					return ;
				}
				imageNumber.Alignment = value ;

				if( m_AutoSizeFitting == true )
				{
					SetSize( imageNumber.PreferredWidth, imageNumber.PreferredHeight ) ;
				}
			}
		}

		/// <summary>
		/// 文字単位のオフセット位置
		/// </summary>
		public Vector3[] CodeOffset
		{
			get
			{
				ImageNumber imageNumber = CImageNumber ;
				if( imageNumber == null )
				{
					return null ;
				}
				return imageNumber.CodeOffset ;
			}
			set
			{
				ImageNumber imageNumber = CImageNumber ;
				if( imageNumber == null )
				{
					return ;
				}
				imageNumber.CodeOffset = value ;

				if( m_AutoSizeFitting == true )
				{
					SetSize( imageNumber.PreferredWidth, imageNumber.PreferredHeight ) ;
				}
			}
		}

		/// <summary>
		/// 文字単位のオフセット位置の設定
		/// </summary>
		/// <param name="codeOffset"></param>
		public void SetCodeOffset( Vector3[] codeOffset )
		{
			ImageNumber imageNumber = CImageNumber ;
			if( imageNumber == null )
			{
				return ;
			}
			imageNumber.SetCodeOffset( codeOffset ) ;
		}

		/// <summary>
		/// ＵＩのサイズを文字のサイズに自動調整するかどうか
		/// </summary>
		[SerializeField]
		protected bool m_AutoSizeFitting = true ;
		public bool AutoSizeFitting{ get{ return m_AutoSizeFitting ; } set{ m_AutoSizeFitting = true ; } }


		/// <summary>
		/// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		/// </summary>
		/// <param name="option"></param>
		override protected void OnBuild( string option = "" )
		{
			ImageNumber imageNumber = CImageNumber ;

			if( imageNumber == null )
			{
				imageNumber = gameObject.AddComponent<ImageNumber>() ;
			}
			if( imageNumber == null )
			{
				// 異常
				return ;
			}

			//----------------------------

			// Default
			imageNumber.Color = Color.white ;
			imageNumber.SpriteSet = SpriteSet.Create( "uGUIHelper/Textures/UIDefaultImageNumber" ) ;

			if( IsCanvasOverlay == true )
			{
				imageNumber.material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}

			imageNumber.raycastTarget = false ;

			ResetRectTransform() ;
		}

		override protected void OnLateUpdate()
		{
			if( m_AutoSizeFitting == true )
			{
				ImageNumber t = CImageNumber ;
				RectTransform r = GetRectTransform() ;
				if( r != null && t != null )
				{
					Vector2 size = r.sizeDelta ;

					if( r.anchorMin.x == r.anchorMax.x )
					{
						size.x = t.PreferredWidth ;
					}
					if( r.anchorMin.y == r.anchorMax.y )
					{
						size.y = t.PreferredHeight ;
					}

					r.sizeDelta = size ;
				}
			}
		}	
	}
}

