/*
 *	DATABRAIN | Logic
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace Databrain.Logic.Elements
{
    public class GridElement : ImmediateModeElement
    {
        private VisualElement canvas;
        private Rect canvasRect;
        private Vector2 offset = new Vector2(0, 0);
        private float gridSize = 10;

        private Color minor = new Color(Color.black.r, Color.black.g, Color.black.b, 20f / 255f);
        private Color major = new Color(Color.black.r, Color.black.g, Color.black.b, 30f / 255f);

        static Material _lineMaterial;

        public GridElement(VisualElement canvas)
        {
            this.canvas = canvas;
            canvasRect = new Rect(canvas.transform.position.x, canvas.transform.position.y, canvas.resolvedStyle.width, canvas.resolvedStyle.height);

            if (_lineMaterial == null)
            {
                Shader _shader = Shader.Find("Hidden/Internal-Colored");
                _lineMaterial = new Material(_shader);

                _lineMaterial.hideFlags = HideFlags.HideAndDontSave;
                _lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
            }

            UpdateBounds();
        }

        void UpdateBounds()
        {
            canvasRect = new Rect(canvas.transform.position.x, canvas.transform.position.y, canvas.resolvedStyle.width, canvas.resolvedStyle.height);
            style.left = canvasRect.x;
            style.top = canvasRect.y;
            style.width = canvasRect.width;
            style.height = canvasRect.height;
            style.flexGrow = 1;
            pickingMode = PickingMode.Ignore;
        }


        protected override void ImmediateRepaint()
        {
            DrawGrid();
        }

        void DrawGrid()
        {
            if (_lineMaterial == null)
            {
                Shader _shader = Shader.Find("Hidden/Internal-Colored");
                _lineMaterial = new Material(_shader);
            }

            _lineMaterial.SetPass(0);

            GL.Begin(GL.LINES);
            DrawGridLines(gridSize, minor);
            DrawGridLines(gridSize * 10, major);
            GL.End();

            UpdateBounds();
        }

        private void DrawGridLines(float _spacing, Color _gridColor)
        {
            float minX = canvasRect.xMin + offset.x % _spacing;
            float minY = canvasRect.yMin + offset.y % _spacing;

            GL.Color(_gridColor);
            for (float x = minX; x < canvasRect.xMax; x += _spacing)
            {
                DrawLine(new Vector2(x, canvasRect.yMin), new Vector2(x, canvasRect.yMax));
            }

            for (float y = minY; y < canvasRect.yMax; y += _spacing)
            {
                DrawLine(new Vector2(canvasRect.xMin, y), new Vector2(canvasRect.xMax, y));
            }
        }

        private void DrawLine(Vector2 _start, Vector2 _end)
        {            
            GL.Vertex(_start);
            GL.Vertex(_end);         
        }
    }
}
#endif