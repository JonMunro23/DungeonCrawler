using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System;
using ModelShark;
using System.Text;

public class InventorySlot : MonoBehaviour, ISlot, IPointerClickHandler
{
    PlayerInventoryManager playerInventoryManager;

    public int slotIndex;

    public bool isInteractable { get; private set; }

    public ItemStack currentSlotItemStack = null;
    public TMP_Text SlotAmountText;
    public Image slotImage;

    public TooltipTrigger tooltipTrigger;

    public bool isSlotOccupied { get; private set; }
    public bool isSlotActive = true;

    public static Action<ISlot> onInventorySlotClicked;

    private void Awake()
    {
        tooltipTrigger = GetComponent<TooltipTrigger>();
    }

    public void InitSlot(PlayerInventoryManager newPlayerInventoryManager, int _slotIndex)
    {
        playerInventoryManager = newPlayerInventoryManager;
        slotIndex = _slotIndex;
        SetInteractable(true);
    }

    public virtual void AddItem(ItemStack itemToAdd)
    {
        currentSlotItemStack = new ItemStack(itemToAdd.itemData, itemToAdd.itemAmount, itemToAdd.loadedAmmo);
        isSlotOccupied = true;

        ConsumableItemData consumableData = GetDataAsConsumable(itemToAdd.itemData);
        if (consumableData)
        {
            if (consumableData.consumableType == ConsumableType.HealSyringe)
            {
                playerInventoryManager.AddHealthSyringe(itemToAdd.itemAmount);
            }
            else if (consumableData.consumableType == ConsumableType.Ammo)
            {
                playerInventoryManager.AddAmmo(consumableData.ammoType, itemToAdd.itemAmount);
            }
        }

        SetTooltipTriggerActive(true);
        UpdateSlotUI();
    }

    public int AddToCurrentItemStack(int amountToAdd)
    {
        int remainder = 0;
        int availableSpace = currentSlotItemStack.itemData.maxItemStackSize - currentSlotItemStack.itemAmount;
        if (availableSpace < amountToAdd)
        {
            currentSlotItemStack.itemAmount += availableSpace;
            remainder = amountToAdd - availableSpace;
        }
        else if(availableSpace >= amountToAdd)
        {
            currentSlotItemStack.itemAmount += amountToAdd;
        }

        ConsumableItemData consumableData = GetDataAsConsumable(currentSlotItemStack.itemData);
        if (consumableData)
        {
            if (consumableData.consumableType == ConsumableType.HealSyringe)
            {
                playerInventoryManager.AddHealthSyringe(amountToAdd);
            }
            else if (consumableData.consumableType == ConsumableType.Ammo)
            {
                playerInventoryManager.AddAmmo(consumableData.ammoType, amountToAdd);
            }
        }

        UpdateSlotUI();

        return remainder;
    }

    public int RemoveFromExistingStack(int amountToRemove)
    {
        int remainder = 0;
        if (currentSlotItemStack.itemAmount < amountToRemove)
        {
            amountToRemove -= currentSlotItemStack.itemAmount;
            currentSlotItemStack.itemAmount -= amountToRemove;
            remainder = amountToRemove;
        }
        else if (currentSlotItemStack.itemAmount >= amountToRemove)
        {
            currentSlotItemStack.itemAmount -= amountToRemove;
        }


        if (currentSlotItemStack.itemAmount <= 0)
        {
            RemoveItem();
        }

        UpdateSlotUI();

        return remainder;
    }

    ConsumableItemData GetDataAsConsumable(ItemData data)
    {
        return data as ConsumableItemData;
    }

    public virtual ItemStack TakeItem()
    {
        ItemStack itemToTake = new ItemStack(currentSlotItemStack.itemData, currentSlotItemStack.itemAmount, currentSlotItemStack.loadedAmmo);

        ConsumableItemData consumableData = GetDataAsConsumable(itemToTake.itemData);
        if (consumableData)
        {
            if (consumableData.consumableType == ConsumableType.HealSyringe)
            {
                playerInventoryManager.RemoveHealthSyringe(itemToTake.itemAmount);
            }
            else if (consumableData.consumableType == ConsumableType.Ammo)
            {
                playerInventoryManager.RemoveAmmo(consumableData.ammoType, itemToTake.itemAmount);
            }
        }
        RemoveItem();
        return itemToTake;
    }

