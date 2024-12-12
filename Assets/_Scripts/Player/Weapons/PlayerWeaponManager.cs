using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
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

    public WeaponItemData heldWeaponData;
    public int heldWeaponLoadedAmmo;

    public WeaponSlotData(int slotIndex, WeaponItemData heldWeaponData, int heldWeaponLoadedAmmo)
    {
        this.slotIndex = slotIndex;
        this.heldWeaponData = heldWeaponData;
        this.heldWeaponLoadedAmmo = heldWeaponLoadedAmmo;
    }
}

public class PlayerWeaponManager : MonoBehaviour
{
    PlayerController playerController;

    [Header("References")]
    [SerializeField] WeaponSlot slotToSpawn;
    [SerializeField] int numWeaponSlots;
    [SerializeField] WeaponItemData defaultWeaponData;
    [SerializeField] Transform weaponSpawnParent;
    AudioEmitter weaponAudioEmitter;

    [SerializeField] WeaponSlot[] spawnedWeaponSlots;
    [SerializeField] int activeSlotIndex = 0;

    [Header("Bonus Weapon Stats")]
    public static int bonusDamage;
    public static int bonusBurstCount;
    public static int bonusCritChance;
    public static int bonusCritMultiplier;
    public static int bonusAccuracy;

    public IWeapon currentWeapon;
    //IWeapon defaultWeapon;

    public static Action<WeaponSlot[]> onWeaponSlotsSpawned;
    public static Action<int> onWeaponSlotSetActive;
    public static Action<int, WeaponItemData> onNewWeaponInitialised;

    private void OnEnable()
    {
        WeaponSlot.onWeaponAddedToSlot += OnWeaponAddedToSlot;
        WeaponSlot.onWeaponRemovedFromSlot += OnWeaponRemovedFromSlot;
        WeaponSlot.onWeaponSwappedInSlot += OnWeaponSwappedInSlot;

        PlayerInventoryManager.onAmmoAddedToInventory += OnInventoryAmmoUpdated;

        StatData.onStatUpdated += OnStatUpdated;

    }

    private void OnDisable()
    {
        WeaponSlot.onWeaponAddedToSlot -= OnWeaponAddedToSlot;
        WeaponSlot.onWeaponRemovedFromSlot -= OnWeaponRemovedFromSlot;
        WeaponSlot.onWeaponSwappedInSlot -= OnWeaponSwappedInSlot;

        PlayerInventoryManager.onAmmoAddedToInventory -= OnInventoryAmmoUpdated;

        StatData.onStatUpdated -= OnStatUpdated;

    }

    public virtual void OnStatUpdated(StatData updatedStat)
    {
        switch (updatedStat.stat)
        {
            case ModifiableStats.Damage:
                bonusDamage = Mathf.RoundToInt(updatedStat.GetCurrentStatValue());
                break;
            case ModifiableStats.BurstCount:
                bonusBurstCount = Mathf.RoundToInt(updatedStat.GetCurrentStatValue());
                break;
            case ModifiableStats.CritChance:
                bonusCritChance = Mathf.RoundToInt(updatedStat.GetCurrentStatValue());
                break;
            case ModifiableStats.CritMultiplier:
                bonusCritMultiplier = Mathf.RoundToInt(updatedStat.GetCurrentStatValue());
                break;
            case ModifiableStats.WeaponAccuracy:
                bonusAccuracy = Mathf.RoundToInt(updatedStat.GetCurrentStatValue());
                break;
        }
    }

    void OnInventoryAmmoUpdated(AmmoType typeAdded)
    {
        if (currentWeapon == null)
            return;

        if (currentWeapon.IsMeleeWeapon())
            return;

        if(currentWeapon.GetRangedWeapon() == null)
            return;

        if (typeAdded == currentWeapon.GetWeaponData().ammoType)
        {
            currentWeapon.GetRangedWeapon().UpdateReserveAmmo();
        }

    }

