/*
 *	DATABRAIN | Logic
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UIElements;

using Databrain.Logic.Attributes;
using Databrain.Logic.Elements;

using System.Reflection;
using System.Threading.Tasks;
using System.Linq;
using UnityEditor.VersionControl;
using UnityEditor;

namespace Databrain.Logic.Manipulators
{
	public class DragEdgeManipulator : Manipulator
	{
		public NodeVisualElement nodeElement;
        public VisualElement nodeContainer;
        public SplineElement spline;
        public VisualElement slot;

		private bool enabled;
		private NodeData nodeData;
		private int outputIndex;
		private NodeEditor nodeEditor;
		private Label wrongConnectionTypeLabel;
		private bool connectionTypeOk = true;
        private Color splineOriginalColor;
		private NodeVisualElement currentMouseOverNodeElement;

        public DragEdgeManipulator(NodeData _nodeData, VisualElement _nodeElement, int _outputIndex, VisualElement _nodeContainer, VisualElement _target, NodeEditor _nodeEditor)
		{
			this.target = _target;
            nodeData = _nodeData;
			nodeElement = _nodeElement as NodeVisualElement;
            outputIndex = _outputIndex;
			nodeEditor = _nodeEditor;
            nodeContainer = _nodeContainer;

            spline = new SplineElement(_nodeContainer, _outputIndex);
			spline.startNode = _nodeElement;
			
			Color _borderSplineColor = _nodeData.color;
			if (_nodeData.borderColors != null)
			{
				if (_nodeData.borderColors.Length >= 2)
				{
					if (!string.IsNullOrEmpty(_nodeData.borderColors[1]))
					{
						
						ColorUtility.TryParseHtmlString(_nodeData.borderColors[1], out _borderSplineColor);
						
					}
				}
			}

			spline.splineColor = _borderSplineColor;
			splineOriginalColor = _borderSplineColor;
			spline.splineEndType = SplineElement.SplineEndType.cursor;

			if (_nodeData.GetType().GetCustomAttribute<Node3LinesSpline>() != null)
			{
				spline.splineTexture = _nodeEditor.splineTexture3Lines;
				spline.splineWidth = 12;
			}


			spline.SetVisible(false);


            slot = _target.Q<VisualElement>("slot");


            _nodeContainer.Add(spline);
		}


		protected override void RegisterCallbacksOnTarget()
		{
			target.RegisterCallback<MouseDownEvent>(MouseDownHandler);
			target.RegisterCallback<MouseUpEvent>(MouseUpHandler);
			target.RegisterCallback<MouseMoveEvent>(MouseMoveHandler);
			target.RegisterCallback<MouseCaptureOutEvent>(MouseCaptureOutHandler);
			target.RegisterCallback<MouseOverEvent>(MouseOverHandler);
			target.RegisterCallback<MouseOutEvent>(MouseOutHandler);
		}

		protected override void UnregisterCallbacksFromTarget()
		{
			target.UnregisterCallback<MouseDownEvent>(MouseDownHandler);
			target.UnregisterCallback<MouseUpEvent>(MouseUpHandler);
			target.UnregisterCallback<MouseMoveEvent>(MouseMoveHandler);
			target.UnregisterCallback<MouseCaptureOutEvent>(MouseCaptureOutHandler);
			target.UnregisterCallback<MouseOverEvent>(MouseOverHandler);
			target.UnregisterCallback<MouseOutEvent>(MouseOutHandler);
		}

		private void MouseDownHandler(MouseDownEvent _evt)
		{
			if (_evt.button == 0)
			{
				target.CaptureMouse();
				_evt.StopImmediatePropagation();

				//targetStartPosition = target.transform.position;
				//pointerStartPosition = _evt.mousePosition;
				//target.CapturePointer(_evt.pointerId);

				spline.SetVisible(true);
				spline.splineEndType = SplineElement.SplineEndType.cursor;
				spline.endPosition = _evt.mousePosition;

				if (spline.endNode != null)
				{
					//(spline.endNode as NodeVisualElement).DisconnectFrom(node);
					(spline.startNode as NodeVisualElement).DisconnectOutput(outputIndex);
					(spline.endNode as NodeVisualElement).DisconnectInput();
					spline.endNode = null;
                }

				enabled = true;
			}
		}

		public void DisconnectSpline()
		{
			spline.SetVisible(false);
			if (spline.endNode != null)
			{
				(spline.endNode as NodeVisualElement).DisconnectInput();
			}
		}

		private void MouseUpHandler(MouseUpEvent _evt)
		{
			if (enabled)
			{
				target.ReleaseMouse();
				_evt.StopImmediatePropagation();


				// Connect node
				var _pickElement = target.panel.Pick(_evt.mousePosition);
				if (_pickElement == null)
				{
					spline.SetVisible(false);
					return;
				}

				var _pickedNode = _pickElement.GetFirstOfType<NodeVisualElement>();
				if (_pickedNode != null && _pickedNode != nodeElement as NodeVisualElement)
				{
					if (connectionTypeOk)
					{
						if (nodeData.ConnectToNode(outputIndex, _pickedNode.nodeData) )
						{
							spline.splineEndType = SplineElement.SplineEndType.node;
							spline.endNode = _pickedNode;

							slot.style.backgroundColor = new Color(233f / 255f, 233f / 255f, 233f / 255f, 190f / 255f);
							(spline.endNode as NodeVisualElement).ConnectInput(); 
						}
						else
						{
							spline.SetVisible(false);
							if (spline.endNode != null)
							{
								(spline.endNode as NodeVisualElement).DisconnectInput();
							}
						}
					}
					else
					{
						spline.SetVisible(false);
                        if (spline.endNode != null)
                        {
                            (spline.endNode as NodeVisualElement).DisconnectInput();
                        }
                    }
				}
				else
				{

					if (_pickedNode != nodeElement as NodeVisualElement)
					{
						// Open Node panel
						var _panel = new NodePanel(nodeEditor, nodeData, outputIndex, OnNodePanelClose);
						NodePanel.ShowPanel(_evt.mousePosition, _panel);
					}
					else
					{
                        spline.SetVisible(false);
						if (spline.endNode != null)
						{
							(spline.endNode as NodeVisualElement).DisconnectInput();
						}
                    }
				}

				enabled = false;
			}

			if (wrongConnectionTypeLabel != null)
			{
				wrongConnectionTypeLabel.visible = false;
			}
		}

		void OnNodePanelClose()
		{
            spline.SetVisible(false);
        }

		public static bool InheritsFrom(System.Type type, System.Type baseType)
		{
			// null does not have base type
			if (type == null)
			{
				return false;
			}

			// only interface or object can have null base type
			if (baseType == null)
			{
				return type.IsInterface || type == typeof(object);
			}

			// check implemented interfaces
			if (baseType.IsInterface)
			{
				return type.GetInterfaces().Contains(baseType);
			}

			// check all base types
			var currentType = type;
			while (currentType != null)
			{
				if (currentType.BaseType == baseType)
				{
					return true;
				}

				currentType = currentType.BaseType;
			}

			return false;
		}

		private void MouseMoveHandler(MouseMoveEvent _evt)
		{
			if (enabled)
			{	
				spline.endPosition =  _evt.mousePosition;
				

				var _pickElement = target.panel.Pick(_evt.mousePosition);
				if (_pickElement == null)
					return;

				var _pickedNode = _pickElement.GetFirstOfType<NodeVisualElement>();
				if (currentMouseOverNodeElement != null)
				{
					currentMouseOverNodeElement.HighlightInputOff();
				}
				currentMouseOverNodeElement = _pickedNode;
				if (_pickedNode != null && _pickedNode != nodeElement as NodeVisualElement)
				{
					
					var _inputConnectionType = _pickedNode.nodeData.GetType().GetCustomAttribute<NodeInputConnectionType>();
					var _outputConnectionType = nodeData.GetType().GetCustomAttribute<NodeOutputConnectionType>();

					// Compare input and output types compatibility.
					var _outputCompatible = true;
					if (_outputConnectionType != null)
					{
						var _foundCompatibleOutputType = _outputConnectionType.types.Where(x => x == _pickedNode.nodeData.GetType()).FirstOrDefault();

						if (_foundCompatibleOutputType != null)
						{
							// output all good
						}
						else
						{
							var _found = false;
							foreach (var _t in _outputConnectionType.types)
							{
								_found = InheritsFrom(_pickedNode.nodeData.GetType(), _t);
								if (_found)
								{
									break;
								}
							}

							_outputCompatible = _found;
						}					
					}

					var _inputCompatible = true;
					if (_inputConnectionType != null)
					{
						var _foundCompatibleInputType = _inputConnectionType.types.Where(x => x == nodeData.GetType()).FirstOrDefault();
						
						if (_foundCompatibleInputType != null)
						{
							// input all good
						}
						else
						{
							var _found = false;
							foreach (var _t in _inputConnectionType.types)
							{
								_found = InheritsFrom(nodeData.GetType(), _t);
								if (_found)
								{
									break;
								}
							}

							_inputCompatible = _found;
						}
					}


					if (_inputCompatible && _outputCompatible)
					{
							if (wrongConnectionTypeLabel != null)
							{
								wrongConnectionTypeLabel.visible = false;
							}

							connectionTypeOk = true;
							spline.splineColor = splineOriginalColor;

							currentMouseOverNodeElement.HighlightInputOn();

					}
					else
					{
						if (wrongConnectionTypeLabel == null)
							{
								wrongConnectionTypeLabel = new Label();
								wrongConnectionTypeLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
								wrongConnectionTypeLabel.style.color = Color.red;
								nodeContainer.Add(wrongConnectionTypeLabel);
							}
							wrongConnectionTypeLabel.visible = true;
							wrongConnectionTypeLabel.text = "";
							if (!_inputCompatible)
							{
								for (int t = 0; t < _inputConnectionType.types.Length; t ++)
								{
									wrongConnectionTypeLabel.text += "Input != " + _inputConnectionType.types[t].Name + "\n";
								}
							}
							if (!_outputCompatible)
							{
								for (int t = 0; t < _outputConnectionType.types.Length; t ++)
								{
									wrongConnectionTypeLabel.text += "Output != " + _outputConnectionType.types[t].Name + "\n";
								}
							}
							wrongConnectionTypeLabel.style.position = Position.Absolute;
							wrongConnectionTypeLabel.transform.position = nodeContainer.WorldToLocal(new Vector2(_evt.mousePosition.x + 15, _evt.mousePosition.y));

							connectionTypeOk = false;
							spline.splineColor = Color.red;

							currentMouseOverNodeElement.HighlightInputOff();
					}
				}
				else
				{
					if (wrongConnectionTypeLabel != null)
					{
						wrongConnectionTypeLabel.visible = false;
					}
					connectionTypeOk = true;
					spline.splineColor = splineOriginalColor;

					if (currentMouseOverNodeElement != null)
					{
						currentMouseOverNodeElement.HighlightInputOff();
					}
				}
			}
		}

		private void MouseOverHandler(MouseOverEvent _evt)
		{
			// spline.SetColor(nodeData.color);

			if (spline.endNode == null)
			{
				slot.style.backgroundColor = Color.white;
			}
			else
			{
                slot.style.backgroundColor = new Color(230f / 255f,60f / 255f, 80f / 255f, 190f / 255f);
			}
        }


		private void MouseOutHandler(MouseOutEvent _evt)
		{
			//spline.SetColor(node.color.GetColor());
			if (spline.splineEndType == SplineElement.SplineEndType.cursor)
			{
				slot.style.backgroundColor = Color.black;
			}
			else
			{
				if (spline.endNode == null || (spline.endNode as NodeVisualElement).nodeData.isDeleted)
				{
					slot.style.backgroundColor = Color.black;
				}
				else
				{
					slot.style.backgroundColor = new Color(233f / 255f, 233f / 255f, 233f / 255f, 190f / 255f);
            	}
			}

			if( currentMouseOverNodeElement != null)
			{
				currentMouseOverNodeElement.HighlightInputOff();
			}
		}


		private void MouseCaptureOutHandler(MouseCaptureOutEvent _evt)
		{
			if (enabled)
			{
				target.ReleaseMouse();
				_evt.StopImmediatePropagation();

				enabled = false;
			}
		}
	}
}
#endif