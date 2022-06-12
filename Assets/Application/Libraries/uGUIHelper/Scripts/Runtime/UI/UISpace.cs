using UnityEngine ;
using System.Collections ;

using uGUIHelper ;

namespace uGUIHelper
{
	/// <summary>
	/// ＵＩの一部として表示したいカメラを簡単につくるためのコンポーネント（クリッピングは効かない事に注意）
	/// </summary>
	public class UISpace : UIView
	{
		[SerializeField][HideInInspector]
		private Camera m_TargetCamera = null ;
		public  Camera   TargetCamera
		{
			get
			{
				return m_TargetCamera ;
			}
			set
			{
				m_TargetCamera = value ;
			}
		}
		
		/// <summary>
		/// カメラデプスを設定する
		/// </summary>
		/// <param name="tDepth"></param>
		/// <returns></returns>
		public bool SetCameraDepth( int depth )
		{
			if( m_TargetCamera == null )
			{
				return false ;
			}

			m_TargetCamera.depth = depth ;

			return true ;
		}

		/// <summary>
		/// カリングマスクを設定する
		/// </summary>
		/// <param name="tMask"></param>
		/// <returns></returns>
		public bool SetCullingMask( int cullingMask )
		{
			if( m_TargetCamera == null )
			{
				return false ;
			}

			m_TargetCamera.cullingMask = cullingMask ;

			return true ;
		}

		[SerializeField][HideInInspector]
		private bool	m_RenderTextureEnabled = false ;
		public  bool	  RenderTextureEnabled
		{
			get
			{
				return m_RenderTextureEnabled ;
			}
			set
			{
				if( m_RenderTextureEnabled != value )
				{
					m_RenderTextureEnabled = value ;

					if( Application.isPlaying == true )
					{
						if( value == true )
						{
							CreateRenderTexture() ;
						}
						else
						{
							DeleteRenderTexture() ;
						}
					}
				}
			}
		}

		[SerializeField][HideInInspector]
		private RenderTexture	m_RenderTexture = null ;

		/// <summary>
		/// レンダーテクスチャのインスタンス
		/// </summary>
		public RenderTexture	Texture
		{
			get
			{
				return m_RenderTexture ;
			}
		}

		[SerializeField][HideInInspector]
		private UIRawImage		m_RenderImage = null ;

		/// <summary>
		/// レンダーイメージのインスタンス
		/// </summary>
		public UIRawImage Image
		{
			get
			{
				return m_RenderImage ;
			}
		}


		[SerializeField][HideInInspector]
		private Vector2			m_Size ;

		[SerializeField][HideInInspector]
		private bool			m_FlexibleFieldOfView = true ;
		public  bool			  FlexibleFieldOfView
		{
			get
			{
				return m_FlexibleFieldOfView ;
			}
			set
			{
				m_FlexibleFieldOfView = value ;
			}
		}

		[SerializeField][HideInInspector]
		private float			m_BasisHeight = 0 ;
		public  float			  BasisHeight
		{
			get
			{
				return m_BasisHeight ;
			}
			set
			{
				m_BasisHeight = value ;
			}
		}


		/// <summary>
		/// カメラの焦点の位置
		/// </summary>
		[SerializeField]
		protected Vector2 m_CameraOffset = Vector2.zero ;
		public Vector2	CameraOffset{ get{ return m_CameraOffset ; } set{ m_CameraOffset = value ; } }


		private float m_InitialFieldOfView = 0 ;

		//-----------------------------------------------------------

		// レンダーテクスチャが有効な場合のみ使用可能なパラメータ

		[SerializeField]
		protected bool m_ImageMask = false ;
		public bool	ImageMask{ get{ return m_ImageMask ; } set{ m_ImageMask = value ; } }

		[SerializeField]
		protected bool m_ImageInversion = false ;
		public bool ImageInversion{ get{ return m_ImageInversion ; } set{ m_ImageInversion = value ; } }

		[SerializeField]
		protected bool m_ImageShadow = false ;
		public bool ImageShadow{ get{ return m_ImageShadow ; } set{ m_ImageShadow = value ; } }

		[SerializeField]
		protected bool m_ImageOutline = false ;
		public bool ImageOutline{ get{ return m_ImageOutline ; } set{ m_ImageOutline = value ; } }

