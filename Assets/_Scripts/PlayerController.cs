using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public AdvancedGridMovement advGridMovement;
    public PlayerHealthController playerHealthController;
    public PlayerEquipmentManager playerEquipmentManager;
    public PlayerInventoryManager playerInventoryManager;
    public PlayerStatsManager playerStatsManager;

    [HideInInspector] public CharacterData playerCharacterData { get; private set; }
    public static Action<PlayerController> onPlayerInitialised;

    [Header("Grid Data")]
    public static GridNode currentOccupiedNode;

    public void InitPlayer(CharacterData playerCharData, GridNode spawnGridNode)
    {
        playerCharacterData = playerCharData;
        currentOccupiedNode = spawnGridNode;

        playerInventoryManager.InitInventory(this);
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

    public void SetCurrentOccupiedNode(GridNode newGridNode)
    {
        currentOccupiedNode = newGridNode;
    }


}
