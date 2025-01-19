// /*
//  *	DATABRAIN | Logic
//  *	(c) 2023 Giant Grey
//  *	www.databrain.cc
//  *	
//  */
// #if UNITY_EDITOR
// using System.Threading.Tasks;
// using System.Linq;

// using UnityEditor;
// using UnityEditor.UIElements;
// using UnityEngine.UIElements;
// using UnityEngine;

// using Databrain.Helpers;
// using Databrain.Attributes;
// using Databrain.UI.Elements;
// using System.Collections.Generic;

// namespace Databrain.Logic
// {
//     [CustomEditor(typeof(LogicController))]
//     public class LogicControllerEditor : Editor
//     {
//         private VisualElement root;


//         private SerializedProperty libraryAsset;
//         private DataLibrary previousLibrary;

//         private Texture2D logo;


//         /// <summary>
//         /// Hacky way to circumwent a strange ui bug where the inspector looses the UI when entering playmode.
//         /// We store the last selected active game object before entering play mode, if it had a logic controller component, simply reselect it.
//         /// </summary>
//         [InitializeOnLoad]
//         public class DetectPlayModeChanges
//         {
//             static LogicController controller;

//             static DetectPlayModeChanges()
//             {
//                 EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
//             }


//             private static void OnPlayModeStateChanged(PlayModeStateChange state)
//             {
//                 var _obj = Selection.activeGameObject;
//                 if (_obj != null)
//                 {
//                     controller = _obj.GetComponent<LogicController>();

//                     if (controller != null)
//                     {
//                         switch (state)
//                         {
//                             case PlayModeStateChange.ExitingEditMode:
//                                 controller.activeGameObject = Selection.activeGameObject;
//                                 break;
//                             case PlayModeStateChange.EnteredPlayMode:
//                                 if (Selection.activeGameObject.GetInstanceID() == controller.activeGameObject.GetInstanceID())
//                                 {
//                                     Reselect();
//                                 }
//                                 break;

//                         }
//                     }
//                 }
//             }

//             private async static void Reselect()
//             {
//                 // Selection.activeGameObject = null;
//                 await Task.Delay(500);
//                 // Selection.activeGameObject = controller.activeGameObject;
//             }
//         }

//         private void OnEnable()
//         {
//             try
//             {
//                 libraryAsset = this.serializedObject.FindProperty(propertyPath: "data");
//             }
//             catch { }
//         }

        
//         public override VisualElement CreateInspectorGUI()
//         {
//             root = new VisualElement();
//             Build();
//             return root;
//         }


//         void Build()
//         {
//             Debug.Log("BUILD");
//             if (root == null)
//             {
//                 root = new VisualElement();
//             }

//             root.Clear();

//             if (logo == null)
//             {
//                 logo = DatabrainHelpers.LoadResourceTexture("logic_icon.png");
//             }

//             var _header = new VisualElement();
//             _header.style.backgroundColor = DatabrainColor.DarkGrey.GetColor();
//             _header.style.flexDirection = FlexDirection.Row;

            
//             var _icon = new VisualElement();
//             _icon.style.backgroundImage = logo;
//             _icon.style.width = 50;
//             _icon.style.height = 50;

//             var _title = new Label();
//             _title.text = "Logic Controller";
//             _title.style.unityFontStyleAndWeight = FontStyle.Bold;
//             _title.style.fontSize = 14;
//             _title.style.unityTextAlign = TextAnchor.MiddleLeft;

//             _header.Add(_icon);
//             _header.Add(_title);

