using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventoryUIController : MonoBehaviour
{
    [SerializeField] GameObject InventoryUIObject;
    [SerializeField] InventorySlot inventorySlot;
    [SerializeField] InventorySlot[] spawnedInventorySlots;
    [SerializeField] GridLayoutGroup invSlotSpawnParent;
    public static Action<InventorySlot[]> onInventorySlotsSpawned;

    [SerializeField] TMP_Text pickupItemText;

    private void OnEnable()
    {
        PlayerInventoryManager.onInventoryOpened += OnInventoryOpened;
        PlayerInventoryManager.onInventoryClosed += OnInventoryClosed;
        PlayerInventoryManager.onInventorySlotsSpawned += OnInventorySlotsSpawned;
        ItemPickupManager.onGroundItemsUpdated += OnNewGroundItemDetected;
        ItemPickupManager.onLastGroundItemRemoved += OnLastGroundItemRemoved;
    }

    private void OnDisable()
    {
        PlayerInventoryManager.onInventoryOpened -= OnInventoryOpened;
        PlayerInventoryManager.onInventoryClosed -= OnInventoryClosed;
        PlayerInventoryManager.onInventorySlotsSpawned -= OnInventorySlotsSpawned;
    }

    void OnNewGroundItemDetected(ItemStack detectedItem)
    {
        pickupItemText.text = $"Press F to pickup {(detectedItem.itemAmount > 0 ? detectedItem.itemAmount : "")} {detectedItem.itemData.itemName}.";
    }

    void OnLastGroundItemRemoved()
    {
        pickupItemText.text = "";
    }

    void OnInventorySlotsSpawned(InventorySlot[] spawnedSlots)
    {
        spawnedInventorySlots = spawnedSlots;

        foreach (InventorySlot slot in spawnedInventorySlots)
        {
            slot.transform.SetParent(invSlotSpawnParent.transform);
            slot.transform.localScale = Vector3.one;
        }
    }

    public void InitPlayerInventory()
    {
        
    }

    private void Start()
    {
        OnInventoryClosed();
    }

    void OnInventoryOpened()
    {
        InventoryUIObject.SetActive(true);
    }

    void OnInventoryClosed()
    {
        InventoryUIObject.SetActive(false);
    }
}
