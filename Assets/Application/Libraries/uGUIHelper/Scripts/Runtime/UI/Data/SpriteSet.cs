using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

namespace uGUIHelper
{
	/// <summary>
	/// アトラスタイプのププライト管理用のクラス
	/// </summary>
	[Serializable]
	public class SpriteSet
	{
		[HideInInspector][SerializeField]
		private Texture2D m_Texture ;
		public  Texture2D   Texture
		{
			get
			{
				return m_Texture ;
			}
			set
			{
				m_Texture = value ;
			}
		}

		[HideInInspector][SerializeField]
		private string m_Path ;
		public  string   Path
		{
			get
			{
				return m_Path ;
			}
			set
			{
				m_Path = value ;
			}
		}

		// 個々のスプライト情報を保持する
		[SerializeField][HideInInspector]
		private List<Sprite> m_Sprites ;

		// 個々のスプライト情報を保持する
		[SerializeField][HideInInspector]
		private List<string> m_Names ;

		//-----------------------------------------------------------

		/// <summary>
		/// スプライト情報の展開の有無
		/// </summary>
		public bool Exist
		{
			get
			{
				if( m_Sprites != null && m_Sprites.Count >  0 )
				{
					return true ;
				}

				return false ;
			}
		}

		/// <summary>
		/// アトラスタイプのスプライトを生成する
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static SpriteSet Create( string path = null )
		{
//			UIAtlasSprite atlasSprite = ScriptableObject.CreateInstance<UIAtlasSprite>() ;

			SpriteSet spriteSet = new SpriteSet() ;

			if( string.IsNullOrEmpty( path ) == false )
			{
				if( spriteSet.Load( path ) == false )
				{
//					DestroyImmediate( atlasSprite ) ;
					return null ;
				}
			}

			return spriteSet ;
		}

		/// <summary>
		/// 指定したパスからアトラスタイプのスプライトをロードする
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private bool Load( string path = "" )
		{
			if( string.IsNullOrEmpty( path ) == true )
			{
				m_Path = path ;
			}

			Sprite[] sprites = Resources.LoadAll<Sprite>( path ) ;
			if( sprites == null )
			{
				return false ;
			}

			if( sprites.Length == 0 )
			{
				return false ;
			}

			SetSprites( sprites ) ;

			return true ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// アトラスタイプのスプライトをセットする
		/// </summary>
		/// <param name="spriteList"></param>
		public void SetSprites( Sprite[] sprites )
		{
			m_Sprites = new List<Sprite>() ;
			m_Names = new List<string>() ;

			int i, l = sprites.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				m_Sprites.Add( sprites[ i ] ) ;
				m_Names.Add( sprites[ i ].name ) ;
			}

			// テクスチャはどこも共通
			m_Texture = sprites[ 0 ].texture ;
		}

		/// <summary>
		/// 全ての有効なスプライトを取得する
		/// </summary>
		/// <returns></returns>
		public Sprite[] GetSprites()
		{
			if( m_Sprites == null || m_Sprites.Count == 0 )
			{
				return null ;
			}

			int i, l = m_Sprites.Count ;

			List<Sprite> sprites = new List<Sprite>() ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_Sprites[ i ] != null )
				{
					sprites.Add( m_Sprites[ i ] ) ;
				}
				else
				{
					// 異常発生
				}
			}

			return sprites.ToArray() ;
		}

		/// <summary>
		/// アトラスタイプのスプライト情報をクリアする
		/// </summary>
		public void ClearSprites()
		{
			m_Texture	= null ;
			m_Sprites	= null ;
			m_Names		= null ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 指定した名前のスプライトを取得する
		/// </summary>
		/// <param name="spriteName"></param>
		/// <returns></returns>
		public Sprite GetSprite( string spriteName )
		{
			if( m_Sprites == null || m_Sprites.Count == 0 )
			{
				return null ;
			}

			int i, l = m_Sprites.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_Sprites[ i ] != null && m_Sprites[ i ].name == spriteName )
				{
					return m_Sprites[ i ] ;
				}
			}

			return null ;

		}

		/// <summary>
		/// インデクサを使ってアクセスを簡易化
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public Sprite this[ string spriteName ]
		{
			get
			{
				return GetSprite( spriteName ) ;
			}
		}

		/// <summary>
		/// アトラスタイプのスプライトの名前リストを取得する
		/// </summary>
		/// <returns></returns>
		public string[] GetSpriteNames()
		{
			if( m_Sprites == null || m_Sprites.Count == 0 )
			{
				return null ;
			}

			int i, l = m_Sprites.Count ;

			List<string> names = new List<string>() ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_Sprites[ i ] != null )
				{
					names.Add( m_Sprites[ i ].name ) ;
				}
				else
				{
					// 異常発生
				}
			}

			return names.ToArray() ;
		}

		/// <summary>
		/// 要素の数を返す
		/// </summary>
		/// <returns></returns>
		public int SpriteCount
		{
			get
			{
				if( m_Sprites == null )
				{
					return 0 ;
				}
	
				return m_Sprites.Count ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// データに異常が無く使用可能な状態にあるか
		/// </summary>
		public bool IsAvailable
		{
			get
			{
				if( m_Sprites == null || m_Names == null )
				{
					return false ;
				}

				if( m_Sprites.Count != m_Names.Count )
				{
					return false ;
				}

				int i, l = m_Sprites.Count ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					if( m_Sprites[ i ] == null || m_Names[ i ] == null )
					{
						return false ;
					}

					if( m_Sprites[ i ].name != m_Names[ i ] )
					{
						return false ;
					}
				}

				return true ;
			}
		}
	}
}
