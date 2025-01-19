/*  
 *  DATABRAIN | Logic add-on
 *  (c) 2024 by Giant Grey / www.giantgrey.com
 *  Author: Marc Egli
 *  
 */
using Databrain.Logic.Attributes;


namespace Databrain.Logic
{
    [NodeTitle("CheckInputSytem")]
    [NodeCategory("Unity/Legacy")]
    [NodeIcon("question")]
    [NodeOutputs(new string[] {"New Input System", "Legacy Input System"})]
    [NodeDescription("Check if Input System is new or legacy")]
    public class CheckInputSytem : NodeData
    {

        public override void ExecuteNode()
        {
            ///////////////////
            #if ENABLE_INPUT_SYSTEM
            ExecuteNextNode(0);
            #else
            ExecuteNextNode(1);
            #endif
        }   
    }
}