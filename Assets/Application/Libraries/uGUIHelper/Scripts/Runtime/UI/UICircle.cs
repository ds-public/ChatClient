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
	[ RequireComponent( typeof( Circle ) ) ]
	public class UICircle : UIView
	{
		/// <summary>
		/// カラー(ショートカット)
		/// </summary>
		public Color Color
		{
			get
			{
				Circle circle = CCircle ;
				if( circle == null )
				{
					return Color.white ;
				}
				return circle.Color ;
			}
			set
			{
				Circle circle = CCircle ;
				if( circle == null )
				{
					return ;
				}
				circle.Color = value ;
			}
		}
	
		/// <summary>
		/// マテリアル(ショートカット)
		/// </summary>
		public Material Material
		{
			get
			{
				Circle circle = CCircle ;
				if( circle == null )
				{
					return null ;
				}
				return circle.material ;
			}
			set
			{
				Circle circle = CCircle ;
				if( circle == null )
				{
					return ;
				}
				circle.material = value ;
			}
		}


		/// <summary>
		/// スプライト(ショートカット)
		/// </summary>
		public  Sprite  Sprite
		{
			get
			{
				Circle circle = CCircle ;
				if( circle == null )
				{
					return null ;
				}
				return circle.Sprite ;
			}
			set
			{
				Circle circle = CCircle ;
				if( circle == null )
				{
					return ;
				}
				circle.Sprite = value ;
			}
		}


		/// <summary>
		/// 内側のカラー
		/// </summary>
		public    Color  InnerColor
		{
			get
			{
				Circle circle = CCircle ;
				if( circle == null )
				{
					return Color.white ;
				}
				return circle.InnerColor ;
			}
			set
			{
				Circle circle = CCircle ;
				if( circle == null )
				{
					return ;
				}
				circle.InnerColor = value ;
			}
		}
		
		/// <summary>
		/// 外側のカラー
		/// </summary>
		public    Color  OuterColor
		{
			get
			{
				Circle circle = CCircle ;
				if( circle == null )
				{
					return Color.white ;
				}
				return circle.OuterColor ;
			}
			set
			{
				Circle circle = CCircle ;
				if( circle == null )
				{
					return ;
				}
				circle.OuterColor = value ;
			}
		}
	
		/// <summary>
		/// 分割数
		/// </summary>
		public    int  Split
		{
			get
			{
				Circle circle = CCircle ;
				if( circle == null )
				{
					return 0 ;
				}
				return circle.Split ;
			}
			set
			{
				Circle circle = CCircle ;
				if( circle == null )
				{
					return ;
				}
				circle.Split = value ;
			}
		}
	


		/// <summary>
		/// 内側の塗りつぶしの有無
		/// </summary>
		public    bool  FillInner
		{
			get
			{
				Circle circle = CCircle ;
				if( circle == null )
				{
					return false ;
				}
				return circle.FillInner ;
			}
			set
			{
				Circle circle = CCircle ;
				if( circle == null )
				{
					return ;
				}
				circle.FillInner = value ;
			}
		}


		/// <summary>
		/// 外周の太さ(塗りつぶし無し限定)
		/// </summary>
		public    float  LineWidth
		{
			get
			{
				Circle circle = CCircle ;
				if( circle == null )
				{
					return 0 ;
				}
				return circle.LineWidth ;
			}
			set
			{
				Circle circle = CCircle ;
				if( circle == null )
				{
					return ;
				}
				circle.LineWidth = value ;
			}
		}

		/// <summary>
		/// テクスチャの張り方(塗りつぶし有り限定)
		/// </summary>
		public    Circle.DecalTypes  DecalType
		{
			get
			{
				Circle circle = CCircle ;
				if( circle == null )
				{
					return Circle.DecalTypes.Normal ;
				}
				return circle.DecalType ;
			}
			set
			{
				Circle circle = CCircle ;
				if( circle == null )
				{
					return ;
				}
				circle.DecalType = value ;
			}
		}

		/// <summary>
		/// テクスチャの張り方(塗りつぶし有り限定)
		/// </summary>
		public    float[]  VertexDistanceScales
		{
			get
			{
				Circle circle = CCircle ;
				if( circle == null )
				{
					return null ;
				}
				return circle.VertexDistanceScales ;
			}
			set
			{
				Circle circle = CCircle ;
				if( circle == null )
				{
					return ;
				}
				circle.VertexDistanceScales = value ;
			}
		}
	
		//--------------------------------------------------------------------------------

		// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		override protected void OnBuild( string option = "" )
		{
			Circle circle = CCircle ;

			if( circle == null )
			{
				circle = gameObject.AddComponent<Circle>() ;
			}
			if( circle == null )
			{
				// 異常
				return ;
			}

			//----------------------------

			// Default
			circle.Color = Color.white ;
			
			//----------------------------------

			if( IsCanvasOverlay == true )
			{
				circle.material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}

			//----------------------------------------------------------

			circle.raycastTarget = false ;

			ResetRectTransform() ;
		}
	}
}

