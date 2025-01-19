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

namespace Databrain.Logic.Manipulators
{
	public class GroupDragManipulator : MouseManipulator
	{

		private NodeCanvasElement nodeCanvas;
		private bool enabled;

		private GroupData groupData;

		public GroupDragManipulator(GroupVisualElement _target, NodeCanvasElement _canvas, GroupData _groupData)
		{
			activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
			activators.Add(new ManipulatorActivationFilter { button = MouseButton.RightMouse });

			target = _target;
			nodeCanvas = _canvas;
			groupData = _groupData;
		}

		protected override void RegisterCallbacksOnTarget()
		{
			target.RegisterCallback<MouseDownEvent>(MouseDownHandler);
			target.RegisterCallback<MouseUpEvent>(MouseUpHandler);
			target.RegisterCallback<MouseMoveEvent>(MouseMoveHandler);
		}

		protected override void UnregisterCallbacksFromTarget()
		{
			target.UnregisterCallback<MouseDownEvent>(MouseDownHandler);
			target.UnregisterCallback<MouseUpEvent>(MouseUpHandler);
			target.UnregisterCallback<MouseMoveEvent>(MouseMoveHandler);
		}

		public void MouseDownHandler(MouseDownEvent _evt)
		{
			if (enabled)
			{
				_evt.StopImmediatePropagation();
				return;
			}

			if (CanStartManipulation(_evt))
			{
				if (_evt.button == 0)
				{
					//targetStartPosition = target.transform.position;
					//pointerStartPosition = _evt.localMousePosition;
					target.CaptureMouse();

					enabled = true;
					

					_evt.StopPropagation();
				}


				if (_evt.button == 1)
				{
					target.AddManipulator(new ContextualMenuManipulator((ContextualMenuPopulateEvent evt) =>
					{
						evt.menu.AppendAction("delete group", (e) =>
						{
							(target as GroupVisualElement).nodeEditor.DeleteGroup(groupData, (target as GroupVisualElement));

							_evt.StopPropagation();
						});
					}));
				}
				// Store target position on other selected nodes
				//for (int i = 0; i < graphVisualElement.selectedNodes.Count; i ++)
				//{
				//	if (graphVisualElement.selectedNodes[i] != (target as NodeVisualElement))
				//	{	
				//		graphVisualElement.selectedNodes[i].targetStartPosition = graphVisualElement.selectedNodes[i].transform.position;
				//	}
				//}
				//}
			}
		}

		public void MouseMoveHandler(MouseMoveEvent _evt)
		{
			if (!enabled || !target.HasMouseCapture())
				return;


			//Vector3 pointerDelta = _evt.localMousePosition - pointerStartPosition;

			// With clamping
			//root.transform.position = new Vector2(
			//	Mathf.Clamp(targetStartPosition.x + pointerDelta.x, 0, target.panel.visualTree.worldBound.width),
			//	Mathf.Clamp(targetStartPosition.y + pointerDelta.y, 0, target.panel.visualTree.worldBound.height)
			//);
			//Debug.Log(graphVisualElement.zoomScale);
			//var targetScale = graphVisualElement.zoomScale;


			//pointerDelta.x /= nodeCanvas.zoomScale;
			//pointerDelta.y /= nodeCanvas.zoomScale;


			target.transform.position = new Vector3(target.transform.position.x + (_evt.mouseDelta.x), target.transform.position.y + _evt.mouseDelta.y, 0);


			// Update position value in node class as well
			groupData.position = target.transform.position;

			for (int g = 0; g < groupData.assignedNodes.Count; g++)
			{
				if (groupData.assignedNodes[g] == null)
					continue;
				if (groupData.assignedNodes[g].nodeVisualElement == null)
					continue;

				var _assignedNodePosition = groupData.assignedNodes[g].nodeVisualElement.transform.position;

				groupData.assignedNodes[g].nodeVisualElement.transform.position = new Vector3(_assignedNodePosition.x + _evt.mouseDelta.x, _assignedNodePosition.y + _evt.mouseDelta.y, 0);
				groupData.assignedNodes[g].position = groupData.assignedNodes[g].nodeVisualElement.transform.position;
				groupData.assignedNodes[g].worldPosition = nodeCanvas.WorldToLocal(groupData.assignedNodes[g].nodeVisualElement.transform.position);
			}


			_evt.StopPropagation();
		}
		public void MouseUpHandler(MouseUpEvent _evt)
		{
			if (!enabled || !target.HasMouseCapture() || !CanStopManipulation(_evt))
				return;


			target.ReleaseMouse();
			enabled = false;
			_evt.StopPropagation();

		}
		public void PointerCaptureOutHandler(PointerCaptureOutEvent _evt)
		{
		}
	}
}
#endif