﻿using Assets.Scripts.Models;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomLevelDetailViewController : MonoBehaviour
{
    [Header("Level information given.")]
    public LevelEditorModel LevelData;

    [Header("Level texture to play.")]
    public RawImage IMGLevel;

    [Header("Texture reference to use later.")]
    public Texture2D CurrentImgTexture;

    [Header("Level name")]
    public TMP_Text TXTLevelName;

    [Header("Loading view")]
    public GameObject GOLoading;

    [Header("Remove button.")]
    public Button BTNRemove;

    [Header("Remove approve button.")]
    public Button BTNRemoveApprove;

    [Header("Upload view.")]
    public CustomLevelDetailUploadViewController UploadView;

    public void OnClickShare()
    {
        // Activate the loading view.
        GOLoading.SetActive(true);

        // We are uploading the image.
        FirebaseStorageController.Instance.UploadImage(this.LevelData.LoadTextureFromFile(), (imageResult) =>
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
                         UploadView.Show(newLevelData);
                 });
            }
        });
    }

    public void OnClickDelete()
    {
        // We remove the level data.
        GameController.Instance.RemoveLevel(this.LevelData);

        // We disable the view.
        GOLoading.SetActive(false);

        // We set remove button disabled.
        Close();
    }

    public void OnClickEdit()
    {
        // We close the panel.
        gameObject.SetActive(false);

        // We activate the level editor.
        LevelDesignerGameViewController levelDesigner = (LevelDesignerGameViewController)GameViewController.Instance.ActivateView(GameViews.LevelEditor);

        // Then we will add data to 
        levelDesigner.UpdateModel = this.LevelData;

        // We set the texture to update.
        levelDesigner.CurrentSelectedTexture = this.LevelData.LoadTextureFromFile();

        // We update col quantity.
        levelDesigner.SLDColCount.SetValueWithoutNotify(this.LevelData.ColCount);

        // We update row quantity.
        levelDesigner.SLDRowCount.SetValueWithoutNotify(this.LevelData.RowCount);

        // We also set size.
        levelDesigner.DDSizes.SetValueWithoutNotify(this.LevelData.Size);

        // We init designer.
        levelDesigner.InitializeDesigner();

        // We put the level name.
        levelDesigner.TXTLevelName.text = this.LevelData.LevelName;

        // Is randomized?
        levelDesigner.IsAlwaysRandom.SetIsOnWithoutNotify(this.LevelData.AlwaysRandom);

        // We close the panel.
        Close();
    }

    public void OnClickPlay()
    {
        // We are looking for the level.
        int levelIndex = GameController.Instance.CustomLevels.IndexOf(this.LevelData) + 1;

        // We activate the level.
        GameController.Instance.ActivateLevel(this.LevelData, LevelStates.UserDefined, levelIndex);

        // We close the panel.
        Close();
    }

    public void Show(LevelEditorModel levelData)
    {
        // We activate the game object.
        gameObject.SetActive(true);

        // Level information.
        this.LevelData = levelData;

        // We bind the texture.
        CurrentImgTexture = levelData.LoadTextureFromFile();

        // We bind the texture.
        IMGLevel.texture = CurrentImgTexture;

        // We use texture size.
        IMGLevel.SetNativeSize();

        // Width and height rate of image to prevent all images become square.
        float whRate = IMGLevel.rectTransform.sizeDelta.y / IMGLevel.rectTransform.sizeDelta.x;

        // We calculate the x size of image.
        float imageXSize = Mathf.Clamp(IMGLevel.rectTransform.sizeDelta.x, 0, 600);

        // We calculate the y size with x ratio.
        float imageYSize = Mathf.Clamp(IMGLevel.rectTransform.sizeDelta.y, 0, 600 * whRate);

        // We make sure it is not big enough.
        IMGLevel.rectTransform.sizeDelta = new Vector2(imageXSize, imageYSize);
    }

    public void Close()
    {
        // We clear the texture reference.
        CurrentImgTexture = null;

        // We have to revert remove button to original state.
        BTNRemove.gameObject.SetActive(true);

        // We have to revert remove approve button to its original state.
        BTNRemoveApprove.gameObject.SetActive(false);

        // And then disable the object.
        gameObject.SetActive(false);
    }
}
