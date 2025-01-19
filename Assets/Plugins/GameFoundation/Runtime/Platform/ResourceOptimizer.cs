using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFoundation.Platform
{
    public class ResourceOptimizer : MonoBehaviour
    {
        [SerializeField]
        private int targetFps = 60;

        [SerializeField]
        private Vector2 designResolution = new Vector2(1024, 1366);

        private void Start()
        {
            Application.targetFrameRate = targetFps;

            // #if UNITY_ANDROID || UNITY_IOS
            var rate = designResolution.y / (float)Screen.currentResolution.height;
            var res = new Vector2(
                Screen.currentResolution.width * rate,
                Screen.currentResolution.height * rate
            );
            var fullScreen = Application.platform == RuntimePlatform.WebGLPlayer ? false : true;
            // Debug.Log(
            //     $"Resolution {Screen.currentResolution.width},{Screen.currentResolution.height}"
            // );

            // change resolution
            Screen.SetResolution((int)res.x, (int)res.y, fullScreen);

            // await UniTask.DelayNextFrame();
            // Debug.Log(
            //     $"Change to resolution {Screen.currentResolution.width},{Screen.currentResolution.height}"
            // );
            // #endif
        }
    }
}
