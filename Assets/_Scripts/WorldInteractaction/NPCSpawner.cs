using System;
using System.Collections.Generic;
using UnityEngine;

enum NPC_Type
{
    Zombie,
    Ranger,
    Bug
}

public class NPCSpawner : TriggerableBase
{
    NPCController npcToSpawn;
    NPCController SpawnedNpc;
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
        SpawnedNpc.SetActive(true);
    }

    public void SpawnNPC(string npcTypeToSpawn)
    {
        SpawnedNpc = Instantiate(DecideNPCToSpawn(npcTypeToSpawn), occupyingGridNode.transform.position + new Vector3(-1.5f, 0, -1.5f), Quaternion.identity);
        SpawnedNpc.InitNPC(GetLevelIndex(), occupyingGridNode);
        spawnedNPCsListRef.Add(SpawnedNpc);
        SpawnedNpc.SetActive(false);
    }
    NPCController DecideNPCToSpawn(string npcType)
    {
        if (Enum.TryParse(npcType, out NPC_Type type))
        {
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
        }
        return npcToSpawn;
    }
}
