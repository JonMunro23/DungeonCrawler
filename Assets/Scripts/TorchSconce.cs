using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorchSconce : MonoBehaviour
{
    [SerializeField]
    ItemPickup itemPickup;
    [SerializeField]
    GameObject torchObject;
    [SerializeField]
    ItemObject torchItemObject;

    public bool hasTorch;

    public void Interact()
    {
        if(hasTorch == true)
        {
            hasTorch = false;
            TakeTorch();
        }
        else if(hasTorch == false)
        {
            PlaceTorch();
        }
    }

    public void PlaceTorch()
    {
        if(itemPickup.hasMouseItem == true)
        {
            if(itemPickup.objectOnMouse.itemType == ItemObject.ItemType.torch)
            {
                torchObject.SetActive(true);
                itemPickup.hasMouseItem = false;
                itemPickup.objectOnMouse = null;
                Destroy(itemPickup.mouseItemClone);
                hasTorch = true;
            }
        }
    }

    public void TakeTorch()
    {
        torchObject.SetActive(false);
        itemPickup.TorchSconceToMouse(torchItemObject);
    }
}
