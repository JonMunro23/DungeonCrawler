using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] HealthController playerHealthController;
    [SerializeField] PlayerInventory playerInventory;

    [HideInInspector] public CharacterData playerCharacterData { get; private set; }

    public static Action<PlayerController> onPlayerInitialised;

    public void InitPlayer(CharacterData playerCharData)
    {
        playerCharacterData = playerCharData;
        playerHealthController.InitHealthController(playerCharacterData);



        onPlayerInitialised?.Invoke(this);
    }
}
