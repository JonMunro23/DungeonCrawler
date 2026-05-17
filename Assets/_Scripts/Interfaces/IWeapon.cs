using System.Collections;
using UnityEngine;

public interface IWeapon : IUseable
{
    public bool IsMeleeWeapon();
    public bool IsDefaultWeapon();
    public bool CanUse();
    public void SetWeaponActive(bool isActive);
    public void SetDefaultWeapon(bool isDefault);
    public WeaponItem GetWeaponItem();
    public WeaponItemData GetWeaponData();
    public Vector2 GetWeaponDamageRange();
    public MeleeWeapon GetMeleeWeapon();
    public RangedWeapon GetRangedWeapon();
    public void InitWeapon(WeaponSlot occupyingSlot, WeaponItem weaponToInit, AudioEmitter _weaponAudioEmitter, IInventory playerInventory);
    public IEnumerator DrawWeapon();
    public IEnumerator HolsterWeapon();
    //public Task ReadyWeapon();
    //public Task UnreadyWeapon();
    public IEnumerator Grab();
    public void RemoveWeapon();
    public int UnloadAmmo();
}
