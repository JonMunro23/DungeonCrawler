using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventoryManager : MonoBehaviour, IInventory
{
    PlayerController playerController;
    [SerializeField]
    List<ItemStack> startingItemStacks = new List<ItemStack>(); 

    [SerializeField] InventorySlot slotToSpawn;
    InventorySlot syringeSlot;
    public InventorySlot[] spawnedInventorySlots;
    [SerializeField] int totalNumInventorySlots;
    public bool isOpen { get; private set; }
    public bool isInContainer { get; private set; }

    [SerializeField] int heldHealthSyringes, heldPistolAmmo, heldRifleAmmo, heldShells;

    [SerializeField] Vector3 openContainerCamPos, defaultCamPos;
    [SerializeField] Vector3 openContainerCamRot, defaultCamRot;
    [SerializeField] float openContainerCamMovementDuration;

    public static Action onInventoryOpened;
    public static Action onInventoryClosed;
    public static Action<InventorySlot[]> onInventorySlotsSpawned;

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
        SetCursorActive(true);
    }

    void OnContainerClosed()
    {
        playerController.MoveCameraPos(defaultCamPos, openContainerCamMovementDuration);
        playerController.RotCamera(defaultCamRot, openContainerCamMovementDuration);

        isInContainer = false;
        SetCursorActive(false);
    }

    public void InitInventory(PlayerController newPlayerController)
    {
        playerController = newPlayerController;

        SetCursorActive(false);
        SpawnInventorySlots();
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

        for (int i = 0;i < startingItemStacks.Count;i++)
        {
            spawnedInventorySlots[i].AddItem(startingItemStacks[i]);
        }

        onInventorySlotsSpawned?.Invoke(spawnedInventorySlots);
    }

    public void ToggleInventory()
    {
        if (isOpen == true)
        {
            CloseInventory();
        }
        else if (isOpen == false)
        {
            OpenInventory();
        }
    }

    private void OpenInventory()
    {
        isOpen = true;
        SetCursorActive(true);
        onInventoryOpened?.Invoke();
    }

    public void CloseInventory()
    {
        isOpen = false;
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

    public void AddHealthSyringe(int amountToAdd) => heldHealthSyringes += amountToAdd;
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

    public void RemoveHealthSyringe(int amountToRemove) => heldHealthSyringes -= amountToRemove;

    async public void TryUseHealthSyringe()
    {
        syringeSlot = FindConsumableOfType(ConsumableType.HealSyringe);
        if(syringeSlot)
        {
            ConsumableItemData consumableData = syringeSlot.currentSlotItemStack.itemData as ConsumableItemData;
            if (!consumableData)
                return;

            IWeapon currentWeapon = playerController.playerWeaponManager.currentWeapon;
            if (currentWeapon != null)
                await playerController.playerWeaponManager.currentWeapon.HolsterWeapon();
        }
    }

    private InventorySlot FindConsumableOfType(ConsumableType typeToFind)
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

    public int GetRemainingHealthSyringes()
    {
        throw new NotImplementedException();
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
}
