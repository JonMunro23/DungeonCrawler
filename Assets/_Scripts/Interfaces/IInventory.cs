public interface IInventory
{
    public int GetRemainingAmmoOfType(AmmoWeaponType ammoTypeToGet);
    public void DecreaseAmmoOfType(AmmoWeaponType ammoTypeToRemove, int amountToRemove);
    public void IncreaseAmmoOfType(AmmoWeaponType ammoTypeToAdd, int amountToAdd);
    public void LockSlotsWithAmmoOfType(AmmoWeaponType ammoTypeToLock);
    public void UnlockSlots();
}
