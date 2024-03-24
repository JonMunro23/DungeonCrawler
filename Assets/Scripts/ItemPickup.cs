using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ItemPickup : MonoBehaviour
{
    UseEquipment useEquipment;
    public GameObject mouseItem;
    public Transform canvasTransform;
    [SerializeField]
    Transform thrownItemSpawnLocation;
    [SerializeField]
    float throwVeloctiy;

    public Vector3 mousePos;
    public bool hasMouseItem, canPickUpItem = true;

    public ItemObject objectOnMouse;
    public int itemAmount;

    public InventorySlot inventorySlot;

    public GameObject mouseItemClone;

    private void Awake()
    {
        useEquipment = GetComponent<UseEquipment>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(hasMouseItem == true)
        {
            mousePos = Input.mousePosition;
            if(mouseItemClone)
                mouseItemClone.transform.position = mousePos;
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    if(hit.transform.CompareTag("Ground") && hit.distance < 10)
                    {
                        MouseItemToWorldItem(hit.point);
                        canPickUpItem = false;
                        Invoke("CanPickUpItem", .1f);
                    }else if(hit.distance > 10 && inventorySlot == null)
                    {
                        MouseItemToThrownWorldItem();
                        canPickUpItem = false;
                        Invoke("CanPickUpItem", .1f);
                    }
                }

                if (inventorySlot != null)
                {
                    if (inventorySlot.isSlotOccupied == false)
                    {
                        if (inventorySlot.isCharEquipSlot == false)
                        {
                            MouseItemToInventorySlot();
                        }
                        else if (inventorySlot.isCharEquipSlot == true)
                        {
                            MouseItemToCharacter();
                        }
                    }
                    else if(inventorySlot.isSlotOccupied == true)
                    {
                        if (inventorySlot.isCharEquipSlot == false)
                        {
                            SwapMouseItemWithInventoryItem();
                        }
                        //else if (inventorySlot.isCharEquipSlot == true)
                        //{
                        //    MouseItemToCharacter();
                        //}
                    }
                }
            }
        }else if(hasMouseItem == false)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (inventorySlot != null)
                {
                    if (inventorySlot.isSlotOccupied == true)
                    {
                        if(inventorySlot.isCharEquipSlot == true)
                        {
                            CharacterSlotToMouseitem();
                        }
                        else if (inventorySlot.isCharEquipSlot == false)
                        {
                            InventorySlotToMouseItem();
                        }
                    }
                }
            }
        }
    }

    public void WorldItemToMouse(GameObject itemToPickup)
    {
        objectOnMouse = itemToPickup.GetComponent<WorldItem>().itemObject;
        hasMouseItem = true;
        mouseItemClone = Instantiate(mouseItem, mousePos, Quaternion.identity, canvasTransform);
        mouseItemClone.GetComponent<RawImage>().texture = objectOnMouse.itemTexture;
        itemAmount = itemToPickup.GetComponent<WorldItem>().amount;

        if(useEquipment.currentWeapon != null)
            useEquipment.currentWeapon.GetComponent<Animator>().Play("Interact");

        if(itemToPickup.GetComponent<WorldItem>().itemObject.isItemStackable == true)
        {
            mouseItemClone.GetComponentInChildren<TMP_Text>().text = itemToPickup.GetComponent<WorldItem>().amount.ToString();
        }
        if(itemToPickup.GetComponent<WorldItem>().isOnPressurePlate == true)
        {
            itemToPickup.transform.position = new Vector3(0,0,0);
            Destroy(itemToPickup, 0.1f);
        }else
        {
            Destroy(itemToPickup, 0.1f);
        }
    }

    public void TorchSconceToMouse(ItemObject itemToPickup)
    {
        objectOnMouse = itemToPickup;
        hasMouseItem = true;
        mouseItemClone = Instantiate(mouseItem, mousePos, Quaternion.identity, canvasTransform);
        mouseItemClone.GetComponent<RawImage>().texture = objectOnMouse.itemTexture;
    }

    public void MouseItemToInventorySlot()
    {
        if (inventorySlot != null)
        {
            //Debug.Log(inventorySlot.name);
            if (inventorySlot.isSlotOccupied == false)
            {
                inventorySlot.itemObject = objectOnMouse;
                if (itemAmount > 1)
                {
                    inventorySlot.itemAmount = itemAmount;
                }
                inventorySlot.InitialiseItem();
                inventorySlot.isSlotOccupied = true;
                itemAmount = 0;
                objectOnMouse = null;
                hasMouseItem = false;
                Destroy(mouseItemClone);
            }
        }
    }

    public void InventorySlotToMouseItem()
    {
        objectOnMouse = inventorySlot.itemObject;
        hasMouseItem = true;
        itemAmount = inventorySlot.itemAmount;
        mouseItemClone = Instantiate(mouseItem, mousePos, Quaternion.identity, canvasTransform);
        mouseItemClone.GetComponent<RawImage>().texture = objectOnMouse.itemTexture;
        if(objectOnMouse.isItemStackable == true)
        {
            mouseItemClone.GetComponentInChildren<TMP_Text>().text = inventorySlot.itemAmount.ToString();
        }
        inventorySlot.MoveItem();
    }

    public void SwapMouseItemWithInventoryItem()
    {
        ItemObject inventoryItemToSwapWith = inventorySlot.itemObject;
        int inventoryItemToSwapWithAmount = inventorySlot.itemAmount;
        inventorySlot.itemObject = objectOnMouse;
        inventorySlot.itemAmount = itemAmount;
        inventorySlot.InitialiseItem();
        itemAmount = inventoryItemToSwapWithAmount;
        objectOnMouse = inventoryItemToSwapWith;
        mouseItemClone.GetComponent<RawImage>().texture = objectOnMouse.itemTexture;
        mouseItemClone.GetComponentInChildren<TMP_Text>().text = "";
        if (objectOnMouse.isItemStackable == true)
        {
            mouseItemClone.GetComponentInChildren<TMP_Text>().text = itemAmount.ToString();
        }
    }

    public void SwapMouseItemWithCharacterItem()
    {
        ItemObject characterItemToSwapWith = inventorySlot.itemObject;
        int inventoryItemToSwapWithAmount = inventorySlot.itemAmount;
        inventorySlot.itemObject = objectOnMouse;
        inventorySlot.itemAmount = itemAmount;
        inventorySlot.InitialiseItem();
        itemAmount = inventoryItemToSwapWithAmount;
        objectOnMouse = characterItemToSwapWith;
        mouseItemClone.GetComponent<RawImage>().texture = objectOnMouse.itemTexture;
        mouseItemClone.GetComponentInChildren<TMP_Text>().text = "";
        if (objectOnMouse.isItemStackable == true)
        {
            mouseItemClone.GetComponentInChildren<TMP_Text>().text = itemAmount.ToString();
        }
    }

    public void MouseItemToWorldItem(Vector3 spawnLocation)
    {
        GameObject spawnedWorldItem = Instantiate(objectOnMouse.itemWorldModel, spawnLocation, Quaternion.identity);
        spawnedWorldItem.GetComponent<WorldItem>().itemObject = objectOnMouse;
        spawnedWorldItem.GetComponent<WorldItem>().amount = itemAmount;
        hasMouseItem = false;
        objectOnMouse = null;
        Destroy(mouseItemClone);

    }

    public void MouseItemToThrownWorldItem()
    {
        GameObject spawnedWorldItem = Instantiate(objectOnMouse.itemWorldModel, thrownItemSpawnLocation.position, Quaternion.identity);
        spawnedWorldItem.GetComponent<Rigidbody>().AddForce(thrownItemSpawnLocation.up * throwVeloctiy*Time.deltaTime, ForceMode.Impulse);
        spawnedWorldItem.GetComponent<WorldItem>().itemObject = objectOnMouse;
        spawnedWorldItem.GetComponent<WorldItem>().amount = itemAmount;
        hasMouseItem = false;
        objectOnMouse = null;
        Destroy(mouseItemClone);

    }

    public void CharacterSlotToMouseitem()
    {
        objectOnMouse = inventorySlot.itemObject;
        hasMouseItem = true;
        inventorySlot.MoveItem();
        mouseItemClone = Instantiate(mouseItem, mousePos, Quaternion.identity, canvasTransform);
        mouseItemClone.GetComponent<RawImage>().texture = objectOnMouse.itemTexture;
    }

    public void MouseItemToCharacter()
    {
        if (inventorySlot != null)
        {
            if (objectOnMouse.slotType == ItemObject.SlotType.hands)
            {
                if(inventorySlot.charEquipSlotName == "main hand" || inventorySlot.charEquipSlotName == "off hand")
                {
                    if (inventorySlot.isSlotOccupied == false)
                    {
                        inventorySlot.itemObject = objectOnMouse;
                        inventorySlot.InitialiseItem();
                        inventorySlot.isSlotOccupied = true;
                        objectOnMouse = null;
                        hasMouseItem = false;
                        Destroy(mouseItemClone);

                    }
                }
            }
            else
            {
                if (inventorySlot.charEquipSlotName == objectOnMouse.slotType.ToString())
                {
                    if (inventorySlot.isSlotOccupied == false)
                    {
                        inventorySlot.itemObject = objectOnMouse;
                        inventorySlot.InitialiseItem();
                        inventorySlot.isSlotOccupied = true;
                        objectOnMouse = null;
                        hasMouseItem = false;
                        Destroy(mouseItemClone);

                    }
                }
            }
        }
    }

    void CanPickUpItem()
    {
        canPickUpItem = true;
    }

}
