#if ENABLE_INPUT_SYSTEM
using System;
using Databrain.Logic.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Databrain.Logic.StateMachine
{
    [NodeTitle("WaitForInput")]
    [NodeCategory("StateMachine/States")]
    [NodeOutputs(new string[] {"On Input"})]
    [NodeDescription("Wait for an input")]
    [NodeColor("#C2FAFF", new string[]{"#74AFE0","","",""})]
    [NodeInputConnectionType(new Type[] {typeof(Actions)})]
    [NodeSize(260, 100)]
    [NodeIcon("gamepad")]
    public class WaitForInput : StateMachineActionNode
    {

        public enum ActionState
        {
            performed,
            started,
            canceled
        }

        public ActionState state;

        public InputAction inputAction;



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

        public override void OnEnter()
        {
            // On Enter
            inputAction.Enable();

            inputAction.performed -= Performed;
            inputAction.started -= Started;
            inputAction.canceled -= Canceled;

            inputAction.performed += Performed;
            inputAction.started += Started;
            inputAction.canceled += Canceled;
        }

        public override void OnExit()
        {
            // On Exit
            inputAction.Disable();

            inputAction.performed -= Performed;
            inputAction.started -= Started;
            inputAction.canceled -= Canceled;
        }

        public override void OnUpdate()
        {
            // On update is called by the state machine controller node on every tick
        }   
    }
}
#endif