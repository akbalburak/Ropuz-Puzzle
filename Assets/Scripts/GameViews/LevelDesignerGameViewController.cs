using Assets.Scripts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelDesignerGameViewController : MonoBehaviour, IGameViewPanel
{
    public static LevelDesignerGameViewController Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    [Header("Upload manager.")]
    public CustomLevelDetailUploadViewController GOUpload;

    [Header("Current selected texture.")]
    public Texture2D CurrentSelectedTexture;

    [Header("Where selected image items will load.")]
    public EditorPlaygroundController EditorPlayground;

    [Header("When an error exist print it.")]
    public TMP_Text TXTAlert;

    [Header("When everytime player enter level, reset level if true.")]
    public Toggle IsAlwaysRandom;

    [Header("Level name we will read.")]
    public TMP_InputField TXTLevelName;

    [Header("Slider that we will read row count.")]
    public Slider SLDRowCount;

    [Header("Slider that we will read col count.")]
    public Slider SLDColCount;

    [Header("Ölçeklendirme için kullanılacak.")]
    public Slider SLDScale;

    [Header("Current selected row quantity")]
    public TMP_Text TXTRowCount;

    [Header("Current selected col quantity")]
    public TMP_Text TXTColumnCount;

    [Header("Recomended sizes")]
    public TMP_Dropdown DDSizes;

    [Header("Minimum allowed size.")]
    public int MinWidth;

    [Header("Minimum item quantity must be in puzzle.")]
    public int MinItemCount;

    [Header("Default size of items.")]
    public int DefaultSize;

    [Header("Max item count in a row.")]
    public int MaxRowItemCount;

    [Header("Max item count in a col.")]
    public int MaxColItemCount;

    [Header("Save button.")]
    public GameObject BTNSave;

    [Header("Play button.")]
    public GameObject BTNPlay;

    [Header("Share button.")]
    public Button BTNShare;

    /// <summary>
    /// We will bind here when we make an update.
    /// </summary>
    public LevelEditorModel UpdateModel { get; set; }
    private void Start()
    {
        // We set the low limit of col.
        SLDColCount.minValue = Mathf.Sqrt(MinItemCount);

        //// We set the low limit of row.
        SLDRowCount.minValue = Mathf.Sqrt(MinItemCount);
    }
    public void OnClickSelectTexture()
    {
        // We are picking an image.
        FileBrowserController.Instance.PickAnImage((Texture2D selectedTexture) =>
        {
            // We clear the text.
            SetError(string.Empty);

            // Minimum image size.
            int size = Mathf.FloorToInt(MinWidth * Mathf.Sqrt(MinItemCount));

            // if file size is not enough just throw error.
            if (selectedTexture == null || selectedTexture.width < size || selectedTexture.height < size)
            {
                // We print file size not valid.
                SetError(ERR_INVALID_SIZE);

                // just return.
                return;
            }

            // Set the texture.
            this.CurrentSelectedTexture = selectedTexture;

            // We initialize designer.
            InitializeDesigner();
        });
    }

    private float CalculateTheMaxScale()
    {
        // if no texture then just return default value.
        if (CurrentSelectedTexture == null)
            return 1;

        // if sizes are not implemented yet just return.
        if (DDSizes.options.Count == 0)
            return 1;

        // We get the value in dd.
        float.TryParse(DDSizes.options[DDSizes.value].text, out float size);

        // We calculate the width and height max can be.
        float xCellSize = CurrentSelectedTexture.width / (SLDColCount.value * size);
        float yCellSize = CurrentSelectedTexture.height / (SLDRowCount.value * size);

        // We decide which one is less.
        float minSide = xCellSize > yCellSize ? yCellSize : xCellSize;

        // Then we return it.
        return minSide;
    }

    public void InitializeDesigner()
    {
        // We decide is width or height is smaller than otherone.
        int smallSide = this.CurrentSelectedTexture.width < this.CurrentSelectedTexture.height ? this.CurrentSelectedTexture.width : this.CurrentSelectedTexture.height;

        // Maximum size of one piece.
        int maxSize = Mathf.FloorToInt(smallSide / Mathf.Sqrt(MinItemCount));

        // We clamp the max size.
        if (maxSize < MinWidth)
            maxSize = MinWidth;

        // First we clear older options.
        DDSizes.ClearOptions();

        // We get the supported sizes for the texture.
        List<int> supportedSizes = GetSupportedSizes(smallSide).ToList();

        // We add the supported sizes.
        DDSizes.AddOptions(supportedSizes.Select(x => new TMP_Dropdown.OptionData($"{x}")).ToList());

        // We tell size changed.
        DDSizes.SetValueWithoutNotify(supportedSizes.IndexOf(supportedSizes.DefaultIfEmpty(-1).Min()));

        // We tell we changed size.
        OnSizeChanged(DDSizes.value);

        // We are refreshing the grid.
        EditorPlayground.Initialize(this);

        // We refresh the ui.
        RefreshUI();

        // We tell we changed size.
        OnSizeChanged(DDSizes.value);

    }

    public void OnRowCountChanged(float value)
    {
        // Maksimum 
        this.SLDScale.maxValue = CalculateTheMaxScale();

        // We reload the grid.
        EditorPlayground.GenerateGrid();

        // We refresh the ui.
        RefreshUI();
    }

    public void OnColumnCountChanged(float value)
    {
        // Maksimum 
        this.SLDScale.maxValue = CalculateTheMaxScale();

        // We reload the grid.
        EditorPlayground.GenerateGrid();

        // We refresh the ui.
        RefreshUI();
    }

    public void OnSizeChanged(int index)
    {
        // We try to parse size option.
        if (!float.TryParse(DDSizes.options[index].text, out float size))
            return;

        // if selcted texture not exists then return.
        if (CurrentSelectedTexture == null)
            return;

        // We clear the offset state.
        EditorPlayground.Offset = Vector3.zero;

        // We update the sizes.
        EditorPlayground.GLG.cellSize = new Vector2(size, size);

        // Old col caunt.
        int oldColCount = (int)SLDColCount.value;

        // Old row count.
        int oldRowCount = (int)SLDRowCount.value;

        // We set default value 0 to prevent errors.
        SLDColCount.SetValueWithoutNotify(0);

        // We set default value 0 to prevent errors.
        SLDRowCount.SetValueWithoutNotify(0);

        // Minumum quantity allowed in a row or col.
        int minItemInRowOrCol = (int)Mathf.Sqrt(MinItemCount);

        // We get the grid size.
        Rect gridRect = EditorPlayground.GetComponent<RectTransform>().rect;

        // Max width can be.
        float widthLimit = Mathf.Clamp(Mathf.FloorToInt(gridRect.width / size), minItemInRowOrCol, MaxColItemCount);

        // Maximum column count in a row.
        SLDColCount.maxValue = Mathf.Clamp(Mathf.FloorToInt(CurrentSelectedTexture.width / (float)size), minItemInRowOrCol, widthLimit);

        // Max height can be.
        float heightLimit = Mathf.Clamp(Mathf.FloorToInt(gridRect.height / size), minItemInRowOrCol, MaxRowItemCount);

        // Maximum row count in grid.
        SLDRowCount.maxValue = Mathf.Clamp(Mathf.FloorToInt(CurrentSelectedTexture.height / (float)size), minItemInRowOrCol, heightLimit);

        // We limit the col to prevent errors.
        oldColCount = Mathf.Clamp(oldColCount, minItemInRowOrCol, (int)SLDColCount.maxValue);

        // We limit the row to prevent errors.
        oldRowCount = Mathf.Clamp(oldRowCount, minItemInRowOrCol, (int)SLDRowCount.maxValue);

        // We set older col count with limations.
        SLDColCount.SetValueWithoutNotify(oldColCount);

        // We set older row count with limations.
        SLDRowCount.SetValueWithoutNotify(oldRowCount);

        // Maksimum 
        this.SLDScale.maxValue = CalculateTheMaxScale();

        // We reload the grid.
        EditorPlayground.GenerateGrid();

        // We refresh ui
        RefreshUI();
    }

    public void OnScaleChanged(float scale)
    {
        EditorPlayground.GenerateGrid();
    }

    public void RefreshUI()
    {
        // We print the row quantity.
        TXTRowCount.text = $"{(int)SLDRowCount.value}";

        // We print the col quantity.
        TXTColumnCount.text = $"{(int)SLDColCount.value}";
    }

    public void OnClickBack()
    {
        // we return to level menu.
        GameViewController.Instance.ActivateView(GameViews.CustomLevelMenu);
    }

    public void OnClickSave()
    {
        // We clear the text.
        SetError(string.Empty);

        // if not valid file name just return.
        if (string.IsNullOrEmpty(TXTLevelName.text.Trim()))
        {
            // Seviye ismi geçersiz hatasını dönüyoruz.
            SetError(ERR_LevelName);

            // if empty return.
            return;
        }

        // Active column count.
        int colCount = (int)SLDColCount.value;

        // Active row count.
        int rowCount = (int)SLDRowCount.value;

        // Size of grid cells.
        int size = EditorPlayground.GetSize;

        // Pixel start x position.
        int pixelStartXPosition = (int)EditorPlayground.Offset.x;

        // Pixel start y position.
        int pixelStartYPosition = (int)EditorPlayground.Offset.y;

        // We are receiving the offset pixels.
        Color[] pixels = CurrentSelectedTexture.GetPixels(pixelStartXPosition, pixelStartYPosition, size * colCount, size * rowCount);

        // We are creating the part texture.
        Texture2D texture = new Texture2D(size * colCount, size * rowCount, TextureFormat.RGB24, false, true);

        // We update the pixels.
        texture.SetPixels(pixels);

        // Then we apply our changes.
        texture.Apply();

        // Root file name.
        string filename = $"{DateTime.Now.ToFileTimeUtc()}";

        // Data file name, texture.
        string dataFileName = $"{filename}.data";

        // We start saving process.
        FileBrowserController.Instance.SaveTextureFile(dataFileName, texture);

        // Name of config file.
        string configFileName = $"{filename}.config";

        // We create order index.
        int orderIndex = GameController.Instance.CustomLevels.Select(x => x.OrderIndex).DefaultIfEmpty(0).Max() + 1;

        // We have to remove previous item.
        if (UpdateModel != null)
        {
            // We remove older config file.
            FileBrowserController.Instance.RemoveFile(UpdateModel.ConfigFileName);

            // We remove image file
            FileBrowserController.Instance.RemoveFile(UpdateModel.ImageUrl);

            // if update then we will update order index with old one.
            orderIndex = UpdateModel.OrderIndex;
        }

        // Scale rate for texture.
        float scale = SLDScale.value;

        // We create level data.
        LevelEditorModel levelData = new LevelEditorModel
        {
            UniqueID = Guid.NewGuid().ToString(),
            AlwaysRandom = IsAlwaysRandom.isOn,
            SeedValue = (int)(new System.Random().NextDouble() * 1000000),
            ColCount = colCount,
            ImageUrl = dataFileName,
            LevelName = TXTLevelName.text,
            RowCount = rowCount,
            Size = size,
            OrderIndex = orderIndex,
            ScaleRate = scale
        };

        // We start saving process.
        FileBrowserController.Instance.SaveConfigFile(configFileName, JsonUtility.ToJson(levelData));

        // We load levels again.
        GameController.Instance.LoadCustomLevels();

        // We get latest model to update if required.
        UpdateModel = GameController.Instance.CustomLevels.Find(x => x.UniqueID == levelData.UniqueID);

        // We tell if created.
        SetError(SUC_RecordCreated);

        // We set as playable.
        SetDesignerAsPlayable();
    }

    public void OnGameViewActivated()
    {
        // We apply a level name randomly.
        TXTLevelName.text = $"Level-{UnityEngine.Random.Range(0, 10000)}";

        // We refresh ui at the begining.
        RefreshUI();
    }

    public void OnGameViewDeactivated()
    {
        // We have to clear update model before we close.
        this.UpdateModel = null;

        // Reset all the states.
        ResetAllStates();

        // if texture exists we will remove it.
        if (CurrentSelectedTexture != null)
            Destroy(CurrentSelectedTexture);
    }

    public void ResetAllStates()
    {
        // We clear the text.
        SetError(string.Empty);

        // We clear the list.
        EditorPlayground.PuzzlePieces.Clear();

        // We are deleting the older items.
        foreach (Transform child in EditorPlayground.GLG.transform)
            Destroy(child.gameObject);
    }

    public void OnClickDelete()
    {
        // if there is no update model then return.
        if (UpdateModel == null)
            return;

        // We remove the level.
        GameController.Instance.RemoveLevel(UpdateModel);

        // We say record deleted.
        SetError(SUC_RecordDeleted);
    }

    public void SetDesignerAsPlayable()
    {
        // We disable the save button.
        BTNSave.gameObject.SetActive(false);

        // We enable the play button.
        BTNPlay.gameObject.SetActive(true);

        // We enable share button when playable ready.
        BTNShare.interactable = true;
    }

    public void SetDesignerAsSaveable()
    {
        // We disable the save button.
        BTNSave.gameObject.SetActive(true);

        // We enable the play button.
        BTNPlay.gameObject.SetActive(false);

        // We enable share button when playable ready.
        BTNShare.interactable = false;
    }

    public void OnClickPlay()
    {
        // if model is null then return.
        if (UpdateModel == null)
        {
            // We set as readable.
            SetDesignerAsSaveable();

            // We return because there is no model.
            return;
        }

        // We set the level to the max level.
        int targetLevel = GameController.Instance.CustomLevels.IndexOf(UpdateModel) + 1;

        // We activate the level.
        GameController.Instance.ActivateLevel(UpdateModel, LevelStates.UserDefined, targetLevel);
    }

    public void OnClickShare()
    {
        // We create the uploader.
        CustomLevelDetailUploadViewController uploader = Instantiate(GOUpload, transform).GetComponent<CustomLevelDetailUploadViewController>();

        // We activate the share view.
        uploader.Show(this.UpdateModel);
    }

    #region Error Management

    public const string ERR_LevelName = "LEVEL_NAME";
    public const string ERR_INVALID_SIZE = "INVALID_SIZE";
    public const string SUC_RecordCreated = "RECORD_CREATED";
    public const string SUC_RecordDeleted = "RECORD_DELETED";
    public void SetError(string keyword)
    {
        // if keyword is empty then just clear errors.
        if (string.IsNullOrEmpty(keyword))
        {
            // Cleared text.
            TXTAlert.text = string.Empty;

            // Return back.
            return;
        }

        // We set text depends on the keyword.
        switch (keyword)
        {
            case ERR_LevelName:
                {
                    switch (SaveLoadController.Instance.SaveData.Language)
                    {
                        case Languages.English:
                            TXTAlert.text = "Please enter a valid level name!";
                            break;
                        case Languages.Turkish:
                            TXTAlert.text = $"Lütfen geçerli bir seviye ismi giriniz!";
                            break;
                    }
                    break;
                }
            case ERR_INVALID_SIZE:
                {
                    // Min size.
                    int size = Mathf.FloorToInt(MinWidth * (MinItemCount / (float)2));

                    switch (SaveLoadController.Instance.SaveData.Language)
                    {
                        case Languages.English:
                            TXTAlert.text = $"Image size must be at least {size}x{size} sizes!";
                            break;
                        case Languages.Turkish:
                            TXTAlert.text = $"Resim boyutu en az {size}x{size} boyutunda olmalı!";
                            break;
                    }
                    break;
                }
            case SUC_RecordCreated:
                {
                    // Total level count.
                    int levelCount = GameController.Instance.CustomLevels.Count;

                    // We print text when succeed.
                    switch (SaveLoadController.Instance.SaveData.Language)
                    {
                        case Languages.English:
                            TXTAlert.text = $"<color=green>{levelCount}. Level created with {TXTLevelName.text} name!</color>";
                            break;
                        case Languages.Turkish:
                            TXTAlert.text = $"<color=green>{levelCount}. seviye {TXTLevelName.text} adıyla oluşturuldu!</color>";
                            break;
                    }
                    break;
                }
            case SUC_RecordDeleted:
                {
                    // We print text when succeed.
                    switch (SaveLoadController.Instance.SaveData.Language)
                    {
                        case Languages.English:
                            TXTAlert.text = $"<color=green>Level deleted successfully!</color>";
                            break;
                        case Languages.Turkish:
                            TXTAlert.text = $"<color=green>Seviye başarıyla silindi!</color>";
                            break;
                    }
                    break;
                }
        }
    }

    #endregion

    public IEnumerable<int> GetSupportedSizes(int smallSize)
    {
        // We get the list of support sizes.
        int[] supportedSizes = new int[] { 64, 128, 256, 512 };

        // We return the supported sizes.
        return supportedSizes.Where(x => Mathf.FloorToInt(smallSize / (float)x) >= Mathf.Sqrt(MinItemCount));
    }

}
