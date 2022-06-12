#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIImageInversion のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIInversion ) ) ]
	public class UIInversionInspector : Editor
	{
		// スンスペクター描画
		public override void OnInspectorGUI()
		{
			// とりあえずデフォルト
	//		DrawDefaultInspector() ;
		
			//--------------------------------------------

			// ターゲットのインスタンス
			UIInversion tTarget = target as UIInversion ;
		
			EditorGUILayout.Separator() ;   // 少し区切りスペース

			// バリュータイプ
			UIInversion.DirectionTypes tDirection = ( UIInversion.DirectionTypes )EditorGUILayout.EnumPopup( "Direction",  tTarget.DirectionType ) ;
			if( tDirection != tTarget.DirectionType )
			{
				Undo.RecordObject( tTarget, "UIInversion : Direction Change" ) ;	// アンドウバッファに登録
				tTarget.DirectionType = tDirection ;
				tTarget.Refresh() ;
				EditorUtility.SetDirty( tTarget ) ;
			}
		}
	}
}

#endif
