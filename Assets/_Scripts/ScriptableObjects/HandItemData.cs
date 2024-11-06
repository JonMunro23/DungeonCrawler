using UnityEngine;

public enum AmmoType
{
    bullets,
    rockets,
    shells
};

[CreateAssetMenu(fileName = "WeaponItemData", menuName = "Items/New Weapon Item")]
public class HandItemData : EquipmentItemData
{
    [Header("Weapon Properties")]
    public GameObject itemPrefab;
    public bool isTwoHanded;
    public bool isMeleeWeapon;
    //public string itemDamageType;
    public Vector2 itemDamageMinMax;
    public int itemRange;
    public float critChance;
    public float critDamageMultiplier;

    public AmmoType ammoType;
    public int projectileCount;
    public bool isProjectile;
    public ProjectileData projectileData;
    public bool isBurst;
    public float perShotInBurstDelay;
    public int burstLength;

    public AudioClip[] attackSFX;
    public AudioClip specialSFX;
    public AudioClip drawSFX;
    public AudioClip hideSFX;
    public AudioClip reloadSFX;
}

