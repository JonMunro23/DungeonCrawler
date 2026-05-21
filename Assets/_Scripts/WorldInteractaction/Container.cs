using DG.Tweening;
using HighlightPlus;
using System;
using System.Collections.Generic;
using UnityEngine;
using static SaveableLevelData;

[System.Serializable]
public class ContainerItemStack
{
    public int containerSlotIndex;
    public ItemStack itemStack;

    public ContainerItemStack(int containerIndex, ItemStack itemStack)
    {
        this.containerSlotIndex = containerIndex;
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

    public static event Action onContainerOpened;
    public static event Action onContainerClosed;

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
        storedItemStacks.Add(itemStackToAdd.containerSlotIndex, itemStackToAdd.itemStack);
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

    public List<ItemStackSaveData> GetStoredItemsSaveData()
    {
        List<ContainerItemStack> storedContainerItemStacks = GetStoredItems();
        List<ItemStackSaveData> itemStackSaveDatas = new List<ItemStackSaveData>();
        foreach (ContainerItemStack containerItemStack in storedContainerItemStacks)
        {
            ItemStackSaveData itemStackSaveData = new ItemStackSaveData
            {
                itemID = containerItemStack.itemStack.Item.ItemData.itemIdentifier,
                amount = containerItemStack.itemStack.ItemAmount,
                slotIndex = containerItemStack.containerSlotIndex
            };

            if (containerItemStack.itemStack.Item is WeaponItem weaponItem)
            {
                itemStackSaveData.isWeapon = true;
                itemStackSaveData.loadedAmmoType = weaponItem.LoadedAmmoData != null
                    ? weaponItem.LoadedAmmoData.itemIdentifier
                    : "";
                itemStackSaveData.loadedAmmo = weaponItem.LoadedAmmo;
            }

            itemStackSaveDatas.Add(itemStackSaveData);
        }

        return itemStackSaveDatas;
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
