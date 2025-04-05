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
        if(!isCharacterMenuOpen)
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

        if (!PlayerInventoryManager.isInContainer && !WorldInteractionManager.hasGrabbedItem && !MainMenu.isInMainMenu)
            HelperFunctions.SetCursorActive(false);
    }

    void ShowCurrentOpenPanel()
    {
        SetPanelsInactive();
        SetPanelActive(currentOpenInventoryPanel);
    }

    public void ToggleInventoryPanel()
    {
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
        if(isCharacterMenuOpen && currentOpenInventoryPanel == InventoryPanel.Stats)
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
