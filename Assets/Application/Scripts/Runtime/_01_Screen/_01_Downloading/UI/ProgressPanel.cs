using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using UnityEngine ;

using uGUIHelper ;
using Cysharp.Threading.Tasks ;

using AssetBundleHelper ;

namespace Template.Screens.DownloadingClasses.UI
{
	/// <summary>
	/// ムービー表示パネル
	/// </summary>
	public partial class ProgressPanel : ExMonoBehaviour
	{
		private UIView				m_View ;

		/// <summary>
		/// パネルのビューを取得する
		/// </summary>
		public	UIView				  View
		{
			get
			{
				if( m_View == null )
				{
					m_View  = GetComponent<UIView>() ;
				}
				return m_View ;
			}
		}

		//-----------------------------------------------------------

		[Header( "プログレスバー" )]

		[SerializeField]
		protected	UIProgressbar	m_Gauge ;

		[SerializeField]
		protected	UITextMesh		m_State ;

		[SerializeField]
		protected	UINumberMesh	m_Ratio ;

		//-----------------------------------------------------------

		[Header( "開発者向け情報" )]

		[SerializeField]
		protected	UIView			m_DevelopmentMode ;

		[SerializeField]
		protected	UIImage			m_Animation ;

		[SerializeField]
		protected	UITextMesh		m_StateMessage ;

		[SerializeField]
		protected	UINumberMesh	m_FileNow ;

		[SerializeField]
		protected	UINumberMesh	m_FileMax ;

		[SerializeField]
		protected	UINumberMesh	m_SizeNow ;

		[SerializeField]
		protected	UINumberMesh	m_SizeMax ;

		[SerializeField]
		protected	UINumberMesh	m_ParallelNow ;

		[SerializeField]
		protected	UINumberMesh	m_ParallelMax ;

		[SerializeField]
		protected	UITextMesh		m_Protocol ;

		[SerializeField]
		protected	UINumberMesh	m_D_Rate ;

		[SerializeField]
		protected	UINumberMesh	m_W_Rate ;

		[SerializeField]
		protected	UITextMesh		m_Time ;

		//-------------------------------------------------------------------------------------------
		// 作業変数

		private float	m_Rate_Time ;

		private long	m_DownloadingRate_Size ;
		private float	m_DownloadingRate_Total ;
		private int		m_DownloadingRate_Count ;

		private int		m_WritingRate_Size ;
		private float	m_WritingRate_Total ;
		private int		m_WritingRate_Count ;

		private float	m_DownloadingTime_Base ;
#if UNITY_EDITOR
		private bool	m_DownloadingDisplay ;
#endif
		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 計測情報を初期化する
		/// </summary>
		public void Prepare()
		{
			// プログレスの状態を初期化する
			m_Gauge.Value = 0 ;
			SetStateToRatio( m_Gauge.Value ) ;

			//----------------------------------------------------------
			// 以下は開発者向けの情報

			m_DevelopmentMode.SetActive( Define.DevelopmentMode ) ;

			//---------------------------------

			m_Animation.SetActive( true ) ;

			m_StateMessage.Text = "Now Downloading ..." ;

			//--------------

			m_FileNow.Value = 0 ;
			m_FileMax.Value = 0 ;

			m_SizeNow.Value = 0 ;
			m_SizeMax.Value = 0 ;

			m_ParallelNow.Value = 0 ;
			m_ParallelMax.Value = 0 ;

			m_Protocol.Text = "???" ;

			m_Rate_Time = Time.realtimeSinceStartup ;

			m_D_Rate.Value = 0 ;

			m_DownloadingRate_Size	= 0 ;
			m_DownloadingRate_Total	= 0 ;
			m_DownloadingRate_Count	= 0 ;

			m_W_Rate.Value = 0 ;

			m_WritingRate_Size	= 0 ;
			m_WritingRate_Total	= 0 ;
			m_WritingRate_Count	= 0 ;

			m_DownloadingTime_Base = Time.realtimeSinceStartup ;
#if UNITY_EDITOR
			m_DownloadingDisplay = false ;
#endif
			m_Time.Text = string.Empty ;
		}


