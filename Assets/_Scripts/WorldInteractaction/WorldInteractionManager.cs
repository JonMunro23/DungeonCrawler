using UnityEngine;
using System;
using System.Collections.Generic;

public class WorldInteractionManager : MonoBehaviour
{
    PlayerController playerController;
    [Header("References")]
    [SerializeField] WorldItem worldItemPrefab;
    [SerializeField] Transform itemDropLocation;
    PlayerControls controls;

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
    //IInteractable nearbyInteractable;

    Coroutine grabCoroutine;

    static IHighlightable highlightedTarget;
    public static IContainer currentOpenContainer;
    //bool isLookingAtPickup, isLookingAtInteractable, isLookingAtContainer;

    public static event Action<ItemStack> onNewItemAttachedToCursor;
    public static event Action onCurrentItemDettachedFromCursor;

    public static event Action<ItemStack> onGroundItemsUpdated;
    public static event Action onLastGroundItemRemoved;

    public static event Action<LookAtTarget> onLookAtTargetChanged;

    private void OnEnable()
    {
        WorldItem.onWorldItemGrabbed += OnWorldItemGrabbed;
        InventorySlot.onInventorySlotLeftClicked += OnInventorySlotClicked;

        InventoryContextMenu.onInventorySlotItemDropped += DropItemFromInventoryIntoWorld;
    }

    private void OnDisable()
    {
        WorldItem.onWorldItemGrabbed -= OnWorldItemGrabbed;
        InventorySlot.onInventorySlotLeftClicked -= OnInventorySlotClicked;

        InventoryContextMenu.onInventorySlotItemDropped -= DropItemFromInventoryIntoWorld;
    }

    public static bool IsLookingAtInteractable() => highlightedTarget != null;

    private void Start()
    {
        itemPickupAudioEmitter = AudioManager.Instance.RegisterSource("[AudioEmitter] CharacterBody", transform.root, spatialBlend: 0);
    }

    public void Init(PlayerController playerController)
    {
        this.playerController = playerController;
        controls = playerController.GetPlayerControls();
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

        AttachItemToMouseCursor(worldItemGrabbed.itemStack, worldItemGrabbed);
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
                if(slotClicked.GetItemStack().Item == currentGrabbedItem.Item)
                {
                    int remainder = slotClicked.AddToCurrentItemStack(currentGrabbedItem.ItemAmount);
                    if (remainder > 0)
                    {
                        DetachItemFromMouseCursor();
                        AttachItemToMouseCursor(new ItemStack(slotClicked.GetItemStack().Item, remainder));
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
        //Debug.Log("Item attached to cursor");
        currentGrabbedItem = new ItemStack(itemToAttach.Item, itemToAttach.ItemAmount);

        onNewItemAttachedToCursor?.Invoke(currentGrabbedItem);
        
        if(worldItem)
            Destroy(worldItem.gameObject);

        hasGrabbedItem = true;
    }

    public void DetachItemFromMouseCursor()
    {
        onCurrentItemDettachedFromCursor?.Invoke();
        currentGrabbedItem = null;
        hasGrabbedItem = false;
    }

    void PlaceGrabbedItemInWorld(Vector3 placementLocation)
    {
        if (!hasGrabbedItem)
            return;

        SpawnWorldItem(currentGrabbedItem, placementLocation);
    }

    void DropItemFromInventoryIntoWorld(ISlot slot)
    {
        SpawnWorldItem(slot.TakeItem(), itemDropLocation.position);
    }

    void SpawnWorldItem(ItemStack itemStackToSpawn, Vector3 placementLocation)
    {
        WorldItem worldItem = Instantiate(worldItemPrefab, placementLocation, Quaternion.Euler(new Vector3(0, playerController.transform.localEulerAngles.y, 0)));
        worldItem.InitWorldItem(GridController.Instance.GetCurrentLevelIndex(), itemStackToSpawn);
        worldItem.transform.GetChild(0).localPosition = new Vector3(worldItem.transform.GetChild(0).localPosition.x, worldItem.transform.GetChild(0).localPosition.y, 0);
        worldItem.GetComponent<BoxCollider>().center = Vector3.zero;
        DetachItemFromMouseCursor();

        HelperFunctions.SetCursorActive(false);
    }

    // Acts as update, called from PlayerController
    public void Tick()
    {
        Ray ray = playerController.playerCamera.ScreenPointToRay(controls.Player.MousePos.ReadValue<Vector2>());
        
        HighlightObjects(ray);

        if (controls.Player.LeftClick.WasPressedThisFrame())
        {
            InteractWithObjects(ray);
        }
    }

    void InteractWithObjects(Ray ray)
    {
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, maxItemGrabDistance))
        {
            if (hasGrabbedItem && hit.transform.CompareTag("Ground"))
            {
                PlaceGrabbedItemInWorld(hit.point);
            }
            else if (hit.transform.TryGetComponent(out IPickup pickup))
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
            else if (hit.transform.TryGetComponent(out IInteractable interactable))
            {
                if (interactable.RequiresItem())
                {
                    if (!hasGrabbedItem) return;

                    if (interactable.TryInteractWithItem(currentGrabbedItem.Item.ItemData))
                    {
                        if (interactable.ConsumesItem())
                            DetachItemFromMouseCursor();

                        PlayGrabAnim();
                    }
                }
                else
                {
                    interactable.Interact();
                    PlayGrabAnim();
                }

            }
        }
    }

    void HighlightObjects(Ray ray)
    {
        RaycastHit hit;
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

    private void PlayGrabAnim()
    {
        if (playerController.playerWeaponManager.currentWeapon != null && playerController.playerWeaponManager.currentWeapon.CanUse())
        {
            if (grabCoroutine != null)
                StopCoroutine(grabCoroutine);

            grabCoroutine = StartCoroutine(playerController.playerWeaponManager.currentWeapon.Grab());

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
            onGroundItemsUpdated?.Invoke(groundItems[0].itemStack);
        else
            onLastGroundItemRemoved?.Invoke();
    }
}
