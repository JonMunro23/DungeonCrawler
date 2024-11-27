using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class RangedWeapon : Weapon
{
    [SerializeField] Transform projectileSpawnLocation;
    ParticleSystem muzzleFX;
    Coroutine burstCoroutine;
    bool canShootBurstShot = true;

    public bool infinteAmmo = false;

    private void Start()
    {
        muzzleFX = projectileSpawnLocation.GetComponent<ParticleSystem>();
    }

    public override void InitWeapon(int occupyingSlotIndex, WeaponItemData dataToInit, AudioEmitter weaponAudioEmitter)
    {
        base.InitWeapon(occupyingSlotIndex, dataToInit, weaponAudioEmitter);

        reserveAmmo = GetReserveAmmo();

        onAmmoUpdated?.Invoke(occupiedSlotIndex, loadedAmmo, reserveAmmo);
    }

    public override void UseWeapon()
    {
        if (!isWeaponDrawn && !canUse)
            return;

        base.UseWeapon();
        if(loadedAmmo > 0 || infinteAmmo)
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

    private Vector3 GetBulletSpread()
    {
        Vector2 randomPoint = new Vector2(Random.Range(-.05f, .05f), Random.Range(-.05f, .05f));
        return new Vector3(randomPoint.x, randomPoint.y, 1);
    }

    AudioClip GetRandomClip()
    {
        AudioClip randClip = null;

        int rand = Random.Range(0, weaponItemData.attackSFX.Length);
        randClip = weaponItemData.attackSFX[rand];
        return randClip;
    }

    private void Shoot()
    {
        weaponAnimator.Play("Fire");
        muzzleFX.Play();

        weaponAudioEmitter.ForcePlay(GetRandomClip(), weaponItemData.attackSFXVolume);
        
        if(!infinteAmmo)
            loadedAmmo--;

        onAmmoUpdated?.Invoke(occupiedSlotIndex, loadedAmmo, reserveAmmo);

        RaycastHit hit;
        for (int i = 0; i < weaponItemData.projectileCount; i++)
        {
            Vector3 origin = projectileSpawnLocation.position;
            Vector3 direction = projectileSpawnLocation.TransformDirection(GetBulletSpread());

            Ray ray = new Ray(origin, direction);
            if (Physics.Raycast(ray, out hit, weaponItemData.itemRange * 3))
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

                SurfaceIdentifier surf = hit.collider.GetSurface();
                BulletDecalsManager.Instance.CreateBulletDecal(surf, hit);
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
            if (canShootBurstShot && loadedAmmo > 0)
            {
                canShootBurstShot = false;
                Shoot();
                yield return new WaitForSeconds(weaponItemData.perShotInBurstDelay);
                canShootBurstShot = true;
            }
        }
    }
}
