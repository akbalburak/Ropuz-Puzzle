using Assets.Scripts.Models;
using System.Collections.Generic;
using UnityEngine;

public class GameHistoryController : MonoBehaviour
{
    public static GameHistoryController Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    /// <summary>
    /// Return the config file name to store each level in a diffrent key.
    /// </summary>
    /// <param name="model">Level informations.</param>
    /// <param name="difficulity">Level difficulity to seperate caches.</param>
    /// <returns></returns>
    public string GetConfigFileName(LevelEditorModel model, GameDifficulities difficulity) => $"{model.ConfigFileName}_{difficulity}";

    /// <summary>
    /// Check is there a play history to recover.
    /// </summary>
    /// <param name="model">Level informations.</param>
    /// <param name="difficulity">Level difficulity to seperate caches.</param>
    /// <returns></returns>
    public bool CheckIsHistoryExists(LevelEditorModel model, GameDifficulities difficulity) => PlayerPrefs.HasKey(GetConfigFileName(model, difficulity));

    /// <summary>
    /// We get history to recovery level.
    /// </summary>
    /// <param name="model">Game level pieces.</param>
    /// <param name="difficulity">Level difficulity to seperate caches.</param>
    /// <returns></returns>
    public GameHistoryModel GetHistory(LevelEditorModel model, GameDifficulities difficulity) => JsonUtility.FromJson<GameHistoryModel>(PlayerPrefs.GetString(GetConfigFileName(model, difficulity)));

    /// <summary>
    /// Update given model with given pieces.
    /// </summary>
    /// <param name="model">Model informations like level row and column indexes.</param>
    /// <param name="pieces">Game level pieces.</param>
    public void SetHistory(LevelEditorModel model, GameDifficulities difficulity, List<GameHistoryPieceModel> pieces)
    {
        // Json value of playground.
        string jsonValue = JsonUtility.ToJson(new GameHistoryModel { HistoryPieces = pieces });

        // Set the value to store.
        PlayerPrefs.SetString(GetConfigFileName(model, difficulity), jsonValue);
    }

    /// <summary>
    /// Remove game history.
    /// </summary>
    /// <param name="model">Model informations like level row and column indexes.</param>
    /// <param name="difficulity">Game level pieces.</param>
    public void RemoveHistory(LevelEditorModel model, GameDifficulities difficulity) => PlayerPrefs.DeleteKey(GetConfigFileName(model, difficulity));
}
