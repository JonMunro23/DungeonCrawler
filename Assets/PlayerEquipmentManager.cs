using System;
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
    [SerializeField] float currentCarryWeight, maxCarryWeight;
    [SerializeField] List<EquippedItem> allCurrentlyEquippedItems = new List<EquippedItem>();
    public EquippedItem leftHandEquippedItem, rightHandEquippedItem;
    [SerializeField] Transform weaponSpawnParent;
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
            if(!currentRightHandWeapon.CheckIsReloading())
                currentRightHandWeapon.TryReloadWeapon();
        }
        else if (currentLeftHandWeapon != null)
        {
            if (!currentLeftHandWeapon.CheckIsReloading())
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
            if(currentRightHandWeapon != null)
                currentRightHandWeapon.Use();
        }
    }

    void OnNewHandItem(EquipmentSlotType handSlot, HandItemData newItemData)
    {
        if(newItemData.isTwoHanded)
        {
            if(handSlot == EquipmentSlotType.leftHand)
            {
                leftHandEquippedItem = new EquippedItem(handSlot, newItemData);
                rightHandEquippedItem = new EquippedItem(EquipmentSlotType.rightHand, newItemData);
                InitialiseHandItem(handSlot, newItemData);
            }
            else if(handSlot == EquipmentSlotType.rightHand)
            {
                rightHandEquippedItem = new EquippedItem(handSlot, newItemData);
                leftHandEquippedItem = new EquippedItem(EquipmentSlotType.leftHand, newItemData);
                InitialiseHandItem(handSlot, newItemData);
            }
        }
        else if (handSlot == EquipmentSlotType.leftHand)
        {
            leftHandEquippedItem = new EquippedItem(handSlot, newItemData);
            InitialiseHandItem(handSlot, newItemData);
        }
        else
        {
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
            }
            else if (handSlot == EquipmentSlotType.rightHand)
            {
                rightHandEquippedItem = null;
                leftHandEquippedItem = null;
                currentRightHandWeapon.RemoveWeapon();
            }
        }
        else if (handSlot == EquipmentSlotType.leftHand)
        {
            leftHandEquippedItem = null;
            currentLeftHandWeapon.RemoveWeapon();
        }
        else
        {
            rightHandEquippedItem = null;
            currentRightHandWeapon.RemoveWeapon();
        }
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
                    currentRightHandWeapon = weapon;
                    currentRightHandWeapon.InitWeapon(handItemData, Hands.both);
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
}
