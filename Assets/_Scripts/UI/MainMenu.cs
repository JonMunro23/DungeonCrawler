using System;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject mainMenuCamera;
    [SerializeField] GameObject hudCanvas;
    [SerializeField] bool startFromMainMenu = false;
    [SerializeField] bool skipIntro = false;
    public static bool isInMainMenu = false;

    public static Action onNewGameStarted;
    public static Action onNewGameStartedSkippedIntro;

    private void Start()
    {
        HelperFunctions.SetCursorActive(true);
        if(startFromMainMenu)
            OpenMainMenu();
    }

    public void NewGame()
    {
        CloseMainMenu();
        
        //hide main menu
        //show loading screen
        //intro cutscene
        //show character selection

        if(skipIntro)
            onNewGameStartedSkippedIntro?.Invoke();
        else
            onNewGameStarted?.Invoke();
    }

    public void OpenMainMenu()
    {
        mainMenu.SetActive(true);
        mainMenuCamera.SetActive(true);
        HelperFunctions.SetCursorActive(true);
        isInMainMenu = true;
    }

    public void CloseMainMenu()
    {
        HelperFunctions.SetCursorActive(false);
        isInMainMenu = false;
        mainMenu.SetActive(false);
        mainMenuCamera.SetActive(false);
    }

    public void SetCameraActive(bool isActive)
    {
        mainMenuCamera.SetActive(isActive);
    }

    public void OpenOptionsMenu()
    {

    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
