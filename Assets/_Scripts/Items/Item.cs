using UnityEngine;

//[System.Serializable]
public class Item
{
    ItemData itemData;
    public ItemData ItemData => itemData;

    public Item (ItemData itemData)
    {
        this.itemData = itemData;
    }
}
