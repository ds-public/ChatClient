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
	[ CustomEditor( typeof( UIAlphaMaskWindow ) ) ]
	public class UIAlphaMaskWindowInspector : Editor
	{
		/// <summary>
		/// スンスペクター描画
		/// </summary>
		public override void OnInspectorGUI()
		{
			// とりあえずデフォルト
			DrawDefaultInspector() ;

			//--------------------------------------------

			EditorGUILayout.HelpBox( "独自のマテリアルを設定したい場合を除いて、\nマテリアルは null で問題ありません。\nテクスチャのみ設定してください。", MessageType.Info ) ;
		}
	}
}

#endif
