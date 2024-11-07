using System;
using UnityEngine;

public class EquipmentSlot : InventorySlot
{
    public EquipmentSlotType slotType;

    public static Action<EquipmentSlotType, EquipmentItemData> onNewEquipmentItem;
    public static Action<EquipmentSlotType> onEquipmentItemRemoved;

    public override void AddItem(Item itemToAdd)
    {
        base.AddItem(itemToAdd);
        InitialiseItem(currentSlotItem);
    }

    public override Item SwapItem(Item itemToSwap)
    {
        Item itemToReturn = base.SwapItem(itemToSwap);
        DeinitialiseItem(itemToReturn);
        InitialiseItem(itemToSwap);
        return itemToReturn;
    }

    public override Item TakeItem()
    {
        Item itemToTake = base.TakeItem();
        DeinitialiseItem(itemToTake);
        return itemToTake;

    }

    public void InitialiseItem(Item itemToInitialise)
    {
        HandItemData handItemData = itemToInitialise.itemData as HandItemData;
        if(handItemData)
        {
            if (slotType == EquipmentSlotType.leftHand)
            {
                onNewHandItem?.Invoke(EquipmentSlotType.leftHand, handItemData);
            }
            else
                onNewHandItem?.Invoke(EquipmentSlotType.rightHand, handItemData);
        }

        onNewEquipmentItem?.Invoke(slotType, itemToInitialise.itemData as EquipmentItemData);
    }

    public void DeinitialiseItem(Item item)
    {
        HandItemData handItemData = item.itemData as HandItemData;
        if (handItemData)
        {
            if (slotType == EquipmentSlotType.leftHand)
            {
                onHandItemRemoved?.Invoke(EquipmentSlotType.leftHand, handItemData);
            }
            else
                onHandItemRemoved?.Invoke(EquipmentSlotType.rightHand, handItemData);
        }

        onEquipmentItemRemoved?.Invoke(slotType);
    }

    public void DisableSlot()
    {
        isSlotActive = false;
        slotImage.color = new Color(255, 255, 255, .12f);
    }

    public void EnableSlot()
    {
        isSlotActive = true;
        slotImage.color = Color.white;
    }
}