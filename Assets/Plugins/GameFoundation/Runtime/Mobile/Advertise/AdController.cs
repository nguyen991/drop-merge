using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace GameFoundation.Mobile
{
    public class AdController : SingletonBehaviour<AdController>
    {
        public enum AdSource
        {
            Fake,
            Admob,
            MAX,
        }

        public enum AdType
        {
            Banner,
            Interstitial,
            Reward,
        }

        [System.Serializable]
        public struct AdId
        {
            public string android;
            public string ios;
        }

        [System.Serializable]
        public struct AdConfig
        {
            public AdSource source;
            public bool requestIDFA;
            public bool autoLoadBanner;
            public List<string> testDevices;
            public AdId banner;
            public AdId interstitial;
            public AdId reward;

            [Header("Editor")]
            public bool useAdFakeOnEditor;
            public bool adFakeAvailable;

            public string GetAdUnitId(AdType adType)
            {
                if (Application.isEditor)
                {
                    return "unused";
                }
                if (Application.isMobilePlatform)
                {
                    return adType switch
                    {
                        AdType.Banner
                            => Application.platform == RuntimePlatform.Android
                                ? banner.android
                                : banner.ios,
                        AdType.Interstitial
                            => Application.platform == RuntimePlatform.Android
                                ? interstitial.android
                                : interstitial.ios,
                        AdType.Reward
                            => Application.platform == RuntimePlatform.Android
                                ? interstitial.android
                                : interstitial.ios,
                        _ => "",
                    };
                }
                else
                {
                    return "unexpected_platform";
                }
            }
        }

        public AdConfig config;

        public bool Initialized { get; private set; } = false;

        private IAdsHandler handler = null;

        public void Init(AdConfig config)
        {
            if (Initialized)
            {
                return;
            }

            this.config = config;
            handler = config.source switch
            {
                AdSource.Admob => new AdmobHandler(),
                AdSource.MAX => new MAXHandler(),
                _ => new AdFakeHandler(config.adFakeAvailable),
            };
            handler.Init(
                config,
                () =>
                {
                    // request ads
                    if (config.autoLoadBanner)
                    {
                        handler.RequestBanner();
                    }
                    RequestInterstitial();
                    RequestReward();
                    Initialized = true;
                }
            );
        }

        public void RequestBanner()
        {
            handler.RequestBanner();
        }

        public void DestroyBanner()
        {
            handler.DestroyBanner();
        }

        public bool IsBanner()
        {
            return handler.IsBanner();
        }

        public void RequestInterstitial()
        {
            handler.RequestInterstitial();
        }

        public void ShowInterstitial(UnityAction<bool> callback = null)
        {
            handler.ShowInterstitial(callback);
        }

        public async UniTask<bool> ShowInterstitialAsync()
        {
            var task = new UniTaskCompletionSource<bool>();
            ShowInterstitial((success) => task.TrySetResult(success));
            var result = await task.Task;
            // await UniTask.SwitchToMainThread();
            return result;
        }

        public bool IsInterstitalAvailable()
        {
            return handler.IsInterstitalAvailable();
        }

        public void DestroyInterstitial()
        {
            handler.DestroyInterstitial();
        }

        public void RequestReward()
        {
            handler.RequestReward();
        }

        public void ShowReward(UnityAction<bool> callback = null)
        {
            handler.ShowReward(callback);
        }

        public async UniTask<bool> ShowRewardAsync()
        {
            var task = new UniTaskCompletionSource<bool>();
            ShowReward((success) => task.TrySetResult(success));
            var result = await task.Task;
            // await UniTask.SwitchToMainThread();
            return result;
        }

        public bool IsRewardAvailable()
        {
            return handler.IsRewardAvailable();
        }
    }
}
