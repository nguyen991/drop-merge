#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


namespace Databrain.Attributes
{

    [CustomPropertyDrawer(typeof(RequireInterfaceAttribute))]
    public class RequireInterfacePropertyDrawer : PropertyDrawer
    {

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var _root = new VisualElement();

             // Check if this is reference type property.
            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                var requiredAttribute = this.attribute as RequireInterfaceAttribute;

                var _prop = new ObjectField();
                _prop.label = property.displayName;
                _prop.allowSceneObjects = true;
                _prop.value = property.objectReferenceValue;
                _prop.BindProperty(property);
                _prop.RegisterCallback<ChangeEvent<Object>>(x => 
                {
                    if (property.objectReferenceValue != x.newValue)
                    {
                        var _hasComponent = (x.newValue as GameObject).GetComponent(requiredAttribute.requiredType);

                        if (_hasComponent == null)
                        {
                            property.objectReferenceValue = null;
                            property.serializedObject.Update();
                            property.serializedObject.ApplyModifiedProperties();
                        }
                    }
                });
                
                _root.Add(_prop);
            }
            else
            {
                // If field is not reference, show error message.
                var _label = new Label();
                _label.text = "Property is not a reference type";

                _root.Add(_label);
            }

            return _root;
        }
    }
}
#endif