		/// <summary>
		/// プログレスの状態を設定する
		/// </summary>
		/// <param name="downloadedSize"></param>
		/// <param name="writtenSize"></param>
		/// <param name="totalSize"></param>
		/// <param name="storedFile"></param>
		/// <param name="totalFile"></param>
		/// <param name="targets"></param>
		/// <param name="nowParallel"></param>
		/// <param name="maxParallel"></param>
		/// <param name="httpVersion"></param>
		public void Set( long downloadedSize, int writtenSize, long totalSize, int storedFile, int totalFile, AssetBundleManager.DownloadEntity[] targets, int nowParallel, int maxParallel, int httpVersion )
		{
			m_Gauge.Value = ( float )downloadedSize / ( float )totalSize ;
			SetStateToRatio( m_Gauge.Value ) ;

			//----------------------------------------------------------

			if( Define.DevelopmentMode == false )
			{
				return ;
			}

			//----------------------------------------------------------

			m_FileNow.Value = storedFile ;
			m_FileMax.Value = totalFile ;

			float now ;
			float max ;

			int mb = 1024 * 1024 ;

			now = ( float )downloadedSize / ( float )mb ;
			now = ( int )( now * 10.0f ) / 10.0f ;

			max = ( float )totalSize / ( float )mb ;
			max = ( int )( max * 10.0f ) / 10.0f ;

			m_SizeNow.Value = now ;
			m_SizeMax.Value = max ;

			m_ParallelNow.Value = nowParallel ;
			m_ParallelMax.Value = maxParallel ;

			if( httpVersion <= 1 )
			{
				m_Protocol.Text = "HTTP/1.1" ;
			}
			else
			{
				m_Protocol.Text = "HTTP/2.0" ;
			}

			//--------------------------------------------------------------------------

			float time ;

			//----------------------------------------------------------
			// 転送速度

			time = Time.realtimeSinceStartup - m_Rate_Time ;

			long size ;
			float mbSize ;

			if( time >= 1.0f )
			{
				// １秒経過
				float tick = Time.realtimeSinceStartup ;

				//---------------------------------
				// D Rate

				size = downloadedSize - m_DownloadingRate_Size ;

				mbSize = ( float )size / ( 1024.0f * 1024.0f ) ;

				// 小数点３桁までとする
				mbSize = ( int )( mbSize * 1000 ) / 1000.0f ;

				m_D_Rate.Value = mbSize ;

				m_DownloadingRate_Size = downloadedSize ;

				m_DownloadingRate_Total += mbSize ;
				m_DownloadingRate_Count ++ ;

				//---------------------------------
				// W Rate

				size = writtenSize - m_WritingRate_Size ;

				mbSize = ( float )size / ( 1024.0f * 1024.0f ) ;

				// 小数点３桁までとする
				mbSize = ( int )( mbSize * 1000 ) / 1000.0f ;

				m_W_Rate.Value = mbSize ;

				m_WritingRate_Size = writtenSize ;

				m_WritingRate_Total += mbSize ;
				m_WritingRate_Count ++ ;

				//---------------------------------

				m_Rate_Time = tick ;
			}

			//----------------------------------------------------------
			// 累計時間

			time = Time.realtimeSinceStartup - m_DownloadingTime_Base ;
			int hour = 0, minute = 0, second = 0 ;

			if( time >= 3600.0f )
			{
				hour = ( int )( time / 3600.0f ) ;
				time %= 3600.0f ;
			}

			if( time >= 60.0f )
			{
				minute = ( int )( time / 60.0f ) ;
				time %= 60.0f ;
			}

			second = ( int )time ;

			string timeName = string.Empty ;

			if( hour >  0 )
			{
				timeName = hour.ToString() + "時間" ;
			}

			if( minute >  0 || ( minute == 0 && hour >  0 ) )
			{
				if( hour == 0 )
				{
					timeName += minute.ToString() ;
				}
				else
				{
					timeName += minute.ToString( "D2" ) ;
				}
				timeName += "分" ;
			}

			if( second >  0 || ( second == 0 && minute >  0 ) )
			{
				if( minute == 0 )
				{
					timeName += second.ToString() ;
				}
				else
				{
					timeName += second.ToString( "D2" ) ;
				}
				timeName += "秒" ;
			}

			m_Time.Text = timeName ;

#if UNITY_EDITOR
			if( downloadedSize == totalSize && m_DownloadingDisplay == false )
			{
				m_DownloadingDisplay = true ;

				string files = string.Empty ;
				int i, l = targets.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					files += " + " + targets[ i ].Path ;
					if( targets[ i ].Keep == true )
					{
						files += " [K]" ;
					}
					files += "\n" ;
				}

				Debug.Log( "<color=#FFFF00>[Downloading File] " + totalFile + "</color>" + "\n" + files ) ;
				Debug.Log( "<color=#FFFF00>[Downloading Size] " + max.ToString() + " (MB)" + "</color>" ) ;

				if( m_DownloadingRate_Count >  0 )
				{
					mbSize = m_DownloadingRate_Total / m_DownloadingRate_Count ;
					mbSize = ( int )( mbSize * 1000 ) / 1000.0f ;
					Debug.Log( "<color=#FFFF00>[Downloading Rate] " + mbSize.ToString() + " (MB/秒)" + "</color>" ) ;
				}

				if( m_WritingRate_Count >  0 )
				{
					mbSize = m_WritingRate_Total / m_WritingRate_Count ;
					mbSize = ( int )( mbSize * 1000 ) / 1000.0f ;
					Debug.Log( "<color=#FFFF00>[Writing Rate] " + mbSize.ToString() + " (MB/秒)" + "</color>" ) ;
				}

				Debug.Log( "<color=#FFFF00>[Downloading Time] " + timeName + "</color>" ) ;
			}
#endif
		}

