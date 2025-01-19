/*  
 *  DATABRAIN | Logic add-on
 *  (c) 2023 by Giant Grey / www.giantgrey.com
 *  Author: Marc Egli
 *  
 */

using UnityEngine;
using Databrain.Attributes;
using Databrain.Logic.Attributes;


namespace Databrain.Logic
{
    [NodeTitle("Debug.Log")]
    [NodeOutputs(new string[] { "Next" })]
    [NodeCategory("Unity")]
    [NodeDescription("Log a message to the console")]
    [NodeIcon("bug")]
    public class DebugLog : NodeData
    {
        [ExposeToInspector]
        public string message;
        

        public override void ExecuteNode()
        {  
            Debug.Log(message);
            ExecuteNextNode(0);
        }
       
    }
}