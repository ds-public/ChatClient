using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerHelper
{
	/// <summary>
	/// ルーム情報
	/// </summary>
	public class MultiplayerRoomData
	{
		/// <summary>
		/// 名前
		/// </summary>
		public string	name ;

		/// <summary>
		/// 最大最大プレイヤー数(0で無制限)
		/// </summary>
		public int		maxPlayers ;

		/// <summary>
		/// 表示可能かどうか
		/// </summary>
		public bool		visible ;

		/// <summary>
		/// 入室可能かどうか
		/// </summary>
		public bool		open ;

		/// <summary>
		/// パスワードが設定されているかどうか
		/// </summary>
		public string	password ;

		/// <summary>
		/// コメント
		/// </summary>
		public string	comment ;

		/// <summary>
		/// 現在のプレイヤー数
		/// </summary>
		public int		playerCount ;
	}
}

