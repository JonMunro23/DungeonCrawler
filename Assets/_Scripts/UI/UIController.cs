using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] HandUIController handUIController;
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
    }

    private void OnDisable()
    {
        PlayerController.onPlayerInitialised -= OnPlayerInitialised;
        ItemPickupManager.onNewItemAttachedToCursor -= OnNewItemAttachedToCursor;
        ItemPickupManager.onCurrentItemDettachedFromCursor -= OnCurrentItemRemovedFromCursor;
    }

    void OnPlayerInitialised(PlayerController playerInitialised)
    {
        initialisedPlayer = playerInitialised;

        handUIController.InitHands();
        playerStatsUIController.InitStatsUI(initialisedPlayer.playerCharacterData);
        playerInventoryUIController.InitPlayerInventory();
    }

    void OnNewItemAttachedToCursor(ItemStack item)
    {
        WeaponItemData handItemData = item.itemData as WeaponItemData;
        if (handItemData != null)
        {
            PlayerEquipmentUIManager.DisableEquipmentSlots();
            return;
        }

        EquipmentItemData equipItemData = item.itemData as EquipmentItemData;
        if (equipItemData != null)
        {
            PlayerEquipmentUIManager.DisableSlotsNotOfType(equipItemData.EquipmentSlotType);
            playerWeaponUIManager.DisableSlots();
        }
    }

    void OnCurrentItemRemovedFromCursor()
    {
        PlayerEquipmentUIManager.RenableSlots();
        playerWeaponUIManager.RenableSlots();
    }


    void UpdateWeaponSlotUI(EquipmentSlotType slotToUpdate, WeaponItemData newSlotData)
    {
        handUIController.UpdateWeaponSlot(slotToUpdate, newSlotData);
    }

    void OnWeaponCooldownBegins()
    {

    }

    void OnWeaponCooldownEnds()
    {

    }
}
