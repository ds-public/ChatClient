using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

//using UnityEngine.Serialization ;

using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.U2D ;

#if UNITY_EDITOR
using UnityEditor ;
#endif

namespace uGUIHelper
{
	/// <summary>
	/// uGUI:Image クラスの機能拡張コンポーネントクラス
	/// </summary>
	[ RequireComponent( typeof( UnityEngine.UI.Image ) ) ]
	public class UIImage : UIView
	{
		/// <summary>
		/// 動作有無のショートカット
		/// </summary>
		public bool Enabled
		{
			get
			{
				return CImage != null ? CImage.enabled : false ;
			}
			set
			{
				if( CImage != null )
				{
					CImage.enabled = value ;
				}
			}
		}

		//-------------------------------------------------------------------------------------------
		// SpriteAtlas 限定
		
		// スプライトアトラス(Unity標準機能)
		[SerializeField][HideInInspector]
		private SpriteAtlas m_SpriteAtlas = null ;

		/// <summary>
		/// アトラススプライトのインスタンス
		/// </summary>
		public  SpriteAtlas  SpriteAtlas
		{
			get
			{
				return m_SpriteAtlas ;
			}
			set
			{
				if( m_SpriteAtlas != value )
				{
					m_SpriteAtlas  = value ;

					//--------------------------------
					// 非実行中だと色々と問題が起こるので自動設定はしない(Cloneが設定されてしまう)
#if false
					if( m_SpriteAtlas != null && m_SpriteAtlas.spriteCount >  0 )
					{
						Sprite[] sprites = new Sprite[ m_SpriteAtlas.spriteCount ] ;
						m_SpriteAtlas.GetSprites( sprites ) ;

						if( Sprite != null )
						{
							int i, l = sprites.Length ;
							for( i  = 0 ; i <  l ; i ++ )
							{
								if( sprites[ i ] == Sprite )
								{
									break ;
								}
							}

							if( i >= l )
							{
								Sprite = sprites[ 0 ] ;
							}
						}
						else
						{
							Sprite = sprites[ 0 ] ;
						}
					}
#endif
				}
			}
		}

		//-------------------------------------------------------------------------------------------
		// SpriteSet 限定

//		[SerializeField][HideInInspector]
//		private SpriteSet m_AtlasSprite = null ;

//		[FormerlySerializedAs("m_AtlasSprite")]
		[SerializeField]
		private SpriteSet m_SpriteSet = null ;

		/// <summary>
		/// アトラススプライトのインスタンス
		/// </summary>
		public  SpriteSet  SpriteSet
		{
			get
			{
				return m_SpriteSet ;
			}
			set
			{
				if( m_SpriteSet != value )
				{
					m_SpriteSet  = value ;
#if false
					if( m_SpriteSet != null && m_SpriteSet.SpriteCount >  0 )
					{
						Sprite[] sprites = m_SpriteSet.GetSprites() ;

						if( Sprite != null )
						{
							int i, l = sprites.Length ;
							for( i  = 0 ; i <  l ; i ++ )
							{
								if( sprites[ i ] == Sprite )
								{
									break ;
								}
							}

							if( i >= l )
							{
								Sprite = sprites[ 0 ] ;
							}
						}
						else
						{
							Sprite = sprites[ 0 ] ;
						}
					}
#endif
				}
			}
		}

		/// <summary>
		/// アトラススプライトの要素となるスプライト群を設定する
		/// </summary>
		/// <param name="sprites"></param>
		/// <returns></returns>
		public bool SetSprites( Sprite[] sprites )
		{
			if( sprites == null || sprites.Length == 0 )
			{
				return false ;
			}

			//----------------------------------------------------------
			// SpriteSet

			if( m_SpriteSet == null )
			{
				m_SpriteSet = new SpriteSet() ;
			}
			else
			{
				m_SpriteSet.ClearSprites() ;
			}

			m_SpriteSet.SetSprites( sprites ) ;

			//----------------------------------------------------------

			return true ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// アトラステクスチャ内のスプライトの一覧を取得する
		/// </summary>
		/// <returns></returns>
		public Sprite[] GetSprites()
		{
			//----------------------------------------------------------
			// SpriteAtlas

			if( m_SpriteAtlas != null && m_SpriteAtlas.spriteCount >  0 )
			{
				Sprite[] sprites = new Sprite[ m_SpriteAtlas.spriteCount ] ;
				m_SpriteAtlas.GetSprites( sprites ) ;
				return sprites ;
			}

			//----------------------------------------------------------
			// SpriteAtlas

			if( m_SpriteSet != null && m_SpriteSet.SpriteCount >  0 )
			{
				Sprite[] sprites = m_SpriteSet.GetSprites() ;
				return sprites ;
			}

			//----------------------------------------------------------
			return null ;
		}

		/// <summary>
		/// アトラステクスチャ内のスプライトの名前一覧を取得する
		/// </summary>
		/// <returns></returns>
		public string[] GetSpriteNames()
		{
			//----------------------------------------------------------
			// SpriteAtlas

			if( m_SpriteAtlas != null )
			{
				Sprite[] sprites = GetSprites() ;
				if( sprites != null && sprites.Length >  0 )
				{
					string[] names = new string[ sprites.Length ] ;
					int i, l = sprites.Length ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						names[ i ] = sprites[ i ].name ;
					}

					return names ;
				}
			}

			//----------------------------------------------------------
			// SpriteSet

			if( m_SpriteSet != null )
			{
				string[] names = m_SpriteSet.GetSpriteNames() ;
				if( names != null && names.Length >  0 )
				{
					return names ;
				}
			}

			//----------------------------------------------------------

			return null ;
		}

