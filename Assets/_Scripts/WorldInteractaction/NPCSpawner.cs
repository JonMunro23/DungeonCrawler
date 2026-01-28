using System;
using System.Collections.Generic;
using UnityEngine;



public class NPCSpawner : TriggerableBase
{
    enum NPC_Type
    {
        Zombie,
        Ranger,
        Bug
    }

    NPCController npcToSpawn;
    NPCController spawnedNpc;

    NPCMovementBehaviour spawnBehaviour;

    [Header("Enemy Types")]
    [SerializeField] NPCController zombie;
    [SerializeField] NPCController ranger;
    [SerializeField] NPCController bug;

    List<NPCController> spawnedNPCsListRef;

    public override void LoadData(SaveableLevelData.TriggerableSaveData data)
    {
        throw new System.NotImplementedException();
    }

    public void AssignSpawnedNPCsList(ref List<NPCController> spawnedNPCs)
    {
        spawnedNPCsListRef = spawnedNPCs;
    }

    public override void Trigger(IInteractable triggeredInteractable)
    {
        spawnedNpc.SetActive(true);
    }

    public void SpawnNPC(string npcTypeToSpawn)
    {
        spawnedNpc = Instantiate(DecideNPCToSpawn(npcTypeToSpawn), occupyingGridNode.transform.position + new Vector3(-1.5f, 0, -1.5f), Quaternion.identity);
        spawnedNpc.InitNPC(GetLevelIndex(), occupyingGridNode);
        spawnedNpc.SetMovementBehaviour(spawnBehaviour);
        spawnedNPCsListRef.Add(spawnedNpc);
        spawnedNpc.SetActive(false);
    }

    public void SetSpawnBehaviour(string spawnBehaviour)
    {
        this.spawnBehaviour = HelperFunctions.ToEnum<NPCMovementBehaviour>(spawnBehaviour); ;
    }

    NPCController DecideNPCToSpawn(string npcType)
    {
        NPC_Type type = HelperFunctions.ToEnum<NPC_Type>(npcType);
        NPCController npcToSpawn = null;
        switch (type)
        {
            case NPC_Type.Zombie:
                npcToSpawn = zombie;
                break;
            case NPC_Type.Ranger:
                npcToSpawn = ranger;
                break;
            case NPC_Type.Bug:
                npcToSpawn = bug;
                break;
        }
        return npcToSpawn;
    }
}
