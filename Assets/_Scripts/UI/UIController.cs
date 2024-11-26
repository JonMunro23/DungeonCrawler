using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] PlayerStatsUIController playerStatsUIController;
    [SerializeField] PlayerInventoryUIController playerInventoryUIController;
    [SerializeField] PlayerEquipmentUIManager PlayerEquipmentUIManager;
    [SerializeField] PlayerWeaponUIManager playerWeaponUIManager;

    PlayerController initialisedPlayer;

    private void OnEnable()
    {
        PlayerController.onPlayerInitialised += OnPlayerInitialised;

        ItemPickupManager.onNewItemAttachedToCursor += OnNewItemAttachedToCursor;
        ItemPickupManager.onCurrentItemDettachedFromCursor += OnCurrentItemRemovedFromCursor;

        WeaponSlot.onWeaponAddedToSlot += OnWeaponAddedToSlot;
        WeaponSlot.onWeaponRemovedFromSlot += OnWeaponRemovedFromSlot;
        WeaponSlot.onWeaponSwappedInSlot += OnWeaponSwappedInSlot;
        WeaponSlot.onWeaponSetToDefault += OnWeaponSetToDefault;

        Weapon.onAmmoUpdated += OnWeaponAmmoUpdated;

        PlayerWeaponManager.onWeaponSlotSetActive += OnWeaponSlotSetActive;

    }

    private void OnDisable()
    {
        PlayerController.onPlayerInitialised -= OnPlayerInitialised;

        ItemPickupManager.onNewItemAttachedToCursor -= OnNewItemAttachedToCursor;
        ItemPickupManager.onCurrentItemDettachedFromCursor -= OnCurrentItemRemovedFromCursor;

        WeaponSlot.onWeaponAddedToSlot -= OnWeaponAddedToSlot;
        WeaponSlot.onWeaponRemovedFromSlot -= OnWeaponRemovedFromSlot;
        WeaponSlot.onWeaponSwappedInSlot -= OnWeaponSwappedInSlot;
        WeaponSlot.onWeaponSetToDefault -= OnWeaponSetToDefault;

        Weapon.onAmmoUpdated -= OnWeaponAmmoUpdated;

        PlayerWeaponManager.onWeaponSlotSetActive -= OnWeaponSlotSetActive;

    }

    void OnPlayerInitialised(PlayerController playerInitialised)
    {
        initialisedPlayer = playerInitialised;

        playerStatsUIController.InitStatsUI(initialisedPlayer.playerCharacterData);
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

    void OnWeaponAddedToSlot(int slotIndex, WeaponItemData newItemData, int loadedAmmo)
    {
        playerWeaponUIManager.UpdateWeaponDisplayImages(slotIndex, newItemData);
    }

    void OnWeaponSwappedInSlot(int slotIndex, WeaponItemData dataToSwapTo, int loadedAmmo)
    {
        playerWeaponUIManager.UpdateWeaponDisplayImages(slotIndex, dataToSwapTo);
    }

    void OnWeaponRemovedFromSlot(int slotIndex)
    {
        playerWeaponUIManager.UpdateWeaponDisplayImages(slotIndex, null);
    }

    void OnWeaponSetToDefault(int slotIndex, WeaponItemData defaultWeaponData)
    {
        playerWeaponUIManager.UpdateWeaponDisplayImages(slotIndex, defaultWeaponData);
    }

    void OnWeaponAmmoUpdated(int slotIndex, int loaded, int reserve)
    {
        playerWeaponUIManager.UpdateWeaponDisplayAmmoCount(slotIndex, loaded, reserve);
    }

    void OnWeaponSlotSetActive(int slotIndex)
    {
        playerWeaponUIManager.SetSlotActive(slotIndex);
    }
}
