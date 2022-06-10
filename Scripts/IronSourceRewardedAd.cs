using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Omnilatent.AdsMediation.IronSourceHelper
{
    public partial class IronSourceAdsManager : MonoBehaviour, IAdsNetworkHelper
    {
        RewardAdObject currentRewardAd;
        Coroutine timeoutLoadRewardCoroutine;
        public static float TIMEOUT_LOADREWARDAD = 12f;

        public static Action<AdPlacement.Type> onRewardedVideoAdOpenedEvent;
        public static Action<AdPlacement.Type, IronSourcePlacement> onRewardedVideoAdClickedEvent;
        public static Action<AdPlacement.Type> onRewardedVideoAdClosedEvent;
        public static Action<AdPlacement.Type, bool> onRewardedVideoAvailabilityChangedEvent;
        public static Action<AdPlacement.Type> onRewardedVideoAdStartedEvent;
        public static Action<AdPlacement.Type> onRewardedVideoAdEndedEvent;
        public static Action<AdPlacement.Type, IronSourcePlacement> onRewardedVideoAdRewardedEvent;
        public static Action<AdPlacement.Type, IronSourceError> onRewardedVideoAdShowFailedEvent;

        void InitRewardAdCallbacks()
        {
            IronSourceEvents.onRewardedVideoAdOpenedEvent += RewardedVideoAdOpenedEvent;
            IronSourceEvents.onRewardedVideoAdClickedEvent += RewardedVideoAdClickedEvent;
            IronSourceEvents.onRewardedVideoAdClosedEvent += RewardedVideoAdClosedEvent;
            IronSourceEvents.onRewardedVideoAvailabilityChangedEvent += RewardedVideoAvailabilityChangedEvent;
            IronSourceEvents.onRewardedVideoAdStartedEvent += RewardedVideoAdStartedEvent;
            IronSourceEvents.onRewardedVideoAdEndedEvent += RewardedVideoAdEndedEvent;
            IronSourceEvents.onRewardedVideoAdRewardedEvent += RewardedVideoAdRewardedEvent;
            IronSourceEvents.onRewardedVideoAdShowFailedEvent += RewardedVideoAdShowFailed;
        }

        RewardAdObject GetCurrentRewardAd(bool makeNewIfNull = true)
        {
            if (currentRewardAd == null)
            {
                Debug.LogError("currentRewardAd is null.");
                if (makeNewIfNull)
                {
                    Debug.Log("New ad will be created");
                    currentRewardAd = new RewardAdObject();
                }
            }
            return currentRewardAd;
        }

        private void RewardedVideoAdOpenedEvent()
        {
            QueueMainThreadExecution(() =>
            {
                onRewardedVideoAdOpenedEvent?.Invoke(GetCurrentRewardAd().adPlacementType);
            });
        }

        private void RewardedVideoAdClickedEvent(IronSourcePlacement iSPlacement)
        {
            QueueMainThreadExecution(() =>
            {
                onRewardedVideoAdClickedEvent?.Invoke(GetCurrentRewardAd().adPlacementType, iSPlacement);
            });
        }

        private void RewardedVideoAdClosedEvent()
        {
            QueueMainThreadExecution(() =>
            {
                onRewardedVideoAdClosedEvent?.Invoke(GetCurrentRewardAd().adPlacementType);
            });
        }

        private void RewardedVideoAvailabilityChangedEvent(bool ready)
        {
            QueueMainThreadExecution(() =>
            {
                onRewardedVideoAvailabilityChangedEvent?.Invoke(GetCurrentRewardAd().adPlacementType, ready);
            });
        }

        private void RewardedVideoAdStartedEvent()
        {
            QueueMainThreadExecution(() =>
            {
                onRewardedVideoAdStartedEvent?.Invoke(GetCurrentRewardAd().adPlacementType);
            });
        }

        private void RewardedVideoAdEndedEvent()
        {
            QueueMainThreadExecution(() =>
            {
                onRewardedVideoAdEndedEvent?.Invoke(GetCurrentRewardAd().adPlacementType);
            });
        }

        private void RewardedVideoAdRewardedEvent(IronSourcePlacement iSPlacement)
        {
            QueueMainThreadExecution(() =>
            {
                GetCurrentRewardAd().onAdClosed?.Invoke(new RewardResult(RewardResult.Type.Finished));
                GetCurrentRewardAd().state = AdObjectState.Shown;
                onRewardedVideoAdRewardedEvent?.Invoke(GetCurrentRewardAd().adPlacementType, iSPlacement);
            });
        }

        private void RewardedVideoAdShowFailed(IronSourceError iSPlacement)
        {
            QueueMainThreadExecution(() =>
            {
                GetCurrentRewardAd().onAdClosed?.Invoke(new RewardResult(RewardResult.Type.Canceled, "Show failed"));
                GetCurrentRewardAd().state = AdObjectState.ShowFailed;
                onRewardedVideoAdShowFailedEvent?.Invoke(GetCurrentRewardAd().adPlacementType, iSPlacement);
            });
        }

        public void Reward(AdPlacement.Type placementType, RewardDelegate onFinish)
        {
            currentRewardAd = new RewardAdObject(placementType, onFinish);
            StartCoroutine(CoReward(placementType, onFinish));
        }

        IEnumerator CoReward(AdPlacement.Type placementType, RewardDelegate onFinish)
        {
            float _timeoutRequestAds = TIMEOUT_LOADREWARDAD;

            GetCurrentRewardAd().state = AdObjectState.Loading;

            float retryInterval = 0.4f;
            WaitForSecondsRealtime delay = new WaitForSecondsRealtime(retryInterval);
            int tryTimes = 0;
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.Log("IronSource ad not reachable " + Application.internetReachability);
                _timeoutRequestAds = 3f;
            }
            while (!IronSource.Agent.isRewardedVideoAvailable() && tryTimes < _timeoutRequestAds / retryInterval)
            {
                yield return delay;
                tryTimes++;
            }
            Debug.Log("IronSource reward ad available:" + IronSource.Agent.isRewardedVideoAvailable());

            if (IronSource.Agent.isRewardedVideoAvailable())
            {
                GetCurrentRewardAd().state = AdObjectState.Showing;
                IronSource.Agent.showRewardedVideo(IronSourceAdID.GetAdID(placementType));
            }
            else
            {
                onFinish?.Invoke(new RewardResult(RewardResult.Type.LoadFailed, "Self timeout"));
            }
            //if (showLoading)
            //    Manager.LoadingAnimation(false);
        }

        /*IEnumerator CoWaitCachedRewardedAdLoad(AdPlacement.Type placementType, RewardDelegate onFinish)
        {
            bool timedOut = false;

            StopCoTimeoutLoadReward();
            timeoutLoadRewardCoroutine = StartCoroutine(CoTimeoutLoadReward(() =>
            {
                timedOut = true;
                GetCurrentRewardAd().state = AdObjectState.LoadFailed;
            }));

            bool loggedLoading = false;
            WaitForSecondsRealtime checkInterval = new WaitForSecondsRealtime(0.1f);
            do
            {
                if (IronSource.Agent.isRewardedVideoAvailable())
                {
                    GetCurrentRewardAd().state = AdObjectState.Ready;
                }
                if (GetCurrentRewardAd().state == AdObjectState.Ready)
                {
                    RewardResult rewardResult = new RewardResult(RewardResult.Type.Canceled);
#if UNITY_EDITOR
                    rewardResult.type = RewardResult.Type.Finished;
#endif
                    GetCurrentRewardAd().onUserEarnedReward += (reward) =>
                    {
                        QueueMainThreadExecution(() =>
                        {
                            rewardResult.type = RewardResult.Type.Finished;
                        });
                    };
                    GetCurrentRewardAd().onAdClosed += (e) =>
                    {
                        QueueMainThreadExecution(() =>
                        {
                            onFinish.Invoke(rewardResult);
                        });
                    };
                    IronSource.Agent.showRewardedVideo(IronSourceAdID.GetAdID(placementType));
                    break;
                }
                else if (GetCurrentRewardAd().state == AdObjectState.LoadFailed)
                {
                    break;
                }
                else if (!timedOut)
                {
                    if (!loggedLoading)
                    {
                        Debug.Log($"No ad of '{placementType}' is ready yet. Wating.");
                        loggedLoading = true;
                    }
                    yield return checkInterval; //TODO: add option to break in case game want to continue instead of waiting for ad ready
                }
            }
            while (GetCurrentRewardAd().state == AdObjectState.Loading && !timedOut);

            StopCoTimeoutLoadReward();

            //No rewardedAd is ready, show message
            if (cacheAdState != CacheAdmobAd.AdStatus.LoadSuccess)
            {
                RewardResult rewardResult;
                if (timedOut)
                {
                    if (cacheAdState == CacheAdmobAd.AdStatus.LoadFailed)
                    {
                        rewardResult = new RewardResult(RewardResult.Type.LoadFailed, AdMobConst.rewardAdSelfTimeoutMsg);
                    }
                    else
                    {
                        rewardResult = new RewardResult(RewardResult.Type.Loading, AdMobConst.loadingRewardAdMsg);
                    }
                }
                else if (cacheAdState == CacheAdmobAd.AdStatus.LoadFailed)
                {
                    rewardResult = new RewardResult(RewardResult.Type.LoadFailed, AdMobConst.adLoadFailCheckConnectionMsg);
                }
                else
                {
                    rewardResult = new RewardResult(RewardResult.Type.Loading, AdMobConst.loadingRewardAdMsg);
                }
                onFinish?.Invoke(rewardResult);
            }
        }*/

        void StopCoTimeoutLoadReward()
        {
            if (timeoutLoadRewardCoroutine != null)
            {
                StopCoroutine(timeoutLoadRewardCoroutine);
                timeoutLoadRewardCoroutine = null;
            }
        }

        IEnumerator CoTimeoutLoadReward(Action onTimeout)
        {
            if (TIMEOUT_LOADREWARDAD > 0f)
            {
                var delay = new WaitForSeconds(TIMEOUT_LOADREWARDAD);
                yield return delay;
            }
            onTimeout.Invoke();
            timeoutLoadRewardCoroutine = null;
        }
    }
}