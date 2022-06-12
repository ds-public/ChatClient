using System ;
using System.Collections.Generic ;

using Cysharp.Threading.Tasks ;

using SceneManagementHelper ;

using uGUIHelper ;

using UnityEngine ; // 要 SceneManagementHelper パッケージ

using CommonDialog = Template.Dialog ;


namespace Template
{
	/// <summary>
	/// シーンクラス(シーンの展開や追加に使用する)  Version 2022/05/15
	/// </summary>
	public class Scene : ExMonoBehaviour
	{
		private static Scene	m_Instance ;
		internal void Awake()
		{
			m_Instance = this ;
			m_AsynchronousAdditionExclusiveLock = false ;
		}
		internal void OnDestroy()
		{
			m_Instance = null ;
		}

		//-----------------------------------

		// スクリーン(単独の画面として起動する事が可能なシーン群)
		public class Screen
		{
			/// <summary>
			/// Boot
			/// </summary>
			public const string Boot					= "Screen_Boot" ;

			/// <summary>
			/// Downloading
			/// </summary>
			public const string Downloading				= "Screen_Downloading" ;

			/// <summary>
			/// Title
			/// </summary>
			public const string Title					= "Screen_Title" ;

			//----------------------------------------------------------

			/// <summary>
			/// チャットクライアント
			/// </summary>
			public const string ChatClient				= "Screen_ChatClient" ;

			/// <summary>
			/// チャットサーバー
			/// </summary>
			public const string ChatServer				= "Screen_ChatServer" ;

			//----------------------------------------------------------

			/// <summary>
			/// Template
			/// </summary>
			public const string Template				= "Screen_Template" ;

			/// <summary>
			/// Reboot
			/// </summary>
			public const string Reboot					= "Screen_Boot" ;
		}

		// レイアウト(２Ｄ系の単独では起動する事ができないシーン群)
		public class Layout
		{
			/// <summary>
			/// Template
			/// </summary>
			public const string Template				= "Layout_Template" ;
		}

		// ダイアログ
		public class Dialog
		{
			/// <summary>
			/// Template
			/// </summary>
			public const string Template				= "Dialog_Template" ;
		}


		//-----------------------------------------------------------------

		/// <summary>
		/// フェード付きシーン遷移で false になるまでフェードインを待たせる
		/// </summary>
		public static bool Ready = true ;

		private static bool m_IsFading = false ;
		public static bool IsFading
		{
			get
			{
				return m_IsFading ;
			}
		}

		/// <summary>
		/// フェード完了を待つ
		/// </summary>
		/// <returns></returns>
		public static async UniTask WaitForFading()
		{
			await m_Instance.WaitWhile( () => m_IsFading ) ;
		}

		//-----------------------------------------------------------------

		// 複数のシーンを非同期でロードするとUniTask+CustomYieldInstructionの組み合わせが上手く処理されない事があるため処理を直列化する
		private static bool m_AsynchronousAdditionExclusiveLock ;

		//-----------------------------------------------------------------

		// Load LoadAsync

		/// <summary>
		/// 指定した名前のシーンを展開する(同期版)
		/// </summary>
		/// <param name="sceneName"></param>
		/// <param name="label"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool Load( string sceneName, string label = "", System.Object value = null )
		{
			Asset.ClearResourceCache( false, "Scene.Load( " + sceneName + " )" ) ;	// リソースキャッシュをクリアする

			return EnhancedSceneManager.Load( sceneName, label, value ) ;
		}

		/// <summary>
		/// 指定した名前のシーンを展開する(同期版)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="sceneName"></param>
		/// <param name="onLoaded"></param>
		/// <param name="targetName"></param>
		/// <param name="label"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool Load<T>( string sceneName, Action<T[]> onLoaded, string targetName = "", string label = "", System.Object value = null ) where T : UnityEngine.Component
		{
			Asset.ClearResourceCache( false, "Scene.Load<T>( " + sceneName + " )" ) ;	// リソースキャッシュをクリアする

			return EnhancedSceneManager.Load<T>( sceneName, onLoaded, targetName, label, value ) ;
		}

