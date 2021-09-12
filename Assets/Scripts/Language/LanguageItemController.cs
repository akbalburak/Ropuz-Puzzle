using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LanguageItemController : MonoBehaviour
{
    [Header("Supported language items.")]
    public List<LanguageItemModel> LanguageItems;

    private void Start() => RefreshText();

    public void RefreshText()
    {
        // We are searching translated value for the current language.
        LanguageItemModel currentLanguage = LanguageItems.Find(x => x.Language == SaveLoadController.Instance.SaveData.Language);

        // We are looking for the content.
        TMP_Text content = GetComponent<TMP_Text>();

        // if content not exists return.
        if (!content)
            return;

        // if language data exists.
        if (currentLanguage != null)
            content.text = currentLanguage.Value;
        else // if value not exists.
        {
            // We are going to search for default value.
            LanguageItemModel defaultLanguage = LanguageItems.Find(x => x.Language == LanguageController.Instance.DefaultLangauge);

            // We make sure default language exists.
            if (defaultLanguage != null)
                content.text = defaultLanguage.Value;
        }
    }
}
