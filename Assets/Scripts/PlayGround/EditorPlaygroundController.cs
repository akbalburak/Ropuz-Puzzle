using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorPlaygroundController : MonoBehaviour
{
    [Header("Designer view panel.")]
    public LevelDesignerGameViewController LDGVC;

    [Header("Is this puzzle dragging.")]
    public bool IsPuzzleDragging;

    [Header("Grid layout component which contains parts.")]
    public GridLayoutGroup GLG;

    [Header("Playground item that we will instantiate.")]
    public GameObject EditorPlaygroundItem;

    [Header("Offset value of image.")]
    public Vector3 Offset;

    /// <summary>
    /// Row, Col, Texture
    /// </summary>
    public List<Tuple<int, int, RawImage>> PuzzlePieces = new List<Tuple<int, int, RawImage>>();

    public void Initialize(LevelDesignerGameViewController levelDesignerGameViewController)
    {
        // We set the designer panel to access data.
        this.LDGVC = levelDesignerGameViewController;

        // We generate grid again.
        GenerateGrid();

        // We set as playable when update model exists.
        if (this.LDGVC.UpdateModel != null)
            LDGVC.SetDesignerAsPlayable();
    }

    public void GenerateGrid()
    {
        // Any little changes convert buttons to saveable.
        LDGVC.SetDesignerAsSaveable();

        // We delete older stuffs.
        LDGVC.ResetAllStates();

        // Active column count.
        int colCount = (int)LDGVC.SLDColCount.value;

        // Active row count.
        int rowCount = (int)LDGVC.SLDRowCount.value;

        // We update the constraint count to the col count..
        GLG.constraintCount = colCount;

        // Per peace size, we think it is a square.;
        int size = (int)GLG.cellSize.x;

        // if there is no seleted texture then return.
        if (LDGVC.CurrentSelectedTexture == null)
            return;

        /** WE FIT GRID TO PLAYGROUND **/

        // Size of grid.
        Rect rectOfGrid = GLG.GetComponent<RectTransform>().rect;

        // Approximate width size without spacing.
        float approximateWidth = colCount * size;

        // Approximate height size without spacing.
        float approximateHeight = rowCount * size;

        // Rate with real width
        float widthRate = rectOfGrid.width / approximateWidth;

        // Rate with real height.
        float heightRate = rectOfGrid.height / approximateHeight;

        // We based the small side to rate.
        float smallRate = widthRate > heightRate ? heightRate : widthRate;

        // We scale to fit.
        transform.localScale = new Vector2(smallRate, smallRate);

        // We check the borders to prevent image goes outside of the grid. if image goes outside exception will be triggered by getpixels method.
        Offset = new Vector2(Mathf.Clamp(Offset.x, 0, LDGVC.CurrentSelectedTexture.width - colCount * size), Mathf.Clamp(Offset.y, 0, LDGVC.CurrentSelectedTexture.height - rowCount * size));

        // Loop the rowcount.
        for (int r = rowCount - 1; r >= 0; r--)
        {
            // We loop each column.
            for (int c = 0; c < colCount; c++)
            {
                // Pixel start x position.
                int pixelStartXPosition = c * size + (int)Offset.x;

                // Pixel start y position.
                int pixelStartYPosition = r * size + (int)Offset.y;

                // We will bind image to this playground item.
                GameObject playgroundItem = Instantiate(EditorPlaygroundItem, GLG.transform);

                // We will apply the data into the playground item.
                RawImage playgroundTexture = playgroundItem.GetComponent<RawImage>();

                // We are receiving the offset pixels.
                Color[] pixels = LDGVC.CurrentSelectedTexture.GetPixels(pixelStartXPosition, pixelStartYPosition, size, size);

                // We are creating the part texture.
                Texture2D texture = new Texture2D(size, size, TextureFormat.RGB24, false);

                // We update the pixels.
                texture.SetPixels(pixels);

                // Then we apply our changes.
                texture.Apply();

                // First step is resize the texture.
                playgroundTexture.texture = texture;

                // We add to pieces list to update when required.
                PuzzlePieces.Add(new Tuple<int, int, RawImage>(r, c, playgroundTexture));
            }
        }
    }

    public void UpdateGrid()
    {
        // Active column count.
        int colCount = (int)LDGVC.SLDColCount.value;

        // Active row count.
        int rowCount = (int)LDGVC.SLDRowCount.value;

        // Per peace size, we think it is a square.;
        int size = (int)GLG.cellSize.x;

        // We check the borders to prevent image goes outside of the grid. if image goes outside exception will be triggered by getpixels method.
        Offset = new Vector2(Mathf.Clamp(Offset.x, 0, LDGVC.CurrentSelectedTexture.width - colCount * size),
            Mathf.Clamp(Offset.y, 0, LDGVC.CurrentSelectedTexture.height - rowCount * size));

        // Each pieces texture updating.
        PuzzlePieces.ForEach(e =>
        {
            // Pixel start x position.
            int pixelStartXPosition = e.Item2 * size + (int)Offset.x;

            // Pixel start y position.
            int pixelStartYPosition = e.Item1 * size + (int)Offset.y;

            // We are receiving the offset pixels.
            Color[] pixels = LDGVC.CurrentSelectedTexture.GetPixels(pixelStartXPosition, pixelStartYPosition, size, size);

            // We are creating the part texture.
            Texture2D texture = e.Item3.texture as Texture2D;

            // We update the pixels.
            texture.SetPixels(pixels);

            // Then we apply our changes.
            texture.Apply();
        });
    }

    #region Drag And Drop Texture

    private Vector3 dragStartPosition;
    public void OnBeginDrag()
    {

        // We tell puzzle draging started.
        IsPuzzleDragging = true;

        // To prevent offset from previous touches we add the diff to the start position.
        dragStartPosition = Input.mousePosition + Offset;
    }

    public void OnEndDrag()
    {
        // When player release the mouse state changed false.
        IsPuzzleDragging = false;
    }

    private void Update()
    {
        // if puzzle is not dragging just returnç
        if (!IsPuzzleDragging)
            return;

        // We calculate between start position with current mouse position.
        Vector3 newDiffrenceBetween = dragStartPosition - Input.mousePosition;


        // if no distance between last position and current position.
        if (Mathf.Abs((newDiffrenceBetween - Offset).magnitude) < 1)
            return;

        // Offset value we will use.
        Offset = newDiffrenceBetween;

        // We generate grid again.
        UpdateGrid();
    }

    #endregion

}
