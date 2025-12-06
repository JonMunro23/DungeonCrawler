using System.Collections.Generic;

public interface IInventory
{
    public int TryGetRemainingAmmoOfType(AmmoItemData ammoTypeToGet);
    public void DecreaseAmmoOfType(AmmoItemData ammoTypeToRemove, int amountToRemove);
    public void IncreaseAmmoOfType(AmmoItemData ammoTypeToAdd, int amountToAdd);
    public void LockSlotsWithAmmoOfType(AmmoItemData ammoTypeToLock);
    public List<AmmoItemData> GetAllUseableAmmoForWeapon(IWeapon weapon);
    public List<ThrowableItemData> GetAllAvailableThrowables();
    public void UnlockSlots();
    public int TryAddItem(ItemStack itemToAdd);
}
