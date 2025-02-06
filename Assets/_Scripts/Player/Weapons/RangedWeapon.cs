using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class RangedWeapon : Weapon
{
    Transform projectileSpawnLocation;
    Coroutine burstCoroutine;
    bool isReloading;
    bool canShootBurst = true;
    bool canShootBurstShot = true;
    int loadedAmmo, reserveAmmo;

    public bool infinteAmmo = false;

    [SerializeField] ParticleSystem muzzleFX;
    [SerializeField] ParticleSystem shellEjectionParticleEffect;
    [SerializeField] Vector2 ejectionSpeed = new Vector2(1, 3);
    [SerializeField] float ejectionStartDelay;
    ParticleSystem[] cachedParticleEffect;

    [Header("Magazine Dropping")]
    [SerializeField] Transform magDropTransform;
    [SerializeField] int maxDroppedMags = 5;
    [SerializeField] int lastDroppedMag;
    List<GameObject> droppedMagList = new List<GameObject>();

    public static Action<WeaponItemData> onRangedWeaponFired;

    private void Start()
    {
        projectileSpawnLocation = GameObject.FindGameObjectWithTag("ProjectileSpawnLocation").transform;
    }

    public override bool CanUse()
    {
        return base.CanUse() && !IsReloading();
    }

    public override void InitWeapon(WeaponSlot occupyingSlot, WeaponItemData dataToInit, AudioEmitter _weaponAudioEmitter, IInventory playerInventory)
    {
        base.InitWeapon(occupyingSlot, dataToInit, _weaponAudioEmitter, playerInventory);

        if (!shellEjectionParticleEffect)
            return;

        if (cachedParticleEffect == null || cachedParticleEffect.Length == 0)
            cachedParticleEffect = shellEjectionParticleEffect.GetComponentsInChildren<ParticleSystem>();

        reserveAmmo = GetReserveAmmo();
        onAmmoUpdated?.Invoke(base.occupyingSlot.GetSlotIndex(), loadedAmmo, reserveAmmo);

    }

    public override Task DrawWeapon()
    {
        onAmmoUpdated?.Invoke(occupyingSlot.GetSlotIndex(), loadedAmmo, GetReserveAmmo());
        canShootBurst = true;
        return base.DrawWeapon();
    }
    public override void UseWeapon()
    {
        if (!CanUse())
            return;

        if(loadedAmmo > 0 || infinteAmmo)
        {
            base.UseWeapon();
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
        Vector2 randomPoint = new Vector2(Random.Range(-weaponItemData.recoilData.weaponSpread, weaponItemData.recoilData.weaponSpread), Random.Range(-weaponItemData.recoilData.weaponSpread, weaponItemData.recoilData.weaponSpread));
        return new Vector3(randomPoint.x, randomPoint.y, 1);
    }
    private void Shoot()
    {
        weaponAnimator.CrossFadeInFixedTime("Fire", .025f);
        muzzleFX.Play();
        EjectCartridge();

        weaponAudioEmitter.ForcePlay(GetRandomClipFromArray(weaponItemData.attackSFX), weaponItemData.attackSFXVolume);

        if (!infinteAmmo)
            SetLoadedAmmo(loadedAmmo - 1);

        onRangedWeaponFired?.Invoke(weaponItemData);
        onAmmoUpdated?.Invoke(occupyingSlot.GetSlotIndex(), loadedAmmo, reserveAmmo);

        RaycastHit hit;
        for (int i = 0; i < weaponItemData.projectileCount; i++)
        {
            Vector3 origin = projectileSpawnLocation.position;
            Vector3 direction = projectileSpawnLocation.TransformDirection(GetBulletSpread());

            Ray ray = new Ray(origin, direction);
            if (Physics.Raycast(ray, out hit, weaponItemData.itemRange * 3))
            {
                Debug.DrawRay(ray.origin, ray.direction * Vector3.Distance(ray.origin, hit.point), Color.yellow, 10);
                if(hit.transform.TryGetComponent(out ShootableTarget target))
                {
                    target.Interact();
                    return;
                }

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
    public bool IsReloading() => isReloading;
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

        int remainingAmmo = playerInventory.GetRemainingAmmoOfType(weaponItemData.ammoType);
        if (remainingAmmo == 0)
            return;

        playerInventory.LockSlotsWithAmmoOfType(weaponItemData.ammoType);
        if (!weaponItemData.bulletByBulletReload)
        {
            playerInventory.IncreaseAmmoOfType(weaponItemData.ammoType, loadedAmmo);
            SetLoadedAmmo(0);
            onAmmoUpdated?.Invoke(occupyingSlot.GetSlotIndex(), loadedAmmo, GetReserveAmmo());

            DropMagazine(transform.root.GetComponent<Collider>());
        }
        remainingAmmo = playerInventory.GetRemainingAmmoOfType(weaponItemData.ammoType);

        int amountToReload = 0;
        if (remainingAmmo >= weaponItemData.magSize)
        {
            amountToReload = weaponItemData.magSize;
        }
        else if (remainingAmmo < weaponItemData.magSize)
        {
            amountToReload = remainingAmmo;
        }


        await PerformReloadAnim(amountToReload);

        playerInventory.UnlockSlots();
    }
    public async Task PerformReloadAnim(int reloadAmount)
    {
        if (weaponItemData.bulletByBulletReload)
        {
            await BulletByBulletReload();
            return;
        }

        isReloading = true;
        weaponAnimator.Play("Reload");
        weaponAudioEmitter.ForcePlay(weaponItemData.reloadSFX, weaponItemData.reloadVolume);
        await Task.Delay((int)(weaponItemData.reloadAnimDuration * 1000));
        isReloading = false;
        SetLoadedAmmo(reloadAmount);
        playerInventory.DecreaseAmmoOfType(weaponItemData.ammoType, reloadAmount);
        onAmmoUpdated?.Invoke(occupyingSlot.GetSlotIndex(), loadedAmmo, GetReserveAmmo());
    }
    private async Task BulletByBulletReload()
    {
        isReloading = true;
        if (loadedAmmo == 0)
        {
            weaponAnimator.Play("InsertInChamber");
            weaponAudioEmitter.ForcePlay(weaponItemData.reloadInsertInChamberSFX, weaponItemData.reloadInsertInChamberVolume);
            await Task.Delay((int)(weaponItemData.reloadInsertInChamberAnimDuration * 1000));
            SetLoadedAmmo(loadedAmmo + 1);
            playerInventory.DecreaseAmmoOfType(weaponItemData.ammoType, 1);
            onAmmoUpdated?.Invoke(occupyingSlot.GetSlotIndex(), loadedAmmo, GetReserveAmmo());
        }
        else
        {
            weaponAnimator.Play("StartReload");
            weaponAudioEmitter.ForcePlay(weaponItemData.reloadStartSFX, weaponItemData.reloadStartVolume);
            await Task.Delay((int)(weaponItemData.reloadStartAnimDuration * 1000));
        }

        while (loadedAmmo < weaponItemData.magSize && reserveAmmo > 0)
        {
            //weaponAnimator.Play("Insert");
            weaponAnimator.CrossFadeInFixedTime("Insert", .1f);
            weaponAudioEmitter.ForcePlay(weaponItemData.reloadInsertSFX, weaponItemData.reloadInsertVolume);
            await Task.Delay((int)(weaponItemData.reloadInsertAnimDuration * 1000));
            SetLoadedAmmo(loadedAmmo + 1);
            playerInventory.DecreaseAmmoOfType(weaponItemData.ammoType, 1);
            onAmmoUpdated?.Invoke(occupyingSlot.GetSlotIndex(), loadedAmmo, GetReserveAmmo());
        }

        weaponAnimator.Play("StopReload");
        weaponAudioEmitter.ForcePlay(weaponItemData.reloadStopSFX, weaponItemData.reloadStopVolume);
        await Task.Delay((int)(weaponItemData.reloadEndAnimDuration * 1000));
        isReloading = false;
        return;
    }
    public virtual int GetLoadedAmmo()
    {
        return loadedAmmo;
    }
    public void SetLoadedAmmo(int loadedAmmo)
    {
        this.loadedAmmo = loadedAmmo;
        occupyingSlot.SetItemStackLoadedAmmo(loadedAmmo);
    }
    public int GetReserveAmmo()
    {
        return playerInventory.GetRemainingAmmoOfType(weaponItemData.ammoType);
    }
    public void UpdateReserveAmmo()
    {
        reserveAmmo = GetReserveAmmo();
        onAmmoUpdated?.Invoke(occupyingSlot.GetSlotIndex(), loadedAmmo, reserveAmmo);
    }
    public override MeleeWeapon GetMeleeWeapon()
    {
        return null;
    }
    public override RangedWeapon GetRangedWeapon()
    {
        return this;
    }
    public override IEnumerator UseCooldown()
    {
        canShootBurst = true;
        return base.UseCooldown();
    }
}
