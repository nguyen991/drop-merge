/*  
 *  DATABRAIN | Logic add-on
 *  (c) 2023 by Giant Grey / www.giantgrey.com
 *  Author: Marc Egli
 *  
 */
using Databrain.Attributes;
using Databrain.Logic.Attributes;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Databrain.Logic
{
    [NodeTitle("Button OnExit")]
    [NodeCategory("Unity/UI")]
    [NodeOutputs(new string[] {"Next"})]
    [NodeDescription("Listen to a UI Button pointer exit event")]
    [NodeNotConnectable]
    public class ButtonOnExit : NodeData
    {
        [DataObjectDropdown(true, sceneComponentType: typeof(UnityEngine.UI.Button))]
        public SceneComponent button;

        private EventTrigger eventTrigger;
        public override void InitNode()
        {
            var _button = button.GetReference<UnityEngine.UI.Button>(this);

            eventTrigger = _button.gameObject.GetComponent<EventTrigger>();

            if (eventTrigger == null)
            {
                eventTrigger = _button.gameObject.AddComponent<EventTrigger>();
            }

            AddEventTrigger(OnPointerEnter, EventTriggerType.PointerExit);
        }

        void OnPointerEnter()
        {
            ExecuteNextNode(0);
        }

        public override void ExecuteNode(){}


        private void AddEventTrigger(UnityAction action, EventTriggerType triggerType)
        {
            
            EventTrigger.TriggerEvent trigger = new EventTrigger.TriggerEvent();
            trigger.AddListener((eventData) => action());
           
            EventTrigger.Entry entry = new EventTrigger.Entry() { callback = trigger, eventID = triggerType };

            eventTrigger.triggers.Add(entry);
        }
    }
}