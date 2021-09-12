using GoogleMobileAds.Api;
using System.Collections;
using UnityEngine;

public class AdsInterstitialController : MonoBehaviour
{
    [Header("We are checking insterstitial ads enabled.")]
    public bool IsInterstitialEnabled;

    [Header("Interstitial unit id for the android.")]
    public string AndroidInterstitialUnitId;

    [Header("Interstitial unit id for the ios.")]
    public string IosInterstitialUnitId;

    private InterstitialAd interstitial;

    // Start is called before the first frame update
    void Start()
    {
        // if interstitial not enabled just reutnr.
        if (!IsInterstitialEnabled)
            return;

        // We are 
        RequestInterstitial();
    }

    private void RequestInterstitial()
    {   
        // if interstitial not enabled just reutnr.
        if (!IsInterstitialEnabled)
            return;

        #if UNITY_ANDROID
            string adUnitId = AndroidInterstitialUnitId;
        #elif UNITY_IPHONE
            string adUnitId = IosInterstitialUnitId;
        #else
            string adUnitId = "unexpected_platform";
        #endif

        // Initialize an InterstitialAd.
        this.interstitial = new InterstitialAd(adUnitId);

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();

        // Load the interstitial with the request.
        this.interstitial.LoadAd(request);
    }

    public IEnumerator ShowInterstitial()
    {
        // if interstitial not enabled just reutnr.
        if (!IsInterstitialEnabled)
            yield break;

        // We are waiting untill intiali loaded.
        yield return new WaitUntil(() => this.interstitial.IsLoaded());

        // if loaded we show the interstitial.
        this.interstitial.Show();

        // We are requesting new interstitial.
        RequestInterstitial();
    }
}
