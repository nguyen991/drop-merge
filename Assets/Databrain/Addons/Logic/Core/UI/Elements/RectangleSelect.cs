/*
 *	DATABRAIN | Logic
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using System;

using UnityEngine;
using UnityEngine.UIElements;


namespace Databrain.Logic.Elements
{
    public class RectangleSelect : ImmediateModeElement
    {
        public Vector2 start { get; set; }
        public Vector2 end { get; set; }

        protected override void ImmediateRepaint()
        {
            VisualElement t = parent;
            Vector2 screenStart = start;
            Vector2 screenEnd = end;


            // Apply offset
            screenStart += t.layout.position;
            screenEnd += t.layout.position;


            var r = new Rect
            {
                min = new Vector2(Math.Min(screenStart.x, screenEnd.x), Math.Min(screenStart.y, screenEnd.y)),
                max = new Vector2(Math.Max(screenStart.x, screenEnd.x), Math.Max(screenStart.y, screenEnd.y))
            };


            this.transform.position = r.position;
            this.style.width = r.width;
            this.style.height = r.height;
        }


    }
}
#endif