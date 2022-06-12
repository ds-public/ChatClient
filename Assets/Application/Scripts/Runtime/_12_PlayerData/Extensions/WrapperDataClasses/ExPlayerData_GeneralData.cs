using System ;
using System.Collections.Generic ;
using System.Linq ;
using System.Threading.Tasks ;

using UnityEngine ;

namespace Template
{
	/// <summary>
	/// プレイヤーの共通データの機能拡張クラス Version 2022/05/04
	/// </summary>
	public partial class PlayerData
	{
		/// <summary>
		/// 共有データ
		/// </summary>
		public static GeneralData	General = new GeneralData() ;

		/// <summary>
		/// 共有データ
		/// </summary>
		public partial class GeneralData
		{
			//----------------------------------------------------------
			// UserProfile関連

			/// <summary>
			/// ブリーダー識別子
			/// </summary>
			public List<long>		Ids					=> PlayerData.Ids ;

			/// <summary>
			/// ログイン回数
			/// </summary>
			public int				LoginTimes			=> 0 ;				

			//----------------------------------------------------------

			/// <summary>
			/// 各値を文字列でまとめて取得する(デバッグ用)
			/// </summary>
			/// <returns></returns>
			new public string ToString()
			{
				string s = "----- PlayerData.GeneralData -----\n" ;
				return s ;
			}

			/// <summary>
			/// コンソールに出力する
			/// </summary>
			public void Print()
			{
				Debug.Log( ToString() ) ;
			}
		}
	}
}
