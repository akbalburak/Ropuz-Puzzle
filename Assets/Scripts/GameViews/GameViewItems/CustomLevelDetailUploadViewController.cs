using Assets.Scripts.Extends;
using Assets.Scripts.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomLevelDetailUploadViewController : MonoBehaviour
{
    public static CustomLevelDetailUploadViewController Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    [Header("Loading state.")]
    public GameObject GOLoading;

    [Header("Level informations.")]
    public LevelEditorModel LevelData;

    [Header("Inputfield to print upload url.")]
    public TMP_InputField INPUploadUrl;

    [Header("Playground area.")]
    public GameObject CustomPlaygroundItem;

    [Header("Playground items to print.")]
    public GameObject CustomLevelPlaygroundItem;

    [Header("Playground item content.")]
    public Transform CustomPlaygroundContent;

    [Header("Detail view that we print informations.")]
    public GameObject GoUploadDetailView;

    public void Show(LevelEditorModel levelData)
    {
        // Level information.
        this.LevelData = levelData;

        // Activate the loading view.
        GOLoading.SetActive(true);

        // We get the texture of selected image.
        Texture2D textureOfUploadedImage = this.LevelData.LoadTextureFromFile();

        // We are uploading the image.
        FirebaseStorageController.Instance.UploadImage(textureOfUploadedImage, (imageResult) =>
        {
            // Disable the loading view.
            GOLoading.SetActive(false);

            // if file name is empty error exists.
            if (imageResult.IsCompleted)
            {
                // Activate the loading view.
                GOLoading.SetActive(true);

                // We get the image url.
                LevelEditorModel newLevelData = LevelData.Clone();

                // We bind the file.
                newLevelData.ImageUrl = imageResult.Result.Name;

                // We bind the texture to is it.
                newLevelData.RemoteTexture = textureOfUploadedImage;

                // We have to change also data url.
                int fileExtPos = newLevelData.ImageUrl.LastIndexOf(".");
                if (fileExtPos >= 0)
                    newLevelData.ConfigFileName = newLevelData.ImageUrl.Substring(0, fileExtPos);

                // We upload the config file.
                FirebaseStorageController.Instance.UploadConfig(newLevelData, (configResult) =>
                {
                    // Disable the loading view.
                    GOLoading.SetActive(false);

                    // We show the upload state.
                    if (imageResult.IsCompleted)
                    {
                        // We enabled loading.
                        GOLoading.SetActive(true);

                        // We create the deep link.
                        DeepLinkController.Instance.CreateADeepLink(newLevelData.ConfigFileName, (url) =>
                        {
                            // We activating the view.
                            GoUploadDetailView.SetActive(true);

                            // We disable the loading.
                            GOLoading.SetActive(false);

                            // We activate the object.
                            this.gameObject.SetActive(true);

                            // We print the url to share.
                            this.INPUploadUrl.text = url?.ToString();
                        });
                    }
                });
            }
        });
    }

    public void OnClickShare()
    {
        // Original image texture.
        Texture2D originalTexture = this.LevelData.LoadTextureFromFile();

        //Rate to multiply with size.
        int rate = (originalTexture.width / 128) + 1;

        // We get the texture of selected image.
        Texture2D textureOfUploadedImage = TextureExtensions.Blur(originalTexture, 10, rate * 2);

        string content;

        // if turkish then text turkish content.
        if (SaveLoadController.Instance.SaveData.Language == Languages.Turkish)
            content = "Bulmacayı çözebilir misin?";
        else
            content = "Can you solve the puzzle?";

        // We share the puzzle.
        ShareController.Instance.Share(textureOfUploadedImage, content, this.INPUploadUrl.text);

    }

    public void Close()
    {
        Destroy(gameObject);
    }

    public void OnClickCopy()
    {
        // We copy to clipboard.
        INPUploadUrl.text.CopyToClipboard();
    }

    /*public IEnumerator GetRandomCombinedTexture(Action<Texture2D> onTextureSelected)
    {
        // We have to weait end of frame.
        yield return new WaitForEndOfFrame();

        // We destroy the older items.
        foreach (Transform child in CustomPlaygroundContent)
            Destroy(child.gameObject);

        // We activate the playground screen.
        CustomPlaygroundContent.parent.gameObject.SetActive(true);

        // We are generating the ground item.
        PlaygroundController CPGC = Instantiate(CustomPlaygroundItem, CustomPlaygroundContent).GetComponent<PlaygroundController>();

        // We create the size.
        CPGC.GridLayoutOfPlayGround.cellSize = new Vector2(LevelData.Size, LevelData.Size);

        // We set the column count.
        CPGC.GridLayoutOfPlayGround.constraintCount = LevelData.ColCount;

        // We get the texture.
        Texture2D ShownLevelTexture2D = LevelData.LoadTextureFromFile();

        // The content we will place pieces.
        Transform piecesGridContent = CPGC.transform.Find("Items");

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
            }
        }

        // Randomizer.
        System.Random random = null;

        // if seed has value then we will generate random number with seed.
        if (!this.LevelData.AlwaysRandom)
            random = new System.Random(this.LevelData.SeedValue);
        else
            random = new System.Random();

        // Playground items.
        List<PlayGroundItemController> Items = GetComponentsInChildren<PlayGroundItemController>().ToList();

        // Size of grid.
        Rect rectOfGrid = CPGC.GetComponent<RectTransform>().rect;

        // Approximate width size without spacing.
        float approximateWidth = this.LevelData.ColCount * this.LevelData.Size;

        // Approximate height size without spacing.
        float approximateHeight = this.LevelData.RowCount * this.LevelData.Size;

        // Rate with real width
        float widthRate = rectOfGrid.width / approximateWidth;

        // Rate with real height.
        float heightRate = rectOfGrid.height / approximateHeight;

        // We based the small side to rate.
        float smallRate = widthRate > heightRate ? heightRate : widthRate;

        // We scale to fit grid.
        CPGC.transform.localScale = new Vector2(smallRate, smallRate);

        // Shuffled list.
        List<PlayGroundItemController> randomList = Items.OrderBy(x => random.NextDouble()).ToList();

        // We loop all the items.
        for (int index = 0; index < randomList.Count; index++)
        {
            // We get the item in the given list.
            PlayGroundItemController randomItem = randomList[index];

            // We change its location.
            randomItem.transform.SetSiblingIndex(index);
        }

        // We update the list.
        Items = randomList;

        // We have to weait end of frame.
        yield return new WaitForEndOfFrame();

        int offsetFromBottom = 100;

        // We get the screenshot.
        Texture2D screenShot = ScreenCapture.CaptureScreenshotAsTexture();

        // We create a captured screen to make texture readable.
        Texture2D capturedScreen = new Texture2D(Screen.width, Screen.height);

        // We load the image.
        capturedScreen.LoadImage(NativeGallery.GetTextureBytes(screenShot, true));

        // We apply changes we make.
        capturedScreen.Apply();

        // We activate the playground screen.
        CustomPlaygroundContent.parent.gameObject.SetActive(false);

        // We are clipping pixels.
        Color[] clipped = capturedScreen.GetPixels(0, offsetFromBottom, Screen.width, Screen.height - offsetFromBottom);

        // We are clipping texture.
        Texture2D clippedTexture = new Texture2D(Screen.width, Screen.height - offsetFromBottom);

        // We add the pixels.
        clippedTexture.SetPixels(clipped);

        // We apply changes.
        clippedTexture.Apply();

        // We destroy the captured screen to create new one.
        Destroy(capturedScreen);

        // We destroy older stuff to prevent memory issues.
        Destroy(screenShot);

        // We get the capture.
        onTextureSelected.Invoke(clippedTexture);
    }
    */
}
