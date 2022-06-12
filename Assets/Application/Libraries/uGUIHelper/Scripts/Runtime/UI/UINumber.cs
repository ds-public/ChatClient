using System.Collections ;
using System.Text ;
using UnityEngine ;
using UnityEngine.UI ;

namespace uGUIHelper
{
	/// <summary>
	/// UIText クラスの数値特化機能拡張コンポーネントクラス
	/// </summary>
	public class UINumber : UIText
	{
		[SerializeField][HideInInspector]
		private double m_Value ;

		/// <summary>
		/// 数値
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
					UpdateNumberText() ;
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
					UpdateNumberText() ;
				}
			}
		}
		
		/// <summary>
		/// 整数部の桁数を設定する
		/// </summary>
		/// <param name="digitInteger">整数部の桁数</param>
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
					UpdateNumberText() ;
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
		/// <param name="digitInteger">整数部の桁数</param>
		/// <param name="digitDecimal">小数部の桁数</param>
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
					UpdateNumberText() ;
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
					UpdateNumberText() ;
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
					UpdateNumberText() ;
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
		/// 整数部および小数部ともに桁数に足りない場合はゼロを表示するかどうか
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
					m_ZeroPadding = value ;
					UpdateNumberText() ;
				}
			}
		}
		
		/// <summary>
		/// 整数部および小数部ともに桁数に足りない場合はゼロを表示するかどうかを設定する
		/// </summary>
		/// <param name="zeroPadding">表示状態(true=表示する・false=表示しない)</param>
		public void SetZeroPadding( bool zeroPadding )
		{
			ZeroPadding = zeroPadding ;
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
					UpdateNumberText() ;
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

		/// <summary>
		/// 数値と記号を全角文字にするかどうか
		/// </summary>
		override public    bool  Zenkaku
		{
			get
			{
				return m_Zenkaku ;
			}
			set
			{
				if( m_Zenkaku != value )
				{
					m_Zenkaku = value ;
					UpdateNumberText() ;
				}
			}
		}

		//-------------------------------------------------------------------------------------------
	
		// 文字列を更新する
		private void UpdateNumberText()
		{
			Text text = CText ;
			if( text == null )
			{
				return ;
			}

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

			if( m_Zenkaku == true )
			{
				t = ToLarge( t ) ;
			}

			text.text = t ;

			if( AutoSizeFitting == true )
			{
				Resize() ;
			}
		}

		protected override void OnStart()
		{
			base.OnStart() ;

			UpdateNumberText() ;
		}

		//------------------------------------------------------------

		// 数値を全角文字列にして返す
		// 数値を全角文字列にして返す
		private string ToLarge( string value )
		{
			int v ;
			char c ;

			StringBuilder sb = new StringBuilder() ;

			int i, l = value.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				v = ( int )value[ i ] ;

				if( v >= ( int )'0' && v <= ( int )'9' )
				{
					c = ( char )( ( int )'０' + ( v - ( int )'0' ) ) ;
				}
				else
				if( v == '+' )
				{
					c = '＋' ;
				}
				else
				if( v == '-' )
				{
					c = '－' ;
				}
				else
				if( v == '±' )
				{
					c = '±' ;
				}
				else
				if( v == '.' )
				{
					c = '.' ;
				}
				else
				if( v == ',' )
				{
					c = ',' ;
				}
				else
				if( v == '%' )
				{
					c = '％' ;
				}
				else
				{
					c = ( char )0 ;
				}

				if( c != 0 )
				{
					sb.Append( c ) ;
				}
			}
		
			return sb.ToString() ;
		}
	}
}

