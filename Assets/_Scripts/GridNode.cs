using UnityEngine;

public class GridNode : MonoBehaviour
{
    [HideInInspector]
    public GridNodeData nodeData;
    PlayerSpawnPoint playerSpawnPoint;

    public void InitNode(GridNodeData newNodeData)
    {
        nodeData = newNodeData;
    }

    public void CreatePlayerSpawnPoint(PlayerSpawnPoint spawnPointToCreate)
    {
        playerSpawnPoint = Instantiate(spawnPointToCreate, transform);
    }

    public void SpawnPlayer(CharacterData playerCharData)
    {
        playerSpawnPoint.SpawnPlayer(playerCharData);
    }
}
