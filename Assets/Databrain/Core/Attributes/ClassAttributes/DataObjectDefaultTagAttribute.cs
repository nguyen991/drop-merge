using System;
using System.Collections.Generic;

namespace Databrain.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class DataObjectDefaultTagAttribute : Attribute
    {
        public List<string> defaultTags = new();

        public DataObjectDefaultTagAttribute(params string[] defaultTags)
        {
            this.defaultTags = new(defaultTags);
        }
    }
}