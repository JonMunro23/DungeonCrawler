using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
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
    bool isPlayerMoving = false;
    bool isLoadingNewAmmoType = false;

    public bool infinteAmmo = false;
    public bool isWeaponReady;

    [SerializeField] float bulletSpreadMultiplier;

    [SerializeField] ParticleSystem muzzleFX;
    [SerializeField] ParticleSystem shellEjectionParticleEffect;
    [SerializeField] Vector2 ejectionSpeed = new Vector2(1, 3);
    ParticleSystem[] cachedParticleEffect;
    Tween bulletSpreadTween;
    Tween weaponReadyTween;


    [Header("Ammo")]
    [SerializeField] AmmoItemData currentLoadedAmmoData;
    [SerializeField] int loadedAmmo, reserveAmmo;

    [Header("Magazine Dropping")]
    [SerializeField] Transform magDropTransform;
    [SerializeField] int maxDroppedMags = 5;
    [SerializeField] int lastDroppedMag;
    List<GameObject> droppedMagList = new List<GameObject>();

    /// <summary>
    /// In order:
    /// int = occupiedSlotIndex
    /// int = loadedAmmo
    /// int = reserveAmmo
    /// </summary>
    public static event Action<int, int> onLoadedAmmoUpdated;
    public static event Action<int, int> onReserveAmmoUpdated;
    public static event Action<WeaponItemData> onRangedWeaponFired;
    public static event Action<bool> onRangedWeaponReadied;
    public static event Action<int, WeaponItem> onNewAmmoTypeLoaded;

    private void OnEnable()
    {
        PlayerMovementManager.onPlayerMoveStarted += OnPlayerMoveStarted;
        PlayerMovementManager.onPlayerMoveEnded += OnPlayerMoveEnded;
    }

    private void OnDisable()
    {
        PlayerMovementManager.onPlayerMoveStarted -= OnPlayerMoveStarted;
        PlayerMovementManager.onPlayerMoveEnded -= OnPlayerMoveEnded;
    }

    void OnPlayerMoveStarted()
    {
        isPlayerMoving = true;
        IncreaseBulletSpreadMultiplierOverTime(weaponItemData.maxWeaponSpreadAmount, .57f);
    }

    void OnPlayerMoveEnded()
    {
        isPlayerMoving = false;
    }

    private void Start()
    {
        projectileSpawnLocation = Camera.main.transform;
    }

    private void Update()
    {
        if (isPlayerMoving) return;

        if (bulletSpreadMultiplier <= weaponItemData.minWeaponSpreadAmount) return;
        //maybe wait a fraction of a second before reducing spread amount?
        bulletSpreadMultiplier -= weaponItemData.spreadReductionSpeed * Time.deltaTime;
        bulletSpreadMultiplier = Mathf.Clamp(bulletSpreadMultiplier, weaponItemData.minWeaponSpreadAmount, weaponItemData.maxWeaponSpreadAmount);
    }

    public override bool CanUse()
    {
        return base.CanUse() && !IsReloading();
    }

    public override void InitWeapon(WeaponSlot occupyingSlot, WeaponItem weaponToInit, AudioEmitter _weaponAudioEmitter, IInventory playerInventory)
    {
        base.InitWeapon(occupyingSlot, weaponToInit, _weaponAudioEmitter, playerInventory);

        if (!shellEjectionParticleEffect)
            return;

        if (cachedParticleEffect == null || cachedParticleEffect.Length == 0)
            cachedParticleEffect = shellEjectionParticleEffect.GetComponentsInChildren<ParticleSystem>();

        currentLoadedAmmoData = weaponToInit.LoadedAmmoData;

        //UpdateReserveAmmo();
    }

    public override IEnumerator DrawWeapon()
    {
        UpdateReserveAmmo();
        canShootBurst = true;
        return base.DrawWeapon();
    }

    public override void UseWeapon()
    {
        if (!CanUse())
            return;

        if (loadedAmmo > 0 || infinteAmmo)
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
        Vector2 randomPoint = new Vector2(
            Random.Range(-weaponItemData.recoilData.weaponSpread, weaponItemData.recoilData.weaponSpread),
            Random.Range(-weaponItemData.recoilData.weaponSpread, weaponItemData.recoilData.weaponSpread)
        );

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
        IncreaseBulletSpreadMultiplierinstantly(weaponItemData.perShotSpreadIncrease);

        HashSet<IDamageable> statusEffectAppliedTargets = null;
        bool shouldApplyAmmoStatusEffect =
            currentLoadedAmmoData != null &&
            currentLoadedAmmoData.ammoStatusEffect != null &&
            (currentLoadedAmmoData.ammoType == AmmoType.Incendiary || currentLoadedAmmoData.ammoType == AmmoType.Acid);

        if (shouldApplyAmmoStatusEffect)
            statusEffectAppliedTargets = new HashSet<IDamageable>();

        RaycastHit hit;
        for (int i = 0; i < weaponItemData.projectileCount; i++)
        {
            Vector3 origin = projectileSpawnLocation.position;
            Vector3 direction = projectileSpawnLocation.TransformDirection(GetBulletSpread());

            Ray ray = new Ray(origin, direction);
            if (Physics.Raycast(ray, out hit, weaponItemData.itemRange * 3))
            {
                if (hit.transform.TryGetComponent(out ShootableTarget target))
                {
                    target.Interact();
                    continue;
                }

                IDamageable damageable = hit.transform.GetComponentInParent<IDamageable>();
                if (damageable != null)
                {
                    int damage = 0;
                    int armourRating = damageable.GetDamageData().currentArmourRating;
                    bool isCrit = hit.transform.CompareTag("CritZone");

                    switch (currentLoadedAmmoData.ammoType)
                    {
                        case AmmoType.Standard:
                            damage = CalculateDamage(armourRating);
                            damageable.TryDamage(damage, DamageType.Physical, isCrit);
                            break;

                        case AmmoType.ArmourPiercing:
                            int reducedAR = Mathf.RoundToInt(armourRating * .5f);
                            damage = CalculateDamage(reducedAR);
                            damageable.TryDamage(damage, DamageType.Physical, isCrit);
                            break;

                        case AmmoType.HollowPoint:
                            // more damage to unarmoured targets but reduced against armour
                            break;

                        case AmmoType.Incendiary:
                            damage = CalculateDamage(armourRating);
                            damageable.TryDamage(damage, DamageType.Fire, isCrit);
                            break;

                        case AmmoType.Acid:
                            damage = CalculateDamage(armourRating);
                            damageable.TryDamage(damage, DamageType.Acid, isCrit);
                            break;
                    }

                    if (shouldApplyAmmoStatusEffect && statusEffectAppliedTargets != null)
                    {
                        if (!statusEffectAppliedTargets.Contains(damageable))
                        {
                            statusEffectAppliedTargets.Add(damageable);
                            damageable.AddStatusEffect(currentLoadedAmmoData.ammoStatusEffect);
                        }
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
        if (IsReloading() || isReadyingWeapon || isWeaponReady || IsMeleeWeapon())
            return;

        isReadyingWeapon = true;
        weaponReadyTween = transform.DOLocalRotate(new Vector3(0, 90, 0), weaponItemData.readyAnimDuration).OnComplete(() =>
        {
            isReadyingWeapon = false;
            isWeaponReady = true;
            onRangedWeaponReadied?.Invoke(isWeaponReady);
            IncreaseBulletSpreadMultiplierinstantly(weaponItemData.maxWeaponSpreadAmount);
        });
    }

    public void UnreadyWeapon()
    {
        isWeaponReady = false;
        isReadyingWeapon = false;
        weaponReadyTween?.Kill();
        onRangedWeaponReadied?.Invoke(isWeaponReady);
        transform.DOLocalRotate(new Vector3(0, 90, 15), weaponItemData.readyAnimDuration);
    }

    private void IncreaseBulletSpreadMultiplierOverTime(float increaseAmount, float timeToIncrease)
    {
        bulletSpreadTween?.Kill();

        float endMultiplier = bulletSpreadMultiplier + increaseAmount;
        endMultiplier = Mathf.Clamp(endMultiplier, weaponItemData.minWeaponSpreadAmount, weaponItemData.maxWeaponSpreadAmount);

        bulletSpreadTween = DOTween.To(() => bulletSpreadMultiplier, x => bulletSpreadMultiplier = x, endMultiplier, timeToIncrease)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                bulletSpreadMultiplier = endMultiplier;
            });

        bulletSpreadMultiplier = Mathf.Clamp(bulletSpreadMultiplier, weaponItemData.minWeaponSpreadAmount, weaponItemData.maxWeaponSpreadAmount);
    }

    private void IncreaseBulletSpreadMultiplierinstantly(float increaseAmount)
    {
        bulletSpreadMultiplier += increaseAmount;
        bulletSpreadMultiplier = Mathf.Clamp(bulletSpreadMultiplier, weaponItemData.minWeaponSpreadAmount, weaponItemData.maxWeaponSpreadAmount);
    }



    public IEnumerator TryReload(AmmoItemData newAmmoTypeToLoad)
    {
        if (isReloading || !isWeaponDrawn)
            yield break;

        if (loadedAmmo == weaponItemData.magSize && (newAmmoTypeToLoad == null || newAmmoTypeToLoad == currentLoadedAmmoData))
            yield break;


        //AmmoItemData oldAmmoType = null;
        if (newAmmoTypeToLoad != null)
        {
            //oldAmmoType = currentLoadedAmmoData;
            //currentLoadedAmmoData = newAmmoTypeToLoad;
            isLoadingNewAmmoType = true;
        }
        AmmoItemData ammoTypeToDealWith = isLoadingNewAmmoType ? newAmmoTypeToLoad : currentLoadedAmmoData;

        //inital fetch to see if we have any reserve ammo and if not cancel reload
        int heldAmmo = GetRemainingAmmoOfType(ammoTypeToDealWith);
        if (heldAmmo == 0)
            yield break;

        playerInventory.LockSlotsWithAmmoOfType(ammoTypeToDealWith);

        if (!weaponItemData.bulletByBulletReload)
        {
            playerInventory.IncreaseAmmoOfType(currentLoadedAmmoData, loadedAmmo);

            // fetch again after yield loaded ammo to pool to get final amount
            heldAmmo = GetRemainingAmmoOfType(ammoTypeToDealWith);

            UpdateLoadedAmmo(0);
            UpdateReserveAmmo();

            DropMagazine(transform.root.GetComponent<Collider>());
        }
        else if (weaponItemData.bulletByBulletReload)
        {
            if (ammoTypeToDealWith != currentLoadedAmmoData && loadedAmmo > 0)
                yield return EjectLoadedShells(currentLoadedAmmoData);
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

        currentLoadedAmmoData = ammoTypeToDealWith;

        yield return PerformReloadAnim(amountToReload);

        isLoadingNewAmmoType = false;

        playerInventory.UnlockSlots();
    }

    IEnumerator PerformReloadAnim(int reloadAmount)
    {
        if (weaponItemData.bulletByBulletReload)
        {
            yield return BulletByBulletReload();
            yield break;
        }

        isReloading = true;
        weaponAnimator.Play("Reload");
        weaponAudioEmitter.ForcePlay(weaponItemData.reloadSFX, weaponItemData.reloadVolume);
        yield return new WaitForSeconds(weaponItemData.reloadAnimDuration);
        isReloading = false;
        //Debug.Log("reload amount = " + reloadAmount);
        UpdateLoadedAmmo(reloadAmount);
        playerInventory.DecreaseAmmoOfType(currentLoadedAmmoData, reloadAmount);
        UpdateReserveAmmo();

        if (isLoadingNewAmmoType)
            onNewAmmoTypeLoaded?.Invoke(occupyingSlot.GetSlotIndex(), weaponItem);
    }

    IEnumerator BulletByBulletReload()
    {
        //Debug.Log("Starting bullet by bullet reload...");
        isReloading = true;
        if (loadedAmmo == 0)
        {
            //Debug.Log("Inserting into chamber...");
            weaponAnimator.Play("InsertInChamber");
            weaponAudioEmitter.ForcePlay(weaponItemData.reloadInsertInChamberSFX, weaponItemData.reloadInsertInChamberVolume);
            yield return new WaitForSeconds(weaponItemData.reloadInsertInChamberAnimDuration);
            UpdateLoadedAmmo(loadedAmmo + 1);
            playerInventory.DecreaseAmmoOfType(currentLoadedAmmoData, 1);
            UpdateReserveAmmo();

            if (isLoadingNewAmmoType)
                onNewAmmoTypeLoaded?.Invoke(occupyingSlot.GetSlotIndex(), weaponItem);
        }
        else
        {
            //Debug.Log("Starting reload...");
            weaponAnimator.Play("StartReload");
            weaponAudioEmitter.ForcePlay(weaponItemData.reloadStartSFX, weaponItemData.reloadStartVolume);
            yield return new WaitForSeconds(weaponItemData.reloadStartAnimDuration);
        }

        while (loadedAmmo < weaponItemData.magSize && reserveAmmo > 0)
        {
            //Debug.Log($"Inserting round {loadedAmmo}...");
            weaponAnimator.CrossFadeInFixedTime("Insert", .1f);
            weaponAudioEmitter.ForcePlay(weaponItemData.reloadInsertSFX, weaponItemData.reloadInsertVolume);
            yield return new WaitForSeconds(weaponItemData.reloadInsertAnimDuration);
            UpdateLoadedAmmo(loadedAmmo + 1);
            playerInventory.DecreaseAmmoOfType(currentLoadedAmmoData, 1);
            UpdateReserveAmmo();
        }

        //Debug.Log("Stopping reload...");
        weaponAnimator.Play("StopReload");
        weaponAudioEmitter.ForcePlay(weaponItemData.reloadStopSFX, weaponItemData.reloadStopVolume);
        yield return new WaitForSeconds(weaponItemData.reloadEndAnimDuration);
        isReloading = false;
    }

    IEnumerator EjectLoadedShells(AmmoItemData ammoToEject)
    {
        while (loadedAmmo != 0)
        {
            weaponAnimator.Play("Pump");
            weaponAudioEmitter.ForcePlay(weaponItemData.ejectShellSFX, weaponItemData.ejectShellVolume);
            yield return new WaitForSeconds(weaponItemData.ejectShellAnimDuration);
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
        occupyingSlot.SetLoadedAmmoType(currentLoadedAmmoData);
    }

    public AmmoItemData GetCurrentLoadedAmmoData()
    {
        return currentLoadedAmmoData;
    }

    public List<AmmoItemData> GetAllUseableHeldAmmo()
    {
        return playerInventory.GetAllUseableAmmoTypesForWeapon(this);
    }

    public void UpdateLoadedAmmo(int loadedAmmo)
    {
        this.loadedAmmo = loadedAmmo;
        occupyingSlot.SetLoadedAmmo(this.loadedAmmo);
        occupyingSlot.SetLoadedAmmoType(currentLoadedAmmoData);
        onLoadedAmmoUpdated?.Invoke(occupyingSlot.GetSlotIndex(), this.loadedAmmo);
    }

    public void UpdateReserveAmmo()
    {
        reserveAmmo = GetReserveAmmoOfCurrentType();
        onReserveAmmoUpdated?.Invoke(occupyingSlot.GetSlotIndex(), reserveAmmo);
    }

    public int GetReserveAmmoOfCurrentType()
    {
        return GetRemainingAmmoOfType(currentLoadedAmmoData);
    }

    public int GetRemainingAmmoOfType(AmmoItemData typeToCheck)
    {
        return playerInventory.TryGetRemainingAmmoOfType(typeToCheck);
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
