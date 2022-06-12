using System ;
using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.U2D ;
using UnityEngine.Events ;
using UnityEngine.EventSystems ;

using UnityEngine.Serialization ;

namespace uGUIHelper
{
	/// <summary>
	/// 拡張:イメージナンバー
	/// </summary>
	public class ImageNumber : MaskableGraphicWrapper
	{
		//-------------------------------------------------------------------------------------------
		// SpriteSet 限定
		
//		[FormerlySerializedAs("m_AtlasSprite")]
		[SerializeField]
		private SpriteSet m_SpriteSet = null ;

		private Texture2D m_BlankTexture = null ;

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

					Texture2D texture = null ;
					if( m_SpriteSet != null )
					{
						texture = m_SpriteSet.Texture ;
					}

					// テクスチャを更新する
					if( texture != null )
					{
						CanvasRenderer.SetTexture( texture ) ;
					}
					else
					{
						if( m_BlankTexture == null )
						{
							m_BlankTexture = Resources.Load<Texture2D>( "uGUIHelper/Textures/UIBlank" ) ;
						}
						CanvasRenderer.SetTexture( m_BlankTexture ) ;
					}

					m_CodeSprite = null ;
					UpdateSize() ;	// 実サイズだけ先に更新する
					SetVerticesDirty() ;
				}
			}
		}

		//-------------------------------------------------------------------------------------------
		
		[SerializeField][HideInInspector]
		private Vector2 m_CodeScale = Vector2.one ;

		/// <summary>
		/// 数値の大きさ
		/// </summary>
		public  Vector2  CodeScale
		{
			get
			{
				return m_CodeScale ;
			}
			set
			{
				if( m_CodeScale != value )
				{
					m_CodeScale  = value ;
					UpdateSize() ;	// 実サイズだけ先に更新する
					SetVerticesDirty() ;
				}
			}
		}


		
		[SerializeField][HideInInspector]
		private float m_CodeSpace = 0 ;

		/// <summary>
		/// 数値間の空き
		/// </summary>
		public  float  CodeSpace
		{
			get
			{
				return m_CodeSpace ;
			}
			set
			{
				if( m_CodeSpace != value )
				{
					m_CodeSpace  = value ;
					UpdateSize() ;	// 実サイズだけ先に更新する
					SetVerticesDirty() ;
				}
			}
		}

		/// <summary>
		/// 文字単位の操作用パラメータ
		/// </summary>
		public Vector3[] CodeOffset = new Vector3[]
		{
			new Vector3( 0, 0, 1 ),
			new Vector3( 0, 0, 1 ),
			new Vector3( 0, 0, 1 ),
			new Vector3( 0, 0, 1 ),
			new Vector3( 0, 0, 1 ),
			new Vector3( 0, 0, 1 ),
			new Vector3( 0, 0, 1 ),
			new Vector3( 0, 0, 1 ),
			new Vector3( 0, 0, 1 ),
			new Vector3( 0, 0, 1 )
		} ;

		public void SetCodeOffset( Vector3[] codeOffset )
		{
			CodeOffset = codeOffset ;
			UpdateSize() ;	// 実サイズだけ先に更新する
			SetVerticesDirty() ;
		}



		[SerializeField][HideInInspector]
		private TextAnchor m_Alignment = TextAnchor.MiddleLeft ;

		/// <summary>
		/// 寄せる位置
		/// </summary>
		public  TextAnchor  Alignment
		{
			get
			{
				return m_Alignment ;
			}
			set
			{
				if( m_Alignment != value )
				{
					m_Alignment  = value ;
					UpdateSize() ;	// 実サイズだけ先に更新する
					SetVerticesDirty() ;
				}
			}
		}

		// 整数部の桁数（０でまた指定桁より大きい場合は任意）
		[HideInInspector][SerializeField]
		protected int m_DigitInteger = 0 ;

		/// <summary>
		/// 整数部の桁数
		/// </summary>
		public    int  DigitInteger
		{
			get
			{
				return m_DigitInteger ;
			}
			set
			{
				if( m_DigitInteger != value )
				{
					m_DigitInteger = value ;
					UpdateSize() ;	// 実サイズだけ先に更新する
					SetVerticesDirty() ;
				}
			}
		}
		
		/// <summary>
		/// 整数部の桁数を設定する
		/// </summary>
		/// <param name="tDigitInteger">整数部の桁数</param>
		public void SetDigitInteger( int digitInteger )
		{
			DigitInteger = digitInteger ;
		}
	
		// 小数部の桁数
		[HideInInspector][SerializeField]
		protected int m_DigitDecimal = 0 ;

		/// <summary>
		/// 小数部の桁数
		/// </summary>
		public    int  DigitDecimal
		{
			get
			{
				return m_DigitDecimal ;
			}
			set
			{
				if( m_DigitDecimal != value )
				{
					m_DigitDecimal = value ;
					UpdateSize() ;	// 実サイズだけ先に更新する
					SetVerticesDirty() ;
				}
			}
		}
		
		/// <summary>
		/// 小数部の桁数を設定する
		/// </summary>
		/// <param name="digitDecimal">小数部の桁数</param>
		public void SetDigitDecimal( int digitDecimal )
		{
			DigitDecimal = digitDecimal ;
		}

		/// <summary>
		/// 整数部と小数部の桁数を設定する
		/// </summary>
		/// <param name="integer">整数部の桁数</param>
		/// <param name="decimal">小数部の桁数</param>
		public void SetDigit( int digitInteger, int digitDecimal )
		{
			DigitInteger = digitInteger ;
			DigitDecimal = digitDecimal ;
		}

		// 指定の桁ごとにカンマを表示するか
		[HideInInspector][SerializeField]
		protected int m_Comma  = 0 ;

		/// <summary>
		/// カンマを挿入する桁数
		/// </summary>
		public    int  Comma
		{
			get
			{
				return m_Comma ;
			}
			set
			{
				if( m_Comma != value )
				{
					m_Comma = value ;
					UpdateSize() ;	// 実サイズだけ先に更新する
					SetVerticesDirty() ;
				}
			}
		}
		
		/// <summary>
		/// カンマを挿入する桁数を設定する
		/// </summary>
		/// <param name="comma">カンマを挿入する桁数</param>
		public void SetComma( int comma )
		{
			Comma = comma ;
		}
	
		// プラス符号を表示するか
		[HideInInspector][SerializeField]
		protected bool m_PlusSign  = false ;

		/// <summary>
		/// プラス符号を表示するかどうか
		/// </summary>
		public    bool  PlusSign
		{
			get
			{
				return m_PlusSign ;
			}
			set
			{
				if( m_PlusSign != value )
				{
					m_PlusSign = value ;
					UpdateSize() ;	// 実サイズだけ先に更新する
					SetVerticesDirty() ;
				}
			}
		}
	
		/// <summary>
		/// プラス符号を表示するかどうかを設定する
		/// </summary>
		/// <param name="plusSign">表示状態(true=表示する・false=表示しない)</param>
		public void SetPlusSign( bool plusSign )
		{
			PlusSign = plusSign ;
		}
		
		// ゼロ符号を表示するか
		[HideInInspector][SerializeField]
		protected bool m_ZeroSign  = false ;

		/// <summary>
		/// ゼロ符号を表示するかどうか
		/// </summary>
		public    bool  ZeroSign
		{
			get
			{
				return m_ZeroSign ;
			}
			set
			{
				if( m_ZeroSign != value )
				{
					m_ZeroSign = value ;
					UpdateSize() ;	// 実サイズだけ先に更新する
					SetVerticesDirty() ;
				}
			}
		}
	
		/// <summary>
		/// ゼロ符号を表示するかどうかを設定する
		/// </summary>
		/// <param name="zeroSign">表示状態(true=表示する・false=表示しない)</param>
		public void SetZeroSign( bool zeroSign )
		{
			ZeroSign = zeroSign ;
		}

		// 桁が足りない場合は０かスペースか
		[HideInInspector][SerializeField]
		protected bool m_ZeroPadding = false ;

		/// <summary>
		/// ゼロ埋めを行うか
		/// </summary>
		public    bool  ZeroPadding
		{
			get
			{
				return m_ZeroPadding ;
			}
			set
			{
				if( m_ZeroPadding != value )
				{
					m_ZeroPadding  = value ;
					UpdateSize() ;	// 実サイズだけ先に更新する
					SetVerticesDirty() ;
				}
			}
		}
	
		// パーセンテージを表示するか
		[HideInInspector][SerializeField]
		protected bool m_Percent  = false ;

		/// <summary>
		/// パーセンテージ記号を表示するかどうか
		/// </summary>
		public    bool  Percent
		{
			get
			{
				return m_Percent ;
			}
			set
			{
				if( m_Percent != value )
				{
					m_Percent = value ;
					UpdateSize() ;	// 実サイズだけ先に更新する
					SetVerticesDirty() ;
				}
			}
		}

		/// <summary>
		/// パーセンテージ記号を表示するかどうかを設定する
		/// </summary>
		/// <param name="percent">表示状態(true=表示する・false=表示しない)</param>
		public void SetPercent( bool percent )
		{
			Percent = percent ;
		}
	
		[SerializeField][HideInInspector]
		private double m_Value ;

		/// <summary>
		/// 値
		/// </summary>
		public  double  Value
		{
			get
			{
				return m_Value ;
			}
			set
			{
				if( m_Value != value )
				{
					m_Value = value ;
					UpdateSize() ;	// 実サイズだけ先に更新する
					SetVerticesDirty() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private float m_PreferredWidth = 0 ;

		/// <summary>
		/// 実際の横幅
		/// </summary>
		public float   PreferredWidth
		{
			get
			{
				return m_PreferredWidth ;
			}
		}

		[SerializeField][HideInInspector]
		private float m_PreferredHeight = 0 ;
		
		/// <summary>
		/// 実際の縦幅
		/// </summary>
		public  float  PreferredHeight
		{
			get
			{
				return m_PreferredHeight ;
			}
		}

		//----------------------------------------------------------

		Dictionary<char,Sprite> m_CodeSprite = null ;

		private void UpdateCodeSprite()
		{
			if( m_SpriteSet == null || m_SpriteSet.Texture == null )
			{
				return ;
			}

			string[] list = m_SpriteSet.GetSpriteNames() ;
			if( list == null )
			{
				return ;
			}

			if( m_CodeSprite == null )
			{
				m_CodeSprite = new Dictionary<char,Sprite>() ;
			}
			else
			{
				m_CodeSprite.Clear() ;
			}
			
			int i, j, l ;
			char c ;
			for( i  = 0 ; i <= 9 ; i ++ )
			{
				c = ( char )( ( int )'0' + i ) ;
				for( j  = 0 ; j <  list.Length ; j ++ )
				{
					if( list[ j ].IndexOf( c ) >= 0 )
					{
						m_CodeSprite.Add( ( char )( '0' + i ), m_SpriteSet[ list[ j ] ] ) ;
						break ;
					}
				}
			}

			l = list.Length ;
			string spriteName ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				spriteName = list[ i ].ToLower() ;
				if( spriteName.Contains( "dot" ) == true && m_CodeSprite.ContainsKey( '.' ) == false )
				{
					m_CodeSprite.Add( '.', m_SpriteSet[ list[ i ] ] ) ;
				}
				else
				if( spriteName.Contains( "comma" ) == true && m_CodeSprite.ContainsKey( ',' ) == false )
				{
					m_CodeSprite.Add( ',', m_SpriteSet[ list[ i ] ] ) ;
				}
				else
				if( spriteName.Contains( "plus" ) == true && m_CodeSprite.ContainsKey( '+' ) == false )
				{
					m_CodeSprite.Add( '+', m_SpriteSet[ list[ i ] ] ) ;
				}
				else
				if( spriteName.Contains( "minus" ) == true && m_CodeSprite.ContainsKey( '-' ) == false )
				{
					m_CodeSprite.Add( '-', m_SpriteSet[ list[ i ] ] ) ;
				}
				else
				if( spriteName.Contains( "percent" ) == true && m_CodeSprite.ContainsKey( '%' ) == false )
				{
					m_CodeSprite.Add( '%', m_SpriteSet[ list[ i ] ] ) ;
				}
				else
				if( spriteName.Contains( "space" ) == true && m_CodeSprite.ContainsKey( ' ' ) == false )
				{
					m_CodeSprite.Add( ' ', m_SpriteSet[ list[ i ] ] ) ;
				}
			}
		}

		protected override void Awake()
		{
			base.Awake() ;

			UpdateCodeSprite() ;
		}

		// メッシュ更新
		protected override void OnPopulateMesh( VertexHelper helper )
		{
			if( IsActive() == false )
			{
				return ;
			}

			helper.Clear() ;

			m_PreferredWidth  = 0 ;
			m_PreferredHeight = 0 ;

			//-----------------------------------------

			if( m_SpriteSet == null || m_SpriteSet.Texture == null )
			{
				return ;
			}

			if( m_CodeSprite == null )
			{
				UpdateCodeSprite() ;
			}

			if( m_CodeSprite == null || m_CodeSprite.Count == 0 )
			{
				return ;
			}

			//----------------------------------------------

			List<UIVertex>	aV = new List<UIVertex>() ;
			List<int>		aI = new List<int>() ;

			// 値を文字列化する
			string vs = GetNumberString() ;
			int i, l = vs.Length ;
			
			float w = Size.x ;
			float h = Size.y ;

			Vector2 pivot = RectTransform.pivot ;

			float xMin = - ( w * pivot.x ) ;
			float xMax = w + xMin ;
			float xCenter = ( xMin + xMax ) * 0.5f ;

			float yMin = - ( h * pivot.y ) ;
			float yMax = h + yMin ;
			float yCenter = ( yMin + yMax ) * 0.5f ;

			int tw = m_SpriteSet.Texture.width ;
			int th = m_SpriteSet.Texture.height ;

			Rect r ;
			float vw, vh ;
			float fw = 0 ;

			// まずはトータルの横幅を計算する

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_CodeSprite.ContainsKey( vs[ i ] ) == true )
				{
					r = m_CodeSprite[ vs[ i ] ].textureRect ;
					
					vw = r.width * m_CodeScale.x ;
					fw += vw ;

					if( i <  ( l - 1 ) )
					{
						fw += CodeSpace ;
					}
				}
			}

			float sx = 0, sy ;

			if( m_Alignment == TextAnchor.UpperLeft || m_Alignment == TextAnchor.MiddleLeft || m_Alignment == TextAnchor.LowerLeft )
			{
				// 横は左寄
				sx = xMin ;
			}
			else
			if( m_Alignment == TextAnchor.UpperRight || m_Alignment == TextAnchor.MiddleRight || m_Alignment == TextAnchor.LowerRight )
			{
				// 横は右寄
				sx = xMax - fw ;
			}
			else
			if( m_Alignment == TextAnchor.MiddleCenter || m_Alignment == TextAnchor.UpperCenter || m_Alignment == TextAnchor.LowerCenter )
			{
				// 横は中央
				sx = xCenter - ( fw * 0.5f ) ;
			}

			UIVertex vd ;

			int o = 0 ;
			float ox, oy, oa ;


			float fh = 0 ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_CodeSprite.ContainsKey( vs[ i ] ) == true )
				{
					// 大元のサイズを取得する
					r = m_CodeSprite[ vs[ i ] ].textureRect ;

					vw = r.width  * m_CodeScale.x ;
					vh = r.height * m_CodeScale.y ;

					if( vh >  fh )
					{
						fh  = vh ;
					}

					ox = 0 ;
					oy = 0 ;
					oa = 1 ;
					if( CodeOffset != null && i <  CodeOffset.Length )
					{
						ox = CodeOffset[ i ].x ;
						oy = CodeOffset[ i ].y ;
						oa = CodeOffset[ i ].z ;
					}

					sy = 0 ;
					if( m_Alignment == TextAnchor.UpperLeft || m_Alignment == TextAnchor.UpperCenter || m_Alignment == TextAnchor.UpperRight )
					{
						// 縦は上寄
						sy = yMax ;
					}
					else
					if( m_Alignment == TextAnchor.LowerLeft || m_Alignment == TextAnchor.LowerCenter || m_Alignment == TextAnchor.LowerRight )
					{
						// 縦は下寄
						sy = yMin + vh ;
					}
					else
					if( m_Alignment == TextAnchor.MiddleLeft || m_Alignment == TextAnchor.MiddleCenter || m_Alignment == TextAnchor.MiddleRight )
					{
						// 縦は中央
						sy = yCenter + ( vh * 0.5f ) ;
					}


					// 0
					vd = new UIVertex()
					{
						position = new Vector3( sx + ox, sy + oy, 0 ),
						normal   = new Vector3( 0, 0, -1 ),
						color    = new Color( 1, 1, 1, oa ),
						uv0      = new Vector2( r.x / tw, ( r.y + r.height ) / th )
					} ;
					aV.Add( vd ) ;

					// 1
					vd = new UIVertex()
					{
						position = new Vector3( sx + vw + ox, sy + oy, 0 ),
						normal   = new Vector3( 0, 0, -1 ),
						color    = new Color( 1, 1, 1, oa ),
						uv0      = new Vector2( ( r.x + r.width ) / tw, ( r.y + r.height ) / th )
					} ;
					aV.Add( vd ) ;

					// 2
					vd = new UIVertex()
					{
						position = new Vector3( sx + vw + ox, sy - vh + oy, 0 ),
						normal   = new Vector3( 0, 0, -1 ),
						color    = new Color( 1, 1, 1, oa ),
						uv0      = new Vector2( ( r.x + r.width ) / tw, r.y / th )
					} ;
					aV.Add( vd ) ;

					// 3
					vd = new UIVertex()
					{
						position = new Vector3( sx + ox, sy - vh + oy, 0 ),
						normal   = new Vector3( 0, 0, -1 ),
						color    = new Color( 1, 1, 1, oa ),
						uv0      = new Vector2( r.x / tw, r.y / th )
					} ;
					aV.Add( vd ) ;

					aI.Add( o + 0 ) ;
					aI.Add( o + 1 ) ;
					aI.Add( o + 3 ) ;

					aI.Add( o + 1 ) ;
					aI.Add( o + 2 ) ;
					aI.Add( o + 3 ) ;
				
					o += 4 ;
								
					sx += vw ;
					if( i <  ( l - 1 ) )
					{
						sx += CodeSpace ;
					}
				}
			}

			// 実際のサイズを保存する
			m_PreferredWidth  = fw ;
			m_PreferredHeight = fh ; 

			if( aV.Count >  0 && ( aV.Count % 4 ) == 0 )
			{
				helper.AddUIVertexStream( aV, aI ) ;
			}
		}

		// マテリアル更新
		protected override void UpdateMaterial()
		{
			base.UpdateMaterial() ;

			// テクスチャを更新する
			Texture2D texture = null ;
			if( m_SpriteSet != null )
			{
				texture = m_SpriteSet.Texture ;
			}

			if( texture != null )
			{
				CanvasRenderer.SetTexture( texture ) ;
			}
			else
			{
				if( m_BlankTexture == null )
				{
					m_BlankTexture = Resources.Load<Texture2D>( "uGUIHelper/Textures/UIBlank" ) ;
				}
				CanvasRenderer.SetTexture( m_BlankTexture ) ;
			}
		}

		//-------------------------------------------------------------------------------------------

		// メッシュ更新
		private void UpdateSize()
		{
			m_PreferredWidth  = 0 ;
			m_PreferredHeight = 0 ;

			//-----------------------------------------

			if( m_SpriteSet == null || m_SpriteSet.Texture == null )
			{
				return ;
			}

			if( m_CodeSprite == null )
			{
				UpdateCodeSprite() ;
			}

			if( m_CodeSprite == null || m_CodeSprite.Count == 0 )
			{
				return ;
			}

			//----------------------------------------------

			// 値を文字列化する
			string vs = GetNumberString() ;
			int i, l = vs.Length ;
			
			Rect r ;
			float vw, vh ;
			float fw = 0 ;

			// まずはトータルの横幅を計算する

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_CodeSprite.ContainsKey( vs[ i ] ) == true )
				{
					r = m_CodeSprite[ vs[ i ] ].textureRect ;
					
					vw = r.width * m_CodeScale.x ;
					fw += vw ;

					if( i <  ( l - 1 ) )
					{
						fw += CodeSpace ;
					}
				}
			}
			
			float fh = 0 ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_CodeSprite.ContainsKey( vs[ i ] ) == true )
				{
					// 大元のサイズを取得する
					r = m_CodeSprite[ vs[ i ] ].textureRect ;

					vh = r.height * m_CodeScale.y ;

					if( vh >  fh )
					{
						fh  = vh ;
					}
				}
			}

			// 実際のサイズを保存する
			m_PreferredWidth  = fw ;
			m_PreferredHeight = fh ; 
		}

		// 文字列を更新する
		private string GetNumberString()
		{
			int i, l ;
			string s, ss ;
			double value = m_Value ;

			//-----------------------------------------------------
		
			// 一旦数値を文字列化する
			string t = "" ;
		
			if( value <  0 )
			{
				t = "-" ;
				value = - value ;
			}
			else
			if( value >  0 )
			{
				if( m_PlusSign == true )
				{
					t = "+" ;
				}
			}
			else
			{
				if( m_ZeroSign == true )
				{
					t = "±" ;	// +-
				}
			}
		
			// 整数部
			if( m_DigitInteger <= 0 )
			{
				s = ( ( int )value ).ToString() ;
			}
			else
			{
				if( m_ZeroPadding == false )
				{
					s = string.Format( "{0," + m_DigitInteger + "}", ( int )value ) ;
				}
				else
				{
					s = string.Format( "{0,0:d" + m_DigitInteger + "}", ( int )value ) ;
				}
			}
		
			// s が整数値部
			l = s.Length ;

			if( m_Comma >  0 && m_Comma <  l )
			{
				// 指定桁ごとにカンマを仕込む
				ss = "" ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( i >  0 && ( ( i % m_Comma ) == 0 ) && s[ l - 1 - i ] >= '0' && s[ l - 1 - i ] <= '9' )
					{
						ss = "," + ss ;
					}
				
					ss = s[ l - 1 - i ] + ss ;
				}
			}
			else
			{
				ss = s ;
			}

			t += ss ;

			// 小数部
			if( m_DigitDecimal >  0 )
			{
				t += "." ;
				
				s = value.ToString() ;

				for( i  = 0 ; i <  s.Length ; i ++ )
				{
					if( ( int )s[ i ] == ( int )'.' )
					{
						break ;
					}
				}
			
				if( i >= s.Length )
				{
					s = "" ;
					l = 0 ;
				}
				else
				{
					s = s.Substring( i + 1, s.Length - ( i + 1 ) ) ;
					if( s.Length >  m_DigitDecimal )
					{
						s = s.Substring( 0, m_DigitDecimal ) ;
					}
				
					l = s.Length ;
				}
			
				l = m_DigitDecimal - l ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					s += "0" ;
				}

				l = s.Length ;

				// s が小数値部
				if( m_Comma >  0 && m_Comma <  l )
				{
					// 指定桁ごとにカンマを仕込む
					ss = "" ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						if( i >  0 && ( ( i % m_Comma ) == 0 ) && s[ l - 1 - i ] >= '0' && s[ l - 1 - i ] <= '9' )
						{
							ss += "," ;
						}
					
						ss += s[ i ] ;
//						ss = ss + s.Substring( i, 1 ) ;
					}
				}
				else
				{
					ss = s ;
				}

				t += ss ;
			}
		
			if( m_Percent == true )
			{
				t += "%" ;
			}

			return t ;
		}
	}
}
