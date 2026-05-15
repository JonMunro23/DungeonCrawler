using UnityEngine;

public class CharacterMenuUIController : MonoBehaviour
{
    public enum InventoryPanel
    {
        Inventory,
        Skills,
        Stats
    }

    [Header("References")]
    [SerializeField] GameObject characterMenuPanelsParent;
    PlayerStatsUIManager playerStatsUIController;
    PlayerInventoryUIManager playerInventoryUIController;
    PlayerSkillsUIManager playerSkillsUIManager;
    PlayerEquipmentUIManager PlayerEquipmentUIManager;
    PlayerWeaponUIManager playerWeaponUIManager;

    InventoryPanel currentOpenInventoryPanel = InventoryPanel.Inventory;
    WeaponItemData defaultWeaponData;
    public static bool isCharacterMenuOpen = false;

    private void OnEnable()
    {
        PlayerInventoryManager.onInventoryOpened += OpenCharacterMenu;
        PlayerInventoryManager.onInventoryClosed += CloseCharacterMenu;

        Container.onContainerClosed += OnContainerClosed;

        WorldInteractionManager.onNewItemAttachedToCursor += OnNewItemAttachedToCursor;
        WorldInteractionManager.onCurrentItemDettachedFromCursor += OnCurrentItemRemovedFromCursor;

        WeaponSlot.onWeaponRemovedFromSlot += OnWeaponRemovedFromSlot;
        WeaponSlot.onWeaponSwappedInSlot += OnWeaponSwappedInSlot;
        WeaponSlot.onWeaponSetToDefault += OnWeaponSetToDefault;

        RangedWeapon.onLoadedAmmoUpdated += OnWeaponLoadedAmmoUpdated;
        RangedWeapon.onReserveAmmoUpdated += OnWeaponReserveAmmoUpdated;

        PlayerWeaponManager.onWeaponSlotSetActive += OnWeaponSlotSetActive;
        PlayerWeaponManager.onNewWeaponInitialised += OnNewWeaponInitialised;
    }

    private void OnDisable()
    {
        PlayerInventoryManager.onInventoryOpened -= OpenCharacterMenu;
        PlayerInventoryManager.onInventoryClosed -= CloseCharacterMenu;

        Container.onContainerClosed -= OnContainerClosed;

        WorldInteractionManager.onNewItemAttachedToCursor -= OnNewItemAttachedToCursor;
        WorldInteractionManager.onCurrentItemDettachedFromCursor -= OnCurrentItemRemovedFromCursor;

        WeaponSlot.onWeaponRemovedFromSlot -= OnWeaponRemovedFromSlot;
        WeaponSlot.onWeaponSwappedInSlot -= OnWeaponSwappedInSlot;
        WeaponSlot.onWeaponSetToDefault -= OnWeaponSetToDefault;

        RangedWeapon.onLoadedAmmoUpdated -= OnWeaponLoadedAmmoUpdated;
        RangedWeapon.onReserveAmmoUpdated -= OnWeaponReserveAmmoUpdated;

        PlayerWeaponManager.onNewWeaponInitialised -= OnNewWeaponInitialised;
        PlayerWeaponManager.onWeaponSlotSetActive -= OnWeaponSlotSetActive;
    }

    private void Awake()
    {
        playerStatsUIController = GetComponent<PlayerStatsUIManager>();
        playerInventoryUIController = GetComponent<PlayerInventoryUIManager>();
        playerSkillsUIManager = GetComponent<PlayerSkillsUIManager>();
        PlayerEquipmentUIManager = GetComponent<PlayerEquipmentUIManager>();
        playerWeaponUIManager = GetComponent<PlayerWeaponUIManager>();
    }

    private void Start()
    {
        CloseCharacterMenu();
    }

    void OnContainerClosed()
    {
        CloseCharacterMenu();
    }

    void OnNewItemAttachedToCursor(ItemStack item)
    {
        WeaponItemData handItemData = item.Item.ItemData as WeaponItemData;
        if (handItemData != null)
        {
            PlayerEquipmentUIManager.DisableAllSlots();
            return;
        }

        EquipmentItemData equipItemData = item.Item.ItemData as EquipmentItemData;
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

    void OnWeaponSlotSetActive(WeaponSlot activeSlot)
    {
        playerWeaponUIManager.SetSlotActive(activeSlot.slotIndex);
    }


    public void InitMenus(PlayerController player)
    {
        playerStatsUIController.InitStatsUI(player);
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
        Debug.Log("Closing menu...");
        isCharacterMenuOpen = false;
        SetPanelsInactive();
        characterMenuPanelsParent.SetActive(false);
        if(PlayerInventoryManager.isInContainer)
            WorldInteractionManager.CloseCurrentOpenContainer();
        //HelperFunctions.SetCursorActive(false);
        if(!WorldInteractionManager.hasGrabbedItem)
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

    void ShowInventoryMenu()
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

    void ShowSkillsMenu()
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

    void ShowStatsMenu()
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
                playerInventoryUIController.OpenInventory();
                break;
            case InventoryPanel.Skills:
                playerSkillsUIManager.OpenSkillsMenu();
                break;
            case InventoryPanel.Stats:
                playerStatsUIController.OpenStatsMenu();
                break;
        }
        currentOpenInventoryPanel = panelToSetActive;
    }

    void SetPanelsInactive()
    {
        playerInventoryUIController.CloseInventory();
        playerSkillsUIManager.CloseSkillsMenu();
        playerStatsUIController.CloseStatsMenu();
    }
}
