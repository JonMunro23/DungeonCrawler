using UnityEngine;

public class EquipmentSlot : InventorySlot
{
    public SlotType slotType;
    [Tooltip("Used when initialising a single handed weapon, only used if slotType = Hands")]
    [SerializeField] Hands Hand;

    public override void AddItem(Item itemToAdd)
    {
        base.AddItem(itemToAdd);
        InitialiseItem(currentSlotItem);
    }

    public override Item SwapItem(Item itemToSwap)
    {
        Item itemToReturn = base.SwapItem(itemToSwap);
        DeinitialiseCurrentItem();
        InitialiseItem(itemToSwap);
        return itemToReturn;
    }

    public override Item TakeItem()
    {
        Item itemToTake = base.TakeItem();
        DeinitialiseCurrentItem();
        return itemToTake;

    }

    public void InitialiseItem(Item itemToInitialise)
    {
        if(slotType == SlotType.hands)
        {
            if (Hand == Hands.right)
            {
                onNewHandItem?.Invoke(Hands.right, itemToInitialise.itemData);
            }
            else
                onNewHandItem?.Invoke(Hands.left, itemToInitialise.itemData);

            return;
        }
    }

    public void DeinitialiseCurrentItem()
    {
        if (slotType == SlotType.hands)
        {
            if (Hand == Hands.right)
            {
                onHandItemRemoved?.Invoke(Hands.right);
            }
            else
                onHandItemRemoved?.Invoke(Hands.left);

            return;
        }
    }

    public void DisableSlot()
    {
        isSlotActive = false;
        slotImage.color = Color.red;
        Debug.Log("Disabled slot");
    }

    public void EnableSlot()
    {
        isSlotActive = true;
        slotImage.color = Color.white;
        Debug.Log("Renabled slot");

    }
}