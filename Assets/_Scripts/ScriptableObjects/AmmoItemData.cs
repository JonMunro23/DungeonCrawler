using UnityEngine;

[CreateAssetMenu(fileName = "AmmoItem", menuName = "Items/New Ammo Item")]
public class AmmoItemData : ItemData
{
    [Header("Ammo Item Properties")]
    public WeaponType[] weaponTypes;
    //public AmmoWeaponType ammoWeaponType;
    public AmmoType ammoType;
}

