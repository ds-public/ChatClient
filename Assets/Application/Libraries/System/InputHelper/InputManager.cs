using UnityEngine ;
using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine.Assertions ;
using System.Linq ;

#if UNITY_EDITOR
using UnityEditor ;
#endif

namespace InputHelper
{
	/// <summary>
	/// 入力操作クラス Version 2021/04/26 0
	/// </summary>
	public class InputManager : MonoBehaviour
	{
		// 注意：パッド系の操作をした場合、一度パッド系の操作を完全解除するまで、実際のバッド系入力ができないようにする事も可能になっている。→modeEnabled

#if UNITY_EDITOR
		/// <summary>
		/// InputManager を生成
		/// </summary>
		[ MenuItem( "GameObject/Helper/InputHelper/InputManager", false, 24 ) ]
		public static void CreateInputManager()
		{
			GameObject go = new GameObject( "InputManager" ) ;
		
			Transform t = go.transform ;
			t.SetParent( null ) ;
			t.localPosition = Vector3.zero ;
			t.localRotation = Quaternion.identity ;
			t.localScale = Vector3.one ;
		
			go.AddComponent<InputManager>() ;
			Selection.activeGameObject = go ;
		}
#endif

		// インプットマネージャのインスタンス(シングルトン)
		private static InputManager m_Instance = null ; 

		/// <summary>
		/// インプットマネージャのインスタンス
		/// </summary>
		public  static InputManager   Instance
		{
			get
			{
				return m_Instance ;
			}
		}
	
		/// <summary>
		/// インプットマネージャのインスタンスを生成する
		/// </summary>
		/// <param name="tRunInbackground">バックグラウンドで再生させるようにするかどうか</param>
		/// <returns>インプットマネージャのインスタンス</returns>
		public static InputManager Create( Transform parent = null )
		{
			if( m_Instance != null )
			{
				return m_Instance ;
			}
			
#if UNITY_EDITOR
			if( InputManagerSettings.Check() == false )
			{
				Debug.LogWarning( "InputManager[Edit->Project Settings->Input]に必要なパラメータが設定されていません\n[Tools->Initialize InputManager]を実行してください" ) ;
				return null ;
			}
#endif
			// オブジェクトが非アクティブだと検出されないのでオブジェクトを非アクティブにしてはならない
			// この判定は必須で mInstance は static であるためシーンの最初はオブジェクトが存在しても null になっている
			m_Instance = GameObject.FindObjectOfType( typeof( InputManager ) ) as InputManager ;
			if( m_Instance == null )
			{
				GameObject go = new GameObject( "InputManager" ) ;
				if( parent != null )
				{
					go.transform.SetParent( parent, false ) ;
				}

				go.AddComponent<InputManager>() ;
			}

			return m_Instance ;
		}
	
		/// <summary>
		/// インプットマネージャのインスタンスを破棄する
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
		
			InputManager instanceOther = GameObject.FindObjectOfType( typeof( InputManager ) ) as InputManager ;
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
		
	//		gameObject.hideFlags = HideFlags.HideInHierarchy ;
		
			//-----------------------------
		
			// 原点じゃないと気持ち悪い
			gameObject.transform.localPosition = Vector3.zero ;
			gameObject.transform.localRotation = Quaternion.identity ;
			gameObject.transform.localScale = Vector3.one ;
		}

		internal void OnDestroy()
		{
			if( m_Instance == this )
			{
				m_Instance  = null ;
			}
		}
	
		//---------------------------------------------------------------------------

		/// <summary>
		/// 入力タイプ
		/// </summary>
		public enum InputTypes
		{
			Unknown = -1,
			Pointer	=  0,	// ポインター(マウス・タッチ)
			GamePad	=  1,	// ゲームパッド
		}

		private InputTypes m_InputType = InputTypes.Pointer ;	// デフォルトはポインターモード
		private bool	m_Hold = false ;
		private float	m_Tick = 0 ;

		private Vector3 m_Position = Vector3.zero ;

		private Action<InputTypes>	m_OnInputTypeChanged = null ;

		public delegate void OnInputTypeChangedDelegate( InputTypes inputType ) ;

		private OnInputTypeChangedDelegate	m_OnInputTypeChangedDelegate ;

		/// <summary>
		/// 縦軸の符号反転
		/// </summary>
		public static bool Invert = false ;	

		//---------------------------------------------------------------------------

