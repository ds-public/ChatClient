#if !UNITY_EDITOR && UNITY_ANDROID
#define ANDROID
#endif

#if !UNITY_EDITOR && UNITY_IOS
#define IOS
#endif

//-------------------------------------
// デバッグ記述

//#define ANDROID
//#define IOS

//-------------------------------------

using System ;

#if ANDROID
using Unity.Notifications.Android ;
#endif
#if IOS
using Unity.Notifications.iOS ;
#endif

using UnityEngine ;

namespace Template
{
	/// <summary>
	/// ローカルプッシュを管理するクラス Version 2022/05/01 0
	/// </summary>
	public class LocalPushManager : SingletonManagerBase<LocalPushManager>
	{
		// Small アイコンの透過画像がめんどくさい
		// https://qoopmk.hatenablog.jp/entry/2019/10/17/080000



#if ANDROID
		private const string	m_AndroidChannelId = "Beast" ;
#endif

#if IOS
		private long			m_iOSIdentidy = 1 ;
#endif
		//-------------------------------------------------------------------------------------------

		// 一応 Awake() 継承
		new protected void Awake()
		{
			base.Awake() ;

#if ANDROID
			//　Androidチャンネルの登録
			// LocalPushNotification.RegisterChannel(引数1,引数２,引数３);
			// 引数１ Androidで使用するチャンネルID なんでもいい LocalPushNotification.AddSchedule()で使用する
			// 引数2　チャンネルの名前　なんでもいい　アプリ名でも入れておく
			// 引数3　通知の説明 なんでもいい　自分がわかる用に書いておくもの　
			RegisterChannel( "LGMF", "LGMF通知" ) ;
#endif
		}


#if ANDROID
		// Androidで使用するプッシュ通知用のチャンネルを登録する。    
		public static void RegisterChannel( string title, string description )
		{
			// チャンネルの登録
			var channel = new AndroidNotificationChannel()
			{
				Id			= m_AndroidChannelId,
				Name		= title,
				Importance	= Importance.High,//ドキュメント　重要度を設定するを参照　https://developer.android.com/training/notify-user/channels?hl=ja
				Description	= description,
			} ;
			AndroidNotificationCenter.RegisterNotificationChannel( channel ) ;
		}
#endif

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 指定した秒数後にプッシュ通知を行う(ただし45秒以上)
		/// </summary>
		/// <param name="title"></param>
		/// <param name="message"></param>
		/// <param name="badgeCount"></param>
		/// <param name="seconds"></param>
		/// <returns></returns>
		public static bool AddSeconds( string title, string message, int badgeCount, int seconds )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_Instance.AddSeconds_Private( title, message, badgeCount, seconds ) ;

			return true ;
		}

		/// <summary>
		/// 指定した日時にプッシュ通知を行う
		/// </summary>
		/// <param name="title"></param>
		/// <param name="message"></param>
		/// <param name="badgeCount"></param>
		/// <param name="seconds"></param>
		/// <returns></returns>
		public static bool Add( string title, string message, int badgeCount, int year, int month, int day, int hour, int minute, int second )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_Instance.Add_Private( title, message, badgeCount, year, month, day, hour, minute, second ) ;

