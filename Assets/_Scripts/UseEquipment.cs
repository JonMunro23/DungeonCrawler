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
    WorldInteraction worldInteraction;
    ItemPickupManager itemPickup;
    PlayerEquipmentManager playerEquipmentManager;

    [SerializeField] bool canUseRightHand = true, canUseLeftHand = true, isInventoryOpen = false;
    float leftHandItemCooldown, rightHandItemCooldown;

    public static event Action<Hands, HandItemData> onHandUsed;
    public static Action onReloadKeyPressed;

    private void Awake()
    {
        playerEquipmentManager = GetComponent<PlayerEquipmentManager>();


        worldInteraction = GetComponent<WorldInteraction>();
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

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButton(0))
        {

            if(!isInventoryOpen && worldInteraction.isClickable == false && itemPickup.hasGrabbedItem == false && DialogueManager.isInDialogue == false)
                UseHand(Hands.left);
        }
        if (Input.GetMouseButton(1))
        {
            if(!isInventoryOpen && worldInteraction.isClickable == false && itemPickup.hasGrabbedItem == false && DialogueManager.isInDialogue == false)
                UseHand(Hands.right);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            TryReloadWeapon();
        }
    }

    private void TryReloadWeapon()
    {
        onReloadKeyPressed?.Invoke();
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

    public void UseRightHand(HandItemData handItemData)
    {
        if (!handItemData || !canUseRightHand)
            return;

        if (handItemData.isMeleeWeapon)
        {
            if (canUseRightHand == true)
            {
                //UseMeleeWeapon(handItemData, Hands.right);
                onHandUsed?.Invoke(Hands.right, handItemData);
            }
        }
        else
        {
            if (handItemData.isTwoHanded)
            {
                onHandUsed?.Invoke(Hands.both, handItemData);
            }
            else
            {
                onHandUsed?.Invoke(Hands.right, handItemData);
            }
        }
    }
}
