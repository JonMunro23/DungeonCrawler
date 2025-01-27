using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EquippedItem
{
    public EquipmentSlotType slotType;
    public EquipmentItemData equipmentItemData;

    public EquippedItem(EquipmentSlotType slotType, EquipmentItemData equipmentItemData)
    {
        this.slotType = slotType;
        this.equipmentItemData = equipmentItemData;
    }
}

public class PlayerEquipmentManager : MonoBehaviour
{
    PlayerController playerController;

    [SerializeField] EquipmentSlot equipmentSlotPrefab;
    [SerializeField] List<EquipmentSlot> spawnedEquipmentSlots = new List<EquipmentSlot>();

    [Header("Equipped Items")]
    [SerializeField] List<EquippedItem> currentlyEquippedItems = new List<EquippedItem>();

    [Header("Carry Weight")]
    [SerializeField] float currentCarryWeight, maxCarryWeight;

    public static Action<EquippedItem> onEquippedItemAdded;
    public static Action<EquippedItem> onEquippedItemRemoved;

    public static Action<List<EquipmentSlot>> onEquipmentSlotsSpawned;

    private void OnEnable()
    {
        EquipmentSlot.onNewEquipmentItem += EquipNewtem;
        EquipmentSlot.onEquipmentItemRemoved += RemoveEquippedItem;

        InventoryContextMenu.onInventorySlotEquipmentItemEquipped += OnInventorySlotEquipmentItemEquipped;
        InventoryContextMenu.onInventorySlotEquipmentItemUnequipped += OnInventorySlotEquipmentItemUnequipped;

        PauseMenu.onQuit += RemoveEquipmentSlots;
    }

    private void OnDisable()
    {
        EquipmentSlot.onNewEquipmentItem -= EquipNewtem;
        EquipmentSlot.onEquipmentItemRemoved -= RemoveEquippedItem;

        InventoryContextMenu.onInventorySlotEquipmentItemEquipped -= OnInventorySlotEquipmentItemEquipped;
        InventoryContextMenu.onInventorySlotEquipmentItemUnequipped -= OnInventorySlotEquipmentItemUnequipped;

        PauseMenu.onQuit -= RemoveEquipmentSlots;
    }

    void OnInventorySlotEquipmentItemEquipped(ISlot slot)
    {
        EquipmentItemData equipmentItemData = slot.GetItemStack().itemData as EquipmentItemData;
        if (equipmentItemData)
        {
            EquippedItem currentlyEquippedItem = GetEquippedItemInSlot(equipmentItemData.EquipmentSlotType);
            if (currentlyEquippedItem == null)
            {
                GetSlotOfType(equipmentItemData.EquipmentSlotType).AddItem(slot.TakeItem());
            }
            else
            {
                slot.AddItem(GetSlotOfType(currentlyEquippedItem.slotType).SwapItem(slot.GetItemStack()));
            }

        }
    }

    void OnInventorySlotEquipmentItemUnequipped(ISlot slot)
    {
        playerController.playerInventoryManager.TryAddItemToInventory(slot.TakeItem());
    }

    public void Init(PlayerController controller)
    {
        playerController = controller;
        SpawnEquipmentSlots();
    }

    void SpawnEquipmentSlots()
    {
        for (int i = 0; i < 5; i++)
        {
            var clone = Instantiate(equipmentSlotPrefab);
            spawnedEquipmentSlots.Add(clone);
        }

        onEquipmentSlotsSpawned.Invoke(spawnedEquipmentSlots);
    }

    void RemoveEquipmentSlots()
    {
        foreach (var slot in spawnedEquipmentSlots)
        {
            Destroy(slot.gameObject);
        }
        spawnedEquipmentSlots.Clear();
    }

    void EquipNewtem(EquipmentSlotType slotType, EquipmentItemData newEquipmentItemData)
    {
        EquippedItem newEquippedItem = new EquippedItem(slotType, newEquipmentItemData);
        currentlyEquippedItems.Add(newEquippedItem);
        CalculateNewCurrentWeight(newEquipmentItemData.itemWeight);
        onEquippedItemAdded?.Invoke(newEquippedItem);
    }

    void RemoveEquippedItem(EquipmentSlotType slotType)
    {
        EquippedItem itemInSlot = GetEquippedItemInSlot(slotType);
        if(itemInSlot != null)
        {
            CalculateNewCurrentWeight(-itemInSlot.equipmentItemData.itemWeight);

            if (currentlyEquippedItems.Contains(itemInSlot))
                currentlyEquippedItems.Remove(itemInSlot);

            onEquippedItemRemoved?.Invoke(itemInSlot);
        }
        
    }

    void RemoveAllEquippedItems()
    {
        if (currentlyEquippedItems.Count == 0)
            return;

        //for (int i = currentlyEquippedItems.Count - 1; i >= 0; i--)
        //{
        //    RemoveEquippedItem(currentlyEquippedItems[i].slotType);
        //}

        foreach (var slot in spawnedEquipmentSlots)
        {
            slot.RemoveItemStack();
        }
    }

    EquippedItem GetEquippedItemInSlot(EquipmentSlotType slot)
    {
        EquippedItem itemToReturn = null;
        foreach (EquippedItem item in currentlyEquippedItems)
        {
            if(item.slotType == slot)
            {
                itemToReturn = item;
                break;
            }
        }
        return itemToReturn;
    }

    void CalculateNewCurrentWeight(float newAddedWeight)
    {
        currentCarryWeight += newAddedWeight;
        //check if overencucumbered
    }

    EquipmentSlot GetSlotOfType(EquipmentSlotType slotType)
    {
        foreach (EquipmentSlot slot in spawnedEquipmentSlots)
        {
            if(slot.slotType == slotType)
                return slot;
        }

        return null;
    }

    void LoadEquippedItems(List<EquippedItem> equippedItems)
    {
        foreach (EquippedItem item in equippedItems)
        {
            EquipmentSlot slot = GetSlotOfType(item.slotType);
            if (!slot.IsSlotEmpty())
                slot.RemoveItemStack();


            slot.AddItem(new ItemStack(item.equipmentItemData, 1));
        }
    }

    public void Save(ref PlayerSaveData data)
    {
        data.equippedItems = currentlyEquippedItems;
    }

    public void Load(PlayerSaveData data)
    {
        RemoveAllEquippedItems();
        LoadEquippedItems(data.equippedItems);
    }
}
