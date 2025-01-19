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
    [NodeTitle("Switch State")]
    [NodeCategory("StateMachine")]
    [NodeDescription("Change state on selected state machine controller")]
    [NodeColor("#D5A771")]
    [NodeOutputs(new string[]{"Next"})]
    [NodeIcon("switchState.png", "LogicResPath.cs")]
    public class SwitchState : NodeData
    {
        [NodeHideVariable]
        public string newState;
        [NodeHideVariable]
        [SerializeReference]
        public StateMachineController stateMachineNode;
        
        [SerializeField]  
        private int selectedIndex;
        [SerializeField]  
        private int selectedStateIndex;

        [SerializeField]
        [Tooltip("Only change to new state if current state = currentState")]
        private bool changeIfCurrentStateIs;
        [SerializeField]
        private string currentState;
        [SerializeField]
        private int selectedCurrentStateIndex;

#if UNITY_EDITOR
        DropdownField dropdownControllers;
        DropdownField dropdownStates;
        DropdownField dropdownStatesCurrent;
        Label stateLabel;
        List<NodeData> nodes;
#endif

        public override void ExecuteNode()
        {
            ///////////////////
            var _runtimeNode = graphData.GetRuntimeNode(stateMachineNode);
            if (!changeIfCurrentStateIs)
            {
                (_runtimeNode as StateMachineController).ChangeState(newState);
            }
            else
            {
                if ((_runtimeNode as StateMachineController).currentState == stateMachineNode.outputs[selectedCurrentStateIndex])
                {
                    (_runtimeNode as StateMachineController).ChangeState(newState);
                }
            }

            ExecuteNextNode(0);
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

            stateLabel.text = "To State: " + newState;

            return stateLabel;
        }
        public override VisualElement CustomGUI()
        {

            return GUI();
        }


        void UpdateAvailableStateMachineControllers()
        {
            List<string> _choices = new List<string>();
            nodes = new List<NodeData>();

            _choices.Add("- none -");
            nodes.Add(null);

            for (int i = 0; i < graphData.nodes.Count; i++)
            {
                if (graphData.nodes[i] == null)
                    continue;

                if (graphData.nodes[i].GetType() == typeof(StateMachineController))
                {
                    _choices.Add(graphData.nodes[i].title + " " + graphData.nodes[i].userTitle);
                    nodes.Add(graphData.nodes[i]);
                }
            }

            if (dropdownControllers == null)
            {
                dropdownControllers = new DropdownField(_choices, selectedIndex);  
            }
            else
            {
                dropdownControllers.choices = _choices;
            }

            if (selectedIndex < _choices.Count)
            {
                dropdownControllers.value = _choices[selectedIndex];
            }
            else
            {
                dropdownControllers.value = _choices[0];
            }
        }
        
        VisualElement GUI()
        {
            var _root = new VisualElement();

            List<string> _states = new List<string>();

            // var _nextState = new TextField();
            // _nextState.label = "To next state";
            // _nextState.bindingPath = "newState";
            UpdateAvailableStateMachineControllers();


            dropdownControllers.RegisterValueChangedCallback(change =>
            {
                selectedIndex = dropdownControllers.index;
                stateMachineNode = nodes[selectedIndex] as StateMachineController;

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
                        dropdownStatesCurrent.choices = _states;

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
                dropdownStatesCurrent = new DropdownField(_states, selectedCurrentStateIndex);
            }

            if (!string.IsNullOrEmpty(newState))
            {
                dropdownStates.value = newState;
            }

            if (!string.IsNullOrEmpty(currentState))
            {
                dropdownStatesCurrent.value = currentState;
            }

            dropdownStates.RegisterValueChangedCallback(change => 
            {
                newState = change.newValue;
                selectedStateIndex = dropdownStates.index;
                
                stateLabel.text = "To State: " + newState;
            });

            if (stateMachineNode != null || _states.Count > 0)
            {
                dropdownStates.SetEnabled(true);
            }
            else
            {
                dropdownStates.SetEnabled(false);
            }

            dropdownStatesCurrent.RegisterValueChangedCallback(change => 
            {
                currentState =change.newValue;
                selectedCurrentStateIndex = dropdownStatesCurrent.index;
            });

            var _switchContainer = new VisualElement();
            _switchContainer.style.marginTop = 10;
            DatabrainHelpers.SetBorder(_switchContainer, 1);

            var _onlySwitchWhen = new Toggle();
            _onlySwitchWhen.label = "Only switch if current state is:";
            _onlySwitchWhen.bindingPath = nameof(changeIfCurrentStateIs);
            _onlySwitchWhen.RegisterValueChangedCallback(change => 
            {
                dropdownStatesCurrent.SetEnabled(change.newValue);
            });
            
            _switchContainer.Add(_onlySwitchWhen);
            _switchContainer.Add(dropdownStatesCurrent);

            _root.Add(new Label("State Machine Controller"));
            _root.Add(dropdownControllers);
            _root.Add(new Label("Switch To State"));
            _root.Add(dropdownStates);
            _root.Add(_switchContainer);

            return _root;
        }
#endif

    }
}