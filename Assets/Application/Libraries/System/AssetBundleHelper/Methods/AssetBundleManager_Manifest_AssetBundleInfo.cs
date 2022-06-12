using System ;
using System.Text ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using UnityEngine ;
using UnityEngine.Networking ;

using StorageHelper ;

/// <summary>
/// アセットバンドルヘルパーパッケージ
/// </summary>
namespace AssetBundleHelper
{
	/// <summary>
	/// アセットバンドルマネージャクラス(メソッド)
	/// </summary>
	public partial class AssetBundleManager
	{
		/// <summary>
		/// マニフェスト情報クラス
		/// </summary>
		public partial class ManifestInfo
		{
			// AssetBundleInfo

			/// <summary>
			/// どちらの格納場所が優先されるか
			/// </summary>
			public enum LocationPriorities
			{
				Storage,
				StreamingAssets,
			}


			/// <summary>
			/// アセットバンドル情報クラス
			/// </summary>
			[Serializable]
			public class AssetBundleInfo
			{
				/// <summary>
				/// マニフェスト内での相対パス
				/// </summary>
				public string	Path = String.Empty ;

				/// <summary>
				/// ハッシュ値
				/// </summary>
				public string	Hash = String.Empty ;

				/// <summary>
				/// アセットバンドルファイルのサイズ
				/// </summary>
				public int		Size = 0 ;			// サイズ(処理の高速化のためにここに保持しておく)※キャッシュオーバーなどの際の処理に使用する

				/// <summary>
				/// ＣＲＣ値(０で使用しない)
				/// </summary>
				public uint		Crc = 0 ;

				/// <summary>
				/// タグ
				/// </summary>
				public string[]	Tags = null ;

				/// <summary>
				/// 最終更新日時
				/// </summary>
				public long		LastUpdateTime = 0 ;

				//---------------------------------

				/// <summary>
				/// キャッシュオーバーする際に破棄可能にするかどうかを示す
				/// </summary>
				public bool		Keep = false ;

				/// <summary>
				/// 更新が必要がどうかを示す
				/// </summary>
				public bool		UpdateRequired = true ;		// 更新が必要かどうか

				/// <summary>
				/// Storage と StreamingAssets でどちらを優先するか
				/// </summary>
				public LocationPriorities	LocationPriority = LocationPriorities.Storage ;

				/// <summary>
				/// ダウンロード中かどうか
				/// </summary>
				public bool		IsDownloading = false ;

				/// <summary>
				/// 非同期アクセス時の排他ロックフラグ
				/// </summary>
				public bool		Busy = false ;

				/// <summary>
				/// コンストラクタ
				/// </summary>
				/// <param name="path">マニフェスト内での相対パス</param>
				/// <param name="hash">ハッシュ値</param>
				/// <param name="time">最終更新日時</param>
				public AssetBundleInfo( string path, string hash, int size, uint crc, string[] tags, long lastUpdateTime )
				{
					Path			= path ;
					Hash			= hash ;
					Size			= size ;
					Crc				= crc ;
					Tags			= tags ;
					LastUpdateTime	= lastUpdateTime ;
				}

