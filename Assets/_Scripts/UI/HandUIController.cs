using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HandUIController : MonoBehaviour
{
    [SerializeField] Image weaponSlot0Image;
    [SerializeField] Image weaponSlot1Image;
    [SerializeField] Image leftHandItemCooldownImage;
    [SerializeField] Image rightHandItemCooldownImage;

    [SerializeField] WeaponItemData defaultHandItem;

    //private void OnEnable()
    //{
    //    InventorySlot.onNewWeapon += OnNewWeaponItem;
    //    InventorySlot.onWeaponRemoved += OnHandItemRemoved;

    //    Weapon.OnWeaponCooldownBegins += OnWeaponCooldownBegins;
    //    Weapon.OnWeaponCooldownEnds += OnWeaponCooldownEnds;
    //}

    //private void OnDisable()
    //{
    //    InventorySlot.onNewWeapon -= OnNewWeaponItem;
    //    InventorySlot.onWeaponRemoved -= OnHandItemRemoved;

    //    Weapon.OnWeaponCooldownBegins -= OnWeaponCooldownBegins;
    //    Weapon.OnWeaponCooldownEnds -= OnWeaponCooldownEnds;
    //}

    public void InitHands()
    {
        //UpdateHands(EquipmentSlotType.leftHand, defaultHandItem);
        //UpdateHands(EquipmentSlotType.rightHand, defaultHandItem);
    }

    public void OnHandItemRemoved(EquipmentSlotType slotType, WeaponItemData removedItemData)
    {
        Debug.Log("OnHandItemRemoved");

        //if (removedItemData.isTwoHanded)
        //{
        //    weaponSlot0Image.sprite = defaultHandItem.itemSprite;
        //    weaponSlot1Image.sprite = defaultHandItem.itemSprite;

        //}
        //else if (slotType == EquipmentSlotType.leftHand)
        //{
        //    weaponSlot0Image.sprite = defaultHandItem.itemSprite;
        //    OnWeaponCooldownEnds();
        //}
        //else
        //{
        //    weaponSlot1Image.sprite = defaultHandItem.itemSprite;
        //    OnWeaponCooldownEnds();
        //}

    }

    public void UpdateWeaponSlot(EquipmentSlotType slotToUpdate, WeaponItemData newSlotData)
    {
        if(slotToUpdate == EquipmentSlotType.weaponSlot0)
            weaponSlot0Image.sprite = newSlotData.itemSprite;
        else if (slotToUpdate == EquipmentSlotType.weaponSlot0)
            weaponSlot1Image.sprite = newSlotData.itemSprite;
    }

    void OnWeaponCooldownBegins()
    {
        //if(hand == Hands.both)
        //{
        //    leftHandItemCooldownImage.enabled = true;
        //    rightHandItemCooldownImage.enabled = true;
        //}
        //else if (hand == Hands.left)
        //{
        //    leftHandItemCooldownImage.enabled = true;
        //}
        //else
        //{
        //    rightHandItemCooldownImage.enabled = true;
        //}
    }

    void OnWeaponCooldownEnds()
    {
        //if(hand == Hands.both)
        //{
        //    leftHandItemCooldownImage.enabled = false;
        //    rightHandItemCooldownImage.enabled = false;
        //}
        //else if (hand == Hands.left)
        //{
        //    leftHandItemCooldownImage.enabled = false;
        //}
        //else
        //{
        //    rightHandItemCooldownImage.enabled = false;
        //}
    }
}
