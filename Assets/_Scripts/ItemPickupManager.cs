using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ItemPickupManager : MonoBehaviour
{
    UseEquipment useEquipment;
    public GameObject mouseItem;
    public Transform canvasTransform;
    [SerializeField]
    Transform thrownItemSpawnLocation;
    [SerializeField]
    float throwVeloctiy;

    public Vector3 mousePos = Vector3.zero;
    public Item currentGrabbedItem = null;

    //GrabbedItemUI grabbedItemUI;

    public bool hasGrabbedItem, canPickUpItem = true;

    public ItemData objectOnMouse;
    public int itemAmount;

    public InventorySlot inventorySlot;

    public GameObject mouseItemClone;
    float maxGrabDistance = 3;

    public static Action<Item> onNewItemAttachedToCursor;
    public static Action onCurrentItemDettachedFromCursor;

    private void OnEnable()
    {
        WorldItem.onWorldItemGrabbed += OnWorldItemGrabbed;
        InventorySlot.onInventorySlotClicked += OnInventorySlotClicked;
    }

    private void OnDisable()
    {
        WorldItem.onWorldItemGrabbed -= OnWorldItemGrabbed;
        InventorySlot.onInventorySlotClicked -= OnInventorySlotClicked;
    }
    void OnWorldItemGrabbed(WorldItem worldItemGrabbed)
    {
        if (hasGrabbedItem)
            return;

        AttachItemToMouseCursor(worldItemGrabbed.item, worldItemGrabbed);
    }

    void OnInventorySlotClicked(InventorySlot slotClicked)
    {
        if (!slotClicked.isSlotActive)
            return;

        if (!hasGrabbedItem)
        {
            if (slotClicked.isSlotOccupied)
            {
                AttachItemToMouseCursor(slotClicked.TakeItem());
                return;
            }
        }
        else
        {
            if (!slotClicked.isSlotOccupied)
            {
                slotClicked.AddItem(currentGrabbedItem);
                DetachItemFromMouseCursor();
                return;
            }
            else
            {
                if(slotClicked.currentSlotItem.itemData == currentGrabbedItem.itemData)
                {
                    slotClicked.AddToCurrentItemStack(currentGrabbedItem.itemAmount);
                    DetachItemFromMouseCursor();
                    return;
                }
                else
                    AttachItemToMouseCursor(slotClicked.SwapItem(currentGrabbedItem));

            }
        }

    }

    void AttachItemToMouseCursor(Item itemToAttach, WorldItem worldItem = null)
    {
        currentGrabbedItem = new Item(itemToAttach.itemData, itemToAttach.itemAmount);

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

        WorldItem spawnedWorldItem = Instantiate(currentGrabbedItem.itemData.itemWorldModel, placementLocation, Quaternion.identity);
        spawnedWorldItem.InitWorldItem(currentGrabbedItem);
        DetachItemFromMouseCursor();
    }

    void ThrowGrabbedItemIntoWorld()
    {
        WorldItem spawnedWorldItem = Instantiate(currentGrabbedItem.itemData.itemWorldModel, thrownItemSpawnLocation.position, Quaternion.identity);
        spawnedWorldItem.GetComponent<Rigidbody>().AddForce(thrownItemSpawnLocation.forward * throwVeloctiy * Time.deltaTime, ForceMode.Impulse);
        spawnedWorldItem.item.itemData = currentGrabbedItem.itemData;
        spawnedWorldItem.item.itemAmount = currentGrabbedItem.itemAmount;

        DetachItemFromMouseCursor();
    }

    // Update is called once per frame
    void Update()
    {
        if (hasGrabbedItem == true)
        {
            if(Input.GetKeyDown(KeyCode.Mouse0))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    Debug.Log(hit.distance);
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


        //    if (Input.GetKeyDown(KeyCode.Mouse0))
        //    {
        //        RaycastHit hit;
        //        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //        if (Physics.Raycast(ray, out hit))
        //        {
        //            if(hit.transform.CompareTag("Ground") && hit.distance < 10)
        //            {
        //                MouseItemToWorldItem(hit.point);
        //                canPickUpItem = false;
        //                Invoke("CanPickUpItem", .1f);
        //            }else if(hit.distance > 10 && inventorySlot == null)
        //            {
        //                MouseItemToThrownWorldItem();
        //                canPickUpItem = false;
        //                Invoke("CanPickUpItem", .1f);
        //            }
        //        }

        //        if (inventorySlot != null)
        //        {
        //            if (inventorySlot.isSlotOccupied == false)
        //            {
        //                if (inventorySlot.isCharEquipSlot == false)
        //                {
        //                    MouseItemToInventorySlot();
        //                }
        //                else if (inventorySlot.isCharEquipSlot == true)
        //                {
        //                    MouseItemToCharacter();
        //                }
        //            }
        //            else if(inventorySlot.isSlotOccupied == true)
        //            {
        //                if (inventorySlot.isCharEquipSlot == false)
        //                {
        //                    SwapMouseItemWithInventoryItem();
        //                }
        //                //else if (inventorySlot.isCharEquipSlot == true)
        //                //{
        //                //    MouseItemToCharacter();
        //                //}
        //            }
        //        }
        //    }
        //}else if(hasMouseItem == false)
        //{
        //    if (Input.GetKeyDown(KeyCode.Mouse0))
        //    {
        //        if (inventorySlot != null)
        //        {
        //            if (inventorySlot.isSlotOccupied == true)
        //            {
        //                if(inventorySlot.isCharEquipSlot == true)
        //                {
        //                    CharacterSlotToMouseitem();
        //                }
        //                else if (inventorySlot.isCharEquipSlot == false)
        //                {
        //                    InventorySlotToMouseItem();
        //                }
        //            }
        //        }
        //    }
        //}
    }

    public void WorldItemToMouse(GameObject itemToPickup)
    {
        //objectOnMouse = itemToPickup.GetComponent<WorldItem>().itemData;
        //hasGrabbedItem = true;
        //mouseItemClone = Instantiate(mouseItem, mousePos, Quaternion.identity, canvasTransform);
        //mouseItemClone.GetComponent<Image>().sprite = objectOnMouse.itemSprite;
        //itemAmount = itemToPickup.GetComponent<WorldItem>().amount;

        //if(useEquipment.currentWeapon != null)
        //    useEquipment.currentWeapon.GetComponent<Animator>().Play("Interact");

        //if(itemToPickup.GetComponent<WorldItem>().itemData.isItemStackable == true)
        //{
        //    mouseItemClone.GetComponentInChildren<TMP_Text>().text = itemToPickup.GetComponent<WorldItem>().amount.ToString();
        //}
        //if(itemToPickup.GetComponent<WorldItem>().isOnPressurePlate == true)
        //{
        //    itemToPickup.transform.position = new Vector3(0,0,0);
        //    Destroy(itemToPickup, 0.1f);
        //}else
        //{
        //    Destroy(itemToPickup, 0.1f);
        //}
    }

    public void TorchSconceToMouse(ItemData itemToPickup)
    {
        //objectOnMouse = itemToPickup;
        //hasGrabbedItem = true;
        //mouseItemClone = Instantiate(mouseItem, mousePos, Quaternion.identity, canvasTransform);
        //mouseItemClone.GetComponent<Image>().sprite = objectOnMouse.itemSprite;
    }

    public void MouseItemToInventorySlot()
    {
        //if (inventorySlot != null)
        //{
        //    //Debug.Log(inventorySlot.name);
        //    if (inventorySlot.isSlotOccupied == false)
        //    {
        //        inventorySlot.currentSlotItem = currentGrabbedItem;
        //        if (itemAmount > 1)
        //        {
        //            inventorySlot.currentSlotItem.itemAmount = itemAmount;
        //        }
        //        inventorySlot.InitialiseItem();
        //        inventorySlot.isSlotOccupied = true;
        //        itemAmount = 0;
        //        objectOnMouse = null;
        //        hasGrabbedItem = false;
        //        Destroy(mouseItemClone);
        //    }
        //}
    }

    public void InventorySlotToMouseItem()
    {
        //objectOnMouse = inventorySlot.currentSlotItem.itemData;
        //hasGrabbedItem = true;
        //itemAmount = inventorySlot.currentSlotItem.itemAmount;
        //mouseItemClone = Instantiate(mouseItem, mousePos, Quaternion.identity, canvasTransform);
        //mouseItemClone.GetComponent<Image>().sprite = objectOnMouse.itemSprite;
        //if(objectOnMouse.isItemStackable == true)
        //{
        //    mouseItemClone.GetComponentInChildren<TMP_Text>().text = inventorySlot.currentSlotItem.itemAmount.ToString();
        //}
        //inventorySlot.MoveItem();
    }

    public void SwapMouseItemWithInventoryItem()
    {
        //ItemData inventoryItemToSwapWith = inventorySlot.currentSlotItem;
        //int inventoryItemToSwapWithAmount = inventorySlot.currentSlotAmount;
        //inventorySlot.currentSlotItem = objectOnMouse;
        //inventorySlot.currentSlotAmount = itemAmount;
        //inventorySlot.InitialiseItem();
        //itemAmount = inventoryItemToSwapWithAmount;
        //objectOnMouse = inventoryItemToSwapWith;
        //mouseItemClone.GetComponent<Image>().sprite = objectOnMouse.itemSprite;
        //mouseItemClone.GetComponentInChildren<TMP_Text>().text = "";
        //if (objectOnMouse.isItemStackable == true)
        //{
        //    mouseItemClone.GetComponentInChildren<TMP_Text>().text = itemAmount.ToString();
        //}
    }

    public void SwapMouseItemWithCharacterItem()
    {
        //ItemData characterItemToSwapWith = inventorySlot.currentSlotItem;
        //int inventoryItemToSwapWithAmount = inventorySlot.currentSlotAmount;
        //inventorySlot.currentSlotItem = objectOnMouse;
        //inventorySlot.currentSlotAmount = itemAmount;
        //inventorySlot.InitialiseItem();
        //itemAmount = inventoryItemToSwapWithAmount;
        //objectOnMouse = characterItemToSwapWith;
        //mouseItemClone.GetComponent<Image>().sprite = objectOnMouse.itemSprite;
        //mouseItemClone.GetComponentInChildren<TMP_Text>().text = "";
        //if (objectOnMouse.isItemStackable == true)
        //{
        //    mouseItemClone.GetComponentInChildren<TMP_Text>().text = itemAmount.ToString();
        //}
    }

    public void MouseItemToWorldItem(Vector3 spawnLocation)
    {
        //WorldItem spawnedWorldItem = Instantiate(objectOnMouse.itemWorldModel, spawnLocation, Quaternion.identity);
        //spawnedWorldItem.itemData = objectOnMouse;
        //spawnedWorldItem.amount = itemAmount;
        //hasGrabbedItem = false;
        //objectOnMouse = null;
        //Destroy(mouseItemClone);

    }

    public void MouseItemToThrownWorldItem()
    {
        //WorldItem spawnedWorldItem = Instantiate(objectOnMouse.itemWorldModel, thrownItemSpawnLocation.position, Quaternion.identity);
        //spawnedWorldItem.GetComponent<Rigidbody>().AddForce(thrownItemSpawnLocation.up * throwVeloctiy*Time.deltaTime, ForceMode.Impulse);
        //spawnedWorldItem.itemData = objectOnMouse;
        //spawnedWorldItem.amount = itemAmount;
        //hasGrabbedItem = false;
        //objectOnMouse = null;
        //Destroy(mouseItemClone);

    }

    public void CharacterSlotToMouseitem()
    {
        //objectOnMouse = inventorySlot.currentSlotItem;
        //hasGrabbedItem = true;
        //inventorySlot.MoveItem();
        //mouseItemClone = Instantiate(mouseItem, mousePos, Quaternion.identity, canvasTransform);
        //mouseItemClone.GetComponent<Image>().sprite = objectOnMouse.itemSprite;
    }

    public void MouseItemToCharacter()
    {
        //if (inventorySlot != null)
        //{
        //    if (objectOnMouse.slotType == ItemData.SlotType.hands)
        //    {
        //        if(inventorySlot.charEquipSlotName == "main hand" || inventorySlot.charEquipSlotName == "off hand")
        //        {
        //            if (inventorySlot.isSlotOccupied == false)
        //            {
        //                inventorySlot.currentSlotItem = objectOnMouse;
        //                inventorySlot.InitialiseItem();
        //                inventorySlot.isSlotOccupied = true;
        //                objectOnMouse = null;
        //                hasGrabbedItem = false;
        //                Destroy(mouseItemClone);

        //            }
        //        }
        //    }
        //    else
        //    {
        //        if (inventorySlot.charEquipSlotName == objectOnMouse.slotType.ToString())
        //        {
        //            if (inventorySlot.isSlotOccupied == false)
        //            {
        //                inventorySlot.currentSlotItem = objectOnMouse;
        //                inventorySlot.InitialiseItem();
        //                inventorySlot.isSlotOccupied = true;
        //                objectOnMouse = null;
        //                hasGrabbedItem = false;
        //                Destroy(mouseItemClone);

        //            }
        //        }
        //    }
        //}
    }

    void CanPickUpItem()
    {
        canPickUpItem = true;
    }

}
