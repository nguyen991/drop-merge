/*  
 *  DATABRAIN | Logic add-on
 *  (c) 2023 by Giant Grey / www.giantgrey.com
 *  Author: Marc Egli
 *  
 */
using System.Collections.Generic;
using Databrain.Attributes;
using Databrain.Logic.Attributes;
using UnityEngine;

namespace Databrain.Logic
{
    [NodeTitle("Graph EventListener")]
    [NodeCategory("Databrain/Events")]
    [NodeOutputs(new string[] {"On Event"})]
    [NodeDescription("Listens to a graph event")]
    [NodeIcon("listen")]
    [NodeColor(DatabrainColor.Gold)]
    public class GraphEventListener : NodeData
    {

        private List<string> availableEvents = new List<string>() { "A" };

        [Dropdown("availableEvents")]
        public string listenToEvent;


#if UNITY_EDITOR
        public override void EditorInitalize()
        {
            RefreshAvailableEvents();
        }
#endif

        public override void SelectNode()
        {
           RefreshAvailableEvents();
        }

        void RefreshAvailableEvents()
        {
            availableEvents = new List<string>();

            for (int i = 0; i < graphData.graphEvents.Count; i++)
            {
                availableEvents.Add(graphData.graphEvents[i].eventName);
            }
        }

        public void CallEvent(string _event)
        {
            if (listenToEvent.Equals(_event))
            {
                graphData.isRunning = true;
                HighlightNode();
                ExecuteNextNode(0);
            }
        }


        public override void ExecuteNode(){}

    }
}