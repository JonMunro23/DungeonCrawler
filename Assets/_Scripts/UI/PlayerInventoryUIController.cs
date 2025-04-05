using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventoryUIController : MonoBehaviour
{
    //[SerializeField] GameObject InventoryParentObject;
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

    public static bool isContextMenuOpen;

    private void OnEnable()
    {
        //PlayerInventoryManager.onInventoryOpened += OpenInventory;
        //PlayerInventoryManager.onInventoryClosed += CloseInventory;
        PlayerInventoryManager.onInventorySlotsSpawned += OnInventorySlotsSpawned;
        PlayerInventoryManager.onSyringeCountUpdated += OnSyringeCountUpdated;

        WorldInteractionManager.onGroundItemsUpdated += OnNewGroundItemDetected;
        WorldInteractionManager.onLastGroundItemRemoved += OnLastGroundItemRemoved;
        WorldInteractionManager.onNearbyContainerUpdated += OnNearbyContainerUpdated;
        WorldInteractionManager.onNearbyInteractableUpdated += OnNearbyInteractableUpdated;

        InventorySlot.onInventorySlotRightClicked += ShowContextMenu;
    }

    private void OnDisable()
    {
        //PlayerInventoryManager.onInventoryOpened -= OpenInventory;
        //PlayerInventoryManager.onInventoryClosed -= CloseInventory;
        PlayerInventoryManager.onInventorySlotsSpawned -= OnInventorySlotsSpawned;
        PlayerInventoryManager.onSyringeCountUpdated -= OnSyringeCountUpdated;

        WorldInteractionManager.onGroundItemsUpdated -= OnNewGroundItemDetected;
        WorldInteractionManager.onLastGroundItemRemoved -= OnLastGroundItemRemoved;
        WorldInteractionManager.onNearbyContainerUpdated -= OnNearbyContainerUpdated;

        InventorySlot.onInventorySlotRightClicked -= ShowContextMenu;

    }

    void OnNewGroundItemDetected(ItemStack detectedItem)
    {
        pickupItemText.text = $"Press F to pickup {(detectedItem.itemAmount > 0 ? detectedItem.itemAmount : "")} {detectedItem.itemData.itemName}.";
    }

    void OnNearbyContainerUpdated(IContainer container)
    {
        if(container == null)
        {
            pickupItemText.text = "";

            return;
        }

        pickupItemText.text = $"Press F to {(container.IsOpen() ? "close" : "open")} container.";
    }

    void OnNearbyInteractableUpdated(IInteractable interactable)
    {
        if (interactable == null)
        {
            pickupItemText.text = "";

            return;
        }

        pickupItemText.text = "Press F to interact.";
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
        CloseInventory();
    }

    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.Mouse0))
        {
            if (contextMenu.gameObject.activeSelf)
                HideContextMenu();
        }
    }

    public void OpenInventory()
    {
        //InventoryParentObject.SetActive(true);
        InventoryObject.SetActive(true);

        HideContextMenu();

        HelperFunctions.SetCursorActive(true);
    }

    public void CloseInventory()
    {
        //InventoryParentObject.SetActive(false);
        InventoryObject.SetActive(false);

        //if(!PlayerInventoryManager.isInContainer && !WorldInteractionManager.hasGrabbedItem && !MainMenu.isInMainMenu && !MainInventoryUIController.isCharacterMenuOpen)
        //    HelperFunctions.SetCursorActive(false);
    }

    //public void CloseInventory()
    //{
    //    CloseInventory();
    //}

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
    }

    void HideContextMenu(ISlot slot = null)
    {
        if(slot != null)
            if(!slot.IsSlotEmpty())
                slot.ShowTooltip();

        contextMenu.gameObject.SetActive(false);
    }
}
