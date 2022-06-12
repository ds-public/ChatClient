using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using Cysharp.Threading.Tasks ;

using uGUIHelper ;

namespace Template.Screens
{
	/// <summary>
	/// タイトルの制御処理
	/// </summary>
	public partial class Title
	{
		/// <summary>
		/// 行動選択の種類
		/// </summary>
		public enum ActionTypes
		{
			Unknown,
			Start,
			Menu,
			LogOut,
			Refresh,
		}

		//-------------------------------------------------------------------------------------------

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// アクション選択
		/// </summary>
		/// <returns></returns>
		private async UniTask<State> State_ActionSelecting( State previous )
		{
			//----------------------------------------------------------

			ActionTypes actionType = ActionTypes.Unknown ;

			//----------------------------------

			// ブロックマスクを無効化する
			Blocker.Off() ;

			//----------------------------------------------------------

			// アクション待ち
			while( true )
			{
				if( actionType != ActionTypes.Unknown )
				{
					break ;
				}

				await Yield() ;
			}

			//------------------------------------------------------------------------------------------


			State next = State.Unknown ;


			Blocker.On() ;

			return next ;
		}

		//-------------------------------------------------------------------------------------------
	}
}
