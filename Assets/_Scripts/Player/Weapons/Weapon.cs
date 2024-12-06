using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class Weapon : MonoBehaviour, IWeapon
{
    [Header("References")]
    public WeaponItemData weaponItemData;
    public Animator weaponAnimator;
    public AudioEmitter weaponAudioEmitter;

    public int occupiedSlotIndex;
    public IInventory playerInventoryManager;
    public int loadedAmmo, reserveAmmo;

    public bool canUse;
    public bool canShootBurst = true;
    public bool isWeaponDrawn;
    public bool isReloading;

    /// <summary>
    /// In order:
    /// int = occupiedSlotIndex
    /// int = loadedAmmo
    /// int = reserveAmmo
    /// </summary>
    public static Action<int, int, int> onAmmoUpdated;
    /// <summary>
    /// float = cooldownLength
    /// </summary>
    public static Action<float> onWeaponCooldownActive;

    [SerializeField] Transform magDropTransform;
    [SerializeField] int maxDroppedMags = 5;
    [SerializeField] int lastDroppedMag;
    List<GameObject> droppedMagList = new List<GameObject>();

    public virtual async Task DrawWeapon()
    {
        onAmmoUpdated?.Invoke(occupiedSlotIndex, loadedAmmo, GetReserveAmmo());
        weaponAnimator.enabled = true;
        weaponAnimator.Play("Draw");
        weaponAudioEmitter.ForcePlay(weaponItemData.drawSFX, weaponItemData.drawVolume);
        await Task.Delay((int)(weaponItemData.drawAnimDuration * 1000));
        isWeaponDrawn = true;
        canUse = true;
        canShootBurst = true;
    }

    public async Task HolsterWeapon()
    {
        isWeaponDrawn = false;
        weaponAnimator.Play("Hide");
        weaponAudioEmitter.ForcePlay(weaponItemData.hideSFX, weaponItemData.hideVolume);
        await Task.Delay((int)(weaponItemData.hideAnimDuration * 1000));
        isWeaponDrawn = false;
        if(weaponAnimator)
            weaponAnimator.enabled = false;
    }

    public void Grab()
    {
        weaponAnimator.Play("Interact");
    }

    public virtual void InitWeapon(int occupyingSlotIndex, WeaponItemData dataToInit, AudioEmitter _weaponAudioEmitter)
    {
        occupiedSlotIndex = occupyingSlotIndex;
        weaponItemData = dataToInit;
        canUse = true;
        weaponAnimator = GetComponent<Animator>();

        weaponAudioEmitter = _weaponAudioEmitter;
    }

    public bool IsReloading() => isReloading;

    public bool IsTwoHanded() => weaponItemData.isTwoHanded;

    public virtual void RemoveWeapon()
    {
        Destroy(gameObject);
    }

    public void DropMagazine(Collider character)
    {

        if (!weaponItemData.magDropPrefab || !magDropTransform)
            return;

        // Object pooling
        if (droppedMagList.Count == maxDroppedMags)
        {
            int mag = lastDroppedMag++ % maxDroppedMags;
            droppedMagList[mag].transform.position = magDropTransform.position;
            droppedMagList[mag].transform.rotation = magDropTransform.rotation;
            droppedMagList[mag].GetComponent<Rigidbody>().linearVelocity = Physics.gravity;
        }
        else
        {
            Rigidbody magazine = Instantiate(weaponItemData.magDropPrefab, magDropTransform.position, magDropTransform.rotation);
            magazine.linearVelocity = Physics.gravity;

            Physics.IgnoreCollision(magazine.GetComponent<Collider>(), character, true);
            droppedMagList.Add(magazine.gameObject);
        }
    }

    

    public async Task TryReload()
    {
        if (isReloading || !isWeaponDrawn)
            return;

        if (loadedAmmo == weaponItemData.magSize)
            return;

        int remainingAmmo = playerInventoryManager.GetRemainingAmmoOfType(weaponItemData.ammoType);
        if (remainingAmmo == 0)
            return;

        playerInventoryManager.LockSlotsWithAmmoOfType(weaponItemData.ammoType);
        if(!weaponItemData.bulletByBulletReload)
        {
            playerInventoryManager.IncreaseAmmoOfType(weaponItemData.ammoType, loadedAmmo);
            loadedAmmo = 0;
            onAmmoUpdated?.Invoke(occupiedSlotIndex, loadedAmmo, GetReserveAmmo());

            DropMagazine(transform.root.GetComponent<Collider>());
        }
        remainingAmmo = playerInventoryManager.GetRemainingAmmoOfType(weaponItemData.ammoType);

        int amountToReload = 0;
        if (remainingAmmo >= weaponItemData.magSize)
        {
            amountToReload = weaponItemData.magSize;
        }
        else if (remainingAmmo < weaponItemData.magSize)
        {
            amountToReload = remainingAmmo;
        }


        await PerformReload(amountToReload);

        playerInventoryManager.UnlockSlots();
    }

    public void Use()
    {
        if (!canUse || !isWeaponDrawn || isReloading || (!weaponItemData.isMeleeWeapon && loadedAmmo == 0))
            return;

        UseWeapon();
    }

    public void UseSpecial()
    {
        if(!canUse || !isWeaponDrawn || isReloading) 
            return;

        Debug.Log("Used special");
    }

    public virtual void UseWeapon()
    {
        canUse = false;
        StartCoroutine(UseCooldown());
    }

    public IEnumerator UseCooldown()
    {
        onWeaponCooldownActive?.Invoke(weaponItemData.itemCooldown);
        yield return new WaitForSeconds(weaponItemData.itemCooldown);
        canUse = true;
        canShootBurst = true;
    }

    async Task PerformReload(int reloadAmount)
    {
        if(weaponItemData.bulletByBulletReload)
        {
            isReloading = true;
            if (loadedAmmo == 0)
            {
                weaponAnimator.Play("InsertInChamber");
                weaponAudioEmitter.ForcePlay(weaponItemData.reloadInsertInChamberSFX, weaponItemData.reloadInsertInChamberVolume);
                await Task.Delay((int)(weaponItemData.reloadInsertInChamberAnimDuration * 1000));
                loadedAmmo++;
                playerInventoryManager.DecreaseAmmoOfType(weaponItemData.ammoType, 1);
                onAmmoUpdated?.Invoke(occupiedSlotIndex, loadedAmmo, GetReserveAmmo());
            }
            else
            {
                weaponAnimator.Play("StartReload");
                weaponAudioEmitter.ForcePlay(weaponItemData.reloadStartSFX, weaponItemData.reloadStartVolume);
                await Task.Delay((int)(weaponItemData.reloadStartAnimDuration * 1000));
            }

            while(loadedAmmo < weaponItemData.magSize && reserveAmmo > 0)
            {
                //weaponAnimator.Play("Insert");
                weaponAnimator.CrossFadeInFixedTime("Insert", .1f);
                weaponAudioEmitter.ForcePlay(weaponItemData.reloadInsertSFX, weaponItemData.reloadInsertVolume);
                await Task.Delay((int)(weaponItemData.reloadInsertAnimDuration * 1000));
                loadedAmmo++;
                playerInventoryManager.DecreaseAmmoOfType(weaponItemData.ammoType, 1);
                onAmmoUpdated?.Invoke(occupiedSlotIndex, loadedAmmo, GetReserveAmmo());
            }

            weaponAnimator.Play("StopReload");
            weaponAudioEmitter.ForcePlay(weaponItemData.reloadStopSFX, weaponItemData.reloadStopVolume);
            await Task.Delay((int)(weaponItemData.reloadEndAnimDuration * 1000));
            isReloading = false;
            return;
        }

        isReloading = true;
        weaponAnimator.Play("Reload");
        weaponAudioEmitter.ForcePlay(weaponItemData.reloadSFX, weaponItemData.reloadVolume);
        await Task.Delay((int)(weaponItemData.reloadAnimDuration * 1000));
        isReloading = false;
        loadedAmmo = reloadAmount;
        playerInventoryManager.DecreaseAmmoOfType(weaponItemData.ammoType, reloadAmount);
        onAmmoUpdated?.Invoke(occupiedSlotIndex, loadedAmmo, GetReserveAmmo());
    }

    public int CalculateDamage()
    {
        float damage = Random.Range(weaponItemData.itemDamageMinMax.x, weaponItemData.itemDamageMinMax.y);
        return Mathf.CeilToInt(damage) + PlayerWeaponManager.bonusDamage;
    }

    public bool RollForHit()
    {
        bool hasHit = false;
        if (weaponItemData.accuracy > 0)
        {
            float rand = Random.Range(0, 101);
            if (rand <= weaponItemData.accuracy + PlayerWeaponManager.bonusAccuracy)
            {
                hasHit = true;
            }
        }
        return hasHit;
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

    public void SetWeaponActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    public bool IsInUse()
    {
        bool isInUse = !canUse || !canShootBurst || !isWeaponDrawn || isReloading;
        return isInUse;
    }

    public WeaponItemData GetWeaponData()
    {
        return weaponItemData;
    }

    public void SetInventoryManager(IInventory playerInventory)
    {
        playerInventoryManager = playerInventory;
    }

    public virtual int GetLoadedAmmo()
    {
        return loadedAmmo;
    }

    public void SetLoadedAmmo(int loadedAmmo)
    {
        this.loadedAmmo = loadedAmmo;
    }

    public int GetReserveAmmo()
    {
        return playerInventoryManager.GetRemainingAmmoOfType(weaponItemData.ammoType);
    }

    public void UpdateReserveAmmo()
    {
        reserveAmmo = GetReserveAmmo();
        onAmmoUpdated?.Invoke(occupiedSlotIndex, loadedAmmo, reserveAmmo);
    }

    public AudioClip GetRandomClipFromArray(AudioClip[] arrayToPullFrom)
    {
        AudioClip randClip = null;

        int rand = Random.Range(0, arrayToPullFrom.Length);
        randClip = arrayToPullFrom[rand];
        return randClip;
    }
}
