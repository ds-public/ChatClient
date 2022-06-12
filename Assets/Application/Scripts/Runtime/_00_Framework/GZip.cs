using System ;
using System.IO ;
using System.Collections ;
using System.Collections.Generic ;

using Cysharp.Threading.Tasks ;

using UnityEngine ;

using ICSharpCode.SharpZipLib.GZip ;

namespace Template
{
	/// <summary>
	/// ＧＺｉｐクラス Version 2021/06/21 0
	/// </summary>
	public class GZip : ExMonoBehaviour
	{
		private static GZip	m_Instance ;
		internal void Awake()
		{
			m_Instance = this ;
		}
		internal void OnDestroy()
		{
			m_Instance = null ;
		}

		//-----------------------------------

		/// <summary>
		/// 伸長・圧縮のバッファサイズ
		/// </summary>
		public static int BufferSize = 4096 ;

		/// <summary>
		/// ＧＺｉｐ形式で圧縮されたバイト配列を伸張する
		/// </summary>
		/// <param name="data"></param>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static byte[] Decompress( byte[] data )
		{
			if( data != null && data.Length >  0 )
			{
				MemoryStream mis = new MemoryStream( data ) ;
	
				return Decompress_Private( mis ) ;
			}
			return null ;
		}

		/// <summary>
		/// ＧＺｉｐ形式で圧縮された環境パスのファイルを伸張する
		/// </summary>
		/// <param name="data"></param>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static byte[] Decompress( string path )
		{
			FileStream fis = File.OpenRead( path ) ;
			if( fis != null )
			{
				return Decompress_Private( fis ) ;
			}
			return null ;
		}

