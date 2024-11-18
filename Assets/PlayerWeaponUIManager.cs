using UnityEngine;

public class PlayerWeaponUIManager : MonoBehaviour
{
    [SerializeField] Transform weaponSlotSpawnParent;

    [SerializeField] WeaponSlot[] weaponSlots;

    private void OnEnable()
    {
        PlayerWeaponManager.onWeaponSlotsSpawned += OnWeaponSlotsSpawned;

        WeaponSlot.onWeaponAddedToSlot += OnWeaponAddedToSlot;
        WeaponSlot.onWeaponRemovedFromSlot += OnWeaponRemovedFromSlot;
    }

    private void OnDisable()
    {
        PlayerWeaponManager.onWeaponSlotsSpawned -= OnWeaponSlotsSpawned;

        WeaponSlot.onWeaponAddedToSlot -= OnWeaponAddedToSlot;
        WeaponSlot.onWeaponRemovedFromSlot -= OnWeaponRemovedFromSlot;
    }

    void OnWeaponSlotsSpawned(WeaponSlot[] slots)
    {
        weaponSlots = slots;

        foreach (WeaponSlot slot in weaponSlots)
        {
            slot.transform.SetParent(weaponSlotSpawnParent, false);
        }
    }

    void OnWeaponAddedToSlot(int slotIndex, WeaponItemData newItemData)
    {
        //SetSpriteInSlotOfType(slotIndex, newItemData.itemSprite);
    }

    void OnWeaponRemovedFromSlot(int slotIndex)
    {
        //SetSpriteInSlotOfType(slotIndex, null);
    }

    public void SetSpriteInSlotOfType(int slotIndex, Sprite spriteToSet)
    {
        foreach (WeaponSlot slot in weaponSlots)
        {
            if (slot.slotIndex == slotIndex)
            {
                slot.slotImage.sprite = spriteToSet;
            }
        }
    }

    public void DisableSlots()
    {
        foreach(WeaponSlot slot in weaponSlots)
        {
            slot.SetInteractable(false);
        }
    }

    public void RenableSlots()
    {
        foreach (WeaponSlot slot in weaponSlots)
        {
            slot.SetInteractable(true);
        }
    }
}
