using UnityEngine;

public class SaveLoadController : MonoBehaviour
{
    public static SaveLoadController Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // We have to make sure save prog loaded.
        Load();
    }

    [Header("When a save not exists, default save data will be loaded.")]
    public SaveLoadModel DefaultSaveData;

    [Header("Keyword for saving.")]
    public string KEYWORD = $"ROPUZ_KAYIT";

    /// <summary>
    /// Player save state. Only must be read from outside.
    /// To save, must be call save method.
    /// </summary>
    public SaveLoadModel SaveData { get; private set; }

    /// <summary>
    /// Saves the user progress to the prefs.
    /// </summary>
    public void Save()
    {
        PlayerPrefs.SetString(this.KEYWORD, JsonUtility.ToJson(SaveData));
    }

    /// <summary>
    /// Load user progress from prefs. if not exists load default.
    /// </summary>
    public void Load()
    {
        // We are searching in prefabs.
        string lastSaveData = PlayerPrefs.GetString(KEYWORD, string.Empty);

        // We have to make sure the save state exists.
        if (!string.IsNullOrEmpty(lastSaveData))
            this.SaveData = JsonUtility.FromJson<SaveLoadModel>(lastSaveData);
        else // if no exists we have to return a default config and we have to save it.
        {
            // We are setting default save data.
            this.SaveData = DefaultSaveData;

            // And saving the new save state.
            this.Save();
        }
    }

    /// <summary>
    /// Clear user progress from prefs.
    /// </summary>
    public void Clear()
    {
        // We are deleting user progress.
        PlayerPrefs.DeleteKey(this.KEYWORD);

        // To prevent error we loading default save data.
        if (this.SaveData == null)
            this.SaveData = DefaultSaveData;
    }

}
