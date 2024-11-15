using System;
using UnityEngine;

public enum Hands
{
    right,
    left,
    both
}

public class UseEquipment : MonoBehaviour
{
    ItemPickupManager itemPickup;
    PlayerEquipmentManager playerEquipmentManager;

    [SerializeField] bool canUseRightHand = true, canUseLeftHand = true, isInventoryOpen = false;
    float leftHandItemCooldown, rightHandItemCooldown;

    public static event Action<Hands, HandItemData> onHandUsed;
    public static Action onReloadKeyPressed;

    private void Awake()
    {
        playerEquipmentManager = GetComponent<PlayerEquipmentManager>();


        itemPickup = GetComponent<ItemPickupManager>();
    }

    private void OnEnable()
    {
        PlayerInventoryManager.onInventoryOpened += OnInventoryOpened;
        PlayerInventoryManager.onInventoryClosed += OnInventoryClosed;
    }

    private void OnDisable()
    {
        PlayerInventoryManager.onInventoryOpened -= OnInventoryOpened;
        PlayerInventoryManager.onInventoryClosed -= OnInventoryClosed;
    }

    void OnInventoryOpened()
    {
        isInventoryOpen = true;
    }

    void OnInventoryClosed()
    {
        isInventoryOpen = false;
    }

    HandItemData GetHandItemData(Hands handToGetFrom)
    {
        HandItemData dataToGet = null;

        if(handToGetFrom == Hands.left)
        {
            if(playerEquipmentManager.leftHandEquippedItem != null)
                dataToGet = playerEquipmentManager.leftHandEquippedItem.equipmentItemData as HandItemData;
        }
        else if (handToGetFrom == Hands.right)
            if(playerEquipmentManager.rightHandEquippedItem != null)
                dataToGet = playerEquipmentManager.rightHandEquippedItem.equipmentItemData as HandItemData;

        return dataToGet;
    }

    public void UseHand(Hands handToUse)
    {
        if (handToUse == Hands.left)
        {
            if (!canUseLeftHand)
                return;
        }
        else if(handToUse == Hands.right)
            if (!canUseRightHand)
                return;

        HandItemData handItemData = GetHandItemData(handToUse);
        if (!handItemData)
            return;

        onHandUsed?.Invoke(handToUse, handItemData);
    }
}
