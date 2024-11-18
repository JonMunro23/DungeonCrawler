using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class RangedWeapon : Weapon
{
    [SerializeField] Transform projectileSpawnLocation;

    AudioSource weaponAudioSource;
    ParticleSystem muzzleFX;

    //bool IsShootingBurst;
    Coroutine burstCoroutine;
    bool canShootBurstShot = true;

    private void Awake()
    {
        weaponAudioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        muzzleFX = projectileSpawnLocation.GetComponent<ParticleSystem>();
    }

    public override void UseWeapon()
    {
        if (!isWeaponDrawn && !canUse)
            return;

        base.UseWeapon();
        //if (CheckAmmo(weaponData.ammoType) != 0)
        //{

        if (weaponItemData.isProjectile)
        {
            GameObject projectile = Instantiate(weaponItemData.projectileData.projModel, projectileSpawnLocation.position, projectileSpawnLocation.rotation);
            //projectile.GetComponentInChildren<Projectile>().projectile = handItemData.itemProjectile;
            projectile.GetComponentInChildren<Projectile>().damage = CalculateDamage();
        }
        else
        {
            if (weaponItemData.isBurst)
            {
                TryShootBurst();
            }
            else
            {
                Shoot();
            }

        }
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
        weaponAudioSource.PlayOneShot(weaponItemData.attackSFX[Random.Range(0, weaponItemData.attackSFX.Length)]);

        RaycastHit hit;
        for (int i = 0; i < weaponItemData.projectileCount; i++)
        {
            if (Physics.Raycast(projectileSpawnLocation.position, projectileSpawnLocation.forward, out hit, weaponItemData.itemRange * 3))
            {

                IDamageable damageable = hit.transform.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    int damage = CalculateDamage();
                    bool isCrit = RollForCrit();
                    if (isCrit)
                        damage *= Mathf.CeilToInt(weaponItemData.critDamageMultiplier);

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
        }
    }

    IEnumerator ShootBurst()
    {
        for (int i = 0; i < weaponItemData.burstLength; i++)
        {
            if (canShootBurstShot)
            {
                canShootBurstShot = false;
                Shoot();
                yield return new WaitForSeconds(weaponItemData.perShotInBurstDelay);
                canShootBurstShot = true;
            }
        }
    }
}
