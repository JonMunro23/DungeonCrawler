using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryContextMenu : MonoBehaviour
{
    [SerializeField] UnityEngine.UI.Button DropButton;
    [SerializeField] UnityEngine.UI.Button UnloadAmmoButton;
    [SerializeField] UnityEngine.UI.Button EquipButton;
    [SerializeField] UnityEngine.UI.Button UseButton;

    ISlot slot;

    public static event Action<ISlot> onInventorySlotWeaponUnloaded;
    public static event Action<ISlot> onInventorySlotItemDropped;

    public static event Action<ISlot> onInventorySlotEquipmentItemEquipped;
    public static event Action<ISlot> onInventorySlotWeaponItemEquipped;

    public static event Action<ISlot> onInventorySlotEquipmentItemUnequipped;
    public static event Action<ISlot> onInventorySlotWeaponItemUnequipped;

    public static event Action<ISlot> onBoosterUsed;
    public static event Action<ISlot> onHealSyringeUsedFromContextMenu;

    public void Init(ISlot slot)
    {
        this.slot = slot;
        ToggleButtons();
    }

    private void ToggleButtons()
    {
        UseButton.gameObject.SetActive(false);
        UseButton.onClick.RemoveAllListeners();

        UnloadAmmoButton.gameObject.SetActive(false);
        UnloadAmmoButton.onClick.RemoveAllListeners();

        EquipButton.gameObject.SetActive(false);
        EquipButton.onClick.RemoveAllListeners();

        WeaponItemData weaponItemData = slot.GetItemStack().itemData as WeaponItemData;
        if (weaponItemData)
        {
            if (slot.GetItemStack().loadedAmmo > 0)
            {
                UnloadAmmoButton.gameObject.SetActive(true);
                UnloadAmmoButton.GetComponentInChildren<TMP_Text>().text = "UNLOAD";
                UnloadAmmoButton.onClick.AddListener(() =>
                {
                    UnloadWeapon();
                });
            }
            //else
            //{
            //    LoadUnloadButton.GetComponentInChildren<TMP_Text>().text = "LOAD";
            //    LoadUnloadButton.onClick.AddListener(() =>
            //    {
            //        OpenLoadAmmoMenu();
            //    });
            //}

            WeaponSlot weaponSlot = slot.GetSlot() as WeaponSlot;
            if (weaponSlot)
            {
                if(!weaponSlot.IsSlotEmpty())
                {
                    EquipButton.GetComponentInChildren<TMP_Text>().text = "UNEQUIP";
                    EquipButton.onClick.AddListener(() =>
                    {
                        Unequip();
                    });
                }
            }
            else
            {
                EquipButton.GetComponentInChildren<TMP_Text>().text = "EQUIP";
                EquipButton.onClick.AddListener(() =>
                {
                    Equip();
                });
            }
            EquipButton.gameObject.SetActive(true);
            return;
        }


        EquipmentItemData equipmentItemData = slot.GetItemStack().itemData as EquipmentItemData;
        if (equipmentItemData)
        {
            EquipmentSlot equipmentSlot = slot.GetSlot() as EquipmentSlot;
            if (equipmentSlot)
            {
                if(!equipmentSlot.IsSlotEmpty())
                {
                    EquipButton.GetComponentInChildren<TMP_Text>().text = "UNEQUIP";
                    EquipButton.onClick.AddListener(() =>
                    {
                        Unequip();
                    });
                }
            }
            else
            {
                EquipButton.GetComponentInChildren<TMP_Text>().text = "EQUIP";
                EquipButton.onClick.AddListener(() =>
                {
                    Equip();
                });
            }
            EquipButton.gameObject.SetActive(true);
            return;
        }

        ConsumableItemData consumableItemData = slot.GetItemStack().itemData as ConsumableItemData;
        if (consumableItemData)
        {
            UseButton.gameObject.SetActive(true);
            switch (consumableItemData.consumableType)
            {
                case ConsumableType.Booster:
                    UseButton.onClick.AddListener(() =>
                    {
                        UseBooster();
                    });
                    break;
                case ConsumableType.HealSyringe:
                    UseButton.onClick.AddListener(() =>
                    {
                        UseHealSyringe();
                    });
                    break;
            }
        }
    }
    public void UseBooster()
    {
        onBoosterUsed?.Invoke(slot);
        HideContextMenu();
    }
    public void UseHealSyringe()
    {
        onHealSyringeUsedFromContextMenu?.Invoke(slot);
        HideContextMenu();
    }

    public void Equip()
    {
        WeaponItemData weaponItemData = slot.GetItemStack().itemData as WeaponItemData;
        if(weaponItemData)
        {
            //add to weapon slot
            onInventorySlotWeaponItemEquipped?.Invoke(slot);
            HideContextMenu();
            return;
        }

        EquipmentItemData equipmentItemData = slot.GetItemStack().itemData as EquipmentItemData;
        if(equipmentItemData)
        {
            //add to appropraite equipment slot
            onInventorySlotEquipmentItemEquipped?.Invoke(slot);
            HideContextMenu();
            return;
        }
    }

    public void Unequip()
    {
        WeaponItemData weaponItemData = slot.GetItemStack().itemData as WeaponItemData;
        if (weaponItemData)
        {
            //remove from weapon slot
            onInventorySlotWeaponItemUnequipped?.Invoke(slot);
            HideContextMenu();
            return;
        }

        EquipmentItemData equipmentItemData = slot.GetItemStack().itemData as EquipmentItemData;
        if (equipmentItemData)
        {
            //remove from appropraite equipment slot
            onInventorySlotEquipmentItemUnequipped?.Invoke(slot);
            HideContextMenu();
            return;
        }
    }

    public void UnloadWeapon()
    {
        onInventorySlotWeaponUnloaded?.Invoke(slot);
        HideContextMenu();
    }

    public void Drop()
    {
        onInventorySlotItemDropped?.Invoke(slot);
        HideContextMenu();
    }

    public void OpenLoadAmmoMenu()
    {
        //show load ammo context menu
        //set available ammo buttons interactable
    }

    //public void LoadAmmo()
    //{
    //    //set loaded ammo in itemstack
    //}

    void HideContextMenu()
    {
        if (slot != null)
            if(!slot.IsSlotEmpty())
                slot.ShowTooltip();

        gameObject.SetActive(false);
    }
}
