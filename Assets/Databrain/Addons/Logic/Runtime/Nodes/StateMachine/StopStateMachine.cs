 /*
 *	DATABRAIN - Logic FSM Nodes
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 */
using System.Collections.Generic;
using Databrain.Logic.Attributes;
using UnityEngine;
using UnityEngine.UIElements;

namespace Databrain.Logic.StateMachine
{
    [NodeTitle("Stop StateMachine")]
    [NodeCategory("StateMachine")]
    [NodeDescription("Stop running state machine")]
    [NodeOutputs(new string[] {"Next"})]
    [NodeColor("#E07666")]
    [NodeIcon("stopStateMachine.png", "LogicResPath.cs")]
    public class StopStateMachine : NodeData
    {  
        [NodeHideVariable]
        [SerializeReference]
        public StateMachineController stateMachineNode;
        [SerializeField]
        private int selectedIndex;

        #if UNITY_EDITOR
        DropdownField dropdownControllers;
        
        #endif

        public override void ExecuteNode()
        {
            ///////////////////
            if (stateMachineNode == null)
            {
                List<NodeData> _nodes = new List<NodeData>();
                for (int i = 0; i < graphData.nodes.Count; i++)
                {
                    if (graphData.nodes[i].GetType() == typeof(StateMachineController))
                    {
                        _nodes.Add(graphData.nodes[i]);
                    }
                }

                try
                {
                    var _runtimeNode = graphData.GetRuntimeNode(_nodes[selectedIndex- 1]);
                    (_runtimeNode as StateMachineController).StopStateMachine();
                }catch
                {
                    Debug.LogWarning("Stop StateMachine error: No state machine defined in node");
                }
            
            }
            else
            {
                var _runtimeNode = graphData.GetRuntimeNode(stateMachineNode);

                (_runtimeNode as StateMachineController).StopStateMachine();
            }

            ExecuteNextNode(0);
        }


#if UNITY_EDITOR
        public override VisualElement CustomGUI()
        {
            return GUI();
        }

        VisualElement GUI()
        {
            var _root = new VisualElement();

            List<string> _choices = new List<string>();
            List<NodeData> _nodes = new List<NodeData>();


            _choices.Add("- none -");
            _nodes.Add(null);

            for (int i = 0; i < graphData.nodes.Count; i++)
            {
                if (graphData.nodes[i] == null)
                    continue;

                if (graphData.nodes[i].GetType() == typeof(StateMachineController))
                {
                    _choices.Add(graphData.nodes[i].title + " " + graphData.nodes[i].userTitle);
                    _nodes.Add(graphData.nodes[i]);
                }
            }


            if (dropdownControllers == null)
            {
                dropdownControllers = new DropdownField(_choices, selectedIndex);
            }

            if (selectedIndex < _choices.Count)
            {
                dropdownControllers.value = _choices[selectedIndex];
            }
            else
            {
                dropdownControllers.value = _choices[0];
            }

            dropdownControllers.RegisterValueChangedCallback(change =>
            {
                selectedIndex = dropdownControllers.index;
                stateMachineNode = _nodes[selectedIndex] as StateMachineController;
            });
       
            _root.Add(new Label("State Machine Controller"));
            _root.Add(dropdownControllers);

            return _root;
        }
#endif
    }
}