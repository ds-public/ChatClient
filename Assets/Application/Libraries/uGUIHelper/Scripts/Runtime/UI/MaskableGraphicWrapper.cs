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
	/// 拡張ＵＩのベースとなる基底クラス
	/// </summary>
	[ RequireComponent( typeof( RectTransform ) ) ]
	[ RequireComponent( typeof( CanvasRenderer ) ) ]
	public class MaskableGraphicWrapper : MaskableGraphic
	{
		[SerializeField][HideInInspector]
		private   RectTransform	m_RectTransform ;
		protected RectTransform	  RectTransform
		{
			get
			{
				if( m_RectTransform != null )
				{
					return m_RectTransform ;
				}
				m_RectTransform = GetComponent<RectTransform>() ;
				return m_RectTransform ;
			}
		}


		protected Vector2 Size
		{
			get
			{
				RectTransform rectTransform = RectTransform ;
				if( rectTransform == null )
				{
					return Vector2.zero ;
				}
				Vector2 size = rectTransform.sizeDelta ;

				// Ｘのチェック
				if( RectTransform.anchorMin.x != RectTransform.anchorMax.x )
				{
					// Stretch なので Stretch じゃなくなるまで親をたどる
					List<RectTransform> list = new List<RectTransform>()
					{
						RectTransform
					} ;

					Transform parent = RectTransform.parent ;
					RectTransform target ;
					float delta = 0 ;
					for( int i  = 0 ; i <  64 ; i ++ )
					{
						if( parent != null )
						{
							target = parent.GetComponent<RectTransform>() ;
							if( target != null )
							{
								if( target.anchorMin.x == target.anchorMax.x  )
								{
									// 発見
									delta = target.sizeDelta.x ;
									break ;
								}
								else
								{
									list.Add( target ) ;
								}
							}
							parent = parent.parent ;
						}
						else
						{
							// 検索終了
							break ;
						}
					}

					if( delta >   0 )
					{
						// マージン分を引く
						for( int i  = list.Count - 1 ; i >= 0 ; i -- )
						{
							delta *= ( list[ i ].anchorMax.x - list[ i ].anchorMin.x ) ;
							delta +=   list[ i ].sizeDelta.x ;
						}

						size.x = delta ;
					}
				}

				// Ｙのチェック
				if( RectTransform.anchorMin.y != RectTransform.anchorMax.y )
				{
					// Stretch なので Stretch じゃなくなるまで親をたどる
					List<RectTransform> list = new List<RectTransform>()
					{
						RectTransform
					} ;

					Transform parent = RectTransform.parent ;
					RectTransform target ;
					float delta = 0 ;
					for( int i  = 0 ; i <  64 ; i ++ )
					{
						if( parent != null )
						{
							target = parent.GetComponent<RectTransform>() ;
							if( target != null )
							{
								if( target.anchorMin.y == target.anchorMax.y )
								{
									// 発見
									delta = target.sizeDelta.y ;
									break ;
								}
								else
								{
									list.Add( target ) ;
								}
							}
							parent = parent.parent ;
						}
						else
						{
							// 検索終了
							break ;
						}
					}

					if( delta >   0 )
					{
						// マージン分を引く
						for( int i  = list.Count - 1 ; i >= 0 ; i -- )
						{
							delta *= ( list[ i ].anchorMax.y - list[ i ].anchorMin.y ) ;
							delta +=   list[ i ].sizeDelta.y ;
						}

						size.y = delta ;
					}
				}

				return size ;
			}
		}


		[SerializeField][HideInInspector]
		private   CanvasRenderer	m_CanvasRenderer ;
		protected CanvasRenderer	  CanvasRenderer
		{
			get
			{
				if( m_CanvasRenderer != null )
				{
					return m_CanvasRenderer ;
				}
				m_CanvasRenderer = GetComponent<CanvasRenderer>() ;
				return m_CanvasRenderer ;
			}
		}

		public Color Color
		{
			get
			{
				return base.color ;
			}
			set
			{
				if( base.color.r != value.r || base.color.g != value.g || base.color.b != value.b || base.color.a != value.a )
				{
					base.color = value ;
					CanvasRenderer.SetColor( value ) ;
				}
			}
		}

		// マテリアル更新
		protected override void UpdateMaterial()
		{
			base.UpdateMaterial() ;

			CanvasRenderer.SetColor( base.color ) ;
		}
	}
}
