using System;
using System.Collections;
using UnityEngine;

public class WeaponSlot : InventorySlot
{
    IWeapon currentWeapon;

    IWeapon defaultWeapon;
    WeaponItemData defaultWeaponData;
    WeaponItem defaultWeaponItem;

    IInventory playerInventory;

    public static event Action<int, WeaponItem> onWeaponAddedToSlot;
    public static event Action<int> onWeaponRemovedFromSlot;
    public static event Action<int, WeaponItem> onWeaponSwappedInSlot;

    public static event Action<int, WeaponItem> onWeaponSetToDefault;

    public static event Action<IWeapon> onWeaponDrawn;

    AudioEmitter audioEmitter;

    public void InitWeaponSlot(int newSlotIndex, IInventory _playerInventory, AudioEmitter weaponAudioEmitter)
    {
        SetSlotIndex(newSlotIndex);
        playerInventory = _playerInventory;
        audioEmitter = weaponAudioEmitter;


        SetInteractable(true);
    }

    public override void AddItem(ItemStack itemToAdd)
    {
        base.AddItem(itemToAdd);
        WeaponItem weaponItemToAdd = itemToAdd.Item as WeaponItem;
        if(weaponItemToAdd != null)
            InitialiseWeaponItem(weaponItemToAdd);

    }

    void InitialiseWeaponItem(WeaponItem weaponToInitialise)
    {
        onWeaponAddedToSlot?.Invoke(GetSlotIndex(), weaponToInitialise);
    }

    public override ItemStack SwapItem(ItemStack itemToSwap)
    {
        ItemStack itemToReturn = base.SwapItem(itemToSwap);
        if (itemToSwap.Item is WeaponItem weaponItemToSwap)
        {
            onWeaponSwappedInSlot?.Invoke(GetSlotIndex(), weaponItemToSwap);
        }

        return itemToReturn;
    }

    public override ItemStack TakeItem()
    {
        ItemStack itemToTake = base.TakeItem();

        WeaponItem weaponItemToTake = itemToTake.Item as WeaponItem;
        if (weaponItemToTake != null)
        {
            weaponItemToTake.SetLoadedAmmo(currentWeapon.GetRangedWeapon() != null ? currentWeapon.GetRangedWeapon().GetLoadedAmmo() : 0);
            DeinitialiseWeaponItem();
            slotImage.sprite = defaultWeapon.GetWeaponData().itemSprite;
        }

        return itemToTake;

    }

    void DeinitialiseWeaponItem()
    {
        onWeaponRemovedFromSlot?.Invoke(GetSlotIndex());
    }

    public void SetSlotWeaponActive(bool isActive)
    {
        if(currentWeapon != null)
            currentWeapon.SetWeaponActive(isActive);           
    }

    public IEnumerator HolsterWeapon()
    {
        if(currentWeapon != null)
            yield return currentWeapon.HolsterWeapon();
    }

    public IEnumerator DrawWeapon()
    {
        if (currentWeapon != null)
        {
            yield return currentWeapon.DrawWeapon();
            onWeaponDrawn?.Invoke(currentWeapon);
        }
    }

    public void SetWeapon(IWeapon newWeapon)
    {
        currentWeapon = newWeapon;
        currentWeapon.SetDefaultWeapon(false);
        currentWeapon.InitWeapon(this, newWeapon.GetWeaponItem(), audioEmitter, playerInventory);     

        //UpdateSlotUI();
    }
    public void InitDefaultWeapon(IWeapon _defaultWeapon)
    {
        defaultWeapon = _defaultWeapon;
        defaultWeaponData = defaultWeapon.GetWeaponData();

        defaultWeaponItem = new WeaponItem(defaultWeaponData, defaultWeaponData.defaultLoadedAmmoData, defaultWeaponData.magSize);
    }

    public void SetWeaponToDefault()
    {
        currentWeapon = defaultWeapon;
        currentWeapon.SetDefaultWeapon(true);
        currentWeapon.InitWeapon(this, defaultWeaponItem, audioEmitter, playerInventory);
        onWeaponSetToDefault?.Invoke(GetSlotIndex(), defaultWeaponItem);
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
        {
            RemoveItem();
            RemoveWeapon();
            SetWeaponToDefault();
        }
    }

    public void SetLoadedAmmo(int newLoadedAmmo)
    {
        WeaponItem weaponItem = GetItemStack().Item as WeaponItem;
        if (weaponItem != null)
        {
            weaponItem.SetLoadedAmmo(newLoadedAmmo);
            UpdateTooltipData();
        }
    }

    public void SetLoadedAmmoType(AmmoItemData newLoadedAmmoType)
    {
        WeaponItem weaponItem = GetItemStack().Item as WeaponItem;
        if (weaponItem != null)
        {
            Debug.Log(newLoadedAmmoType);
            weaponItem.SetLoadedAmmoType(newLoadedAmmoType);
            UpdateTooltipData();
        }
    }
}