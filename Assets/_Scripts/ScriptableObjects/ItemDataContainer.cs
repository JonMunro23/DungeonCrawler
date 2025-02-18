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

    public ItemData GetDataFromIdentifier(string identifier)
    {
        foreach (ItemData item in itemData)
        {
            if(item.itemIdentifier == identifier)
            {
                return item;
            }
        }
        Debug.Log($"Cant find {identifier}");
        return null;
    }
}
