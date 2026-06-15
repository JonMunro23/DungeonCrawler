using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[System.Serializable]
public struct PlayerWeaponSaveData
{
    public int activeSlotIndex;
    public List<WeaponSlotData> slotData;
}

[System.Serializable]
public class WeaponSlotData
{
    public int slotIndex;

    public WeaponItem heldWeapon;

    public WeaponSlotData(int slotIndex, WeaponItem heldWeapon)
    {
        this.slotIndex = slotIndex;
        this.heldWeapon = heldWeapon;
    }
}

public class PlayerWeaponManager : MonoBehaviour
{
    PlayerController playerController;

    [Header("References")]
    [SerializeField] WeaponSlot slotToSpawn;
    [SerializeField] int numWeaponSlots;
    [SerializeField] WeaponItemData defaultWeaponData;
    WeaponItem defaultWeaponItem;
    [SerializeField] Transform weaponSpawnParent;
    AudioEmitter weaponAudioEmitter;

    public WeaponSlot[] spawnedWeaponSlots;
    [SerializeField] int activeSlotIndex = 0;
    public bool isAmmoSelectionMenuOpen;
    bool isLookingAtTarget;

    [Header("Bonus Weapon Stats")]
    public static int bonusDamage;
    public static int bonusBurstCount;
    public static int bonusCritChance;
    public static int bonusCritMultiplier;
    public static int bonusAccuracy;

    public IWeapon currentWeapon;

    Coroutine removeWeaponFromSlotCoroutine, addWeaponToSlotCoroutine, reloadWeaponCoroutine, swapWeaponInSlotCoroutine;

    public static event Action<WeaponSlot[]> onWeaponSlotsSpawned;
    public static event Action<WeaponSlot> onWeaponSlotSetActive;
    public static event Action<int, WeaponItem> onNewWeaponInitialised;

    public static event Action<IWeapon> onWeaponAmmoSelectionMenuOpened;
    public static event Action onWeaponAmmoSelectionMenuClosed;

    private void OnEnable()
    {
        WeaponSlot.onWeaponAddedToSlot += OnWeaponAddedToSlot;
        WeaponSlot.onWeaponRemovedFromSlot += OnWeaponRemovedFromSlot;
        WeaponSlot.onWeaponSwappedInSlot += OnWeaponSwappedInSlot;

        PlayerInventoryManager.onAmmoAddedToInventory += OnInventoryAmmoUpdated;

        WorldInteractionManager.onLookAtTargetChanged += OnLookAtTargetChanged;

        StatData.onStatUpdated += OnStatUpdated;

        InventoryContextMenu.onInventorySlotWeaponItemEquipped += OnInventorySlotWeaponItemEquipped;
        InventoryContextMenu.onInventorySlotWeaponItemUnequipped += OnInventorySlotWeaponItemUnequipped;

        PauseMenu.onQuit += RemoveWeaponSlots;

        AmmoSelectionButton.OnAmmoSelected += OnNewAmmoTypeSelected;
    }

    private void OnDisable()
    {
        WeaponSlot.onWeaponAddedToSlot -= OnWeaponAddedToSlot;
        WeaponSlot.onWeaponRemovedFromSlot -= OnWeaponRemovedFromSlot;
        WeaponSlot.onWeaponSwappedInSlot -= OnWeaponSwappedInSlot;

        PlayerInventoryManager.onAmmoAddedToInventory -= OnInventoryAmmoUpdated;

        WorldInteractionManager.onLookAtTargetChanged -= OnLookAtTargetChanged;

        StatData.onStatUpdated -= OnStatUpdated;

        InventoryContextMenu.onInventorySlotWeaponItemEquipped -= OnInventorySlotWeaponItemEquipped;
        InventoryContextMenu.onInventorySlotWeaponItemUnequipped -= OnInventorySlotWeaponItemUnequipped;

        PauseMenu.onQuit -= RemoveWeaponSlots;

        AmmoSelectionButton.OnAmmoSelected -= OnNewAmmoTypeSelected;
    }

