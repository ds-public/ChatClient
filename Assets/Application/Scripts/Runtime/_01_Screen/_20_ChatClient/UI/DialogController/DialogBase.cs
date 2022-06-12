//#define USE_BLUR

using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using uGUIHelper ;
using Cysharp.Threading.Tasks ;

namespace Template.Screens.ChatClientClasses.UI
{
	/// <summary>
	/// ホーム固有のダイアログの基底クラス
	/// </summary>
	public class DialogBase : ExMonoBehaviour
	{
		private	UIImage			m_View ;
		public	UIImage			  View
		{
			get
			{
				if( m_View == null )
				{
					m_View = GetComponent<UIImage>() ;
				}
				return m_View ;
			}
		}


		[Header( "共通ＵＩ" )]

		[SerializeField]
		protected UIImage		m_Window ;

		[SerializeField]
		protected UIRawImage	m_Background_Blur ;

		[SerializeField]
		protected UIButton		m_CloseButton ;

		//-------------------------------------------------------------------------------------------

		// ダイアログコントローラー
		protected DialogController	m_DialogController ;

		/// <summary>
		/// オーナー(ダイアログコントローラー)を設定する
		/// </summary>
		/// <param name="owner"></param>
		public void SetOwner( DialogController dialogController )
		{
			m_DialogController = dialogController ;
		}

		//-------------------------------------------------------------------------------------------

		// 閉じる前に確認が必要な場合のコールバック
		private Action<Action>	m_OnCloseConfirm ;

		// 閉じられた際に呼び出されるコールバック
		private Action			m_OnClosed ;

		// 外側タッチでダイアログを閉じるかどうか
		private bool			m_OutsideEnabled ;

		// 最初からタッチ状態かの判定に利用するフラグ
		private bool			m_IsPressing ;

		//-------------------------------------------------------------------------------------------

		// 閉じるボタンを押した際に自動でウィンドウのフェードアウトを行わないようにするかどうか
		private bool			m_FadeOutDisabled = false ;

		//-------------------------------------------------------------------------------------------

		internal void Awake()
		{
			m_Background_Blur.SetActive( false ) ;
			m_Window.SetActive( false ) ;
		}

		/// <summary>
		/// ダイアログが開かれているか確認する
		/// </summary>
		public bool IsOpening
		{
			get
			{
				return View.ActiveSelf ;
			}
		}


		// ダイアログを開く際に呼ぶ
		protected async UniTask OpenBase( Action<bool> onOpen, Action onClosed, Action<Action> onCloseConfirm = null, bool outsideEnabled = true, bool fadeOutDisabled = false )
		{
			// 閉じる前に確認が必要な場合のコールバック
			m_OnCloseConfirm	= onCloseConfirm ;

			// 閉じられた際に呼び出されるコールバック
			m_OnClosed			= onClosed ;

			// 外側タッチでダイアログを閉じるかどうか
			m_OutsideEnabled	= outsideEnabled ;

			// 閉じるボタンを押した際に自動でウィンドウのフェードアウトを行わないようにするかどうか
			m_FadeOutDisabled	= fadeOutDisabled ;

			//----------------------------------

			// ウィンドウが非表示状態ならフェードインで表示する
			if( View.ActiveSelf == false )
			{
				await FadeIn( onOpen ) ;
			}

			//----------------------------------

			// 閉じるボタンのコールバックを設定する
			m_CloseButton.SetOnSimpleClick( () =>
			{
				SE.Play( SE.Cancel ) ;

				if( m_OnCloseConfirm != null )
				{
					m_OnCloseConfirm( OnCloseConfirm ) ;
				}
				else
				{
					if( m_FadeOutDisabled == false )
					{
						_ = FadeOut() ;
					}
					else
					{
						m_OnClosed?.Invoke() ;
					}
				}
			} ) ;
		}

		// 閉じる前に確認が必要な際に閉じる事が問題無い場合に呼んでもらう
		private	void OnCloseConfirm()
		{
			// 閉じる
			if( m_FadeOutDisabled == false )
			{
				_ = FadeOut() ;
			}
			else
			{
				m_OnClosed?.Invoke() ;
			}
		}

		/// <summary>
		/// ダイアログを閉じる(外部からの呼び出し専用)
		/// </summary>
		/// <returns></returns>
		public async UniTask Close()
		{
			if( View.ActiveSelf == false )
			{
				// 表示れていない
				return ;
			}

			// コールバックは発生させない
			m_OnClosed = null ;

			await FadeOut() ;
		}

		//-------------------------------------------------------------------------------------------

#if USE_BLUR
		// ブラー用のテクスチャを保存する
		private RenderTexture[] m_BlurTextures ;
#endif
//		private Texture2D m_ScreenImage ;

