using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "ItemDataContainer", menuName = "Items/New ItemData Container")]

public class ItemDataContainer : ScriptableObject
{
    [Header("Items")]
    public List<ItemData> itemData = new List<ItemData>();
    [Header("Weapons")]
    public List<WeaponItemData> weaponItemData = new List<WeaponItemData>();
    [Header("Ammo")]
    public List<AmmoItemData> ammoItemData = new List<AmmoItemData>();
    [Header("Equipment")]
    public List<EquipmentItemData> equipmentItemData = new List<EquipmentItemData>();
    [Header("Consumables")]
    public List<ConsumableItemData> consumableItemData = new List<ConsumableItemData>();
    [Header("Throwables")]
    public List<ThrowableItemData> throwableItemData = new List<ThrowableItemData>();

    public ItemData GetDataFromIdentifier(string identifier)
    {
        foreach (ItemData item in itemData)
        {
            if(item.itemIdentifier == identifier)
            {
                return item;
            }
        }
        foreach (ItemData item in weaponItemData)
        {
            if (item.itemIdentifier == identifier)
            {
                return item;
            }
        }
        foreach (ItemData item in ammoItemData)
        {
            if (item.itemIdentifier == identifier)
            {
                return item;
            }
        }
        foreach (ItemData item in equipmentItemData)
        {
            if (item.itemIdentifier == identifier)
            {
                return item;
            }
        }
        foreach (ItemData item in consumableItemData)
        {
            if (item.itemIdentifier == identifier)
            {
                return item;
            }
        }
        foreach (ItemData item in throwableItemData)
        {
            if (item.itemIdentifier == identifier)
            {
                return item;
            }
        }
        Debug.Log($"Cant find {identifier}");
        return null;
    }
}
