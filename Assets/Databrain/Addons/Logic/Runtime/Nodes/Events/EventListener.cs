/*  
 *  DATABRAIN | Logic add-on
 *  (c) 2023 by Giant Grey / www.giantgrey.com
 *  Author: Marc Egli
 *  
 */
using UnityEngine;

using Databrain.Logic.Attributes;
using Databrain.Events;
using Databrain.Attributes;

namespace Databrain.Logic
{
    [NodeOutputs(new string[] { "On Event" })]
    [NodeTitle("Event Listener")]
    [NodeCategory("Databrain/Events")]
    [NodeSize(200, 50)]
    [NodeIcon("listen")]
    [NodeColor(DatabrainColor.Gold)]
    public class EventListener : NodeData
    {
        [DataObjectDropdown(true)]
        public DatabrainEvent OnEvent;

        [InfoBox("If true, the listener will not execute again after the first time event. The event listener node must be 'reactivated' by executing the node again.", InfoBoxType.Normal)]
        public bool listenOnce;


        public void EventCalled()
        {
            if (listenOnce)
            {
                OnEvent.UnregisterListener(EventCalled);
            }
            
            // Enable the graph
            (graphData as GraphData).isRunning = true;

            HighlightNode();
            ExecuteNextNode(0);
            
        }

        public override void ExecuteNode()
        {
            if (OnEvent != null)
            {
                OnEvent.UnregisterListener(EventCalled);
                OnEvent.RegisterListener(EventCalled);
            }
            else
            {
                Debug.Log("Logic - Event not assigned to event node");
            }
        }
    }
}