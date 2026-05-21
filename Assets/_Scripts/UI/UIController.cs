using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] CharacterMenuUIController characterMenuUIController;
    [SerializeField] MapController mapController;
    [SerializeField] CrosshairController crosshairController;
    PlayerControls controls;

    [Header("Secrets")]
    [SerializeField] SecretDiscoveryUI secretDiscoveryUI;

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
    [SerializeField] List<UnityEngine.UI.Button> loadGameButtons = new List<UnityEngine.UI.Button>();
    SaveSlot slotToLoad;

    [Header("Game Over")]
    [SerializeField] GameObject gameOverScreen;
    [SerializeField] TMP_Text deathCounterText;
    public int deathCounter;

    [Header("New Save")]
    public bool isInputtingName;
    [SerializeField] GameObject saveNameInputPopup;
    [SerializeField] TMP_InputField saveNameInputField;
    [SerializeField] UnityEngine.UI.Button saveNameSubmitButton;

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
    Coroutine transitionLevelCoroutine;

    PlayerController initialisedPlayer;

    Coroutine levelTextLifetime;

    public static bool isTransitioningLevel;

    // ==========================
    #region Unity Lifecycle

    private void OnEnable()
    {
        PlayerInputHandler.OnPlayerControlsInitialised += OnPlayerControlsInitialised;

        PlayerController.onPlayerInitialised += OnPlayerInitialised;
        PlayerController.onPlayerDeath += OnPlayerDeath;

        LevelTransition.onLevelTransitionEntered += OnLevelTransitionEntered;

        SecretAreaTrigger.onSecretDiscovered += OnSecretDiscovered;

        PlayerController.onQuickSave += OnQuickSave;

        SaveSlot.onCreateNewSaveButtonPressed += DisplaySaveNamePopup;
    }

    private void OnDisable()
    {
        PlayerInputHandler.OnPlayerControlsInitialised -= OnPlayerControlsInitialised;

        if(controls != null)
        {
            controls.UIControls.Inventory.performed -= OnInventoryButtonPressed;
            controls.UIControls.Skills.performed -= OnSkillsButtonPressed;
            controls.UIControls.Stats.performed -= OnStatsButtonPressed;
            controls.UIControls.Map.performed -= OnMapButtonPressed;
            controls.UIControls.Pause.performed -= OnPauseButtonPressed;
        }

        PlayerController.onPlayerInitialised -= OnPlayerInitialised;
        PlayerController.onPlayerDeath -= OnPlayerDeath;

        LevelTransition.onLevelTransitionEntered -= OnLevelTransitionEntered;

        SecretAreaTrigger.onSecretDiscovered -= OnSecretDiscovered;

        PlayerController.onQuickSave -= OnQuickSave;

        SaveSlot.onCreateNewSaveButtonPressed -= DisplaySaveNamePopup;
    }

    private void Start()
    {
        SaveSystem.GetSavesFromDirectory();

        //Ensure that name of save is only submitted when pressing ENTER or when the button is pressed
        saveNameInputField.onEndEdit.AddListener(val =>
        {
            if (controls.UIControls.Enter.WasPressedThisFrame())
                SubmitName();
        });
        gameOverScreen.SetActive(false);
        deleteSaveConfirmPopup.SetActive(false);
        overwriteSaveConfrimPopup.SetActive(false);
        loadGameConfrimPopup.SetActive(false);
        levelTransitionFadeOverlay.enabled = true;

        SetLoadGameButtonsInteractable();
    }

    private void Update()
    {
        mapController.HandlePanning();
        mapController.HandleZoom();
    }

    #endregion
    // ==========================

    // ==========================
    #region Event Handlers

    void OnInventoryButtonPressed(InputAction.CallbackContext ctx)
    {
        characterMenuUIController.ToggleInventoryPanel();
    }

    void OnSkillsButtonPressed(InputAction.CallbackContext ctx)
    {
        characterMenuUIController.ToggleSkillsPanel();
    }

    void OnStatsButtonPressed(InputAction.CallbackContext ctx)
    {
        characterMenuUIController.ToggleStatsPanel();
    }

    void OnMapButtonPressed(InputAction.CallbackContext ctx)
    {
        mapController.ToggleMap();
    }

    void OnPauseButtonPressed(InputAction.CallbackContext ctx)
    {
        HandlePauseButtonPressed();
    }

    void OnPlayerControlsInitialised(PlayerControls controls)
    {
        this.controls = controls;
        this.controls.UIControls.Inventory.performed += OnInventoryButtonPressed;
        this.controls.UIControls.Skills.performed += OnSkillsButtonPressed;
        this.controls.UIControls.Stats.performed += OnStatsButtonPressed;
        this.controls.UIControls.Map.performed += OnMapButtonPressed;
        this.controls.UIControls.Pause.performed += OnPauseButtonPressed;
    }

    void OnPlayerInitialised(PlayerController playerInitialised)
    {
        initialisedPlayer = playerInitialised;

        characterMenuUIController.InitMenus(playerInitialised, controls);
        crosshairController.Init(controls);
        mapController.Init(controls);

        StartCoroutine(FadeInScreen());
    }

    void OnPlayerDeath()
    {
        ShowGameOverScreen();
    }

    void OnLevelTransitionEntered(int levelIndex, Vector2Int playerMoveToCoords)
    {
        if (transitionLevelCoroutine != null)
        {
            StopCoroutine(transitionLevelCoroutine);
            transitionLevelCoroutine = null;
        }


        transitionLevelCoroutine = StartCoroutine(TransitionLevel(levelIndex, playerMoveToCoords));
    }

    IEnumerator TransitionLevel(int levelIndex, Vector2Int playerMoveToCoords)
    {
        isTransitioningLevel = true;
        mapController.CloseMap();
        yield return FadeOutScreen();
        GridController.Instance.BeginLevelTransition(levelIndex, playerMoveToCoords);
        yield return FadeInScreen();
        ShowLevelName(levelIndex);
        isTransitioningLevel = false;
        transitionLevelCoroutine = null;
    }

    void OnQuickSave()
    {
        saveStatusText.color = new Color(1, 0.9529412f, 0, 1);
        saveStatusText.DOFade(0, saveStatusTextFadeDuration).SetDelay(3);
    }

    void OnSecretDiscovered(int secretExperienceValue)
    {
        ShowSecretDiscoveryUI(secretExperienceValue);
    }

    #endregion
    // ==========================

    public PlayerControls GetControls() => controls;


    // ==========================
    #region Secret Discovery

    void ShowSecretDiscoveryUI(int secretExperienceValue)
    {
        secretDiscoveryUI.ShowUI(secretExperienceValue);
    }

    #endregion
    // ==========================

    public void HandlePauseButtonPressed()
    {
        if (deleteSaveConfirmPopup.activeSelf)
        {
            CloseDeleteSaveConfirmation();
            return;
        }

        if (loadGameConfrimPopup.activeSelf)
        {
            CloseLoadGameConfirmation();
            return;
        }

        if (overwriteSaveConfrimPopup.activeSelf)
        {
            CloseSaveOverwriteConfirmation();
            return;
        }

        if (isInputtingName)
        {
            HideSaveNamePopup();
            return;
        }

        if (saveMenu.activeSelf)
        {
            CloseSaveMenu();
            return;
        }

        if (loadMenu.activeSelf)
        {
            CloseLoadMenu();
            return;
        }

        pauseMenu.TogglePauseMenu();
    }

    #region Level Transition
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

    IEnumerator FadeInScreen()
    {
        levelTransitionFadeOverlay.DOFade(0, fadeInDuration);
        yield return new WaitForSeconds(fadeInDuration);
    }

    IEnumerator FadeOutScreen()
    {
        levelTransitionFadeOverlay.DOFade(1, fadeOutDuration);
        yield return new WaitForSeconds(fadeOutDuration);
    }

    IEnumerator LevelNameLifetime()
    {
        yield return new WaitForSeconds(levelTextLifetimeDuration);
        HideLevelName();
    }

    #endregion

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
        foreach (UnityEngine.UI.Button button in loadGameButtons)
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