				/// <summary>
				/// 指定したタグに該当するかどうか
				/// </summary>
				/// <param name="tag"></param>
				/// <returns></returns>
				public bool ContainsTag( string tag )
				{
					if( Tags == null || Tags.Length == 0 )
					{
						return false ;
					}

					return Tags.Contains( tag ) ;
				}
#if false
				// 対象がプレハブの場合にシェーダーを付け直す(Unity Editor で Platform が Android iOS の場合の固有バグ対策コード)
				private UnityEngine.Object ReplaceShader( UnityEngine.Object asset, Type type )
				{
#if( UNITY_EDITOR && UNITY_ANDROID ) || ( UNITY_EDITOR && ( UNITY_IOS || UNITY_IPHONE ) )
					if( asset is GameObject )
					{
						GameObject go = asset as GameObject ;

						Renderer[] renderers = go.GetComponentsInChildren<Renderer>( true ) ;
						if( renderers != null && renderers.Length >  0 )
						{
							foreach( var renderer in renderers )
							{
								if( renderer.sharedMaterials != null && renderer.sharedMaterials.Length >  0 )
								{
									foreach( var material in renderer.sharedMaterials )
									{
										if( material != null )
										{
											string shaderName = material.shader.ToString().Replace( " (UnityEngine.Shader)", "" ) ;
											material.shader = Shader.Find( shaderName ) ;
										}
									}
								}
							}
						}

						MeshRenderer[] meshRenderers = go.GetComponentsInChildren<MeshRenderer>( true ) ;
						if( meshRenderers != null && meshRenderers.Length >  0 )
						{
							foreach( var renderer in meshRenderers )
							{
								if( renderer.sharedMaterials != null && renderer.sharedMaterials.Length >  0 )
								{
									foreach( var material in renderer.sharedMaterials )
									{
										if( material != null )
										{
											string shaderName = material.shader.ToString().Replace( " (UnityEngine.Shader)", "" ) ;
											material.shader = Shader.Find( shaderName ) ;
										}
									}
								}
							}
						}

						SkinnedMeshRenderer[] skinnedMeshRenderers = go.GetComponentsInChildren<SkinnedMeshRenderer>( true ) ;
						if( skinnedMeshRenderers != null && skinnedMeshRenderers.Length >  0 )
						{
							foreach( var renderer in skinnedMeshRenderers )
							{
								if( renderer.sharedMaterials != null && renderer.sharedMaterials.Length >  0 )
								{
									foreach( var material in renderer.sharedMaterials )
									{
										if( material != null )
										{
											string shaderName = material.shader.ToString().Replace( " (UnityEngine.Shader)", "" ) ;
											material.shader = Shader.Find( shaderName ) ;
										}
									}
								}
							}
						}

					}
					else
					if( asset is Material )
					{
						Material material = asset as Material ;
						string shaderName = material.shader.ToString().Replace( " (UnityEngine.Shader)", "" ) ;
						material.shader = Shader.Find( shaderName ) ;
					}
#endif
					return asset ;
				}
#endif
				//---------------------------------------------------------

				// AssetBundleInfo :: GetAllAssetPaths

				/// <summary>
				/// アセットバンドルに含まれる全アセットのパスを取得する
				/// </summary>
				/// <param name="assetBundle"></param>
				/// <param name="assetBundlePath"></param>
				/// <param name="assetName"></param>
				/// <param name="type"></param>
				/// <param name="instance"></param>
				/// <returns></returns>
				internal protected ( string, Type )[] GetAllAssetPaths( AssetBundle assetBundle, string assetBundlePath, string localAssetsRootPath, AssetBundleManager instance )
				{
					string localAssetBundlePath = localAssetsRootPath + assetBundlePath + "/" ;
					localAssetBundlePath = localAssetBundlePath.ToLower() ;

					string[] paths = assetBundle.GetAllAssetNames() ;
					if( paths == null || paths.Length == 0 )
					{
						return null ;
					}

					List<( string, Type )> allAssetPaths = new List<( string, Type )>() ;

					// アセットバンドル部のパスと拡張子を削除する
					int i, l = paths.Length ;
					string path, extension ;
					int i0, i1 ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						path = paths[ i ] ;
						path = path.Replace( localAssetBundlePath, "" ) ;

						i0 = path.LastIndexOf( '/' ) ;
						i1 = path.LastIndexOf( '.' ) ;
						if( i1 <= i0 )
						{
							// 拡張子なし
							allAssetPaths.Add( ( path, typeof( System.Object ) ) ) ;
						}
						else
						{
							// 拡張子あり
							extension = path.Substring( i1, path.Length - i1 ) ;
							path = path.Substring( 0, i1 ) ;

							allAssetPaths.Add( ( path, instance.GetTypeFromExtension( extension ) ) ) ;
						}
					}

					return allAssetPaths.ToArray() ;
				}