		/// <summary>
		/// アトラススプライト内のスプライトを表示する
		/// </summary>
		/// <param name="spriteName">スプライト名</param>
		/// <param name="resize">画像のサイズに合わせてリサイズを行うかどうか</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public bool SetSpriteInAtlas( string spriteName, bool resize = false )
		{
			//----------------------------------------------------------
			// SpriteAtlas

			if( m_SpriteAtlas != null )
			{
				Sprite sprite = m_SpriteAtlas.GetSprite( spriteName ) ;
				if( sprite != null )
				{
					Sprite = sprite ;

					if( resize == true )
					{
						SetNativeSize() ;
					}

					return true ;
				}
			}

			//----------------------------------------------------------
			// SpriteSet

			if( m_SpriteSet != null )
			{
				Sprite sprite = m_SpriteSet[ spriteName ] ;
				if( sprite != null )
				{
					Sprite = sprite ;

					if( resize == true )
					{
						SetNativeSize() ;
					}

					return true ;
				}
			}

			//----------------------------------------------------------

			return false ;
		}

		/// <summary>
		/// アトラススプライト内のスプライトを取得する
		/// </summary>
		/// <param name="spriteName">スプライト名</param>
		/// <returns>スプライトのインスタンス</returns>
		public Sprite GetSpriteInAtlas( string spriteName )
		{
			//----------------------------------------------------------
			// SpriteAtlas

			if( m_SpriteAtlas != null )
			{
				Sprite sprite = m_SpriteAtlas.GetSprite( spriteName ) ;
				if( sprite != null )
				{
					return sprite ;
				}
			}

			//----------------------------------------------------------
			// SpriteSet

			if( m_SpriteSet != null )
			{
				Sprite sprite = m_SpriteSet[ spriteName ] ;
				if( sprite != null )
				{
					return sprite ;
				}
			}

			//----------------------------------------------------------

			return null ;
		}

		/// <summary>
		/// アトラススプライト内のスプライトの横幅を取得する
		/// </summary>
		/// <param name="spriteName">スプライト名</param>
		/// <returns>横幅</returns>
		public int GetWidthOfSpriteInAtlas( string spriteName )
		{
			//----------------------------------------------------------
			// SpriteAtlas

			if( m_SpriteAtlas != null )
			{
				Sprite sprite = m_SpriteAtlas.GetSprite( spriteName ) ;
				if( sprite != null )
				{
					return ( int )sprite.rect.width ;
				}
			}

			//----------------------------------------------------------
			// SpriteSet

			if( m_SpriteSet != null )
			{
				Sprite sprite = m_SpriteSet[ spriteName ] ;
				if( sprite != null )
				{
					return ( int )sprite.rect.width ;
				}
			}

			//----------------------------------------------------------

			return 0 ;
		}

