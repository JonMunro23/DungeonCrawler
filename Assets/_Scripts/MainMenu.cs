using System;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject mainCamera;
    [SerializeField] GameObject hudCanvas;

    public static bool isInMainMenu = false;

    public static Action onNewGameStarted;

    private void Start()
    {
        HelperFunctions.SetCursorActive(true);
    }

    public void NewGame()
    {
        CloseMainMenu();
        
        //hide main menu
        //show loading screen
        //intro cutscene
        //show character selection

        onNewGameStarted?.Invoke();
    }

    public void OpenMainMenu()
    {
        mainMenu.SetActive(true);
        mainCamera.SetActive(true);
        HelperFunctions.SetCursorActive(true);
        isInMainMenu = true;
    }

    public void CloseMainMenu()
    {
        HelperFunctions.SetCursorActive(false);
        isInMainMenu = false;
        mainMenu.SetActive(false);
    }

    public void SetCameraActive(bool isActive)
    {
        mainCamera.SetActive(isActive);
    }

    public void OpenOptionsMenu()
    {

    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
