using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("References")]
    public PlayerStatsUIController playerStatsUIController;
    public PlayerInventoryUIController playerInventoryUIController;
    [SerializeField] PlayerEquipmentUIManager PlayerEquipmentUIManager;
    [SerializeField] PlayerWeaponUIManager playerWeaponUIManager;
    public PlayerSkillsUIManager playerSkillsUIManager;

    [Header("Pause Menu")]
    [SerializeField] PauseMenu pauseMenu;

    [Header("Main Menu")]
    public MainMenu mainMenu;

    [Header("Save Menu")]
    public GameObject saveMenu;
    [SerializeField] SaveSlot saveSlotPrefab;
    [SerializeField] Transform saveMenuSlotParent;
    [SerializeField] List<SaveSlot> spawnedSaveSlots = new List<SaveSlot>();

    [Header("Load Menu")]
    public GameObject loadMenu;
    [SerializeField] SaveSlot loadSlotPrefab;
    [SerializeField] Transform loadMenuSlotParent;
    public GameObject loadGameConfrimPopup;
    [SerializeField] TMP_Text LoadGameConfirmPopupText;
    [SerializeField] List<SaveSlot> spawnedLoadSlots = new List<SaveSlot>();
    [SerializeField] List<Button> loadGameButtons = new List<Button>();
    SaveSlot slotToLoad;

    [Header("Game Over")]
    [SerializeField] GameObject gameOverScreen;
    [SerializeField] TMP_Text deathCounterText;
    public int deathCounter;

    [Header("New Save")]
    public bool isInputtingName;
    [SerializeField] GameObject saveNameInputPopup;
    [SerializeField] TMP_InputField saveNameInputField;
    [SerializeField] Button saveNameSubmitButton;

    [Header("Save Deletion")]
    public GameObject deleteSaveConfirmPopup;
    [SerializeField] TMP_Text deleteSaveConfirmationPopupText;
    SaveSlot slotToDelete;

    [Header("Save Overwrite")]
    public GameObject overwriteSaveConfrimPopup;
    [SerializeField] TMP_Text overwriteSaveConfirmPopupText;
    SaveSlot slotToOverwrite;

    [Header("Quick Saving")]
    [SerializeField] TMP_Text saveStatusText;
    [SerializeField] float saveStatusTextFadeDuration;

    [Header("Level Transition")]
    [SerializeField] GameObject levelTransitionParent;
    [SerializeField] TMP_Text levelTransitionText;
    [SerializeField] TMP_Text levelTransitionEnteringText;
    [SerializeField] Image levelTransitionDividingLine;
    [SerializeField] float levelTextLifetimeDuration = 5;
    [SerializeField] Image levelTransitionFadeOverlay;
    [SerializeField] float fadeOutDuration, fadeInDuration;

    PlayerController initialisedPlayer;

    Coroutine levelTextLifetime;

    WeaponItemData defaultWeaponData;

    private void OnEnable()
    {
        PlayerController.onPlayerInitialised += OnPlayerInitialised;
        PlayerController.onPlayerDeath += OnPlayerDeath;

        WorldInteractionManager.onNewItemAttachedToCursor += OnNewItemAttachedToCursor;
        WorldInteractionManager.onCurrentItemDettachedFromCursor += OnCurrentItemRemovedFromCursor;

        WeaponSlot.onWeaponRemovedFromSlot += OnWeaponRemovedFromSlot;
        WeaponSlot.onWeaponSwappedInSlot += OnWeaponSwappedInSlot;
        WeaponSlot.onWeaponSetToDefault += OnWeaponSetToDefault;

        Weapon.onLoadedAmmoUpdated += OnWeaponLoadedAmmoUpdated;
        Weapon.onReserveAmmoUpdated += OnWeaponReserveAmmoUpdated;

        PlayerWeaponManager.onWeaponSlotSetActive += OnWeaponSlotSetActive;
        PlayerWeaponManager.onNewWeaponInitialised += OnNewWeaponInitialised;

        LevelTransition.onLevelTransitionEntered += OnLevelTransitionEntered;

        GridController.onQuickSave += OnQuickSave;

        SaveSlot.onCreateNewSaveButtonPressed += DisplaySaveNamePopup;
    }

    private void OnDisable()
    {
        PlayerController.onPlayerInitialised -= OnPlayerInitialised;
        PlayerController.onPlayerDeath -= OnPlayerDeath;

        WorldInteractionManager.onNewItemAttachedToCursor -= OnNewItemAttachedToCursor;
        WorldInteractionManager.onCurrentItemDettachedFromCursor -= OnCurrentItemRemovedFromCursor;

        WeaponSlot.onWeaponRemovedFromSlot -= OnWeaponRemovedFromSlot;
        WeaponSlot.onWeaponSwappedInSlot -= OnWeaponSwappedInSlot;
        WeaponSlot.onWeaponSetToDefault -= OnWeaponSetToDefault;

        Weapon.onLoadedAmmoUpdated -= OnWeaponLoadedAmmoUpdated;
        Weapon.onReserveAmmoUpdated -= OnWeaponReserveAmmoUpdated;

        PlayerWeaponManager.onNewWeaponInitialised -= OnNewWeaponInitialised;
        PlayerWeaponManager.onWeaponSlotSetActive -= OnWeaponSlotSetActive;

        LevelTransition.onLevelTransitionEntered -= OnLevelTransitionEntered;

        GridController.onQuickSave -= OnQuickSave;

        SaveSlot.onCreateNewSaveButtonPressed -= DisplaySaveNamePopup;
    }

    private void Start()
    {
        SaveSystem.GetSavesFromDirectory();

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
        levelTransitionFadeOverlay.enabled = true;

        SetLoadGameButtonsInteractable();
    }

    void OnPlayerInitialised(PlayerController playerInitialised)
    {
        initialisedPlayer = playerInitialised;

        playerStatsUIController.InitStatsUI(initialisedPlayer);

        _ = FadeInScreen();
    }

    void OnPlayerDeath()
    {
        ShowGameOverScreen();
    }

    void OnNewItemAttachedToCursor(ItemStack item)
    {
        WeaponItemData handItemData = item.itemData as WeaponItemData;
        if (handItemData != null)
        {
            PlayerEquipmentUIManager.DisableAllSlots();
            return;
        }

        EquipmentItemData equipItemData = item.itemData as EquipmentItemData;
        if (equipItemData != null)
        {
            PlayerEquipmentUIManager.DisableSlotsNotOfType(equipItemData.EquipmentSlotType);
            playerWeaponUIManager.DisableSlots();
            return;
        }

        playerWeaponUIManager.DisableSlots();
        PlayerEquipmentUIManager.DisableAllSlots();
    }

    void OnCurrentItemRemovedFromCursor()
    {
        PlayerEquipmentUIManager.RenableSlots();
        playerWeaponUIManager.RenableSlots();
    }

    void OnNewWeaponInitialised(int slotIndex, WeaponItemData newItemData)
    {
        playerWeaponUIManager.UpdateWeaponDisplayImages(slotIndex, newItemData);
    }

    void OnWeaponSwappedInSlot(int slotIndex, WeaponItemData dataToSwapTo, int loadedAmmo)
    {
        playerWeaponUIManager.UpdateWeaponDisplayImages(slotIndex, dataToSwapTo);
    }

    void OnWeaponRemovedFromSlot(int slotIndex)
    {
        playerWeaponUIManager.UpdateWeaponDisplayImages(slotIndex, defaultWeaponData);
    }

    void OnWeaponSetToDefault(int slotIndex, WeaponItemData _defaultWeaponData)
    {
        defaultWeaponData = _defaultWeaponData;
        playerWeaponUIManager.UpdateWeaponDisplayImages(slotIndex, _defaultWeaponData);
    }

    void OnWeaponReserveAmmoUpdated(int slotIndex, int reserve)
    {
        playerWeaponUIManager.UpdateWeaponDisplayReserveAmmoCount(slotIndex, reserve);
    }
    void OnWeaponLoadedAmmoUpdated(int slotIndex, int loaded)
    {
        playerWeaponUIManager.UpdateWeaponDisplayLoadedAmmoCount(slotIndex, loaded);
    }

    void OnWeaponSlotSetActive(int slotIndex)
    {
        playerWeaponUIManager.SetSlotActive(slotIndex);
    }

    async void OnLevelTransitionEntered(int levelIndex, Vector2 playerMoveToCoords)
    {
        await FadeOutScreen();
        await GridController.Instance.BeginLevelTransition(levelIndex, playerMoveToCoords);
        await FadeInScreen();
        ShowLevelName(levelIndex);
    }

    void OnQuickSave()
    {
        saveStatusText.color = new Color(1, 0.9529412f, 0, 1);
        saveStatusText.DOFade(0, saveStatusTextFadeDuration).SetDelay(3);
    }

    void ShowLevelName(int levelIndex)
    {
        levelTransitionParent.SetActive(true);
        levelTransitionText.color = new Color(1,1,1,1);
        levelTransitionEnteringText.color = new Color(1,1,1,1);
        levelTransitionDividingLine.color = new Color(.51f,.51f,.51f,1);
        levelTransitionText.text = GridController.Instance.GetLevelNameFromIndex(levelIndex);

        if(levelTextLifetime != null)
            StopCoroutine(levelTextLifetime);

        levelTextLifetime = StartCoroutine(LevelNameLifetime());
    }

    void HideLevelName()
    {
        levelTransitionText.DOFade(0, 1);
        levelTransitionEnteringText.DOFade(0, 1);
        levelTransitionDividingLine.DOFade(0, 1);
    }

    async Task FadeInScreen()
    {
        levelTransitionFadeOverlay.DOFade(0, fadeInDuration);
        await Task.Delay((int)(fadeInDuration * 1000));
    }

    async Task FadeOutScreen()
    {
        levelTransitionFadeOverlay.DOFade(1, fadeOutDuration);
        await Task.Delay((int)(fadeOutDuration * 1000));
    }

    IEnumerator LevelNameLifetime()
    {
        yield return new WaitForSeconds(levelTextLifetimeDuration);
        HideLevelName();
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
    public void SetLoadGameButtonsInteractable()
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
        gameOverScreen.SetActive(false);

        ResumeGame();
    }

    private void ResumeGame()
    {
        if(MainMenu.isInMainMenu)
        {
            mainMenu.CloseMainMenu();
            mainMenu.SetCameraActive(false);
        }

        if (PauseMenu.isPaused)
            pauseMenu.ClosePauseMenu();

        saveMenu.SetActive(false);
        loadMenu.SetActive(false);
        HideSaveNamePopup();
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