		/// <summary>
		/// 指定した名前のシーンを展開する(非同期版)
		/// </summary>
		/// <param name="sceneName"></param>
		/// <param name="label"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static async UniTask<bool> LoadAsync( string sceneName, string label = "", System.Object value = null )
		{
			Asset.ClearResourceCache( false, "Scene.LoadAsync( " + sceneName + " )" ) ;	// リソースキャッシュをクリアする

			EnhancedSceneManager.Request request = EnhancedSceneManager.LoadAsync( sceneName, label, value ) ;
			await request ;

			if( request.IsDone == true )
			{
				// 成功
				return true ;
			}
			else
			{
				// 失敗(エラーダイアログの表示)
				await CommonDialog.Open( "シーン展開エラー", "<color=#FF7F00>" + sceneName + "</color>", new string[]{ "閉じる" } ) ;

				return false ;
			}
		}

		/// <summary>
		/// 指定した名前のシーンを展開する(非同期版)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="sceneName"></param>
		/// <param name="onLoaded"></param>
		/// <param name="targetName"></param>
		/// <param name="label"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static async UniTask<T> LoadAsync<T>( string sceneName, string label = "", System.Object value = null ) where T : UnityEngine.Component
		{
			T[] targets = await LoadAsync<T>( sceneName, null, label, value ) ;
			if( targets == null || targets.Length == 0 )
			{
				return null ;
			}
			return targets[ 0 ] ;	// 最初の１つだけ返す
		}

		/// <summary>
		/// 指定した名前のシーンを展開する(非同期版)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="sceneName"></param>
		/// <param name="onLoaded"></param>
		/// <param name="targetName"></param>
		/// <param name="label"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static async UniTask<T[]> LoadAsync<T>( string sceneName, string targetName, string label = "", System.Object value = null ) where T : UnityEngine.Component
		{
			Asset.ClearResourceCache( false, "Scene.LoadAsync<T>( " + sceneName + " )" ) ;	// リソースキャッシュをクリアする

			T[] targets = null ;
			EnhancedSceneManager.Request request = EnhancedSceneManager.LoadAsync<T>( sceneName, ( _ ) => { targets = _ ; }, targetName, label, value ) ;
			await request ;

			if( request.IsDone == true )
			{
				// 成功
				return targets ;
			}
			else
			{
				// 失敗(エラーダイアログの表示)
				await CommonDialog.Open( "シーン展開エラー", "<color=#FF7F00>" + sceneName + "</colo>", new string[]{ "閉じる" } ) ;

				return null ;
			}
		}

		//-----------------------------------

