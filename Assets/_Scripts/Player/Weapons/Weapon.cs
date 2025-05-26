using DG.Tweening;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class Weapon : MonoBehaviour, IWeapon
{
    [Header("References")]
    public WeaponItemData weaponItemData;
    public Animator weaponAnimator;
    public AudioEmitter weaponAudioEmitter;

    public WeaponSlot occupyingSlot;
    public IInventory playerInventory;

    public bool canUse;
    public bool isWeaponDrawn;
    public bool isDefaultWeapon;

    /// <summary>
    /// In order:
    /// int = occupiedSlotIndex
    /// int = loadedAmmo
    /// int = reserveAmmo
    /// </summary>
    public static Action<int, int> onLoadedAmmoUpdated;
    public static Action<int, int> onReserveAmmoUpdated;
    /// <summary>
    /// float = cooldownLength
    /// </summary>
    public static Action<float> onWeaponCooldownActive;


    public virtual bool CanUse()
    {
        return canUse && isWeaponDrawn;
    }
   
    public bool IsMeleeWeapon() => GetWeaponData().weaponType == WeaponType.Melee;
    public bool IsDefaultWeapon() => isDefaultWeapon;
    public void SetWeaponActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }
    public void SetDefaultWeapon(bool isDefault)
    {
        isDefaultWeapon = isDefault;
    }
    public WeaponItemData GetWeaponData() => weaponItemData;
    public abstract MeleeWeapon GetMeleeWeapon();
    public abstract RangedWeapon GetRangedWeapon();
    public virtual void InitWeapon(WeaponSlot occupyingSlot, WeaponItemData dataToInit, AudioEmitter _weaponAudioEmitter, IInventory _playerInventory)
    {
        playerInventory = _playerInventory;
        this.occupyingSlot = occupyingSlot;
        weaponItemData = dataToInit;
        canUse = true;
        weaponAnimator = GetComponent<Animator>();

        weaponAudioEmitter = _weaponAudioEmitter;
    }
    public virtual async Task DrawWeapon()
    {
        weaponAnimator.enabled = true;
        weaponAnimator.Play("Draw");
        weaponAudioEmitter.ForcePlay(weaponItemData.drawSFX, weaponItemData.drawVolume);
        await Task.Delay((int)(weaponItemData.drawAnimDuration * 1000));
        if (!IsMeleeWeapon())
            GetRangedWeapon().UnreadyWeapon();
        isWeaponDrawn = true;
        canUse = true;
    }
    
    public async Task HolsterWeapon()
    {
        isWeaponDrawn = false;
        if(weaponAnimator.gameObject.activeSelf && weaponAnimator.enabled)
        {
            weaponAnimator.Play("Hide");
            weaponAudioEmitter.ForcePlay(weaponItemData.hideSFX, weaponItemData.hideVolume);
            await Task.Delay((int)(weaponItemData.hideAnimDuration * 1000));
            if(weaponAnimator)
                weaponAnimator.enabled = false;
        }
    }
    public async Task Grab()
    {
        canUse = false;
        weaponAnimator.Play("Interact");
        await Task.Delay((int)(weaponItemData.grabAnimDuration * 1000));
        canUse = true;
    }
    public virtual void RemoveWeapon()
    {
        Destroy(gameObject);
    }
    public void TryUse()
    {
        if (!IsMeleeWeapon())
            if (!GetRangedWeapon().isWeaponReady)
                return;

        if (!CanUse())
            return;

        UseWeapon();
    }
    public virtual void UseWeapon()
    {
        canUse = false;
        StartCoroutine(UseCooldown());
    }

    public virtual IEnumerator UseCooldown()
    {
        onWeaponCooldownActive?.Invoke(weaponItemData.itemCooldown);
        yield return new WaitForSeconds(weaponItemData.itemCooldown);
        canUse = true;
    }
    public int CalculateDamage(int targetArmourRating)
    {
        float damage = 
            Random.Range(weaponItemData.itemDamageMinMax.x, weaponItemData.itemDamageMinMax.y)
            + PlayerWeaponManager.bonusDamage
            - targetArmourRating;

        if (damage < 0)
            damage = 0;

        return Mathf.CeilToInt(damage);
    }
    public bool RollForCrit()
    {
        bool wasCrit = false;
        if (weaponItemData.critChance > 0)
        {
            float rand = Random.Range(0, 101);
            if (rand <= weaponItemData.critChance + PlayerWeaponManager.bonusCritChance)
            {
                wasCrit = true;
            }
        }
        return wasCrit;
    }
    public AudioClip GetRandomClipFromArray(AudioClip[] arrayToPullFrom)
    {
        AudioClip randClip = null;

        int rand = Random.Range(0, arrayToPullFrom.Length);
        randClip = arrayToPullFrom[rand];
        return randClip;
    }

    public int UnloadAmmo()
    {
        RangedWeapon rangedWeapon = this as RangedWeapon;
        if(rangedWeapon)
        {
            int ammoToReturn = rangedWeapon.GetLoadedAmmo();
            rangedWeapon.UpdateLoadedAmmo(0);
            return ammoToReturn;
        }

        return 0;
    }

    /// <summary>
    /// Return min and max damage ranges after any additional modifiers
    /// </summary>
    /// <returns>Damage range</returns>
    public Vector2 GetWeaponDamageRange()
    {
        return new Vector2(weaponItemData.itemDamageMinMax.x + PlayerWeaponManager.bonusDamage, weaponItemData.itemDamageMinMax.y + PlayerWeaponManager.bonusDamage);
    }

}
