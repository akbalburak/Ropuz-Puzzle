using System.Collections;
using UnityEngine;

public class LoadingGameViewController : MonoBehaviour, IGameViewPanel
{
    [Header("Wait time before redirect to the destination view.")]
    public float WaitForSecondsBeforeChangeView;

    [Header("Which will be redirected to this view.")]
    public GameViews RedirectToGameView;

    public void OnGameViewActivated()
    {
        // We ar eactivating the redirection.
        StartCoroutine(RedirectToLevelMenu());
    }


    public IEnumerator RedirectToLevelMenu()
    {
        // We have to wait be fore redirect.
        yield return new WaitForSeconds(WaitForSecondsBeforeChangeView);

        // if no language selected we will force to select language.
        if (SaveLoadController.Instance.SaveData.Language == Languages.None)
            GameViewController.Instance.ActivateView(GameViews.Language);
        else // Otherwis we will show action view.
            GameViewController.Instance.ActivateView(RedirectToGameView);
    }

    public void OnGameViewDeactivated()
    {
    }
}
