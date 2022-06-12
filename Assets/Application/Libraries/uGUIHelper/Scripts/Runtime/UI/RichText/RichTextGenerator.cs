using UnityEngine ;
using System ;
using System.Collections ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	public class RichTextGenerator
	{
		protected List<UIVertex> m_Vertex = new List<UIVertex>() ;

		protected bool m_Refresh = true ;
		protected bool m_UpdateVertex = true ;
		protected string m_Text ;
		protected RichTextGenerationSettings m_Settings ;




		// 途中改行禁止文字
		protected char[] m_KinsokuCode =
		(
			"abcdefghijklmnopqrstuvwxyz" +
			"ABCDEFGHIJKLMNOPQRSTUVWXYZ" + 
			"0123456789" + 
			"<>=/().,"
		).ToCharArray() ;

		// 行頭禁止文字
		protected char[] m_KinsokuTop =
		(
			",)]｝、。）〕〉》」』】〙〗〟’”｠»"				+	// 終わり括弧類 簡易版
			"ァィゥェォッャュョヮヵヶっぁぃぅぇぉっゃゅょゎ"	+	// 行頭禁則和字 
			"‐゠–〜ー"											+	// ハイフン類
			"?!！？‼⁇⁈⁉"										+	// 区切り約物
			"・:;"												+	// 中点類
			"。."													// 句点類
		).ToCharArray() ;

		// 行末禁止文字
		protected char[] m_KinsokuEnd =
			"(（[｛〔〈《「『【〘〖〝‘“｟«"
			.ToCharArray() ;

		// 文字間のスペースを無視する文字
		protected char[] m_IgnoreLetterSpace =
			"…-－―〜～"
			.ToCharArray() ;


		// 頻繁に使うので基本的なフォントサイズはメンバ変数に保存しておく
		protected int m_BaseFontSize = 0 ;
		protected int m_BaseRubyFontSize = 0 ;
		protected float m_BestFitResizeScale = 1.0f ;
		protected List<List<Block>> m_BlockLineList = null ;

		// ダーシやアンダーラインに使う文字(後で場所を変える)
		protected char m_DashChar = '－' ;

		private List<Code>		m_Code	= new List<Code>() ;
		private List<Word>		m_Ruby	= new List<Word>() ;
		private List<Word>		m_Em	= new List<Word>() ;
		protected int			m_Group = -1 ;

		protected CodeBase		m_Dash	= new CodeBase() ;

		protected float			m_CachedWidth  = 0 ;
		protected float			m_CachedHeight = 0 ;
		protected float			m_CachedFullHeight = 0 ;

//		protected Vector2		m_CursorPosition ;

		/// <summary>
		/// ルビが使用されているかどうか
		/// </summary>
		public bool isRubyUsed
		{
			get
			{
				return m_Ruby.Count >  0 ;
			}
		}

		//--------------------------------------------------------------------------------------------

		public RichTextGenerator() : this( 50 )
		{
		}

		public RichTextGenerator( int tInitialCapacity )
		{
		}

		/// <summary>
		/// 更新が必要な際に呼び出される
		/// </summary>
		/// <param name="tText"></param>
		/// <param name="tSettings"></param>
		/// <returns></returns>
		public bool Populate( string tText, RichTextGenerationSettings tSettings )
		{
			Refresh( tText, tSettings ) ;
			if( m_UpdateVertex == false )
			{
				return true ;	// 頂点も更新する必要が無い
			}

			m_UpdateVertex = false ;

			//-------------------------------------------------------------------------

			m_Vertex.Clear() ;

			if( m_Code.Count == 0 )
			{
				return true ;	// 表示する文字は存在しない
			}

			//-------------------------------------------------------------------------

			float tVW = tSettings.generationExtents.x ;	// 横幅
			float tVH = tSettings.generationExtents.y ;	// 縦幅

			int tFontSize = tSettings.fontSize ;
			int tRubyFontSize = ( int )( tFontSize * tSettings.rubySizeScale ) ;


			int tLine, tStep ;

			List<Block> tBlockLine = null ;
			Block  tBlock = null ;


			int i, o, r, e, v, p, q, ac ;
			float w0, w1, wl, ws, bw, w, px, py, a0, a1, a2 ;
			float xs, tMaxHeight ;
			Code tCode ;

			float dx, dy ;


			float tOX =     - ( tVW * tSettings.pivot.x ) ;
			float tOY = tVH - ( tVH * tSettings.pivot.y ) ;

			float tCW = -1 ;
			float tCH = -1 ;


			if( tSettings.verticalOverflow == VerticalWrapMode.Truncate )
			{
				tCH = tVH ;	// 縦方向のクリッピングが有効(Truncate 用)
			}

			//----------------------------------------------------------

			int l = m_BlockLineList.Count ;

			bool tViewControllEnabled = tSettings.viewControllEnabled ;

			int tStartLine = 0 ;
			if( tViewControllEnabled == true && tSettings.startLineOfView >= 0 && tSettings.startLineOfView <  l )
			{
				tStartLine = tSettings.startLineOfView ;
			}
			int tEndLine = l ;
			if( tViewControllEnabled == true && tSettings.endLineOfView >  tStartLine && tSettings.endLineOfView <= l )
			{
				tEndLine = tSettings.endLineOfView ;
			}
			
			if( tEndLine <= tStartLine )
			{
				tEndLine  = tStartLine + 1 ;
			}

			//----------------------------------------------------------

			int tLengthOfView		= tSettings.lengthOfView ;

			if( tViewControllEnabled == false )
			{
				tLengthOfView = -1 ;	// 全表示
			}

			int tStartOffsetOfView	= GetStartOffsetOfLine_Private( tStartLine ) ;
			if( tLengthOfView == 0 || ( tLengthOfView >  0 && tLengthOfView <= tStartOffsetOfView ) )
			{
				// 表示する文字は存在しない
				return true ;
			}

			int tEndOffsetOfView	= GetEndOffsetOfLine_Private( tEndLine - 1 ) ;
			if( tLengthOfView <  0 || tLengthOfView >  tEndOffsetOfView )
			{
				tLengthOfView  = tEndOffsetOfView ;
			}

			//----------------------------------------------------------

			// 開始と終了の２つの位置から表示対象文字のアルファ値を算出する
			float[] tAlpha = null ;

			if( tViewControllEnabled == true )
			{
				int tStartOffsetOfFade	= tSettings.startOffsetOfFade ;
				if( tStartOffsetOfFade <   tStartOffsetOfView )
				{
					tStartOffsetOfFade  = tStartOffsetOfView  ;
				}

				int tEndOffsetOfFade	= tSettings.endOffsetOfFade ;
				if( tEndOffsetOfFade <  0 )
				{
					tEndOffsetOfFade  =  tEndOffsetOfView ;
				}

				if( tEndOffsetOfFade <  tStartOffsetOfView )
				{
					tEndOffsetOfFade  = tStartOffsetOfView  ;
				}
				if( tEndOffsetOfFade >  tEndOffsetOfView )
				{
					tEndOffsetOfFade  = tEndOffsetOfView ;
				}

				if( tEndOffsetOfFade <  tStartOffsetOfFade )
				{
					tEndOffsetOfFade  = tStartOffsetOfFade ;
				}

				//---------------------------------

				// 以下は実際に表示する文字数の数値にする
				int tLengthOfFade	= tLengthOfView			- tStartOffsetOfView ;

				tStartOffsetOfFade	= tStartOffsetOfFade	- tStartOffsetOfView ;
				tEndOffsetOfFade	= tEndOffsetOfFade		- tStartOffsetOfView ;
				
				if( tLengthOfFade >  0 )
				{
					tAlpha  = new float[ tLengthOfFade ] ;

					p = ( int )( ( tEndOffsetOfFade - tStartOffsetOfFade + tSettings.widthOfFade ) * tSettings.ratioOfFade ) + tStartOffsetOfFade ;	// この値より後は完全非表示
					q = p - tSettings.widthOfFade ;	// この値以下が完全表示
					if( q <  tStartOffsetOfFade )
					{
						q  = tStartOffsetOfFade ;
					}

					for( i  = 0 ; i <  tLengthOfFade ; i ++ )
					{
						 o = 1 + i ;

						if( o >  p || o > tEndOffsetOfFade )
						{
							tAlpha[ i ] = 0 ;
						}
						else
						if( o <= q )
						{
							tAlpha[ i ] = 1 ;
						}
						else
						{
							tAlpha[ i ] = ( float )( p - o + 1 ) / ( float )( tSettings.widthOfFade + 1 ) ;
						}
					}
				}
			}

//			tAlpha = null ;

			int tAlphaIndex = 0 ;

			//----------------------------------------------------------

			int tOffsetOfView ;

			// 各ブロックのメッシュを展開する
			for( tLine  = tStartLine ; tLine <  tEndLine ; tLine ++ )
			{
				tBlockLine = m_BlockLineList[ tLine ] ;

				tOffsetOfView = GetStartOffsetOfLine_Private( tLine ) ;

				for( tStep  = 0 ; tStep <  tBlockLine.Count ; tStep ++ )
				{
					tBlock = tBlockLine[ tStep ] ;

					px = tBlock.x ;
					py = tBlock.y ;

					for( o  = tBlock.offset ; o <  ( tBlock.offset + tBlock.length ) ; o ++ )
					{
						tCode = m_Code[ o ] ;

						if( tCode.ruby >= 0 )
						{
							// ルビ対象文字
							r = tCode.ruby ;

							// 対象の横幅を求める
							w0 = 0 ;
							wl = 0 ;
							e  = 0 ;
							v  = 0 ;

							a0 = -1 ;
							a1 = -1 ;
							ac =  0 ;


							for( i  = o ; i <  ( o + m_Ruby[ r ].length ) ; i ++ )
							{
								tCode = m_Code[ i ] ;

								if( tCode.value != 0 || ( tCode.value == 0 && tCode.space == true ) )
								{
									w0 = w0 + m_Code[ i ].width ;
									e ++ ;

									if( tCode.value != 0 )
									{
										if( tOffsetOfView <  tLengthOfView )
										{
											if( a0 <  0 )
											{
												if( tAlpha != null )
												{
													a0 = tAlpha[ tAlphaIndex + ac ] ;
												}
												else
												{
													a0 = 1.0f ;
												}
											}
											if( tAlpha != null )
											{
												a1 = tAlpha[ tAlphaIndex + ac ] ;
											}
											else
											{
												a1 = 1.0f ;
											}
											ac ++ ;

											v ++ ;
										}
	
										tOffsetOfView ++ ;
									}
								}
							}
							
							// ルビ文字の合計横幅
							w1 = m_Ruby[ r ].GetWidth() ;

							if( w0 >= w1 )
							{
								// 対象の方が長い
								bw = w0 ;

								if( tOffsetOfView >= tLengthOfView )
								{
									for( i  = o ; i <  ( o + m_Ruby[ r ].length ) ; i ++ )
									{
										tCode = m_Code[ i ] ;
										if( tCode.value != 0 || ( tCode.value == 0 && tCode.space == true ) )
										{
											// ルビ対象文字列内でのスペースは無視される
											wl = wl + tCode.width ;

											if( tCode.value != 0 )
											{
												v -- ;
												if( v == 0 )
												{
													break ;
												}
											}
										}
									}
								}
							}
							else
							{
								// ルビの方が長い
								bw = w1 ;

								if( tOffsetOfView >= tLengthOfView )
								{
									// ブロックの途中でルビ文字の表示が切れる

									// ルビ対象文字１文字あたりの左右の追加スペース
									ws = ( w1 - w0 ) / ( float )e ;
									wl = 0 ;
									for( i  = o ; i < ( o + m_Ruby[ r ].length ) ; i ++ )
									{
										tCode = m_Code[ i ] ;
										if( tCode.value != 0 || ( tCode.value == 0 && tCode.space == true ) )
										{
											wl = wl + tCode.width + ws ;

											if( tCode.value != 0 )
											{
												v -- ;
												if( v == 0 )
												{
													break ;
												}
											}
										}
									}
								}
							}

							//-----------------------------------------------

							// 対象の文字を展開する
							dx = px ;
							dy = py ;

							// 文字間隔の半分を計算する
							xs = ( ( bw - w0 ) / ( float )e ) * 0.5f ;
							w0 = 0 ;

							tMaxHeight = 0 ;
							for( i  = o ; i <  ( o + m_Ruby[ r ].length ) ; i ++ )
							{
								if( tAlpha != null )
								{
									a2 = tAlpha[ tAlphaIndex ] ;
								}
								else
								{
									a2 = 1.0f ;
								}

								tCode = m_Code[ i ] ;
								if( tCode.value != 0 )
								{
									if( tCode.dash == false )
									{
										w = AddGeometry( tCode, dx + xs * m_BestFitResizeScale, dy, m_BestFitResizeScale, tCW, tCH, a2 ) ;
									}
									else
									{
										// ダーシ
										w = tCode.width ;
									}
								}
								else
								{
									w = tCode.width ;	// スペース
								}
										
								// ダーシまたはアンダーライン
								if( tCode.dash == true || tCode.strike == true )
								{
									AddDashGeometry( m_Dash, dx, dy, xs + w + xs, m_BestFitResizeScale, tCW, tCH, a2, 0 ) ;
								}

								if( tCode.u == true )
								{
									AddDashGeometry( m_Dash, dx, dy, xs + w + xs, m_BestFitResizeScale, tCW, tCH, a2, tFontSize ) ;
								}

								dx = dx + ( xs + w + xs ) * m_BestFitResizeScale ;

								if( tCode.height >  tMaxHeight )
								{
									tMaxHeight = tCode.height ;
								}

								if( tCode.value != 0 )
								{
									tAlphaIndex ++ ;
								}

								if( wl >  0 )
								{
									w0 = w0 + xs + w + xs ;
									if( w0 >= wl )
									{
										// 表示終了
										break ;
									}
								}
							}

							if( tMaxHeight <  tFontSize )
							{
								tMaxHeight  = tFontSize ;
							}

							// ルビの文字を展開する
							dx = px ;
							dy = py + ( ( tMaxHeight - tFontSize ) + tRubyFontSize ) * m_BestFitResizeScale ;

							xs = ( ( ( bw - w1 ) ) / ( float )m_Ruby[ r ].code.Count ) * 0.5f ;
							w1 = 0 ;

							// アルファ表示用に表示されるルビ文字の数をカウントする(ルビ文字に空白しか無い場合は ac が 0 になってゼロ割りが発生してしまうため文字も空白も区別無くカウントする)
							ac = 0 ;

							for( i  = 0 ; i <  m_Ruby[ r ].code.Count ; i ++ )
							{
								if( wl >  0 )
								{
									if( ( w1 + xs + m_Ruby[ r ].code[ i ].width ) >  wl )
									{
										// 表示終了
										break ;
									}
								}

								if( m_Ruby[ r ].code[ i ].value != 0 )
								{
									w = AddGeometryBase( m_Ruby[ r ].code[ i ], xs * m_BestFitResizeScale + dx, dy, m_BestFitResizeScale, tCW, tCH, a0 + ( ( a1 - a0 ) * ( float )( ac + 1 ) / ( float )m_Ruby[ r ].code.Count ) ) ;
								}
								else
								{
									w = m_Ruby[ r ].code[ i ].width ;
								}
								dx = dx + ( xs + w + xs ) * m_BestFitResizeScale ;
								ac ++ ;

								if( wl >  0 )
								{
									w1 = w1 + xs + w + xs ;
									if( w1 >= wl )
									{
										// 表示終了
										break ;
									}
								}
							}

							px = px + bw ;

							o = o + m_Ruby[ r ].length - 1 ;    // -1 に注意
						}
						else
						{
							// ルビ対象文字ではない

							if( tCode.value != 0 || ( tCode.value == 0 && tCode.space == true ) )
							{
								if( tAlpha != null )
								{
									a2 = tAlpha[ tAlphaIndex ] ;
								}
								else
								{
									a2 = 1.0f ;
								}

								if( tCode.value != 0 )
								{
									if( tCode.dash == false )
									{
										w = AddGeometry( tCode, px, py, m_BestFitResizeScale, tCW, tCH, a2 ) ;
									}
									else
									{
										w = tCode.width ;
									}
								}
								else
								{
									w = tCode.width ;	// スペース
								}

								if( tCode.dash == true || tCode.strike == true )
								{
									AddDashGeometry( m_Dash, px, py, w, m_BestFitResizeScale, tCW, tCH, a2, 0 ) ;
								}

								if( tCode.u == true )
								{
									AddDashGeometry( m_Dash, px, py, w, m_BestFitResizeScale, tCW, tCH, a2, tFontSize ) ;
								}

								if( tCode.em >= 0 )
								{
									// 傍点あり
									r = tCode.em ;

									// 傍点の横幅を求める
									w1 = m_Em[ r ].GetWidth() ;

									tMaxHeight = tCode.height ;
									if( tMaxHeight <  tFontSize )
									{
										tMaxHeight  = tFontSize ;
									}

									// 傍点の文字を展開する
									dx = px + ( ( w - w1 ) * 0.5f ) * m_BestFitResizeScale ;
									dy = py + ( ( tMaxHeight - tFontSize ) + tRubyFontSize ) * m_BestFitResizeScale ;

									for( i  = 0 ; i <  m_Em[ r ].code.Count ; i ++ )
									{
										if( m_Em[ r ].code[ i ].value != 0 )
										{ 
											w = AddGeometryBase( m_Em[ r ].code[ i ], dx, dy, m_BestFitResizeScale, tCW, tCH, a2 ) ;
										}
										else
										{
											w = m_Em[ r ].code[ i ].width ;
										}

										dx = dx + w * m_BestFitResizeScale ;
									}
								}

								px = px + ( w * m_BestFitResizeScale ) ;

								if( tCode.value !=  0 )
								{
									tAlphaIndex ++ ;

									tOffsetOfView ++ ;
								}
							}
						}

						if( tOffsetOfView >= tLengthOfView )
						{
							break ;
						}
					}

					if( tOffsetOfView >= tLengthOfView )
					{
						break ;
					}
				}

				if( tOffsetOfView >= tLengthOfView )
				{
					break ;
				}
			}

			//----------------------------------------------------------

			// 座標を調整する
			UIVertex vp ;
			for( i  = 0 ; i <  m_Vertex.Count ; i ++ )
			{
				vp = m_Vertex[ i ] ;
				vp.position.x += tOX ;
				vp.position.y += tOY ;
				m_Vertex[ i ] = vp ;
			}

			return true ;
		}
	
		// 頂点情報を更新する
		protected void Refresh( string tText, RichTextGenerationSettings tSettings )
		{
			if( m_Refresh == false && m_Text == tText && m_Settings.Equals( tSettings ) == true )
			{
				// 変更の必要無し
				return ;
			}

			m_Refresh = false ;
			m_Text = tText ;
			m_Settings = tSettings ;

			m_UpdateVertex = true ;
			
			//-----------------------------------------------------------

			m_CachedWidth  = 0 ;
			m_CachedHeight = 0 ;

			//-----------------------------------------------------------

			if( tSettings.fontSize <  2 )
			{
				// フォントサイズは最低２は必要
				return ;
			}

			// 基本フォントサイズは頻繁に使うのでメンバ変数に保存しておく
			m_BaseFontSize = tSettings.fontSize ;
			m_BaseRubyFontSize = ( int )( m_BaseFontSize * tSettings.rubySizeScale ) ;

			// ダーシの設定
			m_Dash.value = m_DashChar ;
			m_Dash.color =  tSettings.color ;

			// 文字分解情報を更新する
			Parse( tText, tSettings ) ;

			//-----------------------------------------------------------

			if( m_Code.Count == 0 )
			{
				// 表示するものは無い
				return ;
			}

			//-----------------------------------------------------------

			Font tFont = tSettings.font ;

			if( tFont.dynamic == true )
			{
				// ダイナミックフォントを使用するのでフォントテクスチャを更新する
				UpdateTexture( tSettings ) ;
			}

			//-----------------------------------------------------------

			int i, l ;

			int tFontSize ;

			// 文字の頂点データを取得する
			l = m_Code.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_Code[ i ].value != 0 )
				{
					if( m_Code[ i ].size == 0 )
					{
						tFontSize = m_BaseFontSize ;
					}
					else
					{
						tFontSize = m_Code[ i ].size ;
					}

					m_Code[ i ].GetGeometry( tFont, tFontSize, tSettings.fontStyle, this ) ;
				}
			}

			l = m_Ruby.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				m_Ruby[ i ].GetGeometry( tFont, ( int )( tSettings.fontSize * tSettings.rubySizeScale ), tSettings.fontStyle, this ) ;
			}

			l = m_Em.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				m_Em[ i ].GetGeometry( tFont, ( int )( tSettings.fontSize * tSettings.rubySizeScale ), tSettings.fontStyle, this ) ;
			}

			// ダーシ
			m_Dash.GetGeometryBase( tFont, tSettings.fontSize, tSettings.fontStyle, this ) ;
			CollectDash( m_Dash ) ;	//ダーシのＵＶの位置を調整する

			//---------------------------------------------------------------------------------

			float tVW = tSettings.generationExtents.x ;
			float tVH = tSettings.generationExtents.y ;

			float tRW = 0 ;
			float tRH = 0 ;
			float tSH = 0 ;

			tFontSize = tSettings.fontSize ;
