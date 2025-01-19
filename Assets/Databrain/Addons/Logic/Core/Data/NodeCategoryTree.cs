/*
 *	DATABRAIN | Logic
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Databrain.Logic
{
    public class NodeCategoryTree
    {
        public class NodeInfo
        {
            public string title;
            public Type nodeType;
            public string category;
            public int order;

            public NodeInfo() { }
            public NodeInfo(string _title, Type _nodeType, string _category, int _order)
            {
                title = _title;
                nodeType = _nodeType;
                category = _category;
                order = _order;
            }
        }

        public Dictionary<string, NodeCategoryTree> categories = new Dictionary<string, NodeCategoryTree>();
        public List<NodeInfo> nodesInCategory = new List<NodeInfo>();
        public NodeCategoryTree parentGraphTree;

        public string Path { get; set; }
        public string CompletePath = "";
        public string Name;

        public NodeCategoryTree() { }

        public void AddNode(string _title, Type _nodeType, string _category, int _order)
        {
            if (nodesInCategory == null)
            {
                nodesInCategory = new List<NodeInfo>();
            }

            nodesInCategory.Add(new NodeInfo(_title, _nodeType, _category, _order));
        }

        public NodeInfo GetData(Type _nodeType)
        {
            NodeInfo _nodeInfo = null;
            TraverseData(_nodeType, out _nodeInfo);

            return _nodeInfo;
        }

        protected virtual void TraverseData(Type _nodeType, out NodeInfo _nodeInfo)
        {
            _nodeInfo = null;

            for (int n = 0; n < nodesInCategory.Count; n++)
            {
                if (nodesInCategory[n].nodeType == _nodeType)
                {
                    _nodeInfo = new NodeInfo
                    (
                        nodesInCategory[n].title,
                        nodesInCategory[n].nodeType,
                        nodesInCategory[n].category,
                        nodesInCategory[n].order
                    );
                }
            }

            if (_nodeInfo == null)
            {
                foreach (var tree in this.categories.Keys)
                {
                    if (_nodeInfo == null)
                    {
                        categories[tree].TraverseData(_nodeType, out _nodeInfo);
                    }
                }
            }
        }

        public void Traverse(Action<int, NodeCategoryTree> visitor)
        {
            this.traverse(-1, visitor);
        }

        protected virtual void traverse(int depth, Action<int, NodeCategoryTree> visitor)
        {
            visitor(depth, this);

            //if (depth > -1)
            //    return;

            foreach (var tree in this.categories.Keys)
                categories[tree].traverse(depth + 1, visitor);
        }

        public NodeCategoryTree BuildTree(string _path, string _name)
        {
            // Parse into a sequence of parts.
            string[] parts = _path.Split("/"[0]);

            // The current tree.  Start with this.
            NodeCategoryTree current = this;


            // Iterate through the parts.
            foreach (string part in parts)
            {
                // The child GraphTree.
                NodeCategoryTree child;

                // Does the part exist in the current GraphTree?  If
                // not, then add.
                if (!current.categories.TryGetValue(part, out child))
                {
                    var n = _name;

                    // Add the child.
                    child = new NodeCategoryTree
                    {
                        Path = part,
                        CompletePath = _path,
                        Name = _name,
                        parentGraphTree = current
                    };

                    // Add to the dictionary.
                    current.categories[part] = child;
                }
                

                // Set the current to the child.
                current = child;
            }

            return current;

        }
    }
}