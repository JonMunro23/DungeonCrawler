using System;
using UnityEngine;

public class EquipmentSlot : InventorySlot
{
    public EquipmentSlotType slotType;
    [Tooltip("Used when initialising a single handed weapon, only used if slotType = Hands")]
    [SerializeField] Hands Hand;

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
        if(slotType == EquipmentSlotType.hands)
        {
            HandItemData handItemData = itemToInitialise.itemData as HandItemData;
            if (handItemData.isTwoHanded)
            {
                onNewHandItem?.Invoke(Hands.both, handItemData);
            }
            else
            {
                if (Hand == Hands.right)
                {
                    onNewHandItem?.Invoke(Hands.right, handItemData);
                }
                else
                    onNewHandItem?.Invoke(Hands.left, handItemData);
            }

        }

        onNewEquipmentItem?.Invoke(slotType, itemToInitialise.itemData as EquipmentItemData);
    }

    public void DeinitialiseItem(Item item)
    {
        if (slotType == EquipmentSlotType.hands)
        {
            HandItemData handItemData = item.itemData as HandItemData;
            if(handItemData.isTwoHanded)
            {
                onHandItemRemoved?.Invoke(Hands.both);
            }
            else if (Hand == Hands.right)
            {
                onHandItemRemoved?.Invoke(Hands.right);
            }
            else
                onHandItemRemoved?.Invoke(Hands.left);
        }

        onEquipmentItemRemoved?.Invoke(slotType);
    }

    public void DisableSlot()
    {
        isSlotActive = false;
        slotImage.color = Color.red;
    }

    public void EnableSlot()
    {
        isSlotActive = true;
        slotImage.color = Color.white;
    }
}