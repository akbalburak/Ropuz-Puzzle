using Assets.Scripts.Models;
using System.Linq;
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
    /// Check is there a play history to recover.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public bool CheckIsHistoryExists(LevelEditorModel model) => PlayerPrefs.HasKey(model.ConfigFileName);

    /// <summary>
    /// We get history to recovery level.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public GameHistoryModel GetHistory(LevelEditorModel model) => JsonUtility.FromJson<GameHistoryModel>(PlayerPrefs.GetString(model.ConfigFileName, ""));

    public void SetHistory(PlaygroundController pc)
    {
        // We get the playgrounds.
        RectTransform[] playGrounds = pc.GridLayoutOfPlayGround.GetComponentsInChildren<RectTransform>();

        // Json value of playground.
        string jsonValue = JsonUtility.ToJson(new GameHistoryModel
        {
            Names = playGrounds.Where(x => int.TryParse(x.name, out int result)).Select(x => x.name).ToList()
        });

        // Set the value to store.
        PlayerPrefs.SetString(pc.LevelData.ConfigFileName, jsonValue);
    }

    public void RemoveHistory(PlaygroundController pc) => PlayerPrefs.DeleteKey(pc.LevelData.ConfigFileName);
}
