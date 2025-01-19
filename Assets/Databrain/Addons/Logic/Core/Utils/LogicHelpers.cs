/*
 *	DATABRAIN | Logic
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using System;
using System.Linq;

using UnityEngine;
using UnityEditor;


namespace Databrain.Logic.Utils
{
    public class LogicHelpers
    {
        public static Texture2D LoadResourceTexture(string _name)
        {
            return (Texture2D)(AssetDatabase.LoadAssetAtPath(GetRelativeResPath() + "/" + _name, typeof(Texture2D)));

        }


        public static string GetRelativeResPath()
        {
            var _res = System.IO.Directory.EnumerateFiles("Assets/", "LogicResPath.cs", System.IO.SearchOption.AllDirectories);

            var _path = "";

            var _found = _res.FirstOrDefault();
            if (!string.IsNullOrEmpty(_found))
            {
                _path = _found.Replace("LogicResPath.cs", "").Replace("\\", "/");
            }

            return _path;
        }

        public static Rect Encompass(Rect a, Rect b)
        {
            return new Rect
            {
                xMin = Math.Min(a.xMin, b.xMin),
                yMin = Math.Min(a.yMin, b.yMin),
                xMax = Math.Max(a.xMax, b.xMax),
                yMax = Math.Max(a.yMax, b.yMax)
            };
        }

    }
}
#endif