using System ;
using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;

namespace Template.Screens
{
	/// <summary>
	/// チャットの制御処理
	/// </summary>
	public partial class ChatClient
	{
		/// <summary>
		/// 状態(ステート)
		/// </summary>
		public enum State
		{
			/// <summary>
			/// 行動選択
			/// </summary>
			ActionSelecting,

			//----------------------------------

			/// <summary>
			/// 不明
			/// </summary>
			Unknown,
		}
	}
}
