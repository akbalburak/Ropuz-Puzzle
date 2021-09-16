using Assets.Scripts.Models;
using System.IO;
using UnityEngine;

public class ShareController : MonoBehaviour
{
    public static ShareController Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    [Header("Sharing prefix.")]
    public string SharingUrl;

    public string GetAsShareLink(LevelEditorModel levelData)
    {
        // We get the filename.
        string fileName = new FileInfo(levelData.ConfigFileName).Name;

        // We return the share link.
        return $"{SharingUrl}{fileName}";
    }

    public void Share(string title, string content, string url)
    {
        new NativeShare()
            .SetSubject(title).SetText(content).SetUrl(url)
            .SetCallback((result, shareTarget) => Debug.Log("Share result: " + result + ", selected app: " + shareTarget))
            .Share();
    }
}
