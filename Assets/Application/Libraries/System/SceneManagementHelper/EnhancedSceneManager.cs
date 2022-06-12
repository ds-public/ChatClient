using UnityEngine ;
using System ;
using System.Collections ;
using System.Collections.Generic ;

#if UNITY_EDITOR
using UnityEditor ;
#endif

/// <summary>
/// シーンマネージメントヘルパーパッケージ
/// </summary>
namespace SceneManagementHelper
{
	/// <summary>
	/// エンハンスドシーンマネージャクラス Version 2021/02/16 0
	/// </summary>
	public class EnhancedSceneManager : MonoBehaviour
	{
#if UNITY_EDITOR
		/// <summary>
		/// EnhancedSceneManager を生成
		/// </summary>
		[MenuItem("GameObject/Helper/SceneManagementHelper/EnhancedSceneManager", false, 24)]
		public static void CreateEnhancedSceneManager()
		{
			GameObject go = new GameObject( "EnhancedSceneManager" ) ;
		
			Transform t = go.transform ;
			t.SetParent( null ) ;
			t.localPosition = Vector3.zero ;
			t.localRotation = Quaternion.identity ;
			t.localScale = Vector3.one ;
		
			go.AddComponent<EnhancedSceneManager>() ;
			Selection.activeGameObject = go ;
		}
#endif

		// シングルトンインスタンス
		private static EnhancedSceneManager m_Instance = null ;

		/// <summary>
		/// エンハンスドシーンマネージャのインスタンス
		/// </summary>
		public  static EnhancedSceneManager   Instance
		{
			get
			{
				return m_Instance ;
			}
		}
		
		//---------------------------------------------------------
	
		//---------------------------------------------------------
	
		/// <summary>
		/// エンハンスドシーンマネージャのインスタンスを生成する
		/// </summary>
		/// <returns>エンハンスドシーンマネージャのインスタンス</returns>
		public static EnhancedSceneManager Create( Transform parent = null )
		{
			if( m_Instance != null )
			{
				return m_Instance ;
			}
		
			// オブジェクトが非アクティブだと検出されないのでオブジェクトを非アクティブにしてはならない
			// この判定は必須で mInstance は static であるためシーンの最初はオブジェクトが存在しても null になっている
			m_Instance = GameObject.FindObjectOfType<EnhancedSceneManager>() ;
			if( m_Instance == null )
			{
				GameObject go = new GameObject( "EnhancedSceneManager" ) ;
				if( parent != null )
				{
					go.transform.SetParent( parent, false ) ;
				}

				go.AddComponent<EnhancedSceneManager>() ;
			}

			return m_Instance ;
		}
	
		/// <summary>
		/// エンハンスドシーンマネージャのインスタンスを破棄する
		/// </summary>
		public static void Delete()
		{	
			if( m_Instance != null )
			{
				if( Application.isPlaying == false )
				{
					DestroyImmediate( m_Instance.gameObject ) ;
				}
				else
				{
					Destroy( m_Instance.gameObject ) ;
				}
			
				m_Instance = null ;
			}
		}
	
		//-----------------------------------------------------------------
	
		internal void Awake()
		{
			// 既に存在し重複になる場合は自身を削除する
			if( m_Instance != null )
			{
				GameObject.DestroyImmediate( gameObject ) ;
				return ;
			}
		
			EnhancedSceneManager instanceOther = GameObject.FindObjectOfType<EnhancedSceneManager>() ;
			if( instanceOther != null )
			{
				if( instanceOther != this )
				{
					GameObject.DestroyImmediate( gameObject ) ;
					return ;
				}
			}
		
			//-----------------------------
			// Awake 内でマニュアルで実行した場合とスクリプトで実行した場合の共通の処理を行う必要がある
		
			m_Instance = this ;
			
			// シーン切り替え時に破棄されないようにする(ただし自身がルートである場合のみ有効)
			if( transform.parent == null )
			{
				DontDestroyOnLoad( gameObject ) ;
			}
		
			//-----------------------------
		
			// 原点じゃないと気持ち悪い
			gameObject.transform.localPosition = Vector3.zero ;
			gameObject.transform.localRotation = Quaternion.identity ;
			gameObject.transform.localScale = Vector3.one ;
		
			//-----------------------------

			// 履歴用のメモリを確保する
			m_History = new List<string>()
			{
				UnityEngine.SceneManagement.SceneManager.GetActiveScene().name	// 現在のシーンを最初に履歴に格納する
			} ;
		}

#if false
		internal void Update()
		{
			// ソースを監視して停止している（予定）
		}
#endif
		