				/// <summary>
				/// アセットバンドルに含まれる全アセットのパスを取得する
				/// </summary>
				/// <param name="assetBundle"></param>
				/// <param name="assetBundlePath"></param>
				/// <param name="assetName"></param>
				/// <param name="type"></param>
				/// <param name="instance"></param>
				/// <returns></returns>
				internal protected string[] GetAllAssetPaths( AssetBundle assetBundle, string assetBundlePath, Type type, string localAssetsRootPath, AssetBundleManager instance )
				{
					string localAssetBundlePath = localAssetsRootPath + assetBundlePath + "/" ;
					localAssetBundlePath = localAssetBundlePath.ToLower() ;

					string[] paths = assetBundle.GetAllAssetNames() ;
					if( paths == null || paths.Length == 0 )
					{
						return null ;
					}

					List<string> allAssetPaths = new List<string>() ;

					// アセットバンドル部のパスと拡張子を削除する
					int i, l = paths.Length ;
					string path, extension ;
					int i0, i1 ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						path = paths[ i ] ;
						path = path.Replace( localAssetBundlePath, "" ) ;

						i0 = path.LastIndexOf( '/' ) ;
						i1 = path.LastIndexOf( '.' ) ;
						if( i1 <= i0 )
						{
							// 拡張子なし
//							allAssetPaths.Add( path ) ;
						}
						else
						{
							// 拡張子あり
							extension = path.Substring( i1, path.Length - i1 ) ;
							if( instance.GetTypeFromExtension( extension ) == type )
							{
								path = path.Substring( 0, i1 ) ;
								allAssetPaths.Add( path ) ;
							}
						}
					}

					return allAssetPaths.ToArray() ;
				}

				//---------------------------------------------------------

				// AssetBundleInfo :: Asset
				
				/// <summary>
				/// アセットを取得する(同期版)　※呼び出し非推奨
				/// </summary>
				/// <typeparam name="T">任意のコンポーネント型</typeparam>
				/// <param name="assetBundle">アセットバンドルのインスタンス</param>
				/// <param name="assetBundleName">アセットバンドル名</param>
				/// <param name="assetName">アセット名</param>
				/// <returns>アセットに含まれる任意のコンポーネントのインスタンス</returns>
				internal protected UnityEngine.Object LoadAsset( AssetBundle assetBundle, string assetBundlePath, string assetName, Type type, string localAssetsRootPath, AssetBundleManager instance )
				{
					string path ;

					// ひとまず assetName は空想定でやってみる
					if( string.IsNullOrEmpty( assetName ) == true )
					{
						// アセットバンドル＝単一アセットのケース
						path = ( localAssetsRootPath + assetBundlePath ).ToLower() ;
					}
					else
					{
						// アセットバンドル＝複合アセットのケース
						path = ( localAssetsRootPath + assetBundlePath + "/" + assetName ).ToLower() ;
					}

					// まずはそのままロードしてみる
					UnityEngine.Object asset = assetBundle.LoadAsset( path, type ) ;
					if( asset == null )
					{
						int i0 = path.LastIndexOf( '/' ) ;
						int i1 = path.LastIndexOf( '.' ) ;
						if( i1 <= i0 )
						{
							// 拡張子なし
							// だめなら拡張子を加えてロードしてみる
							List<string> extensions ;
							if( instance.m_TypeToExtension.ContainsKey( type ) == true )
							{
								// 一般タイプ
								extensions = instance.m_TypeToExtension[ type ] ;
							}
							else
							{
								// 不明タイプ
								extensions = instance.m_UnknownTypeToExtension ;
							}

							foreach( string extension in extensions )
							{
								asset = assetBundle.LoadAsset( path + extension, type ) ;
								if( asset != null )
								{
									// 成功したら終了
									break ;
								}
							}
						}
					}
					
//					if( asset != null )
//					{
//						asset = ReplaceShader( asset, type ) ;
//					}

					return asset ;
				}
#if false
				/// <summary>
				/// アセットを取得する(非同期版)　※呼び出し非推奨
				/// </summary>
				/// <typeparam name="T">任意のコンポーネント型</typeparam>
				/// <param name="tAssetBundle">アセットバンドルのインスタンス</param>
				/// <param name="tAssetBundleName">アセットバンドル名</param>
				/// <param name="tName">アセット名</param>
				/// <param name="rAsset">アセットに含まれる任意のコンポーネントのインスタンスを格納するための要素数１以上の配列</param>
				/// <param name="tInstance">アセットバンドルマネージャのインスタンス</param>
				/// <returns>列挙子</returns>
				internal protected IEnumerator LoadAsset_Coroutine( AssetBundle tAssetBundle, string tAssetBundleName, string tAssetName, Type tType, UnityEngine.Object[] rAsset, Request tRequest, AssetBundleManager tInstance )
				{
					if( rAsset == null || rAsset.Length == 0 )
					{
						yield break ;
					}

					CreateIndex( tAssetBundle, tAssetBundleName ) ;

					if( m_Index == null )
					{
						yield break ;
					}

					if( string.IsNullOrEmpty( tAssetName ) == true )
					{
						// mainAsset は Unity5 より非推奨になりました(Unity5 以降の BuildPipeline.BuildAssetBundles から設定項目が消失している)
//						if( tAB.mainAsset != null )
//						{
//							if( rAsset != null && rAsset.Length >  0 )
//							{
//								rAsset[ 0 ] =  tAB.mainAsset as T ;
//							}
//							yield break ;
//						}

						int p = tAssetBundleName.LastIndexOf( '/' ) ;
						if( p >= 0 )
						{
							tAssetName = tAssetBundleName.Substring( p + 1, tAssetBundleName.Length - ( p + 1 ) ) ;
						}
						else
						{
							tAssetName = tAssetBundleName ;
						}
					}

					tAssetName = tAssetName.ToLower() ;

					if( m_Index.ContainsKey( tAssetName ) == false )
					{
						yield break ;
					}

					AssetBundleRequest tR = null ;
					UnityEngine.Object tAsset = null ;

					int n, c = m_Index[ tAssetName ].Count ;

					yield return new WaitForSeconds( 1 ) ;

					for( n  = 0 ; n <  c ; n ++ )
					{
						// tR に対してアクセスすると Assertion failed: Assertion failed on expression: 'Thread::CurrentThreadIsMainThread()' が発生するので現状使用できない
						tR = tAssetBundle.LoadAssetAsync( m_Index[ tAssetName ][ n ], tType ) ;

						if( tRequest == null )
						{
							yield return tR ;
						}
						else
						{
							while( true )
							{
								tRequest.progress =	tR.progress ;
								if( tR.isDone == true )
								{
									break ;
								}
								yield return null ;
							}
						}

						if( tR.isDone == true && tR.asset != null )
						{
							tAsset = tR.asset ;
							tAsset = ReplaceShader( tAsset, tType ) ;
							rAsset[ 0 ] = tAsset ;
							break ;
						}
					}
				}
#endif
				//-----------------------
				// AssetBundleInfo :: AllAssets

