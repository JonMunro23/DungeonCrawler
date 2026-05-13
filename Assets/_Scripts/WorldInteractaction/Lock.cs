using DG.Tweening;
using System.Collections;
using System.Security.Policy;
using UnityEngine;

public class Lock : InteractableBase
{
    [SerializeField] KeyType requiredKey;

    [Header("Key Animation")]
    [SerializeField] Transform key;

    public override bool TryInteractWithItem(ItemData item)
    {
        if (!canUse)
            return false;

        if(item == null)
            return false;

        KeyItemData keyData = item as KeyItemData;
        if (!keyData)
            return false;

        return TryUseKey(keyData);
    }

    public override bool ConsumesItem()
    {
        return true;
    }

    public void SetRequiredKeyType(string requiredType)
    {
        requiredKey = HelperFunctions.ToEnum<KeyType>(requiredType);
    }

    bool TryUseKey(KeyItemData keyData)
    {
        if (keyData.keyType == requiredKey)
        {
            TriggerObjects();
            return true;
        }

        //show key not going in lock?
        return false;
    }

    

    public override void Interact()
    {
        //somehow show player that a keycard is required
    }

    public override void SetStartingActivationState(bool _isActivated)
    {
        isActivated = _isActivated;

        if(isActivated)
        {
            if (isSingleUse)
                canUse = false;

            //show key in lock
        }
    }
}
