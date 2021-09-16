using Assets.Scripts.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomLevelMenuGameViewController : MonoBehaviour, IGameViewPanel
{
    public static CustomLevelMenuGameViewController Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    [Header("Toggle for the sound.")]
    public Toggle BTNSound;

    [Header("Toggle for the music.")]
    public Toggle BTNMusic;

    [Header("All the custom level buttons.")]
    public Transform BTNLevels;

    [Header("Level detail view.")]
    public CustomLevelDetailViewController LevelDetailView;

    private void Initialize()
    {
        // Button is going to be on if sound is active. Otherwise not.
        BTNSound.isOn = !SaveLoadController.Instance.SaveData.IsSoundActive;

        // Button is going to be on if music is active. Otherwise not.
        BTNMusic.isOn = !SaveLoadController.Instance.SaveData.IsMusicActive;

        // We reload the custom levels.
        GameController.Instance.LoadCustomLevels();

        // NOTE : We assuming all the levels are ordered by ascending.
        // We are rotating all the levels.
        foreach (Transform level in BTNLevels)
        {
            // Index plus one giving us the level.
            int levelIndex = level.GetSiblingIndex() + 1;

            // Level button to bind actions.
            Button levelButton = level.GetComponent<Button>();

            // if there is a custome level for index.
            levelButton.interactable = levelIndex <= GameController.Instance.CustomLevels.Count;

            // We have to remove previous listeners.
            levelButton.onClick.RemoveAllListeners();

            // When player click the button.
            levelButton.onClick.AddListener(() => LoadLevelDetails(levelIndex));
        }

    }

    public void OnGameViewActivated()
    {
        Initialize();
    }

    public void OnGameViewDeactivated()
    {
    }

    public void OnSoundValueChanged(bool isOn)
    {
        // Set the sound state.
        SaveLoadController.Instance.SaveData.IsSoundActive = !isOn;

        // We make sure we saved the settings.
        SaveLoadController.Instance.Save();

        // We are refreshing the game sound and music stats.
        AudioController.Instance.RefreshState();
    }

    public void OnMusicValueChanged(bool isOn)
    {
        // Set the music state.
        SaveLoadController.Instance.SaveData.IsMusicActive = !isOn;

        // We make sure we saved the settings.
        SaveLoadController.Instance.Save();

        // We are refreshing the game sound and music stats.
        AudioController.Instance.RefreshState();
    }

    public void OnClickPolicy()
    {
        Application.OpenURL(GameController.Instance.PolicyUrl);
    }

    public void OnClickRateUs()
    {
        Application.OpenURL(GameController.Instance.GooglePlayUrl);
    }

    public void OnClickNewLevel()
    {
        GameViewController.Instance.ActivateView(GameViews.LevelEditor);
    }

    public void LoadLevelDetails(int customLevel)
    {
        // We get the level data.
        LevelEditorModel levelData = GameController.Instance.CustomLevels[customLevel - 1];

        // We also activate the action panel.
        LevelDetailView.Show(levelData);
    }
}