		// 進行度メッセージを設定する
		private void SetStateToRatio( float ratio )
		{
			m_State.Text = "ダウンロード進行度..." ;
			m_Ratio.SetActive( true ) ;
			m_Ratio.Value = ( int )( ratio * 100.0f ) ;
		}

		/// <summary>
		/// アニメーションを停止する
		/// </summary>
		/// <returns></returns>
		public async UniTask Complete( string completedMessage )
		{
			m_State.Text = completedMessage ;
			m_Ratio.SetActive( false ) ;

			//----------------------------------------------------------
			
			if( Define.DevelopmentMode == false )
			{
				return ;
			}

			//----------------------------------

			m_StateMessage.Text = "Download Completed" ;

			m_Animation.StopFlipper( "Move" ) ;

			await When( m_Animation.PlayTweenAndHide( "FadeOut" ) ) ;
		}


		//-----------------------------------------------------------

		/// <summary>
		/// フェードイン
		/// </summary>
		/// <returns></returns>
		public async UniTask FadeIn()
		{
			if( View.ActiveSelf == true )
			{
				return ;
			}

			m_Animation.SetActive( true ) ;
			m_Animation.Alpha = 1 ;	// フェードアウトでアルファが０になっている対策

			await When( View.PlayTween( "FadeIn" ) ) ;

			_ = m_Animation.PlayFlipper( "Move" ) ;
		}

		/// <summary>
		/// フェードアウト
		/// </summary>
		/// <returns></returns>
		public async UniTask FadeOut()
		{
			if( View.ActiveSelf == false )
			{
				return ;
			}

			if( m_Animation.ActiveSelf == true )
			{
				m_Animation.StopFlipper( "Move" ) ;
			}

			await When( View.PlayTweenAndHide( "FadeOut" ) ) ;
		}

	}
}
