using UnityEngine;

public class CharacterMenuUIController : MonoBehaviour
{
    public enum InventoryPanel
    {
        Inventory,
        Skills,
        Stats
    }

    InventoryPanel currentOpenInventoryPanel = InventoryPanel.Inventory;

    UIController uiController;

    [SerializeField] GameObject characterMenuPanelsParent;

    public static bool isCharacterMenuOpen = false;

    private void OnEnable()
    {
        PlayerInventoryManager.onInventoryOpened += OpenCharacterMenu;
        PlayerInventoryManager.onInventoryClosed += CloseCharacterMenu;

        Container.onContainerClosed += OnContainerClosed;
    }

    private void OnDisable()
    {
        PlayerInventoryManager.onInventoryOpened -= OpenCharacterMenu;
        PlayerInventoryManager.onInventoryClosed -= CloseCharacterMenu;

        Container.onContainerClosed -= OnContainerClosed;
    }

    void OnContainerClosed()
    {
        CloseCharacterMenu();
    }

    private void Awake()
    {
        uiController = GetComponentInParent<UIController>();
    }

    private void Start()
    {
        CloseCharacterMenu();
    }

    public void ToggleCharacterMenu()
    {
        if (PauseMenu.isPaused || ThrowableSelectionManager.isThrowableSelectionMenuOpen) return;

        if (!isCharacterMenuOpen)
            OpenCharacterMenu();
        else
            CloseCharacterMenu();
    }

    void OpenCharacterMenu()
    {
        isCharacterMenuOpen = true;
        ShowCurrentOpenPanel();
    }

    void CloseCharacterMenu()
    {
        isCharacterMenuOpen = false;
        SetPanelsInactive();
        characterMenuPanelsParent.SetActive(false);
        if(PlayerInventoryManager.isInContainer)
            WorldInteractionManager.CloseCurrentOpenContainer();
        //HelperFunctions.SetCursorActive(false);
        CrosshairController.SetCrosshairLocked(true);
    }

    void ShowCurrentOpenPanel()
    {
        SetPanelsInactive();
        SetPanelActive(currentOpenInventoryPanel);
    }

    public void ToggleInventoryPanel()
    {
        if (PauseMenu.isPaused || ThrowableSelectionManager.isThrowableSelectionMenuOpen) return;

        if (isCharacterMenuOpen && currentOpenInventoryPanel == InventoryPanel.Inventory)
            CloseCharacterMenu();
        else
            ShowInventoryMenu();
    }

    public void ShowInventoryMenu()
    {
        SetPanelsInactive();
        SetPanelActive(InventoryPanel.Inventory);
    }

    public void ToggleSkillsPanel()
    {
        if (PauseMenu.isPaused || ThrowableSelectionManager.isThrowableSelectionMenuOpen) return;

        if (isCharacterMenuOpen && currentOpenInventoryPanel == InventoryPanel.Skills)
            CloseCharacterMenu();
        else
            ShowSkillsMenu();
    }

    public void ShowSkillsMenu()
    {
        SetPanelsInactive();
        SetPanelActive(InventoryPanel.Skills);
    }

    public void ToggleStatsPanel()
    {
        if (PauseMenu.isPaused || ThrowableSelectionManager.isThrowableSelectionMenuOpen) return;

        if (isCharacterMenuOpen && currentOpenInventoryPanel == InventoryPanel.Stats)
            CloseCharacterMenu();
        else
            ShowStatsMenu();
    }

    public void ShowStatsMenu()
    {
        SetPanelsInactive();
        SetPanelActive(InventoryPanel.Stats);
    }

    void SetPanelActive(InventoryPanel panelToSetActive)
    {
        characterMenuPanelsParent.SetActive(true);
        isCharacterMenuOpen = true;

        switch (panelToSetActive)
        {
            case InventoryPanel.Inventory:
                uiController.playerInventoryUIController.OpenInventory();
                break;
            case InventoryPanel.Skills:
                uiController.playerSkillsUIManager.OpenSkillsMenu();
                break;
            case InventoryPanel.Stats:
                uiController.playerStatsUIController.OpenStatsMenu();
                break;
        }
        currentOpenInventoryPanel = panelToSetActive;
    }

    void SetPanelsInactive()
    {
        uiController.playerInventoryUIController.CloseInventory();
        uiController.playerSkillsUIManager.CloseSkillsMenu();
        uiController.playerStatsUIController.CloseStatsMenu();
    }
}