    public async void Init(PlayerController controller)
    {
        playerController = controller;

        weaponAudioEmitter = AudioManager.Instance.RegisterSource("[AudioEmitter] Weapon", transform, AudioCategory.SFx, 10, 25, 0);

        SpawnWeaponSlots();

        InitialiseDefaultWeapons();

        await SetSlotActive(activeSlotIndex);
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

    void InitialiseDefaultWeapons()
    {
        if (defaultWeaponData.itemPrefab)
        {
            GameObject spawnedWeapon = Instantiate(defaultWeaponData.itemPrefab, weaponSpawnParent);
            if (spawnedWeapon.TryGetComponent(out IWeapon weapon))
            {
                //defaultWeapon = weapon;
                for (int i = 0; i < spawnedWeaponSlots.Length; i++)
                {
                    weapon.InitWeapon(i, defaultWeaponData, weaponAudioEmitter, playerController.playerInventoryManager);
                    spawnedWeaponSlots[i].InitDefaultWeapon(weapon);
                    spawnedWeaponSlots[i].SetWeaponToDefault();
                }
            }
        }
    }

    private async Task SetSlotActive(int slotIndex)
    {
        spawnedWeaponSlots[slotIndex].SetSlotWeaponActive(true);
        await spawnedWeaponSlots[slotIndex].DrawWeapon();
        currentWeapon = spawnedWeaponSlots[slotIndex].GetWeapon();
    }

    /// <summary>
    /// Called when player clicks on a weapon slot. Handles the initialisation of new weapons.
    /// </summary>
    /// <param name="weaponSlot">The slot to put the new weapon in.</param>
    /// <param name="newWeaponItemData">The weapon data to initialise the new weapon with. </param>
    async void OnWeaponAddedToSlot(int slotIndex, WeaponItemData newWeaponItemData, int startingAmmo)
    {
        spawnedWeaponSlots[slotIndex].SetInteractable(false);

        if (!spawnedWeaponSlots[slotIndex].IsSlotEmpty())
        {
            if (activeSlotIndex == slotIndex)
            {
                await spawnedWeaponSlots[slotIndex].HolsterWeapon();
            }

            if (!spawnedWeaponSlots[slotIndex].GetWeapon().IsDefaultWeapon())
                spawnedWeaponSlots[slotIndex].RemoveWeapon();
            else if (!spawnedWeaponSlots[activeSlotIndex].GetWeapon().IsDefaultWeapon())
                spawnedWeaponSlots[slotIndex].SetSlotWeaponActive(false);
        }

        InitialiseNewWeapon(slotIndex, newWeaponItemData, startingAmmo);

        if (activeSlotIndex == slotIndex)
        {
            await SetSlotActive(slotIndex);
        }

        spawnedWeaponSlots[slotIndex].SetInteractable(true);
    }

    async void OnWeaponRemovedFromSlot(int slotIndex)
    {
        spawnedWeaponSlots[slotIndex].SetInteractable(false);

        if (activeSlotIndex == slotIndex)
        {
            await spawnedWeaponSlots[slotIndex].HolsterWeapon();
        }

        if (!spawnedWeaponSlots[slotIndex].GetWeapon().IsDefaultWeapon())
            spawnedWeaponSlots[slotIndex].RemoveWeapon();
        else if (!spawnedWeaponSlots[activeSlotIndex].GetWeapon().IsDefaultWeapon())
            spawnedWeaponSlots[slotIndex].SetSlotWeaponActive(false);


        SetSlotToDefault(slotIndex);

        spawnedWeaponSlots[slotIndex].SetInteractable(true);
    }

    async void OnWeaponSwappedInSlot(int slotIndex, WeaponItemData weaponData, int newWeaponLoadedAmmo)
    {
        spawnedWeaponSlots[slotIndex].SetInteractable(false);
        if (activeSlotIndex == slotIndex)
        {
            await spawnedWeaponSlots[slotIndex].HolsterWeapon();
        }
        if (!spawnedWeaponSlots[slotIndex].GetWeapon().IsDefaultWeapon())
            spawnedWeaponSlots[slotIndex].RemoveWeapon();
        else if (!spawnedWeaponSlots[activeSlotIndex].GetWeapon().IsDefaultWeapon())
            spawnedWeaponSlots[slotIndex].SetSlotWeaponActive(false);

        InitialiseNewWeapon(slotIndex, weaponData, newWeaponLoadedAmmo);

        if (activeSlotIndex == slotIndex)
        {
            await SetSlotActive(slotIndex);
        }

        spawnedWeaponSlots[slotIndex].SetInteractable(true);
    }

    async void SetSlotToDefault(int slotIndex)
    {
        spawnedWeaponSlots[slotIndex].SetWeaponToDefault();
        if (activeSlotIndex == slotIndex)
        {
            await SetSlotActive(slotIndex);
        }
        spawnedWeaponSlots[slotIndex].SetInteractable(true);
    }

    

    void InitialiseNewWeapon(int slotIndex, WeaponItemData weaponItemData, int startingAmmo)
    {
        if (weaponItemData.itemPrefab)
        {
            GameObject spawnedWeapon = Instantiate(weaponItemData.itemPrefab, weaponSpawnParent);
            if (spawnedWeapon.TryGetComponent(out IWeapon weapon))
            {
                weapon.InitWeapon(slotIndex, weaponItemData, weaponAudioEmitter, playerController.playerInventoryManager);
                weapon.SetWeaponActive(false);
                //if (spawnedWeaponSlots[slotIndex].GetWeapon().IsDefaultWeapon() && !spawnedWeaponSlots[activeSlotIndex].GetWeapon().IsDefaultWeapon())
                //    spawnedWeaponSlots[slotIndex].SetSlotWeaponActive(false);


                if(weapon.GetRangedWeapon() != null)
                {
                    weapon.GetRangedWeapon().SetLoadedAmmo(startingAmmo);
                }

                spawnedWeaponSlots[slotIndex].SetWeapon(weapon);

            }

            onNewWeaponInitialised?.Invoke(slotIndex, weaponItemData);
        }
    }

    public void SwitchWeaponSets()
    {
        if (!currentWeapon.CanUse())
            return;

        if (activeSlotIndex == 0)
        {
            if (currentWeapon.IsDefaultWeapon() && spawnedWeaponSlots[1].GetWeapon().IsDefaultWeapon())
                return;

            activeSlotIndex = 1;
            SetWeaponSlotActive(activeSlotIndex);
        }
        else if (activeSlotIndex == 1)
        {
            if (currentWeapon.IsDefaultWeapon() && spawnedWeaponSlots[0].GetWeapon().IsDefaultWeapon())
                return;

            activeSlotIndex = 0;
            SetWeaponSlotActive(activeSlotIndex);
        }
    }

    async void SetWeaponSlotActive(int slotIndex)
    {
        spawnedWeaponSlots[0].SetInteractable(false);
        spawnedWeaponSlots[1].SetInteractable(false);

        if (slotIndex == 0)
        {
            if (spawnedWeaponSlots[1].GetWeapon() != null)
            {
                await spawnedWeaponSlots[1].HolsterWeapon();
                spawnedWeaponSlots[1].SetSlotWeaponActive(false);

                spawnedWeaponSlots[1].SetInteractable(true);
            }
        }
        else if (slotIndex == 1)
        {
            if (spawnedWeaponSlots[0].GetWeapon() != null)
            {
                await spawnedWeaponSlots[0].HolsterWeapon();
                spawnedWeaponSlots[0].SetSlotWeaponActive(false);

                spawnedWeaponSlots[0].SetInteractable(true);
            }
        }

        onWeaponSlotSetActive?.Invoke(slotIndex);

        await SetSlotActive(slotIndex);

        spawnedWeaponSlots[slotIndex].SetInteractable(true);
    }

    public void UseCurrentWeapon()
    {
        if (currentWeapon == null)
            return;

        currentWeapon.TryUse();
    }

    public void UseCurrentWeaponSpecial()
    {
        if (currentWeapon == null)
            return;

        currentWeapon.TryUseSpecial();
    }

    public async void ReloadCurrentWeapon()
    {
        if (currentWeapon == null)
            return;

        if (currentWeapon.IsMeleeWeapon())
            return;

        spawnedWeaponSlots[activeSlotIndex].SetInteractable(false);

        if (currentWeapon.GetRangedWeapon() != null)
        {
            await currentWeapon.GetRangedWeapon().TryReload();
        }

        spawnedWeaponSlots[activeSlotIndex].SetInteractable(true);
    }

    List<WeaponSlotData> GetWeaponSlotData()
    {
        List<WeaponSlotData> slotData = new List<WeaponSlotData>();
        foreach (WeaponSlot slot in spawnedWeaponSlots)
        {
            IWeapon slotWeapon = slot.GetWeapon();
            if (slotWeapon.IsDefaultWeapon())
                continue;

            int loadedAmmo = 0;
            RangedWeapon rangedWeapon = slot.GetWeapon() as RangedWeapon;
            if(rangedWeapon != null)
            {
                loadedAmmo = rangedWeapon.GetLoadedAmmo();
            }

            slotData.Add(new WeaponSlotData(slot.slotIndex, slotWeapon.GetWeaponData(), loadedAmmo));
        }
        return slotData;
    }

    public void Save(ref PlayerSaveData data)
    {
        data.activeWeaponSlotIndex = activeSlotIndex;
        data.weaponSlotData = GetWeaponSlotData();
    }

    public void Load(PlayerSaveData data)
    {
        foreach (WeaponSlot slot in spawnedWeaponSlots)
        {
            slot.UnloadSlot();
        }
        activeSlotIndex = data.activeWeaponSlotIndex;

        for (int i = 0; i < data.weaponSlotData.Count; i++)
        {
            spawnedWeaponSlots[data.weaponSlotData[i].slotIndex].AddItem(new ItemStack(data.weaponSlotData[i].heldWeaponData, 1, data.weaponSlotData[i].heldWeaponLoadedAmmo)); 
        }


        SetWeaponSlotActive(activeSlotIndex);
    }
}
