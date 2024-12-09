using System;
using UnityEngine;

public class PlayerLevelController : MonoBehaviour
{
    public int currentPlayerLevel = 0;
    [SerializeField] int currentExperiencePoints; 
    [SerializeField] int requiredExperiencePoints;

    public static Action<int> onPlayerExperienceUpdated;
    public static Action<int> onPlayerRequiredExperienceUpdated;
    public static Action<int> onPlayerLevelUp;


    private void OnEnable()
    {
        NPCController.onNPCDeath += OnNPCDeath;
    }

    private void OnDisable()
    {
        NPCController.onNPCDeath -= OnNPCDeath;
    }

    private void Start()
    {
        onPlayerRequiredExperienceUpdated?.Invoke(requiredExperiencePoints);
    }

    void OnNPCDeath(NPCController npcKilled)
    {
        AddExperiencePoints(npcKilled.NPCData.experienceValue);
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

    void LevelUp()
    {
        currentPlayerLevel++;
        requiredExperiencePoints = currentExperiencePoints * 2;

        onPlayerRequiredExperienceUpdated?.Invoke(requiredExperiencePoints);
        onPlayerLevelUp?.Invoke(currentPlayerLevel);
    }
}
