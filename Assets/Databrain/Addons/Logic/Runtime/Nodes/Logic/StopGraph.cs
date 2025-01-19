using Databrain.Logic.Attributes;
using UnityEngine;

namespace Databrain.Logic
{
    [NodeTitle("StopGraph")]
    [NodeCategory("Logic")]
    [NodeDescription("Stop the execution of this graph")]
    [NodeColor("#EC695B")]
    [NodeIcon("stop")]
    public class StopGraph : NodeData
    {

        public override void ExecuteNode()
        {
            ///////////////////
            graphData.isRunning = false;

        }   
    }
}