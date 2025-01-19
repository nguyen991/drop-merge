using Databrain.Attributes;
using Databrain.Blackboard;
using Databrain.Logic.Attributes;
#if UNITY_EDITOR
using UnityEditor.UIElements;
#endif
using UnityEngine;
using UnityEngine.UIElements;

namespace Databrain.Logic
{
    [NodeTitle("Set Toggle")]
    [NodeCategory("Logic")]
    [NodeOutputs(new string[] {"Next"})]
    [NodeDescription("This is a node description")]
    public class SetToggle : NodeData
    {
        public enum SetToggleType
        {
            oppositeValue,
            value
        }

        [NodeHideVariable]
        public SetToggleType setType;

        [NodeHideVariable]
        public bool value;

        [DataObjectDropdown]
        [NodeHideVariable]
        public BooleanVariable setTo;

        public override void ExecuteNode()
        {
            ///////////////////
            switch (setType)
            {
                case SetToggleType.oppositeValue:
                    setTo.Value = !setTo.Value;
                    break;
                case SetToggleType.value:
                    setTo.Value = value;
                    break;
            }

            ExecuteNextNode(0);
        }

#if UNITY_EDITOR
        public override VisualElement CustomGUI()
        { 
            return Build();
        }

        VisualElement Build()
        {
            var _root = new VisualElement();

            var _valueProp = new PropertyField();
            _valueProp.bindingPath = "value";
            _valueProp.SetEnabled(setType == SetToggleType.oppositeValue ? false : true);

            var _typeProp = new EnumField();
            _typeProp.bindingPath = "setType";
            _typeProp.RegisterValueChangedCallback(x =>
            {
                Debug.Log("change");
                _valueProp.SetEnabled(setType == SetToggleType.oppositeValue ? false : true);
                Build();

            });

            var _setTo = new PropertyField();
            _setTo.bindingPath = "setTo";


            _root.Add(_typeProp);
            _root.Add(_valueProp);
            _root.Add(_setTo);

            return _root;
        }
#endif

    }
}