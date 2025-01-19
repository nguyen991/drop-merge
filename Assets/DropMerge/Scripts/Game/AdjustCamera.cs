using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DropMerge
{
    public class AdjustCamera : MonoBehaviour
    {
        public BoxCollider2D bounds;
        public List<Camera> cameras = new();

        private void Awake()
        {
            if (cameras.Count == 0)
            {
                cameras.Add(Camera.main);
            }
            var cameraWidth = cameras[0].orthographicSize * 2.0f * Screen.width / Screen.height;
            var boundSize = bounds.size.x + 0.3f;
            if (cameraWidth < boundSize)
            {
                var newSize = boundSize / (2.0f * Screen.width / Screen.height);
                foreach (var camera in cameras)
                {
                    camera.orthographicSize = newSize;
                }
            }
        }
    }
}