		/// <summary>
		/// フェードインでダイアログを表示する
		/// </summary>
		/// <returns></returns>
		private async UniTask FadeIn( Action<bool> onOpen )
		{
			// コントローラー全体の開始を行う
			bool isControllerAwaked = m_DialogController.StartupController() ;

			//----------------------------------

			Blocker.On() ;

			//----------------------------------------------------------

			//----------------------------------------------------------

#if USE_BLUR
			// ブラーのフェードインを行う

			int blurWidth  = m_Owner.BlurWidth ;
			int blurHeight = m_Owner.BlurHeight ;
			int blurLevel  = m_Owner.BlurLevel ;

			if( blurWidth  <  270 )
			{
				blurWidth   = 270 ;
			}
			if( blurHeight <  480 )
			{
				blurHeight  = 480 ;
			}
			if( blurLevel  <    1 )
			{
				blurLevel   =   1 ;
			}

			// ブラー背景テクスチャを生成する
			m_BlurTextures = BlurEffect.CreateTextures( blurWidth, blurHeight, blurLevel ) ;

			if( isControllerAwaked == true )
			{
				// 負荷を下げるために３Ｄ空間は非表示にする(ブラーテクスチャを生成した後で)
//				m_Owner.SharedSpace.gameObject.SetActive( false ) ;
			}

			//----------------------------------------------------------

			View.SetActive( true ) ;
			m_Window.SetActive( true ) ;

			m_Background_Blur.SetActive( true ) ;
			m_Background_Blur.Color = Color.white ;

			// ウィンドウが開く前に後に呼ぶ
			onOpen?.Invoke( false ) ;

			float duration = 0.15f ;
			var tween = m_Window.GetTween( "FadeIn" ) ;
			if( tween != null )
			{
				duration = tween.Duration ;
			}

			await WhenAll
			(
				m_Window.PlayTween( "FadeIn" ),
				BlurEffect.FadeIn( m_BlurTextures, m_Background_Blur, duration )
			) ;

			// ウィンドウが開ききった後に呼ぶ
			onOpen?.Invoke( true ) ;

			// デバッグ
//			m_Background_Blur.Texture = m_ScreenImage ;

#else
			View.SetActive( true ) ;

			m_Background_Blur.SetActive( true ) ;
			m_Background_Blur.Color = new Color( 0, 0, 0, 0.5f ) ;

			// ノッチ(セーフエリフ)対策
			m_Background_Blur.SetMarginY( -960, -960 ) ;

			m_Window.SetActive( true ) ;

			// マスクのフェードインを行う
//			m_Owner.FadeInMask() ;

			// ウィンドウが開く前に後に呼ぶ
			onOpen?.Invoke( false ) ;

			await WhenAll
			(
				m_Background_Blur.PlayTween( "FadeIn" ),
				m_Window.PlayTween( "FadeIn" )
			) ;

			// ウィンドウが開ききった後に呼ぶ
			onOpen?.Invoke( true ) ;
#endif
			//----------------------------------------------------------
			// 外側タッチで閉じる処理

			// ウィンドウ部分はタッチしても無効化するためレイキャストを有効にする
			m_Window.RaycastTarget = true ;

			//--------------

			// 最初からタッチしていた場合は一度タッチを解除する必要がありそのためのフラグ
			m_IsPressing = Input.GetMouseButton( 0 );

			m_Background_Blur.IsInteraction = true ;
			m_Background_Blur.RaycastTarget = true ;
			m_Background_Blur.SetOnSimplePress( ( bool state ) =>
			{
				if( m_OutsideEnabled == true )
				{
					if( state == true  )
					{
						if( m_IsPressing == false )
						{
							// 外側を触れたら閉じる扱いとする
							SE.Play( SE.Cancel ) ;

							if( m_OnCloseConfirm != null )
							{
								m_OnCloseConfirm( OnCloseConfirm ) ;
							}
							else
							{
								if( m_FadeOutDisabled == false )
								{
									_ = FadeOut() ;
								}
								else
								{
									m_OnClosed?.Invoke() ;
								}
							}

							m_IsPressing = true ;
						}
					}
					else
					{
						m_IsPressing = false ;
					}
				}
			} ) ;

			//----------------------------------

			Blocker.Off() ;
		}

		/// <summary>
		/// フェードアウトでダイアログを隠蔽する
		/// </summary>
		/// <returns></returns>
		private async UniTask FadeOut()
		{
			if( View.ActiveSelf == false )
			{
				return ;
			}

			//----------------------------------

			// 不要なコールバックを解除する
			m_Background_Blur.SetOnPress( null ) ;
			m_Background_Blur.RaycastTarget = false ;
			m_Background_Blur.IsInteraction = false ;

			//----------------------------------------------------------

			Blocker.On() ;

			if( m_DialogController.GetOpeningCount() == 1 )
			{
				// 最後のダイアログを閉じようとしている

				// 負荷を下げるために非表示にしていた３Ｄ空間を表示する
//				m_Owner.SharedSpace.gameObject.SetActive( true ) ;
			}

			//----------------------------------------------------------

			//----------------------------------------------------------
#if USE_BLUR
			// ブラーのフェードアウトを行う

			float duration = 0.15f ;
			var tween = m_Window.GetTween( "FadeOut" ) ;
			if( tween != null )
			{
				duration = tween.Duration ;
			}

			await WhenAll
			(
				m_Window.PlayTweenAndHide( "FadeOut" ),
				BlurEffect.FadeOut( m_BlurTextures, m_Background_Blur, duration )
			) ;

			m_Background_Blur.Color = ExColor.Transparency ;
			m_Background_Blur.SetActive( false ) ;

			onClosed?.Invoke() ;

			View.SetActive( false ) ;

			//----------------------------------------------------------

			// プラー背景テクスチャを破棄する
			m_BlurTextures = null ;

			//----------------------------------------------------------
#else
			// マスクのフェードアウトを行う

//			m_Owner.FadeOutMask() ;

			await WhenAll
			(
				m_Background_Blur.PlayTweenAndHide( "FadeOut" ),
				m_Window.PlayTweenAndHide( "FadeOut" )
			) ;

			m_Background_Blur.SetActive( false ) ;

			m_OnClosed?.Invoke() ;

			View.SetActive( false ) ;
#endif
			//----------------------------------------------------------

			m_OnClosed = null ;

			Blocker.Off() ;

			//----------------------------------

			// コントローラー全体の終了を行う
			m_DialogController.CleanupController() ;
		}
	}
}
