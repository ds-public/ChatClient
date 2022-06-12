#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using UnityEditor.UI ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIGraphicEmpty のインスペクタークラス
	/// </summary>
	[CanEditMultipleObjects, CustomEditor( typeof( GraphicEmpty ), false )]
	public class GraphicEmptyInspector : GraphicEditor
	{
		/// <summary>
		/// スンスペクター描画
		/// </summary>
		public override void OnInspectorGUI()
		{
			 base.serializedObject.Update() ;
			 EditorGUILayout.PropertyField( base.m_Script, new GUILayoutOption[ 0 ] ) ;
			 base.RaycastControlsGUI() ;
			 base.serializedObject.ApplyModifiedProperties() ;
		}
	}
}

#endif
