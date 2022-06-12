using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;
using UnityEngine.XR ;

using uGUIHelper ;

namespace Template
{
	/// <summary>
	/// 入力に対する反応演出のクラス Version 2021/10/11 0
	/// </summary>
	public class Ripple : MonoBehaviour
	{
		// シングルトンインスタンス
		private static Ripple m_Instance ;

		/// <summary>
		/// インスタンス
		/// </summary>
		public  static Ripple   Instance
		{
			get
			{
				return m_Instance ;
			}
		}

		//-------------------------------------

		[SerializeField]
		protected UICanvas	m_Canvas ;

		/// <summary>
		/// ＶＲ対応用にキャンバスを取得できるようにする
		/// </summary>
		public UICanvas Canvas
		{
			get
			{
				return m_Canvas ;
			}
		}

		/// <summary>
		/// キャンバスの仮想解像度を設定する
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns></returns>
		public static bool SetCanvasResolution( float width, float height )
		{
			if( m_Instance == null || m_Instance.m_Canvas == null )
			{
				return false ;
			}

			m_Instance.m_Canvas.SetResolution( width, height, true ) ;

			return true ;
		}

		//-------------------------------------

		[SerializeField]
		protected UIView	m_Screen ;

		[SerializeField]
		protected UIView	m_PositiveButtonEffect ;

		[SerializeField]
		protected UIView	m_NegativeButtonEffect ;

		[SerializeField]
		protected UICircle	m_Circle ;

		[SerializeField]
		protected UICircle	m_Star ;

		[SerializeField]
		protected UILine	m_Line ;

		//-----------------------------------------------------------

		/// <summary>
		/// ボタンエフェクトの種類
		/// </summary>
		public enum ButtonEffectTypes
		{
			None,
			Positive,
			Negative,
		}

		//-----------------------------------------------------------

		public UICanvas TargetCanvas
		{
			get
			{
				return m_Canvas ;
			}
		}
		
		private bool m_IsOn = true ;

		private bool m_Focus = true ;
		private int m_FocusWait = 1 ;

		//---------------------------------------------------------------------------

		internal void Awake()
		{
			m_Instance = this ;

			// そのシーンをシーンが切り替わっても永続的に残すようにする場合、そのシーン側で DontDestroyOnLoad を実行してはならない。
			// DontDestroyOnLoad は、呼び出し側のシーンで、そのシーンに対して実行すること。

			if( m_PositiveButtonEffect != null )
			{
				m_PositiveButtonEffect.SetActive( false ) ;
			}

			if( m_NegativeButtonEffect != null )
			{
				m_NegativeButtonEffect.SetActive( false ) ;
			}

			if( m_Circle != null )
			{
				m_Circle.SetActive( false ) ;
			}

			if( m_Star != null )
			{
				m_Star.SetActive( false ) ;
			}

			if( m_Line != null )
			{
				m_Line.SetActive( true ) ;
				m_Line.TrailEnabled = true ;	// トレイルモードを有効にする
			}

			//----------------------------------------------------------

			// キャンバスの解像度を設定する
			float width  = 1080 ;
			float height = 1920 ;

			Settings settings =	ApplicationManager.LoadSettings() ;
			if( settings != null )
			{
				width  = settings.BasicWidth ;
				height = settings.BasicHeight ;
			}

			SetCanvasResolution( width, height ) ;
		}
			
		internal void Update()
		{
			if( m_Focus == false )
			{
				return ;
			}

			if( m_FocusWait >  0 )
			{
				// フォーカスを得た直後は取得できるマウスの座標がおかしいので１フレームだけ待つ
				m_FocusWait -- ;
				return ;
			}

			//----------------------------------

			if( XRSettings.enabled == false )
			{
				m_Canvas.SetActive( true ) ;
				ProcessMove() ;
			}
			else
			{
				m_Canvas.SetActive( false ) ;
			}
		}

		private int m_TouchState ;
		private Vector2 m_TouchPosition ;

		private float m_Interval ;

