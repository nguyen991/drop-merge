/*
 *	DATABRAIN | Logic
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Databrain.Logic.Attributes;
using System.Reflection;

namespace Databrain.Logic.Utils
{
    public class LogicCollectNodes
    {
        public static NodeCategoryTree collectedNodes = new NodeCategoryTree();

        public static NodeCategoryTree CollectNodes(string _filter = "", System.Type[] _supportedTypes = null)
        {
            collectedNodes = new NodeCategoryTree();

            var _allNodeTypes = TypeCache.GetTypesDerivedFrom(typeof(Databrain.Logic.NodeData));
            var availableNodes = _allNodeTypes.ToList();
            // var availableNodes = _allNodeTypes.Where(x => x.Namespace != "Databrain.Dialogue").ToList();
            //var availableNodes = _allNodeTypes.Where(x => x.Namespace == "Databrain.Logic").ToList();


            List<NodeCategoryTree.NodeInfo> collected = new List<NodeCategoryTree.NodeInfo>();

            for (int i = 0; i < availableNodes.Count; i++)
            {
                var _hideNodeAttribute = availableNodes[i].GetCustomAttribute<HideNode>(false);
                if (_hideNodeAttribute != null)
                    continue;

                var _supportedType = true;
                if (_supportedTypes != null)
                {
                    _supportedType = false;
                    for (int t = 0; t < _supportedTypes.Length; t ++)
                    {
                        if (availableNodes[i] == _supportedTypes[t])
                        { 
                            _supportedType = true;
                        }
                        if (availableNodes[i].BaseType == _supportedTypes[t])
                        {
                            _supportedType = true;
                        }
                    }
                } 

                var _nodeInfo = GatherNodeData(availableNodes[i], _filter);
                if (_nodeInfo != null && _supportedType)
                {
                    collected.Add(_nodeInfo);
                }
            }

            // Sort by categories
            collected = collected.OrderBy(c => c.category).ToList();


            for (int c = 0; c < collected.Count; c++)
            {
                var _child = collectedNodes.BuildTree(collected[c].category, collected[c].title);

                _child.AddNode
                (
                    collected[c].title,
                    collected[c].nodeType,
                    collected[c].category,
                    collected[c].order
                );

            }

            return collectedNodes;
        }


        private static NodeCategoryTree.NodeInfo GatherNodeData(System.Type _nodeType, string _filter = "")
        {
            NodeCategoryTree.NodeInfo collected = new NodeCategoryTree.NodeInfo();

            System.Type _found = _nodeType;

            var _nodeCategory = System.Attribute.GetCustomAttribute(_found, typeof(NodeCategory)) as NodeCategory;
            var _nodeTitle = System.Attribute.GetCustomAttribute(_found, typeof(NodeTitle)) as NodeTitle;

            bool _useFilter = false;
            bool _matchFilter = false;
            if (!string.IsNullOrEmpty(_filter))
            {
                // split filter in to several search keywords
                string[] _parts = _filter.Split(',')
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToArray();

                _useFilter = true;

                for (int p = 0; p < _parts.Length; p++)
                {
                    // search for type name
                    if (_found.Name.ToLower().Contains(_parts[p].ToLower()))
                    {
                        _matchFilter = true;
                    }

                    // search for title
                    if (_nodeTitle != null)
                    {
                        if (!string.IsNullOrEmpty(_nodeTitle.title))
                        {
                            if (_nodeTitle.title.ToLower().Contains(_parts[p].ToLower()))
                            {
                                _matchFilter = true;
                            }
                        }
                    }

                    // search for category
                    if (_nodeCategory != null)
                    {
                        if (!string.IsNullOrEmpty(_nodeTitle.title))
                        {
                            if (_nodeCategory.category.ToLower().Contains(_parts[p].ToLower()))
                            {
                                _matchFilter = true;
                            }
                        }
                    }
                }
            }

            collected = new NodeCategoryTree.NodeInfo
            (
                _nodeTitle == null ? _found.Name : _nodeTitle.title,
                _found,
                _nodeCategory == null ? "" : _nodeCategory.category,
                _nodeCategory == null ? 0 :_nodeCategory.order
            );

            // are we using a filter?
            if (_useFilter)
            {
                // does the filter match?
                if (_matchFilter)
                {
                    return collected;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return collected;
            }

        }
    }
}
#endif