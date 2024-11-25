using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerSlot : MonoBehaviour
{
    [SerializeField] ItemStack storedStack;

    [SerializeField] GameObject spawnedWorldItem;

    public void InitSlot(ItemStack stackToInit)
    {
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
    }
}
