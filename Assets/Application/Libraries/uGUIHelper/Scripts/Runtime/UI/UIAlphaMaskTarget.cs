using System ;
using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.Events ;
using UnityEngine.EventSystems ;
using UnityEngine.Rendering ;

using TMPro ;

namespace uGUIHelper
{
	public class UIAlphaMaskTarget : UIBehaviour, IMaterialModifier
	{
		/// <summary>
		/// カスタムマテリアル
		/// </summary>
		public Material				AlphaMaskMaterial = null ;

		//-----------------------------------------------------------

		private Material			m_AlphaMaskMaterial = null ;

		private bool				m_ShouldRecalculateStencil = true ;

		private Material			m_UsingMaskMaterial = null ;

		private Graphic				m_Graphic = null ;

		private TextMeshProUGUI		m_TextMeshProUGUI = null ;

		private UIAlphaMaskWindow	m_Window = null ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// マテリアルの設定が必要な際に呼び出される
		/// </summary>
		/// <param name="tBaseMaterial"></param>
		/// <returns></returns>
		public virtual Material GetModifiedMaterial( Material baseMaterial )
		{
			Material activeMaterial = ChoiceMaterial( baseMaterial ) ;

			int stencilValue = 0 ;

			if( m_ShouldRecalculateStencil == true )
			{
				Transform rootCanvas = MaskUtilities.FindRootSortOverrideCanvas( transform ) ;
				stencilValue =  MaskUtilities.GetStencilDepth( transform, rootCanvas ) ;
				m_ShouldRecalculateStencil = false ;
			}

			if( stencilValue >  0 )
			{
				Material margedMaskMaterial = StencilMaterial.Add( activeMaterial, ( 1 << stencilValue ) - 1, StencilOp.Keep, CompareFunction.Equal, ColorWriteMask.All, ( 1 << stencilValue ) - 1, 0 ) ;

				if( m_UsingMaskMaterial != null )
				{
					StencilMaterial.Remove( m_UsingMaskMaterial ) ;
				}

				m_UsingMaskMaterial = margedMaskMaterial ;

				activeMaterial = m_UsingMaskMaterial ;
			}

			return activeMaterial ;
		}

		protected override void OnEnable()
		{
			base.OnEnable() ;

			// マテリアルが独自に設定されている場合は複製する
			if( AlphaMaskMaterial != null )
			{
				m_AlphaMaskMaterial = GameObject.Instantiate<Material>( AlphaMaskMaterial ) ;
			}

			m_ShouldRecalculateStencil = true ;
		}

		protected override void OnDisable()
		{
			base.OnDisable() ;

			m_ShouldRecalculateStencil = true ;

			if( m_UsingMaskMaterial != null )
			{
				StencilMaterial.Remove( m_UsingMaskMaterial ) ;
				m_UsingMaskMaterial = null ;
			}

			if( m_AlphaMaskMaterial != null )
			{
				DestroyImmediate( m_AlphaMaskMaterial ) ;
				m_AlphaMaskMaterial = null ;
			}
		}

#if UNITY_EDITOR
		protected override void OnValidate()
		{
			base.OnValidate() ;

			m_ShouldRecalculateStencil = true ;
		}
#endif

		protected override void OnTransformParentChanged()
		{
			base.OnTransformParentChanged() ;

			m_ShouldRecalculateStencil = true ;
		}

		protected override void OnCanvasHierarchyChanged()
		{
			base.OnCanvasHierarchyChanged() ;

			m_ShouldRecalculateStencil = true ;
		}

		public virtual void RecalculateMasking()
		{
			m_ShouldRecalculateStencil = true ;
		}

		/// <summary>
		/// アルファマスクマテリアルを更新する
		/// </summary>
		public void RefreshAlphaMask()
		{
			m_ShouldRecalculateStencil = true ;

			if( m_Graphic == null )
			{
				m_Graphic = GetComponent<Graphic>() ;
			}

			if( m_Graphic != null )
			{
				m_Graphic.SetMaterialDirty() ;
			}
		}

		// 使用するマテリアルを選別する
		private Material ChoiceMaterial( Material basisMaterial )
		{
			if( enabled == false )
			{
				return basisMaterial ;
			}

			if( m_AlphaMaskMaterial != null )
			{
				return m_AlphaMaskMaterial ;
			}

			Material cloneMaterial ;

			//----------------------------------------------------------
			// TextMesh(元々RectMask2Dに対応済み)

			if( m_TextMeshProUGUI == null )
			{
				m_TextMeshProUGUI = GetComponent<TextMeshProUGUI>() ;
			}

			if( m_TextMeshProUGUI != null )
			{
				return basisMaterial ;
			}

			//----------------------------------------------------------
			// その他

			// AlphaMaskWindow のマテリアルを取得する
			cloneMaterial = GetAlphaMaskMaterial() ;
			if( cloneMaterial != null )
			{
				return cloneMaterial ;
			}
			else
			{
				return basisMaterial ;
			}
		}

		// AlphaMaskWindow のマテリアルを取得する
		private Material GetAlphaMaskMaterial()
		{
			if( m_Window == null )
			{
				Transform t = transform ;

				int i, l = 64 ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( t.GetComponent<Canvas>() != null )
					{
						break ;
					}
				
					m_Window = t.GetComponent<UIAlphaMaskWindow>() ;
					if( m_Window != null )
					{
						break ;
					}

					if( t.parent == null )
					{
						break ;
					}

					t = t.parent ;
				}
			}

			if( m_Window == null )
			{
				return null ;
			}

			return m_Window.GetAlphaMaskMaterial() ;
		}
	}
}
