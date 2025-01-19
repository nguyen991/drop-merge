/*  
 *  DATABRAIN | Logic add-on
 *  (c) 2023 by Giant Grey / www.giantgrey.com
 *  Author: Marc Egli
 *  
 */
using Databrain.Attributes;
using Databrain.Logic.Attributes;
using UnityEngine;

namespace Databrain.Logic
{
    [NodeTitle("Set Active")]
    [NodeCategory("Unity/GameObject")]
    [NodeOutputs(new string[] { "Next" })]
    [NodeIcon("task")]
    //[NodeAddExposedReference("SetActive:GameObject", typeof(GameObject))]
    public class SetActive : NodeData
    {
        [DataObjectDropdown(true, sceneComponentType: typeof(GameObject))]
        public SceneComponent gameObject;

        [Border(1, DatabrainColor.Grey)]
        public bool flag = false;

        public override void ExecuteNode()
        {
            if (gameObject == null)
            {
                Debug.LogError("Logic: No gameObject assigned to SetActive node");
            }
            else
            {
                var _obj = gameObject.GetReference<GameObject>(this);
                if ( _obj != null)
                {
                    gameObject.GetReference<GameObject>(graphData).SetActive(flag);
                }
            }

            ExecuteNextNode(0);
        }
    }
}