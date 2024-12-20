using System.Threading.Tasks;

public interface IWeapon : IUseable
{
    public bool IsMeleeWeapon();
    public bool IsDefaultWeapon();
    public bool CanUse();
    public void SetWeaponActive(bool isActive);
    public void SetDefaultWeapon(bool isDefault);
    public WeaponItemData GetWeaponData();
    public MeleeWeapon GetMeleeWeapon();
    public RangedWeapon GetRangedWeapon();
    public void InitWeapon(WeaponSlot occupyingSlot, WeaponItemData dataToInit, AudioEmitter _weaponAudioEmitter, IInventory playerInventory);
    public Task DrawWeapon();
    public Task HolsterWeapon();
    public void Grab();
    public void RemoveWeapon();
    public int UnloadAmmo();
}
