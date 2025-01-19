/*
 *	DATABRAIN | Logic
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using System.Collections.Generic;

using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;

namespace Databrain.Logic.Elements
{
    public class NodeCanvasElement : VisualElement
    {
        public List<NodeVisualElement> nodeUIElements = new List<NodeVisualElement>();
        public List<GroupVisualElement> groupUIElements = new List<GroupVisualElement>();
        public List<CommentVisualElement> commentUIElements = new List<CommentVisualElement>();
        public List<NodeVisualElement> selectedNodes = new List<NodeVisualElement>();
        public List<NodeVisualElement> copyNodesClipboard = new List<NodeVisualElement>();

        #if !UNITY_6000_0_OR_NEWER
        public new class UxmlFactory : UxmlFactory<NodeCanvasElement, UxmlTraits>{}
        #endif

        private GraphData graphData;
        private NodeEditor nodeEditor;
        
        public bool hasFocus;

        //[SerializeField]
        //private bool isKeyUpRegistered = false;

        public NodeCanvasElement() { }
        public NodeCanvasElement(GraphData _graph, NodeEditor _nodeEditor)
        {
            graphData = _graph;
            nodeEditor = _nodeEditor;
            
            RegisterCallback<AttachToPanelEvent>(OnEnterPanel);
            RegisterCallback<DetachFromPanelEvent>(OnExitPanel);
            
        }

        void OnEnterPanel(AttachToPanelEvent e)
        {
            panel.visualTree.RegisterCallback<KeyUpEvent>(OnKeyUpShortcut);
        }

        void OnExitPanel(DetachFromPanelEvent e)
        {
            panel.visualTree.UnregisterCallback<KeyUpEvent>(OnKeyUpShortcut);
        }


        void OnKeyUpShortcut(KeyUpEvent evt)
        {
            if (panel.GetCapturingElement(PointerId.mousePointerId) != null)
                return;

            //Debug.Log(evt.character);

            if (evt.keyCode == KeyCode.Delete)
            {
                if (selectedNodes != null && selectedNodes.Count > 0)
                {
                    for (int i = 0; i < selectedNodes.Count; i++)
                    {
                        if (selectedNodes[i].nodeData.nonDeletable)
                        {
                            continue;
                        }
                        nodeEditor.DeleteNode(selectedNodes[i].nodeData);
                       
                        var _node = selectedNodes[i].nodeData;
                        _node.isDeleted = true;
                        graphData.nodes.Remove(_node);

                        AssetDatabase.RemoveObjectFromAsset(_node);

                        // simply hide this node visual element.
                        selectedNodes[i].visible = false;
                        selectedNodes[i].pickingMode = PickingMode.Ignore;
                    }

                    DeselectAll(null);

                    AssetDatabase.Refresh();
                    AssetDatabase.SaveAssets();
                }
            }
            if ((evt.ctrlKey || evt.commandKey) && evt.keyCode == KeyCode.G)
            {
                nodeEditor.CreateGroupObject(selectedNodes);
            }

            if (evt.keyCode == KeyCode.F)
            {
                if (hasFocus)
                {
                    if (selectedNodes.Count > 0)
                    {
                        // Frame selected
                        FrameSelection();
                    }
                }
            }

            if (evt.keyCode == KeyCode.C && evt.actionKey)
            {
                if (hasFocus)
                { 
                    // add selected nodes to copy clipboard
                    copyNodesClipboard = new List<NodeVisualElement>();
                    for (int i = 0; i < selectedNodes.Count; i++)
                    {
                        copyNodesClipboard.Add(selectedNodes[i]);
                    }
                }
            }


            if (evt.keyCode == KeyCode.V && evt.actionKey)
            {
                if (hasFocus)
                {
                    // Paste node
                    nodeEditor.CopyNodesFromSelection();
                }
            }

            //switch (evt.character)
            //{
            //    // Focus
            //    case 'f':
                   
            //        break;

            //    case 'o':

            //        break;

            //    case '[':

            //        break;

            //    case ']':

            //        break;
            //    case ' ':

            //        break;
            //}
        }



        //public void ResetView()
        //{
        //    Vector2[] _points = new Vector2[graphData.nodes.Count * 4];

        //    int _index = 0;
        //    for (var i = 0; i < graphData.nodes.Count; i++)
        //    {
        //        _index = i * 4;

        //        var _pTopLeft = new Vector2(graphData.nodes[i].position.x, graphData.nodes[i].position.y);
        //        var _pTopRight = new Vector2(graphData.nodes[i].position.x + graphData.nodes[i].nodeVisualElement.worldBound.width, graphData.nodes[i].position.y);
        //        var _pBottomLeft = new Vector2(graphData.nodes[i].position.x, graphData.nodes[i].position.y + graphData.nodes[i].nodeVisualElement.worldBound.height);
        //        var _pBottomRight = new Vector2(graphData.nodes[i].position.x + graphData.nodes[i].nodeVisualElement.worldBound.width, graphData.nodes[i].position.y + graphData.nodes[i].nodeVisualElement.worldBound.height);

        //        _points[_index] = _pTopLeft;
        //        _points[_index + 1] = _pTopRight;
        //        _points[_index + 2] = _pBottomLeft;
        //        _points[_index + 3] = _pBottomRight;
        //    }

        //    var _boundRect = GetVector2BoundRect(_points);

         
        //    //var _cPos = nodeEditor.nodeCanvasContainer.transform.position;
        //    var _nPos = new Vector3(Mathf.Abs(_boundRect.position.x), Mathf.Abs(_boundRect.position.y), 0);

        //    var _t = new Translate(_nPos.x, _nPos.y, 0);
        //    nodeEditor.nodeCanvasContainer.style.translate = new StyleTranslate(_t);
            
        //}

        //Rect GetVector2BoundRect(Vector2[] _points)
        //{
        //    var _minX = _points.Min(p => p.x);
        //    var _minY = _points.Min(p => p.y);
        //    var _maxX = _points.Max(p => p.x);
        //    var _maxY = _points.Max(p => p.y);

        //    return new Rect((_minX) - 50, (_minY) - 50, (_maxX - _minX) + 100, (_maxY - _minY) + 100);
        //}


        public void FrameSelection()
        {
            Rect rectToFit = this.layout;
            bool reachedFirstChild = false;

            selectedNodes.ForEach(se =>
            {
                if (!reachedFirstChild)
                {
                    rectToFit =  se.ChangeCoordinatesTo(this, se.layout);
                    reachedFirstChild = true;
                }
                else
                {
                    rectToFit = Utils.LogicHelpers.Encompass(rectToFit, se.ChangeCoordinatesTo(this, se.layout));
                }
            });

            

            var _t = new Translate((nodeEditor.nodeCanvasContainer.transform.position.x - rectToFit.x) + (layout.width * 0.5f) - (rectToFit.width * 0.5f), (nodeEditor.nodeCanvasContainer.transform.position.y - rectToFit.y) + (layout.height * 0.5f) - (rectToFit.height * 0.5f), 0);
            nodeEditor.nodeCanvasContainer.style.translate = new StyleTranslate(_t);

        }


        public void ResetView()
        {
            var _rect = CalculateRectToFitAll();
            var _t = new Translate((nodeEditor.nodeCanvasContainer.transform.position.x - _rect.x) + 50, (nodeEditor.nodeCanvasContainer.transform.position.y - _rect.y) + 70, 0);
            nodeEditor.nodeCanvasContainer.style.translate = new StyleTranslate(_t);
        }


        Rect CalculateRectToFitAll()
        {
            Rect rectToFit = this.layout;
            bool reachedFirstChild = false;
          
            List<VisualElement> _allElements = new List<VisualElement>();
            for (int n = 0; n < nodeUIElements.Count; n++)
            {
                if (!nodeUIElements[n].nodeData.isDeleted)
                {
                    _allElements.Add(nodeUIElements[n]);
                }
            }

            for (int n = 0; n < groupUIElements.Count; n++)
            {              
                _allElements.Add(groupUIElements[n]);            
            }

            _allElements.ForEach(ge =>
            {
                
                if (!reachedFirstChild)
                {
                    rectToFit = ge.ChangeCoordinatesTo(this, ge.layout);
                    reachedFirstChild = true;
                }
                else
                {
                    rectToFit = Utils.LogicHelpers.Encompass(rectToFit, ge.ChangeCoordinatesTo(this, ge.layout));
                }
                
            });


            return rectToFit;
        }



        public void AddToSelection(NodeVisualElement _node)
        {
            selectedNodes.Add(_node);
            _node.SelectNode();
        }

        public void RemoveFromSelection(NodeVisualElement _node)
        {
            for (int i = 0; i < selectedNodes.Count; i++)
            {
                if (selectedNodes[i] == _node)
                {
                    selectedNodes.RemoveAt(i);
                }
            }
        }

        public void DeselectAll(NodeVisualElement _exceptNode)
        {
            selectedNodes = new List<NodeVisualElement>();

            foreach (var _n in nodeUIElements)
            {
                if (_n != _exceptNode)
                {
                    _n.DeselectNode();
                }
            }
        }

        public bool IsSelected(NodeVisualElement _node)
        {
            if (selectedNodes.Contains(_node))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


    }
}
#endif