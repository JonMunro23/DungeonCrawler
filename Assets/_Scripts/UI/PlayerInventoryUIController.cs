using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventoryUIController : MonoBehaviour
{
    [SerializeField] GameObject InventoryUIObject;
    [SerializeField] InventorySlot inventorySlot;
    [SerializeField] InventorySlot[] spawnedInventorySlots;
    [SerializeField] GridLayoutGroup invSlotSpawnParent;
    public static Action<InventorySlot[]> onInventorySlotsSpawned;

    private void OnEnable()
    {
        PlayerInventoryManager.onInventoryOpened += OnInventoryOpened;
        PlayerInventoryManager.onInventoryClosed += OnInventoryClosed;
        PlayerInventoryManager.onInventorySlotsSpawned += OnInventorySlotsSpawned;
    }

    private void OnDisable()
    {
        PlayerInventoryManager.onInventoryOpened -= OnInventoryOpened;
        PlayerInventoryManager.onInventoryClosed -= OnInventoryClosed;
        PlayerInventoryManager.onInventorySlotsSpawned -= OnInventorySlotsSpawned;
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
