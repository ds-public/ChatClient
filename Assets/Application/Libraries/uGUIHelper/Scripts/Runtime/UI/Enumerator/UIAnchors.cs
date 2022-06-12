using UnityEngine ;
using System.Collections ;

namespace uGUIHelper
{
	/// <summary>
	/// スクリプトからuGUIをコントロールする際のアンカーの指定
	/// </summary>
	public enum UIAnchors
	{
		LeftTop,
		CenterTop,
		RightTop,
		StretchTop,
	
		LeftMiddle,
		CenterMiddle,
		RightMiddle,
		StretchMiddle,
	
		LeftBottom,
		CenterBottom,
		RightBottom,
		StretchBottom,
	
		LeftStretch,
		CenterStretch,
		RightStretch,
		Stretch,
	
		Center,
	
		LeftCustom,
		CenterCustom,
		RightCustom,
		StretchCustom,
	
		CustomTop,
		CustomMiddle,
		CustomBottom,
		CustomStretch,
	}
}