		private static byte[] Decompress_Private( Stream cis )
		{
			byte[] result ;

			GZipInputStream gis = new GZipInputStream( cis ) ;

			MemoryStream mos ;

			//----------------------------------

			byte[] buffer = new byte[ BufferSize ] ;
			int length ;

			mos = new MemoryStream() ;

			do
			{
				length = gis.Read( buffer, 0, buffer.Length ) ;
				if( length >  0 )
				{
					mos.Write( buffer, 0, length ) ;
				}
			}
			while( length >  0 ) ;

			result = mos.ToArray() ;

			mos.Close() ;

			gis.Close() ;

			cis.Close() ;

			return result ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ＧＺｉｐ形式で圧縮されたバイト配列から任意のファイルのバイト配列を取得する(非同期)
		/// </summary>
		/// <param name="data"></param>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static async UniTask<byte[]> DecompressAsync( byte[] data, Action<byte[]> onAction = null )
		{
			if( data != null && data.Length >  0 )
			{
				MemoryStream mis = new MemoryStream( data ) ;
				
				return await DecompressAsync_Private( mis, onAction ) ;
			}
			return null ;
		}

		/// <summary>
		/// ＧＺｉｐ形式で圧縮された環境パスのファィルから任意のファイルのバイト配列を取得する(非同期)
		/// </summary>
		/// <param name="data"></param>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static async UniTask<byte[]> DecompressAsync( string path, Action<byte[]> onAction = null )
		{
			FileStream fis = File.OpenRead( path ) ;
			if( fis != null )
			{
				return await DecompressAsync_Private( fis,onAction ) ;
			}
			return null ;
		}

		private static async UniTask<byte[]> DecompressAsync_Private( Stream cis, Action<byte[]> onAction )
		{
			byte[] result ;

			GZipInputStream gis = new GZipInputStream( cis ) ;

			MemoryStream mos ;

			//----------------------------------

			byte[] buffer = new byte[ BufferSize ] ;
			int length ;

			mos = new MemoryStream() ;

			do
			{
				length = gis.Read( buffer, 0, buffer.Length ) ;
				if( length >  0 )
				{
					mos.Write( buffer, 0, length ) ;
					await m_Instance.Yield() ;
				}
			}
			while( length >  0 ) ;

			result = mos.ToArray() ;

			mos.Close() ;

			gis.Close() ;

			cis.Close() ;

			if( result != null )
			{
				onAction?.Invoke( result ) ;

				return result ;
			}
			else
			{
				Debug.LogError( "Decompress Failed" ) ;
				return null ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ＧＺｉｐ形式で圧縮したバイナリ配列をバイト配列から生成する(同期)
		/// </summary>
		/// <param name="password"></param>
		/// <param name="sources"></param>
		/// <returns></returns>
		public static byte[] Compress( byte[] data, int level = 3 )
		{
			if( data != null && data.Length >  0 )
			{
				MemoryStream mis = new MemoryStream( data ) ;
	
				return Compress_Private( mis, level ) ;
			}
			return null ;
		}

		/// <summary>
		/// ＧＺｉｐ形式で圧縮したバイナリ配列を環境パスのファイルから生成する(同期)
		/// </summary>
		/// <param name="password"></param>
		/// <param name="sources"></param>
		/// <returns></returns>
		public static byte[] Compress( string path, int level = 3 )
		{
			FileStream fis = File.OpenRead( path ) ;
			if( fis != null )
			{
				return Compress_Private( fis, level ) ;
			}
			return null ;
		}

		private static byte[] Compress_Private( Stream cis, int level )
		{
			byte[] result ;

			MemoryStream mos = new MemoryStream() ;
			GZipOutputStream gos = new GZipOutputStream( mos ) ;

			gos.SetLevel( level ) ;

			//----------------------------------------------------------

			byte[] buffer = new byte[ BufferSize ] ;	// ファイル用のバッファ
			int length ;

			do
			{
				length = cis.Read( buffer, 0, buffer.Length ) ;
				if( length >  0 )
				{
					gos.Write( buffer, 0, length ) ;
				}
			}
			while( length >  0 ) ;

			//----------------------------------------------------------

			gos.Finish() ;
			gos.Close() ;

			result = mos.ToArray() ;

			mos.Close() ;

			return result ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ＧＺｉｐ形式で圧縮したバイナリ配列をバイト配列から生成する(非同期)
		/// </summary>
		/// <param name="password"></param>
		/// <param name="sources"></param>
		/// <returns></returns>
		public static async UniTask<byte[]> CompressAsync( byte[] data, Action<byte[]> onAction = null, int level = 3 )
		{
			if( data != null && data.Length >  0 )
			{
				MemoryStream mis = new MemoryStream( data ) ;
				
				return await CompressAsync_Private( mis, onAction, level ) ;
			}
			return null ;
		}

		/// <summary>
		/// ＧＺｉｐ形式で圧縮したバイナリ配列を環境パスのファイルから生成する(非同期)
		/// </summary>
		/// <param name="password"></param>
		/// <param name="sources"></param>
		/// <returns></returns>
		public static async UniTask<byte[]> CompressAsync( string path, Action<byte[]> onAction = null, int level = 3 )
		{
			FileStream fis = File.OpenRead( path ) ;
			if( fis != null )
			{
				return await CompressAsync_Private( fis, onAction, level ) ;
			}
			return null ;
		}

		private static async UniTask<byte[]> CompressAsync_Private( Stream cis, Action<byte[]> onAction, int level )
		{
			byte[] result ;

			MemoryStream mos = new MemoryStream() ;
			GZipOutputStream gos = new GZipOutputStream( mos ) ;

			gos.SetLevel( level ) ;

			//----------------------------------------------------------

			byte[] buffer = new byte[ BufferSize ] ;	// ファイル用のバッファ
			int length ;

			do
			{
				length = cis.Read( buffer, 0, buffer.Length ) ;
				if( length >  0 )
				{
					gos.Write( buffer, 0, length ) ;
					await m_Instance.Yield() ;
				}
			}
			while( length >  0 ) ;

			//----------------------------------------------------------

			gos.Finish() ;
			gos.Close() ;

			result = mos.ToArray() ;

			mos.Close() ;

			if( result != null )
			{
				onAction?.Invoke( result ) ;
				return result ;
			}
			else
			{
				Debug.LogError( "Compress Failed" ) ;
				return null ;
			}
		}
	}
}
