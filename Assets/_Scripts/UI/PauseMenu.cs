using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject saveMenu;
    [SerializeField] GameObject loadMenu;

    public static bool isPaused = false;

    // Start is called before the first frame update
    void Start()
    {
        ClosePauseMenu();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            TogglePauseMenu();
        }
    }
    private void TogglePauseMenu()
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
    private void ClosePauseMenu()
    {
        isPaused = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
    }
    private void OpenPauseMenu()
    {
        isPaused = true;
        pauseMenu.SetActive(true);
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        ClosePauseMenu();
        HelperFunctions.SetCursorActive(false);
    }

    public void OpenSaveMenu()
    {

    }

    public void OpenLoadMenu()
    {

    }
}
