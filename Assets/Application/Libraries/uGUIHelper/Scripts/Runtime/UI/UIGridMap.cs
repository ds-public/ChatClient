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
	/// uGUI:RawImage クラスの機能拡張コンポーネントクラス
	/// </summary>
	[ RequireComponent( typeof( GridMap ) ) ]
	public class UIGridMap : UIView
	{
		/// <summary>
		/// テクスチャ(ショートカット)
		/// </summary>
		public Texture2D Texture
		{
			get
			{
				GridMap gridMap = CGridMap ;
				if( gridMap == null )
				{
					return null ;
				}
				return gridMap.texture ;
			}
			set
			{
				GridMap gridMap = CGridMap ;
				if( gridMap == null )
				{
					return ;
				}
				gridMap.texture = value ;
			}
		}
	
		/// <summary>
		/// カラー(ショートカット)
		/// </summary>
		public Color Color
		{
			get
			{
				GridMap gridMap = CGridMap ;
				if( gridMap == null )
				{
					return Color.white ;
				}
				return gridMap.Color ;
			}
			set
			{
				GridMap gridMap = CGridMap ;
				if( gridMap == null )
				{
					return ;
				}
				gridMap.Color = value ;
			}
		}
		
		/// <summary>
		/// マテリアル(ショートカット)
		/// </summary>
		public Material Material
		{
			get
			{
				GridMap gridMap = CGridMap ;
				if( gridMap == null )
				{
					return null ;
				}
				return gridMap.material ;
			}
			set
			{
				GridMap gridMap = CGridMap ;
				if( gridMap == null )
				{
					return ;
				}
				gridMap.material = value ;
			}
		}
	
		/// <summary>
		/// ＵＩの横方向の分割数(ショートカット)
		/// </summary>
		public int VertexHorizontalGrid
		{
			get
			{
				GridMap gridMap = CGridMap ;
				if( gridMap == null )
				{
					return 0 ;
				}
				return gridMap.vertexHorizontalGrid ;
			}
			set
			{
				GridMap gridMap = CGridMap ;
				if( gridMap == null )
				{
					return ;
				}
				gridMap.vertexHorizontalGrid = value ;
			}
		}

		/// <summary>
		/// ＵＩの横方向の分割数(ショートカット)
		/// </summary>
		public int VertexVerticalGrid
		{
			get
			{
				GridMap gridMap = CGridMap ;
				if( gridMap == null )
				{
					return 0 ;
				}
				return gridMap.vertexVerticalGrid ;
			}
			set
			{
				GridMap gridMap = CGridMap ;
				if( gridMap == null )
				{
					return ;
				}
				gridMap.vertexVerticalGrid = value ;
			}
		}

		/// <summary>
		/// メッシュの分割密度(ショートカット)
		/// </summary>
		public GridMap.VertexDensity VertexDensity
		{
			get
			{
				GridMap gridMap = CGridMap ;
				if( gridMap == null )
				{
					return 0 ;
				}
				return gridMap.vertexDensity ;
			}
			set
			{
				GridMap gridMap = CGridMap ;
				if( gridMap == null )
				{
					return ;
				}
				gridMap.vertexDensity = value ;
			}
		}

		/// <summary>
		/// テクスチャの横方向の分割数(ショートカット)
		/// </summary>
		public int TextureHorizontalGrid
		{
			get
			{
				GridMap gridMap = CGridMap ;
				if( gridMap == null )
				{
					return 0 ;
				}
				return gridMap.textureHorizontalGrid ;
			}
			set
			{
				GridMap gridMap = CGridMap ;
				if( gridMap == null )
				{
					return ;
				}
				gridMap.textureHorizontalGrid = value ;
			}
		}

		/// <summary>
		/// テクスチャの横方向の分割数(ショートカット)
		/// </summary>
		public int TextureVerticalGrid
		{
			get
			{
				GridMap gridMap = CGridMap ;
				if( gridMap == null )
				{
					return 0 ;
				}
				return gridMap.textureVerticalGrid ;
			}
			set
			{
				GridMap gridMap = CGridMap ;
				if( gridMap == null )
				{
					return ;
				}
				gridMap.textureVerticalGrid = value ;
			}
		}

		/// <summary>
		/// テクスチャのパディング(ショートカット)
		/// </summary>
		public int TextureGridPadding
		{
			get
			{
				GridMap gridMap = CGridMap ;
				if( gridMap == null )
				{
					return 0 ;
				}
				return gridMap.textureGridPadding ;
			}
			set
			{
				GridMap gridMap = CGridMap ;
				if( gridMap == null )
				{
					return ;
				}
				gridMap.textureGridPadding = value ;
			}
		}

		/// <summary>
		/// マップデータの設定(ショートカット)
		/// </summary>
		/// <param name="tMap"></param>
		public void SetData( int[,] data )
		{
			GridMap gridMap = CGridMap ;
			if( gridMap == null )
			{
				return ;
			}
			gridMap.SetData( data ) ;
		}

		/// <summary>
		/// トランジションの方向(ショートカット)
		/// </summary>
		public GridMap.TransitionTypes TransitionType
		{
			get
			{
				GridMap gridMap = CGridMap ;
				if( gridMap == null )
				{
					return 0 ;
				}
				return gridMap.transitionType ;
			}
			set
			{
				GridMap gridMap = CGridMap ;
				if( gridMap == null )
				{
					return ;
				}
				gridMap.transitionType = value ;
			}
		}

		/// <summary>
		/// トランジションの状態値(ショートカット)
		/// </summary>
		public float TransitionFactor
		{
			get
			{
				GridMap gridMap = CGridMap ;
				if( gridMap == null )
				{
					return 0 ;
				}
				return gridMap.transitionFactor ;
			}
			set
			{
				GridMap gridMap = CGridMap ;
				if( gridMap == null )
				{
					return ;
				}
				gridMap.transitionFactor = value ;
			}
		}

		/// <summary>
		/// トランジションの強度値(ショートカット)
		/// </summary>
		public float TransitionIntensity
		{
			get
			{
				GridMap gridMap = CGridMap ;
				if( gridMap == null )
				{
					return 0 ;
				}
				return gridMap.transitionIntensity ;
			}
			set
			{
				GridMap gridMap = CGridMap ;
				if( gridMap == null )
				{
					return ;
				}
				gridMap.transitionIntensity = value ;
			}
		}

		/// <summary>
		/// トランジションの状態反転(ショートカット)
		/// </summary>
		public bool TransitionReverse
		{
			get
			{
				GridMap gridMap = CGridMap ;
				if( gridMap == null )
				{
					return false ;
				}
				return gridMap.transitionReverse ;
			}
			set
			{
				GridMap gridMap = CGridMap ;
				if( gridMap == null )
				{
					return ;
				}
				gridMap.transitionReverse = value ;
			}
		}

		//----------------------------------------------------------------------

		/// <summary>
		/// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		/// </summary>
		/// <param name="tOption"></param>
		override protected void OnBuild( string option = "" )
		{
			GridMap gridMap = CGridMap ;

			if( gridMap == null )
			{
				gridMap = gameObject.AddComponent<GridMap>() ;
			}
			if( gridMap == null )
			{
				// 異常
				return ;
			}

			//----------------------------

			// Default
			gridMap.Color = Color.white ;

			VertexHorizontalGrid	= 16 ;
			VertexVerticalGrid		= 16 ;
			VertexDensity			= GridMap.VertexDensity.Low ;


			ResetRectTransform() ;
		}
	}
}

