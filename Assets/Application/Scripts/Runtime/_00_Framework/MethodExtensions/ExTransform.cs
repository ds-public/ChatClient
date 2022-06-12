using System ;
using UnityEngine ;

namespace Template
{
	/// <summary>
	/// <see cref="Transform" />の拡張メソッド Version 2020/12/26
	/// </summary>
	public static class ExTransform
	{
		/// <summary>
		/// ヒエラルキーの階層パス名を取得する
		/// </summary>
		/// <param name="self"></param>
		/// <returns></returns>
		public static string GetHierarchyPath( this Transform self )
		{
			string path = self.name ;
			var parent = self.parent ;
			while( parent != null )
			{
				path = $"{parent.name}/{path}" ;
				parent = parent.parent ;
			}
			return path ;
		}
		
		/// <summary>
		/// ローカル座標を設定する
		/// </summary>
		/// <param name="self"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <returns></returns>
		public static Transform SetPosition( this Transform self, float x, float y, float z )
		{
			self.localPosition = new Vector3( x, y, z ) ;
			return self ;
		}

		/// <summary>
		/// ローカルＸを設定する
		/// </summary>
		/// <param name="self"></param>
		/// <param name="x"></param>
		/// <returns></returns>
		public static Transform SetX( this Transform self, float x )
		{
			self.localPosition = new Vector3( x, self.localPosition.y, self.localPosition.z ) ;
			return self ;
		}

		/// <summary>
		/// ローカルＹを設定する
		/// </summary>
		/// <param name="self"></param>
		/// <param name="x"></param>
		/// <returns></returns>
		public static Transform SetY( this Transform self, float y )
		{
			self.localPosition = new Vector3( self.localPosition.x, y, self.localPosition.z ) ;
			return self ;
		}

		/// <summary>
		/// ローカルＺを設定する
		/// </summary>
		/// <param name="self"></param>
		/// <param name="x"></param>
		/// <returns></returns>
		public static Transform SetZ( this Transform self, float z )
		{
			self.localPosition = new Vector3( self.localPosition.x, self.localPosition.y, z ) ;
			return self ;
		}

		/// <summary>
		/// ローカル座標での移動を行う
		/// </summary>
		/// <param name="self"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <returns></returns>
		public static Transform Move( this Transform self, float x, float y, float z )
		{
			self.localPosition = new Vector3( self.localPosition.x + x, self.localPosition.y + y, self.localPosition.z + z ) ;
			return self ;
		}
	}
}
