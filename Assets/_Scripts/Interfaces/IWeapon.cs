using System;
using System.Collections;
using System.Threading.Tasks;

public interface IWeapon : IUseable
{
    public void InitWeapon(int slotIndex, WeaponItemData dataToInit, AudioEmitter weaponAudioEmitter);
    public void SetInventoryManager(IInventory playerInventory);
    public void SetWeaponActive(bool isActive);
    public Task HolsterWeapon();
    public Task DrawWeapon();
    public void RemoveWeapon();
    public void UpdateReserveAmmo();
    public int GetLoadedAmmo();
    public void SetLoadedAmmo(int loadedAmmo);
    public WeaponItemData GetWeaponData();
    public Task TryReload();
    public void Grab();
    public bool IsReloading();
    public bool IsTwoHanded();
    public bool IsInUse();
}
