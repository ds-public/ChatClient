using UnityEngine ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

namespace uGUIHelper
{
	/// <summary>
	/// パラパラアニメの情報を保持する Version 2020/04/14
	/// </summary>
	[CreateAssetMenu( fileName = "UISpriteAnimation", menuName = "ScriptableObject/uGUIHelper/UISpriteAnimation" )]
	public class UISpriteAnimation : ScriptableObject
	{
		[SerializeField]
		protected Texture m_Texture ;
		public    Texture   Texture{ get{ return m_Texture ; } set{ m_Texture = value ; } }

		[SerializeField]
		protected List<Sprite> m_Sprites = null ;
		
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
		/// タイムスケール
		/// </summary>
		[SerializeField]
		protected float m_TimeScale = 1.0f ;
		public    float   TimeScale{ get{ return m_TimeScale ; } set{ m_TimeScale = value ; } }

		/// <summary>
		/// アトラスタイプのスプライトをセットする
		/// </summary>
		/// <param name="sprites"></param>
		public void SetSprites( Sprite[] sprites )
		{
			if( sprites == null || sprites.Length == 0 )
			{
				ClearSprite() ;
				return ;
			}

			m_Sprites = new List<Sprite>() ;
			m_Sprites.AddRange( sprites ) ;

			// 名前でソートする
			m_Sprites.Sort( ( a, b ) => { return string.Compare( a.name, b.name ) ; } ) ;

			// テクスチャはどこも共通
			m_Texture = sprites[ 0 ].texture ;
		}

		/// <summary>
		/// スプライトを取得する
		/// </summary>
		/// <param name="spriteName"></param>
		/// <returns></returns>
		public Sprite GetSprite( string spriteName )
		{
			if( m_Sprites == null )
			{
				return null ;
			}

			return m_Sprites.FirstOrDefault( _ => _.name == spriteName ) ;
		}

		/// <summary>
		/// スプライトを取得する
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public Sprite GetSprite( int index )
		{
			if( m_Sprites == null )
			{
				return null ;
			}

			if( index <  0 || index >= m_Sprites.Count )
			{
				return null ;
			}

			return m_Sprites[ index ] ;
		}


		/// <summary>
		/// 指定のフレームのスプライトインデックス番号を取得する
		/// </summary>
		public Sprite GetSpriteOfFrame( int frameIndex )
		{
			if( m_Sprites == null )
			{
				return null ;
			}

			if( frameIndex <  0 || frameIndex >= m_Frames.Count )
			{
				return null ;
			}

			int spriteIndex = m_Frames[ frameIndex ].SpriteIndex ;

			if( spriteIndex <  0 || spriteIndex >= m_Sprites.Count )
			{
				return null ;
			}

			return m_Sprites[ spriteIndex ] ;
		}

		/// <summary>
		/// 全てのスプライト情報を取得する
		/// </summary>
		/// <returns></returns>
		public Sprite[] GetAllSprites()
		{
			if( m_Sprites == null )
			{
				return null ;
			}

			return m_Sprites.ToArray() ;
		}

		/// <summary>
		/// スプライトの数を取得する
		/// </summary>
		/// <returns></returns>
		public int GetSpriteCount()
		{
			if( m_Sprites == null )
			{
				return 0 ;
			}

			return m_Sprites.Count ;
		}

		/// <summary>
		/// アトラスタイプのスプライト情報をクリアする
		/// </summary>
		public void ClearSprite()
		{
			m_Sprites = null ;
		}

		/// <summary>
		/// アトラスタイプのスプライトの名前リストを取得する
		/// </summary>
		/// <returns></returns>
		public string[] GetAllSpriteNames()
		{
			if( m_Sprites == null || m_Sprites.Count == 0 )
			{
				return null ;
			}

			int i, l = m_Sprites.Count ;
			string[] spriteNames = new string[ l ] ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				spriteNames[ i ] = m_Sprites[ i ].name ;
			}

			return spriteNames ;
		}

		// フレーム管理クラス
		[System.Serializable]
		public class FrameData
		{
			public int		SpriteIndex ;
			public float	Duration ;

			public FrameData( int spriteIndex, float duration )
			{
				SpriteIndex	= spriteIndex ;
				Duration	= duration ;
			}
		}

		[HideInInspector][SerializeField]
		protected List<FrameData> m_Frames = new List<FrameData>() ;
		public  List<FrameData>     Frames
		{
			get
			{
				return m_Frames ;
			}
		}

		// インデクサを使って連想配列っぽいことをやる
		public FrameData this[ int frameIndex ]
		{
			get
			{
				if( m_Frames == null )
				{
					return null ;
				}

				if( frameIndex <  0 || frameIndex >= m_Frames.Count )
				{
					return null ;
				}

				return m_Frames[ frameIndex ] ;
			}
			set
			{
				if( m_Frames == null )
				{
					return ;
				}

				if( frameIndex <  0 || frameIndex >= m_Frames.Count )
				{
					return ;
				}

				m_Frames[ frameIndex ] = value ;
			}
		}

		/// <summary>
		/// フレームを挿入および追加
		/// </summary>
		/// <param name="frameIndex"></param>
		/// <param name="spriteIndex"></param>
		/// <param name="duration"></param>
		/// <returns></returns>
		public bool Insert( int frameIndex, int spriteIndex, float duration )
		{
			if( frameIndex <  0 || m_Sprites == null || spriteIndex <  0 || spriteIndex >= m_Sprites.Count || duration <  0 )
			{
				return false ;
			}

			if( frameIndex <  m_Frames.Count )
			{
				m_Frames.Insert( frameIndex, new FrameData( spriteIndex, duration ) ) ;
			}
			else
			{
				m_Frames.Add( new FrameData( spriteIndex, duration ) ) ;
			}

			return true ;
		}

		/// <summary>
		/// フレームをまとめて削除
		/// </summary>
		/// <param name="frameIndex"></param>
		public void Remove( params int[] frameIndices )
		{
			if( frameIndices == null || frameIndices.Length == 0 )
			{
				return ;
			}

			int i, l = frameIndices.Length ;
			List<FrameData> frames = new List<FrameData>() ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				frames.Add( m_Frames[ frameIndices[ i ] ] ) ;
			}

			if( frames.Count >  0 )
			{
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( m_Frames.Contains( frames[ i ] ) == true )
					{
						m_Frames.Remove( frames[ i ] ) ;
					}
				}
			}
		}

		/// <summary>
		/// フレーム数を返す
		/// </summary>
		public int Length
		{
			get
			{
				if( m_Frames == null )
				{
					return 0 ;
				}
				return m_Frames.Count ;
			}
		}
	}
}
