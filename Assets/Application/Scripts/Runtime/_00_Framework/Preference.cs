using UnityEngine ;
using System ;
using System.Collections ;
using System.Collections.Generic ;

using StorageHelper ;

namespace Template
{
	/// <summary>
	/// 多目的パラメータ保存用データクラス Version 2022/01/30
	/// </summary>
	[Serializable]
	public class Preference : ISerializationCallbackReceiver
	{
		/// <summary>
		/// 定義されたキー名の一覧
		/// </summary>
		public static class Key
		{
			public const string MasterDataHash	= "MasterDataHash" ;

			// 育成完了時の称号選択ダイアログ非表示設定
			public const string AliasSelectSkip	= "AliasSelectSkip" ;

			// 遠征機能解放後の機能チュートリアルState
			public const string AdventureTutorialState	= "AdventureTutorialState" ;
		}

		//-------------------------------------------------------------------------------

		/// <summary>
		/// ショートカットアクセス
		/// </summary>
		public static Preference Data
		{
			get
			{
				if( PreferenceManager.Instance == null )
				{
					return null ;
				}
				return PreferenceManager.Instance.Preference ;
			}
		}

		//-------------------------------------------------------------------------------

		/// <summary>
		/// ストレージにセーブする
		/// </summary>
		/// <returns></returns>
		public static bool Save()
		{
			if( PreferenceManager.Instance == null )
			{
				return false ;
			}
			return PreferenceManager.Save() ;
		}

		/// <summary>
		/// ストレージからロードする
		/// </summary>
		/// <returns></returns>
		public static bool Load()
		{
			if( PreferenceManager.Instance == null )
			{
				return false ;
			}
			return PreferenceManager.Load() ;
		}

		//-------------------------------------------------------------------------------

		/// <summary>
		/// 値を設定する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <returns></returns>
		public static bool SetValue<T>( string key, T value )
		{
			if( string.IsNullOrEmpty( key ) == true )
			{
				return false ;
			}

			if( PreferenceManager.Instance == null )
			{
				return false ;
			}

			if( PreferenceManager.Instance.Preference == null )
			{
				return false ;
			}

			PreferenceManager.Instance.Preference[ key, typeof( T ) ] = value ;

			return true ;
		}

		/// <summary>
		/// 値を取得する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <returns></returns>
		public static T GetValue<T>( string key, T defaultValue = default )
		{
			if( string.IsNullOrEmpty( key ) == true )
			{
				return defaultValue ;
			}

			if( PreferenceManager.Instance == null )
			{
				return defaultValue ;
			}

			if( PreferenceManager.Instance.Preference == null )
			{
				return defaultValue ;
			}

			System.Object value = PreferenceManager.Instance.Preference[ key, typeof( T ) ] ;
			if( value == null )
			{
				return defaultValue ;
			}

			return ( T )value ;
		}

		/// <summary>
		/// キーが存在するか確認する
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public static bool HasKey( string key )
		{
			if( string.IsNullOrEmpty( key ) == true )
			{
				return false ;
			}

			if( PreferenceManager.Instance == null )
			{
				return false ;
			}

			if( PreferenceManager.Instance.Preference == null )
			{
				return false ;
			}

			return PreferenceManager.Instance.Preference.m_Hash.ContainsKey( key ) ;
		}

		/// <summary>
		/// キーと対応するバリューを削除する
		/// </summary>
		/// <param name="key"></param>
		public static bool Delete( string key )
		{
			if( string.IsNullOrEmpty( key ) == true )
			{
				return false ;
			}

			if( PreferenceManager.Instance == null )
			{
				return false ;
			}

			if( PreferenceManager.Instance.Preference == null )
			{
				return false ;
			}

			if( PreferenceManager.Instance.Preference.m_Hash.ContainsKey( key ) == false )
			{
				return false ;
			}

			PreferenceManager.Instance.Preference.m_Hash.Remove( key ) ;

			return true ;
		}

		/// <summary>
		/// 全てのキーとバリューを消去する
		/// </summary>
		public static bool Clear()
		{
			if( PreferenceManager.Instance == null )
			{
				return false ;
			}

			if( PreferenceManager.Instance.Preference == null )
			{
				return false ;
			}

			PreferenceManager.Instance.Preference.m_Hash.Clear() ;

			return true ;
		}

		//-------------------------------------------------------------------------------

		/// <summary>
		/// キーとバリュー(直接アクセス) ※インデクサを使う事を推奨
		/// </summary>
		private readonly Dictionary<string,string> m_Hash = new Dictionary<string,string>() ;

		[Serializable]
		public class Element
		{
			public string	Key ;
			public string	Value ;
		}

