using System;
using UnityEngine;

public class ContainerSlot : MonoBehaviour
{
    Container parentContainer;
    int slotIndex;

    public ItemStack storedStack;

    [SerializeField] WorldItem WorldItemPrefab;
    [SerializeField] WorldItem spawnedWorldItem;

    public static Action<ContainerSlot> onContainerItemGrabbed;

    public void InitSlot(ItemStack stackToInit, Container _parentContainer, int _slotIndex)
    {
        slotIndex = _slotIndex;

        parentContainer = _parentContainer;
        storedStack = stackToInit;

        SpawnWorldItem();
    }

    public void SpawnWorldItem()
    {
        spawnedWorldItem = Instantiate(WorldItemPrefab, transform);
        spawnedWorldItem.InitContainerWorldItem(storedStack, this);
    }

    public void ClearSlot()
    {
        spawnedWorldItem = null;

        storedStack.itemData = null;
        storedStack.itemAmount = 0;
        storedStack.loadedAmmo = 0;

        parentContainer.RemoveStoredItemFromSlot(slotIndex);
    }
}
