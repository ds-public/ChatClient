#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System ;
using System.Collections.Generic ;

using UnityEditorInternal ;

namespace uGUIHelper
{
	/// <summary>
	/// メニューに生成機能追加 Version 2021/04/09
	/// </summary>
	public static class MenuExtension
	{
		/// <summary>
		/// View 生成
		/// </summary>
		[ MenuItem( "uGUIHelper/Add a View" ) ]
		[ MenuItem( "GameObject/uGUIHelper/View", false, 22 ) ]
		public static void AddView()
		{
			GameObject go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI View" ) ;	// アンドウバッファに登録

			GameObject child = new GameObject( GetName<UIView>(), typeof( RectTransform ) ) ;

			Transform t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.localPosition = Vector3.zero ;
			t.localRotation = Quaternion.identity ;
			t.localScale = Vector3.one ;

			UIView view = child.AddComponent<UIView>() ;
			view.SetDefault() ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// Graphic 生成
		/// </summary>
		[ MenuItem( "uGUIHelper/Add a Graphic Empty" ) ]
		[ MenuItem( "GameObject/uGUIHelper/Graphic Empty", false, 22 ) ]
		public static void AddGraphicEmpty()
		{
			GameObject go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI Graphic Empty" ) ;	// アンドウバッファに登録

			GameObject child = new GameObject( "Graphic Empty", typeof( RectTransform ) ) ;

			Transform t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.localPosition = Vector3.zero ;
			t.localRotation = Quaternion.identity ;
			t.localScale = Vector3.one ;

			var graphicEmpty = child.AddComponent<GraphicEmpty>() ;

			UIView view = child.AddComponent<UIView>() ;
			view.SetDefault() ;

			// GraphicEmpty は下に持ってくる
			while( ComponentUtility.MoveComponentDown( graphicEmpty ) ){}

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// Space 生成
		/// </summary>
		[ MenuItem( "uGUIHelper/Add a Space" ) ]
		[ MenuItem( "GameObject/uGUIHelper/Space", false, 22 ) ]
		public static void AddSpace()
		{
			GameObject go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI Space" ) ;	// アンドウバッファに登録

			GameObject child = new GameObject( GetName<UISpace>(), typeof( RectTransform ) ) ;

			Transform t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.localPosition = Vector3.zero ;
			t.localRotation = Quaternion.identity ;
			t.localScale = Vector3.one ;

			UISpace camera = child.AddComponent<UISpace>() ;
			camera.SetDefault() ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// Text 生成
		/// </summary>
		[ MenuItem( "uGUIHelper/Add a Text" ) ]
		[ MenuItem( "GameObject/uGUIHelper/Text", false, 22 ) ]
		public static void AddText()
		{
			GameObject go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI Text" ) ;	// アンドウバッファに登録

			GameObject child = new GameObject( GetName<UIText>(), typeof( RectTransform ) ) ;

			Transform t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.localPosition = Vector3.zero ;
			t.localRotation = Quaternion.identity ;
			t.localScale = Vector3.one ;

			UIText view = child.AddComponent<UIText>() ;
			view.SetDefault() ;
			view.Text = "Text" ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// RichText 生成
		/// </summary>
		[ MenuItem( "uGUIHelper/Add a RichText" ) ]
		[ MenuItem( "GameObject/uGUIHelper/RichText", false, 22 ) ]
		public static void AddRichText()
		{
			GameObject go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI RichText" ) ;	// アンドウバッファに登録

			GameObject child = new GameObject( GetName<UIRichText>(), typeof( RectTransform ) ) ;

			Transform t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.localPosition = Vector3.zero ;
			t.localRotation = Quaternion.identity ;
			t.localScale = Vector3.one ;

			UIRichText view = child.AddComponent<UIRichText>() ;
			view.SetDefault() ;
			view.Text = "RichText" ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// TextMesh 生成
		/// </summary>
		[ MenuItem( "uGUIHelper/Add a TextMesh" ) ]
		[ MenuItem( "GameObject/uGUIHelper/TextMesh", false, 22 ) ]
		public static void AddTextMesh()
		{
			GameObject go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI TextMesh" ) ;	// アンドウバッファに登録

			GameObject child = new GameObject( GetName<UITextMesh>(), typeof( RectTransform ) ) ;

			Transform t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.localPosition = Vector3.zero ;
			t.localRotation = Quaternion.identity ;
			t.localScale = Vector3.one ;

			UITextMesh view = child.AddComponent<UITextMesh>() ;
			view.SetDefault() ;
			view.Text = "TextMesh" ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// Number 生成
		/// </summary>
		[ MenuItem( "uGUIHelper/Add a Number" ) ]
		[ MenuItem( "GameObject/uGUIHelper/Number", false, 22 ) ]
		public static void AddNumber()
		{
			GameObject go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI Number" ) ;	// アンドウバッファに登録

			GameObject child = new GameObject( GetName<UINumber>(), typeof( RectTransform ) ) ;

			Transform t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.localPosition = Vector3.zero ;
			t.localRotation = Quaternion.identity ;
			t.localScale = Vector3.one ;

			UINumber view = child.AddComponent<UINumber>() ;
			view.SetDefault() ;
			view.Text = "Number" ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// Number 生成
		/// </summary>
		[ MenuItem( "uGUIHelper/Add a NumberMesh" ) ]
		[ MenuItem( "GameObject/uGUIHelper/NumberMesh", false, 22 ) ]
		public static void AddNumberMesh()
		{
			GameObject go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI NumberMesh" ) ;	// アンドウバッファに登録

			GameObject child = new GameObject( GetName<UINumberMesh>(), typeof( RectTransform ) ) ;

			Transform t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.localPosition = Vector3.zero ;
			t.localRotation = Quaternion.identity ;
			t.localScale = Vector3.one ;

			UINumberMesh view = child.AddComponent<UINumberMesh>() ;
			view.SetDefault() ;
			view.Text = "NumberMesh" ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// ImageNumber 生成
		/// </summary>
		[ MenuItem( "uGUIHelper/Add a ImageNumber" ) ]
		[ MenuItem( "GameObject/uGUIHelper/ImageNumber", false, 22 ) ]
		public static void AddImageNumber()
		{
			GameObject go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI ImageNumber" ) ;	// アンドウバッファに登録

			GameObject child = new GameObject( GetName<UIImageNumber>(), typeof( RectTransform ) ) ;

			Transform t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.localPosition = Vector3.zero ;
			t.localRotation = Quaternion.identity ;
			t.localScale = Vector3.one ;

			UIImageNumber view = child.AddComponent<UIImageNumber>() ;
			view.SetDefault() ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}


		/// <summary>
		/// Image 生成
		/// </summary>
		[ MenuItem( "uGUIHelper/Add a Image" ) ]
		[ MenuItem( "GameObject/uGUIHelper/Image", false, 22 ) ]
		public static void AddImage()
		{
			AddImage( "" ) ;
		}

		public static void AddImage( string type )
		{
			GameObject go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI Image" ) ;	// アンドウバッファに登録

			string name = "Image" ;
			if( string.IsNullOrEmpty( type ) == false )
			{
				name = type ;
			}

			GameObject child = new GameObject( name, typeof( RectTransform ) ) ;

			Transform t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.localPosition = Vector3.zero ;
			t.localRotation = Quaternion.identity ;
			t.localScale = Vector3.one ;

			UIImage view = child.AddComponent<UIImage>() ;
			view.SetDefault( type ) ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// RawImage 生成
		/// </summary>
		[ MenuItem( "uGUIHelper/Add a RawImage" ) ]
		[ MenuItem( "GameObject/uGUIHelper/RawImage", false, 22 ) ]
		public static void AddRawImage()
		{
			GameObject go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI RawImage" ) ;	// アンドウバッファに登録

			GameObject child = new GameObject( GetName<UIRawImage>(), typeof( RectTransform ) ) ;

			Transform t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.localPosition = Vector3.zero ;
			t.localRotation = Quaternion.identity ;
			t.localScale = Vector3.one ;

			UIRawImage view = child.AddComponent<UIRawImage>() ;
			view.SetDefault() ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// GridMap 生成
		/// </summary>
		[ MenuItem( "uGUIHelper/Add a GridMap" ) ]
		[ MenuItem( "GameObject/uGUIHelper/GridMap", false, 22 ) ]
		public static void AddGridMap()
		{
			GameObject go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI GridMap" ) ;	// アンドウバッファに登録

			GameObject child = new GameObject( GetName<UIGridMap>(), typeof( RectTransform ) ) ;

			Transform t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.localPosition = Vector3.zero ;
			t.localRotation = Quaternion.identity ;
			t.localScale = Vector3.one ;

			UIGridMap view = child.AddComponent<UIGridMap>() ;
			view.SetDefault() ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// ComplexRectangle 生成
		/// </summary>
		[ MenuItem( "uGUIHelper/Add a ComplexRectangle" ) ]
		[ MenuItem( "GameObject/uGUIHelper/ComplexRectangle", false, 22 ) ]
		public static void AddComplexRectangle()
		{
			GameObject go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI ComplexRectangle" ) ;	// アンドウバッファに登録

			GameObject child = new GameObject( GetName<UIComplexRectangle>(), typeof( RectTransform ) ) ;

			Transform t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.localPosition = Vector3.zero ;
			t.localRotation = Quaternion.identity ;
			t.localScale = Vector3.one ;

			UIComplexRectangle view = child.AddComponent<UIComplexRectangle>() ;
			view.SetDefault() ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// Line 生成
		/// </summary>
		[ MenuItem( "uGUIHelper/Add a Line" ) ]
		[ MenuItem( "GameObject/uGUIHelper/Line", false, 22 ) ]
		public static void AddLine()
		{
			GameObject go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI Line" ) ;	// アンドウバッファに登録

			GameObject child = new GameObject( GetName<UILine>(), typeof( RectTransform ) ) ;

			Transform t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.localPosition = Vector3.zero ;
			t.localRotation = Quaternion.identity ;
			t.localScale = Vector3.one ;

			UILine view = child.AddComponent<UILine>() ;
			view.SetDefault() ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// Circle 生成
		/// </summary>
		[ MenuItem( "uGUIHelper/Add a Circle" ) ]
		[ MenuItem( "GameObject/uGUIHelper/Circle", false, 22 ) ]
		public static void AddCircle()
		{
			GameObject go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI Circle" ) ;	// アンドウバッファに登録

			GameObject child = new GameObject( GetName<UICircle>(), typeof( RectTransform ) ) ;

			Transform t = child.transform ;
			t.SetParent(go.transform, false ) ;
			t.localPosition = Vector3.zero ;
			t.localRotation = Quaternion.identity ;
			t.localScale = Vector3.one ;

			UICircle view = child.AddComponent<UICircle>() ;
			view.SetDefault() ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// Arc 生成
		/// </summary>
		[ MenuItem( "uGUIHelper/Add a Arc" ) ]
		[ MenuItem( "GameObject/uGUIHelper/Arc", false, 22 ) ]
		public static void AddArc()
		{
			GameObject go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI Arc" ) ;	// アンドウバッファに登録

			GameObject child = new GameObject( GetName<UIArc>(), typeof( RectTransform ) ) ;

			Transform t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.localPosition = Vector3.zero ;
			t.localRotation = Quaternion.identity ;
			t.localScale = Vector3.one ;

			UIArc view = child.AddComponent<UIArc>() ;
			view.SetDefault() ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// Button 生成
		/// </summary>
		[ MenuItem( "uGUIHelper/Add a Button" ) ]
		[ MenuItem( "GameObject/uGUIHelper/Button", false, 22 ) ]
		public static void AddButton()
		{
			GameObject go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI Button" ) ;	// アンドウバッファに登録

			GameObject child = new GameObject( GetName<UIButton>(), typeof( RectTransform ) ) ;

			Transform t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.localPosition = Vector3.zero ;
			t.localRotation = Quaternion.identity ;
			t.localScale = Vector3.one ;

			UIButton button = child.AddComponent<UIButton>() ;
			button.SetDefault() ;

//			button.AddLabel( "Button", 0xFF000000 ) ;
			button.AddLabelMesh( "Button", 0xFF000000 ) ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// Toggle 生成
		/// </summary>
		[ MenuItem( "uGUIHelper/Add a Toggle" ) ]
		[ MenuItem( "GameObject/uGUIHelper/Toggle", false, 22 ) ]
		public static void AddToggle()
		{
			GameObject go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI Toggle" ) ;	// アンドウバッファに登録

			GameObject child = new GameObject( GetName<UIToggle>(), typeof( RectTransform ) ) ;

			Transform t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.localPosition = Vector3.zero ;
			t.localRotation = Quaternion.identity ;
			t.localScale = Vector3.one ;

			UIToggle toggle = child.AddComponent<UIToggle>() ;
			toggle.SetDefault() ;

			if( toggle.Label != null )
			{
				toggle.Label.Text = "Toggle" ;
			}
			else
			if( toggle.LabelMesh != null )
			{
				toggle.LabelMesh.Text = "Toggle" ;
			}

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// Slider(Horizontal) 生成
		/// </summary>
		[ MenuItem( "uGUIHelper/Slider/Add a Slider(Horizontal)" ) ]
		[ MenuItem( "GameObject/uGUIHelper/Slider/Horizontal", false, 22 ) ]
		public static void AddSliderH()
		{
			AddSlider( "H" ) ;
		}

		[ MenuItem( "uGUIHelper/Slider/Add a Slider(Vertical)" ) ]
		[ MenuItem( "GameObject/uGUIHelper/Slider/Vertical", false, 22 ) ]
		public static void AddSliderV()
		{
			AddSlider( "V" ) ;
		}

		public static void AddSlider( string tType )
		{
			GameObject tGameObject = Selection.activeGameObject ;
			if( tGameObject == null )
			{
				return ;
			}

			if( WillLosePrefab( tGameObject ) == false )
			{
				return ;
			}

			Undo.RecordObject( tGameObject, "Add a child UI Slider" ) ;	// アンドウバッファに登録

			string tName = GetName<UISlider>() ;
			if( string.IsNullOrEmpty( tType ) == false )
			{
				tName = tName + "(" + tType + ")" ;
			}

			GameObject tChild = new GameObject( tName, typeof( RectTransform ) ) ;

			Transform tTransform = tChild.transform ;
			tTransform.SetParent( tGameObject.transform, false ) ;
			tTransform.localPosition = Vector3.zero ;
			tTransform.localRotation = Quaternion.identity ;
			tTransform.localScale = Vector3.one ;

			UISlider tSlider = tChild.AddComponent<UISlider>() ;
			tSlider.SetDefault( tType ) ;

			Selection.activeGameObject = tChild ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// Scrollbar(Horizontal) 生成
		/// </summary>
		[ MenuItem( "uGUIHelper/Scrollbar/Add a Scrollbar(Horizontal)" ) ]
		[ MenuItem( "GameObject/uGUIHelper/Scrollbar/Horizontal", false, 22 ) ]
		public static void AddScrollbarH()
		{
			AddScrollbar( "H" ) ;
		}

		/// <summary>
		/// Scrollbar(Vertical) 生成
		/// </summary>
		[ MenuItem( "uGUIHelper/Scrollbar/Add a Scrollbar(Vertical)" ) ]
		[ MenuItem( "GameObject/uGUIHelper/Scrollbar/Vertical", false, 22 ) ]
		public static void AddScrollbarV()
		{
			AddScrollbar( "V" ) ;
		}

		public static void AddScrollbar( string tType )
		{
			GameObject tGameObject = Selection.activeGameObject ;
			if( tGameObject == null )
			{
				return ;
			}

			if( WillLosePrefab( tGameObject ) == false )
			{
				return ;
			}

			Undo.RecordObject( tGameObject, "Add a child UI Scrollbar" ) ;	// アンドウバッファに登録

			string tName = GetName<UIScrollbar>() ;
			if( string.IsNullOrEmpty( tType ) == false )
			{
				tName = tName + "(" + tType + ")" ;
			}

			GameObject tChild = new GameObject( tName, typeof( RectTransform ) ) ;

			Transform tTransform = tChild.transform ;
			tTransform.SetParent( tGameObject.transform, false ) ;
			tTransform.localPosition = Vector3.zero ;
			tTransform.localRotation = Quaternion.identity ;
			tTransform.localScale = Vector3.one ;

			UIScrollbar tScrollbar = tChild.AddComponent<UIScrollbar>() ;
			tScrollbar.SetDefault( tType ) ;

			Selection.activeGameObject = tChild ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// Dropdown 生成
		/// </summary>
		[ MenuItem( "uGUIHelper/Add a Dropdown" ) ]
		[ MenuItem( "GameObject/uGUIHelper/Dropdown", false, 22 ) ]
		public static void AddDropdown()
		{
			GameObject tGameObject = Selection.activeGameObject ;
			if( tGameObject == null )
			{
				return ;
			}

			if( WillLosePrefab( tGameObject ) == false )
			{
				return ;
			}

			Undo.RecordObject( tGameObject, "Add a child UI Dropdown" ) ;	// アンドウバッファに登録

			GameObject tChild = new GameObject( GetName<UIDropdown>(), typeof( RectTransform ) ) ;

			Transform tTransform = tChild.transform ;
			tTransform.SetParent( tGameObject.transform, false ) ;
			tTransform.localPosition = Vector3.zero ;
			tTransform.localRotation = Quaternion.identity ;
			tTransform.localScale = Vector3.one ;

			UIDropdown tDropdown = tChild.AddComponent<UIDropdown>() ;
			tDropdown.SetDefault() ;

			Selection.activeGameObject = tChild ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// InputField 生成
		/// </summary>
		[ MenuItem( "uGUIHelper/Add a InputField(S)" ) ]
		[ MenuItem( "GameObject/uGUIHelper/InputField(S)", false, 22 ) ]
		public static void AddInputFieldSingle()
		{
			GameObject tGameObject = Selection.activeGameObject ;
			if( tGameObject == null )
			{
				return ;
			}

			if( WillLosePrefab( tGameObject ) == false )
			{
				return ;
			}

			Undo.RecordObject( tGameObject, "Add a child UI InputField" ) ;	// アンドウバッファに登録

			GameObject tChild = new GameObject( GetName<UIInputField>(), typeof( RectTransform ) ) ;

			Transform tTransform = tChild.transform ;
			tTransform.SetParent( tGameObject.transform, false ) ;
			tTransform.localPosition = Vector3.zero ;
			tTransform.localRotation = Quaternion.identity ;
			tTransform.localScale = Vector3.one ;

			UIInputField tInputField = tChild.AddComponent<UIInputField>() ;
			tInputField.SetDefault() ;

			Selection.activeGameObject = tChild ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// InputField 生成
		/// </summary>
		[ MenuItem( "uGUIHelper/Add a InputField(M)" ) ]
		[ MenuItem( "GameObject/uGUIHelper/InputField(M)", false, 22 ) ]
		public static void AddInputFieldMulti()
		{
			GameObject tGameObject = Selection.activeGameObject ;
			if( tGameObject == null )
			{
				return ;
			}

			if( WillLosePrefab( tGameObject ) == false )
			{
				return ;
			}

			Undo.RecordObject( tGameObject, "Add a child UI InputField" ) ;	// アンドウバッファに登録

			GameObject tChild = new GameObject( GetName<UIInputField>(), typeof( RectTransform ) ) ;

			Transform tTransform = tChild.transform ;
			tTransform.SetParent( tGameObject.transform, false ) ;
			tTransform.localPosition = Vector3.zero ;
			tTransform.localRotation = Quaternion.identity ;
			tTransform.localScale = Vector3.one ;

			UIInputField tInputField = tChild.AddComponent<UIInputField>() ;
			tInputField.SetDefault( "MultiLine" ) ;

			Selection.activeGameObject = tChild ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}


		/// <summary>
		/// Progressbar 生成
		/// </summary>
		[ MenuItem( "uGUIHelper/Add a Progressbar" ) ]
		[ MenuItem( "GameObject/uGUIHelper/Progressbar", false, 22 ) ]
		public static void AddProgressbar()
		{
			GameObject tGameObject = Selection.activeGameObject ;
			if( tGameObject == null )
			{
				return ;
			}

			if( WillLosePrefab( tGameObject ) == false )
			{
				return ;
			}

			Undo.RecordObject( tGameObject, "Add a child UI Progressbar" ) ;	// アンドウバッファに登録

			GameObject tChild = new GameObject( GetName<UIProgressbar>(), typeof( RectTransform ) ) ;

			Transform tTransform = tChild.transform ;
			tTransform.SetParent( tGameObject.transform, false ) ;
			tTransform.localPosition = Vector3.zero ;
			tTransform.localRotation = Quaternion.identity ;
			tTransform.localScale = Vector3.one ;

			UIProgressbar tProgressbar = tChild.AddComponent<UIProgressbar>() ;
			tProgressbar.SetDefault() ;

			Selection.activeGameObject = tChild ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// Canvas 生成
		/// </summary>
		[ MenuItem( "uGUIHelper/Add a Canvas" ) ]
		[ MenuItem( "GameObject/uGUIHelper/Canvas", false, 22 ) ]
		public static void AddCanvas()
		{
			Transform tParent = null ;

			GameObject tGameObject = Selection.activeGameObject ;
			if( tGameObject != null )
			{
				if( WillLosePrefab( tGameObject ) == false )
				{
					return ;
				}

				tParent = tGameObject.transform ;

				Undo.RecordObject( tGameObject, "Add a child UI Canvas" ) ; // アンドウバッファに登録
			}

			UICanvas tCanvas = UICanvas.Create( tParent, 800, 600 ) ;
			Selection.activeGameObject = tCanvas.gameObject ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// Camera を子として持つ Canvas を生成
		/// </summary>
		[ MenuItem( "uGUIHelper/Add a Canvas With Camera" ) ]
		[ MenuItem( "GameObject/uGUIHelper/Canvas With Camera", false, 22 ) ]
		public static void AddCanvasWithCamera()
		{
			Transform tParent = null ;

			GameObject tGameObject = Selection.activeGameObject ;
			if( tGameObject != null )
			{
				if( WillLosePrefab( tGameObject ) == false )
				{
					return ;
				}

				tParent = tGameObject.transform ;

				Undo.RecordObject( tGameObject, "Add a child UI Canvas With Camera" ) ; // アンドウバッファに登録
			}

			UICanvas tCanvas = UICanvas.CreateWithCamera( tParent, 800, 600 ) ;
			Selection.activeGameObject = tCanvas.gameObject ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// Camera を親として持つ Canvas を生成
		/// </summary>
		[ MenuItem( "uGUIHelper/Add a Canvas On Camera" ) ]
		[ MenuItem( "GameObject/uGUIHelper/Canvas On Camera", false, 22 ) ]
		public static void AddCanvasOnCamera()
		{
			Transform tParent = null ;

			GameObject tGameObject = Selection.activeGameObject ;
			if( tGameObject != null )
			{
				if( WillLosePrefab( tGameObject ) == false )
				{
					return ;
				}

				tParent = tGameObject.transform ;

				Undo.RecordObject( tGameObject, "Add a child UI Canvas On Camera" ) ;   // アンドウバッファに登録
			}

			UICanvas tCanvas = UICanvas.CreateOnCamera( tParent, 800, 600 ) ;
			Selection.activeGameObject = tCanvas.gameObject ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// Panel(Image) を生成
		/// </summary>
		[ MenuItem( "uGUIHelper/Add a Panel" ) ]
		[ MenuItem( "GameObject/uGUIHelper/Panel", false, 22 ) ]
		public static void AddPanel()
		{
			AddImage( "Panel" ) ;
		}

		/// <summary>
		/// ScrollView(Normal) を生成
		/// </summary>
		[ MenuItem( "uGUIHelper/ScrollView/Add a ScrollView(Basic)" ) ]
		[ MenuItem( "GameObject/uGUIHelper/ScrollView/Basic", false, 22 ) ]
		public static void AddScrollView()
		{
			AddScrollView( "" ) ;
		}

		/// <summary>
		/// ScrollView(Horizontal) を生成
		/// </summary>
		[ MenuItem( "uGUIHelper/ScrollView/Add a ScrollView(Horizontal)" ) ]
		[ MenuItem( "GameObject/uGUIHelper/ScrollView/Horizontal", false, 22 ) ]
		public static void AddScrollViewSH()
		{
			AddScrollView( "SH" ) ;
		}

		/// <summary>
		/// ScrollView(Vertical) を生成
		/// </summary>
		[ MenuItem( "uGUIHelper/ScrollView/Add a ScrollView(Vertical)" ) ]
		[ MenuItem( "GameObject/uGUIHelper/ScrollView/Vertical", false, 22 ) ]
		public static void AddScrollViewSV()
		{
			AddScrollView( "SV" ) ;
		}

		private static void AddScrollView( string tType )
		{
			GameObject tGameObject = Selection.activeGameObject ;
			if( tGameObject == null )
			{
				return ;
			}

			if( WillLosePrefab( tGameObject ) == false )
			{
				return ;
			}

			Undo.RecordObject( tGameObject, "Add a child UI ScrollView" ) ;	// アンドウバッファに登録

			string tName = GetName<UIScrollView>() ;
			if( string.IsNullOrEmpty( tType ) == false )
			{
				tName = tName + "(" + tType + ")" ;
			}

			GameObject tChild = new GameObject( tName, typeof( RectTransform ) ) ;

			Transform tTransform = tChild.transform ;
			tTransform.SetParent( tGameObject.transform, false ) ;
			tTransform.localPosition = Vector3.zero ;
			tTransform.localRotation = Quaternion.identity ;
			tTransform.localScale = Vector3.one ;

			UIScrollView tScrollView = tChild.AddComponent<UIScrollView>() ;
			tScrollView.SetDefault( tType ) ;

			Selection.activeGameObject = tChild ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// ListView(Horizontal) を生成
		/// </summary>
		[ MenuItem( "uGUIHelper/ListView/Add a ListView(Horizontal)" ) ]
		[ MenuItem( "GameObject/uGUIHelper/ListView/Horizontal", false, 22 ) ]
		public static void AddListViewH()
		{
			AddListView( "H" ) ;
		}

		/// <summary>
		/// ListView(Vertical) を生成
		/// </summary>
		[ MenuItem( "uGUIHelper/ListView/Add a ListView(Vertical)" ) ]
		[ MenuItem( "GameObject/uGUIHelper/ListView/Vertical", false, 22 ) ]
		public static void AddListViewV()
		{
			AddListView( "V" ) ;
		}


		private static void AddListView( string tType )
		{
			GameObject tGameObject = Selection.activeGameObject ;
			if( tGameObject == null )
			{
				return ;
			}

			if( WillLosePrefab( tGameObject ) == false )
			{
				return ;
			}

			Undo.RecordObject( tGameObject, "Add a child UI ListView" ) ;	// アンドウバッファに登録

			string tName = GetName<UIListView>() ;
			if( string.IsNullOrEmpty( tType ) == false )
			{
				tName = tName + "(" + tType + ")" ;
			}

			GameObject tChild = new GameObject( tName, typeof( RectTransform ) ) ;

			Transform tTransform = tChild.transform ;
			tTransform.SetParent( tGameObject.transform, false ) ;
			tTransform.localPosition = Vector3.zero ;
			tTransform.localRotation = Quaternion.identity ;
			tTransform.localScale = Vector3.one ;

			UIListView tScrollView = tChild.AddComponent<UIListView>() ;
			tScrollView.SetDefault( tType ) ;

			Selection.activeGameObject = tChild ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}





		/// <summary>
		/// Joystick を生成
		/// </summary>
		[ MenuItem( "uGUIHelper/Add a Joystick" ) ]
		[ MenuItem( "GameObject/uGUIHelper/Joystick", false, 22 ) ]
		public static void AddJoystick()
		{
			GameObject tGameObject = Selection.activeGameObject ;
			if( tGameObject == null )
			{
				return ;
			}

			if( WillLosePrefab( tGameObject ) == false )
			{
				return ;
			}

			Undo.RecordObject( tGameObject, "Add a child UI Joystick" ) ;	// アンドウバッファに登録

			GameObject tChild = new GameObject( GetName<UIJoystick>(), typeof( RectTransform ) ) ;

			Transform tTransform = tChild.transform ;
			tTransform.SetParent( tGameObject.transform, false ) ;
			tTransform.localPosition = Vector3.zero ;
			tTransform.localRotation = Quaternion.identity ;
			tTransform.localScale = Vector3.one ;

			UIJoystick tJoystick = tChild.AddComponent<UIJoystick>() ;
			tJoystick.SetDefault() ;

			Selection.activeGameObject = tChild ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// EventSyetem を生成
		/// </summary>
		[ MenuItem( "uGUIHelper/Create a EventSystem" ) ]
		[ MenuItem( "GameObject/uGUIHelper/EventSystem", false, 22 ) ]
		public static void CreateEventSystem()
		{
			GameObject tParent = Selection.activeGameObject ;

			if( tParent != null )
			{
				if( WillLosePrefab( tParent ) == false )
				{
					return ;
				}

				Undo.RecordObject( tParent, "Create a EventSystem" ) ;	// アンドウバッファに登録
			}

			GameObject tChild = new GameObject( GetName<UIEventSystem>() ) ;

			Transform tTransform = tChild.transform ;

			if( tParent != null )
			{
				tTransform.SetParent( tParent.transform, false ) ;
			}
			tTransform.localPosition = Vector3.zero ;
			tTransform.localRotation = Quaternion.identity ;
			tTransform.localScale = Vector3.one ;

			tChild.AddComponent<UIEventSystem>() ;
			Selection.activeGameObject = tChild ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// Scene を生成
		/// </summary>
		[ MenuItem( "uGUIHelper/Create a Scene" ) ]
		[ MenuItem( "GameObject/uGUIHelper/Scene", false, 22 ) ]
		public static void CreateScene()
		{
			GameObject tGameObject = new GameObject( "Scene" ) ;

			Transform tTransform = tGameObject.transform ;
			tTransform.SetParent( null ) ;
			tTransform.localPosition = Vector3.zero ;
			tTransform.localRotation = Quaternion.identity ;
			tTransform.localScale = Vector3.one ;

//			GameObject tController = new GameObject( "Controller" ) ;
//			tTransform = tController.transform ;
//			tTransform.SetParent( tGameObject.transform, false ) ;
//			tTransform.localPosition = Vector3.zero ;
//			tTransform.localRotation = Quaternion.identity ;
//			tTransform.localScale = Vector3.one ;

			Selection.activeGameObject = tGameObject ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// SpriteAnimation を生成
		/// </summary>
		[ MenuItem( "uGUIHelper/Create a New SpriteAnimation" ) ]
		[ MenuItem( "GameObject/uGUIHelper/SpriteAnimation", false, 22 ) ]
		[ MenuItem( "Assets/Create/uGUIHelper/SpriteAnimation" ) ]
		static public void CreateSpriteAnimation()
		{
			string tPath = "Assets/" ;
			string tName = "A New SpriteAnimation" ;

			if( Selection.objects != null && Selection.objects.Length == 1 && Selection.activeObject != null )
			{
				if( Selection.activeObject.GetType() != typeof( GameObject ) )
				{
					tPath = AssetDatabase.GetAssetPath( Selection.activeObject.GetInstanceID() ) ;

					string tCheckPath = tPath.Replace( "/", "\\" ) ;
					if( System.IO.File.Exists( tCheckPath ) == true )
					{
						// ファイルなのでフォルダ名を取得する

						// 最後のフォルダ区切り位置を取得する
						int s = tPath.LastIndexOf( '/' ) ;

						tPath = tPath.Substring( 0, s ) ;
					}

					tPath += "/" ;
				}
			}

			tPath = tPath + tName + ".asset" ;

			UISpriteAnimation tSpriteAnimation = AssetDatabase.LoadAssetAtPath( tPath, typeof( UISpriteAnimation ) ) as UISpriteAnimation ;
			if( tSpriteAnimation != null )
			{
				// 既にアセットが存在する
				Selection.activeObject = tSpriteAnimation ;

				return ;
			}

			tSpriteAnimation = ScriptableObject.CreateInstance<UISpriteAnimation>() ;
			tSpriteAnimation.name = tName ;

			AssetDatabase.CreateAsset( tSpriteAnimation, tPath ) ;
			AssetDatabase.Refresh() ;

			Selection.activeObject = tSpriteAnimation ;
		}

		//-------------------------------------------------

		private static bool WillLosePrefab( GameObject root )
		{
			if( root == null )
			{
				return false ;
			}

			if( root.transform != null )
			{
				PrefabAssetType type = PrefabUtility.GetPrefabAssetType( root ) ;

				if( type != PrefabAssetType.NotAPrefab )
				{
					return EditorUtility.DisplayDialog( "Losing prefab", "This action will lose the prefab connection. Are you sure you wish to continue?", "Continue", "Cancel" ) ;
				}
			}
			return true ;
		}

		private static string GetName<T> () where T : Component
		{
			string s = typeof( T ).ToString() ;

			int i ;

			i = s.IndexOf( "." ) ;
			if( i >= 0 )
			{
				s = s.Substring( i + 1 ) ;
			}

			i = s.IndexOf( "UI" ) ;
			if( i >= 0 )
			{
				s = s.Substring( i + 2, s.Length - ( i + 2 ) ) ;
			}

			return s ;
		}
	}
}

#endif
