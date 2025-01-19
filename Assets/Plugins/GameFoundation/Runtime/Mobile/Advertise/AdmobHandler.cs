#pragma warning disable CS0414

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif

#if GF_AD_MOB
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
#endif

namespace GameFoundation.Mobile
{
    class AdmobHandler : IAdsHandler
    {
        private AdController.AdConfig config;

#if GF_AD_MOB
        private BannerView bannerView;
        private InterstitialAd interstitialAd;
        private RewardedAd rewardedAd;
        private RewardedInterstitialAd rewardedInterstitialAd;
#endif

        private UnityAction<bool> adResultCallback = null;
        private bool earnedReward = false;

        public void Init(AdController.AdConfig config, UnityAction onInitCompleted)
        {
            this.config = config;

#if UNITY_IOS
            if (config.requestIDFA)
            {
                ATTrackingStatusBinding.RequestAuthorizationTracking();
            }
#endif

#if GF_AD_MOB
            // Configure TagForChildDirectedTreatment and test device IDs.
            List<string> deviceIds = new List<string>() { AdRequest.TestDeviceSimulator };
            deviceIds.AddRange(config.testDevices);
            RequestConfiguration requestConfiguration = new RequestConfiguration
            {
                TestDeviceIds = deviceIds,
                TagForChildDirectedTreatment = TagForChildDirectedTreatment.Unspecified
            };

            // Initialize the Google Mobile Ads SDK.
#if UNITY_IOS
            MobileAds.SetiOSAppPauseOnBackground(true);
#endif
            // MobileAds.SetRequestConfiguration(requestConfiguration);
            MobileAds.Initialize(status =>
            {
                MobileAdsEventExecutor.ExecuteInUpdate(() =>
                {
                    Debug.Log($"Admob initialized");

                    // log mediation
                    Dictionary<string, AdapterStatus> map = status.getAdapterStatusMap();
                    foreach (KeyValuePair<string, AdapterStatus> keyValuePair in map)
                    {
                        string className = keyValuePair.Key;
                        AdapterStatus status = keyValuePair.Value;
                        switch (status.InitializationState)
                        {
                            case AdapterState.NotReady:
                                Debug.Log("Adapter: " + className + " not ready.");
                                break;

                            case AdapterState.Ready:
                                Debug.Log("Adapter: " + className + " is initialized.");
                                break;
                        }
                    }

                    // init completed
                    onInitCompleted?.Invoke();
                });
            });
#else
            // init completed
            onInitCompleted?.Invoke();
#endif
        }

#if GF_AD_MOB
        private AdRequest CreateAdRequest()
        {
            return new AdRequest();
        }
#endif

        #region Banner
        public void RequestBanner()
        {
#if GF_AD_MOB
#if UNITY_EDITOR
            string adUnitId = "unused";
#elif UNITY_ANDROID
            string adUnitId = config.banner.android;
#elif UNITY_IPHONE
            string adUnitId = config.banner.ios;
#else
            string adUnitId = "unexpected_platform";
#endif
            // Clean up banner before reusing
            if (bannerView != null)
            {
                bannerView.Destroy();
            }

            // Create adaptive banner
            AdSize adaptiveSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(
                AdSize.FullWidth
            );
            bannerView = new BannerView(adUnitId, adaptiveSize, AdPosition.Bottom);

            // Add Event Handlers
            bannerView.OnBannerAdLoaded += () => Debug.Log("[Ads] Banner ad loaded.");
            bannerView.OnBannerAdLoadFailed += (error) =>
                MobileAdsEventExecutor.ExecuteInUpdate(() =>
                {
                    Debug.Log("[Ads] Banner ad failed to load: " + error.GetMessage());
                    BIAdFailed(
                        adUnitId,
                        AdController.AdType.Banner,
                        error.GetResponseInfo()?.GetMediationAdapterClassName(),
                        error.GetMessage()
                    );
                });
            bannerView.OnAdFullScreenContentOpened += () => Debug.Log("[Ads] Banner ad opened.");
            bannerView.OnAdFullScreenContentClosed += () => Debug.Log("[Ads] Banner ad closed.");
            bannerView.OnAdPaid += (value) =>
                MobileAdsEventExecutor.ExecuteInUpdate(() =>
                {
                    BIPaidEvent(
                        adUnitId,
                        AdController.AdType.Banner,
                        bannerView.GetResponseInfo()?.GetMediationAdapterClassName(),
                        value
                    );
                });

            // Load a banner ad
            bannerView.LoadAd(CreateAdRequest());
#endif
        }

        public void DestroyBanner()
        {
#if GF_AD_MOB
            if (bannerView != null)
            {
                bannerView.Destroy();
            }
#endif
        }

        public bool IsBanner()
        {
#if GF_AD_MOB
            return bannerView != null;
#else
            return false;
#endif
        }
        #endregion

