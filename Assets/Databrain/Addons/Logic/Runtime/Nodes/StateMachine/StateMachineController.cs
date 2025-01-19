/*
 *	DATABRAIN - Logic FSM Nodes
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 */
using System;
using System.Collections;
using Databrain.Logic.Attributes;
using UnityEngine;

namespace Databrain.Logic.StateMachine
{
    [NodeTitle("FSM Controller")]
    [NodeCategory("StateMachine")]
    [NodeOutputs(new string[] {"Idle"})]
    [NodeDescription("Main FSM controller which controls and run the current state node.")]
    [NodeAddOutputsUI]
    [NodeColor("#E1C9A7", new string[] {"", "#71D5BD", "", ""})]
    [NodeSize(260, 100)]
    [NodeIcon("fsmController.png", "LogicResPath.cs")]
    [Node3LinesSpline]
    [NodeOutputConnectionType(new Type[] {typeof(Actions)})]
    public class StateMachineController : NodeData
    {
        public string currentState;

        [Tooltip("0 = Every frame")]
        public float tickRate;

        public bool debugLog;

        private bool fsmActive;
        NodeData currentStateNodeData;

        public override void InitNode()
        {
            for (int i = 0; i < graphData.nodes.Count; i ++)
            {
                if (graphData.nodes[i].GetType().BaseType == typeof(StateMachineNode))
                {
                    (graphData.nodes[i] as StateMachineNode).OnStateNodeInit(this);
                }
            }

            for (int i = 0; i < graphData.nodes.Count; i ++)
            {
                if (graphData.nodes[i].GetType().BaseType == typeof(StateMachineActionNode))
                {
                    (graphData.nodes[i] as StateMachineActionNode).OnStateNodeInit(this);
                }
            }
        }

        public override void ExecuteNode()
        {
            if (tickRate < 0)
            {
                tickRate = 0;
            }

            if (debugLog)
            {
                Debug.Log("Start State Machine Controller");
            }

            // Execute first state
            ///////////////////
            if (string.IsNullOrEmpty(currentState))
            {
                currentState = outputs[0];
            }
            
            ExecuteNextNode(0, out currentStateNodeData);
            OnEnterState();
            
            if (!fsmActive)
            {
                graphData.logicController.StartCoroutine(StateLoop());
            }   
        }

        void OnEnterState()
        {
            var _nextStateNode = currentStateNodeData as StateMachineNode;
            if (_nextStateNode != null)
            { 
                _nextStateNode.OnEnter();
                graphData.AddHighlightingNode(this);             
            }
        }

        IEnumerator StateLoop()
        {
            fsmActive = true;
            while (fsmActive)
            {
                if ((currentStateNodeData as StateMachineNode) != null)
                {
                    (currentStateNodeData as StateMachineNode).OnUpdate();
                    graphData.AddHighlightingNode((currentStateNodeData as StateMachineNode));
                }

                if (tickRate > 0)
                {
                    yield return new WaitForSeconds(tickRate);
                }
                else
                {
                    // next frame
                    yield return null;
                }
            }
        }

        public void ChangeState(string _newState)
        {
            if (debugLog)
            {
                Debug.Log("State Machine Controller - Change state to: " + _newState);
            }

            

            var _stateNode = currentStateNodeData as StateMachineNode;
            if (_stateNode != null)
            {
                if (debugLog)
                {
                    Debug.Log("Exit state");
                }
                _stateNode.OnExit();
                graphData.RemoveHighlightingNode(_stateNode);
            }

            currentState = _newState;

            for (int i = 0; i < outputs.Count; i++)
            {
                if (outputs[i] == _newState)
                {
                    ExecuteNextNode(i, out currentStateNodeData);
                }
            }

            OnEnterState();
        }

        public void StopStateMachine()
        {
            fsmActive = false;
            graphData.RemoveHighlightingNode(this);
        }
    }
}