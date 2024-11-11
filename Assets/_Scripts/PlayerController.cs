using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public AdvancedGridMovement advGridMovement;
    public PlayerHealthController playerHealthController;
    public PlayerInventoryManager playerInventoryManager;
    public PlayerStatsManager playerStatsManager;

    [HideInInspector] public CharacterData playerCharacterData { get; private set; }
    public static Action<PlayerController> onPlayerInitialised;

    [Header("Grid Data")]
    public static GridNode currentOccupiedNode;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.T))
        {
            if(playerHealthController.CanHeal() && playerHealthController.CanUseSyringe() && playerInventoryManager.HasHealthSyringe())
            {
                playerInventoryManager.UseHealthSyringe();
            }
        }
    }

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

    public static void SetCurrentOccupiedNode(GridNode newGridNode)
    {
        currentOccupiedNode = newGridNode;
    }
}
