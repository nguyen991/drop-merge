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

    public class GroupResizeManipulator : Manipulator
    {
        private bool enabled;

        private VisualElement group;
        private VisualElement resizeDrag;

        public GroupResizeManipulator(VisualElement _group, VisualElement _target)
        {
            group = _group;
            resizeDrag = _target;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            resizeDrag.RegisterCallback<MouseDownEvent>(MouseDownHandler);
            resizeDrag.RegisterCallback<MouseUpEvent>(MouseUpHandler);
            resizeDrag.RegisterCallback<MouseMoveEvent>(MouseMoveHandler);
            resizeDrag.RegisterCallback<MouseCaptureOutEvent>(MouseCaptureOutHandler);
        }



        protected override void UnregisterCallbacksFromTarget()
        {
            resizeDrag.UnregisterCallback<MouseDownEvent>(MouseDownHandler);
            resizeDrag.UnregisterCallback<MouseUpEvent>(MouseUpHandler);
            resizeDrag.UnregisterCallback<MouseMoveEvent>(MouseMoveHandler);
            resizeDrag.UnregisterCallback<MouseCaptureOutEvent>(MouseCaptureOutHandler);
        }


        private void MouseDownHandler(MouseDownEvent evt)
        {
            if (evt.button == 0)
            {
                enabled = true;

                evt.StopPropagation();
                resizeDrag.CaptureMouse();
            }
        }

        private void MouseUpHandler(MouseUpEvent evt)
        {
            enabled = false;
            resizeDrag.ReleaseMouse();

            for (int i = 0; i < (group as GroupVisualElement).graphData.nodes.Count; i++)
            {
                var _node = (group as GroupVisualElement).graphData.nodes[i];

                //Debug.Log(group.transform.position + " _ " + group.contentRect + " _ " + _node.nodeVisualElement.transform.position);
                var _checkRect = new Rect(group.transform.position.x, group.transform.position.y, group.contentRect.width, group.contentRect.height);
                if (_checkRect.Contains(new Vector2(_node.nodeVisualElement.transform.position.x, _node.nodeVisualElement.transform.position.y)))
                {
                    // Add to group selection
                    if (!(group as GroupVisualElement).groupData.assignedNodes.Contains(_node))
                    {
                        (group as GroupVisualElement).groupData.assignedNodes.Add(_node);
                    }
                }
                else
                {
                    if ((group as GroupVisualElement).groupData.assignedNodes.Contains(_node))
                    {
                        (group as GroupVisualElement).groupData.assignedNodes.Remove(_node);
                    }
                }
            }

        }
        private void MouseMoveHandler(MouseMoveEvent evt)
        {
            if (enabled)
            {
                var _w = group.resolvedStyle.width;
                var _h = group.resolvedStyle.height;
                group.style.width = _w + evt.mouseDelta.x; //.current.delta.x * 0.5f;
                group.style.height = _h + evt.mouseDelta.y;

                (group as GroupVisualElement).groupData.size = new Vector2(group.resolvedStyle.width, group.resolvedStyle.height);

            }
        }

        private void MouseCaptureOutHandler(MouseCaptureOutEvent evt)
        {

        }

    }
}
#endif