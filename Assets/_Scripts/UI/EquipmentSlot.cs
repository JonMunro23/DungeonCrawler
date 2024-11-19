using System;
using UnityEngine;

public class EquipmentSlot : InventorySlot
{
    public EquipmentSlotType slotType;

    public static Action<EquipmentSlotType, EquipmentItemData> onNewEquipmentItem;
    public static Action<EquipmentSlotType> onEquipmentItemRemoved;

    [SerializeField] Sprite placeholderIcon;
    [SerializeField] float placeholderIconAlpha;

    private void Start()
    {
        slotImage.sprite = placeholderIcon;
        slotImage.color = slotImage.color = new Color(255, 255, 255, placeholderIconAlpha);
    }

    public override void AddItem(ItemStack itemToAdd)
    {
        base.AddItem(itemToAdd);
        InitialiseEquipmentItem(currentSlotItemStack);
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