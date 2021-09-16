using Assets.Scripts.Models;
using TMPro;
using UnityEngine;

public class CustomLevelDetailUploadViewController : MonoBehaviour
{
    [Header("Inputfield to print upload url.")]
    public TMP_InputField INPUploadUrl;

    [Header("Url data.")]
    public string Url;

    public void Show(LevelEditorModel levelData)
    {
        // We activate the object.
        this.gameObject.SetActive(true);

        // We bind the url.
        this.Url = ShareController.Instance.GetAsShareLink(levelData);

        // We print the url to share.
        this.INPUploadUrl.text = this.Url;
    }

    public void OnClickShare()
    {
        // We share the puzzle.
        ShareController.Instance.Share($"Ropuz Puzzle", $"Can you solve this puzzle?", this.INPUploadUrl.text);
    }

    public void Close()
    {
        // We disable the game object.
        gameObject.SetActive(false);
    }

    public void OnClickCopy()
    {
        // We copy to clipboard.
        INPUploadUrl.text.CopyToClipboard();
    }
}
