#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// Line のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIAlphaMaskTarget ) ) ]
	public class UIAlphaMaskTargetInspector : Editor
	{
		/// <summary>
		/// スンスペクター描画
		/// </summary>
		public override void OnInspectorGUI()
		{
			// とりあえずデフォルト
			DrawDefaultInspector() ;

			//--------------------------------------------

			EditorGUILayout.HelpBox( "独自のマテリアルを設定したい場合を除いて、\nマテリアルは null で問題ありません。", MessageType.Info ) ;
		}
	}
}

#endif