				/// <summary>
				/// 全てのアセットを取得する
				/// </summary>
				/// <param name="assetBundle"></param>
				/// <param name="type"></param>
				/// <param name="instance"></param>
				/// <param name="resourcePath"></param>
				/// <returns></returns>
				internal protected UnityEngine.Object[] LoadAllAssets( AssetBundle assetBundle, Type type, string localAssetsRootPath, string localAssetPath, AssetBundleManager instance )
				{
					UnityEngine.Object[] assets = assetBundle.LoadAllAssets() ;
					if( type != null && assets != null && assets.Length >  0 )
					{
						// タイプ指定があるならタイプで絞る
						List<UnityEngine.Object> temporaryAssets = new List<UnityEngine.Object>() ;
						foreach( var asset in assets )
						{
							if( asset.GetType() == type )
							{
								temporaryAssets.Add( asset ) ;
							}
						}

						if( temporaryAssets.Count == 0 )
						{
							return null ;
						}

						assets = temporaryAssets.ToArray() ;
					}

					if( assets != null && assets.Length >  0 )
					{
						string resourceCachePath ;

						// 最終的なものを生成する
						for( int i  = 0 ; i <  assets.Length ; i ++ )
						{
							resourceCachePath = localAssetsRootPath + localAssetPath + "/" + assets[ i ].name + ":" + assets[ i ].GetType().ToString() ;
							if( instance.ResourceCache != null && instance.ResourceCache.ContainsKey( resourceCachePath ) == true )
							{
								// キャッシュにあればそれを返す
								assets[ i ] = instance.ResourceCache[ resourceCachePath ].Get() ;
							}
//							else
//							{
//								assets[ i ] = ReplaceShader( assets[ i ], assets[ i ].GetType() ) ;
//							}
						}
					}
					else
					{
						assets = null ;
					}

					return assets ;
				}
				
				//-----------------------

				// AssetBundleInfo :: SubAsset