		/// <summary>
		/// フェード演出込みで指定した名前のシーンを展開する(呼び出し元のシーンは途中で破棄されてしまうため常駐型のゲームオブジェクト上で実行する必要がある)
		/// </summary>
		/// <param name="sceneName"></param>
		/// <param name="label"></param>
		/// <param name="value"></param>
		/// <param name="fastLoad"></param>
		/// <param name="onFadeOutFinished"></param>
		/// <param name="blockingFadeIn"></param>
		/// <returns></returns>
		public static async UniTask<bool> LoadWithFade( string sceneName, string label = "", System.Object value = null, bool fastLoad = true, Action<string> onFadeOutFinished = null, bool blockingFadeIn = false, float duration = -1, Fade.FadeTypes fadeType = Fade.FadeTypes.Color )
		{
			if( m_IsFading == true )
			{
				// 直列化による処理のブロッキング
				Debug.LogWarning( "<color=#FF7F00>[警告] LoadWithFade 処理中にロードされた " + sceneName + " によって LoadWithFade が実行された</color>" ) ;
//				Debug.LogWarning( "<color=#FF7F00>[警告] 現在処理中の LoadWithFade が完了するまで " + sceneName + " によって実行された LoadWithFad は待機します</color>" ) ;

//				if( Ready == false && blockingFadeIn == false )
//				{
//					// 現シーンはブロック有効で次シーンでブロック無効だと、現シーンのブロック終了宣言を受けるまで永久に待つ事になってしまうので、強制的にブロックを解除する。
//					Ready  = true ;
//				}
//
//				await m_Instance.WaitWhile( () => m_IsFading ) ;
			}

			//----------------------------------------------------------

			// OnStart() 中に LoadWithFade() を使って別シーンに遷移させる場合は、以下のコードで現シーンを中断させる事を推奨。
//			throw new OperationCanceledException() ;	// タスクキャンセル




//			Debug.Log( "<color=#FF00FF>LoadWithFade 開始 [ " + sceneName + " ]</color>" ) ;

			// 処理開始
			if( m_IsFading == false )
			{
				m_IsFading = true ;

				// 進行許可フラグ
				Ready = false ;

				// フェードアウト演出を実行する
				await Fade.Out( fadeType:fadeType, duration:duration ) ;

				// フェードアウトが終わった際に呼び出すコールバックメソッドを呼び出す
				onFadeOutFinished?.Invoke( sceneName ) ;
			}

			//----------------------------------------------------------

			// デフォルトで準備完了状態とする
			Ready |= ( blockingFadeIn == false ) ;

			Asset.ClearResourceCache( false, "Scene.LoadWithFade( " + sceneName + " )" ) ;	// リソースキャッシュをクリアする

			bool result ;

			// コンテンツ部の新しいシーンを展開する(問題はこの中でさらに LoadWithFade が呼ばれているケース)
			if( fastLoad == true )
			{
				// 同期版
				result = EnhancedSceneManager.Load( sceneName, label, value ) ;
			}
			else
			{
				// 非同期版
				var request = EnhancedSceneManager.LoadAsync( sceneName, label, value ) ;
				await request ;

				result = request.IsDone ;
			}

			// ロードされたシーンで準備が完了するまでフェードインを行わせたくない場合にこのフラグを操作する
			await m_Instance.WaitWhile( () => Ready == false ) ;

			// フェードイン演出を実行する
			await Fade.In( duration:duration ) ;

			// 処理終了
			m_IsFading = false ;

//			Debug.Log( "<color=#FF00FF>LoadWithFade 終了 [ " + sceneName + " ]</color>" ) ;

			return result ;
		}

		//-----------------------------------------------------------

		// Add AddAsync

		/// <summary>
		/// 指定した名前のシーンを追加する(同期版)
		/// </summary>
		/// <param name="sceneName"></param>
		/// <param name="label"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool Add( string sceneName, string label = "", System.Object value = null )
		{
			// 新しいシーンを追加する
			return EnhancedSceneManager.Add( sceneName, label, value ) ;
		}

		/// <summary>
		/// 指定した名前のシーンを追加する(同期版)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="sceneName"></param>
		/// <param name="onLoaded"></param>
		/// <param name="targetName"></param>
		/// <param name="label"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool Add<T>( string sceneName, Action<T[]> onLoaded, string targetName = "", string label = "", System.Object value = null ) where T : UnityEngine.Component
		{
			// 新しいシーンを追加する
			return EnhancedSceneManager.Add<T>( sceneName, onLoaded, targetName, label, value ) ;
		}

