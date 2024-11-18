using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class Weapon : MonoBehaviour, IWeapon
{
    public WeaponItemData weaponItemData;
    public Animator weaponAnimator;

    public EquipmentSlotType currentOccupuiedSlot = EquipmentSlotType.None;

    public bool canUse;
    public bool canShootBurst = true;
    public bool isWeaponDrawn;
    public bool isReloading;

    public async Task DrawWeapon()
    {
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

    public int GetLoadedAmmo()
    {
        throw new NotImplementedException();
    }

    public void Grab()
    {
        if (!IsInUse())
            weaponAnimator.Play("Interact");
    }



    public void InitWeapon(WeaponItemData dataToInit)
    {
        weaponItemData = dataToInit;
        canUse = true;
        weaponAnimator = GetComponent<Animator>();
    }

    public bool IsReloading() => isReloading;

    public bool IsTwoHanded() => weaponItemData.isTwoHanded;

    public void RemoveWeapon()
    {
        Destroy(gameObject);
    }

    public void Reload()
    {
        if (isReloading || !isWeaponDrawn)
            return;

        isReloading = true;
        weaponAnimator.Play("Reload");
        StartCoroutine(ReloadTimer());
    }

    public void Use()
    {
        if (!canUse || !isWeaponDrawn)
            return;

        UseWeapon();
    }

    public void UseSpecial()
    {
        if(!canUse) 
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
        yield return new WaitForSeconds(weaponItemData.itemCooldown);
        canUse = true;
        canShootBurst = true;
    }

    IEnumerator ReloadTimer()
    {
        yield return new WaitForSeconds(weaponItemData.reloadDuration);
        isReloading = false;
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
}
