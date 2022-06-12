using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

namespace Template
{
	public struct Int32Rect
	{
		public int xMin ;
		public int yMin ;
		public int xMax ;
		public int yMax ;

		public int x
		{
			get
			{
				return xMin ;
			}
			set
			{
				xMin = value ;
			}
		}

		public int y
		{
			get
			{
				return yMin ;
			}
			set
			{
				yMin = value ;
			}
		}

		public int width
		{
			get
			{
				return ( xMax - xMin ) + 1 ;
			}
			set
			{
				xMax = ( xMin + value ) - 1 ;
			}
		}

		public int height
		{
			get
			{
				return ( yMax - yMin ) + 1 ;
			}
			set
			{
				yMax = ( yMin + value ) - 1 ;
			}
		}

		public override string ToString()
		{
			return " xMin = " + xMin + " yMin = " + yMin + " xMax = " + xMax + " yMax = " + yMax ; 
		}
	}	
}
