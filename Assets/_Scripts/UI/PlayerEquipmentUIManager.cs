using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEquipmentUIManager : MonoBehaviour
{
    [SerializeField] List<EquipmentSlot> equipmentSlots;
    [SerializeField] List<Transform> equipmentSlotParentTransforms;
    [SerializeField] Sprite[] equipmentSlotDefaultIcons;

    private void OnEnable()
    {
        PlayerEquipmentManager.onEquipmentSlotsSpawned += OnEquipmentSlotsSpawned;
    }

    private void OnDisable()
    {
        PlayerEquipmentManager.onEquipmentSlotsSpawned -= OnEquipmentSlotsSpawned;
    }

    void OnEquipmentSlotsSpawned(List<EquipmentSlot> slots)
    {
        equipmentSlots = slots;

        for (int i = 0; i < equipmentSlots.Count; i++)
        {
            equipmentSlots[i].slotType = (EquipmentSlotType)i+1;
            equipmentSlots[i].transform.SetParent(equipmentSlotParentTransforms[i], false);
            equipmentSlots[i].InitEquipmentSlot(equipmentSlotDefaultIcons[i]);
        }
    }
    
   

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
