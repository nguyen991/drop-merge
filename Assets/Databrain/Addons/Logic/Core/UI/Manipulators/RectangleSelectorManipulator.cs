/*
 *	DATABRAIN | Logic
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;

using Databrain.Logic.Elements;

namespace Databrain.Logic.Manipulators
{
	public class RectangleSelectorManipulator : MouseManipulator
	{
		private NodeCanvasElement graphView;
		private RectangleSelect rectangleSelect;
		private bool active;
		private float topOffset;
		private VisualElement sidebar;

		public RectangleSelectorManipulator(float _topOffset, VisualElement _sidebar, RectangleSelect _rect)
		{
			activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
			if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
			{
				activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse, modifiers = EventModifiers.Command });
			}
			else
			{
				activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse, modifiers = EventModifiers.Control });
			}

			//used for the x offset
			sidebar = _sidebar;
			topOffset = _topOffset;

			rectangleSelect = _rect;
			rectangleSelect.transform.position = Vector3.zero;
			rectangleSelect.SetEnabled(false);

			rectangleSelect.style.position = Position.Absolute;
			rectangleSelect.style.top = 0f;
			rectangleSelect.style.left = 0f;
			rectangleSelect.style.bottom = 0f;
			rectangleSelect.style.right = 0f;

			rectangleSelect.style.borderBottomColor = Color.white;
			rectangleSelect.style.borderTopColor = Color.white;
			rectangleSelect.style.borderLeftColor = Color.white;
			rectangleSelect.style.borderRightColor = Color.white;

			rectangleSelect.style.backgroundColor = new Color(255f / 255f, 255f / 255f, 255f / 255f, 20f / 255f);

			rectangleSelect.style.borderBottomWidth = 1;
			rectangleSelect.style.borderTopWidth = 1;
			rectangleSelect.style.borderLeftWidth = 1;
			rectangleSelect.style.borderRightWidth = 1;


		}


		protected override void RegisterCallbacksOnTarget()
		{
			graphView = target.Q<VisualElement>("nodeCanvas") as NodeCanvasElement;

			target.RegisterCallback<MouseDownEvent>(OnMouseDown);
			target.RegisterCallback<MouseUpEvent>(OnMouseUp);
			target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
			target.RegisterCallback<MouseCaptureOutEvent>(OnMouseCaptureOutEvent);
		}

		protected override void UnregisterCallbacksFromTarget()
		{
			target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
			target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
			target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
			target.UnregisterCallback<MouseCaptureOutEvent>(OnMouseCaptureOutEvent);
		}

		void OnMouseCaptureOutEvent(MouseCaptureOutEvent e)
		{
			if (active)
			{
				rectangleSelect.RemoveFromHierarchy();
				active = false;
			}
		}

		private void OnMouseDown(MouseDownEvent e)
		{
			if (active)
			{
				return;
			}

			var _pickElement = target.panel.Pick(e.mousePosition);
			if (_pickElement.GetFirstOfType<NodeVisualElement>() != null)
			{
				active = false;
				target.ReleaseMouse();
				return;
			}

			if (e.button == 2)
			{
				active = false;
				target.ReleaseMouse();
				return;
			}


			if (CanStartManipulation(e))
			{
				if (!e.actionKey)
				{
					//graphView.ClearSelection();
				}

				rectangleSelect.transform.position = Vector3.zero;

				rectangleSelect.style.borderBottomWidth = 1;
				rectangleSelect.style.borderTopWidth = 1;
				rectangleSelect.style.borderLeftWidth = 1;
				rectangleSelect.style.borderRightWidth = 1;
				rectangleSelect.style.backgroundColor = new Color(255f / 255f, 255f / 255f, 255f / 255f, 20f / 255f);

				rectangleSelect.start = new Vector2(e.localMousePosition.x - sidebar.resolvedStyle.width, e.localMousePosition.y - topOffset);
				rectangleSelect.end = rectangleSelect.start;

				rectangleSelect.SetEnabled(true);

				active = true;
				target.CaptureMouse(); // We want to receive events even when mouse is not over ourself.
				e.StopImmediatePropagation();
			}
		}


		private void OnMouseUp(MouseUpEvent e)
		{

			if (!active)
				return;

			if (!CanStopManipulation(e))
				return;


			rectangleSelect.end = e.localMousePosition;

			var selectionRect = new Rect()
			{
				min = new Vector2(Math.Min(rectangleSelect.start.x, rectangleSelect.end.x), Math.Min(rectangleSelect.start.y, rectangleSelect.end.y)),
				max = new Vector2(Math.Max(rectangleSelect.start.x, rectangleSelect.end.x), Math.Max(rectangleSelect.start.y, rectangleSelect.end.y))
			};

	
			var _rect = new Rect(rectangleSelect.worldBound.x, rectangleSelect.worldBound.y, rectangleSelect.worldBound.size.x, rectangleSelect.worldBound.size.y);
	
			List<VisualElement> selectedNodes = new List<VisualElement>();

			graphView.DeselectAll(null);

			graphView.nodeUIElements.ForEach(child =>
			{

				if (child.worldBound.Overlaps(_rect))
				{
					selectedNodes.Add(child);
				}
			});

			graphView.selectedNodes = new List<NodeVisualElement>();

			foreach (var selectable in selectedNodes)
			{
				if (!(selectable as NodeVisualElement).nodeData.isDeleted)
				{
					graphView.AddToSelection(selectable as NodeVisualElement);
				}
			}

			rectangleSelect.style.backgroundColor = new Color(255f / 255f, 255f / 255f, 255f / 255f, 0f / 255f);
			rectangleSelect.style.borderBottomWidth = 0;
			rectangleSelect.style.borderTopWidth = 0;
			rectangleSelect.style.borderLeftWidth = 0;
			rectangleSelect.style.borderRightWidth = 0;
			rectangleSelect.start = Vector2.zero;
			rectangleSelect.end = Vector2.zero;

			rectangleSelect.SetEnabled(false);

			active = false;
			target.ReleaseMouse();
		}

		private void OnMouseMove(MouseMoveEvent e)
		{
			if (!active)
				return;

			rectangleSelect.end = new Vector2(e.localMousePosition.x - sidebar.resolvedStyle.width, e.localMousePosition.y - topOffset); ;
			e.StopPropagation();
		}


		// get the axis aligned bound
		public Rect ComputeAxisAlignedBound(Rect position, Matrix4x4 transform)
		{
			Vector3 min = transform.MultiplyPoint3x4(position.min);
			Vector3 max = transform.MultiplyPoint3x4(position.max);
			return Rect.MinMaxRect(Math.Min(min.x, max.x), Math.Min(min.y, max.y), Math.Max(min.x, max.x), Math.Max(min.y, max.y));
		}
	}
}
#endif