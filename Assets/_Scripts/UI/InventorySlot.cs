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


    public static event Action<ISlot> onInventorySlotLeftClicked;
    public static event Action<ISlot> onInventorySlotRightClicked;

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
        currentSlotItemStack = new ItemStack(itemToAdd.Item, itemToAdd.ItemAmount);

        ConsumableItemData consumableData = GetDataAsConsumable(itemToAdd.Item.ItemData);
        if (consumableData)
        {
            if (consumableData.consumableType == ConsumableType.HealSyringe)
            {
                playerInventoryManager.AddHealthSyringe(itemToAdd.ItemAmount);
            }
            
        }

        AmmoItemData ammoData = GetDataAsAmmo(itemToAdd.Item.ItemData);
        if (ammoData)
        {
            playerInventoryManager.AddAmmo(ammoData);
        }

        ThrowableItemData throwableData = itemToAdd.Item.ItemData as ThrowableItemData;
        if(throwableData != null)
        {
            playerInventoryManager.playerController.playerThrowableManager.AddThrowableToAvailable(throwableData, itemToAdd.ItemAmount);
        }

        SetTooltipTriggerActive(true);
        UpdateSlotUI();
    }

    public int AddToCurrentItemStack(int amountToAdd)
    {
        int remainder = 0;
        int availableSpace = currentSlotItemStack.Item.ItemData.maxItemStackSize - currentSlotItemStack.ItemAmount;
        if (availableSpace < amountToAdd)
        {
            AddToSlotStack(availableSpace);
            remainder = amountToAdd - availableSpace;
        }
        else if(availableSpace >= amountToAdd)
        {
            AddToSlotStack(amountToAdd);
        }

        UpdateSlotUI();

        return remainder;
    }

    void AddToSlotStack(int amountToAdd)
    {
        currentSlotItemStack.AddToStack(amountToAdd);

        ConsumableItemData consumableData = GetDataAsConsumable(currentSlotItemStack.Item.ItemData);
        if (consumableData)
        {
            if (consumableData.consumableType == ConsumableType.HealSyringe)
            {
                playerInventoryManager.AddHealthSyringe(amountToAdd);
            }
        }

        AmmoItemData ammoData = GetDataAsAmmo(currentSlotItemStack.Item.ItemData);
        if (ammoData)
        {
            playerInventoryManager.AddAmmo(ammoData);
        }

        ThrowableItemData throwableData = currentSlotItemStack.Item.ItemData as ThrowableItemData;
        if (throwableData != null)
        {
            playerInventoryManager.playerController.playerThrowableManager.AddThrowableToAvailable(throwableData, amountToAdd);
        }

    }

    public int RemoveFromExistingStack(int amountToRemove)
    {
        int remainder = 0;
        if (currentSlotItemStack.ItemAmount <= amountToRemove)
        {
            amountToRemove -= currentSlotItemStack.ItemAmount;
            RemoveItem();
            remainder = amountToRemove;
        }
        else if (currentSlotItemStack.ItemAmount >= amountToRemove)
        {
            currentSlotItemStack.RemoveFromStack(amountToRemove);
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
        ItemStack itemToTake = new ItemStack(currentSlotItemStack.Item, currentSlotItemStack.ItemAmount);
        ThrowableItemData throwableTaken = itemToTake.Item.ItemData as ThrowableItemData;
        if (throwableTaken != null)
        {
            playerInventoryManager.playerController.playerThrowableManager.RemoveThrowableFromAvailable(throwableTaken, itemToTake.ItemAmount);
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
        ItemStack oldItem = new ItemStack(currentSlotItemStack.Item, currentSlotItemStack.ItemAmount);

        currentSlotItemStack = itemToSwap;
        UpdateSlotUI();

        return oldItem;
    }

    public void UseItem()
    {
        if (currentSlotItemStack != null)
        {
            currentSlotItemStack.RemoveFromStack(1);
            UpdateSlotUI();
            if (currentSlotItemStack.ItemAmount == 0)
                RemoveItem();
        }
    }

     public void UpdateSlotUI()
    {
        UpdateTooltipData();

        EquipmentSlot equipmentSlot = this as EquipmentSlot;
        WeaponSlot weaponSlot = this as WeaponSlot;


        if (currentSlotItemStack == null)
        {
            if (!equipmentSlot && !weaponSlot)
            {
                slotImage.enabled = false;
            }
            SlotAmountText.text = "";
            return;
        }
        slotImage.color = Color.white;
        slotImage.sprite = currentSlotItemStack.Item.ItemData.itemSprite;
        slotImage.enabled = true;
        if (currentSlotItemStack.ItemAmount > 1)
            SlotAmountText.text = currentSlotItemStack.ItemAmount.ToString();
        else
            SlotAmountText.text = "";
    }

    public void UpdateTooltipData()
    {
        if (!tooltipTrigger)
            return;

        if (currentSlotItemStack == null)
            return;

        tooltipTrigger.SetImage("ItemImage", currentSlotItemStack.Item.ItemData.itemSprite);
        tooltipTrigger.SetText("TitleText", currentSlotItemStack.Item.ItemData.itemName);
        tooltipTrigger.SetText("Description", currentSlotItemStack.Item.ItemData.itemDescription);
        tooltipTrigger.SetText("Stats", string.Empty);

        WeaponItem weaponItem = currentSlotItemStack.Item as WeaponItem;

        EquipmentItemData equipmentItem = currentSlotItemStack.Item.ItemData as EquipmentItemData;
        if(equipmentItem)
        {
            StringBuilder statsText = new StringBuilder();
            if(weaponItem != null)
            {
                statsText.AppendLine($"Loaded Ammo: {weaponItem.LoadedAmmo}");
                statsText.AppendLine($"Loaded Ammo Type: " +
                    $"{(weaponItem.LoadedAmmoData != null ? weaponItem.LoadedAmmoData.ammoType : "None")}");
            }
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
        ConsumableItemData consumableItemData = GetDataAsConsumable(currentSlotItemStack.Item.ItemData);
        if (consumableItemData)
        {
            switch (consumableItemData.consumableType)
            {
                case ConsumableType.HealSyringe:
                    playerInventoryManager.RemoveHealthSyringe(currentSlotItemStack.ItemAmount);
                    break;
            }
        }

        //ThrowableItemData throwableItemData = currentSlotItemStack.itemData as ThrowableItemData;
        //if (throwableItemData)
        //{

        //}



        //AmmoItemData ammoData = GetDataAsAmmo(currentSlotItemStack.itemData);
        //if (ammoData)
        //{
        //    playerInventoryManager.RemoveAmmo(ammoData.ammoWeaponType, currentSlotItemStack.itemAmount);
        //}

        currentSlotItemStack = null;
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
        //Debug.Log(currentSlotItemStack);
        return currentSlotItemStack != null ? false : true;
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
        WeaponItem weaponItem = GetItemStack().Item as WeaponItem;
        if (weaponItem != null)
        {
            int loadedAmmo = weaponItem.LoadedAmmo;
            weaponItem.SetLoadedAmmo(0);
            weaponItem.SetLoadedAmmoType(null);
            UpdateTooltipData();
            return loadedAmmo;
        }
        else
            return 0;
    }
}
