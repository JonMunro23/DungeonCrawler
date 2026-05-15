using System;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    UIController uiController;

    public static bool isPaused = false;

    [Header("Pause Menu")]
    [SerializeField] GameObject pauseMenu;

    public static event Action onPause;
    public static event Action onQuit;

    private void Awake()
    {
        uiController = GetComponentInParent<UIController>();
    }

    void Start()
    {
        ResumeGame();
    }

    #region Pause Menu
    public void TogglePauseMenu()
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
