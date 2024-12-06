using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public struct PlayerInventorySaveData
{
    public List<ItemStack> storedItems;
}
public class PlayerInventoryManager : MonoBehaviour, IInventory
{
    PlayerController playerController;
    [SerializeField]
    List<ItemStack> startingItemStacks = new List<ItemStack>(); 

    [SerializeField] InventorySlot slotToSpawn;
    InventorySlot syringeSlot;
    public InventorySlot[] spawnedInventorySlots;
    [SerializeField] int totalNumInventorySlots;
    public bool isInventoryOpen { get; private set; }
    public bool isInContainer { get; private set; }

    [SerializeField] int heldHealthSyringes, heldPistolAmmo, heldRifleAmmo, heldShells;
    [Space]
    [Header("Camera Anim On Container Interaction")]
    [SerializeField] Vector3 openContainerCamPos, defaultCamPos;
    [SerializeField] Vector3 openContainerCamRot, defaultCamRot;
    [SerializeField] float openContainerCamMovementDuration, closeContainerCamMovementDuration;

    public static Action onInventoryOpened;
    public static Action onInventoryClosed;
    public static Action<InventorySlot[]> onInventorySlotsSpawned;
    public static Action<int> onSyringeCountUpdated;

    public static Action<AmmoType> onAmmoAddedToInventory;

    void OnEnable()
    {
        Container.onContainerOpened += OnContainerOpened;
        Container.onContainerClosed += OnContainerClosed;
    }

    void OnDisable()
    {
        Container.onContainerOpened -= OnContainerOpened;
        Container.onContainerClosed -= OnContainerClosed;
    }

    void OnContainerOpened()
    {
        playerController.MoveCameraPos(openContainerCamPos, openContainerCamMovementDuration);
        playerController.RotCamera(openContainerCamRot, openContainerCamMovementDuration);
        isInContainer = true;

        if (!isInventoryOpen)
            SetCursorActive(true);
    }

    void OnContainerClosed()
    {
        playerController.MoveCameraPos(defaultCamPos, closeContainerCamMovementDuration);
        playerController.RotCamera(defaultCamRot, closeContainerCamMovementDuration);

        isInContainer = false;

        if(!isInventoryOpen)
            SetCursorActive(false);
    }

    public void Init(PlayerController newPlayerController)
    {
        playerController = newPlayerController;

        SpawnInventorySlots();
        SetCursorActive(false);
    }

