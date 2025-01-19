/*
 *	DATABRAIN | Logic
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

using Databrain.Helpers;
using Databrain.Logic.Attributes;
using Databrain.Logic.Manipulators;
using Databrain.Attributes;
using Databrain.UI;

namespace Databrain.Logic.Elements
{
    public class NodeVisualElement : VisualElement
    {

        public GraphData graphData;
        public NodeData nodeData;
        public NodeEditor nodeEditor;
        public Vector2 targetStartPosition;
      
        private VisualElement nodeCanvasContainer;
        private NodeCanvasElement nodeCanvas;
        private Foldout valuesFoldout;
        private VisualElement valuesContent;
        private VisualElement inputSlotBG;

        // Store for each output the edge manipulator including the spline element
        public List<DragEdgeManipulator> edgeManipulators = new List<DragEdgeManipulator>();
        private bool highlightOn;
        private bool forceHighlightOff;
        private bool isSelected;

        private Dictionary<string, PropertyField> enableByProperties = new Dictionary<string, PropertyField>();

        #if !UNITY_6000_0_OR_NEWER
        public new class UxmlFactory : UxmlFactory<NodeVisualElement, UxmlTraits>{}
        #endif

        public NodeVisualElement() { usageHints = UsageHints.DynamicTransform; }
        public NodeVisualElement(NodeData _nodeData, Vector2 _position, GraphData _graphData, VisualElement _nodeCanvasContainer, NodeCanvasElement _nodeCanvas, NodeEditor _nodeEditor)
        {
            usageHints = UsageHints.DynamicTransform;
            
            this.name = _nodeData.guid;
            nodeData = _nodeData;
            nodeData.nodeVisualElement = this;
            graphData = _graphData;
            nodeEditor = _nodeEditor;
            nodeCanvasContainer = _nodeCanvasContainer;
            nodeCanvas = _nodeCanvas;
            enableByProperties = new Dictionary<string, PropertyField>();

            style.position = Position.Absolute;

            // Load visual asset of the node
            var _visualAsset = DatabrainHelpers.GetVisualAsset("LogicAssetsPath.cs", "nodeVisualAsset.uxml");
            _visualAsset.CloneTree(this);

            inputSlotBG = this.Q<VisualElement>("inputSlotBG");

            var _dragArea = this.Q<VisualElement>("dragArea");

            NodeMoveManipulator manipulator = new NodeMoveManipulator(_dragArea, this, _nodeEditor.nodeCanvas, _nodeEditor.nodeCanvasContainer, _nodeEditor);
            _nodeEditor.manipulatorManager.AddManipulator(_dragArea, manipulator);

            #region GetNodeAttributes
            //Get attributes
            // Node Size
            var _nodeSizeAttribute = _nodeData.GetType().GetCustomAttribute<NodeSize>();
            if (_nodeSizeAttribute != null)
            {
                style.minWidth = _nodeSizeAttribute.size.x;
                style.minHeight = new StyleLength(StyleKeyword.Auto); // _nodeSizeAttribute.size.y;
            }
            else
            {
                // default size
                style.minWidth = 150;
                style.minHeight = new StyleLength(StyleKeyword.Auto);
            }


            var _hideNodeOnCanvasAttr = nodeData.GetType().GetCustomAttribute(typeof (HideNodeOnCanvas));
            if (_hideNodeOnCanvasAttr != null)
            {
                style.width = 0f;
                style.height = 0f;
                style.minWidth = 0f;
                style.minHeight = 0f;
                style.visibility = Visibility.Hidden;
                transform.position = new Vector3(-1000, -1000, 0);
                nodeData.nonDeletable = true;
            }

            var _nodeGradientBorderAttr = nodeData.GetType().GetCustomAttribute(typeof(NodeGradientBorder));
            if (_nodeGradientBorderAttr != null)
            {
                var _nodeGradientBorder = this.Q<VisualElement>("nodeGradientBorder");
                _nodeGradientBorder.SetPadding(2, 2, 2, 2);
                _nodeGradientBorder.style.backgroundImage = DatabrainHelpers.LoadTexture((_nodeGradientBorderAttr as NodeGradientBorder).gradientSpriteName, (_nodeGradientBorderAttr as NodeGradientBorder).rootFile);

                var _node = this.Q<VisualElement>("node");
                _node.SetBorder(0);
            }


            // Outputs
            List<string> _outputs = new List<string>();
            
            // Node data already contains outputs
            if (_nodeData.connectedNodesOut.Count > 0)
            {
                for (int o = 0; o < _nodeData.outputs.Count; o++)
                {
                    _outputs.Add(_nodeData.outputs[o]);
                }

                var _nodeOutputsAttribute = _nodeData.GetType().GetCustomAttribute<NodeOutputs>();

                // Compare attributes output list with existing one
                if (_nodeOutputsAttribute != null)
                {
                  
                    // Different sizes
                    if (_nodeOutputsAttribute.outputs.Length > _outputs.Count)
                    {
                        for (int i = _outputs.Count; i < _nodeOutputsAttribute.outputs.Length; i++)
                        {
                            _outputs.Add(_nodeOutputsAttribute.outputs[i]);
                        }
                    }
                    else
                    {
                        // var _customNodeOutputUI = _nodeData.GetType().GetCustomAttribute<NodeAddOutputsUI>();
                        // // Change outputs only if node doesn't have the NodeAddOutputUI as this would interfere with the defined outputs in the NodeOutputs attribute
                        // if (_customNodeOutputUI == null)
                        // {
                        //     for (int i = _outputs.Count - 1; i >= _nodeOutputsAttribute.outputs.Length; i--)
                        //     {
                        //         _outputs.RemoveAt(i);
                        //     }
                        // }
                    }

                    // same size then check for changed names
                    if (_nodeOutputsAttribute.outputs.Length == _outputs.Count)
                    {
                        for (int i = 0; i < _outputs.Count; i ++)
                        {
                            _outputs[i] = _nodeOutputsAttribute.outputs[i];
                        }
                    }
                }
            }
            else
            {
                var _nodeOutputsAttribute = _nodeData.GetType().GetCustomAttribute<NodeOutputs>();

                if (_nodeOutputsAttribute != null)
                {
                    if (_nodeOutputsAttribute.outputs != null)
                    {
                        _outputs = _nodeOutputsAttribute.outputs.ToList();
                    }
                }
            }


            // Title
            var _nodeTitleAttribute = _nodeData.GetType().GetCustomAttribute<NodeTitle>();
            var _title = nodeData.derivedClassName;
            if (_nodeTitleAttribute != null)
            {
                _title = _nodeTitleAttribute.title;
            }

            this.Q<Label>("title").text = string.IsNullOrEmpty(nodeData.userTitle) ? _title : _title + " : " + nodeData.userTitle;
            nodeData.title = _title;

            // Color
            var _nodeColorAttribute = _nodeData.GetType().GetCustomAttribute<NodeColor>();
            Color _color = Color.white;
            if (_nodeColorAttribute != null)
            {
                if (!string.IsNullOrEmpty(_nodeColorAttribute.color))
                {
                    ColorUtility.TryParseHtmlString(_nodeColorAttribute.color, out _color);
                }

                if (_nodeColorAttribute.databrainColor != DatabrainColor.Clear)
                {
                    _color = _nodeColorAttribute.databrainColor.GetColor();
                }
            }
           
            SetColor(_color, _nodeColorAttribute != null ? _nodeColorAttribute.borderColors : new string[]{"","","",""});
                

            // Is node connectable
            var _isNotConnectableAttribute = _nodeData.GetType().GetCustomAttribute<NodeNotConnectable>();
            nodeData.isConnectable = _isNotConnectableAttribute == null ? true : false;

            if (!nodeData.isConnectable)
            {
                var _inputSlot = this.Q<VisualElement>("left");
                _inputSlot.style.visibility = Visibility.Hidden;
                _inputSlot.style.width = 0f;

                var _rightSlot = this.Q<VisualElement>("right");
                _rightSlot.style.width = Length.Percent(100);
            }


            // Node Icon
            var _nodeIconAttribute = _nodeData.GetType().GetCustomAttribute<NodeIcon>();
            if (_nodeIconAttribute != null)
            {
                
                var _icon = this.Q<VisualElement>("icon");
                if (string.IsNullOrEmpty(_nodeIconAttribute.rootFile))
                {
                    _icon.style.backgroundImage = DatabrainHelpers.LoadIcon(_nodeIconAttribute.icon);
                }
                else
                {
                    _icon.style.backgroundImage = DatabrainHelpers.LoadTexture(_nodeIconAttribute.icon, _nodeIconAttribute.rootFile);
                }
            }
            else
            {
                
                var _icon = this.Q<VisualElement>("icon");
                _icon.style.backgroundImage = DatabrainHelpers.LoadIcon("nodes");
            }
            #endregion

            #region BuildOutputs
            // Build Outputs
            var _outputAsset = DatabrainHelpers.GetVisualAsset("LogicAssetsPath.cs", "nodeOutputAsset.uxml");

            nodeData.outputs = new List<string>();

            for (int i = 0; i < _outputs.Count; i++)
            {

                _outputAsset.CloneTree(this.Q<VisualElement>("right"));
                var _outputElement = this.Q<VisualElement>("outputSlot");
                _outputElement.name = "output_" + _outputs[i];

                var _outputLabel = _outputElement.Q<Label>("label");
                _outputLabel.text = _outputs[i];

                var _outputSlot = _outputElement.Q<VisualElement>("slot");

                int _index = i;
                DragEdgeManipulator _dragEdgeManipulator = new DragEdgeManipulator(_nodeData, this, _index, nodeEditor.nodeCanvasContainer, _outputSlot, nodeEditor);

                edgeManipulators.Add(_dragEdgeManipulator);

                //nodeData.connectedNodesOut.Add(null); _outputs[i]);

                nodeData.outputs.Add(_outputs[i]);

                if (i >= nodeData.connectedNodesOut.Count)
                {
                    nodeData.connectedNodesOut.Add(null);
                }
            }

            #endregion

            #region Values
                
            valuesFoldout = this.Q<Foldout>("foldout");
            valuesFoldout.UnregisterCallback<ChangeEvent<bool>>(FoldoutCallback);


            BuildValues(false);


            valuesFoldout.RegisterCallback<ChangeEvent<bool>>(FoldoutCallback);

            valuesContent = this.Q<VisualElement>("values");
             // Add custom node gui
            var _customNodeGUI = nodeData.CustomNodeGUI();
            valuesContent.Add(_customNodeGUI);


            if (valuesFoldout.childCount == 0)
            {
                valuesFoldout.style.visibility = Visibility.Hidden;
                valuesFoldout.style.height = 0;
            }


            var _editor = Editor.CreateEditor((nodeData));
            this.Unbind();
            this.Bind(_editor.serializedObject);
            _editor.serializedObject.ApplyModifiedProperties();


            #endregion


            this.transform.position = _position;


            nodeData.EditorInitalize();
            nodeData.EditorInitalize(_nodeEditor, this);
        }

        void FoldoutCallback(ChangeEvent<bool> result)
        {
            if (result.newValue != result.previousValue && result.newValue)
            {
                var _editor = Editor.CreateEditor((nodeData));
                _editor.serializedObject.ApplyModifiedProperties();
            }
        }


        public void BuildValues(bool _fromNodeInspector)
        {
            valuesFoldout.Clear();

            var _editor = Editor.CreateEditor((nodeData));
            var sp = _editor.serializedObject.GetIterator();

            valuesFoldout.Unbind();
            valuesFoldout.BindProperty(_editor.serializedObject.FindProperty("showValues"));

            var _useOdinInspector = nodeData.GetType().GetCustomAttribute(typeof(UseOdinInspectorAttribute)) as UseOdinInspectorAttribute;
            var _inspectorIMGUI = new VisualElement();


            if (_useOdinInspector != null)
            {
                #if ODIN_INSPECTOR || ODIN_INSPECTOR_3 || ODIN_INSPECTOR_3_1
                Sirenix.OdinInspector.Editor.OdinEditor odinEditor = Sirenix.OdinInspector.Editor.OdinEditor.CreateEditor(nodeData) as Sirenix.OdinInspector.Editor.OdinEditor;

                _inspectorIMGUI = new IMGUIContainer(() =>
                {
                    DrawDefaultInspectorUIElements.DrawInspectorWithOdin(odinEditor, null);
                });

                
                _inspectorIMGUI.style.maxWidth = 400;
                _inspectorIMGUI.style.flexShrink = 1;
                #else

                _inspectorIMGUI = new IMGUIContainer(() =>
                {

                    GUILayout.Label("Odin Inspector not installed");

                });

                #endif

                valuesFoldout.Add(_inspectorIMGUI);
            }
            else
            {
                sp.NextVisible(true);
                while (sp.NextVisible(false))
                {
                    System.Type t = sp.serializedObject.targetObject.GetType();
                    
                    FieldInfo f = null;
                    f = t.GetField(sp.propertyPath);
                    if (f != null)
                    {
                        var _hideAttribute = f.GetCustomAttribute(typeof(NodeHideVariable), true);
                        if (_hideAttribute == null)
                        {
                            var _property = new PropertyField(sp);
                        
                            _property.Unbind();
                            _property.BindProperty(sp);

                            enableByProperties.TryAdd(sp.propertyPath, _property);

                            _property.RegisterValueChangeCallback(x =>
                            {  
                                if (x.changedProperty.propertyType == SerializedPropertyType.ObjectReference)
                                {
                                    if (nodeEditor.selectedNode == nodeData && !_fromNodeInspector)
                                    {
                                        nodeEditor.selectedNode = null;
                                        nodeEditor.PopulateNodeInspector(this, nodeData);
                                    }
                                }

                            });


                            var _enableByAttribute = f.GetCustomAttribute(typeof(EnableByAttribute), true) as EnableByAttribute;
                            if (_enableByAttribute != null)
                            {
                                var _enableField = sp.serializedObject.FindProperty(_enableByAttribute.fieldName);

                                PropertyField _enableByPropertyField = null;
                                if (_enableField != null)
                                {
                                    enableByProperties.TryGetValue(_enableField.propertyPath, out _enableByPropertyField);
                                }
                                
                                if (_enableByPropertyField != null)
                                {
                                    _enableByPropertyField.RegisterCallback<ChangeEvent<bool>>(x =>
                                    {
                                        _property.SetEnabled(x.newValue);
                                    });
                                }

                                _property.SetEnabled(_enableField.boolValue);
                                
                            }


                            valuesFoldout.Add(_property);
                        }
                    }
                }
            }

            _fromNodeInspector = false;

            //this.Bind(_editor.serializedObject);
            _editor.serializedObject.ApplyModifiedProperties();

            // if (valuesFoldout.childCount == 0)
            // {
            //     var _values = this.Q<VisualElement>("values");
            //     _values.style.visibility = Visibility.Hidden;
            //     _values.style.height = 0;
            // }
        }
        
        public void UpdateTitle()
        {
            var _nodeTitleAttribute = nodeData.GetType().GetCustomAttribute<NodeTitle>();
            var _title = nodeData.derivedClassName;
            if (_nodeTitleAttribute != null)
            {
                _title = _nodeTitleAttribute.title;
            }

            this.Q<Label>("title").text = string.IsNullOrEmpty(nodeData.userTitle) ? _title : _title + " : " + nodeData.userTitle;
        }

        public void SetOutputLabel(string _title, int _outputIndex)
        {
            var _outputContainer = this.Q<VisualElement>("right");
            var _label = new Label();
            _label.text = _title;

            _outputContainer.hierarchy.Insert(_outputIndex, _label);
        }

        public void SetColor(Color _color, string [] _borderColors)
        {
            var _node = this.Q<VisualElement>("node");
            _node.style.borderTopColor = _color;
            _node.style.borderLeftColor = _color;
            _node.style.borderRightColor = _color;
            _node.style.borderBottomColor = _color;

            // Override for specific border color
            if (_borderColors != null)
            {
                for (int i = 0; i < _borderColors.Length; i ++)
                {
                    if (i == 0 && !string.IsNullOrEmpty(_borderColors[i]))
                    {
                        Color _leftColor = _color;
                        ColorUtility.TryParseHtmlString(_borderColors[i], out _leftColor);
                        _node.style.borderLeftColor = _leftColor;
                        _node.style.borderLeftWidth = 6;
                    }
                    if (i == 1 && !string.IsNullOrEmpty(_borderColors[i]))
                    {
                        Color _rightColor = _color;
                        ColorUtility.TryParseHtmlString(_borderColors[i], out _rightColor);
                        _node.style.borderRightColor = _rightColor;
                        _node.style.borderRightWidth = 6;
                    }
                    if (i == 2 && !string.IsNullOrEmpty(_borderColors[i]))
                    {
                        Color _topColor = _color;
                        ColorUtility.TryParseHtmlString(_borderColors[i], out _topColor);
                        _node.style.borderTopColor = _topColor;
                        _node.style.borderTopWidth = 6;
                    }
                      if (i == 3 && !string.IsNullOrEmpty(_borderColors[i]))
                    {
                        Color _bottomColor = _color;
                        ColorUtility.TryParseHtmlString(_borderColors[i], out _bottomColor);
                        _node.style.borderBottomColor = _bottomColor;
                        _node.style.borderBottomWidth = 6;
                    }
                }
            }

            //DatabrainHelpers.SetBorder(_node, 0);
            //_node.style.unityBackgroundImageTintColor = _color;

            var _header = this.Q<VisualElement>("header");
            _header.style.unityBackgroundImageTintColor = new Color(_color.r, _color.g, _color.b, 100f / 255f);

            var _icon = this.Q<VisualElement>("icon");    
            _icon.style.unityBackgroundImageTintColor = _color;

            nodeData.color = _color;
            nodeData.borderColors = _borderColors;
        }

        public async void SetHighlightOn()
        {
            if (isSelected)
                return;

            var _col = Color.yellow;

            var _nodeBorder = this.Q<VisualElement>("nodeBorder");

            //_nodeBorder.EnableInClassList("nodeBorder", true);
            DatabrainHelpers.SetBorder(_nodeBorder, 2, _col);
            highlightOn = true;

            await Task.Delay(500);

            highlightOn = false;

            if (forceHighlightOff)
            {             
                forceHighlightOff = false;
                SetHighlightOff();
            }
        }

        public void SetHighlightOff()
        {
            if (isSelected)
                return;


            if (highlightOn)
            {
                forceHighlightOff = true;
                return;
            }

            var _nodeBorder = this.Q<VisualElement>("nodeBorder");

            //_nodeBorder.EnableInClassList("nodeBorder", true);
            DatabrainHelpers.SetBorder(_nodeBorder, 2, new Color(1f, 1f, 1f, 0f));
        }

        // Called in the node editor after rebuilding all nodes
        public void BuildConnections()
        {
            if (nodeData.connectedNodesOut == null)
                return;


            // search for node visual element by guid name
            for (int i = 0; i < nodeData.connectedNodesOut.Count; i++)
            {

                if (nodeData.connectedNodesOut[i] != null && !nodeData.connectedNodesOut[i].isDeleted)
                {
                    
                    var _connectedNode = nodeCanvasContainer.Q<NodeVisualElement>(nodeData.connectedNodesOut[i].guid);
                    if (_connectedNode != null)
                    {
                        if (i < edgeManipulators.Count)
                        { 
                            // found node, connect to it visually with the spline
                        
                            edgeManipulators[i].spline.SetVisible(true);
                            edgeManipulators[i].spline.splineEndType = SplineElement.SplineEndType.node;
                            edgeManipulators[i].spline.endNode = _connectedNode;
                            edgeManipulators[i].slot.style.backgroundColor = new Color(233f / 255f, 233f / 255f, 233f / 255f, 190f / 255f);
                            (edgeManipulators[i].spline.endNode as NodeVisualElement).ConnectInput();
                        }
                    }
                    else
                    {
                        edgeManipulators[i].spline.SetVisible(false);
                        edgeManipulators[i].slot.style.backgroundColor = Color.black;
                        nodeData.connectedNodesOut[i] = null;
                    }
                }
                else
                {
                    nodeData.connectedNodesOut[i] = null;
                    if (i <edgeManipulators.Count)
                    {
                        edgeManipulators[i].spline.SetVisible(false);
                        edgeManipulators[i].slot.style.backgroundColor = Color.black;
                    }
                }
                
            }

            
            var _rightSlots = this.Q<VisualElement>("right");
            var _outputs = _rightSlots.Children();
            var _index = 0;
            // Update labels
            foreach(var _child in _outputs)
            {
                var _outputLabel = _child.Q<Label>("label");
                if (_outputLabel != null && _index < nodeData.outputs.Count)
                {   
                    _outputLabel.text = nodeData.outputs[_index];
                }

                _index ++;
            }
        }

        public void DisconnectOutput(int _outputIndex)
        {
            nodeData.connectedNodesOut[_outputIndex] = null;        
        }

        public void ConnectInput()
        {
            inputSlotBG.style.backgroundColor = new Color(233f / 255f, 233f / 255f, 233f / 255f, 190f / 255f);
        }

        public void DisconnectInput()
        {
            bool _hasConnection = false;
            for (int n = 0; n < graphData.nodes.Count; n++)
            {
                if (graphData.nodes[n] == null)
                    continue;
                    
                for (var i = 0; i < graphData.nodes[n].connectedNodesOut.Count; i++)
                {
                    if (graphData.nodes[n].connectedNodesOut[i] != null && graphData.nodes[n] != this.nodeData)
                    {
                        if (graphData.nodes[n].connectedNodesOut[i].guid == this.nodeData.guid)
                        {
                            _hasConnection = true;
                        }
                    }
                }
            }

            if (!_hasConnection)
            {
                inputSlotBG.style.backgroundColor = Color.black;
            }
        }

        public void HighlightInputOn()
        {
            inputSlotBG.style.backgroundColor = new Color(233f / 255f, 233f / 255f, 233f / 255f, 190f / 255f);
        }

        public void HighlightInputOff()
        {
            DisconnectInput();
        }

        public void SelectNode()
        {
            if (nodeData.isDeleted)
                return;

            isSelected = true;

            var _nodeBorder = this.Q<VisualElement>("nodeBorder");
            _nodeBorder.style.borderBottomColor = Color.white;
            _nodeBorder.style.borderTopColor = Color.white;
            _nodeBorder.style.borderRightColor = Color.white;
            _nodeBorder.style.borderLeftColor = Color.white;
            BringToFront();
     
            if (pickingMode != PickingMode.Ignore)
            {
                nodeEditor.PopulateNodeInspector(this, nodeData);
            }

            nodeData.SelectNode();
        }

        public void DeselectNode()
        {
            if (nodeData.isDeleted)
                return;

            isSelected = false;

            var _nodeBorder = this.Q<VisualElement>("nodeBorder");
            _nodeBorder.style.borderBottomColor = new Color(255f / 255f, 255f / 255f, 255f / 255f, 0f / 255f);
            _nodeBorder.style.borderTopColor = new Color(255f / 255f, 255f / 255f, 255f / 255f, 0f / 255f);
            _nodeBorder.style.borderRightColor = new Color(255f / 255f, 255f / 255f, 255f / 255f, 0f / 255f);
            _nodeBorder.style.borderLeftColor = new Color(255f / 255f, 255f / 255f, 255f / 255f, 0f / 255f);

            nodeCanvas.RemoveFromSelection(this);

            nodeData.DeselectNode();
        }

        public void UpdateOutputs()
        {
            for (int i = 0; i < nodeData.outputs.Count; i++)
            {
                var _output = this.Q<VisualElement>("right").Children().ElementAt(i);
                var _outputLabel = _output.Q<Label>("label");
                _outputLabel.text = nodeData.outputs[i];
            }
        }

        public void AddOutput(string _outputName)
        {
            nodeData.connectedNodesOut.Add(null);
            nodeData.outputs.Add(_outputName);

            var _outputAsset = DatabrainHelpers.GetVisualAsset("LogicAssetsPath.cs", "nodeOutputAsset.uxml");
            _outputAsset.CloneTree(this.Q<VisualElement>("right"));
            var _outputElement = this.Q<VisualElement>("outputSlot");
            _outputElement.name = "output_" + _outputName;

            var _outputLabel = _outputElement.Q<Label>("label");
            _outputLabel.text = _outputName;

            var _outputSlot = _outputElement.Q<VisualElement>("slot");

            int _index = nodeData.connectedNodesOut.Count-1;
            DragEdgeManipulator _dragEdgeManipulator = new DragEdgeManipulator(nodeData, this, _index, nodeEditor.nodeCanvasContainer, _outputSlot, nodeEditor);

            edgeManipulators.Add(_dragEdgeManipulator);

            // var _o = new SerializedObject(nodeData);
            // _o.ApplyModifiedProperties();

            EditorUtility.SetDirty(nodeData);
        }

        public void RemoveOutput(int _outputIndex)
        {
            var _output = this.Q<VisualElement>("right").Children().ElementAt(_outputIndex);
            this.Q<VisualElement>("right").Remove(_output);

            nodeData.outputs.RemoveAt(_outputIndex);
            nodeData.connectedNodesOut.RemoveAt(_outputIndex);

            edgeManipulators[_outputIndex].DisconnectSpline();
            edgeManipulators.RemoveAt(_outputIndex);
        }

        public void RemoveOutput(string _outputName)
        {
            for (int i = 0; i < nodeData.outputs.Count; i++)
            {
                if (string.Equals(nodeData.outputs[i], _outputName))
                {
                    var _outputElement = this.Q<VisualElement>("right").Q<VisualElement>("output_" + _outputName);
                    try
                    {
                        this.Q<VisualElement>("right").Remove(_outputElement);
                    }
                    catch
                    {    
                    }

                    nodeData.outputs.RemoveAt(i);
                    nodeData.connectedNodesOut.RemoveAt(i);
                    edgeManipulators.RemoveAt(i);
                }   
            }

            EditorUtility.SetDirty(nodeData);
        }

        public void ClearOutputs()
        {
            nodeData.outputs = new List<string>();
            nodeData.connectedNodesOut = new List<NodeData>();

            for (int i = 0; i < edgeManipulators.Count; i++)
            {
                
                // edgeManipulators[i].spline.SetVisible(false); 
                edgeManipulators.Clear();
            }

            edgeManipulators = new List<DragEdgeManipulator>();

            this.Q<VisualElement>("right").Clear();
        }

        public void CleanupAllEdges()
        {
            if (edgeManipulators == null)
                return;

            for (int n = 0; n < nodeData.connectedNodesOut.Count; n++)
            {
                if (nodeData.connectedNodesOut[n] != null)
                {
                    var _n = (nodeData.connectedNodesOut[n].nodeVisualElement as NodeVisualElement);
                    nodeData.connectedNodesOut[n] = null;
                    if (_n != null)
                    {
                        _n.DisconnectInput();
                    }
                }
            }

            for (int i = 0; i < edgeManipulators.Count; i++)
            {
                try
                {
                    nodeCanvasContainer.Remove(edgeManipulators[i].spline);
                }
                catch { }
            }

           
        }

        // clean up edges to the deleted node (_toNode)
        public void CleanupEdgesTo(NodeData _toNode)
        {
            if (edgeManipulators == null)
                return;

            for (int i = 0; i < edgeManipulators.Count; i++)
            {
                try
                {
                if (edgeManipulators[i].spline.endNode == _toNode.nodeVisualElement)
                {
                    edgeManipulators[i].slot.style.backgroundColor = Color.black;
                    edgeManipulators[i].spline.endNode = null;
                    edgeManipulators[i].spline.SetVisible(false);                 
                }
                }catch{}
            }
        }
    }
}
#endif