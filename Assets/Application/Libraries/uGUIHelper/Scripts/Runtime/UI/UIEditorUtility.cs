#if UNITY_EDITOR

using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;
using UnityEditor ;
using UnityEditor.SceneManagement ;

namespace uGUIHelper
{
	/// <summary>
	/// エディターモード専用ユーティリティ関数
	/// </summary>
	public static class UIEditorUtility
	{
		/// <summary>
		/// プロジェクト内の指定のコンポーネントを列挙する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="onLoaded"></param>
		/// <param name="rootPath"></param>
		/// <returns></returns>
		public static T[] FindComponents<T>( string rootPath, Action<T> onLoaded = null ) where T: UnityEngine.Object
		{
			List<T> targets = new List<T>() ;

			// Prefab
			string[] prefabGuids = AssetDatabase.FindAssets( "t:prefab", new string[]{ rootPath } ) ;
			if( prefabGuids != null && prefabGuids.Length >  0 )
			{
//				Debug.LogWarning( "全プレハブの数:" + prefabGuids.Length ) ;
				foreach( var guid in prefabGuids )
				{
					string path = AssetDatabase.GUIDToAssetPath( guid ) ;
					GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>( path ) ;

					T[] targetsInPrefab = go.GetComponentsInChildren<T>( true ) ;
					if( targetsInPrefab != null && targetsInPrefab.Length >  0 )
					{
//						Debug.LogWarning( "プレハブ " + path + " 内の UITween の数 = " + targetsInPrefab.Length ) ;

						targets.AddRange( targetsInPrefab ) ;

						if( onLoaded != null )
						{
							foreach( T target in targetsInPrefab )
							{
								onLoaded( target ) ;
								EditorUtility.SetDirty( target ) ;
							}
						}
					}
				}

				if( onLoaded != null )
				{
					AssetDatabase.SaveAssets() ;
					AssetDatabase.Refresh() ;
				}
			}

			// Scene
			string[] sceneGuids = AssetDatabase.FindAssets( "t:scene", new string[]{ rootPath } ) ;
			if( sceneGuids != null && sceneGuids.Length >  0 )
			{
				// 開いていたシーンを保存する
				var activeScene = EditorSceneManager.GetActiveScene() ;
//				Debug.LogWarning( "アクティブシーンのパス : " + activeScene.path ) ;
				string activeScenepath = activeScene.path ;

//				Debug.LogWarning( "全シーンの数:" + prefabGuids.Length ) ;
				foreach( var guid in sceneGuids )
				{
					string path = AssetDatabase.GUIDToAssetPath( guid ) ;
					var scene = EditorSceneManager.OpenScene( path ) ;
					
					T[] targetsInScene = Resources.FindObjectsOfTypeAll<T>() ;
					if( targetsInScene != null && targetsInScene.Length >  0 )
					{
//						Debug.LogWarning( "シーン " + path + " 内の UITween の数 = " + targetsInScene.Length ) ;
						targets.AddRange( targetsInScene ) ;

						if( onLoaded != null )
						{
							foreach( T target in targetsInScene )
							{
								onLoaded( target ) ;
							}

							EditorSceneManager.SaveScene( scene, path ) ;
						}
					}
				}

				if( string.IsNullOrEmpty( activeScenepath ) == false )
				{
					// 開いていたシーンに戻す
					activeScene = EditorSceneManager.GetActiveScene() ;
					if( activeScene.path != activeScenepath )
					{
						EditorSceneManager.OpenScene( activeScenepath ) ;
					}
				}
			}

			if( onLoaded != null )
			{
				AssetDatabase.SaveAssets() ;
				AssetDatabase.Refresh() ;
			}

//			Debug.LogWarning( "------>最終的な対象の数:" + targets.Count ) ;

			if( targets.Count == 0 )
			{
				return null ;
			}

			return targets.ToArray() ;
		}
	}
}

#endif
