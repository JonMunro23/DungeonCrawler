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
    [SerializeField] GameObject loadGameConfrimPopup;
    [SerializeField] TMP_Text LoadGameConfirmPopupText;
    SaveSlot slotToLoad;

    [Header("New Save")]
    [SerializeField] bool isInputtingName;
    [SerializeField] GameObject saveNameInputPopup;
    [SerializeField] TMP_InputField saveNameInputField;
    [SerializeField] Button saveNameSubmitButton;

    [Header("Save Deletion")]
    [SerializeField] GameObject deleteSaveConfirmPopup;
    [SerializeField] TMP_Text deleteSaveConfirmationPopupText;
    SaveSlot slotToDelete;

    [Header("Save Overwrite")]
    [SerializeField] GameObject overwriteSaveConfrimPopup;
    [SerializeField] TMP_Text overwriteSaveConfirmPopupText;
    SaveSlot slotToOverwrite;

    [Header("Game Over")]
    [SerializeField] GameObject gameOverScreen;
    [SerializeField] TMP_Text deathCounterText;
    public int deathCounter;


    private void OnEnable()
    {
        SaveSlot.onCreateNewSaveButtonPressed += DisplaySaveNamePopup;
    }

    private void OnDisable()
    {
        SaveSlot.onCreateNewSaveButtonPressed -= DisplaySaveNamePopup;
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
        deleteSaveConfirmPopup.SetActive(false);
        overwriteSaveConfrimPopup.SetActive(false);
        loadGameConfrimPopup.SetActive(false);
        ResumeGame();
        SaveSystem.GetSavesFromDirectory();
    }

    void Update()
    {
        if (!PlayerController.isPlayerAlive)
            return;

        if(Input.GetKeyDown(pauseKey))
        {
            if(deleteSaveConfirmPopup.activeSelf)
            {
                CloseDeleteSaveConfirmation();
                return;
            }

            if(loadGameConfrimPopup.activeSelf)
            {
                CloseLoadGameConfirmation();
                return;
            }

            if(overwriteSaveConfrimPopup.activeSelf)
            {
                CloseSaveOverwriteConfirmation();
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

    void DeleteSaveConfirmation(SaveSlot slotToDelete)
    {
        this.slotToDelete = slotToDelete;

        deleteSaveConfirmPopup.SetActive(true);
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
        DeleteSave();
        CloseDeleteSaveConfirmation();
    }

    public void CloseDeleteSaveConfirmation()
    {
        deleteSaveConfirmPopup.SetActive(false);
        slotToDelete = null;
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
        clone.slotButton.onClick.AddListener(delegate { LoadGameConfirmation(clone); });
        clone.deleteButton.onClick.AddListener(delegate { DeleteSaveConfirmation(clone); });
        spawnedLoadSlots.Add(clone);
    }

    void LoadGameConfirmation(SaveSlot slotToLoad)
    {
        this.slotToLoad = slotToLoad;

        loadGameConfrimPopup.SetActive(true);
        LoadGameConfirmPopupText.text = $"Load {slotToLoad.slotData.saveName}?";
    }

    public void ConfirmLoadGame()
    {
        slotToLoad.Load();
        CloseLoadGameConfirmation();
        OnSaveLoaded();
    }

    public void CloseLoadGameConfirmation()
    {
        loadGameConfrimPopup.SetActive(false);
        slotToLoad = null;
    }

    void OnSaveLoaded()
    {
        if (!PlayerController.isPlayerAlive)
        {
            gameOverScreen.SetActive(false);
        }

        ResumeGame();
    }
    #endregion

    #region Saving

    void CreateSaveSlot(SaveSystem.SaveData saveData)
    {
        SaveSlot clone = Instantiate(saveSlotPrefab, saveMenuSlotParent);
        clone.Init(saveData);
        clone.slotButton.onClick.AddListener(delegate { OverwriteSaveConfirmation(clone); });
        clone.deleteButton.onClick.AddListener(delegate { DeleteSaveConfirmation(clone); });
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
        SaveSystem.SaveData data = SaveSystem.Save(saveName);
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

    void OverwriteSaveConfirmation(SaveSlot slotToOverwrite)
    {
        this.slotToOverwrite = slotToOverwrite;

        overwriteSaveConfrimPopup.SetActive(true);
        overwriteSaveConfirmPopupText.text = $"Overwrite {slotToOverwrite.slotData.saveName}?";
    }

    public void ConfirmSaveOverwite()
    {
        slotToOverwrite.Save();
        CloseSaveOverwriteConfirmation();
    }

    public void CloseSaveOverwriteConfirmation()
    {
        overwriteSaveConfrimPopup.SetActive(false);
        slotToOverwrite = null;
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
