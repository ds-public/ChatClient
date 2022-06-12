using System ;
using System.Collections.Generic ;
using System.Linq ;
using System.Threading.Tasks ;
using UnityEngine ;

namespace Template.NetworkData
{
	[Serializable]
	public partial class ChatData
	{
		/// <summary>
		/// 発言者
		/// </summary>
		public	string	Speaker ;

		/// <summary>
		/// 文章
		/// </summary>
		public	string	Message ;
	}
}
