#if UNITY_EDITOR

using UnityEngine ;
using UnityEngine.UI ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIGraphicRaycaster のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( GraphicRaycasterWrapper ) ) ]
	public class GraphicRaycasterWrapperInspector : Editor
	{
		/// <summary>
		/// スンスペクター描画
		/// </summary>
		public override void OnInspectorGUI()
		{
			// とりあえずデフォルト
			//	DrawDefaultInspector() ;

			//--------------------------------------------

			// ターゲットのインスタンス
			GraphicRaycasterWrapper tTarget = target as GraphicRaycasterWrapper ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-----------------------
		
			bool tIgnoreReversedGraphics = EditorGUILayout.Toggle( "Ignore Reversed Graphics", tTarget.ignoreReversedGraphics ) ;
			if( tIgnoreReversedGraphics != tTarget.ignoreReversedGraphics )
			{
				Undo.RecordObject( tTarget, "UIGraphicRaycaster : Ignore Reversed Graphics Change" ) ;	// アンドウバッファに登録
				tTarget.ignoreReversedGraphics = tIgnoreReversedGraphics ;
				EditorUtility.SetDirty( tTarget ) ;
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			GraphicRaycaster.BlockingObjects tBlockingObjects = ( GraphicRaycaster.BlockingObjects )EditorGUILayout.EnumPopup( "Blocking Objects",  tTarget.blockingObjects ) ;
			if( tBlockingObjects != tTarget.blockingObjects )
			{
				Undo.RecordObject( tTarget, "UIGraphicRaycaster : Blocking Objects Change" ) ;	// アンドウバッファに登録
				tTarget.blockingObjects = tBlockingObjects ;
				EditorUtility.SetDirty( tTarget ) ;
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			LayerMask tBlockingMask = LayerMaskField( "Blocking Mask", tTarget.BlockingMask ) ;
			if( tBlockingMask != tTarget.BlockingMask )
			{
				Undo.RecordObject( tTarget, "UIGraphicRaycaster : Blocking Mask Change" ) ;	// アンドウバッファに登録
				tTarget.BlockingMask = tBlockingMask ;
				EditorUtility.SetDirty( tTarget ) ;
				if( Application.isPlaying == false )
				{
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
			}

			//-----------------------
		
			EditorGUILayout.Separator() ;	// 少し区切りスペース

			RectTransform tOffScreenImage = EditorGUILayout.ObjectField( "Off Screen Image", tTarget.OffScreenImage, typeof( RectTransform ), true ) as RectTransform ;
			if( tOffScreenImage != tTarget.OffScreenImage )
			{
				Undo.RecordObject( tTarget, "UIGraphicRaycaster : Off Screen Image Change" ) ;	// アンドウバッファに登録
				tTarget.OffScreenImage = tOffScreenImage ;
				EditorUtility.SetDirty( tTarget ) ;
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}
		}


		/// <summary>
		/// レイヤーマスクＧＵＩ
		/// </summary>
		/// <param name="tLabel"></param>
		/// <param name="tValue"></param>
		/// <returns></returns>
		private LayerMask LayerMaskField( string tDescription, LayerMask tLayerMask )
		{
			int i ;
			List<int> tIndexList = new List<int>() ;
			List<string> tLabelList = new List<string>() ;

			int tValue = tLayerMask.value ;

			int c = 0 ;
			int tMaskOld = 0 ;

			for( i  =  0 ; i <  32 ; i ++ )
			{
				if( string .IsNullOrEmpty( LayerMask.LayerToName( i ) ) == false )
				{
					tIndexList.Add( i ) ;
					tLabelList.Add( LayerMask.LayerToName( i ) ) ;
					if( ( tValue & ( 1 << i ) ) != 0 )
					{
						tMaskOld |= ( 1 << c ) ;
					}

					c ++ ;
				}
			}

			int tMaskNew = EditorGUILayout.MaskField( tDescription, tMaskOld, tLabelList.ToArray() ) ;
			if( tMaskNew != tMaskOld )
			{
				tValue = 0 ;
				for( i  = 0 ; i <  c ; i ++ )
				{
					if( ( tMaskNew & ( 1 << i ) ) != 0 )
					{
						tValue |= ( 1 << tIndexList[ i ] ) ;
					}
				}
				tLayerMask = new LayerMask()
				{
					value = tValue
				} ;
			}

			return tLayerMask ;
		}
	}
}

#endif
