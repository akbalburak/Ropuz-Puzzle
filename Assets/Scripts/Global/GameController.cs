using Assets.Scripts.Models;
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

    /// <summary>
    /// Active level when player select a level this value will change automatically.
    /// </summary>
    public int CurrentLevel { get; set; }

    /// <summary>
    /// Is this current level is a custom level.
    /// </summary>
    public LevelStates CurrentLevelState { get; set; }

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

    public void ActivateLevel(LevelStates levelState, int level)
    {
        // We set the level to the max level.
        GameController.Instance.CurrentLevel = level;

        // Set as user defined level.
        GameController.Instance.CurrentLevelState = levelState;

        // We activate the level.
        GameViewController.Instance.ActivateView(GameViews.CurrentLevel);
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

}
