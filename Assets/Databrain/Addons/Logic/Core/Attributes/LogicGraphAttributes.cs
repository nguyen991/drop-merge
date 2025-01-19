using System;
using System.Collections.Generic;


namespace Databrain.Logic.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class LogicGraphDefaultNodes : Attribute
    {
        public Type[] nodes;

        public LogicGraphDefaultNodes(Type[] _nodes)
        {
            nodes = _nodes;
        }
    }
}
