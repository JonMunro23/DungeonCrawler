using System;
using System.Threading.Tasks;
using UnityEngine;

public class WeaponSlot : InventorySlot
{
    IWeapon currentWeapon;

    IWeapon defaultWeapon;
    WeaponItemData defaultWeaponData;

    IInventory playerInventory;

    public static Action<int, WeaponItemData, int> onWeaponAddedToSlot;
    public static Action<int> onWeaponRemovedFromSlot;
    public static Action<int, WeaponItemData, int> onWeaponSwappedInSlot;

    public static Action<int, WeaponItemData> onWeaponSetToDefault;


    AudioEmitter audioEmitter;

    public void InitWeaponSlot(int newSlotIndex, IInventory _playerInventory, AudioEmitter weaponAudioEmitter)
    {
        slotIndex = newSlotIndex;
        playerInventory = _playerInventory;
        audioEmitter = weaponAudioEmitter;


        SetInteractable(true);
    }

    public override void AddItem(ItemStack itemToAdd)
    {
        base.AddItem(itemToAdd);
        InitialiseWeaponItem(itemToAdd.itemData as WeaponItemData, itemToAdd.loadedAmmo);
    }

    void InitialiseWeaponItem(WeaponItemData itemDataToInitialise, int loadedAmmo)
    {
        onWeaponAddedToSlot?.Invoke(slotIndex, itemDataToInitialise, loadedAmmo);
    }

    public override ItemStack SwapItem(ItemStack itemToSwap)
    {
        ItemStack itemToReturn = base.SwapItem(itemToSwap);
        onWeaponSwappedInSlot?.Invoke(slotIndex, itemToSwap.itemData as WeaponItemData, itemToSwap.loadedAmmo);
        return itemToReturn;
    }

    public override ItemStack TakeItem()
    {
        ItemStack itemToTake = base.TakeItem();
        itemToTake.loadedAmmo = currentWeapon.GetRangedWeapon() != null ? currentWeapon.GetRangedWeapon().GetLoadedAmmo() : 0;
        DeinitialiseWeaponItem();
        return itemToTake;

    }

    void DeinitialiseWeaponItem()
    {
        onWeaponRemovedFromSlot?.Invoke(slotIndex);
    }

    public void SetSlotWeaponActive(bool isActive)
    {
        if(currentWeapon != null)
            currentWeapon.SetWeaponActive(isActive);           
    }

    public async Task HolsterWeapon()
    {
        if(currentWeapon != null)
            await currentWeapon.HolsterWeapon();
    }

    public async Task DrawWeapon()
    {
        if (currentWeapon != null)
            await currentWeapon.DrawWeapon();
    }

    public void SetWeapon(IWeapon newWeapon)
    {
        currentWeapon = newWeapon;
        currentWeapon.SetDefaultWeapon(false);
        currentWeapon.InitWeapon(slotIndex, newWeapon.GetWeaponData(), audioEmitter, playerInventory);     

        //UpdateSlotUI();
    }
    public void InitDefaultWeapon(IWeapon _defaultWeapon)
    {
        defaultWeapon = _defaultWeapon;
        defaultWeaponData = defaultWeapon.GetWeaponData();
    }

    public void SetWeaponToDefault()
    {
        currentWeapon = defaultWeapon;
        currentWeapon.SetDefaultWeapon(true);
        currentWeapon.InitWeapon(slotIndex, defaultWeaponData, audioEmitter, playerInventory);
        onWeaponSetToDefault?.Invoke(slotIndex, defaultWeaponData);
    }

    public IWeapon GetWeapon()
    {
        return currentWeapon;
    }

    public void RemoveWeapon()
    {
        currentWeapon.RemoveWeapon();
        currentWeapon = null;
    }

    public void UnloadSlot()
    {
        if (currentWeapon.IsDefaultWeapon())
            SetSlotWeaponActive(false);
        else
            RemoveItem();
    }
}