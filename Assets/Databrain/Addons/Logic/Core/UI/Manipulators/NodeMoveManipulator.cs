/*
 *	DATABRAIN | Logic
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UIElements;

using Databrain.Logic.Elements;
using UnityEditor;

namespace Databrain.Logic.Manipulators
{
	public class NodeMoveManipulator : PointerManipulator
	{
		
		private NodeEditor nodeEditor;
		private NodeCanvasElement nodeCanvas;
		private VisualElement nodeContainer;
		private NodeVisualElement nodeVisualElement;
		private Vector2 targetStartPosition;
		private Vector3 pointerStartPosition;
		private bool enabled;

		public NodeMoveManipulator(VisualElement _dragElement, NodeVisualElement _nodeVisualElement, NodeCanvasElement _canvas, VisualElement _nodeContainer, NodeEditor _nodeEditor)
		{
			this.target = _dragElement;
			nodeVisualElement = _nodeVisualElement;
			nodeCanvas = _canvas;
			nodeContainer = _nodeContainer;
			nodeEditor = _nodeEditor;
		}


		protected override void RegisterCallbacksOnTarget()
		{
			target.RegisterCallback<PointerDownEvent>(PointerDownHandler);
			target.RegisterCallback<PointerUpEvent>(PointerUpHandler);
			target.RegisterCallback<PointerMoveEvent>(PointerMoveHandler);
			target.RegisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
		}

		protected override void UnregisterCallbacksFromTarget()
		{
			target.UnregisterCallback<PointerDownEvent>(PointerDownHandler);
			target.UnregisterCallback<PointerUpEvent>(PointerUpHandler);
			target.UnregisterCallback<PointerMoveEvent>(PointerMoveHandler);
			target.UnregisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
		}

		public void PointerDownHandler(PointerDownEvent _evt)
		{
			if (_evt.button == 2)
				return;


			targetStartPosition = nodeVisualElement.transform.position;
            Vector3 _local = nodeContainer.WorldToLocal(_evt.position);
			pointerStartPosition = _local; // _evt.position;
			target.CapturePointer(_evt.pointerId);

			// Store target position on other selected nodes
			for (int i = 0; i < nodeCanvas.selectedNodes.Count; i++)
			{
				if (nodeCanvas.selectedNodes[i] != nodeVisualElement)
				{
					nodeCanvas.selectedNodes[i].targetStartPosition = nodeCanvas.selectedNodes[i].transform.position;
				}
			}

			//nodeCanvas.DeselectAll();
			if (!nodeCanvas.IsSelected(nodeVisualElement))
			{
				nodeCanvas.DeselectAll(null);
			}
            nodeCanvas.AddToSelection(nodeVisualElement);
			//(target as NodeVisualElement).SelectNode();

			if (_evt.button == 1)
			{
				target.AddManipulator(new ContextualMenuManipulator((ContextualMenuPopulateEvent evt) =>
				{

                    evt.menu.AppendAction("edit", (e) =>
					{

						var script = UnityEditor.MonoScript.FromScriptableObject(nodeVisualElement.nodeData);
                        var path = UnityEditor.AssetDatabase.GetAssetPath(script);

                        UnityEditor.AssetDatabase.OpenAsset(UnityEditor.AssetDatabase.LoadMainAssetAtPath(path));

                    });

                    evt.menu.AppendAction("delete", (e) =>
                    {
                        var targetNode = nodeVisualElement;

                        targetNode.nodeEditor.DeleteNode(targetNode.nodeData);
                        var _node = targetNode.nodeData;
                        _node.isDeleted = true;
                        targetNode.graphData.nodes.Remove(_node);

                        AssetDatabase.RemoveObjectFromAsset(_node);

                        // simply hide this node visual element.
                        targetNode.visible = false;
                        targetNode.pickingMode = PickingMode.Ignore;

                        nodeCanvas.DeselectAll(null);

                        AssetDatabase.Refresh();
                        AssetDatabase.SaveAssets();

                        _evt.StopPropagation();
                    });
                }));

            
            }


            enabled = true;
		}
		public void PointerMoveHandler(PointerMoveEvent _evt)
		{
			if (enabled && target.HasPointerCapture(_evt.pointerId) && !_evt.altKey)
			{
				Vector3 _local = nodeContainer.WorldToLocal(_evt.position);
                Vector3 pointerDelta = _local - pointerStartPosition;

				var _position = new Vector2(targetStartPosition.x + pointerDelta.x, targetStartPosition.y + pointerDelta.y);
				//var _gridPosition = new Vector2(Mathf.RoundToInt(_position.x / 10) * 10, Mathf.RoundToInt(_position.y / 10) * 10);

				nodeVisualElement.transform.position = _position;
				nodeVisualElement.nodeData.position = _position;
				nodeVisualElement.nodeData.worldPosition = nodeCanvas.WorldToLocal(_position);

				// Move other selected nodes
				for (int i = 0; i < nodeCanvas.selectedNodes.Count; i++)
				{
					if (nodeCanvas.selectedNodes[i] != nodeVisualElement)
					{
                        var _positionC = new Vector2(nodeCanvas.selectedNodes[i].targetStartPosition.x + pointerDelta.x, nodeCanvas.selectedNodes[i].targetStartPosition.y + pointerDelta.y);
                        //var _gridPositionC = new Vector2(Mathf.RoundToInt(_positionC.x / 10) * 10, Mathf.RoundToInt(_positionC.y / 10) * 10);

						nodeCanvas.selectedNodes[i].transform.position = _positionC; // new Vector2(nodeCanvas.selectedNodes[i].targetStartPosition.x + pointerDelta.x, nodeCanvas.selectedNodes[i].targetStartPosition.y + pointerDelta.y);
						nodeCanvas.selectedNodes[i].nodeData.position = _positionC; // nodeCanvas.selectedNodes[i].transform.position;
						nodeCanvas.selectedNodes[i].nodeData.worldPosition = nodeCanvas.WorldToLocal(_positionC); // nodeCanvas.WorldToLocal(nodeCanvas.selectedNodes[i].transform.position);
					}
				}

			}
		}
		public void PointerUpHandler(PointerUpEvent _evt)
		{
			if (enabled && target.HasPointerCapture(_evt.pointerId))
			{
                var _zoomPanManipulator = nodeEditor.manipulatorManager.GetManipulator<ZoomPanManipulator>(nodeContainer);
				_zoomPanManipulator.panning = false;

                target.ReleasePointer(_evt.pointerId);
				enabled = false;
			}
		}
		public void PointerCaptureOutHandler(PointerCaptureOutEvent _evt)
		{
			
		}
	}
}
#endif