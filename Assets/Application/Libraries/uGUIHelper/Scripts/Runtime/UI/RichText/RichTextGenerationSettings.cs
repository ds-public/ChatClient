using UnityEngine ;
using System ;


namespace uGUIHelper
{
	public struct RichTextGenerationSettings
	{
		public Vector2				generationExtents ;
		public Vector2				pivot ;

		public Font					font ;
		public FontStyle			fontStyle ;
		public int					fontSize ;
		public float				lineSpacing ;
		public bool					richText ;
		public TextAnchor			textAnchor ;
		public VerticalWrapMode		verticalOverflow ;
		public HorizontalWrapMode	horizontalOverflow ;
		public bool					resizeTextForBestFit ;
		public int					resizeTextMinSize ;
		public int					resizeTextMaxSize ;
		public Color				color ;

		public bool					updateBounds ;
		public bool					generateOutOfBounds ;
		public float				scaleFactor ;
		public bool					alignByGeometry ;

		public bool					viewControllEnabled ;
		public int					lengthOfView ;
		public int					startLineOfView ;
		public int					endLineOfView ;
		public int					startOffsetOfFade ;
		public int					endOffsetOfFade ;
		public float				ratioOfFade ;
		public int					widthOfFade ;

		public float				rubySizeScale ;
		public float				supOrSubSizeScale ;
		public float				topMarginSpacing ;
		public float				bottomMarginSpacing ;
		
		private bool CompareColors( Color c1, Color c2 )
		{
			Color32 tColor1	= c1 ;
			Color32 tColor2	= c2 ;
			return tColor1.Equals( tColor2 ) ;
		}
		
		private bool CompareVector2( Vector2 v1, Vector2 v2 )
		{
			return Mathf.Approximately( v1.x, v2.x ) && Mathf.Approximately( v1.y, v2.y ) ;
		}
		
		public bool Equals( RichTextGenerationSettings other )
		{
#if false
			if( this.CompareVector2( this.generationExtents, other.generationExtents ) == false )
			{
				Debug.LogWarning( "generationExtents が異なる:" + this.generationExtents + " : " + other.generationExtents ) ;
			}

			if( this.CompareVector2( this.pivot, other.pivot ) == false )
			{
				Debug.LogWarning( "pivot が異なる:" + this.pivot + " : " + other.pivot ) ;
			}

			if( this.font != other.font )
			{
				Debug.LogWarning( "font が異なる" ) ;
			}

			if( this.fontStyle != other.fontStyle )
			{
				Debug.LogWarning( "fontStyle が異なる" ) ;
			}

			if( this.fontSize != other.fontSize )
			{
				Debug.LogWarning( "fontSize が異なる" ) ;
			}

			if( Mathf.Approximately( this.lineSpacing, other.lineSpacing ) == false )
			{
				Debug.LogWarning( "lineSpacing が異なる" ) ;
			}

			if( this.richText != other.richText )
			{
				Debug.LogWarning( "richText が異なる" ) ;
			}

			if( this.textAnchor != other.textAnchor )
			{
				Debug.LogWarning( "textAnchor が異なる" ) ;
			}

			if( this.horizontalOverflow != other.horizontalOverflow )
			{
				Debug.LogWarning( "horizontalOverflow が異なる" ) ;
			}

			if( this.verticalOverflow != other.verticalOverflow )
			{
				Debug.LogWarning( "verticalOverflow が異なる" ) ;
			}

			if( this.resizeTextForBestFit != other.resizeTextForBestFit )
			{
				Debug.LogWarning( "resizeTextForBestFit が異なる" ) ;
			}

			if( this.resizeTextMinSize != other.resizeTextMinSize )
			{
				Debug.LogWarning( "resizeTextMinSize が異なる" ) ;
			}

			if( this.resizeTextMaxSize != other.resizeTextMaxSize )
			{
				Debug.LogWarning( "resizeTextMaxSize が異なる" ) ;
			}

			if( this.CompareColors( this.color, other.color ) == false )
			{
				Debug.LogWarning( "color が異なる" ) ;
			}

			if( this.updateBounds != other.updateBounds )
			{
				Debug.LogWarning( "updateBounds が異なる" ) ;
			}

			if( this.lengthOfView != other.lengthOfView )
			{
				Debug.LogWarning( "lengthOfView が異なる" ) ;
			}

			if( this.startLineOfView != other.startLineOfView )
			{
				Debug.LogWarning( "startLineOfView が異なる" ) ;
			}

			if( this.endLineOfView != other.endLineOfView )
			{
				Debug.LogWarning( "endLineOfView が異なる" ) ;
			}

			if( Mathf.Approximately( this.rubySizeScale, other.rubySizeScale ) == false )
			{
				Debug.LogWarning( "rubySizeScale が異なる" ) ;
			}

			if( Mathf.Approximately( this.supOrSubSizeScale, other.supOrSubSizeScale ) == false )
			{
				Debug.LogWarning( "supOrSubSizeScale が異なる" ) ;
			}

			if( Mathf.Approximately( this.topMarginSpacing, other.topMarginSpacing ) == false )
			{
				Debug.LogWarning( "topMarginSpacing が異なる" ) ;
			}

			if( Mathf.Approximately( this.bottomMarginSpacing, other.bottomMarginSpacing ) == false )
			{
				Debug.LogWarning( "bottomMarginSpacing が異なる" ) ;
			}
#endif

			return
				this.CompareVector2( this.generationExtents, other.generationExtents ) == true &&
				this.CompareVector2( this.pivot, other.pivot ) == true &&
				this.font == other.font &&
				this.fontStyle == other.fontStyle &&
				this.fontSize == other.fontSize &&
				Mathf.Approximately( this.lineSpacing, other.lineSpacing ) == true &&
				this.richText == other.richText &&
				this.textAnchor == other.textAnchor &&
				this.horizontalOverflow == other.horizontalOverflow &&
				this.verticalOverflow == other.verticalOverflow &&
				this.resizeTextForBestFit == other.resizeTextForBestFit &&
				this.resizeTextMinSize == other.resizeTextMinSize &&
				this.resizeTextMaxSize == other.resizeTextMaxSize &&
				this.CompareColors( this.color, other.color ) == true &&
				this.updateBounds == other.updateBounds &&

				this.viewControllEnabled == other.viewControllEnabled &&
				this.lengthOfView == other.lengthOfView &&
				this.startLineOfView == other.startLineOfView &&
				this.endLineOfView == other.endLineOfView &&
				this.startOffsetOfFade == other.startOffsetOfFade &&
				this.endOffsetOfFade == other.endOffsetOfFade &&
				Mathf.Approximately( this.ratioOfFade, other.ratioOfFade ) == true &&
				this.widthOfFade == other.widthOfFade &&

				Mathf.Approximately( this.rubySizeScale, other.rubySizeScale ) == true &&
				Mathf.Approximately( this.supOrSubSizeScale, other.supOrSubSizeScale ) == true &&
				Mathf.Approximately( this.topMarginSpacing, other.topMarginSpacing ) == true &&
				Mathf.Approximately( this.bottomMarginSpacing, other.bottomMarginSpacing ) == true ;
		}
	}
}
