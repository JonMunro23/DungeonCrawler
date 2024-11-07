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
    leftHand,
    rightHand
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
    public float itemCooldown;
    public EquipmentSlotType EquipmentSlotType;    
    public List<StatModifier> statModifiers = new List<StatModifier>();
}