    private void OnLookAtTargetChanged(LookAtTarget currentLookAtTarget)
    {
        if (currentLookAtTarget == LookAtTarget.None)
            isLookingAtTarget = false;
        else
            isLookingAtTarget = true;
    }

    public virtual void OnStatUpdated(StatData updatedStat)
    {
        switch (updatedStat.stat)
        {
            case CharacterStats.BonusWeaponDamage:
                bonusDamage = Mathf.RoundToInt(updatedStat.GetCurrentStatValue());
                break;
            case CharacterStats.BonusBurstCount:
                bonusBurstCount = Mathf.RoundToInt(updatedStat.GetCurrentStatValue());
                break;
            case CharacterStats.CritChance:
                bonusCritChance = Mathf.RoundToInt(updatedStat.GetCurrentStatValue());
                break;
            case CharacterStats.CritMultiplier:
                bonusCritMultiplier = Mathf.RoundToInt(updatedStat.GetCurrentStatValue());
                break;
            case CharacterStats.WeaponAccuracy:
                bonusAccuracy = Mathf.RoundToInt(updatedStat.GetCurrentStatValue());
                break;
        }
    }

    void OnInventoryAmmoUpdated(AmmoItemData typeAdded)
    {
        if (currentWeapon == null)
            return;

        if (currentWeapon.IsMeleeWeapon())
            return;

        if(currentWeapon.GetRangedWeapon() == null)
            return;

        if (typeAdded.weaponTypes.Contains(currentWeapon.GetWeaponData().weaponType))
        {
            currentWeapon.GetRangedWeapon().UpdateReserveAmmo();
        }

    }

    void OnInventorySlotWeaponItemEquipped(ISlot slot)
    {
        WeaponItemData weaponItemData = slot.GetItemStack().Item.ItemData as WeaponItemData;
        if (weaponItemData)
        {
            foreach (WeaponSlot weaponSlot in spawnedWeaponSlots)
            {
                if (weaponSlot.IsSlotEmpty())
                {
                    weaponSlot.AddItem(slot.TakeItem());
                    return;
                }
            }

            //slot.AddItem(spawnedWeaponSlots[0].SwapItem(slot.GetItemStack()));
            slot.AddItem(spawnedWeaponSlots[0].SwapItem(slot.TakeItem())); // unsure if this breaks anything compared to above

        }
    }

    void OnInventorySlotWeaponItemUnequipped(ISlot slot)
    {
        playerController.playerInventoryManager.TryAddItem(slot.TakeItem());
    }

    void OnNewAmmoTypeSelected(AmmoItemData newAmmoData)
    {
        //Debug.Log($"Switching to {newAmmoData.ammoType} ammo.");
        TryReloadCurrentWeapon(newAmmoData);
    }

    public void Init(PlayerController controller)
    {
        playerController = controller;

        weaponAudioEmitter = AudioManager.Instance.RegisterSource("[AudioEmitter] Weapon", transform, AudioCategory.SFx, 10, 25, 0);

        SpawnWeaponSlots();

        InitialiseDefaultWeapons();

        StartCoroutine(SetSlotActive(activeSlotIndex));
    }

    public void OpenAmmoSelectionMenu()
    {
        if (isAmmoSelectionMenuOpen || 
            playerController.playerWeaponManager.currentWeapon == null ||
            !playerController.playerWeaponManager.currentWeapon.CanUse() || 
            playerController.playerThrowableManager.IsThrowableActive() ||
            playerController.playerWeaponManager.currentWeapon.GetRangedWeapon() == null ||
            playerController.playerWeaponManager.currentWeapon.GetRangedWeapon().GetAllUseableHeldAmmo().Count == 1) return;

        isAmmoSelectionMenuOpen = true;
        onWeaponAmmoSelectionMenuOpened?.Invoke(currentWeapon);
    }

    public void CloseAmmoSelectionMenu()
    {
        if (!isAmmoSelectionMenuOpen) return;

        isAmmoSelectionMenuOpen = false;
        onWeaponAmmoSelectionMenuClosed?.Invoke();
    }

