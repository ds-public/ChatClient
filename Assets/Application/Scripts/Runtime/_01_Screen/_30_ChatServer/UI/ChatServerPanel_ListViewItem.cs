using System ;
using System.Collections ;
using System.Collections.Generic ;

using Cysharp.Threading.Tasks ;

using uGUIHelper ;

using UnityEngine ;

namespace Template.Screens.ChatServerClasses.UI
{
	/// <summary>
	/// チャットサーバー用ＵＩ
	/// </summary>
	public class ChatServerPanel_ListViewItem : ExMonoBehaviour
	{
		[SerializeField]
		protected UITextMesh	m_Label ;

		[SerializeField]
		protected UITextMesh	m_Message ;

		/// <summary>
		/// スタイルを設定する
		/// </summary>
		/// <param name="label"></param>
		/// <param name="message"></param>
		/// <param name="color"></param>
		public void SetStyle( string label, string message, Color color )
		{
			m_Label.Text	= label ;
			m_Label.Color	= color ;

			m_Message.Text	= message ;
			m_Message.Color	= color ;
		}
	}
}

