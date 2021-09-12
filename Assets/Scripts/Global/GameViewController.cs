using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameViewController : MonoBehaviour
{
    public static GameViewController Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    [Header("When the app start, which view will be loaded.")]
    public GameViews StartView;

    [Header("All active game views.")]
    public List<GameViewModel> GameViews;

    [Header("Active veiw.")]
    public GameViews CurrentView;

    private void Start()
    {
        // We are activating the default view.
        ActivateView(StartView);
    }

    public IGameViewPanel ActivateView(GameViews gameView)
    {
        // View which will open.
        GameViewModel openView = GameViews.FirstOrDefault(x => x.GameView == gameView);

        // if there is no view in the game view list return after throw exception.
        if (openView == null)
        {
            // We are writing error to the log.
            Debug.LogError($"{gameView} not exists in game view list.");

            // Then we return.
            return null;
        }

        // All of the views will be disabled.
        GameViews.ForEach(e =>
        {
            // We make sure it has a panel and this panel is active.
            if (e.GameViewPanel != null && e.GameViewPanel.activeSelf)
            {
                // We have to say we are closing you.
                IGameViewPanel iclosedGameView = e.GameViewPanel.GetComponent<IGameViewPanel>();

                // We have to make sure it is exists.
                if (iclosedGameView != null)
                    iclosedGameView.OnGameViewDeactivated();

                // if it is active then we will disable.
                e.GameViewPanel.SetActive(false);
            }
        });

        // And we are activating the new view.
        openView.GameViewPanel.SetActive(true);

        // We are going to trigger because we activated the panel.
        IGameViewPanel iOpenedGameView = openView.GameViewPanel.GetComponent<IGameViewPanel>();

        // Current view.
        this.CurrentView = gameView;

        // We have to make sure component exists.
        if (iOpenedGameView != null)
            iOpenedGameView.OnGameViewActivated();

        // We return the view.
        return iOpenedGameView;
    }
}
