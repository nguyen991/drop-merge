/*
 *	DATABRAIN - Logic FSM Nodes
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 */
using System;
using Databrain.Logic.Attributes;

namespace Databrain.Logic.StateMachine
{
    [NodeTitle("Action Split")]
    [NodeCategory("StateMachine")]
    [NodeOutputs(new string[] {"OnEnter" , "OnUpdate", "OnExit"})]
    [NodeDescription("Splits state machine actions in to OnEnter, OnUpdate and OnExit. This can be used to connect conventional logic nodes.")]
    [NodeColor("#74AFE0", new string[]{"#74AFE0","","",""})] //#C2FAFF
    [NodeSize(200, 100)]
    [NodeIcon("actionSplit.png", "LogicResPath.cs")]
    [NodeInputConnectionType(new Type[] {typeof(Actions)})]
    public class ActionSplit : StateMachineActionNode
    {

        public override void OnEnter()
        {
            // On Enter
            ExecuteNextNode(0);
        }

        public override void OnExit()
        {
            // On Exit
            ExecuteNextNode(2);
        }

        public override void OnUpdate()
        {
            // On update is called by the state machine controller node on every tick
            ExecuteNextNode(1);
        }   
    }
}