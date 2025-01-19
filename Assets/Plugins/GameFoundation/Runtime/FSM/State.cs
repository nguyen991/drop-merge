using System;
using System.Collections.Generic;

namespace GameFoundation.FSM
{
    public class State
    {
        public StateMachine.PendingChangeStateData PendingChangeState { get; protected set; } =
            null;

        public virtual void OnEnter(string previousState, object data) { }

        public virtual void OnExit(string nextState) { }

        public virtual void OnUpdate() { }

        public virtual void OnDestroy() { }

        protected Dictionary<string, Delegate> actions = new();

        public State AddAction<TData>(string id, Action<TData> action)
        {
            actions.Add(id, action);
            return this;
        }

        public State AddAction(string id, Action action)
        {
            actions.Add(id, action);
            return this;
        }

        public virtual void OnAction<TData>(string id, TData data)
        {
            if (actions.ContainsKey(id))
            {
                actions[id].DynamicInvoke(data);
            }
        }

        public virtual void OnAction(string id)
        {
            if (actions.ContainsKey(id))
            {
                actions[id].DynamicInvoke();
            }
        }

        public virtual void RequestChangeState(string id, object data = null)
        {
            if (CanChangeState(id, data))
            {
                PendingChangeState ??= new StateMachine.PendingChangeStateData
                {
                    id = id,
                    data = data,
                };
            }
        }

        public virtual void ClearPendingState()
        {
            PendingChangeState = null;
        }

        public virtual bool CanChangeState(string id, object data = null)
        {
            return true;
        }
    }
}
