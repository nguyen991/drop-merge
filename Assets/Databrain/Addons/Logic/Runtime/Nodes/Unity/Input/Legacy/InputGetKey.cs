
using Databrain.Attributes;
using Databrain.Logic.Attributes;
using Databrain.Logic.Utils;
using UnityEngine;

namespace Databrain.Logic
{

    [NodeTitle("InputGetKey")]
    [NodeCategory("Unity/Input/Legacy")]
    [NodeIcon("gamepad")]
    [NodeOutputs(new string[] { "Next" })]
    [NodeDescription("Listen to a input key. (Legacy input system)")]
    [NodeColor(DatabrainColor.Gold)]
    [NodeNotConnectable]
    public class InputGetKey : NodeData
    {
        #if ENABLE_INPUT_SYSTEM
        [InfoBox("New Input System is enabled, this node won't work with new Input System.", InfoBoxType.Warning)]
        #endif
        public KeyCode keyCode;

        public enum KeyState
        {
            pressed,
            down,
            up
        }

        public KeyState keyState;

      
        public override void InitNode()
        {
            ListenToKey();
        }

        async void ListenToKey()
        {
            var _wait = new AsyncHelper();
            await _wait.WaitForSeconds(0.2f);
        
            while (Application.isPlaying && graphData.isRunning)
            {
                #if ENABLE_LEGACY_INPUT_MANAGER
                switch (keyState)
                {
                    case KeyState.pressed:
                        if (Input.GetKey(keyCode))
                        {
                            HighlightNode();
                            ExecuteNextNode(0);
                        }
                    break;
                    case KeyState.down:
                        if (Input.GetKeyDown(keyCode))
                        {
                            HighlightNode();
                            ExecuteNextNode(0);
                        }
                    break;
                    case KeyState.up:
                        if (Input.GetKeyUp(keyCode))
                        {
                            HighlightNode();
                            ExecuteNextNode(0);
                        }
                    break;
                }
                #endif
                await _wait.WaitForFrame();
            }
        }

        public override void ExecuteNode(){}

    }

}
