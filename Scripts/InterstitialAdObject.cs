using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Omnilatent.AdsMediation.IronSourceHelper
{
    public class InterstitialAdObject
    {
        public AdPlacement.Type adPlacementType;
        public AdsManager.InterstitialDelegate onAdLoaded;
        public AdsManager.InterstitialDelegate onAdClosed;
        public AdObjectState state;

        public InterstitialAdObject()
        {
        }

        public InterstitialAdObject(AdPlacement.Type adPlacementType, AdsManager.InterstitialDelegate onAdLoaded)
        {
            this.adPlacementType = adPlacementType;
            this.onAdLoaded = onAdLoaded;
        }

        public bool CanShow
        {
            get
            {
                return state == AdObjectState.Ready;
            }
        }
    }
}