using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] KeyCode pauseKey = KeyCode.P;
    [SerializeField] GameObject pauseMenu;
    [SerializeField] List<SaveSlot> spawnedSaveSlots = new List<SaveSlot>();
    [SerializeField] List<SaveSlot> spawnedLoadSlots = new List<SaveSlot>();

    [Header("Save Menu")]
    [SerializeField] GameObject saveMenu;
    [SerializeField] SaveSlot saveSlotPrefab;
    [SerializeField] Transform saveMenuSlotParent;

    [Header("Load Menu")]
    [SerializeField] GameObject loadMenu;
    [SerializeField] SaveSlot loadSlotPrefab;
    [SerializeField] Transform loadMenuSlotParent;

    [Header("New Save")]
    [SerializeField] bool isInputtingName;
    [SerializeField] GameObject saveNameInputPopup;
    [SerializeField] TMP_InputField saveNameInputField;
    [SerializeField] Button saveNameSubmitButton;

    public static bool isPaused = false;

    private void OnEnable()
    {
        SaveSlot.onSaveLoaded += OnSaveLoaded;
        SaveSlot.onSaveDeleted += OnSaveDeleted;
        SaveSlot.onCreateNewSaveButtonPressed += DisplaySaveNamePopup;
    }

    private void OnDisable()
    {
        SaveSlot.onSaveLoaded -= OnSaveLoaded;
        SaveSlot.onSaveDeleted -= OnSaveDeleted;
        SaveSlot.onCreateNewSaveButtonPressed -= DisplaySaveNamePopup;
    }

    void OnSaveLoaded()
    {
        ResumeGame();
    }

    void OnSaveDeleted(SaveSlot slotToDelete)
    {
        if(spawnedSaveSlots.Contains(slotToDelete))
            spawnedSaveSlots.Remove(slotToDelete);

        if(spawnedLoadSlots.Contains(slotToDelete))
            spawnedLoadSlots.Remove(slotToDelete);

        Destroy(slotToDelete.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        //Ensure that name of save is only submitted when pressing ENTER or when the button is pressed
        saveNameInputField.onEndEdit.AddListener(val =>
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                SubmitName();
        });

        ResumeGame();
        SaveSystem.GetSavesFromDirectory();
    }

    private void Update()
    {
        if(Input.GetKeyDown(pauseKey))
        {
            if(isInputtingName)
            {
                HideSaveNamePopup();
                return;
            }

            if(saveMenu.activeSelf)
            {
                CloseSaveMenu();
                return;
            }

            if(loadMenu.activeSelf)
            {
                CloseLoadMenu();
                return;
            }

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
        loadMenu.SetActive(false);
        HideSaveNamePopup();
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
        if(!ItemPickupManager.hasGrabbedItem && !PlayerInventoryManager.isInContainer && !PlayerInventoryUIController.isInventoryOpen)
            HelperFunctions.SetCursorActive(false);

        ClosePauseMenu();
    }


    private void SpawnSaveSlots()
    {
        foreach (SaveSlot item in spawnedSaveSlots)
        {
            Destroy(item.gameObject);
        }
        spawnedSaveSlots.Clear();

        foreach (SaveSystem.SaveData saveData in SaveSystem.GetSaveData())
        {
            CreateSaveSlot(saveData);
        }
    }

    private void SpawnLoadSlots()
    {
        foreach (SaveSlot item in spawnedLoadSlots)
        {
            Destroy(item.gameObject);
        }
        spawnedLoadSlots.Clear();

        foreach (SaveSystem.SaveData saveData in SaveSystem.GetSaveData())
        {
            //Debug.Log(saveData.saveName);
            CreateLoadSlot(saveData);
        }
    }

    void CreateSaveSlot(SaveSystem.SaveData saveData)
    {
        SaveSlot clone = Instantiate(saveSlotPrefab, saveMenuSlotParent);
        clone.Init(saveData);
        spawnedSaveSlots.Add(clone);
    }

    private void CreateLoadSlot(SaveSystem.SaveData saveData)
    {
        var clone = Instantiate(loadSlotPrefab, loadMenuSlotParent);
        clone.Init(saveData);
        spawnedLoadSlots.Add(clone);
    }
    public void OpenSaveMenu()
    {
        saveMenu.SetActive(true);
        pauseMenu.SetActive(false);

        SpawnSaveSlots();
    }

    public void CloseSaveMenu()
    {
        saveMenu.SetActive(false);
        pauseMenu.SetActive(true);
    }

    public void CloseLoadMenu()
    {
        loadMenu.SetActive(false);
        pauseMenu.SetActive(true);
    }

    public void OpenLoadMenu()
    {
        pauseMenu.SetActive(false);
        loadMenu.SetActive(true);

        SpawnLoadSlots();
    }

    public void CreateNewSave(string saveName)
    {
        int saveIndex = SaveSystem.saveDatas.Count;
        SaveSystem.SaveData data = SaveSystem.Save(saveIndex, saveName);
        CreateSaveSlot(data);
    }

    void DisplaySaveNamePopup()
    {
        isInputtingName = true;
        saveNameInputPopup.SetActive(true);
        saveNameInputField.ActivateInputField();
    }

    public void HideSaveNamePopup()
    {
        isInputtingName = false;
        saveNameInputPopup.SetActive(false);
        saveNameInputField.DeactivateInputField();
    }

    public void SubmitName()
    {
        HideSaveNamePopup();
        CreateNewSave(saveNameInputField.text);
        saveNameInputField.text = "";
    }

    public void ValidateInputField()
    {
        if (saveNameInputField.text != "")
            saveNameSubmitButton.interactable = true;
        else
            saveNameSubmitButton.interactable = false;
    }
}
