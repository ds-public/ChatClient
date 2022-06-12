#if UNITY_EDITOR

using System ;
using System.IO ;
using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;
using UnityEditor ;

using TMPro ;

namespace uGUIHelper
{
	/// <summary>
	/// デフォルトフォント設定の操作ウィンドウ
	/// </summary>
	
	public class DefaultSettingsMenu : EditorWindow
	{
		[ MenuItem( "uGUIHelper/Tools/DefaultSettings" ) ]
		internal static void OpenWindow()
		{
			EditorWindow.GetWindow<DefaultSettingsMenu>( false, "uGUIHelper Default Settings", true ) ;
		}

		// レイアウトを描画する
		private void OnGUI()
		{
			string path = null ;
			string[] ids = AssetDatabase.FindAssets( "DefaultSettings" ) ;
			if( ids != null )
			{
				int i, l = ids.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					path = AssetDatabase.GUIDToAssetPath( ids[ i ] ) ;

					if( path.Contains( "Runtime/DefaultSettings" ) == true && Directory.Exists( path ) == true )
					{
						break ;
					}
				}

				if( i >= l )
				{
					path = null ;
				}
			}

			if( string.IsNullOrEmpty( path ) == true )
			{
				EditorGUILayout.HelpBox( "状態が異常です", MessageType.Warning ) ;
				return ;
			}

			path += "/Resources/uGUIHelper" ;
			if( Directory.Exists( path ) == false )
			{
				EditorGUILayout.HelpBox( "保存フォルダが存在しません", MessageType.Warning ) ;
				return ;
			}

			DefaultSettings ds ;

			path += "/DefaultSettings.asset" ;
			if( File.Exists( path ) == false )
			{
				// ファイルが存在しない
				ds = ScriptableObject.CreateInstance<DefaultSettings>() ;
				ds.name = "DefaultSettings" ;
		
				AssetDatabase.CreateAsset( ds, path ) ;
				AssetDatabase.Refresh() ;
			}
			else
			{
				// ファイルが存在する
				ds = AssetDatabase.LoadAssetAtPath<DefaultSettings>( path ) ;
			}

			Selection.activeObject = ds ;

			//----------------------------------------------------------

			bool dirty = false ;

			// ボタンの下地
			Sprite buttonFrame = EditorGUILayout.ObjectField( "Button", ds.ButtonFrame, typeof( Sprite ), false ) as Sprite ;
			if( buttonFrame != ds.ButtonFrame )
			{
				ds.ButtonFrame		= buttonFrame ;
				dirty = true ;
			}

			// ボタン無効化時の色
			Color c ;
			c.r = ds.ButtonDisabledColor.r ;
			c.g = ds.ButtonDisabledColor.g ;
			c.b = ds.ButtonDisabledColor.b ;
			c.a = ds.ButtonDisabledColor.a ;
			ds.ButtonDisabledColor = EditorGUILayout.ColorField( "Button Disabled Color", ds.ButtonDisabledColor ) ;
			if( ds.ButtonDisabledColor.Equals( c ) == false )
			{
				dirty = true ;
			}

			// ボタンラベルのフォントサイズ
			int buttonLabelFontSize =  EditorGUILayout.IntField( "Button Label Font Size", ds.ButtonLabelFontSize ) ;
			if( buttonLabelFontSize != ds.ButtonLabelFontSize )
			{
				ds.ButtonLabelFontSize	= buttonLabelFontSize ;
				dirty = true ;
			}

			// ボタンラベルの色
			c.r = ds.ButtonLabelColor.r ;
			c.g = ds.ButtonLabelColor.g ;
			c.b = ds.ButtonLabelColor.b ;
			c.a = ds.ButtonLabelColor.a ;
			ds.ButtonLabelColor = EditorGUILayout.ColorField( "Button Label Color", ds.ButtonLabelColor ) ;
			if( ds.ButtonLabelColor.Equals( c ) == false )
			{
				dirty = true ;
			}

			bool buttonLabelShadow = EditorGUILayout.Toggle( "Button Label Shadow", ds.ButtonLabelShadow ) ;
			if( buttonLabelShadow != ds.ButtonLabelShadow )
			{
				ds.ButtonLabelShadow		= buttonLabelShadow ;
				dirty = true ;
			}

			bool buttonLabelOutline = EditorGUILayout.Toggle( "Button Label Outline", ds.ButtonLabelOutline ) ;
			if( buttonLabelOutline != ds.ButtonLabelOutline )
			{
				ds.ButtonLabelOutline		= buttonLabelOutline ;
				dirty = true ;
			}

			//----------------------------------

			// プログレスバーの下地
			Sprite progressbarFrame = EditorGUILayout.ObjectField( "ProgressbarFrame", ds.ProgressbarFrame, typeof( Sprite ), false ) as Sprite ;
			if( progressbarFrame != ds.ProgressbarFrame )
			{
				ds.ProgressbarFrame	= progressbarFrame ;
				dirty = true ;
			}

			// プログレスバーの前景
			Sprite progressbarThumb = EditorGUILayout.ObjectField( "ProgressbarThumb", ds.ProgressbarThumb, typeof( Sprite ), false ) as Sprite ;
			if( progressbarThumb != ds.ProgressbarThumb )
			{
				ds.ProgressbarThumb	= progressbarThumb ;
				dirty = true ;
			}

