using System;
using System.Collections;
using System.Threading.Tasks;

public interface IWeapon : IUseable
{
    public void InitWeapon(WeaponItemData dataToInit);
    public void SetWeaponActive(bool isActive);
    public Task HolsterWeapon();
    public Task DrawWeapon();
    public void RemoveWeapon();
    public int GetLoadedAmmo();
    public WeaponItemData GetWeaponData();
    public void Reload();
    public void Grab();
    public bool IsReloading();
    public bool IsTwoHanded();
    public bool IsInUse();


}
