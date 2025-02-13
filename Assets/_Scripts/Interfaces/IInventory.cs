public interface IInventory
{
    public int TryGetRemainingAmmoOfType(AmmoItemData ammoTypeToGet);
    public void DecreaseAmmoOfType(AmmoItemData ammoTypeToRemove, int amountToRemove);
    public void IncreaseAmmoOfType(AmmoItemData ammoTypeToAdd, int amountToAdd);
    public void LockSlotsWithAmmoOfType(AmmoItemData ammoTypeToLock);
    public void UnlockSlots();
}
