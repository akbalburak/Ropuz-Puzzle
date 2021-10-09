using Assets.Scripts.Models;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CurrentLevelGameViewController : MonoBehaviour, IGameViewPanel
{
    #region Singleton

    public static CurrentLevelGameViewController Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    #endregion

    #region General Parameters

    [Header("When user enter the first level we will display the tutorial view.")]
    public GameObject TutorialView;

    [Header("Previous level button.")]
    public Button BTNPreviousLevel;

    [Header("Next level button.")]
    public Button BTNNextLevel;

    [Header("We are going to print current level.")]
    public TMP_Text TXTCurrentLevel;

    [Header("We are going to print to user score.")]
    public TMP_Text TXTUserScore;

    [Header("Continue question when level history exists.")]
    public GameObject GOContinueQuestion;

    [Header("Difficulity panel.")]
    public GameObject AskForDifficulity;

    /// <summary>
    /// Active level when player select a level this value will change automatically.
    /// </summary>
    public int CurrentLevel { get; private set; }

    /// <summary>
    /// Current game difficulity. Easy-> Slider,Normal -> Jigsaw, Hard -> Rotate
    /// </summary>
    public GameDifficulities GameDifficulity { get; private set; }

    /// <summary>
    /// Is this current level is a custom level.
    /// </summary>
    public LevelStates CurrentLevelState { get; private set; }

    /// <summary>
    /// Custom playground manager.
    /// </summary>
    public PlaygroundController CPGC { get; private set; }

    /// <summary>
    /// Shown level data.
    /// </summary>
    public LevelEditorModel LevelData { get; private set; }

    /// <summary>
    /// Shown level texture data.
    /// </summary>
    public Texture2D ShownLevelTexture2D { get; private set; }

    #endregion

    #region General Actions

    public void OnClickBack()
    {
        // We return to the custom level.
        GameViewController.Instance.ActivateView(GameViews.CustomLevelMenu);
    }

    public void OnClickLeave()
    {
        // if system defined game is already playing then return the system levels.
        if (this.CurrentLevelState == LevelStates.SystemDefined)
            GameViewController.Instance.ActivateView(GameViews.LevelMenu);
        else // Otherwise return user defined level page.
            GameViewController.Instance.ActivateView(GameViews.CustomLevelMenu);
    }

    public void RefreshUI()
    {
        // We are printing the current level.
        TXTCurrentLevel.text = $"{this.CurrentLevel}";

        // We are also texting user score.
        TXTUserScore.text = $"{SaveLoadController.Instance.SaveData.ActionScore}";
    }

    public void CheckPrevAndNextButtons()
    {
        // When current level is more than one buton will be activated.
        BTNPreviousLevel.interactable = this.CurrentLevel > 1;

        // We are receiving the max level in the list.
        int maxLevel;

        // if is a custome level then we will get the custom levels count.
        if (CurrentLevelState == LevelStates.UserDefined)
            maxLevel = GameController.Instance.CustomLevels.Count;
        else // otherwise we will receive system levels.
            maxLevel = GameController.Instance.SystemLevels.Count;

        // is user in the max level.
        bool isMaxLevel = this.CurrentLevel >= maxLevel;

        // is user ready to go next level.
        bool isUserReadyToGoNextLevel = true;

        // if current level is system level than we have to ensure level is unlocked.
        if (CurrentLevelState == LevelStates.SystemDefined)
            isUserReadyToGoNextLevel = SaveLoadController.Instance.SaveData.MaxReachedLevel > this.CurrentLevel;

        // We are checking; not to max level and ready to go state.
        BTNNextLevel.interactable = !isMaxLevel && isUserReadyToGoNextLevel;
    }

    public void OnClickNextLevel()
    {
        // We are receiving the max level in the list.
        int maxLevel;

        // if is a custome level then we will get the custom levels count.
        if (CurrentLevelState == LevelStates.UserDefined)
            maxLevel = GameController.Instance.CustomLevels.Count;
        else // otherwise we will receive system levels.
            maxLevel = GameController.Instance.SystemLevels.Count;

        // We are checking the borders; Min level 1 and max level in the list.
        int availableLevel = Mathf.Clamp(this.CurrentLevel + 1, 1, maxLevel);

        // We are going to level.
        this.CurrentLevel = availableLevel;

        // We are activating ads.
        StartCoroutine(AdsController.Instance.AdsInterstitial.ShowInterstitial());

        // Next level model.
        LevelEditorModel nextLevel;

        // We are looking for the next level.
        if (CurrentLevelState == LevelStates.UserDefined)
            nextLevel = GameController.Instance.CustomLevels[this.CurrentLevel - 1];
        else
            nextLevel = GameController.Instance.SystemLevels[this.CurrentLevel - 1];

        // We are activating the level.
        LoadLevel(nextLevel, this.CurrentLevelState, this.CurrentLevel);
    }

    public void OnClickPreviousLevel()
    {
        // We are going previous level.
        int prevLevel = this.CurrentLevel - 1;

        // if previous level is out of index  which means smaller than 1, clamp it.
        if (prevLevel < 1)
            prevLevel = 1;

        // We are going to level.
        this.CurrentLevel = prevLevel;
        // Next level model.
        LevelEditorModel previousLevel;

        // We are looking for the next level.
        if (CurrentLevelState == LevelStates.UserDefined)
            previousLevel = GameController.Instance.CustomLevels[this.CurrentLevel - 1];
        else
            previousLevel = GameController.Instance.SystemLevels[this.CurrentLevel - 1];

        // We are activating the level.
        LoadLevel(previousLevel, this.CurrentLevelState, this.CurrentLevel);
    }

    public void OnClickHint()
    {
        // We are showing the reward.
        StartCoroutine(AdsController.Instance.AdsReward.ShowRewardedAd());
    }

    public void OnClickContinue()
    {
        // We disable the question view.
        GOContinueQuestion.SetActive(false);

        // We continue to level.
        if (GameDifficulity == GameDifficulities.RotatePuzzle)
            CPGC.ContinueToLevel();
        else if (GameDifficulity == GameDifficulities.JigsawPuzzle)
            JP.ContinueToLevel();
        else if (GameDifficulity == GameDifficulities.SliderPuzzle)
            SPC.ContinueToLevel();
    }

    public void OnClickDontContinue()
    {
        // We disable the panel.
        GOContinueQuestion.SetActive(false);

        // We remove the history value.
        GameHistoryController.Instance.RemoveHistory(this.LevelData, this.GameDifficulity);
    }

    public void OnGameViewActivated() { }

    public void OnGameViewDeactivated() { }

    public void LoadLevel(LevelEditorModel levelData, LevelStates levelState, int currentLevel)
    {
        // We delete the older view.
        if (CPGC != null)
            Destroy(CPGC.gameObject);

        // We delete the older jigsaw playground.
        if (JP != null)
            Destroy(JP.gameObject);

        // We delete the older slider game.
        if (SPC != null)
            Destroy(SPC.gameObject);

        // Level state.
        this.CurrentLevelState = levelState;

        // Level information.
        this.CurrentLevel = currentLevel;

        // if a custom level we will load from custom levels of user.
        this.LevelData = levelData;

        // We get the texture.
        ShownLevelTexture2D = LevelData.LoadTextureFromFile();

        // We show dhard difficulity.
        AskForDifficulity.SetActive(true);

        // We check button states again.
        CheckPrevAndNextButtons();
    }

    #endregion

    #region Slider Puzzle

    [Header("Slider game template.")]
    public GameObject SliderPlaygroundTemplate;

    /// <summary>
    /// Jigsaw playground.
    /// </summary>
    public SliderPlaygroundController SPC { get; private set; }

    /// <summary>
    /// On select slider puzzle.
    /// </summary>
    public void OnClickEasy()
    {
        // We set the difficulity.
        GameDifficulity = GameDifficulities.SliderPuzzle;

        // We close the difficulity panel.
        AskForDifficulity.SetActive(false);

        // We create a slider playground.
        this.SPC = Instantiate(SliderPlaygroundTemplate, transform).GetComponent<SliderPlaygroundController>();

        // We load the level details and image.
        this.SPC.LoadLevel(this.LevelData, this.ShownLevelTexture2D);

        // We apply the hint texture.
        this.SPC.LHC.LoadTexture(this.ShownLevelTexture2D, this.LevelData, Vector2.zero);

        // if not continue just shuffle and load level.
        if (GameHistoryController.Instance.CheckIsHistoryExists(this.LevelData, GameDifficulity))
            GOContinueQuestion.SetActive(true);
    }

    #endregion

    #region Jigsaw Puzzle

    [Header("Jigsaw game template.")]
    public GameObject JigsawPlaygroundTemplate;

    /// <summary>
    /// Jigsaw playground.
    /// </summary>
    public JigsawPlayground JP { get; private set; }

    /// <summary>
    /// On select jigsaw puzzle.
    /// </summary>
    public void OnClickJigsawPuzzle()
    {
        // We set the difficulity.
        this.GameDifficulity = GameDifficulities.JigsawPuzzle;

        // We close the difficulity panel.
        this.AskForDifficulity.SetActive(false);

        // We create a jigsaw playground.
        this.JP = Instantiate(JigsawPlaygroundTemplate, transform).GetComponent<JigsawPlayground>();

        // We load the level details and image.
        this.JP.LoadLevel(this.LevelData, this.ShownLevelTexture2D);

        // We apply the hint texture.
        this.JP.LHC.LoadTexture(this.ShownLevelTexture2D, this.LevelData, Vector2.zero);

        // if not continue just shuffle and load level.
        if (GameHistoryController.Instance.CheckIsHistoryExists(this.LevelData, GameDifficulity))
            GOContinueQuestion.SetActive(true);
    }

    #endregion

    #region Rotate Puzzle

    [Header("Playground template we will instantiate.")]
    public GameObject CustomPlaygroundItem;

    [Header("Playground item.")]
    public GameObject CustomLevelPlaygroundItem;

    [Header("Used buttons to rotate objects.")]
    public GameObject CustomLevelRotateItem;

    /// <summary>
    /// On select rotate puzzle.
    /// </summary>
    public void OnClickRotatePuzzle()
    {
        // We set the difficulitiy.
        GameDifficulity = GameDifficulities.RotatePuzzle;

        // We close the difficulity question panel.
        AskForDifficulity.SetActive(false);

        // We are generating the ground item.
        CPGC = Instantiate(CustomPlaygroundItem, transform).GetComponent<PlaygroundController>();

        // We create the size.
        CPGC.GridLayoutOfPlayGround.cellSize = new Vector2(LevelData.Size, LevelData.Size);

        // We set the column count.
        CPGC.GridLayoutOfPlayGround.constraintCount = LevelData.ColCount;

        // We apply the hint texture.
        CPGC.PGHC.LoadTexture(ShownLevelTexture2D, LevelData, CPGC.GridLayoutOfPlayGround.spacing);

        // We activate the level.
        LoadRotategroundGameplay();

        // if not continue just shuffle and load level.
        if (GameHistoryController.Instance.CheckIsHistoryExists(this.LevelData, GameDifficulity))
            GOContinueQuestion.SetActive(true);
    }

    public void LoadRotategroundGameplay()
    {
        // The content we will place pieces.
        Transform piecesGridContent = CPGC.transform.Find("Items");

        // We delete all the older pieces.
        foreach (Transform pieces in piecesGridContent)
            Destroy(pieces.gameObject);

        // Name counter to save level state.
        int nameCounter = 1;

        // Textures are loading from bottom left corner.
        for (int row = LevelData.RowCount - 1; row >= 0; row--)
        {
            // Loop the cols.
            for (int col = 0; col < LevelData.ColCount; col++)
            {
                // Pixel start x position.
                int pixelStartXPosition = col * LevelData.Size;

                // Pixel start y position.
                int pixelStartYPosition = row * LevelData.Size;

                // We will bind image to this playground item.
                GameObject playgroundItem = Instantiate(CustomLevelPlaygroundItem, piecesGridContent);

                // We will apply the data into the playground item.
                RawImage playgroundTexture = playgroundItem.GetComponent<RawImage>();

                // We are receiving the offset pixels.
                Color[] pixels = ShownLevelTexture2D.GetPixels(pixelStartXPosition, pixelStartYPosition, LevelData.Size, LevelData.Size);

                // We are creating the part texture.
                Texture2D texture = new Texture2D(LevelData.Size, LevelData.Size, TextureFormat.RGB24, false);

                // We update the pixels.
                texture.SetPixels(pixels);

                // Then we apply our changes.
                texture.Apply();

                // First step is resize the texture.
                playgroundTexture.texture = texture;

                // Set the player name.
                playgroundItem.transform.name = $"{nameCounter}";

                // We increase the name counter.
                nameCounter++;
            }
        }

        // Döndürmeleri döneceğiz.
        int rotationCount = (LevelData.RowCount - 1) * (LevelData.ColCount - 1);

        // Content where we will put rotators.
        Transform piecesRotatorContent = CPGC.transform.Find("Rotations");

        // We delete all the older pieces.
        foreach (Transform rotations in piecesRotatorContent)
            Destroy(rotations.gameObject);

        // We create the rotation items
        for (int ii = 0; ii < rotationCount; ii++)
            Instantiate(CustomLevelRotateItem, piecesRotatorContent);

        // We update the rotation layout column count.
        piecesRotatorContent.GetComponent<GridLayoutGroup>().constraintCount = LevelData.ColCount - 1;

        // We create the grid system.
        CPGC.LoadPlayGroundGrid(this.LevelData);

        // We save the correct format.
        CPGC.CorrectFormationItems = CPGC.Items.ToList();

        // if random is enabled then we will shuffle it.
        if (LevelData.AlwaysRandom)
            CPGC.ShufflePlayground();
        else // if not enabled we will load with seed value.
            CPGC.ShufflePlayground(LevelData.SeedValue);

        // We update the ui.
        RefreshUI();
    }

    #endregion

}