		private void ProcessMove()
		{
			( int touchState, Vector2 touchPosition ) = m_Screen.GetSinglePointer() ;

			if( touchState == 1 && m_IsOn == true )
			{
				if( m_TouchState == 0 )
				{
					// 押された
					//-------------------------

					m_TouchState = touchState ;
					m_TouchPosition = touchPosition ;

					UICircle circle = m_Circle.Duplicate<UICircle>() ;

					// デリゲートのポインタは保持されないので複製したオブジェクトごとに設定してやる必要がある
					circle.GetTween( "FadeOut" ).SetOnFinished( OnFinish ) ;
					circle.SetPosition( touchPosition ) ;	// 座標設定
					circle.SetActive( true ) ;		// 有効化

					//-------------------------

					m_Interval = Time.realtimeSinceStartup ;

					UICircle star = m_Star.Duplicate<UICircle>() ;

					// 方向ランダム指定
					int a = Random.Range(   0, 360 ) ;
					float r = 2.0f * Mathf.PI * ( float )a / 360.0f ;
					float d = Random.Range( 8.0f, 16.0f ) ;
					Vector3 v = Vector3.zero ;
					v.x = Mathf.Cos( r ) * d ;
					v.y = Mathf.Sin( r ) * d ;

					star.GetTween( "FadeOut" ).PositionTo = v ;

					// デリゲートのポインタは保持されないので複製したオブジェクトごとに設定してやる必要がある
					star.GetTween( "FadeOut" ).SetOnFinished( OnFinish ) ;
					star.SetPosition( touchPosition ) ;	// 座標設定
					star.SetActive( true ) ;		// 有効化

					//-------------------------

					// 新規で押された場合は表示中の軌跡は全てクリアする
					m_Line.ClearTrailPosition() ;
					m_Line.AddTrailPosition( touchPosition ) ;
				}
				else
				if( m_TouchState == touchState )
				{
					// 移動した
					//-------------------------

					if( m_TouchPosition != touchPosition )
					{
						// きちんと移動していたら処理する
						m_TouchPosition = touchPosition ;
						if( ( Time.realtimeSinceStartup - m_Interval ) >  0.1f )
						{
							m_Interval = Time.realtimeSinceStartup ;

							UICircle star = m_Star.Duplicate<UICircle>() ;

							// 方向ランダム指定
							int a = Random.Range(   0, 360 ) ;
							float r = 2.0f * Mathf.PI * ( float )a / 360.0f ;
							float d = Random.Range( 8.0f, 16.0f ) ;
							Vector3 v = Vector3.zero ;
							v.x = Mathf.Cos( r ) * d ;
							v.y = Mathf.Sin( r ) * d ;

							star.GetTween( "FadeOut" ).PositionTo = v ;

							// デリゲートのポインタは保持されないので複製したオブジェクトごとに設定してやる必要がある
							star.GetTween( "FadeOut" ).SetOnFinished( OnFinish ) ;
							star.SetPosition( touchPosition ) ;	// 座標設定
							star.SetActive( true ) ;       // 有効化
						}

						//-------------------------

						m_Line.AddTrailPosition( touchPosition ) ;
					}
				}
			}
			else
			{
				// 離された
				m_TouchState = 0 ;
			}
		}

		private void OnFinish( string identity, UITween tween )
		{
			// 終了したら自身を破棄する
			Destroy( tween.gameObject ) ;
		}

		/// <summary>
		/// 有効にする
		/// </summary>
		public static void On()
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.m_IsOn = true ;
		}

		/// <summary>
		/// 無効にする
		/// </summary>
		public static void Off()
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.m_IsOn = false ;
		}

		internal void OnApplicationFocus( bool state )
		{
			m_Focus = state ;

			if( state == true )
			{
				m_TouchState = 0 ;
				m_Line.ClearTrailPosition() ;

				// フォーカスを得た直後は取得できるマウスの座標がおかしいので１フレームだけ待つ
				m_FocusWait = 1 ;
			}
		}

		//-------------------------------------------------------------------------------------------
		// ボタンエフェクトを再生する


		/// <summary>
		/// ボタンエフェクトを再生する
		/// </summary>
		/// <param name="isPositive"></param>
		/// <param name="position"></param>
		/// <param name="size"></param>
		public static void PlayButtonEffect( ButtonEffectTypes buttonEffectType, UIView view )
		{
			if( m_Instance == null )
			{
				return ;
			}

			Vector2 position	= view.PositionInCanvas ;
			Vector2 size		= view.Size ;

			Vector2 pivot		= view.Pivot ;

			position.x += ( size.x * ( 0.5f - pivot.x ) ) ; 
			position.y += ( size.y * ( 0.5f - pivot.y ) ) ; 

			m_Instance.PlayButtonEffect_Private( buttonEffectType, position, size ) ;
		}

		/// <summary>
		/// ボタンエフェクトを再生する
		/// </summary>
		/// <param name="isPositive"></param>
		/// <param name="position"></param>
		/// <param name="size"></param>
		public static void PlayButtonEffect( ButtonEffectTypes buttonEffectType, Vector2 position, Vector2 size )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.PlayButtonEffect_Private( buttonEffectType, position, size ) ;
		}

		private void PlayButtonEffect_Private( ButtonEffectTypes buttonEffectType, Vector2 position, Vector2 size )
		{
			UIView target ;
			if( buttonEffectType == ButtonEffectTypes.Positive )
			{
				target = m_PositiveButtonEffect ;
			}
			else
			if( buttonEffectType == ButtonEffectTypes.Negative )
			{
				target = m_NegativeButtonEffect ;
			}
			else
			{
				return ;
			}

			target.StopAnimator() ;

			target.SetPosition( position ) ;
			target.SetSize( size ) ;

			target.SetActive( true ) ;
			target.PlayAnimator( "Play", onFinished:( bool state ) =>
			{
				target.SetActive( false ) ;
			} ) ;
		}
	}
}