//			int tRubyFontSize = ( int )( tFontSize * tSettings.rubySizeScale ) ;

			float w, h ;

			int tLine, tStep ;
			float px = 0 ;
			float py = 0 ;


			// ブロックを順番に取り出す

			Block  tBlock = null, tBlockOld = null, tBlockNew = null ;
			int tIndex =  0 ;

			float tLineHeight = tSettings.fontSize * tSettings.lineSpacing ;

			// 全ブロックを生成する
			List<Block> tBlockList = new List<Block>() ;

			l = m_Code.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				tBlock = GetBlock( ref tIndex, tSettings.richText ) ;
				if( tBlock == null )
				{
					break ;
				}
				tBlockList.Add( tBlock ) ;
			}

			// 禁則処理を施し各ブロックの位置を決定する
			// 且つプロックを行単位に切り分ける
			m_BlockLineList = new List<List<Block>>() ;

			List<Block> tBlocksOfLine = new List<Block>() ;

			l = tBlockList.Count ;

			if( tSettings.horizontalOverflow == HorizontalWrapMode.Wrap )
			{
				// 自動改行有効
				for( i  = 0 ; i <  l ; i ++ )
				{
					// 個々のプロックの禁則処理を行う
					tBlock = tBlockList[ i ] ;
					if( i >  0 )
					{
						tBlockOld = tBlockList[ i - 1 ] ;
					}
					else
					{
						tBlockOld = null ;
					}
					if( i <  ( l - 1 ) )
					{
						tBlockNew = tBlockList[ i + 1 ] ;
					}
					else
					{
						tBlockNew = null ;
					}

					if( tBlock.isNewLine == true )
					{
						// 有効な強制改行
						ReduceSpaceBlock( ref tBlocksOfLine ) ;	// 確定行の右側のスペースを全て削除する

						m_BlockLineList.Add( tBlocksOfLine ) ;
						tBlocksOfLine = new List<Block>() ;

						px = 0 ;
					}
					else
					if( tBlock.isKinsokuTop == false && tBlock.isKinsokuEnd == false )
					{
						// 禁則対象ではない

						if( ( px + tBlock.width ) <= tVW )
						{
							// 改行対象にならない

							// そのまま展開
							tBlocksOfLine.Add( tBlock ) ;
					
							px = px + tBlock.width ;
						}
						else
						{
							// 改行対象になる

							if( px == 0 )
							{
								// 自身のプロックで横方向にあふれてしまう

								// そのまま展開
								tBlocksOfLine.Add( tBlock ) ;

								m_BlockLineList.Add( tBlocksOfLine ) ;
								tBlocksOfLine = new List<Block>() ;

								px = 0 ;
							}
							else
							{
								// 先に改行

								ReduceSpaceBlock( ref tBlocksOfLine ) ;	// 確定行の右側のスペースを全て削除する

								m_BlockLineList.Add( tBlocksOfLine ) ;
								tBlocksOfLine = new List<Block>() ;

								px = 0 ;

								if( tBlock.isSpace == false )
								{
									// 改行後に展開
									tBlocksOfLine.Add( tBlock ) ;

									// スペースではない
									px = px + tBlock.width ;
								}
								// 改行対象がスペースだった場合に改行後に左側にスペースが出来るのかはおかしいの削除する(ただし１だけ・２つ以上のスペースが続く場合は２つ目以降のスペースは有効になる)
							}
						}
					}
					else
					if( tBlock.isKinsokuTop == true )
					{
						// 行頭禁則確認 )}] 行頭にきてはならない

						if( ( px + tBlock.width ) <= tVW )
						{
							// 改行対象にならない
	
							// そのまま展開
							tBlocksOfLine.Add( tBlock ) ;
					
							px = px + tBlock.width ;
						}
						else
						{
							// このプロックは改行対象になる
							if( tBlocksOfLine.Count == 0 )
							{
								// 前のブロックが同じ行に存在しないのでそのまま展開して改行する(※このブロックは表示可能横幅を超過する)
								tBlocksOfLine.Add( tBlock ) ;
						
								m_BlockLineList.Add( tBlocksOfLine ) ;
								tBlocksOfLine = new List<Block>() ;

								px = 0 ;
							}
							else
							{
								// 前のプロックが存在する
								if( tBlockOld.isKinsokuTop == true )
								{
									// 同じ行の１つ前のブロックも行頭禁止なので自身が改行する
									m_BlockLineList.Add( tBlocksOfLine ) ;
									tBlocksOfLine = new List<Block>() ;

									px = 0 ;

									// 展開
									tBlocksOfLine.Add( tBlock ) ;

									px = px + tBlock.width ;
								}
								else
								{
									// 同じ行の１つ前のブロックは行頭禁止ではない

									if( ( tBlockOld.width + tBlock.width ) <= tVW )
									{
										// １つ前のプロックとの横幅の合計が表示領域以下の場合は１つ前のブロックごと改行する
										tBlocksOfLine.Remove( tBlockOld ) ;

										ReduceSpaceBlock( ref tBlocksOfLine ) ;	// 確定行の右側のスペースを全て削除する

										m_BlockLineList.Add( tBlocksOfLine ) ;
										tBlocksOfLine = new List<Block>() ;

										px = 0 ;

										// １つ前のブロックの展開位置を修正
										tBlocksOfLine.Add( tBlockOld ) ;

										px = px + tBlockOld.width ;

										// 現在のブロックを展開
										tBlocksOfLine.Add( tBlock ) ;

										px = px + tBlock.width ;
									}
									else
									{
										// １つ前のプロックとの横幅の合計が表示領域超過の場合は自身のみ改行する
								
										m_BlockLineList.Add( tBlocksOfLine ) ;
										tBlocksOfLine = new List<Block>() ;

										px = 0 ;

										// 展開
										tBlocksOfLine.Add( tBlock ) ;

										px = px + tBlock.width ;
									}
								}
							}
						}
					}
					else
					if( tBlock.isKinsokuEnd == true )
					{
						// 行末禁止確認 [{( 行末にきてはならない

						if( ( px + tBlock.width ) >  tVW )
						{
							// 自身が改行対象
							
							ReduceSpaceBlock( ref tBlocksOfLine ) ;	// 確定行の右側のスペースを全て削除する

							m_BlockLineList.Add( tBlocksOfLine ) ;
							tBlocksOfLine = new List<Block>() ;

							px = 0 ;

							// 展開
							tBlocksOfLine.Add( tBlock ) ;

							px = px + tBlock.width ;
						}
						else
						{
							// 自身は改行対象にはならない
							if( tBlockNew == null || ( tBlockNew != null && ( px + tBlock.width + tBlockNew.width ) >  tVW ) )
							{
								// 次のプロックが存在しないまたは次のプロックが改行対象

								ReduceSpaceBlock( ref tBlocksOfLine ) ;	// 確定行の右側のスペースを全て削除する

								// 自身が改行対象
								m_BlockLineList.Add( tBlocksOfLine ) ;
								tBlocksOfLine = new List<Block>() ;
	
								px = 0 ;

								// 展開
								tBlocksOfLine.Add( tBlock ) ;

								px = px + tBlock.width ;
							}
							else
							{
								// 次のブロックが存在し且つ改行対象にはなっていない

								// そのまま展開
								tBlocksOfLine.Add( tBlock ) ;

								px = px + tBlock.width ;
							}
						}
					}
				}
			}
			else
			{
				// 自動改行無効

				// 自動改行有効
				for( i  = 0 ; i <  l ; i ++ )
				{
					// 個々のプロックの禁則処理を行う
					tBlock = tBlockList[ i ] ;

					if( tBlock.isNewLine == true )
					{
						// 有効な強制改行

						ReduceSpaceBlock( ref tBlocksOfLine ) ;	// 確定行の右側のスペースを全て削除する

						m_BlockLineList.Add( tBlocksOfLine ) ;
						tBlocksOfLine = new List<Block>() ;

						px = 0 ;
					}
					else
					{
						// 禁則対象ではない

						// そのまま展開(明示的な改行が示されるまで横に続く)
						tBlocksOfLine.Add( tBlock ) ;
					
						px = px + tBlock.width ;
					}
				}
			}

			// 最後の行を追加
			ReduceSpaceBlock( ref tBlocksOfLine ) ;	// 確定行の右側のスペースを全て削除する

			m_BlockLineList.Add( tBlocksOfLine ) ;

			// ここまでは全ての行で処理する

			//---------------------------------------------------------------------------------

			// テキストアライメントの反映と各ブロックの座標の設定を行う

			l = m_BlockLineList.Count ;
			if( l == 0 )
			{
				l  = 1 ;	// 最低限１行は存在するとみなす
			}

			bool tViewControllEnabled = tSettings.viewControllEnabled ;

			int tStartLine = 0 ;
			if( tViewControllEnabled == true && tSettings.startLineOfView >= 0 && tSettings.startLineOfView <  l )
			{
				tStartLine = tSettings.startLineOfView ;
			}
			int tEndLine = l ;
			if( tViewControllEnabled == true && tSettings.endLineOfView >  tStartLine && tSettings.endLineOfView <= l )
			{
				tEndLine = tSettings.endLineOfView ;
			}
			int tFullEndLine = l ;

			if( tEndLine <= tStartLine )
			{
				tEndLine  = tStartLine + 1 ;
			}

			//----------------------------------

			// 縦位置
			float tTopMarginHeight		= tSettings.fontSize * tSettings.topMarginSpacing ;
			float tBottomMarginHeight	= tSettings.fontSize * tSettings.bottomMarginSpacing ;
			int vl ;

			//----------------------------------

			vl  = tEndLine - tStartLine ;
			if( vl <  0 )
			{
				vl  = 0 ;
			}

			h = tTopMarginHeight + tBottomMarginHeight ;
			if( vl >= 2 )
			{
				h = h + tLineHeight * ( vl - 1 ) ;
			}

			h = h + tFontSize ;	// 最低１行は表示される

			tRH = h ;	// 最終的な縦幅

			//----------------------------------

			vl  = tFullEndLine - tStartLine ;
			if( vl <  0 )
			{
				vl  = 0 ;
			}

			h = tTopMarginHeight + tBottomMarginHeight ;
			if( vl >= 2 )
			{
				h = h + tLineHeight * ( vl - 1 ) ;
			}

			h = h + tFontSize ;	// 最低１行は表示される

			tSH = h ;	// 最終的な縦幅

			//----------------------------------

			tRW = 0 ;	// 最終的な横幅

			//----------------------------------------------------------

			int tLengthOfView		= tSettings.lengthOfView ;

			if( tViewControllEnabled == false )
			{
				tLengthOfView = -1 ;
			}

			int tStartOffsetOfView	= GetStartOffsetOfLine_Private( tStartLine ) ;

			int tEndOffsetOfView	= GetEndOffsetOfLine_Private( tEndLine - 1 ) ;

			if( tLengthOfView <  0 || tLengthOfView >  tEndOffsetOfView )
			{
				tLengthOfView  = tEndOffsetOfView ;
			}

			int tOffsetOfView ;

			if( m_BlockLineList.Count >  0 && tLengthOfView >  tStartOffsetOfView )
			{
				for( tLine  = tStartLine ; tLine <  tEndLine ; tLine ++ )
				{
					tBlocksOfLine = m_BlockLineList[ tLine ] ;

					tOffsetOfView = GetStartOffsetOfLine_Private( tLine ) ;

					w = 0 ;
					for( tStep  = 0 ; tStep <  tBlocksOfLine.Count ; tStep ++ )
					{
						tBlock = tBlocksOfLine[ tStep ] ;
						w = w + GetBlockWidth( tBlock, ref tOffsetOfView, tLengthOfView ) ;

						if( tOffsetOfView >= tLengthOfView )
						{
							break ;
						}
					}

					if( w >  tRW )
					{
						tRW  = w ;
					}

					if( tOffsetOfView >= tLengthOfView )
					{
						break ;
					}
				}
			}

			//---------------------------------------------------------^

			float tAX = 1.0f ;
			float tAY = 1.0f ;
			m_BestFitResizeScale  = 1.0f ;

			if( tSettings.resizeTextForBestFit == true )
			{
				if( tRW >  0 )
				{
					tAX = tVW / tRW ;
				}

				if( tRH >  0 )
				{
					tAY = tVH / tRH ;
				}

				if( tAX <  1.0f || tAY <  1.0f )
				{
					// 縮小する必要がある
					if( tAX <  tAY )
					{
						m_BestFitResizeScale  = tAX ;
					}
					else
					{
						m_BestFitResizeScale  = tAY ;
					}

					float tFA = ( float )tSettings.resizeTextMinSize / ( float )tSettings.fontSize ;

					if( tFA >  1.0f )
					{
						tFA  = 1.0f ;
					}

					if( tFA >  m_BestFitResizeScale )
					{
						m_BestFitResizeScale  = tFA ;
					}
				}
				else
				if( tAX >  1.0f && tAY >  1.0f )
				{
					// 拡大する必要がある
					if( tAX <  tAY )
					{
						m_BestFitResizeScale  = tAX ;
					}
					else
					{
						m_BestFitResizeScale  = tAY ;
					}

					float tFA = ( float )tSettings.resizeTextMaxSize / ( float )tSettings.fontSize ;
					if( tFA <  1.0f )
					{
						tFA  = 1.0f ;
					}

					if( m_BestFitResizeScale >  tFA )
					{
						m_BestFitResizeScale  = tFA ;
					}
				}
			}

			//---------------------------------------------------------^

			if( tSettings.textAnchor == TextAnchor.UpperLeft || tSettings.textAnchor == TextAnchor.UpperCenter || tSettings.textAnchor == TextAnchor.UpperRight )
			{
				// 上寄せ
				py = - ( tTopMarginHeight * m_BestFitResizeScale ) ;
			}
			else
			if( tSettings.textAnchor == TextAnchor.MiddleLeft || tSettings.textAnchor == TextAnchor.MiddleCenter || tSettings.textAnchor == TextAnchor.MiddleRight )
			{
				// 中寄せ
				py =  - ( ( tVH - ( tRH * m_BestFitResizeScale ) ) * 0.5f ) - ( tTopMarginHeight * m_BestFitResizeScale ) ;
			}
			else
			{
				// 下寄せ
				py = - tVH + ( tRH * m_BestFitResizeScale ) - ( tTopMarginHeight * m_BestFitResizeScale ) ;
			}

			//---------------------------------------------------------^

