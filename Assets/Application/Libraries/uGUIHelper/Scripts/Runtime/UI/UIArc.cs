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
	[ RequireComponent( typeof( Arc ) ) ]
	public class UIArc : UIView
	{
		/// <summary>
		/// カラー(ショートカット)
		/// </summary>
		public Color Color
		{
			get
			{
				Arc arc = CArc ;
				if( arc == null )
				{
					return Color.white ;
				}
				return arc.Color ;
			}
			set
			{
				Arc arc = CArc ;
				if( arc == null )
				{
					return ;
				}
				arc.Color = value ;
			}
		}
	
		/// <summary>
		/// マテリアル(ショートカット)
		/// </summary>
		public Material Material
		{
			get
			{
				Arc arc = CArc ;
				if( arc == null )
				{
					return null ;
				}
				return arc.material ;
			}
			set
			{
				Arc arc = CArc ;
				if( arc == null )
				{
					return ;
				}
				arc.material = value ;
			}
		}


		/// <summary>
		/// スプライト(ショートカット)
		/// </summary>
		public  Sprite  Sprite
		{
			get
			{
				Arc arc = CArc ;
				if( arc == null )
				{
					return null ;
				}
				return arc.sprite ;
			}
			set
			{
				Arc arc = CArc ;
				if( arc == null )
				{
					return ;
				}
				arc.sprite = value ;
			}
		}


		/// <summary>
		/// 内側のカラー(ショートカット)
		/// </summary>
		public    Color  InnerColor
		{
			get
			{
				Arc arc = CArc ;
				if( arc == null )
				{
					return Color.white ;
				}
				return arc.innerColor ;
			}
			set
			{
				Arc arc = CArc ;
				if( arc == null )
				{
					return ;
				}
				arc.innerColor = value ;
			}
		}
		
		/// <summary>
		/// 外側のカラー(ショートカット)
		/// </summary>
		public    Color  OuterColor
		{
			get
			{
				Arc arc = CArc ;
				if( arc == null )
				{
					return Color.white ;
				}
				return arc.outerColor ;
			}
			set
			{
				Arc arc = CArc ;
				if( arc == null )
				{
					return ;
				}
				arc.outerColor = value ;
			}
		}
	
		/// <summary>
		/// 円弧開始角度(ショートカット)
		/// </summary>
		public    float  StartAngle
		{
			get
			{
				Arc arc = CArc ;
				if( arc == null )
				{
					return 0 ;
				}
				return arc.startAngle ;
			}
			set
			{
				Arc arc = CArc ;
				if( arc == null )
				{
					return ;
				}
				arc.startAngle = value ;
			}
		}
	
		/// <summary>
		/// 円弧終了角度(ショートカット)
		/// </summary>
		public    float  EndAngle
		{
			get
			{
				Arc arc = CArc ;
				if( arc == null )
				{
					return 0 ;
				}
				return arc.endAngle ;
			}
			set
			{
				Arc arc = CArc ;
				if( arc == null )
				{
					return ;
				}
				arc.endAngle = value ;
			}
		}
	
		/// <summary>
		/// 円弧方向(ショートカット)
		/// </summary>
		public    Arc.Direction  Direction
		{
			get
			{
				Arc arc = CArc ;
				if( arc == null )
				{
					return Arc.Direction.Right ;
				}
				return arc.direction ;
			}
			set
			{
				Arc arc = CArc ;
				if( arc == null )
				{
					return ;
				}
				arc.direction = value ;
			}
		}

		/// <summary>
		/// 形状(ショートカット)
		/// </summary>
		public    Arc.ShapeType  ShapeType
		{
			get
			{
				Arc arc = CArc ;
				if( arc == null )
				{
					return Arc.ShapeType.Circle ;
				}
				return arc.shapeType ;
			}
			set
			{
				Arc arc = CArc ;
				if( arc == null )
				{
					return ;
				}
				arc.shapeType = value ;
			}
		}


		/// <summary>
		/// 分割数(ショートカット)
		/// </summary>
		public    int Split
		{
			get
			{
				Arc arc = CArc ;
				if( arc == null )
				{
					return 0 ;
				}
				return arc.split ;
			}
			set
			{
				Arc arc = CArc ;
				if( arc == null )
				{
					return ;
				}
				arc.split = value ;
			}
		}

		/// <summary>
		/// テクスチャの張り方(形状が円限定)
		/// </summary>
		public    Arc.DecalType  DecalType
		{
			get
			{
				Arc arc = CArc ;
				if( arc == null )
				{
					return Arc.DecalType.Normal ;
				}
				return arc.decalType ;
			}
			set
			{
				Arc arc = CArc ;
				if( arc == null )
				{
					return ;
				}
				arc.decalType = value ;
			}
		}
	
		//--------------------------------------------------------------------------------

		/// <summary>
		/// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		/// </summary>
		/// <param name="tOption"></param>
		override protected void OnBuild( string option = "" )
		{
			Arc arc = CArc ;

			if( arc == null )
			{
				arc = gameObject.AddComponent<Arc>() ;
			}
			if( arc == null )
			{
				// 異常
				return ;
			}

			//----------------------------

			// Default
			arc.Color = Color.white ;

			ResetRectTransform() ;
		}
	}
}

