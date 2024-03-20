using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Omnilatent.AdsMediation.IronSourceHelper
{
    public partial class IronSourceAdsManager : MonoBehaviour, IAdsNetworkHelper
    {
        public static Action<AdPlacement.Type, IronSourceError> onBannerAdLoadFailedEvent;
        BannerAdObject currentBannerAd;

        BannerAdObject GetCurrentBannerAd(bool makeNewIfNull = true)
        {
            if (currentBannerAd == null)
            {
                Debug.LogError("currentBannerAd is null.");
                if (makeNewIfNull)
                {
                    Debug.Log("New ad will be created");
                    currentBannerAd = new BannerAdObject();
                }
            }
            return currentBannerAd;
        }

        public void ShowBanner(AdPlacement.Type placementType, BannerTransform bannerTransform, AdsManager.InterstitialDelegate onAdLoaded = null)
        {
            if (currentBannerAd != null && currentBannerAd.AdPlacementType == placementType && currentBannerAd.State != AdObjectState.LoadFailed)
            {
                if (currentBannerAd.State == AdObjectState.Ready)
                {
                    IronSource.Agent.displayBanner();
                }
            }
            else
            {
                IronSourceBannerPosition ironSourceBannerPosition = IronSourceBannerPosition.BOTTOM;
                switch (bannerTransform.adPosition)
                {
                    case AdPosition.Top:
                    case AdPosition.TopLeft:
                    case AdPosition.TopRight:
                        ironSourceBannerPosition = IronSourceBannerPosition.TOP;
                        break;
                }
                IronSourceBannerSize ironSourceBannerSize = new IronSourceBannerSize("BANNER");
                ironSourceBannerSize.SetAdaptive(true);
                currentBannerAd = new BannerAdObject(placementType, (success, adObject) => { onAdLoaded?.Invoke(success); });
                currentBannerAd.State = AdObjectState.Loading;
                IronSource.Agent.loadBanner(ironSourceBannerSize, ironSourceBannerPosition, IronSourceAdID.GetAdID(placementType));
            }
        }

        public void HideBanner()
        {
            if (currentBannerAd != null)
            {
                IronSource.Agent.hideBanner();
                GetCurrentBannerAd().State = AdObjectState.Ready;
            }
        }

        public void DestroyBanner()
        {
            if (currentBannerAd != null)
            {
                IronSource.Agent.destroyBanner();
                currentBannerAd = null;
            }
        }

        void InitBannerCallbacks()
        {
            IronSourceEvents.onBannerAdLoadedEvent += BannerAdLoadedEvent;
            IronSourceEvents.onBannerAdLoadFailedEvent += BannerAdLoadFailedEvent;
            IronSourceEvents.onBannerAdClickedEvent += BannerAdClickedEvent;
            IronSourceEvents.onBannerAdScreenPresentedEvent += BannerAdScreenPresentedEvent;
            IronSourceEvents.onBannerAdScreenDismissedEvent += BannerAdScreenDismissedEvent;
            IronSourceEvents.onBannerAdLeftApplicationEvent += BannerAdLeftApplicationEvent;
        }

        private void BannerAdLoadedEvent()
        {
            QueueMainThreadExecution(() =>
            {
                GetCurrentBannerAd().State = AdObjectState.Showing;
                GetCurrentBannerAd().onAdLoaded?.Invoke(true, GetCurrentBannerAd());
            });
        }

        private void BannerAdLoadFailedEvent(IronSourceError error)
        {
            QueueMainThreadExecution(() =>
            {
                onBannerAdLoadFailedEvent?.Invoke(GetCurrentBannerAd().AdPlacementType, error);
                GetCurrentBannerAd().State = AdObjectState.LoadFailed;
                GetCurrentBannerAd().onAdLoaded?.Invoke(false, GetCurrentBannerAd());
            });
        }

        private void BannerAdClickedEvent()
        {
        }

        private void BannerAdScreenPresentedEvent()
        {
        }

        private void BannerAdScreenDismissedEvent()
        {
        }

        private void BannerAdLeftApplicationEvent()
        {
        }

        public void ShowBanner(AdPlacement.Type placementType, AdsManager.InterstitialDelegate onAdLoaded = null)
        {
            ShowBanner(placementType, new BannerTransform(AdPosition.Bottom), onAdLoaded);
        }
    }
}