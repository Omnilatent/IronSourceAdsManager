using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Omnilatent.AdsMediation.IronSourceHelper
{
    public partial class IronSourceAdsManager : MonoBehaviour, IAdsNetworkHelper
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
                return;
            }
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

            InitRewardAdCallbacks();
            InitBannerCallbacks();

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
                currentInterstitialAd.State = AdObjectState.Showing;
                return;
            }
            onAdClosed?.Invoke(false);
        }

        private void InterstitialAdReadyEvent()
        {
            QueueMainThreadExecution(() =>
            {
                GetCurrentInterAd().State = AdObjectState.Ready;
                GetCurrentInterAd().onAdLoaded?.Invoke(true);
                onInterstitialAdReadyEvent?.Invoke(GetCurrentInterAd().AdPlacementType);
                Debug.Log($"Iron source ad ready {GetCurrentInterAd().AdPlacementType}");
            });
        }

        private void InterstitialAdLoadFailedEvent(IronSourceError error)
        {
            QueueMainThreadExecution(() =>
            {
                GetCurrentInterAd().State = AdObjectState.LoadFailed;
                GetCurrentInterAd().onAdLoaded?.Invoke(false);
                onInterstitialAdLoadFailedEvent?.Invoke(GetCurrentInterAd().AdPlacementType, error);
            });
        }

        private void InterstitialAdShowSucceededEvent()
        {
            QueueMainThreadExecution(() =>
            {
                GetCurrentInterAd().State = AdObjectState.Shown;
                onInterstitialAdShowSucceededEvent?.Invoke(GetCurrentInterAd().AdPlacementType);
            });
        }

        private void InterstitialAdShowFailedEvent(IronSourceError error)
        {
            QueueMainThreadExecution(() =>
            {
                GetCurrentInterAd().State = AdObjectState.ShowFailed;
                onInterstitialAdShowFailedEvent?.Invoke(GetCurrentInterAd().AdPlacementType, error);
            });
        }

        private void InterstitialAdClickedEvent()
        {
            QueueMainThreadExecution(() =>
            {
                onInterstitialAdClickedEvent?.Invoke(GetCurrentInterAd().AdPlacementType);
            });
        }

        private void InterstitialAdOpenedEvent()
        {
            QueueMainThreadExecution(() =>
            {
                onInterstitialAdOpenedEvent?.Invoke(GetCurrentInterAd().AdPlacementType);
            });
        }

        private void InterstitialAdClosedEvent()
        {
            QueueMainThreadExecution(() =>
            {
                GetCurrentInterAd().State = AdObjectState.Closed;
                GetCurrentInterAd().onAdClosed?.Invoke(true);
                onInterstitialAdClosedEvent?.Invoke(GetCurrentInterAd().AdPlacementType);
            });
        }

        public void RequestAppOpenAd(AdPlacement.Type placementType, RewardDelegate onAdLoaded = null)
        {
            onAdLoaded?.Invoke(new RewardResult(RewardResult.Type.LoadFailed, "IronSources: App Open Ad not supported"));
        }

        public void ShowAppOpenAd(AdPlacement.Type placementType, AdsManager.InterstitialDelegate onAdClosed = null)
        {
            onAdClosed?.Invoke(false);
        }

        public void RequestInterstitialRewardedNoShow(AdPlacement.Type placementType, RewardDelegate onAdLoaded = null)
        {
            onAdLoaded?.Invoke(new RewardResult(RewardResult.Type.LoadFailed, "IronSources: Interstitial Rewarded not supported"));
        }

        public void ShowInterstitialRewarded(AdPlacement.Type placementType, RewardDelegate onAdClosed)
        {
            onAdClosed?.Invoke(new RewardResult(RewardResult.Type.LoadFailed, "IronSources: Interstitial Rewarded not supported"));
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