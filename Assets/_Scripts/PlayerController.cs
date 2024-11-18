using DG.Tweening;
using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public AdvancedGridMovement advGridMovement;
    public ItemPickupManager itemPickupManager;
    public PlayerHealthController playerHealthController;
    public PlayerInventoryManager playerInventoryManager;
    public PlayerEquipmentManager playerEquipmentManager;
    public PlayerWeaponManager playerWeaponManager;
    public PlayerStatsManager playerStatsManager;
    [HideInInspector] public Camera playerCamera;

    [Header("Player Data")]
    public CharacterData playerCharacterData;
    public static GridNode currentOccupiedNode;

    
    public static Action<PlayerController> onPlayerInitialised;


    private void Awake()
    {
        advGridMovement = GetComponent<AdvancedGridMovement>();
        playerHealthController = GetComponent<PlayerHealthController>();
        playerInventoryManager = GetComponent<PlayerInventoryManager>();
        playerEquipmentManager = GetComponent<PlayerEquipmentManager>();
        playerWeaponManager = GetComponent<PlayerWeaponManager>();
        itemPickupManager = GetComponent<ItemPickupManager>();
        playerStatsManager = GetComponent<PlayerStatsManager>();
        playerCamera = GetComponentInChildren<Camera>();
    }

    public void InitPlayer(CharacterData playerCharData, GridNode spawnGridNode)
    {
        playerCharacterData = playerCharData;
        currentOccupiedNode = spawnGridNode;

        playerInventoryManager.InitInventory(this);
        playerWeaponManager.Init(this);
        playerStatsManager.InitPlayerStats(playerCharacterData);
        playerHealthController.InitHealthController(this);
        advGridMovement.InitMovement(this);

        onPlayerInitialised?.Invoke(this);
    }

    public void TryUseHealthSyringe()
    {
        if (playerHealthController.CanHeal() && playerHealthController.CanUseSyringe() && playerInventoryManager.HasHealthSyringe())
        {
            playerInventoryManager.TryUseHealthSyringe();
        }
    }

    public void TryUseCurrentWeapon()
    {
        if(!playerInventoryManager.isOpen && !itemPickupManager.hasGrabbedItem && !DialogueManager.isInDialogue)
        {
            playerWeaponManager.UseCurrentWeapon();
        }
    }

    public void TryUseCurrentWeaponSpecial()
    {
        if (!playerInventoryManager.isOpen && !itemPickupManager.hasGrabbedItem && !DialogueManager.isInDialogue)
        {
            playerWeaponManager.UseCurrentWeaponSpecial();
        }
    }

    public void TryReloadCurrentWeapon()
    {
        playerWeaponManager.ReloadCurrentWeapon();
    }


    public void SetCurrentOccupiedNode(GridNode newGridNode)
    {
        currentOccupiedNode = newGridNode;
    }

    public void ShakeScreen()
    {
        playerCamera.DOShakePosition(.35f, .5f);
    }
}
