/*
 *	DATABRAIN | Logic
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
using Databrain.Logic.Attributes;
using System.Collections.Generic;
using UnityEngine;

namespace Databrain.Logic
{
    public class GroupData : ScriptableObject
    {
        [NodeHideVariable]
        public string guid = "";
        [NodeHideVariable]
        public string derivedClassName;
        [NodeHideVariable]
        public string title;
        [NodeHideVariable]
        public Vector2 position;
        [NodeHideVariable]
        public Vector2 worldPosition;
        [NodeHideVariable]
        public Color color;
        [NodeHideVariable]
        public DataLibrary relatedLibraryObject;
        [NodeHideVariable]
        public GraphData graphData;
        [NodeHideVariable]
        public Vector2 size;
        [NodeHideVariable]
        public Color groupColor = new Color(0f, 0f, 0f, 0f);

        [SerializeReference]
        public List<NodeData> assignedNodes = new List<NodeData>();

        /// <summary>
        /// Stops the running flow inside of this group
        /// </summary>
        public void StopGroup()
        {
            for (int i = 0; i < assignedNodes.Count; i++)
            {
                assignedNodes[i].canRun = false;
            }
        }

        /// <summary>
        /// Starts the flow of a group. Make sure group contains a start node
        /// </summary>
        public void StartGroup()
        {
            for (int i = 0; i < assignedNodes.Count; i++)
            {
                assignedNodes[i].canRun = true;
            }

            for (int i = 0; i < assignedNodes.Count; i++)
            {
                if (assignedNodes[i].derivedClassName == "OnStart")
                {
                    assignedNodes[i].ExecuteNode();
                }

                if (assignedNodes[i].derivedClassName == "OnStartMultiple")
                {
                    assignedNodes[i].ExecuteNode();
                }

                if (assignedNodes[i].derivedClassName == "OnStartGroup")
                {
                    assignedNodes[i].ExecuteNode();
                }
            }
        }
    }
}