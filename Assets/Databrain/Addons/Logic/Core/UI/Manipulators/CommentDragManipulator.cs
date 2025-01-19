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
	public class CommentDragManipulator : MouseManipulator
	{

		private NodeCanvasElement nodeCanvas;
		private bool enabled;

		private CommentData commentData;

		public CommentDragManipulator(CommentVisualElement _target, NodeCanvasElement _canvas, CommentData _commentData)
		{
			activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
			activators.Add(new ManipulatorActivationFilter { button = MouseButton.RightMouse });

			target = _target;
			nodeCanvas = _canvas;
            commentData = _commentData;
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
						evt.menu.AppendAction("delete comment", (e) =>
						{
							(target as CommentVisualElement).nodeEditor.DeleteComment(commentData, (target as CommentVisualElement));

							_evt.StopPropagation();
						});
					}));
				}
			}
		}

		public void MouseMoveHandler(MouseMoveEvent _evt)
		{
			if (!enabled || !target.HasMouseCapture())
				return;


			target.transform.position = new Vector3(target.transform.position.x + (_evt.mouseDelta.x), target.transform.position.y + _evt.mouseDelta.y, 0);


			// Update position value in node class as well
			commentData.position = target.transform.position;


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