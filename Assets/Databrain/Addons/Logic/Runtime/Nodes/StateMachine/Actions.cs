/*
 *	DATABRAIN - Logic FSM Nodes
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 */
using System;
using Databrain.Logic.Attributes;

namespace Databrain.Logic.StateMachine
{
    [NodeTitle("Actions")]
    [NodeCategory("StateMachine")]
    [NodeOutputs(new string[] {"Action"})]
    [NodeDescription("The actions node executes state machine action nodes. Each action node receives the OnEnter, OnUpdate and OnExit call.")]
    [NodeSize(200, 100)]
    [NodeColor("#E1C9A7", new string[]{"#71D5BD","#74AFE0","",""})]
    [NodeIcon("actions.png", "LogicResPath.cs")]
    [NodeAddOutputsUI]
    [NodeInputConnectionType(new Type[] {typeof(StateMachineController)})]
    [NodeOutputConnectionType(new Type[] {typeof(StateMachineActionNode)})]
    [Node3LinesSpline]
    public class Actions : StateMachineNode
    {

        public override void OnEnter()
        {
            for (int i = 0; i < connectedNodesOut.Count; i ++)
            {
                var _stateNode = connectedNodesOut[i] as StateMachineActionNode;
                if (_stateNode != null)
                {
                    _stateNode.OnEnter();
                    graphData.AddHighlightingNode(_stateNode);
                }
            }
        }

        public override void OnUpdate()
        {
            for (int i = 0; i < connectedNodesOut.Count; i ++)
            {
                var _stateNode = connectedNodesOut[i] as StateMachineActionNode;
                if (_stateNode != null)
                {
                    _stateNode.OnUpdate();
                    graphData.AddHighlightingNode(_stateNode);
                }
            }
        }

        public override void OnExit()
        {
            for (int i = 0; i < connectedNodesOut.Count; i ++)
            {
                var _stateNode = connectedNodesOut[i] as StateMachineActionNode;
                if (_stateNode != null)
                {
                    _stateNode.OnExit();
                    graphData.RemoveHighlightingNode(_stateNode);
                }
            }
        }

    }
}