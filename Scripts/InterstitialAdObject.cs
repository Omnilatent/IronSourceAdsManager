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
        public bool ready;

        public InterstitialAdObject()
        {
        }

        public InterstitialAdObject(AdPlacement.Type adPlacementType, AdsManager.InterstitialDelegate onAdLoaded)
        {
            this.adPlacementType = adPlacementType;
            this.onAdLoaded = onAdLoaded;
        }
    }
}