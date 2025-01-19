/*  
 *  DATABRAIN | Logic add-on
 *  (c) 2023 by Giant Grey / www.giantgrey.com
 *  Author: Marc Egli
 *  
 */
#if ENABLE_INPUT_SYSTEM
using Databrain.Logic.Attributes;
using UnityEngine.InputSystem;

namespace Databrain.Logic
{
    [NodeTitle("OnDeviceChange")]
    [NodeCategory("Unity/Input")]
    [NodeOutputs(new string[] {"Next"})]
    [NodeDescription("This is a node description")]
    [NodeNotConnectable]
    public class OnDeviceChange : NodeData
    {

        public InputDeviceChange listenTo;

        public override void InitNode()
        {
            InputSystem.onDeviceChange +=
            (device, change) =>
            {
                switch (change)
                {
                    case InputDeviceChange.Added:
                        // New Device
                        if (listenTo == InputDeviceChange.Added)
                        {
                            // Execute next node
                            ExecuteNextNode(0);
                        }

                        break;
                    case InputDeviceChange.Disconnected:
                        // Device got unplugged
                        if (listenTo == InputDeviceChange.Disconnected)
                        {
                            // Execute next node
                            ExecuteNextNode(0);
                        }

                        break;
                    case InputDeviceChange.Reconnected:
                        // Plugged back in
                        if (listenTo == InputDeviceChange.Reconnected)
                        {
                            // Execute next node
                            ExecuteNextNode(0);
                        }

                        break;
                    case InputDeviceChange.Removed:
                        // Remove from Input System entirely; by default, Devices stay in the system once discovered.
                        if (listenTo == InputDeviceChange.Removed)
                        {
                            // Execute next node
                            ExecuteNextNode(0);
                        }

                        break;
                    default:
                        break;
                }
            };
        }

        public override void ExecuteNode()
        {
        }   
    }
}
#endif