		/// <summary>
		/// 現在のモードが実際に有効かどうか
		/// </summary>
		public static bool InputEnabled
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}

				return ! m_Instance.m_Hold ;
			}
		}

		public static InputTypes InputType
		{
			get
			{
				if( m_Instance == null )
				{
					return InputTypes.Unknown ;
				}

				return m_Instance.m_InputType ;
			}
		}

		//---------------------------------------------------------------------------

		// m_Enabled という名前は MonoBehaviour で定義されているので使ってはいけない
		private bool m_EnabledFlag = true ;

		/// <summary>
		/// 有効にする
		/// </summary>
		public static void Enable()
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.m_EnabledFlag = true ;
		}

		/// <summary>
		/// 無効にする
		/// </summary>
		public static void Disable()
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.m_EnabledFlag = false ;
		}

		/// <summary>
		/// 有効無効状態
		/// </summary>
		public static bool IsEnabled
		{
			get
			{
				if(  m_Instance == null )
				{
					return false ;
				}

				return m_Instance.m_EnabledFlag ;
			}
		}
		
		//---------------------------------------------------------------------------

		/// <summary>
		/// 状態に変化があった際にコールバックで通知するための定義クラス
		/// </summary>
		public class Updater
		{
			public Action<System.Object>	Action = null ;
			public System.Object			Option = null ;

			public Updater( Action<System.Object> action, System.Object option )
			{
				Action = action ;
				Option = option ;
			}
		}

		private readonly Stack<Updater>	m_Updater = new Stack<Updater>() ;

		//-----------------------------------------------------------

		/// <summary>
		/// GamePadUpdater を登録する
		/// </summary>
		/// <param name="action"></param>
		/// <param name="option"></param>
		/// <returns></returns>
		public static bool AddUpdater( Action<System.Object> action, System.Object option = null )
		{
			if( InputManager.m_Instance == null )
			{
				return false ;
			}

			return InputManager.m_Instance.AddUpdater_Private( action, option ) ;
		}
		
		private bool AddUpdater_Private( Action<System.Object> action, System.Object option )
		{
			if( action == null )
			{
				return false ;
			}

			m_Updater.Push( new Updater( action, option ) ) ;

			return true ;
		}

		/// <summary>
		/// GamePadUpdater を除去する
		/// </summary>
		/// <param name="action"></param>
		/// <returns></returns>
		public static bool RemoveUpdater( Action<System.Object> action = null )
		{
			if( InputManager.m_Instance == null )
			{
				return false ;
			}

			return InputManager.m_Instance.RemoveUpdater_Private( action ) ;
		}

		private bool RemoveUpdater_Private( Action<System.Object> action )
		{
			if( m_Updater.Count == 0 )
			{
				return false ;	// キューには何も積まれていない
			}

			if( action != null )
			{
				// 間違った GamePadUpdater を除去しないように確認する

				Updater stateUpdater = m_Updater.Peek() ;
				if( stateUpdater.Action != action )
				{
					// 違う！
					return false ;
				}
			}
			
			m_Updater.Pop() ;

			return true ;
		}

		//---------------------------------------------------------------------------

		internal void LateUpdate()
		{
			if( m_EnabledFlag == false )
			{
				return ;
			}

			//----------------------------------------------------------

			if( m_InputType == InputTypes.Pointer )
			{
				// 現在は Pointer モード
				if( m_Hold == true )
				{
					// 切り替えた直後は一度全開放しないと入力できない
					if( Input.GetMouseButton( 0 ) == false && Input.GetMouseButton( 1 ) == false && Input.GetMouseButton( 2 ) == false )
					{
						m_Hold = false ;
					}
					else
					{
						// 入力し続けても１秒経過したら強制解除する
						m_Tick += Time.unscaledDeltaTime ;
						if( m_Tick >= 1 )
						{
							m_Hold = false ;
						}
					}
				}
				else
				{
					// Pointer モード有効中
					Pointer.Update() ;

//					Debug.LogWarning( "タッチモード中" ) ;
					Vector2 axis_0 = GamePad.GetAxis( 0 ) ;
					Vector2 axis_1 = GamePad.GetAxis( 1 ) ;
					Vector2 axis_2 = GamePad.GetAxis( 2 ) ;

					if( GamePad.GetButtonAll() != 0 || axis_0.x != 0 || axis_0.y != 0 || axis_1.x != 0 || axis_1.y != 0 || axis_2.x != 0 || axis_2.y != 0 )
					{
						// パッドのいずれかが押された
						Cursor.visible = false ;

						m_InputType = InputTypes.GamePad ;
						m_Hold = true ;
						m_Tick = 0 ;

						m_OnInputTypeChanged?.Invoke( m_InputType ) ;
						m_OnInputTypeChangedDelegate?.Invoke( m_InputType ) ;
					}
				}
			}
			else
			{
				// 現在は GamePad モード
				if( m_Hold == true )
				{
					// 切り替えた直後は一度全開放しないと入力できない
					Vector2 axis_0 = GamePad.GetAxis( 0 ) ;
					Vector2 axis_1 = GamePad.GetAxis( 1 ) ;
					Vector2 axis_2 = GamePad.GetAxis( 2 ) ;

					if( GamePad.GetButtonAll() == 0 && axis_0.x == 0 && axis_0.y == 0 && axis_1.x == 0 && axis_1.y == 0 && axis_2.x == 0 && axis_2.y == 0 )
					{
						m_Hold = false ;

						m_Position = Input.mousePosition ;
					}
					else
					{
						// 入力し続けても１秒経過したら強制解除する(DualShock4を繋いている時にXbox互換のプロファイルに切り替えると右スティックが-1,-1で入りっぱなし扱いになってしまうため)
						m_Tick += Time.unscaledDeltaTime ;
						if( m_Tick >= 1 )
						{
							m_Hold = false ;
						}
					}
				}
				else
				{
					// GamePad モード有効中
					GamePad.Update() ;	// SmartMethod 用

//					Debug.LogWarning( "パッドモード中" ) ;
					if( m_Position.Equals( Input.mousePosition ) == false || Input.GetMouseButton( 0 ) == true || Input.GetMouseButton( 1 ) == true || Input.GetMouseButton( 2 ) == true )
					{
						Cursor.visible = true ;

						m_InputType = InputTypes.Pointer ;
						m_Hold = true ;

						m_OnInputTypeChanged?.Invoke( m_InputType ) ;
						m_OnInputTypeChangedDelegate?.Invoke( m_InputType ) ;
					}
				}
			}

			//----------------------------------------------------------

			if( m_Updater.Count >  0 )
			{
				Updater updater = m_Updater.Peek() ;
				updater.Action( updater.Option ) ;
			}	
		}

		//---------------------------------------------------------------------------

		/// <summary>
		/// モード切替を通知するコールバックを設定する
		/// </summary>
		/// <param name="onInputTypeChanged"></param>
		/// <param name="call"></param>
		public static void SetOnInputTypeChanged( Action<InputTypes> onInputTypeChanged, bool call = false )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.SetOnInputTypeChanged_Private( onInputTypeChanged, call ) ;
		}

		// モード切替を通知するコールバックを設定する
		private void SetOnInputTypeChanged_Private( Action<InputTypes> onInputTypeChanged, bool call )
		{
			m_OnInputTypeChanged = onInputTypeChanged ;
			
			// セットした直後に現在のモードでコールバックを呼ぶ
			if( call == true && m_OnInputTypeChanged != null )
			{
				m_OnInputTypeChanged( m_InputType ) ;
			}
		}

		/// <summary>
		/// モード切替を通知するコールバックを設定する
		/// </summary>
		/// <param name="onInputTypeChangedDelegate"></param>
		/// <param name="call"></param>
		public static void AddOnInputTypeChanged( OnInputTypeChangedDelegate onInputTypeChangedDelegate, bool call = false )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.AddOnInputTypeChanged_Private( onInputTypeChangedDelegate, call ) ;
		}

		// モード切替を通知するコールバックを設定する
		private void AddOnInputTypeChanged_Private( OnInputTypeChangedDelegate onInputTypeChangedDelegate, bool call )
		{
			// 念のため既存の登録があったら削除する(おそらく必要は無いと思うが)
			m_OnInputTypeChangedDelegate -= onInputTypeChangedDelegate ;

			m_OnInputTypeChangedDelegate += onInputTypeChangedDelegate ;
			
			// セットした直後に現在のモードでコールバックを呼ぶ
			if( call == true )
			{
				m_OnInputTypeChangedDelegate( m_InputType ) ;
			}
		}

		/// <summary>
		/// モード切替を通知するコールバックを解除する
		/// </summary>
		/// <param name="onInputTypeChangedDelegate"></param>
		public static void RemoveOnInputTypeChanged( OnInputTypeChangedDelegate onInputTypeChangedDelegate )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.RemoveOnInputTypeChanged_Private( onInputTypeChangedDelegate ) ;
		}

		// モード切替を通知するコールバックを設定する
		private void RemoveOnInputTypeChanged_Private( OnInputTypeChangedDelegate onInputTypeChangedDelegate )
		{
			m_OnInputTypeChangedDelegate -= onInputTypeChangedDelegate ;
		}
	}

	//--------------------------------------------------------------------------------------------

