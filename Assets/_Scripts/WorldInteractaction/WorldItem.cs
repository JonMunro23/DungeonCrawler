using HighlightPlus;
using System;
using UnityEngine;

[SelectionBase]
public class WorldItem : MonoBehaviour, IPickup
{
    public int levelIndex;

    public ItemStack itemStack;
    public PressurePlate occupiedPressurePlate;
    public static event Action<WorldItem> onWorldItemGrabbed;
    public static event Action<WorldItem> onWorldItemPickedUp;

    public bool isInContainer;
    ContainerSlot occupiedContainerSlot;

    [SerializeField] HighlightEffect highlightEffect;

    public void InitWorldItem(int _levelIndex, ItemStack itemToInitialise)
    {
        levelIndex = _levelIndex;
        itemStack = itemToInitialise;

        SpawnMesh();
    }

    public void InitContainerWorldItem(ItemStack itemToInitialise, ContainerSlot occupiedContainerSlot)
    {
        isInContainer = true;
        this.occupiedContainerSlot = occupiedContainerSlot;
        GetComponent<Rigidbody>().isKinematic = true;
        itemStack = itemToInitialise;

        SpawnMesh();
    }
    void SpawnMesh()
    {
        GameObject clone = Instantiate(itemStack.Item.ItemData.itemWorldModel, transform);
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
        int remainingItems = inventoryToAddTo.TryAddItem(itemStack);
        if(remainingItems == 0)
        {
            if (occupiedPressurePlate != null)
            {
                occupiedPressurePlate.RemoveGameobjectFromPlate(gameObject);
            }

            if (isInContainer)
                occupiedContainerSlot.ClearSlot();

            onWorldItemPickedUp?.Invoke(this);

            Destroy(gameObject);
        }
    }

    public Vector2Int GetCoords() => GridController.Instance.GetNodeFromWorldPos(transform.position).Coords.Pos;

}
