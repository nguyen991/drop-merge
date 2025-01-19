using Databrain.Logic.Attributes;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Databrain.Logic
{
    [NodeTitle("Stop Group")]
    [NodeCategory("Logic")]
    [NodeOutputs(new string[] {"Next"})]
    [NodeDescription("Stop the execution of a flow inside a group")]
    [NodeColor("#EC695B")]
    [NodeIcon("stopGroup")]
    public class StopGroup : NodeData
    {
        [NodeHideVariable]
        public string groupName;

        public override void ExecuteNode()
        {
            ///////////////////
            for (int g = 0; g < graphData.groups.Count; g++)
            {
                if (graphData.groups[g].title == groupName)
                {
                    graphData.groups[g].StopGroup();
                }
            }


            ExecuteNextNode(0);
        }

#if UNITY_EDITOR
        public override VisualElement CustomGUI()
        {
            var _root = new VisualElement();


            var _options = graphData.groups.Select(x => x.title).ToList();
            var _dropdown = new DropdownField();
            _dropdown.label = "Group";
            _dropdown.choices = _options;
            _dropdown.value = groupName;
            _dropdown.RegisterValueChangedCallback(x =>
            {
                groupName = x.newValue;
            });

            _root.Add(_dropdown);

            return _root;
        }
#endif
    }
}