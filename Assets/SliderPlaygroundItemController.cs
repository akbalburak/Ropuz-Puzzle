using UnityEngine;

public class SliderPlaygroundItemController : MonoBehaviour
{
    [Header("Rect transform of slider piece.")]
    public RectTransform RectTransform;

    [Header("Piece index number. Unique defined.")]
    public int PieceIndex;

    [Header("Current row index of piece.")]
    public int Row;

    [Header("Current col index of piece.")]
    public int Col;

    [Header("When a target defined piece will move through the target.")]
    public Vector2? TargetToMove;

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }

    public void OnMouseDown()
    {
        // if level is completed just return.
        if (SliderPlaygroundController.Instance.IsFinalized)
            return;

        // We make sure any piece not moving.
        if (SliderPlaygroundController.Instance.SliderPlaygroundItems.Exists(x => x.TargetToMove.HasValue))
            return;

        // Empty row and col values.
        SliderPlaygroundItemController emptyPiece = SliderPlaygroundController.Instance.GetEmptyPiece;

        // We store the values.
        int nRow = emptyPiece.Row, nCol = emptyPiece.Col;

        // we check the click piece is near enough? if not just return.
        if (Mathf.Abs(this.Row - nRow) + Mathf.Abs(this.Col - nCol) > 1)
            return;

        // We set the target to move 
        TargetToMove = SliderPlaygroundController.Instance.GetPositionInGrid(nCol, nRow);

        // We update the empty grid props.
        SliderPlaygroundController.Instance.UpdateEmptyGridData(this.Col, this.Row);

        // We change the slider playground values.
        this.Row = nRow;
        this.Col = nCol;
    }


    public void Load(int pieceIndex, int c, int r)
    {
        // We update row and col properties of piece.
        this.Row = r;
        this.Col = c;
        this.PieceIndex = pieceIndex;
    }

    private void Update()
    {
        // if target not exists return.
        if (!TargetToMove.HasValue)
            return;

        // Transition speed of piece.
        float transitionSpeed = Time.deltaTime * SliderPlaygroundController.Instance.SpeedOfSliding * RectTransform.rect.size.x / 100;

        // We force to move piece through the point.
        RectTransform.anchoredPosition = Vector2.MoveTowards(RectTransform.anchoredPosition, TargetToMove.Value, transitionSpeed);

        // if near enough the end point update its position.
        if (Vector2.Distance(RectTransform.anchoredPosition, TargetToMove.Value) <= 5)
        {
            // We update the position.
            RectTransform.anchoredPosition = TargetToMove.Value;

            // We remove the target.
            TargetToMove = null;

            // We check for finalization.
            SliderPlaygroundController.Instance.CheckForFinalization();
        }
    }

}
