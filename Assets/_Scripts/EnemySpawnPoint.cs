using System;
using UnityEngine;

public class EnemySpawnPoint : MonoBehaviour
{
    [SerializeField] NPCGroupController enemyToSpawn;
    public NPCGroupController spawnedEnemies;

    public static Action<PlayerController> onPlayerSpawned;

    public void SpawnEnemy(GridNode spawnGridNode)
    {
        spawnedEnemies = Instantiate(enemyToSpawn, transform.position, transform.rotation);
        spawnedEnemies.InitGroup(spawnGridNode);

        spawnGridNode.SetOccupant(GridNodeOccupant.Enemy);
    }
}
