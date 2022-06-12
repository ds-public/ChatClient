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
	/// uGUI:Image クラスの機能拡張コンポーネントクラス
	/// </summary>
	[ RequireComponent( typeof( Line ) ) ]
	public class UILine : UIView
	{
		/// <summary>
		/// カラー(ショートカット)
		/// </summary>
		public Color Color
		{
			get
			{
				Line line = CLine ;
				if( line == null )
				{
					return Color.white ;
				}
				return line.Color ;
			}
			set
			{
				Line line = CLine ;
				if( line == null )
				{
					return ;
				}
				line.Color = value ;
			}
		}
	
		/// <summary>
		/// マテリアル(ショートカット)
		/// </summary>
		public Material Material
		{
			get
			{
				Line line = CLine ;
				if( line == null )
				{
					return null ;
				}
				return line.material ;
			}
			set
			{
				Line line = CLine ;
				if( line == null )
				{
					return ;
				}
				line.material = value ;
			}
		}

		/// <summary>
		/// スプライト(ショートカット)
		/// </summary>
		public  Sprite  Sprite
		{
			get
			{
				Line line = CLine ;
				if( line == null )
				{
					return null ;
				}
				return line.sprite ;
			}
			set
			{
				Line line = CLine ;
				if( line == null )
				{
					return ;
				}
				line.sprite = value ;
			}
		}

		/// <summary>
		/// 最初のカラー(ショートカット)
		/// </summary>
		public    Color  StartColor
		{
			get
			{
				Line line = CLine ;
				if( line == null )
				{
					return Color.white ;
				}
				return line.startColor ;
			}
			set
			{
				Line line = CLine ;
				if( line == null )
				{
					return ;
				}
				line.startColor = value ;
			}
		}
		
		/// <summary>
		/// 最後のカラー(ショートカット)
		/// </summary>
		public    Color  EndColor
		{
			get
			{
				Line line = CLine ;
				if( line == null )
				{
					return Color.white ;
				}
				return line.endColor ;
			}
			set
			{
				Line line = CLine ;
				if( line == null )
				{
					return ;
				}
				line.endColor = value ;
			}
		}
	
		/// <summary>
		/// 最初の太さ(ショートカット)
		/// </summary>
		public    float  StartWidth
		{
			get
			{
				Line line = CLine ;
				if( line == null )
				{
					return 0 ;
				}
				return line.startWidth ;
			}
			set
			{
				Line line = CLine ;
				if( line == null )
				{
					return ;
				}
				line.startWidth = value ;
			}
		}
	
		/// <summary>
		/// 最後の太さ(ショートカット)
		/// </summary>
		public    float  EndWidth
		{
			get
			{
				Line line = CLine ;
				if( line == null )
				{
					return 0 ;
				}
				return line.endWidth ;
			}
			set
			{
				Line line = CLine ;
				if( line == null )
				{
					return ;
				}
				line.endWidth = value ;
			}
		}
		
		/// <summary>
		/// オフセット(ショートカット)
		/// </summary>
		public  Vector2 Offset
		{
			get
			{
				Line line = CLine ;
				if( line == null )
				{
					return Vector2.zero ;
				}
				return line.offset ;
			}
			set
			{
				Line line = CLine ;
				if( line == null )
				{
					return ;
				}
				line.offset = value ;
			}
		}

		/// <summary>
		/// 頂点配列(ショートカット)
		/// </summary>
		public Vector2[] Vertices
		{
			get
			{
				Line line = CLine ;
				if( line == null )
				{
					return null ;
				}
				return line.vertices ;
			}
			set
			{
				Line line = CLine ;
				if( line == null )
				{
					return ;
				}
				line.vertices = value ;
				line.SetAllDirty() ;
			}
		}

		/// <summary>
		/// 座標の位置タイプ(ショートカット)
		public  Line.PositionType  PositionType
		{
			get
			{
				Line line = CLine ;
				if( line == null )
				{
					return Line.PositionType.Relative ;
				}
				return line.positionType ;
			}
			set
			{
				Line line = CLine ;
				if( line == null )
				{
					return ;
				}
				line.positionType = value ;
			}
		}

		
		/// <summary>
		/// トレイルモード
		/// </summary>
		[SerializeField][HideInInspector]
		private bool m_TrailEnabled = false ;
		public  bool   TrailEnabled
		{
			get
			{
				return m_TrailEnabled ;
			}
			set
			{
				if( m_TrailEnabled != value )
				{
					m_TrailEnabled  = value ;
					if( m_TrailEnabled == true )
					{
						Vertices = null ;
					}
				}
			}
		}

		/// <summary>
		/// トレイル用の頂点が消えるまでの時間
		/// </summary>
		[SerializeField]
		private float m_TrailKeepTime = 0.25f ;
		public  float   TrailKeepTime
		{
			get
			{
				return m_TrailKeepTime ;
			}
			set
			{
				m_TrailKeepTime = value ;
			}
		}

		public class TrailData
		{
			public Vector2	Position ;
			public float	Time ;

			public TrailData( Vector2 position, float time )
			{
				Position	= position ;
				Time		= time ;
			}
		}

		private readonly List<TrailData> m_TrailData = new List<TrailData>() ;

		//---------------------------------------------------------------------

		/// <summary>
		/// ＵＩのサイズを文字のサイズに自動調整するかどうか
		/// </summary>
//		public bool autoSizeFitting = true ;


		/// <summary>
		/// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		/// </summary>
		/// <param name="tOption"></param>
		override protected void OnBuild( string option = "" )
		{
			Line line = CLine ;

			if( line == null )
			{
				line = gameObject.AddComponent<Line>() ;
			}
			if( line == null )
			{
				// 異常
				return ;
			}

			//----------------------------

			// Default
			line.Color = Color.white ;

			//----------------------------------

			if( IsCanvasOverlay == true )
			{
				line.material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}

			//----------------------------------------------------------

			ResetRectTransform() ;
		}

		protected override void OnStart()
		{
			base.OnStart() ;

			if( m_TrailEnabled == true )
			{
				// トレイルが有効である場合は消しておく
				Vertices = null ;
			}
		}

		protected override void OnLateUpdate()
		{
			base.OnLateUpdate() ;

			if( m_TrailEnabled == true )
			{
				ProcessTrail() ;
			}
		}

		/// <summary>
		/// トレイルの頂点を追加する
		/// </summary>
		/// <param name="tMove"></param>
		public void AddTrailPosition( Vector2 move )
		{
			if( m_TrailEnabled == false )
			{
				return ;
			}

			float t = Time.realtimeSinceStartup ;

			int l = m_TrailData.Count ;
			if( l == 0 )
			{
				m_TrailData.Add( new TrailData( move, t ) ) ;
			}
			else
			{
				if( m_TrailData[ l - 1 ].Position != move )
				{
					m_TrailData.Add( new TrailData( move, t ) ) ;
				}
			}
		}

		/// <summary>
		/// トレイルの頂点を消去する
		/// </summary>
		public void ClearTrailPosition()
		{
			m_TrailData.Clear() ;
			Vertices = null ;
		}

		/// <summary>
		/// トレイルを処理する
		/// </summary>
		private void ProcessTrail()
		{
			int i, l  ;
		
			if( m_TrailData.Count == 0 )
			{
				Vertices = null ;
				return ;
			}

			l = m_TrailData.Count ;

			float t = Time.realtimeSinceStartup ;
		
			// 経過時間で頂点を消していく
			for( i  =    0 ; i <  l ; i ++ )
			{
				if( ( t - m_TrailData[ 0 ].Time ) >  m_TrailKeepTime )
				{
					m_TrailData.RemoveAt( 0 ) ;
				}
				else
				{
					break ;
				}
			}

			if( m_TrailData.Count >= 2 )
			{
				l = m_TrailData.Count ;
				Vector2[] vertices = new Vector2[ l ] ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					vertices[ i ] = m_TrailData[ i ].Position ;
				}
				Vertices = vertices ;
			}
			else
			{
				Vertices = null ;
			}
		}
	}
}

