/*  
 *  DATABRAIN | Logic add-on
 *  (c) 2023 by Giant Grey / www.giantgrey.com
 *  Author: Marc Egli
 *  
 */
using Databrain.Attributes;
using Databrain.Blackboard;
using Databrain.Logic.Attributes;
using UnityEngine;

namespace Databrain.Logic
{
    [NodeTitle("Return Value")]
    [NodeCategory("Logic")]
    [NodeColor("#EC695B")]
    [NodeIcon("check")]
    [NodeDescription("Return result value which can be obtained on the OnComplete event of a graph.")]
    public class ReturnValue : NodeData
    {
        [NodeColorField]
        public Color nodeColor;

        [Title("Values")]
        [HorizontalLine(2, DatabrainColor.Grey)]
        public bool returnBool;
        public int returnInt;
        public float returnFloat;
        public string returnString;
        public Vector2 returnVector2;
        public Vector3 returnVector3;


        public override void ExecuteNode()
        {
            graphData.SetResult(new GraphData.GraphProcessingResult(
                returnBool,
                returnFloat,
                returnInt,
                returnString,
                returnVector2,
                returnVector3));


            ExecuteNextNode(0);

            graphData.isRunning = false;

        }

      
    }
}