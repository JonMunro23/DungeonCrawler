using UnityEngine;

public enum AmmoType
{
    Pistol,
    Rifle,
    Shells
};

[CreateAssetMenu(fileName = "WeaponItemData", menuName = "Items/New Weapon Item")]
public class WeaponItemData : EquipmentItemData
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
    public float reloadDuration = 2;
    public AmmoType ammoType;
    public int magSize;
    public int projectileCount;
    public bool isProjectile;
    public ProjectileData projectileData;
    public bool isBurst;
    public float perShotInBurstDelay;
    public int burstLength;

    [Header("Weapon SFX")]
    public AudioClip[] attackSFX;
    public float attackSFXVolume = .3f;
    public AudioClip specialSFX;
    public float specialSFXVolume = .3f;

    [Header("Animations")]
    public float drawAnimDuration;
    public AudioClip drawSFX;
    public float drawVolume = .3f;
    [Space]
    public float hideAnimDuration;
    public AudioClip hideSFX;
    public float hideVolume = .3f;
    [Space]
    public float reloadAnimDuration;
    public AudioClip reloadSFX;
    public float reloadVolume = .3f;
    [Space]
    public bool bulletByBulletReload;
    [Space]
    public float reloadStartAnimDuration;
    public AudioClip reloadStartSFX;
    public float reloadStartVolume = .3f;
    [Space]
    public float reloadInsertAnimDuration;
    public AudioClip reloadInsertSFX;
    public float reloadInsertVolume = .3f;
    [Space]
    public float reloadEndAnimDuration;
    public AudioClip reloadEndSFX;
    public float reloadEndVolume = .3f;
}

