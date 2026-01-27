using System;
using UnityEngine;

enum NPC_Type
{
    Zombie,
    Ranger,
    Bug
}

public class NPCSpawner : TriggerableBase
{
    NPC_Type enemyToSpawn;

    public override void LoadData(SaveableLevelData.TriggerableSaveData data)
    {
        throw new System.NotImplementedException();
    }

    public override void SetIsTriggered(bool isTriggered)
    {
        Debug.Log("Set is triggerd");
    }

    public override void Trigger(IInteractable triggeredInteractable)
    {
        Debug.Log($"Spawning {enemyToSpawn}...");
    }

    public void SetNPCToSpawn(string npcToSpawn)
    {
        if (Enum.TryParse(npcToSpawn, out NPC_Type type))
        {
            enemyToSpawn = type;
        }
    }
}
