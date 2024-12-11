using System;
using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    [SerializeField] PlayerController playerPrefab;
    public PlayerController spawnedPlayer;

    [SerializeField] Vector3 playerSpawnOffset;

    public static Action<PlayerController> onPlayerSpawned;

    public PlayerController SpawnPlayer(CharacterData playerCharData, GridNode spawnGridNode)
    {
        spawnedPlayer = Instantiate(playerPrefab, transform.position + playerSpawnOffset, transform.rotation);
        //spawnedPlayer.InitPlayer(playerCharData, spawnGridNode);

        spawnGridNode.SetOccupant(new GridNodeOccupant(spawnedPlayer.gameObject, GridNodeOccupantType.Player));
        //onPlayerSpawned?.Invoke(spawnedPlayer);

        return spawnedPlayer;
    }
}
