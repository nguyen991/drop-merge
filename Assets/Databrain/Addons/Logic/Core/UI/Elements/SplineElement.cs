/*
 *	DATABRAIN | Logic
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

namespace Databrain.Logic.Elements
{
	public class SplineElement : ImmediateModeElement
	{

        public VisualElement nodeContainer;
        public VisualElement startNode;
		public VisualElement endNode;
		//public VisualElement startKnot;

		public Vector2 endPosition;
		public Color splineColor = Color.white;
		public float splineWidth = 4;

		private Vector2 _positionA;
		private Vector2 _positionB;
		private int nodeHeaderOffset = 50;
		private Vector2 startOffset = new Vector2(0, 0);
		private float _startNodeHeight;
		private float _endNodeHeight;
		private int outputIndex;

		public Texture2D splineTexture;

		public enum SplineEndType
		{
			cursor,
			node
		}

		public SplineEndType splineEndType;

		public bool isVisible;

		public SplineElement(VisualElement _nodeContainer, int _outputIndex)
		{
			nodeContainer = _nodeContainer;
            outputIndex = _outputIndex;

			style.position = Position.Absolute;
			pickingMode = PickingMode.Ignore;
			
		}


		public void SetVisible(bool _visible)
		{
			isVisible = _visible;
		}

		public void SetColor(Color _color)
		{
			splineColor = _color;
		}



		void UpdateBounds()
		{

			var _a = new Vector2(startNode.transform.position.x + startNode.resolvedStyle.width - 10, startNode.transform.position.y + nodeHeaderOffset + (20 * outputIndex));


			_positionA = new Vector2(_a.x + startOffset.x, _a.y + startOffset.y); 

			if (splineEndType == SplineEndType.node)
			{
				_positionB = new Vector2(endNode.transform.position.x + 10, endNode.transform.position.y + nodeHeaderOffset);
			}
			else
			{
				var _b = endPosition;

				var _p = nodeContainer.WorldToLocal(endPosition);
				_b = _p;

				//_b.x -= nodeContainer.worldBound.position.x;
				//_b.y -= nodeContainer.worldBound.position.y;

				_positionB = _b;

                MarkDirtyRepaint();
            }

			if (startNode != null)
			{
				_startNodeHeight = startNode.resolvedStyle.height;
			}
            
			if (endNode != null)
			{
				_endNodeHeight = endNode.resolvedStyle.height;
			}
			
			Vector2 min = Vector2.Min(_positionA, _positionB);
			Vector2 max = Vector2.Max(_positionA, _positionB);

			style.left = min.x;
			style.top = min.y;
			style.width = max.x - min.x;
			style.height = max.y - min.y;

        }


		protected override void ImmediateRepaint()
		{
			if (!isVisible)
				return;

			Vector2 min = Vector2.Min(_positionA, _positionB);
			Vector3 relA = _positionA - min;
			Vector3 relB = _positionB - min;


			Vector3 offsetPoint = relB - relA;


			if (splineEndType == SplineEndType.node)
			{
				Vector3 _startTan = relA + Vector3.right * 30;
				Vector3 _endTan = relB + Vector3.left * 30;

				if (relA.x < relB.x)
				{
					var _distX = Mathf.Abs(relB.x - relA.x);
					var _distY = Mathf.Abs(relB.y - relA.y);

					if (_distX < 50 && _distY < 100)
					{
						Vector3 _startTan2 = relA + Vector3.right;
						Vector3 _endTan2 = relB + Vector3.left;
						Handles.DrawBezier(relA, relB, _startTan2, _endTan2, splineColor, splineTexture, splineWidth);
					}
					else
					{
						var _point1 = new Vector3(relA.x + 10, relA.y);
						var _point2 = new Vector3(relB.x - 10, relB.y);

						

						Handles.DrawBezier(relA, _point1, relA + Vector3.right * 10, _point1 + Vector3.left * 10, splineColor, splineTexture, splineWidth);
						Handles.DrawBezier(_point2, relB, _point2 + Vector3.right * 10, relB + Vector3.left * 10, splineColor, splineTexture, splineWidth);
						Handles.DrawBezier(_point1, _point2, _point1 + Vector3.right * 50, _point2 + Vector3.left * 50, splineColor, splineTexture, splineWidth);
					}
				}
				else
				{

					var _heightOffset = Mathf.Abs(relA.y - relB.y);
                    var _maxHeight = Mathf.Max(_startNodeHeight, _endNodeHeight);

					if (relA.y < relB.y)
					{
						var _point1 = new Vector3(relA.x + 10, relA.y);
						// var _point2 = new Vector3(relA.x + 10, (relB.y - 50) - (3f * outputIndex )); //(relB.y * 0.15f)); //relA.y + ((relB.y - relA.y) * 0.50f));
						// var _point3 = new Vector3(relB.x - 10, (relB.y - 50) - (3f * outputIndex )); //relA.y + ((relB.y - relA.y) * 0.50f));
						// var _point2 = new Vector3(relA.x + 10, relA.y + _startNodeHeight + ((relB.y - relA.y)) - (3f * outputIndex ));
						var _point2 = new Vector3(relA.x + 10, relA.y + ((relB.y - relA.y) * 0.50f) - (3f * outputIndex )); 
						var _point3 = new Vector3(relB.x - 10, relA.y + ((relB.y - relA.y) * 0.50f) - (3f * outputIndex ));

						var _point4 = new Vector3(relB.x - 10, relB.y);
					
						// if (_heightOffset < 10)
						// {
						// 	_point2 = new Vector3(_point2.x, _maxHeight - (nodeHeaderOffset - 10));
						// 	_point3 = new Vector3(_point3.x, _maxHeight - (nodeHeaderOffset - 10));
						// }

							
						Handles.DrawBezier(relA, _point1, relA + Vector3.right * 10, _point1 + Vector3.left * 10, splineColor, splineTexture, splineWidth);
						Handles.DrawBezier(_point1, _point2, _point1 + Vector3.right * 30, _point2 + Vector3.right * 30, splineColor, splineTexture, splineWidth);
						Handles.DrawBezier(_point2, _point3, _point2 + Vector3.left * 10, _point3 + Vector3.right * 10, splineColor, splineTexture, splineWidth);
						Handles.DrawBezier(_point3, _point4, _point3 + Vector3.left * 30, _point4 + Vector3.left * 30, splineColor, splineTexture, splineWidth);
						Handles.DrawBezier(_point4, relB, _point4 + Vector3.right * 10, relB + Vector3.left * 10, splineColor, splineTexture, splineWidth);

					}
					else
					{
						var _point1 = new Vector3(relA.x + 10, relA.y);
						// var _point2 = new Vector3(relA.x + 10, relA.y - nodeHeaderOffset - (20 * outputIndex) - (3f * outputIndex )); // - (relA.y * 0.15f)); //relA.y + ((relB.y - relA.y) * 0.50f));
						// var _point3 = new Vector3(relB.x - 10, relA.y - nodeHeaderOffset - (20 * outputIndex) - (3f * outputIndex)); // - (relA.y * 0.15f)); //relA.y + ((relB.y - relA.y) * 0.50f));
						var _point2 = new Vector3(relA.x + 10, relA.y + ((relB.y - relA.y) * 0.50f));
						var _point3 = new Vector3(relB.x - 10, relA.y + ((relB.y - relA.y) * 0.50f));
						
						var _point4 = new Vector3(relB.x - 10, relB.y);
					

						Handles.DrawBezier(relA, _point1, relA + Vector3.right * 10, _point1 + Vector3.left * 10, splineColor, splineTexture, splineWidth);
						Handles.DrawBezier(_point1, _point2, _point1 + Vector3.right * 30, _point2 + Vector3.right * 30, splineColor, splineTexture, splineWidth);
						Handles.DrawBezier(_point2, _point3, _point2 + Vector3.left * 10, _point3 + Vector3.right * 10, splineColor, splineTexture, splineWidth);
						Handles.DrawBezier(_point3, _point4, _point3 + Vector3.left * 30, _point4 + Vector3.left * 30, splineColor, splineTexture, splineWidth);
						Handles.DrawBezier(_point4, relB, _point4 + Vector3.right * 10, relB + Vector3.left * 10, splineColor, splineTexture, splineWidth);

					}
				


				}

			}
			else
			{
				Vector3 _startTan = relA + Vector3.right * 30;
				Vector3 _endTan = relB + Vector3.left * 100;
				Handles.DrawBezier(relA, relB, _startTan, _endTan, splineColor, splineTexture, splineWidth);
			}

			UpdateBounds();
		}


	}
}
#endif