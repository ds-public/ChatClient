using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.Events ;
using UnityEngine.EventSystems ;

using TMPro ;

#if UNITY_EDITOR
using UnityEditor ;
#endif


namespace uGUIHelper
{
	public class UIAlphaMaskWindow : MonoBehaviour
	{
/*
#if UNITY_EDITOR
		[MenuItem( "Tools/UIAlphaMaskWindow/FieldRefactor" )]
		internal static void FieldRefactor()
		{
			int c = 0 ;
			UIAlphaMaskWindow[] views = UIEditorUtility.FindComponents<UIAlphaMaskWindow>
			(
				"Assets/Application",
				( _ ) =>
				{
//					_.m_AlphaMaskMaterial		= _.alphaMaskMaterial ;
					c ++ ;
				}
			) ;
			Debug.LogWarning( "------> UIAlphaMaskWindowの数:" + c ) ;
		}
#endif
*/

		/// <summary>
		/// アルファマスク対象に使うマテリアル(シェーダー)
		/// </summary>
		[SerializeField]
		protected Material	m_AlphaMaskMaterial ;
		public    Material	  AlphaMaskMaterial{ get{ return m_AlphaMaskMaterial ; } set{ m_AlphaMaskMaterial = value ; } }

		/// <summary>
		/// アルファマスクを全ての子に影響させるかどうか
		/// </summary>
		[SerializeField]
		protected bool		m_AlphaMaskEffectToAllItem = true ;
		public    bool		  AlphaMaskEffectToAllItem{ get{ return m_AlphaMaskEffectToAllItem ; } set{ m_AlphaMaskEffectToAllItem = value ; } }


		/// <summary>
		/// アルファマスクを自身に影響させるかどうか
		/// </summary>
		[SerializeField]
		protected bool		m_AlphaMaskEffectToMyself = false ;
		public    bool		  AlphaMaskEffectToMyself{ get{ return m_AlphaMaskEffectToMyself ; } set{ m_AlphaMaskEffectToMyself = value ; } }

		/// <summary>
		/// オーバーレイ対応したＵＩ用のシェーダーを使用するかどうか
		/// </summary>
		[SerializeField]
		protected bool		m_IsOverlay = false ;

		//-----------------------------------------------------------

		private Material	m_ActiveMaterial = null ;

		//-------------------------------------------------------------------------------------------

		internal void Awake()
		{
			if( m_AlphaMaskMaterial == null )
			{
				bool overlay = m_IsOverlay ;
				UIView view = GetComponent<UIView>() ;
				if( view.IsCanvasOverlay == true )
				{
					overlay = true ;
				}

				float softnessX = 0 ;
				float softnessY = 0 ;

				RectMask2D rectMask2D = GetComponent<RectMask2D>() ;
				if( rectMask2D != null )
				{
					softnessX = rectMask2D.softness.x ;
					softnessY = rectMask2D.softness.y ;
				}

				if( overlay == false )
				{
					m_ActiveMaterial = GameObject.Instantiate( Resources.Load<Material>( "uGUIHelper/Shaders/Normal/UI-AlphaMask" ) ) ;
				}
				else
				{
					m_ActiveMaterial = GameObject.Instantiate( Resources.Load<Material>( "uGUIHelper/Shaders/Overlay/UI-Overlay-AlphaMask" ) ) ;
				}

				m_ActiveMaterial.SetFloat( "_MaskSoftnessX", softnessX ) ;
				m_ActiveMaterial.SetFloat( "_MaskSoftnessY", softnessY ) ;
			}
			else
			{
				m_ActiveMaterial = m_AlphaMaskMaterial ;
			}
		}

		internal void Start()
		{
			Prepare() ;
		}

		public void Prepare()
		{
			if( Application.isPlaying == true && m_AlphaMaskEffectToAllItem == true )
			{
				// アルファマスクを有効にしているのでテンプレートアイテムに適用する

				UIView[] elements = GetComponentsInChildren<UIView>( true ) ;

				if( elements != null )
				{
					int i, l = elements.Length ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						if( m_AlphaMaskEffectToMyself == false && gameObject != elements[ i ].gameObject )
						{
							if( elements[ i ].IsGraphic == true && elements[ i ].IsMask == false && elements[ i ].IsRectMask2D == false && elements[ i ].IsAlphaMaskTarget == false )
							{
								elements[ i ].IsAlphaMaskTarget  = true ;
							}
						}
					}
				}
			}
		}

		internal void OnDestroy()
		{
			if( m_AlphaMaskMaterial == null && m_ActiveMaterial != null )
			{
				DestroyImmediate( m_ActiveMaterial ) ;
				m_ActiveMaterial = null ;
			}
		}

		/// <summary>
		/// 子がアルファマテリアルを取得する際に呼び出すメソッド
		/// </summary>
		/// <returns></returns>
		public Material GetAlphaMaskMaterial()
		{
			if( enabled == false )
			{
				return null ;
			}

			return m_ActiveMaterial ;
		}
	}
}
