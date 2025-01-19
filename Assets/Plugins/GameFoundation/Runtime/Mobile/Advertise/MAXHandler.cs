using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace GameFoundation.Mobile
{
    public class MAXHandler : IAdsHandler
    {
        private AdController.AdConfig config;

        private UnityAction<bool> adResultCallback = null;
        private bool earnedReward = false;

        public void Init(AdController.AdConfig config, UnityAction onInitCompleted)
        {
            this.config = config;
            MaxSdkCallbacks.OnSdkInitializedEvent += (
                MaxSdkBase.SdkConfiguration sdkConfiguration
            ) =>
            {
                onInitCompleted?.Invoke();
            };

            // Banner Callbacks
            MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdLoadFailedEvent;
            MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdClickedEvent;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;
            MaxSdkCallbacks.Banner.OnAdExpandedEvent += OnBannerAdExpandedEvent;
            MaxSdkCallbacks.Banner.OnAdCollapsedEvent += OnBannerAdCollapsedEvent;

            // Interstitial Callbacks
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialLoadFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialDisplayedEvent;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClickedEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialHiddenEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent +=
                OnInterstitialAdFailedToDisplayEvent;

            // Attach callback
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedAdRevenuePaidEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdHiddenEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;

            MaxSdk.InitializeSdk();
        }

        public bool IsBanner()
        {
            return bannerLoaded;
        }

        public void RequestBanner()
        {
            string adUnitId = config.GetAdUnitId(AdController.AdType.Banner);
            MaxSdk.CreateBanner(adUnitId, MaxSdkBase.BannerPosition.BottomCenter);
            MaxSdk.SetBannerBackgroundColor(adUnitId, Color.black);
        }

        public void DestroyBanner()
        {
            MaxSdk.DestroyBanner(config.GetAdUnitId(AdController.AdType.Banner));
        }

        #region Banner Callbacks
        private bool bannerLoaded = false;

        private void OnBannerAdCollapsedEvent(string adUnitId, MaxSdkBase.AdInfo info) { }

        private void OnBannerAdExpandedEvent(string adUnitId, MaxSdkBase.AdInfo info) { }

        private void OnBannerAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo info) { }

        private void OnBannerAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo info) { }

        private void OnBannerAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo info)
        {
            Debug.Log(
                "Banner failed to load with error code: "
                    + info.Code
                    + " and message: "
                    + info.Message
            );
        }

        private void OnBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo info)
        {
            bannerLoaded = true;
        }
        #endregion


        public bool IsInterstitalAvailable()
        {
            return MaxSdk.IsInterstitialReady(config.GetAdUnitId(AdController.AdType.Interstitial));
        }

        public void RequestInterstitial()
        {
            MaxSdk.LoadInterstitial(config.GetAdUnitId(AdController.AdType.Interstitial));
        }

        public void ShowInterstitial(UnityAction<bool> callback = null)
        {
            string adUnitId = config.GetAdUnitId(AdController.AdType.Interstitial);
            if (MaxSdk.IsInterstitialReady(adUnitId))
            {
                adResultCallback = callback;
                MaxSdk.ShowInterstitial(adUnitId);
            }
            else
            {
                callback?.Invoke(false);
            }
        }

        public void DestroyInterstitial() { }

        #region Interstitial Callbacks
        private int interstitialRetry = 0;

        private void OnInterstitialAdFailedToDisplayEvent(
            string adUnitId,
            MaxSdkBase.ErrorInfo errorInfo,
            MaxSdkBase.AdInfo adInfo
        )
        {
            Debug.Log(
                "Interstitial failed to display with error code: "
                    + errorInfo.Code
                    + " and message: "
                    + errorInfo.Message
            );

            // Interstitial ad failed to display. AppLovin recommends that you load the next ad.
            RequestInterstitial();
            adResultCallback?.Invoke(false);
        }

        private void OnInterstitialHiddenEvent(string arg1, MaxSdkBase.AdInfo info)
        {
            RequestInterstitial();
            adResultCallback?.Invoke(true);
        }

        private void OnInterstitialClickedEvent(string arg1, MaxSdkBase.AdInfo info) { }

        private void OnInterstitialDisplayedEvent(string arg1, MaxSdkBase.AdInfo info) { }

        private void OnInterstitialLoadFailedEvent(string arg1, MaxSdkBase.ErrorInfo info)
        {
            interstitialRetry++;
            double retryDelay = Math.Pow(2, Math.Min(6, interstitialRetry));
            UniTask
                .Delay(TimeSpan.FromSeconds(retryDelay))
                .ContinueWith(() => RequestInterstitial());
        }

        private void OnInterstitialLoadedEvent(string arg1, MaxSdkBase.AdInfo info)
        {
            interstitialRetry = 0;
        }
        #endregion

        public bool IsRewardAvailable()
        {
            return MaxSdk.IsRewardedAdReady(config.GetAdUnitId(AdController.AdType.Reward));
        }

        public void RequestReward()
        {
            MaxSdk.LoadRewardedAd(config.GetAdUnitId(AdController.AdType.Reward));
        }

        public void ShowReward(UnityAction<bool> callback = null)
        {
            string adUnitId = config.GetAdUnitId(AdController.AdType.Reward);
            if (MaxSdk.IsRewardedAdReady(adUnitId))
            {
                adResultCallback = callback;
                MaxSdk.ShowRewardedAd(adUnitId);
            }
            else
            {
                callback?.Invoke(false);
            }
        }

        #region Reward Callbacks
        private int rewardRetry = 0;

        private void OnRewardedAdReceivedRewardEvent(
            string adUnitId,
            MaxSdkBase.Reward reward,
            MaxSdkBase.AdInfo info
        )
        {
            earnedReward = true;
        }

        private void OnRewardedAdFailedToDisplayEvent(
            string adUnitId,
            MaxSdkBase.ErrorInfo errorInfo,
            MaxSdkBase.AdInfo adInfo
        )
        {
            Debug.Log(
                "Rewarded ad failed to display with error code: "
                    + errorInfo.Code
                    + " and message: "
                    + errorInfo.Message
            );

            // Rewarded ad failed to display. AppLovin recommends that you load the next ad.
            RequestReward();
            adResultCallback?.Invoke(false);
        }

        private void OnRewardedAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo info)
        {
            RequestReward();
            adResultCallback?.Invoke(earnedReward);
            earnedReward = false;
        }

        private void OnRewardedAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo info) { }

        private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo info) { }

        private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo info) { }

        private void OnRewardedAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo info)
        {
            rewardRetry++;
            double retryDelay = Math.Pow(2, Math.Min(6, rewardRetry));
            UniTask.Delay(TimeSpan.FromSeconds(retryDelay)).ContinueWith(() => RequestReward());
        }

        private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo info)
        {
            rewardRetry = 0;
        }
        #endregion
    }
}
