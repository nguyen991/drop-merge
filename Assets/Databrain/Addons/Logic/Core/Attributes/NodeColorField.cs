/*  
 *  DATABRAIN | Logic add-on
 *  (c) 2023 by Giant Grey / www.giantgrey.com
 *  Author: Marc Egli
 *  
 */
using System;
using UnityEngine;

namespace Databrain.Logic.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class NodeColorFieldAttribute : PropertyAttribute
    {
        public NodeColorFieldAttribute()
        {
        }
    }
}