		/// <summary>
		/// アトラススプライト内のスプライトの縦幅を取得する
		/// </summary>
		/// <param name="spriteName">スプライト名</param>
		/// <returns>縦幅</returns>
		public int GetHeightOfSpriteInAtlas( string spriteName )
		{
			//----------------------------------------------------------
			// SpriteAtlas

			if( m_SpriteAtlas != null )
			{
				Sprite sprite = m_SpriteAtlas.GetSprite( spriteName ) ;
				if( sprite != null )
				{
					return ( int )sprite.rect.height ;
				}
			}

			//----------------------------------------------------------
			// SpriteSet

			if( m_SpriteSet != null )
			{
				Sprite sprite = m_SpriteSet[ spriteName ] ;
				if( sprite != null )
				{
					return ( int )sprite.rect.height ;
				}
			}

			//----------------------------------------------------------

			return 0 ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// １６進数値で色を設定する
		/// </summary>
		/// <param name="color"></param>
		public void SetColor( uint color )
		{
			byte r = ( byte )( ( color >> 16 ) & 0xFF ) ;
			byte g = ( byte )( ( color >>  8 ) & 0xFF ) ;
			byte b = ( byte )( ( color       ) & 0xFF ) ;
			byte a = ( byte )( ( color >> 24 ) & 0xFF ) ;

			Color = new Color32( r, g, b, a ) ;
		}

		/// <summary>
		/// 画像の向きを設定する
		/// </summary>
		public UIInversion.DirectionTypes Inversion
		{
			get
			{
				return CInversion != null ? CInversion.DirectionType : UIInversion.DirectionTypes.None ;
			}
			set
			{
				if( value == UIInversion.DirectionTypes.None )
				{
					IsInversion = false ;
				}
				else
				{
					if( CInversion == null )
					{
						IsInversion = true ;
					}
					CInversion.DirectionType = value ;
				}
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// スプライト(ショートカット)
		/// </summary>
		public Sprite Sprite
		{
			get
			{
				return CImage != null ? CImage.sprite : null ;
			}
			set
			{
				if( CImage != null )
				{
					CImage.sprite = value ;
				}
			}
		}
	
		/// <summary>
		/// カラー(ショートカット)
		/// </summary>
		public Color Color
		{
			get
			{
				return CImage != null ? CImage.color : Color.white ;
			}
			set
			{
				if( CImage != null )
				{
					CImage.color = value ;
				}
			}
		}
	
		/// <summary>
		/// マテリアル(ショートカット)
		/// </summary>
		public Material Material
		{
			get
			{
				return CImage != null ? CImage.material : null ;
			}
			set
			{
				if( CImage != null )
				{
					CImage.material = value ;
				}
			}
		}

		/// <summary>
		/// レイキャストターゲット(ショートカット)
		/// </summary>
		override public bool RaycastTarget
		{
			get
			{
				return CImage != null ? CImage.raycastTarget : false ;
			}
			set
			{
				if( CImage != null )
				{
					CImage.raycastTarget = value ;
				}
			}
		}
		
		/// <summary>
		/// タイプ(ショートカット)
		/// </summary>
		public Image.Type Type
		{
			get
			{
				return CImage != null ? CImage.type : Image.Type.Simple ;
			}
			set
			{
				if( CImage != null )
				{
					CImage.type = value ;
				}
			}
		}
	
		/// <summary>
		/// フィルセンター(ショートカット)
		/// </summary>
		public bool FillCenter
		{
			get
			{
				return CImage != null ? CImage.fillCenter : false ;
			}
			set
			{
				if( CImage != null )
				{
					CImage.fillCenter = value ;
				}
			}
		}

		/// <summary>
		/// フィルメソッド(ショートカット)
		/// </summary>
		public Image.FillMethod FillMethod
		{
			get
			{
				return CImage != null ? CImage.fillMethod : Image.FillMethod.Horizontal ;
			}
			set
			{
				if( CImage != null )
				{
					CImage.fillMethod = value ;
				}
			}
		}

		public enum FillOriginTypes
		{
			Bottom	= 0,
			Right	= 1,
			Top		= 2,
			Left	= 3,
		}

		/// <summary>
		/// フィルオリジン(ショートカット)
		/// </summary>
		public FillOriginTypes FillOriginType
		{
			get
			{
				return CImage != null ? ( FillOriginTypes )CImage.fillOrigin : FillOriginTypes.Bottom ;
			}
			set
			{
				if( CImage != null )
				{
					CImage.fillOrigin = ( int )value ;
				}
			}
		}

		/// <summary>
		/// フィルアマウント(ショートカット)
		/// </summary>
		public float FillAmount
		{
			get
			{
				return CImage != null ? CImage.fillAmount : 0 ;
			}
			set
			{
				if( CImage != null )
				{
					CImage.fillAmount = value ;
				}
			}
		}

		/// <summary>
		/// クロックワイズ(ショートカット)
		/// </summary>
		public bool FillClockwise
		{
			get
			{
				return CImage != null ? CImage.fillClockwise : false ;
			}
			set
			{
				if( CImage != null )
				{
					CImage.fillClockwise = value ;
				}
			}
		}

		/// <summary>
		/// プリサーブアスペクト(ショートカット)
		/// </summary>
		public bool PreserveAspect
		{
			get
			{
				return CImage != null ? CImage.preserveAspect : false ;
			}
			set
			{
				if( CImage != null )
				{
					CImage.preserveAspect = value ;
				}
			}
		}

		/// <summary>
		/// マスクのグラフィック表示の有無(ショートカット)
		/// </summary>
		public bool ShowMaskGraphic
		{
			get
			{
				return CMask != null ? CMask.showMaskGraphic : false ;
			}
			set
			{
				if( CMask != null )
				{
					CMask.showMaskGraphic = value ;
				}
			}
		}

		/// <summary>
		/// RectRransform のサイズをスプライトのサイズに合わせる
		/// </summary>
		public void SetNativeSize()
		{
			if( Sprite != null )
			{
				SetSize( Sprite.rect.width, Sprite.rect.height ) ;
			}
		}

		/// <summary>
		/// シャドーカラーを設定する
		/// </summary>
		public Color ShadowColor
		{
			get
			{
				return CShadow != null ? CShadow.effectColor : Color.white ; 
			}
			set
			{
				if( CShadow == null )
				{
					return ;
				}
				CShadow.effectColor = value ;
			}
		}

		/// <summary>
		/// アウトラインカラーを設定する
		/// </summary>
		public Color OutlineColor
		{
			get
			{
				return COutline != null ? COutline.effectColor : Color.white ; 
			}
			set
			{
				if( COutline == null )
				{
					return ;
				}
				COutline.effectColor = value ;
			}
		}
		
		/// <summary>
		/// バックキー連携
		/// </summary>
		[SerializeField]
		protected bool			m_BackKeyEnabled = false ;
		public    bool			  BackKeyEnabled
		{
			get
			{
				return m_BackKeyEnabled ;
			}
			set
			{
				if( m_BackKeyEnabled != value )
				{
					m_BackKeyEnabled = value ;

					// バックキーの対応処理
					if( m_BackKeyEnabled == true )
					{
						UIEventSystem.RemoveBackKeyTarget( this ) ;
						UIEventSystem.AddBackKeyTarget( this ) ;
					}
					else
					{
						UIEventSystem.RemoveBackKeyTarget( this ) ;
					}
				}
			}
		}

		//-----------------------------------------------------
	
		// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		override protected void OnBuild( string option = "" )
		{
			Image image = CImage ;

			if( image == null )
			{
				image = gameObject.AddComponent<Image>() ;
			}
			if( image == null )
			{
				// 異常
				return ;
			}

			//----------------------------

			if( option.ToLower() == "panel" )
			{
				// Panel
				image.color = new Color32( 255, 255, 255, 100 ) ;
				image.type = Image.Type.Sliced ;

				ResetRectTransform() ;
			
				SetAnchorToStretch() ;
//				SetSize( 0, 0 ) ;
			}
			else
			{
				// Default
//				image.sprite = Resources.Load<Sprite>( "uGUIHelper/Textures/UIBlank" ) ;
				image.color = Color.white ;
				image.type = Image.Type.Sliced ;

				ResetRectTransform() ;
			}

			//----------------------------------

			if( IsCanvasOverlay == true )
			{
				image.material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}

			image.raycastTarget = false ;
		}

		/// <summary>
		/// リソースからスプライトをロードする
		/// </summary>
		/// <param name="path">リソースのパス</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public bool LoadSpriteFromResources( string path )
		{
			Sprite sprite = Resources.Load<Sprite>( path ) ;
			if( sprite == null )
			{
				return false ;
			}

			Sprite = sprite ;

			return true ;
		}

		/// <summary>
		/// スプライトを設定しスプライトのサイズでリサイズする
		/// </summary>
		/// <param name="sprite">スプライトのインスタンス</param>
		public void SetSpriteAndResize( Sprite sprite )
		{
			Sprite = sprite ;

			if( sprite != null )
			{
				Size = sprite.rect.size ;
			}
		}

		/// <summary>
		/// スプライトを設定し任意のサイズにリサイズする
		/// </summary>
		/// <param name="sprite">スプライトのインスタンス</param>
		/// <param name="size">リサイズ後のサイズ</param>
		public void SetSpriteAndResize( Sprite sprite, Vector2 size )
		{
			Sprite = sprite ;
			Size   = size ;
		}

		//-----------------------------------------------------------

		override protected void OnEnable()
		{
			base.OnEnable() ;

			// バックキーの対応処理
			if( m_BackKeyEnabled == true )
			{
				UIEventSystem.AddBackKeyTarget( this ) ;
			}

			//----------------------------------

			// アクティブになった際に１回は必ず更新する
			m_RefreshChildrenColor = true ;

			m_PreviousColor.r = 0 ;
			m_PreviousColor.g = 0 ;
			m_PreviousColor.b = 0 ;
			m_PreviousColor.a = 0 ;
		}

		protected override void OnDisable()
		{
			base.OnDisable() ;

			// バックキーの対応処理
			if( m_BackKeyEnabled == true )
			{
				UIEventSystem.RemoveBackKeyTarget( this ) ;
			}
		}
	}
}
