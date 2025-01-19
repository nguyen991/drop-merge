using System;
using System.Collections.Generic;

namespace GameFoundation.FSM
{
    public class SubState : State
    {
        public StateMachine StateMachine { get; private set; }

        protected string startState;
        protected bool resetOnEnter;
        protected bool callOnExit;

        public SubState(
            StateMachine stateMachine,
            string startState,
            bool resetOnEnter = true,
            bool callOnExit = true
        )
        {
            StateMachine = stateMachine;
            this.startState = startState;
            this.resetOnEnter = resetOnEnter;
            this.callOnExit = callOnExit;
        }

        public override void OnEnter(string previousState, object data)
        {
            if (resetOnEnter || !StateMachine.IsStarted)
            {
                StateMachine.Start(startState);
            }
            else
            {
                StateMachine.Log($"[SubState]: {StateMachine.debugName} OnEnter");
                StateMachine.GetState().OnEnter(previousState, data);
            }
        }

        public override void OnExit(string nextState)
        {
            if (callOnExit)
            {
                StateMachine.Log($"[SubState]: {StateMachine.debugName} OnExit");
                StateMachine.GetState().OnExit(nextState);
            }
        }

        public override void OnUpdate()
        {
            StateMachine.OnUpdate();

            // check request change state
            if (
                StateMachine.GetState().PendingChangeState != null
                && !StateMachine.IsContainState(StateMachine.GetState().PendingChangeState.id)
            )
            {
                PendingChangeState = StateMachine.GetState().PendingChangeState;
                StateMachine.Log(
                    $"[SubState]: {StateMachine.debugName} RequestChangeState: {PendingChangeState.id}"
                );
            }
        }

        public override void OnDestroy()
        {
            StateMachine.OnDestroy();
        }

        public void AddState(string state, State handler)
        {
            StateMachine.AddState(state, handler);
        }

        public override void OnAction<TData>(string id, TData data)
        {
            base.OnAction(id, data);
            StateMachine.OnAction(id, data);
        }

        public override void OnAction(string id)
        {
            base.OnAction(id);
            StateMachine.OnAction(id);
        }

        public override void ClearPendingState()
        {
            base.ClearPendingState();
            StateMachine.GetState().ClearPendingState();
        }
    }
}
