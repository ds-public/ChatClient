using System ;
using System.Text ;
using System.Collections.Generic ;

using Cysharp.Threading.Tasks ;

using UnityEngine ;

namespace Template
{
	/// <summary>
	/// ダウンロードのファサード(窓口)クラス Version 2022/06/07
	/// </summary>
	public class Download
	{
		//-----------------------------------------------------------

		/// <summary>
		/// ファイルをダウンロードしバイト配列として取得する
		/// </summary>
		/// <param name="url"></param>
		/// <param name="onReceived"></param>
		/// <param name="onProgress"></param>
		/// <param name="useProgress"></param>
		/// <param name="useDialog"></param>
		/// <returns></returns>
		public static async UniTask<byte[]> ToBytes( string url, Action<string,byte[]> onReceived = null, Action<int, int> onProgress = null, bool useProgress = true, bool useDialog = true, string title = null, string message = null )
		{
			// 正常系の対応のみ考えれば良い(エラーはWebAPIManager内で処理される)
			byte[] responseData = await DownloadManager.SendRequest
			(
				url,			// Location
				onProgress,
				useProgress,
				useDialog,
				title,
				message
			) ;

			if( responseData == null )
			{
				onReceived?.Invoke( url, null ) ;
				return null ;	// エラー
			}

			onReceived?.Invoke( url, responseData ) ;
			return responseData ;
		}

		/// <summary>
		/// ファイルをダウンロードしテキストとして取得する
		/// </summary>
		/// <param name="url"></param>
		/// <param name="onReceived"></param>
		/// <param name="onProgress"></param>
		/// <param name="useProgress"></param>
		/// <param name="useDialog"></param>
		/// <returns></returns>
		public static async UniTask<string> ToText( string url, Action<string,string> onReceived, Action<int, int> onProgress = null, bool useProgress = true, bool useDialog = true, string title = null, string message = null )
		{
			byte[] responseData = await ToBytes( url, null, onProgress, useProgress, useDialog, title, message ) ;
			if( responseData == null || responseData.Length == 0 )
			{
				// 失敗
				onReceived?.Invoke( url, null ) ;
				return null ;
			}

			string text = UTF8Encoding.UTF8.GetString( responseData ) ;

			onReceived?.Invoke( url, text ) ;
			return text ;
		}

		/// <summary>
		/// ファイルをダウンロードしテクスチャとして取得する
		/// </summary>
		/// <param name="url"></param>
		/// <param name="onReceived"></param>
		/// <param name="onProgress"></param>
		/// <param name="useProgress"></param>
		/// <param name="useDialog"></param>
		/// <returns></returns>
		public static async UniTask<Texture2D> ToTexture( string url, Action<string,Texture2D> onReceived = null, Action<int, int> onProgress = null, bool useProgress = true, bool useDialog = true, string title = null, string message = null )
		{
			byte[] responseData = await ToBytes( url, null, onProgress, useProgress, useDialog, title, message ) ;
			if( responseData == null || responseData.Length == 0 )
			{
				// 失敗
				onReceived?.Invoke( url, null ) ;
				return null ;
			}

			Texture2D texture = new Texture2D( 4, 4, TextureFormat.ARGB32, false, true ) ;
			texture.LoadImage( responseData ) ;

			onReceived?.Invoke( url, texture ) ;
			return texture ;
		}

		/// <summary>
		/// ファイルをダウンロードしスプライトとして取得する
		/// </summary>
		/// <param name="url"></param>
		/// <param name="onReceived"></param>
		/// <param name="onProgress"></param>
		/// <param name="useProgress"></param>
		/// <param name="useDialog"></param>
		/// <returns></returns>
		public static async UniTask<Sprite> ToSprite( string url, Action<string,Sprite> onReceived = null, Action<int, int> onProgress = null, bool useProgress = true, bool useDialog = true, string title = null, string message = null )
		{
			byte[] responseData = await ToBytes( url, null, onProgress, useProgress, useDialog, title, message ) ;
			if( responseData == null || responseData.Length == 0 )
			{
				// 失敗
				onReceived?.Invoke( url, null ) ;
				return null ;
			}

			Texture2D texture = new Texture2D( 4, 4, TextureFormat.ARGB32, false, true ) ;
			texture.LoadImage( responseData ) ;

			Sprite sprite = Sprite.Create
			(
				texture,
				new Rect
				(
					0,
					0,
					texture.width,
					texture.height
				),
				new Vector2( 0.5f, 0.5f )
			) ;

			onReceived?.Invoke( url, sprite ) ;
			return sprite ;
		}

		/// <summary>
		/// ファイルをダウンロードしアセットバンドルとして取得する(取得後に必ずUnloadする事)
		/// </summary>
		/// <param name="url"></param>
		/// <param name="onReceived"></param>
		/// <param name="onProgress"></param>
		/// <param name="useProgress"></param>
		/// <param name="useDialog"></param>
		/// <returns></returns>
		public static async UniTask<AssetBundle> ToAssetBundle( string url, Action<string,AssetBundle> onReceived, Action<int, int> onProgress = null, bool useProgress = true, bool useDialog = true, string title = null, string message = null )
		{
			byte[] responseData = await ToBytes( url, null, onProgress, useProgress, useDialog, title, message ) ;
			if( responseData == null || responseData.Length == 0 )
			{
				// 失敗
				onReceived?.Invoke( url, null ) ;
				return null ;
			}

			AssetBundle assetBundle = AssetBundle.LoadFromMemory( responseData ) ;

			onReceived?.Invoke( url, assetBundle ) ;
			return assetBundle ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// リクエストを破棄する
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static bool RemoveRequest( string path )
		{
			return DownloadManager.RemoveRequest( path ) ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// HTTP ヘッダー情報を追加する(削除するまで通信毎に付与される)
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool AddHeader( string key, string value )
		{
			return DownloadManager.AddHeader( key, value ) ;
		}

		/// <summary>
		/// HTTP ヘッダー情報を削除する(削除するまで通信毎に付与される)
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool RemoveHeader( string key )
		{
			return DownloadManager.RemoveHeader( key ) ;
		}

		/// <summary>
		/// HTTP ヘッダー情報を全て削除する(削除するまで通信毎に付与される)
		/// </summary>
		/// <returns></returns>
		public static bool RemoveAllHeaders()
		{
			return DownloadManager.RemoveAllHeaders() ;
		}
	}
}

