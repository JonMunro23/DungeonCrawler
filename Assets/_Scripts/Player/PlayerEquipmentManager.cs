using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    [Header("Equipped Items")]
    [SerializeField] List<EquippedItem> allCurrentlyEquippedItems = new List<EquippedItem>();

    [Header("Carry Weight")]
    [SerializeField] float currentCarryWeight, maxCarryWeight;

    public static Action<EquippedItem> onEquippedItemAdded;
    public static Action<EquippedItem> onEquippedItemRemoved;

    private void OnEnable()
    {
        EquipmentSlot.onNewEquipmentItem += OnNewEquipmentItem;
        EquipmentSlot.onEquipmentItemRemoved += OnEquipmentItemRemoved;

        WorldInteraction.OnWorldInteraction += OnWorldInteraction;
    }

    private void OnDisable()
    {
        EquipmentSlot.onNewEquipmentItem -= OnNewEquipmentItem;
        EquipmentSlot.onEquipmentItemRemoved -= OnEquipmentItemRemoved;

        WorldInteraction.OnWorldInteraction -= OnWorldInteraction;
    }

    

    void OnWorldInteraction()
    {

        //if(currentRightHandWeapon != null)
        //{
        //    currentRightHandWeapon.Grab();
        //}
        //else if(currentLeftHandWeapon != null)
        //{
        //    currentLeftHandWeapon.Grab();
        //}
    }


    void OnNewEquipmentItem(EquipmentSlotType slotType, EquipmentItemData newEquipmentItemData)
    {
        EquippedItem newEquippedItem = new EquippedItem(slotType, newEquipmentItemData);
        allCurrentlyEquippedItems.Add(newEquippedItem);
        CalculateNewCurrentWeight(newEquipmentItemData.itemWeight);
        onEquippedItemAdded?.Invoke(newEquippedItem);
    }

    void OnEquipmentItemRemoved(EquipmentSlotType slotType)
    {
        EquippedItem itemInSlot = GetEquippedItemInSlot(slotType);
        if(itemInSlot != null)
        {
            CalculateNewCurrentWeight(-itemInSlot.equipmentItemData.itemWeight);

            if (allCurrentlyEquippedItems.Contains(itemInSlot))
                allCurrentlyEquippedItems.Remove(itemInSlot);

            onEquippedItemRemoved?.Invoke(itemInSlot);
        }
    }

    EquippedItem GetEquippedItemInSlot(EquipmentSlotType slot)
    {
        EquippedItem itemToReturn = null;
        foreach (EquippedItem item in allCurrentlyEquippedItems)
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



}