#if UNITY_EDITOR

	/// <summary>
	/// InputManager を設定するクラス(Editor専用)
	/// </summary>
	public class InputManagerSettings
	{
		public enum AxisType	
		{
			KeyOrMouseButton	= 0,
			MouseMovement		= 1,
			JoystickAxis		= 2,
		} ;

		public class Axis
		{
			public string	Name					= "" ;
			public string	DescriptiveName			= "" ;
			public string	DescriptiveNegativeName	= "" ;
			public string	NegativeButton			= "" ;
			public string	PositiveButton			= "" ;
			public string	AltNegativeButton		= "" ;
			public string	AltPositiveButton		= "" ;
	
			public float	Gravity					= 0 ;
			public float	Dead					= 0 ;
			public float	Sensitivity				= 0 ;
	
			public bool		Snap					= false ;
			public bool		Invert					= false ;
		
			public AxisType	Type					= AxisType.KeyOrMouseButton ;
	
			public int		AxisNum					= 1 ;
			public int		JoyNum					= 0 ;
		}

		//-------------------------------------------------------------------------------------------

		[ MenuItem( "Tools/Initialize InputManager" ) ]
		public static void Initialize()
		{
			// 設定をクリアする
			Clear() ;
			
			int i, p ;

			for( p  = 1 ; p <= 4 ; p ++ )
			{
				for( i  =  0 ; i <= 15 ; i ++ )
				{
					AddAxis( CreateButton( "Player_" + p.ToString() + "_Button_" + i.ToString( "D02" ), "joystick " + p.ToString() + " button " + i.ToString() ) ) ;
				}
				for( i  =  1 ; i <= 15 ; i ++ )
				{
					AddAxis( CreatePadAxis( "Player_" + p.ToString() + "_Axis_" + i.ToString( "D02" ), p, i ) ) ;
				}
			}
		}

		/// <summary>
		/// 設定をクリアする
		/// </summary>
		private static void Clear()
		{
			SerializedObject	serializedObject = new SerializedObject( AssetDatabase.LoadAllAssetsAtPath( "ProjectSettings/InputManager.asset" )[ 0 ] ) ;
			SerializedProperty	axesProperty = serializedObject.FindProperty( "m_Axes" ) ;

			int i, j, l, m, p ;

			//--------------

			List<string> keys = new List<string>() ;

			for( p  = 1 ; p <= 4 ; p ++ )
			{
				for( i  =  0 ; i <= 15 ; i ++ )
				{
					keys.Add( "Player_" + p.ToString() + "_Button_" + i.ToString( "D02" ) ) ;
				}

				for( i  =  1 ; i <= 12 ; i ++ )
				{
					keys.Add( "Player_" + p.ToString() + "_Axis_" + i.ToString( "D02" ) ) ;
				}
			}

			m = keys.Count ;

			//--------------

			SerializedProperty axisPropertyElement ;
			string axisName ;

			l = axesProperty.arraySize ;
			i = 0 ;

			while( true )
			{
				axisPropertyElement = axesProperty.GetArrayElementAtIndex( i ) ;
				axisName = GetChildProperty( axisPropertyElement, "m_Name" ).stringValue ;
				
				for( j  = 0 ; j <  m ; j ++ )
				{
					if( axisName == keys[ j ] )
					{
						// 削除対象発見
						break ;
					}
				}

				if( j <  m )
				{
					// 削除対象
					axesProperty.DeleteArrayElementAtIndex( i ) ;
					l -- ;
				}
				else
				{
					i ++ ;
				}

				if( i >= l )
				{
					// 終了
					break ;
				}
			}

			serializedObject.ApplyModifiedProperties() ;
			serializedObject.Dispose() ;
		}

		/// <summary>
		/// InputManager に必要な設定が追加されているか確認する
		/// </summary>
		public static bool Check()
		{
			SerializedObject	serializedObject = new SerializedObject( AssetDatabase.LoadAllAssetsAtPath( "ProjectSettings/InputManager.asset" )[ 0 ] );
			SerializedProperty	axesProperty = serializedObject.FindProperty( "m_Axes" ) ;

			int i, l, m, p ;

			//--------------

			Dictionary<string,bool> keyAndValues = new Dictionary<string, bool>() ;

			for( p  = 1 ; p <= 4 ; p ++ )
			{
				for( i  =  0 ; i <= 15 ; i ++ )
				{
					keyAndValues.Add( "Player_" + p.ToString() + "_Button_" + i.ToString( "D02" ), false ) ;
				}

				for( i  =  1 ; i <= 12 ; i ++ )
				{
					keyAndValues.Add( "Player_" + p.ToString() + "_Axis_" + i.ToString( "D02" ), false ) ;
				}
			}

			m = keyAndValues.Count ;

			//--------------

			SerializedProperty axisPropertyElement ;
			string axisName ;

			l = axesProperty.arraySize ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				axisPropertyElement = axesProperty.GetArrayElementAtIndex( i ) ;
				axisName = GetChildProperty( axisPropertyElement, "m_Name" ).stringValue ;
				
				if( keyAndValues.ContainsKey( axisName ) == true )
				{
					keyAndValues[ axisName ] = true ;
				}
			}

			serializedObject.Dispose() ;

			string[] keys = new string[ m ] ;
			keyAndValues.Keys.CopyTo( keys, 0 ) ;

			for( i  = 0 ; i <  m ; i ++ )
			{
				if( keyAndValues[ keys[ i ] ] == false )
				{
					return false ;	// 設定されていないものがある
				}
			}

			return true ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// ボタンを生成する
		/// </summary>
		/// <param name="tName"></param>
		/// <param name="tPositiveButton"></param>
		/// <param name="tAltPositiveButton"></param>
		/// <returns></returns>
		private static Axis CreateButton( string buttonName, string positiveButton )
		{
			Axis axis = new Axis()
			{
				Name			= buttonName,
				PositiveButton	= positiveButton,
				Gravity			= 1000,
				Dead			= 0.001f,
				Sensitivity		= 1000,
				Type			= AxisType.KeyOrMouseButton
			} ;
			
			return axis ;
		}

		public static Axis CreateKeyAxis( string axisName, string negativeButton, string positiveButton )
		{
			Axis axis = new Axis()
			{
				Name			= axisName,
				NegativeButton	= negativeButton,
				PositiveButton	= positiveButton,
				Gravity			= 3,
				Sensitivity		= 3,
				Dead			= 0.001f,
				Type			= AxisType.KeyOrMouseButton
			} ;
			
			return axis ;
		}

		/// <summary>
		/// ジョイスティックを生成する
		/// </summary>
		/// <param name="tName"></param>
		/// <param name="tJoyNum"></param>
		/// <param name="tAxisNum"></param>
		/// <returns></returns>
		public static Axis CreatePadAxis( string axisName, int joyNum, int axisNum )
		{
			Axis axis = new Axis()
			{
				Name			= axisName,
				Dead			= 0.2f,
				Sensitivity		= 1,
				Type			= AxisType.JoystickAxis,
				AxisNum			= axisNum,
				JoyNum			= joyNum
			} ;
 
			return axis ;
		}

		/// <summary>
		/// 設定を追加する
		/// </summary>
		/// <param name="tAxis"></param>
		private static void AddAxis( Axis axis )
		{
			SerializedObject	serializedObject = new SerializedObject( AssetDatabase.LoadAllAssetsAtPath( "ProjectSettings/InputManager.asset" )[ 0 ] );
			SerializedProperty	axesProperty = serializedObject.FindProperty( "m_Axes" ) ;

			axesProperty.arraySize ++ ;
			serializedObject.ApplyModifiedProperties() ;
	
			SerializedProperty axisProperty = axesProperty.GetArrayElementAtIndex( axesProperty.arraySize - 1 ) ;
		
			GetChildProperty( axisProperty, "m_Name"					).stringValue	= axis.Name ;
			GetChildProperty( axisProperty, "descriptiveName"			).stringValue	= axis.DescriptiveName ;
			GetChildProperty( axisProperty, "descriptiveNegativeName"	).stringValue	= axis.DescriptiveNegativeName ;
			GetChildProperty( axisProperty, "negativeButton"			).stringValue	= axis.NegativeButton ;
			GetChildProperty( axisProperty, "positiveButton"			).stringValue	= axis.PositiveButton ;
			GetChildProperty( axisProperty, "altNegativeButton"			).stringValue	= axis.AltNegativeButton ;
			GetChildProperty( axisProperty, "altPositiveButton"			).stringValue	= axis.AltPositiveButton ;
			GetChildProperty( axisProperty, "gravity"					).floatValue	= axis.Gravity ;
			GetChildProperty( axisProperty, "dead"						).floatValue	= axis.Dead ;
			GetChildProperty( axisProperty, "sensitivity"				).floatValue	= axis.Sensitivity ;
			GetChildProperty( axisProperty, "snap"						).boolValue		= axis.Snap ;
			GetChildProperty( axisProperty, "invert"					).boolValue		= axis.Invert ;
			GetChildProperty( axisProperty, "type"						).intValue		= ( int )axis.Type ;
			GetChildProperty( axisProperty, "axis"						).intValue		= axis.AxisNum - 1 ;
			GetChildProperty( axisProperty, "joyNum"					).intValue		= axis.JoyNum ;
 
			serializedObject.ApplyModifiedProperties() ;

			serializedObject.Dispose() ;
		}
 
		private static SerializedProperty GetChildProperty( SerializedProperty parent, string childName )
		{
			SerializedProperty child = parent.Copy() ;
			child.Next( true ) ;

			do
			{
				if( child.name == childName )
				{
					return child ;
				}
			}
			while( child.Next( false ) ) ;

			return null ;
		}
	}
 
