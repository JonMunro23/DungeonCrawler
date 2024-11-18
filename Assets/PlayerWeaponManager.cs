using System;
using UnityEngine;

public class PlayerWeaponManager : MonoBehaviour
{
    PlayerController playerController;

    [Header("References")]
    [SerializeField] WeaponSlot slotToSpawn;
    [SerializeField] int numWeaponSlots;
    [SerializeField] WeaponItemData defaultWeaponData;
    [SerializeField] Transform weaponSpawnParent;

    [SerializeField] WeaponSlot[] spawnedWeaponSlots;
    [SerializeField] int activeSlotIndex;

    public IWeapon currentWeapon;
    IWeapon defaultWeapon;

    public static Action<WeaponSlot[]> onWeaponSlotsSpawned;
    public static Action<EquipmentSlotType, WeaponItemData> onWeaponSlotSetActive;

    private void OnEnable()
    {
        WeaponSlot.onWeaponAddedToSlot += OnWeaponAddedToSlot;
        WeaponSlot.onWeaponRemovedFromSlot += OnWeaponRemovedFromSlot;
        WeaponSlot.onWeaponSwappedInSlot += OnWeaponSwappedInSlot;
    }

    private void OnDisable()
    {
        WeaponSlot.onWeaponAddedToSlot -= OnWeaponAddedToSlot;
        WeaponSlot.onWeaponRemovedFromSlot -= OnWeaponRemovedFromSlot;
        WeaponSlot.onWeaponSwappedInSlot -= OnWeaponSwappedInSlot;
    }

    public void Init(PlayerController controller)
    {
        playerController = controller;

        SpawnWeaponSlots();
        InitialiseDefaultWeapon(0, defaultWeaponData);
        SetWeaponSlotActive(0);
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

        onWeaponSlotsSpawned?.Invoke(spawnedWeaponSlots);
    }

    void InitialiseDefaultWeapon(int slotIndex, WeaponItemData weaponItemData)
    {
        if (weaponItemData.itemPrefab)
        {
            GameObject spawnedWeapon = Instantiate(weaponItemData.itemPrefab, weaponSpawnParent);
            if (spawnedWeapon.TryGetComponent(out IWeapon weapon))
            {
                defaultWeapon = weapon;
                spawnedWeaponSlots[0].SetWeapon(weapon, weaponItemData);
                spawnedWeaponSlots[1].SetWeapon(weapon, weaponItemData);
            }
        }
    }

    /// <summary>
    /// Called when player clicks on a weapon slot. Handles the initialisation of new weapons.
    /// </summary>
    /// <param name="weaponSlot">The slot to put the new weapon in.</param>
    /// <param name="newWeaponItemData">The weapon data to initialise the new weapon with. </param>
    async void OnWeaponAddedToSlot(int slotIndex, WeaponItemData newWeaponItemData)
    {
        spawnedWeaponSlots[slotIndex].SetInteractable(false);

        if (!spawnedWeaponSlots[slotIndex].IsSlotEmpty())
        {
            if (activeSlotIndex == slotIndex)
            {
                await spawnedWeaponSlots[slotIndex].HolsterWeapon();
            }
        }

        InitialiseNewWeapon(slotIndex, newWeaponItemData);
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

        //if (slotIndex == 1 && activeSlotIndex != 0)
        //{
        //    activeSlotIndex = 0;
        //}

        SetSlotToDefault(slotIndex);
        //spawnedWeaponSlots[slotIndex].SetInteractable(true);

    }

    async void OnWeaponSwappedInSlot(int slotIndex, WeaponItemData weaponData)
    {
        spawnedWeaponSlots[slotIndex].SetInteractable(false);
        if (activeSlotIndex == slotIndex)
        {
            await spawnedWeaponSlots[slotIndex].HolsterWeapon();
        }
        if (spawnedWeaponSlots[slotIndex].GetWeapon() != defaultWeapon)
            spawnedWeaponSlots[slotIndex].RemoveWeapon();

        InitialiseNewWeapon(slotIndex, weaponData);

    }

    async void SetSlotToDefault(int slotIndex)
    {
        spawnedWeaponSlots[slotIndex].SetWeapon(defaultWeapon, defaultWeaponData);
        if (activeSlotIndex == slotIndex)
        {
            spawnedWeaponSlots[slotIndex].SetSlotWeaponActive(true);
            await spawnedWeaponSlots[slotIndex].DrawWeapon();
            currentWeapon = defaultWeapon;
        }
        spawnedWeaponSlots[slotIndex].SetInteractable(true);
    }

    async void InitialiseNewWeapon(int slotIndex, WeaponItemData weaponItemData)
    {
        if (weaponItemData.itemPrefab)
        {
            GameObject spawnedWeapon = Instantiate(weaponItemData.itemPrefab, weaponSpawnParent);
            if (spawnedWeapon.TryGetComponent(out IWeapon weapon))
            {
                weapon.SetWeaponActive(false);
                if (spawnedWeaponSlots[slotIndex].GetWeapon() == defaultWeapon && spawnedWeaponSlots[activeSlotIndex].GetWeapon() != defaultWeapon)
                    spawnedWeaponSlots[slotIndex].SetSlotWeaponActive(false);

                spawnedWeaponSlots[slotIndex].SetWeapon(weapon, weaponItemData);

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
            if (currentWeapon == defaultWeapon && !spawnedWeaponSlots[1].isSlotOccupied)
                return;

            activeSlotIndex = 1;
            SetWeaponSlotActive(activeSlotIndex);
        }
        else if (activeSlotIndex == 1)
        {
            if (currentWeapon == defaultWeapon && !spawnedWeaponSlots[0].isSlotOccupied)
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

    public void ReloadCurrentWeapon()
    {
        if (currentWeapon == null)
            return;

        currentWeapon.Reload();
    }
}
