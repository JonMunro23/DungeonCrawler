using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject mainMenu, mainMenuCamera;

    public static bool isInMainMenu = true;

    public static Action onNewGameStarted;

    private void Start()
    {
        HelperFunctions.SetCursorActive(true);
    }

    public void NewGame()
    {
        CloseMainMenu();
        
        onNewGameStarted?.Invoke();
        //hide main menu
        //show loading screen
        //intro cutscene
        //show character selection
    }

    public void LoadGame()
    {

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
        mainMenuCamera.SetActive(false);
        HelperFunctions.SetCursorActive(false);
        isInMainMenu = false;
        mainMenu.SetActive(false);
    }

    public void OpenOptionsMenu()
    {

    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
