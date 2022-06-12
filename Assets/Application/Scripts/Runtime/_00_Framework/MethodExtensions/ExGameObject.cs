using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

namespace Template
{
	/// <summary>
	/// GameObject 型のメソッド拡張 Version 2021/07/03
	/// </summary>
	public static class ExGameObject
	{
		/// <summary>
		/// 複製を行う(親や姿勢は引き継ぐ)
		/// </summary>
		/// <param name="self"></param>
		/// <returns></returns>
		public static GameObject Duplicate( this GameObject self )
		{
			GameObject clone = GameObject.Instantiate( self, self.transform.parent ) ;
			return clone ;
		}

		/// <summary>
		/// 複製し指定のコンポーネントを取得する(親や姿勢は引き継ぐ)
		/// </summary>
		/// <param name="self"></param>
		/// <returns></returns>
		public static T Duplicate<T>( this GameObject self ) where T : UnityEngine.Component
		{
			GameObject clone = GameObject.Instantiate( self, self.transform.parent ) ;
			return clone.GetComponent<T>() ;
		}

		/// <summary>
		/// ヒエラルキーの階層パス名を取得する
		/// </summary>
		/// <param name="self"></param>
		/// <returns></returns>
		public static string GetHierarchyPath( this GameObject self )
		{
			string path = self.name ;
			var parent = self.transform.parent ;
			while( parent != null )
			{
				path = $"{parent.name}/{path}" ;
				parent = parent.parent ;
			}
			return path ;
		}

		/// <summary>
		/// <seealso cref="GameObject.hideFlags"/>を自分自身だけではなく、子にも適用します
		/// </summary>
		/// <param name="gameObject"></param>
		/// <param name="hideFlags"></param>
		public static void SetHideFlagAll( this GameObject gameObject, HideFlags hideFlags )
		{
			if ( gameObject == null )
			{
				Debug.LogException( new ArgumentNullException( nameof(gameObject) ) ) ;
				return ;
			}

			gameObject.hideFlags =  hideFlags;
			Transform transform  = gameObject.transform ;
			int childCount = transform.childCount ;
			for ( int i = 0 ; i < childCount ; i++ )
			{
				Transform childTransform = transform.GetChild( i ) ;
				SetHideFlagAll( gameObject: childTransform.gameObject, hideFlags: hideFlags ) ;
			}
		}
	}
}
