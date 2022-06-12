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
	/// アセットインポートプロセッサーの基底クラス
	/// </summary>
	public class ImportProcessor
	{
		//-------------------------------------------------------------------------------------------
		// Postprocess 系

		/// <summary>
		/// マテリアルのフィードが行われた際に呼び出されます
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		/// <param name="material"></param>
		/// <param name="renderer"></param>
		public virtual Material OnAssignMaterialModel( AssetPostprocessor assetPostprocessor, Material material, Renderer renderer )	=> null ;

		/// <summary>
		/// アニメーションクリップがインポートされた後に呼び出されます
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		/// <param name="material"></param>
		/// <param name="renderer"></param>
		public virtual void OnPostprocessAnimation( AssetPostprocessor assetPostprocessor, GameObject root, AnimationClip clip ){}

		/// <summary>
		/// アセットバンドルの識別名が変更された後に呼び出されます
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		/// <param name="assetPath"></param>
		/// <param name="previousAssetBundleName"></param>
		/// <param name="newAssetBundleName"></param>
		public virtual void OnPostprocessAssetbundleName( AssetPostprocessor assetPostprocessor, string assetPath, string previousAssetBundleName, string newAssetBundleName ){}

		/// <summary>
		/// オーディオクリップがインポートされた後に呼び出されます
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		/// <param name="audioClip"></param>
		public virtual void OnPostprocessAudio( AssetPostprocessor assetPostprocessor, AudioClip audioClip ){}

		/// <summary>
		/// キューブマップがインポートされた後に呼び出されます
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		/// <param name="cubemap"></param>
		public virtual void OnPostprocessCubemap( AssetPostprocessor assetPostprocessor, Cubemap cubemap ){}

		/// <summary>
		/// ゲームオブジェクト(プレハブ)のプロパティが変更された後に呼び出されます
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		/// <param name="go"></param>
		/// <param name="prorertyNames"></param>
		/// <param name="values"></param>
		public virtual void OnPostprocessGameObjectWithAnimatedUserProperties( AssetPostprocessor assetPostprocessor, GameObject go,  EditorCurveBinding[] bindings ){}

		/// <summary>
		/// ゲームオブジェクト(プレハブ)のプロパティが変更された後に呼び出されます
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		/// <param name="go"></param>
		/// <param name="prorertyNames"></param>
		/// <param name="values"></param>
		public virtual void OnPostprocessGameObjectWithUserProperties( AssetPostprocessor assetPostprocessor, GameObject go, string[] prorertyNames, System.Object[] values ){}

		/// <summary>
		/// マテリアルがインポートされた後に呼び出されます
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		/// <param name="material"></param>
		public virtual void OnPostprocessMaterial( AssetPostprocessor assetPostprocessor, Material material ){}

		/// <summary>
		/// メッシュがインポートされた後に呼び出されます
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		/// <param name="material"></param>
		public virtual void OnPostprocessMeshHierarchy( AssetPostprocessor assetPostprocessor, GameObject root ){}

		/// <summary>
		/// モデルがインポートされた後に呼び出されます
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		/// <param name="go"></param>
		public virtual void OnPostprocessModel( AssetPostprocessor assetPostprocessor, GameObject go ){}

		/// <summary>
		/// スピードツリーがインポートされた後に呼び出されます
		/// </summary>
		public virtual void OnPostprocessSpeedTree( AssetPostprocessor assetPostprocessor, GameObject go ){}

		/// <summary>
		/// スプライトがインポートされた後に呼び出されます
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		/// <param name="texture"></param>
		/// <param name="sprites"></param>
		public virtual void OnPostprocessSprites( AssetPostprocessor assetPostprocessor, Texture2D texture, Sprite[] sprites ){}

		/// <summary>
		/// テクスチャがインポートされた後に呼び出されます
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		/// <param name="texture"></param>
		public virtual void OnPostprocessTexture( AssetPostprocessor assetPostprocessor, Texture2D texture ){}

		//-------------------------------------------------------------------------------------------
		// Preprocess 系

		/// <summary>
		/// アニメーションクリップがインポートされる前に呼び出されます
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		public virtual void OnPreprocessAnimation( AssetPostprocessor assetPostprocessor ){}

		/// <summary>
		/// アセットがインポートされる前に呼び出されます(このメソッドを継承して処理を追加するのが最も負荷が高い:極力使用しないこと)
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		public virtual void OnPreprocessAsset( AssetPostprocessor assetPostprocessor ){}

		/// <summary>
		/// オーディオクリップがインポートされる前に呼び出されます
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		public virtual void OnPreprocessAudio( AssetPostprocessor assetPostprocessor ){}

		/// <summary>
		/// マテリアルがインポートされる前に呼び出されます
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		public virtual void OnPreprocessMaterialDescription( AssetPostprocessor assetPostprocessor, MaterialDescription description, Material material, AnimationClip[] materialAnimations ){}

		/// <summary>
		/// モデルがインポートされる前に呼び出されます
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		public virtual void OnPreprocessModel( AssetPostprocessor assetPostprocessor ){}

		/// <summary>
		/// スピードツリーがインポートされる前に呼び出されます
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		public virtual void OnPreprocessSpeedTree( AssetPostprocessor assetPostprocessor ){}

		/// <summary>
		/// テクスチャがインポートされる前に呼び出されます
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		public virtual void OnPreprocessTexture( AssetPostprocessor assetPostprocessor ){}

		//-------------------------------------------------------------------------------------------
		// OnPostprocessAllAssets 系

		/// <summary>
		/// アセットが追加された後に呼び出されます
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public virtual bool OnAssetImported( string path )						=> false ;

		/// <summary>
		/// アセットが移動された後に呼び出されます
		/// </summary>
		/// <param name="pathTo"></param>
		/// <param name="pathFrom"></param>
		/// <returns></returns>
		public virtual bool OnAssetMoved( string pathTo, string pathFrom )		=> false ;

		/// <summary>
		/// アセットが削除された後に呼び出されます
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public virtual bool OnAssetDeleted( string path )						=> false ;

		/// <summary>
		/// アセットが追加または移動(先)された後に呼び出されます
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public virtual bool OnAssetImportedOrMoved( string path )				=> false ;

		/// <summary>
		/// アセットが削除または移動(元)された後に呼び出されます
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public virtual bool OnAssetDeletedOrMoved( string path )				=> false ;

		/// <summary>
		/// 全てのアセットのインポートが終了した後に呼び出されます
		/// </summary>
		/// <returns></returns>
		public virtual bool OnPostprocessAllAssetsFinished()					=> false ;
	}
}