    void SpawnWeaponSlots()
    {
        spawnedWeaponSlots = new WeaponSlot[numWeaponSlots];

        for (int i = 0; i < spawnedWeaponSlots.Length; i++)
        {
            WeaponSlot spawnedSlot = Instantiate(slotToSpawn);
            spawnedWeaponSlots[i] = spawnedSlot;
            spawnedSlot.InitWeaponSlot(i, playerController.playerInventoryManager, weaponAudioEmitter);
        }
        onWeaponSlotsSpawned?.Invoke(spawnedWeaponSlots);
    }

    void RemoveWeaponSlots()
    {
        foreach (WeaponSlot weaponSlot in spawnedWeaponSlots)
        {
            Destroy(weaponSlot.gameObject);
        }
        Array.Clear(spawnedWeaponSlots, 0, numWeaponSlots);
    }

    void InitialiseDefaultWeapons()
    {
        if (defaultWeaponData == null) return;

        if (defaultWeaponData.itemPrefab)
        {
            GameObject spawnedWeapon = Instantiate(defaultWeaponData.itemPrefab, weaponSpawnParent);

            defaultWeaponItem = new WeaponItem(defaultWeaponData, defaultWeaponData.defaultLoadedAmmoData, defaultWeaponData.magSize);

            if (spawnedWeapon.TryGetComponent(out IWeapon weapon))
            {
                for (int i = 0; i < spawnedWeaponSlots.Length; i++)
                {
                    weapon.InitWeapon(spawnedWeaponSlots[i], defaultWeaponItem, weaponAudioEmitter, playerController.playerInventoryManager);
                    spawnedWeaponSlots[i].InitDefaultWeapon(weapon);
                    spawnedWeaponSlots[i].SetWeaponToDefault();
                }
            }
        }
    }

    private IEnumerator SetSlotActive(int slotIndex)
    {
        spawnedWeaponSlots[slotIndex].SetSlotWeaponActive(true);
        currentWeapon = spawnedWeaponSlots[slotIndex].GetWeapon();
        yield return DrawWeaponInSlot(slotIndex);
    }

    /// <summary>
    /// Called when player clicks on a weapon slot. Handles the initialisation of new weapons.
    /// </summary>
    /// <param name="slotIndex">The index of the slot to put the new weapon in.</param>
    /// <param name="newWeaponAddedToSlot">The new weapon add to the slot. </param>
    /// <param name="startingAmmo">The amount of loaded ammo the new weapon will start with</param>
    void OnWeaponAddedToSlot(int slotIndex, WeaponItem newWeaponAddedToSlot)
    {
        //if(addWeaponToSlotCoroutine != null)
        //{
        //    StopCoroutine(addWeaponToSlotCoroutine);
        //    addWeaponToSlotCoroutine = null;
        //}

        //commented out above as it was causing issues with loading, unsure if it currently breaks anything

        addWeaponToSlotCoroutine = StartCoroutine(AddNewWeaponToSlot(slotIndex, newWeaponAddedToSlot));
    }

    void OnWeaponRemovedFromSlot(int slotIndex)
    {
        if(removeWeaponFromSlotCoroutine != null)
        {
            StopCoroutine(removeWeaponFromSlotCoroutine);
            removeWeaponFromSlotCoroutine = null;
        }

        removeWeaponFromSlotCoroutine = StartCoroutine(RemoveWeaponFromSlot(slotIndex));
    }

    void OnWeaponSwappedInSlot(int slotIndex, WeaponItem newWeapon)
    {
        //if(swapWeaponInSlotCoroutine != null)
        //{
        //    StopCoroutine(swapWeaponInSlotCoroutine);
        //    swapWeaponInSlotCoroutine = null;
        //}

        swapWeaponInSlotCoroutine = StartCoroutine(SwapWeaponInSlot(slotIndex, newWeapon));
    }

