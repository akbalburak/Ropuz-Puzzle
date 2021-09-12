using GoogleMobileAds.Api;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class AdsRewardController : MonoBehaviour
{
    /// <summary>
    /// Called when user should be rewarded.
    /// </summary>
    public UnityEvent<Reward> OnRewardActivated;

    [Header("Is rewarded ads are active in game.")]
    public bool IsRewardAdsActive;

    [Header("Android reward unit id.")]
    public string AndroidRewardUnitId;

    [Header("Ios reward unit id.")]
    public string IosRewardUnitId;

    private RewardedAd rewardedAd;

    private void Start()
    {
        // if not acitve just return.
        if (!IsRewardAdsActive)
            return;

#if UNITY_ANDROID
        string adUnitId = AndroidRewardUnitId;
#elif UNITY_IPHONE
            string adUnitId = IosRewardUnitId;
#else
            string adUnitId = "unexpected_platform";
#endif

        // We are creating the rewarded ad.
        this.rewardedAd = new RewardedAd(adUnitId);

        // Called when the user should be rewarded for interacting with the ad.
        this.rewardedAd.OnUserEarnedReward += (sender, args) => OnRewardActivated.Invoke(args);
    }

    public IEnumerator ShowRewardedAd()
    {
        // if there is no rewarded ads just break.
        if (!IsRewardAdsActive)
            yield break;

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();

        // Load the rewarded ad with the request.
        this.rewardedAd.LoadAd(request);

        // We make sure ads is loaded.
        yield return new WaitUntil(() => this.rewardedAd.IsLoaded());

        // We are showing the rewarded ads.
        this.rewardedAd.Show();

    }

}
