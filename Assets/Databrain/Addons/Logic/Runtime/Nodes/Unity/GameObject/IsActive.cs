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
    [NodeTitle("Is Active")]
    [NodeCategory("Unity/GameObject")]
    [NodeOutputs(new string[] { "Active", "Deactive" })]
    [NodeIcon("question")]
    [NodeDescription("Check if a game object is active or not")]
    [NodeSize(200, 50)]
    public class IsActive : NodeData
    {
        [DataObjectDropdown(true, sceneComponentType: typeof(GameObject))]
        public SceneComponent gameObject;

        public override void ExecuteNode()
        {
            if (gameObject.GetReference<GameObject>(this) == null)
            {
                Debug.Log("Object is not assigned as reference");
            }
            else
            {
                if (gameObject.GetReference<GameObject>(this).activeSelf)
                {
                    ExecuteNextNode(0);
                }
                else
                {
                    ExecuteNextNode(1);
                }
            }
        }
    }
}