		/// <summary>
		/// 指定した名前のシーンを追加する(非同期版)
		/// </summary>
		/// <param name="sceneName"></param>
		/// <param name="label"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static async UniTask<bool> AddAsync( string sceneName, string label = "", System.Object value = null )
		{
			if( m_AsynchronousAdditionExclusiveLock == true )
			{
				await m_Instance.WaitWhile( () => m_AsynchronousAdditionExclusiveLock ) ;
			}

			m_AsynchronousAdditionExclusiveLock = true ;

			//----------------------------------------------------------

			EnhancedSceneManager.Request request = EnhancedSceneManager.AddAsync( sceneName, label, value ) ;
			await request ;

			if( request.IsDone == true )
			{
				// 成功
				m_AsynchronousAdditionExclusiveLock = false ;

				return true ;
			}
			else
			{
				// 失敗(エラーダイアログの表示)
				await CommonDialog.Open( "シーン加算エラー", "<color=#FF7F00>" + sceneName + "</color>", new string[]{ "閉じる" } ) ;

				m_AsynchronousAdditionExclusiveLock = false ;

				return false ;
			}
		}

		/// <summary>
		/// 指定した名前のシーンを追加する(非同期版)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="sceneName"></param>
		/// <param name="onLoaded"></param>
		/// <param name="targetName"></param>
		/// <param name="label"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static async UniTask<T> AddAsync<T>( string sceneName, string label = "", System.Object value = null, bool deactivate = false ) where T : UnityEngine.Component
		{
			T[] targets = await AddAsync<T>( sceneName, null, label, value ) ;
			if( targets == null || targets.Length == 0 )
			{
				return null ;
			}

			if( deactivate == true )
			{
				targets[ 0 ].gameObject.SetActive( false ) ;
			}

			return targets[ 0 ] ;	// 最初の１つだけ返す
		}

		/// <summary>
		/// 指定した名前のシーンを追加する(非同期版)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="sceneName"></param>
		/// <param name="onLoaded"></param>
		/// <param name="targetName"></param>
		/// <param name="label"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static async UniTask<T[]> AddAsync<T>( string sceneName, string targetName, string label = "", System.Object value = null ) where T : UnityEngine.Component
		{
			if( m_AsynchronousAdditionExclusiveLock == true )
			{
				await m_Instance.WaitWhile( () => m_AsynchronousAdditionExclusiveLock ) ;
			}

			m_AsynchronousAdditionExclusiveLock = true ;

			//----------------------------------------------------------

			T[] targets = null ;
			EnhancedSceneManager.Request request = EnhancedSceneManager.AddAsync<T>( sceneName, ( _ ) => { targets = _ ; }, targetName, label, value ) ;
			await request ;

			if( request.IsDone == true )
			{
				// 成功
				m_AsynchronousAdditionExclusiveLock = false ;

				return targets ;
			}
			else
			{
				// 失敗(エラーダイアログの表示)
				await CommonDialog.Open( "シーン加算エラー", "<color=#FF7F00>" + sceneName + "</color>", new string[]{ "閉じる" } ) ;

				m_AsynchronousAdditionExclusiveLock = false ;

				return null ;
			}
		}

		//-----------------------------------------------------

		/// <summary>
		/// 現在のシーンの１つ前に展開されていたシーンを展開する(同期版)
		/// </summary>
		/// <param name="label"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool Back( string label = "", System.Object value = null )
		{
			Asset.ClearResourceCache( false, "Scene.Back()" ) ;	// リソースキャッシュをクリアする

			return EnhancedSceneManager.Back( label, value ) ;
		}

		/// <summary>
		/// 現在のシーンの１つ前に展開されていたシーンを展開する(同期版)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="onLoaded"></param>
		/// <param name="targetName"></param>
		/// <param name="label"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool Back<T>( Action<T[]> onLoaded = null, string targetName = "", string label = "", System.Object value = null ) where T : UnityEngine.Component
		{
			Asset.ClearResourceCache( false, "Scene.Back<T>()" ) ;	// リソースキャッシュをクリアする

			return EnhancedSceneManager.Back<T>( onLoaded, targetName, label, value ) ;
		}