//			float by = 0 ;

			m_ExtraTagEvent.Clear() ;
			int o, c, k ;

			c = 0 ;

			if( m_BlockLineList.Count >  0 && tLengthOfView >  tStartOffsetOfView )
			{
				for( tLine  = tStartLine ; tLine <  tEndLine ; tLine ++ )
				{
					tBlocksOfLine = m_BlockLineList[ tLine ] ;

					tOffsetOfView = GetStartOffsetOfLine_Private( tLine ) ;

					w = 0 ; 
					for( tStep  = 0 ; tStep <  tBlocksOfLine.Count ; tStep ++ )
					{
						tBlock = tBlocksOfLine[ tStep ] ;
						w = w + GetBlockWidth( tBlock, ref tOffsetOfView, tLengthOfView ) ;
						
						c = tOffsetOfView ;

						o = tBlock.offset ;
						l = tBlock.length ;
						for( i  = 0 ; i <  l ; i ++ )
						{
							if( m_Code[ o ].value != 0 )
							{
								c ++ ;
							}
							else
							if( m_Code[ o ].value == 0 && string.IsNullOrEmpty( m_Code[ o ].uniqueName ) == false )
							{
								for( k  = 0 ; k <  m_ExtraTagName.Count ; k ++ )
								{
									if( m_Code[ o ].uniqueName == m_ExtraTagName[ k ] )
									{
										m_ExtraTagEvent.Add( new RichText.ExtraTagEvent( c, m_Code[ o ].uniqueName, m_Code[ o ].uniqueValue ) ) ;
										break ;
									}
								}
							}
							o ++ ;
						}

						if( tOffsetOfView >= tLengthOfView )
						{
							break ;
						}
					}

					if( tSettings.textAnchor == TextAnchor.UpperLeft || tSettings.textAnchor == TextAnchor.MiddleLeft || tSettings.textAnchor == TextAnchor.LowerLeft )
					{
						// 左寄せ
						px = 0 ;
					}
					else
					if( tSettings.textAnchor == TextAnchor.UpperCenter || tSettings.textAnchor == TextAnchor.MiddleCenter || tSettings.textAnchor == TextAnchor.LowerCenter )
					{
						// 中寄せ
						px = ( tVW - ( w * m_BestFitResizeScale ) ) * 0.5f ;
					}
					else
					{
						// 右寄せ
						px = tVW - ( w * m_BestFitResizeScale ) ;
					}

					// 改め横の位置を設定する
					for( tStep  = 0 ; tStep <  tBlocksOfLine.Count ; tStep ++ )
					{
						tBlock = tBlocksOfLine[ tStep ] ;

						tBlock.x = px ;
						tBlock.y = py ;

						StoCodePositionInBlock( tBlock ) ;	// 各ブロック内の文字の座標を設定する

						px = px + ( tBlock.width * m_BestFitResizeScale ) ;
					}

//					by = py - ( tFontSize   * m_BestFitResizeScale ) ;	// 最も下の位置
					py = py - ( tLineHeight * m_BestFitResizeScale ) ;

					if( tOffsetOfView >= tLengthOfView )
					{
						break ;
					}
				}
			}

			// サイズを保存しておく
			m_CachedWidth		=                      tRW                         * m_BestFitResizeScale ;
			m_CachedHeight		= ( tTopMarginHeight + tRH + tBottomMarginHeight ) * m_BestFitResizeScale ;
			m_CachedFullHeight	= ( tTopMarginHeight + tSH + tBottomMarginHeight ) * m_BestFitResizeScale ;


			// カーソルポジョンを保存しておく
//			m_CursorPosition.x = px ;
//			m_CursorPosition.y = by ;

