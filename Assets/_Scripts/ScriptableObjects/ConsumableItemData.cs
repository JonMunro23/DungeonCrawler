using UnityEngine;

public enum ConsumableType
{
    HealSyringe,
    Throwable,
    Ammo
}

[CreateAssetMenu(fileName = "ConsumableItem", menuName = "Items/New Consumable Item")]
public class ConsumableItemData : ItemData
{
    [Header("Consumable Item Properties")]
    public ConsumableType consumableType;
    public AmmoType ammoType;
    public float cooldownBetweenUses;
    
    public int totalRegenAmount = 75;
    public float regenDuration = 3f;

    [Header("Animation Lengths")]
    public float useAnimationLength;
}