		/// <summary>
		/// 現在のシーンの１つ前に展開されていたシーンを展開する(非同期版)
		/// </summary>
		/// <param name="label"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static async UniTask<bool> BackAsync( string label = "", System.Object value = null )
		{
			Asset.ClearResourceCache( false, "Scene.BackAsync()" ) ;	// リソースキャッシュをクリアする

			EnhancedSceneManager.Request request = EnhancedSceneManager.BackAsync( label, value ) ;
			await request ;

			if( request.IsDone == true )
			{
				// 成功
				return true ;
			}
			else
			{
				// 失敗(エラーダイアログの表示)
				await CommonDialog.Open( "シーン後退エラー", "後退に失敗しました", new string[]{ "閉じる" } ) ;

				return false ;
			}
		}

		/// <summary>
		/// 現在のシーンの１つ前に展開されていたシーンを展開する(非同期版)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="onLoaded"></param>
		/// <param name="targetName"></param>
		/// <param name="label"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static async UniTask<T> BackAsync<T>( string label = "", System.Object value = null ) where T : UnityEngine.Component
		{
			T[] targets = await BackAsync<T>( null, label, value ) ;
			if( targets == null || targets.Length == 0 )
			{
				return null ;
			}
			return targets[ 0 ] ;	// 最初の１つだけ返す
		}

		/// <summary>
		/// 現在のシーンの１つ前に展開されていたシーンを展開する(非同期版)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="onLoaded"></param>
		/// <param name="targetName"></param>
		/// <param name="label"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static async UniTask<T[]> BackAsync<T>( string targetName, string label = "", System.Object value = null ) where T : UnityEngine.Component
		{
			Asset.ClearResourceCache( false, "Scene.BackAsync<T>()" ) ;	// リソースキャッシュをクリアする

			T[] targets = null ;
			EnhancedSceneManager.Request request = EnhancedSceneManager.BackAsync<T>( ( _ ) => { targets = _ ; }, targetName, label, value ) ;
			await request ;

			if( request.IsDone == true )
			{
				// 成功
				return targets ;
			}
			else
			{
				// 失敗(エラーダイアログの表示)
				await CommonDialog.Open( "シーン後退エラー", "後退に失敗しました", new string[]{ "閉じる" } ) ;

				return null ;
			}
		}

		//-----------------------------------

		/// <summary>
		/// フェード演出込みで現在のシーンの１つ前に展開されていたシーンを展開する
		/// </summary>
		/// <param name="label"></param>
		/// <param name="value"></param>
		/// <param name="fastLoad"></param>
		/// <param name="onFadeOutFinished"></param>
		/// <param name="blockingFadeIn"></param>
		/// <returns></returns>
		public static async UniTask<bool> BackWithFade( string label = "", System.Object value = null, bool fastLoad = true, Action<string> onFadeOutFinished = null, bool blockingFadeIn = false, float duration = -1 )
		{
			string sceneName = EnhancedSceneManager.GetPreviousName() ;
			if( string.IsNullOrEmpty( sceneName ) == true )
			{
				return false ;	// 戻れない
			}

			//----------------------------------------------------------

			Ready = false ;
			m_IsFading = true ;

			// フェードアウト演出を実行する
			await Fade.Out( duration:duration ) ;

			// フェードアウトが終わった際に呼び出すコールバックメソッドを呼び出す
			onFadeOutFinished?.Invoke( sceneName ) ;

			// デフォルトで準備完了状態とする
			Ready |= ( blockingFadeIn == false ) ;

			Asset.ClearResourceCache( false, "Scene.BackWithFade( " + sceneName + " )" ) ;	// リソースキャッシュをクリアする

			bool result ;

			// コンテンツ部の新しいシーンを展開する
			if( fastLoad == true )
			{
				// 同期版
				result = EnhancedSceneManager.Back( label, value ) ;
			}
			else
			{
				// 非同期版
				var request = EnhancedSceneManager.BackAsync( label, value ) ;
				await request ;

				result = request.IsDone ;
			}

			// ロードされたシーンで準備が完了するまでフェードインを行わせたくない場合にこのフラグを操作する
			await m_Instance.WaitWhile( () => Ready == false ) ;

			// フェードイン演出を実行する
			await Fade.In( duration:duration ) ;

			m_IsFading = false ;

			return result ;
		}

