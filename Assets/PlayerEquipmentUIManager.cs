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

    void OnNewItemAttachedToCursor(Item newItem)
    {
        Debug.Log("new item attached: " + newItem.itemData.itemName);
        DisableSlotNotOfType(newItem.itemData.slotType);
    }

    void OnCurrentItemDettachedFromCursor()
    {
        RenableSlots();
    }

    public void DisableSlotNotOfType(SlotType slotTypeNotToDisable)
    {
        foreach (EquipmentSlot slot in equipmentSlots)
        {
            if (slot.slotType != slotTypeNotToDisable)
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
