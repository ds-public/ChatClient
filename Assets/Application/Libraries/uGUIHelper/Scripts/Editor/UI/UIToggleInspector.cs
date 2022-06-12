#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIToggle のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIToggle ) ) ]
	public class UIToggleInspector : UIViewInspector
	{
		override protected void DrawInspectorGUI()
		{
//			UIToggle tTarget = target as UIToggle ;

//			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------
		}
	}
}

#endif
