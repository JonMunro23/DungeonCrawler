using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLevelManager : MonoBehaviour
{
    public int currentPlayerLevel = 0;
    [SerializeField] int currentExperiencePoints; 
    [SerializeField] int requiredExperiencePoints;

    public static event Action<int> onPlayerExperienceUpdated;
    public static event Action<int> onPlayerRequiredExperienceUpdated;
    public static event Action<int> onPlayerLevelUp;


    private void OnEnable()
    {
        NPCController.onNPCDeath += OnNPCDeath;
        SecretAreaTrigger.onSecretDiscovered += OnSecretDiscovered;
    }

    private void OnDisable()
    {
        NPCController.onNPCDeath -= OnNPCDeath;
        SecretAreaTrigger.onSecretDiscovered -= OnSecretDiscovered;
    }

    private void Start()
    {
        onPlayerRequiredExperienceUpdated?.Invoke(requiredExperiencePoints);
    }

    void OnNPCDeath(NPCController npcKilled)
    {
        AddExperiencePoints(npcKilled.npcData.experienceValue);
    }

    void OnSecretDiscovered(int secretExperienceValue)
    {
        AddExperiencePoints(secretExperienceValue);
    }

    public void AddExperiencePoints(int amountToAdd)
    {
        currentExperiencePoints += amountToAdd;
        onPlayerExperienceUpdated?.Invoke(currentExperiencePoints);
        if(currentExperiencePoints > requiredExperiencePoints)
        {
            LevelUp();
        }
    }

    public void SetLevel(int newLevel)
    {
        currentPlayerLevel = newLevel;
    }

    public void SetRequiredExperiencePoints(int requiredPoints)
    {
        requiredExperiencePoints = requiredPoints;
    }

    void LevelUp()
    {
        currentPlayerLevel++;
        SetRequiredExperiencePoints(currentExperiencePoints * 2);
        //play level up sound
        onPlayerRequiredExperienceUpdated?.Invoke(requiredExperiencePoints);
        onPlayerLevelUp?.Invoke(currentPlayerLevel);
    }

    public void Save(ref PlayerSaveData data)
    {
        data.currentPlayerLevel = currentPlayerLevel;
        data.currentExperiencePoints = currentExperiencePoints;
        data.requiredExperiencePoints = requiredExperiencePoints;
    }

    public void Load(PlayerSaveData data)
    {
        SetLevel(data.currentPlayerLevel);
        AddExperiencePoints(data.currentExperiencePoints);
        SetRequiredExperiencePoints(data.requiredExperiencePoints);
    }
}
