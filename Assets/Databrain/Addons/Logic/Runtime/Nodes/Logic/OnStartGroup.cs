using Databrain.Logic.Attributes;
using UnityEngine;

namespace Databrain.Logic
{
    [NodeTitle("On Start Group")]
    [NodeCategory("Logic")]
    [NodeOutputs(new string[] {"Next"})]
    [NodeDescription("Start the execution of selected group. Make sure group has either an OnStart or an OnStartGroup node.")]
    [NodeColor("#94BA8E")]
    [NodeIcon("onStartGroup")]
    [NodeNotConnectable]
    public class OnStartGroup : NodeData
    {

        public override void ExecuteNode()
        {
            ///////////////////

            ExecuteNextNode(0);
        }   
    }
}