    IEnumerator SwapWeaponInSlot(int slotIndex, WeaponItem newWeapon)
    {
        spawnedWeaponSlots[slotIndex].SetInteractable(false);
        if (activeSlotIndex == slotIndex)
        {
            yield return HolsterWeaponInSlot(slotIndex);
        }
        if (!spawnedWeaponSlots[slotIndex].GetWeapon().IsDefaultWeapon())
            spawnedWeaponSlots[slotIndex].RemoveWeapon();
        else if (!spawnedWeaponSlots[activeSlotIndex].GetWeapon().IsDefaultWeapon())
            spawnedWeaponSlots[slotIndex].SetSlotWeaponActive(false);

        InitialiseNewWeapon(spawnedWeaponSlots[slotIndex], newWeapon);

        if (activeSlotIndex == slotIndex)
        {
            yield return SetSlotActive(slotIndex);
        }

        spawnedWeaponSlots[slotIndex].SetInteractable(true);

        swapWeaponInSlotCoroutine = null;
    }

    IEnumerator SetSlotToDefault(int slotIndex)
    {
        spawnedWeaponSlots[slotIndex].SetWeaponToDefault();
        if (activeSlotIndex == slotIndex)
        {
            yield return SetSlotActive(slotIndex);
        }
        spawnedWeaponSlots[slotIndex].SetInteractable(true);
    }

    void InitialiseNewWeapon(WeaponSlot occupyingSlot, WeaponItem newWeapon)
    {
        if (newWeapon.WeaponItemData == null) return;

        if (newWeapon.WeaponItemData.itemPrefab)
        {
            GameObject spawnedWeapon = Instantiate(newWeapon.WeaponItemData.itemPrefab, weaponSpawnParent);
            if (spawnedWeapon.TryGetComponent(out IWeapon weapon))
            {
                weapon.InitWeapon(occupyingSlot, newWeapon, weaponAudioEmitter, playerController.playerInventoryManager);
                weapon.SetWeaponActive(false);

                if(weapon.GetRangedWeapon() != null)
                {
                    weapon.GetRangedWeapon().SetCurrentLoadedAmmoData(newWeapon.LoadedAmmoData);
                    weapon.GetRangedWeapon().UpdateLoadedAmmo(newWeapon.LoadedAmmo);
                }

                occupyingSlot.SetWeapon(weapon);

            }

            onNewWeaponInitialised?.Invoke(occupyingSlot.GetSlotIndex(), newWeapon);
        }
    }

    IEnumerator AddNewWeaponToSlot(int slotIndex, WeaponItem newWeapon)
    {
        spawnedWeaponSlots[slotIndex].SetInteractable(false);

        if (!spawnedWeaponSlots[slotIndex].IsSlotEmpty())
        {
            if (activeSlotIndex == slotIndex)
                yield return HolsterWeaponInSlot(slotIndex);

            if (!spawnedWeaponSlots[slotIndex].GetWeapon().IsDefaultWeapon())
            {
                spawnedWeaponSlots[slotIndex].RemoveWeapon();
            }

            if (slotIndex == activeSlotIndex)
            {
                if (spawnedWeaponSlots[activeSlotIndex].GetWeapon().IsDefaultWeapon())
                {
                    spawnedWeaponSlots[activeSlotIndex].SetSlotWeaponActive(false);
                }
            }
        }
        InitialiseNewWeapon(spawnedWeaponSlots[slotIndex], newWeapon);

        if (activeSlotIndex == slotIndex)
        {
            yield return SetSlotActive(slotIndex);
        }

        spawnedWeaponSlots[slotIndex].SetInteractable(true);

        addWeaponToSlotCoroutine = null;
    }

    public IEnumerator HolsterCurrentWeapon()
    {
        yield return HolsterWeaponInSlot(activeSlotIndex);
    }

    public IEnumerator DrawCurrentWeapon()
    {
        yield return DrawWeaponInSlot(activeSlotIndex);
    }