//             var _propertiesContainer = new VisualElement();
//             var _dataLibrary = new PropertyField();
//             _dataLibrary.style.paddingRight = 4;
//             _dataLibrary.style.unityFontStyleAndWeight = FontStyle.Bold;
//             _dataLibrary.BindProperty(serializedObject.FindProperty("data"));
//             _dataLibrary.style.backgroundColor = DatabrainColor.Grey.GetColor();
//             _dataLibrary.style.marginBottom = 2;
//             DatabrainHelpers.SetBorder(_dataLibrary, 1, Color.black);
//             _dataLibrary.RegisterCallback<ChangeEvent<UnityEngine.Object>>(x =>
//             {
                
                
//                 if (previousLibrary != null)
//                 {
//                     if ((x.newValue as DataLibrary) != previousLibrary)
//                     {  
//                         previousLibrary = (x.newValue as DataLibrary);
//                         Build();
//                     }
//                 }
//                 else
//                 {
//                     if ((x.newValue as DataLibrary) != previousLibrary)
//                     {
//                         previousLibrary = (x.newValue as DataLibrary);
//                         Build();
//                     }
//                 }
//             });

//             var _graphAsset = new PropertyField();
//             _graphAsset.style.marginBottom = 2;
//             _graphAsset.BindProperty(serializedObject.FindProperty("graphAsset"));
//             _graphAsset.RegisterValueChangeCallback(evt =>
//             {
//                 if (evt.changedProperty.objectReferenceValue != (target as LogicController).graphAsset)
//                 {
//                     Build();
//                 }
//             });
            

//             var _execOnStart = new PropertyField();
//             _execOnStart.style.backgroundColor = DatabrainColor.Grey.GetColor();
//             _execOnStart.style.unityFontStyleAndWeight = FontStyle.Bold;
//             _execOnStart.label = "Execute on start";
//             _execOnStart.BindProperty(serializedObject.FindProperty("executeOnStart"));
//             _execOnStart.style.marginBottom = 2;
//             DatabrainHelpers.SetBorder(_execOnStart, 1, Color.black);

            
//             var _executeGraphInEditor = new Button();
//             DatabrainHelpers.SetBorder(_executeGraphInEditor, 1, DatabrainColor.DarkGrey.GetColor());
//             DatabrainHelpers.SetBorderRadius(_executeGraphInEditor, 0, 0, 0, 0);
//             DatabrainHelpers.SetMargin(_executeGraphInEditor, 0, 0, 10, 0);
//             var _buttonIcon = new VisualElement();
//             _buttonIcon.style.backgroundImage = DatabrainHelpers.LoadIcon("start");
//             _buttonIcon.style.width = 24;
//             _buttonIcon.style.minWidth = 24;
//             _buttonIcon.style.minHeight = 24;
//             _buttonIcon.style.unityBackgroundImageTintColor = DatabrainColor.Green.GetColor();
//             _executeGraphInEditor.Add(_buttonIcon);
//             _executeGraphInEditor.style.height = 30;
//             _executeGraphInEditor.text = "Execute";
//             _executeGraphInEditor.RegisterCallback<ClickEvent>(evt =>
//             {
//                 if ((target as LogicController).graphAsset != null)
//                 {
//                     (target as LogicController).ExecuteGraphEditor();
//                 }
//             });


//             var _topSpace = new VisualElement();
//             _topSpace.style.height = 10;

//             var _bottomSpace = new VisualElement();
//             _bottomSpace.style.height = 10;

//             _propertiesContainer.Add(_topSpace);
//             _propertiesContainer.Add(_dataLibrary);
//             _propertiesContainer.Add(_graphAsset);
//             _propertiesContainer.Add(_execOnStart);
//             //_propertiesContainer.Add(_executeGraphInEditor);
//             _propertiesContainer.Add(_bottomSpace);


//             root.Add(_header);
//             root.Add(_propertiesContainer);
// Debug.Log("HU?");
           
//             if (libraryAsset != null && libraryAsset.objectReferenceValue != null)
//             {
//                 // Get all existing scene components in the data library
//                 var _sceneObjects = (libraryAsset.objectReferenceValue as DataLibrary).GetAllInitialDataObjectsByType(typeof(SceneComponent), true);
//                 Debug.Log(_sceneObjects.Count);


//                 // Filter out scene components which are not used in the selected graph
//                 /////////////////
//                 var _graphSceneComponents = new List<string>();

