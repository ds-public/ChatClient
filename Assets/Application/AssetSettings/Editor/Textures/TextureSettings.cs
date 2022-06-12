using System ;
using System.Linq ;
using System.IO ;
using System.Collections ;
using System.Collections.Generic ;
using System.Text.RegularExpressions ;
using System.Reflection ;

using UnityEngine ;
using UnityEditor ;

using UnityEngine.U2D ;

namespace Template.AssetSettings
{
	/// <summary>
	/// SprineSprites の設定 Version 2021/11/06
	/// </summary>
	public class TextureSettings : ImportProcessor
	{
		private static readonly string[] m_Paths =
		{
//			@"Assets/Application/AssetBundle/Textures/AAA/*",	// テスト用
			@"Assets/Application/AssetBundle/Textures/*",
			@"Assets/Application/ReferencedAssets/Textures/*",
		} ;

		// Textureインポート用のディスパッチャー
		private static readonly ImportDispatcher<AssetImporter> m_TextureDispatcher
			= new ImportDispatcher<AssetImporter>
			(
				m_Paths,
				ReplaceTextureSettings,
				null
			) ;

		// SpriteAtlasインポート用のディスパッチャー
		private static readonly ImportDispatcher<AssetImporter> m_SpriteAtlasDispatcher
			= new ImportDispatcher<AssetImporter>
			(
				m_Paths,
				ReplaceSpriteAtlasSettings,
				null
			) ;

		//---------------

		// バッチ処理用のディスパッチャー
		private static readonly ImportDispatcher<AssetImporter> m_BatchDispatcher
			= new ImportDispatcher<AssetImporter>
			(
				m_Paths,
				null,
				ReplaceBatchSettings
			) ;

		// ※テクスチャとスプライトアトラスは、処理は別に分ける。(スプライトアトラスは、明示的なインポートが不要であるため。)

		//-------------------------------------------------------------------------------------------

		// メニューから全て設定し直す
		[ MenuItem( "AssetSettings/Textures - Reimport" ) ]
		internal static void SetAllAssets()
		{
			string targetPath = null ;
			if( Selection.objects.Length == 1 && Selection.activeObject != null )
			{
				// １つだけ選択（複数選択には対応していない：フォルダかファイル）
				targetPath = AssetDatabase.GetAssetPath( Selection.activeObject.GetInstanceID() ).Replace( '\\', '/' ) ;
			}

			m_BatchDispatcher.SetupAll( targetPath ) ;
		}

		//-------------------------------------------------------------------------------------------
		// コールバックメソッド群(オーバーライド)

		/// <summary>
		/// アセットがインポートされる前に呼び出されます(このメソッドを継承して処理を追加するのが最も負荷が高い)
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		public override void OnPreprocessTexture( AssetPostprocessor assetPostprocessor )
		{
			AssetImporter assetImporter = assetPostprocessor.assetImporter ;
			if( assetImporter.importSettingsMissing == false )
			{
				// 既にインポート済み(metaファイルが作られている)は無視する
//				Debug.Log( "既にインポートされている:" + assetImporter.assetPath ) ;
				return ;
			}

//			Debug.Log( "<color=#00FFFF>[インポートされていないので処理する]:" + assetImporter.assetPath + "</color>" ) ;

			m_TextureDispatcher.SetupAny( assetImporter ) ;
		}

		// 注意:
		// SpriteAtlas を新規作成しても OnPreprocessAsset は呼ばれない

