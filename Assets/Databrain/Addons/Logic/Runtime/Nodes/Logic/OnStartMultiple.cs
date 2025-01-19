using Databrain.Logic.Attributes;
using UnityEngine;

namespace Databrain.Logic
{
    [NodeTitle("OnStartMultiple")]
    [NodeCategory("Logic")]
    [NodeOutputs(new string[] {"Start"})]
    [NodeDescription("Start multiple flows")]
    [NodeColor("#76FFAE")]
    [NodeNotConnectable]
    [NodeIcon("start")]
    [NodeAddOutputsUI]
    public class OnStartMultiple : NodeData
    {

        public override void ExecuteNode()
        {
            ///////////////////
            for (int i = 0; i < outputs.Count; i ++)
            {
                ExecuteNextNode(i);
            }
        }   
    }
}