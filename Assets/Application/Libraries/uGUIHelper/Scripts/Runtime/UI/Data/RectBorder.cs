using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

namespace uGUIHelper
{
	[Serializable]
	public class RectBorder
	{
		public float	left	= 0 ;
		public float	right	= 0 ;
		public float	top		= 0 ;
		public float	bottom	= 0 ;

		public float horizontal
		{
			get
			{
				return left + right ;
			}
		}

		public float vertical
		{
			get
			{
				return top + bottom ;
			}
		}

		public RectBorder()
		{
			left	= 0 ;
			right	= 0 ;
			top		= 0 ;
			bottom	= 0 ;
		}

		public RectBorder( float tLeft, float tRight, float tTop, float tBottom )
		{
			left	= tLeft ;
			right	= tRight ;
			top		= tTop ;
			bottom	= tBottom ;
		}

		public Rect Add( Rect tSR )
		{
			Rect tDR = new Rect() ;

			tDR.xMin = tSR.xMin - left ;
			tDR.yMin = tSR.yMin - bottom ;
			tDR.xMax = tSR.xMax + right ;
			tDR.yMax = tSR.yMax + top ;

			return tDR ;
		}

		public Rect Remove( Rect tSR )
		{
			Rect tDR = new Rect() ;

			tDR.xMin = tSR.xMin + left ;
			tDR.yMin = tSR.yMin + bottom ;
			tDR.xMax = tSR.xMax - right ;
			tDR.yMax = tSR.yMax - top ;

			return tDR ;
		}
	}
}

