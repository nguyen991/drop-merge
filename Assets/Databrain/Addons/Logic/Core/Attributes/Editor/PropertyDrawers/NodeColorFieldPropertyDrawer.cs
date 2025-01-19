/*
 *	DATABRAIN | Logic
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;


namespace Databrain.Logic.Attributes
{
    [CustomPropertyDrawer(typeof(NodeColorFieldAttribute))]
    public class NodeColorFieldPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var _root = new VisualElement();

            var _attribute = (NodeColorFieldAttribute)attribute;

            var _propField = new ColorField();
            _propField.BindProperty(property);
            _propField.label = property.displayName;

            _propField.RegisterValueChangedCallback(x =>
            {
                if (x.newValue != x.previousValue)
                {
                    var _node = (property.serializedObject.targetObject as NodeData);

                    _node.SetColor(x.newValue);
                }
            });

            _root.Add(_propField);


            return _root;
        }

    }
}
#endif