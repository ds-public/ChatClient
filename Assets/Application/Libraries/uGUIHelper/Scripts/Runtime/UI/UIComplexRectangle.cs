using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.Events ;
using UnityEngine.EventSystems ;
using System ;
using System.Collections ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// uGUI:ComplxRectangle クラスの機能拡張コンポーネントクラス
	/// </summary>
	[ RequireComponent( typeof( ComplexRectangle ) ) ]
	public class UIComplexRectangle : UIView
	{
		/// <summary>
		/// オフセット(ショートカット)
		/// </summary>
		public Vector2 Offset
		{
			get
			{
				ComplexRectangle complexRectangle = CComplexRectangle ;
				if( complexRectangle == null )
				{
					return Vector2.zero ;
				}
				return complexRectangle.offset ;
			}
			set
			{
				ComplexRectangle complexRectangle = CComplexRectangle ;
				if( complexRectangle == null )
				{
					return ;
				}
				complexRectangle.offset = value ;
			}
		}
	



		/// <summary>
		/// RectTransform のサイズを自動的に文字列のサイズに合わせるかどうか
		/// </summary>
//		public bool autoSizeFitting = true ;

		/// <summary>
		/// テクスチャ(ショートカット)
		/// </summary>
		public Texture2D Texture
		{
			get
			{
				ComplexRectangle complexRectangle = CComplexRectangle ;
				if( complexRectangle == null )
				{
					return null ;
				}
				return complexRectangle.texture ;
			}
			set
			{
				ComplexRectangle complexRectangle = CComplexRectangle ;
				if( complexRectangle == null )
				{
					return ;
				}
				complexRectangle.texture = value ;
			}
		}
	
		/// <summary>
		/// カラー(ショートカット)
		/// </summary>
		public Color Color
		{
			get
			{
				ComplexRectangle complexRectangle = CComplexRectangle ;
				if( complexRectangle == null )
				{
					return Color.white ;
				}
				return complexRectangle.Color ;
			}
			set
			{
				ComplexRectangle tComplexRectangle = CComplexRectangle ;
				if( tComplexRectangle == null )
				{
					return ;
				}
				tComplexRectangle.Color = value ;
			}
		}
		
		/// <summary>
		/// マテリアル(ショートカット)
		/// </summary>
		public Material Material
		{
			get
			{
				ComplexRectangle complexRectangle = CComplexRectangle ;
				if( complexRectangle == null )
				{
					return null ;
				}
				return complexRectangle.material ;
			}
			set
			{
				ComplexRectangle complexRectangle = CComplexRectangle ;
				if( complexRectangle == null )
				{
					return ;
				}
				complexRectangle.material = value ;
			}
		}
	
		//-----------------------------------------------------------

		[SerializeField][HideInInspector]
		private SpriteSet m_AtlasSprite = null ;

		/// <summary>
		/// スプライトセットのインスタンス
		/// </summary>
		public  SpriteSet  SpriteSet
		{
			get
			{
				return m_AtlasSprite ;
			}
			set
			{
				if( m_AtlasSprite != value )
				{
					m_AtlasSprite  = value ;
				}

				m_AtlasSprite = null ;
			}
		}


		/// <summary>
		/// アトラススプライト内のスプライトを取得する
		/// </summary>
		/// <param name="tName">スプライト名</param>
		/// <returns>スプライトのインスタンス</returns>
		public Sprite GetSpriteInAtlas( string spriteName )
		{
			if( m_AtlasSprite == null || m_AtlasSprite.SpriteCount == 0 )
			{
				return null ;
			}

			return m_AtlasSprite[ spriteName ] ;
		}

		/// <summary>
		/// アトラススプライト内のスプライトの横幅を取得する
		/// </summary>
		/// <param name="tName">スプライト名</param>
		/// <returns>横幅</returns>
		public int GetWidthOfSpriteInAtlas( string spriteName )
		{
			if( m_AtlasSprite == null || m_AtlasSprite.SpriteCount == 0 )
			{
				return 0 ;
			}

			if( m_AtlasSprite[ spriteName ] == null )
			{
				return 0 ;
			}

			return ( int )m_AtlasSprite[ spriteName ].rect.width ;
		}

		/// <summary>
		/// アトラススプライト内のスプライトの縦幅を取得する
		/// </summary>
		/// <param name="tName">スプライト名</param>
		/// <returns>縦幅</returns>
		public int GetHeightOfSpriteInAtlas( string spriteName )
		{
			if( m_AtlasSprite == null || m_AtlasSprite.SpriteCount == 0 )
			{
				return 0 ;
			}

			if( m_AtlasSprite[ spriteName ] == null )
			{
				return 0 ;
			}

			return ( int )m_AtlasSprite[ spriteName ].rect.height ;
		}
		
		//-----------------------------------------------------------

		/// <summary>
		/// 矩形を追加する
		/// </summary>
		/// <param name="tName"></param>
		/// <param name="tOffset"></param>
		/// <param name="tPivot"></param>
		/// <param name="tSize"></param>
		/// <param name="tRotation"></param>
		/// <param name="tColor"></param>
		/// <param name="tUV"></param>
		/// <param name="tPriority"></param>
		public void AddRectangle( string rectName, Vector2 offset, Vector2 pivot, Vector2 size, float rotation, Color color, Rect uv, int priority )
		{
			ComplexRectangle complexRectangle = CComplexRectangle ;
			if( complexRectangle == null )
			{
				return ;
			}

			complexRectangle.AddRectangle( rectName, offset, pivot, size, rotation, color, uv, priority ) ;
		}

		/// <summary>
		/// 矩形を追加する
		/// </summary>
		/// <param name="tName"></param>
		/// <param name="tOffset"></param>
		/// <param name="tPivot"></param>
		/// <param name="tSize"></param>
		/// <param name="tRotation"></param>
		/// <param name="tColor"></param>
		/// <param name="tUV"></param>
		/// <param name="tPriority"></param>
		public void AddRectangle( string rectName, Vector2 offset, Vector2 pivot, Vector2 size, float rotation, Color color, string spriteName, int priority )
		{
			ComplexRectangle complexRectangle = CComplexRectangle ;
			if( complexRectangle == null || m_AtlasSprite == null || string.IsNullOrEmpty( spriteName ) == true || m_AtlasSprite[ spriteName ] == null )
			{
				return ;
			}

			Rect uv = m_AtlasSprite[ spriteName ].textureRect ;

			complexRectangle.AddRectangle( rectName, offset, pivot, size, rotation, color, uv, priority ) ;
		}

		/// <summary>
		/// 矩形を追加する
		/// </summary>
		/// <param name="tName"></param>
		/// <param name="tOffset"></param>
		/// <param name="tPivot"></param>
		/// <param name="tSize"></param>
		/// <param name="tRotation"></param>
		/// <param name="tColor"></param>
		/// <param name="tUV"></param>
		/// <param name="tPriority"></param>
		public void AddRectangle( string rectName, float offsetX, float offsetY, float pivotX, float pivotY, float sizeX, float sizeY, Color color, string spriteName, float padding, int priority )
		{
			ComplexRectangle complexRectangle = CComplexRectangle ;
			if( complexRectangle == null || m_AtlasSprite == null || string.IsNullOrEmpty( spriteName ) == true || m_AtlasSprite[ spriteName ] == null )
			{
				return ;
			}

			Vector2 offset = new Vector2( offsetX, offsetY ) ;
			Vector2 pivot = new Vector2( pivotX, pivotY ) ;

			Rect uv = m_AtlasSprite[ spriteName ].textureRect ;

//			Vector2 tSize = new Vector2( tUV.width, tUV.height ) ;
			Vector2 size = new Vector2( sizeX, sizeY ) ;

			// 座標をテクスチャ系に変換する
			float tw = m_AtlasSprite.Texture.width ;
			float th = m_AtlasSprite.Texture.height ;

			uv.xMin	= ( uv.xMin	+ padding )	/ tw ;
			uv.yMin	= ( uv.yMin	+ padding )	/ th ;
			uv.xMax	= ( uv.xMax	- padding )	/ tw ;
			uv.yMax	= ( uv.yMax	- padding )	/ th ;

//			Debug.LogWarning( "UV:" + tUV ) ;

			complexRectangle.AddRectangle( rectName, offset, pivot, size, 0, color, uv, priority ) ;
		}



		/// <summary>
		/// 矩形を削除する
		/// </summary>
		/// <param name="tName"></param>
		public void RemoveRectangle( string rectName )
		{
			ComplexRectangle complexRectangle = CComplexRectangle ;
			if( complexRectangle == null )
			{
				return ;
			}

			complexRectangle.RemoveRectangle( rectName ) ;
		}

		/// <summary>
		/// 矩形を全て削除する
		/// </summary>
		public void ClearRectangle()
		{
			ComplexRectangle complexRectangle = CComplexRectangle ;
			if( complexRectangle == null )
			{
				return ;
			}

			complexRectangle.ClearRectangle() ;
		}

		//----------------------------------------------------------------------

		/// <summary>
		/// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		/// </summary>
		/// <param name="tOption"></param>
		override protected void OnBuild( string option = "" )
		{
			ComplexRectangle complexRectangle = CComplexRectangle ;

			if( complexRectangle == null )
			{
				complexRectangle = gameObject.AddComponent<ComplexRectangle>() ;
			}
			if( complexRectangle == null )
			{
				// 異常
				return ;
			}

			//----------------------------

			complexRectangle.rectangle.Add( new ComplexRectangle.Rectangle() ) ;

			// Default
			complexRectangle.Color = Color.white ;

			ResetRectTransform() ;
		}

/*		override protected void OnLateUpdate()
		{
			if( autoSizeFitting == true )
			{
				Resize() ;
			}
		}

		private void Resize()
		{
			ComplexRectangle tComplexRectangle = CComplexRectangle ;
			RectTransform r = _rectTransform ;
			if( r != null && tComplexRectangle != null )
			{
				Vector2 tSize = r.sizeDelta ;

				tSize.x = tComplexRectangle.preferredWidth ;
				tSize.y = tComplexRectangle.preferredHeight ;

				r.sizeDelta = tSize ;
			}
		}*/

	}
}