				/// <summary>
				/// アセットに含まれるサブアセットを取得する(同期版)　※呼び出し非推奨
				/// </summary>
				/// <typeparam name="T">任意のコンポーネント型</typeparam>
				/// <param name="tAssetBundle">アセットバンドルのインスタンス</param>
				/// <param name="tAssetBundleName">アセットバンドル名</param>
				/// <param name="tAssetName">アセット名</param>
				/// <param name="tSubName">サブアセット名</param>
				/// <param name="tInstance">アセットバンドルマネージャのインスタンス</param>
				/// <param name="tResourcePath">アセットのリソースパス</param>
				/// <returns>サブアセットに含まれる任意のコンポーネントのインスタンス</returns>
				internal protected UnityEngine.Object LoadSubAsset( AssetBundle assetBundle, string assetBundlePath, string assetName, string subAssetName, Type type, string localAssetsRootPath, string localAssetPath, AssetBundleManager instance )
				{
					UnityEngine.Object[] subAssets = LoadAllSubAssets( assetBundle, assetBundlePath, assetName, type, localAssetsRootPath, localAssetPath, instance ) ;
					if( subAssets == null || subAssets.Length == 0 )
					{
						return null ;
					}

					UnityEngine.Object asset = null ;

					foreach( var subAsset in subAssets )
					{
						if( subAsset.name == subAssetName && subAsset.GetType() == type )
						{
							asset = subAsset ;
							break ;
						}
					}

//					if( asset != null )
//					{
//						asset = ReplaceShader( asset, type ) ;
//					}

					return asset ;
				}
#if false
				/// <summary>
				/// アセットに含まれるサブアセットを取得する(非同期版)　※呼び出し非推奨
				/// </summary>
				/// <typeparam name="T">任意のコンポーネント型</typeparam>
				/// <param name="tAssetBundle">アセットバンドルのインスタンス</param>
				/// <param name="tAssetBundleName">アセットバンドル名</param>
				/// <param name="tAssetName">アセット名</param>
				/// <param name="tSubAssetName">サブアセット名</param>
				/// <param name="rSubAsset">サブアセットに含まれる任意のコンポーネントのインスタンスを格納するための要素数１以上の配列</param>
				/// <param name="tInstance">アセットバンドルマネージャのインスタンス</param>
				/// <param name="tResourcePath">アセットのリソースパス</param>
				/// <returns>列挙子</returns>
				internal protected IEnumerator LoadSubAsset_Coroutine( AssetBundle tAssetBundle, string tAssetBundleName, string tAssetName, string tSubAssetName, Type tType, UnityEngine.Object[] rSubAsset, Request tRequest, AssetBundleManager tInstance, string tResourcePath )
				{
					if( tInstance == null || rSubAsset == null || rSubAsset.Length == 0 )
					{
						yield break ;
					}

					CreateIndex( tAssetBundle, tAssetBundleName ) ;

					if( string.IsNullOrEmpty( tSubAssetName ) == true )
					{
						yield break ;
					}

					List<UnityEngine.Object> tAllSubAssets = null ;

					List<UnityEngine.Object>[] rAllSubAssets = { null } ;
					yield return tInstance.StartCoroutine( LoadAllSubAssets_Coroutine( tAssetBundle, tAssetBundleName, tAssetName, tType, rAllSubAssets, tRequest, tInstance, tResourcePath ) ) ;
					tAllSubAssets = rAllSubAssets[ 0 ] ;

					if( tAllSubAssets == null || tAllSubAssets.Count == 0 )
					{
						yield break ;
					}

					UnityEngine.Object tAsset = null ;

					int i, l = tAllSubAssets.Count ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						if( tAllSubAssets[ i ].name == tSubAssetName && tAllSubAssets[ i ].GetType() == tType )
						{
							tAsset = tAllSubAssets[ i ] ;
							break ;
						}
					}

					if( tAsset != null )
					{
						tAsset = ReplaceShader( tAsset, tType ) ;
					}

					rSubAsset[ 0 ] = tAsset ;
				}
#endif
				//-----------------------

				// AssetBundleInfo :: AllSubAssets

