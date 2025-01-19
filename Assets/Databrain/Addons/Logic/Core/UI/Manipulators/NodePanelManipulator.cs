/*
 *	DATABRAIN | Logic
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using UnityEngine.UIElements;

using Databrain.Logic.Elements;

namespace Databrain.Logic.Manipulators
{
    public class NodePanelManipulator : Manipulator
    {
        private NodeEditor nodeEditor;

        public NodePanelManipulator(NodeEditor _nodeEditor)
        {
            nodeEditor = _nodeEditor;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(MouseDown);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(MouseDown);
        }

        void MouseDown(MouseDownEvent _evt)
        {
            if (_evt.target is NodeCanvasElement)
            {
                if (_evt.button == 1)
                {
                    // Open Node panel
                    var _panel = new NodePanel(nodeEditor);
                    NodePanel.ShowPanel(_evt.mousePosition, _panel);
                }
            }
        }
    }
}
#endif