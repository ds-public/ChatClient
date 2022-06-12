using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Threading ;

using Cysharp.Threading.Tasks ;

using UnityEngine ;

// 要 uGUIHelper パッケージ
using uGUIHelper ;
using SceneManagementHelper ;

namespace Template
{
	/// <summary>
	/// タイアログクラス(汎用ダイアログの表示に使用する) Version 2022/04/27 0
	/// </summary>
	public class Dialog : ExMonoBehaviour
	{
		// シングルトンインスタンス
		private static Dialog m_Instance ;

		/// <summary>
		/// ダイアログクラスのインスタンス
		/// </summary>
		public  static Dialog   Instance
		{
			get
			{
				return m_Instance ;
			}
		}

		//-------------------------------------

		[SerializeField]
		protected UICanvas	m_Canvas ;

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
		
		// スクリーン
		[SerializeField]
		protected UIImage	m_Screen ;

		// ダイアログの展開レイヤー
		[SerializeField,Header("ダイアログを展開するルートノード")]
		protected UIView	m_Layer ;

		//-------------------------------------

		// スタイルごとのウィンドウ

		[SerializeField,Header("スタンダードタイプ")]
		protected DialogStyle.Standard	m_Standard ;

		[SerializeField,Header("エクセプションタイプ")]
		protected DialogStyle.Exception	m_Exception ;


		//-----------------------------------

		// 現在生成中のダイアログのインスタンス

		private List<DialogStyle.DialogStyleBase>	m_Dialogs = new List<DialogStyle.DialogStyleBase>() ;

		private CancellationTokenSource				m_AllClosingTokenSource ;


		//-------------------------------------

		/// <summary>
		/// 肯定的な選択肢を選んだ際に再生される効果音の名前
		/// </summary>
		public	string	SoundOfPositive = string.Empty ;

		// 表示中のダイアログの数
		[SerializeField]
		protected int m_DialogCount ;

		/// <summary>
		/// 表示中のダイアログの数
		/// </summary>
		public static int Count
		{
			get
			{
				if( m_Instance == null )
				{
					return 0 ;
				}
				return m_Instance.m_DialogCount ;
			}
		}

		//-----------------------------------

