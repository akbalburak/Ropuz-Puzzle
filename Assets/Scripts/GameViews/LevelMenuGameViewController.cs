using UnityEngine;
using UnityEngine.UI;

public class LevelMenuGameViewController : MonoBehaviour, IGameViewPanel
{
    public void OnGameViewActivated()
    {
        // We are loading required informations.
        InitializeLevels();
    }

    public void OnGameViewDeactivated()
    {
    }

    [Header("Toggle for the sound.")]
    public Toggle BTNSound;

    [Header("Toggle for the music.")]
    public Toggle BTNMusic;

    [Header("All the level buttons.")]
    public Transform BTNLevels;

    public void InitializeLevels()
    {
        // NOTE : We assuming all the levels are ordered by ascending.
        // We are rotating all the levels.
        foreach (Transform level in BTNLevels)
        {
            // Index plus one giving us the level.
            int levelIndex = level.GetSiblingIndex() + 1;

            // Level button to bind actions.
            Button levelButton = level.GetComponent<Button>();

            // if the level is smaller than or equals to the maximum level user reach then the button will be active.
            if (levelIndex <= SaveLoadController.Instance.SaveData.MaxReachedLevel)
                levelButton.interactable = true;
            else // Otherwise we are going to close the button.
                levelButton.interactable = false;

            // When player click the button.
            levelButton.onClick.AddListener(() =>
            {
                // We activate the level.
                GameController.Instance.ActivateLevel(LevelStates.SystemDefined, levelIndex);
            });
        }

        // Button is going to be on if sound is active. Otherwise not.
        BTNSound.isOn = !SaveLoadController.Instance.SaveData.IsSoundActive;

        // Button is going to be on if music is active. Otherwise not.
        BTNMusic.isOn = !SaveLoadController.Instance.SaveData.IsMusicActive;
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

    public void OnClickChangeLanguage()
    {
        // We are showing the language view.
        GameViewController.Instance.ActivateView(GameViews.Language);
    }

    public void OnClickCustomLevelMenuView()
    {
        GameViewController.Instance.ActivateView(GameViews.CustomLevelMenu);
    }
}
