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

    static IHighlightable highlightedTarget;
    public static IContainer currentOpenContainer;
    bool isLookingAtPickup, isLookingAtInteractable, isLookingAtContainer;

    public static Action<ItemStack> onNewItemAttachedToCursor;
    public static Action onCurrentItemDettachedFromCursor;

    public static Action<ItemStack> onGroundItemsUpdated;
    public static Action onLastGroundItemRemoved;

    public static Action<IContainer> onNearbyContainerUpdated;
    public static Action<IInteractable> onNearbyInteractableUpdated;

    public static Action<LookAtTarget> onLookAtTargetChanged;

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

    public static bool IsLookingAtInteractable() => highlightedTarget != null;

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
        slotGrabbedFrom.ClearSlot();

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
        WorldItem worldItem = Instantiate(worldItemPrefab, placementLocation, Quaternion.Euler(new Vector3(0, playerController.transform.localEulerAngles.y, 0)));
        worldItem.InitWorldItem(GridController.Instance.GetCurrentLevelIndex(), nodePlacedIn.Coords.Pos, itemStackToSpawn);
        worldItem.transform.GetChild(0).localPosition = new Vector3(worldItem.transform.GetChild(0).localPosition.x, worldItem.transform.GetChild(0).localPosition.y, 0);
        worldItem.GetComponent<BoxCollider>().center = Vector3.zero;
        DetachItemFromMouseCursor();

        HelperFunctions.SetCursorActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        Ray ray = playerController.playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, maxItemGrabDistance))
        {
            //Debug.DrawLine(ray.origin, hit.point, Color.yellow);
            if (hit.transform.TryGetComponent(out IPickup pickup))
            {
                if (highlightedTarget != null)
                    if (pickup != highlightedTarget)
                        highlightedTarget.SetHighlighted(false);

                pickup.SetHighlighted(true);
                highlightedTarget = pickup;
                onLookAtTargetChanged?.Invoke(LookAtTarget.Pickup);
            }
            else if (hit.transform.TryGetComponent(out IContainer container))
            {
                if (!container.IsOpen())
                {
                    if (highlightedTarget != null)
                        if (container != highlightedTarget)
                            highlightedTarget.SetHighlighted(false);

                    container.SetHighlighted(true);
                    highlightedTarget = container;
                    onLookAtTargetChanged?.Invoke(LookAtTarget.Container);
                }
            }
            else if (hit.transform.TryGetComponent(out IInteractable interactable))
            {
                if (highlightedTarget != null)
                    if (interactable != highlightedTarget)
                        highlightedTarget.SetHighlighted(false);

                interactable.SetHighlighted(true);
                highlightedTarget = interactable;
                onLookAtTargetChanged?.Invoke(LookAtTarget.Interactable);
            }
            else
            {
                ResetLookAtTarget();
            }
        }
        else
        {
            ResetLookAtTarget();
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (Physics.Raycast(ray, out hit, maxItemGrabDistance))
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
                    pickup.AddToInventory(playerController.playerInventoryManager);
                    PlayGrabAnim();
                }
                else if (hit.transform.TryGetComponent(out IContainer container))
                {
                    container.ToggleContainer();
                    currentOpenContainer = container;
                    PlayGrabAnim();
                }
                else if(hit.transform.TryGetComponent(out IInteractable interactable))
                {
                    interactable.InteractWithItem(currentGrabbedItem.itemData);
                    PlayGrabAnim();
                }
            }
        }
    }

    private void ResetLookAtTarget()
    {
        if (highlightedTarget != null)
        {
            highlightedTarget.SetHighlighted(false);
            highlightedTarget = null;
            onLookAtTargetChanged?.Invoke(LookAtTarget.None);
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
        int remainingItems = playerController.playerInventoryManager.TryAddItem(itemToPickup.item);
        if(remainingItems != itemToPickup.item.itemAmount)
        {
            PlayGrabAnim();

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

    private void PlayGrabAnim()
    {
        if (playerController.playerWeaponManager.currentWeapon != null && playerController.playerWeaponManager.currentWeapon.CanUse())
        {
            playerController.playerWeaponManager.currentWeapon.Grab();
            if(grabSFX != null)
                itemPickupAudioEmitter.ForcePlay(grabSFX, grabSFXVolume);
        }
    }

    public static void CloseCurrentOpenContainer()
    {
        currentOpenContainer.CloseContainer();
        currentOpenContainer = null;
    }

    private void UpdatePickupItemUI()
    {
        if (groundItems.Count > 0)
            onGroundItemsUpdated?.Invoke(groundItems[0].item);
        else
            onLastGroundItemRemoved?.Invoke();
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if(other.TryGetComponent(out WorldItem worldItem))
    //    {
    //        if (worldItem.isInContainer) return;

    //        groundItems.Add(worldItem);
    //        worldItem.SetHighlighted(true);
    //        onGroundItemsUpdated?.Invoke(groundItems[0].item);
    //        return;
    //    }

    //    if(other.TryGetComponent(out IContainer nearbyContainer))
    //    {
    //        if(transform.root.localRotation.eulerAngles.y == other.transform.localRotation.eulerAngles.y)
    //        {
    //            this.nearbyContainer = nearbyContainer;
    //            nearbyContainer.SetHighlighted(true);
    //            onNearbyContainerUpdated?.Invoke(nearbyContainer);
    //        }
    //    }

    //    if(other.TryGetComponent(out IInteractable nearbyInteractable))
    //    {
    //        if (nearbyInteractable.GetInteractableType() == InteractableType.Pressure_Plate) return;

    //        if (transform.root.localRotation.eulerAngles.y == other.transform.localRotation.eulerAngles.y)
    //        {
    //            this.nearbyInteractable = nearbyInteractable;
    //            if(nearbyInteractable.GetInteractableType() == InteractableType.Lever || nearbyInteractable.GetInteractableType() == InteractableType.Keycard_Reader)
    //                nearbyInteractable.SetHighlighted(true);
    //            onNearbyInteractableUpdated?.Invoke(nearbyInteractable);
    //        }
    //    }
    //}

    //private void OnTriggerStay(Collider other)
    //{
    //    if (other.TryGetComponent(out IContainer container))
    //    {
    //        if (transform.root.localRotation.eulerAngles.y == other.transform.localRotation.eulerAngles.y)
    //        {
    //            nearbyContainer = container;
    //            if(!nearbyContainer.IsOpen())
    //                nearbyContainer.SetHighlighted(true);
    //        }
    //        else
    //        {
    //            if(nearbyContainer != null)
    //                nearbyContainer.SetHighlighted(false);

    //            nearbyContainer = null;
    //        }

    //        onNearbyContainerUpdated?.Invoke(nearbyContainer);
    //    }

    //    if (other.TryGetComponent(out IInteractable nearbyInteractable))
    //    {
    //        if (nearbyInteractable.GetInteractableType() == InteractableType.Pressure_Plate) return;

    //        if (transform.root.localRotation.eulerAngles.y == other.transform.localRotation.eulerAngles.y)
    //        {
    //            this.nearbyInteractable = nearbyInteractable;
    //            if (nearbyInteractable.GetInteractableType() == InteractableType.Lever || nearbyInteractable.GetInteractableType() == InteractableType.Keycard_Reader)
    //                nearbyInteractable.SetHighlighted(true);
    //        }
    //        else
    //        {
    //            if (nearbyInteractable != null && (nearbyInteractable.GetInteractableType() == InteractableType.Lever || nearbyInteractable.GetInteractableType() == InteractableType.Keycard_Reader))
    //                nearbyInteractable.SetHighlighted(false);

    //            this.nearbyInteractable = null;
    //        }

    //        onNearbyInteractableUpdated?.Invoke(this.nearbyInteractable);
    //    }
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    if (groundItems.Count > 0)
    //    {
    //        if (other.TryGetComponent(out WorldItem worldItem))
    //        {
    //            if(groundItems.Contains(worldItem))
    //            {
    //                worldItem.SetHighlighted(false);
    //                groundItems.Remove(worldItem);
    //            }
    //        }

    //        if (groundItems.Count == 0)
    //            onLastGroundItemRemoved?.Invoke();
    //    }

    //    if(nearbyContainer != null)
    //    {
    //        if (other.TryGetComponent(out IContainer container))
    //            if (container == nearbyContainer)
    //            {
    //                nearbyContainer.CloseContainer();
    //                nearbyContainer.SetHighlighted(false);
    //                nearbyContainer = null;
    //                onNearbyContainerUpdated?.Invoke(nearbyContainer);
    //            }
    //    }

    //    if(nearbyInteractable != null)
    //    {
    //        if(other.TryGetComponent(out IInteractable interactable))
    //        {
    //            Debug.Log(nearbyInteractable.GetInteractableType());
    //            if (nearbyInteractable.GetInteractableType() == InteractableType.Pressure_Plate) return;

    //            if (interactable == nearbyInteractable)
    //            {
    //                if (nearbyInteractable.GetInteractableType() == InteractableType.Lever || nearbyInteractable.GetInteractableType() == InteractableType.Keycard_Reader)
    //                    nearbyInteractable.SetHighlighted(false);

    //                nearbyInteractable = null;
    //                onNearbyInteractableUpdated?.Invoke(nearbyInteractable);
    //            }
    //        }
    //    }
    //}
}
