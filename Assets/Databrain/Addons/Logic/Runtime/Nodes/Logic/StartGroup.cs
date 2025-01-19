using Databrain.Attributes;
using Databrain.Logic.Attributes;
using System.Linq;
using UnityEngine.UIElements;
using UnityEngine;

namespace Databrain.Logic
{
    [NodeTitle("Start Group")]
    [NodeCategory("Logic")]
    [NodeOutputs(new string[] { "Next" })]
    [NodeDescription("Start the execution of selected group. Make sure group has either an OnStart or an OnStartGroup node.")]
    [NodeColor("#94BA8E")]
    [NodeIcon("startGroup")]
    public class StartGroup : NodeData
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
                    graphData.groups[g].StartGroup();
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