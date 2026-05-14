using System;
using System.Threading.Tasks;
using UnityEngine;

public class WeaponSlot : InventorySlot
{
    IWeapon currentWeapon;

    IWeapon defaultWeapon;
    WeaponItemData defaultWeaponData;

    IInventory playerInventory;

    public static event Action<int, WeaponItemData, int> onWeaponAddedToSlot;
    public static event Action<int> onWeaponRemovedFromSlot;
    public static event Action<int, WeaponItemData, int> onWeaponSwappedInSlot;

    public static event Action<int, WeaponItemData> onWeaponSetToDefault;

    public static event Action<IWeapon> onWeaponDrawn;

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
        WeaponItem weaponItemToAdd = itemToAdd.Item as WeaponItem;
        if(weaponItemToAdd != null)
            InitialiseWeaponItem(weaponItemToAdd.WeaponItemData, weaponItemToAdd.LoadedAmmo);

    }

    void InitialiseWeaponItem(WeaponItemData itemDataToInitialise, int loadedAmmo)
    {
        onWeaponAddedToSlot?.Invoke(slotIndex, itemDataToInitialise, loadedAmmo);
    }

    public override ItemStack SwapItem(ItemStack itemToSwap)
    {
        WeaponItem weaponItemToSwap = itemToSwap.Item as WeaponItem;
        if (weaponItemToSwap != null)
        {
            ItemStack itemToReturn = base.SwapItem(itemToSwap);
            onWeaponSwappedInSlot?.Invoke(slotIndex, weaponItemToSwap.WeaponItemData, weaponItemToSwap.LoadedAmmo);
            return itemToReturn;
        }

        return null;
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
        {
            await currentWeapon.DrawWeapon();
            onWeaponDrawn?.Invoke(currentWeapon);
        }
    }

    public void SetWeapon(IWeapon newWeapon)
    {
        currentWeapon = newWeapon;
        currentWeapon.SetDefaultWeapon(false);
        currentWeapon.InitWeapon(this, newWeapon.GetWeaponData(), audioEmitter, playerInventory);     

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
        currentWeapon.InitWeapon(this, defaultWeaponData, audioEmitter, playerInventory);
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
            weaponItem.SetLoadedAmmoType(newLoadedAmmoType);
            UpdateTooltipData();
        }
    }
}