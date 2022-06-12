#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// ImageNumber のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( ImageNumber ) ) ]
	public class ImageNumberInspector : MaskableGraphicWrapperInspector
	{
		/// <summary>
		/// スンスペクター描画
		/// </summary>
		public override void OnInspectorGUI()
		{
			// とりあえずデフォルト
//			DrawDefaultInspector() ;
		
			//--------------------------------------------
		
			// ターゲットのインスタンス
			ImageNumber view = target as ImageNumber ;
		
			// Graphic の基本情報を描画する
			DrawBasis( view ) ;

			EditorGUILayout.Separator() ;   // 少し区切りスペース

			// アトラス
			DrawAtlas( view ) ;

			// １文字あたりの大きさ
			Vector2 codeScale = EditorGUILayout.Vector2Field( "Code Scale", view.CodeScale ) ;
			if( codeScale != view.CodeScale && codeScale.x >  0 && codeScale.y >  0 )
			{
				Undo.RecordObject( view, "ImageNumber : Code Scale Change" ) ;	// アンドウバッファに登録
				view.CodeScale = codeScale ;
				EditorUtility.SetDirty( view ) ;
			}

			// 文字間隔
			float codeSpace = EditorGUILayout.FloatField( "Code Space", view.CodeSpace ) ;
			if( codeSpace != view.CodeSpace )
			{
				Undo.RecordObject( view, "ImageNumber : Code Space Change" ) ;	// アンドウバッファに登録
				view.CodeSpace = codeSpace ;
				EditorUtility.SetDirty( view ) ;
			}

			// データ配列
			SerializedObject so = new SerializedObject( view ) ;
			SerializedProperty sp = so.FindProperty( "CodeOffset" ) ;
			if( sp != null )
			{
				EditorGUILayout.PropertyField( sp, new GUIContent( "CodeOffset" ), true ) ;
			}
			so.ApplyModifiedProperties() ;

			// コードアンカー
			TextAnchor alignment = ( TextAnchor )EditorGUILayout.EnumPopup( "Alignment",  view.Alignment ) ;
			if( alignment != view.Alignment )
			{
				Undo.RecordObject( view, "ImageNumber : Alignement Change" ) ;	// アンドウバッファに登録
				view.Alignment = alignment ;
				EditorUtility.SetDirty( view ) ;
			}
			
			EditorGUILayout.Separator() ;   // 少し区切りスペース

			// 表示する値
			GUI.backgroundColor = new Color( 0.0f, 1.0f, 1.0f, 1.0f ) ;	// ＧＵＩの下地を灰にする
			double value = EditorGUILayout.DoubleField( "Value", view.Value, GUILayout.Width( 300f ) ) ;
			GUI.backgroundColor = Color.white ;
			if( value != view.Value )
			{
				// 変化があった場合のみ処理する
				Undo.RecordObject( view, "ImageNumber : Value Change" ) ;	// アンドウバッファに登録
				view.Value = value ;
				EditorUtility.SetDirty( view ) ;
			}

			EditorGUILayout.Separator() ;   // 少し区切りスペース

			//-----------------------------------------------------
			
			EditorGUIUtility.labelWidth =  60f ;
			EditorGUIUtility.fieldWidth =  30f ;
		
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// 縦方向揃え
				GUILayout.Label( "Digit", GUILayout.Width( 40.0f ) ) ;	// null でないなら 74
				int digitInteger = EditorGUILayout.IntField( view.DigitInteger, GUILayout.Width( 40f ) ) ;
				if( digitInteger != view.DigitInteger )
				{
					// 変化があった場合のみ処理する
					Undo.RecordObject( view, "ImageNumber : Digit Integer Change" ) ;	// アンドウバッファに登録
					view.DigitInteger = digitInteger ;
					EditorUtility.SetDirty( view ) ;
				}
			
				GUILayout.Label( ".", GUILayout.Width( 10.0f ) ) ;	// null でないなら 74
			
				int digitDecimal = EditorGUILayout.IntField( view.DigitDecimal, GUILayout.Width( 40f ) ) ;
				if( digitDecimal != view.DigitDecimal )
				{
					// 変化があった場合のみ処理する
					Undo.RecordObject( view, "ImageNumber : Digit Decimal Change" ) ;	// アンドウバッファに登録
					view.DigitDecimal = digitDecimal ;
					EditorUtility.SetDirty( view ) ;
				}

				// 適当なスペース
				GUILayout.Label( "", GUILayout.Width( 5f ) ) ;

				// カンマ
				GUILayout.Label( "Comma", GUILayout.Width( 50.0f ) ) ;	// null でないなら 74
				int comma = EditorGUILayout.IntField( view.Comma, GUILayout.Width( 40f ) ) ;
				if( comma != view.Comma )
				{
					// 変化があった場合のみ処理する
					Undo.RecordObject( view, "ImageNumber : Comma Change" ) ;	// アンドウバッファに登録
					view.Comma = comma ;
					EditorUtility.SetDirty( view ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		
			EditorGUIUtility.labelWidth = 116f ;
			EditorGUIUtility.fieldWidth =  40f ;
		
			// 符号を表示するか否か
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// 各種ＧＵＩのデフォルトリソースのロードを有効にするか
				bool plusSign = EditorGUILayout.Toggle( view.PlusSign, GUILayout.Width( 16f ) ) ;
				if( plusSign != view.PlusSign )
				{
					Undo.RecordObject( view, "ImageNumber : Plus Sign Change" ) ;	// アンドウバッファに登録
					view.PlusSign = plusSign ;
					EditorUtility.SetDirty( view ) ;
				}
				GUILayout.Label( "Plus Sign", GUILayout.Width( 80f ) ) ;
//			}
//			GUILayout.EndHorizontal() ;		// 横並び終了
		
			GUILayout.Label( "", GUILayout.Width( 10f ) ) ;

			// 符号を表示するか否か
//			GUILayout.BeginHorizontal() ;	// 横並び
//			{
				// 各種ＧＵＩのデフォルトリソースのロードを有効にするか
				bool zeroSign = EditorGUILayout.Toggle( view.ZeroSign, GUILayout.Width( 16f ) ) ;
				if( zeroSign != view.ZeroSign )
				{
					Undo.RecordObject( view, "ImageNumber : Zero Sign Change" ) ;	// アンドウバッファに登録
					view.ZeroSign = zeroSign ;
					EditorUtility.SetDirty( view ) ;
				}
				GUILayout.Label( "Zero Sign", GUILayout.Width( 80f ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		
//			GUILayout.Label( "", GUILayout.Width( 10f ) ) ;

			// ０埋め
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// 各種ＧＵＩのデフォルトリソースのロードを有効にするか
				bool zeroPadding = EditorGUILayout.Toggle( view.ZeroPadding, GUILayout.Width( 16f ) ) ;
				if( zeroPadding != view.ZeroPadding )
				{
					Undo.RecordObject( view, "ImageNumber : Zero Padding Change" ) ;	// アンドウバッファに登録
					view.ZeroPadding = zeroPadding ;
					EditorUtility.SetDirty( view ) ;
				}
				GUILayout.Label( "Zero Padding", GUILayout.Width( 80f ) ) ;
//			}
//			GUILayout.EndHorizontal() ;		// 横並び終了
		
			GUILayout.Label( "", GUILayout.Width( 10f ) ) ;
		
			// パーセント
//			GUILayout.BeginHorizontal() ;	// 横並び
//			{
				// 各種ＧＵＩのデフォルトリソースのロードを有効にするか
				bool percent = EditorGUILayout.Toggle( view.Percent, GUILayout.Width( 16f ) ) ;
				if( percent != view.Percent )
				{
					Undo.RecordObject( view, "ImageNumber : Percent Change" ) ;	// アンドウバッファに登録
					view.Percent = percent ;
					EditorUtility.SetDirty( view ) ;
				}
				GUILayout.Label( "Percent", GUILayout.Width( 80f ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		}


		// AtlasSprite の項目を描画する
		private void DrawAtlas( ImageNumber view )
		{
			SpriteSet spriteSet ;
			
			Texture spriteSetTextureBase = null ;
			if( view.SpriteSet != null )
			{
				spriteSetTextureBase = view.SpriteSet.Texture ;
			}

			Texture spriteSetTexture = EditorGUILayout.ObjectField( "Sprite Set", spriteSetTextureBase, typeof( Texture ), false ) as Texture ;
			if( spriteSetTexture != spriteSetTextureBase )
			{
				Undo.RecordObject( view, "SpriteSet : Change" ) ;	// アンドウバッファに登録

				List<Sprite> list = new List<Sprite>() ;

				if( spriteSetTexture != null )
				{
					string path = AssetDatabase.GetAssetPath( spriteSetTexture.GetInstanceID() ) ;

					// テクスチャからパスを取得してマルチタイプスプライトとしてロードする
					UnityEngine.Object[] spriteAll = AssetDatabase.LoadAllAssetsAtPath( path ) ;

					if( spriteAll != null )
					{
						int i, l = spriteAll.Length ;
						for( i  = 0 ; i <  l ; i ++ )
						{
							if( spriteAll[ i ] is UnityEngine.Sprite )
							{
								list.Add( spriteAll[ i ] as UnityEngine.Sprite ) ;
							}
						}
					}
				}

				if( list.Count >  0 )
				{
					// 存在するので更新する

					spriteSet = SpriteSet.Create() ;
					spriteSet.SetSprites( list.ToArray() ) ;
//					atlasSprite.name = tAtlasTexture.name + "(Instance Only)" ;
					view.SpriteSet = spriteSet ;
				}
				else
				{
					// 存在しないのでクリアする
					view.SpriteSet = null ;
				}

				EditorUtility.SetDirty( view ) ;
			}
		}
	}
}

#endif