//			Debug.LogWarning( "縦幅:" + m_CachedHeight + " " + tTopMarginHeight + " " + tRH + " " + tBottomMarginHeight + " " +tText + " SL:" + tStartLine + " EL:" + tEndLine + " FEL:" + tFullEndLine ) ;

			//---------------------------------------------------------------------------------
		}

		// ダーシのＵＩの座標を調整する
		private void CollectDash( CodeBase tDash )
		{
			float tXmin = 2, tYmin = 2, tXmax = -1, tYmax = -1 ;
			float x, y ;

			int i ;
			for( i  = 0 ; i <  4 ; i ++ )
			{
				x = tDash.td[ i ].x ;
				y = tDash.td[ i ].y ;

				if( x <  tXmin )
				{
					tXmin  = x ;
				}
				if( x >  tXmax )
				{
					tXmax  = x ;
				}
				if( y <  tYmin )
				{
					tYmin  = y ;
				}
				if( y >  tYmax )
				{
					tYmax  = y ;
				}
			}

			float xd = tXmax - tXmin ;
			float yd = tYmax - tYmin ;

			if( xd >  yd )
			{
				// 横の方が長い
				for( i  = 0 ; i <  4 ; i ++ )
				{
					if( tDash.td[ i ].x == tXmin )
					{
						// 最小の位置
						tDash.td[ i ].x += ( xd * 0.25f ) ;
					}
					else
					if( tDash.td[ i ].x == tXmax )
					{
						// 最大の位置
						tDash.td[ i ].x -= ( xd * 0.25f ) ;
					}
				}
			}
			else
			if( yd >  xd )
			{
				// 縦の方が長い
				for( i  = 0 ; i <  4 ; i ++ )
				{
					if( tDash.td[ i ].y == tYmin )
					{
						// 最小の位置
						tDash.td[ i ].y += ( yd * 0.25f ) ;
					}
					else
					if( tDash.td[ i ].y == tYmax )
					{
						// 最大の位置
						tDash.td[ i ].y -= ( yd * 0.25f ) ;
					}
				}
			}
		}


		// 行が確定して後の左側のスペースを全て削除する
		private void ReduceSpaceBlock( ref List<Block> rBlockLine )
		{
			int i, l ;
			while( true )
			{
				l = rBlockLine.Count ;
				if( l == 0 )
				{
					break ;
				}
				i = l - 1 ;
				if( rBlockLine[ i ].isSpace == true )
				{
					rBlockLine.RemoveAt( i ) ;
				}
				else
				{
					break ;
				}
			}
		}

		/// <summary>
		/// 途中で改行が出来ない文字群
		/// </summary>
		public class Block
		{
			public int offset ;
			public int length ;

			public float width ;	// エレメントの累計横幅(フルのケース)
			public float height ;	// エレメントの累計縦幅

			public bool isKinsokuTop = false ;
			public bool isKinsokuEnd = false ;

			public bool isNewLine = false ;
			public bool isSpace   = false ;

			public float x ;		// ブロックの展開座標Ｘ
			public float y ;		// ブロックの展開座標Ｙ
		}

		// 複数の文字からなるエレメント情報を取得する(処理は基本的にエレメント単位で行われる)
		protected Block GetBlock( ref int rIndex, bool tRichText )
		{
			// 途中改行禁止文字・ルビ付き文字・グループ文字のいずれかが続く限りエレメントとしてカウントする

			if( rIndex >= m_Code.Count )
			{
				// 終了
				return null ;
			}

			Block tBlock = new Block() ;
			Code tCode ;


			tBlock.offset = rIndex ;

			if( tRichText == false )
			{
				// 禁則処理を行わない
				tCode = m_Code[ tBlock.offset ] ;

				if( tCode.value != 0 )
				{
					tBlock.width = tCode.width ;
				}

				tBlock.length = 1 ;

				// プロックの文字が１文字で且つ改行か
				if( tCode.newline == true )
				{
					tBlock.isNewLine = true ;
				}

				// ブロックの文字が１文字で且つ空白か
				if( ( tCode.value == 0 && tCode.space == true ) )
				{
					tBlock.isSpace = true ;
				}

				// 次のインデックスへ
				rIndex = tBlock.offset + tBlock.length ;

				return tBlock ;
			}

			//----------------------------------------------------------
			
			tBlock.length = 0 ;

			int i, l = m_Code.Count ;
			int o = rIndex ;

			bool f ;
			bool tFlag  = false ;
			int  tRuby  = -1 ;
			int  tGroup = -1 ;

			// コントロールコードじゃない位置までインデックスを進ませる
			for( i  = o ; i <  l ; i ++ )
			{
				if( m_Code[ i ].value != 0 )
				{
					tFlag  = IsKinsokuCode( m_Code[ i ].value ) ;
					tRuby  = m_Code[ i ].ruby ;
					tGroup = m_Code[ i ].group ;
					break ;
				}
			}

			// 改行禁止の範囲をブロックとして取り出す
			for( i  = o ; i <  l ; i ++ )
			{
				if( m_Code[ i ].value != 0 )
				{
					f = IsKinsokuCode( m_Code[ i ].value ) ;
				
//					if( i == ( l - 1 ) )
//					{
//						Debug.LogWarning( "f:" + f +  " tFlag:" + tFlag + " OR:" + tRuby + " NR:" + m_Code[ i ].ruby + " OG:" + tGroup + " NG:" + m_Code[ i ].group ) ;
//					}

					if( ( f == false || f != tFlag ) && ( m_Code[ i ].ruby <  0 || m_Code[ i ].ruby != tRuby ) && ( m_Code[ i ].group <  0 || m_Code[ i ].group != tGroup ) )
					{
						break ;	// 終わり
					}

					tFlag  = f ;
					tRuby  = m_Code[ i ].ruby ;
					tGroup = m_Code[ i ].group ;
				}
				else
				{
					// コントロールコードで且つ改行禁止区間である場合は無視する
					if( m_Code[ i ].ruby <  0 && m_Code[ i ].group <  0 )
					{
						break ;	// 終わり
					}
				}

				tBlock.length ++ ;
			}

			// 必ずブロックは完全な状態で切り出し
			// その後ブロック内で表示可能な範囲か判定する

//			Debug.LogWarning( "ブロックサイズ:" + tBlock.length ) ;
//			Debug.LogWarning( "文字:" + m_Code[ tBlock.offset ].value ) ;

			if( tBlock.length == 0 )
			{
				tBlock.length ++ ;	// 最低１文字
			}

			//-----------------------------------------------------------------------

			// 横幅の合計を計算する
			tBlock.width = 0 ;

			// ブロックの中を捜査して横幅に関わる情報を取り出していく
			// ルビ以外はそのまま width を使えばよい
			// ルビの場合はルビの合計横幅と対象文字横幅の合計の長い方を使用する

			int r ;
			float w0, w1 ;

			for( o  = tBlock.offset ; o <  ( tBlock.offset + tBlock.length ) ; o ++ )
			{
				tCode = m_Code[ o ] ;

				if( tCode.ruby >= 0 )
				{
					// ルビ付きの文字
					r = tCode.ruby ;
	
					// ルビの対象文字の合計横幅とルビ文字の合計横幅で長い方を最終的な横幅にする

					// 対象文字の合計横幅
					w0 = 0 ;
					for( i  = o ; i <  ( o + m_Ruby[ r ].length ) ; i ++ )
					{
						tCode = m_Code[ i ] ;

						if( tCode.value != 0 || ( tCode.value == 0 && tCode.space == true ) )
						{
							w0 = w0 + tCode.width ;
						}
					}

					// ルビ文字の合計横幅
					w1 = m_Ruby[ r ].GetWidth() ;

					if( w0 >= w1 )
					{
						// 対象文字の方が横幅が長い
						tBlock.width = tBlock.width + w0 ;
					}
					else
					{
						// ルビ文字の方が横幅が長い
						tBlock.width = tBlock.width + w1 ;
					}

					// 次のエレメントへ
					o = o + m_Ruby[ r ].length - 1 ;    // -1 が必要な事に注意する
				}
				else
				{
					// ルビ対象ではない文字
					if( tCode.value != 0 || ( tCode.value == 0 && tCode.space == true ) )
					{
						tBlock.width = tBlock.width + tCode.width ;
					}
				}
			}

			//-----------------------------------------------------------------------

			// 最初の文字が行頭禁止文字かどうか ]}) など
			tBlock.isKinsokuTop = IsKinsokuTop( m_Code[ tBlock.offset ].value ) ;

			// 最後の文字が行末禁止文字かどうか ({[ など
			tBlock.isKinsokuEnd = IsKinsokuEnd( m_Code[ tBlock.offset + tBlock.length - 1 ].value ) ;

			// プロックの文字が１文字で且つ改行か
			if( tBlock.length == 1 && m_Code[ tBlock.offset ].newline == true )
			{
				tBlock.isNewLine = true ;
			}

			// ブロックの文字が１文字で且つ空白か
			if( tBlock.length == 1 && m_Code[ tBlock.offset ].value == 0 && m_Code[ tBlock.offset ].space == true )
			{
				tBlock.isSpace = true ;
			}

			// 次のインデックスへ
			rIndex = tBlock.offset + tBlock.length ;

			return tBlock ;
		}

		// 実際の横幅を取得する(途中で文字が切れる想定)
		protected float GetBlockWidth( Block tBlock, ref int rOffsetOfView, int tLengthOfView )
		{
			Code tCode ;
			int i, o, r, e, v ;
			float w0, w1, wl, ws, bw, xs ;

			float px = 0 ;

			for( o  = tBlock.offset ; o <  ( tBlock.offset + tBlock.length ) ; o ++ )
			{
				tCode = m_Code[ o ] ;

				if( tCode.ruby >= 0 )
				{
					// ルビ対象文字
					r = tCode.ruby ;

					// 対象の横幅を求める
					w0 = 0 ;
					wl = 0 ;
					e  = 0 ;
					v  = 0 ;

					for( i  = o ; i <  ( o + m_Ruby[ r ].length ) ; i ++ )
					{
						tCode = m_Code[ i ] ;

						if( tCode.value != 0 || ( tCode.value == 0 && tCode.space == true ) )
						{
							w0 = w0 + m_Code[ i ].width ;
							e ++ ;

							if( tCode.value != 0 )
							{
								if( rOffsetOfView <  tLengthOfView )
								{
									v ++ ;
								}
								rOffsetOfView ++ ;
							}
						}
					}

					// ルビ文字の合計横幅
					w1 = m_Ruby[ r ].GetWidth() ;

					if( w0 >= w1 )
					{
						// 対象の方が長い
						bw = w0 ;

						if( rOffsetOfView >= tLengthOfView )
						{
							// 途中で切れた
							for( i  = o ; i <  ( o + m_Ruby[ r ].length ) ; i ++ )
							{
								tCode = m_Code[ i ] ;
								if( tCode.value != 0 || ( tCode.value == 0 && tCode.space == true ) )
								{
									wl = wl + tCode.width ;

									if( tCode.value != 0 )
									{
										v -- ;
										if( v == 0 )
										{
											break ;
										}
									}
								}
							}
						}
					}
					else
					{
						// ルビの方が長い
						bw = w1 ;

						if( rOffsetOfView >= tLengthOfView )
						{
							// ブロックの途中でルビ文字の表示が切れる

							// ルビ対象文字１文字あたりの左右の追加スペース
							ws = ( w1 - w0 ) / ( float )e ;
							wl = 0 ;
							for( i  = o ; i < ( o + m_Ruby[ r ].length ) ; i ++ )
							{
								tCode = m_Code[ i ] ;
								if( tCode.value != 0 || ( tCode.value == 0 && tCode.space == true ) )
								{
									wl = wl + tCode.width + ws ;

									if( tCode.value != 0 )
									{
										v -- ;
										if( v == 0 )
										{
											break ;
										}
									}
								}
							}
						}
					}

					//-----------------------------------------------

					// 文字間隔の半分を計算する
					xs = ( ( bw - w0 ) / ( float )e ) * 0.5f ;
					w0 = 0 ;

					for( i  = o ; i <  ( o + m_Ruby[ r ].length ) ; i ++ )
					{
						tCode = m_Code[ i ] ;

						px = px + ( xs + tCode.width + xs ) ;

						if( wl >  0 )
						{
							w0 = w0 + xs + tCode.width + xs ;
							if( w0 >= wl )
							{
								// 表示終了
								break ;
							}
						}
					}

					//-----------------------------------------------

					// 次のエレメントへ
					o = o + m_Ruby[ r ].length - 1 ;    // -1 が必要な事に注意する
				}
				else
				{
					// ルビ対象文字ではない

					if( tCode.value != 0 || ( tCode.value == 0 && tCode.space == true ) )
					{
						px = px + tCode.width ;

						if( tCode.value != 0 )
						{
							rOffsetOfView ++ ;
						}
					}
				}

				if( rOffsetOfView >= tLengthOfView )
				{
					// 表示終了
					break ;
				}
			}

			return px ;
		}

		// 各文字の座標を設定する
		protected void StoCodePositionInBlock( Block tBlock )
		{
			int i, l, o ;
			float x, y ;

			o = tBlock.offset ;
			l = tBlock.length ;

			x = tBlock.x ;
			y = tBlock.y ;

			CodeBase tCode ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				tCode = m_Code[ o + i ] ;

				tCode.x = x ;
				tCode.y = y ;

				if( tCode.value != 0 )
				{
					x = x + tCode.width ;
				}
			}
		}


		// フォントテクスチャを更新する
		protected void UpdateTexture( RichTextGenerationSettings tSettings )
		{
			Font tFont = tSettings.font ;

			int tBaseFontSize = tSettings.fontSize ;
			FontStyle tBaseFontStyle = tSettings.fontStyle ;

			float tRubySizeScale = tSettings.rubySizeScale ;

			int i, j, l ;
			List<char> c = new List<char>() ;
			int tActiveFontSize = tBaseFontSize ;
			int tFontSize = tBaseFontSize ;
			FontStyle tActiveFontStyle = tBaseFontStyle  ;
			FontStyle tFontStyle = FontStyle.Normal ;


			// ダーシは必ず必要
			if( m_DashChar != 0 )
			{
				tFont.RequestCharactersInTexture( new string( new char[]{ m_DashChar } ), tActiveFontSize, tActiveFontStyle ) ;
			}

			l = m_Code.Count ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_Code[ i ].value != 0 && m_Code[ i ].dash == false )
				{
					// 表示対象文字

					// サイズ
					if( m_Code[ i ].size <= 0 )
					{
						tFontSize = m_BaseFontSize ;
					}
					else
					{
						tFontSize = m_Code[ i ].size ;
					}

					// スタイル
					if( m_Code[ i ].b == false && m_Code[ i ].i == false )
					{
						tFontStyle = tBaseFontStyle ;
					}
					else
					if( m_Code[ i ].b == true  && m_Code[ i ].i == false )
					{
						tFontStyle = FontStyle.Bold ;
					}
					else
					if( m_Code[ i ].b == false && m_Code[ i ].i == true )
					{
						tFontStyle = FontStyle.Italic ;
					}
					else
					if( m_Code[ i ].b == true && m_Code[ i ].i == true )
					{
						tFontStyle = FontStyle.BoldAndItalic ;
					}
				
					if( tFontSize == tActiveFontSize && tFontStyle == tActiveFontStyle )
					{
						// サイズとスタイルが同じ
						c.Add( m_Code[ i ].value ) ;
					}
					else
					{
						// サイズとスタイルが異なる

						if( c.Count >  0 )
						{
							// １文字以上登録があればテクスチャに書き込む
							tFont.RequestCharactersInTexture( new string( c.ToArray() ), tActiveFontSize, tActiveFontStyle ) ;
							c.Clear() ;
						}

						c.Add( m_Code[ i ].value ) ;

						tActiveFontSize  = tFontSize ;
						tActiveFontStyle = tFontStyle ;
					}
				}
			}

			if( c.Count >  0 )
			{
				// １文字以上登録があればテクスチャに書き込む(最後)
				tFont.RequestCharactersInTexture( new string( c.ToArray() ) , tActiveFontSize, tActiveFontStyle ) ;
			}

			//---------------------------------------------

			// Ruby And Em

			tActiveFontSize = ( int )( tBaseFontSize * tRubySizeScale ) ;
			if( tActiveFontSize <  1 )
			{
				tActiveFontSize  = 1 ;
			}

			tActiveFontStyle = tBaseFontStyle ;

			c.Clear() ;

			// Ruby
			l = m_Ruby.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				for( j  = 0 ; j <  m_Ruby[ i ].code.Count ; j ++ )
				{
					// コントロールコード(0)が混入される事は無い
					if( m_Ruby[ i ].code[ j ].value != 0 && c.Contains( m_Ruby[ i ].code[ j ].value ) == false )
					{
						c.Add( m_Ruby[ i ].code[ j ].value ) ;
					}
				}
			}

			// Em
			l = m_Em.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				for( j  = 0 ; j <  m_Em[ i ].code.Count ; j ++ )
				{
					// コントロールコード(0)が混入される事は無い
					if( m_Em[ i ].code[ j ].value != 0 && c.Contains( m_Em[ i ].code[ j ].value ) == false )
					{
						c.Add( m_Em[ i ].code[ j ].value ) ;
					}
				}
			}
		
			if( c.Count >  0 )
			{
				// １文字以上登録があればテクスチャに書き込む
				tFont.RequestCharactersInTexture( new string( c.ToArray() ) , tActiveFontSize, tActiveFontStyle ) ;
			}
		}

		// １文字分のメッシュ情報を追加する(文章文字用)
		protected float AddGeometry( Code tCode, float px, float py, float rs, float cw, float ch, float a )
		{
			// 上付き文字
			// 下付き文字
			// 傍点
			// アンダーライン
			// ストライク
			// ダーシ

			// 縦位置の調整
			float sy = 0 ;
			if( tCode.height <  m_BaseFontSize )
			{
				// 表示文字の縦幅が１行の縦幅より小さい
				if( tCode.sup == true && tCode.sub == false )
				{
					// 上寄り
					sy = 0 ;
				}
				else
				if( tCode.sup == false && tCode.sub == true )
				{
					// 下寄り
					sy = - ( m_BaseFontSize - tCode.height ) ;
				}
				else
				{
					// 中寄り
					sy = - ( m_BaseFontSize - tCode.height ) * 0.5f ;
				}
			}
			else
			if( tCode.height >  m_BaseFontSize )
			{
				// 表示文字の縦幅が１行の縦幅より大きい
				sy = tCode.height - m_BaseFontSize ;
			}
		
			float w ;
		
			if( tCode.dash == false )
			{
				w = AddGeometryBase( tCode, px, py + sy * rs, rs, cw, ch, a ) ;
			}
			else
			{
				// ダーシ
				w = m_Dash.width ;
			}

			return w ;
		}
	
		// ダーシ・取り消し線・アンダーラインの描画
		protected void AddDashGeometry( CodeBase tDash, float px, float py, float dw, float rs, float cw, float ch, float a, float u = 0 )
		{
			float yu = 0 ;
			if( u >  0 )
			{
				// アンダーラインあり
				if( tDash.vd[ 0 ].y <  tDash.vd[ 1 ].y )
				{
					yu = tDash.vd[ 0 ].y ;
				}
				else
				{
					yu = tDash.vd[ 1 ].y ;
				}

				yu = - u - yu - 2 ;
			}

			Vector2[] vd = new Vector2[ 4 ] ;

			vd[ 0 ].x = 0 ;
			vd[ 0 ].y = tDash.vd[ 0 ].y + yu ;

			vd[ 1 ].x = dw ;
			vd[ 1 ].y = tDash.vd[ 1 ].y + yu ;

			vd[ 2 ].x = dw ;
			vd[ 2 ].y = tDash.vd[ 2 ].y + yu ;

			vd[ 3 ].x = 0 ;
			vd[ 3 ].y = tDash.vd[ 3 ].y + yu ;

			UIVertex v ;

			int i ;

			if( ch >  0 )
			{
				// Truncate チェック
				float tMinY =   Mathf.Infinity ;
				float tMaxY = - Mathf.Infinity ;
				float y ;
				for( i  = 0 ; i <  4 ; i ++ )
				{
					y = - ( py + ( vd[ i ].y * rs ) ) ;	// 符号が上下逆であることに注意
					if( y <  tMinY )
					{
						tMinY = y ;
					}
					if( y >  tMaxY )
					{
						tMaxY = y ;
					}
				}

				if( tMinY <  0 || tMaxY >  ch )
				{
					// 表示しない
					return ;
				}
			}

			Color tColor = new Color( tDash.color.r, tDash.color.g, tDash.color.b, tDash.color.a * a ) ;

			for( i  = 0 ; i <  4 ; i ++ )
			{
				v = new UIVertex() ;
				v.position	= new Vector3( px + ( vd[ i ].x * rs ), py + ( vd[ i ].y * rs ),    0 ) ;
				v.normal	= new Vector3(    0,    0,   -1 ) ;
				v.color		= tColor ;
				v.uv0		= new Vector2( tDash.td[ i ].x, tDash.td[ i ].y ) ;
				m_Vertex.Add( v ) ;
			}
		}

		// １文字分のメッシュ情報を追加する(ルビ文字用)
		protected float AddGeometryBase( CodeBase tCode, float px, float py, float rs, float cw, float ch, float a )
		{
			UIVertex v ;

			// 左右のスペースを調整する
			float sx = ( tCode.width - tCode.advance ) * 0.5f ;

			int i ;

			if( ch >  0 )
			{
				// Truncate チェック
				float tMinY =   Mathf.Infinity ;
				float tMaxY = - Mathf.Infinity ;
				float y ;
				for( i  = 0 ; i <  4 ; i ++ )
				{
					y = - ( py + ( tCode.vd[ i ].y * a ) ) ;	// 符号が上下逆であることに注意
					if( y <  tMinY )
					{
						tMinY = y ;
					}
					if( y >  tMaxY )
					{
						tMaxY = y ;
					}
				}

				if( tMinY <  0 || tMaxY >  ch )
				{
					// 表示しない
					return tCode.width ;
				}
			}

			Color tColor = new Color( tCode.color.r, tCode.color.g, tCode.color.b, tCode.color.a * a ) ;

			for( i  = 0 ; i <  4 ; i ++ )
			{
				v = new UIVertex() ;
				v.position	= new Vector3( px + ( sx + tCode.vd[ i ].x ) * rs, py + ( tCode.vd[ i ].y * rs ),    0 ) ;
				v.normal	= new Vector3(    0,    0,   -1 ) ;
				v.color		= tColor ;
				v.uv0		= new Vector2( tCode.td[ i ].x, tCode.td[ i ].y ) ;
				m_Vertex.Add( v ) ;
			}

			return tCode.width ;
		}


		public void Invalidate()
		{
			m_Refresh = true ;
		}

		public float GetPreferredWidth( string tText, RichTextGenerationSettings tSettings )
		{
			Refresh( tText, tSettings ) ;

			return m_CachedWidth ;
		}

		public float GetPreferredHeight( string tText, RichTextGenerationSettings tSettings )
		{
			Refresh( tText, tSettings ) ;

			return m_CachedHeight ;
		}

		public float GetPreferredFullHeight( string tText, RichTextGenerationSettings tSettings )
		{
			Refresh( tText, tSettings ) ;

			return m_CachedFullHeight ;
		}


