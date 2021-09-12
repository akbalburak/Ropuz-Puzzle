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

    [Header("Level detail panel.")]
    public GameObject LevelDetailPanel;

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

    public void OnClickChangeLanguage()
    {
        // We are showing the language view.
        GameViewController.Instance.ActivateView(GameViews.Language);
    }

    public void OnClickLevelMenuView()
    {
        GameViewController.Instance.ActivateView(GameViews.LevelMenu);
    }

    public void OnClickNewLevel()
    {
        GameViewController.Instance.ActivateView(GameViews.LevelEditor);
    }

    public void LoadLevelDetails(int customLevel)
    {
        // We also activate the action panel.
        LevelDetailPanel.SetActive(true);

        // We get the level data.
        LevelEditorModel levelData = GameController.Instance.CustomLevels[customLevel - 1];

        // Get the level name.
        LevelDetailPanel.transform.Find("TXTLevelName").GetComponent<TMP_Text>().text = levelData.LevelName;

        // Get the level texture.
        RawImage rawImage = LevelDetailPanel.transform.Find("LevelImage").GetComponent<RawImage>();

        // We bind the texture.
        rawImage.texture = levelData.GetTexture();

        // We use texture size.
        rawImage.SetNativeSize();

        // Width and height rate of image to prevent all images become square.
        float whRate = rawImage.rectTransform.sizeDelta.y / rawImage.rectTransform.sizeDelta.x;

        // We calculate the x size of image.
        float imageXSize = Mathf.Clamp(rawImage.rectTransform.sizeDelta.x, 0, 600);

        // We calculate the y size with x ratio.
        float imageYSize = Mathf.Clamp(rawImage.rectTransform.sizeDelta.y, 0, 600 * whRate);

        // We make sure it is not big enough.
        rawImage.rectTransform.sizeDelta = new Vector2(imageXSize, imageYSize);

        // Play button to start game.
        Button playButton = LevelDetailPanel.transform.Find("BTNPlay").GetComponent<Button>();

        // We remove previous listeners.
        playButton.onClick.RemoveAllListeners();

        // We add the action.
        playButton.onClick.AddListener(() =>
        {
            // We activate the level.
            GameController.Instance.ActivateLevel(LevelStates.UserDefined, customLevel);
        });

        // Edit button to update level.
        Button editButton = LevelDetailPanel.transform.Find("BTNEdit").GetComponent<Button>();

        // We remove previous listeners.
        editButton.onClick.RemoveAllListeners();

        // We add action to edit button.
        editButton.onClick.AddListener(() =>
        {
            // We close the panel.
            LevelDetailPanel.SetActive(false);

            // We activate the level editor.
            LevelDesignerGameViewController levelDesigner = (LevelDesignerGameViewController)GameViewController.Instance.ActivateView(GameViews.LevelEditor);

            // Then we will add data to 
            levelDesigner.UpdateModel = levelData;

            // We set the texture to update.
            levelDesigner.CurrentSelectedTexture = levelData.GetTexture();

            // We update col quantity.
            levelDesigner.SLDColCount.SetValueWithoutNotify(levelData.ColCount);

            // We update row quantity.
            levelDesigner.SLDRowCount.SetValueWithoutNotify(levelData.RowCount);

            // We also set size.
            levelDesigner.DDSizes.SetValueWithoutNotify(levelData.Size);

            // We init designer.
            levelDesigner.InitializeDesigner();

            // We put the level name.
            levelDesigner.TXTLevelName.text = levelData.LevelName;

            // Is randomized?
            levelDesigner.IsAlwaysRandom.SetIsOnWithoutNotify(levelData.AlwaysRandom);

        });

        // Remove button.
        Button removeButton = LevelDetailPanel.transform.Find("BTNDeleteApprove").GetComponent<Button>();

        // We clear older listeners.
        removeButton.onClick.RemoveAllListeners();

        // We add new remove function.
        removeButton.onClick.AddListener(() =>
        {
            // We remove the level data.
            GameController.Instance.RemoveLevel(levelData);

            // We disable the view.
            LevelDetailPanel.SetActive(false);

            // We set remove button disabled.
            removeButton.gameObject.SetActive(false);
        });
    }
}