        #region Interstitial
        public void RequestInterstitial()
        {
#if GF_AD_MOB
#if UNITY_EDITOR
            string adUnitId = "unused";
#elif UNITY_ANDROID
            string adUnitId = config.interstitial.android;
#elif UNITY_IPHONE
            string adUnitId = config.interstitial.ios;
#else
            string adUnitId = "unexpected_platform";
#endif

            // Clean up interstitial before using it
            if (interstitialAd != null)
            {
                interstitialAd.Destroy();
                interstitialAd = null;
            }

            // Load an interstitial ad.
            InterstitialAd.Load(
                adUnitId,
                CreateAdRequest(),
                (ad, error) =>
                {
                    interstitialAd = ad;
                    if (error != null || ad == null)
                    {
                        MobileAdsEventExecutor.ExecuteInUpdate(() =>
                        {
                            Debug.Log(
                                "[Ads] Interstitial ad failed to load: " + error.GetMessage()
                            );
                            BIAdFailed(
                                adUnitId,
                                AdController.AdType.Interstitial,
                                error.GetResponseInfo()?.GetMediationAdapterClassName(),
                                error.GetMessage()
                            );
                            adResultCallback?.Invoke(false);
                        });
                        return;
                    }
                    Debug.Log("[Ads] Interstitial ad loaded.");

                    // Add Event Handlers
                    interstitialAd.OnAdFullScreenContentOpened += () =>
                        Debug.Log("[Ads] Interstitial ad opened.");
                    interstitialAd.OnAdFullScreenContentClosed += () =>
                        MobileAdsEventExecutor.ExecuteInUpdate(() =>
                        {
                            RequestInterstitial();
                            adResultCallback?.Invoke(true);
                        });
                    interstitialAd.OnAdFullScreenContentFailed += (error) =>
                        MobileAdsEventExecutor.ExecuteInUpdate(() =>
                        {
                            RequestInterstitial();
                            adResultCallback?.Invoke(false);
                        });
                    interstitialAd.OnAdPaid += (value) =>
                        MobileAdsEventExecutor.ExecuteInUpdate(() =>
                        {
                            BIPaidEvent(
                                adUnitId,
                                AdController.AdType.Interstitial,
                                interstitialAd.GetResponseInfo()?.GetMediationAdapterClassName(),
                                value
                            );
                        });
                    interstitialAd.OnAdImpressionRecorded += () =>
                        MobileAdsEventExecutor.ExecuteInUpdate(() =>
                        {
                            BIRecordImpression(
                                adUnitId,
                                AdController.AdType.Interstitial,
                                interstitialAd.GetResponseInfo()?.GetMediationAdapterClassName()
                            );
                        });
                }
            );
#endif
        }

        public bool IsInterstitalAvailable()
        {
#if GF_AD_MOB
            return interstitialAd != null && interstitialAd.CanShowAd();
#else
            return false;
#endif
        }

        public void ShowInterstitial(UnityAction<bool> callback = null)
        {
#if GF_AD_MOB
            if (IsInterstitalAvailable())
            {
                adResultCallback = callback;
                interstitialAd.Show();
            }
            else
            {
                callback?.Invoke(false);
            }
#else
            callback?.Invoke(false);
#endif
        }

        public void DestroyInterstitial()
        {
#if GF_AD_MOB
            if (interstitialAd != null)
            {
                interstitialAd.Destroy();
            }
#endif
        }
        #endregion

        #region Reward
        public void RequestReward()
        {
#if GF_AD_MOB
#if UNITY_EDITOR
            string adUnitId = "unused";
#elif UNITY_ANDROID
            string adUnitId = config.reward.android;
#elif UNITY_IPHONE
            string adUnitId = config.reward.ios;
#else
            string adUnitId = "unexpected_platform";
#endif

            if (rewardedAd != null)
            {
                rewardedAd.Destroy();
                rewardedAd = null;
            }

            RewardedAd.Load(
                adUnitId,
                CreateAdRequest(),
                (ad, error) =>
                {
                    rewardedAd = ad;
                    if (error != null || ad == null)
                    {
                        MobileAdsEventExecutor.ExecuteInUpdate(() =>
                        {
                            Debug.Log("[Ads] Rewarded ad failed to load: " + error.GetMessage());
                            BIAdFailed(
                                adUnitId,
                                AdController.AdType.Reward,
                                error.GetResponseInfo()?.GetMediationAdapterClassName(),
                                error.GetMessage()
                            );
                            adResultCallback?.Invoke(false);
                        });
                        return;
                    }
                    Debug.Log("[Ads] Rewarded ad loaded.");

                    // Add Event Handlers
                    rewardedAd.OnAdFullScreenContentOpened += () =>
                        Debug.Log("[Ads] Rewarded ad opened.");
                    rewardedAd.OnAdPaid += (value) =>
                        MobileAdsEventExecutor.ExecuteInUpdate(() =>
                        {
                            BIPaidEvent(
                                adUnitId,
                                AdController.AdType.Reward,
                                rewardedAd.GetResponseInfo()?.GetMediationAdapterClassName(),
                                value
                            );
                        });
                    rewardedAd.OnAdImpressionRecorded += () =>
                        MobileAdsEventExecutor.ExecuteInUpdate(() =>
                        {
                            BIRecordImpression(
                                adUnitId,
                                AdController.AdType.Reward,
                                rewardedAd.GetResponseInfo()?.GetMediationAdapterClassName()
                            );
                        });
                    rewardedAd.OnAdFullScreenContentFailed += (error) =>
                        MobileAdsEventExecutor.ExecuteInUpdate(() =>
                        {
                            adResultCallback?.Invoke(false);
                        });
                    rewardedAd.OnAdFullScreenContentClosed += () =>
                        MobileAdsEventExecutor.ExecuteInUpdate(() =>
                        {
                            RequestReward();
#if UNITY_EDITOR
                            adResultCallback?.Invoke(true);
#else
                            adResultCallback?.Invoke(earnedReward);
#endif
                        });
                }
            );
#endif
        }

