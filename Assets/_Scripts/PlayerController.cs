using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] HealthController playerHealthController;
    [SerializeField] PlayerInventory playerInventory;
    [SerializeField] PlayerStatsManager playerStatsManager;
    [HideInInspector] public CharacterData playerCharacterData { get; private set; }
    public static Action<PlayerController> onPlayerInitialised;

    [Header("Grid Data")]
    public static GridNode currentOccupiedNode;

    private void OnEnable()
    {
        AdvancedGridMovement.onPlayerMoved += OnMoved;
    }

    private void OnDisable()
    {
        AdvancedGridMovement.onPlayerMoved -= OnMoved;
    }

    public void InitPlayer(CharacterData playerCharData, GridNode spawnGridNode)
    {
        playerCharacterData = playerCharData;
        currentOccupiedNode = spawnGridNode;

        playerStatsManager.InitPlayerStats(playerCharacterData);
        playerHealthController.InitHealthController(playerCharacterData);

        onPlayerInitialised?.Invoke(this);
    }

    void OnMoved()
    {
        currentOccupiedNode.SetOccupied(false);
        currentOccupiedNode = GridController.Instance.GetNodeFromWorldPos(transform.position);
        currentOccupiedNode.SetOccupied(true);
    }
}
