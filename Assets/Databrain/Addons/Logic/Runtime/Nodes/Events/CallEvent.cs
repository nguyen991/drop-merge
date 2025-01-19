/*  
 *  DATABRAIN | Logic add-on
 *  (c) 2023 by Giant Grey / www.giantgrey.com
 *  Author: Marc Egli
 *  
 */
using Databrain.Attributes;
using Databrain.Events;
using Databrain.Logic.Attributes;
using UnityEngine;

namespace Databrain.Logic
{
    [NodeTitle("Call Event")]
    [NodeCategory("Databrain/Events")]
    [NodeOutputs(new string[] {"Next"})]
    [NodeDescription("Call a global Databrain event")]
    [NodeColor(DatabrainColor.Gold)]
    [NodeIcon("event")]
    public class CallEvent : NodeData
    {

        [DataObjectDropdown(true)]
        public DatabrainEvent databrainEvent;


        public override void ExecuteNode()
        {
            ///////////////////
            databrainEvent?.Raise();

            ExecuteNextNode(0);
        }   
    }
}