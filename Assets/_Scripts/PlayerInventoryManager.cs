using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventoryManager : MonoBehaviour
{
    PlayerController playerController;

    [SerializeField] InventorySlot slotToSpawn;
    public InventorySlot[] spawnedInventorySlots;
    [SerializeField] int totalNumInventorySlots;
    [SerializeField] bool isOpen;

    [SerializeField] int heldHealthSyringes;

    public static Action onInventoryOpened;
    public static Action onInventoryClosed;
    public static Action<InventorySlot[]> onInventorySlotsSpawned;

    public void InitInventory(PlayerController newPlayerController)
    {
        playerController = newPlayerController;

        SpawnInventorySlots();
    }

    void SpawnInventorySlots()
    {
        spawnedInventorySlots = new InventorySlot[totalNumInventorySlots];

        for (int i = 0; i < totalNumInventorySlots; i++)
        {
            InventorySlot spawnedSlot = Instantiate(slotToSpawn);
            spawnedInventorySlots[i] = spawnedSlot;
            spawnedSlot.InitSlot(this);
        }

        onInventorySlotsSpawned?.Invoke(spawnedInventorySlots);
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
    }

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

    public bool HasHealthSyringe()
    {
        if(heldHealthSyringes > 0)
            return true;
        else
            return false;
    }

    public void AddHealthSyringe(int amountToAdd) => heldHealthSyringes += amountToAdd;

    public void RemoveHealthSyringe(int amountToRemove) => heldHealthSyringes -= amountToRemove;

    public void UseHealthSyringe()
    {
        InventorySlot syringeSlot = FindConsumableOfType(ConsumableType.HealSyringe);
        if(syringeSlot)
        {
            ConsumableItemData consumableData = syringeSlot.currentSlotItem.itemData as ConsumableItemData;
            if (!consumableData)
                return;

            playerController.playerHealthController.UseHealthSyringe(consumableData);
            syringeSlot.UseItem();
            heldHealthSyringes--;
        }
    }

    private InventorySlot FindConsumableOfType(ConsumableType typeToFind)
    {
        foreach (InventorySlot slot in spawnedInventorySlots)
        {
            if (!slot.currentSlotItem.itemData)
                continue;

            ConsumableItemData consumableData = slot.currentSlotItem.itemData as ConsumableItemData;
            if (!consumableData)
                continue;

            if (consumableData.consumableType == typeToFind)
            {
                return slot;
            }
        }

        return null;
    }
}
