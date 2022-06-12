using System ;
using System.Collections.Generic ;
using System.Linq ;
using System.Text ;
using System.Threading.Tasks ;

using UnityEngine ;

namespace Template.MasterData
{
    public partial class SimpleData
    {
		// ＩＤによる検索を高速化するためのハッシュテーブル
		protected static Dictionary<long,SimpleData> m_Indices ;

		public    static void CreateIndices()
		{
			m_Indices = new Dictionary<long,SimpleData>() ;
			foreach( var record in m_Records )
			{
				m_Indices.Add( record.Id, record ) ;
			}
		}

		/// <summary>
		/// ＩＤから該当するレコードを取得する
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static SimpleData GetById( long id )
		{
			if( m_Indices != null )
			{
				// ハッシュを使用する
				return m_Indices.ContainsKey( id ) ? m_Indices[ id ] : null ;
			}

			// ハッシュを使用しない(Linqは重いため、毎フレーム実行するような処理からこのメソッドを呼ばないこと)
			return m_Records.FirstOrDefault( _ => _.Id == id ) ;
		}

		/// <summary>
		/// 識別子一覧を取得する
		/// </summary>
		/// <returns></returns>
		public static long[] GetIds()
		{
			long[] ids = m_Records.Select( _ => _.Id ).ToArray() ;
			return ids ;
		}

		/// <summary>
		/// 指定したグループに属するレコードの識別子群の配列を取得する
		/// </summary>
		/// <param name="groupId"></param>
		/// <returns></returns>
		public static long[] GetIdsByGroupId( long groupId )
		{
			long[] ids = m_Records.Where( _ => _.GroupId == groupId ).Select( _ => _.Id ).ToArray() ;
			return ids ;
		}

		/// <summary>
		/// 識別子群から該当するレコード群を取得する
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		public static List<MasterData.SimpleData> GetsByIds( params long[] ids )
		{
			List<MasterData.SimpleData> records = new List<SimpleData>() ;

			foreach( var id in ids )
			{
				var record = GetById( id ) ;
				if( record != null && records.Contains( record ) == false )
				{
					records.Add( record ) ;
				}
			}

			if( records.Count == 0 )
			{
				return null ;
			}

			return records ;
		}
	}
}