				/// <summary>
				/// アセットに含まれる全てのサブアセットを取得する(同期版)　※呼び出し非推奨
				/// </summary>
				/// <typeparam name="T">任意のコンポーネント型</typeparam>
				/// <param name="tAssetBundle">アセットバンドルのインスタンス</param>
				/// <param name="tAssetBundleName">アセットバンドル名</param>
				/// <param name="tAssetName">アセット名</param>
				/// <param name="tInstance">アセットバンドルマネージャのインスタンス</param>
				/// <param name="tResourcePath">アセットのリソースパス</param>
				/// <returns>全てのサブアセットに含まれる任意のコンポーネントのインスタンス</returns>
				internal protected UnityEngine.Object[] LoadAllSubAssets( AssetBundle assetBundle, string assetBundlePath, string assetName, Type type, string localAssetsRootPath, string localAssetPath, AssetBundleManager instance )
				{
					string path ;

					if( string.IsNullOrEmpty( assetName ) == true )
					{
						// アセットバンドル＝単一アセットのケース
						path = ( localAssetsRootPath + assetBundlePath ).ToLower() ;
					}
					else
					{
						// アセットバンドル＝複合アセットのケース
						path = ( localAssetsRootPath + assetBundlePath + "/" + assetName ).ToLower() ;
					}

					UnityEngine.Object[] assets = assetBundle.LoadAssetWithSubAssets( path, type ) ;	// 注意：該当が無くても配列数０のインスタンスが返る
					if( assets == null || assets.Length == 0 )
					{
						int i0 = path.LastIndexOf( '/' ) ;
						int i1 = path.LastIndexOf( '.' ) ;
						if( i1 <= i0 )
						{
							// 拡張子なし
							// だめなら拡張子を加えてロードしてみる
							List<string> extensions ;
							if( instance.m_TypeToExtension.ContainsKey( type ) == true )
							{
								// 一般タイプ
								extensions = instance.m_TypeToExtension[ type ] ;
							}
							else
							{
								// 不明タイプ
								extensions = instance.m_UnknownTypeToExtension ;
							}

							foreach( string extension in extensions )
							{
								assets = assetBundle.LoadAssetWithSubAssets( path + extension, type ) ;
								if( assets != null && assets.Length >  0 )
								{
									// 成功したら終了
									break ;
								}
							}
						}
					}

					if( assets != null && assets.Length >  0 )
					{
						string resourceCachePath ;

						// 最終的なものを生成する
						for( int i  = 0 ; i <  assets.Length ; i ++ )
						{
							resourceCachePath = localAssetsRootPath + localAssetPath + "/" + assets[ i ].name + ":" + type.ToString() ;
							if( instance.ResourceCache != null && instance.ResourceCache.ContainsKey( resourceCachePath ) == true )
							{
								// キャッシュにあればそれを返す
								assets[ i ] = instance.ResourceCache[ resourceCachePath ].Get() ;
							}
//							else
//							{
//								assets[ i ] = ReplaceShader( assets[ i ], type ) ;
//							}
						}
					}
					else
					{
						assets = null ;
					}

					return assets ;
				}
#if false
				/// <summary>
				/// アセットに含まれる全てのサブアセットを取得する(非同期版)　※呼び出し非推奨
				/// </summary>
				/// <typeparam name="T">任意のコンポーネント型</typeparam>
				/// <param name="tAssetBundle">アセットバンドルのインスタンス</param>
				/// <param name="tAssetBundleName">アセットバンドル名</param>
				/// <param name="tAssetName">アセット名</param>
				/// <param name="rAllSubAssets">全てのサブアセットに含まれる任意のコンポーネントのインスタンスを格納するための要素数１以上の配列</param>
				/// <param name="tInstance">アセットバンドルマネージャのインスタンス</param>
				/// <param name="tResourcePath">アセットのリソースパス</param>
				/// <returns>列挙子</returns>
				internal protected IEnumerator LoadAllSubAssets_Coroutine( AssetBundle tAssetBundle, string tAssetBundleName, string tAssetName, Type tType, List<UnityEngine.Object>[] rAllSubAssets, Request tRequest, AssetBundleManager tInstance, string tResourcePath )
				{
					if( tInstance == null || rAllSubAssets == null || rAllSubAssets.Length == 0 )
					{
						yield break ;
					}

					CreateIndex( tAssetBundle, tAssetBundleName ) ;

					if( m_Index == null )
					{
						yield break ;
					}

					if( string.IsNullOrEmpty( tAssetName ) == true )
					{
						// 名前が指定されていない場合はメインアセットとみなす
						int p = tAssetBundleName.LastIndexOf( '/' ) ;
						if( p >= 0 )
						{
							tAssetName = tAssetBundleName.Substring( p + 1, tAssetBundleName.Length - ( p + 1 ) ) ;
						}
						else
						{
							tAssetName = tAssetBundleName ;
						}
					}

					tAssetName = tAssetName.ToLower() ;

					List<UnityEngine.Object> tAllSubAssets = null ;
					string tResourceCachePath ;
					UnityEngine.Object tAsset ;

					if( m_Index.ContainsKey( tAssetName ) == true )
					{
						// 単体ファイルとして合致するものが有る
						AssetBundleRequest tR = null ;
						UnityEngine.Object[] t = null ;

						int i, l ;
						int n, c = m_Index[ tAssetName ].Count ;

						for( n  = 0 ; n <  c ; n ++ )
						{
							tR = tAssetBundle.LoadAssetWithSubAssetsAsync( m_Index[ tAssetName ][ n ], tType ) ;
							yield return tR ;

							if( tRequest != null )
							{
								tRequest.progress = ( float )n / ( float )c ;
							}

							if( tR.isDone == true )
							{
								t = tR.allAssets as UnityEngine.Object[] ;
								if( t != null && t.Length >  0 )
								{
									tAllSubAssets = new List<UnityEngine.Object>() ;
	
									l = t.Length ;
									for( i  = 0 ; i <  l ; i ++ )
									{
										tResourceCachePath = tResourcePath + "/" + t[ i ].name + ":" + tType.ToString() ;
										if( tInstance.resourceCache != null && tInstance.resourceCache.ContainsKey( tResourceCachePath ) == true )
										{
											// キャッシュにあればそれを返す
											tAllSubAssets.Add( tInstance.resourceCache[ tResourceCachePath ] ) ;
										}
										else
										{
											tAsset = t[ i ] ;
											tAsset = ReplaceShader( tAsset, tType ) ;
											tAllSubAssets.Add( tAsset ) ;
										}
									}
									
									if( tAllSubAssets.Count >  0 )
									{
										rAllSubAssets[ 0 ] = tAllSubAssets ;
									}

									yield break ;
								}
							}
						}
					}
					else
					{
						// 単体ファイルとして合致するものが無い

						// フォルダ指定の可能性があるので上位フォルダが合致するものをまとめて取得する
						int i, l ;
						int n, c ;

						l = m_Index.Count ;
						string[] tKey = new string[ l ] ;
						m_Index.Keys.CopyTo( tKey, 0 ) ;

						List<string> tTarget = new List<string>() ;

						for( i  = 0 ; i <  l ; i ++ )
						{
							if( tKey[ i ].IndexOf( tAssetName ) == 0 )
							{
								// 発見した
								tTarget.Add( tKey[ i ] ) ;
							}
						}

						if( tTarget.Count == 0 )
						{
							// 合致するものが存在しない
							yield break ;
						}

						tAllSubAssets = new List<UnityEngine.Object>() ;

						string tName ;
						AssetBundleRequest tR ;

						l = tTarget.Count ;
						for( i  = 0 ; i <  l ; i ++ )
						{
							tName = tTarget[ i ].Replace( tAssetName, "" ) ;

							tResourceCachePath = tResourcePath + "/" + tAssetName + "/" + tName + ":" + tType.ToString() ;
							if( tInstance.resourceCache != null && tInstance.resourceCache.ContainsKey( tResourceCachePath ) == true )
							{
								// キャッシュにあればそれを返す
								tAllSubAssets.Add( tInstance.resourceCache[ tResourceCachePath ] ) ;
							}
							else
							{
								c = m_Index[ tTarget[ i ] ].Count ;
								for( n  = 0 ; n <  c ; n ++ )
								{
									tR = tAssetBundle.LoadAssetAsync( m_Index[ tTarget[ i ] ][ n ], tType ) ;
									yield return tR ;

									if( tRequest != null )
									{
										tRequest.progress = ( float )n / ( float )c ;
									}

									if( tR.isDone == true )
									{
										tAsset = tR.asset ;
										tAsset = ReplaceShader( tAsset, tType ) ;
										tAllSubAssets.Add( tAsset ) ;
										break ;
									}
								}
							}
						}

						if( tAllSubAssets.Count >  0 )
						{
							rAllSubAssets[ 0 ] = tAllSubAssets ;
						}

						yield break ;
					}
				}
#endif
			}
		}
	}
}