		internal void OnDestroy()
		{
			if( m_Instance == this )
			{
				m_Instance  = null ;
			}
		}
	
		//-----------------------------------------------------------------

		// シーン間の受け渡し用のパラメータ
		private readonly Dictionary<string,System.Object> m_Parameter = new Dictionary<string,System.Object>() ;

		/// <summary>
		/// シーンロードの履歴(ReadOnlyにすると
		/// </summary>
		[SerializeField]
		private List<string> m_History ;

		[SerializeField]
		private string m_PreviousName ;

		/// <summary>
		/// １つ前のシーン名
		/// </summary>
		public string PreviousName
		{
			get
			{
				return m_PreviousName ;
			}
		}

		/// <summary>
		/// シーン間の受け渡しパラメータを設定する
		/// </summary>
		/// <param name="label">受け渡しパラメータの識別名</param>
		/// <param name="value">受け渡しパラメータをの値</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool SetParameter( string label, System.Object value )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.SetParameter_Private( label, value ) ;
		}

		// シーン間の受け渡しパラメータを保存する
		private bool SetParameter_Private( string label, System.Object value )
		{
			if( m_Parameter.ContainsKey( label ) == true )
			{
				m_Parameter[ label ] = value ;
				return true ;
			}
			else
			{
				m_Parameter.Add( label, value ) ;
				return false ;
			}
		}

		/// <summary>
		/// シーン間の受け渡しパラメータを取得する(任意の型にキャスト)
		/// </summary>
		/// <typeparam name="T">受け渡しパラメータの型</typeparam>
		/// <param name="label">受け渡しパラメータの識別名</param>
		/// <param name="clear">受け渡しパラメータを取得した後に受け渡しパラメータを消去するかどうか</param>
		/// <returns>受け渡しパラメータのインスタンス(任意の型にキャスト)</returns>
		public static T GetParameter<T>( string label, bool clear = true )
		{
			if( m_Instance == null )
			{
				return default ;
			}

			return m_Instance.GetParameter_Private<T>( label, clear ) ;
		}

		// シーン間の受け渡しパラメータを取得する(任意の型にキャスト)
		private T GetParameter_Private<T>( string label, bool clear )
		{
			if( m_Parameter.ContainsKey( label ) == false )
			{
				return default ;
			}

			T value = ( T )m_Parameter[ label ] ;

			if( clear == true )
			{
				m_Parameter.Remove( label ) ;
			}

			return value ;
		}

		/// <summary>
		/// シーン間の受け渡しパラメータを取得する
		/// </summary>
		/// <param name="label">受け渡しパラメータの識別名</param>
		/// <param name="clear">受け渡しパラメータを取得した後に受け渡しパラメータを消去するかどうか</param>
		/// <returns>パラメータの値</returns>
		public static System.Object GetParameter( string label, bool clear = true )
		{
			if( m_Instance == null )
			{
				return null ;
			}

			return m_Instance.GetParameter_Private( label, clear ) ;
		}

		// シーン間の受け渡しパラメータを取得する
		private System.Object GetParameter_Private( string label, bool clear )
		{
			if( m_Parameter.ContainsKey( label ) == false )
			{
				return null ;
			}

			System.Object value = m_Parameter[ label ] ;

			if( clear == true )
			{
				m_Parameter.Remove( label ) ;
			}

			return value ;
		}

