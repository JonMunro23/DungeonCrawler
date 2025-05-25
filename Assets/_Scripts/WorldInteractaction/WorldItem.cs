using HighlightPlus;
using System;
using UnityEngine;

[System.Serializable]
public class ItemStack
{
    public ItemData itemData;
    public int itemAmount = 1;
    public int loadedAmmo = 0;

    public ItemStack(ItemData itemData, int itemAmount = 1, int loadedAmmo = 0)
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

[SelectionBase]
public class WorldItem : MonoBehaviour, IPickup
{
    public int levelIndex;

    public ItemStack item;
    public Vector2 coords;
    public PressurePlate occupiedPressurePlate;
    public static Action<WorldItem> onWorldItemGrabbed;
    public static Action<WorldItem> onWorldItemPickedUp;

    public bool isInContainer;
    ContainerSlot occupiedContainerSlot;

    [SerializeField] HighlightEffect highlightEffect;

    public void InitWorldItem(int _levelIndex, Vector2 _coords, ItemStack itemToInitialise)
    {
        levelIndex = _levelIndex;
        coords = _coords;

        item.itemData = itemToInitialise.itemData;
        item.itemAmount = itemToInitialise.itemAmount;
        item.loadedAmmo = itemToInitialise.loadedAmmo;

        SpawnMesh();
    }

    public void InitContainerWorldItem(ItemStack stackToInitialise, ContainerSlot occupiedContainerSlot)
    {
        isInContainer = true;
        this.occupiedContainerSlot = occupiedContainerSlot;

        item.itemData = stackToInitialise.itemData;
        item.itemAmount = stackToInitialise.itemAmount;
        item.loadedAmmo = stackToInitialise.loadedAmmo;

        SpawnMesh();
    }
    void SpawnMesh()
    {
        GameObject clone = Instantiate(item.itemData.itemWorldModel, transform);
        clone.transform.localPosition = new Vector3(0, 0, isInContainer ? 0 : 1.3f);
        
        if(isInContainer)
        {
            BoxCollider boxCollider = GetComponent<BoxCollider>();
            boxCollider.center = Vector3.zero;
            boxCollider.size = Vector3.one;

        }
    }

    public void Pickup(bool wasGrabbed = false)
    {
        if(occupiedPressurePlate != null)
        {
            occupiedPressurePlate.RemoveGameobjectFromPlate(gameObject);
        }

        if (isInContainer)
            occupiedContainerSlot.ClearSlot();

        if(wasGrabbed)
            onWorldItemGrabbed?.Invoke(this);
        else
            onWorldItemPickedUp?.Invoke(this);

    }

    public void SetHighlighted(bool isHighlighted)
    {
        if(highlightEffect != null)
            highlightEffect.highlighted = isHighlighted;
    }

    public void AddToInventory(IInventory inventoryToAddTo)
    {
        int remainingItems = inventoryToAddTo.TryAddItem(item);
        if(remainingItems == 0)
        {
            if (occupiedPressurePlate != null)
            {
                occupiedPressurePlate.RemoveGameobjectFromPlate(gameObject);
            }

            Destroy(gameObject);
        }
    }
}
