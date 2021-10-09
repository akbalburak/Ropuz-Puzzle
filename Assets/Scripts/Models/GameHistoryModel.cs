using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameHistoryModel
{
    /// <summary>
    /// Unity always generate with the same name order, so we can just store the names to reload game.
    /// </summary>
    public List<GameHistoryPieceModel> HistoryPieces;
}

[Serializable]
public class GameHistoryPieceModel
{
    [Header("Piece index number for slider puzzle.")]
    public int PieceIndex;

    [Header("Rotate puzle for piece name.")]
    public string PieceName;

    [Header("Jigsaw piece x position.")]
    public float X;

    [Header("Jigsaw piece y position.")]
    public float Y;

    [Header("Jigsaw column index.")]
    public int Col;

    [Header("Jigsaw row index.")]
    public int Row;

    /// <summary>
    /// We use this method to load rotate puzzle.
    /// </summary>
    /// <param name="pieceName"></param>
    public GameHistoryPieceModel(string pieceName)
    {
        this.PieceName = pieceName;
    }

    /// <summary>
    /// We use this constuructor when we save jigsaw model.
    /// </summary>
    /// <param name="x">X position of piece.</param>
    /// <param name="y">Y position of piece.</param>
    /// <param name="col">Column index of piece.</param>
    /// <param name="row">Row index of piece.</param>
    public GameHistoryPieceModel(float x, float y, int col, int row)
    {
        this.X = x;
        this.Y = y;
        this.Col = col;
        this.Row = row;
    }

    /// <summary>
    /// Slider puzzle save props.
    /// </summary>
    /// <param name="pieceIndex">Piece unique index. To reorder when user reenter the level.</param>
    /// <param name="col">Column index of piece.</param>
    /// <param name="row">Row index of piece.</param>
    public GameHistoryPieceModel(int pieceIndex, int col, int row)
    {
        this.PieceIndex = pieceIndex;
        this.Col = col;
        this.Row = row;
    }
}
