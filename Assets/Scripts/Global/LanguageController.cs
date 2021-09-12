using UnityEngine;

public class LanguageController : MonoBehaviour
{
    public static LanguageController Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    [Header("Default game language.")]
    public Languages DefaultLangauge = Languages.English;

    public void ChangeLanguage(Languages language)
    {
        // We are updating language.
        SaveLoadController.Instance.SaveData.Language = language;

        // And we are saving.
        SaveLoadController.Instance.Save();

        // All the changes required language items.
        Object[] languageItems = GameObject.FindObjectsOfType(typeof(LanguageItemController), true);

        // We are reloading all the languages.
        foreach (LanguageItemController languageItem in languageItems)
            languageItem.RefreshText();

    }
}
