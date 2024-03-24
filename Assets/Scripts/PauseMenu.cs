using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public static GameObject pauseMenu;

    [SerializeField]
    GameObject menuObject;

    public static bool isPaused = false;
    // Start is called before the first frame update
    void Start()
    {
        pauseMenu = menuObject;
        pauseMenu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Escape"))
        {
            if(isPaused != true)
            {
                pauseMenu.SetActive(true);
                Time.timeScale = 0;
                isPaused = true;
            }
            else
            {
                pauseMenu.SetActive(false);
                Time.timeScale = 1;
                isPaused = false;
            }
        }
    }
}
