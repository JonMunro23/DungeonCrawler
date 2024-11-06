using System.Collections.Generic;
using UnityEngine;

public enum EquipmentSlotType
{
    head,
    chest,
    legs,
    boots,
    gloves,
    neck,
    back,
    ring,
    hands
};

public enum ModifiableStats
{
    MaxHealth,
    Armour,
    Evasion,
    Damage,
    CritChance,
    CritMultiplier,
    BurstAmount
}

public enum ModifyOperation
{
    Increase,
    Decrease,
    IncreaseByPercentage,
    DecreaseByPercentage,
}

[System.Serializable]
public class StatModifier
{
    public ModifiableStats statToModify;
    public ModifyOperation modifyOperation;
    public float modifyAmount;
}

[CreateAssetMenu(fileName = "EquipmentItemData", menuName = "Items/New Equipment Item")]
public class EquipmentItemData : ItemData
{
    [Header("Equipment Properties")]
    public EquipmentSlotType EquipmentSlotType;
    public float itemCooldown;
    
    public List<StatModifier> statModifiers = new List<StatModifier>();
    //add equipment stats
    //evasion + 10,
    //crit chance + 0.4%,
    //etc.
}

