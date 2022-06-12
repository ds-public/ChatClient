using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace uGUIHelper
{
	/// <summary>
	/// GraphicRaycaster 拡張クラス(オフスクリーンに対応版)
	/// </summary>
	public class GraphicRaycasterWrapper : GraphicRaycaster
	{
		/// <summary>
		/// BlockingMask(なぜか Public なアクセサが存在しない)
		/// </summary>
		public LayerMask BlockingMask
		{
			get
			{
				return m_BlockingMask ;
			}
			set
			{
				m_BlockingMask = value ;
			}
		}

		[SerializeField][HideInInspector]
		private RectTransform m_OffScreenImage = null ;

		/// <summary>
		/// オフスクリーンのレクトトランスフォーム
		/// </summary>
		public  RectTransform  OffScreenImage
		{
			get
			{
				return m_OffScreenImage ;
			}
			set
			{
				if( m_OffScreenImage != value )
				{
					m_OffScreenImage  = value ;
				}
			}
		}


		//------------------------------------------------------------------------------------------

		/// <summary>
		/// Raycast のローバーライド
		/// </summary>
		/// <param name="eventData"></param>
		/// <param name="resultAppendList"></param>
		public override void Raycast( PointerEventData eventData, List<RaycastResult> resultAppendList )
		{
			if( m_OffScreenImage != null )
			{
				TransformPosition( eventData ) ;
			}

			base.Raycast( eventData, resultAppendList ) ;
		}

		// 座標を変換する
		private void TransformPosition( PointerEventData eventData )
		{
			// 実際に表示するオンスクリーン上でのオフスクリーンの表示情報を取得する
			Vector2 canvasSize = Vector2.zero ;

			Rect r = RectInCanvas( m_OffScreenImage, ref canvasSize ) ;

			if( r.width == 0 || r.height == 0 || canvasSize.x == 0 || canvasSize.y == 0 )
			{
				return ;
			}

			// 座標をスクリーン系に変換する

			float sx = r.x * Screen.width     / canvasSize.x ;
			float sy = r.y * Screen.height    / canvasSize.y ;

			float sw = r.width  * Screen.width  / canvasSize.x ;
			float sh = r.height * Screen.height / canvasSize.y ;

			// 座標をレンダーテクスチャー系に変換する

			RectTransform rt = GetComponent<RectTransform>() ;
			if( rt == null )
			{
				return ;
			}

			float rw = rt.sizeDelta.x ;
			float rh = rt.sizeDelta.y ;

			Vector2 p ;

			//--------------------------------------

			p = eventData.position ;

			p.x = rw * ( p.x - sx ) / sw ;
			p.y = rh * ( p.y - sy ) / sh ;

			eventData.position = p ;

			//-----

			p = eventData.pressPosition ;

			p.x = rw * ( p.x - sx ) / sw ;
			p.y = rh * ( p.y - sy ) / sh ;

			eventData.pressPosition = p ;

			//-----

			p = eventData.scrollDelta ;

			p.x = rw * ( p.x - sx ) / sw ;
			p.y = rh * ( p.y - sy ) / sh ;

			eventData.scrollDelta = p ;
		}

		/// <summary>
		/// キャンバス上での座標を取得する
		/// </summary>
		public Rect RectInCanvas( RectTransform offScreen, ref Vector2 rCanvasSize )
		{
			// 親サイズ
			Vector2 ps = Vector2.zero ;

			List<RectTransform> rts = new List<RectTransform>() ;
			int i, l ;

			Transform t = offScreen.transform ;

			// まずはキャンバスを検出するまでリストに格納する
			for( i  =  0 ; i <  64 ; i ++ )
			{
				if( t != null )
				{
					if( t.GetComponent<Canvas>() == null )
					{
						RectTransform rt = t.GetComponent<RectTransform>() ;
						if( rt != null )
						{
							rts.Add( rt ) ;
						}
					}
					else
					{
						RectTransform rt = t.GetComponent<RectTransform>() ;
						if( rt != null )
						{
							ps = rt.sizeDelta ;
						}
						break ;	// 終了
					}
				}
				else
				{
					break ;	// 終了
				}

				t = t.parent ;
			}

			if( rts.Count <= 0 || ps.x == 0 || ps.y == 0 )
			{
				return new Rect() ;	// 異常
			}

			float pw = ps.x ;
			float ph = ps.y ;

			float px  = pw * 0.5f ;
			float px0 = 0 ;
			float px1 = pw ;

			float py  = ph * 0.5f ;
			float py0 = 0 ;
			float py1 = ph ;


			l = rts.Count ;
	//		Debug.LogWarning( "階層の数:" + l ) ;
			for( i  = ( l - 1 ) ; i >= 0 ; i -- )
			{
				RectTransform rt = rts[ i ] ;

	//			Debug.Log( rt.name ) ;

				// X

				// 自身の横幅(次の親の横幅)
				if( rt.anchorMin.x != rt.anchorMax.x )
				{
					px0 += ( pw * rt.anchorMin.x ) ;	// 親の最小
					px1 += ( pw * rt.anchorMax.x ) ;	// 親の最大
						
					// マージンの補正をかける
					px0 -= ( ( rt.sizeDelta.x * rt.pivot.x ) - rt.anchoredPosition.x ) ;
					px1 += ( ( rt.sizeDelta.x * rt.pivot.x ) + rt.anchoredPosition.x ) ;

					pw = px1 - px0 ;

	//				Debug.Log( "親のX:" + px0 + " ～ " + px1 + " / " + pw ) ;

					// 中心位置
					px = px0 + ( pw * rt.pivot.x ) /* - ( ( rt.sizeDelta.x * rt.pivot.x ) - rt.anchoredPosition.x ) */ ;
				}
				else
				{
					// 中心位置
	//				Debug.Log( "親のX:" + px0 + " ～ " + px1 ) ;
					px = px0 + ( pw * rt.anchorMin.x ) + rt.anchoredPosition.x ;

					pw = rt.sizeDelta.x ;
				}

				// 親の範囲更新
				px0 = px - ( pw * rt.pivot.x ) ;
				px1 = px0 + pw ;

	//			Debug.Log( "x:" + px ) ;

				// Y
				// 自身の横幅(次の親の横幅)
				if( rt.anchorMin.y != rt.anchorMax.y )
				{
					py0 += ( ph * rt.anchorMin.y ) ;	// 親の最小
					py1 += ( ph * rt.anchorMax.y ) ;	// 親の最大
						
					// マージンの補正をかける
					py0 -= ( ( rt.sizeDelta.y * rt.pivot.y ) - rt.anchoredPosition.y ) ;
					py1 += ( ( rt.sizeDelta.y * rt.pivot.y ) + rt.anchoredPosition.y ) ;

					ph = py1 - py0 ;

	//				Debug.Log( "親のY:" + py0 + " ～ " + py1 + " / " + ph ) ;

					// 中心位置
					py = py0 + ( ph * rt.pivot.y ) /* - ( ( rt.sizeDelta.x * rt.pivot.x ) - rt.anchoredPosition.x ) */ ;
				}
				else
				{
	//				Debug.Log( "親のY:" + py0 + " ～ " + py1 + " / " + py ) ;

					// 中心位置
					py = py0 + ( ph * rt.anchorMin.y ) + rt.anchoredPosition.y ;

					ph = rt.sizeDelta.y ;
				}

				// 親の範囲更新
				py0 = py - ( ph * rt.pivot.y ) ;
				py1 = py0 + ph ;

	//			Debug.Log( "y:" + py ) ;
			}
		
			px -= ( pw * offScreen.pivot.x ) ;
			py -= ( ph * offScreen.pivot.y ) ;

			rCanvasSize.x = ps.x ;
			rCanvasSize.y = ps.y ;

			return new Rect( new Vector2( px, py ), new Vector2( pw, ph ) ) ;
		}
	}
}