    public void SetCursorActive(bool isActive)
    {
        if (isActive)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    void SpawnInventorySlots()
    {
        spawnedInventorySlots = new InventorySlot[totalNumInventorySlots];

        for (int i = 0; i < totalNumInventorySlots; i++)
        {
            InventorySlot spawnedSlot = Instantiate(slotToSpawn);
            spawnedInventorySlots[i] = spawnedSlot;
            spawnedSlot.InitSlot(this, i);
        }

        AddStartingItems();

        onInventorySlotsSpawned?.Invoke(spawnedInventorySlots);
    }

    private void AddStartingItems()
    {
        for (int i = 0; i < startingItemStacks.Count; i++)
        {
            spawnedInventorySlots[i].AddItem(startingItemStacks[i]);
        }
    }

    public void ToggleInventory()
    {
        if (isInventoryOpen == true)
        {
            CloseInventory();
        }
        else if (isInventoryOpen == false)
        {
            OpenInventory();
        }
    }

    private void OpenInventory()
    {
        isInventoryOpen = true;
        SetCursorActive(true);
        onInventoryOpened?.Invoke();
    }

    public void CloseInventory()
    {
        isInventoryOpen = false;
        if(!isInContainer)
            SetCursorActive(false);

        onInventoryClosed?.Invoke();
    }

    public bool HasHealthSyringe()
    {
        if(heldHealthSyringes > 0)
            return true;
        else
            return false;
    }

    public void AddHealthSyringe(int amountToAdd)
    {
        heldHealthSyringes += amountToAdd;
        onSyringeCountUpdated?.Invoke(heldHealthSyringes);
    }
    public void AddAmmo(AmmoType typeToAdd, int amountToAdd)
    {
        switch (typeToAdd)
        {
            case AmmoType.Pistol:
                heldPistolAmmo += amountToAdd;
                break;
            case AmmoType.Rifle:
                heldRifleAmmo += amountToAdd;
                break;
            case AmmoType.Shells:
                heldShells += amountToAdd;
                break;
        }

        onAmmoAddedToInventory?.Invoke(typeToAdd);
    }
    public void RemoveAmmo(AmmoType typeToAdd, int amountToRemove)
    {
        switch (typeToAdd)
        {
            case AmmoType.Pistol:
                heldPistolAmmo -= amountToRemove;
                break;
            case AmmoType.Rifle:
                heldRifleAmmo -= amountToRemove;
                break;
            case AmmoType.Shells:
                heldShells -= amountToRemove;
                break;
        }

        onAmmoAddedToInventory?.Invoke(typeToAdd);
    }
    private void RemoveAllAmmo()
    {
        RemoveAmmo(AmmoType.Pistol, heldPistolAmmo);
        RemoveAmmo(AmmoType.Rifle, heldRifleAmmo);
        RemoveAmmo(AmmoType.Shells, heldShells);
    }
    
    public void RemoveHealthSyringe(int amountToRemove)
    {
        heldHealthSyringes -= amountToRemove;
        onSyringeCountUpdated?.Invoke(heldHealthSyringes);
    }

    public InventorySlot FindSlotWithConsumableOfType(ConsumableType typeToFind)
    {
        foreach (InventorySlot slot in spawnedInventorySlots)
        {
            if (!slot.currentSlotItemStack.itemData)
                continue;

            ConsumableItemData consumableData = slot.currentSlotItemStack.itemData as ConsumableItemData;
            if (!consumableData)
                continue;

            if (consumableData.consumableType == typeToFind)
            {
                return slot;
            }
        }

        return null;
    }

    public InventorySlot GetNextFreeSlot()
    {
        foreach(InventorySlot slot in spawnedInventorySlots)
        {
            if(!slot.isSlotOccupied)
                return slot;
        }

        return null;
    }

    InventorySlot[] GetSlotOfTypeWithSpace(ItemData itemData)
    {
        List<InventorySlot> slotsWithItemAndSpace = new List<InventorySlot>();
        foreach (InventorySlot slot in spawnedInventorySlots)
        {
            if (!slot.isSlotOccupied)
                continue;

            if(slot.currentSlotItemStack.itemData == itemData)
            {
                if(slot.currentSlotItemStack.GetRemainingSpaceInStack() > 0)
                {
                    slotsWithItemAndSpace.Add(slot);
                }
            }
        }

        if(slotsWithItemAndSpace.Count > 0)
            return slotsWithItemAndSpace.ToArray();

        return null;
    }

    public int TryAddItemToInventory(ItemStack itemToAdd)
    {
        InventorySlot[] slotsWithSpace = GetSlotOfTypeWithSpace(itemToAdd.itemData);
        if(slotsWithSpace != null && slotsWithSpace.Length > 0)
        {
            int remainingAmountToAdd = itemToAdd.itemAmount;
            foreach (InventorySlot slot in slotsWithSpace)
            {
                int spaceInSlot = slot.currentSlotItemStack.GetRemainingSpaceInStack();
                if (spaceInSlot > remainingAmountToAdd)
                {
                    slot.AddToCurrentItemStack(remainingAmountToAdd);
                    remainingAmountToAdd = 0;
                    return remainingAmountToAdd;
                }

                int amountToAdd = spaceInSlot;
                slot.AddToCurrentItemStack(amountToAdd);
                remainingAmountToAdd -= amountToAdd;
            }

            if(remainingAmountToAdd > 0)
            {
                InventorySlot freeSlot = GetNextFreeSlot();
                if (freeSlot)
                {
                    freeSlot.AddItem(new ItemStack(itemToAdd.itemData, remainingAmountToAdd, itemToAdd.loadedAmmo));
                    return 0;
                }

                return remainingAmountToAdd;
            }

            return 0;
        }
        else
        {
            InventorySlot freeSlot = GetNextFreeSlot();
            if (freeSlot)
            {
                freeSlot.AddItem(itemToAdd);
                return 0;
            }

            return itemToAdd.itemAmount;
        }
    }

    public int GetRemainingAmmoOfType(AmmoType ammoTypeToGet)
    {
        int ammoToReturn = 0;
        switch (ammoTypeToGet)
        {
            case AmmoType.Pistol:
                ammoToReturn += heldPistolAmmo;
                break;
            case AmmoType.Rifle:
                ammoToReturn += heldRifleAmmo;
                break;
            case AmmoType.Shells:
                ammoToReturn += heldShells;
                break;
        }
        return ammoToReturn;
    }

    public void DecreaseAmmoOfType(AmmoType ammoTypeToRemove, int amountToRemove)
    {
        //reverse list so it takes from the last slot first 
        List<InventorySlot> slotsReversed = new List<InventorySlot>(spawnedInventorySlots.Reverse());

        foreach (ISlot slot in slotsReversed)
        {
            if (slot.IsSlotEmpty())
                continue;

            ItemStack slotItemStack = slot.GetItemStack();

            ConsumableItemData consumableItemData = slotItemStack.itemData as ConsumableItemData;
            if (!consumableItemData)
                continue;

            if (consumableItemData.ammoType != ammoTypeToRemove)
                continue;

            int remainingAmountToRemove = slot.RemoveFromExistingStack(amountToRemove);
            RemoveAmmo(ammoTypeToRemove, amountToRemove);
            if (remainingAmountToRemove == 0)
                return;

            amountToRemove = remainingAmountToRemove;

        }
    }

    public void IncreaseAmmoOfType(AmmoType ammoTypeToAdd, int amountToAdd)
    {
        foreach (ISlot slot in spawnedInventorySlots)
        {
            if (slot.IsSlotEmpty())
                continue;

            ItemStack slotItemStack = slot.GetItemStack();

            if (slotItemStack.GetRemainingSpaceInStack() == 0)
                continue;

            ConsumableItemData consumableItemData = slotItemStack.itemData as ConsumableItemData;
            if (!consumableItemData)
                continue;

            if (consumableItemData.ammoType != ammoTypeToAdd)
                continue;


            int remainingAmountToAdd = slot.AddToCurrentItemStack(amountToAdd);
            if (remainingAmountToAdd > 0)
            {
                Debug.Log(remainingAmountToAdd);
                InventorySlot freeSlot = GetNextFreeSlot();
                if (freeSlot)
                {
                    freeSlot.AddItem(new ItemStack(consumableItemData, remainingAmountToAdd, 0));
                }
            }
        }
    }

    public void LockSlotsWithAmmoOfType(AmmoType ammoTypeToLock)
    {
        foreach(ISlot slot in spawnedInventorySlots)
        {
            if (slot.IsSlotEmpty())
                continue;

            ConsumableItemData consumableItemData = slot.GetItemStack().itemData as ConsumableItemData;
            if (!consumableItemData)
                continue;

            if(consumableItemData.ammoType == ammoTypeToLock)
                slot.SetInteractable(false);
        }
    }

    public void UnlockSlots()
    {
        foreach (ISlot slot in spawnedInventorySlots)
        {
            if (!slot.IsInteractable())
                slot.SetInteractable(true);
        }
    }

    #region Save/Load

    public List<ItemStack> GetStoredItems()
    {
        List<ItemStack> items = new List<ItemStack>();
        foreach (InventorySlot slot in spawnedInventorySlots)
        {
            if (slot.IsSlotEmpty())
                continue;

            items.Add(slot.GetItemStack());
        }
        return items;
    }

    public void LoadItems(List<ItemStack> items)
    {
        RemoveSyringes();
        RemoveAllAmmo();

        foreach (InventorySlot slot in spawnedInventorySlots)
        {
            if(!slot.IsSlotEmpty())
            {
                slot.RemoveItem();
            }
        }

        for (int i = 0; i < items.Count; i++)
        {
            spawnedInventorySlots[i].AddItem(items[i]);
        }
    }

    public void Save(ref PlayerInventorySaveData data)
    {
        data.storedItems = GetStoredItems();
    }

    public void Load(PlayerInventorySaveData data)
    {
        LoadItems(data.storedItems);
    }

    #endregion
}
