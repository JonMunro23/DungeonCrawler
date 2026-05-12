using System.Collections.Generic;
using UnityEngine;

public enum EquipmentSlotType
{
    None,
    Head,
    Chest,
    Legs,
    Boots,
    Gloves,
    //neck,
    Back,
    //ring,
    weaponSlot
};

public enum CharacterStats
{
    MaxHealth,
    Armour,
    Evasion,
    BonusWeaponDamage,
    CritChance,
    CritMultiplier,
    BonusBurstCount,
    WeaponAccuracy,
    RadiationResistance,
    FireResistance,
    AcidResistance
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
    public CharacterStats statToModify;
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

