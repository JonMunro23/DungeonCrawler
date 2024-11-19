using System;
using UnityEngine;

public class NPCSpawnPoint : MonoBehaviour
{
    [SerializeField] NPCController enemyToSpawn;
    public NPCController spawnedEnemies;

    public static Action<PlayerController> onPlayerSpawned;

    public void SpawnEnemy(GridNode spawnGridNode)
    {
        spawnedEnemies = Instantiate(enemyToSpawn, transform.position, transform.rotation);
        spawnedEnemies.InitGroup(spawnGridNode);

        spawnGridNode.SetOccupant(new GridNodeOccupant(spawnedEnemies.gameObject, GridNodeOccupantType.Enemy));
    }
}
