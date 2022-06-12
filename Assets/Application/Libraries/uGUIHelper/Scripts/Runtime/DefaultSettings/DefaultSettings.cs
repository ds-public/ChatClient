using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using TMPro ;

namespace uGUIHelper
{
	/// <summary>
	/// デフォルト設定情報
	/// </summary>
	[CreateAssetMenu( fileName = "DefaultSettings", menuName = "ScriptableObject/uGUIHelper/DefaultSettings" )]
	public class DefaultSettings : ScriptableObject
	{
		// ボタン関係
		public Sprite			ButtonFrame = null ;
		public Color			ButtonDisabledColor = new Color( 0.75f, 0.75f, 0.75f, 0.5f ) ;

		public int				ButtonLabelFontSize = 0 ;
		public Color			ButtonLabelColor = Color.black ;
		public bool				ButtonLabelShadow = false ;
		public bool				ButtonLabelOutline = false ;


		// プログレス関係
		public Sprite			ProgressbarFrame = null ;
		public Sprite			ProgressbarThumb = null ;

		// テキスト関係
		public Color			TextColor = new Color( 1.0f, 1.0f, 1.0f, 1.0f ) ;

		// フォント関係
		public Font				Text_Font = null ;
		public int				Text_FontSize = 0 ;

		public Font				Number_Font = null ;
		public int				Number_FontSize = 0 ;

		public TMP_FontAsset	TextMesh_FontAsset = null ;
		public Material			TextMesh_FontMaterial = null ;
		public int				TextMesh_FontSize = 0 ;

		public TMP_FontAsset	NumberMesh_FontAsset = null ;
		public Material			NumberMesh_FontMaterial = null ;
		public int				NumberMesh_FontSize = 0 ;

		// インプットフィールド関係
		public FontFilter		FontFilter = null ;
		public char				FontAlternateCode = '？' ;
	}
}
