#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UISlider のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UISlider ) ) ]
	public class UISliderInspector : UIViewInspector
	{
		override protected void DrawInspectorGUI()
		{
//			UISlider tTarget = target as UISlider ;

//			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------
		}
	}
}

#endif