		/// <summary>
		/// アセットが追加された後に呼び出されます
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		public override bool OnAssetImported( string assetPath )
		{
			return m_SpriteAtlasDispatcher.SetupAny( assetPath ) ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// SpineSprites の設定を行う(インポート処理用)
		/// </summary>
		private static bool ReplaceTextureSettings( AssetImporter assetImporter )
		{
			if( assetImporter is TextureImporter )
			{
				// テクスチャの設定を行う
				ProcessTextureSettings( assetImporter as TextureImporter ) ;
			}

			// Save は必要ない
			return false ;
		}

		/// <summary>
		/// SpineSprites の設定を行う(インポート用)
		/// </summary>
		/// <param name="assetImporter"></param>
		/// <returns></returns>
		private static bool ReplaceSpriteAtlasSettings( AssetImporter assetImporter )
		{
			string assetPath = assetImporter.assetPath.Replace( '\\', '/' ) ;

			string extension = Path.GetExtension( assetPath ) ;
			if( extension != ".spriteatlas" )
			{
				return false ;	// スプライトアトラス以外は無視する
			}

			SpriteAtlas spriteAltas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>( assetPath ) ;
			if( spriteAltas == null )
			{
//				Debug.Log( "アセットのロードができない" ) ;
				return false ;	// スプライトアトラスではない
			}

			if( ProcessSpriteAtlasSettings( spriteAltas ) == true )
			{
//				Debug.Log( "------>スプライトアトラスを処理した" ) ;
				return true ;
			}

			return false ;
		}

		/// <summary>
		/// SpineSprites の設定を行う(バッチ専用)
		/// </summary>
		private static bool ReplaceBatchSettings( AssetImporter assetImporter )
		{
			if( assetImporter is TextureImporter )
			{
				// テクスチャの設定を行う
				if( ProcessTextureSettings( assetImporter as TextureImporter ) == true )
				{
					Debug.Log( "<color=#FFFF00>[Textures Importing] " + assetImporter.assetPath + "</color>" ) ;
					AssetDatabase.ImportAsset( assetImporter.assetPath ) ;

					return true ;
				}

				return false ;	// Save は不要
			}
			else
			{
				// その他(スプライトアトラス)の設定を行う
				return ReplaceSpriteAtlasSettings( assetImporter ) ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// テクスチャサイズ最大
		/// </summary>
		private const int m_MaxTextureSize = 2048 ;

		/// <summary>
		/// Textures の設定を行う
		/// </summary>
		/// <summary>
		/// SpineSprites の設定を行う
		/// </summary>
		private static bool ProcessTextureSettings( TextureImporter textureImporter )
		{
			// パス
			bool isDirty = false ;

			if ( PreProcessSSRCardTextureSettings(textureImporter, out var isInnerDirty) )
			{
				return isInnerDirty ;
			}

			//----------------------------------
			// 全体設定

			// タイプ
			if( textureImporter.textureType != TextureImporterType.Sprite )
			{
				textureImporter.textureType  = TextureImporterType.Sprite ;
				isDirty = true ;
			}

			// 読み書き許可
			if( textureImporter.isReadable != false )
			{
				textureImporter.isReadable  = false ;
				isDirty = true ;
			}

			// ミップマップ
			if( textureImporter.mipmapEnabled != false )
			{
				textureImporter.mipmapEnabled  = false ;
				isDirty = true ;
			}

			// フィルタ
			if( textureImporter.filterMode != FilterMode.Bilinear )
			{
				textureImporter.filterMode  = FilterMode.Bilinear ;
				isDirty = true ;
			}

			//---------------------------------
			// 各プラットフォームごとの共通設定

			// 最大テクスチャサイズ
			int maxTextureSize = textureImporter.maxTextureSize ;
			if( maxTextureSize >  m_MaxTextureSize )
			{
				maxTextureSize  = m_MaxTextureSize ;
			}
			if( textureImporter.maxTextureSize != maxTextureSize )
			{
				textureImporter.maxTextureSize  = maxTextureSize ;
				isDirty = true ;
			}

			// 圧縮
			if( textureImporter.textureCompression != TextureImporterCompression.Compressed )
			{
				textureImporter.textureCompression  = TextureImporterCompression.Compressed ;
				isDirty = true ;
			}

			// さらに圧縮
			if( textureImporter.crunchedCompression != true )
			{
				textureImporter.crunchedCompression  = true ;
				isDirty = true ;
			}

			//----------------------------------
			// 各プラットフォームごとの個別設定

			// 注意：
			// overridden は、全てのプラットフォームで true にすべし。
			// false になっているものが 1 つでもあると、
			// Inspector のプラッフォームタブでプラットフォームを切り替えた際に、
			// Dirty 状態([Apply]ボタンと[Revert]ボタンが有効化)されてしまう。
			// また、
			// 対象フォルダ内の全対象ファイルをインポート(再設定)し直す際は、
			// Inspector に対象のファイルを表示した状態にしてはならない。
			// 表示されていたファイルが Dirty 状態になってしまう。

			TextureImporterPlatformSettings platformSettings ;
			string platfornName ;

			// Standalone
			platfornName = "Standalone" ;
			platformSettings = textureImporter.GetPlatformTextureSettings( platfornName ) ;
			if( SetPlatformSettings( platformSettings, platfornName, textureImporter.maxTextureSize, TextureImporterFormat.DXT5Crunched ) == true )
			{
				textureImporter.SetPlatformTextureSettings( platformSettings ) ;
				isDirty = true ;
			}

			// iOS
			platfornName = "iPhone" ;
			platformSettings = textureImporter.GetPlatformTextureSettings( platfornName ) ;
			if( SetPlatformSettings( platformSettings, platfornName, textureImporter.maxTextureSize, TextureImporterFormat.ASTC_6x6		) == true )
			{
				textureImporter.SetPlatformTextureSettings( platformSettings ) ;
				isDirty = true ;
			}

			// Android

			// テクスチャのサイズにより圧縮フォーマットを切り替える
			TextureImporterFormat androidTextureFormat = TextureImporterFormat.ASTC_6x6 ;

//			Texture2D texture = AssetDatabase.LoadAssetAtPath( textureImporter.assetPath, typeof( Texture2D ) ) as Texture2D ;

			// 外法(リフレクションを使いインポーターからテクスチャのサイズを取得する)
			object[] size = new object[ 2 ]{ 0, 0 } ;
			var method = typeof( TextureImporter ).GetMethod( "GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance ) ;
			method.Invoke( textureImporter, size ) ;
			if( ( ( int )size[ 0 ] & 3 ) == 0 && ( ( int )size[ 1 ] & 3 ) == 0 )
			{
				// サイズは４の倍数なのでクランチＥＴＣ２が使用できる
				androidTextureFormat = TextureImporterFormat.ETC2_RGBA8Crunched ;
			}

			// Android
			platfornName = "Android" ;
			platformSettings = textureImporter.GetPlatformTextureSettings( platfornName ) ;
			if( SetPlatformSettings( platformSettings, platfornName, textureImporter.maxTextureSize, androidTextureFormat ) == true )
			{
				textureImporter.SetPlatformTextureSettings( platformSettings ) ;
				isDirty = true ;
			}

			return isDirty ;
		}

		// プラットフォームごとの確認と設定
		private static bool SetPlatformSettings( TextureImporterPlatformSettings platformSettings, string platfornName, int maxTextureSize, TextureImporterFormat textureFormat )
		{
			bool isUpdate = false ;

			if( platformSettings != null )
			{
				maxTextureSize = platformSettings.maxTextureSize ;
				if( maxTextureSize >  m_MaxTextureSize )
				{
					maxTextureSize  = m_MaxTextureSize ;
				}

				if
				(
					platformSettings.overridden			!= true										||
					platformSettings.maxTextureSize		!= maxTextureSize							||
					platformSettings.format				!= textureFormat							||
					platformSettings.textureCompression	!= TextureImporterCompression.Compressed	||
					platformSettings.resizeAlgorithm	!= TextureResizeAlgorithm.Mitchell
				)
				{
					isUpdate = true ;
				}
			}
			else
			{
				isUpdate = true ;
			}

			if( isUpdate == true )
			{
				platformSettings.name				= platfornName									;
				platformSettings.overridden			= true											;
				platformSettings.maxTextureSize		= maxTextureSize								;
				platformSettings.format				= textureFormat									;
				platformSettings.textureCompression	= TextureImporterCompression.Compressed			;
				platformSettings.resizeAlgorithm	= TextureResizeAlgorithm.Mitchell				;
			}

			return isUpdate ;
		}


		// https://tetsujp84.hatenablog.com/entry/2019/05/05/050828

		/// <summary>
		/// SpineSprites の設定を行う
		/// </summary>
		private static bool ProcessSpriteAtlasSettings( SpriteAtlas spriteAtlas )
		{
			var serializedObject = new SerializedObject( spriteAtlas ) ;
			var editorData = serializedObject.FindProperty( "m_EditorData" ) ;

			bool isDirty = false ;

			// packing設定も適用
			var packingSetting = editorData.FindPropertyRelative( "packingSettings" ) ;

//			packingSetting.FindPropertyRelative( "enableRotation" ).boolValue		= false ;
			if( CheckPropertyOfBool( packingSetting, "enableRotation", false ) == false )
			{
				StorePropertyOfBool( packingSetting, "enableRotation", false ) ;
				isDirty = true ;
			}

//			packingSetting.FindPropertyRelative( "enableTightPacking" ).boolValue	= false ;
			if( CheckPropertyOfBool( packingSetting, "enableTightPacking", false ) == false )
			{
				StorePropertyOfBool( packingSetting, "enableTightPacking", false ) ;
				isDirty = true ;
			}

//			packingSetting.FindPropertyRelative( "padding" ).intValue				= 2 ;
			if( CheckPropertyOfInt( packingSetting, "padding", 2 ) == false )
			{
				StorePropertyOfInt( packingSetting, "padding", 2 ) ;
				isDirty = true ;
			}

			// 基本設定
			var textureSettings = editorData.FindPropertyRelative( "textureSettings" ) ;

//			textureSettings.FindPropertyRelative( "maxTextureSize" ).intValue				= 2048 ;
			if( CheckPropertyOfInt( textureSettings, "maxTextureSize", 2048 ) == false )
			{
				StorePropertyOfInt( textureSettings, "maxTextureSize", 2048 ) ;
				isDirty = true ;
			}

//			textureSettings.FindPropertyRelative( "textureCompression" ).boolValue			= true ;
			if( CheckPropertyOfBool( textureSettings, "textureCompression", true ) == false )
			{
				StorePropertyOfBool( textureSettings, "textureCompression", true ) ;
				isDirty = true ;
			}

//			textureSettings.FindPropertyRelative( "generateMipMaps" ).boolValue				= false ;
			if( CheckPropertyOfBool( textureSettings, "generateMipMaps", false ) == false )
			{
				StorePropertyOfBool( textureSettings, "generateMipMaps", false ) ;
				isDirty = true ;
			}

//			textureSettings.FindPropertyRelative( "crunchedCompression" ).boolValue			= true ;
			if( CheckPropertyOfBool( textureSettings, "crunchedCompression", true ) == false )
			{
				StorePropertyOfBool( textureSettings, "crunchedCompression", true ) ;
				isDirty = true ;
			}

			// platform設定
			var platformSettings = editorData.FindPropertyRelative( "platformSettings" ) ;

			SerializedProperty settings ;

			if( platformSettings.arraySize == 4 )
			{
				settings = platformSettings.GetArrayElementAtIndex( 0 ) ;
				if( CheckPlatformSettings( settings, "DefaultTexturePlatform",	false,	-1												) == false )
				{
					StorePlatformSettings( settings, "DefaultTexturePlatform",	false,	-1												) ;
					isDirty = true ;
				}

				settings = platformSettings.GetArrayElementAtIndex( 1 ) ;
				if( CheckPlatformSettings( settings, "Standalone",				true,	( int )TextureImporterFormat.DXT5Crunched		) == false )
				{
					StorePlatformSettings( settings, "Standalone",				true,	( int )TextureImporterFormat.DXT5Crunched		) ;
					isDirty = true ;
				}

				settings = platformSettings.GetArrayElementAtIndex( 2 ) ;
				if( CheckPlatformSettings( settings, "iPhone",					true,	( int )TextureImporterFormat.ASTC_6x6			) == false )
				{
					StorePlatformSettings( settings, "iPhone",					true,	( int )TextureImporterFormat.ASTC_6x6			) ;
					isDirty = true ;
				}

				settings = platformSettings.GetArrayElementAtIndex( 3 ) ;
				if( CheckPlatformSettings( settings, "Android",					true,	( int )TextureImporterFormat.ETC2_RGBA8Crunched	) == false )
				{
					StorePlatformSettings( settings, "Android",					true,	( int )TextureImporterFormat.ETC2_RGBA8Crunched	) ;
					isDirty = true ;
				}
			}
			else
			{
				platformSettings.arraySize = 4 ;

				settings = platformSettings.GetArrayElementAtIndex( 0 ) ;
				StorePlatformSettings( settings, "DefaultTexturePlatform",	false,	-1												) ;

				settings = platformSettings.GetArrayElementAtIndex( 1 ) ;
				StorePlatformSettings( settings, "Standalone",				true,	( int )TextureImporterFormat.DXT5Crunched		) ;

				settings = platformSettings.GetArrayElementAtIndex( 2 ) ;
				StorePlatformSettings( settings, "iPhone",					true,	( int )TextureImporterFormat.ASTC_6x6			) ;

				settings = platformSettings.GetArrayElementAtIndex( 3 ) ;
				StorePlatformSettings( settings, "Android",					true,	( int )TextureImporterFormat.ETC2_RGBA8Crunched	) ;

				isDirty = true ;
			}

			if( isDirty == true )
			{
				// 上記の変更を適用
				serializedObject.ApplyModifiedProperties() ;
			}

			//------------------------------------------------------------------------------------------

			// プロパティの値が合っているかチェックする
			bool CheckPropertyOfBool( SerializedProperty property, string key, bool value )
			{
				return ( property.FindPropertyRelative( key ).boolValue	== value ) ;
			}

			// プロパティの値を設定する
			void StorePropertyOfBool( SerializedProperty property, string key, bool value )
			{
				property.FindPropertyRelative( key ).boolValue	= value ;
			}

			// プロパティの値が合っているかチェックする
			bool CheckPropertyOfInt( SerializedProperty property, string key, int value )
			{
				return ( property.FindPropertyRelative( key ).intValue	== value ) ;
			}

			// プロパティの値を設定する
			void StorePropertyOfInt( SerializedProperty property, string key, int value )
			{
				property.FindPropertyRelative( key ).intValue	= value ;
			}

			// プラットフォームの設定を確認する
			bool CheckPlatformSettings( SerializedProperty property, string platform, bool overridden, int textureFormat )
			{
				if( property.FindPropertyRelative( "m_BuildTarget" ).stringValue != platform )
				{
					return false ;
				}

				if( property.FindPropertyRelative( "m_Overridden" ).boolValue != overridden )
				{
					return false ;
				}

				if( property.FindPropertyRelative( "m_MaxTextureSize" ).intValue != 2048 )
				{
					return false ;
				}

				if( property.FindPropertyRelative( "m_TextureFormat" ).intValue	!= textureFormat )
				{
					return false ;
				}

				if( property.FindPropertyRelative( "m_TextureCompression" ).boolValue != true )
				{
					return false ;
				}

				if( property.FindPropertyRelative( "m_CompressionQuality" ).intValue != 50 )
				{
					return false ;
				}

				if( property.FindPropertyRelative( "m_CrunchedCompression" ).boolValue != true )
				{
					return false ;
				}

				return true ;
			}

			// プラットフォームの設定を更新する
			void StorePlatformSettings( SerializedProperty property, string platform, bool overridden, int textureFormat )
			{
				property.FindPropertyRelative( "m_BuildTarget" ).stringValue				= platform ;
				property.FindPropertyRelative( "m_Overridden" ).boolValue					= overridden ;
				property.FindPropertyRelative( "m_MaxTextureSize" ).intValue				= 2048 ;
				property.FindPropertyRelative( "m_TextureFormat" ).intValue					= textureFormat ;
				property.FindPropertyRelative( "m_TextureCompression" ).boolValue			= true ;
				property.FindPropertyRelative( "m_CompressionQuality" ).intValue			= 50 ;
				property.FindPropertyRelative( "m_CrunchedCompression" ).boolValue			= true ;
			}

			return isDirty ;
		}

		//-------------------------------------------------------------------------------------------

		private static bool PreProcessSSRCardTextureSettings( TextureImporter textureImporter, out bool isDirty )
		{
			isDirty = false ;

			var assetPath = textureImporter.assetPath ;
			if ( !assetPath.StartsWith("Assets/Application/AssetBundle/Textures/Cards") )
			{
				return false ;
			}

			if ( !assetPath.Contains("CARD_SSR_") )
			{
				return false ;
			}

			// エフェクト用テクスチャ設定
			if ( assetPath.Contains("/EffectTextures/") )
			{
				isDirty = SetSSREffectTextureSettings( textureImporter ) ;
				return true ;
			}

			var assetName = Path.GetFileNameWithoutExtension( assetPath ) ;
			if ( assetName == "LargeMask" )
			{
				isDirty = SetSSRELargeMaskTextureSettings( textureImporter ) ;
				return true ;
			}

			return false ;
		}

		private static bool SetSSREffectTextureSettings( TextureImporter textureImporter )
		{
			var isDirty = false ;
			// テクスチャタイプ
			if ( textureImporter.textureType != TextureImporterType.Default )
			{
				textureImporter.textureType = TextureImporterType.Default ;
				isDirty                     = true ;
			}

			// 読み書き許可
			if ( textureImporter.isReadable != false )
			{
				textureImporter.isReadable = false ;
				isDirty                    = true ;
			}

			// ミップマップ
			if ( textureImporter.mipmapEnabled )
			{
				textureImporter.mipmapEnabled = false ;
				isDirty                       = true ;
			}

			// フィルタ
			if ( textureImporter.filterMode != FilterMode.Bilinear )
			{
				textureImporter.filterMode = FilterMode.Bilinear ;
				isDirty                    = true ;
			}

			// 最大テクスチャサイズ
			if ( textureImporter.maxTextureSize > 256 )
			{
				textureImporter.maxTextureSize = 256 ;
				isDirty                        = true ;
			}

			// 圧縮
			if ( textureImporter.textureCompression != TextureImporterCompression.Compressed )
			{
				textureImporter.textureCompression = TextureImporterCompression.Compressed ;
				isDirty                            = true ;
			}

			// さらに圧縮
			if ( textureImporter.crunchedCompression != true )
			{
				textureImporter.crunchedCompression = true ;
				isDirty                             = true ;
			}

			var textureSettings = new TextureImporterSettings() ;
			textureImporter.ReadTextureSettings( textureSettings ) ;
			if ( textureSettings.wrapMode == TextureWrapMode.Clamp )
			{
				textureSettings.wrapMode = TextureWrapMode.Repeat ;
				isDirty                  = true ;
			}

			if ( textureSettings.spriteGenerateFallbackPhysicsShape )
			{
				textureSettings.spriteGenerateFallbackPhysicsShape = false ;
				isDirty                                            = true ;
			}

			if ( textureSettings.npotScale != TextureImporterNPOTScale.ToLarger )
			{
				textureSettings.npotScale = TextureImporterNPOTScale.ToLarger ;
				isDirty                   = true ;
			}

			textureImporter.SetTextureSettings( textureSettings ) ;

			TextureImporterPlatformSettings platformSettings ;
			string                          platfornName ;

			// Standalone
			platfornName     = "Standalone" ;
			platformSettings = textureImporter.GetPlatformTextureSettings( platfornName ) ;
			if ( SetPlatformSettings
				(
					platformSettings,
					platfornName,
					textureImporter.maxTextureSize,
					TextureImporterFormat.DXT5Crunched
				)
				== true )
			{
				textureImporter.SetPlatformTextureSettings( platformSettings ) ;
				isDirty = true ;
			}

			// iOS
			platfornName     = "iPhone" ;
			platformSettings = textureImporter.GetPlatformTextureSettings( platfornName ) ;
			if ( SetPlatformSettings
				(
					platformSettings,
					platfornName,
					textureImporter.maxTextureSize,
					TextureImporterFormat.ASTC_6x6
				)
				== true )
			{
				textureImporter.SetPlatformTextureSettings( platformSettings ) ;
				isDirty = true ;
			}

			// Android
			var androidTextureFormat = TextureImporterFormat.ETC2_RGBA8Crunched ;
			platfornName     = "Android" ;
			platformSettings = textureImporter.GetPlatformTextureSettings( platfornName ) ;
			if ( SetPlatformSettings
				(
					platformSettings,
					platfornName,
					textureImporter.maxTextureSize,
					androidTextureFormat
				)
				== true )
			{
				textureImporter.SetPlatformTextureSettings( platformSettings ) ;
				isDirty = true ;
			}

			return isDirty ;
		}

		private static bool SetSSRELargeMaskTextureSettings( TextureImporter textureImporter )
		{
			var isDirty = false ;
			// テクスチャタイプ
			if ( textureImporter.textureType != TextureImporterType.Default )
			{
				textureImporter.textureType = TextureImporterType.Default ;
				isDirty                     = true ;
			}

			// 読み書き許可
			if ( textureImporter.isReadable != false )
			{
				textureImporter.isReadable = false ;
				isDirty                    = true ;
			}

			// ミップマップ
			if ( textureImporter.mipmapEnabled )
			{
				textureImporter.mipmapEnabled = false ;
				isDirty                       = true ;
			}

			// フィルタ
			if ( textureImporter.filterMode != FilterMode.Bilinear )
			{
				textureImporter.filterMode = FilterMode.Bilinear ;
				isDirty                    = true ;
			}

			// 最大テクスチャサイズ
			if ( textureImporter.maxTextureSize > 1024 )
			{
				textureImporter.maxTextureSize = 1024 ;
				isDirty                        = true ;
			}

			// 圧縮
			if ( textureImporter.textureCompression != TextureImporterCompression.Compressed )
			{
				textureImporter.textureCompression = TextureImporterCompression.Compressed ;
				isDirty                            = true ;
			}

			// さらに圧縮
			if ( textureImporter.crunchedCompression != true )
			{
				textureImporter.crunchedCompression = true ;
				isDirty                             = true ;
			}

			var textureSettings = new TextureImporterSettings() ;
			textureImporter.ReadTextureSettings( textureSettings ) ;
			if ( textureSettings.wrapMode != TextureWrapMode.Clamp )
			{
				textureSettings.wrapMode = TextureWrapMode.Clamp ;
				isDirty                  = true ;
			}

			if ( textureSettings.spriteGenerateFallbackPhysicsShape )
			{
				textureSettings.spriteGenerateFallbackPhysicsShape = false ;
				isDirty                                            = true ;
			}

			if ( textureSettings.npotScale != TextureImporterNPOTScale.ToLarger )
			{
				textureSettings.npotScale = TextureImporterNPOTScale.ToLarger ;
				isDirty                   = true ;
			}

			textureImporter.SetTextureSettings( textureSettings ) ;

			TextureImporterPlatformSettings platformSettings ;
			string                          platfornName ;

			// Standalone
			platfornName     = "Standalone" ;
			platformSettings = textureImporter.GetPlatformTextureSettings( platfornName ) ;
			if ( SetPlatformSettings
				(
					platformSettings,
					platfornName,
					textureImporter.maxTextureSize,
					TextureImporterFormat.DXT5Crunched
				)
				== true )
			{
				textureImporter.SetPlatformTextureSettings( platformSettings ) ;
				isDirty = true ;
			}

			// iOS
			platfornName     = "iPhone" ;
			platformSettings = textureImporter.GetPlatformTextureSettings( platfornName ) ;
			if ( SetPlatformSettings
				(
					platformSettings,
					platfornName,
					textureImporter.maxTextureSize,
					TextureImporterFormat.ASTC_6x6
				)
				== true )
			{
				textureImporter.SetPlatformTextureSettings( platformSettings ) ;
				isDirty = true ;
			}

			// Android
			var androidTextureFormat = TextureImporterFormat.ETC2_RGBA8Crunched ;
			platfornName     = "Android" ;
			platformSettings = textureImporter.GetPlatformTextureSettings( platfornName ) ;
			if ( SetPlatformSettings
				(
					platformSettings,
					platfornName,
					textureImporter.maxTextureSize,
					androidTextureFormat
				)
				== true )
			{
				textureImporter.SetPlatformTextureSettings( platformSettings ) ;
				isDirty = true ;
			}

			return isDirty ;
		}
	}
}
