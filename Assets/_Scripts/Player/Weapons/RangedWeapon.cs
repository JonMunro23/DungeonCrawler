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

    [SerializeField] ParticleSystem shellEjectionParticleEffect;
    [SerializeField] Vector2 ejectionSpeed = new Vector2(1, 3);
    [SerializeField] float ejectionStartDelay;
    ParticleSystem[] cachedParticleEffect;

    private void Start()
    {
        muzzleFX = projectileSpawnLocation.GetComponent<ParticleSystem>();
    }

    public override void InitWeapon(int occupyingSlotIndex, WeaponItemData dataToInit, AudioEmitter weaponAudioEmitter)
    {
        base.InitWeapon(occupyingSlotIndex, dataToInit, weaponAudioEmitter);

        if (!shellEjectionParticleEffect)
            return;

        if (cachedParticleEffect == null || cachedParticleEffect.Length == 0)
            cachedParticleEffect = shellEjectionParticleEffect.GetComponentsInChildren<ParticleSystem>();

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

    private void Shoot()
    {
        weaponAnimator.Play("Fire");
        muzzleFX.Play();
        EjectCartridge();

        weaponAudioEmitter.ForcePlay(GetRandomClipFromArray(weaponItemData.attackSFX), weaponItemData.attackSFXVolume);
        
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
                    if(RollForHit())
                    {
                        int damage = CalculateDamage();
                        bool isCrit = RollForCrit();
                        if (isCrit)
                            damage *= Mathf.CeilToInt(weaponItemData.critDamageMultiplier);

                        damageable.TryDamage(damage, isCrit);
                    }
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
        for (int i = 0; i < weaponItemData.burstLength + PlayerWeaponManager.bonusBurstCount; i++)
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

    public void EjectCartridge()
    {
        if (!shellEjectionParticleEffect)
            return;

        ParticleSystem.MainModule mainModule = shellEjectionParticleEffect.main;
        mainModule.startSpeed = Random.Range(ejectionSpeed.x, ejectionSpeed.y);
        mainModule.startDelay = ejectionStartDelay;

        if (cachedParticleEffect.Length > 0)
        {
            for (int i = 0, l = cachedParticleEffect.Length; i < l; i++)
            {
                ParticleSystem.MainModule childrenModule = cachedParticleEffect[i].main;
                childrenModule.startDelay = ejectionStartDelay;
            }
        }

        shellEjectionParticleEffect.Play();
    }
}
