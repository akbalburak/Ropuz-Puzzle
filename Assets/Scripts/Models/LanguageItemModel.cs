using System;
using UnityEngine;

[Serializable]
public class LanguageItemModel
{
    [Header("Language identity.")]
    public Languages Language;

    [Header("Translated content")]
    [TextArea(0, 100)]
    public string Value;
}
