using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Stat
{
    public ModifiableStats stat;
    public float baseStatValue;
    [SerializeField] float currentStatValue;

    public static Action<Stat> onStatUpdated;

    public void InitStat()
    {
        currentStatValue = baseStatValue;
    }

    public void UpdateStat(float newValue)
    {
        currentStatValue = newValue;
        onStatUpdated?.Invoke(this);
    }

    public float GetCurrentStatValue()
    {
        return currentStatValue;
    }
}

public class PlayerStatsManager : MonoBehaviour
{
    [SerializeField] CharacterData playerCharData;
    public List<Stat> playerStats = new List<Stat>();

    private void OnEnable()
    {
        PlayerEquipmentManager.onEquippedItemAdded += OnEquippedItemAdded;
        PlayerEquipmentManager.onEquippedItemRemoved += OnEquippedItemRemoved;

        PlayerSkillsController.onSkillUpdated += OnSkillUpdated;
    }

    private void OnDisable()
    {
        PlayerEquipmentManager.onEquippedItemAdded -= OnEquippedItemAdded;
        PlayerEquipmentManager.onEquippedItemRemoved -= OnEquippedItemRemoved;

        PlayerSkillsController.onSkillUpdated -= OnSkillUpdated;
    }

    public void Init(CharacterData newPlayerCharData)
    {
        playerCharData = newPlayerCharData;
        playerStats = playerCharData.baseCharStats;

        foreach (Stat stat in playerStats)
        {
            stat.InitStat();
        }
    }

    void OnEquippedItemAdded(EquippedItem newlyAddedItem)
    {
        if(newlyAddedItem.equipmentItemData.statModifiers.Count > 0)
        {
            foreach (StatModifier statModifier in newlyAddedItem.equipmentItemData.statModifiers)
            {
                ApplyStatModifier(statModifier);
            }
        }
    }

    void OnEquippedItemRemoved(EquippedItem newlyRemovedItem)
    {
        if (newlyRemovedItem.equipmentItemData.statModifiers.Count > 0)
        {
            foreach (StatModifier statModifier in newlyRemovedItem.equipmentItemData.statModifiers)
            {
                RemoveStatModifier(statModifier);
            }
        }
    }

    void OnSkillUpdated(PlayerSkill unlockedSkill)
    {
        foreach (StatModifier statModifier in unlockedSkill.skillData.statModifiers)
        {
            ApplyStatModifier(statModifier, unlockedSkill.currentSkillLevel);
        }
    }

    void ApplyStatModifier(StatModifier statModifier, int levelMultiplier = 1)
    {
        Stat stat = GetPlayerStat(statModifier.statToModify);
        switch (statModifier.modifyOperation)
        {
            case ModifyOperation.Increase:
                stat.UpdateStat(stat.GetCurrentStatValue() + statModifier.modifyAmount * levelMultiplier);
                break;
            case ModifyOperation.Decrease:
                stat.UpdateStat(stat.GetCurrentStatValue() - statModifier.modifyAmount * levelMultiplier);
                break;
            case ModifyOperation.IncreaseByPercentage:
                stat.UpdateStat(stat.GetCurrentStatValue() + stat.baseStatValue * ((statModifier.modifyAmount * levelMultiplier) / 100));
                break;
            case ModifyOperation.DecreaseByPercentage:
                stat.UpdateStat(stat.GetCurrentStatValue() - stat.baseStatValue * ((statModifier.modifyAmount * levelMultiplier) / 100));
                break;
        }
    }

    //Same as apply just reversed
    void RemoveStatModifier(StatModifier statModifier)
    {
        Stat stat = GetPlayerStat(statModifier.statToModify);
        switch (statModifier.modifyOperation)
        {
            case ModifyOperation.Increase:
                stat.UpdateStat(stat.GetCurrentStatValue() - statModifier.modifyAmount);
                break;
            case ModifyOperation.Decrease:
                stat.UpdateStat(stat.GetCurrentStatValue() + statModifier.modifyAmount);
                break;
            case ModifyOperation.IncreaseByPercentage:
                stat.UpdateStat(stat.GetCurrentStatValue() - stat.baseStatValue * (statModifier.modifyAmount / 100));
                break;
            case ModifyOperation.DecreaseByPercentage:
                stat.UpdateStat(stat.GetCurrentStatValue() + stat.baseStatValue * (statModifier.modifyAmount / 100));
                break;
        }
    }

    Stat GetPlayerStat(ModifiableStats statToGet)
    {
        Stat statToReturn = null;
        foreach (Stat stat in playerStats)
        {
            if (stat.stat == statToGet)
                statToReturn = stat;
        }
        return statToReturn;
    }
}
