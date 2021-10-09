using UnityEngine;
using Assets.Scripts.Models;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;
using Assets.Scripts.Extends;

public class SliderPlaygroundController : MonoBehaviour
{
    public static SliderPlaygroundController Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    [Header("When the level is completed.")]
    public bool IsFinalized;

    /// <summary>
    /// Level information data.
    /// </summary>
    public LevelEditorModel LevelData { get; private set; }

    [Header("Seviye ipucu.")]
    public LevelHintController LHC;

    [Header("Pieces sliding speed.")]
    public float SpeedOfSliding;

    [Header("Slider piece prefab.")]
    public GameObject GOSliderItem;

    [Header("Slider playground items in scene.")]
    public List<SliderPlaygroundItemController> SliderPlaygroundItems;
    private List<Tuple<int, int>> orderedIndex = new List<Tuple<int, int>>();

    /// <summary>
    /// Returns the empty piece.
    /// </summary>
    public SliderPlaygroundItemController GetEmptyPiece => SliderPlaygroundItems?.Find(x => !x.gameObject.activeInHierarchy);

    public void LoadLevel(LevelEditorModel levelData, Texture2D levelTexture)
    {
        // We set the level data.
        this.LevelData = levelData;

        // Slider item content.
        Transform sliderItemContent = transform.Find("Items");

        // We get the position of slider pieces parent.
        Vector2 sliderPiecesParentPosition = sliderItemContent.GetComponent<RectTransform>().anchoredPosition;

        // Unique index number.
        int i = 0;

        // We initiate all the rows.
        for (int r = 0; r < this.LevelData.RowCount; r++)
        {
            // We initiate all the cols.
            for (int c = 0; c < this.LevelData.ColCount; c++)
            {
                // We increase the unique index of pieces.
                i++;

                // Slider piece.
                GameObject sliderItem = Instantiate(GOSliderItem, sliderItemContent);

                // Slider location component.
                RectTransform rectTraOfSlider = sliderItem.GetComponent<RectTransform>();

                // Position of piece.
                Vector2 position = GetPositionInGrid(c, r);

                // We set the sizes.
                rectTraOfSlider.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, this.LevelData.Size);
                rectTraOfSlider.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, this.LevelData.Size);

                // We set the piece position.
                rectTraOfSlider.anchoredPosition = position;

                // Offset texture start position.
                float offsetXTexture = -levelTexture.width / 2, offsetYTexture = -levelTexture.height / 2;

                // Parse texture for the current grid.
                float textureXSize = c * this.LevelData.Size + this.LevelData.Size / 2, textureYSize = r * this.LevelData.Size + this.LevelData.Size / 2;

                // We set the piece texture with offset.
                RawImage pieceBack = sliderItem.transform.Find("Puzzle/Back").GetComponent<RawImage>();
                pieceBack.texture = levelTexture;
                pieceBack.GetComponent<RectTransform>().anchoredPosition = new Vector2(offsetXTexture + textureXSize, offsetYTexture + textureYSize);
                pieceBack.SetNativeSize();

                // We get the slider item comp.
                SliderPlaygroundItemController sliderItemComp = sliderItem.GetComponent<SliderPlaygroundItemController>();
                sliderItemComp.Load(i, c, r);

                // We add the level pieces list.  
                SliderPlaygroundItems.Add(sliderItemComp);
            }
        }

        // Succeed layout.
        orderedIndex = SliderPlaygroundItems.Select(x => new Tuple<int, int>(x.Col, x.Row)).ToList();

        // Piece to remove.
        SliderPlaygroundItemController removedPiece = SliderPlaygroundItems.FirstOrDefault();

        // if piece exists we remove it.
        if (removedPiece)
            removedPiece.gameObject.SetActive(false);

        // We shuffle the playground items.
        List<Tuple<int, int>> randomizedPlaygroundItems = SliderPlaygroundItems.OrderBy(x => Guid.NewGuid()).Select(x => new Tuple<int, int>(x.Col, x.Row)).ToList();

        // We change the positions of playground items.
        SliderPlaygroundItems.ForEach(e =>
        {
            // We get the index in current list.
            int indexInCurrent = SliderPlaygroundItems.IndexOf(e);

            // We get the target position.
            Tuple<int, int> targetPiece = randomizedPlaygroundItems[indexInCurrent];

            // We set the new col and row index.
            e.Col = targetPiece.Item1;
            e.Row = targetPiece.Item2;

            // We set the new position.
            e.RectTransform.anchoredPosition = GetPositionInGrid(e.Col, e.Row);
        });
    }

    public Vector2 GetPositionInGrid(int c, int r)
    {
        // We get the position of slider pieces parent.
        Vector2 sliderPiecesParentPosition = GetComponent<RectTransform>().anchoredPosition;

        // We offset as much as required from center.
        float leftBorder = sliderPiecesParentPosition.x - (this.LevelData.Size * this.LevelData.ColCount) / (float)2;
        float bottomBorder = sliderPiecesParentPosition.y - (this.LevelData.Size * this.LevelData.RowCount) / (float)2;

        // We get the offset of texture..
        float posX = leftBorder + this.LevelData.Size * c;
        float posY = bottomBorder + this.LevelData.Size * r;

        // We return the position.
        return new Vector2(posX, posY);
    }

    public void UpdateEmptyGridData(int c, int r)
    {
        // We look for the empty piece.
        SliderPlaygroundItemController emptyPiece = GetEmptyPiece;

        // We update piece position.
        emptyPiece.RectTransform.anchoredPosition = GetPositionInGrid(c, r);

        // We update the empty position.
        emptyPiece.Col = c;
        emptyPiece.Row = r;
    }

    public void CheckForFinalization()
    {
        // if finalized just return.
        if (this.IsFinalized)
            return;

        // We reduce the action score.
        SaveLoadController.Instance.SaveData.ActionScore--;
        SaveLoadController.Instance.Save();
        CurrentLevelGameViewController.Instance.RefreshUI();

        // Level pieces.
        List<GameHistoryPieceModel> pieces = SliderPlaygroundItems.Select(x => new GameHistoryPieceModel(x.PieceIndex, x.Col, x.Row)).ToList();

        // We remove history if the level completed.
        GameHistoryController.Instance.SetHistory(this.LevelData, GameDifficulities.SliderPuzzle, pieces);

        // We check is level completed.
        if (Enumerable.SequenceEqual(SliderPlaygroundItems.Select(x => new Tuple<int, int>(x.Col, x.Row)), orderedIndex))
        {
            // Set as finalized.
            IsFinalized = true;

            // We update piece.
            SliderPlaygroundItems.ForEach(e => e.gameObject.SetActive(true));

            // Current level information.
            LevelEditorModel currentLevelData = CurrentLevelGameViewController.Instance.LevelData;

            // We are updating max level if it is smaller.
            if (CurrentLevelGameViewController.Instance.CurrentLevel == SaveLoadController.Instance.SaveData.MaxReachedLevel)
                SaveLoadController.Instance.SaveData.MaxReachedLevel = CurrentLevelGameViewController.Instance.CurrentLevel + 1;

            // We are reducing one point.
            SaveLoadController.Instance.SaveData.ActionScore += currentLevelData.ScoreOnWin;

            // We are applying changes.
            SaveLoadController.Instance.Save();

            // And we are reducing from the ui.
            CurrentLevelGameViewController.Instance.RefreshUI();

            // We are updating buttons states.
            CurrentLevelGameViewController.Instance.CheckPrevAndNextButtons();

            // We remove history if the level completed.
            GameHistoryController.Instance.RemoveHistory(this.LevelData, GameDifficulities.SliderPuzzle);
        }
    }

    public void ContinueToLevel()
    {
        // We get the history data.
        GameHistoryModel historyValue = GameHistoryController.Instance.GetHistory(this.LevelData, GameDifficulities.SliderPuzzle);

        // We loop all the items.
        foreach (GameHistoryPieceModel piece in historyValue.HistoryPieces)
        {
            // We are looking for the same piece from previous level.
            SliderPlaygroundItemController pieceInLevel = SliderPlaygroundItems.Find(x => x.PieceIndex == piece.PieceIndex);

            // We make sure piece exists in map.
            if (pieceInLevel != null)
            {
                // We load the piece position from the cache.
                pieceInLevel.RectTransform.anchoredPosition = GetPositionInGrid(piece.Col, piece.Row);

                // We set the data.
                pieceInLevel.Load(piece.PieceIndex, piece.Col, piece.Row);
            }
        }
    }

}
