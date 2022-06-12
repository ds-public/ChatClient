using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using Cysharp.Threading.Tasks ;

using UnityEngine ;

using uGUIHelper ;

namespace Template
{
	/// <summary>
	/// スクリーンのサイズ調整クラス Version 2022/05/04 0
	/// </summary>
	public class ScreenSizeFitter : ExMonoBehaviour
	{
		// セーフエリアの処理を有効にするか
		[SerializeField]
		protected bool		m_SafeAreaEnabled = true ;

		//---------------------------------------------------------------------------

		// 直親のキャンバス
		private Canvas	m_Canvas ;
		private UIView	m_Screen ;

		private RectTransform m_CanvasRectTransform ;

		private float	m_BasicWidth ;
		private float	m_BasicHeight ;

		private float	m_LimitWidth ;
		private float	m_LimitHeight ;

		//-----------------------------------

		private int		m_ScreenWidth ;
		private int		m_ScreenHeight ;

		private float	m_CanvasWidth ;
		private float	m_CanvasHeight ;

		//-------------------------------------------------------------------------------------------

		internal void Awake()
		{
			// 直親のキャンバスを探す

			Transform t = transform.parent ;
			while( t != null )
			{
				m_Canvas = t.GetComponent<Canvas>() ;
				if( m_Canvas != null )
				{
					break ;	// キャンバスを発見した
				}

				t = t.parent ;
			}

			if( m_Canvas != null )
			{
				m_CanvasRectTransform = m_Canvas.GetComponent<RectTransform>() ;
			}

			m_Screen = GetComponent<UIView>() ;

			//----------------------------------

			// キャンバスの解像度を設定する
			m_BasicWidth  = 1080 ;
			m_BasicHeight = 1920 ;

			m_LimitWidth  = 1440 ;
			m_LimitHeight = 2560 ;

			Settings settings =	ApplicationManager.LoadSettings() ;
			if( settings != null )
			{
				m_BasicWidth  = settings.BasicWidth ;
				m_BasicHeight = settings.BasicHeight ;

				m_LimitWidth  = settings.LimitWidth ;
				m_LimitHeight = settings.LimitHeight ;
			}
		}

		internal void Start()
		{
			if( m_CanvasRectTransform == null || m_Screen == null )
			{
				return  ;
			}

			//----------------------------------

			float canvasWidth  = m_CanvasRectTransform.sizeDelta.x ;
			float canvasHeight = m_CanvasRectTransform.sizeDelta.y ;

			Refresh() ;

			//---------------------------------
			// 現在の値を保存する

			m_ScreenWidth  = Screen.width ;
			m_ScreenHeight = Screen.height ;

			m_CanvasWidth  = canvasWidth ;
			m_CanvasHeight = canvasHeight ;
		}

		// Canvas の deltaSize の更新の後の処理が好ましいので LateUpdate() で処理する
		internal void LateUpdate()
		{
			if( m_CanvasRectTransform == null || m_Screen == null )
			{
				return  ;
			}

			//----------------------------------

			float canvasWidth  = m_CanvasRectTransform.sizeDelta.x ;
			float canvasHeight = m_CanvasRectTransform.sizeDelta.y ;

			// 実解像度が変化したら更新する
			if( m_ScreenWidth != Screen.width || m_ScreenHeight != Screen.height || canvasWidth != m_CanvasWidth || canvasHeight != m_CanvasHeight )
			{
				// 更新
				Refresh() ;

				//---------------------------------
				// 現在の値を保存する

				m_ScreenWidth  = Screen.width ;
				m_ScreenHeight = Screen.height ;

				m_CanvasWidth  = canvasWidth ;
				m_CanvasHeight = canvasHeight ;
			}
		}

		// 表示更新する
		private bool Refresh()
		{
			m_Screen.SetAnchorToCenter() ;

			float canvasWidth  = m_CanvasRectTransform.sizeDelta.x ;
			float canvasHeight = m_CanvasRectTransform.sizeDelta.y ;

			float y ;
			float width ;
			float height ;


			if( ( canvasHeight / canvasWidth ) >  ( m_BasicHeight / m_BasicWidth ) )
			{
				// 縦長
				float h = canvasHeight ;

				if( h >  m_LimitHeight )
				{
					h  = m_LimitHeight ;
				}

				y		= 0 ;
				width	= m_BasicWidth ;
				height	= h ;

				//---------------------------------
			}
			else
			if( ( canvasHeight / canvasWidth ) <  ( m_BasicHeight / m_BasicWidth ) )
			{
				// 横長
				float w = canvasWidth ;

				if( w >  m_LimitWidth )
				{
					w  = m_LimitWidth ;
				}

				y		= 0 ;
				width	= w ;
				height	= m_BasicHeight ;
			}
			else
			{
				// 丁度
				y		= 0 ;
				width	= m_BasicWidth ;
				height	= m_BasicHeight ;
			}

			//----------------------------------------------------------

			if( m_SafeAreaEnabled == true )
			{
				// セーフエリアの外にはみ出た部分を削る

				var safeArea = Screen.safeArea ;

				float yMin = canvasHeight * ( float )safeArea.yMin / ( float )Screen.height ;
				float yMax = canvasHeight * ( float )safeArea.yMax / ( float )Screen.height ;

				// 画面上部のマージン幅
				float marginUpper = yMin ;
//				marginUpper = 128 ;	// デバッグ

				// 画面下部のマージン幅
				float marginLower = canvasHeight - yMax ;
//				marginLower = 128 ;	// デバッグ

				// 現在のマージン幅
				float margin = ( canvasHeight - height ) * 0.5f ;

				if( marginUpper <= margin )
				{
					// 画面上部のマージンは現在のままで良い
					marginUpper  = margin ; 
				}

				if( marginLower <= margin )
				{
					// 画面下部のマージンは現在のままで良い
					marginLower  = margin ;
				}

				// 画面上部と画面下部でマージン量が異なる場合に縦位置の補正をかける
				// 画面上部の方が太ければ下へ・画面下部の方が太ければ上へ
				y = ( marginLower - marginUpper ) * 0.5f ;

				// 画面の縦幅をセーフエリアを反映したものに変更
				height = canvasHeight - ( marginUpper + marginLower ) ;
			}

			//----------------------------------------------------------

			m_Screen.SetPositionY( y ) ;
			m_Screen.SetSize( width, height ) ;

			return true ;
		}
	}
}
