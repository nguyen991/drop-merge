 /*
 *	DATABRAIN - Logic FSM Nodes
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 */
using System.Collections.Generic;
using Databrain.Helpers;
using Databrain.Logic.Attributes;
using UnityEngine;
using UnityEngine.UIElements;


namespace Databrain.Logic.StateMachine
{
    [NodeTitle("Is State")]
    [NodeCategory("StateMachine")]
    [NodeDescription("Check current running state and returns true or false")]
    [NodeOutputs(new string[] {"Yes", "No"})]
    [NodeColor("#E1C9A7")]
    [NodeIcon("checkState.png", "LogicResPath.cs")]
    public class IsState : NodeData
    {   
        [NodeHideVariable]
        public string checkForState;
        [NodeHideVariable]
        public int selectedStateIndex;

        [NodeHideVariable]
        [SerializeReference]
        public StateMachineController stateMachineNode;
        [SerializeField]
        private int selectedIndex;

        #if UNITY_EDITOR
            DropdownField dropdownControllers;

            DropdownField dropdownStates;

            Label stateLabel;
        #endif

        
        public override void ExecuteNode()
        {
            if (stateMachineNode.currentState == checkForState)
            {
                ExecuteNextNode(0);
            }
            else
            {
                ExecuteNextNode(1);
            }
        }

#if UNITY_EDITOR
        public override VisualElement CustomNodeGUI()
        {
            if (stateLabel == null)
            {
                stateLabel = new Label();
                stateLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                DatabrainHelpers.SetMargin(stateLabel, 5, 5, 5, 5);
            }

            stateLabel.text = "Is State: " + checkForState;

            return stateLabel;
        }


        public override VisualElement CustomGUI()
        {
            return GUI();
        }


        VisualElement GUI()
        {
            var _root = new VisualElement();

            List<string> _choices = new List<string>();
            List<NodeData> _nodes = new List<NodeData>();
            List<string> _states = new List<string>();

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



                if (stateMachineNode != null)
                {
                    _states = new List<string>();

                    for (int o = 0; o < stateMachineNode.outputs.Count; o ++)
                    {
                        _states.Add(stateMachineNode.outputs[o]);   
                    }

                    if (_states.Count > 0)
                    {
                        dropdownStates.choices = _states;

                        dropdownStates.SetEnabled(true);
                    }
                    else
                    {
                        dropdownStates.SetEnabled(false);
                    }
                }

            });


            if (dropdownStates == null)
            {
            
                if (stateMachineNode != null)
                {
                    _states = new List<string>();
                    
                    for (int o = 0; o < stateMachineNode.outputs.Count; o ++)
                    {
                        _states.Add(stateMachineNode.outputs[o]);   
                    }
                }

                dropdownStates = new DropdownField(_states, selectedStateIndex);
            }
            

            dropdownStates.RegisterValueChangedCallback(change => 
            {
                selectedStateIndex = dropdownStates.index;
                checkForState = change.newValue;
            });

    
            _root.Add(new Label("State Machine Controller"));
            _root.Add(dropdownControllers);
            _root.Add(new Label("Is State"));
            _root.Add(dropdownStates);

            return _root;
        }
#endif
    }
}
