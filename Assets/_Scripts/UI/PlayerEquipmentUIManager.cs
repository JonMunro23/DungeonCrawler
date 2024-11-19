using UnityEngine;

public class PlayerEquipmentUIManager : MonoBehaviour
{
    [SerializeField] EquipmentSlot[] equipmentSlots;

    public void DisableAllSlots()
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
