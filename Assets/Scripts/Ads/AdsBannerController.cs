using GoogleMobileAds.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdsBannerController : MonoBehaviour
{
    [Header("Is banner going to be enabled on startup.")]
    public bool IsBannerActive;

    [Header("Banner unique id for android.")]
    public string AndroidUnitId;

    [Header("Banner unique id for ios.")]
    public string IosUnitId;

    [Header("Banner position.")]
    public AdPosition AdPosition;

    private BannerView bannerView;

    public void Start()
    {
        // if banner is not active just return.
        if (!IsBannerActive)
            return;

        // We are activating the banner.
        this.RequestBanner();

        // Will be triggered when the ads is loaded.
        this.bannerView.OnAdLoaded += BannerView_OnAdLoaded;

        // We are listining to failed state.
        this.bannerView.OnAdFailedToLoad += BannerView_OnAdFailedToLoad;
    }

    private void BannerView_OnAdLoaded(object sender, EventArgs e)
    {
        // We are showing the banner.   
        this.bannerView.Show();
    }

    private void BannerView_OnAdFailedToLoad(object sender, AdFailedToLoadEventArgs e)
    {
        // if we failed we create a new banner request.
        RequestBanner();
    }

    private void RequestBanner()
    {
        // if banner is not active just return.
        if (!IsBannerActive)
            return;

#if UNITY_ANDROID
        string adUnitId = AndroidUnitId;
#elif UNITY_IPHONE
            string adUnitId = IosUnitId;
#else
            string adUnitId = "unexpected_platform";
#endif

        // Create a 320x50 banner at the top of the screen.
        this.bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition);

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();

        // Load the banner with the request.
        this.bannerView.LoadAd(request);
    }
}
