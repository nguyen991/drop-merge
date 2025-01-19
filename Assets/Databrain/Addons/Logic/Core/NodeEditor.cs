/*
 *	DATABRAIN | Logic
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

using Databrain.UI.Elements;
using Databrain.Helpers;
using Databrain.Logic.Elements;
using Databrain.Logic.Manipulators;
using Databrain.Logic.Attributes;
using Databrain.Attributes;

namespace Databrain.Logic
{
    public class NodeEditor
    {
        public NodeCanvasElement nodeCanvas;
        public VisualElement nodeCanvasContainer;
        public VisualElement root;
        public VisualElement openNewWindowButton;
        public NodeData selectedNode;
        public ManipulatorManager manipulatorManager;

        private MinimapElement minimapElement;
        private GridElement gridElement;
        private GraphData graphData;
        private DataLibrary dataLibrary;       
        private NodeVisualElement selectedNodeElement;       
        private VisualElement sidePanel;
        private ScrollView sidePanelContainer;
        private VisualElement nodeOutputUIContainer;
        private Toolbar sideToolbar;
        private ToolbarToggle inspectorToggle;
        private ToolbarToggle graphEventsToggle;
        // private ToolbarToggle graphParameterToggle;
        private VisualElement lockedGraphView;

        private VisualElement loadingBarContainer;
        private VisualElement loadingBarBorder;
        private VisualElement loadingBarProgress;

        private Texture2D iconOpenEditor;
        private Texture2D iconResetView;
        private Texture2D iconCreateGroup;
        private Texture2D iconMinimap;
        public Texture2D splineTexture2Lines;
        public Texture2D splineTexture3Lines;
        

        public bool keyEventsRegistered = false; 
    
        public NodeEditor(GraphData _graph)
        {
            graphData = _graph;
            dataLibrary = _graph.relatedLibraryObject;
        }   

        public VisualElement GUI(bool _externalWindow)
        {

            root = new VisualElement();
            root.style.flexGrow = 1;
            root.name = "root";

            iconOpenEditor = DatabrainHelpers.LoadIcon("open");
            iconResetView = DatabrainHelpers.LoadIcon("reset");
            iconCreateGroup = DatabrainHelpers.LoadIcon("group");
            iconMinimap = DatabrainHelpers.LoadIcon("minimap");

            splineTexture3Lines = DatabrainHelpers.LoadTexture("spline3Lines.png", "LogicResPath.cs");
            
            // Toolbar
            var _toolbar = new VisualElement();
            _toolbar.name = "toolbar";
            DatabrainHelpers.SetBorder(_toolbar, 1, DatabrainColor.LightGrey.GetColor());
            DatabrainHelpers.SetBorderRadius(_toolbar, 5, 5, 5, 5);
            DatabrainHelpers.SetPadding(_toolbar, 4, 4, 6, 6);
            _toolbar.style.flexDirection = FlexDirection.Row;
            _toolbar.style.height = 40;
            _toolbar.style.paddingLeft = 5;
            _toolbar.style.alignItems = Align.Center;
            _toolbar.style.backgroundColor = DatabrainColor.DarkGrey.GetColor();
            _toolbar.style.position = Position.Absolute;
            _toolbar.style.left = 10;
            _toolbar.style.top = 10;

            var _backgroundToolbarIconsSize = new StyleBackgroundSize();
            _backgroundToolbarIconsSize.value = new BackgroundSize(24, 24);

            openNewWindowButton = ToolbarButton();
            openNewWindowButton.style.backgroundImage = iconOpenEditor;
            openNewWindowButton.style.backgroundSize = _backgroundToolbarIconsSize;
            openNewWindowButton.tooltip = "Open in separate window";
            openNewWindowButton.style.width = 28;
            openNewWindowButton.style.height = 28;

            openNewWindowButton.RegisterCallback<ClickEvent>(click =>
            {
                SetLockedGraphView(true);

                NodeEditorWindow.Open(graphData);
            });

            var _toolbarResetView = ToolbarButton();
            _toolbarResetView.style.backgroundSize = _backgroundToolbarIconsSize;
            _toolbarResetView.style.backgroundImage = iconResetView;
            _toolbarResetView.tooltip = "Reset the node canvas view";
            _toolbarResetView.style.width = 28;
            _toolbarResetView.style.height = 28;
            _toolbarResetView.RegisterCallback<ClickEvent>(click =>
            {
                nodeCanvas.ResetView();
            });


            var _toolbarCreateGroup = ToolbarButton();
            _toolbarCreateGroup.style.backgroundSize = _backgroundToolbarIconsSize;
            _toolbarCreateGroup.style.backgroundImage = iconCreateGroup;
            _toolbarCreateGroup.tooltip = "Create group from selected nodes. Shortcut: CTRL + G";
            _toolbarCreateGroup.style.width = 28;
            _toolbarCreateGroup.style.height = 28;
            _toolbarCreateGroup.RegisterCallback<ClickEvent>(click =>
            {
                CreateGroupObject(nodeCanvas.selectedNodes);
            });

            var _toolbarMinimap = ToolbarButton();
            _toolbarMinimap.style.backgroundImage = iconMinimap;
            _toolbarMinimap.style.backgroundSize = _backgroundToolbarIconsSize;
            _toolbarMinimap.tooltip = "Show / Hide minimap";
            _toolbarMinimap.style.width = 28;
            _toolbarMinimap.style.height = 28;
            _toolbarMinimap.RegisterCallback<ClickEvent>(click =>
            {
                graphData.showMinimap = !graphData.showMinimap;
                ShowMinimap();

                if (graphData.showMinimap)
                {
                    DatabrainHelpers.SetBorder(_toolbarMinimap, 2, Color.white);
                }
                else
                {
                    DatabrainHelpers.SetBorder(_toolbarMinimap, 0, Color.white);
                }
            });

            if (graphData.showMinimap)
            {
                DatabrainHelpers.SetBorder(_toolbarMinimap, 2, Color.white);
            }

            _toolbar.Add(openNewWindowButton);
            _toolbar.Add(_toolbarResetView);
            _toolbar.Add(_toolbarCreateGroup);
            _toolbar.Add(_toolbarMinimap);


            loadingBarContainer = new VisualElement();
            loadingBarContainer.style.width = new Length(100, LengthUnit.Percent);
            loadingBarContainer.style.height = new Length(100, LengthUnit.Percent);
            loadingBarContainer.style.backgroundColor = DatabrainColor.DarkGrey.GetColor();
            loadingBarContainer.style.alignItems = Align.Center;
            loadingBarContainer.style.position = Position.Absolute;

            var _loadingLabel = new Label();
            _loadingLabel.style.marginTop = 100;
            _loadingLabel.text = "Loading";
            _loadingLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

            var _loadingSpace = new VisualElement();
            _loadingSpace.style.height = 150;

            loadingBarBorder = new VisualElement();
            loadingBarBorder.style.width = new Length(20, LengthUnit.Percent);
            loadingBarBorder.style.height = 5;
            DatabrainHelpers.SetBorder(loadingBarBorder, 1, Color.black);

            loadingBarProgress = new VisualElement();
            loadingBarProgress.style.backgroundColor = DatabrainColor.Blue.GetColor();
            loadingBarProgress.style.width = new Length(100, LengthUnit.Percent);
            loadingBarProgress.style.height = 5;

            loadingBarBorder.Add(loadingBarProgress);

            loadingBarContainer.Add(_loadingSpace);
            loadingBarContainer.Add(_loadingLabel);
            loadingBarContainer.Add(loadingBarBorder);



            // Create node canvas
            nodeCanvas = new NodeCanvasElement(graphData, this);
            nodeCanvas.RegisterCallback<ClickEvent>(x =>
            {
                nodeCanvas.hasFocus = true;
            });
            nodeCanvas.name = "nodeCanvas";
            nodeCanvas.style.overflow = Overflow.Hidden;
            nodeCanvas.style.flexGrow = 1;
            nodeCanvas.style.backgroundColor = new Color(35f / 255f, 35f / 255f, 35f / 255f);
         

            nodeCanvas.Add(loadingBarContainer);


            lockedGraphView = new VisualElement();
            lockedGraphView.style.width = new Length(100, LengthUnit.Percent);
            lockedGraphView.style.height = new Length(100, LengthUnit.Percent);
            lockedGraphView.style.backgroundColor = DatabrainColor.DarkGrey.GetColor();
            lockedGraphView.style.alignItems = Align.Center;
            lockedGraphView.style.position = Position.Absolute;
            lockedGraphView.visible = false;

            var _lockGraphLabel = new Label();
            DatabrainHelpers.SetMargin(_lockGraphLabel, 20, 20, 20, 10);
            DatabrainHelpers.SetPadding(_lockGraphLabel, 10, 10, 10, 10);
            DatabrainHelpers.SetBorder(_lockGraphLabel, 1, Color.white);
            _lockGraphLabel.text = "This graph is locked because an external Logic window with this graph is currently open. Close the Logic window to unlock this graph again.";
            _lockGraphLabel.style.whiteSpace = WhiteSpace.Normal;

            var _unlockGraphButton = new Button();
            _unlockGraphButton.text = "Close window and unlock";
            _unlockGraphButton.style.width = 200;
            _unlockGraphButton.style.height = 40;
            DatabrainHelpers.SetMargin(_unlockGraphButton, 10, 10, 10, 10);
            _unlockGraphButton.RegisterCallback<ClickEvent>(click =>
            {
              
                var _graphInstance = EditorPrefs.GetInt("Databrain-Logic_GraphDataID");
                if (graphData.GetInstanceID() != _graphInstance)
                {
                    SetLockedGraphView(false);
                    // Rebuild all changes
                    BuildNodes();
                }
                // External editor is open
                else
                {
                    NodeEditorWindow[] _w = Resources.FindObjectsOfTypeAll<NodeEditorWindow>();
                    if (_w.Length > 0)
                    {
                        _w[0].Close();

                        SetLockedGraphView(false);
                        // Rebuild all changes
                        BuildNodes();
                    }
                }
            });

            lockedGraphView.Add(_lockGraphLabel);
            lockedGraphView.Add(_unlockGraphButton);


            // Create split view Inspector/Canvas
            var _splitView = new SplitView();
            _splitView.name = "splitView";
            _splitView.orientation = TwoPaneSplitViewOrientation.Horizontal;
            _splitView.fixedPaneInitialDimension = 300;
            

            // Add Zoom and Pan manipulator
            //nodeCanvas.AddManipulator(new ZoomPanManipulator());
            var _rectSelect = new RectangleSelect();
            _rectSelect.name = "RectangleSelect";
            nodeCanvas.Add(_rectSelect);

            // Add Node panel manipulator
            nodeCanvas.AddManipulator(new NodePanelManipulator(this));

            gridElement = new GridElement(nodeCanvas);
            gridElement.name = "Grid";
            nodeCanvas.Add(gridElement);


            // Create node canvas container (the parent container for the node canvas)
            nodeCanvasContainer = new VisualElement();
            nodeCanvasContainer.RegisterCallback<ClickEvent>(x =>
            {
                nodeCanvas.hasFocus = true;
            });

            nodeCanvasContainer.pickingMode = PickingMode.Ignore;
            nodeCanvasContainer.name = "nodeContainer";
            nodeCanvasContainer.style.flexGrow = 1;
            nodeCanvasContainer.style.position = Position.Absolute;
            nodeCanvasContainer.usageHints = UsageHints.GroupTransform;

            manipulatorManager = new ManipulatorManager();
            var _zoomPanManipulator = new ZoomPanManipulator(nodeCanvasContainer, nodeCanvas, minimapElement);	
            manipulatorManager.AddManipulator(nodeCanvasContainer, _zoomPanManipulator);
            // nodeCanvasContainer.AddManipulator(new ZoomPanManipulator(nodeCanvasContainer, nodeCanvas, minimapElement));

         
            nodeCanvas.Add(nodeCanvasContainer);
            nodeCanvas.Add(_toolbar);
            _rectSelect.PlaceInFront(nodeCanvasContainer);
            
            loadingBarContainer.PlaceInFront(nodeCanvasContainer);


            // Create side panel          
            sidePanel = new VisualElement();
            sidePanel.style.flexDirection = FlexDirection.Row;
            sidePanel.RegisterCallback<ClickEvent>(x =>
            {
                nodeCanvas.hasFocus = false;
            });

            var _leftPanel = new VisualElement();
            _leftPanel.style.flexGrow = 1;
            var _rightPanel = new VisualElement();
            _rightPanel.style.width = 20;
            _rightPanel.style.flexShrink = 0;

            sidePanel.Add(_leftPanel);
            sidePanel.Add(_rightPanel);

            var _inspectorOnOffButton = new Button();
            DatabrainHelpers.SetBorderRadius(_inspectorOnOffButton, 0, 0, 0, 0);
            DatabrainHelpers.SetMargin(_inspectorOnOffButton, 0, 0, 0, 0);
            _inspectorOnOffButton.style.flexDirection = FlexDirection.Column;
            _inspectorOnOffButton.style.flexGrow = 1;
            _inspectorOnOffButton.style.width = 20;
            _inspectorOnOffButton.style.minWidth = 20;
            _inspectorOnOffButton.text = "<";
            _inspectorOnOffButton.style.backgroundColor = DatabrainColor.Grey.GetColor();
            _inspectorOnOffButton.style.color = DatabrainColor.LightGrey.GetColor();

            _inspectorOnOffButton.RegisterCallback<ClickEvent>(evt =>
            {
                if (sidePanel.resolvedStyle.width > 20)
                {
                    _leftPanel.style.display = DisplayStyle.None;
                    var _dragLine = _splitView.Q<VisualElement>("unity-dragline-anchor");
                    _dragLine.visible = false;
                    sidePanel.style.width = 20;
                    _inspectorOnOffButton.text = ">";
                }
                else
                {
                    _leftPanel.style.display = DisplayStyle.Flex;
                    var _dragLine = _splitView.Q<VisualElement>("unity-dragline-anchor");
                    _dragLine.visible = true;
                    _dragLine.style.left = _splitView.fixedPaneInitialDimension;
                    sidePanel.style.width = _splitView.fixedPaneInitialDimension;
                    _inspectorOnOffButton.text = "<";
                }
            });

            _rightPanel.Add(_inspectorOnOffButton);




            sidePanelContainer = new ScrollView();
           

            // var _options = new List<string>();
            // _options.Add("Inspector");
            // _options.Add("Graph Events");
            // _options.Add("Graph Parameters");
            //_options.Add("Exp. Refs.");

            // var _actions = new List<System.Action>();

            System.Action action1 = () =>
            {
                inspectorToggle.value = true;               
                graphEventsToggle.value = false;
                // graphParameterToggle.value = false;
                PopulateNodeInspector(selectedNodeElement, selectedNode);
            };

            System.Action action2 = () =>
            {
                inspectorToggle.value = false;               
                graphEventsToggle.value = true;
                // graphParameterToggle.value = false;
                ShowGraphEvents();
            };



            //  System.Action action3 = () =>
            // {
            //     inspectorToggle.value = false;               
            //     graphEventsToggle.value = false;
            //     graphParameterToggle.value = true;
            //     ShowGraphParameters();
            // };


            // _actions.Add(action1);
            // _actions.Add(action2);

            sideToolbar = new Toolbar();
            inspectorToggle = new ToolbarToggle();
            inspectorToggle.text = "Inspector";
            inspectorToggle.value = true;
            inspectorToggle.RegisterCallback<ClickEvent>(click =>
            {
                action1.Invoke();
            });

            graphEventsToggle = new ToolbarToggle();
            graphEventsToggle.text = "Graph Events";
            graphEventsToggle.RegisterCallback<ClickEvent>(click =>
            {
                action2.Invoke();
            });

            // graphParameterToggle = new ToolbarToggle();
            // graphParameterToggle.text = "Graph Parameters";
            // graphParameterToggle.RegisterCallback<ClickEvent>(click =>
            // {
            //     action3.Invoke();
            // });
          
            sideToolbar.Add(inspectorToggle);
            sideToolbar.Add(graphEventsToggle);
            // sideToolbar.Add(graphParameterToggle);

            _leftPanel.Add(sideToolbar);
            _leftPanel.Add(sidePanelContainer);


            nodeCanvas.AddManipulator(new RectangleSelectorManipulator(0, sidePanel, _rectSelect));

            // var _customTabSplitView = new SplitView();
            // _customTabSplitView.fixedPaneInitialDimension = 100;
            // _customTabSplitView.name = "CustomSplitView";      
            // _customTabSplitView.orientation = TwoPaneSplitViewOrientation.Horizontal;
            // _customTabSplitView.fixedPaneInitialDimension = 300;
            
            // var _customTab = new VisualElement();
            // _customTab.style.width = 100;
            // _customTab.style.backgroundColor = Color.blue;

            // _customTabSplitView.contentContainer.Add(_customTab);
            // _customTabSplitView.contentContainer.Add();

            _splitView.contentContainer.Add(sidePanel);
            _splitView.contentContainer.Add(nodeCanvas);


            root.Add(_splitView);
            root.Add(lockedGraphView);

            // Databrain.Helpers.EditorCoroutines.Execute(BuildNodes());
            BuildNodes();


            NodeData _masterDialogueNode = null;
            bool _customTab = false;
            Attribute _customInspectorAttr = null;
            for (int n = 0; n < graphData.nodes.Count; n ++)
            {   
                if (graphData.nodes[n] == null)
                    continue;
                    
                var _nIndex = n;

                _customInspectorAttr = graphData.nodes[n].GetType().GetCustomAttribute(typeof(NodeCustomInspectorTab), true);
                if (_customInspectorAttr != null)
                {
                    _customTab = true;
                    _masterDialogueNode = graphData.nodes[_nIndex];

                    var _toggle = new ToolbarToggle();
                    
                    _toggle.text = (_customInspectorAttr as NodeCustomInspectorTab).tabName;
                    _toggle.RegisterCallback<ClickEvent>(click =>
                    {
                        if (_nIndex < graphData.nodes.Count)
                        {
                            inspectorToggle.value = false;               
                            graphEventsToggle.value = false;
                            _customInspectorAttr = _customInspectorAttr = graphData.nodes[_nIndex].GetType().GetCustomAttribute(typeof(NodeCustomInspectorTab), true);
                            ShowCustomTab(graphData.nodes[_nIndex], (_customInspectorAttr as NodeCustomInspectorTab).defaultTabWidth);
                        }
                        else
                        {
                            for (int n2 = 0; n2 < graphData.nodes.Count; n2 ++)
                            {
                                _nIndex = n2;
                                inspectorToggle.value = false;               
                                graphEventsToggle.value = false;
                                _customInspectorAttr = _customInspectorAttr = graphData.nodes[_nIndex].GetType().GetCustomAttribute(typeof(NodeCustomInspectorTab), true);
                                ShowCustomTab(graphData.nodes[_nIndex], (_customInspectorAttr as NodeCustomInspectorTab).defaultTabWidth);
                            }
                        }
                    });

                    sideToolbar.Add(_toggle);
                }
            }


            if (EditorPrefs.GetInt("Databrain-Logic_GraphDataID") != -1 && !_externalWindow)
            {
                // External editor is open
                var _graphInstance = EditorPrefs.GetInt("Databrain-Logic_GraphDataID");
                if (graphData.GetInstanceID() == _graphInstance)
                {
                    SetLockedGraphView(true);
                }
            }

            if (_customTab && _masterDialogueNode != null)
            {
                // root.schedule.Execute(() => ShowCustomTab(_masterDialogueNode, (_customInspectorAttr as NodeCustomInspectorTab).defaultTabWidth)).ExecuteLater(100);
                // ShowCustomTab(_masterDialogueNode, (_customInspectorAttr as NodeCustomInspectorTab).defaultTabWidth);
                ShowCustomTab(_masterDialogueNode, (_customInspectorAttr as NodeCustomInspectorTab) != null ? (_customInspectorAttr as NodeCustomInspectorTab).defaultTabWidth : 200);
            }

            return root;
        }

        private VisualElement ToolbarButton()
        {
            var _button = new Button();
            DatabrainHelpers.SetMargin(_button, 2, 2, 2, 2);
            DatabrainHelpers.SetPadding(_button, 0, 0, 0, 0);
            DatabrainHelpers.SetBorder(_button, 0);
            DatabrainHelpers.SetBorderRadius(_button, 0, 0, 0, 0);

            return _button;
        }

        public void SetLockedGraphView(bool _visible)
        {
            lockedGraphView.visible = _visible;
        }

        private void ShowMinimap()
        {
            // Create minimap
            if (minimapElement == null)
            {
                minimapElement = new MinimapElement(nodeCanvas, nodeCanvasContainer);
                nodeCanvas.Add(minimapElement);

                minimapElement.PlaceInFront(nodeCanvasContainer);
            }

            minimapElement.visible = graphData.showMinimap;
        }

       
        // Create node visual element from existing nodes data
        async void BuildNodes()
        // private IEnumerator BuildNodes()
        {
        
            nodeCanvasContainer.Clear();
            nodeCanvas.nodeUIElements = new List<NodeVisualElement>();

            loadingBarContainer.visible = true;
            loadingBarProgress.style.width = 0;

          
            for (int i = 0; i < graphData.nodes.Count; i++)
            {
                if (graphData.nodes[i] == null)
                    continue;

                var _nodeElement = new NodeVisualElement(graphData.nodes[i] as NodeData, graphData.nodes[i].position, graphData, nodeCanvasContainer, nodeCanvas, this);

                nodeCanvasContainer.Add(_nodeElement);
                nodeCanvas.nodeUIElements.Add(_nodeElement as NodeVisualElement);

                graphData.nodes[i].nodeVisualElement = _nodeElement;

                if (graphData.nodes.Count > 10)
                {
                    if (i % 5 == 0)
                    {
                        // yield return null;
                        // await Task.Yield();
                    }
                }

                loadingBarProgress.style.width = new Length((i * 100) / (graphData.nodes.Count + graphData.groups.Count), LengthUnit.Percent);

            }

            // Build conenctions
            for (int i = 0; i < nodeCanvas.nodeUIElements.Count; i++)
            {
                nodeCanvas.nodeUIElements[i].BuildConnections();
            }

            if (graphData.nodes.Count == 0)
            {
                // Create start node for the first time
                CreateNewNode(typeof(OnStart), new Vector2(100, 100));

                // Create additional default nodes based on LogicGraphDefaultNodes attribute
                var _attribute = graphData.GetType().GetCustomAttribute<LogicGraphDefaultNodes>();
                if (_attribute != null)
                {
                    for (int i = 0; i < _attribute.nodes.Length; i++)
                    {
                        CreateNewNode(_attribute.nodes[i], new Vector2(100, 180 + (i * 80)));
                    }
                }
            }

            // yield return null;
            await Task.Yield();

            // Build groups
            BuildGroups();
            // Databrain.Helpers.EditorCoroutines.Execute(BuildGroups());
            // // Build comments
            BuildComments();
            // Databrain.Helpers.EditorCoroutines.Execute(BuildComments());
        }

        async void BuildGroups()
        {
            await Task.Delay(100);

            for (int g = 0; g < graphData.groups.Count; g++)
            {
                if (graphData.groups[g] == null)
                    continue;

                if (float.IsNaN(graphData.groups[g].size.x) || float.IsNaN(graphData.groups[g].size.y))
                    continue;

                var _groupElement = new GroupVisualElement(graphData.groups[g], graphData, nodeCanvasContainer, nodeCanvas, this);
                _groupElement.transform.position = graphData.groups[g].position;
                _groupElement.style.width = graphData.groups[g].size.x;
                _groupElement.style.height = graphData.groups[g].size.y;


                nodeCanvasContainer.Add(_groupElement);
                nodeCanvas.groupUIElements.Add(_groupElement as GroupVisualElement);
                _groupElement.PlaceBehind(nodeCanvas.nodeUIElements[0]);

                await Task.Yield();

                loadingBarProgress.style.width = new Length(((graphData.nodes.Count + g) * 100) / (graphData.nodes.Count + graphData.groups.Count), LengthUnit.Percent);
            }

            loadingBarProgress.style.width = new Length(20, LengthUnit.Percent);

            nodeCanvas.ResetView();

            await Task.Delay(100);

            loadingBarContainer.visible = false;

            ShowMinimap();
        }

        async void BuildComments()
        {
            await Task.Delay(200);

            for (int g = 0; g < graphData.comments.Count; g++)
            {
                if (graphData.comments[g] == null)
                    continue;

                var _commentElement = new CommentVisualElement(graphData.comments[g].position, graphData.comments[g], this, nodeCanvas);
                _commentElement.transform.position = graphData.comments[g].position;

                nodeCanvasContainer.Add(_commentElement);
                nodeCanvas.commentUIElements.Add(_commentElement as CommentVisualElement);
            }
        }

        /// Copy from selected nodes
        public void CopyNodesFromSelection()
        {
            List<NodeVisualElement> _newNodes = new List<NodeVisualElement>();

            for (int i = 0; i < nodeCanvas.copyNodesClipboard.Count; i ++)
            {
                var _from = nodeCanvas.copyNodesClipboard[i].nodeData;
                var _copy = ScriptableObject.Instantiate(_from);

                (_copy as NodeData).guid = Guid.NewGuid().ToString() + "_" + _from.guid;
                (_copy as NodeData).position = new Vector2(_from.position.x + 10, _from.position.y + 10);
                _copy.hideFlags = HideFlags.HideInHierarchy;

                graphData.nodes.Add(_copy as NodeData);

                var _nodeElement = new NodeVisualElement(_copy as NodeData, (_copy as NodeData).position, graphData, nodeCanvasContainer, nodeCanvas, this);
                nodeCanvasContainer.Add(_nodeElement);
                nodeCanvas.nodeUIElements.Add(_nodeElement as NodeVisualElement);
                (_copy as NodeData).nodeVisualElement = _nodeElement;

                _newNodes.Add(_nodeElement);

                AssetDatabase.AddObjectToAsset(_copy, dataLibrary);
                EditorUtility.SetDirty(_copy);
            }



            // check which newly created nodes are connected
            // and re-connect them to the new nodes
            for (int f = 0; f < nodeCanvas.copyNodesClipboard.Count; f++)
            {
                for (int e = 0; e < nodeCanvas.copyNodesClipboard[f].nodeData.connectedNodesOut.Count; e ++)
                {
                    var _connectionSuccess = false;
                    for (int c = 0; c < nodeCanvas.copyNodesClipboard.Count; c++)
                    {
                        if (nodeCanvas.copyNodesClipboard[f].nodeData.connectedNodesOut[e] == nodeCanvas.copyNodesClipboard[c].nodeData && 
                        nodeCanvas.copyNodesClipboard[f].nodeData != nodeCanvas.copyNodesClipboard[c].nodeData)
                        {
                            var _fromGuid = nodeCanvas.copyNodesClipboard[f].nodeData.guid;
                            var _toGuid = nodeCanvas.copyNodesClipboard[c].nodeData.guid;
                            var _outputIndex = e;

                            NodeData _newNodeFrom = null;
                            NodeData _newNodeTo = null;

                            for (int n = 0; n < _newNodes.Count; n ++)
                            {
                                if (_newNodes[n].nodeData.guid.Contains(_fromGuid))
                                {
                                    _newNodeFrom = _newNodes[n].nodeData;
                                }
                            }

                            for (int n = 0; n < _newNodes.Count; n ++)
                            {
                                if (_newNodes[n].nodeData.guid.Contains(_toGuid))
                                {
                                    _newNodeTo = _newNodes[n].nodeData;
                                }
                            }

                            if (_newNodeFrom != null && _newNodeTo != null)
                            {
                                _newNodeFrom.ConnectToNode(_outputIndex, _newNodeTo);
                                _newNodeFrom.nodeVisualElement.BuildConnections();
                                _connectionSuccess = true;
                            }
                            
                        }
                    }

                    if (!_connectionSuccess)
                    {
                        // Update connections to existing nodes (outside of selected nodes)
                        var _fromGuid = nodeCanvas.copyNodesClipboard[f].nodeData.guid;
                        for (int n = 0; n < _newNodes.Count; n ++)
                        {
                            if (_newNodes[n].nodeData.guid.Contains(_fromGuid))
                            {
                                _newNodes[n].BuildConnections();
                            }
                        }
                    }
                }
            }

            
            nodeCanvas.DeselectAll(null);
            for (int i = 0; i < _newNodes.Count; i++)
            {
                nodeCanvas.AddToSelection(_newNodes[i]);
            }

            AssetImporter assetImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(dataLibrary));
            assetImporter.SaveAndReimport();
            //AssetDatabase.SaveAssets();
            //AssetDatabase.Refresh();
        }


        // Creates a new node data + node visual element
        public NodeData CreateNewNode(Type _type, Vector2 _position)
        {
            var _node = ScriptableObject.CreateInstance(_type);

            // Assign node values
            (_node as NodeData).guid = Guid.NewGuid().ToString();
            (_node as NodeData).derivedClassName = _type.Name;
            (_node as NodeData).relatedLibraryObject = dataLibrary;
            (_node as NodeData).graphData = graphData;

            var _nodePosition = nodeCanvasContainer.WorldToLocal(_position);
            (_node as NodeData).position = _nodePosition;

            _node.hideFlags = HideFlags.HideInHierarchy;

            graphData.nodes.Add(_node as NodeData);

            var _nodeElement = new NodeVisualElement(_node as NodeData, _nodePosition, graphData, nodeCanvasContainer, nodeCanvas,this);
            
            nodeCanvasContainer.Add(_nodeElement);
            nodeCanvas.nodeUIElements.Add(_nodeElement as NodeVisualElement);


            (_node as NodeData).nodeVisualElement = _nodeElement;

            AssetDatabase.AddObjectToAsset(_node, dataLibrary);
            EditorUtility.SetDirty(_node);

            AssetImporter assetImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(dataLibrary));
            assetImporter.SaveAndReimport();
            //AssetDatabase.SaveAssets();
            //AssetDatabase.Refresh();

            return (_node as NodeData);
        }


        public GroupData CreateGroupObject(List<NodeVisualElement> _selectedNodes)
        {
            if (_selectedNodes.Count == 0)
                return null;

            var _group = ScriptableObject.CreateInstance(typeof(GroupData)) as GroupData;

            _group.guid = Guid.NewGuid().ToString();
            _group.derivedClassName = typeof(GroupData).Name;
            _group.relatedLibraryObject = dataLibrary;
            _group.graphData = graphData;
            _group.title = "Group";
            _group.hideFlags = HideFlags.HideInHierarchy;


            for (int i = 0; i < _selectedNodes.Count; i++)
            {
                _group.assignedNodes.Add(_selectedNodes[i].nodeData);
            }

            _group.assignedNodes = _group.assignedNodes.GroupBy(x => x.guid).Select(y => y.First()).ToList();
          


            graphData.groups.Add(_group);

            var _groupElement = new GroupVisualElement(_group, graphData, nodeCanvasContainer, nodeCanvas, this);
            nodeCanvas.groupUIElements.Add(_groupElement as GroupVisualElement);
            nodeCanvasContainer.Add(_groupElement);
            
            if (nodeCanvas.nodeUIElements.Count > 0)
            {
                try
                {
                _groupElement.PlaceBehind(nodeCanvas.nodeUIElements[0]);
                }
                catch{}
            }

            AssetDatabase.AddObjectToAsset(_group, dataLibrary);
            EditorUtility.SetDirty(_group);

            AssetImporter assetImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(dataLibrary));
            assetImporter.SaveAndReimport();
            //AssetDatabase.SaveAssets();
            //AssetDatabase.Refresh();


            return _group;
        }

        public CommentData CreateComment(Vector2 _position)
        {
            var _comment = ScriptableObject.CreateInstance(typeof(CommentData)) as CommentData;

            _comment.guid = Guid.NewGuid().ToString();
            _comment.relatedLibraryObject = dataLibrary;
            _comment.graphData = graphData;
            _comment.title = "Comment";
            _comment.comment = "New Comment";
            _comment.hideFlags = HideFlags.HideInHierarchy;

            var _nodePosition = nodeCanvasContainer.WorldToLocal(_position);
            (_comment as CommentData).position = _nodePosition;

            graphData.comments.Add(_comment);

            var _commentElement = new CommentVisualElement(_nodePosition, _comment, this, nodeCanvas);
            nodeCanvas.commentUIElements.Add(_commentElement as CommentVisualElement);
            nodeCanvasContainer.Add(_commentElement);

            AssetDatabase.AddObjectToAsset(_comment, dataLibrary);
            EditorUtility.SetDirty(_comment);
           
            AssetImporter assetImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(dataLibrary));
            assetImporter.SaveAndReimport();
            //AssetDatabase.SaveAssets();
            //AssetDatabase.Refresh();

            return _comment;
        }

        public void DeleteGroup(GroupData _group, GroupVisualElement _groupVisualElement)
        {
            graphData.groups.Remove(_group);

            _groupVisualElement.visible = false;


            AssetDatabase.RemoveObjectFromAsset(_group);
            AssetImporter assetImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(dataLibrary));
            assetImporter.SaveAndReimport();

            //AssetDatabase.Refresh();
            //AssetDatabase.SaveAssets();
        }

        public void DeleteNode(NodeData _node)
        {          
            _node.nodeVisualElement.CleanupAllEdges();
            
            // Clear nodes clipboard just to make sure
            nodeCanvas.copyNodesClipboard = new List<NodeVisualElement>();

            for (int n = 0; n < nodeCanvas.nodeUIElements.Count; n ++)
            {
                for (int o = 0; o < nodeCanvas.nodeUIElements[n].nodeData.outputs.Count; o ++)
                {
                    nodeCanvas.nodeUIElements[n].CleanupEdgesTo(_node);
                }
            }

            if (nodeCanvasContainer.Contains(_node.nodeVisualElement))
            {
                nodeCanvasContainer.Remove(_node.nodeVisualElement);
            }

            // Clear node inspector
            sidePanelContainer.Clear();
        }

        public void DeleteComment(CommentData _comment, CommentVisualElement _commentVisualElement)
        {
            graphData.comments.Remove(_comment);

            _commentVisualElement.visible = false;


            AssetDatabase.RemoveObjectFromAsset(_comment);
            AssetImporter assetImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(dataLibrary));
            assetImporter.SaveAndReimport();
        }


        #region NodeInspector
        public void PopulateNodeInspector(NodeVisualElement _nodeVisualElement, NodeData _nodeData)
        {
            //if (selectedNode == _nodeData)
            //    return;


            sidePanelContainer.Clear();

            inspectorToggle.value = true;
            graphEventsToggle.value = false;

            if (_nodeData == null)
                return;

            selectedNode = _nodeData;
            selectedNodeElement = _nodeVisualElement;

            var _selectedNode = _nodeData;

            var _container = new VisualElement();
            _container.style.marginRight = 10;
            _container.style.marginLeft = 10;
            _container.style.marginBottom = 10;
            _container.style.marginTop = 10;

            var _nodeIcon = new VisualElement();
            _nodeIcon.style.width = 25;
            _nodeIcon.style.height = 25;
            _nodeIcon.style.marginRight = 5;

            var _nodeIconAttribute = _selectedNode.GetType().GetCustomAttribute<NodeIcon>();
            var _nodeColorAttribute = _selectedNode.GetType().GetCustomAttribute<NodeColor>();
            if (_nodeIconAttribute != null)
            {
                if (string.IsNullOrEmpty(_nodeIconAttribute.rootFile))
                {
                    _nodeIcon.style.backgroundImage = DatabrainHelpers.LoadIcon(_nodeIconAttribute.icon);
                }
                else
                {
                    _nodeIcon.style.backgroundImage = DatabrainHelpers.LoadTexture(_nodeIconAttribute.icon, _nodeIconAttribute.rootFile);
                }
            }
            else
            {
                _nodeIcon.style.backgroundImage = DatabrainHelpers.LoadIcon("nodes");
            }

            if (_nodeColorAttribute != null)
            {
                Color _color = Color.white;
                if (!string.IsNullOrEmpty(_nodeColorAttribute.color))
                {
                    ColorUtility.TryParseHtmlString(_nodeColorAttribute.color, out _color);
                }

                if (_nodeColorAttribute.databrainColor != DatabrainColor.Clear)
                {
                    _color = _nodeColorAttribute.databrainColor.GetColor();
                }

                _nodeIcon.style.unityBackgroundImageTintColor = _color;
            }

            var _titleContainer = new VisualElement();
            _titleContainer.style.flexDirection = FlexDirection.Row;
            _titleContainer.style.marginBottom = 4;

            var _title = new Label();
            _title.style.fontSize = 14;
            _title.style.unityFontStyleAndWeight = FontStyle.Bold;
            _title.text = string.IsNullOrEmpty(_selectedNode.userTitle) ? _selectedNode.title : _selectedNode.title + " : " + _selectedNode.userTitle;
            _title.style.unityTextAlign = TextAnchor.MiddleLeft;

            var _addUserTitleButton = new Button();
            //_addUserTitleButton.text = "edit";
            _addUserTitleButton.style.backgroundImage = DatabrainHelpers.LoadIcon("pen2");
            var _bgs = new BackgroundSize();
            _bgs.sizeType = BackgroundSizeType.Contain;
            _bgs.x = new  Length(16, LengthUnit.Pixel);
            _bgs.y = new Length(16, LengthUnit.Pixel);

            _addUserTitleButton.style.backgroundSize = _bgs;
            _addUserTitleButton.style.width = 24;
            _addUserTitleButton.style.height = 24;
            _addUserTitleButton.tooltip = "Edit node title";
            _addUserTitleButton.RegisterCallback<ClickEvent>(click =>
            {
                var _txtf = _titleContainer.Q<TextField>();
                if (_txtf == null)
                {
                    var _textField = new TextField();
                    _textField.style.flexGrow = 1;
                    _textField.style.height = 18;
                    _textField.value = _selectedNode.userTitle;
                    _textField.RegisterValueChangedCallback(x =>
                    {
                        _selectedNode.userTitle = x.newValue;
                    });

                    _titleContainer.Insert(2, _textField);

                    _addUserTitleButton.text = "ok";
                    _addUserTitleButton.style.backgroundImage = null;
                }
                else
                {
                    if (_txtf.visible)
                    {
                        _txtf.visible = false;
                        _title.text = string.IsNullOrEmpty(_selectedNode.userTitle) ? _selectedNode.title : _selectedNode.title + " : " + _selectedNode.userTitle;
                        var _bgs = new BackgroundSize();
                        _bgs.sizeType = BackgroundSizeType.Contain;
                        _bgs.x = new Length(16, LengthUnit.Pixel);
                        _bgs.y = new Length(16, LengthUnit.Pixel);
                        _addUserTitleButton.style.backgroundSize = _bgs;
                        _addUserTitleButton.style.backgroundImage = DatabrainHelpers.LoadIcon("pen2");
                        _addUserTitleButton.text = "";

                        _nodeVisualElement.Q<Label>("title").text = _title.text;
                    }
                    else
                    {
                        _txtf.visible = true;
                        _txtf.value = _selectedNode.userTitle;
                        _addUserTitleButton.text = "ok";
                        _addUserTitleButton.style.backgroundImage = null;
                    }
                }
            });

            _titleContainer.Add(_nodeIcon);
            _titleContainer.Add(_title);
            _titleContainer.Add(DatabrainHelpers.HorizontalSpace());
            _titleContainer.Add(_addUserTitleButton);
            
            _container.Add(_titleContainer);


            // Description 
            var _descriptionAttr = _nodeData.GetType().GetCustomAttribute<NodeDescription>();
            if (_descriptionAttr != null)
            {
                var _descriptionContainer = new VisualElement();
                DatabrainHelpers.SetPadding(_descriptionContainer, 5, 5, 5, 5);
                DatabrainHelpers.SetBorder(_descriptionContainer, 1);
                _descriptionContainer.style.marginBottom = 10;

                var _description = new Label();
                _description.text = _descriptionAttr.description;
                _description.style.whiteSpace = WhiteSpace.Normal;

                _descriptionContainer.Add(_description);

                _container.Add(_descriptionContainer);
            }



            var _nodeEditor = Editor.CreateEditor((_selectedNode));
            _nodeEditor.serializedObject.Update();

            var _useOdinInspector = _selectedNode.GetType().GetCustomAttribute(typeof(UseOdinInspectorAttribute)) as UseOdinInspectorAttribute;
            var _inspectorIMGUI = new VisualElement();

            // DRAW NODE INSPECTOR USING ODIN
            if (_useOdinInspector != null)
            {
                #if ODIN_INSPECTOR || ODIN_INSPECTOR_3 || ODIN_INSPECTOR_3_1
                Sirenix.OdinInspector.Editor.OdinEditor odinEditor = Sirenix.OdinInspector.Editor.OdinEditor.CreateEditor(_selectedNode) as Sirenix.OdinInspector.Editor.OdinEditor;

                _inspectorIMGUI = new IMGUIContainer(() =>
                {
                    DrawDefaultInspectorUIElements.DrawInspectorWithOdin(odinEditor, null);

                });

                #else

                _inspectorIMGUI = new IMGUIContainer(() =>
                {

                    GUILayout.Label("Odin Inspector not installed");

                });

                #endif

                _container.Add(_inspectorIMGUI);
            }
            // DRAW DEFAULT NODE INSPECTOR USING UITOOLKIT
            else
            {
                var sp = _nodeEditor.serializedObject.GetIterator();
                sp.NextVisible(true);
                while (sp.NextVisible(false))
                {
                    if (sp.propertyPath == "m_Script")
                    {

                    }
                    else
                    {
                        System.Type t = sp.serializedObject.targetObject.GetType();
                        FieldInfo f = null;
                        f = t.GetField(sp.propertyPath);
                        if (f != null)
                        {
                            var _hideAttribute = f.GetCustomAttribute(typeof(NodeHideVariable), true);
                            if (_hideAttribute == null)
                            {

                                var pf = new PropertyField(sp);
                        
                                pf.Unbind();
                                pf.BindProperty(sp);
                                pf.RegisterCallback<AttachToPanelEvent>(x => { });

                                if (typeof(DataObject).IsAssignableFrom(f.FieldType))
                                {
                                    pf.RegisterValueChangeCallback(evt =>
                                    {
                                        selectedNode.nodeVisualElement.BuildValues(true);
                                    });
                                }

                            pf.schedule.Execute(() => 
                            {
                                pf.style.flexGrow = 0;

                                var _label = pf.Query<Label>().First();
                                if (_label != null)
                                {
                                    _label.style.flexShrink = 1;
                                    _label.style.flexBasis = 0;
                                }
                            }).ExecuteLater(200);

                                var _enableByAttribute = f.GetCustomAttribute(typeof(EnableByAttribute), true) as EnableByAttribute;
                                if (_enableByAttribute != null)
                                {
                                    var _enableField = _nodeEditor.serializedObject.FindProperty(_enableByAttribute.fieldName);
                                    pf.SetEnabled(_enableField.boolValue);
                                    pf.schedule.Execute(action => { pf.SetEnabled(_enableField.boolValue); }).Every(200);
                                    
                                }

                                _container.Add(pf);
                            }
                        }
                    }
                }
            }
            
            
            sidePanelContainer.Add(_container);

            // If allow add outputs attribute is available show add output button
            var _allowOutputsAttribute = _nodeData.GetType().GetCustomAttribute<NodeAddOutputsUI>();
            if (_allowOutputsAttribute != null)
            {
                BuildOutputUI(_selectedNode, _nodeVisualElement);  
                sidePanelContainer.Add(nodeOutputUIContainer);
            }

            var _customNodeGUI = _nodeData.CustomGUI();
            if (_customNodeGUI != null)
            {
                sidePanelContainer.Add(_customNodeGUI); 
            }
           
            sidePanelContainer.Bind(_nodeEditor.serializedObject);
        }


       void BuildOutputUI(NodeData _selectedNode, NodeVisualElement _nodeVisualElement)
       {
            // nodeAddOutputUI
            if (nodeOutputUIContainer == null)
                {
                    nodeOutputUIContainer = new VisualElement();
                }
                else
                {
                    nodeOutputUIContainer.Clear();
                }
                var _outputsLabel = new Label();
                DatabrainHelpers.SetTitle(_outputsLabel, "Outputs");
                _outputsLabel.style.marginBottom = 5;
                _outputsLabel.style.marginTop = 10;

                nodeOutputUIContainer.Add(_outputsLabel);

                for (int i = 0; i < _selectedNode.outputs.Count; i++)
                {
                    var _outputIndex = i;
                    var _outputItem = new VisualElement();
                    _outputItem.style.flexDirection = FlexDirection.Row;
                    _outputItem.style.backgroundColor = DatabrainColor.Grey.GetColor();
                    DatabrainHelpers.SetBorder(_outputItem, 1, DatabrainColor.LightGrey.GetColor());
                    _outputItem.style.marginBottom = 2;

                    var _outputField = new TextField();
                    _outputField.style.flexGrow = 1;
                    _outputField.value = _selectedNode.outputs[i];
                    _outputField.RegisterValueChangedCallback(changed =>
                    {
                        _selectedNode.outputs[_outputIndex] = changed.newValue;

                        _selectedNode.nodeVisualElement.UpdateOutputs();
                    });

                    var _moveUp = new Button();
                    _moveUp.text = "<";
                    _moveUp.RegisterCallback<ClickEvent>(click => 
                    {
                        var _out1 = _selectedNode.outputs[_outputIndex];
                        var _nodeOut = _selectedNode.connectedNodesOut[_outputIndex];

                        _selectedNode.outputs.RemoveAt(_outputIndex);
                         _selectedNode.connectedNodesOut.RemoveAt(_outputIndex);
                        if (_outputIndex > 0)
                        {
                            _selectedNode.outputs.Insert(_outputIndex - 1, _out1);
                            _selectedNode.connectedNodesOut.Insert(_outputIndex-1, _nodeOut);
                        }

                        BuildOutputUI(_selectedNode, _nodeVisualElement);
                        // Update connections
                        _nodeVisualElement.BuildConnections();
                    });
                    
                    var _moveDown = new Button();
                    _moveDown.text = ">";
                    _moveDown.RegisterCallback<ClickEvent>(click => 
                    {
                        var _out1 = _selectedNode.outputs[_outputIndex];
                        var _nodeOut = _selectedNode.connectedNodesOut[_outputIndex];

                        _selectedNode.outputs.RemoveAt(_outputIndex);
                         _selectedNode.connectedNodesOut.RemoveAt(_outputIndex);
                        if (_outputIndex + 1 <= _selectedNode.outputs.Count)
                        {
                            _selectedNode.outputs.Insert(_outputIndex + 1, _out1);
                            _selectedNode.connectedNodesOut.Insert(_outputIndex + 1, _nodeOut);
                        }

                        BuildOutputUI(_selectedNode, _nodeVisualElement);

                        // Update connections
                        _nodeVisualElement.BuildConnections();
                    });

                    var _removeOutputButton = new Button();
                    _removeOutputButton.text = "x";
                    _removeOutputButton.style.width = 20;
                    _removeOutputButton.RegisterCallback<ClickEvent>(click =>
                    {
                        _nodeVisualElement.RemoveOutput(_outputIndex);
                        selectedNode = null;
                        PopulateNodeInspector(selectedNodeElement, _selectedNode);
                    });

                    _outputItem.Add(_outputField);
                    if (i > 0)
                    {
                        _outputItem.Add(_moveUp);
                    }
                    if (i < _selectedNode.outputs.Count - 1)
                    {
                        _outputItem.Add(_moveDown);
                    }
                    _outputItem.Add(_removeOutputButton);

                    nodeOutputUIContainer.Add(_outputItem);
                }



                //var _outputName = new TextField();

                var _addOutputButton = new Button();
                _addOutputButton.text = "Add new output";
                _addOutputButton.RegisterCallback<ClickEvent>((click) =>
                {
                    var _newOutputName = "";
                    if (_selectedNode.outputs.Count-1 >= 0)
                    {
                        _newOutputName = _selectedNode.outputs[_selectedNode.outputs.Count-1];
                    }
                    _nodeVisualElement.AddOutput(_newOutputName); // _outputName.value);
                    selectedNode = null;
                    
                    PopulateNodeInspector(selectedNodeElement, _selectedNode);
                });

                //_container.Add(_outputName);
                nodeOutputUIContainer.Add(_addOutputButton);   
       }


        #endregion

        #region GraphEvents
        private void ShowGraphEvents()
        {
            sidePanelContainer.Clear();

            var _graphEventsTitle = new Label();
            DatabrainHelpers.SetTitle(_graphEventsTitle, "Graph Events");
            DatabrainHelpers.SetMargin(_graphEventsTitle, 10, 10, 10, 10);

            //var _container = new VisualElement();
            var _editor = Editor.CreateEditor(graphData);

            var _graphEventsList = _editor.serializedObject.FindProperty("graphEvents");
           
            var _propField = new PropertyField();
            _propField.BindProperty(_graphEventsList);

            sidePanelContainer.Add(_graphEventsTitle);
            sidePanelContainer.Add(_propField);

        }
        #endregion

        public void ShowCustomTab(NodeData _node, int _width)
        {
            sidePanelContainer.Clear();

            var _ui = _node.CustomGUI();
            sidePanelContainer.Add(_ui);

            var _toggles = sideToolbar.Query<ToolbarToggle>().ToList();
            foreach ( var _toggle in _toggles)
            {
                _toggle.value = false; 
            }
            
            if (_width > 0)
            {
                var _dragLine = root.Q<SplitView>("splitView").Q<VisualElement>("unity-dragline-anchor");
                _dragLine.visible = true;
                _dragLine.style.left = _width;
                sidePanel.style.width = _width;
            }
        }
    }
}
#endif