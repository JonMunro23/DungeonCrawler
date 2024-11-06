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
        UseEquipment.onHandUsed += HandUsed;
        InventorySlot.onNewHandItem += NewHandItem;
        InventorySlot.onHandItemRemoved += HandItemRemoved;
    }

    private void OnDisable()
    {
        UseEquipment.onHandUsed -= HandUsed;
        InventorySlot.onNewHandItem -= NewHandItem;
        InventorySlot.onHandItemRemoved -= HandItemRemoved;
    }

    public void InitHands()
    {
        NewHandItem(Hands.left, defaultHandItem);
        NewHandItem(Hands.right, defaultHandItem);
    }

    public void HandItemRemoved(Hands hand)
    {
        if (hand == Hands.both)
        {
            leftHandHeldItemImage.sprite = defaultHandItem.itemSprite;
            rightHandHeldItemImage.sprite = defaultHandItem.itemSprite;

        }
        else if (hand == Hands.left)
            leftHandHeldItemImage.sprite = defaultHandItem.itemSprite;
        else
            rightHandHeldItemImage.sprite = defaultHandItem.itemSprite;
    }

    public void NewHandItem(Hands hand, HandItemData newItem)
    {
        if(hand == Hands.both)
        {
            leftHandHeldItemImage.sprite = newItem.itemSprite;
            rightHandHeldItemImage.sprite = newItem.itemSprite;

        }
        else if (hand == Hands.left)
            leftHandHeldItemImage.sprite = newItem.itemSprite;
        else
            rightHandHeldItemImage.sprite = newItem.itemSprite;
    }

    public void HandUsed(Hands hand, HandItemData itemUsed)
    {
        if(hand == Hands.both)
        {
            StartCoroutine(ItemUICooldown(Hands.left, itemUsed));
            StartCoroutine(ItemUICooldown(Hands.right, itemUsed));
        }
        else
            StartCoroutine(ItemUICooldown(hand, itemUsed));
    }

    IEnumerator ItemUICooldown(Hands hand, EquipmentItemData itemUsed)
    {
        if(hand == Hands.left)
        {
            leftHandItemCooldownImage.enabled = true;
            yield return new WaitForSeconds(itemUsed.itemCooldown);
            leftHandItemCooldownImage.enabled = false;
        }
        else
        {
            rightHandItemCooldownImage.enabled = true;
            yield return new WaitForSeconds(itemUsed.itemCooldown);
            rightHandItemCooldownImage.enabled = false;
        }
    }
}
