/*
 *	DATABRAIN | Logic
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
using Databrain.Logic.Attributes;
using UnityEngine;

namespace Databrain.Logic
{
    public class CommentData : ScriptableObject
    {
        [NodeHideVariable]
        public string guid = "";
        [NodeHideVariable]
        public string title;
        [NodeHideVariable]
        public string comment;
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
    }
}