using UnityEngine;
using System;
using System.Collections.Generic;

public class ItemPickupManager : MonoBehaviour
{
    PlayerInventoryManager inventoryManager;
    PlayerWeaponManager playerWeaponManager;

    [SerializeField] Transform thrownItemSpawnLocation;
    [SerializeField] float throwVeloctiy;
    public Vector3 mousePos = Vector3.zero;
    public ItemStack currentGrabbedItem = null;
    public bool hasGrabbedItem, canPickUpItem = true;
    float maxGrabDistance = 3;

    [SerializeField] List<WorldItem> groundItems = new List<WorldItem>();
    IContainer nearbyContainer;

    public static Action<ItemStack> onNewItemAttachedToCursor;
    public static Action onCurrentItemDettachedFromCursor;

    public static Action<ItemStack> onGroundItemsUpdated;
    public static Action onLastGroundItemRemoved;

    public static Action<IContainer> onNearbyContainerUpdated;

    private void OnEnable()
    {
        WorldItem.onWorldItemGrabbed += OnWorldItemGrabbed;
        InventorySlot.onInventorySlotClicked += OnInventorySlotClicked;
        ContainerSlot.onContainerItemGrabbed += OnContainerItemGrabbed;
    }

    private void OnDisable()
    {
        WorldItem.onWorldItemGrabbed -= OnWorldItemGrabbed;
        InventorySlot.onInventorySlotClicked -= OnInventorySlotClicked;
        ContainerSlot.onContainerItemGrabbed -= OnContainerItemGrabbed;
    }

    private void Awake()
    {
        inventoryManager = GetComponent<PlayerInventoryManager>();
        playerWeaponManager = GetComponent<PlayerWeaponManager>();
    }

    void OnWorldItemGrabbed(WorldItem worldItemGrabbed)
    {
        if (hasGrabbedItem)
            return;

        PlayGrabAnim();

        groundItems.Remove(worldItemGrabbed);
        UpdatePickupItemUI();

        AttachItemToMouseCursor(worldItemGrabbed.item, worldItemGrabbed);
    }

    void OnContainerItemGrabbed(ContainerSlot slotGrabbedFrom)
    {
        if (hasGrabbedItem)
            return;

        PlayGrabAnim();
        ItemStack slotItem = slotGrabbedFrom.storedStack;
        AttachItemToMouseCursor(slotItem);
        slotGrabbedFrom.RemoveItemStack();

    }

    void OnInventorySlotClicked(ISlot slotClicked)
    {
        if (!slotClicked.IsInteractable())
            return;

        if (!hasGrabbedItem)
        {
            if (!slotClicked.IsSlotEmpty())
            {
                AttachItemToMouseCursor(slotClicked.TakeItem());
                return;
            }
        }
        else
        {
            if (slotClicked.IsSlotEmpty())
            {
                slotClicked.AddItem(currentGrabbedItem);
                DetachItemFromMouseCursor();
                return;
            }
            else
            {
                if(slotClicked.GetItemStack().itemData == currentGrabbedItem.itemData)
                {
                    int remainder = slotClicked.AddToCurrentItemStack(currentGrabbedItem.itemAmount);
                    if (remainder > 0)
                    {
                        DetachItemFromMouseCursor();
                        AttachItemToMouseCursor(new ItemStack(slotClicked.GetItemStack().itemData, remainder, slotClicked.GetItemStack().loadedAmmo));
                    }
                    else
                        DetachItemFromMouseCursor();

                    return;
                }
                else
                {
                    AttachItemToMouseCursor(slotClicked.SwapItem(currentGrabbedItem));
                }

            }
        }

    }

    void AttachItemToMouseCursor(ItemStack itemToAttach, WorldItem worldItem = null)
    {
        currentGrabbedItem = new ItemStack(itemToAttach.itemData, itemToAttach.itemAmount, itemToAttach.loadedAmmo);

        onNewItemAttachedToCursor?.Invoke(currentGrabbedItem);
        
        if(worldItem)
            Destroy(worldItem.gameObject);

        hasGrabbedItem = true;
    }

