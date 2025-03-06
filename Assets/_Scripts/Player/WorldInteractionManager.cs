using UnityEngine;
using System;
using System.Collections.Generic;

public class WorldInteractionManager : MonoBehaviour
{
    PlayerController playerController;
    [Header("References")]
    [SerializeField] WorldItem worldItemPrefab;
    [SerializeField] Transform itemDropLocation;
    //[SerializeField] Transform thrownItemSpawnLocation;
    //[SerializeField] float throwVeloctiy;
    [HideInInspector] public Vector3 mousePos = Vector3.zero;
    public ItemStack currentGrabbedItem = null;
    public static bool hasGrabbedItem;
    [HideInInspector] public bool canPickUpItem = true;
    float maxItemGrabDistance = 3;

    public AudioEmitter itemPickupAudioEmitter;
    public AudioClip grabSFX;
    public float grabSFXVolume;

    [SerializeField] List<WorldItem> groundItems = new List<WorldItem>();
    IContainer nearbyContainer;
    IInteractable nearbyInteractable;


    public static Action<ItemStack> onNewItemAttachedToCursor;
    public static Action onCurrentItemDettachedFromCursor;

    public static Action<ItemStack> onGroundItemsUpdated;
    public static Action onLastGroundItemRemoved;

    public static Action<IContainer> onNearbyContainerUpdated;
    public static Action<IInteractable> onNearbyInteractableUpdated;

    private void OnEnable()
    {
        WorldItem.onWorldItemGrabbed += OnWorldItemGrabbed;
        InventorySlot.onInventorySlotLeftClicked += OnInventorySlotClicked;
        ContainerSlot.onContainerItemGrabbed += OnContainerItemGrabbed;

        AdvancedGridMovement.onPlayerTurned += OnPlayerTurn;

        InventoryContextMenu.onInventorySlotItemDropped += DropItemFromInventoryIntoWorld;
    }

    private void OnDisable()
    {
        WorldItem.onWorldItemGrabbed -= OnWorldItemGrabbed;
        InventorySlot.onInventorySlotLeftClicked -= OnInventorySlotClicked;
        ContainerSlot.onContainerItemGrabbed -= OnContainerItemGrabbed;

        AdvancedGridMovement.onPlayerTurned -= OnPlayerTurn;

        InventoryContextMenu.onInventorySlotItemDropped += DropItemFromInventoryIntoWorld;
    }

    private void Start()
    {
        itemPickupAudioEmitter = AudioManager.Instance.RegisterSource("[AudioEmitter] CharacterBody", transform.root, spatialBlend: 0);
    }

    public void Init(PlayerController playerController)
    {
        this.playerController = playerController;
    }

