using UnityEngine;

public class PlayerWeaponUIManager : MonoBehaviour
{
    [SerializeField] Transform weaponSlotSpawnParent;

    [SerializeField] WeaponSlot[] weaponSlots;

    [SerializeField] HUDWeaponDisplay[] weaponDisplays = new HUDWeaponDisplay[2];

    [SerializeField] int activeSlotIndex;

    private void OnEnable()
    {
        PlayerWeaponManager.onWeaponSlotsSpawned += OnWeaponSlotsSpawned;
    }

    private void OnDisable()
    {
        PlayerWeaponManager.onWeaponSlotsSpawned -= OnWeaponSlotsSpawned;
    }
    void OnWeaponSlotsSpawned(WeaponSlot[] slots)
    {
        weaponSlots = slots;

        foreach (WeaponSlot slot in weaponSlots)
        {
            slot.transform.SetParent(weaponSlotSpawnParent, false);
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
    public void SetSlotActive(int slotIndex)
    {
        activeSlotIndex = slotIndex;
        if(activeSlotIndex == 0)
        {
            weaponDisplays[0].SetDisplayAsPrimary(true);
            weaponDisplays[1].SetDisplayAsPrimary(false);
        }
        else if (activeSlotIndex == 1)
        {
            weaponDisplays[1].SetDisplayAsPrimary(true);
            weaponDisplays[0].SetDisplayAsPrimary(false);
        }
    }
    public void UpdateWeaponDisplayImages(int slotIndex, WeaponItemData weaponData)
    {
        if(weaponData)
            weaponDisplays[slotIndex].UpdateWeaponData(weaponData);
    }
    public void UpdateWeaponDisplayAmmoCount(int slotIndex, int loadedAmmo, int reserveAmmo)
    {
        weaponDisplays[slotIndex].UpdateAmmoText(loadedAmmo, reserveAmmo);
    }
}
