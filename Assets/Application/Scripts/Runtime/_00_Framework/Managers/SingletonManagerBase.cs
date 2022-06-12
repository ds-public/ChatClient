using UnityEngine ;

#if UNITY_EDITOR
using UnityEditor ;

#endif

namespace Template
{
	/// <summary>
	/// マネージャの基底クラス	Version 2020/10/29 0
	/// </summary>
	public class SingletonManagerBase<T> : ExMonoBehaviour where T : ExMonoBehaviour
	{
		/// <summary>
		/// シングルトンマネージャのインスタンス
		/// </summary>
		protected static T m_Instance ; 

		/// <summary>
		/// シングルトンマネージャのインスタンス
		/// </summary>
		public  static T   Instance
		{
			get
			{
#if UNITY_EDITOR
				if ( ( m_Instance == null ) && !Application.isPlaying )
				{
					string className = typeof( T ).ToString() ;
					int    i         = className.LastIndexOf( '.' ) ;
					if( i >= 0 )
					{
						className = className.Substring( i + 1, className.Length - ( i + 1 ) ) ;
					}

					GameObject go = EditorUtility.CreateGameObjectWithHideFlags(
						className,
						HideFlags.DontSave,
						typeof(T)
					) ;
					go.tag     = "EditorOnly" ;
					m_Instance = go.GetComponent<T>() ;
				}
#endif
				return m_Instance ;
			}
		}

		/// <summary>
		/// シングルトンマネージャを生成する
		/// </summary>
		/// <returns>フェイシャルマネージャのインスタンス</returns>
		public static T Create( Transform parent = null )
		{
			if( m_Instance != null )
			{
				return m_Instance ;
			}

			m_Instance = FindObjectOfType( typeof( T ) ) as T;
			if( m_Instance == null )
			{
				string className = typeof( T ).ToString() ;
				int i = className.LastIndexOf( '.' ) ;
				if( i >= 0 )
				{
					className = className.Substring( i + 1, className.Length - ( i + 1 ) ) ;
				}
				GameObject go = new GameObject( className ) ;
				if( parent != null )
				{
					go.transform.SetParent( parent, false ) ;
				}
				
				m_Instance = go.AddComponent<T>() ;
			}

			return m_Instance ;
		}

		/// <summary>
		/// シングルトンマネージャを破棄する
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

		//-----------------------------------------------------------

		// 生成
		protected void Awake()
		{
			// 既に存在し重複になる場合は自身を削除する
			if( m_Instance != null )
			{
				DestroyImmediate( gameObject ) ;
				return ;
			}
		
			T instanceOther = FindObjectOfType( typeof( T ) ) as T ;
			if( instanceOther != null )
			{
				if( instanceOther != this )
				{
					DestroyImmediate( gameObject ) ;
					return ;
				}
			}
		
			//-----------------------------
			// Awake 内でマニュアルで実行した場合とスクリプトで実行した場合の共通の処理を行う必要がある
		
			m_Instance = this as T ;
		
			// シーン切り替え時に破棄されないようにする(ただし自身がルートである場合のみ有効)
			if( transform.parent == null )
			{
				DontDestroyOnLoad( gameObject ) ;
			}

	//		gameObject.hideFlags = HideFlags.HideInHierarchy ;
		
			//-----------------------------
		
			// 原点じゃないと気持ち悪い
			gameObject.transform.localPosition	= Vector3.zero ;
			gameObject.transform.localRotation	= Quaternion.identity ;
			gameObject.transform.localScale		= Vector3.one ;

			//----------------------------

			// 派生クラスの Awake を呼ぶ
//			OnAwake() ;
		}

		/// <summary>
		/// 派生クラスの Awake
		/// </summary>
//		virtual protected void OnAwake(){}


		// 破棄された際に呼び出される
		internal void OnDestroy()
		{
			// 派生クラスの破棄を呼ぶ
			OnTerminate() ;

			//----------------------------

			if( m_Instance == this )
			{
				m_Instance  = null ;	// 最後の１つが破棄されたのでシングルトンのインスタンスをクリアする
			}
		}

		/// <summary>
		/// 派生クラスの破棄
		/// </summary>
		virtual protected void OnTerminate(){}
	}
}
