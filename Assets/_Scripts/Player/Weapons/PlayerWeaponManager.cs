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
    [SerializeField] int activeSlotIndex;

    public IWeapon currentWeapon;
    IWeapon defaultWeapon;

    public static Action<WeaponSlot[]> onWeaponSlotsSpawned;
    public static Action<int> onWeaponSlotSetActive;

    private void OnEnable()
    {
        WeaponSlot.onWeaponAddedToSlot += OnWeaponAddedToSlot;
        WeaponSlot.onWeaponRemovedFromSlot += OnWeaponRemovedFromSlot;
        WeaponSlot.onWeaponSwappedInSlot += OnWeaponSwappedInSlot;

        PlayerInventoryManager.onAmmoAddedToInventory += OnInventoryAmmoUpdated;
    }

    private void OnDisable()
    {
        WeaponSlot.onWeaponAddedToSlot -= OnWeaponAddedToSlot;
        WeaponSlot.onWeaponRemovedFromSlot -= OnWeaponRemovedFromSlot;
        WeaponSlot.onWeaponSwappedInSlot -= OnWeaponSwappedInSlot;

        PlayerInventoryManager.onAmmoAddedToInventory -= OnInventoryAmmoUpdated;
    }

    void OnInventoryAmmoUpdated(AmmoType typeAdded)
    {
        if (currentWeapon == null)
            return;

        if (typeAdded == currentWeapon.GetWeaponData().ammoType)
        {
            currentWeapon.UpdateReserveAmmo();
        }

    }

    public void Init(PlayerController controller)
    {
        playerController = controller;

        weaponAudioEmitter = AudioManager.Instance.RegisterSource("[AudioEmitter] Weapon", transform, AudioCategory.SFx, 10, 25, 0);

        SpawnWeaponSlots();
    }

    void SpawnWeaponSlots()
    {
        spawnedWeaponSlots = new WeaponSlot[numWeaponSlots];

        for (int i = 0; i < numWeaponSlots; i++)
        {
            WeaponSlot spawnedSlot = Instantiate(slotToSpawn);
            spawnedWeaponSlots[i] = spawnedSlot;
            spawnedSlot.InitWeaponSlot(i);
        }
        InitialiseDefaultWeapon(0, defaultWeaponData);
        onWeaponSlotsSpawned?.Invoke(spawnedWeaponSlots);
    }

    async void InitialiseDefaultWeapon(int slotIndex, WeaponItemData weaponItemData)
    {
        if (weaponItemData.itemPrefab)
        {
            GameObject spawnedWeapon = Instantiate(weaponItemData.itemPrefab, weaponSpawnParent);
            if (spawnedWeapon.TryGetComponent(out IWeapon weapon))
            {
                defaultWeapon = weapon;
                weapon.SetInventoryManager(playerController.playerInventoryManager);
                spawnedWeaponSlots[0].SetWeaponToDefault(weapon, weaponItemData, weaponAudioEmitter);
                spawnedWeaponSlots[1].SetWeaponToDefault(weapon, weaponItemData, weaponAudioEmitter);

                spawnedWeaponSlots[slotIndex].SetSlotWeaponActive(true);
                await spawnedWeaponSlots[slotIndex].DrawWeapon();
                currentWeapon = spawnedWeaponSlots[slotIndex].GetWeapon();
            }
        }
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
        }

        InitialiseNewWeapon(slotIndex, newWeaponItemData, startingAmmo);
    }

    async void OnWeaponRemovedFromSlot(int slotIndex)
    {
        spawnedWeaponSlots[slotIndex].SetInteractable(false);

        if (activeSlotIndex == slotIndex)
        {
            await spawnedWeaponSlots[slotIndex].HolsterWeapon();
        }

        if (spawnedWeaponSlots[slotIndex].GetWeapon() != defaultWeapon)
            spawnedWeaponSlots[slotIndex].RemoveWeapon();


        SetSlotToDefault(slotIndex);
    }

    async void OnWeaponSwappedInSlot(int slotIndex, WeaponItemData weaponData, int newWeaponLoadedAmmo)
    {
        spawnedWeaponSlots[slotIndex].SetInteractable(false);
        if (activeSlotIndex == slotIndex)
        {
            await spawnedWeaponSlots[slotIndex].HolsterWeapon();
        }
        if (spawnedWeaponSlots[slotIndex].GetWeapon() != defaultWeapon)
            spawnedWeaponSlots[slotIndex].RemoveWeapon();

        InitialiseNewWeapon(slotIndex, weaponData, newWeaponLoadedAmmo);

    }

    async void SetSlotToDefault(int slotIndex)
    {
        spawnedWeaponSlots[slotIndex].SetWeaponToDefault(defaultWeapon, defaultWeaponData, weaponAudioEmitter);
        if (activeSlotIndex == slotIndex)
        {
            spawnedWeaponSlots[slotIndex].SetSlotWeaponActive(true);
            await spawnedWeaponSlots[slotIndex].DrawWeapon();
            currentWeapon = defaultWeapon;
        }
        spawnedWeaponSlots[slotIndex].SetInteractable(true);

      
    }

    

    async void InitialiseNewWeapon(int slotIndex, WeaponItemData weaponItemData, int startingAmmo)
    {
        if (weaponItemData.itemPrefab)
        {
            GameObject spawnedWeapon = Instantiate(weaponItemData.itemPrefab, weaponSpawnParent);
            if (spawnedWeapon.TryGetComponent(out IWeapon weapon))
            {
                weapon.SetWeaponActive(false);
                if (spawnedWeaponSlots[slotIndex].GetWeapon() == defaultWeapon && spawnedWeaponSlots[activeSlotIndex].GetWeapon() != defaultWeapon)
                    spawnedWeaponSlots[slotIndex].SetSlotWeaponActive(false);

                weapon.SetInventoryManager(playerController.playerInventoryManager);
                weapon.SetLoadedAmmo(startingAmmo);
                spawnedWeaponSlots[slotIndex].SetWeapon(weapon, weaponItemData, weaponAudioEmitter);

                if (activeSlotIndex == slotIndex)
                {
                    spawnedWeaponSlots[slotIndex].SetSlotWeaponActive(true);
                    await spawnedWeaponSlots[slotIndex].DrawWeapon();
                    currentWeapon = spawnedWeaponSlots[slotIndex].GetWeapon();
                }

                spawnedWeaponSlots[slotIndex].SetInteractable(true);
            }
        }
    }

    public void SwitchWeaponSets()
    {
        if (currentWeapon.IsInUse())
            return;

        if (activeSlotIndex == 0)
        {
            if (currentWeapon == defaultWeapon && spawnedWeaponSlots[1].GetWeapon() == defaultWeapon)
                return;

            activeSlotIndex = 1;
            SetWeaponSlotActive(activeSlotIndex);
        }
        else if (activeSlotIndex == 1)
        {
            if (currentWeapon == defaultWeapon && spawnedWeaponSlots[0].GetWeapon() == defaultWeapon)
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

        spawnedWeaponSlots[slotIndex].SetSlotWeaponActive(true);
        await spawnedWeaponSlots[slotIndex].DrawWeapon();

        currentWeapon = spawnedWeaponSlots[slotIndex].GetWeapon();
        spawnedWeaponSlots[slotIndex].SetInteractable(true);
    }

    public void UseCurrentWeapon()
    {
        if (currentWeapon == null)
            return;

        currentWeapon.Use();
    }

    public void UseCurrentWeaponSpecial()
    {
        if (currentWeapon == null)
            return;

        currentWeapon.UseSpecial();
    }

    public async void ReloadCurrentWeapon()
    {
        if (currentWeapon == null)
            return;

        spawnedWeaponSlots[activeSlotIndex].SetInteractable(false);

        await currentWeapon.TryReload();

        spawnedWeaponSlots[activeSlotIndex].SetInteractable(true);
    }

    List<WeaponSlotData> GetWeaponSlotData()
    {
        List<WeaponSlotData> slotData = new List<WeaponSlotData>();
        foreach (WeaponSlot slot in spawnedWeaponSlots)
        {
            IWeapon slotWeapon = slot.GetWeapon();
            if(slotWeapon == defaultWeapon)
                continue;

            slotData.Add(new WeaponSlotData(slot.slotIndex, slotWeapon.GetWeaponData(), slotWeapon.GetLoadedAmmo()));
        }
        return slotData;
    }

    public void Save(ref PlayerWeaponSaveData data)
    {
        data.activeSlotIndex = activeSlotIndex;
        data.slotData = GetWeaponSlotData();
    }

    public void Load(PlayerWeaponSaveData data)
    {
        activeSlotIndex = data.activeSlotIndex;
        foreach (WeaponSlotData slotData in data.slotData)
        {
            spawnedWeaponSlots[slotData.slotIndex].TakeItem();
            spawnedWeaponSlots[slotData.slotIndex].AddItem(new ItemStack(slotData.heldWeaponData, 1, slotData.heldWeaponLoadedAmmo));
        }
        onWeaponSlotSetActive?.Invoke(activeSlotIndex);
        //SetWeaponSlotActive(activeSlotIndex);
    }
}
