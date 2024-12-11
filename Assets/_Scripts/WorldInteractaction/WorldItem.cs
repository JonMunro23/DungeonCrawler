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
    public int levelIndex;

    public ItemStack item;
    public Vector2 coords;

    public bool isOnPressurePlate;
    public static Action<WorldItem> onWorldItemGrabbed;
    public static Action<WorldItem> onWorldItemPickedUp;

    public void InitWorldItem(int _levelIndex, Vector2 _coords, ItemStack itemToInitialise)
    {
        levelIndex = _levelIndex;
        coords = _coords;

        item.itemData = itemToInitialise.itemData;
        item.itemAmount = itemToInitialise.itemAmount;
        item.loadedAmmo = itemToInitialise.loadedAmmo;

        SpawnMesh();
    }

    void SpawnMesh()
    {
        Instantiate(item.itemData.itemWorldModel, transform);
    }

    public void Pickup(bool wasGrabbed = false)
    {
        //Debug.Log($"Grabbed {item.itemData.itemName}");
        if(wasGrabbed)
            onWorldItemGrabbed?.Invoke(this);
        else
            onWorldItemPickedUp?.Invoke(this);
    }
}
