using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFoundation.FSM
{
    public class StateMachine
    {
        public class PendingChangeStateData
        {
            public string id;
            public object data;
        }

        public string CurrentState { get; private set; } = null;
        public bool IsStarted => CurrentState != null;
        protected Dictionary<string, State> states;

        protected bool isDebugLog = false;
        public string debugName = "FSM";

        public StateMachine()
        {
            states = new Dictionary<string, State>();
        }

        public void Start(string startState)
        {
            Log($"[StateMachine]: {debugName} Start: {startState}");
            CurrentState = startState;
            states[CurrentState].OnEnter(null, null);
        }

        public void Reset()
        {
            if (CurrentState != null)
            {
                Log($"[StateMachine]: {debugName} Reset");
                states[CurrentState].OnExit(null);
                CurrentState = null;
            }
        }

        public void AddState(string state, State handler)
        {
            states.Add(state, handler);
        }

        public void OnAction<TData>(string id, TData data)
        {
            Log($"[StateMachine]: {debugName} OnAction: {id}");
            states[CurrentState].OnAction(id, data);
        }

        public void OnAction(string id)
        {
            Log($"[StateMachine]: {debugName} OnAction: {id}");
            states[CurrentState].OnAction(id);
        }

        public void OnUpdate()
        {
            // update current state
            states[CurrentState].OnUpdate();

            // check request change state
            if (states[CurrentState].PendingChangeState != null)
            {
                ChangeState(states[CurrentState].PendingChangeState);
            }
        }

        public void OnDestroy()
        {
            foreach (var state in states.Values)
            {
                state.OnDestroy();
            }
        }

        public void ChangeState(string id, object data = null)
        {
            if (!CurrentState.Equals(id) && states.ContainsKey(id))
            {
                Log($"[StateMachine]: {debugName} ChangeState: {CurrentState} -> {id}");
                states[CurrentState].ClearPendingState();
                states[CurrentState].OnExit(id);
                var previousState = CurrentState;
                CurrentState = id;
                states[CurrentState].OnEnter(previousState, data);
            }
        }

        protected void ChangeState(PendingChangeStateData data)
        {
            ChangeState(data.id, data.data);
        }

        public State GetState(string id = null)
        {
            return states[id ?? CurrentState];
        }

        public bool IsContainState(string id)
        {
            return states.ContainsKey(id);
        }

        public void SetDebugLog(bool value, string name = null)
        {
            isDebugLog = value;
            debugName = name ?? debugName;
        }

        public void Log(string message)
        {
            if (isDebugLog)
            {
                Debug.Log(message);
            }
        }
    }
}
