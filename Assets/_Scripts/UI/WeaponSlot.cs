using System;
using System.Threading.Tasks;
using UnityEngine;

public class WeaponSlot : InventorySlot
{
    IWeapon weapon;

    public static Action<int, WeaponItemData, int> onWeaponAddedToSlot;
    public static Action<int> onWeaponRemovedFromSlot;
    public static Action<int, WeaponItemData, int> onWeaponSwappedInSlot;

    public static Action<int, WeaponItemData> onWeaponSetToDefault;

    public void InitWeaponSlot(int newSlotIndex)
    {
        slotIndex = newSlotIndex;
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
        itemToTake.loadedAmmo = weapon.GetLoadedAmmo();
        DeinitialiseWeaponItem();
        return itemToTake;

    }

    void DeinitialiseWeaponItem()
    {
        onWeaponRemovedFromSlot?.Invoke(slotIndex);
    }

    public void SetSlotWeaponActive(bool isActive)
    {
        if(weapon != null)
            weapon.SetWeaponActive(isActive);           
    }

    public async Task HolsterWeapon()
    {
        if(weapon != null)
            await weapon.HolsterWeapon();
    }

    public async Task DrawWeapon()
    {
        if (weapon != null)
            await weapon.DrawWeapon();
    }

    public void SetWeapon(IWeapon newWeapon, WeaponItemData newWeaponData, AudioEmitter weaponAudioEmitter)
    {
        weapon = newWeapon;
        weapon.InitWeapon(slotIndex, newWeaponData, weaponAudioEmitter);
    }

    public void SetWeaponToDefault(IWeapon defaultWeapon, WeaponItemData defaultWeaponData, AudioEmitter weaponAudioEmitter)
    {
        weapon = defaultWeapon;
        weapon.InitWeapon(slotIndex, defaultWeaponData, weaponAudioEmitter);
        onWeaponSetToDefault?.Invoke(slotIndex, defaultWeaponData);
    }

    public IWeapon GetWeapon()
    {
        return weapon;
    }

    public void RemoveWeapon()
    {
        weapon.RemoveWeapon();
        weapon = null;
    }
}