using System;
using UnityEngine;

[Serializable]
public class SaveLoadModel
{
    [Header("Player reached maximum level.")]
    public int MaxReachedLevel;

    [Header("Is sound active?")]
    public bool IsSoundActive;

    [Header("Is music active?")]
    public bool IsMusicActive;

    [Header("Player score.")]
    public int ActionScore;

    [Header("Game langauge.")]
    public Languages Language;
}
