using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System;

public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    public Item currentSlotItem = null;
    public TMP_Text SlotAmountText;
    public Image slotImage;

    //[SerializeField]
    //GameObject tooltipDisplay;
    //GameObject tooltipDisplayClone;
    //bool isTooltipDisplayOpen;

    public bool isSlotOccupied;
    public bool isSlotActive = true;

    public static Action<EquipmentSlotType ,HandItemData> onNewHandItem;
    public static Action<EquipmentSlotType, HandItemData> onHandItemRemoved;
    public static Action<InventorySlot> onInventorySlotClicked;

    private void Update()
    {
        //if(isTooltipDisplayOpen == true)
        //{
        //    mousePos = Input.mousePosition;
        //    tooltipDisplayClone.transform.position = mousePos;
        //}
    }

    public virtual void AddItem(Item itemToAdd)
    {
        currentSlotItem = new Item(itemToAdd.itemData, itemToAdd.itemAmount);
        isSlotOccupied = true;

        UpdateSlotUI();
    }

    public void AddToCurrentItemStack(int amountToAdd)
    {
        currentSlotItem.itemAmount += amountToAdd;
        UpdateSlotUI();
    }

    public virtual Item TakeItem()
    {
        Item itemToTake = new Item(currentSlotItem.itemData, currentSlotItem.itemAmount);

        currentSlotItem.itemData = null;
        currentSlotItem.itemAmount = 0;
        isSlotOccupied = false;

        UpdateSlotUI();

        return itemToTake;
    }

    public virtual Item SwapItem(Item itemToSwap)
    {
        Item oldItem = new Item(currentSlotItem.itemData, currentSlotItem.itemAmount);

        currentSlotItem = itemToSwap;
        UpdateSlotUI();

        return oldItem;
    }

    void UpdateSlotUI()
    {
        if (currentSlotItem.itemData == null)
        {
            slotImage.sprite = null;
            slotImage.enabled = false;

            SlotAmountText.text = "";
            return;
        }
            

        slotImage.sprite = currentSlotItem.itemData.itemSprite;
        slotImage.enabled = true;
        if(currentSlotItem.itemAmount > 1)
            SlotAmountText.text = currentSlotItem.itemAmount.ToString();
    }

    //public void ShowTooltipDisplay(ItemData itemToDisplay)
    //{

    //        //tooltipDisplayClone = Instantiate(tooltipDisplay, mousePos, Quaternion.identity, itemPickup.canvasTransform);
    //        tooltipDisplayClone.transform.localScale = new Vector3(2, 2, 2);
    //        isTooltipDisplayOpen = true;
    //        TooltipDisplay display = tooltipDisplayClone.GetComponent<TooltipDisplay>();
    //        display.itemNameText.text = itemToDisplay.itemName;
    //        display.itemTypeText.text = itemToDisplay.itemType.ToString();
    //        display.itemWeightText.text = itemToDisplay.itemWeight.ToString() + "kg";
    //        display.itemValueText.text = itemToDisplay.itemValue.ToString() + "g";
    //        display.itemDescriptionText.text = itemToDisplay.itemDescription;

    //        display.itemPortrait.sprite = itemToDisplay.itemSprite;

    //    if (itemToDisplay.itemType == ItemData.ItemType.meleeWeapon || itemToDisplay.itemType == ItemData.ItemType.rangedWeapon)
    //    {
    //        display.itemDamageText.text = itemToDisplay.itemDamageMin + " - " + itemToDisplay.itemDamageMax + " " + itemToDisplay.itemDamageType;
    //        display.itemCooldownText.text = itemToDisplay.itemCooldown.ToString() + " seconds";

    //    }
    //    else if (itemToDisplay.itemType == ItemData.ItemType.consumable)
    //    {
    //        //display consumable information on tooltipDisplay
    //    }
    //}

    //public void HideTooltipDisplay()
    //{
    //    isTooltipDisplayOpen = false;
    //    Destroy(tooltipDisplayClone);
    //}

    public void OnPointerClick(PointerEventData eventData)
    {
        onInventorySlotClicked?.Invoke(this);
    }
}