		[SerializeField]
		protected bool m_ImageGradient = false ;
		public bool ImageGradient{ get{ return m_ImageGradient ; } set{ m_ImageGradient = value ; } }

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		/// </summary>
		/// <param name="tOption"></param>
		override protected void OnBuild( string option = "" )
		{
			ResetRectTransform() ;

			// ひとまず自身の子としてカメラを生成しておく
			GameObject cameraGameObject = new GameObject( "Camera" ) ;
			
			cameraGameObject.transform.localPosition = Vector3.zero ;
			cameraGameObject.transform.localRotation = Quaternion.identity ;
			cameraGameObject.transform.localScale = Vector3.one ;
			cameraGameObject.transform.SetParent( transform, false ) ;

			m_TargetCamera = cameraGameObject.AddComponent<Camera>() ;
		}


//		override protected void OnAwake()
//		{
//		}

		override protected void OnStart()
		{
			if( Application.isPlaying == true )
			{
				if( m_RenderTextureEnabled == true )
				{
					CreateRenderTexture() ;
					m_Size = Size ;
				}

				if( m_TargetCamera != null )
				{
					m_InitialFieldOfView = m_TargetCamera.fieldOfView ;
				}
			}
		}

		protected override void OnUpdate()
		{
			base.OnUpdate() ;

			//---------------------------------------------------------

			if( m_RenderTextureEnabled == true && m_Size != Size )
			{
				ResizeRenderTexture() ;
				m_Size = Size ;
			}

			if( Application.isPlaying == true )
			{
				if( m_TargetCamera != null )
				{
					if( m_FlexibleFieldOfView == true && m_BasisHeight >  0 )
					{
						// 画面の縦幅に応じて３Ｄカメラの画角を調整し３Ｄの見た目の大きさを画面の縦比に追従させる
						float fov = m_InitialFieldOfView * 0.5f ;
						float distance = ( m_BasisHeight * 0.5f ) / Mathf.Tan( 2.0f * Mathf.PI * fov / 360.0f ) ;
						float height = this.Height * 0.5f ;
						float tanV = height / distance ;
						fov = ( 360.0f * Mathf.Atan( tanV ) / ( 2.0f * Mathf.PI ) ) ;
						m_TargetCamera.fieldOfView = fov * 2.0f ;
					}
				}
			}
		}

		private Rect m_FullRendering = new Rect( 0, 0, 1, 1 ) ;

		override protected void OnLateUpdate()
		{
			base.OnLateUpdate() ;

			// RectTransform の位置に Camera を調整する
			if( m_TargetCamera != null )
			{
				if( m_RenderTextureEnabled == false || Application.isPlaying == false )
				{
					Rect r = RectInCanvas ;

					Vector2 c = GetCanvasSize() ;

					// 画面の左下基準
					r.x += ( c.x * 0.5f ) ;
					r.y += ( c.y * 0.5f ) ;

					float w = c.x ;
					float h = c.y ;

					r.x /= w ;
					r.y /= h ;
					r.width  /= w ;
					r.height /= h ;

					m_TargetCamera.rect = r ;
				}
				else
				{
					m_TargetCamera.rect = m_FullRendering ;
				}

				if( m_TargetCamera != null )
				{
					if( m_CameraOffset.x == 0 && m_CameraOffset.y == 0 )
					{
						m_TargetCamera.ResetProjectionMatrix() ;
//						Debug.LogWarning( "元の行列:\n" + m_TargetCamera.projectionMatrix ) ;
					}
					else
					{
						float sw, sh ;
						if( m_RenderTexture == null )
						{
							sw = Screen.width ;
							sh = Screen.height ;
						}
						else
						{
							sw = m_RenderTexture.width ;
							sh = m_RenderTexture.height ;
						}

						m_TargetCamera.projectionMatrix = PerspectiveOffCenter( m_TargetCamera, sw, sh, m_CameraOffset ) ;
//						Debug.LogWarning( "今の行列:\n" + m_TargetCamera.projectionMatrix ) ;
					}
				}
			}
		}

		override protected void OnDestroy()
		{
			if( m_RenderTextureEnabled == true )
			{
				DeleteRenderTexture() ;
			}
		}

		private void CreateRenderTexture()
		{
			if( m_TargetCamera == null )
			{
				return ;
			}

			m_TargetCamera.clearFlags = CameraClearFlags.SolidColor ;

			if( m_RenderTexture == null && this.Width >  0 && this.Height >  0 )
			{
				m_RenderTexture = new RenderTexture( ( int )this.Width, ( int )this.Height, 24 )
				{
					antiAliasing = 2,
					depth = 24	// 24以上にしないとステンシルがサポートされない事に注意する
				} ;
				m_RenderTexture.Create() ;
			}
			if( m_RenderTexture == null )
			{
				return ;
			}

			if( m_RenderImage == null )
			{
				m_RenderImage = AddView<UIRawImage>() ;
			}
			m_RenderImage.SetAnchorToStretch() ;

			m_TargetCamera.targetTexture = m_RenderTexture ;
			m_RenderImage.Texture = m_RenderTexture ;

			m_RenderImage.IsMask		= m_ImageMask ;
			m_RenderImage.IsInversion	= m_ImageInversion ;
			m_RenderImage.IsShadow		= m_ImageShadow ;
			m_RenderImage.IsOutline		= m_ImageOutline ;
			m_RenderImage.IsGradient	= m_ImageGradient ;
		}

