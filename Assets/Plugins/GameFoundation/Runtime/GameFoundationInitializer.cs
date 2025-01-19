using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameFoundation.Mobile;
using GameFoundation.Storage;
using UnityEngine;
using UnityEngine.Events;

namespace GameFoundation
{
    public class GameFoundationInitializer : MonoBehaviour
    {
        public GameFoundationSetting setting;

        public UnityEvent onInitialized;

        private async void Start()
        {
            GameStorage.Instance.SetDataLayer(setting.storageLayer, null);

            SetDesignResolution();
            await UniTask.NextFrame();

            LoadAdmob();
            await UniTask.NextFrame();

            onInitialized.Invoke();
        }

        private void SetDesignResolution()
        {
            Application.targetFrameRate = setting.targetFps;

            // #if UNITY_ANDROID || UNITY_IOS
            var rate = setting.designResolution.y / (float)Screen.currentResolution.height;
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

        private void LoadAdmob()
        {
            AdController.Instance.Init(setting.admobConfig);
        }
    }
}