    IEnumerator HolsterWeaponInSlot(int slotIndex)
    {
        if (playerController.playerThrowableManager.IsThrowableActive())
        {
            yield return playerController.playerThrowableManager.HolsterThrowable();
        }
        else
            yield return spawnedWeaponSlots[slotIndex].HolsterWeapon();
    }

    IEnumerator DrawWeaponInSlot(int slotIndex)
    {
        yield return spawnedWeaponSlots[slotIndex].DrawWeapon();
    }

    IEnumerator RemoveWeaponFromSlot(int slotIndex)
    {
        spawnedWeaponSlots[slotIndex].SetInteractable(false);

        if (activeSlotIndex == slotIndex)
        {
            yield return HolsterWeaponInSlot(slotIndex);
        }

        if (!spawnedWeaponSlots[slotIndex].GetWeapon().IsDefaultWeapon())
            spawnedWeaponSlots[slotIndex].RemoveWeapon();
        else if (!spawnedWeaponSlots[activeSlotIndex].GetWeapon().IsDefaultWeapon())
            spawnedWeaponSlots[slotIndex].SetSlotWeaponActive(false);


        yield return SetSlotToDefault(slotIndex);

        spawnedWeaponSlots[slotIndex].SetInteractable(true);

        removeWeaponFromSlotCoroutine = null;
    }

    public IEnumerator SwapWeapons()
    {
        CloseAmmoSelectionMenu();

        // is we have a throwable out, holster it and requip previous weapon
        if (playerController.playerThrowableManager.IsThrowableActive())
        {
            //Debug.Log("Throwable is active, holstering...");
            yield return playerController.playerThrowableManager.HolsterThrowable();
            yield return DrawCurrentWeapon();
            yield break;
        }

        //Debug.Log(currentWeapon.CanUse());
        if (!currentWeapon.CanUse())
            yield break;

        if (activeSlotIndex == 0)
        {
            if (currentWeapon.IsDefaultWeapon() && spawnedWeaponSlots[1].GetWeapon().IsDefaultWeapon())
            {
                yield break;
            }

            activeSlotIndex = 1;
            yield return SetWeaponSlotActive(activeSlotIndex);
        }
        else if (activeSlotIndex == 1)
        {
            if (currentWeapon.IsDefaultWeapon() && spawnedWeaponSlots[0].GetWeapon().IsDefaultWeapon())
            {
                yield break;
            }
            
            activeSlotIndex = 0;
            yield return SetWeaponSlotActive(activeSlotIndex);
        }
    }

    IEnumerator SetWeaponSlotActive(int slotIndex)
    {
        //Debug.Log("Setting Weapon slot " + slotIndex + " active...");
        spawnedWeaponSlots[0].SetInteractable(false);
        spawnedWeaponSlots[1].SetInteractable(false);

        if (slotIndex == 0)
        {
            if (spawnedWeaponSlots[1].GetWeapon() != null)
            {
                yield return spawnedWeaponSlots[1].HolsterWeapon();
                spawnedWeaponSlots[1].SetSlotWeaponActive(false);

                spawnedWeaponSlots[1].SetInteractable(true);
            }
        }
        else if (slotIndex == 1)
        {
            if (spawnedWeaponSlots[0].GetWeapon() != null)
            {
                yield return spawnedWeaponSlots[0].HolsterWeapon();
                spawnedWeaponSlots[0].SetSlotWeaponActive(false);

                spawnedWeaponSlots[0].SetInteractable(true);
            }
        }

        onWeaponSlotSetActive?.Invoke(spawnedWeaponSlots[slotIndex]);

        yield return SetSlotActive(slotIndex);

        spawnedWeaponSlots[slotIndex].SetInteractable(true);
    }

    public void UseCurrentWeapon()
    {
        if (isLookingAtTarget)
            return;

        if (isAmmoSelectionMenuOpen)
            return;

        if (currentWeapon == null)
            return;

        currentWeapon.TryUse();
    }

    public void ReadyWeapon()
    {
        //if (isLookingAtTarget)
        //    return;

        //if (isAmmoSelectionMenuOpen)
        //    return;

        if (currentWeapon == null)
            return;

        if (currentWeapon.IsMeleeWeapon())
            return;

         currentWeapon.GetRangedWeapon().ReadyWeapon();
    }

