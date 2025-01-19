/*
 *	DATABRAIN | Logic
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using Databrain.Attributes;
using Databrain.Helpers;

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Databrain.Logic.Elements
{
    public class MinimapElement : ImmediateModeElement
    {
        private int minimapSizeX = 200;
        private int minimapSizeY = 100;
        private NodeCanvasElement nodeCanvas;
        private VisualElement nodeCanvasContainer;

        private static Vector3[] s_CachedRect = new Vector3[4];
        private Color m_PlacematBorderColor = new Color(0.23f, 0.23f, 0.23f);
        private Matrix4x4 m_WorldTransformInverseCache = Matrix4x4.identity;

        public MinimapElement(NodeCanvasElement _canvas, VisualElement _nodeCanvasContainer)
        {
            nodeCanvas = _canvas;
            nodeCanvasContainer = _nodeCanvasContainer;

            style.width = minimapSizeX;
            style.height = minimapSizeY;
            style.position = Position.Absolute;

            RegisterCallback<MouseDownEvent>(OnMouseDown);

            DatabrainHelpers.SetBorder(this, 1, DatabrainColor.LightGrey.GetColor());
            style.backgroundColor = new Color(0.25f, 0.25f, 0.25f, 0.55f);

            this.schedule.Execute(() => CreateMinimap()).ExecuteLater(200);
        }


        private void CreateMinimap()
        {
            transform.position = new Vector3(parent.layout.width - minimapSizeX, parent.layout.height - minimapSizeY);

            DrawNodes();
        }

        protected override void ImmediateRepaint()
        {
            transform.position = new Vector3(parent.layout.width - minimapSizeX, parent.layout.height - minimapSizeY);
            DrawNodes();

            DrawViewport();
        }


        void DrawNodes()
        {
            var _nodeBounds = CalculateRectToFitAll(nodeCanvasContainer);

            for (int n = 0; n < nodeCanvas.nodeUIElements.Count; n++)
            {
                if (nodeCanvas.nodeUIElements[n].nodeData.isDeleted)
                    continue;

                var _originalNodeRect = new Rect(nodeCanvas.nodeUIElements[n].transform.position.x, nodeCanvas.nodeUIElements[n].transform.position.y, nodeCanvas.nodeUIElements[n].layout.width, nodeCanvas.nodeUIElements[n].layout.height);
                var _nodeMini = TransformRectSpace(_originalNodeRect, _nodeBounds, this.contentRect);


                _nodeMini.width = Mathf.Clamp(_nodeMini.width, 10, 30);
                _nodeMini.height = Mathf.Clamp(_nodeMini.height, 5, 20);

                s_CachedRect[0].Set(_nodeMini.xMin, _nodeMini.yMin, 0.0f);
                s_CachedRect[1].Set(_nodeMini.xMax, _nodeMini.yMin, 0.0f);
                s_CachedRect[2].Set(_nodeMini.xMax, _nodeMini.yMax, 0.0f);
                s_CachedRect[3].Set(_nodeMini.xMin, _nodeMini.yMax, 0.0f);
                //var _fillColor = Random.ColorHSV();

                DrawSolidRectangleWithOutline(ref s_CachedRect, nodeCanvas.nodeUIElements[n].nodeData.color, m_PlacematBorderColor);

                DrawSplines(nodeCanvas.nodeUIElements[n].nodeData, _nodeBounds, _nodeMini);
            }
        }

        void DrawViewport()
        {
            var _nodeBounds = CalculateRectToFitAll(nodeCanvasContainer);
            _nodeBounds.position += new Vector2(nodeCanvasContainer.transform.position.x, nodeCanvasContainer.transform.position.y);

            var _focusRect = TransformRectSpace(new Rect(0, 0, (nodeCanvas.layout.width + 20) / 1, nodeCanvas.layout.height / 1), _nodeBounds, this.contentRect);
            //var _focusRect = TransformRectSpace(new Rect(nodeCanvas.layout.x, nodeCanvas.layout.y, nodeCanvas.layout.width / zoomFactor, nodeCanvas.layout.height / zoomFactor), _nodeBounds, this.contentRect);

            Color currentColor = Handles.color;
            Handles.color = Color.white;

            // Draw viewport outline
            Vector3[] points = new Vector3[5];
            points[0] = new Vector3(_focusRect.x, _focusRect.y, 0.0f);
            points[1] = new Vector3(_focusRect.x + _focusRect.width, _focusRect.y, 0.0f);
            points[2] = new Vector3(_focusRect.x + _focusRect.width, _focusRect.y + _focusRect.height, 0.0f);
            points[3] = new Vector3(_focusRect.x, _focusRect.y + _focusRect.height, 0.0f);
            points[4] = new Vector3(_focusRect.x, _focusRect.y, 0.0f);
            Handles.DrawPolyLine(points);

            Handles.color = currentColor;
        }


        void DrawSplines(NodeData _nodeData, Rect _nodeBounds, Rect _miniNodeRect)
        {
            for (int e = 0; e < _nodeData.connectedNodesOut.Count; e++)
            {
                if (_nodeData.connectedNodesOut[e] != null)
                {
                    if (_nodeData.connectedNodesOut[e].isDeleted)
                        continue;

                    var _startRect = _miniNodeRect;
                    //_startRect = new Rect(_startRect.x, _startRect.y, _startRect.width, _startRect.height);

                    var _nodeOutputRect = new Rect(_nodeData.connectedNodesOut[e].nodeVisualElement.transform.position.x, _nodeData.connectedNodesOut[e].nodeVisualElement.transform.position.y, _nodeData.connectedNodesOut[e].nodeVisualElement.layout.width, _nodeData.connectedNodesOut[e].nodeVisualElement.layout.height);
                    
                    var _endRect = TransformRectSpace(_nodeOutputRect, _nodeBounds, this.contentRect);
                    //var _endRect = _miniNodeOutputRect;

                    _endRect.width = Mathf.Clamp(_endRect.width, 10, 30);
                    _endRect.height = Mathf.Clamp(_endRect.height, 5, 20);

                    var _startPos = new Vector3(_startRect.x + _startRect.width, _startRect.y + (_startRect.height / 2), 0);
                    var _endPos = new Vector3(_endRect.x, _endRect.y + (_endRect.height / 2), 0);
                    try
                    {
                        Handles.DrawBezier(_startPos, _endPos, _startPos + Vector3.right * 10, _endPos + Vector3.left * 10, _nodeData.color, null, 2);
                    }
                    catch { }
                }
            }
        }

        void DrawSolidRectangleWithOutline(ref Vector3[] cachedRect, Color faceColor, Color typeColor)
        {
            try { 
                Handles.DrawSolidRectangleWithOutline(cachedRect, faceColor, typeColor);
            }
            catch { }
        }

        Rect TransformRectSpace(Rect rect, Rect oldContainer, Rect newContainer)
        {
            var result = new Rect();
            result.xMin = Mathf.Lerp(newContainer.xMin, newContainer.xMax, Mathf.InverseLerp(oldContainer.xMin, oldContainer.xMax, rect.xMin));
            result.xMax = Mathf.Lerp(newContainer.xMin, newContainer.xMax, Mathf.InverseLerp(oldContainer.xMin, oldContainer.xMax, rect.xMax));
            result.yMin = Mathf.Lerp(newContainer.yMin, newContainer.yMax, Mathf.InverseLerp(oldContainer.yMin, oldContainer.yMax, rect.yMin));
            result.yMax = Mathf.Lerp(newContainer.yMin, newContainer.yMax, Mathf.InverseLerp(oldContainer.yMin, oldContainer.yMax, rect.yMax));
            return result;
        }


        Rect GetRectBoundRect(Rect[] positions)
        {
            var _xMin = float.PositiveInfinity;
            var _xMax = float.NegativeInfinity;
            var _yMin = float.PositiveInfinity;
            var _yMax = float.NegativeInfinity;

            for (var i = 0; i < positions.Length; i++)
            {
                _xMin = Mathf.Min(_xMin, positions[i].xMin);
                _xMax = Mathf.Max(_xMax, positions[i].xMax);
                _yMin = Mathf.Min(_yMin, positions[i].yMin);
                _yMax = Mathf.Max(_yMax, positions[i].yMax);
            }

            return Rect.MinMaxRect(_xMin, _yMin, _xMax, _yMax);
        }


        public Rect CalculateRectToFitAll(VisualElement container)
        {
            Rect rectToFit = container.layout;
            bool reachedFirstChild = false;

            nodeCanvas.nodeUIElements.ForEach(ge =>
            {
                if (!ge.nodeData.isDeleted)
                {
                    if (!reachedFirstChild)
                    {
                        rectToFit = ge.ChangeCoordinatesTo(container, ge.layout);
                        reachedFirstChild = true;
                    }
                    else
                    {
                        rectToFit = Utils.LogicHelpers.Encompass(rectToFit, ge.ChangeCoordinatesTo(container, ge.layout));
                    }
                }
            });

            return rectToFit;
        }


        private void OnMouseDown(MouseDownEvent e)
        {
            if (nodeCanvas == null)
            {
                // Nothing to do if we're not attached to a GraphView!
                return;
            }

            // Refresh MiniMap rects
            //CalculateRects(graphView.contentViewContainer);

            var mousePosition = e.localMousePosition;

            nodeCanvas.nodeUIElements.ForEach(child =>
            {
                if (child == null)
                    return;
                //var selectable = child.GetFirstOfType<ISelectable>();
                //if (selectable == null || !selectable.IsSelectable())
                //    return;

                var _nodeBounds = CalculateRectToFitAll(nodeCanvasContainer);
                var _originalNodeRect = new Rect(child.transform.position.x, child.transform.position.y, child.layout.width, child.layout.height);
                var _nodeMini = TransformRectSpace(_originalNodeRect, _nodeBounds, this.contentRect);
                if (_nodeMini.Contains(mousePosition))
                {
                    nodeCanvas.DeselectAll(null);
                    nodeCanvas.AddToSelection(child);
                    nodeCanvas.FrameSelection();
                    //graphView.ClearSelection();
                    //graphView.AddToSelection(selectable);
                    //graphView.FrameSelection();
                    e.StopPropagation();
                }
            });

            //EatMouseDown(e);
            e.StopPropagation();
        }

    }
}
#endif