using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Weapon : MonoBehaviour, IWeapon
{
    public HandItemData handItemData;
    public Hands currentOccuipedHand;
    
    public Animator weaponAnimator;

    public bool canUse;
    public bool canShootBurst = true;
    public bool isWeaponDrawn;
    public bool isReloading;

    public static Action<Hands> OnHandCooldownBegins;
    public static Action<Hands> OnHandCooldownEnds;


    public IEnumerator DrawWeapon()
    {
        weaponAnimator.Play("Draw");
        yield return new WaitForSeconds(handItemData.drawAnimDuration);
        isWeaponDrawn = true;
    }

    public int GetLoadedAmmo()
    {
        throw new NotImplementedException();
    }

    public void Grab()
    {
        if (!isReloading && canUse)
            weaponAnimator.Play("Interact");
    }

    public IEnumerator HolsterWeapon(Action onHolsteredCallback)
    {
        weaponAnimator.Play("Hide");
        yield return new WaitForSeconds(handItemData.drawAnimDuration);
        isWeaponDrawn = false;

        onHolsteredCallback?.Invoke();
    }

    public void InitWeapon(HandItemData dataToInit, Hands inHand)
    {
        handItemData = dataToInit;
        currentOccuipedHand = inHand;
        canUse = true;

        weaponAnimator = GetComponent<Animator>();

        StartCoroutine(DrawWeapon());
    }

    public bool IsReloading() => isReloading;

    public bool IsTwoHanded() => handItemData.isTwoHanded;

    public void RemoveWeapon()
    {
        Destroy(gameObject);
    }

    public void TryReloadWeapon()
    {
        if (isReloading)
            return;

        isReloading = true;
        weaponAnimator.Play("Reload");
        StartCoroutine(ReloadTimer());
    }

    public void Use()
    {
        if (!canUse)
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
        OnHandCooldownBegins?.Invoke(currentOccuipedHand);
        yield return new WaitForSeconds(handItemData.itemCooldown);
        OnHandCooldownEnds?.Invoke(currentOccuipedHand);
        canUse = true;
        canShootBurst = true;
    }

    IEnumerator ReloadTimer()
    {
        yield return new WaitForSeconds(handItemData.reloadDuration);
        isReloading = false;
    }

    public int CalculateDamage()
    {
        float damage = Random.Range(handItemData.itemDamageMinMax.x, handItemData.itemDamageMinMax.y);
        return Mathf.CeilToInt(damage);
    }

    public bool RollForCrit()
    {
        bool wasCrit = false;
        if (handItemData.critChance > 0)
        {
            float rand = Random.Range(0, 101);
            if (rand <= handItemData.critChance)
            {
                wasCrit = true;
            }
        }
        return wasCrit;
    }


}
