using System;
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

            // When deep link activated event will be triggered.
            Application.deepLinkActivated += OnDeepLinkActivated;

            // We prevent to close.
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {

        // if url is not empty then means there is a coming data we have to execute.
        if (!String.IsNullOrEmpty(Application.absoluteURL))
            OnDeepLinkActivated(Application.absoluteURL);
    }

    public void OnDeepLinkActivated(string url)
    {
        // Decode the URL to determine action. 
        // In this example, the app expects a link formatted like this:
        // unitydl://ropuzbulmaca?dosyaadi
        string[] parameters = url.Split("?"[0]);

        // if no parameter exists just return back.
        if (parameters.Length == 0)
            return;

        // We receive the first parameter as a file url
        string fileName = parameters[1];

        // We are trying to download and activate level.
        StartCoroutine(GameController.Instance.DownloadAndActivateLevel(fileName));
    }
}
