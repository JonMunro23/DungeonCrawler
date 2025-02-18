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
    bool isInteractable;

    [Header("References")]
    [SerializeField] ItemStack currentSlotItemStack = null;
    [SerializeField] TMP_Text SlotAmountText;
    [SerializeField] TooltipTrigger contextMenu;
    public Image slotImage;

    TooltipTrigger tooltipTrigger;


    public static Action<ISlot> onInventorySlotLeftClicked;
    public static Action<ISlot> onInventorySlotRightClicked;

    private void Awake()
    {
        tooltipTrigger = GetComponent<TooltipTrigger>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!IsInteractable())
            return;

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (IsSlotEmpty())
                return;

            onInventorySlotRightClicked?.Invoke(this);           
            return;
        }

        onInventorySlotLeftClicked?.Invoke(this);
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

        ConsumableItemData consumableData = GetDataAsConsumable(itemToAdd.itemData);
        if (consumableData)
        {
            if (consumableData.consumableType == ConsumableType.HealSyringe)
            {
                playerInventoryManager.AddHealthSyringe(itemToAdd.itemAmount);
            }
            
        }

        AmmoItemData ammoData = GetDataAsAmmo(itemToAdd.itemData);
        if (ammoData)
        {
            PlayerInventoryManager.onAmmoAddedToInventory?.Invoke(ammoData);
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
            AddToCurrentSlot(availableSpace);
            remainder = amountToAdd - availableSpace;
        }
        else if(availableSpace >= amountToAdd)
        {
            AddToCurrentSlot(amountToAdd);
        }

        UpdateSlotUI();

        return remainder;
    }

    void AddToCurrentSlot(int amountToAdd)
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

        //AmmoItemData ammoData = GetDataAsAmmo(currentSlotItemStack.itemData);
        //if (ammoData)
        //{
        //    playerInventoryManager.AddAmmo(ammoData.ammoWeaponType, amountToAdd);
        //}
    }

    public int RemoveFromExistingStack(int amountToRemove)
    {
        int remainder = 0;
        if (currentSlotItemStack.itemAmount < amountToRemove)
        {
            amountToRemove -= currentSlotItemStack.itemAmount;
            RemoveItem();
            remainder = amountToRemove;
        }
        else if (currentSlotItemStack.itemAmount >= amountToRemove)
        {
            currentSlotItemStack.itemAmount -= amountToRemove;
        }

        UpdateSlotUI();

        return remainder;
    }

    ConsumableItemData GetDataAsConsumable(ItemData data)
    {
        return data as ConsumableItemData;
    }

    AmmoItemData GetDataAsAmmo(ItemData data)
    {
        return data as AmmoItemData;
    }

    public virtual ItemStack TakeItem()
    {
        ItemStack itemToTake = new ItemStack(currentSlotItemStack.itemData, currentSlotItemStack.itemAmount, currentSlotItemStack.loadedAmmo);
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

     public void UpdateSlotUI()
    {
        UpdateTooltipData();

        EquipmentSlot equipmentSlot = this as EquipmentSlot;
        WeaponSlot weaponSlot = this as WeaponSlot;


        if (currentSlotItemStack.itemData == null)
        {
            if (!equipmentSlot && !weaponSlot)
            {
                slotImage.enabled = false;
            }
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

    

    public void RemoveItem()
    {
        ConsumableItemData consumableItemData = GetDataAsConsumable(currentSlotItemStack.itemData);
        if (consumableItemData)
        {
            switch (consumableItemData.consumableType)
            {
                case ConsumableType.HealSyringe:
                    playerInventoryManager.RemoveHealthSyringe(currentSlotItemStack.itemAmount);
                    break;
            }
        }

        //AmmoItemData ammoData = GetDataAsAmmo(currentSlotItemStack.itemData);
        //if (ammoData)
        //{
        //    playerInventoryManager.RemoveAmmo(ammoData.ammoWeaponType, currentSlotItemStack.itemAmount);
        //}

        currentSlotItemStack.itemData = null;
        currentSlotItemStack.itemAmount = 0;
        SetTooltipTriggerActive(false);
        UpdateSlotUI();
    }



    public ItemStack GetItemStack() => currentSlotItemStack;

    public void SetInteractable(bool _isInteractable)
    {
        isInteractable = _isInteractable;
        
    }

    public bool IsInteractable() => isInteractable;

    public bool IsSlotEmpty()
    {
        return currentSlotItemStack.itemData ? false : true;
    }

    public int GetSlotIndex() => slotIndex;

    public void HideTooltip()
    {
        tooltipTrigger.enabled = false;
    }

    public void ShowTooltip()
    {
        tooltipTrigger.enabled = true;
    }

    public InventorySlot GetSlot()
    {
        return this;
    }

    public int UnloadAmmo()
    {
        WeaponSlot weaponSlot = this as WeaponSlot;
        if (weaponSlot)
        {
            return weaponSlot.GetWeapon().UnloadAmmo();
        }
        else
        {
            int loadedAmmo = GetItemStack().loadedAmmo;
            GetItemStack().loadedAmmo = 0;
            return loadedAmmo;
        }
    }
}
