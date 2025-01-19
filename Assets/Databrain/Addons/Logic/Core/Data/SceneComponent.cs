/*
 *	DATABRAIN | Logic
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
using System;
using System.Linq;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

using Databrain.Helpers;
using Databrain.Attributes;

namespace Databrain.Logic
{
    [DataObjectTypeName("Components")]
    [DataObjectHideBaseFields(_hideIconField: true)]
    [DataObjectSmallListItem]
    [DataObjectIcon("category", DatabrainColor.Coal)]
    [DataObjectOrder(301)]
    public class SceneComponent : DataObject
    {

        [System.Serializable]
        public class ExposedReferenceData
        {
            public string typeName;
            public GenericExposedReference<UnityEngine.Object> referenceObject;

            public ExposedReferenceData(string _name, Type _typeof)
            {
                typeName = _typeof.AssemblyQualifiedName;
                referenceObject = new GenericExposedReference<UnityEngine.Object>(_name);
            }
        }

        [Hide]
        public List<ExposedReferenceData> exposedReferences = new List<ExposedReferenceData>();

        private DropdownField typeDropdown;

       

        public T GetReference<T>(NodeData node) where T : UnityEngine.Object
        {
            return GetReference<T>(node.graphData);
        }

        public T GetReference<T>(GraphData _graph) where T : UnityEngine.Object
        {
            if (_graph.editorExecution)
            {
                var _obj2 = _graph.logicController.GetReferenceValue(new PropertyName(guid), out bool _valid);

                if (_obj2 != null)
                {
                    return (T)Convert.ChangeType(_obj2, typeof(T));
                }
                else
                {
                    return null;
                }
            }
            else
            {

                var _obj = exposedReferences.First().referenceObject.ExposedReference.Resolve(_graph.ExposedPropertyTable);
                
                if (_obj != null)
                {
                    return (T)Convert.ChangeType(_obj, typeof(T));
                }
                else
                {
                    return null;
                }
            }
        }


#if UNITY_EDITOR

        public override VisualElement EditorGUI(SerializedObject _serializedObject, DatabrainEditorWindow _editorWindow)
        {
            var _root = new VisualElement();


            if (typeDropdown == null)
            {
                typeDropdown = new DropdownField("Type");
            }

            _root.Add(typeDropdown);


            UpdateDropdown();


            return _root;
        }

        public override bool CheckForType(Type _type)
        {
            if (exposedReferences.Count > 0)
            {
                if (this.exposedReferences[0].typeName == _type.AssemblyQualifiedName)
                {
                    return true;
                }
            }

            return false;
        }

        public override void OnAddCallback(System.Type _componentType) 
        {
            // Only add one entry
            if (exposedReferences.Count > 0)
            {
                exposedReferences.RemoveAt(0);
            }

            exposedReferences.Add(new SceneComponent.ExposedReferenceData(_componentType.Name, _componentType));
        }


        public override Texture2D ReturnIcon()
        {
            if (exposedReferences.Count > 0)
            {
                var _res = EditorGUIUtility.Load(this.exposedReferences[0].referenceObject.name + " Icon");
                if (_res != null)
                {
                    return (Texture2D)EditorGUIUtility.IconContent(this.exposedReferences[0].referenceObject.name + " Icon").image;
                }
            }

            return null;
        }

        public void UpdateDropdown()
        {
            var _labelChild = typeDropdown.ElementAt(1);


            if (this.exposedReferences.Count > 0)
            {
                Texture _iconTexture = null;

                VisualElement _ic = null;

                _ic = typeDropdown.Q<VisualElement>("icon");
                if (_ic == null)
                {
                    _ic = new VisualElement();
                    _ic.name = "icon";
                    _ic.style.width = 20;
                    _ic.style.height = 20;
                }


                var _res = EditorGUIUtility.Load(this.exposedReferences[0].referenceObject.name + " Icon");
                if (_res != null)
                {
                    _iconTexture = EditorGUIUtility.IconContent(this.exposedReferences[0].referenceObject.name + " Icon").image;
                }
                else
                {
                    _iconTexture = EditorGUIUtility.IconContent("cs Script Icon").image;
                }

                _ic.style.backgroundImage = (Texture2D)_iconTexture;
                _labelChild.Insert(0, _ic);

                typeDropdown.value = this.exposedReferences[0].referenceObject.name;
            }
            else
            {
                typeDropdown.value = "Null";
            }
            typeDropdown.RegisterCallback<ClickEvent>(click =>
            {
                var _panel = new ShowComponentTypesPopup(exposedReferences, this);
                var _position = typeDropdown.LocalToWorld(typeDropdown.transform.position);
                _position = new Vector2(_position.x + 130, _position.y + 24);
                ShowComponentTypesPopup.ShowPanel(_position, _panel); 
            });

            var _editor = DatabrainHelpers.GetOpenEditor(relatedLibraryObject.GetInstanceID());
            if (_editor != null)
            {
                _editor.dataTypelistView.RefreshItem(index);
            }
        }
#endif
    }

#if UNITY_EDITOR
    public class ShowComponentTypesPopup : PopupWindowContent
    {
        private List<SceneComponent.ExposedReferenceData> exposedReferences;
        private DataObject dataObject;

        private ScrollView scrollView;
        private Type[] componentTypes;

        private string searchString = "";


        Dictionary<string, List<Type>> components = new Dictionary<string, List<Type>>();

        List<string> commonTypes = new List<string>() { "GameObject", "Transform", "Camera", "Light", "AudioListener", "MeshRenderer", "BoxCollider", "SphereCollider", "MeshCollider" };
        List<string> commonTypesIcons = new List<string>() { "GameObject Icon", "d_Transform Icon", "Camera Icon", "Light Icon", "AudioListener Icon", "MeshRenderer Icon", "BoxCollider Icon", "SphereCollider Icon", "MeshCollider Icon" };

        public static void ShowPanel(Vector2 _pos, ShowComponentTypesPopup _panel)
        {
            UnityEditor.PopupWindow.Show(new Rect(_pos.x, _pos.y, 0, 0), _panel);
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(500, 600);
        }

        public override void OnGUI(Rect rect){   }

        public override void OnOpen() 
        {
            //var _root = editorWindow.rootVisualElement;
            var _root = new VisualElement();

            scrollView = new ScrollView();
            DatabrainHelpers.SetMargin(scrollView, 0, 0, 10, 0);

            var _container = new VisualElement();
            DatabrainHelpers.SetMargin(_container, 5, 5, 5, 5);

            var _searchContainer = new VisualElement();
            _searchContainer.style.flexGrow = 1;
            _searchContainer.style.flexDirection = FlexDirection.Row;
            _searchContainer.style.height = 26;
            _searchContainer.style.minHeight = 26;

            var _searchIcon = new VisualElement();
            _searchIcon.style.backgroundImage = DatabrainHelpers.LoadIcon("search");
            _searchIcon.style.width = 20;
            _searchIcon.style.height = 20;


            var _searchField = new TextField();
            _searchField.style.marginBottom = 5;
            _searchField.style.flexGrow = 1;
            _searchField.style.minHeight = 20;
            _searchField.RegisterValueChangedCallback(x =>
            {
                if (string.IsNullOrEmpty(x.newValue))
                {
                    searchString = "";
                }
                else
                {
                    searchString = x.newValue;
                }

                Build();
            });

            var _searchCancel = new Button();
            _searchCancel.text = "x";
            _searchCancel.style.width = 20;
            _searchCancel.style.height = 20;
            _searchCancel.RegisterCallback<ClickEvent>(c =>
            {
                searchString = "";
                _searchField.value = "";
                Build();
            });

            _searchContainer.Add(_searchIcon);
            _searchContainer.Add(_searchField);
            _searchContainer.Add(_searchCancel);


            //var _space = new VisualElement();
            //_space.style.height = 15;




            _container.Add(_searchContainer);
            _container.Add(DatabrainHelpers.Separator(1, DatabrainColor.Grey.GetColor()));
            //_container.Add(_space);
            _container.Add(scrollView);

            _root.Add(_container);
          
            editorWindow.rootVisualElement.Add(_root);

            Build();
        }

        void Build()
        {
            scrollView.Clear();

            // Add common types
            for (int i = 0; i < commonTypes.Count; i++)
            {
                if (commonTypes[i].ToLower().Contains(searchString.ToLower()))
                {
                    var _index = i;
                    var _commonTypesButton = IconButton(commonTypes[i], EditorGUIUtility.IconContent(commonTypesIcons[i]).image);
                    //_commonTypesButton.text = commonTypes[i];

                    _commonTypesButton.RegisterCallback<ClickEvent>(click =>
                    {
                        var _icon = EditorGUIUtility.IconContent(commonTypesIcons[_index]).image;
                        dataObject.icon = Sprite.Create((Texture2D)_icon, new Rect(0, 0, _icon.width, _icon.height), Vector2.zero);

                        var _t = Type.GetType("UnityEngine." + commonTypes[_index] + ", UnityEngine");

                        AddExposedReference(commonTypes[_index], _t);

                        (dataObject as SceneComponent).UpdateDropdown();

                        editorWindow.Close();
                    });

                    scrollView.Add(_commonTypesButton);
                }
            }



            foreach (var _key in components.Keys)
            {

                var _availableTypes = 0;
                foreach (var type in components[_key])
                {
                    if (type.Name.ToLower().Contains(searchString.ToLower()))
                    {
                        _availableTypes++;
                    }
                }

                if (_availableTypes > 0)
                {

                    var _namespaceFoldout = new Foldout();
                   
                    if (string.IsNullOrEmpty(searchString))
                    {
                        _namespaceFoldout.value = false;
                    }
                    else
                    {
                        _namespaceFoldout.value = true;


                        foreach (var type in components[_key])
                        {
                            if (type.Name.ToLower().Contains(searchString.ToLower()))
                            {
                                _namespaceFoldout.Add(TypeButton(type));
                            }
                        }

                    }
                    _namespaceFoldout.text = _key;
                    _namespaceFoldout.RegisterValueChangedCallback<bool>(value =>
                    {
                        if (value.newValue && value.newValue != value.previousValue)
                        {
                            foreach (var type in components[_key])
                            {
                                if (type.Name.ToLower().Contains(searchString.ToLower()))
                                {
                                    _namespaceFoldout.Add(TypeButton(type));
                                }
                            }
                        }
                    });

                    
                    scrollView.Add(_namespaceFoldout);
                }
            }
        }

        VisualElement TypeButton(Type type)
        {
            Texture _icon = null;

            var _res = EditorGUIUtility.Load(type.Name + " Icon");
            if (_res != null)
            {
                _icon = EditorGUIUtility.IconContent(type.Name + " Icon").image;
            }
            else
            {
                _icon = EditorGUIUtility.IconContent("cs Script Icon").image;
            }

            var _typeButton = IconButton(type.Name, _icon);

            //_typeButton.text = type.Name;
            _typeButton.RegisterCallback<ClickEvent>(click =>
            {
                if (_icon != null)
                {
                    dataObject.icon = Sprite.Create((Texture2D)_icon, new Rect(0, 0, _icon.width, _icon.height), Vector2.zero);
                }
                else
                {
                    var _res = EditorGUIUtility.Load("cs Script Icon");
                    if (_res != null)
                    {
                        _icon = EditorGUIUtility.IconContent("cs Script Icon").image;
                    }

                    dataObject.icon = Sprite.Create((Texture2D)_icon, new Rect(0, 0, _icon.width, _icon.height), Vector2.zero);
                }

                AddExposedReference(type.Name, type);

                (dataObject as SceneComponent).UpdateDropdown();

                editorWindow.Close();
            });

            return _typeButton;
        }

        VisualElement IconButton(string _title, Texture _icon = null)
        {
            var _root = new VisualElement();
            _root.style.flexDirection = FlexDirection.Row;
            _root.style.backgroundColor = DatabrainColor.Grey.GetColor();
            DatabrainHelpers.SetMargin(_root, 1, 1, 1, 1);
            DatabrainHelpers.SetPadding(_root, 5, 5, 5, 5);

            _root.RegisterCallback<MouseEnterEvent>(e =>
            {
                _root.style.backgroundColor = DatabrainColor.LightGrey.GetColor();
            });

            _root.RegisterCallback<MouseLeaveEvent>(e =>
            {
                _root.style.backgroundColor = DatabrainColor.Grey.GetColor();
            });

            if (_icon != null)
            {
                var _iconElement = new VisualElement();
                _iconElement.style.backgroundImage = (Texture2D)_icon;
                _iconElement.style.width = 20;
                _iconElement.style.height = 20;
                _iconElement.style.marginRight = 5;

                
                _root.Add(_iconElement);
            }

            var _label = new Label();
            _label.text = _title;
            _label.style.unityTextAlign = TextAnchor.MiddleLeft;

            _root.Add(_label);

            return _root;
        }


        public override void OnClose(){}


        void AddExposedReference(string _name, Type _typeof)
        {
            // Only add one entry
            if (exposedReferences.Count > 0)
            {
                exposedReferences.RemoveAt(0);
            }

            exposedReferences.Add(new SceneComponent.ExposedReferenceData(_name, _typeof));
        }


        public ShowComponentTypesPopup(List<SceneComponent.ExposedReferenceData> _exposedReferences, DataObject _dataObject)
        {
            dataObject = _dataObject;
            exposedReferences = _exposedReferences;

            componentTypes = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic)
                // alternative: .GetExportedTypes()
                .SelectMany(domainAssembly => domainAssembly.GetExportedTypes()) //.GetTypes())
                .Where(type => typeof(UnityEngine.Component).IsAssignableFrom(type)
                // alternative: => type.IsSubclassOf(typeof(B))
                // alternative: && type != typeof(B)
                // alternative: && ! type.IsAbstract
                ).ToArray();


            components = new Dictionary<string, List<Type>>();

            for (int c = 0; c < componentTypes.Length; c ++)
            {
                //Debug.Log(componentTypes[c].Name + " __ " + componentTypes[c].Namespace);
                //Debug.Log(componentTypes[c].Namespace);

                var _namespace = componentTypes[c].Namespace;
                if (string.IsNullOrEmpty(componentTypes[c].Namespace))
                {
                    _namespace = "Global";
                }     

                if (components.ContainsKey(_namespace))
                {
                    components[_namespace].Add(componentTypes[c]);
                }
                else
                {
                    var _ls = new List<Type>();
                    _ls.Add(componentTypes[c]);
                    components.Add(_namespace, _ls);

                }
            }

            //componentTypesString = componentTypes.ToList().Select(x => x.Name).ToList();
        }
    }
#endif
}