/*
 *	DATABRAIN | Logic
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

using Databrain.Helpers;
using Databrain.Attributes;
using Databrain.Logic.Attributes;
using Databrain.Logic.Utils;
using System.Linq;
using Databrain.UI;

namespace Databrain.Logic.Elements
{
    public class NodePanel : PopupWindowContent
    {

        private NodeCategoryTree collectedNodes;
        int currentDepth = 0;
        string currentPath;
        string currentCompletePath;
        NodeCategoryTree currentCategory;

        private static Vector2 position;
        public string searchString = "";
        private NodeEditor nodeEditor;
        private NodeData nodeData; // Node data of start node

        private Action onClose;
        private int outputIndex;

        // private VisualElement header;
        private VisualElement specialNodesContainer;
        private ScrollView scrollView;
        private Label nodeDescription;

        Type firstFoundNode;

        public class NodeCategory
        {
            public string category;
            public List<Type> availableNodes = new List<Type>();


            public NodeCategory(string category, Type type)
            {
                this.category = category;
                this.availableNodes.Add(type);
            }    
        }

        public List<NodeCategory> nodeCategories = new List<NodeCategory>();

        public static void ShowPanel(Vector2 _pos, NodePanel _panel)
        {
            position = _pos;
            UnityEditor.PopupWindow.Show(new Rect(_pos.x, _pos.y, 0, 0), _panel);
         }


        public override Vector2 GetWindowSize()
        {
            return new Vector2(250, 400);
        }

        public override void OnGUI(Rect rect){}

        void AddNode(Type _type)
        {
            var _newNode = nodeEditor.CreateNewNode(_type, position);

            // var _outputConnectionTypeAttribute = nodeData.GetType().GetCustomAttribute<NodeOutputConnectionType>();
            // var _outputConnectionOK = true;
            // if (_outputConnectionTypeAttribute != null)
            // {
            //     var _foundCompatibleOutputType = _outputConnectionTypeAttribute.types.Where(x => x == _newNode.GetType()).FirstOrDefault();
            //     if (_foundCompatibleOutputType == null)
            //     {
            //         _outputConnectionOK = false;
            //     }
            // }

            if (nodeData != null)
            {
                nodeData.ConnectToNode(outputIndex, _newNode);
                Connect();
            }

            editorWindow.Close();
        }

        void AddCommentNode()
        {
            var _commentNode = nodeEditor.CreateComment(position);

            editorWindow.Close();
        }

        async void Connect()
        {
            await Task.Delay(200);
            nodeData.nodeVisualElement.BuildConnections();
        }


        public override void OnOpen()
        {

            var _root = editorWindow.rootVisualElement;
            scrollView = new ScrollView();
         
            var _space = new VisualElement();
            _space.style.height = 5;
            _space.style.minHeight = 5;

            var _container = new VisualElement();

            DatabrainHelpers.SetMargin(_container, 5, 5, 5, 5);

            var _searchContainer = new VisualElement();
            _searchContainer.style.flexGrow = 1;
            _searchContainer.style.flexDirection = FlexDirection.Row;
            _searchContainer.SetMargin(0, 5, 0, 0);

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
                    specialNodesContainer.style.display = DisplayStyle.Flex;


                    firstFoundNode = null;

                    collectedNodes = LogicCollectNodes.CollectNodes();
                    Draw(collectedNodes);
                }
                else
                {
                    searchString = x.newValue;
                    specialNodesContainer.style.display = DisplayStyle.None;

                  
                    firstFoundNode = null;
                    if (nodeData != null)
                    {
                        var _outputConnectionType = nodeData.GetType().GetCustomAttribute<NodeOutputConnectionType>();
                        if (_outputConnectionType != null)
                        {
                            collectedNodes = LogicCollectNodes.CollectNodes(searchString, _outputConnectionType.types);
                        }
                        else
                        {
                            collectedNodes = LogicCollectNodes.CollectNodes(searchString);
                        }
                    }
                    else
                    {
                        collectedNodes = LogicCollectNodes.CollectNodes(searchString);
                    }

                    Draw(collectedNodes);
                }

                // var _outputConnectionType = nodeData.GetType().GetCustomAttribute<NodeOutputConnectionType>();
                // if (_outputConnectionType != null)
                // {
                //     searchString = _outputConnectionType.types[0].Name;
                //     Debug.Log(searchString);
                // }
            });
            _searchField.RegisterCallback<KeyDownEvent>(OnKeyDownShortcut);

            FocusSearchFieldDelayed(_searchField);

            _searchContainer.Add(_searchIcon);
            _searchContainer.Add(_searchField);


            SpecialNodes();

            _container.Add(_searchContainer);
            _container.Add(_space);
            _container.Add(specialNodesContainer);
            _container.Add(scrollView);

            nodeDescription = new Label();
            nodeDescription.style.whiteSpace = WhiteSpace.Normal;
            nodeDescription.style.flexShrink = 0;
            nodeDescription.style.minHeight = 30;

            _container.Add(DatabrainHelpers.Separator(1, DatabrainColor.LightGrey.GetColor()));
            _container.Add(nodeDescription);

            _root.Add(_container);


            if (nodeData != null)
            {
                var _outputConnectionType = nodeData.GetType().GetCustomAttribute<NodeOutputConnectionType>();
                if (_outputConnectionType != null)
                {
                    collectedNodes = LogicCollectNodes.CollectNodes(searchString, _outputConnectionType.types);
                }
            }

            Draw(collectedNodes);

        }

        private async void FocusSearchFieldDelayed(TextField _textField)
        {
            await System.Threading.Tasks.Task.Delay(100);
            _textField.Q("unity-text-input").Focus();
        }

        private void OnKeyDownShortcut(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Return)
            {
                if (firstFoundNode != null)
                {
                    AddNode(firstFoundNode);
                }
            }
        }


        void SpecialNodes()
        {
            specialNodesContainer = new VisualElement();
            specialNodesContainer.style.display = DisplayStyle.Flex;
            specialNodesContainer.style.flexShrink = 0;

            var _button = SimpleButton("Comment", TextAnchor.MiddleLeft);
            _button.style.minHeight = 30;
            var _icon = _button.Q<VisualElement>("icon");
            var iconTexture = DatabrainHelpers.LoadIcon("comment");
            _icon.style.backgroundImage = iconTexture;


            _button.RegisterCallback<ClickEvent>(evt =>
            {
                AddCommentNode();
            });

            specialNodesContainer.Add(_button);

            //return specialNodesContainer;;
        }


        void LoadNodeIcon(VisualElement _target, Type _nodeType)
        {
            var _nodeIconAttribute = _nodeType.GetCustomAttribute<NodeIcon>();
            var _nodeColorAttribute = _nodeType.GetCustomAttribute<NodeColor>();

            if (_nodeColorAttribute != null)
            {
                Color _color = new Color(0f,0f,0f,0f);
                if (!string.IsNullOrEmpty(_nodeColorAttribute.color))
                {
                    ColorUtility.TryParseHtmlString(_nodeColorAttribute.color, out _color);
                    _target.style.unityBackgroundImageTintColor = _color;
                }
                else
                { 
                    _target.style.unityBackgroundImageTintColor = _nodeColorAttribute.databrainColor.GetColor();
                }      
            }

           

            if (_nodeIconAttribute != null)
            {
                Texture2D icon = null;
                if (string.IsNullOrEmpty(_nodeIconAttribute.rootFile))
                {
                    icon = DatabrainHelpers.LoadIcon(_nodeIconAttribute.icon);
                }
                else
                {
                    icon = DatabrainHelpers.LoadTexture(_nodeIconAttribute.icon, _nodeIconAttribute.rootFile);
                }

                _target.style.backgroundImage = icon;
            } 
            else
            {
                var icon = DatabrainHelpers.LoadIcon("nodes");
                _target.style.backgroundImage = icon;
            }
        }

     
        VisualElement SimpleButton(string _buttonTitle, TextAnchor _buttonTitleAnchor, int _arrowType = 0)
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

            var _title = new Label();
            _title.text = _buttonTitle;
            _title.style.marginLeft = 4;
            _title.style.flexGrow = 1;
            _title.style.unityTextAlign = _buttonTitleAnchor;

            

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


        public void Draw(NodeCategoryTree tree)
        {
            scrollView.Clear();
            tree.Traverse(DrawGUI);
        }

        void DrawGUI(int depth, NodeCategoryTree tree)
        {
            if (depth == -1)
                return;

           

            if (string.IsNullOrEmpty(searchString))
            {
                if (currentDepth - 1 >= 0)
                {
                    if (currentPath == tree.Path)
                    {
                         var _alreadyAdded = scrollView.Q(currentPath);

                        // Button back
                        var _buttonBack = SimpleButton(currentPath, TextAnchor.MiddleCenter, 2);
                        _buttonBack.style.height = 25;
                        _buttonBack.name = currentPath;
                        _buttonBack.RegisterCallback<ClickEvent>(click =>
                        {
                            currentDepth--;
                            currentPath = currentCategory.parentGraphTree.Path;
                            currentCategory = currentCategory.parentGraphTree;

                            Draw(collectedNodes);
                        });

                        if (_alreadyAdded != null)
                        {
                            _buttonBack.style.display = DisplayStyle.None;
                        }
                        scrollView.Add(_buttonBack);
                    }
                }


                if (depth == currentDepth && currentPath == tree.parentGraphTree.Path)
                {
                   
                        // Draw Categories
                        BuildCategories(tree, depth);
                    
                }


                if (currentPath == tree.Path)
                {
                    tree.nodesInCategory = tree.nodesInCategory.OrderBy(c => c.order).ToList();
                    
                    // Draw nodes
                    for (int n = 0; n < tree.nodesInCategory.Count; n++)
                    {
                       
                        if ( currentCompletePath == tree.nodesInCategory[n].category)
                        {
                            var _index = n;
                            var _button = SimpleButton(tree.nodesInCategory[n].title, TextAnchor.MiddleLeft);
                            _button.style.height = 25;
                            _button.RegisterCallback<ClickEvent>(click =>
                            {
                                AddNode(tree.nodesInCategory[_index].nodeType);
                            });

                            _button.RegisterCallback<MouseEnterEvent>(x =>
                            {
                                var _descAttribute = tree.nodesInCategory[_index].nodeType.GetCustomAttribute<NodeDescription>();
                                if (_descAttribute != null)
                                {
                                    nodeDescription.text = _descAttribute.description;
                                }
                                else
                                {
                                    nodeDescription.text = "";
                                }
                            });

                            _button.RegisterCallback<MouseLeaveEvent>(x =>
                            {
                                nodeDescription.text = "";
                            });

                            var _icon = _button.Q<VisualElement>("icon");
                            LoadNodeIcon(_icon, tree.nodesInCategory[n].nodeType);

                            scrollView.Add(_button);
                        }
                    }
                }
            }
            // search string not empty
            // draw nodes in one list 
            else
            {
                if (tree.nodesInCategory.Count == 0)
                    return;

                var _catLabel = new Label();
                _catLabel.style.marginTop = 5;
                _catLabel.text = string.IsNullOrEmpty(tree.parentGraphTree.Path) ? tree.Path : tree.parentGraphTree.Path + " / " + tree.Path;
                _catLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

                scrollView.SetMargin(10, 0, 0, 0);
                scrollView.Add(_catLabel);
                scrollView.Add(DatabrainHelpers.Separator(1, Color.black));

                tree.nodesInCategory = tree.nodesInCategory.OrderBy(c => c.order).ToList();

                for (int n = 0; n < tree.nodesInCategory.Count; n++)
                {
                    if (firstFoundNode == null)
                    {
                        firstFoundNode = tree.nodesInCategory[n].nodeType;
                    }

                    var _index = n;
                    var _button = SimpleButton(tree.nodesInCategory[n].title, TextAnchor.MiddleLeft);
                    _button.style.height = 25;
                    _button.RegisterCallback<ClickEvent>(click =>
                    {
                        AddNode(tree.nodesInCategory[_index].nodeType);
                    });

                    _button.RegisterCallback<MouseEnterEvent>(x =>
                    {
                        var _descAttribute = tree.nodesInCategory[_index].nodeType.GetCustomAttribute<NodeDescription>();
                        if (_descAttribute != null)
                        {
                            nodeDescription.text = _descAttribute.description;
                        }
                        else
                        {
                            nodeDescription.text = "";
                        }
                    });

                    var _icon = _button.Q<VisualElement>("icon");
                    LoadNodeIcon(_icon, tree.nodesInCategory[n].nodeType);

                    scrollView.Add(_button);

                }
            }
        }

        void BuildCategories(NodeCategoryTree tree, int depth)
        {
            
            if (string.IsNullOrEmpty(tree.Path) && depth == 0)
            {
                // Draw nodes in root panel because they are not in any category

                tree.nodesInCategory = tree.nodesInCategory.OrderBy(c => c.order).ToList();

                for (int n = 0; n < tree.nodesInCategory.Count; n++)
                {
                    var _index = n;
                    var _button = SimpleButton(tree.nodesInCategory[n].title, TextAnchor.MiddleLeft);
                    _button.style.height = 25;
                    _button.RegisterCallback<ClickEvent>(click =>
                    {
                        AddNode(tree.nodesInCategory[_index].nodeType);
                    });

                    _button.RegisterCallback<MouseEnterEvent>(x =>
                    {
                        var _descAttribute = tree.nodesInCategory[_index].nodeType.GetCustomAttribute<NodeDescription>();
                        if (_descAttribute != null)
                        {
                            nodeDescription.text = _descAttribute.description;
                        }
                        else
                        {
                            nodeDescription.text = "";
                        }
                    });

                     _button.RegisterCallback<MouseLeaveEvent>(x =>
                    {
                        nodeDescription.text = "";
                    });

                    var _icon = _button.Q<VisualElement>("icon");
                    LoadNodeIcon(_icon, tree.nodesInCategory[n].nodeType);

                    scrollView.Add(_button);

                }
            }
            else
            {
               
                    var _categoryButton = SimpleButton(tree.Path, TextAnchor.MiddleLeft, 1);
                    _categoryButton.style.height = 25;
                    _categoryButton.style.unityFontStyleAndWeight = FontStyle.Bold;
                    _categoryButton.name = tree.Path;
                    _categoryButton.RegisterCallback<ClickEvent>(click =>
                    {
                        currentDepth = depth + 1;
                        currentPath = tree.Path;
                        currentCompletePath = tree.CompletePath;
                        currentCategory = tree;

                        Draw(collectedNodes);

                    });

                    scrollView.Add(_categoryButton);
                
            }
        }



        public override void OnClose()
        {
            onClose?.Invoke();
        }

        public NodePanel(NodeEditor _nodeEditor, NodeData _nodeData, int _outputIndex, Action _onClose)
        {
            onClose = _onClose;
            nodeEditor = _nodeEditor;

            nodeData = _nodeData;
            outputIndex = _outputIndex;

            collectedNodes = LogicCollectNodes.CollectNodes();
        }

        public NodePanel(NodeEditor _nodeEditor)
        {
            nodeEditor = _nodeEditor;

            collectedNodes = LogicCollectNodes.CollectNodes();
        }
    }

}
#endif