using System;
using UnityEngine;

public class NPCSpawnPoint : MonoBehaviour
{
    [SerializeField] NPCController NPCToSpawn;
    public NPCController spawnedNPC;

    public static Action<PlayerController> onPlayerSpawned;

    //public NPCController SpawnNPC(NPCData npcData, GridNode spawnGridNode)
    //{
    //    spawnedNPC = Instantiate(NPCToSpawn, transform.position, transform.rotation);
    //    spawnedNPC.InitNPC(npcData, spawnGridNode);

    //    spawnGridNode.SetOccupant(new GridNodeOccupant(spawnedNPC.gameObject, GridNodeOccupantType.NPC));

    //    return spawnedNPC;
    //}

    public void DespawnNPC()
    {
        if (!spawnedNPC)
            return;

        //save transform and stats
        Destroy(spawnedNPC.gameObject);

    }
}
