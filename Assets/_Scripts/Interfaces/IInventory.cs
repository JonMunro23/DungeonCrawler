public interface IInventory
{
    public int GetRemainingAmmoOfType(AmmoType ammoTypeToGet);
    public void DecreaseAmmoOfType(AmmoType ammoTypeToRemove, int amountToRemove);
    public void IncreaseAmmoOfType(AmmoType ammoTypeToAdd, int amountToAdd);
    public void LockSlotsWithAmmoOfType(AmmoType ammoTypeToLock);
    public void UnlockSlots();
}
