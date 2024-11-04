using System;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] bool isOpen;
    CharacterData playerCharData;

    public static Action onInventoryOpened;
    public static Action onInventoryClosed;

    public void InitInventory(CharacterData charData)
    {
        playerCharData = charData;
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

        //if(Input.GetKeyDown(KeyCode.Mouse1) && isOpen == true)
        //{
        //    if(itemPickup.inventorySlot != null)
        //    {
        //        if(itemPickup.inventorySlot.itemObject.itemType == ItemObject.ItemType.consumable)
        //        UseItem((ConsumableItemObject)itemPickup.inventorySlot.itemObject);
        //    }
        //}
    }

    //void UseItem(ConsumableItemObject item)
    //{
    //    if(item.resourceToReplenish == "health")
    //    {
    //        //if(playerHealth.currentHealth < playerHealth.maxHealth)
    //        //{
    //        //    playerHealth.Heal(item.replenishAmount);
    //        //    itemPickup.inventorySlot.RemoveItem();
    //        //}
    //    }
    //    else if(item.resourceToReplenish == "mana")
    //    {
    //        if (playerMana.currentPlayerMana < playerMana.maxPlayerMana)
    //        {
    //            playerMana.ReplenishMana(item.replenishAmount);
    //            itemPickup.inventorySlot.RemoveItem();
    //        }
    //    }
    //}

    public void ToggleInventory()
    {
        if (isOpen == true)
        {
            CloseInventory();
        }
        else if (isOpen == false)
        {
            OpenInventory();
        }
    }

    private void OpenInventory()
    {
        isOpen = true;
        onInventoryOpened?.Invoke();
    }

    public void CloseInventory()
    {
        isOpen = false;
        onInventoryClosed?.Invoke();
    }

}