//                 if ((target as LogicController).graphAsset == null)
//                     return;

//                 for (int n = 0; n < (target as LogicController).graphAsset.nodes.Count; n ++)
//                 {
//                     if ((target as LogicController).graphAsset.nodes[n] == null)
//                         continue;

//                     var _nodePublicFields = (target as LogicController).graphAsset.nodes[n].GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
//                     for (int f = 0; f < _nodePublicFields.Length; f ++)
//                     {
//                         if (typeof(SceneComponent).IsAssignableFrom(_nodePublicFields[f].FieldType))       
//                         {
//                             var _objectValue = _nodePublicFields[f].GetValue((target as LogicController).graphAsset.nodes[n]);
//                             if (_objectValue != null)
//                             {
//                                 if (!_graphSceneComponents.Contains((_objectValue as SceneComponent).title))
//                                 {
//                                     _graphSceneComponents.Add((_objectValue as SceneComponent).title);
//                                 }
//                             }
//                         }
//                     }
//                 }


//                 for (int s = _sceneObjects.Count - 1; s >= 0; s--)
//                 {
//                     var _exists = false;
//                     for (int k = 0; k < _graphSceneComponents.Count; k++)
//                     {
//                         if (_graphSceneComponents[k] == _sceneObjects[s].title)
//                         {
//                             _exists = true;
//                         }
//                     }

//                     if (!_exists)
//                     {
//                         _sceneObjects.RemoveAt(s);
//                     }
//                 }
//                 /////////////////

            
//                 if (_sceneObjects == null || _sceneObjects.Count == 0)
//                 {
//                     return;
//                 }
//                 var _expRefsContainer = new VisualElement();
//                 _expRefsContainer.style.flexDirection = FlexDirection.Row;

//                 var _exposedReferencesFoldout = new FoldoutElement("Component references", (x) =>
//                 {
//                     (target as LogicController).expRefFoldout = x;
//                 });

//                 _exposedReferencesFoldout.OpenFoldout((target as LogicController).expRefFoldout);


//                 var _refreshButton = DatabrainHelpers.DatabrainButton("");
//                 var _bgSize = new StyleBackgroundSize();
//                 _bgSize.value = new BackgroundSize(16, 16);
//                 _refreshButton.style.backgroundSize = _bgSize;
//                 _refreshButton.style.backgroundImage = DatabrainHelpers.LoadIcon("refresh");
//                 _refreshButton.style.width = 24;
//                 _refreshButton.style.height = 24;
//                 _refreshButton.style.marginTop = 5;
//                 _refreshButton.RegisterCallback<ClickEvent>(click =>
//                 {
//                     Build();
//                 });

//                 _expRefsContainer.Add(_exposedReferencesFoldout);
//                 _expRefsContainer.Add(_refreshButton);

//                 // Only for debugging
//                 var _imgui = new IMGUIContainer(() =>
//                 {
//                     IMGUI();
//                 });


//                 _sceneObjects = _sceneObjects.OrderBy(x => x.GetType().Name).ToList();

//                 var _lastTypeName = "";

//                 for (int s = 0; s < _sceneObjects.Count; s++)
//                 {
//                     var _sceneObjectsIndex = s;
//                     var _dataObjectSerializedObj = new SerializedObject(_sceneObjects[s]);

//                     var iterator = _dataObjectSerializedObj.GetIterator();

//                     bool enterChildren = true;

//                     if (_lastTypeName != _sceneObjects[s].GetType().Name)
//                     {
//                         var _dataTypeTitle = new Label();
//                         _dataTypeTitle.text = _sceneObjects[s].GetType().Name;
//                         _dataTypeTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
//                         //_dataTypeTitle.style.backgroundColor = DatabrainColor.Grey.GetColor();
//                         DatabrainHelpers.SetPadding(_dataTypeTitle, 4, 2, 4, 4);

//                         _exposedReferencesFoldout.AddContent(_dataTypeTitle);
                        