    void DetachItemFromMouseCursor()
    {
        onCurrentItemDettachedFromCursor?.Invoke();

        currentGrabbedItem.itemData = null;
        currentGrabbedItem.itemAmount = 0;
        hasGrabbedItem = false;
    }

    void PlaceGrabbedItemInWorld(Vector3 placementLocation)
    {
        if (!hasGrabbedItem)
            return;

        //WorldItem spawnedWorldItem = Instantiate(currentGrabbedItem.itemData.itemWorldModel, placementLocation, Quaternion.identity);
        //spawnedWorldItem.InitWorldItem(currentGrabbedItem);
        //DetachItemFromMouseCursor();
    }

    void ThrowGrabbedItemIntoWorld()
    {
        //WorldItem spawnedWorldItem = Instantiate(currentGrabbedItem.itemData.itemWorldModel, thrownItemSpawnLocation.position, Quaternion.identity);
        //spawnedWorldItem.GetComponent<Rigidbody>().AddForce(thrownItemSpawnLocation.forward * throwVeloctiy * Time.deltaTime, ForceMode.Impulse);
        //spawnedWorldItem.item.itemData = currentGrabbedItem.itemData;
        //spawnedWorldItem.item.itemAmount = currentGrabbedItem.itemAmount;

        //DetachItemFromMouseCursor();
    }

    // Update is called once per frame
    void Update()
    {
        if (hasGrabbedItem == true)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    //Debug.Log(hit.distance);
                    if (hit.transform.CompareTag("Ground") && hit.distance < maxGrabDistance)
                    {
                        PlaceGrabbedItemInWorld(hit.point);
                    }
                    //else if (hit.distance > maxGrabDistance)
                    //{
                    //    ThrowGrabbedItemIntoWorld();
                    //}
                }
            }
        }
    }

    /// <summary>
    /// Called from InputHandler on key press
    /// </summary>
    public void TryPickupGroundItem()
    {
        if (groundItems.Count > 0)
        {
            PickupItem(groundItems[0]);
            return;
        }

        if(nearbyContainer != null)
        {
            //playerWeaponManager.currentWeapon.HolsterWeapon();
            PlayGrabAnim();
            nearbyContainer.ToggleContainer();
        }
    }

    void PickupItem(WorldItem itemToPickup)
    {
        int remainingItems = inventoryManager.TryAddItemToInventory(itemToPickup.item);
        if(remainingItems != itemToPickup.item.itemAmount)
        {
            PlayGrabAnim();

            if (remainingItems == 0)
            {
                Destroy(itemToPickup.gameObject);
                groundItems.Remove(itemToPickup);
                UpdatePickupItemUI();
            }
            else
            {
                itemToPickup.item.itemAmount = remainingItems;

            }
        }

    }

    private void PlayGrabAnim()
    {
        if (playerWeaponManager.currentWeapon != null)
            playerWeaponManager.currentWeapon.Grab();
    }

    private void UpdatePickupItemUI()
    {
        if (groundItems.Count > 0)
            onGroundItemsUpdated?.Invoke(groundItems[0].item);
        else
            onLastGroundItemRemoved?.Invoke();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out WorldItem worldItem))
        {
            groundItems.Add(worldItem);
            onGroundItemsUpdated?.Invoke(groundItems[0].item);
            return;
        }

        if(other.TryGetComponent(out IContainer container))
        {
            nearbyContainer = container;
            onNearbyContainerUpdated?.Invoke(nearbyContainer);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (groundItems.Count > 0)
        {
            if (other.TryGetComponent(out WorldItem worldItem))
            {
                if(groundItems.Contains(worldItem))
                    groundItems.Remove(worldItem);
            }

            if (groundItems.Count == 0)
                onLastGroundItemRemoved?.Invoke();
        }

        if(nearbyContainer != null)
        {
            if (other.TryGetComponent(out IContainer container))
                if (container == nearbyContainer)
                {
                    nearbyContainer.CloseContainer();
                    playerWeaponManager.currentWeapon.DrawWeapon();
                    nearbyContainer = null;
                    onNearbyContainerUpdated?.Invoke(nearbyContainer);
                }
        }
    }
}
