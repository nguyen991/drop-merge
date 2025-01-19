/*  
 *  DATABRAIN | Logic add-on
 *  (c) 2023 by Giant Grey / www.giantgrey.com
 *  Author: Marc Egli
 *  
 */
using Databrain.Attributes;
using Databrain.Logic.Attributes;


namespace Databrain.Logic
{
    [NodeTitle("ExecuteGraph")]
    [NodeCategory("Logic")]
    [NodeOutputs(new string[] {"True", "False"})]
    [NodeDescription("Executes another graph and waits for completion. Executes true or false depending on the returning boolean value.")]
    [NodeColor(DatabrainColor.LightGrey)]
    public class ExecuteGraph : NodeData
    {
        [DataObjectDropdown(true)]
        public LogicGraph graph;

        public override void ExecuteNode()
        {
            ///////////////////


            graph.ExecuteGraph(graphData.ExposedPropertyTable).OnComplete(x =>
            {
                if (x.boolValue)
                {
                    ExecuteNextNode(0);
                }
                else
                {
                    ExecuteNextNode(1);
                }
            });
            
        }   
    }
}