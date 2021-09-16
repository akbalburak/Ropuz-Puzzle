using Assets.Scripts.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    [Header("User defined custom levels")]
    public List<LevelEditorModel> CustomLevels = new List<LevelEditorModel>();

    [Header("System defined levels.")]
    public List<LevelEditorModel> SystemLevels = new List<LevelEditorModel>();

    [Header("Rotation speed of images.")]
    public float RotationSpeed;

    [Header("Rotation quantity at the sametime. Only 4 supported for now.")]
    public int RotationQuantity = 4;

    [Header("Policy url when user click the policy.")]
    public string PolicyUrl;

    [Header("When user click rate us redirect url")]
    public string GooglePlayUrl;

    [Header("Loading button while game is loading.")]
    public GameObject GOLoading;

    private void Start()
    {
        // We order with order index.
        this.SystemLevels = this.SystemLevels.OrderBy(x => x.OrderIndex).ToList();
    }

    public void LoadCustomLevels()
    {
        // We load the configuration files.
        this.CustomLevels = FileBrowserController.Instance.LoadConfigFiles("*.config").OrderBy(x => x.OrderIndex).ToList();
    }

    public void ActivateLevel(LevelEditorModel levelData, LevelStates levelState, int level)
    {
        // We activate the level.
        CurrentLevelGameViewController currentLevel = (CurrentLevelGameViewController)GameViewController.Instance.ActivateView(GameViews.CurrentLevel);

        // We load the level.
        currentLevel.LoadLevel(levelData, levelState, level);

    }

    public void RemoveLevel(LevelEditorModel model)
    {
        // We remove the config file.
        FileBrowserController.Instance.RemoveFile(model.ConfigFileName);

        // We also remove the image file.
        FileBrowserController.Instance.RemoveFile(model.ImageUrl);

        // We reload custom levels.
        GameController.Instance.LoadCustomLevels();

        // We refresh the view.
        if (CustomLevelMenuGameViewController.Instance != null && CustomLevelMenuGameViewController.Instance.gameObject.activeSelf)
            CustomLevelMenuGameViewController.Instance.OnGameViewActivated();
    }

    public IEnumerator DownloadAndActivateLevel(string fileName)
    {
        // We wait for the end of frame to prevent exceptions like null references.
        yield return new WaitForEndOfFrame();

        // We activating the loading view.
        GOLoading.SetActive(true);

        LevelEditorModel levelConfiguration = null;

        // We download config from the server.
        FirebaseStorageController.Instance.DownloadUrlForConfig($"{fileName}.config", (LevelEditorModel levelConfig) =>
         {
             // We bind the configuration.
             levelConfiguration = levelConfig;
         });

        // We wait until configuration coming.
        yield return new WaitUntil(() => levelConfiguration != null);

        // We wait until level income.
        Texture2D levelTexture = null;

        // We download texture from the server.
        FirebaseStorageController.Instance.DownloadUrlForTexture(levelConfiguration.ImageUrl, (Texture2D texture) =>
        {
            levelTexture = texture;
        });

        // We wait until level is in coming.
        yield return new WaitUntil(() => levelTexture != null);

        // We bind the texture.
        levelConfiguration.RemoteTexture = levelTexture;
        
        // We activating the loading view.
        GOLoading.SetActive(false);

        // We activate the last level.
        ActivateLevel(levelConfiguration, LevelStates.UserDefined, 0);
    }

}
