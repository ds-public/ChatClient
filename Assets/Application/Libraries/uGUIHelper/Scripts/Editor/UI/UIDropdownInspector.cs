#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIDropdown のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIDropdown ) ) ]
	public class UIDropdownInspector : UIViewInspector
	{
		override protected void DrawInspectorGUI()
		{
//			UIDropdown tTarget = target as UIDropdown ;
			
//			EditorGUILayout.Separator() ;	// 少し区切りスペース
			
			//-------------------------------------------------------------------
		
		}
	}
}

#endif