			//----------------------------------

			// テキストの色
			c.r = ds.TextColor.r ;
			c.g = ds.TextColor.g ;
			c.b = ds.TextColor.b ;
			c.a = ds.TextColor.a ;
			ds.TextColor = EditorGUILayout.ColorField( "Text Color", ds.TextColor ) ;
			if( ds.TextColor.Equals( c ) == false )
			{
				dirty = true ;
			}

			//----------------------------------

			// フォント
			Font text_Font = EditorGUILayout.ObjectField( "Text Font", ds.Text_Font, typeof( Font ), false ) as Font ;
			if( text_Font != ds.Text_Font )
			{
				ds.Text_Font		= text_Font ;
				dirty = true ;
			}

			// フォントサイズ
			int text_FontSize =  EditorGUILayout.IntField( "Text FontSize", ds.Text_FontSize ) ;
			if( text_FontSize != ds.Text_FontSize )
			{
				ds.Text_FontSize	= text_FontSize ;
				dirty = true ;
			}

			//----------------------------------

			// フォント
			Font number_Font = EditorGUILayout.ObjectField( "Number Font", ds.Text_Font, typeof( Font ), false ) as Font ;
			if( number_Font != ds.Number_Font )
			{
				ds.Number_Font		= number_Font ;
				dirty = true ;
			}

			// フォントサイズ
			int number_FontSize =  EditorGUILayout.IntField( "Number FontSize", ds.Text_FontSize ) ;
			if( number_FontSize != ds.Number_FontSize )
			{
				ds.Number_FontSize	= number_FontSize ;
				dirty = true ;
			}

			//----------------------------------

			// フォントアセット
			TMP_FontAsset textMesh_FontAsset = EditorGUILayout.ObjectField( "TextMesh FontAsset", ds.TextMesh_FontAsset, typeof( TMP_FontAsset ), false ) as TMP_FontAsset ;
			if( textMesh_FontAsset != ds.TextMesh_FontAsset )
			{
				ds.TextMesh_FontAsset	= textMesh_FontAsset ;
				dirty = true ;
			}

			// フォントマテリアル
			Material textMesh_FontMaterial = EditorGUILayout.ObjectField( "TextMesh FontMaterial", ds.TextMesh_FontMaterial, typeof( Material ), false ) as Material ;
			if( textMesh_FontMaterial != ds.TextMesh_FontMaterial )
			{
				ds.TextMesh_FontMaterial	= textMesh_FontMaterial ;
				dirty = true ;
			}

			// フォントサイズ
			int textMesh_FontSize =  EditorGUILayout.IntField( "TextMesh FontSize", ds.TextMesh_FontSize ) ;
			if( textMesh_FontSize != ds.TextMesh_FontSize )
			{
				ds.TextMesh_FontSize	= textMesh_FontSize ;
				dirty = true ;
			}

			//----------------------------------

			// フォントアセット
			TMP_FontAsset numberMesh_FontAsset = EditorGUILayout.ObjectField( "NumberMesh FontAsset", ds.NumberMesh_FontAsset, typeof( TMP_FontAsset ), false ) as TMP_FontAsset ;
			if( numberMesh_FontAsset != ds.NumberMesh_FontAsset )
			{
				ds.NumberMesh_FontAsset	= numberMesh_FontAsset ;
				dirty = true ;
			}

			// フォントマテリアル
			Material numberMesh_FontMaterial = EditorGUILayout.ObjectField( "NumberMesh FontMaterial", ds.NumberMesh_FontMaterial, typeof( Material ), false ) as Material ;
			if( numberMesh_FontMaterial != ds.NumberMesh_FontMaterial )
			{
				ds.NumberMesh_FontMaterial	= numberMesh_FontMaterial ;
				dirty = true ;
			}

			// フォントサイズ
			int numberMesh_FontSize =  EditorGUILayout.IntField( "NumberMesh FontSize", ds.NumberMesh_FontSize ) ;
			if( numberMesh_FontSize != ds.NumberMesh_FontSize )
			{
				ds.NumberMesh_FontSize	= numberMesh_FontSize ;
				dirty = true ;
			}

			//----------------------------------

			// インプットフィールド関係
			FontFilter fontFilter = EditorGUILayout.ObjectField( "Font Filter", ds.FontFilter, typeof( FontFilter ), false ) as FontFilter ;
			if( fontFilter != ds.FontFilter )
			{
				ds.FontFilter = fontFilter ;
				dirty = true ;
			}

			string fontAlternateCodeOld = "" + ds.FontAlternateCode ;
			string fontAlternateCodeNew = EditorGUILayout.TextField( "Font Alternate Code", fontAlternateCodeOld ) ;
			if( string.IsNullOrEmpty( fontAlternateCodeNew ) == false && ( fontAlternateCodeNew[ 0 ] != fontAlternateCodeOld[ 0 ] ) )
			{
				ds.FontAlternateCode = fontAlternateCodeNew[ 0 ] ;
				dirty = true ;
			}

			//----------------------------------

			// 更新判定
			if( dirty == true )
			{
				EditorUtility.SetDirty( ds ) ; // 更新実行
//				AssetDatabase.Refresh() ;
			}
		}
	}
}

#endif
