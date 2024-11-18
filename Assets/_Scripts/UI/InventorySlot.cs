using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System;

public class InventorySlot : MonoBehaviour, ISlot ,IPointerClickHandler
{
    PlayerInventoryManager playerInventoryManager;

    int slotIndex;

    public bool isInteractable;

    public ItemStack currentSlotItemStack = null;
    public TMP_Text SlotAmountText;
    public Image slotImage;

    public bool isSlotOccupied;
    public bool isSlotActive = true;

    public static Action<ISlot> onInventorySlotClicked;

    private void Start()
    {
        SetInteractable(true);
    }

    public void InitSlot(PlayerInventoryManager newPlayerInventoryManager, int _slotIndex)
    {
        playerInventoryManager = newPlayerInventoryManager;
        slotIndex = _slotIndex;
    }

    public virtual void AddItem(ItemStack itemToAdd)
    {
        currentSlotItemStack = new ItemStack(itemToAdd.itemData, itemToAdd.itemAmount);
        isSlotOccupied = true;

        ConsumableItemData consumableData = GetDataAsConsumable(itemToAdd.itemData);
        if (consumableData)
        {
            if (consumableData.consumableType == ConsumableType.HealSyringe)
            {
                playerInventoryManager.AddHealthSyringe(itemToAdd.itemAmount);
            }
        }

        UpdateSlotUI();
    }

    public void AddToCurrentItemStack(int amountToAdd)
    {
        currentSlotItemStack.itemAmount += amountToAdd;

        ConsumableItemData consumableData = GetDataAsConsumable(currentSlotItemStack.itemData);
        if (consumableData)
        {
            if (consumableData.consumableType == ConsumableType.HealSyringe)
            {
                playerInventoryManager.AddHealthSyringe(amountToAdd);
            }
        }

        UpdateSlotUI();
    }

    ConsumableItemData GetDataAsConsumable(ItemData data)
    {
        return data as ConsumableItemData;
    }

    public virtual ItemStack TakeItem()
    {
        ItemStack itemToTake = new ItemStack(currentSlotItemStack.itemData, currentSlotItemStack.itemAmount);

        ConsumableItemData consumableData = GetDataAsConsumable(itemToTake.itemData);
        if(consumableData)
        {
            if (consumableData.consumableType == ConsumableType.HealSyringe)
            {
                playerInventoryManager.RemoveHealthSyringe(itemToTake.itemAmount);
            }
        }

        RemoveItem();
        return itemToTake;
    }

    public virtual ItemStack SwapItem(ItemStack itemToSwap)
    {
        ItemStack oldItem = new ItemStack(currentSlotItemStack.itemData, currentSlotItemStack.itemAmount);

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
        if (currentSlotItemStack.itemData == null)
        {
            slotImage.sprite = null;
            slotImage.enabled = false;

            SlotAmountText.text = "";
            return;
        }
            

        slotImage.sprite = currentSlotItemStack.itemData.itemSprite;
        slotImage.enabled = true;
        if (currentSlotItemStack.itemAmount > 1)
            SlotAmountText.text = currentSlotItemStack.itemAmount.ToString();
        else
            SlotAmountText.text = "";
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
        UpdateSlotUI();
    }

    public void AddToExistingStack(int amountToAdd)
    {
        currentSlotItemStack.itemAmount += amountToAdd;
        UpdateSlotUI();
    }

    public ItemStack GetItemStack()
    {
        return currentSlotItemStack;
    }

    public void SetInteractable(bool _isInteractable)
    {
        isInteractable = _isInteractable;
        if (isInteractable == false)
            slotImage.color = new Color(255, 255, 255, .12f);
        else
            slotImage.color = Color.white;
    }

    public bool IsInteractable()
    {
        return isInteractable;
    }

    public bool IsSlotEmpty()
    {
        return currentSlotItemStack.itemData ? false : true;
    }
}
