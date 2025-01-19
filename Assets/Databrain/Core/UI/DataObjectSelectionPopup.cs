/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Databrain.Attributes;
using Databrain.Core.UI;
using Databrain.Helpers;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Databrain.UI
{
    public class DataObjectSelectionPopup : PopupWindowContent
    {
        private DataLibrary dataLibrary;
        private SerializedProperty property;
        private Type dataType;
        private Action<int> updateSelectedIndex;
        private string searchInput = "";
        private VisualElement root;
        private ScrollView scrollView;
        private int searchIndex;
        private bool includeSubtypes;
        
        private DataObjectCategoryTree collectedNodes;
        private string currentPath = "";
        private string currentCompletePath = "";
        private DataObjectCategoryTree currentCategory;
        
        private static Vector2 position;

        private List<string> subTypesFilter = new List<string>(){"All"};
        private string selectedSubType = "All";
        private DataObjectDropdownAttribute attribute;

        public DataObjectSelectionPopup(Type dataType, DataLibrary dataLibrary, SerializedProperty property, DataObjectDropdownAttribute attribute, Action<int> updateSelectedIndex, bool includeSubtypes)
        {
            this.dataType = dataType;
            this.dataLibrary = dataLibrary;
            this.property = property;
            //this.onCloseCallback = onCloseCallback;
            this.includeSubtypes = includeSubtypes;
            this.updateSelectedIndex = updateSelectedIndex;
            this.attribute = attribute;

        }
        public static void ShowPanel(Vector2 _pos, DataObjectSelectionPopup _panel)
        {
            position = _pos;
            UnityEditor.PopupWindow.Show(new Rect(_pos.x, _pos.y, 0, 0), _panel);
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(300, 350);
        }

        public override void OnGUI(Rect rect) { }

        public override void OnOpen()
        {
            searchIndex = -1;
            subTypesFilter = new List<string>();

            root = editorWindow.rootVisualElement;
            scrollView = new ScrollView();
            scrollView.style.flexGrow = 1;
            DatabrainHelpers.SetPadding(scrollView, 5, 5, 5, 5);

            var _searchContainer = new VisualElement();
            _searchContainer.style.flexDirection = FlexDirection.Row;
            //_searchContainer.style.flexGrow = 1;
            _searchContainer.style.height = 20;
            _searchContainer.style.minHeight = 20;
            DatabrainHelpers.SetMargin(_searchContainer, 4, 4, 4, 4);

            var _searchIcon = new VisualElement();
            _searchIcon.style.backgroundImage = DatabrainHelpers.LoadIcon("search");
            _searchIcon.style.width = 20;
            _searchIcon.style.minWidth = 20;
            _searchIcon.style.height = 20;

            var _searchField = new TextField();
            _searchField.style.maxHeight = 20;
            _searchField.style.flexGrow = 1;
            _searchField.RegisterCallback<KeyDownEvent>(x =>
            {
                if (x.keyCode == KeyCode.Return)
                {
                    if (!string.IsNullOrEmpty(searchInput) && searchIndex > -1)
                    {
                        var _availableObjects = dataLibrary.GetAllInitialDataObjectsByType(dataType, includeSubtypes);
                        property.objectReferenceValue = _availableObjects[searchIndex];

                        property.serializedObject.ApplyModifiedProperties();
                        property.serializedObject.Update();

                        editorWindow.Close();
                    }
                }
            });

            _searchField.RegisterValueChangedCallback(x =>
            {
                if (x.newValue != x.previousValue)
                {
                    searchIndex = -1;
                    searchInput = x.newValue;
                    Draw();
                }
            });

            var _cancelSearch = DatabrainHelpers.DatabrainButton("");
            _cancelSearch.style.backgroundColor = new Color(0, 0, 0, 0);
            _cancelSearch.RegisterCallback<ClickEvent>(e =>
            {
                searchInput = "";
                _searchField.value = "";
            });
            _cancelSearch.style.minWidth = 20;
            _cancelSearch.style.width = 20;

            var _cancelSearchIcon = new VisualElement();
            _cancelSearchIcon.style.backgroundImage = DatabrainHelpers.LoadIcon("delete2");
            _cancelSearchIcon.style.width = 18;
            _cancelSearchIcon.style.height = 18;

            _cancelSearch.Add(_cancelSearchIcon);

            _searchContainer.Add(_searchIcon);
            _searchContainer.Add(_searchField);
            _searchContainer.Add(_cancelSearch);

            // subtypes filter
            Type[] _subTypes = null;
            if (includeSubtypes)
            {
                // get sub types
                _subTypes =  AppDomain.CurrentDomain.GetAssemblies()
						.SelectMany(assembly => assembly.GetTypes())
						.Where(type => type.IsSubclassOf(dataType)).ToArray();

                
                subTypesFilter.Add("All");

                for (int i = 0; i < _subTypes.Length; i++)
                {
                    subTypesFilter.Add(_subTypes[i].Name);
                }
            }

            root.Add(_searchContainer);

            if (subTypesFilter.Count > 1)
            {
                var _subTypesFilterContainer = new VisualElement();
                _subTypesFilterContainer.SetPadding(4, 4, 4, 4);
                _subTypesFilterContainer.SetMargin(0, 4, 0, 0);
                _subTypesFilterContainer.style.flexWrap = Wrap.Wrap;
                _subTypesFilterContainer.style.flexDirection = FlexDirection.Row;
                _subTypesFilterContainer.style.flexGrow = 0;
                _subTypesFilterContainer.style.flexShrink = 1;
                
                for (int i = 0; i < subTypesFilter.Count; i++)
                {
                    var _subTypesButton = new Button();
                    _subTypesButton.style.height = 18;
                    _subTypesButton.text = subTypesFilter[i];
                    _subTypesButton.RegisterCallback<ClickEvent>(evt => 
                    {
                        selectedSubType = _subTypesButton.text;
                        Draw();
                    });
                    
                    _subTypesFilterContainer.Add(_subTypesButton);  
                }

                root.Add(new Label("Subtypes:"){style = {paddingLeft = 4, unityFontStyleAndWeight = FontStyle.Bold}});
                root.Add(_subTypesFilterContainer);
            }
           
            root.Add(scrollView);

            Draw();
        }

        VisualElement SimpleButton(string _buttonTitle, TextAnchor _buttonTitleAnchor, int _arrowType = 0, Sprite _iconSprite = null)
        {
            var _button = new VisualElement();
            _button.style.flexDirection = FlexDirection.Row;
            _button.style.flexGrow = 1;
            switch (_arrowType)
            {
                case 0:
                    _button.style.backgroundColor = DatabrainColor.Clear.GetColor();
                    break;
                case 2:
                    _button.style.backgroundColor = DatabrainColor.DarkGrey.GetColor();
                    break;
                default:
                    _button.style.backgroundColor = DatabrainColor.LightGrey.GetColor();
                    break;
            }

            _button.RegisterCallback<MouseEnterEvent>(x =>
            {
                var _colBlue = DatabrainColor.Blue.GetColor();                
                _button.style.backgroundColor = new Color(_colBlue.r, _colBlue.g, _colBlue.b, 120f/255f);
            });

            _button.RegisterCallback<MouseLeaveEvent>(x =>
            {
                switch(_arrowType)
                {
                    case 0:
                        _button.style.backgroundColor = DatabrainColor.Clear.GetColor();
                        break;
                    case 2:
                        _button.style.backgroundColor = DatabrainColor.DarkGrey.GetColor();
                        break;
                    default:
                        _button.style.backgroundColor = DatabrainColor.LightGrey.GetColor();
                        break;
                }
               
            });

            var _icon = new VisualElement();
            _icon.name = "icon";
            DatabrainHelpers.SetMargin(_icon, 4, 4, 4, 4);
            _icon.style.width = 20;
            _icon.style.height = 20;
            if (_iconSprite != null)
            {
                _icon.style.backgroundImage = _iconSprite.texture;
            }
            else
            {
                _icon.SetRadius(10,10,10,10);
                _icon.style.backgroundColor = Color.white;
                _icon.style.width = 5;
                _icon.style.height = 5;
                _icon.style.alignSelf = Align.Center;
            }

            var _title = new Label();
            _title.text = _buttonTitle;
            _title.style.marginLeft = 4;
            _title.style.flexGrow = 1;
            _title.style.unityTextAlign = _buttonTitleAnchor;
            _title.style.whiteSpace = WhiteSpace.Normal;
            _title.style.flexWrap = Wrap.Wrap;

            DatabrainHelpers.SetBorder(_button, 0);
            DatabrainHelpers.SetBorderRadius(_button, 0, 0, 0, 0);
            DatabrainHelpers.SetMargin(_button, 0, 0, 0, 1);

            var _space = new VisualElement();
            _space.style.flexGrow = 1;

            if (_arrowType != 0)
            {
                var _arrow = new Label();
                _arrow.style.unityTextAlign = TextAnchor.MiddleLeft;
                _arrow.style.marginLeft = 4;
                _arrow.style.marginRight = 4;

                switch (_arrowType)
                {
                    case 1:
                        _arrow.text = ">";
                        _button.Add(_title);
                        _button.Add(_space);
                        _button.Add(_arrow);
                        break;
                    case 2:
                        _arrow.text = "<";
                        _button.Add(_arrow);
                        _button.Add(_title);
                        break;
                }           
            }
            else
            {
                _button.Add(_icon);
                _button.Add(_title);
            }
 

            return _button;
        }
        void Draw(DataObjectCategoryTree tree = null)
        {
            if (tree == null)
            {
                if (collectedNodes == null)
                {
                    var objects = dataLibrary.GetAllInitialDataObjectsByType(dataType, includeSubtypes);
                    collectedNodes = DataObjectCategoryTree.CollectNodes(objects);
                }
                tree = collectedNodes;
            }
            /*
            for (int i = -1; i < _availableObjects.Count; i++)
            {
                var _index = i;
                if (_index > -1)
                {
                    if (!string.IsNullOrEmpty(searchInput))
                    {
                        if (!_availableObjects[i].title.ToLower().Contains(searchInput.ToLower()))
                        {
                            continue;
                        }
                    }

                    if (!string.IsNullOrEmpty(selectedSubType) && selectedSubType != "All")
                    {
                        if (_availableObjects[i].GetType().Name != selectedSubType)
                        {
                            continue;
                        }
                    }
                }

                if (searchIndex == -1)
                {
                    searchIndex = i;
                }

                var _b = DatabrainHelpers.DatabrainButton("");
                _b.style.marginBottom = 2;
                _b.style.height = 24;

                if (i == -1)
                {
                    if (string.IsNullOrEmpty(searchInput))
                    {

                        _b.text = "- none -";
                        _b.RegisterCallback<ClickEvent>(x =>
                        {
                            
                            property.objectReferenceValue = null;

                            property.serializedObject.ApplyModifiedProperties();
                            property.serializedObject.Update();

                            updateSelectedIndex?.Invoke(-1);

                            editorWindow.Close();
                        });

                        scrollView.Add(_b);
                    }
                }
                else
                {


                    if (_availableObjects[_index].icon != null)
                    {
                        var _icon = new VisualElement();
                        _icon.style.backgroundImage = _availableObjects[_index].icon.texture;
                        _icon.style.width = 18;
                        _icon.style.height = 18;
                        _icon.style.marginLeft = 5;
                        _b.Add(_icon);
                    }

                    _b.text = _availableObjects[i].title;
                    _b.RegisterCallback<ClickEvent>(x =>
                    {
                        property.objectReferenceValue = _availableObjects[_index];
                        property.serializedObject.ApplyModifiedProperties();
                        property.serializedObject.Update();

                        updateSelectedIndex?.Invoke(_index);

                        editorWindow.Close();
                    });

                    scrollView.Add(_b);
                }
            }
*/
            
            scrollView.Clear();
            
            BuildBackButton(tree);
            BuildNone(tree);
            
            if (string.IsNullOrEmpty(searchInput))
            {
                BuildCategories(tree.categories.Values.ToList());
                BuildObjects(tree.dataObjectsInCategory);
            }
            // search string not empty
            // draw nodes in one list 
            else
            {
                
                BuildCategories(tree.GetAllChildsCategories(), true);
                BuildObjects(tree.GetAllChildsDataObjects(), true);
            }
        }

        private void BuildNone(DataObjectCategoryTree tree)
        {
            
            if (string.IsNullOrEmpty(tree.CompletePath) && 
                string.IsNullOrEmpty(searchInput))
            {

                var _b = DatabrainHelpers.DatabrainButton("");
                _b.style.marginBottom = 2;
                _b.style.height = 24;
                _b.text = "- none -";
                _b.RegisterCallback<ClickEvent>(x =>
                {
                            
                    property.objectReferenceValue = null;

                    property.serializedObject.ApplyModifiedProperties();
                    property.serializedObject.Update();

                    updateSelectedIndex?.Invoke(-1);

                    editorWindow.Close();
                });

                scrollView.Add(_b);
            }
        }
        private void BuildBackButton(DataObjectCategoryTree tree)
        {
            // Button back
            if (!string.IsNullOrEmpty(tree.CompletePath))
            {
                var _title = new Label();
                //_title.text = "/"+tree.CompletePath.Substring(0, tree.CompletePath.Length - tree.Path.Length);
                _title.text = tree.CompletePath;
                _title.style.marginLeft = 4;
                _title.style.flexGrow = 1;
                _title.style.whiteSpace = WhiteSpace.Normal;
                _title.style.flexWrap = Wrap.Wrap;
                var _alreadyAdded = scrollView.Q(currentPath);

                var _buttonBack = SimpleButton(currentPath, TextAnchor.MiddleCenter, 2);
                _buttonBack.style.height = 25;
                _buttonBack.name = currentPath;
                _buttonBack.RegisterCallback<ClickEvent>(click =>
                {
                    currentPath = currentCategory.parentGraphTree.Path;
                    currentCompletePath = currentCategory.parentGraphTree.CompletePath;
                    currentCategory = currentCategory.parentGraphTree;

                    Draw(currentCategory);
                });

                if (_alreadyAdded != null)
                {
                    _buttonBack.style.display = DisplayStyle.None;
                }
                scrollView.Add(_title);
                scrollView.Add(_buttonBack);
            }
        }
        void BuildCategories(List<DataObjectCategoryTree> categories, bool fullName = false)
        {
            foreach (var category in categories)
            {
                if (!string.IsNullOrEmpty(searchInput) &&
                    !category.Path.ToLower().Contains(searchInput.ToLower())) 
                    continue;
                
                var _categoryButton = SimpleButton(fullName?category.CompletePath:category.Path, TextAnchor.MiddleLeft, 1);
                _categoryButton.style.height = 25;
                _categoryButton.style.unityFontStyleAndWeight = FontStyle.Bold;
                _categoryButton.name = category.Path;
                var val = category;
                _categoryButton.RegisterCallback<ClickEvent>(click =>
                {
                    currentPath = val.Path;
                    currentCompletePath = val.CompletePath;
                    currentCategory = val;

                    Draw(currentCategory);
                });

                scrollView.Add(_categoryButton);
            }
        }
        void BuildObjects(List<DataObject> availableObjects, bool fullName = false)
        {
            for (int i = 0; i < availableObjects.Count; i++)
            {
                if (attribute != null)
                {
                    if (attribute.sceneComponentType != null)
                    {
                        #if DATABRAIN_LOGIC
                        if ((availableObjects[i] as Databrain.Logic.SceneComponent).CheckForType(attribute.sceneComponentType))
                        {
                            
                        }
                        else
                        {
                            continue;
                        }
                        #endif
                    }
                }
                var objectName = availableObjects[i].title.Substring(availableObjects[i].title.LastIndexOf('/') + 1);
                if (!string.IsNullOrEmpty(searchInput) &&
                    !objectName.ToLower().Contains(searchInput.ToLower())) 
                    continue;

                if (!string.IsNullOrEmpty(selectedSubType) && selectedSubType != "All" &&
                    (availableObjects[i].GetType().Name != selectedSubType)) 
                    continue;
                
                var _index = i;
                var _button = SimpleButton(fullName ? availableObjects[i].title : objectName, TextAnchor.MiddleLeft, 0, availableObjects[i].icon);
                _button.style.height = 25;
                _button.RegisterCallback<ClickEvent>(click =>
                {
                    Click(availableObjects[_index], _index);
                });
                    
                scrollView.Add(_button);
            }
        }
        protected void Click(DataObject dataObject = null, int _index = -1)
        {
            property.objectReferenceValue = dataObject;
            property.serializedObject.ApplyModifiedProperties();
            property.serializedObject.Update();

            updateSelectedIndex?.Invoke(_index);
            
            editorWindow.Close();
        }
        public override void OnClose()
        {
            collectedNodes = null;
            //onCloseCallback?.Invoke();
        }
    }
}
#endif