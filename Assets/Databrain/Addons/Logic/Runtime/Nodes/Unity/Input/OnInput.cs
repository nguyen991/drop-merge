/*  
 *  DATABRAIN | Logic add-on
 *  (c) 2023 by Giant Grey / www.giantgrey.com
 *  Author: Marc Egli
 *  
 */
#if ENABLE_INPUT_SYSTEM
using Databrain.Attributes;
using Databrain.Logic.Attributes;
using UnityEngine.InputSystem;

namespace Databrain.Logic
{
    [NodeTitle("OnInput")]
    [NodeCategory("Unity/Input")]
    [NodeIcon("gamepad")]
    [NodeOutputs(new string[] {"Next"})]
    [NodeDescription("Wait for an input action")]
    [NodeColor(DatabrainColor.Gold)]
    [NodeNotConnectable]
    public class OnInput : NodeData
    {
        public enum ActionState
        {
            performed,
            started,
            canceled
        }

        public ActionState state;

        public InputAction inputAction;

        public override void InitNode()
        {
            inputAction.Enable();

            inputAction.performed -= Performed;
            inputAction.started -= Started;
            inputAction.canceled -= Canceled;

            inputAction.performed += Performed;
            inputAction.started += Started;
            inputAction.canceled += Canceled;
        }

        void Started(InputAction.CallbackContext context)
        {
            if (state == ActionState.started)
            {
                HighlightNode();
                ExecuteNextNode(0);
            }
        }

        void Performed(InputAction.CallbackContext context)
        {
            if (state == ActionState.performed)
            {
                HighlightNode();
                ExecuteNextNode(0);
            }
        }

        void Canceled(InputAction.CallbackContext context)
        {
            if (state == ActionState.canceled)
            {
                HighlightNode();
                ExecuteNextNode(0);
            }
        }

        public override void ExecuteNode(){ }   
    }
}
#endif