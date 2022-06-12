using System ;
using System.IO ;
using System.Collections ;
using System.Collections.Generic ;

using Cysharp.Threading.Tasks ;

using UnityEngine ;

using ICSharpCode.SharpZipLib.Zip ;

namespace Template
{
	/// <summary>
	/// Ｚｉｐクラス Version 2021/06/21 0
	/// </summary>
	public class Zip : ExMonoBehaviour
	{
		private static Zip	m_Instance ;
		internal void Awake()
		{
			m_Instance = this ;
		}
		internal void OnDestroy()
		{
			m_Instance = null ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 伸長・圧縮のバッファサイズ
		/// </summary>
		public static int BufferSize = 4096 ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// Ｚｉｐ形式で圧縮されたバイト配列内に含まれるファイル名とサイズの一覧を取得する
		/// </summary>
		/// <returns></returns>
		public static ( string, long )[] GetFiles( byte[] data, string password = null )
		{
			if( data != null && data.Length >  0 )
			{
				MemoryStream mis = new MemoryStream( data ) ;
				return GetFiles_Private( mis, password ) ;
			}
			return null ;
		}

		/// <summary>
		/// Ｚｉｐ形式で圧縮された環境パスのファイル内に含まれるファイル名一覧を取得する
		/// </summary>
		/// <returns></returns>
		public static ( string, long )[] GetFiles( string path, string password = null )
		{
			FileStream fis = File.OpenRead( path ) ;
			if( fis != null )
			{
				return GetFiles_Private( fis, password ) ;
			}
			return null ;
		}

		private static ( string, long )[] GetFiles_Private( Stream cis, string password = null )
		{
			List<( string, long )> files = new List<( string, long )>() ;

			ZipInputStream zis = new ZipInputStream( cis ) ;

			if( string.IsNullOrEmpty( password ) == false )
			{
				zis.Password = password ;
			}

			//----------------------------------

			ZipEntry entry ;
			
			do
			{
				// 対象ファイルを探すループ
				entry = zis.GetNextEntry() ;
				if( entry != null && entry.IsFile == true )
				{
					files.Add( ( entry.Name, entry.Size ) ) ;
				}
			}
			while( entry != null ) ;

			zis.Close() ;

			cis.Close() ;

			if( files.Count != 0 )
			{
				return files.ToArray() ;
			}
			else
			{
				return null ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// Ｚｉｐ形式で圧縮されたバイト配列から任意のファイルのバイト配列を取得する(同期)
		/// </summary>
		/// <param name="data"></param>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static byte[] Decompress( byte[] data, string name, string password = null )
		{
			if( data != null && data.Length >  0 )
			{
				MemoryStream mis = new MemoryStream( data ) ;
				return Decompress_Private( mis, name, password ) ;
			}
			return null ;
		}

		/// <summary>
		/// Ｚｉｐ形式で圧縮された環境パスのファイルから任意のファイルのバイト配列を取得する(同期)
		/// </summary>
		/// <param name="data"></param>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static byte[] Decompress( string path, string name, string password = null )
		{
			FileStream fis = File.OpenRead( path ) ;
			if( fis != null )
			{
				return Decompress_Private( fis, name, password ) ;
			}
			return null ;
		}

		private static byte[] Decompress_Private( Stream cis, string name, string password = null )
		{
			byte[] result = null ;

			ZipInputStream zis = new ZipInputStream( cis ) ;

			MemoryStream mos ;

			if( string.IsNullOrEmpty( password ) == false )
			{
				zis.Password = password ;
			}

			//----------------------------------

			ZipEntry entry ;
			
			byte[] buffer = new byte[ BufferSize ] ;
			int length ;

			do
			{
				// 対象ファイルを探すループ
				entry = zis.GetNextEntry() ;
				if( entry != null && entry.IsFile == true && entry.Name == name )
				{
					// 発見
					mos = new MemoryStream() ;

					do
					{
						length = zis.Read( buffer, 0, buffer.Length ) ;
						if( length >  0 )
						{
							mos.Write( buffer, 0, length ) ;
						}
					}
					while( length >  0 ) ;

					result = mos.ToArray() ;

					mos.Close() ;
					break ;
				}
			}
			while( entry != null ) ;

			zis.Close() ;

			cis.Close() ;

			return result ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// Ｚｉｐ形式で圧縮されたバイト配列から任意のファイルのバイト配列を取得する(非同期)
		/// </summary>
		/// <param name="data"></param>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static async UniTask<byte[]> DecompressAsync( byte[] data, string name, Action<byte[]> onAction = null, string password = null )
		{
			if( data != null && data.Length >  0 )
			{
				MemoryStream mis = new MemoryStream( data ) ;
				return await DecompressAsync_Private( mis, name, onAction, password ) ;
			}
			return null ;
		}

		/// <summary>
		/// Ｚｉｐ形式で圧縮された環境パスのファィルから任意のファイルのバイト配列を取得する(非同期)
		/// </summary>
		/// <param name="data"></param>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static async UniTask<byte[]> DecompressAsync( string path, string name, Action<byte[]> onAction = null, string password = null )
		{
			FileStream fis = File.OpenRead( path ) ;
			if( fis != null )
			{
				return await DecompressAsync_Private( fis, name, onAction, password ) ;
			}
			return null ;
		}

		private static async UniTask<byte[]> DecompressAsync_Private( Stream cis, string name, Action<byte[]> onAction, string password )
		{
			byte[] result = null ;

			ZipInputStream zis = new ZipInputStream( cis ) ;

			MemoryStream mos ;

			if( string.IsNullOrEmpty( password ) == false )
			{
				zis.Password = password ;
			}

			//----------------------------------

			ZipEntry entry ;
			
			byte[] buffer = new byte[ BufferSize ] ;
			int length ;

			do
			{
				// 対象ファイルを探すループ
				entry = zis.GetNextEntry() ;
				if( entry != null && entry.IsFile == true && entry.Name == name )
				{
					// 発見
					mos = new MemoryStream() ;

					do
					{
						length = zis.Read( buffer, 0, buffer.Length ) ;
						if( length >  0 )
						{
							mos.Write( buffer, 0, length ) ;
							await m_Instance.Yield() ;
						}
					}
					while( length >  0 ) ;

					result = mos.ToArray() ;

					mos.Close() ;
					break ;
				}
			}
			while( entry != null ) ;

			zis.Close() ;

			cis.Close() ;

			if( result != null )
			{
				onAction?.Invoke( result ) ;
				return result ;
			}
			else
			{
				Debug.LogWarning( "Decompress Failed" ) ;
				return null ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// Ｚｉｐ形式で圧縮されたバイト配列から全てのファイルのバイト配列を取得する(同期)
		/// </summary>
		/// <param name="data"></param>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static ( string, byte[] )[] DecompressAll( byte[] data, string password = null )
		{
			if( data != null && data.Length >  0 )
			{
				MemoryStream mis = new MemoryStream( data ) ;
	
				return DecompressAll_Private( mis, password ) ;
			}
			return null ;
		}

		/// <summary>
		/// Ｚｉｐ形式で圧縮された環境パスのファイルから任意のファイルのバイト配列を取得する(同期)
		/// </summary>
		/// <param name="data"></param>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static ( string, byte[] )[] DecompressAll( string path, string password = null )
		{
			FileStream fis = File.OpenRead( path ) ;
			if( fis != null )
			{
				return DecompressAll_Private( fis, password ) ;
			}
			return null ;
		}

		private static ( string, byte[] )[] DecompressAll_Private( Stream cis, string password = null )
		{
			List<( string, byte[] )> resultAll = new List<( string, byte[] )>() ;
			byte[] result ;

			ZipInputStream zis = new ZipInputStream( cis ) ;

			MemoryStream mos ;

			if( string.IsNullOrEmpty( password ) == false )
			{
				zis.Password = password ;
			}

			//----------------------------------

			ZipEntry entry ;
			
			byte[] buffer = new byte[ BufferSize ] ;
			int length ;

			do
			{
				// 対象ファイルを探すループ
				entry = zis.GetNextEntry() ;
				if( entry != null && entry.IsFile == true )
				{
					// 発見
					mos = new MemoryStream() ;

					do
					{
						length = zis.Read( buffer, 0, buffer.Length ) ;
						if( length >  0 )
						{
							mos.Write( buffer, 0, length ) ;
						}
					}
					while( length >  0 ) ;

					result = mos.ToArray() ;
					resultAll.Add( ( entry.Name, result ) ) ;

					mos.Close() ;
					break ;
				}
			}
			while( entry != null ) ;

			zis.Close() ;

			cis.Close() ;

			if( resultAll.Count != 0 )
			{
				return resultAll.ToArray() ;
			}
			else
			{
				return null ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// Ｚｉｐ形式で圧縮されたバイト配列から全てのファイルのバイト配列を取得する(非同期)
		/// </summary>
		/// <param name="data"></param>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static async UniTask<( string, byte[] )[]> DecompressAllAsync( byte[] data, Action<( string, byte[] )[]> onAction = null, string password = null )
		{
			if( data != null && data.Length >  0 )
			{
				MemoryStream mis = new MemoryStream( data ) ;
				
				return await DecompressAllAsync_Private( mis, onAction, password ) ;
			}
			return null ;
		}

		/// <summary>
		/// Ｚｉｐ形式で圧縮された環境パスのファイルから全てのファイルのバイト配列を取得する(非同期)
		/// </summary>
		/// <param name="data"></param>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static async UniTask<( string, byte[] )[]> DecompressAllAsync( string path, Action<( string, byte[] )[]> onAction = null, string password = null )
		{
			FileStream fis = File.OpenRead( path ) ;
			if( fis != null )
			{
				return await DecompressAllAsync_Private( fis, onAction, password ) ;
			}
			return null ;
		}

		private static async UniTask<( string, byte[] )[]> DecompressAllAsync_Private( Stream cis, Action<( string, byte[] )[]> onAction, string password )
		{
			List<( string, byte[] )> resultAll = new List<( string, byte[] )>() ;
			byte[] result ;

			ZipInputStream zis = new ZipInputStream( cis ) ;

			MemoryStream mos ;

			if( string.IsNullOrEmpty( password ) == false )
			{
				zis.Password = password ;
			}

			//----------------------------------

			ZipEntry entry ;
			
			byte[] buffer = new byte[ BufferSize ] ;
			int length ;

			do
			{
				// 対象ファイルを探すループ
				entry = zis.GetNextEntry() ;
				if( entry != null && entry.IsFile == true )
				{
					// 発見
					mos = new MemoryStream() ;

					do
					{
						length = zis.Read( buffer, 0, buffer.Length ) ;
						if( length >  0 )
						{
							mos.Write( buffer, 0, length ) ;
							await m_Instance.Yield() ;
						}
					}
					while( length >  0 ) ;

					result = mos.ToArray() ;
					resultAll.Add( ( entry.Name, result ) ) ;

					mos.Close() ;
					break ;
				}
			}
			while( entry != null ) ;

			zis.Close() ;

			cis.Close() ;

			if( resultAll.Count >  0 )
			{
				onAction?.Invoke( resultAll.ToArray() ) ;
				return resultAll.ToArray() ;
			}
			else
			{
				Debug.LogWarning( "DecompressAll Failed" ) ;
				return null ;
			}
		}
		
		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// Ｚｉｐ形式で圧縮したバイナリ配列を生成する(ソースはバイト配列かファイルの環境パス)(同期)
		/// </summary>
		/// <param name="password"></param>
		/// <param name="sources"></param>
		/// <returns></returns>
		public static byte[] Compress( ( string name, System.Object data )[] sources, string password = null, int level = 3 )
		{
			if( sources == null || sources.Length == 0 )
			{
				return null ;	// 失敗
			}

			byte[] result ;

			MemoryStream mos = new MemoryStream() ;
			ZipOutputStream zos = new ZipOutputStream( mos ) ;

			if( string.IsNullOrEmpty( password ) == false )
			{
				// パスワード設定
				zos.Password = password ;
			}

			zos.SetLevel( level ) ;

			//----------------------------------------------------------

			ZipEntry entry ;
			string name ;
			byte[] data ;
			string path ;

			byte[] buffer = new byte[ BufferSize ] ;	// ファイル用のバッファ
			int length ;

			FileStream fis ;

			int i, l = sources.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( string.IsNullOrEmpty( sources[ i ].name ) == false && sources[ i ].data != null )
				{
					name = sources[ i ].name ;

					if( sources[ i ].data is byte[] )
					{
						// バイト配列
						data = sources[ i ].data as byte[] ;
						if( data.Length >  0 )
						{
							entry = new ZipEntry( name )
							{
								DateTime	= DateTime.Now,
								Size		= data.Length
							} ;
							zos.PutNextEntry( entry ) ;
					
							zos.Write( data, 0, data.Length ) ;

							zos.CloseEntry() ;
						}
					}
					else
					if( sources[ i ].data is string )
					{
						// パス
						path = sources[ i ].data as string ;
						fis = File.OpenRead( path ) ;
						if( fis != null )
						{
							entry = new ZipEntry( name )
							{
								DateTime	= DateTime.Now,
								Size		= fis.Length
							} ;
							zos.PutNextEntry( entry ) ;
					
							do
							{
								length = fis.Read( buffer, 0, buffer.Length ) ;
								if( length >  0 )
								{
									zos.Write( buffer, 0, length ) ;
								}
							}
							while( length >  0 ) ;

							fis.Close() ;

							zos.CloseEntry() ;
						}
					}
				}
			}

			//----------------------------------------------------------

			zos.Finish() ;
			zos.Close() ;

			result = mos.ToArray() ;

			mos.Close() ;

			return result ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// Ｚｉｐ形式で圧縮したバイナリ配列を生成する(ソースはバイト配列かファイルの環境パス)(非同期)
		/// </summary>
		/// <param name="password"></param>
		/// <param name="sources"></param>
		/// <returns></returns>
		public static async UniTask<byte[]> CompressAsync( ( string, System.Object )[] sources, Action<byte[]> onAction = null, string password = null, int level = 3 )
		{
			if( sources == null || sources.Length == 0 )
			{
				Debug.LogWarning( "Not found sources" ) ;
				return null ;
			}

			byte[] result ;

			MemoryStream mos = new MemoryStream() ;
			ZipOutputStream zos = new ZipOutputStream( mos ) ;

			if( string.IsNullOrEmpty( password ) == false )
			{
				// パスワード設定
				zos.Password = password ;
			}

			zos.SetLevel( level ) ;

			//----------------------------------------------------------

			ZipEntry entry ;
			string name ;
			byte[] data ;
			string path ;

			byte[] buffer = new byte[ BufferSize ] ;	// ファイル用のバッファ
			int length ;

			MemoryStream mis ;
			FileStream fis ;

			int i, l = sources.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( string.IsNullOrEmpty( sources[ i ].Item1 ) == false && sources[ i ].Item2 != null )
				{
					name = sources[ i ].Item1 ;

					if( sources[ i ].Item2 is byte[] )
					{
						// バイト配列
						data = sources[ i ].Item2 as byte[] ;
						if( data.Length >  0 )
						{
							entry = new ZipEntry( name )
							{
								DateTime	= DateTime.Now,
								Size		= data.Length
							} ;
							zos.PutNextEntry( entry ) ;
					
							mis = new MemoryStream( data ) ;

							do
							{
								length = mis.Read( buffer, 0, buffer.Length ) ;
								if( length >  0 )
								{
									zos.Write( buffer, 0, length ) ;
									await m_Instance.Yield() ;
								}
							}
							while( length >  0 ) ;

							mis.Close() ;

							zos.CloseEntry() ;
						}
					}
					else
					if( sources[ i ].Item2 is string )
					{
						// パス
						path = sources[ i ].Item2 as string ;
						fis = File.OpenRead( path ) ;
						if( fis != null )
						{
							entry = new ZipEntry( name )
							{
								DateTime	= DateTime.Now,
								Size		= fis.Length
							} ;
							zos.PutNextEntry( entry ) ;
					
							do
							{
								length = fis.Read( buffer, 0, buffer.Length ) ;
								if( length >  0 )
								{
									zos.Write( buffer, 0, length ) ;
									await m_Instance.Yield() ;
								}
							}
							while( length >  0 ) ;

							fis.Close() ;

							zos.CloseEntry() ;
						}
					}
				}
			}

			//----------------------------------------------------------

			zos.Finish() ;
			zos.Close() ;

			result = mos.ToArray() ;

			mos.Close() ;

			if( result != null )
			{
				onAction?.Invoke( result ) ;
				return result ;
			}
			else
			{
				Debug.LogWarning( "CompressAll Failed" ) ;
				return null ;
			}
		}
	}
}
