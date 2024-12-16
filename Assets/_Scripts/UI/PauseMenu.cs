using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] KeyCode pauseKey = KeyCode.P;
    [SerializeField] List<SaveSlot> spawnedSaveSlots = new List<SaveSlot>();
    [SerializeField] List<SaveSlot> spawnedLoadSlots = new List<SaveSlot>();
    public static bool isPaused = false;

    [Header("Pause Menu")]
    [SerializeField] GameObject pauseMenu;
    [SerializeField] List<Button> loadGameButtons = new List<Button>();

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

    [Header("Save Deletion")]
    [SerializeField] GameObject deleteSaveConfirmationPopup;
    [SerializeField] TMP_Text deleteSaveConfirmationPopupText;
    SaveSlot slotToDelete;

    [Header("Game Over")]
    [SerializeField] GameObject gameOverScreen;
    [SerializeField] TMP_Text deathCounterText;
    public int deathCounter;


    private void OnEnable()
    {
        SaveSlot.onSaveLoaded += OnSaveLoaded;
        SaveSlot.onSaveDeleteButtonPressed += OnSaveDeleteButtonPressed;
        SaveSlot.onCreateNewSaveButtonPressed += DisplaySaveNamePopup;
    }

    private void OnDisable()
    {
        SaveSlot.onSaveLoaded -= OnSaveLoaded;
        SaveSlot.onSaveDeleteButtonPressed -= OnSaveDeleteButtonPressed;
        SaveSlot.onCreateNewSaveButtonPressed -= DisplaySaveNamePopup;
    }

    void OnSaveLoaded()
    {
        if(!PlayerController.isPlayerAlive)
        {
            gameOverScreen.SetActive(false);
        }

        ResumeGame();
    }

    void Start()
    {
        //Ensure that name of save is only submitted when pressing ENTER or when the button is pressed
        saveNameInputField.onEndEdit.AddListener(val =>
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                SubmitName();
        });
        gameOverScreen.SetActive(false);
        deleteSaveConfirmationPopup.SetActive(false);
        ResumeGame();
        SaveSystem.GetSavesFromDirectory();
    }

    void Update()
    {
        if (!PlayerController.isPlayerAlive)
            return;

        if(Input.GetKeyDown(pauseKey))
        {
            if(deleteSaveConfirmationPopup.activeSelf)
            {
                CancelDeleteSave();
                return;
            }

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

    #region Save Deletion

    void OnSaveDeleteButtonPressed(SaveSlot slotToDelete)
    {
        this.slotToDelete = slotToDelete;

        deleteSaveConfirmationPopup.SetActive(true);
        deleteSaveConfirmationPopupText.text = $"Delete save {slotToDelete.slotData.saveName}?";
    }

    void DeleteSave()
    {
        SaveSystem.DeleteSaveData(slotToDelete.slotData);

        if (spawnedSaveSlots.Contains(slotToDelete))
            spawnedSaveSlots.Remove(slotToDelete);

        if (spawnedLoadSlots.Contains(slotToDelete))
            spawnedLoadSlots.Remove(slotToDelete);

        Destroy(slotToDelete.gameObject);

        if (SaveSystem.GetSaveData().Count == 0)
        {
            if (loadMenu.activeSelf)
                CloseLoadMenu();
        }
    }

    public void ConfirmDeleteSave()
    {
        deleteSaveConfirmationPopup.SetActive(false);
        DeleteSave();
    }

    public void CancelDeleteSave()
    {
        deleteSaveConfirmationPopup.SetActive(false);
    }

    #endregion

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

    void ClosePauseMenu()
    {
        isPaused = false;
        pauseMenu.SetActive(false);
        saveMenu.SetActive(false);
        loadMenu.SetActive(false);
        HideSaveNamePopup();
        Time.timeScale = 1;
    }

    void OpenPauseMenu()
    {
        isPaused = true;
        pauseMenu.SetActive(true);
        Time.timeScale = 0;

        SetLoadGameButtonsInteractable();
    }
    public void ResumeGame()
    {
        if(!ItemPickupManager.hasGrabbedItem && !PlayerInventoryManager.isInContainer && !PlayerInventoryUIController.isInventoryOpen)
            HelperFunctions.SetCursorActive(false);

        ClosePauseMenu();
    }

    #endregion

    #region Loading
    public void OpenLoadMenu()
    {
        loadMenu.SetActive(true);

        SpawnLoadSlots();
    }
    public void CloseLoadMenu()
    {
        loadMenu.SetActive(false);
        SetLoadGameButtonsInteractable();
    }
    void SetLoadGameButtonsInteractable()
    {
        foreach (Button button in loadGameButtons)
        {
            if (SaveSystem.GetSaveData().Count == 0)
                button.interactable = false;
            else
                button.interactable = true;
        }
    }
    void SpawnLoadSlots()
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
    void CreateLoadSlot(SaveSystem.SaveData saveData)
    {
        var clone = Instantiate(loadSlotPrefab, loadMenuSlotParent);
        clone.Init(saveData);
        spawnedLoadSlots.Add(clone);
    }
    #endregion

    #region Saving

    void CreateSaveSlot(SaveSystem.SaveData saveData)
    {
        SaveSlot clone = Instantiate(saveSlotPrefab, saveMenuSlotParent);
        clone.Init(saveData);
        spawnedSaveSlots.Add(clone);
    }

    public void OpenSaveMenu()
    {
        saveMenu.SetActive(true);

        SpawnSaveSlots();
    }

    void SpawnSaveSlots()
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

    public void CloseSaveMenu()
    {
        saveMenu.SetActive(false);

        SetLoadGameButtonsInteractable();
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

    #endregion

    #region Game Over

    public void ShowGameOverScreen()
    {
        gameOverScreen.SetActive(true);

        SetLoadGameButtonsInteractable();

        //deathCounterText.text = deathCounter.ToString();
        HelperFunctions.SetCursorActive(true);
        Time.timeScale = 0;
    }

    #endregion
}
