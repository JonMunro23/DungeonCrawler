using UnityEngine;

public class PlayerEquipmentUIManager : MonoBehaviour
{
    [SerializeField] EquipmentSlot[] equipmentSlots;

    private void OnEnable()
    {
        ItemPickupManager.onNewItemAttachedToCursor += OnNewItemAttachedToCursor;
        ItemPickupManager.onCurrentItemDettachedFromCursor += OnCurrentItemDettachedFromCursor;
    }

    private void OnDisable()
    {
        ItemPickupManager.onNewItemAttachedToCursor -= OnNewItemAttachedToCursor;
        ItemPickupManager.onCurrentItemDettachedFromCursor -= OnCurrentItemDettachedFromCursor;
    }

    void OnNewItemAttachedToCursor(ItemStack newItem)
    {
        WeaponItemData handItemData = newItem.itemData as WeaponItemData;
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

    void DisableEquipmentSlots()
    {
        foreach(EquipmentSlot slot in equipmentSlots)
        {
            slot.SetInteractable(false);
        }
    }

    public void DisableSlotsNotOfType(EquipmentSlotType slotTypeNotToDisable)
    {
        foreach (EquipmentSlot slot in equipmentSlots)
        {
            if (slot.slotType != slotTypeNotToDisable)
            {
                slot.SetInteractable(false);
            }
        }

    }

    public void DisableSlotOfType(EquipmentSlotType slotTypeToDisable)
    {
        foreach (EquipmentSlot slot in equipmentSlots)
        {
            if (slot.slotType == slotTypeToDisable)
            {
                slot.SetInteractable(false);
            }
        }
    }

    public void RenableSlots()
    {
        foreach (EquipmentSlot slot in equipmentSlots)
        {
            slot.SetInteractable(true);
        }
    }
}
