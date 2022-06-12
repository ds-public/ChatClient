#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// GridMap のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( ComplexRectangle ) ) ]
	public class ComplexRectangleInspector : MaskableGraphicWrapperInspector
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
			ComplexRectangle tTarget = target as ComplexRectangle ;
		
			// Graphic の基本情報を描画する
			DrawBasis( tTarget ) ;

			EditorGUILayout.Separator() ;   // 少し区切りスペース

			// オフセット
			Vector2 tOffset = EditorGUILayout.Vector2Field( "Offset", tTarget.offset ) ;
			if( tOffset.Equals( tTarget.offset ) == false )
			{
				Undo.RecordObject( tTarget, "ComplexRectangle : Offset Change" ) ;	// アンドウバッファに登録
				tTarget.offset = tOffset ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			// データ配列
			SerializedObject tSO = new SerializedObject( tTarget ) ;
			SerializedProperty tSP = tSO.FindProperty( "rectangle" ) ;
			if( tSP != null )
			{
				EditorGUILayout.PropertyField( tSP, new GUIContent( "Rectangle" ), true ) ;
			}
			tSO.ApplyModifiedProperties() ;

			// テクスチャ
			Texture2D tTexture = EditorGUILayout.ObjectField( "Texture", tTarget.texture, typeof( Texture2D ), false ) as Texture2D ;
			if( tTexture != tTarget.texture )
			{
				Undo.RecordObject( tTarget, "ComplexRectangle : Texture Change" ) ;	// アンドウバッファに登録
				tTarget.texture = tTexture ;
				EditorUtility.SetDirty( tTarget ) ;
			}
		}

		//--------------------------------------------------------------------------

		private Dictionary<string,string> mJapanese_Message = new Dictionary<string, string>()
		{
			{ "SetTexture",				"テクスチャを設定してください" },
			{ "InteractionNone",		"UIInteraction クラスが必要です" },
			{ "HSB_CanvasGroupNone",	"Horizontal Scrollbar に CanvasGroup クラスが必要です" },
			{ "VSB_CanvasGroupNone",	"Vertical Scrollbar に CanvasGroup クラスが必要です" },
		} ;
		private Dictionary<string,string> mEnglish_Message = new Dictionary<string, string>()
		{
			{ "SetTexture",				"Please set the texture" },
			{ "InteractionNone",		"'UIInteraction' is necessary" },
			{ "HSB_CanvasGroupNone",	"'CanvasGroup' is necessary to horizontal scrollbar" },
			{ "VSB_CanvasGroupNone",	"'CanvasGroup' is necessary to vertical scrollbar" },
		} ;

		private string GetMessage( string tLabel )
		{
			if( Application.systemLanguage == SystemLanguage.Japanese )
			{
				if( mJapanese_Message.ContainsKey( tLabel ) == false )
				{
					return "指定のラベル名が見つかりません" ;
				}
				return mJapanese_Message[ tLabel ] ;
			}
			else
			{
				if( mEnglish_Message.ContainsKey( tLabel ) == false )
				{
					return "Specifying the label name can not be found" ;
				}
				return mEnglish_Message[ tLabel ] ;
			}
		}

	}
}

#endif
