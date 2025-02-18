using System.Collections.Generic;
using UnityEngine;

public enum ConsumableType
{
    HealSyringe,
    Booster
}

[CreateAssetMenu(fileName = "ConsumableItem", menuName = "Items/New Consumable Item")]
public class ConsumableItemData : ItemData
{
    [Header("Consumable Item Properties")]
    public ConsumableType consumableType;
    //public AmmoWeaponType ammoWeaponType;
    //public AmmoType ammoType;
    public float cooldownBetweenUses;
    
    public int totalRegenAmount = 75;
    public float regenDuration = 3f;

    [Header("Injector Properties")]
    public List<StatModifier> statModifiers = new List<StatModifier>();

    [Header("Animation Lengths")]
    public float useAnimationLength;
}

