using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Omnilatent.AdsMediation.IronSourceHelper
{
    public class IronSourceAdsManager : MonoBehaviour, IAdsNetworkHelper
    {
        bool initialized = false;
        InterstitialAdObject currentInterstitialAd;

        public Action<AdPlacement.Type> onInterstitialAdReadyEvent;
        public Action<AdPlacement.Type, IronSourceError> onInterstitialAdLoadFailedEvent;
        public Action<AdPlacement.Type> onInterstitialAdShowSucceededEvent;
        public Action<AdPlacement.Type, IronSourceError> onInterstitialAdShowFailedEvent;
        public Action<AdPlacement.Type> onInterstitialAdClickedEvent;
        public Action<AdPlacement.Type> onInterstitialAdOpenedEvent;
        public Action<AdPlacement.Type> onInterstitialAdClosedEvent;


        private void Start()
        {
            Init();
        }

        public void Init()
        {
            if (initialized) return;
            IronSourceEvents.onInterstitialAdReadyEvent += InterstitialAdReadyEvent;
            IronSourceEvents.onInterstitialAdLoadFailedEvent += InterstitialAdLoadFailedEvent;
            IronSourceEvents.onInterstitialAdShowSucceededEvent += InterstitialAdShowSucceededEvent;
            IronSourceEvents.onInterstitialAdShowFailedEvent += InterstitialAdShowFailedEvent;
            IronSourceEvents.onInterstitialAdClickedEvent += InterstitialAdClickedEvent;
            IronSourceEvents.onInterstitialAdOpenedEvent += InterstitialAdOpenedEvent;
            IronSourceEvents.onInterstitialAdClosedEvent += InterstitialAdClosedEvent;
            initialized = true;
        }

        InterstitialAdObject GetCurrentInterAd()
        {
            if (currentInterstitialAd == null)
            {
                Debug.LogError("currentInterstitialAd is null, this is not supposed to happen. New ad will be created");
                currentInterstitialAd = new InterstitialAdObject();
            }
            return currentInterstitialAd;
        }

        private void InterstitialAdReadyEvent()
        {
            QueueMainThreadExecution(() =>
            {
                GetCurrentInterAd().ready = true;
                GetCurrentInterAd().onAdLoaded?.Invoke(true);
            });
        }

        private void InterstitialAdLoadFailedEvent(IronSourceError error)
        {
            QueueMainThreadExecution(() =>
            {
                GetCurrentInterAd().ready = false;
                GetCurrentInterAd().onAdLoaded?.Invoke(false);
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
            });
        }

        public void RequestInterstitialNoShow(AdPlacement.Type placementType, AdsManager.InterstitialDelegate onAdLoaded = null, bool showLoading = true)
        {
            if (currentInterstitialAd != null && currentInterstitialAd.ready)
            {
                onAdLoaded?.Invoke(true);
                return;
            }
            currentInterstitialAd = new InterstitialAdObject(placementType, onAdLoaded);
            IronSource.Agent.loadInterstitial();
        }

        public void ShowInterstitial(AdPlacement.Type placementType, AdsManager.InterstitialDelegate onAdClosed)
        {
            if (currentInterstitialAd != null && currentInterstitialAd.ready)
            {
                string placementName = IronSourceAdID.GetAdID(placementType);
                currentInterstitialAd.onAdClosed = onAdClosed;
                IronSource.Agent.showInterstitial(placementName);
                return;
            }
            onAdClosed?.Invoke(false);
        }





        public void RequestAppOpenAd(AdPlacement.Type placementType, AdsManager.InterstitialDelegate onAdLoaded = null)
        {
            throw new System.NotImplementedException();
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