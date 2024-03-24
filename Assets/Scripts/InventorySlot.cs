using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    ItemPickup itemPickup;
    PlayerHealth playerHealth;
    UseEquipment useEquipment;

    public ItemObject itemObject;
    public int itemAmount;
    public TMP_Text itemAmountText;
    public RawImage itemImage;
    [SerializeField]
    GameObject tooltipDisplay;

    Vector3 mousePos;

    GameObject displayClone;

    public bool isSlotOccupied, isCharEquipSlot;
    bool isTooltipDisplayOpen;

    public string charEquipSlotName;

    private void Awake()
    {
        itemPickup = GameObject.FindGameObjectWithTag("Player").GetComponent<ItemPickup>();
        playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealth>();
        useEquipment = GameObject.FindGameObjectWithTag("Player").GetComponent<UseEquipment>();
    }

    private void Update()
    {
        if(isTooltipDisplayOpen == true)
        {
            mousePos = Input.mousePosition;
            displayClone.transform.position = mousePos;
        }
    }

    public void InitialiseItem()
    {
        itemImage.texture = itemObject.itemTexture;
        itemImage.enabled = true;
        if (itemAmount > 1)
            itemAmountText.text = itemAmount.ToString();
        else
            itemAmountText.text = "";
        if(isCharEquipSlot == true)
        {
            InitialiseStatBonuses();
            
            if(charEquipSlotName == "main hand")
            {
                useEquipment.InitialiseHandItem(Hands.left, itemObject);
                return;

            }
            if (charEquipSlotName == "off hand")
            {
                useEquipment.InitialiseHandItem(Hands.right, itemObject);

            }
        }
    }

    void InitialiseStatBonuses()
    {
        playerHealth.IncreaseArmour(itemObject.armourBonus);
        playerHealth.IncreaseEvasion(itemObject.evasionBonus);
    }

    public void RemoveItem()
    {
        itemAmount -= 1;
        if (itemAmount == 0)
        {
            if (isCharEquipSlot == true)
            {
                RemoveStatBonuses();
            }
            if (charEquipSlotName == "main hand")
            {
                useEquipment.RemoveHandItem(Hands.left);

            }
            if (charEquipSlotName == "off hand")
            {
                useEquipment.RemoveHandItem(Hands.right);

            }
            itemObject = null;
            itemImage.texture = null;
            itemImage.enabled = false;
            itemAmount = 0;
            itemAmountText.text = "";
            isSlotOccupied = false;
        }
        else
        {
            itemAmountText.text = itemAmount.ToString();
        }
    }

    public void MoveItem()
    {
        if (isCharEquipSlot == true)
        {
            RemoveStatBonuses();
        }
        if (charEquipSlotName == "main hand")
        {
            useEquipment.RemoveHandItem(Hands.left);
        }
        if (charEquipSlotName == "off hand")
        {
            useEquipment.RemoveHandItem(Hands.right);

        }
        itemObject = null;
        itemImage.texture = null;
        itemImage.enabled = false;
        itemAmount = 0;
        itemAmountText.text = "";
        isSlotOccupied = false;
    }

    void RemoveStatBonuses()
    {
        playerHealth.DecreaseArmour(itemObject.armourBonus);
        playerHealth.DecreaseEvasion(itemObject.evasionBonus);
    }

    public void ShowTooltipDisplay(ItemObject itemToDisplay)
    {
        if(itemPickup.inventorySlot != null)
        {
                displayClone = Instantiate(tooltipDisplay, mousePos, Quaternion.identity, itemPickup.canvasTransform);
                displayClone.transform.localScale = new Vector3(2, 2, 2);
                isTooltipDisplayOpen = true;
                TooltipDisplay display = displayClone.GetComponent<TooltipDisplay>();
                display.itemNameText.text = itemToDisplay.itemName;
                display.itemTypeText.text = itemToDisplay.itemType.ToString();
                display.itemWeightText.text = itemToDisplay.itemWeight.ToString() + "kg";
                display.itemValueText.text = itemToDisplay.itemValue.ToString() + "g";
                display.itemDescriptionText.text = itemToDisplay.itemDescription;

                display.itemPortrait.texture = itemToDisplay.itemTexture;

            if (itemToDisplay.itemType == ItemObject.ItemType.meleeWeapon || itemToDisplay.itemType == ItemObject.ItemType.rangedWeapon)
            {
                display.itemDamageText.text = itemToDisplay.itemDamageMin + " - " + itemToDisplay.itemDamageMax + " " + itemToDisplay.itemDamageType;
                display.itemCooldownText.text = itemToDisplay.itemCooldown.ToString() + " seconds";

            }
            else if (itemToDisplay.itemType == ItemObject.ItemType.consumable)
            {
                //display consumable information on tooltipDisplay
            }
        }
    }

    public void HideTooltipDisplay()
    {
        isTooltipDisplayOpen = false;
        Destroy(displayClone);
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        itemPickup.inventorySlot = eventData.pointerCurrentRaycast.gameObject.transform.parent.GetComponent<InventorySlot>();
        if (itemPickup.inventorySlot.isSlotOccupied == true)
        {
            ShowTooltipDisplay(eventData.pointerCurrentRaycast.gameObject.transform.parent.GetComponent<InventorySlot>().itemObject);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        itemPickup.inventorySlot = null;
        HideTooltipDisplay();
    }
}