#endif

	//--------------------------------------------------------------------------------------------

	public class GamePad
	{
		public const int B1	= 0x0001 ;
		public const int B2	= 0x0002 ;
		public const int B3	= 0x0004 ;
		public const int B4	= 0x0008 ;

		public const int R1	= 0x0010 ;
		public const int L1	= 0x0020 ;
		public const int R2	= 0x0040 ;
		public const int L2	= 0x0080 ;
		public const int R3	= 0x0100 ;
		public const int L3	= 0x0200 ;

		public const int O1	= 0x0400 ;
		public const int O2	= 0x0800 ;
		public const int O3	= 0x1000 ;
		public const int O4	= 0x2000 ;

		private static readonly Dictionary<int,int> m_ButtonNumberIndex = new Dictionary<int, int>()
		{
			{ B1,  0 },
			{ B2,  1 },
			{ B3,  2 },
			{ B4,  3 },

			{ R1,  4 },
			{ L1,  5 },
			{ R2,  6 },
			{ L2,  7 },
			{ R3,  8 },
			{ L3,  9 },

			{ O1, 10 },
			{ O2, 11 },
			{ O3, 12 },
			{ O4, 13 },
		} ;

		public class Profile
		{
			public int[]	ButtonNumber = null ;
			public int[]	AxisNumber = null ;

			public bool		AnalogButtonCorrection = false ;
			public float	AnalogButtonThreshold = 0.2f ;

			public Profile( int[] buttonNumber, int[] axisNumber, bool analogButtonCorrection, float analogButtonThreshold )
			{
				ButtonNumber			= buttonNumber ;
				AxisNumber				= axisNumber ;
				AnalogButtonCorrection	= analogButtonCorrection ;
				AnalogButtonThreshold	= analogButtonThreshold ;
			}
		}

#if UNITY_EDITOR || UNITY_STANDALONE
		public static Profile Profile_Xbox = new Profile
		(
			new int[]{  0,  1,  2,  3,  5,  4, -1, -1,  9,  8,  7,  6,	11, 10 },
			new int[]{  6,  7,  1,  2,  4,  5, 10,  9 },
			false, 0.4f
		) ;
#elif !UNITY_EDITOR && ( UNITY_ANDROID || UNITY_IOS || UNITY_IPHONE )
		public static Profile Profile_Xbox = new Profile
		(
			new int[]{  0,  1,  2,  3,  5,  4, -1, -1,  9,  8,  7,  6,	11, 10 },
//			new int[]{  6,  7,  1,  2,  4,  5, 10,  9 },
			new int[]{  5,  6,  1,  2,  3,  4,  8,  7 },
			false, 0.4f
		) ;
#endif

		public static Profile Profile_DualShock = new Profile
		(
			new int[]{  1,  2,  0,  3,  5,  4,  7,  6, 11, 10,  9,  8,	13, 12 },
			new int[]{  7,  8,  1,  2,  3,  6,  5,  4 },
			true,  0.4f
		) ;

		// プロファイル情報(0～7)
		private static readonly Profile[] m_Profile =
		{
			// Xbox
			Profile_Xbox,
			Profile_Xbox,
			Profile_Xbox,
			Profile_Xbox,
			Profile_Xbox,
			Profile_Xbox,
			Profile_Xbox,
			Profile_Xbox,
		} ;

		/// <summary>
		/// プロファィル情報を設定する
		/// </summary>
		/// <param name="tNumber"></param>
		/// <param name="tButtonNumber"></param>
		/// <param name="tAxisNumber"></param>
		/// <param name="tAnalogButtonCorrection"></param>
		/// <param name="tAnalogButtonThreshold"></param>
		/// <returns></returns>
		public static bool SetProfile( int number, int[] buttonNumber, int[] axisNumber, bool analogButtonCorrection, float analogButtonThreshold )
		{
			if( number <  0 || number >= m_Profile.Length )
			{
				return false ;
			}

			m_Profile[ number ] = new Profile( buttonNumber, axisNumber, analogButtonCorrection, analogButtonThreshold ) ;
			return true ;
		}

		/// <summary>
		/// プロファイル情報を設定する
		/// </summary>
		/// <param name="tNumber"></param>
		/// <param name="tProfile"></param>
		/// <returns></returns>
		public static bool RegisterProfile( int number, Profile profile )
		{
			if( number <  0 || number >= m_Profile.Length )
			{
				return false ;
			}

			m_Profile[ number ] = profile ;
			return true ;

		}

		// プレイヤー番号に対するデフォルトのプロフィール番号
		private static readonly int[] m_DefaultProfileNumber =
		{
			-1, -1, -1, -1
		} ;

		/// <summary>
		/// プレイヤー番号に対するデフォルトのプロフィール番号を設定する
		/// </summary>
		/// <param name="tPlayerNumber"></param>
		/// <param name="tProfileNumber"></param>
		/// <returns></returns>
		public static bool SetDefaultProfileNumber( int playerNumber, int profileNumber )
		{
			if( playerNumber <  0 || playerNumber >= m_DefaultProfileNumber.Length || profileNumber <  0 || profileNumber >= m_Profile.Length )
			{
				return false ;
			}

			m_DefaultProfileNumber[ playerNumber ] = profileNumber ;

			return true ;
		}

		//-------------------------------------------------------------------------------------------

		private static readonly string[][]	m_ButtonName =
		{
//			new string[]
//			{
//				"Button_00", "Button_01", "Button_02","Button_03", "Button_04", "Button_05", "Button_06", "Button_07", "Button_08","Button_09", "Button_10", "Button_11", "Button_12", "Button_13", "Button_14","Button_15"
//			},
			new string[]
			{
				"Player_1_Button_00", "Player_1_Button_01", "Player_1_Button_02","Player_1_Button_03", "Player_1_Button_04", "Player_1_Button_05", "Player_1_Button_06", "Player_1_Button_07", "Player_1_Button_08","Player_1_Button_09", "Player_1_Button_10", "Player_1_Button_11", "Player_1_Button_12", "Player_1_Button_13", "Player_1_Button_14","Player_1_Button_15"
			},
			new string[]
			{
				"Player_2_Button_00", "Player_2_Button_01", "Player_2_Button_02","Player_2_Button_03", "Player_2_Button_04", "Player_2_Button_05", "Player_2_Button_06", "Player_2_Button_07", "Player_2_Button_08","Player_2_Button_09", "Player_2_Button_10", "Player_2_Button_11", "Player_2_Button_12", "Player_2_Button_13", "Player_2_Button_14","Player_2_Button_15"
			},
			new string[]
			{
				"Player_3_Button_00", "Player_3_Button_01", "Player_3_Button_02","Player_3_Button_03", "Player_3_Button_04", "Player_3_Button_05", "Player_3_Button_06", "Player_3_Button_07", "Player_3_Button_08","Player_3_Button_09", "Player_3_Button_10", "Player_3_Button_11", "Player_3_Button_12", "Player_3_Button_13", "Player_3_Button_14","Player_3_Button_15"
			},
			new string[]
			{
				"Player_4_Button_00", "Player_4_Button_01", "Player_4_Button_02","Player_4_Button_03", "Player_4_Button_04", "Player_4_Button_05", "Player_4_Button_06", "Player_4_Button_07", "Player_4_Button_08","Player_4_Button_09", "Player_4_Button_10", "Player_4_Button_11", "Player_4_Button_12", "Player_4_Button_13", "Player_4_Button_14","Player_4_Button_15"
			},
		} ;

		private static readonly string[][]	m_AxisName =
		{
//			new string[]
//			{
//				"", "Axis_01", "Axis_02","Axis_03", "Axis_04", "Axis_05", "Axis_06", "Axis_07", "Axis_08","Axis_09", "Axis_10", "Axis_11", "Axis_12"
//			},
			new string[]
			{
				"", "Player_1_Axis_01", "Player_1_Axis_02","Player_1_Axis_03", "Player_1_Axis_04", "Player_1_Axis_05", "Player_1_Axis_06", "Player_1_Axis_07", "Player_1_Axis_08","Player_1_Axis_09", "Player_1_Axis_10", "Player_1_Axis_11", "Player_1_Axis_12", "Player_1_Axis_13", "Player_1_Axis_14", "Player_1_Axis_15"
			},
			new string[]
			{
				"", "Player_2_Axis_01", "Player_2_Axis_02","Player_2_Axis_03", "Player_2_Axis_04", "Player_2_Axis_05", "Player_2_Axis_06", "Player_2_Axis_07", "Player_2_Axis_08","Player_2_Axis_09", "Player_2_Axis_10", "Player_2_Axis_11", "Player_2_Axis_12", "Player_2_Axis_13", "Player_2_Axis_14", "Player_2_Axis_15"
			},
			new string[]
			{
				"", "Player_3_Axis_01", "Player_3_Axis_02","Player_3_Axis_03", "Player_3_Axis_04", "Player_3_Axis_05", "Player_3_Axis_06", "Player_3_Axis_07", "Player_3_Axis_08","Player_3_Axis_09", "Player_3_Axis_10", "Player_3_Axis_11", "Player_3_Axis_12", "Player_3_Axis_13", "Player_3_Axis_14", "Player_3_Axis_15"
			},
			new string[]
			{
				"", "Player_4_Axis_01", "Player_4_Axis_02","Player_4_Axis_03", "Player_4_Axis_04", "Player_4_Axis_05", "Player_4_Axis_06", "Player_4_Axis_07", "Player_4_Axis_08","Player_4_Axis_09", "Player_4_Axis_10", "Player_4_Axis_11", "Player_4_Axis_12", "Player_4_Axis_13", "Player_4_Axis_14", "Player_4_Axis_15"
			},
		} ;

		public static float RepeatStartingTime = 0.5f ;
		public static float RepeatIntervalTime = 0.05f ;
		public static float EnableThreshold = 0.75f ;
		
		public static int	Layer ;

		public static void SetLayer( int layer )
		{
			if( layer <  0 )
			{
				layer  = 0 ;
			}

			Layer = layer ;
		}

		/// <summary>
		/// ボタン１とボタン２の入れ替え
		/// </summary>
		public static bool SwapB1toB2 ;	// ボタン１とボタン２を入れ替えるかどうか(入れ替えない場合はＤＳの場合は×が決定になる)

		/// <summary>
		/// ボタン３とボタン４の入れ替え
		/// </summary>
		public static bool SwapB3toB4 ;	// ボタン３とボタン４を入れ替えるかどうか

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 指定したレイヤーの全てのボタンの状態を取得する
		/// </summary>
		/// <param name="layer"></param>
		/// <param name="playerNumber"></param>
		/// <param name="profileNumber"></param>
		/// <returns></returns>
		public static int GetButtonAllOfLayer( int layer, int playerNumber = 1, int profileNumber = 1 )
		{
			if( layer >= 0 && layer <  Layer )
			{
				// 無効
				return 0 ;
			}

			return GetButtonAll( playerNumber, profileNumber ) ;
		}

		/// <summary>
		/// 全てのボタンの状態を取得する
		/// </summary>
		/// <param name="playerNumber"></param>
		/// <param name="profileNumber"></param>
		/// <returns></returns>
		public static int GetButtonAll( int playerNumber = -1, int profileNumber = -1 )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( InputManager.Instance == null || InputManager.IsEnabled == false )
			{
				return 0 ;
			}

			//----------------------------------

			int button = 0 ;

			//----------------------------------

			if( playerNumber <  -1 || playerNumber >= m_DefaultProfileNumber.Length )
			{
				playerNumber  = -1 ;
			}

			if( playerNumber <  0 )
			{
				// Key
				if( Input.GetKey( KeyCode.Z ) == true )
				{
					button |= B1 ;
				}
				if( Input.GetKey( KeyCode.X ) == true )
				{
					button |= B2 ;
				}
				if( Input.GetKey( KeyCode.C ) == true )
				{
					button |= B3 ;
				}
				if( Input.GetKey( KeyCode.V ) == true )
				{
					button |= B4 ;
				}

				if( Input.GetKey( KeyCode.E ) == true )
				{
					button |= R1 ;
				}
				if( Input.GetKey( KeyCode.Q ) == true )
				{
					button |= L1 ;
				}

				if( Input.GetKey( KeyCode.RightShift ) == true )
				{
					button |= R2 ;
				}
				if( Input.GetKey( KeyCode.LeftShift ) == true )
				{
					button |= L2 ;
				}

				if( Input.GetKey( KeyCode.RightControl ) == true )
				{
					button |= R3 ;
				}
				if( Input.GetKey( KeyCode.LeftControl ) == true )
				{
					button |= L3 ;
				}
				if( Input.GetKey( KeyCode.Return ) == true )
				{
					button |= O1 ;
				}
				if( Input.GetKey( KeyCode.Escape ) == true )
				{
					button |= O2 ;
				}
				if( Input.GetKey( KeyCode.Space ) == true )
				{
					button |= O3 ;
				}
				if( Input.GetKey( KeyCode.Backspace ) == true )
				{
					button |= O4 ;
				}
			}
			
			int p, ps, pe, q ;

			if( playerNumber <  0 )
			{
				ps = 0 ;
				pe = 3 ;
			}
			else
			{
				ps = playerNumber ;
				pe = playerNumber ;
			}

			for( p  = ps ; p <= pe ; p ++ )
			{
				// GamePad 0～
				if( profileNumber >= 0 && profileNumber <  m_Profile.Length )
				{
					q = profileNumber ;
				}
				else
				{
					q = m_DefaultProfileNumber[ p ] ;
				}

				if( q >= 0 && q <  m_Profile.Length )
				{
					if( SwapB1toB2 == false )
					{
						// ボタン１とボタン２の入れ替え：なし
						if( Input.GetButton( m_ButtonName[ p ][ m_Profile[ q ].ButtonNumber[  0 ] ] ) == true )
						{
							button |= B1 ;
						}
						if( Input.GetButton( m_ButtonName[ p ][ m_Profile[ q ].ButtonNumber[  1 ] ] ) == true )
						{
							button |= B2 ;
						}
					}
					else
					{
						// ボタン１とボタン２の入れ替え：あり
						if( Input.GetButton( m_ButtonName[ p ][ m_Profile[ q ].ButtonNumber[  0 ] ] ) == true )
						{
							button |= B2 ;
						}
						if( Input.GetButton( m_ButtonName[ p ][ m_Profile[ q ].ButtonNumber[  1 ] ] ) == true )
						{
							button |= B1 ;
						}
					}

					if( SwapB3toB4 == false )
					{
						// ボタン３とボタン４の入れ替え：なし
						if( Input.GetButton( m_ButtonName[ p ][ m_Profile[ q ].ButtonNumber[  2 ] ] ) == true )
						{
							button |= B3 ;
						}
						if( Input.GetButton( m_ButtonName[ p ][ m_Profile[ q ].ButtonNumber[  3 ] ] ) == true )
						{
							button |= B4 ;
						}
					}
					else
					{
						// ボタン３とボタン４の入れ替え：あり
						if( Input.GetButton( m_ButtonName[ p ][ m_Profile[ q ].ButtonNumber[  2 ] ] ) == true )
						{
							button |= B4 ;
						}
						if( Input.GetButton( m_ButtonName[ p ][ m_Profile[ q ].ButtonNumber[  3 ] ] ) == true )
						{
							button |= B3 ;
						}
					}

					if( Input.GetButton( m_ButtonName[ p ][ m_Profile[ q ].ButtonNumber[  4 ] ] ) == true )
					{
						button |= R1 ;
					}
					if( Input.GetButton( m_ButtonName[ p ][ m_Profile[ q ].ButtonNumber[  5 ] ] ) == true )
					{
						button |= L1 ;
					}

					if( m_Profile[ q ].ButtonNumber[  6 ] >= 0 )
					{
						if( Input.GetButton( m_ButtonName[ p ][ m_Profile[ q ].ButtonNumber[  6 ] ] ) == true )
						{
							button |= R2 ;
						}
					}
					else
					{
						float axis = Input.GetAxis( m_AxisName[ p ][ m_Profile[ q ].AxisNumber[  6 ] ] ) ;
						if( m_Profile[ q ].AnalogButtonCorrection == true )
						{
							axis = axis * 0.5f + 0.5f ;
						}
						if( axis >= m_Profile[ q ].AnalogButtonThreshold )
						{
							button |= R2 ;
						}
					}

					if( m_Profile[ q ].ButtonNumber[  7 ] >= 0 )
					{
						if( Input.GetButton( m_ButtonName[ p ][ m_Profile[ q ].ButtonNumber[  7 ] ] ) == true )
						{
							button |= L2 ;
						}
					}
					else
					{
						float axis = Input.GetAxis( m_AxisName[ p ][ m_Profile[ q ].AxisNumber[  7 ] ] ) ;
						if( m_Profile[ q ].AnalogButtonCorrection == true )
						{
							axis = axis * 0.5f + 0.5f ;
						}
						if( axis >= m_Profile[ q ].AnalogButtonThreshold )
						{
							button |= L2 ;
						}
					}

					if( Input.GetButton( m_ButtonName[ p ][ m_Profile[ q ].ButtonNumber[  8 ] ] ) == true )
					{
						button |= R3 ;
					}
					if( Input.GetButton( m_ButtonName[ p ][ m_Profile[ q ].ButtonNumber[  9 ] ] ) == true )
					{
						button |= L3 ;
					}
					if( Input.GetButton( m_ButtonName[ p ][ m_Profile[ q ].ButtonNumber[ 10 ] ] ) == true )
					{
						button |= O1 ;
					}
					if( Input.GetButton( m_ButtonName[ p ][ m_Profile[ q ].ButtonNumber[ 11 ] ] ) == true )
					{
						button |= O2 ;
					}
					if( Input.GetButton( m_ButtonName[ p ][ m_Profile[ q ].ButtonNumber[ 12 ] ] ) == true )
					{
						button |= O3 ;
					}
					if( Input.GetButton( m_ButtonName[ p ][ m_Profile[ q ].ButtonNumber[ 13 ] ] ) == true )
					{
						button |= O4 ;
					}
				}
			}

			return button ;
		}

		/// <summary>
		/// 指定したレイヤーのボタンの状態を取得する
		/// </summary>
		/// <param name="buttonNumber"></param>
		/// <param name="layer"></param>
		/// <param name="playerNumber"></param>
		/// <param name="profileNumber"></param>
		/// <returns></returns>
		public static bool GetButtonOfLayer( int buttonNumber, int layer, int playerNumber = 1, int profileNumber = 1 )
		{
			if( layer >= 0 && layer <  Layer )
			{
				// 無効
				return false ;
			}

			return GetButton( buttonNumber, playerNumber, profileNumber ) ;
		}

		/// <summary>
		/// ボタンの状態を取得する
		/// </summary>
		/// <param name="buttonNumber"></param>
		/// <param name="playerNumber"></param>
		/// <param name="profileNumber"></param>
		/// <returns></returns>
		public static bool GetButton( int buttonNumber, int playerNumber = -1, int profileNumber = -1 )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( InputManager.Instance == null || InputManager.IsEnabled == false )
			{
				return false ;
			}

			//----------------------------------

			int button = GetButtonAll( playerNumber, profileNumber ) ;

			if( ( button & buttonNumber ) == 0 )
			{
				return false ;
			}

			return true ;
		}

		/// <summary>
		/// 指定したレイヤーのアクシズの状態を取得する
		/// </summary>
		/// <param name="axisNumber"></param>
		/// <param name="layer"></param>
		/// <param name="playerNumber"></param>
		/// <param name="profileNumber"></param>
		/// <returns></returns>
		public static Vector2 GetAxisOfLayer( int axisNumber, int layer, int playerNumber = 1, int profileNumber = 1 )
		{
			if( layer >= 0 && layer <  Layer )
			{
				// 無効
				return Vector2.zero ;
			}

			return GetAxis( axisNumber, playerNumber, profileNumber ) ;
		}

		/// <summary>
		/// アクシズの状態を取得する
		/// </summary>
		/// <param name="axisNumber"></param>
		/// <param name="playerNumber"></param>
		/// <param name="profileNumber"></param>
		/// <returns></returns>
		public static Vector2 GetAxis( int axisNumber, int playerNumber = -1, int profileNumber = -1 )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( InputManager.Instance == null || InputManager.IsEnabled == false )
			{
				return Vector2.zero ;
			}

			//----------------------------------

			float oAxisX = 0 ;
			float oAxisY = 0 ;

			if( playerNumber <  -1 || playerNumber >= m_DefaultProfileNumber.Length )
			{
				playerNumber  = -1 ;
			}
			
			if( playerNumber <  0 )
			{
				// Key

				if( axisNumber == 0 )
				{
					// SCX
					if( Input.GetKey( KeyCode.D ) == true || Input.GetKey( KeyCode.RightArrow ) == true )
					{
						oAxisX += 1 ;
					}
					if( Input.GetKey( KeyCode.A ) == true || Input.GetKey( KeyCode.LeftArrow ) == true )
					{
						oAxisX -= 1 ;
					}

					// SCY
					if( Input.GetKey( KeyCode.W ) == true || Input.GetKey( KeyCode.UpArrow ) == true )
					{
						oAxisY -= 1 ;
					}
					if( Input.GetKey( KeyCode.S ) == true || Input.GetKey( KeyCode.DownArrow ) == true )
					{
						oAxisY += 1 ;
					}
				}

				if( axisNumber == 1 )
				{
					// SCX
					if( Input.GetKey( KeyCode.D ) == true )
					{
						oAxisX += 1 ;
					}
					if( Input.GetKey( KeyCode.A ) == true )
					{
						oAxisX -= 1 ;
					}

					// SCY
					if( Input.GetKey( KeyCode.W ) == true )
					{
						oAxisY -= 1 ;
					}
					if( Input.GetKey( KeyCode.S ) == true )
					{
						oAxisY += 1 ;
					}
				}

				if( axisNumber == 2 )
				{
					// SCX
					if( Input.GetKey( KeyCode.RightArrow ) == true )
					{
						oAxisX += 1 ;
					}
					if( Input.GetKey( KeyCode.LeftArrow ) == true )
					{
						oAxisX -= 1 ;
					}

					// SCY
					if( Input.GetKey( KeyCode.UpArrow ) == true )
					{
						oAxisY -= 1 ;
					}
					if( Input.GetKey( KeyCode.DownArrow ) == true )
					{
						oAxisY += 1 ;
					}
				}
			}
			
			int p, ps, pe, q ;

			if( playerNumber <  0 )
			{
				ps = 0 ;
				pe = 3 ;
			}
			else
			{
				ps = playerNumber ;
				pe = playerNumber ;
			}

			for( p  = ps ; p <= pe ; p ++ )
			{
				// 0～
				if( profileNumber >= 0 && profileNumber <  m_Profile.Length )
				{
					q = profileNumber ;
				}
				else
				{
					q = m_DefaultProfileNumber[ p ] ;
				}

				if( q >= 0 && q <  m_Profile.Length )
				{
					if( axisNumber == 0 )
					{
						// SCX
						float axisX = Input.GetAxis( m_AxisName[ p ][ m_Profile[ q ].AxisNumber[  0 ] ] ) ;
						if( axisX != 0 )
						{
							oAxisX = axisX ;
						}

						// SCY
						float axisY = Input.GetAxis( m_AxisName[ p ][ m_Profile[ q ].AxisNumber[  1 ] ] ) ;
						if( axisY != 0 )
						{
#if UNITY_EDITOR || UNITY_STANDALONE
							oAxisY = - axisY ;
#endif
#if !UNITY_EDITOR && UNITY_ANDROID
							oAxisY = axisY ;
#endif
						}
					}
					else
					if( axisNumber == 1 )
					{
						// SLX
						float axisX = Input.GetAxis( m_AxisName[ p ][ m_Profile[ q ].AxisNumber[  2 ] ] ) ;
						if( axisX != 0 )
						{
							oAxisX = axisX ;
						}

						// SLY
						float axisY = Input.GetAxis( m_AxisName[ p ][ m_Profile[ q ].AxisNumber[  3 ] ] ) ;
						if( axisY != 0 )
						{
							oAxisY = axisY ;
						}
					}
					else
					if( axisNumber == 2 )
					{
						// SRX
						float axisX = Input.GetAxis( m_AxisName[ p ][ m_Profile[ q ].AxisNumber[  4 ] ] ) ;
						if( axisX != 0 )
						{
							oAxisX = axisX ;
						}

						// SRY
						float axisY = Input.GetAxis( m_AxisName[ p ][ m_Profile[ q ].AxisNumber[  5 ] ] ) ;
						if( axisY != 0 )
						{
							oAxisY = axisY ;
						}
					}
					else
					if( axisNumber == 3 )
					{
						// R2
						float axisX = Input.GetAxis( m_AxisName[ p ][ m_Profile[ q ].AxisNumber[  6 ] ] ) ;
						if( m_Profile[ q ].AnalogButtonCorrection == true )
						{
							axisX = axisX * 0.5f + 0.5f ;
						}
						if( axisX != 0 )
						{
							oAxisX = axisX ;
							if( oAxisX <  0 )
							{
								oAxisX  = - oAxisX ;
							}
						}

						// L2
						float axisY = Input.GetAxis( m_AxisName[ p ][ m_Profile[ q ].AxisNumber[  7 ] ] ) ;
						if( m_Profile[ q ].AnalogButtonCorrection == true )
						{
							axisY = axisY * 0.5f + 0.5f ;
						}
						if( axisY != 0 )
						{
							oAxisY = axisY ;
							if( oAxisY <  0 )
							{
								oAxisY  = - oAxisY ;
							}
						}
					}
				}
			}

			// 縦軸の符号反転
			if( InputManager.Invert == true && axisNumber != 3 )
			{
				oAxisY = - oAxisY ;
			}

			return new Vector2( oAxisX, oAxisY ) ;
		}

		/// <summary>
		/// 接続中のゲームパッドの名前を取得する
		/// </summary>
		/// <returns></returns>
		public static string[] GetNames()
		{
			return Input.GetJoystickNames() ;
		}

		//-------------------------------------------------------------------------------------------

		private static readonly int[] m_PlayerNumbers  = {  0,  1,  2,  3, -1 } ;
		private static readonly int[] m_ProfileNumbers = {  0,  1,  2,  3, -1 } ;

		private static readonly int[] m_ButtonNumbers =
		{
			B1,
			B2,
			B3,
			B4,

			R1,
			L1,
			R2,
			L2,
			R3,
			L3,

			O1,
			O2,
			O3,
			O4,
		} ;

		private static readonly int[] m_AxisNumbers =
		{
			0,
			1,
			2,
			3,
		} ;

		/// <summary>
		/// Smart 系の状態更新
		/// </summary>
		public static void Update()
		{
			int i, l, j, m, k ;

			l = m_PlayerNumbers.Length ;

			int button ;
			Vector2 axis ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				// Button
				button = GetButtonAll( m_PlayerNumbers[ i ], m_ProfileNumbers[ i ] ) ;

				m = m_ButtonNumbers.Length ;
				for( j  = 0 ; j <  m ; j ++ )
				{
					k = m_ButtonNumberIndex[ m_ButtonNumbers[ j ] ] ;

					UpdateButtonStates( i, k, m_ButtonNumbers[ j ], button ) ;
				}

				// Axis
				m = m_AxisNumbers.Length ;
				for( j  = 0 ; j <  m ; j ++ )
				{
					k = m_AxisNumbers[ j ] ;

					axis = GetAxis( k, m_PlayerNumbers[ i ], m_ProfileNumbers[ i ] ) ;

					UpdateAxisStates( i, k, axis ) ;
				}
			}
		}

		// ボタンの状態更新を行う
		private static void UpdateButtonStates( int i, int k, int buttonNmber, int button )
		{
			m_ButtonHoldStates[ i, k ] = false ;
			m_ButtonOnceStates[ i, k ] = false ;

			if( ( button & buttonNmber ) != 0 )
			{
				if( m_ButtonHoldKeepFlags[ i, k ] == false )
				{
					// ホールド開始
					m_ButtonHoldStates[ i, k ] = true ;

					m_ButtonHoldKeepFlags[ i, k ] = true ;
					m_ButtonHoldWakeTimes[ i, k ] = Time.realtimeSinceStartup ;
					m_ButtonHoldLoopTimes[ i, k ] = Time.realtimeSinceStartup ;

					m_ButtonOnceStates[ i, k ] = true ;
				}
				else
				{
					// ホールド最中
					if( ( Time.realtimeSinceStartup - m_ButtonHoldWakeTimes[ i, k ] ) >= RepeatStartingTime )
					{
						// リピート中
						if( ( Time.realtimeSinceStartup - m_ButtonHoldLoopTimes[ i, k ] ) >= RepeatIntervalTime )
						{
							m_ButtonHoldStates[ i, k ] = true ;

							m_ButtonHoldLoopTimes[ i, k ] = Time.realtimeSinceStartup ;
						}
					}
				}
			}
			else
			{
				// ホールド解除
				m_ButtonHoldKeepFlags[ i, k ] = false ;
			}
		}

		// 方向キーの状態更新を行う
		private static void UpdateAxisStates( int i, int k, Vector2 axis )
		{
			if( axis.x >=     EnableThreshold   )
			{
				axis.x  =  1 ;
			}
			else
			if( axis.x <= ( - EnableThreshold ) )
			{
				axis.x  = -1 ;
			}
			else
			{
				axis.x  =  0 ;
			}

			if( axis.y >=     EnableThreshold   )
			{
				axis.y  =  1 ;
			}
			else
			if( axis.y <= ( - EnableThreshold ) )
			{
				axis.y  = -1 ;
			}
			else
			{
				axis.y  =  0 ;
			}

			m_AxisHoldStates[ i, k ] = Vector2.zero ;
			m_AxisOnceStates[ i, k ] = Vector2.zero ;

			if( axis.x != 0 || axis.y != 0 )
			{
				if( m_AxisHoldKeepFlags[ i, k ] == false )
				{
					// ホールド開始
					m_AxisHoldStates[ i, k ] = axis ;

					m_AxisHoldKeepFlags[ i, k ] = true ;
					m_AxisHoldWakeTimes[ i, k ] = Time.realtimeSinceStartup ;
					m_AxisHoldLoopTimes[ i, k ] = Time.realtimeSinceStartup ;

					m_AxisOnceStates[ i, k ] = axis ;
				}
				else
				{
					// ホールド最中
					if( ( Time.realtimeSinceStartup - m_AxisHoldWakeTimes[ i, k ] ) >= RepeatStartingTime )
					{
						// リピート中
						if( ( Time.realtimeSinceStartup - m_AxisHoldLoopTimes[ i, k ] ) >= RepeatIntervalTime )
						{
							m_AxisHoldStates[ i, k ] = axis ;
	
							m_AxisHoldLoopTimes[ i, k ] = Time.realtimeSinceStartup ;
						}
					}
				}
			}
			else
			{
				// ホールド解除
				m_AxisHoldKeepFlags[ i, k ] = false ;
			}
		}

		//-------------------------------------------------------------------------------------------
		
		private static readonly bool[,]		m_ButtonHoldKeepFlags	= new  bool[ 5, 16 ] ;
		private static readonly float[,]	m_ButtonHoldWakeTimes	= new float[ 5, 16 ] ;
		private static readonly float[,]	m_ButtonHoldLoopTimes	= new float[ 5, 16 ] ;
		private static readonly bool[,]		m_ButtonHoldStates		= new  bool[ 5, 16 ] ;
		private static readonly bool[,]		m_ButtonOnceStates		= new  bool[ 5, 16 ] ;
		
		/// <summary>
		/// 指定したレイヤーのリピート機能付きでボタンの押下状態を取得する
		/// </summary>
		/// <param name="buttonNumber"></param>
		/// <param name="layer"></param>
		/// <param name="playerNumber"></param>
		/// <returns></returns>
		public static bool GetSmartButtonOfLayer( int buttonNumber, int layer, int playerNumber = 1 )
		{
			if( layer >= 0 && layer <  Layer )
			{
				// 無効
				return false ;
			}

			return GetSmartButton( buttonNumber, playerNumber ) ;
		}

		/// <summary>
		/// リピート機能付きでボタンの押下状態を取得する
		/// </summary>
		/// <param name="buttonNumber"></param>
		/// <param name="playerNumber"></param>
		/// <returns></returns>
		public static bool GetSmartButton( int buttonNumber, int playerNumber = -1 )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( InputManager.Instance == null || InputManager.IsEnabled == false )
			{
				return false ;
			}

			//----------------------------------

			int i, k ;

			if( playerNumber >= 0 && playerNumber <  m_DefaultProfileNumber.Length )
			{
				i = playerNumber ;
			}
			else
			{
				i  = 4 ;
			}

			k = m_ButtonNumberIndex[ buttonNumber ] ;

			return m_ButtonHoldStates[ i, k ] ;
		}

		/// <summary>
		/// 一度だけ反応するボタンの押下状態を取得する
		/// </summary>
		/// <param name="buttonNumber"></param>
		/// <param name="playerNumber"></param>
		/// <returns></returns>
		public static bool GetOnceButton( int buttonNumber, int playerNumber = -1 )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( InputManager.Instance == null || InputManager.IsEnabled == false )
			{
				return false ;
			}

			//----------------------------------

			int i, k ;

			if( playerNumber >= 0 && playerNumber <  m_DefaultProfileNumber.Length )
			{
				i = playerNumber ;
			}
			else
			{
				i  = 4 ;
			}

			k = m_ButtonNumberIndex[ buttonNumber ] ;

			return m_ButtonOnceStates[ i, k ] ;
		}

		//-------------------------------------------------------------------------------------------

		private static readonly bool[,]		m_AxisHoldKeepFlags = new  bool[ 5, 4 ] ;
		private static readonly float[,]	m_AxisHoldWakeTimes = new float[ 5, 4 ] ;
		private static readonly float[,]	m_AxisHoldLoopTimes = new float[ 5, 4 ] ;
		private static readonly Vector2[,]	m_AxisHoldStates = new Vector2[ 5, 4 ] ;
		private static readonly Vector2[,]	m_AxisOnceStates = new Vector2[ 5, 4 ] ;

		/// <summary>
		/// 指定したレイヤーのリピート機能付きでアクシズの状態を取得する
		/// </summary>
		/// <param name="axisNumber"></param>
		/// <param name="layer"></param>
		/// <param name="playerNumber"></param>
		/// <returns></returns>
		public static Vector2 GetSmartAxisOfLayer( int axisNumber, int layer, int playerNumber = 1 )
		{
			if( layer >= 0 && layer <  Layer )
			{
				// 無効
				return Vector2.zero ;
			}

			return GetSmartAxis( axisNumber, playerNumber ) ;
		}

		/// <summary>
		/// リピート機能付きでアクシズの状態を取得する
		/// </summary>
		/// <param name="axisNumber"></param>
		/// <param name="playerNumber"></param>
		/// <returns></returns>
		public static Vector2 GetSmartAxis( int axisNumber, int playerNumber = -1 )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( InputManager.Instance == null || InputManager.IsEnabled == false )
			{
				return Vector2.zero ;
			}

			//----------------------------------

			int i, k ;

			if( playerNumber >= 0 && playerNumber <  m_DefaultProfileNumber.Length )
			{
				i = playerNumber ;
			}
			else
			{
				i  = 4 ;
			}

			k = axisNumber ;

			return m_AxisHoldStates[ i, k ] ;
		}

		/// <summary>
		/// 一度だけ反応するアクシズの状態を取得する
		/// </summary>
		/// <param name="axisNumber"></param>
		/// <param name="playerNumber"></param>
		/// <returns></returns>
		public static Vector2 GetOnceAxis( int axisNumber, int playerNumber = -1 )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( InputManager.Instance == null || InputManager.IsEnabled == false )
			{
				return Vector2.zero ;
			}

			//----------------------------------

			int i, k ;

			if( playerNumber >= 0 && playerNumber <  m_DefaultProfileNumber.Length )
			{
				i = playerNumber ;
			}
			else
			{
				i  = 4 ;
			}

			k = axisNumber ;

			return m_AxisOnceStates[ i, k ] ;
		}
	}

	//--------------------------------------------------------------------------------------------

	/// <summary>
	/// ポインターの制御クラス
	/// </summary>
	public static class Pointer
	{
		private static readonly bool[]	m_Press = new bool[ 3 ] ;
		private static readonly int[]	m_State = new int[ 3 ] ;
		private static readonly float[]	m_Timer = new float[ 3 ] ;

		public static float		RepeatStartingTime = 0.5f ;
		public static float		RepeatIntervalTime = 0.05f ;

		public static int		LB = 0 ;
		public static int		RB = 1 ;
		public static int		CB = 2 ;

		/// <summary>
		/// 毎フレーム更新
		/// </summary>
		public static void Update()
		{
			int i, l = 3 ;
			float t ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Input.GetMouseButton( i ) == true )
				{
					if( m_State[ i ] == 0 )
					{
						m_Press[ i ] = true ;
						m_State[ i ] = 1 ;
						m_Timer[ i ] = Time.realtimeSinceStartup ;
					}
					else
					if( m_State[ i ] == 1 )
					{
						t = Time.realtimeSinceStartup - m_Timer[ i ] ;

						if( t <  RepeatStartingTime )
						{
							m_Press[ i ] = false ;
						}
						else
						{
							m_Press[ i ] = true ;
							m_State[ i ] = 2 ;
							m_Timer[ i ] = Time.realtimeSinceStartup ;
						}
					}
					else
					if( m_State[ i ] == 2 )
					{
						t = Time.realtimeSinceStartup - m_Timer[ i ] ;

						if( t <  RepeatIntervalTime )
						{
							m_Press[ i ] = false ;
						}
						else
						{
							m_Press[ i ] = true ;
							m_State[ i ] = 2 ;
							m_Timer[ i ] = Time.realtimeSinceStartup ;
						}
					}
				}
				else
				{
					m_Press[ i ] = false ;
					m_State[ i ] = 0 ;
					m_Timer[ i ] = 0 ;
				}
			}
		}

		/// <summary>
		/// リピート付きでマウスボタンの状態を取得する
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public static bool GetSmartButton( int index )
		{
			if( index <  0 || index >= m_Press.Length )
			{
				return false ;
			}

			return m_Press[ index ] ;
		}
	}
}

