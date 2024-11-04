using System;
using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    [SerializeField] PlayerController playerPrefab;

    public PlayerController spawnedPlayer;

    public static Action<PlayerController> onPlayerSpawned;

    public void SpawnPlayer(CharacterData playerCharData)
    {
        spawnedPlayer = Instantiate(playerPrefab, transform.position, transform.rotation);
        spawnedPlayer.InitPlayer(playerCharData);
        //onPlayerSpawned?.Invoke(spawnedPlayer);
    }
}
