using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

namespace uGUIHelper
{
	/// <summary>
	/// フォントフィルター情報
	/// </summary>
	[CreateAssetMenu( fileName = "FontFilter", menuName = "ScriptableObject/uGUIHelper/FontFilter" )]
	public class FontFilter : ScriptableObject
	{
		public byte[] flag = new byte[ 8192 ] ;	// 65536ビットのフラグ
	}
}
