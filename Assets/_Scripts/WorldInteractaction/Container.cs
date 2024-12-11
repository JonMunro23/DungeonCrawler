using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ContainerItemStack
{
    public int containerIndex;
    public ItemStack itemStack;

    public ContainerItemStack(int containerIndex, ItemStack itemStack)
    {
        this.containerIndex = containerIndex;
        this.itemStack = itemStack;
    }
}

public class Container : MonoBehaviour, IContainer
{
    int levelIndex;
    Vector2 coords;

    [SerializeField] Grid containerGrid;
    [SerializeField] ContainerSlot containerSlotPrefab;
    const int X_NUMSLOTS = 4, Y_NUMSLOTS = 2;
    bool isOpen;

    [SerializeField] List<ContainerItemStack> storedItems = new List<ContainerItemStack>();

    [Header("Animation")]
    [SerializeField] Transform lidTransform;
    [SerializeField] Vector3 openRot, closedRot;
    [SerializeField] float openDuration;

    public static Action onContainerOpened;
    public static Action onContainerClosed;

    public void InitContainer(int _levelIndex, Vector2 _coords)
    {
        levelIndex = _levelIndex;
        coords = _coords;

        GenerateSlots();
    }

    void GenerateSlots()
    {
        int index = 0;
        for (int i = 0; i < X_NUMSLOTS; i++)
        {
            for (int j = 0; j < Y_NUMSLOTS; j++)
            {
                ContainerSlot clone = Instantiate(containerSlotPrefab, containerGrid.GetCellCenterWorld(new Vector3Int(-i, j)), Quaternion.identity, containerGrid.transform);
                if (storedItems.Count - 1 >= index)
                    if (storedItems[index] != null)
                        clone.InitSlot(storedItems[index].itemStack, this, index);

                index++;
            }
        }
    }

    public void AddNewStoredItem(int containerIndex, ItemStack itemStackToAdd)
    {
        storedItems.Add(new ContainerItemStack(containerIndex, itemStackToAdd));
    }

    public void RemoveStoredItemFromSlot(int slotIndex)
    {
        storedItems.RemoveAt(slotIndex);
    }

    void OpenContainer()
    {
        lidTransform.DOLocalRotate(openRot, openDuration);
        onContainerOpened?.Invoke();
    }

    public void CloseContainer()
    {
        if (!isOpen)
            return;

        isOpen = false;
        lidTransform.DOLocalRotate(closedRot, openDuration);
        onContainerClosed?.Invoke();
    }

    public void ToggleContainer()
    {
        if (isOpen)
        {
            CloseContainer();
        }
        else
        {
            isOpen = true;
            OpenContainer();
        }

    }

    public bool IsOpen()
    {
        return isOpen;
    }

    public List<ContainerItemStack> GetStoredItems()
    {
        return storedItems;
    }

    public Vector2 GetCoords()
    {
        return coords;
    }

    public int GetLevelIndex()
    {
        return levelIndex;
    }
}