        public bool IsRewardAvailable()
        {
#if GF_AD_MOB
            return rewardedAd != null && rewardedAd.CanShowAd();
#else
            return false;
#endif
        }

        public void ShowReward(UnityAction<bool> callback = null)
        {
#if GF_AD_MOB
            if (IsRewardAvailable())
            {
                adResultCallback = callback;
                earnedReward = false;
                rewardedAd.Show(reward =>
                {
                    earnedReward = true;
                });
            }
            else
            {
                callback?.Invoke(false);
            }
#else
            callback?.Invoke(false);
#endif
        }
        #endregion

        #region BI_EVENTS
#if GF_AD_MOB
        private void BIPaidEvent(
            string adUnitId,
            string adFormat,
            string mediation,
            AdValue adValue
        )
        {
            LogEvent.Log(
                "bi_ad_value",
                new LogEvent.Parameter("ad_platform", "admob"),
                new LogEvent.Parameter(
                    "ad_source",
                    string.IsNullOrEmpty(mediation) ? "UNKNOWN" : mediation
                ),
                new LogEvent.Parameter("ad_platform_unit_id", adUnitId),
                new LogEvent.Parameter("ad_source_unit_id", "UNKNOWN"),
                new LogEvent.Parameter("ad_format", adFormat),
                new LogEvent.Parameter("ad_number", AdNumberToday(adFormat, false)),
                new LogEvent.Parameter("estimated_value", adValue.Value),
                new LogEvent.Parameter("est_value_currency", adValue.CurrencyCode),
                new LogEvent.Parameter("precision_type", (int)adValue.Precision),
                new LogEvent.Parameter("est_value_usd", adValue.Value)
            );
            Debug.Log($"[Ads] bi_ad_value: {adFormat} {adValue.Value}{adValue.CurrencyCode}");
        }

        private void BIRecordImpression(string adUnitId, string adFormat, string mediation)
        {
            LogEvent.Log(
                "bi_ad_impression",
                new LogEvent.Parameter("ad_platform", "admob"),
                new LogEvent.Parameter(
                    "ad_source",
                    string.IsNullOrEmpty(mediation) ? "UNKNOWN" : mediation
                ),
                new LogEvent.Parameter("ad_platform_unit_id", adUnitId),
                new LogEvent.Parameter("ad_source_unit_id", "UNKNOWN"),
                new LogEvent.Parameter("ad_format", adFormat),
                new LogEvent.Parameter("ad_number", AdNumberToday(adFormat, true))
            );
            Debug.Log($"[Ads] bi_ad_impression: {adFormat}");
        }

        private void BIAdFailed(string adUnitId, string adFormat, string mediation, string message)
        {
            LogEvent.Log(
                "bi_ad_request_failed",
                new LogEvent.Parameter("ad_platform", "admob"),
                new LogEvent.Parameter(
                    "ad_source",
                    string.IsNullOrEmpty(mediation) ? "UNKNOWN" : mediation
                ),
                new LogEvent.Parameter("ad_platform_unit_id", adUnitId),
                new LogEvent.Parameter("ad_source_unit_id", "UNKNOWN"),
                new LogEvent.Parameter("ad_format", adFormat),
                new LogEvent.Parameter("ad_number", AdNumberToday(adFormat, false)),
                new LogEvent.Parameter("error_message", message)
            );
        }

        private int AdNumberToday(string adFormat, bool increase)
        {
            var adNumber = PlayerPrefs.GetInt(adFormat, 0);
            if (!increase)
            {
                return adNumber;
            }

            adNumber += 1;

            var lastDateStr = PlayerPrefs.GetString($"{adFormat}_date");
            var now = DateTime.Now;
            if (now.ToString("d") != lastDateStr)
            {
                adNumber = 1;
            }

            PlayerPrefs.SetInt(adFormat, adNumber);
            PlayerPrefs.SetString($"{adFormat}_date", now.ToString("d"));

            return adNumber;
        }
#endif
        #endregion BI_EVENTS
    }
}
