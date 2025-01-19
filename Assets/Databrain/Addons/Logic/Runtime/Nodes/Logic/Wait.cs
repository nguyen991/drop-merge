/*  
 *  DATABRAIN | Logic add-on
 *  (c) 2023 by Giant Grey / www.giantgrey.com
 *  Author: Marc Egli
 *  
 */
using UnityEngine;
using Databrain.Attributes;
using Databrain.Logic.Attributes;
using Databrain.Logic.Utils;
using System.Collections;

namespace Databrain.Logic
{
    [NodeTitle("Wait")]
    [NodeCategory("Logic")]
    [NodeIcon("hourglass")]
    [NodeOutputs(new string[] { "Next" })]
    [NodeColor(DatabrainColor.Blue)]
    [NodeDescription("Wait for seconds before executing next node")]
    public class Wait : NodeData
    {
        public float seconds;

        public bool unscaledSeconds;
        
        public async override void ExecuteNode()
        {
            var _wait = new AsyncHelper();
            await _wait.WaitForSeconds(seconds, unscaledSeconds);
 
            ExecuteNextNode(0);
        }
    }
}