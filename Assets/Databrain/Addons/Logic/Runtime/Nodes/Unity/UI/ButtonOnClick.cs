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
    [NodeTitle("Button OnClick")]
    [NodeCategory("Unity/UI")]
    [NodeOutputs(new string[] {"Next"})]
    [NodeDescription("Listen to a UI Button click")]
    [NodeNotConnectable]
    public class ButtonOnClick : NodeData
    {

        [DataObjectDropdown(true, sceneComponentType: typeof(UnityEngine.UI.Button))]
        public SceneComponent button;

        public override void InitNode()
        {
            if (button != null)
            {
                var _image = button.GetReference<UnityEngine.UI.Button>(this);

                _image.onClick.AddListener(() =>
                {
                    ExecuteNextNode(0);
                });
            }
            else
            {
                Debug.LogWarning("Logic - Warning no button component assigned to node: " + this.title);
            }
        }

        public override void ExecuteNode(){}   
    }
}