//                         _lastTypeName = _sceneObjects[s].GetType().Name;
//                     }

//                     while (iterator.NextVisible(enterChildren))
//                     {
//                         enterChildren = false;
//                         if (iterator.propertyPath == "exposedReferences")
//                         {
//                             var _propListObjects = serializedObject.FindProperty("listUnityEngineObjects.Array");
//                             var _propListNames = serializedObject.FindProperty("listOfPropNames.Array");

//                             // Clean up list
//                             for (int p = 0; p < (target as LogicController).listOfPropNames.Count; p++)
//                             {
//                                 var _exists = false;
//                                 for (int q = 0; q < _sceneObjects.Count; q++)
//                                 {

//                                     if (_sceneObjects[q].guid == (target as LogicController).listOfPropNames[p])
//                                     {
//                                         if ((_sceneObjects[q] as SceneComponent).exposedReferences != null)
//                                         {
//                                             if (p < (target as LogicController).listUnityEngineObjects.Count)
//                                             {
//                                                 if ((target as LogicController).listUnityEngineObjects[p] != null)
//                                                 {
//                                                     if ((_sceneObjects[q] as SceneComponent).exposedReferences[0].typeName == (target as LogicController).listUnityEngineObjects[p].GetType().AssemblyQualifiedName)
//                                                     {
//                                                         _exists = true;
//                                                     }
//                                                     else
//                                                     {
//                                                         //Debug.Log("NO " + (_sceneObjects[q] as GraphObjectReference).exposedReferences[0].typeName + " _ " + (target as LogicController).listUnityEngineObjects[p].GetType().AssemblyQualifiedName);
//                                                     }
//                                                 }
//                                             }
//                                         }
//                                         else
//                                         {
//                                             _exists = true;
//                                         }
//                                     }
//                                 }

//                                 if (!_exists)
//                                 {
//                                     _propListNames.DeleteArrayElementAtIndex(p);
//                                     _propListObjects.DeleteArrayElementAtIndex(p);
//                                 }


//                             }

                            
//                             for (int i = 0; i < iterator.arraySize; i++)
//                             {
//                                 var _index = i;
//                                 var _item = new VisualElement();
//                                 _item.style.backgroundColor = DatabrainColor.Grey.GetColor();
//                                 _item.style.flexDirection = FlexDirection.Row;
//                                 _item.style.flexGrow = 1;
//                                 //_item.style.backgroundColor = DatabrainColor.Grey.GetColor();
//                                 DatabrainHelpers.SetMargin(_item, 2, 2, 0, 1);
//                                 DatabrainHelpers.SetPadding(_item, 5, 5, 5, 5);
//                                 DatabrainHelpers.SetBorder(_item, 1);


//                                 var _type = iterator.GetArrayElementAtIndex(i).FindPropertyRelative("typeName");
                                
//                                 System.Type _tt = System.Type.GetType(_type.stringValue);
                                
//                                 if (_tt == null)
//                                 {
//                                     continue;
//                                 }
//                                 var _propName = "referenceObject";
//                                 var _objectProp = iterator.GetArrayElementAtIndex(i).FindPropertyRelative(_propName);
//                                 var _object = iterator.GetArrayElementAtIndex(i).FindPropertyRelative(_propName).FindPropertyRelative("ExposedReference");
//                                 //var _originalName = iterator.GetArrayElementAtIndex(i).FindPropertyRelative(_propName).FindPropertyRelative("name");
//                                 //var _name = iterator.GetArrayElementAtIndex(i).FindPropertyRelative("newName");
//                                 var _exposedName = iterator.GetArrayElementAtIndex(i).FindPropertyRelative(_propName).FindPropertyRelative("ExposedReference").FindPropertyRelative("exposedName");
//                                 var _defaultValue = iterator.GetArrayElementAtIndex(i).FindPropertyRelative(_propName).FindPropertyRelative("ExposedReference").FindPropertyRelative("defaultValue");



