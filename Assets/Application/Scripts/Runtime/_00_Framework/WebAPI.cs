using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;

namespace Template
{
	/// <summary>
	/// ウェブＡＰＩのファサードクラス Version 2022/05/04
	/// </summary>
	public class WebAPI
	{
		//---------------------------------------------------------------------

		// カテゴリごとの通信ＡＰＩのスタティックインスタンスを記述する

		// デバッガーカテゴリの WebAPI 群
		public static WebAPIs.Debugger			Debugger			= new WebAPIs.Debugger() ;

		// メンテナンスカテゴリの WebAPI 群
		public static WebAPIs.Maintenance		Maintenance			= new WebAPIs.Maintenance() ;

		// メンテナンスカテゴリの WebAPI 群
		public static WebAPIs.Player			Player				= new WebAPIs.Player() ;

		
		//---------------------------------------------------------------------
	}
}
