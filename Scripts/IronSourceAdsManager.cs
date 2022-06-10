using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Omnilatent.AdsMediation.IronSourceHelper
{
    public enum AdObjectState
    {
        None = 0, Loading, Ready, Shown, LoadFailed, ShowFailed
    }

    public class IronSourceAdsManager : MonoBehaviour, IAdsNetworkHelper
    {
        bool initialized = false;
        InterstitialAdObject currentInterstitialAd;

        public static Action<AdPlacement.Type> onInterstitialAdReadyEvent;
        public static Action<AdPlacement.Type, IronSourceError> onInterstitialAdLoadFailedEvent;
        public static Action<AdPlacement.Type> onInterstitialAdShowSucceededEvent;
        public static Action<AdPlacement.Type, IronSourceError> onInterstitialAdShowFailedEvent;
        public static Action<AdPlacement.Type> onInterstitialAdClickedEvent;
        public static Action<AdPlacement.Type> onInterstitialAdOpenedEvent;
        public static Action<AdPlacement.Type> onInterstitialAdClosedEvent;

        static IronSourceAdsManager instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            Init();
        }

        public void Init()
        {
            if (initialized) return;

            Initilize();

            IronSourceEvents.onInterstitialAdReadyEvent += InterstitialAdReadyEvent;
            IronSourceEvents.onInterstitialAdLoadFailedEvent += InterstitialAdLoadFailedEvent;
            IronSourceEvents.onInterstitialAdShowSucceededEvent += InterstitialAdShowSucceededEvent;
            IronSourceEvents.onInterstitialAdShowFailedEvent += InterstitialAdShowFailedEvent;
            IronSourceEvents.onInterstitialAdClickedEvent += InterstitialAdClickedEvent;
            IronSourceEvents.onInterstitialAdOpenedEvent += InterstitialAdOpenedEvent;
            IronSourceEvents.onInterstitialAdClosedEvent += InterstitialAdClosedEvent;
            initialized = true;
        }

        static void Initilize()
        {
            var developerSettings = Resources.Load<IronSourceMediationSettings>(IronSourceConstants.IRONSOURCE_MEDIATION_SETTING_NAME);
            if (developerSettings != null)
            {
#if UNITY_ANDROID
                string appKey = developerSettings.AndroidAppKey;
#elif UNITY_IOS
        string appKey = developerSettings.IOSAppKey;
#endif
                if (appKey.Equals(string.Empty))
                {
                    Debug.LogWarning("IronSourceInitilizer Cannot init without AppKey");
                }
                else
                {
                    IronSource.Agent.init(appKey);
                    IronSource.UNITY_PLUGIN_VERSION = "7.2.1-ri";
                }
            }
        }

        InterstitialAdObject GetCurrentInterAd(bool makeNewIfNull = true)
        {
            if (currentInterstitialAd == null)
            {
                Debug.LogError("currentInterstitialAd is null.");
                if (makeNewIfNull)
                {
                    Debug.Log("New ad will be created");
                    currentInterstitialAd = new InterstitialAdObject();
                }
            }
            return currentInterstitialAd;
        }

        private void InterstitialAdReadyEvent()
        {
            QueueMainThreadExecution(() =>
            {
                GetCurrentInterAd().ready = true;
                GetCurrentInterAd().onAdLoaded?.Invoke(true);
                onInterstitialAdReadyEvent?.Invoke(GetCurrentInterAd().adPlacementType);
                Debug.Log($"Iron source ad ready {GetCurrentInterAd().adPlacementType}");
            });
        }

        private void InterstitialAdLoadFailedEvent(IronSourceError error)
        {
            QueueMainThreadExecution(() =>
            {
                GetCurrentInterAd().ready = false;
                GetCurrentInterAd().onAdLoaded?.Invoke(false);
                onInterstitialAdLoadFailedEvent?.Invoke(GetCurrentInterAd().adPlacementType, error);
            });
        }

        private void InterstitialAdShowSucceededEvent()
        {
            QueueMainThreadExecution(() =>
            {
                onInterstitialAdShowSucceededEvent?.Invoke(GetCurrentInterAd().adPlacementType);
            });
        }

        private void InterstitialAdShowFailedEvent(IronSourceError error)
        {
            QueueMainThreadExecution(() =>
            {
                onInterstitialAdShowFailedEvent?.Invoke(GetCurrentInterAd().adPlacementType, error);
            });
        }

        private void InterstitialAdClickedEvent()
        {
            QueueMainThreadExecution(() =>
            {
                onInterstitialAdClickedEvent?.Invoke(GetCurrentInterAd().adPlacementType);
            });
        }

        private void InterstitialAdOpenedEvent()
        {
            QueueMainThreadExecution(() =>
            {
                onInterstitialAdOpenedEvent?.Invoke(GetCurrentInterAd().adPlacementType);
            });
        }

        private void InterstitialAdClosedEvent()
        {
            QueueMainThreadExecution(() =>
            {
                GetCurrentInterAd().onAdClosed?.Invoke(true);
                onInterstitialAdClosedEvent?.Invoke(GetCurrentInterAd().adPlacementType);
            });
        }

        public void RequestInterstitialNoShow(AdPlacement.Type placementType, AdsManager.InterstitialDelegate onAdLoaded = null, bool showLoading = true)
        {
            Debug.Log($"Iron source request ad {placementType}");
            if (currentInterstitialAd != null && currentInterstitialAd.CanShow)
            {
                onAdLoaded?.Invoke(true);
                return;
            }
            currentInterstitialAd = new InterstitialAdObject(placementType, onAdLoaded);
            IronSource.Agent.loadInterstitial();
        }

        public void ShowInterstitial(AdPlacement.Type placementType, AdsManager.InterstitialDelegate onAdClosed)
        {
            Debug.Log($"Iron source show ad {placementType}");
            if (currentInterstitialAd != null && currentInterstitialAd.CanShow)
            {
                string placementName = IronSourceAdID.GetAdID(placementType);
                currentInterstitialAd.onAdClosed = onAdClosed;
                IronSource.Agent.showInterstitial(placementName);
                currentInterstitialAd.shown = true;
                return;
            }
            onAdClosed?.Invoke(false);
        }




        public void RequestAppOpenAd(AdPlacement.Type placementType, RewardDelegate onAdLoaded = null)
        {
            throw new NotImplementedException();
        }

        public void ShowAppOpenAd(AdPlacement.Type placementType, AdsManager.InterstitialDelegate onAdClosed = null)
        {
            throw new System.NotImplementedException();
        }



        public void RequestInterstitialRewardedNoShow(AdPlacement.Type placementType, RewardDelegate onAdLoaded = null)
        {
            throw new System.NotImplementedException();
        }

        public void ShowInterstitialRewarded(AdPlacement.Type placementType, RewardDelegate onAdClosed)
        {
            throw new System.NotImplementedException();
        }

        public void Reward(AdPlacement.Type placementType, RewardDelegate onFinish)
        {
            throw new System.NotImplementedException();
        }

        public void ShowBanner(AdPlacement.Type placementType, AdsManager.InterstitialDelegate onAdLoaded = null)
        {
            throw new System.NotImplementedException();
        }

        public void ShowBanner(AdPlacement.Type placementType, BannerTransform bannerTransform, AdsManager.InterstitialDelegate onAdLoaded = null)
        {
            throw new System.NotImplementedException();
        }

        public void HideBanner()
        {
            throw new System.NotImplementedException();
        }

        public static void QueueMainThreadExecution(Action action)
        {
#if UNITY_ANDROID
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                action.Invoke();
            });
#else
        action.Invoke();
#endif
        }
    }
}