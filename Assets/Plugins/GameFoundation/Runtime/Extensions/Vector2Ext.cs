using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFoundation.Extensions
{
    public static class Vector2Ext
    {
        public static bool FuzzyEquals(this Vector2 a, Vector2 b, float tolerance = 0.0001f)
        {
            return Mathf.Abs(a.x - b.x) < tolerance && Mathf.Abs(a.y - b.y) < tolerance;
        }
    }
}
