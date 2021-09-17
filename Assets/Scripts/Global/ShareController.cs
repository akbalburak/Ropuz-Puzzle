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

    public void Share(Texture2D texture, string content, string url)
    {
        new NativeShare()
            .SetSubject("").SetText(content).SetUrl(url).AddFile(texture)
            .SetCallback((result, shareTarget) => Debug.Log("Share result: " + result + ", selected app: " + shareTarget))
            .Share();
    }
}
