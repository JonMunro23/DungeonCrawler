using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject loadMenu;

    [Header("Save Menu")]
    [SerializeField] GameObject saveMenu;
    [SerializeField] SaveSlot saveSlotPrefab;
    [SerializeField] Transform saveSlotParent;
    [SerializeField] List<SaveSlot> spawnedSaveSlots = new List<SaveSlot>();

    public static bool isPaused = false;

    // Start is called before the first frame update
    void Start()
    {
        SpawnSaveSlots();
        ResumeGame();
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
        saveMenu.SetActive(false);
        //loadMenu.SetActive(false);
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
        HelperFunctions.SetCursorActive(false);
        ClosePauseMenu();
    }

    public void OpenSaveMenu()
    {
        saveMenu.SetActive(true);
        pauseMenu.SetActive(false);

        SpawnSaveSlots();
    }

    private void SpawnSaveSlots()
    {
        foreach (var item in spawnedSaveSlots)
        {
            Destroy(item.gameObject);
        }
        spawnedSaveSlots.Clear();

        foreach (var item in SaveSystem.GetSaves())
        {
            var clone = Instantiate(saveSlotPrefab, saveSlotParent);
            clone.Init(item.Key, item.Value);
            spawnedSaveSlots.Add(clone);
        }
    }

    public void CloseSaveMenu()
    {
        saveMenu.SetActive(false);
        pauseMenu.SetActive(true);
    }

    public void OpenLoadMenu()
    {

    }
}
