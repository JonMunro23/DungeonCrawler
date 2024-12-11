using System;
using UnityEngine;

public class ContainerSlot : MonoBehaviour, IPickup
{
    Container parentContainer;
    int slotIndex;

    public ItemStack storedStack;

    [SerializeField] GameObject spawnedWorldItem;

    public static Action<ContainerSlot> onContainerItemGrabbed;

    public void InitSlot(ItemStack stackToInit, Container _parentContainer, int slotIndex)
    {
        parentContainer = _parentContainer;
        storedStack = stackToInit;

        SpawnWorldItem();
    }

    public void SpawnWorldItem()
    {
        spawnedWorldItem = Instantiate(storedStack.itemData.itemWorldModel, transform);
    }

    public void RemoveItemStack()
    {
        if(spawnedWorldItem)
            Destroy(spawnedWorldItem);

        storedStack.itemData = null;
        storedStack.itemAmount = 0;
        storedStack.loadedAmmo = 0;

        parentContainer.RemoveStoredItemFromSlot(slotIndex);
    }

    public void Pickup(bool wasGrabbed = false)
    {
        if (storedStack.itemData == null)
            return;

        onContainerItemGrabbed?.Invoke(this);
    }
}
