using Assets.Scripts.Models;
using GoogleMobileAds.Api;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayGroundHintController : MonoBehaviour
{
    [Header("When hint is activated we are going to show this object.")]
    public GameObject HintObject;

    private void Start()
    {

        // We have to bind the action to the rewarded ads.
        AdsController.Instance.AdsReward.OnRewardActivated.AddListener(OnHintVideoRewarded);
    }

    private void OnHintVideoRewarded(Reward arg0)
    {
        HintObject.SetActive(true);
    }

    public void LateUpdate()
    {
        // if hint object is active and user click the screen we will disable the hint object.
        if (HintObject.activeSelf && Input.GetMouseButtonDown(0))
            HintObject.SetActive(false);
    }

    private void OnDestroy()
    {
        // We are removing the rewarded callback.
        AdsController.Instance.AdsReward.OnRewardActivated.RemoveListener(OnHintVideoRewarded);
    }

    public void LoadTexture(Texture2D shownLevelTexture2D, LevelEditorModel levelData, Vector2 spacing)
    {
        // We apply the texture.
        HintObject.GetComponent<RawImage>().texture = shownLevelTexture2D;

        // We also have to give the same size to the hint object.
        HintObject.GetComponent<RectTransform>().sizeDelta = new Vector2(
            (levelData.ColCount * levelData.Size) + (levelData.ColCount - 1) * spacing.x,
            (levelData.RowCount * levelData.Size) + (levelData.RowCount - 1) * spacing.y);
        
        /** INTRESTINGLY, if we dont activate once the hint object game is crashing.
         This is a temporarly solution. Probably, texture issues. */
        HintObject.gameObject.SetActive(true);
        HintObject.gameObject.SetActive(false);
    }
}
