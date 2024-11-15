using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventoryManager : MonoBehaviour
{
    PlayerController playerController;

    [SerializeField] InventorySlot slotToSpawn;
    InventorySlot syringeSlot;
    public InventorySlot[] spawnedInventorySlots;
    [SerializeField] int totalNumInventorySlots;
    [SerializeField] bool isOpen;

    [SerializeField] int heldHealthSyringes;

    public static Action onInventoryOpened;
    public static Action onInventoryClosed;
    public static Action<InventorySlot[]> onInventorySlotsSpawned;

    Action onWeaponsHolsteredCallback;

    private void OnEnable()
    {
        onWeaponsHolsteredCallback += OnWeaponsHolstered;
    }

    private void OnDisable()
    {
        onWeaponsHolsteredCallback -= OnWeaponsHolstered;
    }

    public void InitInventory(PlayerController newPlayerController)
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        playerController = newPlayerController;

        SpawnInventorySlots();
    }

    void SpawnInventorySlots()
    {
        spawnedInventorySlots = new InventorySlot[totalNumInventorySlots];

        for (int i = 0; i < totalNumInventorySlots; i++)
        {
            InventorySlot spawnedSlot = Instantiate(slotToSpawn);
            spawnedInventorySlots[i] = spawnedSlot;
            spawnedSlot.InitSlot(this);
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
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        onInventoryOpened?.Invoke();
    }

    public void CloseInventory()
    {
        isOpen = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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

    public void RemoveHealthSyringe(int amountToRemove) => heldHealthSyringes -= amountToRemove;

    public void TryUseHealthSyringe()
    {
        syringeSlot = FindConsumableOfType(ConsumableType.HealSyringe);
        if(syringeSlot)
        {
            ConsumableItemData consumableData = syringeSlot.currentSlotItem.itemData as ConsumableItemData;
            if (!consumableData)
                return;

            
            playerController.playerEquipmentManager.HolsterWeapons(onWeaponsHolsteredCallback);
        }
    }

    private InventorySlot FindConsumableOfType(ConsumableType typeToFind)
    {
        foreach (InventorySlot slot in spawnedInventorySlots)
        {
            if (!slot.currentSlotItem.itemData)
                continue;

            ConsumableItemData consumableData = slot.currentSlotItem.itemData as ConsumableItemData;
            if (!consumableData)
                continue;

            if (consumableData.consumableType == typeToFind)
            {
                return slot;
            }
        }

        return null;
    }

    void OnWeaponsHolstered()
    {
        ConsumableItemData consumableData = syringeSlot.currentSlotItem.itemData as ConsumableItemData;
        if (!consumableData)
            return;

        playerController.playerHealthController.UseHealthSyringe(consumableData);
        syringeSlot.UseItem();
        heldHealthSyringes--;
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

    public List<InventorySlot> GetSlotsWithItem(ItemStack itemToCheck)
    {
        List<InventorySlot> slotsWithItem = new List<InventorySlot>();
        foreach (InventorySlot slot in spawnedInventorySlots)
        {
            if (!slot.currentSlotItem.itemData)
                continue;

            if (slot.currentSlotItem.itemData == itemToCheck.itemData)
                slotsWithItem.Add(slot);
        }

        return slotsWithItem;
    }

    InventorySlot[] GetSlotOfTypeWithSpace(ItemData itemData)
    {
        List<InventorySlot> slotsWithItemAndSpace = new List<InventorySlot>();
        foreach (InventorySlot slot in spawnedInventorySlots)
        {
            if (!slot.isSlotOccupied)
                continue;

            if(slot.currentSlotItem.itemData == itemData)
            {
                if(slot.currentSlotItem.GetRemainingSpaceInStack() > 0)
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
                int spaceInSlot = slot.currentSlotItem.GetRemainingSpaceInStack();
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
                    freeSlot.AddItem(new ItemStack(itemToAdd.itemData, remainingAmountToAdd));
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

}
