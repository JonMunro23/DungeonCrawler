using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class Weapon : MonoBehaviour, IWeapon
{
    [Header("References")]
    public WeaponItemData weaponItemData;
    public Animator weaponAnimator;
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

    public virtual async Task DrawWeapon()
    {
        onAmmoUpdated?.Invoke(occupiedSlotIndex, loadedAmmo, GetReserveAmmo());
        weaponAnimator.enabled = true;
        weaponAnimator.Play("Draw");
        await Task.Delay((int)(weaponItemData.drawAnimDuration * 1000));
        isWeaponDrawn = true;
        canUse = true;
        canShootBurst = true;
    }

    public async Task HolsterWeapon()
    {
        isWeaponDrawn = false;
        weaponAnimator.Play("Hide");
        await Task.Delay((int)(weaponItemData.hideAnimDuration * 1000));
        isWeaponDrawn = false;
        if(weaponAnimator)
            weaponAnimator.enabled = false;
    }

    public void Grab()
    {
        if (!IsInUse())
            weaponAnimator.Play("Interact");
    }



    public virtual void InitWeapon(int occupyingSlotIndex, WeaponItemData dataToInit)
    {
        occupiedSlotIndex = occupyingSlotIndex;
        weaponItemData = dataToInit;
        canUse = true;
        weaponAnimator = GetComponent<Animator>();

    }

    public bool IsReloading() => isReloading;

    public bool IsTwoHanded() => weaponItemData.isTwoHanded;

    public virtual void RemoveWeapon()
    {
        Destroy(gameObject);
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
        playerInventoryManager.IncreaseAmmoOfType(weaponItemData.ammoType, loadedAmmo);
        remainingAmmo = playerInventoryManager.GetRemainingAmmoOfType(weaponItemData.ammoType);
        loadedAmmo = 0;
        onAmmoUpdated?.Invoke(occupiedSlotIndex, loadedAmmo, GetReserveAmmo());

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
        if (!canUse || !isWeaponDrawn || isReloading || loadedAmmo == 0)
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
        isReloading = true;
        weaponAnimator.Play("Reload");
        await Task.Delay((int)(weaponItemData.reloadDuration * 1000));
        isReloading = false;
        loadedAmmo = reloadAmount;
        playerInventoryManager.DecreaseAmmoOfType(weaponItemData.ammoType, reloadAmount);
        onAmmoUpdated?.Invoke(occupiedSlotIndex, loadedAmmo, GetReserveAmmo());
    }

    public int CalculateDamage()
    {
        float damage = Random.Range(weaponItemData.itemDamageMinMax.x, weaponItemData.itemDamageMinMax.y);
        return Mathf.CeilToInt(damage);
    }

    public bool RollForCrit()
    {
        bool wasCrit = false;
        if (weaponItemData.critChance > 0)
        {
            float rand = Random.Range(0, 101);
            if (rand <= weaponItemData.critChance)
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
}
