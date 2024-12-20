using System;
using UnityEngine;

public class EquipmentSlot : InventorySlot
{

    public EquipmentSlotType slotType;

    public static Action<EquipmentSlotType, EquipmentItemData> onNewEquipmentItem;
    public static Action<EquipmentSlotType> onEquipmentItemRemoved;

    [SerializeField] Sprite defaultSlotIcon;
    [SerializeField] float placeholderIconAlpha;

    public void InitEquipmentSlot(Sprite defaultSlotIcon)
    {
        this.defaultSlotIcon = defaultSlotIcon;
        slotImage.sprite = defaultSlotIcon;
        slotImage.color = slotImage.color = new Color(255, 255, 255, placeholderIconAlpha);
        SetInteractable(true);
    }

    public override void AddItem(ItemStack itemToAdd)
    {
        base.AddItem(itemToAdd);
        InitialiseEquipmentItem(GetItemStack());
    }

    public override ItemStack SwapItem(ItemStack itemToSwap)
    {
        ItemStack itemToReturn = base.SwapItem(itemToSwap);
        DeinitialiseEquipmentItem(itemToReturn);
        InitialiseEquipmentItem(itemToSwap);
        return itemToReturn;
    }

    public override ItemStack TakeItem()
    {
        ItemStack itemToTake = base.TakeItem();
        DeinitialiseEquipmentItem(itemToTake);
        slotImage.sprite = defaultSlotIcon;
        slotImage.color = new Color(.75f, .75f, .75f, .33f);
        return itemToTake;

    }

    public void InitialiseEquipmentItem(ItemStack itemToInitialise)
    {
        onNewEquipmentItem?.Invoke(slotType, itemToInitialise.itemData as EquipmentItemData);
    }

    public void DeinitialiseEquipmentItem(ItemStack item)
    {
        onEquipmentItemRemoved?.Invoke(slotType);
    }
}