//                                 var _pfObject = new ObjectField();
//                                 _pfObject.tooltip = "Type: " + _tt.Name; 
//                                 _pfObject.objectType = _tt;
//                                 _pfObject.label = "";

//                                 var _objArray = serializedObject.FindProperty("listUnityEngineObjects.Array");
//                                 Debug.Log(_objArray.arraySize);

//                                 if (_objArray.arraySize > 0)
//                                 {
                                    
//                                     var newPropertyName = new PropertyName(_sceneObjects[_sceneObjectsIndex].guid);
//                                     var _ind = (target as LogicController).listOfPropNames.IndexOf(newPropertyName);
//                                     if (_ind < _objArray.arraySize && _ind > -1)
//                                     {
//                                         var _prop = _objArray.GetArrayElementAtIndex(_ind);

//                                         if (_prop != null)
//                                         {   
//                                             //_pfObject.BindProperty(_prop);
//                                             _pfObject.value = _prop.objectReferenceValue;
//                                         }
//                                     }
                                   
//                                 }
//                                 else
//                                 {

//                                 }

//                                 //Debug.Log("exposed name: " + _exposedName.stringValue);

//                                 _pfObject.RegisterCallback<ChangeEvent<UnityEngine.Object>>(x =>
//                                 {

//                                     var newPropertyName = new PropertyName(_sceneObjects[_sceneObjectsIndex].guid);
                                    
//                                     if (x.newValue != null && x.newValue != x.previousValue)
//                                     {


//                                         // Check if it already exists in the lists
//                                         bool _idValid = false;
//                                         var _ref = (target as IExposedPropertyTable).GetReferenceValue(newPropertyName, out _idValid);
//                                         // Object does not exist
//                                         if (!_idValid)
//                                         {
                                            
//                                             _exposedName.stringValue = _sceneObjects[_sceneObjectsIndex].guid;
//                                             _exposedName.serializedObject.ApplyModifiedProperties();
                                           
//                                             var _textPropName = new PropertyName(_sceneObjects[_sceneObjectsIndex].guid);
//                                             //Debug.Log("A " + _exposedName.stringValue + " _ " + _textPropName);
//                                             (target as IExposedPropertyTable).SetReferenceValue(_textPropName, x.newValue);
//                                         }
//                                         else
//                                         {
//                                             //Debug.Log("A " + _exposedName + " _ " + newPropertyName);
//                                             (target as IExposedPropertyTable).SetReferenceValue(newPropertyName, x.newValue);
//                                         }

//                                         Editor.CreateEditor(target).serializedObject.ApplyModifiedProperties();

//                                         serializedObject.ApplyModifiedProperties();

//                                         _dataObjectSerializedObj.ApplyModifiedProperties();

//                                         _defaultValue.objectReferenceValue = x.newValue;
                                        
//                                         _object.serializedObject.ApplyModifiedProperties();
//                                         _objectProp.serializedObject.ApplyModifiedProperties();
//                                         libraryAsset.serializedObject.ApplyModifiedProperties();

//                                         EditorUtility.SetDirty(target);

//                                     }
//                                     else
//                                     {
//                                         if (x.newValue == null)
//                                         {
//                                             (target as IExposedPropertyTable).SetReferenceValue(newPropertyName, null);
//                                         }
//                                     }
//                                 });




//                                 var _dataObjectTitle = new Label();
//                                 _dataObjectTitle.text = _sceneObjects[s].title;
//                                 _dataObjectTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
//                                 _dataObjectTitle.style.marginLeft = 3;


//                                 var _typeLabel = new Label();
//                                 _typeLabel.text = "Type: " + _tt.Name;

//                                 var _rightContainer = new VisualElement();
//                                 _rightContainer.style.flexGrow = 1;
//                                 var _leftContainer = new VisualElement();

//                                 // Load type icon
//                                 Texture _typeIcon = null;

//                                 var _res = EditorGUIUtility.Load(_tt.Name + " Icon");
//                                 if (_res != null)
//                                 {
//                                     _typeIcon = EditorGUIUtility.IconContent(_tt.Name + " Icon").image;
//                                 }

