using System ;
using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;
#if UNITY_EDITOR
using UnityEditor ;
#endif

namespace Template
{
	/// <summary>
	/// 起動時の各種設定クラス Version 2022/05/07
	/// </summary>
	[CreateAssetMenu( fileName = "Settings", menuName = "ScriptableObject/Beast/Settings" )]
	public class Settings : ScriptableObject
	{
		[Header("システムバージョン名")]
		public string	SystemVersionName = "0.8.4" ;

		[Header("リビジョン[Android] (システムバージョン名が変ったら0にリセットする事)")]
		public int		Revision_Android = 0 ;

		[Header("リビジョン[iOS] (システムバージョン名が変ったら0にリセットする事)")]
		public int		Revision_iOS = 0 ;

		/// <summary>
		/// リビジョン
		/// </summary>
		public int		Revision
		{
			get
			{
#if UNITY_IOS
				return Revision_iOS ;
#else
				return Revision_Android ;
#endif
			}
		}

		/// <summary>
		/// プラットフォームを自動判断してクライアントバージョンを返す
		/// </summary>
		public string   ClientVersionName
		{
			get
			{
#if UNITY_IOS
				return SystemVersionName ;	// iOS は、メジャーバージョン(4桁).マイナーバージョン(2桁).パッチバージョン(2桁)でなくてはならないルールがあるためシステムバージョンそのもののを返す
#else
				return SystemVersionName + "." + Revision.ToString() ;	// Android はバージョン表記の形式は自由
#endif
			}
		}

		[Header("クライアントバージョン値 (リリース後にクライアントに変更があったら +1 すること)")]
		public int		ClientVersionCode = 1 ;

		//-----------------------------------------------------------

		[Header( "基準解像度" )]
		public float	BasicWidth  = 1080 ;
		public float	BasicHeight = 1920 ;

		[Header( "最大解像度" )]
		public float	LimitWidth  = 1440 ;
		public float	LimitHeight = 2560 ;

		[Header( "初期フレームレート" )]
		public int		FrameRate_Rendering = 30 ;
		public int		FrameRate_FixedTime = 60 ;

		/// <summary>
		/// アセットバンドルのプラットフォームをビルドプラットフォームと強制的に違うものにしてデバッグするためのパラメータ
		/// </summary>
		public enum AssetBundlePlatformTypes
		{
			BuildPlatform,	// アセットバンドルのプラットフォームはビルドプラットフォームと同じ
			Windows,		// アセットバンドルのプラットフォームを強制的にWindowsにする
			OSX,			// アセットバンドルのプラットフォームを強制的にOSXにする
			Android,		// アセットバンドルのプラットフォームを強制的にAndroidにする
			iOS,			// アセットバンドルのプラットフォームを強制的にiOSにする
		}

		/// <summary>
		/// アセットバンドルのプラットフォームをビルドプラットフォームと強制的に違うものにしてデバッグするためのパラメータ
		/// </summary>
		[Header("ダウンロードするアセットバンドルプラットフォーム")]
		public AssetBundlePlatformTypes AssetBundlePlatformType = AssetBundlePlatformTypes.BuildPlatform ;

		/// <summary>
		/// ローカルアセットを使用するかどうか
		/// </summary>
		[Header("ローカルのアセットを使用するかどうか")]
		public bool UseLocalAssets = true ;

		/// <summary>
		/// ローカルアセットバンドルを使用するかどうか
		/// </summary>
		[Header("ローカルのアセットバンドルを使用するかどうか")]
		public bool UseLocalAssetBundle = true ;

		/// <summary>
		/// ストリーミングアセット(内のアセットバンドル)を使用するかどうか
		/// </summary>
		[Header("ストリーミングアセットのアセットバンドルを使用するかどうか")]
		public bool UseStreamingAssets = true ;

		/// <summary>
		/// リモートアセット(アセットバンドル)を使用するかどうか
		/// </summary>
		[Header("リモートのアセットバンドルを使用するかどうか")]
		public bool UseRemoteAssetBundle = true ;

		/// <summary>
		/// 並列ダウンロードを使用するかどうか
		/// </summary>
		[Header("並列ダウンロードを使用するかどうか)")]
		public bool	UseParallelDownload = true ;

		/// <summary>
		/// 並列ダウンロード時の最大数
		/// </summary>
		[Header("並列ダウンロード時の最大数)")]
		public int MaxParallelDownloadCount = 6 ;

		/// <summary>
		/// リモートアセットバンドルのエンドポイントを強制指定する
		/// </summary>
		[Header("リモートアセットバンドルのエンドポイントを強制指定する")]
		public string RemoteAssetBundlePath = string.Empty ;	// 言語とプラットフォームも指定する必要がある

		/// <summary>
		/// アセットバンドルの全体のバージョン値(デバッグ用)の値が異なる場合にアセットバンドルを全消去するかどうか
		/// </summary>
		[Header("アセットバンドルが古い場合に全消去を行うかどうか")]
		public bool EnableAssetBundleCleaning = true ;

