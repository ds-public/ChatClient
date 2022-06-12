using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;
using UnityEngine ;

namespace uGUIHelper
{
	/// <summary>
	/// ボタングループ
	/// </summary>
	public class UIButtonGroup : MonoBehaviour
	{
		/// <summary>
		/// グループ内のボタンが全てオフ状態を許容するかどうか(trueで全てオフの状態を許容する)
		/// </summary>
		[SerializeField]
		protected bool m_AllowSwitchOff = false ;
		public    bool   AllowSwitchOff{ get{ return m_AllowSwitchOff ; } set{ m_AllowSwitchOff = value ; } }

		//-----------------------------------------------------------

		protected readonly List<UIButton> m_Buttons = new List<UIButton>() ;

		// このボタングループに属するボタンを登録する
		private void AddGroupButtons()
		{
			UIButton[] buttons = GetComponentsInChildren<UIButton>() ;
			foreach( var button in buttons )
			{
				if( button != this && button.TargetButtonGroup == this )
				{
					Add( button ) ;
				}
			}
		}

		internal void Awake()
		{
			// このボタングループに属するボタンを登録する
			m_Buttons.Clear() ;
			AddGroupButtons() ;
		}

		/// <summary>
		/// ボタンを追加する
		/// </summary>
		/// <param name="button"></param>
		public void Add( UIButton button )
		{
			if( m_Buttons.Contains( button ) == false )
			{
				m_Buttons.Add( button ) ;
			}
		}

		/// <summary>
		/// ボタンを削除する
		/// </summary>
		/// <param name="button"></param>
		public void Remove( UIButton button )
		{
			if( m_Buttons.Contains( button ) == true )
			{
				m_Buttons.Remove( button ) ;
			}
		}

		/// <summary>
		/// ボタングループの状態を更新する
		/// </summary>
		/// <param name="button"></param>
		public void SetState( UIButton targetButton, bool state, bool isClick )
		{
			// このボタングループに属するボタンを登録する(既に追加済みのものは無視する)
			AddGroupButtons() ;

			if( m_Buttons.Contains( targetButton ) == false )
			{
				return ;
			}

			if( state == true )
			{
				// オンにする
				targetButton.Interactable = false ;

				// 押されたもの以外をオフ状態にする
				foreach( var button in m_Buttons )
				{
					if( button != targetButton )
					{
						button.Interactable = true ;
					}
				}

				// コールバックを呼ぶ
				if( OnValueChanged != null )
				{
					string identity = targetButton.Identity ;
					if( string.IsNullOrEmpty( identity ) == true )
					{
						identity = targetButton.name ;
					}

					OnValueChanged( identity, targetButton, isClick ) ;
				}
			}
			else
			{
				// オフにする
				if( m_AllowSwitchOff == false )
				{
					return ;
				}

				// 全てオフを許容する
				targetButton.Interactable = true ;
			}
		}

		/// <summary>
		/// 選択中のボタンを取得する
		/// </summary>
		public UIButton Selection
		{
			get
			{
				return m_Buttons.FirstOrDefault( _ => ( _.Interactable == false ) ) ;
			}
		}

		/// <summary>
		/// ボタンが選択された際に呼び出されるコールバックメソッド
		/// </summary>
		public Action<string,UIButton,bool> OnValueChanged ;

		public void SetOnValueChanged( Action<string,UIButton,bool> onValueChanged )
		{
			OnValueChanged = onValueChanged ;
		}

		//-------------------------------------------------------------------------------------------
		// UIView 関係

		/// <summary>
		/// 表示状態を設定する
		/// </summary>
		/// <param name="state"></param>
		public void SetActive( bool state )
		{
			gameObject.SetActive( state ) ;
		}


		private UIView	m_View ;

		/// <summary>
		/// UIView のインスタンスを取得する
		/// </summary>
		public  UIView	  View
		{
			get
			{
				if( m_View == null )
				{
					m_View = GetComponent<UIView>() ;
				}
				return m_View ;
			}
		}
	}
}

