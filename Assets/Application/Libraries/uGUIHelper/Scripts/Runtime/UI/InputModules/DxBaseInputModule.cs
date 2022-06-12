using System ;
using System.Collections.Generic ;

using UnityEngine ;
using UnityEngine.EventSystems ;

namespace uGUIHelper
{
	[RequireComponent(typeof(EventSystem))]
	public abstract class DxBaseInputModule : BaseInputModule
	{
		// マウスオーバーの処理
		new protected void HandlePointerExitAndEnter( PointerEventData currentPointerData, GameObject newEnterTarget )
		{
			if( newEnterTarget == null || currentPointerData.pointerEnter == null )
			{
				// マウスオーバーの対象が１つも無くなった→１フレーム前の対象に「出る」を送る
				var hoveredCount = currentPointerData.hovered.Count ;
				for( var i  = 0 ; i <  hoveredCount ; ++ i )
				{
					if( currentPointerData.hovered[ i ] != null && currentPointerData.hovered[ i ].activeInHierarchy == false )
					{
						// 非アクティブだと SendMessage() は機能しないため直接コンポーネントを取得して送信する

						var interaction = currentPointerData.hovered[ i ].GetComponent<UIInteraction>() ;
						if( interaction != null )
						{
							interaction.OnPointerExit( currentPointerData ) ;
						}

						var interactionForScrollView = currentPointerData.hovered[ i ].GetComponent<UIInteractionForScrollView>() ;
						if( interactionForScrollView != null )
						{
							interactionForScrollView.OnPointerExit( currentPointerData ) ;
						}
					}
					else
					{
						ExecuteEvents.Execute( currentPointerData.hovered[ i ], currentPointerData, ExecuteEvents.pointerExitHandler ) ;
					}
				}

				currentPointerData.hovered.Clear() ;

				if( newEnterTarget == null )
				{
					if( currentPointerData.pointerEnter != null )
					{
						currentPointerData.pointerEnter = null ;
					}
					return ;
				}
			}

			// マウスオーバー状態だが以前の対象と同じか確認する
			if( currentPointerData.pointerEnter == newEnterTarget && newEnterTarget != null )
			{
				// ※以前と同じ対象でもリアルタイムで座標を取りたいのでメッセージを送る
				if( currentPointerData.IsPointerMoving() == true )
				{
					var hoveredCount = currentPointerData.hovered.Count ;
					for( var i  = 0 ; i <  hoveredCount ; ++ i )
					{
						if( currentPointerData.hovered[ i ] != null )
						{
							ExecuteEvents.Execute( currentPointerData.hovered[ i ], currentPointerData, ExecuteEvents.pointerEnterHandler ) ;
						}
					}
				}

				return ;
			}

			GameObject commonRoot = FindCommonRoot( currentPointerData.pointerEnter, newEnterTarget ) ;

			//--------------------------------------------------------------------------
			// 以下は対象が変化した際にくる
			
			// 古い対象が存在するか確認する
			if( currentPointerData.pointerEnter != null )
			{
				// 古い対象が存在する
				Transform t = currentPointerData.pointerEnter.transform ;

				while( t != null )
				{
					if( commonRoot != null && commonRoot.transform == t )
					{
						break ;
					}

					// 外に出た事を通知する
					if( t.gameObject.activeInHierarchy == false )
					{
						// 非アクティブだと SendMessage() は機能しないため直接コンポーネントを取得して送信する

						var interaction = t.gameObject.GetComponent<UIInteraction>() ;
						if( interaction != null )
						{
							interaction.OnPointerExit( currentPointerData ) ;
						}

						var interactionForScrollView = t.gameObject.GetComponent<UIInteractionForScrollView>() ;
						if( interactionForScrollView != null )
						{
							interactionForScrollView.OnPointerExit( currentPointerData ) ;
						}
					}
					else
					{
						ExecuteEvents.Execute( t.gameObject, currentPointerData, ExecuteEvents.pointerExitHandler ) ;
					}
					currentPointerData.hovered.Remove( t.gameObject ) ;
					t = t.parent ;
				}
			}

			//----------------------------------
			// 以前と状況が異なるため現在の箇所に入っている
			currentPointerData.pointerEnter = newEnterTarget ;
			if( newEnterTarget != null )
			{
				Transform t = newEnterTarget.transform ;

				while( t != null && t.gameObject != commonRoot )
				{
					// 新しい対象に入った事を通知する
					ExecuteEvents.Execute( t.gameObject, currentPointerData, ExecuteEvents.pointerEnterHandler ) ;
					currentPointerData.hovered.Add( t.gameObject ) ;
					t = t.parent ;
				}
			}
		}
	}
}