		//-----------------------------------------------------

		/// <summary>
		/// 指定した名前のシーンを削除する
		/// </summary>
		/// <param name="sceneName"></param>
		/// <param name="label"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool Remove( string sceneName, string label = "", System.Object value = null )
		{
			return EnhancedSceneManager.Remove( sceneName, label, value ) ;
		}

		/// <summary>
		/// 指定した名前のシーンを削除する(非同期版)
		/// </summary>
		/// <param name="sceneName"></param>
		/// <param name="onResult"></param>
		/// <param name="label"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static async UniTask<bool> RemoveAsync( string sceneName, string label = "", System.Object value = null )
		{
			EnhancedSceneManager.Request request = EnhancedSceneManager.RemoveAsync( sceneName, null, label, value ) ;
			await request ;

			if( request.IsDone == true )
			{
				// 成功
				return true ;
			}
			else
			{
				// 失敗(エラーダイアログの表示)
				await CommonDialog.Open( "シーン削除エラー", "<color=#FF7F00>" + sceneName + "</color>", new string[]{ "閉じる" } ) ;

				return false ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// １つ前のシーンの名前を取得する
		/// </summary>
		/// <returns>１つ前のシーンの名前(nullで存在しない)</returns>
		public static string GetPreviousName()
		{
			return EnhancedSceneManager.GetPreviousName() ;
		}

		/// <summary>
		/// 現在のシーン名を取得する
		/// </summary>
		/// <returns></returns>
		public static string GetActiveName()
		{
			return EnhancedSceneManager.GetActiveName() ;
		}

		/// <summary>
		/// 現在のシーン名
		/// </summary>
		/// <returns></returns>
		public static string Name
		{
			get
			{
				// 現在のシーンの名前を取得する
				return EnhancedSceneManager.GetActiveName() ;
			}
		}

		/// <summary>
		/// 指定の名前のシーンが現在ロードされているか取得する
		/// </summary>
		/// <param name="sceneName"></param>
		/// <returns></returns>
		public static bool IsLoaded( string sceneName )
		{
			return EnhancedSceneManager.IsLoaded( sceneName ) ;
		}

		//-----------------------------------------------------

		/// <summary>
		/// 受け渡しパラメータを設定する
		/// </summary>
		/// <param name="label"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool SetParameter( string label, System.Object value )
		{
			return EnhancedSceneManager.SetParameter( label, value ) ;
		}

		/// <summary>
		/// 受け渡しパラメータを取得する
		/// </summary>
		/// <param name="label"></param>
		/// <param name="clear"></param>
		/// <returns></returns>
		public static System.Object GetParameter( string label, bool clear = true )
		{
			return EnhancedSceneManager.GetParameter( label, clear ) ;
		}

		/// <summary>
		/// 受け渡しパラメータを取得する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="label"></param>
		/// <param name="clear"></param>
		/// <returns></returns>
		public static T GetParameter<T>( string label, bool clear = true )
		{
			return EnhancedSceneManager.GetParameter<T>( label, clear ) ;
		}

		/// <summary>
		/// 受け渡しパラメータが存在するか確認する
		/// </summary>
		/// <param name="label"></param>
		/// <returns></returns>
		public static bool ContainsParameter( string label )
		{
			return EnhancedSceneManager.ContainsParameter( label ) ;
		}

		/// <summary>
		/// 受け渡しパラメータが存在するか確認する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="label"></param>
		/// <returns></returns>
		public static bool ContainsParameter<T>( string label )
		{
			return EnhancedSceneManager.ContainsParameter<T>( label ) ;
		}

		/// <summary>
		/// 受け渡しパラメータを削除する
		/// </summary>
		/// <param name="label"></param>
		/// <returns></returns>
		public static bool RemoveParameter( string label )
		{
			return EnhancedSceneManager.RemoveParameter( label ) ;
		}
	}
}
