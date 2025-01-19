/*  
 *  DATABRAIN | Logic add-on
 *  (c) 2023 by Giant Grey / www.giantgrey.com
 *  Author: Marc Egli
 *  
 */
using Databrain.Attributes;
using System;
using UnityEngine;

namespace Databrain.Logic.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeSize : Attribute
    {
        public Vector2Int size;

        /// <summary>
        /// Set the size of the node
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public NodeSize(int width, int height)
        {
            this.size = new Vector2Int(width, height);
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class NodeOutputs : Attribute
    {
        public string[] outputs;

        /// <summary>
        /// Set the node outputs
        /// </summary>
        /// <param name="_outputs"></param>
        public NodeOutputs(string[] _outputs)
        {
            outputs = _outputs;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class NodeTitle : Attribute
    {
        public string title;

        /// <summary>
        /// Set the node title
        /// </summary>
        /// <param name="_title"></param>
        public NodeTitle(string _title)
        {
            title = _title;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class NodeDescription : Attribute
    {
        public string description;

        /// <summary>
        /// Set the node description
        /// </summary>
        /// <param name="description"></param>
        public NodeDescription (string description)
        {
            this.description = description;
        }
    }


    [AttributeUsage(AttributeTargets.Class)]
    public class NodeIcon : Attribute
    {
        public string icon;
        public string rootFile;

        /// <summary>
        /// Set a node icon
        /// </summary>
        /// <param name="icon"></param>
        public NodeIcon (string icon)
        {
            this.icon = icon;
        }

        public NodeIcon (string icon, string rootFile)
        {
            this.icon = icon;
            this.rootFile = rootFile;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class NodeColor : Attribute
    {
        public string color;
        public string[] borderColors;
        public DatabrainColor databrainColor = DatabrainColor.Clear;

        /// <summary>
        /// Set a node color
        /// </summary>
        /// <param name="_color"></param>
        public NodeColor(DatabrainColor _color)
        {
            databrainColor = _color;
        }

        /// <summary>
        /// Additional _borderColors (left, right, top, bottom)
        /// </summary>
        /// <param name="_color"></param>
        /// <param name="_borderColors"></param>
        public NodeColor(string _color, params string[] _borderColors)
        {
            color = _color;
            borderColors = _borderColors;
        }
    }


    [AttributeUsage(AttributeTargets.Class)]
    public class NodeCategory : Attribute
    {
        public string category;
        public int order = 0;

        /// <summary>
        /// Set a category for this node
        /// </summary>
        /// <param name="category"></param>
        public NodeCategory(string category)
        {
            this.category = category;
        }

        /// <summary>
        /// Set a category and order for this node
        /// </summary>
        /// <param name="category"></param>
        public NodeCategory(string category, int order)
        {
            this.category = category;
            this.order = order;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class NodeHideVariable : Attribute{}

    [AttributeUsage(AttributeTargets.Class)]
    public class NodeAddOutputsUI : Attribute { }

    [AttributeUsage(AttributeTargets.Class)]
    public class NodeNotConnectable : Attribute { }

    [AttributeUsage(AttributeTargets.Class)]
    public class HideNode : Attribute{}

    [AttributeUsage(AttributeTargets.Class)]
    public class HideNodeOnCanvas : Attribute { }

    [AttributeUsage(AttributeTargets.Class)]
    public class NodeInputConnectionType : Attribute
    {
        public Type[] types;
        public NodeInputConnectionType(Type[] types)
        {
            this.types = types;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class NodeOutputConnectionType : Attribute
    {
        public Type[] types;
        public NodeOutputConnectionType(Type[] types)
        {
            this.types = types;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class Node3LinesSpline : Attribute{}


    [AttributeUsage(AttributeTargets.Class)]
    public class NodeCustomInspectorTab : Attribute
    {
        public string tabName;
        public int defaultTabWidth;

        public NodeCustomInspectorTab(string tabName, int defaultTabWidth = 0)
        {
            this.tabName = tabName;
            this.defaultTabWidth = defaultTabWidth;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class NodeGradientBorder : Attribute
    {
        public string gradientSpriteName;
        public string rootFile;

        public NodeGradientBorder(string gradientSpriteName, string rootFile)
        {
            this.gradientSpriteName = gradientSpriteName;
            this.rootFile = rootFile;
        }
    }
}