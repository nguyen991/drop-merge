/*
 *	DATABRAIN | Logic
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using Databrain.Logic.Elements;
using System;

using UnityEngine;
using UnityEngine.UIElements;

namespace Databrain.Logic.Manipulators
{
	public class ZoomPanManipulator : Manipulator
	{
		#region Default Values
		public static readonly float DefaultReferenceScale = 1;
		public static readonly float DefaultMinScale = 0.25f;
		public static readonly float DefaultMaxScale = 1;
		public static readonly float DefaultScaleStep = 0.15f;
		#endregion

		/// <summary>
		/// Scale that should be computed when scroll wheel offset is at zero.
		/// </summary>
		public float ReferenceScale { get; set; } = DefaultReferenceScale;

		public float MinZoom { get; set; } = DefaultMinScale;
		public float MaxZoom { get; set; } = DefaultMaxScale;


		/// <summary>
		/// Relative scale change when zooming in/out (e.g. For 15%, use 0.15).
		/// </summary>
		/// <remarks>
		/// Depending on the values of <c>minScale</c>, <c>maxScale</c> and <c>scaleStep</c>, it is not guaranteed that
		/// the first and last two scale steps will correspond exactly to the value specified in <c>scaleStep</c>.
		/// </remarks>
		public float ZoomStep { get; set; } = DefaultScaleStep;


		float scale = 1;
		public bool panning;
		float pointX;
		float pointY;
		Vector3 pointerStartPosition;
		Vector3 startCanvasPosition;
        Vector3 start;

        public VisualElement canvas;
		public VisualElement nodeCanvas;
		private MinimapElement minimap;

        public ZoomPanManipulator(VisualElement _canvas, VisualElement _nodeCanvas, MinimapElement _minimap)
		{
			nodeCanvas = _nodeCanvas;
            canvas = _canvas;
			minimap = _minimap;
        }

        public ZoomPanManipulator(float min, float max, float step)
        {
            MinZoom = min;
            MaxZoom = max;
            ZoomStep = step;

            canvas = target;
            canvas.style.transformOrigin = (new TransformOrigin(0, 0, 0));

        }


        protected override void RegisterCallbacksOnTarget()
		{
            nodeCanvas.RegisterCallback<PointerDownEvent>(PointerDown);
            nodeCanvas.RegisterCallback<PointerMoveEvent>(PointerMove);
            nodeCanvas.RegisterCallback<PointerUpEvent>(PointerUp);
            nodeCanvas.RegisterCallback<WheelEvent>(OnWheel);	
		}

		protected override void UnregisterCallbacksFromTarget()
		{
            nodeCanvas.UnregisterCallback<PointerDownEvent>(PointerDown);
            nodeCanvas.UnregisterCallback<PointerMoveEvent>(PointerMove);
            nodeCanvas.UnregisterCallback<PointerUpEvent>(PointerUp);
            nodeCanvas.UnregisterCallback<WheelEvent>(OnWheel);
        }


        void OnWheel(WheelEvent evt)
		{
			canvas.style.transformOrigin = (new TransformOrigin(0, 0, 0));

            var pos = canvas.transform.position;
			var _zoomCenter = nodeCanvas.ChangeCoordinatesTo(canvas, evt.localMousePosition);
            float x2 = _zoomCenter.x + canvas.layout.x;
			float y2 = _zoomCenter.y + canvas.layout.y;
			//Debug.Log("center " + _zoomCenter);
		
			pos += Vector3.Scale(new Vector3(x2, y2, 0), new Vector3(scale, scale, 1));

            scale = CalculateNewZoom(scale, -evt.delta.y, ZoomStep, ReferenceScale, MinZoom, MaxZoom);//Calculate zoom
			

			pos -= Vector3.Scale(new Vector3(x2, y2, 0), new Vector3(scale, scale, 1));

			//Debug.Log("New pos: " + pos);
			canvas.transform.position = pos;

            var _s = new Scale(new Vector3(Vector3.one.x * scale, Vector3.one.y * scale, 1.0f));
            canvas.style.scale = new StyleScale(_s);
        }

		void PointerDown(PointerDownEvent evt)
		{
			if (evt.button == 2 || (evt.button == 1 && evt.altKey) || (evt.button == 0 && evt.altKey))
			{
				#if UNITY_6000_0_OR_NEWER
				evt.StopPropagation();
				#else
				evt.PreventDefault();
				#endif

				start = new Vector2(evt.position.x - pointX, evt.position.y - pointY);
                pointerStartPosition = evt.position;
                startCanvasPosition = canvas.transform.position;

                panning = true;
			}
		}

		void PointerMove(PointerMoveEvent evt)
		{
		
			#if UNITY_6000_0_OR_NEWER
			evt.StopPropagation();
			#else
			evt.PreventDefault();
			#endif

			if (!panning) return;

            Vector3 _pointerDelta = evt.position - pointerStartPosition;
			var _newPosition = new Vector2(startCanvasPosition.x + _pointerDelta.x, startCanvasPosition.y + _pointerDelta.y);

			pointX = _newPosition.x;
			pointY = _newPosition.y;

			canvas.transform.position = _newPosition;
			//Debug.Log(_newPosition);
			
			//SetTransform();
		}

		void PointerUp(PointerUpEvent evt)
		{
			panning = false;
		}

		// Compute the parameters of our exponential model:
		// z(w) = (1 + s) ^ (w + a) + b
		// Where
		// z: calculated zoom level
		// w: accumulated wheel deltas (1 unit = 1 mouse notch)
		// s: zoom step
		//
		// The factors a and b are calculated in order to satisfy the conditions:
		// z(0) = referenceZoom
		// z(1) = referenceZoom * (1 + zoomStep)
		private static float CalculateNewZoom(float currentZoom, float wheelDelta, float zoomStep, float referenceZoom, float minZoom, float maxZoom)
		{
			#region Validation
			if (minZoom <= 0)
			{
				//Debug.LogError($"The minimum zoom ({minZoom}) must be greater than zero.");
				return currentZoom;
			}
			if (referenceZoom < minZoom)
			{
				//Debug.LogError($"The reference zoom ({referenceZoom}) must be greater than or equal to the minimum zoom ({minZoom}).");
				return currentZoom;
			}
			if (referenceZoom > maxZoom)
			{
				//Debug.LogError($"The reference zoom ({referenceZoom}) must be less than or equal to the maximum zoom ({maxZoom}).");
				return currentZoom;
			}
			if (zoomStep < 0)
			{
				//Debug.LogError($"The zoom step ({zoomStep}) must be greater than or equal to zero.");
				return currentZoom;
			}
			#endregion

			currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

			if (Mathf.Approximately(wheelDelta, 0)) return currentZoom;

			// Calculate the factors of our model:
			double a = Math.Log(referenceZoom, 1 + zoomStep);
			double b = referenceZoom - Math.Pow(1 + zoomStep, a);

			// Convert zoom levels to scroll wheel values.
			double minWheel = Math.Log(minZoom - b, 1 + zoomStep) - a;
			double maxWheel = Math.Log(maxZoom - b, 1 + zoomStep) - a;
			double currentWheel = Math.Log(currentZoom - b, 1 + zoomStep) - a;

			// Except when the delta is zero, for each event, consider that the delta corresponds to a rotation by a
			// full notch. The scroll wheel abstraction system is buggy and incomplete: with a regular mouse, the
			// minimum wheel movement is 0.1 on OS X and 3 on Windows. We can't simply accumulate deltas like these, so
			// we accumulate integers only. This may be problematic with high resolution scroll wheels: many small
			// events will be fired. However, at this point, we have no way to differentiate a high resolution scroll
			// wheel delta from a non-accelerated scroll wheel delta of one notch on OS X.
			wheelDelta = Math.Sign(wheelDelta);
			currentWheel += wheelDelta;

			// Assimilate to the boundary when it is nearby.
			if (currentWheel > maxWheel - 0.5) return maxZoom;
			if (currentWheel < minWheel + 0.5) return minZoom;


			// Snap the wheel to the unit grid.
			currentWheel = Math.Round(currentWheel);

			// Do not assimilate again. Otherwise, points as far as 1.5 units away could be stuck to the boundary
			// because the wheel delta is either +1 or -1.

			// Calculate the corresponding zoom level.
			return (float)(Math.Pow(1 + zoomStep, currentWheel + a) + b);
		}
	}
}
#endif