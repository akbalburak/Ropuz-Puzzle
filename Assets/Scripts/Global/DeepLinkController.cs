using Firebase.DynamicLinks;
using Firebase.Extensions;
using System;
using System.IO;
using TMPro;
using UnityEngine;

public class DeepLinkController : MonoBehaviour
{
    public static DeepLinkController Instance { get; private set; }

    private void Awake()
    {
        // Singleton pattern.
        if (Instance == null)
        {
            // Set the instance to make singleton.
            Instance = this;

            // We prevent to close.
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [Header("Main deeplink url.")]
    public string Url = "https://ropuzpuzzle.page.link";

    [Header("Android app url to redirect if not exists.")]
    public string AndroidAppUrl = "com.abbasgamestudio.RopuzBulmaca";

    void Start()
    {
        DynamicLinks.DynamicLinkReceived += OnDynamicLink;
    }

    // Display the dynamic link received by the application.
    void OnDynamicLink(object sender, EventArgs args)
    {
        ReceivedDynamicLinkEventArgs dynamicLinkEventArgs = args as ReceivedDynamicLinkEventArgs;
        OnDeepLinkActivated(dynamicLinkEventArgs.ReceivedDynamicLink.Url.ToString());
    }

    public void OnDeepLinkActivated(string url)
    {
        // Decode the URL to determine action. 
        // In this example, the app expects a link formatted like this:
        // unitydl://ropuzbulmaca?dosyaadi
        string[] parameters = url.Split("/"[0]);

        // if no parameter exists just return back.
        if (parameters.Length == 0)
            return;

        // We receive the first parameter as a file url
        string fileName = parameters[parameters.Length - 1];

        // We are trying to download and activate level.
        StartCoroutine(GameController.Instance.DownloadAndActivateLevel(fileName));
    }

    public void CreateADeepLink(string filename, Action<Uri> onReceivedDeepLink)
    {
        // We get the filename.
        filename = new FileInfo(filename).Name;

        // We add also the application link.
        DynamicLinkComponents components = new DynamicLinkComponents(new System.Uri($"{Url}/{filename}"), Url)
        {
            AndroidParameters = new AndroidParameters(AndroidAppUrl),
            
        };

        DynamicLinkOptions options = new DynamicLinkOptions
        {
            PathLength = DynamicLinkPathLength.Unguessable
        };

        // We get the short link.
        DynamicLinks.GetShortLinkAsync(components, options).ContinueWithOnMainThread((t) =>
         {
             ShortDynamicLink result = t.Result;

             // if succeed we return the url.
             if (t.IsCompleted)
                 onReceivedDeepLink(result.Url);
             else
                 onReceivedDeepLink(null);
         });
    }
}
