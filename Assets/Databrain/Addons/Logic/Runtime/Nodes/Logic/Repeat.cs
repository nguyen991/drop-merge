using Databrain.Attributes;
using Databrain.Logic.Attributes;

namespace Databrain.Logic
{
    [NodeTitle("Repeat")]
    [NodeCategory("Logic")]
    [NodeOutputs(new string[] {"Finished", "Loop"})]
    [NodeDescription("Repeat next node. Useful for creating loops.")]
    [NodeColor(DatabrainColor.Blue)]
    [NodeIcon("refresh")]
    [NodeSize(200, 150)]
    public class Repeat : NodeData
    {
        [InfoBox("Repeat count. -1 = Infinite")]
        public int repeatCount = -1;

        private int currentCount = 0;


        public override void InitNode()
        {
            currentCount = 0;
        }

        public override void ExecuteNode()
        {
            ///////////////////

            if (repeatCount > -1)
            {
                if (currentCount < repeatCount)
                {
                    currentCount++;

                    // Loop
                    ExecuteNextNode(1);
                }
                else
                {
                    // Finished
                    ExecuteNextNode(0);
                }
            }
            else
            {
                // Loop
                ExecuteNextNode(1);
            }
        }   
    }
}