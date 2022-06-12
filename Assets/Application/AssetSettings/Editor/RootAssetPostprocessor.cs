using System ;
using System.Linq ;
using System.IO ;
using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;
using UnityEditor ;
using UnityEditor.AssetImporters ;

// 参考
// https://qiita.com/TD12734/items/ca4308f01f6caf5d90c4

namespace Template.AssetSettings
{
	/// <summary>
	/// インポーターのルートクラス Version 2022/05/04
	/// </summary>
	public class RootAssetPostprocessor : AssetPostprocessor
	{
		//-------------------------------------------------------------------------------------------

		// 各種インポートプロセッサーを登録してください
		private static readonly List<ImportProcessor> m_ImportProcessors = new List<ImportProcessor>()
		{
			// テクスチャ関連
//			new TextureSettings(),				// コマンドで必要に応じて最適化
//			new SpineSpritesSettings(),			// コマンドで必要に応じて最適化
		} ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// バッチ処理中かどうか
		/// </summary>
		public static bool isBatching = false ;

		/// <summary>
		/// 全てのインポートが終了した後に呼び出されます
		/// </summary>
		/// <param name="importedAssetPaths"></param>
		/// <param name="deletedAssetPaths"></param>
		/// <param name="movedAssetPaths"></param>
		/// <param name="movedFromAssetPaths"></param>
		public static void OnPostprocessAllAssets
		(
			string[] importedAssetPaths,
			string[] deletedAssetPaths,
			string[] movedAssetPaths,
			string[] movedFromAssetPaths
		)
		{
			int i, l ;

			//------------------------------------------------------------------------------------------

//			Debug.Log( "コンパイル状態です:1:" + EditorApplication.isCompiling ) ;

			if( isBatching == true )
			{
				Debug.Log( "<color=#FFFF00>バッチ処理中であるため OnPostprocessAllAssets は無視されます</color>" ) ;
				return ;
			}

			// コンパイル状態を確認する
			if( EditorApplication.isCompiling == true )
			{
				// ソース・ファイルのインポートは全面的に無視する(ソースファイルの自動書き換えを行いたい場合は別の仕組みを用意する事)
				return ;
			}

			bool isSourceFile = true ;
			foreach( var importAssetPath in importedAssetPaths )
			{
				string extension = Path.GetExtension( importAssetPath ) ;
				if( extension != ".cs" && extension != ".js" )
				{
	//				Debug.Log( "ソースファイルなので以後の処理は無視する:2" ) ;
					// ソースファイルなので以後の処理は無視する
					isSourceFile = false ;
					break ;
				}
			}

			if( isSourceFile == true )
			{
				// 対象全てがソースファイルのインポートは全面的に無視する(ソースファイルの自動書き換えを行いたい場合は別の仕組みを用意する事)
				return ;
			}

			//------------------------------------------------------------------------------------------
			// Windows64 とMacOSX 環境でパス区切りが異なるので統一する

			if( importedAssetPaths != null && importedAssetPaths.Length >  0 )
			{
				l = importedAssetPaths.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					importedAssetPaths[ i ]		= importedAssetPaths[ i ].Replace( '\\', '/' ) ;
				}
			}

			// 移動が行われたもの
			if( movedAssetPaths != null && movedAssetPaths.Length >  0 )
			{
				l = movedAssetPaths.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					movedAssetPaths[ i ]		= movedAssetPaths[ i ].Replace( '\\', '/' ) ;
					movedFromAssetPaths[ i ]	= movedFromAssetPaths[ i ].Replace( '\\', '/' ) ;
				}
			}

			// 削除が行われたもの
			if( deletedAssetPaths != null && deletedAssetPaths.Length >  0 )
			{
				l = deletedAssetPaths.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					deletedAssetPaths[ i ]		= deletedAssetPaths[ i ].Replace( '\\', '/' ) ;
				}
			}

			//------------------------------------------------------------------------------------------

//			Debug.Log( "[コンパイル状態]" + EditorApplication.isCompiling ) ;

