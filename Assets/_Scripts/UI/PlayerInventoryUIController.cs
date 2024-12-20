using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventoryUIController : MonoBehaviour
{
    [SerializeField] GameObject InventoryParentObject;
    [SerializeField] GameObject InventoryObject;
    [SerializeField] InventorySlot inventorySlot;
    [SerializeField] InventorySlot[] spawnedInventorySlots;
    [SerializeField] GridLayoutGroup invSlotSpawnParent;
    public static Action<InventorySlot[]> onInventorySlotsSpawned;

    [SerializeField] TMP_Text pickupItemText;

    [Header("Syringe")]
    [SerializeField] TMP_Text syringeAmountText;

    [Header("Context Menu")]
    [SerializeField] InventoryContextMenu contextMenu;

    public static bool isInventoryOpen, isContextMenuOpen;

    private void OnEnable()
    {
        PlayerInventoryManager.onInventoryOpened += OnInventoryOpened;
        PlayerInventoryManager.onInventoryClosed += OnInventoryClosed;
        PlayerInventoryManager.onInventorySlotsSpawned += OnInventorySlotsSpawned;
        PlayerInventoryManager.onSyringeCountUpdated += OnSyringeCountUpdated;

        WorldInteractionManager.onGroundItemsUpdated += OnNewGroundItemDetected;
        WorldInteractionManager.onLastGroundItemRemoved += OnLastGroundItemRemoved;
        WorldInteractionManager.onNearbyContainerUpdated += OnContainerDetected;

        InventorySlot.onInventorySlotRightClicked += ShowContextMenu;
        InventorySlot.onInventorySlotLeftClicked += HideContextMenu;
    }

    private void OnDisable()
    {
        PlayerInventoryManager.onInventoryOpened -= OnInventoryOpened;
        PlayerInventoryManager.onInventoryClosed -= OnInventoryClosed;
        PlayerInventoryManager.onInventorySlotsSpawned -= OnInventorySlotsSpawned;
        PlayerInventoryManager.onSyringeCountUpdated -= OnSyringeCountUpdated;

        WorldInteractionManager.onGroundItemsUpdated -= OnNewGroundItemDetected;
        WorldInteractionManager.onLastGroundItemRemoved -= OnLastGroundItemRemoved;
        WorldInteractionManager.onNearbyContainerUpdated -= OnContainerDetected;

        InventorySlot.onInventorySlotRightClicked -= ShowContextMenu;
        InventorySlot.onInventorySlotLeftClicked -= HideContextMenu;

    }

    void OnNewGroundItemDetected(ItemStack detectedItem)
    {
        pickupItemText.text = $"Press F to pickup {(detectedItem.itemAmount > 0 ? detectedItem.itemAmount : "")} {detectedItem.itemData.itemName}.";
    }

    void OnContainerDetected(IContainer container)
    {
        if(container == null)
        {
            pickupItemText.text = "";

            return;
        }

        pickupItemText.text = $"Press F to {(container.IsOpen() ? "close" : "open")} container.";
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
            slot.transform.SetParent(invSlotSpawnParent.transform, false);
        }
    }

    private void Start()
    {
        OnInventoryClosed();
    }

    void OnInventoryOpened()
    {
        InventoryParentObject.SetActive(true);
        InventoryObject.SetActive(true);

        HideContextMenu();

        HelperFunctions.SetCursorActive(true);

        isInventoryOpen = true;
    }

    void OnInventoryClosed()
    {
        InventoryParentObject.SetActive(false);
        InventoryObject.SetActive(false);

        if(!PlayerInventoryManager.isInContainer && !WorldInteractionManager.hasGrabbedItem)
            HelperFunctions.SetCursorActive(false);

        isInventoryOpen = false;
    }

    public void CloseInventory()
    {
        OnInventoryClosed();
    }

    void OnSyringeCountUpdated(int newSyringeCount)
    {
        syringeAmountText.text = newSyringeCount.ToString();
    }

    void ShowContextMenu(ISlot slot)
    {
        slot.HideTooltip();
        contextMenu.gameObject.SetActive(true);
        contextMenu.transform.position = Input.mousePosition;
        contextMenu.Init(slot);
        //update context menu buttons depending on item in slot
    }

    void HideContextMenu(ISlot slot = null)
    {
        if(slot != null)
            if(!slot.IsSlotEmpty())
                slot.ShowTooltip();

        contextMenu.gameObject.SetActive(false);
    }
}
