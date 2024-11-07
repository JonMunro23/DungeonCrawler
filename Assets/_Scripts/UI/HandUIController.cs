using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HandUIController : MonoBehaviour
{
    [SerializeField] Image leftHandHeldItemImage;
    [SerializeField] Image rightHandHeldItemImage;
    [SerializeField] Image leftHandItemCooldownImage;
    [SerializeField] Image rightHandItemCooldownImage;

    [SerializeField] HandItemData defaultHandItem;

    private void OnEnable()
    {
        InventorySlot.onNewHandItem += OnNewHandItem;
        InventorySlot.onHandItemRemoved += OnHandItemRemoved;

        RangedWeapon.OnHandCooldownBegins += OnHandCooldownBegins;
        RangedWeapon.OnHandCooldownEnds += OnHandCooldownEnds;
    }

    private void OnDisable()
    {
        InventorySlot.onNewHandItem -= OnNewHandItem;
        InventorySlot.onHandItemRemoved -= OnHandItemRemoved;

        RangedWeapon.OnHandCooldownBegins -= OnHandCooldownBegins;
        RangedWeapon.OnHandCooldownEnds -= OnHandCooldownEnds;
    }

    public void InitHands()
    {
        OnNewHandItem(EquipmentSlotType.leftHand, defaultHandItem);
        OnNewHandItem(EquipmentSlotType.rightHand, defaultHandItem);
    }

    public void OnHandItemRemoved(EquipmentSlotType slotType, HandItemData removedItemData)
    {
        if (removedItemData.isTwoHanded)
        {
            leftHandHeldItemImage.sprite = defaultHandItem.itemSprite;
            rightHandHeldItemImage.sprite = defaultHandItem.itemSprite;

        }
        else if (slotType == EquipmentSlotType.leftHand)
        {
            leftHandHeldItemImage.sprite = defaultHandItem.itemSprite;
            OnHandCooldownEnds(Hands.left);
        }
        else
        {
            rightHandHeldItemImage.sprite = defaultHandItem.itemSprite;
            OnHandCooldownEnds(Hands.right);
        }

    }

    public void OnNewHandItem(EquipmentSlotType slotType, HandItemData newItem)
    {
        if(newItem.isTwoHanded)
        {
            leftHandHeldItemImage.sprite = newItem.itemSprite;
            rightHandHeldItemImage.sprite = newItem.itemSprite;

        }
        else if (slotType == EquipmentSlotType.leftHand)
            leftHandHeldItemImage.sprite = newItem.itemSprite;
        else
            rightHandHeldItemImage.sprite = newItem.itemSprite;
    }

    void OnHandCooldownBegins(Hands hand)
    {
        if(hand == Hands.both)
        {
            leftHandItemCooldownImage.enabled = true;
            rightHandItemCooldownImage.enabled = true;
        }
        else if (hand == Hands.left)
        {
            leftHandItemCooldownImage.enabled = true;
        }
        else
        {
            rightHandItemCooldownImage.enabled = true;
        }
    }

    void OnHandCooldownEnds(Hands hand)
    {
        if(hand == Hands.both)
        {
            leftHandItemCooldownImage.enabled = false;
            rightHandItemCooldownImage.enabled = false;
        }
        else if (hand == Hands.left)
        {
            leftHandItemCooldownImage.enabled = false;
        }
        else
        {
            rightHandItemCooldownImage.enabled = false;
        }
    }
}
