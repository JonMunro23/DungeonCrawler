using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StatData
{
    public ModifiableCharacterStats stat;
    [SerializeField] float baseStatValue;
    [SerializeField] float currentStatValue;

    public static Action<StatData> onStatUpdated;

    public StatData(ModifiableCharacterStats stat, float baseStatValue, float currentStatValue)
    {
        this.stat = stat;
        this.baseStatValue = baseStatValue;
        this.currentStatValue = currentStatValue;
    }

    public void InitStat()
    {
        SetCurrentStatValue(baseStatValue);
    }

    public void SetCurrentStatValue(float newValue)
    {
        currentStatValue = newValue;
        onStatUpdated?.Invoke(this);
    }

    public void IncreaseCurrentStatValue(float valueToAdd)
    {
        SetCurrentStatValue(currentStatValue + valueToAdd);
    }

    public void DecreaseCurrentStatValue(float valueToRemove)
    {
        SetCurrentStatValue(currentStatValue - valueToRemove);
    }

    public float GetCurrentStatValue()
    {
        return currentStatValue;
    }

    public float GetBaseStatValue()
    {
        return baseStatValue;
    }
}

public class PlayerStatsManager : MonoBehaviour
{
    [SerializeField] CharacterData playerCharData;
    public List<StatData> playerStats = new List<StatData>();

    private void OnEnable()
    {
        PlayerEquipmentManager.onEquippedItemAdded += OnEquippedItemAdded;
        PlayerEquipmentManager.onEquippedItemRemoved += OnEquippedItemRemoved;

        PlayerSkillsManager.onSkillUpdated += OnSkillUpdated;

        InventoryContextMenu.onBoosterUsed += OnBoosterUsed;
    }

    private void OnDisable()
    {
        PlayerEquipmentManager.onEquippedItemAdded -= OnEquippedItemAdded;
        PlayerEquipmentManager.onEquippedItemRemoved -= OnEquippedItemRemoved;

        PlayerSkillsManager.onSkillUpdated -= OnSkillUpdated;

        InventoryContextMenu.onBoosterUsed -= OnBoosterUsed;
    }

    public void Init(CharacterData newPlayerCharData)
    {
        playerCharData = newPlayerCharData;

        foreach (StatData stat in playerCharData.baseCharStats)
        {
            StatData newStat = new StatData(stat.stat, stat.GetBaseStatValue(), stat.GetCurrentStatValue());
            playerStats.Add(newStat);
            newStat.InitStat();
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
            ApplyStatModifier(statModifier);
        }
    }

    void OnBoosterUsed(ISlot slot)
    {
        ConsumableItemData consumableItemData = slot.GetItemStack().itemData as ConsumableItemData;
        if (consumableItemData)
        {
            foreach(StatModifier statModifier in consumableItemData.statModifiers)
            {
                ApplyStatModifier(statModifier);
            }
        }

        slot.RemoveItem();
    }

    void ApplyStatModifier(StatModifier statModifier)
    {
        StatData stat = GetPlayerStat(statModifier.statToModify);
        switch (statModifier.modifyOperation)
        {
            case ModifyOperation.Increase:
                stat.IncreaseCurrentStatValue(statModifier.modifyAmount);
                break;
            case ModifyOperation.Decrease:
                stat.DecreaseCurrentStatValue(statModifier.modifyAmount);
                break;
            case ModifyOperation.IncreaseByPercentage:
                stat.IncreaseCurrentStatValue(stat.GetBaseStatValue() * ((statModifier.modifyAmount) / 100));
                break;
            case ModifyOperation.DecreaseByPercentage:
                stat.DecreaseCurrentStatValue(stat.GetBaseStatValue() * ((statModifier.modifyAmount) / 100));
                break;
        }
    }

    //Same as apply just reversed
    void RemoveStatModifier(StatModifier statModifier)
    {
        StatData stat = GetPlayerStat(statModifier.statToModify);
        switch (statModifier.modifyOperation)
        {
            case ModifyOperation.Increase:
                stat.DecreaseCurrentStatValue(statModifier.modifyAmount);
                break;
            case ModifyOperation.Decrease:
                stat.IncreaseCurrentStatValue(statModifier.modifyAmount);
                break;
            case ModifyOperation.IncreaseByPercentage:
                stat.DecreaseCurrentStatValue(stat.GetBaseStatValue() * ((statModifier.modifyAmount) / 100));
                break;
            case ModifyOperation.DecreaseByPercentage:
                stat.IncreaseCurrentStatValue(stat.GetBaseStatValue() * ((statModifier.modifyAmount) / 100));
                break;
        }
    }

    StatData GetPlayerStat(ModifiableCharacterStats statToGet)
    {
        StatData statToReturn = null;
        foreach (StatData stat in playerStats)
        {
            if (stat.stat == statToGet)
                statToReturn = stat;
        }

        //If stat dosent exist, create it
        if(statToReturn == null)
        {
            StatData newStat = new StatData(statToGet, 0, 0);
            playerStats.Add(newStat);
            statToReturn = newStat;
        }

        return statToReturn;
    }

    public void Load()
    {
        foreach (StatData stat in playerStats)
        {
            stat.InitStat();
        }
    }
}
