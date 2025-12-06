using DG.Tweening;
using HighlightPlus;
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

    [SerializeField] Dictionary<int, ItemStack> storedItemStacks = new Dictionary<int, ItemStack>();

    [Header("Animation")]
    [SerializeField] Transform lidTransform;
    [SerializeField] Vector3 openRot, closedRot;
    [SerializeField] float openDuration;

    HighlightEffect highlightEffect;
    BoxCollider boxCollider;

    public static Action onContainerOpened;
    public static Action onContainerClosed;

    private void Awake()
    {
        highlightEffect = GetComponent<HighlightEffect>();
        boxCollider = GetComponent<BoxCollider>();
    }

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
                if(storedItemStacks.TryGetValue(index, out ItemStack stack))
                {
                    clone.InitSlot(stack, this, index);
                }
                //foreach (ContainerItemStack itemStack in storedItemStacks.Values)
                //{
                //    if(itemStack.containerIndex == index)
                //    {
                //        clone.InitSlot(itemStack.itemStack, this, index);
                //    }
                //}
                index++;
            }
        }
    }

    public void AddNewStoredItemStack(ContainerItemStack itemStackToAdd)
    {
        storedItemStacks.Add(itemStackToAdd.containerIndex, itemStackToAdd.itemStack);
    }

    public void RemoveStoredItemFromSlot(int slotIndex)
    {
        storedItemStacks.Remove(slotIndex);
    }

    void OpenContainer()
    {
        lidTransform.DOLocalRotate(openRot, openDuration);
        onContainerOpened?.Invoke();
        boxCollider.enabled = false;
        SetHighlighted(false);
    }

    public void CloseContainer()
    {
        if (!isOpen)
            return;

        isOpen = false;
        lidTransform.DOLocalRotate(closedRot, openDuration);
        boxCollider.enabled = true;
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
        List<ContainerItemStack > storedItems = new List<ContainerItemStack>();
        foreach (int index in storedItemStacks.Keys)
        {
            storedItems.Add(new ContainerItemStack(index, storedItemStacks[index]));
        }

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

    public void LoadContainerItemStacks(List<ContainerItemStack> itemStacks)
    {
        foreach (ContainerItemStack itemStack in itemStacks)
        {
            AddNewStoredItemStack(itemStack);
        }
    }

    public float GetRotation()
    {
        return transform.localRotation.eulerAngles.y;
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }

    public void SetHighlighted(bool isHighlighted)
    {
        highlightEffect.highlighted = isHighlighted;
    }
}
