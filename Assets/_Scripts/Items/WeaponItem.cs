//[System.Serializable]
public class WeaponItem : Item
{
    WeaponItemData weaponItemData;
    public WeaponItemData WeaponItemData => weaponItemData;

    AmmoItemData loadedAmmoData;
    public AmmoItemData LoadedAmmoData => loadedAmmoData;

    int loadedAmmo;
    public int LoadedAmmo => loadedAmmo;

    public WeaponItem(ItemData itemData, AmmoItemData loadedAmmoData, int loadedAmmo) : base(itemData)
    {
        weaponItemData = itemData as WeaponItemData;
        this.loadedAmmoData = loadedAmmoData;
        this.loadedAmmo = loadedAmmo;
    }

    public void SetLoadedAmmo(int newValue)
    {
        loadedAmmo = newValue;
    }

    public void SetLoadedAmmoType(AmmoItemData newAmmoType)
    {
        loadedAmmoData = newAmmoType;
    }

    //public int UnloadAmmo()
    //{
    //    int ammoToReturn = loadedAmmo;
    //    SetLoadedAmmo(0);
    //    return ammoToReturn;
    //}
}
