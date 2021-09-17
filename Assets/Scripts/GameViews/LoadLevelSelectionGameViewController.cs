using TMPro;
using UnityEngine;

public class LoadLevelSelectionGameViewController : MonoBehaviour, IGameViewPanel
{
    [Header("Level url we want to use.")]
    public TMP_InputField INPLevelUrl;

    public void OnClickLoad()
    {
        // We are loading the level.
        DeepLinkController.Instance.OnDeepLinkActivated(INPLevelUrl.text);
    }

    public void OnClickPaste()
    {
        // We paste the text.
        INPLevelUrl.text = GUIUtility.systemCopyBuffer;
    }

    public void OnGameViewActivated()
    {
    }

    public void OnGameViewDeactivated()
    {
    }
}