		private void DeleteRenderTexture()
		{
			if( m_TargetCamera != null )
			{
				m_TargetCamera.targetTexture = null ;
			}

			if( m_RenderImage != null )
			{
				m_RenderImage.Texture = null ;
				DestroyImmediate( m_RenderImage.gameObject ) ;
				m_RenderImage = null ;
			}

			if( m_RenderTexture != null )
			{
				DestroyImmediate( m_RenderTexture ) ;
				m_RenderTexture = null ;
			}

			if( m_TargetCamera != null )
			{
				m_TargetCamera.clearFlags = CameraClearFlags.Depth ;
			}
		}

		private void ResizeRenderTexture()
		{
			if( m_TargetCamera == null || m_RenderTexture == null || m_RenderImage == null )
			{
				return ;
			}

			m_TargetCamera.targetTexture = null ;

			m_RenderImage.Texture = null ;

			DestroyImmediate( m_RenderTexture ) ;
			m_RenderTexture = null ;

			if( this.Width >  0 && this.Height >  0 )
			{
				m_RenderTexture = new RenderTexture( ( int )this.Width, ( int )this.Height, 24 )
				{
					antiAliasing = 2,
					depth = 24	// 24以上にしないとステンシルがサポートされない事に注意する
				} ;
				m_RenderTexture.Create() ;
			}
			if( m_RenderTexture == null )
			{
				if( m_RenderImage != null )
				{
					m_RenderImage.Texture = null ;
					DestroyImmediate( m_RenderImage.gameObject ) ;
					m_RenderImage = null ;
				}

				return ;
			}

			m_TargetCamera.targetTexture = m_RenderTexture ;
			m_RenderImage.Texture = m_RenderTexture ;
		}

		private Matrix4x4 PerspectiveOffCenter( Camera camera, float sw, float sh, Vector2 cameraOffset )
		{
			float xMin = ( camera.rect.xMin - 0.5f ) * 2.0f ;
			float xMax = ( camera.rect.xMax - 0.5f ) * 2.0f ;
			float yMin = ( camera.rect.yMin - 0.5f ) * 2.0f ;
			float yMax = ( camera.rect.yMax - 0.5f ) * 2.0f ;

			float fov   = camera.fieldOfView ;
			float ncp	= camera.nearClipPlane ;
			float fcp	= camera.farClipPlane ;

			//----------------------------------------------------------

			float vw = xMax - xMin ;
			float vh = yMax - yMin ;

			if( vw <= 0 || vh <= 0 )
			{
				return camera.projectionMatrix ;	// ０はダメよ
			}

			sw *= vw ;
			sh *= vh ;

			float ah = Mathf.Tan( 2.0f * Mathf.PI * fov / 360.0f ) ;
			float aw = ah * sh / sw ;

			//----------------------------------------------------------

			float x = aw ;
			float y = ah ;

			float a = ( xMax + xMin ) / vw ;
			float b = ( yMax + yMin ) / vh ;

			float c = - ( fcp + ncp ) / ( fcp - ncp ) ;
			float d = - ( 2.0f * fcp * ncp ) / ( fcp - ncp ) ;
			float e = - 1.0f ;

			Matrix4x4 m = new Matrix4x4() ;
			m[ 0, 0 ] = x ;
			m[ 0, 1 ] = 0 ;
			m[ 0, 2 ] = a ;
			m[ 0, 3 ] = cameraOffset.x ;
			m[ 1, 0 ] = 0 ;
			m[ 1, 1 ] = y ;
			m[ 1, 2 ] = b ;
			m[ 1, 3 ] = cameraOffset.y ;
			m[ 2, 0 ] = 0 ;
			m[ 2, 1 ] = 0 ;
			m[ 2, 2 ] = c ;
			m[ 2, 3 ] = d ;
			m[ 3, 0 ] = 0 ;
			m[ 3, 1 ] = 0 ;
			m[ 3, 2 ] = e ;
			m[ 3, 3 ] = 0 ;

			//----------------------------------------------------------

			return m ;
		}
	}
}
