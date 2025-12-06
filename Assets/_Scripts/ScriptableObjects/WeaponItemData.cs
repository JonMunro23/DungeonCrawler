using UnityEngine;

public enum WeaponType
{
    Melee,
    Pistol,
    SMG,
    Rifle,
    Shotgun,
    Magnum
}

public enum AmmoType
{
    Standard,
    ArmourPiercing,
    HollowPoint,
    Incendiary,
    Acid
}

public enum StatusEffectType
{
    None,
    Fire,
    Acid
}

public enum DamageType
{
    Standard,
    Fire,
    Acid,
    Explosive
}


[CreateAssetMenu(fileName = "WeaponItemData", menuName = "Items/New Weapon Item")]
public class WeaponItemData : EquipmentItemData
{
    [Header("Weapon Properties")]
    public GameObject itemPrefab;
    public Rigidbody magDropPrefab;
    public bool isTwoHanded;
    public WeaponType weaponType;
    public Vector2 itemDamageMinMax;
    public int itemRange;
    public int accuracy;
    public float critChance;
    public float critDamageMultiplier;
    public int magSize;
    public int projectileCount;
    public bool isProjectile;
    public ProjectileData projectileData;
    public bool isBurst;
    public float perShotInBurstDelay;
    public int burstLength;

    [Header("Ammo")]
    public AmmoItemData defaultLoadedAmmoData;

    [Header("Recoil Data")]
    public WeaponRecoilData recoilData;
    public float spreadReductionSpeed = 1.2f;
    public float perShotSpreadIncrease = .2f;
    public float onWeaponReadiedSpreadAmount = 2f;

    [Header("Weapon SFX")]
    public AudioClip[] attackSFX;
    public float attackSFXVolume = .3f;
    public AudioClip specialSFX;
    public float specialSFXVolume = .3f;

    [Header("Animations")]
    public float grabAnimDuration;
    public AudioClip grabSFX;
    public float grabVolume;
    [Space]
    public float drawAnimDuration;
    public AudioClip drawSFX;
    public float drawVolume = .3f;
    [Space]
    public float readyAnimDuration;
    public AudioClip readySFX;
    public float readyVolume = .3f;
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
    public float reloadInsertInChamberAnimDuration;
    public AudioClip reloadInsertInChamberSFX;
    public float reloadInsertInChamberVolume = .3f;
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
    public AudioClip reloadStopSFX;
    public float reloadStopVolume = .3f;
    [Space]
    public float ejectShellAnimDuration;
    public AudioClip ejectShellSFX;
    public float ejectShellVolume = .3f;
}

