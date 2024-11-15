using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EquippedItem
{
    public EquipmentSlotType slotType;
    public EquipmentItemData equipmentItemData;

    public EquippedItem(EquipmentSlotType slotType, EquipmentItemData equipmentItemData)
    {
        this.slotType = slotType;
        this.equipmentItemData = equipmentItemData;
    }
}

public class PlayerEquipmentManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform weaponSpawnParent;
    [SerializeField] HandItemData startingHandItem;

    [Header("Equipped Items")]
    [SerializeField] List<EquippedItem> allCurrentlyEquippedItems = new List<EquippedItem>();
    public EquippedItem leftHandEquippedItem, rightHandEquippedItem;
    [SerializeField] bool isLeftHandHolstered, isRightHandHolstered;

    [Header("Carry Weight")]
    [SerializeField] float currentCarryWeight, maxCarryWeight;

    public IWeapon currentLeftHandWeapon, currentRightHandWeapon;

    public static Action<EquippedItem> onEquippedItemAdded;
    public static Action<EquippedItem> onEquippedItemRemoved;

    private void OnEnable()
    {
        EquipmentSlot.onNewEquipmentItem += OnNewEquipmentItem;
        EquipmentSlot.onEquipmentItemRemoved += OnEquipmentItemRemoved;

        InventorySlot.onNewHandItem += OnNewHandItem;
        InventorySlot.onHandItemRemoved += OnHandItemRemoved;

        UseEquipment.onHandUsed += OnHandUsed;
        UseEquipment.onReloadKeyPressed += OnReloadKeyPressed;

        WorldInteraction.OnWorldInteraction += OnWorldInteraction;
    }

    private void OnDisable()
    {
        EquipmentSlot.onNewEquipmentItem -= OnNewEquipmentItem;
        EquipmentSlot.onEquipmentItemRemoved -= OnEquipmentItemRemoved;

        InventorySlot.onNewHandItem -= OnNewHandItem;
        InventorySlot.onHandItemRemoved -= OnHandItemRemoved;

        UseEquipment.onHandUsed -= OnHandUsed;
        UseEquipment.onReloadKeyPressed -= OnReloadKeyPressed;

        WorldInteraction.OnWorldInteraction -= OnWorldInteraction;
    }

    private void Start()
    {
        OnNewHandItem(EquipmentSlotType.rightHand, startingHandItem);
    }

    void OnWorldInteraction()
    {
        if(currentRightHandWeapon != null)
        {
            currentRightHandWeapon.Grab();
        }
        else if(currentLeftHandWeapon != null)
        {
            currentLeftHandWeapon.Grab();
        }
    }

    private void OnReloadKeyPressed()
    {
        if(currentRightHandWeapon != null)
        {
            if(!currentRightHandWeapon.IsReloading())
                currentRightHandWeapon.TryReloadWeapon();
        }
        else if (currentLeftHandWeapon != null)
        {
            if (!currentLeftHandWeapon.IsReloading())
                currentLeftHandWeapon.TryReloadWeapon();
        }
    }

    void OnHandUsed(Hands handUsed, HandItemData handItemData)
    {
        if (handUsed == Hands.left)
        {
            if (currentLeftHandWeapon != null)
                currentLeftHandWeapon.Use();
        }
        else
        {
            if (currentRightHandWeapon != null)
                currentRightHandWeapon.Use();
        }
    }

    void OnNewHandItem(EquipmentSlotType handSlot, HandItemData newItemData)
    {
        if(newItemData.isTwoHanded)
        {
            if (currentLeftHandWeapon != null)
            {
                currentLeftHandWeapon.RemoveWeapon();
                currentLeftHandWeapon = null;
            }
            else if (currentRightHandWeapon != null)
            {
                currentRightHandWeapon.RemoveWeapon();
                currentRightHandWeapon = null;
            }

            if (handSlot == EquipmentSlotType.leftHand)
            {
                leftHandEquippedItem = new EquippedItem(handSlot, newItemData);
                //rightHandEquippedItem = new EquippedItem(EquipmentSlotType.rightHand, newItemData);
                InitialiseHandItem(handSlot, newItemData);
            }
            else if(handSlot == EquipmentSlotType.rightHand)
            {
                rightHandEquippedItem = new EquippedItem(handSlot, newItemData);
                //leftHandEquippedItem = new EquippedItem(EquipmentSlotType.leftHand, newItemData);
                InitialiseHandItem(handSlot, newItemData);
            }
        }
        else if (handSlot == EquipmentSlotType.leftHand)
        {
            if (currentLeftHandWeapon != null)
            {
                currentLeftHandWeapon.RemoveWeapon();
                currentLeftHandWeapon = null;
            }

            leftHandEquippedItem = new EquippedItem(handSlot, newItemData);
            InitialiseHandItem(handSlot, newItemData);
        }
        else
        {
            if (currentRightHandWeapon != null)
            {
                currentRightHandWeapon.RemoveWeapon();
                currentRightHandWeapon = null;
            }

            rightHandEquippedItem = new EquippedItem(handSlot, newItemData);
            InitialiseHandItem(handSlot, newItemData);
        }
    }

    void OnHandItemRemoved(EquipmentSlotType handSlot, HandItemData newItemData)
    {
        if (newItemData.isTwoHanded)
        {
            if (handSlot == EquipmentSlotType.leftHand)
            {
                leftHandEquippedItem = null;
                rightHandEquippedItem = null;
                currentLeftHandWeapon.RemoveWeapon();
                currentLeftHandWeapon = null;
            }
            //else if (handSlot == EquipmentSlotType.rightHand)
            //{
            //    rightHandEquippedItem = null;
            //    leftHandEquippedItem = null;
            //    currentRightHandWeapon.RemoveWeapon();
            //    currentRightHandWeapon = null;
            //}
        }
        else if (handSlot == EquipmentSlotType.leftHand)
        {
            leftHandEquippedItem = null;
            currentLeftHandWeapon.RemoveWeapon();
            currentLeftHandWeapon = null;
        }
        else
        {
            rightHandEquippedItem = null;
            currentRightHandWeapon.RemoveWeapon();
            currentRightHandWeapon = null;
        }

        OnNewHandItem(handSlot, startingHandItem);
    }

    void OnNewEquipmentItem(EquipmentSlotType slotType, EquipmentItemData newEquipmentItemData)
    {
        EquippedItem newEquippedItem = new EquippedItem(slotType, newEquipmentItemData);
        allCurrentlyEquippedItems.Add(newEquippedItem);
        CalculateNewCurrentWeight(newEquipmentItemData.itemWeight);
        onEquippedItemAdded?.Invoke(newEquippedItem);
    }

    void OnEquipmentItemRemoved(EquipmentSlotType slotType)
    {
        EquippedItem itemInSlot = GetEquippedItemInSlot(slotType);
        CalculateNewCurrentWeight(-itemInSlot.equipmentItemData.itemWeight);

        if (allCurrentlyEquippedItems.Contains(itemInSlot))
            allCurrentlyEquippedItems.Remove(itemInSlot);

        onEquippedItemRemoved?.Invoke(itemInSlot);
    }

    EquippedItem GetEquippedItemInSlot(EquipmentSlotType slot)
    {
        EquippedItem itemToReturn = null;
        foreach (EquippedItem item in allCurrentlyEquippedItems)
        {
            if(item.slotType == slot)
            {
                itemToReturn = item;
                break;
            }
        }
        return itemToReturn;
    }

    void CalculateNewCurrentWeight(float newAddedWeight)
    {
        currentCarryWeight += newAddedWeight;
        //check if overencucumbered
    }

    public void InitialiseHandItem(EquipmentSlotType slotType, HandItemData handItemData)
    {
        if(handItemData.isTwoHanded)
        {
            if (handItemData.itemPrefab)
            {
                GameObject spawnedWeapon = Instantiate(handItemData.itemPrefab, weaponSpawnParent);
                if (spawnedWeapon.TryGetComponent(out IWeapon weapon))
                {
                    currentLeftHandWeapon = weapon;
                    currentLeftHandWeapon.InitWeapon(handItemData, Hands.both);
                    //currentRightHandWeapon = weapon;
                    //currentRightHandWeapon.InitWeapon(handItemData, Hands.both);
                }
            }
        }
        else if (slotType == EquipmentSlotType.leftHand)
        {
            if (handItemData.itemPrefab)
            {
                GameObject spawnedWeapon = Instantiate(handItemData.itemPrefab, weaponSpawnParent);
                if(spawnedWeapon.TryGetComponent(out IWeapon weapon))
                {
                    currentLeftHandWeapon = weapon;
                    currentLeftHandWeapon.InitWeapon(handItemData, Hands.left);
                }
            }
        }
        else if (slotType == EquipmentSlotType.rightHand)
        {
            if (handItemData.itemPrefab)
            {
                GameObject spawnedWeapon = Instantiate(handItemData.itemPrefab, weaponSpawnParent);
                if (spawnedWeapon.TryGetComponent(out IWeapon weapon))
                {
                    currentRightHandWeapon = weapon;
                    currentRightHandWeapon.InitWeapon(handItemData, Hands.right);
                }
            }
        }
    }

    public void HolsterWeapons(Action onHolsteredCallback)
    {
        Action leftHandHolstered = null;
        Action rightHandHolstered = null;

        leftHandHolstered += OnLeftHandHolstered;
        rightHandHolstered += OnRightHandHolstered;

        if(currentLeftHandWeapon != null)
        {
            StartCoroutine(currentLeftHandWeapon.HolsterWeapon(leftHandHolstered));
            if (currentLeftHandWeapon.IsTwoHanded())
                return;
        }

        if(currentRightHandWeapon != null)
        {
            StartCoroutine(currentRightHandWeapon.HolsterWeapon(rightHandHolstered));
        }

        isLeftHandHolstered = false;
        isRightHandHolstered = false;

        void OnLeftHandHolstered()
        {
            isLeftHandHolstered = true;
            CheckIsHolstered();
        }

        void OnRightHandHolstered()
        {
            isRightHandHolstered = true;
            CheckIsHolstered();
        }

        void CheckIsHolstered()
        {

            bool leftHolstered = false;
            bool rightHolstered = false;
            if (currentLeftHandWeapon != null)
            {
                if (isLeftHandHolstered)
                {
                    leftHolstered = true;
                    if (currentLeftHandWeapon.IsTwoHanded())
                    {
                        onHolsteredCallback?.Invoke();
                        return;
                    }
                }
            }
            else
                leftHolstered = true;

            if(currentRightHandWeapon != null)
            {
                if(isRightHandHolstered)
                {
                    rightHolstered = true;
                }
            }
            else
                rightHolstered = true;

            if (rightHolstered && leftHolstered)
                onHolsteredCallback?.Invoke();
        }

    }

    public void DrawWeapons()
    {
        if (currentLeftHandWeapon != null)
        {
            isLeftHandHolstered = false;
            StartCoroutine(currentLeftHandWeapon.DrawWeapon());
            if (currentLeftHandWeapon.IsTwoHanded())
                return;
        }
        
        if (currentRightHandWeapon != null)
        {
            isRightHandHolstered = false;
            StartCoroutine(currentRightHandWeapon.DrawWeapon());
        }
    }
}