		[SerializeField]
		private Element[]	m_Elements ;

		//-----------------------------------------------------------

		/// <summary>
		/// インデクサでの直接アクセス(内部的には全て文字列として保存されている)
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public string this[ string key ]
		{
			get
			{
				if( string.IsNullOrEmpty( key ) == true )
				{
					return null ;
				}

				if( m_Hash.ContainsKey( key ) == false )
				{
					return null ;
				}

				return m_Hash[ key ] ;
			}
			set
			{
				if( string.IsNullOrEmpty( key ) == true )
				{
					return ;
				}

				if( m_Hash.ContainsKey( key ) == false )
				{
					m_Hash.Add( key, value ) ;
				}
				else
				{
					m_Hash[ key ] = value ;
				}
			}
		}

		/// <summary>
		/// インデクサでの直接アクセス(内部的には全て文字列として保存されている)
		/// </summary>
		/// <param name="key"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public System.Object this[ string key, Type type ]
		{
			get
			{
				if( string.IsNullOrEmpty( key ) == true )
				{
					return null ;
				}

				if( m_Hash.ContainsKey( key ) == false )
				{
					return null ;
				}

				if( type == typeof( bool ) )
				{
					bool.TryParse( m_Hash[ key ], out bool value ) ;
					return value ;
				}
				else
				if( type == typeof( byte ) )
				{
					if( byte.TryParse( m_Hash[ key ], out byte value ) == false )
					{
						value = 0 ;
					}
					return value ;
				}
				else
				if( type == typeof( char ) )
				{
					if( char.TryParse( m_Hash[ key ], out char value ) == false )
					{
						value = ( char )0 ;
					}
					return value ;
				}
				else
				if( type == typeof( short ) )
				{
					if( short.TryParse( m_Hash[ key ], out short value ) == false )
					{
						value = 0 ;
					}
					return value ;
				}
				else
				if( type == typeof( ushort ) )
				{
					if( ushort.TryParse( m_Hash[ key ], out ushort value ) == false )
					{
						value = 0 ;
					}
					return value ;
				}
				else
				if( type == typeof( int ) )
				{
					if( int.TryParse( m_Hash[ key ], out int value ) == false )
					{
						value = 0 ;
					}
					return value ;
				}
				else
				if( type == typeof( uint ) )
				{
					if( uint.TryParse( m_Hash[ key ], out uint value ) == false )
					{
						value = 0 ;
					}
					return value ;
				}
				else
				if( type == typeof( long ) )
				{
					if( long.TryParse( m_Hash[ key ], out long value ) == false )
					{
						value = 0 ;
					}
					return value ;
				}
				else
				if( type == typeof( ulong ) )
				{
					if( ulong.TryParse( m_Hash[ key ], out ulong value ) == false )
					{
						value = 0 ;
					}
					return value ;
				}
				else
				if( type == typeof( float ) )
				{
					if( float.TryParse( m_Hash[ key ], out float value ) == false )
					{
						value = 0 ;
					}
					return value ;
				}
				else
				if( type == typeof( double ) )
				{
					if( double.TryParse( m_Hash[ key ], out double value ) == false )
					{
						value = 0 ;
					}
					return value ;
				}
				
				return m_Hash[ key ] ;
			}
			set
			{
				if( string.IsNullOrEmpty( key ) == true )
				{
					return ;
				}

				if( m_Hash.ContainsKey( key ) == false )
				{
					m_Hash.Add( key, value.ToString() ) ;
				}
				else
				{
					m_Hash[ key ] = value.ToString() ;
				}
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// シリアライズ前に呼び出される
		/// </summary>
		public void OnBeforeSerialize()
		{
			// ハッシュをシリアライズ用の配列に格納する
			int i, l = m_Hash.Keys.Count ;

			if( l >  0 )
			{
				m_Elements = new Element[ l ] ;

				string[] keys = new string[ l ] ;
				m_Hash.Keys.CopyTo( keys, 0 ) ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					m_Elements[ i ] = new Element(){ Key = keys[ i ], Value = m_Hash[ keys[ i ] ] } ;
				}
			}
			else
			{
				m_Elements = null ;
			}
		}

		/// <summary>
		/// デシリアライズ後に呼び出される
		/// </summary>
		public void OnAfterDeserialize()
		{
			// 高速化のためハッシュに展開する
			m_Hash.Clear() ;

			if( m_Elements != null && m_Elements.Length >  0 )
			{
				int i, l = m_Elements.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					m_Hash.Add( m_Elements[ i ].Key, m_Elements[ i ].Value ) ;
				}
			}
		}
	}
}
