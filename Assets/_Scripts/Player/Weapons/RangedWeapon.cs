using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class RangedWeapon : Weapon
{
    Transform projectileSpawnLocation;
    Coroutine burstCoroutine;
    bool isReloading;
    bool canShootBurst = true;
    bool canShootBurstShot = true;
    bool isReadyingWeapon;

    public bool infinteAmmo = false;
    public bool isWeaponReady;

    [SerializeField] float bulletSpreadMultiplier;

    [SerializeField] ParticleSystem muzzleFX;
    [SerializeField] ParticleSystem shellEjectionParticleEffect;
    [SerializeField] Vector2 ejectionSpeed = new Vector2(1, 3);
    ParticleSystem[] cachedParticleEffect;

    [Header("Ammo")]
    [SerializeField] AmmoItemData currentLoadedAmmoData;
    [SerializeField] int loadedAmmo, reserveAmmo;


    [Header("Magazine Dropping")]
    [SerializeField] Transform magDropTransform;
    [SerializeField] int maxDroppedMags = 5;
    [SerializeField] int lastDroppedMag;
    List<GameObject> droppedMagList = new List<GameObject>();

    public static Action<WeaponItemData> onRangedWeaponFired;
    public static Action<bool> onRangedWeaponReadied;

    private void Start()
    {
        projectileSpawnLocation = Camera.main.transform;
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

        currentLoadedAmmoData = dataToInit.defaultLoadedAmmoData;

        UpdateReserveAmmo();
    }

    public override Task DrawWeapon()
    {
        UpdateReserveAmmo();
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
                //GameObject projectile = Instantiate(weaponItemData.projectileData.projModel, projectileSpawnLocation.position, projectileSpawnLocation.rotation);
                ////projectile.GetComponentInChildren<Projectile>().projectile = handItemData.itemProjectile;
                //projectile.GetComponentInChildren<Projectile>().damage = CalculateDamage();
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
        //// Clamp the spreadMultiplier to be between 0 and 1
        //bulletSpreadMultiplier = Mathf.Clamp01(bulletSpreadMultiplier);

        // Calculate the random spread offset
        Vector2 randomPoint = new Vector2(
            Random.Range(-weaponItemData.recoilData.weaponSpread, weaponItemData.recoilData.weaponSpread),
            Random.Range(-weaponItemData.recoilData.weaponSpread, weaponItemData.recoilData.weaponSpread)
        );

        // Apply the multiplier to the random spread
        randomPoint *= bulletSpreadMultiplier;

        return new Vector3(randomPoint.x, randomPoint.y, 1);
    }
    public float GetBulletSpreadMultiplier()
    {
        return bulletSpreadMultiplier;
    }

    private void Shoot()
    {
        weaponAnimator.CrossFadeInFixedTime("Fire", .025f);
        muzzleFX.Play();
        EjectCartridge(.65f);

        weaponAudioEmitter.ForcePlay(GetRandomClipFromArray(weaponItemData.attackSFX), weaponItemData.attackSFXVolume);

        if (!infinteAmmo)
            UpdateLoadedAmmo(loadedAmmo - 1);

        onRangedWeaponFired?.Invoke(weaponItemData);
        IncreaseBulletSpreadMultiplier(weaponItemData.perShotSpreadIncrease);

        RaycastHit hit;
        for (int i = 0; i < weaponItemData.projectileCount; i++)
        {
            Vector3 origin = projectileSpawnLocation.position;
            Vector3 direction = projectileSpawnLocation.TransformDirection(GetBulletSpread());

            Ray ray = new Ray(origin, direction);
            if (Physics.Raycast(ray, out hit, weaponItemData.itemRange * 3))
            {
                //Debug.DrawRay(ray.origin, ray.direction * Vector3.Distance(ray.origin, hit.point), Color.yellow, 10);
                if(hit.transform.TryGetComponent(out ShootableTarget target))
                {
                    target.Interact();
                    return;
                }

                IDamageable damageable = hit.transform.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    int damage = 0;
                    int AR = damageable.GetDamageData().currentArmourRating;
                    //bool isCrit = RollForCrit();
                    //bool wasHit = RollForHit();

                    switch (currentLoadedAmmoData.ammoType)
                    {
                        case AmmoType.Standard:
                            damage = CalculateDamage(AR);
                            //if (isCrit)
                            //    damage *= Mathf.CeilToInt(weaponItemData.critDamageMultiplier);
                            damageable.TryDamage(damage, DamageType.Standard);
                            break;
                        case AmmoType.ArmourPiercing:
                            int reducedAR = Mathf.RoundToInt(AR * .5f);
                            damage = CalculateDamage(reducedAR);
                            //if (isCrit)
                            //    damage *= Mathf.CeilToInt(weaponItemData.critDamageMultiplier);
                            damageable.TryDamage(damage, DamageType.Standard);
                            break;
                        case AmmoType.HollowPoint:
                            //more damage to unarmoured targets but reduced against armour
                            break;
                        case AmmoType.Incendiary:
                            damage = CalculateDamage(AR);
                            //if (isCrit)
                            //    damage *= Mathf.CeilToInt(weaponItemData.critDamageMultiplier);
                            damageable.TryDamage(damage, DamageType.Fire);
                            damageable.AddStatusEffect(StatusEffectType.Fire);
                            break;
                        case AmmoType.Acid:
                            damage = CalculateDamage(AR);
                            //if (isCrit)
                            //    damage *= Mathf.CeilToInt(weaponItemData.critDamageMultiplier);
                            damageable.TryDamage(damage, DamageType.Acid);
                            damageable.AddStatusEffect(StatusEffectType.Acid);
                            break;

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
        for (int i = 0; i < GetBurstCount(); i++)
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
    public void EjectCartridge(float delayBeforeEjection)
    {
        if (!shellEjectionParticleEffect)
            return;

        ParticleSystem.MainModule mainModule = shellEjectionParticleEffect.main;
        mainModule.startSpeed = Random.Range(ejectionSpeed.x, ejectionSpeed.y);
        mainModule.startDelay = delayBeforeEjection;

        if (cachedParticleEffect.Length > 0)
        {
            for (int i = 0, l = cachedParticleEffect.Length; i < l; i++)
            {
                ParticleSystem.MainModule childrenModule = cachedParticleEffect[i].main;
                childrenModule.startDelay = delayBeforeEjection;
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

    public void ReadyWeapon()
    {
        if (isReadyingWeapon || isWeaponReady || IsMeleeWeapon())
            return;

        isReadyingWeapon = true;
        transform.DOLocalRotate(new Vector3(0, 90, 0), weaponItemData.readyAnimDuration).OnComplete(() =>
        {
            isReadyingWeapon = false;
            isWeaponReady = true;
            onRangedWeaponReadied?.Invoke(isWeaponReady);
            IncreaseBulletSpreadMultiplier(weaponItemData.onWeaponReadiedSpreadAmount);
        });
        //await Task.Delay((int)(weaponItemData.readyAnimDuration * 1000));
    }


    private Tween bulletSpreadTween;
    private void IncreaseBulletSpreadMultiplier(float increaseAmount)
    {
        bulletSpreadMultiplier += increaseAmount;

        bulletSpreadTween?.Kill();

        bulletSpreadTween = DOTween.To(() => bulletSpreadMultiplier, x => bulletSpreadMultiplier = x, 0f, weaponItemData.spreadReductionSpeed)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                bulletSpreadMultiplier = 0;
            });
    }

    public void UnreadyWeapon()
    {
        if (IsMeleeWeapon())
            return;

        isWeaponReady = false;
        onRangedWeaponReadied?.Invoke(isWeaponReady);
        transform.DOLocalRotate(new Vector3(0, 90, 15), weaponItemData.readyAnimDuration);
        //await Task.Delay((int)(weaponItemData.readyAnimDuration * 1000));
    }

    public async Task TryReload(AmmoItemData newAmmoTypeToLoad)
    {
        if (isReloading || !isWeaponDrawn)
            return;

        if (loadedAmmo == weaponItemData.magSize && (newAmmoTypeToLoad == null || newAmmoTypeToLoad == currentLoadedAmmoData))
            return;

        AmmoItemData oldAmmoType = null;
        if (newAmmoTypeToLoad != null)
        {
            oldAmmoType = currentLoadedAmmoData;
            currentLoadedAmmoData = newAmmoTypeToLoad;
        }

        int heldAmmo = playerInventory.TryGetRemainingAmmoOfType(currentLoadedAmmoData);
        if (heldAmmo == 0)
            return;

        playerInventory.LockSlotsWithAmmoOfType(currentLoadedAmmoData);
        if (!weaponItemData.bulletByBulletReload)
        {
            if(oldAmmoType != null)
                playerInventory.IncreaseAmmoOfType(oldAmmoType, loadedAmmo);
            else
                playerInventory.IncreaseAmmoOfType(currentLoadedAmmoData, loadedAmmo);
            UpdateLoadedAmmo(0);
            UpdateReserveAmmo();

            DropMagazine(transform.root.GetComponent<Collider>());
        }
        else if(weaponItemData.bulletByBulletReload)
        {
            if(oldAmmoType != newAmmoTypeToLoad && loadedAmmo > 0)
                await EjectLoadedShells(oldAmmoType);
        }

        int amountToReload = 0;
        if (heldAmmo >= weaponItemData.magSize)
        {
            amountToReload = weaponItemData.magSize;
        }
        else if (heldAmmo < weaponItemData.magSize)
        {
            amountToReload = heldAmmo;
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
        UpdateLoadedAmmo(reloadAmount);
        playerInventory.DecreaseAmmoOfType(currentLoadedAmmoData, reloadAmount);
        UpdateReserveAmmo();
    }
    private async Task BulletByBulletReload()
    {
        isReloading = true;
        if (loadedAmmo == 0)
        {
            weaponAnimator.Play("InsertInChamber");
            weaponAudioEmitter.ForcePlay(weaponItemData.reloadInsertInChamberSFX, weaponItemData.reloadInsertInChamberVolume);
            await Task.Delay((int)(weaponItemData.reloadInsertInChamberAnimDuration * 1000));
            UpdateLoadedAmmo(loadedAmmo + 1);
            playerInventory.DecreaseAmmoOfType(currentLoadedAmmoData, 1);
            UpdateReserveAmmo();
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
            UpdateLoadedAmmo(loadedAmmo + 1);
            playerInventory.DecreaseAmmoOfType(currentLoadedAmmoData, 1);
            UpdateReserveAmmo();
        }

        weaponAnimator.Play("StopReload");
        weaponAudioEmitter.ForcePlay(weaponItemData.reloadStopSFX, weaponItemData.reloadStopVolume);
        await Task.Delay((int)(weaponItemData.reloadEndAnimDuration * 1000));
        isReloading = false;
        return;
    }

    async Task EjectLoadedShells(AmmoItemData ammoToEject)
    {
        while(loadedAmmo != 0)
        {
            weaponAnimator.Play("Pump");
            weaponAudioEmitter.ForcePlay(weaponItemData.ejectShellSFX, weaponItemData.ejectShellVolume);
            await Task.Delay((int)(weaponItemData.ejectShellAnimDuration * 1000));
            EjectCartridge(0);
            UpdateLoadedAmmo(loadedAmmo - 1);
            playerInventory.IncreaseAmmoOfType(ammoToEject, 1);
        }
    }

    public virtual int GetLoadedAmmo()
    {
        return loadedAmmo;
    }

    public void SetCurrentLoadedAmmoData(AmmoItemData newAmmoItemData)
    {
        currentLoadedAmmoData = newAmmoItemData;
    }
    public AmmoItemData GetCurrentLoadedAmmoData()
    {
        return currentLoadedAmmoData;
    }

    public List<AmmoItemData> GetAllUseableHeldAmmo()
    {
        return playerInventory.GetAllUseableAmmoForWeapon(this);
    }

    public void UpdateLoadedAmmo(int loadedAmmo)
    {
        this.loadedAmmo = loadedAmmo;
        occupyingSlot.SetItemStackLoadedAmmo(this.loadedAmmo);
        onLoadedAmmoUpdated?.Invoke(occupyingSlot.GetSlotIndex(), this.loadedAmmo);
    }
    public void UpdateReserveAmmo()
    {
        reserveAmmo = GetReserveAmmo();
        onReserveAmmoUpdated?.Invoke(occupyingSlot.GetSlotIndex(), reserveAmmo);
    }
    public int GetReserveAmmo()
    {
        return playerInventory.TryGetRemainingAmmoOfType(currentLoadedAmmoData);
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
    public int GetBurstCount()
    {
        return weaponItemData.burstLength + PlayerWeaponManager.bonusBurstCount;
    }
}
