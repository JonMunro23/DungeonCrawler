using System;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    UIController uiController;

    [SerializeField] KeyCode pauseKey = KeyCode.P;
    public static bool isPaused = false;

    [Header("Pause Menu")]
    [SerializeField] GameObject pauseMenu;

    public static Action onPause;
    public static Action onQuit;

    private void Awake()
    {
        uiController = GetComponentInParent<UIController>();
    }

    void Start()
    {
        ResumeGame();
    }

    public void HandleEscapePressed()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (uiController.deleteSaveConfirmPopup.activeSelf)
            {
                uiController.CloseDeleteSaveConfirmation();
                return;
            }

            if (uiController.loadGameConfrimPopup.activeSelf)
            {
                uiController.CloseLoadGameConfirmation();
                return;
            }

            if (uiController.overwriteSaveConfrimPopup.activeSelf)
            {
                uiController.CloseSaveOverwriteConfirmation();
                return;
            }

            if (uiController.isInputtingName)
            {
                uiController.HideSaveNamePopup();
                return;
            }

            if (uiController.saveMenu.activeSelf)
            {
                uiController.CloseSaveMenu();
                return;
            }

            if (uiController.loadMenu.activeSelf)
            {
                uiController.CloseLoadMenu();
                return;
            }

            TogglePauseMenu();
        }
    }

    #region Pause Menu
    void TogglePauseMenu()
    {
        if (!isPaused)
        {
            OpenPauseMenu();
        }
        else
        {
            ClosePauseMenu();
        }

        HelperFunctions.SetCursorActive(isPaused);
    }

    public void ClosePauseMenu()
    {
        isPaused = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
    }

    void OpenPauseMenu()
    {
        isPaused = true;
        onPause?.Invoke();
        pauseMenu.SetActive(true);
        uiController.SetLoadGameButtonsInteractable();
        Time.timeScale = 0;


    }
    public void ResumeGame()
    {
        HelperFunctions.SetCursorActive(false);

        ClosePauseMenu();
    }

    public void QuitToMainMenu()
    {
        //show loading screen
        //set levels inactive
        //show main menu
        ClosePauseMenu();
        onQuit?.Invoke();
        uiController.mainMenu.OpenMainMenu();
    }

    #endregion

}
