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
    PlayerInventoryUIManager playerInventoryUIManager;
    PlayerSkillsUIManager playerSkillsUIManager;
    PlayerEquipmentUIManager PlayerEquipmentUIManager;
    PlayerWeaponUIManager playerWeaponUIManager;

    InventoryPanel currentOpenInventoryPanel = InventoryPanel.Inventory;
    WeaponItemData defaultWeaponData;
    WeaponItem defaultWeaponItem;
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
        RangedWeapon.onNewAmmoTypeLoaded += OnNewAmmoTypeLoaded;

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
        RangedWeapon.onNewAmmoTypeLoaded -= OnNewAmmoTypeLoaded;

        PlayerWeaponManager.onNewWeaponInitialised -= OnNewWeaponInitialised;
        PlayerWeaponManager.onWeaponSlotSetActive -= OnWeaponSlotSetActive;
    }

    private void Awake()
    {
        playerStatsUIController = GetComponent<PlayerStatsUIManager>();
        playerInventoryUIManager = GetComponent<PlayerInventoryUIManager>();
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

    void OnNewWeaponInitialised(int slotIndex, WeaponItem newWeapon)
    {
        playerWeaponUIManager.UpdateWeaponDisplayImages(slotIndex, newWeapon);
    }

    void OnWeaponSwappedInSlot(int slotIndex, WeaponItem weaponToSwapTo)
    {
        playerWeaponUIManager.UpdateWeaponDisplayImages(slotIndex, weaponToSwapTo);
    }

    void OnWeaponRemovedFromSlot(int slotIndex)
    {
        playerWeaponUIManager.UpdateWeaponDisplayImages(slotIndex, defaultWeaponItem);
    }

    void OnWeaponSetToDefault(int slotIndex, WeaponItem defaultWeapon)
    {
        defaultWeaponItem = defaultWeapon;
        defaultWeaponData = defaultWeapon.WeaponItemData;
        playerWeaponUIManager.UpdateWeaponDisplayImages(slotIndex, defaultWeapon);
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
        playerWeaponUIManager.SetSlotActive(activeSlot.GetSlotIndex());
    }

    void OnNewAmmoTypeLoaded(int slotIndex, WeaponItem weaponLoaded)
    {
        playerWeaponUIManager.UpdateWeaponDisplayImages(slotIndex, weaponLoaded);
    }

    public void InitMenus(PlayerController player, PlayerControls controls)
    {
        playerStatsUIController.InitStatsUI(player);
        playerInventoryUIManager.Init(controls);
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
                playerInventoryUIManager.OpenInventory();
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
        playerInventoryUIManager.CloseInventory();
        playerSkillsUIManager.CloseSkillsMenu();
        playerStatsUIController.CloseStatsMenu();
    }
}