//                                 if (_typeIcon != null)
//                                 {
//                                     var _iconElement = new VisualElement();
//                                     _iconElement.style.backgroundImage = (Texture2D)_typeIcon;
//                                     _iconElement.style.width = 20;
//                                     _iconElement.style.height = 20;
//                                     DatabrainHelpers.SetMargin(_iconElement, 4, 4, 6, 4);
//                                     _leftContainer.Add(_iconElement);
//                                     _item.Add(_leftContainer);
//                                 }

//                                 _rightContainer.Add(_dataObjectTitle);
//                                 _rightContainer.Add(_pfObject);

//                                 _item.Add(_rightContainer);


//                                 _exposedReferencesFoldout.AddContent(_item);
//                             }
//                         }
//                         // ONLY DEBUG
//                         //var _ooo = new PropertyField(iterator);
//                         //_ooo.BindProperty(iterator);
//                         //_ooo.label = iterator.displayName;

//                         //Debug.Log(iterator.propertyPath);

//                         //root.Add(_ooo);
//                     }


                  
                   

//                     _dataObjectSerializedObj.ApplyModifiedProperties();
//                     serializedObject.ApplyModifiedProperties();
//                 }


//                 root.Add(_expRefsContainer);

//                 //Unity OnComplete Events
//                 var _unityOnCompleteEventsFoldout = new FoldoutElement("Unity OnComplete Events", (x) =>
//                 {
//                     (target as LogicController).unityEventsFoldout = x;
//                 });

//                 _unityOnCompleteEventsFoldout.OpenFoldout((target as LogicController).unityEventsFoldout);

//                 var _unityEventsProp = new PropertyField();
//                 _unityEventsProp.style.marginTop = 5;
//                 _unityEventsProp.BindProperty(serializedObject.FindProperty("onCompleteUnityEvent"));
//                 _unityOnCompleteEventsFoldout.AddContent(_unityEventsProp);


//                 root.Add(_unityOnCompleteEventsFoldout);


//                 // only debug
//                 //root.Add(_imgui);
//             }
//         }


//         #region DEBUG
//         public void IMGUI()
//         {
//             EditorGUI.BeginChangeCheck();

//             base.DrawDefaultInspector();


//             //if (EditorGUI.EndChangeCheck())
//             //{
//             //    if (assetProperty != null && assetProperty.objectReferenceValue != null)
//             //    {
//             //        assetSerializedObject = new SerializedObject(assetProperty.objectReferenceValue, this.target);
//             //    }
//             //    else
//             //    {
//             //        assetSerializedObject = null;
//             //    }
//             //}


//             //if (assetSerializedObject != null)
//             //{
//             //    var iterator = assetSerializedObject.GetIterator();
//             //    bool enterChildren = true;
//             //    while (iterator.NextVisible(enterChildren))
//             //    {
//             //        enterChildren = false;
//             //        EditorGUILayout.PropertyField(iterator, includeChildren: true);
//             //    }
//             //}

//             if (libraryAsset != null)
//             {
               
//                 var _sceneObjects = (libraryAsset.objectReferenceValue as DataLibrary).GetAllInitialDataObjectsByType(typeof(SceneComponent), true);

//                 for (int i = 0; i < _sceneObjects.Count; i++)
//                 {
//                     var _s = new SerializedObject(_sceneObjects[i]);
//                      //var _s  = this.serializedObject.FindProperty(propertyPath: "data");
//                     var iterator = _s.GetIterator();
//                     bool enterChildren = true;
//                     while (iterator.NextVisible(enterChildren))
//                     {
//                         enterChildren = false;
//                         EditorGUILayout.PropertyField(iterator, includeChildren: true);
//                     }
//                 }
//             }

//             serializedObject.Update();
//             serializedObject.ApplyModifiedProperties();
//         }
//         #endregion
//     }
// }
// #endif