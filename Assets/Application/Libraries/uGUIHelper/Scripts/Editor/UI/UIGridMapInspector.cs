#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections ;

namespace uGUIHelper
{
	/// <summary>
	/// UIGridMap のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIGridMap ) ) ]
	public class UIGridMapInspector : UIViewInspector
	{
		/// <summary>
		/// スンスペクター描画
		/// </summary>
		override protected void DrawInspectorGUI()
		{
			// ターゲットのインスタンス
			UIGridMap tTarget = target as UIGridMap ;
		
			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------
			
			// マテリアル選択
			DrawMaterial( tTarget ) ;

			//-------------------------------------------------------------------
		}
	}
}

#endif
