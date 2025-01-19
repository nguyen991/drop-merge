/*  
 *  DATABRAIN | Logic add-on
 *  (c) 2023 by Giant Grey / www.giantgrey.com
 *  Author: Marc Egli
 *  
 */
using Databrain.Logic.Attributes;

namespace Databrain.Logic
{
    [NodeTitle("On Start")]
    [NodeCategory("Logic")]
    [NodeOutputs(new string[] { "Start" })]
    [NodeColor("#76FFAE")]
    [NodeNotConnectable]
    [NodeIcon("start")]
    [NodeDescription("This is where the graph starts.")]
    public class OnStart : NodeData
    {
       
        public override void ExecuteNode()
        {
            ExecuteNextNode(0);
        }    
    }
}