using System;
using UnityEngine;

[System.Serializable]
public class Item
{
    public ItemData itemData;
    public int itemAmount = 1;  

    public Item(ItemData itemData, int itemAmount)
    {
        this.itemData = itemData;
        this.itemAmount = itemAmount;
    }
}

public class WorldItem : MonoBehaviour, IPickup
{
    public Item item;
    public bool isOnPressurePlate;

    public static Action<WorldItem> onWorldItemGrabbed;

    public void InitWorldItem(Item itemToInitialise)
    {
        item.itemData = itemToInitialise.itemData;
        item.itemAmount = itemToInitialise.itemAmount;
    }

    public void GrabPickup()
    {
        Debug.Log($"Grabbed {item.itemData.itemName}");
        onWorldItemGrabbed?.Invoke(this);
    }
}
