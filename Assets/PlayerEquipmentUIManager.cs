using UnityEngine;

public class PlayerEquipmentUIManager : MonoBehaviour
{
    [SerializeField] EquipmentSlot[] equipmentSlots;

    private void OnEnable()
    {
        ItemPickupManager.onNewItemAttachedToCursor += OnNewItemAttachedToCursor;
        ItemPickupManager.onCurrentItemDettachedFromCursor += OnCurrentItemDettachedFromCursor;
        InventorySlot.onNewHandItem += OnNewHandItem;
        InventorySlot.onHandItemRemoved += OnHandItemRemoved;
    }

    private void OnDisable()
    {
        ItemPickupManager.onNewItemAttachedToCursor -= OnNewItemAttachedToCursor;
        ItemPickupManager.onCurrentItemDettachedFromCursor -= OnCurrentItemDettachedFromCursor;
        InventorySlot.onNewHandItem -= OnNewHandItem;
        InventorySlot.onHandItemRemoved -= OnHandItemRemoved;
    }

    void OnNewItemAttachedToCursor(Item newItem)
    {
        EquipmentItemData yeet = newItem.itemData as EquipmentItemData;
        if (yeet != null)
            DisableSlotNotOfType(yeet.EquipmentSlotType);
    }

    void OnCurrentItemDettachedFromCursor()
    {
        RenableSlots();
    }

    void OnNewHandItem(Hands hands, HandItemData itemData)
    {
        if (hands == Hands.both)
        {
            foreach (EquipmentSlot slot in equipmentSlots)
            {
                if(slot.slotType == EquipmentSlotType.hands)
                {
                    slot.slotImage.sprite = itemData.itemSprite;
                }
            }
        }
    }

    void OnHandItemRemoved(Hands hands)
    {
        Debug.Log(hands);
        if(hands == Hands.both)
        {
            foreach (EquipmentSlot slot in equipmentSlots)
            {
                if (slot.slotType == EquipmentSlotType.hands)
                {
                    slot.slotImage.sprite = null;
                }
            }
        }
    }

    public void DisableSlotNotOfType(EquipmentSlotType slotTypeNotToDisable)
    {
        foreach (EquipmentSlot slot in equipmentSlots)
        {
            if (slot.slotType != slotTypeNotToDisable)
            {
                slot.DisableSlot();
            }
        }

    }

    public void DisableSlotOfType(EquipmentSlotType slotTypeNotToDisable)
    {
        foreach (EquipmentSlot slot in equipmentSlots)
        {
            if (slot.slotType == slotTypeNotToDisable)
            {
                slot.DisableSlot();
            }
        }

    }

    public void RenableSlots()
    {
        foreach (EquipmentSlot slot in equipmentSlots)
        {
            slot.EnableSlot();
        }
    }
}
