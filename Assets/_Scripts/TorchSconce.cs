using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorchSconce : MonoBehaviour
{
    [SerializeField]
    ItemPickupManager itemPickup;
    [SerializeField]
    GameObject torchObject;
    [SerializeField]
    ItemData torchItemObject;

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
        //if(itemPickup.hasGrabbedItem == true)
        //{
        //    if(itemPickup.objectOnMouse.itemType == ItemData.ItemType.torch)
        //    {
        //        torchObject.SetActive(true);
        //        itemPickup.hasGrabbedItem = false;
        //        itemPickup.objectOnMouse = null;
        //        Destroy(itemPickup.mouseItemClone);
        //        hasTorch = true;
        //    }
        //}
    }

    public void TakeTorch()
    {
        //torchObject.SetActive(false);
        //itemPickup.TorchSconceToMouse(torchItemObject);
    }
}
