using System;
using System.Threading.Tasks;
using UnityEngine;

public class WeaponSlot : EquipmentSlot
{
    public int slotIndex;
    IWeapon weapon;

    public static Action<int, WeaponItemData> onWeaponAddedToSlot;
    public static Action<int> onWeaponRemovedFromSlot;
    public static Action<int, WeaponItemData> onWeaponSwappedInSlot;

    public void InitWeaponSlot(int newSlotIndex)
    {
        slotIndex = newSlotIndex;
        SetInteractable(true);
    }

    public override void AddItem(ItemStack itemToAdd)
    {
        base.AddItem(itemToAdd);
        InitialiseWeaponItem(itemToAdd.itemData as WeaponItemData);
    }

    void InitialiseWeaponItem(WeaponItemData itemDataToInitialise)
    {
        onWeaponAddedToSlot?.Invoke(slotIndex, itemDataToInitialise);
    }

    public override ItemStack SwapItem(ItemStack itemToSwap)
    {
        ItemStack itemToReturn = base.SwapItem(itemToSwap);
        onWeaponSwappedInSlot?.Invoke(slotIndex, itemToSwap.itemData as WeaponItemData);
        return itemToReturn;
    }

    public override ItemStack TakeItem()
    {
        ItemStack itemToTake = base.TakeItem();
        DeinitialiseWeaponItem();
        return itemToTake;

    }

    void DeinitialiseWeaponItem()
    {
        onWeaponRemovedFromSlot?.Invoke(slotIndex);
    }

    public WeaponSlot(int slotIndex, IWeapon weapon)
    {
        this.slotIndex = slotIndex;
        this.weapon = weapon;
    }

    public void SetSlotWeaponActive(bool isActive)
    {
        if(weapon != null)
            weapon.SetWeaponActive(isActive);           
    }

    public async Task HolsterWeapon()
    {
        await weapon.HolsterWeapon();
    }

    public async Task DrawWeapon()
    {
        await weapon.DrawWeapon();
    }

    public void SetWeapon(IWeapon newWeapon, WeaponItemData newWeaponData)
    {
        weapon = newWeapon;
        weapon.InitWeapon(newWeaponData);
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