		/// <summary>
		/// アセットバンドルの全体のバージョン値(デバッグ用)
		/// </summary>
		[Header("クライアントアセットバンドルバージョン")]
		public int	AssetBundleVersion = 0 ;

		//-----------------------------------------------------

		/// <summary>
		/// ＰＣ環境上でモバイル端末の動作を確認するためのフラグ
		/// </summary>
		public enum PlatformType
		{
			Default,	// 現在のプラットフォームの動作に従う
			Mobile,		// デバッグ用に強制的にモバイルモードの動作にする
		}

		[Header("モバイル端末エミュレーションを強制的に行うかどうか")]
		public PlatformType SelectedPlatformType = PlatformType.Default ;

		/// <summary>
		/// 例外が発生した時に例外ダイアログを表示する
		/// </summary>
		[Header("例外発生時にダイアログを表示するかどうか(実機限定・開発環境でのみ有効)")]
		public bool EnableExceptionDialog = true ;

		/// <summary>
		/// エンドポイント
		/// </summary>
		public enum EndPointNames
		{
			Experiment	=  0,
			Development =  1,
			Stable		=  2,
			Staging		=  3,
			Review		=  4,
			Release		=  5,
			Debug		=  7,

			Any			= 99,
		}

		/// <summary>
		/// エンドポイントの情報
		/// </summary>
		[Serializable]
		public class EndPointData
		{
			public EndPointNames	Name ;
			public string			DisplayName ;
			public string			Path ;
			public string			Description ;
		}

		private static Dictionary<EndPointNames,string> m_EndPointNames = new Dictionary<EndPointNames, string>()
		{
			{ EndPointNames.Experiment,		"実験サーバー"			},
			{ EndPointNames.Development,	"開発サーバー"			},
			{ EndPointNames.Stable,			"安定サーバー"			},
			{ EndPointNames.Staging,		"監修サーバー"			},
			{ EndPointNames.Review,			"審査サーバー"			},
			{ EndPointNames.Release,		"本番サーバー"			},
			{ EndPointNames.Debug,			"試験用サーバー"		},
		} ;


		[Header("WebAPIの通信先(エンドポイント)リスト")] //[NonSerialized]
		public EndPointData[] WebAPI_EndPoints = new EndPointData[]
		{
			new EndPointData()
			{
				Name		= EndPointNames.Experiment,
				DisplayName = m_EndPointNames[ EndPointNames.Experiment		], Path = "http://api-experiment.net/api/",
				Description = "システムバージョンアップ対応用の環境です"
			},
			new EndPointData()
			{
				Name		= EndPointNames.Development,
				DisplayName = m_EndPointNames[ EndPointNames.Development	], Path = "https://api-development.net/api/",
				Description = "基本の開発用の環境です"
			},
			new EndPointData()
			{
				Name		= EndPointNames.Stable,
				DisplayName = m_EndPointNames[ EndPointNames.Stable			], Path = "https://api-stable.net/api/",
				Description = "バランスチェック用の環境です"
			},
			new EndPointData()
			{
				Name		= EndPointNames.Staging,
				DisplayName = m_EndPointNames[ EndPointNames.Staging		], Path = "https://api-staging.net/api/",
				Description = "監修用の環境です"
			},
			new EndPointData()
			{
				Name		= EndPointNames.Review,
				DisplayName = m_EndPointNames[ EndPointNames.Review			], Path = "https://api-review.net/api/",
				Description = "Appleのアプリ審査用の環境です"
			},
			new EndPointData()
			{
				Name		= EndPointNames.Release,
				DisplayName = m_EndPointNames[ EndPointNames.Release		], Path = "https://api-release.net/api/",
				Description = "一般公開用の環境です"
			},
			new EndPointData()
			{
				Name		= EndPointNames.Debug,
				DisplayName = m_EndPointNames[ EndPointNames.Debug			], Path = "https://api-debug.net/api/",
				Description = "デバッグ専用の環境です",
			},
		} ;

		[Header("デフォルトのWebAPIの通信先(エンドポイント)")]
		public EndPointNames WebAPI_DefaultEndPoint = EndPointNames.Development ;	// バッチビルド時に書き換える

		[Header("各種デバッグ用の機能を有効にするか(ただし開発環境でのみ有効)")]
		public bool DevelopmentMode = true ;	// バッチビルド時に書き換える

		//-------------------------------------------------------------------------------------------

		[Header("各種情報の暗号化を行う(実機は常に有効)")]
		public bool	SecurityEnabled = true ;

		//-------------------------------------------------------------------------------------------

		[Header("リアルタイム通信サーバーのアドレス")]
		public string	ServerAddress		= "localhost" ;

		[Header("リアルタイム通信サーバーのポート番号")]
		public int		PortNumber	= 32000 ;
	}
}
