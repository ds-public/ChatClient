using System ;
using UnityEngine ;

namespace MultiplayerHelper
{
	/// <summary>
	/// 非同期処理待ち状態クラス
	/// </summary>
	public class MultiplayerRequest : CustomYieldInstruction
	{
		private readonly MonoBehaviour m_Owner = default ;
		public MultiplayerRequest( MonoBehaviour owner )
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
		/// エラーが発生したかどうか
		/// </summary>
		public string Error = string.Empty ;
	}
}