			return true ;
		}

		/// <summary>
		/// 指定した日時にプッシュ通知を行う(UTC)
		/// </summary>
		/// <param name="title"></param>
		/// <param name="message"></param>
		/// <param name="badgeCount"></param>
		/// <param name="seconds"></param>
		/// <returns></returns>
		public static bool Add( string title, string message, int badgeCount, long utc )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			var dt = ClientTime.UnixTimeToDateTime( utc ) ;
			m_Instance.Add_Private( title, message, badgeCount, dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second ) ;

			return true ;
		}

		/// <summary>
		/// 指定した日時にプッシュ通知を行う(DateTime)
		/// </summary>
		/// <param name="title"></param>
		/// <param name="message"></param>
		/// <param name="badgeCount"></param>
		/// <param name="seconds"></param>
		/// <returns></returns>
		public static bool Add( string title, string message, int badgeCount, DateTime dt )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_Instance.Add_Private( title, message, badgeCount, dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second ) ;

			return true ;
		}

		/// <summary>
		/// 指定した識別子の登録済みの通知を消去する
		/// </summary>
		/// <param name="title"></param>
		/// <param name="message"></param>
		/// <param name="badgeCount"></param>
		/// <param name="seconds"></param>
		/// <returns></returns>
		public static bool Remove( string identity )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_Instance.Remove_Private( identity ) ;

			return true ;
		}

		/// <summary>
		/// 登録済みの通知を全て消去する
		/// </summary>
		/// <param name="title"></param>
		/// <param name="message"></param>
		/// <param name="badgeCount"></param>
		/// <param name="seconds"></param>
		/// <returns></returns>
		public static bool RemoveAll()
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_Instance.RemoveAll_Private() ;

			return true ;
		}

		//-------------------------------------------------------------------------------------------

		//「アプリを開始した時」、「Homeボタンを押した時」、「Backボタンを押した時(ボタンはAndroidのみに存在)」、「OvewViewボタンを押した時(ボタンはAndroidのみに存在)」に実行
		internal void OnApplicationFocus( bool hasFocus )
		{
			// フォーカスを得た時と失った時に受け取り済みの通知を全て消去する
			ClearAll_Private() ;
		}

		//-------------------------------------------------------------------------------------------

		// 受信済みの通知を全て消去する
		private void ClearAll_Private()
		{
#if ANDROID
			// Androidの通知をすべて削除します
			AndroidNotificationCenter.CancelAllDisplayedNotifications() ;
#endif
#if IOS
			// iOSの通知をすべて削除します
			iOSNotificationCenter.RemoveAllDeliveredNotifications() ;

			// バッジを消します
			iOSNotificationCenter.ApplicationBadge = 0 ;
#endif
		}

		// 登録済みの通知を消去する
		private void Remove_Private( string identity )
		{
#if ANDROID
			// Androidの通知をすべて削除します
			int id ;
			if( int.TryParse( identity, out id ) == false )
			{
				return ;
			}
			AndroidNotificationCenter.CancelNotification( id ) ;
#endif
#if IOS
			// iOSの通知をすべて削除します
			iOSNotificationCenter.RemoveScheduledNotification( identity ) ;
#endif
		}

		// 登録済みの通知を全て消去する
		private void RemoveAll_Private()
		{
#if ANDROID
			// Androidの通知をすべて削除します
			AndroidNotificationCenter.CancelAllNotifications() ;
#endif
#if IOS
			// iOSの通知をすべて削除します
			iOSNotificationCenter.RemoveAllScheduledNotifications() ;
#endif
		}

		//-------------------------------------------------------------------------------------------
   
		// プッシュ通知を登録する(Android)
		private string AddSeconds_Private( string title, string message, int badgeCount, int elapsedTime )
		{
#if ANDROID
			return SetNotificationForAndroid( title, message, badgeCount, DateTime.Now.AddSeconds( elapsedTime ) ) ;
#elif IOS
			var trigger = new iOSNotificationTimeIntervalTrigger()
			{
				TimeInterval = new TimeSpan( 0, 0, elapsedTime ),
				Repeats = false
			} ;

			return SetNotificationForiOS( title, message, badgeCount, trigger ) ;
#else
			return null ;
#endif
		}

		// プッシュ通知を登録する(iOS)
		private string Add_Private( string title, string message, int badgeCount, int year, int month, int day, int hour, int minute, int second )
		{
#if ANDROID
			return SetNotificationForAndroid( title, message, badgeCount, new DateTime( year, month, day, hour, minute, second ) ) ;
#elif IOS
			var trigger = new iOSNotificationCalendarTrigger()
			{
				Year	= year,
				Month	= month,
				Day		= day,
				Hour	= hour,
				Minute	= minute,
				Second	= second,
				Repeats	= false
			} ;

			return SetNotificationForiOS( title, message, badgeCount, trigger ) ;
#else
			return null ;
#endif
		}

		//-----------------------------------


#if ANDROID
		// 通知を登録(Android)   
		private string SetNotificationForAndroid( string title, string message, int badgeCount, DateTime dateTime )
		{
			// 通知を作成します。
			var notification = new AndroidNotification
			{
				Title		= title,
				Text		= message,
				Number		= badgeCount,

				// Androidのアイコンを設定
				SmallIcon	= "icon_0",	// どの画像を使用するかアイコンのIdentifierを指定　指定したIdentifierが見つからない場合アプリアイコンになる。
				LargeIcon	= "icon_1",	// どの画像を使用するかアイコンのIdentifierを指定　指定したIdentifierが見つからない場合アプリアイコンになる。
				FireTime	= dateTime
			} ;
			
			// 通知を送信します。
			int id = AndroidNotificationCenter.SendNotification( notification, m_AndroidChannelId ) ;

			return id.ToString() ;
		}
#endif

#if IOS
		// 通知を登録(iOS)
		private string SetNotificationForiOS( string title, string message, int badgeCount, iOSNotificationTrigger trigger )
		{
			// 固有識別子を生成する
			string identity = ClientTime.GetCurrentUnixTime().ToString() + m_iOSIdentidy.ToString() ;
			m_iOSIdentidy ++ ;	// 同じ日時(秒)で識別子が被らないようにシーケンシャルな値を加算する

			// 通知を作成
			iOSNotificationCenter.ScheduleNotification( new iOSNotification()
			{
				// プッシュ通知を個別に取り消しなどをする場合はこのIdentifierを使用します。(未検証)
				Identifier			= identity,
				Title				= title,
				Body				= message,
				ShowInForeground	= true,			// アプリがフォアグラウンドの時に通知を表示するか
				Badge				= badgeCount,
				Trigger				= trigger
			} ) ;

			return identity ;
		}
#endif
	}
}
