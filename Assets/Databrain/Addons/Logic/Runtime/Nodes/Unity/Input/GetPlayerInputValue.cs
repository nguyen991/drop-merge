/*  
 *  DATABRAIN | Logic add-on
 *  (c) 2023 by Giant Grey / www.giantgrey.com
 *  Author: Marc Egli
 *  
 */
#if ENABLE_INPUT_SYSTEM
using Databrain.Attributes;
using Databrain.Blackboard;
using Databrain.Logic.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Databrain.Logic
{
    [NodeTitle("GetPlayerInputValue")]
    [NodeCategory("Unity/Input")]
    [NodeOutputs(new string[] {"Next"})]
    [NodeDescription("This is a node description")]
    public class GetPlayerInputValue : NodeData
    {
        [DataObjectDropdown(true, sceneComponentType: typeof(GameObject))]
        public SceneComponent playerObject;

        public InputActionReference reference;

        public BooleanVariable boolValue;
        public Vector3Variable vector3Value;
        public Vector3Variable vector2Value;
        public FloatVariable floatValue;
        public IntegerVariable intValue;


        private GameObject _playerObject;
        private PlayerInput _playerInput;

        public override void InitNode()
        {
            _playerObject = playerObject.GetReference<GameObject>(this);
            _playerInput = _playerObject.GetComponent<PlayerInput>();
        }
        public override void ExecuteNode()
        {
            // Enter code here
            ///////////////////
            if (_playerInput == null)
                return;


            var _action = _playerInput.actions[reference.action.name];


            switch (_action.expectedControlType)
            {
                case "Vector2":
                    var _value2 = _action.ReadValue<Vector2>();
                    vector2Value.Value = _value2;
                    break;
                case "Button":
                    var _triggered = _action.triggered;
                    boolValue.Value = _triggered;
                    break;
                case "Vector3":
                    var _value3 = _action.ReadValue<Vector3>();
                    vector3Value.Value = _value3;
                    break;
                case "TrackedDevicePosition":
                    var _valueT3 = _action.ReadValue<Vector3>();
                    vector3Value.Value = _valueT3;
                    break;
                //case "TrackedDeviceOrientation":
                //    var _valueQ = _action.ReadValue<Quaternion>();
                //    vector4Value.Value = new Vector4(_valueQ.x, _valueQ.y, _valueQ.z, _valueQ.w);
                //    break;
                case "Float":
                    var _valueF = _action.ReadValue<float>();
                    floatValue.Value = _valueF;
                    break;
            }



            ExecuteNextNode(0);
        }   
    }
}
#endif