    void OnPlayerTurn(int turnDir)
    {
        if (nearbyContainer == null)
            return;

        nearbyContainer.CloseContainer();
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

    public void DetachItemFromMouseCursor()
    {
        onCurrentItemDettachedFromCursor?.Invoke();

        currentGrabbedItem.itemData = null;
        currentGrabbedItem.itemAmount = 0;
        hasGrabbedItem = false;
    }

    void PlaceGrabbedItemInWorld(GridNode nodePlacedIn, Vector3 placementLocation)
    {
        if (!hasGrabbedItem)
            return;

        SpawnWorldItem(currentGrabbedItem, nodePlacedIn, placementLocation);
    }

    void DropItemFromInventoryIntoWorld(ISlot slot)
    {
        SpawnWorldItem(slot.TakeItem(), PlayerController.currentOccupiedNode, itemDropLocation.position);
    }

    void SpawnWorldItem(ItemStack itemStackToSpawn, GridNode nodePlacedIn, Vector3 placementLocation)
    {
        WorldItem worldItem = Instantiate(worldItemPrefab, placementLocation, Quaternion.Euler(new Vector3(0, playerController.advGridMovement.GetTargetRot(), 0)));
        worldItem.InitWorldItem(GridController.Instance.GetCurrentLevelIndex(), nodePlacedIn.Coords.Pos, itemStackToSpawn);
        worldItem.transform.GetChild(0).localPosition = new Vector3(worldItem.transform.GetChild(0).localPosition.x, worldItem.transform.GetChild(0).localPosition.y, 0);
        worldItem.GetComponent<BoxCollider>().center = Vector3.zero;
        DetachItemFromMouseCursor();

        if (!PlayerInventoryManager.isInContainer && !PlayerInventoryUIController.isInventoryOpen)
            HelperFunctions.SetCursorActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            RaycastHit hit;
            Ray ray = playerController.playerCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.distance < maxItemGrabDistance)
                {
                    if (hasGrabbedItem && hit.transform.CompareTag("Ground"))
                    {
                        GridNode node = hit.transform.GetComponentInParent<GridNode>();
                        if (!node)
                            return;

                        PlaceGrabbedItemInWorld(node, hit.point);
                        return;
                    }
                    else if(hit.transform.TryGetComponent(out IPickup pickup))
                    {
                        pickup.Pickup(true);
                    }
                    else if(hit.transform.TryGetComponent(out IInteractable interactable))
                    {
                        interactable.InteractWithItem(currentGrabbedItem.itemData);
                    }
                }
            }
        }
    }


    /// <summary>
    /// Called from InputHandler on key press
    /// </summary>
    public void Interact()
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

        if(nearbyInteractable != null)
        {
            PlayGrabAnim();
            if (currentGrabbedItem != null)
                nearbyInteractable.InteractWithItem(currentGrabbedItem.itemData);
            else
                nearbyInteractable.Interact();
        }
    }

    void PickupItem(WorldItem itemToPickup)
    {
        int remainingItems = playerController.playerInventoryManager.TryAddItemToInventory(itemToPickup.item);
        if(remainingItems != itemToPickup.item.itemAmount)
        {
            PlayGrabAnim(grabSFX);

            if (remainingItems == 0)
            {
                IPickup pickupInterface = itemToPickup;
                pickupInterface.Pickup();

                groundItems.Remove(itemToPickup);
                Destroy(itemToPickup.gameObject);
                UpdatePickupItemUI();
            }
            else
            {
                itemToPickup.item.itemAmount = remainingItems;

            }
        }

    }

    private void PlayGrabAnim(AudioClip grabSFX = null)
    {
        if (playerController.playerWeaponManager.currentWeapon != null && playerController.playerWeaponManager.currentWeapon.CanUse())
        {
            playerController.playerWeaponManager.currentWeapon.Grab();
            if(grabSFX != null)
                itemPickupAudioEmitter.ForcePlay(grabSFX, grabSFXVolume);
        }
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

        if(other.TryGetComponent(out IContainer nearbyContainer))
        {
            if(transform.root.localRotation.eulerAngles.y == other.transform.localRotation.eulerAngles.y)
            {
                this.nearbyContainer = nearbyContainer;
                onNearbyContainerUpdated?.Invoke(nearbyContainer);
            }
        }

        if(other.TryGetComponent(out IInteractable nearbyInteractable))
        {
            if (transform.root.localRotation.eulerAngles.y == other.transform.localRotation.eulerAngles.y)
            {
                this.nearbyInteractable = nearbyInteractable;
                onNearbyInteractableUpdated?.Invoke(nearbyInteractable);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out IContainer container))
        {
            if (transform.root.localRotation.eulerAngles.y == other.transform.localRotation.eulerAngles.y)
            {
                nearbyContainer = container;
            }
            else
                nearbyContainer = null;

            onNearbyContainerUpdated?.Invoke(nearbyContainer);
        }

        if (other.TryGetComponent(out IInteractable nearbyInteractable))
        {
            if (transform.root.localRotation.eulerAngles.y == other.transform.localRotation.eulerAngles.y)
            {
                this.nearbyInteractable = nearbyInteractable;
            }
            else
                this.nearbyInteractable = null;

            onNearbyInteractableUpdated?.Invoke(this.nearbyInteractable);
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
                    nearbyContainer = null;
                    onNearbyContainerUpdated?.Invoke(nearbyContainer);
                }
        }

        if(nearbyInteractable != null)
        {
            if(other.TryGetComponent(out IInteractable interactable))
                if(interactable == nearbyInteractable)
                {
                    nearbyInteractable = null;
                    onNearbyInteractableUpdated?.Invoke(nearbyInteractable);
                }
        }
    }
}
