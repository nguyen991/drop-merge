using Databrain.Logic.Attributes;
using UnityEngine;

namespace Databrain.Logic
{
    [NodeTitle("RandomSwitch")]
    [NodeCategory("Logic")]
    [NodeOutputs(new string[] {"A", "B"})]
    [NodeDescription("Randomly selects output A or B")]
    [NodeIcon("random")]
    public class RandomSwitch : NodeData
    {

        public override void ExecuteNode()
        {
            var _rnd = Random.Range(0f, 1f);
            if (_rnd <= 0.5f)
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