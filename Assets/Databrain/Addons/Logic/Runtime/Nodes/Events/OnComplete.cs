using Databrain.Logic.Attributes;
using UnityEngine;

namespace Databrain.Logic
{
    [NodeTitle("On Complete")]
    [NodeCategory("Logic")]
    [NodeOutputs(new string[] {})]
    [NodeDescription("This is a node description")]
    [NodeColor("#EC695B")]
    
    public class OnComplete : NodeData
    {

        public override void ExecuteNode()
        {
            ///////////////////
            ExecuteNextNode(0);

            graphData.isRunning = false;
        }   
    }
}