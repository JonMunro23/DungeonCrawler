using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerScript : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadSceneAsync("TestLevel");
    }

    public void OptionsFromMenu()
    {

    }

    public void LoadGameFromMenu()
    {

    }

    public void ResumeGame()
    {
        PauseMenu.pauseMenu.SetActive(false);
        Time.timeScale = 1;
        PauseMenu.isPaused = false;
    }

    public void Retry()
    {
        SceneManager.LoadSceneAsync("TestLevel");
        Time.timeScale = 1;
        PauseMenu.isPaused = false;
    }

    public void SaveGame()
    {

    }

    public void LoadGameFromPause()
    {

    }

    public void OptionsFromPause()
    {

    }

    public void QuitToMenu()
    {
        SceneManager.LoadSceneAsync("MainMenu");
        Time.timeScale = 1;
        PauseMenu.isPaused = false;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
