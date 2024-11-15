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

    void OnNewItemAttachedToCursor(ItemStack newItem)
    {
        HandItemData handItemData = newItem.itemData as HandItemData;
        if(handItemData != null)
        {
            DisableEquipmentSlots();
            return;
        }

        EquipmentItemData equipItemData = newItem.itemData as EquipmentItemData;
        if (equipItemData != null)
            DisableSlotsNotOfType(equipItemData.EquipmentSlotType);
    }

    void OnCurrentItemDettachedFromCursor()
    {
        RenableSlots();
    }

    void OnNewHandItem(EquipmentSlotType slotType, HandItemData newItemData)
    {
        if (slotType == EquipmentSlotType.leftHand || slotType == EquipmentSlotType.rightHand)
        {
            if(newItemData.isTwoHanded)
            {
                SetSpriteInSlotOfType(newItemData.itemSprite, EquipmentSlotType.rightHand);
                SetSpriteInSlotOfType(newItemData.itemSprite, EquipmentSlotType.leftHand);
            }
            else
            {
                SetSpriteInSlotOfType(newItemData.itemSprite, slotType);
            }
        }
    }

    void OnHandItemRemoved(EquipmentSlotType slotTypeRemovedFrom, HandItemData removedItemData)
    {
        if(removedItemData.isTwoHanded)
        {
            SetSpriteInSlotOfType(null, EquipmentSlotType.rightHand);
            SetSpriteInSlotOfType(null, EquipmentSlotType.leftHand);
        }
        else
            SetSpriteInSlotOfType(null, slotTypeRemovedFrom);
    }

    public void SetSpriteInSlotOfType(Sprite spriteToSet, EquipmentSlotType slotType)
    {
        foreach (EquipmentSlot slot in equipmentSlots)
        {
            if (slot.slotType == slotType)
            {
                slot.slotImage.sprite = spriteToSet;
            }
        }
    }

    void DisableEquipmentSlots()
    {
        foreach(EquipmentSlot slot in equipmentSlots)
        {
            if (slot.slotType != EquipmentSlotType.rightHand && slot.slotType != EquipmentSlotType.leftHand)
                slot.DisableSlot();
        }
    }

    public void DisableSlotsNotOfType(EquipmentSlotType slotTypeNotToDisable)
    {
        foreach (EquipmentSlot slot in equipmentSlots)
        {
            if (slot.slotType != slotTypeNotToDisable)
            {
                slot.DisableSlot();
            }
        }

    }

    public void DisableSlotOfType(EquipmentSlotType slotTypeToDisable)
    {
        foreach (EquipmentSlot slot in equipmentSlots)
        {
            if (slot.slotType == slotTypeToDisable)
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
