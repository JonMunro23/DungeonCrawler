using System;
using System.Collections;
using System.Net.Sockets;
using UnityEngine;
using Random = UnityEngine.Random;

public class RangedWeapon : MonoBehaviour, IWeapon
{
    public HandItemData weaponData;
    public Hands currentOccuipedHand;

    [SerializeField] Transform projectileSpawnLocation;
    [SerializeField] bool canUse;

    Animator weaponAnimator;
    AudioSource weaponAudioSource;
    ParticleSystem muzzleFX;

    bool IsShootingBurst;
    bool canShootBurst = true;
    Coroutine burstCoroutine;
    bool canShootBurstShot = true;

    bool isReloading = false;

    public static Action<Hands> OnHandCooldownBegins;
    public static Action<Hands> OnHandCooldownEnds;

    private void Awake()
    {
        weaponAnimator = GetComponent<Animator>();
        weaponAudioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        muzzleFX = projectileSpawnLocation.GetComponent<ParticleSystem>();
    }

    public int GetLoadedAmmo()
    {
        throw new System.NotImplementedException();
    }

    public void InitWeapon(HandItemData dataToInit, Hands inHand)
    {
        weaponData = dataToInit;
        currentOccuipedHand = inHand;
        canUse = true;
    }

    public void TryReloadWeapon()
    {
        if (isReloading)
            return;

        isReloading = true;
        weaponAnimator.Play("Reload");
        StartCoroutine(ReloadTimer());
    }

    public void RemoveWeapon()
    {
        Destroy(gameObject);
    }

    public void Use()
    {
        if (!canUse)
            return;

        //if (CheckAmmo(weaponData.ammoType) != 0)
        //{
            canUse = false;

            if (weaponData.isProjectile)
            {
                GameObject projectile = Instantiate(weaponData.projectileData.projModel, projectileSpawnLocation.position, projectileSpawnLocation.rotation);
                //projectile.GetComponentInChildren<Projectile>().projectile = handItemData.itemProjectile;
                projectile.GetComponentInChildren<Projectile>().damage = CalculateDamage(weaponData);
            }
            else
            {
                if (weaponData.isBurst)
                {
                    TryShootBurst();
                }
                else
                {
                    Shoot();
                }

            }

            StartCoroutine(WeaponCooldown());

        //}
    }
    int CalculateDamage(HandItemData handItemData)
    {
        float damage = Random.Range(handItemData.itemDamageMinMax.x, handItemData.itemDamageMinMax.y);
        return Mathf.CeilToInt(damage);
    }

    bool RollForCrit(HandItemData handItemData)
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

    int CheckAmmo(AmmoType ammoType)
    {
        //if (ammoType == AmmoType.bullets)
        //{
        //    //check inventory for bullets and return amount
        //    if (bullets > 0)
        //    {
        //        bullets--;
        //    }
        //    return bullets;
        //}
        //else if (ammoType == AmmoType.rockets)
        //{
        //    //check inventory for rockets and return amount
        //    if (rockets > 0)
        //    {
        //        rockets--;
        //    }
        //    return rockets;
        //}
        //else if (ammoType == AmmoType.shells)
        //{
        //    //check inventory for shells and return amount
        //    if (shells > 0)
        //    {
        //        shells--;
        //    }
        //    return shells;
        //}
        return 0;
    }

    private void Shoot()
    {
        weaponAnimator.Play("Fire");
        muzzleFX.Play();
        weaponAudioSource.PlayOneShot(weaponData.attackSFX[Random.Range(0, weaponData.attackSFX.Length)]);

        RaycastHit hit;
        for (int i = 0; i < weaponData.projectileCount; i++)
        {
            if (Physics.Raycast(projectileSpawnLocation.position, projectileSpawnLocation.forward, out hit, weaponData.itemRange * 3))
            {

                IDamageable damageable = hit.transform.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    int damage = CalculateDamage(weaponData);
                    bool isCrit = RollForCrit(weaponData);
                    if (isCrit)
                        damage *= Mathf.CeilToInt(weaponData.critDamageMultiplier);

                    damageable.TakeDamage(damage, isCrit);
                }
            }
        }
    }

    void TryShootBurst()
    {
        if (canShootBurst)
        {
            canShootBurst = false;
            burstCoroutine = StartCoroutine(ShootBurst());
        }
    }

    public void StopBurst()
    {
        if (burstCoroutine != null)
        {
            StopCoroutine(burstCoroutine);
            canShootBurst = true;
            IsShootingBurst = false;
        }
    }

    IEnumerator ShootBurst()
    {
        IsShootingBurst = true;

        for (int i = 0; i < weaponData.burstLength; i++)
        {
            if (canShootBurstShot)
            {
                canShootBurstShot = false;
                Shoot();
                yield return new WaitForSeconds(weaponData.perShotInBurstDelay);
                canShootBurstShot = true;
            }
        }

        IsShootingBurst = false;
    }

    public bool CheckIsReloading() => isReloading;

    public void Grab()
    {
        if (!isReloading && canUse)
            weaponAnimator.Play("Interact");
    }

    IEnumerator ReloadTimer()
    {
        yield return new WaitForSeconds(weaponData.reloadDuration);
        isReloading = false;
    }

    IEnumerator WeaponCooldown()
    {
        OnHandCooldownBegins?.Invoke(currentOccuipedHand);
        yield return new WaitForSeconds(weaponData.itemCooldown);
        OnHandCooldownEnds?.Invoke(currentOccuipedHand);
        canUse = true;
        canShootBurst = true;
    }
}