    private void SetTooltipTriggerActive(bool isActive)
    {
        tooltipTrigger.enabled = isActive;
    }

    public virtual ItemStack SwapItem(ItemStack itemToSwap)
    {
        ItemStack oldItem = new ItemStack(currentSlotItemStack.itemData, currentSlotItemStack.itemAmount, currentSlotItemStack.loadedAmmo);

        currentSlotItemStack = itemToSwap;
        UpdateSlotUI();

        return oldItem;
    }

    public void UseItem()
    {
        if (currentSlotItemStack != null)
        {
            currentSlotItemStack.itemAmount--;
            UpdateSlotUI();
            if (currentSlotItemStack.itemAmount == 0)
                RemoveItem();
        }
    }

     void UpdateSlotUI()
    {
        UpdateTooltipData();

        if (currentSlotItemStack.itemData == null)
        {
            slotImage.sprite = null;
            slotImage.enabled = false;

            SlotAmountText.text = "";
            return;
        }
        slotImage.color = Color.white;
        slotImage.sprite = currentSlotItemStack.itemData.itemSprite;
        slotImage.enabled = true;
        if (currentSlotItemStack.itemAmount > 1)
            SlotAmountText.text = currentSlotItemStack.itemAmount.ToString();
        else
            SlotAmountText.text = "";
    }

    void UpdateTooltipData()
    {
        if (!tooltipTrigger)
            return;

        if (currentSlotItemStack.itemData == null)
            return;

        tooltipTrigger.SetImage("ItemImage", currentSlotItemStack.itemData.itemSprite);
        tooltipTrigger.SetText("TitleText", currentSlotItemStack.itemData.itemName);
        tooltipTrigger.SetText("Description", currentSlotItemStack.itemData.itemDescription);
        tooltipTrigger.SetText("Stats", string.Empty);

        EquipmentItemData equipmentItem = currentSlotItemStack.itemData as EquipmentItemData;
        if(equipmentItem)
        {
            StringBuilder statsText = new StringBuilder();
            if(equipmentItem.statModifiers.Count > 0)
            {
                tooltipTrigger.TurnSectionOn("Stats");
                foreach (var item in equipmentItem.statModifiers)
                {
                    string modifyOperator = string.Empty;
                    bool isPercentage = false;
                    switch (item.modifyOperation)
                    {
                        case ModifyOperation.Increase:
                            modifyOperator = "+";
                            break;
                        case ModifyOperation.IncreaseByPercentage:
                            modifyOperator = "+";
                            isPercentage = true;
                            break;
                        case ModifyOperation.Decrease:
                            modifyOperator = "-";
                            break;
                        case ModifyOperation.DecreaseByPercentage:
                            modifyOperator = "-";
                            isPercentage = true;
                            break;
                    }

                    statsText.AppendLine($"{modifyOperator}{item.modifyAmount}{(isPercentage ? "%" : string.Empty)} {item.statToModify}");
                }

                tooltipTrigger.SetText("Stats", statsText.ToString());
            }

        }

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!IsInteractable())
            return;

        onInventorySlotClicked?.Invoke(this);
    }

    public void RemoveItem()
    {
        currentSlotItemStack.itemData = null;
        currentSlotItemStack.itemAmount = 0;
        isSlotOccupied = false;
        SetTooltipTriggerActive(false);
        UpdateSlotUI();
    }

    

    public ItemStack GetItemStack()
    {
        return currentSlotItemStack;
    }

    public void SetInteractable(bool _isInteractable)
    {
        isInteractable = _isInteractable;
        
    }

    public bool IsInteractable()
    {
        return isInteractable;
    }

    public bool IsSlotEmpty()
    {
        return currentSlotItemStack.itemData ? false : true;
    }

    public int GetSlotIndex()
    {
        return slotIndex;
    }
}