		/// <summary>
		/// シーン間の受け渡しパラメータの存在を確認するする
		/// </summary>
		/// <param name="label">受け渡しパラメータの識別名</param>
		/// <returns>結果(true=存在する・false=存在しない)</returns>
		public static bool ContainsParameter( string label )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.ContainsParameter_Private( label ) ;
		}

		// シーン間の受け渡しパラメータの存在を確認する
		private bool ContainsParameter_Private( string label )
		{
			if( m_Parameter.ContainsKey( label ) == false )
			{
				return false ;
			}

			return true ;
		}

		/// <summary>
		/// シーン間の受け渡しパラメータの存在を確認するする
		/// </summary>
		/// <param name="label">受け渡しパラメータの識別名</param>
		/// <returns>結果(true=存在する・false=存在しない)</returns>
		public static bool ContainsParameter<T>( string label )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.ContainsParameter_Private<T>( label ) ;
		}

		// シーン間の受け渡しパラメータの存在を確認する
		private bool ContainsParameter_Private<T>( string label )
		{
			if( m_Parameter.ContainsKey( label ) == false )
			{
				return false ;
			}

			if( m_Parameter[ label ] is T )
			{
				return true ;
			}

			return false ;
		}

		/// <summary>
		/// シーン間の受け渡しパラメータを削除する
		/// </summary>
		/// <param name="label">受け渡しパラメータの識別名</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool RemoveParameter( string label )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.RemoveParameter_Private( label ) ;
		}

		// シーン間の受け渡しパラメータを削除する
		private bool RemoveParameter_Private( string label )
		{
			if( m_Parameter.ContainsKey( label ) == true )
			{
				m_Parameter.Remove( label ) ;
				return true ;
			}
			else
			{
				return false ;
			}
		}

		/// <summary>
		/// シーン間の受け渡しパラメータを全て消去する
		/// </summary>
		/// <returns></returns>
		public static bool ClearParameter()
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.ClearParameter_Private() ;

		}

		// シーン間の受け渡しパラメータを全て消去する
		private bool ClearParameter_Private()
		{
			m_Parameter.Clear() ;

			return true ;
		}

		/// <summary>
		/// シーンの遷移の履歴を消去する
		/// </summary>
		/// <returns></returns>
		public static bool ClearHistory()
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.ClearHistory_Private() ;
		}

		// シーンの遷移の履歴を消去する
		private bool ClearHistory_Private()
		{
			m_History.Clear() ;
			m_History.Add( UnityEngine.SceneManagement.SceneManager.GetActiveScene().name ) ;

			return true ;
		}

		//-----------------------------------------------------------------

		/// <summary>
		/// リクエスト待ちクラス
		/// </summary>
		public class Request : CustomYieldInstruction
		{
			private readonly MonoBehaviour m_Owner = default ;
			public Request( MonoBehaviour owner )
			{
				// 自身が削除された際にコルーチンの終了待ちをブレイクする施策
				m_Owner = owner ;
			}

			public override bool keepWaiting
			{
				get
				{
					if( IsDone == false && string.IsNullOrEmpty( Error ) == true && m_Owner != null && m_Owner.gameObject.activeInHierarchy == true )
					{
						return true ;    // 継続
					}
					return false ;   // 終了
				}
			}

			/// <summary>
			/// 通信が終了したかどうか
			/// </summary>
			public bool IsDone = false ;

			/// <summary>
			/// エラーメッセージ
			/// </summary>
			public string	Error = string.Empty ;

			/// <summary>
			/// 
			/// </summary>
			public UnityEngine.Object[]		Instances = null ;
		}

		//-----------------------------------------------------------
		
		/// <summary>
		/// シーンをロードする(同期版)
		/// </summary>
		/// <param name="sceneName">シーン名</param>
		/// <param name="label">受け渡しパラメータの識別名</param>
		/// <param name="value">受け渡しパラメータのインスタンス</param>
		/// <returns>結果((true=成功・false=失敗)</returns>
		public static bool Load( string sceneName, string label = null, System.Object value = null )
		{
			if( m_Instance == null )
			{
				Create() ;
			}

			return m_Instance.LoadOrAdd_Private( sceneName, null, null, null, label, value, UnityEngine.SceneManagement.LoadSceneMode.Single, true ) ;
		}

		/// <summary>
		/// シーンをロードする(同期版)
		/// </summary>
		/// <param name="sceneName">シーン名</param>
		/// <param name="onLoaded">シーン内の特定のコンポーネントのインスタンスを取得するコールバック</param>
		/// <param name="targetName">該当のコンポーネントをさらに名前により絞り込む</param>
		/// <param name="label">受け渡しパラメータの識別名</param>
		/// <param name="value">受け渡しパラメータのインスタンス</param>
		/// <returns>結果((true=成功・false=失敗)</returns>
		public static bool Load<T>( string sceneName, Action<T[]> onLoaded, string targetName = null, string label = null, System.Object value = null ) where T : UnityEngine.Component
		{
			if( m_Instance == null )
			{
				Create() ;
			}

			return m_Instance.LoadOrAdd_Private
			(
				sceneName, typeof( T ),
				( UnityEngine.Object[] temporaryTargets ) =>
				{
					if( onLoaded != null )
					{
						int i, l = temporaryTargets.Length ;
						T[] targets = new T[ l ] ;
						for( i  = 0 ; i <  l ; i ++ )
						{
							targets[ i ] = temporaryTargets[ i ] as T ;
						}
						onLoaded( targets ) ;
					}
				},
				targetName, label, value, UnityEngine.SceneManagement.LoadSceneMode.Single, true
			) ;
		}

		/// <summary>
		/// シーンをロードする(非同期版)　※このメソッドは常駐済みのコンポーネントで呼び出すこと
		/// </summary>
		/// <typeparam name="T">シーン内の特定のコンポーネント型</typeparam>
		/// <param name="sceneName">シーン名</param>
		/// <param name="label">受け渡しパラメータの識別名</param>
		/// <param name="value">受け渡しパラメータのインスタンス</param>
		/// <returns>列挙子</returns>
		public static Request LoadAsync( string sceneName, string label = null, System.Object value = null )
		{
			if( m_Instance == null )
			{
				Create() ;
			}

			Request request = new Request( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.LoadOrAddAsync_Private( sceneName, null, null, null, label, value, UnityEngine.SceneManagement.LoadSceneMode.Single, true, request ) ) ;
			return request ;
		}

		/// <summary>
		/// シーンをロードする(非同期版)　※このメソッドは常駐済みのコンポーネントで呼び出すこと
		/// </summary>
		/// <typeparam name="T">シーン内の特定のコンポーネント型</typeparam>
		/// <param name="sceneName">シーン名</param>
		/// <param name="onLoaded">シーン内の特定のコンポーネントのインスタンスを取得するコールバック</param>
		/// <param name="targetName">該当のコンポーネントをさらに名前により絞り込む</param>
		/// <param name="label">受け渡しパラメータの識別名</param>
		/// <param name="value">受け渡しパラメータのインスタンス</param>
		/// <returns>列挙子</returns>
		public static Request LoadAsync<T>( string sceneName, Action<T[]> onLoaded, string targetName = null, string label = null, System.Object value = null ) where T : UnityEngine.Component
		{
			if( m_Instance == null )
			{
				Create() ;
			}

			Request request = new Request( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.LoadOrAddAsync_Private
			(
				sceneName, typeof( T ),
				( UnityEngine.Object[] temporaryTargets ) =>
				{
					if( onLoaded != null )
					{
						int i, l = temporaryTargets.Length ;
						T[] targets = new T[ l ] ;
						for( i  = 0 ; i <  l ; i ++ )
						{
							targets[ i ] = temporaryTargets[ i ] as T ;
						}
						onLoaded( targets ) ;
					}
				},
				targetName, label, value, UnityEngine.SceneManagement.LoadSceneMode.Single, true, request
			) ) ;
			return request ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// シーンを加算する(同期版)
		/// </summary>
		/// <param name="sceneName">シーン名</param>
		/// <param name="label">受け渡しパラメータの識別名</param>
		/// <param name="value">受け渡しパラメータのインスタンス</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Add( string sceneName, string label = null, System.Object value = null )
		{
			if( m_Instance == null )
			{
				Create() ;
			}

			return m_Instance.LoadOrAdd_Private( sceneName, null, null, null, label, value, UnityEngine.SceneManagement.LoadSceneMode.Additive, false ) ;
		}

		/// <summary>
		/// シーンを加算する(同期版)
		/// </summary>
		/// <param name="sceneName">シーン名</param>
		/// <param name="onLoaded">シーン内の特定のコンポーネントのインスタンスを取得するコールバック</param>
		/// <param name="targetName">該当のコンポーネントをさらに名前により絞り込む</param>
		/// <param name="label">受け渡しパラメータの識別名</param>
		/// <param name="value">受け渡しパラメータのインスタンス</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Add<T>( string sceneName, Action<T[]> onLoaded, string targetName = null, string label = null, System.Object value = null ) where T : UnityEngine.Component
		{
			if( m_Instance == null )
			{
				Create() ;
			}

			return m_Instance.LoadOrAdd_Private
			(
				sceneName, typeof( T ),
				( UnityEngine.Object[] temporaryTargets ) =>
				{
					if( onLoaded != null )
					{
						int i, l = temporaryTargets.Length ;
						T[] targets = new T[ l ] ;
						for( i  = 0 ; i <  l ; i ++ )
						{
							targets[ i ] = temporaryTargets[ i ] as T ;
						}
						onLoaded( targets ) ;
					}
				},
				targetName, label, value, UnityEngine.SceneManagement.LoadSceneMode.Additive, false
			) ;
		}

		/// <summary>
		/// シーンを加算する(非同期版)
		/// </summary>
		/// <param name="sceneName">シーン名</param>
		/// <param name="label">受け渡しパラメータの識別名</param>
		/// <param name="value">受け渡しパラメータのインスタンス</param>
		/// <returns>列挙子</returns>
		public static Request AddAsync( string sceneName, string label = null, System.Object value = null )
		{
			if( m_Instance == null )
			{
				Create() ;
			}

			Request request = new Request( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.LoadOrAddAsync_Private( sceneName, null, null, null, label, value, UnityEngine.SceneManagement.LoadSceneMode.Additive, false, request ) ) ;
			return request ;
		}
		
		/// <summary>
		/// シーンを加算する(非同期版)
		/// </summary>
		/// <param name="sceneName">シーン名</param>
		/// <param name="onLoaded">シーン内の特定のコンポーネントのインスタンスを取得するコールバック</param>
		/// <param name="targetName">該当のコンポーネントをさらに名前により絞り込む</param>
		/// <param name="label">受け渡しパラメータの識別名</param>
		/// <param name="value">受け渡しパラメータのインスタンス</param>
		/// <returns>列挙子</returns>
		public static Request AddAsync<T>( string sceneName, Action<T[]> onLoaded, string targetName = null, string label = null, System.Object value = null ) where T : UnityEngine.Component
		{
			if( m_Instance == null )
			{
				Create() ;
			}

			Request request = new Request( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.LoadOrAddAsync_Private
			(
				sceneName, typeof( T ),
				( UnityEngine.Object[] temporaryTargets ) =>
				{
					if( onLoaded != null )
					{
						int i, l = temporaryTargets.Length ;
						T[] targets = new T[ l ] ;
						for( i  = 0 ; i <  l ; i ++ )
						{
							targets[ i ] = temporaryTargets[ i ] as T ;
						}
						onLoaded( targets ) ;
					}
				},
				targetName, label, value, UnityEngine.SceneManagement.LoadSceneMode.Additive, false, request
			) ) ;
			return request ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 現在のシーンの１つ前にロードされていてシーンをロードする(同期版)
		/// </summary>
		/// <param name="label">受け渡しパラメータの識別名</param>
		/// <param name="value">受け渡しパラメータのインスタンス</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Back( string label = null, System.Object value = null )
		{
			if( m_Instance == null )
			{
				Create() ;
			}

			if( m_Instance.m_History.Count <= 1 )
			{
				return true ;	// １つ前のシーンは存在しない
			}

			int c = m_Instance.m_History.Count ;
			string sceneName = m_Instance.m_History[ c - 2 ] ;
			m_Instance.m_History.RemoveAt( c - 1 ) ;
			
			return m_Instance.LoadOrAdd_Private( sceneName, null, null, null, label, value, UnityEngine.SceneManagement.LoadSceneMode.Single, false ) ;
		}

		/// <summary>
		/// 現在のシーンの１つ前にロードされていてシーンをロードする(同期版)
		/// </summary>
		/// <param name="onLoaded">シーン内の特定のコンポーネントのインスタンスを取得するコールバック</param>
		/// <param name="targetName">該当のコンポーネントをさらに名前により絞り込む</param>
		/// <param name="label">受け渡しパラメータの識別名</param>
		/// <param name="value">受け渡しパラメータのインスタンス</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Back<T>( Action<T[]> onLoaded, string targetName = null, string label = null, System.Object value = null ) where T : UnityEngine.Component
		{
			if( m_Instance == null )
			{
				Create() ;
			}

			if( m_Instance.m_History.Count <= 1 )
			{
				return true ;	// １つ前のシーンは存在しない
			}

			int c = m_Instance.m_History.Count ;
			string sceneName = m_Instance.m_History[ c - 2 ] ;
			m_Instance.m_History.RemoveAt( c - 1 ) ;
			
			return m_Instance.LoadOrAdd_Private
			(
				sceneName, typeof( T ),
				( UnityEngine.Object[] temporaryTargets ) =>
				{
					if( onLoaded != null )
					{
						int i, l = temporaryTargets.Length ;
						T[] targets = new T[ l ] ;
						for( i  = 0 ; i <  l ; i ++ )
						{
							targets[ i ] = temporaryTargets[ i ] as T ;
						}
						onLoaded( targets ) ;
					}
				},
				targetName, label, value, UnityEngine.SceneManagement.LoadSceneMode.Single, false
			) ;
		}

		/// <summary>
		/// 現在のシーンの１つ前にロードされていてシーンをロードする(非同期版)
		/// </summary>
		/// <param name="label">受け渡しパラメータの識別名</param>
		/// <param name="value">受け渡しパラメータのインスタンス</param>
		/// <returns>列挙子</returns>
		public static Request BackAsync( string label = null, System.Object value = null )
		{
			if( m_Instance == null )
			{
				Create() ;
			}

			Request request = new Request( m_Instance ) ;

			if( m_Instance.m_History.Count <= 1 )
			{
				request.IsDone = true ;
				return request ;	// １つ前のシーンは存在しない
			}

			int c = m_Instance.m_History.Count ;
			string sceneName = m_Instance.m_History[ c - 2 ] ;
			m_Instance.m_History.RemoveAt( c - 1 ) ;
			
			m_Instance.StartCoroutine( m_Instance.LoadOrAddAsync_Private( sceneName, null, null, null, label, value, UnityEngine.SceneManagement.LoadSceneMode.Single, false, request ) ) ;

			return request ;
		}

		/// <summary>
		/// 現在のシーンの１つ前にロードされていてシーンをロードする(非同期版)
		/// </summary>
		/// <param name="onLoaded">シーン内の特定のコンポーネントのインスタンスを取得するコールバック</param>
		/// <param name="targetName">該当のコンポーネントをさらに名前により絞り込む</param>
		/// <param name="label">受け渡しパラメータの識別名</param>
		/// <param name="value">受け渡しパラメータのインスタンス</param>
		/// <returns>列挙子</returns>
		public static Request BackAsync<T>( Action<T[]> onLoaded, string targetName = null, string label = null, System.Object value = null ) where T : UnityEngine.Component
		{
			if( m_Instance == null )
			{
				Create() ;
			}

			Request request = new Request( m_Instance ) ;

			if( m_Instance.m_History.Count <= 1 )
			{
				request.IsDone = true ;
				return request ;	// １つ前のシーンは存在しない
			}

			int c = m_Instance.m_History.Count ;
			string sceneName = m_Instance.m_History[ c - 2 ] ;
			m_Instance.m_History.RemoveAt( c - 1 ) ;
			
			m_Instance.StartCoroutine( m_Instance.LoadOrAddAsync_Private
			(
				sceneName, typeof( T ),
				( UnityEngine.Object[] temporaryTargets ) =>
				{
					if( onLoaded != null )
					{
						int i, l = temporaryTargets.Length ;
						T[] targets = new T[ l ] ;
						for( i  = 0 ; i <  l ; i ++ )
						{
							targets[ i ] = temporaryTargets[ i ] as T ;
						}
						onLoaded( targets ) ;
					}
				},
				targetName, label, value, UnityEngine.SceneManagement.LoadSceneMode.Single, false, request
			) ) ;

			return request ;
		}

		//-----------------------------------------------------------

		// シーンをロードまたは加算する(同期版)
		private bool LoadOrAdd_Private( string sceneName, Type type, Action<UnityEngine.Object[]> onLoaded, string targetName, string label, System.Object value, UnityEngine.SceneManagement.LoadSceneMode mode, bool useHistory )
		{
			if( string.IsNullOrEmpty( sceneName ) == true )
			{
				return false ;
			}
			
			if( string.IsNullOrEmpty( label ) == false )
			{
				SetParameter_Private( label, value ) ;
			}

			if( mode == UnityEngine.SceneManagement.LoadSceneMode.Single )
			{
				// ロードの場合は履歴に追加する
				m_PreviousName = GetActiveName() ;

				if( useHistory == true )
				{
					m_History.Add( sceneName ) ;
				}
			}
			
			UnityEngine.SceneManagement.SceneManager.LoadScene( sceneName, mode ) ;

			//------------------------------------------------------------------------------------------

			UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName( sceneName ) ;
			if( scene.IsValid() == false || scene.isLoaded == false )
			{
				return false ;
			}

			if( type != null && onLoaded != null )
			{
				GetInstance_Private( scene, type, onLoaded, targetName, null ) ;
			}

			return true ;
		}

		// シーンをロードまたは加算する(非同期版)
		private IEnumerator LoadOrAddAsync_Private( string sceneName, Type type, Action<UnityEngine.Object[]> onLoaded, string targetName, string label, System.Object value, UnityEngine.SceneManagement.LoadSceneMode mode, bool useHistory, Request request )
		{
			if( string.IsNullOrEmpty( sceneName ) == true )
			{
				request.Error = "Bad scene name." ;
				yield break ;
			}
			
			//----------------------------------------------------------

			if( type != null )
			{
				// 指定の型のコンポーネントが存在する場合はそれが完全に消滅するまで待つ
				while( true )
				{
					if( GameObject.FindObjectOfType( type ) == null )
					{
						break ;
					}
					yield return null ;
				}
			}

			//----------------------------------------------------------

			if( string.IsNullOrEmpty( label ) == false )
			{
				SetParameter_Private( label, value ) ;
			}

			if( mode == UnityEngine.SceneManagement.LoadSceneMode.Single )
			{
				// ロードの場合は履歴に追加する
				m_PreviousName = GetActiveName() ;

				if( useHistory == true )
				{
					m_History.Add( sceneName ) ;
				}
			}

			yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync( sceneName, mode ) ;
			
			//------------------------------------------------------------------------------------------

			UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName( sceneName ) ;
			if( scene.IsValid() == false )
			{
				request.Error = "Could not load." ;
				yield break ;
			}

			// シーンの展開が完了するのを待つ
			yield return new WaitWhile( () => scene.isLoaded == false ) ;

			if( type != null && onLoaded != null )
			{
				GetInstance_Private( scene, type, onLoaded, targetName, request ) ;
			}

			request.IsDone = true ;
		}

		//-----------------------------------

		private void GetInstance_Private( UnityEngine.SceneManagement.Scene scene, Type type, Action<UnityEngine.Object[]> onLoaded, string targetName, Request request )
		{
			// 指定の型のコンポーネントを探してインスタンスを取得する
			List<UnityEngine.Object> targets = new List<UnityEngine.Object>() ;
			UnityEngine.Object[] temporaryTargets ;

			GameObject[] gos = scene.GetRootGameObjects() ;
			if( gos != null && gos.Length >  0 )
			{
				foreach( var go in gos )
				{
					temporaryTargets = go.GetComponentsInChildren( type, true ) ;
					if( temporaryTargets != null && temporaryTargets.Length >  0 )
					{
						foreach( var target in temporaryTargets )
						{
							targets.Add( target ) ;
						}
					}
				}
			}

			if( targets.Count == 0 )
			{
				return ;	// 該当無し
			}

			if( string.IsNullOrEmpty( targetName ) == false )
			{
				// 名前によるフィルタ有り
				List<UnityEngine.Object> filteredTargets = new List<UnityEngine.Object>() ;
				foreach( var target in targets )
				{
					if( target.name == targetName )
					{
						filteredTargets.Add( target ) ;
					}
				}

				if( filteredTargets.Count == 0 )
				{
					return ;	// 該当無し
				}

				targets = filteredTargets ;
			}

			temporaryTargets = targets.ToArray() ;
			request.Instances = temporaryTargets ;
			onLoaded?.Invoke( temporaryTargets ) ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 指定の名前のシーンがロード中か確認する
		/// </summary>
		/// <param name="sceneName"></param>
		/// <returns></returns>
		public static bool IsLoaded( string sceneName )
		{
			UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName( sceneName ) ;
			
			return scene.isLoaded ;
		}
		
		//-----------------------------------------------------------

		/// <summary>
		/// ロードまたは加算されたシーンを破棄する(同期版)
		/// </summary>
		/// <param name="sceneName">シーン名</param>
		/// <param name="label">受け渡しパラメータの識別名</param>
		/// <param name="value">受け渡しパラメータのインスタンス</param>
		/// <returns>結果(true=成功・false=失敗</returns>
		public static bool Remove( string sceneName, string label = null, System.Object value = null )
		{
			if( m_Instance == null )
			{
				Create() ;
			}

			return m_Instance.Remove_Private( sceneName, label, value ) ;
		}

		// ロードまたは加算されたシーンを破棄する(実際は非同期になってしまう)
		private bool Remove_Private( string sceneName, string label, System.Object value )
		{
			if( string.IsNullOrEmpty( sceneName ) == true )
			{
				return false ;
			}
			
			UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName( sceneName ) ;
			if( scene.isLoaded == false )
			{
				// そのようなシーンは実際は存在しない
				return false ;
			}

			if( string.IsNullOrEmpty( label ) == false )
			{
				SetParameter_Private( label, value ) ;
			}

			// 同期メソッドが廃止されてしまった
			// 最後にロードしたシーンを破棄しようとすると警告が出る
			UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync( sceneName ) ;
			return true ;
		}

		//-----------------------------------

		/// <summary>
		/// ロードまたは加算されたシーンを破棄する(非同期版)
		/// </summary>
		/// <param name="sceneName">シーン名</param>
		/// <param name="onResult">結果を取得するコールバック</param>
		/// <param name="label">受け渡しパラメータの識別名</param>
		/// <param name="value">受け渡しパラメータのインスタンス</param>
		/// <returns>列挙子</returns>
		public static Request RemoveAsync( string sceneName, Action<bool> onResult, string label = null, System.Object value = null )
		{
			if( m_Instance == null )
			{
				Create() ;
			}

			Request request = new Request( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.RemoveAsync_Private( sceneName, onResult, label, value, request ) ) ;
			return request ;
		}

		// ロードまたは加算されたシーンを破棄する(非同期版)
		private IEnumerator RemoveAsync_Private( string sceneName, Action<bool> onResult, string label, System.Object value, Request request )
		{
			if( string.IsNullOrEmpty( sceneName ) == true )
			{
				request.Error = "Could not remove." ;
				onResult?.Invoke( false ) ;
				yield break ;
			}
			
			UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName( sceneName ) ;
			if( scene.isLoaded == false )
			{
				// そのようなシーンは実際は存在しない
				request.Error = "Could not remove." ;
				onResult?.Invoke( false ) ;
				yield break ;
			}

			if( string.IsNullOrEmpty( label ) == false )
			{
				SetParameter_Private( label, value ) ;
			}

			// 最後にロードしたシーンを破棄しようとすると警告が出る
			AsyncOperation asyncOperation = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync( sceneName ) ;
			yield return asyncOperation ;

			if( asyncOperation != null && asyncOperation.isDone == true )
			{
				request.IsDone = true ;
				onResult?.Invoke( true ) ;
			}
			else
			{
				request.Error = "could not remove" ;
				onResult?.Invoke( false ) ;
			}
		}
		
		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// １つ前のシーンの名前を取得する
		/// </summary>
		/// <returns>１つ前のシーンの名前(nullで存在しない)</returns>
		public static string GetPreviousName()
		{
			if( m_Instance == null )
			{
				return null ;
			}

			return m_Instance.GetPreviousName_Private() ;
		}

		// １つ前のシーンの名前を取得する
		private string GetPreviousName_Private()
		{
//			if( history.Count <= 1 )
//			{
//				return null ;
//			}
//
//			int c = history.Count ;
//			return history[ c - 2 ] ;
			return m_PreviousName ;
		}
		
		/// <summary>
		/// 現在のシーン名を取得する
		/// </summary>
		/// <returns></returns>
		public static string GetActiveName()
		{
			return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name ;
		}

		//-----------------------------------------------------------------
	}
}


