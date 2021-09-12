using UnityEngine;

public class LanguageGameViewController : MonoBehaviour, IGameViewPanel
{
    public GameViews PreviousGameView;
    private void OnEnable()
    {
        // if view not init yet just return.
        if (GameViewController.Instance == null)
            return;

        // We receive last view before change it.
        PreviousGameView = GameViewController.Instance.CurrentView;
    }
    public void OnClickChangeLanguage(int language)
    {
        LanguageController.Instance.ChangeLanguage((Languages)language);

        // To return previous page we check which page we were in.
        if (PreviousGameView == GameViews.LevelMenu || PreviousGameView == GameViews.CustomLevelMenu)
            GameViewController.Instance.ActivateView(PreviousGameView);
        else // We are going to return to the level menu if non of this page was the previous page.
            GameViewController.Instance.ActivateView(GameViews.LevelMenu);
    }

    public void OnGameViewActivated()
    {
    }

    public void OnGameViewDeactivated()
    {
    }
}
