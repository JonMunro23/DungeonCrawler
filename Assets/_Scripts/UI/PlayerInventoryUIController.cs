using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventoryUIController : MonoBehaviour
{
    [SerializeField] GameObject InventoryUIObject;
    [SerializeField] InventorySlot inventorySlot;
    [SerializeField] GridLayoutGroup inventoryLayoutGroup;
    [SerializeField] int numInventorySlots;
    [SerializeField] List<InventorySlot> spawnedInventorySlots = new List<InventorySlot>();

    private void OnEnable()
    {
        PlayerInventory.onInventoryOpened += OnInventoryOpened;
        PlayerInventory.onInventoryClosed += OnInventoryClosed;
    }

    private void OnDisable()
    {
        PlayerInventory.onInventoryOpened -= OnInventoryOpened;
        PlayerInventory.onInventoryClosed -= OnInventoryClosed;
    }

    public void InitPlayerInventory()
    {
        spawnedInventorySlots.Clear();
        for (int i = 0; i < numInventorySlots; i++)
        {
            spawnedInventorySlots.Add(Instantiate(inventorySlot, inventoryLayoutGroup.transform));
        }
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
