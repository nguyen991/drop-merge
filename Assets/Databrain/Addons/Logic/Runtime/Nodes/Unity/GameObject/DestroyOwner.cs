using Databrain.Attributes;
using Databrain.Logic.Attributes;
using UnityEngine;

namespace Databrain.Logic
{
    [NodeTitle("Destroy Owner")]
    [NodeCategory("Unity/GameObject")]
    [NodeOutputs(new string[] {"Next"})]
    [NodeDescription("Destroys the owner object of this graph")]
    [NodeIcon("bomb")]
    public class DestroyOwner : NodeData
    {

        public override void ExecuteNode()
        {
            ///////////////////

            Destroy(graphData.graphOwner);


            ExecuteNextNode(0);
        }   
    }
}