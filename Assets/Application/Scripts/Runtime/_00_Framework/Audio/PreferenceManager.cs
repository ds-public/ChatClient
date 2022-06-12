using UnityEngine ;
using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine.Assertions ;
using System.Linq ;

using StorageHelper ;

/// <summary>
/// Ｂｅａｓｔパッケージ
/// </summary>
namespace Template
{
	/// <summary>
	/// プリファレンス系情報を保持するクラス Version 2021/01/30 0
	/// </summary>
	public class PreferenceManager : SingletonManagerBase<PreferenceManager>
	{
		//---------------------------------------------------------------------------

		// カテゴリごとに分かれたプリファレンス系データ
		
		/// <summary>
		/// 多目的データ(いずれ無くなる可能性大)
		/// </summary>
		public Preference Preference = new Preference() ;

		//---------------------------------------------------------------------------

		new protected void Awake()
		{
			base.Awake() ;

			// プリファレンスをストレージから読み出す
			Load_Private() ;
		}
		
		//---------------------------------------------------------------------------

		/// <summary>
		/// 全てのプリファレンス系データをロードする
		/// </summary>
		/// <returns></returns>
		public static bool Load()
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.Load_Private() ;
		}

		// 全てのプリファレンス系データをロードする
		private bool Load_Private()
		{
			string path = GetPath() ;

			if( string.IsNullOrEmpty( path ) == true )
			{
				// パスが設定されていない
				return false ;
			}

			if( StorageAccessor.Exists( path ) != StorageAccessor.Target.File )
			{
				// ファイルが存在しない
				return false ;
			}

			( string key, string vector ) = GetCryptoKeyAndVector() ;

			string json = StorageAccessor.LoadText( path, key, vector ) ;
			if( string.IsNullOrEmpty( json ) == true )
			{
				return false ;
			}

//			Debug.LogWarning( "LOAD:" + json + " : " + json.Length ) ;

			JsonUtility.FromJsonOverwrite( json, Preference ) ;

			return true ;
		}

		/// <summary>
		/// 全てのプリファレンス系データをセーブする
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Save()
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.Save_Private() ;
		}

		// 全てのプリファレンス系データをセーブする
		private bool Save_Private()
		{
			string path = GetPath() ;

//			Debug.Log( "プリファレンスの保存先:" + path ) ;

			if( string.IsNullOrEmpty( path ) == true )
			{
				// パスが設定されていない
//				Debug.LogError( "パスが設定されていません:" + GetType() ) ;
				return false ;
			}

			string json = JsonUtility.ToJson( Preference ) ;

//			Debug.LogWarning( "SAVE:" + json + " " + json.Length ) ;

			( string key, string vector ) = GetCryptoKeyAndVector() ;

			return StorageAccessor.SaveText( path, json, true, key, vector ) ;
		}

		/// <summary>
		/// 既にセーブされたファイルがストレージに存在するか
		/// </summary>
		/// <returns></returns>
		public bool IsSaved
		{
			get
			{
				string path = GetPath() ;

				if( string.IsNullOrEmpty( path ) == true )
				{
					// パスが設定されていない
					return false ;
				}

				if( StorageAccessor.Exists( path ) != StorageAccessor.Target.File )
				{
					return false ;
				}

				return true ;
			}
		}

		//---------------------------------------------------------------------------

		private const string m_FilePath = "preference" ;

		// パスを取得する
		private string GetPath()
		{
			string path = m_FilePath ;

			// 暗号化
			if( Define.SecurityEnabled == true )
			{
				path = Security.GetHash( path ) ;
			}

			path = "System/" + path ;

			return path ;
		}

		// 暗号化のキーとベクターを取得する
		private ( string, string ) GetCryptoKeyAndVector()
		{
			string key		= null ;
			string vector	= null ;

			// 暗号化
			if( Define.SecurityEnabled == true )
			{
				key		= Define.CryptoKey ;
				vector	= Define.CryptoVector ;
			}

			return ( key, vector ) ;
		}

	}

}