/*		/// <summary>
		/// カーソルの位置を取得する
		/// </summary>
		/// <param name="tText"></param>
		/// <param name="tSettings"></param>
		/// <returns></returns>
		public Vector2 GetCursorPosition( string tText, RichTextGenerationSettings tSettings )
		{
			Refresh( tText, tSettings ) ;

			return m_CursorPosition ;
		}*/

		public List<UIVertex> verts
		{
			get
			{
				return m_Vertex ;
			}
		}


		/// <summary>
		/// ルビまたは傍点が存在するか
		/// </summary>
		public bool	IsNeedTopMargin( string tText, RichTextGenerationSettings tSettings )
		{
			Refresh( tText, tSettings ) ;

			if( m_Ruby.Count >  0 )
			{
				return true ;
			}

			if( m_Em.Count >  0 )
			{
				return true ;
			}

			return false ;
		}

		/// <summary>
		/// アンダーラインが存在するか
		/// </summary>
		public bool IsNeedBottomMargin( string tText, RichTextGenerationSettings tSettings )
		{
			Refresh( tText, tSettings ) ;

			int i, l = m_Code.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_Code[ i ].u == true )
				{
					return true ;
				}
			}

			return false ;
		}

		/// <summary>
		/// 描画対象となる文字数を返す
		/// </summary>
		/// <returns></returns>
		public int GetLength( string tText, RichTextGenerationSettings tSettings )
		{
			Refresh( tText, tSettings ) ;

			int i, l = m_Code.Count ;
			int c = 0 ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_Code[ i ].value != 0 )
				{
					c ++ ;
				}
			}

			return c ;
		}

		/// <summary>
		/// 行数を取得する
		/// </summary>
		/// <param name="tText"></param>
		/// <param name="tSettings"></param>
		/// <returns></returns>
		public int GetLine( string tText, RichTextGenerationSettings tSettings )
		{
			Refresh( tText, tSettings ) ;

			if( m_Code.Count == 0 )
			{
				return 0 ;
			}

			return m_BlockLineList.Count ;
		}

		/// <summary>
		/// 指定したラインの開始時の描画対象となる文字数を取得する
		/// </summary>
		/// <param name="tText"></param>
		/// <param name="tSettings"></param>
		/// <param name="tLine"></param>
		/// <returns></returns>
		public int GetStartOffsetOfLine( string tText, RichTextGenerationSettings tSettings, int tLine )
		{
			Refresh( tText, tSettings ) ;

			return GetStartOffsetOfLine_Private( tLine ) ;
		}

		private int GetStartOffsetOfLine_Private( int tLine )
		{
			if( m_Code.Count == 0 )
			{
				// 表示すべき文字が存在しない
				return 0 ;
			}

			if( tLine <  0 || tLine >= m_BlockLineList.Count )
			{
				// 不正
				return 0 ;
			}

			int i, l, p, c ;

			List<Block> tBlocksOfLine = m_BlockLineList[ tLine ] ;
			if( tBlocksOfLine.Count == 0 )
			{
				if( tLine == 0 )
				{
					return 0 ;
				}

				for( p  = tLine - 1 ; p >= 0 ; p -- )
				{
					tBlocksOfLine = m_BlockLineList[ p ] ;
					if( tBlocksOfLine.Count >  0 )
					{
						// 意味のあるプロックを発見した
						
						// この行には何も無いので１つ前の行の最後の値を返す
						l =	tBlocksOfLine[ tBlocksOfLine.Count - 1 ].offset + tBlocksOfLine[ tBlocksOfLine.Count - 1 ].length - 1 ;
						c = 0 ;
						for( i  = 0 ; i <= l ; i ++ )
						{
							if( m_Code[ i ].value != 0 )
							{
								c ++ ;
							}
						}

						return c ;
					}
				}

				return 0 ;	// 最後まで意味のあるブロックは発見出来なかった
			}

			//-------------------------------------------------

			l = tBlocksOfLine[ 0 ].offset ;

			c = 0 ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_Code[ i ].value != 0 )
				{
					c ++ ;
				}
			}
			
			return c ;
		}

		/// <summary>
		/// 指定したラインの終了時の描画対象となる文字数を取得する
		/// </summary>
		/// <param name="tText"></param>
		/// <param name="tSettings"></param>
		/// <param name="tLine"></param>
		/// <returns></returns>
		public int GetEndOffsetOfLine( string tText, RichTextGenerationSettings tSettings, int tLine )
		{
			Refresh( tText, tSettings ) ;

			return GetEndOffsetOfLine_Private( tLine ) ;
		}

		private int GetEndOffsetOfLine_Private( int tLine )
		{
			if( m_Code.Count == 0 )
			{
				// 表示すべき文字が存在しない
				return 0 ;
			}

			if( tLine <  0 || tLine >= m_BlockLineList.Count )
			{
				// 不正
				return 0 ;
			}

			int i, l, p, c ;

			List<Block> tBlocksOfLine = m_BlockLineList[ tLine ] ;
			if( tBlocksOfLine.Count == 0 )
			{
				if( tLine == 0 )
				{
					return 0 ;
				}

				for( p  = tLine - 1 ; p >= 0 ; p -- )
				{
					tBlocksOfLine = m_BlockLineList[ p ] ;
					if( tBlocksOfLine.Count >  0 )
					{
						// 意味のあるプロックを発見した
						
						// この行には何も無いので１つ前の行の最後の値を返す
						l =	tBlocksOfLine[ tBlocksOfLine.Count - 1 ].offset + tBlocksOfLine[ tBlocksOfLine.Count - 1 ].length - 1 ;
						c = 0 ;
						for( i  = 0 ; i <= l ; i ++ )
						{
							if( m_Code[ i ].value != 0 )
							{
								c ++ ;
							}
						}

						return c ;
					}
				}

				return 0 ;	// 最後まで意味のあるブロックは発見出来なかった
			}

			//-------------------------------------------------

			l = tBlocksOfLine[ tBlocksOfLine.Count - 1 ].offset + tBlocksOfLine[ tBlocksOfLine.Count - 1 ].length ;

			c = 0 ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_Code[ i ].value != 0 )
				{
					c ++ ;
				}
			}

			return c ;
		}

		private List<string> m_ExtraTagName = new List<string>(){ "wait", "shake" } ;

		/// <summary>
		/// 拡張タグ名を設定する
		/// </summary>
		/// <param name="tTagName"></param>
		public void SetExtraTagName( params string[] tTagName )
		{
			m_ExtraTagName.Clear() ;

			if( tTagName == null || tTagName.Length ==  0 )
			{
				return ;
			}
			
			int i, l = tTagName.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				m_ExtraTagName.Add( tTagName[ i ] ) ;
			}
		}


		private List<RichText.ExtraTagEvent> m_ExtraTagEvent = new List<RichText.ExtraTagEvent>() ;

		/// <summary>
		/// ウェイトタグ系イベントが発生する場所と種別を取得する
		/// </summary>
		/// <returns></returns>
		public RichText.ExtraTagEvent[] GetExtraTagEvent( string tText, RichTextGenerationSettings tSettings, params string[] tFilter )
		{
			Refresh( tText, tSettings ) ;

			if( tFilter == null || tFilter.Length == 0 )
			{
				return m_ExtraTagEvent.ToArray() ;
			}

			List<RichText.ExtraTagEvent> tExtraTagEvent = new List<RichText.ExtraTagEvent>() ;

			string tTagName ;
			int i, j, l = m_ExtraTagEvent.Count, m = tFilter.Length ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				tTagName  = m_ExtraTagEvent[ i ].tagName ;
				for( j  = 0 ; j <  m ; j ++ )
				{
					if( tTagName == tFilter[ j ] )
					{
						break ;
					}
				}

				if( j <  m )
				{
					tExtraTagEvent.Add( m_ExtraTagEvent[ i ] ) ;
				}
			}

			return tExtraTagEvent.ToArray() ;
		}

		/// <summary>
		/// キャレットの座標を取得する
		/// </summary>
		/// <param name="tLength"></param>
		/// <returns></returns>
		public Vector2 GetCaretPosition( string tText, RichTextGenerationSettings tSettings, int tLength, int tStartOffsetOfView, int tEndOffsetOfView )
		{
			Refresh( tText, tSettings ) ;

			CodeBase tCode ;
			Vector2 tResult = Vector2.zero ;

			if( tLength <  0 )
			{
				tCode = GetTargetCode( tEndOffsetOfView ) ;
				if( tCode == null )
				{
					return Vector2.zero ;	// どうにもならない
				}
				
				tResult.x = tCode.x + ( tCode.width * m_BestFitResizeScale ) ;
				tResult.y = tCode.y - ( m_BaseFontSize * m_BestFitResizeScale ) ;
			}
			else
			if( tLength <= tStartOffsetOfView )
			{
				// 表示する文字が無い

				tCode = GetTargetCode( tStartOffsetOfView + 1 ) ;
				if( tCode == null )
				{
					return Vector2.zero ;	// どうにもならない
				}

				tResult.x = tCode.x ;
				tResult.y = tCode.y - ( m_BaseFontSize * m_BestFitResizeScale ) ;
			}
			else
			if( tLength >= tEndOffsetOfView )
			{
				// 表示外
				tCode = GetTargetCode( tEndOffsetOfView ) ;
				if( tCode == null )
				{
					return Vector2.zero ;	// どうにもならない
				}

				tResult.x = tCode.x ;
				tResult.y = tCode.y - ( m_BaseFontSize * m_BestFitResizeScale ) ;
			}
			else
			{
				tCode = GetTargetCode( tLength ) ;
				if( tCode == null )
				{
					return Vector2.zero ;	// どうにもならない
				}

				tResult.x = tCode.x + ( tCode.width * m_BestFitResizeScale ) ;
				tResult.y = tCode.y - ( m_BaseFontSize * m_BestFitResizeScale ) ;
			}

//			tResult.x = tResult.x * m_BestFitResizeScale ;
//			tResult.y = tResult.y * m_BestFitResizeScale ;

			return tResult ;
		}

		// 指定した表示文字数に該当する文字情報を取得する
		private CodeBase GetTargetCode( int tLength )
		{
			if( tLength <= 0 )
			{
				return null ;
			}

			int i, l = m_Code.Count ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_Code[ i ].value != 0 )
				{
					tLength -- ;
					if( tLength == 0 )
					{
						// 該当を発見した
						return m_Code[ i ] ;
					}
				}
			}

			return null ;	// それ以外は無効
		}



		//-----------------------------------------------------------------------------------------

		/// <summary>
		/// タグ情報
		/// </summary>
		public class TagData
		{
			public string	name ;
			public string	value ;

			public int		offset ;
			public int		length ;

			public TagData( string tName, string tValue, int tOffset, int tLength )
			{
				name	= tName ;
				value	= tValue ;

				offset	= tOffset ;
				length	= tLength ;
			}
		}
	
		public class CodeBase
		{
			public char			value		= ( char )0 ;
			public bool			space		= false ;

			public Vector2[]	vd		= new Vector2[ 4 ] ;
			public Vector2[]	td		= new Vector2[ 4 ] ;

			public float		advance ;


			public float		width ;
			public float		height ;

			public Color		color	= Color.white ;	// カラー

			public float		x ;		// 最終的な左上のＸ座標
			public float		y ;		// 最終的な左上のＹ座標


			public void GetGeometryBase( Font tFont, int tFontSize, FontStyle tFontStyle, RichTextGenerator tOwner )
			{
				// Unity2018.2 の一時的なバグ対応(存在するフォントまで存在しないと返される→例 気)
				if( value == 0 /* || tFont.HasCharacter( value ) == false */ )
				{
					return ;	// 無効
				}

				CharacterInfo tCI ;

				float vx0, vy0, vx1, vy1, tAdvance ;

				if( tFont.dynamic == false )
				{
					// スタティック
					tFont.GetCharacterInfo( value, out tCI ) ;
				
					float tRatio = ( float )tFontSize / ( float )tFont.fontSize ;

					vx0 = (                    tCI.minX ) * tRatio ;
					vy0 = ( - tFont.fontSize + tCI.maxY ) * tRatio ;
					vx1 = (                    tCI.maxX ) * tRatio ;
					vy1 = ( - tFont.fontSize + tCI.minY ) * tRatio ;

					tAdvance = tCI.advance * tRatio ;
				}
				else
				{
					// ダイナミック
					tFont.GetCharacterInfo( value, out tCI, tFontSize, tFontStyle ) ;

					vx0 =               tCI.minX ;
					vy0 = - tFontSize + tCI.maxY ;
					vx1 =               tCI.maxX ;
					vy1 = - tFontSize + tCI.minY ;

					tAdvance = tCI.advance ;
				}


				vd[ 0 ] = new Vector2( vx0, vy0 ) ;
				vd[ 1 ] = new Vector2( vx1, vy0 ) ;
				vd[ 2 ] = new Vector2( vx1, vy1 ) ;
				vd[ 3 ] = new Vector2( vx0, vy1 ) ;

				if( tCI.uvBottomLeft.x != tCI.uvBottomRight.x )
				{
					// flip : flase

					td[ 0 ] = new Vector2( tCI.uvBottomLeft.x,	tCI.uvTopLeft.y		) ;
					td[ 1 ] = new Vector2( tCI.uvBottomRight.x,	tCI.uvTopLeft.y		) ;
					td[ 2 ] = new Vector2( tCI.uvBottomRight.x,	tCI.uvBottomLeft.y	) ;
					td[ 3 ] = new Vector2( tCI.uvBottomLeft.x,	tCI.uvBottomLeft.y	) ;

				}
				else
				{
					// flip : true

					td[ 0 ] = new Vector2( tCI.uvTopLeft.x,		tCI.uvBottomLeft.y	) ;
					td[ 1 ] = new Vector2( tCI.uvTopLeft.x,		tCI.uvBottomRight.y	) ;
					td[ 2 ] = new Vector2( tCI.uvBottomLeft.x,	tCI.uvBottomRight.y	) ;
					td[ 3 ] = new Vector2( tCI.uvBottomLeft.x,	tCI.uvBottomLeft.y	) ;
				}
			
				advance = tAdvance ;

				//-------------------------------------------------
				// 横幅

				// 横幅
				if( tOwner.IsIgnoreLetterSpace( value ) == true )
				{
					// 横幅はフォントの横幅に従う
					width = advance ;
				}
				else
				{
					// 横幅は最低横幅に従う
					int tFontWidth ;
					if( tOwner.IsHankaku( value ) == true )
					{
						// 半角コード
						tFontWidth = tFontSize / 2 ;
					}
					else
					{
						// 全角コード
						tFontWidth = tFontSize ;
					}

					if( tFontWidth >  advance )
					{
						width = tFontWidth ;
					}
					else
					{
						// フォントの横幅の方が最低横幅より大きい
						width = advance ;
					}
				}

				// 縦幅
				height = tFontSize ;
			}
		}

		public class Code : CodeBase
		{
			public int		size		=  0 ;			// サイズ 
			public int		ruby		= -1 ;			// 関連するルビ(-1で関連するルビは無し)
			public int		em			= -1 ;			// 傍点

			public bool		b			= false ;		// ボールド
			public bool		i			= false ;		// イタリック
			public bool		u			= false ;		// アンダーライン
			public bool		strike		= false ;		// 取り消し線
			public bool		sup			= false ;		// 上つき文字
			public bool		sub			= false ;		// 下つき文字
			public bool		dash		= false ;		// ダーシ
			public int		group		= -1 ;			// グループ

			public bool		newline		= false ;		// 改行

			public int		emoji		= 0 ;			// 絵文字

			public string	uniqueName	= "" ;			// 独自タグの名前
			public string	uniqueValue = "" ;			// 独自タグの値

			public Code(){}

			public Code( char tValue, Color tBaseColor )
			{
				value = tValue ;
				color = tBaseColor ;
			}

			/// <summary>
			/// 頂点とＵＶ情報を取得する
			/// </summary>
			/// <param name="tSettings"></param>
			/// <returns></returns>
			public bool GetGeometry( Font tFont, int tBaseFontSize, FontStyle tBaseFontStyle, RichTextGenerator tOwner )
			{
				// Unity2018.2 の一時的なバグ対応(存在するフォントまで存在しないと返される→例 気)
				if( value == 0 /* || tFont.HasCharacter( value ) == false */ )
				{
					return false ;	// 異常
				}

				int tFontSize ;
				FontStyle tFontStyle ;

				if( size <= 0 )
				{
					tFontSize = tBaseFontSize ;
				}
				else
				{
					tFontSize = size ;
				}

				if( b == true  && i == false )
				{
					tFontStyle = FontStyle.Bold ;
				}
				else
				if( b == false && i == true )
				{
					tFontStyle = FontStyle.Italic ;
				}
				else
				if( b == true && i == true )
				{
					tFontStyle = FontStyle.BoldAndItalic ;
				}
				else
				{
					tFontStyle = tBaseFontStyle ;
				}

				if( dash == false )
				{
					// 頂点情報を取得
					GetGeometryBase( tFont, tFontSize, tFontStyle, tOwner ) ;
				}
				else
				{
					// ダーシの場合は例外処理を行う(頂点情報も取得しない)

					int tFontWidth ;
					if( tOwner.IsHankaku( value ) == true )
					{
						// 半角コード
						tFontWidth = tFontSize / 2 ;
					}
					else
					{
						// 全角コード
						tFontWidth = tFontSize ;
					}

					// 横幅はダーシキャラのコードの大きさ種別(半角・全角)に従い固定(横スペースあるなしも無視する)
					width = tFontWidth ;
					height = tFontSize ;
				}

				return true ;
			}
		}

		public class Word
		{
			public string	text ;
			public int		length ;	// 対象の文字数

			public Color	color = Color.white ;

			public List<CodeBase>	code = new List<CodeBase>() ;

			public Word( string tText, int tLength, int tFontSize, Color tBaseColor )
			{
				text	= tText ;
				length	= tLength ;
				color	= tBaseColor ;

				int i, l = text.Length ;
				char c ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					c = text[ i ] ;
					
					if( c != '\n' )
					{
						if( c != ' ' && c != '　' )
						{
							CodeBase w = new CodeBase() ;

							w.value = c ;
							w.color	= color ;

							code.Add( w ) ;
						}
					}
					else
					if( c == ' ' )
					{
						CodeBase w = new CodeBase() ;
						
						w.space = true ;
						w.width = tFontSize * 0.5f ;

						code.Add( w ) ;
					}
					else
					{
						CodeBase w = new CodeBase() ;
							
						w.space = true ;
						w.width = tFontSize ;

						code.Add( w ) ;
					}
				}
			}

			/// <summary>
			/// 頂点とＵＶ情報を取得する
			/// </summary>
			/// <param name="tSettings"></param>
			/// <returns></returns>
			public void GetGeometry( Font tFont, int tFontSize, FontStyle tFontStyle, RichTextGenerator tOwner )
			{
				int i, l = code.Count ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( code[ i ].value != 0 )
					{
						code[ i ].GetGeometryBase( tFont, tFontSize, tFontStyle, tOwner ) ;
					}
				}
			}

			// ルビや傍点のトータルの横幅を取得する
			public float GetWidth()
			{
				int i, l = code.Count ;
				float tWidth = 0 ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					tWidth = tWidth + code[ i ].width ;
				}

				return tWidth ;
			}
		}


		// リッチテキストをパースする
		private void Parse( string tText, RichTextGenerationSettings tSettings )
		{
			m_Code.Clear() ;
			m_Ruby.Clear() ;
			m_Em.Clear() ;
			m_Group = -1 ;

			if( string.IsNullOrEmpty( tText ) == true )
			{
				return ;
			}

			//------------------------------------------------------------------

			int i, j, o, p, s, t, e, l = tText.Length ;
			char c ;
			bool r ;
			Code tCode ;

			if( tSettings.richText == false )
			{
				// リッチテキストの処理は無効になっている
				for( i  = 0 ; i <  l ; i ++ )
				{
					c = tText[ i ] ;
					if( c != '\n' )
					{
						// 改行以外
						if( c != ' ' && c != '　' )
						{
							m_Code.Add( new Code( c, tSettings.color ) ) ;
						}
						else
						if( c == ' ' )
						{
							tCode = new Code() ;
							tCode.space = true ;
							tCode.width = m_BaseFontSize * 0.5f ;
							m_Code.Add( tCode ) ;
						}
						else
						if( c == '　' )
						{
							tCode = new Code() ;
							tCode.space = true ;
							tCode.width = m_BaseFontSize ;
							m_Code.Add( tCode ) ;
						}
						else
						if( c == '\\' )
						{
							r = false ;
							if( ( i + 1 ) <  l )
							{
								if( tText[ i + 1 ] == 'n' )
								{
									// これも改行
									tCode = new Code() ;
									tCode.newline = true ;
									m_Code.Add( tCode ) ;

									i ++ ;
									r = true ;
								}
							}

							if( r == false )
							{
								m_Code.Add( new Code( c, tSettings.color ) ) ;
							}
						}
					}
					else
					{
						tCode = new Code() ;
						tCode.newline = true ;
						m_Code.Add( tCode ) ;
					}
				}

				return ;
			}

			//------------------------------------------------------------------

			List<TagData> tTagData = new List<TagData>() ;
			string tTagName = null ;
			string tTagValue = null ;
			bool tClose ;

			o = 0 ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				// タグの開始をチェックする
				t = o ;
				if( tText[ o ] == '\\' )
				{
					// エスケープコード
					if( ( o + 1 ) <  l )
					{
						o ++ ;
						if( tText[ o ] == 'n' )
						{
							// 文字タイプの改行
							tCode = new Code() ;
							tCode.newline = true ;

							m_Code.Add( tCode ) ;
							o ++ ;
						}
						else
						{
							// 改行でない場合はエスケープをスキップし次の文字を強制的に１文字として格納する
							PutCode( tText, tSettings.color, ref o ) ;
						}
					}
					else
					{
						// \ 文字
						PutCode( tText, tSettings.color, ref o ) ;
					}
				}
				else
				if( CheckTagOfStart( tText, ref o, ref tTagName, ref tTagValue ) == true )
				{
					// タグ開始

					if( tTagName == "dash" )
					{
						// 単独タイプ
						int tSize = -1 ;
						if( int.TryParse( tTagValue, out tSize ) == true )
						{
							for( j  = 0 ; j < tSize ; j ++ )
							{
								tCode = new Code( m_DashChar, tSettings.color ) ;	// 後で文字を選択出来るようにする
								tCode.dash = true ;

								m_Code.Add( tCode ) ;
							}
						}
					}
					else
					if( tTagName == "space" )
					{
						// 単独タイプ
						int tSize = -1 ;
						if( int.TryParse( tTagValue, out tSize ) == true )
						{
							tCode = new Code() ;
							tCode.space = true ;
							tCode.width = tSize ;

							m_Code.Add( tCode ) ;
						}
					}
					else
					if( tTagName == "emoji" )
					{
						// 単独タイプ
						int tSize = -1 ;
						if( int.TryParse( tTagValue, out tSize ) == true )
						{
							tCode = new Code() ;
							tCode.emoji = tSize ;

							m_Code.Add( tCode ) ;
						}
					}
					else
					if( tTagName == "color" || tTagName == "size" || tTagName == "ruby" || tTagName == "em" || tTagName == "b" || tTagName == "i" || tTagName == "u" || tTagName == "strike" || tTagName == "sup" || tTagName == "sub" || tTagName == "group" )
					{
						// 開始～終了のタイプ
	//					Debug.LogWarning( "タグ開始:" + tTagName + " " + t + " " + o ) ;
						tTagData.Add( new TagData( tTagName, tTagValue, m_Code.Count, o - t ) ) ;

						// きちんとタグが閉じるという保証がないのでここまでも表示文字列に格納しておく
						p = t ;
						for( j  = t ; j <  o ; j ++ )
						{
							// 表示文字
							PutCode( tText, tSettings.color, ref p ) ;
			
							if( p >= l )
							{
								break ;	// パース終了
							}
						}
					}
					else
					{
						// その他の独自タグ(単独タイプ限定)
						tCode = new Code() ;
						tCode.uniqueName	= tTagName ;
						tCode.uniqueValue	= tTagValue ;

						m_Code.Add( tCode ) ;
					}
				}
				else
				if( CheckTagOfEnd( tText, ref o, ref tTagName ) == true )
				{
					// タグ終了
					tClose = false ;
					if( tTagData.Count >  0 )
					{
						TagData tActiveTag = tTagData[ tTagData.Count - 1 ] ;
	
//						Debug.LogWarning( "タグ終了:" + tTagName ) ;
	
						if( tActiveTag.name == tTagName )
						{
							// アクティブなタグは有効
	
							// 開始タグの文字列の箇所を除去する
							s = tActiveTag.offset ;
							e = s + tActiveTag.length - 1 ;
							for( j  = e ; j >= s ; j -- )
							{
								m_Code.RemoveAt( j ) ;
							}
	
							// アクティブなタグに関わる文字列のステータスを全て設定
							ProcessActiveTag( tActiveTag, s, tSettings ) ;
	
							// アクティブなタグを除去する
							tTagData.Remove( tActiveTag ) ;

							// 正しくタグが閉じられた
							tClose = true ;
						}
					}

					if( tClose == false )
					{
						// 閉じられなかった終了タグを文字列として保存して次へ

						// きちんとタグが閉じるという保証がないのでここまでも表示文字列に格納しておく
						p = t ;
						for( j  = t ; j <  o ; j ++ )
						{
							// 表示文字
							PutCode( tText, tSettings.color, ref p ) ;
							if( p >= l )
							{
								break ;	// パース終了
							}
						}
					}
				}
				else
				{
					// タグが処理された直後は文字の登録は行わない

					// 表示文字
					PutCode( tText, tSettings.color, ref o ) ;
				}

				if( o >= l )
				{
					break ;	// パース終了
				}
			}
		}

		// １文字を格納する
		protected void PutCode( string tText, Color tBaseColor, ref int rIndex )
		{
			// 表示文字
			char c = GetCode( tText, ref rIndex ) ;

			Code tCode = null ;
			
			if( c != '\n' )
			{
				if( c == ' ' )
				{
					tCode = new Code() ;
					tCode.space = true ;
					tCode.width = m_BaseFontSize * 0.5f ;
				}
				else
				if( c == '　' )
				{
					tCode = new Code() ;
					tCode.space = true ;
					tCode.width = m_BaseFontSize ;
				}
				else
				{
					tCode = new Code( c, tBaseColor ) ;
				}
			}	
			else
			{
				tCode = new Code() ;
				tCode.newline = true ;
			}

			m_Code.Add( tCode ) ;
		}

		// １文字を取得する
		protected char GetCode( string tText, ref int rIndex )
		{
			int l = tText.Length ;
			int o = rIndex ;

			if( o >= l )
			{
				return ( char )0 ;	// ここへ来るような状態はまずい
			}

			char c = tText[ o ] ;
			o ++ ;

			if( c == '\\' )
			{
				if( o <  l )
				{
					if( tText[ o ] == 'r' )
					{
						// 改行
						c = '\n' ;
						o ++ ;
					}
				}
			}

			// インデックスを更新
			rIndex = o ;

			return c ;
		}

		// タグ開始を取得する
		protected bool CheckTagOfStart( string tText, ref int rIndex, ref string rName, ref string rValue )
		{
			int o = rIndex ;

			o = CheckCode( tText, o, '<' ) ;
			if( o <  0 )
			{
				return false ;	// タグではない
			}

			// スペースをスキップする
			o = SkipSpace( tText, o ) ;
			if( o <  0 )
			{
				return false ;	// タグではない
			}

			if( tText[ o ] == '/' )
			{
				return false ;	// 開始タグでは絶対に最初にスラッシュはこない
			}

			// タグ名を取得する
			string tName = GetWord( tText, ref o, ' ', '=', '>' ) ;
			if( string.IsNullOrEmpty( tName ) == true )
			{
	//			Debug.LogWarning( "開始タグではない" ) ;
				return false ;	// タグではない
			}

			tName = tName.ToLower() ;	// 小文字化

//			Debug.LogWarning( "==========タグ名称:" + tName ) ;

			if( tName == "color" || tName == "size" || tName == "ruby" || tName == "em" || tName == "dash" || tName == "space" || tName == "emoji" )
			{
				// パラメータが有るタイプ
				bool tIsContinue = true ;
				if( tName == "dash" || tName == "space" || tName == "emoji" )
				{
					tIsContinue = false ;
				}

				if( IsTagClosed( tName, tText, ref o, ref rValue, tIsContinue ) == false )
				{
					return false ;	// タグではない
				}
			}
			else
			if( tName == "b" || tName == "i" || tName == "u" || tName == "strike" || tName == "sup" || tName == "sub" || tName == "group" )
			{
				// パラメータが無いタイプ
				if( IsTagClosed( tText, ref o ) == false )
				{
					return false ;	// タグではない
				}
			}
			else
			{
				// その他の独自タグ

				if( IsTagClosed( tName, tText, ref o, ref rValue, false ) == false )
				{
					rValue = "" ;
					if( IsTagClosed( tText, ref o ) == false )
					{
						return false ;	// タグではない
					}
				}
			}

//			Debug.LogWarning( "開始タグと認識:" + tName ) ;

			// インデックスを更新
			rIndex = o ;

			// タグ名を保存
			rName = tName ;

			return true ;
		}


		// タグ終了を取得する
		private bool CheckTagOfEnd( string tText, ref int rIndex, ref string rName )
		{
			int o = rIndex ;

			o = CheckCode( tText, o, '<' ) ;
			if( o <  0 )
			{
				return false ;	// タグではない
			}

			// スペースをスキップする
			o = SkipSpace( tText, o ) ;
			if( o <  0 )
			{
				return false ;	// タグではない
			}

			o = CheckCode( tText, o, '/' ) ;
			if( o <  0 )
			{
	//			Debug.LogWarning( "開始タグではない" ) ;
				return false ;	// タグではない
			}

			// スペースをスキップする
			o = SkipSpace( tText, o ) ;
			if( o <  0 )
			{
				return false ;	// タグではない
			}

			// タグ名を取得する
			string tName = GetWord( tText, ref o, ' ', '>' ) ;
			if( string.IsNullOrEmpty( tName ) == true )
			{
				return false ;	// タグではない
			}

			tName = tName.ToLower() ;	// 小文字化

			// パラメータが無いタイプ
			if( IsTagClosed( tText, ref o, false ) == false )
			{
				return false ;	// タグではない
			}

			// インデックスを更新
			rIndex = o ;

			// タグ名を保存
			rName = tName ;

			return true ;
		}


		// パラメータが有る場合のタグ開始終端の確認
		private bool IsTagClosed( string tName, string tText, ref int rIndex, ref string rValue, bool tIsContinue )
		{
			int l = tText.Length ;
			int o = rIndex ;

			// スペースをスキップする
			o = SkipSpace( tText, o ) ;
			if( o <  0 )
			{
				return false ;	// 異常
			}
		
			o = CheckCode( tText, o, '=' ) ;
			if( o <  0 )
			{
				return false ;	// 異常
			}

			// スペースをスキップする
			o = SkipSpace( tText, o ) ;
			if( o <  0 )
			{
				return false ;	// 異常
			}
		
			// パラメータの最初
			char c = tText[ o ] ;

			if( c == '>' )
			{
				return false ;	// 異常(いきなり終わってはダメ)
			}

			// 文字列定義かの確認
			string v = null ;

			if( c == '"' )
			{
				// スペースを含む文字列

				// ダブルクォート開き
				o ++ ;
				if( o >= l )
				{
					return false ;	// 異常
				}

				v = GetWord( tText, ref o, '"' ) ;
				if( string.IsNullOrEmpty( v ) == true )
				{
					return false ;	// 異常
				}

				// ダブルクォート閉じ
				o ++ ;
				if( o >= l )
				{
					// 異常
					return false ;
				}
			}
			else
			{
				if( tName != "ruby" )
				{
					// スペースを含まない文字列
					v = GetWord( tText, ref o, ' ', '>' ) ;
				}
				else
				{
					// 例外的にスペースを含んでも良い(ただしパラメータ数が１つ)
					v = GetWord( tText, ref o, '>' ) ;
				}

				if( string.IsNullOrEmpty( v ) == true )
				{
					return false ;	// 異常
				}
			}
			
			// タグの開始の終端
			if( IsTagClosed( tText, ref o, tIsContinue ) == false )
			{
				return false ;	// 異常
			}

			// インデックスを更新
			rIndex = o ;

			// パラメータ値
			rValue = v ;

			return true ;
		}

		// パラメータが無い場合のタグ開始終端の確認
		private bool IsTagClosed( string tText, ref int rIndex, bool tIsContinue = true )
		{
			int l = tText.Length ;
			int o = rIndex ;

			// スペースをスキップする
			o = SkipSpace( tText, o ) ;
			if( o <  0 )
			{
				return false ;	// 異常
			}

			if( tText[ o ] != '>' )
			{
				return false ;	// 異常
			}

			o ++ ;

			if( tIsContinue == true )
			{
				if( o >= l )
				{
					return false ;	// 異常
				}
			}

			// インデックスを更新
			rIndex = o ;

			return true ;
		}

		// 指定の文字かどうか確認する
		private int CheckCode( string tText, int tIndex, char tCode, bool tIsContinue = true )
		{
			int l = tText.Length ;
			int o = tIndex ;

			if( tText[ o ] != tCode )
			{
				return -1 ;	// 異常
			}

			o ++ ;

			if( tIsContinue == true )
			{
				if( o >= l )
				{
					return -1 ;	// 異常
				}
			}

			return o ;
		}


		// スペースをスキップする
		private int SkipSpace( string tText, int tIndex, bool tIsContinue = true )
		{
			int i, l = tText.Length ;

			// スペース以外の文字
			for( i  = tIndex ; i <  l ; i ++ )
			{
				if( tText[ i ] != ' ' )
				{
					break ;
				}
			}

			if( tIsContinue == true )
			{
				if( i >= l )
				{
					return -1 ;	// スペースが最後まで続くのは異常
				}
			}

			return i ;
		}

		// 指定の文字が見つかるまでの文字列を取得する
		private string GetWord( string tText, ref int rIndex, params char[] tCode )
		{
			int i, l = tText.Length ;
			int o = rIndex ;

			int j, m = tCode.Length ;
			char c ;

			for( i  = o ; i <  l ; i ++ )
			{
				c = tText[ i ] ;
				if( c == '\\' )
				{
					// エスケープ
					c ++ ;
					continue ;
				}

				for( j  = 0 ; j <  m ; j ++ )
				{
					if( c == tCode[ j ] )
					{
						break ;
					}
				}
				if( j <  m )
				{
					break ;
				}
			}

			if( i >= l )
			{
				return null ;	// 区切りが見つからないので異常
			}

			if( ( i - o ) <= 0 )
			{
				return null ;	// 文字が空なので異常
			} 

			// インデックスを更新
			rIndex = i ;

			return tText.Substring( o, i - o ) ;
		}


		private static Dictionary<string,Color> m_StandardColor = new Dictionary<string, Color>()
		{
			{ "black",		Color.black		},
			{ "red",		Color.red		},
			{ "green",		Color.green		},
			{ "yellow",		Color.yellow	},
			{ "blue",		Color.blue		},
			{ "magenta",	Color.magenta	},
			{ "cyan",		Color.cyan		},
			{ "white",		Color.white		},
			{ "gray",		Color.gray		},
			{ "grey",		Color.grey		},	
		} ;

		private bool ParseColor( string tARGB, ref Color rColor )
		{
			tARGB = tARGB.ToLower() ;

			int i, l = tARGB.Length ;
			char c ;

	//		Debug.LogWarning( "COLOR:" + tARGB ) ;

			int p = 3 ;
			int v = 0 ;
			int b = 0 ;

			int[] e = { 255,   0,   0,   0 } ;

			for( i  =  l - 1 ; i >= 0 ; i -- )
			{
				c = tARGB[ i ] ;
				if( c >= '0' && c <= '9' )
				{
					v = v + ( ( c - '0'      ) * ( 1 << ( b * 4 ) ) ) ;
				}
				else
				if( c >= 'a' && c <= 'f' )
				{
					v = v + ( ( c - 'a' + 10 ) * ( 1 << ( b * 4 ) ) ) ;
				}
				else
				if( c == '#')
				{
					break ;	// 終了
				}
				else
				{
					return false ;	// 不正な文字発覚
				}

				b ++ ;
				if( b == 2 )
				{
					b = 0 ;

					e[ p ] = v ;
					v = 0 ;
					p -- ;
				}

				if( p <  1 )
				{
					break ;
				}
			}

			rColor.r = ( float )e[ 1 ] / 255.0f ;
			rColor.g = ( float )e[ 2 ] / 255.0f ;
			rColor.b = ( float )e[ 3 ] / 255.0f ;
			rColor.a = 1.0f ;

			return true ;
		}


		// タグの情報を反映させる
		private void ProcessActiveTag( TagData tActiveTag, int tIndex, RichTextGenerationSettings tSettings )
		{
			int i, l = m_Code.Count ;
			int o = tIndex ;

			if( o >= l )
			{
//				Debug.LogWarning( "タグの対象が空:" + tActiveTag.name ) ;
				return ;	// タグの対象が空
			}

			string tName  = tActiveTag.name ;
			string tValue = tActiveTag.value ;

	//		Debug.LogWarning( "========処理対象タグ名:" + tName ) ;

			if( tName == "color" )
			{
				// カラー
				Color tColor = Color.white ;

				tValue = tValue.ToLower() ;
				if( m_StandardColor.ContainsKey( tValue ) == true )
				{
					tColor = m_StandardColor[ tValue ] ;
				}
				else
				{
					if( ParseColor( tValue, ref tColor ) == false )
					{
						// 不正な文字が混入
						return ;
					}
				}

				for( i  = o ; i <  l ; i ++ )
				{
					m_Code[ i ].color = tColor ;
				}
			}
			else
			if( tName == "size" )
			{
				// サイズ
				int tSign = 0 ;

				tValue = tValue.Replace( " ", "" ) ;
				if( tValue.Length >= 2 )
				{
					if( tValue[ 0 ] == '+' )
					{
						tSign =  1 ;
						tValue = tValue.Substring( 1, tValue.Length - 1 ) ;
					}
					else
					if( tValue[ 0 ] == '-' )
					{
						tSign  = -1 ;
						tValue = tValue.Substring( 1, tValue.Length - 1 ) ;
					}
				}

				int tSize = -1 ;
				if( int.TryParse( tValue, out tSize ) == false )
				{
					// 不正な文字が混入
					return ;
				}

				if( tSign >  0 )
				{
					tSize = m_BaseFontSize + tSize ;
				}
				else
				if( tSign <  0 )
				{
					tSize = m_BaseFontSize - tSize ;
				}

				if( tSize <  1 )
				{
					tSize =  1 ;	// 最低１
				}

				for( i  = o ; i <  l ; i ++ )
				{
					m_Code[ i ].size = tSize ;	// Sup Sub より　Size の値を優先する
				}
			}
			else
			if( tName == "ruby" )
			{
				// ルビ
				int r = m_Ruby.Count ;

				// ルビ文字内の改行等は無効
				tValue = tValue.Replace( "\n", "" ) ;

				// ルビの場合は禁則処理の区切りが異なるため同じ文字列でも異なるルビとして扱う必要がある
				m_Ruby.Add( new Word( tValue, l - o, ( int )( tSettings.fontSize * tSettings.rubySizeScale ), tSettings.color ) ) ;

				for( i  = o ; i <  l ; i ++ )
				{
					m_Code[ i ].ruby = r ;
				}
			}
			else
			if( tName == "em" )
			{
				// 傍点

				// 傍点の場合は禁則処理に関係しないため既に同じ文字列が登録されているならそれを使う
				int r = m_Em.Count ;

				tValue = tValue.Replace( "\n", "" ) ;

				for( i  = 0 ; i <  r ; i ++ )
				{
					if( m_Em[ i ].text == tValue )
					{
						// 既に登録済み
						break ;
					}
				}

				if( i <  r )
				{
					// 既に登録済み
					r = i ;
				}
				else
				{
					// 初めて登録
					m_Em.Add( new Word( tValue, 1, ( int )( tSettings.fontSize * tSettings.rubySizeScale ), tSettings.color ) ) ;	// 対象は常に１文字
				}

				for( i  = o ; i <  l ; i ++ )
				{
					m_Code[ i ].em = r ;
				}
			}
			else
			if( tName == "b" )
			{
				// ボールド
				for( i  = o ; i <  l ; i ++ )
				{
					m_Code[ i ].b = true ;
				}
			}
			else
			if( tName == "i" )
			{
				// イタリック
				for( i  = o ; i <  l ; i ++ )
				{
					m_Code[ i ].i = true ;
				}
			}
			else
			if( tName == "u" )
			{
				// アンダーライン
				for( i  = o ; i <  l ; i ++ )
				{
					m_Code[ i ].u = true ;
				}
			}
			else
			if( tName == "strike" )
			{
				// 取り消し線
				for( i  = o ; i <  l ; i ++ )
				{
					m_Code[ i ].strike = true ;
				}
			}
			else
			if( tName == "sup" )
			{
				// 上つき文字
				int tSize = ( int )( tSettings.fontSize * tSettings.supOrSubSizeScale ) ;
				for( i  = o ; i <  l ; i ++ )
				{
					if( m_Code[ i ].size == 0 )
					{
						m_Code[ i ].size = tSize ;	// size が設定されていない場合のみ設定する
					}
					m_Code[ i ].sup = true ;
				}
			}
			else
			if( tName == "sub" )
			{
				// 下つき文字
				int tSize = ( int )( tSettings.fontSize * tSettings.supOrSubSizeScale ) ;

				for( i  = o ; i <  l ; i ++ )
				{
					if( m_Code[ i ].size == 0 )
					{
						m_Code[ i ].size = tSize ;	// size が設定されていない場合のみ設定する
					}
					m_Code[ i ].sub = true ;
				}
			}
			else
			if( tName == "group" )
			{
				// グループ
				m_Group ++ ;
				for( i  = o ; i <  l ; i ++ )
				{
					m_Code[ i ].group = m_Group ;
				}
			}

		}

		// 途中改行禁止文字かの確認
		protected bool IsKinsokuCode( char tCode )
		{
			if( tCode == 0 )
			{
				return false ;
			}

			int i, l = m_KinsokuCode.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_KinsokuCode[ i ] == tCode )
				{
					return true ;	// 該当文字
				}
			}

			return false ;
		}

		// 行頭禁止文字かの確認
		protected bool IsKinsokuTop( char tCode )
		{
			if( tCode == 0 )
			{
				return false ;
			}

			int i, l = m_KinsokuTop.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_KinsokuTop[ i ] == tCode )
				{
					return true ;	// 該当文字
				}
			}

			return false ;
		}

		// 行末禁止文字かの確認
		protected bool IsKinsokuEnd( char tCode )
		{
			if( tCode == 0 )
			{
				return false ;
			}

			int i, l = m_KinsokuEnd.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_KinsokuEnd[ i ] == tCode )
				{
					return true ;	// 該当文字
				}
			}

			return false ;
		}
	
		// 最低文字横幅を無視する対象かの確認
		public bool IsIgnoreLetterSpace( char tCode )
		{
			if( tCode == 0 )
			{
				return false ;
			}

			int i, l = m_IgnoreLetterSpace.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_IgnoreLetterSpace[ i ] == tCode )
				{
					return true ;	// 該当文字
				}
			}

			return false ;
		}

		// 文字が半角かどうかの判定
		public bool IsHankaku( char tCode )
		{
			if
			(
				  tCode <  0x007E ||							// 英数字
				  tCode == 0x00A5 ||							// 記号 \
				  tCode == 0x203E ||							// 記号 ~
				( tCode >= 0xFF61 && tCode <= 0xFF9F )			// 半角カナ
			)
			{
				// 半角
				return true ;
			}

			// 全角
			return false ;
		}
	}
}

