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
    [SerializeField] float currentWeight, maxWeight;
    [SerializeField] List<EquippedItem> equippedItems = new List<EquippedItem>();

    public static Action<EquippedItem> onEquippedItemAdded;
    public static Action<EquippedItem> onEquippedItemRemoved;
    private void OnEnable()
    {
        EquipmentSlot.onNewEquipmentItem += OnNewEquipmentItem;
        EquipmentSlot.onEquipmentItemRemoved += OnEquipmentItemRemoved;
    }

    private void OnDisable()
    {
        EquipmentSlot.onNewEquipmentItem -= OnNewEquipmentItem;
        EquipmentSlot.onEquipmentItemRemoved -= OnEquipmentItemRemoved;
    }

    void OnNewEquipmentItem(EquipmentSlotType slotType, EquipmentItemData newEquipmentItemData)
    {
        EquippedItem newEquippedItem = new EquippedItem(slotType, newEquipmentItemData);
        equippedItems.Add(newEquippedItem);
        CalculateNewCurrentWeight(newEquipmentItemData.itemWeight);
        onEquippedItemAdded?.Invoke(newEquippedItem);
    }

    void OnEquipmentItemRemoved(EquipmentSlotType slotType)
    {
        EquippedItem itemInSlot = GetEquippedItemInSlot(slotType);
        CalculateNewCurrentWeight(-itemInSlot.equipmentItemData.itemWeight);

        if (equippedItems.Contains(itemInSlot))
            equippedItems.Remove(itemInSlot);

        onEquippedItemRemoved?.Invoke(itemInSlot);
    }

    EquippedItem GetEquippedItemInSlot(EquipmentSlotType slot)
    {
        EquippedItem itemToReturn = null;
        foreach (EquippedItem item in equippedItems)
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
        currentWeight += newAddedWeight;
        //check if overencucumbered
    }
}
