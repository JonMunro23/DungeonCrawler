using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public GameObject inventory;
    ItemPickup itemPickup;
    PlayerHealth playerHealth;
    PlayerMana playerMana;

    bool isOpen = false;

    private void Awake()
    {
        itemPickup = GetComponent<ItemPickup>();
        playerHealth = GetComponent<PlayerHealth>();
        playerMana = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMana>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            CloseInventory();
        }

        if(Input.GetKeyDown(KeyCode.Mouse1) && isOpen == true)
        {
            if(itemPickup.inventorySlot != null)
            {
                if(itemPickup.inventorySlot.itemObject.itemType == ItemObject.ItemType.consumable)
                UseItem((ConsumableItemObject)itemPickup.inventorySlot.itemObject);
            }
        }
    }

    void UseItem(ConsumableItemObject item)
    {
        if(item.resourceToReplenish == "health")
        {
            if(playerHealth.currentPlayerHealth < playerHealth.maxPlayerHealth)
            {
                playerHealth.Heal(item.replenishAmount);
                itemPickup.inventorySlot.RemoveItem();
            }
        }
        else if(item.resourceToReplenish == "mana")
        {
            if (playerMana.currentPlayerMana < playerMana.maxPlayerMana)
            {
                playerMana.ReplenishMana(item.replenishAmount);
                itemPickup.inventorySlot.RemoveItem();
            }
        }
    }

    public void ToggleInventory()
    {
        if (isOpen == true)
        {
            CloseInventory();
        }
        else if (isOpen == false)
        {
            isOpen = true;
            inventory.SetActive(true);
        }
    }

    public void CloseInventory()
    {
        isOpen = false;
        inventory.SetActive(false);
        if (itemPickup.inventorySlot != null)
        {
            itemPickup.inventorySlot.HideTooltipDisplay();
        }
        itemPickup.inventorySlot = null;
    }

}
