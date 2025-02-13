/*
 *	DATABRAIN
 *	(c) 2024 Giant Grey
 *	www.databrain.cc
 *	
 */
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Databrain.UI.Elements
{
    public enum GradientDirection
        {
            Horizontal,
            Vertical
        }
    
        public class GradientElement : VisualElement
        {
            static readonly Vertex[] _vertices = new Vertex[4];
            static readonly ushort[] _indices = { 0, 1, 2, 2, 3, 0 };
            static readonly CustomStyleProperty<Color> _gradientFromProperty = new("--gradient-from");
            static readonly CustomStyleProperty<Color> _gradientToProperty = new("--gradient-to");
            static readonly CustomStyleProperty<string> _gradientDirectionProperty = new("--gradient-direction");
            public GradientDirection gradientDirection;
    
            public Color gradientFrom;
            public Color gradientTo;
    
            public GradientElement()
            {
                generateVisualContent += GenerateVisualContent;
                RegisterCallback<CustomStyleResolvedEvent>(OnStylesResolved);
            }
    
            void OnStylesResolved(CustomStyleResolvedEvent @event)
            {
                @event.customStyle.TryGetValue(_gradientFromProperty, out gradientFrom);
                @event.customStyle.TryGetValue(_gradientToProperty, out gradientTo);
                @event.customStyle.TryGetValue(_gradientDirectionProperty, out var gradientDirectionAsString);
                if (Enum.TryParse(typeof(GradientDirection), gradientDirectionAsString, true, out var gradientDirection))
                    this.gradientDirection = (GradientDirection)gradientDirection;
                else
                    this.gradientDirection = GradientDirection.Horizontal;
            }
    
            void GenerateVisualContent(MeshGenerationContext meshGenerationContext)
            {
                var rect = contentRect;
                if (rect.width < 0.1f || rect.height < 0.1f)
                    return;
    
                UpdateVerticesTint();
                UpdateVerticesPosition(rect);
    
                var meshWriteData = meshGenerationContext.Allocate(_vertices.Length, _indices.Length);
                meshWriteData.SetAllVertices(_vertices);
                meshWriteData.SetAllIndices(_indices);
            }
    
            static void UpdateVerticesPosition(Rect rect)
            {
                const float left = 0f;
                var right = rect.width;
                const float top = 0f;
                var bottom = rect.height;
    
                _vertices[0].position = new Vector3(left, bottom, Vertex.nearZ);
                _vertices[1].position = new Vector3(left, top, Vertex.nearZ);
                _vertices[2].position = new Vector3(right, top, Vertex.nearZ);
                _vertices[3].position = new Vector3(right, bottom, Vertex.nearZ);
            }
    
            void UpdateVerticesTint()
            {
                if (gradientDirection is GradientDirection.Horizontal)
                {
                    _vertices[0].tint = gradientFrom;
                    _vertices[1].tint = gradientFrom;
                    _vertices[2].tint = gradientTo;
                    _vertices[3].tint = gradientTo;
                }
                else
                {
                    _vertices[0].tint = gradientTo;
                    _vertices[1].tint = gradientFrom;
                    _vertices[2].tint = gradientFrom;
                    _vertices[3].tint = gradientTo;
                }
            }
    
            // public new class UxmlFactory : UxmlFactory<GradientElement, GradientElementUxmlTraits>
            // {
            // }
    
            // public class GradientElementUxmlTraits : UxmlTraits
            // {
            // }
        }
    }