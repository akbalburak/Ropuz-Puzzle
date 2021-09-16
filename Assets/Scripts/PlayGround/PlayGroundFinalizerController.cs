using Assets.Scripts.Models;
using System;
using System.Collections;
using UnityEngine;

public class PlayGroundFinalizerController : MonoBehaviour
{
    [Header("Custom play ground")]
    public PlaygroundController CPGC;

    [Header("Reduction speed of grid spacing.")]
    public float OffsetReductionSpeed;

    [Header("When the grid is going to finalize.")]
    public bool IsGoingToFinalize;

    [Header("True when the finalized.")]
    public bool IsFinalized;

    [Header("Growing target of finalizer.")]
    public Vector3 GrowingSize;

    [Header("Growing speed.")]
    public float GrowingSpeed;

    public void FinalizeThePlayGround()
    {
        // Finalized state changed.
        this.IsFinalized = false;

        // We are teliing start finalizing.
        this.IsGoingToFinalize = true;

        // We are closing the rotation content.
        CPGC.RotationsContent.gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        // If finalizing not started return.
        if (!this.IsGoingToFinalize)
            return;

        // If it is already finalized return.
        if (this.IsFinalized)
            return;

        // Grid spacing.
        Vector2 gridSpacing = CPGC.GridLayoutOfPlayGround.spacing;

        // We are reducing the initial size.
        gridSpacing = new Vector2(gridSpacing.x - Time.fixedDeltaTime * OffsetReductionSpeed, gridSpacing.y - Time.fixedDeltaTime * OffsetReductionSpeed);

        // We are reducing the initial size.
        transform.localScale = new Vector2(transform.localScale.x + Time.fixedDeltaTime * GrowingSpeed, transform.localScale.y + Time.fixedDeltaTime * GrowingSpeed);

        // if smaller than or equal to 0  clamp it.
        CPGC.GridLayoutOfPlayGround.spacing = gridSpacing.x <= 0 && gridSpacing.y <= 0 ? Vector2.zero : gridSpacing;

        // Local scale borders.
        if (transform.localScale.x >= GrowingSize.x && transform.localScale.y >= GrowingSize.y)
            transform.localScale = GrowingSize;

        // We are checking conditions is finalized?
        if (transform.localScale.x >= GrowingSize.x && transform.localScale.y >= GrowingSize.y && gridSpacing.x <= 0 && gridSpacing.y <= 0)
            OnLevelFinalized();

    }

    private void OnLevelFinalized()
    {
        // Set as finalized.
        this.IsFinalized = true;

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
    }
}
