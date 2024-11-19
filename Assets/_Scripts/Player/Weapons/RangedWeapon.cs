using System;
using System.Collections;
using System.Threading.Tasks;
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

    public override void InitWeapon(int occupyingSlotIndex, WeaponItemData dataToInit)
    {
        base.InitWeapon(occupyingSlotIndex, dataToInit);

        reserveAmmo = GetReserveAmmo();
        //if (reserveAmmo < dataToInit.magSize)
        //{
        //    loadedAmmo = reserveAmmo;
        //    playerInventoryManager.DecreaseAmmoOfType(dataToInit.ammoType, reserveAmmo);
        //    reserveAmmo = 0;
        //}
        //else
        //{
        //    loadedAmmo = dataToInit.magSize;
        //    playerInventoryManager.DecreaseAmmoOfType(dataToInit.ammoType, loadedAmmo);
        //    reserveAmmo -= loadedAmmo;
        //}

        onAmmoUpdated?.Invoke(occupiedSlotIndex, loadedAmmo, reserveAmmo);
    }

    public override void UseWeapon()
    {
        if (!isWeaponDrawn && !canUse)
            return;

        base.UseWeapon();
        if(loadedAmmo > 0)
        {
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
    }

    private void Shoot()
    {
        weaponAnimator.Play("Fire");
        muzzleFX.Play();
        weaponAudioSource.PlayOneShot(weaponItemData.attackSFX[Random.Range(0, weaponItemData.attackSFX.Length)]);
        
        loadedAmmo--;


        onAmmoUpdated?.Invoke(occupiedSlotIndex, loadedAmmo, reserveAmmo);

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

    public override void RemoveWeapon()
    {
        base.RemoveWeapon();
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