//			Debug.Log( "インポートあり: 追=" + importedAssetPaths.Length + " 削=" + deletedAssetPaths.Length + " 移(先)=" + movedAssetPaths.Length + " 移(元)=" + movedFromAssetPaths.Length ) ;
//			if( importedAssetPaths.Length >  0 )
//			{
//				Debug.Log( "ファイル数 : " + importedAssetPaths.Length + " " + importedAssetPaths[ 0 ] ) ;
//			}

			//------------------------------------------------------------------------------------------

			// アセットに対して何らかの変更が行われたか
			bool isDirty = false ;

			try
			{
				EditorApplication.LockReloadAssemblies() ;

				AssetDatabase.StartAssetEditing() ;


				// 追加・削除・移動が行われた

				//-------------------------------------------------------------------------
				// 各種インポーターへのフック

				// 追加が行われたもの(内部で再インポートしてはならない)
				if( importedAssetPaths != null && importedAssetPaths.Length >  0 )
				{
					l = importedAssetPaths.Length ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						isDirty |= OnAssetImported( importedAssetPaths[ i ] ) ;
					}
				}

				// 移動が行われたもの
				if( movedAssetPaths != null && movedAssetPaths.Length >  0 )
				{
					l = movedAssetPaths.Length ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						isDirty |= OnAssetMoved( movedAssetPaths[ i ], movedFromAssetPaths[ i ] ) ;
					}
				}

				// 削除が行われたもの
				if( deletedAssetPaths != null && deletedAssetPaths.Length >  0 )
				{
					l = deletedAssetPaths.Length ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						isDirty |= OnAssetDeleted( deletedAssetPaths[ i ] ) ;
					}
				}

				// 追加および移動(先)が行われたもの
				if( ( importedAssetPaths != null && importedAssetPaths.Length >  0 ) || ( movedAssetPaths != null && movedAssetPaths.Length >  0 ) )
				{
					// 統合化
					importedAssetPaths.Concat( movedAssetPaths ) ;
				}

				if( importedAssetPaths != null && importedAssetPaths.Length >  0 )
				{
					l = importedAssetPaths.Length ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						isDirty |= OnAssetImportedOrMoved( importedAssetPaths[ i ] ) ;
					}
				}

				// 削除および移動(元)が行われたもの
				if( ( deletedAssetPaths != null && deletedAssetPaths.Length >  0 ) || ( movedFromAssetPaths != null && movedFromAssetPaths.Length >  0 ) )
				{
					// 統合化
					deletedAssetPaths.Concat( movedFromAssetPaths ) ;
				}

				if( deletedAssetPaths != null && deletedAssetPaths.Length >  0 )
				{
					l = importedAssetPaths.Length ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						isDirty |= OnAssetDeletedOrMoved( importedAssetPaths[ i ] ) ;
					}
				}

				// 全てのアセットの追加・移動・削除が終了した
				isDirty |= OnPostprocessAllAssetsFinished() ;
			}
			finally
			{
				AssetDatabase.StopAssetEditing() ;

				if( isDirty == true )
				{
					// 何も処理をしていなければ保存はしないようにする
					Debug.Log( "<color=#FF0000>[OnPostprocessAllAssets] アセットに対して何らかの変更が行われたため AssetDatabase.SaveAssets() を実行します</color>" ) ;
					AssetDatabase.SaveAssets() ;

//					AssetDatabase.Refresh() ;
					AssetDatabase.Refresh
					(
						ImportAssetOptions.ForceSynchronousImport |
						ImportAssetOptions.DontDownloadFromCacheServer
					) ;

					AssetDatabase.ReleaseCachedFileHandles() ;
				}

				EditorApplication.UnlockReloadAssemblies() ;
			}
		}

		//---------------------------------------------------------------------------

		/// <summary>
		/// マテリアルのフィードが行われた際に呼び出されます
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		/// <param name="material"></param>
		/// <param name="renderer"></param>
		internal Material OnAssignMaterialModel( Material material, Renderer renderer )
		{
			if( isBatching == true ){ return null ; }

			Material sourceMaterial = null ;

			foreach( var importProcessor in m_ImportProcessors )
			{
				sourceMaterial = importProcessor.OnAssignMaterialModel( this, material, renderer ) ;
				if( sourceMaterial != null )
				{
					break ;
				}
			}

			return sourceMaterial ;
		}

		/// <summary>
		/// アニメーションクリップがインポートされた後に呼び出されます
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		/// <param name="material"></param>
		/// <param name="renderer"></param>
		internal void OnPostprocessAnimation( GameObject root, AnimationClip clip )
		{
			if( isBatching == true ){ return ; }

			foreach( var importProcessor in m_ImportProcessors )
			{
				importProcessor.OnPostprocessAnimation( this, root, clip ) ;
			}
		}

		/// <summary>
		/// アセットバンドルの識別名が変更された後に呼び出されます
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		/// <param name="assetPath"></param>
		/// <param name="previousAssetBundleName"></param>
		/// <param name="newAssetBundleName"></param>
		internal void OnPostprocessAssetbundleName( string assetPath, string previousAssetBundleName, string newAssetBundleName )
		{
			if( isBatching == true ){ return ; }

			foreach( var importProcessor in m_ImportProcessors )
			{
				importProcessor.OnPostprocessAssetbundleName( this, assetPath, previousAssetBundleName, newAssetBundleName ) ;
			}
		}

		/// <summary>
		/// オーディオクリップがインポートされた後に呼び出されます
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		/// <param name="audioClip"></param>
		internal void OnPostprocessAudio( AudioClip audioClip )
		{
			if( isBatching == true ){ return ; }

			foreach( var importProcessor in m_ImportProcessors )
			{
				importProcessor.OnPostprocessAudio( this, audioClip ) ;
			}
		}

		/// <summary>
		/// キューブマップがインポートされた後に呼び出されます
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		/// <param name="cubemap"></param>
		internal void OnPostprocessCubemap( Cubemap cubemap )
		{
			if( isBatching == true ){ return ; }

			foreach( var importProcessor in m_ImportProcessors )
			{
				importProcessor.OnPostprocessCubemap( this, cubemap ) ;
			}
		}

		/// <summary>
		/// ゲームオブジェクト(プレハブ)のプロパティが変更された後に呼び出されます
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		/// <param name="go"></param>
		/// <param name="prorertyNames"></param>
		/// <param name="values"></param>
		internal void OnPostprocessGameObjectWithAnimatedUserProperties( GameObject go,  EditorCurveBinding[] bindings )
		{
			if( isBatching == true ){ return ; }

			foreach( var importProcessor in m_ImportProcessors )
			{
				importProcessor.OnPostprocessGameObjectWithAnimatedUserProperties( this, go, bindings ) ;
			}
		}

		/// <summary>
		/// ゲームオブジェクト(プレハブ)のプロパティが変更された後に呼び出されます
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		/// <param name="go"></param>
		/// <param name="prorertyNames"></param>
		/// <param name="values"></param>
		internal void OnPostprocessGameObjectWithUserProperties( GameObject go, string[] prorertyNames, System.Object[] values )
		{
			if( isBatching == true ){ return ; }

			foreach( var importProcessor in m_ImportProcessors )
			{
				importProcessor.OnPostprocessGameObjectWithUserProperties( this, go, prorertyNames, values ) ;
			}
		}

		/// <summary>
		/// マテリアルがインポートされた後に呼び出されます
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		/// <param name="material"></param>
		internal void OnPostprocessMaterial( Material material )
		{
			if( isBatching == true ){ return ; }

			foreach( var importProcessor in m_ImportProcessors )
			{
				importProcessor.OnPostprocessMaterial( this, material ) ;
			}
		}

		/// <summary>
		/// メッシュがインポートされた後に呼び出されます
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		/// <param name="material"></param>
		internal void OnPostprocessMeshHierarchy( GameObject root )
		{
			if( isBatching == true ){ return ; }

			foreach( var importProcessor in m_ImportProcessors )
			{
				importProcessor.OnPostprocessMeshHierarchy( this, root ) ;
			}
		}

		/// <summary>
		/// モデルがインポートされた後に呼び出されます
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		/// <param name="go"></param>
		internal void OnPostprocessModel( GameObject go )
		{
			if( isBatching == true ){ return ; }

			foreach( var importProcessor in m_ImportProcessors )
			{
				importProcessor.OnPostprocessModel( this, go ) ;
			}
		}

		/// <summary>
		/// スピードツリーがインポートされた後に呼び出されます
		/// </summary>
		internal void OnPostprocessSpeedTree( GameObject go )
		{
			if( isBatching == true ){ return ; }

			foreach( var importProcessor in m_ImportProcessors )
			{
				importProcessor.OnPostprocessSpeedTree( this, go ) ;
			}
		}

		/// <summary>
		/// スプライトがインポートされた後に呼び出されます
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		/// <param name="texture"></param>
		/// <param name="sprites"></param>
		internal void OnPostprocessSprites( Texture2D texture, Sprite[] sprites )
		{
			if( isBatching == true ){ return ; }

			foreach( var importProcessor in m_ImportProcessors )
			{
				importProcessor.OnPostprocessSprites( this, texture, sprites ) ;
			}
		}

		/// <summary>
		/// テクスチャがインポートされた後に呼び出されます
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		/// <param name="texture"></param>
		internal void OnPostprocessTexture( Texture2D texture )
		{
			if( isBatching == true ){ return ; }

			foreach( var importProcessor in m_ImportProcessors )
			{
				importProcessor.OnPostprocessTexture( this, texture ) ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// アニメーションクリップがインポートされる前に呼び出されます
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		internal void OnPreprocessAnimation()
		{
			if( isBatching == true ){ return ; }

			foreach( var importProcessor in m_ImportProcessors )
			{
				importProcessor.OnPreprocessAnimation( this ) ;
			}
		}

		/// <summary>
		/// アセットがインポートされる前に呼び出されます(ここの処理が最も高負荷となる)
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		internal void OnPreprocessAsset()
		{
			if( isBatching == true ){ return ; }

//			Debug.Log( "コンパイル状態です2:" + EditorApplication.isCompiling ) ;

			// このタイミングでは EditorApplication.isCompiling は false になってしまうので判定に意味はない
//			if( EditorApplication.isCompiling == true )
//			{
//				// ソース・ファイルのインポートは全面的に無視する(ソースファイルの自動書き換えを行いたい場合は別の仕組みを用意する事)
//				return ;
//			}

			string assetPath = assetImporter.assetPath ;
			string extension = Path.GetExtension( assetPath ) ;
			if( extension == ".cs" || extension == ".js" )
			{
//				Debug.Log( "ソースファイルなので以後の処理は無視する:2" ) ;
				// ソースファイルなので以後の処理は無視する
				return ;
			}

			//----------------------------------

			foreach( var importProcessor in m_ImportProcessors )
			{
				importProcessor.OnPreprocessAsset( this ) ;
			}
		}

		/// <summary>
		/// オーディオクリップがインポートされる前に呼び出されます
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		internal void OnPreprocessAudio()
		{
			if( isBatching == true ){ return ; }

			foreach( var importProcessor in m_ImportProcessors )
			{
				importProcessor.OnPreprocessAudio( this ) ;
			}
		}

		/// <summary>
		/// マテリアルがインポートされる前に呼び出されます
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		internal void OnPreprocessMaterialDescription( MaterialDescription description, Material material, AnimationClip[] materialAnimations )
		{
			if( isBatching == true ){ return ; }

			foreach( var importProcessor in m_ImportProcessors )
			{
				importProcessor.OnPreprocessMaterialDescription( this, description, material, materialAnimations ) ;
			}
		}

		/// <summary>
		/// モデルがインポートされる前に呼び出されます
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		internal void OnPreprocessModel()
		{
			if( isBatching == true ){ return ; }

			foreach( var importProcessor in m_ImportProcessors )
			{
				importProcessor.OnPreprocessModel( this ) ;
			}
		}

		/// <summary>
		/// スピードツリーがインポートされる前に呼び出されます
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		internal void OnPreprocessSpeedTree()
		{
			if( isBatching == true ){ return ; }

			foreach( var importProcessor in m_ImportProcessors )
			{
				importProcessor.OnPreprocessSpeedTree( this ) ;
			}
		}

		/// <summary>
		/// テクスチャがインポートされる前に呼び出されます
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		public virtual void OnPreprocessTexture()
		{
			if( isBatching == true ){ return ; }

			foreach( var importProcessor in m_ImportProcessors )
			{
				importProcessor.OnPreprocessTexture( this ) ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// アセットが追加された後に呼び出されます
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private static bool OnAssetImported( string path )
		{
			bool isDirty = false ;

			foreach( var importProcessor in m_ImportProcessors )
			{
				isDirty |= importProcessor.OnAssetImported( path ) ;
			}

			return isDirty ;
		}

		/// <summary>
		/// アセットが移動された後に呼び出されます
		/// </summary>
		/// <param name="pathTo"></param>
		/// <param name="pathFrom"></param>
		/// <returns></returns>
		private static bool OnAssetMoved( string pathTo, string pathFrom )
		{
			bool isDirty = false ;

			foreach( var importProcessor in m_ImportProcessors )
			{
				isDirty |= importProcessor.OnAssetMoved( pathTo, pathFrom ) ;
			}

			return isDirty ;
		}

		/// <summary>
		/// アセットが削除された後に呼び出されます
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private static bool OnAssetDeleted( string path )
		{
			bool isDirty = false ;

			foreach( var importProcessor in m_ImportProcessors )
			{
				isDirty |= importProcessor.OnAssetDeleted( path ) ;
			}

			return isDirty ;
		}

		/// <summary>
		/// アセットが追加または移動(先)された後に呼び出されます
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private static bool OnAssetImportedOrMoved( string path )
		{
			bool isDirty = false ;

			foreach( var importProcessor in m_ImportProcessors )
			{
				isDirty |= importProcessor.OnAssetImportedOrMoved( path ) ;
			}

			return isDirty ;
		}

		/// <summary>
		/// アセットが削除または移動(元)された後に呼び出されます
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private static bool OnAssetDeletedOrMoved( string path )
		{
			bool isDirty = false ;

			foreach( var importProcessor in m_ImportProcessors )
			{
				isDirty |= importProcessor.OnAssetDeletedOrMoved( path ) ;
			}

			return isDirty ;
		}

		/// <summary>
		/// 全てのアセットのインポートが終了した後に呼び出されます
		/// </summary>
		/// <returns></returns>
		private static bool OnPostprocessAllAssetsFinished()
		{
			bool isDirty = false ;

			foreach( var importProcessor in m_ImportProcessors )
			{
				isDirty |= importProcessor.OnPostprocessAllAssetsFinished() ;
			}

			return isDirty ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// バージョン
		/// </summary>
		/// <returns></returns>
		public override uint GetVersion()
		{
			return 0 ;
		}
	}
}
