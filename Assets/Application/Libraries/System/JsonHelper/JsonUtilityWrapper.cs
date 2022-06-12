using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JsonHelper
{
	public class JsonUtilityWrapper
	{
		public static string ToJson( object targetObject, bool prettyPrint = false, bool reduceNullElements = false )
		{
			string jsonText = JsonUtility.ToJson( targetObject, prettyPrint ) ;

			if( reduceNullElements == true )
			{
				// null になってしまう項目を削る
				JsonObject jsonObject = new JsonObject( jsonText ) ;
				jsonText = jsonObject.ToString( "\t", false ) ;
			}

			return jsonText ;
		}
	}
}
