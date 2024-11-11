using System;
using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    [SerializeField] PlayerController playerPrefab;
    public PlayerController spawnedPlayer;

    public static Action<PlayerController> onPlayerSpawned;

    public void SpawnPlayer(CharacterData playerCharData, GridNode spawnGridNode)
    {
        spawnedPlayer = Instantiate(playerPrefab, transform.position, transform.rotation);
        spawnedPlayer.InitPlayer(playerCharData, spawnGridNode);

        spawnGridNode.SetOccupant(new GridNodeOccupant(spawnedPlayer.gameObject, GridNodeOccupantType.Player));
        //onPlayerSpawned?.Invoke(spawnedPlayer);
    }
}
