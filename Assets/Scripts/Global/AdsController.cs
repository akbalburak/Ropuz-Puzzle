using GoogleMobileAds.Api;
using UnityEngine;

public class AdsController : MonoBehaviour
{
    public static AdsController Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // We are activating admob ads.
        MobileAds.Initialize((initState) => { });
    }

    [Header("Banner ads manager.")]
    public AdsBannerController AdsBanner;

    [Header("Full screen ads.")]
    public AdsInterstitialController AdsInterstitial;

    [Header("When rewarded ads activated.")]
    public AdsRewardController AdsReward;
}
