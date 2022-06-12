using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using Cysharp.Threading.Tasks ;

using UnityEngine ;

namespace Template.MasterData
{
	/// <summary>
	/// MasterData の基底クラス Version 2020/12/19
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class MasterDataBase<T> where T : class, new()
	{
		// レコードのフィールド
		protected static List<T> m_Records  ;

		// レコードのプロパティ
		public    static List<T>   Records{ get{ return m_Records ; } }

		// 配列数を取得する
		public    static int	Length
		{
			get
			{
				if( m_Records == null )
				{
					return 0 ;
				}
				return m_Records.Count ;
			}
		}

		// 配列数を取得する
		public    static int	Count
		{
			get
			{
				if( m_Records == null )
				{
					return 0 ;
				}
				return m_Records.Count ;
			}
		}

		//---------------------------------------------------------------------------

		/// <summary>
		/// MessagePack からマスターデータを展開する
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static bool Deserialize( byte[] data )
		{
			// Setting の DataType を反映させるには GetDataType() とする
			m_Records = DataPacker.Deserialize<List<T>>( data, false ) ;
			return ( m_Records != null ) ;
		}

		//---------------------------------------------------------------------------

		/// <summary>
		/// ロードする(同期)
		/// </summary>
		public static void Load( string path )
		{
			// ＣＳＶテキストファイルからマスターデータを展開する
			m_Records = MasterDataManager.LoadFromCsv<T>( path, 1, 3, 3, 4, 7, 2 ) ;
		}

		/// <summary>
		/// ロードする(非同期)
		/// </summary>
		/// <returns></returns>
		public static async UniTask LoadAsync( string path )
		{
			// ＣＳＶテキストファイルからマスターデータを展開する
			await MasterDataManager.LoadFromCsvAsync<T>( path, 1, 3, 3, 4, 7, 2, _ => m_Records = _ ) ;
		}
    }
}
