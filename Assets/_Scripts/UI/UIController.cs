using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] HandUIController handUIController;
    [SerializeField] PlayerStatsUIController playerStatsUIController;
    [SerializeField] PlayerInventoryUIController playerInventoryUIController;
    [SerializeField] PlayerEquipmentUIManager PlayerEquipmentUIManager;

    PlayerController initialisedPlayer;

    private void OnEnable()
    {
        PlayerController.onPlayerInitialised += OnPlayerInitialised;
    }

    private void OnDisable()
    {
        PlayerController.onPlayerInitialised -= OnPlayerInitialised;
    }

    void OnPlayerInitialised(PlayerController playerInitialised)
    {
        initialisedPlayer = playerInitialised;

        handUIController.InitHands();
        playerStatsUIController.InitStatsUI(initialisedPlayer.playerCharacterData);
        playerInventoryUIController.InitPlayerInventory();
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
