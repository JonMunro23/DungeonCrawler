using System;
using UnityEngine;

[System.Serializable]
public class ItemStack
{
    public ItemData itemData;
    public int itemAmount = 1;
    public int loadedAmmo = 0;

    public ItemStack(ItemData itemData, int itemAmount, int loadedAmmo = 0)
    {
        this.itemData = itemData;
        this.itemAmount = itemAmount;
        this.loadedAmmo = loadedAmmo;
    }

    public int GetRemainingSpaceInStack()
    {
        return itemData.maxItemStackSize - itemAmount;
    }
}

public class WorldItem : MonoBehaviour, IPickup
{
    public ItemStack item;
    public bool isOnPressurePlate;

    public static Action<WorldItem> onWorldItemGrabbed;

    public void InitWorldItem(ItemStack itemToInitialise)
    {
        item.itemData = itemToInitialise.itemData;
        item.itemAmount = itemToInitialise.itemAmount;
        item.loadedAmmo = itemToInitialise.loadedAmmo;

        SpawnMesh();
    }

    void SpawnMesh()
    {
        Instantiate(item.itemData.itemWorldModel, transform);
    }

    public void GrabPickup()
    {
        //Debug.Log($"Grabbed {item.itemData.itemName}");
        onWorldItemGrabbed?.Invoke(this);
    }
}
