using Assets.Scripts.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class PlaygroundController : MonoBehaviour
{
    [Header("Current Custom level manager.")]
    public CurrentLevelGameViewController CCLGVC;

    [Header("Finalizer when playground completed.")]
    public PlayGroundFinalizerController PGFC;

    [Header("Hint controller.")]
    public PlayGroundHintController PGHC;

    [Header("Correct item formation.")]
    public List<PlayGroundItemController> CorrectFormationItems;

    [Header("Playground grid layout")]
    public GridLayoutGroup GridLayoutOfPlayGround;

    [Header("Transform where the rotation buttons exists..")]
    public Transform RotationsContent;

    [Header("All the rotation buttons.")]
    public List<Button> BTNRotations;

    [Header("Level data to load informations.")]
    public LevelEditorModel LevelData;

    [Header("Items that will be rotated.")]
    public List<PlayGroundItemController> Items;

    public void LoadPlayGroundGrid(LevelEditorModel levelData)
    {
        // We set the level data.
        this.LevelData = levelData;

        // Playground items.
        Items = GetComponentsInChildren<PlayGroundItemController>().ToList();

        // Any rotation button will trigger the rotation.
        foreach (Transform rotation in RotationsContent.transform)
        {
            // Button for rotate.
            Button btnRotation = rotation.GetComponent<Button>();

            // We are setting click action.
            btnRotation.onClick.AddListener(() => OnClickRotate(btnRotation));

            // We are adding to list to use in the further.
            if (!BTNRotations.Contains(btnRotation))
                BTNRotations.Add(btnRotation);
        }

        // We have to also calculate rotations paddings.
        GridLayoutGroup rotationsGridLayout = RotationsContent.GetComponent<GridLayoutGroup>();

        // We also update rotation grid size to prevnt offset.
        rotationsGridLayout.cellSize = GridLayoutOfPlayGround.cellSize;

        /** WE FIT GRID TO PLAYGROUND **/

        // Size of grid.
        Rect rectOfGrid = GridLayoutOfPlayGround.GetComponent<RectTransform>().rect;

        // Approximate width size without spacing.
        float approximateWidth = CurrentLevelGameViewController.Instance.LevelData.ColCount * CurrentLevelGameViewController.Instance.LevelData.Size;

        // Approximate height size without spacing.
        float approximateHeight = CurrentLevelGameViewController.Instance.LevelData.RowCount * CurrentLevelGameViewController.Instance.LevelData.Size;

        // Rate with real width
        float widthRate = rectOfGrid.width / approximateWidth;

        // Rate with real height.
        float heightRate = rectOfGrid.height / approximateHeight;

        // We based the small side to rate.
        float smallRate = widthRate > heightRate ? heightRate : widthRate;

        // We scale to fit grid.
        GridLayoutOfPlayGround.transform.localScale = new Vector2(smallRate, smallRate);

        // We scale to fit grid.
        RotationsContent.transform.localScale = new Vector2(smallRate, smallRate);

        // We scale to the screen.
        PGHC.HintObject.transform.localScale = new Vector2(smallRate, smallRate);
    }

    public void OnClickRotate(Button sender)
    {
        // We are reducing one point.
        SaveLoadController.Instance.SaveData.ActionScore -= 1;

        // We are applying changes.
        SaveLoadController.Instance.Save();

        // And we are reducing from the ui.
        CurrentLevelGameViewController.Instance.RefreshUI();

        // We are receiving the index of rotation.
        int index = BTNRotations.IndexOf(sender);

        // Items that will return.
        List<PlayGroundItemController> rotationItems = new List<PlayGroundItemController>();

        // Rotater count in a row.
        int rotationCountInaRow = GridLayoutOfPlayGround.constraintCount - 1;

        // Index where the rotation wil lstart.
        int rotationStartValue = Mathf.FloorToInt(index / (float)rotationCountInaRow) * GridLayoutOfPlayGround.constraintCount + (index % rotationCountInaRow);

        // Indexed rotation query.
        IEnumerable<PlayGroundItemController> query = Items.Skip(rotationStartValue);

        // First two.
        rotationItems.AddRange(query.Take(GameController.Instance.RotationQuantity / 2));

        // Second two.
        rotationItems.AddRange(query.Skip(GridLayoutOfPlayGround.constraintCount).Take(GameController.Instance.RotationQuantity / 2).Reverse());

        // We are closing the interactable to prevent bugs.
        BTNRotations.ForEach(e => e.interactable = false);

        // Grid layout will be disabled.
        GridLayoutOfPlayGround.enabled = false;

        // We are starting rotate items.
        StartCoroutine(StartRotate(rotationItems));

    }

    private IEnumerator StartRotate(List<PlayGroundItemController> rotationItems)
    {
        // We will use the indexes of the elements to replace in grid layout.
        List<Tuple<PlayGroundItemController, int>> indexes = Items.Select((x, i) => new Tuple<PlayGroundItemController, int>(x, Items.IndexOf(x))).ToList();

        // We are going to rotate all the items.
        rotationItems.ForEach(e =>
        {
            // We are looking for the next element.
            int nextIndex = rotationItems.IndexOf(e) + 1;

            // if we reach the last element we will return to the first one.
            if (nextIndex >= rotationItems.Count)
                nextIndex = 0;

            // Next item is target.
            PlayGroundItemController nextItem = rotationItems.ElementAt(nextIndex);

            // Target rotation unit will rotate.
            e.StartRotate(nextItem.GetComponent<RectTransform>().anchoredPosition);

            // We are changing location in grid.
            e.transform.SetSiblingIndex(nextItem.transform.GetSiblingIndex());
        });

        // We are waiting untill rotations of all items completed.
        yield return new WaitUntil(() => rotationItems.TrueForAll(e => e.IsRotationCompleted));

        // We are resetting ground item.
        rotationItems.ForEach(e => e.RevertItemProps());

        // We are enabling the layout.
        GridLayoutOfPlayGround.enabled = true;

        // We are reordering list.
        Items = GetComponentsInChildren<PlayGroundItemController>().ToList();

        // We are re activating rotate buttons.
        BTNRotations.ForEach(e => e.interactable = true);

        // True when the both of sequence equals.
        bool isCompleted = CorrectFormationItems.SequenceEqual(Items);

        // When true call finalizer.
        if (isCompleted)
        {
            // We remove history if the level completed.
            GameHistoryController.Instance.RemoveHistory(this);

            // We are finalizing the playground.
            PGFC.FinalizeThePlayGround();
        }
        else
        {
            // We remove history if the level completed.
            GameHistoryController.Instance.SetHistory(this);
        }
    }

    public void ShufflePlayground(int? seed = null)
    {
        // Randomizer.
        System.Random random = null;

        // if seed has value then we will generate random number with seed.
        if (seed.HasValue)
            random = new System.Random(seed.Value);
        else
            random = new System.Random();

        // Shuffled list.
        List<PlayGroundItemController> randomList = this.Items.OrderBy(x => random.NextDouble()).ToList();

        // We loop all the items.
        for (int index = 0; index < randomList.Count; index++)
        {
            // We get the item in the given list.
            PlayGroundItemController randomItem = randomList[index];

            // We change its location.
            randomItem.transform.SetSiblingIndex(index);
        }

        // We update the list.
        this.Items = randomList;
    }

    public void ContinueToLevel()
    {
        // We get the history data.
        GameHistoryModel historyValue = GameHistoryController.Instance.GetHistory(this.LevelData);

        // We will store the items with order indexes.
        List<Tuple<int, PlayGroundItemController>> unOrderedItems = new List<Tuple<int, PlayGroundItemController>>();

        this.Items = this.Items.OrderBy(x => historyValue.Names.IndexOf(x.name)).ToList();

        // We loop all the items.
        foreach (PlayGroundItemController playItem in this.Items.OrderBy(x => historyValue.Names.IndexOf(x.name)))
        {
            // we get the sibling index.
            int siblingIndex = historyValue.Names.IndexOf(playItem.name);

            // We change its location.
            playItem.transform.SetSiblingIndex(siblingIndex);
        }
    }
}