		/// <summary>
		/// 外側にも反応するかどうか(ただしボタンが１つの場合のみ)
		/// </summary>
		/// 
		public static bool OutsideEnabled
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}
				return m_Instance.m_OutsideEnable ;
			}
			set
			{
				if( m_Instance == null )
				{
					return ;
				}
				m_Instance.m_OutsideEnable = value ;
			}
		}
		
		private bool m_OutsideEnable = true ;

		//---------------------------------------------------------------------------

		internal void Awake()
		{
			m_Instance = this ;

			//----------------------------------------------------------

			// 各要素を非表示状態にしておく

			m_Screen.Enabled = false ;
//			m_Screen.Enabled = true ;
//			m_Screen.Color = new Color( 0, 0, 0, 0 ) ;
//			m_Screen.RaycastTarget = true ;

			if( m_Standard != null )
			{
				m_Standard.SetActive( false ) ;
			}

			if( m_Exception != null )
			{
				m_Exception.SetActive( false ) ;
			}

			//----------------------------------------------------------

			// 表示しているダイアログの数
			m_DialogCount = m_Layer.transform.childCount ;

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

			//----------------------------------------------------------

			m_AllClosingTokenSource = new CancellationTokenSource() ;
		}

		internal void OnDestroy()
		{
			// トークンソースの後始末を行う
			if( m_AllClosingTokenSource != null )
			{
				m_AllClosingTokenSource.Dispose() ;
				m_AllClosingTokenSource = null ;
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 開いている全てのダイアログをまとめて破棄する(破棄なのでフェードアウトは無い)
		/// </summary>
		public static void CloseAll()
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.CloseAll_Private() ;
		}

		private void CloseAll_Private()
		{
			m_AllClosingTokenSource.Cancel() ;

			if( m_Dialogs.Count >  0 )
			{
				int i, l = m_Dialogs.Count ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					var dialog = m_Dialogs[ i ] ;
					if( dialog != null )
					{
						DestroyImmediate( dialog.gameObject ) ;
					}
				}

				m_Dialogs.Clear() ;
			}

			m_AllClosingTokenSource.Dispose() ;
			m_AllClosingTokenSource = new CancellationTokenSource() ;
		}

		//-----------------------------------------------------------

		// 任意のスタイルのダイアログを生成する
		private T Create<T>( Action<T,string,string> callback ) where T : UnityEngine.Component
		{
			if( m_Layer.transform.childCount == 0 )
			{
				// ダイアログシステムを有効化する
				gameObject.SetActive( true ) ;
			}

			//----------------------------------------------------------

			DialogStyle.DialogStyleBase styleBase = null ;

			if( m_Standard is T )
			{
				// スタンダードタイプ
				DialogStyle.Standard dialog = m_Layer.AddPrefab<DialogStyle.Standard>( m_Standard.gameObject ) ;
				dialog.Callback = callback as Action<DialogStyle.Standard,int> ;
				styleBase = dialog ;
			}
			
			if( m_Exception is T )
			{
				// エクセプションタイプ
				DialogStyle.Exception dialog = m_Layer.AddPrefab<DialogStyle.Exception>( m_Exception.gameObject ) ;
				dialog.Callback = callback as Action<DialogStyle.Exception,int> ;
				styleBase = dialog ;
			}
			
			if( styleBase == null )
			{
				Debug.LogWarning( "該当スタイルが存在しない" ) ;
				// 該当するスタイルが存在しない
				return null ;
			}

			// 汎用的な準備処理
			styleBase.Prepare() ;

			// 表示順を調整する
			float z = GetHiestPriority() ;
			Vector3 p = styleBase.transform.position ;
			p.z = z ;
			styleBase.transform.position = p ;

			m_Layer.SortChildByZ() ;

			// フェードインを実行して表示を要求する
			FadeIn( styleBase ) ;

			return styleBase as T ;	// インスタンスを返す(設定を反映させるためには Commit を実行する必要がある)
		}

		//-----------------------------------------------------------

		// ダイアログをフェードイン効果付きで表示する
		private void FadeIn( DialogStyle.DialogStyleBase styleBase )
		{
			// ダイアログコントローラー自体をアクティブにしておく
			styleBase.SetActive( true ) ;

			// フェードインで表示する
			_ = styleBase.FadeIn() ;
		}

		internal void Update()
		{
			if( m_Layer.transform.childCount != m_DialogCount )
			{
				if( m_Layer.transform.childCount == 0 )
				{
					// 表示しているダイアログが無くなったらスリープ状態に入る
					gameObject.SetActive( false ) ;
				}

				// 現在のダイアログの表示数を更新する
				m_DialogCount = m_Layer.transform.childCount ;
			}
		}
		
		//-------------------------------------------------------------------------------------------
		
		/// <summary>
		/// デフォルト(スタンダード)ダイアログを開く
		/// </summary>
		/// <returns>The open.</returns>
		/// <param name="title">Title.</param>
		/// <param name="message">Message.</param>
		/// <param name="buttonLabels">Button labels.</param>
		/// <param name="onClosed">On closed.</param>
		/// <param name="buttonIndex">Button index.</param>
		public static async UniTask<int> Open( string title, string message, string[] buttonLabels = null, Action<int> onClosed = null, bool isSilent = false, bool outsideEnabled = true )
		{
			if( m_Instance == null )
			{
				return -1 ;
			}

			return await m_Instance.Open_Private( title, message, buttonLabels, onClosed, isSilent, outsideEnabled ) ;
		}

		// デフォルト(スタンダード)ダイアログを開く
		private async UniTask<int> Open_Private( string title, string message, string[] buttonLabels, Action<int> onClosed, bool isSilent, bool outsideEnabled = true )
		{
			DialogStyle.Standard dialog = Create<DialogStyle.Standard>( null ) ;

			m_Dialogs.Add( dialog ) ;		// 登録

			dialog.Title.Text				= title ;
			dialog.Message.Text				= message ;
			dialog.SelectionButtonLabels	= buttonLabels ;
			dialog.AutoClose				= true ;
			dialog.AutoCloseCallback		= onClosed ;
			dialog.IsSilent					= isSilent ;
			dialog.OutsideEnabled			= outsideEnabled ;
			dialog.Commit() ;

			await WaitUntil( () => dialog.IsClosed, cancellationToken:m_AllClosingTokenSource.Token ) ;

			m_Dialogs.Remove( dialog ) ;	// 除去

			return dialog.Result ;
		}
		
		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// エクセプションダイアログを開く
		/// </summary>
		/// <returns>The open.</returns>
		/// <param name="title">Title.</param>
		/// <param name="message">Message.</param>
		/// <param name="buttonLabels">Button labels.</param>
		/// <param name="onClosed">On closed.</param>
		/// <param name="buttonIndex">Button index.</param>
		public static async UniTask<int> OpenException( string title, string message, string[] buttonLabels = null, Action<int> onClosed = null )
		{
			if( m_Instance == null )
			{
				return -1 ;
			}

			return await m_Instance.OpenException_Private( title, message, buttonLabels, onClosed ) ;
		}

		// エクセプションダイアログを開く
		private async UniTask<int> OpenException_Private( string title, string message, string[] buttonLabels, Action<int> onClosed )
		{
			DialogStyle.Exception dialog =Create<DialogStyle.Exception>( null ) ;

			m_Dialogs.Add( dialog ) ;		// 登録

			dialog.Title.Text				= title ;
			dialog.Message.Text				= message ;
			dialog.SelectionButtonLabels	= buttonLabels ;
			dialog.AutoClose				= true ;
			dialog.AutoCloseCallback		= onClosed ;
			dialog.Commit() ;

			await WaitUntil( () => dialog.IsClosed, cancellationToken:m_AllClosingTokenSource.Token ) ;

			m_Dialogs.Remove( dialog ) ;	// 除去

			return dialog.Result ;
		}
		
		//-------------------------------------------------------------------------------------------

		// 表示中のダイアログで最もプライオリティ値が高い値を取得する
		private int GetHiestPriority()
		{
			int i, l = m_Layer.transform.childCount ;

			if( l == 0 )
			{
				return 0 ;
			}

			float highestZ = - Mathf.Infinity ;
			float z ;

			for( i  = 0 ; i <  l ; i ++  )
			{
				z = m_Layer.transform.GetChild( i ).transform.position.z ;
				if( z >  highestZ )
				{
					highestZ  = z ;
				}
			}

			return ( int )highestZ ;
		}

		/// <summary>
		/// 肯定的な回答を選んだ際に再生される効果音名を設定する
		/// </summary>
		/// <param name="soundOfPositive">効果音名</param>
		public static void SetSoundOfPositive( string soundOfPositive )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.SoundOfPositive = soundOfPositive ;
		}

		//-------------------------------------------------------------------------------------------
	}
}
