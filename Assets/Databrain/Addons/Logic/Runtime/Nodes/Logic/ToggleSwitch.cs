using Databrain.Attributes;
using Databrain.Blackboard;
using Databrain.Logic.Attributes;
using UnityEngine;

namespace Databrain.Logic
{
    [NodeTitle("Toggle Switch")]
    [NodeCategory("Logic")]
    [NodeOutputs(new string[] {"True", "False"})]
    [NodeDescription("Switch flow based on toggle")]
    public class ToggleSwitch : NodeData
    {

        [DataObjectDropdown]
        public BooleanVariable toggle;


        public override void ExecuteNode()
        {
            ///////////////////
            if (toggle != null)
            {
                if (toggle.Value)
                {
                    ExecuteNextNode(0);
                }
                else
                {
                    ExecuteNextNode(1);
                }
            }

        }   
    }
}