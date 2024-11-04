using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum ItemType
{
    meleeWeapon,
    rangedWeapon,
    armour,
    ammunition,
    consumable,
    key,
    torch
};
public enum AmmoType
{
    bullets,
    rockets,
    shells
};

public enum SlotType
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

[CreateAssetMenu(fileName = "ItemData", menuName = "Items/New Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Global Item Properties")]
    public string itemName;
    [TextArea]
    public string itemDescription;
    public Sprite itemSprite;
    public WorldItem itemWorldModel;
    public GameObject itemPrefab;

    
    public ItemType itemType;

    public float itemCooldown;

    public int itemDamageMin;
    public int itemDamageMax;
    public float critChance;
    public float critDamageMultiplier;
    public string itemDamageType;
    public bool isTwoHanded;

    public float itemWeight;
    public float itemValue;

    public bool isItemStackable;
    public int maxItemStackSize;


    

    public bool isBurst;
    public float perShotInBurstDelay;
    public int burstLength;

    [Header("Projectile Properties")]
    public bool usesProjectiles;
    public int projectileAmount;
    public int itemRange;
    public AmmoType ammoType;
    public ProjectileObject itemProjectile;

    public AudioClip[] fireSFX;
    public AudioClip drawSFX;
    public AudioClip hideSFX;
    public AudioClip reloadSFX;

    
    [Header("Armour Properties")]
    public SlotType slotType;
    [Space]
    [Header("Armour Stat Bonuses")]

    public int armourBonus;
    public int evasionBonus;

    public enum KeyType
    {
        rusted,
        silver,
        golden,
        writhing,
        sticky
    };
    public KeyType keyType;

    //public enum ConsumableType
    //{
    //    healing,
    //    buff,

    //};
    //[Header("Consumable Properties")]
    //public ConsumableType consumableType;
    //public float healAmount;

    //public enum BuffType
    //{
    //    Protection,
    //    StatIncrease,
    //    AttackUp
    //};
    //public BuffType buffType;

}

