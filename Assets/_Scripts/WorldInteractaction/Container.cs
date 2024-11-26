using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour, IContainer
{
    [SerializeField] Grid grid;
    [SerializeField] ContainerSlot containerSlotPrefab;
    const int X_NUMSLOTS = 4, Y_NUMSLOTS = 2;
    bool isOpen;

    [SerializeField] List<ItemStack> storedItems = new List<ItemStack>();

    [Header("Animation")]
    [SerializeField] Transform lidTransform;
    [SerializeField] Vector3 openRot, closedRot;
    [SerializeField] float openDuration;

    public static Action onContainerOpened;
    public static Action onContainerClosed;

    // Start is called before the first frame update
    public void InitContainer()
    {
        GenerateSlots();
    }

    void GenerateSlots()
    {
        int index = 0;
        for (int i = 0; i < X_NUMSLOTS; i++)
        {
            for (int j = 0; j < Y_NUMSLOTS; j++)
            {
                ContainerSlot clone = Instantiate(containerSlotPrefab, grid.GetCellCenterWorld(new Vector3Int(i, j)), Quaternion.identity, grid.transform);
                if (storedItems.Count - 1 >= index)
                    if (storedItems[index] != null)
                        clone.InitSlot(storedItems[index], this, index);

                index++;
            }
        }
    }

    public void AddNewStoredItem(ItemStack itemStackToAdd)
    {
        storedItems.Add(itemStackToAdd);
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
}