    public void UnreadyWeapon()
    {
        //if (isLookingAtTarget)
        //    return;

        //if (isAmmoSelectionMenuOpen)
        //    return;

        if (currentWeapon == null)
            return;

        if (currentWeapon.IsMeleeWeapon())
            return;

        currentWeapon.GetRangedWeapon().UnreadyWeapon();
    }

    public void TryReloadCurrentWeapon(AmmoItemData ammoTypeToLoad = null)
    {
        if (currentWeapon == null)
            return;

        if (currentWeapon.IsMeleeWeapon())
            return;

        if(reloadWeaponCoroutine != null)
        {
            StopCoroutine(reloadWeaponCoroutine);
            reloadWeaponCoroutine = null;
        }

         reloadWeaponCoroutine = StartCoroutine(ReloadWeapon(ammoTypeToLoad));
    }

    IEnumerator ReloadWeapon(AmmoItemData ammoTypeToLoad)
    {
        spawnedWeaponSlots[activeSlotIndex].SetInteractable(false);

        if (currentWeapon.GetRangedWeapon() != null)
        {
            yield return currentWeapon.GetRangedWeapon().TryReload(ammoTypeToLoad);
        }

        spawnedWeaponSlots[activeSlotIndex].SetInteractable(true);

        reloadWeaponCoroutine = null;
    }

    List<WeaponItem> GetWeaponItems()
    {
        List<WeaponItem> weaponItems = new List<WeaponItem>();
        foreach (WeaponSlot slot in spawnedWeaponSlots)
        {
            IWeapon slotWeapon = slot.GetWeapon();
            if (slotWeapon.IsDefaultWeapon())
                continue;

            weaponItems.Add(slotWeapon.GetWeaponItem());
        }
        return weaponItems;
    }

    public void Save(ref PlayerSaveData data)
    {
        data.activeWeaponSlotIndex = activeSlotIndex;
        //data.weaponItems = GetWeaponItems();

        List<WeaponItem> weaponItems = GetWeaponItems();
        List<ItemStackSaveData> stackSaveData = new List<ItemStackSaveData>();

        foreach (WeaponItem item in weaponItems)
        {
            ItemStackSaveData saveData = new ItemStackSaveData
            {
                itemID = item.ItemData.itemIdentifier,
                amount = 1,
                isWeapon = true,
                loadedAmmoType = item.LoadedAmmoData != null
                    ? item.LoadedAmmoData.itemIdentifier
                    : "",
                loadedAmmo = item.LoadedAmmo
            };
            stackSaveData.Add(saveData);
        }

        data.weaponItems = stackSaveData;
    }

    public void Load(PlayerSaveData data)
    {
        foreach (WeaponSlot slot in spawnedWeaponSlots)
        {
            slot.UnloadSlot();
        }

        activeSlotIndex = data.activeWeaponSlotIndex;

        ItemDatabase itemDatabase = GridController.Instance.itemDatabase;
        for (int i = 0; i < data.weaponItems.Count; i++)
        {
            ItemStackSaveData savedItem = data.weaponItems[i];
            ItemData itemData = itemDatabase.GetItemDataFromIdentifier(savedItem.itemID);

            if (itemData == null)
                continue;

            Item item;
            if (itemData is WeaponItemData weaponItemData)
            {
                AmmoItemData ammoItemData = null;

                if (!string.IsNullOrEmpty(savedItem.loadedAmmoType))
                    ammoItemData = itemDatabase.GetItemDataFromIdentifier(savedItem.loadedAmmoType) as AmmoItemData;

                item = new WeaponItem(weaponItemData, ammoItemData, savedItem.loadedAmmo);
            }
            else
            {
                item = new Item(itemData);
            }

            spawnedWeaponSlots[i].AddItem(new ItemStack(item, 1));
        }

        StartCoroutine(SetWeaponSlotActive(activeSlotIndex));
    }
}
