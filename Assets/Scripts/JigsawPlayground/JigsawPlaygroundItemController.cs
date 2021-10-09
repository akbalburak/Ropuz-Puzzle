using System.Collections.Generic;
using UnityEngine;

public class JigsawPlaygroundItemController : MonoBehaviour
{
    [Header("UI location component.")]
    public RectTransform RectTransform;

    [Header("Connected pieces to the jigsaw piece.")]
    public List<JigsawPlaygroundItemController> ConnectedJigsaws;

    [Header("Column index.")]
    public int Column;

    [Header("Row index.")]
    public int Row;

    private void Start()
    {
        // Location with a random position.
        RectTransform = GetComponent<RectTransform>();
        RectTransform.anchoredPosition = JigsawPlayground.Instance.GetRandomPosition();
    }

    public void SetGridData(int row, int col)
    {
        // We set the column and row values.
        this.Column = col;
        this.Row = row;
    }

    public void ConnectToPiece(JigsawPlaygroundItemController jigsaw)
    {
        // if not exists between two piece.
        if (!ConnectedJigsaws.Contains(jigsaw))
        {
            // We join together.
            ConnectedJigsaws.Add(jigsaw);

            // We select all the releated pieces of this.
            JigsawPlayground.Instance.SelectReleatedPieces(this);
        }

        // We make sure piece also connected.
        if (!jigsaw.ConnectedJigsaws.Contains(this))
            jigsaw.ConnectToPiece(this);
    }

    public void OnMouseDown()
    {
        // We set as selected when click thep iece.
        JigsawPlayground.Instance.OnSelectPiece(this);
    }
}
