using System ;
using System.IO ;
using System.Text ;
using System.Security.Cryptography ;

namespace Template
{
	/// <summary>
	/// セキュリティ関連 Version 2022/01/30
	/// </summary>
	public partial class Security
	{
		/// <summary>
		/// バイト配列を暗号化する
		/// </summary>
		/// <param name="originalData"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		public static byte[] Encrypt( byte[] originalData, string password )
		{
			string hash, key, vector ;

			if( password.Length >= 64 )
			{
				// 64文字以上なら最初の64文字を使用する
				hash = password ;
			}
			else
			{
				// 64文字未満ならハッシュ化して最初の64文字を使用する
				hash = GetHashValue( password ) ;
			}

			key			= hash.Substring(  0, 32 ) ;
			vector		= hash.Substring( 32, 32 ) ;

			return Encrypt( originalData, key, vector ) ;
		}

		/// <summary>
		/// バイト配列を暗号化する
		/// </summary>
		/// <param name="originalData">暗号化前のバイト配列</param>
		/// <param name="key">暗号化キー(32文字以下)</param>
		/// <param name="vector">暗号化ベクター(32文字以上)</param>
		/// <returns>暗号化後のバイト配列</returns>
		public static byte[] Encrypt( byte[] originalData, string key, string vector )
		{
			// オリジナルのサイズがわからなくなるので保存する
			byte[] data = new byte[ 4 + originalData.Length ] ;
			long size = originalData.Length ;
		
			data[ 0 ] = ( byte )( ( size >>  0 ) & 0xFF ) ;
			data[ 1 ] = ( byte )( ( size >>  8 ) & 0xFF ) ;
			data[ 2 ] = ( byte )( ( size >> 16 ) & 0xFF ) ;
			data[ 3 ] = ( byte )( ( size >> 24 ) & 0xFF ) ;
	
			Array.Copy( originalData, 0, data, 4, size ) ;
		
			//-----------------------------------------------------
			// 暗号化用の種別オブジェクト生成

			// 少し弱いらしいので使わない
//			TripleDESCryptoServiceProvider kind = new TripleDESCryptoServiceProvider() ;

			// こちらを使う
			RijndaelManaged kind = new RijndaelManaged()
			{
				Padding = PaddingMode.Zeros,
				Mode = CipherMode.CBC,
				KeySize   = 256,
				BlockSize = 256
			} ;

			//-----------------------------------------------------
			// 暗号用のキー情報をセットする

			byte[] aKey    = Encoding.UTF8.GetBytes( key    ) ;	// 32バイト以下

			if( aKey.Length >  32 )
			{
				byte[] aKeyWork = new byte[ 32 ] ;
				Array.Copy( aKey, aKeyWork, 32 ) ;
				aKey = aKeyWork ;
			}

			byte[] aVector = Encoding.UTF8.GetBytes( vector ) ;	// 32バイト以上

			if( aVector.Length <  32 )
			{
				byte[] aVectorWork = new byte[ 32 ] ;
				Array.Copy( aVector, aVectorWork, aVector.Length ) ;
				aVector = aVectorWork ;
			}

			//--------------

			ICryptoTransform encryptor = kind.CreateEncryptor( aKey, aVector ) ;
		
			//-----------------------------------------------------
		
			MemoryStream memoryStream = new MemoryStream() ;

			// 暗号化
			CryptoStream cryptoStream = new CryptoStream( memoryStream, encryptor, CryptoStreamMode.Write ) ;
		
			cryptoStream.Write( data, 0, data.Length ) ;
			cryptoStream.FlushFinalBlock() ;
		
			cryptoStream.Close() ;
		
			byte[] cryptoData = memoryStream.ToArray() ;
		
			memoryStream.Close() ;
		
			//-----------------------------------------------------
		
			encryptor.Dispose() ;
		
			kind.Clear() ;
			kind.Dispose() ;
		
			//-----------------------------------------------------
		
			return cryptoData ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// バイト配列を復号化する
		/// </summary>
		/// <param name="cryptoData"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		public static byte[] Decrypt( byte[] cryptoData, string password )
		{
			string hash, key, vector ;

			if( password.Length >= 64 )
			{
				// 64文字以上なら最初の64文字を使用する
				hash = password ;
			}
			else
			{
				// 64文字未満ならハッシュ化して最初の64文字を使用する
				hash = GetHashValue( password ) ;
			}

			key			= hash.Substring(  0, 32 ) ;
			vector		= hash.Substring( 32, 32 ) ;

			return Decrypt( cryptoData, key, vector ) ;
		}

		/// <summary>
		/// バイト配列を復号化する
		/// </summary>
		/// <param name="cryptoData">暗号化されたバイト配列</param>
		/// <param name="key">暗号化キー(32文字以下)</param>
		/// <param name="vector">暗号化ベクター(32文字以上)</param>
		/// <returns>復号化されたバイト配列</returns>
		public static byte[] Decrypt( byte[] cryptoData, string key, string vector )
		{
			//-----------------------------------------------------
			// 暗号化用の種別オブジェクト生成

			// 少し弱いらしいので使わない
//			TripleDESCryptoServiceProvider kind = new TripleDESCryptoServiceProvider() ;
			
			// こちらを使う
			RijndaelManaged kind = new RijndaelManaged()
			{
				Padding = PaddingMode.Zeros,
				Mode = CipherMode.CBC,
				KeySize   = 256,
				BlockSize = 256
			} ;

			//-----------------------------------------------------
			// 複合用のキー情報をセットする

			byte[] aKey    = Encoding.UTF8.GetBytes( key    ) ;	// 32バイト以下

			if( aKey.Length >  32 )
			{
				byte[] aKeyWork = new byte[ 32 ] ;
				Array.Copy( aKey, aKeyWork, 32 ) ;
				aKey = aKeyWork ;
			}

			byte[] aVector = Encoding.UTF8.GetBytes( vector ) ;	// 32バイト以上

			if( aVector.Length <  32 )
			{
				byte[] aVectorWork = new byte[ 32 ] ;
				Array.Copy( aVector, aVectorWork, aVector.Length ) ;
				aVector = aVectorWork ;
			}

			//--------------
		
			ICryptoTransform decryptor = kind.CreateDecryptor( aKey, aVector ) ;
		
			//-----------------------------------------------------
		
			byte[] data = new byte[ cryptoData.Length ] ;
		
			//-----------------------------------------------------
		
			MemoryStream memoryStream = new MemoryStream( cryptoData ) ;
		
			// 復号化
			CryptoStream cryptoStream = new CryptoStream( memoryStream, decryptor, CryptoStreamMode.Read ) ;
		
			cryptoStream.Read( data, 0, data.Length ) ;
			cryptoStream.Close() ;
		
			memoryStream.Close() ;
		
			//-----------------------------------------------------
		
			decryptor.Dispose() ;
		
			kind.Clear() ;
			kind.Dispose() ;
		
			//-----------------------------------------------------
		
			long size = ( ( long )data[ 0 ] <<  0 ) | ( ( long )data[ 1 ] <<  8 ) | ( ( long )data[ 2 ] << 16 ) | ( ( long )data[ 3 ] ) ;
		
			byte[] originalData = new byte[ size ] ;
			Array.Copy( data, 4, originalData, 0, size ) ;
		
			return originalData ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ハッシュ値を取得する
		/// </summary>
		/// <param name="text"></param>
		public static string GetHashValue( string text )
		{
			byte[] data = Encoding.UTF8.GetBytes( text ) ;
			data = new SHA256CryptoServiceProvider().ComputeHash( data ) ;

			// バイト配列 → 16進数文字列
			var sb = new StringBuilder();
			foreach( byte code in data )
			{
				sb.Append( code.ToString( "x2" ) ) ;
			}

			// 64文字のハッシュ値
			return sb.ToString() ;
		}

		//-------------------------------------------------------------------------------------------

		//---------------------------------------------------------------------------
		// AES128

		// AES128で暗号化
		public static string EncryptDataToBase64( byte[] data, byte[] key, byte[] vector )
		{
			return Convert.ToBase64String( Encrypt( data, key, vector) ) ;
		}

		// AES128で暗号化
		public static string EncryptWordToBase64( string text, byte[] key, byte[] vector )
		{
			return Convert.ToBase64String( Encrypt( Encoding.UTF8.GetBytes( text ), key, vector ) ) ;
		}

		public static byte[] EncryptWord( string text, byte[] key, byte[] vector )
		{
			return Encrypt( Encoding.UTF8.GetBytes( text ), key, vector ) ;
		}

		// AES128で暗号化
		public static byte[] Encrypt( byte[] data, byte[] key, byte[] vector )
		{
//			RijndaelManaged aes = new RijndaelManaged() ;
			AesManaged aes	= new AesManaged()
			{
				BlockSize	= 16 * 8,
				KeySize		= key.Length * 8,
				Mode		= CipherMode.CBC,
				Padding		= PaddingMode.PKCS7,
				Key			= key,
				IV			= vector	// GenerateIV() を使ってはダメ。毎回値が変わる。
			} ;
			
//			Debug.LogWarning( "Data:" + PrintBytes( tData ) ) ;

			//----------------------------------

			ICryptoTransform encryptor = aes.CreateEncryptor( aes.Key, aes.IV ) ;

			byte[] encodedData = encryptor.TransformFinalBlock( data, 0, data.Length ) ;

			encryptor.Dispose() ;

			aes.Dispose() ;

			return encodedData ;
		}

		//-----------------------------------

		// AES128で復号化
		public static byte[] DecryptDataFromBase64( string text, byte[] key, byte[] vector )
		{
			return Decrypt( key, vector, Convert.FromBase64String( text ) ) ;
		}

		// AES128で復号化
		public static string DecryptWordFromBase64( string text, byte[] key, byte[] vector )
		{
			return Encoding.UTF8.GetString( Decrypt( Convert.FromBase64String( text ), key, vector ) ) ;
		}

		// AES128で復号化
		public static string DecryptWord( byte[] data, byte[] key, byte[] vector )
		{
			return Encoding.UTF8.GetString( Decrypt( data, key, vector ) ) ;
		}

		// AES128で復号化
		public static byte[] Decrypt( byte[] data, byte[] key, byte[] vector )
		{
//			RijndaelManaged aes = new RijndaelManaged() ;
			AesManaged aes	= new AesManaged()
			{
				BlockSize	= 16 * 8,
				KeySize		= key.Length * 8,
				Mode		= CipherMode.CBC,
				Padding		= PaddingMode.PKCS7,
				Key			= key,
				IV			= vector	// GenerateIV() を使ってはダメ。毎回値が変わる。
			} ;
			
			//----------------------------------

			ICryptoTransform decrypter = aes.CreateDecryptor( aes.Key, aes.IV ) ;

			byte[] decodedData = decrypter.TransformFinalBlock( data, 0, data.Length ) ;

			decrypter.Dispose() ;

			aes.Dispose() ;

			return decodedData ;
		}

		//-----------------------------------------------------------

		// Base64 文字列をバイト配列に変換する
		public static byte[] DecodeBase64( string data )
		{
			return Convert.FromBase64String( data ) ;
		}

		// バイト配列を Base64 文字列に変換する
		public static string EncodeBase64( byte[] data )
		{
			return Convert.ToBase64String( data ) ;
		}

		//-------------------------------------------------------------------------------------------

		// XOR
		public static byte[] Xor( byte[] data, byte key, bool clone = false )
		{
			byte[] clonedData = data ;
			if( clone == true )
			{
				clonedData = new byte[ data.Length ] ;
			}

			for( int i  = 0 ; i <  data.Length ; i ++ )
			{
				byte p = data[ i ] ;
				clonedData[ i ] = ( byte )( p ^ key ) ;
			}

			return clonedData ;
		}

		//-------------------------------------------------------------------------------------------

		// ハッシュ生成インスタンス
		//		private static MD5CryptoServiceProvider mHashGenerator = new MD5CryptoServiceProvider() ;
		private static readonly HMACSHA256 m_HashGenerator = new HMACSHA256( new byte[]{ 0, 1, 2, 3 } ) ;	// コンストラクタに適当なキー値を入れる事(でないと毎回ランダムになってしまう)

		// ハッシュコードを計算する
		public static string GetHash( string fileName )
		{
			if( string.IsNullOrEmpty( fileName ) == true )
			{
				return "" ;
			}

			byte[] data = Encoding.UTF8.GetBytes( fileName ) ;
			return GetHash( data ) ;
		}

		// ハッシュコードを計算する
		public static string GetHash( byte[] data )
		{
			byte[] hash = m_HashGenerator.ComputeHash( data ) ;

			string text = "" ;
			foreach( var code in hash )
			{
				text += code.ToString( "x2" ) ;
			}

			return text ;
		}
	}
}
