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
    public float cooldownBetweenUses;
    
    public float healthRegenAmount = .5f;
    public float healthRegenDuration = 3f;

    [Header("Animation Lengths")]
    public float useAnimationLength;
}

