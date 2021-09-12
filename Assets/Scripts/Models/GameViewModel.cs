
using System;
using UnityEngine;

[Serializable]
public class GameViewModel
{
    [Header("View identity.")]
    public GameViews GameView;

    [Header("View when view is active will be enabled.")]
    public GameObject GameViewPanel;
}
